# Laporan Perubahan Backend — `BE-OPT-054`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-054` |
| Judul | Penyaring Kepala Optik dan perkiraan harga pesanan draf (`OPT-API-v1.1`) |
| Slice | Penyesuaian layar — `EPIC OPT-04`, `OPT-06` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r3, disetujui Yoga Aji Pratama 2026-11-04) — Rincian `BE-OPT-054` |
| Trace | `OPT-DEC-067`, `075`, `076`; `OPT-DEC-018`, `010`; `OPT-AC-031`, `034`; screen brief §5.8, §5.10, §8; `OPT-OQ-019` |
| Contract version | Delta tambahan `OPT-API-v1` → `v1.1` sesuai rincian task (pemanggil lama tidak terdampak) |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain. Penentu tarif Billing hanya dibaca, lewat jalur yang sudah disetujui (`OPT-APR-011`) |
| Dependency | `BE-OPT-032` ✅, `BE-OPT-041` ✅, screen brief ✅ |
| Klasifikasi | `MEDIUM` — 1 endpoint baru, 2 penyaring, 4 tanda baris, tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `37ae965c` (branch `yoga`) |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` di berkas Optik yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-ENT-003` (Optik tidak menyimpan harga; harga dibaca dari penentu tarif Billing) |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Screen brief Optik yang disetujui 2026-11-04 membutuhkan dua hal yang belum tersedia di backend:

1. **Kepala Optik** perlu menemukan pesanan yang menunggu keputusan batal dan pesanan yang sudah boleh ditutup
   *Tidak Diambil* langsung dari daftar, tanpa membuka pesanan satu per satu (`OPT-DEC-067`).
2. **Petugas penjualan** perlu melihat perkiraan harga sebelum menagih. Sebelumnya harga baru terlihat sesudah
   pesanan ditagihkan, padahal barang tidak bisa diubah lagi sesudah itu (`OPT-DEC-075`, `076`).

---

## 2. Proses bisnis

### 2.1 Daftar pesanan untuk Kepala Optik

Dua penyaring baru di daftar pesanan:

| Penyaring | Isi |
| --- | --- |
| Menunggu persetujuan batal | Pesanan yang punya pengajuan batal belum diputuskan. Setelah Kepala Optik menyetujui atau menolak, pesanan itu hilang dari penyaring |
| Lewat batas ambil | Pesanan *Siap Diambil* yang sudah boleh ditutup *Tidak Diambil*: tanggal siap (WIB) ditambah batas hari unit (bawaan 90 hari) sudah sampai hari ini |

Setiap baris daftar (dan detail pesanan) sekarang membawa empat tanda: **terlambat**, **perlu dihubungi**,
**menunggu keputusan batal**, dan **lewat batas ambil**. Aturan tiap tanda sama persis dengan penyaringnya.

**Contoh batas hari** (hari ini 3 November 2026, batas 90 hari):

| Siap diambil | Lewat batas? | Tombol Tutup Tidak Diambil |
| --- | --- | --- |
| 5 Agustus 2026 pukul 23.59 WIB | Ya | Diterima |
| 6 Agustus 2026 pukul 00.00 WIB | Belum | Ditolak "Pesanan baru bisa ditutup sesudah 90 hari sejak siap diambil." |

Penyaring, tanda, dan tombol tutup memakai **satu fungsi yang sama** (`OptOrderService.IsPastNotCollectedLimit`),
sehingga ketiganya tidak pernah berbeda pendapat. Batas hari mengikuti pengaturan unit kunjungan; bila Admin Optik
mengubahnya menjadi 60 hari, pesanan yang siap 4 September langsung terhitung lewat batas pada 3 November.

### 2.2 Perkiraan harga pesanan draf

Setelah petugas menyimpan draf, layar meminta perkiraan harga. Backend menghitungnya dengan penentu tarif Billing
yang **sama** dengan saat Tagihkan: klinik dan kelas kunjungan yang sama, serta waktu tarif yang sama (waktu terakhir
draf diubah).

**Contoh:** draf frame FRM-A-HTM-52 + lensa LNS-156 → Rp 450.000 + Rp 600.000 = perkiraan **Rp 1.050.000**. Setelah
ditagihkan, baris tagihannya persis Rp 450.000 dan Rp 600.000.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Ada barang tanpa tarif | `200`; barang itu ditandai "Tarif barang FRM-B-CKL-50 belum ada. Hubungi bagian tarif.", total dikosongkan, `AllTariffsAvailable = false`. Tagihkan tetap akan ditolak seperti sebelumnya |
| Pesanan sudah ditagihkan atau bukan draf | `409` "Harga pesanan yang sudah ditagihkan dibaca dari tagihan." |
| Pesanan remake garansi | `409` "Pesanan remake tidak ditagihkan." |
| Pesanan tidak ada | `404` |
| Master tarif/Billing tidak terjangkau | `503` "Perkiraan harga belum dapat dihitung karena layanan tagihan sedang tidak dapat diakses. Silakan coba lagi." |
| Pengguna tanpa `OptOrder : Read` | `403` (filter izin) |

