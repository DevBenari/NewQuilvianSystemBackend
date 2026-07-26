using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstWorkCalendarConfiguration : IEntityTypeConfiguration<MstWorkCalendar>
    {
        public void Configure(EntityTypeBuilder<MstWorkCalendar> entity)
        {
            entity.ToTable("MstWorkCalendar", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkCalendarCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WorkCalendarName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CalendarYear).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.EndDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.TimeZoneId).HasMaxLength(100).HasDefaultValue("Asia/Jakarta").IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkCalendarCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.HospitalSiteId, x.CalendarYear, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.StartDate, x.EndDate });
            entity.HasIndex(x => new { x.IsDefault, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstWorkCalendar> entity)
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
