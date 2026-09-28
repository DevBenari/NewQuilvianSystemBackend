using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Petugas yang bertugas pada satu sesi beserta hasil pemeriksaan kewenangan klinisnya.
    /// </summary>
    /// <remarks>
    /// Selama pembacaan kewenangan dari Human Resource belum tersedia (<c>HMD-DEP-002</c>),
    /// <see cref="CompetencyVerificationStatus"/> bernilai <c>NotVerifiable</c> — tidak boleh
    /// diisi <c>Verified</c>.
    /// </remarks>
    [Table("HmdSessionStaffAssignment", Schema = "public")]
    public class HmdSessionStaffAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        public HmdSession? Session { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public HmdStaffRole StaffRole { get; set; }

        [Required]
        public Guid AssignedByUserId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public HmdCompetencyVerificationStatus CompetencyVerificationStatus { get; set; } =
            HmdCompetencyVerificationStatus.NotVerifiable;

        public DateTime? CompetencyCheckedAt { get; set; }

        [MaxLength(200)]
        public string? CompetencySourceReference { get; set; }
    }
}
