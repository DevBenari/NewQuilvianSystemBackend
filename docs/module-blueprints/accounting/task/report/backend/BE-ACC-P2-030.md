# Laporan Perubahan Backend — `BE-ACC-P2-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-030` |
| Judul | Aturan posting ditolak untuk jenis Saldo Subledger |
| Slice | Wave D — kartu baru revisi 6 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-030` (revisi 6, `APPROVED` Rizki 28 September 2026) |
| Trace | `ACC-DEC-096`, `087`; perluasan `FR-P2-008` |
| Contract version | `ACC-API-0.14` (grup Event Type dan Posting Rule); `ACC-VALIDATION-0.10` — approved Rizki 28 September 2026 |
| Dependency | `BE-ACC-P2-018` ✅, `BE-ACC-P2-022` ✅ |
| Pasangan frontend | `FE-ACC-P2-018` — diuji bersama |
| Klasifikasi | `LIGHT` — skor 3: berkas diperiksa 9–20 (1), berkas diubah 3 (0), logika bisnis sederhana (1), perubahan kontrak aditif yang sudah approved (1); database, keamanan, dan UI 0 |
| Task mode | `BACKEND` (Langkah D, berpasangan dengan frontend atas perintah Rizki 28 September 2026) |
| Target tulis | `NewQuilvianSystemBackend` — `MasterData/EventType` dan `MasterData/PostingRule`; laporan ini; baris status roadmap dan traceability. **Tidak** termasuk migration, build, maupun commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `618b206e` (branch `rizkiG`), perubahan source belum di-commit |
| Status | **✅ SELESAI — 29 September 2026.** 7 dari 7 acceptance terpetakan ke source. Build Rizki terbukti tak langsung: `QuilvianSystemBackend.dll` bertanggal 29 September 2026 10.20, sesudah source task terakhir diubah (10.02); jumlah warning belum dilaporkan. Uji S1–S3 dan S6–S9 dijalankan Rizki 29 September 2026 12.01–12.02 WIB lewat skrip Playwright, response body mentah tercatat (bagian 5.3). S4, S5, dan L6 tidak berlaku — dev tidak punya aturan lama berjenis saldo (S2: 0); acceptance (4) dibuktikan dari source. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 29 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `MasterData` — `EventType` dan `PostingRule` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (registry repository yang berlaku, `ACC-TD-015`) |
| Keberlakuan | `TOUCHED LEGACY` — `AccEventTypeService` (`017`, `022`), `AccPostingRuleService` (`018`), `EventTypeOptionResponse` (`017`). Nol entity, nol configuration, nol berkas baru |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service, controller tidak berubah), `QBE-API-001` (`AccountingServiceResult` → kode status yang ada), `QBE-VAL-001` (`409`/`422` berpesan persis kontrak), `QBE-DTO-001` (bidang aditif). Tidak berlaku: `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-002/003`, `QBE-PERM-001` (endpoint dan izinnya tidak berubah), `QBE-LOG-001` (jalur tulis yang ada tidak diubah pencatatannya) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/` suite; registry repository — sama dengan `BE-ACC-P2-014` pada hari sebelumnya, tanpa perubahan |

## 1. Masalah yang diperbaiki

Sejak `BE-ACC-P2-028`, pesan berjenis Saldo Subledger tidak pernah dijurnal: jalurnya menyimpan saldo
akun kontrol, bukan membaca aturan posting. Namun `POST /posting-rules` masih menerima aturan untuk
jenis itu. Aturan tersebut tidak pernah dipakai, tetapi tampil aktif di daftar, sehingga petugas bisa
mengira saldo akhir bulan ikut dijurnal (`ACC-DEC-096`).

## 2. Proses bisnis

1. Petugas menyusun aturan posting untuk jenis kejadian Transaksi — tidak berubah.
2. Bila jenis yang ditunjuk berperlakuan Saldo Subledger, simpan ditolak `422` beserta alasannya.
3. Aturan lama yang terlanjur berjenis saldo tidak dapat diubah lagi; satu-satunya tindakan yang
   diterima adalah **Nonaktifkan**.
