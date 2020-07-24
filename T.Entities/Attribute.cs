using System;

namespace T.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AliasAttribute : Attribute
    {
        public AliasAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}