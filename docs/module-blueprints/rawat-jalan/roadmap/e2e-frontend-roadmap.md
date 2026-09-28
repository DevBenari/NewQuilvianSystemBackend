# Roadmap Frontend — Rawat Jalan ke Invoice Canonical (V2)

## Metadata

```yaml
blueprint_id: RJ-BIL-BP-001
scope_name: "Rawat Jalan → Invoice Canonical (V2) — frontend"
scope_prefix: RJ-E2E
task_prefix: FE-RJE
module_slug: rawat-jalan
blueprint_revision: 27
roadmap_revision: 1
status: DRAFT
approval_gate: "Menunggu approval roadmap oleh pemilik"
design_approval: "RJ-E2E-DEC-015 — Sukma Giri, 2026-09-28"
contract_versions:
  - "RJ-E2E-CONTRACT-001@1.0.0 (approved)"
  - "RJ-E2E-CONTRACT-001@1.0.1 (draft) — hanya FE-RJE-002"
frontend_repository: "V2QuilvianSystemFrontendDev"
frontend_sha: "83b8b72744d4afaaedb3d2af9dd0b83fb272f9d6 (sukmagpV2)"
backend_sha: "063d38bc306bb6b46bdf088513fa6cdcc80399d8 (sukmagp)"
stack: "Next.js App Router, JavaScript/JSX, Redux, Axios, base component Quilvian"
ui_authority: "RJ-E2E-FE-001, RJ-E2E-FE-002 approved; RJ-E2E-FE-003, RJ-E2E-FE-004 DEV_DISCRETION"
implementation_authority: "NOT_GRANTED — diberikan per task"
builder_execution: "NOT_AUTHORIZED — seluruh task"
backend_roadmap: roadmap/e2e-backend-roadmap.md
traceability: roadmap/e2e-requirement-traceability.md
```

> Penomoran: task frontend memakai tiga digit (`FE-RJE-001`); layar memakai dua digit (`FE-RJE-01`,
> dari `03-frontend-architecture.md` bagian `V2.2`). Satu task mengerjakan satu layar dengan nomor
> urut yang sama.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD sudah terbukti |
| 🟡 | Source sudah ada tetapi kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan atau gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
BE-RJE-014 [BE] ─> FE-RJE-001

BE-RJE-012 [BE] ─> FE-RJE-003

BE-RJE-013 [BE] ─> FE-RJE-002
```

`[BE]` = task backend pada `e2e-backend-roadmap.md`, cermin baca-saja; tandanya disalin dari sana.

Jumlah pasangan prasyarat → task: **3**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RJE-014` | `FE-RJE-001` — menunggu `BE-RJE-014` selesai |
| 1 | `BE-RJE-012` | `FE-RJE-003` — menunggu `BE-RJE-012` selesai |
| — | ⛔ menunggu approval `RJ-E2E-CONTRACT-001@1.0.1` lewat `BE-RJE-013` | `FE-RJE-002` |

