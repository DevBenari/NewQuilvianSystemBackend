using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Administrator.UserManagement.Models
{
    [Table("DctFeeProfile", Schema = "public")]
    public class DctFeeProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DoctorId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal ConsultationFee { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal FollowUpFee { get; set; } = 0;

        [Column(TypeName = "numeric(18,2)")]
        public decimal TelemedicineFee { get; set; } = 0;

        [Column(TypeName = "numeric(5,2)")]
        public decimal DoctorSharePercentage { get; set; } = 0;

        [MaxLength(50)]
        public string? FeeCalculationType { get; set; }

        public bool IsFeeActive { get; set; } = true;

        public MstDoctor? Doctor { get; set; }
    }
}
