using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDoctorScheduleConfiguration : IEntityTypeConfiguration<MstDoctorSchedule>
    {
        public void Configure(EntityTypeBuilder<MstDoctorSchedule> entity)
        {
            entity.ToTable("MstDoctorSchedule", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ScheduleCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ScheduleName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ScheduleType)
                .HasConversion<int>()
                .HasDefaultValue(DoctorScheduleType.WeeklyRecurring)
                .IsRequired();

            entity.Property(x => x.DoctorId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.ClinicId)
                .IsRequired();

            entity.Property(x => x.RoomId)
                .IsRequired(false);

            entity.Property(x => x.PracticeDay)
                .HasConversion<int>()
                .HasDefaultValue(DayOfWeek.Monday)
                .IsRequired();

            entity.Property(x => x.PracticeDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.StartTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            entity.Property(x => x.EndTime)
                .HasColumnType("time without time zone")
                .IsRequired();

            entity.Property(x => x.IsOvernight)
                .HasDefaultValue(false);

            entity.Property(x => x.SessionName)
                .HasMaxLength(100);

            entity.Property(x => x.PracticeLocation)
                .HasMaxLength(100);

            entity.Property(x => x.RoomName)
                .HasMaxLength(50);

            entity.Property(x => x.MaxPatientQuota)
                .HasDefaultValue(0);

            entity.Property(x => x.MaxAppointmentQuota)
                .HasDefaultValue(0);

            entity.Property(x => x.MaxWalkInQuota)
                .HasDefaultValue(0);

            entity.Property(x => x.EstimatedServiceMinutes)
                .HasDefaultValue(15);

            entity.Property(x => x.IsAllowWalkIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowAppointment)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowKioskRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.IsTelemedicineAvailable)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSubstituteSchedule)
                .HasDefaultValue(false);

            entity.Property(x => x.SubstituteDoctorId)
                .IsRequired(false);

            entity.Property(x => x.ScheduleStatus)
                .HasConversion<int>()
                .HasDefaultValue(DoctorScheduleStatus.Active)
                .IsRequired();

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubstituteDoctor)
                .WithMany()
                .HasForeignKey(x => x.SubstituteDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ScheduleCode)
                .IsUnique();

            entity.HasIndex(x => x.ScheduleName);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.SubstituteDoctorId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.RoomId);

            entity.HasIndex(x => x.PracticeDay);

            entity.HasIndex(x => x.PracticeDate);

            entity.HasIndex(x => x.ScheduleType);

            entity.HasIndex(x => x.ScheduleStatus);

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.ServiceUnitId,
                x.ClinicId,
                x.PracticeDay,
                x.StartTime,
                x.EndTime
            })
            .IsUnique()
            .HasFilter("\"PracticeDate\" IS NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.ServiceUnitId,
                x.ClinicId,
                x.PracticeDate,
                x.StartTime,
                x.EndTime
            })
            .IsUnique()
            .HasFilter("\"PracticeDate\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.PracticeDay,
                x.ScheduleStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.PracticeDay,
                x.ScheduleStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.PracticeDate,
                x.ScheduleStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAllowWalkIn,
                x.IsAllowAppointment,
                x.IsAllowKioskRegistration,
                x.IsTelemedicineAvailable,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
