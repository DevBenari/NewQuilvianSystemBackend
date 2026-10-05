using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.AccountingIntegration;

public sealed class FinSubledgerControlAccountMapConfiguration : IEntityTypeConfiguration<FinSubledgerControlAccountMap>
{
    public void Configure(EntityTypeBuilder<FinSubledgerControlAccountMap> entity)
    {
        entity.ToTable("FinSubledgerControlAccountMap", "public", table =>
        {
            table.HasCheckConstraint("CK_FinSubledgerControlAccountMap_BalanceGroup",
                "\"BalanceGroup\" IN ('KAS-KASIR','KAS-KECIL','PIUTANG','UTANG-SUPPLIER','UTANG-JASA-MEDIS')");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.BalanceGroup).HasMaxLength(30).IsRequired();
        entity.Property(x => x.SegmentKey).HasMaxLength(40);
        entity.Property(x => x.ControlAccountCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.Notes).HasMaxLength(300);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.BalanceGroup, x.SegmentKey })
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinSubledgerControlAccountMap_Group_Segment");

        entity.HasIndex(x => x.ControlAccountCode)
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinSubledgerControlAccountMap_ControlAccountCode");

        entity.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_FinSubledgerControlAccountMap_IsActive");
    }
}
