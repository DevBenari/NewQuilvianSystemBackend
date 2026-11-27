# Laporan Perubahan Backend — `FIN-BE-CUR-RATE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIN-BE-CUR-RATE-001` |
| Judul | Mata Uang & Kurs: "Kurs ke IDR" terkini, "Update Terakhir" beserta nama pengguna, `DecimalPlaces` tidak tertimpa diam-diam, dan Hapus mata uang yang aman |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap (permintaan "Perbaiki BE–FE Mata Uang & Kurs", 27 Nov 2026). Tidak ada baris roadmap, jadi **tidak ada tanda status yang dipasang pada roadmap** |
| Trace | `FIN-DEC-018` (Finance memiliki master mata uang & kurs; Accounting hanya IDR), `FR-FIN-003` (satu mata uang dasar), `FR-FIN-004` (master yang sudah dipakai ditolak dihapus), `FIN-API-1.0` (kontrak Currency) |
| Contract version | Aditif pada respons; satu endpoint baru (`DELETE`); satu field request menjadi opsional. `contracts/api-contract.md` **belum diperbarui** (lihat bagian 7) |
| Dependency | Tidak ada. FE `FIN-FE-CUR-RATE-001` membaca kontrak ini |
| Task mode | `BACKEND`, tanpa migration, tanpa eksekusi database |
| Target tulis | `NewQuilvianSystemBackend` (source) + laporan ini |
| Branch / SHA saat dikerjakan | `Yasmina` @ `1cd7b477dc48799acc68792aea534d348b902975` |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-27 |
| Status | 🟡 **SEBAGIAN** — source selesai dan lolos pemeriksaan statis; `dotnet build` **TIDAK DIJALANKAN** (dilarang pada task ini), jadi kompilasi dan terjemahan LINQ→SQL belum terbukti |

---

