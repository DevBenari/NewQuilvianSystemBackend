using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Vendor
{
    /// <summary>
    /// Nilai per kriteria — kamus data manajemen-vendor bagian 5. Satu baris per penilaian × kriteria; nilai 1–5
    /// dijaga CHECK (PBJ-VAL-110). Relasi ke penilaian diatur di <see cref="PrcVendorEvaluationConfiguration"/>.
    /// </summary>
    public class PrcVendorEvaluationScoreConfiguration : IEntityTypeConfiguration<PrcVendorEvaluationScore>
    {
        public void Configure(EntityTypeBuilder<PrcVendorEvaluationScore> builder)
        {
            builder.ToTable("PrcVendorEvaluationScore", "public", table =>
                table.HasCheckConstraint(
                    "CK_PrcVendorEvaluationScore_Score",
                    "\"Score\" IS NULL OR (\"Score\" >= 1 AND \"Score\" <= 5)"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CriterionName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.WeightPercent).HasColumnType("numeric(5,2)");
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => new { x.EvaluationId, x.CriterionId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.CriterionId);

            builder.HasOne(x => x.Criterion).WithMany()
                .HasForeignKey(x => x.CriterionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
