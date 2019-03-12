using System.Text.RegularExpressions;
using AutoUnitTestingCore;
using NUnit.Framework;

namespace NUnitTestProject
{
    public class IntegerCompilerTests
    {
        [Test]
        public void OneFromToPositiveIntRandom()
        {
            Assert.AreEqual("3", Compiler.Compile(new StringInputProvider("<3..3>")));
        }

        [Test]
        public void OneFromToNegativeIntRandom()
        {
            Assert.AreEqual("-13", Compiler.Compile(new StringInputProvider("<-13..-13>")));
        }

        [Test]
        public void OneIntRandom()
        {
            Assert.IsTrue(Regex.IsMatch(Compiler.Compile(new StringInputProvider("<int_random>")), @"^-?\d+$"));
        }

        [Test]
        public void IntegerRandomWithVariable()
        {
            Assert.AreEqual(@"-15 -15", Compiler.Compile(new StringInputProvider(@"<$myVar:-15..-15> <lua(return myVar)>")));
        }

        [Test]
        public void WrongIntRandom()
        {
            Assert.AreEqual("<15..1>", Compiler.Compile(new StringInputProvider("<15..1>")));
        }
    }
}