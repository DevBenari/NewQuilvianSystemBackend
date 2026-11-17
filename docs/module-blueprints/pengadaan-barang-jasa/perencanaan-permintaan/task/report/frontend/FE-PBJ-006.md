# Laporan Perubahan Frontend — `FE-PBJ-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-006` |
| Judul | Rencana kebutuhan dan revisi |
| Slice | Gelombang 2 (`MVP-2`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | `FE-PBJ-PP-01`, `-02`, `-08` (rencana); API §Need Plan; `PBJ-VAL-040`, `-042`; `BE-PBJ-009` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-001` (nama menu), `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant (tombol tanpa izin disembunyikan; angka hasil hitungan dari server) |
| Dependency | `FE-PBJ-001` ✅, `FE-PBJ-002` ✅, `BE-PBJ-009` ✅ |
| Klasifikasi | `HEAVY` — daftar, form/detail berbaris, revisi, banding versi |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Backend `BE-PBJ-009` menyediakan rencana kebutuhan tahunan berversi per unit-tahun, kolom pemakaian oleh PR dan alokasi paket, revisi, dan banding versi.

---

## 2. Proses bisnis dari sisi pengguna

1. Penyusun unit membuka **Rencana Pengadaan → Rencana Kebutuhan Tahunan**: ringkasan (total, versi berlaku, menunggu persetujuan) dan daftar; saringan tahun, status, unit, dan "Termasuk versi lama".
2. **+ Buat Rencana**: pilih unit dan tahun (bawaan tahun depan), tambah baris barang, jumlah, harga perkiraan, bulan dibutuhkan, sumber dana, alasan. **Simpan draf**.
3. Bila unit-tahun sudah punya rencana: "Unit … sudah memiliki rencana 2027." + tombol **Buka rencana yang sudah ada** (`PBJ-VAL-040`, memakai `existingId`).
4. Detail baca menampilkan kolom "Sudah dipakai PR", "Dialokasikan paket", dan "Sisa untuk PR" dari server.
5. **Buat Revisi** (alasan wajib) membuat versi baru berstatus draf dan membuka versi itu. Menurunkan jumlah di bawah pemakaian lalu **Ajukan** menampilkan pesan di baris: "Baris TEST-PBJ Kursi Roda …: sudah dipakai 3 UNIT, tidak bisa diturunkan ke 0." (`PBJ-VAL-042`).
6. **Revisi Perencanaan** (tab Rencana Kebutuhan): semua versi dengan alasan revisi; **Banding** membuka panel versi sebelum → sesudah per baris (Ditambah/Dihapus/Diubah/Tetap) dengan jumlah, harga, dan nilai.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` §2–§7, `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `permission-audit-matrix.md`, DTO backend terkait, modul acuan `corporate/accounting/journal` (baris isian) dan `hr/workflow/approval-inbox-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/procurement/use-plan-document-detail.jsx` | Controller generik dokumen berversi: muat, simpan `expectedRowVersion`, ajukan `Idempotency-Key`, batal, revisi (pindah ke versi baru) |
| `src/components/view/corporate/procurement/planning/plan-document-list-view.jsx` | Daftar dokumen perencanaan (varian rencana, pagu, rencana paket) |
| `src/lib/hooks/corporate/procurement/use-procurement-options.jsx` | Pilihan unit, katalog, sumber dana |
| `src/components/view/corporate/procurement/planning/need-plan-detail-view.jsx` | Layar `-02` |
| `src/components/view/corporate/procurement/planning/planning-revision-view.jsx` | Layar `-08` (tab rencana + panel banding) |
| `src/lib/services/corporate/procurement/need-plan-service.js` | Panggilan rencana kebutuhan |
| `src/app/corporate/procurement/planning/need-plans/**`, `planning/revisions/page.jsx` | Rute |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir Rencana Kebutuhan Tahunan dan Revisi Perencanaan |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi mengikuti `frontend-architecture.md`: `src/app/corporate/procurement/**/page.jsx` hanya entry point dan `metadata` → view di `src/components/view/corporate/procurement/**` → hook di `src/lib/hooks/corporate/procurement/**` → service `InstanceAxios` di `src/lib/services/corporate/procurement/**`; konstanta di `src/lib/constants/corporate/procurement/procurement-constants.jsx`; fungsi murni di `src/utils/corporate/procurement/procurement-utils.jsx`; gaya di `src/style/corporate/procurement/procurement.module.css` (token saja, tanpa warna literal, tanpa `!important`, tanpa inline style). Tidak ada Redux slice baru: state layar bersifat lokal per layar dan dibaca ulang dari server (roadmap mengunci service per dokumen). Deviasi terhadap checklist yang dipertahankan: definisi kolom tabel ditulis di dalam view (bukan berkas `-table-columns.jsx` terpisah) dan layar form/detail memakai susunan `COMPOSE` (lihat keputusan di bawah). Aturan tipografi pada `procurement.module.css` hanya menyasar kelas fitur (judul kartu, teks redup, tabel baris), memakai token, dan tidak menimpa komponen shared.

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul halaman dan aksi utama | `REUSE` | `Hero` (`actions`) |
| Kartu ringkasan angka | `REUSE` | `SummaryGrid` |
| Saringan, pencarian, atur ulang | `REUSE` | `DataFilter` + `FilterSelect` |
| Tabel daftar dan paginasi | `REUSE` | `DataTable` + `Pagination` lewat `PaginationComponent` |
| Label status dokumen | `WRAP` | `StatusBadge` dibungkus `procurement-status-badge.jsx` (peta kode status → label Bahasa Indonesia + nada warna; teks selalu tampil, BRD B10) |
| Konfirmasi dan alasan wajib | `REUSE` | `ConfirmModal` (`requireReason`) |
| Notifikasi hasil aksi | `REUSE` | `ToastStack` |
| Pesan galat / info | `REUSE` | `InformationAlert`, `AccessDeniedGate` |
| Tombol | `REUSE` | `BaseButton` |
| Isian form | `REUSE` | `BaseTextField`, `BaseInputField`, `BaseSelectField`, `BaseDateField`, `BaseTextAreaField`, `BaseCheckboxField` |
| Tabel baris dokumen yang dapat diisi | `COMPOSE` | `<table>` + field base di dalam sel, mengikuti `journal-form-view.jsx` (Akuntansi); tabel baca-saja memakai `data-flat-table="true"` |
| Susunan halaman form/detail dokumen | `COMPOSE` | `Hero` + kartu bagian (`procurement.module.css`) + `ConfirmModal` + `ToastStack` |
| Tab Lampiran / Persetujuan / Riwayat | `COMPOSE` | `BaseButton` ber-`role="tab"` (belum ada base Tabs; pola sama dengan layar Akuntansi) |

Pilihan untuk elemen bukan `REUSE`:

1. **Susunan form/detail dokumen**
   1. `COMPOSE` Hero + kartu bagian, seperti yang dibangun **(rekomendasi)** — satu layar melayani mode draf (ubah) dan mode baca/verifikasi beserta tab alur; konsistensi visual terjaga karena semua bagian memakai token dan base field; risiko regresi nol pada komponen shared; biaya rendah.
   2. Migrasi ke `BaseDetailView` dengan isi editor sebagai `children` — lebih seragam dengan layar detail Akuntansi, tetapi mode ubah perlu dipisah ke `BaseEditorView` sehingga satu dokumen punya dua layar; biaya sedang.
2. **Tabel baris yang dapat diisi**
   1. `COMPOSE` tabel + field base **(rekomendasi)** — sama dengan baris jurnal; tidak menyentuh `DataTable`.
   2. `EXTEND` `DataTable` agar menerima sel isian — menyentuh komponen shared; risiko regresi di semua modul.
3. **Tab**
   1. `COMPOSE` `BaseButton role="tab"` **(rekomendasi)** — tanpa komponen baru.
   2. `NEW` komponen Tabs bersama — butuh keputusan pengguna; tidak dikerjakan.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` yang mengubah perilaku bawaan base component.


---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil data..." / "Memuat rencana..." / "Memuat versi..." |
| Kosong | "Belum ada rencana kebutuhan pada saringan ini." / "Rencana ini baru punya satu versi." |
| Gagal | "Data gagal dimuat." + **Coba lagi** |
| Tanpa hak akses | `AccessDeniedGate`; tombol Simpan/Ajukan/Buat Revisi/Batalkan tersembunyi tanpa izin |
| Data basi / di luar cakupan | Pesan + **Muat ulang** / "Rencana tidak ditemukan atau bukan milik unit Anda." |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Planning / Need Plan

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/need-plans` | Daftar (versi berlaku / termasuk versi lama) | `ProcurementNeedPlan : Read` |
| `GET` | `/v1/corporate/procurement-management/need-plans/summary` | Ringkasan | `ProcurementNeedPlan : Read` |
| `GET` | `/v1/corporate/procurement-management/need-plans/{id}` | Detail beserta kolom terpakai/dialokasikan/sisa | `ProcurementNeedPlan : Read` |
| `GET` | `/v1/corporate/procurement-management/need-plans/{id}/status-history` | Riwayat | `ProcurementNeedPlan : Read` |
| `GET` | `/v1/corporate/procurement-management/need-plans/{id}/versions` | Daftar versi | `ProcurementNeedPlan : Read` |
| `GET` | `/v1/corporate/procurement-management/need-plans/{id}/compare/{otherId}` | Banding versi per baris | `ProcurementNeedPlan : Read` |
| `POST` | `/v1/corporate/procurement-management/need-plans` | Simpan draf baru | `ProcurementNeedPlan : Create` |
| `PUT` | `/v1/corporate/procurement-management/need-plans/{id}` | Ubah draf | `ProcurementNeedPlan : Update` |
| `POST` | `/v1/corporate/procurement-management/need-plans/{id}/submit` | Ajukan (`Idempotency-Key`) | `ProcurementNeedPlan : Submit` |
| `POST` | `/v1/corporate/procurement-management/need-plans/{id}/cancel` | Batalkan | `ProcurementNeedPlan : Cancel` |
| `POST` | `/v1/corporate/procurement-management/need-plans/{id}/revisions` | Buat Revisi | `ProcurementNeedPlan : Revise` |
#### Corporate / Human Resource / Master Data / Organization

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/organization/departments/options` | Pilihan unit (cadangan: unit akun) | `Organization : Read` |


---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000`; panggilan API `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` (sumber `sukmagp` berisi BE-PBJ-002..011) dengan database `QuilvianNewDevSukma`. Aktor uji non-SuperAdmin `TEST-PBJ … 20261116110649` (pengusul IGD `reqA`, kepala IGD `headA`, Tim Pengadaan `proc`, Manajer `mgr`, Direktur `dirk`, Keuangan `keu`) memakai bundel izin `permission-audit-matrix.md` §3. Tangkapan layar tersimpan di folder `shots/` scratchpad sesi.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada berkas Pengadaan | 0 error, 9 warning `react-hooks/set-state-in-effect` (pola muat-data di efek, sama dengan modul lain) | `PASS` | keluaran perintah, 16 November 2026 |
| `npm run lint:errors` (seluruh repo) | exit 0 | `PASS` | `fe-lint3.log` scratchpad |
| `node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 4800 test, 4749 lulus, 51 gagal — seluruhnya modul lain; 8 berkas gagal yang membaca menu diuji A/B dengan `menu-items.jsx` versi `HEAD` dan hasilnya identik; tidak ada test yang mengimpor berkas Pengadaan | `EXISTING / ENVIRONMENT ISSUE` | baseline 28 Okt 2026 tercatat 41 gagal di modul lain |
| `npm run build` | `Compiled successfully`; 24 rute `/corporate/procurement/**` terbentuk | `PASS` | `fe-build4.log` scratchpad |
| `AUTOMATED TEST` | `SKIPPED (opsional) — test-policy: test baru tidak wajib; logika murni berada di procurement-utils.jsx` | `NOT RUN` | — |
| -01 daftar + -02 detail dengan kolom pemakaian | Sesuai | `PASS` | `a-02-needplan-detail.png` |
| -08 banding v1 → v2 (Rp 509.000.000 → Rp 609.000.000, alasan revisi) | Sesuai | `PASS` | `a-08-revisions.png` |
| -02 `PBJ-VAL-040` + tautan ke rencana yang ada | Sesuai | `PASS` | `e-02-val040.png` |
| -02 Buat Revisi → versi draf baru; `PBJ-VAL-042` di baris | Sesuai | `PASS` | `d-02-val042.png` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Rencana kedua unit-tahun sama → pesan `PBJ-VAL-040` dengan tautan | Terpenuhi | Uji fase E |
| (2) Banding versi menampilkan sebelum → sesudah per baris | Terpenuhi | Uji fase A |
| (3) `PBJ-VAL-042` tampil di baris yang bermasalah | Terpenuhi | Uji fase D (awalan "Baris 1:" ganda sudah dirapikan sesudah uji) |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 9 warning lint `set-state-in-effect` |
| Masalah yang diketahui | Pengusul tanpa `Organization : Read` hanya dapat memilih unit akunnya sendiri (sesuai cakupan pengusul) |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Data uji | Revisi draf `f57dbe75…` (v3 dari RKB-000006) dibiarkan sebagai data uji |
| Langkah berikutnya | — |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |
