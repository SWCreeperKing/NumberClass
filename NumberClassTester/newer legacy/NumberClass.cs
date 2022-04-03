using System;
using System.Text;

namespace NumberClassTester.newer_legacy
{
    public class NumberClass
    {
        public static readonly double W = Math.Exp(Math.Exp(-1.0));
        public static readonly NumberClass Two = new(2);
        public static readonly NumberClass NegativeOne = new(-1);
        public static readonly NumberClass Zero = new();
        public static readonly NumberClass Milli = new(0.001);
        public static readonly NumberClass One = new(1);
        public static readonly NumberClass Half = new(0.5);
        public static readonly NumberClass Four = new(4);
        public static readonly NumberClass Ten = new(10);
        public static readonly NumberClass Thousand = new(1000);
        public static readonly NumberClass TenThousand = new(10000);
        public static readonly NumberClass MaxInt = new("1e15");
        public static readonly NumberClass TenBillions = new("1e10");
        public static readonly NumberClass MaxDouble = new("1e300");
        public static readonly NumberClass E2000 = new(1, 2000);
        public static readonly NumberClass E3003 = new(1, 3003);
        public static readonly NumberClass MaxManti = new("1ee16");
        public static readonly NumberClass MaxExpo = new(1, 10, 1e16);
        public static readonly NumberClass MaxNumberClass = new(9, 9.999e99, double.PositiveInfinity);

        public static readonly Formatter formatter = new();
        public static double[]? doubles;

        public bool IsTrueInfinite => expHeight.IsInfinite();
        public bool IsZero => mantissa == 0;
        public bool IsOne => mantissa == 1 && exponent == 0;

        public double RealExponent =>
            expHeight == 1 && exponent > 300 || expHeight != 1 ? exponent : Math.Pow(10, exponent);

        public double mantissa;
        public double exponent;
        public double expHeight;

        public NumberClass(NumberClass nc) => SetTo(nc);

        public NumberClass(double mantissa = 0, double exponent = 0, double expHeight = 0)
        {
            InitCheck();
            (this.mantissa, this.exponent, this.expHeight) = (mantissa, exponent, expHeight);
            Update();
        }

        public NumberClass(string text)
        {
            InitCheck();
            var parts = text.Replace(",", "").Split('e');
            (mantissa, exponent, expHeight) = parts.Length switch
            {
                1 => (parts[0].Tpdd(), 0, 0),
                2 => (parts[0].Tpdd(), parts[1].Tpdd(), 0),
                _ => (1, parts[^1].Tpdd(), parts.Length - 2)
            };
            Update();
        }

        public void InitCheck()
        {
            if (doubles is not null) return;
            doubles = new double[617];
            for (var i = -308; i <= 308; i++) doubles[i + 308] = Math.Pow(10, i);
        }

        public void Update()
        {
            Sanitize();
            UpdateMantissa();
            UpdateExponent();
            Sanitize();
        }

        private void Sanitize()
        {
            if (!mantissa.IsFinite() && exponent.IsFinite() && !double.IsNaN(expHeight))
            {
                mantissa = exponent = expHeight = 0;
            }
        }

        private void UpdateMantissa()
        {
            if (mantissa < 0) mantissa = exponent = expHeight = 0;
            var shift = 0d;
            switch (mantissa)
            {
                case 0:
                    exponent = expHeight = 0;
                    return;
                case < 1 or >= 10:
                    shift = Math.Floor(Math.Log10(mantissa));
                    break;
            }

            if (shift == 0) return;
            
            mantissa *= doubles[(int) (308 - shift)];
            switch (expHeight)
            {
                case 0:
                    exponent += shift;
                    break;
                case 1:
                    if (shift > 1) exponent += Math.Log10(shift);
                    break;
                case -1:
                    exponent += Math.Pow(10, shift);
                    break;
            }
        }

