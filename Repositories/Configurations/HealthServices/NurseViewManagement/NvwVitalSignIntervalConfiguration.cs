using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.NurseViewManagement
{
    public class NvwVitalSignIntervalConfiguration : IEntityTypeConfiguration<NvwVitalSignInterval>
    {
        public void Configure(EntityTypeBuilder<NvwVitalSignInterval> builder)
        {
            builder.ToTable("NvwVitalSignInterval", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_NvwVitalSignInterval_Level",
                    "\"EwsRiskLevel\" BETWEEN 1 AND 4");
                table.HasCheckConstraint(
                    "CK_NvwVitalSignInterval_Minutes",
                    "\"IntervalMinutes\" BETWEEN 1 AND 1440");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EwsRiskLevel).HasConversion<int>().IsRequired();
            builder.Property(x => x.IntervalMinutes).IsRequired();
            builder.Property(x => x.RatificationReference).HasMaxLength(200).IsRequired();
            builder.Property(x => x.RatifiedOn).IsRequired();

            builder.HasIndex(x => x.EwsRiskLevel)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
