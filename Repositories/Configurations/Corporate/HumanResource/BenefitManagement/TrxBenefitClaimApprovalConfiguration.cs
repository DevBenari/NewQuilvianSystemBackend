using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    public class TrxBenefitClaimApprovalConfiguration : IEntityTypeConfiguration<TrxBenefitClaimApproval>
    {
        public void Configure(EntityTypeBuilder<TrxBenefitClaimApproval> entity)
        {
            entity.ToTable("TrxBenefitClaimApproval", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.BenefitClaim)
                .WithMany(x => x.Approvals)
                .HasForeignKey(x => x.BenefitClaimId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkflowStep)
                .WithMany()
                .HasForeignKey(x => x.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RejectionReason)
                .WithMany()
                .HasForeignKey(x => x.RejectionReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApproverUser)
                .WithMany()
                .HasForeignKey(x => x.ApproverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BenefitClaimId, x.StepOrder })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.ApproverUserId, x.ApprovalStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxBenefitClaimApproval> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