Perkiraan hanya membaca: tidak menerbitkan fakta tagihan, tidak membuat baris tagihan, dan tidak mengubah `Version`
pesanan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OptOrderService` (daftar, tutup tidak diambil, Tagihkan), `OptBillingGateway.CheckItemTariffsAsync`,
`BillingSourceTariffResolver.ResolveOpticalAsync` (baca-saja; tarif barang dibaca dari baris pesanan tersimpan, sebab
`OPT-DEC-076`), `OptSettingConfiguration` (indeks unik per unit), `OptOrderController`, harness `PesananUji`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/DTOs/OptOrderDtos.cs` | `OptOrderListQuery` + `HasPendingCancellation`, `PastNotCollectedLimit`; `OptOrderListItemResponse` + `IsOverdue`, `NeedsContact`, `HasPendingCancellation`, `IsPastNotCollectedLimit` (ikut terbaca di detail karena `OptOrderResponse` turunannya); DTO baru `OptOrderPricePreviewResponse`, `OptOrderPricePreviewLine` |
| `Areas/HealthServices/OpticalManagement/Services/OptOrderService.cs` | Dua penyaring; `MarkFlagsAsync` untuk daftar dan detail; `IsPastNotCollectedLimit` dipakai bersama oleh penyaring, tanda, dan `CloseNotCollectedAsync` (perilaku tutup tidak berubah); `GetPricePreviewAsync` |
| `Areas/HealthServices/OpticalManagement/Services/OptBillingGateway.cs` | `EstimateItemPricesAsync` (masukan sama dengan `CheckItemTariffsAsync`, baca-saja), record `OptItemPriceEstimate` |
| `Areas/HealthServices/OpticalManagement/Controllers/OptOrderController.cs` | `GET {id}/price-preview` (`OptOrder : Read`) |
| `Tests/QuilvianSystemBackend.OpticalTests/Be054OrderFlagsAndPricePreviewTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 8 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/Infrastructure/PesananUji.cs` | `PemutusBilling` juga bisa memutus pembacaan master tarif (`TarifPutus`) untuk uji `503` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be053EndToEndTests.cs`, `OpticalPermissionRegistryTests.cs` | Matriks izin + baris `GET orders/{id}/price-preview`; hitungan endpoint 44 → 45 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `OPT-API-v1.1` tambahan: 2 parameter query, 4 properti respons, 1 endpoint baru. Pemanggil lama tanpa parameter baru memperoleh hasil sama (terbukti uji). `contracts/api-contract.md` terkunci hash; penyegarannya tercatat di decision log bagian 11 |
| Database | Tanpa migration; baca-saja |
| Keamanan/Auth | Endpoint baru `OptOrder : Read`; registri izin Optik 45 endpoint, sama persis dengan matriks uji |
| Data | Respons pratinjau hanya berisi kode/nama barang, jumlah, dan harga; tanpa data klinis |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/optical-management/orders`

| Method | Path | Parameter / isi | Respons | Hak akses |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Tambahan: `HasPendingCancellation` (bool), `PastNotCollectedLimit` (bool) | `PagedResult<OptOrderListItemResponse>` + `IsOverdue`, `NeedsContact`, `HasPendingCancellation`, `IsPastNotCollectedLimit` | `OptOrder : Read` |
| `GET` | `/{id}/price-preview` | — | `OptOrderPricePreviewResponse`: `OrderId`, `OrderNumber`, `Lines[]` (`ItemId`, `LineNo`, `DrugCode`, `DrugName`, `Quantity`, `UnitPrice?`, `LineAmount?`, `TariffAvailable`, `Message?`), `EstimatedTotal?`, `AllTariffsAvailable` | `OptOrder : Read` |

**Contoh respons pratinjau:**

