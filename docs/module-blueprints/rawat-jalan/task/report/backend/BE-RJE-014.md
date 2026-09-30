# Laporan Perubahan Backend — `BE-RJE-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-014` |
| Judul | Endpoint Ringkasan Billing kunjungan |
| Slice | `MVP-4` — `EPIC RJE-07` Ringkasan Billing untuk dokter |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-014` |
| Trace | `FR-RJE-060`; `AC-RJ-009`, `010`, `015`; `RJ-E2E-DEC-008`, `025`; `contracts/api-contract.md` V2; `contracts/permission-audit-matrix.md` V2-2/V2-3 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-003` ✅ |
| Klasifikasi | `MEDIUM` — endpoint baca baru dengan butir hak akses baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `eeb18c57` (`sukmagp`) + perubahan `BE-RJE-008`/`009`/`010` yang belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — kelima acceptance criteria terbukti; satu delta kontrak tercatat (bagian 1) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Registry | `Bil` `ACTIVE` |
| Keberlakuan | `NEW CODE` — controller, service, DTO baru |
| Arketipe endpoint | Transaksi — **monitoring read-only**: satu `GET`, tanpa endpoint tulis, tanpa `options`/`status`/`DELETE` |
| QBE yang berlaku | `QBE-SVC-001` (controller tidak menyentuh `ApplicationDbContext`), `QBE-VAL-001`, `QBE-SEC` hak akses lewat `[AccessController]`/`[AccessAction]`/`[AccessPermission]` |
| Hak akses | `[AccessPermission("EncounterBillingSummary", "Read")]` = `ControllerName` dan nama `[AccessAction]` yang sama; tanpa hardcode peran |
| Tidak berlaku | Model, configuration, migration — tidak disentuh |
| Wewenang | `RJ-E2E-DEC-025` |

---

## 1. Masalah yang diperbaiki

Dokter perlu tahu apakah pelayanan yang ia berikan sudah masuk tagihan, berapa totalnya, dan
berapa bagian pasien. Sebelum task ini, jawabannya hanya ada di API invoice kasir, yang sekaligus
membuka tagihan **semua** pasien dan harga per item.

**Contoh:** dr. B membuka kunjungan pasiennya dan melihat:

- tagihan `OPEN` nomor `BIL-20260928-00000009`;
- 2 pelayanan: *USG Regio Cruris* (Dikerjakan) dan *D6 IGA* (Diterima Lab);
- total Rp2.383.000, seluruhnya tanggungan pasien, bayar tunai.

dr. B tidak melihat harga per item. Bila ia mencoba `GET /billing/invoices/{id}`, jawabannya `403`.

