namespace SmartQuant
{
    public class ObjectTable
    {
        private IdArray<object> fields = new IdArray<object>(16);

        public int Size =>this.fields.Size;

        public object this[int index]
        {
            get
            {
                return this.fields[index];
            }
            set
            {
                this.fields[index] = value;
            }
        }

        public ObjectTable()
        {
        }

        public ObjectTable(ObjectTable table)
        {
            table.CopyTo(this);
        }

        public void Clear()
        {
            this.fields.Clear();
        }

        public void Remove(int id)
        {
            this.fields.Remove(id);
        }

        public double GetDouble(int index)
        {
            return (double)this.fields[index];
        }

        public int GetInt(int index)
        {
            return (int)this.fields[index];
        }

        public string GetString(int index)
        {
            return (string)this.fields[index];
        }

        public void CopyTo(ObjectTable table)
        {
            this.fields.CopyTo(table.fields);
        }
    }
}