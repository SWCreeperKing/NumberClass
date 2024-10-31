using System;
using System.Runtime.InteropServices;
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
        public NumberClass n5;
        public NumberClass n6;

        [SetUp]
        public void Setup()
        {
            n1 = 10000;
            n2 = "1e50";
            n3 = new NumberClass(1, 1e99);
            n4 = new NumberClass(1, 101, 1);
            n5 = new NumberClass(1, 2, 3).Log10();
            n6 = new NumberClass(1, 2, 2);
        }

        [Test]
        public void NullTest()
        {
            NumberClass? nc1 = 6;
            NumberClass? nc2 = null;
            Assert.That(nc1, Is.Not.Null);
            Assert.That(nc2, Is.Null);
        }

        [Test]
        public void PlusPlus()
        {
            NumberClass n = 1;
            Assert.That(n == 1, Is.True);
            n++;
            Assert.That(n == 2, Is.True);
            Assert.That(++n == 3, Is.True);
        }

        [Test]
        public void LogTest()
        {
            Assert.That(n1.Log10() == 4, Is.True);
            Assert.That(n2.Log10() == 50, Is.True);
            Assert.That(n2.Log(n1) == 12.5, Is.True);
            Console.WriteLine(n2.Log().GetRealMantissa());
            Assert.That(Math.Abs(n2.Log().GetRealMantissa() - 115.12925464970229f) < .00001, Is.True);
        }

        [Test]
        public void AddTest()
        {
            Assert.That(n1 + n2 == new NumberClass(1, 50), Is.True);
            var prePreAdd = new NumberClass("6e29");
            var preAdd = new NumberClass("1e30") + prePreAdd;
            var addRes = new NumberClass(1.6f, 30);
            Assert.That(preAdd == addRes, Is.True);
        }

        [Test]
        public void SubTest()
        {
            var sub1 = n1 - n2;
            var sub1Res = new NumberClass(-1, 50);
            var sub2 = n2 - n1;
            var sub2Res = new NumberClass(1, 50);
            Assert.That(sub1 == sub1Res, Is.True);
            Assert.That(sub2, Is.EqualTo(sub2Res));
            Assert.That(NumberClass.One / 2 == .5f, Is.True);

            var sub3n1 = new NumberClass(2.2561784573198107, 6);
            var sub3n2 = new NumberClass(1.9603208295529388, 7);
            var sub3 = sub3n1 - sub3n2;
            var sub3Res = new NumberClass(-1.73470298382095773, 7);
            Console.WriteLine(sub3.ToExactString());
            Console.WriteLine(sub3Res.ToExactString());
            Assert.That(sub3 == sub3Res, Is.True);
        }

        [Test]
        public void MultiTest()
        {
            Assert.That(n1 * n2 == new NumberClass(1, 54), Is.True);
        }

        [Test]
        public void DivTest()
        {
            Assert.That(n1 / n2 == new NumberClass(1, -46), Is.True);
            Assert.That(n2 / n1 == new NumberClass(1, 46), Is.True);
        }

        [Test]
        public void PowerTest()
        {
            // Assert.That(new NumberClass(2).Pow(2) == 4, Is.True);
            // Assert.That(new NumberClass(4).Pow(4) == 256, Is.True);
            Console.WriteLine(n1.Pow(n2).ToExactString());
            Assert.That(n1.Pow(n2) == new NumberClass(1, 4e50), Is.True);
            Assert.That(n2.Pow(n1) == new NumberClass(1, 50e4), Is.True);
        }

        [Test]
        public void CompareTest()
        {
            Assert.That(n1 > n2, Is.False);
            Assert.That(n1 < n2, Is.True);
            Assert.That(n1 != n2, Is.True);
            Assert.That(n1 == n2, Is.False);
        }

        [Test]
        public void RootTest()
        {
            Assert.That(new NumberClass(3).Pow(4).Root(4) == 3, Is.True);
        }

        [Test]
        public void Serialize()
        {
            var jsonString = JsonConvert.SerializeObject(n2);
            var n2Converted = JsonConvert.DeserializeObject<NumberClass>(jsonString);
            Console.WriteLine(n2);
            Console.WriteLine(jsonString);
            Console.WriteLine(n2Converted);
            Assert.That(n2Converted == n2, Is.True);
        }

        [Test]
        public void MagnitudeTest()
        {
            Assert.That(n1.Magnitude == 0, Is.True);
            Assert.That(n2.Magnitude == 0, Is.True);
            Assert.That(n3.Magnitude == 0, Is.True);
            Assert.That(n4.Magnitude == 1, Is.True);
        }

        [Test]
        public void MagnitudeAdd()
        {
            Assert.That((n3 + n3).ToExactString() == "2_1e99_0", Is.True);
            Assert.That((n3 + n4).ToExactString() == "1.01_101_1", Is.True);
        }

        [Test]
        public void MagnitudeSubtract()
        {
            Assert.That((n3 - n3).ToExactString() == "0_0_0", Is.True);
            Assert.That((n3 - n4).ToExactString() == "-9.9_100_1", Is.True);
            Assert.That((n4 - n3).ToExactString() == "9.9_100_1", Is.True);
        }

        [Test]
        public void MagnitudeMultiply()
        {
            Assert.That((n3 * n4).ToRawString() == "1_200_1", Is.True);
        }

        [Test]
        public void MagnitudeDivide()
        {
            Assert.That((n3 / n4).ToRawString() == "1_-2_0", Is.True);
        }

        [Test]
        public void MagnitudePower()
        {
            Console.WriteLine(n3.Pow(n4).ToRawString());
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