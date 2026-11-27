using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Baris negosiasi — kamus data bagian 4. Harga baru positif dan tidak melebihi harga sebelumnya (PBJ-VAL-137).
    /// </summary>
    public class PrcNegotiationLineConfiguration : IEntityTypeConfiguration<PrcNegotiationLine>
    {
        public void Configure(EntityTypeBuilder<PrcNegotiationLine> builder)
        {
            builder.ToTable("PrcNegotiationLine", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcNegotiationLine_NewPrice", "\"NewUnitPriceBeforeTax\" > 0 AND \"NewUnitPriceBeforeTax\" <= \"PreviousUnitPriceBeforeTax\"");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PreviousUnitPriceBeforeTax).HasColumnType("numeric(18,2)");
            builder.Property(x => x.NewUnitPriceBeforeTax).HasColumnType("numeric(18,2)");

            builder.HasIndex(x => new { x.NegotiationId, x.QuotationLineId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcNegotiationLine_NegotiationId_QuotationLineId");
            builder.HasIndex(x => x.QuotationLineId);

            builder.HasOne(x => x.Negotiation).WithMany(x => x.Lines)
                .HasForeignKey(x => x.NegotiationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.QuotationLine).WithMany()
                .HasForeignKey(x => x.QuotationLineId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
