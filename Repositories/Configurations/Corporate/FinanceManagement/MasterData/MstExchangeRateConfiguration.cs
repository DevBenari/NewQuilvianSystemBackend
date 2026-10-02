using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.MasterData;

public sealed class MstExchangeRateConfiguration : IEntityTypeConfiguration<MstExchangeRate>
{
    public void Configure(EntityTypeBuilder<MstExchangeRate> entity)
    {
        entity.ToTable("MstExchangeRate", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.RateDate).HasColumnType("date").IsRequired();
        entity.Property(x => x.BuyRate).HasPrecision(18, 2);
        entity.Property(x => x.SellRate).HasPrecision(18, 2);
        entity.Property(x => x.MiddleRate).HasPrecision(18, 2);
        entity.Property(x => x.Source).HasMaxLength(100);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.CurrencyId, x.RateDate })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstExchangeRate_Currency_RateDate");
    }
}
