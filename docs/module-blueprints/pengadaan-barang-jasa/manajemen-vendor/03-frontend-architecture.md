# Manajemen Vendor — Arsitektur Frontend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2, sub-modul `manajemen-vendor` · status `approved` — pengguna sesi, 17 November 2026 (`PBJ-DEC-062`) |
| Masukan | `02-backend-architecture.md`, `contracts/*` 1.0 (approved); `PBJ-FE-001`..`004`; `PBJ-DEC-051`, `060` |
| Repository | `V2QuilvianSystemFrontendDev` @`059b6c612` |
| Kewenangan UI | `PBJ-FE-001` nama menu SOURCE; `PBJ-FE-004` standar layar daftar (approved); rupa lain `PBJ-FE-002` `DEV_DISCRETION`; `PBJ-FE-003` invariant |

## 1. Lokasi kode

Mengikuti pola `perencanaan-permintaan`: route `src/app/corporate/procurement/vendors/**`, view
`src/components/view/corporate/procurement/vendors/`, hook `src/lib/hooks/corporate/procurement/use-vendor-*.jsx`, service
`src/lib/services/corporate/procurement/vendor-*-service.js`, konstanta di `procurement-constants.jsx`, gaya di
`procurement.module.css`. Baris filter memakai helper bersama `shared/procurement-list-filter.jsx` (`FE-PBJ-009`).

## 2. Kebutuhan layar

| ID | Layar | Submenu SOURCE | Tujuan |
| --- | --- | --- | --- |
| `FE-PBJ-VD-01` | Daftar Vendor | 5.1 Daftar vendor | Melihat vendor beserta status, kategori, keadaan dokumen |
| `FE-PBJ-VD-02` | Detail Vendor | (anak `-01`) | Profil, kategori, dokumen, rekening, status, riwayat, penilaian satu vendor |
| `FE-PBJ-VD-03` | Kategori Vendor | 5.2 Kategori | Vendor per kategori dan pengaturan kategori vendor |
| `FE-PBJ-VD-04` | Dokumen Legalitas | 5.3 Dokumen Legalitas | Daftar dokumen, antrean verifikasi, kedaluwarsa |
| `FE-PBJ-VD-05` | Rekening Vendor | 5.4 Rekening | Rekening tersamarkan, baca saja (`PBJ-DEC-015`) |
| `FE-PBJ-VD-06` | Status Vendor | 5.5 Status | Status vendor dan daftar usulan blokir/buka blokir |
| `FE-PBJ-VD-07` | Detail Usulan Status | (anak `-06`) | Isi, ajukan, tarik usulan; tab Persetujuan dan Riwayat |
| `FE-PBJ-VD-08` | Riwayat Vendor | 5.6 Riwayat | Linimasa satu vendor |
| `FE-PBJ-VD-09` | Daftar Penilaian | 10.x (nama menunggu `PBJ-OQ-033`) | Penilaian berkala per vendor × kategori × periode |
| `FE-PBJ-VD-10` | Form/Detail Penilaian | (anak `-09`) | Isi nilai, finalkan, revisi |
| `FE-PBJ-VD-11` | Jenis Dokumen Legalitas | Pengaturan Pengadaan | Master jenis dokumen |
| `FE-PBJ-VD-12` | Dokumen Wajib per Kategori | Pengaturan Pengadaan | Aktifkan kewajiban per kategori (`PBJ-DEC-061`) |
| `FE-PBJ-VD-13` | Kriteria Penilaian | Pengaturan Pengadaan | Kriteria dan bobot per kategori |

Empat submenu menu 10 lainnya (evaluasi kontrak, rekap keterlambatan, rekap NCR, rekomendasi — nama final menunggu
`PBJ-OQ-033`) bergantung `S-V7` dan **tidak** dibuat pada rilis ini.

## 3. Peta butir menu

Daftar induk seluruh modul ada di `../02-module-map.md` bagian 3.1. Butir milik sub-modul ini:

