using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstHolidayConfiguration : IEntityTypeConfiguration<MstHoliday>
    {
        public void Configure(EntityTypeBuilder<MstHoliday> entity)
        {
            entity.ToTable("MstHoliday", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkCalendarId).IsRequired();
            entity.Property(x => x.HolidayCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.HolidayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.StartDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.EndDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.HolidayType).HasMaxLength(50).HasDefaultValue("National").IsRequired();
            entity.Property(x => x.IsNationalHoliday).HasDefaultValue(false);
            entity.Property(x => x.IsPaidHoliday).HasDefaultValue(true);
            entity.Property(x => x.IsRecurringAnnually).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.WorkCalendar)
                .WithMany(x => x.Holidays)
                .HasForeignKey(x => x.WorkCalendarId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkCalendarId, x.HolidayCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkCalendarId, x.StartDate, x.EndDate });
            entity.HasIndex(x => new { x.HolidayType, x.IsNationalHoliday, x.IsPaidHoliday, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstHoliday> entity)
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
