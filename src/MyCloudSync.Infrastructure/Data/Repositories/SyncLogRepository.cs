using System.Globalization;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class SyncLogRepository : ISyncLogRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SyncLogRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Add(SyncLogEntry entry)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO SyncLog (sync_pair_id, file_state_id, action, direction, result, message) " +
            "VALUES ($pair, $file, $action, $direction, $result, $message)";
        command.Parameters.AddWithValue("$pair", entry.SyncPairId);
        command.Parameters.AddWithValue("$file", (object?)entry.FileStateId ?? DBNull.Value);
        command.Parameters.AddWithValue("$action", entry.Action);
        command.Parameters.AddWithValue("$direction", (object?)entry.Direction ?? DBNull.Value);
        command.Parameters.AddWithValue("$result", entry.Result.ToString());
        command.Parameters.AddWithValue("$message", (object?)entry.Message ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<SyncLogEntry> GetRecent(int count)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, sync_pair_id, file_state_id, action, direction, result, message, created_at " +
            "FROM SyncLog ORDER BY created_at DESC, id DESC LIMIT $count";
        command.Parameters.AddWithValue("$count", count);

        var entries = new List<SyncLogEntry>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            entries.Add(new SyncLogEntry
            {
                Id = reader.GetInt32(0),
                SyncPairId = reader.GetInt32(1),
                FileStateId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Action = reader.GetString(3),
                Direction = reader.IsDBNull(4) ? null : reader.GetString(4),
                Result = Enum.Parse<LogResult>(reader.GetString(5)),
                Message = reader.IsDBNull(6) ? null : reader.GetString(6),
                CreatedAt = DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
            });
        }

        return entries;
    }

    public void Clear()
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM SyncLog";
        command.ExecuteNonQuery();
    }
}
