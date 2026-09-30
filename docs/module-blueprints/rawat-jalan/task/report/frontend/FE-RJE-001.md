# Laporan Perubahan Frontend — `FE-RJE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RJE-001` |
| Judul | Tab Ringkasan Billing |
| Slice | `MVP-4` — `EPIC RJE-07` Ringkasan Billing untuk dokter |
| Roadmap | [roadmap/e2e-frontend-roadmap.md](../../../roadmap/e2e-frontend-roadmap.md), kartu `FE-RJE-001` |
| Trace | `FR-RJE-061`; `AC-RJ-009`, `010`, `015`; `RJ-E2E-DEC-008`; `RJ-E2E-FE-001`, `002`, `003`; `03-frontend-architecture.md` V2.4, V2.6; `SEC-RJ-004` |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` (endpoint `GET /encounter-billing-summaries/{encounterId}` dari `BE-RJE-014`) |
| Wewenang UI | Isi dan sumber data wajib mengikuti skema V2.4 (`RJ-E2E-FE-001/002` approved). Tata letak, urutan, gaya `DEV_DISCRETION` (`RJ-E2E-FE-003`) dan hanya memakai base component serta token |
| Dependency | `BE-RJE-014` ✅ (di-commit pemilik) |
| Klasifikasi | `MEDIUM` — satu tab baru, satu service, satu hook, penyaringan tab menurut hak akses |
| Task mode | `FRONTEND` — frontend target tulis; backend read-only (kecuali laporan dan tautan bukti ini) |
| Target tulis | `V2QuilvianSystemFrontendDev` (`sukmagpV2`); laporan di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `83b8b7274` (`sukmagpV2`), working tree bersih saat mulai |
| Commit backend yang dijadikan rujukan | HEAD `sukmagp` sesudah commit pemilik atas `BE-RJE-011/012/013` (build `out-rje012b`) |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI** — ketujuh acceptance criteria terbukti di Chromium terhadap backend sungguhan |

---

## 1. Keadaan yang ditemukan di awal

Workspace konsultasi dokter (`/health-services/registration-management/doctor-queues`) punya tab
Hasil Skrining, SOAP, CPPT, Resep, Tindakan, Surat Dokter, dan beberapa tab lain. Tidak ada satu pun
yang menunjukkan apakah pelayanan dokter sudah masuk tagihan. Semua tab bersifat statis; belum ada
tab yang tampil atau tersembunyi menurut hak akses.

**Preflight rute detail invoice.** Kartu meminta arti `[slug]` pada
`…/billing/invoices/[slug]/detail-billing` dipastikan lebih dulu. Hasilnya: `[slug]` adalah **token
rute privat** yang dibuat `registerPrivateRouteToken({ scope: "billing-invoice", type: "invoice", id })`,
bukan ID invoice mentah. Pola ini dipakai `use-cashier-billing-overview.js`, dan halaman detail
membacanya lewat `resolvePrivateRouteToken`.

---

## 2. Proses bisnis dari sisi pengguna

1. dr. B membuka *Klinis Dokter*, lalu menekan **Lanjutkan** pada pasien yang sedang dikonsultasi.
2. Tab **Ringkasan Billing** tampil bila akun dr. B memegang `EncounterBillingSummary : Read`.
3. Saat tab dibuka, ringkasan dibaca dari Billing:
   - **Kepala:** status tagihan (mis. *Berjalan*), nomor invoice, penjamin (*Tunai*), dan waktu
     tagihan terakhir diperbarui.
   - **Angka:** Total Pelayanan, Total Tagihan, Ditanggung Penjamin, dan Tanggungan Pasien. Semuanya
     tampil **persis** dari Billing, tanpa dijumlah ulang di layar.
   - **Pemberitahuan:** bila ada pelayanan yang masih diproses atau perlu ditinjau Billing, misalnya
     "1 pelayanan sedang diproses ke tagihan · 0 perlu rekonsiliasi oleh Billing."
   - **Daftar pelayanan:** nama, jumlah, status pelayanan, dan status tagihan (*Tercatat*,
     *Menunggu*, *Perlu Ditinjau*, *Tidak Ditagihkan*), **tanpa harga**.
4. **Muat ulang** membaca ringkasan lagi. Data yang sedang tampil **tidak** hilang selama memuat.
5. Petugas yang juga memegang `BillingInvoice : Read` melihat tombol **Buka Detail Billing**, yang
   membuka detail invoice di modul Billing.

**Jalur tidak normal.**
- Kunjungan belum punya tagihan: "Belum ada tagihan untuk kunjungan ini. Tagihan terbentuk otomatis
  setelah pelayanan pertama tercatat." Ini keadaan normal, bukan galat.
- Gagal memuat pertama kali: "Ringkasan tagihan gagal dimuat" dengan tombol **Coba lagi**.
- Gagal saat muat ulang: pesan yang sama muncul di atas data terakhir yang berhasil, yang tetap
  tampil.
- Tanpa hak akses: tab tidak tampil. Bila server menjawab `403`, panel menampilkan "Akses Ringkasan
  Billing tidak tersedia".
- `404`: "Kunjungan tidak ditemukan."
- `422`: pesan kalkulasi dari Billing.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `doctor-queue-view.jsx`, `doctor-queue-client.jsx`, `ConsultationTabs.jsx`,
`ClinicalTabNav.jsx`, `QueuePatientCard.jsx`, `useDoctorConsultationWorkspace.js`,
`doctor-queue.constants.js`, `doctor-queue-display-utils.js`; rujukan visual
`nursing-billing-section.jsx` beserta `patient-billing-summary.service.js` dan
`patient-billing-summary-utils.js`; `use-permission.jsx`; base component `summary-grid`, `data-table`,
`status-badge`, `information-alert`, `base-button`, `ClinicalContentPanel`, `ClinicalStateBoundary`;
`billing-invoice-constants.js`, `use-cashier-billing-overview.js`, `private-route-token-utils`; halaman
`invoices/[slug]/detail-billing`; spec e2e Bank Darah `blood-unit-storage-screen.spec.mjs` (pola);
backend `EncounterBillingSummaryController`/`Dtos` (kontrak).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/billing-management/encounter-billing-summary.service.js` | **Baru.** `getEncounterBillingSummary(encounterId, config)` lewat `InstanceAxios`, meneruskan `signal` |
| `src/utils/health-services/billing-management/encounter-billing-summary-utils.js` | **Baru.** Normalisasi respons dengan *whitelist* field (harga per item dibuang walau dikirim), label status tagihan dan status pelayanan, kartu angka dari nilai API apa adanya, kalimat pemberitahuan sinkron |
| `src/lib/hooks/health-services/registration-management/doctor-queue/use-doctor-billing-summary.js` | **Baru.** Controller tab: muat awal vs muat ulang, data lama dipertahankan saat muat ulang dan saat gagal, `403`/`404`/`422`, abort saat berganti kunjungan |
| `src/components/view/health-services/registration-management/doctor-queues/tabs/billing-summary/doctor-billing-summary-tab.jsx` | **Baru.** Komposisi tab dari base component; token rute detail invoice didaftarkan saat tombol ditekan |
| `src/style/health-services/registration-management/doctor-queues/doctor-billing-summary-tab.module.css` | **Baru.** Tata letak kepala dan tombol; hanya token |
| `src/lib/constants/health-services/registration-management/doctor-queue/doctor-queue.constants.js` | `DOCTOR_BILLING_SUMMARY_PERMISSION` dan entri tab `billingSummary` di `DOCTOR_QUEUE_TABS` |
| `src/components/features/health-services/doctor-queue-features/ConsultationTabs.jsx` | Menyaring tab `billingSummary` dengan `usePermission("EncounterBillingSummary", "Read")` |
| `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Merender `DoctorBillingSummaryTab` untuk tab `billingSummary` |
| `tests/unit/doctor-billing-summary.test.mjs` | **Baru** (opsional) — 6 test fungsi murni di folder yang sudah ada |
| `tests/e2e/doctor-billing-summary-tab.spec.mjs` | **Baru** — spec runtime R1–R5 di folder yang sudah ada, pola Bank Darah |

### 3.3 Kepatuhan arsitektur frontend

- Alur mengikuti `app → view → hook → service → InstanceAxios`. View tidak memanggil Axios, dan
  normalisasi respons ada di `utils`.
- Tidak ada Redux slice baru: data hanya dipakai satu tab, sesuai pola `nursing-billing-section`
  (service dan state lokal).
- Tidak ada komponen baru di `components/features` maupun `components/ui`.
- Tidak ada `dangerouslySetInnerHTML`; teks server dirender React sebagai teks.

**Gerbang base component.**

`UI GATE: 8 elemen — REUSE 7, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Panel tab beserta judul dan aksi | `ClinicalContentPanel` | `components/ui/clinical-workspace`, dipakai `nursing-billing-section` | REUSE | `title`, `subtitle`, `actions` = tombol Muat ulang |
| Keadaan memuat/gagal/tanpa akses | `ClinicalStateBoundary` | idem | REUSE | `loading`, `error` + `retryAction`, `denied` |
| Status tagihan dan status per pelayanan | `StatusBadge` | `base-features/status-badge.jsx` (`status` + `label`) | REUSE | Status `active/pending/warning/info/inactive` |
| Kepala: status, nomor invoice, penjamin, waktu | `StatusBadge` + teks | — | COMPOSE | Dirangkai di view; CSS module hanya untuk tata letak |
| Empat angka ringkasan | `SummaryGrid` | `base-features/summary-grid.jsx`; `formatValue` meneruskan untaian non-angka apa adanya | REUSE | Nilai Rupiah dari `formatBillingIdr` yang sudah ada |
| Pemberitahuan sinkron, keadaan tanpa invoice, galat saat muat ulang | `InformationAlert` | `base-features/information-alert.jsx` (`variant`, `title`, `message`, `children`) | REUSE | `warning`, `info`, `danger` |
| Daftar pelayanan | `DataTable` | `base-features/data-table.jsx` (`pagination={false}`, `sortLatestFirst={false}`) | REUSE | Empat kolom tanpa harga |
| Tombol Muat ulang, Coba lagi, Buka Detail Billing | `BaseButton` | `base-features/base-button.jsx` (`loading`, `loadingLabel`, `variant`) | REUSE | — |

