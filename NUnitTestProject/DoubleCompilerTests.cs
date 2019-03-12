using System.Globalization;
using System.Text.RegularExpressions;
using AutoUnitTestingCore;
using NUnit.Framework;

namespace NUnitTestProject
{
    public class DoubleCompilerTests
    {
        [Test]
        public void OneFromToNegativeDoubleRandom()
        {
            Assert.AreEqual("-40.5", Compiler.Compile(new StringInputProvider("<-40.5..-40.5>")));
        }
        [Test]
        public void RuCultureRandom()
        {
            Assert.AreEqual("-40,5", Compiler.Compile(new StringInputProvider("<-40.5..-40.5>"), CultureInfo.GetCultureInfo("ru")));
        }

        [Test]
        public void OneFromToNegativeDoubleWithFormatRandom()
        {
            Assert.AreEqual("-13.54", Compiler.Compile(new StringInputProvider("<-13.543..-13.543###.##>")));
        }

        [Test]
        public void OneFromToPositiveDoubleRandom()
        {
            Assert.AreEqual("150.5 150.5", Compiler.Compile(new StringInputProvider("<$a:150.5..150.5> <lua(return a)>")));
        }

        [Test]
        public void WrongDoubleRandom()
        {
            Assert.AreEqual("<1.0..0.0>", Compiler.Compile(new StringInputProvider("<1.0..0.0>")));
        }

        [Test]
        public void OneDoubleRandom()
        {
            Assert.IsTrue(Regex.IsMatch(Compiler.Compile(new StringInputProvider("<double_random>")), @"^-?\d+\.\d+$"));
        }

        [Test]
        public void OneDoubleWithFormatRandom()
        { 
            Assert.IsTrue(Regex.IsMatch(Compiler.Compile(new StringInputProvider("<double_random#0.00>")), @"^-?\d\.\d\d$"));
        }

        [Test]
        public void LuaDoubleRandom()
        {
            Assert.IsTrue(Regex.IsMatch(Compiler.Compile(new StringInputProvider("<$s_:double_random> <lua(return s_)>")), @"^-?\d+\.\d+ -?\d+\.\d+$"));
        }
    }
}