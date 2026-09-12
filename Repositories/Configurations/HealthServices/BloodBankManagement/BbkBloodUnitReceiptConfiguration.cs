using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan penerimaan fisik kantong, entity di dalam <c>BD-AGG-02</c>.</summary>
    /// <remarks>
    /// Index <c>ProviderRequestId</c> dan <c>ReceivedAt</c> mengikuti kamus data kontrak <c>v4</c>.
    /// <b>Index unik <c>(ProviderRequestId, Sequence)</c> tambahan builder</b>: urutan kedatangan
    /// dihitung service di bawah kunci penasihat per permintaan, dan index ini menjadi penjaga
    /// fisiknya sehingga dua penerimaan tidak mungkin menyandang urutan yang sama
    /// (<c>QBE-CODE-003</c> — penghitung urutan wajib berpelindung).
    /// </remarks>
    public class BbkBloodUnitReceiptConfiguration : IEntityTypeConfiguration<BbkBloodUnitReceipt>
    {
        public void Configure(EntityTypeBuilder<BbkBloodUnitReceipt> builder)
        {
            builder.ToTable("BbkBloodUnitReceipt", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProviderRequestId).IsRequired();
            builder.Property(x => x.ReceivedQuantity).IsRequired();
            builder.Property(x => x.ReceivedAt).IsRequired();
            builder.Property(x => x.ReceivedByUserId).IsRequired();
            builder.Property(x => x.Sequence).IsRequired();

            builder.HasIndex(x => x.ProviderRequestId);
            builder.HasIndex(x => x.ReceivedAt);
            builder.HasIndex(x => new { x.ProviderRequestId, x.Sequence }).IsUnique();

            builder.HasMany(x => x.Units)
                .WithOne(x => x.Receipt!)
                .HasForeignKey(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
