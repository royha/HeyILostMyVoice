/**************************** Pronunciation.cs *****************************\
Module Name:  Pronunciation.cs
Project:      Hey, I Lost My Voice!
Description:  Properties and methods used to convert written text to an
              SSML pronounced string, ready to be spoken.

MIT licence.
\***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace HeyILostMyVoice
{
    /// <summary>
    /// Windows form and functionality for the **Hey, I Lost My Voice!** program.
    /// </summary>
    public partial class HeyILostMyVoiceForm : Form
    {
        #region Fields

        /// <summary>
        /// The written string to be pronounced.
        /// </summary>
        private string writtenString;

        /// <summary>
        /// The string to speak with all pronunciation changes included.
        /// </summary>
        private string pronouncedString;

        /// <summary>
        /// The lookup table to link a position in the writtenText to its corresponding location
        /// in the pronouncedString.
        /// </summary>
        private List<PositionsAndLengths> writtenToSpoken;

        /// <summary>
        /// The current location being spoken in the <c>writtenToSpoken</c> table.
        /// </summary>
        private int writtenToSpokenIndex;

        /// <summary>
        /// The offset in writtenString for where speech was restarted due to skip ahead or skip 
        /// back, or a change in speech rate, volume, or voice.
        /// </summary>
        private int speechWrittenRestartOffset;

        /// <summary>
        /// The offset in pronouncedString for where speech was restarted due to skip ahead or 
        /// skip back, or a change in speech rate, volume, or voice.
        /// </summary>
        private int speechSpokenRestartOffset;

        /// <summary>
        /// Indicates the location in writtenString for the word that is currently being spoken.
        /// </summary>
        private int speechLocationInWritten;

        #endregion

        #region methods

        /// <summary>
        /// Returns the position in the written text that corresponds to the given position in
        /// the <c>pronouncedString</c>.
        /// </summary>
        /// <param name="speakingPosition">The position in the <c>pronouncedString</c> to find
        /// in the written string.</param>
        /// <returns>The corresponding location in the written string.</returns>
        /// <remarks>This method is primarly used to highlight the words in the text box during 
        /// speech.</remarks>
        private int GetWrittenPositionFromSpoken(int speakingPosition)
        {
            // Remove the SSML header string from the calculations and add the restart offset.
            speakingPosition = speakingPosition - VoiceTalker.ssmlOpeningLength + speechSpokenRestartOffset;

            // Bump the index if necessary.
            while (writtenToSpokenIndex + 1 < writtenToSpoken.Count &&
                speakingPosition > writtenToSpoken[writtenToSpokenIndex].SpokenPosition + writtenToSpoken[writtenToSpokenIndex].SpokenLength - 1)
            {
                ++writtenToSpokenIndex;
            }

            // Return the written position which is the same distance from .WrittenPosition as speakingPosition is from .SpokenPosition.
            return writtenToSpoken[writtenToSpokenIndex].WrittenPosition + speakingPosition - writtenToSpoken[writtenToSpokenIndex].SpokenPosition;
        }


        /// <summary>
        /// Creates an SSML string with altered spelling and phoneme pronunciations based on
        /// the given <paramref name="written"/> string. Also creates the <c>writtenToSpoken</c>
        /// table to connect the written words with their corresponding location in the 
        /// pronounced string, to be used when highlighting words in the text box as they are
        /// spoken.
        /// </summary>
        /// <param name="written">The string of words to be pronounced and added to the 
        /// pronounced return string.</param>
        /// <returns>The string of pronounced words, ready to be spoken using SSML.</returns>
        private string CreatePronunciationString(string written)
        {
            // Set the global.
            writtenString = written;

            // Start with both strings being identical.
            String spoken = written;

            // Clear out the mapping between written and spoken.
            writtenToSpoken = new List<PositionsAndLengths>();

            // Create a first entry.
            writtenToSpoken.Add(new PositionsAndLengths(0, spoken.Length, 0, written.Length, PronunciationTypeEnum.None));

            foreach (XmlNode node in pronunciationsNode.ChildNodes)
            {
                // Set values for case sensitive and whole word pronunciation changes.
                bool caseSensitive = (String.Compare(node.Attributes["CaseSensitive"].InnerText, "true", true) == 0) ? true : false;
                bool wholeWord = (String.Compare(node.Attributes["WholeWord"].InnerText, "true", true) == 0) ? true : false;

                // Spelling replacement.
                if (String.Compare(node.Attributes["Type"].InnerText, "Spelling", true) == 0)
                {
                    PronounceBySpelling(
                        written,
                        ref spoken,
                        node.Attributes["WrittenText"].InnerText,
                        node.Attributes["PronouncedText"].InnerText,
                        caseSensitive,
                        wholeWord);
                }
                else
                {
                    PronounceByPhoneme(written,
                        ref spoken,
                        node.Attributes["WrittenText"].InnerText,
                        node.Attributes["PronouncedText"].InnerText,
                        node.Attributes["Type"].InnerText,
                        caseSensitive,
                        wholeWord);
                }
            }

            // Sort writtenToSpoken before returning the string.
            writtenToSpoken.Sort(PositionsAndLengths.ComparePac);
            PositionsAndLengths.IsSorted = true;
            writtenToSpokenIndex = 0;

            // Remove characters that cause SSML to fail.
            CleanUpSsml(ref spoken);

#if DEBUG
            StringBuilder sb = new StringBuilder();
            foreach (PositionsAndLengths pal in writtenToSpoken)
            {
                string s1 = pal.SpokenPosition + ", " + pal.SpokenLength + ", " + pal.WrittenPosition + ", " + pal.WrittenLength + ", " + pal.PronunciationType.ToString() + "\n";
                sb.Append(s1);
            }
            string s2 = sb.ToString();
#endif
            return spoken;
        }


        /// <summary>
        /// The phoneme closing tag as a readonly string.
        /// </summary>
        private static readonly string phonemeClosing = "</phoneme>";


        /// <summary>
        /// Creates SSML phonemes in the spoken string for every occurance of the specified word in the 
        /// written string. Updates the <c>writtenToSpoken</c> table to connect the written text with
        /// its corresponding location in spoken text.
        /// </summary>
        /// <param name="written">The full written string ready to be pronounced.</param>
        /// <param name="spoken">The string that will be spoken which will contain the phoneme pronounced words.</param>
        /// <param name="writtenText">The word to be pronounced.</param>
        /// <param name="pronouncedText">The SSML phoneme pronunciation for the <paramref name="writtenText"/> word.</param>.
        /// <param name="alphabet">The phonetic alphabet to use for the pronunciation of the word in <paramref name="writtenText"/>.</param>
        /// <param name="caseSensitive">Use <c>true</c> to perform a case-sensitive search; <c>false</c> to perform a case-insensitive search.</param>
        /// <param name="wholeWord">Use <c>true</c> to match on whole words only; <c>false</c> to match text anywhere in the <paramref name="written"/> string.</param>
        private void PronounceByPhoneme(string written, ref string spoken, string writtenText, string pronouncedText, string alphabet, bool caseSensitive, bool wholeWord)
        {
            int writtenStart = 0;
            int writtenEnd;
            int spokenStart;
            int lpacIndex;

            // Set up the regular expression pattern and options.
            string regexPattern = (wholeWord) ? "\\b" + writtenText + "\\b" : writtenText;
            RegexOptions regexOptions = (caseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase;

            // Initialize the regular expression search.
            Match match = Regex.Match(written, regexPattern, regexOptions);

            // Find and change each occurance of this entry.
            while (match.Success)
            {
                // We have the location in written. Now find the corresponding position in spoken, and get the index of the 
                // appropriate entry in writtenToSpoken.
                writtenStart = match.Index;
                spokenStart = GetSpokenPositionFromWritten(writtenStart, out lpacIndex);
                writtenEnd = writtenStart + writtenText.Length;

                // Avoid overlapped entries (for example, written = "sometimes" words = { "some", "times", "metime" }, the "metime" won't fit in the "some" entry).
                if (writtenEnd > writtenToSpoken[lpacIndex].WrittenPosition + writtenToSpoken[lpacIndex].WrittenLength)
                    return;

                // Create the phoneme string opening.
                string phonemeOpening = "<phoneme alphabet=\"" + alphabet + "\" ph=\"" + pronouncedText + "\">";

                // Create the full phoneme tag.
                string phonemeText = String.Concat(phonemeOpening, writtenText, phonemeClosing);

                // Change the string at this spokenPosition.
                spoken = String.Concat(spoken.Substring(0, spokenStart), phonemeText, spoken.Substring(spokenStart + writtenText.Length));

                // Determine the change due to the spoken string length.
                int lengthDelta = phonemeText.Length - writtenText.Length;

                // If the pronounced text is a different length than the written text, adjust the SpokenPosition on all entries after this one.
                if (lengthDelta != 0)
                {
                    for (int i = lpacIndex + 1; i < writtenToSpoken.Count; ++i)
                    {
                        writtenToSpoken[i].SpokenPosition += lengthDelta;
                    }
                }

                // Create five lookup entries for this change. The text before the change, the phoneme open, the change, the phoneme close and the text after the change.

                // The before entry.
                AddEntry(
                    SpokenPosition: writtenToSpoken[lpacIndex].SpokenPosition,
                    SpokenLength: spokenStart - writtenToSpoken[lpacIndex].SpokenPosition,
                    WrittenPosition: writtenToSpoken[lpacIndex].WrittenPosition,
                    WrittenLength: writtenStart - writtenToSpoken[lpacIndex].WrittenPosition,
                    PronunciationType: PronunciationTypeEnum.None);

                // The phoneme tag opening.
                AddEntry(
                    SpokenPosition: spokenStart,
                    SpokenLength: phonemeOpening.Length,
                    WrittenPosition: writtenStart,
                    WrittenLength: 0,
                    PronunciationType: PronunciationTypeEnum.PhonemeOverhead);

                // The change entry.
                AddEntry(
                    SpokenPosition: spokenStart + phonemeOpening.Length,
                    SpokenLength: writtenText.Length,
                    WrittenPosition: writtenStart,
                    WrittenLength: writtenText.Length,
                    PronunciationType: PronunciationTypeEnum.Phoneme);

                // The phoneme tag closing.
                AddEntry(
                    SpokenPosition: spokenStart + phonemeOpening.Length + writtenText.Length,
                    SpokenLength: phonemeClosing.Length,
                    WrittenPosition: writtenStart + writtenText.Length,
                    WrittenLength: 0,
                    PronunciationType: PronunciationTypeEnum.PhonemeOverhead);

                // The after entry.
                AddEntry(
                    SpokenPosition: spokenStart + phonemeText.Length,
                    SpokenLength: writtenToSpoken[lpacIndex].SpokenLength - (spokenStart - writtenToSpoken[lpacIndex].SpokenPosition + phonemeText.Length - lengthDelta),
                    WrittenPosition: writtenEnd,
                    WrittenLength: writtenToSpoken[lpacIndex].WrittenLength - (writtenStart - writtenToSpoken[lpacIndex].WrittenPosition + writtenText.Length),
                    PronunciationType: PronunciationTypeEnum.None);

                // Remove the original entry from the lookup table
                writtenToSpoken.RemoveAt(lpacIndex);

                // Sort the list so it is ready for use.
                writtenToSpoken.Sort(PositionsAndLengths.ComparePac);
                PositionsAndLengths.IsSorted = true;

                // Continue with search.
                match = match.NextMatch();
            }
        }


        /// <summary>
        /// Uses altered spelling in the spoken string for every occurance of the specified word in the 
        /// written string. Updates the <c>writtenToSpoken</c> table to connect the written text with
        /// its corresponding location in spoken text.
        /// </summary>
        /// <param name="written">The full written string ready to be pronounced.</param>
        /// <param name="spoken">The string that will be spoken which will contain the phoneme pronounced words.</param>
        /// <param name="writtenText">The word to be pronounced.</param>
        /// <param name="pronouncedText">The altered spelling for the <paramref name="writtenText"/> word.</param>.
        /// <param name="caseSensitive">Use <c>true</c> to perform a case-sensitive search; <c>false</c> to perform a case-insensitive search.</param>
        /// <param name="wholeWord">Use <c>true</c> to match on whole words only; <c>false</c> to match text anywhere in the <paramref name="written"/> string.</param>
        private void PronounceBySpelling(string written, ref string spoken, string writtenText, string pronouncedText, bool caseSensitive, bool wholeWord)
        {
            int writtenStart = 0;
            int writtenEnd;
            int spokenStart;
            int lpacIndex;

            // Set up the regular expression pattern and options.
            string regexPattern = (wholeWord) ? "\\b" + writtenText + "\\b" : writtenText;
            RegexOptions regexOptions = (caseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase;

            // Initialize the regular expression search.
            Match match = Regex.Match(written, regexPattern, regexOptions);

            // Find and change each occurance of this entry.
            while (match.Success)
            {
                // We have the location in written. Now find the corresponding position in spoken, and get the index of the 
                // appropriate entry in writtenToSpoken.
                writtenStart = match.Index;
                spokenStart = GetSpokenPositionFromWritten(writtenStart, out lpacIndex);
                writtenEnd = writtenStart + writtenText.Length;

                // Avoid overlapped entries (for example, written = "sometimes" words = { "some", "times", "metime" }, the "metime" won't fit in the "some" entry).
                if (writtenEnd > writtenToSpoken[lpacIndex].WrittenPosition + writtenToSpoken[lpacIndex].WrittenLength)
                    return;

                // Change the string at this spokenPosition.
                spoken = String.Concat(spoken.Substring(0, spokenStart), pronouncedText, spoken.Substring(spokenStart + writtenText.Length));

                // Determine the change due to the spoken string length.
                int lengthDelta = pronouncedText.Length - writtenText.Length;

                // If the pronounced text is a different length than the written text, adjust the SpokenPosition on all entries after this one.
                if (lengthDelta != 0)
                {
                    for (int i = lpacIndex + 1; i < writtenToSpoken.Count; ++i)
                    {
                        writtenToSpoken[i].SpokenPosition += lengthDelta;
                    }
                }

                // Create three lookup entries for this change. The text before the change, the change, and the text after the change.

                // The before entry.
                AddEntry(
                    SpokenPosition: writtenToSpoken[lpacIndex].SpokenPosition,
                    SpokenLength: spokenStart - writtenToSpoken[lpacIndex].SpokenPosition,
                    WrittenPosition: writtenToSpoken[lpacIndex].WrittenPosition,
                    WrittenLength: writtenStart - writtenToSpoken[lpacIndex].WrittenPosition,
                    PronunciationType: PronunciationTypeEnum.None);

                // The change entry.
                AddEntry(
                    SpokenPosition: spokenStart,
                    SpokenLength: pronouncedText.Length,
                    WrittenPosition: writtenStart,
                    WrittenLength: writtenText.Length,
                    PronunciationType: PronunciationTypeEnum.Spelling);

                // The after entry.
                AddEntry(
                    SpokenPosition: spokenStart + pronouncedText.Length,
                    SpokenLength: writtenToSpoken[lpacIndex].SpokenLength - (spokenStart - writtenToSpoken[lpacIndex].SpokenPosition + pronouncedText.Length - lengthDelta),
                    WrittenPosition: writtenEnd,
                    WrittenLength: writtenToSpoken[lpacIndex].WrittenLength - (writtenStart - writtenToSpoken[lpacIndex].WrittenPosition + writtenText.Length),
                    PronunciationType: PronunciationTypeEnum.None);

                // Remove the original entry from the lookup table
                writtenToSpoken.RemoveAt(lpacIndex);

                // Sort the list so it is ready for use.
                writtenToSpoken.Sort(PositionsAndLengths.ComparePac);
                PositionsAndLengths.IsSorted = true;

                // Continue with search.
                match = match.NextMatch();
            }
        }


        /// <summary>
        /// Creates an entry in the PositionsAndLengths writtenToSpoken array.
        /// </summary>
        /// <param name="SpokenPosition">The position in the spoken string.</param>
        /// <param name="SpokenLength">The length of the spoken word.</param>
        /// <param name="WrittenPosition">The position in the written string.</param>
        /// <param name="WrittenLength">The length of the written word.</param>
        /// <param name="PronunciationType">The pronunciation type.</param>
        private void AddEntry(int SpokenPosition, int SpokenLength, int WrittenPosition, int WrittenLength, PronunciationTypeEnum PronunciationType)
        {
            // Avoid adding empty entries.
            if (SpokenLength > 0 || WrittenLength > 0)
            {
                writtenToSpoken.Add(new PositionsAndLengths(SpokenPosition, SpokenLength, WrittenPosition, WrittenLength, PronunciationType));
            }
        }


        /// <summary>
        /// Returns the position in the spoken text that corresponds to the given position in
        /// the written string.
        /// </summary>
        /// <param name="writtenStart">The position in the written string.</param>
        /// <param name="lpacIndex">The index into the <c>writtenToSpoken</c> lookup table.</param>
        /// <returns>The corresponding location in the spoken string.</returns>
        /// <remarks>This method is primarily used to add entries to the <c>writtenToSpoken</c>
        /// lookup table.</remarks>
        private int GetSpokenPositionFromWritten(int writtenStart, out int lpacIndex)
        {
            // If writtenToSpoken is null, empty, or it has a single entry, there have been no pronunciation changes. Return writtenStart.
            if (writtenToSpoken == null || writtenToSpoken.Count <= 1)
            {
                lpacIndex = 0;
                return writtenStart;
            }

            // Search for the entry with the range that covers this writtenStart location in the written string.
            int i;
            for (i = 0; i < writtenToSpoken.Count &&
                !(writtenStart >= writtenToSpoken[i].WrittenPosition && writtenStart < writtenToSpoken[i].WrittenPosition + writtenToSpoken[i].WrittenLength);
                ++i)
                ;

            // Return the index to the entry that will be directly above the new entry that's about to be created.
            lpacIndex = i;

            // Return the character position in spoken that corresponds to the given written character position.
            return writtenStart - writtenToSpoken[lpacIndex].WrittenPosition + writtenToSpoken[lpacIndex].SpokenPosition;
        }


        /// <summary>
        /// Cleans up the SSML before speech.
        /// </summary>
        /// <param name="spoken">The spoken string to clean up.</param>
        /// <remarks>Because extraneous <, >, and & characters break SSML speech, this method performs a simple, 
        /// single level parse of the SSML to replace those characters with spaces. Only &lt;phoneme ...&gt; and 
        /// &lt;/phoneme&gt; tags are respected in this string.</remarks>
        private void CleanUpSsml(ref string spoken)
        {
            string[] tags = { "phoneme", "/phoneme" /* , "break", "/break", "prosody", "/prosody", "emphasis", "/emphasis", "audio", "/audio", "mark", "/mark", "voice", "/voice", "s", "/s", "p", "/p", "meta", "/meta", "metadata", "/metadata", "say-as", "/say-as", "sub", "/sub", "!--", */ };
            int mainIndex = 0;

            while (mainIndex != -1)
            {
                // Search for opening angle bracket.
                int index = spoken.IndexOf('<', mainIndex);

                // If an opening angle bracket was found
                if (index > -1)
                {
                    // Replace any closing angle brackets with a space between mainIndex and index.
                    ReplaceCharRange(ref spoken, '>', ' ', mainIndex, index);

                    // Check to see if the opening angle bracket is the beginning of a recognized SSML tag.
                    bool tagFound = false;
                    foreach (string tag in tags)
                    {
                        if (index + 1 < spoken.Length && String.Compare(spoken, index + 1, tag, 0, tag.Length, true) == 0)
                        {
                            tagFound = true;
                            break;
                        }
                    }
                    if (!tagFound)
                    {
                        // It doesn't appear to be a tag. Consider this a stray opening angle bracket and replace it with a space.
                        ReplaceCharAt(ref spoken, ' ', index);

                        // Replace any ampersands with space between mainIndex and index.
                        ReplaceCharRange(ref spoken, '&', ' ', mainIndex, index);

                        // Continue with the search.
                        mainIndex = index;
                        continue;
                    }

                    // Replace any ampersands with space between mainIndex and index.
                    ReplaceCharRange(ref spoken, '&', ' ', mainIndex, index);

                    // We found the opening of a tag. Search for the closing angle bracket of this tag.
                    mainIndex = index;
                    index = spoken.IndexOf('>', mainIndex);

                    // If the closing angle bracket was found ...
                    if (index > 0)
                    {
                        // Replace any stray opening angle brackets between mainIndex and index.
                        if (index + 1 < spoken.Length)
                        {
                            ReplaceCharRange(ref spoken, '<', ' ', mainIndex + 1, index);

                            // Move index past this closing angle bracket.
                            ++index;
                        }
                        else
                        {
                            // We're at the end of the string.
                            index = -1;
                        }
                    }
                    else
                    {
                        // The closing angle bracket for a tag was not found.
                        // Replace the initial opening angle bracket with a space.
                        ReplaceCharAt(ref spoken, ' ', mainIndex);

                        // Replace opening angle brackets and ampersands from
                        // mainIndex to end of spoken string.
                        ReplaceCharRange(ref spoken, '<', ' ', mainIndex, spoken.Length);
                        ReplaceCharRange(ref spoken, '&', ' ', mainIndex, spoken.Length);
                    }
                }
                else
                {
                    // Opening angle bracket for a tag was not found.
                    // Replace closing angle brackets and ampersands from 
                    // mainIndex to end of spoken string.
                    ReplaceCharRange(ref spoken, '>', ' ', mainIndex, spoken.Length);
                    ReplaceCharRange(ref spoken, '&', ' ', mainIndex, spoken.Length);
                }

                mainIndex = index;
            }
        }


        /// <summary>
        /// Replaces the character at location <paramref name="index"/> in the string <paramref name="s"/>
        /// with the character <paramref name="c"/>.
        /// </summary>
        /// <param name="s">The string to change.</param>
        /// <param name="c">The replacement character.</param>
        /// <param name="index">The index into the string <paramref name="s"/>.</param>
        private void ReplaceCharAt(ref string s, char c, int index)
        {
            // Replace the character with a space and continue searching.
            if (index == 0)
            {
                // It's the first character in the spoken string.
                s = String.Concat(c, s.Substring(index + 1));
            }
            else if (index + 1 == s.Length)
            {
                // It's the last character in the spoken string.
                s = String.Concat(s.Substring(0, index), c);
            }
            else
            {
                // It's not the first or last character in the spoken string.
                s = String.Concat(s.Substring(0, index), c, s.Substring(index + 1));
            }
        }


        /// <summary>
        /// Replaces all occurances of the character <paramref name="findChar"/> with <paramref name="replaceChar"/>
        /// in the string <paramref name="s"/> between <paramref name="startIndex"/> and <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="s">The string to change.</param>
        /// <param name="findChar">The character to replace.</param>
        /// <param name="replaceChar">The replacement character.</param>
        /// <param name="startIndex">The starting location for replacement in the string<paramref name="s"/>.</param>
        /// <param name="endIndex">The ending location for replacement in the string<paramref name="s"/>.</param>
        private void ReplaceCharRange(ref string s, char findChar, char replaceChar, int startIndex, int endIndex)
        {
            int i = startIndex;
            while ((i = s.IndexOf(findChar, i, endIndex - i)) != -1)
            {
                ReplaceCharAt(ref s, replaceChar, i);
            }
        }

        #endregion
    }
}