Tidak ada pilihan yang perlu diajukan: semua elemen `REUSE` atau `COMPOSE` di layer view.
Perubahan `ConsultationTabs` menyentuh komponen fitur domain dokter, bukan base component.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat (pertama) | Kartu kerangka "Memuat ringkasan tagihan..." |
| Memuat ulang | Data lama tetap tampil; tombol berubah "Memuat..."; panel `aria-busy` |
| Kosong (`NO_INVOICE`) | "Belum ada tagihan untuk kunjungan ini. Tagihan terbentuk otomatis setelah pelayanan pertama tercatat." |
| Daftar pelayanan kosong | "Belum ada pelayanan tercatat." |
| Gagal (pertama) | "Ringkasan tagihan gagal dimuat" + **Coba lagi** |
| Gagal (saat muat ulang) | Peringatan merah di atas data terakhir yang berhasil + **Coba lagi** |
| `404` / `422` | "Kunjungan tidak ditemukan." / pesan kalkulasi dari Billing |
| Tanpa hak akses | Tab tidak tampil. `403` dari server → "Akses Ringkasan Billing tidak tersedia" |
| Tanpa `BillingInvoice : Read` | Tombol Buka Detail Billing tidak ada |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Billing Management / Encounter Billing Summary

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/billing-management/encounter-billing-summaries/{encounterId}` | Seluruh isi tab | `EncounterBillingSummary : Read` |

Rute tujuan tombol detail: `/health-services/billing-management/billing/invoices/{token}/detail-billing`
(butir `BillingInvoice : Read`, dijaga layar dan backend).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0`, nol error | `PASS` | — |
| `npx eslint` pada berkas task | Nol error, nol warning (sesudah penanda disable dipindah ke baris yang benar) | `PASS` | — |
| `npm run test:unit` | 2063 test: **2056 pass, 7 fail** | `UNRELATED EXISTING ISSUE` untuk 7 kegagalan | Kegagalan ada di `accounting-reconciliation`, `hemodialysis-navigation-and-privacy-audit` (2), `hemodialysis-sidebar-navigation` (2), `menu-permission-filter`, `petty-cash-finance-separation`. Tidak satu pun membaca berkas yang diubah task ini (diperiksa dengan grep path sumber tiap test). Keenam test baru `doctor-billing-summary.test.mjs` **pass** |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 57s`, `postbuild` standalone berhasil | `PASS` | — |
| `npx playwright test tests/e2e/doctor-billing-summary-tab.spec.mjs --workers=1` | **7 passed (29,0 detik)** pada run pertama | `PASS` | Bagian 6.1 |
| Run ulang sesudah perbaikan pesan (bersama spec `FE-RJE-002`) | **9 passed (33,8 detik)**; R5 kini juga memeriksa isi pesan gagal | `PASS` | Bagian 8 — perbaikan |
| `npm run test:unit` (run ulang, kode akhir) | 2066 test: 2059 pass, 7 fail — tujuh kegagalan lama yang sama | `UNRELATED EXISTING ISSUE` | — |

`AUTOMATED TEST: node --test tests/unit/doctor-billing-summary.test.mjs — PASS (6/6)`

### 6.1 Validasi runtime — 30 September 2026

Build standalone dijalankan di `http://127.0.0.1:3710` dan diuji lewat Playwright Chromium.
Backend sungguhan berjalan di `http://localhost:5219` terhadap `QuilvianNewDevSukma`.