Ketiga task tidak saling menunggu dan boleh dikerjakan paralel begitu backend pasangannya selesai.
Kerangka layar boleh disiapkan lebih awal memakai kontrak `1.0.0` yang sudah terkunci, tetapi task
baru boleh dinyatakan selesai setelah divalidasi terhadap backend sungguhan.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RJE-001` | Dokter melihat tab Ringkasan Billing kunjungan yang sedang ditangani | `FR-RJE-061`; `RJ-E2E-DEC-008`; `RJ-E2E-FE-001`, `002`, `003` | `1.0.0` | `patient-billing-summary.service.js`, `nursing-billing-section.jsx`, base component, `usePermission` | Service, tab, konstanta tab, render di workspace | `BE-RJE-014` | Kartu | Pola Bank Darah FE + `UAT-17`, `18`, `24` | Kebocoran harga per item / Security | Kartu |
| `FE-RJE-002` | Dokter melihat masalah penyerahan tagihan setelah Selesai Konsultasi | `FR-RJE-062`; `RJ-E2E-FE-004` | `1.0.1` (draft) | Alur Selesai Konsultasi yang ada | Membaca `BillingHandoffIssues` | `BE-RJE-013` | Kartu | Pola Bank Darah FE + `UAT-19` | Konsultasi tampak gagal padahal sukses / Clinical | Kartu |
| `FE-RJE-003` | Petugas Billing menangani antrean rekonsiliasi dari menu | `FR-RJE-053`; `RJ-E2E-DEC-009` | `1.0.0` | `DataTable`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `StatusBadge`, `AccessDeniedGate`, `ToastStack` | Route, view, service, butir menu | `BE-RJE-012` | Kartu | Pola Bank Darah FE + `UAT-15`, `16` | Klik ganda / Billing | Kartu |

## Aturan yang berlaku untuk setiap task

- **Verifikasi pola Bank Darah frontend** (contoh `bank-darah/task/report/frontend/FE-BD-005.md`
  bagian `6`): `npm run lint:errors`, `npm run build`, `npm run test:unit` (test fungsi murni di folder
  `tests/unit/` **yang sudah ada**), dan validasi runtime R0..Rn di Chromium terhadap backend
  sungguhan (`tests/e2e/*.spec.mjs` **yang sudah ada polanya**, cookie dan alamat dari environment,
  data samaran). Kegagalan test lama dibandingkan dengan baseline, bukan diabaikan. Tidak ada folder
  test baru.
- **Kewenangan UI.** Isi dan sumber data mengikuti skema `03-frontend-architecture.md` bagian `V2.4`.
  Tata letak, warna, jarak, ikon, nama dan urutan butir menu `DEV_DISCRETION`. Setiap elemen UI
  diputuskan reuse atau buat baru berdasarkan bukti.
- **Keamanan.** Tidak ada tombol Bayar/Diskon/Void/Finalisasi/Settlement; tidak ada
  `dangerouslySetInnerHTML`; tombol tanpa hak akses disembunyikan, dan server tetap menolak `403`.
- **Laporan** ditulis ke `task/report/frontend/<TASK-ID>.md`; tanda pada grafik, tabel, dan kartu
  diperbarui bersama.

## Kartu task

### `FE-RJE-001` — Tab Ringkasan Billing

| Field | Isi |
| --- | --- |
| **Outcome** | dr. B membuka tab dan melihat status tagihan, nomor invoice, total, bagian penjamin dan pasien, serta daftar pelayanan tanpa harga |
| **Cakupan** | `encounter-billing-summary.service.js`; `doctor-billing-summary-tab.jsx`; entri baru `DOCTOR_QUEUE_TABS`; render di `doctor-queue-view.jsx`; tab hanya tampil bila `usePermission("EncounterBillingSummary", "Read")`; tombol *Buka Detail Billing* hanya bila `usePermission("BillingInvoice", "Read")`, menuju detail invoice (arti `[slug]` pada `…/invoices/[slug]/detail-billing` dipastikan saat preflight) |
| **Acceptance criteria** | 1. `NO_INVOICE` → kalimat "Belum ada tagihan untuk kunjungan ini…", bukan galat (`UAT-17`). 2. Angka tampil apa adanya dari API, tanpa dijumlah ulang; sama dengan `calculation-preview` (`UAT-24`). 3. Daftar pelayanan tanpa kolom harga. 4. Pengguna tanpa `BillingInvoice : Read` tidak melihat tombol detail (`UAT-18`). 5. Muat ulang mempertahankan data lama yang sah selama memuat. 6. Gagal → pesan + Coba lagi. 7. Teks `<script>` tampil sebagai teks |
| **Verifikasi** | Pola Bank Darah FE; runtime terhadap kunjungan uji dengan dan tanpa invoice |
| **DoD** | AC terbukti; laporan |

### `FE-RJE-002` — Pemberitahuan penyerahan tagihan

| Field | Isi |
| --- | --- |
| **Status** | Kontrak `1.0.1` `approved` (`RJ-E2E-DEC-017`); menunggu `BE-RJE-013` selesai |
| **Outcome** | Dokter tahu bila penyerahan tagihan bermasalah, tanpa mengira konsultasinya gagal |
| **Cakupan** | Membaca `BillingHandoffIssues` dari respons tombol Selesai. Bila kelak `RJ-DOC-FE-001` memindahkan tombol ke `PATCH /doctor-consultations/{id}/complete`, field yang sama dibaca dari respons itu (sudah ada di `1.0.0`) |
| **Acceptance criteria** | 1. Daftar tidak kosong → pemberitahuan tampil; status konsultasi tetap selesai (`UAT-19`). 2. Daftar kosong → tidak ada pemberitahuan. 3. Bentuk pemberitahuan `DEV_DISCRETION` |
| **Verifikasi** | Pola Bank Darah FE; runtime dengan Billing dibuat gagal sementara |
| **DoD** | AC terbukti; laporan |

### `FE-RJE-003` — Layar Antrean Rekonsiliasi Tagihan Klinis

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas Billing membuka *Rekonsiliasi Tagihan Klinis* dari menu, menyaring, mengirim ulang, dan menyelesaikan item |
| **Cakupan** | `src/app/health-services/billing-management/billing/charge-reconciliations/page.jsx` + view; `charge-reconciliation.service.js`; butir menu di grup *Billing dan Kasir* (`menu-items.jsx`); aksi dijaga `BillingChargeReconciliation : Update` |
| **Acceptance criteria** | 1. Butir menu membuka layar (keterjangkauan). 2. Saringan status, jenis pelayanan, sebab, tanggal, pencarian ≤ 100 karakter. 3. Kirim Ulang item `TARIFF_NOT_FOUND` setelah tarif diisi → hilang dari antrean `ReconciliationRequired` (`UAT-15`). 4. Selesaikan tanpa alasan → pesan di bawah isian (`UAT-16`). 5. `409` → "Item sedang diproses sistem…", muat ulang tanpa kirim otomatis. 6. Tombol nonaktif selama permintaan berjalan. 7. Tanpa `Update` → daftar tanpa tombol aksi; tanpa `Read` → `AccessDeniedGate`. 8. Keempat keadaan memuat, kosong, gagal, berisi tersedia |
| **Verifikasi** | Pola Bank Darah FE; runtime dengan item antrean hasil `BE-RJE-001` (`LEGACY_PRE_BRIDGE`) atau tarif yang sengaja dikosongkan |
| **DoD** | AC terbukti; butir menu terdaftar; laporan |

## Coverage gap

| Gap | Penanganan |
| --- | --- |
| Layar master kebijakan kirim ulang | `POST-MVP` (`04-prd-to-mvp.md` bagian 8) |
| Tab pemesanan Lab/Radiologi dokter | Tetap `CONDITIONAL` (`RJ-DOC-DEC-002`) |
| E2E lintas layar (dokter → kasir → farmasi) | `verify-module-readiness` setelah `MVP-4` |
