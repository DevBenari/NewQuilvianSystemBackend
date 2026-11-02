using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan riwayat status order — <c>data-dictionary.md</c> bagian 3.</summary>
    public class McuOrderStatusHistoryConfiguration : IEntityTypeConfiguration<McuOrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<McuOrderStatusHistory> builder)
        {
            builder.ToTable("McuOrderStatusHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();

            builder.HasIndex(x => new { x.McuOrderId, x.ChangedAt })
                .HasDatabaseName("IX_McuOrderStatusHistory_Order_ChangedAt");

            builder.HasOne(x => x.McuOrder)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.McuOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
