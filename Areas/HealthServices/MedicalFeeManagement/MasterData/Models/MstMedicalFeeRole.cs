using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models
{
    /// <summary>
    /// Daftar peran pelaksana yang berlaku untuk perhitungan jasa medis di seluruh sumber layanan,
    /// contoh <c>OPERATOR</c> atau <c>ANESTHESIA</c>, sehingga "operator" berarti sama di kamar
    /// operasi dan di tindakan poli. Peran yang sudah dipakai baris tarif dinonaktifkan
    /// (<see cref="IsActive"/>), tidak dihapus — MDF-DES-011, data-dictionary.md bagian 1.
    /// </summary>
    [Table("MstMedicalFeeRole", Schema = "public")]
    public class MstMedicalFeeRole : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string RoleCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Nama nilai <c>OprTeamRole</c> milik Operating Room yang dipetakan, disimpan sebagai teks.
        /// Kosong berarti peran ini tidak punya padanan di kamar operasi. Satu nilai dipetakan paling
        /// banyak ke satu peran.
        /// </summary>
        [MaxLength(30)]
        public string? OprTeamRoleMapping { get; set; }

        /// <summary>Peran utama, dipakai saat sumber layanan hanya menyimpan satu pelaksana.</summary>
        public bool IsPrimaryRole { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
