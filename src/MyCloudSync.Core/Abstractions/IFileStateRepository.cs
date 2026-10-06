using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface IFileStateRepository
{
    IReadOnlyList<FileState> GetByPair(int syncPairId);
    void Save(FileState state);
    void Delete(int fileStateId);
}
