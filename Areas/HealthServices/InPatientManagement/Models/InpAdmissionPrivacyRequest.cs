using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Pilihan privasi selama transportasi pada Permintaan Privasi — kamus data 20.6
    /// (<c>BE-RWI-192</c>). Satu baris per dokumen; bawaan "Tidak" (PRD Lampiran A.5).
    /// </summary>
    public class InpAdmissionPrivacyRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public bool IsTransportPrivacyRequested { get; set; }

        public InpAdmissionDocument? Document { get; set; }
    }
}
