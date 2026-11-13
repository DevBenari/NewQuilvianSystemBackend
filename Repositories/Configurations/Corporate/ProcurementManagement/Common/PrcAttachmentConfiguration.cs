using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Common
{
    /// <summary>Pemetaan lampiran Pengadaan — kamus data bagian 5. Polimorfik, tanpa FK.</summary>
    public class PrcAttachmentConfiguration : IEntityTypeConfiguration<PrcAttachment>
    {
        public void Configure(EntityTypeBuilder<PrcAttachment> builder)
        {
            builder.ToTable("PrcAttachment", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DocumentType).HasMaxLength(40).IsRequired();
            builder.Property(x => x.AttachmentCategory).HasMaxLength(50);
            builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.StoredFileName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
            builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired();

            builder.HasIndex(x => new { x.DocumentType, x.DocumentId });
            builder.HasIndex(x => new { x.AttachmentKey, x.VersionNo }).IsUnique();
            builder.HasIndex(x => x.StoredFileName).IsUnique();
            builder.HasIndex(x => x.Sha256);
        }
    }
}
