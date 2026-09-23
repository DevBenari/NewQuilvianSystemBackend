using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabSpecimenDetailTypeConfiguration : IEntityTypeConfiguration<LabSpecimenDetailType>
    {
        public void Configure(EntityTypeBuilder<LabSpecimenDetailType> builder)
        {
            builder.ToTable("LabSpecimenDetailType", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabSpecimenTypeId).IsRequired();
            builder.Property(x => x.DetailTypeCode).IsRequired().HasMaxLength(64);
            builder.Property(x => x.DetailTypeNameId).HasMaxLength(300);
            builder.Property(x => x.DetailTypeNameEn).IsRequired().HasMaxLength(300);
            builder.Property(x => x.SubTypeName).HasMaxLength(200);
            builder.Property(x => x.SnomedCode).HasMaxLength(32);
            builder.Property(x => x.IsActive).IsRequired();

            builder.HasOne(x => x.LabSpecimenType)
                .WithMany()
                .HasForeignKey(x => x.LabSpecimenTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.DetailTypeCode)
                .IsUnique()
                .HasDatabaseName("IX_LabSpecimenDetailType_DetailTypeCode")
                .HasFilter("\"IsDelete\" = false");

            // PARSIAL, dan pembatasnya ADA DUA. Baris yang ditambahkan lewat jalan keluar
            // Lainnya seluruhnya berkode SNOMED kosong — index unik tanpa pembatas kedua akan
            // menolak baris lokal KEDUA, sebab NULL kedua dianggap bertabrakan pada sebagian
            // mesin. Pembatas "IS NOT NULL" membuat keduanya lolos.
            builder.HasIndex(x => x.SnomedCode)
                .IsUnique()
                .HasDatabaseName("IX_LabSpecimenDetailType_SnomedCode")
                .HasFilter("\"IsDelete\" = false AND \"SnomedCode\" IS NOT NULL");

            builder.HasIndex(x => x.LabSpecimenTypeId);

            // Pengelompokan pelaporan — satu-satunya kegunaan SubTypeName.
            builder.HasIndex(x => x.SubTypeName);

            // Pencarian DUA BAHASA (RULE-007): pemeriksaan duplikasi membandingkan nama
            // Indonesia maupun Inggris.
            builder.HasIndex(x => x.DetailTypeNameId);
            builder.HasIndex(x => x.DetailTypeNameEn);
        }
    }

    public class LabSpecimenDetailConfiguration : IEntityTypeConfiguration<LabSpecimenDetail>
    {
        public void Configure(EntityTypeBuilder<LabSpecimenDetail> builder)
        {
            builder.ToTable("LabSpecimenDetail", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LabSpecimenId).IsRequired();
            builder.Property(x => x.LabSpecimenDetailTypeId).IsRequired();
            builder.Property(x => x.DetailNameSnapshot).IsRequired().HasMaxLength(300);

            // Cascade dari wadahnya: rincian nol punya umur sendiri.
            builder.HasOne(x => x.LabSpecimen)
                .WithMany()
                .HasForeignKey(x => x.LabSpecimenId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict pada data induk (INV-31).
            builder.HasOne(x => x.LabSpecimenDetailType)
                .WithMany()
                .HasForeignKey(x => x.LabSpecimenDetailTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // VAL-105. Satu rincian tidak boleh dicentang dua kali pada wadah yang sama.
            builder.HasIndex(x => new { x.LabSpecimenId, x.LabSpecimenDetailTypeId })
                .IsUnique()
                .HasDatabaseName("IX_LabSpecimenDetail_SpecimenId_DetailTypeId")
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
