namespace MyCloudSync.Infrastructure.Data;

public static class AppPaths
{
    public static string DataFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyCloudSync");

    public static string DatabaseFile => Path.Combine(DataFolder, "mycloudsync.db");

    public static string ClientSecretFile => Path.Combine(AppContext.BaseDirectory, "client_secret.json");
}
