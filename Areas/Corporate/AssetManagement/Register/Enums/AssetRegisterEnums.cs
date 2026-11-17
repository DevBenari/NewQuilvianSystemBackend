namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

// AST-BP-001 register, BE-AST-003. Seluruh enum disimpan sebagai int (HasConversion<int>) —
// 02-backend-architecture.md bagian 5.4 dan 5.6. Nilai tidak boleh digeser; nilai baru selalu di ujung.

/// <summary>Status pencatatan aset (state-transition-matrix bagian 1).</summary>
public enum AssetRecordStatus
{
    Draft = 1,
    Diajukan = 2,
    Terdaftar = 3,
    Dibatalkan = 4,
    Dihapus = 5
}

/// <summary>Kondisi fisik aset yang berlaku (AST-DEC-017, state-transition-matrix bagian 2). Hanya berubah lewat penetapan laporan kondisi.</summary>
public enum AssetCondition
{
    Aktif = 1,
    RusakRingan = 2,
    RusakBerat = 3,
    TidakDigunakan = 4,
    Hilang = 5
}

/// <summary>Apakah aset sudah punya penempatan berjalan.</summary>
public enum AssetPlacementState
{
    BelumDitempatkan = 1,
    Ditempatkan = 2
}

/// <summary>Cara aset diperoleh.</summary>
public enum AssetAcquisitionType
{
    Pembelian = 1,
    HibahMasuk = 2,
    SaldoAwal = 3
}

/// <summary>Jenis proses yang sedang mengunci aset (AST-DES-005, state-transition-matrix bagian 7). <c>Tidak</c> = tidak terkunci.</summary>
public enum AssetActiveProcessKind
{
    Tidak = 0,
    Penyerahan = 1,
    Mutasi = 2,
    Peminjaman = 3,
    Pemanfaatan = 4,
    Penghapusan = 5
}

/// <summary>Tujuan penyerahan (AST-DES-006).</summary>
public enum HandoverPurpose
{
    PenempatanAwal = 1,
    PindahDalamUnit = 2,
    KoreksiOpname = 3,
    PascaPemulihan = 4
}

/// <summary>Status penyerahan (state-transition-matrix bagian 4).</summary>
public enum HandoverStatus
{
    Diserahkan = 1,
    Dikonfirmasi = 2,
    Ditolak = 3,
    Dibatalkan = 4
}

/// <summary>Status laporan kondisi (state-transition-matrix bagian 3). <c>Dilaporkan</c> = laporan terbuka.</summary>
public enum ConditionReportStatus
{
    Dilaporkan = 1,
    Ditetapkan = 2,
    Ditolak = 3
}

/// <summary>Asal laporan kondisi.</summary>
public enum ConditionReportSource
{
    LaporanUnit = 1,
    PemeriksaanBagianAset = 2,
    PengembalianPinjaman = 3,
    TindakLanjutOpname = 4
}

/// <summary>Jenis pemanfaatan aset oleh pihak lain (AST-DEC-021).</summary>
public enum UtilizationType
{
    Sewa = 1,
    KerjaSamaOperasional = 2,
    PinjamPakai = 3
}

/// <summary>Status pemanfaatan (state-transition-matrix bagian 5).</summary>
public enum UtilizationStatus
{
    Draft = 1,
    Diajukan = 2,
    MenungguDirektur = 3,
    Disetujui = 4,
    Berjalan = 5,
    Berakhir = 6,
    Ditolak = 7,
    Dibatalkan = 8
}

/// <summary>Penyetuju terakhir pemanfaatan, dikunci saat diajukan (AST-DEC-029).</summary>
public enum UtilizationApprovalLevel
{
    KepalaBagianAset = 1,
    Direktur = 2
}

/// <summary>Status batch impor data awal (state-transition-matrix bagian 6).</summary>
public enum ImportBatchStatus
{
    Diunggah = 1,
    GagalValidasi = 2,
    Tervalidasi = 3,
    DisetujuiBagianAset = 4,
    Diterapkan = 5,
    Ditolak = 6,
    Dibatalkan = 7
}

/// <summary>Jenis peristiwa pada linimasa aset, ditulis seluruh sub-modul (AST-DES-004).</summary>
public enum AssetHistoryEventType
{
    Didaftarkan = 1,
    Ditempatkan = 2,
    PindahDalamUnit = 3,
    KondisiBerubah = 4,
    Dimutasi = 5,
    Dipinjam = 6,
    Dikembalikan = 7,
    Dimanfaatkan = 8,
    PemanfaatanBerakhir = 9,
    NilaiBerubah = 10,
    Diopname = 11,
    Dihapus = 12,
    Dipulihkan = 13,
    DiimporSaldoAwal = 14
}
