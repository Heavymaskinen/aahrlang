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
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("hej", program.GetData("0"));
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
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("hej", program.GetFunction(666)(null));
        }

        [Test]
        public void UtilTest()
        {
            var str = "[GREET]('hello') ";
            Assert.AreEqual("GREET",Utils.GetInner(str, '[', ']'));
            Assert.AreEqual("'hello'",Utils.GetInner(str, '(', ')'));
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
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("hello", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleDeepParameterScopes()
        {
            var code = "def GREET 1\n" +
                       "[\n" +
                       "GREET => (b) {\n" +
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
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }
        
        [Test]
        public void HandleMinusExpression()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "3 - 2\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("1", program.GetFunction(666)(null));
        }
        
        [Test]
        public void HandleModuloExpression()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "2 % 2\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("0", program.GetFunction(666)(null));
        }
        
        [Test]
        public void HandleDivisionExpression()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "2 / 2\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("1", program.GetFunction(666)(null));
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
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleParameterSumExpression()
        {
            var code = "[\n" +
                       "1 => (a,b) {\n" +
                       "$a + $b\n" +
                       "}\n" +
                       "666 => () {\n" +
                       "[1](1,1)\n" +
                       "}\n";

            var parser = new Parser();
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
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
            var program = parser.ScanAndParse(code);
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
            Assert.Throws<Exception>(() => parser.ScanAndParse(code));
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
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("1", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleSingleComment()
        {
            var code = "[\n" +
                       "MAIN => () {\n" +
                       "42\n" +
                       Parser.CommentToken + "[here][0] = 1\n" +
                       "}\n" +
                       "]\n";

            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("42", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleForLoop()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 0\n" +
                       "[here][1] = 0\n" +
                       "for ([here][0] < 10; [here][0] = [here][0]+1)\n" +
                       "[here][1] = [here][0]" +
                       Environment.NewLine+Environment.NewLine+
                       "}\n" +
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("9", program.GetFunction(666)(null));
        }
        
        [Test]
        public void HandleCodeAfterForLoop()
        {
            var code = "[\n" +
                       "666 => () {\n" +
                       "[here][0] = 0\n" +
                       "[here][1] = 0\n" +
                       "for ([here][0] < 10; [here][0] = [here][0]+1)\n" +
                       "[here][1] = [here][0]" +
                       Environment.NewLine+Environment.NewLine+
                       "2\n"+
                       "}\n" +
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("2", program.GetFunction(666)(null));
        }

        [Test]
        public void TestRun()
        {
            var args = new[] { "2", "2" };
            new ArrhInterpreter().RunFile("second.arrh", args);
        }

        [Test]
        public void ScanEntry()
        {
            var scanner = new Scanner();
            var scanned = scanner.Parse("[\n"
                          +"0 => 2\n"
                          +"]\n");
            Assert.AreEqual(1, scanned.entries.Count);
            Assert.AreEqual("2", scanned.DataValue(0).Value);
            Assert.AreEqual("0", scanned.entries[0].Index);
        }

        [Test]
        public void ScanFunction()
        {
            var scanner = new Scanner();
            scanner.Parse("[\n"
                          + "[0] => () {\n"
                          + "[1] = 2 +2\n"
                          + "}\n"
                          + "]\n");
        }

        [Test]
        public void HandleEntryArrays()
        {
            var code = "[\n" +
                       "0 => [1,2,3]"+
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual(new []{"1","2","3"}, program.GetArrayData(0));
        }
        
        [Test]
        public void AccessEntryArray()
        {
            var code = "[\n" +
                       "0 => [1,2,3]\n"+
                       "MAIN => {\n"+
                       "[0][0]\n"+
                       "}\n"+
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("1", program.GetFunction(666)(null));
        }
        
        [Test]
        public void AddToEntryArray()
        {
            var code = "[\n" +
                       "0 => [1,2,3]\n"+
                       "MAIN => {\n"+
                       "[0][] = 4\n"+
                       "[0][3]\n"+
                       "}\n"+
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("4", program.GetFunction(666)(null));
        }
        
        [Test]
        public void GetArraySize()
        {
            var code = "[\n" +
                       "0 => [1,2,3,5]\n"+
                       "MAIN => {\n"+
                       "[0][] = 3\n"+
                       "size(&[0])\n"+
                       "}\n"+
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("5", program.GetFunction(666)(null));
        }

        [Test]
        public void HandleObject()
        {
            var code = "[\n" +
                       "0 => [\n"+
                       "0 => 'hej'\n"+
                       "]\n"+
                       "MAIN => {\n"+
                       "[0][0]\n"+
                       "}\n"+
                       "]\n";
            
            var parser = new Parser();
            var program = parser.ScanAndParse(code);
            Assert.AreEqual("hej", program.GetFunction(666)(null));
        }
    }
}