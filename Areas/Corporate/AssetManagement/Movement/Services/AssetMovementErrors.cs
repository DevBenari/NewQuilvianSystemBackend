using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;

public static class AssetMovementErrorCodes
{
    public const string AssetNotPlaced = "AST-VAL-MOV-001";
    public const string AssetCondition = "AST-VAL-MOV-002";
    public const string ProcessLocked = "AST-VAL-MOV-003";
    public const string SingleSourceUnit = "AST-VAL-MOV-004";
    public const string SameUnit = "AST-VAL-MOV-005";
    public const string UnitInStockTake = "AST-VAL-MOV-006";
    public const string ItemCount = "AST-VAL-MOV-007";
    public const string ReasonNote = "AST-VAL-MOV-008";
    public const string UnitApprover = "AST-VAL-MOV-010";
    public const string SelfApproval = "AST-VAL-MOV-011";
    public const string Receiver = "AST-VAL-MOV-012";
    public const string ReceiveTarget = "AST-VAL-MOV-013";
    public const string ReasonRequired = "AST-VAL-MOV-014";
    public const string Status = "AST-VAL-MOV-015";
    public const string VersionMismatch = "AST-VAL-MOV-016";
    public const string LoanDate = "AST-VAL-MOV-020";
    public const string Borrower = "AST-VAL-MOV-021";
    public const string ReturnCondition = "AST-VAL-MOV-022";
    public const string OpenExtension = "AST-VAL-MOV-023";
    public const string Field = "AST-MOV-FIELD";
    public const string NotFound = "AST-MOV-NOT-FOUND";
    public const string OutsideScope = "AST-MOV-SCOPE";
}

public static class AssetMovementMessages
{
    public const string AssetNotPlacedFormat = "Aset {0} belum terdaftar atau belum ditempatkan.";
    public const string AssetConditionFormat = "Aset {0} berstatus {1} dan tidak dapat dipindahkan.";
    public const string ProcessLockedFormat = "Aset {0} sedang dalam proses {1}.";
    public const string SingleSourceUnit = "Satu mutasi hanya untuk aset dari satu unit asal.";
    public const string SameUnit = "Unit tujuan harus berbeda dari unit asal. Untuk pindah ruangan gunakan penyerahan.";
    public const string UnitInStockTakeFormat = "Aset sedang dalam inventarisasi. Pengajuan dapat dilakukan setelah inventarisasi unit {0} selesai.";
    public const string ItemCount = "Pilih 1 sampai 200 aset.";
    public const string ReasonNote = "Alasan mutasi wajib diisi, paling panjang 1000 karakter.";
    public const string CorrectsTransferRequired = "Mutasi koreksi wajib merujuk mutasi asal yang sudah diserahterimakan.";
    public const string ReasonManualOnly = "Pilih alasan Kebutuhan Layanan, Koreksi Mutasi, atau Lainnya. Koreksi opname dibuat dari hasil inventarisasi.";
    public const string TargetUnitRequired = "Pilih unit tujuan yang masih aktif.";
    public const string UnitApproverFormat = "Hanya Kepala Unit {0} yang dapat menyetujui.";
    public const string SelfApproval = "Anda tidak dapat menyetujui pengajuan Anda sendiri.";
    public const string ReceiverFormat = "Hanya Kepala Unit {0} yang dapat menerima mutasi ini.";
    public const string ReceiveTarget = "Pilih ruangan milik unit tujuan dan penanggung jawab yang masih aktif.";
    public const string ReceiveCondition = "Kondisi saat diterima hanya Aktif, Rusak Ringan, Rusak Berat, atau Tidak Digunakan.";
    public const string ReceiveItems = "Kondisi diterima hanya untuk aset dalam mutasi ini.";
    public const string ReasonRequired = "Alasan wajib diisi, paling panjang 500 karakter.";
    public const string StatusFormat = "Pengajuan ini sudah {0}; tindakan tidak dapat dilakukan.";
    public const string VersionMismatch = "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi.";
    public const string NotFound = "Mutasi tidak ditemukan atau sudah dihapus.";
    public const string OutsideScope = "Mutasi ini tidak melibatkan unit Anda.";
    public const string CancelScope = "Hanya pengaju atau Kepala Bagian Aset yang dapat membatalkan mutasi ini.";
    public const string RowVersionRequired = "Muat ulang data lalu coba lagi.";
    public const string LoanSingleOwnerUnit = "Satu pinjaman hanya untuk aset dari satu unit pemilik.";
    public const string LoanSameUnit = "Unit peminjam harus berbeda dari unit pemilik aset.";
    public const string BorrowerUnitRequired = "Pilih unit peminjam yang masih aktif.";
    public const string LoanPurpose = "Tujuan peminjaman wajib diisi, paling panjang 1000 karakter.";
    public const string LoanDateRequired = "Tanggal mulai dan rencana tanggal kembali wajib diisi.";
    public const string LoanReturnBeforeStart = "Rencana tanggal kembali harus sama dengan atau sesudah tanggal mulai.";
    public const string LoanStartTooEarly = "Tanggal mulai pinjaman paling awal kemarin.";
    public const string ExtensionDate = "Tanggal kembali baru harus sesudah tanggal kembali yang berlaku.";
    public const string OwnerConfirmFormat = "Hanya Kepala Unit {0} yang dapat mengonfirmasi pengembalian.";
    public const string Borrower = "Hanya Kepala Unit peminjam yang dapat melakukan langkah ini.";
    public const string ReturnConditionRequired = "Isi kondisi setiap aset yang dikembalikan.";
    public const string ReturnConditionValue = "Kondisi saat kembali hanya Aktif, Rusak Ringan, Rusak Berat, atau Tidak Digunakan.";
    public const string ReturnItems = "Kondisi kembali hanya untuk aset dalam pinjaman ini.";
    public const string OpenExtension = "Masih ada permintaan perpanjangan yang belum diputuskan.";
    public const string LoanNotFound = "Pinjaman tidak ditemukan atau sudah dihapus.";
    public const string ExtensionNotFound = "Permintaan perpanjangan tidak ditemukan.";
    public const string LoanOutsideScope = "Pinjaman ini tidak melibatkan unit Anda.";
    public const string LoanCreateScope = "Pinjaman hanya dapat diajukan untuk unit Anda atau atas aset milik unit Anda.";
    public const string LoanCancelScope = "Hanya pengaju atau Kepala Unit pemilik yang dapat membatalkan pinjaman ini.";
    public const string ExtensionClosedOnReturn = "Pinjaman sudah dikembalikan sebelum perpanjangan diputuskan.";
}

