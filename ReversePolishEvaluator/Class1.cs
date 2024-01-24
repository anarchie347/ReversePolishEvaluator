using System.Linq.Expressions;

namespace ReversePolishEvaluator
{
    public static class ReversePolish
    {
        private static readonly Operation<int>[] defaultIntOperations = new[]
        {
            new Operation<int>(a => a[0] + a[1], 2, "+"),
            new Operation<int>(a => a[0] - a[1], 2, "-"),
            new Operation<int>(a => a[0] * a[1], 2, "*"),
            new Operation<int>(a => a[0] / a[1], 2, "/"),
            new Operation<int>(a => (int)Math.Pow(a[0], a[1]), 2, "^")
        };
        private static readonly Operation<double>[] defaultDoubleOperations = new[]
        {
            new Operation<double>(a => a[0] + a[1], 2, "+"),
            new Operation<double>(a => a[0] - a[1], 2, "-"),
            new Operation<double>(a => a[0] * a[1], 2, "*"),
            new Operation<double>(a => a[0] / a[1], 2, "/"),
            new Operation<double>(a => Math.Pow(a[0], a[1]), 2, "^")
        };
        public static int EvaluateIntExpression(string expression, char separator)
        {
            return Evaluate(expression, separator, int.Parse, defaultIntOperations);
        }
        public static int EvaluateIntExpression(string[] splitExpression)
        {
            return Evaluate(splitExpression, int.Parse, defaultIntOperations);
        }
        public static double EvaluateDoubleExpression(string expression, char separator)
        {
            return Evaluate(expression, separator, double.Parse, defaultDoubleOperations);
        }
        public static double EvaluateDoubleExpression(string[] splitExpression)
        {
            return Evaluate(splitExpression, double.Parse, defaultDoubleOperations);
        }

        public static T Evaluate<T>(string expression, char separator, Func<string, T> stringMapFunc, params Operation<T>[] operations)
        {
            return Evaluate(Parse(expression, separator, stringMapFunc, operations));
        }
        public static T Evaluate<T>(string[] splitExpression, Func<string, T> stringMapFunc, params Operation<T>[] operations)
        {
            return Evaluate(Parse(splitExpression, stringMapFunc, operations));
        }
        public static T Evaluate<T>(Token<T>[] expression)
        {
            Stack<T> stack = new Stack<T>();
            for (int i = 0; i < expression.Length; i++)
            {
                Token<T> token = expression[i];
                if (token.IsValue)
                {
                    stack.Push(token.Value);
                }
                else
                {
                    T[] args = new T[token.ArgsCount];
                    for (int j = token.ArgsCount - 1; j >= 0; j--)
                    {
                        args[j] = stack.Pop();
                    }
                    stack.Push(token.Function(args));
                }
            }
            return stack.Pop();
        }
        public static Token<T>[] Parse<T>(string expression, char separator, Func<string, T> stringMapFunc, params Operation<T>[] operations)
        {
            return Parse(expression.Split(separator), stringMapFunc, operations);
        }
        public static Token<T>[] Parse<T>(string[] splitExpression, Func<string, T> stringMapFunc, params Operation<T>[] operations)
        {
            return Array.ConvertAll(splitExpression, strToken =>
            {
                int index = Array.FindIndex(operations, op => op.Symbol == strToken);
                if (index < 0)
                {
                    return Token<T>.NewValue(stringMapFunc(strToken));
                }
                return Token<T>.NewOperation(operations[index]);
            });
        }
    }

    public delegate T Function<T>(params T[] args);
    public struct Operation<T>
    {
        public Function<T> Function { get; private set; }
        public int ArgsCount { get; private set; }
        public string Symbol { get; private set; }
        public Operation(Function<T> func, int argsCount, string symbol)
        {
            Function = func;
            ArgsCount = argsCount;
            Symbol = symbol;
        }
        public T Invoke(params T[] args)
        {
            return Function.Invoke(args);
        }
    }
    public struct Token<T>
    {
        public bool IsValue { get; private set; }
        public T Value { get; private set; }
        public Function<T> Function { get; private set; }
        public int ArgsCount { get; private set; }

        public static Token<T> NewValue(T value)
        {
            return new Token<T>(value);
        }
        public static Token<T> NewOperation(Function<T> func, int argsCount)
        {
            return new Token<T>(func, argsCount);
        }
        public static Token<T> NewOperation(Operation<T> operation)
        {
            return new Token<T>(operation.Function, operation.ArgsCount);
        }
        private Token(T value)
        {
            IsValue = true;
            Value = value;
            Function = (T[] a) => a[0];
            ArgsCount = 1;
        }
        private Token(Function<T> func, int argsCount)
        {
            IsValue = false;
            Value = default;
            Function = func;
            ArgsCount = argsCount;
        }
    }
}