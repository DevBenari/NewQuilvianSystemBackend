using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Sumber baris PR item paket — kamus data bagian 3. Satu baris PR hanya di satu paket hidup: index unik untuk sumber yang
    /// belum dilepas (PBJ-AC-026, 409 PBJ-VAL-121).
    /// </summary>
    public class PrcPackageItemSourceConfiguration : IEntityTypeConfiguration<PrcPackageItemSource>
    {
        public void Configure(EntityTypeBuilder<PrcPackageItemSource> builder)
        {
            builder.ToTable("PrcPackageItemSource", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcPackageItemSource_Quantity", "\"Quantity\" > 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Quantity).HasColumnType("numeric(18,3)");

            builder.HasIndex(x => x.PurchaseRequestLineId)
                .IsUnique()
                .HasFilter("\"IsReleased\" = false AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcPackageItemSource_LiveRequestLine");
            builder.HasIndex(x => x.PackageItemId);
            builder.HasIndex(x => x.PurchaseRequestId);
            builder.HasIndex(x => x.DepartmentId);

            builder.HasOne(x => x.PackageItem).WithMany(x => x.Sources)
                .HasForeignKey(x => x.PackageItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PurchaseRequestLine).WithMany()
                .HasForeignKey(x => x.PurchaseRequestLineId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PurchaseRequest).WithMany()
                .HasForeignKey(x => x.PurchaseRequestId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
