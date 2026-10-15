using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

public class PhmPrescriptionStockReservationConfiguration
    : IEntityTypeConfiguration<PhmPrescriptionStockReservation>
{
    public void Configure(EntityTypeBuilder<PhmPrescriptionStockReservation> builder)
    {
        builder.ToTable("PhmPrescriptionStockReservation", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.ReleaseReasonCode).HasMaxLength(64);
        builder.Property(x => x.SourceEvent).HasMaxLength(64);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(128);

        // Pembacaan utama selalu bertanya "apa penahanan resep ini", baik oleh Billing sebelum
        // melanjutkan, oleh Farmasi sebelum memproses, maupun oleh layar status.
        builder.HasIndex(x => new { x.PrescriptionId, x.State });

        // Penyapu kedaluwarsa menyapu menurut batas waktu, bukan menurut resep. Saringannya
        // membuang baris yang tidak mungkin kedaluwarsa — yang sudah dikonsumsi, dilepas, atau
        // tidak berbatas waktu — supaya indeksnya tetap kecil.
        builder.HasIndex(x => x.ExpiresAt)
            .HasFilter("\"State\" = 1 AND \"ExpiresAt\" IS NOT NULL AND \"IsDelete\" = false");

        // Satu batch pada satu baris resep hanya boleh ditahan satu kali selagi aktif. Inilah
        // penjaga terakhir terhadap penahanan ganda, di samping pemeriksaan idempotency di
        // service: bila dua request lolos bersamaan, basis data yang menolak yang kedua.
        builder.HasIndex(x => new { x.PrescriptionId, x.PrescriptionItemId, x.DrugBatchId })
            .IsUnique()
            .HasFilter("\"State\" = 1 AND \"PrescriptionItemId\" IS NOT NULL AND \"IsDelete\" = false");

        builder.HasIndex(x => new { x.PrescriptionId, x.PrescriptionCompoundItemId, x.DrugBatchId })
            .IsUnique()
            .HasFilter("\"State\" = 1 AND \"PrescriptionCompoundItemId\" IS NOT NULL AND \"IsDelete\" = false");

        builder.HasIndex(x => x.IdempotencyKey);

        builder.HasOne(x => x.Prescription)
            .WithMany()
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PrescriptionItem)
            .WithMany()
            .HasForeignKey(x => x.PrescriptionItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Drug)
            .WithMany()
            .HasForeignKey(x => x.DrugId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DrugBatch)
            .WithMany()
            .HasForeignKey(x => x.DrugBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StorageLocation)
            .WithMany()
            .HasForeignKey(x => x.StorageLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
