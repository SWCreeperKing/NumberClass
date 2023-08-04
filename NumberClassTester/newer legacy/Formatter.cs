using System;
using System.Collections.Generic;
using System.Text;

namespace NumberClassTester.newer_legacy
{
    public class Formatter
    {
        public enum Notation
        {
            Standered,
            Scientific,
            Engineering,
        }

        public enum ExponentMode
        {
            Arrow,
            Letter
        }

        private static readonly string[] Monofix = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No" };
        private static readonly string[] BifixBig = { "", "Dc", "Vg", "Tg", "Qag", "Qig", "Sxg", "Spg", "Ocg", "Nog" };
        private static readonly string[] BifixSmall = { "", "U", "D", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No" };

        private static readonly string[] TrifixBig =
            { "", "Ct", "Dct", "Tct", "Qact", "Qict", "Ssct", "Spct", "Occt", "Noct" };

        private static readonly char[][] StacksOfZeros = new char[16][];
        private static readonly char[][] StacksOfDottedZeros = new char[16][];

        static Formatter()
        {
            for (var i = 0; i < 16; i++)
            {
                StacksOfZeros[i] = new char[i + 1];
                for (var j = 0; j < i + 1; j++) StacksOfZeros[i][j] = '0';
            }

            for (var i = 0; i < 16; i++)
            {
                StacksOfDottedZeros[i] = new char[i + 1];
                StacksOfDottedZeros[i][0] = '.';
                for (var j = 1; j < i + 1; j++) StacksOfDottedZeros[i][j] = '0';
            }
        }

        private string _rightArrow = "&#8614;";

        private string _upArrow = "&uarr;";

        // private transient StringBuilder stringBuilder = new StringBuilder();
        private char[] _separator = ",".ToCharArray();
        private int _maxPrecision = 15;
        private int _maxDecimals = 1000;
        private Notation _notation = Notation.Scientific;
        private ExponentMode _exponentMode = ExponentMode.Letter;
        private bool _removeTrailingZero;
        private bool _isInteger;
        private bool _showInfinity;
        private bool _asHtml;

        public Formatter()
        {
        }

        public Formatter(Formatter form)
        {
            _isInteger = false;
            SetSeparator(form._separator);
            SetMaxPrecision(form._maxPrecision);
            SetNotation(form._notation);
            SetExponentMode(form._exponentMode);
            SetRemoveTrailingZero(form._removeTrailingZero);
            SetInteger(form._isInteger);
            SetShowInfinity(form._showInfinity);
            SetAsHtml(form._asHtml);
            SetUpArrow(form._upArrow);
            SetRightArrow(form._rightArrow);
            SetMaxDecimals(form._maxDecimals);
        }

        private static NumberClass CompatReInfiniteStd(NumberClass infinite, bool removeTrailingZero)
        {
            if (infinite.exponent > 1e10)
            {
                infinite.exponent = Math.Log10(infinite.exponent);
                infinite.expHeight += 1;
            }

            if (removeTrailingZero) infinite.DefloatMantissa(14);
            return infinite;
        }

        private static NumberClass LogTheExp(NumberClass infinite)
        {
            infinite.exponent = Math.Log10(infinite.exponent);
            infinite.expHeight += 1;
            return infinite;
        }

        /**
     * @param d           must be smaller than MAX.INT
     * @param maxDecimals
     */
        private static string DoubleToString(double d, int precision, bool removeTrailingZero, bool asHtml,
            int maxDecimals)
        {
            var re = (long) d;
            StringBuilder stringBuilder = new();
            stringBuilder.Append(re);
            var delta = precision - stringBuilder.Length;

            if (delta <= 0) return stringBuilder.ToString();
            delta = Math.Min(maxDecimals, delta);
            return AddMoreDigits(stringBuilder, d, re, delta, removeTrailingZero, asHtml).ToString();
        }

        private static StringBuilder AddMoreDigits(StringBuilder sb, double d, long re, int delta,
            bool removeTrailingZero, bool asHtml)
        {
            var rem = d - re;
            if (rem < 1e-10) rem = 0;
            else if (rem != 0) rem += +1e-15;

            var s = ((long) Math.Floor(rem * NumberClass.Pow10(delta - 1))).ToString();
            if (s == "0.0") s = string.Empty;

            delta -= s.Length + 1;
            if (s.Length == 0 && removeTrailingZero) return sb;

            sb.Append('.');
            if (delta > 0) sb.Append(StacksOfZeros[delta - 1]);

            sb.Append(s);
            if (delta < 0) sb.Length += delta;

            return removeTrailingZero ? RemoveTrailingDot(RemoveTrailingZero(sb)) : ReplaceTrailingDot(sb, asHtml);
        }

        private static String DoubleToSeparatedString(double d, int precision, int maxDecimals, char[] separator,
            bool noDecimal, bool removeTrailingZero, bool asHtml)
        {
            StringBuilder stringBuilder = new();
            var re = (long) d;
            var splitCount = (int) Math.Log10(re) / 3;
            stringBuilder.Append(re);
            int delta;
            if (splitCount < 0) delta = precision - stringBuilder.Length;
            else delta = precision - stringBuilder.Length - splitCount;

            switch (splitCount)
            {
                case 0:
                case unchecked(-int.MinValue) / 3:
                    break;
                case 1 or 2 or 3 or 4 or 5 or 6:
                    for (var i = splitCount; i > 0; i--) stringBuilder.Insert(stringBuilder.Length - i * 3, separator);
                    break;
                default:
                    throw new Exception($"Unsupported value: {splitCount}");
            }

            if (noDecimal) return stringBuilder.ToString();
            if (delta > 0)
            {
                return AddMoreDigits(stringBuilder, d, re, Math.Min(maxDecimals, delta), removeTrailingZero, asHtml)
                    .ToString();
            }

            return stringBuilder.ToString();
        }
        
        private static StringBuilder RemoveTrailingZero(StringBuilder stringBuilder)
        {
            var i = stringBuilder.Length;
            if (stringBuilder[--i] != '0') return stringBuilder;
            while (stringBuilder[--i] == '0')
            {
            }

            stringBuilder.Length = i + 1;
            return stringBuilder;
        }

        private static StringBuilder RemoveTrailingDot(StringBuilder stringBuilder)
        {
            var i = stringBuilder.Length - 1;
            if (stringBuilder[i] == '.') stringBuilder.Length = i;
            return stringBuilder;
        }

        private static StringBuilder RemoveTrailingSpace(StringBuilder stringBuilder)
        {
            var i = stringBuilder.Length - 1;
            if (i < 0) return stringBuilder;

            switch (stringBuilder[i])
            {
                case ' ':
                    stringBuilder.Length = i;
                    break;
                case ';':
                    if (stringBuilder.Length > 5) stringBuilder.Length = i - 5;
                    break;
            }

            return stringBuilder;
        }

        private static StringBuilder ReplaceTrailingDot(StringBuilder stringBuilder, bool asHtml)
        {
            var i = stringBuilder.Length - 1;

            if (stringBuilder.ToString()[i] == '.')
            {
                stringBuilder.Replace(" ", "", i, i + 1);
            }

            return stringBuilder;
        }

        private static StringBuilder RemoveInitialStar(StringBuilder stringBuilder)
        {
            if (stringBuilder[0] == '*') stringBuilder.Remove(0, 1);
            return stringBuilder;
        }

        private static String NumberToString(double number, int precision, bool isEngineering,
            bool removeTrailingZero, bool asHtml, int maxDecimals)
        {
            return NumberToString(number / Math.Pow(10, Math.Floor(Math.Log10(number))), Math.Floor(Math.Log10(number)),
                precision, isEngineering, removeTrailingZero, asHtml, maxDecimals);
        }

        /**
     * @param mantissa      is assumed between 1 and 9.9999
     * @param exponent
     * @param isEngineering
     * @param maxDecimals
     */
        private static String NumberToString(double mantissa, double exponent, int precision, bool isEngineering,
            bool removeTrailingZero, bool asHtml, int maxDecimals)
        {
            if (!isEngineering) return DoubleToString(mantissa, precision, removeTrailingZero, asHtml, maxDecimals);
            var engShift = (int) (exponent % 3);
            while (engShift-- > 0) mantissa *= 10;
            return DoubleToString(mantissa, precision, removeTrailingZero, asHtml, maxDecimals);
        }

        private static String ExponentToStdAbrev(double exponent)
        {
            int monoIndexS;
            int monoIndexB;
            switch (exponent)
            {
                case < 3:
                    return "";
                case < 33:
                    var monoIndex = (int) exponent / 3;
                    return $" {Monofix[monoIndex]}";
                case < 303:
                    monoIndexS = (int) ((exponent - 3) % 30) / 3;
                    monoIndexB = (int) (exponent - 3) / 30;
                    return $" {BifixSmall[monoIndexS]}{BifixBig[monoIndexB]}";
                case < 3003:
                    monoIndexS = (int) ((exponent - 3) % 30) / 3;
                    monoIndexB = (int) ((exponent - 3) % 300) / 30;
                    var triIndex = (int) ((exponent - 3) / 300);
                    return $" {BifixSmall[monoIndexS]}{BifixBig[monoIndexB]}{TrifixBig[triIndex]}";
                default:
                    return "";
            }
        }

        private static int SeparatedStringMinLength(double d, IReadOnlyCollection<char> separator)
        {
            var v = (int) Math.Floor(Math.Log10(d)) + 1;
            return v + (v - 1) / 3 * separator.Count;
        }

        private string ToStdNumber(NumberClass nc)
        {
            var intHeight = (int) nc.expHeight;
            string stdAbrev;
            string lifter;
            string exponent;
            int lifterLen;
            double tExp;
            double tnum;
            double b;

            void Lifer(int number, string text) => (lifter, lifterLen) = (text, number);

            void DoExponent(int arrowN, string arrowS, int letterN, string letterS)
            {
                if (_exponentMode == ExponentMode.Arrow) Lifer(arrowN, arrowS);
                else Lifer(letterN, letterS);
            }

            if (nc.expHeight < (_exponentMode == ExponentMode.Letter ? 5 : 2))
            {
                switch (intHeight)
                {
                    case 0:
                        if (nc.exponent < 3003)
                        {
                            stdAbrev = ExponentToStdAbrev(nc.exponent);
                            var precision = Math.Max(0, _maxPrecision - stdAbrev.Length);
                            var mantissa = NumberToString(nc.mantissa, nc.exponent, precision, true,
                                _removeTrailingZero, _asHtml, _maxDecimals);
                            return $"{mantissa}{stdAbrev}";
                        }
                        else
                        {
                            DoExponent(5, $"*10{_upArrow}", 2, "e");

                            tExp = Math.Floor(Math.Log10(nc.exponent));
                            var newPrecision = _maxPrecision - lifterLen - 1;
                            if (!(tExp > newPrecision - newPrecision / 3))
                            {
                                lifterLen--;
                                exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision, _maxPrecision,
                                    _separator, true, _removeTrailingZero, _asHtml);
                                var precision = Math.Max(0, _maxPrecision - exponent.Length - lifterLen);
                                var mantissa = NumberToString(nc.mantissa, precision, true, _removeTrailingZero,
                                    _asHtml, _maxDecimals);
                                return $"{mantissa}{lifter}{exponent}";
                            }
                            else
                            {
                                tnum = nc.exponent / NumberClass.Pow10((int) Math.Floor(Math.Log10(nc.exponent)));
                                stdAbrev = ExponentToStdAbrev(tExp);
                                exponent = NumberToString(tnum, tExp, _maxPrecision - stdAbrev.Length - lifterLen, true,
                                    _removeTrailingZero, _asHtml, 100);
                                var precision = Math.Max(0,
                                    _maxPrecision - exponent.Length - lifterLen - stdAbrev.Length);
                                var mantissa = NumberToString(nc.mantissa, precision, true,
                                    _removeTrailingZero, _asHtml, _maxDecimals);
                                return $"{mantissa}{lifter}{exponent}{stdAbrev}";
                            }
                        }
                    case 1:
                        switch (nc.exponent)
                        {
                            case < 16:
                                DoExponent(5, $"*10{_upArrow}", 2, "e");

                                b = nc.exponent % 1;
                                tnum = Math.Pow(10, b);
                                tExp = nc.exponent - b;
                                stdAbrev = ExponentToStdAbrev(tExp);
                                exponent = NumberToString(tnum, tExp, _maxPrecision - stdAbrev.Length - lifterLen,
                                    true, _removeTrailingZero, _asHtml, 100);
                                var precision = Math.Max(0,
                                    _maxPrecision - exponent.Length - lifterLen - stdAbrev.Length);
                                var mantissa = NumberToString(nc.mantissa, precision, true, _removeTrailingZero,
                                    _asHtml, _maxDecimals);
                                return $"{mantissa}{lifter}{exponent}{stdAbrev}";
                            case < 3003:
                                DoExponent(3, $"10{_upArrow}", 1, "E");

                                b = nc.exponent % 1;
                                tnum = Math.Pow(10, b);
                                tExp = nc.exponent - b;
                                stdAbrev = ExponentToStdAbrev(tExp);
                                exponent = NumberToString(tnum, tExp, _maxPrecision - stdAbrev.Length - lifterLen,
                                    true, _removeTrailingZero, _asHtml, 100);
                                return $"{lifter}{exponent}{stdAbrev}";
                            default:
                                DoExponent(6, $"10{_upArrow}10{_upArrow}", 2, "EE");

                                tExp = Math.Floor(Math.Log10(nc.exponent));
                                tnum = nc.exponent / NumberClass.Pow10((int) tExp);
                                stdAbrev = ExponentToStdAbrev(tExp);
                                exponent = NumberToString(tnum, tExp, _maxPrecision - stdAbrev.Length - lifterLen, true,
                                    _removeTrailingZero, _asHtml, 100);
                                return lifter + exponent + stdAbrev;
                        }
                    default:
                        Lifer(0, "");
                        if (nc.exponent < 3003)
                        {
                            for (var i = 0; i < intHeight; i++) DoExponent(3, $"10{_upArrow}", 1, "E");

                            b = nc.exponent % 1;
                            tnum = Math.Pow(10, b);
                            tExp = nc.exponent - b;
                        }
                        else
                        {
                            for (var i = 0; i < intHeight + 1; i++) DoExponent(2, $"10{_upArrow}", 1, "E");

                            tExp = Math.Floor(Math.Log10(nc.exponent));
                            tnum = nc.exponent / NumberClass.Pow10((int) tExp);
                        }

                        stdAbrev = ExponentToStdAbrev(tExp);
                        exponent = NumberToString(tnum, tExp, _maxPrecision - stdAbrev.Length - lifterLen, true,
                            _removeTrailingZero, _asHtml, 100);
                        return $"{lifter}{exponent}{stdAbrev}";
                }
            }

            if (nc.expHeight < 1000)
            {
                DoExponent(5, $"10{_upArrow}{_upArrow}", 2, "F");

                string height;
                if (nc.exponent < 3003)
                {
                    height = DoubleToSeparatedString(nc.expHeight, _maxPrecision, _maxPrecision, _separator, true,
                        _removeTrailingZero, _asHtml);
                    b = nc.exponent % 1;
                    tnum = Math.Pow(10, b);
                    tExp = nc.exponent - b;
                }
                else
                {
                    height = DoubleToSeparatedString(nc.expHeight + 1, _maxPrecision, _maxPrecision, _separator,
                        true, _removeTrailingZero, _asHtml);
                    tExp = Math.Floor(Math.Log10(nc.exponent));
                    tnum = nc.exponent / NumberClass.Pow10((int) tExp);
                }

                stdAbrev = ExponentToStdAbrev(tExp);
                var precision = Math.Max(0, _maxPrecision - stdAbrev.Length - lifterLen - height.Length);
                exponent = NumberToString(tnum, tExp, precision, true, false, _asHtml, 100);
                return $"{lifter}{height}{_rightArrow}{exponent}{stdAbrev}";
            }
            else
            {
                DoExponent(5, $"10{_upArrow}{_upArrow}", 2, "F");

                string height;
                var newPrecision = _maxPrecision - lifterLen - 1;
                if (SeparatedStringMinLength(nc.expHeight, _separator) > newPrecision)
                {
                    lifterLen--;
                    var flooredHeight = Math.Floor(Math.Log10(nc.expHeight));
                    tnum = nc.expHeight / NumberClass.Pow10((int) flooredHeight);
                    tExp = flooredHeight;
                    stdAbrev = ExponentToStdAbrev(tExp);
                    var precision = Math.Max(0, _maxPrecision - stdAbrev.Length - lifterLen);
                    exponent = NumberToString(tnum, tExp, precision, true, _removeTrailingZero, _asHtml, 100);
                    return $"{lifter}{exponent}{stdAbrev}";
                }


                if (nc.exponent < 10)
                {
                    height = DoubleToSeparatedString(nc.expHeight, _maxPrecision, _maxPrecision,
                        _separator, true, _removeTrailingZero, _asHtml);
                    tnum = nc.exponent;
                    var precision = Math.Max(0, _maxPrecision - lifterLen - height.Length);
                    exponent = DoubleToString(tnum, precision, _removeTrailingZero, _asHtml, 100);
                    return $"{lifter}{height}{_rightArrow}{exponent}";
                }
                else
                {
                    height = DoubleToSeparatedString(nc.expHeight + 1, _maxPrecision, _maxPrecision,
                        _separator, true, _removeTrailingZero, _asHtml);
                    tnum = Math.Log10(nc.exponent);
                    var precision = Math.Max(0, _maxPrecision - lifterLen - height.Length);
                    exponent = DoubleToString(tnum, precision, _removeTrailingZero, _asHtml, 100);
                    return $"{lifter}{height}{_rightArrow}{exponent}";
                }
            }
        }

