using TeensChurch.API.DTOs;
using TeensChurch.API.Models;

namespace TeensChurch.API.Services;

public interface ICsvImportService
{
    Task<BulkUploadResult> ProcessBulkUploadAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<byte[]> ExportMembersToCsvAsync(IEnumerable<Member> members);
    byte[] GenerateSampleCsvTemplate();
}
