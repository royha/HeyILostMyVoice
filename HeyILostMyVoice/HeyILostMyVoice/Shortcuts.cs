/****************************** Shortcuts.cs *******************************\
Module Name:  Shortcuts.cs
Project:      Hey, I Lost My Voice!
Description:  Methods used for the shortcut feature of Hey, I Lost My Voice!

MIT licence.
\***************************************************************************/

using System;
using System.Speech.Synthesis;
using System.Windows.Forms;
using System.Xml;

namespace HeyILostMyVoice
{
    public partial class HeyILostMyVoiceForm : Form
    {
        /// <summary>
        /// Expands a shortcut into its replacement text and speaks that replacement text by using the word voice.
        /// </summary>
        /// <returns>Returns <c>true</c> if a shortcut was found and expanded; otherwise, <c>false</c>.</returns>
        private Boolean ExpandShortcut()
        {
            // If there are no shortcuts in the list, we're done here.
            if (shortcutsNode == null)
                return false;

            try
            {
                // Search backwards for the beginning of the word
                int i = richTextBox1.SelectionStart;
                while (--i > 0 && richTextBox1.Text[i] != ' ' && richTextBox1.Text[i] != '\n')
                    ;
                i = (i < 0) ? 0 : i;

                // Find the word in the list of shortcuts

                // Select the potential shortcut word from the text box.
                // TODO: I shouldn't have to use Trim on this.
                String shortcutCandidate = richTextBox1.Text.Substring(i, richTextBox1.SelectionStart - i).Trim();

                // Return if this string contains a double-quote.
                if (shortcutCandidate.Contains("\""))
                    return false;

                // Create the XPath query string
                String xpathQueryString = "Shortcut[@ShortcutText=\"" + shortcutCandidate + "\"]";

                // Find the shortcut if it's there
                XmlNode shortcutNode = shortcutsNode.SelectSingleNode(xpathQueryString);

                // If the shortcut isn't there, we're done.
                if (shortcutNode == null)
                    return false;

                // Find the replacement text for this shortcut
                String replacementText = shortcutNode.Attributes["ReplacementText"].Value;

                // Replace the shortcut with its replacement text
                richTextBox1.SelectionStart -= shortcutCandidate.Length;
                richTextBox1.SelectionLength = shortcutCandidate.Length;
                richTextBox1.SelectedText = replacementText;

                // Speak the newly added text
                // TODO: Does not check for pronunciation words in the replacement text.
                if (checkBoxSpeakOnWord.Checked && voiceTalker.ParaSpeechState == SynthesizerState.Ready)
                    voiceTalker.WordSpeakAsync(replacementText);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in ExpandShortcut: " + ex.ToString() + "\nInnerException: " + ex.InnerException + "\nStack Trace:\n" + ex.StackTrace);
            }

            // Shortcut was expanded. Return true.
            return true;
        }
    }
}