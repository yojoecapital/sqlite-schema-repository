using Microsoft.Data.Sqlite;
using SqliteSchemaRepository.Schema;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SqliteSchemaRepository;

public class Repository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(ModelSchema<T> modelSchema, SqliteConnection connection) where T : class, new()
{
    protected ModelSchema<T> ModelSchema { get; } = modelSchema;
    protected SqliteConnection Connection { get; } = connection;

    public void CreateTable()
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.CreateTableCommandText;
        command.ExecuteNonQuery();
    }

    public T SelectByKey(object valueOfKey)
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.SelectByKeyCommandText;
        command.Parameters.AddWithValue(ModelSchema.KeyName, valueOfKey);
        var reader = command.ExecuteReader();
        if (!reader.Read()) return default;
        return ModelSchema.CreateModelFrom(reader);
    }

    public IEnumerable<T> SelectAll()
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.SelectAllCommandText;
        var reader = command.ExecuteReader();
        while (reader.Read()) yield return ModelSchema.CreateModelFrom(reader);
    }

    public int DeleteByKey(object valueOfKey)
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.DeleteByKeyCommandText;
        command.Parameters.AddWithValue(ModelSchema.KeyName, valueOfKey);
        return command.ExecuteNonQuery();
    }

    public int DeleteAll()
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.DeleteAllCommandText;
        return command.ExecuteNonQuery();
    }

    public int Insert(T model)
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.InsertCommandText;
        ModelSchema.PopulateParemetersWith(command.Parameters, model);
        return command.ExecuteNonQuery();
    }

    public int Update(T model)
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.UpdateByKeyCommandText;
        ModelSchema.PopulateParemetersWith(command.Parameters, model);
        return command.ExecuteNonQuery();
    }

    public int Upsert(T model)
    {
        var command = Connection.CreateCommand();
        command.CommandText = ModelSchema.UpsertCommandText;
        ModelSchema.PopulateParemetersWith(command.Parameters, model);
        return command.ExecuteNonQuery();
    }
}