using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstLeavePolicyConfiguration : IEntityTypeConfiguration<MstLeavePolicy>
    {
        public void Configure(EntityTypeBuilder<MstLeavePolicy> entity)
        {
            entity.ToTable("MstLeavePolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.LeavePolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LeavePolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.IsFallback).HasDefaultValue(false);
            entity.Property(x => x.MinimumServiceMonths).HasDefaultValue(0);
            entity.Property(x => x.MinimumNoticeDays).HasDefaultValue(0);
            entity.Property(x => x.AllowDuringProbation).HasDefaultValue(false);
            entity.Property(x => x.AllowNegativeBalance).HasDefaultValue(false);
            entity.Property(x => x.NegativeBalanceLimitDays).HasPrecision(18, 4);
            entity.Property(x => x.AllowBackdatedRequest).HasDefaultValue(false);
            entity.Property(x => x.BackdatedLimitDays).HasDefaultValue(0);
            entity.Property(x => x.AllowFutureDatedRequest).HasDefaultValue(true);
            entity.Property(x => x.DayCalculationMethod)
                .HasMaxLength(50)
                .HasDefaultValue(LeaveValueConstants.DayCalculationMethod.ScheduledWorkDays)
                .IsRequired();
            entity.Property(x => x.ExcludeHoliday).HasDefaultValue(true);
            entity.Property(x => x.ExcludeWeeklyOff).HasDefaultValue(true);
            entity.Property(x => x.ReservationTiming)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.ReservationTiming.OnSubmit)
                .IsRequired();
            entity.Property(x => x.DeductionTiming)
                .HasMaxLength(30)
                .HasDefaultValue(LeaveValueConstants.DeductionTiming.OnApproval)
                .IsRequired();
            entity.Property(x => x.RequireAttachment).HasDefaultValue(false);
            entity.Property(x => x.RequireReplacementEmployee).HasDefaultValue(false);
            entity.Property(x => x.RequireManagerApproval).HasDefaultValue(true);
            entity.Property(x => x.RequireHrVerification).HasDefaultValue(false);
            entity.Property(x => x.ApprovalWorkflowCode).HasMaxLength(100);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date");
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeaveType)
                .WithMany(x => x.LeavePolicies)
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceType).WithMany().HasForeignKey(x => x.WorkforceTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmployeeCategory).WithMany().HasForeignKey(x => x.EmployeeCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmploymentType).WithMany().HasForeignKey(x => x.EmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EmploymentStatus).WithMany().HasForeignKey(x => x.EmploymentStatusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ContractType).WithMany().HasForeignKey(x => x.ContractTypeId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.LeavePolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.LeavePolicyName);
            entity.HasIndex(x => new { x.LeaveTypeId, x.Priority, x.IsFallback, x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.PositionId, x.WorkLocationId });
            entity.HasIndex(x => new { x.WorkforceTypeId, x.EmployeeCategoryId, x.EmploymentTypeId, x.EmploymentStatusId, x.ContractTypeId });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstLeavePolicy> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
