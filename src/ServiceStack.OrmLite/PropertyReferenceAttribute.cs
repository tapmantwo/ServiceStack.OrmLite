namespace ServiceStack.OrmLite
{
    using System;

    using ServiceStack.DataAnnotations;

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyReferenceAttribute : AttributeBase
    {
        public Type ReferencedType { get; set; }

        public string PropertyName { get; set; }

        public PropertyReferenceAttribute(Type referencedType, string propertyName)
        {
            this.ReferencedType = referencedType;
            this.PropertyName = propertyName;
        }
    }
}