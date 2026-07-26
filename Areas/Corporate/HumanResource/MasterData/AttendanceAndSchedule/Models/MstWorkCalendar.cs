using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstWorkCalendar", Schema = "public")]
    public class MstWorkCalendar : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? HospitalSiteId { get; set; }

        [Required]
        [MaxLength(50)]
        public string WorkCalendarCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string WorkCalendarName { get; set; } = string.Empty;

        public int CalendarYear { get; set; } = DateTime.UtcNow.Year;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string TimeZoneId { get; set; } = "Asia/Jakarta";

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public MstHospitalSite? HospitalSite { get; set; }

        public ICollection<MstHoliday> Holidays { get; set; }
            = new List<MstHoliday>();
    }
}
