using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpDoctorAssignmentConfiguration : IEntityTypeConfiguration<InpDoctorAssignment>
    {
        public void Configure(EntityTypeBuilder<InpDoctorAssignment> builder)
        {
            // INV-INP-12 — penugasan singkat penulisan catatan terlambat selalu berperan
            // dokter jaga, selalu punya waktu selesai yang lebih besar dari waktu mulai, dan
            // selalu beralasan. Ditambahkan BE-RWI-079.
            //
            // KENAPA DI DATABASE, BUKAN HANYA DI SERVICE. Jendela penulisan inilah yang dibaca
            // penjaga kewenangan menulis catatan klinis. Baris LateDocumentation tanpa
            // EndDateTime bukan "penugasan singkat yang kurang lengkap" — ia jendela penulisan
            // yang tidak pernah tertutup, dan dokter jaga itu memperoleh akses menulis rekam
            // medis pasien tersebut selamanya. Penjaga di service hanya bekerja pada jalur yang
            // memanggilnya; constraint bekerja pada setiap jalur, termasuk skrip perbaikan data
            // dan import — data-dictionary.md bagian 18.4.
            builder.ToTable("InpDoctorAssignment", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_InpDoctorAssignment_LateDocumentation",
                    "\"AssignmentPurpose\" <> 1 "
                    + "OR (\"AssignmentRole\" = 3 "
                    + "AND \"EndDateTime\" IS NOT NULL "
                    + "AND \"EndDateTime\" > \"StartDateTime\" "
                    + "AND \"HandoverReason\" IS NOT NULL "
                    + "AND length(trim(\"HandoverReason\")) > 0)");
            });

            builder.HasKey(x => x.Id);


            builder.Property(x => x.HandoverReason).HasMaxLength(500);

            // BE-RWI-074 — nilai bawaan database 1 (Dpjp) supaya baris lama sah tanpa diisi
            // terpisah. Nilai 0 tidak pernah dipakai enum, sehingga peran kosong mustahil
            // tersimpan.
            builder.Property(x => x.AssignmentRole)
                .HasDefaultValue(InpDoctorAssignmentRole.Dpjp);

            // BE-RWI-079 — nilai bawaan database 0 (Regular). Berbeda dari AssignmentRole,
            // nilai 0 DIPAKAI di sini dan artinya bukan "belum ditetapkan": seluruh baris lama
            // memang penugasan biasa, sehingga bawaannya adalah jawaban yang benar, bukan
            // jawaban kosong — data-dictionary.md bagian 18.1.
            builder.Property(x => x.AssignmentPurpose)
                .HasDefaultValue(InpDoctorAssignmentPurpose.Regular);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.DoctorId);
            builder.HasIndex(x => x.StartDateTime);
            builder.HasIndex(x => x.EndDateTime);
            builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();

            // INV-INP-03 — satu episode punya TEPAT SATU DPJP aktif.
            //
            // Bentuk sampai BE-RWI-073, DICABUT BE-RWI-074 pada 11 September 2026:
            //     IX_InpDoctorAssignment_EpisodeId_Active, filter "EndDateTime" IS NULL
            //
            // Filter lama menolak SETIAP penugasan terbuka kedua, termasuk konsulen dan
            // dokter jaga yang disahkan RWI-DEC-099. Filter baru menegakkan bunyi
            // INV-INP-03 apa adanya sambil mengizinkan konsulen dan dokter jaga
            // berdampingan — data-dictionary.md bagian 2.1.
            builder.HasIndex(x => x.EpisodeId, "IX_InpDoctorAssignment_EpisodeId_ActiveDpjp")
                .IsUnique()
                .HasFilter("\"EndDateTime\" IS NULL AND \"AssignmentRole\" = 1");

            // Mempercepat penilaian kewenangan menulis pada waktu klinis tertentu. Namanya
            // ditulis eksplisit karena nama bawaan EF melewati batas 63 karakter Postgres.
            builder.HasIndex(
                x => new { x.EpisodeId, x.DoctorId, x.AssignmentRole, x.StartDateTime },
                "IX_InpDoctorAssignment_Episode_Doctor_Role_Period");

            // BE-RWI-079 — census "pasien saya". Saringan census assignedToMe berangkat dari
            // dokter lalu menilai masa berlaku, bukan berangkat dari episode, sehingga index
            // per episode di atas tidak membantunya sama sekali. Filter soft-delete membuat
            // index ini hanya memuat baris yang benar-benar dibaca — NFR-026,
            // data-dictionary.md bagian 18.1.
            builder.HasIndex(
                x => new { x.DoctorId, x.EndDateTime },
                "IX_InpDoctorAssignment_DoctorId_Active")
                .HasFilter("\"IsDelete\" = false");

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.DoctorAssignments)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