        private String ToSciNotation(NumberClass nc)
        {
            var intHeight = (int) nc.expHeight;
            var lifter = "";
            string exponent;
            var lifterLen = 0;
            int newPrecision;

            void Lifer(int number, string text) => (lifter, lifterLen) = (text, number);

            void DoExponent(int arrowN, string arrowS, int letterN, string letterS)
            {
                if (_exponentMode == ExponentMode.Arrow) Lifer(arrowN, arrowS);
                else Lifer(letterN, letterS);
            }

            string height;
            if (nc.expHeight + 1 < (_exponentMode == ExponentMode.Letter ? 5 : 3))
            {
                switch (intHeight)
                {
                    case 0:
                        DoExponent(4, $"*10{_upArrow}", 1, "e");

                        newPrecision = _maxPrecision - lifterLen;
                        if (SeparatedStringMinLength(nc.exponent, _separator) <= newPrecision)
                        {
                            exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision, _maxPrecision,
                                _separator, true, _removeTrailingZero, _asHtml);
                            var mantissa = NumberToString(nc.mantissa, nc.exponent,
                                _maxPrecision - exponent.Length - lifterLen, false, _removeTrailingZero, _asHtml,
                                _maxDecimals); //putting front numbers
                            return $"{mantissa}{lifter}{exponent}";
                        }
                        else
                        {
                            // #.##e#,### or  #.## *10&uarr; #,###
                            LogTheExp(nc);
                            DoExponent(6, $"10{_upArrow}10{_upArrow}", 2, "EE");

                            exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision - lifterLen,
                                _maxPrecision, _separator, false, _removeTrailingZero, _asHtml);
                            return $"{lifter}{exponent}";
                        }
                    case 1:
                        DoExponent(6, $"10{_upArrow}10{_upArrow}", 2, "EE");

                        newPrecision = _maxPrecision - lifterLen;
                        if (SeparatedStringMinLength(nc.exponent, _separator) > newPrecision)
                        {
                            LogTheExp(nc);
                            lifter += lifter;
                            lifterLen += lifterLen;
                        }

                        exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision - lifterLen,
                            _maxPrecision, _separator, false, _removeTrailingZero, _asHtml);
                        return $"{lifter}{exponent}";
                    default:

