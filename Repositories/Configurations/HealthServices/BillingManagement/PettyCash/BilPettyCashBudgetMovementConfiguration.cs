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
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_MovementType", "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT')");
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_BalanceAfter", "\"BalanceAfter\" >= 0");
            // VoucherId terisi hanya untuk DISBURSEMENT.
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_VoucherId", "(\"MovementType\" = 'DISBURSEMENT' AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" <> 'DISBURSEMENT' AND \"VoucherId\" IS NULL)");
            // Reason wajib untuk TOP_UP dan ADJUSTMENT.
            table.HasCheckConstraint("CK_BilPettyCashBudgetMovement_Reason", "\"MovementType\" NOT IN ('TOP_UP','ADJUSTMENT') OR \"Reason\" IS NOT NULL");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.MovementType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
        entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500);
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
