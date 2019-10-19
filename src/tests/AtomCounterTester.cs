using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using System.Text.RegularExpressions;

namespace XLedgerExercises.Tests
{
    public class AtomCounterTester
    {

        //Tests the regular expression used to tokenize the input
        [InlineData(new []{"C", "B", "A"}, "ABC")]
        [InlineData(new[] { "]", "}", ")", "A", "(", "{", "[" }, "[{(A)}]")]
        [InlineData(new[] { "42", "H" }, "H42")]
        [InlineData(new[] { "42", ")", "2", "H", "(" }, "(H2)42")]
        [Theory]
        public void TestTokenizer(string[] orderedMatches,  string toSearch)
        {
            Match found = AtomCounter.GetTokenizer().Match(toSearch);
            foreach (string match in orderedMatches)
            {
                Assert.Equal(match, found.Value);
                found = found.NextMatch();
            }
        }

        //Tests from exercise
        [InlineData("H2O", new [] { "H", "O"}, new [] { 2, 1 })]
        [InlineData("Mg(OH)2", new[] { "Mg", "O", "H" }, new[] { 1, 2, 2 })]
        [InlineData("K4[ON(SO3)2]2", new[] { "K", "O", "N", "S" }, new[] { 4, 14, 2, 4 })]

        //My Tests
        [InlineData("(H12O)13", new[] { "H", "O" }, new[] { 12*13, 13 })] //Testing numbers with multiple digits
        [InlineData("([(H2O)4]3)2", new[] { "H", "O" }, new[] { 48, 24 })] //Testing consecutive brackets

        [Theory]
        public void TestRepetitions(string molecule, string[] keys, int[] values)
        {
            Dictionary<string, int> atoms = Enumerable.Range(0, keys.Length).ToDictionary(i => keys[i], i => values[i]);

            Assert.Equal(atoms, AtomCounter.ParseMolecule(molecule));
        }
    }
}
