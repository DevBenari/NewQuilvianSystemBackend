using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    // =====================================================================
    // Permintaan
    // =====================================================================

    /// <summary>
    /// Menyimpan seluruh isi laporan Patologi Anatomi sekaligus.
    ///
    /// <b>Bentuknya sengaja utuh, bukan per ruas.</b> Yang ditulis patolog adalah satu laporan
    /// diagnostik, bukan kumpulan isian lepas — dan <c>PUT</c> yang mengganti seluruh isi membuat
    /// dua penyunting bersamaan tidak dapat menghasilkan laporan yang tidak dimaksudkan keduanya.
    ///
    /// <b>Yang sengaja TIDAK diterima:</b> <c>issuedAt</c>, <c>effectiveAt</c>, waktu finalisasi,
    /// pelaku, dan nama parameter. Seluruhnya diturunkan server. Bila salah satunya tetap
    /// dikirim, permintaannya <b>ditolak terbuka</b> lewat <see cref="ExtraFields"/> — bukan
    /// diabaikan diam-diam, sebab pemanggil yang mengira waktunya tersimpan padahal tidak adalah
    /// keadaan yang justru berbahaya (<c>AC-140</c>).
    /// </summary>
    public class LabPathologyReportRequest
    {
        /// <summary>
        /// Tingkat temuan. <b>Nilai, bukan status lifecycle</b> — dinilai patolog secara manual.
        /// </summary>
        public LabPathologyFindingStatus? FindingStatus { get; set; }

        /// <summary>Penanggung jawab analis. Kosong bila belum ditetapkan.</summary>
        public Guid? AnalystUserId { get; set; }

        /// <summary>
        /// Nilai per parameter. Boleh sebagian saat menyimpan; kelengkapannya baru diuji saat
        /// <c>finalize</c> (<c>VAL-95</c>).
        /// </summary>
        public List<LabPathologyReportValueRequest> Values { get; set; } = new();

        /// <summary>
        /// Penampung ruas yang tidak dikenal. <b>Ia ada justru supaya ruas terlarang dapat
        /// ditolak, bukan supaya ruas tambahan dapat diterima.</b> Menaruh <c>issuedAt</c> dan
        /// <c>effectiveAt</c> sebagai properti biasa akan membuat keduanya tampil pada Swagger
        /// seolah-olah diterima; menampungnya di sini menolaknya tanpa pernah menjanjikannya.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraFields { get; set; }
    }

    /// <summary>
    /// Nilai satu ruas isian.
    ///
    /// <b>Nama parameter tidak diterima</b> — ia diturunkan server dari data induk, lalu
    /// disalin sebagai snapshot. Menerima namanya dari pemanggil berarti mengizinkan satu ruas
    /// tersimpan dengan label yang tidak pernah ada pada data induk.
    /// </summary>
    public class LabPathologyReportValueRequest
    {
        [Required]
        public Guid LabPathologyParameterId { get; set; }

        /// <summary>Isi yang ditulis patolog. Tanpa batas panjang (<c>RULE-011</c>).</summary>
        public string? Value { get; set; }

        /// <summary>Penampung ruas tidak dikenal; sebab yang sama dengan induknya.</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraFields { get; set; }
    }

    /// <summary>
    /// Membuka kembali laporan yang sudah difinalkan.
    ///
    /// <b>Alasannya wajib</b> (<c>VAL-97</c>). Sebelum difinalkan, laporan masih ditulis dan
    /// perubahan adalah bagian dari menulis. Sesudah difinalkan, patolog sudah menyatakan
    /// diagnosisnya selesai — mengubahnya kembali adalah pernyataan bahwa pernyataan sebelumnya
    /// perlu diperbaiki, dan itu perlu dapat dijelaskan.
    /// </summary>
    public class LabPathologyReopenRequest
    {
        /// <summary>
        /// Alasan membuka kembali. Wajib, dan <b>sengaja TANPA <c>[Required]</c></b>.
        ///
        /// Penjaga model bawaan menolak dengan <c>400</c>, sedangkan <c>VAL-97</c> menetapkan
        /// <c>422</c> beserta kalimat yang sudah ditentukan bagi pengguna. Membiarkan
        /// <c>[Required]</c> membuat penjaga bawaan menembak lebih dulu, sehingga aturan
        /// bisnisnya nol pernah tercapai dan pesannya nol pernah terlihat — kelas kesalahan yang
        /// sama dengan penjaga <c>VAL-76</c> yang dicabut pada <c>r18</c> karena terbukti tidak
        /// pernah tercapai.
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Konteks klinis pesanan, ditulis <b>dokter pemesan</b>.
    ///
    /// Jalur tulisnya memakai <c>LabOrder : Update</c>, bukan <c>LabExamination : Update</c>
    /// (<c>LAB-DEC-091</c>, <c>INV-40</c>) — sebab berbagi satu izin dengan patolog berarti
    /// memberi patolog hak menulis riwayat penyakit yang tidak pernah ia tanyakan.
    /// </summary>
    public class LabPathologyOrderContextRequest
    {
        /// <summary>Diagnosa Awal. Menjadi sumber pengisian otomatis Diagnosa Klinis pada IHK.</summary>
        public string? InitialDiagnosis { get; set; }

        /// <summary>Riwayat penyakit yang relevan bagi pembacaan jaringan.</summary>
        public string? RelevantHistory { get; set; }

        /// <summary>Masa terakhir haid. Hanya bermakna bagi sitologi ginekologi.</summary>
        public DateOnly? LastMenstrualPeriod { get; set; }

        /// <summary>Keterangan klinis lain.</summary>
        public string? ClinicalNote { get; set; }
    }

    // =====================================================================
    // Tanggapan
    // =====================================================================

    /// <summary>
    /// Satu ruas pada formulir laporan, beserta isinya bila sudah diisi.
    ///
    /// <b>Bentuk inilah yang membuat layar nol perlu menebak formulirnya.</b> Urutan, label,
    /// penanda wajib, dan isinya seluruhnya datang dari sini — sehingga ruas keenam belas kelak
    /// cukup menambah satu baris data induk tanpa rilis frontend.
    ///
    /// <b>Satu ruas muncul SEKALI walau dua kategori memakainya</b> (<c>AC-136</c>). Pesanan
    /// Histologi + Imunohistokimia memakai <c>Anjuran</c> pada keduanya, dan patolog hanya perlu
    /// mengisinya satu kali.
    /// </summary>
    public class LabPathologyReportFieldResponse
    {
        public Guid LabPathologyParameterId { get; set; }

        public string ParameterCode { get; set; } = string.Empty;

        public string ParameterName { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        /// <summary>Wajib terisi sebelum laporan dapat difinalkan (<c>VAL-95</c>).</summary>
        public bool IsRequired { get; set; }

        /// <summary>Isi yang sudah tersimpan. Kosong berarti belum diisi.</summary>
        public string? Value { get; set; }

        /// <summary>
        /// Golongan yang membuat ruas ini berlaku. Lebih dari satu ketika pesanannya memuat
        /// beberapa golongan — dan ruasnya tetap satu.
        /// </summary>
        public List<string> CategoryCodes { get; set; } = new();
    }

    /// <summary>
    /// Laporan beserta bentuk formulirnya.
    ///
    /// <b><see cref="IssuedAt"/> dan <see cref="EffectiveAt"/> adalah TURUNAN</b>
    /// (<c>INV-38</c>, <c>LAB-DEC-092</c>): yang pertama dari <c>FinalizedAt</c>, yang kedua dari
    /// waktu pengambilan bahan. Keduanya nol disimpan, dan nol dapat dikirim pemanggil.
    /// </summary>
    public class LabPathologyReportResponse
    {
        public Guid LabOrderId { get; set; }

        /// <summary>Kosong ketika laporannya belum pernah disimpan sama sekali.</summary>
        public Guid? Id { get; set; }

        public LabPathologyFindingStatus? FindingStatus { get; set; }

        public Guid? AnalystUserId { get; set; }

        /// <summary>
        /// Kapan patolog menyatakan laporannya selesai ditulis. <b>Bukan waktu rilis</b>
        /// (<c>LAB-DEC-088</c>).
        /// </summary>
        public DateTime? FinalizedAt { get; set; }

        public Guid? FinalizedByUserId { get; set; }

        public int ReopenCount { get; set; }

        /// <summary>
        /// Apakah laporan sedang dalam keadaan final. <b>Diturunkan dari
        /// <see cref="FinalizedAt"/>, bukan dibaca dari kolom status</b> — <c>INV-36</c>
        /// menegakkan nol status lifecycle pada tabelnya.
        /// </summary>
        public bool IsFinalized { get; set; }

        /// <summary>Turunan dari <see cref="FinalizedAt"/>. Nol disimpan.</summary>
        public DateTime? IssuedAt { get; set; }

        /// <summary>
        /// Turunan dari waktu pengambilan bahan paling awal pada pesanan ini. Nol disimpan.
        /// </summary>
        public DateTime? EffectiveAt { get; set; }

        /// <summary>Ruas yang berlaku bagi pesanan ini, berurut sesuai tampilnya.</summary>
        public List<LabPathologyReportFieldResponse> Fields { get; set; } = new();

        /// <summary>
        /// Terisi <b>hanya</b> ketika <see cref="Fields"/> kosong, dan menyebutkan apa yang
        /// belum diatur beserta siapa yang mengaturnya (<c>VAL-100</c>).
        ///
        /// <b>Ruas ini ada karena daftar kosong tanpa sebab akan terbaca sebagai kerusakan.</b>
        /// Keadaan ini PASTI terjadi pada hari pertama, sebelum pemetaan jenis pemeriksaan
        /// diisi — dan patolog pertama yang membukanya akan mengira sistemnya rusak.
        /// </summary>
        public string? FormUnavailableReason { get; set; }
    }

    /// <summary>Konteks klinis yang tersimpan pada satu pesanan.</summary>
    public class LabPathologyOrderContextResponse
    {
        public Guid LabOrderId { get; set; }

        /// <summary>Kosong ketika konteksnya belum pernah ditulis.</summary>
        public Guid? Id { get; set; }

        public string? InitialDiagnosis { get; set; }

        public string? RelevantHistory { get; set; }

        public DateOnly? LastMenstrualPeriod { get; set; }

        public string? ClinicalNote { get; set; }
    }
}
