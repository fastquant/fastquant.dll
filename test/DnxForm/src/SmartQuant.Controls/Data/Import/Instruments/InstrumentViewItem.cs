// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.Import.Instruments.InstrumentViewItem
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data.Import.Instruments
{
  internal class InstrumentViewItem : ListViewItem
  {
    public Instrument Instrument { get; private set; }

    public InstrumentViewItem(Instrument instrument)
      : base(new string[5])
    {
      this.Instrument = instrument;
      this.SubItems[0].Text = instrument.Symbol;
      this.SubItems[1].Text = instrument.Type.ToString();
      this.SubItems[2].Text = instrument.Exchange;
      this.SubItems[3].Text = CurrencyId.GetName(instrument.CurrencyId);
      this.SubItems[4].Text = instrument.Maturity == DateTime.MinValue ? string.Empty : instrument.Maturity.ToShortDateString();
    }
  }
}
