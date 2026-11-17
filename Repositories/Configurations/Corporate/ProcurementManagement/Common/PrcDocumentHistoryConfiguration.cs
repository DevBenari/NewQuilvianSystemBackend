using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Common
{
    /// <summary>
    /// Pemetaan riwayat dokumen Pengadaan — <c>data-dictionary.md</c> bagian 5. Polimorfik, tanpa FK.
    /// </summary>
    public class PrcDocumentHistoryConfiguration : IEntityTypeConfiguration<PrcDocumentHistory>
    {
        public void Configure(EntityTypeBuilder<PrcDocumentHistory> builder)
        {
            builder.ToTable("PrcDocumentHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DocumentType).HasMaxLength(40).IsRequired();
            builder.Property(x => x.DocumentId).IsRequired();
            builder.Property(x => x.DocumentNumber).HasMaxLength(30);
            builder.Property(x => x.ActionCode).HasMaxLength(40).IsRequired();
            builder.Property(x => x.FromStatus).HasMaxLength(30);
            builder.Property(x => x.ToStatus).HasMaxLength(30);
            builder.Property(x => x.Reason).HasMaxLength(1000);
            builder.Property(x => x.SnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.ActorUserId).IsRequired();
            builder.Property(x => x.OccurredAt).IsRequired();

            builder.HasIndex(x => new { x.DocumentType, x.DocumentId, x.OccurredAt });
            builder.HasIndex(x => x.DocumentNumber);
            builder.HasIndex(x => x.ActorUserId);
        }
    }
}
