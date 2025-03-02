using System;
using System.Diagnostics.CodeAnalysis;

namespace SqliteSchemaRepository.Schema;

public class NullableModelProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : ModelProperty<T> where T : class
{
    public readonly bool isNullable;

    public NullableModelProperty(
        string name, PropertyType propertyType, Func<T, object> getter, Action<T, object> setter, bool isNullable
    ) : base(name, propertyType, getter, setter)
    {
        this.isNullable = isNullable;
    }

    public NullableModelProperty(
        string name, PropertyType propertyType, Func<T, object> getter, Action<T, object> setter
    ) : base(name, propertyType, getter, setter)
    {
        isNullable = true;
    }

    public NullableModelProperty(string name, PropertyType propertyType) : base(name, propertyType)
    {
        isNullable = true;
    }

    public NullableModelProperty(string name, PropertyType propertyType, bool isNullable) : base(name, propertyType)
    {
        this.isNullable = isNullable;
    }
}