using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptOrderStatusHistory</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptOrderStatusHistoryConfiguration : IEntityTypeConfiguration<OptOrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OptOrderStatusHistory> builder)
        {
            builder.ToTable("OptOrderStatusHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();

            builder.HasIndex(x => new { x.OrderId, x.OccurredAt }).HasDatabaseName("IX_OptOrderStatusHistory_OrderId_OccurredAt");

            builder.HasOne(x => x.Order)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
