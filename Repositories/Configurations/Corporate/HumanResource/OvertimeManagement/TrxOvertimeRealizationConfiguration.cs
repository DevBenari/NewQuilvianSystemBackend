using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class TrxOvertimeRealizationConfiguration : IEntityTypeConfiguration<TrxOvertimeRealization>
    {
        public void Configure(EntityTypeBuilder<TrxOvertimeRealization> entity)
        {
            entity.ToTable("TrxOvertimeRealization", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RealizationNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ActualStartDate).HasColumnType("date");
            entity.Property(x => x.ActualEndDate).HasColumnType("date");
            entity.Property(x => x.ActualStartAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ActualEndAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CalculatedAmount).HasPrecision(18, 2);
            entity.Property(x => x.VerifiedAmount).HasPrecision(18, 2);
            entity.Property(x => x.PostedAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.RealizationNotes).HasMaxLength(2000);
            entity.Property(x => x.EvidenceSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.CalculationResultJson).HasColumnType("jsonb");
            entity.Property(x => x.RealizationStatus).HasMaxLength(40).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedToPayrollAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.OvertimeRequest).WithMany(x => x.Realizations).HasForeignKey(x => x.OvertimeRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedToPayrollByUser).WithMany().HasForeignKey(x => x.PostedToPayrollByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RealizationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.OvertimeRequestId, x.RealizationVersion }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.ActualStartDate, x.RealizationStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPeriodId, x.IsPayrollPosted, x.IsDelete });
        }
    }
}
