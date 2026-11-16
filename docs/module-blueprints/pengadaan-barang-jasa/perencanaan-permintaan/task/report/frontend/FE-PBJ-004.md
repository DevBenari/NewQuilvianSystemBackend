# Laporan Perubahan Frontend — `FE-PBJ-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-004` |
| Judul | Pengajuan, batal, tarik, dan daftar turunan |
| Slice | Gelombang 3 (`MVP-1`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | `FE-PBJ-PP-10` (aksi), `-14`, `-15`, `-16`; API submit/cancel/plan-line-options; STATE §1; `BE-PBJ-007` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-001` (nama menu), `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant (tombol tanpa izin disembunyikan; angka hasil hitungan dari server) |
| Dependency | `FE-PBJ-003` ✅, `BE-PBJ-007` ✅ |
| Klasifikasi | `MEDIUM` — aksi dokumen, pilihan rencana, 3 varian daftar, butir menu |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Layar PR (`FE-PBJ-003`) sudah menyimpan draf. Backend `BE-PBJ-007` menyediakan Ajukan dengan `Idempotency-Key`, Batal/Tarik dengan alasan, dan pilihan baris rencana beserta sisanya.

---

## 2. Proses bisnis dari sisi pengguna

1. Pada baris PR, setelah barang dipilih, **Baris Rencana** menampilkan rencana unit-tahun beserta sisa, mis. "RKB-000006 v2 · sisa 0 UNIT". Bila tidak ada rencana: "Tidak ada rencana untuk barang ini; baris akan ditandai di luar rencana."
2. Bila jumlah melebihi sisa, isi **Alasan Luar Rencana**. Tanpa alasan, Ajukan ditolak dan pesan tampil tepat di bawah baris: "Baris 1: 1 UNIT di luar rencana. Isi alasannya."
3. **Ajukan** meminta konfirmasi; satu `Idempotency-Key` dipakai untuk satu niat aksi. Klik ganda pada tombol konfirmasi hanya menghasilkan satu permintaan dan satu entri riwayat "Diajukan".
4. Sesudah diajukan, kepala menampilkan nomor `PR-…`, status "Menunggu Verifikasi", tanda Mendesak/Di luar rencana (jumlah baris)/Melebihi pagu, dan kolom Dalam/Luar Rencana dari respons server.
5. **Batalkan** (draf/diajukan/dikembalikan) dan **Tarik** (menunggu persetujuan) mewajibkan alasan; tombol konfirmasi nonaktif sampai alasan diisi. Tarik menutup PR sebagai "Dibatalkan" (`PBJ-DEC-033` `G-04`).
6. Layar turunan: **Permintaan Mendesak** (terkunci `URGENT`, urut tanggal dibutuhkan, kolom sisa hari), **Riwayat Pengajuan** (semua status), **Pengajuan Menunggu Persetujuan** (terkunci `PENDING_APPROVAL`) di menu Dashboard Pengadaan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` §2–§7, `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `permission-audit-matrix.md`, DTO backend terkait, modul acuan `corporate/accounting/journal` (baris isian) dan `hr/workflow/approval-inbox-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/procurement/use-purchase-request-detail.jsx` | Ajukan (simpan dulu bila ada perubahan), batal/tarik, pilihan rencana per barang, galat per baris |
| `src/components/view/corporate/procurement/requests/purchase-request-lines.jsx` | Pilihan baris rencana + sisa, alasan luar rencana, galat di bawah baris |
| `src/components/view/corporate/procurement/requests/purchase-request-detail-view.jsx` | Tombol aksi berizin, dialog konfirmasi + alasan wajib, tanda di kepala |
| `src/components/view/corporate/procurement/requests/purchase-request-list-view.jsx` | Varian `urgent`, `history`, `pending` |
| `src/app/corporate/procurement/requests/urgent/page.jsx`, `requests/history/page.jsx`, `dashboard/pending-approvals/page.jsx` | Rute |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir Permintaan Mendesak, Riwayat Pengajuan, Pengajuan Menunggu Persetujuan |

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
| Memuat | Tombol aksi terkunci selama permintaan berjalan |
| Kosong | "Tidak ada permintaan mendesak pada saringan ini." / "Tidak ada pengajuan yang menunggu persetujuan." |
| Gagal | Pesan backend; galat ber-`lineNo` tampil di bawah baris |
| Tanpa hak akses | Tombol Ajukan/Batalkan/Tarik tersembunyi tanpa izin dan bila status tidak memungkinkan |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Request / Purchase Request

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/procurement-management/purchase-requests/{id}/submit` | Ajukan (header `Idempotency-Key`) | `ProcurementRequest : Submit` |
| `POST` | `/v1/corporate/procurement-management/purchase-requests/{id}/cancel` | Batalkan (draf/diajukan/dikembalikan) atau Tarik (menunggu persetujuan) | `ProcurementRequest : Cancel` |
| `GET` | `/v1/corporate/procurement-management/purchase-requests/plan-line-options` | Pilihan baris rencana + sisa | `ProcurementRequest : Create` |
#### Corporate / Procurement Management / Master Data / Item

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/items/options` | Pemilih katalog (cari di server) | `ProcurementItem : Read` |
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
| -10 galat luar rencana tampil di bawah baris (422) | Sesuai | `PASS` | `b-FAIL-2.png` (uji sebelum alasan diisi) |
| -10 ajukan dengan konfirmasi → PR-000014 "Menunggu Verifikasi" | Sesuai | `PASS` | `b-10-submitted.png` |
| -10 klik ganda tombol konfirmasi Ajukan → 1 POST submit, 1 riwayat "Diajukan" | Sesuai | `PASS` | `ui-results-d.json` |
| -10 Batalkan draf; tombol nonaktif sebelum alasan diisi | Sesuai | `PASS` | `ui-results-d.json` |
| -10 Tarik dari persetujuan → "Dibatalkan" (riwayat `WITHDRAW`) | Sesuai matriks status | `PASS` | `ui-results-g.json` + riwayat dokumen |
| -14 terkunci `urgencyLevel=URGENT` | Sesuai | `PASS` | `a-14-urgent.png` |
| -15 riwayat | Sesuai | `PASS` | `ui-results-a.json` |
| -16 terkunci `status=PENDING_APPROVAL` | Sesuai | `PASS` | `ui-results-a.json` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Galat `PBJ-VAL-001`..`012` tampil di dekat isian | Terpenuhi | Galat ber-`lineNo` tampil di bawah baris; galat kepala tampil di atas kartu Kepala Permintaan karena amplop galat API hanya membawa `code` dan `lineNo` (tanpa nama field) |
| (2) Klik ganda Ajukan tidak membuat dua pengajuan | Terpenuhi | 1 POST submit, 1 riwayat |
| (3) Setelah diajukan, nomor `PR-…` dan pembagian dalam/luar rencana tampil dari respons | Terpenuhi | PR-000014: Dalam Rencana 0, Luar Rencana 1 |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 9 warning lint `set-state-in-effect` |
| Masalah yang diketahui | Item daftar PR backend tidak membawa batas waktu mendesak, langkah persetujuan saat ini, maupun aksi terakhir; layar `-14` memakai tanggal dibutuhkan untuk sisa hari, `-16` dan `-15` tidak menampilkan kolom langkah/aksi terakhir (tersedia di tab Riwayat dan Persetujuan detail). Usulan delta kontrak: tambah `urgencyDeadline`, `currentStepName`, `lastActionCode` ke `PurchaseRequestListItem` |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | Keputusan delta kontrak kolom daftar |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |
