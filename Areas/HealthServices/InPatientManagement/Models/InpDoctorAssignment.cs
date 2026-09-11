using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpDoctorAssignment", Schema = "public")]
    public class InpDoctorAssignment : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Peran dokter pada baris ini. Kolom baru <c>BE-RWI-074</c>, bernilai bawaan
        /// <see cref="InpDoctorAssignmentRole.Dpjp"/> sehingga seluruh baris lama sah tanpa
        /// perlu ditebak.
        /// </summary>
        /// <remarks>
        /// <b>Peran tidak boleh diubah pada baris yang sudah ada.</b> Mengubah
        /// <c>Consultant</c> menjadi <c>Dpjp</c> di tempat akan menulis ulang sejarah:
        /// dokumen yang ditulis semasa ia konsulen mendadak terbaca seakan ditulis DPJP.
        /// Penugasan adalah catatan berperiode, dan catatan berperiode dikoreksi dengan
        /// menutup baris lama lalu membuka baris baru — <c>state-transition-matrix.md</c>
        /// bagian 6A.3.
        /// </remarks>
        [Required]
        public InpDoctorAssignmentRole AssignmentRole { get; set; } = InpDoctorAssignmentRole.Dpjp;

        /// <summary>
        /// Urutan penugasan, dimulai dari 1. Deretnya <b>satu per episode</b>, bukan satu
        /// per peran, sehingga konsulen dan dokter jaga ikut mengambil nomor dari deret yang
        /// sama.
        /// </summary>
        public int SequenceNumber { get; set; }

        public DateTime StartDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndDateTime { get; set; }

        [Required]
        public Guid AssignedByUserId { get; set; }

        [MaxLength(500)]
        public string? HandoverReason { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public MstDoctor? Doctor { get; set; }

        public ApplicationUser? AssignedByUser { get; set; }
    }
}
