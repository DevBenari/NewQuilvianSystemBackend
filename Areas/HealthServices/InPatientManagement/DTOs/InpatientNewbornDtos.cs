using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

public sealed class InpatientNewbornAdmissionContext
{
    public Guid DeliveryCaseId { get; set; }
    [Required, MaxLength(20)] public string Source { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string CareMode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string CareSource { get; set; } = string.Empty;
    public bool RequiresIntensiveCare { get; set; }
    /// <summary>
    /// Wajib bila DPJP bayi bukan dokter anak (K-05 ISSUE-EPS-040). Disimpan sebagai baris
    /// "Alasan DPJP non-Anak: …" pada catatan admisi; tidak punya kolom sendiri.
    /// </summary>
    [MaxLength(300)] public string? NonPediatricDoctorReason { get; set; }
    [Obsolete("Penagihan invoice adalah ranah Billing/Cashier/AR dan tidak dipilih saat admisi bayi.")]
    [MaxLength(20)] public string? FamilyBillingPreference { get; set; }
    [Obsolete("Bayi tidak membuat rencana deposit tambahan pada admisi.")]
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal? PlannedDepositAmount { get; set; }
}

public sealed class NewbornMotherCandidateQuery : IValidatableObject
{
    public string? Search { get; set; }
    [DataType(DataType.Date)] public DateTime? FromDate { get; set; }
    [DataType(DataType.Date)] public DateTime? ToDate { get; set; }
    [Range(1, 100000)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date)
        {
            yield return new ValidationResult(
                "Tanggal awal tidak boleh melewati tanggal akhir.",
                [nameof(FromDate), nameof(ToDate)]);
        }
    }
}

public sealed class NewbornMotherCandidateResponse
{
    public Guid DeliveryCaseId { get; set; }
    public string DeliveryCaseNumber { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int DeliveryCaseStatus { get; set; }
    public string DeliveryCaseStatusName { get; set; } = string.Empty;
    public Guid MotherPatientId { get; set; }
    public string MotherName { get; set; } = string.Empty;
    public string MotherMedicalRecordNumber { get; set; } = string.Empty;
    public Guid MotherEncounterId { get; set; }
    public string MotherEncounterNumber { get; set; } = string.Empty;
    public Guid PaymentSourceId { get; set; }
    public DateTime PaymentSourceExpectedAt { get; set; }
    public int PaymentType { get; set; }
    public string? PaymentSourceName { get; set; }
    public Guid? MotherEpisodeId { get; set; }
    public string? MotherEpisodeNumber { get; set; }
    public Guid? CurrentServiceUnitId { get; set; }
    public string? CurrentServiceUnitName { get; set; }
    public Guid? CurrentRoomId { get; set; }
    public string? CurrentRoomName { get; set; }
    /// <summary>Bed penempatan aktif ibu, contoh "303-A" (FIX-EPS-040-12).</summary>
    public Guid? CurrentBedId { get; set; }
    public string? CurrentBedCode { get; set; }
    public string? CurrentBedName { get; set; }
    /// <summary>Kelas perawatan penempatan aktif ibu, contoh "VIP".</summary>
    public string? CurrentPatientClassName { get; set; }
    public int RegisteredBabyCount { get; set; }
    public int NextBirthOrderPreview { get; set; }
    public bool HasActiveInpatientBaby { get; set; }
    public List<NewbornRegisteredBabyDto> RegisteredBabies { get; set; } = [];
}

public sealed class NewbornRegisteredBabyDto
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public int BirthOrder { get; set; }
    public Guid? EpisodeId { get; set; }
    public string? EpisodeNumber { get; set; }
    public int? EpisodeStatus { get; set; }
    public string? EpisodeStatusName { get; set; }
    public string? RoomName { get; set; }
    public string? BedCode { get; set; }
    public bool IsCurrentlyInRoom { get; set; }
}

public sealed class NewbornMotherCandidatePagedResult
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
    public List<NewbornMotherCandidateResponse> Items { get; set; } = [];
}
