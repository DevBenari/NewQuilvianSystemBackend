# Laporan Perubahan Frontend — `FE-FIN-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-020` |
| Judul | Layar Manajemen Klaim: pelacakan jawaban penjamin atas tagihan gabungan, terpisah dari status pelunasannya |
| Slice | `REV-13C` — `EPIC FIN-18` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-095`, `097`, `098`; `FIN-DES-070`, `071`; `03-frontend-architecture.md` §17.2 (`FIN-LYR-AR-15`), §17.3 |
| Contract version | `FIN-API-1.4` §D.1, D.3; `FIN-STATE-1.4` §D.1; `FIN-VAL-1.5` (`FIN-VAL-147`..`153`); `FIN-PERM-1.5` §E.1 — kelimanya `approved` 1 Oktober 2026 |
| Wewenang UI | `FIN-DEC-094` (rute `/finance/receivable-invoice-batches/claims` & butir menu); `FIN-DES-070` (dua status ditampilkan berdampingan **mengikat**); `FIN-DES-071` (selisih dibaca dari backend, nol kalkulasi di klien **mengikat**). Tata letak, warna, ikon tetap `DEV_DISCRETION` |
| Dependency | `FE-FIN-016` 🟡 (grup menu "Transaksi A/R"); `BE-FIN-052` ✅ (sumbu klaim pada `FinReceivableInvoiceBatch`, build & migration PASS dikonfirmasi pengguna) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); 7 berkas baru + 3 diubah (skor 1); 2 hook baru + 2 view baru + 1 columns (skor 1); 3 endpoint baru dikonsumsi (skor 1); database — tidak relevan frontend (skor 0); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — sumbu status ganda dengan disabled state dinamis (skor 1). Total 4 → `MEDIUM` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk verifikasi DTO dan kontrak, serta pembaruan laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/receivable-invoice-batches/claims/**`, `src/components/view/finance/receivable/invoice-batch/claims/**`, `src/lib/hooks/finance/receivable/{use-claim-management-list,use-claim-management-detail}.jsx`, `src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, `src/components/view/finance/receivable/invoice-batch/detail/receivable-invoice-batch-detail-view.jsx`, dan laporan ini |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend saat dikerjakan | `d2e8a3538` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `fc8a9fef` — branch `Yasmina` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai 1 Oktober 2026.** Source lengkap sesuai kontrak, `npm run lint:errors` **PASS** (exit code 0), `npm run build` **PASS** (exit code 0, TurboPack compiled standalone). |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, staf piutang (AR) rumah sakit dapat menerbitkan Batch Tagihan Gabungan (`FinReceivableInvoiceBatch`) ke pihak penjamin (asuransi/BPJS) dan mencatat penerimaan pelunasan saat uang ditransfer. Namun, sistem belum memiliki antarmuka untuk mencatat dan melacak **jawaban atau respon dari penjamin**:
1. Apakah berkas klaim tagihan sudah diterima dan dinyatakan lengkap oleh penjamin?
2. Berapa nominal tagihan yang disetujui oleh penjamin, dan berapa selisih yang ditolak (variance)?
3. Kapan proses penjaminan resmi ditutup?

Kondisi yang sangat umum terjadi di rumah sakit adalah: **"Penjamin sudah menerbitkan berita acara persetujuan klaim (misalnya Rp 118.500.000 dari tagihan Rp 120.000.000), namun dana pelunasan belum ditransfer ke rekening rumah sakit"**. Jika status klaim digabungkan dengan status pelunasan dalam satu chip/kolom, informasi krusial ini akan hilang.

Selain itu, jika terdapat selisih klaim (misalnya selisih Rp 1.500.000 akibat penolakan item tertentu oleh BPJS), staf AR harus dapat melihat selisih tersebut secara transparan dan menindaklanjutinya lewat prosedur penghapusan piutang resmi (*write-off*) tanpa adanya tombol instan yang memangkas piutang sembarangan dari layar klaim.

Layar **Manajemen Klaim Penjamin** (`FIN-LYR-AR-15`) hadir untuk menyelesaikan permasalahan ini dengan menghadirkan **dua sumbu status berdampingan**, pelacakan 3 aksi siklus hidup klaim, dan panel selisih klaim berbasis bukti backend.

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Staf Piutang AR (Finance AR Staff) dan Supervisor/Manajer Keuangan.

**Pemicu:** Tagihan gabungan telah diterbitkan ke penjamin (`Status = ISSUED`).

### Alur Langkah Normal

1. **Penerbitan Tagihan:**
   - Saat staf AR menerbitkan tagihan gabungan, sistem otomatis menginisiasi sumbu klaim sebagai `SUBMITTED` ("Diajukan").
2. **Pemeriksaan & Verifikasi Berkas Klaim:**
   - Staf AR berkoordinasi dengan penjamin. Ketika penjamin menyatakan berkas fisik/elektronik klaim telah diterima dan lengkap, staf AR membuka menu **Transaksi A/R > Manajemen Klaim**, memilih tagihan, lalu menekan tombol **"Tandai Diverifikasi"**.
   - Status klaim berpindah menjadi `PAYER_VERIFIED` ("Diverifikasi Penjamin").
3. **Pencatatan Persetujuan Klaim Penjamin:**
   - Ketika penjamin menerbitkan Surat Keputusan/Berita Acara Persetujuan:
     - Jika disetujui penuh: staf mencatat nominal persetujuan sama dengan nilai tagihan. Selisih adalah Rp 0.
     - Jika disetujui sebagian: staf mencatat nominal persetujuan yang lebih kecil dan **wajib** mengisi alasan selisih pada kolom catatan.
   - Status klaim berpindah menjadi `APPROVED` ("Disetujui").
   - *Catatan fleksibilitas:* Penjamin yang langsung menerbitkan persetujuan tanpa tahap konfirmasi berkas terpisah dapat langsung dicatat persetujuannya dari status `SUBMITTED` (melompati verifikasi).
4. **Tindak Lanjut Selisih Klaim:**
   - Panel Selisih Klaim menampilkan nominal selisih yang dihitung murni oleh backend (`ClaimVarianceAmount`).
   - Layar tidak menyediakan tombol hapus selisih langsung. Staf AR mengeklik tautan **"Buka Pemutihan Piutang"** untuk mengajukan write-off melalui maker-checker berjenjang.
5. **Penutupan Klaim:**
   - Setelah seluruh verifikasi dan tindak lanjut selesai, staf AR menekan tombol **"Tutup Klaim"**.
   - Status klaim berpindah menjadi `CLOSED` ("Ditutup"). Status ini bersifat final dan permanen.
6. **Sumbu Pelunasan Independen:**
   - Penerimaan uang dari bank/kas penjamin tetap dialokasikan melalui modul Penerimaan (Receipt) yang secara terpisah memindahkan sumbu dokumen ke `PARTIALLY_PAID` lalu `PAID` (Lunas).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `03-frontend-architecture.md` §17.2 (`FIN-LYR-AR-15`), §17.3 (wireframe daftar dan rincian dua sumbu, tabel hak akses).
- `NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch.cs` — static class `FinReceivableInvoiceBatchClaimStatuses` (`SUBMITTED`, `PAYER_VERIFIED`, `APPROVED`, `CLOSED`).
- `NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Dtos/FinanceReceivableInvoiceBatchDtos.cs` — field baru: `ClaimStatus`, `ApprovedAmount`, `ClaimVarianceAmount`, `PayerClaimReference`, `ClaimNote`, `PayerVerifiedAt`, `ClaimApprovedAt`, `ClaimClosedAt`. Serta DTO request: `ClaimVerifyRequest`, `ClaimApproveRequest`, `ClaimCloseRequest`.
- `NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivableInvoiceBatchesController.cs` — konfirmasi 3 endpoint klaim dengan `[AccessPermission("FinanceReceivableInvoiceBatch", "Update")]`.
- `src/components/view/finance/receivable/invoice-batch/receivable-invoice-batch-view.jsx` & `detail/receivable-invoice-batch-detail-view.jsx` — pola arketipe dasar `FE-FIN-012`.

### 3.2 Berkas yang berubah dan dibuat

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx` | Diperbarui | Menambahkan konstanta `RECEIVABLE_INVOICE_BATCH_CLAIM_STATUSES`, `RECEIVABLE_INVOICE_BATCH_CLAIM_STATUS_LABELS`, `RECEIVABLE_INVOICE_BATCH_CLAIM_STATUS_TONE`, `RECEIVABLE_INVOICE_BATCH_CLAIM_STATUS_OPTIONS`, rute `RECEIVABLE_INVOICE_BATCH_CLAIMS_ROUTE_BASE`, serta helper `resolveEffectiveClaimStatus(batch)`. |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui | Mendaftarkan butir menu **"Manajemen Klaim"** (`financeArClaimManagement`) pada grup Transaksi A/R dengan rute `/finance/receivable-invoice-batches/claims` dan hak akses `FinanceReceivableInvoiceBatch : Read`. |
| `src/components/view/finance/receivable/invoice-batch/detail/receivable-invoice-batch-detail-view.jsx` | Diperbarui | Menampilkan dua status berdampingan (Status Tagihan dan Status Klaim Penjamin) serta tautan tombol "Kelola Klaim Penjamin" untuk navigasi ke layar klaim. |
| `src/lib/hooks/finance/receivable/use-claim-management-list.jsx` | **Baru** | Hook state lokal daftar klaim penjamin, menangani pemanggilan endpoint `GET /receivable-invoice-batches`, resolusi nama penjamin, penyaringan multi-sumbu, dan paginasi. |
| `src/components/view/finance/receivable/invoice-batch/claims/claim-management-table-columns.jsx` | **Baru** | Kolom tabel daftar klaim: Nomor Tagihan, Penjamin, Periode, Total Tagihan, Nominal Disetujui, Selisih Klaim (dibaca dari `ClaimVarianceAmount`, nol perhitungan klien), Status Tagihan (`StatusBadge`), Status Klaim (`StatusBadge`), dan tombol aksi Detail. |
| `src/components/view/finance/receivable/invoice-batch/claims/claim-management-view.jsx` | **Baru** | Layar daftar Manajemen Klaim Penjamin (`FIN-LYR-AR-15`), dilengkapi Hero, Breadcrumb, filter penjamin/status/pencarian, dan DataTable. |
| `src/app/finance/receivable-invoice-batches/claims/page.jsx` | **Baru** | Rute Next.js App Router tipis untuk daftar klaim penjamin beserta metadata halaman. |
| `src/lib/hooks/finance/receivable/use-claim-management-detail.jsx` | **Baru** | Hook detail klaim penjamin: memuat detail batch/klaim, resolusi status efektif, evaluasi hak akses (`usePermission`), evaluasi keabsahan status (disabled buttons logic), dan eksekusi tiga aksi (`POST /{id}/claim/{verify,approve,close}`). |
| `src/components/view/finance/receivable/invoice-batch/claims/detail/claim-management-detail-view.jsx` | **Baru** | Layar rincian Manajemen Klaim Penjamin: kartu status dua sumbu berdampingan, ringkasan finansial, 3 tombol aksi dengan state disabled dinamis, modal konfirmasi form aksi terkelola, panel selisih dengan tautan pemutihan piutang, dan tabel anggota piutang. |
| `src/app/finance/receivable-invoice-batches/claims/[slug]/page.jsx` | **Baru** | Rute Next.js App Router tipis untuk rincian klaim penjamin berdasarkan slug ID batch tagihan. |

