using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDiagnosisDrugRecommendationConfiguration : IEntityTypeConfiguration<MstDiagnosisDrugRecommendation>
    {
        public void Configure(EntityTypeBuilder<MstDiagnosisDrugRecommendation> entity)
        {
            entity.ToTable("MstDiagnosisDrugRecommendation", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DiagnosisId)
                .IsRequired();

            entity.Property(x => x.DrugId)
                .IsRequired();

            entity.Property(x => x.RecommendationType)
                .HasMaxLength(50)
                .HasDefaultValue("Supportive")
                .IsRequired();

            entity.Property(x => x.IndicationText)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(x => x.DoseText)
                .HasMaxLength(250)
                .IsRequired(false);

            entity.Property(x => x.Route)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(x => x.Frequency)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(x => x.DurationText)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(x => x.CautionNote)
                .HasMaxLength(500)
                .IsRequired(false);
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
                .WithMany(x => x.DrugRecommendations)
                .HasForeignKey(x => x.DiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Drug)
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DiagnosisId);

            entity.HasIndex(x => x.DrugId);

            entity.HasIndex(x => new
            {
                x.DiagnosisId,
                x.RecommendationType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DrugId,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
