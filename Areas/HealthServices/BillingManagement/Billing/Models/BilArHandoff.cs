using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilArHandoff", Schema = "public")]
public sealed class BilArHandoff : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid FinalizationRecordId { get; set; }
    [Required, MaxLength(30)] public string DebtorType { get; set; } = BillingArDebtorTypes.Payer;
    public Guid? DebtorReferenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingHandoffStatuses.Created;
    public Guid HandoffKey { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }

    /// <summary>BE-FIN-091, BE-FIN-107, FIN-DEC-184, FIN-INTEGRATION-1.9 R.4.1: terisi hanya untuk baris EMPLOYEE_BENEFIT.
    /// Berisi MstEmployee.Id pegawai pemilik manfaat, divalidasi ke HR saat Billing menerbitkan baris ini.</summary>
    public Guid? BenefitOwnerId { get; set; }

    /// <summary>
    /// Identitas canonical pemilik manfaat: MstWorkforceProfile.Id (karyawan atau dokter tetap). Properti terpisah
    /// karena <see cref="BenefitOwnerId"/> bermakna MstEmployee.Id dan tidak boleh diisi dengan nilai lain. Baris
    /// EMPLOYEE_BENEFIT baru memuat ini; baris lama dibaca lewat BenefitOwnerId dan di-backfill terpisah.
    /// </summary>
    public Guid? BenefitOwnerWorkforceProfileId { get; set; }

    /// <summary>BE-FIN-091, FIN-DEC-184/198: kode hubungan keluarga (SELF/SPOUSE/CHILD/PARENT/OTHER).</summary>
    [MaxLength(50)] public string? BenefitRelationship { get; set; }

    /// <summary>BE-FIN-107, FIN-DEC-208, FIN-INTEGRATION-1.9 R.4.3: porsi sisa tagihan pasien yang dipilih pegawai
    /// untuk dijadikan utang pegawai (D). Terisi hanya pada baris EMPLOYEE_BENEFIT; bawaan 0.</summary>
    public decimal DeferredPatientAmount { get; set; } = 0m;

    /// <summary>BE-FIN-107, BE-FIN-119, FIN-DEC-218, FIN-INTEGRATION-1.10 S.6: nama pihak yang menyetujui
    /// penangguhan tagihan menjadi utang pegawai bila pasien bukan pegawai sendiri.</summary>
    [MaxLength(200)] public string? DeferredConsentGivenBy { get; set; }

    /// <summary>Pilihan pembiayaan saat finalisasi: EMPLOYEE_BENEFIT atau penjamin terkait.</summary>
    [MaxLength(50)] public string? FinancingChoice { get; set; }

    /// <summary>Catatan atau nomor bukti otorisasi penangguhan piutang karyawan.</summary>
    [MaxLength(500)] public string? DeferredConsentNotes { get; set; }

    /// <summary>
    /// True hanya untuk baris PAYER porsi Benefit Karyawan RS (P) atas penjamin internal: biaya yang ditanggung RS sendiri,
    /// ditutup berkala tanpa uang diterima. Membedakannya dari piutang PAYER pihak luar (asuransi/perusahaan) yang
    /// benar-benar dibayar. Constraint: hanya boleh true bila DebtorType = PAYER.
    /// </summary>
    public bool IsInternalBenefit { get; set; }

    /// <summary>
    /// Asal piutang karyawan (hanya EMPLOYEE_BENEFIT; lihat <see cref="BillingEmployeeArOrigins"/>). Kosong untuk baris lama
    /// dan jenis debitur lain.
    /// </summary>
    [MaxLength(30)] public string? EmployeeArOrigin { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
    public BilFinalizationRecord FinalizationRecord { get; set; } = null!;
}

public static class BillingArDebtorTypes
{
    public const string PatientGuarantor = "PATIENT_GUARANTOR";
    public const string Payer = "PAYER";

    /// <summary>BE-FIN-091, FIN-DEC-184: porsi tagihan manfaat karyawan di atas plafon, ditanggung pegawai.</summary>
    public const string EmployeeBenefit = "EMPLOYEE_BENEFIT";
}

/// <summary>Asal piutang karyawan (EMPLOYEE_BENEFIT): kelebihan plafon, porsi sisa tagihan pasien, atau campuran keduanya.</summary>
public static class BillingEmployeeArOrigins
{
    public const string OverAllowance = "OVER_ALLOWANCE";
    public const string PatientPortion = "PATIENT_PORTION";
    public const string Mixed = "MIXED";
}

public static class BillingHandoffStatuses
{
    public const string Created = "CREATED";
    public const string Acknowledged = "ACKNOWLEDGED";
}
