# Laporan Perubahan Backend — `BE-RJE-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-012` |
| Judul | API antrean rekonsiliasi dan kebijakan |
| Slice | `MVP-3` — `EPIC RJE-06` Keandalan |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-012` |
| Trace | `FR-RJE-052`, `FR-RJE-054`; `RJ-E2E-DEC-009`, `016`, `026`; `contracts/api-contract.md` V2; `contracts/validation-matrix.md` `RJE-VAL-020`–`025`, `030`, `031`; `contracts/permission-audit-matrix.md` V2-2 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-010` ✅, `BE-RJE-011` ✅ |
| Klasifikasi | `MEDIUM` — lima endpoint baru, empat butir hak akses baru, aksi tulis dengan penjaga konkurensi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2af16d93` (`sukmagp`) + perubahan `BE-RJE-011`/`013` yang belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI** — ketujuh acceptance criteria terbukti; satu delta kontrak tercatat |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` (antrean) dan `MasterData` (kebijakan) |
| Registry | `Bil` `ACTIVE` |
| Keberlakuan | `NEW CODE` — dua controller, dua service, dua berkas DTO |
| Arketipe endpoint | Antrean = transaksi **worklist** (`GET /`, `POST /{itemType}/{id}/retry`, `POST /{itemType}/{id}/resolve`). Kebijakan = master data seed tetap dengan **dua** endpoint sesuai kontrak (`GET /`, `PUT /{id}`); baseline master data lain sengaja tidak dibuat karena tidak ada tambah/hapus kebijakan |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-VAL-001`, `QBE-TXN-001` (kunci baris `NOWAIT` + token konkurensi; transaksi di-commit sebelum kirim ulang), `QBE-AUD-001` |
| Hak akses | `BillingChargeReconciliation : Read/Update`, `BillingSyncPolicy : Read/Update`; nama `[AccessPermission]` = `ControllerName` + nama `[AccessAction]`; tanpa hardcode peran |
| Tidak berlaku | Model, configuration, migration |
| Wewenang | `RJ-E2E-DEC-026` |

---

## 1. Masalah yang diperbaiki

Pekerja `BE-RJE-010`/`011` berhenti mencoba dan memindahkan item ke "perlu rekonsiliasi", tetapi
tidak ada cara bagi petugas untuk melihat, mengirim ulang, atau menyelesaikan item itu. Kebijakan
kirim ulang juga hanya bisa diubah lewat database.

**Contoh:** resep Tn. C tertahan `TARIFF_NOT_FOUND` karena tarif Acetin sempat berakhir. Setelah
admin tarif memperbaikinya, petugas Billing membuka antrean, menekan *Kirim ulang*, dan item resep
Rp413.168 langsung masuk invoice. Item yang memang ditagih manual oleh kasir cukup diselesaikan
dengan jenis *BILLED_MANUALLY* dan alasannya; item itu keluar dari antrean dan tercatat siapa yang
memutuskannya.

**Delta kontrak.** Kontrak menamai jenis item folio `CHARGE_LINE`. Sejak `RJ-E2E-DEC-016`, kolom
sinkron berada di `BilProcessingEffect`, bukan `BilChargeLine`. Nama path `CHARGE_LINE`
dipertahankan supaya kontrak dan frontend tidak berubah, tetapi `Id`-nya adalah **id efek folio**.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Petugas Billing menuntaskan item yang tidak dapat diselesaikan otomatis; admin mengatur kebijakan tanpa rilis |
| Pelaku | Petugas Billing (`BillingChargeReconciliation`); Supervisor/Admin Billing (`BillingSyncPolicy`) |
| Isi antrean | **`CLINICAL_FACT`**: fakta `OutcomeUnknown` yang masih dicoba (`FAILED`), fakta dengan `ReconciliationRequiredAt` (`RECONCILIATION_REQUIRED`), yang sudah diselesaikan (`RESOLVED`). **`CHARGE_LINE`**: efek folio `Failed`, `ReconciliationRequired`, `Resolved`. Tanpa saringan status: yang belum selesai saja |
| Saringan | `Status`, `SourceDomain`, `EncounterId`, `ErrorCode`, `DateFrom`/`DateTo`, `Search` (nomor invoice atau kunjungan), `Page`/`PageSize` (maks. 100) |
| Kirim ulang | Fakta → `ClinicalMilestoneFactProducer.RedispatchAsync` (kunci sama, tanpa revisi baru). Efek folio → dikembalikan `Pending` lalu `BillingClinicalChargeBridgeService.TrySyncEffectAsync`. Tidak ada jalur kedua |
| Selesaikan | `Resolution` (`BILLED_MANUALLY`, `NOT_BILLABLE`, `DUPLICATE`) + `Note` 10–500 karakter; dicatat waktu, penyelesai, dan catatan. Item keluar dari antrean dan tidak disentuh pekerja lagi |
| Penjaga | Baris item dikunci `FOR UPDATE NOWAIT` lalu disimpan dengan token konkurensi (`InvoiceSyncVersion`/`Version`). Item yang sedang dipegang pemroses lain langsung ditolak `409 RJE-VAL-025` tanpa menunggu |
| Kebijakan | `PUT` mengubah `MaxAttemptCount` (0–20), `BaseDelaySeconds` (10–3600), `MaxDelaySeconds` (≥ Base, ≤ 86400), `IsActive`; dijaga `RowVersion`. Menonaktifkan kebijakan menghentikan kirim ulang otomatis (fail-closed) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak API/validasi/permission V2, `AdministrationFeePoliciesController` (pola master data Billing),
`PatientBillingSummaryController`, `MstBillingSyncPolicy` beserta konfigurasi (token `RowVersion`,
check constraint), `CliClinicalMilestoneFactConfiguration` (token `Version`),
`BillingOperationalConfigurations` (token `InvoiceSyncVersion`), `ApplicationUser.DisplayName`,
`PagedResult`, pola kode validasi `BE-RJE-004`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingChargeReconciliationController.cs` | **Baru.** `GET /`, `POST /{itemType}/{id}/retry`, `POST /{itemType}/{id}/resolve` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeReconciliationService.cs` | **Baru.** Daftar gabungan fakta + efek; kirim ulang; penyelesaian manual; kunci `NOWAIT`; audit |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/ChargeReconciliationDtos.cs` | **Baru.** Query, item, permintaan penyelesaian, konstanta |
| `Areas/HealthServices/BillingManagement/MasterData/Controllers/BillingSyncPolicyController.cs` | **Baru.** `GET /`, `PUT /{id}` |
| `Areas/HealthServices/BillingManagement/MasterData/Services/BillingSyncPolicyService.cs` | **Baru.** Validasi batas, `RowVersion`, audit sebelum/sesudah |
| `Areas/HealthServices/BillingManagement/MasterData/DTOs/BillingSyncPolicyDtos.cs` | **Baru** |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Mendaftarkan kedua service |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint sesuai kontrak V2, dengan delta arti id `CHARGE_LINE` (bagian 1). Kode penolakan dikirim di `errors.code`, sama dengan `BE-RJE-004`. Status saringan yang tidak dikenal dijawab `422` tanpa kode, karena matriks validasi tidak menetapkan kode untuknya |
| Database | Tanpa skema. Penyelesaian mengisi kolom `Reconciliation*` yang sudah ada sejak `BE-RJE-001` |
| Keamanan/Auth | Empat butir akses baru, terdaftar otomatis saat aplikasi menyala. Respons tanpa nama pasien (hanya nomor kunjungan dan invoice) |

