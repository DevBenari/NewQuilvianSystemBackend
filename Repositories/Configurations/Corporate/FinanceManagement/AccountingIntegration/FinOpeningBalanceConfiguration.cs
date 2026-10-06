using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.AccountingIntegration;

public sealed class FinOpeningBalanceConfiguration : IEntityTypeConfiguration<FinOpeningBalance>
{
    public void Configure(EntityTypeBuilder<FinOpeningBalance> entity)
    {
        entity.ToTable("FinOpeningBalance", "public", table =>
        {
            table.HasCheckConstraint("CK_FinOpeningBalance_Status",
                "\"Status\" IN ('DRAFT','APPROVED','LOCKED')");
            table.HasCheckConstraint("CK_FinOpeningBalance_ItemGroupZero",
                "\"BalanceGroup\" NOT IN ('PIUTANG','UTANG-SUPPLIER','UTANG-JASA-MEDIS') OR \"Amount\" = 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.BalanceGroup).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.CutoverDate).HasColumnType("date").IsRequired();
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue(FinOpeningBalanceStatuses.Draft);
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.AccountingReferenceDocument).HasMaxLength(200).IsRequired();
        entity.Property(x => x.ApprovedBy);
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.LockedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.BalanceGroup)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinOpeningBalance_BalanceGroup");

        entity.HasIndex(x => x.CutoverDate).HasDatabaseName("IX_FinOpeningBalance_CutoverDate");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinOpeningBalance_Status");
    }
}
