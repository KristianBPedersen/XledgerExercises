using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace XLedgerExercises
{
    /*
     * This class contains functionality for counting the number of atoms in a standardized chemical notation formula
     * It solves the exercise descibed in https://www.codewars.com/kata/molecule-to-atoms/csharp
     * 
     * The core idea of the algorithm is to use brackets, numbers and atoms as tokens and iterate backwards over them.
     * The advantage of iterating this way is that one encounters the numbers before the atoms they modify.
     * If a number is encountered directly before an bracket it is added to the current multiplier and added to the stack so it can be removed when the bracket is opened.
     * If a number is encountered directly in front of an atom it can be multiplied with the current multiplier to get the correct multiplier for the atom.
     * The multiplier is then used to increment the atoms entry in the dictionaty, which is returned when every token is processed.
     * 
     */
    class AtomCounter
    {
        private int localMultiplier = 1;
        private int currentMultiplier = 1;
        private Stack<int> multiplierStack = new Stack<int>();
        
        private Regex tokenizer = GetTokenizer();
        private Dictionary<string, int> result = new Dictionary<string, int>();

        //Static method to initialize in compliance with exercise example
        //Counts the number of atoms of each type in molecule
        //Skips ill defined tokens and assumes balanced brackets
        public static Dictionary<string, int> ParseMolecule(string molecule)
        {
            return new AtomCounter().CountAtoms(molecule);
        }

        //Returns a regex expression matching all legal tokens
        public static Regex GetTokenizer()
        {
            String closingBracketPattern = @"(?<closeBracket>[\]\}\)])"; //Matches any round, square or curly closing-brackets
            String openingBracketPattern = @"(?<openBracket>[\[\{\(])"; //Matches any round, square or curly opening-brackets
            String atomPattern = @"(?<atom>[A-Z][a-z]?)"; //Matches atoms in standard chemical notation
            String positiveIntegerPattern = @"(?<number>\d+)"; //Matches positive integers
            String pattern = "(" + String.Join('|', closingBracketPattern, openingBracketPattern, atomPattern, positiveIntegerPattern) + ")"; //Matches one of the above patterns

            return new Regex(pattern, RegexOptions.RightToLeft | RegexOptions.Compiled);
        }



        //Counts the number of atoms of each type in molecule
        //Tokenizes the input and iterates backwards over them, invoking the function to process them
        public Dictionary<string, int> CountAtoms(string molecule)
        {
            for (Match match = tokenizer.Match(molecule); match.Success; match = match.NextMatch())
            {
                if (match.Groups["closeBracket"].Success) ProcessClosingBracket();
                else if (match.Groups["openBracket"].Success) ProcessOpenBracket();
                else if (match.Groups["atom"].Success) ProcessAtom(match.Value);
                else ProcessNumber(match.Value);
            }

            return result;
        }

        //Processes an open bracket by removing the multiplier associated with it
        protected void ProcessOpenBracket()
        {
            currentMultiplier /= multiplierStack.Pop();
            localMultiplier = 1;
        }

        //Processes a closing bracket by adding the multiplier associated with it
        protected void ProcessClosingBracket()
        {
            multiplierStack.Push(localMultiplier);
            currentMultiplier *= localMultiplier;
            localMultiplier = 1;
        }

        //Processes an atom by adding the appropriate number of them to the result
        protected void ProcessAtom(String atom)
        {
            int valueIncrement = currentMultiplier * localMultiplier;
            if (result.ContainsKey(atom))
            {
                result[atom] += valueIncrement;
            }
            else
            {
                result.Add(atom, valueIncrement);
            }
            localMultiplier = 1;
        }

        //Processes a number by storing it, so it can be used as a multiplier for a prefixed atom or braket
        protected void ProcessNumber(String number)
        {
            localMultiplier = int.Parse(number);
        }


    }
}
