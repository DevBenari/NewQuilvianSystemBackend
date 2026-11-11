using System;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    /// <summary>
    /// Command inisiasi pemanggilan cuti darurat (Leave Recall) oleh Kepala Ruangan / Kepala Unit (HRD-DEC-113).
    /// </summary>
    public class InitiateLeaveRecallCommand
    {
        [Required(ErrorMessage = "ID Permohonan cuti wajib diisi.")]
        public Guid LeaveRequestId { get; set; }

        [Required(ErrorMessage = "Tanggal efektif pemanggilan dinas darurat wajib diisi.")]
        public DateOnly EffectiveRecallDate { get; set; }

        [Required(ErrorMessage = "Kategori darurat rumah sakit wajib diisi.")]
        [MaxLength(50)]
        public string EmergencyCategory { get; set; } = "ClinicalSurge";
        // ClinicalSurge, MassCasualty, EpidemicOutbreak, CriticalStaffingShortage, Other

        [Required(ErrorMessage = "Alasan operasional darurat wajib diisi.")]
        [MaxLength(2000)]
        public string RecallReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command persetujuan pemanggilan cuti oleh HR Manager.
    /// </summary>
    public class ApproveLeaveRecallCommand
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Command penolakan usulan pemanggilan cuti oleh HR Manager.
    /// </summary>
    public class RejectLeaveRecallCommand
    {
        [Required(ErrorMessage = "Alasan penolakan oleh HR Manager wajib diisi.")]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO rincian tiket pemanggilan cuti darurat.
    /// </summary>
    public class LeaveRecallDto
    {
        public Guid Id { get; set; }
        public string RecallNumber { get; set; } = string.Empty;
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public DateOnly EffectiveRecallDate { get; set; }
        public decimal RestoredDays { get; set; }
        public string EmergencyCategory { get; set; } = string.Empty;
        public string RecallReason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid RequestedByHeadUnitId { get; set; }
        public string RequestedByHeadUnitName { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public Guid? ApprovedByHrManagerId { get; set; }
        public string? ApprovedByHrManagerName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    /// <summary>
    /// Hasil eksekusi pemanggilan dinas darurat dan pemulihan saldo.
    /// </summary>
    public class LeaveRecallExecutionResultDto
    {
        public Guid RecallId { get; set; }
        public string RecallNumber { get; set; } = string.Empty;
        public decimal RestoredDays { get; set; }
        public decimal? BalanceRemaining { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
