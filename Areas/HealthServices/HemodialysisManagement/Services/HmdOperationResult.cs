namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Jenis hasil satu tindakan Hemodialisa, dipetakan satu-satu ke kode HTTP kontrak
    /// <c>HMD-CONTRACT-v1</c>.
    /// </summary>
    /// <remarks>
    /// <see cref="BusinessRule"/> (<c>422</c>) dan <see cref="Locked"/> (<c>423</c>) sengaja
    /// dipisah dari <see cref="Validation"/> (<c>400</c>). Yang pertama berarti "isiannya benar,
    /// tetapi syaratnya belum terpenuhi"; yang kedua berarti "catatan sudah disahkan — pakai
    /// addendum"; yang ketiga berarti "isiannya salah". Ketiganya menuntut tindakan berbeda dari
    /// petugas.
    /// </remarks>
    public enum HmdResultKind
    {
        Success = 1,
        Created = 2,
        Validation = 3,
        Forbidden = 4,
        NotFound = 5,
        Conflict = 6,
        BusinessRule = 7,
        Locked = 8
    }

    /// <summary>Hasil satu tindakan beserta kode aturan dan pesannya.</summary>
    public sealed class HmdResult<T>
    {
        private HmdResult(HmdResultKind kind)
        {
            Kind = kind;
        }

        public HmdResultKind Kind { get; private init; }

        public T? Value { get; private init; }

        /// <summary>Kode aturan <c>validation-matrix.md</c>, misalnya <c>HMD-VAL-032</c>.</summary>
        public string? Code { get; private init; }

        public string? Message { get; private init; }

        /// <summary>Rincian tambahan untuk layar, misalnya butir yang belum terpenuhi. Tidak pernah memuat data sensitif.</summary>
        public object? Details { get; private init; }

        public bool IsSuccess => Kind is HmdResultKind.Success or HmdResultKind.Created;

        public static HmdResult<T> Ok(T value) => new(HmdResultKind.Success) { Value = value };

        public static HmdResult<T> Created(T value) => new(HmdResultKind.Created) { Value = value };

        public static HmdResult<T> Invalid(string code, string message, object? details = null) =>
            new(HmdResultKind.Validation) { Code = code, Message = message, Details = details };

        public static HmdResult<T> Forbidden(string code, string message) =>
            new(HmdResultKind.Forbidden) { Code = code, Message = message };

        public static HmdResult<T> NotFound(string message) =>
            new(HmdResultKind.NotFound) { Code = HmdErrorCodes.NotFound, Message = message };

        public static HmdResult<T> Conflict(string code, string message, object? details = null) =>
            new(HmdResultKind.Conflict) { Code = code, Message = message, Details = details };

        public static HmdResult<T> Rule(string code, string message, object? details = null) =>
            new(HmdResultKind.BusinessRule) { Code = code, Message = message, Details = details };

        public static HmdResult<T> Locked(string code, string message) =>
            new(HmdResultKind.Locked) { Code = code, Message = message };

        /// <summary>Meneruskan kegagalan dari hasil bertipe lain tanpa mengubah jenis, kode, dan pesannya.</summary>
        public static HmdResult<T> From<TOther>(HmdResult<TOther> failure) =>
            new(failure.Kind) { Code = failure.Code, Message = failure.Message, Details = failure.Details };
    }

    /// <summary>
    /// Kode dan kalimat penolakan modul Hemodialisa.
    /// </summary>
    /// <remarks>
    /// <c>contracts/validation-matrix.md</c> adalah <b>satu-satunya</b> tempat kalimat pesan hidup.
    /// Kalimat di bawah disalin apa adanya dari sana. Kode tanpa nomor matriks — misalnya
    /// <see cref="InvalidTransition"/> — adalah pelengkap teknis untuk keadaan yang matriksnya
    /// tidak menyebut kalimat khusus, dan dicatat sebagai delta pada laporan task.
    /// </remarks>
    public static class HmdErrorCodes
    {
        public const string NotFound = "HMD-NOT-FOUND";
        public const string InvalidTransition = "HMD-INVALID-TRANSITION";
        public const string InvalidRequest = "HMD-INVALID-REQUEST";
        public const string ConcurrencyConflict = "HMD-VAL-902";
        public const string ActorNotDoctor = "HMD-ACTOR-NOT-DOCTOR";
        public const string SettingMissing = "HMD-SETTING-MISSING";
        public const string NumberAllocationFailed = "HMD-NUMBER-ALLOCATION-FAILED";

        public const string Val001 = "HMD-VAL-001";
        public const string Val002 = "HMD-VAL-002";
        public const string Val003 = "HMD-VAL-003";
        public const string Val004 = "HMD-VAL-004";
        public const string Val005 = "HMD-VAL-005";
        public const string Val006 = "HMD-VAL-006";
        public const string Val010 = "HMD-VAL-010";
        public const string Val011 = "HMD-VAL-011";
        public const string Val012 = "HMD-VAL-012";
        public const string Val013 = "HMD-VAL-013";
        public const string Val020 = "HMD-VAL-020";
        public const string Val021 = "HMD-VAL-021";
        public const string Val022 = "HMD-VAL-022";
        public const string Val023 = "HMD-VAL-023";
        public const string Val024 = "HMD-VAL-024";
        public const string Val030 = "HMD-VAL-030";
        public const string Val031 = "HMD-VAL-031";
        public const string Val032 = "HMD-VAL-032";
        public const string Val033 = "HMD-VAL-033";
        public const string Val034 = "HMD-VAL-034";
        public const string Val035 = "HMD-VAL-035";
        public const string Val036 = "HMD-VAL-036";
        public const string Val037 = "HMD-VAL-037";
        public const string Val038 = "HMD-VAL-038";
        public const string Val040 = "HMD-VAL-040";
        public const string Val041 = "HMD-VAL-041";
        public const string Val042 = "HMD-VAL-042";
        public const string Val043 = "HMD-VAL-043";
        public const string Val044 = "HMD-VAL-044";
        public const string Val045 = "HMD-VAL-045";
        public const string Val046 = "HMD-VAL-046";
        public const string Val050 = "HMD-VAL-050";
        public const string Val051 = "HMD-VAL-051";
        public const string Val052 = "HMD-VAL-052";
        public const string Val053 = "HMD-VAL-053";
        public const string Val054 = "HMD-VAL-054";
        public const string Val055 = "HMD-VAL-055";
        public const string Val056 = "HMD-VAL-056";
        public const string Val057 = "HMD-VAL-057";
        public const string Val060 = "HMD-VAL-060";
        public const string Val061 = "HMD-VAL-061";
        public const string Val070 = "HMD-VAL-070";
        public const string Val071 = "HMD-VAL-071";
        public const string Val072 = "HMD-VAL-072";
        public const string Val073 = "HMD-VAL-073";
        public const string Val074 = "HMD-VAL-074";
        public const string Val075 = "HMD-VAL-075";
        public const string Val080 = "HMD-VAL-080";
        public const string Val090 = "HMD-VAL-090";
        public const string Val091 = "HMD-VAL-091";
        public const string Val092 = "HMD-VAL-092";
        public const string Val100 = "HMD-VAL-100";
        public const string Val101 = "HMD-VAL-101";
        public const string Val102 = "HMD-VAL-102";
        public const string Val103 = "HMD-VAL-103";
        public const string Val900 = "HMD-VAL-900";
    }

    /// <summary>Kalimat penolakan, persis seperti <c>contracts/validation-matrix.md</c>.</summary>
    public static class HmdMessages
    {
        public const string Val001 = "Permintaan tidak dapat dikirim karena alasan klinis belum diisi atau data kunjungan pasien tidak ditemukan.";
        public const string Val002 = "Permintaan ini sudah diproses orang lain. Muat ulang halaman untuk melihat status terbaru.";
        public const string Val003 = "Alasan penahanan wajib diisi agar unit peminta tahu apa yang perlu dilengkapi.";
        public const string Val004 = "Permintaan ini tidak sedang ditahan.";
        public const string Val005 = "Penolakan permintaan hemodialisa hanya dapat dilakukan dokter, dan alasan klinisnya wajib diisi.";
        public const string Val006 = "Permintaan sudah diterima unit Hemodialisa dan tidak dapat dibatalkan dari sini. Hubungi koordinator unit.";

        public const string Val010 = "Episode tidak dapat dibuat karena data pasien atau dokter penanggung jawab belum lengkap.";
        public const string Val011 = "Pasien ini sudah memiliki episode hemodialisa yang aktif. Tutup atau tangguhkan episode itu lebih dulu.";
        public const string Val012 = "Alasan penangguhan wajib diisi.";
        public const string Val013 = "Episode belum dapat ditutup karena masih ada sesi yang catatannya belum disahkan.";

        public const string Val020 = "Resep hanya dapat dibuat pada episode hemodialisa yang aktif.";
        public const string Val021 = "Resep belum dapat diaktifkan. Periksa kembali frekuensi, durasi, target penarikan cairan, dan akses vaskular yang dipilih.";
        public const string Val022 = "Penggantian resep gagal. Resep lama tetap berlaku.";
        public const string Val023 = "Resep tidak dapat dibatalkan karena sedang dipakai sesi yang berjalan.";
        public const string Val024 = "Resep yang sudah aktif tidak dapat diubah. Buat resep baru untuk menggantikannya.";

        public const string Val030 = "Sesi belum dapat dijadwalkan karena episode atau resep hemodialisa pasien belum aktif.";
        public const string Val031 = "Pasien sudah memiliki sesi lain pada jam tersebut.";
        public const string Val032 = "Mesin ini sudah dipakai pasien lain pada jam tersebut.";
        public const string Val033 = "Station ini sudah dipakai pasien lain pada jam tersebut.";
        public const string Val034 = "Mesin ini sedang tidak dapat digunakan. Pilih mesin lain.";
        public const string Val035 = "Pasien ini memerlukan mesin atau ruang khusus. Mesin yang dipilih tidak memenuhi kebutuhan tersebut.";
        public const string Val036 = "Pasien belum memiliki data kunjungan yang sah untuk hari ini.";
        public const string Val037 = "Petugas ini tidak memiliki kewenangan dialisis yang berlaku.";
        public const string Val038 = "Perawat ini sudah menangani jumlah pasien maksimum pada shift tersebut.";

        public const string WarningNotVerifiable = "Kewenangan petugas ini belum dapat diperiksa karena data kewenangan belum tersedia.";
        public const string WarningNurseRatio = "Perawat ini menangani lebih dari batas yang disarankan pada shift tersebut.";

        public const string Val040 = "Sesi belum dapat dinyatakan siap karena masih ada butir persiapan yang belum terpenuhi.";
        public const string Val041 = "Penilaian sebelum tindakan belum lengkap. Isi berat badan dan tanda vital pasien lebih dulu.";
        public const string Val042 = "Unit hemodialisa belum dinyatakan siap untuk shift ini.";
        public const string Val043 = "Sesi belum dapat dinyatakan siap karena dokter penanggung jawab belum ditetapkan.";
        public const string Val044 = "Butir ini tidak dapat dilewati. Lengkapi lebih dulu sebelum sesi dimulai.";
        public const string Val045 = "Alasan penahanan wajib diisi.";
        public const string Val046 = "Alasan wajib diisi ketika melewati butir persiapan.";
        public const string Val050 = "Sesi belum dapat dimulai. Periksa kembali status persiapan.";
        public const string Val051 = "Mesin berubah status dan tidak lagi dapat digunakan. Pilih mesin lain lalu nyatakan siap kembali.";
        public const string Val052 = "Konteks kunjungan pasien tidak dapat diverifikasi. Sesi tidak dapat dimulai.";
        public const string Val053 = "Sesi ini sudah dimulai.";

        public const string Val054 = "Pemantauan hanya dapat dicatat ketika sesi sedang berlangsung.";
        public const string Val055 = "Waktu pengamatan tidak boleh lebih awal dari waktu sesi dimulai.";
        public const string Val056 = "Catatan pemberian obat belum lengkap. Isi obat, dosis, satuan, dan jalur pemberiannya.";
        public const string Val057 = "Catatan komplikasi belum lengkap. Isi jenis kejadian dan tindakan yang dilakukan.";

        public const string Val060 = "Sesi belum dapat diselesaikan karena penilaian setelah tindakan dan tujuan pasien belum diisi.";
        public const string Val061 = "Alasan penghentian wajib diisi.";
        public const string Val070 = "Dokumentasi belum dapat diselesaikan. Periksa kembali waktu selesai, berat badan setelah tindakan, jumlah cairan yang ditarik, dan tanda vital akhir.";
        public const string Val071 = "Alasan pengembalian wajib diisi agar perawat tahu bagian mana yang perlu dilengkapi.";
        public const string Val072 = "Pengesahan catatan hemodialisa hanya dapat dilakukan dokter penanggung jawab sesi.";
        public const string Val073 = "Catatan gagal disahkan karena pendaftaran dokumen rekam medis tidak berhasil. Coba lagi.";
        public const string Val074 = "Pengesahan harus dilakukan orang yang berbeda dari yang menyelesaikan dokumentasi.";
        public const string Val075 = "Catatan sesi ini sudah disahkan dan tidak dapat diubah. Gunakan koreksi rekam medis untuk memperbaikinya.";

        public const string Val080 = "Alasan pembatalan wajib diisi.";

        public const string Val090 = "Mesin sedang dipakai sesi yang berlangsung dan statusnya belum dapat diubah.";
        public const string Val091 = "Alasan perubahan status wajib diisi agar riwayatnya dapat ditelusuri.";
        public const string Val092 = "Kode ini sudah dipakai. Gunakan kode lain.";

        public const string Val100 = "Lembar kesiapan untuk tanggal dan shift ini sudah ada.";
        public const string Val101 = "Unit belum dapat dinyatakan siap karena masih ada butir pemeriksaan yang belum terpenuhi.";
        public const string Val102 = "Hasil pemeriksaan pengolahan air sudah kedaluwarsa. Perbarui hasilnya sebelum menyatakan unit siap.";
        public const string Val103 = "Alasan wajib diisi ketika menyatakan unit tidak siap.";

        public const string Val900 = "Konteks pasien tidak dapat diverifikasi. Tidak ada data klinis yang dapat ditampilkan maupun disimpan.";
        public const string Val902 = "Data ini sudah diubah pengguna lain. Muat ulang halaman untuk melihat versi terbaru.";

        public const string ActorNotDoctor = "Akun Anda tidak tertaut ke data dokter yang aktif, sehingga tindakan ini tidak dapat dilakukan.";
        public const string SettingMissing = "Pengaturan unit hemodialisa belum tersedia. Hubungi admin unit untuk mengisinya lebih dulu.";
        public const string NumberAllocationFailed = "Nomor dokumen gagal diterbitkan. Hubungi administrator sistem.";
    }
}
