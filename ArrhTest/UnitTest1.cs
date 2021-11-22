using ArrhLang;
using NUnit.Framework;
using System;

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
        public void HandleLocalEntrySumExpression()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 1\n"+
                       "[here][0] + 1\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }
        
        [Test]
        public void HandleLocalEntrySelfIncrement()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 1\n"+
                       "[here][1] = 1\n"+
                       "[here][0] = [here][1] + 1\n"+
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }
        
        [Test]
        public void HandleEntrySelfIncrement()
        {
            var code = "[\n" +
                       "1 => 1\n"+
                       "666 => () {\n" +
                       "[1] = [1] + 1\n"+
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
        public void HandleStringConcatExpression()
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
        public void HandleIfStatement()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 1\n" +
                       "if [here][0] < 2\n" +
                       "4" + Environment.NewLine + Environment.NewLine +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("4", program.GetFunction(666)(null));
        }

        [Test]
        public void TooManyNewLines_WhitespaceOverflow()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 1\n" +
                       "if [here][0] < 2\n" +
                       "4" + Environment.NewLine + Environment.NewLine +
                       "out('Hi')" +
                       Environment.NewLine + Environment.NewLine +
                       Environment.NewLine + Environment.NewLine +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            Assert.Throws<Exception>(() => parser.ParseIt(code));
        }

        [Test]
        public void HandleBlockComment()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 1\n" +
                       Parser.BlockCommentToken +
                       "if [here][0] < 2\n" +
                       "4" + Environment.NewLine + Environment.NewLine +
                       "out('Hi')" +
                       Environment.NewLine + Environment.NewLine +
                       Parser.BlockCommentToken + "\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("1", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleSingleComment()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "42\n" +
                       Parser.CommentToken + "[here][0] = 1\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("42", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleForLoop()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 0\n" +
                       "[here][1] = 0\n" +
                       "for ([here][0] < 10; [here][0] = [here][0]+1) { \n" +
                       "[here][1] = [here][0]" +
                       Environment.NewLine+Environment.NewLine+
                       "}\n" +
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("9", program.GetFunction(666)(null));
        }
        
        [Test]
        public void HandleCodeAfterForLoop()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 0\n" +
                       "[here][1] = 0\n" +
                       "for ([here][0] < 10; [here][0] = [here][0]+1) { \n" +
                       "[here][1] = [here][0]" +
                       Environment.NewLine+Environment.NewLine+
                       "2\n"+
                       "}\n" +
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ParseIt(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }

        [Test]
        public void TestRun()
        {
            var args = new[] { "2", "2" };
            new ArrhInterpreter().RunFile("second.arrh", args);
        }
    }
}