**Delta kontrak.** Kontrak V2 menulis total berasal dari `PreviewCalculationAsync`. Ternyata
pratinjau itu hanya berlaku untuk invoice `OPEN` ("Hanya invoice OPEN yang dapat dihitung ulang");
invoice `FINAL`/`CLOSED` dijawab `422`. Karena dokter justru sering membuka kunjungan yang sudah
lunas, angka invoice non-`OPEN` dibaca dari **versi kalkulasi terkini yang dikunci saat finalisasi**
— angka yang sama dengan yang dilihat kasir. Invoice `OPEN` tetap memakai pratinjau. Ini keputusan
teknis yang menjaga maksud kontrak ("angka sama dengan Billing"), bukan kebijakan baru.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Dokter membaca status dan total tagihan satu kunjungan tanpa harga per item dan tanpa akses ke invoice lain |
| Pelaku | Dokter/perawat poli yang diberi butir `EncounterBillingSummary : Read` oleh admin di layar Akses Role |
| Isi | Status (`NO_INVOICE`/`OPEN`/`FINAL`/`CLOSED`/`SETTLED_BY_WRITE_OFF`), nomor invoice, jumlah item aktif, total kotor, bagian penjamin (`PrimaryAmount`), bagian pasien, label sumber bayar, waktu perubahan terakhir, jumlah yang menunggu kirim ulang, jumlah yang menunggu rekonsiliasi, dan daftar pelayanan |
| Daftar pelayanan | Item invoice aktif (`RECORDED`, label status mis. *Dikerjakan*, *Diterima Lab*, *Diresepkan*, *Diserahkan*); revisi terakhir tiap fakta folio yang belum masuk invoice (`PENDING` bila `Pending`/`Failed`, `RECONCILIATION` bila antrean, `NOT_BILLED` bila gratis atau pengulangan kesalahan rumah sakit); fakta yang tidak pernah sampai ke folio (`RECONCILIATION`) |
| Sumber bayar | Penjamin utama kunjungan (pola ringkasan tagihan rawat inap): *Tunai*, *Asuransi — Nama*, *Penjamin Perusahaan — Nama* |
| Jalur tidak normal | Belum ada invoice → `200` `NO_INVOICE` (keadaan normal di awal kunjungan). Kunjungan tidak ada → `404`. Kalkulasi menolak → `422`. Tanpa butir → `403`. Tanpa sesi → `401` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientBillingSummaryController.cs` dan `PatientBillingSummaryService.cs` (pola), `BillingCalculationService.PreviewCalculationAsync`,
`BilCalculationVersion.cs`, `BillingInvoicesController.PreviewCalculation`, `AccessPermissionService.cs`,
`RoleAccessController.cs`, `ExternalUserController.cs` (akun uji), `AuthController` (geofence login).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/EncounterBillingSummaryController.cs` | **Baru.** `GET /{encounterId}`; `[AccessController]` modul `HEALTH_SERVICE_BILLING_MANAGEMENT`, `ControllerName = "EncounterBillingSummary"`; `[AccessAction("Read", …)]` + `[AccessPermission("EncounterBillingSummary", "Read")]` |
| `Areas/HealthServices/BillingManagement/Billing/Services/EncounterBillingSummaryService.cs` | **Baru.** Baca saja (`AsNoTracking`); angka dari pratinjau (`OPEN`) atau versi kalkulasi terkunci |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/EncounterBillingSummaryDtos.cs` | **Baru.** `EncounterBillingSummaryResponse`, `EncounterBillingSummaryServiceResponse` (tanpa harga), konstanta status |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Mendaftarkan `EncounterBillingSummaryService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru sesuai kontrak V2, dengan delta sumber angka untuk invoice non-`OPEN` (bagian 1) |
| Database | Tanpa skema. Butir akses `EncounterBillingSummary`/`Read` terdaftar otomatis di `SysControllerAccess`/`SysActionAccess` saat aplikasi menyala |
| Keamanan/Auth | Butir berdiri sendiri, tidak menumpang `BillingInvoice : Read`. Respons tanpa `UnitPrice`/`TotalPrice`. Tidak ada pemeriksaan "dokter penanggung jawab" di luar hak akses; kontrak V2 tidak memintanya |

---

## 4. Dokumentasi endpoint

### Health Services / Billing Management / Encounter Billing Summary

