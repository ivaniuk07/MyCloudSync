using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class FileStateRepository : IFileStateRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public FileStateRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyList<FileState> GetByPair(int syncPairId)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, sync_pair_id, relative_path, drive_file_id, is_folder, size_bytes, local_hash, drive_md5, " +
            "local_modified_at, drive_modified_at, last_synced_at, status " +
            "FROM FileState WHERE sync_pair_id = $pair ORDER BY relative_path";
        command.Parameters.AddWithValue("$pair", syncPairId);

        var states = new List<FileState>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            states.Add(new FileState
            {
                Id = reader.GetInt32(0),
                SyncPairId = reader.GetInt32(1),
                RelativePath = reader.GetString(2),
                DriveFileId = reader.IsDBNull(3) ? null : reader.GetString(3),
                IsFolder = reader.GetBoolean(4),
                SizeBytes = reader.IsDBNull(5) ? null : reader.GetInt64(5),
                LocalHash = reader.IsDBNull(6) ? null : reader.GetString(6),
                DriveMd5 = reader.IsDBNull(7) ? null : reader.GetString(7),
                LocalModifiedAt = DbDate.Read(reader, 8),
                DriveModifiedAt = DbDate.Read(reader, 9),
                LastSyncedAt = DbDate.Read(reader, 10),
                Status = Enum.Parse<FileStatus>(reader.GetString(11))
            });
        }

        return states;
    }

    public void Save(FileState state)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();

        if (state.Id == 0)
        {
            command.CommandText =
                "INSERT INTO FileState (sync_pair_id, relative_path, drive_file_id, is_folder, size_bytes, local_hash, drive_md5, " +
                "local_modified_at, drive_modified_at, last_synced_at, status) " +
                "VALUES ($pair, $path, $driveId, $folder, $size, $localHash, $driveMd5, $localModified, $driveModified, $synced, $status); " +
                "SELECT last_insert_rowid();";
        }
        else
        {
            command.CommandText =
                "UPDATE FileState SET sync_pair_id = $pair, relative_path = $path, drive_file_id = $driveId, is_folder = $folder, " +
                "size_bytes = $size, local_hash = $localHash, drive_md5 = $driveMd5, local_modified_at = $localModified, " +
                "drive_modified_at = $driveModified, last_synced_at = $synced, status = $status WHERE id = $id";
            command.Parameters.AddWithValue("$id", state.Id);
        }

        command.Parameters.AddWithValue("$pair", state.SyncPairId);
        command.Parameters.AddWithValue("$path", state.RelativePath);
        command.Parameters.AddWithValue("$driveId", (object?)state.DriveFileId ?? DBNull.Value);
        command.Parameters.AddWithValue("$folder", state.IsFolder);
        command.Parameters.AddWithValue("$size", (object?)state.SizeBytes ?? DBNull.Value);
        command.Parameters.AddWithValue("$localHash", (object?)state.LocalHash ?? DBNull.Value);
        command.Parameters.AddWithValue("$driveMd5", (object?)state.DriveMd5 ?? DBNull.Value);
        command.Parameters.AddWithValue("$localModified", DbDate.ToDb(state.LocalModifiedAt));
        command.Parameters.AddWithValue("$driveModified", DbDate.ToDb(state.DriveModifiedAt));
        command.Parameters.AddWithValue("$synced", DbDate.ToDb(state.LastSyncedAt));
        command.Parameters.AddWithValue("$status", state.Status.ToString());

        if (state.Id == 0)
        {
            state.Id = Convert.ToInt32(command.ExecuteScalar());
        }
        else
        {
            command.ExecuteNonQuery();
        }
    }

    public void Delete(int fileStateId)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM FileState WHERE id = $id";
        command.Parameters.AddWithValue("$id", fileStateId);
        command.ExecuteNonQuery();
    }

    public void DeleteByPair(int syncPairId)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM FileState WHERE sync_pair_id = $pair";
        command.Parameters.AddWithValue("$pair", syncPairId);
        command.ExecuteNonQuery();
    }
}
