# Laporan Perubahan Frontend — `FE-RWI-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-033` |
| Judul | Tidak ada lagi layar yang tidak dapat dicapai |
| Slice | `F13 — Perapian dan kesiapan` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 5, `FE-RWI-033` |
| Trace | `IA-INP-01` s.d. `IA-INP-05`; [`03-frontend-architecture.md`](../../../03-frontend-architecture.md) bagian 11A |
| Skema tampilan | [`05-skema-tampilan.md`](../../../05-skema-tampilan.md) — register bagian 2, peta navigasi bagian 23, klasifikasi bagian 24 |
| Contract version | Tidak ada kontrak baru. `GET /census/filters/metadata` sudah tersedia sejak `0.2.0` dan tidak berubah; task ini hanya mulai memakainya |
| Wewenang UI | Brief UI pemilik 28 Agustus 2026 mengunci induk dan urutan tujuh menu operasional. Ikon, warna, jarak, dan bentuk expand/collapse tetap `DEV_DISCRETION`. Batasnya kelima aturan `IA-INP` |
| Dependency | `FE-RWI-020` s.d. `FE-RWI-032` — seluruhnya selesai |
| Klasifikasi | `MEDIUM` — empat berkas source disunting pada tiga lapisan (menu, utility, hook, view), tanpa route, komponen, maupun endpoint baru |
| Task mode | `FRONTEND` — backend strict read-only, kecuali berkas laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; berkas laporan ini beserta roadmap dan `requirement-traceability.md` modul Rawat Inap |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `2535c1303` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `3d14cac` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Ketujuh acceptance criteria terpenuhi. `npm run lint` `0 errors` — 571 warning, sama persis dengan garis dasar dan nol pada berkas task ini; `npm run build` `✓ Compiled successfully in 34.0s`. Test `.mjs` `NOT RUN` atas arahan pengguna |

---

## 1. Keadaan yang ditemukan di awal

Tiga hal ditemukan, dan hanya dua di antaranya benar-benar rusak.

**Pertama, submenu `Rawat Inap` berisi sembilan butir, bukan tujuh.** Dua di antaranya —
**Butir Administrasi** dan **Pengaturan Rawat Inap** — bukan layar kerja harian, melainkan
layar pengelola master. Keduanya mengelola `MstInpatientClearanceItem` dan
`MstInpatientSetting`, yang pada arsitektur backend maupun permission matrix memang dimiliki
bounded context **Health Services / Master Data**, bukan Rawat Inap. Menaruhnya di antara
Selisih Tempat Tidur dan layar kerja lain membuat petugas ruangan melihat dua butir yang
tidak pernah mereka pakai, sementara admin master data harus mencarinya di tempat yang salah.

**Kedua, penyaring layar Pasien Sedang Dirawat mengambil pilihannya dari master lengkap.**
`use-inpatient-census.jsx` memanggil `useSelectResource("serviceUnits")` dan
`useSelectResource("patientClasses")`, yang menjawab **seluruh** unit layanan dan **seluruh**
kelas pasien — termasuk poliklinik dan kelas rawat jalan. Petugas dapat memilih unit
poliklinik sebagai penyaring census, lalu menerima daftar kosong tanpa penjelasan, padahal
memang tidak akan pernah ada pasien rawat inap di sana. Sementara itu
`GET /census/filters/metadata` yang sudah tersedia sejak kontrak `0.2.0` sama sekali tidak
dipanggil — persis "endpoint tak bertuan" yang dilarang bagian 11A.

**Ketiga — dan ini yang ternyata tidak rusak — keterjangkauan layar.** Penelusuran satu per
satu membuktikan kesembilan belas layar sudah punya jalan masuk, termasuk Sesi Koreksi yang
pada roadmap revision `3` masih ditandai tidak terjangkau. Fakta itu sudah dikoreksi lebih
dulu oleh impact scan 28 Agustus pada [`05-skema-tampilan.md`](../../../05-skema-tampilan.md)
bagian 24, dan penelusuran task ini mengonfirmasinya. Tabel jalur lengkapnya ada di
bagian 6.3.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Siapa membuka apa, dan dari mana

Modul Rawat Inap dipakai tiga kelompok orang yang kebutuhannya berbeda:

1. **Petugas ruangan, petugas admisi, perawat, dan DPJP** — membuka layar kerja harian.
   Semuanya sekarang berada di satu tempat: `Pelayanan Kesehatan → Rawat Inap`, tujuh butir,
   berurutan dari yang paling sering dipakai.
