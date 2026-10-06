using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface ISettingsRepository
{
    AppSettings Load();
    void Save(AppSettings settings);
}
