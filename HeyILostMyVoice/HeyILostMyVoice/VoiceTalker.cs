/***************************** VoiceTalker.cs ******************************\
Module Name:  VoiceTalker.cs
Project:      Hey, I Lost My Voice!
Description:  Properties and methods used to use the SAPI speech synthesizer.

MIT licence.
\***************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Speech.Synthesis;

namespace HeyILostMyVoice
{
    /// <summary>
    /// The VoiceTalker class encapsulates the three speech synthesizer objects (speak letter, speak word, and 
    /// speak paragraph/selection/all) and their properties and methods.
    /// </summary>
    /// <remarks>
    /// Speak paragraph and speak word are implemented already. Speak letter is ready to implement, but it is 
    /// slow to speak the letter after it is entered which limits its usefulness in most situations. The design 
    /// decision was to not implement speak letter in **Hey, I Lost My Voice!** The feature could be added later 
    /// for use with those who could benefit from it, such as those with severe disabilites.
    /// </remarks>
    public class VoiceTalker
    {
        #region Fields

        /// <summary>
        /// Speech Synthesizer object to interact with SAPI through C#/.NET. Object is used to 
        /// speak paragraphs, selections, and all text.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer%28v=vs.110%29.aspx"/>
        private SpeechSynthesizer speechSynthPara = new SpeechSynthesizer();

        /// <summary>
        /// Speech Synthesizer object to interact with SAPI through C#/.NET. Object is used to 
        /// speak the word after entry.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer%28v=vs.110%29.aspx"/>
        private SpeechSynthesizer speechSynthWord = new SpeechSynthesizer();

        /// <summary>
        /// Speech Synthesizer object to interact with SAPI through C#/.NET. Object is used to 
        /// speak the letter after entry.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer%28v=vs.110%29.aspx"/>
        private SpeechSynthesizer speechSynthLetter = new SpeechSynthesizer();

        /// <summary>
        /// The opening SSML tags for a proper SSML string. Prepended to the pronounced string just before calling the speech synthesizer.
        /// </summary>
        private static readonly string ssmlOpening = "<?xml version=\"1.0\"?><speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">";

        /// <summary>
        /// The length of the opening SSML tag.
        /// </summary>
        public static readonly int ssmlOpeningLength = ssmlOpening.Length;

        /// <summary>
        /// The closing SSML tag for a proper SSML string. Appended to the pronounced string just before calling the speech synthesizer.
        /// </summary>
        private static readonly string ssmlClosing = "</speak>";

        /// <summary>
        /// The length of the closing SSML tag.
        /// </summary>
        public static readonly int ssmlClosingLength = ssmlClosing.Length;

        #endregion // Fields


        #region Properties

        /// <summary>
        /// Gets and sets the speech rate for speaking paragraph/selection/all.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.rate(v=vs.110).aspx"/>
        public int ParaRate { get { return speechSynthPara.Rate; } set { speechSynthPara.Rate = value; } }

        /// <summary>
        /// Gets and sets the speech volume for speaking paragraph/selection/all.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.volume(v=vs.110).aspx"/>
        public int ParaVolume { get { return speechSynthPara.Volume; } set { speechSynthPara.Volume = value; } }

        /// <summary>
        /// Gets the state of the speech synthesizer as a SynthesizerState enum.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.synthesizerstate%28v=vs.110%29.aspx"/>
        public SynthesizerState ParaSpeechState { get { return speechSynthPara.State; } }

        /// <summary>
        /// Gets and sets the speech rate for speaking a word after entry.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.rate(v=vs.110).aspx"/>
        public int WordRate { get { return speechSynthWord.Rate; } set { speechSynthWord.Rate = value; } }

        /// <summary>
        /// Gets and sets the speech volume for speaking a word after entry.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.volume(v=vs.110).aspx"/>
        public int WordVolume { get { return speechSynthWord.Volume; } set { speechSynthWord.Volume = value; } }

        /// <summary>
        /// Gets the state of the speech synthesizer as a SynthesizerState enum.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.synthesizerstate%28v=vs.110%29.aspx"/>
        public SynthesizerState WordSpeechState { get { return speechSynthWord.State; } }

        /// <summary>
        /// Gets and sets the speech rate for speaking a letter after entry.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.rate(v=vs.110).aspx"/>
        public int LetterRate { get { return speechSynthLetter.Rate; } set { speechSynthLetter.Rate = value; } }

        /// <summary>
        /// Gets and sets the speech volume for speaking a letter after entry.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.volume(v=vs.110).aspx"/>
        public int LetterVolume { get { return speechSynthLetter.Volume; } set { speechSynthLetter.Volume = value; } }

        /// <summary>
        /// Gets the state of the speech synthesizer as a SynthesizerState enum.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.synthesizerstate%28v=vs.110%29.aspx"/>
        public SynthesizerState LetterSpeechState { get { return speechSynthLetter.State; } }

        #endregion // Properties

        
        #region Events

        /// <summary>
        /// Adds an event handler for the event executed when speech completes for the paragraph, selection, or all text.
        /// </summary>
        /// <param name="paraSpeakCompleted">The method to handle the SpeakCompleted event.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.speakcompleted%28v=vs.110%29.aspx"/>
        public void SetParaSpeakCompleted(EventHandler<SpeakCompletedEventArgs> paraSpeakCompleted)
        {
            speechSynthPara.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(paraSpeakCompleted);
        }

        /// <summary>
        /// Adds an event handler for the event executed on each word spoken in the paragraph, selection, or all text.
        /// </summary>
        /// <param name="paraSpeakProgress">The method to handle the SpeakProgress event.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.speakprogress%28v=vs.110%29.aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speakprogresseventargs%28v=vs.110%29.aspx"/>
        public void SetParaSpeakProgress(EventHandler<SpeakProgressEventArgs> paraSpeakProgress)
        {
            speechSynthPara.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(paraSpeakProgress);
        }

        #endregion // Events


        #region Methods

        /// <summary>
        /// Returns the list of voices installed on the computer.
        /// </summary>
        /// <returns>The list of voices installed on the computer.</returns>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/ms586869(v=vs.110).aspx"/>
        public ReadOnlyCollection<InstalledVoice> GetInstalledVoices()
        {
            return speechSynthPara.GetInstalledVoices();
        }

        /// <summary>
        /// Stops all speech synthesis, usually to allow for a new speech call.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.speakasynccancelall(v=vs.110).aspx"/>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.resume%28v=vs.110%29.aspx"/>
        public void CancelAllSpeech()
        {
            speechSynthLetter.SpeakAsyncCancelAll();
            speechSynthWord.SpeakAsyncCancelAll();

            if (ParaSpeechState == SynthesizerState.Paused)
            {
                // Strangely, canceling a speech synthesizer in a paused state doesn't set the state to ready, 
                // but leaves it paused. Resuming it, then canceling it, resets it to ready.
                speechSynthPara.Resume();
            }
            speechSynthPara.SpeakAsyncCancelAll();
        }

        /// <summary>
        /// Speaks each letter as it is entered. Not implemented in **Hey, I Lost My Voice!**
        /// </summary>
        /// <param name="stringToSpeak">String with the letter to speak.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/ms586891%28v=vs.110%29.aspx"/>
        public void LetterSpeakAsync(String stringToSpeak)
        {
            speechSynthLetter.SpeakAsync(stringToSpeak);
        }

        /// <summary>
        /// Speaks each word as it is entered.
        /// </summary>
        /// <param name="stringToSpeak">String with the word to speak.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/ms586891%28v=vs.110%29.aspx"/>
        public void WordSpeakAsync(String stringToSpeak)
        {
            speechSynthWord.SpeakSsmlAsync(ssmlOpening + stringToSpeak + ssmlClosing);
        }

        /// <summary>
        /// Speaks the paragraph / selection / all text.
        /// </summary>
        /// <param name="stringToSpeak">String with the text to speak.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/ms586891%28v=vs.110%29.aspx"/>
        public void ParaSpeakAsync(String stringToSpeak)
        {
            // speechSynthPara.SpeakAsync(stringToSpeak);
            speechSynthPara.SpeakSsmlAsync(ssmlOpening + stringToSpeak + ssmlClosing);
        }

        /// <summary>
        /// Pauses speech of the paragraph / selection / all.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.pause%28v=vs.110%29.aspx"/>
        public void ParaSpeakPause()
        {
            speechSynthPara.Pause();
        }

        /// <summary>
        /// Resumes paused speech of paragraph / selection / all.
        /// </summary>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.resume%28v=vs.110%29.aspx"/>
        public void ParaSpeakResume()
        {
            speechSynthPara.Resume();
        }

        /// <summary>
        /// Selects a voice for the letter speech synthesizer. Not implemented in **Hey, I Lost My Voice!**
        /// </summary>
        /// <param name="newVoice">The voice to use for this speech synthesizer.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.selectvoice%28v=vs.110%29.aspx"/>
        public void LetterSelectVoice(String newVoice)
        {
            speechSynthLetter.SelectVoice(newVoice);
        }

        /// <summary>
        /// Selects a voice for the word speech synthesizer.
        /// </summary>
        /// <param name="newVoice">The voice to use for this speech synthesizer.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.selectvoice%28v=vs.110%29.aspx"/>
        public void WordSelectVoice(String newVoice)
        {
            speechSynthWord.SelectVoice(newVoice);
        }

        /// <summary>
        /// Selects a voice for the paragraph / selection / all speech synthesizer.
        /// </summary>
        /// <param name="newVoice">The voice to use for this speech synthesizer.</param>
        /// <seealso cref="https://msdn.microsoft.com/en-us/library/system.speech.synthesis.speechsynthesizer.selectvoice%28v=vs.110%29.aspx"/>
        public void ParaSelectVoice(String newVoice)
        {
            speechSynthPara.SelectVoice(newVoice);

            // The functionality is written for a separate voice to speak the words as they are entered
            // (ie., a whispering male voice to speak word, and a normal volume female voice to speak 
            // the paragraph). It didn't seem as useful as I thought it would, so using this line, I 
            // force the word voice to be the same as the paragraph / speak all / speak selection voice.
            speechSynthWord.SelectVoice(newVoice);
        }

        #endregion // Methods
    }
}
