namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;

public enum AssetBookStatus
{
    Aktif = 1,
    Dihentikan = 2
}

public enum ValueEntryType
{
    Perolehan = 1,
    SaldoAwal = 2,
    Penyusutan = 3,
    PenyesuaianPenyusutan = 4,
    PerbaikanBesar = 5,
    KoreksiHargaPerolehan = 6,
    PenurunanNilai = 7,
    PerubahanEstimasi = 8,
    Penghapusan = 9,
    Pemulihan = 10
}

public enum AccountingOutboxStatus
{
    Pending = 1,
    Held = 2,
    Acknowledged = 3,
    Failed = 4
}

public enum ValueChangeType
{
    PerbaikanBesar = 1,
    KoreksiHargaPerolehan = 2,
    PenurunanNilai = 3,
    PerubahanEstimasi = 4
}

public enum ValueChangeStatus
{
    Diajukan = 1,
    Disetujui = 2,
    Ditolak = 3,
    Dibatalkan = 4
}

public enum DepreciationAdjustmentReason
{
    Tidak = 0,
    KoreksiHarga = 1,
    TerlambatDicatat = 2,
    Pemulihan = 3
}
