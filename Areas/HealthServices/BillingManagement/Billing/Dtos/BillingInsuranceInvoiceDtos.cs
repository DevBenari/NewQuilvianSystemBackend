namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

// BKC-DES-007: dokumen "Invoice Asuransi" bukan Claim Letter formal - nomornya sama dengan
// InvoiceNumber, tidak ada seri nomor tersendiri. BKC-DEC-068: hanya baris ber-CoveredAmount > 0
// yang tampil; penyaringan dikerjakan di sini (server), bukan di layar.
public sealed class InsuranceInvoiceDocumentResponse
{
    public Guid InvoiceId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceStatus { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTimeOffset? InvoiceDate { get; set; }

    public string PayerKind { get; set; } = InsuranceInvoicePayerKinds.Unknown;

    // false berarti layar MUST menyembunyikan tombol cetak dan menampilkan Warnings.
    public bool IsPrintable { get; set; }

    // true bila angkanya dibaca dari BilCalculationVersion tersimpan (invoice non-OPEN),
    // false bila dihitung segar lewat PreviewCalculationAsync (invoice OPEN).
    public bool IsFromLockedSnapshot { get; set; }

    // BIL-VAL-033/BKC-DES-004: false untuk snapshot lama sebelum rincian per baris ada.
    public bool IsPerItemBreakdownAvailable { get; set; }

    public int CalculationVersionNo { get; set; }
    public string CalculationContractVersion { get; set; } = string.Empty;
    public DateTimeOffset CalculatedAt { get; set; }

    public InsuranceInvoicePatientResponse? Patient { get; set; }
    public InsuranceInvoicePayerResponse? Payer { get; set; }

    public IReadOnlyList<InsuranceInvoiceItemResponse> Items { get; set; } = [];
    public InsuranceInvoiceTotalResponse Totals { get; set; } = new();

    // Kalimat berbahasa Indonesia siap tampil. Kosong bila tidak ada yang perlu diberitahukan.
    public IReadOnlyList<string> Warnings { get; set; } = [];
}

public sealed class InsuranceInvoicePatientResponse
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

// Yang sengaja TIDAK ada di sini: RuleCode, RuleName, ApprovalInstruction, BillingInstruction
// (isi kesepakatan komersial RS-asuransi), CardNumberSnapshot (nomor kartu asuransi), dan
// kontak PIC perusahaan asuransi. Lihat 02-backend-architecture.md § Yang sengaja tidak dibuat.
public sealed class InsuranceInvoicePayerResponse
{
    public string InsuranceProviderName { get; set; } = string.Empty;
    public string? InsuranceGroupName { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ClaimMethod { get; set; } = string.Empty;
    public string? ContractNumber { get; set; }
    public string? OfficeAddress { get; set; }
    public string? PolicyNumber { get; set; }
    public string? MemberNumber { get; set; }
    public string? PlanName { get; set; }
    public string? ClassName { get; set; }
    public string? BenefitPlanCode { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public bool IsEligible { get; set; }
    public bool IsPolicyActive { get; set; }
}

public sealed class InsuranceInvoiceItemResponse
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

public sealed class InsuranceInvoiceTotalResponse
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

public static class InsuranceInvoicePayerKinds
{
    public const string Insurance = "INSURANCE";
    public const string Cash = "CASH";
    public const string CompanyGuarantor = "COMPANY_GUARANTOR";
    public const string Unknown = "UNKNOWN";
}

public static class InsuranceInvoiceItemKinds
{
    public const string Item = "ITEM";
    public const string AdministrationFee = "ADMINISTRATION_FEE";
    public const string RoomCharge = "ROOM_CHARGE";
}
