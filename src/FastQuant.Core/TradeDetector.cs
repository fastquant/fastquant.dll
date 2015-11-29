using System;
using System.Collections.Generic;

namespace SmartQuant
{
    public enum TradeDetectionType
    {
        FIFO,
        LIFO
    }

    class EventArgs1 : EventArgs
    {
        public TradeInfo TradeInfo { get; set; }
        public EventArgs1(TradeInfo tradeInfo)
        {
            TradeInfo = tradeInfo;
        }
    }

    internal delegate void Delegate1(object sender, EventArgs1 e);

    public class TradeDetector
    {
        internal Portfolio Portfolio { get; }

        public DateTime OpenDateTime { get; private set; }
        public bool HasPosition { get; private set; }

        public List<TradeInfo> Trades { get; } = new List<TradeInfo>();
        internal event Delegate1 Detected;
        public TradeDetector(TradeDetectionType type, Portfolio portfolio)
        {
            Portfolio = portfolio;
        }

        public void Add(Fill fill)
        {
            throw new NotImplementedException();
        }

        public void OnEquity(double equity)
        {
            throw new NotImplementedException();
        }
    }
}