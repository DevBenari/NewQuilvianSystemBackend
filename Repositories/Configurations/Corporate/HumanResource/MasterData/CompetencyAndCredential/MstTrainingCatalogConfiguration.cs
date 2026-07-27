using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstTrainingCatalogConfiguration : IEntityTypeConfiguration<MstTrainingCatalog>
    {
        public void Configure(EntityTypeBuilder<MstTrainingCatalog> entity)
        {
            entity.ToTable("MstTrainingCatalog", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TrainingCategoryId).IsRequired();
            entity.Property(x => x.TrainingCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TrainingName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TrainingType).HasMaxLength(50).HasDefaultValue("Internal").IsRequired();
            entity.Property(x => x.DeliveryMethod).HasMaxLength(50).HasDefaultValue("Classroom").IsRequired();
            entity.Property(x => x.DefaultProviderName).HasMaxLength(200);
            entity.Property(x => x.DurationHours).HasPrecision(8, 2).HasDefaultValue(0m);
            entity.Property(x => x.ValidityMonths).IsRequired(false);
            entity.Property(x => x.IsMandatory).HasDefaultValue(false);
            entity.Property(x => x.RequiresAssessment).HasDefaultValue(false);
            entity.Property(x => x.MinimumPassingScore).HasPrecision(5, 2).IsRequired(false);
            entity.Property(x => x.IssuesCertificate).HasDefaultValue(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
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

            entity.HasOne(x => x.TrainingCategory)
                .WithMany(x => x.TrainingCatalogs)
                .HasForeignKey(x => x.TrainingCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CertificationType)
                .WithMany()
                .HasForeignKey(x => x.CertificationTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.TrainingCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.TrainingName);
            entity.HasIndex(x => new { x.TrainingCategoryId, x.IsMandatory, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.TrainingType, x.DeliveryMethod, x.IsActive, x.IsDelete });
        }
    }
}
