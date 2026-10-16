namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    /// <summary>
    /// Rentang laporan operasional Radiologi — menu v1 Laporan dan Pemeriksaan (<c>BE-RAD-24</c>,
    /// <c>RAD-DEC-021</c>). Tanggal kalender WIB <c>yyyy-MM-dd</c>; kosong = 30 hari terakhir; maksimal 366 hari.
    /// </summary>
    public class RadOperationalReportQuery
    {
        public string? StartDate { get; set; }

        public string? EndDate { get; set; }
    }

    /// <summary>Bungkus laporan operasional: rentang yang benar-benar dipakai server dan barisnya.</summary>
    public class RadOperationalReportResponse<T>
    {
        public string PeriodStartDate { get; set; } = string.Empty;

        public string PeriodEndDate { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }

        /// <summary>Benar bila baris rincian dipotong pada batas server.</summary>
        public bool IsTruncated { get; set; }

        public List<T> Rows { get; set; } = new();
    }

    /// <summary>Statistik Pemeriksaan — satu baris per pemeriksaan.</summary>
    public class RadExaminationStatisticRow
    {
        public Guid ProcedureId { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string ModalityName { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Outpatient { get; set; }
        public int Inpatient { get; set; }
        public int Emergency { get; set; }
        public int Urgent { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    /// <summary>Dokter Pengirim — dokter pemberi instruksi, cadangan pengguna pemesan.</summary>
    public class RadReferrerRow
    {
        public string ReferrerName { get; set; } = string.Empty;

        /// <summary><c>Doctor</c> bila dari dokter pemberi instruksi, <c>User</c> bila dari akun pemesan.</summary>
        public string ReferrerSource { get; set; } = string.Empty;

        public int Total { get; set; }
        public int PatientCount { get; set; }
        public int Urgent { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    /// <summary>Rincian pesanan — dipakai Pasien Perusahaan dan Pemeriksaan Per Item.</summary>
    public class RadOrderReportRow
    {
        public Guid RadOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderedAt { get; set; }
        public string? CompanyName { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public string? EncounterType { get; set; }
        public string ProcedureName { get; set; } = string.Empty;
        public string ModalityName { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public bool IsUrgent { get; set; }
    }

    /// <summary>Pemeriksaan Harian — satu baris per tanggal WIB, termasuk hari kosong.</summary>
    public class RadDailyExaminationRow
    {
        public string Date { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Outpatient { get; set; }
        public int Inpatient { get; set; }
        public int Emergency { get; set; }
        public int Urgent { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    /// <summary>Setup Tarif Tindakan — pemeriksaan Radiologi beserta tarifnya (baca saja).</summary>
    public class RadProcedureTariffRow
    {
        public Guid ProcedureId { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public bool IsActive { get; set; }
        public List<RadTariffItem> Tariffs { get; set; } = new();
    }

    public class RadTariffItem
    {
        public string TariffCode { get; set; } = string.Empty;
        public string TariffName { get; set; } = string.Empty;
        public string? PatientClassName { get; set; }
        public decimal NormalPrice { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