        private void UpdateExponent()
        {
            switch (exponent)
            {
                case 0:
                    expHeight = 0;
                    return;
                case < 100 when expHeight > 0:
                    exponent = Math.Pow(10, exponent);
                    expHeight--;
                    if (expHeight == 0 && exponent < 1e17)
                    {
                        mantissa = Math.Pow(10, exponent % 1);
                        exponent = Math.Floor(exponent);
                    }

                    break;
                case >= 1e100:
                    IncreaseHeight();
                    exponent = Math.Log10(exponent);
                    break;
                default:
                    return;
            }

            switch (exponent)
            {
                case < 100 when expHeight > 0:
                    exponent = Math.Pow(10.0, exponent);
                    expHeight--;
                    if (expHeight == 0.0 && exponent < 1e17)
                    {
                        mantissa = Math.Pow(10.0, exponent % 1);
                        exponent = Math.Floor(exponent);
                    }

                    break;
                case >= 1e100:
                    throw new ArgumentException("Exponent is >= e100? that's odd (#1)");
                default:
                    return;
            }

            switch (exponent)
            {
                case < 100 when expHeight > 0:
                    exponent = Math.Pow(10.0, exponent);
                    expHeight--;
                    if (expHeight == 0.0 && exponent < 1e17)
                    {
                        mantissa = Math.Pow(10.0, exponent % 1);
                        exponent = Math.Floor(exponent);
                    }

                    break;
                case >= 1e100:
                    throw new ArgumentException("Exponent is >= e100? that's odd (#2)");
                default:
                    return;
            }

            switch (exponent)
            {
                case < 100 when expHeight > 0:
                    throw new ArgumentException("exponent is < 100 but expHeight is > 0? odd");
                case >= 1e100:
                    throw new ArgumentException("Exponent is >= e100? that's odd (#3)");
            }
        }

        private void IncreaseHeight(double d)
        {
            expHeight += d;
            Update();
        }

        public void DecreaseHeight(double val)
        {
            expHeight -= val;
            NormalizeExponent();
            Update();
        }

        private void IncreaseHeight() => expHeight++;
        public double GetPseudoHeight() => expHeight + PseudoExponent();
        public double PseudoExponent() => exponent >= 100 ? Math.Sqrt(Math.Log10(exponent) - 2) / 98 : 0;
        public void SetTo(NumberClass nc) => (mantissa, exponent, expHeight) = nc;
        public NumberClass Max(NumberClass nc) => nc > this ? nc : this;
        public NumberClass Min(NumberClass nc) => nc < this ? nc : this;

        public void SetToMax(NumberClass nc)
        {
            if (nc > this) SetTo(nc);
        }

        public void SetToMin(NumberClass nc)
        {
            if (nc < this) SetTo(nc);
        }

        public float PercentCompare(NumberClass nc)
        {
            return nc.IsTrueInfinite || nc.IsZero
                ? 0f
                : this >= nc
                    ? 1f
                    : nc >= MaxInt && this >= Ten
                        ? expHeight == 0 && nc.expHeight == 0
                            ? (float) ((exponent + Math.Log10(mantissa)) / (nc.exponent + Math.Log10(nc.mantissa)))
                            : expHeight + 1 < nc.expHeight
                                ? new NumberClass(GetPseudoHeight()).PercentCompare(nc.GetPseudoHeight())
                                : expHeight + 1 == nc.expHeight
                                    ? nc.exponent > 10000
                                        ? (float) (Math.Log10(Math.Log10(exponent)) / Math.Log10(nc.exponent))
                                        : (float) (Math.Log10(exponent) / nc.exponent)
                                    : expHeight == nc.expHeight
                                        ? nc.expHeight == 0
                                            ? nc.exponent > 10000
                                                ? (float) (Math.Log10(exponent + Math.Log10(mantissa)) /
                                                           Math.Log10(nc.exponent + Math.Log10(nc.mantissa)))
                                                : (float) ((exponent + Math.Log10(mantissa)) /
                                                           (nc.exponent + Math.Log10(nc.mantissa)))
                                            : nc.exponent > 10000
                                                ? (float) (Math.Log10(exponent) / Math.Log10(nc.exponent))
                                                : (float) (exponent / nc.exponent)
                                        : new NumberClass(GetPseudoHeight()).PercentCompare(nc.GetPseudoHeight())
                        : (float) (this / nc);
        }

        public void NormalizeExponent()
        {
            if (expHeight != 0) return;
            var reminder = exponent % 1;
            if (!(reminder > 0)) return;
            exponent -= reminder;
            mantissa = Math.Pow(10, reminder);
        }

