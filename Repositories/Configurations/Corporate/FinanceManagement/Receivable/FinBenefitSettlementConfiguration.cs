using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinBenefitSettlementConfiguration : IEntityTypeConfiguration<FinBenefitSettlement>
{
    public void Configure(EntityTypeBuilder<FinBenefitSettlement> entity)
    {
        entity.ToTable("FinBenefitSettlement", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_FinBenefitSettlement_Status",
                "\"Status\" IN ('DRAF','DITERBITKAN','DIBATALKAN')");
            table.HasCheckConstraint(
                "CK_FinBenefitSettlement_Total",
                "\"TotalAmount\" > 0 AND \"ItemCount\" > 0");
            // Wajib terisi bila DITERBITKAN (FIN-DES-102).
            table.HasCheckConstraint(
                "CK_FinBenefitSettlement_Posted",
                "(\"Status\" = 'DITERBITKAN' AND \"PostedBy\" IS NOT NULL AND \"PostedAt\" IS NOT NULL) OR \"Status\" <> 'DITERBITKAN'");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.SettlementNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.AccountingPeriodCode).HasMaxLength(7).IsRequired();
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinBenefitSettlementStatuses.Draf);
        entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelReason).HasMaxLength(500);
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Kejadian Accounting yang dititipkan — tertahan FIN-OQ-103, kolom tetap kosong untuk saat ini.
        entity.HasOne(x => x.AccountingEvent)
            .WithMany()
            .HasForeignKey(x => x.AccountingEventId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.SettlementNumber).IsUnique().HasDatabaseName("IX_FinBenefitSettlement_SettlementNumber");
        // FIN-DES-102: satu periode untuk satu penjamin hanya boleh punya satu pelunasan yang tidak dibatalkan.
        entity.HasIndex(x => new { x.AccountingPeriodCode, x.DebtorReferenceId })
            .IsUnique()
            .HasFilter("\"Status\" <> 'DIBATALKAN'")
            .HasDatabaseName("IX_FinBenefitSettlement_Period_Debtor_NotCancelled");
        entity.HasIndex(x => new { x.Status, x.AccountingPeriodCode }).HasDatabaseName("IX_FinBenefitSettlement_Status_AccountingPeriodCode");
        entity.HasIndex(x => x.AccountingEventId).HasDatabaseName("IX_FinBenefitSettlement_AccountingEventId");
    }
}
