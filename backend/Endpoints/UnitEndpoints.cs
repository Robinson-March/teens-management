using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeensChurch.API.Data;
using TeensChurch.API.DTOs;
using TeensChurch.API.Models;

namespace TeensChurch.API.Endpoints;

public static class UnitEndpoints
{
    public static void MapUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/units")
            .WithTags("Units");

        // 1. GET /api/units - Retrieve all church units with member counts
        group.MapGet("/", async (AppDbContext db) =>
        {
            var units = await db.Units.AsNoTracking().OrderBy(u => u.Name).ToListAsync();

            // Load member department strings to compute member counts
            var memberDepts = await db.Members.AsNoTracking()
                .Where(m => !string.IsNullOrEmpty(m.Departments))
                .Select(m => m.Departments!)
                .ToListAsync();

            var result = units.Select(u =>
            {
                var count = memberDepts.Count(d => d.IndexOf(u.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                return new UnitDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Description = u.Description,
                    BadgeColor = u.BadgeColor,
                    IconKey = u.IconKey,
                    IsDefault = u.IsDefault,
                    MemberCount = count,
                    CreatedAt = u.CreatedAt
                };
            }).ToList();

            return Results.Ok(result);
        })
        .WithName("GetUnits")
        .WithSummary("Retrieve all church units with active member counts.");

        // 2. POST /api/units - Add a new church unit
        group.MapPost("/", async (
            CreateUnitDto dto,
            IValidator<CreateUnitDto> validator,
            AppDbContext db) =>
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var trimmedName = dto.Name.Trim();
            var exists = await db.Units.AnyAsync(u => u.Name.ToLower() == trimmedName.ToLower());
            if (exists)
            {
                return Results.Conflict(new { message = $"A unit named '{trimmedName}' already exists." });
            }

            var unit = new ChurchUnit
            {
                Id = Guid.NewGuid(),
                Name = trimmedName,
                Description = dto.Description?.Trim(),
                BadgeColor = string.IsNullOrWhiteSpace(dto.BadgeColor) ? "purple" : dto.BadgeColor.Trim().ToLowerInvariant(),
                IconKey = string.IsNullOrWhiteSpace(dto.IconKey) ? "sparkles" : dto.IconKey.Trim(),
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Units.Add(unit);
            await db.SaveChangesAsync();

            var unitDto = new UnitDto
            {
                Id = unit.Id,
                Name = unit.Name,
                Description = unit.Description,
                BadgeColor = unit.BadgeColor,
                IconKey = unit.IconKey,
                IsDefault = unit.IsDefault,
                MemberCount = 0,
                CreatedAt = unit.CreatedAt
            };

            return Results.Created($"/api/units/{unit.Id}", unitDto);
        })
        .WithName("CreateUnit")
        .WithSummary("Create a new church unit.");

        // 3. PUT /api/units/{id} - Edit an existing unit
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateUnitDto dto,
            [FromHeader(Name = "X-Delete-Code")] string? code,
            IConfiguration config,
            IValidator<UpdateUnitDto> validator,
            AppDbContext db) =>
        {
            var expectedCode = config["DeleteCode"];
            if (!string.IsNullOrEmpty(expectedCode) && code != expectedCode)
            {
                return Results.Unauthorized();
            }

            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.ToDictionary());
            }

            var unit = await db.Units.FindAsync(id);
            if (unit is null)
            {
                return Results.NotFound(new { message = $"Unit with id '{id}' was not found." });
            }

            var newName = dto.Name.Trim();
            var oldName = unit.Name;

            // Check if name is taken by another unit
            var duplicate = await db.Units.AnyAsync(u => u.Id != id && u.Name.ToLower() == newName.ToLower());
            if (duplicate)
            {
                return Results.Conflict(new { message = $"Another unit named '{newName}' already exists." });
            }

            unit.Name = newName;
            unit.Description = dto.Description?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.BadgeColor))
            {
                unit.BadgeColor = dto.BadgeColor.Trim().ToLowerInvariant();
            }
            if (!string.IsNullOrWhiteSpace(dto.IconKey))
            {
                unit.IconKey = dto.IconKey.Trim();
            }

            // If unit name changed, update affected member records
            if (!oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
            {
                var affectedMembers = await db.Members
                    .Where(m => m.Departments != null && m.Departments.Contains(oldName))
                    .ToListAsync();

                foreach (var member in affectedMembers)
                {
                    if (member.Departments != null)
                    {
                        member.Departments = member.Departments.Replace(oldName, newName, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            await db.SaveChangesAsync();

            var memberDepts = await db.Members.AsNoTracking()
                .Where(m => !string.IsNullOrEmpty(m.Departments))
                .Select(m => m.Departments!)
                .ToListAsync();

            var count = memberDepts.Count(d => d.IndexOf(unit.Name, StringComparison.OrdinalIgnoreCase) >= 0);

            var unitDto = new UnitDto
            {
                Id = unit.Id,
                Name = unit.Name,
                Description = unit.Description,
                BadgeColor = unit.BadgeColor,
                IconKey = unit.IconKey,
                IsDefault = unit.IsDefault,
                MemberCount = count,
                CreatedAt = unit.CreatedAt
            };

            return Results.Ok(unitDto);
        })
        .WithName("UpdateUnit")
        .WithSummary("Update an existing church unit's details and badge styling.");

        // 4. DELETE /api/units/{id} - Remove a church unit
        group.MapDelete("/{id:guid}", async (
            Guid id,
            [FromQuery] bool? force,
            [FromHeader(Name = "X-Delete-Code")] string? code,
            IConfiguration config,
            AppDbContext db) =>
        {
            var expectedCode = config["DeleteCode"];
            if (!string.IsNullOrEmpty(expectedCode) && code != expectedCode)
            {
                return Results.Unauthorized();
            }

            var unit = await db.Units.FindAsync(id);
            if (unit is null)
            {
                return Results.NotFound(new { message = $"Unit with id '{id}' was not found." });
            }

            // Check if members are assigned to this unit
            var assignedMembers = await db.Members
                .Where(m => m.Departments != null && m.Departments.Contains(unit.Name))
                .ToListAsync();

            if (assignedMembers.Any())
            {
                if (force != true)
                {
                    return Results.Conflict(new
                    {
                        message = $"Cannot delete unit '{unit.Name}' because {assignedMembers.Count} member(s) are currently assigned to it.",
                        assignedCount = assignedMembers.Count
                    });
                }

                // If force=true, remove unit from affected members' department strings
                foreach (var member in assignedMembers)
                {
                    if (member.Departments != null)
                    {
                        var parts = member.Departments
                            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim())
                            .Where(p => !p.Equals(unit.Name, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        member.Departments = parts.Any() ? string.Join(", ", parts) : null;
                    }
                }
            }

            db.Units.Remove(unit);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteUnit")
        .WithSummary("Remove a church unit.");
    }
}
