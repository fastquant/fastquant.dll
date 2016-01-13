using System;
using System.Collections.Generic;

namespace SmartQuant.Charting
{
    public static class Extensions
    {
        public static IEnumerable<double> CumulativeSum(this IEnumerable<double> sequence)
        {
            double sum = 0;
            foreach (var item in sequence)
            {
                sum += item;
                yield return sum;
            }        
        }
    }
}

