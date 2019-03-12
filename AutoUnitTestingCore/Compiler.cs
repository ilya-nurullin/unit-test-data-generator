using System;
using System.Globalization;
using System.Linq;
using AutoUnitTestingCore.Compilers;
using log4net;

namespace AutoUnitTestingCore
{
    public class Compiler
    {
        private static readonly Random Rand = new Random();
        private static readonly ILog Log = LogManager.GetLogger(typeof(Compiler));

        // !!! Make sure that NoPrintCompiler is the latest compiler in the queue !!!
        private static readonly ICompiler[] AllCompilers =
        {
            new DoubleCompiler(), new IntegerCompiler(),
            new Int64Compiler(), new CharsCompiler(),
            new RandomStringCompiler(), new LuaCompiler(),
            new MatrixCompiler(), new ShowLuaVarCompiler(),

            new NoPrintCompiler(), // !!! Make sure that NoPrintCompiler is the latest compiler in the queue !!!
        };
        protected static CultureInfo cultureInfo;

        public static string Compile(IInputProvider inputProvider, CultureInfo cultureInfo = null,
            ICompiler[] compilers = null)
        {
            if (cultureInfo == null)
                cultureInfo = CultureInfo.InvariantCulture;

            Compiler.cultureInfo = cultureInfo;
            compilers = compilers ?? AllCompilers;
            return compilers.Aggregate(inputProvider.GetInput(), (input, compiler) => compiler.Compile(input, cultureInfo));
        }
    }
}