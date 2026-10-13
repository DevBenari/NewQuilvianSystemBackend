using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>
/// Query parameter untuk Settlement AR V2 (Finance - Transaksi A/R - Pembayaran dan Pelunasan - Settlement AR).
/// Mendukung filter kategori (COMPANY, PATIENT, EMPLOYEE), pencarian teks, entitas penjamin/pasien/karyawan,
/// nomor invoice, rentang tanggal (StartDate dan EndDate), serta server-side pagination (maksimum 10 baris).
/// </summary>
public sealed class FinanceSettlementQuery
{
    /// <summary>
    /// Kategori tab debitur: "COMPANY" (Perusahaan / Asuransi), "PATIENT" (Pasien Umum), "EMPLOYEE" (Karyawan).
    /// Default: "COMPANY".
    /// </summary>
    public string Category { get; set; } = FinanceSettlementCategories.Company;

    /// <summary>
    /// Kata kunci pencarian: nomor invoice, nomor RM, nama pasien, atau nomor piutang.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter ID entitas penjamin/asuransi (untuk kategori COMPANY).
    /// </summary>
    public Guid? PayerId { get; set; }

    /// <summary>
    /// Filter ID pasien (untuk kategori PATIENT).
    /// </summary>
    public Guid? PatientId { get; set; }

    /// <summary>
    /// Filter ID karyawan penanggung benefit (untuk kategori EMPLOYEE).
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Filter nomor invoice Billing atau Invoice AR batch.
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Tanggal awal periode analisis saldo (inklusif). Menjadi patokan awal perhitungan Beginning Balance.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Tanggal akhir periode analisis saldo (inklusif). Menjadi patokan akhir perhitungan Ending Balance.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Kolom pengurutan data: "patientName", "invoiceNumber", "dueDate", "outstandingAmount", "beginningBalance", "endingBalance".
    /// </summary>
    public string SortBy { get; set; } = "patientName";

    /// <summary>
    /// Arah pengurutan: "asc" atau "desc".
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Halaman saat ini (1-indexed). Default: 1.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Ukuran halaman: dibatasi maksimal 10 baris sesuai spesifikasi UI V1.
    /// </summary>
    [Range(1, 10)]
    public int PageSize { get; set; } = 10;
}

public static class FinanceSettlementCategories
{
    public const string Company = "COMPANY";
    public const string Patient = "PATIENT";
    public const string Employee = "EMPLOYEE";
}

/// <summary>
/// Baris data tabel Settlement AR V2 (6 kolom utama mengikuti V1).
/// 1. No.
/// 2. Nama Pasien (bold) + No. RM (subtitle)
/// 3. No. Invoice (teal, clickable)
/// 4. Total Beginning Balance (Rupiah, kanan)
/// 5. Total Ending Balance (Rupiah, kanan; merah jika > 0, hijau jika == 0)
/// 6. Total Piutang (Rupiah, kanan)
/// </summary>
public sealed class FinanceSettlementRowResponse
{
    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;

    public Guid? InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid? PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string? EncounterNumber { get; set; }

    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }
    public string DebtorName { get; set; } = string.Empty;

    public Guid? BenefitOwnerId { get; set; }
    public string? BenefitOwnerName { get; set; }
    public string? BenefitRelationship { get; set; }

    /// <summary>Total Piutang (OriginalAmount)</summary>
    public decimal TotalPiutang { get; set; }

    /// <summary>Saldo tepat sebelum awal periode (atau saldo awal berjalan)</summary>
    public decimal BeginningBalance { get; set; }

    /// <summary>Saldo setelah seluruh mutasi sampai akhir periode (atau outstanding terkini)</summary>
    public decimal EndingBalance { get; set; }

    /// <summary>Sisa piutang aktual terkini dari buku FinReceivable</summary>
    public decimal OutstandingAmount { get; set; }

    public decimal AllocatedAmount { get; set; }
    public decimal AdjustedAmount { get; set; }
    public decimal WrittenOffAmount { get; set; }

    public DateOnly DueDate { get; set; }
    public DateTimeOffset? InvoiceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? LastMovementDate { get; set; }
}

/// <summary>
/// Ringkasan global Settlement AR (3 summary cards).
/// 1. Grandtotal Piutang
/// 2. Grandtotal Beginning Balance
/// 3. Grandtotal Ending Balance
/// </summary>
public sealed class FinanceSettlementSummaryResponse
{
    public string Category { get; set; } = FinanceSettlementCategories.Company;
    public int TotalReceivables { get; set; }
    public decimal GrandTotalPiutang { get; set; }
    public decimal GrandTotalBeginningBalance { get; set; }
    public decimal GrandTotalEndingBalance { get; set; }
    public decimal GrandTotalOutstanding { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public DateOnly? BalanceAsOfDate { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
}

/// <summary>
/// Kontrak respon paginasi Settlement AR.
/// </summary>
public sealed class FinanceSettlementPagedResponse
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
    public List<FinanceSettlementRowResponse> Items { get; set; } = new();
    public FinanceSettlementSummaryResponse Summary { get; set; } = new();
}

/// <summary>
/// Kontrak respon Detail Settlement AR.
/// </summary>
public sealed class FinanceSettlementDetailResponse
{
    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;

    public Guid? InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid? PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string? EncounterNumber { get; set; }

    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }
    public string DebtorName { get; set; } = string.Empty;

    public Guid? BenefitOwnerId { get; set; }
    public string? BenefitOwnerName { get; set; }
    public string? BenefitRelationship { get; set; }

    public DateTimeOffset? InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public decimal OriginalAmount { get; set; }
    public decimal BeginningBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal OutstandingAmount { get; set; }

    public decimal AllocatedAmount { get; set; }
    public decimal AdjustedAmount { get; set; }
    public decimal WrittenOffAmount { get; set; }

    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public string? Notes { get; set; }
}

