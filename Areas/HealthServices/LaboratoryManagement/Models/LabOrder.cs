using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Pesanan pemeriksaan laboratorium.
    ///
    /// Entity ini otoritatif atas keadaan operasional pesanan saja. Tidak ada satu pun kolom
    /// finansial di sini — tidak ada Paid, Settlement, PayerApproval, Void, Refund, maupun
    /// Reversal. Akibat finansial sepenuhnya milik Billing sesuai <c>RJ-BIL-GATE-DEC-003</c>.
    /// </summary>
    public class LabOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Procedure utama pesanan. Sejak <c>RJ-BIL-BE-003</c> setiap sampel membawa procedure
        /// komponen pemeriksaannya sendiri, sehingga satu pesanan dapat memuat beberapa
        /// pemeriksaan dengan tarif berbeda. Nilai ini tetap dipertahankan sebagai procedure
        /// yang dipesan dokter dan sebagai default komponen pertama.
        /// </summary>
        [Required]
        public Guid ProcedureId { get; set; }

        public LabOrderStatus OrderStatus { get; set; } = LabOrderStatus.Requested;

        /// <summary>
        /// Status operasional sebelum pesanan ditahan. Disimpan agar <c>OnHold</c> benar-benar
        /// mempertahankan keadaan sebelumnya dan dapat dilanjutkan tanpa menebak.
        /// </summary>
        public LabOrderStatus? StatusBeforeHold { get; set; }

        public DateTime? RequestedAt { get; set; }

        public Guid? RequestedByUserId { get; set; }

        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Token konkurensi. Dua petugas yang memindahkan status pesanan yang sama secara
        /// bersamaan tidak boleh sama-sama berhasil.
        /// </summary>
        public int Version { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public MstProcedure? Procedure { get; set; }

        public ICollection<LabSpecimen> Specimens { get; set; } = new List<LabSpecimen>();
    }
}
