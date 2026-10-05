# Laporan Perubahan Frontend — `FE-FIN-029`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-029` |
| Judul | Layar Snapshot Saldo Subledger menampilkan baris sebanyak akun control yang ada, nilai negatif apa adanya, sebab penolakan yang dapat ditindaklanjuti, dan selisih rekap kas harian terhadap posisi terhitung |
| Slice | `REV-14B` — `EPIC FIN-21` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian REV-14B |
| Trace | `FR-FIN-139`, `FR-FIN-142`, `FR-FIN-143`, `FR-FIN-145`; `FIN-DEC-112`, `113`, `125`; `03-frontend-architecture.md` 19.2 nomor 11 |
| Contract version | `FIN-API-1.5` F.6, F.7; `FIN-VAL-1.7` `FIN-VAL-170`, `177`, `178`, `211` |
| Wewenang UI | Frontend Owner. **Mengikat (invariant):** nilai negatif terbaca negatif beserta keterangan berlawanan dengan saldo normal akun, tidak disembunyikan atau dijadikan nol; pesan gagal tertutup memuat kelompok dan segmen beserta tautan ke layar pemetaan; bagian selisih menampilkan **kedua angka** berdampingan. **`DEV_DISCRETION`:** cara menampilkan nilai negatif (warna, ikon), tata letak bagian selisih |
| Dependency | `BE-FIN-068` ✅ (snapshot dirombak), `BE-FIN-067` ✅ (posisi dan selisih) |
| Klasifikasi | `MEDIUM` — penyesuaian layar existing (`FE-FIN-015`) + 2 komponen view-lokal + 1 thunk + 1 utilitas |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend read-only (tidak diubah); laporan dan bukti roadmap ditulis di repository backend |
| Branch | `yasmina` (upstream `origin/yasmina`); penetapan eksplisit pemegang modul belum dikonfirmasi, sama seperti `FE-FIN-027`/`028` |
| Tanggal | 3 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak. `npm run lint:errors`, `npm run test:unit`, `npm run build` **NOT RUN** (standing instruction: dijalankan manual oleh pengguna). Verifikasi manual UI **NOT FEASIBLE** dari sesi ini. **Bukti "layar `FE-FIN-015` tidak regresi untuk periode lama" belum ada** (§7) |

---

## 1. Masalah yang Diperbaiki

Setelah `BE-FIN-068` dan `BE-FIN-067`, kode layar snapshot yang dibuat saat `FE-FIN-015` masih berasumsi **tepat empat akun**:

1. **Asumsi "selalu empat baris" tersebar di lima tempat** — kartu kelengkapan (`Lengkap (4 Akun)`, `N/4 Akun`), kartu agregat, kartu jumlah (`N dari 4 Akun`), label tab (`4 Akun Kontrol`), dan teks keadaan kosong. Setelah pemetaan menghasilkan baris lebih dari empat (kelima kelompok, per segmen), keterangannya salah.
2. **Penghitungan kelengkapan di klien.** Reducer penerbitan memakai `items.length >= 4`, padahal `IsComplete` kini ditentukan backend (apakah setiap pemetaan aktif sudah terbit).
3. **Nilai negatif tidak diberi keterangan.** `BE-FIN-068` mencabut pemotongan ke nol; angka negatif kini sampai ke layar tetapi tanpa penjelasan bahwa nilai itu berlawanan dengan saldo normal akun.
4. **Penolakan gagal tertutup hanya muncul sebagai toast.** Modal penerbitan tidak menampilkan galatnya, dan pesan backend hanya menyebut **satu** kelompok/segmen pertama yang belum terpetakan.
5. **Empat kolom override kode akun** pada modal sudah usang (`FIN-API-1.5` F.7) tetapi masih dikirim dan menyiratkan kode akun dapat ditimpa.
6. **Selisih rekap kas harian terhadap posisi terhitung** (`FIN-DEC-125`) belum punya tempat di layar.
7. **Pemasangan tabel snapshot tidak sesuai API `DataTable`** (cacat bawaan `FE-FIN-015`, ditemukan saat memeriksa tabel yang diubah): kolom memakai `accessor` padahal `DataTable` membaca `key`; kolom "No" memakai argumen kedua `render` sebagai angka padahal itu objek `meta` (hasilnya "[object Object]1"); `getRowId` dan `emptyMessage` bukan prop `DataTable` (yang dibaca `rowKey`, `emptyTitle`, `emptyDescription`), sehingga teks keadaan kosong yang ditulis di view tidak pernah tampil. Diperbaiki **hanya pada tabel snapshot**, karena tanpa itu baris dan keterangan baru tidak tampil dengan benar.