2. **Supervisor** — memakai layar yang sama, ditambah Sesi Koreksi yang hanya muncul pada
   episode yang sudah ditutup.
3. **Admin master data** — mengatur butir administrasi pulang dan pengaturan Rawat Inap.
   Keduanya kini berada di `Pelayanan Kesehatan → Master Data`, bersama Unit Layanan,
   Ruangan, Tempat Tidur, dan Kelas Pasien yang memang sudah lama tinggal di sana.

### 2.2 Yang berubah di layar

**Menu.** Sebelum perubahan, membuka Pengaturan Rawat Inap berarti masuk ke submenu
`Rawat Inap` lalu menggulir sampai butir kesembilan. Sesudahnya, jalannya adalah
`Pelayanan Kesehatan → Master Data → Pengaturan Rawat Inap`. Alamat halamannya **tidak
berubah** — tetap `/health-services/inpatient-management/settings` — sehingga tautan yang
sudah disimpan pengguna, di-bookmark, atau ditempel pada dokumen internal tetap bekerja.
Hal yang sama berlaku untuk Butir Administrasi Rawat Inap.

Label `FE-INP-13` juga diperjelas dari **Butir Administrasi** menjadi **Butir Administrasi
Rawat Inap**. Di dalam submenu `Rawat Inap` kata "Rawat Inap" memang berlebihan; setelah
butirnya duduk di antara puluhan master lain, tanpa kata itu pembacanya tidak tahu butir
administrasi milik modul mana.

**Penyaring census.** Sebelumnya daftar Unit Layanan pada layar Pasien Sedang Dirawat memuat
seluruh unit rumah sakit. Sesudahnya, daftar itu hanya memuat unit layanan bertipe rawat inap
dan kelas perawatan yang berlaku untuk rawat inap, karena isinya kini datang dari
`GET /census/filters/metadata` — penyaringan yang memang milik server. Petugas tidak lagi
dapat memilih penyaring yang hasilnya sudah pasti kosong.

Contoh konkretnya: rumah sakit dengan 24 unit layanan yang hanya 5 di antaranya bangsal rawat
inap. Sebelumnya pilihan Unit Layanan berisi 24 butir, 19 di antaranya menjamin daftar kosong.
Sesudahnya pilihan itu berisi 5 butir, semuanya dapat menghasilkan baris.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Pilihan penyaring gagal dimuat | Layar **tetap** berdiri. Kedua daftar penyaring kosong beserta kalimat "Unit layanan rawat inap tidak ditemukan" atau "Kelas perawatan rawat inap tidak ditemukan"; pencarian teks bebas dan daftar pasien tetap berjalan. Pilihan unit dan kelas sengaja **tidak** dikarang di layar: mengarangnya akan membuat petugas menyaring memakai unit yang tidak ada |
| Jumlah baris per halaman gagal dimuat | Cadangan `10 / 25 / 50 / 100` dipakai. Daftar ini hanya soal tampilan, jadi ia boleh punya cadangan |
| Daftar pasien gagal dimuat | Pesan gagal beserta tombol **Coba Lagi**, seperti sebelumnya; tidak berubah oleh task ini |
| Pengguna membuka `/health-services/inpatient-management/settings` langsung dari alamat lama | Tetap terbuka. Route tidak dipindahkan |
| Pengguna tidak berhak membuka layar master | `AccessDeniedGate` menahan seperti sebelumnya. Permission tidak diubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Untuk apa |
| --- | --- |
| `docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md` | Acceptance criteria, dependency, batas scope |
| `docs/module-blueprints/rawat-inap/05-skema-tampilan.md` bagian 2, 23, 24 | Register 19 layar, peta navigasi target, aturan migrasi menu |
| `docs/module-blueprints/rawat-inap/03-frontend-architecture.md` bagian 11A | Cakupan endpoint yang mengikat |
| `docs/module-blueprints/rawat-inap/contracts/api-contract.md` | Daftar lengkap operasi yang wajib punya pemilik |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientCensusController.cs` | Kontrak `GET /census/filters/metadata` dan hak aksesnya |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientCensusDtos.cs` | Bentuk `CensusFilterMetadataResponse` |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Isi metadata yang benar-benar dihasilkan server |
| `src/utils/menu-sidebar/menu-items.jsx` | Susunan menu as-is |
| `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx` | Cara `key`, `subMenu`, dan `subItems` dirender |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx` | Pola pembacaan `filters/metadata` yang sudah mapan di modul ini |
| `src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx` | Pola `FilterSelect` beserta butir "Semua …" |
| `src/lib/constants/health-services/inpatient-management/inpatient-dashboard-constants.jsx` | Tautan cepat beranda |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Tautan menuju layar per-episode |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Definisi menu `FE-INP-12` dan `FE-INP-13` **dipindahkan** dari `subMenu` grup `Rawat Inap` ke akhir `subItems` grup `Master Data` Pelayanan Kesehatan. Label `FE-INP-13` menjadi **Butir Administrasi Rawat Inap**. `key`, `pathname`, dan ikon dibawa apa adanya |
| `src/utils/health-services/inpatient-management/inpatient-census-utils.jsx` | Fungsi murni baru `normalizeCensusFilterMetadata` beserta pembantu `normalizeOptions`, membaca `CensusFilterMetadataResponse` menjadi tiga daftar pilihan |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-census.jsx` | `useSelectResource` untuk `serviceUnits` dan `patientClasses` dilepas, diganti satu `useEffect` yang membaca `GET /census/filters/metadata` dengan `AbortController`. Hook kini mengembalikan `serviceUnitOptions`, `patientClassOptions`, dan `pageSizeOptions` |
| `src/components/view/health-services/inpatient-management/inpatient-census-view.jsx` | Dua `ResourceFilterSelect` diganti `FilterSelect` beroption metadata, masing-masing dengan butir "Semua …" di puncaknya; daftar jumlah baris tidak lagi disusun dari constant di view |
| `tests/unit/inpatient-census.test.mjs` | Satu test baru untuk kriteria 7: hook memanggil `filters/metadata`, tidak lagi memakai `useSelectResource`, dan normalizer membuang pilihan tanpa nilai |
| `tests/e2e/inpatient-census.spec.mjs` | Mock `filters/metadata` ditambahkan **sebelum** cabang census, karena path-nya juga mengandung segmen census |

