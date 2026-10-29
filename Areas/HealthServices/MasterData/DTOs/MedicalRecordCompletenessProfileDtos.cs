using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    // =========================================================================
    // Master profil dokumen wajib rekam medis (`BE-37`, kontrak API 0.2.0 A3).
    //
    // Satu profil = satu kunci (jenis kunjungan + jenis unit + kondisi) beserta kriteria dokumen
    // yang dituntut. Kriteria dipilih dari katalog tetap `CompletenessCriterion`; master hanya
    // memilih, tidak mendefinisikan arti "terpenuhi" (`RM-DEC-045`, `RM-DEC-047`).
    // =========================================================================

    /// <summary>Jumlah profil untuk kartu statistik halaman index.</summary>
    public class MedicalRecordCompletenessProfileSummaryResponse
    {
        public int TotalProfile { get; set; }
        public int ActiveProfile { get; set; }
        public int InactiveProfile { get; set; }

        /// <summary>
        /// Profil aktif tanpa satu pun kriteria. Sah — Rawat Jalan unit Laboratorium dan Radiologi
        /// memang tidak menuntut dokumen dokter (`RM-DEC-060`) — tetapi perlu terlihat supaya
        /// tidak terjadi tanpa sengaja.
        /// </summary>
        public int ActiveProfileWithoutCriterion { get; set; }
    }

    /// <summary>Satu baris daftar profil.</summary>
    public class MedicalRecordCompletenessProfileListItemResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode profil, mis. <c>RJ</c>, <c>RJ-LAB</c>, <c>K-OK</c>.</summary>
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Kosong = semua jenis kunjungan (hanya pada profil kondisi).</summary>
        public EncounterType? EncounterType { get; set; }

        public string EncounterTypeName { get; set; } = string.Empty;

        /// <summary>Kosong = semua jenis unit.</summary>
        public ServiceUnitType? ServiceUnitType { get; set; }

        public string ServiceUnitTypeName { get; set; } = string.Empty;

        public CompletenessCondition Condition { get; set; }

        public string ConditionName { get; set; } = string.Empty;

        /// <summary>Jumlah kriteria aktif pada profil.</summary>
        public int CriterionCount { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>Satu profil beserta kriteria aktifnya, terurut menurut nilai kriteria.</summary>
    public class MedicalRecordCompletenessProfileDetailResponse : MedicalRecordCompletenessProfileListItemResponse
    {
        public Guid CreateBy { get; set; }

        public Guid? UpdateBy { get; set; }

        public List<MedicalRecordCompletenessProfileItemResponse> Items { get; set; } = new();
    }

    public class MedicalRecordCompletenessProfileItemResponse
    {
        public Guid Id { get; set; }

        public CompletenessCriterion Criterion { get; set; }

        public string CriterionName { get; set; } = string.Empty;

        /// <summary>Arti "terpenuhi" kriteria ini, untuk dibaca petugas rekam medis.</summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>Satu kriteria pada katalog <c>/criteria/options</c>.</summary>
    public class MedicalRecordCompletenessCriterionOptionResponse
    {
        public int Value { get; set; }

        /// <summary>Nama enum, mis. <c>PrimaryDiagnosis</c>.</summary>
        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan membuat profil. Butir urutan tampil (<c>sortOrder</c>) pada kontrak A3 tidak
    /// diterima: kolomnya sengaja tidak dibuat (roadmap R2.2, QBE), dan kriteria diurutkan menurut
    /// nilai katalognya. Klien yang tetap mengirimnya tidak ditolak — isiannya diabaikan.
    /// </summary>
    public class CreateMedicalRecordCompletenessProfileRequest
    {
        [Required(ErrorMessage = "Kode profil wajib diisi.")]
        [MaxLength(30, ErrorMessage = "Kode profil terlalu panjang. Batasnya 30 huruf.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama profil wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama profil terlalu panjang. Batasnya 150 huruf.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Wajib bila <see cref="Condition"/> = <c>None</c> (`VAL-RM-A04`).</summary>
        public EncounterType? EncounterType { get; set; }

        /// <summary>Kosong = semua jenis unit.</summary>
        public ServiceUnitType? ServiceUnitType { get; set; }

        public CompletenessCondition Condition { get; set; } = CompletenessCondition.None;

        /// <summary>Boleh kosong: profil tanpa kriteria berarti tidak ada dokumen dokter wajib.</summary>
        public List<MedicalRecordCompletenessProfileItemRequest> Items { get; set; } = new();
    }

    /// <summary>Sama dengan permintaan membuat, tanpa kode — kode profil tidak dapat diubah.</summary>
    public class UpdateMedicalRecordCompletenessProfileRequest
    {
        [Required(ErrorMessage = "Nama profil wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama profil terlalu panjang. Batasnya 150 huruf.")]
        public string Name { get; set; } = string.Empty;

        public EncounterType? EncounterType { get; set; }

        public ServiceUnitType? ServiceUnitType { get; set; }

        public CompletenessCondition Condition { get; set; } = CompletenessCondition.None;

        public List<MedicalRecordCompletenessProfileItemRequest> Items { get; set; } = new();
    }

    public class MedicalRecordCompletenessProfileItemRequest
    {
        public CompletenessCriterion Criterion { get; set; }
    }

    public class UpdateMedicalRecordCompletenessProfileStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class MedicalRecordCompletenessProfileFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public MedicalRecordCompletenessProfileDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MedicalRecordCompletenessProfileEnumOptionResponse> EncounterTypeOptions { get; set; } = new();
        public List<MedicalRecordCompletenessProfileEnumOptionResponse> ServiceUnitTypeOptions { get; set; } = new();
        public List<MedicalRecordCompletenessProfileEnumOptionResponse> ConditionOptions { get; set; } = new();
        public List<MedicalRecordCompletenessProfileSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MedicalRecordCompletenessProfileQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<MedicalRecordCompletenessProfileFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<MedicalRecordCompletenessProfileFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class MedicalRecordCompletenessProfileDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public EncounterType? EncounterType { get; set; }
        public ServiceUnitType? ServiceUnitType { get; set; }
        public CompletenessCondition? Condition { get; set; }
        public string SortBy { get; set; } = "code";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MedicalRecordCompletenessProfileEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MedicalRecordCompletenessProfileSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MedicalRecordCompletenessProfileQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class MedicalRecordCompletenessProfileFormFieldMetadataResponse
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
