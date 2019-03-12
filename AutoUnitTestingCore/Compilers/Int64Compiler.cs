using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    class Int64Compiler: AbstractCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Int64Compiler));
        private static readonly Random Rand = RandomProvider.Rand;

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return IntegerRandom(IntegerFromToRandom(input));
        }

        private static string IntegerFromToRandom(string input)
        {
            // replace <from..to> from&to - inclusive to rand(from, to+1). to+1 for include to.
            return Regex.Replace(input, $"(<int64, ?{VarNameRegex}{IntIntervalRegex}>)", m =>
            {
                Group group = m.Groups[1];
                long from = long.Parse(m.Groups["from"].Value);
                long to = long.Parse(m.Groups["to"].Value);
                string originalInput = @group.Value;
                if (from > to)
                {
                    Log.Error($"FROM > TO in {originalInput}");
                    return originalInput;
                }
                
                string replace = LongRandom(from, to + 1, Rand).ToString();

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
            return Regex.Replace(input, $"(<{VarNameRegex}int64_random>)", m =>
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

        private static long LongRandom(long min, long max, Random rand)
        {
            long result = rand.Next((Int32)(min >> 32), (Int32)(max >> 32));
            result = (result << 32);
            result = result | (long)rand.Next((Int32)min, (Int32)max);
            return result;
        }
    }
}
