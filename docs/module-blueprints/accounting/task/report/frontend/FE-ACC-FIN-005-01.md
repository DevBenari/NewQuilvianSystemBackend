# Laporan Perubahan Frontend — `FE-ACC-FIN-005-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-005-01` |
| Judul | Buku besar bertelusur dan berunduh |
| Slice | Gelombang pengiriman `FIN-0`, gelombang eksekusi 1 |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 1, kartu `FE-ACC-FIN-005-01` |
| Trace | ACC-FIN-005 (PRD `ACC-FIN-PRD-001`; ClickUp `z8m132yv7x`); `FR-FIN-001`..`005`; `ACC-DEC-147` butir 1, `148`, `149`; `03-frontend-architecture.md` bagian 15.4, 15.9 |
| Contract version | `ACC-API-0.17` — `approved` Rizki 24 November 2026 (`GATE-DESAIN-1124`) |
| Dependency | `BE-ACC-FIN-005-01` 🟡 — source lengkap, build `PASS` (DLL 15:19:56 WIB), uji runtime dijadwalkan bersama task ini |
| Task mode | `FRONTEND` — perintah Rizki 24 November 2026 "sudah build, lanjut FE-ACC-FIN-005-01" |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `965b3f5dd` |
| Tanggal | 24 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-8 terbukti 24 November 2026: uji layar Playwright oleh agent terhadap `next dev` yang dinyalakan Rizki (14/14), ESLint 0/0. UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Layar Buku Besar sebelumnya hanya bisa memilih akun dan rentang tanggal, lalu menampilkan tabel. Petugas tidak dapat:

- melihat bagian satu unit biaya;
- melihat saldo awal dan saldo akhir rentang sekaligus;
- tahu asal baris dan membuka jurnalnya dengan sekali klik;
- mengunduh mutasi; dan
- datang dari laporan dengan saringan yang sudah terisi.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Menelusuri mutasi satu akun sampai jurnal dan asalnya, lalu menyimpannya sebagai CSV |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Membuka menu Buku Besar, atau mengklik baris akun di Laba Rugi atau Neraca (sesudah `FE-ACC-FIN-008-02`/`03`) |
| Langkah | 1) Pilih akun; bila perlu pilih unit biaya dan rentang tanggal. 2) Ringkasan empat angka dan tabel tampil. 3) Klik nomor jurnal → Rincian Jurnal. 4) Klik asal kejadian → Rincian Kejadian. 5) Unduh CSV |
| Jalur tidak normal | Ringkasan gagal → pesan "Ringkasan gagal dimuat." tanpa menahan tabel. Unduhan ditolak `422` → pesan backend tampil apa adanya. Tautan dari tab lain tak terbaca → "Saringan dari tautan tidak dapat dibaca. Pilih akun kembali." Tanpa hak → `AccessDeniedGate` |
| Hasil | Tabel bertanda asal dan unit, ringkasan saldo, berkas CSV |