## 1. Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `MasterData` |
| Pemilik / prefix registry | `Fin` / `Mst` (sudah terdaftar). Tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY`: `CurrenciesController`, `CurrencyService`, `CurrencyDtos` |
| QBE | Controller tidak memakai `ApplicationDbContext`; tidak ada hardcode role; `AccessAction`/`AccessPermission` pada `DELETE` baru memakai pasangan `Delete`/`Delete`; tidak ada entity, kolom, atau migration |
| Pemeriksa konformansi | `Invoke-QbeConformanceCheck.ps1 -Path` pada 3 berkas: VIOLATION 0, REVIEW 0, INFO 0 — **PASS** |
| Migration / database | **Tidak ada migration dan tidak ada DDL/DML.** `MstCurrency` dan `MstExchangeRate` tidak berubah skema. `DecimalPlaces` tetap ada di model dan kolom database |

---

## 2. Masalah yang diperbaiki

| No | Keadaan sebelumnya | Akibat bagi pengguna |
| ---: | --- | --- |
| 1 | `GET /` hanya memuat data `MstCurrency`; kurs terbaru tidak diproyeksikan | Petugas harus membuka detail satu per satu untuk tahu kurs hari ini |
| 2 | Respons tidak memuat nama pengguna pelaku perubahan | Tidak diketahui siapa yang terakhir mengubah mata uang atau mencatat kurs |
| 3 | `UpdateCurrencyRequest.DecimalPlaces` bertipe `int` dengan bawaan 2 | Klien yang tidak mengirim field itu (mis. form tanpa isian Desimal) **diam-diam menimpa** `DecimalPlaces` menjadi 2 |
| 4 | Tidak ada `DELETE` mata uang | Mata uang salah input tidak bisa dibersihkan |
| 5 | `PATCH /{id}/status` dan `PUT` boleh menonaktifkan mata uang dasar/IDR | Pilihan mata uang aktif untuk transaksi baru (`GET /options?onlyActive=true`) kehilangan mata uang dasarnya |
| 6 | `PUT` boleh mengganti kode mata uang yang sudah dipakai rekening bank | Rujukan teks `CurrencyCode` pada tabel lain terputus diam-diam |

---

## 3. Proses bisnis

### 3.1 Kurs ke IDR

| Langkah | Aturan |
| ---: | --- |
| 1 | Satu halaman daftar (maks. 100 mata uang) diambil dengan filter, pencarian, dan urutan yang sudah ada |
| 2 | Untuk halaman itu, backend memilih **satu** kurs per mata uang: `RateDate` terbaru yang **tidak melebihi hari ini** (zona waktu aplikasi Asia/Jakarta), belum terhapus, dan `MiddleRate > 0` |
| 3 | `CurrentRateToIdr` = `MiddleRate` kurs itu (Rupiah per satu unit). IDR selalu `1` dengan dasar `IDENTITY` |
| 4 | Tanpa kurs yang memenuhi syarat, `CurrentRateToIdr = null` — bukan `0` |

> **Contoh.** USD punya kurs 20 Nov 2026 = 15.800, 25 Nov 2026 = 15.825, dan 1 Des 2026 = 15.900. Pada 27 Nov 2026 yang tampil **15.825** (berlaku 25 Nov). Kurs 1 Des tidak dipakai karena bertanggal masa depan; baru berlaku sendiri pada 1 Des tanpa tindakan apa pun.
>
> **Contoh.** EUR hanya punya kurs tanggal 3 Des 2026 → pada 27 Nov tampil "Belum ada kurs", bukan Rp0.

**Dasar kurs.** `MiddleRate` dibaca sebagai Rupiah per satu unit mata uang asing. Dasarnya: (a) pemilik Finance menyatakan semantik itu pada permintaan task; (b) konsumen nyata `TravelCurrencyRateSyncWorker` (TRV-W4, HR Business Travel) sudah memakainya persis demikian — `BaseCurrencyAmount = ClaimedAmount × MiddleRate` dengan aturan "kurs terakhir pada atau sebelum tanggal transaksi" dan `MiddleRate > 0`. `BuyRate`/`SellRate` tidak dipakai dan tidak diganti diam-diam. Respons memuat `CurrentRateBasis = "MIDDLE"` supaya dasarnya terbaca.

### 3.2 Update Terakhir

Pelaku dan waktu dipilih dari **aktivitas yang lebih baru** antara:

| Sumber | Waktu | Pelaku |
| --- | --- | --- |
| Perubahan master (`LastUpdatedSource = CURRENCY`) | `UpdateDateTime`, atau `CreateDateTime` bila belum pernah diubah | `UpdateBy`, atau `CreateBy` |
| Pencatatan kurs (`LastUpdatedSource = EXCHANGE_RATE`) | `CreateDateTime` kurs terbaru yang dicatat (**bukan** `RateDate`) | `CreateBy` kurs itu |

Nama diambil dari `ApplicationUser` (`DisplayName`, lalu `UserName`, lalu `UserCode`). Pengguna yang tidak ditemukan menghasilkan `LastUpdatedByName = null` dengan waktu tetap dipertahankan; nama pengguna yang sedang login **tidak pernah** dipakai sebagai pengganti.

### 3.3 Hapus mata uang

| Syarat (semua harus terpenuhi) | Bila dilanggar |
| --- | --- |
| Bukan mata uang dasar dan bukan IDR | `400` "merupakan mata uang dasar/acuan kurs" |
| Tidak punya satu pun riwayat kurs (termasuk baris kurs yang sudah ditandai terhapus) | `400` "sudah memiliki N riwayat kurs" |
| Kodenya tidak dipakai tabel mana pun yang punya kolom teks `CurrencyCode`/`Currency` | `400` "dipakai pada rekening bank, …" |

Yang terjadi bila lolos: `IsDelete = true`, `IsActive = false`, `DeleteDateTime`, `DeleteBy` — **soft delete**. Riwayat kurs, rekening bank, dan transaksi tidak disentuh. Kode yang sudah terhapus bisa dipakai lagi karena indeks unik `IX_MstCurrency_CurrencyCode` berfilter `IsDelete = false`.

Pemeriksaan referensi menelusuri **seluruh** tabel pada model EF (≈60 tabel HR/Payroll/Finance/Accounting/Billing), bukan hanya foreign key, karena kode mata uang disimpan sebagai teks tanpa foreign key. Pemeriksaan dan penandaan terhapus berjalan dalam satu transaksi `Serializable`.

> **Contoh.** Admin menghapus "ZZZ" yang salah ketik dan belum pernah dipakai → berhasil. Admin menghapus "USD" yang punya 12 baris kurs → `400` "Mata uang USD tidak dapat dihapus karena sudah memiliki 12 riwayat kurs. Nonaktifkan saja bila tidak dipakai lagi."

### 3.4 Aturan tambahan yang baru

| Aturan | Respons | Catatan |
| --- | --- | --- |
| Mata uang dasar atau IDR tidak boleh dinonaktifkan (`PATCH /status` maupun `PUT` dengan `isActive=false`) | `422` | **Perilaku baru** — sebelumnya diizinkan. Perlu konfirmasi pemilik (bagian 8) |
| Kode mata uang yang sudah dipakai tabel lain tidak boleh diubah | `409` | **Perilaku baru**, bersikap aman. Perlu konfirmasi pemilik |
| `DecimalPlaces` tidak dikirim → nilai tersimpan dipertahankan; pada create memakai bawaan 2 | — | Memperbaiki masalah nomor 3 |

---

## 4. Perubahan yang dikerjakan

### 4.1 Berkas yang berubah (diverifikasi `git status`)

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/MasterData/DTOs/CurrencyDtos.cs` | `DecimalPlaces` pada request menjadi `int?`. `CurrencyResponse` + `CurrentRateToIdr`, `CurrentRateDate`, `CurrentRateSource`, `CurrentRateBasis`, `ExchangeRateCount`, `LastUpdatedAt`, `LastUpdatedByName`, `LastUpdatedSource`. Konstanta `CurrencyRateBases`, `CurrencyLastUpdatedSources`; DTO `CurrencyDeleteResponse` |
| `Areas/Corporate/FinanceManagement/MasterData/Services/CurrencyService.cs` | `ProjectAsync` (kurs terkini, jumlah riwayat, aktivitas kurs, nama pengguna; query tetap untuk satu halaman). `DeleteAsync` + `FindCodeReferencesAsync`. Penjaga mata uang dasar/IDR dan penjaga perubahan kode. `UpdateAsync` mempertahankan `DecimalPlaces`. Semua respons (list, detail, create, update, status) memakai proyeksi yang sama. `CurrencyInUseException` baru |
| `Areas/Corporate/FinanceManagement/MasterData/Controllers/CurrenciesController.cs` | `DELETE /{id}` dengan `Currency : Delete`; komentar lama "tidak ada DELETE" diganti penjelasan |

