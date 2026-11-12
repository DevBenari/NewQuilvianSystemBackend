using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdEmploymentLifecycleSettingConfiguration : IEntityTypeConfiguration<HrdEmploymentLifecycleSetting>
    {
        public void Configure(EntityTypeBuilder<HrdEmploymentLifecycleSetting> builder)
        {
            builder.ToTable("HrdEmploymentLifecycleSetting", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ContractReminderDays).HasDefaultValue(60);
            builder.Property(x => x.ContractEscalationDays).HasDefaultValue(14);
            builder.Property(x => x.ProbationReminderDays).HasDefaultValue(14);
            builder.Property(x => x.LateStartFlagDays).HasDefaultValue(3);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.HospitalSiteId)
                .HasDatabaseName("IX_HrdEmploymentLifecycleSetting_HospitalSiteId")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