```json
{
  "orderId": "…",
  "orderNumber": "OPT-ORD-RSMMC-000001",
  "lines": [
    { "lineNo": 1, "drugCode": "FRM-A-HTM-52", "quantity": 1, "unitPrice": 450000, "lineAmount": 450000, "tariffAvailable": true, "message": null },
    { "lineNo": 2, "drugCode": "LNS-156", "quantity": 1, "unitPrice": 600000, "lineAmount": 600000, "tariffAvailable": true, "message": null }
  ],
  "estimatedTotal": 1050000,
  "allTariffsAvailable": true
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet`/`node` berjalan | `PASS` | `tasklist` |
| `dotnet test … --filter Be054 -p:RunAnalyzers=False` (termasuk build) | 0 error; 8 lulus | `PASS` | Keluaran `dotnet test` |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests --no-build` | 247 lulus, 0 gagal (239 lama + 8 baru) | `PASS` | Keluaran `dotnet test` |
| (1) Menunggu batal | Hanya pesanan berpengajuan `Requested`; pengajuan `Rejected` tidak terhitung; hilang sesudah ditolak; tanda di daftar dan detail benar | `PASS` | Uji `Kriteria_1_…` |
| (2) Batas hari | 23.59 WIB hari batas → lewat dan tutup diterima; 00.00 WIB hari berikutnya → belum dan tutup ditolak `OPT-VAL-056`; pengaturan unit 60 hari dipakai | `PASS` | Uji teori `Kriteria_2_…` + uji unit |
| (3) Empat tanda = penyaringnya | Himpunan pesanan bertanda sama dengan hasil tiap penyaring pada data campuran (terlambat, tepat waktu, perlu dihubungi, lewat batas, draf) | `PASS` | Uji `Kriteria_3_dan_4_…` |
| (4) Pemanggil lama | Tanpa parameter baru, atau parameter bernilai `false` → 5 pesanan, sama dengan sebelumnya | `PASS` | Uji yang sama |
| (5) `OPT-AC-034` | Perkiraan Rp 1.050.000; tiap baris sama dengan baris tagihan (`UnitPrice`, `Quantity`) sesudah Tagihkan | `PASS` | Uji `Kriteria_5_dan_8_…` |
| (6) Barang tanpa tarif | `200`, baris ditandai + pesan, total kosong | `PASS` | Uji `Kriteria_6_…` |
| (7) Penolakan | Sudah ditagihkan `409`, remake `409`, tidak ada `404`, master tarif putus `503`, lalu pulih | `PASS` | Uji `Kriteria_7_…` |
| (8) Tanpa efek | Tidak ada baris tagihan; `Version`, status, `BilledAt` tetap | `PASS` | Uji `Kriteria_5_dan_8_…` |
| Izin | `GET orders/{id}/price-preview` = `OptOrder : Read`; registri 45 endpoint sama dengan matriks | `PASS` | `Be053EndToEndTests`, `OpticalPermissionRegistryTests` |
| Uji mutasi 1: batas `<=` diubah `<` | 2 uji batas gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| Uji mutasi 2: penyaring batal tanpa syarat `Requested` | 2 uji gagal; berkas dipulihkan (`cmp` identik), lalu build ulang dan 25 uji terkait lulus | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 4 berkas produksi | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (pemanggilan terpisah) | Terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`; probe dihapus | `PASS` | Keluaran checker |

**Tidak dijalankan:** PostgreSQL; permintaan HTTP sungguhan untuk `403` (dibuktikan lewat atribut dan registri izin);
uji yang membedakan waktu tarif pratinjau dari waktu Tagihkan (harness tidak memakai tarif bermasa berlaku; kesamaan
dijamin karena keduanya membaca `UpdateDateTime ?? CreateDateTime` pesanan).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1)–(8) rincian `BE-OPT-054` | Terpenuhi di harness | Bagian 5 |
| `OPT-AC-031` | Terpenuhi di harness | Kriteria (1), (2) |
| `OPT-AC-034` | Terpenuhi di harness | Kriteria (5), (6) |
| Laporan, roadmap, traceability | Diperbarui | Dokumen ini; roadmap r3; traceability r2 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan pelaksanaan yang perlu dikonfirmasi | (a) Penyaring "perlu dihubungi" tetap memakai pengaturan pertama (perilaku lama), sedangkan "lewat batas ambil" memakai pengaturan unit kunjungan seperti Tutup Tidak Diambil. Hasilnya sama selama Optik hanya punya satu unit. (b) Perkiraan adalah harga normal tarif (bruto), sebelum diskon dan tanggungan penjamin, sama dengan baris tagihan sebelum diskon |
| Risiko tersisa | Perkiraan bisa berbeda dari tagihan bila tarif berubah antara pratinjau dan Tagihkan; layar melabelinya *perkiraan*. Belum dicoba di PostgreSQL |
| Penyegaran blueprint | `contracts/api-contract.md` (grup Opt Order) dan `contracts/permission-audit-matrix.md` (45 endpoint) — dicatat di decision log bagian 11 |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Status Git | `M` empat berkas Optik di atas; `M` roadmap, traceability, decision log, brief; `??` laporan ini. Berkas uji diabaikan Git |
| Langkah berikutnya | Frontend: `FE-OPT-002`/`003` (lapisan data, termasuk pemanggil `v1.1`), lalu layar sesuai brief |
