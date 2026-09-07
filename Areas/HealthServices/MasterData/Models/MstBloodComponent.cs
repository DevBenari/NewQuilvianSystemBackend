using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Katalog komponen darah yang dilayani Bank Darah — Packed Red Cells, Trombosit
    /// Concentrate, Fresh Frozen Plasma, dan seterusnya.
    ///
    /// Katalog ini bukan daftar pilihan biasa. Ia dipakai dua tempat yang menentukan:
    /// pendeteksian order darah ganda membandingkan pasien, kunjungan, dan komponen sekaligus
    /// (<c>DEC-BD-005</c>), dan gerbang pemberian darah menghitung masa berlaku bukti
    /// kecocokan dari <see cref="CompatibilityEvidenceValidityHours"/> milik komponennya
    /// (<c>DEC-BD-032</c>).
    ///
    /// Letak berkas mengikuti aturan struktur: master tinggal di MasterData/Models, bukan di
    /// folder submodul Bank Darah. Prefix <c>Mst</c> berasal dari registry kepemilikan modul,
    /// bukan disimpulkan dari nama folder (<c>QBE-NAM-004</c>).
    /// </summary>
    [Table("MstBloodComponent", Schema = "public")]
    public class MstBloodComponent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode komponen yang dikenali petugas, unik di seluruh tabel. Contoh <c>PRC</c>,
        /// <c>TC</c>, <c>FFP</c>.
        /// </summary>
        /// <remarks>
        /// Kode ini <b>ditulis pengguna</b>, bukan dialokasikan sistem. Ia identifier domain
        /// yang sudah dipakai BDRS sehari-hari, bukan nomor urut dokumen — sama seperti
        /// <c>PurposeCode</c> pada master keperluan akses rekam medis. Karena itu tidak ada
        /// number-series yang terlibat, dan aturan alokasi kode bisnis tidak berlaku di sini.
        /// Yang menjaganya tetap tunggal adalah index unik pada kolom ini.
        /// </remarks>
        [Required]
        [MaxLength(20)]
        public string ComponentCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ComponentName { get; set; } = string.Empty;

        /// <summary>
        /// Berapa jam bukti pemeriksaan kecocokan masih berlaku untuk komponen ini.
        /// </summary>
        /// <remarks>
        /// Boleh kosong, dan kekosongannya <b>bukan</b> nilai bawaan yang longgar melainkan
        /// gerbang yang tertutup: selama belum diisi, pemberian darah komponen ini ditolak
        /// dengan alasan masa berlakunya belum ditetapkan (<c>VAL-BD-020b</c>). Ini disengaja
        /// — menebak angka jam untuk darah lebih berbahaya daripada menahan prosesnya.
        ///
        /// <b>Contoh.</b> PRC dikonfigurasi 72 jam dan TC 24 jam. Bukti kecocokan PRC yang
        /// dicatat Senin pukul 08.00 masih membuka gerbang sampai Kamis pukul 08.00,
        /// sedangkan bukti TC pada jam yang sama berhenti berlaku Selasa pukul 08.00.
        ///
        /// Nilainya <b>selalu</b> dibaca dari kolom ini saat gerbang dinilai. Menanamnya di
        /// kode controller atau frontend melanggar <c>INV-BD-023</c>.
        /// </remarks>
        public int? CompatibilityEvidenceValidityHours { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