                        void LiferA(int number, string text)
                        {
                            lifter += text;
                            lifterLen += number;
                        }

                        void DoExponentA(int arrowN, string arrowS, int letterN, string letterS)
                        {
                            if (_exponentMode == ExponentMode.Arrow) LiferA(arrowN, arrowS);
                            else LiferA(letterN, letterS);
                        }

                        for (var i = 0; i <= intHeight; i++) DoExponentA(3, $"10{_upArrow}", 1, "E");

                        newPrecision = _maxPrecision - lifterLen;
                        if (SeparatedStringMinLength(nc.exponent, _separator) > newPrecision)
                        {
                            LogTheExp(nc);
                            if (nc.expHeight + 2 >= (_exponentMode == ExponentMode.Letter ? 5 : 3))
                            {
                                DoExponent(5, $"10{_upArrow}{_upArrow}", 2, "F");
                                height = DoubleToSeparatedString(nc.expHeight + 2, _maxPrecision,
                                    _maxPrecision, _separator, true, _removeTrailingZero, _asHtml);
                                exponent = DoubleToSeparatedString(nc.exponent,
                                    _maxPrecision - lifterLen - height.Length, _maxPrecision, _separator, false,
                                    _removeTrailingZero, _asHtml);
                                return $"{lifter}{height}{_rightArrow}{exponent}";
                            }

                            DoExponentA(3, $"10{_upArrow}", 1, "E");
                        }