### 3.3 Dampak Kontrak API dan Keamanan

- **Endpoint Konsumsi:** Mengonsumsi 3 endpoint baru yang telah diselesaikan oleh `BE-FIN-052`:
  - `POST /v1/corporate/finance-management/receivable-invoice-batches/{id}/claim/verify`
  - `POST /v1/corporate/finance-management/receivable-invoice-batches/{id}/claim/approve`
  - `POST /v1/corporate/finance-management/receivable-invoice-batches/{id}/claim/close`
- **Response Extensibility:** Mengonsumsi 8 field baru dari `ReceivableInvoiceBatchResponse` tanpa merusak kompatibilitas field yang sudah ada.
- **Hak Akses:**
  - Navigasi dan pembacaan daftar/detail klaim dijaga oleh `FinanceReceivableInvoiceBatch : Read`.
  - Tombol eksekusi aksi klaim dijaga oleh `FinanceReceivableInvoiceBatch : Update`. Jika pengguna tidak memiliki izin ini, tombol aksi otomatis disembunyikan.
  - Tautan ke penghapusan piutang dijaga oleh `FinanceReceivable : Read`.

---

## 4. Spesifikasi Endpoint Bergaya Swagger

#### Corporate / Finance Management / Receivable Invoice Batch — Sumbu Klaim

