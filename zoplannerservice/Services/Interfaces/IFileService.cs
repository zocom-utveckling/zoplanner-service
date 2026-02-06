using zoplannerservice.Models;

namespace zoplannerservice.Services.Interfaces;
public interface IFileService
{
    Task<FileResponse> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<FileResponse> ReplaceAsync(int id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