### 3.3 Kepatuhan arsitektur frontend

- **Alur dependensi utuh.** `view → hook → service → InstanceAxios`. View tidak memanggil
  Axios; hook tidak mengembalikan JSX; normalisasi payload berada di `utils` sebagai fungsi
  murni. Sesuai `rules/frontend/frontend-architecture.md`.
- **Pola sudah ada, bukan pola baru.** Pembacaan `filters/metadata` menyalin pola
  `use-inpatient-episode-worklist.jsx` baris demi baris: `useEffect` sekali jalan,
  `AbortController`, penanda `cancelled`, dan kegagalan metadata yang **tidak** menggagalkan
  layar. Tidak ada arsitektur state, HTTP, maupun abstraksi paralel yang ditambahkan.
- **Endpoint tidak tersebar.** Path `filters/metadata` dipanggil lewat
  `inpatientCensusService`, yang base URL-nya sudah terdaftar di `inpatient-api.service.js`.
- **Menu dipindahkan, bukan diduplikasi.** Aturan migrasi 1 dan 3 pada skema bagian 23.
  Tidak ada entri kembar yang tertinggal.
- **Route tidak dinormalisasi.** Aturan migrasi 4 melarangnya tanpa compatibility redirect
  dan task tersendiri, jadi `pathname` kedua butir dibiarkan apa adanya.

### 3.4 Gerbang keputusan base component

`UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Penyaring unit layanan | `FilterSelect` | `src/components/features/base-features/filter-select.jsx`; dipakai `inpatient-episode-worklist-view.jsx:397` untuk penyaring yang sama persis | `REUSE` | `FilterSelect` dengan `options` dari metadata |
| Penyaring kelas perawatan | `FilterSelect` | berkas yang sama; dipakai `inpatient-episode-worklist-view.jsx:407` | `REUSE` | `FilterSelect` dengan `options` dari metadata |
| Butir menu master data | struktur `subItems` yang sudah ada | `src/utils/menu-sidebar/menu-items.jsx` grup `healthServicesMasterData` | `REUSE` | Objek butir yang sudah ada dipindahkan apa adanya |

Tidak ada baris berstatus bukan `REUSE`, sehingga tidak ada pilihan bernomor yang perlu
diajukan dan tidak ada yang menunggu keputusan pengguna.

**Catatan pemilihan komponen.** `ResourceFilterSelect` dilepas bukan karena kurang baik,
melainkan karena tugasnya memang berbeda: ia pembungkus `useSelectResource`, yaitu jalur
menuju endpoint `options` master data. Begitu sumber pilihannya pindah ke metadata census,
pembungkus itu tidak lagi punya `select` untuk dibungkus, dan `FilterSelect` adalah base
component yang sama yang sudah dipakai layar bersaudaranya untuk penyaring identik.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka `DataTable` beserta kalimat "Mengambil daftar pasien yang sedang dirawat...". Penyaring tetap dapat dibuka; isinya menyusul begitu metadata tiba |
| Kosong | "Belum ada pasien yang dirawat di unit ini." beserta anjuran mengubah penyaring atau membuka admisi |
| Gagal memuat daftar | `InformationAlert` merah beserta tombol **Coba Lagi** |
| Gagal memuat pilihan penyaring | Penyaring berisi kalimat "Unit layanan rawat inap tidak ditemukan" atau "Kelas perawatan rawat inap tidak ditemukan". Layar tetap dapat dipakai |
| Tanpa hak akses | `AccessDeniedGate` menampilkan kalimat penolakan baku; tidak berubah oleh task ini |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Census

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/census/filters/metadata` | Mengisi pilihan Unit Layanan, Kelas Perawatan, dan jumlah baris per halaman pada layar Pasien Sedang Dirawat | `InpatientCensus : Read` |
| `GET` | `/v1/health-services/inpatient-management/census` | Daftar pasien yang sedang dirawat — sudah dipakai sebelum task ini | `InpatientCensus : Read` |

