using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.JournalManagement
{
    public class AccJournalConfiguration : IEntityTypeConfiguration<AccJournal>
    {
        public void Configure(EntityTypeBuilder<AccJournal> entity)
        {
            entity.ToTable("AccJournal", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.JournalNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.JournalTypeId)
                .IsRequired();

            entity.Property(x => x.AccountingPeriodId)
                .IsRequired();

            entity.Property(x => x.DocumentNumber)
                .HasMaxLength(50);

            entity.Property(x => x.DocumentDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.AccountingDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.JournalStatus)
                .HasConversion<int>()
                .HasDefaultValue(JournalStatus.Draft)
                .IsRequired();

            entity.Property(x => x.TotalDebit)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(x => x.TotalCredit)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(x => x.SubmittedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.PostedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            entity.Property(x => x.CorrectionType)
                .HasConversion<int>()
                .IsRequired(false);

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
            // pola yang sama dengan AccChartOfAccount dan AccAccountingPeriod.
            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.JournalType)
                .WithMany()
                .HasForeignKey(x => x.JournalTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AccountingPeriod)
                .WithMany()
                .HasForeignKey(x => x.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pembalikan menunjuk jurnal lain pada tabel yang sama.
            entity.HasOne(x => x.ReversalOfJournal)
                .WithMany()
                .HasForeignKey(x => x.ReversalOfJournalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nomor jurnal unik per badan hukum (ACC-DEC-037). Nomor terlewat diizinkan,
            // nomor kembar tidak (ACC-DEC-014).
            entity.HasIndex(x => new { x.LegalEntityId, x.JournalNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.JournalTypeId);

            entity.HasIndex(x => x.AccountingPeriodId);

            entity.HasIndex(x => x.AccountingDate);

            entity.HasIndex(x => x.JournalStatus);

            entity.HasIndex(x => x.ReversalOfJournalId);
        }
    }
}
