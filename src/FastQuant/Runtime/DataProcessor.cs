using System;

namespace FastQuant
{
    public class DataProcessor
    {
        internal DataSeriesObject emitter;

        protected void Emit(DataObject obj)
        {
            if (!this.emitter.queue.IsFull())
                this.emitter.queue.Write(obj);
            else
                Console.WriteLine($"DataProcessor::Emit Can not write data object. Data queue is full. Data queue size = {this.emitter.queue.Size}");
        }

        protected virtual DataObject OnData(DataObject obj)
        {
            if (obj.TypeId == DataObjectType.Bar)
            {
                var bar = (Bar)obj;
                if (EmitBarOpen)
                    Emit(new Bar(bar.OpenDateTime, bar.OpenDateTime, bar.InstrumentId, bar.Type, bar.Size, bar.Open, 0, 0, 0, 0, 0));

                if (EmitBarOpenTrade)
                    Emit(new Trade(bar.DateTime, 0, bar.InstrumentId, bar.Open, (int)(bar.Volume / 4)));

                if (EmitBarHighTrade && EmitBarLowTrade)
                {
                    if (bar.Close > bar.Open)
                    {
                        Emit(new Trade(new DateTime(bar.DateTime.Ticks + (bar.CloseDateTime.Ticks - bar.DateTime.Ticks) / 3), 0, bar.InstrumentId, bar.Low, (int)(bar.Volume / 4)));
                        Emit(new Trade(new DateTime(bar.DateTime.Ticks + (bar.CloseDateTime.Ticks - bar.DateTime.Ticks) * 2 / 3), 0, bar.InstrumentId, bar.High, (int)(bar.Volume / 4)));
                    }
                    else
                    {
                        Emit(new Trade(new DateTime(bar.DateTime.Ticks + (bar.CloseDateTime.Ticks - bar.DateTime.Ticks) / 3), 0, bar.InstrumentId, bar.High, (int)(bar.Volume / 4L)));
                        Emit(new Trade(new DateTime(bar.DateTime.Ticks + (bar.CloseDateTime.Ticks - bar.DateTime.Ticks) * 2 / 3), 0, bar.InstrumentId, bar.Low, (int)(bar.Volume / 4)));
                    }
                }
                else
                {
                    if (EmitBarHighTrade)
                        Emit(new Trade(new DateTime(bar.DateTime.Ticks + (bar.CloseDateTime.Ticks - bar.DateTime.Ticks) / 2), 0, bar.InstrumentId, bar.High, (int)(bar.Volume / 4)));

                    if (EmitBarLowTrade)
                        Emit(new Trade(new DateTime(bar.DateTime.Ticks + (bar.CloseDateTime.Ticks - bar.DateTime.Ticks) / 2), 0, bar.InstrumentId, bar.Low, (int)(bar.Volume / 4)));
                }

                if (EmitBarCloseTrade)
                    Emit(new Trade(bar.CloseDateTime, 0, bar.InstrumentId, bar.Close, (int)(bar.Volume / 4)));

                if (!EmitBar)
                    return null;
            }
            return obj;
        }

        public bool EmitBar { get; set; } = true;
        public bool EmitBarOpen { get; set; } = true;
        public bool EmitBarHighTrade { get; set; }
        public bool EmitBarLowTrade { get; set; }
        public bool EmitBarOpenTrade { get; set; }
        public bool EmitBarCloseTrade { get; set; }

        internal DataObject Process(DataSeriesObject emitter)
        {
            this.emitter = emitter;
            return OnData(emitter.obj);
        }
    }
}