Endpoint yang **berhenti** dipakai layar census: `GET /v1/.../service-units/options` dan
`GET /v1/.../patient-classes/options`. Keduanya tetap dipakai layar master data pemiliknya dan
tidak menjadi yatim.

---

## 6. Verifikasi

### 6.1 Perintah

| Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `✖ 571 problems (0 errors, 571 warnings)` | `PASS` | Sama persis dengan garis dasar yang tercatat pada `FE-RWI-030` s.d. `032`. Penyaringan keluaran atas keempat berkas task ini menghasilkan nol baris |
| `npm run build` | `✓ Compiled successfully in 34.0s`; `postbuild` selesai — "Standalone runtime siap dijalankan." | `PASS` | Keluaran perintah |

`AUTOMATED TEST: SKIPPED (opsional) — atas arahan pengguna 1 September 2026, validasi task ini
dibatasi pada npm run build dan npm run lint; berkas .mjs tidak dijalankan.` Satu test unit
tetap **ditulis** untuk kriteria 7 supaya cakupannya tidak diam-diam berkurang; ketiga fakta
yang diperiksanya diverifikasi manual terhadap source lewat `grep` — pemanggilan
`get("filters/metadata"` ada, `useSelectResource` nol kemunculan pada hook census, dan
`normalizeCensusFilterMetadata` benar-benar diekspor.

### 6.2 Grep anti-regresi UI

Dijalankan atas `inpatient-census-view.jsx` dan `menu-items.jsx`.

| # | Pemeriksaan | Hasil |
| --: | --- | --- |
| 1 | Warna literal | Bersih |
| 2 | Typography menimpa komponen shared | Bersih |
| 3 | Tombol non-base | 1 hit, `inpatient-census-view.jsx:156` — baris **existing** milik tautan "Detail Episode" yang tidak disentuh task ini. Perbaikannya milik `FE-RWI-037` |
| 4 | Tabel mentah | Bersih |
| 5 | Bootstrap utility typography | Hit pada `menu-items.jsx` seluruhnya `className="fs-4"` pada ikon menu — konvensi yang berlaku untuk 155 butir menu di repository, dan kedua baris yang ditambahkan task ini mengikutinya persis. Satu hit `fw-semibold` pada `inpatient-census-view.jsx:84` adalah baris existing |
| 6 | `!important` baru | Bersih |

### 6.3 Penelusuran keterjangkauan seluruh 19 layar

Klik dihitung dari **Beranda Rawat Inap**. Membuka grup menu tidak dihitung sebagai klik
tujuan, sesuai cara hitung skema bagian 23.

