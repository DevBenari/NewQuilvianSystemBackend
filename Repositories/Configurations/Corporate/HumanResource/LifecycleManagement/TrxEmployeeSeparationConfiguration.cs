using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxEmployeeSeparationConfiguration : IEntityTypeConfiguration<TrxEmployeeSeparation>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeSeparation> builder)
        {
            builder.ToTable("TrxEmployeeSeparation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.SeparationNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.SeparationType).HasMaxLength(50);
            builder.Property(x => x.RequestDate).HasColumnType("date");
            builder.Property(x => x.ApprovedDate).HasColumnType("date");
            builder.Property(x => x.EffectiveSeparationDate).HasColumnType("date");
            builder.Property(x => x.LastWorkingDate).HasColumnType("date");
            builder.Property(x => x.SeparationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReasonText).HasMaxLength(2000);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TerminationReason).WithMany().HasForeignKey(x => x.TerminationReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PreviousEmploymentStatus).WithMany().HasForeignKey(x => x.PreviousEmploymentStatusId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FinalEmploymentStatus).WithMany().HasForeignKey(x => x.FinalEmploymentStatusId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FinalPayrollPeriod).WithMany().HasForeignKey(x => x.FinalPayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.SeparationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveSeparationDate, x.SeparationStatus });
        }
    }
}
