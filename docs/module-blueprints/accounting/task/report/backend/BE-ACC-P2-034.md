# Laporan Perubahan Backend — `BE-ACC-P2-034`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-034` |
| Judul | Jurnal dari kejadian dikenali sebagai jalur otomatis saat diajukan |
| Slice | `P2-CTRL` lanjutan |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-034` (revisi 7, `APPROVED` Rizki 29 September 2026) |
| Trace | `ACC-DEC-064`; `FR-P2-037`, `UAT-P2-25`; `BE-ACC-P2-012` acceptance (2); audit kesiapan 29 September 2026 gap G-01 |
| Contract version | Nol perubahan. `ACC-VALIDATION-0.10` bagian 3b; `ACC-API-0.14` `POST /journals/{id}/submit` |
| Dependency | `BE-ACC-P2-012` ✅, `BE-ACC-P2-021` ✅ |
| Pasangan frontend | Tidak ada |
| Klasifikasi | `LIGHT` — skor 2: berkas diperiksa 4–8 (0), satu berkas diubah (0), logika bisnis sederhana (1), menyentuh jalur pengajuan jurnal yang ada (1); kontrak, database, keamanan, UI 0 |
| Task mode | `BACKEND` (T-1 audit kesiapan, perintah Rizki 29 September 2026) |
| Target tulis | `JournalManagement/Services/AccJournalService.cs`; laporan ini; baris status roadmap, traceability, MODULE-STATUS. **Tidak** termasuk migration, build, maupun commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `618b206e` (branch `rizkiG`) + working tree `BE-ACC-P2-029`/`030` yang belum di-commit |
| Tanggal | 29 September 2026 |
| Status | **✅ SELESAI — 29 September 2026.** 7 dari 7 acceptance terpetakan ke source; +7/−0 di satu berkas; nol migration, nol endpoint, nol `Program.cs`, nol `//` baru. Build Rizki terbukti tak langsung: `QuilvianSystemBackend.dll` 29 September 2026 14.46, sesudah source diubah 14.36; jumlah warning tidak terbukti (klaim "0 warning" dari laporan agen tanpa keluaran build). Uji API S1–S5 dijalankan Rizki 14.57 WIB lewat skrip Playwright, response mentah tercatat (bagian 5.3): acceptance (1) dan (4) terbukti runtime; (2), (3), (5), (6), (7) dari source sesuai resep. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 29 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `JournalManagement` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (registry repository yang berlaku, `ACC-TD-015`) |
| Keberlakuan | `TOUCHED LEGACY` — `AccJournalService.BerasalDariJalurOtomatisAsync` (`BE-ACC-P2-012`). Nol entity, nol configuration, nol berkas baru |
| QBE yang berlaku | `QBE-SVC-001` (aturan tetap di service), `QBE-VAL-001` (`422` yang ada, kini tidak salah sasaran). Tidak berlaku: `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001` (nol endpoint dan bidang), `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-002/003`, `QBE-LOG-001` (jalur tulis yang ada tidak diubah pencatatannya) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/` suite; registry repository — sama dengan `BE-ACC-P2-029` pada hari yang sama, tanpa perubahan |

## 1. Masalah yang diperbaiki

Draft jurnal hasil kejadian keuangan yang menyentuh control account ditolak `422` saat diajukan:

```json
{ "statusCode": 422, "message": "Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual." }
```

Padahal jurnal itu **memang** lahir dari kejadian akuntansi, dan `ACC-DEC-064` justru menetapkan
kejadian akuntansi sebagai jalan yang sah menuju Kas Kasir, Kas Kecil, Piutang, dan Hutang. Penyebabnya:
penentu asal-usul yang dipakai saat pengajuan hanya mengenal hasil template, pembalikan penuh, dan
jurnal penutup tahun. Perlakuan **Langsung Disahkan** tidak terkena karena lewat
`SahkanDariKejadianAsync`; perlakuan **Buat Draft** — bawaan aturan posting — terkena.

## 2. Proses bisnis

1. Finance mengirim kejadian, misalnya pembayaran pasien Rp 250.000.
2. Aturan posting `PATIENT_PAYMENT` berperlakuan Buat Draft menyusun jurnal debit Kas Kasir, kredit
   Pendapatan — status Draft, ditautkan ke kejadiannya lewat `AccAccountingEvent.JournalId`.
3. Petugas akuntansi membuka rincian jurnal itu dan menekan **Ajukan**. **Sesudah task ini:**
   pengajuan diterima dan jurnal menunggu persetujuan.
4. Penyetuju — orang lain, prinsip empat mata — menyetujui, lalu jurnal disahkan.
5. Jurnal yang disusun manusia lewat Form Jurnal tetap tidak boleh menyentuh control account.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `JournalManagement/Services/AccJournalService.cs` | `CreateManualAsync` (327), `UpdateAsync` (363), `SubmitAsync` (547), penyesuaian `ReverseAsync` (928), `SahkanDariKejadianAsync`, `PeriksaControlAccountSaatDiajukanAsync`, `BerasalDariJalurOtomatisAsync`; `ApproveAsync`/`PostAsync` tidak memeriksa control account |
| `AccountingEvent/Services/AccAccountingEventService.cs` baris 596–637 | Jurnal kejadian dibuat lewat `CreateAsync`, `JournalId` ditulis dalam transaksi yang sama |
| `AccountingEvent/Models/AccAccountingEvent.cs` | `JournalId` (`Guid?`), `IsDelete` dari `IdentityModel` |
| `JournalManagement/DTOs`, `Controllers/JournalController.cs` | Bentuk body dan rute untuk resep uji |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
|---|---|
| `JournalManagement/Services/AccJournalService.cs` | + `using ...AccountingEvent.Models`. `BerasalDariJalurOtomatisAsync`: sesudah pemeriksaan hasil template, tambah pemeriksaan **hasil kejadian** — `AnyAsync` atas `AccAccountingEvent` yang tidak terhapus dengan `JournalId == jurnal.Id`; bila ada, jurnal diakui sebagai jalur otomatis. +7/−0 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Hal | Dampak |
|---|---|
| API | Bentuk tidak berubah. `POST /journals/{id}/submit` berhenti menolak draft hasil kejadian — kode kini setia pada `ACC-VALIDATION-0.10` bagian 3b |
| Database | Satu query baca tambahan, hanya bila jurnal yang diajukan menyentuh control account. Nol migration |
| Keamanan | Tidak berubah. Pengecualian ditentukan jejak data yang hanya ditulis jalur kejadian, bukan isian pengguna |
| `Program.cs`, `ApplicationDbContext` | Tidak tersentuh |

### 3.4 Keputusan implementasi yang perlu diketahui

1. **Dasar pengecualian adalah tautan kejadian, bukan status kejadian.** Kejadian berstatus apa pun yang
   menunjuk jurnal ini — dalam praktik `Terjurnal` — cukup. Hanya kejadian terhapus yang tidak dihitung
   (acceptance 6).
2. **Diletakkan sesudah pemeriksaan template, sebelum pembalikan dan `JT`.** Urutannya tidak mengubah
   hasil ketiga jalur lama: setiap pemeriksaan hanya dapat mengembalikan `true` bagi asal-usulnya sendiri.
3. **Celah penyuntingan tertutup oleh perilaku yang ada.** `UpdateAsync` menolak penyuntingan draft yang
   barisnya menyentuh control account, sehingga draft kejadian tidak dapat diganti isinya lalu diajukan
   dengan akun control lain. Bila OQ-034-2 kelak mengizinkan penyuntingan, pengecualian ini wajib ditinjau.
4. **Komentar XML lama tidak diubah.** Keterangan di atas `BerasalDariJalurOtomatisAsync` masih menulis
   "Jalur kejadian akuntansi (`P2-1`) belum ada di kode" — kini usang, tetapi dibiarkan karena aturan
   nol komentar baru. Sama dengan penanganan komentar usang pada `BE-ACC-P2-014`.

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Journal — perilaku yang berubah

| Method | Path | Sebelum | Sesudah |
|---|---|---|---|
| `POST` | `api/v1/corporate/accounting/journals/{id}/submit` | Draft hasil kejadian yang menyentuh control account → `422` | `200`, status `PendingApproval` |

Endpoint lain tidak berubah: `POST /journals`, `PUT /journals/{id}`, dan `POST /journals/{id}/reverse`
dengan penyesuaian tetap menolak control account `422`.

## 5. Verifikasi

### 5.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `git diff --stat -- Areas/Corporate/AccountingManagement/JournalManagement/` | Satu berkas, +7/−0 | `PASS` | — |
| Penelusuran `//` pada baris tambahan | Nol | `PASS` | `git diff` |
| Titik pemeriksaan control account | Empat: buat manual, ubah, ajukan, penyesuaian. Hanya **ajukan** yang memakai penentu asal-usul; tiga lainnya tidak disentuh | `PASS` | Grep `AlasanControlAccountAsync` |
| Pemetaan acceptance ke source | 7/7 | `PASS` | Bagian 6 |
| `dotnet build` (Rizki) | `QuilvianSystemBackend.dll` 14.46, sesudah source diubah 14.36; S3 `200` hanya mungkin dari kode baru. Klaim "0 warning, 0 error" berasal dari laporan agen, keluaran build tidak dilampirkan | `PASS` (tak langsung) | Tanggal berkas DLL; bagian 5.3 |
| Uji API S1–S5 (Rizki, skrip Playwright) | S1–S4 sesuai harapan; S5 `403` empat mata (opsional, satu pengguna) | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/be_p2_034_s1_s5_report.json` — bagian 5.3 |
| Automated test | Bukan acceptance (`ACC-DEC-081`) | `NOT RUN` | — |

### 5.2 Resep uji untuk Rizki — Swagger, tanpa SQL

**Build lebih dahulu** (backend dimatikan dulu bila sedang berjalan):

```bash
dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

