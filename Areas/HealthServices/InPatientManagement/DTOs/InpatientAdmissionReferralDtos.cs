using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Saringan daftar permintaan admisi dari kamar pulih (<c>BE-RWI-181</c>, API 11.6).
    /// </summary>
    public class AdmissionReferralQuery
    {
        /// <summary>Bawaan <c>Pending</c> bila dikosongkan.</summary>
        public InpAdmissionReferralStatus? Status { get; set; }

        /// <summary>
        /// Hanya permintaan <c>Pending</c> yang menunggu lebih lama dari
        /// <c>MstInpatientSetting.PendingAdmissionReferralAlertMinutes</c>.
        /// </summary>
        public bool? OverdueOnly { get; set; }

        /// <summary>Nama pasien, No. RM, atau nomor kasus OK.</summary>
        [MaxLength(100)]
        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Satu permintaan admisi (API 11.6). Tidak memuat rupiah.</summary>
    public class AdmissionReferralResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? MedicalRecordNumber { get; set; }
        public Guid SourceEncounterId { get; set; }

        /// <summary><c>Outpatient</c>, <c>Emergency</c>, <c>OneDayCare</c>, atau jenis kunjungan lain apa adanya.</summary>
        public string? SourceEncounterType { get; set; }

        public Guid OprCaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public List<string> ProcedureNames { get; set; } = [];
        public Guid PrimarySurgeonId { get; set; }
        public string? PrimarySurgeonName { get; set; }
        public InpRequestedCareLevel RequestedCareLevel { get; set; }
        public string? RecoveryDecisionNote { get; set; }
        public InpAdmissionReferralStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public int WaitingMinutes { get; set; }
        public bool IsOverdue { get; set; }
        public string? CancelledReason { get; set; }
        public Guid? CompletedEpisodeId { get; set; }
        public Guid RowVersion { get; set; }
    }
}
