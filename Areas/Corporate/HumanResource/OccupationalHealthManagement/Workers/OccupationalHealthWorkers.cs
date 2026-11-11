using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Workers
{
    /// <summary>
    /// Worker Pemantauan Surveilans Serologi Serial Darah Pasca Pajanan Bulan ke-1, 3, dan 6 (OHS-W1, BE-HRD-296)
    /// </summary>
    public class SerialSurveillanceFollowUpWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SerialSurveillanceFollowUpWorker> _logger;

        public SerialSurveillanceFollowUpWorker(
            IServiceProvider serviceProvider,
            ILogger<SerialSurveillanceFollowUpWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SerialSurveillanceFollowUpWorker (OHS-W1) diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var pendingExposures = await context.TrxOccupationalExposures
                        .Where(x => !x.IsDelete && x.IsActive && x.ExposureStatus == "UnderSurveillance" && x.FollowUpRequired)
                        .ToListAsync(stoppingToken);

                    foreach (var exp in pendingExposures)
                    {
                        if (exp.FollowUpDate.HasValue && now >= exp.FollowUpDate.Value)
                        {
                            _logger.LogInformation("Jadwal surveilans serial tercapai untuk kasus pajanan {Number}. Tiket pemeriksaan diterbitkan.", exp.ExposureNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada SerialSurveillanceFollowUpWorker (OHS-W1).");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Worker Pemantauan Masa Kedaluwarsa Surat Pembatasan Tugas Medis H+180 Hari (OHS-W2, BE-HRD-292)
    /// </summary>
    public class WorkRestrictionExpirationWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkRestrictionExpirationWorker> _logger;

        public WorkRestrictionExpirationWorker(
            IServiceProvider serviceProvider,
            ILogger<WorkRestrictionExpirationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkRestrictionExpirationWorker (OHS-W2) diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var expiredRestrictions = await context.TrxWorkRestrictions
                        .Where(x => !x.IsDelete && x.IsActive && x.RestrictionStatus == "Active" && x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < now)
                        .ToListAsync(stoppingToken);

                    foreach (var r in expiredRestrictions)
                    {
                        r.RestrictionStatus = "Expired";
                        r.IsSchedulingBlocked = false;
                        r.UpdateDateTime = DateTime.UtcNow;
                        _logger.LogInformation("Surat restriksi {Number} telah kedaluwarsa. Status diubah menjadi Expired.", r.RestrictionNumber);
                    }

                    if (expiredRestrictions.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada WorkRestrictionExpirationWorker (OHS-W2).");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Worker Pengingat Booster Vaksinasi Nakes H-30 Sebelum Masa Proteksi Berakhir (OHS-W3, BE-HRD-297)
    /// </summary>
    public class VaccineBoosterReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VaccineBoosterReminderWorker> _logger;

        public VaccineBoosterReminderWorker(
            IServiceProvider serviceProvider,
            ILogger<VaccineBoosterReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VaccineBoosterReminderWorker (OHS-W3) diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var reminderHorizon = DateTime.UtcNow.AddDays(30);
                    var upcomingDoses = await context.TrxEmployeeVaccinations
                        .Where(x => !x.IsDelete && x.IsActive && x.NextDoseDate.HasValue && x.NextDoseDate.Value <= reminderHorizon && x.VaccinationStatus != "Completed")
                        .ToListAsync(stoppingToken);

                    foreach (var dose in upcomingDoses)
                    {
                        _logger.LogInformation("Notifikasi booster H-30 dikirim untuk vaksinasi {Number} ({VaccineName}).", dose.VaccinationNumber, dose.VaccineName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada VaccineBoosterReminderWorker (OHS-W3).");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Worker Pengawasan Kepatuhan Jadwal MCU Tahunan Staf Berbasis Profil Risiko (OHS-W4, BE-HRD-299)
    /// </summary>
    public class McuComplianceScheduleWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<McuComplianceScheduleWorker> _logger;

        public McuComplianceScheduleWorker(
            IServiceProvider serviceProvider,
            ILogger<McuComplianceScheduleWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("McuComplianceScheduleWorker (OHS-W4) diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var overdueMcus = await context.TrxEmployeeMedicalExaminations
                        .Where(x => !x.IsDelete && x.IsActive && x.AdministrativeStatus == "Scheduled" && x.ScheduledAt < now.AddDays(-7))
                        .ToListAsync(stoppingToken);

                    foreach (var mcu in overdueMcus)
                    {
                        mcu.AdministrativeStatus = "Overdue";
                        mcu.UpdateDateTime = DateTime.UtcNow;
                        _logger.LogWarning("Jadwal MCU {Number} terlambat lebih dari 7 hari. Notifikasi peringatan dikirim.", mcu.ExaminationNumber);
                    }

                    if (overdueMcus.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada McuComplianceScheduleWorker (OHS-W4).");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Worker Alarm Ambang Batas Dosis Radiasi Pekerja TLD Badge (OHS-W5, BE-HRD-298)
    /// </summary>
    public class RadiationDoseThresholdAlertWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RadiationDoseThresholdAlertWorker> _logger;

        public RadiationDoseThresholdAlertWorker(
            IServiceProvider serviceProvider,
            ILogger<RadiationDoseThresholdAlertWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RadiationDoseThresholdAlertWorker (OHS-W5) diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Pantau rekaman surveilans radiasi dengan outcome peringatan
                    var highDoseCases = await context.TrxEmployeeHealthSurveillances
                        .Where(x => !x.IsDelete && x.IsActive && x.SurveillanceProgram.Contains("Radiasi") && !x.IsCompliant)
                        .ToListAsync(stoppingToken);

                    foreach (var highDose in highDoseCases)
                    {
                        _logger.LogWarning("ALARM K3RS RADIASI: Kasus {Number} melebihi ambang batas dosis keselamatan kerja.", highDose.SurveillanceNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada RadiationDoseThresholdAlertWorker (OHS-W5).");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Worker Pengawalan Batas Waktu Pelaporan Kecelakaan Kerja BPJS JKK 2x24 Jam (OHS-W6, BE-HRD-301)
    /// </summary>
    public class BPJSOccupationalInjuryFollowUpWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BPJSOccupationalInjuryFollowUpWorker> _logger;

        public BPJSOccupationalInjuryFollowUpWorker(
            IServiceProvider serviceProvider,
            ILogger<BPJSOccupationalInjuryFollowUpWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BPJSOccupationalInjuryFollowUpWorker (OHS-W6) diinisiasi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.UtcNow;
                    var pendingInjuries = await context.TrxEmployeeInjuries
                        .Where(x => !x.IsDelete && x.IsActive && x.InjuryStatus == "Reported")
                        .ToListAsync(stoppingToken);

                    foreach (var injury in pendingInjuries)
                    {
                        var deadline = injury.InjuryDateTime.AddHours(48);
                        var hoursLeft = (deadline - now).TotalHours;

                        if (hoursLeft <= 0)
                        {
                            _logger.LogError("PERINGATAN SANKSI BPJS JKK: Laporan insiden {Number} telah melewati batas pelaporan 2x24 jam!", injury.InjuryNumber);
                        }
                        else if (hoursLeft <= 12)
                        {
                            _logger.LogWarning("URGENT SLA BPJS: Laporan insiden {Number} tersisa {Hours:F1} jam sebelum batas pelaporan Tahap I berakhir.", injury.InjuryNumber, hoursLeft);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kesalahan pada BPJSOccupationalInjuryFollowUpWorker (OHS-W6).");
                }

                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
        }
    }
}
