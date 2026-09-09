using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.MasterData
{
    public class AccAccountingConfigurationConfiguration : IEntityTypeConfiguration<AccAccountingConfiguration>
    {
        public void Configure(EntityTypeBuilder<AccAccountingConfiguration> entity)
        {
            entity.ToTable("AccAccountingConfiguration", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.RetainedEarningsAccountId)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            // MstLegalEntity dirujuk saja dan tidak disalin. WithMany() tanpa navigasi balik,
            // pola yang sama dipakai AccChartOfAccount dan AccRecurringJournalTemplate.
            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict: akun yang sudah ditetapkan sebagai laba ditahan tidak boleh ikut hilang
            // bersama barisnya, dan sebaliknya penghapusan akunnya harus tertahan selama masih
            // dipakai pengaturan ini.
            entity.HasOne(x => x.RetainedEarningsAccount)
                .WithMany()
                .HasForeignKey(x => x.RetainedEarningsAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu badan hukum hanya boleh punya SATU pengaturan yang hidup.
            //
            // Filter "IsDelete" = false dipakai di sini — berbeda dari penjaga terbit ganda pada
            // AccRecurringJournalRun yang sengaja tanpa filter. Alasannya berlawanan arah:
            // pengaturan yang sudah dihapus lunak tidak menimbulkan akibat akuntansi apa pun,
            // sehingga menahan badan hukum itu dari membuat pengaturan baru justru mengunci
            // tutup tahun tanpa alasan. Yang dijaga cukup "hanya satu yang berlaku sekarang".
            entity.HasIndex(x => x.LegalEntityId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.RetainedEarningsAccountId);
        }
    }
}