        public NumberClass AddToHeight(NumberClass r, double toExp)
        {
            if (expHeight > 0)
            {
                NumberClass rExponent;
                var delta = r.expHeight + toExp - expHeight;
                var expMin = Math.Min(r.expHeight, expHeight);
                var hToAdd = 0d;
                var mantissa = this.mantissa;
                var exponent = this.exponent;
                if (expMin == 0)
                {
                    if (!(delta > -2)) return new NumberClass(mantissa, exponent, expHeight + hToAdd);
                    switch ((int) delta)
                    {
                        case -1:
                            rExponent = Ten.Power(this.exponent) + r;
                            hToAdd--;
                            break;
                        case 0:
                            rExponent = r + this.exponent;
                            break;
                        case 1:
                            rExponent = r + Math.Log10(this.exponent);
                            hToAdd++;
                            break;
                        case 2:
                            rExponent = r + Math.Log10(Math.Log10(this.exponent));
                            hToAdd += 2;
                            break;
                        case 3:
                            var val11 = Math.Log10(Math.Log10(Math.Log10(this.exponent)));
                            rExponent = r + val11;
                            hToAdd += 3;
                            break;
                        case 4:
                            var val1 = Math.Log10(Math.Log10(Math.Log10(Math.Log10(this.exponent))));
                            switch (val1)
                            {
                                case double.NaN:
                                case double.NegativeInfinity:
                                    return this;
                                case < 0 when r > -val1:
                                    rExponent = r + val1;
                                    hToAdd += 4;
                                    break;
                                case < 0:
                                    val1 += (double) r;
                                    val1 = Math.Pow(10.0, val1);
                                    rExponent = new NumberClass(val1);
                                    hToAdd += 3.0;
                                    break;
                                default:
                                    rExponent = r + val1;
                                    hToAdd += 4;
                                    break;
                            }

                            break;

                        default:
                            return this;
                    }

                    hToAdd += rExponent.expHeight;
                    rExponent.expHeight = 0;
                    if (rExponent > MaxDouble)
                    {
                        hToAdd++;
                        exponent = (double) rExponent.Log10();
                    }
                    else exponent = (double) rExponent;

                    return new NumberClass(mantissa, exponent, expHeight + hToAdd);
                }

                NumberClass r2;
                if (expMin == 1 && delta == 0)
                {
                    var r1 = new NumberClass(this);
                    r2 = new NumberClass(r);
                    r1.mantissa = 1;
                    r1.expHeight -= toExp + 1;
                    r1.NormalizeExponent();
                    r2.NormalizeExponent();
                    r1 += r2;
                    r1.expHeight += toExp + 1;
                    return r1;
                }

                if (delta > 0)
                {
                    mantissa = r.mantissa;
                    exponent = r.exponent;
                    hToAdd += delta;
                    return new NumberClass(mantissa, exponent, expHeight + hToAdd);
                }

                r2 = new NumberClass(r);
                r2.expHeight += toExp + 1;
                return Max(r2);
            }
            else
            {
                NumberClass rExponent;
                var delta = r.expHeight + toExp;
                var hToAdd = 0d;
                var mantissa = this.mantissa;
                var exponent = this.exponent;
                double normalizedExponent;
                switch ((int) delta)
                {
                    case 0:
                        normalizedExponent = this.exponent;
                        break;
                    case 1:
                        normalizedExponent = Math.Log10(this.exponent);
                        hToAdd = 1;
                        break;
                    case 2:
                        normalizedExponent = Math.Log10(Math.Log10(this.exponent));
                        hToAdd = 2;
                        break;
                    case 3:
                        normalizedExponent = Math.Log10(Math.Log10(Math.Log10(this.exponent)));
                        hToAdd = 3;
                        break;
                    case 4:
                        normalizedExponent = Math.Log10(Math.Log10(Math.Log10(Math.Log10(this.exponent))));
                        hToAdd = 4;
                        break;
                    default:
                        normalizedExponent = double.NaN;
                        break;
                }

                switch (normalizedExponent)
                {
                    case double.NaN:
                    case double.NegativeInfinity:
                        return this;
                    case < 0 when r > -normalizedExponent:
                        rExponent = r + normalizedExponent;
                        break;
                    case < 0:
                        normalizedExponent += (double) r;
                        normalizedExponent = Math.Pow(10.0, normalizedExponent);
                        rExponent = new NumberClass(normalizedExponent);
                        hToAdd--;
                        break;
                    default:
                        rExponent = r + normalizedExponent;
                        break;
                }

                hToAdd += rExponent.expHeight;
                rExponent.expHeight = 0;
                if (rExponent > MaxDouble)
                {
                    hToAdd++;
                    exponent = (double) rExponent.Log10();
                }
                else exponent = (double) rExponent;

                return new NumberClass(mantissa, exponent, expHeight + hToAdd);
            }
        }

