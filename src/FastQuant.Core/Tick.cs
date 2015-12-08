using System;

namespace SmartQuant
{
    public enum TickType
    {
        Bid,
        Ask,
        Trade
    }

    public class Tick : DataObject
    {
        public byte ProviderId { get; set; }

        public int InstrumentId { get; set; }

        public double Price { get; set; }

        public int Size { get; set; }

        public DateTime ExchangeDateTime { get; set; }

        public Tick()
        {
        }

        public Tick(Tick tick)
            : this(tick.DateTime, tick.ExchangeDateTime, tick.ProviderId, tick.InstrumentId, tick.Price, tick.Size)
        {
        }

        public Tick(DateTime dateTime, byte providerId, int instrumentId, double price, int size)
            : this(dateTime, default(DateTime), providerId, instrumentId, price, size)
        {
        }

        public Tick(DateTime dateTime, DateTime exchangeDateTime, byte providerId, int instrumentId, double price, int size)
            : base(dateTime)
        {
            ExchangeDateTime = exchangeDateTime;
            ProviderId = providerId;
            InstrumentId = instrumentId;
            Price = price;
            Size = size;
        }

        public override string ToString() => $"Tick {DateTime} {ProviderId} {InstrumentId} {Price} {Size}";
    }
}