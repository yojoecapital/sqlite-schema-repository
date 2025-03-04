using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SqliteSchemaRepository.Schema;

public class ModelSchema<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> where T : class, new()
{
    private readonly string tableName;
    private readonly ModelProperty<T> key;
    private readonly ReadOnlyDictionary<string, (int ordinal, NullableModelProperty<T> property)> properties;
    private readonly string select;
    private readonly string insert;
    private readonly string update;

    public ModelSchema(
        string tableName,
        ModelProperty<T> key,
        IEnumerable<NullableModelProperty<T>> properties
    )
    {
        this.tableName = tableName;
        this.key = key;
        this.properties = properties
           .Select((prop, index) => new { prop, index })
           .ToDictionary(x => x.prop.name, x => (x.index + 1, x.prop)).AsReadOnly();
        var combined = (new ModelProperty<T>[] { key }).Concat(properties.Cast<ModelProperty<T>>()).ToArray();
        select = string.Join(", ", combined.Select(property => property.name));
        insert = string.Join(", ", combined.Select(property => $"@{property.name}"));
        update = string.Join(", ", properties.Select(property => $"{property.name} = @{property.name}"));
    }

    public string GetCreateIndexCommandText(IEnumerable<string> propertyNames, bool isUnique = false)
    {
        string indexName = $"idx_{tableName}_" + string.Join("_", propertyNames);
        string columns = string.Join(", ", propertyNames);
        string uniqueClause = isUnique ? "UNIQUE " : string.Empty;
        return $"CREATE {uniqueClause}INDEX IF NOT EXISTS {indexName} ON {tableName} ({columns});";
    }

    public string CreateTableCommandText
    {
        get
        {
            var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {tableName} (");
            builder.AppendLine();
            builder.Append($"    {key.name} {key.SqlType} PRIMARY KEY");
            builder.AppendLine(",");
            foreach (var (_, property) in properties.Values)
            {
                builder.Append($"    {property.name} {property.SqlType}");
                if (!property.isNullable) builder.Append(" NOT NULL");
                builder.AppendLine(",");
            }

            // Remove the last comma
            builder.Length -= 1 + Environment.NewLine.Length;
            builder.AppendLine();
            builder.AppendLine(");");
            return builder.ToString();
        }
    }

    public void SetValueOfKey(T model, SqliteDataReader reader)
    {
        key.setter(model, key.ReadValueFrom(reader, 0));
    }

    public void SetValueOfKey(T model, object value) => key.setter(model, value);

    public object GetValueOfKey(T model) => key.getter(model);

    public void SetValueOfProperty(T model, string nameOfProperty, SqliteDataReader reader)
    {
        var (ordinal, property) = properties[nameOfProperty];
        property.setter(model, property.ReadValueFrom(reader, ordinal));
    }

    public void SetValueOfProperty(T model, string nameOfProperty, object value) => properties[nameOfProperty].property.setter(model, value);

    public object GetValueOfProperty(T model, string nameOfProperty) => properties[nameOfProperty].property.getter(model);

    public T CreateModelFrom(SqliteDataReader reader)
    {
        var model = new T();
        key.setter(model, key.ReadValueFrom(reader, 0));
        int ordinal = 1;
        foreach (var pair in properties.Values)
        {
            pair.property.setter(model, pair.property.ReadValueFrom(reader, ordinal));
            ordinal++;
        }
        return model;
    }

    public void PopulateParemetersWith(SqliteParameterCollection parameters, T model)
    {
        parameters.AddWithValue(key.name, GetValueOfKey(model));
        int ordinal = 1;
        foreach (var pair in properties.Values)
        {
            var value = pair.property.getter(model);
            if (pair.property.isNullable && value == null) value = DBNull.Value;
            parameters.AddWithValue(pair.property.name, value);
            ordinal++;
        }
    }

    public IEnumerable<string> PropertyNames => properties.Keys;
    public string KeyName => key.name;

    public string GetSelectByCommandText(string whereClause)
    {
        return @$"SELECT {select}
FROM {tableName}
WHERE {whereClause};
";
    }

    private string GetSelectByCommandText(ModelProperty<T> modelProperty) => GetSelectByCommandText($"{modelProperty.name} = @{modelProperty.name}");

    public string SelectByKeyCommandText => GetSelectByCommandText(key);

    public string SelectAllCommandText => @$"SELECT {select}
FROM {tableName};";

    public string GetDeleteByCommandText(string whereClause)
    {
        return @$"DELETE FROM {tableName}
WHERE {whereClause};
";
    }

    private string GetDeleteByCommandText(ModelProperty<T> modelProperty) => GetDeleteByCommandText($"{modelProperty.name} = @{modelProperty.name}");

    public string DeleteByKeyCommandText => GetDeleteByCommandText(key);

    public string DeleteAllCommandText => $"DELETE FROM {tableName};";

    public string InsertCommandText => @$"INSERT INTO {tableName} ({select})
VALUES ({insert});
";

    public string GetUpdateByCommandText(string whereClause)
    {
        return @$"UPDATE {tableName}
SET {update}
WHERE {whereClause};
";
    }

    private string GetUpdateByCommandText(ModelProperty<T> modelProperty) => GetUpdateByCommandText($"{modelProperty.name} = @{modelProperty.name}");

    public string UpdateByKeyCommandText => GetUpdateByCommandText(key);

    public string UpdateAllCommandText => @$"UPDATE {tableName}
SET {update};";

    public string UpsertCommandText => @$"
INSERT INTO {tableName} ({select})
VALUES ({insert})
ON CONFLICT({key.name}) DO UPDATE SET
{update};
";
}