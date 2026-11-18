using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs
{
    public class HolidayFilterMetadataResponse
    {
        /// <summary>Kalender kerja aktif beserta rentang tanggalnya; tanggal libur wajib berada di dalam rentang kalender yang dipilih.</summary>
        public List<HolidayWorkCalendarOptionResponse> WorkCalendarOptions { get; set; } = new();

        public List<HolidayOptionResponse> HolidayTypeOptions { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class HolidayOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class HolidayWorkCalendarOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CalendarYear { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsDefault { get; set; }
    }

    public class HolidayQueryRequest
    {
        public Guid? WorkCalendarId { get; set; }

        /// <summary>Hari libur yang tanggalnya bersinggungan dengan tahun ini.</summary>
        public int? Year { get; set; }

        public string? HolidayType { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>Cari pada kode atau nama hari libur.</summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class HolidayResponse
    {
        public Guid Id { get; set; }
        public Guid WorkCalendarId { get; set; }
        public string? WorkCalendarCode { get; set; }
        public string? WorkCalendarName { get; set; }
        public string HolidayCode { get; set; } = string.Empty;
        public string HolidayName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string HolidayType { get; set; } = string.Empty;
        public string HolidayTypeLabel { get; set; } = string.Empty;
        public bool IsNationalHoliday { get; set; }
        public bool IsPaidHoliday { get; set; }
        public bool IsRecurringAnnually { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class CreateHolidayRequest
    {
        [Required]
        public Guid WorkCalendarId { get; set; }

        [Required, MaxLength(50)]
        public string HolidayCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string HolidayName { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        /// <summary>Salah satu nilai HolidayTypeOptions pada filters/metadata.</summary>
        [Required, MaxLength(50)]
        public string HolidayType { get; set; } = "National";

        public bool IsNationalHoliday { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateHolidayRequest : CreateHolidayRequest
    {
    }

    public class UpdateHolidayStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
