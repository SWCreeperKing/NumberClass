using System;

namespace NumberClass
{
    class Program
    {
        static void Main(string[] args)
        {
            NumberClass v1 = 10000;
            NumberClass v2 = "1e50";
            
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
            
            Console.WriteLine($"{new NumberClass(16).Sqrt()}");
            Console.WriteLine($"{new NumberClass(160).Sqrt()}");
            Console.WriteLine($"{new NumberClass(1600).Sqrt()}");
            Console.WriteLine($"{new NumberClass(16000).Sqrt()}");

            Console.ReadLine();
        }
    }
}