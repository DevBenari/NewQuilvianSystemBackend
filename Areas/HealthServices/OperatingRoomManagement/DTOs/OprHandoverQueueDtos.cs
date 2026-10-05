using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

/// <summary>
/// Saringan daftar serah terima pasca operasi per unit tujuan (<c>BE-RWI-177</c>, API 11.5.3).
/// </summary>
public class HandoverQueueQuery
{
    public Guid? DestinationUnitId { get; set; }

    /// <summary>Bawaan: semua status. Daftar Pantau memakai <c>Sent</c> dengan <c>OverdueOnly = true</c>.</summary>
    public OprHandoverStatus? Status { get; set; }

    /// <summary>
    /// Hanya serah terima <c>Sent</c> yang menunggu lebih lama dari
    /// <c>MstInpatientSetting.PendingSurgicalHandoverAlertMinutes</c>.
    /// </summary>
    public bool? OverdueOnly { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
}

/// <summary>Satu baris daftar serah terima (API 11.5.3).</summary>
public class HandoverQueueItemResponse
{
    public Guid HandoverId { get; set; }
    public Guid CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? MedicalRecordNumber { get; set; }
    public Guid DestinationUnitId { get; set; }
    public string DestinationUnitName { get; set; } = string.Empty;

    /// <summary>Unit bed aktif pasien saat ini; kosong bila pasien belum menempati bed.</summary>
    public string? CurrentUnitName { get; set; }

    /// <summary>Penanda "pasien belum di unit tujuan" bila <c>false</c> (<c>INV-RWF-28</c>).</summary>
    public bool PatientInDestinationUnit { get; set; }

    public OprHandoverStatus Status { get; set; }
    public string? SentByName { get; set; }
    public DateTime SentAt { get; set; }

    /// <summary>Menit sejak dikirim sampai sekarang (status <c>Sent</c>) atau sampai diproses.</summary>
    public int WaitingMinutes { get; set; }

    /// <summary>Masih <c>Sent</c> dan menunggu lebih lama dari ambang pengaturan.</summary>
    public bool IsOverdue { get; set; }
}
