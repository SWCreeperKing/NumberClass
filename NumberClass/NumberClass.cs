using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public readonly struct NumberClass
{
    public const double MagnitudeLimiter = 1e100;

    public static readonly NumberClass MaxMagnitudeValue = new(9.99, double.MaxValue, UInt128.MaxValue);
    public static readonly NumberClass MaxValue = new(9.99, double.MaxValue);
    public static readonly NumberClass Double = new(double.MaxValue);
    public static readonly NumberClass Float = new(float.MaxValue);
    public static readonly NumberClass Long = new(long.MaxValue);
    public static readonly NumberClass Int = new(int.MaxValue);
    public static readonly NumberClass E = new(Math.E);
    public static readonly NumberClass One = new(1);
    public static readonly NumberClass Zero = new();

    public static string Formatter = SciNotation.Name;

    public static Dictionary<string, Formatter> Formats = new()
    {
        { SciNotation.Name, new SciNotation() },
        { Engineering.Name, new Engineering() }
    };

    public double Mantissa { get; init; }
    public double Exponent { get; init; }
    public UInt128 Magnitude { get; init; }

    public NumberClass(double mantissa = 0, double exponent = 0, UInt128 magnitude = default)
    {
        (Mantissa, Exponent, Magnitude) = Update(mantissa, exponent, magnitude);
    }

    public NumberClass(NumberClass nc)
        => (Mantissa, Exponent, Magnitude) = Update(nc.Mantissa, nc.Exponent, nc.Magnitude);

    // todo rank functionality later
    public NumberClass(string s)
    {
        double man, exp = 0;

        double Parse(Span<char> text)
        {
            return text.ToString() == "" ? 1 : double.Parse(text.ToString(), CultureInfo.InvariantCulture);
        }

        var span = new Span<char>(s.ToLower().ToCharArray());
        if (span.Contains('e'))
        {
            var e = s.Count(c => c == 'e');
            if (e > 2) throw new Exception("NumberClass: string can only have 2 `e`s");
            var first = span.IndexOf('e');
            if (e == 2)
            {
                var last = span.LastIndexOf('e');
                man = Parse(span[..first]);
                exp = Parse(span[(first + 1)..last]) * Math.Pow(10, Parse(span[(last + 1)..]));
            }
            else (man, exp) = (Parse(span[..first]), Parse(span[(first + 1)..]));
        }
        else man = double.Parse(s, CultureInfo.InvariantCulture);

        (Mantissa, Exponent, Magnitude) = Update(man, exp);
    }

    private static (double man, double exp, UInt128) Update(double man = 0, double exp = 0, UInt128 magnitude = default)
    {
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
                => Math.Abs(min.Exponent - Math.Pow(10, max.Exponent))),
            _ => 16
        };

    public UInt128 MaxMagnitude(NumberClass nc) => nc.Magnitude > Magnitude ? nc.Magnitude : Magnitude;
    public UInt128 MinMagnitude(NumberClass nc) => nc.Magnitude < Magnitude ? nc.Magnitude : Magnitude;

    public double MagnitudeDelta(NumberClass nc)
        => (double) (Magnitude > nc.Magnitude ? Magnitude - nc.Magnitude : nc.Magnitude - Magnitude);

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

    //VVVVV continue magnitude VVVVV
    public static NumberClass operator *(NumberClass n1, NumberClass n2)
    {
        if (n1 == Zero || n2 == Zero) return Zero;
        if (n1 == One || n2 == One) return n1 == One ? n2 : n1;
        return new NumberClass(n1.Mantissa * n2.Mantissa, n1.Exponent + n2.Exponent);
    }

    public static NumberClass operator /(NumberClass n1, NumberClass n2)
    {
        if (n2 == Zero) throw new DivideByZeroException("NumberClass: Can not divide by 0");
        if (n1 == Zero) return Zero;
        if (n2 == One) return n1;
        return new NumberClass(n1.Mantissa / n2.Mantissa, n1.Exponent - n2.Exponent);
    }

    public NumberClass Pow(NumberClass n)
    {
        if (n == One || this == One || this == Zero) return this;
        if (n == Zero) return One;
        if (Exponent == 0 && n.Exponent == 0) return Math.Pow(Mantissa, n.Mantissa);

        var tempExpo = Exponent + Math.Log10(Mantissa);
        if (Math.Max(Math.Log10(Exponent), 0) + n.Exponent < 300)
        {
            tempExpo *= n.GetRealMantissa();
            return tempExpo < 1e17
                ? new NumberClass(Math.Pow(10, tempExpo % 1), Math.Floor(tempExpo))
                : new NumberClass(Mantissa, tempExpo);
        }

        return new NumberClass(Mantissa, Math.Log10(tempExpo) + (n.Exponent + Math.Log10(n.Exponent)));
    }

    public NumberClass Root(long rootBase)
    {
        var mod = Exponent % rootBase;
        return new NumberClass(Math.Pow(Mantissa * Math.Pow(10, mod), 1f / rootBase), (Exponent - mod) / rootBase);
    }

    public NumberClass Sqrt() => Root(2);
    public NumberClass Cbrt() => Root(3);

    public NumberClass Log() => Log(E);
    public NumberClass Log(NumberClass logBase) => this == Zero ? Zero : Log10() / logBase.Log10();
    public NumberClass Log2() => Log(2);
    public NumberClass Log10() => Exponent + Math.Log10(Mantissa);

    #endregion

    #region casting operators

    public static implicit operator NumberClass(double d) => new(d);
    public static implicit operator NumberClass(string s) => new(s);

    public static explicit operator int(NumberClass n)
    {
        if (n > Int) return int.MaxValue;
        return (int) (n.Mantissa * Math.Pow(10, n.Exponent));
    }

    public static explicit operator long(NumberClass n)
    {
        if (n > Long) return long.MaxValue;
        return (long) (n.Mantissa * Math.Pow(10, n.Exponent));
    }

    public static explicit operator double(NumberClass n)
    {
        if (n > Double) return double.MaxValue;
        return n.Mantissa * Math.Pow(10, n.Exponent);
    }

    public static explicit operator float(NumberClass n)
    {
        if (n > Float) return float.MaxValue;
        return (float) (n.Mantissa * Math.Pow(10, n.Exponent));
    }

    #endregion

    // func<n1, n2, isOrderKept, result>
    public T MinMaxOp<T>(NumberClass nc, Func<NumberClass, NumberClass, bool, T> minMaxAction)
        => this > nc ? minMaxAction(nc, this, false) : minMaxAction(this, nc, true);

    public override string ToString() => Formats[Formatter].Format(this);
    public string ToExactString() => $"{Mantissa}_{Exponent}_{Magnitude}";
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