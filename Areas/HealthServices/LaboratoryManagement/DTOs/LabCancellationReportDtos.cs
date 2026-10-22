using System.Text.Json.Serialization;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring Laporan Pembatalan (<c>LAB-API-v1</c> <c>r49</c> 44.4, <c>BE-LAB-99</c>). Mewarisi periode dan disiplin
    /// laporan operasional — aturan periode <c>VAL-147</c>..<c>VAL-149</c> dipakai ulang; periode dibandingkan dengan
    /// <b>waktu pembatalan</b> (WIB).
    /// </summary>
    public class LabCancellationReportQuery : LabOperationalReportQuery
    {
        /// <summary>Kosong = pembatalan pesanan dan pemeriksaan.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LabCancellationKind? CancellationKind { get; set; }

        /// <summary>Petugas yang membatalkan — pilihan dari <c>/cancelled-by-options</c>.</summary>
        public Guid? CancelledByUserId { get; set; }

        /// <summary>Unit layanan kunjungan pesanan.</summary>
        public Guid? ServiceUnitId { get; set; }

        /// <summary>Bawaan 1.</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Bawaan 25; dijepit 1..100, tidak ditolak (pola <c>LabOrderPagedQuery</c>, 32.10 no. 5).</summary>
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Daftar peristiwa pembatalan — satu baris per peristiwa (<c>INV-58</c>), terbaru di atas. <see cref="Totals"/>
    /// dihitung atas <b>seluruh</b> hasil penyaring, sama di setiap halaman (<c>AC-332</c>).
    /// </summary>
    public class LabCancellationReportResponse
    {
        public LabReportPeriodResponse Period { get; set; } = new();

        public LabCancellationReportTotals Totals { get; set; } = new();

        public List<LabCancellationReportRow> Items { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }
    }

    /// <summary>
    /// Isi <c>POST /print-log</c> Laporan Pembatalan — penyaring yang sama dengan daftar yang sedang tampil
    /// (<c>LAB-DEC-248</c>, <c>BE-LAB-100</c>). Ruas halaman diabaikan.
    /// </summary>
    public class LabCancellationReportPrintRequest : LabCancellationReportQuery
    {
    }

    /// <summary>
    /// Seluruh baris hasil penyaring tanpa halaman — bahan berkas unduhan (<c>BE-LAB-100</c>). Bukan kontrak HTTP: yang
    /// keluar adalah berkas <c>.xlsx</c>.
    /// </summary>
    public class LabCancellationReportExport
    {
        public LabReportPeriodResponse Period { get; set; } = new();

        public LabCancellationReportTotals Totals { get; set; } = new();

        public List<LabCancellationReportRow> Items { get; set; } = new();
    }

    public class LabCancellationReportTotals
    {
        public int OrderCount { get; set; }

        public int ExaminationCount { get; set; }

        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Satu peristiwa pembatalan. Memuat <b>identitas pasien</b> (<c>LAB-DEC-240</c>) — dibaca saat diminta, tidak
    /// disalin, dan tidak pernah masuk log (A8.12).
    /// </summary>
    public class LabCancellationReportRow
    {
        /// <summary>Penunjuk baris riwayat — dipakai <c>GET /{historyId}</c>.</summary>
        public Guid HistoryId { get; set; }

        /// <summary>Waktu peristiwa (UTC); layar menampilkan WIB.</summary>
        public DateTime CancelledAt { get; set; }

        /// <summary><c>Order</c> atau <c>Examination</c>.</summary>
        public string Kind { get; set; } = string.Empty;

        public Guid LabOrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }

        /// <summary><c>ClinicalPathology</c>, <c>AnatomicalPathology</c>, <c>Microbiology</c>, atau kosong.</summary>
        public string? Discipline { get; set; }

        public string DisciplineName { get; set; } = string.Empty;

        /// <summary>Hanya baris <c>Examination</c>.</summary>
        public Guid? LabExaminationId { get; set; }

        /// <summary>Hanya baris <c>Examination</c>: nama tersimpan saat dipesan.</summary>
        public string? ExaminationName { get; set; }

        /// <summary>Hanya baris <c>Order</c>: jumlah pemeriksaan di dalam pesanan itu, termasuk yang belum masuk wadah.</summary>
        public int? ExaminationCount { get; set; }

        /// <summary>Status sebelum dibatalkan.</summary>
        public string? FromStatus { get; set; }

        /// <summary>Hanya baris <c>Examination</c>: benar bila status sebelum batal <c>ChargeEligible</c>.</summary>
        public bool? WasChargeEligible { get; set; }

        /// <summary>Kosong bila pembatalan lama tersimpan tanpa alasan — lihat <see cref="IsReasonRecorded"/>.</summary>
        public string? Reason { get; set; }

        /// <summary>Salah → layar menulis <i>Alasan tidak dicatat</i> (<c>AC-328</c>).</summary>
        public bool IsReasonRecorded { get; set; }

        public Guid CancelledByUserId { get; set; }

        public string? CancelledByName { get; set; }

        public string? ServiceUnitName { get; set; }
    }

    /// <summary>Detail satu peristiwa: pesanan, pemeriksaan dan wadahnya, serta riwayat transisi pesanan.</summary>
    public class LabCancellationReportDetailResponse
    {
        public LabCancellationReportRow Row { get; set; } = new();

        public List<LabCancellationExaminationItem> Examinations { get; set; } = new();

        public List<LabCancellationSpecimenItem> Specimens { get; set; } = new();

        public List<LabCancellationHistoryItem> History { get; set; } = new();
    }

    public class LabCancellationExaminationItem
    {
        /// <summary>Kosong bagi permintaan yang belum masuk wadah.</summary>
        public Guid? LabExaminationId { get; set; }

        public string? ExaminationName { get; set; }

        /// <summary>Status pemeriksaan, atau <c>AwaitingSpecimen</c> bagi permintaan yang belum masuk wadah.</summary>
        public string ExaminationStatus { get; set; } = string.Empty;
    }

    public class LabCancellationSpecimenItem
    {
        public Guid LabSpecimenId { get; set; }

        public string SpecimenBarcode { get; set; } = string.Empty;

        public string SpecimenStatus { get; set; } = string.Empty;
    }

    public class LabCancellationHistoryItem
    {
        public string Action { get; set; } = string.Empty;

        public string? FromStatus { get; set; }

        public string ToStatus { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? ActorName { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    public class LabCancelledByOptionResponse
    {
        public Guid UserId { get; set; }

        public string? Name { get; set; }
    }

    /// <summary>Bentuk penyaring Laporan Pembatalan.</summary>
    public class LabCancellationReportFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>Panjang periode paling banyak, inklusif (<c>VAL-149</c>).</summary>
        public int MaxPeriodDays { get; set; }

        public int DefaultPageSize { get; set; } = 25;

        public int MaxPageSize { get; set; } = 100;

        public List<LabEnumOptionResponse> Disciplines { get; set; } = new();

        public List<LabEnumOptionResponse> CancellationKinds { get; set; } = new();

        /// <summary>Unit layanan aktif dari data induk.</summary>
        public List<LabServiceUnitOptionResponse> ServiceUnits { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }
}
