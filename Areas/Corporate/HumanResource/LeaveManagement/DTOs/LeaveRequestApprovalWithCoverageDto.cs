using System;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    /// <summary>
    /// Item permohonan cuti pada kotak masuk atasan dilengkapi indikator peringatan kuota pelayanan (Safe Staffing Coverage Warning).
    /// </summary>
    public class LeaveApprovalInboxItemDto
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public bool IsHalfDay { get; set; }
        public string? HalfDayPeriod { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Flag peringatan kuota pelayanan unit 24/7 (HRD-DEC-111).
        /// </summary>
        public bool HasStaffingCoverageWarning { get; set; }
        public string? CoverageWarningDetails { get; set; }

        // Metadata surat dokter (jika cuti sakit)
        public bool RequiresMedicalCertificate { get; set; }
        public bool HasMedicalAttachment { get; set; }
        public bool IsDoctorNoteVerified { get; set; }
        public string? DoctorNoteFaskesName { get; set; }
        public string? DoctorSipNumber { get; set; }
    }

    /// <summary>
    /// Command persetujuan cuti oleh atasan langsung (dengan catatan mitigasi jika warning).
    /// </summary>
    public class ApproveLeaveRequestCommand
    {
        /// <summary>
        /// Catatan mitigasi penugasan perawat/staf pengganti. Wajib jika HasStaffingCoverageWarning = true (VR-LEV-21).
        /// </summary>
        [MaxLength(500)]
        public string? MitigationNotes { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }
    }

    /// <summary>
    /// Command penolakan cuti oleh atasan langsung.
    /// </summary>
    public class RejectLeaveRequestCommand
    {
        [Required(ErrorMessage = "Alasan penolakan operasional wajib diisi.")]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command permintaan revisi tanggal/durasi cuti kepada pemohon.
    /// </summary>
    public class ReviseLeaveRequestCommand
    {
        [Required(ErrorMessage = "Catatan revisi wajib diisi.")]
        [MaxLength(1000)]
        public string RevisionNotes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO hasil aksi workflow approval cuti.
    /// </summary>
    public class LeaveApprovalResultDto
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool HasStaffingCoverageWarning { get; set; }
        public string? MitigationNotes { get; set; }
        public decimal? BalanceRemaining { get; set; }
        public bool IsPendingPayrollReconciliation { get; set; }
        public DateTime ActionAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
