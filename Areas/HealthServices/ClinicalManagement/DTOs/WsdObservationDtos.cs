using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;

public class RegisterWsdDrainRequest
{
    public Guid EpisodeId { get; set; }
    [Required, MaxLength(50)] public string DrainLabel { get; set; } = string.Empty;
    [MaxLength(100)] public string? InsertionSite { get; set; }
    public DateTime InsertedAt { get; set; }
    [Range(0, 9999999)] public decimal? InitialResidualMl { get; set; }
}
public class CorrectWsdDrainRequest
{
    [Required, MaxLength(50)] public string DrainLabel { get; set; } = string.Empty;
    [MaxLength(100)] public string? InsertionSite { get; set; }
    public DateTime InsertedAt { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public int ExpectedVersion { get; set; }
}
public class RemoveWsdDrainRequest { public DateTime RemovedAt { get; set; } }
public class RecordWsdReadingRequest
{
    public DateTime PeriodStartAt { get; set; }
    public DateTime PeriodEndAt { get; set; }
    [Range(0, 9999999)] public decimal CurrentResidualMl { get; set; }
    [Range(0, 9999999)] public decimal? DiscardedVolumeMl { get; set; }
    public Guid? ShiftId { get; set; }
}
public class CorrectWsdReadingRequest
{
    [Range(0, 9999999)] public decimal CurrentResidualMl { get; set; }
    [Range(0, 9999999)] public decimal? DiscardedVolumeMl { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}
public class WsdDrainResponse
{
    public Guid Id { get; set; }
    public Guid EpisodeId { get; set; }
    public string DrainLabel { get; set; } = string.Empty;
    public string? InsertionSite { get; set; }
    public DateTime InsertedAt { get; set; }
    public decimal InitialResidualMl { get; set; }
    public DateTime? RemovedAt { get; set; }
    public CliWsdDrainStatus Status { get; set; }
    public int Version { get; set; }
    public WsdReadingResponse? LatestReading { get; set; }
}
public class WsdReadingResponse
{
    public Guid Id { get; set; }
    public DateTime PeriodStartAt { get; set; }
    public DateTime PeriodEndAt { get; set; }
    public decimal PreviousResidualMl { get; set; }
    public decimal CurrentResidualMl { get; set; }
    public decimal DiscardedVolumeMl { get; set; }
    public decimal IncreaseMl { get; set; }
    public Guid FluidBalanceEntryId { get; set; }
    public ClinicalMeasurementStatus Status { get; set; }
    public int RevisionNumber { get; set; }
    public string? RecordedByName { get; set; }
}
