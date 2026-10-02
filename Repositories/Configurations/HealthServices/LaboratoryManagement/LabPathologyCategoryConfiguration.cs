using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPathologyCategoryConfiguration : IEntityTypeConfiguration<LabPathologyCategory>
    {
        public void Configure(EntityTypeBuilder<LabPathologyCategory> builder)
        {
            builder.ToTable("LabPathologyCategory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CategoryCode).HasMaxLength(32).IsRequired();
            builder.Property(x => x.CategoryName).HasMaxLength(128).IsRequired();

            // VAL-101. PARSIAL, dan itu bukan pilihan gaya. Penghapusan di sistem ini bersifat
            // penandaan, sehingga baris tertandai hapus tetap menempati kodenya — kode yang
            // pernah dipakai lalu dihapus tidak akan pernah dapat dipakai lagi. Persis cacat
            // yang sudah dibayar modul ini lewat LAB-CONFLICT-005.
            builder.HasIndex(x => x.CategoryCode)
                .IsUnique()
                .HasDatabaseName("IX_LabPathologyCategory_CategoryCode")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
