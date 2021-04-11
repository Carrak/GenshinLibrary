﻿using System;

namespace GenshinLibrary.Attributes
{
    class ExampleAttribute : Attribute
    {
        public string Value { get; }

        public ExampleAttribute(string value)
        {
            Value = value;
        }
    }
}