- **Jawaban API tidak dikarang.** Setiap request `/v1/**` diteruskan ke backend dengan cookie sesi
  `superadmin` dari environment.
- **Yang dipasang di spec hanya:** (a) daftar izin pada R3b/R3c; (b) jeda empat detik atau pemutusan
  jaringan pada request ringkasan untuk R4/R5.
- **Data uji disiapkan lewat SQL dan dipulihkan sesudahnya.** Tanggal antrean `5e382440…` (invoice
  `OPEN`) dan `6ab76dc2…` (tanpa invoice) dimajukan ke hari ini, karena layar hanya memuat antrean
  hari ini. Deskripsi satu item invoice diberi penanda `<script>window.__rje001xss=1</script>TEST-RJE-FE001`
  untuk AC 7.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R1 | Antrean tanpa invoice → Lanjutkan → tab Ringkasan Billing | Kalimat "Belum ada tagihan untuk kunjungan ini…" tampil; tidak ada pesan galat; backend menjawab `200` `NO_INVOICE` | 1 | `PASS` |
| R2 | Antrean dengan invoice `OPEN` → tab | Total Tagihan, Ditanggung Penjamin, Tanggungan Pasien sama dengan `GET /billing/invoices/{id}/calculation-preview`; nomor invoice tampil; kepala tabel tanpa "harga/tarif/total"; jumlah baris = `Services[]`; penanda `<script>` tampil sebagai teks dan `window.__rje001xss` tetap `undefined` | 2, 3, 7 | `PASS` |
| R3a | Sesi `superadmin` (memegang `BillingInvoice : Read`) → Buka Detail Billing | Tombol tampil; klik tiba di `/billing/invoices/{token}/detail-billing` | 4 | `PASS` |
| R3b | Izin hanya `DoctorQueue : Read/FinishConsultation` + `EncounterBillingSummary : Read` | Tab tampil dan terisi; tombol Buka Detail Billing **tidak ada** | 4 | `PASS` |
| R3c | Izin tanpa `EncounterBillingSummary : Read` | Tab Ringkasan Billing tidak ada di navigasi; tidak ada request ringkasan sama sekali | — | `PASS` |
| R4 | Muat ulang dengan request ringkasan ke-2 dijeda 4 detik | Selama memuat: tombol "Memuat...", panel `aria-busy="true"`, nomor invoice lama tetap tampil; sesudahnya tombol kembali "Muat ulang" dengan data tetap ada | 5 | `PASS` |
| R5 | Request ringkasan ke-1 dan ke-3 diputus jaringannya | Pertama: "Ringkasan tagihan gagal dimuat" + Coba lagi → data termuat. Muat ulang gagal: pesan tampil di atas tabel yang tetap berisi → Coba lagi → pesan hilang | 6 | `PASS` |

