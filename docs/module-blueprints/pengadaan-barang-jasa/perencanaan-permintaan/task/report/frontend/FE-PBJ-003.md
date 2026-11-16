# Laporan Perubahan Frontend — `FE-PBJ-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-003` |
| Judul | Daftar dan draft PR |
| Slice | Gelombang 2 (`MVP-1`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | `FE-PBJ-PP-09`, `-10` (draf); API §Purchase Request; `AT-PP-08`; `BE-PBJ-006` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-001` (nama menu), `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant (tombol tanpa izin disembunyikan; angka hasil hitungan dari server) |
| Dependency | `FE-PBJ-001` ✅, `FE-PBJ-002` ✅, `BE-PBJ-006` ✅ |
| Klasifikasi | `HEAVY` — layar daftar 4 varian, form/detail dengan baris isian, hook detail, 409/404 |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Belum ada layar PR. Backend menyediakan daftar ber-cakupan unit, detail, simpan/ubah draf dengan `expectedRowVersion` (409 `PBJ-VAL-090` bila basi), riwayat, dan 404 untuk PR di luar cakupan.

---

## 2. Proses bisnis dari sisi pengguna

1. Pengusul unit membuka **Permintaan Pengadaan → Pengajuan Pembelian Barang & Jasa**. Ringkasan dan daftar PR unitnya tampil; saringan unit hanya berisi unit dalam cakupan pengguna.
2. **+ Buat PR** membuka form. Unit terisi dari akun ("Unit akun Anda" bila nama unit tidak ada di sesi). Pengguna memilih tahun anggaran, sumber dana, tanggal dibutuhkan, urgensi (Mendesak menambah alasan dan batas waktu), dan justifikasi.
3. **+ Tambah baris**: cari barang di katalog, isi spesifikasi, jumlah (satuan tampil di label), harga perkiraan. Nilai baris diberi label "perkiraan"; total resmi dari server setelah disimpan.
4. **Simpan draf** → layar pindah ke alamat PR; status "Draf" dan PR muncul di daftar saringan Draf.
5. Data basi: bila PR diubah di tempat lain, Simpan menampilkan "Data sudah diubah orang lain…" + **Muat ulang**; isian di layar tidak dibuang sampai pengguna memuat ulang.
6. Tautan PR unit lain atau id tidak dikenal → "Pengajuan tidak ditemukan atau bukan milik unit Anda." Alamat yang bukan GUID langsung ke halaman not-found tanpa memanggil API.
7. Tab **Lampiran** (`FE-PBJ-002`) dan **Riwayat** (linimasa aksi beserta pelaku).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` §2–§7, `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `permission-audit-matrix.md`, DTO backend terkait, modul acuan `corporate/accounting/journal` (baris isian) dan `hr/workflow/approval-inbox-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/corporate/procurement/purchase-request-service.js` | Seluruh panggilan PR dan verifikasi |
| `src/lib/hooks/corporate/procurement/use-purchase-request-detail.jsx` | Controller form/detail: muat, simpan dengan `expectedRowVersion`, basi, galat per baris, kunci idempoten per niat aksi |
| `src/lib/hooks/corporate/procurement/use-procurement-options.jsx` | Pilihan referensi, katalog, unit, dan saringan unit dalam cakupan |
| `src/components/view/corporate/procurement/requests/purchase-request-list-view.jsx` | Daftar PR (varian `requests`, `urgent`, `history`, `pending`, `verification`, `validation`) |
| `src/components/view/corporate/procurement/requests/purchase-request-detail-view.jsx` | Form/detail PR dan ruang kerja verifikasi |
| `src/components/view/corporate/procurement/requests/purchase-request-lines.jsx` | Baris PR (isian, baca, verifikasi) |
| `src/components/view/corporate/procurement/shared/procurement-status-badge.jsx`, `procurement-history-panel.jsx`, `procurement-document-tabs.jsx` | Label status, linimasa, tab |
| `src/app/corporate/procurement/requests/page.jsx`, `requests/new/page.jsx`, `requests/[slug]/page.jsx` | Rute; `[slug]` menolak token non-GUID dengan `notFound()` |

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
| Memuat | "Mengambil data pengajuan..." / "Memuat pengajuan..." |
| Kosong | "Belum ada pengajuan pada saringan ini." |
| Gagal | "Data pengajuan gagal dimuat." + **Coba lagi** |
| Tanpa hak akses | `AccessDeniedGate`; tombol Buat PR/Simpan tersembunyi tanpa izin |
| Data basi (409) | "Data sudah diubah orang lain…" + **Muat ulang**, isian dipertahankan |
| Di luar cakupan (404) | "Pengajuan tidak ditemukan atau bukan milik unit Anda." |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Request / Purchase Request

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/purchase-requests` | Daftar ber-saringan (unit, status, tahun, urgensi; terkunci per varian) | `ProcurementRequest : Read` |
| `GET` | `/v1/corporate/procurement-management/purchase-requests/summary` | Ringkasan total/mendesak/luar rencana/lewat pagu | `ProcurementRequest : Read` |
| `GET` | `/v1/corporate/procurement-management/purchase-requests/{id}` | Detail | `ProcurementRequest : Read` |
| `GET` | `/v1/corporate/procurement-management/purchase-requests/{id}/status-history` | Tab Riwayat | `ProcurementRequest : Read` |
| `POST` | `/v1/corporate/procurement-management/purchase-requests` | Simpan draf baru | `ProcurementRequest : Create` |
| `PUT` | `/v1/corporate/procurement-management/purchase-requests/{id}` | Ubah draf dengan `expectedRowVersion` | `ProcurementRequest : Update` |
#### Corporate / Procurement Management / Master Data / Item

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/items/options` | Pemilih katalog | `ProcurementItem : Read` |


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
| -09 daftar unit sendiri (23 baris) | Sesuai | `PASS` | `a-09-list.png` |
| -09 saringan Draf → `status=DRAFT` terkirim, semua baris Draf | Sesuai | `PASS` | `ui-results-a.json` |
| -10 buat draf lewat form (tahun, sumber dana, tanggal, barang, jumlah, harga, baris rencana) | PR tersimpan | `PASS` | `b-10-saved.png` (izin baca sumber dana diberikan sementara ke jabatan uji, lihat §8) |
| -10 data basi 409 + Muat ulang | Isian dipertahankan; muat ulang mengambil versi server | `PASS` | `b-10-stale.png` |
| -10 404 di luar cakupan | Pesan tampil | `PASS` | `ui-results-a.json` |
| Token rute non-GUID | Halaman not-found aplikasi, API tidak dipanggil | `PASS` | `ui-results-a.json` |
| -10 sumber dana tanpa hak baca | Pesan izin tampil | `PASS` | `a-10-new-nofund.png` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Saringan unit hanya menampilkan unit dalam cakupan | Terpenuhi | Pilihan dibentuk dari baris ber-cakupan server; pengusul IGD hanya melihat IGD |
| (2) Simpan draft → muncul di daftar sebagai "Draf" | Terpenuhi | Uji fase B + saringan Draf fase A |
| (3) `AT-PP-08` dari sisi FE: tautan PR unit lain → pesan 404 | Terpenuhi | Uji fase A |
| (4) Data basi → pesan + Muat ulang tanpa membuang isian | Terpenuhi | Uji fase B |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 9 warning lint `set-state-in-effect` |
| Masalah yang diketahui | Lihat tabel temuan izin di bawah |
| Temuan izin | Lihat tabel berikut |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | Pemadatan konteks; dilanjutkan dari berkas yang ada |
| Data uji | Jabatan uji `stafA` diberi `ProcurementFundSource : Read` sementara untuk menjalankan alur buat PR, lalu dikembalikan ke bundel "Unit Peminta"; dokumen uji tercatat di `ledger/ui-policy.log` scratchpad |
| Langkah berikutnya | Keputusan pemilik soal bundel izin |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |

### Temuan izin

| Temuan | Dampak di layar | Tindak lanjut yang disarankan |
| --- | --- | --- |
| Bundel "Unit Peminta" tidak memuat `ProcurementFundSource : Read`, padahal sumber dana wajib pada PR | Pengusul melihat pilihan Sumber Dana kosong dengan pesan "minta administrator memberi hak baca Sumber Dana Pengadaan"; PR tidak dapat disimpan | Pemilik memutuskan: tambah izin itu ke peran Unit Peminta (Akses Role) atau backend menyediakan feed pilihan sumber dana di bawah `ProcurementRequest : Create` |
| Bundel "Tim Pengadaan" tidak memuat `Measurement : Read` | Form tambah barang tidak dapat memilih satuan (pesan izin tampil) | Tambah izin baca master Satuan ke peran Tim Pengadaan |
| Bundel "Keuangan / Anggaran" tidak memuat `Organization : Read` | Pemilih unit pada pagu hanya berisi unit pengguna sendiri | Tambah izin baca Organisasi ke peran Keuangan, atau feed unit di bawah `ProcurementBudget : Create` |
| Rincian langkah persetujuan butuh `WorkflowInstance : Read` yang tidak ada di bundel mana pun | Tab Persetujuan menampilkan "Anda belum berhak melihat rincian langkah persetujuan." | Tambah izin itu ke peran yang perlu memantau, atau endpoint ringkas langkah di modul Pengadaan |

## Pembaruan 16 November 2026 (malam) — temuan izin ditutup

| Temuan (lihat tabel di atas) | Penutupan |
| --- | --- |
| Unit Peminta tanpa `ProcurementFundSource : Read` | `PBJ-DEC-039` — izin masuk bundel; uji layar: pengusul memilih "Investasi" |
| Tim Pengadaan tanpa `Measurement : Read` | `PBJ-DEC-039` |
| Keuangan tanpa `Organization : Read` | `PBJ-DEC-039` — uji layar: pilihan unit pagu berisi 30 unit |
| Rincian langkah butuh `WorkflowInstance : Read` | `PBJ-DEC-040` — tab Persetujuan memakai `GET /{dokumen}/{id}/approval` (izin baca dokumen + cakupan unit); `WorkflowInstance : Read` tidak diberikan karena tidak dibatasi per peserta |

| Verifikasi | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas Pengadaan | 0 error, warning `set-state-in-effect` pola muat-data | `PASS` |
| `npm run lint:errors` dan `npm run build` | exit 0; 24 rute `/corporate/procurement/**` | `PASS` |
| Uji layar `ui-results-j.json` (7 skenario) | 7/7 | `PASS` |
