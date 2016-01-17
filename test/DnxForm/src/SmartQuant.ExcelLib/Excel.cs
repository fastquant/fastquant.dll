using System;
using System.Runtime.InteropServices;

namespace SmartQuant.ExcelLib
{
  public class Excel
  {
    //private Application excel;
    private WorkbookList workbooks;

    public bool Visible
    {
      get
      {
                throw new NotImplementedException();
            }
            set
      {
                throw new NotImplementedException();
            }
        }

    public WorkbookList Workbooks
    {
      get
      {
        return this.workbooks;
      }
    }

    public Excel()
    {
            throw new NotImplementedException();

      //      this.excel = (Application) Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("00024500-0000-0000-C000-000000000046")));
      //this.workbooks = new WorkbookList(this.excel.Workbooks);
    }
  }
}
