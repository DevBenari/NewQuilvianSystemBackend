using System;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Response DTO representasi satu versi formasi jabatan.
    /// Sesuai kontrak API v7 Bagian 13 grup Position Formation (BE-HRD-032).
    /// </summary>
    public sealed class PositionFormationResponse
    {
        public Guid Id { get; set; }
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public int VersionNumber { get; set; }
        public decimal FormationQuantity { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public PositionFormationSourceType SourceType { get; set; }
        public string SourceTypeName => SourceType.ToString();
        public Guid SourceDocumentId { get; set; }
        public string SourceDocumentNumber { get; set; } = string.Empty;
        public Guid ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime ApprovedAt { get; set; }
        public Guid? PreviousFormationId { get; set; }
    }

    /// <summary>
    /// Query DTO untuk pencarian formasi jabatan yang berlaku pada suatu tanggal.
    /// </summary>
    public sealed class PositionFormationPagedQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public DateOnly? Date { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
