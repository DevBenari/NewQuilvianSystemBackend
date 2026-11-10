using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // Konfigurasi layanan rehab — contracts/api-contract.md v1 grup "Rehab Discipline", BE-RHB-005.

    public class RehabDisciplineQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>Saring layanan yang sudah/belum punya profesi.</summary>
        public bool? HasProfession { get; set; }

        public string? SortBy { get; set; } = "disciplineCode";
        public string? SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateRehabDisciplineRequest
    {
        /// <summary>Huruf besar, angka, dan garis bawah; tetap setelah dibuat. Contoh <c>PHYSIOTHERAPY</c>.</summary>
        [Required(ErrorMessage = "Kode layanan wajib diisi.")]
        [MaxLength(40, ErrorMessage = "Kode layanan terlalu panjang. Batasnya 40 huruf.")]
        public string DisciplineCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama layanan wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Nama layanan terlalu panjang. Batasnya 100 huruf.")]
        public string DisciplineName { get; set; } = string.Empty;

        /// <summary>Profesi klinis (<c>MstProfession</c>). Wajib bila <see cref="IsActive"/> benar.</summary>
        public Guid? ProfessionId { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>Kode layanan tidak dapat diubah; yang dapat diubah hanya nama dan profesi.</summary>
    public class UpdateRehabDisciplineRequest
    {
        [Required(ErrorMessage = "Nama layanan wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Nama layanan terlalu panjang. Batasnya 100 huruf.")]
        public string DisciplineName { get; set; } = string.Empty;

        public Guid? ProfessionId { get; set; }
    }

    public class UpdateRehabDisciplineStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class RehabDisciplineProcedureItemRequest
    {
        [Required]
        public Guid ProcedureId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Daftar tindakan yang boleh dipakai layanan. Tindakan yang tidak disebut dinonaktifkan,
    /// bukan dihapus, agar program lama tetap terbaca.
    /// </summary>
    public class SaveRehabDisciplineProceduresRequest
    {
        public List<RehabDisciplineProcedureItemRequest> Procedures { get; set; } = new();
    }

    /// <summary>
    /// Jenis instrumen yang ditawarkan. Jenis yang tidak disebut dinonaktifkan, bukan dihapus.
    /// </summary>
    public class SaveRehabDisciplineInstrumentsRequest
    {
        public List<ClinicalInstrumentKind> InstrumentKinds { get; set; } = new();
    }

    public class RehabDisciplineListItem
    {
        public Guid Id { get; set; }
        public string DisciplineCode { get; set; } = string.Empty;
        public string DisciplineName { get; set; } = string.Empty;
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public bool IsActive { get; set; }
        public int ActiveProcedureCount { get; set; }
        public int ActiveInstrumentCount { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class RehabDisciplineOptionResponse
    {
        public Guid Id { get; set; }
        public string DisciplineCode { get; set; } = string.Empty;
        public string DisciplineName { get; set; } = string.Empty;
        public Guid? ProfessionId { get; set; }
    }

    public class RehabDisciplineProcedureResponse
    {
        public Guid Id { get; set; }
        public Guid ProcedureId { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public bool IsProcedureActive { get; set; }
        public bool IsActive { get; set; }
    }

    public class RehabDisciplineInstrumentResponse
    {
        public Guid Id { get; set; }
        public ClinicalInstrumentKind InstrumentKind { get; set; }
        public string InstrumentKindLabel { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RehabDisciplineDetailResponse : RehabDisciplineListItem
    {
        public string? ProfessionCode { get; set; }
        public List<RehabDisciplineProcedureResponse> Procedures { get; set; } = new();
        public List<RehabDisciplineInstrumentResponse> Instruments { get; set; } = new();
        public bool IsInUse { get; set; }
    }

    public class RehabDisciplineSummaryResponse
    {
        public int TotalDiscipline { get; set; }
        public int ActiveDiscipline { get; set; }
        public int InactiveDiscipline { get; set; }
        public int WithoutProfessionDiscipline { get; set; }
    }

    public class RehabMedicalInstrumentsResponse
    {
        public List<RehabDisciplineInstrumentResponse> Instruments { get; set; } = new();
    }

    public class RehabDisciplineFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public RehabDisciplineDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<RehabCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<RehabOptionResponse> StatusOptions { get; set; } = new();
        public List<RehabOptionResponse> InstrumentKindOptions { get; set; } = new();
        public List<RehabOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RehabQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<RehabFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<RehabFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class RehabDisciplineDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? HasProfession { get; set; }
        public string SortBy { get; set; } = "disciplineCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Pilihan bernilai-label yang dipakai bersama layar Rehab.</summary>
    public class RehabOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class RehabCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class RehabQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class RehabFormFieldMetadataResponse
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
}
