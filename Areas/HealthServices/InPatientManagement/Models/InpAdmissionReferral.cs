using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Permintaan admisi dari kamar pulih (<c>BE-RWI-181</c>, kamus data 19.8, <c>RWI-DEC-201</c>,
    /// <c>INV-RWF-32</c>). Tidak pernah membuat episode sendiri — petugas admisi yang membuka admisi
    /// berlangkah dari permintaan ini.
    /// </summary>
    /// <remarks>
    /// Contoh <c>UAT-RWF-16</c>: pasien poliklinik dioperasi lalu dokter anestesi memutuskan
    /// "Rawat inap". Satu permintaan <c>Pending</c> lahir dengan kunjungan asal = kunjungan
    /// poliklinik (= <c>OprCase.EncounterId</c>) dan muncul di daftar petugas admisi. Admisi yang
    /// dibuka dari permintaan itu membuat statusnya <c>Completed</c> dalam transaksi yang sama.
    /// Baris ini juga menjadi target FK <c>BilInvoiceEncounterLink.SourceReferralId</c>
    /// (<c>RWI-DEC-207</c>), sehingga tidak pernah dihapus fisik.
    /// </remarks>
    public class InpAdmissionReferral : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PatientId { get; set; }

        /// <summary>Kunjungan asal (poliklinik, IGD, ODC) = <c>OprCase.EncounterId</c>.</summary>
        public Guid SourceEncounterId { get; set; }

        public Guid OprCaseId { get; set; }

        /// <summary>Usulan DPJP awal; petugas admisi tetap memilih.</summary>
        public Guid PrimarySurgeonId { get; set; }

        public InpRequestedCareLevel RequestedCareLevel { get; set; }

        /// <summary>Salinan <c>OprRecovery.DecisionNote</c> saat dibuat. SENSITIF.</summary>
        public string? RecoveryDecisionNote { get; set; }

        public InpAdmissionReferralStatus Status { get; set; } = InpAdmissionReferralStatus.Pending;

        /// <summary>Dasar "lamanya menunggu" pada Daftar Pantau.</summary>
        public DateTime RequestedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        /// <summary>Contoh "Pasien boleh pulang dari kamar pulih". SENSITIF.</summary>
        public string? CancelledReason { get; set; }

        public Guid? CompletedEpisodeId { get; set; }

        public DateTime? CompletedAt { get; set; }

        /// <summary>Konkurensi optimistis; berganti setiap kali baris diubah.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstPatient? Patient { get; set; }

        public RegPatientEncounter? SourceEncounter { get; set; }

        public OprCase? OprCase { get; set; }

        public MstDoctor? PrimarySurgeon { get; set; }

        public InpEpisode? CompletedEpisode { get; set; }
    }
}
