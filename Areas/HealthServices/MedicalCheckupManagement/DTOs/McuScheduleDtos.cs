namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Daily Schedule (BE-MCU-007)
    // =====================================================================

    public class McuDailyScheduleQuery
    {
        /// <summary>Kosong = hari ini (WIB).</summary>
        public DateOnly? From { get; set; }

        /// <summary>Kosong = 29 hari sesudah <see cref="From"/>. Rentang paling panjang 92 hari.</summary>
        public DateOnly? To { get; set; }
    }

    /// <summary>
    /// Satu tanggal. Tanggal yang belum pernah diatur tetap dikembalikan dengan
    /// <see cref="IsConfigured"/> = <c>false</c> dan dianggap tutup, agar kalender tidak berlubang.
    /// </summary>
    public class McuDailyScheduleResponse
    {
        public Guid? Id { get; set; }
        public DateOnly ScheduleDate { get; set; }
        public bool IsConfigured { get; set; }
        public bool IsOpen { get; set; }
        public int Quota { get; set; }
        public int ApprovedOverrideQuota { get; set; }

        /// <summary>Kuota + pelampauan yang disetujui.</summary>
        public int Capacity { get; set; }
        public int UsedQuota { get; set; }
        public int RemainingQuota { get; set; }
        public Guid? DutyMcuDoctorId { get; set; }
        public string? DutyMcuDoctorName { get; set; }
        public int Version { get; set; }
        public List<McuQuotaOverrideResponse> QuotaOverrides { get; set; } = new();
    }

    public class McuQuotaOverrideResponse
    {
        public Guid Id { get; set; }
        public int AdditionalQuota { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid ApprovedByUserId { get; set; }
        public DateTime ApprovedAt { get; set; }
    }

    public class UpsertMcuDailyScheduleRangeRequest
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public bool IsOpen { get; set; } = true;

        /// <summary>≥ 0, dan tidak boleh di bawah peserta yang sudah terjadwal (<c>VAL-MCU-14</c>).</summary>
        public int Quota { get; set; }

        /// <summary>Dokter MCU jaga (<c>MCU-DEC-042</c>). Kosong = tanpa dokter jaga.</summary>
        public Guid? DutyMcuDoctorId { get; set; }
    }

    public class CreateMcuQuotaOverrideRequest
    {
        /// <summary>≥ 1.</summary>
        public int AdditionalQuota { get; set; }

        /// <summary>Wajib (<c>VAL-MCU-13</c>), contoh <i>Kontrak tahunan PT Contoh</i>.</summary>
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Hasil pakai/lepas kuota bagi service pemanggil (booking, check-in, jadwal ulang).</summary>
    public class McuQuotaReservation
    {
        public Guid DailyScheduleId { get; set; }
        public DateOnly ScheduleDate { get; set; }
        public int UsedQuota { get; set; }
        public int Capacity { get; set; }
        public Guid? DutyMcuDoctorId { get; set; }
    }
}
