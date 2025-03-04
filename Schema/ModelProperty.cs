using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;

namespace SqliteSchemaRepository.Schema;

public class ModelProperty<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> where T : class
{
    public readonly string name;
    private readonly PropertyType propertyType;
    public readonly Func<T, object> getter;
    public readonly Action<T, object> setter;

    public string SqlType => propertyType.sqlType;

    public virtual object ReadValueFrom(SqliteDataReader reader, int ordinal) => propertyType.ReadValueFrom(reader, ordinal);

    public ModelProperty(
        string name, PropertyType propertyType,
        Func<T, object> getter, Action<T, object> setter
    )
    {
        this.name = name;
        this.propertyType = propertyType;
        this.getter = getter;
        this.setter = setter;
    }

    public ModelProperty(string name, PropertyType propertyType)
    {
        this.name = name;
        this.propertyType = propertyType;
        getter = GetGetter(name);
        setter = GetSetter(name);
    }

    public static Func<T, object> GetGetter(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName) ?? throw new InvalidOperationException($"Property '{propertyName}' not found.");
        return propertyInfo.GetValue;
    }

    public static Action<T, object> GetSetter(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName) ?? throw new InvalidOperationException($"Property '{propertyName}' not found.");
        return propertyInfo.SetValue;
    }
}