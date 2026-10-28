using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Satu kriteria dalam profil dokumen wajib. Kriteria berasal dari katalog
    /// <see cref="CompletenessCriterion"/> — master hanya memilih, tidak mendefinisikan.
    /// </summary>
    [Table("MstMedicalRecordCompletenessProfileItem", Schema = "public")]
    public class MstMedicalRecordCompletenessProfileItem : IdentityModel
    {
        public Guid Id { get; set; }

        public Guid ProfileId { get; set; }

        public CompletenessCriterion Criterion { get; set; }

        public bool IsActive { get; set; } = true;

        public MstMedicalRecordCompletenessProfile? Profile { get; set; }
    }
}
