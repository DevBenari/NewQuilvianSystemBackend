# Laporan Perubahan Backend — `BE-LAB-84`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-84` |
| Judul | Laporan penolakan wadah dan index waktu keputusan |
| Slice | Gelombang `MVP-11a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6an.3** |
| Trace | `FR-17.2`; `LAB-DEC-159` butir 3; `INV-56`; `FR-17.9` bagian laporan ini |
| Contract version | `LAB-API-v1` **`r37`** 32.2-32.3; `erd/data-dictionary.md` 19.2 — **`approved` 2026-09-28** |
| Dependency | `BE-LAB-83` ✅ (controller, service, DTO, `ResolvePeriod`) |
| Klasifikasi | `MEDIUM` — skor 9: repository 0, berkas diperiksa 1 (±12), berkas diubah 2 (4 + migration), logika bisnis 1, kontrak API 1 (endpoint aditif pada grup yang ada), database **2** (index baru, migration dibuat dan diterapkan), keamanan/auth 0 (kunci `Read` yang sudah ada), UI/workflow 0 |
| Task mode | `BACKEND` |
| Wewenang | Source: diberikan. **Membuat migration** dan **menerapkannya ke database dev**: diberikan eksplisit oleh pemilik modul — *"lanjut ke BE-LAB-84 lalu membuat dan menerapkan ke database devYoga"* |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Repositories/Configurations/`, `Migrations/`, dokumen blueprint Laboratorium; database **`QuilvianNewDevYoga`** |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | **`7ff35b8c`** (branch `yoga`) — merge `origin/QuilvianIntegrationBackend` di atas `e13e0b62` *"updates BE modul lab"*, yang memuat `BE-LAB-67`..`83`. **Impact scan merge:** di luar dokumen hanya `Program.cs`, `ApplicationDbContext.cs`, satu konfigurasi Akuntansi, dan migration `20260928041937_AddAccSubledgerBalance` beserta snapshot; **nol berkas Laboratorium**. Registrasi DI Laboratorium utuh |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — migration **dibuat dan diterapkan** ke `QuilvianNewDevYoga`: `Up` membuat `IX_LabSpecimen_DecidedAt` dan satu baris riwayat; `Down` dibuktikan menghapus keduanya di dalam transaksi yang lalu di-*rollback*; **nol baris data berubah** (sidik `LabSpecimen` identik di setiap tahap). Laporan berjalan **lewat HTTP terhadap data sungguhan** — PK September 5 diputuskan, 1 ditolak, 20 — dan **cocok dengan hitungan SQL independen**. Perencana PostgreSQL memakai index baru ketika seq scan dimatikan. Harness **15/15**; regresi `69`, `72`..`77`, `81`..`83` utuh; build 0 error tanpa warning baru; registri tetap 1576 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (mencakup source **dan pembuatan migration** modul Laboratorium; eksekusi database di luar dev pemilik tetap wewenang terpisah) |
| Keberlakuan | `NEW CODE` — `GetSpecimenRejectionAsync`, tiga DTO baru, endpoint baru. `TOUCHED LEGACY` — `LabSpecimenConfiguration` (satu index) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001` (periode lewat `ResolvePeriod` bersama), `QBE-DTO-001`, `QBE-PERM-001` (kunci `LabOperationalReport : Read` yang sudah ada), `QBE-CFG-002` (index di konfigurasi EF, bukan SQL lepas). **Tidak berlaku:** `QBE-LOG-001` (baca saja), `QBE-ENT-*` (nol kolom), `QBE-DB-001`/`002` (bukan migration legacy) |
| Governance yang dibaca | `AGENTS.md` backend — termasuk *"Jangan otomatis membuat … migration"* dan *"Menjalankan migration … memerlukan instruksi eksplisit terpisah"*, keduanya dipenuhi izin di atas; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `tooling/migrations/Update-MigrationHistory.ps1` (opsional, tidak dijalankan — lihat 7) |

---

## 1. Masalah yang diperbaiki

Kepala instalasi tidak dapat melihat **seberapa sering sampel ditolak dan karena apa**. Keputusan kelayakan
sudah tercatat sejak `S2`, tetapi tidak ada satu pun laporan yang membacanya, dan kolom tanggal keputusan
(`DecidedAt`) tidak ber-index. Laporan per rentang tanggal atas kolom itu akan membaca seluruh tabel.

---

## 2. Proses bisnis

**Laporan penolakan wadah** menjawab: *dari seluruh wadah yang diputuskan pada periode ini, berapa yang
ditolak, dan karena alasan apa*.

| Aturan | Contoh |
| --- | --- |
| Dasarnya **keputusan**, bukan status wadah hari ini | Di dev, satu-satunya wadah yang ditolak kini berstatus *Perlu Pengambilan Ulang*, bukan *Ditolak*. Laporan tetap menghitungnya sebagai penolakan 9 September |
| Tanggalnya **tanggal keputusan WIB** | Wadah yang tiba 30 September dan diputuskan tidak layak 1 Oktober 07.00 WIB masuk **Oktober** |
| Wadah pengganti adalah keputusan tersendiri | Tabung hemolisis ditolak lalu penggantinya diterima pada hari yang sama → **dua** keputusan, **satu** penolakan |
| **Ketiga disiplin** terhitung | Patologi Anatomi punya angka — keputusan kelayakan ada di semua disiplin, walau rilisnya belum |
| Nol keputusan → angka **kosong**, bukan 0 | Mikrobiologi tanpa satu keputusan pun: *—*, bukan *0%* |
| Rincian per alasan | *Hemolisis 8, Volume kurang 4* — nama dari katalog alasan; alasan yang katalognya dinonaktifkan tetap bernama |

**Angka sungguhan dev, September 2026:** Patologi Klinik 5 diputuskan, 1 ditolak — **20%**; alasannya
*Jumlah sampel tidak mencukupi*. Patologi Anatomi dan Mikrobiologi nol keputusan — angka kosong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6an.3 | Cakupan, tiga jebakan, verifikasi migration, DoD |
| `contracts/api-contract.md` 32.2-32.3 | Endpoint, ruas respons, arti `rejectionRatePercent` |
| `erd/data-dictionary.md` 19.2 | Nama dan bentuk index |
| `02-backend-architecture.md` 23.4 | DTO; *berlaku bagi ketiga disiplin* |
| `testing/acceptance-test-matrix.md` amandemen 2026-09-28 | `AC-251`, tiga baris `INV-56`, `ARCH-GAP-LAB-11` penolakan, baris *Migration* |
| `Models/LabSpecimen.cs`, `Models/MstLabRejectionReason.cs` | `DecidedAt`, `RejectionReasonCode`, `RejectionReasonId`, `ReasonName` |
| `Repositories/Configurations/…/LabSpecimenConfiguration.cs` | Index yang sudah ada — sejajar `PhysicallyReceivedAt` |
| `AGENTS.md`; `tooling/migrations/Update-MigrationHistory.ps1`; `git log`/`git diff` merge | Aturan migration; keadaan snapshot sesudah merge |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabOperationalReportService.cs` | `GetSpecimenRejectionAsync` — satu kueri `GROUP BY` (disiplin order × kode alasan × nama alasan); wadah layak membentuk kelompok tanpa kode, sehingga jumlah keputusan dan penolakan lahir dari kueri yang sama. `BarisPenolakan` — angka satu desimal, kosong bila pembagi nol |
| `DTOs/LabOperationalReportDtos.cs` | `LabSpecimenRejectionReportResponse`, `LabSpecimenRejectionRow`, `LabSpecimenRejectionReasonRow` — ruas persis 23.4 |
| `Controllers/LabOperationalReportController.cs` | `GET /specimen-rejection`, `[AccessPermission("LabOperationalReport", "Read")]` |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabSpecimenConfiguration.cs` | `HasIndex(x => x.DecidedAt)` beserta alasannya, sejajar index `PhysicallyReceivedAt` |
| `Migrations/20260930032822_AddLabSpecimenDecidedAtIndex.cs` (+ `.Designer.cs`) | **Baru.** `Up`: `CreateIndex IX_LabSpecimen_DecidedAt`. `Down`: `DropIndex`. Tidak ada yang lain |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | `+ b.HasIndex("DecidedAt");` — **satu-satunya** perubahan snapshot |

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Menghitung dari `SpecimenStatus` hari ini | Diputuskan = `DecidedAt` dalam periode; ditolak = `RejectionReasonCode` terisi | Data dev: wadah yang ditolak kini berstatus 6 dan tetap terhitung; harness: layak lalu dibatalkan tetap satu keputusan, *Rejected* tanpa `DecidedAt` tidak |
| Menulis 0 saat nol wadah diputuskan | `rejectionRatePercent` kosong bila pembagi nol | HTTP: PA dan Mikrobiologi `null` |
| Migration di atas snapshot lama | Dibangkitkan **sesudah merge** di atas snapshot `7ff35b8c`; isinya diperiksa sebelum dipakai | Diff snapshot hanya satu baris; `Up`/`Down` hanya index |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Order tanpa disiplin | *Belum tergolong*, sama dengan laporan jumlah — hanya tanpa penyaring dan hanya bila ada keputusan |
| Wadah atau order terhapus | Tidak dihitung |
| Kode alasan tanpa baris katalog | `reasonName` = kodenya — tetap terbaca, tidak hilang |
| Pembulatan | Satu desimal, `MidpointRounding.AwayFromZero`. JSON menulis angka bulat tanpa desimal (`20`, bukan `20.0`) — nilainya sama; bentuk *3,0* milik CSV `BE-LAB-86` dan layar |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif** — `GET /specimen-rejection` sesuai `r37` 32.2-32.3 |
| Database | **Satu index, nol kolom, nol baris data berubah.** `CREATE INDEX` non-concurrent mengunci tulis `LabSpecimen` sesaat — di dev 8 baris, **32 ms**. Untuk produksi: jalankan di luar jam sibuk bila tabelnya ternyata besar |
| Keamanan/Auth | Kunci `LabOperationalReport : Read` yang sama — registri tetap 1576. Respons tanpa nama pasien, No. RM, barcode, catatan penolakan, maupun petugas |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Operational Report

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/specimen-rejection` | Angka penolakan wadah per disiplin menurut tanggal keputusan, beserta rincian per alasan | `LabOperationalReport : Read` | `LabOperationalReportQuery` | `ApiResponse<LabSpecimenRejectionReportResponse>` |

