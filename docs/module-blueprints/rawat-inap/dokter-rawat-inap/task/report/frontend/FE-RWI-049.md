# Laporan Perubahan Frontend — `FE-RWI-049`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-049` |
| Judul | Pemeriksaan penunjang laboratorium dan radiologi |
| Slice | `DOK-MVP-FE` urutan 7 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap.md` §3 kartu `FE-RWI-049` |
| Trace | `FE-DOK-07`; `03-frontend-architecture.md` §3.7; `INV-DOK-12`; `RUL-DOK-02`; `VAL-DOK-22`, `VAL-DOK-23`, `VAL-DOK-30`, `VAL-DOK-31` |
| Contract version | `0.3.0` — `approved` oleh Muhammad Hamzah, 3 September 2026 |
| Wewenang UI | `skema-tampilan-dokter-rawat-inap.md` §4–5, §12, §17–20, §22 dan rules §1 roadmap |
| Dependency | `FE-RWI-043` ✅ selesai; `BE-RWI-052` ✅ selesai |
| Klasifikasi | `MEDIUM` — dua service baru, satu hook, satu utility, satu constant, tiga komponen domain, satu stylesheet. Nol base component baru |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | Dikerjakan di atas `52b07d363e92525739fb2ad63075ec80f6d4e230`, branch `HamzahV2`. Source-nya kemudian **di-commit pemilik pekerjaan sendiri** sebagai `e194509dc`; agent tidak menjalankan satu pun tindakan Git |
| Commit backend yang dijadikan rujukan | `3a6373e90e5a590bfad1ba214c5c941e602fc245`, branch `MHamzah` |
| Tanggal | 8 September 2026 |
| Status | ✅ `SELESAI`. Kelima acceptance criteria terpetakan ke source yang ada dan seluruh validasi dijalankan. Butir DoD bukti visual dikecualikan atas keputusan pengguna bahwa e2e dan `.mjs` bukan gerbang selesai. Tidak ada isu §6 yang tersisa untuk task ini |

---

## 1. Keadaan yang ditemukan di awal

Tab **Penunjang** masih berupa kerangka. Yang lebih menentukan: di sisi frontend **tidak ada satu
pun folder service** untuk laboratorium maupun radiologi. `ls src/lib/services/health-services/`
mengembalikan sepuluh folder, dan `laboratory-management` serta `radiology-management` tidak ada
di antaranya.

Anggapan lama yang menjadi akar masalahnya sudah terbukti keliru. `api-contract.md` §8 mencatat:
grup Radiologi tidak ada pada kontrak `0.1.0` **karena modulnya dianggap belum ada**, padahal
modul itu sudah hidup sejak migration `20260828093000_AddRadiologyManagement`. Selama anggapan itu
bertahan, layar dokter menampilkan kalimat "pemeriksaan radiologi belum tersedia di sistem" kepada
dokter yang sebenarnya bisa memesannya.

Backend sudah siap: `LabOrderController` dan `RadOrderController` keduanya memiliki
`GET /episodes/{episodeId}` yang mengembalikan `IsResultFinal` beserta `ResultAvailabilityNote`
siap tampil, dibuat `BE-RWI-052`.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter yang merawat pasien rawat inap.

**Kapan layar ini dibuka.** Ketika dokter perlu memesan pemeriksaan penunjang, dan ketika ia perlu
membaca hasil pemeriksaan yang sudah keluar.

**Langkah normalnya, berurutan:**

1. Dokter membuka tab **Penunjang**. Layar menampilkan **dua bagian berjudul terpisah**:
   **Laboratorium** lebih dulu, lalu **Radiologi**. Keduanya tidak pernah dicampur menjadi satu
   daftar.
2. Bagian **Laboratorium** menampilkan Pemeriksaan, Tanggal, Status Order, dan **Hasil**.
3. Bagian **Radiologi** menampilkan Pemeriksaan, **Modalitas**, **Jadwal**, Status Order, dan
   **Hasil**. Pesanan yang belum dijadwalkan berbunyi "Belum dijadwalkan", bukan tanda hubung.
4. Untuk memesan, dokter menekan **Pesan Lab** atau **Pesan Radiologi**. Modal laboratorium
   meminta satu hal: pemeriksaannya. Modal radiologi meminta pemeriksaan, modalitas, dan indikasi
   klinis.
5. Pesanan otomatis melekat pada perawatan yang sedang dibuka, sehingga hasilnya tidak bercampur
   dengan perawatan lain pasien yang sama.

**Membaca hasil.** Kolom Hasil membedakan tiga keadaan, dan ketiganya berbeda kalimat:

| Keadaan | Yang tampil | Artinya |
| --- | --- | --- |
| Final | **HASIL FINAL** bertone hijau | Hasil sudah sah dipakai sebagai dasar keputusan klinis |
| Belum final | **HASIL BELUM FINAL** bertone kuning, **ditambah kalimat merah** "Hasil ini belum final dan belum sah dipakai sebagai dasar keputusan klinis." | Pemeriksaan sudah berjalan, hasilnya belum disahkan |
| Belum dapat dipastikan | **KEFINALAN HASIL BELUM DAPAT DIPASTIKAN** bertone merah, ditambah kalimat larangan yang sama | Layar tidak menerima penanda kefinalan dari server |

Perbedaannya sengaja tidak hanya warna. Peringatan keselamatan yang hanya dibedakan warna hilang
bagi sebagian pembaca, dan yang hilang di sini bukan hiasan.

**Yang tidak ada di layar ini, dan itu disengaja.** Tidak ada tombol **Input Hasil** bagi siapa
pun. Hasil diterbitkan Laboratorium dan Radiologi; ruang kerja dokter hanya membacanya. Kalimatnya
tertulis di bawah kedua bagian, supaya tidak ada yang mencari tombol yang memang tidak boleh ada.

**Jalur tidak normal:**

- **Belum ada pesanan** — Laboratorium dan Radiologi punya kalimat kosongnya sendiri-sendiri.
- **Gagal memuat salah satu** — hanya bagian itu yang menampilkan galat beserta **Coba Lagi**.
  Laboratorium yang gagal **tidak** menghilangkan Radiologi pasien yang sama.
- **Tanpa hak baca salah satu modul** — gerbang penolakan per bagian; hak baca laboratorium dan
  radiologi memang terpisah.
- **Tanpa hak tulis** — tombol Pesan tidak ditampilkan; membaca hasil tidak pernah ikut tertahan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/frontend-roadmap.md` kartu `FE-RWI-049` dan rules §1.1–§1.4
- `contracts/api-contract.md` §7, §8, dan §11
- `skema-tampilan-dokter-rawat-inap.md` §12 beserta §12.1, §12.2, dan bagian Result State
- `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` (read-only)
- `Areas/HealthServices/LaboratoryManagement/Controllers/LabCatalogController.cs` (read-only)
- `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs`, `LabCatalogDtos.cs` (read-only)
- `Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs` (read-only)
- `Areas/HealthServices/RadiologyManagement/Controllers/RadStudyController.cs` (read-only)
- `Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs` (read-only)
- `Areas/HealthServices/MasterData/Controllers/ProcedureController.cs` (read-only)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/laboratory-management/lab-order.service.js` | **Baru**, beserta foldernya. Tiga fungsi baca dan satu fungsi pesan. **Nol** fungsi penulisan hasil |
| `src/lib/services/health-services/radiology-management/rad-order.service.js` | **Baru**, beserta foldernya. Idem |
| `src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx` | **Baru.** Tiga keadaan kefinalan beserta label dan tone-nya, dan seluruh salinan teks |
| `src/utils/health-services/inpatient-management/inpatient-supporting-service-utils.jsx` | **Baru.** Penentu kefinalan tiga arah, normalisasi pesanan, penapis isolasi perawatan, dan penyusun payload |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx` | **Baru.** Membaca kedua daftar secara mandiri, ditambah katalog pemeriksaan dan modalitas |
| `…/tabs/supporting-service/supporting-service-tab.jsx` | Kerangka diganti isi sebenarnya: dua section berurutan |
| `…/tabs/supporting-service/supporting-order-section.jsx` | **Baru.** Satu section beserta definisi kolom Lab dan Radiologi yang terpisah |
| `…/tabs/supporting-service/supporting-result-finality.jsx` | **Baru.** Penanda kefinalan beserta kalimat larangannya |
| `…/tabs/supporting-service/supporting-order-modal.jsx` | **Baru.** Formulir pesan; tidak memuat satu pun field hasil |
| `src/style/health-services/inpatient-management/physician-supporting-service.module.css` | **Baru.** |
| `tests/unit/inpatient-physician-clinical-tabs.test.mjs` | Tiga uji khusus task ini ditambahkan |

