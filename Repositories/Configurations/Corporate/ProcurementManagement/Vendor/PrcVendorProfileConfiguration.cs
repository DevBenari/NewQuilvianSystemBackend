using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Vendor
{
    /// <summary>
    /// Profil vendor Pengadaan — kamus data manajemen-vendor bagian 3. Satu profil per supplier di antara baris aktif;
    /// alasan wajib saat vendor Dibatasi dijaga CHECK (PBJ-VAL-105).
    /// </summary>
    public class PrcVendorProfileConfiguration : IEntityTypeConfiguration<PrcVendorProfile>
    {
        public void Configure(EntityTypeBuilder<PrcVendorProfile> builder)
        {
            builder.ToTable("PrcVendorProfile", "public", table =>
                table.HasCheckConstraint(
                    "CK_PrcVendorProfile_RestrictionReason",
                    "\"IsRestricted\" = false OR (\"RestrictionReason\" IS NOT NULL AND length(btrim(\"RestrictionReason\")) > 0)"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RestrictionReason).HasMaxLength(1000);

            // PBJ-VAL-090: dua orang mengubah vendor yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.SupplierId).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.IsRestricted);

            builder.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Categories).WithOne(x => x.VendorProfile)
                .HasForeignKey(x => x.VendorProfileId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
