using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

/// <summary>
/// Salinan keadaan finansial resep (PHA-DES-001, PHA-DEC-071).
/// </summary>
public class PrescriptionFinancialProjectionConfiguration
    : IEntityTypeConfiguration<PhmPrescriptionFinancialProjection>
{
    public void Configure(EntityTypeBuilder<PhmPrescriptionFinancialProjection> builder)
    {
        builder.ToTable("PhmPrescriptionFinancialProjection", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClearanceStatus)
            .HasMaxLength(25)
            .HasDefaultValue(PrescriptionClearanceProjectionStatuses.Unknown)
            .IsRequired();

        builder.Property(x => x.FinancialOutcome).HasMaxLength(30);
        builder.Property(x => x.ReasonCode).HasMaxLength(40);
        builder.Property(x => x.FinancialVersion).HasDefaultValue(0L).IsRequired();

        builder.Property(x => x.SyncState)
            .HasMaxLength(30)
            .HasDefaultValue(PrescriptionClearanceSyncStates.NeverSynced)
            .IsRequired();

        builder.Property(x => x.RetryCount).HasDefaultValue(0).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);

        builder.Property(x => x.EffectiveAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.SyncedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.LastAttemptAt).HasColumnType("timestamp with time zone");

        // Kendali konkurensi optimistik. Dua surat untuk resep yang sama yang diproses
        // bersamaan menjadi tepat satu baris, dan yang kalah mengulang bacaannya
        // (PHA-AT-CLR-11).
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        // Tepat satu baris per resep. Unik tanpa filter: baris yang ditandai terhapus pun
        // tidak boleh membuka jalan bagi baris kedua, karena satu resep hanya punya satu
        // keadaan finansial.
        builder.HasIndex(x => x.PrescriptionId).IsUnique();

        // Sapuan "resep mana saja yang sedang tidak boleh dikerjakan" pada layar kerja.
        builder.HasIndex(x => x.ClearanceStatus);

        // Sapuan rekonsiliasi mencari salinan yang gagal disinkronkan.
        builder.HasIndex(x => x.SyncState);

        builder.HasIndex(x => x.InvoiceId);

        builder.HasOne(x => x.Prescription).WithMany()
            .HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
    }
}
