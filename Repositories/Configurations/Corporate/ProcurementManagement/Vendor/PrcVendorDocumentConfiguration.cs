using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Vendor
{
    /// <summary>
    /// Dokumen legalitas vendor — kamus data manajemen-vendor bagian 3. Index gabungan melayani pencarian
    /// "dokumen terverifikasi terbaru per jenis"; index tanggal berlaku melayani daftar segera kedaluwarsa.
    /// </summary>
    public class PrcVendorDocumentConfiguration : IEntityTypeConfiguration<PrcVendorDocument>
    {
        public void Configure(EntityTypeBuilder<PrcVendorDocument> builder)
        {
            builder.ToTable("PrcVendorDocument", "public", table =>
                table.HasCheckConstraint(
                    "CK_PrcVendorDocument_ExpiryDate",
                    "\"ExpiryDate\" IS NULL OR \"IssuedDate\" IS NULL OR \"ExpiryDate\" >= \"IssuedDate\""));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocumentNumber).HasMaxLength(100);
            builder.Property(x => x.VerificationStatus).HasMaxLength(20).IsRequired();
            builder.Property(x => x.RejectionReason).HasMaxLength(1000);

            // PBJ-VAL-090: dua pemeriksa memutus dokumen yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => new { x.VendorProfileId, x.DocumentTypeId, x.VerificationStatus });
            builder.HasIndex(x => x.ExpiryDate);
            builder.HasIndex(x => x.DocumentTypeId);

            builder.HasOne(x => x.VendorProfile).WithMany()
                .HasForeignKey(x => x.VendorProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DocumentType).WithMany()
                .HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
