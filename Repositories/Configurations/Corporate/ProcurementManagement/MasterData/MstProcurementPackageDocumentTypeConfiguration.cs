using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    /// <summary>
    /// Jenis dokumen paket — kamus data pemilihan-penyedia bagian 2. Kode unik di antara baris aktif; tahap dibatasi
    /// CHECK ke tiga nilai <c>ProcurementStatuses.PackageDocumentStage</c>.
    /// </summary>
    public class MstProcurementPackageDocumentTypeConfiguration : IEntityTypeConfiguration<MstProcurementPackageDocumentType>
    {
        public void Configure(EntityTypeBuilder<MstProcurementPackageDocumentType> builder)
        {
            builder.ToTable("MstProcurementPackageDocumentType", "public", table =>
                table.HasCheckConstraint(
                    "CK_MstProcurementPackageDocumentType_Stage",
                    "\"Stage\" IN ('PREPARATION', 'QUOTATION', 'NEGOTIATION')"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocumentTypeCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.DocumentTypeName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Stage).HasMaxLength(20).IsRequired();
            builder.HasIndex(x => x.DocumentTypeCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
