using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static NumberClass;
using Double = System.Double;

public readonly struct NumberClass
{
    private static readonly Dictionary<string, (double Mantissa, double Exponent, UInt128 Magnitude)> StringCache =
        new();

    private static readonly Regex NormalSyn = new(@"(e*)([\d.,]*e{1}[\d.,]*e{0,1}[\d.,]*)$");
    private static readonly Regex ComplexSyn = new(@"([\d.,]*)e{1}([\d.,]*e{0,1})([\d.,]*)$");
    private static readonly Regex MagSyn = new(@"([\d,]*)f([\d.,]*e{1}[\d.,]*e{0,1}[\d.,]*)$");

    public const double MagnitudeLimiter = 1e100;
    public const int U128Exponent = 38;
    public static readonly double U128Double = (double)UInt128.MaxValue;
    public static readonly NumberClass NegInf = new(double.NegativeInfinity, double.NegativeInfinity);
    public static readonly NumberClass Inf = new(double.PositiveInfinity, double.PositiveInfinity);
    public static readonly NumberClass NaN = new(double.NaN, double.NaN);
    public static readonly NumberClass MaxMagnitudeValue = new(9.99, double.MaxValue, UInt128.MaxValue);
    public static readonly NumberClass MaxValue = new(9.99, double.MaxValue);
    public static readonly NumberClass Double = new(double.MaxValue);
    public static readonly NumberClass Float = new(float.MaxValue);
    public static readonly NumberClass Long = new(long.MaxValue);
    public static readonly NumberClass Int = new(int.MaxValue);
    public static readonly NumberClass E = new(Math.E);
    public static readonly NumberClass Ten = new(1, 1);
    public static readonly NumberClass One = new(1);
    public static readonly NumberClass Zero = new();
    public static readonly NumberClass NegOne = new(-1);
    public static readonly NumberClass NegTwo = new(-2);

    public static string Formatter = SciNotation.Name;

    public static Dictionary<string, Formatter> Formats = new()
    {
        { SciNotation.Name, new SciNotation() },
        { Engineering.Name, new Engineering() }
    };

    public double Mantissa { get; init; }
    public double Exponent { get; init; }
    public UInt128 Magnitude { get; init; } // 340,282,366,920,938,463,463,374,607,431,768,211,456

    public NumberClass(double mantissa = 0, double exponent = 0, UInt128 magnitude = default)
    {
        (Mantissa, Exponent, Magnitude) = Update(mantissa, exponent, magnitude);
    }

    public NumberClass(NumberClass nc)
        => (Mantissa, Exponent, Magnitude) = Update(nc.Mantissa, nc.Exponent, nc.Magnitude);

    /// <summary>
    /// Supported Formatting <br />
    /// M = mantissa (double)<br />
    /// N = exponent's mantissa (double, 1 by default)<br />
    /// X = exponent (double)<br />
    /// L = layer (non-negative integer, up to 2^128 <br />
    /// or 340,282,366,920,938,463,463,374,607,431,768,211,456) <br />
    /// base 'A' formatting:
    /// <list type="bullet">
    /// <item>M => M</item>
    /// <item>eX => 10^X</item>
    /// <item>Me => M * 10 </item>
    /// <item>MeX => M * 10 ^ X </item>
    /// <item>eeX => 10 ^ (1 * 10 ^ X) </item>
    /// <item>MeeX => M * 10 ^ (1 * 10 ^ X) </item>
    /// <item>MeNeX => M * 10 ^ (N * 10 ^ X) </item>
    /// </list>
    ///
    /// extended formatting:
    /// <list type="bullet">
    /// <item>e... (N e's) A => 10^... (N 10^s) A</item> 
    /// <item>LfA => 10^... (L 10^s) A</item> 
    /// </list>>
    /// note: <br />
    /// fM or fX is not allowed, do fMe or FeX <br />
    /// eeeX may cause problems
    /// </summary>
    public NumberClass(string s)
    {
        if (StringCache.TryGetValue(s, out var cached))
        {
            (Mantissa, Exponent, Magnitude) = cached;
            return;
        }

        s = s.ToLower();
        var rawS = s;
        double man, exp = 0;
        UInt128 mag = 0;

        if (s.Contains('f'))
        {
            var fLoc = s.IndexOf('f');
            mag = UInt128.Parse(s[..fLoc], CultureInfo.CurrentCulture);
            (man, exp) = ParseComplex(s[(fLoc + 1)..]);
        }
        else if (s.Contains('e'))
        {
            var match = NormalSyn.Match(s).Groups;
            mag = (UInt128) match[1].Value.Count(c => c == 'e');
            (man, exp) = ParseComplex(match[2].Value);
        }
        else man = Parse(s);

        StringCache[rawS] = (Mantissa, Exponent, Magnitude) = Update(man, exp, mag);
        return;

        double Parse(string text)
            => double.TryParse(text, CultureInfo.CurrentCulture, out var d)  ? d : 1;

        (double man, double exp) ParseComplex(string s)
        {
            var regRes = ComplexSyn.Match(s).Groups;
            var man = Parse(regRes[1].Value);
            var expMan = Parse(regRes[2].Value);
            var exp = regRes[3].Value != "" ? Parse(regRes[3].Value[..^1]) : -1;

            return exp == -1 ? (man, expMan) : (man, expMan * Math.Pow(10, exp));
        }
    }

    private static (double man, double exp, UInt128 mag) Update(double man = 0, double exp = 0,
        UInt128 magnitude = default)
    {
        if (double.IsNaN(man) || double.IsNaN(exp) || double.IsInfinity(man) || double.IsInfinity(exp))
            return (man, exp, magnitude);
        if (man == 0) return (0, 0, 0);

        var isNeg = man < 0;
        if (isNeg) man = -man;

        var log = Math.Floor(Math.Log10(man));
        man /= Math.Pow(10, log);
        if (isNeg) man = -man;
        exp += log;

        if (exp < MagnitudeLimiter || magnitude == UInt128.MaxValue) return (man, exp, magnitude);
        return (man, Math.Log10(exp), magnitude + 1);
    }

    public bool IsNaN() => double.IsNaN(Mantissa) || double.IsNaN(Exponent);
    public bool IsNeg() => Mantissa < 0;
    public double GetRealMantissa() => Magnitude > 0 || Exponent > 308 ? Mantissa : Mantissa * Math.Pow(10, Exponent);
    public NumberClass Max(NumberClass nc) => nc > this ? nc : this;
    public NumberClass Min(NumberClass nc) => nc < this ? nc : this;
    public NumberClass Negate() => this with { Mantissa = -Mantissa };
    public NumberClass Ceiling() => this with { Mantissa = Math.Ceiling(Mantissa) };
    public NumberClass Floor() => this with { Mantissa = Math.Floor(Mantissa) };
    public NumberClass Round() => this with { Mantissa = Math.Round(Mantissa) };
    public NumberClass Abs() => this with { Mantissa = Math.Abs(Mantissa) };

    public double ExponentialDelta(NumberClass nc)
        => MagnitudeDelta(nc) switch
        {
            0 => Math.Abs(Exponent - nc.Exponent),
            1 => MinMaxOp(nc, (min, max, _)
                => Math.Abs(double.Log10(min.Exponent) - max.Exponent)),
            _ => 16
        };

    public UInt128 MaxMagnitude(NumberClass nc) => nc.Magnitude > Magnitude ? nc.Magnitude : Magnitude;
    public UInt128 MinMagnitude(NumberClass nc) => nc.Magnitude < Magnitude ? nc.Magnitude : Magnitude;

    public double MagnitudeDelta(NumberClass nc)
        => (double)(Magnitude > nc.Magnitude ? Magnitude - nc.Magnitude : nc.Magnitude - Magnitude);

    #region compare operators

    public static bool operator ==(NumberClass n1, NumberClass n2)
        => Math.Round(n1.Mantissa, 7) == Math.Round(n2.Mantissa, 7) && n1.Exponent == n2.Exponent &&
           n1.Magnitude == n2.Magnitude;

    public static bool operator !=(NumberClass n1, NumberClass n2)
        => Math.Round(n1.Mantissa, 7) != Math.Round(n2.Mantissa, 7) || n1.Exponent != n2.Exponent ||
           n1.Magnitude != n2.Magnitude;

    public static bool operator >(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n2.IsNeg();
        if (n1.Magnitude != n2.Magnitude) return n1.Magnitude > n2.Magnitude;
        if (n1.Exponent != n2.Exponent) return n1.Exponent > n2.Exponent;
        return n1.Mantissa > n2.Mantissa;
    }

    public static bool operator <(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n1.IsNeg();
        if (n1.Magnitude != n2.Magnitude) return n1.Magnitude < n2.Magnitude;
        if (n1.Exponent != n2.Exponent) return n1.Exponent < n2.Exponent;
        return n1.Mantissa < n2.Mantissa;
    }

    public static bool operator >=(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n2.IsNeg();
        if (n1.Magnitude != n2.Magnitude) return n1.Magnitude > n2.Magnitude;
        if (n1.Exponent != n2.Exponent) return n1.Exponent > n2.Exponent;
        return n1.Mantissa >= n2.Mantissa;
    }

    public static bool operator <=(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n1.IsNeg();
        if (n1.Magnitude != n2.Magnitude) return n1.Magnitude < n2.Magnitude;
        if (n1.Exponent != n2.Exponent) return n1.Exponent < n2.Exponent;
        return n1.Mantissa <= n2.Mantissa;
    }

    #endregion

    #region arithmetic operators

    public static NumberClass operator ++(NumberClass nc) => nc + 1;
    public static NumberClass operator --(NumberClass nc) => nc - 1;

    public static NumberClass operator +(NumberClass n1, NumberClass n2)
    {
        var delta = n1.ExponentialDelta(n2);
        if (delta > 15) return n1.Max(n2);
        return n1.MinMaxOp(n2, (min, max, _)
            => new NumberClass(max.Mantissa + min.Mantissa / Math.Pow(10, delta), max.Exponent, max.Magnitude));
    }

    public static NumberClass operator -(NumberClass n1, NumberClass n2)
    {
        var delta = n1.ExponentialDelta(n2);
        return delta switch
        {
            > 15 when n1 >= n2 => n1,
            > 15 => n2.Negate(),
            _ => n1.MinMaxOp(n2, (min, max, isOrderKept) =>
            {
                if (!isOrderKept)
                    return new NumberClass(n1.Mantissa - n2.Mantissa / Math.Pow(10, delta), max.Exponent,
                        max.Magnitude);
                return new NumberClass(n1.Mantissa / Math.Pow(10, delta) - n2.Mantissa, max.Exponent, max.Magnitude);
            })
        };
    }

    public static NumberClass operator *(NumberClass n1, NumberClass n2)
    {
        if (n1 == Zero || n2 == Zero) return Zero;
        if (n1 == One || n2 == One) return n1 == One ? n2 : n1;

        var exponent = n1.MagnitudeDelta(n2) switch
        {
            0 => n1.Exponent + n2.Exponent,
            1 => n1.MinMaxOp(n2, (min, max, _)
                => double.Log10(min.Exponent) + max.Exponent),
            _ => Math.Max(n1.Exponent, n2.Exponent)
        };

        return new NumberClass(n1.Mantissa * n2.Mantissa, exponent, n1.MaxMagnitude(n2));
    }

    public static NumberClass operator /(NumberClass n1, NumberClass n2)
    {
        if (n2 == Zero) throw new DivideByZeroException("NumberClass: Can not divide by 0");
        if (n1 == Zero) return Zero;
        if (n2 == One) return n1;

        var exponent = n1.MagnitudeDelta(n2) switch
        {
            0 => n1.Exponent - n2.Exponent,
            1 => n1.MinMaxOp(n2, (min, max, _)
                => double.Log10(min.Exponent) - max.Exponent),
            _ => n1.Exponent > n2.Exponent ? n1.Exponent : -n2.Exponent
        };

        return new NumberClass(n1.Mantissa / n2.Mantissa, exponent);
    }

    public NumberClass Pow(NumberClass n) // from https://github.com/Patashu/break_eternity.js
    {
        if (n == One || this == One || this == Zero) return this;
        if (n == Zero) return One;

        var ret = (Abs().Log10() * n).Pow10WithThis();
        if (IsNeg() && int.Parse($"{$"{n.Mantissa}"[^1]}") % 2 != 0) return ret.Negate();
        return ret;
    }

    public NumberClass Pow10WithThis()
    {
        if (Exponent < -15) return Zero;
        var man = Mantissa;
        var expo = Exponent;

        if (Exponent < 0)
        {
            man /= Math.Pow(10, Math.Abs(Exponent));
            expo = 0;
        }

        var trunc = double.Truncate(man);

        if (trunc > 0)
        {
            var tExpo = Math.Floor(Math.Log10(trunc));
            trunc /= Math.Pow(10, tExpo);
            expo += tExpo;
        }

        var leftExpo = expo % 100;
        var newExpo = trunc * Math.Pow(10, leftExpo);
        var num = new NumberClass(Math.Round(Math.Pow(10, man - trunc), 12), newExpo,
            Magnitude + (UInt128)(expo - leftExpo) / 100);
        return num;
    }

    public NumberClass Root(long rootBase)
    {
        if (Magnitude > 0)
        {
            // todo: find a way to do this /shrug
            Console.WriteLine("EXECUTED ADVANCED POWER, WIP RETURNING MAX");
            return this;
        }

        var mod = Exponent % rootBase;
        return new NumberClass(Math.Pow(Mantissa * Math.Pow(10, mod), 1f / rootBase), (Exponent - mod) / rootBase);
    }

    public NumberClass Sqrt() => Root(2);
    public NumberClass Cbrt() => Root(3);

    public NumberClass Log() => Log(E);

    public NumberClass Log(NumberClass logBase) => this == Zero ? Zero : Log10() / logBase.Log10();

    public NumberClass Log2() => Log(2);

    public NumberClass Log10() =>
        Magnitude > 1
            ? new NumberClass(Exponent + Math.Log10(Mantissa), 1, Magnitude - 1)
            : Exponent + Math.Log10(Mantissa);

    #endregion

    #region casting operators

    public static implicit operator NumberClass(double d) => new(d);
    public static implicit operator NumberClass(string s) => new(s);

    public static explicit operator int(NumberClass n)
    {
        if (n > Int) return int.MaxValue;
        return (int)(n.Mantissa * Math.Pow(10, n.Exponent));
    }

    public static explicit operator long(NumberClass n)
    {
        if (n > Long) return long.MaxValue;
        return (long)(n.Mantissa * Math.Pow(10, n.Exponent));
    }

    public static explicit operator double(NumberClass n)
    {
        if (n > Double) return double.MaxValue;
        return n.Mantissa * Math.Pow(10, n.Exponent);
    }

    public static explicit operator float(NumberClass n)
    {
        if (n > Float) return float.MaxValue;
        return (float)(n.Mantissa * Math.Pow(10, n.Exponent));
    }

    #endregion

    // func<n1, n2, isOrderKept, result>
    public T MinMaxOp<T>(NumberClass nc, Func<NumberClass, NumberClass, bool, T> minMaxAction)
        => this > nc ? minMaxAction(nc, this, false) : minMaxAction(this, nc, true);

    public override string ToString() => Formats[Formatter].Format(this);
    public string ToExactString() => $"{Math.Round(Mantissa, 10)}_{Exponent}_{Magnitude}".Replace("E+", "e");
    public string ToRawString() => $"{Mantissa}_{Exponent}_{Magnitude}";
}

