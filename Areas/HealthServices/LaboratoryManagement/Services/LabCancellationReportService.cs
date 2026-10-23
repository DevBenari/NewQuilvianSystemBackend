using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Laporan Pembatalan Order &amp; Pemeriksaan (<c>LAB-API-v1</c> <c>r49</c> 44.2, BR-149, BR-151,
    /// <c>02-backend-architecture.md</c> 32.4, <c>BE-LAB-99</c>) — <b>hanya membaca</b>.
    ///
    /// <para>
    /// <b>Satu baris = satu peristiwa</b> <c>Order.Cancel</c> atau <c>Examination.Cancel</c> pada riwayat perpindahan
    /// (<c>INV-58</c>), menurut waktu peristiwa dalam WIB. Status pemeriksaan hari ini <b>tidak</b> dibaca: membatalkan
    /// pesanan tidak mengubah status pemeriksaannya (<c>ARCH-GAP-LAB-16</c>), sehingga baris yang lahir dari status
    /// akan hilang atau ganda. Pembatalan wadah bukan baris — terbaca pada detail.
    /// </para>
    ///
    /// <para>
    /// <b>Memuat identitas pasien</b> (<c>LAB-DEC-240</c>): No. RM dan nama dibaca per halaman lewat kunjungan —
    /// jalur yang sama dengan rincian pesanan (<c>LAB-DEC-166</c>) — tidak disalin, dan tidak pernah dikirim ke log.
    /// </para>
    /// </summary>
    public class LabCancellationReportService
    {
        public const string OrderCancelAction = "Order.Cancel";
        public const string ExaminationCancelAction = "Examination.Cancel";

        /// <summary>Status detail bagi permintaan yang belum masuk wadah.</summary>
        public const string AwaitingSpecimenStatus = "AwaitingSpecimen";

        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public LabCancellationReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LabCancellationReportFilterMetadataResponse> GetFilterMetadataAsync(
            CancellationToken cancellationToken = default)
        {
            var result = LabFilterMetadataFactory.LabCancellationReport();
            result.MaxPageSize = MaxPageSize;
            result.ServiceUnits = await _dbContext.MstServiceUnits
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.ServiceUnitName)
                .Select(x => new LabServiceUnitOptionResponse { Id = x.Id, Code = x.ServiceUnitCode, Name = x.ServiceUnitName })
                .ToListAsync(cancellationToken);
            return result;
        }

        /// <exception cref="LabOperationalReportValidationException"><c>VAL-147</c>..<c>VAL-149</c>.</exception>
        public async Task<LabCancellationReportResponse> GetListAsync(
            LabCancellationReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var periode = LabOperationalReportService.ResolvePeriod(query);
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            var dasar = Saring(Peristiwa(periode.StartUtc, periode.EndUtc), query);

            // Total atas SELURUH hasil penyaring, bukan halaman (RPT-003, AC-332).
            var perJenis = await dasar
                .GroupBy(x => x.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
            var totalOrder = perJenis.Where(x => x.Action == OrderCancelAction).Sum(x => x.Count);
            var totalExam = perJenis.Where(x => x.Action == ExaminationCancelAction).Sum(x => x.Count);

            var halaman = await dasar
                .OrderByDescending(x => x.OccurredAt)
                .ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new LabCancellationReportResponse
            {
                Period = periode.Response,
                Totals = new LabCancellationReportTotals
                {
                    OrderCount = totalOrder,
                    ExaminationCount = totalExam,
                    TotalCount = totalOrder + totalExam
                },
                Items = await LengkapiAsync(halaman, cancellationToken),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalOrder + totalExam
            };
        }

        /// <summary>
        /// Seluruh baris hasil penyaring <b>tanpa halaman</b> untuk unduhan (<c>BE-LAB-100</c>) — penyaring, urutan, dan
        /// pelengkap baris yang SAMA dengan <see cref="GetListAsync"/>, sehingga isi berkas tidak pernah berbeda dari layar
        /// (<c>RPT-002</c>, <c>AC-332</c>). Banyaknya baris dibatasi panjang periode (<c>VAL-149</c>).
        /// </summary>
        /// <exception cref="LabOperationalReportValidationException"><c>VAL-147</c>..<c>VAL-149</c>.</exception>
        public async Task<LabCancellationReportExport> GetAllForExportAsync(
            LabCancellationReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var periode = LabOperationalReportService.ResolvePeriod(query);

            var rows = await Saring(Peristiwa(periode.StartUtc, periode.EndUtc), query)
                .OrderByDescending(x => x.OccurredAt)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var items = await LengkapiAsync(rows, cancellationToken);
            var totalOrder = rows.Count(x => x.Action == OrderCancelAction);
            var totalExam = rows.Count(x => x.Action == ExaminationCancelAction);

            return new LabCancellationReportExport
            {
                Period = periode.Response,
                Totals = new LabCancellationReportTotals
                {
                    OrderCount = totalOrder,
                    ExaminationCount = totalExam,
                    TotalCount = totalOrder + totalExam
                },
                Items = items
            };
        }

        /// <summary>
        /// Detail satu peristiwa: barisnya, pemeriksaan pesanan (termasuk permintaan yang belum masuk wadah), wadahnya,
        /// dan riwayat transisi pesanan lengkap. Bukan peristiwa batal → <see cref="KeyNotFoundException"/> (<c>404</c>).
        /// </summary>
        public async Task<LabCancellationReportDetailResponse> GetDetailAsync(
            Guid historyId,
            CancellationToken cancellationToken = default)
        {
            var baris = await _dbContext.LabTransitionHistories
                .AsNoTracking()
                .Where(h => h.Id == historyId && (h.Action == OrderCancelAction || h.Action == ExaminationCancelAction))
                .Select(Proyeksi)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Peristiwa pembatalan tidak ditemukan.");

            var row = (await LengkapiAsync(new List<Baris> { baris }, cancellationToken))[0];

            var pemeriksaan = await _dbContext.LabExaminations
                .AsNoTracking()
                .Where(e => e.LabOrderId == baris.LabOrderId && !e.IsDelete)
                .OrderBy(e => e.CreateDateTime)
                .Select(e => new LabCancellationExaminationItem
                {
                    LabExaminationId = e.Id,
                    ExaminationName = e.ProcedureNameSnapshot ?? (e.Procedure != null ? e.Procedure.ProcedureName : null),
                    ExaminationStatus = e.ExaminationStatus.ToString()
                })
                .ToListAsync(cancellationToken);

            var belumMasukWadah = await _dbContext.LabOrderedProcedures
                .AsNoTracking()
                .Where(p => p.LabOrderId == baris.LabOrderId && !p.IsDelete && p.OrderedStatus == LabOrderedProcedureStatus.Ordered)
                .OrderBy(p => p.CreateDateTime)
                .Select(p => new LabCancellationExaminationItem
                {
                    LabExaminationId = null,
                    ExaminationName = p.ProcedureNameSnapshot ?? (p.Procedure != null ? p.Procedure.ProcedureName : null),
                    ExaminationStatus = AwaitingSpecimenStatus
                })
                .ToListAsync(cancellationToken);

            var wadah = await _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(s => s.LabOrderId == baris.LabOrderId && !s.IsDelete)
                .OrderBy(s => s.SpecimenSequence)
                .Select(s => new LabCancellationSpecimenItem
                {
                    LabSpecimenId = s.Id,
                    SpecimenBarcode = s.SpecimenBarcode,
                    SpecimenStatus = s.SpecimenStatus.ToString()
                })
                .ToListAsync(cancellationToken);

            var riwayat = await _dbContext.LabTransitionHistories
                .AsNoTracking()
                .Where(h => h.LabOrderId == baris.LabOrderId)
                .OrderBy(h => h.OccurredAt)
                .ThenBy(h => h.Id)
                .Select(h => new { h.Action, h.FromStatus, h.ToStatus, h.ReasonNote, h.ActorUserId, h.OccurredAt })
                .ToListAsync(cancellationToken);
            var namaPelaku = await NamaPelakuAsync(riwayat.Select(h => h.ActorUserId), cancellationToken);

            return new LabCancellationReportDetailResponse
            {
                Row = row,
                Examinations = pemeriksaan.Concat(belumMasukWadah).ToList(),
                Specimens = wadah,
                History = riwayat.Select(h => new LabCancellationHistoryItem
                {
                    Action = h.Action,
                    FromStatus = h.FromStatus,
                    ToStatus = h.ToStatus,
                    Reason = h.ReasonNote,
                    ActorName = namaPelaku.GetValueOrDefault(h.ActorUserId),
                    OccurredAt = h.OccurredAt
                }).ToList()
            };
        }

        /// <summary>Petugas yang membatalkan pada periode itu — pilihan penyaring <i>Dibatalkan oleh</i>.</summary>
        /// <exception cref="LabOperationalReportValidationException"><c>VAL-147</c>..<c>VAL-149</c>.</exception>
        public async Task<List<LabCancelledByOptionResponse>> GetCancelledByOptionsAsync(
            LabOperationalReportQuery query,
            CancellationToken cancellationToken = default)
        {
            var periode = LabOperationalReportService.ResolvePeriod(query);
            var pelaku = await Peristiwa(periode.StartUtc, periode.EndUtc)
                .Select(x => x.ActorUserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            var nama = await NamaPelakuAsync(pelaku, cancellationToken);

            return pelaku
                .Select(id => new LabCancelledByOptionResponse { UserId = id, Name = nama.GetValueOrDefault(id) })
                .OrderBy(x => x.Name ?? "￿")
                .ToList();
        }

        /// <summary>
        /// Penyaring dalam kalimat untuk log baca — <i>"Pemeriksaan — seluruh unit — Patologi Klinik — seluruh
        /// petugas"</i>. Tanpa data pasien maupun alasan (<c>LAB-PERM-v1</c> 19.5).
        /// </summary>
        public async Task<string> DescribeFiltersAsync(LabCancellationReportQuery query, CancellationToken cancellationToken = default)
        {
            var jenis = query.CancellationKind switch
            {
                LabCancellationKind.Order => "Pesanan",
                LabCancellationKind.Examination => "Pemeriksaan",
                _ => "seluruh jenis"
            };
            var unit = query.ServiceUnitId is Guid uid
                ? await _dbContext.MstServiceUnits.AsNoTracking().Where(x => x.Id == uid).Select(x => x.ServiceUnitName).FirstOrDefaultAsync(cancellationToken) ?? "unit tidak dikenal"
                : "seluruh unit";
            var disiplin = query.Discipline is LabDiscipline d ? LabFilterMetadataFactory.LabelDisiplin(d) : "seluruh disiplin";
            var petugas = query.CancelledByUserId is Guid pid
                ? (await NamaPelakuAsync(new[] { pid }, cancellationToken)).GetValueOrDefault(pid) ?? "petugas tidak dikenal"
                : "seluruh petugas";
            return $"{jenis} — {unit} — {disiplin} — {petugas}";
        }

        // =================================================================
        // Pembantu
        // =================================================================

        /// <summary>Peristiwa batal pesanan/pemeriksaan dalam rentang UTC, beserta ruas yang dibaca laporan.</summary>
        private IQueryable<Baris> Peristiwa(DateTime startUtc, DateTime endUtc) =>
            _dbContext.LabTransitionHistories
                .AsNoTracking()
                .Where(h =>
                    (h.Action == OrderCancelAction || h.Action == ExaminationCancelAction) &&
                    h.OccurredAt >= startUtc &&
                    h.OccurredAt < endUtc)
                .Select(Proyeksi);

        /// <summary>
        /// Disiplin baris: disiplin pesanan, lalu disiplin katalog — pemeriksaan bagi baris <c>Examination</c>, pemeriksaan
        /// utama pesanan bagi baris <c>Order</c> (urutan <c>VAL-126</c>).
        /// </summary>
        private static readonly System.Linq.Expressions.Expression<Func<Models.LabTransitionHistory, Baris>> Proyeksi = h => new Baris
        {
            Id = h.Id,
            Action = h.Action,
            OccurredAt = h.OccurredAt,
            LabOrderId = h.LabOrderId,
            LabExaminationId = h.LabExaminationId,
            EncounterId = h.EncounterId,
            FromStatus = h.FromStatus,
            ReasonNote = h.ReasonNote,
            ActorUserId = h.ActorUserId,
            OrderNumber = h.LabOrder != null ? h.LabOrder.OrderNumber : string.Empty,
            Discipline = h.LabOrder!.Discipline ?? (h.LabExaminationId != null
                ? (h.LabExamination!.Procedure != null ? h.LabExamination.Procedure.LabDiscipline : null)
                : (h.LabOrder.Procedure != null ? h.LabOrder.Procedure.LabDiscipline : null)),
            ExaminationName = h.LabExamination != null
                ? (h.LabExamination.ProcedureNameSnapshot ?? (h.LabExamination.Procedure != null ? h.LabExamination.Procedure.ProcedureName : null))
                : null
        };

        private IQueryable<Baris> Saring(IQueryable<Baris> rows, LabCancellationReportQuery query)
        {
            if (query.CancellationKind == LabCancellationKind.Order)
                rows = rows.Where(x => x.Action == OrderCancelAction);
            else if (query.CancellationKind == LabCancellationKind.Examination)
                rows = rows.Where(x => x.Action == ExaminationCancelAction);

            if (query.CancelledByUserId is Guid pelaku)
                rows = rows.Where(x => x.ActorUserId == pelaku);

            if (query.Discipline is LabDiscipline disiplin)
                rows = rows.Where(x => x.Discipline == disiplin);

            if (query.ServiceUnitId is Guid unit)
                rows = rows.Where(x => _dbContext.RegPatientEncounters.Any(e => e.Id == x.EncounterId && e.ServiceUnitId == unit));

            return rows;
        }

        /// <summary>
        /// Melengkapi baris satu halaman: pasien dan unit lewat kunjungan, nama pembatal, dan jumlah pemeriksaan pesanan.
        /// Setiap sumber SATU kueri untuk seluruh halaman (paling banyak 100 baris).
        /// </summary>
        private async Task<List<LabCancellationReportRow>> LengkapiAsync(List<Baris> rows, CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
                return new List<LabCancellationReportRow>();

            var encounterIds = rows.Select(x => x.EncounterId).Distinct().ToList();
            var kunjungan = await _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(e => encounterIds.Contains(e.Id))
                .Select(e => new { e.Id, e.PatientId, e.ServiceUnitId })
                .ToDictionaryAsync(e => e.Id, cancellationToken);

            var patientIds = kunjungan.Values.Select(e => e.PatientId).Distinct().ToList();
            var pasien = await _dbContext.MstPatients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.Id))
                .Select(p => new { p.Id, p.FullName, p.MedicalRecordNumber })
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var unitIds = kunjungan.Values.Select(e => e.ServiceUnitId).Distinct().ToList();
            var unit = await _dbContext.MstServiceUnits
                .AsNoTracking()
                .Where(u => unitIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.ServiceUnitName, cancellationToken);

            var namaPelaku = await NamaPelakuAsync(rows.Select(x => x.ActorUserId), cancellationToken);

            // Baris Order: jumlah pemeriksaan pesanan = pemeriksaan + permintaan yang belum masuk wadah.
            var orderIds = rows.Where(x => x.Action == OrderCancelAction).Select(x => x.LabOrderId).Distinct().ToList();
            var jumlah = new Dictionary<Guid, int>();
            if (orderIds.Count > 0)
            {
                var e1 = await _dbContext.LabExaminations.AsNoTracking()
                    .Where(e => orderIds.Contains(e.LabOrderId) && !e.IsDelete)
                    .GroupBy(e => e.LabOrderId).Select(g => new { g.Key, N = g.Count() })
                    .ToListAsync(cancellationToken);
                var e2 = await _dbContext.LabOrderedProcedures.AsNoTracking()
                    .Where(p => orderIds.Contains(p.LabOrderId) && !p.IsDelete && p.OrderedStatus == LabOrderedProcedureStatus.Ordered)
                    .GroupBy(p => p.LabOrderId).Select(g => new { g.Key, N = g.Count() })
                    .ToListAsync(cancellationToken);
                foreach (var x in e1.Concat(e2))
                    jumlah[x.Key] = jumlah.GetValueOrDefault(x.Key) + x.N;
            }

            return rows.Select(x =>
            {
                var k = kunjungan.GetValueOrDefault(x.EncounterId);
                var p = k is null ? null : pasien.GetValueOrDefault(k.PatientId);
                var isOrder = x.Action == OrderCancelAction;
                return new LabCancellationReportRow
                {
                    HistoryId = x.Id,
                    CancelledAt = x.OccurredAt,
                    Kind = isOrder ? nameof(LabCancellationKind.Order) : nameof(LabCancellationKind.Examination),
                    LabOrderId = x.LabOrderId,
                    OrderNumber = x.OrderNumber,
                    MedicalRecordNumber = p?.MedicalRecordNumber,
                    PatientName = p?.FullName,
                    Discipline = x.Discipline?.ToString(),
                    DisciplineName = x.Discipline is LabDiscipline d ? LabFilterMetadataFactory.LabelDisiplin(d) : "Belum terklasifikasi",
                    LabExaminationId = isOrder ? null : x.LabExaminationId,
                    ExaminationName = isOrder ? null : x.ExaminationName,
                    ExaminationCount = isOrder ? jumlah.GetValueOrDefault(x.LabOrderId) : null,
                    FromStatus = x.FromStatus,
                    WasChargeEligible = isOrder ? null : x.FromStatus == nameof(LabExaminationStatus.ChargeEligible),
                    Reason = string.IsNullOrWhiteSpace(x.ReasonNote) ? null : x.ReasonNote,
                    IsReasonRecorded = !string.IsNullOrWhiteSpace(x.ReasonNote),
                    CancelledByUserId = x.ActorUserId,
                    CancelledByName = namaPelaku.GetValueOrDefault(x.ActorUserId),
                    ServiceUnitName = k is null ? null : unit.GetValueOrDefault(k.ServiceUnitId)
                };
            }).ToList();
        }

        /// <summary>Nama pelaku — jalur yang sama dengan <c>resultEnteredByName</c> (<c>r48</c>).</summary>
        private async Task<Dictionary<Guid, string?>> NamaPelakuAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            var daftar = ids.Distinct().ToList();
            if (daftar.Count == 0)
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => daftar.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode })
                .ToDictionaryAsync(x => x.Id, x => (string?)x.Name, cancellationToken);
        }

        private sealed class Baris
        {
            public Guid Id { get; set; }
            public string Action { get; set; } = string.Empty;
            public DateTime OccurredAt { get; set; }
            public Guid LabOrderId { get; set; }
            public Guid? LabExaminationId { get; set; }
            public Guid EncounterId { get; set; }
            public string? FromStatus { get; set; }
            public string? ReasonNote { get; set; }
            public Guid ActorUserId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public LabDiscipline? Discipline { get; set; }
            public string? ExaminationName { get; set; }
        }
    }
}
