using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models
{
    /// <summary>
    /// Pencacah satu deret nomor bisnis pada satu periode. Satu baris menyimpan nilai terakhir
    /// yang <b>sudah terbit</b> untuk pasangan <see cref="SequenceKey"/> dan
    /// <see cref="ScopeKey"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Baris di sini lahir sendiri.</b> Tidak ada layar yang membuatnya dan tidak ada seeder
    /// yang mengisinya; ia muncul saat sebuah deret dipakai pertama kali. Menyemai baris dengan
    /// nilai nol justru melanggar check constraint <c>CurrentValue &gt; 0</c>, dan menyemainya
    /// dengan nilai tebakan berisiko menerbitkan nomor yang sudah menempel pada catatan lain.
    /// </para>
    ///
    /// <para>
    /// <b>Pencacah hanya bergerak naik.</b> Tidak ada jalur kode yang menurunkannya, menyetel
    /// ulangnya, atau menghapus barisnya — dan ketiadaan itu disengaja, bukan belum sempat dibuat.
    /// Sekali sebuah nomor terbit, ia menjadi milik satu catatan selamanya (<c>INV-PLT-001</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Deret yang berlubang adalah keadaan sah.</b> Ketika pekerjaan bisnis yang meminta nomor
    /// dibatalkan, nomornya <b>hangus</b> dan pencacah tetap di posisi barunya (<c>DEC-PLT-008</c>).
    /// Lubang yang terbentuk <b>tidak boleh</b> dirapikan dengan mengisi celahnya
    /// (<c>INV-PLT-002</c>) — nomor pada celah itu justru yang paling mungkin sudah sempat
    /// terlihat, tercatat di kertas, atau disebutkan lewat telepon.
    /// </para>
    ///
    /// <para>
    /// <b>Yang menjaga entity ini bukan kolom versi.</b> Urutan pengambilan nomor diserialkan
    /// <c>pg_advisory_xact_lock</c> di dalam transaksi alokasi, dan index unik
    /// <c>(SequenceKey, ScopeKey)</c> menjadi penjaga terakhirnya di database. Token versi akan
    /// memulangkan kegagalan lalu menuntut percobaan ulang; kunci penasihat justru mengantre dan
    /// menyelesaikannya.
    /// </para>
    ///
    /// <para>
    /// <b>Bentuknya sengaja sama dengan <c>BilNumberSeries</c>.</b> Empat deret Billing yang sudah
    /// produksi <b>tidak</b> dipindahkan pada slice ini (<c>DEC-PLT-003</c>, <c>INV-PLT-003</c>);
    /// menjaga kolomnya identik membuat pemindahan di <c>PLT-SLICE-02</c> menjadi penyalinan baris,
    /// bukan penerjemahan bentuk.
    /// </para>
    /// </remarks>
    [Table("NumNumberSeries", Schema = "public")]
    public class NumNumberSeries : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Penanda deret, ditetapkan modul pemiliknya. Contoh <c>BBK_BLOOD_ORDER</c>.
        /// </summary>
        /// <remarks>
        /// Satu penanda dimiliki tepat satu modul (<c>DEC-PLT-005</c>). Dua modul yang memakai
        /// penanda sama akan berbagi satu pencacah, sehingga nomor keduanya saling menyela.
        /// </remarks>
        [Required]
        [MaxLength(50)]
        public string SequenceKey { get; set; } = string.Empty;

        /// <summary>
        /// Periode berlakunya pencacah. <c>GLOBAL</c> bila deret tidak pernah diulang, atau
        /// <c>2026</c>, <c>202609</c>, <c>20260909</c> bila diulang.
        /// </summary>
        /// <remarks>
        /// Untuk deret baru nilainya selalu <c>GLOBAL</c>, karena <c>DEC-PLT-004</c> menetapkan
        /// deret berjalan terus. Nilai periode lain hanya muncul pada deret lama yang kelak
        /// dipindahkan.
        /// </remarks>
        [Required]
        [MaxLength(50)]
        public string ScopeKey { get; set; } = string.Empty;

        /// <summary>
        /// Kebijakan pengulangan deret. Salah satu dari <c>NEVER</c>, <c>YEARLY</c>,
        /// <c>MONTHLY</c>, atau <c>DAILY</c>; dijaga check constraint database.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ResetPolicy { get; set; } = string.Empty;

        /// <summary>
        /// Nilai terakhir yang <b>sudah terbit</b> — bukan nomor berikutnya.
        /// </summary>
        /// <remarks>
        /// Perbedaan satu langkah ini menyesatkan bila labelnya kabur, sehingga layar pemantauan
        /// wajib menyebutkannya. Dijaga check constraint <c>&gt; 0</c>: nilai nol berarti deret
        /// belum pernah dipakai, dan barisnya semestinya belum ada.
        /// </remarks>
        public long CurrentValue { get; set; }

        /// <summary>Waktu alokasi terakhir pada deret dan periode ini.</summary>
        public DateTimeOffset LastAllocatedAt { get; set; }
    }
}
