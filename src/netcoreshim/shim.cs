namespace System.Drawing
{
    public struct Color
    {
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
    public sealed class BrowsableAttribute : Attribute
    {
        public BrowsableAttribute(bool browsable)
        {
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class PasswordPropertyTextAttribute : Attribute
    {
        public PasswordPropertyTextAttribute()
        { }
        public PasswordPropertyTextAttribute(bool password)
        { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ReadOnlyAttribute : Attribute
    {
        public ReadOnlyAttribute(bool isReadOnly)
        { }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute()
        {

        }
        public DescriptionAttribute(string description)
        {

        }
    }
    [AttributeUsage(AttributeTargets.All)]
    public class CategoryAttribute : Attribute
    {
        public CategoryAttribute()
        {

        }
        public CategoryAttribute(string category)
        {

        }
    }
}