| # | Layar | Jalur | Klik | Bukti source |
| --: | --- | --- | :---: | --- |
| 1 | `FE-INP-19` Beranda Rawat Inap | Titik awal | 0 | `menu-items.jsx` butir 1 grup Rawat Inap |
| 2 | `FE-INP-03` Admisi Rawat Inap | Menu Rawat Inap → Admisi Rawat Inap; juga tautan cepat beranda | 1 | `menu-items.jsx`; `inpatient-dashboard-constants.jsx` butir `admission` |
| 3 | `FE-INP-02` Papan Tempat Tidur | Menu Rawat Inap → Papan Tempat Tidur; juga tautan cepat beranda dan tautan dari Selisih Tempat Tidur | 1 | `menu-items.jsx`; `inpatient-dashboard-constants.jsx` butir `bed-board`; `inpatient-bed-drift-view.jsx:179` |
| 4 | `FE-INP-16` Daftar Kerja Episode | Menu Rawat Inap → Daftar Kerja Episode; juga tautan cepat beranda | 1 | `menu-items.jsx`; `inpatient-dashboard-constants.jsx` butir `episodes` |
| 5 | `FE-INP-01` Pasien Sedang Dirawat | Menu Rawat Inap → Pasien Sedang Dirawat; juga tautan cepat beranda | 1 | `menu-items.jsx`; `inpatient-dashboard-constants.jsx` butir `census` |
| 6 | `FE-INP-09` Daftar Pantau | Menu Rawat Inap → Daftar Pantau; juga empat kartu pantau beranda | 1 | `menu-items.jsx`; `inpatient-dashboard-constants.jsx` butir `monitoring` |
| 7 | `FE-INP-10` Selisih Tempat Tidur | Menu Rawat Inap → Selisih Tempat Tidur; juga tautan cepat beranda | 1 | `menu-items.jsx`; `inpatient-dashboard-constants.jsx` butir `bed-drift` |
| 8 | `FE-INP-13` Butir Administrasi Rawat Inap | Master Data → Butir Administrasi Rawat Inap | 2 | `menu-items.jsx` grup `healthServicesMasterData` |
| 9 | `FE-INP-12` Pengaturan Rawat Inap | Master Data → Pengaturan Rawat Inap | 2 | `menu-items.jsx` grup `healthServicesMasterData` |
| 10 | `FE-INP-04` Detail Episode | Daftar Kerja Episode → Detail Episode; juga dari baris census | 2 | `inpatient-episode-worklist-view.jsx`; `inpatient-census-view.jsx` kolom Aksi |
| 11 | `FE-INP-17` Pembatalan Admisi | Daftar Kerja Episode → **Batalkan Admisi** pada baris; juga dari Detail Episode | 2 | `inpatient-episode-worklist-view.jsx`; `inpatient-episode-detail-view.jsx:386` |
| 12 | `FE-INP-05` Perpindahan Pasien | Daftar Kerja → Detail Episode → **Pindahkan Pasien** | 3 | `inpatient-episode-detail-view.jsx:753` |
| 13 | `FE-INP-14` Pencatatan Kepergian | Daftar Kerja → Detail Episode → **Catat Kepergian** | 3 | `inpatient-episode-detail-view.jsx:898` |
| 14 | `FE-INP-15` Kebutuhan Isolasi | Daftar Kerja → Detail Episode → **Simpan Kebutuhan Isolasi**; juga langkah Dokter alur admisi | 3 | `inpatient-episode-detail-view.jsx:522`; `use-inpatient-admission-doctor.jsx:477` |
| 15 | `FE-INP-06` Keputusan Pulang dan Resume | Daftar Kerja → Detail Episode → **Keputusan Pulang** | 3 | `inpatient-episode-detail-view.jsx:393` |
| 16 | `FE-INP-08` Kelayakan Keuangan | Daftar Kerja → Detail Episode → **Kelayakan Keuangan** | 3 | `inpatient-episode-detail-view.jsx:400` |
| 17 | `FE-INP-07` Penutupan Episode | Daftar Kerja → Detail Episode → **Penutupan Episode** | 3 | `inpatient-episode-detail-view.jsx:407` dan `650` |
| 18 | `FE-INP-18` Cetak Persetujuan | Daftar Kerja → Detail Episode → **Cetak Persetujuan**; juga langkah 8 alur admisi | 3 | `inpatient-episode-detail-view.jsx:420` |
| 19 | `FE-INP-11` Sesi Koreksi Episode | Daftar Kerja tersaring **Sudah ditutup** → Detail Episode → **Sesi Koreksi** | 3 | `inpatient-episode-detail-view.jsx:435`, dirender hanya bagi supervisor pada episode `Closed` |

Tidak ada layar yang melampaui tiga klik, dan tidak ada layar yang tidak punya jalan masuk.

### 6.4 Cakupan endpoint terhadap bagian 11A

Setiap operasi pada [`contracts/api-contract.md`](../../../contracts/api-contract.md)
diperiksa satu per satu terhadap pemanggilnya di source frontend.

