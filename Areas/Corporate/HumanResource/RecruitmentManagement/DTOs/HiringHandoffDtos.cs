using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class GenerateHiringHandoffRequest
    {
        [Required(ErrorMessage = "ID Surat Penawaran Kerja (OfferId) wajib diisi.")]
        public Guid OfferId { get; set; }

        public Guid? HeadcountRequestSeatId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class ConfirmHiringHandoffRequest
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class HiringHandoffResponse
    {
        public Guid Id { get; set; }
        public string HandoffNumber { get; set; } = string.Empty;
        public Guid OfferId { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateNik { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public Guid? WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public Guid? HeadcountRequestSeatId { get; set; }
        public int? SeatNumber { get; set; }
        public DateTime HandoffDate { get; set; }
        public HandoffStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class HiringHandoffListItemResponse
    {
        public Guid Id { get; set; }
        public string HandoffNumber { get; set; } = string.Empty;
        public string OfferNumber { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public string? WorkforceProfileCode { get; set; }
        public DateTime HandoffDate { get; set; }
        public HandoffStatus Status { get; set; }
        public string StatusName => Status.ToString();
    }

    public sealed class HiringHandoffQuery
    {
        public HandoffStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
