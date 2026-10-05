using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

/// <summary>
/// Ambang nilai pembayaran langsung piutang dan utang supplier (BE-FIN-074, FIN-DES-086, FIN-DEC-134).
/// Master berjejak dengan satu baris aktif — BUKAN <c>appsettings.json</c>, karena ambang MUST dapat
/// diubah pejabat berwenang beserta alasan dan jejak audit (<see cref="IdentityModel"/> menjawab
/// siapa dan kapan; perubahannya dicatat logger seperti Update lain).
///
/// Tabel riwayat perubahan ambang SENGAJA TIDAK dibuat — kolom audit dan logger sudah menjawab
/// perubahan terakhir (FIN-DES-086).
///
/// Tanpa baris aktif, SELURUH pembayaran langsung ditolak — perilaku yang disengaja, bukan dianggap
/// tak terbatas. Nilai awalnya belum ditetapkan desain ini (FIN-OQ-074).
///
/// BE-FIN-086 (FIN-DES-094, FIN-DEC-145/146/153): ambang SELALU berlaku seketika, sehingga kolom
/// <c>EffectiveFrom</c> dibuang — ia menjanjikan perubahan terjadwal yang tidak pernah ditegakkan
/// siapa pun. Penggantinya BUKAN kolom lain: tanggal perubahan terakhir dibaca dari kolom audit
/// <see cref="IdentityModel"/>.
/// </summary>
[Table("MstDirectPaymentThreshold", Schema = "public")]
public sealed class MstDirectPaymentThreshold : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Ambang rupiah; berlaku sama untuk piutang dan utang (FIN-DES-086).</summary>
    public decimal Amount { get; set; }

    /// <summary>Wajib diisi setiap kali ambang diubah (FIN-DEC-134).</summary>
    [Required, MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>Hanya satu baris aktif pada satu waktu — dijaga unique index parsial.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Penanda versi optimistic concurrency (BE-FIN-086, FIN-DEC-146, FIN-VAL-228). Mengikuti pola
    /// <c>FinOpeningBalance</c> dan <c>FinOpeningItemBatch</c>: <see cref="Guid"/> yang diputar service
    /// pada setiap penyimpanan, bukan <c>byte[]</c> xmin PostgreSQL.
    ///
    /// Gunanya: dua pejabat yang sama-sama membuka layar ambang tidak saling menimpa diam-diam.
    /// Penyimpanan yang membawa versi basi dijawab 409, BUKAN ditimpa.
    /// </summary>
    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
