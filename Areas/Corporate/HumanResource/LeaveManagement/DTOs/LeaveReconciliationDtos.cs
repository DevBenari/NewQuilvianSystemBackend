using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs
{
    /// <summary>
    /// DTO cuti pending yang akan direkonsiliasi retrospektif ke payroll (HRD-DEC-114, LEV-W5, INT-HR-48).
    /// </summary>
    public class PendingLeavePayrollDto
    {
        public Guid LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool IsPendingPayrollReconciliation { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    /// <summary>
    /// Command penutupan periode cuti pra-payroll (penguncian immutabilitas IsPayrollClosed = true).
    /// </summary>
    public class CloseLeavePeriodCommand
    {
        [Required]
        [Range(2020, 2050)]
        public int PeriodYear { get; set; }

        [Required]
        [Range(1, 12)]
        public int PeriodMonth { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Hasil eksekusi penutupan periode cuti dan serah terima snapshot payroll.
    /// </summary>
    public class CloseLeavePeriodResultDto
    {
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public int TotalClosedRequests { get; set; }
        public int TotalPendingReconciledRequests { get; set; }
        public DateTime ClosedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command pembalikan eksekusi cuti yang dilindungi guard HTTP 409 Conflict.
    /// </summary>
    public class ReverseExecutionCommand
    {
        [Required(ErrorMessage = "Alasan pembalikan cuti wajib diisi.")]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}
