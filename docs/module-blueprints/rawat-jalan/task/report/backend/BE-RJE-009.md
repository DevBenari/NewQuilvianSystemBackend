# Laporan Perubahan Backend — `BE-RJE-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-009` |
| Judul | Pembatalan dan koreksi |
| Slice | `MVP-3` — `EPIC RJE-05` Pembatalan dan koreksi |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-009` |
| Trace | `FR-RJE-040`, `041`, `042`; `RJ-E2E-DEC-010`, `025`; `02-backend-architecture.md` V2.7.5; flowchart `pembatalan-dan-koreksi.md` |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-005` ✅ |
| Klasifikasi | `MEDIUM` — cabang baru di jembatan; memakai `VoidItemAsync` dan `CreateAdjustmentAsync` yang sudah ada |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `eeb18c57` (`sukmagp`) + perubahan `BE-RJE-008` yang belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — keempat acceptance criteria terbukti, ditambah dua uji pengaman |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Registry | `Bil` `ACTIVE` |
| Keberlakuan | Perluasan service `NEW CODE` (`BE-RJE-003`); `TOUCHED LEGACY` pada `BillingChargeSourceAdapter` (satu method baca) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-AUD-001` |
| Tidak berlaku | Model, configuration, migration, endpoint, permission — tidak disentuh. Producer pembatalan klinis sudah ada dan **tidak** diubah |
| Wewenang | `RJ-E2E-DEC-025` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, fakta pembatalan klinis sampai ke folio tetapi berhenti di jembatan dengan status
`Pending` / `CANCELLATION_PENDING_SUPPORT`. Akibatnya, tindakan yang dibatalkan dokter tetap
tertagih.

**Contoh:** tindakan *Nebulizer* sudah dikerjakan dan tertagih Rp1.132.000, lalu ternyata salah
pasien. Dokter membatalkannya.

- Item invoice **tidak** dihapus dan **tidak** di-void, karena pelayanannya sudah terjadi.
- Billing menerima adjustment `CREDIT` Rp1.132.000 yang menunggu persetujuan.
- Bila yang dibatalkan adalah resep yang belum diproses farmasi, itemnya langsung di-void.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Pembatalan klinis membatalkan tagihannya tanpa menghapus riwayat, dan tanpa pembatalan palsu atas tagihan yang tidak pernah ada |
| Pelaku | Dokter/penginput (membatalkan pelayanan); Billing (menyetujui adjustment) |
| Pemicu | Fakta `ClinicalCancellation` dari producer yang sudah ada: tindakan (`PATCH /patient-procedures/{id}/cancel`), resep (`PATCH /prescriptions/{id}/cancel`), Lab (`PUT /lab-orders/{id}/cancel`, hanya untuk spesimen yang sudah diterima) |
| Keputusan jembatan | 1. Tagihan versi sebelumnya masih `Pending`/`Failed` → **tunggu** (`Failed`, dicoba ulang pekerja), supaya pembatalan tidak mendahului tagihannya. 2. Tidak ada item aktif → selesai, `NO_FINANCIAL_CHANGE`. 3. Invoice `OPEN` dan item masih bisa di-void normal (`CONFIRMED`, `ACCEPTED`, `PRESCRIBED` — aturan yang sama dengan `ValidateVoid`) → **void**. 4. Selain itu → adjustment **`CREDIT`** sebesar nilai efektif (total item + adjustment versi sebelumnya), termasuk pada invoice `OPEN` untuk pelayanan yang sudah dikerjakan (`RJ-E2E-DEC-010`) |
| Pengaman urutan terbalik | Tagihan versi lama yang baru diproses setelah pembatalannya tercatat `NO_FINANCIAL_CHANGE`. Tanpa pengaman ini, upsert akan membuat item baru, karena item yang sudah di-void tidak lagi dianggap ada |
| Jalur tidak normal | Invoice `CLOSED` → Billing menolak adjustment → antrean `ADJUSTMENT_REJECTED`. Void ditolak (mis. perhitungan terkunci) → antrean `SOURCE_REJECTED`. Konflik `RowVersion` → dicoba ulang dengan kunci yang sama |
| Hasil akhir | Item `VOIDED` berstatus sumber `CANCELLED`, atau adjustment `CREDIT` `SUBMITTED`, atau antrean dengan sebab terbaca |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingInvoiceService.VoidItemAsync`, `BillingChargeSourceAdapter.ValidateVoid`,
`BillingFinancialExceptionService.CreateAdjustmentAsync`, `ClinicalMilestoneFactProducer` (CASE A:
pembatalan tanpa tagihan sebelumnya), `PatientProcedureController.CancelProcedure`,
`PrescriptionController.CancelPrescription` dan `PrescriptionWorkflowService.CancelAsync`,
`LabOrderService` (pembatalan) dan `LabSpecimenService.EmitClinicalCancellationAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | Cabang `BuildCancellationPlanAsync` dan `VoidAsync`; `BuildAdjustmentPlanAsync` menerima nilai target secara eksplisit (dipakai koreksi pasca-final dan pembatalan); pengaman "versi lama sesudah pembatalan"; hasil `ITEM_VOIDED`, `NO_FINANCIAL_CHANGE`, dan tunggu-versi-sebelumnya; pesan adjustment tidak lagi menyebut "sudah final" untuk invoice `OPEN` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | `IsNormallyVoidable(domain, status)` — method baca atas kebijakan yang sama dengan `ValidateVoid` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada perubahan |
| Database | Tanpa skema. Kode sebab baru pada `BilProcessingEffect.InvoiceSyncErrorCode`: `ITEM_VOIDED` |
| Keamanan/Auth | Tidak ada endpoint atau permission baru. Void dan adjustment tercatat atas nama aktor fakta, lewat audit service yang sama |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje009b` | `0 Error(s)`, `230 Warning(s)`, 1 menit 52 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Findings: none`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R6 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**, sesi `superadmin`.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Tindakan dibuat, disetujui, dikerjakan (item `PERFORMED` Rp1.132.000, invoice `OPEN`), lalu dibatalkan lewat API | `200` `billingHandoff = Emitted`; efek v2 `Synced` `ADJUSTMENT_SUBMITTED`; adjustment **`CREDIT` Rp1.132.000 `SUBMITTED`** ("pembatalan klinis atas pelayanan berstatus PERFORMED, nilai 1132000 menjadi 0"); item tetap `ACTIVE` | 2 | `PASS` |
| R2 | Resep Panadol × 2 difinalkan (item `PRESCRIBED` Rp129.008), lalu dibatalkan dokter sebelum diproses farmasi | `200`; efek v2 `ITEM_VOIDED`; item **`VOIDED`**, sumber `CANCELLED` versi 2, alasan "Pembatalan klinis PHARMACY … versi 2." | 4 | `PASS` |
| R3 | Order Lab nyata (dibuat dan dikonfirmasi lewat API); wadah dan pemeriksaan sintetis; fakta tagih v1 → item `ACCEPTED` Rp1.251.000; fakta batal v2 lewat folio | Efek v2 `ITEM_VOIDED`; item **`VOIDED`** sumber `CANCELLED`; replay v2 → "Milestone telah diproses sebelumnya", tanpa perubahan | 1 | `PASS` |
| R4 | Order Lab kedua dibuat lalu dibatalkan lewat API sebelum spesimen diterima | `200`; `billingHandoffs = []`; **nol** fakta, nol item, tanpa void maupun adjustment | 3 | `PASS` |
| R5 | Tindakan `PERFORMED` pada invoice `CLOSED` (`6810a4d2…`) dibatalkan lewat API | Efek v4 `ReconciliationRequired` `ADJUSTMENT_REJECTED` ("Invoice sudah closed …"); item tetap `ACTIVE` | — | `PASS` |
| R6 | Efek tagih v1 atas pemeriksaan R3 dikembalikan ke `Pending` (SQL), lalu fakta v1 dikirim ulang | Efek v1 `Synced` `NO_FINANCIAL_CHANGE` "Pelayanan ini sudah dibatalkan pada versi yang lebih baru"; item tetap `VOIDED` — tidak ada item baru | — | `PASS` |

**Percobaan yang tidak dipakai sebagai bukti.**
- `POST /lab-examinations/by-order/{id}` dijawab `404` "Wadah sampel tidak ditemukan", dan
  konfirmasi order tidak membuat wadah.
- Pembatalan order Lab pertama dijawab `422`, karena skrip memakai nama field `reason`; nama yang
  benar adalah `cancelReason`.
- Pesan R1 pada run itu masih "Tagihan sudah final …", lalu diperbaiki dan diverifikasi ulang
  lewat build `out-rje009b`. Perubahannya hanya teks pesan.

**Kondisi sintetis dan alasannya.**
- Pengambilan dan penerimaan sampel Lab terhalang pemisahan tugas bagi satu akun uji, seperti di
  `BE-RJE-003`. Karena itu wadah dan pemeriksaan R3 dibuat lewat SQL (bertanda `TEST-RJE009`),
  dan kedua faktanya diserahkan lewat endpoint folio internal.
- R6 mengubah status efek lewat SQL untuk mensimulasikan pengiriman yang terlambat.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Tindakan | `c9571702…` `Cancelled` (adjustment `CREDIT` `SUBMITTED`); `796110de…` (data uji `BE-RJE-003`) `Cancelled` dengan efek di antrean |
| Resep | `84164045…` `Cancelled`, item `VOIDED` |
| Lab | Order `52ac1a34…` dengan wadah/pemeriksaan sintetis `TEST-RJE009-*`, item `VOIDED`; order `316ea6cd…`, `fe4f2f90…`, `bc3c3aaf…`, `3f187b50…`, `6960936d…` dari percobaan |
| Konsultasi | 2 konsultasi baru dipakai (`6abd8dc3…`, `d8bd2c04…`) |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Lab `ACCEPTED` batal, invoice `OPEN` → item `VOIDED` (`UAT-10`) | Terpenuhi (penerimaan sampel disimulasikan) | R3 |
| 2. Tindakan `PERFORMED` batal → adjustment `CREDIT` `SUBMITTED`, item tetap `ACTIVE` (`UAT-11`) | Terpenuhi | R1 |
| 3. Lab batal sebelum diterima → tanpa void dan adjustment (`UAT-12`) | Terpenuhi | R4 |
| 4. Resep `PRESCRIBED` batal sebelum diproses → item `VOIDED` | Terpenuhi | R2 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar log aplikasi yang sudah ada |
| Risiko tersisa | Pembatalan yang menunggu versi sebelumnya berstatus `Failed` dan baru diproses ulang oleh pekerja `BE-RJE-010`. Dua efek pembatalan lama dari uji sebelumnya masih `Pending` `CANCELLATION_PENDING_SUPPORT` sampai pekerja itu ada. Radiologi belum punya producer pembatalan (di luar task) |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-RJE-008` dan `BE-RJE-009` belum di-stage atau di-commit. Snapshot per task tersimpan di scratchpad supaya bisa di-commit terpisah |
| Langkah berikutnya | `BE-RJE-010` (pekerja kirim ulang sinkron invoice) |
