using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    /// <summary>
    /// Jenis dokumen legalitas vendor — kamus data manajemen-vendor bagian 2. Kode unik di antara baris aktif;
    /// service menyimpan kode dalam huruf besar sehingga keunikan berlaku tanpa beda huruf (pola master Pengadaan).
    /// </summary>
    public class MstProcurementVendorDocumentTypeConfiguration : IEntityTypeConfiguration<MstProcurementVendorDocumentType>
    {
        public void Configure(EntityTypeBuilder<MstProcurementVendorDocumentType> builder)
        {
            builder.ToTable("MstProcurementVendorDocumentType", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocumentTypeCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.DocumentTypeName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.DocumentTypeCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
