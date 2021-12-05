using System;

namespace GenshinLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    class EnumIgnoreAttribute : Attribute
    {
        public readonly string[] IgnoredNames;

        public EnumIgnoreAttribute(params string[] ignoredNames)
        {
            IgnoredNames = ignoredNames;
        }
    }
}
