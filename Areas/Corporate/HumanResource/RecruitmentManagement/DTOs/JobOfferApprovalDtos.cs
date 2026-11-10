using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class ApproveJobOfferRequest
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class RejectJobOfferRequest
    {
        [Required(ErrorMessage = "Alasan penolakan wajib diisi.")]
        [MinLength(5, ErrorMessage = "Alasan penolakan minimal 5 karakter.")]
        [MaxLength(500, ErrorMessage = "Alasan penolakan maksimal 500 karakter.")]
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class JobOfferApprovalResponse
    {
        public Guid Id { get; set; }
        public Guid OfferId { get; set; }
        public int StepNumber { get; set; }
        public string ApproverRole { get; set; } = string.Empty;
        public Guid? ApproverUserId { get; set; }
        public string? ApproverName { get; set; }
        public ApprovalStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime? DecisionDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class JobOfferApprovalHistoryResponse
    {
        public Guid OfferId { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public JobOfferApprovalTier ApprovalTier { get; set; }
        public string ApprovalTierName => ApprovalTier.ToString();
        public JobOfferStatus CurrentOfferStatus { get; set; }
        public string CurrentOfferStatusName => CurrentOfferStatus.ToString();
        public List<JobOfferApprovalResponse> Approvals { get; set; } = new();
    }
}
