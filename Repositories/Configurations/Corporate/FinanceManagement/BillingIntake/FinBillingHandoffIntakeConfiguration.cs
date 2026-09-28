using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.BillingIntake;

public sealed class FinBillingHandoffIntakeConfiguration : IEntityTypeConfiguration<FinBillingHandoffIntake>
{
    public void Configure(EntityTypeBuilder<FinBillingHandoffIntake> entity)
    {
        entity.ToTable("FinBillingHandoffIntake", "public", table =>
        {
            // Empat nilai terakhir ditambahkan BE-FIN-022 (FIN-DES-029) untuk mutasi deposit,
            // kelebihan bayar, pengembalian, dan pengesahan selisih kas shift.
            table.HasCheckConstraint("CK_FinBillingHandoffIntake_HandoffType", "\"HandoffType\" IN ('AR','AP','COLLECTION','ADJUSTMENT','DEPOSIT_MOVEMENT','REFUNDABLE_CREDIT','REFUND_CASE','CASH_VARIANCE_REVIEW')");
            table.HasCheckConstraint("CK_FinBillingHandoffIntake_Status", "\"Status\" IN ('NEW','CONSUMED','ACKNOWLEDGED','ERROR')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.HandoffType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinBillingHandoffIntakeStatuses.New);
        entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
        entity.Property(x => x.RetryCount).HasDefaultValue(0);
        entity.Property(x => x.ConsumedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Satu fakta Billing hanya boleh diolah satu kali (FIN-DES-008, FIN-DES-009).
        entity.HasIndex(x => new { x.HandoffType, x.SourceHandoffKey })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinBillingHandoffIntake_Identity");

        entity.HasIndex(x => new { x.Status, x.HandoffType })
            .HasDatabaseName("IX_FinBillingHandoffIntake_Status");
    }
}
