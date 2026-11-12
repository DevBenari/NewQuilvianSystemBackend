using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdWorkforcePlanningSettingConfiguration : IEntityTypeConfiguration<HrdWorkforcePlanningSetting>
    {
        public void Configure(EntityTypeBuilder<HrdWorkforcePlanningSetting> builder)
        {
            builder.ToTable("HrdWorkforcePlanningSetting", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.ChangeReason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.HospitalSiteId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_HrdWorkforcePlanningSetting_Site");
        }
    }
}
