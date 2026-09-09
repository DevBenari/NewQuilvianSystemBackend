using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// Bentuk tabel butir masalah keperawatan — <c>BE-RWI-059</c>.
    /// </summary>
    public class CliNursingCarePlanItemConfiguration : IEntityTypeConfiguration<CliNursingCarePlanItem>
    {
        public void Configure(EntityTypeBuilder<CliNursingCarePlanItem> entity)
        {
            entity.ToTable("CliNursingCarePlanItem", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CarePlanId)
                .IsRequired();

            // Tanpa foreign key: katalog terminologi keperawatan belum diputuskan (OQ-RI-011),
            // sehingga tabel tujuannya memang belum ada.
            entity.Property(x => x.NursingDiagnosisId)
                .IsRequired(false);

            entity.Property(x => x.SourceAssessmentId)
                .IsRequired(false);

            entity.Property(x => x.ProblemStatement)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.GoalStatement)
                .HasMaxLength(500);

            entity.Property(x => x.PlannedIntervention);

            entity.Property(x => x.EvaluationNote);

            entity.Property(x => x.LastEvaluatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ItemStatus)
                .HasConversion<int>()
                .HasDefaultValue(NursingCarePlanItemStatus.Active)
                .IsRequired();

            entity.Property(x => x.ResolvedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CloseReason)
                .HasMaxLength(500);

            entity.Property(x => x.VersionNumber)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.AuthoredByEmployeeId)
                .IsRequired();

            entity.Property(x => x.AuthoredAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

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

            // SetNull, bukan Restrict. Butir asuhan tidak boleh ikut hilang hanya karena
            // pengkajian asalnya dihapus lunak - AC-CAP013-01 hanya meminta jejaknya, bukan
            // ketergantungan hidup-mati.
            entity.HasOne(x => x.SourceAssessment)
                .WithMany()
                .HasForeignKey(x => x.SourceAssessmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.AuthoredByEmployee)
                .WithMany()
                .HasForeignKey(x => x.AuthoredByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CarePlanId);

            entity.HasIndex(x => x.ItemStatus);

            entity.HasIndex(x => x.SourceAssessmentId);
        }
    }
}
