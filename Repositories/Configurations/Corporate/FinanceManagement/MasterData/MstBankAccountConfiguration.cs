using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.MasterData;

public sealed class MstBankAccountConfiguration : IEntityTypeConfiguration<MstBankAccount>
{
    public void Configure(EntityTypeBuilder<MstBankAccount> entity)
    {
        entity.ToTable("MstBankAccount", "public", table =>
        {
            table.HasCheckConstraint("CK_MstBankAccount_AccountType", "\"AccountType\" IN ('OPERATIONAL','COLLECTION','PAYMENT')");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.AccountName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.AccountType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired().HasDefaultValue("IDR");
        entity.Property(x => x.IsActive).HasDefaultValue(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // BankId merujuk MstBank existing milik Administrator/MasterData — pola identik
        // WfpBankAccountConfiguration.HasOne(x => x.Bank), bukan master Bank baru milik Finance.
        entity.HasOne(x => x.Bank)
            .WithMany()
            .HasForeignKey(x => x.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.BankId, x.AccountNumber })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstBankAccount_Bank_AccountNumber");
    }
}
