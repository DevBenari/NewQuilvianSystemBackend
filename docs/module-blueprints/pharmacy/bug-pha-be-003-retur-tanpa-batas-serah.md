# `BUG-PHA-BE-003` — Retur obat tidak dibatasi jumlah yang pernah diserahkan

Ditemukan 2 Oktober 2026 saat penambahan uji `DrugReturnService`. **Selesai** 2 Oktober 2026.

| Hal | Isi |
|---|---|
| Status | ✅ **Selesai** — diperbaiki beserta 13 uji regresi |
| Prioritas | **P1** — menambah saldo stok dari barang yang tidak pernah keluar |
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

## Keputusan pemilik modul — 2 Oktober 2026

**`SourceDrugUsageId` wajib untuk retur obat normal.** Pembandingnya **satu penyerahan tertentu**,
bukan total seluruh penyerahan pada kunjungan — total kunjungan terlalu longgar dan mencampur
beberapa penyerahan berbeda, sehingga kelebihan retur atas satu penyerahan dapat tersembunyi di
balik penyerahan lain yang belum diretur.

## Perbaikan

Satu penjaga baru, `EnsureReturnableAgainstSourceAsync`, dipanggil dari **tiga** titik:
`CreateAsync`, `UpdateAsync` (barisnya diganti seluruhnya), dan `VerifyAsync` (sebelum stok
bertambah).

| Syarat | Kode | Penegakan |
|---|---|---|
| 1. Sumber wajib ada | `PHM080` | `SourceDrugUsageId` null atau kosong ditolak; penyerahan yang tidak ditemukan juga |
| 2. Sumber milik kunjungan yang sesuai | `PHM081` | `usage.EncounterId` harus sama; penyerahan `Draft`/`Cancelled` ditolak |
| 3. Obat harus pernah diserahkan | `PHM082` | dibandingkan ke `DrugId` pada item penyerahan |
| 4. Batch harus sesuai | `PHM082` | dibandingkan ke `PhmDrugUsageAllocation.DrugBatchId` — penyerahan memang dilacak per batch |
| 5. Jumlah kumulatif ≤ yang diserahkan | `PHM083` | pesannya menyebut diserahkan, sudah diretur, dan sisanya |
| 6. `Verify` hanya menambah stok setelah lolos | — | pembanding dijalankan ulang sebelum `ReceiveBatchAsync` |
| 7. Retur berulang menghitung yang sebelumnya | `PHM083` | retur lain atas sumber yang sama ikut dihitung |

Pada syarat 7, yang sudah **diperiksa** dihitung sebesar jumlah yang **diterima**, sementara yang
masih berjalan dihitung sebesar yang diajukan — jumlah itu sudah dipesan dan belum boleh dipesan
ulang retur lain. Retur yang **ditolak atau dibatalkan tidak ikut dihitung**: barangnya tidak
pernah diterima kembali, jadi hak retur pasien kembali utuh.

## Kebutuhan terpisah: flow `Legacy/Untracked Return`

Retur obat yang penyerahannya **tidak tercatat di sistem** — misalnya sisa obat yang dibawa pasien
dari rawatan lama, atau obat dari masa sebelum modul ini berjalan — **sengaja tidak dipaksakan**
masuk jalur retur normal. Jalur normal sekarang menolaknya dengan `PHM080`, dan pesannya mengarahkan
ke jalur terpisah.

Kebutuhan yang masih harus dirancang pemilik modul:

1. **Alasan wajib** yang menyebutkan mengapa penyerahannya tidak tercatat; bukan teks bebas tanpa
   kategori, supaya pemakaiannya dapat dipantau dan tidak menjadi jalan pintas harian.
2. **Otorisasi khusus** — bukan kewenangan yang sama dengan retur biasa, karena jalur ini menambah
   stok tanpa pembanding apa pun.
3. **Penandaan pada barisnya** supaya retur tak terlacak dapat dipisahkan pada laporan dan audit
   stok, dan lonjakannya terlihat.
4. **Batas atas atau persetujuan berjenjang** untuk jumlah besar, karena satu-satunya pengaman yang
   tersisa adalah penilaian manusia.

Belum ada task untuk ini; dicatat di sini supaya kebutuhannya tidak hilang bersama perbaikan di
atas.

## Uji regresi

`Tests/QuilvianSystemBackend.PharmacyTests/DrugReturnTests.cs`, 13 uji, dengan sumber retur yang
lahir dari **alur penyerahan produksi penuh** — resep ditelaah, disiapkan, ditelaah akhir, lalu
diserahkan lewat container DI sungguhan — bukan baris penyerahan yang ditanam langsung ke tabel.
