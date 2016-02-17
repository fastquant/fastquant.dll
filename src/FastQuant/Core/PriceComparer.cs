// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System;

namespace FastQuant.Providers
{
    public enum PriceSortOrder
    {
        Ascending,
        Descending,
    }

    public class PriceComparer : IComparer<double>
    {
        private PriceSortOrder sortOrder;

        public PriceComparer(PriceSortOrder priceSortOrder)
        {
            this.sortOrder = priceSortOrder;
        }

        public int Compare(double x, double y)
        {
            switch (this.sortOrder)
            {
                case PriceSortOrder.Ascending:
                    return x.CompareTo(y);
                case PriceSortOrder.Descending:
                    return y.CompareTo(x);
                default:
                    throw new ArgumentException($"Unknown price sort order - {this.sortOrder}");
            }
        }
    }
}