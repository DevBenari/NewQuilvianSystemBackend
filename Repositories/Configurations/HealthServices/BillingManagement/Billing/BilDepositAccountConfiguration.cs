using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Configurations;

public sealed class BilDepositAccountConfiguration : IEntityTypeConfiguration<BilDepositAccount>
{
    public void Configure(EntityTypeBuilder<BilDepositAccount> entity)
    {
        entity.ToTable("BilDepositAccount", "public", table =>
        {
            table.HasCheckConstraint("CK_BilDepositAccount_AvailableBalance", "\"AvailableBalance\" >= 0");
            table.HasCheckConstraint("CK_BilDepositAccount_Status", "\"Status\" IN ('ACTIVE','CLOSED')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.AvailableBalance).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
        entity.HasIndex(x => x.EncounterId).IsUnique();
        entity.HasIndex(x => x.AccountNumber).IsUnique();
        entity.HasMany(x => x.Movements).WithOne(x => x.DepositAccount)
            .HasForeignKey(x => x.DepositAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
