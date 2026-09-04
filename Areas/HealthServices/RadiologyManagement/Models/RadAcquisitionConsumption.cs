using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Bahan yang benar-benar terpakai pada sebuah acquisition.
    ///
    /// Baris ini adalah **fakta konsumsi**, bukan nominal. Tidak ada satu pun kolom harga di
    /// sini, dan itu disengaja: <c>RJ-BIL-GATE-DEC-004</c> menyatakan Radiologi menyerahkan
    /// fakta konsumsi sedangkan Billing tetap pemilik kanonik perhitungan biaya.
    ///
    /// Gunanya paling terasa pada acquisition yang dihentikan di tengah jalan. Kontras yang
    /// sudah disuntikkan tetap terpakai walau citranya gagal, dan menagih nol untuk itu sama
    /// salahnya dengan menagih penuh. Yang menentukan adalah apa yang tercatat di sini.
    /// </summary>
    public class RadAcquisitionConsumption : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RadStudyId { get; set; }

        public RadConsumptionItemType ItemType { get; set; }

        [Required]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Apakah bahan ini tetap terpakai walaupun acquisition-nya gagal atau dihentikan.
        /// Inilah pembeda antara "gagal, tidak ada yang terpakai" dan "gagal, tetapi kontrasnya
        /// sudah masuk".
        /// </summary>
        public bool ConsumedDespiteFailure { get; set; }

        public Guid? RecordedByUserId { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }

        public RadStudy? RadStudy { get; set; }
    }
}