**Contoh.** Petugas memilih Beban Obat, unit Radiologi, 1–30 November 2026. Layar menampilkan Saldo Awal Rp 0, Total Debit Rp 4.000.000, Total Kredit Rp 0, Saldo Akhir Rp 4.000.000 (3 baris mutasi). Di bawahnya muncul catatan "Saldo ini hanya bagian unit RAD — Radiologi…". Unduhan menghasilkan `buku-besar_5-1001_20261101-20261130.csv`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`general-ledger-view.jsx`, `use-general-ledger.jsx`, `accounting-general-ledger-slice.jsx`, `general-ledger-constants.jsx`, `general-ledger/page.jsx` dan `general-ledger-client.jsx`, `journals/page.jsx` (preseden `Suspense`), `use-journal.jsx` dan `use-accounting-event-detail.jsx` (preseden token rute privat), `journal-form-view.jsx` (preseden `useSelectResource("costCenters")`), `use-supplier-payable-movements.jsx` (preseden unduhan blob), base component `summary-grid.jsx`, `data-filter.jsx`, `resource-filter-select.jsx`, `information-alert.jsx`, `base-button.jsx`, `private-route-token-utils.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/accounting/general-ledger/general-ledger-view.jsx` | Saringan Unit Biaya, tombol Unduh CSV di `actions` `DataFilter`, `SummaryGrid` empat angka, catatan unit, peringatan tautan dan galat unduhan, kolom Asal dan Unit Biaya, nomor jurnal dan asal kejadian sebagai tombol |
| `src/lib/hooks/corporate/accounting/general-ledger/use-general-ledger.jsx` | Saringan `costCenterId`, pembacaan saringan dari alamat, ringkasan, unduhan blob, `openJournal`, `openAccountingEvent` |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | Thunk `getLedgerMovementSummary` beserta state dan selector; ringkasan ikut dibersihkan bersama mutasi |
| `src/lib/constants/corporate/accounting/general-ledger/general-ledger-constants.jsx` | `apiBase`, `costCenterId` di saringan bawaan, kolom Asal dan Unit Biaya, format `journal`, `GENERAL_LEDGER_SOURCE_KIND`, `GENERAL_LEDGER_QUERY_KEYS`, `GENERAL_LEDGER_ROUTE_TOKEN`, `GENERAL_LEDGER_COPY` |
| `src/utils/corporate/accounting/accounting-download-utils.js` | **Baru.** `saveBlobAsFile`, `readDownloadErrorMessage` (membaca pesan JSON dari galat blob), `toFileNameSegment`, `toCompactDate` — dipakai ulang `FE-ACC-FIN-008-01`..`03` |
| `src/utils/corporate/accounting/general-ledger/general-ledger-query-utils.js` | **Baru.** `readGeneralLedgerFiltersFromQuery`, `buildGeneralLedgerHref` (dipakai laporan untuk menelusur) |
| `src/style/corporate/accounting/general-ledger-view.module.css` | Satu kelas `.sourceText` memakai token, tanpa nilai literal |
| `src/app/corporate/accounting/general-ledger/page.jsx` | Dibungkus `Suspense`, karena hook kini memakai `useSearchParams` — pola `journals/page.jsx`. **Delta lingkup:** berkas ini tidak tercantum di daftar berkas kartu; perubahannya satu pembungkus pada rute layar yang sama |

Nol baris komentar baru. Komentar lama tidak disunting.

### 3.3 Dampak kontrak, keamanan, privasi

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Memakai `GET /movements` (+`CostCenterId`), `GET /movements/summary`, `GET /movements/export` persis `ACC-API-0.17`. **Nol delta kontrak** |
| Nama berkas unduhan | Disusun frontend dengan pola backend (`buku-besar_{kode akun}_{yyyyMMdd}-{yyyyMMdd}.csv`), karena CORS backend tidak mengekspos `Content-Disposition` (temuan `BE-ACC-FIN-005-01`) |
| Keamanan | Akun dan unit biaya di alamat memakai token rute privat (`scope` `accounting`), bukan Guid mentah. Penegakan hak tetap di backend |
| Privasi | Kolom Asal hanya menampilkan modul dan nomor transaksi sumber dari backend |

## 4. Gerbang keputusan base component

