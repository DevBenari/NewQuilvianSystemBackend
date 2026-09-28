using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Dtos;

public static class PettyCashBudgetAdjustmentDirections
{
    public const string Increase = "INCREASE";
    public const string Decrease = "DECREASE";
    public static readonly IReadOnlySet<string> All = new HashSet<string>([Increase, Decrease], StringComparer.OrdinalIgnoreCase);
}

public sealed class PettyCashBudgetMovementQuery
{
    public string? MovementType { get; set; }

    /// <summary>Kosong berarti periode yang sedang Aktif (perilaku lama, dipertahankan).
    /// Diisi untuk melihat riwayat periode manapun, termasuk yang sudah Ditutup
    /// (PC-DES-017, BE-BKC-054).</summary>
    public Guid? BudgetId { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class PettyCashBudgetAmountRequestBase
{
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid ExpectedRowVersion { get; set; }
}

public class PettyCashBudgetTopUpRequest : PettyCashBudgetAmountRequestBase
{
    /// <summary>Sumber dana penambahan anggaran: TRANSFER atau CASH (PC-DES-026).
    /// TRANSFER = dana masuk melalui transfer bank; CASH = dana masuk secara tunai/langsung.</summary>
    [Required, MaxLength(30)] public string FundingSourceType { get; set; } = string.Empty;

    /// <summary>Nomor referensi transfer (contoh: nomor bukti transfer BCA/BNI/dll).
    /// Wajib bila FundingSourceType = TRANSFER. Boleh kosong bila FundingSourceType = CASH.</summary>
    [MaxLength(100)] public string? TransferReference { get; set; }
}

public sealed class PettyCashBudgetAdjustmentRequest : PettyCashBudgetAmountRequestBase
{
    [Required, MaxLength(20)] public string Direction { get; set; } = string.Empty;
}

/// <summary>PC-DES-017. PoolCode dibaca dari periode yang sedang Aktif bila dikosongkan
/// (satu kolam untuk seluruh rumah sakit pada MVP ini, PC-DEC-010).</summary>
public sealed class CreatePettyCashBudgetPeriodRequest
{
    [MaxLength(30)] public string? PoolCode { get; set; }
    [MaxLength(100)] public string? PoolName { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal BudgetAmount { get; set; }
}

public sealed class ActivatePettyCashBudgetPeriodRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

/// <summary>PC-DES-018. SuccessorBudgetId wajib diisi hanya bila periode yang ditutup masih
/// bersaldo — ditegakkan di service, bukan data annotation, karena syaratnya bersyarat.</summary>
public sealed class ClosePettyCashBudgetPeriodRequest
{
    public Guid? SuccessorBudgetId { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class PettyCashBudgetPeriodQuery
{
    public string? Status { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class PettyCashBudgetResponse
{
    public Guid Id { get; set; }
    public string PoolCode { get; set; } = string.Empty;
    public string PoolName { get; set; } = string.Empty;

    /// <summary>Saldo saat ini pada kolam kas kecil.</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>Pemakaian Saldo (Terminologi Baru, menggantikan Pemakaian Periode / TotalDisbursedAmount).</summary>
    public decimal PemakaianSaldo { get; set; }

    /// <summary>Pemakaian Saldo (English naming: DisbursedBalance).</summary>
    public decimal DisbursedBalance { get; set; }

    /// <summary>Pemakaian Saldo (properti lama untuk kompatibilitas).</summary>
    public decimal TotalDisbursedAmount { get; set; }

    /// <summary>Sisa Saldo (Terminologi Baru, menggantikan Sisa Anggaran / RemainingBudgetAmount).
    /// Sisa Saldo = Saldo Saat Ini - Pemakaian Saldo (atau CurrentBalance net).</summary>
    public decimal SisaSaldo { get; set; }

    /// <summary>Sisa Saldo (English naming: RemainingBalance).</summary>
    public decimal RemainingBalance { get; set; }

    /// <summary>Sisa Saldo (properti lama untuk kompatibilitas).</summary>
    public decimal RemainingBudgetAmount { get; set; }

    /// <summary>PC-DES-017, BE-BKC-054 (opsional/legacy).</summary>
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public decimal BudgetAmount { get; set; }

    /// <summary>DRAFT, ACTIVE, atau CLOSED (PC-DES-017). Warisan ACTIVE/INACTIVE dari
    /// sebelum revisi 15 September 2026 dipetakan migration menjadi nilai-nilai ini.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Periode penerus penerima carry-forward. Kosong sampai baris ini benar-benar
    /// ditutup (PC-DES-018, BE-BKC-054).</summary>
    public Guid? SupersededByBudgetId { get; set; }

    /// <summary>Warisan pra-revisi 15 September 2026 (PC-DES-016).</summary>
    public decimal ReservedAmount { get; set; }

    /// <summary>Warisan, lihat catatan pada <see cref="ReservedAmount"/>.</summary>
    public decimal AvailableAmount { get; set; }

    public decimal TotalTopUpAmount { get; set; }
    public DateTimeOffset? LastMovementAt { get; set; }
    public Guid RowVersion { get; set; }
}

/// <summary>BE-BKC-058, PC-DES-025. Satu panggilan untuk seluruh kartu ringkasan halaman
/// gabungan Petty Cash — Saldo Saat Ini, Pemakaian Saldo, Sisa Saldo.</summary>
public sealed class PettyCashOverviewResponse
{
    public PettyCashBudgetResponse ActiveBudget { get; set; } = new();

    /// <summary>Saldo Saat Ini.</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>Pemakaian Saldo (Terminologi Baru: PemakaianSaldo).</summary>
    public decimal PemakaianSaldo { get; set; }

    /// <summary>Pemakaian Saldo (English naming: DisbursedBalance).</summary>
    public decimal DisbursedBalance { get; set; }

    /// <summary>Pemakaian Saldo (properti lama untuk kompatibilitas frontend).</summary>
    public decimal TotalDisbursedThisPeriod { get; set; }

    /// <summary>Sisa Saldo (Terminologi Baru: SisaSaldo).</summary>
    public decimal SisaSaldo { get; set; }

    /// <summary>Sisa Saldo (English naming: RemainingBalance).</summary>
    public decimal RemainingBalance { get; set; }

    /// <summary>Sisa Saldo (properti lama untuk kompatibilitas frontend).</summary>
    public decimal RemainingBudgetAmount { get; set; }

    /// <summary>Jumlah voucher berstatus CASH_RECEIVED (menunggu bukti).</summary>
    public int PendingEvidenceCount { get; set; }

    /// <summary>Jumlah voucher berstatus REQUESTED (menunggu pencairan).</summary>
    public int PendingDisbursementCount { get; set; }
}

public sealed class PettyCashBudgetMovementResponse
{
    public Guid Id { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? VoucherId { get; set; }
    public string? VoucherNumber { get; set; }
    public string? Reason { get; set; }
    public string? FundingSourceType { get; set; }
    public string? TransferReference { get; set; }
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