Kode status sama dengan `/examination-count`: `200`, `400` (`VAL-147`, `VAL-148`, disiplin tak dikenal),
`401`, `403`, `422` (`VAL-149`).

Respons September 2026 — **tangkapan sungguhan** dari database dev:

```json
{
  "period": { "startDate": "2026-09-01", "endDate": "2026-09-30", "generatedAt": "2026-09-30T03:35:17Z" },
  "rows": [
    { "discipline": "ClinicalPathology", "disciplineName": "Patologi Klinik", "decidedCount": 5, "rejectedCount": 1, "rejectionRatePercent": 20 },
    { "discipline": "AnatomicalPathology", "disciplineName": "Patologi Anatomi", "decidedCount": 0, "rejectedCount": 0, "rejectionRatePercent": null },
    { "discipline": "Microbiology", "disciplineName": "Mikrobiologi", "decidedCount": 0, "rejectedCount": 0, "rejectionRatePercent": null }
  ],
  "reasons": [
    { "discipline": "ClinicalPathology", "reasonCode": "INSUFFICIENT_QUANTITY", "reasonName": "Jumlah sampel tidak mencukupi", "count": 1 }
  ]
}
```

---

## 5. Verifikasi

### 5.1 Migration — dibuat dan diterapkan ke `QuilvianNewDevYoga`