### 3.3 Kepatuhan arsitektur frontend

Alur `constants → utils → service → hook → view` diikuti. Base yang dipakai ulang:
`ClinicalDataTable`, `ClinicalStateBoundary`, `ClinicalActionGuard`, `ClinicalSafetyAlert`,
`ClinicalAuditBadge`, `ClinicalEmptyState`, `BaseButton`, `BaseNativeSelectField`,
`BaseTextAreaField`, `ConfirmModal`.

**Nol base component baru dibuat.** `ClinicalSegmentedNav` milik `FE-RWI-048` **tidak** dipakai,
sesuai roadmap §1.2 yang menyatakan layout dua section pada revision ini tidak memerlukannya —
sehingga tidak ada dependency baru antar-cabang yang dibuat.

**Dua folder service baru** dibuat karena keduanya memang belum pernah ada di repository. Bentuk
berkasnya mengikuti persis pola service existing: satu konstanta base URL, satu `unwrapApiResponse`
lokal, fungsi bernama jelas, lalu satu objek default export.

**Penapisan isolasi perawatan berlapis dua.** Penyaring episode ditegakkan server lewat
`GET /episodes/{episodeId}`; `filterOrdersByEpisode` di layar adalah lapis kedua yang menangkap
baris tanpa penanda perawatan atau bertanda perawatan lain. Lapis kedua ini tidak menggantikan
lapis pertama, dan tanpa `episodeId` yang jelas ia mengembalikan daftar kosong — bukan seluruh
baris.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membaca pesanan laboratorium..." dan "Membaca pesanan radiologi..." berdiri sendiri-sendiri |
| Kosong | "Belum ada pesanan laboratorium pada perawatan ini." dan "Belum ada pesanan radiologi pada perawatan ini." adalah dua kalimat berbeda, masing-masing menegaskan bahwa pesanan perawatan lain tidak ikut ditampilkan |
| Gagal | Judul galat per bagian beserta **Coba Lagi**; bagian lainnya tetap terbaca |
| Tanpa hak akses | Gerbang penolakan per bagian, karena hak baca Lab dan Rad memang terpisah |
| Tanpa hak tulis | Tombol Pesan tidak dirender; membaca hasil tetap berjalan |
| Hanya baca | Seluruh hasil hanya baca bagi peran mana pun; tidak ada aksi pengisian hasil |
| Episode ditutup | Jalur pemesanan tertutup lewat penjaga kewenangan; daftar dan hasil tetap terbaca |
| Kefinalan tidak diketahui | Disebut "KEFINALAN HASIL BELUM DAPAT DIPASTIKAN" beserta kalimat larangan — bukan FINAL, dan bukan "belum ada pemeriksaan" |
| Kiriman ganda | Tombol konfirmasi modal nonaktif selama permintaan berjalan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/episodes/{episodeId}` | Membaca pesanan dan ketersediaan hasil satu perawatan | `LabOrder : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-orders` | Memesan pemeriksaan beserta `InpEpisodeId` | `LabOrder : Create` |

#### Health Services / Laboratory Management / Lab Catalog

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-catalog/examinations` | Mengisi pilihan pemeriksaan pada modal | `LabCatalog : Read` |

