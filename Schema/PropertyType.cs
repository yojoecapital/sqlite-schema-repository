using System;
using Microsoft.Data.Sqlite;

namespace SqliteSchemaRepository.Schema;

public abstract class PropertyType(string sqlType)
{
    public readonly string sqlType = sqlType;

    public abstract object ReadValueFrom(SqliteDataReader reader, int ordinal);

    public static PropertyType Text => new PropertyType<string>("TEXT", ReadString);
    public static PropertyType String => new PropertyType<string>("VARCHAR", ReadString);
    public static PropertyType Integer => new PropertyType<int>("INTEGER", ReadInteger);
    public static PropertyType Long => new PropertyType<long>("INTEGER", ReadLong);
    public static PropertyType Double => new PropertyType<double>("REAL", ReadDouble);
    public static PropertyType Float => new PropertyType<float>("REAL", ReadFloat);
    public static PropertyType Boolean => new PropertyType<bool>("INTEGER", ReadBoolean);

    private static string ReadString(SqliteDataReader reader, int ordinal) => reader.GetString(ordinal);
    private static int ReadInteger(SqliteDataReader reader, int ordinal) => reader.GetInt32(ordinal);
    private static long ReadLong(SqliteDataReader reader, int ordinal) => reader.GetInt64(ordinal);
    private static double ReadDouble(SqliteDataReader reader, int ordinal) => reader.GetDouble(ordinal);
    private static float ReadFloat(SqliteDataReader reader, int ordinal) => reader.GetFloat(ordinal);
    private static bool ReadBoolean(SqliteDataReader reader, int ordinal) => reader.GetBoolean(ordinal);

}

public class PropertyType<T>(string sqlType, Func<SqliteDataReader, int, T> read) : PropertyType(sqlType)
{
    public readonly Func<SqliteDataReader, int, T> read = read;
    public override object ReadValueFrom(SqliteDataReader reader, int ordinal) => read(reader, ordinal);
}