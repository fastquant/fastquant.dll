// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using static System.Math;

namespace SmartQuant.Quant
{
    public enum EOptionPosition
    {
        InTheMoney,
        AtTheMoney,
        OutOfTheMoney
    }

    public enum EOptionPrice
    {
        BlackScholes,
        Binomial,
        Trinomial,
        MonteCarlo
    }

    public enum EOptionType
    {
        European,
        American,
        Exotic,
        Bermudian,
        Digial
    }

    public enum EPutCall
    {
        Call,
        Put
    }

    public class FinMath
    {
        public static double FV1(double p, double r, double n)
        {
            return p * (1.0 + r * n);
        }

        public static double FV2(double p, double r, double n)
        {
            return p * Pow(1.0 + r, n);
        }

        public static double FV3(double p, double r, double n, double m)
        {
            return p * Pow(1.0 + r / m, n * m);
        }

        public static double FV4(double p, double r, double n)
        {
            return p * Exp(r * n);
        }

        public static double PV1(double f, double r, double n)
        {
            return f / (1.0 + r * n);
        }
        
        public static double PV2(double f, double r, double n)
        {
            return f / Pow(1.0 + r, n);
        }

        public static double PV3(double f, double r, double n, double m)
        {
            return f / Pow(1.0 + r / m, n * m);
        }

        public static double PV4(double f, double r, double n)
        {
            return f / Exp(r * n);
        }

        public static double dPV2(double f, double r, double n)
        {
            return -f * n / Pow(1.0 + r, n + 1.0);
        }

        public static double d2PV2(double f, double r, double n)
        {
            return f* n * (n + 1.0) / Pow(1.0 + r, n + 2.0);
        }

        public static double Fact(int n) => Enumerable.Range(1, n).Aggregate((s, i) => s * i);

        public static double C(int m, int n)
        {
            return FinMath.Fact(n) / (FinMath.Fact(m) * FinMath.Fact(n - m));
        }

        public static double Binom(int m, int n, double p)
        {
            return FinMath.C(m, n) * Math.Pow(p, (double)m) * Math.Pow(1.0 - p, (double)(n - m));
        }

        public static double u(double t, double s, int n)
        {
            return Math.Exp(s * Math.Sqrt(t / (double)n));
        }

        public static double d(double t, double s, int n)
        {
            return Math.Exp(-s * Math.Sqrt(t / (double)n));
        }

        public static double p(double t, double s, int n, double r)
        {
            double num1 = FinMath.FV2(1.0, r, t / (double)n);
            double num2 = FinMath.u(t, s, n);
            double num3 = FinMath.d(t, s, n);
            return (num1 - num3) / (num2 - num3);
        }

        public static double N(double z)
        {
            double num1 = 2.506628;
            double num2 = 0.3193815;
            double num3 = -0.3565638;
            double num4 = 1.7814779;
            double num5 = -1.821256;
            double num6 = 1.3302744;
            double num7 = z > 0.0 || z == 0.0 ? 1.0 : -1.0;
            double num8 = 1.0 / (1.0 + 0.2316419 * num7 * z);
            return 0.5 + num7 * (0.5 - Math.Exp(-z * z / 2.0) / num1 * (num8 * (num2 + num8 * (num3 + num8 * (num4 + num8 * (num5 + num8 * num6))))));
        }

        public static double n(double z)
        {
            return 1.0 / Math.Sqrt(2.0 * Math.PI) * Math.Exp(-0.5 * z * z);
        }

        public static double Call(double s, double x)
        {
            return Math.Max(0.0, s - x);
        }

        public static double Put(double s, double x)
        {
            return Math.Max(0.0, x - s);
        }

        public static double Payoff(double S, double X, EPutCall PutCall)
        {
            switch (PutCall)
            {
                case EPutCall.Call:
                    return FinMath.Call(S, X);
                case EPutCall.Put:
                    return FinMath.Put(S, X);
                default:
                    return 0.0;
            }
        }

        public static double Parity(double P, double S, double X, double t, double r, EPutCall PutCall)
        {
            switch (PutCall)
            {
                case EPutCall.Call:
                    return P - (S - X * Math.Exp(-r * t));
                case EPutCall.Put:
                    return P + (S - X * Math.Exp(-r * t));
                default:
                    return 0.0;
            }
        }

        public static double BM(double S, double X, double t, double s, double r, EPutCall PutCall, int n)
        {
            double F = 0.0;
            double x1 = FinMath.u(t, s, n);
            double x2 = FinMath.d(t, s, n);
            double p = FinMath.p(t, s, n, r);
            for (int m = 0; m <= n; ++m)
                F += FinMath.Binom(m, n, p) * FinMath.Payoff(S * Math.Pow(x1, (double)m) * Math.Pow(x2, (double)(n - m)), X, PutCall);
            return FinMath.PV4(F, r, t);
        }