| Grup | Operasi | Layar pemilik | Pemanggil di source |
| --- | --- | --- | --- |
| Episode | `GET /filters/metadata` | `FE-INP-16` | `use-inpatient-episode-worklist.jsx:128` |
| Episode | `GET /summary` | `FE-INP-19` | `use-inpatient-dashboard.jsx:131` |
| Episode | `GET /` | `FE-INP-16` | `use-inpatient-episode-worklist.jsx` |
| Episode | `GET /{id}` | `FE-INP-04` | `use-inpatient-episode-detail.jsx` |
| Episode | `GET /{id}/status-history` | `FE-INP-04` | `use-inpatient-episode-detail.jsx` |
| Episode | `POST /` | `FE-INP-03` titik tulis 2 | `use-inpatient-admission-doctor.jsx:457` |
| Episode | `PUT /{id}` | `FE-INP-03` titik tulis 3 | `use-inpatient-admission-confirmation.jsx:212` |
| Episode | `PATCH /{id}/cancel` | `FE-INP-17` | `use-inpatient-episode-detail.jsx:254`; `use-inpatient-episode-worklist.jsx:337` |
| Episode | `POST /{id}/doctor-assignments` | `FE-INP-04` | `use-inpatient-episode-detail.jsx:421` |
| Episode | `GET /{id}/doctor-assignments` | `FE-INP-04` | `use-inpatient-episode-detail.jsx` |
| Episode | `POST /{id}/nurse-assignments` | `FE-INP-04` | `use-inpatient-episode-detail.jsx:474` |
| Episode | `GET /{id}/nurse-assignments` | `FE-INP-04` | `use-inpatient-episode-detail.jsx` |
| Episode | `PATCH /{id}/isolation-requirement` | `FE-INP-15` | `use-inpatient-episode-detail.jsx:316`; `use-inpatient-admission-doctor.jsx:477` |
| Episode | `POST /{id}/correction-sessions` | `FE-INP-11` | `use-inpatient-correction.jsx:207` |
| Episode | `PATCH /{id}/correction-sessions/{sessionId}/close` | `FE-INP-11` | `use-inpatient-correction.jsx:316` |
| Bed Occupancy | `GET /available-beds` | `FE-INP-03` | `use-inpatient-admission-bed.jsx` |
| Bed Occupancy | `GET /bed-board` | `FE-INP-02` | `use-inpatient-bed-board.jsx` |
| Bed Occupancy | `POST /reservations` | `FE-INP-03` | `use-inpatient-admission-bed.jsx:230` |
| Bed Occupancy | `PATCH /reservations/{id}/cancel` | `FE-INP-03`; `FE-INP-02` | `use-inpatient-admission-bed.jsx:286`. Sisi papan menjadi scope `FE-RWI-036` |
| Bed Occupancy | `POST /placements` | `FE-INP-02` | `use-inpatient-bed-board-actions.jsx:91` |
| Bed Occupancy | `POST /placements/transfer` | `FE-INP-05` | `use-inpatient-episode-detail.jsx:365` |
| Bed Occupancy | `GET /placements/by-episode/{episodeId}` | `FE-INP-04` | `use-inpatient-episode-detail.jsx` |
| Discharge | `POST /{episodeId}/decide` | `FE-INP-06` | `use-inpatient-discharge.jsx:198` |
| Discharge | `POST /{episodeId}/record-departure` | `FE-INP-14` | `use-inpatient-episode-detail.jsx:568` |
| Discharge | `GET /{episodeId}/summary` | `FE-INP-06`, `FE-INP-11` | `use-inpatient-discharge.jsx:104`; `use-inpatient-correction.jsx:113` |
| Discharge | `PUT /{episodeId}/summary` | `FE-INP-06`, `FE-INP-11` | `use-inpatient-discharge.jsx:254`; `use-inpatient-correction.jsx:260` |
| Discharge | `PATCH /{episodeId}/summary/sign` | `FE-INP-06` | `use-inpatient-discharge.jsx:309` |
| Discharge | `GET /{episodeId}/clearance` | `FE-INP-07` | `use-inpatient-closure.jsx:99` |
| Discharge | `POST /{episodeId}/clearance/{itemId}/mark` | `FE-INP-07` | `use-inpatient-closure.jsx:180` |
| Discharge | `POST /{episodeId}/financial-clearance` | `FE-INP-08` | `use-inpatient-financial-clearance.jsx:149` |
| Discharge | `GET /{episodeId}/closure-readiness` | `FE-INP-07`, `FE-INP-08` | `use-inpatient-closure.jsx:98`; `use-inpatient-financial-clearance.jsx:80` |
| Discharge | `POST /{episodeId}/close` | `FE-INP-07` | `use-inpatient-closure.jsx:265` |
| Discharge | `POST /{episodeId}/close-with-override` | `FE-INP-07` | `use-inpatient-closure.jsx:377` |
| Census | `GET /filters/metadata` | `FE-INP-01` | `use-inpatient-census.jsx:84` — **baru dipakai task ini** |
| Census | `GET /summary` | `FE-INP-19` | `use-inpatient-dashboard.jsx:98` |
| Census | `GET /` | `FE-INP-01` | `use-inpatient-census.jsx` |
| Monitoring | `GET /pending-closures` | `FE-INP-09`, `FE-INP-19` | `use-inpatient-monitoring.jsx`; `use-inpatient-dashboard.jsx` |
| Monitoring | `GET /closures-without-financial-clearance` | `FE-INP-09`, `FE-INP-19` | idem |
| Monitoring | `GET /unassigned-nurse-episodes` | `FE-INP-09`, `FE-INP-19` | idem |
| Monitoring | `GET /isolation-mismatch` | `FE-INP-09`, `FE-INP-19` | idem |
| Monitoring | `GET /bed-drift` | `FE-INP-10` | `use-inpatient-bed-drift.jsx:21` |
| Master Data / Inpatient Setting | `GET /` | `FE-INP-12` | `use-inpatient-setting.jsx` |
| Master Data / Inpatient Setting | `PUT /{id}` | `FE-INP-12` | `use-inpatient-setting.jsx` |
| Master Data / Clearance Item | `GET /` | `FE-INP-13` | `use-inpatient-clearance-items.jsx:92` |
| Master Data / Clearance Item | `GET /{id}` | `FE-INP-13` | `use-inpatient-clearance-items.jsx:175` |
| Master Data / Clearance Item | `POST /` | `FE-INP-13` | `use-inpatient-clearance-items.jsx:258` |
| Master Data / Clearance Item | `PUT /{id}` | `FE-INP-13` | `use-inpatient-clearance-items.jsx:251` |
| Master Data / Clearance Item | `PATCH /{id}/status` | `FE-INP-13` | `use-inpatient-clearance-items.jsx:296` |
| Master Data / Clearance Item | `DELETE /{id}` | `FE-INP-13` | `use-inpatient-clearance-items.jsx:334` |

