using ArrhLang;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                       "[here][0] = 10\n"+
                       "[0]\n" +
                       "[here][0]\n"+
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

        
    }

}