namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Pengisian hasil satu pemeriksaan (<c>LAB-API-v1</c> <c>r21</c>, slice <c>S4a</c>).
    ///
    /// <b>Empat hal sengaja TIDAK diterima dari pemanggil:</b> satuan, batas nilai yang berlaku,
    /// waktu pengetikan, dan pelakunya. Keempatnya diturunkan server — satuan dan batas dari
    /// <c>LabValueBound</c> yang berlaku bagi pasien itu, waktu dan pelaku dari sesi.
    ///
    /// Menerimanya dari pemanggil berarti mengizinkan sebuah hasil mengaku diperiksa dengan
    /// batas yang tidak pernah berlaku baginya.
    /// </summary>
    public class LabExaminationResultRequest
    {
        /// <summary>Diisi <b>hanya</b> bila bentuk hasilnya <c>Numeric</c>.</summary>
        public decimal? ResultNumeric { get; set; }

        /// <summary>
        /// Diisi <b>hanya</b> bila bentuk hasilnya <c>Choice</c>. Wajib menunjuk
        /// <c>LabValueOption</c> milik batas nilai yang berlaku, bukan pilihan milik pemeriksaan
        /// lain (<c>VAL-81</c>).
        /// </summary>
        public Guid? ResultOptionId { get; set; }

        /// <summary>
        /// <b>Kapan pemeriksaannya dikerjakan</b>, bukan kapan hasilnya diketik. Boleh kosong;
        /// bila kosong diisi waktu sekarang. Tidak boleh di masa depan (<c>VAL-82</c>).
        /// </summary>
        public DateTime? ExaminedAt { get; set; }
    }

    /// <summary>
    /// Keadaan hasil sesudah diisi.
    ///
    /// <b>Nol ruas status di sini</b>, dan itu batas yang paling penting pada <c>r21</c>.
    /// Validasi dan rilis tertahan <c>LAB-SIGN-001</c>; yang dikembalikan adalah <b>apa yang
    /// tercatat</b>, bukan <b>apa yang terjadi berikutnya</b>.
    /// </summary>
    public class LabExaminationResultResponse
    {
        public Guid LabExaminationId { get; set; }

        public Guid LabOrderId { get; set; }

        public string? ProcedureName { get; set; }

        /// <summary><c>Numeric</c> atau <c>Choice</c>, diturunkan dari batas nilai yang berlaku.</summary>
        public string ResultForm { get; set; } = string.Empty;

        public decimal? ResultNumeric { get; set; }

        public Guid? ResultOptionId { get; set; }

        /// <summary>Nama pilihan siap tampil, supaya layar tidak perlu memanggil daftar pilihan lagi.</summary>
        public string? ResultOptionName { get; set; }

        public string? ResultUnitSnapshot { get; set; }

        public Guid? ResultValueBoundId { get; set; }

        public DateTime? ExaminedAt { get; set; }

        public DateTime? ResultEnteredAt { get; set; }

        /// <summary>
        /// Apakah nilainya di luar rentang normal batas yang berlaku.
        ///
        /// <b>Ini keterangan, bukan penandaan nilai kritis.</b> Nilai kritis beserta kewajiban
        /// pelaporannya adalah <c>LAB-DEC-004</c>, yang tertahan <c>LAB-SIGN-001</c> — dan
        /// menandainya di sini akan menjanjikan alur pelaporan yang belum diputuskan.
        /// Kosong berarti batasnya tidak menyatakan rentang normal.
        /// </summary>
        public bool? IsOutOfNormalRange { get; set; }
    }

    /// <summary>
    /// Bentuk hasil yang berlaku bagi satu pemeriksaan, beserta rujukan dan hasil yang sudah
    /// terisi (<c>LAB-API-v1</c> <c>r22</c>).
    ///
    /// <b>Layar tidak dapat menurunkan ini sendiri.</b> Bentuk hasil ditentukan
    /// <c>LabValueBound</c> yang berlaku bagi <b>pasien tertentu</b> — dibedakan jenis kelamin
    /// dan kelompok umur — bukan oleh jenis pemeriksaannya saja. Menebaknya berarti menampilkan
    /// kotak angka untuk pemeriksaan yang hanya menerima pilihan, dan petugas baru mengetahuinya
    /// sesudah <c>422</c> datang.
    /// </summary>
    public class LabExaminationResultFormResponse
    {
        public Guid LabExaminationId { get; set; }

        public string? ProcedureName { get; set; }

        /// <summary><c>Numeric</c> atau <c>Choice</c>. Kosong bila belum ada batas nilai yang berlaku.</summary>
        public string? ResultForm { get; set; }

        /// <summary>
        /// <c>false</c> bila pemeriksaannya sudah gugur atau dibatalkan, atau nol punya batas
        /// nilai yang berlaku. Layar memakainya untuk menonaktifkan isian <b>beserta
        /// alasannya</b>, bukan menyembunyikannya tanpa keterangan.
        /// </summary>
        public bool CanEnterResult { get; set; }

        /// <summary>Alasan ketika <see cref="CanEnterResult"/> bernilai <c>false</c>.</summary>
        public string? BlockedReason { get; set; }

        public string? Unit { get; set; }

        public decimal? NormalLow { get; set; }

        public decimal? NormalHigh { get; set; }

        /// <summary>
        /// Pilihan yang sah — hanya terisi pada bentuk <c>Choice</c>.
        ///
        /// Pengetikan bebas tidak diterima pada bentuk itu (<c>AC-28</c>), sehingga daftar ini
        /// adalah satu-satunya nilai yang dapat dipilih.
        /// </summary>
        public List<LabExaminationResultOptionResponse> Options { get; set; } = new();

        public decimal? ResultNumeric { get; set; }

        public Guid? ResultOptionId { get; set; }

        public DateTime? ExaminedAt { get; set; }

        public DateTime? ResultEnteredAt { get; set; }
    }

    /// <summary>Satu pilihan hasil yang sah.</summary>
    public class LabExaminationResultOptionResponse
    {
        public Guid Id { get; set; }

        public string OptionName { get; set; } = string.Empty;

        /// <summary>
        /// Apakah pilihan ini di luar rentang rujukan.
        ///
        /// <b>Bukan penanda nilai kritis</b> — <c>LabValueOption.IsCritical</c> sengaja tidak
        /// ikut dikirim, karena nilai kritis adalah <c>LAB-DEC-004</c> yang tertahan
        /// <c>LAB-SIGN-001</c>.
        /// </summary>
        public bool IsOutOfReference { get; set; }
    }

    /// <summary>
    /// Membuka kembali penulisan hasil sebelum rilis (<c>LAB-API-v1</c> <c>r26</c>
    /// bagian 21.2, <c>LAB-DEC-097</c>).
    ///
    /// <b>Reopen di sini BUKAN koreksi hasil terrilis.</b> Ia penyuntingan biasa atas hasil
    /// yang belum pernah dirilis, sehingga ia <b>tidak</b> menyentuh <c>S6</c> maupun
    /// <c>DEC-LAB-014</c>.
    /// </summary>
    public class LabReopenRequest
    {
        /// <summary>
        /// Alasan membuka kembali. <b>Wajib</b> (<c>VAL-107</c> jalur sahnya), maksimal 500.
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Mencatat fakta konsultasi — penanda <c>Definitif</c> (<c>LAB-DEC-106</c>).
    ///
    /// <b>Dua hal sengaja TIDAK diterima dari pemanggil:</b> siapa yang mencatat, dan kapan
    /// ia dicatat. Keduanya diturunkan dari sesi. Menerimanya berarti mengizinkan seseorang
    /// mencatat konsultasi atas nama orang lain — alasan yang sama dipakai
    /// <c>LAB-DEC-105</c> menolak ruas <c>Analis</c> yang dapat dipilih.
    /// </summary>
    public class LabConsultationRequest
    {
        /// <summary>
        /// Kepada siapa hasil ini dikonsultasikan. <b>Wajib</b>, maksimal 200.
        ///
        /// Teks, bukan penunjuk pengguna: konsultannya sering berada di luar daftar pengguna
        /// sistem (<c>LAB-EVD-005</c>).
        /// </summary>
        public string? ConsultedToName { get; set; }

        /// <summary>
        /// Kapan konsultasinya terjadi. <b>Wajib</b>, dan <b>tidak boleh berada di masa
        /// depan</b> (<c>VAL-108</c>).
        /// </summary>
        public DateTime? ConsultedAt { get; set; }
    }

    /// <summary>
    /// Keadaan kelengkapan dan konsultasi sebuah hasil Mikrobiologi
    /// (<c>LAB-API-v1</c> <c>r26</c> bagian 21.3).
    ///
    /// <b>Bacalah <see cref="IsFinalized"/> bersama <see cref="IsReleased"/>.</b> Hasil yang
    /// sudah Final <b>belum</b> boleh dikirim ke pasien — pengiriman menunggu rilis, dan rilis
    /// Mikrobiologi adalah <c>S4d</c> yang tertahan <c>DEC-LAB-011</c>.
    /// </summary>
    public class LabExaminationCompletionResponse
    {
        public Guid LabExaminationId { get; set; }

        public Guid LabOrderId { get; set; }

        public string? ProcedureName { get; set; }

        /// <summary><c>FinalizedAt != null</c>. Penulis menyatakan selesai menulis.</summary>
        public bool IsFinalized { get; set; }

        public DateTime? FinalizedAt { get; set; }

        public Guid? FinalizedByUserId { get; set; }

        /// <summary>Berapa kali penulisan dibuka kembali sebelum rilis.</summary>
        public int ReopenCount { get; set; }

        /// <summary><c>ConsultedAt != null</c> — penanda <c>Definitif</c>.</summary>
        public bool IsConsulted { get; set; }

        public string? ConsultedToName { get; set; }

        public DateTime? ConsultedAt { get; set; }

        public Guid? ConsultedByUserId { get; set; }

        /// <summary>
        /// <b>Selalu bernilai salah pada rilis ini</b>, dan itu disengaja.
        ///
        /// Rilis Mikrobiologi adalah <c>S4d</c> dan belum dibangun. Ruas ini ada supaya layar
        /// dan pemanggil <b>tidak perlu menyimpulkan</b> bahwa Final sama dengan rilis —
        /// kesimpulan yang justru ditolak <c>LAB-DEC-097</c>.
        /// </summary>
        public bool IsReleased { get; set; }

        /// <summary>
        /// Kenapa hasil ini belum boleh dikirim ke pasien. Kosong ketika sudah boleh.
        /// </summary>
        public string? DeliveryBlockedReason { get; set; }
    }
}
