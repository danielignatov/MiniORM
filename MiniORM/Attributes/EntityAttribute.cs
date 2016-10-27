namespace MiniORM.Attributes
{
    using System;

    class EntityAttribute : Attribute
    {
        public string TableName { get; set; }
    }
}