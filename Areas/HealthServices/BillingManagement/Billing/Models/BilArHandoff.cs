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

    /// <summary>BE-FIN-091, FIN-DEC-184/198: kode hubungan keluarga (SELF/SPOUSE/CHILD/PARENT/OTHER).</summary>
    [MaxLength(50)] public string? BenefitRelationship { get; set; }

    /// <summary>BE-FIN-107, FIN-DEC-208, FIN-INTEGRATION-1.9 R.4.3: porsi sisa tagihan pasien yang dipilih pegawai
    /// untuk dijadikan utang pegawai (D). Terisi hanya pada baris EMPLOYEE_BENEFIT; bawaan 0.</summary>
    public decimal DeferredPatientAmount { get; set; } = 0m;

    /// <summary>BE-FIN-107, BE-FIN-119, FIN-DEC-218, FIN-INTEGRATION-1.10 S.6: nama pihak yang menyetujui
    /// penangguhan tagihan menjadi utang pegawai bila pasien bukan pegawai sendiri.</summary>
    [MaxLength(200)] public string? DeferredConsentGivenBy { get; set; }

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

public static class BillingHandoffStatuses
{
    public const string Created = "CREATED";
    public const string Acknowledged = "ACKNOWLEDGED";
}
