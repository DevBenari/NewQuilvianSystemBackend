using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Negosiasi — kamus data bagian 4. Nomor putaran positif dan unik per penawaran.
    /// </summary>
    public class PrcNegotiationConfiguration : IEntityTypeConfiguration<PrcNegotiation>
    {
        public void Configure(EntityTypeBuilder<PrcNegotiation> builder)
        {
            builder.ToTable("PrcNegotiation", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcNegotiation_RoundNo", "\"RoundNo\" >= 1");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Participants).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Summary).HasMaxLength(2000).IsRequired();

            builder.HasIndex(x => new { x.QuotationId, x.RoundNo })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcNegotiation_QuotationId_RoundNo");

            builder.HasOne(x => x.Quotation).WithMany()
                .HasForeignKey(x => x.QuotationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
