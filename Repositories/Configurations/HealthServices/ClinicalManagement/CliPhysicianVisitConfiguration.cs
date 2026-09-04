using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// Bentuk tabel kejadian visite dokter — <c>BE-RWI-041</c>.
    /// </summary>
    public class CliPhysicianVisitConfiguration : IEntityTypeConfiguration<CliPhysicianVisit>
    {
        public void Configure(EntityTypeBuilder<CliPhysicianVisit> entity)
        {
            entity.ToTable("CliPhysicianVisit", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PhysicianVisitNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.InpEpisodeId)
                .IsRequired(false);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.DoctorId)
                .IsRequired();

            entity.Property(x => x.VisitDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.VisitRole)
                .HasConversion<int>()
                .HasDefaultValue(Areas.HealthServices.ClinicalManagement.Enums.PhysicianVisitRole.Dpjp)
                .IsRequired();

            entity.Property(x => x.VisitStatus)
                .HasConversion<int>()
                .HasDefaultValue(Areas.HealthServices.ClinicalManagement.Enums.PhysicianVisitStatus.Recorded)
                .IsRequired();

            entity.Property(x => x.ConsultationId)
                .IsRequired(false);

            entity.Property(x => x.ProgressNoteId)
                .IsRequired(false);

            entity.Property(x => x.PatientProcedureId)
                .IsRequired(false);

            entity.Property(x => x.Note)
                .HasMaxLength(1000);

            entity.Property(x => x.RecordedByUserId)
                .IsRequired();

            entity.Property(x => x.IdempotencyKey)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
                .HasMaxLength(500);

            entity.Property(x => x.CorrectsVisitId)
                .IsRequired(false);

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

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InpEpisode)
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ketiga tautan dokumen memakai SetNull, bukan Restrict. INV-DOK-07: satu kejadian
            // tidak wajib punya catatan, dan satu catatan tidak wajib punya kejadian.
            entity.HasOne(x => x.Consultation)
                .WithMany()
                .HasForeignKey(x => x.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.ProgressNote)
                .WithMany()
                .HasForeignKey(x => x.ProgressNoteId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.PatientProcedure)
                .WithMany()
                .HasForeignKey(x => x.PatientProcedureId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.RecordedByUser)
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CorrectsVisit)
                .WithMany()
                .HasForeignKey(x => x.CorrectsVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique PENUH, bukan parsial. Kunci milik kejadian yang sudah dibatalkan pun tidak
            // boleh dipakai ulang: bila boleh, sebuah kiriman ulang lama dapat menghidupkan
            // kembali kejadian yang sengaja dibatalkan.
            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique();

            entity.HasIndex(x => x.PhysicianVisitNumber)
                .IsUnique();

            entity.HasIndex(x => x.EncounterId);

            // Riwayat kunjungan satu perawatan, terurut waktu kedatangan.
            entity.HasIndex(x => new
            {
                x.InpEpisodeId,
                x.VisitDateTime
            });

            // Kunjungan seorang dokter sepanjang waktu, dipakai laporan dan agregasi tarif.
            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.VisitDateTime
            });

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.VisitStatus);

            entity.HasIndex(x => x.CorrectsVisitId);

            // Tidak ada unique atas pasangan InpEpisodeId, DoctorId, dan tanggal. Dokter yang
            // benar-benar datang dua kali pada hari yang sama menghasilkan dua baris, dan
            // melarangnya membatalkan keputusan pemilik - RWI-DEC-085.
        }
    }
}