---

## 2. Proses Bisnis dari Sisi Pengguna

1. Petugas Accounting membuka **Monitoring Keuangan → tab Saldo Subledger Bulanan** dan memilih periode.
2. Layar memuat snapshot periode (`GET /subledger-balances/{period}`) **dan** perbandingan kas (`GET .../{period}/variance`).
3. **Tabel snapshot** menampilkan satu baris per pemetaan aktif, berapa pun jumlahnya. Nilai negatif tampil bertanda minus, berwarna, dan disertai keterangan *"Bernilai negatif — berlawanan dengan saldo normal akun ini. Ditampilkan apa adanya."*
4. **Menerbitkan snapshot:** *Kalkulasi & Terbitkan Saldo Subledger* → modal. Bila ditolak karena pemetaan belum lengkap (`FIN-VAL-177`/`178`), modal tetap terbuka dan memuat pesan backend, **daftar lengkap** kelompok/segmen yang belum terpetakan (dari endpoint cakupan), dan tautan *Buka Pemetaan Akun Control →*. Setelah modal ditutup, keterangan yang sama tetap tampil di atas tab sampai ditutup atau periode diganti.
5. Galat lain (mis. periode sebelum cutover, `FIN-VAL-170`) hanya menampilkan pesannya — memetakan akun tidak memperbaikinya, sehingga tautan tidak ditawarkan.
6. **Bagian Perbandingan Kas** menampilkan tiga angka berdampingan: *Posisi kas terhitung*, *Saldo penutupan rekap kas harian* (beserta tanggal dan status rekap), dan *Selisih (rekap − terhitung)*, ditambah daftar mutasi kas periode itu.
7. Bila periode itu **belum punya rekap kas harian**, layar menulis *"Belum ada rekap"* dan *"Tidak dapat dinyatakan"* — bukan angka nol (§5 delta 1).

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

| No | Elemen | Keputusan | Sumber |
|:--:|---|:--:|---|
| 1 | Kartu ringkasan, filter periode, tabel snapshot, modal penerbitan | `REUSE` (diubah, bukan diganti) | `finance-monitoring-view.jsx`, `subledger-balances-table-columns.jsx`, `generate-subledger-snapshots-modal.jsx` |
| 2 | Format nominal | `REUSE` | `formatMoney` (`monitoring-utils`) |
| 3 | Label jenis dan arah mutasi, nominal bertanda, tanggal | `REUSE` | `cash-management-utils` (`getCashMovementTypeLabel`, `getCashMovementDirectionLabel`, `formatSignedCashMoney`, `formatDate`) dan `DAILY_CASH_STATUS_LABELS` |
| 4 | Label kelompok dan segmen pada daftar belum terpetakan | `REUSE` | `subledger-control-account-utils` (`getBalanceGroupLabel`, `getSegmentLabel`) |
| 5 | Cakupan pemetaan | `REUSE` | thunk `fetchControlAccountCoverage` dan state `financeSubledgerSetup.coverage` (`FE-FIN-027`) — tidak ada pemanggilan endpoint baru untuk ini |
| 6 | Keterangan penerbitan ditolak | `NEW (view-lokal)` | `snapshot-gate-alert.jsx` — spesifik domain, dipakai dua tempat (modal dan tab) |
| 7 | Bagian selisih | `NEW (view-lokal)` | `cash-variance-section.jsx` — spesifik domain, satu tempat |

**Hasil UI Gate:** tidak ada base component, wrapper, atau abstraksi generik baru. Styling baru lewat satu CSS Module (`src/style/corporate/finance/monitoring/subledger-balances.module.css`) yang hanya memakai token global, karena layar monitoring tidak dibungkus `.dataPage` (token `--base-*` tidak tersedia). Komponen existing pada layar ini memakai inline style; bagian yang diubah mengikuti pola itu hanya bila baris tersebut memang sudah inline.

