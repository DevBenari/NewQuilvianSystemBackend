using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Konfigurasi Farmasi untuk MAR — jam pemberian per kode frekuensi dan pengaturan MAR — <c>BE-RWI-114</c>
    /// kriteria 5 dan 6, <c>FR-KEP-071</c>, <c>VAL-KEP-34</c>, api-contract 7.13.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Contoh.</b> <c>q12h</c> tanpa unit: slot 1 08.00, slot 2 20.00. ICU menimpa <c>q8h</c> menjadi 06.00,
    /// 14.00, 22.00. Mengganti jadwal <b>tidak</b> memindahkan dosis <c>Due</c> yang sudah terbentuk.
    /// </para>
    /// <para>
    /// <b>Frekuensi tanpa jadwal tetap terlihat.</b> Kode yang dipakai resep aktif tetapi belum punya jam dikembalikan
    /// <c>GET /frequency-codes-without-schedule</c>, dan pada MAR tampil "Jadwal pemberian untuk frekuensi {kode}
    /// belum dikonfigurasi". Butir itu tidak membentuk dosis; perawat mencatat pemberiannya lewat jalur tanpa jadwal.
    /// </para>
    /// </remarks>
    public partial class MedicationAdministrationService
    {
        private static readonly string[] FormatJam = { "HH:mm", "H:mm", "HH:mm:ss" };

        public async Task<List<MedicationScheduleTimeSetResponse>> GetScheduleTimesAsync(
            Guid? serviceUnitId,
            string? frequencyCode,
            CancellationToken cancellationToken = default)
        {
            var kode = NormalizeFrequencyCode(frequencyCode);

            var query = _dbContext.Set<PhmMedicationScheduleTime>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            // Unit dipilih: jadwal unit itu beserta bawaan, supaya penimpaan terlihat berdampingan.
            if (serviceUnitId.HasValue)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value || x.ServiceUnitId == null);

            if (kode != null)
                query = query.Where(x => x.FrequencyCode == kode);

            var baris = await query
                .OrderBy(x => x.FrequencyCode)
                .ThenBy(x => x.ServiceUnitId)
                .ThenBy(x => x.SlotNumber)
                .ToListAsync(cancellationToken);

            var unitIds = baris.Where(x => x.ServiceUnitId.HasValue).Select(x => x.ServiceUnitId!.Value).Distinct().ToList();
            var namaUnit = await _dbContext.Set<MstServiceUnit>().AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.ServiceUnitName, cancellationToken);

            var kaliPerHari = await ExpectedTimesPerDayAsync(baris.Select(x => x.FrequencyCode).Distinct().ToList(), cancellationToken);

            return baris
                .GroupBy(x => new { x.FrequencyCode, x.ServiceUnitId })
                .Select(g => new MedicationScheduleTimeSetResponse
                {
                    FrequencyCode = g.Key.FrequencyCode,
                    ServiceUnitId = g.Key.ServiceUnitId,
                    ServiceUnitName = g.Key.ServiceUnitId.HasValue ? namaUnit.GetValueOrDefault(g.Key.ServiceUnitId.Value) : null,
                    IsDefault = !g.Key.ServiceUnitId.HasValue,
                    ExpectedTimesPerDay = kaliPerHari.GetValueOrDefault(g.Key.FrequencyCode),
                    Times = g.OrderBy(x => x.SlotNumber).Select(x => new MedicationScheduleSlotResponse
                    {
                        Id = x.Id,
                        SlotNumber = x.SlotNumber,
                        TimeOfDay = x.TimeOfDay.ToString("HH:mm", CultureInfo.InvariantCulture)
                    }).ToList()
                })
                .ToList();
        }

        /// <summary>Mengganti seluruh slot satu kode frekuensi untuk satu unit atau bawaan.</summary>
        public async Task<NursingResult<List<MedicationScheduleTimeSetResponse>>> ReplaceScheduleTimesAsync(
            ReplaceMedicationScheduleTimesRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kode = NormalizeFrequencyCode(request.FrequencyCode);

            if (kode == null || kode.Length > 50)
                return BadRequest<List<MedicationScheduleTimeSetResponse>>("Kode frekuensi wajib diisi, paling banyak 50 karakter.", "FREQUENCY_CODE_REQUIRED");

            if (request.ServiceUnitId.HasValue)
            {
                var unitAda = await _dbContext.Set<MstServiceUnit>().AsNoTracking()
                    .AnyAsync(x => x.Id == request.ServiceUnitId.Value && !x.IsDelete, cancellationToken);

                if (!unitAda)
                    return NursingResult<List<MedicationScheduleTimeSetResponse>>.Fail(StatusCodes.Status404NotFound, "Unit layanan tidak ditemukan.");
            }

            var jam = new List<TimeOnly>();

            foreach (var teks in request.Times ?? new List<string>())
            {
                if (!TimeOnly.TryParseExact(teks?.Trim(), FormatJam, CultureInfo.InvariantCulture, DateTimeStyles.None, out var nilai))
                    return BadRequest<List<MedicationScheduleTimeSetResponse>>($"Jam pemberian \"{teks}\" tidak sah. Gunakan format 08:00.", "INVALID_TIME");

                // VAL-KEP-34a — slot ganda jam yang sama.
                if (jam.Contains(nilai))
                    return BadRequest<List<MedicationScheduleTimeSetResponse>>($"Jam pemberian {nilai:HH\\:mm} tercantum lebih dari sekali.", "DUPLICATE_TIME");

                jam.Add(nilai);
            }

            jam.Sort();

            // VAL-KEP-34a — jumlah slot sama dengan kali per hari kode frekuensi yang dikenal resep aktif.
            if (jam.Count > 0)
            {
                var kaliPerHari = (await ExpectedTimesPerDayAsync(new List<string> { kode }, cancellationToken)).GetValueOrDefault(kode);

                if (kaliPerHari.HasValue && kaliPerHari.Value != jam.Count)
                    return BadRequest<List<MedicationScheduleTimeSetResponse>>($"Kode {kode} membutuhkan {kaliPerHari.Value} jam pemberian.", "SLOT_COUNT_MISMATCH");
            }

            var now = DateTime.UtcNow;
            var lama = await _dbContext.Set<PhmMedicationScheduleTime>()
                .Where(x => !x.IsDelete && x.FrequencyCode == kode && x.ServiceUnitId == request.ServiceUnitId)
                .ToListAsync(cancellationToken);

            // Slot dipakai ulang per nomor, supaya unique (kode, unit, slot) tidak pernah bertabrakan dalam satu simpan.
            for (var i = 0; i < jam.Count; i++)
            {
                var nomorSlot = i + 1;
                var baris = lama.FirstOrDefault(x => x.SlotNumber == nomorSlot);

                if (baris == null)
                {
                    _dbContext.Set<PhmMedicationScheduleTime>().Add(new PhmMedicationScheduleTime
                    {
                        Id = Guid.NewGuid(),
                        FrequencyCode = kode,
                        ServiceUnitId = request.ServiceUnitId,
                        SlotNumber = nomorSlot,
                        TimeOfDay = jam[i],
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }
                else
                {
                    baris.TimeOfDay = jam[i];
                    baris.IsActive = true;
                    baris.UpdateDateTime = now;
                    baris.UpdateBy = actorUserId;
                }
            }

            foreach (var sisa in lama.Where(x => x.SlotNumber > jam.Count))
            {
                sisa.IsActive = false;
                sisa.IsDelete = true;
                sisa.DeleteDateTime = now;
                sisa.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "MedicationScheduleSetting.ReplaceScheduleTimes", "Farmasi mengganti jam pemberian satu kode frekuensi.",
                new { FrequencyCode = kode, request.ServiceUnitId, SlotCount = jam.Count, UpdatedBy = actorUserId });

            var hasil = (await GetScheduleTimesAsync(request.ServiceUnitId, kode, cancellationToken))
                .Where(x => x.ServiceUnitId == request.ServiceUnitId)
                .ToList();

            return NursingResult<List<MedicationScheduleTimeSetResponse>>.Ok(hasil,
                jam.Count == 0 ? "Jadwal kode frekuensi dihapus." : "Jam pemberian berhasil disimpan. Dosis yang sudah terbentuk tidak dipindahkan.");
        }

        /// <summary>
        /// Kode frekuensi yang dipakai butir berjadwal episode berjalan tetapi belum punya jam untuk unit episodenya
        /// maupun bawaan — <c>BE-RWI-114</c> kriteria 6.
        /// </summary>
        public async Task<List<string>> GetFrequencyCodesWithoutScheduleAsync(CancellationToken cancellationToken = default)
        {
            var dipakai = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && !x.IsStopped && !x.IsAsNeeded &&
                            x.DoseKind == PrescriptionDoseKind.Fixed &&
                            x.FrequencyCode != null && x.FrequencyCode != "" &&
                            !x.Prescription!.IsDelete &&
                            x.Prescription.PrescriptionStatus == PrescriptionStatus.Submitted &&
                            x.Prescription.PrescriptionOrderType != PrescriptionOrderType.Discharge &&
                            x.Prescription.InpEpisodeId != null)
                .Join(_dbContext.Set<InpEpisode>().AsNoTracking()
                        .Where(e => !e.IsDelete && (e.EpisodeStatus == InpEpisodeStatus.Admitted || e.EpisodeStatus == InpEpisodeStatus.DischargePending)),
                    x => x.Prescription!.InpEpisodeId,
                    e => (Guid?)e.Id,
                    (x, e) => new { x.FrequencyCode, e.ServiceUnitId })
                .Distinct()
                .ToListAsync(cancellationToken);

            var hasil = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kelompok in dipakai.GroupBy(x => x.ServiceUnitId))
            {
                var jadwal = await ResolveScheduleAsync(kelompok.Select(x => x.FrequencyCode!), kelompok.Key, cancellationToken);

                foreach (var x in kelompok)
                {
                    var kode = NormalizeFrequencyCode(x.FrequencyCode)!;

                    if (!jadwal.ContainsKey(kode))
                        hasil.Add(kode);
                }
            }

            return hasil.ToList();
        }

        public Task<MedicationAdministrationSettingResponse> GetSettingAsync(CancellationToken cancellationToken = default) =>
            GetEffectiveSettingAsync(cancellationToken);

        public async Task<NursingResult<MedicationAdministrationSettingResponse>> UpdateSettingAsync(
            UpdateMedicationAdministrationSettingRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // VAL-KEP-34b
            if (request.DoseGenerationHorizonHours is < 1 or > 72 ||
                request.MissedAfterMinutes is < 0 ||
                request.PrnEvaluationMinutes is < 0)
                return BadRequest<MedicationAdministrationSettingResponse>("Nilai pengaturan di luar batas.", "SETTING_OUT_OF_RANGE");

            var now = DateTime.UtcNow;
            var baris = await _dbContext.Set<PhmMedicationAdministrationSetting>()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (baris == null)
            {
                baris = new PhmMedicationAdministrationSetting
                {
                    Id = Guid.NewGuid(),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<PhmMedicationAdministrationSetting>().Add(baris);
            }
            else
            {
                baris.UpdateDateTime = now;
                baris.UpdateBy = actorUserId;
            }

            baris.DoseGenerationHorizonHours = request.DoseGenerationHorizonHours;
            baris.MissedAfterMinutes = request.MissedAfterMinutes;
            baris.PrnEvaluationMinutes = request.PrnEvaluationMinutes;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "MedicationScheduleSetting.UpdateSetting", "Farmasi mengubah pengaturan MAR.",
                new { baris.Id, baris.DoseGenerationHorizonHours, baris.MissedAfterMinutes, baris.PrnEvaluationMinutes, UpdatedBy = actorUserId });

            return NursingResult<MedicationAdministrationSettingResponse>.Ok(await GetEffectiveSettingAsync(cancellationToken), "Pengaturan MAR berhasil disimpan.");
        }

        /// <summary>
        /// Kali per hari per kode dari butir resep aktif. Hanya kode yang <c>FrequencyPerDay</c>-nya seragam dan bulat
        /// yang dianggap dikenal; kode lain tidak dibatasi jumlah slotnya.
        /// </summary>
        private async Task<Dictionary<string, int?>> ExpectedTimesPerDayAsync(List<string> codes, CancellationToken cancellationToken)
        {
            var hasil = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);

            if (codes.Count == 0)
                return hasil;

            var nilai = await _dbContext.Set<PhmPrescriptionItem>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.FrequencyCode != null && codes.Contains(x.FrequencyCode) && x.FrequencyPerDay != null)
                .Select(x => new { x.FrequencyCode, x.FrequencyPerDay })
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var kelompok in nilai.GroupBy(x => x.FrequencyCode!, StringComparer.OrdinalIgnoreCase))
            {
                var berbeda = kelompok.Select(x => x.FrequencyPerDay!.Value).Distinct().ToList();

                if (berbeda.Count == 1 && berbeda[0] > 0 && berbeda[0] == decimal.Truncate(berbeda[0]))
                    hasil[kelompok.Key] = (int)berbeda[0];
            }

            return hasil;
        }
    }
}
