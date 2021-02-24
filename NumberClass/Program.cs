using System;

namespace NumberClass
{
    class Program
    {
        static void Main(string[] args)
        {
            var v1 = new NumberClass(1, 4);
            var v2 = new NumberClass("1e50");
            Console.WriteLine($"{v1.Log10()} {v2.Log10()}");
            Console.WriteLine($"Log{v1} {v2} = {v2.Log(v1)}");
            Console.WriteLine($"{v1} * {v2} = {v1 * v2}");
            Console.WriteLine($"{v1} + {v2} = {v1 + v2}");
            Console.WriteLine($"{v1} - {v2} = {v1 - v2}");
            Console.WriteLine($"{v2} / {v1} = {v2 / v1}");
            Console.WriteLine($"\"1e50\" / {v1} = {"1e50" / v1}");
            Console.WriteLine($"1 / 2 = {new NumberClass(1) / 2}");
            Console.WriteLine($"{v1} ^ {v2} = {v1.Pow(v2)}");
            Console.WriteLine($"{v1} > {v2} = {v1 > v2}");
            Console.WriteLine($"{v1} < {v2} = {v1 < v2}");
            Console.WriteLine($"{v1} != {v2} = {v1 != v2}");
            Console.WriteLine($"{v1} != {v2} = {v1 != v2}");

            Console.WriteLine($"{new NumberClass("1e30")}");
            Console.WriteLine($"{new NumberClass("1ee30")}");

            Console.ReadLine();
        }
    }
}