        public NumberClass AddToHeight(double d, double toExp)
        {
            if (expHeight > 0)
            {
                NumberClass rExponent;
                var delta = toExp - expHeight;
                var hToAdd = 0d;
                var mantissa = this.mantissa;
                var exponent = this.exponent;
                if (!(delta > -2)) return new NumberClass(mantissa, exponent, expHeight + hToAdd);
                switch ((int) delta)
                {
                    case -1:
                        rExponent = Ten.Power(this.exponent) + d;
                        hToAdd--;
                        break;
                    case 0:
                        rExponent = new NumberClass(d + this.exponent);
                        break;
                    case 1:
                        rExponent = new NumberClass(d + Math.Log10(this.exponent));
                        hToAdd++;
                        break;
                    case 2:
                        rExponent = new NumberClass(d + Math.Log10(Math.Log10(this.exponent)));
                        hToAdd += 2;
                        break;
                    case 3:
                        rExponent = new NumberClass(d + Math.Log10(Math.Log10(Math.Log10(this.exponent))));
                        hToAdd += 3;
                        break;

                    case 4:
                        var val1 = Math.Log10(Math.Log10(Math.Log10(Math.Log10(this.exponent))));
                        switch (val1)
                        {
                            case double.NaN:
                            case double.NegativeInfinity:
                                return this;
                            case < 0 when d > -val1:
                                rExponent = new NumberClass(d + val1);
                                hToAdd += 4;
                                break;
                            case < 0:
                                val1 += d;
                                val1 = Math.Pow(10.0, val1);
                                rExponent = new NumberClass(val1);
                                hToAdd += 3;
                                break;
                            default:
                                rExponent = new NumberClass(d + val1);
                                hToAdd += 4;
                                break;
                        }

                        break;
                    default: return this;
                }

                hToAdd += rExponent.expHeight;
                rExponent.expHeight = 0;
                if (rExponent > MaxDouble)
                {
                    hToAdd++;
                    exponent = (double) rExponent.Log10();
                }
                else exponent = (double) rExponent;

                return new NumberClass(mantissa, exponent, expHeight + hToAdd);
            }
            else
            {
                NumberClass rExponent;
                var hToAdd = 0d;
                var mantissa = this.mantissa;
                var exponent = this.exponent;
                double normalizedExponent;
                switch ((int) toExp)
                {
                    case 0:
                        normalizedExponent = this.exponent;
                        break;
                    case 1:
                        normalizedExponent = Math.Log10(this.exponent);
                        hToAdd = 1;
                        break;
                    case 2:
                        normalizedExponent = Math.Log10(Math.Log10(this.exponent));
                        hToAdd = 2;
                        break;
                    case 3:
                        normalizedExponent = Math.Log10(Math.Log10(Math.Log10(this.exponent)));
                        hToAdd = 3;
                        break;
                    case 4:
                        normalizedExponent = Math.Log10(Math.Log10(Math.Log10(Math.Log10(this.exponent))));
                        hToAdd = 4;
                        break;
                    default:
                        normalizedExponent = double.NaN;
                        break;
                }

                switch (normalizedExponent)
                {
                    case double.NaN:
                    case double.NegativeInfinity:
                        return this;
                    case < 0 when d > -normalizedExponent:
                        rExponent = new NumberClass(d + normalizedExponent);
                        break;
                    case < 0:
                        normalizedExponent += d;
                        normalizedExponent = Math.Pow(10.0, normalizedExponent);
                        rExponent = new NumberClass(normalizedExponent);
                        hToAdd--;
                        break;
                    default:
                        rExponent = new NumberClass(d + normalizedExponent);
                        break;
                }

                hToAdd += rExponent.expHeight;
                rExponent.expHeight = 0;
                if (rExponent > MaxDouble)
                {
                    hToAdd++;
                    exponent = (double) rExponent.Log10();
                }
                else exponent = (double) rExponent;

                return new NumberClass(mantissa, exponent, expHeight + hToAdd);
            }
        }

        public NumberClass AddToPseudoHeight(NumberClass nc) => AddToPseudoHeight((double) nc);

        public NumberClass AddToPseudoHeight(double d)
        {
            var pseudoH = d;
            pseudoH += PseudoExponent();
            var r = new NumberClass(this);
            if (pseudoH > 1)
            {
                if (pseudoH < long.MaxValue)
                {
                    long extraH;
                    var ratio = pseudoH % 1;
                    extraH = (long) Math.Round(pseudoH - ratio, 0);
                    r.expHeight += extraH;
                    r.exponent = ToPseudoExponent(ratio);
                }
                else r.expHeight += d;
            }
            else r.exponent = ToPseudoExponent(pseudoH);

            r.Update();
            return r;
        }

        public double ToPseudoExponent(double ratio) => Math.Pow(10, 98 * (ratio * ratio) + 2);

        public NumberClass MultiplyExponentByPonticon(double d)
        {
            if (!(expHeight > 0)) return this;
            var r = new NumberClass(this);
            r.Update();
            if (Math.Pow(d, Math.Log10(r.exponent)).IsFinite()) r.exponent *= Math.Pow(d, Math.Log10(r.exponent));
            else
            {
                var toMult = new NumberClass(d).Power(Math.Log10(r.exponent));
                toMult.Update();
                r.expHeight += toMult.expHeight;
                if (Math.Pow(10, toMult.exponent).IsFinite())
                {
                    r.exponent *= toMult.mantissa * Math.Pow(10, toMult.exponent);
                }
                else
                {
                    var multAgain = Ten.Power(toMult.exponent) * toMult.mantissa;
                    r.IncreaseHeight();
                    multAgain = multAgain.Log10();
                    r.expHeight *= multAgain.mantissa * Math.Pow(10.0, exponent);
                }
            }

            r.Update();
            return r;
        }

        public void DefloatMantissa(int keepDigits)
        {
            mantissa = mantissa.Defloat(keepDigits);
            UpdateMantissa();
        }

        public NumberClass RoundToMultipleOf(double number) => RoundToDigits(Math.Log10(number));

        public NumberClass RoundToDigits(double digits)
        {
            return !(expHeight > 0 || exponent - 16 > digits)
                ? new NumberClass(RoundToDigits(mantissa * Math.Pow(10.0, exponent), digits))
                : this;
        }

        public double RoundToDigits(double val, double digits)
        {
            return Math.Round(val * Math.Pow(10, digits)) / Math.Pow(10.0, digits);
        }

        public void Deconstruct(out double mantissa, out double exponent, out double expHeight)
        {
            (mantissa, exponent, expHeight) = (this.mantissa, this.exponent, this.expHeight);
        }

        #region Arithmetic Operators

        public static NumberClass operator ++(NumberClass nc) => nc += 1;
        public static NumberClass operator --(NumberClass nc) => nc -= 1;

        public static NumberClass operator +(NumberClass nc1, NumberClass nc2)
        {
            var r = new NumberClass(nc1);
            if (r.expHeight > 1 || nc2.expHeight > 1 || r.expHeight == 1.0 && r.exponent > 15 ||
                nc2.expHeight == 1 && nc2.exponent > 15 || Math.Abs(nc1.RealExponent - nc2.RealExponent) > 15 ||
                r.exponent == 0 && Math.Abs(r.expHeight - nc2.expHeight) > 0)
            {
                r.SetToMax(nc2);
            }
            else
            {
                var delta = (int) Math.Round(r.RealExponent - nc2.RealExponent);
                var potato = Pow10(-delta);
                r.mantissa += nc2.mantissa * potato;
                r.UpdateMantissa();
            }

            return r;
        }

        public static NumberClass operator -(NumberClass nc1, NumberClass nc2)
        {
            if (nc2 >= nc1) return Zero;

            if (nc1.expHeight > 1 || nc2.expHeight > 1 || nc1.expHeight == 1 && nc1.exponent > 15 ||
                nc2.expHeight == 1 && nc2.exponent > 15 ||
                Math.Abs(nc1.RealExponent - nc2.RealExponent) > 15)
            {
                return nc1;
            }

            var r = new NumberClass(nc1);
            r.mantissa -= nc2.mantissa / Pow10((int) (nc1.RealExponent - nc2.RealExponent));
            r.UpdateMantissa();
            return r;
        }

        public static NumberClass operator *(NumberClass nc1, NumberClass nc2)
        {
            if (nc2.IsOne) return nc1;
            if (nc2.IsZero || nc1.IsZero) return Zero;
            if (nc1.expHeight > 1 || nc2.expHeight > 1) return nc1.Max(nc2);

            NumberClass dupe;
            switch (nc1.expHeight + nc2.expHeight)
            {
                case 0:
                    dupe = new NumberClass(nc1);
                    dupe.mantissa *= nc1.mantissa;
                    dupe.exponent += nc1.exponent;
                    dupe.Update();
                    return dupe;
                case 1:
                    if (nc1.expHeight == 1)
                    {
                        dupe = new NumberClass(nc1);
                        var tExponent = Math.Log10(nc2.exponent + Math.Log10(nc2.mantissa));
                        if (Math.Abs(nc1.exponent - tExponent) < 10)
                        {
                            dupe.exponent += Math.Log10(1 + Math.Pow(10, tExponent - nc1.exponent));
                        }
                        else if (nc1.exponent < tExponent) dupe.exponent = tExponent;

                        dupe.mantissa *= nc2.mantissa;
                        dupe.Update();
                        return dupe;
                    }
                    else
                    {
                        dupe = new NumberClass(nc2);
                        var tExponent = Math.Log10(nc1.exponent + Math.Log10(nc1.mantissa));
                        if (Math.Abs(nc2.exponent - tExponent) < 10)
                        {
                            dupe.exponent += Math.Log10(1 + Math.Pow(10, tExponent - nc2.exponent));
                        }
                        else if (nc2.exponent < tExponent) dupe.exponent = tExponent;

                        dupe.mantissa *= nc2.mantissa;
                        dupe.Update();
                        return dupe;
                    }
                case 2:
                    dupe = new NumberClass(nc1);
                    if (Math.Abs(nc1.exponent - nc2.exponent) < 10)
                    {
                        dupe.exponent += Math.Log10(1 + Math.Pow(10, nc2.exponent - nc1.exponent));
                    }
                    else if (nc1.exponent < nc2.exponent) dupe.exponent = nc2.exponent;

                    dupe.mantissa *= nc2.mantissa;
                    dupe.Update();
                    return dupe;
                default:
                    return nc1.Max(nc2);
            }
        }

        public static NumberClass operator /(NumberClass nc1, NumberClass nc2)
        {
            var divided = new NumberClass(nc1);
            var divider = new NumberClass(nc2);

            double dividerExponent;
            if (divider.IsOne) return nc1;
            if (divider.IsZero || divided.IsZero) return Zero;
            if (divided == divider) return One;
            if (nc1.expHeight > 1 || divider.expHeight > 1 || Math.Abs(nc1.expHeight - divider.expHeight) > 1)
            {
                divided.mantissa /= divider.mantissa;
                return divided > divider ? divided : Zero;
            }

            if (nc1.expHeight == 1 && divider.expHeight == 0)
            {
                dividerExponent = Math.Log10(divider.exponent + Math.Log10(divider.mantissa));
                if (Math.Abs(nc1.exponent - dividerExponent) < 10)
                    divided.exponent -= Math.Log10(1 + Math.Pow(10, dividerExponent - divided.exponent));
                else if (divider > divided) return Zero;
                divided.mantissa /= divider.mantissa;
            }
            else if (nc1.expHeight == 0 && divider.expHeight == 1)
            {
                dividerExponent = Math.Log10(divided.exponent + Math.Log10(divided.mantissa));
                if (Math.Abs(divider.exponent - dividerExponent) < 10)
                    divider.exponent -= Math.Log10(1 + Math.Pow(10, dividerExponent - divider.exponent));
                else if (divider >= divided) return Zero;

                divider.mantissa /= divider.mantissa;
                divided.SetTo(divider);
            }
            else if (nc1.expHeight == 1 && divider.expHeight == 1)
            {
                if (Math.Abs(divided.exponent - divider.exponent) < 10)
                    divided.exponent -= Math.Log10(1 + Math.Pow(10, divider.exponent - divided.exponent));
                else if (divider > divided) return Zero;

                divided.mantissa /= divider.mantissa;
            }
            else
            {
                divided.mantissa /= divider.mantissa;
                divided.exponent -= divider.exponent;
            }

            divided.Update();
            return divided;
        }

        public NumberClass Pow(NumberClass nc) => Power(nc);

        public NumberClass Power(NumberClass nc)
        {
            var tMantissa = mantissa;
            var tExponent = 0d;
            var tHeight = expHeight;
            if (nc.IsOne || IsOne || IsZero) return this;
            if (nc.IsZero) return One;
            NumberClass r;
            switch ((int) expHeight)
            {
                case 0:
                    if (exponent == 0 && nc.exponent == 0) tMantissa = Math.Pow(mantissa, nc.mantissa);
                    else if (nc.expHeight == 0 && nc.exponent < 308)
                    {
                        var compactedExponent = exponent + Math.Log10(mantissa);
                        tExponent = compactedExponent * (double) nc;
                        switch (tExponent)
                        {
                            case < 1e17:
                                var v1 = tExponent % 1;
                                tMantissa = Math.Pow(10, v1);
                                tExponent -= v1;
                                break;
                            case > 1e308:
                                tExponent = Math.Log10(compactedExponent) + nc.exponent + Math.Log10(nc.mantissa);
                                tHeight++;
                                break;
                        }
                    }
                    else
                        switch (nc.expHeight)
                        {
                            case 0:
                                tExponent = Math.Log10(exponent + Math.Log10(mantissa)) + nc.exponent +
                                            Math.Log10(nc.mantissa);
                                tHeight++;
                                break;
                            case 1:
                                r = Log10() * nc;
                                r.expHeight++;
                                return r;
                            default:
                                r = new NumberClass(nc);
                                r.expHeight++;
                                return r;
                        }

                    break;
                case 1:
                    switch (nc.expHeight)
                    {
                        case 0:
                            tExponent = exponent + Math.Log10(nc.mantissa) + nc.exponent;
                            break;
                        case 1:
                            r = Log10() * nc;
                            r.expHeight++;
                            return r;
                        default:
                            r = new NumberClass(nc);
                            r.expHeight++;
                            return r;
                    }

                    break;

                case 2:
                    switch (nc.expHeight)
                    {
                        case 0:
                            tExponent = exponent;
                            break;
                        case 1:
                            r = Log10() * nc;
                            r.expHeight++;
                            return r;

                        default:
                            r = new NumberClass(nc);
                            r.expHeight++;
                            return r;
                    }

                    break;


                default:
                    if (nc.expHeight < expHeight)
                    {
                        if (expHeight - nc.expHeight == 1.0)
                        {
                            if (exponent > nc.exponent) return this;
                            tExponent = nc.exponent;
                        }
                        else return this;
                    }
                    else
                    {
                        r = new NumberClass(nc);
                        r.expHeight++;
                        return r;
                    }

                    break;
            }

            return new NumberClass(tMantissa, tExponent, tHeight);
        }

