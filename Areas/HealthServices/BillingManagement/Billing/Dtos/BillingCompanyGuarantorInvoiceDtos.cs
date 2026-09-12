namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>
/// Dokumen "Lembar Tagihan Penjamin Perusahaan" (Company Guarantor Invoice Document).
/// Mengikuti pola Invoice Asuransi (BKC-DEC-065-069, MPY-DEC-006, MPY-DES-013, CAP-38).
/// Murni dokumen baca-saja, nomor dokumen memakai nomor invoice yang sama tanpa nomor seri baru (CAP-41).
/// </summary>
public sealed class CompanyGuarantorInvoiceDocumentResponse
{
    public Guid InvoiceId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceStatus { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTimeOffset? InvoiceDate { get; set; }

    public string PayerKind { get; set; } = CompanyGuarantorInvoicePayerKinds.Unknown;

    /// <summary>
    /// False bila layar MUST menyembunyikan tombol cetak dan menampilkan Warnings.
    /// </summary>
    public bool IsPrintable { get; set; }

    /// <summary>
    /// True bila angkanya dibaca dari BilCalculationVersion tersimpan (invoice non-OPEN),
    /// False bila dihitung segar lewat PreviewCalculationAsync (invoice OPEN).
    /// </summary>
    public bool IsFromLockedSnapshot { get; set; }

    /// <summary>
    /// False untuk snapshot lama sebelum rincian per baris tersedia.
    /// </summary>
    public bool IsPerItemBreakdownAvailable { get; set; }

    public int CalculationVersionNo { get; set; }
    public string CalculationContractVersion { get; set; } = string.Empty;
    public DateTimeOffset CalculatedAt { get; set; }

    public CompanyGuarantorInvoicePatientResponse? Patient { get; set; }
    public CompanyGuarantorInvoicePayerResponse? Payer { get; set; }
    public CompanyGuarantorInvoiceReimbursementRouteResponse? ReimbursementRoute { get; set; }

    public IReadOnlyList<CompanyGuarantorInvoiceItemResponse> Items { get; set; } = [];
    public CompanyGuarantorInvoiceTotalResponse Totals { get; set; } = new();

    /// <summary>
    /// Kalimat peringatan/keterangan berbahasa Indonesia siap tampil di layar UI.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; set; } = [];
}

public sealed class CompanyGuarantorInvoicePatientResponse
{
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? AgeText { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public string? ServiceUnitName { get; set; }
    public string? RoomName { get; set; }
    public string? PatientClassName { get; set; }
}

/// <summary>
/// Data perusahaan penjamin dan identitas karyawan pada lembar tagihan (MPY-DES-013).
/// Yang sengaja TIDAK dimuat: RuleCode, RuleName, ApprovalInstruction, BillingInstruction,
/// dan kontak PIC pribadi - seluruhnya merupakan kesepakatan komersial internal RS-perusahaan.
/// </summary>
public sealed class CompanyGuarantorInvoicePayerResponse
{
    public Guid CompanyGuarantorId { get; set; }
    public string CompanyGuarantorCode { get; set; } = string.Empty;
    public string CompanyGuarantorName { get; set; } = string.Empty;
    public string? CompanyGroupName { get; set; }
    public string GuarantorType { get; set; } = string.Empty;
    public string? ContractNumber { get; set; }
    public string? OfficeAddress { get; set; }
    public string BillingMethod { get; set; } = string.Empty;

    // Identitas Karyawan (MPY-DES-013)
    public string? EmployeeNumber { get; set; }
    public string? EmployeeName { get; set; }
    public string? PlanName { get; set; }
    public string? ClassName { get; set; }
    public string? BenefitPlanCode { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public bool IsEligible { get; set; }
    public bool IsPolicyActive { get; set; }
}

/// <summary>
/// Keterangan rute reimbursement penggantian biaya perusahaan ke asuransi mitra (MPY-DES-014).
/// Metadata baca-saja: rumah sakit tetap menagih perusahaan penjamin (debitur tetap Company Guarantor).
/// </summary>
public sealed class CompanyGuarantorInvoiceReimbursementRouteResponse
{
    public string RouteType { get; set; } = "SELF"; // SELF | INSURANCE_PROVIDER
    public Guid? InsuranceProviderId { get; set; }
    public string? InsuranceProviderName { get; set; }
    public string? InsuranceProviderCode { get; set; }
    public string? Description { get; set; }
}

public sealed class CompanyGuarantorInvoiceItemResponse
{
    public string Kind { get; set; } = string.Empty;

    // Null untuk baris ADMINISTRATION_FEE dan ROOM_CHARGE - keduanya bukan BilInvoiceItem.
    public Guid? InvoiceItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal ItemDiscount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal CoveredNetAmount { get; set; }
    public decimal CoveredTaxAmount { get; set; }
    public decimal CoveredAmount { get; set; }
    public decimal PatientAmount { get; set; }
}

public sealed class CompanyGuarantorInvoiceTotalResponse
{
    public decimal EligibleAmount { get; set; }
    public decimal CoveredNetAmount { get; set; }
    public decimal CoveredTaxAmount { get; set; }
    public decimal TotalCoveredAmount { get; set; }
    public decimal PrimaryAmount { get; set; }
    public decimal ExcessAmount { get; set; }
    public decimal UnresolvedCoverageAmount { get; set; }
    public decimal PatientAmount { get; set; }
}

public static class CompanyGuarantorInvoicePayerKinds
{
    public const string CompanyGuarantor = "COMPANY_GUARANTOR";
    public const string Insurance = "INSURANCE";
    public const string Cash = "CASH";
    public const string Unknown = "UNKNOWN";
}

public static class CompanyGuarantorInvoiceItemKinds
{
    public const string Item = "ITEM";
    public const string AdministrationFee = "ADMINISTRATION_FEE";
    public const string RoomCharge = "ROOM_CHARGE";
}
