using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxReferenceCheckConfiguration : IEntityTypeConfiguration<TrxReferenceCheck>
    {
        public void Configure(EntityTypeBuilder<TrxReferenceCheck> builder)
        {
            builder.ToTable("TrxReferenceCheck", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ReferenceName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ReferenceCompany).HasMaxLength(200);
            builder.Property(x => x.ReferencePosition).HasMaxLength(150);
            builder.Property(x => x.RelationshipToCandidate).HasMaxLength(100);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.PhoneNumber).HasMaxLength(50);
            builder.Property(x => x.CheckStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ContactedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CheckResult).HasMaxLength(30);
            builder.Property(x => x.Feedback).HasMaxLength(2000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CheckedByUser).WithMany().HasForeignKey(x => x.CheckedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateApplicationId, x.CheckStatus, x.CheckResult });
        }
    }
}
