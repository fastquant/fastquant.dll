// Copyright (c) FastQuant Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;

namespace SmartQuant
{
    public class Transaction
    {
        public List<Fill> Fills { get; } = new List<Fill>();

        public Instrument Instrument => Fills[0].Instrument;

        public Order Order => Fills[0].Order;

        public OrderSide Side => Fills[0].Side;

        public string Text => Fills[0].Order.Text;

        public double Price { get; private set; }

        public double Qty { get; private set; }

        public double Commission { get; private set; }

        public bool IsDone { get; internal set; }

        public double Amount => Side == OrderSide.Buy ? Qty : -Qty;

        public virtual double Value => Instrument.Factor != 0 ? Qty * Price * Instrument.Factor : Qty * Price;

        public virtual double NetCashFlow => Instrument.Factor != 0 ? Amount * Price * Instrument.Factor : Amount * Price;

        public virtual double CashFlow => NetCashFlow - Commission;

        public Transaction()
        {
        }

        public Transaction(Fill fill)
        {
            Add(fill);
        }

        public void Add(Fill fill)
        {
            Fills.Add(fill);
            Qty += fill.Qty;
            Commission += fill.Commission;
            Price = Fills.Sum(f => f.Qty*f.Price)/Qty;
        }

        public override string ToString() => $"{Side} {Qty} {Instrument.Symbol} {Price}";
    }

    public delegate void TransactionEventHandler(object sender, OnTransaction transaction);
}

