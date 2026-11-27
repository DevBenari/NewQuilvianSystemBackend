# Laporan Perubahan Frontend — `FIN-FE-DPT-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIN-FE-DPT-001` |
| Judul | `/finance/master-data/direct-payment-threshold`: "Ambang" → "Batas Pembayaran Langsung"; pengajuan, persetujuan, aktivasi, riwayat; Update Terakhir |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap (28 Nov 2026). Tidak ada baris roadmap, jadi **tidak ada tanda status yang dipasang pada roadmap** |
| Trace | `FIN-DES-086`, `FIN-DEC-134/147/151`, `FIN-VAL-228`, `FIN-BE-DPT-001` |
| Contract version | Mengonsumsi kontrak `FIN-BE-DPT-001` (belum dibuild, **tabel belum ada**) |
| Wewenang UI | Susunan halaman (A–E), form, dan popup diminta pemilik pada task; komponen mengikuti base component yang sudah ada |
| Dependency | `FIN-BE-DPT-001` **dan skemanya** harus diterapkan lebih dulu. Terhadap backend lama: ringkasan/daftar/riwayat gagal dimuat (pesan galat tampil), dan `PUT` tidak lagi dipanggil |
| Task mode | `FRONTEND` (+ laporan ini di repository backend) |
| Branch / SHA | `yasmina` @ `533b21dc6a6f6c630a79071fc75dbf2b825371eb` |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-28 |
| Status | 🟡 **SEBAGIAN** — source selesai; ESLint, pemeriksa CSS, dan 30 tes lulus; `npm run build` dan dev server **TIDAK DIJALANKAN** (dilarang); layar **belum pernah dibuka** (`VISUAL CHECK: NOT FEASIBLE`) |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Temuan |
| --- | --- |
| Layar | Hero "Ambang Pembayaran Langsung", kartu nilai aktif, form "Ubah Ambang", dialog konfirmasi |
| Pesan | "Perubahan ambang berlaku langsung dan tidak melalui jenjang persetujuan lain" (invarian `FIN-PERM-1.7 G.5`) |
| Hook | Satu hook membaca **dan** menyimpan (`PUT`) |
| Konsumen lain | Layar Payment AR dan AP memakai `useDirectPaymentThreshold` hanya untuk `data.amount` + `buildDirectPaymentConfirmMessage` |
| Menu | "Ambang Pembayaran Langsung" |

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas Finance membuka **Finance → Data Master → Batas Pembayaran Langsung** dan melihat batas yang berlaku beserta Tanggal Berlaku, pengaktif, dan Update Terakhir. Batas yang aktif sebelum alur ini ada menampilkan "Data historis belum tersedia".
2. Empat kartu menunjukkan Total, Menunggu Persetujuan, Disetujui-Belum Aktif, dan Ditolak (dari backend; tak terbaca tampil "—", bukan 0).
3. Pemegang izin `Update` mengisi **Ajukan Perubahan Batas** (Batas Saat Ini hanya baca, Nominal Batas Baru, Alasan) dan menekan **Ajukan Perubahan** → toast "…sedang menunggu persetujuan"; batas aktif **tidak berubah**. Belum ada batas: judul menjadi "Ajukan Batas Pembayaran Pertama".
4. Di tab **Daftar Pengajuan**, menu aksi baris menampilkan hanya yang sah:

| Aksi | Tampil bila | Dialog |
| --- | --- | --- |
| Lihat Detail | selalu | Seluruh tahap, pelaku, dan tanggal (pengaju, penyetuju, penolak + alasan, pengaktif, Tanggal Berlaku, yang digantikan) |
| Setujui | status `MENUNGGU` **dan** izin `Approve` **dan** (bukan pengajuan sendiri, kecuali SuperAdmin) | Ringkasan + catatan "Menyetujui tidak mengaktifkan batas" |
| Tolak | status `MENUNGGU` **dan** izin `Reject` | Ringkasan + kolom **Alasan Penolakan wajib** (tombol nonaktif selama kosong) |
| Aktifkan | `DISETUJUI` + `BELUM_AKTIF` **dan** izin `Activate` | "Nominal Aktif Sekarang" berdampingan dengan "Nominal Baru", penyetuju, tanggal persetujuan, dan kalimat "Batas … akan mulai berlaku setelah aktivasi…" |

5. Tab **Riwayat** menampilkan setiap kejadian (terbaru dulu) dengan filter jenis kejadian, tanggal, dan nomor pengajuan, plus paginasi.

**Jalur tidak normal**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Backend menolak (mis. pengajuan terbuka lain, menyetujui sendiri) | Dialog/form tetap terbuka dan menampilkan pesan backend apa adanya |
| Versi basi (409) | Dialog ditutup, toast "Data Sudah Berubah", data dimuat ulang |
| Tanpa hak `Approve`/`Reject`/`Activate` | Tombolnya tidak muncul |
| Tanpa hak mengajukan | Form diganti keterangan baca-saja |
| Belum ada batas aktif | Peringatan merah "seluruh pembayaran langsung sedang ditolak" |
| Klik ganda | Penjaga `inFlightRef` mencegah tindakan ganda |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas (diverifikasi `git status`)

