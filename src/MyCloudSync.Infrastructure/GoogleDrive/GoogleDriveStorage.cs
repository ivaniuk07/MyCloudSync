using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Infrastructure.GoogleDrive;

public class GoogleDriveStorage : ICloudStorage
{
    public Task<IReadOnlyList<CloudFile>> ListFilesAsync(string folderId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<CloudFolder>> ListFoldersAsync(string parentId)
    {
        throw new NotImplementedException();
    }

    public Task<CloudFolder> CreateFolderAsync(string parentId, string name)
    {
        throw new NotImplementedException();
    }

    public Task<CloudFile> UploadAsync(string folderId, string relativePath, Stream content)
    {
        throw new NotImplementedException();
    }

    public Task DownloadAsync(string fileId, Stream destination)
    {
        throw new NotImplementedException();
    }

    public Task TrashAsync(string fileId)
    {
        throw new NotImplementedException();
    }
}
