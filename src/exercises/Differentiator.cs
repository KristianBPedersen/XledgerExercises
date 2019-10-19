using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XLedgerExercises
{
    /* 
     * Class for differentiating simple prefix arithmetic and trigonometric expressions
     * It solves the exercise from https://www.codewars.com/kata/symbolic-differentiation-of-prefix-expressions/csharp
     * The core of the algorithm is to parse the expression from the outside, inward and determine what differentiation rules are to be applied
     * Then, if necessary, any nested arguments are differentiated to be used in the rule.
     * As such it is a recursive algorithm, essentially iterating through a grammar tree in a depth-first, left-to-right fashion.
     * 
     * If I were to do it again I probably would have created a class to represent tokens.
     * It could hold the logic for doing operations on tokens, making the notation much cleaner.
     * The current implementation is also slowed down by the cloning of substrings of the expression. 
     * With such a token class one could represent the tokens with start and stop index in a shared string, thus improving efficiency by an order of magnitute.
     * However, implementing this change would essentially amount to refactoring every line of the program, and is therefore not done.
     */

    class Differentiator
    {
        //Differentiates a expression on the prefix format where operation p applied to x and y is represented (p x y)
        //Returns a string of the differentiated expression in the same format.
        public static string Differentiate(string expression)
        {
            Match tokens = DifferentiationTokenizer.tokenizer.Match(expression);

            if (tokens.Groups["operator"].Success)
            {
                return DifferentiateOperator(tokens.Groups["operator"].Value, tokens.Groups["arguments"].Value);
            }
            if (tokens.Groups["function"].Success)
            {
                return DifferentiateFunction(tokens.Groups["function"].Value, tokens.Groups["arguments"].Value);
            }
            if ((tokens.Groups["constant"].Success))
            {
                return DifferentiateConstant(tokens.Value);
            }
            if ((tokens.Groups["variable"].Success))
            {
                return DifferentiateVariable(tokens.Value);
            }

            throw new FormatException();
        }

        /* 
         * Differentiates an operator expression. Uses rules for operands being general diffentiable functions.
         * +/-: Differentiation linear over +/-
         * *: Chain rule
         *./: https://www.wolframalpha.com/input/?i=differentiate+f%28x%29%2Fg%28x%29
         * ^: https://www.wolframalpha.com/input/?i=differentiate+f%28x%29%5Eg%28x%29 (using expanded form)
         */
        public static string DifferentiateOperator(string operatorSymbol, string arguments)
        {
            string[] operands = DifferentiationTokenizer.SplitArguments(arguments);

            string firstDerivative = Differentiate(operands[0]);
            string secondDerivative = Differentiate(operands[1]);

            switch (operatorSymbol[0])
            {
                case '+':
                case '-':
                    return StringMath.AddStrings(firstDerivative, secondDerivative, operatorSymbol[0]);
                case '*':
                    return StringMath.AddStrings(StringMath.MultiplyStrings(firstDerivative, operands[1]), StringMath.MultiplyStrings(secondDerivative, operands[0]));
                case '/':
                    return StringMath.DivideStrings(
                        StringMath.AddStrings(
                            StringMath.MultiplyStrings(firstDerivative, operands[1]), 
                            StringMath.MultiplyStrings(secondDerivative, operands[0]), '-'),
                        StringMath.PowerStrings(operands[1], "2"));
                case '^':
                    return StringMath.AddStrings(
                        StringMath.MultiplyStrings(
                            StringMath.MultiplyStrings(firstDerivative, operands[1]),
                            StringMath.PowerStrings(operands[0], StringMath.AddStrings(operands[1], "1", '-'))),
                        StringMath.MultiplyStrings(
                            StringMath.MultiplyStrings(secondDerivative, StringMath.LogarithmString(operands[0])),
                            StringMath.PowerStrings(operands[0], operands[1])
                        )
                    );
                default:
                    throw new FormatException();
            }
        }

        /* 
         * Differentiates one-variable functions with the substitution rule, which works as follows:
         * First it computes the outer derivative.
         * This is the derivative of the function with the argument substituted for a single variable.
         * Then it multiplies this with the derivative of the substituted argument.
         */
        public static string DifferentiateFunction(string functionName, string argument)
        {
            var outerDerivative = functionName switch
            {
                "sin" => StringMath.ToPrefixFormat(argument, "cos"),
                "cos" => StringMath.MultiplyStrings("-1", StringMath.ToPrefixFormat(argument, "sin")),
                "tan" => StringMath.DivideStrings("1", StringMath.PowerStrings(StringMath.ToPrefixFormat(argument, "cos"), "2")),
                "ln" => StringMath.DivideStrings("1", argument),
                "exp" => StringMath.ToPrefixFormat(argument, "exp"),
                _ => throw new FormatException(),
            };
            return StringMath.MultiplyStrings(Differentiate(argument), outerDerivative);
        }

        //Differentiates a variable.
        //Since we are looking at only single valued functions tge result of this is always 1
        public static string DifferentiateVariable(string variable)
        {
            return "1";
        }

        //Differentiates a constant.
        //This is always 0.
        public static string DifferentiateConstant(string constant)
        {
            return "0";
        }
    }

    /* 
     * Contains logic for doing arithmetic with strings on the prefix format.
     * Simplifies factors of 0 and 1 where possible, 
     * Computes all function chains with only constants
     * But does not simplify polynomials or expressions with multiple single-valued functions
     */
    public class StringMath
    {
        //Returns the string representation of the operator applied to the operands.
        public static string ToPrefixFormat(string operand1, string operand2, char operatorSymbol)
        {
            return "(" + operatorSymbol + " " + operand1 + " " + operand2 + ")";
        }

        //Returns the string representation of the function applied on the argument.
        public static string ToPrefixFormat(string argument, string functionName)
        {
            return "(" + functionName + " " + argument + ")";
        }

        //Returns a string representing the addition of the mathematical values the operands represent.
        //Also support operatorSymbol = '-'. Then it acts as subtraction on the values.
        public static string AddStrings(string operand1, string operand2, char operatorSymbol = '+')
        {

            if (operand1 == "0")
            {
                return (operatorSymbol == '-') ? InvertString(operand2) : operand2;
            }

            if (operand2 == "0")
            {
                return operand1;
            }

            if (DifferentiationTokenizer.IsNumber(operand1) && DifferentiationTokenizer.IsNumber(operand2))
            {
                return (int.Parse(operand1) + (operatorSymbol == '+' ? 1 : -1) * int.Parse(operand2)).ToString();
            }
            return ToPrefixFormat(operand1, operand2, operatorSymbol);
        }

        //Returns a string representing the multiplication of the mathematical values the operands represent.
        public static string MultiplyStrings(string operand1, string operand2)
        {
            if (operand1 == "0" || operand2 == "0")
            {
                return "0";
            }
            if (operand1 == "1")
            {
                return operand2;
            }
            if (operand2 == "1")
            {
                return operand1;
            }
            if (DifferentiationTokenizer.IsNumber(operand1) && DifferentiationTokenizer.IsNumber(operand2))
            {
                return (int.Parse(operand1) * int.Parse(operand2)).ToString();
            }
            return ToPrefixFormat(operand1, operand2, '*');
        }

        //Returns a string representing the division of the mathematical values the operands represent.
        public static string DivideStrings(string operand1, string operand2)
        {

            if (operand1 == "0")
            {
                return "0";
            }

            if (operand2 == "1")
            {
                return operand1;
            }

            if (DifferentiationTokenizer.IsNumber(operand1) && DifferentiationTokenizer.IsNumber(operand2))
            {
                return (int.Parse(operand1) / int.Parse(operand2)).ToString();
            }
            return ToPrefixFormat(operand1, operand2, '/');
        }

        //Returns a string representing the power function applied to the mathematical values the operands represent.
        public static string PowerStrings(string operand1, string operand2)
        {
            if (operand2 == "0")
            {
                return "1";
            }
            if (operand2 == "1")
            {
                return operand1;
            }

            if (operand1 == "0")
            {
                return "0";
            }

            if (DifferentiationTokenizer.IsNumber(operand1) && DifferentiationTokenizer.IsNumber(operand2))
            {
                return ((int)Math.Round(Math.Pow(int.Parse(operand1), int.Parse(operand2)))).ToString();
            }
            return ToPrefixFormat(operand1, operand2, '^');
        }

        //TODO: Remove
        public static string LogarithmString(string expression)
        {
            return "(ln " + expression + ")";
        }

        //Inverts the sign of a string representation of a number. If it was positive it is now negative and visa versa. 
        public static string InvertString(string number)
        {
            return (number[0] == '-') ? number.Substring(1) : String.Concat('-', number);
        }
    }


    /*
     * Contains functions related to the tokenization required to do differentiation.
     * The core idea is to use regular expressions to parse the mathematical grammar.
     * Since this pattern is somewhat complex the pieces making it up are made public so they can be tested directly on.
     */
    public class DifferentiationTokenizer
    {
        public const string operatorPattern = @"(?<operator>[\+\-\*\/\^])";
        public const string functionPattern = @"(?<function>sin|cos|tan|exp|ln)";
        public const string argumentsPattern = @"(?<arguments>.+)";
        public const string prefixFunctionPattern = @"\((" + operatorPattern + "|" + functionPattern + @")\ " + argumentsPattern + @"\)";
        public const string constantPattern = @"(?<constant>\-?\d+)";
        public const string variablePattern = @"(?<variable>x)";


        public static Regex tokenizer = CreateTokenizer();
        private static Regex numberChecker = CreateNumberChecker();
        private static Regex bracketFinder = CreateBracketFinder();

        private static Regex CreateTokenizer()
        {
            string expressionPattern = @"^(" + String.Join("|", prefixFunctionPattern, constantPattern, variablePattern) + @")";
            return new Regex(expressionPattern, RegexOptions.Compiled);
        }

        private static Regex CreateNumberChecker()
        {
            return new Regex("^" + constantPattern + "$", RegexOptions.Compiled);
        }

        private static Regex CreateBracketFinder()
        {
            return new Regex(@"[\(\)]", RegexOptions.Compiled);
        }

            public static bool IsNumber(string input)
        {
            return numberChecker.IsMatch(input);
        }

        public static string[] SplitArguments(string arguments)
        {
            if (arguments[0] == '(')
            {
                int bracketBalance = 0;
                Match bracket = bracketFinder.Match(arguments);

                while (bracketBalance >= 0)
                {
                    bracket = bracket.NextMatch();
                    if (bracket.Value[0] == ')') bracketBalance--;
                    else bracketBalance++;
                }
                return new string[] { arguments.Substring(0, bracket.Index + 1), arguments.Substring(bracket.Index + 2) };
            }

            string constantOrVariable = DifferentiationTokenizer.tokenizer.Match(arguments).Value;
            return new string[] { constantOrVariable, arguments.Substring(constantOrVariable.Length + 1) };
        }
    } 
}
