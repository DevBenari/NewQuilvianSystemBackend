# Laporan Perubahan Frontend — `FE-FIN-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-012` |
| Judul | Petugas AR menerbitkan satu dokumen tagihan untuk banyak piutang satu penjamin |
| Slice | `REV-4` — AR Invoice Agregat dan Potongan AR (`EPIC FIN-16`, `FIN-17`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 4 (tabel task, baris `FE-FIN-012`) dan `roadmap/00-delivery-roadmap.md` bagian 4 |
| Trace | `FIN-DEC-048`, `054`, `060`; `FR-FIN-089`..`092`; `FIN-API-1.1` §B.7; `FIN-PERM-1.1` §B.5; `FIN-STATE-1.2` §B.7; `FIN-VAL-1.2` `114`..`117` |
| Contract version | `FIN-API-1.1` (`locked` 25 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak layar Purchasing/AP dan Batch Tagihan (`03-frontend-architecture.md` §12.6); isi dan sumber data B.7 terkunci kontrak |
| Dependency | `BE-FIN-039` — lihat catatan konflik status pada bagian 8 (laporan tracked backend `BE-FIN-039.md` menyatakan 🟡 SEBAGIAN dan `dotnet build` sengaja `NOT RUN`, meski register `00-delivery-roadmap.md` menandainya ✅; pemilik repository mengonfirmasi langsung kepada saya bahwa `dotnet build` backend sudah dijalankan ulang dan berhasil sebelum task frontend ini dimulai) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); >8 berkas dibaca (roadmap, 3 dokumen kontrak, DTO/controller backend, 9 berkas frontend rujukan pola Purchasing/Receivable) (skor 1); 15 berkas dibuat/diubah (skor 1); logika UI sedang — seleksi multi-piutang, lifecycle 5 status, dokumen gabungan multi-invoice (skor 1); kontrak API — 7 endpoint baru dikonsumsi mengikuti pola yang sudah ada (skor 1); tanpa perubahan skema (skor 0); tanpa keputusan otorisasi baru — `AccessDeniedGate` existing (skor 0); UI/workflow — layar baru bercakupan sedang (skor 1). Total 5 → `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/receivable-invoice-batches/**`, `src/components/view/finance/receivable/invoice-batch/**`, `src/lib/hooks/finance/receivable/**` (baru), `src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx` (baru), `src/utils/finance/receivable/receivable-utils.jsx` (tambah `getApiErrorMessage`); `NewQuilvianSystemBackend` — laporan ini dan tautan bukti pada roadmap/traceability modul yang sama |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | Belum di-commit — lihat Status Git bagian 8 (branch `yasmina`, HEAD `a31da3c2`) |
| Commit backend yang dijadikan rujukan | `50f29ccc` (working tree juga memuat perubahan `BE-FIN-039` yang belum di-commit — lihat `BE-FIN-039.md`) |
| Tanggal | 30 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap, `npm run lint:errors` PASS, `npm run build` PASS; verifikasi manual/runtime **NOT FEASIBLE** — lihat bagian 6 |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini, layar Batch Tagihan AR (Tagihan Gabungan Penjamin) sama sekali belum ada di
frontend — roadmap mencatatnya "Belum dikerjakan". Backend-nya (`BE-FIN-039`) baru menyediakan
tujuh endpoint di `FinanceReceivableInvoiceBatchesController` (dikonfirmasi ada di source saat
task ini dimulai), tetapi laporan tracked-nya sendiri menyatakan `dotnet build` belum pernah
dijalankan dan migrasi skema `BE-FIN-038` belum dieksekusi ke database. Layar `/finance/receivable`
yang sudah ada dipakai sebagai sumber pemilihan piutang sesuai arahan task card, tetapi tidak ada
jalur dari layar itu menuju pembuatan batch — keduanya berdiri sendiri.

Komponen presentasional dokumen tagihan (`CompanyGuarantorInvoiceDocument`, dibangun untuk
`FE-BKC-031` pada modul Billing) sudah ada dan bentuknya persis sama dengan
`CompanyGuarantorInvoiceDocumentResponse` yang dipakai `GET /{id}/document` — ditemukan lewat
pemeriksaan berkas, dipakai ulang penuh tanpa modifikasi.

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Petugas AR.

**Langkah normal — Buat Batch:**

1. Petugas AR membuka **Tagihan Gabungan Penjamin** (`/finance/receivable-invoice-batches`), klik
   **+ Buat Batch Tagihan**.
2. Di layar Buat Batch (`/finance/receivable-invoice-batches/new`), petugas memilih **satu**
   penjamin lewat dropdown pencarian. Begitu penjamin dipilih, layar memuat daftar piutang yang
   memenuhi syarat digabung dari `GET /eligible-receivables?debtorReferenceId=...` — daftar ini
   ditampilkan apa adanya dari backend, layar tidak menyaring ulang.
3. Petugas mencentang piutang mana yang disertakan (checkbox per baris atau "pilih semua"), lalu
   mengisi Periode Awal dan Periode Akhir tagihan.
4. Petugas menekan **Simpan Batch Tagihan** → `POST /` membuat batch berstatus **Draf**, layar
   berpindah ke halaman rincian batch yang baru dibuat.

**Langkah normal — Terbitkan dan unduh dokumen:**

5. Pada halaman rincian batch (`/finance/receivable-invoice-batches/{id}`), selama status masih
   **Draf**, tombol **Terbitkan Batch** dan **Batalkan Batch** tersedia. Menekan **Terbitkan
   Batch** memunculkan modal konfirmasi, lalu memanggil `POST /{id}/issue` — status berubah ke
   **Diterbitkan** dan anggotanya terkunci (tombol Terbitkan/Batalkan hilang, keterangan "anggota
   batch ini terkunci" muncul).
6. Petugas menekan **Lihat/Unduh Dokumen** kapan saja untuk membuka halaman dokumen gabungan
   (`/finance/receivable-invoice-batches/{id}/document`), yang memuat header batch (penjamin,
   periode, total tercover) diikuti rincian per-invoice lengkap (identitas pasien, rincian item,
   coverage) memakai ulang tampilan `CompanyGuarantorInvoiceDocument`. Tombol **Unduh PDF**
   menghasilkan satu berkas PDF berisi seluruh invoice anggota, satu invoice per halaman.

**Jalur tidak normal:**

- **Memuat:** kerangka spinner ditampilkan saat daftar/detail/dokumen sedang diambil, bukan layar
  kosong.
- **Kosong:** "Tidak ada piutang yang memenuhi syarat digabung untuk penjamin ini" saat daftar
  layak-gabung kosong; "Data Batch Tagihan AR tidak ditemukan" saat daftar batch kosong.
- **Gagal:** pesan error dari backend ditampilkan apa adanya lewat `InformationAlert`/toast, tanpa
  mengarang pesan baru.
- **Tanpa hak akses:** `AccessDeniedGate` (base component existing) menangani `403` secara
  konsisten dengan seluruh layar Finance lain.
- **Piutang dua penjamin tidak dapat digabung:** dicegah **struktural** oleh desain layar, bukan
  oleh validasi tambahan di klien — daftar piutang yang bisa dicentang pada satu pemuatan hanya
  pernah berasal dari satu `debtorReferenceId` (satu pemanggilan `GET /eligible-receivables` per
  penjamin terpilih), sehingga tidak ada jalur UI untuk mencampur piutang dua penjamin dalam satu
  seleksi. Backend tetap menegakkan `FIN-VAL-115` sebagai lapis pertahanan kedua.
- **Batch bukan Draf:** anggota tampil terkunci (tabel anggota read-only tanpa kontrol
  tambah/kurang di seluruh status), tombol Terbitkan/Batalkan disembunyikan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` (kartu task
  `FE-FIN-012`) dan `roadmap/00-delivery-roadmap.md` bagian 4
- `docs/module-blueprints/finance-management/03-frontend-architecture.md` §12.2 (Layar AR Invoice
  Agregat), §12.6 (`DEV_DISCRETION`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.7 (7 endpoint),
  `permission-audit-matrix.md` §B.5, `state-transition-matrix.md` §B.7, `validation-matrix.md`
  `FIN-VAL-114`..`117`
- `docs/module-blueprints/finance-management/00-interview-decisions.md` (`FIN-DEC-048`, `054`,
  `060`)
- `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-039.md` — status
  sebenarnya, dampak kontrak, dan daftar berkas backend
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivableInvoiceBatchesController.cs`,
  `Dtos/FinanceReceivableInvoiceBatchDtos.cs` — kontrak request/response **sebenarnya** (dipakai
  sebagai source of truth; ditemukan satu delta dari `api-contract.md`, lihat bagian 8)
  , dibaca, **tidak diubah**
- `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingCompanyGuarantorInvoiceDtos.cs` —
  bentuk `CompanyGuarantorInvoiceDocumentResponse` per invoice
- Frontend — pola rujukan terdekat rumpun `REV-4`: `invoice-exchange-view.jsx`,
  `use-invoice-exchange-list.jsx`, `use-invoice-exchange-editor.jsx`, `use-invoice-exchange-detail.jsx`,
  `invoice-exchange-table-columns.jsx`, `invoice-exchange-detail-view.jsx`,
  `purchasing-invoice-detail-view.jsx`, `supplier-return-form-view.jsx` (pola tabel item mentah +
  checkbox), `finance-receivable-view.jsx`, `use-finance-receivable.jsx`,
  `receivable-utils.jsx`, `use-supplier-name-resolver.jsx`,
  `use-administrator-select.jsx`/`administrator-select-resources.js` (resource
  `administratorCompanyGuarantors`, endpoint detail `GET /v1/administrator/master-data/company-guarantors/{id}`),
  `company-guarantor-invoice-document.jsx` dan `use-dokumen-kasir.js` (pola unduh PDF `html2pdf.js`)
  — seluruhnya dibaca, **tidak diubah**

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx` | **Baru.** Endpoint base, status lifecycle (`DRAFT`/`ISSUED`/`PARTIALLY_PAID`/`PAID`/`CANCELLED`), label/tone, opsi filter |
| `src/utils/finance/receivable/receivable-utils.jsx` | Tambah `getApiErrorMessage` (util domain receivable sudah ada, hanya menambah satu fungsi, mengikuti konvensi "setiap domain Finance menyimpan formatter sendiri") |
| `src/lib/hooks/finance/receivable/use-company-guarantor-name-resolver.jsx` | **Baru.** Resolve nama penjamin per Id, pola identik `use-supplier-name-resolver.jsx` |
| `src/lib/hooks/finance/receivable/use-receivable-invoice-batch-list.jsx` | **Baru.** Hook Daftar Batch — `GET /` berpaging, filter penjamin/status |
| `src/lib/hooks/finance/receivable/use-receivable-invoice-batch-editor.jsx` | **Baru.** Hook Buat Batch — muat `GET /eligible-receivables` saat penjamin berganti, seleksi checkbox, `POST /` |
| `src/lib/hooks/finance/receivable/use-receivable-invoice-batch-detail.jsx` | **Baru.** Hook Detail — `GET /{id}`, aksi `POST /{id}/issue` dan `POST /{id}/cancel` dengan `ExpectedRowVersion` |
| `src/lib/hooks/finance/receivable/use-receivable-invoice-batch-document.jsx` | **Baru.** Hook Dokumen — `GET /{id}/document` |
| `src/components/view/finance/receivable/invoice-batch/receivable-invoice-batch-table-columns.jsx` | **Baru.** Kolom Daftar Batch |
| `src/components/view/finance/receivable/invoice-batch/receivable-invoice-batch-view.jsx` | **Baru.** Layar Daftar Batch |
| `src/components/view/finance/receivable/invoice-batch/receivable-invoice-batch-form-view.jsx` | **Baru.** Layar Buat Batch (pilih penjamin, tabel checkbox piutang layak-gabung, periode) |
| `src/components/view/finance/receivable/invoice-batch/detail/receivable-invoice-batch-detail-view.jsx` | **Baru.** Layar Detail Batch (rincian anggota, aksi Terbitkan/Batalkan, tautan dokumen) |
| `src/components/view/finance/receivable/invoice-batch/document/receivable-invoice-batch-document-view.jsx` | **Baru.** Layar Dokumen Gabungan (header batch + rincian per-invoice pakai ulang `CompanyGuarantorInvoiceDocument`, unduh PDF `html2pdf.js`) |
| `src/app/finance/receivable-invoice-batches/page.jsx` | **Baru.** Route Daftar Batch |
| `src/app/finance/receivable-invoice-batches/new/page.jsx` | **Baru.** Route Buat Batch |
| `src/app/finance/receivable-invoice-batches/[slug]/page.jsx` | **Baru.** Route Detail Batch |
| `src/app/finance/receivable-invoice-batches/[slug]/document/page.jsx` | **Baru.** Route Dokumen Batch |

Menu sidebar (`src/utils/finance/menu-sidebar/corporateFinance.js`) **sengaja tidak disentuh** —
`FIN-DEC-060` menegaskan butir menu "Tagihan Gabungan Penjamin" **MUST NOT** didaftarkan sebelum
`FE-FIN-014` (pendaftaran menu adalah scope task itu, bukan task ini); layar boleh dibangun lebih
dulu tanpa menu.

### 3.3 Kepatuhan arsitektur frontend

Seluruh hook mengikuti pola state lokal (`useState`/`useCallback`, tanpa Redux slice) yang sudah
dipakai rumpun Purchasing sebelahan (`FE-FIN-008`/`009`/`010`/`011`, revisi yang sama), **bukan**
pola Redux slice yang dipakai domain Finance yang lebih lama (`receivable`, `master-data`,
`cash-management`) — dipilih karena ini pola yang lebih baru pada domain `finance-management`
untuk kebutuhan sejenis (aggregate ber-lifecycle dengan aksi POST /{id}/<aksi>), bukan preferensi
pribadi. Route baru mengikuti struktur `src/app/finance/<slug>` flat yang sudah ada (sejajar
`/finance/receivable`, `/finance/purchasing`), sesuai `03-frontend-architecture.md` §12.5 (rute di
bawah `/finance/...`, tanpa group menu baru). Base component dipakai ulang penuh — lihat tabel
keputusan base component pada bagian 4.

**Tabel keputusan base component (gerbang langkah 3 skill):**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Layout halaman, Hero, breadcrumb, filter, tabel daftar, pagination, toast, badge status, modal konfirmasi, resource select | `REUSE` | Identik pola `invoice-exchange-view.jsx`/`finance-receivable-view.jsx`/`purchasing-invoice-detail-view.jsx` |
| Tabel pilih piutang layak-gabung (checkbox multi-select) | `COMPOSE` — tabel bootstrap mentah + `<input type="checkbox">`, bukan `DataTable` | Opsi 1 (**dipilih**): ikuti idiom `supplier-return-form-view.jsx` — nol risiko regresi. Opsi 2 (ditolak): tambah prop `selectable` ke `DataTable` (`EXTEND` yang mengubah base component bersama, dipakai puluhan halaman lain) — tidak diperlukan untuk kebutuhan satu layar |
| Rincian per-invoice pada dokumen gabungan | `REUSE` penuh `CompanyGuarantorInvoiceDocument` (`FE-BKC-031`, modul Billing) | Presentasional murni, bentuknya identik `CompanyGuarantorInvoiceDocumentResponse` — ditemukan lewat pemeriksaan berkas sebelum menulis komponen baru |
| Unduh PDF dokumen gabungan | `COMPOSE` — `html2pdf.js` dari ref DOM | Opsi 1 (**dipilih**): pola identik `use-dokumen-kasir.js` (`downloadCompanyGuarantorInvoice`) — menghasilkan Blob yang bisa diunduh. Opsi 2 (ditolak): `react-to-print` (dipakai modul Resep) — hanya membuka dialog print, bukan Blob file, dan bukan pola yang dipakai domain dokumen tagihan penjamin ini |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Spinner + teks "Mengambil data Batch Tagihan AR...", "Memuat detail Batch Tagihan AR...", "Memuat piutang yang memenuhi syarat...", atau "Memuat dokumen tagihan gabungan..." sesuai layar |
| Kosong | "Data Batch Tagihan AR tidak ditemukan." (daftar); "Tidak ada piutang yang memenuhi syarat digabung untuk penjamin ini." (form); "Batch ini belum memiliki invoice anggota." (dokumen) |
| Gagal | Pesan error backend apa adanya lewat `InformationAlert`/toast; opsi reset filter tetap tersedia |
| Tanpa hak akses | `AccessDeniedGate` menampilkan pesan akses ditolak standar, konsisten seluruh layar Finance |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Receivable Invoice Batch

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receivable-invoice-batches` | Daftar batch, disaring penjamin/status | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/v1/corporate/finance-management/receivable-invoice-batches/eligible-receivables` | Daftar piutang layak digabung untuk satu penjamin (layar Buat Batch) | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/v1/corporate/finance-management/receivable-invoice-batches/{id}` | Rincian batch beserta anggota | `FinanceReceivableInvoiceBatch : Read` |
| `GET` | `/v1/corporate/finance-management/receivable-invoice-batches/{id}/document` | Dokumen tagihan gabungan | `FinanceReceivableInvoiceBatch : Read` |
| `POST` | `/v1/corporate/finance-management/receivable-invoice-batches` | Membuat batch `DRAFT` dari piutang terpilih | `FinanceReceivableInvoiceBatch : Create` |
| `POST` | `/v1/corporate/finance-management/receivable-invoice-batches/{id}/issue` | Menerbitkan batch | `FinanceReceivableInvoiceBatch : Issue` |
| `POST` | `/v1/corporate/finance-management/receivable-invoice-batches/{id}/cancel` | Membatalkan batch `DRAFT` | `FinanceReceivableInvoiceBatch : Update` |

Endpoint tambahan yang **dibaca ulang, tidak diubah**, dipakai resolver nama penjamin:
`GET /v1/administrator/master-data/company-guarantors/{id}` (endpoint detail master data
Administrator yang sudah ada).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error | `PASS` | Keluaran perintah: `eslint . --quiet` selesai tanpa output error |
| `npm run build` | Seluruh route (termasuk 4 route baru `/finance/receivable-invoice-batches`, `/new`, `/[slug]`, `/[slug]/document`) berhasil di-compile | `PASS` | Keluaran build mencantumkan keempat route baru tanpa `Failed to compile` |
| Campur piutang dua penjamin dalam satu seleksi | Tidak dapat dilakukan — daftar layak-gabung hanya pernah berasal dari satu `debtorReferenceId` per pemuatan | `PASS` (review kode/desain) | `use-receivable-invoice-batch-editor.jsx` — `eligibleReceivables` dimuat ulang dan dikosongkan setiap `debtorReferenceId` berganti |
| Batch bukan `DRAFT` menyembunyikan aksi Terbitkan/Batalkan | Tombol tidak dirender, keterangan "anggota batch ini terkunci" tampil | `PASS` (review kode) | `receivable-invoice-batch-detail-view.jsx` — `canIssue`/`canCancel` dari `batch.status === DRAFT` |
| Alur penuh: buat batch → muat piutang layak-gabung → pilih → simpan → terbitkan → lihat dokumen → unduh PDF | — | `NOT FEASIBLE` | Memerlukan backend berjalan (`dotnet run`) dan migrasi `BE-FIN-038` (`AddArInvoiceBatchAndReceiptDeduction`) sudah dieksekusi ke database — laporan `BE-FIN-039.md` §7 mencatat migrasi ini **belum** dieksekusi ke lingkungan mana pun |

Uji manual: `NOT FEASIBLE` — tidak ada database dengan skema `FinReceivableInvoiceBatch(Item)`
aktif yang dapat diuji terhadapnya, dan tidak ada instance backend berjalan yang dikonfirmasi pada
sesi ini.

**AUTOMATED TEST:** `NOT APPLICABLE — repository ini tidak memelihara Jest/test runner otomatis (rules/frontend/test-policy.md); pembuatan test baru bersifat opsional dan tidak diminta eksplisit pada task ini.`

**Tidak dijalankan:** `npm run test:e2e` dan `npm run test:uat` — tidak diminta task ini dan
memerlukan environment yang mendukungnya.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Daftar piutang yang layak digabung diambil dari `GET /eligible-receivables`, bukan disaring layar | Terpenuhi | `use-receivable-invoice-batch-editor.jsx` — hasil response dipakai apa adanya, nol filter tambahan di klien |
| Batch `ISSUED` tampil terkunci | Terpenuhi | `receivable-invoice-batch-detail-view.jsx` — nol kontrol ubah anggota di seluruh status; aksi Terbitkan/Batalkan hanya muncul saat `DRAFT` |
| `npm run lint:errors` | Terpenuhi | `PASS`, lihat bagian 6 |
| `npm run build` | Terpenuhi | `PASS`, lihat bagian 6 |
| Verifikasi manual: piutang dua penjamin tidak dapat digabung | **Belum terpenuhi lewat uji runtime** — terpenuhi lewat review desain/kode (structural guarantee) | Lihat bagian 6; uji runtime `NOT FEASIBLE` karena backend/database belum tersedia untuk diuji |
| DoD: Nol PPN/Faktur Pajak ditampilkan (`FIN-DEC-054`) | Terpenuhi | Nol field PPN/Faktur Pajak pada seluruh layar batch; layar dokumen menampilkan field `taxAmount` hanya pada rincian **item per-invoice** dari `CompanyGuarantorInvoiceDocument` (komponen existing modul Billing, di luar cakupan tulis task ini), bukan pada level batch |

Task ini **belum** dapat ditandai `✅` — kriteria verifikasi manual/runtime belum terbukti lewat
eksekusi sungguhan, hanya lewat review kode/desain.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Konflik status dependency ditemukan sebelum implementasi dimulai:** `00-delivery-roadmap.md` menandai `BE-FIN-039` ✅ dengan klaim "`dotnet build` PASS 0 error", tetapi laporan tracked `BE-FIN-039.md` menyatakan 🟡 SEBAGIAN, `dotnet build` sengaja `NOT RUN`, dan migrasi `BE-FIN-038` belum dieksekusi ke database. Saya menahan task ini dan menanyakan ke pemilik repository sebelum melanjutkan; pemilik mengonfirmasi telah menjalankan `dotnet build` sendiri dan berhasil, lalu mengarahkan saya melanjutkan FE-FIN-012 dari kontrak yang sudah terkunci. Konfirmasi build ini **tidak** mencakup eksekusi migrasi ke database maupun uji runtime — laporan `BE-FIN-039.md` itu sendiri **tidak** saya perbarui atas nama pemilik backend, karena wewenang laporan lintas repository task ini sempit hanya untuk `FE-FIN-012.md` |
| Masalah yang diketahui | (1) Delta kontrak: `api-contract.md` §B.7 menuliskan response `GET /eligible-receivables` sebagai `PagedResult<EligibleReceivableResponse>`, tetapi controller backend sebenarnya (`FinanceReceivableInvoiceBatchesController.GetEligibleReceivables`) mengembalikan `List<EligibleReceivableResponse>` polos tanpa paging — frontend dibangun mengikuti **source backend sebenarnya** (List, bukan PagedResult), sesuai aturan "source backend adalah bukti otoritatif atas perilaku runtime". Dicatat di sini sebagai delta kontrak yang perlu ditinjau ulang pemilik kontrak, bukan diperbaiki sepihak. (2) `Idempotency-Key` disebut kontrak pada `POST /`, `POST /{id}/issue`, `POST /{id}/cancel`, tetapi implementasi controller backend saat ini tidak memeriksa header itu sama sekali — konsisten dengan gap yang sama yang sudah dicatat `FE-FIN-008` untuk rumpun Purchasing, bukan regresi baru task ini; frontend tidak mengirim header itu karena tidak ada mekanisme idempotency-key existing yang dipakai ulang di rumpun `receivable` |
| Dependency backend | `BE-FIN-039` — source lengkap dan dikonfirmasi `dotnet build` PASS oleh pemilik repository, tetapi migrasi skema `BE-FIN-038` belum dieksekusi ke database mana pun (per `BE-FIN-039.md` §7) sehingga endpoint belum bisa diuji hidup. Dampaknya di layar: seluruh 7 endpoint pada task ini belum pernah dipanggil terhadap backend yang benar-benar berjalan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` (repository frontend) pada akhir pekerjaan mencakup task ini: file baru `src/app/finance/receivable-invoice-batches/**`, `src/components/view/finance/receivable/invoice-batch/**`, `src/lib/hooks/finance/receivable/use-receivable-invoice-batch-*.jsx`, `src/lib/hooks/finance/receivable/use-company-guarantor-name-resolver.jsx`, `src/lib/constants/finance/receivable/receivable-invoice-batch-constants.jsx`; file berubah `src/utils/finance/receivable/receivable-utils.jsx`. Sejumlah besar perubahan milik task frontend lain (`FE-FIN-008`/`009`/`010`, rumpun Purchasing) juga terlihat pada `git status` — tidak disentuh, tidak dilaporkan di sini karena bukan bagian task ini |
| Langkah berikutnya | (1) Pemilik repository backend mengeksekusi migrasi `AddArInvoiceBatchAndReceiptDeduction` (`BE-FIN-038`) ke lingkungan pengembangan; (2) jalankan backend (`dotnet run`) dan uji ketujuh endpoint hidup terhadap layar ini, termasuk skenario `FIN-VAL-114`..`117`; (3) setelah runtime terbukti, perbarui laporan ini dan tanda status roadmap dari 🟡 ke ✅; (4) selaraskan `api-contract.md` §B.7 dengan bentuk response `GetEligibleReceivables` yang sebenarnya (List, bukan PagedResult) |
