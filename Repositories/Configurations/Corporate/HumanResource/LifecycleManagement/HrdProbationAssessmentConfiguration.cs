using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdProbationAssessmentConfiguration : IEntityTypeConfiguration<HrdProbationAssessment>
    {
        public void Configure(EntityTypeBuilder<HrdProbationAssessment> builder)
        {
            builder.ToTable("HrdProbationAssessment", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.AssessmentNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Result).HasConversion<int>().IsRequired();
            builder.Property(x => x.ReasonText).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.EvidenceStoragePath).HasMaxLength(500);
            builder.Property(x => x.EvidenceFileName).HasMaxLength(250);
            builder.Property(x => x.EvidenceContentType).HasMaxLength(150);
            builder.Property(x => x.AssessmentStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DecisionNote).HasMaxLength(1000);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.Probation).WithMany(p => p.Assessments).HasForeignKey(x => x.ProbationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DecidedByUser).WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ProbationId)
                .HasDatabaseName("IX_HrdProbationAssessment_ProbationId_Open")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"AssessmentStatus\" IN (1, 2)");
        }
    }
}
