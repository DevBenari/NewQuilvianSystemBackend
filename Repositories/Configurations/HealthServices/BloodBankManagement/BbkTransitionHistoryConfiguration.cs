using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>Pemetaan riwayat perpindahan status Bank Darah, append-only.</summary>
    /// <remarks>
    /// Keempat index mengikuti kamus data kontrak <c>v4</c>: <c>Scope</c>, <c>EntityId</c>,
    /// <c>OccurredAt</c>, dan <c>CorrelationId</c>. Pembacaan "tunjukkan riwayat entity ini"
    /// dilayani index <c>EntityId</c>, yang sangat selektif karena berupa GUID.
    ///
    /// <b><c>ReasonCode</c> tidak dipasangi FK fisik ke <c>MstBloodBankReason</c>.</b> Kamus
    /// data menyebut relasinya, tetapi FK ke kolom yang bukan kunci utama menuntut alternate
    /// key baru pada <c>MstBloodBankReason</c> — perubahan schema pada tabel lain — dan
    /// preseden modul ini, <c>BbkBloodGroupConflictResolution.ReasonCode</c> pada
    /// <c>BE-BD-005</c>, memilih hal yang sama. Keberadaan kodenya dijaga service
    /// (<c>VAL-BD-016</c>), dan teksnya disalin ke <c>ReasonNote</c> sehingga riwayat tetap
    /// terbaca walau data induknya kelak disunting.
    ///
    /// <b>Tidak ada FK ke keempat entity ber-scope</b>, dan itu disengaja: satu kolom
    /// <c>EntityId</c> menunjuk empat tabel berbeda tergantung <c>Scope</c>, sehingga FK
    /// sungguhan tidak dapat dibentuk tanpa memecah tabel ini menjadi empat. Kamus data
    /// kontrak <c>v4</c> menetapkan bentuk satu tabel.
    /// </remarks>
    public class BbkTransitionHistoryConfiguration : IEntityTypeConfiguration<BbkTransitionHistory>
    {
        public void Configure(EntityTypeBuilder<BbkTransitionHistory> builder)
        {
            builder.ToTable("BbkTransitionHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Scope).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EntityId).IsRequired();
            builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
            builder.Property(x => x.FromStatus).HasMaxLength(30);
            builder.Property(x => x.ToStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ReasonCode).HasMaxLength(30);
            builder.Property(x => x.ReasonNote).HasMaxLength(500);
            builder.Property(x => x.ActorUserId).IsRequired();
            builder.Property(x => x.OccurredAt).IsRequired();

            builder.HasIndex(x => x.Scope);
            builder.HasIndex(x => x.EntityId);
            builder.HasIndex(x => x.OccurredAt);
            builder.HasIndex(x => x.CorrelationId);
        }
    }
}
