# Laporan Perubahan Frontend — `FE-PBJ-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-007` |
| Judul | Pagu |
| Slice | Gelombang 3 (`MVP-3`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | `FE-PBJ-PP-04`, `-05`, `-08` (pagu); API §Budget; `PBJ-VAL-060`, `-062`; `BE-PBJ-010` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-001` (nama menu), `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant (tombol tanpa izin disembunyikan; angka hasil hitungan dari server) |
| Dependency | `FE-PBJ-006` ✅, `BE-PBJ-010` ✅ |
| Klasifikasi | `MEDIUM` — daftar, form/detail, bagian revisi |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Backend `BE-PBJ-010` menyediakan pagu berversi per unit × kategori × sumber dana × tahun, komitmen dari PR disetujui, sisa pagu, dan revisi.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas Keuangan membuka **Rencana Pengadaan → Rencana Anggaran**: ringkasan (total, menunggu persetujuan, pagu berlaku, sisa) dan daftar dengan kolom Pagu dan Sisa dari server.
2. **+ Buat Pagu**: unit, tahun, kategori, sumber dana, nilai → **Simpan draf** → **Ajukan**.
3. Kombinasi yang sudah ada: "Pagu untuk kombinasi ini sudah ada." + **Buka pagu yang sudah ada** (`PBJ-VAL-060`).
4. Detail menampilkan Nilai Pagu, Terpakai (komitmen), dan Sisa Pagu dari server.
5. **Buat Revisi** (alasan wajib) → versi draf baru; nilai di bawah komitmen lalu **Ajukan** → "Pagu tidak boleh lebih kecil dari komitmen yang sudah berjalan (Rp 10.000.000)." (`PBJ-VAL-062`).
6. **Lihat versi** membuka tab Pagu di Revisi Perencanaan. Pengguna tanpa `ProcurementBudget : Read` tidak melihat butir menu, tab pagu, maupun ringkasan sisa pagu di PR.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` §2–§7, `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `permission-audit-matrix.md`, DTO backend terkait, modul acuan `corporate/accounting/journal` (baris isian) dan `hr/workflow/approval-inbox-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/procurement/use-plan-document-detail.jsx` | Controller generik dokumen berversi: muat, simpan `expectedRowVersion`, ajukan `Idempotency-Key`, batal, revisi (pindah ke versi baru) |
| `src/components/view/corporate/procurement/planning/plan-document-list-view.jsx` | Daftar dokumen perencanaan (varian rencana, pagu, rencana paket) |
| `src/components/view/corporate/procurement/planning/budget-detail-view.jsx` | Layar `-05` |
| `src/components/view/corporate/procurement/planning/planning-revision-view.jsx` | Tab Pagu (disembunyikan tanpa `ProcurementBudget : Read`) |
| `src/lib/services/corporate/procurement/budget-service.js` | Panggilan pagu |
| `src/app/corporate/procurement/planning/budgets/**` | Rute |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir Rencana Anggaran |

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
| Memuat | "Mengambil data..." / "Memuat pagu..." |
| Kosong | "Belum ada pagu pada saringan ini." |
| Gagal | "Data gagal dimuat." + **Coba lagi** |
| Tanpa hak akses | Butir menu dan nominal tersembunyi tanpa `ProcurementBudget : Read` |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Planning / Budget

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/budgets` | Daftar | `ProcurementBudget : Read` |
| `GET` | `/v1/corporate/procurement-management/budgets/summary` | Ringkasan | `ProcurementBudget : Read` |
| `GET` | `/v1/corporate/procurement-management/budgets/{id}` | Detail | `ProcurementBudget : Read` |
| `GET` | `/v1/corporate/procurement-management/budgets/{id}/status-history` | Riwayat | `ProcurementBudget : Read` |
| `POST` | `/v1/corporate/procurement-management/budgets` | Simpan draf baru | `ProcurementBudget : Create` |
| `PUT` | `/v1/corporate/procurement-management/budgets/{id}` | Ubah draf | `ProcurementBudget : Update` |
| `POST` | `/v1/corporate/procurement-management/budgets/{id}/submit` | Ajukan | `ProcurementBudget : Submit` |
| `POST` | `/v1/corporate/procurement-management/budgets/{id}/revisions` | Buat Revisi | `ProcurementBudget : Revise` |
#### Corporate / Procurement Management / Master Data / Item Category

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/item-categories/options` | Pilihan kategori | `ProcurementItemCategory : Read` |
#### Corporate / Procurement Management / Master Data / Fund Source

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/fund-sources/options` | Pilihan sumber dana | `ProcurementFundSource : Read` |


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
| -04 daftar + -05 detail sisa pagu | Sesuai | `PASS` | `a-05-budget.png` |
| -08 tab pagu (versi termasuk lama) | Sesuai | `PASS` | `ui-results-a.json` |
| -05 `PBJ-VAL-060` + tautan | Sesuai | `PASS` | `e-05-val060.png` |
| -05 Buat Revisi + `PBJ-VAL-062` | Sesuai | `PASS` | `h-05-val062.png` |
| Pengusul tanpa `ProcurementBudget : Read`: ringkasan sisa pagu di PR tersembunyi, `budgets/remaining` tidak dipanggil | Sesuai | `PASS` | jejak API `ui-results-b.json` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kolom sisa = respons server | Terpenuhi | Kolom `remainingAmount` ditampilkan apa adanya |
| (2) `PBJ-VAL-060`/`062` terbaca | Terpenuhi | Uji fase E dan H |
| (3) Nominal tidak tampil bagi pengguna tanpa `ProcurementBudget : Read` | Terpenuhi | Menu, tab, dan ringkasan tersembunyi |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 9 warning lint `set-state-in-effect` |
| Masalah yang diketahui | Peran Keuangan tanpa `Organization : Read` hanya dapat memilih unit akunnya sendiri saat membuat pagu (temuan izin `FE-PBJ-003` §8); backend belum punya saringan pagu per `lineageKey`, sehingga "Lihat versi" menyaring di halaman yang terbaca |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Data uji | Pagu draf `b4d0bc1c…` dan revisi draf `ddbfd3f4…` dibiarkan sebagai data uji |
| Langkah berikutnya | Keputusan izin unit untuk Keuangan |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |
