using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// MAR — <i>Medication Administration Record</i> rawat inap: pembentukan dosis, MAR harian, pencatatan,
    /// cek ganda, koreksi, dan pembatalan dosis — <c>BE-RWI-114</c> s.d. <c>BE-RWI-118</c>, <c>CAP-023-MAR</c>,
    /// arsitektur keperawatan 0.4 bagian 11.5.10–11.5.11, state matrix 0.5.0 bagian 5.5.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Satu baris = satu dosis, dan dosis tidak pernah dihapus.</b> Rekam pemberian obat adalah catatan
    /// hukum: koreksi menyimpan revisi, pembatalan menyimpan alasan. Tidak ada jalur <c>DELETE</c>.
    /// </para>
    /// <para>
    /// <b>Pembentukan dosis idempoten</b> (<c>INT-KEP-08</c>). Ceftriaxone 2 kali sehari, jadwal 08.00 dan
    /// 20.00. MAR dibuka pukul 07.00 → dosis 08.00 dan 20.00 terbentuk. Dibuka lagi pukul 07.05 → nol dosis
    /// baru. Dijaga dua lapis: pembacaan slot yang sudah ada, dan unique
    /// <c>UX_PhmMedicationAdministration_Item_ScheduledAt</c> yang pelanggarannya ditelan sebagai "sudah ada".
    /// </para>
    /// <para>
    /// <b>Penjaga tulis</b> memakai <see cref="NursingEpisodeWriteGuard"/> milik <c>ClinicalManagement</c>:
    /// episode berjalan dan pencatat ditempatkan di unit episode saat simpan. Pelaksana selalu dari akun login.
    /// </para>
    /// </remarks>
    public partial class MedicationAdministrationService
    {
        private const string LogCategory = "HealthServices.Pharmacy.MedicationAdministration";
        private const string SequenceKey = "PHM_MEDICATION_ADMINISTRATION";
        private const string NumberPrefix = "MAR";

        public const int DefaultDoseGenerationHorizonHours = 24;

        /// <summary>Jendela ±60 menit dari jadwal sebelum catatan penyimpangan wajib — <c>VAL-KEP-30c</c>.</summary>
        public const int DeviationWindowMinutes = 60;

        /// <summary>Toleransi jam perangkat untuk waktu pemberian — <c>VAL-KEP-30f</c>.</summary>
        public const int FutureToleranceMinutes = 5;

        public const string AlasanResepDihentikan = "resep dihentikan";
        public const string AlasanPerawatanDitutup = "perawatan ditutup";

        public const string RevisionKindCorrection = "Correction";
        public const string RevisionKindDoubleCheckRejected = "DoubleCheckRejected";

        private readonly ApplicationDbContext _dbContext;
        private readonly NursingEpisodeWriteGuard _writeGuard;
        private readonly NursingActorService _actorService;
        private readonly InpatientClinicalContextService _contextService;
        private readonly DailyMonitoringService _dailyMonitoringService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly LoggerService _loggerService;

        public MedicationAdministrationService(
            ApplicationDbContext dbContext,
            NursingEpisodeWriteGuard writeGuard,
            NursingActorService actorService,
            InpatientClinicalContextService contextService,
            DailyMonitoringService dailyMonitoringService,
            NumberSeriesAllocator numberSeriesAllocator,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _actorService = actorService;
            _contextService = contextService;
            _dailyMonitoringService = dailyMonitoringService;
            _numberSeriesAllocator = numberSeriesAllocator;
            _loggerService = loggerService;
        }

        // =====================================================================
        // BE-RWI-114 — pembentukan dosis
        // =====================================================================

        /// <summary>
        /// Membentuk dosis <c>Due</c> dari butir resep aktif berjadwal sampai cakrawala pengaturan MAR.
        /// Dipanggil saat MAR dibuka dan oleh <see cref="MedicationDoseSchedulerHostedService"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Butir yang dibaca: resep episode berstatus <c>Submitted</c>, bukan obat pulang; butir aktif, tidak
        /// dihentikan, bukan PRN, berdosis tetap (<c>DoseKind = Fixed</c>), dan kode frekuensinya punya jam
        /// pada <c>PhmMedicationScheduleTime</c> untuk unit episode — atau bawaan bila unit tidak punya.
        /// <c>AdministrationTime</c> teks bebas tidak pernah dipakai.
        /// </para>
        /// <para>
        /// Slot dibentuk dari waktu sekarang sampai cakrawala, tidak sebelum resep diajukan. Mengubah jadwal
        /// tidak memindahkan dosis yang sudah terbentuk.
        /// </para>
        /// <para>
        /// Jaring pengaman <c>INT-KEP-09</c>: dosis <c>Due</c> butir yang sudah dihentikan dengan jadwal ≥ waktu
        /// henti dibatalkan di sini juga, sehingga penghentian dari jalur mana pun tidak meninggalkan dosis
        /// yang dapat diberikan.
        /// </para>
        /// </remarks>
        public async Task<DoseGenerationResult> EnsureDosesAsync(
            Guid episodeId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var hasil = new DoseGenerationResult { EpisodeId = episodeId };

            var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.EncounterId, x.PatientId, x.ServiceUnitId, x.EpisodeStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null || NursingEpisodeWriteGuard.EpisodeRejection(episode.EpisodeStatus) != null)
                return hasil;

            var now = DateTime.UtcNow;

            hasil.CancelledStoppedCount = await CancelDueDosesOfStoppedItemsAsync(episodeId, actorUserId, now, cancellationToken);

            if (hasil.CancelledStoppedCount > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
            }

            var setting = await GetEffectiveSettingAsync(cancellationToken);
            var until = now.AddHours(setting.DoseGenerationHorizonHours);

            var butir = await ScheduledItemsQuery(episodeId)
                .Select(x => new
                {
                    x.Id,
                    x.PrescriptionId,
                    x.DrugId,
                    x.FrequencyCode,
                    x.Dose,
                    x.DoseUnitSymbolSnapshot,
                    x.DoseUnitNameSnapshot,
                    x.IsHighAlertSnapshot,
                    BerlakuSejak = x.Prescription!.SubmittedAt ?? x.Prescription.PrescriptionDateTime
                })
                .ToListAsync(cancellationToken);

            if (butir.Count == 0)
                return hasil;

            var jadwal = await ResolveScheduleAsync(butir.Select(x => x.FrequencyCode!), episode.ServiceUnitId, cancellationToken);

            var kandidat = new List<(Guid ItemId, DateTime ScheduledAt)>();

            foreach (var item in butir)
            {
                if (!jadwal.TryGetValue(NormalizeFrequencyCode(item.FrequencyCode)!, out var jam) || jam.Count == 0)
                {
                    if (!hasil.FrequencyCodesWithoutSchedule.Contains(item.FrequencyCode!))
                        hasil.FrequencyCodesWithoutSchedule.Add(item.FrequencyCode!);

                    continue;
                }

                var mulai = item.BerlakuSejak > now ? DateTime.SpecifyKind(item.BerlakuSejak, DateTimeKind.Utc) : now;

                for (var tanggal = HospitalTimeZone.TodayLocal(mulai); tanggal <= HospitalTimeZone.TodayLocal(until); tanggal = tanggal.AddDays(1))
                {
                    foreach (var slot in jam)
                    {
                        var terjadwal = HospitalTimeZone.ToUtc(tanggal, slot);

                        if (terjadwal >= mulai && terjadwal <= until)
                            kandidat.Add((item.Id, terjadwal));
                    }
                }
            }

            if (kandidat.Count == 0)
                return hasil;

            var itemIds = kandidat.Select(x => x.ItemId).Distinct().ToList();
            var awal = kandidat.Min(x => x.ScheduledAt);

            var sudahAda = (await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                    .Where(x => itemIds.Contains(x.PrescriptionItemId) &&
                                x.DoseSource == MedicationDoseSource.Scheduled &&
                                !x.IsDelete &&
                                x.ScheduledAt >= awal &&
                                x.ScheduledAt <= until)
                    .Select(x => new { x.PrescriptionItemId, x.ScheduledAt })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.PrescriptionItemId, x.ScheduledAt!.Value))
                .ToHashSet();

            var baru = new List<PhmMedicationAdministration>();
            var butirById = butir.ToDictionary(x => x.Id);

            foreach (var (itemId, terjadwal) in kandidat)
            {
                if (sudahAda.Contains((itemId, terjadwal)))
                {
                    hasil.ExistingCount++;
                    continue;
                }

                var item = butirById[itemId];
                string nomor;

                try
                {
                    nomor = await AllocateNumberAsync(actorUserId, cancellationToken);
                }
                catch (NumberSeriesAllocationException)
                {
                    hasil.Warning = "Nomor dosis MAR gagal diterbitkan; sebagian dosis belum terbentuk. Hubungi administrator sistem.";
                    break;
                }

                baru.Add(new PhmMedicationAdministration
                {
                    Id = Guid.NewGuid(),
                    AdministrationNumber = nomor,
                    PrescriptionId = item.PrescriptionId,
                    PrescriptionItemId = item.Id,
                    EncounterId = episode.EncounterId,
                    InpEpisodeId = episode.Id,
                    PatientId = episode.PatientId,
                    DrugId = item.DrugId,
                    DoseSource = MedicationDoseSource.Scheduled,
                    ScheduledAt = terjadwal,
                    DoseStatus = MedicationDoseStatus.Due,
                    PlannedDose = item.Dose,
                    PlannedDoseUnitSnapshot = Truncate(item.DoseUnitSymbolSnapshot ?? item.DoseUnitNameSnapshot, 50),
                    IsHighAlertSnapshot = item.IsHighAlertSnapshot,
                    DoubleCheckStatus = MedicationDoubleCheckStatus.NotRequired,
                    RevisionNumber = 0,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            if (baru.Count == 0)
                return hasil;

            _dbContext.Set<PhmMedicationAdministration>().AddRange(baru);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                hasil.CreatedCount = baru.Count;
            }
            catch (DbUpdateException)
            {
                // Pembentukan paralel (MAR dibuka dua perawat sekaligus, atau bersamaan dengan hosted service)
                // menabrak unique slot. Ulangi satu per satu; tabrakan berarti dosisnya sudah ada.
                _dbContext.ChangeTracker.Clear();

                foreach (var dosis in baru)
                {
                    _dbContext.Set<PhmMedicationAdministration>().Add(dosis);

                    try
                    {
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        hasil.CreatedCount++;
                    }
                    catch (DbUpdateException)
                    {
                        hasil.ExistingCount++;
                    }
                    finally
                    {
                        _dbContext.ChangeTracker.Clear();
                    }
                }
            }

            if (hasil.CreatedCount > 0)
            {
                await _loggerService.InfoAsync(LogCategory, "MedicationAdministration.EnsureDoses", "Dosis MAR terjadwal dibentuk.",
                    new { EpisodeId = episodeId, hasil.CreatedCount, hasil.ExistingCount, hasil.CancelledStoppedCount });
            }

            return hasil;
        }

        /// <summary>Episode yang dosisnya dibentuk hosted service: <c>Admitted</c> dan <c>DischargePending</c>.</summary>
        public Task<List<Guid>> GetEpisodesForDoseGenerationAsync(CancellationToken cancellationToken = default) =>
            _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => !x.IsDelete &&
                            (x.EpisodeStatus == InpEpisodeStatus.Admitted || x.EpisodeStatus == InpEpisodeStatus.DischargePending))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

        /// <summary>Akun pelaku pembentukan terjadwal bila konfigurasi tidak menyebutnya: akun SuperAdmin.</summary>
        public Task<Guid> ResolveSystemActorUserIdAsync(CancellationToken cancellationToken = default) =>
            _dbContext.Users.AsNoTracking()
                .Where(x => x.NormalizedUserName == "SUPERADMIN" || x.NormalizedEmail == "SUPERADMIN@ADMIN.COM")
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

        // =====================================================================
        // BE-RWI-114 — MAR harian dan detail
        // =====================================================================

        public async Task<NursingResult<MedicationAdministrationChartResponse>> GetChartAsync(
            Guid episodeId,
            DateOnly? date,
            bool includeStopped,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.ServiceUnitId, x.EpisodeStatus, x.ClosedAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
                return NursingResult<MedicationAdministrationChartResponse>.Fail(StatusCodes.Status404NotFound, "Perawatan rawat inap tidak ditemukan.");

            var penolakan = NursingEpisodeWriteGuard.EpisodeRejection(episode.EpisodeStatus);
            var now = DateTime.UtcNow;
            var hari = date ?? HospitalTimeZone.TodayLocal(now);
            var awal = HospitalTimeZone.ToUtc(hari, TimeOnly.MinValue);
            var akhir = HospitalTimeZone.ToUtc(hari.AddDays(1), TimeOnly.MinValue);

            var response = new MedicationAdministrationChartResponse
            {
                EpisodeId = episodeId,
                Date = hari,
                WindowStartUtc = awal,
                WindowEndUtc = akhir,
                IsEpisodeWritable = penolakan == null,
                EpisodeReadOnlyReason = penolakan
            };

            if (penolakan == null)
            {
                var pembentukan = await EnsureDosesAsync(episodeId, actorUserId, cancellationToken);
                response.DoseGenerationWarning = pembentukan.Warning;
            }

            var setting = await GetEffectiveSettingAsync(cancellationToken);
            response.MissedAfterMinutes = setting.MissedAfterMinutes;

            var butirQuery = EpisodeItemsQuery(episodeId);

            if (!includeStopped)
                butirQuery = butirQuery.Where(x => !x.IsStopped || x.StoppedAt == null || x.StoppedAt >= awal);

            var butir = await butirQuery
                .OrderBy(x => x.Prescription!.PrescriptionDateTime)
                .ThenBy(x => x.DrugNameSnapshot)
                .Select(x => new MedicationAdministrationChartItem
                {
                    PrescriptionItemId = x.Id,
                    PrescriptionId = x.PrescriptionId,
                    PrescriptionNumber = x.Prescription!.PrescriptionNumber,
                    DrugId = x.DrugId,
                    DrugName = x.DrugNameSnapshot,
                    GenericName = x.GenericNameSnapshot,
                    Strength = x.StrengthSnapshot,
                    Dose = x.Dose,
                    DoseUnit = x.DoseUnitSymbolSnapshot ?? x.DoseUnitNameSnapshot,
                    Route = x.RouteSnapshot,
                    FrequencyCode = x.FrequencyCode,
                    FrequencyText = x.FrequencyText,
                    Signa = x.Signa,
                    AdministrationInstruction = x.AdministrationInstruction,
                    IsStopped = x.IsStopped,
                    StoppedAt = x.StoppedAt,
                    IsHighAlert = x.IsHighAlertSnapshot,
                    IsAsNeeded = x.IsAsNeeded,
                    DoseKind = x.DoseKind
                })
                .ToListAsync(cancellationToken);

            var jadwal = await ResolveScheduleAsync(
                butir.Where(x => !string.IsNullOrWhiteSpace(x.FrequencyCode)).Select(x => x.FrequencyCode!),
                episode.ServiceUnitId,
                cancellationToken);

            var dosis = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
                            ((x.ScheduledAt != null && x.ScheduledAt >= awal && x.ScheduledAt < akhir) ||
                             (x.ScheduledAt == null && x.AdministeredAt != null && x.AdministeredAt >= awal && x.AdministeredAt < akhir) ||
                             (x.ScheduledAt == null && x.AdministeredAt == null && x.CreateDateTime >= awal && x.CreateDateTime < akhir)))
                .ToListAsync(cancellationToken);

            var sel = await ToListItemsAsync(dosis, actorUserId, episode.ClosedAt, episode.EpisodeStatus, setting.MissedAfterMinutes, now, cancellationToken);

            foreach (var item in butir)
            {
                var berjadwal = !item.IsAsNeeded && item.DoseKind == PrescriptionDoseKind.Fixed && !string.IsNullOrWhiteSpace(item.FrequencyCode);

                if (!string.IsNullOrWhiteSpace(item.FrequencyCode) &&
                    jadwal.TryGetValue(NormalizeFrequencyCode(item.FrequencyCode)!, out var jam) && jam.Count > 0)
                {
                    item.ScheduleConfigured = true;
                    item.ScheduleTimes = jam.Select(t => t.ToString("HH:mm")).ToList();
                }
                else if (berjadwal)
                {
                    item.ScheduleMessage = $"Jadwal pemberian untuk frekuensi {item.FrequencyCode} belum dikonfigurasi.";

                    if (!response.FrequencyCodesWithoutSchedule.Contains(item.FrequencyCode!))
                        response.FrequencyCodesWithoutSchedule.Add(item.FrequencyCode!);
                }

                item.Doses = sel
                    .Where(d => d.PrescriptionItemId == item.PrescriptionItemId)
                    .OrderBy(d => d.ScheduledAt ?? d.AdministeredAt)
                    .ToList();
            }

            response.Items = butir;

            return NursingResult<MedicationAdministrationChartResponse>.Ok(response, "MAR berhasil diambil.");
        }

        public async Task<NursingResult<MedicationAdministrationResponse>> GetAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var dosis = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return dosis == null
                ? NotFound<MedicationAdministrationResponse>()
                : NursingResult<MedicationAdministrationResponse>.Ok(await ToResponseAsync(dosis, actorUserId, false, cancellationToken), "Detail dosis berhasil diambil.");
        }

        public async Task<NursingResult<List<MedicationAdministrationRevisionResponse>>> GetRevisionsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (!ada)
                return NotFound<List<MedicationAdministrationRevisionResponse>>();

            var revisi = await _dbContext.Set<PhmMedicationAdministrationRevision>().AsNoTracking()
                .Where(x => x.AdministrationId == id && !x.IsDelete)
                .OrderByDescending(x => x.RevisionNumber)
                .ToListAsync(cancellationToken);

            var penggunaIds = revisi.Select(x => x.CorrectedByUserId).Distinct().ToList();
            var nama = await _dbContext.Users.AsNoTracking()
                .Where(x => penggunaIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            var hasil = revisi.Select(x => new MedicationAdministrationRevisionResponse
            {
                Id = x.Id,
                RevisionNumber = x.RevisionNumber,
                RevisionKind = x.RevisionKind,
                RevisionKindLabel = x.RevisionKind == RevisionKindDoubleCheckRejected ? "Cek ganda ditolak" : "Koreksi",
                PreviousDoseStatus = x.PreviousDoseStatus,
                PreviousDoseStatusLabel = DoseStatusLabel(x.PreviousDoseStatus),
                PreviousActualDose = x.PreviousActualDose,
                PreviousActualRoute = x.PreviousActualRouteSnapshot,
                PreviousAdministeredAt = x.PreviousAdministeredAt,
                PreviousStatusReason = x.PreviousStatusReason,
                PreviousDeviationNote = x.PreviousDeviationNote,
                PreviousRecordedByUserId = x.PreviousRecordedByUserId,
                CorrectionReason = x.CorrectionReason,
                CorrectedByUserId = x.CorrectedByUserId,
                CorrectedByName = nama.GetValueOrDefault(x.CorrectedByUserId),
                CorrectedAt = x.CorrectedAt
            }).ToList();

            return NursingResult<List<MedicationAdministrationRevisionResponse>>.Ok(hasil, "Riwayat koreksi dosis berhasil diambil.");
        }

        // =====================================================================
        // Pembantu bersama
        // =====================================================================

        /// <summary>Butir resep episode yang tampil pada MAR: resep diajukan, bukan obat pulang, butir aktif.</summary>
        private IQueryable<PhmPrescriptionItem> EpisodeItemsQuery(Guid episodeId) =>
            _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive &&
                            x.Prescription!.InpEpisodeId == episodeId &&
                            !x.Prescription.IsDelete &&
                            x.Prescription.PrescriptionStatus == PrescriptionStatus.Submitted &&
                            x.Prescription.PrescriptionOrderType != PrescriptionOrderType.Discharge);

        /// <summary>Butir yang membentuk dosis terjadwal — <c>INT-KEP-08</c>.</summary>
        private IQueryable<PhmPrescriptionItem> ScheduledItemsQuery(Guid episodeId) =>
            EpisodeItemsQuery(episodeId)
                .Where(x => !x.IsStopped && !x.IsAsNeeded &&
                            x.DoseKind == PrescriptionDoseKind.Fixed &&
                            x.FrequencyCode != null && x.FrequencyCode != "");

        /// <summary>
        /// Jam pemberian per kode frekuensi untuk satu unit. Baris unit mengalahkan bawaan untuk kode yang sama;
        /// kode tanpa baris aktif tidak masuk kamus.
        /// </summary>
        internal async Task<Dictionary<string, List<TimeOnly>>> ResolveScheduleAsync(
            IEnumerable<string> frequencyCodes,
            Guid serviceUnitId,
            CancellationToken cancellationToken)
        {
            var kode = frequencyCodes
                .Select(NormalizeFrequencyCode)
                .Where(x => x != null)
                .Select(x => x!)
                .Distinct()
                .ToList();

            var hasil = new Dictionary<string, List<TimeOnly>>(StringComparer.OrdinalIgnoreCase);

            if (kode.Count == 0)
                return hasil;

            var baris = await _dbContext.Set<PhmMedicationScheduleTime>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive &&
                            (x.ServiceUnitId == serviceUnitId || x.ServiceUnitId == null) &&
                            kode.Contains(x.FrequencyCode))
                .Select(x => new { x.FrequencyCode, x.ServiceUnitId, x.SlotNumber, x.TimeOfDay })
                .ToListAsync(cancellationToken);

            foreach (var kelompok in baris.GroupBy(x => x.FrequencyCode, StringComparer.OrdinalIgnoreCase))
            {
                var milikUnit = kelompok.Where(x => x.ServiceUnitId == serviceUnitId).ToList();
                var dipakai = milikUnit.Count > 0 ? milikUnit : kelompok.Where(x => x.ServiceUnitId == null).ToList();

                if (dipakai.Count > 0)
                    hasil[kelompok.Key] = dipakai.OrderBy(x => x.TimeOfDay).Select(x => x.TimeOfDay).Distinct().ToList();
            }

            return hasil;
        }

        internal async Task<MedicationAdministrationSettingResponse> GetEffectiveSettingAsync(CancellationToken cancellationToken)
        {
            var baris = await _dbContext.Set<PhmMedicationAdministrationSetting>().AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            return baris == null
                ? new MedicationAdministrationSettingResponse
                {
                    IsConfigured = false,
                    DoseGenerationHorizonHours = DefaultDoseGenerationHorizonHours
                }
                : new MedicationAdministrationSettingResponse
                {
                    Id = baris.Id,
                    IsConfigured = true,
                    DoseGenerationHorizonHours = baris.DoseGenerationHorizonHours is >= 1 and <= 72 ? baris.DoseGenerationHorizonHours : DefaultDoseGenerationHorizonHours,
                    MissedAfterMinutes = baris.MissedAfterMinutes,
                    PrnEvaluationMinutes = baris.PrnEvaluationMinutes,
                    UpdateDateTime = baris.UpdateDateTime ?? baris.CreateDateTime
                };
        }

        /// <summary>Nomor dosis MAR <c>MAR-20260915-000123</c> dari alokator bersama — <c>QBE-CODE-006</c>.</summary>
        internal async Task<string> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken) =>
            await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(SequenceKey, NumberPrefix, NumberSeriesResetPolicies.Daily, 6, actorUserId, DateTimeOffset.UtcNow),
                cancellationToken);

        private async Task<List<MedicationAdministrationListItem>> ToListItemsAsync(
            List<PhmMedicationAdministration> dosis,
            Guid actorUserId,
            DateTime? episodeClosedAt,
            InpEpisodeStatus episodeStatus,
            int? missedAfterMinutes,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (dosis.Count == 0)
                return new List<MedicationAdministrationListItem>();

            var itemIds = dosis.Select(x => x.PrescriptionItemId).Distinct().ToList();
            var namaObat = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(x => itemIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DrugNameSnapshot, cancellationToken);

            var pegawaiIds = dosis.Where(x => x.RecordedByEmployeeId.HasValue).Select(x => x.RecordedByEmployeeId!.Value).Distinct().ToList();
            var namaPegawai = await _dbContext.Set<MstEmployee>().AsNoTracking()
                .Where(x => pegawaiIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

            var pasienIds = dosis.Select(x => x.PatientId).Distinct().ToList();
            var pasien = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => pasienIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.MedicalRecordNumber })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var diberikanIds = dosis.Where(x => x.DoseStatus == MedicationDoseStatus.Administered).Select(x => x.Id).ToList();
            var punyaIntake = diberikanIds.Count == 0
                ? new HashSet<Guid>()
                : (await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                    .Where(x => x.MedicationAdministrationId != null &&
                                diberikanIds.Contains(x.MedicationAdministrationId.Value) &&
                                x.EntryStatus == ClinicalMeasurementStatus.Active &&
                                !x.IsDelete)
                    .Select(x => x.MedicationAdministrationId!.Value)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            return dosis.Select(x =>
            {
                var item = new MedicationAdministrationListItem();
                FillListItem(item, x, actorUserId, episodeClosedAt, episodeStatus, missedAfterMinutes, now);
                item.DrugName = namaObat.GetValueOrDefault(x.PrescriptionItemId) ?? string.Empty;
                item.RecordedByName = x.RecordedByEmployeeId.HasValue ? namaPegawai.GetValueOrDefault(x.RecordedByEmployeeId.Value) : null;
                item.HasIntakeEntry = punyaIntake.Contains(x.Id);

                if (pasien.TryGetValue(x.PatientId, out var p))
                {
                    item.PatientName = p.FullName;
                    item.MedicalRecordNumber = p.MedicalRecordNumber;
                }

                return item;
            }).ToList();
        }

        private static void FillListItem(
            MedicationAdministrationListItem item,
            PhmMedicationAdministration x,
            Guid actorUserId,
            DateTime? episodeClosedAt,
            InpEpisodeStatus episodeStatus,
            int? missedAfterMinutes,
            DateTime now)
        {
            var berjalan = NursingEpisodeWriteGuard.EpisodeRejection(episodeStatus) == null;

            item.Id = x.Id;
            item.AdministrationNumber = x.AdministrationNumber;
            item.PrescriptionItemId = x.PrescriptionItemId;
            item.InpEpisodeId = x.InpEpisodeId;
            item.PatientId = x.PatientId;
            item.DoseSource = x.DoseSource;
            item.DoseSourceLabel = x.DoseSource switch
            {
                MedicationDoseSource.Scheduled => "Terjadwal",
                MedicationDoseSource.AsNeeded => "Sesuai kebutuhan / tanpa jadwal",
                _ => "Sliding scale"
            };
            item.ScheduledAt = x.ScheduledAt;
            item.DoseStatus = x.DoseStatus;
            item.DoseStatusLabel = x.DoseStatus == MedicationDoseStatus.Due && x.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending
                ? "Menunggu cek ganda"
                : DoseStatusLabel(x.DoseStatus);
            item.DoubleCheckStatus = x.DoubleCheckStatus;
            item.DoubleCheckStatusLabel = x.DoubleCheckStatus switch
            {
                MedicationDoubleCheckStatus.Pending => "Menunggu",
                MedicationDoubleCheckStatus.Confirmed => "Dikonfirmasi",
                MedicationDoubleCheckStatus.Rejected => "Ditolak",
                _ => "Tidak diperlukan"
            };
            item.PlannedDose = x.PlannedDose;
            item.PlannedDoseUnit = x.PlannedDoseUnitSnapshot;
            item.ActualDose = x.ActualDose;
            item.ActualDoseUnit = x.ActualDoseUnitSnapshot;
            item.ActualRoute = x.ActualRouteSnapshot;
            item.AdministeredAt = x.AdministeredAt;
            item.RecordedByEmployeeId = x.RecordedByEmployeeId;
            item.RecordedByUserId = x.RecordedByUserId;
            item.IsHighAlert = x.IsHighAlertSnapshot;
            item.RevisionNumber = x.RevisionNumber;
            item.CanDoubleCheck = x.DoseStatus == MedicationDoseStatus.Due &&
                                  x.DoubleCheckStatus == MedicationDoubleCheckStatus.Pending &&
                                  x.RecordedByUserId != actorUserId;
            item.IsOverdue = berjalan &&
                             x.DoseStatus == MedicationDoseStatus.Due &&
                             x.ScheduledAt.HasValue &&
                             missedAfterMinutes.HasValue &&
                             now > x.ScheduledAt.Value.AddMinutes(missedAfterMinutes.Value);
            item.IsUnrecordedAtClosure = !berjalan &&
                                         episodeClosedAt.HasValue &&
                                         x.DoseStatus == MedicationDoseStatus.Due &&
                                         x.ScheduledAt.HasValue &&
                                         x.ScheduledAt.Value <= episodeClosedAt.Value;
        }

        private async Task<MedicationAdministrationResponse> ToResponseAsync(
            PhmMedicationAdministration x,
            Guid actorUserId,
            bool isReplay,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(e => e.Id == x.InpEpisodeId)
                .Select(e => new { e.EpisodeStatus, e.ClosedAt })
                .FirstOrDefaultAsync(cancellationToken);

            var setting = await GetEffectiveSettingAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var status = episode?.EpisodeStatus ?? InpEpisodeStatus.Closed;

            var dasar = (await ToListItemsAsync(new List<PhmMedicationAdministration> { x }, actorUserId, episode?.ClosedAt, status, setting.MissedAfterMinutes, now, cancellationToken)).Single();

            var item = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(i => i.Id == x.PrescriptionItemId)
                .Select(i => new { i.DoseKind })
                .FirstOrDefaultAsync(cancellationToken);

            var namaPemeriksa = x.DoubleCheckedByEmployeeId.HasValue
                ? await _dbContext.Set<MstEmployee>().AsNoTracking().Where(e => e.Id == x.DoubleCheckedByEmployeeId.Value).Select(e => e.FullName).FirstOrDefaultAsync(cancellationToken)
                : null;

            var pelaksanaan = await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking()
                .Where(e => e.MedicationAdministrationId == x.Id && !e.IsDelete)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var intake = await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                .Where(e => e.MedicationAdministrationId == x.Id && e.EntryStatus == ClinicalMeasurementStatus.Active && !e.IsDelete)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var response = new MedicationAdministrationResponse
            {
                PrescriptionId = x.PrescriptionId,
                EncounterId = x.EncounterId,
                DrugId = x.DrugId,
                StatusReason = x.StatusReason,
                DeviationNote = x.DeviationNote,
                RecordedAt = x.RecordedAt,
                DoubleCheckedByEmployeeId = x.DoubleCheckedByEmployeeId,
                DoubleCheckedByName = namaPemeriksa,
                DoubleCheckedByUserId = x.DoubleCheckedByUserId,
                DoubleCheckedAt = x.DoubleCheckedAt,
                DoubleCheckNote = x.DoubleCheckNote,
                PrnIndication = x.PrnIndication,
                PrnEvaluationDueAt = x.PrnEvaluationDueAt,
                PrnEvaluationNote = x.PrnEvaluationNote,
                PrnEvaluatedAt = x.PrnEvaluatedAt,
                PrnEvaluatedByUserId = x.PrnEvaluatedByUserId,
                SlidingScaleExecutionId = pelaksanaan,
                LinkedFluidEntryId = intake,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                IsReplay = isReplay
            };

            CopyListItem(dasar, response);

            if (NursingEpisodeWriteGuard.EpisodeRejection(status) == null)
            {
                var slidingScale = x.DoseSource == MedicationDoseSource.SlidingScale || item?.DoseKind == PrescriptionDoseKind.SlidingScale;

                if (x.DoseStatus == MedicationDoseStatus.Due && x.DoubleCheckStatus != MedicationDoubleCheckStatus.Pending &&
                    (!slidingScale || pelaksanaan.HasValue))
                    response.AvailableActions.Add("Record");

                if (response.CanDoubleCheck)
                    response.AvailableActions.Add("DoubleCheck");

                if (x.DoseStatus is MedicationDoseStatus.Administered or MedicationDoseStatus.Held or MedicationDoseStatus.Refused or MedicationDoseStatus.Missed)
                    response.AvailableActions.Add("Correct");

                if (x.DoseSource == MedicationDoseSource.AsNeeded && x.DoseStatus == MedicationDoseStatus.Administered &&
                    !string.IsNullOrWhiteSpace(x.PrnIndication) && !x.PrnEvaluatedAt.HasValue)
                    response.AvailableActions.Add("PrnEvaluation");

                if (x.DoseStatus == MedicationDoseStatus.Administered)
                    response.AvailableActions.Add("ReportAdverseReaction");
            }

            return response;
        }

        private static void CopyListItem(MedicationAdministrationListItem from, MedicationAdministrationListItem to)
        {
            to.Id = from.Id;
            to.AdministrationNumber = from.AdministrationNumber;
            to.PrescriptionItemId = from.PrescriptionItemId;
            to.InpEpisodeId = from.InpEpisodeId;
            to.PatientId = from.PatientId;
            to.PatientName = from.PatientName;
            to.MedicalRecordNumber = from.MedicalRecordNumber;
            to.DrugName = from.DrugName;
            to.DoseSource = from.DoseSource;
            to.DoseSourceLabel = from.DoseSourceLabel;
            to.ScheduledAt = from.ScheduledAt;
            to.DoseStatus = from.DoseStatus;
            to.DoseStatusLabel = from.DoseStatusLabel;
            to.DoubleCheckStatus = from.DoubleCheckStatus;
            to.DoubleCheckStatusLabel = from.DoubleCheckStatusLabel;
            to.PlannedDose = from.PlannedDose;
            to.PlannedDoseUnit = from.PlannedDoseUnit;
            to.ActualDose = from.ActualDose;
            to.ActualDoseUnit = from.ActualDoseUnit;
            to.ActualRoute = from.ActualRoute;
            to.AdministeredAt = from.AdministeredAt;
            to.RecordedByEmployeeId = from.RecordedByEmployeeId;
            to.RecordedByName = from.RecordedByName;
            to.RecordedByUserId = from.RecordedByUserId;
            to.IsHighAlert = from.IsHighAlert;
            to.IsOverdue = from.IsOverdue;
            to.IsUnrecordedAtClosure = from.IsUnrecordedAtClosure;
            to.HasIntakeEntry = from.HasIntakeEntry;
            to.CanDoubleCheck = from.CanDoubleCheck;
            to.RevisionNumber = from.RevisionNumber;
        }

        public static string DoseStatusLabel(MedicationDoseStatus status) => status switch
        {
            MedicationDoseStatus.Due => "Belum dicatat",
            MedicationDoseStatus.Administered => "Diberikan",
            MedicationDoseStatus.Held => "Ditahan",
            MedicationDoseStatus.Refused => "Ditolak pasien",
            MedicationDoseStatus.Missed => "Terlewat",
            MedicationDoseStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        internal static string? NormalizeFrequencyCode(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? Truncate(string? value, int max) =>
            string.IsNullOrWhiteSpace(value) ? null : (value.Trim().Length > max ? value.Trim()[..max] : value.Trim());

        private static string? NormalizeText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static NursingResult<T> NotFound<T>() =>
            NursingResult<T>.Fail(StatusCodes.Status404NotFound, "Dosis MAR tidak ditemukan.");

        private static NursingResult<T> Conflict<T>(string message, string code) =>
            NursingResult<T>.Fail(StatusCodes.Status409Conflict, message, code);

        private static NursingResult<T> BadRequest<T>(string message, string? code = null) =>
            NursingResult<T>.Fail(StatusCodes.Status400BadRequest, message, code);
    }
}
