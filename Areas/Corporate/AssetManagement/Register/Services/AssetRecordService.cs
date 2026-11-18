using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetRecordService
{
    private const string LogCategory = "Corporate.AssetManagement.Register";
    private const string AssetDocumentType = "AstAsset";
    private const int MaxPageSize = 100;
    private const int MaxDraftQuantity = 200;
    private const int MaxBatchSize = 200;
    private const int MaxReasonLength = 500;

    private readonly ApplicationDbContext _context;
    private readonly AssetEntityResolver _entityResolver;
    private readonly AssetActorResolver _actorResolver;
    private readonly AssetNumberingService _numberingService;
    private readonly AssetHistoryService _historyService;
    private readonly AssetBookService _bookService;
    private readonly AssetValuationParameterQueryService _parameterQuery;
    private readonly LoggerService _loggerService;

    public AssetRecordService(
        ApplicationDbContext context,
        AssetEntityResolver entityResolver,
        AssetActorResolver actorResolver,
        AssetNumberingService numberingService,
        AssetHistoryService historyService,
        AssetBookService bookService,
        AssetValuationParameterQueryService parameterQuery,
        LoggerService loggerService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _entityResolver = entityResolver ?? throw new ArgumentNullException(nameof(entityResolver));
        _actorResolver = actorResolver ?? throw new ArgumentNullException(nameof(actorResolver));
        _numberingService = numberingService ?? throw new ArgumentNullException(nameof(numberingService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _parameterQuery = parameterQuery ?? throw new ArgumentNullException(nameof(parameterQuery));
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }

    private async Task<AssetActor> RequireActorAsync(CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.GetCurrentAsync(cancellationToken);
        if (!actor.IsAuthenticated)
        {
            throw AssetManagementException.Forbidden(AssetRegisterErrorCodes.OutsideUnit, AssetRegisterMessages.OutsideUnit);
        }

        return actor;
    }

    private async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await work();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _context.ChangeTracker.Clear();
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch, exception);
        }
        catch
        {
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<AstAsset> LockAssetAsync(Guid id, AssetActor actor, CancellationToken cancellationToken)
    {
        var asset = await _context.AstAssets
            .FromSqlInterpolated($"SELECT * FROM public.\"AstAsset\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw NotFound();

        actor.EnsureCanAccessUnit(asset.CurrentOrganizationUnitId);
        return asset;
    }

    private static void EnsureStatus(AstAsset asset, AssetRecordStatus expected, string actionLabel)
    {
        if (asset.RecordStatus != expected)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.Status,
                string.Format(AssetRegisterMessages.StatusFormat, AssetRegisterLabels.RecordStatus(asset.RecordStatus).ToLowerInvariant(), actionLabel));
        }
    }

    private static string RequiredReason(string? value)
    {
        var reason = value?.Trim();
        if (string.IsNullOrEmpty(reason) || reason.Length > MaxReasonLength)
        {
            throw AssetManagementException.Invalid(AssetRegisterErrorCodes.ReasonRequired, AssetRegisterMessages.ReasonRequired);
        }

        return reason;
    }

    private static void Touch(AstAsset asset, Guid actorUserId, DateTime now)
    {
        asset.UpdateBy = actorUserId;
        asset.UpdateDateTime = now;
        asset.RowVersion = Guid.NewGuid();
    }

    private Task AuditAsync(string action, string message) =>
        _loggerService.AuditAsync(LogCategory, action, message);

    private static AssetManagementException NotFound() =>
        AssetManagementException.NotFound(AssetRegisterErrorCodes.NotFound, AssetRegisterMessages.NotFound);
}

public static class AssetRegisterLabels
{
    public static string RecordStatus(AssetRecordStatus value) => value switch
    {
        AssetRecordStatus.Draft => "Draft",
        AssetRecordStatus.Diajukan => "Diajukan",
        AssetRecordStatus.Terdaftar => "Terdaftar",
        AssetRecordStatus.Dibatalkan => "Dibatalkan",
        AssetRecordStatus.Dihapus => "Dihapus",
        _ => value.ToString()
    };

