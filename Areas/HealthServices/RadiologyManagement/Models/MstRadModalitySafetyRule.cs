using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Aturan yang mengikat: butir keselamatan mana yang berlaku untuk modalitas dan pemeriksaan
    /// mana, dan mana di antaranya yang wajib.
    ///
    /// <b>Tabel ini sengaja lahir kosong.</b> Tidak ada satu baris pun yang diisi migration, dan
    /// itu bukan kelalaian. <c>RJ-BIL-GATE-DEC-004</c> menyatakan daftar akhir gerbang
    /// keselamatan mengikuti SOP dan otoritas klinis, "bukan keputusan dokumen ini". Mengisinya
    /// dari sini berarti sebuah program menetapkan kapan pasien boleh disinari — dan itu bukan
    /// wewenang program.
    ///
    /// Akibatnya, sampai admin mengisi aturan untuk sebuah modalitas, acquisition pada modalitas
    /// itu **ditolak** dengan <c>RAD_SAFETY_POLICY_NOT_CONFIGURED</c>. Menolak adalah perilaku
    /// yang benar untuk konfigurasi yang belum ada: yang gagal terlihat dan dapat diperbaiki,
    /// sedangkan yang diloloskan diam-diam baru ketahuan setelah ada yang celaka.
    ///
    /// Pola ini sama dengan <c>MstBillingApprovalPolicy</c> pada <c>RJ-BIL-BE-006</c>, yang juga
    /// lahir kosong dan menahan tindakan sampai kebijakannya ditetapkan pemiliknya.
    /// </summary>
    public class MstRadModalitySafetyRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ModalityId { get; set; }

        /// <summary>
        /// Pemeriksaan tertentu yang dikenai aturan ini. Kosong berarti aturan berlaku untuk
        /// seluruh pemeriksaan pada modalitas tersebut.
        /// </summary>
        public Guid? ProcedureId { get; set; }

        [Required]
        public Guid SafetyRequirementId { get; set; }

        /// <summary>
        /// Wajib atau tidak. Butir wajib yang berkeadaan <c>Pending</c> atau <c>Failed</c>
        /// memblokir acquisition normal, sesuai invariant <c>GATE-DEC-004</c>.
        /// </summary>
        public bool IsMandatory { get; set; } = true;

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Versi aturan. Study yang sudah berjalan membekukan versi yang berlaku saat itu,
        /// sehingga perubahan aturan di kemudian hari tidak menulis ulang penilaian yang sudah
        /// terjadi.
        /// </summary>
        public int RuleVersion { get; set; } = 1;

        public string? Note { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public MstRadModality? Modality { get; set; }

        public MstRadSafetyRequirement? SafetyRequirement { get; set; }

        public MstProcedure? Procedure { get; set; }
    }
}
