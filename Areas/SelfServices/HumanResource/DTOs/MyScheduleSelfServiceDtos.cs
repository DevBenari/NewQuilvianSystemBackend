namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs
{
    public class MyScheduleQueryRequest
    {
        /// <summary>Default awal bulan berjalan. Rentang yang dapat dilihat: 3 bulan ke belakang sampai akhir bulan depan (HRD-ASM-80).</summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>Default akhir bulan berjalan. Satu permintaan paling panjang 31 hari.</summary>
        public DateOnly? ToDate { get; set; }
    }

    public class MyUnitRosterQueryRequest
    {
        /// <summary>Format yyyy-MM; default bulan berjalan. Bulan yang dapat dilihat: 3 bulan ke belakang sampai bulan depan.</summary>
        public string? Month { get; set; }
    }

    public class MyScheduleDayResponse
    {
        public DateOnly Date { get; set; }

        /// <summary>
        /// Scheduled, RestDay, Holiday, Leave, SickLeave, OfficialDuty, StudyAssignment, atau NoSchedule.
        /// Ketidakhadiran (cuti, dinas luar, tugas belajar) mengalahkan jadwal pada hari yang sama.
        /// </summary>
        public string DayStatus { get; set; } = string.Empty;

        public string DayStatusLabel { get; set; } = string.Empty;

        /// <summary>PublishedRoster atau FixedWorkSchedule; null bila tidak ada jadwal terbit.</summary>
        public string? ScheduleSource { get; set; }

        public string? WorkLocationName { get; set; }
        public string? LeaveTypeName { get; set; }

        public bool IsHoliday { get; set; }
        public List<string> HolidayNames { get; set; } = new();

        /// <summary>Shift dari roster yang sudah terbit (roster Draft tidak pernah tampil).</summary>
        public List<MyScheduleShiftResponse> Shifts { get; set; } = new();

        /// <summary>Jam kerja tetap untuk pegawai unit jam kantor.</summary>
        public MyFixedScheduleResponse? FixedSchedule { get; set; }

        public List<MyOnCallResponse> OnCalls { get; set; } = new();

        /// <summary>Shift terbit yang semula milik pegawai ini tetapi sudah dialihkan ke petugas lain atau dihapus.</summary>
        public List<MyReleasedShiftResponse> ReleasedShifts { get; set; } = new();
    }

    public class MyScheduleShiftResponse
    {
        public Guid ShiftAssignmentId { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }
        public string? ColorCode { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsNightShift { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public string RosterPeriodCode { get; set; } = string.Empty;

        /// <summary>Benar bila shift ini berubah sesudah roster terbit (HRD-DEC-195, HRD-DEC-209 butir 3).</summary>
        public bool IsChanged { get; set; }

        public string? ChangeType { get; set; }
        public string? ChangeTypeLabel { get; set; }
        public DateTime? ChangedAt { get; set; }
        public string? PreviousShiftName { get; set; }
        public DateTime? PreviousStartAt { get; set; }
        public DateTime? PreviousEndAt { get; set; }
    }

    public class MyFixedScheduleResponse
    {
        public string? WorkScheduleName { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }

    public class MyOnCallResponse
    {
        public Guid OnCallAssignmentId { get; set; }
        public string? OnCallTypeName { get; set; }
        public string OnCallRole { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string AssignmentStatus { get; set; } = string.Empty;
    }

    public class MyReleasedShiftResponse
    {
        public Guid ShiftAssignmentId { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string ChangeTypeLabel { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }

    public class MyUnitRosterResponse
    {
        public string Month { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public List<MyUnitRosterUnitResponse> Units { get; set; } = new();
    }

    public class MyUnitRosterUnitResponse
    {
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public List<string> RosterPeriodCodes { get; set; } = new();
        public List<MyUnitRosterMemberResponse> Members { get; set; } = new();
    }

    public class MyUnitRosterMemberResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsMe { get; set; }

        /// <summary>
        /// Hanya hari dinas. Tanggal yang tidak ada di daftar ditampilkan "Libur" — termasuk hari rekan cuti, sakit,
        /// dinas luar, atau tugas belajar; alasannya tidak pernah dikirim (HRD-DEC-209 butir 4).
        /// </summary>
        public List<MyUnitRosterDayResponse> Days { get; set; } = new();
    }

    public class MyUnitRosterDayResponse
    {
        public DateOnly Date { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }
        public string? ColorCode { get; set; }
    }
}