    public static string Condition(AssetCondition value) => value switch
    {
        AssetCondition.Aktif => "Aktif",
        AssetCondition.RusakRingan => "Rusak Ringan",
        AssetCondition.RusakBerat => "Rusak Berat",
        AssetCondition.TidakDigunakan => "Tidak Digunakan",
        AssetCondition.Hilang => "Hilang",
        _ => value.ToString()
    };

    public static string PlacementState(AssetPlacementState value) => value switch
    {
        AssetPlacementState.BelumDitempatkan => "Belum Ditempatkan",
        AssetPlacementState.Ditempatkan => "Ditempatkan",
        _ => value.ToString()
    };

    public static string HandoverPurpose(HandoverPurpose value) => value switch
    {
        Enums.HandoverPurpose.PenempatanAwal => "Penempatan Awal",
        Enums.HandoverPurpose.PindahDalamUnit => "Pindah Dalam Unit",
        Enums.HandoverPurpose.KoreksiOpname => "Koreksi Opname",
        Enums.HandoverPurpose.PascaPemulihan => "Pascapemulihan",
        _ => value.ToString()
    };

    public static string HandoverStatus(HandoverStatus value) => value switch
    {
        Enums.HandoverStatus.Diserahkan => "Diserahkan",
        Enums.HandoverStatus.Dikonfirmasi => "Dikonfirmasi",
        Enums.HandoverStatus.Ditolak => "Ditolak",
        Enums.HandoverStatus.Dibatalkan => "Dibatalkan",
        _ => value.ToString()
    };

    public static string ConditionReportStatus(ConditionReportStatus value) => value switch
    {
        Enums.ConditionReportStatus.Dilaporkan => "Dilaporkan",
        Enums.ConditionReportStatus.Ditetapkan => "Ditetapkan",
        Enums.ConditionReportStatus.Ditolak => "Ditolak",
        _ => value.ToString()
    };

    public static string ConditionReportSource(ConditionReportSource value) => value switch
    {
        Enums.ConditionReportSource.LaporanUnit => "Laporan Unit",
        Enums.ConditionReportSource.PemeriksaanBagianAset => "Pemeriksaan Bagian Aset",
        Enums.ConditionReportSource.PengembalianPinjaman => "Pengembalian Pinjaman",
        Enums.ConditionReportSource.TindakLanjutOpname => "Tindak Lanjut Opname",
        _ => value.ToString()
    };

    public static string AcquisitionType(AssetAcquisitionType value) => value switch
    {
        AssetAcquisitionType.Pembelian => "Pembelian",
        AssetAcquisitionType.HibahMasuk => "Hibah Masuk",
        AssetAcquisitionType.SaldoAwal => "Saldo Awal",
        _ => value.ToString()
    };

    public static string HistoryEvent(AssetHistoryEventType value) => value switch
    {
        AssetHistoryEventType.Didaftarkan => "Didaftarkan",
        AssetHistoryEventType.Ditempatkan => "Ditempatkan",
        AssetHistoryEventType.PindahDalamUnit => "Pindah Dalam Unit",
        AssetHistoryEventType.KondisiBerubah => "Kondisi Berubah",
        AssetHistoryEventType.Dimutasi => "Dimutasi",
        AssetHistoryEventType.Dipinjam => "Dipinjam",
        AssetHistoryEventType.Dikembalikan => "Dikembalikan",
        AssetHistoryEventType.Dimanfaatkan => "Dimanfaatkan",
        AssetHistoryEventType.PemanfaatanBerakhir => "Pemanfaatan Berakhir",
        AssetHistoryEventType.NilaiBerubah => "Nilai Berubah",
        AssetHistoryEventType.Diopname => "Diopname",
        AssetHistoryEventType.Dihapus => "Dihapus",
        AssetHistoryEventType.Dipulihkan => "Dipulihkan",
        AssetHistoryEventType.DiimporSaldoAwal => "Diimpor Saldo Awal",
        _ => value.ToString()
    };
}
