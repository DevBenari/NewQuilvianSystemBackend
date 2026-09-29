# Laporan Perubahan Frontend — `FE-ACC-P2-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-017` |
| Judul | Pulihkan butir menu Rekonsiliasi Control Account — diperluas: butir Tutup Tahun |
| Slice | Wave D — pasangan `BE-ACC-P2-014` |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-017` (revisi 7, `APPROVED` Rizki 28 September 2026) |
| Trace | `03-frontend-architecture.md` bagian 9 butir 7; `FE-ACC-P2-008`; temuan 28 September 2026 (butir hilang lewat merge `8f01cf06c`) |
| Contract version | Nol API. Layar tujuan dijaga `AccountingReconciliation : Read` seperti sebelumnya |
| Wewenang UI | Entri dipulihkan **persis** seperti `f6b1498fe` (Rekonsiliasi) dan `f37e949ea` (Tutup Tahun); tidak ada keputusan rupa baru. Perluasan cakupan: keputusan Rizki 29 September 2026 ("perluas FE-ACC-P2-017") |
| Dependency | — |
| Klasifikasi | `LIGHT` — skor 1: satu berkas diubah, nol logika bisnis, nol kontrak, satu butir menu |
| Task mode | `FRONTEND` (Langkah D, berpasangan dengan `BE-ACC-P2-014` atas perintah Rizki 28 September 2026) |
| Target tulis | `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `a6d269077` (branch `RizkiV2`); butir Rekonsiliasi masuk `5adc9f316`. Perluasan dikerjakan di atas `5adc9f316`, belum di-commit |
| Commit backend yang dijadikan rujukan | `485d8b8b` (branch `rizkiG`) |
| Tanggal | 28 September 2026; perluasan 29 September 2026 |
| Status | **✅ SELESAI — 29 September 2026.** Butir Rekonsiliasi: eslint 0/0, unit test 22/22, build Rizki 411/411, klik menu L1 dijalankan Rizki (pernyataan owner). Perluasan atas keputusan Rizki 29 September 2026 — butir Tutup Tahun: +6/−0 identik `f37e949ea`; eslint 0/0; unit test 38/38; `menu-permission-filter` 8/9 (1 gagal lama M0 Bank Darah, di luar task); build Rizki tak langsung (`.next/BUILD_ID` 10.17). Klik butir Tutup Tahun dibuktikan tangkapan layar Rizki 29 September 2026: sidebar Accounting memuat Rekonsiliasi Control Account lalu **Tutup Tahun** (aktif), dan layar Tutup Tahun terbuka. UAT belum dijalankan. Riwayat: 🟡 28 September 2026; ✅ 29 September 2026 sebelum perluasan; 🟡 29 September 2026 selama perluasan |

## 1. Keadaan yang ditemukan di awal

Butir "Rekonsiliasi Control Account" ditambahkan `f6b1498fe` dan masih ada pada `f37e949ea`
(15 September 2026). Resolusi merge **`8f01cf06c`** ("merge dengan branch rizki", 18 September
2026) menghapusnya bersama impor ikon `RiSafeLine`. Route `/corporate/accounting/reconciliation`
tetap terbangun, sehingga layar hanya terjangkau lewat alamat langsung, dan test "route, menu, dan
store terdaftar" pada `tests/unit/accounting-reconciliation.test.mjs` gagal.

**Temuan tambahan di luar kartu:** merge yang sama juga menghapus butir **"Tutup Tahun"**
(`corporateAccountingYearEndClosing`, ikon `RiBookletLine`, `/corporate/accounting/year-end-closing`).
Butir itu semula **tidak** dipulihkan karena kartu hanya mencakup Rekonsiliasi. **29 September 2026
Rizki memutuskan memperluas kartu ini**, dan butir Tutup Tahun dipulihkan pada pengerjaan kedua.
Route `/corporate/accounting/year-end-closing` beserta layarnya masih ada; yang hilang hanya butir menu.

## 2. Proses bisnis dari sisi pengguna

1. Petugas akuntansi membuka menu Accounting di sidebar.
2. Sesudah Neraca Saldo tampil butir **Rekonsiliasi Control Account**.
3. Klik butir itu membuka layar Rekonsiliasi. Pengguna tanpa hak `AccountingReconciliation : Read`
   tetap melihat penolakan akses dari layar, sama seperti sebelum butir hilang.
4. Sesudah Rekonsiliasi tampil butir **Tutup Tahun**; kliknya membuka layar Tutup Tahun. Kewenangan
   tetap dijaga layar itu sendiri, tidak berubah.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `src/utils/menu-sidebar/menu-items.jsx` pada `HEAD`, `f37e949ea`, `f6b1498fe` | Bentuk asli entri, letaknya, dan impor ikon |
