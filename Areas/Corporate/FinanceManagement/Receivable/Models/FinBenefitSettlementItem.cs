using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Satu kartu piutang yang ditutup oleh sebuah pelunasan internal (FIN-DES-102). Dibuat bersama
/// induknya; tidak pernah disunting sendiri.
///
/// BE-FIN-092 — temuan dicatat, bukan didiamkan: kamus data menetapkan ReceivableId "unique
/// terfilter untuk induk yang Status &lt;&gt; DIBATALKAN", tetapi filter partial index Postgres
/// hanya boleh merujuk kolom pada tabel yang sama — Status ada di FinBenefitSettlement (induk),
/// bukan di tabel ini. Constraint database untuk invarian "satu kartu piutang MUST NOT ditutup dua
/// kali oleh pelunasan yang tidak dibatalkan" TIDAK dapat dibuat sebagai partial index lintas tabel
/// pada EF Core/Postgres. Index biasa (non-unique) dipasang untuk performa kueri; penegakan
/// invarian lintas tabel ini MUST dikerjakan di layer service (FinanceBenefitSettlementService,
/// BE-FIN-099) lewat penguncian, mengikuti pola AcquireLockAsync yang sudah berjalan di
/// FinanceBillingIntakeService — bukan diselesaikan oleh task ini.
/// </summary>
[Table("FinBenefitSettlementItem", Schema = "public")]
public sealed class FinBenefitSettlementItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SettlementId { get; set; }
    public FinBenefitSettlement? Settlement { get; set; }

    /// <summary>Kartu piutang yang ditutup. FinReceivable.cs tidak diberi collection balik — pola
    /// yang sama dengan FinReceivableMovement.Receivable.</summary>
    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    public decimal Amount { get; set; }
}
