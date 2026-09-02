using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.JournalManagement
{
    public class AccJournalLineConfiguration : IEntityTypeConfiguration<AccJournalLine>
    {
        public void Configure(EntityTypeBuilder<AccJournalLine> entity)
        {
            // Check constraint lapis kedua: tepat satu sisi yang terisi. Service tetap
            // memeriksanya lebih dahulu supaya pesannya dapat dibaca pengguna; constraint ini
            // menjaga bila ada jalur tulis yang melewatkannya.
            entity.ToTable("AccJournalLine", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_AccJournalLine_TepatSatuSisiTerisi",
                    "(\"DebitAmount\" > 0 AND \"CreditAmount\" = 0) "
                    + "OR (\"DebitAmount\" = 0 AND \"CreditAmount\" > 0)");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JournalId)
                .IsRequired();

            entity.Property(x => x.LineNumber)
                .IsRequired();

            entity.Property(x => x.AccountId)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.DebitAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(x => x.CreditAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m)
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

            // Satu-satunya relasi Cascade pada modul ini. Baris jurnal tidak punya makna tanpa
            // jurnalnya, dan penghapusan jurnal hanya mungkin saat masih Draft.
            entity.HasOne(x => x.Journal)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.JournalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // MstCostCenter milik Human Resource — dirujuk saja, MUST NOT disalin, dan boleh
            // kosong. Wajib hanya bila akunnya berjenis Expense (ACC-DEC-019), dan itu aturan
            // service.
            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.JournalId, x.LineNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AccountId);

            entity.HasIndex(x => x.CostCenterId);
        }
    }
}
