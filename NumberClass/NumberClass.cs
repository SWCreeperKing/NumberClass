using System;

namespace NumberClass
{
#pragma warning disable 660,661
    public class NumberClass
#pragma warning restore 660,661
    {
        // NumberClass made by SW_CreeperKing#5787
        // Special thanks to Number Engineer#9999 (developer of Incremental Unlimited 1 & 2 and Line Maze Idle) for math help
        public enum Format
        {
            Scientific,
            Engineering,
            Alphabet
        }

        public static bool CutOff1E = true; // format; 1e1e30 => 1ee30 
        public static float Version { get; } = .22f;
        public static Format format = Format.Scientific;

        public static NumberClass MaxValue = new NumberClass(9.99, double.MaxValue);
        public static NumberClass Double = new NumberClass(double.MaxValue);
        public static NumberClass Float = new NumberClass(float.MaxValue);
        public static NumberClass Long = new NumberClass(long.MaxValue);
        public static NumberClass Int = new NumberClass(int.MaxValue);
        public static NumberClass One = new NumberClass(1);
        public static NumberClass Zero = new NumberClass();

        public double mantissa;
        public double exponent;

        public NumberClass() : this(0)
        {
        }
        
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
            var log = (long) Math.Log10(mantissa);
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
            n1 == Zero || n2 == Zero
                ? 0
                : n1 == One || n2 == One
                    ? n1.Max(n2)
                    : new NumberClass(n1.mantissa * n2.mantissa, n1.exponent + n2.exponent);

        public static NumberClass operator /(NumberClass n1, NumberClass n2) =>
            n1 == Zero
                ? Zero
                : n2 == Zero
                    ? throw new DivideByZeroException("NumberClass: Can not divide by 0")
                    : n2 == One
                        ? n1
                        : new NumberClass(n1.mantissa / n2.mantissa, n1.exponent - n2.exponent);

        public NumberClass Pow(NumberClass n)
        {
            if (n == One || this == One || this == Zero) return this;
            if (n == Zero) return One;
            if (exponent == 0 && n.exponent == 0) return Math.Pow(mantissa, n.mantissa);
            var tempExpo = exponent + Math.Log10(mantissa);
            if (Math.Max(Math.Log10(exponent), 0) + n.exponent < 300)
            {
                tempExpo *= n.GetRealMantissa();
                return tempExpo < 1e17
                    ? new NumberClass(Math.Pow(10, tempExpo % 1), Math.Floor(tempExpo))
                    : new NumberClass(mantissa, tempExpo);
            }

            tempExpo = Math.Log10(tempExpo);
            tempExpo += n.exponent + Math.Log10(n.exponent);
            return new NumberClass(mantissa, tempExpo);
        }

        public NumberClass Root(long @base)
        {
            var mod = exponent % @base;
            return new NumberClass(Math.Pow(mantissa * Math.Pow(10, mod), 1f / @base), (exponent - mod) / @base);
        }

        public NumberClass Sqrt() => Root(2);
        public NumberClass Cbrt() => Root(3);
        public NumberClass Log10() => exponent + Math.Log10(mantissa);
        public NumberClass Log(NumberClass @base) => Log10() / @base.Log10();
        public NumberClass Log2() => Log(2);
        public static NumberClass operator ++(NumberClass n) => n += One;
        public static NumberClass operator --(NumberClass n) => n -= One;

        public static bool operator >(NumberClass n1, NumberClass n2) =>
            n1.exponent > n2.exponent || n1.exponent == n2.exponent && n1.mantissa > n2.mantissa;

        public static bool operator <(NumberClass n1, NumberClass n2) =>
            n1.exponent < n2.exponent || n1.exponent == n2.exponent && n1.mantissa < n2.mantissa;

        public static bool operator ==(NumberClass n1, NumberClass n2) =>
            n1.mantissa == n2.mantissa && n1.exponent == n2.exponent;

        public static bool operator !=(NumberClass n1, NumberClass n2) =>
            n1.mantissa != n2.mantissa || n1.exponent != n2.exponent;

        public static bool operator >=(NumberClass n1, NumberClass n2) => n1 == n2 || n1 > n2;
        public static bool operator <=(NumberClass n1, NumberClass n2) => n1 == n2 || n1 < n2;

        public static implicit operator NumberClass(double d) => new NumberClass(d);
        public static implicit operator NumberClass(string s) => new NumberClass(s);

        public static explicit operator int(NumberClass n) =>
            (int) (n > Long ? long.MaxValue : n.mantissa * Math.Pow(10, n.exponent));

        public static explicit operator long(NumberClass n) =>
            (long) (n > Long ? long.MaxValue : n.mantissa * Math.Pow(10, n.exponent));

        public static explicit operator double(NumberClass n) =>
            n > Double ? long.MaxValue : n.mantissa * Math.Pow(10, n.exponent);

        public static explicit operator float(NumberClass n) =>
            (float) (n > Float ? long.MaxValue : n.mantissa * Math.Pow(10, n.exponent));

        public double GetRealMantissa() => exponent > 308 ? mantissa : mantissa * Math.Pow(10, exponent);
        public NumberClass Ceiling() => new NumberClass(Math.Ceiling(mantissa), exponent);
        public NumberClass Floor() => new NumberClass(Math.Floor(mantissa), exponent);
        public NumberClass Round() => new NumberClass(Math.Round(mantissa), exponent);
        public NumberClass Max(NumberClass n) => n > this ? n : this;
        public NumberClass Min(NumberClass n) => n < this ? n : this;
        public NumberClass Abs() => new NumberClass(Math.Abs(mantissa), exponent);
        public bool IsNeg() => mantissa < 0;
        public bool IsNaN() => double.IsNaN(mantissa) || double.IsNaN(exponent);
        public override string ToString() => FormatNc(format);
        public string ToString(Func<double, double, string> format) => format.Invoke(mantissa, exponent);

        public string FormatNc(Format format)
        {
            if (exponent < 5) return $"{mantissa * Math.Pow(10, exponent):#,##0.##}";
            var useMan = Math.Log10(exponent) < 15; // at a point the mantissa is useless
            string CutOff1Check(string s) => s.Replace("e1e", CutOff1E ? "ee" : "e1e");

            string formatMantissa;
            string formatExponent;
            switch (format)
            {
                case Format.Alphabet:

                    // string Letter(double n)
                    // {
                    //     
                    // }
                    //
                    // if (useMan)
                    // {
                    //     
                    // }
                    return "WIP";
                    break;
                case Format.Engineering:
                    var extended = exponent % 3;
                    formatMantissa = useMan ? $"{Math.Floor(mantissa * Math.Pow(10, extended) * 100) / 100}" : "";
                    formatExponent = new NumberClass(exponent - extended).FormatNc(Format.Engineering);
                    return CutOff1Check($"{formatMantissa}e{formatExponent}");
                default:
                    formatMantissa = useMan ? $"{Math.Floor(mantissa * 100) / 100}" : "";
                    formatExponent = new NumberClass(exponent).FormatNc(Format.Scientific);
                    return CutOff1Check($"{formatMantissa}e{formatExponent}");
            }
        }
    }
}