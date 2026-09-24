using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Services
{
    public class AccAccountingEventService
    {
        public const string AlasanJenisBelumTerdaftar = "EVENT_TYPE_NOT_REGISTERED";
        public const string AlasanAturanPostingKosong = "POSTING_RULE_MISSING";
        public const string AlasanKomponenTidakDipakai = "COMPONENT_UNMAPPED";
        public const string AlasanKomponenKurang = "COMPONENT_MISSING";

        public const int BatasCobaUlangTerjadwal = 3;

        private const int UkuranGelombangPenjadwal = 100;

        private static readonly TimeSpan[] JedaCobaUlangTerjadwal =
        {
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15)
        };

        private const int PanjangNomorMaksimum = 50;
        private const int PanjangKodeJenisMaksimum = 50;
        private const int PanjangModulMaksimum = 50;
        private const int PanjangTransaksiMaksimum = 100;
        private const int PanjangVersiMaksimum = 20;
        private const int PanjangKodeKomponenMaksimum = 50;
        private const int PanjangPesanGagalMaksimum = 1000;
        private const int PanjangKeteranganJurnalMaksimum = 500;

        private static readonly TimeSpan ZonaWaktuWib = TimeSpan.FromHours(7);

        private static readonly string[] PenandaDataPasien =
        {
            "patient", "pasien", "medicalrecord", "rekammedis", "mrn",
            "visit", "kunjungan", "encounter", "doctor", "dokter"
        };

        private readonly ApplicationDbContext _db;
        private readonly AccJournalService _journalService;
        private readonly AccAccountingEventSchedulerOptions _options;

        public AccAccountingEventService(
            ApplicationDbContext db,
            AccJournalService journalService,
            IOptions<AccAccountingEventSchedulerOptions> options)
        {
            _db = db;
            _journalService = journalService;
            _options = options.Value;
        }

        public async Task<AccountingServiceResult<PagedResult<AccountingEventListDto>>> GetPagedAsync(
            AccountingEventPagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<AccountingEventListDto>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccAccountingEvent> q = _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.LegalEntityId.HasValue)
                q = q.Where(x => x.LegalEntityId == query.LegalEntityId.Value);

            if (query.EventStatus.HasValue)
                q = q.Where(x => x.EventStatus == query.EventStatus.Value);

            if (!string.IsNullOrWhiteSpace(query.EventTypeCode))
            {
                var kode = query.EventTypeCode.Trim();
                q = q.Where(x => x.EventTypeCode == kode);
            }

            if (!string.IsNullOrWhiteSpace(query.PeriodCode))
            {
                var bulan = TafsirkanKodePeriode(query.PeriodCode);

                if (bulan is null)
                {
                    return AccountingServiceResult<PagedResult<AccountingEventListDto>>.Fail(
                        StatusCodes.Status400BadRequest, "Kode periode harus berbentuk YYYY-MM.");
                }

                var awal = bulan.Value;
                var akhir = awal.AddMonths(1);
                q = q.Where(x => x.AccountingDate >= awal && x.AccountingDate < akhir);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var cari = query.Search.Trim().ToLower();
                q = q.Where(x => x.EventNumber.ToLower().Contains(cari));
            }

            var menurun = !string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "eventnumber" => menurun ? q.OrderByDescending(x => x.EventNumber) : q.OrderBy(x => x.EventNumber),
                "accountingdate" => menurun ? q.OrderByDescending(x => x.AccountingDate) : q.OrderBy(x => x.AccountingDate),
                "amount" => menurun ? q.OrderByDescending(x => x.Amount) : q.OrderBy(x => x.Amount),
                _ => menurun ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime)
            };

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AccountingEventListDto
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    EventNumber = x.EventNumber,
                    EventTypeCode = x.EventTypeCode,
                    EventTypeName = x.EventType != null ? x.EventType.EventTypeName : null,
                    SourceModule = x.SourceModule,
                    AccountingDate = x.AccountingDate,
                    Amount = x.Amount,
                    CurrencyCode = x.CurrencyCode,
                    EventStatus = x.EventStatus,
                    HoldReasonCode = x.HoldReasonCode,
                    JournalId = x.JournalId,
                    JournalNumber = x.Journal != null ? x.Journal.JournalNumber : null,
                    AttemptCount = x.AttemptCount,
                    ReceivedAt = x.CreateDateTime
                })
                .ToListAsync(ct);

            var hasil = new PagedResult<AccountingEventListDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };

            return AccountingServiceResult<PagedResult<AccountingEventListDto>>.Ok(
                hasil, "Daftar kejadian keuangan berhasil diambil.");
        }

        public async Task<AccountingServiceResult<AccountingEventDetailDto>> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingEventDetailDto>(_db, ct);
            if (penjaga is not null) return penjaga;

            var kejadian = await _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Include(x => x.EventType)
                .Include(x => x.Journal)
                    .ThenInclude(j => j!.AccountingPeriod)
                .Include(x => x.Components.Where(c => !c.IsDelete))
                .Include(x => x.Attempts.Where(a => !a.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (kejadian is null)
            {
                return AccountingServiceResult<AccountingEventDetailDto>.Fail(
                    StatusCodes.Status404NotFound, "Kejadian tidak ditemukan.");
            }

            var rincian = new AccountingEventDetailDto
            {
                Id = kejadian.Id,
                LegalEntityId = kejadian.LegalEntityId,
                EventNumber = kejadian.EventNumber,
                EventTypeCode = kejadian.EventTypeCode,
                EventTypeName = kejadian.EventType?.EventTypeName,
                SourceModule = kejadian.SourceModule,
                AccountingDate = kejadian.AccountingDate,
                Amount = kejadian.Amount,
                CurrencyCode = kejadian.CurrencyCode,
                EventStatus = kejadian.EventStatus,
                HoldReasonCode = kejadian.HoldReasonCode,
                JournalId = kejadian.JournalId,
                JournalNumber = kejadian.Journal?.JournalNumber,
                AttemptCount = kejadian.AttemptCount,
                ReceivedAt = kejadian.CreateDateTime,
                SourceTransactionId = kejadian.SourceTransactionId,
                SourceVersion = kejadian.SourceVersion,
                EventOccurredAt = kejadian.EventOccurredAt.ToOffset(ZonaWaktuWib),
                DocumentDate = kejadian.DocumentDate,
                CorrelationId = kejadian.CorrelationId,
                CausationId = kejadian.CausationId,
                IgnoreReason = kejadian.IgnoreReason,
                JournalStatus = kejadian.Journal?.JournalStatus.ToString(),
                JournalPeriodCode = kejadian.Journal?.AccountingPeriod?.PeriodCode,
                RawPayload = kejadian.RawPayload,
                Components = kejadian.Components
                    .OrderBy(x => x.ComponentCode)
                    .Select(x => new AccountingEventComponentDto { ComponentCode = x.ComponentCode, Amount = x.Amount })
                    .ToList(),
                Attempts = kejadian.Attempts
                    .OrderByDescending(x => x.AttemptNumber)
                    .Select(x => new AccountingEventAttemptDto
                    {
                        AttemptNumber = x.AttemptNumber,
                        AttemptedAt = x.AttemptedAt.ToOffset(ZonaWaktuWib),
                        IsSuccess = x.IsSuccess,
                        FailureMessage = x.FailureMessage
                    })
                    .ToList()
            };

            return AccountingServiceResult<AccountingEventDetailDto>.Ok(
                rincian, "Rincian kejadian keuangan berhasil diambil.");
        }

        public async Task<AccountingServiceResult<AccountingEventSummaryDto>> GetSummaryAsync(
            Guid? legalEntityId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingEventSummaryDto>(_db, ct);
            if (penjaga is not null) return penjaga;

            IQueryable<AccAccountingEvent> q = _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (legalEntityId.HasValue)
                q = q.Where(x => x.LegalEntityId == legalEntityId.Value);

            var jumlah = await q
                .GroupBy(x => x.EventStatus)
                .Select(g => new { Status = g.Key, Jumlah = g.Count() })
                .ToListAsync(ct);

            int Hitung(AccountingEventStatus status) => jumlah.FirstOrDefault(x => x.Status == status)?.Jumlah ?? 0;

            var ringkasan = new AccountingEventSummaryDto
            {
                LegalEntityId = legalEntityId,
                Total = jumlah.Sum(x => x.Jumlah),
                Diterima = Hitung(AccountingEventStatus.Diterima),
                Tertahan = Hitung(AccountingEventStatus.Tertahan),
                Gagal = Hitung(AccountingEventStatus.Gagal),
                Terjurnal = Hitung(AccountingEventStatus.Terjurnal),
                Diabaikan = Hitung(AccountingEventStatus.Diabaikan),
                Tercatat = Hitung(AccountingEventStatus.Tercatat)
            };

            return AccountingServiceResult<AccountingEventSummaryDto>.Ok(
                ringkasan, "Ringkasan kejadian keuangan berhasil diambil.");
        }

        private static DateTime? TafsirkanKodePeriode(string kode)
        {
            return DateTime.TryParseExact(
                kode.Trim(), "yyyy-MM",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var bulan)
                ? bulan
                : null;
        }

        public async Task<AccountingServiceResult<AccountingEventDetailDto>> CobaUlangAsync(
            Guid id,
            Guid callerUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingEventDetailDto>(_db, ct);
            if (penjaga is not null) return penjaga;

            var kejadian = await _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new { x.Id, x.EventNumber, x.EventStatus })
                .FirstOrDefaultAsync(ct);

            if (kejadian is null)
            {
                return AccountingServiceResult<AccountingEventDetailDto>.Fail(
                    StatusCodes.Status404NotFound, "Kejadian tidak ditemukan.");
            }

            if (kejadian.EventStatus is not (AccountingEventStatus.Gagal or AccountingEventStatus.Tertahan))
            {
                return AccountingServiceResult<AccountingEventDetailDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Kejadian {kejadian.EventNumber} berstatus {kejadian.EventStatus} dan tidak dapat dicoba ulang. "
                    + "Coba ulang hanya untuk kejadian Gagal atau Tertahan.");
            }

            var nomorTerakhir = await _db.Set<AccAccountingEventAttempt>()
                .Where(x => x.AccountingEventId == id)
                .MaxAsync(x => (int?)x.AttemptNumber, ct) ?? 0;

            var pelaku = _options.SystemActorUserId ?? callerUserId;
            var hasil = await ProsesKejadianAsync(id, kejadian.EventStatus, nomorTerakhir + 1, pelaku, ct);

            var rincian = await GetByIdAsync(id, ct);
            if (!rincian.Success) return rincian;

            var pesan = hasil.EventStatus switch
            {
                nameof(AccountingEventStatus.Terjurnal) =>
                    $"Kejadian {hasil.EventNumber} berhasil dijurnal sebagai {hasil.JournalNumber}.",
                nameof(AccountingEventStatus.Tertahan) =>
                    $"Kejadian {hasil.EventNumber} masih tertahan: {hasil.HoldReasonCode}.",
                _ => $"Coba ulang kejadian {hasil.EventNumber} belum berhasil. Lihat riwayat percobaan."
            };

            rincian.Message = pesan;
            return rincian;
        }

        public async Task<AccountingServiceResult<AccountingEventDetailDto>> AbaikanAsync(
            Guid id,
            IgnoreAccountingEventRequest request,
            Guid callerUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingEventDetailDto>(_db, ct);
            if (penjaga is not null) return penjaga;

            var alasan = request.Reason?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(alasan) || alasan.Length > 500)
            {
                return AccountingServiceResult<AccountingEventDetailDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan mengabaikan kejadian wajib diisi, maksimal 500 karakter.");
            }

            var kejadian = await _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new { x.Id, x.EventNumber, x.EventStatus })
                .FirstOrDefaultAsync(ct);

            if (kejadian is null)
            {
                return AccountingServiceResult<AccountingEventDetailDto>.Fail(
                    StatusCodes.Status404NotFound, "Kejadian tidak ditemukan.");
            }

            if (kejadian.EventStatus != AccountingEventStatus.Gagal)
            {
                var sebab = kejadian.EventStatus == AccountingEventStatus.Tertahan
                    ? "Kejadian Tertahan menunggu aturan posting dan tidak boleh diabaikan. Lengkapi aturannya lalu coba ulang."
                    : $"Hanya kejadian Gagal yang dapat diabaikan; kejadian ini berstatus {kejadian.EventStatus}.";

                return AccountingServiceResult<AccountingEventDetailDto>.Fail(StatusCodes.Status409Conflict, sebab);
            }

            var sekarang = DateTime.UtcNow;

            var berubah = await _db.Set<AccAccountingEvent>()
                .Where(x => x.Id == id && x.EventStatus == AccountingEventStatus.Gagal)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EventStatus, AccountingEventStatus.Diabaikan)
                    .SetProperty(x => x.IgnoreReason, alasan)
                    .SetProperty(x => x.UpdateDateTime, sekarang)
                    .SetProperty(x => x.UpdateBy, callerUserId), ct);

            if (berubah == 0)
            {
                return AccountingServiceResult<AccountingEventDetailDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Status kejadian {kejadian.EventNumber} baru saja berubah. Muat ulang rinciannya.");
            }

            var rincian = await GetByIdAsync(id, ct);
            if (rincian.Success) rincian.Message = $"Kejadian {kejadian.EventNumber} ditandai Diabaikan.";

            return rincian;
        }

        public async Task<AccountingServiceResult<AccountingEventReceiptDto>> TerimaAsync(
            ReceiveAccountingEventRequest request,
            Guid callerUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingEventReceiptDto>(_db, ct);
            if (penjaga is not null) return penjaga;

            var isian = PeriksaIsian(request);
            if (isian is not null) return isian;

            var badanHukum = await PeriksaBadanHukumAsync(request.LegalEntityId!.Value, ct);
            if (badanHukum is not null) return badanHukum;

            var nomor = request.EventNumber!.Trim();
            var kodeJenis = request.EventTypeCode!.Trim();
            var modul = request.SourceModule!.Trim();
            var transaksi = request.SourceTransactionId!.Trim();
            var versi = request.SourceVersion!.Trim();

            var sudahAda = await CariKirimanUlangAsync(nomor, modul, transaksi, kodeJenis, versi, ct);
            if (sudahAda.HasValue) return await BalasKirimanUlangAsync(sudahAda.Value, ct);

            var jenis = await _db.Set<AccEventType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.EventTypeCode == kodeJenis, ct);

            var menurutJenis = PeriksaIsianMenurutJenis(request, jenis);
            if (menurutJenis is not null) return menurutJenis;

            var sekarang = DateTime.UtcNow;

            var kejadian = new AccAccountingEvent
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId!.Value,
                EventNumber = nomor,
                EventTypeId = jenis?.Id,
                EventTypeCode = kodeJenis,
                SourceModule = modul,
                SourceTransactionId = transaksi,
                SourceVersion = versi,
                EventOccurredAt = request.EventOccurredAt!.Value.ToUniversalTime(),
                AccountingDate = request.AccountingDate!.Value.Date,
                DocumentDate = request.AccountingDate!.Value.Date,
                Amount = request.Amount!.Value,
                CurrencyCode = AccAccountingEvent.MataUangRupiah,
                EventStatus = AccountingEventStatus.Diterima,
                RawPayload = JsonSerializer.Serialize(request),
                CorrelationId = request.CorrelationId!.Value,
                CausationId = request.CausationId!.Value,
                CreateDateTime = sekarang,
                CreateBy = callerUserId
            };

            foreach (var komponen in request.Components ?? new List<AccountingEventComponentRequest>())
            {
                kejadian.Components.Add(new AccAccountingEventComponent
                {
                    Id = Guid.NewGuid(),
                    AccountingEventId = kejadian.Id,
                    ComponentCode = komponen.ComponentCode!.Trim(),
                    Amount = komponen.Amount!.Value,
                    CreateDateTime = sekarang,
                    CreateBy = callerUserId
                });
            }

            _db.Set<AccAccountingEvent>().Add(kejadian);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException exception) when (MelanggarUniqueIndex(exception))
            {
                _db.ChangeTracker.Clear();

                var pemenang = await CariKirimanUlangAsync(nomor, modul, transaksi, kodeJenis, versi, ct);
                if (pemenang.HasValue) return await BalasKirimanUlangAsync(pemenang.Value, ct);

                throw;
            }

            var pelaku = _options.SystemActorUserId ?? callerUserId;
            var tandaTerima = await ProsesKejadianAsync(
                kejadian.Id, AccountingEventStatus.Diterima, nomorPercobaan: 1, pelaku, ct);

            return BalasKejadianBaru(tandaTerima);
        }

        public async Task<AccountingEventReceiptDto> ProsesKejadianAsync(
            Guid accountingEventId,
            AccountingEventStatus statusAsal,
            int nomorPercobaan,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            _db.ChangeTracker.Clear();

            var kejadian = await _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Include(x => x.Components.Where(c => !c.IsDelete))
                .FirstAsync(x => x.Id == accountingEventId, ct);

            if (kejadian.EventStatus != statusAsal)
                return await PetakanTandaTerimaAsync(accountingEventId, ct);

            try
            {
                var jenis = await _db.Set<AccEventType>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => !x.IsDelete && x.IsActive && x.EventTypeCode == kejadian.EventTypeCode, ct);

                if (jenis is null)
                {
                    return await TahanAsync(
                        kejadian, statusAsal, null, AlasanJenisBelumTerdaftar,
                        $"Jenis kejadian {kejadian.EventTypeCode} belum terdaftar.",
                        nomorPercobaan, actorUserId, ct);
                }

                var aturan = await AccPostingRuleService.CariAturanAktifAsync(
                    _db, kejadian.LegalEntityId, jenis.Id, ct);

                if (aturan is null)
                {
                    return await TahanAsync(
                        kejadian, statusAsal, jenis.Id, AlasanAturanPostingKosong,
                        $"Jenis kejadian {jenis.EventTypeCode} belum punya aturan posting aktif.",
                        nomorPercobaan, actorUserId, ct);
                }

                var nilaiKomponen = kejadian.Components
                    .ToDictionary(x => x.ComponentCode, x => x.Amount, StringComparer.Ordinal);

                var komponenAturan = aturan.Lines
                    .Select(x => x.ComponentCode)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                var tidakDipakai = nilaiKomponen.Keys.Where(x => !komponenAturan.Contains(x)).ToList();

                if (tidakDipakai.Count > 0)
                {
                    return await TahanAsync(
                        kejadian, statusAsal, jenis.Id, AlasanKomponenTidakDipakai,
                        $"Komponen {string.Join(", ", tidakDipakai)} tidak dipakai aturan posting.",
                        nomorPercobaan, actorUserId, ct);
                }

                var kurang = komponenAturan
                    .Where(x => x != AccPostingRuleLine.KomponenTotal && !nilaiKomponen.ContainsKey(x))
                    .ToList();

                if (kurang.Count > 0)
                {
                    return await TahanAsync(
                        kejadian, statusAsal, jenis.Id, AlasanKomponenKurang,
                        $"Aturan posting menuntut komponen {string.Join(", ", kurang)} yang tidak dibawa kejadian.",
                        nomorPercobaan, actorUserId, ct);
                }

                var tanggal = await TentukanTanggalAkuntansiAsync(
                    kejadian.LegalEntityId, kejadian.AccountingDate, aturan.JournalTypeId, ct);

                if (tanggal.Alasan is not null)
                    throw new PemrosesanKejadianException(tanggal.Alasan);

                var permintaan = SusunPermintaanJurnal(kejadian, aturan, nilaiKomponen, tanggal.Tanggal!.Value);

                await using var transaksi = await _db.Database.BeginTransactionAsync(ct);

                var jurnal = await _journalService.CreateAsync(permintaan, actorUserId, null, ct);
                if (!jurnal.Success) throw new PemrosesanKejadianException(jurnal.Message);

                if (aturan.Treatment == AccountingEventTreatment.LangsungSahkan)
                {
                    var sah = await _journalService.SahkanDariKejadianAsync(jurnal.Data!.Id, actorUserId, ct);
                    if (!sah.Success) throw new PemrosesanKejadianException(sah.Message);
                }

                var sekarang = DateTime.UtcNow;
                var idJurnal = jurnal.Data!.Id;

                var berubah = await _db.Set<AccAccountingEvent>()
                    .Where(x => x.Id == kejadian.Id && x.EventStatus == statusAsal)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.EventStatus, AccountingEventStatus.Terjurnal)
                        .SetProperty(x => x.EventTypeId, jenis.Id)
                        .SetProperty(x => x.JournalId, idJurnal)
                        .SetProperty(x => x.HoldReasonCode, (string?)null)
                        .SetProperty(x => x.UpdateDateTime, sekarang)
                        .SetProperty(x => x.UpdateBy, actorUserId), ct);

                if (berubah == 0)
                {
                    await transaksi.RollbackAsync(ct);
                    _db.ChangeTracker.Clear();
                    return await PetakanTandaTerimaAsync(kejadian.Id, ct);
                }

                TambahPercobaan(kejadian.Id, nomorPercobaan, true, null, actorUserId, sekarang);
                await _db.SaveChangesAsync(ct);
                await transaksi.CommitAsync(ct);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _db.ChangeTracker.Clear();
                await CatatPercobaanGagalAsync(kejadian.Id, nomorPercobaan, exception.Message, actorUserId, ct);
            }

            _db.ChangeTracker.Clear();
            return await PetakanTandaTerimaAsync(kejadian.Id, ct);
        }

        public async Task<AccountingEventRetryCycleResult> CobaUlangTerjadwalAsync(
            DateTime sekarangUtc,
            CancellationToken ct = default)
        {
            var hasil = new AccountingEventRetryCycleResult();
            var tenggang = TimeSpan.FromSeconds(Math.Max(0, _options.GracePeriodSeconds));
            var pelaku = _options.SystemActorUserId ?? Guid.Empty;
            var sekarang = new DateTimeOffset(DateTime.SpecifyKind(sekarangUtc, DateTimeKind.Utc));

            _db.ChangeTracker.Clear();

            var kandidat = await _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.EventStatus == AccountingEventStatus.Diterima)
                .OrderBy(x => x.CreateDateTime)
                .Take(UkuranGelombangPenjadwal)
                .Select(x => new
                {
                    x.Id,
                    x.EventNumber,
                    x.AttemptCount,
                    x.CreateDateTime,
                    TerakhirDicoba = x.Attempts.Max(a => (DateTimeOffset?)a.AttemptedAt),
                    NomorTerakhir = x.Attempts.Max(a => (int?)a.AttemptNumber)
                })
                .ToListAsync(ct);

            foreach (var k in kandidat)
            {
                var dasar = k.TerakhirDicoba
                            ?? new DateTimeOffset(DateTime.SpecifyKind(k.CreateDateTime, DateTimeKind.Utc));
                var jeda = k.AttemptCount < JedaCobaUlangTerjadwal.Length
                    ? JedaCobaUlangTerjadwal[k.AttemptCount]
                    : TimeSpan.Zero;

                if (dasar + (jeda > tenggang ? jeda : tenggang) > sekarang) continue;

                hasil.Considered++;

                try
                {
                    if (k.AttemptCount >= BatasCobaUlangTerjadwal)
                    {
                        if (await TandaiGagalTerjadwalAsync(k.Id, pelaku, ct)) hasil.MarkedFailed++;
                        else hasil.Skipped++;
                        continue;
                    }

                    var hitunganBaru = k.AttemptCount + 1;
                    var waktuKlaim = DateTime.UtcNow;

                    var diklaim = await _db.Set<AccAccountingEvent>()
                        .Where(x => x.Id == k.Id
                                    && x.EventStatus == AccountingEventStatus.Diterima
                                    && x.AttemptCount == k.AttemptCount)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.AttemptCount, hitunganBaru)
                            .SetProperty(x => x.UpdateDateTime, waktuKlaim)
                            .SetProperty(x => x.UpdateBy, pelaku), ct);

                    if (diklaim == 0)
                    {
                        hasil.Skipped++;
                        continue;
                    }

                    var tanda = await ProsesKejadianAsync(
                        k.Id, AccountingEventStatus.Diterima, (k.NomorTerakhir ?? 0) + 1, pelaku, ct);

                    if (tanda.EventStatus == nameof(AccountingEventStatus.Terjurnal))
                    {
                        hasil.Journaled++;
                    }
                    else if (tanda.EventStatus == nameof(AccountingEventStatus.Tertahan))
                    {
                        hasil.Held++;
                    }
                    else if (tanda.EventStatus != nameof(AccountingEventStatus.Diterima))
                    {
                        hasil.Skipped++;
                    }
                    else if (hitunganBaru >= BatasCobaUlangTerjadwal
                             && await TandaiGagalTerjadwalAsync(k.Id, pelaku, ct))
                    {
                        hasil.MarkedFailed++;
                    }
                    else
                    {
                        hasil.StillPending++;
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _db.ChangeTracker.Clear();
                    hasil.Errors.Add($"{k.EventNumber}: {exception.Message}");
                }
            }

            return hasil;
        }

        private async Task<bool> TandaiGagalTerjadwalAsync(Guid accountingEventId, Guid actorUserId, CancellationToken ct)
        {
            var sekarang = DateTime.UtcNow;

            var berubah = await _db.Set<AccAccountingEvent>()
                .Where(x => x.Id == accountingEventId && x.EventStatus == AccountingEventStatus.Diterima)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EventStatus, AccountingEventStatus.Gagal)
                    .SetProperty(x => x.UpdateDateTime, sekarang)
                    .SetProperty(x => x.UpdateBy, actorUserId), ct);

            return berubah > 0;
        }

        private async Task<AccountingEventReceiptDto> TahanAsync(
            AccAccountingEvent kejadian,
            AccountingEventStatus statusAsal,
            Guid? eventTypeId,
            string kodeAlasan,
            string pesan,
            int nomorPercobaan,
            Guid actorUserId,
            CancellationToken ct)
        {
            await using var transaksi = await _db.Database.BeginTransactionAsync(ct);

            var sekarang = DateTime.UtcNow;

            var berubah = await _db.Set<AccAccountingEvent>()
                .Where(x => x.Id == kejadian.Id && x.EventStatus == statusAsal)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EventStatus, AccountingEventStatus.Tertahan)
                    .SetProperty(x => x.EventTypeId, eventTypeId)
                    .SetProperty(x => x.HoldReasonCode, kodeAlasan)
                    .SetProperty(x => x.UpdateDateTime, sekarang)
                    .SetProperty(x => x.UpdateBy, actorUserId), ct);

            if (berubah > 0)
            {
                TambahPercobaan(kejadian.Id, nomorPercobaan, false, pesan, actorUserId, sekarang);
                await _db.SaveChangesAsync(ct);
                await transaksi.CommitAsync(ct);
            }
            else
            {
                await transaksi.RollbackAsync(ct);
            }

            _db.ChangeTracker.Clear();
            return await PetakanTandaTerimaAsync(kejadian.Id, ct);
        }

        private async Task CatatPercobaanGagalAsync(
            Guid accountingEventId,
            int nomorPercobaan,
            string pesan,
            Guid actorUserId,
            CancellationToken ct)
        {
            var sudahTercatat = await _db.Set<AccAccountingEventAttempt>()
                .AnyAsync(x => x.AccountingEventId == accountingEventId && x.AttemptNumber == nomorPercobaan, ct);

            if (sudahTercatat) return;

            TambahPercobaan(accountingEventId, nomorPercobaan, false, pesan, actorUserId, DateTime.UtcNow);
            await _db.SaveChangesAsync(ct);
        }

        private void TambahPercobaan(
            Guid accountingEventId,
            int nomorPercobaan,
            bool berhasil,
            string? pesan,
            Guid actorUserId,
            DateTime sekarang)
        {
            _db.Set<AccAccountingEventAttempt>().Add(new AccAccountingEventAttempt
            {
                Id = Guid.NewGuid(),
                AccountingEventId = accountingEventId,
                AttemptNumber = nomorPercobaan,
                AttemptedAt = new DateTimeOffset(sekarang, TimeSpan.Zero),
                IsSuccess = berhasil,
                FailureMessage = pesan is null
                    ? null
                    : pesan.Length > PanjangPesanGagalMaksimum ? pesan[..PanjangPesanGagalMaksimum] : pesan,
                CreateDateTime = sekarang,
                CreateBy = actorUserId
            });
        }

        private async Task<(DateTime? Tanggal, string? Alasan)> TentukanTanggalAkuntansiAsync(
            Guid legalEntityId,
            DateTime tanggalKejadian,
            Guid journalTypeId,
            CancellationToken ct)
        {
            var tanggal = tanggalKejadian.Date;

            var kodeJenisJurnal = await _db.Set<AccJournalType>()
                .AsNoTracking()
                .Where(x => x.Id == journalTypeId)
                .Select(x => x.JournalTypeCode)
                .FirstOrDefaultAsync(ct) ?? string.Empty;

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete
                         && x.LegalEntityId == legalEntityId
                         && x.StartDate <= tanggal
                         && x.EndDate >= tanggal, ct);

            if (periode is null)
            {
                return (null, $"Belum ada periode akuntansi untuk tanggal {tanggal:yyyy-MM-dd}. "
                              + "Minta administrator membangkitkan periode tahun buku ini.");
            }

            if (AccAccountingPeriodService.AlasanPenolakanJenisJurnal(
                    periode.PeriodStatus, periode.PeriodCode, kodeJenisJurnal) is null)
            {
                return (tanggal, null);
            }

            var sesudahnya = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityId == legalEntityId && x.StartDate > periode.EndDate)
                .OrderBy(x => x.StartDate)
                .ToListAsync(ct);

            var terbuka = sesudahnya.FirstOrDefault(x => AccAccountingPeriodService.AlasanPenolakanJenisJurnal(
                x.PeriodStatus, x.PeriodCode, kodeJenisJurnal) is null);

            return terbuka is null
                ? (null, $"Periode {periode.PeriodCode} tidak menerima jurnal ini dan belum ada periode terbuka sesudahnya.")
                : (terbuka.StartDate.Date, null);
        }

        private static CreateJournalRequest SusunPermintaanJurnal(
            AccAccountingEvent kejadian,
            AccPostingRule aturan,
            IReadOnlyDictionary<string, decimal> nilaiKomponen,
            DateTime tanggalAkuntansi)
        {
            var keterangan = $"Kejadian {kejadian.EventNumber} ({kejadian.EventTypeCode}) — transaksi asal {kejadian.SourceTransactionId}";

            var baris = new List<CreateJournalLineRequest>();
            var nomorBaris = 1;

            foreach (var aturanBaris in aturan.Lines.OrderBy(x => x.LineNumber))
            {
                var nilai = aturanBaris.ComponentCode == AccPostingRuleLine.KomponenTotal
                    ? kejadian.Amount
                    : nilaiKomponen[aturanBaris.ComponentCode];

                if (nilai <= 0m) continue;

                baris.Add(new CreateJournalLineRequest
                {
                    LineNumber = nomorBaris++,
                    AccountId = aturanBaris.AccountId,
                    CostCenterId = aturanBaris.CostCenterId,
                    Description = aturanBaris.Description,
                    DebitAmount = aturanBaris.Side == PostingSide.Debit ? nilai : 0m,
                    CreditAmount = aturanBaris.Side == PostingSide.Kredit ? nilai : 0m
                });
            }

            return new CreateJournalRequest
            {
                LegalEntityId = kejadian.LegalEntityId,
                JournalTypeId = aturan.JournalTypeId,
                DocumentNumber = kejadian.EventNumber,
                DocumentDate = kejadian.DocumentDate,
                AccountingDate = tanggalAkuntansi,
                Description = keterangan.Length > PanjangKeteranganJurnalMaksimum
                    ? keterangan[..PanjangKeteranganJurnalMaksimum]
                    : keterangan,
                Lines = baris
            };
        }

        private async Task<Guid?> CariKirimanUlangAsync(
            string nomor,
            string modul,
            string transaksi,
            string kodeJenis,
            string versi,
            CancellationToken ct)
        {
            var id = await _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Where(x => x.EventNumber == nomor
                            || (x.SourceModule == modul
                                && x.SourceTransactionId == transaksi
                                && x.EventTypeCode == kodeJenis
                                && x.SourceVersion == versi))
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(ct);

            return id;
        }

        private async Task<AccountingServiceResult<AccountingEventReceiptDto>> BalasKirimanUlangAsync(
            Guid accountingEventId,
            CancellationToken ct)
        {
            var tandaTerima = await PetakanTandaTerimaAsync(accountingEventId, ct);

            return AccountingServiceResult<AccountingEventReceiptDto>.Ok(
                tandaTerima,
                $"Kejadian {tandaTerima.EventNumber} sudah pernah diterima. Tidak ada jurnal baru yang dibuat.",
                StatusCodes.Status200OK);
        }

        private static AccountingServiceResult<AccountingEventReceiptDto> BalasKejadianBaru(
            AccountingEventReceiptDto tandaTerima)
        {
            if (tandaTerima.EventStatus == nameof(AccountingEventStatus.Tertahan))
            {
                return new AccountingServiceResult<AccountingEventReceiptDto>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status422UnprocessableEntity,
                    Message = $"Kejadian {tandaTerima.EventNumber} diterima tetapi ditahan ({tandaTerima.HoldReasonCode}). "
                              + "Tidak ada jurnal yang dibuat. Accounting akan melengkapi aturannya lalu memproses ulang.",
                    Data = tandaTerima
                };
            }

            var pesan = tandaTerima.JournalNumber is null
                ? $"Kejadian {tandaTerima.EventNumber} diterima. Penjurnalan tertunda dan akan dicoba ulang otomatis."
                : $"Kejadian {tandaTerima.EventNumber} diterima dan dijurnal sebagai {tandaTerima.JournalNumber}.";

            return AccountingServiceResult<AccountingEventReceiptDto>.Ok(
                tandaTerima, pesan, StatusCodes.Status201Created);
        }

        private async Task<AccountingEventReceiptDto> PetakanTandaTerimaAsync(
            Guid accountingEventId,
            CancellationToken ct)
        {
            var data = await _db.Set<AccAccountingEvent>()
                .AsNoTracking()
                .Where(x => x.Id == accountingEventId)
                .Select(x => new
                {
                    x.Id,
                    x.EventNumber,
                    x.EventStatus,
                    x.HoldReasonCode,
                    x.CreateDateTime,
                    JournalNumber = x.Journal != null ? x.Journal.JournalNumber : null,
                    PeriodCode = x.Journal != null && x.Journal.AccountingPeriod != null
                        ? x.Journal.AccountingPeriod.PeriodCode
                        : null
                })
                .FirstAsync(ct);

            return new AccountingEventReceiptDto
            {
                AccountingEventId = data.Id,
                EventNumber = data.EventNumber,
                EventStatus = data.EventStatus.ToString(),
                JournalNumber = data.JournalNumber,
                AccountingPeriodCode = data.PeriodCode,
                HoldReasonCode = data.EventStatus == AccountingEventStatus.Tertahan ? data.HoldReasonCode : null,
                ReceivedAt = new DateTimeOffset(DateTime.SpecifyKind(data.CreateDateTime, DateTimeKind.Utc))
                    .ToOffset(ZonaWaktuWib)
            };
        }

        private static AccountingServiceResult<AccountingEventReceiptDto>? PeriksaIsian(
            ReceiveAccountingEventRequest request)
        {
            var kosong = new List<string>();

            if (string.IsNullOrWhiteSpace(request.EventNumber)) kosong.Add(nameof(request.EventNumber));
            if (string.IsNullOrWhiteSpace(request.EventTypeCode)) kosong.Add(nameof(request.EventTypeCode));
            if (string.IsNullOrWhiteSpace(request.SourceModule)) kosong.Add(nameof(request.SourceModule));
            if (string.IsNullOrWhiteSpace(request.SourceTransactionId)) kosong.Add(nameof(request.SourceTransactionId));
            if (string.IsNullOrWhiteSpace(request.SourceVersion)) kosong.Add(nameof(request.SourceVersion));
            if (!request.EventOccurredAt.HasValue) kosong.Add(nameof(request.EventOccurredAt));
            if (!request.AccountingDate.HasValue) kosong.Add(nameof(request.AccountingDate));
            if (!request.Amount.HasValue) kosong.Add(nameof(request.Amount));
            if (string.IsNullOrWhiteSpace(request.CurrencyCode)) kosong.Add(nameof(request.CurrencyCode));
            if (request.LegalEntityId is null || request.LegalEntityId == Guid.Empty) kosong.Add(nameof(request.LegalEntityId));
            if (request.CorrelationId is null || request.CorrelationId == Guid.Empty) kosong.Add(nameof(request.CorrelationId));
            if (request.CausationId is null || request.CausationId == Guid.Empty) kosong.Add(nameof(request.CausationId));

            if (kosong.Count > 0)
            {
                return Gagal(
                    StatusCodes.Status400BadRequest,
                    $"Pesan kejadian tidak lengkap. Bidang berikut wajib diisi: {string.Join(", ", kosong)}.");
            }

            var dataPasien = (request.AdditionalFields?.Keys ?? Enumerable.Empty<string>())
                .Where(nama => PenandaDataPasien.Any(penanda =>
                    nama.Replace("_", string.Empty).Contains(penanda, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (dataPasien.Count > 0)
                return Gagal(StatusCodes.Status400BadRequest, "Pesan kejadian tidak boleh memuat identitas pasien.");

            var terlaluPanjang = new List<string>();

            if (request.EventNumber!.Trim().Length > PanjangNomorMaksimum) terlaluPanjang.Add(nameof(request.EventNumber));
            if (request.EventTypeCode!.Trim().Length > PanjangKodeJenisMaksimum) terlaluPanjang.Add(nameof(request.EventTypeCode));
            if (request.SourceModule!.Trim().Length > PanjangModulMaksimum) terlaluPanjang.Add(nameof(request.SourceModule));
            if (request.SourceTransactionId!.Trim().Length > PanjangTransaksiMaksimum) terlaluPanjang.Add(nameof(request.SourceTransactionId));
            if (request.SourceVersion!.Trim().Length > PanjangVersiMaksimum) terlaluPanjang.Add(nameof(request.SourceVersion));

            if (terlaluPanjang.Count > 0)
            {
                return Gagal(
                    StatusCodes.Status400BadRequest,
                    $"Isian berikut melebihi panjang yang diizinkan: {string.Join(", ", terlaluPanjang)}.");
            }

            if (!string.Equals(request.CurrencyCode!.Trim(), AccAccountingEvent.MataUangRupiah, StringComparison.Ordinal))
                return Gagal(StatusCodes.Status409Conflict, "Sistem akuntansi hanya menerima rupiah.");

            var komponen = request.Components ?? new List<AccountingEventComponentRequest>();

            if (komponen.Any(x => string.IsNullOrWhiteSpace(x.ComponentCode)
                                  || x.ComponentCode!.Trim().Length > PanjangKodeKomponenMaksimum
                                  || !x.Amount.HasValue
                                  || x.Amount.Value < 0m))
            {
                return Gagal(
                    StatusCodes.Status400BadRequest,
                    "Setiap rincian komponen wajib punya kode (maksimal 50 karakter) dan nilai yang tidak negatif.");
            }

            var kembar = komponen
                .GroupBy(x => x.ComponentCode!.Trim(), StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (kembar.Count > 0)
            {
                return Gagal(
                    StatusCodes.Status400BadRequest,
                    $"Komponen berikut dikirim lebih dari sekali: {string.Join(", ", kembar)}.");
            }

            return null;
        }

        private static AccountingServiceResult<AccountingEventReceiptDto>? PeriksaIsianMenurutJenis(
            ReceiveAccountingEventRequest request,
            AccEventType? jenis)
        {
            if (jenis?.EventKind == EventTypeKind.SaldoSubledger)
            {
                return Gagal(
                    StatusCodes.Status409Conflict,
                    "Pesan saldo subledger belum dapat diterima. Jalurnya dibangun pada BE-ACC-P2-028.");
            }

            var membawaSaldo = request.SubledgerBalance is not null;

            if (jenis is not null && membawaSaldo)
            {
                return Gagal(
                    StatusCodes.Status400BadRequest,
                    "Rincian saldo subledger hanya dan wajib ada pada pesan saldo subledger.");
            }

            if (membawaSaldo && (request.Components?.Count ?? 0) > 0)
            {
                return Gagal(
                    StatusCodes.Status400BadRequest,
                    "Pesan saldo subledger tidak boleh membawa rincian komponen.");
            }

            if (!membawaSaldo && request.Amount!.Value <= 0m)
                return Gagal(StatusCodes.Status400BadRequest, "Nilai kejadian harus lebih besar dari nol.");

            return null;
        }

        private async Task<AccountingServiceResult<AccountingEventReceiptDto>?> PeriksaBadanHukumAsync(
            Guid legalEntityId,
            CancellationToken ct)
        {
            var ada = await _db.Set<MstLegalEntity>()
                .AnyAsync(x => x.Id == legalEntityId && !x.IsDelete && x.IsActive, ct);

            if (!ada) return Gagal(StatusCodes.Status422UnprocessableEntity, "Badan hukum tidak ditemukan.");

            var utama = await AccountingLegalEntityGuard.AmbilBadanHukumUtamaAsync(_db, ct);

            return utama == legalEntityId
                ? null
                : Gagal(StatusCodes.Status403Forbidden,
                    "Badan hukum yang dituju bukan badan hukum yang dilayani Accounting saat ini.");
        }

        private static bool MelanggarUniqueIndex(DbUpdateException exception)
            => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

        private static AccountingServiceResult<AccountingEventReceiptDto> Gagal(int statusCode, string pesan)
            => AccountingServiceResult<AccountingEventReceiptDto>.Fail(statusCode, pesan);

        private sealed class PemrosesanKejadianException : Exception
        {
            public PemrosesanKejadianException(string pesan) : base(pesan)
            {
            }
        }
    }
}
