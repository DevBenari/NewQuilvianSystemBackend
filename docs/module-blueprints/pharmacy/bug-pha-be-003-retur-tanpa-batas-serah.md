# `BUG-PHA-BE-003` — Retur obat tidak dibatasi jumlah yang pernah diserahkan

Ditemukan 2 Oktober 2026 saat penambahan uji `DrugReturnService`. **Belum diperbaiki**; task yang
menemukannya berfokus verifikasi.

| Hal | Isi |
|---|---|
| Prioritas usulan | **P1** — menambah saldo stok dari barang yang tidak pernah keluar |
| Pemilik | modul Farmasi |
| Berkas | `Areas/HealthServices/PharmacyManagement/Services/DrugReturnService.cs` |
| Terjaga uji | `Tests/QuilvianSystemBackend.PharmacyTests/DrugReturnTests.cs` → `Retur_tidak_dibatasi_jumlah_yang_pernah_diserahkan` |

## Masalah

Retur pada source ini mengacu pada **batch obat**, bukan pada baris penyerahan. Tidak ada satu pun
pemeriksaan yang membandingkan jumlah yang dikembalikan dengan jumlah yang pernah benar-benar
diserahkan kepada pasien tersebut.

Yang diperiksa hanya:

| Pemeriksaan | Kode | Isi |
|---|---|---|
| Jumlah per item | `PHM077` | harus lebih dari nol |
| Batch kembar | `PHM078` | satu batch satu baris |
| Jumlah diterima saat pemeriksaan | `PHM071` | tidak boleh melebihi jumlah yang **dikembalikan** |

`PHM071` membandingkan jumlah diterima dengan jumlah yang **diajukan pada retur itu sendiri**,
bukan dengan jumlah yang diserahkan. Jadi lapisan pembanding terhadap penyerahan memang tidak ada.

`SourceDrugUsageId` pada `CreateDrugReturnRequest` bersifat **opsional** dan hanya disimpan sebagai
rujukan — ia tidak dipakai memvalidasi apa pun, dan tidak diperiksa apakah penyerahan yang dirujuk
memang milik kunjungan yang sama.

## Akibatnya

Retur 1.000 tablet atas resep berisi 10 tetap diterima sebagai `Draft`, dapat diajukan, dan
setelah diperiksa jumlah itu benar-benar masuk ke saldo lewat
`DrugStockService.ReceiveBatchAsync`. Stok bertambah dari barang yang tidak pernah keluar.

Keadaan ini tidak membutuhkan niat buruk: salah ketik satu angka nol sudah cukup, dan
pemeriksaannya di layar tidak punya pembanding apa pun untuk menolaknya.

## Acceptance ketika nanti diperbaiki

1. **Jumlah retur per batch tidak melebihi jumlah yang pernah diserahkan** dari batch itu kepada
   kunjungan tersebut, dikurangi yang sudah pernah diretur dan diterima.
2. **Retur yang merujuk penyerahan tertentu diperiksa kepemilikannya** — penyerahan itu harus
   benar-benar milik kunjungan pada retur tersebut.
3. **Retur berulang atas batch yang sama terakumulasi**, sehingga dua retur masing-masing separuh
   tidak dapat melampaui jumlah yang diserahkan bila dijumlahkan.

## Yang perlu diputuskan lebih dulu

**Apakah `SourceDrugUsageId` menjadi wajib?** Membuatnya wajib memberi pembanding yang pasti, tetapi
menutup retur obat yang penyerahannya tidak tercatat di sistem — misalnya sisa obat yang dibawa
pasien dari rawat inap lama. Menjadikannya tetap opsional menuntut pembandingnya dihitung dari
seluruh penyerahan pada kunjungan itu, yang lebih longgar tetapi tidak menutup kasus apa pun.

Pilihan itu milik pemilik modul; keduanya menutup celah ini dengan tingkat ketelitian yang berbeda.

## Catatan

Uji yang menjaganya **sengaja dinamai sesuai perilaku sebenarnya**. Ia lulus hari ini dan akan
gagal begitu bug ini diperbaiki — kegagalan itulah penanda bahwa perbaikannya benar-benar mengubah
perilaku, dan ujinya harus diperbarui bersama perbaikan tersebut.
