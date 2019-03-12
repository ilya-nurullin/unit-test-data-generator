using System;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using log4net;

namespace AutoUnitTestingCore.Compilers
{
    public class MatrixCompiler: AbstractCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatrixCompiler));

        public override string Compile(string input, CultureInfo cultureInfo)
        {
            return DoubleMatrix(IntMatrix(input), cultureInfo);
        }

        private static string IntMatrix(string input)
        {
            return Regex.Replace(input, $@"<int_matrix(\({IntIntervalRegex}\))?: ?((?<width>\d+)|(?<width_var_name>\$[A-Za-z]\w*)) ?x ?((?<height>\d+)|(?<height_var_name>\$[A-Za-z]\w*))>", m =>
            {
                Group group = m.Groups[1];
                int width = 0;
                if (m.Groups["width"].Success)
                {
                    width = int.Parse(m.Groups["width"].Value);
                }
                else if (m.Groups["width_var_name"].Success)
                {
                    width = int.Parse(ShowLuaVarCompiler.GetLuaVarValue(m.Groups["width_var_name"].Value));
                }

                int height = 0;
                if (m.Groups["height"].Success)
                {
                    height = int.Parse(m.Groups["height"].Value);
                }
                else if (m.Groups["height_var_name"].Success)
                {
                    height = int.Parse(ShowLuaVarCompiler.GetLuaVarValue(m.Groups["height_var_name"].Value));
                }

                int from = int.MinValue;
                if (m.Groups["from"].Success)
                {
                    from = int.Parse(m.Groups["from"].Value);
                }

                int to = int.MaxValue;
                if (m.Groups["to"].Success)
                {
                    to = int.Parse(m.Groups["to"].Value) + 1;
                }

                if (from > to)
                {
                    Log.Error($"FROM > TO in {input}");
                    return input;
                }

                string replace = "";
                int[] intsRow = new int[width];
                for (int i = 0; i < height; i++)
                {
                    intsRow = intsRow.Select(x => RandomProvider.Rand.Next(from, to)).ToArray();
                    replace += string.Join(" ", intsRow) + "\r\n";
                }

                return replace.Trim(' ', '\n', '\r');
            });
        }

        private static string DoubleMatrix(string input, CultureInfo cultureInfo)
        {
            return Regex.Replace(input, $@"<double_matrix(\(({DoubleIntervalRegex})?{DoublePrecisionRegex}?\))?: ?(?<width>\d+)x(?<height>\d+)>", m =>
            {
                Group group = m.Groups[1];
                int width = int.Parse(m.Groups["width"].Value);
                int height = int.Parse(m.Groups["height"].Value);
                double from = 0;
                if (m.Groups["from"].Success)
                {
                    from = double.Parse(m.Groups["from"].Value, NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                double to = 1;
                if (m.Groups["to"].Success)
                {
                    to = double.Parse(m.Groups["to"].Value, NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                if (from > to)
                {
                    Log.Error($"FROM > TO in {input}");
                    return input;
                }

                Func<double> randFunc;
                if (m.Groups["from"].Success && m.Groups["to"].Success)
                {
                    randFunc = () => DoubleCompiler.RandomDouble(from, to);
                }
                else
                {
                    randFunc = () => RandomProvider.Rand.NextDouble();
                }

                string format = m.Groups["format"].Value;

                string replace = "";
                string[] doublesRow = new string[width];
                for (int i = 0; i < height; i++)
                {
                    doublesRow = doublesRow.Select(x => (format.Length > 0) 
                        ? randFunc().ToString(format, cultureInfo)
                        : randFunc().ToString(cultureInfo)
                        ).ToArray();
                    replace += string.Join(" ", doublesRow) + "\r\n";
                }

                return replace.Trim(' ', '\n', '\r');
            });
        }
    }
}