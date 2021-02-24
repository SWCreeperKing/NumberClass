using System;

namespace NumberClass
{
#pragma warning disable 660,661
    public class NumberClass
#pragma warning restore 660,661
    {
        // NumberClass made by SW_CreeperKing#5787
        // Special thanks to NumberEngineer#9999 (developer of Incremental Unlimited 1 & 2 and Line Maze Idle) for math help

        public static bool CutOff1E = true; // format; 1e1e30 => 1ee30 

        public static NumberClass Double = new NumberClass(double.MaxValue);
        public static NumberClass Long = new NumberClass(long.MaxValue);
        public static NumberClass Int = new NumberClass(int.MaxValue);

        public double mantissa;
        public double exponent;

        public NumberClass(double mantissa = 0, double exponent = 0)
        {
            (this.mantissa, this.exponent) = (mantissa, exponent);
            Update();
        }

        public NumberClass(NumberClass nc) : this(nc.mantissa, nc.exponent)
        {
        }

        public NumberClass(string s)
        {
            if ((s = s.ToLower().Replace("ee", "e1e")).Contains("e"))
            {
                var split = s.Split('e');
                if (split.Length == 2) (mantissa, exponent) = (double.Parse(split[0]), double.Parse(split[1]));
                else
                    (mantissa, exponent) = (double.Parse(split[0]),
                        double.Parse(split[1]) * Math.Pow(10, double.Parse(split[2])));
            }
            else mantissa = double.Parse(s);

            Update();
        }

        private void Update()
        {
            if (mantissa == 0)
            {
                exponent = 0;
                return;
            }

            var isNeg = mantissa < 0;
            if (isNeg) mantissa = Math.Abs(mantissa);
            var log = Math.Log10(mantissa);
            mantissa /= Math.Pow(10, log);
            exponent += log;
            if (isNeg) mantissa = -mantissa;
        }

        public static NumberClass operator +(NumberClass n1, NumberClass n2)
        {
            var delta = Math.Abs(n1.exponent - n2.exponent);
            return delta > 12
                ? n1.Max(n2)
                : delta == 0
                    ? new NumberClass(n1.mantissa + n2.mantissa, n1.exponent)
                    : n1 > n2
                        ? new NumberClass(n1.mantissa + n2.mantissa / Math.Pow(10, delta), n1.exponent)
                        : new NumberClass(n2.mantissa + n1.mantissa / Math.Pow(10, delta), n2.exponent);
        }

        public static NumberClass operator -(NumberClass n1, NumberClass n2) =>
            n1 + new NumberClass(-n2.mantissa, n2.exponent);

        public static NumberClass operator *(NumberClass n1, NumberClass n2) =>
            n1 == 0 || n2 == 0
                ? 0
                : n1 == 1 || n2 == 1
                    ? n1.Max(n2)
                    : new NumberClass(n1.mantissa * n2.mantissa, n1.exponent + n2.exponent);

        public static NumberClass operator /(NumberClass n1, NumberClass n2) =>
            n1 == 0
                ? 0
                : n2 == 0
                    ? throw new DivideByZeroException("NumberClass: Can not divide by 0")
                    : n2 == 1
                        ? n1
                        : new NumberClass(n1.mantissa / n2.mantissa, n1.exponent - n2.exponent);

        // probs not possible due to floating point numbers being weird
        // public static NumberClass operator %(NumberClass n1, NumberClass n2)
        // {
        //     if (n1 < n2) return n1;
        //     if (n2 >= Long || n2 >= Long) return null;
        //     var r = n1 / n2;
        //     Console.WriteLine($"r => {r.mantissa}e{r.exponent}");
        //     Console.WriteLine($"{r} - {(long) r} = {r - (long) r}");
        //     return (r - (long) r) * n2;
        // }

        public NumberClass Pow(NumberClass n1)
        {
            if (n1 == 0 || this == 1) return 1;
            if (n1 == 1) return this;
            if (n1.IsNeg()) return 1 / Pow(n1.Abs());
            return new NumberClass(Math.Pow(mantissa, n1.mantissa), exponent * n1.exponent);
        }

        // public NumberClass Root(NumberClass @base) => Pow(1 / @base); // lazy root sqrt(16) = (16)^(1/2) sadly doesn't work because of floats
        public NumberClass Log10() => exponent + Math.Log10(mantissa);
        public NumberClass Log(NumberClass @base) => Log10() / @base.Log10();

        public static bool operator >(NumberClass n1, NumberClass n2) =>
            n1.exponent > n2.exponent ||
            n1.exponent == n2.exponent && n1.mantissa > n2.mantissa;

        public static bool operator <(NumberClass n1, NumberClass n2) =>
            n1.exponent < n2.exponent ||
            n1.exponent == n2.exponent && n1.mantissa < n2.mantissa;

        public static bool operator ==(NumberClass n1, NumberClass n2) =>
            n1!.mantissa == n2!.mantissa && n1.exponent == n2.exponent;

        public static bool operator !=(NumberClass n1, NumberClass n2) =>
            n1!.mantissa != n2!.mantissa || n1.exponent != n2.exponent;

        public static bool operator >=(NumberClass n1, NumberClass n2) => n1 == n2 || n1 > n2;
        public static bool operator <=(NumberClass n1, NumberClass n2) => n1 == n2 || n1 < n2;

        public static implicit operator NumberClass(double d) => new NumberClass(d);
        public static implicit operator NumberClass(string s) => new NumberClass(s);

        public static explicit operator long(NumberClass n) =>
            (long) (n > Long ? long.MaxValue : n.mantissa * Math.Pow(10, n.exponent));

        public NumberClass Max(NumberClass n) => n > this ? n : this;
        public NumberClass Abs() => new NumberClass(Math.Abs(mantissa), exponent);
        public bool IsNeg() => mantissa < 0;

        public override string ToString() =>
            exponent < 5
                ? $"{mantissa * Math.Pow(10, exponent):#,##0.##}"
                : $"{mantissa:0.##}e{new NumberClass(exponent)}".Replace("e1e", CutOff1E ? "ee" : "e1e");

        public string ToString(Func<double, double, string> format) => format.Invoke(mantissa, exponent);
    }
}