| Tingkat | Induk | Butir | Route usulan | Layar | Hak akses |
| ---: | --- | --- | --- | --- | --- |
| 1 | Pengadaan Barang & Jasa | 5. Data Penyedia/Vendor | — | — | Salah satu izin di bawah |
| 2 | 5 | Daftar vendor | `/corporate/procurement/vendors` | `-01` (→ `-02`) | `ProcurementVendor : Read` |
| 2 | 5 | Kategori | `/corporate/procurement/vendors/categories` | `-03` | `ProcurementVendor : Read` |
| 2 | 5 | Dokumen Legalitas | `/corporate/procurement/vendors/documents` | `-04` | `ProcurementVendorDocument : Read` |
| 2 | 5 | Rekening | `/corporate/procurement/vendors/bank-accounts` | `-05` | `ProcurementVendor : Read` |
| 2 | 5 | Status | `/corporate/procurement/vendors/status` | `-06` (→ `-07`) | `ProcurementVendor : Read` |
| 2 | 5 | Riwayat | `/corporate/procurement/vendors/history` | `-08` | `ProcurementVendor : Read` |
| 1 | Pengadaan Barang & Jasa | 10. Evaluasi Penyedia | — | — | `ProcurementVendorEvaluation : Read` |
| 2 | 10 | (nama `PBJ-OQ-033`) | `/corporate/procurement/vendors/evaluations` | `-09` (→ `-10`) | `ProcurementVendorEvaluation : Read` |
| 2 | Pengaturan Pengadaan | Jenis Dokumen Legalitas | `/corporate/procurement/settings/vendor-document-types` | `-11` | `ProcurementVendorDocumentType : Read` |
| 2 | Pengaturan Pengadaan | Dokumen Wajib per Kategori | `/corporate/procurement/settings/category-document-requirements` | `-12` | `ProcurementCategoryDocumentRequirement : Read` |
| 2 | Pengaturan Pengadaan | Kriteria Penilaian | `/corporate/procurement/settings/evaluation-criteria` | `-13` | `ProcurementEvaluationCriterion : Read` |

Layar anak: `-02` dari baris `-01`/`-03`/`-05`/`-06`; `-07` dari daftar usulan `-06` dan tombol "Usulkan blokir" di
`-02`; `-10` dari baris `-09` dan tombol "+ Buat Penilaian".

## 4. Skema fitur per layar

### 4.1 Daftar Vendor — `FE-PBJ-VD-01`

```text
┌ Hero: "Daftar Vendor" (tanpa tombol) ─────────────────────────────────────────┐
├ Kartu ringkasan: Total · Aktif · Dibatasi · Diblokir · Dokumen kedaluwarsa ───┤
├ Filter (satu baris, PBJ-FE-004): Tgl awal · Tgl akhir · Periode · Kategori ·  │
│   Keadaan dokumen · Status vendor · Cari · Jumlah baris · ↻                    │
├ Tabel: Kode · Nama · Kategori · Status · Dokumen · Nilai terakhir · Aksi ─────┤
└ Paginasi ─────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber | Hak akses tombol | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Hitungan status dan dokumen | `GET /vendors/summary` | — | Angka 0 / kartu disembunyikan |
| Filter | Saringan relevan: Kategori, Keadaan dokumen; Status: Aktif/Dibatasi/Diblokir/Nonaktif | `GET /vendors/filters/metadata` | — | — |
| Tabel | Vendor | `GET /vendors` | "Buka" = `Read` | "Belum ada vendor pada saringan ini." / "Data vendor gagal dimuat." + Coba lagi |

Tidak ada tombol tambah: vendor dibuat di Master Data Supplier (`PBJ-DEC-053`); keterangan dan tautan ke layar Master Data
tampil bagi pemegang `Supplier : Create`.

### 4.2 Detail Vendor — `FE-PBJ-VD-02`

```text
┌ Hero detail: nama vendor, kode, lencana status (tombol hero boleh: halaman detail) ┐
│   Kembali · Atur kategori · Batasi / Cabut · Usulkan blokir / buka blokir            │
├ Tab: Profil & Kategori | Dokumen | Rekening | Status | Penilaian | Riwayat ─────────┤
└────────────────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber | Hak akses tombol | Kosong / gagal |
| --- | --- | --- | --- | --- |
| Profil & Kategori | Identitas baca saja dari Master Data; kategori | `GET /vendors/{id}`; `PUT /vendors/{id}/categories` | Atur kategori = `ProcurementVendor : Update` | "Belum ada kategori." |
| Dokumen | Dokumen per jenis, keadaan berlaku, verifikasi | `GET /vendor-documents?supplierId=` | "+ Catat Dokumen" = `ProcurementVendorDocument : Create`; Verifikasi/Tolak = `Verify`, tersembunyi bagi pengunggah | "Belum ada dokumen." |
| Rekening | Bank, `****1234`, nama pemilik, "Tidak tersedia di Master Data" | `GET /vendors/{id}` | — | "Data rekening dikelola Finance" (`PBJ-DEC-015`) |
| Status | Status tampil, alasan, tanggal berakhir, usulan terbuka | `GET /vendors/{id}`, `GET /vendor-status-requests?supplierId=` | Batasi/Cabut = `Restrict`; Usulkan = `ProcurementVendorStatusRequest : Create` | — |
| Penilaian | Penilaian per kategori dan periode | `GET /vendor-evaluations?supplierId=` | "+ Buat Penilaian" = `ProcurementVendorEvaluation : Create` | "Belum ada penilaian." |
| Riwayat | Linimasa | `GET /vendors/{id}/history` | — | "Belum ada riwayat." |

### 4.3 Dokumen Legalitas — `FE-PBJ-VD-04`

