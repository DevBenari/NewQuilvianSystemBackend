# Laporan Perubahan Backend — `BE-ACC-P2-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-014` |
| Judul | Perbandingan subledger, laporan selisih, dan penghalang rekonsiliasi |
| Slice | `P2-RECON` — Wave D tahap akhir |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-014` (revisi 6, `APPROVED` Rizki 28 September 2026) |
| Trace | `FR-P2-040`, `042`, `043`, `044`; `ACC-DEC-066`, `071`, `076`, `107`..`115`; `UAT-P2-28`, `30`..`33` |
| Contract version | `ACC-API-0.13` (grup Reconciliation dan Accounting Period); `ACC-VALIDATION-0.9` bagian 4 dan 4b; `ACC-STATE-0.5` bagian 2; `02-backend-architecture.md` bagian 23 — seluruhnya approved `GATE-DESAIN-0928` |
| Dependency | `BE-ACC-P2-013` ✅, `BE-ACC-P2-028` ✅, `GATE-DESAIN-0928` ✅ |
| Pasangan frontend | `FE-ACC-P2-016`, `FE-ACC-P2-017` — diuji bersama |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika bisnis kompleks (2), mengubah kontrak API (2), perilaku query pada tabel yang ada (1), keamanan berkaitan tetapi bukan intinya (1); repository dan UI 0 |
| Task mode | `BACKEND` (Langkah D, berpasangan dengan frontend atas perintah Rizki 28 September 2026) |
| Target tulis | `NewQuilvianSystemBackend` — `Reconciliation` dan dua service `AccountingPeriod`; laporan ini; baris status roadmap dan traceability. **Tidak** termasuk migration, build, maupun commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `485d8b8b` (branch `rizkiG`), perubahan source belum di-commit |
| Tanggal | 28 September 2026 |
| Status | **✅ SELESAI — 29 September 2026.** 12 dari 12 acceptance terpetakan ke source. Build Rizki 29 September 2026 terbukti dari `QuilvianSystemBackend.dll` bertanggal 08.48, sesudah source task terakhir diubah; jumlah warning belum dilaporkan. Uji Swagger S1–S13 + layar L1–L13 dijalankan Rizki sendiri 29 September 2026, sesuai harapan (bagian 5.3). UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 28–29 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `Reconciliation`; menyentuh `AccountingPeriod/Services` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 39. Salinan registry suite skill v1.17.1 tidak memuat `Acc`; registry repository yang berlaku (`ACC-TD-015`) |
| Keberlakuan | `NEW CODE` — dua enum, tiga DTO, method baru. `TOUCHED LEGACY` — `AccControlAccountReconciliationService` (`013`), `ReconciliationController` (`013`), `AccPeriodClosingService` (`005`, `026`), `AccAccountingPeriodService` (MVP). Nol entity, nol configuration |
| QBE yang berlaku | `QBE-SVC-001` (logika di service; controller satu baris), `QBE-API-001` (`ApiResponse<T>`, `ToActionResult`, kode status yang ada), `QBE-PERM-001` (`AccessAction` + `AccessPermission` yang cocok dengan `ControllerName`), `QBE-VAL-001` (`400`/`404`/`409`), `QBE-DTO-001`, `QBE-ENUM-001` (enum di `Reconciliation/Enums`). Tidak berlaku: `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-MOD-002/003` (nol model baru), `QBE-LOG-001` (pembacaan tanpa perubahan state; `ACC-DEC-115`) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/` suite (`TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `REPORT_TEMPLATE`); `BACKEND_ENGINEERING_CONTRACT.md`; registry repository |

## 1. Masalah yang diperbaiki

Saldo subledger dari Finance sudah dapat diterima dan disimpan sejak `BE-ACC-P2-028`, tetapi belum
pernah **dibandingkan** dengan buku besar, dan tutup bulan tidak pernah ditahan karenanya. Layar
Rekonsiliasi masih menulis "Belum tersedia" di kolom Subledger dan Selisih. Task ini menyelesaikan
tiga permintaan `ACC-DEC-066` yang tersisa: saldo subledger, laporan selisih, dan penghalang
penutupan `ACC-DEC-076` — dengan titik mulai `ACC-DEC-107` supaya tutup bulan tidak tertahan
sebelum Finance mengirim saldo apa pun.

## 2. Proses bisnis

1. Finance menutup periodenya dan mengirim satu pesan saldo per control account (`028`).
2. Petugas akuntansi membuka layar Rekonsiliasi dan memilih periode. Sistem menghitung, **saat itu
   juga**, saldo buku besar setiap control account per tanggal akhir periode, lalu membandingkannya
   dengan saldo dari Finance.
3. Setiap akun berkeadaan **Cocok**, **Berselisih**, **Belum diterima**, atau **Cut-off bukan akhir
   periode**. Akun wajib yang tidak Cocok menahan penutupan.
4. Saat Manager menekan Ajukan, sistem menghitung ulang. Bila masih ada akun yang menahan, pengajuan
   ditolak `409` dengan rincian jumlah akun per keadaan.
5. Sesudah disetujui (Tutup Sementara), Finance masih dapat menyatakan ulang saldo. Saat Manager
   menutup permanen, sistem menghitung ulang sekali lagi dan menolak `409` bila selisih muncul.

**Contoh.** Saldo pertama Finance untuk badan hukum X adalah periode September 2026. Agustus 2026
yang masih terbuka tetap "belum dapat diperiksa" dan boleh diajukan. September 2026 ditahan: Kas
Kecil belum dikirimi saldo (buku besar Rp 0 tidak membebaskan), dan Utang Supplier berselisih
Rp 500. Ajukan September ditolak: "Rekonsiliasi saldo subledger periode September 2026 belum
bersih: 1 control account belum menerima saldo subledger, 1 berselisih."

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `02-backend-architecture.md` bagian 23; `api-contract.md` grup Reconciliation dan Accounting Period; `validation-matrix.md` bagian 4, 4b; `state-transition-matrix.md` bagian 2 | Kontrak terkunci |
| `AccControlAccountReconciliationService.cs`, `ReconciliationController.cs`, `ControlAccountReconciliationDtos.cs` | Pola `gl-balances` yang dipakai ulang |
| `AccPeriodClosingService.cs` | Daftar periksa, `SubmitClosingAsync`, pembantu `Butir`/`ButirBelumTersedia` |
| `AccAccountingPeriodService.cs` | `CloseAsync`, `PeriksaPerpindahanTutup`, `NamaPeriode` (sudah `public static`) |
| `AccSubledgerBalance.cs`, `AccAccountingPeriod.cs`, `AccChartOfAccount.cs`, `IdentityModel.cs` | Nama kolom |
| `AccountingServiceResult.cs`, `AccountingLegalEntityGuard.cs`, `PeriodClosingDtos.cs` | Pola hasil, penjaga badan hukum, bidang butir daftar periksa |
| `AccountingEventStatus.cs`, `ApiResponse.cs` | Gaya enum `[Display]`; bentuk respons |
| Laporan `BE-ACC-P2-028` | Data uji dev dan templat pesan saldo |

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/Corporate/AccountingManagement/Reconciliation/Enums/SubledgerReconciliationState.cs` | **Baru** | `BelumBerlaku = 1`, `Bersih = 2`, `BelumBersih = 3`, ber-`[Display]` |
| `Areas/Corporate/AccountingManagement/Reconciliation/Enums/SubledgerReconciliationItemStatus.cs` | **Baru** | `Cocok = 1`, `Berselisih = 2`, `BelumDiterima = 3`, `CutOffBukanAkhirPeriode = 4`, ber-`[Display]` |
| `Areas/Corporate/AccountingManagement/Reconciliation/DTOs/ControlAccountReconciliationDtos.cs` | Diperbarui | + `SubledgerComparisonQuery`, `SubledgerComparisonReportResponse`, `SubledgerComparisonAccountResponse` (+87 baris) |
| `Areas/Corporate/AccountingManagement/Reconciliation/Services/AccControlAccountReconciliationService.cs` | Diperbarui | + `GetSubledgerComparisonAsync`; + `public static HitungRekonsiliasiSubledgerAsync` (aturan delapan langkah bagian 23.3); + `public static AlasanRekonsiliasiBelumBerlaku`; pembantu `RincianBelumBersih`; perhitungan saldo buku besar dipindah ke pembantu statis `HitungSaldoBukuBesarAsync` yang dipakai bersama `GetGlBalancesAsync` (+287/−28) |
| `Areas/Corporate/AccountingManagement/Reconciliation/Controllers/ReconciliationController.cs` | Diperbarui | + action `GetSubledgerComparison` (+8) |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccPeriodClosingService.cs` | Diperbarui | + `KodeRekonsiliasiSubledger`; butir keempat; pemeriksaan ulang di `SubmitClosingAsync`; pembantu `ButirRekonsiliasiSubledger` (+34/−1) |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccAccountingPeriodService.cs` | Diperbarui | Pemeriksaan rekonsiliasi di `CloseAsync` bila tujuannya `Closed` (+14) |

Total source +430/−29 baris pada lima berkas ditambah dua berkas baru (35 baris). **Nol** perubahan
pada `Program.cs`, `ApplicationDbContext`, entity, configuration, dan `Migrations/`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
|---|---|
| Kontrak API | Endpoint baru `GET /reconciliation/subledger-comparison`. `GET /periods/{id}/closing-checklist` membawa **empat** penghalang. `POST /periods/{id}/submit-closing` dan `POST /periods/{id}/close` (`Permanent = true`) dapat menjawab `409` karena rekonsiliasi. `GET /reconciliation/gl-balances` **tidak berubah** |
| Database | Hanya membaca `AccSubledgerBalance`, `AccAccountingPeriod`, `AccChartOfAccount`, `AccJournal`, `AccJournalLine`, `AccAccountingEvent`. Nol tulis, nol migration |
| Keamanan/Auth | `[AccessAction("Read", …)]` + `[AccessPermission("AccountingReconciliation", "Read")]` — hak yang sama dengan `gl-balances`; nol hak baru, nol hardcode peran. Pembacaan tidak dicatat `LoggerService` (`ACC-DEC-115`) |

### 3.4 Keputusan implementasi yang perlu diketahui

| Hal | Yang dipilih | Alasan |
|---|---|---|
| Letak fungsi hitung | `public static` di `AccControlAccountReconciliationService` | Service sudah terdaftar di `Program.cs` baris 715; satu fungsi dipakai layar, daftar periksa, Ajukan, dan Tutup Permanen |
| Pembantu saldo buku besar | Query yang **sama persis** dari `GetGlBalancesAsync` dipindah ke `HitungSaldoBukuBesarAsync` | Acceptance (4): angka `gl-balances` dan rekonsiliasi tidak mungkin berbeda |
| Dua komentar lama di query itu | Ikut dipindah apa adanya | Komentar yang sudah ada tidak dibuang; nol komentar **baru** |
| Titik mulai | Periode paling awal (menurut `StartDate`) yang punya baris `AccSubledgerBalance` belum terhapus untuk badan hukum itu | `ACC-DEC-107`; pesan saldo `Tertahan`/`Gagal` tidak punya baris, jadi tidak menyalakan |
| Nama periode | `AccAccountingPeriodService.NamaPeriode` yang sudah `public static` | Tidak menambah salinan nama bulan |
| Hitungan per keadaan (`MatchedCount` dan seterusnya) | Seluruh akun yang tampil, wajib maupun tidak | "Jumlah akun per keadaan" pada kontrak. `BlockingCount` dan kalimat `StateMessage` dihitung dari akun yang **menahan** saja |
| Kalimat rincian | Bagian pertama berawalan "control account": "1 control account belum menerima saldo subledger, 1 saldonya bertanggal cut-off bukan akhir periode, 1 berselisih." | Contoh bagian 23.4; tetap terbaca bila hanya satu keadaan, mis. "1 control account berselisih." |
| Urutan pemeriksaan Ajukan | Jurnal belum disahkan → kejadian Gagal → rekonsiliasi | Rekonsiliasi paling mahal dihitung; dua pemeriksaan murah lebih dulu |
| Setujui | Tidak memeriksa | `ACC-DEC-111` |
| Komentar kode | Nol baris `//` atau `///` baru | Arahan owner |

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Reconciliation

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `api/v1/corporate/accounting/reconciliation/subledger-comparison` | Perbandingan saldo buku besar dengan saldo subledger per periode | `AccountingReconciliation : Read` | `accountingPeriodId` (query, `Guid`, wajib) | `ApiResponse<SubledgerComparisonReportResponse>` |

| Kode | Kapan | Pesan |
|---|---|---|
| `200` | Berhasil, termasuk `BelumBerlaku` | "Rekonsiliasi saldo subledger periode September 2026 berhasil dihitung." / "Belum ada akun yang ditandai sebagai control account pada badan hukum ini." |
| `400` | `accountingPeriodId` kosong | "Periode akuntansi wajib disebutkan." |
| `404` | Periode tidak ada | "Periode akuntansi tidak ditemukan." |
| `409` | Penjaga badan hukum utama | Pesan penjaga yang sudah ada |

#### Corporate / Accounting / Accounting Period — perilaku yang berubah

| Method | Path | Perubahan |
|---|---|---|
| `GET` | `api/v1/corporate/accounting/periods/{id}/closing-checklist` | `blockers` berisi empat butir; keempat `SUBLEDGER_RECONCILIATION` |
| `POST` | `api/v1/corporate/accounting/periods/{id}/submit-closing` | `409` "Rekonsiliasi saldo subledger periode {nama} belum bersih: {rincian}" |
| `POST` | `api/v1/corporate/accounting/periods/{id}/close` | Bila `permanent: true`: `409` "Periode {nama} belum dapat ditutup permanen: rekonsiliasi saldo subledger belum bersih — {rincian}" |

## 5. Verifikasi

### 5.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `git diff --numstat -- Areas/` | Lima berkas diubah (+430/−29) dan dua berkas baru | `PASS` | Bagian 3.2 |
| `git ls-files --eol` | Berkas yang diubah `i/lf w/lf`; berkas baru ditulis LF tanpa BOM, sama dengan berkas enum lain | `PASS` | — |
| Penelusuran `//` pada baris tambahan | Hanya dua komentar lama yang dipindah bersama query-nya | `PASS` | Bagian 3.4 |
| `AccessPermission` lawan `ControllerName` dan `AccessAction` | `"AccountingReconciliation"` / `"Read"` — cocok | `PASS` | `ReconciliationController.cs` |
| `Program.cs`, `ApplicationDbContext`, `Migrations/` | Tidak tersentuh | `PASS` | `git status --short` |
| Pemetaan acceptance ke source | 12/12 | `PASS` | Bagian 6 |
| `dotnet build` (Rizki) | Build Rizki 29 September 2026 terbukti dari `QuilvianSystemBackend.dll` bertanggal 08.48, sesudah source task terakhir diubah; jumlah warning belum dilaporkan | `PASS` (tak langsung) | Tanggal berkas DLL |
| Uji Swagger dan layar | Dijalankan Rizki 29 September 2026, sesuai harapan (bagian 5.3) | `PASS` (pernyataan owner) | Laporan uji Rizki; response mentah tidak dilampirkan |
| Automated test | Bukan acceptance (`ACC-DEC-081`) | `NOT RUN` | — |

### 5.2 Skenario uji untuk Rizki — Swagger, lalu layar

**Build lebih dahulu** (backend dimatikan dulu bila sedang berjalan):

```bash
dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

**Persiapan, lewat layar — bukan SQL.**

1. Layar Jenis Kejadian: **aktifkan kembali** `UJI-SALDO-028` (Jenis Perlakuan Saldo Subledger).
2. Ambil `id` periode September dan Agustus 2026:
   `GET api/v1/corporate/accounting/periods?legalEntityId=<LEGAL_ENTITY_ID>&fiscalYear=2026`.
3. Catat `LegalEntityId` badan hukum utama — sama dengan uji `028`.

**Templat pesan saldo** untuk `POST api/v1/corporate/accounting/accounting-events`. Setiap akun
memakai `SourceTransactionId` sendiri — kunci anti-ganda kedua memakai gabungan modul asal, nomor
transaksi, jenis, dan versi, sehingga dua akun dengan nomor transaksi dan versi yang sama akan
terbaca sebagai kiriman ulang.

```json
{
  "EventNumber": "EVT-UJI-014A",
  "EventTypeCode": "UJI-SALDO-028",
  "SourceModule": "Finance",
  "SourceTransactionId": "UJI-014-2026-09-<KODE_AKUN>",
  "SourceVersion": "<VERSI>",
  "EventOccurredAt": "2026-10-01T08:00:00+07:00",
  "AccountingDate": "2026-09-30",
  "Amount": 0.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "<LEGAL_ENTITY_ID>",
  "CorrelationId": "0f014000-0000-4000-8000-000000000001",
  "CausationId": "0f014000-0000-4000-8000-000000000001",
  "SubledgerBalance": {
    "AccountingPeriodCode": "2026-09",
    "ControlAccountCode": "<KODE_AKUN>"
  }
}
```

`<VERSI>` = `subledgerVersionNumber` akun itu pada hasil S1 **ditambah 1**, atau `1` bila kosong.
Akun `1-1002` sudah versi 4 dari uji `028`, jadi mulai dari `5`. `Amount` selalu menurut saldo
normal: akun bersaldo normal kredit dikirim **positif** (`ACC-DEC-109`).

| # | Langkah | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| S1 | `GET api/v1/corporate/accounting/reconciliation/subledger-comparison?accountingPeriodId=<ID_SEP_2026>` | `200`; `reconciliationState` `3`; `startPeriodCode` `"2026-09"`; `accounts` memuat seluruh control account beserta `glBalance`; akun tanpa saldo `itemStatus` `3`, `subledgerBalance` `null`, `difference` `null`, `isBlocking` `true` bila wajib; `stateMessage` sama dengan rinciannya. **Catat `glBalance` dan `subledgerVersionNumber` setiap akun** | (1), (5), (6) |
| S2 | S1 untuk `<ID_AGU_2026>` | `200`; `reconciliationState` `1`; `startPeriodCode` `"2026-09"`; `blockingCount` `0`; `stateMessage` "Belum dapat diperiksa: rekonsiliasi saldo subledger belum berlaku untuk periode ini." | (3) |
| S3 | `GET api/v1/corporate/accounting/periods/<ID_AGU_2026>/closing-checklist` | `blockers` 4 butir; butir `SUBLEDGER_RECONCILIATION`: `state` `2`, `count` `0`, `isBlocking` `false`, `unavailableReason` "Rekonsiliasi saldo subledger berlaku mulai periode 2026-09." | (3) |
| S4 | Checklist `<ID_SEP_2026>` | Butir `SUBLEDGER_RECONCILIATION`: `state` `1`, `count` = `blockingCount` S1, `isBlocking` `true`, `message` = `stateMessage` S1; `canSubmitClosing` `false` | (8) |
| S5 | `POST .../periods/<ID_SEP_2026>/submit-closing`, body `{ "note": "Uji 014" }` | `409` "Rekonsiliasi saldo subledger periode September 2026 belum bersih: …". **Bila** pesannya tentang jurnal belum disahkan atau kejadian gagal, itu penghalang yang diperiksa lebih dulu — catat saja; bukti (9) lalu diambil dari S4 dan source | (9) |
| S6 | Pilih satu akun wajib berstatus `3`, sebut akun A. Kirim templat dengan `EventNumber` `EVT-UJI-014A`, `AccountingDate` **`2026-09-15`**, `Amount` = `glBalance` A | `201` `Tercatat`. S1 ulang: A `itemStatus` `4`, `subledgerAsOfDate` 15 September, `difference` `null`, `isBlocking` `true`; `cutOffMismatchCount` +1 | (6), `ACC-DEC-110` |
| S7 | A, `EVT-UJI-014B`, versi +1, `AccountingDate` `2026-09-30`, `Amount` = `glBalance` A **+ 500** | `201`. S1 ulang: A `itemStatus` `2`, `difference` `-500.00` | (6), toleransi nol |
| S8 | A, `EVT-UJI-014C`, versi +1, `Amount` = `glBalance` A | `201`. S1 ulang: A `itemStatus` `1`, `difference` `0.00`, `isBlocking` `false` | (6) |
| S9 | Bila ada control account bersaldo normal **kredit** (`normalBalance` `2`): kirim `EVT-UJI-014D` dengan `Amount` **positif** = `glBalance` akun itu | S1 ulang: `itemStatus` `1` | (7). Bila tidak ada, dibuktikan lewat source |
| S10 | Kirim saldo `Amount` = `glBalance` (versi +1, `EventNumber` `EVT-UJI-014E`, `…F`, dan seterusnya) untuk **setiap** akun wajib yang belum `1` — termasuk yang `glBalance`-nya `0` | S1: `reconciliationState` `2`, `blockingCount` `0`, `stateMessage` "Seluruh control account cocok dengan saldo subledger."; checklist S4: butir `count` `0`, `isBlocking` `false` | (2), (5), (8) |
| S11 | `GET …/subledger-comparison` tanpa parameter | `400` "Periode akuntansi wajib disebutkan." | (11) |
| S12 | `GET …/subledger-comparison?accountingPeriodId=11111111-1111-4111-8111-111111111111` | `404` "Periode akuntansi tidak ditemukan." | (11) |
| S13 | `GET api/v1/corporate/accounting/reconciliation/gl-balances?legalEntityId=<LEGAL_ENTITY_ID>&asOfDate=2026-09-30` | Setiap `balanceInNormalBalance` = `glBalance` akun yang sama pada S1 | (4) |
| S14 *(opsional — mengubah status periode dev)* | Sesudah S10 bersih dan penghalang lain nol: Ajukan September (Manager A) → Setujui (**pengguna lain**, `AccountingPeriod : Approve`) → kirim saldo akun A versi +1 dengan `Amount` = `glBalance` + 1 → `POST .../periods/<ID_SEP_2026>/close` body `{ "permanent": true, "reason": "Uji 014" }` | `409` "Periode September 2026 belum dapat ditutup permanen: rekonsiliasi saldo subledger belum bersih — 1 control account berselisih."; status tetap Tutup Sementara. Kembalikan dengan saldo versi +1 yang cocok, atau **Buka Kembali** beralasan | (10) |

**Uji layar** — dijalankan bersama `FE-ACC-P2-016` dan `017`, skenarionya di laporan
[`FE-ACC-P2-016`](../frontend/FE-ACC-P2-016.md). Urutannya: S1–S5 di Swagger, periksa layar,
lalu S6–S10, dan periksa layar lagi.

### 5.3 Hasil uji Rizki — 29 September 2026

Rizki menjalankan sendiri skenario S1–S13 di Swagger berselang-seling dengan layar L1–L13
(laporan `FE-ACC-P2-016` bagian 6.2), dan menyatakan seluruh hasilnya sesuai harapan pada tabel
bagian 5.2. Response body mentah dan tangkapan layar **tidak dilampirkan** — hasilnya berupa
gambar dan akan memakan banyak token. Atas keputusan Rizki, pernyataan owner yang menjalankan
uji sendiri diterima sebagai bukti; angka, id, dan jam per skenario karena itu tidak tercatat di
laporan ini.

| Skenario | Hasil | Klasifikasi |
|---|---|---|
| S1–S5 — perbandingan September, Agustus belum berlaku, dua daftar periksa, Ajukan `409` | Sesuai harapan | `PASS` (pernyataan owner) |
| S6–S10 — cut-off 15 September, selisih Rp 500, cocok, saldo kredit positif, rekonsiliasi bersih | Sesuai harapan | `PASS` (pernyataan owner) |
| S11–S13 — `400`, `404`, kesamaan dengan `gl-balances` | Sesuai harapan | `PASS` (pernyataan owner) |
| S14 *(opsional)* — Tutup Permanen `409` | Tidak disebut dalam laporan uji | `NOT RUN` — kriteria (10) dibuktikan dari source |

**Bersih-bersih, lewat layar.** Nonaktifkan kembali `UJI-SALDO-028`. Baris saldo uji September
2026 tetap ada dan tetap menyalakan rekonsiliasi di dev sejak September — memang disengaja
(`ACC-DEC-107`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti di source |
|---|---|---|
| (1) Endpoint `200` berbentuk `ACC-API-0.13`, enum angka | Terpenuhi di source | `ReconciliationController.GetSubledgerComparison`; `SubledgerComparisonReportResponse`/`AccountResponse` — nama dan tipe bidang sama dengan kontrak; enum tanpa konverter string |
| (2) Belum pernah ada saldo → `BelumBerlaku`, `StartPeriodCode` kosong, tidak menahan | Terpenuhi di source | `titikMulai is null` → `berlaku = false` → `menahan` selalu salah; `AlasanRekonsiliasiBelumBerlaku` → "Finance belum pernah…"; butir `ButirBelumTersedia` (`IsBlocking = false`) |
| (3) Sebelum titik mulai → `BelumBerlaku` beralasan periode | Terpenuhi di source | `periode.StartDate.Date >= titikMulai.StartDate.Date`; alasan "…berlaku mulai periode {kode}." |
| (4) Buku besar per `EndDate` inklusif, saldo normal, sama dengan `gl-balances` | Terpenuhi di source | `HitungSaldoBukuBesarAsync(db, idAkun, akhirPeriode)` — query yang sama dengan `gl-balances`; pembalikan tanda bila `NormalBalance.Credit` |
| (5) Akun wajib `ACC-DEC-108` | Terpenuhi di source | `wajib = IsControlAccount && IsPostable && (IsActive \|\| saldoBukuBesar != 0)`; daftar akun = control ∪ akun bersaldo |
| (6) Empat keadaan; cut-off → `4`, `Difference` kosong; Rp 500 → `2` | Terpenuhi di source | Rantai `if` keadaan; `Difference` hanya untuk `Cocok`/`Berselisih`; pembanding `decimal` persis |
| (7) Utang positif → `Cocok` | Terpenuhi di source | Pembanding `saldo.Balance == saldoBukuBesar` dengan `saldoBukuBesar` menurut saldo normal |
| (8) Butir daftar periksa `Evaluated`, `Count` = `BlockingCount`, `Message` = `StateMessage` | Terpenuhi di source | `ButirRekonsiliasiSubledger` → `Butir(…, rekonsiliasi.StateMessage, rekonsiliasi.BlockingCount, menahan: true)` |
| (9) Ajukan `409` | Terpenuhi di source | `SubmitClosingAsync` sesudah pemeriksaan kejadian Gagal; nol perubahan data sebelum `return` |
| (10) Tutup permanen `409`; `Permanent = false` dan Setujui tidak diperiksa | Terpenuhi di source | `CloseAsync`: `if (tujuan == AccountingPeriodStatus.Closed)` sesudah `PeriksaPerpindahanTutup`; `ApproveClosingAsync` tidak diubah |
| (11) `400`, `404`, penjaga `409` | Terpenuhi di source | `GetSubledgerComparisonAsync` |
| (12) Nol migration, nol `Program.cs`, nol `LoggerService` baca, nol komentar baru | Terpenuhi | `git status`; controller tanpa pemanggilan logger |

| Butir DoD | Keadaan |
|---|---|
| Source berubah | ✅ |
| Build Rizki 0 error | ✅ tak langsung — DLL terbentuk 29 September 2026 08.48; warning belum dilaporkan |
| Uji Swagger dan layar tercatat | ✅ 29 September 2026 — dijalankan Rizki, sesuai harapan (bagian 5.3; pernyataan owner, tanpa response mentah) |
| Laporan task tertulis | ✅ |

## 7. Catatan penutup

| Hal | Isi |
|---|---|
| Status Git (`git status --short`) | ` M` lima berkas source di bagian 3.2; `??` `Areas/Corporate/AccountingManagement/Reconciliation/Enums/`; ` M` laporan ini beserta roadmap dan traceability. Tidak ada yang di-stage |
| Migration / database | Tidak ada |
| Risiko tersisa | (a) Rekonsiliasi di dev menyala sejak September 2026 — tutup bulan September tertahan sampai seluruh control account wajib punya saldo yang cocok; disengaja. (b) `BE-ACC-P2-029` juga mengubah `AccPeriodClosingService` — kerjakan sesudah task ini. (c) Balapan Tutup Permanen dengan pesan saldo pada milidetik yang sama diterima sebagai risiko (bagian 23.5) |
| Temuan di luar cakupan | Komentar XML lama pada `ReconciliationController`, `AccControlAccountReconciliationService`, dan `ControlAccountBalanceReportResponse` masih menyebut `BE-ACC-P2-014` "terblokir `DEC-ACC-P2-011`" — sudah usang, **tidak** diubah karena komentar lama di luar cakupan |
| Task berikutnya | `BE-ACC-P2-030` + `FE-ACC-P2-018`, lalu `BE-ACC-P2-029` |
