using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Angka dan sakelar yang wajar berbeda antar rumah sakit dan antar unit — satu baris per unit HD.
    /// </summary>
    /// <remarks>
    /// Memenuhi syarat 3 dan 5. Tidak satu pun angka ini boleh ditanam di kode maupun di frontend;
    /// bila baris pengaturan unit tidak ada, alur yang membutuhkannya ditolak, bukan diisi nilai
    /// bawaan diam-diam.
    /// </remarks>
    [Table("HmdSetting", Schema = "public")]
    public class HmdSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ServiceUnitId { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public int MaxPatientsPerNurse { get; set; } = 3;

        public bool EnforceNurseRatio { get; set; }

        public int WaterResultValidityHours { get; set; } = 720;

        public bool EnforceCompetencyCheck { get; set; }

        public bool AllowMultipleActiveEpisodePerPatient { get; set; }

        public int SessionStartGraceMinutes { get; set; } = 60;

        /// <summary>Pengesah harus berbeda dari penyelesai dokumentasi (<c>HMD-GATE-006</c>).</summary>
        public bool RequireDifferentSigner { get; set; } = true;

        /// <summary>
        /// Tindakan <c>MstProcedure</c> yang dibentuk menjadi <c>TrxPatientProcedure</c> saat sesi
        /// dimulai. Disimpan sebagai data, bukan dicari dari kode tindakan yang ditanam di kode.
        /// </summary>
        public Guid? ProcedureId { get; set; }

        public MstProcedure? Procedure { get; set; }

        public int Version { get; set; }
    }
}
