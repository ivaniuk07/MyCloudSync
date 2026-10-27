using System.Security.Cryptography;
using MyCloudSync.Core.Abstractions;
using MyCloudSync.Core.Models;

namespace MyCloudSync.Tests;

public class InMemoryCloudStorage : ICloudStorage
{
    private readonly Dictionary<string, (string Path, byte[] Content)> _files = new();
    private int _nextId = 1;

    public bool IsOffline { get; set; }

    public List<string> TrashedPaths { get; } = new();

    public string AddFile(string relativePath, string text)
    {
        var id = $"file{_nextId++}";
        _files[id] = (relativePath, System.Text.Encoding.UTF8.GetBytes(text));
        return id;
    }

    public void ChangeFile(string relativePath, string text)
    {
        var id = _files.First(file => file.Value.Path == relativePath).Key;
        _files[id] = (relativePath, System.Text.Encoding.UTF8.GetBytes(text));
    }

    public string? ReadFile(string relativePath)
    {
        var file = _files.Values.FirstOrDefault(value => value.Path == relativePath);
        return file.Content == null ? null : System.Text.Encoding.UTF8.GetString(file.Content);
    }

    public Task<IReadOnlyList<CloudFile>> ListFilesAsync(string folderId)
    {
        if (IsOffline)
        {
            throw new HttpRequestException("offline");
        }

        IReadOnlyList<CloudFile> files = _files.Select(file => ToCloudFile(file.Key)).ToList();
        return Task.FromResult(files);
    }

    public Task<IReadOnlyList<CloudFolder>> ListFoldersAsync(string parentId)
    {
        return Task.FromResult<IReadOnlyList<CloudFolder>>(new List<CloudFolder>());
    }

    public Task<CloudFolder> CreateFolderAsync(string parentId, string name)
    {
        return Task.FromResult(new CloudFolder($"folder{_nextId++}", name));
    }

    public Task<CloudFile> UploadAsync(string folderId, string relativePath, Stream content)
    {
        var id = $"file{_nextId++}";
        _files[id] = (relativePath, ReadAll(content));
        return Task.FromResult(ToCloudFile(id));
    }

    public Task<CloudFile> UpdateAsync(string fileId, string relativePath, Stream content)
    {
        _files[fileId] = (relativePath, ReadAll(content));
        return Task.FromResult(ToCloudFile(fileId));
    }

    public async Task DownloadAsync(string fileId, Stream destination)
    {
        await destination.WriteAsync(_files[fileId].Content);
    }

    public Task TrashAsync(string fileId)
    {
        TrashedPaths.Add(_files[fileId].Path);
        _files.Remove(fileId);
        return Task.CompletedTask;
    }

    private CloudFile ToCloudFile(string id)
    {
        var (path, content) = _files[id];
        var md5 = Convert.ToHexString(MD5.HashData(content)).ToLowerInvariant();
        return new CloudFile(id, path, md5, content.Length, DateTime.UtcNow);
    }

    private static byte[] ReadAll(Stream stream)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
