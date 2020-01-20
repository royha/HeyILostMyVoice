/************************* HeyILostMyVoiceForm.cs **************************\
Module Name:  HeyILostMyVoiceForm.cs
Project:      Hey, I Lost My Voice!
Description:  Properties and methods used for the main Windows form, and
              overall funcationality of Hey, I Lost My Voice!

MIT licence.
\***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Speech;
using System.Speech.Synthesis;
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
        /// Holds the XML configuration data, including settings, shortcuts, and pronunciations.
        /// </summary>
        XmlDataDocument configurationData = new XmlDataDocument();

        /// <summary>
        /// Points to the Settings node of the configuration file.
        /// </summary>
        XmlNode settingsNode;

        /// <summary>
        /// Points to the Shortcuts node of the configuration file.
        /// </summary>
        XmlNode shortcutsNode;

        /// <summary>
        /// Points to the Pronunications node of the configuration file.
        /// </summary>
        XmlNode pronunciationsNode;

        /// <summary>
        /// The voice object that interfaces with the Windows/.NET SpeechAPI.
        /// </summary>
        VoiceTalker voiceTalker = new VoiceTalker();

        /// <summary>
        /// Indicates whether the paragraph speech synthesizer is speaking a paragraph, or speaking all or a selection.
        /// true indicates the speech synthesizer is speaking all or speaking a selection; false indicates that the 
        /// paragraph speech synthesizer is speaking a paragraph.
        /// </summary>
        /// <remarks>
        /// Used to indicate whether to suspend editing and highlight words while as they are spoken (highlightSpokenWord == true)
        /// or to continue to allow editing and not highlight words as they are spoken (highlightSpokenWord == false).
        ///
        /// To differentiate between speak all or speak selection: When <c>speakSelectionLength</c> is greater than zero,
        /// the program is speaking the selection. When <c>speakSelectionLength</c> is equal to zero, the program is 
        /// speaking all text in the text box.
        /// </remarks>
        private bool highlightSpokenWord = false;

        /// <summary>
        /// Stores the cursor location in the text box prior to speaking all or selection, to restore the cursor location
        /// after highlighting words as they are spoken.
        /// </summary>
        private int cursorLocation = 0;

        /// <summary>
        /// Stores the selection start location prior to speaking selection, to restore the selection after highlighting
        /// words as they are spoken.
        /// </summary>
        private int speakSelectionStart = 0;

        /// <summary>
        /// Stores the selection start length prior to speaking selection, to restore the selection after highlighting
        /// words as they are spoken.
        /// </summary>
        private int speakSelectionLength = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of Hey, I Lost My Voice!
        /// </summary>
        public HeyILostMyVoiceForm()
        {
            InitializeComponent();

            // Set default font.
            // TODO: This will be stored in Settings.
            try
            {
                richTextBox1.Font = new Font("Calibri", 12, FontStyle.Regular);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error choosing font in initialization: " + ex.ToString() + "\nInnerException: " + ex.InnerException + "\nStack Trace:\n" + ex.StackTrace);
            }

            // Set default values for speech rate and volume.
            // TODO: This will be stored in Settings.
            voiceTalker.WordRate = trackBarWordRate.Value;
            voiceTalker.WordVolume = trackBarWordVolume.Value;
            voiceTalker.ParaRate = trackBarParaRate.Value;
            voiceTalker.ParaVolume = trackBarParaVolume.Value;

            // Set the SpeakCompleted event handler to handle state change from speaking to not speaking after speech has completed.
            voiceTalker.SetParaSpeakCompleted((o, e) => { StopSpeech(); });

            // Set the SpeakProgress event handler to display the speech progress by highlighting the word being spoken.
            voiceTalker.SetParaSpeakProgress(SpeakProgress);

            // Load the list of available voices.
            try
            {
                foreach (InstalledVoice voice in voiceTalker.GetInstalledVoices())
                {
                    comboBoxParaVoice.Items.Add(voice.VoiceInfo.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding voices to the combo boxes: " + ex.ToString() + "\nInnerException: " + ex.InnerException + "\nStack Trace:\n" + ex.StackTrace);
            }

            // Choose a default voice.
            // TODO: The default voice will be in Settings.
            comboBoxParaVoice.SelectedIndex = 0;

            // Load the configuration XML file.
            try
            {
                configurationData.Load("HeyILostMyVoiceConfig.xml");

                // TODO: If the file is not found, I need to create intelligent defaults and save out a new config file.
                // And I should allow for the config file not saving, in case this is being run off read-only media.

                // Find the <Settings> node.
                settingsNode = configurationData.SelectSingleNode("HeyILostMyVoiceSettings/Settings");

                // Find the <Shortcuts> node/
                shortcutsNode = configurationData.SelectSingleNode("HeyILostMyVoiceSettings/Shortcuts");

                // Find the <Pronunciations> node.
                pronunciationsNode = configurationData.SelectSingleNode("HeyILostMyVoiceSettings/Pronunciations");
            }
            catch (Exception ex)
            {
                // TODO: Errors should be logged.
                MessageBox.Show("Error loading the configuration XML file: " + ex.ToString() + "\nInnerException: " + ex.InnerException + "\nStack Trace:\n" + ex.StackTrace);
            }

            // BUG: The text box does not have focus when the program starts up.
            Application.DoEvents();
            richTextBox1.Focus();
        }


        /// <summary>
        /// Gracefully handle window resize events.
        /// </summary>
        /// <param name="sender">The control that sent the event.</param>
        /// <param name="e">A <c>LayoutEventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.control.layout%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.layouteventargs%28v=vs.110%29.aspx"/>
        private void HeyILostMyVoiceForm_Layout(object sender, LayoutEventArgs e)
        {
            // This event is raised once at startup with the AffectedControl
            // and AffectedProperty properties on the LayoutEventArgs as null. 
            // The event provides size preferences for that case.
            if ((e.AffectedControl != null) && (e.AffectedProperty != null))
            {
                // Ensure that the affected property is the Bounds property
                // of the form.
                if (e.AffectedProperty.ToString() == "Bounds")
                {
                    // Ensure minimum form width.
                    if (this.Width < 537)
                    {
                        this.Width = 537;
                    }

                    // Ensure minimum form height.
                    if (this.Height < 220)
                    {
                        this.Height = 220;
                    }

                    // Set the appropriate size for the text box.
                    richTextBox1.Width = this.Width - 40;
                    richTextBox1.Height = this.Height - 145;
                }
            }
        }


        /// <summary>
        /// Filters the keys entered into the rich text box.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">A <c>KeyEventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.control.keydown%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.keyeventargs%28v=vs.110%29.aspx"/>
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Esc to stop speech.
            if (e.KeyCode == Keys.Escape)
            {
                voiceTalker.CancelAllSpeech();
                StopSpeech();
            }

            // If speaking selection or speaking all.
            if (highlightSpokenWord)
            {
                // Space will pause and resume.
                if (e.KeyCode == Keys.Space)
                {
                    PauseOrResume();
                }

                // Left arrow skips back.
                if (e.KeyCode == Keys.Left)
                {
                    SkipBack(e.Shift);
                }

                // Right arrow skips ahead.
                if (e.KeyCode == Keys.Right)
                {
                    SkipAhead(e.Shift);
                }

                // Up arrow increases rate.
                if (e.KeyCode == Keys.Up)
                {
                    // Set the new speech rate.
                    voiceTalker.ParaRate = (voiceTalker.ParaRate + 1 > 10) ? 10 : voiceTalker.ParaRate + 1;
                    trackBarParaRate.Value = voiceTalker.ParaRate;

                    // Restart speech with the new rate.
                    RestartSpeech(speechLocationInWritten);
                }

                // Down arrow decreases rate.
                if (e.KeyCode == Keys.Down)
                {
                    // Set the new speech rate.
                    voiceTalker.ParaRate = (voiceTalker.ParaRate - 1 < -10) ? -10 : voiceTalker.ParaRate - 1;
                    trackBarParaRate.Value = voiceTalker.ParaRate;

                    // Restart speech with the new rate.
                    RestartSpeech(speechLocationInWritten);
                }

                // Ignore all other key presses.
                e.Handled = true;
                return;
            }

            // Check for end of word.
            if (e.KeyCode == Keys.Space)
            {
                if (ExpandShortcut() == false)
                    SpeakWord();
            }

            // Check for end of paragraph (right now, only checking for Enter)
            if (e.KeyCode == Keys.Enter)
            {
                // If Ctrl+Enter is pressed, speak all the text.
                if (e.Control)
                {
                    e.Handled = true;
                    SpeakAll();
                    Application.DoEvents();
                    return;
                }
                // Selected text is spoken with the Enter key. I don't want the text 
                // to disappear when Enter is pressed, so I say it's "Handled".
                if (richTextBox1.SelectionLength > 0)
                {
                    e.Handled = true;
                    SpeakSelection();
                    return;
                }
                // No text is selected. Speak the most recent paragraph.
                else
                {
                    ExpandShortcut();
                    SpeakPara();
                }
            }

            // Check for certain punctuation marks to expand shortcuts.
            if ((e.KeyCode == Keys.OemPeriod && !e.Shift) ||
                (e.KeyCode == Keys.Oemcomma && !e.Shift) ||
                e.KeyCode == Keys.OemQuotes ||
                e.KeyCode == Keys.OemSemicolon ||
                e.KeyCode == Keys.Decimal ||
                (e.KeyCode == Keys.D1 && e.Shift))
            {
                ExpandShortcut();
            }
        }


        /// <summary>
        /// Prevents edits to be made to the text box while speaking all or speaking selection.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">A <c>KeyEventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.control.keypress%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.keypresseventargs%28v=vs.110%29.aspx"/>
        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Prevent edits while highlighting words being spoken.
            if (highlightSpokenWord)
            {
                e.Handled = true;
                return;
            }
        }


        /// <summary>
        /// Skips back the speech a couple seconds, in case the listener missed something.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.control.click%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void buttonSkipBack_Click(object sender, EventArgs e)
        {
            if (highlightSpokenWord)
            {
                // Skip backwards a bit in the text.
                SkipBack(false);
            }

            // Put the focus back on the text box.
            richTextBox1.Focus();
        }


        /// <summary>
        /// Handles clicking on the play/pause button to speak all, speak selection, pause, and resume speech.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.control.click%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void buttonPlayPause_Click(object sender, EventArgs e)
        {
            // If not already speaking.
            if (voiceTalker.ParaSpeechState == SynthesizerState.Ready)
            {
                // Indicate that editing will be halted and words will be highlighted as they are spoken.
                highlightSpokenWord = true;

                try
                {
                    // If text is selected in the text box, speak the selection.
                    if (richTextBox1.SelectionLength > 0)
                    {
                        SpeakSelection();
                    }
                    // If text is not selected, speak all text in the text box.
                    else
                    {
                        SpeakAll();
                        Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in speak all: " + ex.ToString() + "\nInnerException: " + ex.InnerException + "\nStack Trace:\n" + ex.StackTrace);
                }
            }
            // We're already speaking or paused. Clicking here will pause or resume.
            else
            {
                if (highlightSpokenWord)
                {
                    PauseOrResume();
                }
            }

            // Return focus to the text box.
            richTextBox1.Focus();
        }


        /// <summary>
        /// Skips the speech ahead a couple seconds.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.control.click%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void buttonSkipAhead_Click(object sender, EventArgs e)
        {
            if (highlightSpokenWord)
            {
                // Skip forwards a bit in the text.
                SkipAhead(false);
            }

            // Put the focus back on the text box.
            richTextBox1.Focus();
        }


        /// <summary>
        /// Stops speech.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.control.click%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void buttonStop_Click(object sender, EventArgs e)
        {
            // Stop all speech.
            voiceTalker.CancelAllSpeech();

            // Set stopped state.
            StopSpeech();

            // Return focus to the text box.
            richTextBox1.Focus();
        }


        /// <summary>
        /// Highlights words as they are spoken.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">A <c>SpeakProgressEventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.speakprogress%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speakprogresseventargs%28v=vs.110%29.aspx"/>
        private void SpeakProgress(object o, SpeakProgressEventArgs args)
        {
            // Determine the highlight starting location in the textbox from the pronounced string.
            int writtenStart = GetWrittenPositionFromSpoken(args.CharacterPosition);
            int writtenLength;

            // Are we in a pronunciation change? 
            if (writtenToSpoken[writtenToSpokenIndex].PronunciationType == PronunciationTypeEnum.Spelling ||
                writtenToSpoken[writtenToSpokenIndex].PronunciationType == PronunciationTypeEnum.Phoneme)
            {
                // We are in a pronunciation change.
                writtenStart = writtenToSpoken[writtenToSpokenIndex].WrittenPosition;
                writtenLength = writtenToSpoken[writtenToSpokenIndex].WrittenLength;
            }
            else if (writtenToSpoken[writtenToSpokenIndex].PronunciationType == PronunciationTypeEnum.None)
            {
                writtenLength = args.CharacterCount;
            }
            else // PronunciationTypeEnum.PhonemeOverhead
            {
                writtenLength = 0;
            }

            // Update the speech location.
            speechLocationInWritten = writtenStart;

            // If highlighting words as they are spoken.
            if (highlightSpokenWord)
            {
                // Select the current word.
                richTextBox1.SelectionStart = writtenStart + speakSelectionStart;
                richTextBox1.SelectionLength = writtenLength;
            }
        }


        /// <summary>
        /// Turns on or off the Speak on word feature.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.checkbox.checkedchanged%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void checkBoxSpeakOnWord_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Focus();
        }


        /// <summary>
        /// Adjusts the Speak on word speech rate.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.trackbar.scroll%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void trackBarWordRate_Scroll(object sender, EventArgs e)
        {
            voiceTalker.WordRate = trackBarWordRate.Value;
            richTextBox1.Focus();
        }


        /// <summary>
        /// Adjusts the Speak on word speech volume.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.trackbar.scroll%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void trackBarWordVolume_Scroll(object sender, EventArgs e)
        {
            voiceTalker.WordVolume = trackBarWordVolume.Value;
            richTextBox1.Focus();
        }


        /// <summary>
        /// Turns on or off the Speak on Enter feature.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.checkbox.checkedchanged%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void checkBoxSpeakOnPara_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Focus();
        }


        /// <summary>
        /// Adjusts the Speak on Enter speech rate.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.trackbar.scroll%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void trackBarParaRate_Scroll(object sender, EventArgs e)
        {
            // Set the new speech rate.
            voiceTalker.ParaRate = trackBarParaRate.Value;

            // If speaking, restart speech with the new rate at the current location.
            RestartSpeech(speechLocationInWritten);

            // Put the focus back on the text box.
            richTextBox1.Focus();
        }


        /// <summary>
        /// Adjusts the Speak on Enter speech volume.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.trackbar.scroll%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void trackBarParaVolume_Scroll(object sender, EventArgs e)
        {
            // Set the new volume.
            voiceTalker.ParaVolume = trackBarParaVolume.Value;

            // If speaking, restart speech with the new rate at the current location.
            RestartSpeech(speechLocationInWritten);

            // Put the focus back on the text box.
            richTextBox1.Focus();
        }


        /// <summary>
        /// Selects the voice from the list of available voices.
        /// </summary>
        /// <param name="sender">The object that sent this event.</param>
        /// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.windows.forms.combobox.selectedindexchanged%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.eventargs%28v=vs.110%29.aspx"/>
        private void comboBoxParaVoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Because the voice can only change while speech is stopped, I must copy the funcationality of RestartSpeech() to this event.

            // Check for valid pronouncedString and speechLocationInWritten.
            if (pronouncedString == null || speechLocationInWritten >= writtenString.Length)
            {
                // Stop speech.
                voiceTalker.CancelAllSpeech();
                StopSpeech();
                
                // Set the new voice.
                voiceTalker.ParaSelectVoice((String)comboBoxParaVoice.SelectedItem);

                // Put the focus back on the text box.
                richTextBox1.Focus();

                return;
            }

            if (highlightSpokenWord)
            {
                RestartSpeech(speechLocationInWritten, (String)comboBoxParaVoice.SelectedItem);
            }
            else
            {
                // Set the new voice.
                voiceTalker.ParaSelectVoice((String)comboBoxParaVoice.SelectedItem);
            }

            // Put the focus back on the text box.
            richTextBox1.Focus();
        }

        #endregion
    }
}