        public static double BM(double S, double X, double t, double s, double r, EPutCall PutCall)
        {
            return FinMath.BM(S, X, t, s, r, PutCall, 100);
        }

        public static double MC(double S, double X, double t, double s, double r, EPutCall PutCall, int n)
        {
            double num1 = (r - 0.5 * s * s) * t;
            double num2 = s * Math.Sqrt(t);
            double num3 = 0.0;
            for (int index = 0; index < n; ++index)
                num3 += FinMath.Payoff(S * Math.Exp(num1 + num2 * Random.Gaus()), X, PutCall);
            return FinMath.PV4(num3 / (double)n, r, t);
        }

        public static double MC(double S, double X, double t, double s, double r, EPutCall PutCall)
        {
            return FinMath.MC(S, X, t, s, r, PutCall, 100000);
        }

        public static double d1(double S, double X, double t, double s, double r)
        {
            return (Math.Log(S / X) + (r + s * s / 2.0) * t) / (s * Math.Sqrt(t));
        }

        public static double d2(double S, double X, double t, double s, double r)
        {
            return FinMath.d1(S, X, t, s, r) - s * Math.Sqrt(t);
        }

        public static double BS(double S, double X, double t, double s, double r, EPutCall PutCall)
        {
            switch (PutCall)
            {
                case EPutCall.Call:
                    return S * FinMath.N(FinMath.d1(S, X, t, s, r)) - X * Math.Exp(-r * t) * FinMath.N(FinMath.d2(S, X, t, s, r));
                case EPutCall.Put:
                    return -S * FinMath.N(-FinMath.d1(S, X, t, s, r)) + X * Math.Exp(-r * t) * FinMath.N(-FinMath.d2(S, X, t, s, r));
                default:
                    return 0.0;
            }
        }

        public static double Delta(double S, double X, double t, double s, double r, EPutCall PutCall)
        {
            switch (PutCall)
            {
                case EPutCall.Call:
                    return FinMath.N(FinMath.d1(S, X, t, s, r));
                case EPutCall.Put:
                    return FinMath.N(FinMath.d1(S, X, t, s, r)) - 1.0;
                default:
                    return 0.0;
            }
        }

        public static double Gamma(double S, double X, double t, double s, double r)
        {
            return FinMath.n(FinMath.d1(S, X, t, s, r)) / (S * s * Math.Sqrt(t));
        }

        public static double Theta(double S, double X, double t, double s, double r, EPutCall PutCall)
        {
            switch (PutCall)
            {
                case EPutCall.Call:
                    return -S * FinMath.n(FinMath.d1(S, X, t, s, r)) * s / (2.0 * Math.Sqrt(t)) - r * X * Math.Exp(-r * t) * FinMath.N(FinMath.d2(S, X, t, s, r));
                case EPutCall.Put:
                    return -S * FinMath.n(FinMath.d1(S, X, t, s, r)) * s / (2.0 * Math.Sqrt(t)) + r * X * Math.Exp(-r * t) * FinMath.N(-FinMath.d2(S, X, t, s, r));
                default:
                    return 0.0;
            }
        }

        public static double Vega(double S, double X, double t, double s, double r)
        {
            return S * Math.Sqrt(t) * FinMath.n(FinMath.d1(S, X, t, s, r));
        }

        public static double Rho(double S, double X, double t, double s, double r, EPutCall PutCall)
        {
            switch (PutCall)
            {
                case EPutCall.Call:
                    return X * t * Math.Exp(-r * t) * FinMath.N(FinMath.d2(S, X, t, s, r));
                case EPutCall.Put:
                    return -X * t * Math.Exp(-r * t) * FinMath.N(-FinMath.d2(S, X, t, s, r));
                default:
                    return 0.0;
            }
        }

        public static double ImpliedVolatility(double S, double X, double t, double r, double P, EOptionType OptionType, EPutCall PutCall, EOptionPrice Method, int n, double Eps)
        {
            double num1 = 0.0;
            double num2 = 10.0;
            double num3 = 0.0;
            double s = 0.0;
            while (Math.Abs(num2 - num1) > Eps)
            {
                s = num1 + (num2 - num1) / 2.0;
                switch (Method)
                {
                    case EOptionPrice.BlackScholes:
                        num3 = FinMath.BS(S, X, t, s, r, PutCall);
                        break;
                    case EOptionPrice.Binomial:
                        num3 = FinMath.BM(S, X, t, s, r, PutCall, n);
                        break;
                    case EOptionPrice.MonteCarlo:
                        num3 = FinMath.MC(S, X, t, s, r, PutCall, n);
                        break;
                }
                if (num3 > P)
                    num2 = s;
                else
                    num1 = s;
            }
            return s;
        }