4. Jenis kejadian yang masih punya aturan aktif tidak dapat dipindah ke perlakuan Saldo Subledger
   (`409`). Aturannya dinonaktifkan lebih dahulu, lalu perlakuannya boleh diubah bila jenis itu belum
   dipakai kejadian (aturan `022` tetap).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `MasterData/EventType/DTOs/EventTypeDtos.cs`, `Enums/EventTypeKind.cs`, `Models/AccEventType.cs` | Bentuk pilihan jenis dan nilai enum perlakuan |
| `MasterData/EventType/Services/AccEventTypeService.cs` | `GetOptionsAsync`, `UpdateAsync` — urutan pemeriksaan yang ada |
| `MasterData/EventType/Controllers/EventTypeController.cs` | Rute `PUT /{id}` dan `GET /options`; tidak diubah |
| `MasterData/PostingRule/Services/AccPostingRuleService.cs`, `DTOs/*.cs`, `Controllers/PostingRuleController.cs` | Jalur `CreateAsync`, `UpdateAsync`, `DeactivateAsync`; `UpdatePostingRuleRequest` tidak membawa `EventTypeId` |
| `contracts/api-contract.md` (grup Event Type, Posting Rule), `contracts/validation-matrix.md` baris 232–233 | Pesan dan kode status persis |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
|---|---|
| `MasterData/EventType/DTOs/EventTypeDtos.cs` | `EventTypeOptionResponse` + `EventKind` (`EventTypeKind`, dikirim sebagai angka). +2/−0 |
| `MasterData/EventType/Services/AccEventTypeService.cs` | `GetOptionsAsync` mengisi `EventKind`. `UpdateAsync`: bila perlakuan berubah **menjadi** `SaldoSubledger` dan jenis itu masih punya aturan posting aktif yang tidak terhapus → `409` "Jenis kejadian ini masih punya aturan posting aktif. Nonaktifkan aturannya lebih dahulu." Pemeriksaan ini berjalan **sesudah** pemeriksaan "sudah dipakai kejadian" yang ada. +12/−1 |
| `MasterData/PostingRule/Services/AccPostingRuleService.cs` | + `using ...EventType.Enums`. `CreateAsync`: sesudah jenis kejadian ditemukan, jenis ber-`SaldoSubledger` → `422`. `UpdateAsync`: sesudah aturan dimuat, jenis aturan ber-`SaldoSubledger` → `422`. Helper `JenisSaldoSubledger<T>(kode)` berpesan persis kontrak. `DeactivateAsync` tidak diubah. +13/−0 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Hal | Dampak |
|---|---|
| API | `GET /event-types/options` bertambah `eventKind` (aditif; butir, urutan, dan penyaring tidak berubah). `POST`/`PUT /posting-rules` dapat menjawab `422` baru. `PUT /event-types/{id}` dapat menjawab `409` baru. Semuanya sesuai `ACC-API-0.14` |
| Database | Hanya baca tambahan (`AnyAsync` pada `AccPostingRule`). Nol migration, nol entity, nol configuration |
| Keamanan | Endpoint, `AccessAction`, dan `AccessPermission` tidak berubah. Nol hardcode role |
| `Program.cs`, `ApplicationDbContext` | Tidak tersentuh |

### 3.4 Keputusan implementasi yang perlu diketahui

1. **Acceptance (2) dan (3) ditangkap satu pemeriksaan.** `UpdatePostingRuleRequest` tidak membawa
   `EventTypeId`, karena jenis kejadian adalah identitas aturan yang tidak dapat dipindah lewat `PUT`.
   Satu-satunya cara aturan berada pada jenis saldo adalah aturan lama, atau jenisnya diubah menjadi
   saldo sesudah aturannya dinonaktifkan. `UpdateAsync` memeriksa jenis aturan yang tersimpan, sehingga
   kedua kasus ditolak `422`.
2. **Letak `422` sebelum pemeriksaan baris.** Pemeriksaan jenis berjalan sebelum `SiapkanAsync`,
   jadi aturan untuk jenis saldo ditolak dengan alasan yang benar walaupun barisnya belum lengkap.
3. **Urutan dua `409` pada `PUT /event-types/{id}`.** Bila jenis sudah dipakai kejadian **dan** masih
   punya aturan aktif, pesan "sudah dipakai kejadian" yang muncul. Penghalang itu permanen, sehingga
   menonaktifkan aturan tidak akan membuka jalan.
