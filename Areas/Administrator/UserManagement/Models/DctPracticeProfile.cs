using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Models
{
    [Table("DctPracticeProfile", Schema = "public")]
    public class DctPracticeProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DoctorId { get; set; }

        [MaxLength(100)]
        public string? PolyclinicName { get; set; }

        public int DefaultConsultationDurationMinutes { get; set; } = 15;

        public int MaxPatientPerSession { get; set; } = 0;

        public bool AllowOnlineAppointment { get; set; } = true;

        public bool AllowWalkInAppointment { get; set; } = true;

        public bool AllowTelemedicine { get; set; } = false;

        [MaxLength(500)]
        public string? PracticeNote { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDoctor? Doctor { get; set; }
    }
}