        public static double Pow10(int i) => doubles[308 + i];

        public NumberClass Log10()
        {
            if (One > this) return new NumberClass();
            var reInfinite = new NumberClass(this);
            switch (reInfinite.expHeight)
            {
                case > 0:
                    reInfinite.expHeight--;
                    if (reInfinite.expHeight == 0.0 && exponent < 1e17)
                    {
                        reInfinite.mantissa = Math.Pow(10, exponent % 1);
                        reInfinite.exponent = Math.Floor(exponent);
                    }

                    break;
                case 0:
                    reInfinite.mantissa = reInfinite.exponent + Math.Log10(reInfinite.mantissa);
                    reInfinite.exponent = 0.0;
                    break;
            }

            reInfinite.Update();
            return reInfinite;
        }

        public NumberClass LogX(NumberClass x) => One > this ? new NumberClass() : Log10() / x.Log10();

        public NumberClass Floor()
        {
            return this > MaxInt ? this : new NumberClass(Math.Floor(mantissa * Math.Pow(10, exponent)));
        }

        public NumberClass Tetrate(double d)
        {
            var val1 = d;
            if (this == Zero || val1 == 0.0) return this;
            if (this == One || val1 == 1.0) return this;
            if (val1 < 1e16) val1 = Math.Round(val1);

            var ttp = 500d;
            var num = (double) this;
            var result = new NumberClass(this);
            if (num <= W - 0.01 && val1 > 99) return new NumberClass(TetrateDouble(num, val1));
            int maxVal;
            if (num <= W)
            {
                var res = num;
                var lastRes = 0d;
                var converged = false;
                maxVal = (int) Math.Min(val1, ttp);
                for (var i = 1; i < maxVal; i++)
                {
                    res = Math.Pow(num, res);
                    if (res == lastRes)
                    {
                        converged = true;
                        break;
                    }

                    lastRes = res;
                }

                return new NumberClass(converged || maxVal == val1 ? res : TetrateDouble(num, val1));
            }

            if (num <= W + 0.001)
            {
                if (val1 > 1e14)
                {
                    result.exponent = 10;
                    result.IncreaseHeight(val1 - 1);
                    return result;
                }

                var res = num;
                var converged = false;
                maxVal = (int) Math.Min(val1, ttp);
                for (var i = 1; i < maxVal; i++)
                {
                    res = Math.Pow(num, res);
                    val1--;
                    if (res <= 10) continue;
                    converged = true;
                    break;
                }

                if (!converged || val1 == 1.0) return new NumberClass(res);
                result = new NumberClass(res);
            }

            if (val1 > 1e14)
            {
                result.exponent = 10;
                result.IncreaseHeight(val1 - 1);
                return result;
            }

            maxVal = (int) Math.Min(val1, ttp);
            for (var i = 1; i < maxVal; i++)
            {
                result.SetTo(Power(result));
                val1--;
                if (!(result.expHeight > 1)) continue;
                result.IncreaseHeight(val1 - 1);
                break;
            }

            return result;
        }

        public double TetrateDouble(double x, double n)
        {
            var y = x * x;
            double z;
            var logx = Math.Log(x);
            if (x > 1.428) y = 2.718281828459045 - 5.27318144255 * Math.Sqrt(1.444667861009766 - x);
            for (var k = 0; k <= 4; k++)
            {
                z = Math.Pow(x, y);
                y -= (z - y) / (z * logx - 1);
            }

            return y + (x - y) * Math.Pow(Math.Log(y), n - 1) / -Math.Log(W - x);
        }