public static class AssetMovementStockTake
{
    public static async Task EnsureNotHeldAsync(AssetStockTakeGate gate, IEnumerable<Guid> organizationUnitIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gate);
        if (await gate.FindUnitNameInStockTakeAsync(organizationUnitIds, cancellationToken) is { } unitName)
        {
            throw AssetManagementException.Conflict(
                AssetMovementErrorCodes.UnitInStockTake,
                string.Format(AssetMovementMessages.UnitInStockTakeFormat, unitName));
        }
    }
}

public static class AssetMovementLabels
{
    public static string TransferStatus(TransferStatus value) => value switch
    {
        Enums.TransferStatus.Diajukan => "Diajukan",
        Enums.TransferStatus.DisetujuiUnitAsal => "Disetujui Unit Asal",
        Enums.TransferStatus.DisetujuiBagianAset => "Disetujui Bagian Aset",
        Enums.TransferStatus.Diserahterimakan => "Diserahterimakan",
        Enums.TransferStatus.Ditolak => "Ditolak",
        Enums.TransferStatus.Dibatalkan => "Dibatalkan",
        _ => value.ToString()
    };

    public static string TransferReason(TransferReason value) => value switch
    {
        Enums.TransferReason.KebutuhanLayanan => "Kebutuhan Layanan",
        Enums.TransferReason.KoreksiOpname => "Koreksi Opname",
        Enums.TransferReason.KoreksiMutasi => "Koreksi Mutasi",
        Enums.TransferReason.Lainnya => "Lainnya",
        _ => value.ToString()
    };

    public static string LoanStatus(LoanStatus value) => value switch
    {
        Enums.LoanStatus.Diajukan => "Diajukan",
        Enums.LoanStatus.Disetujui => "Disetujui",
        Enums.LoanStatus.Dipinjam => "Dipinjam",
        Enums.LoanStatus.MenungguKonfirmasiKembali => "Menunggu Konfirmasi Kembali",
        Enums.LoanStatus.Dikembalikan => "Dikembalikan",
        Enums.LoanStatus.Ditolak => "Ditolak",
        Enums.LoanStatus.Dibatalkan => "Dibatalkan",
        _ => value.ToString()
    };

    public static string LoanExtensionStatus(LoanExtensionStatus value) => value switch
    {
        Enums.LoanExtensionStatus.Diajukan => "Diajukan",
        Enums.LoanExtensionStatus.Disetujui => "Disetujui",
        Enums.LoanExtensionStatus.Ditolak => "Ditolak",
        _ => value.ToString()
    };
}
