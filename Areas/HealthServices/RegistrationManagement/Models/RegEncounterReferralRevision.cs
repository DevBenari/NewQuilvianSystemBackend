using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Jejak satu perubahan rincian rujukan atau suratnya (<c>RJ-DOC-DEC-078</c>).
    /// </summary>
    /// <remarks>
    /// Hanya ruas yang berubah yang disimpan, sebagai JSON nilai lama dan nilai baru. Isinya
    /// dapat memuat diagnosa dan alasan, sehingga tidak boleh disalin ke application log.
    /// </remarks>
    [Table("RegEncounterReferralRevision", Schema = "public")]
    public class RegEncounterReferralRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterReferralId { get; set; }

        public ReferralRevisionType RevisionType { get; set; }

        public string? OldValuesJson { get; set; }

        public string? NewValuesJson { get; set; }

        public DateTime ChangedAt { get; set; }

        public Guid? ChangedBy { get; set; }

        public RegEncounterReferral? EncounterReferral { get; set; }
    }
}
