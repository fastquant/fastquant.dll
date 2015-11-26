// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public enum Cross
    {
        Above,
        Below,
        None
    }

    public class TimeSeries : IIdNamedItem, ISeries
    {
        protected string name;
        protected internal string description;

        public double this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public double this[int index, BarData barData]
        {
            get { throw new NotImplementedException(); }
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        public double First
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime FirstDateTime
        {
            get { throw new NotImplementedException(); }
        }

        public int Id
        {
            get { throw new NotImplementedException(); }
        }

        public List<Indicator> Indicators
        {
            get { throw new NotImplementedException(); }
        }

        public double Last
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastDateTime
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime GetDateTime(int index)
        {
            throw new NotImplementedException();
        }

        public int GetIndex(DateTime dateTime, IndexOption option = IndexOption.Null)
        {
            throw new NotImplementedException();
        }

        public double GetMax(DateTime dateTime1, DateTime dateTime2)
        {
            throw new NotImplementedException();
        }

        public double GetMax(int index1, int index2, BarData barData)
        {
            throw new NotImplementedException();
        }

        public double GetMin(DateTime dateTime1, DateTime dateTime2)
        {
            throw new NotImplementedException();
        }

        public double GetMin(int index1, int index2, BarData barData)
        {
            throw new NotImplementedException();
        }

        public Cross Crosses(TimeSeries series, DateTime dateTime)
        {
            throw new NotImplementedException();
        }
    }
}