using System;

namespace SmartQuant
{
    public class DataProcessor
    {
        public DataProcessor()
        {
        }

        protected void Emit(DataObject obj)
        {
            throw new NotImplementedException();
        }

        protected virtual DataObject OnData(DataObject obj)
        {
            throw new NotImplementedException();
        }

        public bool EmitBar { get; set; }

        public bool EmitBarCloseTrade { get; set; }

        public bool EmitBarHighTrade { get; set; }

        public bool EmitBarLowTrade { get; set; }

        public bool EmitBarOpen { get; set; }

        public bool EmitBarOpenTrade { get; set; }

    }
}