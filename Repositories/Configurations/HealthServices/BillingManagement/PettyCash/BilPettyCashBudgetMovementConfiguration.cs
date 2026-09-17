using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Configurations;

public sealed class BilPettyCashBudgetMovementConfiguration : IEntityTypeConfiguration<BilPettyCashBudgetMovement>
{
    public void Configure(EntityTypeBuilder<BilPettyCashBudgetMovement> entity)
    {
        entity.ToTable("BilPettyCashBudgetMovement", "public", table =>
        {
            // Empat nilai baru ditambahkan (PC-DES-019): RETURN, REVERSAL,
            // CARRY_FORWARD_OUT, CARRY_FORWARD_IN. Ketiganya belum ditulis service manapun
            // pada task fondasi ini — baru dipakai BE-BKC-054/057.
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_MovementType", "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT','RETURN','REVERSAL','CARRY_FORWARD_OUT','CARRY_FORWARD_IN')");
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_BalanceAfter", "\"BalanceAfter\" >= 0");
            // VoucherId wajib untuk DISBURSEMENT, RETURN, dan REVERSAL — ketiganya menunjuk
            // ke voucher yang uangnya bergerak. Kosong untuk sisanya (PC-DES-019). Perilaku
            // untuk DISBURSEMENT/TOP_UP/ADJUSTMENT identik dengan sebelum revisi ini.
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_VoucherId", "(\"MovementType\" IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" NOT IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NULL)");
            // Reason wajib untuk seluruh jenis KECUALI DISBURSEMENT (PC-DES-019). Perilaku
            // untuk TOP_UP/ADJUSTMENT/DISBURSEMENT identik dengan sebelum revisi ini.
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_Reason", "\"MovementType\" = 'DISBURSEMENT' OR \"Reason\" IS NOT NULL");
            // FundingSourceType hanya boleh bernilai TRANSFER atau CASH bila diisi (PC-DES-026).
            // NULL sah untuk historical records dan seluruh jenis movement selain TOP_UP.
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_FundingSourceType",
                "\"FundingSourceType\" IS NULL OR \"FundingSourceType\" IN ('TRANSFER','CASH')");
            // TransferReference hanya diisi bila FundingSourceType = TRANSFER (PC-DES-026).
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_TransferReference",
                "\"TransferReference\" IS NULL OR \"FundingSourceType\" = 'TRANSFER'");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.MovementType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
        entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500);
        entity.Property(x => x.FundingSourceType).HasMaxLength(30);
        entity.Property(x => x.TransferReference).HasMaxLength(100);
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Jaring pengaman terakhir terhadap klik ganda: satu voucher paling banyak
        // satu pengurangan saldo, bekerja walaupun kunci penasihat dan RowVersion gagal.
        entity.HasIndex(x => x.VoucherId)
            .IsUnique()
            .HasFilter("\"MovementType\" = 'DISBURSEMENT' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_BilPettyCashBudgetMovement_Voucher_Disbursement");

        // Pasangannya untuk pembalikan: satu voucher paling banyak SATU baris REVERSAL
        // (PC-DES-019, FR-BKC-101). RETURN sengaja TIDAK diberi index serupa — pengembalian
        // sisa boleh berkali-kali per voucher; batasnya ditegakkan aturan bisnis
        // (BIL-VAL-098), bukan index.
        entity.HasIndex(x => x.VoucherId)
            .IsUnique()
            .HasFilter("\"MovementType\" = 'REVERSAL' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_BilPettyCashBudgetMovement_Voucher_Reversal");

        entity.HasIndex(x => new { x.BudgetId, x.OccurredAt })
            .HasDatabaseName("IX_BilPettyCashBudgetMovement_Budget_OccurredAt");

        entity.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL")
            .HasDatabaseName("IX_BilPettyCashBudgetMovement_IdempotencyKey");

        entity.HasOne(x => x.Budget)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.BudgetId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Voucher)
            .WithMany()
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
