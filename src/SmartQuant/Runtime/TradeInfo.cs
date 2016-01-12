
using System;

namespace SmartQuant
{
    public class TradeInfo
    {
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
                throw new NotImplementedException();
            }
        }

        public double PnL
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}