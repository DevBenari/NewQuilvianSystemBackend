# Manajemen Vendor — Roadmap Frontend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `manajemen-vendor` (`approved`, `PBJ-DEC-062`) |
| Revisi roadmap | `1` · status `draft` |
| Tanggal | 17 November 2026 |
| Kontrak | set `1.0` (`PBJ-VD-API`, `-PERM`, `-STATE`, `-VAL`); `03-frontend-architecture.md` |
| Kewenangan UI | `PBJ-FE-001` (nama menu SOURCE), `PBJ-FE-004` (standar layar daftar), `PBJ-FE-003` (invariant); rupa lain `PBJ-FE-002` `DEV_DISCRETION`; nama submenu menu 10 menunggu `PBJ-OQ-033` |
| SHA | FE `059b6c612` (branch `sukmagpV2`), BE `c6b86a6e` |
| Repository target | `V2QuilvianSystemFrontendDev` |

**Kebijakan uji frontend:** bukti utama verifikasi manual kontrol interaktif (Playwright dari scratchpad, aktor
non-SuperAdmin), lint, dan build. Test unit opsional. Baris filter wajib lolos pemeriksaan satu baris (`AT-VD-63`).

**Kontrak dikunci:** frontend boleh mulai paralel dengan backend hanya untuk kontrak set `1.0`. Bila respons nyata berbeda,
kontrak yang direvisi, bukan frontend yang menyesuaikan diam-diam.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai; terbukti di laporan task |
| 🟡 | Sebagian |
| ⛔ | Terblokir; nama blocker disebut |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

Empat blok. `FE-PBJ-VD-002` pada blok 2–4 adalah node yang sama dengan blok 1 (sambungan).

```text
BE-PBJ-VD-002 [BE] ✅ ─> FE-PBJ-VD-001 ✅ ─┐
                                           │
BE-PBJ-VD-003 [BE] ✅ ─────────────────────┴─> FE-PBJ-VD-002 ✅
```

```text
FE-PBJ-VD-002 ✅ ──────────┐
                           │
BE-PBJ-VD-004 [BE] ✅ ─┐   │
                       │   │
BE-PBJ-VD-005 [BE] ✅ ─┴───┴─> FE-PBJ-VD-003 ✅
```

```text
FE-PBJ-VD-002 ✅ ──────────┐
                           │
BE-PBJ-VD-006 [BE] ✅ ─┐   │
                       │   │
BE-PBJ-VD-007 [BE] ✅ ─┴───┴─> FE-PBJ-VD-004 ✅
```

```text
FE-PBJ-VD-002 ✅ ──────┐
                       │
BE-PBJ-VD-008 [BE] ✅ ─┴─> FE-PBJ-VD-005 ✅
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja.

Jumlah pasangan prasyarat → task: **11**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-PBJ-VD-002` | `FE-PBJ-VD-001` |
| 2 | `FE-PBJ-VD-001`, `BE-PBJ-VD-003` | `FE-PBJ-VD-002` |
| 3 | `FE-PBJ-VD-002` + prasyarat `[BE]` masing-masing | `FE-PBJ-VD-003`, `FE-PBJ-VD-004`, `FE-PBJ-VD-005` |

Gelombang dihitung dari prasyarat frontend; prasyarat `[BE]` menahan mulai sampai task backend-nya ✅.

## Tabel task