| # | Langkah | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| S1 | `GET api/v1/corporate/accounting/journals?search=JU/2031/01/00001`, lalu `GET …/journals/<ID>` | Status Draft. **Catat kode akun tiap baris**, lalu cocokkan di layar Daftar Akun: apakah salah satunya bertanda control account? Bila **ya**, lompat ke S3 memakai jurnal ini | Persiapan |
| S2 | *(hanya bila S1 tidak menyentuh control account)* `POST api/v1/corporate/accounting/accounting-events` dengan body S2 — jenis `PATIENT_PAYMENT` yang aturannya menyentuh control account. Bila aturan itu belum menyentuh control account, ubah dulu satu barisnya ke **1-1002 Kas Kasir** di layar Aturan Posting | `201`, status Terjurnal, tanda terima memuat nomor jurnal Draft baru | Persiapan |
| S3 | `POST api/v1/corporate/accounting/journals/<ID_DRAFT_KEJADIAN>/submit` | **`200`**, status `PendingApproval` (Menunggu Persetujuan). Sebelum build ini jawabannya `422` "…bukan jurnal manual." | (1) |
| S4 | `POST api/v1/corporate/accounting/journals` dengan body S4 (jurnal manual ke 1-1002 Kas Kasir) | `422` "Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual." | (4) |
| S5 | *(opsional, pengguna kedua berizin)* `POST …/journals/<ID_DRAFT_KEJADIAN>/approve`, lalu `…/post` | `200` dua kali, jurnal Disahkan. Satu pengguna saja: `approve` menjawab `403` empat mata — itu bukan kegagalan task ini | (2) |