| Berkas | Perubahan |
| --- | --- |
| `src/app/finance/master-data/direct-payment-threshold/page.jsx` | Metadata → "Batas Pembayaran Langsung" |
| `src/utils/menu-sidebar/menu-items.jsx` | **Hanya label** menu → "Batas Pembayaran Langsung" (key, path, izin `Read` tetap) |
| `src/lib/constants/…/direct-payment-threshold-constants.jsx` | Istilah "Batas"; pesan `NO_APPROVAL_TIER` **dihapus**; konstanta status, label, opsi filter, endpoint, nama izin |
| `src/utils/finance/…/direct-payment-threshold-utils.jsx` | `buildThresholdPayload` (PUT) **dihapus**; normalisasi pengajuan/ringkasan/riwayat; deskripsi status, Tanggal Berlaku, Update Terakhir, batas legacy; pembangun payload & parameter; tindakan baris |
| `src/lib/state/slice/finance/…/master-data-direct-payment-threshold-slice.jsx` | Thunk `PUT` **dihapus**; + ringkasan, daftar, riwayat, ajukan/setujui/tolak/aktifkan; **tanpa pembaruan optimistik**; pesan mentah Axios tidak ditampilkan |
| `src/lib/hooks/finance/…/use-direct-payment-threshold.jsx` | Menjadi **baca-saja** (dipakai AR/AP; API `data` tidak berubah) |
| `src/lib/hooks/finance/…/use-direct-payment-threshold-workflow.jsx` (**baru**) | Controller halaman: izin, tab, filter, form, dialog, penjaga klik ganda, muat ulang setelah sukses |
| `…/direct-payment-threshold-view.jsx` | Disusun ulang (Hero + breadcrumb, batas berlaku, ringkasan, form, tab, tabel, dialog, toast) |
| `…/direct-payment-threshold-table-columns.jsx` (**baru**) | Kolom pengajuan (menu aksi bersyarat) dan riwayat |
| `…/direct-payment-threshold-dialogs.jsx` (**baru**) | Dialog Detail/Setujui/Tolak/Aktifkan |
| `src/style/corporate/finance/master-data/direct-payment-threshold-view.module.css` | Hanya komentar judul (tidak ada kelas baru) |
| `tests/unit/direct-payment-threshold-utils.test.mjs` | Diperbarui: asumsi lama ("tanpa jenjang persetujuan", payload `PUT`, kata "ambang") diganti |
| `tests/unit/direct-payment-threshold-workflow.test.mjs` (**baru**) | 17 tes baru (fungsi murni, pemeriksaan statis FE, dan kontrak yang dibaca dari source BE) |

Tidak diubah: `finance-payment-ar-view.jsx` dan `finance-payment-ap-view.jsx` (tetap memakai hook baca-saja yang sama; tesnya memastikan API-nya tidak berubah), `direct-payment-threshold-client.jsx`, `store.jsx` (kunci reducer tetap).

### 3.2 Kepatuhan arsitektur

Alur `app → view → hook → slice → InstanceAxios` dipertahankan; view tidak memanggil Axios; tidak ada CSS Module baru; satu slice yang sama diperluas (tidak ada slice paralel); `parseThresholdAmountInput` dipertahankan karena dipakai `opening-item-batch-utils`.

### 3.3 Gerbang tampilan

**Modul referensi visual:** `src/components/view/payroll/policy/payroll-policy-list-view.jsx` (aksi baris + konfirmasi) dan modul Mata Uang & Kurs (tugas sebelumnya).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header + breadcrumb | `Hero` + `FinanceBreadcrumb` | `REUSE` | Pola Finance yang sudah ada |
| Batas berlaku | Kelas modul yang sudah ada (`currentCard`, `metaGrid`) + `StatusBadge` | `REUSE` | Tidak ada kelas baru |
| Ringkasan | `SummaryGrid` | `REUSE` | — |
| Form pengajuan | `BaseTextField`, `BaseTextAreaField`, `BaseButton` | `REUSE` | — |
| Tab + filter + cari | `DataFilter` (`tabs`), `FilterSelect`, `FilterDatePicker` | `REUSE` | — |
| Tabel | `DataTable`, `Pagination`, `StatusBadge` | `REUSE` | — |
| Menu aksi baris | `RowActionMenu` | `REUSE` | Aksi bersyarat per status+izin |
| Sel dua baris | `tableStyles.*Cell` + utilitas Bootstrap | `COMPOSE` | Tanpa komponen baru |
| Dialog Detail/Setujui/Tolak/Aktifkan | `ConfirmModal` (+ `requireReason` untuk Tolak) | `REUSE` | — |
| Galat dalam dialog, notifikasi | `InformationAlert`, `ToastStack` | `REUSE` | — |

