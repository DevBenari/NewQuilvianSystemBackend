using System;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Response DTO untuk pengaturan perencanaan tenaga kerja per rumah sakit.
    /// Sesuai kontrak API v7 Bagian 13 grup Setting (BE-HRD-031).
    /// </summary>
    public sealed class WorkforcePlanningSettingResponse
    {
        public Guid? Id { get; set; }
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public int? LongLeaveThresholdDays { get; set; }
        public int? OverdueRequestThresholdDays { get; set; }
        public string? ChangeReason { get; set; }
        public Guid? RowVersion { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
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
        public string ChangeReason { get; set; } = string.Empty;

        public Guid? RowVersion { get; set; }
    }
}