**Nol operasi tanpa pemilik.** Sebelum task ini `GET /census/filters/metadata` adalah
satu-satunya yang menganggur; sekarang ia dimiliki `FE-INP-01`. Endpoint pada bagian 8
api-contract ("Yang sengaja tidak dibuat") memang tidak ada, jadi tidak ikut dihitung.
`PATCH /beds/{id}/availability` milik grup Bed adalah **rencana perubahan perilaku** backend
`BE-RWI-006`, bukan operasi baru yang menuntut layar.

### 6.5 Uji manual

`MANUAL TEST: NOT FEASIBLE.` Alasannya konkret dan sama dengan yang tercatat pada
`FE-RWI-030` s.d. `032`: data master Rawat Inap pada environment target belum layak
(`RWI-UI-GAP-007`) — pengaturan `DEFAULT` tidak ditemukan, butir administrasi kosong, papan
nol tempat tidur, dan tidak ada episode untuk membuktikan aksi berbasis baris. Penyaring
census tidak dapat dibuktikan berpengaruh terhadap hasil selama census-nya sendiri kosong.
Pembuktian runtime ujung-ke-ujung tetap milik `FE-RWI-035`.

Yang **dapat** diverifikasi tanpa runtime, dan sudah diverifikasi:

- susunan tujuh butir menu operasional beserta urutannya, dibaca langsung dari
  `menu-items.jsx` grup `healthServicesInpatientManagement`;
- kedua butir master data tampil tepat satu kali dan tidak ada duplikatnya —
  pencarian teks label menghasilkan tepat satu baris untuk masing-masing;
- `pathname` kedua butir tidak berubah, sehingga direct URL lama tetap bekerja;
- bentuk `CensusFilterMetadataResponse` yang dibaca normalizer cocok baris demi baris dengan
  `InpCensusQueryService.GetCensusFilterMetadataAsync`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Setiap layar bagian 2 dapat dicapai dari beranda dalam paling banyak tiga klik | ✅ Terpenuhi | Tabel jalur 19 layar pada bagian 6.3. Klik maksimum 3, dicapai delapan layar anak Detail Episode |
