using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilNumberSeriesConfiguration : IEntityTypeConfiguration<BilNumberSeries>
{
    public void Configure(EntityTypeBuilder<BilNumberSeries> entity)
    {
        entity.ToTable("BilNumberSeries", "public", table =>
        {
            table.HasCheckConstraint("CK_BilNumberSeries_CurrentValue", "\"CurrentValue\" > 0");
            table.HasCheckConstraint("CK_BilNumberSeries_ResetPolicy", "\"ResetPolicy\" IN ('NEVER','YEARLY','MONTHLY','DAILY')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SequenceKey).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ScopeKey).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ResetPolicy).HasMaxLength(20).IsRequired();
        entity.Property(x => x.LastAllocatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => new { x.SequenceKey, x.ScopeKey }).IsUnique();
    }
}
