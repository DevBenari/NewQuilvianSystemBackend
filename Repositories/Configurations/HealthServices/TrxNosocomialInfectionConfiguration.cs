using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Configurations
{
    /// <summary>
    /// Konfigurasi tabel catatan infeksi nosokomial.
    /// </summary>
    /// <remarks>
    /// Relasi sengaja disimpan sebagai kolom identifier tanpa navigation property. Entitas ini
    /// menunjuk ke pasien, kunjungan, unit pelayanan, dan pengguna yang seluruhnya dimiliki
    /// modul lain; menautkannya lewat navigation property akan menarik lima modul ke dalam
    /// setiap query surveilans tanpa ada yang membutuhkannya. Join dilakukan secara eksplisit
    /// pada endpoint yang memang perlu menampilkan nama.
    /// </remarks>
    public class TrxNosocomialInfectionConfiguration
        : IEntityTypeConfiguration<TrxNosocomialInfection>
    {
        public void Configure(EntityTypeBuilder<TrxNosocomialInfection> builder)
        {
            builder.ToTable("TrxNosocomialInfection", "public");

            builder.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            builder.Property(x => x.NosocomialRecordNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.InfectionType).HasConversion<int>();
            builder.Property(x => x.Status).HasConversion<int>();
            builder.Property(x => x.OnsetCategory).HasConversion<int>();

            builder.Property(x => x.InfectionTypeOther).HasMaxLength(250);
            builder.Property(x => x.DeviceName).HasMaxLength(150);
            builder.Property(x => x.CriteriaMet).HasMaxLength(2000);
            builder.Property(x => x.CultureSpecimenType).HasMaxLength(250);
            builder.Property(x => x.CultureResult).HasMaxLength(500);
            builder.Property(x => x.CausativeOrganism).HasMaxLength(250);
            builder.Property(x => x.AntibioticTherapy).HasMaxLength(1000);
            builder.Property(x => x.ReportedByNameSnapshot).HasMaxLength(150);
            builder.Property(x => x.VerifiedByNameSnapshot).HasMaxLength(150);
            builder.Property(x => x.RuledOutReason).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(2000);

            builder.Property(x => x.OnsetDateTime).IsRequired();
            builder.Property(x => x.ReportedAt).IsRequired();

            // =========================
            // CONSTRAINTS
            // =========================

            // Lama pemakaian alat menjadi penyebut indikator mutu. Nilai negatif akan
            // menghasilkan angka insiden yang tidak punya arti, jadi ditolak di basis data.
            builder.HasCheckConstraint(
                "CK_TrxNosocomialInfection_DeviceUsageDays",
                "\"DeviceUsageDays\" IS NULL OR \"DeviceUsageDays\" >= 0");

            builder.HasCheckConstraint(
                "CK_TrxNosocomialInfection_HoursSinceAdmission",
                "\"HoursSinceAdmission\" IS NULL OR \"HoursSinceAdmission\" >= 0");

            // =========================
            // INDEXES
            // =========================

            builder.HasIndex(x => x.NosocomialRecordNumber).IsUnique();

            // Riwayat infeksi satu pasien, diurutkan dari kejadian terbaru.
            builder.HasIndex(x => new { x.PatientId, x.OnsetDateTime });

            // Layar pengkajian IGD membuka catatan menurut kunjungannya.
            builder.HasIndex(x => x.EmergencyVisitId);

            builder.HasIndex(x => x.EncounterId);

            // Laporan surveilans PPI: per unit, per jenis, per status, dalam rentang waktu.
            builder.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.InfectionType,
                x.Status,
                x.OnsetDateTime
            });
        }
    }
}
