using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Text.RegularExpressions;

namespace XLedgerExercises.Tests
{
    /*
     * Testing more here than in the other tests.
     * This is a much more complex program (both in the intuitive and cyclomatic sense),
     * so therefore it makes sense to do more tests for this case to prove correctness.
     * Having this many unit tests were also a great help when bugfixing.
     */
    public class DifferentiatorTester
    {

        [InlineData("+", "+")]
        [InlineData("-", "-")]
        [InlineData("*", "*")]
        [InlineData("/", "/")]
        [InlineData("^", "^")]
        [InlineData("?", "")]
        [Theory]
        public static void TestOperatorPattern(string testString, string match)
        {
            Assert.Equal(match, Regex.Match(testString, DifferentiationTokenizer.operatorPattern).Value);
        }

        [InlineData("sin", "sin")]
        [InlineData("cos", "cos")]
        [InlineData("tan", "tan")]
        [InlineData("ln", "ln")]
        [InlineData("exp", "exp")]
        [InlineData("cin", "")]
        [Theory]
        public static void TestFunctionPattern(string testString, string match)
        {
            Assert.Equal(match, Regex.Match(testString, DifferentiationTokenizer.functionPattern).Value);
        }

        [InlineData("22", "22")]
        [InlineData("0", "0")]
        [InlineData("x", "")]
        [InlineData("12 x","12")]
        [Theory]
        public static void TestConstantPattern(string testString, string match)
        {
            Assert.Equal(match, Regex.Match(testString, DifferentiationTokenizer.constantPattern).Value);
        }

        [InlineData("22", "")]
        [InlineData("0", "")]
        [InlineData("x", "x")]
        [Theory]
        public static void TestVariablePattern(string testString, string match)
        {
            Assert.Equal(match, Regex.Match(testString, DifferentiationTokenizer.variablePattern).Value);
        }

        [InlineData("(+ 12 x)", "(+ 12 x)")]
        [InlineData("(* 0 0)", "(* 0 0)")]
        [InlineData("(+ (+ 12 x) 12)", "(+ (+ 12 x) 12)")]
        [InlineData("x", "")]
        [Theory]
        public static void TestFunctionExpression(string testString, string match)
        {
            Assert.Equal(match, Regex.Match(testString, DifferentiationTokenizer.prefixFunctionPattern).Value);
        }

        [Fact]
        public static void TestTokenization()
        {
            Regex tokenizer = DifferentiationTokenizer.tokenizer;
            Match match = tokenizer.Match("(+ (+ 12 x) 12)");

            Assert.Equal("+", match.Groups["operator"].Value);
            Assert.Equal("(+ 12 x) 12", match.Groups["arguments"].Value);
        }

        [InlineData("5", "7", "12")]
        [InlineData("0", "2", "2")]
        [InlineData("(+ x 12)", "13", "(+ (+ x 12) 13)")]
        [Theory]
        public static void TestAddition(string expression1, string expression2, string answer)
        {
            Assert.Equal(answer, StringMath.AddStrings(expression1, expression2));
        }

        [InlineData("5", "7", "35")]
        [InlineData("0", "2", "0")]
        [InlineData("2", "0", "0")]
        [InlineData("2", "1", "2")]
        [InlineData("(+ x 12)", "13", "(* (+ x 12) 13)")]
        [Theory]
        public static void TestMultiplication(string expression1, string expression2, string answer)
        {
            Assert.Equal(answer, StringMath.MultiplyStrings(expression1, expression2));
        }

        [InlineData("35", "7", "5")]
        [InlineData("(+ x 12)", "13", "(/ (+ x 12) 13)")]
        [Theory]
        public static void TestDivision(string expression1, string expression2, string answer)
        {
            Assert.Equal(answer, StringMath.DivideStrings(expression1, expression2));
        }

        [InlineData("5", "7", "78125")]
        [InlineData("0", "2", "0")]
        [InlineData("2", "0", "1")]
        [InlineData("3", "1", "3")]
        [InlineData("(+ x 12)", "13", "(^ (+ x 12) 13)")]
        [Theory]
        public static void TestPower(string expression1, string expression2, string answer)
        {
            Assert.Equal(answer, StringMath.PowerStrings(expression1, expression2));
        }

        //Testing expressions without functions
        [InlineData("17", "0")]
        [InlineData("x", "1")]
        //Testing operators
        [InlineData("(+ 3 x)", "1")]
        [InlineData("(* 3 x)", "3")]
        [InlineData("(- 3 x)", "-1")]
        [InlineData("(/ 4 x)", "(/ -4 (^ x 2))")]
        [InlineData("(^ 5 x)", "(* (ln 5) (^ 5 x))")]
        //Testing functions with inner non-trivial expression
        [InlineData("(sin (^ x 2))", "(* (* 2 x) (cos (^ x 2)))")]
        [InlineData("(cos (^ x 3))", "(* (* 3 (^ x 2)) (* -1 (sin (^ x 3))))")]
        [InlineData("(tan (/ 1 x))", "(* (/ -1 (^ x 2)) (/ 1 (^ (cos (/ 1 x)) 2)))")]
        [InlineData("(ln (* x x))", "(* (+ x x) (/ 1 (* x x)))")]
        [InlineData("(exp (- x (* x 2)))", "(* -1 (exp (- x (* x 2))))")]
        //Testing complex expressions
        [InlineData("(sin (sin x))", "(* (cos x) (cos (sin x)))")] //Testing nested functions
        [InlineData("(+ x (+ x (+ x (+ x x))))", "5")] //Testing deep parenthesis nesting
        //Tests from exercise
        [InlineData("(* 1 x)", "1")]
        [InlineData("(^ x 3)", "(* 3 (^ x 2))")]
        [InlineData("(cos x)", "(* -1 (sin x))")]
        [Theory]
        public static void TestDifferentiation(string expression, string answer)
        {
            Assert.Equal(answer, Differentiator.Differentiate(expression));
        }
    }
}