| Langkah | Hasil |
| --- | --- |
| `dotnet ef migrations add AddLabSpecimenDecidedAtIndex --no-build` (sesudah build `-p:RunAnalyzers=False`) | `20260930032822_AddLabSpecimenDecidedAtIndex`; isi dan diff snapshot diperiksa — hanya index |
| `dotnet ef migrations script 20260929041709_AddLabResultValidationAndRelease AddLabSpecimenDecidedAtIndex` | Skrip `Up` dibaca sebelum dijalankan: `CREATE INDEX "IX_LabSpecimen_DecidedAt" ON public."LabSpecimen" ("DecidedAt")` + satu `INSERT` riwayat, dalam satu transaksi |
| Skrip `Down` (arah sebaliknya) | `DROP INDEX public."IX_LabSpecimen_DecidedAt"` + satu `DELETE` riwayat |
| Keadaan **sebelum** | `LabSpecimen` 8 baris, sidik `f81ceb66…`; riwayat 252 baris, terakhir `20260929041709_AddLabResultValidationAndRelease`; index **tidak ada** |
| **`Up` diterapkan** — satu skrip, bukan `database update` | **32 ms.** Index ada (`CREATE INDEX … USING btree ("DecidedAt")`); riwayat 253; `LabSpecimen` 8 baris, sidik **identik** |
| **`Down` dibuktikan** — isi skrip `Down` dijalankan di dalam satu transaksi, keadaan dibaca, lalu `ROLLBACK` | Di dalam transaksi: index **hilang**, baris riwayat **hilang**, `LabSpecimen` 8 baris dengan sidik **identik**. Sesudah `ROLLBACK`: kembali ke keadaan `Up` |
| Keadaan **akhir** database dev | Migration `20260930032822_AddLabSpecimenDecidedAtIndex` **terpasang** |
| Migration modul lain | **Tidak disentuh.** `dotnet ef migrations list` sesudahnya: **13** migration modul lain tertunda di dev — dua belas yang sudah tercatat sebelumnya ditambah `20260928041937_AddAccSubledgerBalance` yang baru masuk lewat merge — dan **nol** milik Laboratorium. Skrip hanya memuat rentang `BE-LAB-70` → `BE-LAB-84` |