`UI GATE: 7 elemen — REUSE 7, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Header dan badan hukum | `Hero` + `AccountingHeroLegalEntity` | sudah dipakai layar ini | REUSE | Tidak berubah |
| Saringan akun, tanggal, ukuran halaman | `DataFilter`, `FilterSelect`, `FilterDatePicker` | sudah dipakai layar ini | REUSE | Tidak berubah |
| Saringan unit biaya | `ResourceFilterSelect` + `useSelectResource("costCenters")` | dipakai `journal-form-view.jsx` | REUSE | `onlyActive: false`, karena unit nonaktif sah sebagai saringan (`VAL-GL-01`) |
| Tombol Unduh CSV | `BaseButton` di `actions` `DataFilter` | `base-button.jsx` `loading`, `loadingLabel`; `data-filter.jsx` prop `actions` | REUSE | Bukan di header halaman (PRD 7.1) |
| Ringkasan empat angka | `SummaryGrid` | dipakai `period-closing-view.jsx` | REUSE | Nilai teks rupiah ditampilkan apa adanya oleh `formatValue` |
| Catatan unit, galat, tautan tak terbaca | `InformationAlert` | dipakai `period-closing-view.jsx`, `accounting-configuration-view.jsx` | REUSE | `info`, `warning`, `danger` |
| Tabel dengan tautan sel | `DataTable` + `BaseButton variant="ghost" size="sm"` | sudah dipakai layar ini; tombol sel serupa di `accounting-event-detail-view.jsx` | REUSE | Tanpa `<button>` mentah |

Tidak ada pilihan yang menunggu keputusan.

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Layar Buku Besar dan Neraca Saldo yang ada |
| Urutan halaman list | **Menyimpang dari pola standar, sesuai brief.** Ringkasan diletakkan sesudah saringan, mengikuti skema `03-frontend-architecture.md` bagian 15.4 yang disetujui (`GATE-DESAIN-1124`), karena ringkasan bergantung pada akun yang dipilih |
| Kolom tabel di berkas `*-table-columns.jsx` terpisah | **Tidak.** Kolom tetap di konstanta seperti layar ini sebelumnya (`TOUCHED LEGACY`); tidak direfaktor |
| Grep hex baru / `!important` / `<button>`/`<table>` mentah / inline style | 0 / 0 / 0 / 0 |
| `globals.css` | Tidak disentuh |
| Typography base component di-override | Tidak — `.sourceText` hanya menyasar `<span>` milik layar |
| Keadaan memuat, kosong, gagal, tanpa hak | Memakai keadaan `DataTable`, `SummaryGrid`, `InformationAlert`, `AccessDeniedGate` yang ada |
| Loading label tombol ber-API | `Mengunduh...` |

## 6. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` 7 berkas JS/JSX tersentuh | Tanpa keluaran, kode keluar `0` — 0 galat, 0 peringatan | `PASS` |
| Pemindaian baris komentar baru pada diff dan berkas baru | Nol | `PASS` |
| Grep anti-regresi UI | 0 temuan | `PASS` |
| `npm run build` | Tidak dijalankan — server dan build frontend dijalankan Rizki | `NOT RUN` |
| Uji layar `03-uji-layar.mjs` U1–U10 (masuk lewat klik menu Akuntansi → Buku Besar) | 10/10 | `PASS` |
| Uji keadaan `04-uji-layar-tambahan.mjs` T1 kosong, T2 gagal (`400` asli), T3 tombol selama unduh | 3/3 | `PASS` |
| Uji keadaan memuat `05-uji-memuat.mjs` T4 | Gagal pada putaran pertama → **cacat ditemukan dan diperbaiki** (lihat bagian 9) → 1/1 | `PASS` sesudah perbaikan |
| Regresi U1–U10 dan T1–T3 sesudah perbaikan slice | 13/13 | `PASS` |

`MANUAL TEST: PASS` — Playwright (Edge, headless) terhadap server `next dev` yang dinyalakan Rizki, 24 November 2026 15.40–15.55 WIB; bukti JSON + PNG `01`..`14` di `QuilvianSystemFrontendDev/test-with-agy/accounting/fe-acc-fin-005-01-20261124/`. T3 dan T4 menunda respons **asli** 2–2,5 detik lewat `page.route` agar keadaan sementara terekam; isi respons tidak diubah. U10 memakai pengguna nyata tanpa hak (`QUILVIAN_ASET_APPROVER`), bukan simulasi.

