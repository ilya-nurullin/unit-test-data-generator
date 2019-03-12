using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AutoUnitTestingCore.Compilers
{
    public class LuaCompiler: AbstractCompiler
    {
        public static readonly dynamic Lua = new DynamicLua.DynamicLua();

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            Lua("math.randomseed(os.time())");
            return Regex.Replace(input, @"<lua\((.+)\)>", m =>
                {
                    return Lua(m.Groups[1].Value).ToString();
                }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
    }
}