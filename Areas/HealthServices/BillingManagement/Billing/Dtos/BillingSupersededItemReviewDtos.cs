using System;
using System.Collections.Generic;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>
/// Parameter query pencarian dan penyaringan kandidat tinjauan void biaya administrasi (BKC-DES-089, BIL-API-2.0 Bagian B).
/// </summary>
public class SupersededItemReviewListQuery
{
    /// <summary>
    /// Status tinjauan: PENDING (default, belum ditinjau), DECIDED (sudah diputuskan), atau ALL (semua kandidat).
    /// </summary>
    public string? ReviewState { get; set; } = "PENDING";

    /// <summary>
    /// Filter status invoice (mis. OPEN, FINAL, CLOSED, SETTLED_BY_WRITE_OFF).
    /// </summary>
    public string? InvoiceStatus { get; set; }

    /// <summary>
    /// Pencarian teks berdasarkan nomor invoice.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Nomor halaman (bawaan 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Ukuran per halaman (bawaan 20, maksimal 100).
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Data item kandidat tinjauan void biaya administrasi yang keliru (BKC-DES-089, BIL-API-2.0 Bagian B).
/// </summary>
public class SupersededItemReviewCandidateResponse
{
    public Guid InvoiceItemId { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceStatus { get; set; } = string.Empty;
    public Guid InvoiceRowVersion { get; set; }
    public Guid EncounterId { get; set; }
    public DateTime? EncounterDate { get; set; }

    /// <summary>
    /// Nama pasien. Data sensitif — hanya untuk tampilan antarmuka kasir, DILARANG masuk log audit.
    /// </summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Nomor Rekam Medis (No RM). Data sensitif — hanya untuk tampilan antarmuka kasir, DILARANG masuk log audit.
    /// </summary>
    public string MedicalRecordNumber { get; set; } = string.Empty;

    public string ItemDescription { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset? VoidedAt { get; set; }

    /// <summary>
    /// Menunjukkan apakah kunjungan rawat jalan ini bertautan resmi ke invoice rawat inap aktif.
    /// </summary>
    public bool IsLinkedToInpatient { get; set; }

    /// <summary>
    /// Menunjukkan apakah kategori item adalah biaya administrasi (MstTariffCategory.IsAdministrationFee).
    /// </summary>
    public bool IsAdministrationFeeCategory { get; set; }

    /// <summary>
    /// Daftar opsi keputusan yang sah untuk kondisi invoice saat ini (REINSTATE, KEEP, NEEDS_POST_CLOSE_CORRECTION).
    /// </summary>
    public List<string> AllowedDecisions { get; set; } = new();

    /// <summary>
    /// Hasil keputusan jika item sudah pernah ditinjau, atau null jika masih PENDING.
    /// </summary>
    public SupersededItemReviewResponse? Review { get; set; }
}

/// <summary>
/// Payload permohonan penetapan keputusan tinjauan item (BKC-DES-089, BIL-API-2.0 Bagian B).
/// </summary>
public class DecideSupersededItemRequest
{
    /// <summary>
    /// Keputusan: REINSTATE, KEEP, atau NEEDS_POST_CLOSE_CORRECTION.
    /// </summary>
    public string Decision { get; set; } = string.Empty;

    /// <summary>
    /// Alasan keputusan (wajib, 10–500 karakter setelah trim, BIL-VAL-151). Dilarang memuat PII/nama pasien.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optimistic concurrency check: RowVersion invoice yang diharapkan (BIL-VAL-157).
    /// </summary>
    public Guid ExpectedInvoiceRowVersion { get; set; }
}

/// <summary>
/// Data hasil penetapan keputusan tinjauan item (BKC-DES-089, BIL-API-2.0 Bagian B).
/// </summary>
public class SupersededItemReviewResponse
{
    public Guid ReviewId { get; set; }
    public Guid InvoiceItemId { get; set; }
    public Guid InvoiceId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string InvoiceStatusAtDecision { get; set; } = string.Empty;
    public Guid? ReinstatedInvoiceItemId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid DecidedBy { get; set; }
    public string DecidedByName { get; set; } = string.Empty;
    public DateTimeOffset DecidedAt { get; set; }
    public bool IsReplay { get; set; }
}