### 4.2 Tidak diubah (dengan alasan)

| Hal | Alasan |
| --- | --- |
| `MstCurrency`, `MstExchangeRate`, konfigurasi EF, migration | Tidak ada perubahan skema; presisi `decimal(18,2)` dan indeks unik `CurrencyId + RateDate` dipertahankan |
| `POST/GET /{id}/exchange-rates` | Tetap append-only; tanggal sama tetap `409` (`NO CHANGE REQUIRED`) |
| DI (`services.AddScoped<CurrencyService>()`) | Konstruktor tidak berubah |
| `TravelCurrencyRateSyncWorker`, Accounting, Finance transaksi | Tidak disentuh; selisih dilaporkan di bagian 6 |

### 4.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Respons bertambah field (aditif; klien lama aman). Request `DecimalPlaces` menjadi opsional (klien lama yang mengirim angka tetap sah). Satu endpoint baru |
| Database | **Tidak ada** perubahan skema/data oleh task ini. Satu pembacaan baru: `SELECT 1 … LIMIT 1` per tabel ber-kolom kode mata uang, hanya saat Hapus atau ganti kode |
| Keamanan/Auth | Izin baru `Currency : Delete` muncul di layar Akses Role **tanpa diberikan ke siapa pun**; admin harus mencentangnya. Tidak ada hardcode role |

