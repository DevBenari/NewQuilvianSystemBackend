# Laporan Perubahan Frontend — `FE-AST-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-015` |
| Judul | Jadwal dan detail sesi |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/frontend-roadmap.md` — kartu `FE-AST-015` |
| Trace | `CAP-07.1`; `SCR-AST-INV-01` (Jadwal Inventaris), `SCR-AST-INV-08` (Detail Sesi); `03-frontend-architecture.md` §1–2; `02-module-map.md` §3.1 (grup Inventaris & Pemeriksaan Fisik) |
| Contract version | `AST-INV-1.0.0`; layar mengikuti **source** `BE-AST-021` termasuk deltanya (`filters/metadata`, `summary`, `startBlockers`, body `{ rowVersion }` pada mulai) |
| Wewenang UI | Tata letak `DEV_DISCRETION`; menu wajib (kriteria 3) |
| Dependency | `BE-AST-021` ✅; `FE-AST-001` [FE-REG] 🟡 (sisa pilih-aset; tidak dipakai layar ini) |
| Klasifikasi | `STANDARD` — 4 route, 5 view/kolom, 5 hook, 1 service, 1 konstanta, 1 utilitas; tanpa CSS baru |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-23 (pasangan `BE-AST-021`) |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `4fd83da04` (branch `RizkiV2`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — ketiga kriteria terpetakan ke source dan teruji di layar: Playwright **16/16**; `npm run build` Rizki berhasil (`BUILD_ID` 2026-11-23 11:25:04) |

---

## 1. Proses bisnis dari sisi pengguna

| Layar | Siapa | Yang dilihat / dilakukan |
|---|---|---|
| **Jadwal Inventaris** (`/corporate/asset-management/stock-take/schedules`) | Pemegang `AssetStockTake : Read` | Kartu Dijadwalkan/Berjalan/Pencocokan/Menunggu Verifikasi/Tim Saya, tab per status + Tim Saya, saringan unit dan tahun, tabel sesi (nomor, unit/ruangan, rencana, ketua + jumlah tim, dipindai/diharapkan, status + tanda Lewat Jadwal). Tombol **+ Jadwalkan Inventarisasi** bila berhak `Create` |
| **Jadwalkan / Ubah Jadwal** (`…/schedules/create`, `…/[slug]/update`) | `AssetStockTake : Create` | Unit, ruangan opsional (hanya ruangan unit itu), tanggal mulai/selesai, catatan; bagian **Tim Inventarisasi**: pilih pegawai → Tambah ke Tim; anggota pertama otomatis ketua, ganti lewat menu baris "Jadikan ketua", "Hapus dari tim" |
| **Detail Sesi** (`…/schedules/[slug]`) | Unit sesi, penjadwal, anggota tim, atau hak seluruh unit | Ringkasan, peran pengguna (ketua/anggota), tabel tim, tombol Mulai Sesi / Ubah Jadwal / Batalkan Sesi sesuai `availableActions` × hak. Sebelum mulai, peringatan kuning menyebut proses penahan dan tanggal paling awal; bila mulai ditolak, alasan dari backend tampil sebagai alert merah dan toast |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/app/corporate/asset-management/stock-take/schedules/{page,create/page,[slug]/page,[slug]/update/page}.jsx` | Baru — 4 route |
| `src/components/view/corporate/asset-management/stock-take/asset-stock-take-{list-view,detail-view,form-view,table-columns,action-modals}.jsx` | Baru |
| `src/lib/hooks/corporate/asset-management/stock-take/use-asset-stock-take-{route,list,detail,editor,actions}.jsx` | Baru |
| `src/lib/services/corporate/asset-management/asset-stock-take-service.js` | Baru — 7 panggilan API |
| `src/lib/constants/corporate/asset-management/stock-take/asset-stock-take-constants.jsx` | Baru — rute, tab, kartu, field form, peta galat→field |
| `src/utils/corporate/asset-management/stock-take/asset-stock-take-utils.js` | Baru — parameter daftar, validasi form, payload, uraian penahan |
| `src/utils/corporate/asset-management/asset-status-labels.js` | + enum `stockTakeStatus` (label + nada) |
| `src/utils/menu-sidebar/menu-items.jsx` | + grup **Inventaris & Pemeriksaan Fisik** (`RiSurveyLine`) dengan butir **Jadwal Inventaris** (`AssetStockTake : Read`), di antara Kondisi & Status Aset dan Pemanfaatan & Pengamanan Aset sesuai module map; lima butir lain grup ini menyusul `FE-AST-016..017` |

Tanpa CSS baru; tanpa baris komentar.

### 2.1 Keputusan komponen

| Elemen | Base component | Status |
|---|---|---|
| Hero, kartu ringkasan, saringan + tab, tabel + paginasi | `Hero`, `SummaryCards`, `DataFilter`, `DataTable`, `Pagination` | `REUSE` |
| Saringan unit, pilih pegawai | `AssetMasterResourceFilter` (`ResourceFilterSelect`) | `REUSE` |
| Saringan tahun, jumlah baris | `FilterSelect` | `REUSE` |
| Status + Lewat Jadwal, Ketua Tim | `StatusBadge` | `REUSE` |
| Aksi baris | `RowActionMenu` | `REUSE` |
| Form jadwal (unit, ruangan bergantung unit, tanggal, catatan) | `BaseGroupedEditorView` | `REUSE` |
| Rincian sesi + bagian tim | `BaseDetailView`, `BaseDetailSection` | `REUSE` |
| Konfirmasi mulai, batal beralasan | `ConfirmModal` (`requireReason`) | `REUSE` |
| Peringatan penahan, galat | `InformationAlert`, `ToastStack` | `REUSE` |
| Penyusun tim | `DataFilter` + `AssetMasterResourceFilter` + `BaseButton` + `DataTable` + `RowActionMenu` | `COMPOSE` |

