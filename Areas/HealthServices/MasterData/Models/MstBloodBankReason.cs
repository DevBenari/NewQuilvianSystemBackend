using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Daftar alasan terkendali Bank Darah — pilihan yang wajib dipakai petugas ketika
    /// membatalkan order, mengalihkan kantong, menetapkan kantong tidak layak, menempuh jalur
    /// darurat, dan tindakan berjejak lain.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa alasan tidak boleh diketik bebas.</b> <c>INV-BD-016</c> menetapkan alasan pada
    /// pembatalan, pengalihan, penetapan tidak layak, dan jalur darurat tidak boleh berupa teks
    /// bebas semata. Kotak teks bebas menghasilkan jawaban yang tidak dapat dikelompokkan,
    /// sehingga tinjauan berubah menjadi membaca ratusan kalimat satu per satu. Dengan daftar
    /// terkendali, alasan menjadi dapat dihitung dan dibandingkan.
    ///
    /// <b>Kategori menentukan lebih dari sekadar pengelompokan.</b> Pada pembatalan order,
    /// kategori alasan adalah satu-satunya yang membedakan pembatalan klinis dari pembatalan
    /// operasional — keduanya memakai butir hak akses yang sama, yaitu
    /// <c>BloodOrder : Cancel</c> (<c>DEC-BD-044</c>). Tanpa pemisahan kategori, peninjau tidak
    /// dapat membedakan order yang dicabut karena pasiennya tidak jadi ditransfusi dari order
    /// yang dihapus karena salah input.
    ///
    /// <b>Teks alasan disalin saat dipakai.</b> Menonaktifkan sebuah alasan tidak mengubah makna
    /// riwayat lama, karena rekam tindakan menyimpan salinan teksnya sendiri.
    /// </remarks>
    [Table("MstBloodBankReason", Schema = "public")]
    public class MstBloodBankReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode alasan, unik di seluruh tabel. Contoh <c>CANCEL-KLINIS-01</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Teks yang ditampilkan kepada petugas saat memilih alasan.</summary>
        [Required]
        [MaxLength(200)]
        public string ReasonText { get; set; } = string.Empty;

        /// <summary>
        /// Kategori alasan. Nilainya berasal dari daftar tertutup
        /// <see cref="BloodBankReasonCategories"/>.
        /// </summary>
        /// <remarks>
        /// Disimpan sebagai teks, bukan angka, mengikuti kamus data kontrak <c>v4</c> yang
        /// menetapkannya <c>string(40)</c> ber-index. Pilihan itu disengaja: kategori dibaca
        /// langsung pada laporan dan penyaring tanpa perlu tabel penerjemah, dan penambahan
        /// kategori baru tidak menuntut migration.
        ///
        /// Konsekuensinya, <b>daftar tertutupnya dijaga di service</b>, bukan oleh tipe kolom.
        /// Lihat catatan pada laporan task <c>BE-BD-001</c>.
        /// </remarks>
        [Required]
        [MaxLength(40)]
        public string ReasonCategory { get; set; } = string.Empty;

        /// <summary>
        /// Alasan nonaktif tidak lagi muncul sebagai pilihan, tetapi <b>tidak</b> mengubah makna
        /// riwayat lama yang menyebutnya.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Daftar tertutup kategori alasan Bank Darah, sesuai kamus data kontrak <c>v4</c>.
    /// </summary>
    /// <remarks>
    /// Ditulis sebagai konstanta, bukan enum yang dipersistensi, karena kolomnya memang
    /// <c>string(40)</c>. Kelas ini yang membuat modul tetap memiliki daftarnya sendiri:
    /// nilai di luar daftar ini ditolak service, sehingga salah ketik tidak diam-diam
    /// menciptakan kategori baru yang tak pernah dibaca siapa pun.
    /// </remarks>
    public static class BloodBankReasonCategories
    {
        /// <summary>Pembatalan order karena kebutuhan klinis berubah (`DEC-BD-044`).</summary>
        public const string OrderCancellationClinical = "OrderCancellationClinical";

        /// <summary>Pembatalan order karena kekeliruan operasional (`DEC-BD-044`).</summary>
        public const string OrderCancellationOperational = "OrderCancellationOperational";

        /// <summary>Pemberian lewat jalur darurat (`DEC-BD-017`).</summary>
        public const string Emergency = "Emergency";

        /// <summary>Penyelesaian kantong yang menunggu keputusan (`DEC-BD-019`).</summary>
        public const string PendingReviewResolution = "PendingReviewResolution";

        /// <summary>Pengembalian kantong ke PMI.</summary>
        public const string Return = "Return";

        /// <summary>Penetapan kantong tidak layak (`DEC-BD-043`, `DEC-BD-045`).</summary>
        public const string NotUsable = "NotUsable";

        /// <summary>Kiriman melebihi jumlah yang diminta (`DEC-BD-025`).</summary>
        public const string OverDelivery = "OverDelivery";

        /// <summary>Pembatalan alokasi kantong (`DEC-BD-029`).</summary>
        public const string AllocationCancellation = "AllocationCancellation";

        /// <summary>Koreksi pencatatan pemberian (`DEC-BD-030`).</summary>
        public const string IssuanceCorrection = "IssuanceCorrection";

        /// <summary>Penolakan permintaan koreksi (`DEC-BD-041`).</summary>
        public const string CorrectionRejection = "CorrectionRejection";

        /// <summary>Kesepuluh kategori yang sah, dalam urutan kontrak.</summary>
        public static readonly string[] All =
        {
            OrderCancellationClinical,
            OrderCancellationOperational,
            Emergency,
            PendingReviewResolution,
            Return,
            NotUsable,
            OverDelivery,
            AllocationCancellation,
            IssuanceCorrection,
            CorrectionRejection
        };

        /// <summary>
        /// Mencocokkan teks kategori tanpa membedakan huruf besar-kecil, lalu memulangkan
        /// bentuk bakunya. Memulangkan <c>null</c> bila kategorinya tidak dikenal.
        /// </summary>
        public static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var keyword = value.Trim();

            return All.FirstOrDefault(x => string.Equals(x, keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}
