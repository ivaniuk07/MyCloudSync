using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.Data.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SettingsRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public AppSettings Load()
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT check_interval_sec, autostart, start_minimized, show_notifications, language " +
            "FROM AppSettings WHERE id = 1";

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return new AppSettings();
        }

        return new AppSettings
        {
            CheckIntervalSec = reader.GetInt32(0),
            Autostart = reader.GetBoolean(1),
            StartMinimized = reader.GetBoolean(2),
            ShowNotifications = reader.GetBoolean(3),
            Language = reader.IsDBNull(4) ? "uk" : reader.GetString(4)
        };
    }

    public void Save(AppSettings settings)
    {
        using var connection = _connectionFactory.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "UPDATE AppSettings SET check_interval_sec = $interval, autostart = $autostart, " +
            "start_minimized = $minimized, show_notifications = $notifications, language = $language " +
            "WHERE id = 1";
        command.Parameters.AddWithValue("$interval", settings.CheckIntervalSec);
        command.Parameters.AddWithValue("$autostart", settings.Autostart);
        command.Parameters.AddWithValue("$minimized", settings.StartMinimized);
        command.Parameters.AddWithValue("$notifications", settings.ShowNotifications);
        command.Parameters.AddWithValue("$language", settings.Language);
        command.ExecuteNonQuery();
    }
}
