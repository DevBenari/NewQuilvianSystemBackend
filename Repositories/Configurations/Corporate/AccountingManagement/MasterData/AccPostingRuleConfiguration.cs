using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.MasterData
{
    public class AccPostingRuleConfiguration : IEntityTypeConfiguration<AccPostingRule>
    {
        public void Configure(EntityTypeBuilder<AccPostingRule> entity)
        {
            entity.ToTable("AccPostingRule", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.EventTypeId)
                .IsRequired();

            entity.Property(x => x.JournalTypeId)
                .IsRequired();

            // Bawaan BuatDraft — yang lebih aman bila perlakuan lupa ditetapkan (kamus data bagian 12).
            entity.Property(x => x.Treatment)
                .HasConversion<int>()
                .HasDefaultValue(AccountingEventTreatment.BuatDraft)
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

            // MstLegalEntity dirujuk saja dan tidak disalin — WithMany() tanpa navigasi balik,
            // pola yang sama dengan AccRecurringJournalTemplate.
            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EventType)
                .WithMany()
                .HasForeignKey(x => x.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ACC-DEC-074 — jenis jurnal yang dihasilkan aturan.
            entity.HasOne(x => x.JournalType)
                .WithMany()
                .HasForeignKey(x => x.JournalTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu jenis kejadian hanya boleh punya SATU aturan aktif per badan hukum. Filter
            // IsActive memungkinkan aturan lama disimpan nonaktif sebagai riwayat; filter IsDelete
            // mengikuti konvensi unique index berfilter pada seluruh modul Accounting.
            entity.HasIndex(x => new { x.LegalEntityId, x.EventTypeId })
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false");

            entity.HasIndex(x => x.EventTypeId);

            entity.HasIndex(x => x.JournalTypeId);
        }
    }
}
