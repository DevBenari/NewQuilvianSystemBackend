using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

public class TrxDrugReturnConfiguration : IEntityTypeConfiguration<TrxDrugReturn>
{
    public void Configure(EntityTypeBuilder<TrxDrugReturn> builder)
    {
        builder.ToTable("TrxDrugReturn", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.DecisionReason).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.ReturnNumber).IsUnique();
        builder.HasIndex(x => new { x.EncounterId, x.ReturnedAt });

        // Pemeriksa menyapu menurut status untuk menemukan retur yang menunggu diperiksa.
        builder.HasIndex(x => new { x.Status, x.ReturnedAt });
        builder.HasIndex(x => new { x.StorageLocationId, x.Status });

        builder.HasOne(x => x.Encounter).WithMany()
            .HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StorageLocation).WithMany()
            .HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReturnedByWorkforce).WithMany()
            .HasForeignKey(x => x.ReturnedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.VerifiedByWorkforce).WithMany()
            .HasForeignKey(x => x.VerifiedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SourceDrugUsage).WithMany()
            .HasForeignKey(x => x.SourceDrugUsageId).OnDelete(DeleteBehavior.Restrict);
        // Sengaja tanpa properti navigasi: konfigurasi Farmasi tidak perlu mengenal tipe
        // milik Operasi untuk dapat menyimpan asal returnya.
        builder.HasIndex(x => x.SourceOprMaterialUsageId);
    }
}

public class TrxDrugReturnItemConfiguration : IEntityTypeConfiguration<TrxDrugReturnItem>
{
    public void Configure(EntityTypeBuilder<TrxDrugReturnItem> builder)
    {
        builder.ToTable("TrxDrugReturnItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DrugCodeSnapshot).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DrugNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MeasurementNameSnapshot).HasMaxLength(50);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.Quantity).HasColumnType("numeric(18,3)");
        builder.Property(x => x.AcceptedQuantity).HasColumnType("numeric(18,3)");

        // Satu batch hanya boleh disebut sekali dalam satu retur; dua baris batch yang sama
        // membuat pemeriksa menghitung barang yang sama dua kali.
        builder.HasIndex(x => new { x.DrugReturnId, x.DrugBatchId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        builder.HasIndex(x => x.DrugBatchId);

        builder.HasOne(x => x.DrugReturn).WithMany(x => x.Items)
            .HasForeignKey(x => x.DrugReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Drug).WithMany()
            .HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DrugBatch).WithMany()
            .HasForeignKey(x => x.DrugBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Measurement).WithMany()
            .HasForeignKey(x => x.MeasurementId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TrxDrugReturnHistoryConfiguration : IEntityTypeConfiguration<TrxDrugReturnHistory>
{
    public void Configure(EntityTypeBuilder<TrxDrugReturnHistory> builder)
    {
        builder.ToTable("TrxDrugReturnHistory", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Source).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        builder.HasIndex(x => new { x.DrugReturnId, x.OccurredAt });

        builder.HasIndex(x => new { x.Action, x.CorrelationId })
            .IsUnique()
            .HasFilter("\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

        builder.HasOne(x => x.DrugReturn).WithMany()
            .HasForeignKey(x => x.DrugReturnId).OnDelete(DeleteBehavior.Cascade);
    }
}
