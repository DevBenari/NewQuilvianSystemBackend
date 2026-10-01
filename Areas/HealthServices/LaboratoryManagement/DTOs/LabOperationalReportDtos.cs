using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring ketiga laporan operasional (<c>LAB-API-v1</c> <c>r37</c> 32.3). Tanpa halaman —
    /// laporan adalah ringkasan, bukan daftar.
    /// </summary>
    public class LabOperationalReportQuery
    {
        /// <summary>
        /// Tanggal operasional <b>WIB</b> awal periode, inklusif. Wajib (<c>VAL-147</c>).
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Tanggal operasional <b>WIB</b> akhir periode, inklusif. Wajib (<c>VAL-147</c>); tidak
        /// sebelum <see cref="StartDate"/> (<c>VAL-148</c>); paling panjang 366 hari (<c>VAL-149</c>).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Kosong berarti seluruh disiplin. Nilai tak dikenal ditolak model binding.</summary>
        public LabDiscipline? Discipline { get; set; }
    }

    /// <summary>Periode yang benar-benar dipakai laporan.</summary>
    public class LabReportPeriodResponse
    {
        /// <summary>Tanggal operasional WIB.</summary>
        public DateOnly StartDate { get; set; }

        /// <summary>Tanggal operasional WIB.</summary>
        public DateOnly EndDate { get; set; }

        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Jumlah pemeriksaan yang <b>dirilis</b> pada periode itu (<c>LAB-DEC-159</c> butir 2,
    /// <c>INV-55</c>). Tervalidasi tanpa rilis tidak dihitung (<c>LAB-DEC-155</c>).
    /// </summary>
    public class LabExaminationCountReportResponse
    {
        public LabReportPeriodResponse Period { get; set; } = new();

        public List<LabExaminationCountRow> Rows { get; set; } = new();

        /// <summary>Jumlah seluruh baris yang <see cref="LabExaminationCountRow.IsCountable"/>.</summary>
        public int TotalCountable { get; set; }
    }

    /// <summary>Satu disiplin — atau <i>Belum tergolong</i> bagi order lama berdisiplin kosong.</summary>
    public class LabExaminationCountRow
    {
        /// <summary><c>ClinicalPathology</c>, <c>AnatomicalPathology</c>, <c>Microbiology</c>, atau kosong bagi <i>Belum tergolong</i>.</summary>
        public string? Discipline { get; set; }

        public string DisciplineName { get; set; } = string.Empty;

        /// <summary>
        /// Salah bila disiplin itu <b>belum punya jalur rilis</b> (<c>ARCH-GAP-LAB-11</c>) —
        /// <see cref="Total"/> lalu kosong, <b>bukan 0</b>.
        /// </summary>
        public bool IsCountable { get; set; }

        public string? NotCountableReason { get; set; }

        public int? Total { get; set; }

        /// <summary>Rincian per jenis pemeriksaan (23.10 butir 4). Kosong pada baris yang belum dapat dihitung.</summary>
        public List<LabExaminationCountProcedureRow> Procedures { get; set; } = new();
    }

    public class LabExaminationCountProcedureRow
    {
        public Guid ProcedureId { get; set; }

        /// <summary>Nama yang tersimpan saat dipesan (<c>ProcedureNameSnapshot</c>), bukan nama katalog hari ini.</summary>
        public string ProcedureName { get; set; } = string.Empty;

        public int Total { get; set; }
    }

    /// <summary>
    /// Angka penolakan wadah menurut <b>tanggal keputusan</b> kelayakan (<c>LAB-DEC-159</c> butir 3,
    /// <c>INV-56</c>). Berlaku bagi ketiga disiplin — keputusan kelayakan ada pada semuanya.
    /// </summary>
    public class LabSpecimenRejectionReportResponse
    {
        public LabReportPeriodResponse Period { get; set; } = new();

        public List<LabSpecimenRejectionRow> Rows { get; set; } = new();

        /// <summary>Rincian per disiplin dan per alasan penolakan.</summary>
        public List<LabSpecimenRejectionReasonRow> Reasons { get; set; } = new();
    }

    public class LabSpecimenRejectionRow
    {
        /// <summary><c>ClinicalPathology</c>, <c>AnatomicalPathology</c>, <c>Microbiology</c>, atau kosong bagi <i>Belum tergolong</i>.</summary>
        public string? Discipline { get; set; }

        public string DisciplineName { get; set; } = string.Empty;

        /// <summary>Wadah yang diputuskan — layak maupun tidak — pada periode itu.</summary>
        public int DecidedCount { get; set; }

        /// <summary>Wadah yang diputuskan tidak layak (kode alasan penolakan terisi).</summary>
        public int RejectedCount { get; set; }

        /// <summary>Tidak layak ÷ diputuskan × 100, satu desimal. <b>Kosong</b> bila nol wadah diputuskan — bukan 0.</summary>
        public decimal? RejectionRatePercent { get; set; }
    }

    public class LabSpecimenRejectionReasonRow
    {
        public string? Discipline { get; set; }

        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Nama alasan dari katalog alasan penolakan; kode bila baris katalognya tidak ditemukan.</summary>
        public string ReasonName { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    /// <summary>
    /// Waktu penyelesaian — dari <b>wadah dinyatakan layak</b> sampai <b>hasil dirilis</b>
    /// (<c>LAB-DEC-159</c> butir 4, <c>INV-57</c>, <c>ARCH-GAP-LAB-13</c>).
    /// </summary>
    public class LabTurnaroundTimeReportResponse
    {
        public LabReportPeriodResponse Period { get; set; } = new();

        public List<LabTurnaroundTimeRow> Rows { get; set; } = new();
    }

    /// <summary>Satu disiplin × satu kesegeraan.</summary>
    public class LabTurnaroundTimeRow
    {
        /// <summary><c>ClinicalPathology</c>, <c>AnatomicalPathology</c>, <c>Microbiology</c>, atau kosong bagi <i>Belum tergolong</i>.</summary>
        public string? Discipline { get; set; }

        public string DisciplineName { get; set; } = string.Empty;

        /// <summary><c>Cito</c> atau <c>Routine</c>.</summary>
        public string Urgency { get; set; } = string.Empty;

        /// <summary>
        /// Salah bila disiplin itu <b>belum punya jalur rilis</b> (<c>ARCH-GAP-LAB-11</c>) — seluruh
        /// angka lalu kosong, <b>bukan 0</b>.
        /// </summary>
        public bool IsCountable { get; set; }

        public string? NotCountableReason { get; set; }

        public int? ReleasedCount { get; set; }

        /// <summary>Rata-rata selang <i>wadah layak → hasil dirilis</i> dalam menit, satu desimal. Kosong bila nol hasil.</summary>
        public decimal? AverageMinutes { get; set; }

        /// <summary>
        /// Hanya pada baris <b>cito</b>: selang yang <b>melebihi</b> batas cito — batas dan perbandingan yang
        /// sama dengan daftar pantau keterlambatan. Tepat di batas tidak terlambat.
        /// </summary>
        public int? OverdueCount { get; set; }

        /// <summary>Hanya pada baris <b>cito</b>: pemeriksaan yang batas citonya belum diatur — tidak dinilai terlambat, tetap masuk rata-rata.</summary>
        public int? WithoutLimitCount { get; set; }
    }

    /// <summary>Bentuk penyaring laporan operasional — periode dan disiplin.</summary>
    public class LabOperationalReportFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>Panjang periode paling banyak, inklusif (<c>VAL-149</c>).</summary>
        public int MaxPeriodDays { get; set; }

        public List<LabEnumOptionResponse> Disciplines { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }
}
