﻿using System;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;

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
            ScientificStatic,
            ScientificExtended,
            Engineering,
        }

        public static float Version { get; } = .25f;
        public static bool CutOff1E = true; // format; 1e1e30 => 1ee30 
        public static int SciStaticLeng = 4;
        public static Format format = Format.Scientific;

        public static readonly NumberClass MaxValue = new NumberClass(9.99, double.MaxValue);
        public static readonly NumberClass Double = new NumberClass(double.MaxValue);
        public static readonly NumberClass Float = new NumberClass(float.MaxValue);
        public static readonly NumberClass Long = new NumberClass(long.MaxValue);
        public static readonly NumberClass Int = new NumberClass(int.MaxValue);
        public static readonly NumberClass One = new NumberClass(1);
        public static readonly NumberClass Zero = new NumberClass();

        public double mantissa;
        public double exponent;

        public long stage;
        // future:
        // mant[stage]exp
        // at the point of stage++ mant is usless so just keep [stage]exp
        // when stage++ mantissa = exponent, exponent = 0, update();
        // new potential? (need number to proof):
        // 1 * 10 ^^^....^ 308
        //       this ^ should extend to long.max, meaning there should be long.max Es b/t man and exp
        // this should only cost 8 bytes more
        // class in bytes: 16 => 24
        // plausible problems:
        // - rip from string, you will probs be missed (for now) 

        public NumberClass() : this(0)
        {
        }

        public NumberClass(double mantissa = 0, double exponent = 0, long stage = 0)
        {
            (this.mantissa, this.exponent, this.stage) = (mantissa, exponent, stage);
            Update();
        }

        public NumberClass(NumberClass nc) : this(nc.mantissa, nc.exponent, nc.stage)
        {
        }

        public NumberClass(string s)
        {
            // todo: support for stage
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

            // if mantissa becomes useless
            // stage swap
            if (!IsMantissaUseless()) return;
            // stage++;
            // mantissa = exponent;
            // exponent = 0;
            // Update();
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
            n1.stage > n2.stage || n1.stage == n2.stage &&
            (n1.exponent > n2.exponent || n1.exponent == n2.exponent && n1.mantissa > n2.mantissa);

        public static bool operator <(NumberClass n1, NumberClass n2) =>
            n1.stage < n2.stage || n1.stage == n2.stage &&
            (n1.exponent < n2.exponent || n1.exponent == n2.exponent && n1.mantissa < n2.mantissa);

        public static bool operator ==(NumberClass n1, NumberClass n2) =>
            n1.stage == n2.stage && n1.mantissa == n2.mantissa && n1.exponent == n2.exponent;

        public static bool operator !=(NumberClass n1, NumberClass n2) =>
            n1.stage != n2.stage || n1.mantissa != n2.mantissa || n1.exponent != n2.exponent;

        public static bool operator >=(NumberClass n1, NumberClass n2) => n1 == n2 || n1 > n2;
        public static bool operator <=(NumberClass n1, NumberClass n2) => n1 == n2 || n1 < n2;

        public static implicit operator NumberClass(double d) => new NumberClass(d);
        public static implicit operator NumberClass(string s) => new NumberClass(s);

        public static explicit operator int(NumberClass n) =>
            n.stage > 0 ? int.MaxValue : (int) (n > Int ? int.MaxValue : n.mantissa * Math.Pow(10, n.exponent));

        public static explicit operator long(NumberClass n) =>
            n.stage > 0 ? long.MaxValue : (long) (n > Long ? long.MaxValue : n.mantissa * Math.Pow(10, n.exponent));

        public static explicit operator double(NumberClass n) =>
            n.stage > 0
                ? double.MaxValue
                : n > Double
                    ? double.MaxValue
                    : n.mantissa * Math.Pow(10, n.exponent);

        public static explicit operator float(NumberClass n) =>
            n.stage > 0 ? float.MaxValue : (float) (n > Float ? float.MaxValue : n.mantissa * Math.Pow(10, n.exponent));

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

        // mantissa becomes useless once exponent precision is lost
        public bool IsMantissaUseless() => Math.Log10(exponent) >= 15;

        public string FormatNc(Format format)
        {
            if (exponent < 5) return $"{mantissa * Math.Pow(10, exponent):#,##0.##}";
            var useMan = !IsMantissaUseless(); // if take mantissa or leave it

            // does not catch engineering but w.e. is probs fine
            string CutOff1Check(string s) => AddStage(!CutOff1E ? s : Regex.Replace(s, @"e(1|1.0*)e", "ee"));

            // get proper format
            // can be #.000 or #.### 
            string GetFormatFromCount(int count, bool optional = true) =>
                $"#.{string.Join("", Enumerable.Repeat(optional ? '#' : '0', count))}";

            // todo: get advice on how to properly format stage
            string AddStage(string s) => stage > 0 ? $"[{stage}]{s}" : s;

            string formatMantissa;
            string formatExponent;
            string stringForm;
            switch (format)
            {
                case Format.Engineering:
                    var extended = exponent % 3;
                    formatMantissa = useMan ? $"{mantissa * Math.Pow(10, extended):##0.##}" : "";
                    formatExponent = new NumberClass(exponent - extended).FormatNc(Format.Engineering)
                        .Replace("1e", "e");
                    return CutOff1Check($"{formatMantissa}e{formatExponent}");
                case Format.ScientificStatic:
                    // format to keep numclass the same leng
                    formatExponent = new NumberClass(exponent).FormatNc(Format.Scientific);
                    formatMantissa = useMan ? $"{mantissa.ToString(GetFormatFromCount(SciStaticLeng, false))}" : "";
                    return CutOff1Check($"{formatMantissa}e{formatExponent}");
                default:
                    formatMantissa = useMan
                        ? $"{mantissa.ToString(GetFormatFromCount(format != Format.ScientificExtended ? 2 : 6))}"
                        : "";
                    formatExponent = new NumberClass(exponent).FormatNc(Format.Scientific);
                    return CutOff1Check($"{formatMantissa}e{formatExponent}");
            }
        }
    }
}