using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class MstOppeMetricCatalogConfiguration : IEntityTypeConfiguration<MstOppeMetricCatalog>
    {
        public void Configure(EntityTypeBuilder<MstOppeMetricCatalog> builder)
        {
            builder.ToTable("MstOppeMetricCatalog", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.MetricCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.MetricName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.CompetencyDomain).HasMaxLength(50).IsRequired();
            builder.Property(x => x.SpecialtyGroup).HasMaxLength(50).HasDefaultValue("ALL").IsRequired();
            builder.Property(x => x.DataSourceType).HasMaxLength(30).HasDefaultValue("SIMRS_VOLUME").IsRequired();
            builder.Property(x => x.TargetStandardValue).HasColumnType("decimal(8,2)").HasDefaultValue(85.00m);
            builder.Property(x => x.MetricWeightPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(10.00m);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.MetricCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