                        exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision - lifterLen,
                            _maxPrecision, _separator, false, _removeTrailingZero, _asHtml);
                        return lifter + exponent;
                }
            }

            nc.expHeight++;

            DoExponent(5, $"10{_upArrow}{_upArrow}", 2, "F");
            newPrecision = _maxPrecision - lifterLen - SeparatedStringMinLength(nc.expHeight, _separator) - 1;

            if (newPrecision <= 0)
            {
                string lifter2;
                int lifterLen2;

                void Lifer2(int number, string text) => (lifter2, lifterLen2) = (text, number);

                void DoExponent2(int arrowN, string arrowS, int letterN, string letterS)
                {
                    if (_exponentMode == ExponentMode.Arrow) Lifer2(arrowN, arrowS);
                    else Lifer2(letterN, letterS);
                }

                DoExponent2(4, $"*10{_upArrow},", 1, "e");

                lifterLen--;
                var flooredHeight = Math.Floor(Math.Log10(nc.expHeight));
                var tnum = nc.expHeight / NumberClass.Pow10((int) flooredHeight);
                exponent = DoubleToSeparatedString(flooredHeight, _maxPrecision - lifterLen - lifterLen2 - 1,
                    _maxPrecision, _separator, true, _removeTrailingZero, _asHtml);
                var precision = Math.Max(0, +_maxPrecision - lifterLen - lifterLen2 - exponent.Length);
                height = DoubleToString(tnum, precision, _removeTrailingZero, _asHtml, 100);
                return $"{lifter}{height}{lifter2}{exponent}";
            }

            if (SeparatedStringMinLength(nc.exponent, _separator) > newPrecision) LogTheExp(nc);

            height = DoubleToSeparatedString(nc.expHeight, _maxPrecision, _maxPrecision, _separator,
                true, _removeTrailingZero, _asHtml);
            exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision - lifterLen - height.Length, _maxPrecision,
                _separator, false, _removeTrailingZero, _asHtml);
            return $"{lifter}{height}{_rightArrow}{exponent}";
        }

        private String ToEngineeringNumber(NumberClass nc)
        {
            var intHeight = (int) nc.expHeight;
            var lifter = "";
            string exponent;
            var lifterLen = 0;
            double tnum;
            double tExp;

            void Lifer(int number, string text) => (lifter, lifterLen) = (text, number);

            void DoExponent(int arrowN, string arrowS, int letterN, string letterS)
            {
                if (_exponentMode == ExponentMode.Arrow) Lifer(arrowN, arrowS);
                else Lifer(letterN, letterS);
            }

            void DoExponentC(int arrowN, string arrowS, string letterS)
            {
                if (_exponentMode == ExponentMode.Arrow)
                {
                    lifter += arrowS;
                    lifterLen -= arrowN;
                }
                else
                {
                    lifter += letterS;
                    lifterLen++;
                }
            }

            if (nc.expHeight < (_exponentMode == ExponentMode.Letter ? 5 : 2))
            {
                switch (intHeight)
                {
                    case 0:
                        DoExponent(4, $"*10{_upArrow}", 1, "e");

                        var newPrecision = _maxPrecision - lifterLen - 1;
                        if (nc.exponent < NumberClass.Pow10(newPrecision - newPrecision / 3))
                        {
                            exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision, _maxPrecision, _separator,
                                true, _removeTrailingZero, _asHtml);
                            var mantissa = NumberToString(nc.mantissa, nc.exponent,
                                _maxPrecision - exponent.Length - lifterLen, true, _removeTrailingZero, _asHtml,
                                _maxDecimals); //putting front numbers
                            return $"{mantissa}{lifter}{exponent}";
                        }
                        else
                        {
                            // #.##e#,### or  #.## *10&uarr; #,###
                            var precision = (_maxPrecision - lifterLen) / 2;
                            tExp = Math.Floor(Math.Log10(nc.exponent));
                            tExp -= tExp % 3;
                            tnum = nc.exponent / NumberClass.Pow10((int) tExp);
                            exponent = DoubleToSeparatedString(tExp, precision, _maxPrecision, _separator, true,
                                _removeTrailingZero, _asHtml);
                            var mantissa = NumberToString(nc.mantissa, nc.exponent, 1, false,
                                _removeTrailingZero, _asHtml, _maxDecimals); //putting front numbers
                            var mantissa2 = NumberToString(tnum, tExp,
                                _maxPrecision - exponent.Length - lifterLen * 2 - mantissa.Length, true,
                                _removeTrailingZero, _asHtml, _maxDecimals); //putting front numbers
                            return mantissa + lifter + mantissa2 + lifter + exponent;
                        }
                    case 1:
                        if (nc.exponent < 16)
                        {
                            DoExponent(4, $"*10{_upArrow}", 1, "e");
                            var precision = (_maxPrecision - lifterLen) / 2;
                            tExp = Math.Floor(nc.exponent);
                            var v = tExp % 3;
                            tExp -= v;
                            tnum = Math.Pow(10, nc.exponent % 1 + v);
                            exponent = DoubleToSeparatedString(tExp, precision, _maxPrecision, _separator, true,
                                _removeTrailingZero, _asHtml);
                            var mantissa = NumberToString(nc.mantissa, nc.exponent, 1, false,
                                _removeTrailingZero, _asHtml, _maxDecimals); //putting front numbers
                            var mantissa2 = NumberToString(tnum, tExp,
                                _maxPrecision - exponent.Length - lifterLen * 2 - mantissa.Length, true,
                                _removeTrailingZero, _asHtml, _maxDecimals); //putting front numbers
                            return mantissa + lifter + mantissa2 + lifter + exponent;
                        }
                        else
                        {
                            string lifter2;
                            int lifterLen2;

                            void Lifer2(int number, string text) => (lifter2, lifterLen2) = (text, number);

                            void DoExponent2(int arrowN, string arrowS, int letterN, string letterS)
                            {
                                if (_exponentMode == ExponentMode.Arrow) Lifer2(arrowN, arrowS);
                                else Lifer2(letterN, letterS);
                            }

                            DoExponent(3, $"10{_upArrow}", 1, "E");
                            DoExponent2(4, $"*10{_upArrow}", 1, "e");

                            tExp = Math.Floor(nc.exponent);
                            var v = tExp % 3;
                            tExp -= v;
                            if (SeparatedStringMinLength(tExp, _separator) + EngineringMinLength(tExp) + lifterLen +
                                lifterLen2 > _maxPrecision)
                            {
                                exponent = DoubleToSeparatedString(nc.exponent, _maxPrecision - 2 * lifterLen,
                                    _maxPrecision, _separator, false, _removeTrailingZero, _asHtml);
                                return $"{lifter}{lifter}{exponent}";
                            }

                            tnum = Math.Pow(10, nc.exponent % 1 + v);
                            exponent = DoubleToSeparatedString(tExp, _maxPrecision, _maxPrecision,
                                _separator,
                                true, _removeTrailingZero, _asHtml);
                            var mantissa2 = NumberToString(tnum, tExp,
                                _maxPrecision - exponent.Length - lifterLen - lifterLen2, true, _removeTrailingZero,
                                _asHtml, _maxDecimals); //putting front numbers
                            return $"{lifter}{mantissa2}{lifter2}{exponent}";
                        }
                    default:

                        void DoExponentB(int arrowN, string arrowS, string letterS)
                        {
                            if (_exponentMode == ExponentMode.Arrow)
                            {
                                lifter += arrowS;
                                lifterLen -= arrowN;
                            }
                            else lifter += letterS;
                        }

                        if (nc.exponent < 3003)
                        {
                            for (var i = 0; i < intHeight; i++) DoExponentB(2, $"10{_upArrow}", "E");

                            lifterLen += lifter.Length;
                            var b = nc.exponent % 1;
                            tnum = Math.Pow(10, b);
                            tExp = nc.exponent - b;
                        }
                        else
                        {
                            for (var i = 0; i < intHeight + 1; i++) DoExponentB(2, $"10{_upArrow}", "E");

                            lifterLen += lifter.Length;
                            tExp = Math.Floor(Math.Log10(nc.exponent));
                            tnum = nc.exponent / NumberClass.Pow10((int) tExp);
                        }

                        var stdAbrev = ExponentToStdAbrev(tExp); //putting exponent suffix  (ez)
                        exponent = NumberToString(tnum, tExp, _maxPrecision - stdAbrev.Length - lifterLen, true,
                            _removeTrailingZero, _asHtml, 100);
                        return $"{lifter}{exponent}{stdAbrev}";
                }
            }

            if (nc.expHeight < 1000)
            {
                DoExponentC(3, $"10{_upArrow}{_upArrow}", "F");

                string height;
                lifterLen += lifter.Length;
                if (nc.exponent < 3003)
                {
                    height = DoubleToSeparatedString(nc.expHeight, _maxPrecision, _maxPrecision, _separator,
                        true,
                        _removeTrailingZero, _asHtml);
                    var b = nc.exponent % 1;
                    tnum = Math.Pow(10, b);
                    tExp = nc.exponent - b;
                }
                else
                {
                    height = DoubleToSeparatedString(nc.expHeight + 1, _maxPrecision, _maxPrecision, _separator, true,
                        _removeTrailingZero, _asHtml);
                    tExp = Math.Floor(Math.Log10(nc.exponent));
                    tnum = nc.exponent / NumberClass.Pow10((int) tExp);
                }

                var stdAbrev = ExponentToStdAbrev(tExp); //putting exponent suffix  (ez)
                var precision = Math.Max(0, _maxPrecision - stdAbrev.Length - lifterLen - height.Length);
                exponent = NumberToString(tnum, tExp, precision, true, false, _asHtml, 100);
                return $"{lifter}{height}{_rightArrow}{exponent}{stdAbrev}";
            }
            else
            {
                DoExponentC(3, $"10{_upArrow}{_upArrow}", "F");

                string height;
                lifterLen += lifter.Length;
                var logedH = Math.Log10(nc.expHeight);
                var newPrecision = _maxPrecision - lifterLen - 1;
                if (!(logedH > newPrecision - newPrecision / 3))
                {
                    if (nc.exponent < 10)
                    {
                        height = DoubleToSeparatedString(nc.expHeight, _maxPrecision, _maxPrecision,
                            _separator, true, _removeTrailingZero, _asHtml);
                        tnum = nc.exponent;
                    }
                    else
                    {
                        height = DoubleToSeparatedString(nc.expHeight + 1, _maxPrecision, _maxPrecision,
                            _separator, true, _removeTrailingZero, _asHtml);
                        tnum = Math.Log10(nc.exponent);
                    }

                    var precision = Math.Max(0, _maxPrecision - lifterLen - height.Length);
                    exponent = DoubleToString(tnum, precision, _removeTrailingZero, _asHtml, 100);
                    return $"{lifter}{height}{_rightArrow}{exponent}";
                }
                else
                {
                    lifterLen--;
                    var flooredHeight = Math.Floor(Math.Log10(nc.expHeight));
                    tnum = nc.expHeight / NumberClass.Pow10((int) flooredHeight);
                    tExp = flooredHeight;
                    var stdAbrev = ExponentToStdAbrev(tExp); //putting exponent suffix  (ez)
                    var precision = Math.Max(0, _maxPrecision - stdAbrev.Length - lifterLen);
                    exponent = NumberToString(tnum, tExp, precision, true, _removeTrailingZero, _asHtml, 100);
                    return lifter + exponent + stdAbrev;
                }
            }
        }

        private static int EngineringMinLength(double tExp) => ((int) tExp - 1) % 3 + 1;

        public Formatter SetUpArrow(string upArrow)
        {
            _upArrow = upArrow;
            return this;
        }

        public Formatter SetRightArrow(string rightArrow)
        {
            _rightArrow = rightArrow;
            return this;
        }

        public Formatter SetMaxDecimals(int maxDecimals)
        {
            _maxDecimals = maxDecimals;
            return this;
        }

        public Formatter SetMaxPrecision(int maxPrecision)
        {
            _maxPrecision = maxPrecision;
            return this;
        }

        public Formatter SetSeparator(char[] separator)
        {
            _separator = separator;
            return this;
        }

        public Formatter SetSeparator(string separator)
        {
            _separator = separator.ToCharArray();
            return this;
        }

        public Formatter SetNotation(Notation notation)
        {
            _notation = notation;
            return this;
        }

        public Formatter SetExponentMode(ExponentMode exponentMode)
        {
            _exponentMode = exponentMode;
            return this;
        }

        public Formatter SetRemoveTrailingZero(bool removeTrailingZero)
        {
            _removeTrailingZero = removeTrailingZero;
            return this;
        }

        public Formatter SetInteger(bool integer)
        {
            _isInteger = integer;
            return this;
        }

        public Formatter SetShowInfinity(bool showInfinity)
        {
            _showInfinity = showInfinity;
            return this;
        }

        public Formatter SetAsHtml(bool asHtml)
        {
            _asHtml = asHtml;
            return this;
        }

        public string GetString(double reInfinite) => GetString(new NumberClass(reInfinite));
        public string GetString(string reInfinite) => GetString(new NumberClass(reInfinite));

        public string GetString(NumberClass nc)
        {
            try
            {
                if (nc == NumberClass.Zero) return "0";
                if (nc.expHeight.IsInfinite()) return "&infin;&#128165;";
                if (nc.Log10().Floor() <= 1 + _maxPrecision - _maxPrecision / 3)
                {
                    return DoubleToSeparatedString(nc.ToSafeDouble(), _maxPrecision, _maxDecimals, _separator,
                        _isInteger, _removeTrailingZero, _asHtml);
                }

                return AssignMethod(nc);
            }
            catch (Exception)
            {
                throw new Exception(nc is null
                    ? "ReInfinite was null"
                    : $"{nc}crashed for some reasons");
            }
        }

        /**
         * @param reInfinite that is already poped
         * @return text
         */
        private string AssignMethod(NumberClass nc)
        {
            return _notation switch
            {
                Notation.Standered => ToStdNumber(CompatReInfiniteStd(nc, _removeTrailingZero)),
                Notation.Scientific => ToSciNotation(nc),
                Notation.Engineering => ToEngineeringNumber(nc),
                _ => ToStdNumber(CompatReInfiniteStd(nc, _removeTrailingZero))
            };
        }
    }
}