---

## 4. Dokumentasi endpoint

### Health Services / Billing Management / Billing / Charge Reconciliations

Base URL: `api/v1/health-services/billing-management/billing/charge-reconciliations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar antrean | `BillingChargeReconciliation : Read` | query `ChargeReconciliationQuery` | `PagedResult<ChargeReconciliationItemResponse>` | **Tersedia** |
| `POST` | `/{itemType}/{id}/retry` | Kirim ulang dengan identitas sama | `BillingChargeReconciliation : Update` | path `itemType` (`CLINICAL_FACT`/`CHARGE_LINE`), `id` | `ChargeReconciliationItemResponse` | **Tersedia** |
| `POST` | `/{itemType}/{id}/resolve` | Selesaikan manual | `BillingChargeReconciliation : Update` | `ResolveChargeReconciliationRequest` | `ChargeReconciliationItemResponse` | **Tersedia** |

Kode: `200`; `403`; `404`; `409` `RJE-VAL-021`/`025`; `422` `RJE-VAL-020`/`022`/`023`/`024`.

### Health Services / Billing Management / Master Data / Billing Sync Policy

Base URL: `api/v1/health-services/billing-management/master-data/billing-sync-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Membaca kedua kebijakan | `BillingSyncPolicy : Read` | — | `List<BillingSyncPolicyResponse>` | **Tersedia** |
| `PUT` | `/{id}` | Mengubah batas dan jeda | `BillingSyncPolicy : Update` | `UpdateBillingSyncPolicyRequest` | `BillingSyncPolicyResponse` | **Tersedia** |

