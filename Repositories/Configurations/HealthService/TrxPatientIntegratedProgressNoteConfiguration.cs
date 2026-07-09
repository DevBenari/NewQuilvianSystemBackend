using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientIntegratedProgressNoteConfiguration : IEntityTypeConfiguration<TrxPatientIntegratedProgressNote>
    {
        public void Configure(EntityTypeBuilder<TrxPatientIntegratedProgressNote> entity)
        {
            entity.ToTable("TrxPatientIntegratedProgressNote", "public");

            entity.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            entity.Property(x => x.ProgressNoteNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.EncounterId)
                .IsRequired(false);

            entity.Property(x => x.QueueId)
                .IsRequired(false);

            entity.Property(x => x.ConsultationId)
                .IsRequired(false);

            entity.Property(x => x.AssessmentId)
                .IsRequired(false);

            entity.Property(x => x.VitalSignId)
                .IsRequired(false);

            entity.Property(x => x.DoctorId)
                .IsRequired(false);

            entity.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.NoteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.ProfessionType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ProfessionName)
                .HasMaxLength(100);

            entity.Property(x => x.ProviderUserId)
                .IsRequired(false);

            entity.Property(x => x.ProviderDisplayNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.ProviderRoleSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.ServiceUnitNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.LocationSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.SourceModule)
                .HasMaxLength(80);

            entity.Property(x => x.SourceReferenceId)
                .IsRequired(false);

            entity.Property(x => x.SourceReferenceNumber)
                .HasMaxLength(80);

            entity.Property(x => x.SubjectiveSummary)
                .HasColumnType("text");

            entity.Property(x => x.ObjectiveSummary)
                .HasColumnType("text");

            entity.Property(x => x.AssessmentSummary)
                .HasColumnType("text");

            entity.Property(x => x.PlanSummary)
                .HasColumnType("text");

            entity.Property(x => x.Instruction)
                .HasColumnType("text");

            entity.Property(x => x.Evaluation)
                .HasColumnType("text");

            entity.Property(x => x.NoteText)
                .HasColumnType("text");

            entity.Property(x => x.PrivateNote)
                .HasColumnType("text");

            entity.Property(x => x.IsGeneratedFromSource)
                .HasDefaultValue(false);

            entity.Property(x => x.IsReadOnlyGenerated)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
                .HasMaxLength(250);

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

            // =========================
            // INDEXES
            // =========================

            entity.HasIndex(x => x.ProgressNoteNumber)
                .IsUnique();

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.EncounterId);
            entity.HasIndex(x => x.QueueId);
            entity.HasIndex(x => x.ConsultationId);
            entity.HasIndex(x => x.AssessmentId);
            entity.HasIndex(x => x.VitalSignId);
            entity.HasIndex(x => x.DoctorId);
            entity.HasIndex(x => x.ServiceUnitId);
            entity.HasIndex(x => x.ClinicId);
            entity.HasIndex(x => x.ProviderUserId);
            entity.HasIndex(x => x.CancelledByUserId);

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.NoteDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.NoteDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.ProfessionType,
                x.NoteDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.ProfessionType,
                x.NoteDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.SourceModule,
                x.SourceReferenceId,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete,
                x.IsCancel
            });

            // =========================
            // RELATIONS
            // =========================

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Queue)
                .WithMany()
                .HasForeignKey(x => x.QueueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Consultation)
                .WithMany()
                .HasForeignKey(x => x.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Assessment)
                .WithMany()
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VitalSign)
                .WithMany()
                .HasForeignKey(x => x.VitalSignId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ProviderUser)
                .WithMany()
                .HasForeignKey(x => x.ProviderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