Body S2:

```json
{
  "EventNumber": "EVT-UJI-034A",
  "EventTypeCode": "PATIENT_PAYMENT",
  "SourceModule": "Finance",
  "SourceTransactionId": "UJI-034-2031-01",
  "SourceVersion": "1",
  "EventOccurredAt": "2031-01-20T08:00:00+07:00",
  "AccountingDate": "2031-01-20",
  "Amount": 250000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "<LEGAL_ENTITY_ID>",
  "CorrelationId": "0f034000-0000-4000-8000-000000000001",
  "CausationId": "0f034000-0000-4000-8000-000000000001"
}
```

Body S4 — `JournalTypeId` jenis `JU`; `AccountId` baris 1 = id **1-1002 Kas Kasir**, baris 2 = akun
pendapatan mana pun yang bukan control:

```json
{
  "LegalEntityId": "<LEGAL_ENTITY_ID>",
  "JournalTypeId": "<ID_JENIS_JU>",
  "AccountingDate": "2031-01-20",
  "Description": "Uji regresi BE-ACC-P2-034",
  "Lines": [
    { "LineNumber": 1, "AccountId": "<ID_1-1002>", "DebitAmount": 1000.00, "CreditAmount": 0 },
    { "LineNumber": 2, "AccountId": "<ID_AKUN_PENDAPATAN>", "DebitAmount": 0, "CreditAmount": 1000.00 }
  ]
}
```

