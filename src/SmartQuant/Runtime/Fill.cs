// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace SmartQuant
{
    public class Fill : DataObject
    {
        public override byte TypeId => DataObjectType.Fill;

        public Order Order { get; private set; }

        public Instrument Instrument { get; private set; }

        public byte CurrencyId { get; set; }

        public OrderSide Side { get; set; }

        public double Qty { get; set; }

        public double Price { get; set; }

        public string Text { get; set; }

        public double Commission { get; private set; }

        public double Value => Instrument.Factor != 0 ? Price * Qty * Instrument.Factor : Price * Qty;

        public double NetCashFlow => Side == OrderSide.Buy ? -Value : Value;

        public double CashFlow => NetCashFlow - Commission;

        public Fill()
        {
        }

        public Fill(DateTime dateTime, Order order, Instrument instrument, byte currencyId, OrderSide side, double qty, double price, string text = "")
        {
            DateTime = dateTime;
            Order = order;
            Instrument = instrument;
            CurrencyId = currencyId;
            Side = side;
            Qty = qty;
            Price = price;
            Text = text;
        }

        public Fill(ExecutionReport report)
        {
            DateTime = report.DateTime;
            Order = report.Order;
            Instrument = report.Instrument;
            CurrencyId = report.CurrencyId;
            Side = report.Side;
            Qty = report.LastQty;
            Price = report.Price;
            Text = report.Text;
        }

        public Fill(Fill fill)
        {
            DateTime = fill.DateTime;
            Order = fill.Order;
            Instrument = fill.Instrument;
            CurrencyId = fill.CurrencyId;
            Side = fill.Side;
            Qty = fill.Qty;
            Price = fill.Price;
            Commission = fill.Commission;
            Text = fill.Text;
        }

        public string GetSideAsString()
        {
            switch (Side)
            {
                case OrderSide.Buy:
                    return "Buy";
                case OrderSide.Sell:
                    return "Sell";
                default:
                    return "Undefined";
            }
        }

        public override string ToString()
        {
            return $"{DateTime} {GetSideAsString()} {Instrument.Symbol} {Qty} {Price} {Text}";
        }


        #region Extra
        public int OrderId { get; set; }
        public int InstrumentId { get; set; }
        #endregion
    }
}