Baris filter: Tanggal awal, Tanggal akhir, Periode, **Jenis dokumen**, **Keadaan berlaku** (Berlaku/Segera kedaluwarsa/
Kedaluwarsa), **Status verifikasi**, Cari vendor/nomor, Jumlah baris, ↻. Tombol filter: "+ Catat Dokumen" (`Create`).
Kolom: Vendor, Jenis, Nomor, Terbit, Berlaku sampai, Keadaan, Status verifikasi, Pengunggah, Aksi. Catat dokumen dan
unggah berkas memakai komponen lampiran `FE-PBJ-002`; rupa form (modal/drawer/halaman) `DEV_DISCRETION`.

### 4.4 Status Vendor — `FE-PBJ-VD-06` dan Detail Usulan — `FE-PBJ-VD-07`

`-06` berisi dua daftar (rupa tab/bagian `DEV_DISCRETION`): vendor non-Aktif (filter: tanggal, periode, Kategori,
Status vendor, cari, jumlah baris, ↻) dan usulan (filter: tanggal, periode, **Jenis usulan**, **Status usulan**, cari
nomor/vendor, jumlah baris, ↻; tombol filter "+ Usulan" = `ProcurementVendorStatusRequest : Create`).
`-07` mengikuti pola detail PR: kepala (nomor `UBV`, vendor, jenis, status), isian alasan dan bukti penilaian (saat
`DRAFT`/`RETURNED`), tombol hero Ajukan/Batalkan/Tarik, tab Persetujuan (`GET /{id}/approval`) dan Riwayat.

### 4.5 Penilaian — `FE-PBJ-VD-09` dan `FE-PBJ-VD-10`

`-09` baris filter: Tanggal awal, Tanggal akhir, Periode, **Kategori**, **Periode penilaian** (tahun + semester),
**Status penilaian**, Cari vendor, Jumlah baris, ↻; tombol filter "+ Buat Penilaian". Kolom: Vendor, Kategori, Periode,
Nilai akhir, Rekomendasi, Status, Penilai, Pemfinal.
`-10`: kepala (vendor, kategori, periode, status), tabel kriteria salinan (nama, bobot, nilai 1–5, catatan), nilai akhir
dari server, rekomendasi, catatan; tombol Simpan, Finalkan (tersembunyi bagi penilai sendiri), Buat revisi, Batalkan.
Peringatan bila total bobot kategori belum 100% muncul sebelum draf dibuat.

### 4.6 Layar daftar lain

| Layar | Saringan relevan | Status | Tombol di filter |
| --- | --- | --- | --- |
| `-03` Kategori Vendor | Kategori | Status vendor | — (atur kategori di `-02`) |
| `-05` Rekening Vendor | Bank | Status vendor | — (baca saja) |
| `-08` Riwayat Vendor | Vendor (wajib dipilih), Jenis kejadian | — | — |
| `-11` Jenis Dokumen | Wajib tanggal berlaku | Aktif/Nonaktif | "+ Tambah Jenis" |
| `-12` Dokumen Wajib | Kategori, Jenis dokumen | Aktif/Nonaktif | "+ Tambah Kewajiban" |
| `-13` Kriteria Penilaian | Kategori | Aktif/Nonaktif | "+ Tambah Kriteria"; total bobot per kategori tampil di ringkasan |

Semua baris filter mengikuti `PBJ-FE-004`: Tanggal awal, Tanggal akhir, Periode, saringan relevan, Status, Pencarian,
Jumlah baris, ↻; hero tanpa tombol kecuali `-02`, `-07`, `-10` (halaman detail/form).

## 5. Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` bagian 3: Tim Pengadaan mencatat dokumen, memverifikasi dokumen
orang lain, mengatur kategori, dan menilai; Manajer menambah Batasi/Cabut, usulan blokir, dan finalisasi; Direktur membaca
dan memutuskan usulan dari kotak masuk `Wfl`.

## 6. Penanganan keadaan

| Keadaan | Perilaku |
| --- | --- |
| Memuat | Kerangka tabel/detail bawaan base component |
| Kosong | Kalimat kosong per wilayah (bagian 4) |
| Gagal | `InformationAlert` + "Coba lagi"; 403 → "Akses Ditolak"; 404 → "Vendor tidak ditemukan." |
| Data basi | 409 `PBJ-VAL-090` → muat ulang dan beri tahu |
| Pengiriman ganda | Tombol aksi status nonaktif saat memproses; `Idempotency-Key` per niat aksi |
| Penjaga orang yang sama | Tombol Verifikasi/Finalkan tidak tampil bagi pengunggah/penilai; server tetap menolak (403) |

## 7. Kewenangan UI

| Area | Kewenangan |
| --- | --- |
| Nama menu 5 dan submenu 5.1–5.6 | `PBJ-FE-001` (SOURCE) |
| Nama submenu menu 10 | Menunggu `PBJ-OQ-033` |
| Baris filter, hero, tombol | `PBJ-FE-004` |
| Rekening tersamarkan, tombol tersembunyi ≠ penjaga server | `PBJ-FE-003` invariant |
| Tab/modal/drawer, urutan kolom, ikon | `DEV_DISCRETION` (`PBJ-FE-002`) |
