using System;
using System.Text;

namespace XLedgerExercises
{
    /*
    * Answers Oppgave 1 in the test
    * Returns a string containing the characters of the input,
    * except substring  with characters sequentially repeated more than maxRepetitions are shortened to the limit.
    */
    class DuplicateRemover
    {
        public static String RemoveRepetitions(String input, int maxRepetitions)
        {
            StringBuilder withoutRepetitions = new StringBuilder(input.Length);

            char lastLetter = (char)0;
            int repetitions = -1;

            foreach (char letter in input)
            {
                repetitions = (letter == lastLetter) ? repetitions + 1 : 0;
                if (repetitions < maxRepetitions) withoutRepetitions.Append(letter);
                lastLetter = letter;
            }

            return withoutRepetitions.ToString();
        }
    }
}
