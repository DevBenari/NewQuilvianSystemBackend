using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Laporan transfer ruangan per periode, dibaca dari linimasa penempatan bed (<c>BE-RWI-184</c>,
    /// <c>RWI-DEC-205</c>, <c>INV-RWF-34</c>, API 11.7, <c>P2</c>). Tidak ada tabel laporan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Transfer</b> = penempatan yang lahir karena penempatan sebelumnya berakhir dengan alasan
    /// <c>Transfer</c>. <b>Koreksi</b> = penempatan yang mengoreksi salah catat (<c>CorrectsPlacementId</c>
    /// terisi, <c>integrasi-billing</c> <c>I1</c>). Keduanya dibedakan lewat <c>EntryKind</c>, supaya
    /// koreksi tidak terbaca sebagai perpindahan pasien.
    /// </para>
    /// <para>
    /// Contoh <c>UAT-RWF-20</c>: laporan 1–7 Okt memuat Budi pindah Melati 302/2 kelas 2 → ICU 01
    /// pada 3 Okt 10.15 beralasan "Perburukan, butuh ventilator", dicatat Ns. Siti, jenis Transfer;
    /// serta koreksi kelas pasien lain yang salah pilih saat admisi, jenis Correction.
    /// </para>
    /// </remarks>
    public sealed class InpRoomTransferReportService
    {
        /// <summary>VAL-RWF-90 (disahkan <c>RWI-DEC-220</c> butir 4).</summary>
        public const string PeriodInvalidMessage = "Pilih periode paling lama 31 hari";

        public const int MaxPeriodDays = 31;

        private const int MaxExportRows = 20_000;

        private readonly ApplicationDbContext _dbContext;

        public InpRoomTransferReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Memeriksa periode; mengembalikan pesan bila tidak sah.</summary>
        public static string? ValidatePeriod(RoomTransferReportQuery query, out DateTime fromUtc, out DateTime toUtcExclusive)
        {
            fromUtc = default;
            toUtcExclusive = default;
            if (!query.PeriodFrom.HasValue || !query.PeriodTo.HasValue)
                return PeriodInvalidMessage;

            var from = query.PeriodFrom.Value;
            var to = query.PeriodTo.Value;
            if (to < from) return PeriodInvalidMessage;
            if ((to.Date - from.Date).TotalDays + 1 > MaxPeriodDays) return PeriodInvalidMessage;

            fromUtc = from.ToUniversalTime();
            // Tanggal tanpa jam berarti sampai akhir hari itu.
            toUtcExclusive = (to.TimeOfDay == TimeSpan.Zero ? to.AddDays(1) : to).ToUniversalTime();
            return null;
        }

        public async Task<PagedResult<RoomTransferReportRow>> GetAsync(RoomTransferReportQuery query,
            CancellationToken cancellationToken = default)
        {
            ValidatePeriod(query, out var fromUtc, out var toUtcExclusive);
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 50 : Math.Min(query.PageSize, 200);

            var rows = await ReadRowsAsync(query, fromUtc, toUtcExclusive, cancellationToken);

            return new PagedResult<RoomTransferReportRow>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = rows.Count,
                TotalPage = (int)Math.Ceiling(rows.Count / (double)pageSize),
                Items = [.. rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)]
            };
        }

        /// <summary>Ekspor Excel dengan saringan yang sama, tanpa paging.</summary>
        public async Task<(byte[] Content, int RowCount)> ExportAsync(RoomTransferReportQuery query,
            CancellationToken cancellationToken = default)
        {
            ValidatePeriod(query, out var fromUtc, out var toUtcExclusive);
            var rows = await ReadRowsAsync(query, fromUtc, toUtcExclusive, cancellationToken);
            var exported = rows.Take(MaxExportRows).ToList();

            string[] header =
            [
                "Waktu Transfer", "No. RM", "Nama Pasien", "Kelas Asal", "Ruang Asal", "Bed Asal",
                "Kelas Tujuan", "Ruang Tujuan", "Bed Tujuan", "Alasan", "Dicatat Oleh", "Jenis"
            ];
            var content = InpXlsxWriter.Write("Transfer Ruangan", header, exported.Select(x => (IReadOnlyList<string?>)new[]
            {
                // Ditampilkan waktu setempat Asia/Jakarta (UTC+7), seperti layar laporan.
                x.TransferredAt.AddHours(7).ToString("yyyy-MM-dd HH:mm"),
                x.MedicalRecordNumber, x.PatientName, x.FromClassName, x.FromRoomName, x.FromBedNumber,
                x.ToClassName, x.ToRoomName, x.ToBedNumber, x.Reason, x.RecordedByName,
                x.EntryKind == RoomTransferEntryKinds.Correction ? "Koreksi" : "Transfer"
            }));
            return (content, exported.Count);
        }

        private async Task<List<RoomTransferReportRow>> ReadRowsAsync(RoomTransferReportQuery query, DateTime fromUtc,
            DateTime toUtcExclusive, CancellationToken cancellationToken)
        {
            // Penempatan tujuan di dalam periode: koreksi (CorrectsPlacementId) atau hasil transfer.
            var targets = await _dbContext.InpBedPlacements.AsNoTracking()
                .Where(x => !x.IsDelete && x.StartDateTime >= fromUtc && x.StartDateTime < toUtcExclusive &&
                    x.Episode != null && !x.Episode.IsDelete)
                .Select(x => new
                {
                    x.Id, x.EpisodeId, x.SequenceNumber, x.StartDateTime, x.ServiceUnitId, x.PatientClassId,
                    x.RoomId, x.BedId, x.TransferReason, x.ChangeReason, x.PlacedByUserId, x.CorrectsPlacementId
                })
                .ToListAsync(cancellationToken);
            if (targets.Count == 0) return [];

            var episodeIds = targets.Select(x => x.EpisodeId).Distinct().ToList();
            var siblings = await _dbContext.InpBedPlacements.AsNoTracking()
                .Where(x => episodeIds.Contains(x.EpisodeId) && !x.IsDelete)
                .Select(x => new
                {
                    x.Id, x.EpisodeId, x.SequenceNumber, x.ServiceUnitId, x.PatientClassId, x.RoomId, x.BedId,
                    x.EndReason, x.EndDateTime
                })
                .ToListAsync(cancellationToken);

            var pairs = new List<(Guid ToId, Guid FromId, string Kind)>();
            foreach (var target in targets)
            {
                if (target.CorrectsPlacementId.HasValue)
                {
                    if (!query.IncludeCorrections) continue;
                    pairs.Add((target.Id, target.CorrectsPlacementId.Value, RoomTransferEntryKinds.Correction));
                    continue;
                }

                // Pendahulu langsung pada episode yang sama yang berakhir karena Transfer.
                var previous = siblings
                    .Where(x => x.EpisodeId == target.EpisodeId && x.SequenceNumber < target.SequenceNumber)
                    .OrderByDescending(x => x.SequenceNumber)
                    .FirstOrDefault();
                if (previous != null && previous.EndReason == InpBedPlacementEndReason.Transfer)
                    pairs.Add((target.Id, previous.Id, RoomTransferEntryKinds.Transfer));
            }
            if (pairs.Count == 0) return [];

            var siblingById = siblings.ToDictionary(x => x.Id);
            var targetById = targets.ToDictionary(x => x.Id);

            // Master untuk nama kelas, ruang, bed, pasien, dan pencatat.
            var classIds = pairs.SelectMany(p => new[] { siblingById.GetValueOrDefault(p.ToId)?.PatientClassId, siblingById.GetValueOrDefault(p.FromId)?.PatientClassId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var roomIds = pairs.SelectMany(p => new[] { siblingById.GetValueOrDefault(p.ToId)?.RoomId, siblingById.GetValueOrDefault(p.FromId)?.RoomId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var bedIds = pairs.SelectMany(p => new[] { siblingById.GetValueOrDefault(p.ToId)?.BedId, siblingById.GetValueOrDefault(p.FromId)?.BedId })
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();

            var classNames = await _dbContext.MstPatientClasses.AsNoTracking()
                .Where(x => classIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.PatientClassName, cancellationToken);
            var roomNames = await _dbContext.MstRooms.AsNoTracking()
                .Where(x => roomIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.RoomName, cancellationToken);
            var bedNumbers = await _dbContext.MstBeds.AsNoTracking()
                .Where(x => bedIds.Contains(x.Id))
                .Select(x => new { x.Id, Number = x.BedNumber ?? x.BedCode })
                .ToDictionaryAsync(x => x.Id, x => x.Number, cancellationToken);
            var patients = await _dbContext.InpEpisodes.AsNoTracking()
                .Where(x => episodeIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null
                })
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            var recorderIds = targets.Select(x => x.PlacedByUserId).Distinct().ToList();
            var recorders = await _dbContext.Users.AsNoTracking()
                .Where(x => recorderIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            var result = new List<RoomTransferReportRow>();
            foreach (var pair in pairs)
            {
                var to = targetById[pair.ToId];
                if (!siblingById.TryGetValue(pair.FromId, out var from)) continue;

                // Saringan unit dan kelas.
                if (query.FromServiceUnitId.HasValue && from.ServiceUnitId != query.FromServiceUnitId.Value) continue;
                if (query.ToServiceUnitId.HasValue && to.ServiceUnitId != query.ToServiceUnitId.Value) continue;
                if (query.ClassId.HasValue && from.PatientClassId != query.ClassId.Value && to.PatientClassId != query.ClassId.Value)
                    continue;

                patients.TryGetValue(to.EpisodeId, out var patient);
                result.Add(new RoomTransferReportRow
                {
                    PlacementId = to.Id,
                    TransferredAt = to.StartDateTime,
                    MedicalRecordNumber = patient?.MedicalRecordNumber,
                    PatientName = patient?.PatientName ?? string.Empty,
                    FromClassName = classNames.GetValueOrDefault(from.PatientClassId),
                    FromRoomName = roomNames.GetValueOrDefault(from.RoomId),
                    FromBedNumber = bedNumbers.GetValueOrDefault(from.BedId),
                    ToClassName = classNames.GetValueOrDefault(to.PatientClassId),
                    ToRoomName = roomNames.GetValueOrDefault(to.RoomId),
                    ToBedNumber = bedNumbers.GetValueOrDefault(to.BedId),
                    Reason = pair.Kind == RoomTransferEntryKinds.Correction ? to.ChangeReason : to.TransferReason ?? to.ChangeReason,
                    RecordedByName = recorders.GetValueOrDefault(to.PlacedByUserId),
                    EntryKind = pair.Kind
                });
            }

            return [.. result.OrderBy(x => x.TransferredAt).ThenBy(x => x.PatientName)];
        }
    }
}
