using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDiagnosisConfiguration : IEntityTypeConfiguration<MstDiagnosis>
    {
        public void Configure(EntityTypeBuilder<MstDiagnosis> entity)
        {
            entity.ToTable("MstDiagnosis", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DiagnosisChapterId)
                .IsRequired(false);

            entity.Property(x => x.ParentDiagnosisId)
                .IsRequired(false);

            entity.Property(x => x.DiagnosisCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DiagnosisName)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.DiagnosisNameEnglish)
                .HasMaxLength(500);

            entity.Property(x => x.ShortName)
                .HasMaxLength(200);

            entity.Property(x => x.DiagnosisGroupName)
                .HasMaxLength(300);

            entity.Property(x => x.DiagnosisCategoryName)
                .HasMaxLength(300);

            entity.Property(x => x.DiagnosisType)
                .HasMaxLength(50)
                .HasDefaultValue("ICD10")
                .IsRequired();

            entity.Property(x => x.IcdVersion)
                .HasMaxLength(100)
                .HasDefaultValue("ICD-10")
                .IsRequired();

            entity.Property(x => x.IsSelectableForClinicalUse)
                .HasDefaultValue(true);

            entity.Property(x => x.IsBillable)
                .HasDefaultValue(true);

            entity.Property(x => x.IsPrimaryDiagnosisAllowed)
                .HasDefaultValue(true);

            entity.Property(x => x.IsSecondaryDiagnosisAllowed)
                .HasDefaultValue(true);

            entity.Property(x => x.IsChronicDisease)
                .HasDefaultValue(false);

            entity.Property(x => x.IsInfectiousDisease)
                .HasDefaultValue(false);

            entity.Property(x => x.IsExternalCause)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPregnancyRelated)
                .HasDefaultValue(false);

            entity.Property(x => x.IsMentalHealthRelated)
                .HasDefaultValue(false);

            entity.Property(x => x.IsRareDisease)
                .HasDefaultValue(false);

            entity.Property(x => x.GenderRestriction)
                .HasMaxLength(50);

            entity.Property(x => x.MinimumAgeYear)
                .IsRequired(false);

            entity.Property(x => x.MaximumAgeYear)
                .IsRequired(false);

            entity.Property(x => x.ExternalDiagnosisCode)
                .HasMaxLength(50);

            entity.Property(x => x.IntegrationCode)
                .HasMaxLength(50);

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

            entity.HasOne(x => x.DiagnosisChapter)
                .WithMany(x => x.Diagnoses)
                .HasForeignKey(x => x.DiagnosisChapterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ParentDiagnosis)
                .WithMany(x => x.ChildDiagnoses)
                .HasForeignKey(x => x.ParentDiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DiagnosisChapterId);

            entity.HasIndex(x => x.ParentDiagnosisId);

            entity.HasIndex(x => x.DiagnosisCode)
                .IsUnique();

            entity.HasIndex(x => x.DiagnosisName);

            entity.HasIndex(x => x.ShortName);

            entity.HasIndex(x => x.DiagnosisType);

            entity.HasIndex(x => x.IcdVersion);

            entity.HasIndex(x => x.ExternalDiagnosisCode)
                .HasFilter("\"ExternalDiagnosisCode\" IS NOT NULL");

            entity.HasIndex(x => x.IntegrationCode)
                .HasFilter("\"IntegrationCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.DiagnosisChapterId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ParentDiagnosisId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DiagnosisType,
                x.IcdVersion,
                x.IsSelectableForClinicalUse,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsBillable,
                x.IsPrimaryDiagnosisAllowed,
                x.IsSecondaryDiagnosisAllowed,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsChronicDisease,
                x.IsInfectiousDisease,
                x.IsExternalCause,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsPregnancyRelated,
                x.IsMentalHealthRelated,
                x.IsRareDisease,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.GenderRestriction,
                x.MinimumAgeYear,
                x.MaximumAgeYear
            });
        }
    }
}