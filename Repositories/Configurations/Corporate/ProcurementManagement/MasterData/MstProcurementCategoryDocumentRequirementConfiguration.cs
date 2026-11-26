using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    /// <summary>
    /// Dokumen wajib per kategori — kamus data manajemen-vendor bagian 2. Satu pasangan kategori × jenis dokumen
    /// di antara baris aktif.
    /// </summary>
    public class MstProcurementCategoryDocumentRequirementConfiguration : IEntityTypeConfiguration<MstProcurementCategoryDocumentRequirement>
    {
        public void Configure(EntityTypeBuilder<MstProcurementCategoryDocumentRequirement> builder)
        {
            builder.ToTable("MstProcurementCategoryDocumentRequirement", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ItemCategoryId, x.DocumentTypeId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.DocumentTypeId);

            builder.HasOne(x => x.ItemCategory).WithMany()
                .HasForeignKey(x => x.ItemCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DocumentType).WithMany()
                .HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
