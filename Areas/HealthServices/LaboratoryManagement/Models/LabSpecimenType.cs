using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Jenis bahan yang dibawa sebuah wadah sampel — darah, urin, cairan tubuh, dahak, nanah,
    /// jaringan, dan seterusnya (<c>LAB-DEC-040</c>, <c>BR-35</c>).
    ///
    /// <b>Kenapa ini data induk, bukan teks bebas.</b> Sebelumnya jenis sampel hanya tersimpan
    /// pada <see cref="LabSpecimen.SpecimenDescription"/> yang berupa teks bebas. Sebagai teks
    /// bebas, "cairan kista", "Cairan Kista", dan "c. kista" terhitung tiga jenis berbeda, dan
    /// laporan jenis sampel tidak pernah dapat dipercaya.
    ///
    /// <b>Kenapa prefix <c>Lab</c>, bukan <c>Mst</c>.</b> Baris riwayat
    /// <c>MODULE_OWNERSHIP_PREFIX_REGISTRY.md</c> tertanggal 2026-09-02 menetapkan data induk
    /// milik Laboratorium memakai prefix <c>Lab</c> — sebagaimana <see cref="LabValueBound"/>
    /// dan <see cref="LabValueOption"/>. <see cref="MstLabRejectionReason"/> yang sudah ada
    /// diperlakukan legacy dan tidak dinamai ulang.
    ///
    /// <b>Yang tidak diurus tabel ini.</b> Volume minimal per jenis pemeriksaan tetap berada di
    /// Rilis 2 sebagai bagian katalog pemeriksaan mandiri (<c>LAB-DEC-001</c>). Tabel ini
    /// menjawab "wadah yang datang berisi apa", bukan "pemeriksaan ini butuh sampel apa".
    /// </summary>
    public class LabSpecimenType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode jenis. Dinormalkan menjadi huruf kapital dan unik di antara baris yang belum
        /// ditandai terhapus (<c>VAL-61</c>).
        /// </summary>
        [Required]
        public string SpecimenTypeCode { get; set; } = string.Empty;

        /// <summary>Nama yang dilihat petugas saat memilih jenis wadah.</summary>
        [Required]
        public string SpecimenTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Penanda baris <c>Lainnya</c> — jalan keluar ketika jenis sampel yang datang belum
        /// terdaftar. Wadah berjenis ini wajib disertai keterangan (<c>VAL-53</c>).
        ///
        /// <b>Kenapa berupa kolom, bukan kode tetap di dalam service.</b> Kode literal membuat
        /// perilaku wajib-berketerangan bergantung pada ejaan sebuah string; satu baris yang
        /// kodenya <c>OTH</c> alih-alih <c>OTHER</c> akan diam-diam melewati validasi. Penanda
        /// kolom membuat aturannya melekat pada datanya sendiri.
        ///
        /// Hanya satu baris aktif yang boleh bernilai benar (<c>VAL-62</c>), dan baris itu
        /// tidak boleh dinonaktifkan selama ia satu-satunya (<c>VAL-63</c>).
        /// </summary>
        public bool IsOtherBucket { get; set; }

        /// <summary>Urutan tampil pada daftar pilihan petugas.</summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Jenis yang tidak lagi dipakai <b>dinonaktifkan</b>, bukan dihapus — wadah lama yang
        /// menunjuk ke sini harus tetap dapat dibaca.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Keterangan bagi petugas, misalnya contoh bahan yang termasuk jenis ini.</summary>
        public string? Description { get; set; }
    }
}
