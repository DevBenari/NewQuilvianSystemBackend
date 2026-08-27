using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService.MedicalRecordManagement
{
    public class TrxClinicalDocumentIntegrityConfiguration
        : IEntityTypeConfiguration<TrxClinicalDocumentIntegrity>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalDocumentIntegrity> builder)
        {
            builder.ToTable("TrxClinicalDocumentIntegrity", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DocumentKind).HasConversion<int>();
            builder.Property(x => x.IntegrityStatus).HasConversion<int>();
            builder.Property(x => x.LockTrigger).HasConversion<int>();

            builder.Property(x => x.SignatureDeviceInfo).HasMaxLength(250);
            builder.Property(x => x.SignatureIpAddress).HasMaxLength(64);
            builder.Property(x => x.CancelledReason).HasMaxLength(250);

            // Satu dokumen tepat satu baris keutuhan. Tanpa index ini, satu dokumen bisa punya
            // dua status yang bertentangan.
            builder.HasIndex(x => new { x.DocumentKind, x.DocumentId })
                .IsUnique();

            // "Berapa catatan pasien ini yang belum ditandatangani?"
            builder.HasIndex(x => new { x.PatientId, x.IntegrityStatus, x.IsDelete });

            // Dipakai saat kunjungan ditutup untuk menemukan dokumen yang masih terbuka.
            builder.HasIndex(x => new { x.EncounterId, x.IntegrityStatus, x.IsDelete });

            // "Catatan saya yang belum saya tandatangani."
            builder.HasIndex(x => new { x.AuthorUserId, x.IntegrityStatus, x.IsDelete });

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
