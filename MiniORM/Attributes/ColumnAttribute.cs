namespace MiniORM.Attributes
{
    using System;

    class ColumnAttribute : Attribute
    {
        public string Name { get; set; }
    }
}