---

## 4. Endpoint yang Dikonsumsi

Base URL: `/api/v1/corporate/finance-management/accounting-events`

| Method | Path | Hak akses | Dipanggil lewat | Catatan |
| :-- | :-- | :-- | :-- | :-- |
| `GET` | `/subledger-balances/{period}` | `FinanceAccountingEvent : Read` | `fetchSubledgerSnapshotsByPeriod` (existing) | `IsComplete` dipakai apa adanya dari backend |
| `POST` | `/subledger-balances/generate` | `FinanceAccountingEvent : Create` | `generateSubledgerSnapshots` (existing) | Body kini hanya `accountingPeriodCode` dan `notes`; empat ruas override tidak lagi dikirim |
| `GET` | `/subledger-balances/{period}/variance` | `FinanceAccountingEvent : Read` | `fetchCashVariance` (**baru**) | `signal` diteruskan; dibatalkan saat periode diganti |
| `GET` | `.../subledger-setup/control-accounts/coverage` | `FinanceSubledgerSetup : Read` | `fetchControlAccountCoverage` (existing, `FE-FIN-027`) | Dipicu **hanya** setelah penerbitan gagal tertutup |

### Delta antara kontrak, backend saat ini, dan layar

Semua dibaca dari `SubledgerPositionDtos.cs`, `FinanceSubledgerBalanceCalculator.cs`, `FinanceSubledgerSnapshotService.cs`, dan controller. Backend tidak diubah.

| # | Temuan | Perlakuan di layar |
|:--:|---|---|
| 1 | Bila periode belum punya rekap kas harian, backend memulangkan `DailyCashClosingBalance = 0.00`, `DailyCashSnapshotDate = null`, dan **`HasVariance = true`** (selisih = 0 − posisi terhitung) | Nol itu bukan angka rekap. Layar memakai `DailyCashSnapshotDate == null` sebagai penanda "belum ada rekap": menulis *Belum ada rekap* dan *Tidak dapat dinyatakan*, bukan perbandingan dan bukan "selisih". Disarankan backend memulangkan `null` untuk saldo penutupan pada keadaan itu |
| 2 | Pesan gagal tertutup (`FIN-VAL-177`/`178`) hanya menyebut **satu** butir pertama; acceptance criteria meminta pesan memuat kelompok dan segmen | Pesan backend ditampilkan apa adanya **dan** dilengkapi daftar penuh dari endpoint cakupan. Tanpa hak `FinanceSubledgerSetup : Read` daftar tidak muncul, pesan dan tautan tetap ada |
| 3 | `GenerateSubledgerSnapshotsResponse` tidak memuat `IsComplete` | Reducer tidak lagi menghitungnya (`>= 4` dibuang); diisi `null` sesaat, lalu hook memuat ulang `GET` periode untuk nilai sebenarnya. Selama `null`, kartu menulis *Memeriksa...* |
| 4 | `SubledgerAccountSnapshotItemResponse` tidak memuat `SegmentKey`; segmen hanya ada pada teks `AccountName` (mis. "Piutang - Penjamin") | Layar tidak mengurai teks itu; `AccountName` ditampilkan apa adanya |
| 5 | `UTANG-JASA-MEDIS` selalu `0.00` (`FIN-DEC-122`, belum ada penulisnya) | Baris diberi keterangan bahwa nol itu bukan hasil perhitungan saldo nol |
| 6 | `ExplainingMovements` tidak berpaging dan memuat `Notes`, sedangkan catatan mutasi dapat memuat nama pihak ketiga (`03-frontend-architecture.md` 19.6) | `Notes` **dibuang saat normalisasi** dan tidak pernah dirender; daftar dibatasi 200 baris dengan penanda jujur "menampilkan N dari M" dan tautan ke Buku Mutasi Kas |
| 7 | Empat ruas override kode akun pada request sudah `[Obsolete]` dan diabaikan | Kolom dan pengirimannya dihapus dari modal; teks "Empat Akun Kontrol yang Dihitung" diganti penjelasan "satu baris per pemetaan aktif" |
| 8 | Backend tidak menyebut sisi saldo normal tiap kelompok | Keterangan nilai negatif sengaja tidak menyebut debit/kredit: *"berlawanan dengan saldo normal akun ini"* |
| 9 | `api-contract.md` F.6 masih memberi label **"Rencana (belum tersedia)"** pada dua endpoint yang sudah ada (`position`, `variance`) dan `restate` | Tidak diubah pada task ini (artefak kontrak di luar wewenang tulis task ini); dilaporkan |

