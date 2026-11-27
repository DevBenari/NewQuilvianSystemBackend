using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Item paket — kamus data bagian 3. Satu baris per barang katalog dalam satu paket (PBJ-DEC-022); jumlah positif.
    /// </summary>
    public class PrcPackageItemConfiguration : IEntityTypeConfiguration<PrcPackageItem>
    {
        public void Configure(EntityTypeBuilder<PrcPackageItem> builder)
        {
            builder.ToTable("PrcPackageItem", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcPackageItem_Quantity", "\"Quantity\" > 0");
                table.HasCheckConstraint("CK_PrcPackageItem_HpsUnitPrice", "\"HpsUnitPrice\" >= 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Specification).HasMaxLength(2000);
            builder.Property(x => x.Quantity).HasColumnType("numeric(18,3)");
            builder.Property(x => x.HpsUnitPrice).HasColumnType("numeric(18,2)");
            builder.Property(x => x.HpsAmount).HasColumnType("numeric(18,2)");

            builder.HasIndex(x => new { x.PackageId, x.ItemId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcPackageItem_PackageId_ItemId");
            builder.HasIndex(x => x.ItemCategoryId);

            builder.HasOne(x => x.Package).WithMany(x => x.Items)
                .HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Item).WithMany()
                .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
