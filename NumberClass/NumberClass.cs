using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public record NumberClass
{
    public static readonly NumberClass MaxValue = new(9.99, double.MaxValue);
    public static readonly NumberClass Double = new(double.MaxValue);
    public static readonly NumberClass Float = new(float.MaxValue);
    public static readonly NumberClass Long = new(long.MaxValue);
    public static readonly NumberClass Int = new(int.MaxValue);
    public static readonly NumberClass E = new(Math.E);
    public static readonly NumberClass One = new(1);
    public static readonly NumberClass Zero = new();

    public static readonly float Version = 1f;

    public static string formatter = SciNotation.Name;

    public static Dictionary<string, Formatter> formats = new()
    {
        { SciNotation.Name, new SciNotation() },
        { Engineering.Name, new Engineering() }
    };

    public double Mantissa { get; }
    public double Exponent { get; }

    public NumberClass() => Mantissa = Exponent = 0;
    public NumberClass(double mantissa, double exponent = 0) => (Mantissa, Exponent) = Update(mantissa, exponent);
    public NumberClass(NumberClass nc) => (Mantissa, Exponent) = Update(nc.Mantissa, nc.Exponent);

    public NumberClass(string s)
    {
        double man, exp = 0;
        double Parse(Span<char> text) => text.ToString() == "" ? 1 : double.Parse(text.ToString());

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
        else man = double.Parse(s);

        (Mantissa, Exponent) = Update(man, exp);
    }

    private static (double man, double exp) Update(double man = 0, double exp = 0)
    {
        if (man == 0) return (0, 0);

        var isNeg = man < 0;
        if (isNeg) man = -man;

        var log = Math.Floor(Math.Log10(man));
        man /= Math.Pow(10, log);
        if (isNeg) man = -man;
        exp += log;

        return (man, exp);
    }

    public bool IsNaN() => double.IsNaN(Mantissa) || double.IsNaN(Exponent);
    public bool IsNeg() => Mantissa < 0;
    public double GetRealMantissa() => Exponent > 308 ? Mantissa : Mantissa * Math.Pow(10, Exponent);
    public NumberClass Max(NumberClass nc) => nc > this ? nc : this;
    public NumberClass Min(NumberClass nc) => nc < this ? nc : this;
    public NumberClass Ceiling() => new(Math.Ceiling(Mantissa), Exponent);
    public NumberClass Floor() => new(Math.Floor(Mantissa), Exponent);
    public NumberClass Round() => new(Math.Round(Mantissa), Exponent);
    public NumberClass Abs() => new(Math.Abs(Mantissa), Exponent);

    #region compare operators

    public static bool operator >(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n2.IsNeg();
        if (n1.Exponent != n2.Exponent) return n1.Exponent > n2.Exponent;
        return n1.Mantissa > n2.Mantissa;
    }

    public static bool operator <(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n1.IsNeg();
        if (n1.Exponent != n2.Exponent) return n1.Exponent < n2.Exponent;
        return n1.Mantissa < n2.Mantissa;
    }

    public static bool operator >=(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n2.IsNeg();
        if (n1.Exponent != n2.Exponent) return n1.Exponent > n2.Exponent;
        return n1.Mantissa >= n2.Mantissa;
    }

    public static bool operator <=(NumberClass n1, NumberClass n2)
    {
        if (n1.IsNeg() ^ n2.IsNeg()) return n1.IsNeg();
        if (n1.Exponent != n2.Exponent) return n1.Exponent < n2.Exponent;
        return n1.Mantissa <= n2.Mantissa;
    }

    #endregion

    #region arithmetic operators

    public static NumberClass operator ++(NumberClass nc) => nc += 1;
    public static NumberClass operator --(NumberClass nc) => nc -= 1;

    public static NumberClass operator +(NumberClass n1, NumberClass n2)
    {
        var delta = Math.Abs(n1.Exponent - n2.Exponent);
        if (delta > 12) return n1.Max(n2);
        if (delta == 0) return new NumberClass(n1.Mantissa + n2.Mantissa, n1.Exponent);

        if (n1 > n2) return new NumberClass(n1.Mantissa + n2.Mantissa / Math.Pow(10, delta), n1.Exponent);
        return new NumberClass(n2.Mantissa + n1.Mantissa / Math.Pow(10, delta), n2.Exponent);
    }

    public static NumberClass operator -(NumberClass n1, NumberClass n2)
    {
        var delta = Math.Abs(n1.Exponent - n2.Exponent);
        if (delta > 12) return n1.Max(n2);
        if (delta == 0) return new NumberClass(n1.Mantissa + n2.Mantissa, n1.Exponent);

        if (n1 > n2) return new NumberClass(n1.Mantissa + n2.Mantissa / Math.Pow(10, delta), n1.Exponent);
        return new NumberClass(n2.Mantissa - n1.Mantissa / Math.Pow(10, delta), n2.Exponent);
    }

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

        tempExpo = Math.Log10(tempExpo);
        tempExpo += n.Exponent + Math.Log10(n.Exponent);
        return new NumberClass(Mantissa, tempExpo);
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

    public override string ToString() => formats[formatter].Format(this);
}

