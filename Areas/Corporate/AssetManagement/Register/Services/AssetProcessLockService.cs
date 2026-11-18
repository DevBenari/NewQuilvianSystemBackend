using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetProcessLockService
{
    public void Acquire(AstAsset asset, AssetActiveProcessKind kind, Guid processId, Guid? expectedRowVersion = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        EnsureProcess(kind, processId);
        EnsureRowVersion(asset, expectedRowVersion);

        if (asset.ActiveProcessKind == kind && asset.ActiveProcessId == processId)
        {
            return;
        }

        if (asset.ActiveProcessKind != AssetActiveProcessKind.Tidak)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.ProcessLocked,
                string.Format(AssetRegisterMessages.ProcessLockedFormat, Label(asset), ProcessName(asset.ActiveProcessKind)));
        }

        asset.ActiveProcessKind = kind;
        asset.ActiveProcessId = processId;
        asset.RowVersion = Guid.NewGuid();
    }

    public void Release(AstAsset asset, AssetActiveProcessKind kind, Guid processId)
    {
        ArgumentNullException.ThrowIfNull(asset);
        EnsureProcess(kind, processId);

        if (asset.ActiveProcessKind == AssetActiveProcessKind.Tidak)
        {
            return;
        }

        if (asset.ActiveProcessKind != kind || asset.ActiveProcessId != processId)
        {
            throw AssetManagementException.Conflict(
                AssetRegisterErrorCodes.ProcessLockNotOwned,
                string.Format(AssetRegisterMessages.ProcessLockNotOwnedFormat, Label(asset), ProcessName(asset.ActiveProcessKind)));
        }

        asset.ActiveProcessKind = AssetActiveProcessKind.Tidak;
        asset.ActiveProcessId = null;
        asset.RowVersion = Guid.NewGuid();
    }

    public static bool IsLocked(AstAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        return asset.ActiveProcessKind != AssetActiveProcessKind.Tidak;
    }

    public static string ProcessName(AssetActiveProcessKind kind) => kind switch
    {
        AssetActiveProcessKind.Penyerahan => "penyerahan",
        AssetActiveProcessKind.Mutasi => "mutasi",
        AssetActiveProcessKind.Peminjaman => "peminjaman",
        AssetActiveProcessKind.Pemanfaatan => "pemanfaatan",
        AssetActiveProcessKind.Penghapusan => "penghapusan",
        _ => "lain"
    };

    private static void EnsureProcess(AssetActiveProcessKind kind, Guid processId)
    {
        if (kind == AssetActiveProcessKind.Tidak || !Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Jenis proses kunci tidak sah.");
        }

        if (processId == Guid.Empty)
        {
            throw new ArgumentException("Id proses wajib diisi.", nameof(processId));
        }
    }

    private static void EnsureRowVersion(AstAsset asset, Guid? expectedRowVersion)
    {
        if (expectedRowVersion.HasValue && expectedRowVersion.Value != asset.RowVersion)
        {
            throw AssetManagementException.Conflict(AssetRegisterErrorCodes.VersionMismatch, AssetRegisterMessages.VersionMismatch);
        }
    }

    private static string Label(AstAsset asset) =>
        string.IsNullOrWhiteSpace(asset.InventoryNumber) ? asset.AssetName : asset.InventoryNumber;
}
