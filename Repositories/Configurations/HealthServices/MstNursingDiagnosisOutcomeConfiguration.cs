using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstNursingDiagnosisOutcomeConfiguration : IEntityTypeConfiguration<MstNursingDiagnosisOutcome>
    {
        public void Configure(EntityTypeBuilder<MstNursingDiagnosisOutcome> entity)
        {
            entity.ToTable("MstNursingDiagnosisOutcome", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.NursingDiagnosisId)
                .IsRequired();

            entity.Property(x => x.OutcomeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.OutcomeName)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(x => x.Expectation)
                .IsRequired(false);

            entity.Property(x => x.TerminologySystem)
                .HasMaxLength(50)
                .HasDefaultValue("SLKI")
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

            entity.HasIndex(x => x.NursingDiagnosisId);

            entity.HasIndex(x => x.OutcomeCode);

            entity.HasOne(x => x.NursingDiagnosis)
                .WithMany(x => x.Outcomes)
                .HasForeignKey(x => x.NursingDiagnosisId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
