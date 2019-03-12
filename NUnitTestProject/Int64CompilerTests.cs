using System.Text.RegularExpressions;
using AutoUnitTestingCore;
using NUnit.Framework;

namespace NUnitTestProject
{
    public class Int64CompilerTests
    {
        [Test]
        public void OneFromToPositiveIntRandom()
        {
            Assert.AreEqual("3", Compiler.Compile(new StringInputProvider("<int64, 3..3>")));
        }

        [Test]
        public void OneFromToNegativeIntRandom()
        {
            Assert.AreEqual("-13", Compiler.Compile(new StringInputProvider("<int64, -13..-13>")));
        }

        [Test]
        public void OneIntRandom()
        {
            Assert.IsTrue(Regex.IsMatch(Compiler.Compile(new StringInputProvider("<int64_random>")), @"^-?\d+$"));
        }

        [Test]
        public void IntegerRandomWithVariable()
        {
            Assert.AreEqual(@"-15 -15", Compiler.Compile(new StringInputProvider(@"<int64, $myVar:-15..-15> <lua(return myVar)>")));
        }

        [Test]
        public void WrongIntRandom()
        {
            Assert.AreEqual("<int64, 15..1>", Compiler.Compile(new StringInputProvider("<int64, 15..1>")));
        }
    }
}