using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class TrxFppeCaseLogConfiguration : IEntityTypeConfiguration<TrxFppeCaseLog>
    {
        public void Configure(EntityTypeBuilder<TrxFppeCaseLog> builder)
        {
            builder.ToTable("trx_fppe_case_logs", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.SimrsPatientEpisodeId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ProcedurePerformed).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ProcedureDate).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.TechnicalSkillScore).HasDefaultValue(3);
            builder.Property(x => x.CriticalDecisionScore).HasDefaultValue(3);
            builder.Property(x => x.HasComplication).HasDefaultValue(false);
            builder.Property(x => x.ProctorFeedbackNotes).IsRequired();
            builder.Property(x => x.ObservedIndependenceLevel).HasDefaultValue(2);
            builder.Property(x => x.IsVerifiedByProctor).HasDefaultValue(true);
            builder.Property(x => x.ProctorVerifiedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.FppeEngagement)
                .WithMany()
                .HasForeignKey(x => x.FppeEngagementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.FppeEngagementId);
        }
    }
}
