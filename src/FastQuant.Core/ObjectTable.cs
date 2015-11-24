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
    }
}