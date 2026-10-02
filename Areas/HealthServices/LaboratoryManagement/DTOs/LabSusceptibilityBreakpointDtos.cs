namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>Penyaring daftar rentang breakpoint.</summary>
    public class LabSusceptibilityBreakpointPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public bool? IsActive { get; set; }

        /// <summary>Menyaring per organisme.</summary>
        public Guid? LabOrganismId { get; set; }

        /// <summary>Menyaring per antibiotik.</summary>
        public Guid? LabAntibioticId { get; set; }

        /// <summary>Pencarian pada nama organisme dan nama antibiotik.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Menambah rentang breakpoint (<c>LAB-API-v1</c> <c>r27</c> bagian 22.5).
    ///
    /// <b>Hak tulisnya dipegang wewenang klinis Mikrobiologi</b>, bukan kepala instalasi:
    /// angka di sini menentukan pasien mendapat antibiotik yang benar.
    /// </summary>
    public class CreateLabSusceptibilityBreakpointRequest
    {
        public Guid LabOrganismId { get; set; }

        public Guid LabAntibioticId { get; set; }

        /// <summary>Zona <b>di bawah</b> nilai ini menghasilkan <c>Resistant</c>.</summary>
        public int LowerMm { get; set; }

        /// <summary>
        /// Zona <b>di atas</b> nilai ini menghasilkan <c>Sensitive</c>. Tidak boleh lebih
        /// kecil daripada <c>LowerMm</c> (<c>VAL-115</c>).
        /// </summary>
        public int UpperMm { get; set; }

        /// <summary>Versi panduan, misalnya <c>CLSI M100 ED34</c>.</summary>
        public string? GuidelineVersion { get; set; }
    }

    /// <summary>Mengubah rentang breakpoint beserta status aktifnya.</summary>
    public class UpdateLabSusceptibilityBreakpointRequest
    {
        public int LowerMm { get; set; }

        public int UpperMm { get; set; }

        public string? GuidelineVersion { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>Satu baris rentang breakpoint beserta nama data induknya.</summary>
    public class LabSusceptibilityBreakpointResponse
    {
        public Guid Id { get; set; }

        public Guid LabOrganismId { get; set; }

        public string? OrganismName { get; set; }

        public Guid LabAntibioticId { get; set; }

        public string? AntibioticName { get; set; }

        /// <summary>Kandungan cakram antibiotik ini, bila ada.</summary>
        public int? DiscContentUg { get; set; }

        public int LowerMm { get; set; }

        public int UpperMm { get; set; }

        public string? GuidelineVersion { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Ringkasan data induk breakpoint (<c>GET /summary</c>).
    ///
    /// Dihitung <b>hanya dari baris yang belum ditandai terhapus</b>, mengikuti baseline
    /// master data. Ketiga pencacah terakhir menjawab pertanyaan yang benar-benar ditanyakan
    /// wewenang klinis: berapa kombinasi yang sudah punya rentang, dan berapa yang masih
    /// kosong kandungan cakramnya.
    /// </summary>
    public class LabSusceptibilityBreakpointSummaryResponse
    {
        public int TotalBreakpoint { get; set; }

        public int ActiveBreakpoint { get; set; }

        public int InactiveBreakpoint { get; set; }

        /// <summary>Banyaknya organisme berbeda yang sudah punya sedikitnya satu rentang.</summary>
        public int CoveredOrganism { get; set; }

        /// <summary>Banyaknya antibiotik berbeda yang sudah punya sedikitnya satu rentang.</summary>
        public int CoveredAntibiotic { get; set; }

        /// <summary>Baris aktif yang kandungan cakramnya belum diisi — kolom <c>UG</c> pada cetakan.</summary>
        public int MissingDiscContent { get; set; }
    }

    /// <summary>
    /// Baris ringan untuk dropdown (<c>GET /options</c>).
    ///
    /// <b>Sengaja jauh lebih ringkas daripada baris list.</b> Pemakainya hanya perlu
    /// mengenali kombinasinya; rentang dan versi pedoman nol dipakai memilih.
    /// </summary>
    public class LabSusceptibilityBreakpointOptionResponse
    {
        public Guid Id { get; set; }

        /// <summary>Teks gabungan kuman dan antibiotik, misalnya <c>Branhamella catarrhalis — Ampicillin</c>.</summary>
        public string Label { get; set; } = string.Empty;

        public Guid LabOrganismId { get; set; }

        public Guid LabAntibioticId { get; set; }
    }

    /// <summary>
    /// Mengubah status aktif saja (<c>PATCH /{id}/status</c>).
    ///
    /// <b>Endpoint tersendiri, bukan <c>PUT</c> bersebagian ruas.</b> Menonaktifkan satu
    /// rentang yang keliru dan mengubah angka rentangnya adalah dua tindakan yang berbeda
    /// akibatnya; menyatukannya pada satu jalur membuat keduanya sama mudahnya terjadi tanpa
    /// sengaja — pada data yang menentukan penilaian <c>S</c>/<c>I</c>/<c>R</c>.
    /// </summary>
    public class LabSusceptibilityBreakpointStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
