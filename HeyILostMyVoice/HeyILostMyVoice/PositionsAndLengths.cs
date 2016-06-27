/************************* PositionsAndLengths.cs **************************\
Module Name:  PositionsAndLengths.cs
Project:      Hey, I Lost My Voice!
Description:  Properties and methods used for the PositionsAndLengths class
              that is a lookup table entry to connect a length of text in
              the written text to its corresponding location in the spoken
              text.

MIT licence.
\***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyILostMyVoice
{
    /// <summary>
    /// Identifier for the type of PositionsAndLengths entry.
    /// </summary>
    public enum PronunciationTypeEnum
    {
        /// <summary>
        /// Normal text with no pronunciation changes.
        /// </summary>
        None,

        /// <summary>
        /// A pronunciation change that uses spelling. "Sequim" to "Squim", as an example.
        /// </summary>
        Spelling,

        /// <summary>
        /// A pronunciation change that uses an SSML phoneme element. The text "llama" found between opening
        /// and closing phoneme tags, as an example:
        ///     &lt;phoneme alphabet="x-microsoft-ups" ph="J AA M AX"&gt;llama&lt;/phoneme&gt;
        /// </summary>
        Phoneme,

        /// <summary>
        /// The text of an SSML tag. The text "&lt;phoneme alphabet="x-microsoft-ups" ph="J AA M AX"&gt;" or "&lt;/phoneme&gt;", 
        /// as an example.
        /// </summary>
        PhonemeOverhead
    };


    /// <summary>
    /// A lookup table entry to connect a length of text in the written text to its corresponding 
    /// location in the spoken text.
    /// </summary>
    class PositionsAndLengths
    {
        #region Fields

        /// <summary>
        /// The sorted state of the list of PositionsAndLengths objects. <c>true</c> if the list 
        /// is sorted; otherwise <c>false</c>.
        /// </summary>
        static private bool isSorted = false;

        /// <summary>
        /// The location of an entry in the spoken string.
        /// </summary>
        private int spokenPosition;

        /// <summary>
        /// The length of an entry in the spoken string.
        /// </summary>
        private int spokenLength;

        /// <summary>
        /// The location of an entry in the written string.
        /// </summary>
        private int writtenPosition;

        /// <summary>
        /// The length of an entry in the written string.
        /// </summary>
        private int writtenLength;

        /// <summary>
        /// The PositionsAndLengths entry type.
        /// </summary>
        private PronunciationTypeEnum pronunciationType;

        #endregion


        #region Properties

        /// <summary>
        /// The sorted state of the list of PositionsAndLengths objects. <c>true</c> if the list 
        /// is sorted; otherwise <c>false</c>.
        /// </summary>
        public static bool IsSorted { get { return isSorted; } set { isSorted = value; } }

        /// <summary>
        /// The location of an entry in the spoken string.
        /// </summary>
        public int SpokenPosition { get { return spokenPosition; } set { spokenPosition = value; } }

        /// <summary>
        /// The length of an entry in the spoken string.
        /// </summary>
        public int SpokenLength { get { return spokenLength; } set { spokenLength = value; } }

        /// <summary>
        /// The location of an entry in the written string.
        /// </summary>
        public int WrittenPosition { get { return writtenPosition; } set { writtenPosition = value; } }

        /// <summary>
        /// The length of an entry in the written string.
        /// </summary>
        public int WrittenLength { get { return writtenLength; } set { writtenLength = value; } }

        /// <summary>
        /// The PositionsAndLengths entry type.
        /// </summary>
        public PronunciationTypeEnum PronunciationType { get { return pronunciationType; } set { pronunciationType = value; } }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new PositionsAndLengths entry with the specified values.
        /// </summary>
        /// <param name="SpokenPosition">The location of this entry in the spoken string.</param>
        /// <param name="SpokenLength">The length of this entry in the spoken string.</param>
        /// <param name="WrittenPosition">The location of this entry in the written string.</param>
        /// <param name="WrittenLength">The length of this entry in the written string.</param>
        /// <param name="PronunciationType">The entry type.</param>
        public PositionsAndLengths(int SpokenPosition, int SpokenLength, int WrittenPosition, int WrittenLength, PronunciationTypeEnum PronunciationType)
        { 
            // Store the provided values.
            spokenPosition = SpokenPosition;
            spokenLength = SpokenLength;
            writtenPosition = WrittenPosition;
            writtenLength = WrittenLength;
            pronunciationType = PronunciationType;

            // Adding a new object invalidates the previous sort.
            isSorted = false;
        }


        /// <summary>
        /// Compares two PositionsAndLengths entries by SpokenPosition. Used to sort the PositionsAndLengths list.
        /// </summary>
        /// <param name="x">One PositionsAndLengths entry.</param>
        /// <param name="y">Another PositionsAndLengths entry.</param>
        /// <returns>Returns 1 if x is greater than y.
        /// Returns 0 if x and y are equal.
        /// returns -1 if x is less than y.</returns>
        static public int ComparePac(PositionsAndLengths x, PositionsAndLengths y)
        {
            // Compare the .WrittenEnd values.
            if (x.SpokenPosition > y.SpokenPosition) return 1;
            if (x.SpokenPosition < y.SpokenPosition) return -1;
            return 0;
        }

        #endregion
    }
}
