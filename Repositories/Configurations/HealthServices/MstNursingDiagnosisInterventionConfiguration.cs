using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstNursingDiagnosisInterventionConfiguration : IEntityTypeConfiguration<MstNursingDiagnosisIntervention>
    {
        public void Configure(EntityTypeBuilder<MstNursingDiagnosisIntervention> entity)
        {
            entity.ToTable("MstNursingDiagnosisIntervention", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.NursingDiagnosisId)
                .IsRequired();

            entity.Property(x => x.PillarType)
                .IsRequired();

            entity.Property(x => x.InterventionCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.InterventionName)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(x => x.ActionDescription)
                .IsRequired();

            entity.Property(x => x.TerminologySystem)
                .HasMaxLength(50)
                .HasDefaultValue("SIKI")
                .IsRequired();

            entity.Property(x => x.IsDefaultRecommendation)
                .HasDefaultValue(true);

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

            entity.HasIndex(x => x.NursingDiagnosisId);

            entity.HasIndex(x => x.PillarType);

            entity.HasOne(x => x.NursingDiagnosis)
                .WithMany(x => x.Interventions)
                .HasForeignKey(x => x.NursingDiagnosisId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
