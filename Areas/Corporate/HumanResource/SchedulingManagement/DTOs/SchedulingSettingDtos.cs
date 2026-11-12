using System;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs
{
    public sealed class SchedulingSettingResponse
    {
        public Guid Id { get; set; }
        public Guid HospitalSiteId { get; set; }
        public int MinRestHoursBetweenShifts { get; set; } = 11;
        public int MaxConsecutiveWorkDays { get; set; } = 6;
        public int MaxConsecutiveNightShifts { get; set; } = 2;
        public int RetroactiveCorrectionWindowDays { get; set; } = 3;
        public bool RestHoursEscalatedToBlock { get; set; }
        public bool ConsecutiveDaysEscalatedToBlock { get; set; }
        public bool NightShiftsEscalatedToBlock { get; set; }
        public Guid RowVersion { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class UpdateSchedulingSettingRequest
    {
        public Guid? HospitalSiteId { get; set; }

        [Range(8, 24, ErrorMessage = "VR-SCH-80: Jeda istirahat minimal antar shift harus antara 8 dan 24 jam.")]
        public int MinRestHoursBetweenShifts { get; set; } = 11;

        [Range(1, 14, ErrorMessage = "VR-SCH-80: Batas maksimal hari kerja berturut-turut harus antara 1 dan 14 hari.")]
        public int MaxConsecutiveWorkDays { get; set; } = 6;

        [Range(1, 7, ErrorMessage = "VR-SCH-80: Batas maksimal shift malam berturut-turut harus antara 1 dan 7 hari.")]
        public int MaxConsecutiveNightShifts { get; set; } = 2;

        [Range(0, 30, ErrorMessage = "VR-SCH-80: Batas maksimal toleransi koreksi mundur harus antara 0 dan 30 hari.")]
        public int RetroactiveCorrectionWindowDays { get; set; } = 3;

        public bool RestHoursEscalatedToBlock { get; set; }
        public bool ConsecutiveDaysEscalatedToBlock { get; set; }
        public bool NightShiftsEscalatedToBlock { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan untuk pencegahan konflik pembaruan konkuren.")]
        public Guid RowVersion { get; set; }
    }
}
