using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan alokasi kantong darah, <c>BD-AGG-03</c> / <c>BE-BD-006</c>.</summary>
    /// <remarks>
    /// <para>
    /// <b>Index unik terfilter <see cref="ActiveUnitIndexName"/></b> mengikuti kamus data kontrak
    /// <c>v4</c>: <c>(BloodUnitId) WHERE "AllocationStatus" = 0</c>. Inilah penjaga sebenarnya
    /// invariant "satu kantong tidak pernah punya dua alokasi aktif" (<c>INV-BD-019</c>,
    /// <c>VAL-BD-018c</c>). Baris yang sudah dibatalkan tetap tersimpan dengan
    /// <c>AllocationStatus = 1</c> dan berada di luar filter, sehingga unique polos <b>tidak</b>
    /// dapat dipakai — pola yang sama persis dengan penempatan berlaku pada <c>BE-BD-015</c>
    /// (<c>ARCH-BD-POS-03</c>).
    /// </para>
    /// <para>
    /// <b>Angka <c>0</c> pada filter disengaja harfiah</b>, mengikuti SQL kamus data. Ia mengikat
    /// <c>BbkAllocationStatus.Active</c> tetap bernilai <c>0</c>; catatan itu juga ditulis pada
    /// enum-nya supaya penjaganya tidak diam-diam berpindah makna.
    /// </para>
    /// <para>
    /// <b>Index biasa <c>BloodUnitId</c></b> tetap ada untuk membaca seluruh riwayat alokasi satu
    /// kantong — termasuk yang sudah dibatalkan; index terfilter hanya memuat baris yang berlaku.
    /// </para>
    /// <para>
    /// Seluruh FK memakai <c>Restrict</c>: alokasi adalah rekam klinis yang asalnya tidak boleh
    /// putus, dan baris order yang pernah dialokasikan tidak boleh hilang dari bawah riwayatnya.
    /// </para>
    /// <para>
    /// <b><c>CancelReasonCode</c> sengaja bukan foreign key.</b> Kamus data menuliskannya sebagai
    /// relasi ke <c>MstBloodBankReason.ReasonCode</c>, tetapi kolom itu bukan primary key pada
    /// master tersebut, dan kode alasan disimpan sebagai salinan yang maknanya tidak boleh berubah
    /// ketika master disunting (<c>INV-BD-035</c>). Kesahihan kodenya ditegakkan service saat
    /// pembatalan — pola yang sama dengan <c>BbkTransitionHistory.ReasonCode</c>, yang juga tidak
    /// ber-FK.
    /// </para>
    /// </remarks>
    public class BbkBloodUnitAllocationConfiguration : IEntityTypeConfiguration<BbkBloodUnitAllocation>
    {
        public const string ActiveUnitIndexName = "IX_BbkBloodUnitAllocation_ActiveUnit";

        public void Configure(EntityTypeBuilder<BbkBloodUnitAllocation> builder)
        {
            builder.ToTable("BbkBloodUnitAllocation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BloodUnitId).IsRequired();
            builder.Property(x => x.BloodOrderLineId).IsRequired();
            builder.Property(x => x.AllocationStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.AllocatedByUserId).IsRequired();
            builder.Property(x => x.AllocatedAt).IsRequired();
            builder.Property(x => x.CancelReasonCode).HasMaxLength(30);
            builder.Property(x => x.CancelReasonNote).HasMaxLength(500);

            builder.HasIndex(x => x.BloodUnitId, "IX_BbkBloodUnitAllocation_BloodUnitId");

            builder.HasIndex(x => x.BloodUnitId, ActiveUnitIndexName)
                .IsUnique()
                .HasFilter("\"AllocationStatus\" = 0");

            builder.HasIndex(x => x.BloodOrderLineId);
            builder.HasIndex(x => x.AllocationStatus);

            builder.HasOne(x => x.BloodUnit)
                .WithMany()
                .HasForeignKey(x => x.BloodUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.BloodOrderLine)
                .WithMany()
                .HasForeignKey(x => x.BloodOrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
