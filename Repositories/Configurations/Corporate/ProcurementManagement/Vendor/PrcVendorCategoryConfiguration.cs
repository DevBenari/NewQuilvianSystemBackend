using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Vendor
{
    /// <summary>
    /// Kategori yang dilayani vendor — kamus data manajemen-vendor bagian 3. Satu pasangan vendor × kategori di
    /// antara baris aktif; relasi ke profil diatur di <see cref="PrcVendorProfileConfiguration"/>.
    /// </summary>
    public class PrcVendorCategoryConfiguration : IEntityTypeConfiguration<PrcVendorCategory>
    {
        public void Configure(EntityTypeBuilder<PrcVendorCategory> builder)
        {
            builder.ToTable("PrcVendorCategory", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.VendorProfileId, x.ItemCategoryId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.ItemCategoryId);

            builder.HasOne(x => x.ItemCategory).WithMany()
                .HasForeignKey(x => x.ItemCategoryId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
