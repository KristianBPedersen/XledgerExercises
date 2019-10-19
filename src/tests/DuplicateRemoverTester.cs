using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace XLedgerExercises.Tests
{
    public class DuplicateRemoverTester
    {
        //Tests from exercise
        [InlineData("aabbb", 2, "aabb")]
        [InlineData("abbb", 1, "ab")]
        [InlineData("cccaabbb", 1, "cab")]
        [InlineData("aabbcc", 3, "aabbcc")]

        //My tests
        [InlineData("aabbcc", 0, "")] //Boundary
        [InlineData("\0\0\0", 1, "\0")] //Input value same as placeholder for lastLetter

        [Theory]
        public void TestRepetitions(string withoutRepetitions, int maxRepetitions, string withRepetitions)
        {
            Assert.Equal(withRepetitions, DuplicateRemover.RemoveRepetitions(withoutRepetitions, maxRepetitions));
        }
    }
}
