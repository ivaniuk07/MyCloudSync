using MyCloudSync.Core.Models;

namespace MyCloudSync.Core.Abstractions;

public interface ICloudStorage
{
    Task<IReadOnlyList<CloudFile>> ListFilesAsync(string folderId);
    Task<IReadOnlyList<CloudFolder>> ListFoldersAsync(string parentId);
    Task<CloudFolder> CreateFolderAsync(string parentId, string name);
    Task<CloudFile> UploadAsync(string folderId, string relativePath, Stream content);
    Task DownloadAsync(string fileId, Stream destination);
    Task TrashAsync(string fileId);
}