public abstract class Formatter
{
    public readonly StringBuilder sb = new();
    public static bool cutOff1E = true; // format; 1e1e30 => 1ee30

    // get proper format can be 0.000 or 0.### 
    string GetFormatFromCount(int count, bool optional = false)
    {
        return $"0.{string.Join("", Enumerable.Repeat(optional ? '#' : '0', count))}";
    }

    public string Format(NumberClass nc)
    {
        sb.Clear();
        return FormatRaw(nc).ToString();
    }

    protected abstract StringBuilder FormatRaw(NumberClass nc);
}

public class SciNotation : Formatter
{
    public static readonly string Name = "SciNotation";
    public static int beforeSciCut = 5;
    public static int beforeSciCutExponent = 3;
    public static char e = 'e';

    protected override StringBuilder FormatRaw(NumberClass nc)
    {
        if (nc.Exponent <= beforeSciCut) return sb.Append($"{nc.GetRealMantissa():###,##0.##}");
        var expExp = Math.Floor(Math.Log10(nc.Exponent));

        if (expExp <= beforeSciCutExponent)
        {
            return sb.Append($"{nc.Mantissa:0.##}{e}{nc.Exponent:###,###}");
        }

        var expMan = nc.Exponent / Math.Pow(10, expExp);
        if (expExp <= 15) sb.Append($"{nc.Mantissa:0.00}");
        sb.Append(e);

        if (cutOff1E && expMan == 1) return sb.Append($"{e}{expExp:###,###}");
        return sb.AppendFormat("{1:0.00}{0}{2:###,###}", e, expMan, expExp);
    }
}

public class Engineering : Formatter
{
    public static readonly string Name = "Engineering";
    public static int beforeSciCut = 5;
    public static int beforeSciCutExponent = 3;
    public static char e = 'e';

    protected override StringBuilder FormatRaw(NumberClass nc)
    {
        if (nc.Exponent <= beforeSciCut) return sb.Append($"{nc.GetRealMantissa():###,##0.##}");
        var expExp = Math.Floor(Math.Log10(nc.Exponent));

        var ext = nc.Exponent % 3;
        var nMan = nc.Mantissa * Math.Pow(10, ext);
        var nExp = nc.Exponent - ext;
        if (expExp <= beforeSciCutExponent)
        {
            return sb.Append($"{nMan:##0.##}{e}{nExp:###,###}");
        }

        var expMan = nExp / Math.Pow(10, expExp);
        if (expExp <= 15) sb.Append($"{nMan:##0.00}");
        sb.Append(e);

        var expExt = expExp % 3;
        var nExpMan = expMan * Math.Pow(10, expExt);
        var nExpExp = expExp - expExt;
        if (cutOff1E && expMan == 1) return sb.Append($"{e}{nExpExp:###,###}");
        return sb.Append($"{nExpMan:##0.00}{e}{nExpExp:###,###}");
    }
}