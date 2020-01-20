/******************************** Speech.cs ********************************\
Module Name:  Speech.cs
Project:      Hey, I Lost My Voice!
Description:  Methods used to start, stop, pause, and resume speech.

MIT licence.
\***************************************************************************/

using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace HeyILostMyVoice
{
    /// <summary>
    /// Windows form and functionality for the **Hey, I Lost My Voice!** program.
    /// </summary>
    public partial class HeyILostMyVoiceForm : Form
    {
        #region Fields

        /// <summary>
        /// When restarting speech due to skip ahead or voice change, StopSpeech() is called. By adding an entry to this list,
        /// the default behavior of StopSpeech() is prevented, allowing speech to restart properly, then end properly. A list
        /// is used instead of a single flag as sometimes events get stacked up.
        /// </summary>
        private List<bool> speechRestartOverrides = new List<bool>();

        #endregion

        #region Methods

        /// <summary>
        /// Pauses or resumes speech output.
        /// </summary>
        private void PauseOrResume()
        {
            // If the para voice is speaking, pause speech.
            if (voiceTalker.ParaSpeechState == SynthesizerState.Speaking)
            {
                // Pause the speech.
                voiceTalker.ParaSpeakPause();

                // Set the button image to the Play bitmap.
                buttonPlayPause.BackgroundImage = HeyILostMyVoice.Properties.Resources.media_play_2x;

                // Set the tooltip to Resume.
                toolTip1.SetToolTip(buttonPlayPause, "Resume");
            }
            // If the para voice is paused, resume speech.
            else if (voiceTalker.ParaSpeechState == SynthesizerState.Paused)
            {
                // Resume speech.
                voiceTalker.ParaSpeakResume();

                // Set the button image to the Pause bitmap.
                buttonPlayPause.BackgroundImage = HeyILostMyVoice.Properties.Resources.media_pause_2x;

                // Set the tooltip to Resume.
                toolTip1.SetToolTip(buttonPlayPause, "Pause");
            }
        }


        /// <summary>
        /// Speaks the word behind the most recent space entered.
        /// </summary>
        private void SpeakWord()
        {
            try
            {
                // If speak on word is not checked, or the previous paragraph is still being spoken, return.
                if (!checkBoxSpeakOnWord.Checked || voiceTalker.ParaSpeechState != SynthesizerState.Ready)
                    return;

                // Search backwards for the beginning of the word
                int i = richTextBox1.SelectionStart;
                while (--i > 0 && richTextBox1.Text[i] != ' ' && richTextBox1.Text[i] != '\n')
                    ;
                i = (i < 0) ? 0 : i;

                // Speak the word
                String wordToSpeak = richTextBox1.Text.Substring(i, richTextBox1.SelectionStart - i);

                // Speak the word if we found one.
                if (wordToSpeak != String.Empty)
                {
                    // Create pronunciation string.
                    pronouncedString = CreatePronunciationString(wordToSpeak);

                    // Speak the word.
                    voiceTalker.WordSpeakAsync(pronouncedString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in speak word: " + ex.ToString() + "\nInnerException: " + ex.InnerException + "\nStack Trace:\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Speaks the paragraph just entered into the rich text box.
        /// </summary>
        private void SpeakPara()
        {
            // If we are not speaking on Enter, we're done.
            if (!checkBoxSpeakOnPara.Checked)
                return;

            // Set the offset so that the text being spoken can be hilighted
            speakSelectionStart = richTextBox1.SelectionStart;

            // Speak the most recently entered paragraph
            try
            {
                // Search backwards for the beginning of the paragraph
                int i = richTextBox1.SelectionStart;
                while (--i > 0 && richTextBox1.Text[i] != '\n')
                    ;
                i = (i < 0) ? 0 : i;

                // Identify the start of the paragraph.
                String paraToSpeak = richTextBox1.Text.Substring(i, richTextBox1.SelectionStart - i);

                // If there's something to speak, speak it.
                if (paraToSpeak != String.Empty)
                {
                    // Cancel all speach first.
                    voiceTalker.CancelAllSpeech();

                    // Create pronunciation string.
                    pronouncedString = CreatePronunciationString(paraToSpeak);

                    // Speak the paragraph.
                    voiceTalker.ParaSpeakAsync(pronouncedString);

                    // Grey out the play/pause and skip buttons.
                    buttonSkipBack.Enabled = false;
                    buttonPlayPause.Enabled = false;
                    buttonSkipAhead.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in speak paragraph: " + ex.ToString() + "\nInnerException: " + ex.InnerException + "\nStack Trace:\n" + ex.StackTrace);
            }

            // Set the play/pause button to indicate it's ready to pause.
            //buttonPlayPause.BackgroundImage = HeyILostMyVoice.Properties.Resources.media_pause_2x;
            //toolTip1.SetToolTip(buttonPlayPause, "Pause");
        }

        /// <summary>
        /// Speaks the selected text in the text box.
        /// </summary>
        private void SpeakSelection()
        {
            // Stop all current speech.
            voiceTalker.CancelAllSpeech();

            // Indicate we are highlighting spoken words to prevent edits during speech.
            highlightSpokenWord = true;

            // Store the existing cursor location.
            cursorLocation = richTextBox1.SelectionStart;

            // Set the start and length of the current selection, to indicate we are speaking the selection.
            speakSelectionStart = richTextBox1.SelectionStart;
            speakSelectionLength = richTextBox1.SelectionLength;

            // Create pronunciation string.
            pronouncedString = CreatePronunciationString(richTextBox1.SelectedText);

            // Speak the selected text.
            voiceTalker.ParaSpeakAsync(pronouncedString);

            // Set the play/pause button to pause.
            buttonPlayPause.BackgroundImage = HeyILostMyVoice.Properties.Resources.media_pause_2x;
            toolTip1.SetToolTip(buttonPlayPause, "Pause");
        }

        /// <summary>
        /// Speaks the contents of the text box.
        /// </summary>
        private void SpeakAll()
        {
            // Stop all current speech.
            voiceTalker.CancelAllSpeech();

            // Indicate we are highlighting spoken words to prevent edits during speech.
            highlightSpokenWord = true;

            // Store the existing cursor location.
            cursorLocation = richTextBox1.SelectionStart;

            // Set the start and length of the current selection to zero, to indicate we are speaking all.
            speakSelectionStart = 0;
            speakSelectionLength = 0;

            // Create pronunciation string.
            pronouncedString = CreatePronunciationString(richTextBox1.Text);

            // Speak the selected text.
            voiceTalker.ParaSpeakAsync(pronouncedString);

            // Set the play/pause button to pause.
            buttonPlayPause.BackgroundImage = HeyILostMyVoice.Properties.Resources.media_pause_2x;
            toolTip1.SetToolTip(buttonPlayPause, "Pause");
        }

        
        /// <summary>
        /// Restores state (cursor position, selected text, etc.) to the stopped state.
        /// </summary>
        private void StopSpeech()
        {
            // Restarting speech after a skip back, ahead, or a rate, volume, or voice change while speaking.
            if (speechRestartOverrides.Count > 0)
            {
                speechRestartOverrides.RemoveAt(0);
                return;
            }

            // Restore play/pause button to Play.
            buttonPlayPause.BackgroundImage = HeyILostMyVoice.Properties.Resources.media_play_2x;
            toolTip1.SetToolTip(buttonPlayPause, "Play");

            // Re-enable the play/pause and skip buttons.
            buttonSkipBack.Enabled = true;
            buttonPlayPause.Enabled = true;
            buttonSkipAhead.Enabled = true;


            // If highlighting words as they are spoken, restore cursor location and selection.
            if (highlightSpokenWord)
            {
                richTextBox1.SelectionStart = cursorLocation;
                richTextBox1.SelectionLength = speakSelectionLength;
            }
            
            // Set these fields to their pre-speech values.
            cursorLocation = 0;
            speakSelectionStart = 0;
            speakSelectionLength = 0;
            speechWrittenRestartOffset = 0;
            speechSpokenRestartOffset = 0;
            speechLocationInWritten = 0;
            highlightSpokenWord = false;
        }


        /// <summary>
        /// Restarts speech at the specified location in the written string.
        /// </summary>
        /// <param name="restartLocationInWritten">The restart location in the written string.</param>
        private void RestartSpeech(int restartLocationInWritten)
        {
            RestartSpeech(restartLocationInWritten, null);
        }


        /// <summary>
        /// Restarts speech at the specified location in the written string, after changing the voice to 
        /// <paramref name="newVoice"/> when <paramref name="newVoice"/> is not <c>null</c>.
        /// </summary>
        /// <param name="restartLocationInWritten">The restart location in the written string.</param>
        /// <param name="newVoice">The new voice to use, or <c>null</c> if there is no change to the voice.</param>
        /// <remarks>The <paramref name="newVoice"/> was added because a voice can only be changed after stopping
        /// speech.</remarks>
        private void RestartSpeech(int restartLocationInWritten, string newVoice)
        {
            if (pronouncedString == null || restartLocationInWritten >= writtenString.Length)
            {
                voiceTalker.CancelAllSpeech();
                StopSpeech();
                return;
            }

            if (highlightSpokenWord)
            {
                // Let StopSpeech() know this is a restart, not a full stop.
                speechRestartOverrides.Add(true);

                // Store the previous speech state.
                SynthesizerState previousSpeechState = voiceTalker.ParaSpeechState;

                // Stop all speech.
                voiceTalker.CancelAllSpeech();

                if (newVoice != null)
                {
                    voiceTalker.ParaSelectVoice(newVoice);
                }

                // If we're not at the beginning of a word, find the beginning of the word.
                if (!IsSpokenChar(writtenString[restartLocationInWritten]))
                {
                    restartLocationInWritten = SearchAheadToBeginningOfWord(writtenString, restartLocationInWritten);
                }

                // Set the written offset.
                speechWrittenRestartOffset = restartLocationInWritten;

                // Find the correct speech starting location for this new written starting location.
                speechSpokenRestartOffset = GetSpokenPositionFromWritten(restartLocationInWritten, out writtenToSpokenIndex);

                // Create a new string starting at the restarted location in pronouncedString.
                string newPronouncedString = pronouncedString.Substring(speechSpokenRestartOffset);

                // Restart speech from the new location.
                voiceTalker.ParaSpeakAsync(newPronouncedString);

                // Was the previous speech state was paused?
                if (previousSpeechState == SynthesizerState.Paused)
                {
                    // Pause it again.
                    voiceTalker.ParaSpeakPause();

                    // Highlight the current word.
                    richTextBox1.SelectionStart = restartLocationInWritten;
                    richTextBox1.SelectionLength = SearchAheadToBeyondEndOfWord(writtenString, restartLocationInWritten) - restartLocationInWritten;
                }

                // Set the new speech location.
                speechLocationInWritten = restartLocationInWritten;
            }
        }


        /// <summary>
        /// Skips back 12 seconds, and searches back from there for the beginning of a word, then 
        /// restarts speech at that location.
        /// Skips back 3 seconds if the shift key is down
        /// </summary>
        private void SkipBack(Boolean ShiftPressed)
        {
            // Store the current speech location.
            int intialWrittenLocation = speechLocationInWritten;
            int runningWrittenLocation = speechLocationInWritten;

            // Number of seconds to skip.
            double skipSeconds = ShiftPressed ? 3.0D : 12.0D;

            // This formula returns approximately the number of characters spoken per second
            // at any given speech rate. It does not, however, account for pauses in punctuation.
            double spokenCharsPerSec = Math.Pow(2, (voiceTalker.ParaRate + 11) * 0.15) * 4;
            int totalCountToSkipBack = Convert.ToInt32(Math.Round(spokenCharsPerSec * 8.0D, 0));

            // Search back the minimum number of words (in this case, two), counting the number of spoken characters.
            runningWrittenLocation = SearchBackToBeforeStartOfWord(writtenString, runningWrittenLocation);
            runningWrittenLocation = SearchBackToEndOfWord(writtenString, runningWrittenLocation);
            runningWrittenLocation = SearchBackToBeforeStartOfWord(writtenString, runningWrittenLocation);
            runningWrittenLocation = SearchBackToEndOfWord(writtenString, runningWrittenLocation);
            runningWrittenLocation = SearchBackToBeforeStartOfWord(writtenString, runningWrittenLocation);

            // Calculate the number of skipped spoken characters.
            int spokenCharCount = intialWrittenLocation - runningWrittenLocation;

            // Keep searching back until we have gone back at least as many characters as we need to.
            while (runningWrittenLocation > 0 && spokenCharCount < totalCountToSkipBack)
            {
                runningWrittenLocation = SearchBackToEndOfWord(writtenString, runningWrittenLocation);
                runningWrittenLocation = SearchBackToBeforeStartOfWord(writtenString, runningWrittenLocation);

                spokenCharCount = intialWrittenLocation - runningWrittenLocation;
            }

            // Move ahead to point directly at the next word to be spoken.
            runningWrittenLocation = SearchAheadToBeginningOfWord(writtenString, runningWrittenLocation);

            // Restart speech at the new location.
            RestartSpeech(runningWrittenLocation);
        }


        /// <summary>
        /// Searches backwards in the given string, starting at <paramref name="index"/> to find the
        /// start of a word. Assumes <paramref name="s"/>[<paramref name="index"/>] is already within
        /// or at the end of a word.
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <param name="index">The location within the string <paramref name="s"/> to begin the search.</param>
        /// <returns>The location before the start of a word.</returns>
        private int SearchBackToBeforeStartOfWord(string s, int index)
        {
            while (index > 0 && IsSpokenChar(s[index]))
                --index;

            return index;
        }


        /// <summary>
        /// Searches backwards in the given string, starting at <paramref name="index"/> to find the
        /// end of a word. Assumes <paramref name="s"/>[<paramref name="index"/>] is already within
        /// or at the end of a set of non-spoken characters.
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <param name="index">The location within the string <paramref name="s"/> to begin the search.</param>
        /// <returns>The location of the end of a word.</returns>
        private int SearchBackToEndOfWord(string s, int index)
        {
            while (index > 0 && !IsSpokenChar(s[index]))
                --index;

            return index;
        }


        /// <summary>
        /// Skips ahead 12 seconds, and searches  ahead from there for the beginning of a word, then restarts 
        /// speech at that location.
        /// Skips ahead 3 seconds if the shift key is down.
        /// </summary>
        private void SkipAhead(Boolean ShiftPressed)
        {
            // Store the current speech location.
            int intialLocation = speechLocationInWritten;
            int runningWrittenLocation = speechLocationInWritten;
            int spokenCharCount = 0;

            // Number of seconds to skip.
            double skipSeconds = ShiftPressed ? 3.0D : 12.0D;

            // This formula returns approximately the number of characters spoken per second
            // at any given speech rate. It does not, however, account for pauses in punctuation.
            double spokenCharsPerSec = Math.Pow(2, (voiceTalker.ParaRate + 11) * 0.15) * 4;
            int totalCountToSkipAhead = Convert.ToInt32(Math.Round(spokenCharsPerSec * skipSeconds, 0));

            // Skip ahead by one sentence at a time until we have gone beyond the 3 second timeframe.
            while (runningWrittenLocation < writtenString.Length && spokenCharCount < totalCountToSkipAhead)
            {
                runningWrittenLocation = SearchAheadToEndOfSentence(writtenString, runningWrittenLocation);
                runningWrittenLocation = SearchAheadToBeginningOfWord(writtenString, runningWrittenLocation);

                spokenCharCount = runningWrittenLocation - intialLocation;
            }

            // Restart speech at the new location.
            RestartSpeech(runningWrittenLocation);
        }


        /// <summary>
        /// Searches ahead in the given string, starting at <paramref name="index"/> to one character
        /// beyond the end of a word. Assumes <paramref name="s"/>[<paramref name="index"/>] is already 
        /// within or at the end of a word.
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <param name="index">The location within the string <paramref name="s"/> to begin the search.</param>
        /// <returns>The location of the end of a word.</returns>
        private int SearchAheadToBeyondEndOfWord(string s, int index)
        {
            while (index < s.Length && IsSpokenChar(s[index]))
                ++index;

            return index;
        }


        /// <summary>
        /// Searches ahead in the given string, starting at <paramref name="index"/> to the beginning
        /// of a word. Assumes <paramref name="s"/>[<paramref name="index"/>] is already within or at 
        /// a non-speaking character.
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <param name="index">The location within the string <paramref name="s"/> to begin the search.</param>
        /// <returns>The location of the beginning of a word.</returns>
        private int SearchAheadToBeginningOfWord(string s, int index)
        {
            while (index < s.Length && !IsSpokenChar(s[index]))
                ++index;

            return index;
        }


        /// <summary>
        /// Searches ahead in the given string, starting at <paramref name="index"/> to the end of
        /// the current sentence.
        /// </summary>
        /// <param name="s">The string to search.</param>
        /// <param name="index">The location within the string <paramref name="s"/> to begin the search.</param>
        /// <returns>The location of the end of a sentence.</returns>
        private int SearchAheadToEndOfSentence(string s, int index)
        {
            while (index < s.Length && s[index].ToString().IndexOfAny(new char[] { '.', '?', '!', ':' }) == -1)
                ++index;

            return index;
        }


        /// <summary>
        /// Identifies whether the given character is a spoken character or not. Returns <c>true</c> if <paramref name="c"/>
        /// is a spoken character; otherwise, <c>false</c>.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsSpokenChar(Char c)
        {
            if (Char.IsLetterOrDigit(c)) return true;
            if (c.ToString().IndexOf("$'-_") != -1) return true;
            return false;
        }

        #endregion
    }
}