4. **Aturan nonaktif tidak menahan perubahan perlakuan.** Hanya aturan `IsActive && !IsDelete` yang
   dihitung, sama dengan `ActivePostingRuleCount`.

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Master Data / Event Type

| Method | Path | Perubahan |
|---|---|---|
| `GET` | `/options` | Setiap butir + `eventKind` (`1` Transaksi, `2` Saldo Subledger) |
| `PUT` | `/{id}` | `409` "Jenis kejadian ini masih punya aturan posting aktif. Nonaktifkan aturannya lebih dahulu." saat perlakuan diubah menjadi `2` dan masih ada aturan aktif |

#### Corporate / Accounting / Master Data / Posting Rule

| Method | Path | Perubahan |
|---|---|---|
| `POST` | `/` | `422` "Jenis kejadian {kode} berperlakuan Saldo Subledger. Pesan saldo tidak pernah menjadi jurnal, sehingga tidak memerlukan aturan posting." |
| `PUT` | `/{id}` | `422` yang sama bila jenis aturan berperlakuan Saldo Subledger |
| `PATCH` | `/{id}/deactivate` | Tidak berubah — tetap diterima untuk aturan berjenis saldo |

## 5. Verifikasi

### 5.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `git diff --numstat -- Areas/` | Tiga berkas, +27/−1 | `PASS` | Bagian 3.2 |
| Penelusuran `//` pada baris tambahan | Nol | `PASS` | `git diff` |
| Jalur lain yang membuat atau menghidupkan aturan posting | Hanya `CreateAsync` (`Set<AccPostingRule>().Add`); tidak ada endpoint aktivasi aturan | `PASS` | Grep `Areas/Corporate/AccountingManagement` |
| Pesan `409`/`422` dibandingkan dengan `validation-matrix.md` baris 232–233 | Sama persis | `PASS` | — |
| `Program.cs`, `ApplicationDbContext`, `Migrations/` | Tidak tersentuh | `PASS` | `git status --short` |
| Pemetaan acceptance ke source | 7/7 | `PASS` | Bagian 6 |
| `dotnet build` (Rizki) | `QuilvianSystemBackend.dll` 29 September 2026 10.20, sesudah source terakhir diubah 10.02; pesan `409`/`422` baru muncul pada uji, jadi yang berjalan memang build baru. Jumlah warning belum dilaporkan | `PASS` (tak langsung) | Tanggal berkas DLL; bagian 5.3 |
| Uji API S1–S3, S6–S9 (Rizki, skrip Playwright) | Seluruhnya sesuai harapan; S4 dan S5 tidak berlaku | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/be_p2_030_fe_p2_018_test_report.json` — bagian 5.3 |
| Uji layar L1–L5 | Dicatat di laporan `FE-ACC-P2-018` bagian 6.3 | `PASS` | Tangkapan layar `p2_l1`, `p2_l3`, `p2_l4`, `p2_l5` |

### 5.2 Skenario uji untuk Rizki — Swagger, lalu layar

**Build lebih dahulu** (backend dimatikan dulu bila sedang berjalan):

```bash
dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

**Persiapan, lewat layar — bukan SQL.** Pastikan `UJI-SALDO-028` aktif di layar Jenis Kejadian.
Opsi jenis hanya memuat jenis aktif.

