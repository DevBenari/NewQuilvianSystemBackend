using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Mengisi hasil Mikrobiologi berstruktur — status temuan beserta <b>seluruh</b> isolat
    /// dan kepekaannya sekaligus (<c>LAB-API-v1</c> <c>r24</c> bagian 19.2, diperluas
    /// <c>r27</c> bagian 22.2).
    ///
    /// <b>Satu PUT utuh, bukan endpoint terpisah per isolat.</b> Isolat dan kepekaannya adalah
    /// <i>isi</i> sebuah hasil, bukan benda yang berdiri sendiri. Endpoint terpisah akan
    /// membuat separuh hasil dapat tersimpan tanpa separuh lainnya — dan itu persis batas
    /// konsistensi yang dilindungi aggregate.
    ///
    /// <b>Empat hal sengaja TIDAK diterima:</b> nama organisme, nama antibiotik, kandungan
    /// cakram, dan rentang breakpoint. Keempatnya <b>diturunkan server</b> dari data induk
    /// lalu disimpan sebagai snapshot. Menerimanya berarti mengizinkan hasil menyebut kuman
    /// yang tidak ada di daftar mana pun, atau breakpoint yang tidak pernah disahkan siapa pun.
    /// </summary>
    public class LabMicrobiologyResultRequest
    {
        /// <summary><b>Wajib</b> (<c>VAL-103</c>). Normal, Positif, atau Negatif.</summary>
        public LabMicrobiologyFinding? MicrobiologyFinding { get; set; }

        /// <summary>Kapan pemeriksaannya dikerjakan. Tidak boleh masa depan (<c>VAL-89</c>).</summary>
        public DateTime? ExaminedAt { get; set; }

        /// <summary>Kualifikasi yang dicetak — <c>Definitif</c> atau <c>Sementara</c>.</summary>
        public LabResultQualifier? ResultQualifier { get; set; }

        /// <summary>Jenis biakan — menentukan kata pada label cetak.</summary>
        public LabCultureType? CultureType { get; set; }

        /// <summary>Metode uji — menentukan kolom mana yang berlaku.</summary>
        public LabSusceptibilityMethod? SusceptibilityMethod { get; set; }

        /// <summary>
        /// Boleh <b>kosong</b> — nol pertumbuhan adalah hasil yang sah (<c>AC-111</c>).
        /// </summary>
        public List<LabMicrobiologyIsolateRequest> Isolates { get; set; } = [];
    }

    /// <summary>Satu organisme yang ditemukan pada biakan.</summary>
    public class LabMicrobiologyIsolateRequest
    {
        /// <summary><b>Wajib</b>, menunjuk organisme yang aktif (<c>VAL-85</c>).</summary>
        public Guid LabOrganismId { get; set; }

        /// <summary>
        /// Bernilai salah untuk kuman yang ditemukan tetapi <b>tidak diuji</b> kepekaannya
        /// (<c>LAB-DEC-126</c>). Isolat seperti itu tidak boleh punya baris kepekaan
        /// (<c>VAL-117</c>).
        /// </summary>
        public bool IsSusceptibilityTested { get; set; } = true;

        /// <summary>Catatan analis, maksimal 500.</summary>
        public string? Note { get; set; }

        public List<LabIsolateSusceptibilityRequest> Susceptibilities { get; set; } = [];
    }

    /// <summary>Satu antibiotik yang diujikan terhadap satu isolat.</summary>
    public class LabIsolateSusceptibilityRequest
    {
        /// <summary><b>Wajib</b>, menunjuk antibiotik yang aktif (<c>VAL-86</c>).</summary>
        public Guid LabAntibioticId { get; set; }

        /// <summary>Nilai MIC pada metode dilusi.</summary>
        public decimal? Concentration { get; set; }

        /// <summary><b>Wajib bila <c>Concentration</c> terisi</b> (<c>VAL-112</c>).</summary>
        public Guid? ConcentrationUnitId { get; set; }

        /// <summary>
        /// Lebar zona hambat pada metode difusi. <b><c>0</c> adalah nilai sah</b>; kosong
        /// berarti belum diukur (<c>VAL-116</c>, <c>LAB-DEC-128</c>).
        /// </summary>
        public int? ZoneDiameterMm { get; set; }

        /// <summary>
        /// <b>Opsional sejak <c>r27</c>.</b> Bila kosong, server menghitungnya dari zona
        /// terhadap breakpoint. Menjadi <b>wajib</b> ketika breakpoint belum disetel
        /// (<c>VAL-114</c>).
        /// </summary>
        public LabSusceptibilityResult? Result { get; set; }

        /// <summary>
        /// <b>Wajib bila nilai yang dikirim berbeda dari hitungan sistem</b>
        /// (<c>VAL-113</c>). Penimpaan dibuka sebab resistensi intrinsik menuntut penilaian
        /// di luar rumus.
        /// </summary>
        public string? ResultOverrideReason { get; set; }

        /// <summary>Catatan analis, maksimal 500.</summary>
        public string? Note { get; set; }
    }

    /// <summary>Hasil Mikrobiologi yang tersimpan, dibaca utuh.</summary>
    public class LabMicrobiologyResultResponse
    {
        public Guid LabExaminationId { get; set; }

        public Guid LabOrderId { get; set; }

        public string? ProcedureName { get; set; }

        public LabMicrobiologyFinding? MicrobiologyFinding { get; set; }

        public LabResultQualifier? ResultQualifier { get; set; }

        public LabCultureType? CultureType { get; set; }

        public LabSusceptibilityMethod? SusceptibilityMethod { get; set; }

        public DateTime? ExaminedAt { get; set; }

        public DateTime? ResultEnteredAt { get; set; }

        /// <summary>Nama analis, diturunkan dari pencatat (<c>LAB-DEC-105</c>).</summary>
        public Guid? ResultEnteredByUserId { get; set; }

        /// <summary><c>FinalizedAt != null</c>.</summary>
        public bool IsFinalized { get; set; }

        /// <summary>
        /// <b>Selalu salah pada rilis ini</b> — rilis Mikrobiologi adalah <c>S4d</c>.
        /// Ruas ini ada supaya pemanggil nol perlu menyimpulkan bahwa Final sama dengan rilis.
        /// </summary>
        public bool IsReleased { get; set; }

        /// <summary>
        /// Ada tidaknya aturan nilai kritis yang berlaku sama sekali (<c>LAB-DEC-103</c>
        /// butir 5).
        ///
        /// <b>Bukan ruas hiasan.</b> Tanpa ia, layar tidak dapat membedakan <i>"tidak ada
        /// yang kritis"</i> dari <i>"belum ada aturannya"</i> — keduanya terlihat persis
        /// sama: nol penanda menyala. Layar <b>wajib</b> menyatakan keadaan kedua secara
        /// terbaca, bukan diam.
        /// </summary>
        public bool CriticalRuleAvailable { get; set; }

        public List<LabMicrobiologyIsolateResponse> Isolates { get; set; } = [];
    }

    /// <summary>Satu isolat beserta antibiogramnya.</summary>
    public class LabMicrobiologyIsolateResponse
    {
        public Guid Id { get; set; }

        public Guid LabOrganismId { get; set; }

        /// <summary>Snapshot — nama sebagaimana berlaku saat isolat dicatat.</summary>
        public string? OrganismName { get; set; }

        public bool IsSusceptibilityTested { get; set; }

        /// <summary>
        /// Apakah organisme ini punya sedikitnya satu rentang breakpoint yang berlaku.
        ///
        /// <b>Bernilai salah berarti interpretasinya memang perlu diketik</b> — bukan berarti
        /// sistem gagal. Tanpa ruas ini keduanya terlihat persis sama pada layar.
        /// </summary>
        public bool BreakpointAvailable { get; set; }

        public string? Note { get; set; }

        public List<LabIsolateSusceptibilityResponse> Susceptibilities { get; set; } = [];
    }

    /// <summary>Satu baris antibiogram.</summary>
    public class LabIsolateSusceptibilityResponse
    {
        public Guid Id { get; set; }

        public Guid LabAntibioticId { get; set; }

        /// <summary>Snapshot nama antibiotik.</summary>
        public string? AntibioticName { get; set; }

        /// <summary>Snapshot kandungan cakram — kolom <c>UG</c> pada cetakan.</summary>
        public int? DiscContentUg { get; set; }

        /// <summary>Snapshot batas bawah — bagian kolom <c>R-S</c> pada cetakan.</summary>
        public int? BreakpointLowerMm { get; set; }

        /// <summary>Snapshot batas atas.</summary>
        public int? BreakpointUpperMm { get; set; }

        public decimal? Concentration { get; set; }

        public Guid? ConcentrationUnitId { get; set; }

        public string? ConcentrationUnitSymbol { get; set; }

        public int? ZoneDiameterMm { get; set; }

        /// <summary>Hitungan sistem saat baris disimpan. Kosong bila breakpoint tidak ada.</summary>
        public LabSusceptibilityResult? ComputedResult { get; set; }

        /// <summary>Nilai yang berlaku dan yang dicetak.</summary>
        public LabSusceptibilityResult Result { get; set; }

        public bool IsResultOverridden { get; set; }

        /// <summary>
        /// Apakah baris ini tergolong kritis menurut data induk aturan (<c>LAB-DEC-103</c>).
        ///
        /// <b>Dihitung saat dibaca, nol disimpan.</b> Aturan kritis dapat berubah, dan nilai
        /// tersimpan akan membekukan penilaian lama sebagai kalau-kalau fakta. Berbeda dari
        /// <see cref="ComputedResult"/>, yang memang fakta masa lalu.
        ///
        /// <b>Bernilai salah TIDAK berarti aman</b> bila
        /// <c>LabMicrobiologyResultResponse.CriticalRuleAvailable</c> juga salah — itu berarti
        /// aturannya belum disetel sama sekali.
        /// </summary>
        public bool IsCritical { get; set; }

        public string? ResultOverrideReason { get; set; }

        public string? Note { get; set; }
    }
}
