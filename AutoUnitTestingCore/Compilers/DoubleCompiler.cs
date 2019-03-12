using System;
using System.Globalization;
using System.Text.RegularExpressions;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    public class DoubleCompiler : AbstractCompiler
    {
        private static readonly Random Rand = RandomProvider.Rand;
        private static readonly ILog Log = LogManager.GetLogger(typeof(DoubleCompiler));

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return DoubleRandom(DoubleFromToRandom(input, cultureInfo), cultureInfo);
        }


        private static string DoubleRandom(string input, CultureInfo cultureInfo)
        {
            // replace <double_random> to rand.NextDouble().
            return Regex.Replace(input, $"(<{VarNameRegex}double_random{DoublePrecisionRegex}?>)", m =>
            {
                Group group = m.Groups[1];
                string originalInput = @group.Value;
                string format = m.Groups["format"].Value;
                string replace = format.Length > 0 
                    ? Rand.NextDouble().ToString(format, cultureInfo) 
                    : Rand.NextDouble().ToString(cultureInfo);


                CreateLuaVariable(m.Groups["var_name"], Log, replace);

                Log.Info($"Replacing {originalInput} with {replace}");
                return string.Format("{0}{1}{2}",
                    m.Value.Substring(0, @group.Index - m.Index),
                    replace,
                    m.Value.Substring(@group.Index - m.Index + @group.Length));
            });
        }

        private static string DoubleFromToRandom(string input, CultureInfo cultureInfo)
        {
            // replace <from..to> from&to - inclusive to rand(from, to+1). to+1 for include to.
            return Regex.Replace(input, $"(<{VarNameRegex}{DoubleIntervalRegex}{DoublePrecisionRegex}?>)", m =>
            {
                Group group = m.Groups[1];
                double from = double.Parse(m.Groups["from"].Value, NumberStyles.Any, CultureInfo.InvariantCulture);
                double to = double.Parse(m.Groups["to"].Value, NumberStyles.Any, CultureInfo.InvariantCulture);
                string originalInput = @group.Value;
                if (from > to)
                {
                    Log.Error($"FROM > TO in {originalInput}");
                    return originalInput;
                }

                string format = m.Groups["format"].Value;

                string replace = format.Length > 0
                    ? RandomDouble(from, to).ToString(format, cultureInfo)
                    : RandomDouble(from, to).ToString(cultureInfo);

                CreateLuaVariable(m.Groups["var_name"], Log, replace);

                Log.Info($"Replacing {originalInput} with {replace}");
                return string.Format("{0}{1}{2}",
                    m.Value.Substring(0, @group.Index - m.Index),
                    replace,
                    m.Value.Substring(@group.Index - m.Index + @group.Length));
            });
        }

        public static double RandomDouble(double from, double to)
        {
            return Rand.NextDouble() * (to - from) + from;
        }
    }
}