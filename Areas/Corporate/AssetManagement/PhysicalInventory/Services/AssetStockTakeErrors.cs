using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public static class AssetStockTakeErrorCodes
{
    public const string UnitInactive = "AST-VAL-INV-001";
    public const string Dates = "AST-VAL-INV-002";
    public const string Team = "AST-VAL-INV-003";
    public const string LocationUnit = "AST-VAL-INV-004";
    public const string NotYetStartable = "AST-VAL-INV-005";
    public const string ProcessInFlight = "AST-VAL-INV-006";
    public const string ActiveSession = "AST-VAL-INV-007";
    public const string ReasonRequired = "AST-VAL-INV-020";
    public const string Status = "AST-VAL-INV-023";
    public const string VersionMismatch = "AST-VAL-INV-024";
    public const string Field = "AST-INV-FIELD";
    public const string NotFound = "AST-INV-NOT-FOUND";
    public const string OutsideScope = "AST-INV-SCOPE";
}

public static class AssetStockTakeMessages
{
    public const string UnitInactive = "Pilih unit yang masih aktif.";
    public const string Dates = "Tanggal selesai harus sama dengan atau sesudah tanggal mulai.";
    public const string DatesRequired = "Tanggal mulai dan tanggal selesai wajib diisi.";
    public const string Team = "Tim wajib punya anggota dengan tepat satu ketua.";
    public const string TeamSize = "Tim inventarisasi paling banyak 30 orang.";
    public const string TeamEmployee = "Anggota tim harus pegawai yang masih aktif.";
    public const string LocationUnit = "Ruangan bukan milik unit yang dipilih.";
    public const string NotYetStartableFormat = "Sesi baru dapat dimulai pada {0}.";
    public const string ProcessInFlightFormat = "Selesaikan dulu {0} proses yang masih berjalan di unit ini: {1}.";
    public const string ActiveSessionFormat = "Unit ini sedang diinventarisasi pada sesi {0}.";
    public const string ReasonRequired = "Alasan wajib diisi, paling panjang 500 karakter.";
    public const string NotesLength = "Catatan paling panjang 1000 karakter.";
    public const string StatusFormat = "Sesi ini sudah {0}; tindakan tidak dapat dilakukan.";
    public const string VersionMismatch = "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi.";
    public const string RowVersionRequired = "Muat ulang data lalu coba lagi.";
    public const string NotFound = "Sesi inventarisasi tidak ditemukan atau sudah dihapus.";
    public const string OutsideScope = "Sesi inventarisasi ini bukan untuk unit Anda.";
    public const string StartScope = "Hanya ketua tim atau Petugas Aset yang dapat memulai sesi ini.";
}

public static class AssetStockTakeLabels
{
    public static string Status(StockTakeStatus value) => value switch
    {
        StockTakeStatus.Dijadwalkan => "Dijadwalkan",
        StockTakeStatus.Berjalan => "Berjalan",
        StockTakeStatus.Pencocokan => "Pencocokan",
        StockTakeStatus.MenungguVerifikasi => "Menunggu Verifikasi",
        StockTakeStatus.Diverifikasi => "Diverifikasi",
        StockTakeStatus.Dibatalkan => "Dibatalkan",
        _ => value.ToString()
    };
}
