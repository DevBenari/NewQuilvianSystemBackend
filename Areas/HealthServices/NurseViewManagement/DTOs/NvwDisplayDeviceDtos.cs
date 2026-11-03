using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs
{
    public class NvwDisplayDevicePagedQuery
    {
        public string? Search { get; set; }
        public string? FloorCode { get; set; }
        public NvwDisplayDeviceStatus? Status { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class NvwDisplayDeviceCreateRequest
    {
        [Required(ErrorMessage = "Nama perangkat wajib diisi, paling banyak 100 karakter.")]
        [MaxLength(100, ErrorMessage = "Nama perangkat wajib diisi, paling banyak 100 karakter.")]
        public string DeviceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar.")]
        [MaxLength(20, ErrorMessage = "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar.")]
        public string FloorCode { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Isian terlalu panjang.")]
        public string? LocationName { get; set; }

        [MaxLength(500, ErrorMessage = "Isian terlalu panjang.")]
        public string? Description { get; set; }
    }

    public class NvwDisplayDeviceUpdateRequest : NvwDisplayDeviceCreateRequest
    {
        [Required]
        public int Version { get; set; }
    }

    public class NvwDisplayDeviceLoginRequest
    {
        [Required]
        public int Version { get; set; }
    }

    public class NvwDisplayDeviceDeactivateRequest
    {
        [Required(ErrorMessage = "Alasan menonaktifkan wajib diisi (5–500 karakter).")]
        [MinLength(5, ErrorMessage = "Alasan menonaktifkan wajib diisi (5–500 karakter).")]
        [MaxLength(500, ErrorMessage = "Alasan menonaktifkan wajib diisi (5–500 karakter).")]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public int Version { get; set; }
    }

    public class NvwDisplayDeviceResponse
    {
        public Guid Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string? LocationName { get; set; }
        public NvwDisplayDeviceStatus DeviceStatus { get; set; }
        public string DeviceStatusLabel { get; set; } = string.Empty;
        public bool HasLogin { get; set; }
        public DateTime? LastOnlineAt { get; set; }
        public bool IsOnline { get; set; }
    }

    public class NvwDisplayDeviceDetailResponse : NvwDisplayDeviceResponse
    {
        public string? Description { get; set; }
        public string? LoginUserName { get; set; }
        public DateTime? LoginIssuedAt { get; set; }
        public string? LoginIssuedByName { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public string? DeactivatedByName { get; set; }
        public string? DeactivationReason { get; set; }
        public int Version { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class NvwDisplayDeviceLoginResponse
    {
        public Guid DeviceId { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string LoginUserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    public class NvwFloorOptionResponse
    {
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public int RoomCount { get; set; }
        public int BedCount { get; set; }
    }
}
