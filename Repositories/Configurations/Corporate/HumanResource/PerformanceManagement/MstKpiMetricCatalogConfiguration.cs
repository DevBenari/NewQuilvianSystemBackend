using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class MstKpiMetricCatalogConfiguration : IEntityTypeConfiguration<MstKpiMetricCatalog>
    {
        public void Configure(EntityTypeBuilder<MstKpiMetricCatalog> entity)
        {
            entity.ToTable("mst_kpi_metric_catalogs", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.MetricCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MetricName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ComponentType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TargetProfessionGroup).HasMaxLength(50).IsRequired();
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(30);
            entity.Property(x => x.TargetPolarity).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DefaultTargetValue).HasColumnType("decimal(15,2)");
            entity.Property(x => x.ScoringRubricJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasIndex(x => x.MetricCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ComponentType);
            entity.HasIndex(x => x.IsActive);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<MstKpiMetricCatalog> entity)
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
