using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalRecordManagement
{
    public class MrcAccessLogConfiguration
        : IEntityTypeConfiguration<MrcAccessLog>
    {
        public void Configure(EntityTypeBuilder<MrcAccessLog> builder)
        {
            builder.ToTable("MrcAccessLog", "public");

            // Kunci utama gabungan, bukan Id saja.
            //
            // Ini disiapkan untuk pembagian tabel per tahun: PostgreSQL mensyaratkan kolom
            // pembagi ikut menjadi bagian kunci utama. Menyiapkannya sejak awal jauh lebih murah
            // daripada mengubah kunci utama pada tabel yang sudah berisi jutaan baris — dan
            // itulah bagian yang menuntut waktu henti layanan bila ditunda.
            builder.HasKey(x => new { x.Id, x.AccessedAt });

            builder.Property(x => x.AccessType).HasConversion<int>();
            builder.Property(x => x.AccessScope).HasConversion<int>();

            builder.Property(x => x.UserDisplayNameSnapshot).HasMaxLength(200).IsRequired();
            builder.Property(x => x.UserRoleSnapshot).HasMaxLength(150);
            builder.Property(x => x.AccessReason).HasMaxLength(500);
            builder.Property(x => x.ReviewNote).HasMaxLength(500);
            builder.Property(x => x.IpAddress).HasMaxLength(64);
            builder.Property(x => x.ClientInfo).HasMaxLength(250);
            builder.Property(x => x.RequestPath).HasMaxLength(250);

            // "Siapa saja membuka rekam medis pasien ini?"
            builder.HasIndex(x => new { x.PatientId, x.AccessedAt });

            // "Apa saja yang dibuka pengguna ini?"
            builder.HasIndex(x => new { x.UserId, x.AccessedAt });

            // Antrean tinjauan unit rekam medis.
            builder.HasIndex(x => new { x.IsFlaggedForReview, x.ReviewedAt, x.AccessedAt });

            // Laporan perbandingan akses rawatan dan beralasan.
            builder.HasIndex(x => new { x.AccessType, x.AccessedAt });

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AccessPurpose)
                .WithMany()
                .HasForeignKey(x => x.AccessPurposeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
