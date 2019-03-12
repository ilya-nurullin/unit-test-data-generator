using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    public class CharsCompiler: AbstractCompiler
    {
        private static readonly Random Rand = RandomProvider.Rand;
        private static readonly ILog Log = LogManager.GetLogger(typeof(CharsCompiler));

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return Regex.Replace(input, $@"<{VarNameRegex}chars_(?<length>\d+): ?\[(?<chars>.+)\]>", m =>
            {
                Group group = m.Groups[1];
                int length = int.Parse(m.Groups["length"].Value);
                string chars = m.Groups["chars"].Value;
                string originalInput = @group.Value;

                string replace = RandomProvider.RandomStringFrom(length, chars);

                CreateLuaVariable(m.Groups["var_name"], Log, replace);

                Log.Info($"Replacing {originalInput} with {replace}");
                return replace;
            });
        }
    }
}