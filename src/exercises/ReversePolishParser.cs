using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace XLedgerExercises
{
    /*
     * This class parsers arithmetic expressions in reverse polish syntax.
     * It solves the exercise at https://www.codewars.com/kata/reverse-polish-notation-calculator/csharp
     * Any operator in polish notation is to be applied to the two operands immediately proceeding it.
     * They are then replaced by a single new operand at the position of the operand furthest back.
     * To achieve this efficiently a stack is used. 
     * At all times the top of the stack reprents the operand closest to the currently read token.
     */
    class ReversePolishParser
    {
        private Stack<int> operands = new Stack<int>();

        public static int Parse(string expression)
        {
            if (expression == "") return 0;
            return new ReversePolishParser().ParseReversePolish(expression);
        }

        public int ParseReversePolish(string expression)
        {
            foreach (string token in expression.Split(" "))
            {
                if (Regex.IsMatch(token, @"-?\d+")) ProcessNumber(token);
                else ProcessOperator(token);
            }

            return operands.Pop();
        }

        public void ProcessNumber(string number)
        {
            operands.Push(int.Parse(number));
        }

        public void ProcessOperator(string symbol)
        {
            int secondOperand = operands.Pop();
            int firstOperand = operands.Pop();
            int result = symbol switch
            {
                "+" => firstOperand + secondOperand,
                "-" => firstOperand - secondOperand,
                "*" => firstOperand * secondOperand,
                "/" => firstOperand / secondOperand,
                _ => throw new FormatException("Unrecognized arithmetic symbol: " + symbol),
            };
            operands.Push(result);
        }
    }
}