Base URL: `api/v1/health-services/billing-management/encounter-billing-summaries`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{encounterId}` | Ringkasan tagihan satu kunjungan tanpa harga per item | `EncounterBillingSummary : Read` | path `encounterId` | `ApiResponse<EncounterBillingSummaryResponse>` | **Tersedia** |

Kode status: `200` (termasuk `NO_INVOICE`), `401`, `403`, `404`, `422`.

**Contoh respons (dipotong):**

```json
{
  "billingStatus": "OPEN",
  "invoiceNumber": "BIL-20260928-00000009",
  "serviceCount": 2,
  "grossAmount": 2383000.0,
  "payerAmount": 0,
  "patientAmount": 2383000.0,
  "paymentSourceLabel": "Tunai",
  "pendingSyncCount": 0,
  "reconciliationCount": 0,
  "services": [
    { "description": "USG REGIO CRURIS", "quantity": 1.0, "serviceStatusLabel": "Dikerjakan", "billingState": "RECORDED" }
  ]
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … --no-incremental -o <scratchpad>/out-rje014b` | `0 Error(s)`, `230 Warning(s)`, 1 menit 45 detik; nol warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Findings: none`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R6 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi pada `http://localhost:5219` terhadap **`QuilvianNewDevSukma`**.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login superadmin; tanpa cookie | `200`; `401` | — | `PASS` |
| R1 | Kunjungan Rawat Jalan tanpa invoice | `200` "Kunjungan belum memiliki tagihan."; `NO_INVOICE`, angka 0, `Tunai`, daftar kosong | 1 | `PASS` |
| R2 | Kunjungan ber-invoice `OPEN` dibandingkan dengan `GET /billing/invoices/{id}/calculation-preview` | Ringkasan: gross Rp2.383.000, penjamin 0, pasien Rp2.383.000. Pratinjau kasir: angka yang sama persis | 2 | `PASS` |
| R3 | Seluruh kunci JSON respons diperiksa rekursif | Tidak ada `unitPrice` maupun `totalPrice`; kunci layanan hanya `description`, `quantity`, `serviceStatusLabel`, `billingState` | 3 | `PASS` |
| R4 | Pengguna uji dengan satu butir `EncounterBillingSummary : Read` (kebijakan akses departemen+jabatan lewat `POST /role-access/policies`, akun lewat `POST /external-users`, login dengan koordinat geofence rumah sakit) | Ringkasan `200` (Rp2.383.000); `GET /billing/invoices/{id}` **`403`** "Anda tidak memiliki akses ke menu atau fitur ini."; `GET /billing/invoices` `403`; `calculation-preview` `403` | 4 | `PASS` |
| R5 | Kunjungan acak; tanpa sesi | `404` "Kunjungan tidak ditemukan."; `401` | 5 | `PASS` |
| R6 | Kunjungan resep A (invoice `CLOSED`) | `200`; `CLOSED`, gross Rp826.336 dari versi kalkulasi terkunci; layanan "Resep … Diserahkan" `RECORDED` dan "Jasa konsultasi — Perlu ditinjau Billing" `RECONCILIATION` (tarif konsultasi tidak ada, dari `BE-RJE-008`) | — | `PASS` |

**Percobaan yang tidak dipakai sebagai bukti.**
- Run pertama memilih kunjungan tanpa invoice untuk R2.
- Kunjungan `CLOSED` dijawab `422`; dari situ delta kontrak di bagian 1 ditemukan dan diperbaiki
  sebelum build akhir.
- Kebijakan akses pertama ditolak "Position tidak valid atau tidak sesuai department", jadi
  dipilih jabatan yang memang milik departemennya.
- Login pengguna uji pertama ditolak `403` oleh geofence login, karena permintaan tanpa koordinat.
  Akun itu tetap tercipta dan kemudian dinonaktifkan.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Akun uji | `test-rje014-e7c057d3@example.test`, `test-rje014-7fdcb509@example.test` — **dinonaktifkan** (`IsActive = false`); data external user `TEST-RJE014 Dokter Uji` tersisa |
| Kebijakan akses | Pasangan uji (Coder Casemix) dikosongkan kembali lewat API; nol kebijakan tersisa |
| Butir akses | `EncounterBillingSummary`/`Read` terdaftar dan siap dicentang admin |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kunjungan tanpa invoice → `200 NO_INVOICE` (`UAT-17`) | Terpenuhi | R1 |
| 2. Angka sama dengan `calculation-preview` | Terpenuhi (invoice `OPEN`; non-`OPEN` memakai kalkulasi terkunci — delta bagian 1) | R2, R6 |
| 3. Respons tanpa `UnitPrice`/`TotalPrice` | Terpenuhi | R3 |
| 4. Pengguna dengan butir ringkasan saja → `403` pada `GET /billing/invoices/{id}` (`UAT-18`) | Terpenuhi | R4 |
| 5. Tanpa sesi → `401` | Terpenuhi | R5 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar log aplikasi yang sudah ada |
| Risiko tersisa | Endpoint tidak membatasi dokter pada kunjungan yang ia tangani sendiri; siapa pun yang memegang butir ini dapat membaca ringkasan kunjungan mana pun **tanpa** harga per item. Kontrak V2 tidak meminta pembatasan itu. Bila diperlukan, itu keputusan baru. Delta sumber angka invoice non-`OPEN` perlu dicatat ke kontrak API pada revisi berikutnya |
| Perubahan sampingan | `NONE` di repository. Data uji: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-RJE-008`, `009`, `010`, `014` belum di-stage atau di-commit; snapshot per task di scratchpad |
| Langkah berikutnya | `BE-RJE-011` (pekerja kirim ulang fakta klinis), lalu `BE-RJE-012`, `BE-RJE-013`; frontend `FE-RJE-001` dapat mulai memakai endpoint ini |