`UI GATE: PASS — 10 elemen REUSE 9, COMPOSE 1, EXTEND 0, WRAP 0, NEW 0`

> **Alternatif yang dipertimbangkan.** Satu popup dengan tiga tombol (Kembali/Tolak/Setujui) seperti rancangan awal permintaan tidak dapat dibentuk dari `ConfirmModal` (satu tombol konfirmasi). Dipilih dua dialog terpisah (Setujui, Tolak) — menjaga base component dan membuat alasan penolakan wajib terisi. Komponen dialog baru ditolak karena membutuhkan persetujuan `NEW`.

Keluaran pemeriksa:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs <view> <dialogs> <table-columns>
OK — 3 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/finance/master-data/direct-payment-threshold` | — | `VISUAL CHECK: NOT FEASIBLE` — dev server dan build dilarang; tabel backend juga belum ada |

---

## 4. State yang ditangani

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat batas…", kerangka kartu ringkasan, kerangka tabel |
| Kosong | "Belum ada pengajuan/riwayat batas pembayaran langsung." |
| Gagal | Kotak galat per bagian (batas, ringkasan, daftar) dengan pesan Indonesia; batas gagal dimuat punya tombol Coba Lagi |
| Tanpa hak akses | `AccessDeniedGate` bila tanpa `Read` |
| Belum ada batas | Peringatan merah fail-closed |
| Tindakan berjalan | Tombol berputar dan terkunci |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Master Data / Direct Payment Threshold

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Batas berlaku (juga layar AR/AP) | `MstDirectPaymentThreshold : Read` |
| `GET` | `/proposals/summary`, `/proposals`, `/history` | Kartu, tabel pengajuan, riwayat | `Read` |
| `POST` | `/proposals` | Ajukan | `Update` |
| `POST` | `/proposals/{id}/approve` · `reject` · `activate` | Setujui · Tolak · Aktifkan | `Approve` · `Reject` · `Activate` |

`PUT /` **tidak lagi dipanggil**. Alamat dan pasangan izin dicocokkan dengan source controller backend oleh tes.

---

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` (folder view, hooks, slice, constants, utils, app, 2 berkas tes) | 0 error, 0 peringatan | `PASS` |
| `check-css-usage.mjs` (3 berkas JSX) | `OK — 3 berkas diperiksa, 0 temuan.` | `PASS` |
| `node --import ./tests/helpers/register.mjs --test` pada `direct-payment-threshold-utils` + `-workflow` | 30 tes, 30 lulus | `PASS` |
| Tes menu terkait (`master-data-sidebar-grouping`, `hr-master-data-sidebar-grouping`, `hemodialysis-sidebar-navigation`, `sidebar-scroll`, `opening-item-batch-utils`) | 9 gagal. **Gagal sama persis (9 dari 9) saat `menu-items.jsx` dikembalikan sementara ke HEAD** — menghitung kelompok menu Master Data layanan kesehatan, tidak terkait Finance. Berkas sudah dipulihkan | `EXISTING / ENVIRONMENT ISSUE` |
| `npm run build`, dev server, buka layar, klik tombol/dialog, responsif, screenshot | Tidak dilakukan | `NOT RUN` / `MANUAL TEST: NOT FEASIBLE` |

`AUTOMATED TEST: node --test tests/unit/direct-payment-threshold-*.test.mjs — PASS (30/30)`

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dependency backend | Tanpa `FIN-BE-DPT-001` + tabelnya, daftar/ringkasan/riwayat menampilkan galat; layar AR/AP **tidak terpengaruh** |
| Kolom Update Terakhir pada daftar | Nama pelaku tahap terakhir pengajuan dari audit, bukan pengguna yang sedang login; tak ditemukan → "Pengguna tidak tersedia" |
| Perilaku `DataTable` | Tabel baru memakai `sortLatestFirst={false}` agar urutan dari backend (terbaru dulu) dipertahankan |
| Komentar kode lama | Sebagian komentar rujukan (FIN-…) pada kode lain masih menyebut "ambang"; hanya teks tampilan yang diubah |
| Perubahan sampingan | `NONE`. `menu-items.jsx` sempat dikembalikan ke HEAD untuk pembuktian tes lalu dipulihkan (diff akhir = label ini + perubahan Logistik dari tugas sebelumnya) |
| Status Git | 10 berkas `M` + 4 `??` terkait tugas ini; tidak ada stage/commit/push |
| Langkah berikutnya | Terapkan backend + skema, `npm run build`, buka layar dengan alur penuh (ajukan → setujui → aktifkan) dan simpan screenshot |
