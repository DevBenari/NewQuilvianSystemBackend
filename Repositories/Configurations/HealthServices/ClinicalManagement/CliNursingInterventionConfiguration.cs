using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// Bentuk tabel catatan tindakan keperawatan - <c>BE-RWI-061</c>.
    /// </summary>
    public class CliNursingInterventionConfiguration : IEntityTypeConfiguration<CliNursingIntervention>
    {
        public void Configure(EntityTypeBuilder<CliNursingIntervention> entity)
        {
            entity.ToTable("CliNursingIntervention", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.InpEpisodeId)
                .IsRequired(false);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.CarePlanItemId)
                .IsRequired(false);

            entity.Property(x => x.InterventionName)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(x => x.PerformedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.PerformedByEmployeeId)
                .IsRequired();

            entity.Property(x => x.ResultNote);

            entity.Property(x => x.RecordStatus)
                .HasConversion<int>()
                .HasDefaultValue(NursingInterventionStatus.Recorded)
                .IsRequired();

            entity.Property(x => x.FinalizedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.FinalizedByUserId)
                .IsRequired(false);

            entity.Property(x => x.IdempotencyKey)
                .HasMaxLength(100);

            entity.Property(x => x.IsBillable)
                .HasDefaultValue(false);

            entity.Property(x => x.BillingDispatchStatus)
                .HasConversion<int>()
                .HasDefaultValue(NursingBillingDispatchStatus.NotApplicable)
                .IsRequired();

            entity.Property(x => x.BillingDispatchedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.BillingDispatchAttemptCount)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.BillingDispatchFailureReason)
                .HasMaxLength(500);

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

            // SetNull, bukan Restrict. CAP-013 aturan 6: menutup butir rencana tidak boleh
            // menghapus tindakan yang sudah dilakukan. Tindakannya tetap hidup walaupun rujukan
            // rencananya lepas.
            entity.HasOne(x => x.CarePlanItem)
                .WithMany()
                .HasForeignKey(x => x.CarePlanItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.PerformedByEmployee)
                .WithMany()
                .HasForeignKey(x => x.PerformedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique PARSIAL, bukan penuh. Kunci permintaan boleh kosong - kamus data bagian 6 -
            // sehingga penyaringnya menyebut dua syarat sekaligus: kunci terisi, dan baris belum
            // terhapus. Tanpa penyaring pertama, seluruh baris tanpa kunci akan saling
            // bertabrakan; tanpa penyaring kedua, kunci milik baris yang sudah dihapus lunak
            // menahan pencatatan ulang selamanya.
            //
            // Penjaganya WAJIB di database. Pemeriksaan di aplikasi saja akan bocor ketika dua
            // instance aplikasi berjalan - VAL-KEP-15, QBE-CODE-004.
            entity.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.EncounterId);

            // Daftar tindakan satu perawatan, terurut waktu tindakan.
            entity.HasIndex(x => new
            {
                x.InpEpisodeId,
                x.PerformedAt
            });

            entity.HasIndex(x => x.PerformedByEmployeeId);

            entity.HasIndex(x => x.CarePlanItemId);

            entity.HasIndex(x => x.RecordStatus);

            entity.HasIndex(x => x.BillingDispatchStatus);
        }
    }
}