Periode Januari 2031 dipakai karena sudah dibangkitkan dan terbuka (`BE-ACC-P2-026` B2), dan jauh
dari bulan yang sedang diuji rekonsiliasinya.

**Bersih-bersih.** Jurnal uji yang menunggu persetujuan dapat ditolak beralasan lewat
`POST …/journals/<ID>/reject`, body `{ "reason": "Membersihkan data uji 034" }` — butuh izin
`Journal : Approve`, sama dengan menyetujui; bila hanya ada satu pengguna, biarkan menunggu persetujuan
di Januari 2031 yang tidak sedang ditutup. Bila baris aturan
`PATIENT_PAYMENT` diubah pada S2, kembalikan lewat layar Aturan Posting.

### 5.3 Hasil uji Rizki — 29 September 2026

Rizki menjalankan `test-with-agy/test-p2-034-s1-s5.mjs` (disusun agen Antigravity) terhadap backend
`https://localhost:7184`, akun SuperAdmin, badan hukum PT Metropolitan Medical Centre, 14.57.28–14.57.35 WIB.
Seluruh langkah lewat API — skrip tidak memakai SQL. Tabel di bawah dicocokkan agent dengan JSON mentah
`be_p2_034_s1_s5_report.json`.

**Run sebelumnya tidak tercatat.** Saat S1, `JU/2031/01/00001` sudah berstatus `2` (Menunggu
Persetujuan), bukan Draft seperti ditulis ringkasan agen, dan nomor `JU/2031/01/00002` sudah terpakai.
Keduanya jejak percobaan sebelum run ini yang tidak meninggalkan JSON. Bukti acceptance (1) karena itu
diambil **hanya** dari draft baru `JU/2031/01/00003` yang lahir dan diajukan di dalam run ini.

