using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstLeavePolicyConfiguration : IEntityTypeConfiguration<MstLeavePolicy>
    {
        public void Configure(EntityTypeBuilder<MstLeavePolicy> entity)
        {
            entity.ToTable("MstLeavePolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.LeaveTypeId).IsRequired();
            entity.Property(x => x.LeavePolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LeavePolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.MinimumServiceMonths).HasDefaultValue(0);
            entity.Property(x => x.MinimumNoticeDays).HasDefaultValue(0);
            entity.Property(x => x.AllowDuringProbation).HasDefaultValue(false);
            entity.Property(x => x.AllowNegativeBalance).HasDefaultValue(false);
            entity.Property(x => x.AllowBackdatedRequest).HasDefaultValue(false);
            entity.Property(x => x.BackdatedLimitDays).HasDefaultValue(0);
            entity.Property(x => x.AllowFutureDatedRequest).HasDefaultValue(true);
            entity.Property(x => x.ExcludeHoliday).HasDefaultValue(true);
            entity.Property(x => x.ExcludeWeeklyOff).HasDefaultValue(true);
            entity.Property(x => x.RequireAttachment).HasDefaultValue(false);
            entity.Property(x => x.RequireReplacementEmployee).HasDefaultValue(false);
            entity.Property(x => x.RequireManagerApproval).HasDefaultValue(true);
            entity.Property(x => x.RequireHrVerification).HasDefaultValue(false);
            entity.Property(x => x.ApprovalWorkflowCode).HasMaxLength(100);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.LeaveType)
                .WithMany(x => x.LeavePolicies)
                .HasForeignKey(x => x.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeCategory)
                .WithMany()
                .HasForeignKey(x => x.EmployeeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmploymentType)
                .WithMany()
                .HasForeignKey(x => x.EmploymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.LeavePolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.LeavePolicyName);
            entity.HasIndex(x => x.LeaveTypeId);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.LeaveTypeId, x.IsDefault, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);
        }
    }
}
