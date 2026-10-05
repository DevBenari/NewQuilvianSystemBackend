using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.MasterData;

public sealed class MstDirectPaymentThresholdConfiguration : IEntityTypeConfiguration<MstDirectPaymentThreshold>
{
    public void Configure(EntityTypeBuilder<MstDirectPaymentThreshold> entity)
    {
        entity.ToTable("MstDirectPaymentThreshold", "public", table =>
        {
            table.HasCheckConstraint("CK_MstDirectPaymentThreshold_Amount", "\"Amount\" > 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.ChangeReason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.IsActive).HasDefaultValue(true);

        // BE-FIN-086 (FIN-DES-094): EffectiveFrom DIBUANG — ambang selalu berlaku seketika
        // (FIN-DEC-145/153). Penggantinya penanda versi, mengikuti FinOpeningBalanceConfiguration.
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // FIN-DES-086: satu baris aktif saja — tanpa baris aktif, seluruh pembayaran langsung ditolak.
        entity.HasIndex(x => x.IsActive)
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
            .HasDatabaseName("IX_MstDirectPaymentThreshold_Active");
    }
}
