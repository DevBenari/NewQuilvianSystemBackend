using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilFinalizationRecordConfiguration : IEntityTypeConfiguration<BilFinalizationRecord>
{
    public void Configure(EntityTypeBuilder<BilFinalizationRecord> entity)
    {
        entity.ToTable("BilFinalizationRecord", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_BilFinalizationRecord_DepartureReason",
                "\"DepartureReason\" IS NULL OR \"DepartureReason\" IN ('DEATH','EMERGENCY_TRANSFER','DAMA')");
            table.HasCheckConstraint(
                "CK_BilFinalizationRecord_DepartureConsistency",
                "(\"IsDepartureException\" = FALSE AND \"DepartureReason\" IS NULL) OR (\"IsDepartureException\" = TRUE AND \"DepartureReason\" IS NOT NULL AND \"DebtorIdentity\" IS NOT NULL)");
            table.HasCheckConstraint(
                "CK_BilFinalizationRecord_OutstandingNonNegative",
                "\"OutstandingAtFinalization\" >= 0");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DepartureReason).HasMaxLength(30);
        entity.Property(x => x.DebtorIdentity).HasMaxLength(200);
        entity.Property(x => x.DebtorRelationship).HasMaxLength(100);
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.OutstandingAtFinalization).HasPrecision(18, 2);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.FinalizedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.IdempotencyKey).IsUnique();
        entity.HasIndex(x => x.CorrelationId).IsUnique();
        entity.HasIndex(x => x.InvoiceId).IsUnique();
        entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
