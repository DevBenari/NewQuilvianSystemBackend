using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class MstHrServiceCategoryConfiguration : IEntityTypeConfiguration<MstHrServiceCategory>
    {
        public void Configure(EntityTypeBuilder<MstHrServiceCategory> entity)
        {
            entity.ToTable("MstHrServiceCategory", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DefaultSlaHours).HasDefaultValue(24);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsEmployeeVisible).HasDefaultValue(true);
            entity.Property(x => x.IsManagerVisible).HasDefaultValue(true);
            entity.Property(x => x.IsConfidentialByDefault).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasIndex(x => x.ServiceCategoryCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.IsEmployeeVisible, x.IsActive, x.SortOrder });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<MstHrServiceCategory> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
