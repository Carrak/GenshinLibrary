using System;
using System.Linq;

namespace GenshinLibrary.Attributes
{
    public static class AttributeExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            if (type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() is TAttribute att)
                return att;
            return default;
        }

        public static TAttribute GetAttribute<TAttribute>(this object obj) where TAttribute : Attribute
        {
            if (obj.GetType().GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() is TAttribute att)
                return att;
            return default;
        }
    }
}