#### Health Services / Radiology Management / Rad Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/radiology-management/rad-orders/episodes/{episodeId}` | Membaca pesanan, modalitas, jadwal, dan ketersediaan hasil satu perawatan | `RadOrder : Read` |
| `POST` | `/v1/health-services/radiology-management/rad-orders` | Memesan pemeriksaan beserta `InpEpisodeId` | `RadOrder : Create` |

#### Health Services / Radiology Management / Rad Study

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/radiology-management/rad-studies/modalities` | Mengisi pilihan modalitas pada modal | `RadStudy : Read` |

#### Health Services / Master Data / Procedure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/procedures?isRadiology=true` | Mengisi pilihan pemeriksaan radiologi pada modal | `Procedure : Read` |

**Tidak ada satu pun endpoint penulisan hasil yang dipanggil, dan tidak ada yang tersedia.**
`api-contract.md` §11 menyatakannya disengaja lewat `RUL-DOK-02`; laporan `BE-RWI-052` mencatat
dua uji arsitektur yang membuktikan nol tabel salinan hasil dan nol permukaan pemesanan yang
menerima isi hasil.

**Delta kontrak yang dicatat:** kontrak §7 dan §8 menandai `GET /episodes/{episodeId}` sebagai
**Rencana**, padahal keduanya sudah hidup di backend. Selain itu, kontrak tidak menyebut sumber
pilihan pemeriksaan dan modalitas; ketiga endpoint katalog di atas dipakai karena keduanya
memang satu-satunya sumber yang ada, dan seluruhnya hanya dibaca.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Keluaran `eslint . --quiet` kosong |
| `npm run test:unit` | 542 uji lulus, 0 gagal | `PASS` | `tests 542 / pass 542 / fail 0`; 21 di antaranya uji baru `tests/unit/inpatient-physician-clinical-tabs.test.mjs` |
| `npm run build` | Berhasil beserta `postbuild` | `PASS` | `✓ Compiled successfully in 33.2s` |
| Final, belum final, dan tidak diketahui menghasilkan tiga kalimat berbeda; hanya yang pertama dianggap hasil sah | Ketiganya sesuai | `PASS` | `FE-RWI-049 K2` |
| Pesanan perawatan lain dan pesanan tanpa penanda perawatan tidak ikut tampil | Hanya baris perawatan yang cocok yang lolos; tanpa `episodeId`, nol baris | `PASS` | `FE-RWI-049 K3` |
| Pemindaian kalimat "radiologi belum tersedia di sistem" dan "Input Hasil" pada lima berkas | Nol hasil pada keduanya | `PASS` | `FE-RWI-049 K4/K5` |
| Grep warna literal, `!important`, dan `<table>` mentah | Nol hasil | `PASS` | Grep checklist konsistensi UI |
| Verifikasi interaktif di peramban | Tidak dijalankan | `NOT RUN` | Lihat catatan di bawah |

