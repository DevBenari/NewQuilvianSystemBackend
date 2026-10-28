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

        // Dinetralkan 25-10-2026 atas persetujuan pemilik (laporan FIX-EPS-040-BE). Draf commit 985a1fc7
        // tidak dapat di-build (memakai MstPatient.Name/Citizen dan InpEpisodeStatus.Discharged yang tidak
        // ada), dan bila hanya nama field-nya dibetulkan, draf itu akan membuat No. RM acak di luar deret
        // nomor resmi (QBE-CODE-003) serta episode berunit layanan kosong. Admisi bayi tetap lewat layar
        // Admisi Bayi Baru Lahir sampai consumer BE-RWI-184 dirancang ulang memakai
        // NewbornPatientRegistrationService dan alur episode resmi. Draf aslinya ada di riwayat Git.
        await Task.CompletedTask;
        return new NewbornBirthEventConsumerResult
        {
            Succeeded = false,
            Message = "Consumer kelahiran SC (BE-RWI-184) belum aktif. Daftarkan bayi lewat layar Admisi Bayi Baru Lahir."
        };
    }
}