---

## 5. Berkas yang Diubah dan Dibuat

### Baru (`QuilvianSystemFrontendDev`)

| Berkas | Isi |
| --- | --- |
| `src/utils/finance/monitoring/subledger-snapshot-utils.jsx` | Fungsi murni: nilai negatif, penghitungan baris/kelengkapan, pengenalan galat gagal tertutup, normalisasi cakupan dan selisih (membuang `Notes`), keadaan selisih, pembatasan daftar, klasifikasi galat |
| `src/components/view/finance/monitoring/subledger-balances/snapshot-gate-alert.jsx` | Keterangan penerbitan ditolak beserta rincian dan tautan |
| `src/components/view/finance/monitoring/subledger-balances/cash-variance-section.jsx` | Bagian selisih kas |
| `src/style/corporate/finance/monitoring/subledger-balances.module.css` | CSS Module |
| `tests/unit/subledger-snapshot-utils.test.mjs` | 11 unit test (ditulis, **belum dijalankan**) |

### Diubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/finance/monitoring/finance-monitoring-view.jsx` | Lima keterangan "4 akun" dihapus; kartu kelengkapan memakai `resolveSnapshotCompletion`; keterangan gagal ditampilkan di tab; bagian selisih dipasang; prop baru ke modal; `DataTable` snapshot memakai `rowKey`, `emptyTitle`/`emptyDescription`, `loadingText`, dan `sortLatestFirst={false}` (urutan baris mengikuti backend) |
| `.../subledger-balances/subledger-balances-table-columns.jsx` | Kolom nominal: tanda negatif, keterangan, keterangan nol utang jasa medis; `key` ditambahkan pada setiap kolom; kolom "No" memakai `meta.rowNumber` |
| `.../subledger-balances/generate-subledger-snapshots-modal.jsx` | Override dihapus; penjelasan diganti; galat gagal tertutup ditampilkan |
| `src/lib/hooks/finance/monitoring/use-finance-subledger-balances.jsx` | Selisih (dengan abort), cakupan saat gagal tertutup, muat ulang setelah penerbitan |
| `src/lib/state/slice/finance/monitoring/finance-monitoring-slice.jsx` | Thunk `fetchCashVariance`, state selisih dan kode status galat, `isComplete: null` pasca-penerbitan |
| `src/lib/constants/finance/monitoring/monitoring-constants.jsx` | Entri kelima `UTANG-JASA-MEDIS` pada meta kategori; konstanta pesan dan batas baris |

Tidak ada perubahan pada repository backend selain berkas laporan ini dan bukti roadmap.

---

## 6. Wewenang UI yang Dipakai

Semua keputusan makna dipenuhi sebagai invariant (nilai negatif, dua angka berdampingan, tautan pemetaan). Yang dipilih sendiri (`DEV_DISCRETION`): warna dan keterangan teks untuk negatif (warna **tidak** menjadi satu-satunya penanda), tata letak tiga kartu, dan penggunaan `<details>` untuk daftar mutasi. **Tidak** ada kata-kata peringatan tentang kedudukan rekap kas harian yang dikarang — kata-kata itu menunggu brief pemilik (`FE-FIN-026`); label di bagian ini hanya netral-faktual.

---

## 7. Verifikasi dan Validasi

