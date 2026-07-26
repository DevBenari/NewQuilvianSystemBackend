using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstHoliday", Schema = "public")]
    public class MstHoliday : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkCalendarId { get; set; }

        [Required]
        [MaxLength(50)]
        public string HolidayCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string HolidayName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string HolidayType { get; set; } = "National";
        // National, Regional, Hospital, Religious, CollectiveLeave, Other

        public bool IsNationalHoliday { get; set; } = false;

        public bool IsPaidHoliday { get; set; } = true;

        public bool IsRecurringAnnually { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkCalendar? WorkCalendar { get; set; }
    }
}
