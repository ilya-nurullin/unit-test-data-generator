using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoUnitTestingCore;
using AutoUnitTestingCore.Compilers;
using NUnit.Framework;

namespace NUnitTestProject
{
    public class CompilerTests
    {
        [Test]
        public void SingleRandomString()
        {
            Assert.AreEqual(@"a'bc\s", Compiler.Compile(new StringInputProvider(@"<srand('a\'bc\\s')>")));
        }

        [Test]
        public void RandomParamsString()
        {
            List<string> strings = new List<string>();
            for (int i = 0; i < RandomProvider.Rand.Next(3, 17); i++)
            {
                strings.Add(RandomProvider.RandomString(RandomProvider.Rand.Next(3, 33)));
            }

            string actual =
                Compiler.Compile(
                    new StringInputProvider($@"<srand({string.Join(", ", strings.Select(s => $"'{s}'"))})>"));
            Assert.IsTrue(strings.Contains(actual), $"{actual} is not in expected strings");
        }

        [Test]
        public void LuaSimpleReturn()
        {
            Assert.AreEqual(@"hello world", Compiler.Compile(new StringInputProvider(@"<lua(return 'hello world')>")));
        }

        [Test]
        public void CharsCompiler()
        {
            for (int i = 0; i < 5; i++)
            {
                string chars = RandomProvider.RandomStringFrom(RandomProvider.Rand.Next(3, 33),
                    @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789=/+*!@#$%^&()|");
                int length = RandomProvider.Rand.Next(3, 33);
                string compiled = Compiler.Compile(new StringInputProvider($@"<$chrs:chars_{length}:[{chars}]>"));
                Assert.IsTrue(Regex.IsMatch(
                    compiled,
                    @"^[" + chars + "}]{" + length + "}$"), $"Compiled: {compiled}");
            }
        }

        [Test]
        public void NoPrintCompilerTest()
        {
            string input = @"<noprint 
<$myint: 154..154>
<$mydouble: 0.1..0.1>
<$mychar: chars_3: [a]>
<$mystr: srand('wow')>
/>
<=$myint>
<=$mydouble>
<=$mychar>
<=$mystr>";
            string expected = @"154
0.1
aaa
wow";
            var res = Compiler.Compile(new StringInputProvider(input));
            Assert.AreEqual(expected, res);
        }

        [Test]
        public void IntMatrixCompilerTest()
        {
            string input = @"<int_matrix(7..7): 2x3>";
            string expected = @"7 7
7 7
7 7";
            var res = Compiler.Compile(new StringInputProvider(input));
            Assert.AreEqual(expected, res);

            input = @"<int_matrix: 5x1>";
            expected = @"-?\d+ -?\d+ -?\d+ -?\d+ -?\d+";
            res = Compiler.Compile(new StringInputProvider(input));
            Assert.IsTrue(Regex.IsMatch(res, expected), $"Regex is failed. Compiled: {res}");
        }

        [Test]
        public void IntMatrixWithVarsCompilerTest()
        {
            string input = @"<noprint <$width: 2..2> <$height: 3..3> /><int_matrix(7..7): $width x $height>";
            string expected = @"7 7
7 7
7 7";
            var res = Compiler.Compile(new StringInputProvider(input));
            Assert.AreEqual(expected, res);

            input = @"<int_matrix: 5x1>";
            expected = @"-?\d+ -?\d+ -?\d+ -?\d+ -?\d+";
            res = Compiler.Compile(new StringInputProvider(input));
            Assert.IsTrue(Regex.IsMatch(res, expected), $"Regex is failed. Compiled: {res}");
        }

        [Test]
        public void DoubleMatrixCompilerTest()
        {
            string input = @"<double_matrix(8.0..8.0#0.0): 3x2>";
            string expected = @"8.0 8.0 8.0
8.0 8.0 8.0";
            var res = Compiler.Compile(new StringInputProvider(input));
            Assert.AreEqual(expected, res);

            input = @"<noprint <$width: 2..2> <$height: 3..3> /><double_matrix(13.0..13.0#0.0): $width x $height>";
            expected = @"13.0 13.0
13.0 13.0
13.0 13.0";
            res = Compiler.Compile(new StringInputProvider(input));
            Assert.IsTrue(Regex.IsMatch(res, expected), $"Regex is failed. Compiled: {res}");

            input = @"<double_matrix: 5x1>";
            expected = @"-?\d+\.\d+ -?\d+\.\d+ -?\d+\.\d+ -?\d+\.\d+ -?\d+\.\d+";
            res = Compiler.Compile(new StringInputProvider(input));
            Assert.IsTrue(Regex.IsMatch(res, expected), $"Regex is failed. Compiled: {res}");

            input = @"<double_matrix(#0.000): 4x1>";
            expected = @"-?\d+\.\d{3} -?\d+\.\d{3} -?\d+\.\d{3} -?\d+\.\d{3}";
            res = Compiler.Compile(new StringInputProvider(input));
            Assert.IsTrue(Regex.IsMatch(res, expected), $"Regex is failed. Compiled: {res}");
        }
    }
}