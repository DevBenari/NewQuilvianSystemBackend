using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwBedTabletPagedQuery
    {
        public string? Search { get; set; }
        public string? FloorCode { get; set; }
        public NvwBedTabletStatus? Status { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class NvwBedTabletCreateRequest
    {
        [Required(ErrorMessage = "Nama tablet wajib diisi, paling banyak 100 karakter.")]
        [MaxLength(100, ErrorMessage = "Nama tablet wajib diisi, paling banyak 100 karakter.")]
        public string DeviceName { get; set; } = string.Empty;

        public Guid BedId { get; set; }

        [MaxLength(150, ErrorMessage = "Isian terlalu panjang.")]
        public string? LocationNote { get; set; }

        [MaxLength(500, ErrorMessage = "Isian terlalu panjang.")]
        public string? Description { get; set; }
    }

    public class NvwBedTabletUpdateRequest : NvwBedTabletCreateRequest
    {
        [Required]
        public int Version { get; set; }
    }

    public class NvwBedTabletLoginRequest
    {
        [Required]
        public int Version { get; set; }
    }

    public class NvwBedTabletDeactivateRequest
    {
        [Required(ErrorMessage = "Alasan menonaktifkan wajib diisi (5–500 karakter).")]
        [MinLength(5, ErrorMessage = "Alasan menonaktifkan wajib diisi (5–500 karakter).")]
        [MaxLength(500, ErrorMessage = "Alasan menonaktifkan wajib diisi (5–500 karakter).")]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public int Version { get; set; }
    }

    public class NvwBedTabletResponse
    {
        public Guid Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public Guid BedId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string FloorCode { get; set; } = string.Empty;
        public NvwBedTabletStatus DeviceStatus { get; set; }
        public string DeviceStatusLabel { get; set; } = string.Empty;
        public bool HasLogin { get; set; }
        public DateTime? LastOnlineAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class NvwBedTabletDetailResponse : NvwBedTabletResponse
    {
        public string? LocationNote { get; set; }
        public string? Description { get; set; }
        public string? LoginUserName { get; set; }
        public DateTime? LoginIssuedAt { get; set; }
        public string? LoginIssuedByName { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public string? DeactivatedByName { get; set; }
        public string? DeactivationReason { get; set; }
        public bool HasActiveCall { get; set; }
        public int Version { get; set; }
        public List<string> AvailableActions { get; set; } = new();

        [JsonIgnore]
        public string? PreviousBedCode { get; set; }
    }

    public class NvwBedTabletLoginResponse
    {
        public Guid TabletId { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string LoginUserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Version { get; set; }

        [JsonIgnore]
        public string BedCode { get; set; } = string.Empty;

        [JsonIgnore]
        public string DeviceStatusLabel { get; set; } = string.Empty;
    }

    public class NvwBedOptionResponse
    {
        public Guid BedId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string FloorCode { get; set; } = string.Empty;
        public bool HasTabletInUse { get; set; }
    }
}
