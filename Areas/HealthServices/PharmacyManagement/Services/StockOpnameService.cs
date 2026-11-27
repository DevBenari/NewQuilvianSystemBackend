using System.Data.Common;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>Izin Stock Opname (resource <c>StockOpname</c>).</summary>
public static class StockOpnamePermissions
{
    public const string Resource = "StockOpname";

    /// <summary>Melihat sesi, lembar, dan berita acara.</summary>
    public const string Read = "Read";

    /// <summary>Membuat sesi, mengisi hasil hitung, mencatat temuan, mengajukan, dan membatalkan.</summary>
    public const string Create = "Create";

    /// <summary>Menyetujui (sekaligus memposting penyesuaian) atau menolak. Harus diberikan eksplisit.</summary>
    public const string Approve = "Approve";
}

/// <summary>Deret nomor sesi Stock Opname pada alokator nomor bersama (QBE-CODE-005, DEC-PLT-004).</summary>
public static class StockOpnameNumberSeries
{
    public const string SequenceKey = "PHM_STOCK_OPNAME";
    public const string Prefix = "SOP";
    public const string ResetPolicy = NumberSeriesResetPolicies.Never;
    public const int Digits = 6;
}

/// <summary>Kode galat Stock Opname. PHM036–PHM038 dipakai bersama hitung selisih (FIN-LGS-084).</summary>
public static class StockOpnameErrorCodes
{
    public const string Location = "PHM036";
    public const string InvalidCount = "PHM037";
    public const string SchemaUnavailable = "PHM039";
    public const string NotFound = "PHM051";
    public const string InvalidState = "PHM052";
    public const string ActiveSessionExists = "PHM053";
    public const string NotReadyToSubmit = "PHM054";
    public const string BalanceChanged = "PHM055";
    public const string NotAuthorizedToDecide = "PHM056";
    public const string VersionConflict = "PHM057";
    public const string InvalidRequest = "PHM058";
    public const string InvalidFinding = "PHM059";
    public const string SelfApproval = "PHM067";
}

/// <summary>Tabel Stock Opname belum ada di basis data (migrasi belum dijalankan); dipetakan ke <c>503</c>.</summary>
public sealed class StockOpnameSchemaUnavailableException() : Exception(DefaultMessage)
{
    public const string DefaultMessage =
        "Penyimpanan Stock Opname belum tersedia: tabel PhmStockOpname dan PhmStockOpnameLine belum dibuat " +
        "di basis data (migrasi belum dijalankan). Gunakan mode terbatas.";

    public string Code => StockOpnameErrorCodes.SchemaUnavailable;
}

public sealed class StockOpnameNotFoundException(string message) : Exception(message)
{
    public string Code => StockOpnameErrorCodes.NotFound;
}

public sealed class StockOpnameConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class StockOpnameForbiddenException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class StockOpnameUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>
/// Mengenali galat basis data karena tabel atau kolom Stock Opname belum ada.
/// </summary>
/// <remarks>
/// PostgreSQL: <c>42P01</c> (undefined_table) dan <c>42703</c> (undefined_column). SQLite (uji):
/// pesan <c>no such table</c>/<c>no such column</c>. Dibaca lewat <see cref="DbException"/> supaya
/// proyek utama tidak bergantung pada pustaka SQLite.
/// </remarks>
internal static class StockOpnameSchemaGuard
{
    public static bool IsMissingSchema(Exception? exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is not DbException db) continue;
            if (db.SqlState is "42P01" or "42703") return true;
            if (db.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase) ||
                db.Message.Contains("no such column", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

/// <summary>
/// Transaksi Stock Opname Logistik (FIN-LGS-091): sesi dan detail perhitungan.
/// </summary>
/// <remarks>
/// <para>
/// Aturan yang disetujui pemilik (FIN-LGS-088): gudang tidak dibekukan, sehingga baris yang saldonya
/// berubah atau bergerak sejak snapshot wajib dihitung ulang sebelum keputusan; penyetuju memegang izin
/// <c>StockOpname:Approve</c> secara eksplisit dan bukan pembuat, pengaju, maupun penghitung sesi;
/// temuan tanpa saldo tidak menambah stok; toleransi selisih 0; berita acara hanya dari sesi selesai;
/// penyesuaian diposting sekali, bersamaan dengan persetujuan, dalam satu transaksi.
/// </para>
/// <para>
/// Setiap koreksi saldo tercatat di kartu stok lewat <see cref="DrugStockService"/> (mutasi
/// <c>Adjustment</c> bersumber <c>StockOpname</c>). Posting ganda dicegah tiga lapis: status dan versi
/// sesi, pemeriksaan mutasi bersumber sesi ini, dan indeks unik <c>(SourceDocumentType, CorrelationId)</c>.
/// </para>
/// <para>
/// Sebelum migrasi tabel dijalankan, semua operasi menjawab <see cref="StockOpnameSchemaUnavailableException"/>
/// (BLOCKED_SCHEMA) alih-alih galat basis data mentah.
/// </para>
/// </remarks>
public sealed class StockOpnameService
{
    private const string LogCategory = "PharmacyManagement";
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;
    private const int ShownLineNumbers = 10;

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly DrugStockService _drugStockService;
    private readonly NumberSeriesAllocator _numberSeries;
    private readonly StockRequestApprovalAuthority _authority;

    public StockOpnameService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService, DrugStockService drugStockService, NumberSeriesAllocator numberSeries)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _drugStockService = drugStockService;
        _numberSeries = numberSeries;
        // Pemeriksaan izin eksplisit yang sama dengan persetujuan permintaan stok: tanpa jalan pintas
        // SuperAdmin, hanya penempatan akses yang sedang berlaku.
        _authority = new StockRequestApprovalAuthority(dbContext);
    }

    // ====================================================================== kesiapan

    /// <summary>
    /// Apakah tabel Stock Opname sudah ada. Halaman memakai mode terbatas bila belum.
    /// </summary>
    public async Task<StockOpnameAvailabilityResponse> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Membaca seluruh kolom, sehingga kolom yang belum dimigrasi juga terdeteksi.
            await _dbContext.PhmStockOpnames.AsNoTracking().Take(1).ToListAsync(cancellationToken);
            await _dbContext.PhmStockOpnameLines.AsNoTracking().Take(1).ToListAsync(cancellationToken);

