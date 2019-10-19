using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace XLedgerExercises.Tests
{
    public class BaseEncoderTester
    {
        [InlineData("Man", "TWFu")] //Test from wiki
        [InlineData("this is a string!!", "dGhpcyBpcyBhIHN0cmluZyEh")] //Test from exercise
        [InlineData("\0\0\0", "AAAA")] //Testing handling of remainder = 0
        [InlineData("M", "TQ")] //Case where characters do not perfectly fit
        [Theory]
        public void TestToBase64(string unconverted, string converted)
        {
            Assert.Equal(converted, BaseEncoder.ToBase64(unconverted));
        }

        [InlineData("TWFu", "Man")] //Test from wiki
        [InlineData("dGhpcyBpcyBhIHN0cmluZyEh", "this is a string!!")] //Test from exercise
        [InlineData("AAAA", "\0\0\0")] //Testing handling of remainder = 0
        [InlineData("TQ", "M\0")] //Case where characters do not perfectly fit
        [Theory]
        public void TestToBase32(string unconverted, string converted)
        {
            Assert.Equal(converted, BaseEncoder.FromBase64(unconverted));
        }
    }
}
