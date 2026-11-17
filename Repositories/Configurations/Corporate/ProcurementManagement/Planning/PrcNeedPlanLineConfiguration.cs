using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Planning
{
    /// <summary>
    /// Baris rencana kebutuhan — kamus data bagian 3. Barang unik per rencana (PBJ-VAL-044) dan
    /// <c>LineKey</c> unik per rencana, keduanya hanya untuk baris yang masih hidup.
    /// </summary>
    public class PrcNeedPlanLineConfiguration : IEntityTypeConfiguration<PrcNeedPlanLine>
    {
        public void Configure(EntityTypeBuilder<PrcNeedPlanLine> builder)
        {
            builder.ToTable("PrcNeedPlanLine", "public", table =>
                table.HasCheckConstraint("CK_PrcNeedPlanLine_Quantity", "\"Quantity\" > 0"));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity).HasColumnType("numeric(18,4)");
            builder.Property(x => x.EstimatedUnitPrice).HasColumnType("numeric(18,2)");
            builder.Property(x => x.EstimatedTotal).HasColumnType("numeric(18,2)");
            builder.Property(x => x.Justification).HasMaxLength(1000);
            builder.Property(x => x.Specification).HasMaxLength(1000);

            builder.HasIndex(x => new { x.NeedPlanId, x.LineKey })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.NeedPlanId, x.ItemId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.LineKey);

            builder.HasOne(x => x.Item).WithMany()
                .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FundSource).WithMany()
                .HasForeignKey(x => x.FundSourceId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
