using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinBenefitSettlementItemConfiguration : IEntityTypeConfiguration<FinBenefitSettlementItem>
{
    public void Configure(EntityTypeBuilder<FinBenefitSettlementItem> entity)
    {
        entity.ToTable("FinBenefitSettlementItem", "public", table =>
        {
            table.HasCheckConstraint("CK_FinBenefitSettlementItem_Amount", "\"Amount\" > 0");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Amount).HasPrecision(18, 2);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Settlement)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Receivable)
            .WithMany()
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.SettlementId, x.ReceivableId }).IsUnique().HasDatabaseName("IX_FinBenefitSettlementItem_SettlementId_ReceivableId");

        // BE-FIN-092: kamus data minta "unique terfilter ReceivableId untuk induk yang Status <>
        // DIBATALKAN" — tidak dapat dibuat sebagai partial index Postgres karena Status ada di
        // tabel induk (FinBenefitSettlement), bukan di tabel ini (lihat catatan pada model). Index
        // biasa dipasang untuk performa kueri; penegakan invarian "satu kartu piutang MUST NOT
        // ditutup dua kali" menjadi tanggung jawab service pemilik (BE-FIN-099).
        entity.HasIndex(x => x.ReceivableId).HasDatabaseName("IX_FinBenefitSettlementItem_ReceivableId");
    }
}
