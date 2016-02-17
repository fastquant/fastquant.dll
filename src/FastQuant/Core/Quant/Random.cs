// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace FastQuant.Quant
{
    public class Random
    {
        public static double Seed1 { get; set; } = 9876;

        public static double Seed2 { get; set; } = 54321;

        public static double Rndm() => Lecuyer();

        public static int Binomial(int ntot, double prob)
        {
            if (prob < 0 || prob > 1)
                return 0;
            return Enumerable.Repeat(0, ntot).Count(i => Rndm() <= prob);
        }

        public static double Gaus(double mean, double sigma)
        {
            double d;
            do
            {
                d = Rndm();
            } while (d == 0.0);
            return mean + sigma * Math.Sin(Rndm() * 6.283185) * Math.Sqrt(-2 * Math.Log(d));
        }

        public static double Gaus() => Gaus(0, 1);

        public static int Poisson(double mean)
        {
            if (mean <= 0)
                return 0;
            if (mean > 88.0)
                return (int)(Gaus(0, 1) * Math.Sqrt(mean) + mean + 0.5);
            double c = Math.Exp(-mean);
            double sum = 1;
            int count = -1;
            do
            {
                ++count;
                sum *= Rndm();
            }
            while (sum > c);
            return count;
        }

        private static double Lecuyer()
        {
            int k, z;
            k = (int)Seed1 / 53668;
            Seed1 = 40014 * (Seed1 % 53668) - k * 12211;
            if (Seed1 < 0)
                Seed1 += 2147483563;
            k = (int)Seed2 / 52774;
            Seed2 = 40692 * (Seed2 % 52774) - k * 3791;
            if (Seed2 < 0)
                Seed2 += 2147483399;
            z = (int)(Seed1 - Seed2);
            if (z < 1)
                z += 2147483562;
            return z * 4.6566128E-10;
        }
    }
}