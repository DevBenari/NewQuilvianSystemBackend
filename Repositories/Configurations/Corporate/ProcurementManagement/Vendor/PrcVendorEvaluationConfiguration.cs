using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Vendor
{
    /// <summary>
    /// Penilaian vendor — kamus data manajemen-vendor bagian 5. Satu penilaian asli (bukan revisi) berstatus
    /// <c>DRAFT</c>/<c>FINAL</c> per vendor × kategori × periode dijaga index unik parsial (PBJ-VAL-109,
    /// PBJ-DEC-059); keunikan rantai revisi dijaga service. Nilai akhir 1,00–5,00 dijaga CHECK.
    /// </summary>
    public class PrcVendorEvaluationConfiguration : IEntityTypeConfiguration<PrcVendorEvaluation>
    {
        public void Configure(EntityTypeBuilder<PrcVendorEvaluation> builder)
        {
            builder.ToTable("PrcVendorEvaluation", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_PrcVendorEvaluation_FinalScore",
                    "\"FinalScore\" IS NULL OR (\"FinalScore\" >= 1 AND \"FinalScore\" <= 5)");
                table.HasCheckConstraint(
                    "CK_PrcVendorEvaluation_PeriodNo",
                    "\"PeriodNo\" >= 1 AND \"PeriodNo\" <= 4");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PeriodType).HasMaxLength(10).IsRequired();
            builder.Property(x => x.SourceType).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(12).IsRequired();
            builder.Property(x => x.FinalScore).HasColumnType("numeric(4,2)");
            builder.Property(x => x.Recommendation).HasMaxLength(10);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.RevisionReason).HasMaxLength(1000);

            // PBJ-VAL-090: dua orang menyunting/memfinalkan penilaian yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => new { x.VendorProfileId, x.ItemCategoryId, x.PeriodYear, x.PeriodType, x.PeriodNo })
                .IsUnique()
                .HasFilter("\"Status\" IN ('DRAFT', 'FINAL') AND \"PreviousEvaluationId\" IS NULL AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcVendorEvaluation_OriginalPerPeriod");
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.ItemCategoryId);
            builder.HasIndex(x => x.PreviousEvaluationId);

            builder.HasOne(x => x.VendorProfile).WithMany()
                .HasForeignKey(x => x.VendorProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ItemCategory).WithMany()
                .HasForeignKey(x => x.ItemCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PreviousEvaluation).WithMany()
                .HasForeignKey(x => x.PreviousEvaluationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Scores).WithOne(x => x.Evaluation)
                .HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
