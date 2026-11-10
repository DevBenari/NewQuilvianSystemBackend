using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    /// <summary>
    /// DTO surat perintah penugasan dinas luar rumah sakit (ambulans rujukan, kurir lab, workshop, home care) (BE-HRD-109, HRD-DEC-109).
    /// </summary>
    public class CreateOfficialDutyRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        /// <summary>
        /// Kategori dinas: AmbulanceReferral, LabCourier, TrainingSeminar, HomeCare, Other
        /// </summary>
        [Required, MaxLength(50)]
        public string DutyCategory { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string AssignmentOrderNumber { get; set; } = string.Empty;

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [Required, MaxLength(255)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }
    }

    public class UpdateOfficialDutyRequest
    {
        [Required, MaxLength(50)]
        public string DutyCategory { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string AssignmentOrderNumber { get; set; } = string.Empty;

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [Required, MaxLength(255)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }
    }

    public class ApproveOfficialDutyRequest
    {
        [Required]
        public Guid ApproverWorkforceProfileId { get; set; }

        [MaxLength(500)]
        public string? ApprovalNote { get; set; }
    }

    public class RejectOfficialDutyRequest
    {
        [Required]
        public Guid ApproverWorkforceProfileId { get; set; }

        [Required, MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class OfficialDutyQueryRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? WorkforceProfileId { get; set; }
        public string? DutyCategory { get; set; }
        public string? Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class OfficialDutyResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNik { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string DutyCategory { get; set; } = string.Empty;
        public string AssignmentOrderNumber { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? AttachmentUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? ApprovedByWorkforceId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreateDateTime { get; set; }
    }

    public class OfficialDutyPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<OfficialDutyResponse> Items { get; set; } = new();
    }
}
