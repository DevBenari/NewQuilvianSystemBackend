using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class CreateJobVacancyRequest
    {
        [Required(ErrorMessage = "RequisitionId wajib diisi.")]
        public Guid RequisitionId { get; set; }

        [Required(ErrorMessage = "Judul lowongan wajib diisi.")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Range(1, 1000, ErrorMessage = "QuotaSeats minimal 1.")]
        public int QuotaSeats { get; set; } = 1;

        public bool IsClinical { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? ClosingDate { get; set; }
    }

    public sealed class UpdateJobVacancyRequest
    {
        [Required(ErrorMessage = "Judul lowongan wajib diisi.")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Range(1, 1000, ErrorMessage = "QuotaSeats minimal 1.")]
        public int QuotaSeats { get; set; }

        public bool IsClinical { get; set; }

        public DateTime? ClosingDate { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class PublishJobVacancyRequest
    {
        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class CloseJobVacancyRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class JobVacancyResponse
    {
        public Guid Id { get; set; }
        public string VacancyNumber { get; set; } = string.Empty;
        public Guid RequisitionId { get; set; }
        public string? RequisitionNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int QuotaSeats { get; set; }
        public bool IsClinical { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public JobVacancyStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public Guid RowVersion { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public sealed class JobVacancyListItemResponse
    {
        public Guid Id { get; set; }
        public string VacancyNumber { get; set; } = string.Empty;
        public Guid RequisitionId { get; set; }
        public string? RequisitionNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int QuotaSeats { get; set; }
        public bool IsClinical { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public JobVacancyStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class JobVacancyQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public Guid? RequisitionId { get; set; }
        public bool? IsClinical { get; set; }
        public JobVacancyStatus? Status { get; set; }
    }
}
