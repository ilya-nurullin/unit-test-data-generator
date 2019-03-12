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
    class NoPrintCompiler: AbstractCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NoPrintCompiler));

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return Regex.Replace(input, "<noprint ?\r?\n?.+/>\r?\n?", m => "", RegexOptions.Singleline);
        }
    }
}
