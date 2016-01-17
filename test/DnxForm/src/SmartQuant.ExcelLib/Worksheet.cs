using System;
using System.Reflection;

namespace SmartQuant.ExcelLib
{
    public class Worksheet
    {
        //   private Interop.Excel.Worksheet worksheet;

        public string Name
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

        //internal Worksheet(Interop.Excel.Worksheet worksheet)
        //{
        //  this.worksheet = worksheet;
        //}

        public Range GetRange(int row, int column)
        {
            throw new NotImplementedException();

        }

        public void Activate()
        {
            throw new NotImplementedException();

        }

        public void InsertPicture(string filename)
        {
            throw new NotImplementedException();

        }
    }
}
