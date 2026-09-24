using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgDoctorAssignmentConfiguration : IEntityTypeConfiguration<EmgDoctorAssignment>
    {
        public void Configure(EntityTypeBuilder<EmgDoctorAssignment> builder)
        {
            builder.ToTable("EmgDoctorAssignment", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AssignmentReason).HasMaxLength(500);

            // Kamus data bagian 4 — index riwayat, dibaca urut waktu oleh GET /.
            builder.HasIndex(x => new { x.EmergencyVisitId, x.EffectiveFrom });

            // FR-IGD-019 — TEPAT SATU dokter berjalan per kunjungan, dijaga BASIS DATA, bukan
            // hanya service. Dua permintaan bersamaan yang sama-sama lolos pemeriksaan service
            // tetap ditolak di sini; salah satunya gagal dengan 23505.
            //
            // Penyaringnya "EffectiveTo" IS NULL — sejalan IGD-DEC-130 yang menjadikannya
            // satu-satunya penanda penugasan berjalan. IsDelete SENGAJA tidak ikut disaring:
            // menambahkannya membuat baris yang di-soft-delete melepaskan slot berjalannya,
            // sehingga satu kunjungan dapat punya dua baris EffectiveTo IS NULL yang keduanya
            // terbaca service. Pelajaran BE-IGD-047, tempat penghitung menyaring IsDelete
            // sedangkan index-nya tidak.
            //
            // Namanya ditulis eksplisit supaya stabil dan tidak bergantung pada pembangkit nama
            // EF, seperti IX_InpDoctorAssignment_EpisodeId_ActiveDpjp.
            builder.HasIndex(x => x.EmergencyVisitId, "IX_EmgDoctorAssignment_EmergencyVisitId_Active")
                .IsUnique()
                .HasFilter("\"EffectiveTo\" IS NULL");

            builder.HasIndex(x => x.DoctorId);
            builder.HasIndex(x => x.EffectiveTo);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany(x => x.DoctorAssignments)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Nullable BE-IGD-048 / IGD-DEC-136: pelaku historis boleh tidak dapat dibuktikan
            // pada baris hasil pengisian data lama. Relasi karena itu opsional; FK tetap
            // Restrict supaya AspNetUsers yang pernah jadi pelaku tidak dapat dihapus begitu
            // saja selama riwayatnya masih dirujuk.
            builder.HasOne(x => x.AssignedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
