using System;
using System.Collections.Generic;

public class EternalNumber
{
    // https://github.com/Patashu/break_eternity.js/blob/master/break_eternity.js
    // https://github.com/Patashu/break_eternity.js/blob/7f5fb2d3e924cc7290dc409f1a5d2fb5e33e5065/break_eternity.js#L871
    
    public const int NumberExpMax = 308;
    public const int NumberExpMin = -324;
    public const float FirstNegLayer = 1 / 9e15f;
    public const float ExpLimit = 9e15f;
    public static readonly double LayerDown = Math.Log10(9e15);

    public int sign;
    public double layer;
    public double mag;

    public double Mantissa
    {
        get
        {
            if (sign == 0) return 0;
            return sign * layer switch
            {
                0 => mag == 5e-324 ? 5 : mag / Math.Pow(10, Math.Floor(Math.Log10(mag))),
                1 => Math.Pow(10, mag - Math.Floor(mag)),
                _ => 1
            };
        }
        set
        {
            if (layer <= 2) FromMantissaExponent(value, Exponent);
            else
            {
                sign = Math.Sign(value);
                if (sign == 0) layer = Exponent;
            }
        }
    }

    public double Exponent
    {
        get
        {
            if (sign == 0) return 0;
            return layer switch
            {
                0 => Math.Floor(Math.Log10(mag)),
                1 => Math.Floor(mag),
                2 => Math.Floor(Math.Sign(mag) * Math.Pow(10, Math.Abs(mag))),
                _ => mag * double.PositiveInfinity
            };
        }
        set => FromMantissaExponent(Mantissa, value);
    }

    public EternalNumber(double layer = 0, double mag = 0, int sign = 0)
    {
        (this.layer, this.mag, this.sign) = (layer, mag, sign);
        Normalize();
    }
    
    public void Normalize()
    {
        if (sign == 0 || mag == 0 && layer == 0)
        {
            layer = mag = sign =0;
            return;
        }

        if (layer == 0 && mag < 0)
        {
            mag = -mag;
            sign = -sign;
        }

        if (layer == 0 && mag < FirstNegLayer)
        {
            layer++;
            mag = Math.Log10(mag);
            return;
        }

        var absmag = Math.Abs(mag);
        var signmag = Math.Sign(mag);

        if (absmag >= ExpLimit)
        {
            layer++;
            mag = signmag * Math.Log10(absmag);
            return;
        }

        while (absmag < LayerDown && layer > 0)
        {
            layer--;
            if (layer == 0) mag = Math.Pow(10, mag);
            else
            {
                mag = signmag * Math.Pow(10, absmag);
                absmag = Math.Abs(mag);
                signmag = Math.Sign(mag);
            }
        }

        if (layer != 0) return;
        switch (mag)
        {
            case < 0:
                mag = -mag;
                sign = -sign;
                break;
            case 0:
                sign = 0;
                break;
        }
    }

    public void FromMantissaExponent(double mantissa, double exponent)
    {
        layer = 1;
        sign = Math.Sign(mantissa);
        mag = exponent + Math.Log10(mantissa);
        Normalize();
    }
}