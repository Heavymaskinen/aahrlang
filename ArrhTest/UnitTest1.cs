using ArrhLang;
using NUnit.Framework;

namespace ArrhTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ParseDataStructure()
        {
            var code = "[\n" +
                       "0 => 'hej'\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("hej", program.GetData(0));
        }

        [Test]
        public void ParseFunctionReturningString()
        {
            var code = "[\n" +
                       "0 => 'hej'\n" +
                       "1 => () {\n" +
                       "[0]\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("hej", program.GetFunction(1)(null));
        }

        [Test]
        public void ParseLocalVariable()
        {
            var code = "[\n" +
                       "0 => 'hej'\n" +
                       "1 => () {\n" +
                       "[here][0] = 10\n" +
                       "[0]\n" +
                       "[here][0]\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("10", program.GetFunction(1)(null));
        }

        [Test]
        public void ParseMainFunction()
        {
            var code = "[\n" +
                       "0 => 'hej'\n" +
                       "1 => () {\n" +
                       "[0]\n" +
                       "}\n" +
                       "666 => () {\n" +
                       "[1]()\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("hej", program.GetFunction(666)(null));
        }

        [Test]
        public void DefineConstants()
        {
            var code = "def GREET 1\n" +
                "[\n" +
                       "0 => 'hej'\n" +
                       "GREET => () {\n" +
                       "[0]\n" +
                       "}\n" +
                       "666 => () {\n" +
                       "[GREET]()\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("hej", program.GetFunction(666)(null));
        }

        [Test]
        public void UseMethodParameters()
        {
            var code = "def GREET 1\n" +
                       "[\n" +
                       "GREET => (a) {\n" +
                       "$a\n" +
                       "}\n" +
                       "666 => () {\n" +
                       "[GREET]('hello')\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("hello", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleDeepParameterScopes()
        {
            var code = "def GREET 1\n" +
                       "[\n" +
                       "GREET => (a) {\n" +
                       "[2]('hi')\n" +
                       "}\n" +
                       "2 => (a) {\n" +
                       "$a\n" +
                       "}\n" +
                       "666 => () {\n" +
                       "[GREET]('hello')\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("hi", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleSumExpression()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "1 + 1\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleParameterSumExpression()
        {
            var code = "[\n" +
                       "1 => (a,b) {\n" +
                       "$a + $b\n" +
                       "}\n" +
                       "MAIN => () {\n" +
                       "[1](1,1)\n" +
                       "}\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleStringSumExpression()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "'1' ^ '1'\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("11", program.GetFunction(666)(null));
        }

        [Test]
        public void TestRun()
        {
            new ArrhInterpreter().RunFile("second.arrh");
        }


    }

}