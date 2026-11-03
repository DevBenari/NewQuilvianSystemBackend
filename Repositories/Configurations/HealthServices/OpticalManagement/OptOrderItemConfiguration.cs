using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptOrderItem</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptOrderItemConfiguration : IEntityTypeConfiguration<OptOrderItem>
    {
        public void Configure(EntityTypeBuilder<OptOrderItem> builder)
        {
            builder.ToTable("OptOrderItem", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemCategory).HasConversion<int>().IsRequired();
            builder.Property(x => x.LensSupply).HasConversion<int?>();
            builder.Property(x => x.Quantity).HasPrecision(10, 3);
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();

            builder.HasIndex(x => new { x.OrderId, x.LineNo }).IsUnique().HasDatabaseName("UX_OptOrderItem_OrderId_LineNo");
            builder.HasIndex(x => x.DrugId).HasDatabaseName("IX_OptOrderItem_DrugId");

            builder.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstDrug>()
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
