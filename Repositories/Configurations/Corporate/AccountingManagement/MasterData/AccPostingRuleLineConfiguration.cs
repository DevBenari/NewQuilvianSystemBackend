using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.MasterData
{
    public class AccPostingRuleLineConfiguration : IEntityTypeConfiguration<AccPostingRuleLine>
    {
        public void Configure(EntityTypeBuilder<AccPostingRuleLine> entity)
        {
            entity.ToTable("AccPostingRuleLine", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PostingRuleId)
                .IsRequired();

            entity.Property(x => x.LineNumber)
                .IsRequired();

            entity.Property(x => x.ComponentCode)
                .HasMaxLength(50)
                .HasDefaultValue(AccPostingRuleLine.KomponenTotal)
                .IsRequired();

            entity.Property(x => x.AccountId)
                .IsRequired();

            entity.Property(x => x.Side)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

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

            // Cascade: baris aturan tidak punya makna tanpa aturannya, sama seperti baris template
            // terhadap templatenya. Satu-satunya Cascade pada ketiga tabel master aturan posting.
            entity.HasOne(x => x.PostingRule)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.PostingRuleId)
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

            // Satu aturan tidak boleh punya dua baris bernomor sama.
            entity.HasIndex(x => new { x.PostingRuleId, x.LineNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ComponentCode);

            entity.HasIndex(x => x.AccountId);

            entity.HasIndex(x => x.CostCenterId);
        }
    }
}
