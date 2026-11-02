using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan jadwal harian MCU — <c>data-dictionary.md</c> bagian 2.</summary>
    /// <remarks>Satu baris per tanggal; <c>Version</c> menjaga pakai/lepas kuota yang serentak.</remarks>
    public class McuDailyScheduleConfiguration : IEntityTypeConfiguration<McuDailySchedule>
    {
        public void Configure(EntityTypeBuilder<McuDailySchedule> builder)
        {
            builder.ToTable("McuDailySchedule", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsOpen).HasDefaultValue(true).ValueGeneratedNever();
            builder.Property(x => x.UsedQuota).HasDefaultValue(0);
            builder.Property(x => x.ApprovedOverrideQuota).HasDefaultValue(0);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.ScheduleDate)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.DutyMcuDoctorId);

            builder.HasOne(x => x.DutyMcuDoctor)
                .WithMany()
                .HasForeignKey(x => x.DutyMcuDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
