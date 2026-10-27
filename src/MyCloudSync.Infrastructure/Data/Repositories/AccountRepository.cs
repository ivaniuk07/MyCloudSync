using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public AccountRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Account? GetCurrent()
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT id, email, display_name, access_token, refresh_token, token_expires_at, created_at " +
            "FROM Account ORDER BY id LIMIT 1";

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Account
        {
            Id = reader.GetInt32(0),
            Email = reader.GetString(1),
            DisplayName = reader.IsDBNull(2) ? null : reader.GetString(2),
            AccessToken = reader.GetString(3),
            RefreshToken = reader.GetString(4),
            TokenExpiresAt = DbDate.Read(reader, 5),
            CreatedAt = DbDate.Parse(reader.GetString(6))
        };
    }

    public void Save(Account account)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();

        if (account.Id == 0)
        {
            command.CommandText =
                "INSERT INTO Account (email, display_name, access_token, refresh_token, token_expires_at) " +
                "VALUES ($email, $name, $access, $refresh, $expires); SELECT last_insert_rowid();";
        }
        else
        {
            command.CommandText =
                "UPDATE Account SET email = $email, display_name = $name, access_token = $access, " +
                "refresh_token = $refresh, token_expires_at = $expires WHERE id = $id";
            command.Parameters.AddWithValue("$id", account.Id);
        }

        command.Parameters.AddWithValue("$email", account.Email);
        command.Parameters.AddWithValue("$name", (object?)account.DisplayName ?? DBNull.Value);
        command.Parameters.AddWithValue("$access", account.AccessToken);
        command.Parameters.AddWithValue("$refresh", account.RefreshToken);
        command.Parameters.AddWithValue("$expires", DbDate.ToDb(account.TokenExpiresAt));

        if (account.Id == 0)
        {
            account.Id = Convert.ToInt32(command.ExecuteScalar());
        }
        else
        {
            command.ExecuteNonQuery();
        }
    }

    public void Delete(int accountId)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Account WHERE id = $id";
        command.Parameters.AddWithValue("$id", accountId);
        command.ExecuteNonQuery();
    }
}
