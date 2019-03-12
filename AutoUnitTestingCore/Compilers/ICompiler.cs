using System;
using System.Globalization;
using System.Text.RegularExpressions;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    public interface ICompiler
    {
        string Compile(string input, CultureInfo cultureInfo);
    }

    public abstract class AbstractCompiler : ICompiler
    {
        public abstract string Compile(string input, CultureInfo cultureInfo);

        protected const string VarNameRegex = @"(?<var_name>\$[A-Za-z]\w*: ?)?";
        protected const string IntIntervalRegex = @"(?<from>-?\d+)\.\.(?<to>-?\d+)";
        protected const string DoubleIntervalRegex = @"(?<from>-?\d+\.\d+)\.\.(?<to>-?\d+\.\d+)";
        protected const string DoublePrecisionRegex = @"(#(?<format>.+))";

        protected static string GetVarNameFromRegEx(string varName)
        {
            return varName.Replace("$", "").Replace(":", "").Trim();
        }

        protected static void CheckVariable(string varName, ILog log)
        {
            if (LuaCompiler.Lua[varName] != null)
            {
                log.Fatal($"Variable {varName} is already defined in Lua.");
                Environment.Exit(ErrorCodes.LuaVariableAlreadyExists);
            }
        }

        protected static void CreateLuaVariable(Group varGroup, ILog log, string replace)
        {
            if (varGroup.Success)
            {
                string varName = varGroup.Value;
                CheckVariable(varName, log);
                LuaCompiler.Lua[GetVarNameFromRegEx(varName)] = replace;
            }
        }
    }
}