### 5.2 Kode dan HTTP

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -p:RunAnalyzers=False` (dua kali: sebelum dan sesudah migration) | **0 error, 230 warning**; nol dari berkas yang disentuh | `PASS` | Keluaran build |
| Harness EF InMemory `BE-LAB-84` | **15 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` 31/31, `72` 43/43, `73` 38/38, `74` 28/28, `75` 20/20, `76` 16/16, `77` 21/21, `81` 25/25, `82` 12/12 dan 13/13, `83` 25/25 | `PASS` | Harness yang sama |
| **SQL terhadap PostgreSQL** (sesi read-only) | Satu perintah: `SELECT … count(*)::int … LEFT JOIN "MstLabRejectionReason" … WHERE … "DecidedAt" >= @start AND "DecidedAt" <= @end … GROUP BY "Discipline", "RejectionReasonCode", "ReasonName"`; parameter September `2026-08-31T17:00Z` .. `2026-09-30T16:59:59.9999999Z` | `PASS` | Log perintah Npgsql |
| **Rencana eksekusi** | Bawaan: seq scan (tabel 8 baris — wajar). Dengan `enable_seqscan = off`: **`Index Scan using "IX_LabSpecimen_DecidedAt"`** dengan kondisi rentang `DecidedAt` | `PASS` dengan catatan | `EXPLAIN` |
| Presisi batas akhir | Npgsql **memotong** `16:59:59.9999999Z` menjadi `16:59:59.999999+00` — tetap sebelum tengah malam WIB. (Literal teks di `EXPLAIN` dibulatkan PostgreSQL ke `17:00:00`; itu hanya pada kueri tulisan tangan, bukan parameter aplikasi) | `PASS` | Uji parameter Npgsql |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |
| **Pembanding independen** — SQL `AT TIME ZONE 'Asia/Jakarta'` | PK 5 diputuskan, 1 ditolak; alasan *INSUFFICIENT_QUANTITY* 1 — **sama** dengan respons HTTP | `PASS` | Kueri baca-saja |
| Startup, registri, Swagger, log | Registri tetap **1576**; `specimen-rejection` di Swagger (`StartDate`, `EndDate`, `Discipline`; `200`/`400`/`422`; tanpa deskripsi); nol galat tak dikenal; nol log domain untuk pembukaan laporan | `PASS` | Log dan Swagger |

**Rincian HTTP** — superadmin kecuali disebut lain; nol penulisan.

| Panggilan | Hasil |
| --- | --- |
| `GET /specimen-rejection?startDate=2026-09-01&endDate=2026-09-30` | `200` — PK 5/1 = 20; PA dan Mikrobiologi 0/0, angka `null`; satu alasan |
| Periode 8 September saja | `200` — seluruhnya 0/0, angka `null` |
| Periode 9 September saja (hari keputusan) | `200` — PK 5/1 = 20 |
| `discipline=Microbiology` | `200` — satu baris Mikrobiologi |
| Tanpa `endDate` / terbalik / 367 hari / 366 hari | `400` / `400` (bunyi `VAL-148` sama) / `422` / `200` |
| `discipline=Hematology` | `400` — validasi model |
| Akun Kepala Instalasi (tanpa izin laporan) | `403` |
| Tanpa token | `401` |
| Regresi `GET /examination-count` | `200` |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| **`AC-251`** — 400 wadah PK diputuskan 1-7 September, 12 ditolak (8 hemolisis, 4 volume kurang) | `decidedCount 400`, `rejectedCount 12`, **`3.0`**; rincian *Hemolisis 8*, *Volume kurang 4* |
| Jebakan status hari ini | Wadah layak lalu dibatalkan **tetap** keputusan; *Rejected* tanpa `DecidedAt`, belum diputuskan, wadah dan order terhapus **tidak** dihitung |
| **`INV-56` wadah pengganti** (PA) | Ditolak + pengganti diterima hari yang sama → dua keputusan, satu penolakan |
| **`ARCH-GAP-LAB-11` penolakan** | PA **terhitung** — 4 keputusan, 3 ditolak, 75 |
| Alasan berkatalog nonaktif; kode tanpa katalog | Tetap bernama; nama = kode |
| **`AC-251` nol keputusan** | Mikrobiologi 0/0, angka **kosong** |
| **`INV-56` tanggal keputusan** — 1 Okt 07.00 WIB | Masuk Oktober, tidak di September |
| Batas hari — 31 Agu 23.59 WIB | Masuk 31 Agustus |
| Order tanpa disiplin | *Belum tergolong* 1/1, alasannya ikut terinci |
| Penyaring `AnatomicalPathology` | Hanya baris dan alasan PA |
| `VAL-147`/`148`/`149` | `400`, `400`, `422` |
| Jumlah kueri | **1** |
| Privasi | Ruas: `Count`, `DecidedCount`, `Discipline`, `DisciplineName`, `Period`, `ReasonCode`, `ReasonName`, `Reasons`, `RejectedCount`, `RejectionRatePercent`, `Rows` |

