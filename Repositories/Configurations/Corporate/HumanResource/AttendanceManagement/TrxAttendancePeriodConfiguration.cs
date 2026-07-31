using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class TrxAttendancePeriodConfiguration
        : IEntityTypeConfiguration<TrxAttendancePeriod>
    {
        public void Configure(EntityTypeBuilder<TrxAttendancePeriod> builder)
        {
            builder.ToTable("TrxAttendancePeriod", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.PeriodCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PeriodName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.PeriodStatus)
                .HasMaxLength(30)
                .HasDefaultValue(AttendanceValueConstants.AttendancePeriodStatus.Open)
                .IsRequired();
            builder.Property(x => x.RequirePayrollHandoff).HasDefaultValue(true);
            builder.Property(x => x.ScheduledCloseAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.LastValidatedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ValidationSnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CloseReason).HasMaxLength(1000);
            builder.Property(x => x.ReopenedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReopenReason).HasMaxLength(1000);
            builder.Property(x => x.ReopenCount).HasDefaultValue(0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LastProcessingRun).WithMany().HasForeignKey(x => x.LastProcessingRunId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ClosedByUser).WithMany().HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReopenedByUser).WithMany().HasForeignKey(x => x.ReopenedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PeriodCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.StartDate, x.EndDate, x.PeriodStatus });
            builder.HasIndex(x => new { x.LegalEntityId, x.HospitalSiteId, x.OrganizationUnitId, x.DepartmentId, x.StartDate, x.EndDate });
            builder.HasIndex(x => new { x.PeriodStatus, x.ScheduledCloseAt, x.IsActive, x.IsDelete });
        }
    }
}
