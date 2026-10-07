using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Master <b>jenis</b> alat medis besar beserta satuan tagih dan pembulatannya —
    /// <c>keperawatan</c> kontrak <c>0.6.0</c> kamus data 12.13 (<c>BE-RWI-172</c>, migration <c>K8</c>).
    /// </summary>
    /// <remarks>
    /// Bukan unit aset bernomor seri (<c>RWI-DEC-179</c> butir 1). Contoh: "Ventilator" ditagih per
    /// hari, "Syringe Pump" per hari. Tarif per kelas memakai <c>MstTariff.MedicalEquipmentId</c>;
    /// tidak ada tabel tarif kedua (<c>PR-RWF-04</c>). Layar dan endpoint CRUD-nya milik task
    /// <c>keperawatan</c>, bukan task ini.
    /// </remarks>
    [Table("MstMedicalEquipment", Schema = "public")]
    public class MstMedicalEquipment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string EquipmentCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EquipmentName { get; set; } = string.Empty;

        /// <summary>Teks bebas; taksonomi kategori alat belum ditetapkan.</summary>
        [MaxLength(100)]
        public string? CategoryName { get; set; }

        public MstEquipmentChargeUnit ChargeUnit { get; set; }

        public MstEquipmentRoundingRule RoundingRule { get; set; } = MstEquipmentRoundingRule.CeilingWholeUnit;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Konkurensi optimistis; berganti setiap kali baris diubah.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();
    }
}
