using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDrugCategoryConfiguration : IEntityTypeConfiguration<MstDrugCategory>
    {
        public void Configure(EntityTypeBuilder<MstDrugCategory> entity)
        {
            entity.ToTable("MstDrugCategory", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DrugCategoryCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DrugCategoryName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.DrugGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.DrugCategoryType)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .IsRequired();

            entity.Property(x => x.IsAntibiotic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNarcotic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPsychotropic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsHighAlert)
                .HasDefaultValue(false);

            entity.Property(x => x.IsChronicDiseaseDrug)
                .HasDefaultValue(false);

            entity.Property(x => x.IsVaccine)
                .HasDefaultValue(false);

            entity.Property(x => x.IsConsumable)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCoveredByInsuranceDefault)
                .HasDefaultValue(true);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.DrugCategoryCode)
                .IsUnique();

            entity.HasIndex(x => x.DrugCategoryName);

            entity.HasIndex(x => x.DrugGroupName);

            entity.HasIndex(x => x.DrugCategoryType);

            entity.HasIndex(x => new
            {
                x.IsAntibiotic,
                x.IsNarcotic,
                x.IsPsychotropic,
                x.IsHighAlert,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsCoveredByInsuranceDefault,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