| Pemeriksaan | Perintah | Status | Keterangan |
| --- | --- | :--: | --- |
| Lint | `npm run lint:errors` | `NOT RUN` | Standing instruction pengguna. **Menunggu pengguna** |
| Unit test | `npm run test:unit` atau `node --import ./tests/helpers/register.mjs --test tests/unit/subledger-snapshot-utils.test.mjs` | `NOT RUN` | 11 test ditulis; hasil belum diketahui |
| Build | `npm run build` | `NOT RUN` | Alasan sama |
| Resolusi import dan named export | skrip pemeriksa berkas | `PASS` | 26 berkas; 0 import tak terselesaikan. Lima "missing export" yang ditandai skrip adalah ekspor hasil destructuring `slice.actions` dan sudah dicek manual keberadaannya |
| `git status --short` | — | `PASS` | §10 |

### Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Alasan konkret: sesi ini tidak menjalankan server dev (instruksi build/test manual), tidak memegang sesi login, dan tidak memverifikasi backend + data berjalan. Belum satu pun perilaku di bawah ini pernah teramati.

| # | Skenario | Yang harus teramati |
|:--:|---|---|
| 1 | Periode dengan 5 atau 6 baris snapshot | Seluruh baris tampil; tidak ada teks "4 akun"; kartu jumlah menulis jumlah sebenarnya |
| 2 | Periode lama (sebelum REV-14, 4 baris, tanpa selisih) — **regresi `FE-FIN-015`** | Tabel tetap tampil benar; kartu kelengkapan mengikuti `IsComplete` backend; bagian selisih memuat atau memberi keterangan, tanpa merusak layar |
| 3 | Snapshot berisi nilai negatif | Tampil bertanda minus, berwarna, dengan keterangan; **bukan** `Rp 0` |
| 4 | Terbitkan dengan satu kelompok belum terpetakan | Modal tetap terbuka; pesan backend, daftar lengkap kelompok/segmen, dan tautan pemetaan muncul; tidak ada baris outbox baru |
| 5 | Tutup modal setelah gagal | Keterangan yang sama tetap di atas tab; *Tutup keterangan* menghapusnya; ganti periode menghapusnya |
| 6 | Terbitkan untuk periode sebelum cutover | Hanya pesan cutover; **tanpa** tautan pemetaan |
| 7 | Terbitkan sukses | Modal menutup; tabel dan kartu kelengkapan terisi dari `GET` (sempat *Memeriksa...*) |
| 8 | Pengguna tanpa `FinanceSubledgerSetup : Read` mengalami gagal tertutup | Pesan dan tautan tampil; daftar rinci tidak (tanpa galat tambahan) |
| 9 | Periode dengan rekap kas harian dan posisi berbeda | Tiga angka berdampingan, selisih menonjol, daftar mutasi terbuka lewat `<details>` tanpa `Notes` |
| 10 | Periode dengan rekap dan posisi sama | Keterangan "sama"; selisih tampil `Rp 0` sebagai angka sah |
| 11 | Periode **tanpa** rekap kas harian | *Belum ada rekap* / *Tidak dapat dinyatakan*; bukan `Rp 0` dan bukan perbandingan |
| 12 | Periode sebelum cutover pada bagian selisih (`422`) | Keterangan buku mutasi belum berjalan, bukan angka nol |
| 13 | Ganti periode berulang cepat | Tidak ada selisih periode lama yang terbaca sebagai periode baru; tidak ada galat akibat permintaan terbatalkan |

---

## 8. Pemenuhan Acceptance Criteria

Status "Terpenuhi (source)" = terpenuhi menurut pembacaan kode; **belum teramati saat dijalankan**.

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | Jumlah baris tidak lagi tetap empat | Terpenuhi (source) | Lima keterangan "4" dihapus; `countSnapshotRows`, `resolveSnapshotCompletion`; unit test |
| 2 | Nilai negatif **MUST** terbaca negatif beserta keterangan berlawanan dengan saldo normal akun — tidak disembunyikan atau nol | Terpenuhi (source) | `formatMoney` mempertahankan tanda; kolom nominal menambah warna dan teks keterangan; unit test `getNegativeBalanceNote` |
| 3 | Pesan gagal tertutup memuat kelompok dan segmennya | Terpenuhi (source) | Pesan backend + daftar penuh dari cakupan; delta 2 |
| 4 | Pesan gagal tertutup memuat tautan ke layar pemetaan | Terpenuhi (source) | `SnapshotGateAlert`; hanya untuk galat gagal tertutup |
| 5 | Bagian selisih menampilkan **kedua angka** berdampingan, bukan hanya pernyataan tidak cocok | Terpenuhi (source) | `CashVarianceSection`: tiga kartu berdampingan; keadaan tanpa rekap dinyatakan terpisah (delta 1) |
| 6 | Bagian selisih ditambahkan pada layar yang sama | Terpenuhi (source) | Dipasang di tab Saldo Subledger |
| 7 | `FE-FIN-015` tidak regresi untuk periode lama | **Belum terbukti** | Skenario manual 2 |
| 8 | `npm run lint` dan `npm run build` PASS | **Belum terpenuhi — NOT RUN** | §7 |

