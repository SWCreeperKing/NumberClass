using System;

namespace NumberClass
{
    class Program
    {
        static void Main(string[] args)
        {
            NumberClass.format = NumberClass.Format.Scientific;
            
            NumberClass v1 = 10000;
            NumberClass v2 = "1e50";
            NumberClass v3 = 1;

            Console.WriteLine(new NumberClass(1, 0));

            Console.WriteLine($"{v3} {v3++} {v3}");

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

            Console.WriteLine($"{new NumberClass(16).Sqrt()}");
            Console.WriteLine($"{new NumberClass(160).Sqrt()}");
            Console.WriteLine($"{new NumberClass(1600).Sqrt()}");
            Console.WriteLine($"{new NumberClass(16000).Sqrt()}");

            Console.WriteLine($"{new NumberClass("1e30")}");
            Console.WriteLine($"{new NumberClass("1ee30")}");
            Console.WriteLine($"{new NumberClass("1ee308")}");
            Console.WriteLine($"{new NumberClass("1e30") + "6e29"}");

            Console.WriteLine(new NumberClass(3).Pow(4));
            Console.WriteLine(new NumberClass(3).Pow(4).Root(4));

            NumberClass b = 2;
            NumberClass.format = NumberClass.Format.Engineering;
            while (true)
            {
                Console.WriteLine(b = b.Pow(2));
                Console.ReadKey();
            }
            
            Console.ReadLine();
        }
    }
}