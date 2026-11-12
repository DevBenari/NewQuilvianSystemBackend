using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceDeviceIncidentConfiguration : IEntityTypeConfiguration<HrdAttendanceDeviceIncident>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceDeviceIncident> builder)
        {
            builder.ToTable("HrdAttendanceDeviceIncident", "public", table =>
                table.HasCheckConstraint("CK_HrdAttendanceDeviceIncident_Range", "\"EndAt\" > \"StartAt\""));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.EndAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Active").IsRequired();
            builder.Property(x => x.CancelReason).HasMaxLength(500);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.HasOne(x => x.AttendanceDevice)
                .WithMany()
                .HasForeignKey(x => x.AttendanceDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AttendanceDeviceId, x.StartAt });
        }
    }
}
