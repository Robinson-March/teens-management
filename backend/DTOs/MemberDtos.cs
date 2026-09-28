namespace TeensChurch.API.DTOs;

public class CreateMemberDto
{
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AcademicLevel { get; set; }
    public string? Departments { get; set; }
    public string ServiceTime { get; set; } = "8:30 service";
    public string? GuardianName { get; set; }
    public string? Status { get; set; } = "active";
}

public class UpdateMemberDto
{
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AcademicLevel { get; set; }
    public string? Departments { get; set; }
    public string ServiceTime { get; set; } = "8:30 service";
    public string? GuardianName { get; set; }
    public string? Status { get; set; } = "active";
}

public class MemberQueryParameters
{
    public string? Search { get; set; }
    public string? Department { get; set; }
    public string? Service { get; set; }
    public string? Status { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

public class BulkUploadResult
{
    public int TotalProcessed { get; set; }
    public int SuccessfulInserts { get; set; }
    public int FailedRows { get; set; }
    public List<BulkUploadError> Errors { get; set; } = new();
}

public class BulkUploadError
{
    public int Row { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class DashboardStatsDto
{
    public int TotalRegistered { get; set; }
    public int ActiveUnitsCount { get; set; }
    public double AverageAge { get; set; }
    public int SeniorHighAndCandidatesCount { get; set; }
    public int ActiveCount { get; set; }
    public int AttentionCount { get; set; }
    public Dictionary<string, double> AcademicDistribution { get; set; } = new();
}
