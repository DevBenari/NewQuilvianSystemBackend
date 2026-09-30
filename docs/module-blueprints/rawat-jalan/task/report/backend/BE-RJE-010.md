# Laporan Perubahan Backend — `BE-RJE-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-010` |
| Judul | Pekerja kirim ulang sinkron invoice |
| Slice | `MVP-3` — `EPIC RJE-06` Keandalan |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-010` |
| Trace | `FR-RJE-051`; `AC-RJ-002`, `003`; `RJ-E2E-DEC-009`, `025`; `02-backend-architecture.md` V2.11 dan tabel `BilInvoiceSyncWorker` |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-003` ✅ |
| Klasifikasi | `MEDIUM` — hosted service baru; aturan percobaan sudah ada di jembatan sejak `BE-RJE-003` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `eeb18c57` (`sukmagp`) + perubahan `BE-RJE-008`/`009` yang belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — keempat acceptance criteria terbukti; waktu jadwal tercatat |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Registry | `Bil` `ACTIVE` |
| Keberlakuan | `NEW CODE` — `Billing/Workers/BilInvoiceSyncWorker.cs` (folder baru, sesuai pohon folder V2) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-TXN-001` (scope dan `DbContext` per siklus; penulisan status dijaga token konkurensi), `QBE-AUD-001` |
| Tidak berlaku | Model, configuration, migration, endpoint, permission — tidak disentuh |
| Wewenang | `RJ-E2E-DEC-025`, termasuk mengubah sementara kebijakan `INVOICE_SYNC` di database uji |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, penerusan ke invoice yang gagal sementara hanya **dijadwalkan**. Jembatan
mencatat `Failed` beserta waktu percobaan berikutnya, tetapi tidak ada yang benar-benar mencoba
lagi. Tagihan tertinggal sampai ada yang memeriksa manual.

**Contoh:** dokter menyelesaikan *Nebulizer* tepat saat database invoice sibuk. Klinisnya tetap
tersimpan, dan tagihannya dicatat "gagal, coba lagi 60 detik lagi". Sekarang pekerja latar
mengirim ulang tepat pada jadwal itu, dan item muncul sekali di invoice tanpa campur tangan
petugas.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Gangguan sementara tidak membuat tagihan tertinggal atau tertagih dua kali |
| Pelaku | Sistem (pekerja latar); admin Billing mengatur kebijakan `INVOICE_SYNC` tanpa rilis |
| Siklus | Setiap 10 detik, satu scope baru: baca kebijakan `INVOICE_SYNC` aktif, ambil paling banyak 50 efek yang jatuh tempo, urut dari yang paling lama |
| Jatuh tempo | `Failed` yang `InvoiceSyncNextAttemptAt`-nya sudah lewat; `Pending` bertanggal jadwal yang sudah lewat; `Pending` tanpa jadwal yang sudah menunggu lebih dari `BaseDelaySeconds` (supaya tidak berebut dengan pemanggilan langsung sesaat setelah commit) |
| Pengiriman | `BillingClinicalChargeBridgeService.TrySyncEffectAsync`. Jembatan yang menghitung percobaan, menjadwalkan `min(Base × 2^(n−1), Max)`, dan memindahkan ke antrean `RETRY_EXHAUSTED` pada batas percobaan — aturan yang sama dengan pemanggilan langsung |
| Fail-closed | Tanpa kebijakan aktif: pekerja **tidak** mengirim ulang; baris yang jatuh tempo langsung ke antrean `SYNC_POLICY_INACTIVE`. Gagal pertama saat kebijakan mati juga langsung ke antrean (sudah di jembatan) |
| Ganda | Pekerja, pemanggilan langsung, dan instance lain aman berjalan bersamaan: status ditulis dengan token `InvoiceSyncVersion`, kunci idempotency invoice deterministik per fakta dan versi |
| Hasil akhir | Item muncul sekali, atau baris berada di antrean rekonsiliasi dengan sebab terbaca |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientIntegrationOutboxWorker.cs` (pola), `AccRecurringJournalSchedulerHostedService.cs`,
`BillingClinicalChargeBridgeService.RecordAsync` dan `BackoffSeconds`, `MstBillingSyncPolicy.cs`
beserta check constraint-nya, `BillingOperationalConfigurations.cs` (index `InvoiceSyncStatus` +
`InvoiceSyncNextAttemptAt`, token `InvoiceSyncVersion`), `Program.cs` (pendaftaran Billing),
`LoggerService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Workers/BilInvoiceSyncWorker.cs` | **Baru.** `BackgroundService`: siklus 10 detik, batch 50, pemilihan jatuh tempo, fail-closed `ParkAsync` |
| `Program.cs` | `AddHostedService<BilInvoiceSyncWorker>()` satu kali di blok Billing, beserta `using` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada |
| Database | Tanpa skema. Query memakai index `(InvoiceSyncStatus, InvoiceSyncNextAttemptAt)` dari `BE-RJE-001` |
| Keamanan/Auth | Tidak ada endpoint. Percobaan pekerja tercatat di audit `BillingBridge.*` tanpa pengguna (`Path = "-"`) |
| Operasi | Pekerja aktif di setiap instance aplikasi; aman berkat penjaga versi. Belum ada tombol konfigurasi untuk mematikannya selain menonaktifkan kebijakan `INVOICE_SYNC` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje010` | `0 Error(s)`, `230 Warning(s)`, 1 menit 53 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Findings: none`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R4 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**. Log start:
"BilInvoiceSyncWorker aktif. Poll=10s, Batch=50".

- **Sebelum start:** tidak ada baris `Pending`/`Failed`, jadi sapuan pertama tidak menyentuh data
  lama.
