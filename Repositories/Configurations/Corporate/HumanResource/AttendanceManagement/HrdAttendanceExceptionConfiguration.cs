using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceExceptionConfiguration : IEntityTypeConfiguration<HrdAttendanceException>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceException> builder)
        {
            builder.ToTable("HrdAttendanceException", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ExceptionCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ExceptionType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Severity).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ExceptionStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.DetectedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.ExpectedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActualAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DetectionRule).HasMaxLength(100);
            builder.Property(x => x.Message).HasMaxLength(1000);
            builder.Property(x => x.ResolvedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ResolutionNote).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AttendanceDaily).WithMany(x => x.Exceptions).HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CorrectionRequest).WithMany(x => x.Exceptions).HasForeignKey(x => x.CorrectionRequestId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ResolvedByUser).WithMany().HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AttendanceDailyId, x.ExceptionCode }).IsUnique().HasFilter("\"IsDelete\" = false AND \"ExceptionStatus\" <> 'Closed'");
            builder.HasIndex(x => new { x.ExceptionStatus, x.Severity, x.IsPayrollBlocking });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.DetectedAt });
        }
    }
}
