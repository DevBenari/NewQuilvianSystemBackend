# Laporan Perubahan Frontend — `FE-ACC-P2-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-008` |
| Judul | Layar Rekonsiliasi Control Account |
| Slice | `P2-RECON` |
| Roadmap | `docs/module-blueprints/accounting/roadmap/frontend-roadmap-phase2.md` — kartu `FE-ACC-P2-008` dan peta butir menu |
| Trace | `ACC-DEC-066`, `ACC-DEC-064`, `ACC-DEC-071`; `FR-P2-039`, `FR-P2-040` |
| Contract version | Grup Reconciliation **usulan `ACC-API-0.11`** dan hak akses **usulan `ACC-PERMISSION-0.6`** — belum diratifikasi. Sumber kebenaran as-is: `ReconciliationController` dan `ControlAccountReconciliationDtos.cs` pada backend `rizkiG` `cca0957`; bentuk responsnya **terbukti di runtime** (bagian 6) |
| Wewenang UI | Butir menu tingkat 2 `/accounting/reconciliation` sesuai kartu dan peta butir menu roadmap; pemakaian ulang tabel dan format rupiah Neraca Saldo. Bukan bagian halaman Buku Besar |
| Dependency | `BE-ACC-P2-013` 🟡 — endpoint berdiri dan terbukti menjawab di runtime; yang belum hanya uji PostgreSQL sisi backend. `BE-ACC-P2-014` belum dikerjakan — **`DEFERRED BACKEND CAPABILITY`**, menunggu gelombang `P2-1` |
| Klasifikasi | `MEDIUM` — satu route baru, satu slice, satu hook, satu view, satu CSS module; nol base component baru; nol perubahan backend |
| Task mode | `CROSS-REPO` — source di frontend `RizkiV2`; laporan, roadmap, dan traceability di backend `rizkiG`; source backend **read-only** |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/**`; laporan ini serta tanda status dan tautan bukti pada roadmap dan traceability |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `460f717a0` (branch `RizkiV2`) ditambah perubahan `FE-ACC-P2-003`/`004`/`007` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `cca0957` (branch `rizkiG`) |
| Tanggal | 11 September 2026 |
| Status | **✅ `SELESAI` sisi development** — `IMPLEMENTATION COMPLETE` · developer verification `PASS` · `READY FOR UAT`. UAT **belum** dijalankan |

```text
IMPLEMENTATION STATUS          : IMPLEMENTATION COMPLETE
DEVELOPER VERIFICATION STATUS  : PASS — lint 0 error, 677 uji unit lulus, build compiled,
                                 bentuk respons backend terbukti cocok di runtime
UAT STATUS                     : READY FOR UAT — belum diuji tim UAT (bukan UAT PASS)
```

---

## 1. Keadaan yang ditemukan di awal

| Yang diperiksa | Hasil |
| --- | --- |
| Frontend | Belum ada route, menu, slice, maupun layar rekonsiliasi |
| Endpoint sisi buku besar | **Ada.** `GET /api/v1/corporate/accounting/reconciliation/gl-balances`, dijaga `[AccessPermission("AccountingReconciliation", "Read")]`, `BE-ACC-P2-013` |
| Endpoint sisi subledger | **Tidak ada**, begitu pula bidangnya. `ControlAccountBalanceResponse` hanya membawa sisi buku besar. Sisi subledger adalah `BE-ACC-P2-014`, yang menunggu gelombang `P2-1` (`ACC-DEC-071`) |
| Selisih | **Tidak ada** di backend — tidak mungkin ada tanpa saldo subledger |
| Metadata control account | Ada. Backend hanya mengembalikan akun ber-`IsControlAccount = true`; `/options` juga membawa penandanya (`BE-ACC-P2-012`) |
| Pemilih akun Buku Besar | **Tidak** menyaring akun control — `use-general-ledger.jsx` memetakan seluruh `/options` apa adanya. Tidak diubah |
| Data dev | 6 control account pada badan hukum utama: Kas Kasir, Kas Kecil, dua Piutang, dua Utang |

### Selisih antara dokumen dan kode

| Selisih | Yang dipakai |
| --- | --- |
| Kartu menulis route `/accounting/reconciliation`; seluruh layar Accounting memakai awalan `/corporate/accounting/...` (contoh `/accounting/recurring-journals` → `/corporate/accounting/recurring-journals`) | `/corporate/accounting/reconciliation`, mengikuti konvensi yang berlaku |
| Komentar `ReconciliationController` dan `AccControlAccountReconciliationService` masih menulis `BE-ACC-P2-014` "terblokir `DEC-ACC-P2-011`" | Keputusan itu **sudah ditutup** `ACC-DEC-071` (10 Sep 2026). Komentar backend usang; **tidak diubah** karena backend read-only pada task ini |
| Grup Reconciliation belum diratifikasi (`ACC-API-0.11` usulan) | Kode backend, sesuai catatan kontrak sendiri: *"untuk baris `gl-balances` yang benar adalah kode"* |
| Kartu tidak punya baris Verifikasi | DoD kartu (lint dan build) ditambah validasi minimum `AGENTS.md` frontend (lint, unit test, build) |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** staf atau kepala akuntansi yang memeriksa apakah saldo akun kontrol — Kas Kasir,
Kas Kecil, Piutang, Utang — cocok dengan catatan rincinya. Lazimnya dibuka menjelang penutupan
bulan.

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Pengguna | Membuka menu **Akuntansi › Rekonsiliasi Control Account** |
| 2 | Layar | Menampilkan spanduk kuning **"Rekonsiliasi belum lengkap."** di atas segalanya: saldo buku besar sudah ada, saldo subledger belum diterima dari modul Keuangan |
| 3 | Layar | Memakai badan hukum yang terakhir dipilih di modul Akuntansi, lalu langsung memanggil backend |
| 4 | Layar | Menampilkan kartu keterangan (kapan dihitung, saldo per tanggal, sumber tiap sisi), ringkasan, dan tabel seluruh control account |
| 5 | Pengguna | Opsional: memilih **Saldo per tanggal** — saldo dihitung sampai tanggal itu, **termasuk** tanggalnya |
| 6 | Pengguna | Menekan **Muat Ulang** kapan saja untuk menghitung ulang; angka tidak pernah diambil dari simpanan |

### 2.2 Contoh berangka — data dev 11 September 2026

| Kode | Akun | Control | Baris Disahkan | Saldo Buku Besar | Saldo Subledger | Selisih | Status |
| --- | --- | --- | ---: | ---: | --- | --- | --- |
| 1-1002 | Kas Kasir | Control | 0 | Rp 0 | *Belum tersedia* | *Belum tersedia* | Menunggu saldo subledger |
| 1-1003 | Kas Kecil | Control | 1 | Rp 1.000.000 | *Belum tersedia* | *Belum tersedia* | Menunggu saldo subledger |
| 2-1001 | Utang Pemasok | Control | 0 | Rp 0 | *Belum tersedia* | *Belum tersedia* | Menunggu saldo subledger |

Dua hal yang sengaja **dibedakan** pada tabel ini:

- **Rp 0 pada Saldo Buku Besar adalah angka sungguhan.** Kas Kasir memang belum pernah menerima
  jurnal yang disahkan, dan nol itu informasi rekonsiliasi yang sah.
- **"Belum tersedia" bukan nol.** Ditulis miring dan redup, tanpa awalan Rp, supaya tidak pernah
  terbaca sebagai angka. Selisih **tidak dihitung** selama salah satu sisinya belum ada. Kalau
  dihitung dengan subledger yang dianggap nol, Kas Kecil akan tampil "selisih Rp 1.000.000", dan
  Kas Kasir akan tampil "cocok" padahal belum ada yang membandingkannya.

### 2.3 Saldo menurut saldo normal

Kolom Saldo Buku Besar memakai **`BalanceInNormalBalance`**, bukan `Balance`, sesuai kartu
roadmap. Akun Utang yang sehat — misalnya menerima kredit Rp 4.000.000 — tampil **Rp 4.000.000**,
bukan −Rp 4.000.000. Angka **merah** berarti saldonya berlawanan dengan saldo normal akun dan
perlu ditelusuri; keterangan itu tertulis di kartu atas dan pada `title` sel angkanya.

### 2.4 Jalur tidak normal

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Badan hukum belum dipilih | Tabel: "Badan hukum belum dipilih." Permintaan tidak dikirim |
| Belum ada akun bertanda control account | Tabel: "Belum ada akun yang ditandai sebagai control account." dengan keterangan cara menandainya lewat COA, dan penegasan **"Daftar kosong di sini bukan berarti tidak ada selisih."** |
| Tanpa hak `AccountingReconciliation : Read` | Seluruh layar diganti `AccessDeniedGate` ("Ups! Akses Ditolak") |
| `409` penjaga badan hukum utama | Kotak kuning berisi pesan backend apa adanya |
| `400` | Kotak merah berisi pesan backend apa adanya — mis. "Badan hukum wajib disebutkan." |
| `404` | "Layanan rekonsiliasi control account belum tersedia pada server ini." |
| `5xx` | "Server gagal menghitung saldo control account. Tekan Muat Ulang; …" — isi respons server **tidak** ditampilkan |
| Server tidak terjangkau | "Server tidak dapat dihubungi. Periksa koneksi, lalu tekan Muat Ulang." |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; suite skill `rules/frontend/frontend-architecture.md`,
`base-component-decision-gate.md`, `ui-consistency-checklist.md`, `design-tokens.md`,
`test-policy.md`, `REPORT_TEMPLATE.md`, `rule-output/status-task-roadmap.md`. Backend:
`ReconciliationController.cs`, `AccControlAccountReconciliationService.cs`,
`ControlAccountReconciliationDtos.cs`, `AccountingLegalEntityGuard.cs`, `AuthController.cs` (untuk
uji kontrak). Dokumen: roadmap frontend dan backend Phase 2, traceability Phase 2,
`api-contract.md` grup Reconciliation, laporan `BE-ACC-P2-013` dan `FE-ACC-P2-007`. Frontend:
layar Neraca Saldo (view, hook, konstanta, slice, CSS), Buku Besar, Tutup Tahun, pemilih badan
hukum, `use-permission.jsx`, `access-denied-gate.jsx`, `access-denied-utils.jsx`, `summary-grid.jsx`,
`status-badge.jsx`, `information-alert.jsx`, `data-filter.jsx`, `filter-date-picker.jsx`,
`InstanceAxios.jsx`, `menu-items.jsx`, `store.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/corporate/accounting/reconciliation/page.jsx` (baru) | Route tipis + metadata |
| `src/app/corporate/accounting/reconciliation/control-account-reconciliation-client.jsx` (baru) | Client wrapper |
| `src/components/view/corporate/accounting/reconciliation/control-account-reconciliation-view.jsx` (baru) | Layar |
| `src/lib/hooks/corporate/accounting/reconciliation/use-control-account-reconciliation.jsx` (baru) | Hak akses, permintaan tanpa cache, sebab kosong, galat |
| `src/lib/state/slice/corporate/accounting/accounting-reconciliation-slice.jsx` (baru) | Satu thunk `getControlAccountGlBalances`; jawaban permintaan lama diabaikan lewat `requestId` |
| `src/lib/constants/corporate/accounting/reconciliation/control-account-reconciliation-constants.jsx` (baru) | Konfigurasi, kolom, status, salinan teks kosong dan galat |
| `src/utils/corporate/accounting/reconciliation/control-account-reconciliation-utils.jsx` (baru) | Normalizer, `computeReconciliationDifference`, `resolveReconciliationStatus`, penyusun query, pemilih pesan galat |
| `src/style/corporate/accounting/control-account-reconciliation-view.module.css` (baru) | Kartu konteks, kartu meta, sel angka dan sel "Belum tersedia" — hanya token |
| `src/lib/state/store.jsx` | Reducer `accountingReconciliation` |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir "Rekonsiliasi Control Account" sesudah Neraca Saldo |
| `tests/unit/accounting-reconciliation.test.mjs` (baru) | 22 uji |
| `tests/fixtures/reconciliation-gl-balances-2026-09.json` (baru) | Payload asli `gl-balances` dari backend dev, 11 Sep 2026 |

Sisa `FE-ACC-P2-007` — dialog penyesuaian dan Form Jurnal Berulang — dikerjakan pada sesi yang sama
dan dicatat di laporan [`fe-acc-p2-007`](fe-acc-p2-007-penanda-control-account-layar-coa.md)
bagian 9.

### 3.3 Kepatuhan arsitektur frontend

Alurnya `page → client → view → hook → slice → InstanceAxios`. View tidak memanggil Axios dan tidak
menormalisasi respons; normalisasi dan seluruh keputusan "tersedia atau belum" ada di `utils`
sebagai fungsi murni yang diuji sendiri. Endpoint, kolom, dan salinan teks ada di `constants`.
Slice ditulis manual dengan alasan yang sama seperti `accounting-general-ledger-slice.jsx`: satu
pembacaan, nol mutasi. Pemilih badan hukum, `usePermission`, dan `AccessDeniedGate` dipakai ulang;
nol pola baru.

**Keputusan yang perlu dijelaskan — layar tidak membaca bidang subledger apa pun.** Menebak nama
bidang yang belum ada (`subledgerBalance`, `difference`) sama dengan mengarang kontrak. Normalizer
menetapkan `subledgerBalance = null` secara tegas, dan selisih serta status diturunkan dari situ.
Saat `BE-ACC-P2-014` berdiri, hanya satu baris di `normalizeControlAccountBalanceRow` yang perlu
diubah; kolom, spanduk, ringkasan, dan status akan menyala sendiri.

### 3.4 Gerbang keputusan base component

`UI GATE: 11 elemen — REUSE 10, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero`, dipakai Neraca Saldo | REUSE | `eyebrow`/`title`/`description` dari config |
| Spanduk rekonsiliasi belum lengkap | `InformationAlert` | `variant="warning"`, dipakai Tutup Tahun | REUSE | Di atas tabel, bukan catatan kaki |
| Pemilih badan hukum | `AccountingLegalEntitySelect` | `view/corporate/accounting/shared/`, dipakai seluruh layar Accounting | REUSE | Apa adanya |
| Keterangan hanya-disahkan dan saldo normal | Kartu konteks Neraca Saldo | `.contextCard` + `.postedOnlyNotice` di `trial-balance-view.module.css` | REUSE | Struktur disalin ke CSS module layar ini |
| Kartu keterangan perhitungan | `.metaCard` Tutup Tahun | `year-end-closing-view.module.css` | COMPOSE | Lihat pilihan di bawah |
| Ringkasan | `SummaryGrid` | `items[].title/value`, dipakai Neraca Saldo | REUSE | Nilai string "Belum tersedia" dirender apa adanya |
| Filter saldo per tanggal | `DataFilter` + `FilterDatePicker` | pola Buku Besar | REUSE | Tombol atur ulang mengosongkan tanggal |
| Tombol Muat Ulang | `BaseButton` lewat `actions` milik `DataFilter` | `normalizeActions` menerima array elemen | REUSE | `variant="secondary"`, `loading` |
| Tabel | `DataTable` | `pagination={false}`, `sortLatestFirst={false}` seperti Neraca Saldo/Tutup Tahun | REUSE | Urutan kode akun dari backend dipertahankan |
| Penanda control dan status | `StatusBadge` + `CONTROL_ACCOUNT_BADGE` | badge yang sama dengan tabel COA (`FE-ACC-P2-007`) | REUSE | Nada `info` untuk "Menunggu saldo subledger" |
| Galat dan akses ditolak | `InformationAlert`, `AccessDeniedGate` | pola Tutup Tahun | REUSE | `401`/`403` ke gerbang, lainnya ke kotak pesan |

**Keputusan: kartu keterangan perhitungan (COMPOSE)**

- **A. Markup baris label-nilai + `.metaCard` salinan Tutup Tahun — Rekomendasi, dipakai.** Tampil
  sama dengan kartu meta layar Tutup Tahun; nol base component baru; tanpa risiko regresi karena
  hanya CSS module layar ini.
- **B. `SummaryGrid` kedua.** Menampilkan tanggal dan kalimat sumber sebagai "nilai summary"
  berukuran besar, dan label dilewatkan `toIndonesianLabel` yang dapat mengubah kata. Tidak cocok
  untuk teks keterangan.
- **C. `BaseDetailView`.** Dirancang untuk halaman rincian master data lengkap dengan aksi; terlalu
  berat untuk empat baris keterangan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka `SummaryGrid` dan "Menghitung saldo control account..." pada tabel; tombol Muat Ulang berputar; filter tanggal dimatikan |
| Berhasil | Kartu keterangan, ringkasan (jumlah akun, total debit, total kredit, Saldo Subledger "Belum tersedia", Selisih "Belum tersedia"), dan tabel |
| Kosong | Tiga sebab berbeda — lihat 2.4 |
| Gagal | `InformationAlert` bernada sesuai status, dengan tindakan pemulihan Muat Ulang; laporan lama dibuang |
| Tanpa hak akses | `AccessDeniedGate`. Bila daftar hak sudah dimuat dan hak baca tidak ada, permintaan **tidak dikirim**; bila belum dimuat, backend yang menolak `403` |
| Sisi subledger belum tersedia | Spanduk kuning, sel miring "Belum tersedia", status "Menunggu saldo subledger" |
| Responsif | Kartu dan ringkasan memakai grid `auto-fit`; tabel sembilan kolom digulir mendatar di dalam `DataTable`; padding kartu mengecil di bawah 576px |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Accounting / Reconciliation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/reconciliation/gl-balances` | Seluruh isi layar | `AccountingReconciliation : Read` |

Pemilih badan hukum memakai ulang resource select `legalEntities` yang sudah ada; bukan endpoint
baru.

**Parameter query** — `ControlAccountBalanceQuery`:

| Parameter | Wajib | Sumber di layar |
| --- | --- | --- |
| `legalEntityId` | Ya | Pemilih badan hukum; permintaan ditahan selama kosong |
| `asOfDate` | Tidak | Filter "Saldo per tanggal", format `YYYY-MM-DD`; kosong berarti seluruh riwayat |

**Bidang respons yang dipakai** — `ApiResponse<ControlAccountBalanceReportResponse>`:

| Bidang | Dipakai untuk |
| --- | --- |
| `data.evaluatedAt` | "Dihitung pada" |
| `data.asOfDate` | "Saldo per" |
| `data.accountCount`, `data.totalDebit`, `data.totalCredit` | Ringkasan — tidak dijumlahkan layar |
| `accounts[].accountId` | Kunci baris |
| `accounts[].accountCode`, `accountName` | Kolom Kode Akun dan Nama Akun |
| `accounts[].accountType`, `normalBalance` | Kolom "Jenis / Saldo Normal", lewat label COA |
| `accounts[].balanceInNormalBalance` | **Kolom Saldo Buku Besar** |
| `accounts[].postedLineCount` | Kolom Baris Disahkan |
| `accounts[].balance`, `totalDebit`, `totalCredit` | Dinormalisasi, tidak ditampilkan per baris |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 18 berkas yang disentuh `FE-ACC-P2-007` dan `008` | `0 errors`, 2 warning | `PASS` / `EXISTING WARNING` | Keduanya `react-hooks/set-state-in-effect` pada efek yang sudah ada sebelum task ini (`setJournalId`, `setCorrectionType`); nol warning di berkas `FE-ACC-P2-008` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **677 lulus, 0 gagal** — sebelumnya 651 | `PASS` | +22 uji rekonsiliasi, +4 uji control account |
| `npm run build` | `✓ Compiled successfully in 45s`, exit 0 | `PASS` | `○ /corporate/accounting/reconciliation` terdaftar |
| Grep anti-regresi pada berkas baru | Nol warna literal, nol `!important`, nol `<button>`/`<table>` mentah, nol `style=` | `PASS` | `font-size`/`line-height` hanya pada `.notice` dan `.metaLabel` milik layar ini sendiri, memakai token; `fs-4` di menu adalah konvensi ikon sidebar yang sudah dipakai seluruh butir |
| Uji kontrak runtime — login SuperAdmin seed, `GET gl-balances` | `200`; amplop `data/errors/message/statusCode/success/timestamp`; kunci baris **persis** sama dengan yang dibaca normalizer; 6 control account, Kas Kecil Rp 1.000.000 dari 1 baris | `PASS` | Payload disimpan sebagai fixture; uji `payload asli gl-balances cocok dengan normalizer` |
| `GET gl-balances?asOfDate=2000-01-01` | `200`, 6 akun tetap muncul, seluruh saldo 0 | `PASS` | Batas tanggal dan "akun bersaldo nol tetap tampil" |
| `GET gl-balances` tanpa `legalEntityId` | `400` "Badan hukum wajib disebutkan." | `PASS` | Pesan diteruskan apa adanya |
| `GET /chart-of-accounts/options` | `200`; keenam control account `isControlAccount = true` | `PASS` | Control account **tidak** disembunyikan secara global |

Uji kontrak hanya memakai login dan `GET`. Nol `POST`/`PUT`/`DELETE` ke data Accounting, nol SQL.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`

`MANUAL TEST: NOT FEASIBLE` — sesi ini tidak menjalankan peramban. Uji fungsional dan UAT
diserahkan owner kepada tim UAT terpisah; bentuk data yang dirender layar sudah dibuktikan lewat
uji kontrak di atas.

**Tidak dijalankan:** uji peramban (lihat di atas); `npm run test:e2e` (repository tidak punya
`playwright.config`); `npm run test:uat` (tidak diminta).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kolom saldo buku besar tampil segera setelah `BE-ACC-P2-013` berdiri | **Terpenuhi** | Kolom Saldo Buku Besar dari `balanceInNormalBalance`; endpoint terbukti menjawab di runtime; uji fixture |
| (2) Kolom saldo subledger dan selisih ditampilkan sebagai "belum tersedia", bukan disembunyikan | **Terpenuhi** | Kolom `subledgerBalance` dan `difference` ada di config; sel "Belum tersedia"; spanduk; uji `kolom saldo subledger dan selisih ADA` dan `layar tidak pernah mengganti subledger atau selisih yang kosong dengan nol` |
| (3) Tidak memakai cache | **Terpenuhi** | `resetReconciliationState` saat layar ditutup, `clearReconciliation` saat badan hukum berganti, Muat Ulang memanggil ulang, `requestId` membuang jawaban usang; uji `tanpa cache` |
| DoD — lint hijau | Terpenuhi | `0 errors` |
| DoD — build hijau | Terpenuhi | `✓ Compiled successfully` |
| DoD — laporan task tertulis | Terpenuhi | Berkas ini |
| UAT | **Belum dijalankan** — `READY FOR UAT` | Diserahkan ke tim UAT atas keputusan owner 11 September 2026; **bukan** `UAT PASS` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kontrak grup Reconciliation dan hak aksesnya masih **usulan** (`ACC-API-0.11`, `ACC-PERMISSION-0.6`). Bila ratifikasi mengubah nama bidang, hanya normalizer yang perlu disesuaikan — uji fixture akan gagal lebih dahulu |
| Masalah yang diketahui | (1) **Tidak ada tautan per akun ke Buku Besar** — `use-general-ledger.jsx` tidak menerima akun lewat URL, jadi tautan itu menuntut perubahan layar Buku Besar dan diusulkan sebagai task tersendiri. (2) Komentar backend `ReconciliationController`/service menyebut `BE-ACC-P2-014` terblokir `DEC-ACC-P2-011`; usang sejak `ACC-DEC-071`, milik owner backend |
| Dependency backend | **`DEFERRED BACKEND CAPABILITY`** — Saldo Subledger: `DEFERRED`, backend capability belum tersedia (`BE-ACC-P2-014`, gelombang `P2-1`). Selisih: `DEFERRED`, membutuhkan saldo subledger. `BE-ACC-P2-013` 🟡 hanya karena uji PostgreSQL sisi backend; endpoint-nya berjalan |
| Hak akses untuk UAT | Menurut catatan pemeriksaan 11 September 2026, `SysAccessPolicy` berisi **0 pemberian** untuk `AccountingReconciliation`, sehingga hanya SuperAdmin yang dapat membuka layar ini. Pemberiannya lewat layar **Akses Role**, bukan SQL. Keadaan terkini tidak diperiksa ulang pada sesi ini |
| Perubahan sampingan | `NONE` di luar cakupan. Sisa `FE-ACC-P2-007` dikerjakan atas instruksi owner dan dilaporkan di laporannya sendiri |
| Interupsi | (1) Owner menghentikan sesi sebelum satu berkas pun diubah, lalu melanjutkannya; pekerjaan diteruskan dari hasil baca yang sudah terverifikasi, nol pengulangan. (2) Satu perintah `node -e` gagal karena PowerShell mengupas tanda kutip; diganti uji fixture di `tests/unit`, yang juga pola repository |
| Status Git | Frontend — milik task ini: `?? src/app/corporate/accounting/reconciliation/`, `?? src/components/view/corporate/accounting/reconciliation/`, `?? src/lib/constants/corporate/accounting/reconciliation/`, `?? src/lib/hooks/corporate/accounting/reconciliation/`, `?? src/lib/state/slice/corporate/accounting/accounting-reconciliation-slice.jsx`, `?? src/style/corporate/accounting/control-account-reconciliation-view.module.css`, `?? src/utils/corporate/accounting/reconciliation/`, `?? tests/fixtures/reconciliation-gl-balances-2026-09.json`, `?? tests/unit/accounting-reconciliation.test.mjs`, `M src/lib/state/store.jsx`, `M src/utils/menu-sidebar/menu-items.jsx`. Sisanya milik `FE-ACC-P2-003`/`004`/`007` yang belum di-commit. Backend — hanya dokumen: roadmap frontend dan backend, traceability, laporan `BE-ACC-P2-012`, laporan `FE-ACC-P2-007`, dan berkas ini. **Nol commit, stage, push, merge, atau rebase** |
| Langkah berikutnya | Tim UAT menjalankan skenario di bawah; paralel, development berlanjut ke task berikutnya |

### Yang perlu diuji tim UAT

| No | Langkah | Yang diharapkan |
| ---: | --- | --- |
| 1 | Masuk sebagai pengguna berhak `AccountingReconciliation : Read`, buka Akuntansi › Rekonsiliasi Control Account | Spanduk kuning "Rekonsiliasi belum lengkap." di atas; tabel memuat seluruh control account badan hukum terpilih |
| 2 | Periksa kolom Saldo Subledger dan Selisih | Bertuliskan "Belum tersedia" (miring), **tidak pernah Rp 0**; status "Menunggu saldo subledger" |
| 3 | Bandingkan Saldo Buku Besar Kas Kecil dengan layar Buku Besar untuk akun yang sama | Angkanya sama; hanya jurnal Disahkan yang terhitung |
| 4 | Pilih Saldo per tanggal sebelum jurnal pertama, lalu atur ulang | Seluruh saldo Rp 0 dan akun tetap tampil; atur ulang kembali ke seluruh riwayat |
| 5 | Sahkan satu jurnal non-manual ke control account, lalu tekan Muat Ulang | Saldo dan "Dihitung pada" berubah tanpa memuat ulang halaman |
| 6 | Ganti badan hukum | Angka badan hukum lama hilang seketika, lalu diganti angka badan hukum baru |
| 7 | Tinggalkan layar, buka lagi | Angka dihitung ulang; tidak ada angka lama yang sempat tampil |
| 8 | Masuk sebagai pengguna tanpa hak | "Ups! Akses Ditolak" menggantikan seluruh layar |
| 9 | Akun Utang dengan saldo kredit | Tampil positif; saldo berlawanan tampil merah |
| 10 | Layar sempit (≤ 576px) | Kartu menyusun ulang ke bawah; tabel dapat digulir mendatar tanpa merusak halaman |
