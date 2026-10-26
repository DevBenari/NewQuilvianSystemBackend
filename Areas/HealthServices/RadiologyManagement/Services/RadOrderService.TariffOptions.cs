using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Pilihan pemeriksaan Booking Radiologi yang digerakkan tarif, dan penilaian tarif per kelas pasien.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Daftar "Nama Pemeriksaan" dibangun dari master tarif radiologi (<c>MstTariff.TariffName</c>) dan dicari menurut
    /// nama serta kode tarifnya. Satu baris per pemeriksaan (<c>MstProcedure</c>), betapapun banyak variasi tarif
    /// kelasnya. Tarif yang tampil adalah tarif yang berlaku untuk kelas yang dipakai, dipilih dengan aturan yang sama
    /// dengan Billing (<see cref="MstTariffSelector"/>): tarif kelas pasien lebih dulu, lalu tarif umum, dan dua tarif
    /// sama spesifik dengan harga berbeda dilaporkan sebagai konflik — tidak pernah dipilih acak atau menurut harga.
    /// </para>
    /// <para>
    /// Nama tarif yang belum dapat dipesan tetap tampil (nonaktif, dengan alasannya) supaya kasir dan admin melihat
    /// apa yang kurang: tarif yang belum dipetakan ke master pemeriksaan (<c>ProcedureId</c> kosong), pemeriksaan yang
    /// belum bertanda radiologi (<c>MstProcedure.IsRadiology</c>), atau pemeriksaan tanpa tarif untuk kelas ini.
    /// Pemetaan tidak pernah ditebak dari kemiripan nama atau kode.
    /// </para>
    /// <para>
    /// Kelas pratinjau hanya mengubah harga yang DITAMPILKAN. Kelas kunjungan (<c>RegPatientEncounter.PatientClassId</c>)
    /// tidak pernah diubah dari sini, dan Billing menagih menurut kelas kunjungan.
    /// </para>
    /// </remarks>
    public partial class RadOrderService
    {
        private sealed record RadTariffContext(Guid? EncounterClassId, string? EncounterClassName, Guid? ClinicId, bool EncounterFound);

        private async Task<RadTariffContext> LoadTariffContextAsync(Guid? encounterId, CancellationToken cancellationToken)
        {
            if (!encounterId.HasValue || encounterId.Value == Guid.Empty)
            {
                return new RadTariffContext(null, null, null, false);
            }

            var encounter = await _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(e => e.Id == encounterId.Value && !e.IsDelete)
                .Select(e => new
                {
                    e.PatientClassId,
                    PatientClassName = e.PatientClass != null ? e.PatientClass.PatientClassName : null,
                    e.ClinicId,
                })
                .FirstOrDefaultAsync(cancellationToken);

            return encounter == null
                ? new RadTariffContext(null, null, null, false)
                : new RadTariffContext(encounter.PatientClassId, encounter.PatientClassName, encounter.ClinicId, true);
        }

        /// <summary>Tarif yang berlaku pada waktu tertentu — syarat sama dengan <c>BillingSourceTariffResolver.EffectiveTariffs</c>.</summary>
        private IQueryable<MstTariff> EffectiveProcedureTariffs(DateTime at) =>
            _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(t => t.ProcedureId != null && t.IsActive && !t.IsDelete && !t.IsCancel &&
                            (t.EffectiveStartDate == null || t.EffectiveStartDate <= at) &&
                            (t.EffectiveEndDate == null || at < t.EffectiveEndDate));

        /// <summary>
        /// Tarif radiologi yang berlaku: kategori tarif bertanda radiologi (atau bernama "Radiology"), atau milik
        /// pemeriksaan yang bertanda radiologi. Termasuk tarif yang <c>ProcedureId</c>-nya masih kosong.
        /// </summary>
        private IQueryable<MstTariff> EffectiveRadiologyTariffs(DateTime at) =>
            _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(t => t.IsActive && !t.IsDelete && !t.IsCancel &&
                            (t.EffectiveStartDate == null || t.EffectiveStartDate <= at) &&
                            (t.EffectiveEndDate == null || at < t.EffectiveEndDate) &&
                            ((t.TariffCategory != null &&
                              (t.TariffCategory.IsRadiology || t.TariffCategory.TariffCategoryName.ToLower() == "radiology")) ||
                             (t.Procedure != null && t.Procedure.IsRadiology)));

        private async Task<Dictionary<Guid, MstTariffSelection>> SelectTariffsAsync(
            IReadOnlyCollection<Guid> procedureIds,
            Guid? patientClassId,
            Guid? clinicId,
            DateTime at,
            CancellationToken cancellationToken)
        {
            var hasil = new Dictionary<Guid, MstTariffSelection>();
            if (procedureIds.Count == 0) return hasil;

            var kandidat = await EffectiveProcedureTariffs(at)
                .Where(t => procedureIds.Contains(t.ProcedureId!.Value) &&
                            (t.ClinicId == null || t.ClinicId == clinicId) &&
                            (t.PatientClassId == null || t.PatientClassId == patientClassId))
                .ToListAsync(cancellationToken);

            var perPemeriksaan = kandidat.ToLookup(t => t.ProcedureId!.Value);
            foreach (var id in procedureIds)
            {
                hasil[id] = MstTariffSelector.Select(perPemeriksaan[id], patientClassId, clinicId);
            }

            return hasil;
        }

        private async Task<Dictionary<Guid, string>> LoadPatientClassNamesAsync(
            IEnumerable<Guid?> ids,
            CancellationToken cancellationToken)
        {
            var daftar = ids.Where(x => x.HasValue && x.Value != Guid.Empty).Select(x => x!.Value).Distinct().ToList();
            if (daftar.Count == 0) return new Dictionary<Guid, string>();

            return await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(c => daftar.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.PatientClassName, cancellationToken);
        }

        /// <summary>Alasan yang dapat dibaca kasir untuk tarif yang tidak sah; <c>null</c> bila sah.</summary>
        public static string? DescribeTariffProblem(
            string status,
            string? patientClassName,
            IReadOnlyCollection<MstTariff>? tied = null)
        {
            switch (status)
            {
                case RadTariffStatuses.Resolved:
                    return null;
                case RadTariffStatuses.EncounterRequired:
                    return "Pilih pasien (No. RM) terlebih dahulu agar tarif sesuai kelas pasien dapat dinilai.";
                case RadTariffStatuses.ProcedureNotMapped:
                    return "Tarif radiologi ini belum dipetakan ke master pemeriksaan (ProcedureId pada master tarif kosong), jadi belum dapat dipesan. Lengkapi pemetaannya di master tarif.";
                case RadTariffStatuses.ProcedureNotRadiology:
                    return "Master pemeriksaan untuk tarif ini belum bertanda Radiologi (Procedure: IsRadiology), jadi belum dapat dipesan. Tandai pemeriksaannya di master pemeriksaan.";
                case RadTariffStatuses.Ambiguous:
                    var kode = tied == null ? string.Empty : string.Join(", ", tied.Select(t => t.TariffCode));
                    return string.IsNullOrWhiteSpace(patientClassName)
                        ? $"Ada lebih dari satu tarif umum dengan harga berbeda ({kode}). Rapikan master tarif sebelum pemeriksaan ini dipilih."
                        : $"Ada lebih dari satu tarif kelas {patientClassName} dengan harga berbeda ({kode}). Rapikan master tarif sebelum pemeriksaan ini dipilih.";
                case RadTariffStatuses.ProcedureInvalid:
                    return "Pemeriksaan radiologi tidak ditemukan atau tidak aktif.";
                default:
                    return string.IsNullOrWhiteSpace(patientClassName)
                        ? "Kelas pasien belum tercatat pada kunjungan dan belum ada tarif umum untuk pemeriksaan ini."
                        : $"Tarif belum tersedia untuk kelas {patientClassName} pada tanggal ini. Lengkapi tarif radiologi untuk kelas tersebut.";
            }
        }

        private static string StatusFrom(MstTariffSelectionStatus status) => status switch
        {
            MstTariffSelectionStatus.Resolved => RadTariffStatuses.Resolved,
            MstTariffSelectionStatus.Ambiguous => RadTariffStatuses.Ambiguous,
            _ => RadTariffStatuses.NotFound,
        };

        /// <summary>
        /// Pilihan "Nama Pemeriksaan" Booking, dibangun dari tarif radiologi dan dicari menurut nama/kode tarif dan
        /// pemeriksaan. Urutan: pemeriksaan bertarif sah untuk kelas ini lebih dulu, lalu pemeriksaan lain, lalu nama
        /// tarif yang belum terpetakan ke master pemeriksaan. Yang tidak dapat dipesan tampil nonaktif beserta alasannya.
        /// Baca murni.
        /// </summary>
        public async Task<PagedResult<RadBookingProcedureOptionResponse>> GetBookingProcedureOptionsAsync(
            RadBookingProcedureOptionQuery? query,
            CancellationToken cancellationToken = default)
        {
            var (halaman, ukuran, cari) = RapikanPilihan(query);
            var konteks = await LoadTariffContextAsync(query?.EncounterId, cancellationToken);
            var kelasId = query?.PatientClassId is { } dipilih && dipilih != Guid.Empty
                ? dipilih
                : konteks.EncounterClassId;
            var klinikId = konteks.ClinicId;
            var sekarang = DateTime.UtcNow;
            var tarifRadiologi = EffectiveRadiologyTariffs(sekarang);

            // Bagian A — pemeriksaan (MstProcedure) yang bertanda radiologi ATAU punya tarif radiologi.
            var pemeriksaan = _dbContext.MstProcedures
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive &&
                            (x.IsRadiology || tarifRadiologi.Any(t => t.ProcedureId == x.Id)));

            if (cari != null)
            {
                pemeriksaan = pemeriksaan.Where(x =>
                    x.ProcedureName.ToLower().Contains(cari) ||
                    x.ProcedureCode.ToLower().Contains(cari) ||
                    (x.ExternalProcedureCode != null && x.ExternalProcedureCode.ToLower().Contains(cari)) ||
                    (x.ProcedureCategoryName != null && x.ProcedureCategoryName.ToLower().Contains(cari)) ||
                    _dbContext.Set<MstTariff>().Any(t => t.ProcedureId == x.Id && !t.IsDelete && t.IsActive &&
                        (t.TariffName.ToLower().Contains(cari) || t.TariffCode.ToLower().Contains(cari))));
            }

            var tarifBerlaku = EffectiveProcedureTariffs(sekarang)
                .Where(t => (t.ClinicId == null || t.ClinicId == klinikId) &&
                            (t.PatientClassId == null || t.PatientClassId == kelasId));

            // Bagian B — nama tarif radiologi yang belum dipetakan ke pemeriksaan (ProcedureId kosong), satu baris
            // per nama betapapun banyak varian tarifnya.
            var tanpaPemeriksaan = tarifRadiologi.Where(t => t.ProcedureId == null);
            if (cari != null)
            {
                tanpaPemeriksaan = tanpaPemeriksaan.Where(t =>
                    t.TariffName.ToLower().Contains(cari) || t.TariffCode.ToLower().Contains(cari));
            }

            var namaTanpaPemeriksaan = tanpaPemeriksaan
                .GroupBy(t => t.TariffName.Trim().ToLower())
                .Select(g => new { Nama = g.Min(t => t.TariffName), Kode = g.Min(t => t.TariffCode), Jumlah = g.Count() });

            var totalA = await pemeriksaan.CountAsync(cancellationToken);
            var totalB = await namaTanpaPemeriksaan.CountAsync(cancellationToken);
            var lewati = (halaman - 1) * ukuran;

            var halamanA = lewati < totalA
                ? await pemeriksaan
                    .OrderBy(x => x.IsRadiology && tarifBerlaku.Any(t => t.ProcedureId == x.Id) ? 0 : 1)
                    .ThenBy(x => x.ProcedureName).ThenBy(x => x.ProcedureCode)
                    .Skip(lewati).Take(ukuran)
                    .Select(x => new
                    {
                        x.Id,
                        x.ProcedureCode,
                        x.ProcedureName,
                        x.ProcedureCategoryName,
                        x.ExternalProcedureCode,
                        x.IsRadiology,
                    })
                    .ToListAsync(cancellationToken)
                : [];

            var sisa = ukuran - halamanA.Count;
            var lewatiB = Math.Max(0, lewati - totalA);
            var halamanB = sisa > 0 && lewatiB < totalB
                ? await namaTanpaPemeriksaan
                    .OrderBy(x => x.Nama).ThenBy(x => x.Kode)
                    .Skip(lewatiB).Take(sisa)
                    .ToListAsync(cancellationToken)
                : [];

            // Id tarif untuk baris B (satu per kode; kode ganda, bila ada, memakai yang pertama).
            var kodeB = halamanB.Select(x => x.Kode).ToList();
            var idTarifB = new Dictionary<string, Guid>();
            if (kodeB.Count > 0)
            {
                var barisTarifB = await tarifRadiologi
                    .Where(t => t.ProcedureId == null && kodeB.Contains(t.TariffCode))
                    .Select(t => new { t.TariffCode, t.Id })
                    .ToListAsync(cancellationToken);
                idTarifB = barisTarifB
                    .GroupBy(t => t.TariffCode)
                    .ToDictionary(g => g.Key, g => g.First().Id);
            }

            var pilihanTarif = konteks.EncounterFound
                ? await SelectTariffsAsync(
                    halamanA.Where(x => x.IsRadiology).Select(x => x.Id).ToList(), kelasId, klinikId, sekarang, cancellationToken)
                : new Dictionary<Guid, MstTariffSelection>();

            var namaKelas = await LoadPatientClassNamesAsync(
                pilihanTarif.Values.Select(s => s.Chosen?.PatientClassId)
                    .Append(kelasId)
                    .Concat(pilihanTarif.Values.SelectMany(s => s.Tied.Select(t => t.PatientClassId))),
                cancellationToken);

            var dipakaiNama = kelasId.HasValue ? namaKelas.GetValueOrDefault(kelasId.Value) : null;

            var items = halamanA.Select(x =>
            {
                var respons = new RadBookingProcedureOptionResponse
                {
                    Id = x.Id,
                    ProcedureCode = x.ProcedureCode,
                    ProcedureName = x.ProcedureName,
                    ProcedureCategoryName = x.ProcedureCategoryName,
                    ExternalProcedureCode = x.ExternalProcedureCode,
                    DisplayName = x.ProcedureName,
                };

                if (!x.IsRadiology)
                {
                    respons.TariffStatus = RadTariffStatuses.ProcedureNotRadiology;
                    respons.UnavailableReason = DescribeTariffProblem(respons.TariffStatus, dipakaiNama);
                    return respons;
                }

                if (!konteks.EncounterFound)
                {
                    respons.TariffStatus = RadTariffStatuses.EncounterRequired;
                    respons.UnavailableReason = DescribeTariffProblem(respons.TariffStatus, dipakaiNama);
                    return respons;
                }

                var pilihan = pilihanTarif[x.Id];
                respons.TariffStatus = StatusFrom(pilihan.Status);
                respons.IsSelectable = pilihan.Status == MstTariffSelectionStatus.Resolved;
                respons.UnavailableReason = DescribeTariffProblem(respons.TariffStatus, dipakaiNama, pilihan.Tied);

                if (pilihan.Chosen is { } tarif)
                {
                    respons.TariffId = tarif.Id;
                    respons.TariffCode = tarif.TariffCode;
                    respons.TariffName = tarif.TariffName;
                    respons.TariffAmount = tarif.NormalPrice;
                    respons.TariffPatientClassId = tarif.PatientClassId;
                    respons.TariffPatientClassName = tarif.PatientClassId.HasValue
                        ? namaKelas.GetValueOrDefault(tarif.PatientClassId.Value)
                        : null;
                    respons.EffectiveStartDate = tarif.EffectiveStartDate;
                    respons.EffectiveEndDate = tarif.EffectiveEndDate;
                    if (!string.IsNullOrWhiteSpace(tarif.TariffName)) respons.DisplayName = tarif.TariffName.Trim();
                }

                return respons;
            }).ToList();

            items.AddRange(halamanB.Select(b =>
            {
                var nama = b.Nama.Trim();
                return new RadBookingProcedureOptionResponse
                {
                    // Tidak ada pemeriksaan untuk dikirim saat booking; Id memakai id tarif supaya baris tetap unik.
                    Id = idTarifB.GetValueOrDefault(b.Kode),
                    ProcedureCode = b.Kode,
                    ProcedureName = nama,
                    DisplayName = nama,
                    TariffStatus = RadTariffStatuses.ProcedureNotMapped,
                    IsSelectable = false,
                    TariffId = idTarifB.TryGetValue(b.Kode, out var tarifId) ? tarifId : null,
                    TariffCode = b.Kode,
                    TariffName = nama,
                    UnavailableReason = $"{DescribeTariffProblem(RadTariffStatuses.ProcedureNotMapped, null)} ({b.Jumlah} varian tarif)",
                };
            }));

            var total = totalA + totalB;
            return new PagedResult<RadBookingProcedureOptionResponse>
            {
                PageNumber = halaman,
                PageSize = ukuran,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)ukuran),
                Items = items,
            };
        }

        /// <summary>
        /// Pilihan "Kelas Pasien" Booking dari <c>MstPatientClass</c> (aktif dan dipakai untuk pemetaan tarif).
        /// Kelas kunjungan selalu ikut dan ditandai sebagai kelas bawaan. Baca murni.
        /// </summary>
        public async Task<List<RadBookingClassOptionResponse>> GetBookingClassOptionsAsync(
            Guid? encounterId,
            CancellationToken cancellationToken = default)
        {
            var konteks = await LoadTariffContextAsync(encounterId, cancellationToken);
            var kelasKunjungan = konteks.EncounterClassId;

            return await _dbContext.Set<MstPatientClass>()
                .AsNoTracking()
                .Where(c => !c.IsDelete && c.IsActive && (c.IsForTariffMapping || c.Id == kelasKunjungan))
                .OrderBy(c => c.SortOrder).ThenBy(c => c.ClassLevel).ThenBy(c => c.PatientClassName)
                .Select(c => new RadBookingClassOptionResponse
                {
                    Id = c.Id,
                    PatientClassCode = c.PatientClassCode,
                    PatientClassName = c.PatientClassName,
                    IsEncounterClass = kelasKunjungan != null && c.Id == kelasKunjungan,
                    SortOrder = c.SortOrder,
                })
                .ToListAsync(cancellationToken);
        }
    }
}
