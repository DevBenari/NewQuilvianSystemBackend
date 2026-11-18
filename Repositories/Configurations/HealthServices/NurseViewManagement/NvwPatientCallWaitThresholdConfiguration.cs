using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.NurseViewManagement
{
    public class NvwPatientCallWaitThresholdConfiguration : IEntityTypeConfiguration<NvwPatientCallWaitThreshold>
    {
        public void Configure(EntityTypeBuilder<NvwPatientCallWaitThreshold> builder)
        {
            builder.ToTable("NvwPatientCallWaitThreshold", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_NvwPatientCallWaitThreshold_Order",
                    "\"WarningAfterMinutes\" BETWEEN 1 AND 240 AND \"CriticalAfterMinutes\" > \"WarningAfterMinutes\" AND \"CriticalAfterMinutes\" <= 480");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ScopeCode)
                .HasMaxLength(20)
                .HasDefaultValue(NvwPatientCallWaitThreshold.GlobalScopeCode)
                .IsRequired();
            builder.Property(x => x.WarningAfterMinutes).IsRequired();
            builder.Property(x => x.CriticalAfterMinutes).IsRequired();
            builder.Property(x => x.RatificationReference).HasMaxLength(200).IsRequired();
            builder.Property(x => x.RatifiedOn).IsRequired();

            builder.HasIndex(x => x.ScopeCode, "IX_NvwPatientCallWaitThreshold_ScopeCode_Active")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