Penyusun tim (`COMPOSE`):
1. **Rangkai dari base component yang ada** (dipilih, rekomendasi): rupa sama dengan pemilih aset di form mutasi; tanpa
   risiko regresi pada komponen bersama; biaya kecil.
2. Multi-select pegawai baru dengan penanda ketua: konsisten di satu kontrol, tetapi komponen baru (`NEW`) yang harus
   disetujui dan dirawat; tidak ada pemakai lain saat ini.

`UI GATE: 10 elemen — REUSE 9, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0`

---

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint 19 berkas | 0 galat, 0 peringatan (satu peringatan React Compiler `preserve-manual-memoization` diperbaiki dengan pola `currentRowVersion` seperti form pemanfaatan) |
| `npm run build` (Rizki) | Berhasil, `BUILD_ID` 2026-11-23 11:25:04; `app-path-routes-manifest.json` memuat 4 route `stock-take/schedules` |
| Grep anti-regresi | Tanpa nilai warna/ukuran literal, tanpa CSS baru, tanpa override tipografi |

**MANUAL TEST (Playwright, Edge headless, SuperAdmin + LOKET): 16/16**
— `QuilvianSystemFrontendDev/test-with-agy/asset-management/fe-ast-015-20261123/uji-fe-015.mjs`, hasil `hasil-uji.jsonl`,
tangkapan layar `01..08-*.png`. Percobaan pertama berhenti sesudah langkah 2 karena skrip menunggu event `load` pada
navigasi `router.push`; `probe-create.mjs` membuktikan navigasi berhasil, skrip diubah ke `waitUntil: "commit"`.
Percobaan kedua tertunda karena disk C: penuh, lalu dijalankan ulang sesudah dibersihkan.

| Langkah | Hasil |
| --- | --- |
| Menu grup + butir tampil; daftar memuat 200 | Lulus |
| Simpan form kosong → pesan unit dan tim di layar | Lulus |
| Isi IGD, hari ini–hari ini, dua pegawai; anggota pertama ketua; "Jadikan ketua" memindahkan ketua | Lulus |
| Simpan → `POST` membawa unit IGD, 2 anggota, tepat 1 ketua; ketua dari respons = pegawai yang dipilih; pindah ke detail `AST-OPN-000009` | Lulus |
| Detail: nomor, tim, ketua, tombol Mulai Sesi dan Ubah Jadwal | Lulus |
| Mutasi IGD `AST-MUT-000016` dibuat lewat API → muat ulang detail: peringatan menyebut nomornya | Lulus |
| Mulai Sesi → 409 `AST-VAL-INV-006`; toast "Sesi Belum Dapat Dimulai" dan alert merah menyebut `AST-MUT-000016` | Lulus |
| Ubah Jadwal terisi data lama; hapus satu anggota, ubah catatan → `PUT` dengan `rowVersion`, 1 anggota yang juga ketua | Lulus |
| Mulai Sesi → teks konfirmasi menyebut pembekuan dan penahanan; 200; status Berjalan; tombol Mulai/Ubah hilang | Lulus |
| Batalkan Sesi dengan alasan → 200, alasan tampil, tombol Batalkan hilang | Lulus |
| Tab Dibatalkan → permintaan `status=Dibatalkan`, baris sesi ada | Lulus |
| LOKET membuka Jadwal Inventaris → "Ups! Akses Ditolak"; grup Manajemen Aset tidak ada di menunya | Lulus |
| Tanpa galat halaman dan tanpa respons 5xx | Lulus |

`AUTOMATED TEST: SKIPPED (opsional) — repository tanpa framework test; bukti = Playwright di atas.`

Data uji dirapikan: sesi `AST-OPN-000009` `Dibatalkan`, mutasi `AST-MUT-000016` dibatalkan.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Jadwalkan dengan unit, ruangan opsional, tanggal, tim dan ketua | `asset-stock-take-form-view.jsx`, `use-asset-stock-take-editor.jsx` | Langkah form + `POST` | ✅ |
| 2. Mulai menampilkan daftar proses yang menahan bila ditolak | `asset-stock-take-detail-view.jsx` (`startBlockers`, `startError`), `use-asset-stock-take-actions.jsx` | Peringatan sebelum mulai + 409 di layar | ✅ |
| 3. Grup menu "Inventaris & Pemeriksaan Fisik" dan butir "Jadwal Inventaris" terdaftar | `menu-items.jsx` | Langkah menu; LOKET tidak melihatnya | ✅ |

| DoD | Status |
|---|---|
| `V-FE` lint, build, uji layar | Terpenuhi |
| `UAT-INV-02` | Milik tim UAT |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `BE-AST-021` |

---

## 5. Catatan

- Tampilan ruangan pada form mengikuti `dependsOn` unit; ruangan unit lain ditolak backend (`AST-VAL-INV-004`) dan
  pesannya dipetakan ke field ruangan.
- Butir menu Pemeriksaan Fisik, Pencocokan Data, Selisih Inventaris, Verifikasi Hasil, dan Berita Acara ditambahkan
  bersama layarnya di `FE-AST-016`/`FE-AST-017`.
- Task berikutnya: `BE-AST-022` + `FE-AST-016` (pemindaian QR).
