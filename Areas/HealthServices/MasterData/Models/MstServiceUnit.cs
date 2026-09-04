using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstServiceUnit", Schema = "public")]
    public class MstServiceUnit : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ServiceUnitCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ServiceUnitName { get; set; } = string.Empty;

        public ServiceUnitType ServiceUnitType { get; set; } = ServiceUnitType.Unknown;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        public bool IsAvailableForRegistration { get; set; } = true;

        public bool IsAvailableForKiosk { get; set; } = false;

        public bool IsAvailableForAppointment { get; set; } = false;

        /// <summary>
        /// Menandai unit pelayanan ini berwenang membuat order darah ke Bank Darah.
        /// </summary>
        /// <remarks>
        /// Bawaannya <b>menolak</b> (<c>DEC-BD-012</c>). Kewenangan memesan darah adalah sifat
        /// konfigurasi, bukan daftar unit yang dikunci di dalam kode: unit baru diberi
        /// kewenangan lewat layar master unit pelayanan, tanpa satu baris kode pun berubah.
        ///
        /// <b>Contoh.</b> Rawat Inap, IGD, dan Rawat Jalan diberi nilai <c>true</c> saat modul
        /// Bank Darah dinyalakan. Ketika kelak Kamar Operasi perlu memesan darah sendiri,
        /// admin cukup menyalakan penanda ini pada unit tersebut.
        ///
        /// Kolom ini <b>dititipkan</b> Bank Darah pada tabel milik Master Data. Pengelolaannya
        /// tetap lewat kontrak unit pelayanan milik Master Data, bukan lewat endpoint Bank
        /// Darah.
        /// </remarks>
        public bool IsAvailableForBloodOrder { get; set; } = false;

        public bool IsQueueRequired { get; set; } = true;

        public bool IsDoctorRequired { get; set; } = false;

        public bool IsScreeningRequired { get; set; } = false;

        public Guid? OrganizationUnitId { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
