# Laporan Perubahan Backend — `BE-RJE-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-011` |
| Judul | Pekerja kirim ulang fakta klinis |
| Slice | `MVP-3` — `EPIC RJE-06` Keandalan |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-011` |
| Trace | `FR-RJE-050`; `AC-RJ-013`, `AC-RJ-014`; `RJ-E2E-DEC-009`, `026`; `contracts/integration-contract.md` V2-3; rujukan `RJ-DOC-BE-005` AC `5`–`6` |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-001` ✅ |
| Klasifikasi | `MEDIUM` — hosted service baru dan perubahan pada producer fakta klinis yang dipakai semua modul klinis |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2af16d93` (`sukmagp`), working tree belum di-commit |
| Tanggal | 29 September 2026 |
| Status | ✅ **SELESAI** — ketiga acceptance criteria terbukti; satu perbaikan ketahanan producer (bagian 1) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` (producer fakta, pekerja); membaca master `BillingManagement` (`MstBillingSyncPolicy`) |
| Registry | `Cli` `ACTIVE`; `Bil` `ACTIVE` |
| Keberlakuan | `NEW CODE` — `ClinicalManagement/Workers/ClinicalFactDispatchWorker.cs` (folder baru sesuai pohon folder V2); `TOUCHED LEGACY` — `ClinicalMilestoneFactProducer` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-TXN-001` (penyerahan di luar transaksi klinis; scope per fakta), `QBE-AUD-001` |
| Tidak berlaku | Model, configuration, migration (kolom `NextDispatchAttemptAt`/`ReconciliationRequiredAt` sudah ada sejak `BE-RJE-001`), endpoint, permission |
| Wewenang | `RJ-E2E-DEC-026` |

---

## 1. Masalah yang diperbaiki

Bila Billing tidak menjawab saat dokter menyelesaikan pelayanan, fakta klinis tercatat
`OutcomeUnknown` dan tidak ada yang mengirimnya lagi. Selama itu, revisi berikutnya atas pelayanan
yang sama ditolak ("selesaikan rekonsiliasi lebih dulu"), padahal belum ada antrean yang
menampungnya.

**Contoh:** dokter mengerjakan tindakan tepat saat tabel Billing terkunci.

- Tindakan tetap tersimpan, dan dokter menerima `200`.
- Fakta dicatat "belum pasti, coba lagi 60 detik lagi".
- Semenit kemudian pekerja mengirim fakta yang sama dengan kunci yang sama. Tagihan muncul sekali,
  dan fakta berstatus `Dispatched`.

**Perbaikan ketahanan yang ditemukan saat uji.**

- Billing memakai `DbContext` yang sama dengan producer. Setelah panggilan Billing habis waktu,
  koneksi itu tertinggal rusak.
- Akibatnya penyimpanan status `OutcomeUnknown` ikut gagal, dan **dokter menerima `500`** untuk
  tindakan yang sebenarnya sudah tercatat (run pertama R1: `500` setelah 61 detik; fakta tertinggal
  `Pending`).
- Producer kini membersihkan change tracker dan menutup koneksi setelah penyerahan gagal, lalu
  membaca ulang fakta dengan koneksi baru. Bila pencatatan tetap gagal, pemanggil klinis menerima
  `OutcomeUnknown` (aman secara klinis) dan fakta dibiarkan `Pending` untuk pekerja.
- `BillingFolioService` sendiri sudah membersihkan change tracker konteks bersama ini saat konflik,
  jadi pemanggil klinis sudah terbiasa dengan perilaku itu.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Fakta yang hasil penyerahannya tidak pasti dikirim ulang dengan identitas yang sama sampai pasti, atau masuk antrean |
| Pelaku | Sistem (pekerja latar); admin Billing mengatur kebijakan `FACT_DISPATCH` |
| Penjadwalan | Setiap hasil `OutcomeUnknown` (penyerahan pertama maupun kirim ulang) dijadwalkan `min(Base × 2^(n−1), Max)` menurut `FACT_DISPATCH`. Percobaan ke-`MaxAttemptCount` → `ReconciliationRequiredAt` + `RETRY_EXHAUSTED`. Tanpa kebijakan aktif → `ReconciliationRequiredAt` + `SYNC_POLICY_INACTIVE`. Hasil pasti (`Dispatched`, `Rejected`) menghapus jadwal |
| Kirim ulang | `ClinicalMilestoneFactProducer.RedispatchAsync(id)` menyusun permintaan dari baris tersimpan dan memakai `IdempotencyKey` yang sama. Snapshot bertipe `text` yang sudah dinormalisasi, dan waktu/desimal diseragamkan ke ketelitian kolom, sehingga sidik jari Billing identik. Tidak pernah menerbitkan revisi baru. Fakta yang sudah pasti atau sudah menunggu rekonsiliasi dikembalikan apa adanya |
| Pekerja | `ClinicalFactDispatchWorker`: setiap 10 detik, paling banyak 50 fakta `OutcomeUnknown`/`Pending` yang jatuh tempo (`Pending` tanpa jadwal baru dianggap tertinggal setelah `BaseDelaySeconds`); satu scope per fakta; fail-closed bila kebijakan tidak aktif |
| Batas scope Dokter | AC `5`–`6` `RJ-DOC-BE-005` (fakta yang belum terkirim ditemukan dan dikirim ulang dengan identitas sama) **diwujudkan** di sini. AC `1`–`4` (deteksi *producer gap*) tetap milik roadmap Dokter dan wajib memanggil `RedispatchAsync`, bukan membangun jalur kedua |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ClinicalMilestoneFactProducer.cs` (`EmitAsync`, `DecideNextStep`, `DispatchAsync`,
`ApplyDispatchOutcomeAsync`, `Normalize`), `CliClinicalMilestoneFact.cs`,
`ClinicalMilestoneFactDtos.cs`, `BillingFolioService.cs` (sidik jari, transaksi, jalur
`BIL_OUTCOME_UNKNOWN`), `MstBillingSyncPolicy.cs`, `BilInvoiceSyncWorker.cs` (pola `BE-RJE-010`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs` | `RedispatchAsync`; `ScheduleRedispatchAsync` dipanggil setiap pencatatan hasil; koneksi dibersihkan setelah penyerahan gagal; kegagalan pencatatan tidak lagi naik ke pemanggil klinis |
| `Areas/HealthServices/ClinicalManagement/Workers/ClinicalFactDispatchWorker.cs` | **Baru.** `BackgroundService` kirim ulang fakta |
| `Program.cs` | `AddHostedService<ClinicalFactDispatchWorker>()` beserta `using` |
| `docs/…/00-interview-decisions.md` | `RJ-E2E-DEC-026` (wewenang `BE-RJE-011`, `013`, `012`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. **Perilaku berubah:** kegagalan mencatat hasil penyerahan tidak lagi menjadi `500` bagi pemanggil klinis |
| Database | Tanpa skema. `BillingOutcomeCode` fakta memakai `RETRY_EXHAUSTED`/`SYNC_POLICY_INACTIVE` saat berhenti otomatis |
| Keamanan/Auth | Tidak ada endpoint. Kirim ulang tercatat atas nama aktor fakta asli, dengan audit `ClinicalFact.Dispatch` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje011b` | `0 Error(s)`, `230 Warning(s)`, 2 menit 5 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Findings: none`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R3 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 29 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**; log start
"ClinicalFactDispatchWorker aktif. Poll=10s, Batch=50".

- **Cara mensimulasikan "Billing tidak menjawab":** sesi database kedua menahan
  `LOCK TABLE "BilProcessingEffect" IN EXCLUSIVE MODE`. Insert folio tertahan dan habis waktu
  setelah 30 detik (default Npgsql). Melepas lock berarti Billing pulih.
- **Persiapan sebelum start:** 12 fakta sintetis dari uji sebelumnya (`TEST-RJE003/005/009/010-FACT-*`)
  tercatat `Pending` padahal sudah punya efek folio, karena dulu diserahkan langsung ke folio tanpa
  lewat producer. Keduabelasnya ditandai `Dispatched` beserta tautan efeknya lewat SQL, supaya
  pekerja tidak mengirim ulang data uji yang sebenarnya sudah terkirim.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Kebijakan asli (5/60/3600). Tindakan dikerjakan saat folio ditahan, lalu lock dilepas | `execute` **`200`** dalam 31 detik; fakta v1 `OutcomeUnknown`, percobaan 1, kunci `CF-e20833cb…-1`, jadwal +60 detik. Pekerja mengirim ulang pada 02:14:27 UTC: `Dispatched`, percobaan 2, **kunci sama**, **tetap 1 revisi**; 1 efek `Synced`; 1 item `PERFORMED` Rp198.000 | 1 | `PASS` |
| R2 | Kebijakan uji sementara 5/10/20; folio ditahan terus | Percobaan 1 (langsung) + 4 (pekerja); setelah percobaan ke-5 (02:17:59 UTC) **`ReconciliationRequiredAt` terisi**, kode `RETRY_EXHAUSTED`, jadwal kosong. 25 detik setelah lock dilepas tidak ada kirim ulang otomatis; nol efek. Kebijakan dipulihkan 5/60/3600 | 2 | `PASS` |
| R3a | Kebijakan dinonaktifkan; tindakan dikerjakan saat folio ditahan | `200`; fakta langsung `ReconciliationRequiredAt` + `SYNC_POLICY_INACTIVE`, tanpa jadwal; 25 detik kemudian tidak berubah, nol efek | 3 | `PASS` |
| R3b | Fakta terjadwal; kebijakan lalu dinonaktifkan dan jadwal dimajukan | Dalam 10 detik pekerja mengisi `ReconciliationRequiredAt` + `SYNC_POLICY_INACTIVE` tanpa mengirim; nol efek. Kebijakan diaktifkan kembali | 3 | `PASS` |

**Percobaan yang tidak dipakai sebagai bukti.**
- Run R1 pertama, **sebelum** perbaikan ketahanan: `execute` `500` setelah 61 detik, dan fakta
  tertinggal `Pending` karena pencatatan hasil timeout pada koneksi bersama yang rusak. Pekerja tetap
  memulihkannya (`Dispatched`, kunci sama, satu item), tetapi `500` itu yang mendorong perbaikan di
  bagian 1.
- Dua kegagalan skrip uji: kutipan rusak dan nama variabel tertimpa.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan dan sesi database kedua.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Kebijakan `FACT_DISPATCH` | Aktif 5/60/3600 |
| Fakta | 3 fakta tindakan uji `TEST-RJE011` menunggu rekonsiliasi (R2 `RETRY_EXHAUSTED`, R3a/R3b `SYNC_POLICY_INACTIVE`) — disiapkan sebagai data uji antrean `BE-RJE-012` |
| Fakta sintetis lama | 12 baris `TEST-RJE00x-FACT-*` kini `Dispatched` (bagian 5.1) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `OutcomeUnknown` dikirim ulang dengan `IdempotencyKey` sama; tidak ada fakta baru (`AC-RJ-014`) | Terpenuhi | R1 |
| 2. Batas percobaan → `ReconciliationRequiredAt` terisi | Terpenuhi | R2 |
| 3. Kebijakan nonaktif → tidak ada kirim ulang otomatis | Terpenuhi | R3a, R3b |
| DoD: laporan | Terpenuhi | Berkas ini |
| DoD: catatan rujukan ke `RJ-DOC-BE-005` | Terpenuhi | Bagian 2 dan komentar kelas pekerja |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar log aplikasi yang sudah ada |
| Risiko tersisa | Fakta `Rejected` oleh Billing tidak dikirim ulang dan tidak masuk antrean otomatis (penolakan kontrak bersifat pasti). Deteksi *producer gap* (fakta yang tidak pernah tertulis) tetap di roadmap Dokter. Pekerja berjalan di setiap instance; aman karena Billing idempoten per kunci |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | Source dan dokumen `M`/baru; belum di-stage atau di-commit |
| Langkah berikutnya | `BE-RJE-013`, lalu `BE-RJE-012` (API antrean memakai `RedispatchAsync` dan jembatan) |
