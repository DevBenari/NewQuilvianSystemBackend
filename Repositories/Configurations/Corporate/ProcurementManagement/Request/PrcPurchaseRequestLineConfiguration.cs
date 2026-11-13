using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Request
{
    /// <summary>Baris PR — kamus data bagian 4. Barang dari katalog (Restrict); kuantitas positif dijaga DB.</summary>
    public class PrcPurchaseRequestLineConfiguration : IEntityTypeConfiguration<PrcPurchaseRequestLine>
    {
        public void Configure(EntityTypeBuilder<PrcPurchaseRequestLine> builder)
        {
            builder.ToTable("PrcPurchaseRequestLine", "public", table =>
                table.HasCheckConstraint("CK_PrcPurchaseRequestLine_Quantity", "\"RequestedQuantity\" > 0"));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Specification).HasMaxLength(1000);
            builder.Property(x => x.RequestedQuantity).HasColumnType("numeric(18,4)");
            builder.Property(x => x.EstimatedUnitPrice).HasColumnType("numeric(18,2)");
            builder.Property(x => x.InPlanQuantity).HasColumnType("numeric(18,4)");
            builder.Property(x => x.OutOfPlanQuantity).HasColumnType("numeric(18,4)");
            builder.Property(x => x.OutOfPlanReason).HasMaxLength(1000);
            builder.Property(x => x.StockAvailableQuantity).HasColumnType("numeric(18,4)");
            builder.Property(x => x.StockFulfilledQuantity).HasColumnType("numeric(18,4)");
            builder.Property(x => x.StockSourceNote).HasMaxLength(300);
            builder.Property(x => x.PurchaseQuantity).HasColumnType("numeric(18,4)");
            builder.Property(x => x.PurchaseAmount).HasColumnType("numeric(18,2)");

            // Baris lama yang diganti saat draf disunting ditandai IsDelete, jadi keunikan nomor baris
            // hanya berlaku untuk baris yang masih hidup.
            builder.HasIndex(x => new { x.PurchaseRequestId, x.LineNo })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.NeedPlanLineKey);

            builder.HasOne(x => x.Item).WithMany()
                .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
