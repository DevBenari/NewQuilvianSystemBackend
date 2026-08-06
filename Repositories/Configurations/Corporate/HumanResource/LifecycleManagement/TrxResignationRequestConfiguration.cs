using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxResignationRequestConfiguration : IEntityTypeConfiguration<TrxResignationRequest>
    {
        public void Configure(EntityTypeBuilder<TrxResignationRequest> entity)
        {
            entity.ToTable("TrxResignationRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RequestDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ProposedLastWorkingDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ResignationReason).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.HandoverPlan).HasMaxLength(2000);
            entity.Property(x => x.ManagerComment).HasMaxLength(2000);
            entity.Property(x => x.RequestStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.WithdrawnAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.WithdrawalReason).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeSeparation)
                .WithMany()
                .HasForeignKey(x => x.EmployeeSeparationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason)
                .WithMany()
                .HasForeignKey(x => x.RequestReasonId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason)
                .WithMany()
                .HasForeignKey(x => x.RejectionReasonId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser)
                .WithMany()
                .HasForeignKey(x => x.RejectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RequestNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.RequestStatus, x.IsDelete });
            entity.HasIndex(x => new { x.EmployeeId, x.RequestStatus, x.IsDelete });
            entity.HasIndex(x => x.WorkflowInstanceId);
            entity.HasIndex(x => x.EmployeeSeparationId)
                .IsUnique()
                .HasFilter("\"EmployeeSeparationId\" IS NOT NULL AND \"IsDelete\" = false");
        }
    }
}