`MANUAL TEST: PASS` — dijalankan agent lewat Chromium sungguhan terhadap backend sungguhan (bukan
jawaban tiruan).

**Pemeriksaan kebocoran.** Nol kemunculan token atau cookie di `test-results/` dan
`playwright-report/`. Berkas cookie di scratchpad dihapus sesudah run.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `NO_INVOICE` → kalimat "Belum ada tagihan untuk kunjungan ini…", bukan galat (`UAT-17`) | Terpenuhi | R1 |
| 2. Angka tampil apa adanya dari API, tanpa dijumlah ulang; sama dengan `calculation-preview` (`UAT-24`) | Terpenuhi | R2; test unit AC 2 |
| 3. Daftar pelayanan tanpa kolom harga | Terpenuhi | R2; test unit AC 3 (whitelist field, sumber tanpa `unitPrice`/`totalPrice`) |
| 4. Pengguna tanpa `BillingInvoice : Read` tidak melihat tombol detail (`UAT-18`) | Terpenuhi | R3a, R3b |
| 5. Muat ulang mempertahankan data lama yang sah selama memuat | Terpenuhi | R4 |
| 6. Gagal → pesan + Coba lagi | Terpenuhi | R5 |
| 7. Teks `<script>` tampil sebagai teks | Terpenuhi | R2 (penanda di data sungguhan); test unit AC 7 |
| DoD: laporan | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar 7 kegagalan unit lama (bagian 6) |
| Masalah yang diketahui | Selama daftar izin belum termuat, `usePermission` memilih tampil (perilaku bawaan hook): tab dan tombol detail dapat terlihat sesaat, lalu disembunyikan bila izin ternyata tidak ada. Backend tetap menolak `403`, jadi tidak ada data yang bocor |
| Dependency backend | `NONE` — `BE-RJE-014` sudah di-commit |
| Perubahan sampingan | Run Playwright mengubah dua berkas tracked (`test-results/.last-run.json` dan satu `error-context.md` lama terhapus). Keduanya tidak berubah saat task dimulai, dan dipulihkan dengan `git restore` pada kedua path itu saja. Data uji di database dipulihkan (tanggal antrean, deskripsi item) |
| Interupsi | `NONE` |
| Perbaikan sesudah run pertama | Saat mengerjakan `FE-RJE-002` ditemukan bahwa `InformationAlert` **tidak merender `message` bila `children` diisi**. Akibatnya, pada peringatan gagal saat muat ulang, isi pesan (mis. "Kunjungan tidak ditemukan.") tidak tampil — hanya judul dan tombol. Kalimat dipindah ke dalam `children` (`data-testid="billing-summary-refresh-error"`), spec R5 diperketat untuk memeriksa isinya, lalu dijalankan ulang: lulus. Build ulang `Compiled successfully in 45s`, `lint:errors` `PASS` |
| Status Git | `M ConsultationTabs.jsx`, `M doctor-queue-view.jsx`, `M doctor-queue.constants.js`; baru: `tabs/billing-summary/`, `use-doctor-billing-summary.js`, `encounter-billing-summary.service.js`, `doctor-billing-summary-tab.module.css`, `encounter-billing-summary-utils.js`, `tests/e2e/doctor-billing-summary-tab.spec.mjs`, `tests/unit/doctor-billing-summary.test.mjs`. Belum di-stage atau di-commit |
| Langkah berikutnya | `FE-RJE-002` (pemberitahuan penyerahan tagihan saat Selesai) |
