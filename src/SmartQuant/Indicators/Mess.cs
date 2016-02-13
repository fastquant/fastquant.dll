using System;
using System.ComponentModel;

namespace SmartQuant.Indicators
{
    public class EWM : Indicator
    {
        public double Length { get; }

        public double InitValue { get; }

        public EWM(ISeries input, double length, double initValue = 0) : base(input)
        {
            Length = length;
            InitValue = initValue;
        }

        public override void Calculate(int index)
        {
            var v = index == 0
                ? InitValue
                : Math.Exp(-1.0/Length)*this[index - 1] + (1.0 - Math.Exp(-1.0/Length))*this.input[index];

            Add(this.input.GetDateTime(index), v);
        }
    }

    public class EWV : Indicator
    {
        private TimeSeries timeSeries_0;
        private EWM ewm_0;
        private EWM ewm_1;

        public double Length { get; }

        public double InitValue { get; }

        public EWV(ISeries input, double length, double initValue = 0) : base(input)
        {
            Length = length;
            InitValue = initValue;
            this.ewm_0 = new EWM(input, length);
            this.ewm_0.AutoUpdate = false;
            this.timeSeries_0 = new TimeSeries();
            this.ewm_1 = new EWM(this.timeSeries_0, length);
        }

        public override void Calculate(int index)
        {
            this.ewm_0.Calculate(index);
            this.timeSeries_0.Add(this.input.GetDateTime(index), Math.Pow(this.input[index], 2.0));
            if (index > 0)
            {
                Console.WriteLine(this.ewm_1.Last + " " + Math.Pow(this.ewm_0.Last, 2.0));
                Add(this.input.GetDateTime(index), Math.Sqrt(this.ewm_1.Last - Math.Pow(this.ewm_0.Last, 2.0)));
            }
            else
                Add(this.input.GetDateTime(index), InitValue);
        }
    }


    [Serializable]
    public class TRIX : Indicator
    {
        public TRIX(ISeries input, int length, BarData barData = BarData.Close) : base(input)
        {
        }
    }

    [Serializable]
    public class UltOsc : Indicator
    {
        protected int n1;
        protected int n2;
        protected int n3;

        [Category("Parameters"), Description("")]
        public int N1
        {
            get
            {
                return this.n1;
            }
            set
            {
                this.n1 = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public int N2
        {
            get
            {
                return this.n2;
            }
            set
            {
                this.n2 = value;
                Init();
            }
        }

        [Category("Parameters"), Description("")]
        public int N3
        {
            get
            {
                return this.n3;
            }
            set
            {
                this.n3 = value;
                Init();
            }
        }

        public UltOsc(ISeries input, int n1, int n2, int n3) : base(input)
        {
            this.n1 = n1;
            this.n2 = n2;
            this.n3 = n3;
            Init();
        }

        protected override void Init()
        {
            this.name = $"UOSC ({this.n1}, {this.n2}, {this.n3})";
            this.description = "Ultimate Oscillator";
            Clear();
            this.calculate = true;
        }

        public override void Calculate(int index)
        {
            var value = Value(this.input, index, this.n1, this.n2, this.n3);
            if (!double.IsNaN(value))
                Add(this.input.GetDateTime(index), value);
        }

        public static double Value(ISeries input, int index, int n1, int n2, int n3)
        {
            if (index >= Math.Max(n1, Math.Max(n2, n3)))
            {
                double num = 0.0;
                double num2 = 0.0;
                for (int i = index; i > index - n1; i--)
                {
                    double num3 = input[i, BarData.Close];
                    double val = input[i - 1, BarData.Close];
                    double val2 = input[i, BarData.Low];
                    num += num3 - Math.Min(val2, val);
                    num2 += TR.Value(input, i);
                }
                double num4 = (double)(n3 / n1) * (num / num2);
                num = 0.0;
                num2 = 0.0;
                for (int j = index; j > index - n2; j--)
                {
                    double num3 = input[j, BarData.Close];
                    double val = input[j - 1, BarData.Close];
                    double val2 = input[j, BarData.Low];
                    num += num3 - Math.Min(val2, val);
                    num2 += TR.Value(input, j);
                }
                double num5 = (double)(n3 / n2) * (num / num2);
                num = 0.0;
                num2 = 0.0;
                for (int k = index; k > index - n3; k--)
                {
                    double num3 = input[k, BarData.Close];
                    double val = input[k - 1, BarData.Close];
                    double val2 = input[k, BarData.Low];
                    num += num3 - Math.Min(val2, val);
                    num2 += TR.Value(input, k);
                }
                double num6 = num / num2;
                return (num4 + num5 + num6) / (double)(n3 / n1 + n3 / n2 + 1) * 100.0;
            }
            return double.NaN;
        }
    }
}