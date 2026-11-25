using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class AttendanceDeviceFilterMetadataResponse
    {
        public List<AttendanceDeviceOptionResponse> DeviceTypeOptions { get; set; } = new();
        public List<AttendanceDeviceOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class AttendanceDeviceOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class AttendanceDeviceQueryRequest
    {
        /// <summary>Cari pada kode, nama, nomor seri, atau ID eksternal mesin.</summary>
        public string? Search { get; set; }
        public string? DeviceType { get; set; }
        public bool? IsActive { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public string? SortBy { get; set; } = "attendanceDeviceCode";
        public string? SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class AttendanceDeviceResponse
    {
        public Guid Id { get; set; }
        public string AttendanceDeviceCode { get; set; } = string.Empty;
        public string AttendanceDeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string DeviceTypeLabel { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? ModelName { get; set; }
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public string? MacAddress { get; set; }
        public string? IntegrationProvider { get; set; }

        /// <summary>Identitas yang dikirim mesin pada setiap scan; dipakai mencocokkan scan, termasuk scan tertahan.</summary>
        public string? ExternalDeviceId { get; set; }

        public Guid? AttendanceLocationId { get; set; }
        public string? AttendanceLocationName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public bool IsOnline { get; set; }
        public bool IsPrimary { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Jumlah scan berstatus "Menunggu Pendaftaran Mesin" yang identitasnya cocok dengan mesin ini.
        /// Lebih dari nol hanya bila mesin nonaktif; mesin aktif melepasnya sesudah disimpan.
        /// </summary>
        public int HeldScanCount { get; set; }

        /// <summary>
        /// Hasil pelepasan scan tertahan sesudah mesin didaftarkan, diubah, atau diaktifkan (HRD-DEC-212).
        /// Hanya terisi pada respons tulis; null bila pelepasan tidak dijalankan.
        /// </summary>
        public AttendanceRawLogReleaseResponse? HeldScanRelease { get; set; }

        /// <summary>Pesan bila pelepasan gagal; mesin tetap tersimpan dan scan tetap tertahan sampai dipicu ulang.</summary>
        public string? HeldScanReleaseMessage { get; set; }

        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class UnregisteredDeviceResponse
    {
        /// <summary>Identitas mesin persis seperti yang dikirim mesin; dipakai mengisi kode/ID eksternal saat mendaftarkan.</summary>
        public string ExternalDeviceId { get; set; } = string.Empty;

        public int HeldScanCount { get; set; }
        public DateTime FirstEventAt { get; set; }
        public DateTime LastEventAt { get; set; }
        public DateTime LastReceivedAt { get; set; }

        /// <summary>Terisi bila identitas ini milik mesin yang terdaftar tetapi nonaktif; cukup diaktifkan, tidak perlu didaftarkan ulang.</summary>
        public Guid? InactiveAttendanceDeviceId { get; set; }

        public string? InactiveAttendanceDeviceCode { get; set; }
    }

    public class CreateAttendanceDeviceRequest
    {
        [Required, MaxLength(50)]
        public string AttendanceDeviceCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string AttendanceDeviceName { get; set; } = string.Empty;

        /// <summary>Salah satu nilai DeviceTypeOptions pada filters/metadata.</summary>
        [Required, MaxLength(50)]
        public string DeviceType { get; set; } = "Fingerprint";

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? ModelName { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        public int? Port { get; set; }

        [MaxLength(50)]
        public string? MacAddress { get; set; }

        [MaxLength(100)]
        public string? IntegrationProvider { get; set; }

        [MaxLength(100)]
        public string? ExternalDeviceId { get; set; }

        public Guid? AttendanceLocationId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public bool IsPrimary { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>Default aktif. Mesin aktif langsung melepas scan tertahan yang identitasnya cocok.</summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Mengubah data mesin. Status aktif diubah lewat PATCH /{id}/status.</summary>
    public class UpdateAttendanceDeviceRequest
    {
        [Required, MaxLength(50)]
        public string AttendanceDeviceCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string AttendanceDeviceName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string DeviceType { get; set; } = "Fingerprint";

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? ModelName { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        public int? Port { get; set; }

        [MaxLength(50)]
        public string? MacAddress { get; set; }

        [MaxLength(100)]
        public string? IntegrationProvider { get; set; }

        [MaxLength(100)]
        public string? ExternalDeviceId { get; set; }

        public Guid? AttendanceLocationId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public bool IsPrimary { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateAttendanceDeviceStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
