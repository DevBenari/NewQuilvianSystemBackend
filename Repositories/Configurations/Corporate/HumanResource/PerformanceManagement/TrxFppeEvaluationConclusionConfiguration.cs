using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxFppeEvaluationConclusionConfiguration : IEntityTypeConfiguration<TrxFppeEvaluationConclusion>
    {
        public void Configure(EntityTypeBuilder<TrxFppeEvaluationConclusion> builder)
        {
            builder.ToTable("trx_fppe_evaluation_conclusions", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RecommendedAction).HasMaxLength(50).HasDefaultValue("GRANT_INDEPENDENT").IsRequired();
            builder.Property(x => x.PlenaryReviewSummary).IsRequired();
            builder.Property(x => x.FinalizedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.SignatureHash).HasMaxLength(256).IsRequired();

            builder.HasOne(x => x.FppeEngagement)
                .WithMany()
                .HasForeignKey(x => x.FppeEngagementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.FppeEngagementId);
        }
    }
}
