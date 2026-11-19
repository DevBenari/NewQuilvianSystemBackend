# Laporan Perubahan Frontend — `FE-PBJ-VD-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-VD-005` |
| Judul | Penilaian berkala |
| Slice | Gelombang 3 roadmap frontend `manajemen-vendor` revisi 1 |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | Layar `FE-PBJ-VD-09`, `FE-PBJ-VD-10`, tab Penilaian `FE-PBJ-VD-02`; FR-VD-040, 041, 042; `PBJ-DEC-048`, `049`, `050`, `059`; `PBJ-FE-003`, `004` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor Evaluation, dengan delta `BE-PBJ-VD-008` D-1..D-8 (ringkasan D-7, jenis periode D-8, nilai akhir dihitung ulang D-5) |
| Wewenang UI | `PBJ-FE-004` standar layar daftar; `PBJ-FE-003` invariant (tombol tersembunyi bukan penjaga); nama submenu menu 10 tetap label sementara "Penilaian Vendor" (`PBJ-OQ-033`); rupa form `-10` `DEV_DISCRETION` (arsitektur frontend bagian 4.5) |
| Dependency | `FE-PBJ-VD-002` ✅, `BE-PBJ-VD-008` ✅ |
| Klasifikasi | `MEDIUM` — 2 layar baru, 1 tab baru, 2 hook baru, 1 service baru, perluasan 2 komponen bersama modul |
| Task mode | `FRONTEND` (instruksi pengguna "lanjutkan", 19 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `b1b360efa` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `a6df5619` (branch `sukmagp`, belum di-commit); tidak ada perubahan backend pada task ini |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — lint dan build `PASS`; uji layar 16/16 `PASS`; cek API/DB 5/5 `PASS` |

---

## 1. Keadaan yang ditemukan di awal

Butir menu 10 "Penilaian Vendor" (`/corporate/procurement/vendors/evaluations`) sudah ada sejak `FE-PBJ-VD-001` dengan label sementara `PBJ-OQ-033`, tetapi rutenya masih 404. Detail Vendor hanya menampilkan "penilaian final terakhir per kategori" di tab Profil.

`BE-PBJ-VD-008` sudah lengkap: daftar, ringkasan, detail, buat draf, isi nilai, finalkan, revisi, batal, riwayat, dan penyedia akses lampiran `PBJ_VENDOR_EVALUATION`. Master kriteria (`FE-PBJ-VD-001`) sudah menyediakan ringkasan total bobot aktif per kategori (`GET /master-data/evaluation-criteria/summary?itemCategoryId=`), jadi peringatan "bobot belum 100%" bisa ditampilkan sebelum draf dibuat.

---

## 2. Proses bisnis dari sisi pengguna

1. **Membuat draf.** Petugas Tim Pengadaan (penilai) menekan **+ Buat Penilaian** di `-09`, atau di tab Penilaian Detail Vendor dengan vendor terkunci. Dia memilih vendor, kategori yang dilayani vendor itu, tahun, jenis periode, dan periode (mis. "Semester I 2026").
   - Bila total bobot kriteria aktif kategori itu belum 100%, peringatan tampil sebelum draf dibuat. Server lalu menolak dengan `PBJ-VAL-108`.
   - Bila penilaian vendor × kategori × periode yang sama sudah ada, server menolak dengan `PBJ-VAL-109`.
2. **Mengisi nilai.** Kriteria aktif kategori disalin ke draf beserta bobotnya. Penilai memberi nilai 1–5 dan catatan per kriteria, memilih rekomendasi (Lanjutkan/Batasi/Blokir), lalu **Simpan**. Nilai akhir selalu dihitung server, mis. 40/40/20 dengan nilai 4/5/3 → 4,20.
3. **Memfinalkan.** Manajer Pengadaan membuka draf dan menekan **Finalkan**. Penilai tidak melihat tombol itu dan mendapat keterangan "Anda penilai penilaian ini, jadi finalisasi dilakukan orang lain". Bila masih ada kriteria kosong atau rekomendasi belum dipilih, server menolak dengan `PBJ-VAL-110`.
4. **Merevisi.** Penilaian final yang salah dikoreksi lewat **Buat revisi** dengan alasan. Draf revisi menyalin nilai lama, lalu difinalkan orang lain. Penilaian asal menjadi **Digantikan revisi**, dan keduanya tetap bisa dibuka lewat tombol **Buka penilaian asal** dan **Buka revisi**.
5. **Rekomendasi hanya saran.** Penilaian final dengan rekomendasi Batasi/Blokir menampilkan keterangan bahwa status vendor tidak berubah otomatis. Pembatasan dan usulan blokir tetap lewat Status Vendor (`FE-PBJ-VD-004`).

Contoh: Rina (Tim Pengadaan) menilai "Prima Husada" untuk kategori Alat Kesehatan, Semester I 2026: Ketepatan waktu 4, Kualitas 5, Layanan purnajual 3 → 4,20, rekomendasi Lanjutkan. Budi (Manajer) memfinalkannya. Seminggu kemudian Rina sadar nilai layanan seharusnya 4. Dia membuat revisi dengan alasan "Nilai layanan purnajual salah input." dan mengubah nilainya menjadi 4,40, lalu Budi memfinalkan revisi itu. Penilaian 4,20 tetap tersimpan sebagai Digantikan revisi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` bagian 2–7; `contracts/api-contract.md` §Vendor Evaluation; `testing/acceptance-test-matrix.md` `AT-VD-30`..`36`; laporan `BE-PBJ-VD-008` (D-1..D-8).

Backend:

- `VendorEvaluationsController.cs`, `VendorEvaluationDtos.cs`, `ProcurementVendorEvaluationService.cs`;
- `ProcurementEvaluationCriteriaController.cs`, `ProcurementVendorMasterDtos.cs` (ringkasan bobot).

Frontend:

- `need-plan-detail-view.jsx` (pola form baris `lineTable`);
- `purchase-request-detail-view.jsx`, `vendor-status-request-detail-view.jsx` (pola detail + hero + modal alasan);
- `procurement-document-tabs.jsx`;
- `vendor-master-data-service.js`;
- `vendor-detail-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas (`V2QuilvianSystemFrontendDev`) | Status | Isi |
| --- | --- | --- |
| `src/lib/services/corporate/procurement/vendor-evaluation-service.js` | Baru | Metadata, ringkasan, daftar, detail, riwayat, buat draf, simpan nilai, finalkan (`Idempotency-Key`), revisi, batal |
| `src/lib/hooks/corporate/procurement/use-vendor-evaluation-list.jsx` | Baru | Daftar `-09` (ringkasan mengikuti saringan kategori dan periode) atau penilaian satu vendor untuk tab Penilaian (dikunci `supplierId`, tanpa ringkasan) |
| `src/lib/hooks/corporate/procurement/use-vendor-evaluation-detail.jsx` | Baru | Form draf baru (kategori = kategori vendor; peringatan bobot dari ringkasan master kriteria), isian nilai per kriteria, simpan, finalkan (menyimpan perubahan dulu bila ada), revisi, batal; penanda penilai dari `selectUserInfo`; `PBJ-VAL-090`/`093` → data basi |
| `src/components/view/corporate/procurement/vendors/vendor-evaluation-list-view.jsx` | Baru | Layar `-09` dan pembentuk kolom yang dipakai bersama tab Penilaian |
| `src/components/view/corporate/procurement/vendors/vendor-evaluation-detail-view.jsx` | Baru | Layar `-10`: form buat, kepala penilaian, tabel nilai per kriteria, rekomendasi, catatan, tab Lampiran + Riwayat, modal Finalkan/Buat revisi/Batalkan |
| `src/components/view/corporate/procurement/vendors/vendor-evaluation-tab.jsx` | Baru | Tab Penilaian `-02` |
| `src/components/view/corporate/procurement/vendors/vendor-detail-view.jsx` | Diubah | Tab **Penilaian** (urutan arsitektur bagian 4.2) hanya bagi pemegang `ProcurementVendorEvaluation : Read` |
| `src/app/corporate/procurement/vendors/evaluations/page.jsx` | Baru | Entry `-09` |
| `src/app/corporate/procurement/vendors/evaluations/new/page.jsx` | Baru | Entry `-10` baru (`?supplierId=&itemCategoryId=`) |
| `src/app/corporate/procurement/vendors/evaluations/[slug]/page.jsx` | Baru | Entry `-10` detail; token bukan GUID → 404 |
| `src/app/corporate/procurement/vendors/[slug]/page.jsx` | Diubah | `?tab=evaluations` diterima |
| `src/components/view/corporate/procurement/shared/procurement-document-tabs.jsx` | Diubah | Prop opsional `showApproval` (bawaan `true`); penilaian memakai `false` karena tidak lewat alur persetujuan |
| `src/components/view/corporate/procurement/shared/procurement-status-badge.jsx` | Diubah | `kind` baru `vendorEvaluation`; bawaan tidak berubah |

Tidak ada stylesheet baru. Kelas yang dipakai (`card`, `cardHeader`, `cardTitle`, `formGrid`, `summaryList`, `flagRow`, `actionRow`, `filterRow`, `tableScroll`, `lineTable`, `cellField`, `cellFieldWide`, `numeric`, `amount`, `muted`) sudah ada di `procurement.module.css`.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: `page.jsx` → view → hook → service `InstanceAxios`. Tidak ada Redux slice baru.
- Tabel nilai per kriteria memakai pola form baris rencana kebutuhan (`lineTable` dengan `BaseSelectField` dan `BaseTextField` di sel), sesuai kolom "Kemampuan existing" di roadmap.
- Validasi di layar hanya untuk isian kunci (vendor, kategori, periode). `PBJ-VAL-104`, `108`..`111`, `093` dibaca dari server (`PBJ-FE-003`).

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul `-09` | `REUSE` | `Hero` tanpa tombol (`PBJ-FE-004`) |
| Judul `-10` dengan aksi | `REUSE` | `Hero` + `actions`, pola detail PR |
| Kartu ringkasan | `REUSE` | `SummaryGrid` (Draf, Final, Digantikan Revisi, Dibatalkan, Rata-rata Nilai Akhir) |
| Baris filter + "+ Buat Penilaian" | `REUSE` | `DataFilter` (`actions`) + `periodFilterControls` + `FilterSelect` ×4 + `pageSizeControl` |
| Tabel | `REUSE` | `DataTable` + `Pagination`; tab Penilaian tanpa paginasi (`pagination={false}`) |
| Isian | `REUSE` | `BaseSelectField` (vendor `serverSide`, kategori, tahun, jenis periode, periode, nilai, rekomendasi), `BaseTextField`, `BaseTextAreaField` |
| Konfirmasi Finalkan/Buat revisi/Batalkan | `REUSE` | `ConfirmModal` (`requireReason`, `children` untuk pesan server) |
| Notifikasi, galat, akses ditolak | `REUSE` | `ToastStack`, `InformationAlert`, `AccessDeniedGate` |
| Tabel nilai per kriteria | `COMPOSE` | `lineTable` + base field di sel, pola `need-plan-detail-view.jsx` |
| Tab Lampiran + Riwayat tanpa Persetujuan | `EXTEND` (komponen bersama modul) | `ProcurementDocumentTabs` diberi prop opsional `showApproval`; bawaan tetap |
| Lencana status penilaian | `EXTEND` (komponen bersama modul) | `ProcurementStatusBadge` diberi `kind` `vendorEvaluation`; bawaan tetap |

`EXTEND` menyasar komponen milik modul Pengadaan di `procurement/shared/`, bukan base component. Prop-nya opsional dan perilaku bawaannya tidak berubah.

Pilihan untuk elemen bukan `REUSE`:

1. **Tabel nilai per kriteria**
   1. `COMPOSE` `lineTable` + base field **(rekomendasi)** — pola form baris yang diminta roadmap; tampilan sama dengan rencana kebutuhan.
   2. `DataTable` dengan sel berisi field — `DataTable` ditujukan untuk daftar baca; isian di dalamnya belum dipakai modul lain.
2. **Tab tanpa Persetujuan**
   1. `EXTEND` `ProcurementDocumentTabs` dengan `showApproval` **(rekomendasi)** — satu komponen tab untuk semua dokumen Pengadaan; satu baris prop.
   2. Tab sendiri di view penilaian — menyalin pola tab dan panel lampiran/riwayat.
3. **Lencana status**
   1. `EXTEND` `ProcurementStatusBadge` **(rekomendasi)** — sama dengan status dokumen Pengadaan lain.
   2. Lencana terpisah — menduplikasi pemetaan label dan nada.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` pada base component.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
| --- | --- |
| Memuat | `DataTable loading` "Mengambil penilaian..." / "Mengambil penilaian vendor..."; `-10` "Memuat penilaian..." |
| Kosong | `-09` "Belum ada penilaian pada saringan ini."; tab "Belum ada penilaian."; kategori "Vendor belum punya kategori. Atur kategori di Detail Vendor." |
| Gagal | `InformationAlert` "Penilaian gagal dimuat." / "Penilaian vendor gagal dimuat." + **Coba lagi**; `-10` galat + Coba lagi |
| Tidak ditemukan | `-10` "Penilaian tidak ditemukan."; token bukan GUID → 404 |
| Akses ditolak | `-09` → `AccessDeniedGate`; tab Penilaian tidak tampil tanpa `ProcurementVendorEvaluation : Read` dan `?tab=evaluations` jatuh ke Profil; `-10` baru tanpa `Create` → Akses Ditolak |
| Peringatan sebelum draf | Total bobot kriteria aktif ≠ 100% → peringatan di form; tanpa izin baca master kriteria peringatan tidak tampil dan server tetap menolak (`PBJ-VAL-108`) |
| Validasi server | `PBJ-VAL-108`, `109` di form buat; `PBJ-VAL-110` dan `104` di modal (modal tetap terbuka); `PBJ-VAL-111` bila tetap dikirim |
| Data basi | 409 `PBJ-VAL-090`/`093` → "Penilaian sudah diubah orang lain." + **Muat ulang** |
| Pengiriman ganda | Tombol nonaktif saat memproses; satu `Idempotency-Key` per niat Finalkan |
| Penjaga orang yang sama | Finalkan tidak tampil bagi penilai; server tetap menolak (`PBJ-FE-003`) |
| Hanya baca | Tanpa `Create`: tanpa "+ Buat Penilaian"; penilaian non-Draf atau tanpa `Update`: nilai tampil sebagai teks, hanya tombol yang diizinkan |

---

## 5. Endpoint yang dikonsumsi

Base `api/v1/corporate/procurement-management`, semua lewat `InstanceAxios` dengan `AbortController`.

| Method dan path | Dipakai di | Hak akses |
| --- | --- | --- |
| `GET /vendor-evaluations/filters/metadata`, `GET /vendor-evaluations/summary`, `GET /vendor-evaluations` | `-09`; tab Penilaian (`supplierId`); pilihan jenis periode dan rekomendasi `-10` | `ProcurementVendorEvaluation : Read` |
| `GET /vendor-evaluations/{id}`, `/{id}/history` | `-10` | `ProcurementVendorEvaluation : Read` |
| `POST /vendor-evaluations` | Form buat `-10` | `ProcurementVendorEvaluation : Create` |
| `PUT /vendor-evaluations/{id}`, `POST /{id}/cancel` | Simpan nilai, Batalkan | `ProcurementVendorEvaluation : Update` |
| `POST /vendor-evaluations/{id}/finalize` + `Idempotency-Key` | Finalkan | `ProcurementVendorEvaluation : Finalize` (bukan penilai) |
| `POST /vendor-evaluations/{id}/revisions` | Buat revisi | `ProcurementVendorEvaluation : Revise` |
| `GET /master-data/evaluation-criteria/summary?itemCategoryId=` | Peringatan bobot sebelum draf | `ProcurementEvaluationCriterion : Read` |
| `GET /vendors`, `GET /vendors/{id}` | Pencarian vendor dan kategori vendor di form buat | `ProcurementVendor : Read` |
| `GET/POST /attachments` (`PBJ_VENDOR_EVALUATION`) | Tab Lampiran `-10` | `ProcurementAttachment : Read, Upload` + hak atas penilaian |

### Delta terhadap kontrak dan rancangan

| ID | Delta | Keputusan |
| --- | --- | --- |
| F-1 | Saringan "Periode penilaian" = dua pilihan: Tahun dan Semester (`periodType=SEMESTER` + `periodNo`) | Arsitektur bagian 4.5 menyebut "tahun + semester"; jenis periode lain (triwulan, tahunan) tetap bisa dibuat di form, tetapi saringannya mengikuti rancangan |
| F-2 | Peringatan bobot memakai ringkasan master kriteria, sehingga butuh `ProcurementEvaluationCriterion : Read` | Kontrak penilaian tidak menyediakan total bobot. Tanpa izin itu peringatan tidak tampil dan server tetap menolak (`PBJ-VAL-108`) |
| F-3 | Kategori di form buat dibatasi kategori yang dilayani vendor (dari detail vendor) | Server menolak kategori lain dengan `PBJ-VAL-108`; pilihan dipersempit supaya penilai tidak menebak |
| F-4 | Finalkan menyimpan perubahan nilai lebih dulu bila ada | Supaya yang difinalkan sama dengan yang terlihat |
| F-5 | Ringkasan `-09` mengikuti saringan kategori dan periode, bukan saringan status/cari | Bentuk `GET /summary` delta D-7 hanya menerima kategori dan periode |
| F-6 | Tab Lampiran `-10` butuh `ProcurementAttachment : Read` | Sama dengan dokumen Pengadaan lain; aktor baca saja tanpa izin itu melihat galat akses di tab Lampiran. Dicatat untuk peta peran |

---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000`. Panggilan `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` dengan database `QuilvianNewDevSukma`. Dev server 3000 dan backend 7184 milik pengguna sedang mati, jadi keduanya dinyalakan sementara untuk uji ini lalu dimatikan lagi.

Aktor non-SuperAdmin `TEST-PBJ … FEVD005`:

| Aktor | Izin |
| --- | --- |
| `pen` (Tim Pengadaan, penilai) | `ProcurementVendorEvaluation : Read, Create, Update, Revise` + `ProcurementVendor : Read` + `ProcurementEvaluationCriterion : Read` + `ProcurementAttachment : Read, Upload` |
| `mgr` (Manajer Pengadaan) | `ProcurementVendorEvaluation : Read, Update, Finalize, Revise` + `ProcurementVendor : Read, Update` + `ProcurementAttachment : Read` |
| `ro` | `ProcurementVendor : Read` + `ProcurementVendorEvaluation : Read` |
| `noeval` | `ProcurementVendor : Read` |

Data uji run `20261119110409`:

- Kategori K1 dengan kriteria Ketepatan waktu 40, Kualitas 40, Layanan purnajual 20 (total 100).
- Kategori K2 dengan total bobot 80.
- Kategori K3 dengan satu kriteria Kepatuhan dokumen 100.
- Supplier S1 "Prima Husada" yang melayani ketiganya.

Skrip: `ui_fe_vd005.cjs`, `fevd005_api_check.py` di scratchpad sesi; tangkapan layar di `shots-fevd005/`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 13 berkas yang berubah | 0 error; 1 warning `react-hooks/set-state-in-effect` (pola muat-data yang sama dengan hook lain) | `PASS` | `fevd005-eslint.log` |
| `npm run lint:errors` (seluruh repo) | exit 0 | `PASS` | `fevd005-lint.log` |
| `npx next build` | `Compiled successfully in 84s`, exit 0; 39 rute `/corporate/procurement/**` termasuk `/vendors/evaluations`, `/vendors/evaluations/new`, `/vendors/evaluations/[slug]` | `PASS` | `build-fevd005.log` |
| Grep anti-regresi pada berkas baru: warna literal, `style=`, `px`/`fontSize`, `button`/`input`/`select` mentah, axios langsung, stylesheet baru | 0 temuan. Satu `<table>` adalah pola `lineTable` modul (sama dengan rencana kebutuhan) | `PASS` | keluaran perintah |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — test-policy: test baru tidak wajib | `NOT RUN` | — |
| Uji layar putaran 1 | 15/16 `PASS`; `UI-01` gagal karena skrip mencari "RATA-RATA", padahal `SummaryGrid` menulis "RATA RATA NILAI AKHIR" | `PARTIAL` (riwayat) | `ui-fevd005.results.run1.json` |
| `UI-01` dan `UI-16` diulang sesudah perbaikan skrip | 2/2 | `PASS` | `ui-fevd005.results.run1-ui01.json` |
| Cek API/DB `API-01`..`05` | 5/5 sesudah `API-01` dirancang ulang (lihat di bawah) | `PASS` | `fevd005_api.results.json` |

`API-01` versi pertama meminta penilai tanpa izin `Finalize` memfinalkan draf sendiri. Hasilnya 403 karena izin, bukan `PBJ-VAL-111`. Cek dirancang ulang: Manajer (punya `Finalize`) membuat revisi K3 sehingga menjadi penilainya, lalu memfinalkannya sendiri.

### Rincian uji layar

| Skenario | Hasil | Bukti |
| --- | --- | --- |
| `UI-01` `-09`: menu "Penilaian Vendor"; hero tanpa tombol; 5 kartu; 10 kontrol filter satu baris (`AT-VD-63`); "+ Buat Penilaian" di baris filter | `PASS` (ulang) | `01-evaluations.png` |
| `UI-02` `AT-VD-30`: K2 → peringatan "Total bobot … 80%" sebelum draf; Buat draf → 422 `PBJ-VAL-108` tampil, tetap di `/new` | `PASS` | `02-val108.png` |
| `UI-03` Tab Penilaian `-02` → "+ Buat Penilaian" (vendor terkunci) → K1 Semester I 2026 → 201 Draf; 3 kriteria salinan 40/40/20; penilai tanpa Finalkan + keterangan; tab Lampiran + Riwayat tanpa Persetujuan | `PASS` | `03-draft.png` |
| `UI-04` `AT-VD-31`: penilaian kedua S1 × K1 × Semester I → 409 `PBJ-VAL-109` tampil; K3 periode sama → 201 | `PASS` | — |
| `UI-05` `AT-VD-32`: nilai 4/5/3 + Lanjutkan → Simpan → nilai akhir server 4,20 | `PASS` | `05-scored.png` |
| `UI-06` `AT-VD-34`: Manajer memfinalkan K3 dengan kriteria kosong → 422 `PBJ-VAL-110` di modal; tetap Draf | `PASS` | — |
| `UI-07` `AT-VD-33`: Manajer memfinalkan K1 → Final terkunci (tanpa isian/Simpan), "Buat revisi" tampil, pemfinal tercatat, `Idempotency-Key` | `PASS` | `07-final.png` |
| `UI-08` `AT-VD-35`: penilai membuat revisi; alasan pendek → 422 `PBJ-VAL-104` modal tetap terbuka; alasan sah → draf revisi (nilai disalin, "Buka penilaian asal"); Layanan 3 → 4 → 4,40 | `PASS` | — |
| `UI-09` `AT-VD-35`: Manajer memfinalkan revisi → 4,40; penilaian asal "Digantikan revisi" 4,20 dengan tombol "Buka revisi" | `PASS` | `09-superseded.png` |
| `UI-10` `AT-VD-36`: penilai mengisi K3 nilai 2 + rekomendasi Blokir → 2,00 | `PASS` | — |
| `UI-11` `AT-VD-36`: Manajer memfinalkan → keterangan "Rekomendasi hanya saran"; vendor S1 tetap `ACTIVE`, tidak diblokir | `PASS` | `11-block-recommendation.png` |
| `UI-12` `-09` saringan: cari run → 3; Status Final (`status=FINAL`) → 2; Tahun + Semester I (`periodType=SEMESTER&periodNo=1`) → 3; Kategori K3 → 1 | `PASS` | `12-filters.png` |
| `UI-13` Tab Penilaian: 3 penilaian, "+ Buat Penilaian"; Profil menampilkan penilaian final terakhir 4,40 | `PASS` | — |
| `UI-14` Baca saja: `-09` tanpa "+ Buat Penilaian"; `-10` hanya Kembali; tab tanpa tombol tambah | `PASS` | — |
| `UI-15` Tanpa izin penilaian: tab Penilaian tidak ada, `?tab=evaluations` jatuh ke Profil; menu tidak tampil; `-09` Akses Ditolak | `PASS` | — |
| `UI-16` Tanpa `pageerror` di empat sesi | `PASS` | 0 |

### Cek API/DB

| ID | Skenario | Hasil |
| --- | --- | --- |
| `API-01` | `AT-VD-33`: pemfinal yang juga penilai revisi memfinalkan sendiri | 403 `PBJ-VAL-111` "Penilaian harus difinalkan orang lain."; revisi uji dibatalkan |
| `API-02` | `AT-VD-33`: `PUT` pada revisi Final | 409 `PBJ-VAL-093` |
| `API-03` | `AT-VD-32`: bobot master K1 diubah menjadi 40/50/10 | Penilaian asal tetap 4,20 dengan bobot salinan 40/40/20 |
| `API-04` | `AT-VD-35`: penilaian asal dan revisi | `SUPERSEDED` dan `FINAL` 4,40; revisi menunjuk penilaian asal |
| `API-05` | `AT-VD-36`: rekomendasi Blokir final | Vendor `ACTIVE`, `IsBlacklisted = false`, 0 usulan status |

Uji manual: seluruh jalur di atas dijalankan lewat layar dengan empat aktor; lebar layar 1440 px.

**Tidak dijalankan:** tes unit baru (opsional); `test:uat` (dilarang repository); lebar layar selain 1440 px; unggah lampiran penilaian (komponen lampiran `FE-PBJ-002` sudah diuji di task lain).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-30` bobot 80% → `PBJ-VAL-108` | Terpenuhi | `UI-02` |
| `AT-VD-31` duplikat vendor × kategori × periode → `PBJ-VAL-109`; kategori lain 201 | Terpenuhi | `UI-04` |
| `AT-VD-32` 40/40/20 × 4/5/3 = 4,20; tetap setelah master berubah | Terpenuhi | `UI-05`; `API-03` |
| `AT-VD-33` penilai tidak memfinalkan sendiri; Manajer memfinalkan; Final terkunci | Terpenuhi | `UI-03`, `UI-07`; `API-01`, `API-02` |
| `AT-VD-34` finalkan dengan kriteria kosong → `PBJ-VAL-110` | Terpenuhi | `UI-06` |
| `AT-VD-35` revisi Final lalu finalkan; keduanya terbaca | Terpenuhi | `UI-08`, `UI-09`; `API-04` |
| `AT-VD-36` rekomendasi Blokir tidak mengubah vendor | Terpenuhi | `UI-11`; `API-05` |
| Finalkan tersembunyi bagi penilai | Terpenuhi | `UI-03`, `UI-08` |
| `AT-VD-63` satu baris filter `-09` | Terpenuhi | `UI-01` |
| Hak akses: baca saja dan tanpa izin | Terpenuhi | `UI-14`, `UI-15` |
| DoD: lint dan build | Terpenuhi | Tabel verifikasi |
| DoD: laporan ini dan status roadmap | Terpenuhi | Berkas ini; roadmap dan traceability ditandai ✅ |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status task | ✅ Selesai 19 November 2026 |
| Dependency backend | Tidak ada perubahan backend pada task ini |
| Data uji | Run `20261119110409` di `QuilvianNewDevSukma`. **Sudah dibersihkan 19 November 2026** atas izin pemilik ("lanjutkan" atas tawaran pembersihan): dry run lalu apply `cleanup_vendor_fixture.py`, satu transaksi; dry run ulang tidak menemukan sisa; riwayat dibiarkan sebagai jejak. Isinya: 1 unit, 4 jabatan, 4 akun, policy, 3 kategori, 6 kriteria (bobot K1 diubah menjadi 40/50/10 oleh `API-03`), 1 supplier + profil + kategori, 5 penilaian beserta riwayatnya. Id di `fevd005.fixture.json` dan `fevd005.ui-state.json` scratchpad |
| Risiko tersisa | Peringatan bobot butuh `ProcurementEvaluationCriterion : Read` (F-2), dan tab Lampiran butuh `ProcurementAttachment : Read` (F-6); keduanya perlu dicentang untuk peran Tim Pengadaan/Manajer di Akses Role. Nama submenu menu 10 masih label sementara (`PBJ-OQ-033`) |
| Task berikutnya | Seluruh task roadmap `manajemen-vendor` revisi 1 selesai (13/13). Langkah berikutnya yang disarankan: audit kesiapan sub-modul (`verify-module-readiness`) |