public abstract class Formatter
{
    public static bool CutOff1E = true; // format; 1e1e30 => 1ee30

    // get proper format can be 0.000 or 0.### 
    string GetFormatFromCount(int count, bool optional = false)
    {
        return $"0.{string.Join("", Enumerable.Repeat(optional ? '#' : '0', count))}";
    }

    public string Format(NumberClass nc) => FormatRaw(nc);
    protected abstract string FormatRaw(NumberClass nc);
}

public class SciNotation : Formatter
{
    public static readonly string Name = "SciNotation";
    public static int BeforeSciCut = 5;
    public static int BeforeSciCutExponent = 3;
    public static char E = 'e';

    protected override string FormatRaw(NumberClass nc)
    {
        if (nc.Exponent <= BeforeSciCut) return $"{nc.GetRealMantissa():###,##0.##}";
        var expExp = Math.Floor(Math.Log10(nc.Exponent));

        if (expExp <= BeforeSciCutExponent) return $"{nc.Mantissa:0.##}{E}{nc.Exponent:###,###}";

        var expMan = nc.Exponent / Math.Pow(10, expExp);

        string GetMantissaIfReasonable() => expExp <= 15 ? $"{nc.Mantissa:0.00}" : "";

        if (CutOff1E && expMan == 1) return $"{GetMantissaIfReasonable()}{E}{E}{expExp:###,###}";
        return $"{GetMantissaIfReasonable()}{E}{expMan:0.00}{E}{expExp:###,###}";
    }
}

