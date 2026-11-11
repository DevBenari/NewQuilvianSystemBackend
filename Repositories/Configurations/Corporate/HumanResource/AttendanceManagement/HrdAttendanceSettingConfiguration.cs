using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceSettingConfiguration : IEntityTypeConfiguration<HrdAttendanceSetting>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceSetting> builder)
        {
            builder.ToTable("HrdAttendanceSetting", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.HospitalCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.GracePeriodMinutes).HasDefaultValue(10);
            builder.Property(x => x.MaxBackdatedCorrectionDays).HasDefaultValue(3);
            builder.Property(x => x.RequireAttachmentForCorrection).HasDefaultValue(true);
            builder.Property(x => x.AutoConvertIncompleteToAbsent).HasDefaultValue(true);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.HospitalCode);
        }
    }
}
