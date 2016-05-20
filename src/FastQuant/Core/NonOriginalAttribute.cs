using System;

namespace FastQuant
{
    [AttributeUsage(AttributeTargets.All)]
    internal class NotOriginalAttribute: Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All)]
    internal class UglyNamingAttribute : Attribute
    {
    }
}
