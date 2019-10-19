using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace XLedgerExercises.Tests
{
    public class ReversePolishParserTester
    {
        //Tests from exercise
        [InlineData("5 1 2 + 4 * + 3 -", 14)]
        [InlineData("", 0)]
        //My Tests
        [InlineData("10 2 /", 5)]  //Tests numbers with multiple digits and division
        [InlineData("1 0 *", 0)] //Tests 0 as operand
        [InlineData("2 -3 -", 5)] //Tests negative operands
        [Theory]
        public void TestRepetitions(string expression, int result)
        {
            Assert.Equal(result, ReversePolishParser.Parse(expression));
        }

        //Tests that invalid operators are detected
        [Fact]
        public void TestErrorMessage()
        {
            Assert.Throws<FormatException>(() => ReversePolishParser.Parse("1 2 a"));
        }
    }
}