- **Cara mensimulasikan "koneksi invoice terputus":** sesi database terpisah menahan advisory lock
  `BIL_ENCOUNTER_<kunjungan>` yang juga diambil `UpsertChargeAsync`. Perintah jembatan habis waktu
  setelah 30 detik (default Npgsql), lalu tercatat `TRANSIENT_FAILURE`. Melepas lock berarti
  koneksi pulih.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Kebijakan asli (5/60/3600). Tindakan dikerjakan saat lock ditahan, lalu lock dilepas | `execute` `200` setelah 31 detik; efek `Failed` `TRANSIENT_FAILURE`, percobaan 1, jadwal 09:35:02 UTC (+60 detik). Pekerja mengirim ulang pada **09:35:03**; efek `Synced`; **1** item `PERFORMED` Rp1.132.000, **1** receipt | 1 | `PASS` |
| R2 | Kebijakan uji sementara 5/10/20 (batas bawah check constraint: Base ≥ 10); lock ditahan terus | Percobaan pada 09:36:02 (langsung), lalu 09:37:12, 09:38:02, 09:38:52, 09:39:43 (pekerja). Setelah percobaan ke-5: `ReconciliationRequired` **`RETRY_EXHAUSTED`**, nol item. Kebijakan dipulihkan 5/60/3600 | 2 | `PASS` |
| R3a | Kebijakan dinonaktifkan; tindakan dikerjakan saat lock ditahan | Gagal pertama langsung `ReconciliationRequired` **`SYNC_POLICY_INACTIVE`** tanpa jadwal; 25 detik kemudian tidak berubah — pekerja tidak mengirim ulang | 3 | `PASS` |
| R3b | Baris `Failed` terjadwal; kebijakan lalu dinonaktifkan dan jadwal dimajukan ke masa lalu | Dalam 10 detik pekerja memindahkannya ke antrean **`SYNC_POLICY_INACTIVE`** tanpa mengirim | 3 | `PASS` |
| R4 | Fakta Lab sintetis diserahkan saat lock ditahan (`Failed`); jadwal dimajukan ke "sekarang"; lock dilepas bersamaan dengan 6 pemanggilan langsung identik secara paralel, sementara pekerja berjalan | Keenamnya `200`; efek `Synced` pada 09:45:07 dengan `InvoiceSyncVersion = 2` (satu tulisan gagal lalu satu tulisan sukses; lima pemroses lain dibuang penjaga versi); audit hanya satu `BillingBridge.Sync`; **1** item `ACCEPTED` Rp1.251.000, **1** receipt | 4 | `PASS` |

**Percobaan yang tidak dipakai sebagai bukti.**
- Kebijakan uji Base = 1 detik ditolak check constraint `CK_MstBillingSyncPolicy_BaseDelaySeconds`;
  diganti Base = 10.
- Konsultasi "segar" habis. Uji berikutnya memakai konsultasi berjalan dengan invoice `OPEN`, dan
  tindakan yang sama tidak boleh dipilih dua kali dalam satu konsultasi.
- R4 versi pertama: pemanggilan ulang fakta dari producer ditolak `400` (format tanggal skrip)
  lalu `409` "input material yang berbeda". Skrip tidak dapat menyusun ulang isi fakta producer
  secara persis — itu pelindung folio yang bekerja, bukan uji konkurensi. Pada run itu pekerja
  sendirian menyinkronkan satu item. Uji konkurensi diulang dengan fakta sintetis yang isinya
  dikendalikan skrip (R4 di atas).

**Batas bukti R4.** Keenam pemroses langsung terbukti bersaing, satu sama lain dan dengan jadwal
pekerja. Detik yang persis sama antara pekerja dan pemanggilan langsung tidak dapat dipaksakan dari
luar. Penjaganya sama untuk semua pemroses (token versi), jadi perilakunya berlaku sama.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan dan sesi database kedua.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Kebijakan `INVOICE_SYNC` | Kembali aktif 5/60/3600 |
| Tindakan | 5 tindakan uji `TEST-RJE010` (R1, R2, R3a, R3b, R4 versi pertama); R2/R3a/R3b di antrean rekonsiliasi |
| Lab | Wadah dan pemeriksaan sintetis `TEST-RJE010-*` pada order `52ac1a34…`, item `ACCEPTED` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Koneksi invoice diputus lalu pulih → item muncul sekali (`UAT-13`) | Terpenuhi | R1 |
| 2. Gagal ke-5 → `ReconciliationRequired` `RETRY_EXHAUSTED` | Terpenuhi | R2 |
| 3. Kebijakan nonaktif → langsung antrean (`UAT-14`) | Terpenuhi | R3a, R3b |
| 4. Pekerja dan pemanggilan langsung bersamaan → satu item | Terpenuhi (batas bukti di 5.1) | R4 |
| Verifikasi: waktu jadwal dicatat | Terpenuhi | R1, R2 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar log aplikasi yang sudah ada |
| Risiko tersisa | Satu percobaan yang tertahan lock memakan satu slot siklus hingga 30 detik; 50 baris macet bersamaan akan memperlambat siklus, bukan menggandakan tagihan. Baris `Pending` dengan sebab "belum didukung" akan dicoba ulang setiap `BaseDelaySeconds`. Pekerja kirim ulang **fakta klinis** (`BE-RJE-011`) masih terpisah dan belum dikerjakan |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-RJE-008`, `009`, `010` belum di-stage atau di-commit; snapshot per task di scratchpad |
| Langkah berikutnya | `BE-RJE-014` (endpoint Ringkasan Billing kunjungan) |
