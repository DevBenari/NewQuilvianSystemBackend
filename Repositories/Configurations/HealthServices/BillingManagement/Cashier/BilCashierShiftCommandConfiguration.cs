using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Configurations;

public sealed class BilCashierShiftCommandConfiguration : IEntityTypeConfiguration<BilCashierShiftCommand>
{
    public void Configure(EntityTypeBuilder<BilCashierShiftCommand> entity)
    {
        entity.ToTable("BilCashierShiftCommand", "public");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CommandType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.ActorRole).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Authority).HasMaxLength(100).IsRequired();
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.StatusBefore).HasMaxLength(30);
        entity.Property(x => x.StatusAfter).HasMaxLength(30).IsRequired();
        entity.Property(x => x.OpeningCash).HasPrecision(18, 2);
        entity.Property(x => x.SystemCash).HasPrecision(18, 2);
        entity.Property(x => x.PhysicalCash).HasPrecision(18, 2);
        entity.Property(x => x.Variance).HasPrecision(18, 2);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500);
        entity.Property(x => x.SourceType).HasMaxLength(40);
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ResponseJson).HasColumnType("jsonb").IsRequired();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.IdempotencyKey).IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => new { x.SourceType, x.SourceId }).IsUnique()
            .HasFilter("\"SourceType\" IS NOT NULL AND \"SourceId\" IS NOT NULL");
        entity.HasIndex(x => new { x.ShiftId, x.OccurredAt });
        entity.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
