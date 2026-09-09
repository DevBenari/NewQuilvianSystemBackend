using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.RecurringJournal
{
    public class AccRecurringJournalTemplateLineConfiguration : IEntityTypeConfiguration<AccRecurringJournalTemplateLine>
    {
        public void Configure(EntityTypeBuilder<AccRecurringJournalTemplateLine> entity)
        {
            // Check constraint lapis kedua: tepat satu sisi yang terisi. Bentuknya sama persis
            // dengan CK_AccJournalLine_TepatSatuSisiTerisi, karena baris template memang akan
            // menjadi baris jurnal.
            //
            // Catatan pengujian: constraint ini MUSTAHIL dipenuhi di SQLite, karena EF menyimpan
            // decimal sebagai TEXT di sana (ACC-TD-001). Uji terhadap PostgreSQL sungguhan.
            entity.ToTable("AccRecurringJournalTemplateLine", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_AccRecurringJournalTemplateLine_TepatSatuSisiTerisi",
                    "(\"DebitAmount\" > 0 AND \"CreditAmount\" = 0) "
                    + "OR (\"DebitAmount\" = 0 AND \"CreditAmount\" > 0)");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TemplateId)
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

            // Cascade: baris template tidak punya makna tanpa templatenya, sama seperti baris
            // jurnal terhadap jurnalnya. Berbeda dari riwayat persetujuan yang memakai Restrict
            // karena ia bukti audit.
            entity.HasOne(x => x.Template)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // MstCostCenter milik Human Resource — dirujuk saja, MUST NOT disalin, dan boleh
            // kosong bila akunnya bukan berjenis Expense.
            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu template tidak boleh punya dua baris bernomor sama.
            entity.HasIndex(x => new { x.TemplateId, x.LineNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AccountId);

            entity.HasIndex(x => x.CostCenterId);
        }
    }
}
