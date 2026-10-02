using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.PettyCash;

public sealed class FinPettyCashBudgetMovementConfiguration : IEntityTypeConfiguration<FinPettyCashBudgetMovement>
{
    public void Configure(EntityTypeBuilder<FinPettyCashBudgetMovement> entity)
    {
        entity.ToTable("FinPettyCashBudgetMovement", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPettyCashBudgetMovement_MovementType", "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT','RETURN','REVERSAL','CARRY_FORWARD_OUT','CARRY_FORWARD_IN')");
            table.HasCheckConstraint("CK_FinPettyCashBudgetMovement_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_FinPettyCashBudgetMovement_BalanceAfter", "\"BalanceAfter\" >= 0");
            table.HasCheckConstraint("CK_FinPettyCashBudgetMovement_VoucherId", "(\"MovementType\" IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" NOT IN ('DISBURSEMENT','RETURN','REVERSAL') AND \"VoucherId\" IS NULL)");
            table.HasCheckConstraint("CK_FinPettyCashBudgetMovement_Reason", "\"MovementType\" = 'DISBURSEMENT' OR \"Reason\" IS NOT NULL");
            table.HasCheckConstraint("CK_FinPettyCashBudgetMovement_FundingSourceType",
                "\"FundingSourceType\" IS NULL OR \"FundingSourceType\" IN ('TRANSFER','CASH')");
            table.HasCheckConstraint("CK_FinPettyCashBudgetMovement_TransferReference",
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

        entity.HasIndex(x => x.VoucherId)
            .IsUnique()
            .HasFilter("\"MovementType\" = 'DISBURSEMENT' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinPettyCashBudgetMovement_Voucher_Disbursement");

        entity.HasIndex(x => x.VoucherId)
            .IsUnique()
            .HasFilter("\"MovementType\" = 'REVERSAL' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinPettyCashBudgetMovement_Voucher_Reversal");

        entity.HasIndex(x => new { x.BudgetId, x.OccurredAt })
            .HasDatabaseName("IX_FinPettyCashBudgetMovement_Budget_OccurredAt");

        entity.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL")
            .HasDatabaseName("IX_FinPettyCashBudgetMovement_IdempotencyKey");

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

