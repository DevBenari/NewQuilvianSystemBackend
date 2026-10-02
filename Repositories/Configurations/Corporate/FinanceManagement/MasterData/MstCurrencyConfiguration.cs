using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.MasterData;

public sealed class MstCurrencyConfiguration : IEntityTypeConfiguration<MstCurrency>
{
    public void Configure(EntityTypeBuilder<MstCurrency> entity)
    {
        entity.ToTable("MstCurrency", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        entity.Property(x => x.CurrencyName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Symbol).HasMaxLength(10);
        entity.Property(x => x.DecimalPlaces).HasDefaultValue(2);
        entity.Property(x => x.IsBaseCurrency).HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasDefaultValue(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.CurrencyCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstCurrency_CurrencyCode");

        // FR-FIN-003: hanya satu mata uang dasar boleh aktif.
        entity.HasIndex(x => x.IsBaseCurrency)
            .IsUnique()
            .HasFilter("\"IsBaseCurrency\" = true AND \"IsDelete\" = false")
            .HasDatabaseName("IX_MstCurrency_BaseCurrency");
    }
}
