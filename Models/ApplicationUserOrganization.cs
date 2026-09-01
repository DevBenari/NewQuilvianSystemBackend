using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Models
{
    [Table("AspNetUserOrganization", Schema = "public")]
    public class ApplicationUserOrganization : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        /// <summary>
        /// Penempatan organisasi otoritatif (<c>WfpOrganizationAssignment.Id</c>) yang menghasilkan
        /// baris proyeksi ini.
        ///
        /// Nullable karena baris warisan yang dibuat sebelum Phase A0 tidak selalu dapat
        /// dibuktikan sumbernya. Baris seperti itu dibiarkan bernilai <c>null</c> dan dilaporkan
        /// sebagai <i>legacy-unresolved</i>; menebak sumbernya berarti mengarang sejarah.
        /// </summary>
        public Guid? SourceAssignmentId { get; set; }

        /// <summary>
        /// Penanda penempatan utama. <b>Bukan</b> syarat kelayakan otorisasi: penempatan sekunder
        /// yang aktif dan masih berlaku tetap ikut menyumbang izin.
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public ApplicationUser? User { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }
    }
}
