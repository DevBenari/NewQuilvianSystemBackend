using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Performance
{
    public class MstKpiCatalogConfiguration : IEntityTypeConfiguration<MstKpiCatalog>
    {
        public void Configure(EntityTypeBuilder<MstKpiCatalog> entity)
        {
            entity.ToTable("MstKpiCatalog", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.KpiCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.KpiName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.KpiCategory).HasMaxLength(100).HasDefaultValue("General").IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.MeasurementUnit).HasMaxLength(100);
            entity.Property(x => x.TargetDirection).HasMaxLength(50).HasDefaultValue("HigherIsBetter").IsRequired();
            entity.Property(x => x.MeasurementFrequency).HasMaxLength(50).HasDefaultValue("Annual").IsRequired();
            entity.Property(x => x.DataSource).HasMaxLength(250);
            entity.Property(x => x.CalculationFormula).HasMaxLength(2000);
            entity.Property(x => x.DefaultTargetValue).HasPrecision(18, 4).IsRequired(false);
            entity.Property(x => x.MinimumTargetValue).HasPrecision(18, 4).IsRequired(false);
            entity.Property(x => x.MaximumTargetValue).HasPrecision(18, 4).IsRequired(false);
            entity.Property(x => x.DefaultWeight).HasPrecision(7, 2).HasDefaultValue(0m);
            entity.Property(x => x.IsQuantitative).HasDefaultValue(true);
            entity.Property(x => x.IsCascadable).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.KpiCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.KpiName);
            entity.HasIndex(x => new { x.KpiCategory, x.MeasurementFrequency, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.OrganizationUnitId, x.DepartmentId, x.PositionId });
            entity.HasIndex(x => new { x.TargetDirection, x.IsQuantitative, x.IsCascadable });
        }
    }
}
