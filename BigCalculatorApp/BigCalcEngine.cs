using System;
using System.Data;
using System.Globalization;
using System.Numerics;

namespace BigCalculatorApp
{
    public static class BigCalcEngine
    {
        /// <summary>
        /// Parse and evaluate an expression like (8-5)*2.5^3, returning a decimal result.
        /// Supports +, -, *, /, ^, parentheses, and decimal numbers.
        /// </summary>
        public static decimal EvaluateExpression(string expr)
        {
            // Replace symbols with standard ones
            expr = expr.Replace("×", "*")
                       .Replace("÷", "/")
                       .Replace("x", "*");

            // Remove spaces
            expr = expr.Replace(" ", "");

            if (string.IsNullOrEmpty(expr))
                return 0m;

            int index = 0;
            decimal result = ParseExpression(expr, ref index);

            // If there's leftover unparsed text, that's an error
            if (index < expr.Length)
                throw new Exception($"Unexpected character at position {index} in '{expr}'.");

            return result;
        }

        //========================================================================
        // Grammar Implementation
        //========================================================================

        //
        // Expression = Term { (+|-) Term }
        //
        private static decimal ParseExpression(string expr, ref int index)
        {
            decimal value = ParseTerm(expr, ref index);

            while (true)
            {
                if (index >= expr.Length)
                    break;

                char c = expr[index];

                // If we see + or -, consume it and parse the next Term
                if (c == '+' || c == '-')
                {
                    index++; // consume the operator
                    decimal rightVal = ParseTerm(expr, ref index);

                    if (c == '+')
                        value = value + rightVal;
                    else
                        value = value - rightVal;
                }
                else
                {
                    // Not a plus or minus => expression ends here
                    break;
                }
            }

            return value;
        }

        //
        // Term = Exponent { (*|/) Exponent }
        //
        private static decimal ParseTerm(string expr, ref int index)
        {
            decimal value = ParseExponent(expr, ref index);

            while (true)
            {
                if (index >= expr.Length)
                    break;

                char c = expr[index];

                if (c == '*' || c == '/')
                {
                    index++; // consume the operator
                    decimal rightVal = ParseExponent(expr, ref index);

                    if (c == '*')
                        value = value * rightVal;
                    else
                    {
                        if (rightVal == 0)
                            throw new DivideByZeroException("Divide by zero.");
                        value = value / rightVal;
                    }
                }
                else
                {
                    // Not * or / => this Term is done
                    break;
                }
            }

            return value;
        }

        //
        // Exponent = Primary [ '^' Exponent ]
        //
        // (We allow right-associative exponent, i.e. 2^3^2 => 2^(3^2)=512, if you want that.
        // But to keep it simple, we'll parse only left^right once. Modify if you need repeated.)
        //
        private static decimal ParseExponent(string expr, ref int index)
        {
            // Parse base
            decimal baseVal = ParsePrimary(expr, ref index);

            // If next char is '^', parse the exponent
            if (index < expr.Length && expr[index] == '^')
            {
                index++; // consume '^'
                decimal exponentVal = ParseExponent(expr, ref index);
                // For repeated exponent logic, do: ParsePrimary(...) if you want left-associative.

                double result = Math.Pow((double)baseVal, (double)exponentVal);
                return (decimal)result;
            }

            return baseVal;
        }

        //
        // Primary = Number | ( Expression ) | (optionally a unary '-') 
        // Also handle '√' if you want sqrt.
        //
        private static decimal ParsePrimary(string expr, ref int index)
        {
            // Skip any whitespace
            while (index < expr.Length && char.IsWhiteSpace(expr[index]))
            {
                index++;
            }

            // Optional unary plus or minus
            bool negative = false;
            if (index < expr.Length && (expr[index] == '+' || expr[index] == '-'))
            {
                negative = (expr[index] == '-');
                index++;
            }

            // Check for sqrt
            if (index < expr.Length && expr[index] == '√')
            {
                index++; // consume '√'
                decimal inside = ParsePrimary(expr, ref index);
                if (inside < 0)
                    throw new Exception("Cannot take sqrt of negative number with decimals.");

                double sqrtVal = Math.Sqrt((double)inside);
                return negative ? -(decimal)sqrtVal : (decimal)sqrtVal;
            }

            // Parentheses
            if (index < expr.Length && expr[index] == '(')
            {
                index++; // consume '('
                decimal val = ParseExpression(expr, ref index);

                if (index >= expr.Length || expr[index] != ')')
                    throw new Exception("Missing closing parenthesis.");
                index++; // consume ')'

                return negative ? -val : val;
            }

            // Otherwise, parse a number
            int startPos = index;
            bool foundDecimal = false;

            while (index < expr.Length)
            {
                char c = expr[index];
                if (char.IsDigit(c))
                {
                    index++;
                }
                else if (c == '.')
                {
                    if (foundDecimal)
                        throw new Exception("Multiple decimal points in number.");
                    foundDecimal = true;
                    index++;
                }
                else
                {
                    // no longer part of the number
                    break;
                }
            }

            if (startPos == index)
            {
                // Means we didn't parse any digits => error
                throw new Exception($"Invalid number at position {index} in expression.");
            }

            string numStr = expr.Substring(startPos, index - startPos);
            if (!decimal.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal numberVal))
                throw new Exception($"Cannot parse '{numStr}' as a decimal.");

            return negative ? -numberVal : numberVal;
        }
    }
}
