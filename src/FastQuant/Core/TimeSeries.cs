// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
 
namespace FastQuant
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

        private static readonly Func<double, double, double> opAdd = (a, b) => a + b;
        private static readonly Func<double, double, double> opSub = (a, b) => a - b;
        private static readonly Func<double, double, double> opMul = (a, b) => a * b;
        private static readonly Func<double, double, double> opDiv = (a, b) => a / b;

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

        public double this[DateTime dateTime, int row, SearchOption option = SearchOption.ExactFirst] => GetByDateTime(dateTime, option).Value;

        public virtual int Count => (int)this.series.Count;

        public virtual double First => this[0];

        public virtual double Last => this[Count - 1];

        public virtual DateTime FirstDateTime => this.series[0].DateTime;

        public virtual DateTime LastDateTime => this.series[this.series.Count - 1].DateTime;

        public double GetValue(int index) => this[index];

        public bool Contains(DateTime dateTime) => IndexOf(dateTime, SearchOption.ExactFirst) != -1;

        public Cross Crosses(TimeSeries series, DateTime dateTime)
        {
            var num = IndexOf(dateTime);
            var num2 = series.IndexOf(dateTime);
            if (num <= 0 || num >= this.series.Count)
            {
                return Cross.None;
            }
            if (num2 > 0 && num2 < series.Count)
            {
                DateTime dateTime2 = GetDateTime(num - 1);
                DateTime dateTime3 = series.GetDateTime(num2 - 1);
                if (dateTime2 == dateTime3)
                {
                    if (this.GetValue(num - 1) <= series.GetValue(num2 - 1) && this.GetValue(num) > series.GetValue(num2))
                    {
                        return Cross.Above;
                    }
                    if (this.GetValue(num - 1) >= series.GetValue(num2 - 1) && this.GetValue(num) < series.GetValue(num2))
                    {
                        return Cross.Below;
                    }
                }
                else
                {
                    double value;
                    double value2;
                    if (dateTime2 < dateTime3)
                    {
                        DateTime dateTime4 = this.GetDateTime(num - 1);
                        value = this.GetValue(num - 1);
                        if (series.IndexOf(dateTime4, SearchOption.Next) != num2)
                        {
                            value2 = series.GetValue(series.IndexOf(dateTime4, SearchOption.Next));
                        }
                        else
                        {
                            value2 = series.GetValue(series.IndexOf(dateTime4, SearchOption.Prev));
                        }
                    }
                    else
                    {
                        DateTime dateTime5 = series.GetDateTime(num2 - 1);
                        value2 = series.GetValue(num2 - 1);
                        if (this.IndexOf(dateTime5, SearchOption.Prev) != num)
                        {
                            value = this.GetValue(this.IndexOf(dateTime5, SearchOption.Next));
                        }
                        else
                        {
                            value = this.GetValue(this.IndexOf(dateTime5, SearchOption.Prev));
                        }
                    }
                    if (value <= value2 && this.GetValue(num) > series.GetValue(num2))
                    {
                        return Cross.Above;
                    }
                    if (value >= value2 && this.GetValue(num) < series.GetValue(num2))
                    {
                        return Cross.Below;
                    }
                }
                return Cross.None;
            }
            return Cross.None;
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
            foreach (var indicator in Indicators.Where(i => i.AutoUpdate))
                indicator.Update((int)this.series.Count - 1);
        }

        public void Remove(int index) => this.series.Remove(index);

        public TimeSeriesItem GetByDateTime(DateTime dateTime, SearchOption option = SearchOption.ExactFirst)
        {
            var i = IndexOf(dateTime, option);
            return i == -1 ? null : GetItem(i);
        }

        public virtual DateTime GetDateTime(int index) => GetItem(index).DateTime;

        public TimeSeriesItem GetItem(int index) => (TimeSeriesItem)this.series[index];

        public TimeSeriesItem GetMaxItem() => this.max;

        public TimeSeriesItem GetMinItem() => this.min;

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

        public double GetMax() => this.max?.Value ?? double.NaN;

        public double GetMax(int index1, int index2)
        {
            TimeSeriesItem result = null;
            for (int i = index1; i <= index2; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                result = result == null ? item : item.Value > result.Value ? item : result;
            }
            return result?.Value ?? double.NaN;
        }

        public virtual double GetMax(DateTime dateTime1, DateTime dateTime2)
        {
            TimeSeriesItem result = null;
            for (var i = 0; i < this.series.Count; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                if (dateTime1 <= item.DateTime && item.DateTime <= dateTime2)
                    result = result == null ? item : item.Value > result.Value ? item : result;
            }
            return result?.Value ?? double.NaN;
        }

        public virtual double GetMax(int index1, int index2, BarData barData) => GetMax(index1, index2);

        public double GetMin() => this.min?.Value ?? double.NaN;

        public double GetMin(int index1, int index2)
        {
            TimeSeriesItem result = null;
            for (var i = index1; i <= index2; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                result = result == null ? item : item.Value < result.Value ? item : result;
            }
            return result?.Value ?? double.NaN;
        }

        public virtual double GetMin(DateTime dateTime1, DateTime dateTime2)
        {
            TimeSeriesItem result = null;
            for (var i = 0; i < this.series.Count; ++i)
            {
                var item = this.series[i] as TimeSeriesItem;
                if (dateTime1 <= item.DateTime && item.DateTime <= dateTime2)
                    result = result == null ? item : item.Value < result.Value ? item : result;
            }
            return result?.Value ?? double.NaN;
        }

        public virtual double GetMin(int index1, int index2, BarData barData) => GetMin(index1, index2);

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
            return OpTwoTimeSeries(series1, series2, name, opMul, true);

        }

        public static TimeSeries operator +(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}+{Value:F2})";
            return OpTimeSeriesAndValue(series, Value, name, opAdd, valueAsSecond: true, checkZero: false);
        }

        public static TimeSeries operator -(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}-{Value:F2})";
            return OpTimeSeriesAndValue(series, Value, name, opSub, valueAsSecond: true, checkZero: false);
        }

        public static TimeSeries operator *(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}*{Value:F2})";
            return OpTimeSeriesAndValue(series, Value, name, opMul, valueAsSecond: true, checkZero: false);
        }

        public static TimeSeries operator /(TimeSeries series, double Value)
        {
            EnsureNotNull(series);
            var name = $"({series.Name}/{Value:F2})";
            return OpTimeSeriesAndValue(series, Value, name, opDiv, valueAsSecond: true, checkZero: true);
        }

        public static TimeSeries operator +(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}+{series.Name})";
            return OpTimeSeriesAndValue(series, Value, name, opAdd, valueAsSecond: false, checkZero: false);
        }

        public static TimeSeries operator -(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}-{series.Name})";
            return OpTimeSeriesAndValue(series, Value, name, opSub, valueAsSecond: false, checkZero: false);
        }

        public static TimeSeries operator *(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}*{series.Name})";
            return OpTimeSeriesAndValue(series, Value, name, opMul, valueAsSecond: false, checkZero: false);
        }

        public static TimeSeries operator /(double Value, TimeSeries series)
        {
            EnsureNotNull(series);
            var name = $"({Value:F2}/{series.Name})";
            return OpTimeSeriesAndValue(series, Value, name, opDiv, valueAsSecond: false, checkZero: true);
        }

        public double Ago(int n)
        {
            var i = Count - n - 1;
            if (i < 0)
                throw new ArgumentException($"TimeSeries::Ago Can not return an entry {n} entries ago: time series is too short.");
            return this[i];
        }

        public TimeSeries Shift(int offset)
        {
            var ts = new TimeSeries(this.name, this.description, -1);
            var num = offset < 0 ? Math.Abs(offset) : 0;

            for (var i = num; i < Count; i++)
            {
                var num2 = i + offset;
                if (num2 >= Count)
                {
                    break;
                }
                var dateTime = GetDateTime(num2);
                var value = this[i];
                ts[dateTime] = value;
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

        public double GetSum() => this.dirty ? this.sum = GetSum(0, Count - 1, 0) : this.sum;

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

        public double GetMedian() => this.dirty ? this.median = GetMedian(0, Count - 1) : this.median;

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

        public double GetVariance() => this.dirty ? this.variance = GetVariance(0, Count - 1, 0) : this.variance;

        public virtual double GetVariance(int row) => GetVariance(0, Count - 1, row);

        public virtual double GetVariance(int index1, int index2) => GetVariance(index1, index2, 0);

        public virtual double GetVariance(DateTime dateTime1, DateTime dateTime2) => GetVariance(dateTime1, dateTime2, 0);

        public double GetVariance(int index1, int index2, int row)
        {
            EnsureAtLeastOneElement();
            EnsureIndexInRange(index1, index2);
            double mean = GetMean(index1, index2, row);
            int count = index2 - index1 + 1;
            return Enumerable.Range(index1, count).Sum(i => (mean - this[i, row]) * (mean - this[i, row])) / (count - 1);
        }

        public virtual double GetVariance(DateTime dateTime1, DateTime dateTime2, int row)
        {
            EnsureAtLeastOneElement();
            int i1, i2;
            EnsureIndexInRange(dateTime1, dateTime2, out i1, out i2);
            return GetVariance(i1, i2, row);
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

        public double GetStdDev() => Math.Sqrt(GetVariance());

        public double GetStdDev(int row) => Math.Sqrt(GetVariance(row));

        public double GetStdDev(int index1, int index2) => Math.Sqrt(this.GetVariance(index1, index2));

        public double GetStdDev(DateTime dateTime1, DateTime dateTime2) => Math.Sqrt(this.GetVariance(dateTime1, dateTime2));

        public double GetStdDev(int index1, int index2, int row) => Math.Sqrt(this.GetVariance(index1, index2, row));

        public double GetStdDev(DateTime dateTime1, DateTime dateTime2, int row) => Math.Sqrt(this.GetVariance(dateTime1, dateTime2, row));

        public double GetMoment(int k, int index1, int index2, int row)
        {
            EnsureNotEmpty($"Can not calculate momentum. Series {this.name} is empty.");
            EnsureIndexInRange(index1, index2);
            double m = k != 1 ? GetMean(index1, index2, row) : 0;
            var count = index2 - index1 + 1;
            return Enumerable.Range(index1, count).Sum(i => Math.Pow(this[i, row] - m, k)) / count;
        }

        public double GetAsymmetry(int index1, int index2, int row)
        {
            EnsureIndexInRange(index1, index2);
            var sd = GetStdDev(index1, index2, row);
            return sd == 0 ? 0 : GetMoment(3, index1, index2, row)/Math.Pow(sd, 3.0);
        }

        public double GetExcess(int index1, int index2, int row)
        {
            EnsureNotEmpty($"Can not calculate excess. Series {this.name} is empty.");
            EnsureIndexInRange(index1,index2);
            double sd = GetStdDev(index1, index2, row);
            return sd == 0 ? 0 : GetMoment(4, index1, index2, row)/Math.Pow(sd, 4);
        }

        public double GetAutoCorrelation(int lag) => GetAutoCovariance(lag)/GetVariance();

        public virtual double GetAutoCovariance(int lag)
        {
            if (lag >= Count)
                throw new ArgumentException("Not enough data points in the series to calculate autocovariance");
            var m = GetMean();
            return Enumerable.Range(lag, Count - lag).Sum(i => (this[i, 0] - m)*(this[i - lag, 0] - m))/(Count - lag);
        }

        public double GetCorrelation(TimeSeries series) => GetCovariance(series)/(GetStdDev()*series.GetStdDev());

        public double GetCorrelation(int row1, int row2, int index1, int index2)
        {
            return GetCovariance(row1, row2, index1, index2) / (GetStdDev(index1, index2, row1) * GetStdDev(index1, index2, row2));
        }

        public virtual double GetPositiveVariance() => GetPositiveVariance(0);

        public virtual double GetPositiveVariance(int row) => GetPositiveVariance(0, Count - 1, row);

        public virtual double GetPositiveVariance(int index1, int index2) => GetPositiveVariance(index1, index2, 0);

        public virtual double GetPositiveVariance(DateTime dateTime1, DateTime dateTime2) => GetPositiveVariance(dateTime1, dateTime2, 0);

        public double GetPositiveVariance(int index1, int index2, int row)
        {
            EnsureAtLeastOneElement();
            EnsureIndexInRange(index1, index2);

            int cnt = 0;
            double pmean = 0;
            for (var i = index1; i <= index2; i++)
            {
                if (this[i, row] > 0)
                {
                    pmean += this[i, row];
                    cnt++;
                }
            }
            pmean /= cnt;

            double pv = 0;
            for (var i = index1; i <= index2; i++)
                if (this[i, row] > 0.0)
                    pv += (pmean - this[i, row])*(pmean - this[i, row]);
            return pv/cnt;
        }

        public virtual double GetPositiveVariance(DateTime dateTime1, DateTime dateTime2, int row)
        {
            EnsureAtLeastOneElement();
            int i1, i2;
            EnsureIndexInRange(dateTime1, dateTime2, out i1, out i2);
            return GetPositiveVariance(i1, i2, row);
        }

        public double GetPositiveStdDev() => Math.Sqrt(GetPositiveVariance());

        public double GetPositiveStdDev(int row) => Math.Sqrt(GetPositiveVariance(row));

        public double GetPositiveStdDev(int index1, int index2) => Math.Sqrt(GetPositiveVariance(index1, index2));

        public double GetPositiveStdDev(DateTime dateTime1, DateTime dateTime2) => Math.Sqrt(GetPositiveVariance(dateTime1, dateTime2));

        public double GetPositiveStdDev(int index1, int index2, int row) => Math.Sqrt(GetPositiveVariance(index1, index2, row));

        public double GetPositiveStdDev(DateTime dateTime1, DateTime dateTime2, int row) => Math.Sqrt(GetPositiveVariance(dateTime1, dateTime2, row));

        public virtual double GetNegativeVariance() => GetNegativeVariance(0);

        public virtual double GetNegativeVariance(int row) => GetNegativeVariance(0, Count - 1, row);

        public virtual double GetNegativeVariance(int index1, int index2) => GetNegativeVariance(index1, index2, 0);

        public virtual double GetNegativeVariance(DateTime dateTime1, DateTime dateTime2) => GetNegativeVariance(dateTime1, dateTime2, 0);

        public double GetNegativeVariance(int index1, int index2, int row)
        {
            EnsureAtLeastOneElement();
            EnsureIndexInRange(index1, index2);

            int cnt = 0;
            double nmean = 0;
            for (int i = index1; i <= index2; i++)
            {
                if (this[i, row] < 0)
                {
                    nmean += this[i, row];
                    cnt++;
                }
            }
            nmean /= (double) cnt;

            double nv = 0.0;
            for (int j = index1; j <= index2; j++)
                if (this[j, row] < 0)
                    nv += (nmean - this[j, row])*(nmean - this[j, row]);
            return nv/(double) cnt;
        }

        public virtual double GetNegativeVariance(DateTime dateTime1, DateTime dateTime2, int row)
        {
            EnsureAtLeastOneElement();
            int i1, i2;
            EnsureIndexInRange(dateTime1,dateTime2,out i1,out i2);
            return GetNegativeVariance(i1, i2, row);
        }

        public double GetNegativeStdDev() => Math.Sqrt(GetNegativeVariance());

        public double GetNegativeStdDev(int row) => Math.Sqrt(GetNegativeVariance(row));

        public double GetNegativeStdDev(int index1, int index2) => Math.Sqrt(GetNegativeVariance(index1, index2));

        public double GetNegativeStdDev(DateTime dateTime1, DateTime dateTime2) => Math.Sqrt(GetNegativeVariance(dateTime1, dateTime2));

        public double GetNegativeStdDev(int index1, int index2, int row) => Math.Sqrt(GetNegativeVariance(index1, index2, row));

        public double GetNegativeStdDev(DateTime dateTime1, DateTime dateTime2, int row) => Math.Sqrt(GetNegativeVariance(dateTime1, dateTime2, row));

        #endregion

        public virtual TimeSeries GetPositiveSeries()
        {
            var ts = new TimeSeries();
            for (int i = 0; i < Count; i++)
                if (this[i] > 0)
                    ts.Add(GetDateTime(i), this[i]);
            return ts;
        }

        public virtual TimeSeries GetNegativeSeries()
        {
            var ts = new TimeSeries();
            for (int i = 0; i < Count; i++)
                if (this[i] < 0)
                    ts.Add(GetDateTime(i), this[i]);
            return ts;
        }

        public virtual TimeSeries GetReturnSeries()
        {
            var ts = new TimeSeries(this.name, this.description + " (return)", -1);
            if (Count <= 1)
                return ts;

            double p0 = this[0];
            for (var i = 0; i < Count; i++)
            {
                var p1 = this[i];
                ts.Add(GetDateTime(i), p0 != 0 ? p1/p0 : 0);
                p0 = p1;
            }
            return ts;
        }


        [NotOriginal]
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

        [NotOriginal]
        private static TimeSeries OpTimeSeriesAndValue(TimeSeries series, double value, string name, Func<double, double, double> op, bool valueAsSecond = true, bool checkZero = false)
        {
            var ts = new TimeSeries(name, "", -1);
            for (int i = 0; i < series.Count; ++i)
            {
                var dt = series.GetDateTime(i);
                var sv = series[i, 0];
                if (valueAsSecond)
                    ts.Add(dt, op(sv, value));
                else
                {
                    if (!checkZero || sv != 0.0)
                        ts.Add(dt, op(value, sv));
                }
            }
            return ts;
        }

        [NotOriginal]
        private static void EnsureNotNull(params object[] objs)
        {
            if (objs.Any(o => o == null))
                throw new ArgumentNullException($"Operator argument can not be null");
        }

        [NotOriginal]
        private void EnsureAtLeastOneElement(string message = "")
        {
            if (Count <= 1)
                throw new ArgumentException("Can not calculate. Insufficient number of elements in the array.");
        }

        [NotOriginal]
        private void EnsureNotEmpty(string message = "")
        {
            if (Count <= 0)
                throw new ArgumentException(message);
        }

        [NotOriginal]
        private void EnsureIndexInRange(int index1, int index2)
        {
            if (index1 > index2)
                throw new ArgumentException($"{nameof(index1)} must be smaller than {nameof(index2)}");
            if (index1 < 0 || index2 > Count - 1)
                throw new ArgumentOutOfRangeException($"{nameof(index1)} is out of range");
            if (index2 < 0 || index2 > Count - 1)
                throw new ArgumentOutOfRangeException($"{nameof(index2)} is out of range");
        }

        [NotOriginal]
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