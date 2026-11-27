using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    public static class WorkforcePlanningSettingStatuses
    {
        public const string Complete = "Complete";
        public const string Incomplete = "Incomplete";
        public const string NotConfigured = "NotConfigured";

        public static string Compute(int? longLeaveThresholdDays, int? overdueRequestThresholdDays)
        {
            if (longLeaveThresholdDays.HasValue && overdueRequestThresholdDays.HasValue)
            {
                return Complete;
            }

            if (longLeaveThresholdDays.HasValue || overdueRequestThresholdDays.HasValue)
            {
                return Incomplete;
            }

            return NotConfigured;
        }
    }

    /// <summary>
    /// Response DTO untuk pengaturan perencanaan tenaga kerja per rumah sakit.
    /// Sesuai kontrak API v7 Bagian 13 grup Setting (BE-HRD-031) dan perluasan BE-HRD-FIN-WFP-002.
    /// </summary>
    public sealed class WorkforcePlanningSettingResponse
    {
        public Guid? Id { get; set; }
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteCode { get; set; }
        public string? HospitalSiteName { get; set; }
        public int? LongLeaveThresholdDays { get; set; }
        public int? OverdueRequestThresholdDays { get; set; }
        public string ConfigurationStatus { get; set; } = WorkforcePlanningSettingStatuses.NotConfigured;
        public string? ChangeReason { get; set; }
        public Guid? RowVersion { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
    }

    public sealed class WorkforcePlanningSettingListQuery
    {
        public string? Search { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? ConfigurationStatus { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public sealed class WorkforcePlanningSettingSummaryResponse
    {
        public int TotalHospitalSites { get; set; }
        public int CompleteCount { get; set; }
        public int IncompleteCount { get; set; }
        public int NotConfiguredCount { get; set; }
        public int IncompleteOrNotConfiguredCount { get; set; }
    }

    public sealed class WorkforcePlanningSettingListResponse : PagedResult<WorkforcePlanningSettingResponse>
    {
        public WorkforcePlanningSettingSummaryResponse Summary { get; set; } = new();
    }

    /// <summary>
    /// Request DTO untuk pembaruan pengaturan perencanaan tenaga kerja per rumah sakit.
    /// Sesuai kontrak API v7 Bagian 13 grup Setting dan aturan VR-WFP-40, VR-WFP-41 (BE-HRD-031).
    /// </summary>
    public sealed class UpdateWorkforcePlanningSettingRequest
    {
        [Required(ErrorMessage = "ID rumah sakit wajib diisi.")]
        public Guid HospitalSiteId { get; set; }

        public int? LongLeaveThresholdDays { get; set; }

        public int? OverdueRequestThresholdDays { get; set; }

        [Required(ErrorMessage = "Tuliskan alasan perubahan pengaturan.")]
        [MaxLength(500, ErrorMessage = "Alasan perubahan pengaturan maksimal 500 karakter.")]
        public string ChangeReason { get; set; } = string.Empty;

        public Guid? RowVersion { get; set; }
    }
}
