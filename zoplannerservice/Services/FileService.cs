using zoplannerservice.Models;
using zoplannerservice.Services.Interfaces;

namespace zoplannerservice.Services;
public class FileService : IFileService
{
    private readonly ISpringApiClient _springClient;
    private const string ApiEndpoint = "files";

    public FileService(ISpringApiClient springClient)
    {
        _springClient = springClient;
    }

    public async Task<FileResponse> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var uploaded = await _springClient.PostMultipartAsync<FileResponse>(ApiEndpoint, fileStream, fileName, contentType, ct);
        return uploaded ?? throw new InvalidOperationException("Backend returned null after upload");
    }

    public async Task<FileResponse> ReplaceAsync(int id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var updated = await _springClient.PutMultipartAsync<FileResponse>($"{ApiEndpoint}/{id}", fileStream, fileName, contentType, ct);
        return updated ?? throw new InvalidOperationException("Backend returned null after replace");
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _springClient.DeleteAsync($"{ApiEndpoint}/{id}", ct);
}

