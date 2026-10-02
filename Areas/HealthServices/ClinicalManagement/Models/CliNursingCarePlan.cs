using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Rencana asuhan keperawatan satu perawatan rawat inap — <c>CAP-013</c>,
    /// <c>BE-RWI-059</c>, <c>FR-KEP-012</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Satu perawatan tepat satu rencana asuhan.</b> Yang banyak adalah butir masalahnya,
    /// bukan rencananya. Bila satu perawatan boleh memiliki dua rencana asuhan, perawat pada
    /// giliran berikutnya tidak punya cara mengetahui mana yang sedang berlaku. Karena itu
    /// <c>InpEpisodeId</c> dijaga unique parsial pada baris yang belum terhapus.
    /// </para>
    /// <para>
    /// <b>Nama tabelnya bukan <c>Trx*</c>.</b> <c>QBE-NAM-001</c> melarang awalan itu untuk kode
    /// baru; prefix registry milik <c>ClinicalManagement</c> adalah <c>Cli</c>. Entity, berkas,
    /// configuration, <c>DbSet</c>, dan nama tabel adalah satu paket:
    /// <c>CliNursingCarePlan</c> / <c>CliNursingCarePlan.cs</c> /
    /// <c>CliNursingCarePlanConfiguration</c> / <c>CliNursingCarePlans</c> /
    /// <c>public."CliNursingCarePlan"</c>. <c>02-backend-architecture.md</c> bagian 4.2 yang
    /// masih menulis <c>TrxNursingCarePlan</c> dibaca sebagai usulan bentuk, bukan nama final.
    /// </para>
    /// <para>
    /// <b>Tabel ini milik <c>ClinicalManagement</c>, bukan milik Rawat Inap.</b>
    /// <c>RWI-DEC-081</c> menaruh seluruh tabel dokumentasi klinis rawat inap di sini;
    /// membuat <c>InpNursingCarePlan</c> akan melanggarnya.
    /// </para>
    /// </remarks>
    [Table("CliNursingCarePlan", Schema = "public")]
    public class CliNursingCarePlan : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Jangkar klinis. Menjembatani rencana asuhan ke mesin klinis lainnya.</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang dinaungi rencana ini. <b>Wajib</b>, dan unique parsial:
        /// satu perawatan tepat satu rencana asuhan.
        /// </summary>
        [Required]
        public Guid InpEpisodeId { get; set; }

        /// <summary>Pasien pemilik rencana. Penjaga salah pasien.</summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Saat rencana asuhan dibuka.</summary>
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Perawat yang membuka rencana asuhan, diturunkan dari pengguna yang terautentikasi.
        /// </summary>
        /// <remarks>
        /// Tidak diterima dari badan permintaan. Menerimanya dari klien berarti siapa pun dapat
        /// mengaku membuka rencana atas nama perawat lain.
        /// </remarks>
        [Required]
        public Guid OpenedByEmployeeId { get; set; }

        /// <summary>Penanda rencana yang masih dipakai ruang kerja keperawatan.</summary>
        public bool IsActive { get; set; } = true;

        public RegPatientEncounter? Encounter { get; set; }

        public InpEpisode? InpEpisode { get; set; }

        public MstPatient? Patient { get; set; }

        public MstEmployee? OpenedByEmployee { get; set; }

        /// <summary>Butir masalah keperawatan pada rencana ini.</summary>
        public ICollection<CliNursingCarePlanItem> Items { get; set; } = [];
    }
}
