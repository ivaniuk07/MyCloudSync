using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MyCloudSync.Infrastructure.Data;

public static class DbDate
{
    private const string Format = "yyyy-MM-dd'T'HH:mm:ss'Z'";

    public static object ToDb(DateTime? value)
    {
        if (value == null)
        {
            return DBNull.Value;
        }

        return value.Value.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture);
    }

    public static DateTime Parse(string value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
    }

    public static DateTime? Read(SqliteDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? null : Parse(reader.GetString(index));
    }
}
