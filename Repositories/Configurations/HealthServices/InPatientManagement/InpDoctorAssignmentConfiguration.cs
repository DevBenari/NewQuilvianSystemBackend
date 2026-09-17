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
            builder.ToTable("InpDoctorAssignment", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.HandoverReason).HasMaxLength(500);

            // BE-RWI-074 — nilai bawaan database 1 (Dpjp) supaya baris lama sah tanpa diisi
            // terpisah. Nilai 0 tidak pernah dipakai enum, sehingga peran kosong mustahil
            // tersimpan.
            builder.Property(x => x.AssignmentRole)
                .HasDefaultValue(InpDoctorAssignmentRole.Dpjp);

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