---

## 9. Risiko

1. **Regresi `FE-FIN-015`** adalah risiko yang disebut roadmap dan **belum dibuktikan**; skenario manual 2 adalah satu-satunya bukti yang sah.
2. **Kelengkapan sesaat `null`.** Antara penerbitan sukses dan pemuatan ulang `GET`, kartu menulis *Memeriksa...*; bila `GET` gagal, keadaan itu bertahan sampai pengguna memuat ulang.
3. **Delta 1 bergantung pada tanggal rekap `null`.** Bila backend suatu saat memulangkan tanggal untuk periode tanpa rekap, layar akan menampilkan perbandingan palsu; sebaiknya kontraknya dipertegas.
4. **Daftar rinci gagal tertutup** bergantung pada hak `FinanceSubledgerSetup : Read`; tanpanya pengguna hanya melihat butir pertama dari pesan backend.
5. Meta kategori masih memuat empat kode akun bawaan lama (`1-1002`, dst.) sebagai kunci pencarian sekunder; baris dikenali lewat kategori sehingga tidak berdampak, tetapi nilai itu tidak lagi mencerminkan pemetaan.

### Temuan di luar cakupan (`UNRELATED EXISTING ISSUE`, tidak diperbaiki)

Cacat sejenis pada tabel lain yang **tidak** disentuh task ini; sudah dicocokkan dengan prop `DataTable` yang sebenarnya:

- Tab *Fakta Billing* dan *Antrean Kejadian* pada `finance-monitoring-view.jsx` memberi `DataTable` prop `getRowId` dan `emptyMessage` (tidak dikenal; yang dibaca `rowKey` dan `emptyTitle`).
- `subledger-control-accounts-view.jsx` (`FE-FIN-027`) memberi `DataTable` prop `getRowId` dan `emptyText` (tidak dikenal).
- `finance-monitoring-view.jsx` memakai banyak inline style; tidak dirapikan.

---

## 10. Status Git (frontend)

Branch `yasmina`; tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Working tree juga memuat pekerjaan `FE-FIN-027` (perbaikan), `FE-FIN-028`, yang belum di-commit.

```text
 M src/components/view/finance/monitoring/finance-monitoring-view.jsx
 M src/components/view/finance/monitoring/subledger-balances/generate-subledger-snapshots-modal.jsx
 M src/components/view/finance/monitoring/subledger-balances/subledger-balances-table-columns.jsx
 M src/lib/constants/finance/monitoring/monitoring-constants.jsx
 M src/lib/hooks/finance/monitoring/use-finance-subledger-balances.jsx
 M src/lib/state/slice/finance/monitoring/finance-monitoring-slice.jsx
?? src/components/view/finance/monitoring/subledger-balances/cash-variance-section.jsx
?? src/components/view/finance/monitoring/subledger-balances/snapshot-gate-alert.jsx
?? src/style/corporate/finance/monitoring/
?? src/utils/finance/monitoring/subledger-snapshot-utils.jsx
?? tests/unit/subledger-snapshot-utils.test.mjs
```

(Berkas `FE-FIN-028` dan perbaikan `FE-FIN-027` tidak diulang di sini.)

---

## 11. Langkah Berikutnya

Jalankan manual: `npm run lint:errors`, `npm run test:unit`, `npm run build`, lalu amati skenario §7 — terutama nomor 2 (regresi periode lama), 4, 11. Setelah itu task dapat ditandai ✅.