| 2. Layar sesi koreksi dapat dicapai dari daftar kerja tersaring `Closed` | ✅ Terpenuhi | Baris 19 tabel 6.3. Penyaring status memuat "Sudah ditutup" (`inpatient-episode-worklist-constants.jsx` butir cadangan status); tautan **Sesi Koreksi** dirender bagi supervisor pada episode `Closed` (`inpatient-episode-detail-view.jsx:435`) |
| 3. Submenu `Rawat Inap` berisi tepat tujuh butir dan berurutan | ✅ Terpenuhi | `menu-items.jsx` grup `healthServicesInpatientManagement`: Beranda Rawat Inap, Admisi Rawat Inap, Papan Tempat Tidur, Daftar Kerja Episode, Pasien Sedang Dirawat, Daftar Pantau, Selisih Tempat Tidur — persis urutan roadmap |
| 4. **Butir Administrasi Rawat Inap** dan **Pengaturan Rawat Inap** tampil tepat satu kali di Master Data, tidak lagi di submenu Rawat Inap, tetap memakai route dan permission existing | ✅ Terpenuhi | `menu-items.jsx` akhir `subItems` grup `healthServicesMasterData`. Definisi dipindahkan, bukan disalin: pencarian label menghasilkan tepat satu kemunculan masing-masing. `pathname` tetap `/health-services/inpatient-management/clearance-items` dan `/settings`; tidak ada berkas permission yang disentuh |
| 5. Layar per-episode tidak mendapat butir menu | ✅ Terpenuhi | Ketujuh butir submenu seluruhnya layar tingkat modul. Kesembilan layar per-episode (`FE-INP-04` s.d. `08`, `11`, `14`, `15`, `17`, `18`) dicapai dari Daftar Kerja atau Detail Episode — tabel 6.3 baris 10 s.d. 19 |
| 6. Tidak ada operasi api contract yang tidak dimiliki satu layar | ✅ Terpenuhi | Tabel cakupan endpoint 6.4 — 49 operasi, nol tanpa pemilik. Yang sebelumnya menganggur, `GET /census/filters/metadata`, ditutup task ini |
| 7. Penyaring census memakai `filters/metadata`, bukan daftar yang ditanam di kode | ✅ Terpenuhi | `use-inpatient-census.jsx:84`; `useSelectResource` nol kemunculan pada hook itu; jumlah baris per halaman kini juga datang dari metadata |

### Definition of Done

| Butir | Status |
| --- | --- |
| Ketujuh kriteria lulus | ✅ |
| Laporan memuat tabel jalur untuk seluruh 19 layar | ✅ bagian 6.3 |
| Bukti hierarki tujuh operasional + dua master/configuration tanpa duplikasi | ✅ bagian 7 kriteria 3 dan 4 |
| `npm run lint` dan `npm run build` lulus | ✅ bagian 6.1 |

Butir verification roadmap "e2e menuju sesi koreksi lewat daftar kerja" **tidak dijalankan**
atas arahan pengguna 1 September 2026 yang membatasi validasi pada `build` dan `lint`.
Keterjangkauannya tetap dibuktikan lewat penelusuran source pada tabel 6.3 baris 19.
Pembuktian runtime-nya milik `FE-RWI-035`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` 571 warning, seluruhnya existing dan nol pada berkas task ini. Klasifikasi: `EXISTING WARNING` |
| Masalah yang diketahui | Layar `FE-INP-01`, `12`, dan `13` tetap berstatus `REPAIR` menurut skema bagian 24. Task ini hanya memindahkan induk menu dan memperbaiki sumber pilihan penyaring; layout, empty-state, dan surface aksi ketiganya milik `FE-RWI-037`, `040`, dan `041`, yang memang berdependensi pada task ini |
| Dependency backend | `NONE` yang menahan. `GET /census/filters/metadata` sudah tersedia dan terbukti berjalan 26 Agustus 2026. `RWI-UI-GAP-007` masih menahan **pembuktian runtime**, bukan implementasinya |
| Perubahan sampingan | `NONE`. Empat berkas source dan dua berkas test yang berubah seluruhnya berada di dalam scope task; tidak ada perbaikan lint, format, atau refactor oportunistik |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir pekerjaan menampilkan berkas milik task ini beserta berkas `FE-RWI-034` yang dikerjakan pada sesi yang sama. Tidak ada `git add`, commit, push, pull, merge, rebase, maupun deploy |
| Langkah berikutnya | `FE-RWI-037` (repair Census) dan `FE-RWI-041` (repair Pengaturan) kini terbuka: keduanya berdependensi pada task ini dan pada metadata penyaring yang sudah tersedia |