| # | Langkah | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| S1 | `GET api/v1/corporate/accounting/event-types/options` | `200`; **setiap** butir punya `eventKind`; `UJI-SALDO-028` bernilai `2`, jenis lain `1`; jumlah dan urutan butir sama seperti sebelumnya | (6) |
| S2 | `GET api/v1/corporate/accounting/posting-rules?eventTypeId=<ID_UJI_SALDO_028>` | Catat apakah ada aturan lama berjenis saldo, beserta `id` dan `isActive`-nya — dipakai S5 | Persiapan |
| S3 | `POST api/v1/corporate/accounting/posting-rules` dengan body di bawah | `422` "Jenis kejadian UJI-SALDO-028 berperlakuan Saldo Subledger. Pesan saldo tidak pernah menjadi jurnal, sehingga tidak memerlukan aturan posting."; S2 ulang: jumlah aturan tetap | (1) |
| S4 | *(bila S2 menemukan aturan)* `PUT api/v1/corporate/accounting/posting-rules/<ID_ATURAN>` dengan body `PUT` di bawah | `422`, pesan sama dengan S3 | (2), (3) |
| S5 | *(bila aturan S2 masih aktif)* `PATCH api/v1/corporate/accounting/posting-rules/<ID_ATURAN>/deactivate` | `200`; `isActive` `false` | (4) |
| S6 | Buat jenis uji Transaksi: `POST api/v1/corporate/accounting/event-types` dengan body di bawah, lalu **di layar** Form Tambah Aturan Posting buat satu aturan untuk `UJI-030-TRX` (L2) | `201`, lalu aturan tersimpan | Persiapan (5) |
| S7 | `PUT api/v1/corporate/accounting/event-types/<ID_UJI_030>` dengan `EventKind` `2` | `409` "Jenis kejadian ini masih punya aturan posting aktif. Nonaktifkan aturannya lebih dahulu."; perlakuan tetap Transaksi | (5) |
| S8 | Nonaktifkan aturan `UJI-030-TRX` lewat layar, lalu ulangi S7 | `200` "…Jenis perlakuan berubah dari Transaksi menjadi Saldo Subledger." | (5) |
| S9 | `PUT` aturan `UJI-030-TRX` yang kini nonaktif, body `PUT` di bawah (akun dari rincian aturan) | `422` "Jenis kejadian UJI-030-TRX berperlakuan Saldo Subledger. …" | (3) |

Body S3 — `JournalTypeId` dan `AccountId` boleh id apa pun yang sah; penolakan terjadi sebelum baris diperiksa:

```json
{
  "LegalEntityId": "<LEGAL_ENTITY_ID>",
  "EventTypeId": "<ID_UJI_SALDO_028>",
  "JournalTypeId": "<ID_JENIS_JURNAL_MANA_PUN>",
  "Treatment": 2,
  "Lines": [
    { "LineNumber": 1, "AccountId": "<ID_AKUN_1>", "Side": 1 },
    { "LineNumber": 2, "AccountId": "<ID_AKUN_2>", "Side": 2 }
  ]
}
```

Body `PUT` untuk S4 dan S9 — sama dengan body S3 tanpa `LegalEntityId` dan `EventTypeId`.

Body S6:

```json
{
  "EventTypeCode": "UJI-030-TRX",
  "EventTypeName": "Uji BE-ACC-P2-030",
  "SourceModule": "Finance",
  "EventKind": 1
}
```

Body S7 dan S8:

```json
{
  "EventTypeName": "Uji BE-ACC-P2-030",
  "SourceModule": "Finance",
  "EventKind": 2
}
```

**Uji layar** — dijalankan bersama `FE-ACC-P2-018`; skenarionya di laporan
[`FE-ACC-P2-018`](../frontend/FE-ACC-P2-018.md) bagian 6.2.

**Bersih-bersih, lewat layar.** Nonaktifkan `UJI-030-TRX` dan `UJI-SALDO-028` di layar Jenis Kejadian.

### 5.3 Hasil uji Rizki — 29 September 2026

Rizki menjalankan skrip Playwright `test-with-agy/test-p2-030-and-018.mjs` (disusun agen Antigravity)
terhadap backend `https://localhost:7184` dan frontend `http://localhost:3000`, basis data
`QuilvianNewDevRizki`, akun SuperAdmin, badan hukum PT Metropolitan Medical Centre. Seluruh langkah
lewat API dan layar — skrip tidak memakai SQL. Response body di bawah dicocokkan agent dengan berkas
JSON mentah; `timestamp` berasal dari `ApiResponse` backend.

