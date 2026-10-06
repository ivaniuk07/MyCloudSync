using System.Reflection;

namespace MyCloudSync.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public DatabaseInitializer(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Initialize()
    {
        using var connection = _connectionFactory.Open();

        using var walCommand = connection.CreateCommand();
        walCommand.CommandText = "PRAGMA journal_mode = WAL;";
        walCommand.ExecuteNonQuery();

        using var schemaCommand = connection.CreateCommand();
        schemaCommand.CommandText = ReadSchema();
        schemaCommand.ExecuteNonQuery();
    }

    private static string ReadSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().First(name => name.EndsWith("schema.sql"));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
