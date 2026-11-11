using System;
using System.Collections.Generic;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Parameter pencarian potret gap berhalaman (BE-HRD-039).
    /// </summary>
    public sealed class StaffingGapSnapshotPagedQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public StaffingGapSnapshotType? SnapshotType { get; set; }
        public PositionFormationSourceType? SourceDocumentType { get; set; }
        public Guid? SourceDocumentId { get; set; }
        public DateOnly? PeriodMonth { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Item daftar potret gap tenaga kerja.
    /// </summary>
    public sealed class StaffingGapSnapshotListItemResponse
    {
        public Guid Id { get; set; }
        public StaffingGapSnapshotType SnapshotType { get; set; }
        public string SnapshotTypeName => SnapshotType.ToString();
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public DateOnly ReferenceDate { get; set; }
        public PositionFormationSourceType? SourceDocumentType { get; set; }
        public string? SourceDocumentTypeName => SourceDocumentType?.ToString();
        public Guid? SourceDocumentId { get; set; }
        public DateOnly? PeriodMonth { get; set; }
        public DateTime TakenAt { get; set; }
        public DateTime? ReconstructedAt { get; set; }
        public int TotalLines { get; set; }
    }

    /// <summary>
    /// Baris rincian potret gap per unit dan jabatan.
    /// </summary>
    public sealed class StaffingGapSnapshotLineResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public decimal? FormationQuantity { get; set; }
        public decimal? FilledFte { get; set; }
        public decimal? ToBeVacatedFte { get; set; }
        public decimal? TemporarilyUnavailableFte { get; set; }
        public decimal? NotYetCompetentFte { get; set; }
        public decimal? OutsourcedFte { get; set; }
        public int? ConcurrentAssignmentCount { get; set; }
        public decimal? PermanentGap { get; set; }
        public decimal? TemporaryGap { get; set; }
        public decimal? CompetencyGap { get; set; }
        public bool PlacementDataComplete { get; set; }
        public bool LeaveDataComplete { get; set; }
        public bool CompetencyDataComplete { get; set; }
        public bool WorkforceTypeDataComplete { get; set; }
        public bool IsComplete => PlacementDataComplete && LeaveDataComplete && CompetencyDataComplete && WorkforceTypeDataComplete;
    }

    /// <summary>
    /// Detail lengkap satu potret gap beserta seluruh baris rinciannya (BE-HRD-039).
    /// </summary>
    public sealed class StaffingGapSnapshotResponse
    {
        public Guid Id { get; set; }
        public StaffingGapSnapshotType SnapshotType { get; set; }
        public string SnapshotTypeName => SnapshotType.ToString();
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public DateOnly ReferenceDate { get; set; }
        public PositionFormationSourceType? SourceDocumentType { get; set; }
        public string? SourceDocumentTypeName => SourceDocumentType?.ToString();
        public Guid? SourceDocumentId { get; set; }
        public DateOnly? PeriodMonth { get; set; }
        public DateTime TakenAt { get; set; }
        public DateTime? ReconstructedAt { get; set; }
        public List<StaffingGapSnapshotLineResponse> Lines { get; set; } = new();
    }
}
