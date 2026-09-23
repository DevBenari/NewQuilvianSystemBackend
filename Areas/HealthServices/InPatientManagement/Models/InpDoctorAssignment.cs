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
        /// Alasan penugasan ini dibuat. Kolom baru <c>BE-RWI-079</c>, bernilai bawaan
        /// <see cref="InpDoctorAssignmentPurpose.Regular"/> sehingga seluruh baris lama sah
        /// tanpa perlu ditebak.
        /// </summary>
        /// <remarks>
        /// <b>Tujuan bukan peran.</b> Peran menjawab kewenangan, tujuan menjawab sebab. Dokter
        /// jaga yang benar-benar menjaga shift dan dokter jaga yang hanya diberi jendela satu
        /// jam untuk menyelesaikan catatan tertinggal berperan sama, dan hanya kolom ini yang
        /// membedakan keduanya — <c>RWI-DEC-130</c>.
        ///
        /// <para>
        /// <b>Nilai tidak boleh diubah pada baris yang sudah ada,</b> dengan alasan yang sama
        /// seperti <see cref="AssignmentRole"/>: mengubahnya menulis ulang sebab sebuah
        /// penugasan dibuat, dan laporan jumlah jaga yang dibaca dari kolom ini berubah untuk
        /// periode yang sudah lewat. Koreksi dilakukan dengan menutup baris lama lalu membuka
        /// baris baru — <c>state-transition-matrix.md</c> bagian 6A.3.
        /// </para>
        ///
        /// <para>
        /// <b>Penjaganya ada di database, bukan hanya di service.</b> Check constraint
        /// <c>CK_InpDoctorAssignment_LateDocumentation</c> menolak baris
        /// <see cref="InpDoctorAssignmentPurpose.LateDocumentation"/> yang tidak berperan
        /// <see cref="InpDoctorAssignmentRole.OnCallDoctor"/>, tidak berwaktu selesai, atau
        /// tidak beralasan — <c>INV-INP-12</c>.
        /// </para>
        /// </remarks>
        [Required]
        public InpDoctorAssignmentPurpose AssignmentPurpose { get; set; }
            = InpDoctorAssignmentPurpose.Regular;

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
