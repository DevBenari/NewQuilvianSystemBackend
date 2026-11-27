using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu perubahan aktivasi penyerahan hasil kepada pasien untuk satu disiplin (<c>LAB-DC-079</c>,
    /// <c>LAB-DEC-295</c>). Keadaan kini = baris terbaru per disiplin; <b>tanpa baris = nonaktif</b>
    /// (<c>INV-80</c>).
    /// </summary>
    /// <remarks>
    /// Tabel peristiwa tersendiri, bukan kolom <see cref="LabDisciplineSetting"/>: pengubahnya memegang
    /// izin aktivasi tersendiri (<c>LAB-DEC-296</c>), sedangkan <c>PUT lab-discipline-settings</c>
    /// menimpa baris setelan utuh dengan <c>LabDisciplineSetting : Update</c>
    /// (<c>02-backend-architecture.md</c> 34.10 no. 2). Baris tidak pernah diubah atau dihapus.
    /// </remarks>
    [Table("LabPatientDeliveryActivation", Schema = "public")]
    public class LabPatientDeliveryActivation : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Patologi Klinik atau Mikrobiologi (<c>VAL-178</c>).</summary>
        public LabDiscipline Discipline { get; set; }

        /// <summary>Keadaan sesudah perubahan ini.</summary>
        public bool Enabled { get; set; }

        /// <summary>Alasan perubahan, mis. rujukan konfirmasi Kepala Instalasi (<c>LAB-SIGN-002</c>).</summary>
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Penegasan bahwa daftar pemeriksaan sensitif sudah ditetapkan — wajib saat mengaktifkan.</summary>
        public bool SensitiveListConfirmed { get; set; }

        public Guid ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}