**Uji manual:** `NOT FEASIBLE`.

**Alasan konkret.** Tidak ada `playwright.config.*` di akar repository sehingga
`npm run test:e2e` tidak dapat dijalankan tanpa menambah konfigurasi baru. Pengujian bermakna
juga menuntut data master pemeriksaan laboratorium dan radiologi beserta modalitasnya —
tertahan `RWI-UI-GAP-007`.

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat`, dan screenshot dua section beserta
detail hasil pada tiga viewport.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Laboratorium dan radiologi tampil sebagai **dua daftar yang jelas** | Terpenuhi | `supporting-service-tab.jsx` merender dua `SupportingOrderSection` berurutan dengan judul, deskripsi, kolom, batas state, dan tombol pesan masing-masing. Kolomnya pun berbeda: Radiologi membawa Modalitas dan Jadwal yang tidak ada pada Laboratorium |
| 2. Hasil yang belum final tampil dengan penanda dan **tidak** terlihat sama dengan hasil final | Terpenuhi | `resolveResultFinality` membedakan tiga keadaan, dan `SupportingResultFinality` menambahkan kalimat larangan merah pada dua keadaan yang bukan final. Uji `FE-RWI-049 K2` |
| 3. Hasil milik perawatan lain **tidak ikut tampil** | Terpenuhi | Penyaring episode ditegakkan server lewat `GET /episodes/{episodeId}`, dan `filterOrdersByEpisode` menjadi lapis kedua di layar. Uji `FE-RWI-049 K3` |
| 4. Kalimat "pemeriksaan radiologi belum tersedia di sistem" **tidak ada lagi** di mana pun | Terpenuhi | Uji pemindaian `FE-RWI-049 K4/K5` atas lima berkas mengembalikan nol hasil. Bagian Radiologi justru berfungsi penuh: baca, pesan, modalitas, dan jadwal |
| 5. Tidak ada tombol mengisi hasil | Terpenuhi | Uji pemindaian yang sama membuktikan nol kemunculan "Input Hasil". Kedua service pun tidak memuat satu pun fungsi penulisan hasil |

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Kelima acceptance existing terbukti | Terpenuhi |
| State dan permission terbukti | Terpenuhi pada source dan tabel bagian 4; belum diverifikasi di peramban |
| Visual Acceptance Criteria terbukti | **Belum diverifikasi di peramban.** Dikecualikan atas keputusan pengguna bahwa e2e dan `.mjs` bukan gerbang selesai |
| Gate §4.1 relevan (butir 15, 16, 18, 19) | Terbukti pada source: Lab dan Rad terpisah jelas, belum final berbeda dari final tanpa Input Hasil, kosong berbeda dari gagal, tidak ada global finalize |
| Laporan menyertakan bukti final versus non-final, pemisahan Lab/Rad, dan isolasi perawatan | Terpenuhi pada bagian 6 dan 7 |
| Tidak membuat hasil salinan atau aksi input hasil | Terpenuhi |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` pada scope task ini. Satu uji struktural `FE-RWI-043` sempat gagal karena tab ini belum memakai penjaga kewenangan secara eksplisit; **diperbaiki** dengan menambahkan `ClinicalActionGuard` pada berkas tab |
| Masalah yang diketahui | Baris registry `RadiologyManagement / Rad` masih `PLANNED` menurut laporan `BE-RWI-052`, dan itu tetap utang terbuka di sisi backend. Layar ini tidak terdampak karena hanya membaca dan memesan lewat endpoint yang sudah ada |
| Dependency backend | `BE-RWI-052` ✅ selesai 4 September 2026. Tidak ada bagian layar ini yang tertahan backend |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Saat pekerjaan agent selesai, berkas task ini muncul sebagai `??` dan `M` pada branch `HamzahV2`, dan **tidak ada tindakan Git yang dijalankan agent** — tanpa `git add`, commit, push, merge, maupun rebase. Pemilik pekerjaan kemudian meng-commit sendiri sebagai `e194509dc`, sehingga `git status --short` pada repository frontend kini bersih |
| Langkah berikutnya | Menjalankan verifikasi interaktif begitu data master pemeriksaan dan modalitas tersedia, lalu melampirkan bukti tiga viewport beserta tampilan detail hasil |
