using System;
using System.Globalization;
using System.Text.RegularExpressions;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    public class ShowLuaVarCompiler: AbstractCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ShowLuaVarCompiler));

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return Regex.Replace(input, @"<=(\$[A-Za-z]\w*)>", m =>
            {
                Group group = m.Groups[1];

                var fullVarName = @group.Value;
                var varValue = GetLuaVarValue(fullVarName);
                string replace = varValue;

                if (varValue == null)
                {
                    Log.Fatal($"Variable {fullVarName} is not defined in Lua.");
                    Environment.Exit(ErrorCodes.LuaVariableNotExists);
                }
                return replace;
            });
        }

        public static dynamic GetLuaVarValue(string fullVarName)
        {
            string varName = fullVarName.Replace("$", "");

            return LuaCompiler.Lua[varName];
        }

    }
}