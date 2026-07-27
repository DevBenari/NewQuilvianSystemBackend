using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstTrainingCategoryConfiguration : IEntityTypeConfiguration<MstTrainingCategory>
    {
        public void Configure(EntityTypeBuilder<MstTrainingCategory> entity)
        {
            entity.ToTable("MstTrainingCategory", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TrainingCategoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TrainingCategoryName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.IsMandatoryCategory).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(500);
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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.TrainingCategoryCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.TrainingCategoryName);
            entity.HasIndex(x => new { x.IsMandatoryCategory, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.CreateDateTime, x.TrainingCategoryName });
        }
    }
}
