using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class InpatientClearanceItemSummaryResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int InactiveData { get; set; }

        public int MandatoryData { get; set; }

        public int OptionalData { get; set; }
    }

    public class InpatientClearanceItemOptionResponse
    {
        public Guid Id { get; set; }

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public bool IsMandatory { get; set; }

        public int SortOrder { get; set; }
    }

    public class InpatientClearanceItemFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, startDate dan endDate akan diabaikan.";

        public string ResetButtonLabel { get; set; } = "Reset";

        public InpatientClearanceItemDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<InpatientClearanceItemCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<InpatientClearanceItemSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<InpatientClearanceItemBooleanOptionResponse> MandatoryOptions { get; set; } = new();

        public List<InpatientClearanceItemBooleanOptionResponse> StatusOptions { get; set; } = new();

        public List<InpatientClearanceItemQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public List<InpatientClearanceItemFormFieldMetadataResponse> CreateFields { get; set; } = new();

        public List<InpatientClearanceItemFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class InpatientClearanceItemDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public string? Search { get; set; }

        public bool? IsMandatory { get; set; }

        public bool? IsActive { get; set; }

        public string SortBy { get; set; } = "sortOrder";

        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class InpatientClearanceItemCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool UsesStartDate { get; set; }

        public bool UsesEndDate { get; set; }
    }

    public class InpatientClearanceItemSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class InpatientClearanceItemBooleanOptionResponse
    {
        public bool Value { get; set; }

        public string Label { get; set; } = string.Empty;
    }

    public class InpatientClearanceItemQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Required { get; set; } = "No";

        public string Description { get; set; } = string.Empty;

        public string? Example { get; set; }
    }

    public class InpatientClearanceItemFormFieldMetadataResponse
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

    /// <summary>
    /// Bentuk balasan satu butir administrasi yang menahan penutupan episode Rawat Inap.
    /// </summary>
    public class InpatientClearanceItemResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode butir, unik di seluruh tabel. Contoh <c>ADM-DOC</c>.</summary>
        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Butir wajib menahan penutupan episode selama belum ditandai. Butir tidak wajib
        /// tetap dapat ditandai, tetapi tidak menahan apa pun.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>Urutan tampil butir pada daftar periksa yang dikerjakan petugas.</summary>
        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateInpatientClearanceItemRequest
    {
        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsMandatory { get; set; } = true;

        [Range(0, 9999)]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Bentuk permintaan mengubah butir administrasi. Sengaja memuat <c>ItemCode</c>, karena
    /// butir yang salah ketik kodenya harus dapat dibetulkan selama kode barunya belum dipakai
    /// butir lain.
    /// </summary>
    public class UpdateInpatientClearanceItemRequest : CreateInpatientClearanceItemRequest
    {
    }

    /// <summary>
    /// Bentuk permintaan mengaktifkan atau menonaktifkan butir administrasi.
    /// </summary>
    /// <remarks>
    /// Menonaktifkan butir TIDAK menghapus penandaan yang sudah ada pada episode lama.
    /// Penandaan adalah catatan bahwa sesuatu pernah diselesaikan seseorang pada suatu waktu;
    /// menghapusnya karena butirnya tidak berlaku lagi akan membuat riwayat episode lama
    /// berbohong.
    /// </remarks>
    public class UpdateInpatientClearanceItemStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteInpatientClearanceItemRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }
}
