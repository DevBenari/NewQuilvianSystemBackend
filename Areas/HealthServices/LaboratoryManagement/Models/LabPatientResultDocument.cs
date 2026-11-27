using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Root aggregate <c>AGG-LAB-04</c> Dokumen Hasil untuk Pasien (<c>LAB-DC-071</c>): <b>satu per
    /// order</b> Patologi Klinik atau Mikrobiologi yang seluruh pemeriksaannya dirilis
    /// (<c>LAB-DEC-067</c>, <c>INV-70</c>).
    /// </summary>
    /// <remarks>
    /// Berbeda dari dokumen hasil di rekam medis (<c>LAB-DC-057</c>, satu per rilis pemeriksaan): dokumen
    /// ini yang diserahkan kepada pasien lewat WhatsApp, cetak, atau unduh. Isinya ada pada
    /// <see cref="LabPatientResultVersion"/>; versi 1 terbit sesudah order <i>Selesai</i>
    /// (<c>02-backend-architecture.md</c> 34.1).
    /// </remarks>
    [Table("LabPatientResultDocument", Schema = "public")]
    public class LabPatientResultDocument : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Order pemilik dokumen — satu dokumen per order.</summary>
        public Guid LabOrderId { get; set; }

        /// <summary>
        /// Disiplin order, disalin saat terbit: hanya Patologi Klinik atau Mikrobiologi
        /// (<c>LAB-DEC-288</c>). Dipakai membaca aktivasi tanpa join ke order.
        /// </summary>
        public LabDiscipline Discipline { get; set; }

        /// <summary>Nomor versi yang berstatus <see cref="LabPatientResultVersionStatus.Current"/>.</summary>
        public int CurrentVersionNumber { get; set; } = 1;

        /// <summary>Token konkurensi — menjaga pergantian versi kelak (koreksi <c>S6</c>).</summary>
        public int Version { get; set; }

        public LabOrder? LabOrder { get; set; }

        public ICollection<LabPatientResultVersion> Versions { get; set; } = new List<LabPatientResultVersion>();

        public ICollection<LabPatientResultDelivery> Deliveries { get; set; } = new List<LabPatientResultDelivery>();
    }
}
