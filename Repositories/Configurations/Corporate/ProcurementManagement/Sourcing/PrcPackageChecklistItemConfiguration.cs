using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Checklist dokumen paket — kamus data bagian 3. Satu jenis dokumen per paket.
    /// </summary>
    public class PrcPackageChecklistItemConfiguration : IEntityTypeConfiguration<PrcPackageChecklistItem>
    {
        public void Configure(EntityTypeBuilder<PrcPackageChecklistItem> builder)
        {
            builder.ToTable("PrcPackageChecklistItem", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocumentTypeCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.DocumentTypeName).HasMaxLength(150).IsRequired();

            builder.HasIndex(x => new { x.PackageId, x.DocumentTypeId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcPackageChecklistItem_PackageId_DocumentTypeId");
            builder.HasIndex(x => x.DocumentTypeId);

            builder.HasOne(x => x.Package).WithMany(x => x.ChecklistItems)
                .HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DocumentType).WithMany()
                .HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
