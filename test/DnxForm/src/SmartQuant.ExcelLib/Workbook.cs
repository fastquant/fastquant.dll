// Decompiled with JetBrains decompiler
// Type: SmartQuant.ExcelLib.Workbook
// Assembly: SmartQuant.ExcelLib, Version=1.0.5820.33983, Culture=neutral, PublicKeyToken=null
// MVID: 4EF6D9C5-C859-4AA7-B7E8-07FB3B37A269
// Assembly location: C:\Program Files\SmartQuant Ltd\OpenQuant 2014\SmartQuant.ExcelLib.dll

using System;

namespace SmartQuant.ExcelLib
{
  public class Workbook
  {
   // private Interop.Excel.Workbook workbook;

    public WorksheetList Worksheets
    {
      get
            {
                throw new NotImplementedException();

              //  return new WorksheetList(this.workbook.Worksheets);
      }
    }

    //internal Workbook(Interop.Excel.Workbook workbook)
    //{
    //  this.workbook = workbook;
    //}
  }
}
