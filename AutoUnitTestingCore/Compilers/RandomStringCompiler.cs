using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    public class RandomStringCompiler: AbstractCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RandomStringCompiler));

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return Regex.Replace(input, $@"<{VarNameRegex}srand\(('.+')\)>", m =>
            {
                var matches = Regex.Matches(m.Value, @"'(\\.|[^\'])*'", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                List<String> stringsToRandom = new List<string>();
                for (var i = 0; i < matches.Count; i++)
                {
                    string match = matches[i].Value;
                    match = match.Substring(1); // remove first quote
                    match = match.Substring(0, match.Length - 1); // remove last quote
                    stringsToRandom.Add(ReplaceEscapes(match));
                }
                string replace = stringsToRandom[RandomProvider.Rand.Next(stringsToRandom.Count)];

                CreateLuaVariable(m.Groups["var_name"], Log, replace);

                return replace;
            });
        }

        private string ReplaceEscapes(string input)
        {
            return input.Replace(@"\\", @"\").Replace(@"\'", @"'");
        }
    }
}