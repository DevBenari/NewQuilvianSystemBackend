using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Bentuk balasan satu kebijakan batas waktu pengkajian — <c>BE-RWI-055</c>.
    /// </summary>
    public class ClinicalAssessmentPolicyResponse
    {
        public Guid Id { get; set; }

        public string PolicyCode { get; set; } = string.Empty;

        public string PolicyName { get; set; } = string.Empty;

        public PatientAssessmentType AssessmentType { get; set; }

        /// <summary>Kosong berarti berlaku untuk seluruh jenis pelayanan.</summary>
        public ServiceUnitType? ServiceUnitType { get; set; }

        public int DueWithinMinutes { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Benar bila kebijakan ini sedang berlaku pada saat balasan dibentuk: aktif, sudah
        /// mulai, dan belum berakhir.
        /// </summary>
        public bool IsCurrentlyEffective { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>Bentuk ringan untuk kotak pilihan pada layar lain.</summary>
    public class ClinicalAssessmentPolicyOptionResponse
    {
        public Guid Id { get; set; }

        public string PolicyCode { get; set; } = string.Empty;

        public string PolicyName { get; set; } = string.Empty;

        public PatientAssessmentType AssessmentType { get; set; }

        public ServiceUnitType? ServiceUnitType { get; set; }

        public int DueWithinMinutes { get; set; }
    }

    /// <summary>Rekap jumlah kebijakan untuk kartu statistik halaman master.</summary>
    public class ClinicalAssessmentPolicySummaryResponse
    {
        public int TotalPolicy { get; set; }

        public int ActivePolicy { get; set; }

        public int InactivePolicy { get; set; }

        /// <summary>Kebijakan yang periodenya sedang berjalan hari ini.</summary>
        public int CurrentlyEffectivePolicy { get; set; }

        /// <summary>Kebijakan yang periodenya sudah berakhir.</summary>
        public int ExpiredPolicy { get; set; }

        /// <summary>
        /// Benar bila belum ada satu pun kebijakan yang berlaku. Layar memakainya untuk
        /// menuliskan "batas waktu pengkajian belum ditetapkan", bukan "tidak ada data".
        /// </summary>
        public bool IsPolicyMasterEmpty { get; set; }
    }

    public class CreateClinicalAssessmentPolicyRequest
    {
        [Required(ErrorMessage = "Kode kebijakan wajib diisi.")]
        [MaxLength(50, ErrorMessage = "Kode kebijakan terlalu panjang. Batasnya 50 huruf.")]
        public string PolicyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama kebijakan wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama kebijakan terlalu panjang. Batasnya 150 huruf.")]
        public string PolicyName { get; set; } = string.Empty;

        public PatientAssessmentType AssessmentType { get; set; } = PatientAssessmentType.Initial;

        /// <summary>Kosongkan bila kebijakan berlaku untuk seluruh jenis pelayanan.</summary>
        public ServiceUnitType? ServiceUnitType { get; set; }

        [Range(1, 100000, ErrorMessage = "Batas waktu harus antara 1 dan 100000 menit.")]
        public int DueWithinMinutes { get; set; }

        [Required(ErrorMessage = "Awal masa berlaku wajib diisi.")]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [MaxLength(500, ErrorMessage = "Keterangan terlalu panjang. Batasnya 500 huruf.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Bentuk permintaan mengubah kebijakan. Memuat <c>PolicyCode</c> supaya kode yang salah
    /// ketik dapat dibetulkan selama kode barunya belum dipakai kebijakan lain.
    /// </summary>
    public class UpdateClinicalAssessmentPolicyRequest : CreateClinicalAssessmentPolicyRequest
    {
    }

    /// <summary>Bentuk permintaan mengaktifkan atau menonaktifkan kebijakan.</summary>
    /// <remarks>
    /// Menonaktifkan kebijakan <b>tidak</b> menyentuh satu pun pengkajian yang sudah memakainya.
    /// Pengkajian menyimpan penunjuk kebijakannya sendiri, sehingga penilaian keterlambatan yang
    /// lalu tetap terbaca apa adanya — <c>AC-CAP012-04</c>.
    /// </remarks>
    public class UpdateClinicalAssessmentPolicyStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class ClinicalAssessmentPolicyEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicalAssessmentPolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicalAssessmentPolicyCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class ClinicalAssessmentPolicyQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class ClinicalAssessmentPolicyFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class ClinicalAssessmentPolicyDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public PatientAssessmentType? AssessmentType { get; set; }
        public ServiceUnitType? ServiceUnitType { get; set; }
        public bool? OnlyCurrentlyEffective { get; set; }
        public string SortBy { get; set; } = "effectiveFrom";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Konfigurasi halaman master kebijakan batas waktu pengkajian: penyaring, pengurutan, dan
    /// metadata form. Dipanggil layar sebelum data utamanya diambil.
    /// </summary>
    public class ClinicalAssessmentPolicyFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string ResetButtonLabel { get; set; } = "Reset";

        public ClinicalAssessmentPolicyDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<ClinicalAssessmentPolicyCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<ClinicalAssessmentPolicySortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<ClinicalAssessmentPolicyEnumOptionResponse> AssessmentTypeOptions { get; set; } = new();

        public List<ClinicalAssessmentPolicyEnumOptionResponse> ServiceUnitTypeOptions { get; set; } = new();

        public List<ClinicalAssessmentPolicyQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public List<ClinicalAssessmentPolicyFormFieldMetadataResponse> CreateFields { get; set; } = new();

        public List<ClinicalAssessmentPolicyFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }
}
