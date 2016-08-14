using System.Runtime.InteropServices;

namespace System
{
    // FIXME: will be removed.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    [ComVisible(true)]
    public sealed class SerializableAttribute : Attribute
    { }

    // FIXME: will be removed.
    [Serializable]
    [ComVisible(true)]
    public class ApplicationException : Exception
    {
        public ApplicationException(string message)
        { }
    }
}

namespace System.Drawing
{
    public struct Color
    {
        public static Color Red => FromArgb(0);

        public static Color Green => FromArgb(0);

        public static Color Blue => FromArgb(0);

        public static Color Yellow => FromArgb(0);

        public int ToArgb()
        {
            return 0;
        }

        public static Color FromArgb(int argb)
        {
            return new Color();
        }
    }
}

namespace System.ComponentModel
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class PasswordPropertyTextAttribute : Attribute
    {
        public PasswordPropertyTextAttribute()
        { }
        public PasswordPropertyTextAttribute(bool password)
        { }
    }
}