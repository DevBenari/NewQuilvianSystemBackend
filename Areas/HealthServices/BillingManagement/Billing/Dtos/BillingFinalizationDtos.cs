using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class FinalizeInvoiceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(30)] public string? DepartureReason { get; set; }
    [MaxLength(200)] public string? DebtorIdentity { get; set; }
    [MaxLength(100)] public string? DebtorRelationship { get; set; }

    /// <summary>Alasan finalisasi (opsional). Kosong disimpan sebagai string kosong karena kolom DB NOT NULL.</summary>
    [MaxLength(500)] public string? Reason { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }

    /// <summary>Pilihan pembiayaan eksplisit: EMPLOYEE_BENEFIT atau GENERAL_SELF_PAY (Section 5, 7).</summary>
    [MaxLength(50)] public string? FinancingChoice { get; set; }

    /// <summary>Nominal pembayaran langsung di kasir (Cash/Transfer).</summary>
    public decimal? DirectPayAmount { get; set; }

    /// <summary>Nominal kewajiban pegawai yang secara eksplisit ditangguhkan menjadi AR Karyawan.</summary>
    public decimal? EmployeeDeferredAmount { get; set; }

    /// <summary>BE-FIN-107, FIN-DEC-208, FIN-INTEGRATION-1.9 R.4.3: nominal pilihan pegawai untuk sisa tagihan pasien (D). Kompatibilitas ke belakang.</summary>
    public decimal? EmployeeReceivableChoiceAmount { get; set; }

    /// <summary>BE-FIN-107, BE-FIN-119, FIN-DEC-218, FIN-INTEGRATION-1.10 S.4: nama pemberi persetujuan bila pasien bukan pegawai.</summary>
    [MaxLength(200)] public string? EmployeeReceivableConsentGivenBy { get; set; }

    /// <summary>Catatan atau nomor referensi dokumen persetujuan penangguhan (Section 7).</summary>
    [MaxLength(500)] public string? EmployeeReceivableConsentNotes { get; set; }
}

/// <summary>
/// Permintaan penutupan tagihan (close bill) dengan penangguhan sisa kewajiban pasien ke Piutang Pasien
/// (Finance AR, DebtorType: PATIENT_GUARANTOR). Jalur sah dan terotorisasi terpisah dari departure exception.
/// </summary>
public sealed class FinalizeWithPatientReceivableRequest
{
    [Required]
    public Guid ExpectedRowVersion { get; set; }

    /// <summary>Alasan penangguhan piutang pasien (wajib).</summary>
    [Required, MaxLength(500)]
    public string DeferralReason { get; set; } = string.Empty;

    /// <summary>Nomor dokumen / surat pernyataan persetujuan piutang pasien (opsional).</summary>
    [MaxLength(100)]
    public string? AgreementDocumentNumber { get; set; }

    /// <summary>Tanggal jatuh tempo piutang pasien yang disetujui.</summary>
    public DateTimeOffset? DueDate { get; set; }

    /// <summary>Nama penanggung sisa tagihan bila bukan pasien sendiri (opsional; bila kosong diisi nama pasien authoritative).</summary>
    [MaxLength(200)]
    public string? GuarantorName { get; set; }

    /// <summary>Hubungan penanggung dengan pasien (SELF / SPOUSE / PARENT / CHILD / OTHER).</summary>
    [MaxLength(100)]
    public string? GuarantorRelationship { get; set; }

    /// <summary>Nomor kontak penanggung.</summary>
    [MaxLength(50)]
    public string? GuarantorPhone { get; set; }

    /// <summary>Alamat penanggung.</summary>
    [MaxLength(300)]
    public string? GuarantorAddress { get; set; }

    /// <summary>Konfirmasi eksplisit dari kasir/billing bahwa sisa tagihan dialihkan ke Finance AR.</summary>
    [Required]
    public bool UserConfirmation { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class FinalizationPreviewResponse
{
    public Guid InvoiceId { get; set; }
    public bool AllOrdersComplete { get; set; }
    public bool CalculationCurrent { get; set; }
    public decimal Outstanding { get; set; }
    public bool IsReadyForNormalFinalization { get; set; }
    public IReadOnlyList<string> BlockingReasons { get; set; } = [];

    /// <summary>
    /// Kode penahan finalisasi yang dapat dibaca mesin — kontrak <c>integrasi-billing</c>
    /// <c>1.1.0</c> API 3.9: <c>BIL-FIN-020</c> invoice "perlu diperiksa",
    /// <c>BIL-FIN-021</c> masih ada layanan "tarif belum ada", <c>BIL-FIN-022</c> masih ada pelayanan
    /// eligible yang belum masuk invoice (<c>BE-BKC-FIX-021</c>).
    /// </summary>
    public IReadOnlyList<string> BlockingCodes { get; set; } = [];

    public int CalculationVersion { get; set; }
    public Guid InvoiceRowVersion { get; set; }

    /// <summary>BE-FIN-107, FIN-INTEGRATION-1.9 R.4: rincian pembagian porsi RS Benefit dan utang pegawai.</summary>
    public EmployeeBenefitSplitResponse? EmployeeBenefitSplit { get; set; }
}

public sealed class FinalizationResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public int CalculationVersion { get; set; }
    public decimal OutstandingAtFinalization { get; set; }
    public bool IsDepartureException { get; set; }
    public string? DepartureReason { get; set; }
    public string InvoiceStatus { get; set; } = string.Empty;
    public DateTimeOffset FinalizedAt { get; set; }
    public Guid InvoiceRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public bool IsReplay { get; set; }

    /// <summary>BE-FIN-107, FIN-INTEGRATION-1.9 R.4: rincian pembagian porsi RS Benefit dan utang pegawai.</summary>
    public EmployeeBenefitSplitResponse? EmployeeBenefitSplit { get; set; }
}

/// <summary>
/// Respons pratinjau dan hasil pembagian porsi benefit karyawan saat finalisasi tagihan
/// (BE-FIN-107; FIN-INTEGRATION-1.9 R.4.2; FIN-API-1.10 R.5).
/// Seluruh angka dihitung authoritative di backend (kebijakan zero calculation di klien).
/// </summary>
public sealed class EmployeeBenefitSplitResponse
{
    public bool IsEmployeeBenefitVisit { get; set; }
    public string? FinancingChoice { get; set; } = "EMPLOYEE_BENEFIT";
    public string? BenefitOwnerName { get; set; }

