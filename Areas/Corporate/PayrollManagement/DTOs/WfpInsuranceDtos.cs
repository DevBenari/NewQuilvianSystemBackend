using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs
{
    public class WfpInsuranceSummaryResponse
    {
        public int TotalInsuranceProfile { get; set; }
        public int ActiveInsuranceProfile { get; set; }
        public int InactiveInsuranceProfile { get; set; }
        public int BpjsKesehatanEnabledProfile { get; set; }
        public int BpjsKetenagakerjaanEnabledProfile { get; set; }
        public int PrivateInsuranceEnabledProfile { get; set; }

        /// <summary>Profil dengan status kepesertaan eksplisit, valid, dan berlaku saat ini (termasuk tidak mengikuti dengan dasar).</summary>
        public int ConfiguredProfile { get; set; }

        /// <summary>Profil yang belum menetapkan status kepesertaan, belum/tidak berlaku, atau tidak sah. Semua flag false bukan "tidak wajib".</summary>
        public int NotConfiguredProfile { get; set; }
    }

    public class WfpInsuranceResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public bool IsBpjsKesehatanEnabled { get; set; }
        public string? BpjsKesehatanNumber { get; set; }
        public bool IsBpjsKetenagakerjaanEnabled { get; set; }
        public string? BpjsKetenagakerjaanNumber { get; set; }
        public bool IsPrivateInsuranceEnabled { get; set; }
        public string? PrivateInsuranceProvider { get; set; }
        public string? PrivateInsuranceNumber { get; set; }
        public decimal BpjsHealthEmployeeRate { get; set; }
        public decimal BpjsHealthEmployerRate { get; set; }
        public decimal BpjsEmploymentEmployeeRate { get; set; }
        public decimal BpjsEmploymentEmployerRate { get; set; }
        public decimal PrivateInsuranceEmployeeContribution { get; set; }
        public decimal PrivateInsuranceEmployerContribution { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Kepesertaan iuran HRD (additive). Kosong berarti belum dikonfigurasi. Tarif per pegawai di atas dipertahankan untuk
        // kompatibilitas dan tidak dipakai mesin hitung: tarif dan batas BPJS berasal dari Pengaturan Penggajian run. ----

        /// <summary>ACTIVE | NOT_PARTICIPATING | NOT_APPLICABLE; kosong = belum dikonfigurasi.</summary>
        public string? ParticipationStatus { get; set; }
        public string? ParticipationBasis { get; set; }

        /// <summary>Program BPJS Ketenagakerjaan yang diikuti (JHT,JP,JKK,JKM). Kosong berarti semua program.</summary>
        public string? BpjsEmploymentPrograms { get; set; }

        /// <summary>NOT_CONFIGURED | CONFIGURED | NOT_EFFECTIVE | ERROR (turunan dari isi profil dan masa berlaku hari ini).</summary>
        public string ConfigurationStatus { get; set; } = string.Empty;
        public List<string> ConfigurationIssues { get; set; } = new();

        /// <summary>Tarif per pegawai pada profil terisi (&gt; 0) tetapi diabaikan perhitungan; Payroll memakai kebijakan aktif.</summary>
        public bool HasIgnoredPerEmployeeRates { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpInsuranceDetailResponse : WfpInsuranceResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpInsuranceFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpInsuranceDefaultFilterResponse DefaultFilter { get; set; } = new();

        /// <summary>Pilihan status kepesertaan dari kontrak Backend (bukan daftar tertanam di layar).</summary>
        public List<WfpInsuranceOptionResponse> ParticipationStatusOptions { get; set; } = new();

        /// <summary>Pilihan program BPJS Ketenagakerjaan.</summary>
        public List<WfpInsuranceOptionResponse> EmploymentProgramOptions { get; set; } = new();
        public List<WfpInsuranceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpInsuranceDefaultFilterResponse
    {
        public bool? IsBpjsKesehatanEnabled { get; set; }
        public bool? IsBpjsKetenagakerjaanEnabled { get; set; }
        public bool? IsPrivateInsuranceEnabled { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpInsuranceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpInsuranceOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class CreateWfpInsuranceRequest
    {
        public bool IsBpjsKesehatanEnabled { get; set; }

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; }

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; }

        [MaxLength(200)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public decimal BpjsHealthEmployeeRate { get; set; }
        public decimal BpjsHealthEmployerRate { get; set; }
        public decimal BpjsEmploymentEmployeeRate { get; set; }
        public decimal BpjsEmploymentEmployerRate { get; set; }
        public decimal PrivateInsuranceEmployeeContribution { get; set; }
        public decimal PrivateInsuranceEmployerContribution { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Kepesertaan iuran HRD (additive; kosong = belum dikonfigurasi, kompatibel dengan klien lama). ----

        [MaxLength(30)]
        public string? ParticipationStatus { get; set; }

        [MaxLength(500)]
        public string? ParticipationBasis { get; set; }

        [MaxLength(50)]
        public string? BpjsEmploymentPrograms { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpInsuranceRequest : CreateWfpInsuranceRequest
    {
    }

    public class UpdateWfpInsuranceStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
