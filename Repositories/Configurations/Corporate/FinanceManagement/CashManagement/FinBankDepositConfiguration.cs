using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.CashManagement;

public sealed class FinBankDepositConfiguration : IEntityTypeConfiguration<FinBankDeposit>
{
    public void Configure(EntityTypeBuilder<FinBankDeposit> entity)
    {
        entity.ToTable("FinBankDeposit", "public", table =>
        {
            table.HasCheckConstraint("CK_FinBankDeposit_Status",
                "\"Status\" IN ('DRAFT','POSTED','VERIFIED','CANCELLED')");
            table.HasCheckConstraint("CK_FinBankDeposit_Amount", "\"Amount\" > 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.DepositNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DepositDate).HasColumnType("date");
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.DepositSlipNumber).HasMaxLength(100);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinBankDepositStatuses.Draft);
        entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.Notes).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.DepositNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinBankDeposit_DepositNumber");

        entity.HasIndex(x => x.DepositDate)
            .HasDatabaseName("IX_FinBankDeposit_DepositDate");

        entity.HasIndex(x => x.BankAccountId)
            .HasDatabaseName("IX_FinBankDeposit_BankAccountId");

        entity.HasIndex(x => x.CashierShiftId)
            .HasDatabaseName("IX_FinBankDeposit_CashierShiftId");

        entity.HasIndex(x => x.Status)
            .HasDatabaseName("IX_FinBankDeposit_Status");

        entity.HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
