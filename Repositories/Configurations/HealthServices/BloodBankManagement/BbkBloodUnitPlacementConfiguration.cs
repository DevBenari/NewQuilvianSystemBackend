using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan riwayat penempatan kantong, <c>BD-DOM-25</c>.</summary>
    /// <remarks>
    /// <para>
    /// <b>Index unik terfilter <see cref="CurrentUnitIndexName"/></b> mengikuti kamus data:
    /// <c>(BloodUnitId) WHERE "IsCurrent" = true</c>. Satu kantong tidak pernah punya dua penempatan
    /// yang berlaku, walaupun dua petugas memindahkannya hampir bersamaan. Penempatan lama tetap
    /// tersimpan dengan <c>IsCurrent = false</c>, sehingga unique polos tidak dapat dipakai — pola yang
    /// sama dengan alokasi aktif (<c>ARCH-BD-POS-03</c>).
    /// </para>
    /// <para>
    /// <b>Index biasa <c>BloodUnitId</c></b> tetap ada untuk membaca seluruh riwayat satu kantong;
    /// index terfilter hanya memuat baris yang berlaku.
    /// </para>
    /// <para>
    /// Seluruh FK memakai <c>Restrict</c>: riwayat penempatan wajib tetap terbaca, termasuk ketika
    /// lokasinya sudah dinonaktifkan.
    /// </para>
    /// </remarks>
    public class BbkBloodUnitPlacementConfiguration : IEntityTypeConfiguration<BbkBloodUnitPlacement>
    {
        public const string CurrentUnitIndexName = "IX_BbkBloodUnitPlacement_CurrentUnit";

        public void Configure(EntityTypeBuilder<BbkBloodUnitPlacement> builder)
        {
            builder.ToTable("BbkBloodUnitPlacement", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BloodUnitId).IsRequired();
            builder.Property(x => x.StorageLocationId).IsRequired();
            builder.Property(x => x.PlacedAt).IsRequired();
            builder.Property(x => x.PlacedByUserId).IsRequired();
            builder.Property(x => x.IsCurrent).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => x.BloodUnitId, "IX_BbkBloodUnitPlacement_BloodUnitId");

            builder.HasIndex(x => x.BloodUnitId, CurrentUnitIndexName)
                .IsUnique()
                .HasFilter("\"IsCurrent\" = true");

            builder.HasIndex(x => x.StorageLocationId);
            builder.HasIndex(x => x.PreviousPlacementId);
            builder.HasIndex(x => x.PlacedAt);

            builder.HasOne(x => x.BloodUnit)
                .WithMany()
                .HasForeignKey(x => x.BloodUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.StorageLocation)
                .WithMany()
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreviousPlacement)
                .WithMany()
                .HasForeignKey(x => x.PreviousPlacementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
