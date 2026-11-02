using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;

public sealed class HrdBenefitSettlementApprovalQuery
{
    /// <summary>Nomor dokumen atau nama pemilik benefit (karyawan/dokter). Dicari di server pada seluruh data.</summary>
    public string? Search { get; set; }

    /// <summary>Status persetujuan HRD (bukan status dokumen Finance). Kosong atau "all" = semua.</summary>
    public string? Status { get; set; }
    public string? PeriodCode { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string SortBy { get; set; } = "createdDate";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class HrdApproveSettlementRequest
{
    [Required] public Guid RowVersion { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class HrdRejectSettlementRequest
{
    [Required] public Guid RowVersion { get; set; }
    [Required, MaxLength(500)] public string RejectReason { get; set; } = string.Empty;
}

public class HrdBenefitSettlementApprovalItemResponse
{
    public Guid Id { get; set; }
    public string SettlementNumber { get; set; } = string.Empty;
    public DateTimeOffset CreatedDate { get; set; }
    public string AccountingPeriodCode { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }
    public string? DebtorName { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }

    /// <summary>Jumlah pemilik benefit yang identitas canonical-nya berhasil ditentukan (bukan hitungan nama).</summary>
    public int OwnerCount { get; set; }

    /// <summary>Jumlah kartu piutang yang pemilik benefitnya belum dapat ditentukan; tidak dihitung pada <see cref="OwnerCount"/>.</summary>
    public int UnidentifiedOwnerCount { get; set; }

    /// <summary>Status dokumen Finance: DRAF, DITERBITKAN, atau DIBATALKAN.</summary>
    public string DocumentStatus { get; set; } = string.Empty;

    /// <summary>
    /// Keputusan HRD: MENUNGGU_PERSETUJUAN, DISETUJUI, DITOLAK, TIDAK_DAPAT_DIVERIFIKASI (dokumen sudah
    /// DITERBITKAN tetapi bukti keputusan HRD tidak tercatat), atau TANPA_KEPUTUSAN (DIBATALKAN sebelum diputuskan).
    /// Tidak pernah berisi status dokumen Finance.
    /// </summary>
    public string ApprovalStatus { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class HrdBenefitSettlementApprovalDetailResponse : HrdBenefitSettlementApprovalItemResponse
{
    public string? Notes { get; set; }
    public string? ApprovalNotes { get; set; }

    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedByName { get; set; }

    /// <summary>Alasan pembatalan Finance; hanya terisi untuk dokumen DIBATALKAN.</summary>
    public string? CancelReason { get; set; }

    /// <summary>Jumlah rincian yang berhasil dipetakan pada <see cref="Items"/>.</summary>
    public int MappedItemCount { get; set; }

    /// <summary>True bila rincian tidak lengkap/tidak konsisten sehingga dokumen tidak dapat disetujui.</summary>
    public bool HasIntegrityIssue { get; set; }
    public List<string> IntegrityMessages { get; set; } = new();
    public List<HrdBenefitSettlementOwnerRowResponse> Items { get; set; } = new();
}

public sealed class HrdBenefitSettlementOwnerRowResponse
{
    public Guid ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public string? PatientName { get; set; }
    public string? PatientType { get; set; }

    /// <summary>Tanggal kunjungan; null bila kunjungan tidak dapat ditelusuri (tidak diganti tanggal lain).</summary>
    public DateTimeOffset? ServiceDate { get; set; }
    public decimal Amount { get; set; }

    // Rincian Pemilik Benefit (Karyawan / Dokter)
    public Guid? BenefitOwnerWorkforceProfileId { get; set; }
    public string? BenefitOwnerName { get; set; }
    public string? BenefitOwnerStaffNumber { get; set; }
    public string? BenefitOwnerKind { get; set; } // "Karyawan" atau "Dokter"; null bila belum teridentifikasi
    public string? BenefitPlanName { get; set; }
    public string? RelationshipCode { get; set; } // "SELF", "SPOUSE", "CHILD"; null bila tidak diketahui

    /// <summary>False bila pemilik benefit tidak dapat ditentukan dari kartu piutang maupun penjamin kunjungan.</summary>
    public bool IsOwnerIdentified { get; set; }

    // Informasi Plafon. SALDO SAAT INI dari HRD (bukan plafon historis periode dokumen).
    // Null berarti saldo tidak berhasil dibaca — berbeda dengan nilai 0.
    public bool IsBalanceAvailable { get; set; }
    public string? BalanceNote { get; set; }
    public decimal? CoverageLimitAmount { get; set; }
    public decimal? UsedAmount { get; set; }
    public decimal? RemainingAmount { get; set; }

    /// <summary>Nominal baris ini (kartu piutang ini saja).</summary>
    public decimal ProposedAmount { get; set; }

    /// <summary>Total seluruh baris pada dokumen ini yang memakai sumber saldo yang sama.</summary>
    public decimal OwnerTotalProposedAmount { get; set; }

    /// <summary>Sisa saldo dikurangi <see cref="OwnerTotalProposedAmount"/>; null bila saldo tidak terbaca.</summary>
    public decimal? ProjectedRemainingAmount { get; set; }
    public bool? IsSufficient { get; set; }
}
