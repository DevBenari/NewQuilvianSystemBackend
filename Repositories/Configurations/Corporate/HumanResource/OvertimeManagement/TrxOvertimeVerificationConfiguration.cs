using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class TrxOvertimeVerificationConfiguration : IEntityTypeConfiguration<TrxOvertimeVerification>
    {
        public void Configure(EntityTypeBuilder<TrxOvertimeVerification> entity)
        {
            entity.ToTable("TrxOvertimeVerification", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VerificationType).HasMaxLength(40).HasDefaultValue("Supervisor").IsRequired();
            entity.Property(x => x.VerificationStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.ActionAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VerifiedAmount).HasPrecision(18, 2);
            entity.Property(x => x.VerificationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.Comments).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.OvertimeRealization).WithMany(x => x.Verifications).HasForeignKey(x => x.OvertimeRealizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStep).WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifierUser).WithMany().HasForeignKey(x => x.VerifierUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifierWorkforceProfile).WithMany().HasForeignKey(x => x.VerifierWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.OvertimeRealizationId, x.VerificationOrder }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.VerifierUserId, x.VerificationStatus, x.IsDelete });
            entity.HasIndex(x => new { x.VerifierWorkforceProfileId, x.VerificationStatus, x.IsDelete });
        }
    }
}
