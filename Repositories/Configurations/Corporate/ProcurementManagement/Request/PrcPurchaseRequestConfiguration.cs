using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Request
{
    /// <summary>PR — kamus data bagian 4. Nomor PR unik bila terisi (QBE-CODE-004).</summary>
    public class PrcPurchaseRequestConfiguration : IEntityTypeConfiguration<PrcPurchaseRequest>
    {
        public void Configure(EntityTypeBuilder<PrcPurchaseRequest> builder)
        {
            builder.ToTable("PrcPurchaseRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestNumber).HasMaxLength(30);
            builder.Property(x => x.UrgencyLevel).HasMaxLength(20).IsRequired();
            builder.Property(x => x.UrgencyReason).HasMaxLength(1000);
            builder.Property(x => x.Justification).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
            builder.Property(x => x.RequestedTotal).HasColumnType("numeric(18,2)");
            builder.Property(x => x.PurchaseTotal).HasColumnType("numeric(18,2)");
            builder.Property(x => x.OverBudgetAmount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.CancelReason).HasMaxLength(1000);

            // PBJ-VAL-090: dua orang menyunting draf yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.RequestNumber)
                .IsUnique()
                .HasFilter("\"RequestNumber\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.Status, x.DepartmentId });
            builder.HasIndex(x => x.RequesterUserId);
            builder.HasIndex(x => x.FiscalYear);
            builder.HasIndex(x => x.UrgencyLevel);
            builder.HasIndex(x => x.WorkflowInstanceId);

            builder.HasOne(x => x.Department).WithMany()
                .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FundSource).WithMany()
                .HasForeignKey(x => x.FundSourceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Lines).WithOne(x => x.PurchaseRequest)
                .HasForeignKey(x => x.PurchaseRequestId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
