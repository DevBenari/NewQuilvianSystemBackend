using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Planning
{
    /// <summary>
    /// Komitmen pagu — kamus data bagian 3. Satu komitmen aktif per (garis keturunan pagu, dokumen
    /// sumber) dijaga index unik parsial, sehingga pemanggilan ulang tidak menggandakan komitmen.
    /// </summary>
    public class PrcBudgetCommitmentConfiguration : IEntityTypeConfiguration<PrcBudgetCommitment>
    {
        public void Configure(EntityTypeBuilder<PrcBudgetCommitment> builder)
        {
            builder.ToTable("PrcBudgetCommitment", "public", table =>
                table.HasCheckConstraint("CK_PrcBudgetCommitment_Amount", "\"Amount\" >= 0"));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceDocumentType).HasMaxLength(40).IsRequired();
            builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");

            builder.HasIndex(x => new { x.BudgetLineageKey, x.SourceDocumentType, x.SourceDocumentId })
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcBudgetCommitment_ActiveSource");
            builder.HasIndex(x => x.BudgetLineageKey);
            builder.HasIndex(x => new { x.SourceDocumentType, x.SourceDocumentId });
        }
    }
}
