using System.Text.Json.Serialization;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring Statistik Pesanan (<c>LAB-API-v1</c> <c>r49</c> 44.4). Mewarisi periode dan disiplin
    /// laporan operasional, sehingga aturan periode <c>VAL-147</c>..<c>VAL-149</c> dipakai ulang apa adanya.
    /// Tanpa halaman — laporan adalah ringkasan.
    /// </summary>
    public class LabOrderStatisticsReportQuery : LabOperationalReportQuery
    {
        /// <summary>
        /// Kosong = seluruh jenis kunjungan. Nilai tak dikenal ditolak model binding. Konverter teks: isi JSON
        /// <c>POST /print-log</c> menerima nama seperti query; angka tetap diterima.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EncounterType? EncounterType { get; set; }

        /// <summary>Unit layanan kunjungan. Kosong = seluruh unit.</summary>
        public Guid? ServiceUnitId { get; set; }
    }

    /// <summary>
    /// Isi <c>POST /print-log</c> — penyaring yang sama dengan laporan yang sedang tampil
    /// (<c>LAB-DEC-248</c>).
    /// </summary>
    public class LabOrderStatisticsReportPrintRequest : LabOrderStatisticsReportQuery
    {
    }

    /// <summary>
    /// Beban permintaan per jenis kunjungan → unit layanan → disiplin (<c>LAB-DEC-241</c>..<c>246</c>).
    /// Dihitung menurut <b>waktu pesanan diminta</b> (WIB); batal, <c>Draft</c>, dan gugur tidak dihitung —
    /// sama dengan Beranda (<c>INV-59</c>). <b>Tanpa identitas pasien</b>.
    /// </summary>
    public class LabOrderStatisticsReportResponse
    {
        public LabReportPeriodResponse Period { get; set; } = new();

        public List<LabOrderStatisticsGroup> Groups { get; set; } = new();

        /// <summary>Atas seluruh hasil penyaring, bukan satu halaman (<c>RPT-003</c>).</summary>
        public LabOrderStatisticsTotals Total { get; set; } = new();
    }

    /// <summary>Satu jenis kunjungan.</summary>
    public class LabOrderStatisticsGroup
    {
        /// <summary><c>Outpatient</c>, <c>Emergency</c>, <c>Inpatient</c>, <c>MedicalCheckup</c>, <c>Telemedicine</c>, atau <c>Unknown</c>.</summary>
        public string EncounterType { get; set; } = string.Empty;

        public string EncounterTypeName { get; set; } = string.Empty;

        public List<LabOrderStatisticsUnitRow> Units { get; set; } = new();

        public LabOrderStatisticsTotals Subtotal { get; set; } = new();
    }

    /// <summary>Satu unit layanan di dalam satu jenis kunjungan.</summary>
    public class LabOrderStatisticsUnitRow
    {
        /// <summary>Kosong bila kunjungan tidak menunjuk unit yang tercatat.</summary>
        public Guid? ServiceUnitId { get; set; }

        /// <summary>Nama dari data induk apa adanya (<c>LAB-DEC-243</c>); <i>Unit tidak tercatat</i> bila tidak ditemukan.</summary>
        public string ServiceUnitName { get; set; } = string.Empty;

        public List<LabOrderStatisticsCell> Cells { get; set; } = new();

        public int OrderCount { get; set; }

        public int ExaminationCount { get; set; }
    }

    /// <summary>Satu disiplin pada satu baris.</summary>
    public class LabOrderStatisticsCell
    {
        /// <summary><c>ClinicalPathology</c>, <c>AnatomicalPathology</c>, <c>Microbiology</c>, atau kosong bagi <i>Belum terklasifikasi</i>.</summary>
        public string? Discipline { get; set; }

        public string DisciplineName { get; set; } = string.Empty;

        /// <summary>Pesanan bukan batal/<c>Draft</c>.</summary>
        public int OrderCount { get; set; }

        /// <summary>Pemeriksaan bukan batal/gugur, ditambah permintaan yang belum masuk wadah.</summary>
        public int ExaminationCount { get; set; }
    }

    /// <summary>Jumlah — dipakai <c>Subtotal</c> per jenis kunjungan dan <c>Total</c>.</summary>
    public class LabOrderStatisticsTotals
    {
        public int OrderCount { get; set; }

        public int ExaminationCount { get; set; }

        public List<LabOrderStatisticsCell> Cells { get; set; } = new();
    }

    /// <summary>Bentuk penyaring Statistik Pesanan — periode, jenis kunjungan, unit layanan, disiplin.</summary>
    public class LabOrderStatisticsReportFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>Panjang periode paling banyak, inklusif (<c>VAL-149</c>).</summary>
        public int MaxPeriodDays { get; set; }

        public List<LabEnumOptionResponse> EncounterTypes { get; set; } = new();

        /// <summary>
        /// Unit layanan aktif dari data induk — dibaca di sini supaya pembaca laporan tidak membutuhkan izin
        /// <c>service-units/options</c>.
        /// </summary>
        public List<LabServiceUnitOptionResponse> ServiceUnits { get; set; } = new();

        public List<LabEnumOptionResponse> Disciplines { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class LabServiceUnitOptionResponse
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Jawaban <c>POST /print-log</c> laporan <c>S16b</c> (<c>LAB-DEC-248</c>). Layar baru membuka dialog
    /// cetak sesudah menerima jawaban ini. Dipakai bersama Laporan Pembatalan (<c>BE-LAB-100</c>).
    /// </summary>
    public class LabReportPrintLogResponse
    {
        public DateTime LoggedAt { get; set; }
    }
}