`AUTOMATED TEST: SKIPPED (opsional) — unit test tidak ditulis atas kesepakatan kerja Rizki.`

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Unit biaya menyaring tabel dan ringkasan, catatan tampil | **Terpenuhi** | U3: 1 baris, catatan unit, '1 baris mutasi'; PNG `03` |
| AC-2 Ringkasan empat angka; gagalnya tidak menahan tabel | **Terpenuhi** | U2 empat angka; T2 'Ringkasan gagal dimuat.' sementara layar tetap tampil; PNG `02`, `12` |
| AC-3 Nomor jurnal membuka Rincian Jurnal lewat token | **Terpenuhi** | U5: `/corporate/accounting/journals/jb-2026-09-00001-31545fedd639`; PNG `05` |
| AC-4 Asal kejadian membuka Rincian Kejadian; asal lain teks | **Terpenuhi** | U6/U7: dua tombol `Finance · AR-UJI-*`, membuka `/accounting-events/finance-ar-uji-0113-…`; PNG `06`, `07` |
| AC-5 Unduhan bernama benar; tombol mati selama proses | **Terpenuhi** | U4 `buku-besar_4001_awal-akhir.csv` berisi hanya baris unit; T3 'Mengunduh...' dan mati, lalu aktif kembali |
| AC-6 Saringan dari alamat; token tak terbaca memberi pesan | **Terpenuhi** | U6 permintaan membawa `accountId`, `dateFrom`, `dateTo`; U8 pesan tautan tak terbaca; PNG `06`, `08` |
| AC-7 Keadaan memuat, belum pilih akun, kosong, gagal | **Terpenuhi** | T4 memuat (PNG `14`), U1/U9 belum pilih akun, T1 kosong (PNG `11`), T2 gagal (PNG `12`) |
| AC-8 Lint nol galat | **Terpenuhi** | ESLint kode keluar `0` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Unit biaya terpilih hanya dikosongkan lewat tombol Atur Ulang; `ResourceFilterSelect` tidak menyediakan pilihan kosong. Diterima karena pola yang sama dipakai di layar lain |
| Risiko | Pilihan unit biaya dari `useSelectResource("costCenters")` tidak disaring per badan hukum; unit badan hukum lain akan ditolak backend `400`. Pada MVP satu badan hukum (`ACC-DEC-041`) risiko ini tidak muncul |
| Perubahan sampingan | `NONE` |
| Status Git frontend | `M` page.jsx, general-ledger-view.jsx, general-ledger-constants.jsx, use-general-ledger.jsx, accounting-general-ledger-slice.jsx, general-ledger-view.module.css; `??` accounting-download-utils.js, `utils/corporate/accounting/general-ledger/` |
| Langkah berikutnya | Rizki menyalakan server frontend dan menjalankan uji `F10`, `F12`, `F13`, `F14` (alamat buatan), `F15`, `F16`, `F19` bersama backend. Bila lulus, `BE-` dan `FE-ACC-FIN-005-01` naik ke ✅, lalu `BE-ACC-FIN-008-01` |

## 9. Cacat yang ditemukan saat uji dan diperbaiki

| Hal | Isi |
| --- | --- |
| Gejala | Selama mutasi dan ringkasan dimuat, layar menampilkan "Ringkasan tidak tersedia." dan "Akun ini belum memiliki mutasi." alih-alih keadaan memuat — padahal akun itu punya 2 mutasi (PNG `14` putaran pertama, ditimpa putaran akhir) |
| Penyebab | Reducer `rejected` menurunkan `movementsLoading` **sebelum** memeriksa pembatalan. Permintaan yang dibatalkan (efek ganda `reactStrictMode: true` di dev, atau pergantian saringan di produksi) tiba sesudah `pending` permintaan berikutnya dan mematikan tanda loading permintaan yang masih berjalan. Pola ini **sudah ada** pada `getLedgerMovements` dan ikut tersalin ke `getLedgerMovementSummary` |
| Perbaikan | Pola baku Redux Toolkit: `pending` menyimpan `action.meta.requestId`; `fulfilled` dan `rejected` milik permintaan lama diabaikan. Diterapkan pada mutasi dan ringkasan di `accounting-general-ledger-slice.jsx`; ESLint kode keluar `0` |
| Bukti | T4 lulus; regresi U1–U10 dan T1–T3 13/13 lulus |
| Sisa | `getTrialBalance` di slice yang sama memakai pola lama; layar Neraca Saldo disentuh `FE-ACC-FIN-008-01`, jadi diperbaiki di sana |

## 10. Temuan yang tidak diperbaiki

| Temuan | Alasan dibiarkan |
| --- | --- |
| Saat galat backend (T2), tabel di bawah pesan galat tetap menampilkan penjelasan kosong "Tidak ada mutasi pada rentang tanggal ini." | Perilaku `DataTable` lama sebelum task ini; tidak menyesatkan karena pesan galat tampil di atasnya |
| Kerangka `SummaryGrid` saat memuat selalu lima kartu, sedangkan ringkasan buku besar empat | Perilaku base component; tidak diubah dari layar feature |
| Tombol Unduh CSV tetap aktif walau rentang tanggal terbalik | Backend menolak `400` dan pesannya tampil apa adanya (A9) |
