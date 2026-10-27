using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class SyncPairRepository : ISyncPairRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SyncPairRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public SyncPair? GetActive()
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, account_id, local_path, drive_folder_id, drive_folder_name, sync_mode, is_active, page_token, last_sync_at " +
            "FROM SyncPair WHERE is_active = 1 ORDER BY id LIMIT 1";

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new SyncPair
        {
            Id = reader.GetInt32(0),
            AccountId = reader.GetInt32(1),
            LocalPath = reader.GetString(2),
            DriveFolderId = reader.GetString(3),
            DriveFolderName = reader.IsDBNull(4) ? null : reader.GetString(4),
            Mode = Enum.Parse<SyncMode>(reader.GetString(5)),
            IsActive = reader.GetBoolean(6),
            PageToken = reader.IsDBNull(7) ? null : reader.GetString(7),
            LastSyncAt = DbDate.Read(reader, 8)
        };
    }

    public void Save(SyncPair pair)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();

        if (pair.Id == 0)
        {
            command.CommandText =
                "INSERT INTO SyncPair (account_id, local_path, drive_folder_id, drive_folder_name, sync_mode, is_active, page_token, last_sync_at) " +
                "VALUES ($account, $local, $folderId, $folderName, $mode, $active, $token, $lastSync); SELECT last_insert_rowid();";
        }
        else
        {
            command.CommandText =
                "UPDATE SyncPair SET account_id = $account, local_path = $local, drive_folder_id = $folderId, " +
                "drive_folder_name = $folderName, sync_mode = $mode, is_active = $active, page_token = $token, " +
                "last_sync_at = $lastSync WHERE id = $id";
            command.Parameters.AddWithValue("$id", pair.Id);
        }

        command.Parameters.AddWithValue("$account", pair.AccountId);
        command.Parameters.AddWithValue("$local", pair.LocalPath);
        command.Parameters.AddWithValue("$folderId", pair.DriveFolderId);
        command.Parameters.AddWithValue("$folderName", (object?)pair.DriveFolderName ?? DBNull.Value);
        command.Parameters.AddWithValue("$mode", pair.Mode.ToString());
        command.Parameters.AddWithValue("$active", pair.IsActive);
        command.Parameters.AddWithValue("$token", (object?)pair.PageToken ?? DBNull.Value);
        command.Parameters.AddWithValue("$lastSync", DbDate.ToDb(pair.LastSyncAt));

        if (pair.Id == 0)
        {
            pair.Id = Convert.ToInt32(command.ExecuteScalar());
        }
        else
        {
            command.ExecuteNonQuery();
        }
    }

    public IReadOnlyList<ExcludeRule> GetExcludeRules(int syncPairId)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, sync_pair_id, pattern, is_enabled FROM ExcludeRule WHERE sync_pair_id = $pair ORDER BY id";
        command.Parameters.AddWithValue("$pair", syncPairId);

        var rules = new List<ExcludeRule>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rules.Add(new ExcludeRule
            {
                Id = reader.GetInt32(0),
                SyncPairId = reader.GetInt32(1),
                Pattern = reader.GetString(2),
                IsEnabled = reader.GetBoolean(3)
            });
        }

        return rules;
    }

    public void SaveExcludeRules(int syncPairId, IEnumerable<string> patterns)
    {
        using var connection = _connectionFactory.Open();
        using var transaction = connection.BeginTransaction();

        using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.CommandText = "DELETE FROM ExcludeRule WHERE sync_pair_id = $pair";
            deleteCommand.Parameters.AddWithValue("$pair", syncPairId);
            deleteCommand.ExecuteNonQuery();
        }

        foreach (var pattern in patterns.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO ExcludeRule (sync_pair_id, pattern) VALUES ($pair, $pattern)";
            insertCommand.Parameters.AddWithValue("$pair", syncPairId);
            insertCommand.Parameters.AddWithValue("$pattern", pattern);
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
