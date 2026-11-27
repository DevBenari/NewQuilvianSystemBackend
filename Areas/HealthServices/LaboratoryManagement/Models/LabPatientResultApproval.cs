using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu keputusan penyetuju kedua atas <b>satu versi</b> dokumen yang memuat pemeriksaan sensitif
    /// (<c>LAB-DC-075</c>, <c>LAB-DEC-182</c>, <c>LAB-DEC-297</c>). Tidak pernah diubah; keputusan
    /// berlaku = baris terbaru per versi (<c>INV-74</c>).
    /// </summary>
    [Table("LabPatientResultApproval", Schema = "public")]
    public class LabPatientResultApproval : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VersionId { get; set; }

        public LabPatientResultApprovalDecision Decision { get; set; }

        /// <summary>Wajib bila ditolak. Dapat menyebut kondisi klinis — jangan dicatat ke log.</summary>
        [MaxLength(1000)]
        public string? Reason { get; set; }

        /// <summary>Penyetuju — selalu orang, bukan perilis order (<c>VAL-174</c>).</summary>
        public Guid DecidedByUserId { get; set; }

        public DateTime DecidedAt { get; set; }

        public LabPatientResultVersion? ResultVersion { get; set; }
    }
}
