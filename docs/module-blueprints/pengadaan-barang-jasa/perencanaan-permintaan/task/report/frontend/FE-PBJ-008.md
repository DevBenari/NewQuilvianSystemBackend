# Laporan Perubahan Frontend — `FE-PBJ-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-008` |
| Judul | Konsolidasi dan rencana paket |
| Slice | Gelombang 3 (`MVP-4`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | `FE-PBJ-PP-03`, `-06`, `-07`; API §Consolidation, §Package Plan; `PBJ-VAL-050`, `-051`; `BE-PBJ-011` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-001` (nama menu), `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant (tombol tanpa izin disembunyikan; angka hasil hitungan dari server) |
| Dependency | `FE-PBJ-006` ✅, `BE-PBJ-011` ✅ |
| Klasifikasi | `MEDIUM` — konsolidasi berpilih, daftar, form/detail paket |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Backend `BE-PBJ-011` menyediakan konsolidasi rencana kebutuhan disetujui per barang beserta rincian unit, alokasi, dan sisa, serta rencana paket berbaris alokasi.

---

## 2. Proses bisnis dari sisi pengguna

1. Tim Pengadaan membuka **Rencana Pengadaan → Konsolidasi Kebutuhan Unit** (bawaan tahun depan, saringan kategori dan jenis). Tabel per barang: total, dialokasikan, sisa, nilai perkiraan.
2. **Unit (n)** membuka rincian unit asal (unit, nomor rencana dan versi, rencana, dialokasikan, sisa). Centang baris unit dan sesuaikan **Jumlah dialokasikan**. Pilihan dikunci pada satu jenis pengadaan; baris berjenis lain dinonaktifkan dengan keterangan.
3. **Buat rencana paket dari pilihan (n)** membuka form paket baru dengan baris unit terpilih (unit, barang, rencana, sisa, jumlah, harga, nilai perkiraan). Isi judul, metode indikatif, target bulan → **Simpan draf**.
4. Alokasi melebihi rencana → "Alokasi … unit … melebihi rencana (5), sudah dialokasikan 3." (`PBJ-VAL-050`) di baris.
5. **Rencana Paket Pengadaan** menampilkan daftar paket (nomor, judul, jenis, target, jumlah unit, nilai). Detail paket: **Ajukan**, **Batalkan** (alasan wajib), tab Lampiran/Persetujuan/Riwayat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` §2–§7, `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `permission-audit-matrix.md`, DTO backend terkait, modul acuan `corporate/accounting/journal` (baris isian) dan `hr/workflow/approval-inbox-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/procurement/use-plan-document-detail.jsx` | Controller generik dokumen berversi: muat, simpan `expectedRowVersion`, ajukan `Idempotency-Key`, batal, revisi (pindah ke versi baru) |
| `src/components/view/corporate/procurement/planning/plan-document-list-view.jsx` | Daftar dokumen perencanaan (varian rencana, pagu, rencana paket) |
| `src/components/view/corporate/procurement/planning/plan-consolidation-view.jsx` | Layar `-03` |
| `src/components/view/corporate/procurement/planning/package-plan-detail-view.jsx` | Layar `-07` (paket baru dari query `lines=<lineKey>:<jumlah>`) |
| `src/lib/services/corporate/procurement/package-plan-service.js` | Konsolidasi dan rencana paket |
| `src/utils/corporate/procurement/procurement-utils.jsx` | `encodeSelection`/`decodeSelection` pilihan konsolidasi |
| `src/app/corporate/procurement/planning/consolidation/page.jsx`, `planning/package-plans/**` | Rute (`new` dan `[slug]` dibungkus `Suspense` karena membaca query) |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir Konsolidasi Kebutuhan Unit dan Rencana Paket Pengadaan |

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
| Memuat | "Mengambil data konsolidasi..." / "Memuat rencana paket..." |
| Kosong | "Belum ada rencana kebutuhan yang disetujui untuk tahun ini." / "Belum ada rencana paket pada saringan ini." |
| Gagal | Pesan + **Coba lagi** |
| Tanpa hak akses | Kotak centang dan tombol buat paket nonaktif/tersembunyi tanpa `ProcurementPackagePlan : Create` |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Planning / Consolidation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/plan-consolidations?fiscalYear=&itemCategoryId=&procurementTypeId=` | Konsolidasi per barang + rincian unit | `ProcurementPackagePlan : Read` |
#### Corporate / Procurement Management / Planning / Package Plan

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/package-plans` | Daftar | `ProcurementPackagePlan : Read` |
| `GET` | `/v1/corporate/procurement-management/package-plans/summary` | Ringkasan | `ProcurementPackagePlan : Read` |
| `GET` | `/v1/corporate/procurement-management/package-plans/{id}` | Detail | `ProcurementPackagePlan : Read` |
| `GET` | `/v1/corporate/procurement-management/package-plans/{id}/status-history` | Riwayat | `ProcurementPackagePlan : Read` |
| `POST` | `/v1/corporate/procurement-management/package-plans` | Simpan draf baru | `ProcurementPackagePlan : Create` |
| `PUT` | `/v1/corporate/procurement-management/package-plans/{id}` | Ubah draf | `ProcurementPackagePlan : Update` |
| `POST` | `/v1/corporate/procurement-management/package-plans/{id}/submit` | Ajukan | `ProcurementPackagePlan : Submit` |
| `POST` | `/v1/corporate/procurement-management/package-plans/{id}/cancel` | Batalkan | `ProcurementPackagePlan : Cancel` |


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
| -03 buka unit, centang, buat paket → -07 tersimpan dengan unit asal | Sesuai | `PASS` | `a-03-selected.png`, `a-07-saved.png` |
| -06 daftar rencana paket | Sesuai | `PASS` | `ui-results-a.json` |
| -07 `PBJ-VAL-050` alokasi melebihi rencana | Sesuai | `PASS` | `d-07-val050.png` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Rincian unit asal tampil per barang | Terpenuhi | Uji fase A |
| (2) `PBJ-VAL-050`/`051` terbaca | Terpenuhi | 050 diuji di layar; 051 dicegah di layar karena pilihan dikunci satu jenis, pesan backend memakai jalur galat yang sama |
| (3) Rencana paket yang dibuat dari konsolidasi memuat unit asal | Terpenuhi | Baris paket "TEST-PBJ Radiologi Uji …" |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 9 warning lint `set-state-in-effect` |
| Masalah yang diketahui | Penanggung jawab paket (`picUserId`) belum dapat dipilih di layar karena belum ada feed pengguna; nilai yang ada dipertahankan saat ubah |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Data uji | Rencana paket draf `f594c4c2…` dibiarkan sebagai data uji |
| Langkah berikutnya | Keputusan feed pemilih penanggung jawab paket |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |
