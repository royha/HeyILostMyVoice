# Hey, I Lost My Voice! Developer Documentation

The Hey, I Lost My Voice! program is a well-documented, easy-to-use speech synthesis and text reading program for use in Microsoft Windows 7 and higher.

## Design philosophy

The design of the Hey, I Lost My Voice! is to make it as easy to use as possible. To that end, I sought to balance simplicity of design with flexibility of use. The most commonly accessed options are available on the main window (volume, rate, voice, etc.), and less frequently accessed options are planned for the Settings dialog. 

The other design philosophy of Hey, I Lost My Voice! is to make the code easy to read and understand. Functions and variables are all extensively documented, and this document is designed to give a thorough overview of the code -- not just what is there, but how it works -- to make it easy for developers to understand the code and easily modify the program for whatever purpose they might desire.

## How Hey, I Lost My Voice! Works

The Hey, I Lost My Voice! program examines the words entered by the user, and optionally speaks these words as they are being typed. If the user types a Shortcut word as defined by the `<Shortcuts>` element of the `HeyILostMyVoice.xml` file, the program replaces the shortcut with the replacement text, to allow for faster typing which can be helpful during conversations. When the user presses Enter, the entire paragraph is optionally spoken. When the user clicks the Play button, all text in the text box or the text selected in the text box is spoken.

Hey, I Lost My Voice! converts the text in the text box to an SSML string that includes custom pronunciations for specific words as defined by the `<Pronunciations>` element of the `HeyILostMyVoice.xml` file. When the SSML string is created, it is spoken by using the Microsoft Speech API as implemented by the .NET 4.6 runtime.

SAPI allows for pause and resume of speech, so that feature was implemented in a rather straightforward way. SAPI does not allow for changes to voice, rate or volume on the fly, however, so that had to be implemented in the program's code. This was done by stopping the existing speech, changing the speech feature (voice, rate, volume), building a new SSML string, and starting speech again with the new settings.

One feature of SAPI is that it has a callback, the `SpeakProgressEvent`, to indicate which word in the text is currently being spoken. Because the SSML text is different than the text box text, a lookup table is created as the SSML string is created to map the SSML word to the word in the text box so that the correct word is highlighted during speech.

## Overview of the code

### Initialization

The current state of initialization is not the planned final state. Settings like font and font size, speech rates and volumes, and default voice are hard coded and user changes to those settings are not persisted. I chose the default values early in development with a plan to add a Settings dialog box, add logic to save and load the settings file, and provide intelligent defaults when the settings file cannot be loaded.

Presently, initialization is performed in the `HeyILostMyVoiceForm()` constructor. In addition to setting the previously mentioned hard coded values, callback event handlers are set for the `SpeakCompleted()` and `SpeakProgress()` SAPI calls. The HeyILostMyVoice.xml file is loaded, and the XML nodes for Shortcuts (`shortcutsNode`) and Pronunciations (`pronunciationsNode`) are set.

Multiple instances of Hey, I Lost My Voice! can be open and actively speaking at the same time.

### Identifying state

Hey, I Lost My Voice! has two primary states. The first state is human voice replacement, in which the typing of words and paragraphs makes the program speak, while allowing typing and text editing to continue during speech. The second state is that of a text reader, during which editing of text is blocked, and the word being spoken is highlighted (selected) in the text box during the speech of that word.

The state is identified by the `highlightSpokenWord` field. When `highlightSpokenWord` is false, typing and editing are allowed (human voice replacement). When `highlightSpokenWord` is true, typing and editing are blocked because the program is highlighting each word as it is spoken (text reader).

The text reader state is further broken down to two states. One state is to read all text in the `richTextBox1` text box, the other state is to read the selected text in the `richTextBox1` text box. These states are identified by the `speakSelectionLength` field. When the `speakSelectionLength` field is greater than zero or not equal to zero, the program is speaking the selected text in the `richTextBox1` text box. When the `speakSelectionLength` field is equal to zero, the program is speaking all text in the `richTextBox1` text box.

|State|Description|Values|
|:--|:--|:--|
|Human voice replacement|`highlightSpokenWord == false;`|Allows editing. Speak on Enter and Speak on word are active (if selected). Does not highlight words as they are spoken.|
|Speak all text | `highlightSpokenWord == false; speakSelectionLength == 0;` | Does not allow editing. Speaks all text, highlighting each word as it is spoken.|
|Speak selected text | `highlightSpokenWord == false; speakSelectionLength > 0;` | Does not allow editing. Speaks selected text, highlighting each word as it is spoken.|

### When typing

**KeyDown** events are handled by the `richTextBox1_KeyDown()` method. The first key the `richTextBox1_KeyDown()` method checks for is the Esc key. Regardless of state, the Esc key is used to stop speech.

If the program is speaking selection or speaking all, Spacebar pauses or resumes, Left Arrow skips back, Right Arrow skips ahead, Up arrow increases speech rate, Down Arrow decreases speech rate. After checking for these control keys while speaking selection or speaking all, the `richTextBox1_KeyDown()` method returns to the caller.

If the program is in human voice replacement mode, pressing Spacebar invokes the `ExpandShortcut()` method. If `ExpandShortcut()` returns false, the previous word was not a shortcut and was not spoken by `ExpandShortcut()`, so the `SpeakWord()` method is called to speak the word that was just typed.

Pressing Enter when in human voice replacement mode can do two things. If there is a selection, it will speak the selection by calling the `SpeakSelection()` method. If there is no selection, it will speak the paragraph by calling the `SpeakPara()` method.

If the key wasn't Spacebar or Enter, then it will check for punctuation keys (period, comma, colon, semicolon). If the key was one of those punctuation keys, the `ExpandShortcut()` method will be called to see if the most recent text entered is a shortcut that needs to be expanded and spoken.

**KeyPress** events are handled by the `richTextBox1_KeyPress()` method. If the program is operating as a text reader (speak all or speak selection), the `richTextBox1_KeyPress()` method will prevent edits to the `richTextBox1` text box.

The `ExpandShortcut()` method searches back from the cursor to identify the previous word. It then looks up the word in the list of shortcuts. If the word matches a shortcut, the shortcut is replaced by the replacement text and spoken if **Speak on word** is checked and the speech synthesizer isn't busy speaking the previous paragraph.

The `SpeakWord()` method identifies the word in front of the cursor in the `richTextBox1` text box, creates an SSML pronunciation string for the word, then calls the `voiceTalker.WordSpeakAsync` method with the pronunciation string.

The `SpeakPara()` method identifies the paragraph in front of the cursor in the `richTextBox1` text box, and if there is a paragraph, it cancels any existing speech by calling the `voiceTalker.CancelAllSpeech()` method. It creates an SSML pronunciation string for the paragraph and calls the `voiceTalker.ParaSpeakAsync` method with the pronunciation string. Then it disables the SkipAhead, SkipBack, and PlayPause buttons on the window.

When the program finishes speaking the paragraph, the `StopSpeech()` method is called to return the program state to a speech-completed state. Button images and states are restored, the `richTextBox1` text box selection is restored, and variables are set to a ready-to-speak state.

### When reading the selection or all text

There is one way to speak all text: Click the Play button when speech is not underway (if speech is underway, the Pay button becomes the Pause / Resume button). There are two ways to speak a selection: Select text and click the Play button, or select text and press Enter.

For speak selection, the `SpeakSelection()` method is called. For speak all, the `SpeakAll()` method is called. These two methods are nearly identical. The only difference is how they set the `speakSelectionStart` and `speakSelectionLength` variables. In the `SpeakSelection()` method, the values are set to the `richTextBox1.SelectionStart` and `richTextBox1.SelectionLength` values, respectively. In the `SpeakAll()` method, the `richTextBox1` values are set to zero.

The `SpeakSelection()` and `SpeakAll()` methods cancel all current speech, set the `highlightSpokenWord` variable to `true`, set the `speakSelectionStart` and `speakSelectionLength` variables, create the pronunciation string by calling the `CreatePronunciationString()` method, call the `voiceTalker.ParaSpeakAsync` method with the pronunciation string, and set the `buttonPlayPause button to Pause.

The `CreatePronunciationString()` method creates an SSML string, minus the SSML opening and closing strings, as identified in the `ssmlOpening` and `ssmlClosing` variables. The opening and closing SSML strings are added in the `voiceTalker.ParaSpeakAsync` method. The `CreatePronunciationString()` method also creates the `writtenToSpoken` lookup table which identifies where the text box words appear in the SSML text, to allow for accurate word highlighting during speech.

Each entry in the `writtenToSpoken` lookup table identify is a `PositionsAndLengths` class object, which identifies a "chunk" of text. Each chunk of text consists of the `SpokenPosition` and `SpokenLength` variables, which identify the chunk's location in the SSML string, and the `WrittenPosition` and `WrittenLength` variables, which identify the chunk's location in the written string. New chunks are created each time a pronunciation change is processed into the SSML string (with the exception of spelling changes that are identical in length to the word in the written string).

Some pronunciation changes can be performed by a change in spelling. "Obama" in the text box can be spelled "Obahma" in the SSML string. Other pronunciation changes must use the `<phoneme>` SSML element, which contains the `alphabet` and `ph` attributes. The text, "whatchamacallit" in the text box would appear as `<phoneme alphabet="x-microsoft-ups" ph="S1 W AA T . CH AX . M AX . S2 K AA L . IH T">whatchamacallit</phoneme>`.

When the `CreatePronunciationString()` method adds a spelling change pronunciation, the `PronounceBySpelling()` method is called. When the `CreatePronunciationString()` method adds a phoneme change pronunciation, the `PronounceByPhoneme()` method is called.

If the length of the spelling change pronunciation is identical to the word it is replacing (ie,. "Rice" with "Ryze"), the word in the SSML string is replaced with the new spelling, and no new chunks are created. If the spelling change length is different than the word it is replacing (ie., "Obama" with "Obahma"), the `PronounceBySpelling()` method takes the current chunk and breaks it into three new chunks. The first chunk is the SSML text before the change, the second chunk is the changed spelling in the SSML text, and the last chunk is the text after the change.

When the `PronounceByPhoneme()` method is called, it adds five new chunks. The first chunk is the SSML text before the change, the second chunk is the opening to the **phoneme** element, (ie., `<phoneme alphabet="x-microsoft-ups" ph="S1 W AA T . CH AX . M AX . S2 K AA L . IH T">`), the third chunk is the word being pronounced, (ie., ``), the fourth chunk is the closing to the **phoneme** element (ie., `</phoneme>`), and the last chunk is the text after the change.

After all of the pronunciations have been added to the SSML string, `CleanUpSsml()` is called to remove characters that would cause SSML validation to fail, such as extraneous greater than and less than characters, and ampersands.

### When changing rate, volume, or voice on the fly

The Microsoft SAPI allows for pause and resume of speech while speech is underway. It does not, however, allow for a change of speech rate, volume, or voice while the speech engine is speaking. To accomplish this change, speech must be stopped, the voice must be changed, and the speech restarted on the same word that was being spoken. This is accomplished by the `RestartSpeech()` overloaded method.

This process is also used by the `SkipAhead()` and `SkipBack()`methods.

## Contributions

I am open to contributions to Hey, I Lost My Voice!. Code is, of course, welcome. So is artwork, since I don't have an icon for this program yet. Translation of the program and end-user documentation to other languages is welcome as well.

The goals of Hey, I Lost My Voice! are:

1. To give a voice to those who have temporarily or permanently lost their ability to speak.
2. To make a quality text-to-speech reader app.

I plan to continue active development on this program. Accessibility is important to me, and this program is my current top focus for accessibility.

 My current roadmap for this program includes:

1. Create a Settings tabbed dialog box, with Settings, a Shortcuts editor, and a Pronunciation editor.
1. Persist changed settings to the HeyILostMyVoice.xml file, and choose intelligent defaults when the file can't be loaded.
1. Autoload the file HeyILostMyVoiceQuickStart.txt. Include quick start instructions in the file.
1. Create and add program icon.
1. Pronunciation by voice. A changed pronunciation with one voice ("Obahma" on Microsoft Anna) may not work with another voice.
1. Ensure the highlighted word in the text box stays 1/3rd to 1/5th of the way above the bottom of the text box.
1. Shortcut improvements, such as reverting the shortcut on backspace or ctrl-z, and capitalizing the first letter of an expanded shortcut if it's at the beginning of a sentence.
1. Change from XmlDataDocument for the .xml file to XDocument and use LINQ to XML processing.
1. Improve error handling
1. Create a YouTube video or two showing how to use this program.

My day job is that of a programmer writer — a specialized technical writer who writes documents for software developers. Because of my job, I have to read and understand many different code projects using a variety of coding styles and documentation standards (all too frequently in my job, “none” is the documentation standard). So I chose to write and document this project the way I would dream of finding a project I would have to work on either as a developer or as a programmer writer. And that is what I want to find in pull requests.

What I want in submitted changes:

- Keep the structure of the code so that it's consistent, easy to read and understand, and easy for a new developer to get up to speed quickly on the project.
- Clear and concise comments above each function to describe at minimum the summary of the function, the parameters, and return values. Additional remarks are always welcome.
- Comments within each function to describe the actions of the function.
- A comment on each variable to describe the purpose and use of that variable.

Contributions that do not maintain these standards are not likely to be accepted.