        #endregion

        #region Comparison Operators

        public static bool operator >(NumberClass nc1, NumberClass nc2)
        {
            return nc1.expHeight == nc2.expHeight
                ? nc1.exponent == nc2.exponent
                    ? nc1.mantissa > nc2.mantissa
                    : nc1.exponent > nc2.exponent
                        ? nc2.mantissa == 0
                        : nc1.mantissa != 0
                : nc1.expHeight > nc2.expHeight;
        }

        public static bool operator >=(NumberClass nc1, NumberClass nc2)
        {
            return nc1.expHeight == nc2.expHeight
                ? nc1.exponent == nc2.exponent
                    ? nc1.mantissa >= nc2.mantissa
                    : nc2.exponent > nc1.exponent
                        ? nc2.mantissa == 0
                        : nc1.mantissa != 0
                : nc1.expHeight > nc2.expHeight;
        }

        public static bool operator ==(NumberClass nc1, NumberClass nc2)
        {
            return (nc1!.mantissa, nc1.exponent, nc1.expHeight) == (nc2!.mantissa, nc2.exponent, nc2.expHeight);
        }

        public static bool operator <(NumberClass nc1, NumberClass nc2) => !(nc1 >= nc2);
        public static bool operator <=(NumberClass nc1, NumberClass nc2) => !(nc1 < nc2);
        public static bool operator !=(NumberClass nc1, NumberClass nc2) => !(nc1 == nc2);

        #endregion

        #region Parse Operators

        public double ToSafeDouble()
        {
            switch (expHeight)
            {
                case > 1:
                    return double.MaxValue;
                case > 0:
                    return mantissa * Math.Pow(10, Math.Pow(10, exponent));
            }

            if (!(exponent < 17)) return exponent > 307 ? double.MaxValue : mantissa * Math.Pow(10, exponent);
            var defloater = Math.Pow(10, 12 - exponent);
            return defloater < 1
                ? (long) Math.Round(mantissa * Math.Pow(10, exponent), 0)
                : (long) Math.Round(mantissa * Math.Pow(10, exponent) * defloater, 0) / defloater;
        }

        public static explicit operator int(NumberClass nc)
        {
            return nc.expHeight > 0
                ? int.MaxValue
                : nc.exponent > 9
                    ? int.MaxValue
                    : (int) Math.Round(nc.mantissa * Math.Pow(10, nc.exponent), 0);
        }

        public static explicit operator long(NumberClass nc)
        {
            return nc.expHeight > 0
                ? long.MaxValue
                : nc.exponent > 19
                    ? long.MaxValue
                    : (long) Math.Round(nc.mantissa * Math.Pow(10, nc.exponent), 0);
        }

        public static explicit operator float(NumberClass nc)
        {
            return nc.expHeight > 0 || nc.exponent > 38
                ? float.MaxValue
                : (float) (nc.mantissa * Math.Pow(10, nc.exponent));
        }

        public static explicit operator double(NumberClass nc)
        {
            return nc.expHeight > 1
                ? double.MaxValue
                : nc.expHeight > 0
                    ? nc.mantissa * Math.Pow(10, Math.Pow(10, nc.exponent))
                    : nc.exponent > 307
                        ? double.MaxValue
                        : nc.mantissa * Math.Pow(10, nc.exponent);
        }

        public static explicit operator string(NumberClass nc) => nc.ToString();

        public static implicit operator NumberClass(float f) => new(f);
        public static implicit operator NumberClass(double d) => new(d);
        public static implicit operator NumberClass(string s) => new(s);

        #endregion

        // public override string ToString() => formatter.GetString(UnFormattedString());
        public override string ToString() => UnFormattedString();

        public string UnFormattedString()
        {
            StringBuilder sb = new();
            sb.Append(mantissa);
            if (exponent == 0) return sb.ToString();
            sb.Append('_');
            sb.Append(exponent);
            if (expHeight == 0) return sb.ToString();
            sb.Append('_');
            sb.Append(expHeight);
            return sb.ToString();
        }
    }

    public static class ReHelp
    {
        // try parse double default
        public static double Tpdd(this string s, double def = 0) => double.TryParse(s, out var d) ? d : def;
        public static bool IsFinite(this double d) => Math.Abs(d) <= 1.7976931348623157e308;
        public static bool IsInfinite(this double d) => d is double.PositiveInfinity or double.NegativeInfinity;

        public static double Defloat(this double number, int keepDigits)
        {
            return (long) Math.Round(number * NumberClass.Pow10(keepDigits)) / NumberClass.Pow10(keepDigits);
        }
    }
}