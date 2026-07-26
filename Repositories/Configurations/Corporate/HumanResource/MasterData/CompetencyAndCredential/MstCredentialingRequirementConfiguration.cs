using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstCredentialingRequirementConfiguration : IEntityTypeConfiguration<MstCredentialingRequirement>
    {
        public void Configure(EntityTypeBuilder<MstCredentialingRequirement> entity)
        {
            entity.ToTable("MstCredentialingRequirement", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequirementCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RequirementName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RequirementType).HasMaxLength(50).HasDefaultValue("Document").IsRequired();
            entity.Property(x => x.MinimumCompetencyLevel).HasConversion<int>().IsRequired(false);
            entity.Property(x => x.MinimumExperienceMonths).HasDefaultValue(0);
            entity.Property(x => x.RequiredQuantity).HasDefaultValue(1);
            entity.Property(x => x.ValidityMonths).IsRequired(false);
            entity.Property(x => x.IsMandatory).HasDefaultValue(true);
            entity.Property(x => x.RequiresDocument).HasDefaultValue(true);
            entity.Property(x => x.RequiresVerification).HasDefaultValue(true);
            entity.Property(x => x.RequiresExpiryDate).HasDefaultValue(false);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
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

            entity.HasOne(x => x.Profession).WithMany().HasForeignKey(x => x.ProfessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Specialization).WithMany().HasForeignKey(x => x.SpecializationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Competency).WithMany().HasForeignKey(x => x.CompetencyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TrainingCatalog).WithMany().HasForeignKey(x => x.TrainingCatalogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CertificationType).WithMany().HasForeignKey(x => x.CertificationTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LicenseType).WithMany().HasForeignKey(x => x.LicenseTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClinicalPrivilegeCatalog).WithMany().HasForeignKey(x => x.ClinicalPrivilegeCatalogId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RequirementCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.RequirementType, x.IsMandatory, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.ProfessionId, x.SpecializationId, x.PositionId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }
    }
}