---

## 5. Dokumentasi endpoint

#### Corporate / Finance Management / Master Data / Currency

Base URL: `api/v1/corporate/finance-management/master-data/currencies`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar mata uang + Kurs ke IDR terkini + Update Terakhir | `Currency : Read` | Diperluas |
| `GET` | `/{id}` | Detail satu mata uang (proyeksi yang sama) | `Currency : Read` | Diperluas |
| `POST` | `/` | Menambah mata uang (`decimalPlaces` opsional) | `Currency : Create` | Diubah (opsional) |
| `PUT` | `/{id}` | Mengubah mata uang (`decimalPlaces` opsional; tidak dikirim = dipertahankan) | `Currency : Update` | Diubah |
| `PATCH` | `/{id}/status` | Aktif/nonaktif; mata uang dasar/IDR tidak boleh dinonaktifkan | `Currency : Update` | Ditambah penjaga |
| `DELETE` | `/{id}` | Soft delete bila belum pernah dipakai | `Currency : Delete` | **Baru** |
| `GET` | `/{id}/exchange-rates` | Riwayat kurs | `Currency : Read` | Tetap |
| `POST` | `/{id}/exchange-rates` | Mencatat kurs satu tanggal | `Currency : Update` | Tetap |
| `GET` | `/summary`, `/filters/metadata`, `/options` | Ringkasan, metadata, pilihan | `Currency : Read` | Tetap |

Kode status `DELETE`: `200` dihapus; `400` ditolak karena dipakai (pesan menyebut alasan); `401` belum masuk; `403` tanpa hak `Currency : Delete`; `404` tidak ditemukan atau sudah terhapus.

Contoh respons `GET /` (data samaran):

```json
{
  "id": "…", "currencyCode": "USD", "currencyName": "Dolar Amerika Serikat", "isActive": true,
  "currentRateToIdr": 15825.00, "currentRateDate": "2026-11-25", "currentRateBasis": "MIDDLE",
  "exchangeRateCount": 12,
  "lastUpdatedAt": "2026-11-27T04:21:00Z", "lastUpdatedByName": "Petugas Finance", "lastUpdatedSource": "EXCHANGE_RATE"
}
```

---

## 6. Perlindungan Finance dan Accounting

| Hal | Bukti statis |
| --- | --- |
| Riwayat kurs tidak berubah/terhapus | Tidak ada `.Remove`, `RemoveRange`, `ExecuteDelete`, `ExecuteSqlRaw`, atau `DELETE FROM` pada `CurrencyService` (diperiksa tes FE `finance-currency-rate-column`). Hapus memblokir bila ada satu pun baris kurs |
| Kurs lama tidak ditimpa | Tidak ada jalur update kurs; kurs baru = baris baru; tanggal sama = `409` |
| Transaksi historis | Tidak ada tulis ke tabel transaksi. `TRV-W4` menyimpan snapshot `ExchangeRate` pada butir klaim saat konversi, sehingga kurs baru tidak mengubah nominal lama |
| Rekening bank & kejadian akuntansi | Penelusuran referensi memblokir Hapus bila kodenya dipakai `MstBankAccount`, `FinAccountingEventOutbox`, `AccAccountingEvent`, atau tabel lain berkolom `CurrencyCode`/`Currency` |
| Accounting | Kontrak Accounting tetap IDR saja (`FIN-DEC-018`); tidak ada perubahan |

---

