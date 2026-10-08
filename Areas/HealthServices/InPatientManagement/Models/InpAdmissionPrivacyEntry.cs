using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Satu kerabat yang boleh menjenguk atau satu permintaan khusus — kamus data 20.7
    /// (<c>BE-RWI-192</c>). Satu nama satu baris, tidak digabung koma: "Sdr. Dimas, Jr." tetap satu
    /// baris (kelemahan V1 nomor 9). Paling banyak tiga baris per jenis (G-40).
    /// </summary>
    public class InpAdmissionPrivacyEntry : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public InpAdmissionPrivacyEntryType EntryType { get; set; }

        /// <summary>1–3, mengikuti tiga baris V1.</summary>
        public int LineNo { get; set; }

        /// <summary>SENSITIF.</summary>
        public string Text { get; set; } = string.Empty;

        public InpAdmissionDocument? Document { get; set; }
    }
}
