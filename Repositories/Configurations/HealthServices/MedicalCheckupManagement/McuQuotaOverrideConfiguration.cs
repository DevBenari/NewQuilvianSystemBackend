using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan pelampauan kuota — <c>data-dictionary.md</c> bagian 2.</summary>
    public class McuQuotaOverrideConfiguration : IEntityTypeConfiguration<McuQuotaOverride>
    {
        public void Configure(EntityTypeBuilder<McuQuotaOverride> builder)
        {
            builder.ToTable("McuQuotaOverride", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.DailyScheduleId);

            builder.HasOne(x => x.DailySchedule)
                .WithMany(x => x.QuotaOverrides)
                .HasForeignKey(x => x.DailyScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
