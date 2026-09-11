using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Satu permintaan kebutuhan darah untuk seorang pasien pada satu kunjungan.
    /// Aggregate root <c>BD-AGG-01</c> / <c>BD-DOM-01</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Angka pemenuhan sengaja bukan kolom.</b> Tidak ada <c>FulfilledQuantity</c> di sini,
    /// dan itu keputusan yang mengikat (<c>BD-DOM-17</c>): jumlah yang sudah diberikan dihitung
    /// dari pemberian kantong yang nyata setiap kali ditanya. Menyimpannya sebagai kolom yang
    /// dapat disunting berarti membuka jalan bagi angka pemenuhan yang berbeda dari kantong yang
    /// benar-benar keluar — dan pada modul darah selisih itu bukan kesalahan pembukuan,
    /// melainkan kantong yang tidak diketahui ada di mana.
    /// </para>
    ///
    /// <para>
    /// <b>Deteksi order ganda bukan urusan entity ini.</b> <c>BD-XINV-01</c> membandingkan order
    /// ini dengan order lain milik pasien dan kunjungan yang sama, sehingga hidup di
    /// <c>BbkBloodOrderService</c> — satu aggregate tidak dapat menjaga invariant yang
    /// definisinya melibatkan aggregate lain.
    /// </para>
    ///
    /// <para>
    /// <b>Nol duplikasi hulu.</b> Pasien, kunjungan, unit pelayanan, dan dokter peminta
    /// dirujuk lewat foreign key ke tabel pemiliknya masing-masing dan <b>tidak pernah</b>
    /// disalin ke Bank Darah (<c>BD-DOM-20</c>, <c>BD-CAP-001/002/004/006</c>).
    /// </para>
    ///
    /// <para>
    /// <b><see cref="OrderNumber"/> datang dari provider bersama</b>
    /// (<c>NumberSeriesAllocator</c>, <c>QBE-CODE-006</c>), tidak pernah dari
    /// <c>Count+1</c> atau <c>Max+1</c> (<c>QBE-CODE-002/003</c>). Bank Darah menetapkan awalan
    /// dan formatnya; mesin alokasinya milik Platform (<c>DEC-PLT-005</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Jejak kejadian tidak disalin ke kolom order.</b> Siapa yang membatalkan dan dengan
    /// alasan apa, kapan order ganda dilanjutkan beserta alasan tertulisnya, dan kapan order
    /// kedaluwarsa — seluruhnya hidup di <c>BbkTransitionHistory</c> yang hanya dapat
    /// ditambah (<c>BD-DOM-15</c>). Kamus data kontrak <c>v4</c> memang tidak memberi kolom
    /// untuk itu pada tabel ini, dan menyimpannya dua kali membuka jalan bagi dua catatan
    /// yang berselisih (<c>QBE-ENT-003</c>).
    /// </para>
    /// </remarks>
    [Table("BbkBloodOrder", Schema = "public")]
    public class BbkBloodOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor bisnis order, unik di seluruh tabel. Diterbitkan
        /// <c>NumberSeriesAllocator</c> pada deret <c>BBK_BLOOD_ORDER</c>.
        /// </summary>
        /// <remarks>
        /// Nomor yang sudah terbit <b>tidak pernah dipakai ulang</b>, bahkan ketika order yang
        /// memintanya batal tersimpan: deret boleh berlubang, satu nomor tidak boleh menempel
        /// pada dua catatan (<c>INV-PLT-001</c>, <c>INV-PLT-002</c>).
        /// </remarks>
        [Required]
        [MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>Pasien yang membutuhkan darah.</summary>
        [Required]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public MstPatient? Patient { get; set; }

        /// <summary>
        /// Kunjungan asal order. Menjadi penentu kedaluwarsa lewat
        /// <c>BbkEncounterStatusReader</c> (<c>DEC-BD-014</c>).
        /// </summary>
        [Required]
        public Guid EncounterId { get; set; }

        [ForeignKey(nameof(EncounterId))]
        public TrxPatientEncounter? Encounter { get; set; }

        /// <summary>
        /// Unit pelayanan pemesan. Wajib berpenanda
        /// <c>IsAvailableForBloodOrder = true</c> (<c>VAL-BD-013</c>, <c>BD-DOM-18</c>).
        /// </summary>
        [Required]
        public Guid ServiceUnitId { get; set; }

        [ForeignKey(nameof(ServiceUnitId))]
        public MstServiceUnit? ServiceUnit { get; set; }

        /// <summary>
        /// Dokter peminta. Dialah pemegang wewenang pembatalan berkategori klinis
        /// (<c>DEC-BD-044</c>).
        /// </summary>
        [Required]
        public Guid RequestingDoctorId { get; set; }

        [ForeignKey(nameof(RequestingDoctorId))]
        public MstDoctor? RequestingDoctor { get; set; }

        public BbkOrderSource OrderSource { get; set; } = BbkOrderSource.Electronic;

        /// <summary>
        /// Petugas yang menginput order. <b>Wajib terisi</b> ketika
        /// <see cref="OrderSource"/> bernilai <c>Manual</c> (<c>VAL-BD-010</c>).
        /// </summary>
        /// <remarks>
        /// Untuk order elektronik pun nilainya tetap diisi dari pengguna terautentikasi, karena
        /// <c>VAL-BD-011</c> menuntut setiap order menyimpan siapa yang membuatnya — tanpa
        /// kecuali. Nullable di sini mengikuti kamus data, bukan izin untuk mengosongkannya.
        /// </remarks>
        public Guid? InputByUserId { get; set; }

        public BbkBloodOrderStatus OrderStatus { get; set; } = BbkBloodOrderStatus.Active;

        /// <summary>
        /// Token pencegah tulis-bersamaan. Dipetakan sebagai concurrency token, sehingga dua
        /// pembatalan serentak tidak sama-sama tersimpan.
        /// </summary>
        public int Version { get; set; }

        public ICollection<BbkBloodOrderLine> Lines { get; set; } = new List<BbkBloodOrderLine>();
    }
}
