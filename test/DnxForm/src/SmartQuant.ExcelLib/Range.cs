using System;

namespace SmartQuant.ExcelLib
{
  public class Range
  {
   // private Interop.Excel.Range range;

    public bool Italic
    {
      set
      {
                throw new NotImplementedException();
            }
        }

    public bool Bold
    {
      set
      {
                throw new NotImplementedException();
            }
        }

    public bool Underline
    {
      set
      {
                throw new NotImplementedException();
            }
        }

    public object Value
    {
      get
      {
                throw new NotImplementedException();
            }
            set
      {
   //     this.range.Value2 = value;
      }
    }

    //internal Range(Interop.Excel.Range range)
    //{
    //  this.range = range;
    //}

    public void Select()
    {
      // ISSUE: reference to a compiler-generated method
    //  this.range.Select();
    }
  }
}
