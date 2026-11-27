namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;

public enum StockTakeStatus
{
    Dijadwalkan = 1,
    Berjalan = 2,
    Pencocokan = 3,
    MenungguVerifikasi = 4,
    Diverifikasi = 5,
    Dibatalkan = 6
}

public enum StockTakeResultCategory
{
    Sesuai = 1,
    SesuaiSedangDipinjam = 2,
    TidakDitemukan = 3,
    BedaLokasi = 4,
    BedaKondisi = 5,
    TidakTerdaftar = 6
}

public enum StockTakeFollowUpType
{
    KoreksiLokasi = 1,
    UsulanHilang = 2,
    PerubahanKondisi = 3,
    PendaftaranTemuan = 4
}

public enum StockTakeFollowUpStatus
{
    Usulan = 1,
    Diproses = 2,
    Diabaikan = 3
}
