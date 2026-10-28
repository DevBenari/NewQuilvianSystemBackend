using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.DTOs
{
    // =========================================================================
    // Request DTOs
    // =========================================================================

    /// <summary>
    /// Request untuk membuat peran pelaksana jasa medis baru — MDF-VAL-1.0 aturan 1.1-1.5.
    /// </summary>
    public class CreateMedicalFeeRoleRequest
    {
        [Required(ErrorMessage = "Kode peran (RoleCode) wajib diisi.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "RoleCode harus berupa 2-30 karakter.")]
        [RegularExpression("^[A-Z0-9_]{2,30}$", ErrorMessage = "RoleCode harus berupa 2-30 karakter huruf besar, angka, atau garis bawah.")]
        public string RoleCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama peran (RoleName) wajib diisi.")]
        [StringLength(100, ErrorMessage = "RoleName maksimal 100 karakter.")]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Nama enum OprTeamRole milik Operating Room Management jika ada padanannya di kamar operasi.
        /// </summary>
        [MaxLength(30, ErrorMessage = "OprTeamRoleMapping maksimal 30 karakter.")]
        public string? OprTeamRoleMapping { get; set; }

        /// <summary>Menandai apakah peran ini adalah peran utama (default pelaksana tunggal).</summary>
        public bool IsPrimaryRole { get; set; } = false;

        [MaxLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter.")]
        public string? Description { get; set; }

        public bool? IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request untuk memperbarui peran pelaksana jasa medis yang sudah ada.
    /// </summary>
    public class UpdateMedicalFeeRoleRequest
    {
        [Required(ErrorMessage = "Kode peran (RoleCode) wajib diisi.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "RoleCode harus berupa 2-30 karakter.")]
        [RegularExpression("^[A-Z0-9_]{2,30}$", ErrorMessage = "RoleCode harus berupa 2-30 karakter huruf besar, angka, atau garis bawah.")]
        public string RoleCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama peran (RoleName) wajib diisi.")]
        [StringLength(100, ErrorMessage = "RoleName maksimal 100 karakter.")]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(30, ErrorMessage = "OprTeamRoleMapping maksimal 30 karakter.")]
        public string? OprTeamRoleMapping { get; set; }

        public bool IsPrimaryRole { get; set; } = false;

        [MaxLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter.")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request untuk mengubah status aktif peran.
    /// </summary>
    public class ToggleMedicalFeeRoleActivationRequest
    {
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Parameter pencarian dan pagination untuk daftar peran.
    /// </summary>
    public class MedicalFeeRolePagedQuery
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPrimaryRole { get; set; }
        public bool? HasOprTeamRoleMapping { get; set; }
        public string? SortBy { get; set; } = "RoleCode";
        public string? SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    // =========================================================================
    // Response DTOs
    // =========================================================================

    /// <summary>
    /// Representasi data peran pelaksana jasa medis.
    /// </summary>
    public class MedicalFeeRoleResponse
    {
        public Guid Id { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? OprTeamRoleMapping { get; set; }
        public bool IsPrimaryRole { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int SharingRulesCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid UpdateBy { get; set; }
    }

    /// <summary>
    /// Opsi ringan peran untuk komponen select / dropdown.
    /// </summary>
    public class MedicalFeeRoleOptionResponse
    {
        public Guid Id { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? OprTeamRoleMapping { get; set; }
        public bool IsPrimaryRole { get; set; }
        public bool IsActive { get; set; }
        public string Label => $"{RoleCode} — {RoleName}";
        public string Value => Id.ToString();
    }

    /// <summary>
    /// Ringkasan statistik jumlah data peran.
    /// </summary>
    public class MedicalFeeRoleSummaryResponse
    {
        public int TotalFeeRole { get; set; }
        public int ActiveFeeRole { get; set; }
        public int InactiveFeeRole { get; set; }
        public int PrimaryFeeRole { get; set; }
        public int MappedToOprFeeRole { get; set; }
    }

    // =========================================================================
    // Filter Metadata Response DTOs
    // =========================================================================

    public class MedicalFeeRoleFilterMetadataResponse
    {
        public MedicalFeeRoleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MedicalFeeRoleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<string> OprTeamRoleOptions { get; set; } = new();
        public List<MedicalFeeRoleQueryParamResponse> QueryParameters { get; set; } = new();
        public List<MedicalFeeRoleFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<MedicalFeeRoleFieldMetadataResponse> UpdateFields { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class MedicalFeeRoleDefaultFilterResponse
    {
        public string SortBy { get; set; } = "RoleCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public bool? IsActive { get; set; } = null;
    }

    public class MedicalFeeRoleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MedicalFeeRoleQueryParamResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class MedicalFeeRoleFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = "Informasi Peran";
        public string InputType { get; set; } = "text";
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    // =========================================================================
    // Service Result Wrapper
    // =========================================================================

    public class MedicalFeeServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public T? Data { get; set; }

        public static MedicalFeeServiceResult<T> Ok(T data, string message = "Success", int statusCode = StatusCodes.Status200OK)
            => new() { Success = true, StatusCode = statusCode, Message = message, Data = data };

        public static MedicalFeeServiceResult<T> Fail(int statusCode, string message, string? errorCode = null)
            => new() { Success = false, StatusCode = statusCode, Message = message, ErrorCode = errorCode };
    }
}

