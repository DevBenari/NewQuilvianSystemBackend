using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

public class TrxDrugUsageConfiguration : IEntityTypeConfiguration<TrxDrugUsage>
{
    public void Configure(EntityTypeBuilder<TrxDrugUsage> builder)
    {
        builder.ToTable("TrxDrugUsage", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UsageNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.CancelReason).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.UsageNumber).IsUnique();

        // Pertanyaan yang paling sering diajukan: apa saja yang dipakai pasien ini.
        builder.HasIndex(x => new { x.EncounterId, x.UsedAt });

        // Billing menyapu menurut status untuk mencari yang belum ditagihkan.
        builder.HasIndex(x => new { x.Status, x.UsedAt });
        builder.HasIndex(x => new { x.StorageLocationId, x.UsedAt });

        builder.HasOne(x => x.Encounter).WithMany()
            .HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StorageLocation).WithMany()
            .HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecordedByWorkforce).WithMany()
            .HasForeignKey(x => x.RecordedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxDrugUsageItemConfiguration : IEntityTypeConfiguration<TrxDrugUsageItem>
{
    public void Configure(EntityTypeBuilder<TrxDrugUsageItem> builder)
    {
        builder.ToTable("TrxDrugUsageItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DrugCodeSnapshot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DrugNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MeasurementNameSnapshot).HasMaxLength(50);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.Quantity).HasColumnType("numeric(18,3)");

        // Satu obat boleh muncul lebih dari sekali pada satu pemakaian, karena obat yang sama
        // dapat diberikan pada waktu dan dosis berbeda dalam satu pencatatan. Karena itu
        // TIDAK ada indeks unik atas pasangan pemakaian dan obat di sini — berbeda dari
        // permintaan dan transfer, yang menyiapkan barang sekali untuk satu kebutuhan.
        builder.HasIndex(x => x.DrugUsageId);
        builder.HasIndex(x => x.DrugId);

        builder.HasOne(x => x.DrugUsage).WithMany(x => x.Items)
            .HasForeignKey(x => x.DrugUsageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Measurement).WithMany()
            .HasForeignKey(x => x.MeasurementId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxDrugUsageAllocationConfiguration : IEntityTypeConfiguration<TrxDrugUsageAllocation>
{
    public void Configure(EntityTypeBuilder<TrxDrugUsageAllocation> builder)
    {
        builder.ToTable("TrxDrugUsageAllocation", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasColumnType("numeric(18,3)");

        builder.HasIndex(x => new { x.DrugUsageItemId, x.SequenceNumber });

        // Penarikan obat bertanya "batch ini sudah sampai ke siapa saja"; indeks inilah yang
        // membuat pertanyaan itu terjawab cepat.
        builder.HasIndex(x => x.DrugBatchId);

        builder.HasOne(x => x.DrugUsageItem).WithMany(x => x.Allocations)
            .HasForeignKey(x => x.DrugUsageItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.DrugBatch).WithMany()
            .HasForeignKey(x => x.DrugBatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
