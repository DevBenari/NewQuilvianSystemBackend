using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models
{
    /// <summary>
    /// Penutupan klinis satu kunjungan (`AGG-RM-02`, `RM-DEC-051`, `BE-25`).
    ///
    /// Dicatat SEKALI, pada penguncian otomatis pertama kunjungan itu, lalu tidak pernah
    /// berubah — termasuk ketika rekonsiliasi IGD mengubah <c>CompletedAt</c>. Konsep ini ada
    /// karena sebelumnya waktu tutup klinis hanya tersimpan pada dokumen yang kebetulan masih
    /// konsep; kunjungan tanpa konsep tidak meninggalkan jejak sama sekali.
    ///
    /// Keberadaan barisnya juga penanda "masuk daftar kerja kelengkapan" (`RM-DEC-050`): baris
    /// hanya lahir sejak fitur dirilis, sehingga kunjungan lama tidak memenuhi daftar kerja.
    /// </summary>
    [Table("MrcEncounterClinicalClosure", Schema = "public")]
    public class MrcEncounterClinicalClosure : IdentityModel
    {
        public Guid Id { get; set; }

        /// <summary>Unik — satu kunjungan, satu penutupan klinis.</summary>
        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>UTC. Waktu tutup klinis dari pemanggil, atau saat pencatatan.</summary>
        public DateTime ClinicalClosedAt { get; set; }

        public ClinicalClosureSource ClosureSource { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }
    }
}
