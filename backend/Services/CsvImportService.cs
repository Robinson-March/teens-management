using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using TeensChurch.API.Data;
using TeensChurch.API.DTOs;
using TeensChurch.API.Models;

namespace TeensChurch.API.Services;

public sealed class MemberCsvRecord
{
    public string? FullName { get; set; }
    public string? Age { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AcademicLevel { get; set; }
    public string? Departments { get; set; }
    public string? ServiceTime { get; set; }
    public string? GuardianName { get; set; }
}

public sealed class MemberCsvMap : ClassMap<MemberCsvRecord>
{
    public MemberCsvMap()
    {
        Map(m => m.FullName).Name("FullName", "Name", "Full Name", "Member Name", "Teen Member");
        Map(m => m.Age).Name("Age", "Years", "Date of Birth / Age");
        Map(m => m.PhoneNumber).Name("PhoneNumber", "Phone", "Phone Number", "Contact", "Parent / Guardian WhatsApp Contact").Optional();
        Map(m => m.AcademicLevel).Name("AcademicLevel", "Level", "Academic Level", "Class", "Academic Class").Optional();
        Map(m => m.Departments).Name("Departments", "Interested Department", "Department", "Unit", "Church Unit", "Assigned Ministry Unit").Optional();
        Map(m => m.ServiceTime).Name("ServiceTime", "Service", "Service Time", "Attendance Block").Optional();
        Map(m => m.GuardianName).Name("GuardianName", "Guardian", "Guardian Name", "Parent", "Parent / Guardian").Optional();
    }
}

public class CsvImportService : ICsvImportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(AppDbContext context, ILogger<CsvImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BulkUploadResult> ProcessBulkUploadAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        var result = new BulkUploadResult();
        var validMembers = new List<Member>();

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            PrepareHeaderForMatch = args => args.Header.ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "")
        };

        using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        using var csv = new CsvReader(reader, csvConfig);
        csv.Context.RegisterClassMap<MemberCsvMap>();

        if (!await csv.ReadAsync())
        {
            return result;
        }

        csv.ReadHeader();

        int rowNumber = 1; // Header is row 1
        while (await csv.ReadAsync())
        {
            rowNumber++;
            result.TotalProcessed++;

            try
            {
                var record = csv.GetRecord<MemberCsvRecord>();
                if (record == null)
                {
                    result.FailedRows++;
                    result.Errors.Add(new BulkUploadError
                    {
                        Row = rowNumber,
                        Reason = "Empty record found."
                    });
                    continue;
                }

                // 1. FullName validation
                var fullName = record.FullName?.Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    result.FailedRows++;
                    result.Errors.Add(new BulkUploadError
                    {
                        Row = rowNumber,
                        Reason = "FullName is required but was blank."
                    });
                    continue;
                }

                // 2. Age parsing & sanitization
                int? parsedAge = null;
                var rawAge = record.Age?.Trim();
                if (!string.IsNullOrWhiteSpace(rawAge))
                {
                    if (string.Equals(rawAge, "Not Provided", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rawAge, "N/A", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rawAge, "None", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rawAge, "null", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(rawAge, "-", StringComparison.OrdinalIgnoreCase))
                    {
                        parsedAge = null;
                    }
                    else
                    {
                        // Handle composite formats like "14/15" or "14-15"
                        var candidate = rawAge.Split(new[] { '/', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        if (candidate != null && int.TryParse(candidate, out int ageVal))
                        {
                            if (ageVal is >= 1 and <= 120)
                            {
                                parsedAge = ageVal;
                            }
                            else
                            {
                                result.FailedRows++;
                                result.Errors.Add(new BulkUploadError
                                {
                                    Row = rowNumber,
                                    Reason = "Age value out of realistic range (1-120)."
                                });
                                continue;
                            }
                        }
                        else
                        {
                            result.FailedRows++;
                            result.Errors.Add(new BulkUploadError
                            {
                                Row = rowNumber,
                                Reason = "Age format invalid (expected number)."
                            });
                            continue;
                        }
                    }
                }

                // 3. ServiceTime default or validation
                var serviceTime = string.IsNullOrWhiteSpace(record.ServiceTime)
                    ? "8:30 service"
                    : record.ServiceTime.Trim();

                // 4. Academic level normalization
                string? parsedAcademicLevel = null;
                if (!string.IsNullOrWhiteSpace(record.AcademicLevel))
                {
                    parsedAcademicLevel = AcademicLevelClassifier.NormalizeLevel(record.AcademicLevel);
                }

                // 5. Phone sanitization
                string? parsedPhone = null;
                var rawPhone = record.PhoneNumber?.Trim();
                if (!string.IsNullOrWhiteSpace(rawPhone) &&
                    !string.Equals(rawPhone, "Not Provided", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(rawPhone, "don't have", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(rawPhone, "none", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(rawPhone, "n/a", StringComparison.OrdinalIgnoreCase) &&
                    rawPhone != "-")
                {
                    parsedPhone = rawPhone;
                }

                // 6. Department sanitization & default to General Assembly
                string? parsedDept = null;
                var rawDept = record.Departments?.Trim();
                if (!string.IsNullOrWhiteSpace(rawDept) &&
                    !string.Equals(rawDept, "Not Provided", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(rawDept, "Not Interested", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(rawDept, "none", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(rawDept, "n/a", StringComparison.OrdinalIgnoreCase) &&
                    rawDept != "-")
                {
                    if (string.Equals(rawDept, "Choir (wants to learn)", StringComparison.OrdinalIgnoreCase))
                    {
                        parsedDept = "Choir";
                    }
                    else
                    {
                        parsedDept = rawDept;
                    }
                }
                else
                {
                    parsedDept = "General Assembly";
                }

                var member = new Member
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Age = parsedAge,
                    PhoneNumber = parsedPhone,
                    AcademicLevel = parsedAcademicLevel,
                    Departments = parsedDept,
                    ServiceTime = serviceTime,
                    GuardianName = string.IsNullOrWhiteSpace(record.GuardianName) ? null : record.GuardianName.Trim(),
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };

                validMembers.Add(member);
            }
            catch (Exception ex)
            {
                result.FailedRows++;
                result.Errors.Add(new BulkUploadError
                {
                    Row = rowNumber,
                    Reason = $"Parsing error: {ex.Message}"
                });
            }
        }

        // Database Transaction - save all valid records in a single atomic transaction
        if (validMembers.Count > 0)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _context.Members.AddRangeAsync(validMembers, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    result.SuccessfulInserts = validMembers.Count;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Transaction rolled back during bulk upload.");
                    throw;
                }
            });
        }

        return result;
    }

    public async Task<byte[]> ExportMembersToCsvAsync(IEnumerable<Member> members)
    {
        using var memoryStream = new MemoryStream();
        await using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("FullName");
            csv.WriteField("Age");
            csv.WriteField("PhoneNumber");
            csv.WriteField("AcademicLevel");
            csv.WriteField("Departments");
            csv.WriteField("ServiceTime");
            csv.WriteField("GuardianName");
            csv.WriteField("Status");
            csv.WriteField("CreatedAt");
            await csv.NextRecordAsync();

            foreach (var m in members)
            {
                csv.WriteField(m.FullName);
                csv.WriteField(m.Age?.ToString() ?? "Not Provided");
                csv.WriteField(m.PhoneNumber ?? "");
                csv.WriteField(m.AcademicLevel ?? "");
                csv.WriteField(m.Departments ?? "");
                csv.WriteField(m.ServiceTime);
                csv.WriteField(m.GuardianName ?? "");
                csv.WriteField(m.Status);
                csv.WriteField(m.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync();
        }

        return memoryStream.ToArray();
    }

    public byte[] GenerateSampleCsvTemplate()
    {
        var sampleRows = new StringBuilder();
        sampleRows.AppendLine("FullName,Age,PhoneNumber,AcademicLevel,Departments,ServiceTime,GuardianName");
        sampleRows.AppendLine("Samuel Adekunle,15,0803 111 2233,SSS 2,Choir & Vocals,8:30 service,Deacon Adekunle");
        sampleRows.AppendLine("Grace Olamide,16,0812 555 7788,SSS 3,Media & Sound Tech,6:30 service,Mrs. Olamide");
        sampleRows.AppendLine("Ezekiel Bassey,Not Provided,0809 333 4455,JSS 3,Teens Ushering,8:30 service,Mr. Bassey");
        sampleRows.AppendLine("Faithful Eze,14,0802 999 0011,JSS 2,Sanctuary Keepers,8:30 service,Mrs. Eze");

        return Encoding.UTF8.GetBytes(sampleRows.ToString());
    }
}
