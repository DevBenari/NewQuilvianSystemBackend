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

        /// <summary>
        /// <c>Normal</c>, <c>Low</c>, <c>High</c>, atau <c>OutOfReference</c>
        /// (<c>r33</c> 28.3). Kosong bila batasnya tidak menyatakan rentang normal.
        /// <see cref="IsOutOfNormalRange"/> tetap dikirim demi konsumen lama.
        /// </summary>
        public string? ReferenceFlag { get; set; }
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

        // Sembilan ruas di bawah ditambahkan r33 28.3.

        /// <summary><c>Routine</c> atau <c>Cito</c>, supaya halaman dapat mendahulukan pemeriksaan cito.</summary>
        public string Urgency { get; set; } = string.Empty;

        /// <summary>
        /// <c>Normal</c>, <c>Low</c>, <c>High</c>, atau <c>OutOfReference</c>, dihitung dari batas
        /// nilai yang berlaku <b>saat hasil disimpan</b>. Kosong bila hasil belum diisi atau
        /// batasnya tidak menyatakan rentang normal. Nilai kritis tidak termasuk — itu <c>S5</c>.
        /// </summary>
        public string? ReferenceFlag { get; set; }

        public bool IsFinalized { get; set; }

        public DateTime? FinalizedAt { get; set; }

        public Guid? FinalizedByUserId { get; set; }

        public int ReopenCount { get; set; }

        public bool IsConsulted { get; set; }

        public string? ConsultedToName { get; set; }

        public DateTime? ConsultedAt { get; set; }

        // Ruas di bawah ditambahkan r34 29.3 (BE-LAB-76) — sama dengan respons kelengkapan.

        /// <summary>
        /// Pengisi hasil — <b>hanya pada respons ini</b>, supaya layar dapat memperingatkan pengisi
        /// <b>sebelum</b> ia menekan Validasi atas hasilnya sendiri (<c>VAL-129</c>).
        /// </summary>
        public Guid? ResultEnteredByUserId { get; set; }

        /// <summary><c>NotEntered</c>, <c>Draft</c>, <c>Final</c>, <c>Validated</c>, <c>Released</c> — turunan (<c>LAB-DEC-080</c>).</summary>
        public string ResultStatus { get; set; } = string.Empty;

        public bool IsValidated { get; set; }

        public DateTime? ValidatedAt { get; set; }

        public Guid? ValidatedByUserId { get; set; }

        /// <summary>Nama pemvalidasi — baris <i>Validasi oleh</i>.</summary>
        public string? ValidatedByName { get; set; }

        /// <summary>Jabatan pemvalidasi saat itu — snapshot.</summary>
        public string? ValidatedByPositionName { get; set; }

        /// <summary><i>"Divalidasi oleh pengisi sendiri — {nama} — {alasan}"</i>, atau kosong.</summary>
        public string? ValidationExceptionMarker { get; set; }

        public bool IsReleased { get; set; }

        public DateTime? ReleasedAt { get; set; }

        public Guid? ReleasedByUserId { get; set; }

        /// <summary>Nama perilis — baris <i>Otorisasi oleh</i>.</summary>
        public string? ReleasedByName { get; set; }

        /// <summary>Jabatan perilis saat itu — snapshot.</summary>
        public string? ReleasedByPositionName { get; set; }

        /// <summary><i>"Dirilis oleh pemvalidasi sendiri — {nama} — {alasan}"</i>, atau kosong.</summary>
        public string? ReleaseExceptionMarker { get; set; }

        /// <summary>Kosong bila sudah dirilis; selebihnya menyatakan kenapa hasil belum boleh dikirim.</summary>
        public string? DeliveryBlockedReason { get; set; }
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
    /// Permintaan Validasi dan Rilis hasil (<c>LAB-API-v1</c> <c>r34</c> 29.3).
    ///
    /// <b>Pelakunya tidak diterima dari pemanggil</b> — diturunkan dari sesi. Kedua ruas hanya
    /// diisi bila pelaku <b>merangkap</b> peran pada hasil itu: pemvalidasi yang juga pengisi,
    /// atau perilis yang juga pemvalidasi (<c>LAB-DEC-003</c>). Dikirim tanpa merangkap →
    /// ditolak <c>422</c> (<c>VAL-132</c>), supaya penanda pengecualian tidak pernah tercatat
    /// pada hasil yang tidak merangkap.
    /// </summary>
    public class LabResultSignOffRequest
    {
        /// <summary>Alasan <b>aktif</b> pada daftar <c>lab-four-eyes-exception-reasons</c>.</summary>
        public Guid? ExceptionReasonId { get; set; }

        /// <summary>
        /// Catatan bebas, wajib bila alasannya <c>requiresNote</c>, maksimal 500. Disimpan pada
        /// riwayat, <b>tidak</b> tercetak.
        /// </summary>
        public string? ExceptionNote { get; set; }
    }

    /// <summary>
    /// Permintaan <i>Kembalikan ke analis</i> (<c>LAB-API-v1</c> <c>r34</c> 29.3,
    /// <c>LAB-DEC-138</c>). Pelakunya diturunkan dari sesi.
    /// </summary>
    public class LabResultReturnRequest
    {
        /// <summary>
        /// Alasan <b>aktif</b> pada daftar <c>lab-result-correction-reasons</c>. <b>Wajib</b>
        /// (<c>VAL-135</c>) — kodenya menjadi kunci laporan mutu.
        /// </summary>
        public Guid? CorrectionReasonId { get; set; }

        /// <summary>Catatan bebas pada riwayat; wajib bila alasannya <c>requiresNote</c>, maksimal 500.</summary>
        public string? Note { get; set; }
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

        // Ruas di bawah ditambahkan r34 29.3 (BE-LAB-73). Ruas rilis menyusul BE-LAB-74.

        /// <summary>
        /// <c>NotEntered</c>, <c>Draft</c>, <c>Final</c>, <c>Validated</c>, atau <c>Released</c>.
        /// <b>Diturunkan</b> setiap kali dibaca, bukan kolom tersimpan (<c>LAB-DEC-080</c>).
        /// </summary>
        public string ResultStatus { get; set; } = string.Empty;

        /// <summary><c>ValidatedAt != null</c>.</summary>
        public bool IsValidated { get; set; }

        public DateTime? ValidatedAt { get; set; }

        public Guid? ValidatedByUserId { get; set; }

        /// <summary>Nama pemvalidasi — baris <i>Validasi oleh</i>.</summary>
        public string? ValidatedByName { get; set; }

        /// <summary>Jabatan pemvalidasi <b>saat ia memvalidasi</b> — snapshot, bukan jabatan hari ini.</summary>
        public string? ValidatedByPositionName { get; set; }

        /// <summary>
        /// Bunyi penanda bila pemvalidasi juga pengisi hasil, persis
        /// <c>02-backend-architecture.md</c> 20.10 butir 5:
        /// <i>"Divalidasi oleh pengisi sendiri — {nama} — {alasan}"</i>. Kosong bila tidak merangkap.
        /// </summary>
        public string? ValidationExceptionMarker { get; set; }

        /// <summary>
        /// <c>ReleasedAt != null</c> — hasil sudah menjadi dokumen klinis pasien (<c>BE-LAB-74</c>).
        ///
        /// Bernilai sebenarnya untuk Patologi Klinik. Mikrobiologi tetap salah sampai <c>S4d</c>,
        /// sebab rilisnya belum dibangun. Ruas ini ada supaya layar dan pemanggil <b>tidak perlu
        /// menyimpulkan</b> bahwa Final sama dengan rilis — kesimpulan yang ditolak
        /// <c>LAB-DEC-097</c>.
        /// </summary>
        public bool IsReleased { get; set; }

        public DateTime? ReleasedAt { get; set; }

        public Guid? ReleasedByUserId { get; set; }

        /// <summary>Nama perilis — baris <i>Otorisasi oleh</i> (<c>LAB-DEC-120</c>).</summary>
        public string? ReleasedByName { get; set; }

        /// <summary>Jabatan perilis <b>saat ia merilis</b> — snapshot (20.10 butir 1).</summary>
        public string? ReleasedByPositionName { get; set; }

        /// <summary>
        /// Bunyi penanda bila perilis juga pemvalidasi, persis 20.10 butir 5:
        /// <i>"Dirilis oleh pemvalidasi sendiri — {nama} — {alasan}"</i>. Kosong bila tidak merangkap.
        /// </summary>
        public string? ReleaseExceptionMarker { get; set; }

        /// <summary>
        /// Kenapa hasil ini belum boleh dikirim ke pasien. Kosong ketika sudah boleh.
        /// </summary>
        public string? DeliveryBlockedReason { get; set; }
    }
}
