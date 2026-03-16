using Chickquita.Application.Interfaces;
using Chickquita.Domain.Entities;
using Chickquita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chickquita.Api.Tests.Helpers;

/// <summary>
/// SQLite-compatible subclass of ApplicationDbContext for integration tests.
/// Adds a value converter for DailyRecord.CollectionTime (TimeSpan?) so SQLite can
/// handle ORDER BY on that column. PostgreSQL supports the "time" type natively but
/// SQLite requires the value stored as INTEGER (ticks).
/// </summary>
internal sealed class SqliteApplicationDbContext : ApplicationDbContext
{
    public SqliteApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DailyRecord>()
            .Property(d => d.CollectionTime)
            .HasColumnType("INTEGER")
            .HasConversion(
                v => v.HasValue ? v.Value.Ticks : (long?)null,
                v => v.HasValue ? TimeSpan.FromTicks(v.Value) : (TimeSpan?)null);
    }
}
