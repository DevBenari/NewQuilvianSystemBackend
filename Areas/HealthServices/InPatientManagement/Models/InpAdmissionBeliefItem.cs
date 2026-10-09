using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Satu hal yang bertentangan dengan nilai dan kepercayaan pasien — kamus data 20.8
    /// (<c>BE-RWI-192</c>, <c>RWI-DEC-242</c>). 1–5 butir per dokumen; contoh "Tidak menerima
    /// transfusi darah". Isinya SENSITIF.
    /// </summary>
    public class InpAdmissionBeliefItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        /// <summary>Nomor butir tercetak 1–5.</summary>
        public int ItemNo { get; set; }

        /// <summary>SENSITIF.</summary>
        public string Text { get; set; } = string.Empty;

        public InpAdmissionDocument? Document { get; set; }
    }
}
