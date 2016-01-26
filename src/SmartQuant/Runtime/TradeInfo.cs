
using System;

namespace SmartQuant
{
    public class TradeInfo
    {
        private double double_0;
        private double double_1;
        private double double_2;
        private double double_3;

        public byte BaseCurrencyId { get; set; }

        public double Qty { get; set; }

        public double EntryCost { get; set; }

        public DateTime EntryDate { get; set; }

        public double EntryPrice { get; set; }

        public double ETD { get; set; }

        public double ExitCost { get; set; }

        public DateTime ExitDate { get; set; }

        public double ExitPrice { get; set; }

        public Instrument Instrument { get; set; }

        public bool IsLong { get; set; }

        public bool IsWinning => NetPnL > 0.0;

        public long Length => (ExitDate - EntryDate).Ticks;

        public double MAE { get; set; }

        public double MFE { get; set; }

        public double NetPnL
        {
            get
            {
                double num = (Instrument.Factor == 0.0) ? 1.0 : this.Instrument.Factor;
                double num2 = num * Qty * (ExitPrice - EntryPrice) * (IsLong ? 1 : -1);
                num2 -= EntryCost + ExitCost;
                if (this.Instrument.CurrencyId != this.BaseCurrencyId)
                {
                    if (this.double_1 == num2)
                    {
                        return this.double_3;
                    }
                    this.double_1 = num2;
                    num2 = this.Instrument.Framework.CurrencyConverter.Convert(num2, Instrument.CurrencyId, BaseCurrencyId);
                    this.double_3 = num2;
                }
                return num2;
            }
        }

        public double PnL
        {
            get
            {
                double factor = (Instrument.Factor == 0) ? 1 : Instrument.Factor;
                double num2 = factor * Qty * (ExitPrice - EntryPrice) * (IsLong ? 1 : -1);
                if (Instrument.CurrencyId != BaseCurrencyId)
                {
                    if (this.double_0 == num2)
                    {
                        return this.double_2;
                    }
                    this.double_0 = num2;
                    num2 = Instrument.Framework.CurrencyConverter.Convert(num2, Instrument.CurrencyId, BaseCurrencyId);
                    this.double_2 = num2;
                }
                return num2;
            }
        }
    }
}