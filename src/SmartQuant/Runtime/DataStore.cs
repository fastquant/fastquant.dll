// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace SmartQuant
{
    public class DataStore
    {
        private IdArray<TickSeries> ticks = new IdArray<TickSeries>();
        private IdArray<TickSeries> asks = new IdArray<TickSeries>();
        private IdArray<TickSeries> bids = new IdArray<TickSeries>();
        private IdArray<TickSeries> trades = new IdArray<TickSeries>();
        private IdArray<IdArray<IdArray<BarSeries>>> bars = new IdArray<IdArray<IdArray<BarSeries>>>();

        public void Add(Bar bar)
        {
            var barsWithInstrumentId = this.bars[bar.InstrumentId] = this.bars[bar.InstrumentId] ?? new IdArray<IdArray<BarSeries>>(8);
            var barsWithInstrumentIdAndType = barsWithInstrumentId[(int)bar.Type] = barsWithInstrumentId[(int)bar.Type] ?? new IdArray<BarSeries>();
            var barsWithInstrumentIdAndTypeAndSize = barsWithInstrumentIdAndType[(int)bar.Size] = barsWithInstrumentIdAndType[(int)bar.Size] ?? new BarSeries("", "", -1, -1);
            barsWithInstrumentIdAndTypeAndSize.Add(bar);
        }

        public void Add(Tick tick)
        {
            GetOrCreateTickSeriesFor(this.ticks, tick.InstrumentId).Add(tick);
            switch (tick.TypeId)
            {
                case EventType.Bid:
                    GetOrCreateTickSeriesFor(this.bids, tick.InstrumentId).Add(tick);
                    break;
                case EventType.Ask:
                    GetOrCreateTickSeriesFor(this.asks, tick.InstrumentId).Add(tick);
                    break;
                case EventType.Trade:
                    GetOrCreateTickSeriesFor(this.trades, tick.InstrumentId).Add(tick);
                    break;
            }
        }

        public TickSeries GetTickSeries(Instrument instrument) => this.ticks[instrument.Id];

        public TickSeries GetAskSeries(Instrument instrument) => this.asks[instrument.Id];

        public TickSeries GetBidSeries(Instrument instrument) => this.bids[instrument.Id];

        public TickSeries GetTradeSeries(Instrument instrument) => this.trades[instrument.Id];

        public BarSeries GetBarSeries(Instrument instrument, BarType type, long barSize) => this.bars?[instrument.Id]?[(int)type]?[(int)barSize];

        public void Clear()
        {
        }

        private TickSeries GetOrCreateTickSeriesFor(IdArray<TickSeries> array, int instrumentId) => array[instrumentId] = array[instrumentId] ?? new TickSeries("", "");
    }
}
