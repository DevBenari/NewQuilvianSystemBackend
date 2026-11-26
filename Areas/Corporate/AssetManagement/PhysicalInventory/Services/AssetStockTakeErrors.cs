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
    public const string NotTeamMember = "AST-VAL-INV-010";
    public const string CountingClosed = "AST-VAL-INV-011";
    public const string UnknownCode = "AST-VAL-INV-012";
    public const string FoundLocation = "AST-VAL-INV-013";
    public const string AssetRemoved = "AST-VAL-INV-014";
    public const string ObservedCondition = "AST-VAL-INV-015";
    public const string Finding = "AST-VAL-INV-016";
    public const string ReasonRequired = "AST-VAL-INV-020";
    public const string DiscrepancyNote = "AST-VAL-INV-021";
    public const string Verifier = "AST-VAL-INV-022";
    public const string Status = "AST-VAL-INV-023";
    public const string VersionMismatch = "AST-VAL-INV-024";
    public const string FollowUpClosed = "AST-VAL-INV-030";
    public const string FollowUpBlocked = "AST-VAL-INV-031";
    public const string FollowUpNotFound = "AST-INV-FOLLOWUP-NOT-FOUND";
    public const string ResultNotFound = "AST-INV-RESULT-NOT-FOUND";
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
    public const string CountingScope = "Hanya ketua tim atau Petugas Aset yang dapat menutup atau membuka kembali pemindaian.";
    public const string NotTeamMember = "Anda bukan anggota tim inventarisasi sesi ini.";
    public const string CountingClosed = "Pemindaian sudah ditutup.";
    public const string CodeRequired = "Isi nomor aset yang dipindai atau diketik.";
    public const string UnknownCodeFormat = "Nomor {0} tidak terdaftar. Catat sebagai temuan tidak terdaftar.";
    public const string FoundLocation = "Pilih ruangan tempat aset ditemukan.";
    public const string AssetRemovedFormat = "Aset {0} sudah dihapus dari register.";
    public const string ObservedCondition = "Pilih kondisi aset saat diperiksa: Aktif, Rusak Ringan, Rusak Berat, atau Tidak Digunakan.";
    public const string Finding = "Temuan wajib diberi keterangan dan foto.";
    public const string NotesLength500 = "Catatan paling panjang 500 karakter.";
    public const string FindingLength = "Keterangan temuan paling panjang 500 karakter.";
    public const string TeamNoteLength = "Catatan tim paling panjang 1000 karakter.";
    public const string DiscrepancyNoteFormat = "Beri catatan pada {0} selisih sebelum mengajukan verifikasi.";
    public const string Verifier = "Verifikasi harus dilakukan Kepala Bagian Aset yang bukan anggota tim.";
    public const string SubmitScope = "Hanya ketua tim atau Petugas Aset yang dapat mengubah catatan dan mengajukan verifikasi.";
    public const string FollowUpClosedFormat = "Usulan ini sudah {0}.";
    public const string FollowUpBlockedFormat = "Usulan belum dapat diproses: aset {0} sedang dalam proses {1}.";
    public const string FollowUpAssetRemovedFormat = "Usulan belum dapat diproses: aset {0} sudah dihapus dari register.";
    public const string FollowUpDraftRequired = "Lengkapi data draft aset (nama, kelompok, kepemilikan, cara dan tanggal perolehan) untuk mendaftarkan temuan.";
    public const string FollowUpCustodianRequired = "Pilih penanggung jawab ruangan tujuan untuk penyerahan koreksi opname.";
    public const string FollowUpNotFound = "Usulan tindak lanjut tidak ditemukan.";
    public const string FollowUpOutsideScope = "Usulan ini bukan untuk unit Anda.";
    public const string ResultNotFound = "Hasil pencocokan tidak ditemukan pada sesi ini.";
    public const string DocumentNotReady = "Berita acara tersedia sesudah sesi diverifikasi.";
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

    public static string FollowUpType(StockTakeFollowUpType value) => value switch
    {
        StockTakeFollowUpType.KoreksiLokasi => "Koreksi Lokasi",
        StockTakeFollowUpType.UsulanHilang => "Usulan Hilang",
        StockTakeFollowUpType.PerubahanKondisi => "Perubahan Kondisi",
        StockTakeFollowUpType.PendaftaranTemuan => "Pendaftaran Temuan",
        _ => value.ToString()
    };

    public static string FollowUpStatus(StockTakeFollowUpStatus value) => value switch
    {
        StockTakeFollowUpStatus.Usulan => "Usulan",
        StockTakeFollowUpStatus.Diproses => "Diproses",
        StockTakeFollowUpStatus.Diabaikan => "Diabaikan",
        _ => value.ToString()
    };

    public static string Category(StockTakeResultCategory value) => value switch
    {
        StockTakeResultCategory.Sesuai => "Sesuai",
        StockTakeResultCategory.SesuaiSedangDipinjam => "Sesuai, Sedang Dipinjam",
        StockTakeResultCategory.TidakDitemukan => "Tidak Ditemukan",
        StockTakeResultCategory.BedaLokasi => "Beda Lokasi",
        StockTakeResultCategory.BedaKondisi => "Beda Kondisi",
        StockTakeResultCategory.TidakTerdaftar => "Tidak Terdaftar",
        _ => value.ToString()
    };
}
