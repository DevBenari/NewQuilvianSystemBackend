using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.RecurringJournal
{
    public class AccRecurringJournalTemplateConfiguration : IEntityTypeConfiguration<AccRecurringJournalTemplate>
    {
        public void Configure(EntityTypeBuilder<AccRecurringJournalTemplate> entity)
        {
            // Batas 1 sampai 28 dijaga di database, bukan hanya di service. Tanggal 29, 30, dan
            // 31 tidak ada di setiap bulan, sehingga template bertanggal itu akan terlewat pada
            // Februari tanpa menimbulkan error apa pun — kegagalan diam yang paling mahal.
            entity.ToTable("AccRecurringJournalTemplate", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_AccRecurringJournalTemplate_DayOfMonth_1_28",
                    "\"DayOfMonth\" >= 1 AND \"DayOfMonth\" <= 28");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.TemplateCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TemplateName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.JournalTypeId)
                .IsRequired();

            entity.Property(x => x.Frequency)
                .HasConversion<int>()
                .HasDefaultValue(RecurringFrequency.Bulanan)
                .IsRequired();

            entity.Property(x => x.DayOfMonth)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.StartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EndDate)
                .HasColumnType("date")
                .IsRequired(false);

            // Bawaan false: template baru wajib diperiksa sebelum diaktifkan.
            entity.Property(x => x.IsActive)
                .HasDefaultValue(false)
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
            // pola yang sama dipakai AccAccountingPeriod dan AccChartOfAccount.
            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.JournalType)
                .WithMany()
                .HasForeignKey(x => x.JournalTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu badan hukum hanya boleh punya satu template per kode (ACC-DEC-037).
            entity.HasIndex(x => new { x.LegalEntityId, x.TemplateCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.IsActive);
        }
    }
}
