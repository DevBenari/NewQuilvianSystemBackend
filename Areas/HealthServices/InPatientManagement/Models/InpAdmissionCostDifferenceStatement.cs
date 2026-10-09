using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Subjek pernyataan Selisih Biaya sebagai kode — kamus data 20.9 (<c>BE-RWI-192</c>,
    /// <c>FR-RWA-103</c>). Keterangan wajib bila "saudara kandung lainnya".
    /// </summary>
    public class InpAdmissionCostDifferenceStatement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public InpCostDifferenceSubject Subject { get; set; }

        /// <summary>Misalnya "kakak kandung".</summary>
        public string? SubjectOtherText { get; set; }

        public InpAdmissionDocument? Document { get; set; }
    }
}
