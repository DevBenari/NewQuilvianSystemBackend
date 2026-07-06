using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDiagnosisProcedureRecommendationConfiguration : IEntityTypeConfiguration<MstDiagnosisProcedureRecommendation>
    {
        public void Configure(EntityTypeBuilder<MstDiagnosisProcedureRecommendation> entity)
        {
            entity.ToTable("MstDiagnosisProcedureRecommendation", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DiagnosisId)
                .IsRequired();

            entity.Property(x => x.ProcedureId)
                .IsRequired(false);

            entity.Property(x => x.RecommendationType)
                .HasMaxLength(50)
                .HasDefaultValue("Procedure")
                .IsRequired();

            entity.Property(x => x.RecommendationName)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.InstructionText)
                .HasMaxLength(1000)
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
                .WithMany(x => x.ProcedureRecommendations)
                .HasForeignKey(x => x.DiagnosisId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DiagnosisId);

            entity.HasIndex(x => x.ProcedureId);

            entity.HasIndex(x => x.RecommendationName);

            entity.HasIndex(x => new
            {
                x.DiagnosisId,
                x.RecommendationType,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
