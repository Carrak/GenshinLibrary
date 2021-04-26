using System;

namespace GenshinLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class GifExampleAttribute : Attribute
    {
        public string Link { get; }

        public GifExampleAttribute(string link)
        {
            Link = link;
        }
    }
}