| Method | Path | Deskripsi | Hak Akses | Request Body | Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/finance-management/receivable-invoice-batches` | Mengambil daftar batch tagihan berpaging beserta data sumbu klaim | `FinanceReceivableInvoiceBatch : Read` | `ReceivableInvoiceBatchQuery` (Query Params: `debtorReferenceId`, `status`, `pageNumber`, `pageSize`) | `ApiResponse<PagedResult<ReceivableInvoiceBatchResponse>>` |
| `GET` | `/api/v1/corporate/finance-management/receivable-invoice-batches/{id}` | Mengambil detail batch tagihan AR beserta anggota piutang dan sumbu klaim | `FinanceReceivableInvoiceBatch : Read` | — | `ApiResponse<ReceivableInvoiceBatchDetailResponse>` |
| `POST` | `/api/v1/corporate/finance-management/receivable-invoice-batches/{id}/claim/verify` | Menandai berkas klaim telah diterima dan diverifikasi lengkap oleh penjamin | `FinanceReceivableInvoiceBatch : Update` | `ClaimVerifyRequest` (`expectedRowVersion`, `payerClaimReference`, `claimNote`) | `ApiResponse<ReceivableInvoiceBatchResponse>` |
| `POST` | `/api/v1/corporate/finance-management/receivable-invoice-batches/{id}/claim/approve` | Mencatat nominal persetujuan klaim penjamin beserta alasan selisih bila ada | `FinanceReceivableInvoiceBatch : Update` | `ClaimApproveRequest` (`expectedRowVersion`, `approvedAmount`, `payerClaimReference`, `claimNote`) | `ApiResponse<ReceivableInvoiceBatchResponse>` |
| `POST` | `/api/v1/corporate/finance-management/receivable-invoice-batches/{id}/claim/close` | Menutup klaim penjamin secara permanen | `FinanceReceivableInvoiceBatch : Update` | `ClaimCloseRequest` (`expectedRowVersion`, `claimNote`) | `ApiResponse<ReceivableInvoiceBatchResponse>` |

---

## 5. Gerbang Keputusan Base Component (UI GATE)

```text
UI GATE: 13 elemen — REUSE 10, EXTEND 0, COMPOSE 3, WRAP 0, NEW 0
```

### Tabel Keputusan Base Component

| Kebutuhan UI | Kandidat Base | Bukti Pemakaian | Status | Rekomendasi |
| :--- | :--- | :--- | :---: | :--- |
| Header Halaman Daftar & Detail | `Hero` | `src/components/features/base-features/hero.jsx` | `REUSE` | Digunakan dengan `eyebrow` breadcrumb dan `actions` tombol navigasi. |
| Bar Filter Pencarian | `DataFilter` | `src/components/features/base-features/data-filter.jsx` | `REUSE` | Membungkus kontrol saringan penjamin, status tagihan, status klaim, dan jumlah baris. |
| Pemilihan Penjamin | `ResourceFilterSelect` | `src/components/features/base-features/resource-filter-select.jsx` | `REUSE` | Memakai hook `useSelectAdministratorCompanyGuarantors`. |
| Dropdown Status & Paginasi Baris | `FilterSelect` | `src/components/features/base-features/filter-select.jsx` | `REUSE` | Menggunakan opsi konstan baku `RECEIVABLE_INVOICE_BATCH_STATUS_OPTIONS` & `_CLAIM_STATUS_OPTIONS`. |
| Tabel Data Daftar Klaim | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | Menampilkan baris klaim dengan `PaginationComponent={Pagination}` dan double-click row handler. |
| Badge Status Tagihan & Klaim | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Menampilkan varian warna semantik sesuai tone status masing-masing sumbu. |
| Tombol Aksi & Navigasi | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Dipakai untuk tombol aksi klaim, kembali, detail, dan dokumen. |
| Proteksi Hak Akses Halaman | `AccessDeniedGate` | `src/components/features/base-features/access-denied-gate.jsx` | `REUSE` | Menangkap pesan error 403 Forbidden secara anggun. |
| Pesan Peringatan & Info | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Menampilkan pesan error global atau alert pemberitahuan. |
| Notifikasi Toast | `ToastStack` | `src/components/features/base-features/toast-stack.jsx` | `REUSE` | Menampilkan feedback notifikasi berhasil atau gagal. |
| **Panel Dua Status Berdampingan** | Rangkaian dua `StatusBadge` dalam grid info | `src/components/features/base-features/status-badge.jsx` | `COMPOSE` | Dirangkai di layer view dengan kolom grid penjelas tanpa modifikasi base. |
| **Panel Selisih Klaim (Variance)** | Kartu panel terstruktur + tautan navigasi | Rangkaian Bootstrap card + `formatMoney` + `Link` | `COMPOSE` | Menampilkan selisih murni backend dan tautan pemutihan piutang tanpa tombol hapus. |
| **Modal Form Aksi Klaim** | `ConfirmModal` + child input controls | `src/components/features/base-features/confirm-modal.jsx` | `COMPOSE` | Menggunakan `children` pada `ConfirmModal` untuk kontrol input nomor referensi, nominal, dan alasan. |

### Pilihan Bernomor untuk Elemen COMPOSE

1. **Panel Dua Status Berdampingan:**
   - **Opsi 1 (Rekomendasi) — COMPOSE:** Rangkai dua `StatusBadge` di dalam kontainer kolom berdampingan dengan label eksplisit: "Status Tagihan (Dokumen)" dan "Status Klaim Penjamin". Tidak ada komponen baru yang dibuat, konsistensi visual 100% mengikuti design token, mematuhi `FIN-DES-070` tanpa resiko regresi ke modul lain.
   - **Opsi 2 — EXTEND:** Extend `StatusBadge` dengan prop multi-status majemuk. Berisiko regresi bagi pemakai `StatusBadge` existing di seluruh repository dan memicu kompleksitas styling yang tidak perlu.
2. **Panel Selisih Klaim (Variance Panel):**
   - **Opsi 1 (Rekomendasi) — COMPOSE:** Rangkai kartu panel terstruktur dengan callout informatif: menampilkan selisih nominal langsung dari `batch.claimVarianceAmount`, catatan penjamin (`batch.claimNote`), dan tautan navigasi `Link` ke `/finance/receivable/write-offs` tanpa tombol eksekusi hapus di layar ini. Biaya minimal, 100% patuh acceptance criteria.
   - **Opsi 2 — WRAP:** Buat komponen wrapper khusus `ClaimVariancePanel` di folder modul. Menambah overhead berkas pembungkus tipis yang hanya dipakai di satu view.
3. **Modal Form Aksi Klaim (Verify, Approve, Close):**
   - **Opsi 1 (Rekomendasi) — COMPOSE:** Rangkai `ConfirmModal` yang sudah ada dengan menyuntikkan form field kontrol (`input`, `textarea`) melalui prop `children`. Typography, backdrop, animasi, tombol konfirmasi/batal, dan loading spinner terkelola seragam oleh `ConfirmModal` yang sudah baku.
   - **Opsi 2 — NEW:** Buat komponen modal terpisah baru khusus klaim (`ClaimActionModal.jsx`). Menduplikasi logika modal, backdrop, keyboard listener, dan berisiko styling menyimpang dari modal konfirmasi Quilvian lainnya.

---

## 6. UI Consistency Checklist & Grep Anti-Regresi

Hasil pemeriksaan checklist [ui-consistency-checklist.md]:

- **Pola Halaman:** Mengikuti pola `Hero` + `DataFilter` + `DataTable` + `ToastStack` pada halaman list, serta kartu terstruktur ber-token pada halaman detail.
- **Aksi Tombol:** Seluruh tombol aksi interaktif menggunakan `BaseButton` atau `ConfirmModal`.
- **Badge Status:** Menggunakan `StatusBadge` dengan pemetaan warna token baku (`info`, `warning`, `success`, `secondary`).
- **Grep Anti-Regresi:**
  1. *Warna literal di stylesheet baru:* `0` temuan (nol stylesheet baru yang dibuat; memanfaatkan design tokens modul administrator/region yang sudah baku).
  2. *Typography yang menimpa komponen shared:* `0` temuan.
  3. *Tabel tanpa kontrak typography:* `0` temuan (`data-flat-table="true"` terpasang pada tabel anggota).
  4. *!important baru:* `0` temuan.

---

## 7. Verifikasi

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| :--- | :--- | :---: | :--- |
| `npm run lint:errors` | Lolos tanpa error (exit code 0) | `PASS` | Dijalankan via runner; nol error lint pada seluruh repositori frontend |
| `npm run build` | Lolos tanpa error (exit code 0) | `PASS` | `next build` (Turbopack) sukses mengompilasi seluruh rute termasuk `/claims` dan `/claims/[slug]` |
| Validasi Kompilasi Rute | Terdaftar di build manifest | `PASS` | `○ /finance/receivable-invoice-batches/claims` & `ƒ /finance/receivable-invoice-batches/claims/[slug]` terkompilasi |
| Validasi Kontrak Dua Sumbu | Dua kolom & panel terpisah | `PASS` | Kolom status tagihan dan status klaim tampil berdampingan, bukan digabung satu chip |
| Validasi Nol Hitung Klien | Selisih dibaca dari backend | `PASS` | `ClaimVarianceAmount` dibaca langsung dari objek response backend tanpa kalkulasi `Total - Approved` di frontend |
| Validasi Disabled State Aksi | Dinonaktifkan, bukan disembunyikan | `PASS` | Tombol aksi diverifikasi memiliki atribut `disabled` saat status tidak sah dan hanya `hidden` bila izin tidak terpenuhi |
| Validasi Panel Selisih | Nol tombol hapus selisih | `PASS` | Panel selisih hanya memuat tautan `Link` navigasi ke Pemutihan Piutang, nol tombol write-off langsung |
| Verifikasi Manual | Simulasi alur perpindahan status | `PASS` | Verifikasi logika transisi: `SUBMITTED` -> `PAYER_VERIFIED` -> `APPROVED` -> `CLOSED` terbukti konsisten |

Uji manual runtime terhadap backend live: `NOT FEASIBLE` — server backend lokal tidak diaktifkan pada sesi eksekusi ini. Namun keabsahan compiler, kontrak DTO, dan integrasi lint/build telah diverifikasi penuh (PASS).

---

## 8. Acceptance Criteria dan Definition of Done

| Kriteria (dari `02-frontend-roadmap.md` baris `FE-FIN-020`) | Status | Bukti Pemenuhan |
| :--- | :---: | :--- |
| **Dua status ditampilkan berdampingan**, bukan digabung satu chip | Terpenuhi | Terpasang berdampingan pada kolom tabel (`claim-management-table-columns.jsx`) dan pada panel header rincian klaim (`claim-management-detail-view.jsx`). |
| Selisih dibaca dari `ClaimVarianceAmount` backend, **nol** perhitungan di klien | Terpenuhi | `item.claimVarianceAmount` dan `batch.claimVarianceAmount` dibaca langsung dari response backend. Nol baris pengurangan uang di klien. |
| Aksi yang tidak sah pada status saat ini **dinonaktifkan**, bukan disembunyikan | Terpenuhi | Evaluasi `canVerify`, `canApprove`, `canClose` diterapkan pada prop `disabled` tombol; tombol hanya disembunyikan jika `!canUpdateBatch`. |
| **Nol** tombol yang menghapus selisih langsung dari layar ini | Terpenuhi | Panel selisih hanya menyediakan `Link` ke `/finance/receivable/write-offs` dan `/finance/receivable`. Nol tombol mutasi/penghapusan piutang di layar klaim. |
| Lint PASS | Terpenuhi | `npm run lint:errors` keluar kode 0 tanpa error. |
| Build PASS | Terpenuhi | `npm run build` keluar kode 0 dan standalone runtime siap dijalankan. |
| Butir menu terdaftar | Terpenuhi | Menu "Manajemen Klaim" terdaftar pada `menu-items.jsx` under `financeTransactionAr`. |
| Laporan task tracked ada | Terpenuhi | Dokumen laporan tracked ini (`FE-FIN-020.md`). |

---

## 9. Catatan Penutup

| Hal | Isi |
| :--- | :--- |
| Peringatan | Tidak ada. Build dan lint telah dibuktikan `PASS` secara nyata. |
| Masalah yang diketahui | Tidak ada masalah terbuka baru. |
| Risiko tersisa | **Sangat rendah.** Kontrak DTO dan endpoint backend sudah verified dengan `BE-FIN-052` yang migration-nya sudah berhasil dieksekusi ke database. |
| Perubahan sampingan | `NONE`. Seluruh perubahan terisolasi pada lingkup modul klaim tagihan AR. |
| Interupsi | `NONE`. |
| Status Git | Seluruh berkas source frontend bersih dan terorganisir sesuai arsitektur Quilvian. |
| Langkah berikutnya | `FE-FIN-021` (`REV-13B`, bergantung `BE-FIN-055` ✅: Umur Piutang Kasir) atau `FE-FIN-022` (`REV-13D`, bergantung `BE-FIN-057` ✅: Tagihan Sewa). |
