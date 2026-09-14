using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan permintaan darah ke PMI, aggregate root <c>BD-AGG-02</c>.</summary>
    /// <remarks>
    /// <para>
    /// <b>Index unik <c>RequestNumber</c></b> adalah penjaga terakhir keunikan nomor bisnis
    /// (<c>QBE-CODE-004</c>).
    /// </para>
    /// <para>
    /// <b>Index unik terfilter <c>IX_BbkProviderRequest_BloodOrderId_Active</c> adalah penjaga
    /// fisik <c>BD-XINV-02</c></b>: satu order tidak dapat punya dua permintaan yang masih berjalan
    /// (<c>Requested</c> atau <c>PartiallyFulfilled</c>). Service sudah memeriksanya di bawah kunci
    /// penasihat; index ini memastikan jaminan itu tetap berlaku walau ada jalur tulis lain.
    /// Permintaan yang sudah terpenuhi, dibatalkan, atau ditutup tidak ikut terhitung, sehingga
    /// order yang permintaannya dibatalkan tetap dapat dibuatkan permintaan baru.
    /// </para>
    /// <para>
    /// Seluruh FK memakai <c>Restrict</c>: permintaan adalah rekam pasokan yang wajib tetap
    /// terbaca walaupun baris hulunya kelak dirapikan.
    /// </para>
    /// </remarks>
    public class BbkProviderRequestConfiguration : IEntityTypeConfiguration<BbkProviderRequest>
    {
        public void Configure(EntityTypeBuilder<BbkProviderRequest> builder)
        {
            builder.ToTable("BbkProviderRequest", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.BloodOrderId).IsRequired();
            builder.Property(x => x.PatientId).IsRequired();
            builder.Property(x => x.RequestStatus).HasConversion<int>().IsRequired();
            // Concurrency token sungguhan: UPDATE hanya berhasil bila versi di database masih sama
            // dengan yang dibaca. Inilah yang menjaga sisa permintaan tidak pernah negatif.
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.RequestNumber).IsUnique();
            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.RequestStatus);

            // Index biasa pada BloodOrderId mengikuti kamus data. Index terfilter di bawah hanya
            // memuat permintaan yang masih berjalan, sehingga tidak melayani pembacaan
            // "seluruh permintaan milik order ini".
            builder.HasIndex(x => x.BloodOrderId, "IX_BbkProviderRequest_BloodOrderId");

            builder.HasIndex(x => x.BloodOrderId)
                .IsUnique()
                .HasDatabaseName("IX_BbkProviderRequest_BloodOrderId_Active")
                .HasFilter("\"RequestStatus\" IN (0, 1) AND \"IsDelete\" = false");

            builder.HasOne(x => x.BloodOrder)
                .WithMany()
                .HasForeignKey(x => x.BloodOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Receipts)
                .WithOne(x => x.ProviderRequest!)
                .HasForeignKey(x => x.ProviderRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Units)
                .WithOne(x => x.ProviderRequest!)
                .HasForeignKey(x => x.ProviderRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
