using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan order darah, aggregate root <c>BD-AGG-01</c>.</summary>
    /// <remarks>
    /// <para>
    /// <b>Index unik pada <c>OrderNumber</c> adalah penjaga terakhir keunikan nomor bisnis.</b>
    /// Alokator sudah menjamin keunikan dari sisi pencacah, tetapi jaminan itu hidup di tabel
    /// deret — bukan di tabel ini. Index inilah yang membuat dua order tidak mungkin menyandang
    /// nomor sama walau apa pun yang terjadi di antaranya.
    /// </para>
    /// <para>
    /// <b>Index gabungan <c>(PatientId, OrderStatus)</c> melayani pertanyaan terpanas modul
    /// ini</b>: "apakah pasien ini masih punya order aktif". Pertanyaan itu diajukan pada
    /// setiap pembuatan order lewat deteksi ganda <c>BD-XINV-01</c>, sehingga jalur bacanya
    /// tidak boleh memindai tabel.
    /// </para>
    /// <para>
    /// Seluruh FK memakai <c>Restrict</c>: order darah adalah rekam klinis yang wajib tetap
    /// terbaca walaupun baris hulunya kelak dirapikan.
    /// </para>
    /// </remarks>
    public class BbkBloodOrderConfiguration : IEntityTypeConfiguration<BbkBloodOrder>
    {
        public void Configure(EntityTypeBuilder<BbkBloodOrder> builder)
        {
            builder.ToTable("BbkBloodOrder", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PatientId).IsRequired();
            builder.Property(x => x.EncounterId).IsRequired();
            builder.Property(x => x.ServiceUnitId).IsRequired();
            builder.Property(x => x.RequestingDoctorId).IsRequired();
            builder.Property(x => x.OrderSource).HasConversion<int>().IsRequired();
            builder.Property(x => x.OrderStatus).HasConversion<int>().IsRequired();
            // Concurrency token sungguhan: UPDATE hanya berhasil bila versi di database masih
            // sama dengan yang dibaca. Tanpa ini, dua pembatalan serentak sama-sama tersimpan.
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.OrderNumber).IsUnique();
            builder.HasIndex(x => new { x.PatientId, x.OrderStatus });
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.ServiceUnitId);
            builder.HasIndex(x => x.RequestingDoctorId);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestingDoctor)
                .WithMany()
                .HasForeignKey(x => x.RequestingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Lines)
                .WithOne(x => x.BloodOrder!)
                .HasForeignKey(x => x.BloodOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
