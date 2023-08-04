using System;
using System.Numerics;
using Newtonsoft.Json;
using NUnit.Framework;

public class Program
{
    [TestFixture]
    public class TestClass
    {
        public NumberClass n1;
        public NumberClass n2;
        public NumberClass n3;
        public NumberClass n4;

        [SetUp]
        public void Setup()
        {
            n1 = 10000;
            n2 = "1e50";
            n3 = new NumberClass(1, 1e100);
            n4 = new NumberClass(1, 101, 1);
        }

        [Test]
        public void NullTest()
        {
            NumberClass? nc1 = 6;
            NumberClass? nc2 = null;
            Assert.IsNotNull(nc1);
            Assert.IsNull(nc2);
        }

        [Test]
        public void PlusPlus()
        {
            NumberClass n = 1;
            Assert.True(n == 1);
            n++;
            Assert.True(n == 2 && ++n == 3);
        }

        [Test]
        public void LogTest()
        {
            Assert.True(n1.Log10() == 4 && n2.Log10() == 50 && n2.Log(n1) == 12.5);
            Assert.True(n2.Log().GetRealMantissa() == 115.12925464970229);
        }

        [Test]
        public void AddTest()
        {
            Assert.True(n1 + n2 == new NumberClass(1, 50));
            var preAdd = new NumberClass("1e30") + "6e29";
            var addRes = new NumberClass(1.6f, 30);
            Assert.True(preAdd == addRes);
        }

        [Test]
        public void SubTest()
        {
            var sub1 = n1 - n2;
            var sub1Res = new NumberClass(-1, 50);
            var sub2 = n2 - n1;
            var sub2Res = new NumberClass(1, 50);
            Assert.True(sub1 == sub1Res);
            Assert.True(sub2 == sub2Res);
            Assert.True(NumberClass.One / 2 == .5f);
            var sub3n1 = new NumberClass(2.2561784573198107, 6);
            var sub3n2 = new NumberClass(1.9603208295529388, 7);
            var sub3 = sub3n1 - sub3n2;
            var sub3Res = new NumberClass(-1.73470298382095773, 7);
            Console.WriteLine(sub3.ToExactString());
            Console.WriteLine(sub3Res.ToExactString());
            Assert.True(sub3 == sub3Res);
        }

        [Test]
        public void MultiTest()
        {
            Assert.True(n1 * n2 == new NumberClass(1, 54));
        }

        [Test]
        public void DivTest()
        {
            Assert.True(n1 / n2 == new NumberClass(1, -46));
            Assert.True(n2 / n1 == new NumberClass(1, 46));
        }

        [Test]
        public void PowerTest()
        {
            Assert.True(n1.Pow(n2) == new NumberClass(1, 4e50));
            Assert.True(n2.Pow(n1) == new NumberClass(1, 50e4));
        }

        [Test]
        public void CompareTest()
        {
            Assert.False(n1 > n2);
            Assert.True(n1 < n2);
            Assert.True(n1 != n2);
            Assert.False(n1 == n2);
        }

        [Test]
        public void RootTest()
        {
            Assert.True(new NumberClass(3).Pow(4).Root(4) == 3);
        }

        [Test]
        public void Serialize()
        {
            var jsonString = JsonConvert.SerializeObject(n2);
            var n2Converted = JsonConvert.DeserializeObject<NumberClass>(jsonString);
            Console.WriteLine(n2);
            Console.WriteLine(jsonString);
            Console.WriteLine(n2Converted);
            Assert.True(n2Converted == n2);
        }

        [Test]
        public void MagnitudeTest()
        {
            Assert.True(n1.Magnitude == 0);
            Assert.True(n2.Magnitude == 0);
            Assert.True(n3.Magnitude == 1);
        }

        [Test]
        public void MagnitudeAdd()
        {
            Assert.True((n3 + n3).ToExactString() == "2_100_1");
            Assert.True((n3 + n4).ToExactString() == "1.1_101_1");
        }
        
        [Test]
        public void MagnitudeSubtract()
        {
            Assert.True((n3 - n3).ToExactString() == "0_0_0");
            Assert.True((n3 - n4).ToExactString() == "-9_100_1");
            Assert.True((n4 - n3).ToExactString() == "9_100_1");
        }

        /*
        NumberClass b = 2;

        var n = new NumberClass(1e12) - new NumberClass(9.2e11);
        Console.WriteLine(n);

        // NumberClass.formatter = Engineering.Name;
        SciNotation.beforeSciCut = 8;
        Engineering.beforeSciCut = 8;

        var convert = JsonConvert.SerializeObject(new NumberClass(5.342, 3));
        Console.WriteLine("e");
        Console.WriteLine(convert);
        Console.WriteLine(JsonConvert.DeserializeObject<NumberClass>(convert));
        Console.WriteLine("E");
         */
    }
}