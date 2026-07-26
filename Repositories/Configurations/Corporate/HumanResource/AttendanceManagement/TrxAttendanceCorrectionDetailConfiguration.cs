using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class TrxAttendanceCorrectionDetailConfiguration : IEntityTypeConfiguration<TrxAttendanceCorrectionDetail>
    {
        public void Configure(EntityTypeBuilder<TrxAttendanceCorrectionDetail> builder)
        {
            builder.ToTable("TrxAttendanceCorrectionDetail", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.FieldName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DataType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.OriginalValue).HasMaxLength(2000);
            builder.Property(x => x.RequestedValue).HasMaxLength(2000);
            builder.Property(x => x.ApprovedValue).HasMaxLength(2000);
            builder.Property(x => x.DetailStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(1000);
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.AttendanceCorrectionRequest).WithMany(x => x.Details).HasForeignKey(x => x.AttendanceCorrectionRequestId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AppliedByUser).WithMany().HasForeignKey(x => x.AppliedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AttendanceCorrectionRequestId, x.FieldName }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.DetailStatus, x.IsApplied });
        }
    }
}