        public static double ImpliedVolatility(double S, double X, double t, double r, double P, EOptionType OptionType, EPutCall PutCall, EOptionPrice Method)
        {
            int n = 0;
            switch (Method)
            {
                case EOptionPrice.Binomial:
                    n = 100;
                    break;
                case EOptionPrice.MonteCarlo:
                    n = 100000;
                    break;
            }
            double Eps = 0.001;
            return FinMath.ImpliedVolatility(S, X, t, r, P, OptionType, PutCall, Method, n, Eps);
        }

        public static double FuturesPrice(double S, double t, double r)
        {
            return S * Math.Exp(r * t);
        }

        public static double FuturesPrice(double S, double t, double r, double I)
        {
            return (S - I) * Math.Exp(r * t);
        }

        public static double Min(double Value1, double Value2, double Value3)
        {
            return Math.Min(Math.Min(Value1, Value2), Value3);
        }

        public static double Max(double Value1, double Value2, double Value3)
        {
            return Math.Max(Math.Max(Value1, Value2), Value3);
        }

        public static int Min(int Value1, int Value2, int Value3)
        {
            return Math.Min(Math.Min(Value1, Value2), Value3);
        }

        public static int Max(int Value1, int Value2, int Value3)
        {
            return Math.Max(Math.Max(Value1, Value2), Value3);
        }

        public static DateTime Min(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1 <= dateTime2 ? dateTime1 : dateTime2;
        }

        public static DateTime Max(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1 >= dateTime2 ? dateTime1 : dateTime2;
        }

        public static DateTime Min(DateTime DateTime1, DateTime DateTime2, DateTime DateTime3)
        {
            return FinMath.Min(FinMath.Min(DateTime1, DateTime2), DateTime3);
        }

        public static DateTime Max(DateTime DateTime1, DateTime DateTime2, DateTime DateTime3)
        {
            return FinMath.Max(FinMath.Max(DateTime1, DateTime2), DateTime3);
        }

        public static double Percentile(double Level, double[] Data)
        {
            if (Level < 0.0 || Level > 100.0)
                throw new ArgumentException("Can not calculate percentile. Percentile value should be between 0 and 100");
            if (Data == null)
                throw new ArgumentException("Can not calculate percentile. Data array can not be null");
            int length = Data.Length;
            double[] array = new double[length];
            Data.CopyTo((Array)array, 0);
            Array.Sort<double>(array);
            double num1 = Level / 100.0 * (double)(length + 1) - 1.0;
            int index = (int)num1;
            double num2 = num1 - (double)index;
            if (index < 0)
                return array[0];
            if (index >= length - 1)
                return array[length - 1];
            if (num2 == 0.0)
                return array[index];
            else
                return array[index] + (array[index + 1] - array[index]) * num2;
        }

        public static int BinarySearch(int n, double[] SearchArray, double SearchValue)
        {
            int num1 = n + 1;
            int num2 = 0;
            while (num1 - num2 > 1)
            {
                int num3 = (num1 + num2) / 2;
                if (SearchValue == SearchArray[num3 - 1])
                    return num3 - 1;
                if (SearchValue < SearchArray[num3 - 1])
                    num1 = num3;
                else
                    num2 = num3;
            }
            return num2 - 1;
        }

        public static int BinarySearch(int n, int[] SearchArray, int SearchValue)
        {
            int num1 = n + 1;
            int num2 = 0;
            while (num1 - num2 > 1)
            {
                int num3 = (num1 + num2) / 2;
                if (SearchValue == SearchArray[num3 - 1])
                    return num3 - 1;
                if (SearchValue < SearchArray[num3 - 1])
                    num1 = num3;
                else
                    num2 = num3;
            }
            return num2 - 1;
        }

        public static double Distance(double X1, double Y1, double X2, double Y2)
        {
            return Math.Sqrt(Math.Pow(X1 - X2, 2.0) + Math.Pow(Y1 - Y2, 2.0));
        }

        public static double Distance(double X1, double Y1, double Z1, double X2, double Y2, double Z2)
        {
            return Math.Sqrt(Math.Pow(X1 - X2, 2.0) + Math.Pow(Y1 - Y2, 2.0) + Math.Pow(Z1 - Z2, 2.0));
        }

        public static double Percent(double P, double Base)
        {
            return Base / 100.0 * P;
        }

        public static int GetNDays(DateTime date1, DateTime date2)
        {
            return (date1 - date2).Days;
        }
    }
}