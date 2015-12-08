// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartQuant
{
    public enum Cross
    {
        Above,
        Below,
        None
    }

    public class TimeSeriesItem : DataObject
    {
        public override byte TypeId => DataObjectType.TimeSeriesItem;

        public double Value { get; }

        public TimeSeriesItem()
        {
        }

        public TimeSeriesItem(TimeSeriesItem item) : this(item.DateTime, item.Value)
        {
        }

        public TimeSeriesItem(DateTime dateTime, double value)
        {
            DateTime = dateTime;
            Value = value;
        }

        public override string ToString() => $"TimeSeriesItem {DateTime} {Value}";
    }

    public class TimeSeries : IIdNamedItem, ISeries
    {
        protected string name;
        protected internal string description;
        private IDataSeries series;

        private TimeSeriesItem min;
        private TimeSeriesItem max;
        private bool dirty;
        private double sum;
        private double mean;
        private double median;
        private double variance;

        private static Func<double, double, double> opAdd = (a, b) => a + b;
        private static Func<double, double, double> opSub = (a, b) => a - b;
        private static Func<double, double, double> opMul = (a, b) => a * b;
        private static Func<double, double, double> opDiv = (a, b) => a / b;

        public List<Indicator> Indicators { get; } = new List<Indicator>();

        public int Id { get; } = -1;

        public string Name => this.name;

        public string Description => this.description;

        public TimeSeries() : this(string.Empty, null, -1, null)
        {
        }

        public TimeSeries(IDataSeries series) : this(series.Name, series.Description, -1, series)
        {
        }

        public TimeSeries(string name, string description = "", int id = -1) : this(name, description, id, null)
        {
        }

        private TimeSeries(string name, string description = "", int id = -1, IDataSeries series = null)
        {
            this.name = name;
            this.description = description;
            Id = id;
            this.series = series ?? new MemorySeries(name, description);
        }

        public void Clear()
        {
            this.series.Clear();
            this.min = this.max = null;
            this.dirty = true;
        }

        public virtual double this[int index] => ((TimeSeriesItem)this.series[(long)index]).Value;

        public virtual double this[int index, BarData barData] => this[index];

        public double this[int index, int row] => this[index];

        public double this[DateTime dateTime, SearchOption option = SearchOption.ExactFirst]
        {
            get
            {
                return GetByDateTime(dateTime, option).Value;
            }
            set
            {
                Add(dateTime, value);
            }
        }

        public double this[DateTime dateTime, int row, SearchOption option = SearchOption.ExactFirst]
            => GetByDateTime(dateTime, option).Value;

        public virtual int Count => (int)this.series.Count;

        public virtual double First => this[0];

        public virtual double Last => this[Count - 1];

        public virtual DateTime FirstDateTime => this.series[0].DateTime;

        public virtual DateTime LastDateTime => this.series[this.series.Count - 1].DateTime;

        public bool Contains(DateTime dateTime) => IndexOf(dateTime, SearchOption.ExactFirst) != -1;

        public Cross Crosses(TimeSeries series, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public Cross Crosses(double level, int index)
        {
            if (index <= 0 || index > this.series.Count - 1)
                return Cross.None;
            var prev = GetItem(index - 1).Value;
            var current = GetItem(index).Value;
            if (prev <= level && level < current)
                return Cross.Above;
            if (prev >= level && level > current)
                return Cross.Below;
            return Cross.None;
        }

        //TODO: rewrite it
        public int IndexOf(DateTime dateTime, SearchOption option = SearchOption.ExactFirst)
        {
            int num = (int)this.series.Count - 1;
            if (dateTime == this.GetDateTime(num))
            {
                return num;
            }
            num = 0;
            int num2 = 0;
            int num3 = (int)this.series.Count - 1;
            bool flag = true;
            while (flag)
            {
                if (num3 < num2)
                {
                    return -1;
                }
                num = (num2 + num3) / 2;
                switch (option)
                {
                    case SearchOption.Next:
                        if (this.series[(long)num].DateTime >= dateTime && (num == 0 || this.series[(long)(num - 1)].DateTime < dateTime))
                        {
                            flag = false;
                        }
                        else if (this.series[(long)num].DateTime < dateTime)
                        {
                            num2 = num + 1;
                        }
                        else
                        {
                            num3 = num - 1;
                        }
                        break;
                    case SearchOption.Prev:
                        if (this.series[(long)num].DateTime <= dateTime && ((long)num == this.series.Count - 1L || this.series[(long)(num + 1)].DateTime > dateTime))
                        {
                            flag = false;
                        }
                        else if (this.series[(long)num].DateTime > dateTime)
                        {
                            num3 = num - 1;
                        }
                        else
                        {
                            num2 = num + 1;
                        }
                        break;
                    case SearchOption.ExactFirst:
                        if (this.series[(long)num].DateTime == dateTime)
                        {
                            flag = false;
                        }
                        else if (this.series[(long)num].DateTime > dateTime)
                        {
                            num3 = num - 1;
                        }
                        else if (this.series[(long)num].DateTime < dateTime)
                        {
                            num2 = num + 1;
                        }
                        break;
                }
            }
            return num;
        }

        public void Add(DateTime dateTime, double value)
        {
            var item = new TimeSeriesItem(dateTime, value);
            this.max = this.max == null ? item : (this.max.Value < item.Value ? item : this.max);
            this.min = this.min == null ? item : (this.min.Value > item.Value ? item : this.min);
            this.series.Add(item);

            // Update the dependent indicators
            foreach (var indicator in Indicators)
                if (indicator.AutoUpdate)
                    indicator.Update((int)this.series.Count - 1);
        }

        public void Remove(int index)
        {
            this.series.Remove(index);
        }

        public TimeSeriesItem GetByDateTime(DateTime dateTime, SearchOption option = SearchOption.ExactFirst)
        {
            int i = IndexOf(dateTime, option);
            return i == -1 ? null : GetItem(i);
        }

        public virtual DateTime GetDateTime(int index)
        {
            return GetItem(index).DateTime;
        }

        public TimeSeriesItem GetItem(int index)
        {
            return (TimeSeriesItem)this.series[index];
        }

        public TimeSeriesItem GetMaxItem()
        {
            return this.max;
        }

        public TimeSeriesItem GetMinItem()
        {
            return this.min;
        }

        //TODO: rewrite it
        public virtual int GetIndex(DateTime datetime, IndexOption option = IndexOption.Null)
        {
            int num = 0;
            int num2 = 0;
            int num3 = (int)this.series.Count - 1;
            bool flag = true;
            while (flag)
            {
                if (num3 < num2)
                {
                    return -1;
                }
                num = (num2 + num3) / 2;
                switch (option)
                {
                    case IndexOption.Null:
                        if (this.series[(long)num].DateTime == datetime)
                        {
                            flag = false;
                        }
                        else if (this.series[(long)num].DateTime > datetime)
                        {
                            num3 = num - 1;
                        }
                        else if (this.series[(long)num].DateTime < datetime)
                        {
                            num2 = num + 1;
                        }
                        break;
                    case IndexOption.Next:
                        if (this.series[(long)num].DateTime >= datetime && (num == 0 || this.series[(long)(num - 1)].DateTime < datetime))
                        {
                            flag = false;
                        }
                        else if (this.series[(long)num].DateTime < datetime)
                        {
                            num2 = num + 1;
                        }
                        else
                        {
                            num3 = num - 1;
                        }
                        break;
                    case IndexOption.Prev:
                        if (this.series[(long)num].DateTime <= datetime && ((long)num == this.series.Count - 1L || this.series[(long)(num + 1)].DateTime > datetime))
                        {
                            flag = false;
                        }
                        else if (this.series[(long)num].DateTime > datetime)
                        {
                            num3 = num - 1;
                        }
                        else
                        {
                            num2 = num + 1;
                        }
                        break;
                }
            }
            return num;
        }


        public double GetMax()
        {
            return this.max != null ? this.max.Value : double.NaN;
        }

        public double GetMax(int index1, int index2)
        {
            TimeSeriesItem result = null;
            for (int i = index1; i <= index2; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                result = result == null ? item : item.Value > result.Value ? item : result;
            }
            return result != null ? result.Value : double.NaN;
        }

        public virtual double GetMax(DateTime dateTime1, DateTime dateTime2)
        {
            TimeSeriesItem result = null;
            for (int i = 0; i < this.series.Count; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                if (dateTime1 <= item.DateTime && item.DateTime <= dateTime2)
                    result = result == null ? item : item.Value > result.Value ? item : result;
            }
            return result != null ? result.Value : double.NaN;
        }

        public virtual double GetMax(int index1, int index2, BarData barData)
        {
            return GetMax(index1, index2);
        }

        public double GetMin()
        {
            return this.min != null ? this.min.Value : double.NaN;
        }

        public double GetMin(int index1, int index2)
        {
            TimeSeriesItem result = null;
            for (int i = index1; i <= index2; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                result = result == null ? item : item.Value < result.Value ? item : result;
            }
            return result != null ? result.Value : double.NaN;
        }

        public virtual double GetMin(DateTime dateTime1, DateTime dateTime2)
        {
            TimeSeriesItem result = null;
            for (int i = 0; i < this.series.Count; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                if (dateTime1 <= item.DateTime && item.DateTime <= dateTime2)
                    result = result == null ? item : item.Value < result.Value ? item : result;
            }
            return result != null ? result.Value : double.NaN;
        }

        public virtual double GetMin(int index1, int index2, BarData barData)
        {
            return GetMin(index1, index2);
        }

        public static TimeSeries operator +(TimeSeries series1, TimeSeries series2)
        {
            EnsureNotNull(series1, series2);
            var name = $"({series1.Name}+{series2.Name})";
            return OpTwoTimeSeries(series1, series2, name, opAdd, false);
        }

        public static TimeSeries operator -(TimeSeries series1, TimeSeries series2)
        {
            EnsureNotNull(series1, series2);
            var name = $"({series1.Name}-{series2.Name})";
            return OpTwoTimeSeries(series1, series2, name, opSub, false);

        }

        public static TimeSeries operator *(TimeSeries series1, TimeSeries series2)
        {
            EnsureNotNull(series1, series2);
            var name = $"({series1.Name}*{series2.Name})";
            return OpTwoTimeSeries(series1, series2, name, opMul, false);
        }

        public static TimeSeries operator /(TimeSeries series1, TimeSeries series2)
        {
            EnsureNotNull(series1, series2);
            var name = $"({series1.Name}/{series2.Name})";
            var ts = new TimeSeries(name, "", -1);
            return OpTwoTimeSeries(series1, series2, name, opMul, true);

        }

        public static TimeSeries operator +(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}+{Value:F2})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opAdd, valueAsSecond: true, checkZero: false);
        }

        public static TimeSeries operator -(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}-{Value:F2})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opSub, valueAsSecond: true, checkZero: false);

        }

        public static TimeSeries operator *(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}*{Value:F2})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opMul, valueAsSecond: true, checkZero: false);
        }

        public static TimeSeries operator /(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}/{Value:F2})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opDiv, valueAsSecond: true, checkZero: true);
        }

        public static TimeSeries operator +(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}+{series.Name})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opAdd, valueAsSecond: false, checkZero: false);
        }

        public static TimeSeries operator -(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}-{series.Name})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opSub, valueAsSecond: false, checkZero: false);
        }

        public static TimeSeries operator *(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}*{series.Name})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opMul, valueAsSecond: false, checkZero: false);
        }

        public static TimeSeries operator /(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}/{series.Name})";
            var ts = new TimeSeries(name, "", -1);
            return OpTimeSeriesAndValue(series, Value, name, opDiv, valueAsSecond: false, checkZero: true);
        }

        public double Ago(int n)
        {
            int i = Count - n - 1;
            if (i < 0)
                throw new ArgumentException($"TimeSeries::Ago Can not return an entry {n} entries ago: time series is too short.");
            return this[i];
        }

        public TimeSeries Shift(int offset)
        {
            var ts = new TimeSeries(this.name, this.description, -1);
            int num = offset < 0 ? Math.Abs(offset) : 0;

            for (int i = num; i < Count; i++)
            {
                int num2 = i + offset;
                if (num2 >= this.Count)
                {
                    break;
                }
                DateTime dateTime = GetDateTime(num2);
                double value = this[i];
                ts[dateTime, SearchOption.ExactFirst] = value;
            }
            return ts;
        }

        #region Math Functions

        public TimeSeries Exp() => MathTransform($"Exp({this.name})", Math.Exp);

        public virtual TimeSeries Log() => MathTransform($"Log({this.name})", Math.Log);

        public TimeSeries Log10() => MathTransform($"Log10({this.name})", Math.Log10);

        public TimeSeries Pow(double pow)
        {
            Func<double, double> func = d => Math.Pow(d, pow);
            return MathTransform($"Pow({this.name})", func);
        }

        public TimeSeries Sqrt() => MathTransform($"Sqrt({this.name})", Math.Sqrt);

        private TimeSeries MathTransform(string name, Func<double, double> func)
        {
            var ts = new TimeSeries(name, this.description, -1);
            for (int i = 0; i < Count; i++)
                ts.Add(GetDateTime(i), func(this[i, 0]));
            return ts;
        }

        #endregion

        #region Statistics Functions
        public double GetSum()
        {
            return this.dirty ? this.sum = GetSum(0, Count - 1, 0) : this.sum;
        }

        public double GetSum(int index1, int index2, int row)
        {
            EnsureIndexInRange(index1, index2);
            return Enumerable.Range(index1, index2 - index1 + 1).Sum(i => this[i, row]);
        }

        public double GetMean()
        {
            EnsureNotEmpty("Can not calculate mean. Array is empty.");
            return this.dirty ? this.mean = GetMean(0, Count - 1, 0) : this.mean;
        }

        public virtual double GetMean(int row) => GetMean(0, Count - 1, row);

        public virtual double GetMean(int index1, int index2) => GetMean(index1, index2, 0);

        public virtual double GetMean(DateTime dateTime1, DateTime dateTime2) => GetMean(dateTime1, dateTime2, 0);

        public double GetMean(int index1, int index2, int row)
        {
            EnsureNotEmpty("Can not calculate mean. Array is empty.");
            return GetSum(index1, index2, row)/(double) (index2 - index1 + 1);
        }

        public double GetMean(DateTime dateTime1, DateTime dateTime2, int row)
        {
            EnsureNotEmpty("Can not calculate mean. Array is empty.");
            int i1, i2;
            EnsureIndexInRange(dateTime1, dateTime2, out i1, out i2);
            return GetMean(i1, i2, row);
        }

        public double GetMedian()
        {
            EnsureNotEmpty("Can not calculate median. Array is empty.");
            return this.dirty ? this.median = GetMedian(0, Count - 1) : this.median;
        }

        public virtual double GetMedian(int row) => GetMedian(0, this.Count - 1, row);

        public virtual double GetMedian(int index1, int index2) => GetMedian(index1, index2, 0);

        public virtual double GetMedian(DateTime dateTime1, DateTime dateTime2) => GetMedian(dateTime1, dateTime2, 0);

        public double GetMedian(int index1, int index2, int row)
        {
            EnsureNotEmpty("Can not calculate median. Array is empty.");
            EnsureIndexInRange(index1, index2);
            //TODO: Rewrite it
            var arr = Enumerable.Range(index1, index2 - index1 + 1).Select(i => this[i, row]).ToArray();
            Array.Sort(arr);
            return arr[arr.Count()/2];
        }

        public double GetMedian(DateTime dateTime1, DateTime dateTime2, int row)
        {
            EnsureNotEmpty("Can not calculate median. Array is empty.");
            int i1, i2;
            EnsureIndexInRange(dateTime1, dateTime2, out i1, out i2);
            return GetMedian(i1, i2, row);
        }

        public double GetCovariance(int row1, int row2, int index1, int index2)
        {
            EnsureNotEmpty("Can not calculate covariance. Array is empty.");
            EnsureIndexInRange(index1, index2);
            double m1 = GetMean(index1, index2, row1);
            double m2 = GetMean(index1, index2, row2);
            int count = index2 - index1 + 1;
            return count == 1 ? 0 : Enumerable.Range(index1, count).Sum(i => (this[i, row1] - m1) * (this[i, row2] - m2)) / (count - 1);
        }

        public double GetCovariance(TimeSeries series)
        {
            EnsureNotNull(series);
            var m1 = GetMean();
            var m2 = series.GetMean();
            int count = 0;
            double sum = 0;
            for (int i = 0; i < Count; ++i)
            {
                var dateTime = GetDateTime(i);
                if (series.Contains(dateTime))
                {
                    sum += (this[i] - m1) * (series[dateTime, SearchOption.ExactFirst] - m2);
                    ++count;
                }
            }
            return count <= 1 ? 0 : sum / (count - 1);
        }

        public double GetMoment(int k, int index1, int index2, int row)
        {
            EnsureNotEmpty($"Can not calculate momentum. Series {this.name} is empty.");
            EnsureIndexInRange(index1, index2);
            double m = k != 1 ? GetMean(index1, index2, row) : 0;
            var count = index2 - index1 + 1;
            return Enumerable.Range(index1, count).Sum(i => Math.Pow(this[i, row] - m, k)) / count;
        }

        #endregion
        private static TimeSeries OpTwoTimeSeries(TimeSeries ts1, TimeSeries ts2, string name, Func<double, double, double> op, bool checkZero = false)
        {
            var ts = new TimeSeries(name, "", -1);
            for (int i = 0; i < ts1.Count; ++i)
            {
                var datetime = ts1.GetDateTime(i);
                if (ts2.Contains(datetime))
                {
                    var d2 = ts2[datetime, SearchOption.ExactFirst];
                    if (!checkZero || d2 != 0.0)
                        ts.Add(datetime, op(ts1[datetime, 0, SearchOption.ExactFirst], d2));
                }
            }
            return ts;
        }

        private static TimeSeries OpTimeSeriesAndValue(TimeSeries series, double value, string name, Func<double, double, double> op, bool valueAsSecond = true, bool checkZero = false)
        {
            var ts = new TimeSeries(name, "", -1);
            for (int i = 0; i < series.Count; ++i)
            {
                var dt = series.GetDateTime(i);
                var sv = series[i, 0];
                if (valueAsSecond)
                {
                    ts.Add(dt, op(sv, value));
                }
                else
                {
                    if (!checkZero || sv != 0.0)
                        ts.Add(dt, op(value, sv));
                }
            }
            return ts;
        }

        private static void EnsureNotNull(params object[] objs)
        {
            if (objs.Any(o => o == null))
                throw new ArgumentNullException($"Operator argument can not be null");
        }

        private void EnsureAtLeastOneElement(string message = "")
        {
            if (Count <= 1)
                throw new ArgumentException("Can not calculate. Insufficient number of elements in the array.");
        }

        private void EnsureNotEmpty(string message = "")
        {
            if (Count <= 0)
                throw new ArgumentException(message);
        }

        private void EnsureIndexInRange(int index1, int index2)
        {
            if (index1 > index2)
                throw new ArgumentException($"{nameof(index1)} must be smaller than {nameof(index2)}");
            if (index1 < 0 || index2 > Count - 1)
                throw new ArgumentOutOfRangeException($"{nameof(index1)} is out of range");
            if (index2 < 0 || index2 > Count - 1)
                throw new ArgumentOutOfRangeException($"{nameof(index2)} is out of range");
        }

        private void EnsureIndexInRange(DateTime dt1, DateTime dt2, out int idx1, out int idx2)
        {
            if (dt1 >= dt2)
                throw new ArgumentException("dateTime1 must be smaller than dateTime2");
            idx1 = GetIndex(dt1, IndexOption.Null);
            idx2 = GetIndex(dt2, IndexOption.Null);
            if (idx1 == -1)
                throw new ArgumentOutOfRangeException("dateTime1 is out of range");
            if (idx2 == -1)
                throw new ArgumentOutOfRangeException("dateTime2 is out of range");
        }
    }
}