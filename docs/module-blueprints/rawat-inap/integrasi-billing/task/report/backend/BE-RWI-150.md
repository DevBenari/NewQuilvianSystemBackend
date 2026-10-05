# Laporan Perubahan Backend — `BE-RWI-150`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-150` |
| Judul | Penerima ketukan pintu Billing dan invoice `RANAP` otomatis |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-150` |
| Trace | `FR-RWF-010`, `014`, `016`; `RWI-DEC-166`, `192`; `INT-RWF-01`; `INV-RWF-06`; `UAT-RWF-11`; API 3.10; integrasi 4.2; data 6.7 |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-147` ✅; `BE-RWI-149` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, diperiksa 1, diubah 2, logika 2, kontrak API 1, database 1, keamanan 0, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Billing/**` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Prefix registry | `Bil` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (penerima, DTO); `TOUCHED LEGACY` (`BillingInvoiceService`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-LOG-001` |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Pesan outbox Rawat Inap tidak punya penerima di Billing: invoice rawat inap dibuka manual dan
tidak ada tanda terima yang membuktikan pesan benar-benar diproses.

## 2. Proses bisnis

1. Penerima **bukan endpoint HTTP**; dipanggil worker outbox dalam proses yang sama (API 3.10).
2. Satu pesan = satu transaksi Billing: kunci advisori per kunci idempotensi, cek tanda terima,
   buka invoice `RANAP` bila belum ada (`OpenInpatientInvoiceAsync`), hitung ulang tarif kamar untuk
   `BED_OCCUPIED`/`OCCUPANCY_CORRECTED`/`BED_RELEASED` bila invoice `OPEN`, lalu simpan
   `BilInpatientEventReceipt`.
3. Contoh: Budi diadmisi → `ADMISSION_CONFIRMED` → invoice `RANAP` terbuka, tanda terima
   `INVOICE_OPENED`, `Accepted = true`. Pesan yang sama datang lagi → `DUPLICATE`, invoice tetap satu.
4. Event tempat tidur yang tiba sebelum `ADMISSION_CONFIRMED` tetap membuka invoice lebih dulu.
5. Invoice sudah final → tidak dihitung ulang (perubahan memakai adjustment Billing).
6. Kunjungan tidak dikenal → `Accepted = false`, `REJECTED_UNKNOWN_ENCOUNTER`, tanpa tanda terima.
7. Galat teknis → transaksi dibatalkan, tidak ada tanda terima, galat diteruskan ke worker untuk
   dicoba ulang. Admisi di Rawat Inap tidak terpengaruh karena pesan hanya diantrekan di outbox
   pada transaksi admisi (`UAT-RWF-11`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingInvoiceService.cs` (pembukaan invoice, `MapServiceType`), `BillingCalculationService.cs`
(`RecalculateAsync`), `InpatientIntegrationOutboxWorker.cs`, kontrak integrasi 4.2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Services/BillingInpatientEventReceiver.cs` | Baru — `ReceiveAsync` |
| `Billing/Dtos/BillingInpatientEventDtos.cs` | Baru — `InpatientBillingEventEnvelope` (daftar putih), `InpatientEventReceipt`, kosakata event dan `SourceType` |
| `Billing/Models/BilInpatientEventReceipt.cs` | `BillingInpatientEventOutcomes` (model dari `BE-RWI-149`) |
| `Billing/Services/BillingInvoiceService.cs` | `OpenInpatientInvoiceAsync` — idempoten, label `RANAP` |
| `Billing/BillingManagementServiceCollectionExtensions.cs` | Registrasi `BillingInpatientEventReceiver` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint publik (sesuai API 3.10) |
| Database | Menulis `BilInpatientEventReceipt` (tabel dari `I2`) |
| Keamanan/Auth | Tidak ada pintu publik pengubah data |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — penerima di dalam aplikasi.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review alur transaksi dan idempotensi | Satu transaksi; tanda terima unik; rollback saat galat | `PASS` | Review source |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Proses bisnis empat jenis event | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pesan pertama → invoice `RANAP`, `INVOICE_OPENED`, `Accepted = true` | Terpenuhi (source) | `ReceiveAsync` |
| 2. Pesan sama dikirim ulang → `DUPLICATE`, invoice tetap satu | Terpenuhi (source) | Cek tanda terima di bawah kunci advisori; unique index |
| 3. Galat Billing → tanpa tanda terima, transaksi batal | Terpenuhi (source) | Blok `catch` rollback |
| 4. `UAT-RWF-11` admisi tetap tersimpan | Terpenuhi (source) | Pengiriman asinkron lewat outbox |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pesan ditolak (kunjungan tidak dikenal) tidak meninggalkan tanda terima; worker mencatatnya gagal sampai `DeadLetter` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Bergantung migration `I2` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??` |
| Langkah berikutnya | Uji event dengan worker sesudah build dan migration |
