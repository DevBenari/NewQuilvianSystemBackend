using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.HealthServices.NutritionManagement;

/// <summary>
/// Dua pasien rawat inap — satu masih dirawat, satu sudah pulang — beserta master gizi
/// minimum. Dipakai bersama seluruh pengujian Phase 1 modul Gizi.
/// </summary>
/// <remarks>
/// Pasien yang sudah pulang sengaja ikut disiapkan, karena justru itulah yang membuktikan
/// daftar pasien gizi menyaring dengan benar.
/// </remarks>
internal sealed class NutritionTestContext : IAsyncDisposable
{
    public required ApplicationDbContext Context { get; init; }
    public required MutableHttpContextAccessor Accessor { get; init; }
    public required LoggerService Logger { get; init; }

    public required Guid ActivePatientId { get; init; }
    public required Guid ActiveEncounterId { get; init; }
    public required Guid DischargedPatientId { get; init; }
    public required Guid DischargedEncounterId { get; init; }

    public required Guid DietTypeRegularId { get; init; }
    public required Guid DietTypeDiabetesId { get; init; }
    public required Guid FoodFormRegularId { get; init; }
    public required Guid FoodFormSoftId { get; init; }
    public required Guid MealScheduleId { get; init; }
    public required Guid WorkforceId { get; init; }
    public required Guid UserId { get; init; }

    public static async Task<NutritionTestContext> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"nutrition-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var activePatientId = Guid.NewGuid();
        var activeEncounterId = Guid.NewGuid();
        var dischargedPatientId = Guid.NewGuid();
        var dischargedEncounterId = Guid.NewGuid();
        var workforceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var bedId = Guid.NewGuid();
        var serviceUnitId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();

        context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = workforceId, ProfileCode = "WF-GZ", DisplayName = "Ahli Gizi Uji", IsActive = true
        });

        context.Set<MstDoctor>().Add(new MstDoctor
        {
            Id = doctorId, WorkforceProfileId = workforceId, DoctorCode = "DR-GZ",
            DoctorNumber = "SIP-GZ", FullName = "dr. Uji", IsActive = true
        });

        context.Set<MstRoom>().Add(new MstRoom
        {
            Id = roomId, ServiceUnitId = serviceUnitId, RoomCode = "R1",
            RoomName = "Melati 1", IsActive = true
        });

        context.Set<MstBed>().Add(new MstBed
        {
            Id = bedId, RoomId = roomId, BedCode = "B1", BedName = "Bed 1", IsActive = true
        });

        foreach (var (patientId, encounterId, code) in new[]
        {
            (activePatientId, activeEncounterId, "AKTIF"),
            (dischargedPatientId, dischargedEncounterId, "PULANG")
        })
        {
            context.Set<MstPatient>().Add(new MstPatient
            {
                Id = patientId, PatientCode = $"PT-{code}", MedicalRecordNumber = $"RM-{code}",
                FullName = $"Pasien {code}", IsActive = true
            });

            context.Set<TrxPatientEncounter>().Add(new TrxPatientEncounter
            {
                Id = encounterId, EncounterNumber = $"ENC-{code}", PatientId = patientId,
                ServiceUnitId = serviceUnitId, EncounterDate = DateTime.UtcNow,
                RegisteredByUserId = userId, IsActive = true
            });
        }

        var activeEpisodeId = Guid.NewGuid();

        context.Set<InpEpisode>().Add(new InpEpisode
        {
            Id = activeEpisodeId, EpisodeNumber = "EP-AKTIF", EncounterId = activeEncounterId,
            PatientId = activePatientId, ServiceUnitId = serviceUnitId,
            PatientClassId = Guid.NewGuid(), EpisodeStatus = InpEpisodeStatus.Admitted,
            AdmittedAt = DateTime.UtcNow.AddDays(-1), IsActive = true
        });

        context.Set<InpEpisode>().Add(new InpEpisode
        {
            Id = Guid.NewGuid(), EpisodeNumber = "EP-PULANG", EncounterId = dischargedEncounterId,
            PatientId = dischargedPatientId, ServiceUnitId = serviceUnitId,
            PatientClassId = Guid.NewGuid(), EpisodeStatus = InpEpisodeStatus.Closed,
            AdmittedAt = DateTime.UtcNow.AddDays(-5), ClosedAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true
        });

        context.Set<InpBedPlacement>().Add(new InpBedPlacement
        {
            Id = Guid.NewGuid(), EpisodeId = activeEpisodeId, BedId = bedId, RoomId = roomId,
            ServiceUnitId = serviceUnitId, PatientClassId = Guid.NewGuid(), SequenceNumber = 1,
            StartDateTime = DateTime.UtcNow.AddDays(-1), PlacedByUserId = userId, IsActive = true
        });

        context.Set<InpDoctorAssignment>().Add(new InpDoctorAssignment
        {
            Id = Guid.NewGuid(), EpisodeId = activeEpisodeId, DoctorId = doctorId,
            SequenceNumber = 1, StartDateTime = DateTime.UtcNow.AddDays(-1),
            AssignedByUserId = userId, IsActive = true
        });

        var dietRegularId = Guid.NewGuid();
        var dietDiabetesId = Guid.NewGuid();
        var formRegularId = Guid.NewGuid();
        var formSoftId = Guid.NewGuid();
        var mealScheduleId = Guid.NewGuid();

        context.GzDietTypes.AddRange(
            new GzDietType { Id = dietRegularId, DietTypeCode = "DT1", DietTypeName = "Diet Biasa", IsActive = true },
            new GzDietType { Id = dietDiabetesId, DietTypeCode = "DT2", DietTypeName = "Diet Diabetes", IsActive = true });

        context.GzFoodForms.AddRange(
            new GzFoodForm { Id = formRegularId, FoodFormCode = "FF1", FoodFormName = "Biasa", IsActive = true },
            new GzFoodForm { Id = formSoftId, FoodFormCode = "FF2", FoodFormName = "Lunak", IsActive = true });

        context.GzMealSchedules.Add(new GzMealSchedule
        {
            Id = mealScheduleId, MealScheduleCode = "MS1", MealScheduleName = "Makan Siang",
            ServingTime = new TimeOnly(12, 0), IsActive = true
        });

        await context.SaveChangesAsync();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(userId);

        return new NutritionTestContext
        {
            Context = context,
            Accessor = accessor,
            Logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor),
            ActivePatientId = activePatientId,
            ActiveEncounterId = activeEncounterId,
            DischargedPatientId = dischargedPatientId,
            DischargedEncounterId = dischargedEncounterId,
            DietTypeRegularId = dietRegularId,
            DietTypeDiabetesId = dietDiabetesId,
            FoodFormRegularId = formRegularId,
            FoodFormSoftId = formSoftId,
            MealScheduleId = mealScheduleId,
            WorkforceId = workforceId,
            UserId = userId
        };
    }

    public async ValueTask DisposeAsync() => await Context.DisposeAsync();
}

/// <summary>Pengakses HttpContext yang penggunanya dapat diganti di tengah pengujian.</summary>
internal sealed class MutableHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = new DefaultHttpContext();

    public void SetUser(Guid userId)
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Test"))
        };
    }
}
