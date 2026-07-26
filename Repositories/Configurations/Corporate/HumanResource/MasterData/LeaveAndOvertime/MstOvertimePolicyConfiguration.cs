using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.LeaveAndOvertime
{
    public class MstOvertimePolicyConfiguration : IEntityTypeConfiguration<MstOvertimePolicy>
    {
        public void Configure(EntityTypeBuilder<MstOvertimePolicy> entity)
        {
            entity.ToTable("MstOvertimePolicy", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OvertimePolicyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OvertimePolicyName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RequirePreApproval).HasDefaultValue(true);
            entity.Property(x => x.RequirePostVerification).HasDefaultValue(true);
            entity.Property(x => x.RequireAttendanceMatch).HasDefaultValue(true);
            entity.Property(x => x.MinimumOvertimeMinutes).HasDefaultValue(30);
            entity.Property(x => x.OvertimeThresholdMinutes).HasDefaultValue(0);
            entity.Property(x => x.RoundingIntervalMinutes).HasDefaultValue(30);
            entity.Property(x => x.RoundingMethod).HasMaxLength(50).HasDefaultValue("Down").IsRequired();
            entity.Property(x => x.DeductBreakMinutes).HasDefaultValue(false);
            entity.Property(x => x.BreakDeductionMinutes).HasDefaultValue(0);
            entity.Property(x => x.AllowBeforeShift).HasDefaultValue(false);
            entity.Property(x => x.AllowAfterShift).HasDefaultValue(true);
            entity.Property(x => x.AllowRestDay).HasDefaultValue(true);
            entity.Property(x => x.AllowHoliday).HasDefaultValue(true);
            entity.Property(x => x.AllowDuringLeave).HasDefaultValue(false);
            entity.Property(x => x.AttendanceToleranceMinutes).HasDefaultValue(15);
            entity.Property(x => x.ApprovalWorkflowCode).HasMaxLength(100);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

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

            entity.HasIndex(x => x.OvertimePolicyCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.OvertimePolicyName);
            entity.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId });
            entity.HasIndex(x => new { x.EmployeeCategoryId, x.EmploymentTypeId });
            entity.HasIndex(x => new { x.IsDefault, x.RequirePreApproval, x.RequirePostVerification, x.IsActive, x.IsDelete });
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
