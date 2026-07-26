using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxBackgroundCheckConfiguration : IEntityTypeConfiguration<TrxBackgroundCheck>
    {
        public void Configure(EntityTypeBuilder<TrxBackgroundCheck> builder)
        {
            builder.ToTable("TrxBackgroundCheck", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.CheckType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ProviderName).HasMaxLength(200);
            builder.Property(x => x.ExternalReferenceNumber).HasMaxLength(200);
            builder.Property(x => x.ConsentAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CheckStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CheckResult).HasMaxLength(30);
            builder.Property(x => x.RiskLevel).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ReportFilePath).HasMaxLength(500);
            builder.Property(x => x.Findings).HasMaxLength(2000);
            builder.Property(x => x.ReviewedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReviewedByUser).WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateApplicationId, x.CheckType, x.CheckStatus });
        }
    }
}
