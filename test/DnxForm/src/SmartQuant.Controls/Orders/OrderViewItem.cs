using SmartQuant;
using System.Drawing;
using System.Windows.Forms;

namespace SmartQuant.Controls.Orders
{
  class OrderViewItem : DataGridViewRow
  {
    private int columnCount = 11;
    private Color color = SystemColors.Window;
    private Order order;

    public Order Order
    {
      get
      {
        return this.order;
      }
    }

    public Color Color
    {
      get
      {
        return this.color;
      }
    }

    public object this[int index]
    {
      get
      {
        object obj = (object) null;
        switch (index)
        {
          case 0:
            obj = (object) this.order.DateTime;
            break;
          case 1:
            obj = (object) this.order.ProviderId;
            break;
          case 2:
            obj = (object) this.order.Instrument.Symbol;
            break;
          case 3:
            obj = (object) this.order.Side;
            break;
          case 4:
            obj = (object) this.order.Type;
            break;
          case 5:
            obj = (object) this.order.Qty;
            break;
          case 6:
            obj = (object) this.order.AvgPx.ToString(this.order.Instrument.PriceFormat);
            break;
          case 7:
            obj = (object) this.order.Price.ToString(this.order.Instrument.PriceFormat);
            break;
          case 8:
            obj = (object) this.order.StopPx.ToString(this.order.Instrument.PriceFormat);
            break;
          case 9:
            obj = (object) this.order.Status;
            break;
          case 10:
            obj = (object) this.order.Text;
            break;
        }
        return obj;
      }
    }

    public OrderViewItem(Order order)
    {
      this.order = order;
      this.Update();
    }

    public void Update()
    {
      switch (this.order.Status)
      {
        case OrderStatus.New:
          this.color = Color.FromArgb((int) byte.MaxValue, (int) byte.MaxValue, 230);
          break;
        case OrderStatus.PartiallyFilled:
          this.color = Color.SkyBlue;
          break;
        case OrderStatus.Filled:
          this.color = Color.FromArgb(220, (int) byte.MaxValue, 220);
          break;
        case OrderStatus.Cancelled:
          this.color = Color.FromArgb((int) byte.MaxValue, 230, 230);
          break;
      }
    }

    protected override DataGridViewCellCollection CreateCellsInstance()
    {
      DataGridViewCellCollection viewCellCollection = new DataGridViewCellCollection((DataGridViewRow) this);
      for (int index = 0; index < this.columnCount; ++index)
        viewCellCollection.Add((DataGridViewCell) new DataGridViewTextBoxCell());
      return viewCellCollection;
    }
  }
}
