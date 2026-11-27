using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Baris penawaran — kamus data bagian 4. Satu baris per item paket; harga positif dan harga terkini tidak melebihi
    /// harga awal (PBJ-ASM-005).
    /// </summary>
    public class PrcQuotationLineConfiguration : IEntityTypeConfiguration<PrcQuotationLine>
    {
        public void Configure(EntityTypeBuilder<PrcQuotationLine> builder)
        {
            builder.ToTable("PrcQuotationLine", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcQuotationLine_OfferedPrice", "\"OfferedUnitPriceBeforeTax\" > 0");
                table.HasCheckConstraint("CK_PrcQuotationLine_CurrentPrice", "\"CurrentUnitPriceBeforeTax\" > 0 AND \"CurrentUnitPriceBeforeTax\" <= \"OfferedUnitPriceBeforeTax\"");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OfferedUnitPriceBeforeTax).HasColumnType("numeric(18,2)");
            builder.Property(x => x.CurrentUnitPriceBeforeTax).HasColumnType("numeric(18,2)");
            builder.Property(x => x.FinalUnitPrice).HasColumnType("numeric(18,2)");
            builder.Property(x => x.LineAmount).HasColumnType("numeric(18,2)");

            builder.HasIndex(x => new { x.QuotationId, x.PackageItemId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcQuotationLine_QuotationId_PackageItemId");
            builder.HasIndex(x => x.PackageItemId);

            builder.HasOne(x => x.Quotation).WithMany(x => x.Lines)
                .HasForeignKey(x => x.QuotationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PackageItem).WithMany()
                .HasForeignKey(x => x.PackageItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SourceQuotationLine).WithMany()
                .HasForeignKey(x => x.SourceQuotationLineId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
