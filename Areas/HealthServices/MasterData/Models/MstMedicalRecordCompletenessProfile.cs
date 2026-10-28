using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Satu himpunan dokumen wajib untuk satu kunci (`AGG-RM-04`, `RM-DEC-045`, `RM-DEC-060`, `BE-37`).
    ///
    /// Kunci = jenis kunjungan + jenis unit layanan (kosong = semua) + kondisi bersyarat. Profil
    /// berunit menggantikan profil umum jenis kunjungan yang sama; profil kondisi berlaku di atas
    /// profil jenis kunjungan. Profil TANPA butir sah dan berarti "tidak ada dokumen dokter
    /// wajib" — dipakai Rawat Jalan unit Laboratorium dan Radiologi.
    ///
    /// Sengaja tanpa kolom urutan tampilan: QBE melarang <c>SortOrder</c> presentasi generik
    /// yang dipersistensi pada kode baru; daftar diurutkan menurut kode profil.
    /// </summary>
    [Table("MstMedicalRecordCompletenessProfile", Schema = "public")]
    public class MstMedicalRecordCompletenessProfile : IdentityModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Wajib bila <see cref="Condition"/> = <c>None</c>.</summary>
        public EncounterType? EncounterType { get; set; }

        /// <summary>Kosong = semua jenis unit.</summary>
        public ServiceUnitType? ServiceUnitType { get; set; }

        public CompletenessCondition Condition { get; set; } = CompletenessCondition.None;

        public bool IsActive { get; set; } = true;

        public ICollection<MstMedicalRecordCompletenessProfileItem> Items { get; set; } = new List<MstMedicalRecordCompletenessProfileItem>();
    }
}
