using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan baris kebutuhan pada order darah.</summary>
    /// <remarks>
    /// Index mengikuti kamus data kontrak <c>v4</c>: <c>BloodOrderId</c> dan
    /// <c>BloodComponentId</c>, keduanya tidak unik. Kontrak tidak menetapkan larangan satu
    /// komponen muncul dua kali dalam satu order, sehingga tidak ada index unik yang
    /// menegakkannya — menambahkannya berarti mengarang aturan bisnis.
    /// </remarks>
    public class BbkBloodOrderLineConfiguration : IEntityTypeConfiguration<BbkBloodOrderLine>
    {
        public void Configure(EntityTypeBuilder<BbkBloodOrderLine> builder)
        {
            builder.ToTable("BbkBloodOrderLine", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BloodOrderId).IsRequired();
            builder.Property(x => x.BloodComponentId).IsRequired();
            builder.Property(x => x.RequestedQuantity).IsRequired();
            builder.Property(x => x.Sequence).IsRequired();

            builder.HasIndex(x => x.BloodOrderId);
            builder.HasIndex(x => x.BloodComponentId);

            builder.HasOne(x => x.BloodComponent)
                .WithMany()
                .HasForeignKey(x => x.BloodComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
