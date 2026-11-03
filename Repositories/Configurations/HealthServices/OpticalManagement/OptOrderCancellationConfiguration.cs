using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptOrderCancellation</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptOrderCancellationConfiguration : IEntityTypeConfiguration<OptOrderCancellation>
    {
        public void Configure(EntityTypeBuilder<OptOrderCancellation> builder)
        {
            builder.ToTable("OptOrderCancellation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).HasConversion<int>().IsRequired();

            builder.HasIndex(x => x.OrderId).IsUnique().HasFilter("\"Status\" = 1 AND \"IsDelete\" = false").HasDatabaseName("UX_OptOrderCancellation_OrderId_Requested");

            builder.HasOne(x => x.Order)
                .WithMany(x => x.Cancellations)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
