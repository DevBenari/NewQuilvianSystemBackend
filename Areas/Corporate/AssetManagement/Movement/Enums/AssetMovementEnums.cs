namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;

public enum TransferStatus
{
    Diajukan = 1,
    DisetujuiUnitAsal = 2,
    DisetujuiBagianAset = 3,
    Diserahterimakan = 4,
    Ditolak = 5,
    Dibatalkan = 6
}

public enum TransferReason
{
    KebutuhanLayanan = 1,
    KoreksiOpname = 2,
    KoreksiMutasi = 3,
    Lainnya = 9
}

public enum LoanStatus
{
    Diajukan = 1,
    Disetujui = 2,
    Dipinjam = 3,
    MenungguKonfirmasiKembali = 4,
    Dikembalikan = 5,
    Ditolak = 6,
    Dibatalkan = 7
}

public enum LoanExtensionStatus
{
    Diajukan = 1,
    Disetujui = 2,
    Ditolak = 3
}