| # | Langkah | Hasil Aktual | Status | Keterangan |
|---:|---|---|---|---|
| S1 | Cek `JU/2031/01/00001` (`a06638ac-…`) | Status **`2`** (sudah diajukan run sebelumnya); baris `1-1002 Kas Kasir` debit 200.000 dan `1-2001 Piutang Pasien Umum` kredit 200.000, keduanya `isControlAccount: true` — aturan `PATIENT_PAYMENT` memang menyentuh dua control account | Persiapan | — |
| S2 | `POST /accounting-events` `EVT-UJI-034A` (`PATIENT_PAYMENT`, 250.000) | `201`, `Terjurnal`; draft `JU/2031/01/00003` (`231bfe04-cd04-47f2-a149-f62038d40482`) debit Kas Kasir, kredit Piutang Pasien Umum. Aturan posting tidak perlu diubah | Persiapan | — |
| S3 | `POST /journals/231bfe04-…/submit` | `200` "Jurnal berhasil diajukan."; status `2` Menunggu Persetujuan (14.57.34 WIB) | `PASS` | **Acceptance (1)** |
| S4 | Regresi: Jurnal manual ke `1-1002 Kas Kasir` | `422 Unprocessable Entity`: *"Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual."* | `PASS` | **Membuktikan Acceptance (4)** |
| S5 | `POST /journals/231bfe04-…/approve` oleh pengaju sendiri | `403` "Anda tidak dapat menyetujui jurnal yang Anda buat sendiri." (14.57.35+07:00) — aturan empat mata, **bukan** bukti acceptance (2); tidak ada pengguna kedua | `SKIPPED` | (2) tetap dari source |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti di source |
|---|---|---|
| (1) Draft hasil kejadian menyentuh control account → ajukan `200` | Terpenuhi di source & live API | `BerasalDariJalurOtomatisAsync` → `hasilKejadian` → `true`; terbukti via S3 |
| (2) Disetujui dan disahkan seperti biasa | Terpenuhi di source | `ApproveAsync`/`PostAsync` tidak memeriksa control account. S5 `403` adalah aturan empat mata, bukan bukti persetujuan |
| (3) Draft Form Jurnal sebelum akun ditandai control tetap ditolak saat diajukan | Terpenuhi di source | Draft manual tidak punya tautan kejadian → `hasilKejadian` salah → pemeriksaan lama berjalan |
| (4) Jurnal manual baru, ubah draft, penyesuaian `JP` tetap `422` | Terpenuhi di source & live API | `CreateManualAsync` memanggil `AlasanControlAccountAsync` langsung; terbukti via S4 |
| (5) Template, pembalikan penuh, `JT` tetap dikenali | Terpenuhi di source | Ketiga pemeriksaan tidak diubah |
| (6) Kejadian terhapus tidak menjadi dasar pengecualian | Terpenuhi di source | `!x.IsDelete` |
| (7) Nol migration, endpoint, bidang, berkas frontend | Terpenuhi | `git status --short` |

| Butir DoD | Keadaan |
|---|---|
| Source berubah | ✅ |
| Build Rizki 0 error | ✅ tak langsung — DLL 29 September 2026 14.46; warning belum terbukti |
| Uji Swagger tercatat, termasuk regresi (3)–(5) | ✅ 29 September 2026 — bagian 5.3: (1) dan regresi (4) runtime; regresi (3) dan (5) dari source sesuai resep |
| Baris `FR-P2-037` di traceability diperbarui | ✅ bagian 3f |
| Laporan task tertulis | ✅ |

## 7. Catatan penutup

| Hal | Isi |
|---|---|
| Status Git (`git status --short`) | ` M` `AccJournalService.cs`; ditambah working tree `BE-ACC-P2-029`/`030` dan dokumen yang belum di-commit. Tidak ada yang di-stage |
| Migration / database | Tidak ada |
| Risiko tersisa | Pengecualian berdasarkan asal-usul; aman selama draft kejadian tidak dapat disunting ke akun control lain (OQ-034-2) |
| Data uji tertinggal | `JU/2031/01/00001` dan `00003` menunggu persetujuan di Januari 2031; `EVT-UJI-034A` Terjurnal. Tidak menahan periode yang sedang dipakai |
| Temuan di luar cakupan | OQ-034-1 — `DELETE /journals/{id}` atas draft hasil kejadian membuat kejadian tetap `Terjurnal` menunjuk jurnal terhapus. Menunggu keputusan owner |
| Task berikutnya | T-2 audit kesiapan (`FE-ACC-009`, menunggu build owner); keputusan OQ-034-1/2 lewat `grill-me` |
