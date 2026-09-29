# Laporan Perubahan Frontend — `FE-ACC-P2-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-017` |
| Judul | Pulihkan butir menu Rekonsiliasi Control Account |
| Slice | Wave D — pasangan `BE-ACC-P2-014` |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-017` (revisi 7, `APPROVED` Rizki 28 September 2026) |
| Trace | `03-frontend-architecture.md` bagian 9 butir 7; `FE-ACC-P2-008`; temuan 28 September 2026 (butir hilang lewat merge `8f01cf06c`) |
| Contract version | Nol API. Layar tujuan dijaga `AccountingReconciliation : Read` seperti sebelumnya |
| Wewenang UI | Entri dipulihkan **persis** seperti `f6b1498fe`; tidak ada keputusan rupa baru |
| Dependency | — |
| Klasifikasi | `LIGHT` — skor 1: satu berkas diubah, nol logika bisnis, nol kontrak, satu butir menu |
| Task mode | `FRONTEND` (Langkah D, berpasangan dengan `BE-ACC-P2-014` atas perintah Rizki 28 September 2026) |
| Target tulis | `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `a6d269077` (branch `RizkiV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `485d8b8b` (branch `rizkiG`) |
| Tanggal | 28 September 2026 |
| Status | **🟡 SEBAGIAN — 28 September 2026.** 4 dari 4 acceptance terpenuhi di source dan diperiksa: eslint 0/0, unit test `accounting-reconciliation` 22/22; `npm run build` Rizki 29 September 2026 berhasil (Compiled 53 detik, 411/411 halaman, postbuild standalone siap). **Belum:** klik butir menu di layar |

## 1. Keadaan yang ditemukan di awal

Butir "Rekonsiliasi Control Account" ditambahkan `f6b1498fe` dan masih ada pada `f37e949ea`
(15 September 2026). Resolusi merge **`8f01cf06c`** ("merge dengan branch rizki", 18 September
2026) menghapusnya bersama impor ikon `RiSafeLine`. Route `/corporate/accounting/reconciliation`
tetap terbangun, sehingga layar hanya terjangkau lewat alamat langsung, dan test "route, menu, dan
store terdaftar" pada `tests/unit/accounting-reconciliation.test.mjs` gagal.

**Temuan tambahan di luar kartu:** merge yang sama juga menghapus butir **"Tutup Tahun"**
(`corporateAccountingYearEndClosing`, ikon `RiBookletLine`, `/corporate/accounting/year-end-closing`).
Butir itu **tidak** dipulihkan karena kartu hanya mencakup Rekonsiliasi — lihat bagian 8.

## 2. Proses bisnis dari sisi pengguna

1. Petugas akuntansi membuka menu Accounting di sidebar.
2. Sesudah Neraca Saldo tampil butir **Rekonsiliasi Control Account**.
3. Klik butir itu membuka layar Rekonsiliasi. Pengguna tanpa hak `AccountingReconciliation : Read`
   tetap melihat penolakan akses dari layar, sama seperti sebelum butir hilang.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `src/utils/menu-sidebar/menu-items.jsx` pada `HEAD`, `f37e949ea`, `f6b1498fe` | Bentuk asli entri, letaknya, dan impor ikon |
| `git log -m -S corporateAccountingReconciliation` | Commit yang menambah dan menghapus butir |
| `tests/unit/accounting-reconciliation.test.mjs`; `package.json` | Test yang menjaga butir menu; cara menjalankan unit test |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | + impor `RiSafeLine`; + entri `{ label: "Rekonsiliasi Control Account", key: "corporateAccountingReconciliation", icon: <RiSafeLine className="fs-4" />, pathname: "/corporate/accounting/reconciliation" }` sesudah Neraca Saldo. +7/−0 baris |

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
| Klik butir menu di layar | Menunggu Rizki, bersama uji `FE-ACC-P2-016` | `NOT RUN` | — |
| `AUTOMATED TEST` | Test yang sudah ada dijalankan; nol test baru (opsional) | `PASS` | Baris di atas |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Butir tampil di menu Accounting pada letak semula | Terpenuhi di source; **layar belum** | Entri sesudah Neraca Saldo — sama dengan urutan `f37e949ea` |
| (2) Mengarah ke `/corporate/accounting/reconciliation` | Terpenuhi | `pathname` pada entri |
| (3) Test `accounting-reconciliation` kembali lulus | Terpenuhi | 22/22 |
| (4) Diff hanya entri itu dan impor ikonnya | Terpenuhi | `git diff --stat` |

| Butir DoD | Keadaan |
| --- | --- |
| Lint hijau | ✅ |
| Test terkait lulus | ✅ |
| Build owner berhasil | ✅ 29 September 2026 |
| Laporan task tertulis | ✅ |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status Git frontend | ` M src/utils/menu-sidebar/menu-items.jsx` |
| Risiko | `menu-items.jsx` berkas bersama; resolusi merge berikutnya dapat menghapusnya lagi. Periksa butir ini setiap kali merge integration → `RizkiV2`, dan sebutkan di deskripsi PR |
| Temuan di luar cakupan | Butir **Tutup Tahun** hilang lewat merge yang sama — pemulihannya butuh keputusan Rizki (perluasan kartu ini atau kartu baru) |
| Task berikutnya | `FE-ACC-P2-016`, lalu uji sekali bersama `BE-ACC-P2-014` |
