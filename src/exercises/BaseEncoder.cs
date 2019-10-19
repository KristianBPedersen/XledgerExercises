using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XLedgerExercises
{
    /* 
     * A class for converting between base256 and base64 strings.
     * It solves the exercise described at https://www.codewars.com/kata/base64-encoding/csharp
     * A base 256 string is considered to be a default C# string containing only standard ascii characters.
     * Since this is .Net the default string is utf-8 based, but it should work for most other reasonable defaults as well. 
     * A base 64 string is a default string containing only the characters from https://en.wikipedia.org/wiki/Base64#Base64_table 
     *
     * The algorithm for conversion is essentially long form division by the target base.
     * It starts at the front of the string to be converted and in every step it processes a single character in the string.
     * The residual from the last step is multipied by the source base, then the value of the character is added to it.
     * Then as many characters in the new base is as possible is determined, by the use of integer division and modulo with a bit-filter.
     * How many such charactes can be extracted is determined by keeping track of the number of unprocessed bits.
     */

    class BaseEncoder
    {
        //Returns the base64 representation of the input as described above.
        public static string ToBase64(string toConvert)
        {
            return ToBase(toConvert, 6, 8, CreateBase64Table(), CreateIdentityTable(256));
        }

        //Returns the base256 version of the base64 input as described above.
        public static string FromBase64(string toConvert)
        {
            return ToBase(toConvert, 8, 6, CreateIdentityTable(256), InvertTable(CreateBase64Table()));
        }

        //Creates a table such that the i-th element in the table is the i-th character in the base64 system
        public static char[] CreateBase64Table()
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+\\".ToCharArray();
        }

        //Returns an inverted table. If table[i] = c, then inverted[c] = i.
        public static char[] InvertTable(char[] table)
        {
            char[] invertedTable = new char[table.Max() + 1];
            for (int i = 0; i < table.Length; i++) invertedTable[table[i]] = (char)i;

            return invertedTable;
        }

        //Creates a table such that table[i] = i
        //This functions as a value table for the base256 representation mentioned above, 
        //since every character has itself as its value in this representation
        public static char[] CreateIdentityTable(int size)
        {
            return new char[size].Select((_,i) => (char)i).ToArray();
        }

        //Computes a table containing the sequence of the first powers of two.
        //This precomputing is done as much for readability as for efficiency
        //Since constantly using Math.Pow and converting makes the code harder to read.
        public static int[] ComputePowersOfTwoTable(int numberOfPowers)
        {
            int[] table = new int[numberOfPowers];
            table[0] = 1;
            for (int i = 1; i < table.Length; i++) table[i] = table[i-1]*2;
            return table;
        }

        //Performs the algorithm described in the introduction
        //toBits and fromBits is the number of bits used in respectivly the target and source base
        //The original implementation of this used fewer variables, but was refactored to this to make it more readable.
        public static string ToBase(string toConvert, int toBits, int fromBits, char[] toCharValues, char[] fromCharValues)
        {
            StringBuilder converted = new StringBuilder();

            int[] twoToPowerOf = ComputePowersOfTwoTable(toBits + fromBits);
            int fromBase = twoToPowerOf[fromBits];

            int residue = 0;
            int residueBits = 0;

            foreach (char c in toConvert)
            {
                residue = residue * fromBase + fromCharValues[c];
                residueBits += fromBits;

                while (residueBits >= toBits)
                {
                    int filter = twoToPowerOf[residueBits - toBits];
                    converted.Append(toCharValues[residue/filter]); //Filters away everything but the first toBits bits
                    residue %= filter; //Filters away the used bits
                    residueBits -= toBits; //Keeps track of the number of bits not processed
                }
            }

            //Processes the remaining bits
            if (residueBits > 0) converted.Append(toCharValues[residue * twoToPowerOf[toBits - residueBits]]);

            return converted.ToString();
        }
    }
}