Kode: `200`; `403`; `404`; `409` `RJE-VAL-031`; `422` `RJE-VAL-030`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje012b` | `0 Error(s)`, `230 Warning(s)` (= baseline), 1 menit 40 detik; nol warning dari berkas task | `PASS` | Build pertama 231: satu warning nullability di service baru, diperbaiki sebelum build akhir |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Findings: none`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R8 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 30 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Daftar tanpa saringan; saringan `RECONCILIATION_REQUIRED` + `PHARMACY` + `TARIFF_NOT_FOUND`; halaman 2 ukuran 2; status salah | `200`, 21 item (4 `CLINICAL_FACT`, 17 `CHARGE_LINE`) dengan nomor kunjungan, domain, sebab, percobaan, nomor invoice; saringan → 1 item resep; halaman 2/11 berisi 2 item; status salah → `422` | 1 | `PASS` |
| R2 | Efek resep C (`TARIFF_NOT_FOUND`); tarif Acetin sudah aktif kembali; kirim ulang | `200`; status **`SYNCED`**; invoice baru `BIL-20260930-00000001`; item `PRESCRIBED` Rp413.168 (= 2 × 64.504 + 2 × 142.080) | 2 | `PASS` |
| R3 | Selesaikan tanpa alasan, alasan 6 karakter, jenis salah, jenis item salah; lalu selesaikan sah; ulangi | `422 RJE-VAL-022` (dua kali), `422 RJE-VAL-023`, `422 RJE-VAL-024`; sah → `RESOLVED` oleh *SuperAdmin*, catatan "BILLED_MANUALLY: …"; ulang selesaikan dan kirim ulang → `409 RJE-VAL-021` | 3 | `PASS` |
| R4 | Item R2 (sudah `Synced`) dikirim ulang | `422 RJE-VAL-020` "Item ini sudah tercatat di tagihan …" | 4 | `PASS` |
| R5 | Sesi kedua memegang baris efek (`FOR UPDATE`, meniru pemroses lain); kirim ulang dan selesaikan | Keduanya langsung `409 RJE-VAL-025`; status item tidak berubah | 5 | `PASS` |
| R6 | `PUT` kebijakan `INVOICE_SYNC`: `MaxAttemptCount = 25`; `MaxDelay < BaseDelay`; `RowVersion` acak; perubahan sah; ulangi dengan versi lama | `422 RJE-VAL-030` (dua kali); `409 RJE-VAL-031`; sah `200` dengan `RowVersion` baru; ulang dengan versi lama `409 RJE-VAL-031` | 6 | `PASS` |
| R7 | Pengguna uji tanpa butir akses (pasangan departemen+jabatan tanpa kebijakan; login dengan koordinat geofence) memanggil kelima endpoint | Kelimanya **`403`**; akun lalu dinonaktifkan | 7 | `PASS` |
| R8 | Fakta `RETRY_EXHAUSTED` dari `BE-RJE-011` dikirim ulang manual | `200`; status `DISPATCHED`, percobaan 6; tetap **1** revisi fakta | — | `PASS` |

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan dan sesi database kedua.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Antrean | Satu efek `RESOLVED` (`b09b5f7b…`, BILLED_MANUALLY); resep C `SYNCED`; satu fakta tindakan `BE-RJE-011` `Dispatched` |
| Kebijakan | `INVOICE_SYNC` nilai tetap 5/60/3600 aktif, `RowVersion` baru; `FACT_DISPATCH` tidak berubah |
| Akun uji | `test-rje012-1ef6231a@example.test` **dinonaktifkan**; pasangan akses uji tetap tanpa kebijakan |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar memuat fakta dan baris folio, dengan saringan dan halaman | Terpenuhi | R1 |
| 2. Kirim ulang `TARIFF_NOT_FOUND` setelah tarif diisi → `Synced` (`UAT-15`) | Terpenuhi | R2 |
| 3. Selesaikan tanpa alasan → `422 RJE-VAL-022` (`UAT-16`) | Terpenuhi | R3 |
| 4. Item `Synced` dikirim ulang → `422 RJE-VAL-020` | Terpenuhi | R4 |
| 5. Balapan dengan pekerja → `409 RJE-VAL-025` | Terpenuhi (pemroses lain disimulasikan dengan kunci baris) | R5 |
| 6. `MaxAttemptCount = 25` → `422 RJE-VAL-030`; `RowVersion` basi → `409 RJE-VAL-031` | Terpenuhi | R6 |
| 7. Tanpa butir → `403` | Terpenuhi | R7 |
| DoD: laporan | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Risiko tersisa | Daftar menggabungkan dan memotong halaman di memori; cukup untuk ukuran antrean, tetapi perlu diubah bila antrean tumbuh ribuan baris. Fakta `Rejected` (penolakan kontrak oleh Billing) tidak ditampilkan karena kontrak membatasi antrean pada fakta yang "belum pasti sampai". Delta arti id `CHARGE_LINE` perlu dicatat ke kontrak pada revisi berikutnya |
| Perubahan sampingan | `NONE`. Data uji: bagian 5.2 |
| Status Git | Belum di-stage atau di-commit |
| Langkah berikutnya | Seluruh task backend V2 selesai. Berikutnya frontend `FE-RJE-001`..`003`, lalu `verify-module-readiness` |
