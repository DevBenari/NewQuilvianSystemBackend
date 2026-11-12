using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class CreateJobRequisitionRequest
    {
        [Required(ErrorMessage = "HeadcountRequestId wajib diisi.")]
        public Guid HeadcountRequestId { get; set; }

        [Required(ErrorMessage = "HospitalSiteId wajib diisi.")]
        public Guid HospitalSiteId { get; set; }

        [Required(ErrorMessage = "OrganizationUnitId wajib diisi.")]
        public Guid OrganizationUnitId { get; set; }

        [Required(ErrorMessage = "PositionId wajib diisi.")]
        public Guid PositionId { get; set; }

        [Range(1, 1000, ErrorMessage = "RequestedSeats minimal 1.")]
        public int RequestedSeats { get; set; } = 1;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class UpdateJobRequisitionRequest
    {
        [Range(1, 1000, ErrorMessage = "RequestedSeats minimal 1.")]
        public int RequestedSeats { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class SubmitJobRequisitionRequest
    {
        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class CancelJobRequisitionRequest
    {
        [Required(ErrorMessage = "Alasan pembatalan wajib diisi.")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class JobRequisitionResponse
    {
        public Guid Id { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public Guid HeadcountRequestId { get; set; }
        public string? HeadcountRequestNumber { get; set; }
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionName { get; set; }
        public int RequestedSeats { get; set; }
        public int ApprovedSeats { get; set; }
        public int FulfilledSeats { get; set; }
        public JobRequisitionStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? Notes { get; set; }
        public Guid RowVersion { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public sealed class JobRequisitionListItemResponse
    {
        public Guid Id { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public Guid HeadcountRequestId { get; set; }
        public string? HeadcountRequestNumber { get; set; }
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionName { get; set; }
        public int RequestedSeats { get; set; }
        public int ApprovedSeats { get; set; }
        public int FulfilledSeats { get; set; }
        public JobRequisitionStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class JobRequisitionQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public JobRequisitionStatus? Status { get; set; }
    }
}
