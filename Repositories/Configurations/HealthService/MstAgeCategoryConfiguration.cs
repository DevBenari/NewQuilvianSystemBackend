using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstAgeCategoryConfiguration : IEntityTypeConfiguration<MstAgeCategory>
    {
        public void Configure(EntityTypeBuilder<MstAgeCategory> entity)
        {
            entity.ToTable("MstAgeCategory", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AgeCategoryCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.AgeCategoryName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.AgeCategoryShortName)
                .HasMaxLength(75);

            entity.Property(x => x.MinAgeDays)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.MaxAgeDays)
                .IsRequired(false);

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSelectableInKiosk)
                .HasDefaultValue(true);

            entity.Property(x => x.IsSelectableInRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.IsUsedForClinicalRule)
                .HasDefaultValue(true);

            entity.Property(x => x.StandardReference)
                .HasMaxLength(250);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.UpdateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.AgeCategoryCode)
                .IsUnique();

            entity.HasIndex(x => x.AgeCategoryName);

            entity.HasIndex(x => new
            {
                x.MinAgeDays,
                x.MaxAgeDays,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsDefault,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsSelectableInKiosk,
                x.IsSelectableInRegistration,
                x.IsUsedForClinicalRule,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
