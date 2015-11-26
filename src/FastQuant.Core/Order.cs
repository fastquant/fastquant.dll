using System.ComponentModel;

namespace SmartQuant
{
    public enum OrderType : byte
    {
        Market,
        Stop,
        Limit,
        StopLimit,
        MarketOnClose,
        Pegged,
        TrailingStop,
        TrailingStopLimit
    }

    public enum OrderStatus : byte
    {
        NotSent,
        PendingNew,
        New,
        Rejected,
        PartiallyFilled,
        Filled,
        PendingCancel,
        Cancelled,
        Expired,
        PendingReplace,
        Replaced
    }

    public enum OrderSide : byte
    {
        Buy,
        Sell
    }

    public enum TimeInForce : byte
    {
        ATC,
        Day,
        GTC,
        IOC,
        OPG,
        OC,
        FOK,
        GTX,
        GTD,
        GFS,
        AUC
    }

    public enum ExecType
    {
        ExecNew,
        ExecRejected,
        ExecTrade,
        ExecPendingCancel,
        ExecCancelled,
        ExecCancelReject,
        ExecPendingReplace,
        ExecReplace,
        ExecReplaceReject
    }

    public class Order
    {
        public string Text { get; set; }

        public double AvgPx { get; set; }
        public int Id { get; internal set; }
        public bool IsDone { get; set; }
        public double Price { get; set; }
        [ReadOnly(true)]
        public double Qty { get; set; }
        public OrderSide Side { get; set; }
        public double StopPx { get; set; }
        public OrderType Type { get; set; }
        public string OCA { get; set; }
    }
}