public class Engineering : Formatter
{
    public static readonly string Name = "Engineering";
    public static int BeforeSciCut = 5;
    public static int BeforeSciCutExponent = 3;
    public static char E = 'e';

    protected override string FormatRaw(NumberClass nc)
    {
        if (nc.Exponent <= BeforeSciCut) return $"{nc.GetRealMantissa():###,##0.##}";
        var expExp = Math.Floor(Math.Log10(nc.Exponent));

        var ext = nc.Exponent % 3;
        var nMan = nc.Mantissa * Math.Pow(10, ext);
        var nExp = nc.Exponent - ext;
        if (expExp <= BeforeSciCutExponent) return $"{nMan:##0.##}{E}{nExp:###,###}";

        var expMan = nExp / Math.Pow(10, expExp);

        string GetMantissaIfReasonable() => expExp <= 15 ? $"{nMan:0.00}" : "";

        var expExt = expExp % 3;
        var nExpMan = expMan * Math.Pow(10, expExt);
        var nExpExp = expExp - expExt;
        if (CutOff1E && expMan == 1) return $"{GetMantissaIfReasonable()}{E}{E}{nExpExp:###,###}";
        return $"{GetMantissaIfReasonable()}{E}{nExpMan:##0.00}{E}{nExpExp:###,###}";
    }
}