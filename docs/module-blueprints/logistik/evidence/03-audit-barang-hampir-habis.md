# Logistik — Audit Kritis "Barang Hampir Habis"

| Field | Nilai |
| --- | --- |
| Blueprint ID | `LGS-BP-001` r2 |
| Objek | `GET /api/v1/health-services/pharmacy-management/drug-stock/low-stock` (`LGS-BE-ADH-01`) dan tab Hampir Habis |
| Tanggal | 2026-11-17 |
| Status rumus | **Tidak diubah.** Tetap seperti implementasi sampai pemilik Farmasi memutuskan `LGS-OQ-019` |

## 1. Perilaku aktual

| Besaran | Perlakuan di endpoint | Sumber kode |
| --- | --- | --- |
| Saldo fisik Siap Pakai | **Dipakai sebagai pembanding**: jumlah `QuantityOnHand` dari `PhmDrugStockBalance` berstatus `Available` pada obat dan lokasi kebijakan | `DrugStockService.GetLowStockAsync`, proyeksi `OnHand` |
| Reserved | Dihitung dan dikirim (`quantityReserved`), **tidak** ikut menentukan peringatan | idem, proyeksi `Reserved` |
| Available | Dikirim (`quantityAvailable = onHand − reserved`), **tidak** ikut menentukan peringatan | idem |
| Reorder point | **Ambang peringatan**: baris muncul bila `onHand ≤ ReorderPointQuantity` | idem, `.Where(x => x.IsUnitMismatch || x.OnHand <= x.ReorderPointQuantity)` |
| Stok minimum | Dikirim (`minimumStockQuantity`), tidak dipakai sebagai ambang | idem |
| Safety stock | Dikirim, tidak dikurangkan | idem |
| Karantina, kedaluwarsa, rusak | Tidak ikut dihitung | filter `Status == Available` |
| Satuan kebijakan ≠ satuan stok obat | Baris tampil dengan `isUnitMismatch = true`, tanpa dibandingkan | `IsUnitMismatch` |
| Kebijakan tanpa lokasi | Tidak dievaluasi | filter `StorageLocationId != null` |
| Reorder point = 0 | Tidak dievaluasi | filter `ReorderPointQuantity > 0` |
| Kebijakan tidak aktif / belum atau sudah lewat masa berlaku | Tidak dievaluasi | filter `IsActive`, `EffectiveStartDate`, `EffectiveEndDate` |

## 2. Dasar keputusan Farmasi

| ID | Isi | Kaitan |
| --- | --- | --- |
| `PHA-DEC-010` | Stok minimum adalah *reorder alert*, bukan safety stock | Menyebut **stok minimum** sebagai peringatan |
| `PHA-DEC-020` | Pisahkan *reorder point* untuk peringatan pengadaan dan safety stock untuk jumlah yang tidak tersedia bagi resep rutin | Menyebut **reorder point** sebagai peringatan pengadaan |
| `PHA-DEC-022` | Reorder point dan safety stock ditetapkan per obat/per lokasi | Dasar evaluasi per lokasi |
| `PHA-DEC-019` | Stok yang dapat diresepkan = usable on-hand − reserved − safety stock | Berlaku untuk resep, bukan peringatan pengadaan |
| `PHA-DEC-041` | Stok fisik baru berkurang saat penyerahan | Konsisten dengan pemakaian saldo fisik |

## 3. Konflik kebijakan yang ditemukan

1. **Ambang ganda.** `PHA-DEC-010` menyebut stok minimum sebagai reorder alert, sedangkan
   `PHA-DEC-020`/`022` memakai istilah reorder point. Master `MstDrugStockPolicy` menyimpan
   `MinimumStockQuantity` **dan** `ReorderPointQuantity` sebagai dua kolom terpisah. Implementasi
   memakai reorder point. Bila keduanya diisi berbeda, daftar bisa berbeda dari harapan pemakai.
2. **Basis perbandingan.** Tidak ada keputusan yang menyebut apakah peringatan memakai saldo fisik
   atau saldo tersedia. Implementasi memakai saldo fisik siap pakai.
3. **Cakupan kebijakan.** Validasi master mencegah duplikat hanya untuk kombinasi obat + lokasi +
   unit layanan + klinik yang sama (`DrugStockPolicyController.cs` baris 910–921). Dua kebijakan
   untuk obat dan lokasi yang sama dengan cakupan unit/klinik berbeda akan menghasilkan dua baris.

Ketiganya dicatat sebagai `LGS-OQ-019` untuk pemilik Farmasi.

## 4. Data nyata (query baca, 2026-11-17)

| Database | Kebijakan aktif | Dievaluasi (berlokasi, reorder point > 0) | Minimum ≠ reorder point | Satuan beda | Duplikat obat+lokasi | Saldo Siap Pakai > 0 | Saldo ber-reserved |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `QuilvianNewDevMaster` | **0** | 0 | 0 | 0 | 0 | 7.893 | 0 |
| `QuilvianNewDevIkbalFr` | **0** | 0 | 0 | — | — | 3 | 0 |

**Akibat langsung:** daftar Barang Hampir Habis **selalu kosong** di kedua database karena belum
ada satu pun kebijakan stok. Ini bukan kesalahan kode. Fitur baru memberi hasil setelah master
"Kebijakan Stock Obat" (`/health-services/master-data/drug-stock-policy`) diisi per obat per
lokasi oleh petugas berwenang (`PHA-DEC-022`). Karena belum ada saldo ber-reserved, perbedaan
saldo fisik dan saldo tersedia juga belum dapat diamati pada data.

## 5. Rekomendasi

1. Pertahankan rumus saat ini sampai `LGS-OQ-019` diputuskan.
2. Prioritaskan pengisian kebijakan stok untuk obat dan lokasi yang paling kritis supaya fitur
   dapat diuji dengan data nyata.
3. Bila pemilik Farmasi memilih stok minimum atau saldo tersedia sebagai dasar, perubahan cukup
   pada satu predikat di `GetLowStockAsync`; kontrak respons tidak berubah.
