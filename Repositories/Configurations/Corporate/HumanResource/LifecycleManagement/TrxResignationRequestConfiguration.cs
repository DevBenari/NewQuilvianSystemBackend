using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxResignationRequestConfiguration : IEntityTypeConfiguration<TrxResignationRequest>
    {
        public void Configure(EntityTypeBuilder<TrxResignationRequest> builder)
        {
            builder.ToTable("TrxResignationRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequestDate).HasColumnType("date");
            builder.Property(x => x.ProposedLastWorkingDate).HasColumnType("date");
            builder.Property(x => x.ResignationReason).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.HandoverPlan).HasMaxLength(2000);
            builder.Property(x => x.ManagerComment).HasMaxLength(2000);
            builder.Property(x => x.RequestStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.WithdrawnAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.WithdrawalReason).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeSeparation).WithMany().HasForeignKey(x => x.EmployeeSeparationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.RequestStatus, x.ProposedLastWorkingDate });
        }
    }
}
