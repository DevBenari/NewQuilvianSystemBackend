using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Penawaran — kamus data bagian 4. Satu penawaran RECORDED per vendor per RFQ (409 PBJ-VAL-134); PPN 0–100; alasan
    /// batal wajib saat VOID.
    /// </summary>
    public class PrcQuotationConfiguration : IEntityTypeConfiguration<PrcQuotation>
    {
        public void Configure(EntityTypeBuilder<PrcQuotation> builder)
        {
            builder.ToTable("PrcQuotation", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcQuotation_Status", "\"Status\" IN ('RECORDED', 'VOID')");
                table.HasCheckConstraint("CK_PrcQuotation_TaxPercent", "\"TaxPercent\" >= 0 AND \"TaxPercent\" <= 100");
                table.HasCheckConstraint("CK_PrcQuotation_VoidReason", "\"Status\" <> 'VOID' OR length(btrim(coalesce(\"VoidReason\", ''))) > 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.TaxPercent).HasColumnType("numeric(5,2)");
            builder.Property(x => x.TotalBeforeTax).HasColumnType("numeric(18,2)");
            builder.Property(x => x.TaxAmount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.TotalAmount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.VoidReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => new { x.RfqId, x.SupplierId })
                .IsUnique()
                .HasFilter("\"Status\" = 'RECORDED' AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcQuotation_RecordedPerRfqSupplier");
            builder.HasIndex(x => x.PackageId);
            builder.HasIndex(x => x.SupplierId);
            builder.HasIndex(x => x.RecordedByUserId);

            builder.HasOne(x => x.Rfq).WithMany()
                .HasForeignKey(x => x.RfqId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Package).WithMany()
                .HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SourceQuotation).WithMany()
                .HasForeignKey(x => x.SourceQuotationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
