using System;
using System.ComponentModel.DataAnnotations;

namespace T.Common
{

    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string value)
        {
            Value = value;
        }
        public string Value { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute() { }
        public ColumnAttribute(string name) { Name = name; }
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IdentityAttribute : Attribute
    {

    }
    
    public class DisplayDate : DisplayFormatAttribute
    {
        public DisplayDate()
        {
            ApplyFormatInEditMode = true;
            DataFormatString = Constants.DateFormatString;
        }
    }

    public class DisplayMoney : DisplayFormatAttribute
    {
        public DisplayMoney()
        {
            ApplyFormatInEditMode = true;
            DataFormatString = Constants.DecimalFormat;
        }
    }
}