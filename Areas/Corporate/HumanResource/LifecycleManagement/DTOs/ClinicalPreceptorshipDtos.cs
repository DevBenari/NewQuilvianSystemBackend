using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class ClinicalPreceptorHistoryItemResponse
    {
        public Guid Id { get; set; }
        public Guid PreceptorWorkforceProfileId { get; set; }
        public string PreceptorProfileCode { get; set; } = string.Empty;
        public string PreceptorDisplayName { get; set; } = string.Empty;
        public DateOnly AssignedFrom { get; set; }
        public DateOnly? AssignedUntil { get; set; }
        public bool IsCurrent => !AssignedUntil.HasValue;
        public string? ChangeReason { get; set; }
        public string AssignedByUserName { get; set; } = string.Empty;
    }

    public sealed class ClinicalPreceptorshipListItemResponse
    {
        public Guid Id { get; set; }
        public Guid OnboardingCaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public PreceptorshipStatus PreceptorshipStatus { get; set; }
        public string PreceptorshipStatusName => PreceptorshipStatus.ToString();
        public string CurrentPreceptorDisplayName { get; set; } = string.Empty;
        public DateOnly? AccompaniedSince { get; set; }
        public DateOnly? IndependentOn { get; set; }
    }

    public sealed class ClinicalPreceptorshipResponse
    {
        public Guid Id { get; set; }
        public Guid OnboardingCaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid OrganizationUnitId { get; set; }
        public string OrganizationUnitName { get; set; } = string.Empty;
        public PreceptorshipStatus PreceptorshipStatus { get; set; }
        public string PreceptorshipStatusName => PreceptorshipStatus.ToString();
        public DateOnly? AccompaniedSince { get; set; }
        public DateOnly? IndependentOn { get; set; }
        public string? DeclaredByUserName { get; set; }
        public string? EvidenceStoragePath { get; set; }
        public string? EvidenceFileName { get; set; }
        public string? AssessmentNote { get; set; }
        public Guid RowVersion { get; set; }
        public List<ClinicalPreceptorHistoryItemResponse> Preceptors { get; set; } = new();
    }

    public sealed class ClinicalPreceptorshipQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public PreceptorshipStatus? PreceptorshipStatus { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? Search { get; set; }
    }

    public sealed class ChangePreceptorRequest
    {
        [Required(ErrorMessage = "Preceptor baru wajib dipilih.")]
        public Guid NewPreceptorWorkforceProfileId { get; set; }

        [Required(ErrorMessage = "Alasan pergantian preceptor wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan maksimal 500 karakter.")]
        public string ChangeReason { get; set; } = string.Empty;
    }

    public sealed class DeclareClinicalIndependentRequest
    {
        [Required(ErrorMessage = "Tanggal penetapan mandiri wajib diisi.")]
        public DateOnly IndependentOn { get; set; }

        [Required(ErrorMessage = "Catatan evaluasi kelaikan klinis mandiri wajib diisi.")]
        [MaxLength(2000, ErrorMessage = "Catatan evaluasi maksimal 2000 karakter.")]
        public string AssessmentNote { get; set; } = string.Empty;

        [Required(ErrorMessage = "Berkas bukti asesmen kompetensi klinis mandiri wajib dilampirkan.")]
        public IFormFile EvidenceFile { get; set; } = null!;
    }
}
