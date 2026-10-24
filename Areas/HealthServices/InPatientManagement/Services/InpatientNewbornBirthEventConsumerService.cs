using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>
/// Event payload kelahiran bayi dari kamar operasi IBS (BE-RWI-184, OPS-DEC-042).
/// </summary>
public class OprBirthRecordedIntegrationEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = "operating-room.birth-event.recorded";
    public string Version { get; set; } = "1.0";
    public DateTime OccurredAt { get; set; }
    public Guid CaseId { get; set; }
    public Guid BirthEventId { get; set; }
    public Guid MotherPatientId { get; set; }
    public string Gender { get; set; } = "Male";
    public decimal WeightGrams { get; set; }
    public decimal LengthCentimeters { get; set; }
    public decimal? HeadCircumferenceCm { get; set; }
    public int ApgarScore1Min { get; set; }
    public int ApgarScore5Min { get; set; }
    public int? ApgarScore10Min { get; set; }
    public string? ConditionNote { get; set; }
}

/// <summary>
/// Hasil konsumsi event kelahiran bayi intraoperatif SC (BE-RWI-184).
/// </summary>
public class NewbornBirthEventConsumerResult
{
    public bool Succeeded { get; set; }
    public bool AlreadyProcessed { get; set; }
    public Guid? BabyPatientId { get; set; }
    public string? BabyMedicalRecordNumber { get; set; }
    public Guid? BabyEpisodeId { get; set; }
    public string? BabyEpisodeNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Consumer event kelahiran intraoperatif SC untuk auto-generate rekam medis bayi & episode rawat gabung (BE-RWI-184, OPS-DEC-042, RWI-DEC-357).
/// </summary>
public class InpatientNewbornBirthEventConsumerService(
    ApplicationDbContext dbContext,
    LoggerService loggerService)
{
    public async Task<NewbornBirthEventConsumerResult> ConsumeAsync(
        OprBirthRecordedIntegrationEvent @event,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (@event.CaseId == Guid.Empty)
        {
            return new NewbornBirthEventConsumerResult
            {
                Succeeded = false,
                Message = "CaseId kasus operasi wajib diisi."
            };
        }

        // Idempotency: jika bayi untuk kasus persalinan dan event ini sudah pernah dibuat, lewati
        var existingBaby = await dbContext.Set<MstPatient>()
            .FirstOrDefaultAsync(x => x.DeliveryCaseId == @event.CaseId && !x.IsDelete, cancellationToken);

        if (existingBaby != null)
        {
            var existingEpisode = await dbContext.Set<InpEpisode>()
                .FirstOrDefaultAsync(x => x.PatientId == existingBaby.Id && !x.IsDelete, cancellationToken);

            return new NewbornBirthEventConsumerResult
            {
                Succeeded = true,
                AlreadyProcessed = true,
                BabyPatientId = existingBaby.Id,
                BabyMedicalRecordNumber = existingBaby.MedicalRecordNumber,
                BabyEpisodeId = existingEpisode?.Id,
                BabyEpisodeNumber = existingEpisode?.EpisodeNumber,
                Message = "Event kelahiran bayi ini sudah diproses sebelumnya (Idempotent)."
            };
        }

        // Cari identitas ibu dan episode rawat inap aktif sang ibu
        var motherPatient = await dbContext.Set<MstPatient>()
            .FirstOrDefaultAsync(x => x.Id == @event.MotherPatientId && !x.IsDelete, cancellationToken);

        var motherEpisode = await dbContext.Set<InpEpisode>()
            .FirstOrDefaultAsync(x => x.PatientId == @event.MotherPatientId && x.EpisodeStatus != InpEpisodeStatus.Discharged && !x.IsDelete, cancellationToken);

        var now = DateTime.UtcNow;
        var babyMrn = $"BY-{now:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var motherName = motherPatient?.Name ?? "Ibu";

        // 1. Buat profil pasien bayi baru lahir
        var babyPatient = new MstPatient
        {
            Id = Guid.NewGuid(),
            MedicalRecordNumber = babyMrn,
            Name = $"Bayi Ny. {motherName}",
            Gender = @event.Gender,
            BirthDate = @event.OccurredAt,
            IsNewborn = true,
            MotherPatientId = @event.MotherPatientId,
            DeliveryCaseId = @event.CaseId,
            BloodType = motherPatient?.BloodType ?? "Unknown",
            Citizen = motherPatient?.Citizen ?? "WNI",
            CreateDateTime = now,
            CreateBy = actorUserId,
            IsActive = true
        };
        dbContext.Set<MstPatient>().Add(babyPatient);

        // 2. Buat episode rawat inap neonatus (tertataut ke episode ibu)
        var babyEpisodeNumber = $"RWI-BY/{now:yyyyMMdd}/{Random.Shared.Next(100, 999)}";
        var babyEpisode = new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = babyEpisodeNumber,
            EncounterId = motherEpisode?.EncounterId ?? Guid.NewGuid(),
            PatientId = babyPatient.Id,
            ServiceUnitId = motherEpisode?.ServiceUnitId ?? Guid.Empty,
            PatientClassId = motherEpisode?.PatientClassId ?? Guid.Empty,
            EpisodeStatus = InpEpisodeStatus.Admitted,
            AdmittedAt = now,
            MotherEpisodeId = motherEpisode?.Id,
            IsRoomingInBabyPlanned = motherEpisode?.IsRoomingInBabyPlanned ?? true,
            ClinicalSubCategory = "ObstetricSurgery",
            Notes = $"Auto-generated kelahiran SC di OK. APGAR: {@event.ApgarScore1Min}/{@event.ApgarScore5Min}/{@event.ApgarScore10Min ?? 10}, BB: {@event.WeightGrams}g, PB: {@event.LengthCentimeters}cm.",
            CreateDateTime = now,
            CreateBy = actorUserId,
            IsActive = true
        };
        dbContext.Set<InpEpisode>().Add(babyEpisode);

        await dbContext.SaveChangesAsync(cancellationToken);

        await loggerService.InfoAsync(
            "InpatientManagement",
            "NewbornBirthEventConsumer",
            $"Berhasil membuka episode neonatus {babyEpisode.EpisodeNumber} untuk bayi Ny. {motherName}.",
            new { BabyPatientId = babyPatient.Id, BabyEpisodeId = babyEpisode.Id, CaseId = @event.CaseId });

        return new NewbornBirthEventConsumerResult
        {
            Succeeded = true,
            AlreadyProcessed = false,
            BabyPatientId = babyPatient.Id,
            BabyMedicalRecordNumber = babyPatient.MedicalRecordNumber,
            BabyEpisodeId = babyEpisode.Id,
            BabyEpisodeNumber = babyEpisode.EpisodeNumber,
            Message = "Episode rawat inap bayi baru lahir berhasil dibuka otomatis."
        };
    }
}
