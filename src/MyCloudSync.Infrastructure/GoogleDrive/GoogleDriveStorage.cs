using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace MyCloudSync.Infrastructure.GoogleDrive;

public class GoogleDriveStorage : ICloudStorage
{
    private const string FolderMimeType = "application/vnd.google-apps.folder";
    private const string FileFields = "id, name, md5Checksum, size, modifiedTime";

    private readonly GoogleAuthService _authService;

    public GoogleDriveStorage(GoogleAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IReadOnlyList<CloudFile>> ListFilesAsync(string folderId)
    {
        var service = await _authService.GetDriveServiceAsync();
        await CheckFolderExistsAsync(service, folderId);

        var files = new List<CloudFile>();
        await AddFilesAsync(service, folderId, "", files);
        return files;
    }

    public async Task<IReadOnlyList<CloudFolder>> ListFoldersAsync(string parentId)
    {
        var service = await _authService.GetDriveServiceAsync();
        var items = await ListChildrenAsync(service, parentId, $" and mimeType = '{FolderMimeType}'");

        return items
            .Select(item => new CloudFolder(item.Id, item.Name))
            .OrderBy(folder => folder.Name)
            .ToList();
    }

    public async Task<CloudFolder> CreateFolderAsync(string parentId, string name)
    {
        var service = await _authService.GetDriveServiceAsync();
        var id = await CreateFolderAsync(service, parentId, name);
        return new CloudFolder(id, name);
    }

    public async Task<CloudFile> UploadAsync(string folderId, string relativePath, Stream content)
    {
        var service = await _authService.GetDriveServiceAsync();
        var parts = relativePath.Split('/');

        var parentId = folderId;
        foreach (var folderName in parts[..^1])
        {
            parentId = await GetOrCreateFolderAsync(service, parentId, folderName);
        }

        var metadata = new DriveFile { Name = parts[^1], Parents = new List<string> { parentId } };
        var request = service.Files.Create(metadata, content, "application/octet-stream");
        request.Fields = FileFields;

        var progress = await request.UploadAsync();
        if (progress.Status != UploadStatus.Completed)
        {
            throw progress.Exception ?? new IOException($"Не вдалося вивантажити {relativePath}");
        }

        return ToCloudFile(request.ResponseBody, relativePath);
    }

    public async Task<CloudFile> UpdateAsync(string fileId, string relativePath, Stream content)
    {
        var service = await _authService.GetDriveServiceAsync();
        var request = service.Files.Update(new DriveFile(), fileId, content, "application/octet-stream");
        request.Fields = FileFields;

        var progress = await request.UploadAsync();
        if (progress.Status != UploadStatus.Completed)
        {
            throw progress.Exception ?? new IOException($"Не вдалося вивантажити {relativePath}");
        }

        return ToCloudFile(request.ResponseBody, relativePath);
    }

    public async Task DownloadAsync(string fileId, Stream destination)
    {
        var service = await _authService.GetDriveServiceAsync();
        var progress = await service.Files.Get(fileId).DownloadAsync(destination);

        if (progress.Status != DownloadStatus.Completed)
        {
            throw progress.Exception ?? new IOException("Не вдалося завантажити файл");
        }
    }

    public async Task TrashAsync(string fileId)
    {
        var service = await _authService.GetDriveServiceAsync();
        await service.Files.Update(new DriveFile { Trashed = true }, fileId).ExecuteAsync();
    }

    private static async Task CheckFolderExistsAsync(DriveService service, string folderId)
    {
        var request = service.Files.Get(folderId);
        request.Fields = "id, trashed";
        var folder = await request.ExecuteAsync();

        if (folder.Trashed == true)
        {
            throw new InvalidOperationException("Папку на Google Drive видалено. Оберіть іншу папку в налаштуваннях.");
        }
    }

    private static async Task AddFilesAsync(DriveService service, string folderId, string pathPrefix, List<CloudFile> files)
    {
        foreach (var item in await ListChildrenAsync(service, folderId, ""))
        {
            var path = pathPrefix + item.Name;

            if (item.MimeType == FolderMimeType)
            {
                await AddFilesAsync(service, item.Id, path + "/", files);
            }
            else if (item.Md5Checksum != null)
            {
                files.Add(ToCloudFile(item, path));
            }
        }
    }

    private static async Task<List<DriveFile>> ListChildrenAsync(DriveService service, string folderId, string extraFilter)
    {
        var items = new List<DriveFile>();
        string? pageToken = null;

        do
        {
            var request = service.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false{extraFilter}";
            request.Fields = "nextPageToken, files(id, name, mimeType, md5Checksum, size, modifiedTime)";
            request.PageSize = 1000;
            request.PageToken = pageToken;

            var result = await request.ExecuteAsync();
            items.AddRange(result.Files);
            pageToken = result.NextPageToken;
        }
        while (pageToken != null);

        return items;
    }

    private static async Task<string> GetOrCreateFolderAsync(DriveService service, string parentId, string name)
    {
        var escapedName = name.Replace("\\", "\\\\").Replace("'", "\\'");
        var existing = await ListChildrenAsync(service, parentId, $" and mimeType = '{FolderMimeType}' and name = '{escapedName}'");

        return existing.Count > 0 ? existing[0].Id : await CreateFolderAsync(service, parentId, name);
    }

    private static async Task<string> CreateFolderAsync(DriveService service, string parentId, string name)
    {
        var metadata = new DriveFile { Name = name, MimeType = FolderMimeType, Parents = new List<string> { parentId } };
        var request = service.Files.Create(metadata);
        request.Fields = "id";

        var folder = await request.ExecuteAsync();
        return folder.Id;
    }

    private static CloudFile ToCloudFile(DriveFile file, string relativePath)
    {
        var modifiedAt = file.ModifiedTimeDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow;
        return new CloudFile(file.Id, relativePath, file.Md5Checksum, file.Size ?? 0, modifiedAt);
    }
}
