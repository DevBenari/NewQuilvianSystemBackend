using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.RecurringJournal
{
    public class AccRecurringJournalRunConfiguration : IEntityTypeConfiguration<AccRecurringJournalRun>
    {
        public void Configure(EntityTypeBuilder<AccRecurringJournalRun> entity)
        {
            entity.ToTable("AccRecurringJournalRun", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TemplateId)
                .IsRequired();

            entity.Property(x => x.AccountingPeriodId)
                .IsRequired();

            entity.Property(x => x.JournalId)
                .IsRequired();

            entity.Property(x => x.GeneratedAt)
                .HasColumnType("timestamp with time zone")
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

            // Restrict pada ketiganya. Riwayat penerbitan adalah bukti bahwa sebuah periode sudah
            // pernah diterbitkan, dan bukti itu tidak boleh ikut terhapus bersama apa pun.
            entity.HasOne(x => x.Template)
                .WithMany(x => x.Runs)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AccountingPeriod)
                .WithMany()
                .HasForeignKey(x => x.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Journal)
                .WithMany()
                .HasForeignKey(x => x.JournalId)
                .OnDelete(DeleteBehavior.Restrict);

            // PENJAGA TERBIT GANDA. Satu template hanya boleh terbit sekali per periode.
            //
            // Sengaja TANPA filter IsDelete, berbeda dari unique index lain di modul ini:
            // menghapus lunak catatan penerbitan lalu menerbitkan ulang akan menghasilkan jurnal
            // kedua untuk bulan yang sama — persis yang dijaga index ini. Bila sebuah penerbitan
            // memang keliru, jurnalnya dibalik lewat pembalikan jurnal, bukan dengan menghapus
            // jejak penerbitannya.
            entity.HasIndex(x => new { x.TemplateId, x.AccountingPeriodId })
                .IsUnique();

            entity.HasIndex(x => x.JournalId);
        }
    }
}
