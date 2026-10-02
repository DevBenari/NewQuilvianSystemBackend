using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.CashManagement;

public sealed class FinCashMovementConfiguration : IEntityTypeConfiguration<FinCashMovement>
{
    public void Configure(EntityTypeBuilder<FinCashMovement> entity)
    {
        entity.ToTable("FinCashMovement", "public", table =>
        {
            table.HasCheckConstraint("CK_FinCashMovement_Direction", "\"Direction\" IN ('IN','OUT')");
            table.HasCheckConstraint("CK_FinCashMovement_Amount", "\"Amount\" > 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.MovementType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Direction).HasMaxLength(3).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.BusinessDate).HasColumnType("date").IsRequired();
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();
        entity.Property(x => x.SourceReferenceType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.SourceReferenceId).HasMaxLength(100).IsRequired();
        entity.Property(x => x.PaymentMethodCode).HasMaxLength(30);
        entity.Property(x => x.Notes).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.SourceReferenceType, x.SourceReferenceId, x.MovementType })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinCashMovement_Source");

        entity.HasIndex(x => new { x.BusinessDate, x.Direction }).HasDatabaseName("IX_FinCashMovement_BusinessDate_Direction");
        entity.HasIndex(x => x.CashierShiftId).HasDatabaseName("IX_FinCashMovement_CashierShiftId");
        entity.HasIndex(x => x.CorrelationId).HasDatabaseName("IX_FinCashMovement_CorrelationId");
    }
}
