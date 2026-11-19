using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    /// <summary>Konfigurasi tabel parameter lembur (MSL-10, migration <c>20261118100000_AddHrdOvertimeSetting</c>).</summary>
    public class HrdOvertimeSettingConfiguration : IEntityTypeConfiguration<HrdOvertimeSetting>
    {
        public void Configure(EntityTypeBuilder<HrdOvertimeSetting> builder)
        {
            builder.ToTable("HrdOvertimeSetting", "public");
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
            builder.Property(x => x.DailyMaxHoursRegular).HasColumnType("numeric(5,2)").HasDefaultValue(4m);
            builder.Property(x => x.WeeklyMaxHoursRegular).HasColumnType("numeric(5,2)").HasDefaultValue(18m);
            builder.Property(x => x.RoundingIntervalMinutes).HasDefaultValue(30);
            builder.Property(x => x.MinimumThresholdMinutes).HasDefaultValue(30);
            builder.Property(x => x.CtoValidityDays).HasDefaultValue(90);
            builder.Property(x => x.MonthlyCutoffDay).HasDefaultValue(20);
            builder.Property(x => x.BiometricDiscrepancyTolerance).HasDefaultValue(30);
            builder.Property(x => x.PostClaimCitoGraceDays).HasDefaultValue(1);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.HospitalCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
