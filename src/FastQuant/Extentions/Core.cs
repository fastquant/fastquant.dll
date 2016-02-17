
namespace FastQuant
{
    public static class IdArrayExtensions
    {
        public static T GetOrCreate<T>(this IdArray<T> array, int id, int size = 1024) where T : class, new()
        {
            var o = array[id];
            if (o == null)
                array[id] = o = new T();
            return o;
        }
    }
}