| `git log -m -S corporateAccountingReconciliation` | Commit yang menambah dan menghapus butir |
| `tests/unit/accounting-reconciliation.test.mjs`; `package.json` | Test yang menjaga butir menu; cara menjalankan unit test |
| `git log -S corporateAccountingYearEndClosing`; `src/app/corporate/accounting/year-end-closing/` | Asal butir Tutup Tahun (`460f717a0`) dan bukti route masih ada |
| Grep `corporateAccountingYearEndClosing` di `src/` dan `tests/` | Nol peta izin berbasis key dan nol test yang merujuk butir itu — cukup satu entri |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | + impor `RiSafeLine`; + entri `{ label: "Rekonsiliasi Control Account", key: "corporateAccountingReconciliation", icon: <RiSafeLine className="fs-4" />, pathname: "/corporate/accounting/reconciliation" }` sesudah Neraca Saldo. +7/−0 baris — sudah di-commit `5adc9f316` |
| `src/utils/menu-sidebar/menu-items.jsx` (perluasan) | + entri `{ label: "Tutup Tahun", key: "corporateAccountingYearEndClosing", icon: <RiBookletLine className="fs-4" />, pathname: "/corporate/accounting/year-end-closing" }` sesudah Rekonsiliasi. `RiBookletLine` sudah diimpor. +6/−0 baris |

### 3.3 Kepatuhan arsitektur frontend

Nol komponen, nol gaya, nol state. Entri memakai bentuk entri menu lain di sub-menu yang sama.
`UI GATE`: nol elemen baru — butir menu memakai struktur `menuItems` yang sudah ada (`REUSE`).

## 4. State yang ditangani di layar

Tidak ada state baru. Layar tujuan tidak berubah pada task ini.

## 5. Endpoint yang dikonsumsi

Tidak ada.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src/utils/menu-sidebar/menu-items.jsx` | Keluar `0`, nol pesan | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/accounting-reconciliation.test.mjs` | `tests 22`, `pass 22`, `fail 0` — termasuk "route, menu, dan store terdaftar" | `PASS` | Keluaran perintah |
| `git diff --stat` | Satu berkas, +7/−0 | `PASS` | — |
| `npm run build` (Rizki) | `npm run build` Rizki 29 September 2026 berhasil (Compiled 53 detik, 411/411 halaman, postbuild standalone siap) | `PASS` | Keluaran build yang ditempel Rizki |
| Klik butir menu di layar | Dijalankan Rizki 29 September 2026 sebagai L1 uji `FE-ACC-P2-016`, sesuai harapan | `PASS` (pernyataan owner) | Laporan uji Rizki; tangkapan layar tidak dilampirkan |
| `AUTOMATED TEST` | Test yang sudah ada dijalankan; nol test baru (opsional) | `PASS` | Baris di atas |
| **Perluasan 29 September 2026** | | | |
| `git diff --stat` | Satu berkas, +6/−0; blok entri dibandingkan dengan `f37e949ea` baris 652–664 lewat `diff` — identik | `PASS` | Keluaran perintah |
| `npx eslint src/utils/menu-sidebar/menu-items.jsx` | Keluar `0`, nol pesan | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/accounting-reconciliation.test.mjs tests/unit/accounting-configuration-and-year-end.test.mjs` | `tests 38`, `pass 38`, `fail 0` | `PASS` | Keluaran perintah |
| `node … --test tests/unit/menu-permission-filter.test.mjs` | `tests 9`, `pass 8`, `fail 1` — M0 tiga butir Setup Bank Darah | `UNRELATED EXISTING ISSUE` | Sama dengan baseline `FE-ACC-P2-016` |
| `npm run build` (Rizki) | `.next/BUILD_ID` 29 September 2026 10.17, sesudah perluasan ditulis 09.58 | `PASS` (tak langsung) | Tanggal berkas build |
| Klik butir Tutup Tahun di layar | Tangkapan layar Rizki 29 September 2026: butir Tutup Tahun tampil sesudah Rekonsiliasi Control Account, berstatus aktif, layar Tutup Tahun terbuka (badan hukum PT Metropolitan Medical Centre) | `PASS` | Tangkapan layar dikirim Rizki di percakapan |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Butir tampil di menu Accounting pada letak semula | Terpenuhi di source dan di layar (L1, 29 September 2026) | Entri sesudah Neraca Saldo — sama dengan urutan `f37e949ea` |
| (2) Mengarah ke `/corporate/accounting/reconciliation` | Terpenuhi | `pathname` pada entri |
| (3) Test `accounting-reconciliation` kembali lulus | Terpenuhi | 22/22 |
| (4) Diff hanya entri itu dan impor ikonnya | Terpenuhi | `git diff --stat` |
| (5) *Perluasan 29 September 2026:* butir Tutup Tahun tampil sesudah Rekonsiliasi dan mengarah ke `/corporate/accounting/year-end-closing` | Terpenuhi di source dan di layar (29 September 2026) | Entri identik `f37e949ea`; diff +6/−0 |

| Butir DoD | Keadaan |
| --- | --- |
| Lint hijau | ✅ |
| Test terkait lulus | ✅ |
| Build owner berhasil | ✅ 29 September 2026 — termasuk perluasan (tak langsung, `.next` 10.17) |
| Klik menu di layar | Rekonsiliasi ✅ 29 September 2026 — Rizki (pernyataan owner); Tutup Tahun ✅ 29 September 2026 — tangkapan layar Rizki |
| Laporan task tertulis | ✅ |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status Git frontend | ` M src/utils/menu-sidebar/menu-items.jsx` (perluasan Tutup Tahun, di atas `5adc9f316`) |
| Risiko | `menu-items.jsx` berkas bersama; resolusi merge berikutnya dapat menghapusnya lagi. Periksa butir ini setiap kali merge integration → `RizkiV2`, dan sebutkan di deskripsi PR |
| Temuan di luar cakupan | — (Tutup Tahun kini masuk cakupan atas keputusan Rizki 29 September 2026) |
| Task berikutnya | — (kartu selesai) |
