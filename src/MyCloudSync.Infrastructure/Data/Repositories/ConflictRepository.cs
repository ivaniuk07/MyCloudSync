using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class ConflictRepository : IConflictRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ConflictRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Add(Conflict conflict)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO Conflict (file_state_id, conflict_copy_name, local_modified_at, drive_modified_at) " +
            "VALUES ($file, $copy, $localModified, $driveModified); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$file", conflict.FileStateId);
        command.Parameters.AddWithValue("$copy", conflict.ConflictCopyName);
        command.Parameters.AddWithValue("$localModified", DbDate.ToDb(conflict.LocalModifiedAt));
        command.Parameters.AddWithValue("$driveModified", DbDate.ToDb(conflict.DriveModifiedAt));

        conflict.Id = Convert.ToInt32(command.ExecuteScalar());
    }

    public IReadOnlyList<Conflict> GetOpen()
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, file_state_id, conflict_copy_name, local_modified_at, drive_modified_at, detected_at, is_resolved " +
            "FROM Conflict WHERE is_resolved = 0 ORDER BY detected_at DESC, id DESC";

        var conflicts = new List<Conflict>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            conflicts.Add(new Conflict
            {
                Id = reader.GetInt32(0),
                FileStateId = reader.GetInt32(1),
                ConflictCopyName = reader.GetString(2),
                LocalModifiedAt = DbDate.Parse(reader.GetString(3)),
                DriveModifiedAt = DbDate.Parse(reader.GetString(4)),
                DetectedAt = DbDate.Parse(reader.GetString(5)),
                IsResolved = reader.GetBoolean(6)
            });
        }

        return conflicts;
    }
}
