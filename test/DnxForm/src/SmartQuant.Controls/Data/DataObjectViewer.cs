// Decompiled with JetBrains decompiler
// Type: SmartQuant.Controls.Data.DataObjectViewer
// Assembly: SmartQuant.Controls, Version=1.0.5820.33995, Culture=neutral, PublicKeyToken=null
// MVID: EFEF2D43-0E96-48AE-8F56-611B584714E6
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.Controls.dll

using SmartQuant;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartQuant.Controls.Data
{
  internal abstract class DataObjectViewer
  {
    protected DataSeries dataSeries;
    protected string priceFormat;
    protected ListViewItem lastItem;

    protected DataObjectViewer()
    {
      this.lastItem = (ListViewItem) null;
    }

    protected abstract ColumnHeader[] GetCustomColumnHeaders();

    protected abstract string[] GetCustomSubItems(int index);

    public abstract DataObjectEditor GetEditor();

    public static DataObjectViewer GetViewer(byte? dataObjectType)
    {
      byte? nullable = dataObjectType;
      if (!(nullable.HasValue ? new int?((int) nullable.GetValueOrDefault()) : new int?()).HasValue)
        return (DataObjectViewer) null;
      switch (dataObjectType.Value)
      {
        case 2:
        case 3:
        case 4:
          return (DataObjectViewer) new TickViewer(dataObjectType.Value);
        case 5:
          return (DataObjectViewer) new QuoteViewer();
        case 6:
          return (DataObjectViewer) new BarViewer();
        default:
          return (DataObjectViewer) new DefaultObjectViewer();
      }
    }

    public void SetDataSeries(DataSeries dataSeries)
    {
      this.dataSeries = dataSeries;
    }

    public void SetPriceFormat(string priceFormat)
    {
      this.priceFormat = priceFormat;
    }

    public ColumnHeader[] GetColumnHeaders()
    {
      List<ColumnHeader> list = new List<ColumnHeader>();
      list.Add(this.CreateColumnHeader("#", 0, HorizontalAlignment.Left));
      list.AddRange((IEnumerable<ColumnHeader>) this.GetCustomColumnHeaders());
      return list.ToArray();
    }

    public void ResetLastItem()
    {
      this.lastItem = (ListViewItem) null;
    }

    public ListViewItem GetListViewItem(int index)
    {
      if ((long) index == this.dataSeries.Count - 1L)
      {
        if (this.lastItem == null)
        {
          this.lastItem = new ListViewItem(index.ToString("n0"));
          this.lastItem.SubItems.AddRange(this.GetCustomSubItems(index));
        }
        return this.lastItem;
      }
      ListViewItem listViewItem = new ListViewItem(index.ToString("n0"));
      listViewItem.SubItems.AddRange(this.GetCustomSubItems(index));
      return listViewItem;
    }

    protected ColumnHeader CreateColumnHeader(string text, int width, HorizontalAlignment alignment)
    {
      return new ColumnHeader()
      {
        Text = text,
        Width = width,
        TextAlign = alignment
      };
    }
  }
}
