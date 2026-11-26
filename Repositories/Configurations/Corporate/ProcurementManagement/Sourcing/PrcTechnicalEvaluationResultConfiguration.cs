using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Hasil evaluasi teknis — kamus data bagian 4. Satu hasil per baris penawaran × kriteria × evaluator; FAIL wajib alasan
    /// (PBJ-VAL-136).
    /// </summary>
    public class PrcTechnicalEvaluationResultConfiguration : IEntityTypeConfiguration<PrcTechnicalEvaluationResult>
    {
        public void Configure(EntityTypeBuilder<PrcTechnicalEvaluationResult> builder)
        {
            builder.ToTable("PrcTechnicalEvaluationResult", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcTechnicalEvaluationResult_Result", "\"Result\" IN ('PASS', 'FAIL')");
                table.HasCheckConstraint("CK_PrcTechnicalEvaluationResult_FailReason", "\"Result\" <> 'FAIL' OR length(btrim(coalesce(\"Reason\", ''))) > 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Result).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500);

            builder.HasIndex(x => new { x.QuotationLineId, x.CriterionId, x.EvaluatorUserId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcTechnicalEvaluationResult_LineCriterionEvaluator");
            builder.HasIndex(x => x.CriterionId);
            builder.HasIndex(x => x.EvaluatorUserId);

            builder.HasOne(x => x.QuotationLine).WithMany()
                .HasForeignKey(x => x.QuotationLineId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Criterion).WithMany()
                .HasForeignKey(x => x.CriterionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SourceResult).WithMany()
                .HasForeignKey(x => x.SourceResultId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
