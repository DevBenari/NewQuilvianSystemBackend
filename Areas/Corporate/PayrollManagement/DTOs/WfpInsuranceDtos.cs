using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs
{
    public class WfpInsuranceSummaryResponse
    {
        public int TotalInsuranceProfile { get; set; }
        public int ActiveInsuranceProfile { get; set; }
        public int InactiveInsuranceProfile { get; set; }
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
        public bool IsPrivateInsuranceEnabled { get; set; }
        public string? PrivateInsuranceProvider { get; set; }
        public string? PrivateInsuranceNumber { get; set; }
        public decimal PrivateInsuranceEmployeeContribution { get; set; }
        public decimal PrivateInsuranceEmployerContribution { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Kepesertaan asuransi swasta HRD. Kosong berarti belum dikonfigurasi. Kolom BPJS pada profil (nomor peserta, tarif per
        // pegawai, program) adalah data historis yang tidak lagi dikelola atau ditampilkan lewat kontrak ini (PAY-BE-FIX-007). ----

        /// <summary>ACTIVE | NOT_PARTICIPATING | NOT_APPLICABLE; kosong = belum dikonfigurasi.</summary>
        public string? ParticipationStatus { get; set; }
        public string? ParticipationBasis { get; set; }

        /// <summary>NOT_CONFIGURED | CONFIGURED | NOT_EFFECTIVE | ERROR (turunan dari isi profil dan masa berlaku hari ini).</summary>
        public string ConfigurationStatus { get; set; } = string.Empty;
        public List<string> ConfigurationIssues { get; set; } = new();

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

        public List<WfpInsuranceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpInsuranceDefaultFilterResponse
    {
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
        public bool IsPrivateInsuranceEnabled { get; set; }

        [MaxLength(200)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public decimal PrivateInsuranceEmployeeContribution { get; set; }
        public decimal PrivateInsuranceEmployerContribution { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Kepesertaan asuransi swasta HRD (kosong = belum dikonfigurasi, kompatibel dengan klien lama). Klien lama yang masih
        // mengirim isian BPJS tidak ditolak: isian itu diabaikan dan kolom BPJS yang tersimpan tidak diubah. ----

        [MaxLength(30)]
        public string? ParticipationStatus { get; set; }

        [MaxLength(500)]
        public string? ParticipationBasis { get; set; }

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