    /// <summary>Identitas canonical pemilik benefit (MstWorkforceProfile.Id); terisi pula untuk dokter tetap.</summary>
    public Guid? BenefitOwnerWorkforceProfileId { get; set; }

    /// <summary>EMPLOYEE atau DOCTOR.</summary>
    public string? BenefitOwnerType { get; set; }
    public string? BenefitOwnerStaffNumber { get; set; }
    public string? RelationshipCode { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CoveredByRuleAmount { get; set; }
    public decimal RemainingAllowanceAmount { get; set; }
    public decimal RsBenefitAmount { get; set; }
    /// <summary>E: kelebihan biaya di atas plafon (dihitung ulang dari HR setiap kali).</summary>
    public decimal ExcessOverAllowanceAmount { get; set; }

    /// <summary>Bagian E yang sudah dibayar langsung lewat tender (alokasi BENEFIT_EXCESS, setelah reversal).</summary>
    public decimal ExcessPaidAmount { get; set; }

    /// <summary>E sisa = E - ExcessPaidAmount. Yang masih harus dibayar langsung atau ditangguhkan menjadi AR Karyawan.</summary>
    public decimal ExcessRemainingAmount { get; set; }
    public decimal PatientPortionAmount { get; set; }

    /// <summary>E sisa + K: total yang masih harus diselesaikan pasien (sebelum penangguhan D).</summary>
    public decimal RemainingPatientObligation { get; set; }
    public decimal DirectPayAmount { get; set; }
    public decimal EmployeeDeferredAmount { get; set; }
    public decimal ChoiceAmount { get; set; }
    public decimal EmployeeReceivableAmount { get; set; }
    public decimal CashAtCashierAmount { get; set; }
    public bool IsUsable { get; set; }
    public bool RequiresConsent { get; set; }
    public string? BlockingReasonCode { get; set; }
    public string? BlockingMessage { get; set; }
}

/// <summary>
/// Permintaan Berita Acara Koreksi Salah Pegawai / Pemilik Manfaat setelah tagihan difinalisasi
/// (BE-FIN-109; FIN-DEC-173, FIN-DEC-187, FIN-DEC-200; FR-FIN-231..234; INT-BIL-FIN-002 §P.6).
/// </summary>
public sealed class CorrectBenefitDebtorRequest
{
    /// <summary>Nomor Berita Acara Koreksi resmi (wajib).</summary>
    [Required, MaxLength(100)]
    public string OfficialReportNumber { get; set; } = string.Empty;

    /// <summary>Alasan koreksi (wajib).</summary>
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// ID pegawai pengganti di HR (MstEmployee.Id). Kompatibilitas kontrak lama; salah satu dari ini atau
    /// <see cref="CorrectWorkforceProfileId"/> wajib diisi. Bila keduanya diisi harus orang yang sama.
    /// </summary>
    public Guid CorrectEmployeeId { get; set; }

    /// <summary>Pemilik pengganti berbasis MstWorkforceProfile.Id (karyawan atau dokter tetap); identitas utama.</summary>
    public Guid? CorrectWorkforceProfileId { get; set; }

    /// <summary>ID anggota keluarga pengganti bila pasien adalah anak/pasangan pegawai (opsional).</summary>
    public Guid? CorrectFamilyMemberId { get; set; }

    /// <summary>Nama pemberi persetujuan bila pasien bukan pegawai sendiri (opsional).</summary>
    [MaxLength(200)]
    public string? ConsentGivenBy { get; set; }

    /// <summary>Catatan tambahan koreksi (opsional).</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Hasil koreksi pemilik manfaat salah orang (BE-FIN-109).
/// </summary>
public sealed class CorrectBenefitDebtorResponse
{
    public bool Success { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid OldHandoffId { get; set; }
    public Guid? AdjustmentId { get; set; }
    public Guid? NewHandoffId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid? OldBenefitOwnerId { get; set; }

    /// <summary>MstEmployee.Id pemilik baru; Guid.Empty bila pemilik baru dokter tanpa data karyawan.</summary>
    public Guid NewBenefitOwnerId { get; set; }
    public Guid? OldBenefitOwnerWorkforceProfileId { get; set; }
    public Guid? NewBenefitOwnerWorkforceProfileId { get; set; }
    public string? NewBenefitOwnerName { get; set; }
    public string? NewRelationshipCode { get; set; }
    public decimal OldEmployeeAmount { get; set; }
    public decimal NewEmployeeAmount { get; set; }
    public decimal ReturnedAllowanceAmount { get; set; }
    public decimal UsedAllowanceAmount { get; set; }
    public string OfficialReportNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

