using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    // =========================================================================
    // MAR — BE-RWI-114 s.d. BE-RWI-118, api-contract keperawatan 0.5.0 bagian 7.11 dan 7.13.
    // =========================================================================

    /// <summary>MAR satu hari: butir resep sebagai baris, dosis sebagai sel.</summary>
    public class MedicationAdministrationChartResponse
    {
        public Guid EpisodeId { get; set; }

        /// <summary>Tanggal jam dinding rumah sakit.</summary>
        public DateOnly Date { get; set; }

        public DateTime WindowStartUtc { get; set; }

        public DateTime WindowEndUtc { get; set; }

        /// <summary><c>false</c> bila perawatan sudah ditutup — MAR hanya-baca.</summary>
        public bool IsEpisodeWritable { get; set; }

        public string? EpisodeReadOnlyReason { get; set; }

        /// <summary>Penanda "lewat waktu" hanya aktif bila terisi — <c>VAL-KEP-36a</c>/<c>36b</c>.</summary>
        public int? MissedAfterMinutes { get; set; }

        public List<MedicationAdministrationChartItem> Items { get; set; } = new();

        /// <summary>Kode frekuensi butir episode yang belum punya jadwal — <c>VAL-KEP-36e</c>.</summary>
        public List<string> FrequencyCodesWithoutSchedule { get; set; } = new();

        /// <summary>Terisi bila pembentukan dosis gagal; dosis yang sudah ada tetap ditampilkan.</summary>
        public string? DoseGenerationWarning { get; set; }
    }

    public class MedicationAdministrationChartItem
    {
        public Guid PrescriptionItemId { get; set; }

        public Guid PrescriptionId { get; set; }

        public string PrescriptionNumber { get; set; } = string.Empty;

        public Guid DrugId { get; set; }

        public string DrugName { get; set; } = string.Empty;

        public string? GenericName { get; set; }

        public string? Strength { get; set; }

        public decimal Dose { get; set; }

        public string? DoseUnit { get; set; }

        public string? Route { get; set; }

        public string? FrequencyCode { get; set; }

        public string? FrequencyText { get; set; }

        public string? Signa { get; set; }

        public string? AdministrationInstruction { get; set; }

        public bool IsStopped { get; set; }

        public DateTime? StoppedAt { get; set; }

        public bool IsHighAlert { get; set; }

        public bool IsAsNeeded { get; set; }

        public PrescriptionDoseKind DoseKind { get; set; }

        /// <summary><c>true</c> bila butir berjadwal punya jam pemberian untuk unit episode atau bawaan.</summary>
        public bool ScheduleConfigured { get; set; }

        /// <summary>"Jadwal pemberian untuk frekuensi q6h belum dikonfigurasi" — <c>VAL-KEP-36e</c>.</summary>
        public string? ScheduleMessage { get; set; }

        public List<string> ScheduleTimes { get; set; } = new();

        public List<MedicationAdministrationListItem> Doses { get; set; } = new();
    }

    public class MedicationAdministrationListItem
    {
        public Guid Id { get; set; }

        public string AdministrationNumber { get; set; } = string.Empty;

        public Guid PrescriptionItemId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string DrugName { get; set; } = string.Empty;

        public MedicationDoseSource DoseSource { get; set; }

        public string DoseSourceLabel { get; set; } = string.Empty;

        public DateTime? ScheduledAt { get; set; }

        public MedicationDoseStatus DoseStatus { get; set; }

        public string DoseStatusLabel { get; set; } = string.Empty;

        public MedicationDoubleCheckStatus DoubleCheckStatus { get; set; }

        public string DoubleCheckStatusLabel { get; set; } = string.Empty;

        public decimal? PlannedDose { get; set; }

        public string? PlannedDoseUnit { get; set; }

        public decimal? ActualDose { get; set; }

        public string? ActualDoseUnit { get; set; }

        public string? ActualRoute { get; set; }

        public DateTime? AdministeredAt { get; set; }

        public Guid? RecordedByEmployeeId { get; set; }

        public string? RecordedByName { get; set; }

        public Guid? RecordedByUserId { get; set; }

        public bool IsHighAlert { get; set; }

        /// <summary>Penanda tampilan lewat waktu; status tidak berubah otomatis — <c>VAL-KEP-36a</c>.</summary>
        public bool IsOverdue { get; set; }

        /// <summary>"Tidak dicatat sebelum perawatan ditutup" — state matrix 5.5.</summary>
        public bool IsUnrecordedAtClosure { get; set; }

        /// <summary>Dosis diberikan yang sudah punya entri intake cairan aktif — <c>BE-RWI-122</c>.</summary>
        public bool HasIntakeEntry { get; set; }

        /// <summary>Pengguna yang membuka boleh melakukan cek ganda: bukan pencatatnya sendiri.</summary>
        public bool CanDoubleCheck { get; set; }

        public int RevisionNumber { get; set; }
    }

    public class MedicationAdministrationResponse : MedicationAdministrationListItem
    {
        public Guid PrescriptionId { get; set; }

        public Guid EncounterId { get; set; }

        public Guid DrugId { get; set; }

        public string? StatusReason { get; set; }

        public string? DeviationNote { get; set; }

        public DateTime? RecordedAt { get; set; }

        public Guid? DoubleCheckedByEmployeeId { get; set; }

        public string? DoubleCheckedByName { get; set; }

        public Guid? DoubleCheckedByUserId { get; set; }

        public DateTime? DoubleCheckedAt { get; set; }

        public string? DoubleCheckNote { get; set; }

        public string? PrnIndication { get; set; }

        public DateTime? PrnEvaluationDueAt { get; set; }

        public string? PrnEvaluationNote { get; set; }

        public DateTime? PrnEvaluatedAt { get; set; }

        public Guid? PrnEvaluatedByUserId { get; set; }

        public Guid? SlidingScaleExecutionId { get; set; }

        public Guid? LinkedFluidEntryId { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public List<string> AvailableActions { get; set; } = new();

        public bool IsReplay { get; set; }
    }

    /// <summary>Mencatat hasil dosis <c>Due</c> — <c>PATCH /{id}/record</c>.</summary>
    public class RecordMedicationAdministrationRequest
    {
        /// <summary><c>Administered</c>, <c>Held</c>, <c>Refused</c>, atau <c>Missed</c>.</summary>
        public MedicationDoseStatus DoseStatus { get; set; }

        public decimal? ActualDose { get; set; }

        public string? ActualRoute { get; set; }

        public DateTime? AdministeredAt { get; set; }

        public string? StatusReason { get; set; }

        public string? DeviationNote { get; set; }

        public int? ExpectedRevisionNumber { get; set; }

        public string? IdempotencyKey { get; set; }
    }

    /// <summary>Pemberian obat sesuai kebutuhan (PRN) — <c>POST /as-needed</c>.</summary>
    public class RecordAsNeededAdministrationRequest
    {
        public Guid PrescriptionItemId { get; set; }

        public decimal? ActualDose { get; set; }

        public string? ActualRoute { get; set; }

        public DateTime? AdministeredAt { get; set; }

        /// <summary>Wajib. "Nyeri skala 6".</summary>
        public string? PrnIndication { get; set; }

        /// <summary>Wajib bila dosis berbeda dari resep — <c>VAL-KEP-30c</c>.</summary>
        public string? DeviationNote { get; set; }

        public string? IdempotencyKey { get; set; }
    }

    /// <summary>Pemberian butir berfrekuensi yang jadwalnya belum dikonfigurasi — <c>POST /unscheduled</c>.</summary>
    public class RecordUnscheduledAdministrationRequest
    {
        public Guid PrescriptionItemId { get; set; }

        public decimal? ActualDose { get; set; }

        public string? ActualRoute { get; set; }

        public DateTime? AdministeredAt { get; set; }

        /// <summary>Wajib; disimpan pada <c>DeviationNote</c>.</summary>
        public string? UnscheduledReason { get; set; }

        public string? IdempotencyKey { get; set; }
    }

    /// <summary>Perawat kedua mengonfirmasi atau menolak — <c>PATCH /{id}/double-check</c>.</summary>
    public class DoubleCheckRequest
    {
        /// <summary><c>Confirm</c> atau <c>Reject</c>.</summary>
        public string? Decision { get; set; }

        /// <summary>Wajib bila <c>Reject</c>.</summary>
        public string? Note { get; set; }

        public int? ExpectedRevisionNumber { get; set; }
    }

    /// <summary>Koreksi hasil yang sudah tercatat — <c>PUT /{id}/correct</c>.</summary>
    public class CorrectMedicationAdministrationRequest
    {
        public MedicationDoseStatus DoseStatus { get; set; }

        public decimal? ActualDose { get; set; }

        public string? ActualRoute { get; set; }

        public DateTime? AdministeredAt { get; set; }

        public string? StatusReason { get; set; }

        public string? DeviationNote { get; set; }

        /// <summary>Wajib. "Salah pilih pasien bed 3, dosis dicatat ulang".</summary>
        public string? CorrectionReason { get; set; }

        public int? ExpectedRevisionNumber { get; set; }
    }

    public class PrnEvaluationRequest
    {
        public string? PrnEvaluationNote { get; set; }
    }

    public class MedicationAdministrationRevisionResponse
    {
        public Guid Id { get; set; }

        public int RevisionNumber { get; set; }

        /// <summary><c>Correction</c> atau <c>DoubleCheckRejected</c>.</summary>
        public string RevisionKind { get; set; } = string.Empty;

        public string RevisionKindLabel { get; set; } = string.Empty;

        public MedicationDoseStatus PreviousDoseStatus { get; set; }

        public string PreviousDoseStatusLabel { get; set; } = string.Empty;

        public decimal? PreviousActualDose { get; set; }

        public string? PreviousActualRoute { get; set; }

        public DateTime? PreviousAdministeredAt { get; set; }

        public string? PreviousStatusReason { get; set; }

        public string? PreviousDeviationNote { get; set; }

        public Guid? PreviousRecordedByUserId { get; set; }

        public string CorrectionReason { get; set; } = string.Empty;

        public Guid CorrectedByUserId { get; set; }

        public string? CorrectedByName { get; set; }

        public DateTime CorrectedAt { get; set; }
    }

    /// <summary>Hasil satu pembentukan dosis — dipakai pembuktian idempoten dan hosted service.</summary>
    public class DoseGenerationResult
    {
        public Guid EpisodeId { get; set; }

        public int CreatedCount { get; set; }

        public int ExistingCount { get; set; }

        /// <summary>Dosis <c>Due</c> butir yang sudah dihentikan yang dibatalkan jaring pengaman.</summary>
        public int CancelledStoppedCount { get; set; }

        public List<string> FrequencyCodesWithoutSchedule { get; set; } = new();

        public string? Warning { get; set; }
    }

    // =========================================================================
    // Pengaturan jadwal dan MAR — api-contract 7.13, FR-KEP-071.
    // =========================================================================

    public class MedicationScheduleTimeSetResponse
    {
        public string FrequencyCode { get; set; } = string.Empty;

        public Guid? ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        /// <summary><c>true</c> = jadwal bawaan seluruh unit.</summary>
        public bool IsDefault { get; set; }

        /// <summary>Kali per hari yang dipakai resep aktif berkode ini, bila seragam.</summary>
        public int? ExpectedTimesPerDay { get; set; }

        public List<MedicationScheduleSlotResponse> Times { get; set; } = new();
    }

    public class MedicationScheduleSlotResponse
    {
        public Guid Id { get; set; }

        public int SlotNumber { get; set; }

        /// <summary>"08:00", jam dinding rumah sakit.</summary>
        public string TimeOfDay { get; set; } = string.Empty;
    }

    public class ReplaceMedicationScheduleTimesRequest
    {
        public string? FrequencyCode { get; set; }

        /// <summary><c>null</c> = jadwal bawaan.</summary>
        public Guid? ServiceUnitId { get; set; }

        /// <summary>"08:00", "20:00". Daftar kosong menghapus jadwal kode itu untuk unit atau bawaan.</summary>
        public List<string> Times { get; set; } = new();
    }

    public class MedicationAdministrationSettingResponse
    {
        public Guid? Id { get; set; }

        /// <summary><c>false</c> = belum pernah disimpan; nilai di bawah adalah bawaan.</summary>
        public bool IsConfigured { get; set; }

        public int DoseGenerationHorizonHours { get; set; }

        public int? MissedAfterMinutes { get; set; }

        public int? PrnEvaluationMinutes { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    public class UpdateMedicationAdministrationSettingRequest
    {
        public int DoseGenerationHorizonHours { get; set; }

        public int? MissedAfterMinutes { get; set; }

        public int? PrnEvaluationMinutes { get; set; }
    }
}