            return new StockOpnameAvailabilityResponse
            {
                IsPersistenceReady = true,
                Status = StockOpnameAvailabilityStatuses.Ready,
                Message = "Penyimpanan Stock Opname siap dipakai."
            };
        }
        catch (Exception ex) when (StockOpnameSchemaGuard.IsMissingSchema(ex))
        {
            return new StockOpnameAvailabilityResponse
            {
                IsPersistenceReady = false,
                Status = StockOpnameAvailabilityStatuses.BlockedSchema,
                Message = StockOpnameSchemaUnavailableException.DefaultMessage
            };
        }
    }

    // ====================================================================== baca

    public Task<PagedResult<StockOpnameSummaryResponse>> GetListAsync(StockOpnameListQuery query,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var sessions = _dbContext.PhmStockOpnames.AsNoTracking().Where(x => !x.IsDelete);

            if (query.StorageLocationId is { } locationId && locationId != Guid.Empty)
                sessions = sessions.Where(x => x.StorageLocationId == locationId);
            if (query.Status is { } status)
                sessions = sessions.Where(x => x.Status == status);

            var search = query.Search?.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
                sessions = sessions.Where(x => x.OpnameNumber.ToLower().Contains(search) ||
                                               x.StorageLocationNameSnapshot.ToLower().Contains(search));

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > MaxPageSize ? DefaultPageSize : query.PageSize;
            var total = await sessions.CountAsync(cancellationToken);

            var rows = await sessions
                .OrderByDescending(x => x.CreateDateTime)
                .ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Session = x,
                    LineCount = x.Lines.Count(l => !l.IsDelete && l.LineType == StockOpnameLineType.Balance),
                    CountedLineCount = x.Lines.Count(l =>
                        !l.IsDelete && l.LineType == StockOpnameLineType.Balance && l.PhysicalQuantity != null),
                    FindingCount = x.Lines.Count(l => !l.IsDelete && l.LineType == StockOpnameLineType.Finding)
                })
                .ToListAsync(cancellationToken);

            var names = await LoadUserNamesAsync(rows.Select(x => (Guid?)x.Session.CreateBy), cancellationToken);

            return new PagedResult<StockOpnameSummaryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = rows.Select(x =>
                {
                    var summary = new StockOpnameSummaryResponse();
                    FillSummary(summary, x.Session, x.LineCount, x.CountedLineCount, x.FindingCount, names);
                    return summary;
                }).ToList()
            };
        });

    /// <summary>Detail sesi beserta baris; null bila tidak ada.</summary>
    public Task<StockOpnameDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(() => BuildDetailAsync(id, cancellationToken));

    /// <summary>Berita acara; hanya untuk sesi berstatus Selesai.</summary>
    public Task<StockOpnameReportResponse> GetReportAsync(Guid id, CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var detail = await BuildDetailAsync(id, cancellationToken) ?? throw NotFound();
            if (detail.Status != StockOpnameStatus.Completed)
                throw new StockOpnameConflictException(StockOpnameErrorCodes.InvalidState,
                    "Berita acara hanya tersedia untuk sesi yang sudah selesai (disetujui dan penyesuaiannya diposting).");

            return new StockOpnameReportResponse
            {
                StockOpnameId = detail.Id,
                DocumentNumber = detail.OpnameNumber,
                StorageLocationName = detail.StorageLocationName,
                StockStatusScope = detail.StockStatusScope,
                OpnameDate = detail.OpnameDate,
                CompletedAt = detail.DecidedAt ?? detail.CreatedAt,
                CreatedByUserId = detail.CreatedByUserId,
                CreatedByName = detail.CreatedByName,
                CountedByNames = detail.Lines
                    .Select(x => x.CountedByName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToList(),
                ApprovedByUserId = detail.DecidedByUserId ?? Guid.Empty,
                ApprovedByName = detail.DecidedByName,
                DecisionReason = detail.DecisionReason,
                Summary = detail.Summary,
                AdjustmentCount = detail.Lines.Count(x => x.AdjustmentMutationId != null),
                DifferenceLines = detail.Lines
                    .Where(x => x.LineType == StockOpnameLineType.Balance && x.VarianceQuantity is { } v && v != 0m)
                    .ToList(),
                Findings = detail.Lines.Where(x => x.LineType == StockOpnameLineType.Finding).ToList()
            };
        });

    // ====================================================================== buat

    /// <summary>
    /// Membuat sesi dan memotret seluruh saldo lokasi (sesuai cakupan status, termasuk saldo nol)
    /// sebagai baris hitung. Satu sesi aktif per lokasi.
    /// </summary>
    public Task<StockOpnameDetailResponse> CreateAsync(CreateStockOpnameRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();

            if (request.StorageLocationId is not { } locationId || locationId == Guid.Empty)
                throw Unprocessable(StockOpnameErrorCodes.Location, "Lokasi penyimpanan wajib dipilih.");

            var location = await _dbContext.MstDrugStorageLocations.AsNoTracking()
                .Where(x => x.Id == locationId && !x.IsDelete)
                .Select(x => new { x.Id, x.StorageLocationName })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw Unprocessable(StockOpnameErrorCodes.Location, "Lokasi penyimpanan tidak terdaftar.");

            if (request.StockStatusScope is { } scope && !Enum.IsDefined(scope))
                throw Unprocessable(StockOpnameErrorCodes.InvalidRequest, "Cakupan status stok tidak dikenal.");

            if (request.PreviousStockOpnameId is { } previousId)
            {
                var previous = await _dbContext.PhmStockOpnames.AsNoTracking()
                    .Where(x => x.Id == previousId && !x.IsDelete)
                    .Select(x => new { x.Status, x.StorageLocationId })
                    .FirstOrDefaultAsync(cancellationToken);
                if (previous == null || previous.Status != StockOpnameStatus.Rejected ||
                    previous.StorageLocationId != locationId)
                    throw Unprocessable(StockOpnameErrorCodes.InvalidRequest,
                        "Sesi sebelumnya harus sesi yang ditolak pada lokasi yang sama.");
            }

            var activeExists = await _dbContext.PhmStockOpnames.AsNoTracking().AnyAsync(x =>
                x.StorageLocationId == locationId && !x.IsDelete &&
                (x.Status == StockOpnameStatus.Counting || x.Status == StockOpnameStatus.Submitted),
                cancellationToken);
            if (activeExists) throw ActiveSessionConflict(location.StorageLocationName);

            var balanceQuery = _dbContext.PhmDrugStockBalances.AsNoTracking()
                .Where(x => x.StorageLocationId == locationId && !x.IsDelete);
            if (request.StockStatusScope is { } statusScope)
                balanceQuery = balanceQuery.Where(x => x.Status == statusScope);

            var balances = await balanceQuery
                .Select(x => new { x.Id, x.DrugId, x.DrugBatchId, x.Status, x.QuantityOnHand, x.Version })
                .ToListAsync(cancellationToken);

            if (balances.Count > StockOpnameCalculator.MaxLines)
                throw Unprocessable(StockOpnameErrorCodes.InvalidRequest,
                    $"Lokasi ini punya {balances.Count} baris saldo, melebihi batas {StockOpnameCalculator.MaxLines} " +
                    "baris satu sesi. Buat sesi per status stok.");

            var drugs = await LoadDrugsAsync(balances.Select(x => x.DrugId), cancellationToken);
            var batches = await LoadBatchesAsync(balances.Select(x => x.DrugBatchId), cancellationToken);

            var now = DateTime.UtcNow;
            var opnameNumber = await _numberSeries.AllocateAsync(new NumberAllocationRequest(
                StockOpnameNumberSeries.SequenceKey, StockOpnameNumberSeries.Prefix,
                StockOpnameNumberSeries.ResetPolicy, StockOpnameNumberSeries.Digits, actor,
                DateTimeOffset.UtcNow), cancellationToken);

            var session = new PhmStockOpname
            {
                OpnameNumber = opnameNumber,
                StorageLocationId = locationId,
                StorageLocationNameSnapshot = Truncate(location.StorageLocationName, 200),
                StockStatusScope = request.StockStatusScope,
                OpnameDate = request.OpnameDate ?? DateOnly.FromDateTime(DateTime.Now),
                Status = StockOpnameStatus.Counting,
                PreviousStockOpnameId = request.PreviousStockOpnameId,
                Notes = Normalize(request.Notes, 500),
                Version = 1,
                CreateBy = actor,
                CreateDateTime = now
            };

            var ordered = balances
                .Select(x => new
                {
                    Balance = x,
                    Drug = drugs.GetValueOrDefault(x.DrugId),
                    Batch = batches.GetValueOrDefault(x.DrugBatchId)
                })
                .OrderBy(x => x.Drug?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Batch?.ExpiryDate == null)
                .ThenBy(x => x.Batch?.ExpiryDate)
                .ThenBy(x => x.Balance.Status)
                .ThenBy(x => x.Batch?.BatchNumber, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var lineNumber = 0;
            foreach (var item in ordered)
            {
                session.Lines.Add(new PhmStockOpnameLine
                {
                    StockOpnameId = session.Id,
                    LineNumber = ++lineNumber,
                    LineType = StockOpnameLineType.Balance,
                    DrugStockBalanceId = item.Balance.Id,
                    DrugId = item.Balance.DrugId,
                    DrugBatchId = item.Balance.DrugBatchId,
                    StockStatus = item.Balance.Status,
                    DrugCodeSnapshot = Truncate(item.Drug?.Code ?? string.Empty, 50),
                    DrugNameSnapshot = Truncate(item.Drug?.Name ?? string.Empty, 200),
                    StockUnitNameSnapshot = TruncateOrNull(item.Drug?.StockUnitName, 50),
                    BatchNumberSnapshot = TruncateOrNull(item.Batch?.BatchNumber, 100),
                    ExpiryDateSnapshot = item.Batch?.ExpiryDate,
                    SnapshotQuantity = item.Balance.QuantityOnHand,
                    SnapshotVersion = item.Balance.Version,
                    SnapshotTakenAt = now,
                    CreateBy = actor,
                    CreateDateTime = now
                });
            }

            _dbContext.PhmStockOpnames.Add(session);
            await SaveAsync(location.StorageLocationName, cancellationToken);

            await AuditAsync("Create",
                $"Membuat Stock Opname {session.OpnameNumber} untuk {session.StorageLocationNameSnapshot} " +
                $"({session.Lines.Count} baris saldo).", actor, session,
                new { LineCount = session.Lines.Count, session.StockStatusScope, session.PreviousStockOpnameId });

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    // ====================================================================== hitung

    /// <summary>Menyimpan hasil hitung baris saldo yang dikirim; baris lain tidak tersentuh.</summary>
    public Task<StockOpnameDetailResponse> SaveCountsAsync(Guid id, SaveStockOpnameCountsRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Counting, "Hasil hitung hanya dapat diubah selama sesi dihitung.");
            EnsureVersion(session, request.ExpectedVersion);

            var entries = request.Counts ?? [];
            if (entries.Count == 0)
                throw Unprocessable(StockOpnameErrorCodes.InvalidCount, "Tidak ada hasil hitung yang dikirim.");
            if (entries.Count > StockOpnameCalculator.MaxLines)
                throw Unprocessable(StockOpnameErrorCodes.InvalidCount,
                    $"Paling banyak {StockOpnameCalculator.MaxLines} baris sekali simpan.");

            var lines = session.Lines.ToDictionary(x => x.Id);
            var seen = new HashSet<Guid>();
            var now = DateTime.UtcNow;
            var changed = 0;

            foreach (var entry in entries)
            {
                if (entry?.LineId is not { } lineId || lineId == Guid.Empty || !lines.TryGetValue(lineId, out var line))
                    throw Unprocessable(StockOpnameErrorCodes.InvalidCount, "Ada baris yang tidak dikenali pada sesi ini.");
                if (!seen.Add(lineId))
                    throw Unprocessable(StockOpnameErrorCodes.InvalidCount,
                        $"Baris {line.LineNumber}: dikirim lebih dari sekali.");
                if (line.LineType != StockOpnameLineType.Balance)
                    throw Unprocessable(StockOpnameErrorCodes.InvalidCount,
                        $"Baris {line.LineNumber}: temuan diubah dengan menghapus dan mencatatnya kembali.");

                ValidateQuantity(line.LineNumber, entry.PhysicalQuantity, allowZero: true);

                var note = Normalize(entry.Note, 500);
                if (line.PhysicalQuantity == entry.PhysicalQuantity && line.Note == note) continue;

                line.PhysicalQuantity = entry.PhysicalQuantity;
                line.Note = note;
                line.CountedByUserId = entry.PhysicalQuantity is null ? null : actor;
                line.CountedAt = entry.PhysicalQuantity is null ? null : now;
                Touch(line, actor, now);
                changed++;
            }

            if (changed > 0)
            {
                session.Version++;
                Touch(session, actor, now);
                await SaveAsync(null, cancellationToken);
                await AuditAsync("Count", $"Menyimpan hasil hitung {changed} baris Stock Opname {session.OpnameNumber}.",
                    actor, session, new { ChangedLineCount = changed });
            }

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    /// <summary>
    /// Memotret ulang satu baris saldo yang berubah. Hasil hitung baris itu dikosongkan karena diambil
    /// pada saldo yang sudah tidak berlaku; baris lain tidak tersentuh.
    /// </summary>
    public Task<StockOpnameDetailResponse> RefreshLineAsync(Guid id, Guid lineId, StockOpnameCommandRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Counting, "Baris hanya dapat dimuat ulang selama sesi dihitung.");
            EnsureVersion(session, request.ExpectedVersion);

            var line = session.Lines.FirstOrDefault(x => x.Id == lineId)
                ?? throw Unprocessable(StockOpnameErrorCodes.InvalidRequest, "Baris tidak ditemukan pada sesi ini.");
            if (line.LineType != StockOpnameLineType.Balance || line.DrugStockBalanceId is not { } balanceId)
                throw Unprocessable(StockOpnameErrorCodes.InvalidRequest, "Hanya baris saldo yang dapat dimuat ulang.");

            var balance = await _dbContext.PhmDrugStockBalances.AsNoTracking()
                .Where(x => x.Id == balanceId && !x.IsDelete)
                .Select(x => new { x.QuantityOnHand, x.Version })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw Unprocessable(StockOpnameErrorCodes.InvalidRequest,
                    $"Baris {line.LineNumber}: saldonya sudah tidak ada. Batalkan sesi ini dan buat sesi baru.");

            var now = DateTime.UtcNow;
            var before = line.SnapshotQuantity;
            line.SnapshotQuantity = balance.QuantityOnHand;
            line.SnapshotVersion = balance.Version;
            line.SnapshotTakenAt = now;
            line.PhysicalQuantity = null;
            line.VarianceQuantity = null;
            line.CountedByUserId = null;
            line.CountedAt = null;
            Touch(line, actor, now);

            session.Version++;
            Touch(session, actor, now);
            await SaveAsync(null, cancellationToken);

            await AuditAsync("RefreshLine",
                $"Memuat ulang baris {line.LineNumber} Stock Opname {session.OpnameNumber} ({before} → {balance.QuantityOnHand}).",
                actor, session, new { line.LineNumber, PreviousSnapshot = before, NewSnapshot = balance.QuantityOnHand });

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    /// <summary>Mencatat temuan tanpa saldo. Temuan tidak pernah diposting dan tidak menambah stok.</summary>
    public Task<StockOpnameDetailResponse> AddFindingAsync(Guid id, AddStockOpnameFindingRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Counting, "Temuan hanya dapat dicatat selama sesi dihitung.");
            EnsureVersion(session, request.ExpectedVersion);

            if (request.DrugId is not { } drugId || drugId == Guid.Empty)
                throw Unprocessable(StockOpnameErrorCodes.InvalidFinding, "Obat atau barang temuan wajib dipilih.");
            if (request.StockStatus is not { } stockStatus || !Enum.IsDefined(stockStatus))
                throw Unprocessable(StockOpnameErrorCodes.InvalidFinding, "Status stok temuan wajib dipilih.");
            if (request.PhysicalQuantity is not { } quantity || quantity <= 0m)
                throw Unprocessable(StockOpnameErrorCodes.InvalidFinding, "Jumlah temuan wajib lebih dari nol.");
            ValidateQuantity(0, quantity, allowZero: false, code: StockOpnameErrorCodes.InvalidFinding);

            var drug = (await LoadDrugsAsync([drugId], cancellationToken)).GetValueOrDefault(drugId)
                ?? throw Unprocessable(StockOpnameErrorCodes.InvalidFinding, "Obat atau barang temuan tidak terdaftar.");

            string? batchNumber;
            DateOnly? expiry;
            if (request.DrugBatchId is { } batchId && batchId != Guid.Empty)
            {
                var batch = await _dbContext.PhmDrugBatches.AsNoTracking()
                    .Where(x => x.Id == batchId && !x.IsDelete)
                    .Select(x => new { x.DrugId, x.BatchNumber, x.ExpiryDate })
                    .FirstOrDefaultAsync(cancellationToken);
                if (batch == null || batch.DrugId != drugId)
                    throw Unprocessable(StockOpnameErrorCodes.InvalidFinding, "Batch temuan bukan milik obat yang dipilih.");

                // Batch × status yang punya baris saldo di sesi ini harus dihitung di baris itu.
                if (session.Lines.Any(x => x.LineType == StockOpnameLineType.Balance &&
                                           x.DrugBatchId == batchId && x.StockStatus == stockStatus))
                    throw Unprocessable(StockOpnameErrorCodes.InvalidFinding,
                        "Batch dan status ini sudah punya baris saldo di lembar hitung; isi stok fisiknya di baris itu.");

                batchNumber = batch.BatchNumber;
                expiry = batch.ExpiryDate;
            }
            else
            {
                batchNumber = Normalize(request.BatchNumber, 100);
                expiry = request.ExpiryDate;
            }

            var duplicate = session.Lines.Any(x => x.LineType == StockOpnameLineType.Finding &&
                                                   x.DrugId == drugId && x.StockStatus == stockStatus &&
                                                   x.DrugBatchId == request.DrugBatchId &&
                                                   string.Equals(x.BatchNumberSnapshot, batchNumber,
                                                       StringComparison.OrdinalIgnoreCase));
            if (duplicate)
                throw Unprocessable(StockOpnameErrorCodes.InvalidFinding,
                    "Temuan yang sama sudah dicatat; hapus temuan itu bila jumlahnya perlu diubah.");

            var lastLineNumber = await _dbContext.PhmStockOpnameLines.AsNoTracking()
                .Where(x => x.StockOpnameId == session.Id)
                .Select(x => (int?)x.LineNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var now = DateTime.UtcNow;
            var finding = new PhmStockOpnameLine
            {
                StockOpnameId = session.Id,
                LineNumber = lastLineNumber + 1,
                LineType = StockOpnameLineType.Finding,
                DrugId = drugId,
                DrugBatchId = request.DrugBatchId is { } givenBatch && givenBatch != Guid.Empty ? givenBatch : null,
                StockStatus = stockStatus,
                DrugCodeSnapshot = Truncate(drug.Code, 50),
                DrugNameSnapshot = Truncate(drug.Name, 200),
                StockUnitNameSnapshot = TruncateOrNull(drug.StockUnitName, 50),
                BatchNumberSnapshot = batchNumber,
                ExpiryDateSnapshot = expiry,
                SnapshotQuantity = 0m,
                SnapshotTakenAt = now,
                PhysicalQuantity = quantity,
                Note = Normalize(request.Note, 500),
                CountedByUserId = actor,
                CountedAt = now,
                CreateBy = actor,
                CreateDateTime = now
            };
            _dbContext.PhmStockOpnameLines.Add(finding);

            session.Version++;
            Touch(session, actor, now);
            await SaveAsync(null, cancellationToken);

            await AuditAsync("AddFinding",
                $"Mencatat temuan baris {finding.LineNumber} pada Stock Opname {session.OpnameNumber}; tidak menambah stok.",
                actor, session, new { finding.LineNumber, DrugId = drugId, finding.PhysicalQuantity });

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    /// <summary>Menghapus (soft delete) temuan yang salah catat selama sesi dihitung.</summary>
    public Task<StockOpnameDetailResponse> RemoveFindingAsync(Guid id, Guid lineId, int? expectedVersion,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Counting, "Temuan hanya dapat dihapus selama sesi dihitung.");
            EnsureVersion(session, expectedVersion);

            var line = session.Lines.FirstOrDefault(x => x.Id == lineId && x.LineType == StockOpnameLineType.Finding)
                ?? throw Unprocessable(StockOpnameErrorCodes.InvalidFinding, "Temuan tidak ditemukan pada sesi ini.");

            var now = DateTime.UtcNow;
            line.IsDelete = true;
            line.DeleteBy = actor;
            line.DeleteDateTime = now;
            Touch(line, actor, now);

            session.Version++;
            Touch(session, actor, now);
            await SaveAsync(null, cancellationToken);

            await AuditAsync("RemoveFinding",
                $"Menghapus temuan baris {line.LineNumber} pada Stock Opname {session.OpnameNumber}.",
                actor, session, new { line.LineNumber });

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    // ====================================================================== ajukan / batal

    /// <summary>
    /// Mengajukan sesi: semua baris saldo sudah dihitung dan tidak ada yang berubah sejak snapshot.
    /// Selisih ditetapkan dan seluruh baris dikunci.
    /// </summary>
    public Task<StockOpnameDetailResponse> SubmitAsync(Guid id, StockOpnameCommandRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Counting, "Hanya sesi yang sedang dihitung yang dapat diajukan.");
            EnsureVersion(session, request.ExpectedVersion);

            var lines = session.Lines.ToList();
            var live = await EvaluateLiveAsync(session.StorageLocationId, lines, cancellationToken);
            var balanceLines = lines.Where(x => x.LineType == StockOpnameLineType.Balance).ToList();

            var notCounted = balanceLines.Where(x => x.PhysicalQuantity is null).Select(x => x.LineNumber).ToList();
            var changed = balanceLines
                .Where(x => live.TryGetValue(x.Id, out var state) && state.Changed)
                .Select(x => x.LineNumber)
                .ToList();

            if (notCounted.Count > 0 || changed.Count > 0)
            {
                var parts = new List<string>();
                if (notCounted.Count > 0) parts.Add($"baris {FormatLineNumbers(notCounted)} belum dihitung");
                if (changed.Count > 0)
                    parts.Add($"saldo baris {FormatLineNumbers(changed)} berubah sejak dihitung dan harus dimuat ulang");
                throw Unprocessable(StockOpnameErrorCodes.NotReadyToSubmit,
                    $"Sesi belum dapat diajukan: {string.Join("; ", parts)}.");
            }

            var now = DateTime.UtcNow;
            foreach (var line in lines)
            {
                line.VarianceQuantity = line.LineType == StockOpnameLineType.Balance
                    ? line.PhysicalQuantity!.Value - line.SnapshotQuantity
                    : line.PhysicalQuantity;
                Touch(line, actor, now);
            }

            session.Status = StockOpnameStatus.Submitted;
            session.SubmittedByUserId = actor;
            session.SubmittedAt = now;
            session.Version++;
            Touch(session, actor, now);
            await SaveAsync(null, cancellationToken);

            var differences = balanceLines.Count(x => x.VarianceQuantity is { } v && Math.Abs(v) > StockOpnameCalculator.VarianceTolerance);
            await AuditAsync("Submit",
                $"Mengajukan Stock Opname {session.OpnameNumber} ({differences} baris berselisih).",
                actor, session, new { DifferenceCount = differences });

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    /// <summary>Membatalkan sesi yang belum diajukan; lokasi kembali bebas untuk sesi baru.</summary>
    public Task<StockOpnameDetailResponse> CancelAsync(Guid id, StockOpnameCommandRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Counting, "Hanya sesi yang belum diajukan yang dapat dibatalkan.");
            EnsureVersion(session, request.ExpectedVersion);

            var now = DateTime.UtcNow;
            session.Status = StockOpnameStatus.Cancelled;
            session.IsCancel = true;
            session.CancelBy = actor;
            session.CancelDateTime = now;
            session.Version++;
            Touch(session, actor, now);
            await SaveAsync(null, cancellationToken);

            await AuditAsync("Cancel", $"Membatalkan Stock Opname {session.OpnameNumber}.", actor, session);

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    // ====================================================================== keputusan

    /// <summary>
    /// Menyetujui sesi dan memposting setiap selisih ke kartu stok dalam satu transaksi. Bila ada saldo
    /// yang berubah sejak dihitung, tidak ada yang tersimpan (409 <c>PHM055</c>); sesi harus ditolak dan
    /// dihitung ulang dengan sesi baru.
    /// </summary>
    public Task<StockOpnameDetailResponse> ApproveAsync(Guid id, DecideStockOpnameRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Submitted, "Hanya sesi yang sudah diajukan yang dapat disetujui.");
            EnsureVersion(session, request.ExpectedVersion);
            await EnsureCanDecideAsync(session, actor, cancellationToken);

            var alreadyPosted = await _dbContext.PhmDrugStockMutations.AsNoTracking().AnyAsync(x =>
                x.SourceDocumentType == DrugStockSourceDocumentTypes.StockOpname &&
                x.SourceDocumentId == session.Id && !x.IsDelete, cancellationToken);
            if (alreadyPosted)
                throw new StockOpnameConflictException(StockOpnameErrorCodes.InvalidState,
                    "Penyesuaian sesi ini sudah pernah diposting.");

            var balanceLines = session.Lines
                .Where(x => x.LineType == StockOpnameLineType.Balance)
                .OrderBy(x => x.LineNumber)
                .ToList();
            var balanceIds = balanceLines.Select(x => x.DrugStockBalanceId!.Value).Distinct().ToList();

            var ownTransaction = _dbContext.Database.CurrentTransaction is null;
            await using var transaction = ownTransaction
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;
            var posted = 0;

            try
            {
                var balances = await _dbContext.PhmDrugStockBalances
                    .Where(x => balanceIds.Contains(x.Id) && !x.IsDelete)
                    .ToDictionaryAsync(x => x.Id, cancellationToken);
                var latest = await LoadLatestMovementsAsync(session.StorageLocationId, balanceLines, cancellationToken);

                var changed = balanceLines
                    .Where(x => !balances.TryGetValue(x.DrugStockBalanceId!.Value, out var balance) ||
                                balance.QuantityOnHand != x.SnapshotQuantity ||
                                MovedAfterSnapshot(latest, x))
                    .Select(x => x.LineNumber)
                    .ToList();
                if (changed.Count > 0)
                    throw new StockOpnameConflictException(StockOpnameErrorCodes.BalanceChanged,
                        $"Saldo baris {FormatLineNumbers(changed)} berubah sejak dihitung. Persetujuan dibatalkan tanpa " +
                        "perubahan apa pun; tolak sesi ini lalu hitung ulang dengan sesi baru.");

                var now = DateTime.UtcNow;
                foreach (var line in balanceLines)
                {
                    var variance = line.VarianceQuantity ?? 0m;
                    if (Math.Abs(variance) <= StockOpnameCalculator.VarianceTolerance) continue;

                    var mutation = await _drugStockService.ApplyStockOpnameAdjustmentAsync(
                        balances[line.DrugStockBalanceId!.Value], variance, BuildAdjustmentReason(session, line),
                        session.Id, $"opname-{line.Id:N}", cancellationToken);

                    line.AdjustmentMutationId = mutation.Id;
                    Touch(line, actor, now);
                    posted++;
                }

                session.Status = StockOpnameStatus.Completed;
                session.DecidedByUserId = actor;
                session.DecidedAt = now;
                session.DecisionReason = Normalize(request.Reason, 500);
                session.Version++;
                Touch(session, actor, now);

                await SaveAsync(null, cancellationToken);
                if (transaction != null) await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                // Perubahan saldo dan mutasi yang belum tersimpan tidak boleh terbawa ke operasi lain.
                _dbContext.ChangeTracker.Clear();
                throw;
            }

            await AuditAsync("Approve",
                $"Menyetujui Stock Opname {session.OpnameNumber}; {posted} penyesuaian diposting ke kartu stok.",
                actor, session, new { AdjustmentCount = posted });

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    /// <summary>Menolak sesi dengan alasan wajib. Final: hitung ulang memakai sesi baru.</summary>
    public Task<StockOpnameDetailResponse> RejectAsync(Guid id, DecideStockOpnameRequest request,
        CancellationToken cancellationToken = default) =>
        GuardSchemaAsync(async () =>
        {
            var actor = GetCurrentUserId();
            var session = await LoadForUpdateAsync(id, cancellationToken);
            EnsureStatus(session, StockOpnameStatus.Submitted, "Hanya sesi yang sudah diajukan yang dapat ditolak.");
            EnsureVersion(session, request.ExpectedVersion);

            var reason = Normalize(request.Reason, 500)
                ?? throw Unprocessable(StockOpnameErrorCodes.InvalidRequest, "Alasan penolakan wajib diisi.");

            await EnsureCanDecideAsync(session, actor, cancellationToken);

            var now = DateTime.UtcNow;
            session.Status = StockOpnameStatus.Rejected;
            session.DecidedByUserId = actor;
            session.DecidedAt = now;
            session.DecisionReason = reason;
            session.Version++;
            Touch(session, actor, now);
            await SaveAsync(null, cancellationToken);

            await AuditAsync("Reject", $"Menolak Stock Opname {session.OpnameNumber}: {reason}", actor, session,
                new { Reason = reason });

            return (await BuildDetailAsync(session.Id, cancellationToken))!;
        });

    // ====================================================================== detail dan evaluasi

    private sealed record LiveState(decimal? Current, bool Changed);

    private sealed record DrugInfo(string Code, string Name, string? StockUnitName);

    private sealed record BatchInfo(string BatchNumber, DateOnly? ExpiryDate);

    private async Task<StockOpnameDetailResponse?> BuildDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await _dbContext.PhmStockOpnames.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        if (session == null) return null;

        var lines = await _dbContext.PhmStockOpnameLines.AsNoTracking()
            .Where(x => x.StockOpnameId == id && !x.IsDelete)
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        var active = session.Status is StockOpnameStatus.Counting or StockOpnameStatus.Submitted;
        var live = active
            ? await EvaluateLiveAsync(session.StorageLocationId, lines, cancellationToken)
            : new Dictionary<Guid, LiveState>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var lineResponses = lines.Select(x => ToLineResponse(session, x, active ? live : null, today)).ToList();

        var names = await LoadUserNamesAsync(
            new[] { (Guid?)session.CreateBy, session.SubmittedByUserId, session.DecidedByUserId }
                .Concat(lines.Select(x => x.CountedByUserId)),
            cancellationToken);
        foreach (var line in lineResponses)
            line.CountedByName = line.CountedByUserId is { } counter ? names.GetValueOrDefault(counter) : null;

        var balanceLines = lineResponses.Where(x => x.LineType == StockOpnameLineType.Balance).ToList();
        var detail = new StockOpnameDetailResponse
        {
            PreviousStockOpnameId = session.PreviousStockOpnameId,
            Notes = session.Notes,
            SubmittedByUserId = session.SubmittedByUserId,
            SubmittedByName = session.SubmittedByUserId is { } submitter ? names.GetValueOrDefault(submitter) : null,
            DecidedByUserId = session.DecidedByUserId,
            DecidedByName = session.DecidedByUserId is { } decider ? names.GetValueOrDefault(decider) : null,
            DecisionReason = session.DecisionReason,
            Summary = StockOpnameCalculator.SummarizeResults(balanceLines.Select(x => x.Result).ToList()),
            Lines = lineResponses,
            IsPersisted = true
        };
        FillSummary(detail, session, balanceLines.Count, balanceLines.Count(x => x.PhysicalQuantity != null),
            lineResponses.Count - balanceLines.Count, names);

        detail.CanEdit = session.Status == StockOpnameStatus.Counting;
        detail.CanCancel = session.Status == StockOpnameStatus.Counting;
        if (session.Status == StockOpnameStatus.Counting)
        {
            detail.SubmitBlockedReason = DescribeSubmitBlock(balanceLines);
            detail.CanSubmit = detail.SubmitBlockedReason is null;
        }

        if (session.Status == StockOpnameStatus.Submitted)
        {
            detail.DecisionBlockedReason = await DescribeDecisionBlockAsync(session, lines, cancellationToken);
            detail.CanDecide = detail.DecisionBlockedReason is null;
        }

        return detail;
    }

    private static StockOpnameLineResponse ToLineResponse(PhmStockOpname session, PhmStockOpnameLine line,
        IReadOnlyDictionary<Guid, LiveState>? live, DateOnly today)
    {
        var response = new StockOpnameLineResponse
        {
            Id = line.Id,
            LineNumber = line.LineNumber,
            LineType = line.LineType,
            DrugStockBalanceId = line.DrugStockBalanceId,
            DrugId = line.DrugId,
            DrugCode = line.DrugCodeSnapshot,
            DrugName = line.DrugNameSnapshot,
            StockUnitName = line.StockUnitNameSnapshot,
            DrugBatchId = line.DrugBatchId,
            BatchNumber = line.BatchNumberSnapshot,
            ExpiryDate = line.ExpiryDateSnapshot,
            IsExpired = line.ExpiryDateSnapshot is { } expiry && expiry < today,
            StockStatus = line.StockStatus,
            SnapshotQuantity = line.SnapshotQuantity,
            SnapshotTakenAt = line.SnapshotTakenAt,
            PhysicalQuantity = line.PhysicalQuantity,
            Note = line.Note,
            CountedByUserId = line.CountedByUserId,
            CountedAt = line.CountedAt,
            AdjustmentMutationId = line.AdjustmentMutationId
        };

        if (line.LineType == StockOpnameLineType.Finding)
        {
            response.Result = StockOpnameVarianceResults.Finding;
            response.VarianceQuantity = line.PhysicalQuantity;
            return response;
        }

        if (live != null && live.TryGetValue(line.Id, out var state))
        {
            response.CurrentQuantity = state.Current;
            response.ChangedSinceSnapshot = state.Changed;

            if (line.PhysicalQuantity is null)
            {
                response.Result = StockOpnameVarianceResults.NotCounted;
                return response;
            }

            if (state.Changed)
            {
                response.Result = StockOpnameVarianceResults.NeedsRecount;
                response.VarianceQuantity = session.Status == StockOpnameStatus.Submitted ? line.VarianceQuantity : null;
                return response;
            }

            var variance = line.VarianceQuantity ?? line.PhysicalQuantity.Value - line.SnapshotQuantity;
            response.VarianceQuantity = variance;
            response.Result = StockOpnameCalculator.ResultFromVariance(variance);
            return response;
        }

        if (line.PhysicalQuantity is null)
        {
            response.Result = StockOpnameVarianceResults.NotCounted;
            return response;
        }

        var stored = line.VarianceQuantity ?? line.PhysicalQuantity.Value - line.SnapshotQuantity;
        response.VarianceQuantity = stored;
        response.Result = StockOpnameCalculator.ResultFromVariance(stored);
        return response;
    }

    /// <summary>
    /// Saldo saat ini dan tanda berubah per baris saldo: jumlah berbeda dari snapshot, saldo sudah tidak
    /// ada, atau ada mutasi batch × status itu di lokasi itu sesudah snapshot baris tersebut.
    /// </summary>
    private async Task<Dictionary<Guid, LiveState>> EvaluateLiveAsync(Guid locationId,
        IReadOnlyCollection<PhmStockOpnameLine> lines, CancellationToken cancellationToken)
    {
        var balanceLines = lines
            .Where(x => x.LineType == StockOpnameLineType.Balance && x.DrugStockBalanceId != null)
            .ToList();
        var result = new Dictionary<Guid, LiveState>();
        if (balanceLines.Count == 0) return result;

        var balanceIds = balanceLines.Select(x => x.DrugStockBalanceId!.Value).Distinct().ToList();
        var balances = await _dbContext.PhmDrugStockBalances.AsNoTracking()
            .Where(x => balanceIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.QuantityOnHand })
            .ToDictionaryAsync(x => x.Id, x => x.QuantityOnHand, cancellationToken);
        var latest = await LoadLatestMovementsAsync(locationId, balanceLines, cancellationToken);

        foreach (var line in balanceLines)
        {
            if (!balances.TryGetValue(line.DrugStockBalanceId!.Value, out var current))
            {
                result[line.Id] = new LiveState(null, true);
                continue;
            }

            result[line.Id] = new LiveState(current,
                current != line.SnapshotQuantity || MovedAfterSnapshot(latest, line));
        }

        return result;
    }

    /// <summary>Waktu mutasi terakhir per batch × status di lokasi itu, sejak snapshot tertua.</summary>
    private async Task<Dictionary<(Guid DrugBatchId, DrugStockStatus Status), DateTime>> LoadLatestMovementsAsync(
        Guid locationId, IReadOnlyCollection<PhmStockOpnameLine> balanceLines, CancellationToken cancellationToken)
    {
        var latest = new Dictionary<(Guid, DrugStockStatus), DateTime>();
        if (balanceLines.Count == 0) return latest;

        var since = balanceLines.Min(x => x.SnapshotTakenAt);
        var batchIds = balanceLines.Where(x => x.DrugBatchId != null).Select(x => x.DrugBatchId!.Value).Distinct().ToList();

        var movements = await _dbContext.PhmDrugStockMutations.AsNoTracking()
            .Where(x => !x.IsDelete && x.StorageLocationId == locationId && batchIds.Contains(x.DrugBatchId) &&
                        x.OccurredAt > since)
            .Select(x => new { x.DrugBatchId, x.Status, x.OccurredAt })
            .ToListAsync(cancellationToken);

        foreach (var movement in movements)
        {
            var key = (movement.DrugBatchId, movement.Status);
            if (!latest.TryGetValue(key, out var known) || movement.OccurredAt > known)
                latest[key] = movement.OccurredAt;
        }

        return latest;
    }

    private static bool MovedAfterSnapshot(IReadOnlyDictionary<(Guid DrugBatchId, DrugStockStatus Status), DateTime> latest,
        PhmStockOpnameLine line) =>
        line.DrugBatchId is { } batchId &&
        latest.TryGetValue((batchId, line.StockStatus), out var occurredAt) &&
        occurredAt > line.SnapshotTakenAt;

    private static string? DescribeSubmitBlock(IReadOnlyCollection<StockOpnameLineResponse> balanceLines)
    {
        var notCounted = balanceLines.Where(x => x.Result == StockOpnameVarianceResults.NotCounted)
            .Select(x => x.LineNumber).ToList();
        var recount = balanceLines.Where(x => x.Result == StockOpnameVarianceResults.NeedsRecount)
            .Select(x => x.LineNumber).ToList();
        var changedNotCounted = balanceLines
            .Where(x => x.Result == StockOpnameVarianceResults.NotCounted && x.ChangedSinceSnapshot)
            .Select(x => x.LineNumber).ToList();

        var parts = new List<string>();
        if (notCounted.Count > 0) parts.Add($"Baris {FormatLineNumbers(notCounted)} belum dihitung.");
        if (recount.Count > 0 || changedNotCounted.Count > 0)
            parts.Add($"Saldo baris {FormatLineNumbers(recount.Concat(changedNotCounted).Distinct().OrderBy(x => x).ToList())} " +
                      "berubah sejak dimuat; muat ulang barisnya lalu hitung kembali.");
        return parts.Count == 0 ? null : string.Join(" ", parts);
    }

    private async Task<string?> DescribeDecisionBlockAsync(PhmStockOpname session,
        IReadOnlyCollection<PhmStockOpnameLine> lines, CancellationToken cancellationToken)
    {
        var userId = TryGetCurrentUserId();
        if (userId is null) return "Identitas pengguna tidak valid.";
        if (IsSelfInvolved(session, lines, userId.Value)) return SelfApprovalMessage;
        if (!await _authority.HasExplicitGrantAsync(userId.Value, StockOpnamePermissions.Resource,
                StockOpnamePermissions.Approve, cancellationToken))
            return NoGrantMessage;
        return null;
    }

    private const string SelfApprovalMessage =
        "Anda membuat, mengajukan, atau menghitung sesi ini, sehingga tidak boleh memutusnya (larangan self-approval).";

    private const string NoGrantMessage =
        "Anda tidak memegang izin persetujuan Stock Opname (StockOpname:Approve) pada penempatan akses yang berlaku.";

    private async Task EnsureCanDecideAsync(PhmStockOpname session, Guid actor, CancellationToken cancellationToken)
    {
        if (IsSelfInvolved(session, session.Lines.ToList(), actor))
            throw new StockOpnameForbiddenException(StockOpnameErrorCodes.SelfApproval, SelfApprovalMessage);

        if (!await _authority.HasExplicitGrantAsync(actor, StockOpnamePermissions.Resource,
                StockOpnamePermissions.Approve, cancellationToken))
            throw new StockOpnameForbiddenException(StockOpnameErrorCodes.NotAuthorizedToDecide, NoGrantMessage);
    }

    private static bool IsSelfInvolved(PhmStockOpname session, IEnumerable<PhmStockOpnameLine> lines, Guid userId) =>
        session.CreateBy == userId ||
        session.SubmittedByUserId == userId ||
        lines.Any(x => x.CountedByUserId == userId);

    private static void FillSummary(StockOpnameSummaryResponse target, PhmStockOpname session, int lineCount,
        int countedLineCount, int findingCount, IReadOnlyDictionary<Guid, string> names)
    {
        target.Id = session.Id;
        target.OpnameNumber = session.OpnameNumber;
        target.StorageLocationId = session.StorageLocationId;
        target.StorageLocationName = session.StorageLocationNameSnapshot;
        target.StockStatusScope = session.StockStatusScope;
        target.OpnameDate = session.OpnameDate;
        target.Status = session.Status;
        target.LineCount = lineCount;
        target.CountedLineCount = countedLineCount;
        target.FindingCount = findingCount;
        target.CreatedByUserId = session.CreateBy;
        target.CreatedByName = names.GetValueOrDefault(session.CreateBy);
        target.CreatedAt = session.CreateDateTime;
        target.SubmittedAt = session.SubmittedAt;
        target.DecidedAt = session.DecidedAt;
        target.Version = session.Version;
    }

    // ====================================================================== bantuan

    private async Task<T> GuardSchemaAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (StockOpnameSchemaGuard.IsMissingSchema(ex))
        {
            throw new StockOpnameSchemaUnavailableException();
        }
    }

    private async Task<PhmStockOpname> LoadForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PhmStockOpnames
            .Include(x => x.Lines.Where(l => !l.IsDelete))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw NotFound();

    private static void EnsureStatus(PhmStockOpname session, StockOpnameStatus expected, string message)
    {
        if (session.Status != expected)
            throw new StockOpnameConflictException(StockOpnameErrorCodes.InvalidState, message);
    }

    private static void EnsureVersion(PhmStockOpname session, int? expectedVersion)
    {
        if (expectedVersion is null)
            throw Unprocessable(StockOpnameErrorCodes.InvalidRequest, "Versi sesi wajib dikirim.");
        if (expectedVersion.Value != session.Version)
            throw new StockOpnameConflictException(StockOpnameErrorCodes.VersionConflict,
                "Sesi sudah diubah pengguna lain. Muat ulang sesi sebelum melanjutkan.");
    }

    private static void ValidateQuantity(int lineNumber, decimal? value, bool allowZero,
        string code = StockOpnameErrorCodes.InvalidCount)
    {
        if (value is not { } quantity) return;
        var prefix = lineNumber > 0 ? $"Baris {lineNumber}: " : string.Empty;

        if (quantity < 0m || (!allowZero && quantity == 0m))
            throw Unprocessable(code, $"{prefix}jumlah tidak boleh negatif{(allowZero ? string.Empty : " atau nol")}.");
        if (quantity > StockOpnameCalculator.MaxQuantity)
            throw Unprocessable(code, $"{prefix}jumlah melebihi batas.");
        if (!StockOpnameCalculator.HasValidScale(quantity))
            throw Unprocessable(code, $"{prefix}jumlah paling banyak {StockOpnameCalculator.MaxDecimals} angka desimal.");
    }

    private async Task SaveAsync(string? locationName, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new StockOpnameConflictException(StockOpnameErrorCodes.VersionConflict,
                "Sesi atau saldo berubah bersamaan oleh proses lain. Muat ulang sesi sebelum melanjutkan.");
        }
        catch (DbUpdateException ex) when (Mentions(ex, "UX_PhmStockOpname_ActiveLocation") ||
                                           Mentions(ex, "PhmStockOpname.StorageLocationId"))
        {
            throw ActiveSessionConflict(locationName ?? "ini");
        }
        catch (DbUpdateException ex) when (Mentions(ex, "CorrelationId"))
        {
            throw new StockOpnameConflictException(StockOpnameErrorCodes.InvalidState,
                "Penyesuaian sesi ini sudah pernah diposting.");
        }
    }

    /// <summary>
    /// Pesan pelanggaran keunikan memuat nama indeks (PostgreSQL) atau tabel.kolom (SQLite).
    /// </summary>
    private static bool Mentions(Exception exception, string text)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current.Message.Contains(text, StringComparison.Ordinal)) return true;
        }

        return false;
    }

    private static StockOpnameConflictException ActiveSessionConflict(string locationName) =>
        new(StockOpnameErrorCodes.ActiveSessionExists,
            $"Lokasi {locationName} masih punya sesi Stock Opname yang aktif. Selesaikan atau batalkan sesi itu lebih dahulu.");

    private static StockOpnameUnprocessableException Unprocessable(string code, string message) => new(code, message);

    private static StockOpnameNotFoundException NotFound() => new("Sesi Stock Opname tidak ditemukan.");

    private static string BuildAdjustmentReason(PhmStockOpname session, PhmStockOpnameLine line)
    {
        var reason = $"Stock Opname {session.OpnameNumber} baris {line.LineNumber}";
        return string.IsNullOrWhiteSpace(line.Note) ? reason : Truncate($"{reason}: {line.Note}", 1000);
    }

    private static string FormatLineNumbers(IReadOnlyCollection<int> numbers)
    {
        var shown = string.Join(", ", numbers.Take(ShownLineNumbers));
        return numbers.Count > ShownLineNumbers ? $"{shown} dan {numbers.Count - ShownLineNumbers} baris lain" : shown;
    }

    private static void Touch(QuilvianSystemBackend.Models.IdentityModel entity, Guid actor, DateTime now)
    {
        entity.UpdateBy = actor;
        entity.UpdateDateTime = now;
    }

    private Guid GetCurrentUserId() =>
        TryGetCurrentUserId() ?? throw new StockOpnameForbiddenException(StockOpnameErrorCodes.NotAuthorizedToDecide,
            "Identitas pengguna tidak valid.");

    private Guid? TryGetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? user?.FindFirstValue("user_id");
        return Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
    }

    private async Task<Dictionary<Guid, string>> LoadUserNamesAsync(IEnumerable<Guid?> ids,
        CancellationToken cancellationToken)
    {
        var list = ids.Where(x => x is { } value && value != Guid.Empty).Select(x => x!.Value).Distinct().ToList();
        if (list.Count == 0) return new Dictionary<Guid, string>();

        var users = await _dbContext.Users.AsNoTracking()
            .Where(x => list.Contains(x.Id))
            .Select(x => new { x.Id, x.DisplayName, x.UserName })
            .ToListAsync(cancellationToken);

        return users.ToDictionary(x => x.Id,
            x => !string.IsNullOrWhiteSpace(x.DisplayName) ? x.DisplayName : x.UserName ?? string.Empty);
    }

    private async Task<Dictionary<Guid, DrugInfo>> LoadDrugsAsync(IEnumerable<Guid> drugIds,
        CancellationToken cancellationToken)
    {
        var ids = drugIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, DrugInfo>();

        var drugs = await _dbContext.MstDrugs.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.DrugCode, x.DrugName, x.StockUnitMeasurementId })
            .ToListAsync(cancellationToken);

        var unitIds = drugs.Where(x => x.StockUnitMeasurementId.HasValue)
            .Select(x => x.StockUnitMeasurementId!.Value).Distinct().ToList();
        var unitNames = unitIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.MstMeasurements.AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.MeasurementName, cancellationToken);

        return drugs.ToDictionary(x => x.Id, x => new DrugInfo(x.DrugCode, x.DrugName,
            x.StockUnitMeasurementId is { } unitId ? unitNames.GetValueOrDefault(unitId) : null));
    }

    private async Task<Dictionary<Guid, BatchInfo>> LoadBatchesAsync(IEnumerable<Guid> batchIds,
        CancellationToken cancellationToken)
    {
        var ids = batchIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, BatchInfo>();

        return await _dbContext.PhmDrugBatches.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => new BatchInfo(x.BatchNumber, x.ExpiryDate), cancellationToken);
    }

    private Task AuditAsync(string action, string message, Guid actor, PhmStockOpname session, object? detail = null) =>
        _loggerService.AuditAsync(LogCategory, $"StockOpname.{action}", message, new
        {
            StockOpnameId = session.Id,
            session.OpnameNumber,
            Status = session.Status.ToString(),
            session.Version,
            Detail = detail,
            ActorUserId = actor,
            UserId = actor
        });

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length > maxLength ? value[..maxLength] : value;

    private static string? TruncateOrNull(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? null : Truncate(value, maxLength);
}
