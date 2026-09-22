using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabMicrobiologyIsolateConfiguration : IEntityTypeConfiguration<LabMicrobiologyIsolate>
    {
        public void Configure(EntityTypeBuilder<LabMicrobiologyIsolate> builder)
        {
            builder.ToTable("LabMicrobiologyIsolate", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabExaminationId).IsRequired();
            builder.Property(x => x.LabOrganismId).IsRequired();

            builder.Property(x => x.OrganismNameSnapshot).IsRequired().HasMaxLength(200);
            builder.Property(x => x.IsSusceptibilityTested).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            // AC-108. Restrict pada keduanya. Isolat adalah TEMUAN pada pasien: menghapus
            // pemeriksaan atau organisme dari bawahnya akan membuat temuan itu kehilangan
            // artinya. Organisme yang tidak lagi dipakai dinonaktifkan, bukan dihapus
            // (INV-31).
            builder.HasOne(x => x.LabExamination)
                .WithMany()
                .HasForeignKey(x => x.LabExaminationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LabOrganism)
                .WithMany()
                .HasForeignKey(x => x.LabOrganismId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.LabExaminationId);

            // Satu pemeriksaan tidak boleh mencatat organisme yang sama dua kali.
            //
            // PARSIAL, dan itu bukan pilihan gaya. Penghapusan di sistem ini berupa PENANDAAN
            // (IsDelete), bukan penghapusan baris — index unik penuh akan membuat isolat yang
            // sudah ditandai hapus TETAP MENEMPATI kuncinya, sehingga analis yang salah memilih
            // organisme, menghapusnya, lalu memilih organisme yang benar akan ditolak database
            // tanpa sebab yang dapat ia pahami.
            builder.HasIndex(x => new { x.LabExaminationId, x.LabOrganismId })
                .IsUnique()
                .HasDatabaseName("IX_LabMicrobiologyIsolate_LabExaminationId_LabOrganismId")
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
