using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class HrdSchedulingSettingConfiguration : IEntityTypeConfiguration<HrdSchedulingSetting>
    {
        public void Configure(EntityTypeBuilder<HrdSchedulingSetting> entity)
        {
            entity.ToTable("HrdSchedulingSettings", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.MinRestHoursBetweenShifts).HasDefaultValue(11).IsRequired();
            entity.Property(x => x.MaxConsecutiveWorkDays).HasDefaultValue(6).IsRequired();
            entity.Property(x => x.MaxConsecutiveNightShifts).HasDefaultValue(2).IsRequired();
            entity.Property(x => x.RetroactiveCorrectionWindowDays).HasDefaultValue(3).IsRequired();
            entity.Property(x => x.RestHoursEscalatedToBlock).HasDefaultValue(false).IsRequired();
            entity.Property(x => x.ConsecutiveDaysEscalatedToBlock).HasDefaultValue(false).IsRequired();
            entity.Property(x => x.NightShiftsEscalatedToBlock).HasDefaultValue(false).IsRequired();
            entity.Property(x => x.RowVersion).IsConcurrencyToken();

            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.HospitalSiteId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
