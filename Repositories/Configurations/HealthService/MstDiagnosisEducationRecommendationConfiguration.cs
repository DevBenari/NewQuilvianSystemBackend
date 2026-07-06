using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDiagnosisEducationRecommendationConfiguration : IEntityTypeConfiguration<MstDiagnosisEducationRecommendation>
    {
        public void Configure(EntityTypeBuilder<MstDiagnosisEducationRecommendation> entity)
        {
            entity.ToTable("MstDiagnosisEducationRecommendation", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DiagnosisId)
                .IsRequired();

            entity.Property(x => x.EducationType)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .IsRequired();

            entity.Property(x => x.EducationTitle)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.EducationText)
                .HasMaxLength(2000)
                .IsRequired();
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

            entity.HasOne(x => x.Diagnosis)
                .WithMany(x => x.EducationRecommendations)
                .HasForeignKey(x => x.DiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DiagnosisId);

            entity.HasIndex(x => x.EducationTitle);

            entity.HasIndex(x => new
            {
                x.DiagnosisId,
                x.EducationType,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
