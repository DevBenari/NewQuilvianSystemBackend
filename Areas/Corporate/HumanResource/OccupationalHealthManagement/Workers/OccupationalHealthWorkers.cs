using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Workers
{
    // Remediasi Gelombang 2 (10 November 2026, kelompok D backend-unfinished-work.md):
    // OHS-W1, OHS-W3, OHS-W5, dan OHS-W6 sebelumnya hanya menulis log. Kini temuan diteruskan ke pusat
    // pemberitahuan dalam aplikasi (IHrdNotificationService) — ke pegawai yang bersangkutan dan ke petugas K3RS
    // yang hak aksesnya ditentukan admin lewat layar Akses Role — dengan kunci anti-duplikat.

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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var now = DateTime.UtcNow;
                    var pendingExposures = await context.TrxOccupationalExposures
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.ExposureStatus == "UnderSurveillance" && x.FollowUpRequired
                                    && x.FollowUpDate.HasValue && x.FollowUpDate.Value <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var exp in pendingExposures)
                    {
                        var request = new HrdNotificationRequest(
                            Title: $"Jadwal pemeriksaan serologi serial kasus pajanan {exp.ExposureNumber}",
                            Body: $"Jadwal surveilans serologi serial untuk kasus pajanan {exp.ExposureNumber} jatuh pada {exp.FollowUpDate:dd MMMM yyyy}. Mohon jadwalkan pengambilan darah dan catat hasilnya pada rekam surveilans.",
                            Category: HrdNotificationCategories.OccupationalHealth,
                            Priority: HrdNotificationPriorities.High,
                            DeepLinkUrl: $"/hrd/occupational-health/exposures/{exp.Id}",
                            SourceModule: "Hrd.OccupationalHealth.OHS-W1",
                            ReferenceType: "TrxOccupationalExposure",
                            ReferenceId: exp.Id,
                            DedupKey: $"OHS-W1|{exp.Id:N}|{exp.FollowUpDate:yyyyMMdd}");

                        var sentStaff = await notifier.NotifyPermissionHoldersAsync("NeedleStickEmergency", "ManageSerialSurveillance", request, stoppingToken);
                        var sentEmployee = await notifier.NotifyWorkforceProfileAsync(exp.WorkforceProfileId, request, stoppingToken);

                        if (sentStaff + sentEmployee > 0)
                        {
                            _logger.LogInformation("OHS-W1: Jadwal serologi serial {Number} dikirim ke {Staff} petugas dan {Emp} akun pegawai.", exp.ExposureNumber, sentStaff, sentEmployee);
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var now = DateTime.UtcNow;
                    var reminderHorizon = now.AddDays(30);
                    var upcomingDoses = await context.TrxEmployeeVaccinations
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.NextDoseDate.HasValue && x.NextDoseDate.Value <= reminderHorizon && x.VaccinationStatus != "Completed")
                        .ToListAsync(stoppingToken);

                    foreach (var dose in upcomingDoses)
                    {
                        var daysLeft = (int)(dose.NextDoseDate!.Value.Date - now.Date).TotalDays;
                        var isOverdue = daysLeft < 0;
                        var request = new HrdNotificationRequest(
                            Title: isOverdue
                                ? $"Booster {dose.VaccineName} sudah terlewat"
                                : $"Booster {dose.VaccineName} dijadwalkan {daysLeft} hari lagi",
                            Body: isOverdue
                                ? $"Dosis lanjutan vaksin {dose.VaccineName} (nomor {dose.VaccinationNumber}) seharusnya diberikan pada {dose.NextDoseDate:dd MMMM yyyy} dan belum tercatat. Mohon segera jadwalkan agar perlindungan imunisasi tidak terputus."
                                : $"Dosis lanjutan vaksin {dose.VaccineName} (nomor {dose.VaccinationNumber}) jatuh tempo pada {dose.NextDoseDate:dd MMMM yyyy}. Mohon jadwalkan kunjungan ke klinik K3RS.",
                            Category: HrdNotificationCategories.OccupationalHealth,
                            Priority: isOverdue ? HrdNotificationPriorities.High : HrdNotificationPriorities.Normal,
                            DeepLinkUrl: $"/hrd/occupational-health/vaccinations/{dose.Id}",
                            SourceModule: "Hrd.OccupationalHealth.OHS-W3",
                            ReferenceType: "TrxEmployeeVaccination",
                            ReferenceId: dose.Id,
                            DedupKey: $"OHS-W3|{dose.Id:N}|{dose.NextDoseDate:yyyyMMdd}|{(isOverdue ? "OVERDUE" : "H30")}");

                        var sentEmployee = await notifier.NotifyWorkforceProfileAsync(dose.WorkforceProfileId, request, stoppingToken);
                        var sentStaff = await notifier.NotifyPermissionHoldersAsync("StaffVaccination", "RecordVaccination", request, stoppingToken);

                        if (sentEmployee + sentStaff > 0)
                        {
                            _logger.LogInformation("OHS-W3: Pengingat booster {Number} ({VaccineName}) dikirim ke {Emp} akun pegawai dan {Staff} petugas.", dose.VaccinationNumber, dose.VaccineName, sentEmployee, sentStaff);
                        }
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
                        _logger.LogWarning("Jadwal MCU {Number} terlambat lebih dari 7 hari. Status diubah menjadi Overdue.", mcu.ExaminationNumber);
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    // Pantau rekaman surveilans radiasi dengan outcome peringatan
                    var highDoseCases = await context.TrxEmployeeHealthSurveillances
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.SurveillanceProgram.Contains("Radiasi") && !x.IsCompliant)
                        .ToListAsync(stoppingToken);

                    foreach (var highDose in highDoseCases)
                    {
                        var doseText = highDose.MeasuredDoseMsv.HasValue ? $"{highDose.MeasuredDoseMsv.Value:0.###} mSv" : "dosis tidak tercatat";
                        var request = new HrdNotificationRequest(
                            Title: $"Alarm dosis radiasi: kasus {highDose.SurveillanceNumber}",
                            Body: $"Rekaman surveilans radiasi {highDose.SurveillanceNumber} ({doseText}) dinyatakan tidak memenuhi ambang keselamatan kerja. Mohon tinjau rotasi tugas pekerja radiasi dan tindak lanjut medis K3RS.",
                            Category: HrdNotificationCategories.OccupationalHealth,
                            Priority: HrdNotificationPriorities.Critical,
                            DeepLinkUrl: $"/hrd/occupational-health/surveillance/{highDose.Id}",
                            SourceModule: "Hrd.OccupationalHealth.OHS-W5",
                            ReferenceType: "TrxEmployeeHealthSurveillance",
                            ReferenceId: highDose.Id,
                            DedupKey: $"OHS-W5|{highDose.Id:N}");

                        var sentStaff = await notifier.NotifyPermissionHoldersAsync("OccupationalMedicalExamination", "RecordHazardSurveillance", request, stoppingToken);
                        var sentEmployee = await notifier.NotifyWorkforceProfileAsync(highDose.WorkforceProfileId, request, stoppingToken);

                        if (sentStaff + sentEmployee > 0)
                        {
                            _logger.LogWarning("OHS-W5: Alarm dosis radiasi {Number} dikirim ke {Staff} petugas dan {Emp} akun pegawai.", highDose.SurveillanceNumber, sentStaff, sentEmployee);
                        }
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
                    var notifier = scope.ServiceProvider.GetRequiredService<IHrdNotificationService>();

                    var now = DateTime.UtcNow;
                    var pendingInjuries = await context.TrxEmployeeInjuries
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.InjuryStatus == "Reported")
                        .ToListAsync(stoppingToken);

                    foreach (var injury in pendingInjuries)
                    {
                        var deadline = injury.InjuryDateTime.AddHours(48);
                        var hoursLeft = (deadline - now).TotalHours;
                        if (hoursLeft > 12) continue;

                        var isBreached = hoursLeft <= 0;
                        var request = new HrdNotificationRequest(
                            Title: isBreached
                                ? $"Batas pelaporan BPJS JKK terlewati: {injury.InjuryNumber}"
                                : $"Batas pelaporan BPJS JKK tersisa {hoursLeft:F0} jam: {injury.InjuryNumber}",
                            Body: isBreached
                                ? $"Laporan kecelakaan kerja {injury.InjuryNumber} sudah melewati batas pelaporan tahap I BPJS Ketenagakerjaan 2x24 jam (INT-HR-90). Segera siapkan klaim tahap I beserta alasan keterlambatan untuk menghindari sanksi."
                                : $"Laporan kecelakaan kerja {injury.InjuryNumber} harus dilaporkan ke BPJS Ketenagakerjaan sebelum {deadline:dd MMMM yyyy HH:mm} UTC. Mohon siapkan klaim tahap I sekarang.",
                            Category: HrdNotificationCategories.OccupationalHealth,
                            Priority: isBreached ? HrdNotificationPriorities.Critical : HrdNotificationPriorities.High,
                            DeepLinkUrl: $"/hrd/occupational-health/injuries/{injury.Id}",
                            SourceModule: "Hrd.OccupationalHealth.OHS-W6",
                            ReferenceType: "TrxEmployeeInjury",
                            ReferenceId: injury.Id,
                            DedupKey: $"OHS-W6|{injury.Id:N}|{(isBreached ? "BREACH" : "H12")}");

                        var sent = await notifier.NotifyPermissionHoldersAsync("WorkRestrictionAndInjury", "PrepareBpjsClaimStage1", request, stoppingToken);
                        if (sent > 0)
                        {
                            _logger.LogWarning("OHS-W6: Peringatan SLA BPJS {Number} ({Stage}) dikirim ke {Count} petugas.", injury.InjuryNumber, isBreached ? "terlewati" : "H-12 jam", sent);
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
