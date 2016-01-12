namespace SmartQuant
{
    public class ObjectTable
    {
        public IdArray<object> Fields { get; set; } = new IdArray<object>(16);

        public int Size => Fields.Size;

        public object this[int index]
        {
            get
            {
                return Fields[index];
            }
            set
            {
                Fields[index] = value;
            }
        }

        public ObjectTable()
        {
        }

        public ObjectTable(ObjectTable table)
        {
            table.CopyTo(this);
        }

        public void Clear() => Fields.Clear();
  
        public void Remove(int id)=> Fields.Remove(id);

        public double GetDouble(int index) => (double)this[index];

        public int GetInt(int index) => (int)this[index];

        public string GetString(int index) => (string)this[index];

        public void CopyTo(ObjectTable table) => Fields.CopyTo(table.Fields);
    }
}