## 7. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE conformance (3 berkas) | VIOLATION 0 / REVIEW 0 | `PASS` |
| Penelusuran source: kurs terkini, soft delete, `DecimalPlaces`, izin `Delete` | Sesuai (tes statis FE membaca source BE) | `PASS` (statis) |
| `dotnet restore` / `dotnet build` | **Tidak dijalankan** — dilarang pada task ini | `NOT RUN` |
| Terjemahan LINQ→SQL: subquery korelasi `Max(DateOnly?)`, `SqlQueryRaw<int>` + `AnyAsync`, transaksi `Serializable` | Belum terbukti | `NOT RUN` |
| Panggilan API / data nyata / UAT | Tidak dijalankan | `NOT RUN` |
| Migration / DDL / DML | Tidak ada, tidak dijalankan | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Catatan pengamatan: repository backend masih memuat folder `Tests/` dengan beberapa `.csproj`, padahal kebijakan menyatakan sudah dihapus. Tidak disentuh.

---

## 8. Keputusan terbuka dan risiko

| ID | Hal | Status |
| --- | --- | --- |
| CUR-D1 | Basis "Kurs ke IDR" = `MiddleRate` (bukan Beli/Jual) | `NEEDS CONFIRMATION` — didukung TRV-W4 dan pernyataan pemilik, tetapi blueprint tidak menulisnya |
| CUR-D2 | Definisi Update Terakhir = aktivitas lebih baru antara master dan pencatatan kurs | `NEEDS CONFIRMATION` |
| CUR-D3 | Tanggal harian vs waktu efektif; koreksi kurs pada tanggal yang sama | `BLOCKED` — butuh versioning/skema; tidak dibangun. Sementara: tanggal sama ditolak `409` |
| CUR-D4 | Hapus (soft delete) mata uang yang belum pernah dipakai | `IMPLEMENTED` atas permintaan pemilik Finance; kontrak `FIN-API-1.0` tidak melarang tertulis, `FR-FIN-004` mendukung penolakan bila dipakai. `api-contract.md` perlu diperbarui |
| CUR-D5 | Mata uang dasar/IDR tidak boleh dinonaktifkan | `NEEDS CONFIRMATION` (perilaku baru) |
| CUR-D6 | Kode mata uang yang sudah dipakai tidak boleh diubah | `NEEDS CONFIRMATION` (perilaku baru, bersikap aman) |
| CUR-D7 | Sumber kurs manual vs integrasi eksternal | `NEEDS CONFIRMATION` — tetap manual |
| CUR-R1 | Kurs `decimal(18,2)`: mata uang bernilai < Rp0,01 per unit tidak dapat direpresentasikan | Risiko diketahui |
| CUR-R2 | Race: pembuatan rekening bank bersamaan dengan Hapus pada transaksi non-`Serializable` tidak sepenuhnya terdeteksi | Risiko kecil, diketahui |
| CUR-R3 | Kegagalan serialisasi (`40001`) saat Hapus bersamaan menjadi `500` | Risiko kecil, diketahui |

### Temuan di luar task (tidak diperbaiki)

| Temuan | Lokasi |
| --- | --- |
| `contracts/api-contract.md` masih menulis endpoint Currency sebagai "Rencana" dan izin `FinanceCurrency`, padahal kode memakai `Currency` | `docs/module-blueprints/finance-management/contracts/api-contract.md` |
| `BankAccountService.DeleteAsync` melakukan soft delete **tanpa** pemeriksaan pemakaian (tidak menegakkan `FR-FIN-004`) | `Services/BankAccountService.cs` |
| `DeepLinkUrl` TRV-W4 menunjuk `/finance/master/exchange-rates` yang tidak ada (rute nyata `/finance/master-data/currency`) | `BusinessTravelWorkers.cs` |

---

## 9. Status Git

Berkas task ini: 3 berkas source `M` + laporan ini. Tidak ada stage/commit/push, tidak ada migration. Entri `??` laporan `FIN-*-LGS-LINK-001` berasal dari task sebelumnya.

Langkah berikutnya: `dotnet build`; admin mencentang `Currency : Delete` pada Akses Role; uji dengan data (USD ber-kurs, kurs masa depan, mata uang tanpa kurs, mata uang belum dipakai); pemilik memutuskan CUR-D1…D7; perbarui `api-contract.md`.