| # | Hasil mentah | Klasifikasi |
|---:|---|---|
| S1 | `200`; 2 butir, seluruhnya memuat `eventKind`; `UJI-SALDO-028` (`624c055e-…`) `eventKind` `2` | `PASS` |
| S2 | `200`; aturan berjenis `UJI-SALDO-028`: **0** | Persiapan — S4, S5 tidak berlaku |
| S3 | `422` "Jenis kejadian UJI-SALDO-028 berperlakuan Saldo Subledger. Pesan saldo tidak pernah menjadi jurnal, sehingga tidak memerlukan aturan posting." — `12:01:50.56+07:00` | `PASS` |
| S6 | `UJI-030-TRX` dibuat, id `abfed169-bb8f-4c56-8690-4284ab95d86c`, `eventKind` `1` | Persiapan |
| L2 | Aturan `c93ffeb4-aaad-40f3-aed2-543750f99479` dibuat `201` — **lewat API**, bukan lewat form | Persiapan |
| S7 | `409` "Jenis kejadian ini masih punya aturan posting aktif. Nonaktifkan aturannya lebih dahulu." — `12:01:56.53+07:00` | `PASS` |
| S8 | `deactivate` `200`, lalu `PUT` `200` "Jenis kejadian berhasil diperbarui. Jenis perlakuan berubah dari Transaksi menjadi Saldo Subledger."; `eventKind` `2`, `activePostingRuleCount` `0`, `accountingEventCount` `0` | `PASS` |
| S9 | `422` "Jenis kejadian UJI-030-TRX berperlakuan Saldo Subledger. …" — `12:01:57.46+07:00` | `PASS` |
| S4, S5 | Tidak dijalankan — tidak ada aturan lama berjenis saldo di dev, dan aturan baru semacam itu memang tidak dapat lagi dibuat aktif | `NOT APPLICABLE` — (4) dibuktikan dari source |
| Bersih-bersih | `UJI-030-TRX` dan `UJI-SALDO-028` dinonaktifkan lewat `PATCH …/deactivate` | Selesai |

Laporan ringkas agen di `testing/live-browser-testing-report-2026-09-29.md` tidak dipakai sebagai
bukti; yang dipakai JSON mentah dan tangkapan layar di atas.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti di source |
|---|---|---|
| (1) `POST` berjenis saldo → `422` berpesan persis, nol baris tersimpan | Terpenuhi di source | `CreateAsync` → `JenisSaldoSubledger` sebelum `SiapkanAsync`; tidak ada `Add` sebelum `return` |
| (2) `PUT` yang memindahkan aturan ke jenis saldo → `422` | Terpenuhi di source | `UpdatePostingRuleRequest` tanpa `EventTypeId` — perpindahan jenis lewat `PUT` tidak mungkin; jenis yang berubah menjadi saldo ditangkap (3) |
| (3) `PUT` aturan lama berjenis saldo → `422` | Terpenuhi di source | `UpdateAsync`: `aturan.EventType?.EventKind == SaldoSubledger` sebelum baris dihapus |
| (4) `PATCH .../deactivate` aturan berjenis saldo → `200` | Terpenuhi di source | `DeactivateAsync` tidak diubah dan tidak membaca `EventKind` |
| (5) Ubah perlakuan menjadi saldo saat ada aturan aktif → `409`; tanpa aturan aktif dan tanpa kejadian → `200` | Terpenuhi di source | `AccEventTypeService.UpdateAsync` — pemeriksaan baru hanya saat `perlakuanBerubah && perlakuan == SaldoSubledger` |
| (6) `/event-types/options` memuat `eventKind` angka; butir, urutan, penyaring tetap | Terpenuhi di source | Hanya proyeksi `Select` yang bertambah; `Where`/`OrderBy` tidak diubah; enum tanpa konverter string |
| (7) Nol migration | Terpenuhi | `git status --short` |

| Butir DoD | Keadaan |
|---|---|
| Source berubah | ✅ |
| Build Rizki 0 error | ✅ tak langsung — DLL 29 September 2026 10.20; warning belum dilaporkan |
| Uji Swagger dan layar tercatat | ✅ 29 September 2026 — bagian 5.3 (S4/S5 tidak berlaku) |
| Laporan task tertulis | ✅ |

## 7. Catatan penutup

| Hal | Isi |
|---|---|
| Status Git (`git status --short`) | ` M` tiga berkas source di bagian 3.2; ` M` laporan ini beserta roadmap, traceability, dan MODULE-STATUS. Tidak ada yang di-stage |
| Migration / database | Tidak ada |
| Risiko tersisa | (a) Jenis uji `UJI-030-TRX` tertinggal di dev sebagai master nonaktif — jenis kejadian tidak dapat dihapus. (b) Aturan berjenis saldo yang masih aktif di dev tetap aktif sampai dinonaktifkan lewat layar (S5); backend tidak menonaktifkannya otomatis, sesuai `ACC-DEC-096` |
| Temuan di luar cakupan | — |
| Task berikutnya | `BE-ACC-P2-029` |
