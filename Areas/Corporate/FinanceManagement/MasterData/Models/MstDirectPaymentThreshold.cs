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

    public DateOnly EffectiveFrom { get; set; }
}