**Tidak dijalankan:**

- `Down` yang **dibiarkan** terjadi — sengaja; dibuktikan di dalam transaksi yang di-*rollback* supaya
  database dev tetap dalam keadaan terpasang.
- Rencana eksekusi pada volume produksi — dev 8 baris; perencana memilih seq scan secara bawaan.
- `AC-253` persona persisnya — kredensialnya tidak tersedia (sama dengan `BE-LAB-83`).
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-251` — 400/12 → `3.0`, rincian 8 + 4 | ✅ **Terpenuhi** pada harness; angka sungguhan dev cocok dengan SQL independen | — |
| `INV-56` wadah pengganti | ✅ **Terpenuhi** pada harness | — |
| `INV-56` tanggal keputusan | ✅ **Terpenuhi** pada harness; batas WIB pada parameter PostgreSQL | — |
| `AC-251` nol keputusan → kosong | ✅ **Terpenuhi** — harness dan HTTP | — |
| `ARCH-GAP-LAB-11` penolakan — PA terhitung | ✅ **Terpenuhi** pada harness | — |
| Baris *Migration* — index ada sesudah `Up`, hilang sesudah `Down`, nol baris berubah | ✅ **Terpenuhi** terhadap `QuilvianNewDevYoga` | 5.1 |
| Verifikasi roadmap — rencana eksekusi memakai index baru | ✅ **Terpenuhi dengan catatan** — dipakai saat seq scan dimatikan; pada 8 baris perencana memilih seq scan | 5.2 |
| DoD — endpoint berjalan; migration naik-turun terbukti; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** `tooling/migrations/Update-MigrationHistory.ps1` (memindah Designer lama ke `Migrations/History/`) **tidak dijalankan**. Skrip itu opsional, dan menjalankannya akan memindah puluhan Designer modul lain di luar cakupan task ini; `BE-LAB-70` pun tidak menjalankannya. Designer baru ikut dikompilasi seperti migration lain. **2.** Tiga belas migration modul lain masih tertunda di dev — bukan milik Laboratorium; `AddAccSubledgerBalance` (Akuntansi) bertambah lewat merge `7ff35b8c` |
| Risiko tersisa | **Rendah.** Index aditif; `CREATE INDEX` mengunci tulis sesaat — di produksi dijalankan di luar jam sibuk bila tabelnya besar |
| Status database | `QuilvianNewDevYoga`: `20260930032822_AddLabSpecimenDecidedAtIndex` **terpasang**; `IX_LabSpecimen_DecidedAt` ada; nol baris data berubah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M` `Controllers/LabOperationalReportController.cs`, `DTOs/LabOperationalReportDtos.cs`, `Services/LabOperationalReportService.cs`, `Repositories/Configurations/HealthServices/LaboratoryManagement/LabSpecimenConfiguration.cs`, `Migrations/ApplicationDbContextModelSnapshot.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`; `??` `Migrations/20260930032822_AddLabSpecimenDecidedAtIndex.cs`, `.Designer.cs`, laporan ini. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-85` (laporan waktu penyelesaian) naik menjadi `SIAP DIKERJAKAN` — memakai `LabCitoTurnaroundPolicy` dan `LabReleasableDisciplines`; nol migration. **2.** `BE-LAB-86` (unduhan CSV) sesudah `BE-LAB-85`. **3.** `BE-LAB-78` tetap `SIAP DIKERJAKAN` |
