using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Handoff billing sesi — api-contract v1 grup Rehab Billing Handoff (BE-RHB-013).
    // =====================================================================

    /// <summary>Status penyerahan tindakan satu sesi ke Billing, beserta rincian per intervensi.</summary>
    public class RehabBillingHandoffResponse
    {
        public Guid SessionId { get; set; }
        public string SessionNumber { get; set; } = string.Empty;
        public RehabSessionStatus SessionStatus { get; set; }
        public string SessionStatusName { get; set; } = string.Empty;
        public RehabRecordStatus RecordStatus { get; set; }
        public string RecordStatusName { get; set; } = string.Empty;
        public RehabBillingHandoffStatus BillingHandoffStatus { get; set; }
        public string BillingHandoffStatusName { get; set; } = string.Empty;
        public DateTime? BillingHandoffAt { get; set; }

        /// <summary>Kode penolakan dan jumlah tindakan saja; tidak memuat data klinis (AC 5).</summary>
        public string? BillingHandoffError { get; set; }

        public List<RehabBillingHandoffItem> Items { get; set; } = new();
    }

    /// <summary>Satu intervensi sesi beserta tindakan pasien bertarif hasil handoff.</summary>
    public class RehabBillingHandoffItem
    {
        public Guid InterventionId { get; set; }
        public Guid ProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public string? ProcedureName { get; set; }
        public int Quantity { get; set; }

        /// <summary><c>TrxPatientProcedure</c>; kosong sampai sesi ditandatangani (atau di-backfill lewat retry).</summary>
        public Guid? PatientProcedureId { get; set; }

        public bool? IsBillable { get; set; }

        /// <summary>Penanda dari ClinicalManagement/Billing bahwa item tagihan sudah terbentuk.</summary>
        public bool? IsBillingGenerated { get; set; }
    }
}
