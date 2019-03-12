using System;
using System.Globalization;
using System.Text.RegularExpressions;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    public class IntegerCompiler : AbstractCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IntegerCompiler));
        private static readonly Random Rand = RandomProvider.Rand;

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return IntegerRandom(IntegerFromToRandom(input));
        }

        private static string IntegerFromToRandom(string input)
        {
            // replace <from..to> from&to - inclusive to rand(from, to+1). to+1 for include to.
            return Regex.Replace(input, $"(<{VarNameRegex}{IntIntervalRegex}>)", m =>
            {
                Group group = m.Groups[1];
                int from = int.Parse(m.Groups["from"].Value);
                int to = int.Parse(m.Groups["to"].Value);
                string originalInput = @group.Value;
                if (from > to)
                {
                    Log.Error($"FROM > TO in {originalInput}");
                    return originalInput;
                }

                string replace = Rand.Next(from, to + 1).ToString();

                CreateLuaVariable(m.Groups["var_name"], Log, replace);

                Log.Info($"Replacing {originalInput} with {replace}");
                return string.Format("{0}{1}{2}",
                    m.Value.Substring(0, @group.Index - m.Index),
                    replace,
                    m.Value.Substring(@group.Index - m.Index + @group.Length));
            });
        }

        private static string IntegerRandom(string input)
        {
            // replace <int_random> to both inclusive rand(int.MinValue, int.MaxValue).
            return Regex.Replace(input, $"(<{VarNameRegex}int_random>)", m =>
            {
                Group group = m.Groups[1];
                string originalInput = @group.Value;

                string replace = Convert.ToInt32(DoubleCompiler.RandomDouble(int.MinValue, int.MaxValue))
                    .ToString();

                CreateLuaVariable(m.Groups["var_name"], Log, replace);

                Log.Info($"Replacing {originalInput} with {replace}");
                return string.Format("{0}{1}{2}",
                    m.Value.Substring(0, @group.Index - m.Index),
                    replace,
                    m.Value.Substring(@group.Index - m.Index + @group.Length));
            });
        }
    }
}