| Task ID | Outcome | Layar | Kontrak / izin | Reuse | Dependency | Acceptance criteria | Verifikasi | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-PBJ-VD-001` | Butir menu 5 dan 10, layanan API vendor, tiga layar Pengaturan | `-11`, `-12`, `-13`; menu | API §Master Data (3 grup); `ProcurementVendorDocumentType`, `ProcurementCategoryDocumentRequirement`, `ProcurementEvaluationCriterion` | Pola master `settings/items`; `procurement-list-filter` | `BE-PBJ-VD-002` | Menu tampil sesuai izin; tiga master CRUD; total bobot tampil; `AT-VD-63` | Lint, build, Playwright | ✅ 18 November 2026 ([laporan](../task/report/frontend/FE-PBJ-VD-001.md)) |
| `FE-PBJ-VD-002` | Daftar dan detail vendor | `-01`, `-02` (Profil & Kategori, Rekening, Riwayat), `-03`, `-05`, `-08` | API §Vendor; `ProcurementVendor : Read, Update` | `DataTable`, `DataFilter`, tab detail pola PR | `FE-PBJ-VD-001`, `BE-PBJ-VD-003` | `AT-VD-01`, `03`, `04`; rekening `****` | Playwright aktor Tim Pengadaan dan Unit Peminta | ✅ 18 November 2026 ([laporan](../task/report/frontend/FE-PBJ-VD-002.md)) |
| `FE-PBJ-VD-003` | Dokumen legalitas dan kelayakan | `-04`; tab Dokumen `-02` | API §Vendor Document, eligibility; `ProcurementVendorDocument : Create, Verify` | Komponen lampiran `FE-PBJ-002` | `FE-PBJ-VD-002`, `BE-PBJ-VD-004`, `BE-PBJ-VD-005` | `AT-VD-10`..`16`; tombol Verifikasi tersembunyi bagi pengunggah | Playwright dua aktor | ✅ 19 November 2026 ([laporan](../task/report/frontend/FE-PBJ-VD-003.md)) |
| `FE-PBJ-VD-004` | Status vendor, usulan blokir, kunci centang Master Data | `-06`, `-07`; tab Status `-02`; layar Master Data Supplier (centang Blacklist baca saja) | API §Vendor Status Request, restriction; `ProcurementVendor : Restrict`; `ProcurementVendorStatusRequest : Create, Cancel` | Pola detail PR + tab Persetujuan `FE-PBJ-005` | `FE-PBJ-VD-002`, `BE-PBJ-VD-006`, `BE-PBJ-VD-007` | `AT-VD-20`, `22`..`26`, `29` | Playwright pengusul dan penyetuju | ✅ 19 November 2026 ([laporan](../task/report/frontend/FE-PBJ-VD-004.md)) |
| `FE-PBJ-VD-005` | Penilaian berkala | `-09`, `-10`; tab Penilaian `-02` | API §Vendor Evaluation; `ProcurementVendorEvaluation : *` | Form baris pola rencana kebutuhan | `FE-PBJ-VD-002`, `BE-PBJ-VD-008` | `AT-VD-30`..`36`; Finalkan tersembunyi bagi penilai | Playwright penilai dan Manajer | ✅ 19 November 2026 ([laporan](../task/report/frontend/FE-PBJ-VD-005.md)) |

## Kartu task

### ✅ `FE-PBJ-VD-001` — Menu dan Pengaturan vendor

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir menu "5. Data Penyedia/ Vendor" (6 submenu) dan "10. Evaluasi Penyedia" (1 submenu; label sementara "Penilaian Vendor" sampai `PBJ-OQ-033`); service `vendor-*-service.js`; konstanta izin dan status vendor; layar `-11`..`-13` mengikuti standar master Pengaturan Pengadaan |
| Acceptance criteria | (1) Butir menu tampil hanya bagi pemegang izin baca. (2) Master CRUD dengan baris filter `PBJ-FE-004`. (3) `-13` menampilkan total bobot aktif per kategori dan peringatan bila ≠ 100% |
| DoD | Kriteria + laporan `task/report/frontend/FE-PBJ-VD-001.md` |
| **Status** | ✅ **SELESAI 18 November 2026.** Menu 5 (6 butir) dan 10 (1 butir, label sementara `PBJ-OQ-033`), 3 butir Pengaturan; layar `-11`..`-13` dengan sembilan endpoint master; total bobot aktif per kategori dan peringatan ≠ 100%. `npx eslint` 10 berkas 0 masalah, `npm run lint:errors` exit 0, `npm run build` `Compiled successfully`; uji layar Playwright 24/24 `PASS` dengan tiga aktor non-SuperAdmin (`AT-VD-63` terbukti di tiga layar). Butir menu 5.x dan 10 mengarah ke rute yang dibuat `FE-PBJ-VD-002`..`005`. Laporan: [laporan](../task/report/frontend/FE-PBJ-VD-001.md) |

### ✅ `FE-PBJ-VD-002` — Daftar dan detail vendor

| Aspek | Isi |
| --- | --- |
| Cakupan | `-01` daftar (ringkasan, baris filter, tanpa tombol tambah; tautan Master Data bagi pemegang `Supplier : Create`); `-02` detail dengan tab Profil & Kategori, Rekening, Riwayat (tab lain menyusul task 003–005); `-03` vendor per kategori; `-05` rekening tersamarkan; `-08` riwayat satu vendor |
| Acceptance criteria | `AT-VD-01`, `03`, `04`; atur kategori memperbarui tampilan tanpa muat ulang halaman |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 18 November 2026.** Layar `-01`, `-02` (Profil & Kategori, Rekening, Riwayat), `-03`, `-05`, `-08`; `-05` memakai delta `BE-PBJ-VD-003` (rekening tersamarkan + saringan bank di daftar, usulan `PBJ-VD-API` 1.1) atas keputusan pemilik. `npx eslint` 0 error (2 warning pola muat-data), `npm run lint:errors` exit 0, `npm run build` `Compiled successfully`; uji layar Playwright 15/15 `PASS` (Tim Pengadaan, baca-saja, pemegang `Supplier : Create`, Unit Peminta). Laporan: [laporan](../task/report/frontend/FE-PBJ-VD-002.md) |

### ✅ `FE-PBJ-VD-003` — Dokumen legalitas

| Aspek | Isi |
| --- | --- |
| Cakupan | `-04` daftar dan antrean verifikasi; catat dokumen + unggah berkas; verifikasi/tolak (alasan wajib); tanda Segera kedaluwarsa/Kedaluwarsa; tab Dokumen di `-02` dengan kelayakan per kategori |
| Acceptance criteria | `AT-VD-10`..`16`; `PBJ-VAL-101`/`102`/`103` terbaca dari server |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 19 November 2026.** Layar `-04` (daftar dan antrean verifikasi, catat + unggah, Verifikasi/Tolak/Batal) dan tab Dokumen `-02` dengan kelayakan per kategori. `npx eslint` 0 error (2 warning pola muat-data), `npm run build` `Compiled successfully`; uji layar Playwright putaran 2 16/16 `PASS` dengan empat aktor non-SuperAdmin (pengunggah, pemeriksa, baca saja, tanpa izin dokumen) — `AT-VD-10`..`16` dan `AT-VD-63` terbukti di layar; cek API 4/4 `PASS` (`PBJ-VAL-103` 403 untuk pengunggah, supplier berizin kedaluwarsa tidak masuk daftar hitam). Putaran 2 sempat tertahan basis data dev mati 18 November 2026 15:56 sampai 19 November 2026 pagi. Data uji putaran 1 dan 2 sudah dibersihkan 19 November 2026. Laporan: [laporan](../task/report/frontend/FE-PBJ-VD-003.md) |

### ✅ `FE-PBJ-VD-004` — Status vendor dan usulan

| Aspek | Isi |
| --- | --- |
| Cakupan | `-06` (vendor non-Aktif dan daftar usulan), `-07` detail usulan (ajukan, batal, tarik, tab Persetujuan dan Riwayat), Batasi/Cabut di `-02`; centang Blacklist pada layar Master Data Supplier menjadi baca saja dengan keterangan (`PBJ-DEC-060`) |
| Acceptance criteria | `AT-VD-20`, `22`..`26`, `29` |
| Catatan | Rilis bersama `BE-PBJ-VD-007` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 19 November 2026.** Layar `-06` (tab Vendor Tidak Aktif dengan saringan `vendorStatus=NON_ACTIVE` dan tab Usulan), `-07` (buat, simpan draf, ajukan, tarik, batal, sinkron; tab Lampiran/Persetujuan/Riwayat), tab Status `-02` (Batasi/Cabut, Usulkan blokir/buka blokir), jenis "Usulan Status Vendor" di Antrean Persetujuan, dan centang "Diblokir (Pengadaan)" baca saja di Master Data Supplier. Butuh pengerjaan ulang `BE-PBJ-VD-003` (delta D-10, keputusan pemilik). `npx eslint` 0 error (2 warning pola muat-data), `npm run lint:errors` exit 0, `npm run build` `Compiled successfully` (36 rute Pengadaan); uji layar Playwright 16/16 `PASS` dengan empat aktor non-SuperAdmin (Manajer, penyetuju, baca saja, admin Master Data) — `AT-VD-20`, `22`..`26`, `29`, `AT-VD-63`; cek API/DB 5/5 `PASS`. Konfigurasi persetujuan uji `PBJ-VND-*` dipasang lalu dicabut atas izin pemilik. Temuan di luar task: simpan supplier non-pajak dari Master Data gagal (pajak). Data uji sudah dibersihkan 19 November 2026. Laporan: [laporan](../task/report/frontend/FE-PBJ-VD-004.md) |

### ✅ `FE-PBJ-VD-005` — Penilaian berkala

| Aspek | Isi |
| --- | --- |
| Cakupan | `-09` daftar; `-10` form/detail (salinan kriteria, nilai 1–5, nilai akhir dari server, rekomendasi, finalisasi, revisi, batal); tab Penilaian di `-02` |
| Acceptance criteria | `AT-VD-30`..`36` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 19 November 2026.** Layar `-09` (daftar, 5 kartu, saringan kategori + tahun + semester + status, "+ Buat Penilaian"), `-10` (buat draf dengan peringatan bobot ≠ 100%, nilai 1–5 per kriteria salinan, nilai akhir dari server, rekomendasi, finalkan oleh bukan penilai, revisi, batal; tab Lampiran + Riwayat), dan tab Penilaian `-02`. `npx eslint` 0 error (1 warning pola muat-data), `npm run lint:errors` exit 0, `npm run build` `Compiled successfully` (39 rute Pengadaan); uji layar Playwright 16/16 `PASS` dengan empat aktor non-SuperAdmin (penilai, Manajer, baca saja, tanpa izin) — `AT-VD-30`..`36`, `AT-VD-63`; cek API/DB 5/5 `PASS`. Tanpa perubahan backend. Data uji sudah dibersihkan 19 November 2026. Laporan: [laporan](../task/report/frontend/FE-PBJ-VD-005.md) |
