using Microsoft.EntityFrameworkCore;
using TeensChurch.API.Models;

namespace TeensChurch.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<ChurchUnit> Units => Set<ChurchUnit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChurchUnit>(entity =>
        {
            entity.ToTable("Units");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Description)
                .HasMaxLength(250);

            entity.Property(e => e.BadgeColor)
                .HasMaxLength(30)
                .HasDefaultValue("purple");

            entity.Property(e => e.IconKey)
                .HasMaxLength(50)
                .HasDefaultValue("sparkles");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Members");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ServiceTime)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.AcademicLevel)
                .HasMaxLength(50);

            entity.Property(e => e.Departments)
                .HasMaxLength(200);

            entity.Property(e => e.GuardianName)
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
