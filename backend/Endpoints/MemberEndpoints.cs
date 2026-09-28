using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeensChurch.API.Data;
using TeensChurch.API.DTOs;
using TeensChurch.API.Models;
using TeensChurch.API.Services;

namespace TeensChurch.API.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members");

        // 1. GET /api/members - Paginated list with filtering
        group.MapGet("/", async (
            [AsParameters] MemberQueryParameters query,
            AppDbContext db) =>
        {
            var membersQuery = db.Members.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                membersQuery = membersQuery.Where(m =>
                    m.FullName.ToLower().Contains(term) ||
                    (m.PhoneNumber != null && m.PhoneNumber.Contains(term)) ||
                    (m.GuardianName != null && m.GuardianName.ToLower().Contains(term)) ||
                    (m.AcademicLevel != null && m.AcademicLevel.ToLower().Contains(term)) ||
                    (m.Departments != null && m.Departments.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(query.Department))
            {
                var dept = query.Department.Trim().ToLower();
                membersQuery = membersQuery.Where(m => m.Departments != null && m.Departments.ToLower().Contains(dept));
            }

            if (!string.IsNullOrWhiteSpace(query.Service))
            {
                var srv = query.Service.Trim().ToLower();
                membersQuery = membersQuery.Where(m => m.ServiceTime.ToLower().Contains(srv));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var st = query.Status.Trim().ToLower();
                membersQuery = membersQuery.Where(m => m.Status.ToLower() == st);
            }

            var totalCount = await membersQuery.CountAsync();
            var page = Math.Max(1, query.Page ?? 1);
            var pageSize = Math.Clamp(query.PageSize ?? 50, 1, 100);

            var items = await membersQuery
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Member>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Results.Ok(result);
        })
        .WithName("GetMembers")
        .WithSummary("Retrieve a paginated list of teens church members.");

        // 2. GET /api/members/stats - Dashboard analytics
        group.MapGet("/stats", async (AppDbContext db) =>
        {
            var total = await db.Members.CountAsync();
            var activeCount = await db.Members.CountAsync(m => m.Status == "active");
            var attentionCount = await db.Members.CountAsync(m => m.Status == "attention");

            var avgAgeQuery = await db.Members
                .Where(m => m.Age.HasValue)
                .Select(m => m.Age!.Value)
                .ToListAsync();
            var averageAge = avgAgeQuery.Count > 0 ? Math.Round(avgAgeQuery.Average(), 1) : 15.0;

            // Senior High / WAEC / JAMB count
            var seniorHighCount = await db.Members.CountAsync(m =>
                m.AcademicLevel != null && (
                    m.AcademicLevel.Contains("SS") ||
                    m.AcademicLevel.Contains("SSS") ||
                    m.AcademicLevel.Contains("Pre-Varsity") ||
                    m.AcademicLevel.Contains("JAMB") ||
                    m.AcademicLevel.Contains("A-Level") ||
                    m.AcademicLevel.Contains("Jambite")));

            // Unique units
            var allDepartments = await db.Members
                .Where(m => !string.IsNullOrEmpty(m.Departments))
                .Select(m => m.Departments!)
                .ToListAsync();

            var distinctUnits = allDepartments
                .SelectMany(d => d.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(d => d.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            // Academic distribution
            var jss12 = await db.Members.CountAsync(m => m.AcademicLevel != null && (m.AcademicLevel.Contains("JSS 1") || m.AcademicLevel.Contains("JSS 2") || m.AcademicLevel.Contains("JSS1") || m.AcademicLevel.Contains("JSS2")));
            var jss3 = await db.Members.CountAsync(m => m.AcademicLevel != null && (m.AcademicLevel.Contains("JSS 3") || m.AcademicLevel.Contains("JSS3")));
            var ss1 = await db.Members.CountAsync(m => m.AcademicLevel != null && (m.AcademicLevel.Contains("SS 1") || m.AcademicLevel.Contains("SSS 1") || m.AcademicLevel.Contains("SS1") || m.AcademicLevel.Contains("SSS1")));
            var ss23 = await db.Members.CountAsync(m => m.AcademicLevel != null && (m.AcademicLevel.Contains("SS 2") || m.AcademicLevel.Contains("SS 3") || m.AcademicLevel.Contains("SSS 2") || m.AcademicLevel.Contains("SSS 3") || m.AcademicLevel.Contains("SS2") || m.AcademicLevel.Contains("SS3")));
            var preVarsity = await db.Members.CountAsync(m => m.AcademicLevel != null && (m.AcademicLevel.Contains("Pre-Varsity") || m.AcademicLevel.Contains("Jambite") || m.AcademicLevel.Contains("A-Level")));

            double SafePercent(int count) => total > 0 ? Math.Round((double)count / total * 100, 1) : 0;

            var stats = new DashboardStatsDto
            {
                TotalRegistered = total,
                ActiveUnitsCount = Math.Max(distinctUnits, 6),
                AverageAge = averageAge,
                SeniorHighAndCandidatesCount = seniorHighCount,
                ActiveCount = activeCount,
                AttentionCount = attentionCount,
                AcademicDistribution = new Dictionary<string, double>
                {
                    ["Junior Teens (JSS1-2)"] = SafePercent(jss12),
                    ["Graduating JSS (JSS3)"] = SafePercent(jss3),
                    ["Senior Teens (SS1)"] = SafePercent(ss1),
                    ["Exam Class (SS2/SS3)"] = SafePercent(ss23),
                    ["Pre-Varsity / Gap"] = SafePercent(preVarsity)
                }
            };

            return Results.Ok(stats);
        })
        .WithName("GetDashboardStats")
        .WithSummary("Retrieve dashboard analytics and statistics.");

        // 3. GET /api/members/{id} - Get member by Id
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var member = await db.Members.FindAsync(id);
            return member is not null ? Results.Ok(member) : Results.NotFound(new { message = $"Member with id '{id}' was not found." });
        })
        .WithName("GetMemberById")
        .WithSummary("Retrieve a specific member's full profile.");

        // 4. POST /api/members - Create a new member
        group.MapPost("/", async (
            CreateMemberDto dto,
            IValidator<CreateMemberDto> validator,
            AppDbContext db) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var member = new Member
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName.Trim(),
                Age = dto.Age,
                PhoneNumber = dto.PhoneNumber?.Trim(),
                AcademicLevel = dto.AcademicLevel?.Trim(),
                Departments = dto.Departments?.Trim(),
                ServiceTime = dto.ServiceTime.Trim(),
                GuardianName = dto.GuardianName?.Trim(),
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "active" : dto.Status.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            db.Members.Add(member);
            await db.SaveChangesAsync();

            return Results.Created($"/api/members/{member.Id}", member);
        })
        .WithName("CreateMember")
        .WithSummary("Create a new member profile.");

        // 5. PUT /api/members/{id} - Update existing member
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateMemberDto dto,
            IValidator<UpdateMemberDto> validator,
            AppDbContext db) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var member = await db.Members.FindAsync(id);
            if (member is null)
            {
                return Results.NotFound(new { message = $"Member with id '{id}' was not found." });
            }

            member.FullName = dto.FullName.Trim();
            member.Age = dto.Age;
            member.PhoneNumber = dto.PhoneNumber?.Trim();
            member.AcademicLevel = dto.AcademicLevel?.Trim();
            member.Departments = dto.Departments?.Trim();
            member.ServiceTime = dto.ServiceTime.Trim();
            member.GuardianName = dto.GuardianName?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                member.Status = dto.Status.Trim();
            }

            await db.SaveChangesAsync();
            return Results.Ok(member);
        })
        .WithName("UpdateMember")
        .WithSummary("Update an existing member profile.");

        // 6. DELETE /api/members/{id} - Remove member
        group.MapDelete("/{id:guid}", async (Guid id, [FromHeader(Name = "X-Delete-Code")] string? code, IConfiguration config, AppDbContext db) =>
        {
            var expectedCode = config["DeleteCode"];
            if (string.IsNullOrEmpty(expectedCode) || code != expectedCode)
            {
                return Results.Unauthorized();
            }

            var member = await db.Members.FindAsync(id);
            if (member is null)
            {
                return Results.NotFound(new { message = $"Member with id '{id}' was not found." });
            }

            db.Members.Remove(member);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteMember")
        .WithSummary("Remove a member from the database.");

        // 7. POST /api/members/bulk-upload - Bulk CSV upload
        group.MapPost("/bulk-upload", async (
            IFormFile? file,
            ICsvImportService csvService) =>
        {
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { message = "No file was uploaded." });
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB per PRD Section 5
            if (file.Length > maxFileSize)
            {
                return Results.BadRequest(new { message = "File size exceeds the 5MB limit." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".csv")
            {
                return Results.BadRequest(new { message = "Invalid file format. Please upload a valid .csv file." });
            }

            using var stream = file.OpenReadStream();
            var result = await csvService.ProcessBulkUploadAsync(stream);

            return Results.Ok(result);
        })
        .DisableAntiforgery()
        .RequireRateLimiting("BulkUploadPolicy")
        .WithName("BulkUploadMembers")
        .WithSummary("Bulk import church members via CSV upload.");

        // 8. GET /api/members/export - Export all members to CSV
        group.MapGet("/export", async (
            [AsParameters] MemberQueryParameters query,
            AppDbContext db,
            ICsvImportService csvService) =>
        {
            var membersQuery = db.Members.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                membersQuery = membersQuery.Where(m =>
                    m.FullName.ToLower().Contains(term) ||
                    (m.PhoneNumber != null && m.PhoneNumber.Contains(term)) ||
                    (m.GuardianName != null && m.GuardianName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                membersQuery = membersQuery.Where(m => m.Status.ToLower() == query.Status.Trim().ToLower());
            }

            var members = await membersQuery.OrderBy(m => m.FullName).ToListAsync();
            var csvBytes = await csvService.ExportMembersToCsvAsync(members);

            return Results.File(
                csvBytes,
                contentType: "text/csv",
                fileDownloadName: $"TeensChurch_Members_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        })
        .WithName("ExportMembersCsv")
        .WithSummary("Export member roster as CSV.");

        // 9. GET /api/members/template - Download sample CSV template
        group.MapGet("/template", (ICsvImportService csvService) =>
        {
            var templateBytes = csvService.GenerateSampleCsvTemplate();
            return Results.File(
                templateBytes,
                contentType: "text/csv",
                fileDownloadName: "TeensChurch_Registration_Template.csv");
        })
        .WithName("GetCsvTemplate")
        .WithSummary("Download sample CSV import template.");
    }
}
