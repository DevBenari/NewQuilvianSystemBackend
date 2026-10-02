# Laporan Perubahan Frontend — `FE-LAB-10`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-10` |
| Judul | Pengelolaan jenis specimen dan daftar pantau `Lainnya` |
| Slice | `S13a` / `S13b` — gelombang `MVP-5a`, `EPIC-LAB-11` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6b |
| Trace | `FR-11.1`, `FR-11.2`; `LAB-DEC-040`, `LAB-DEC-064`, `LAB-DEC-071`, `LAB-DEC-074`; `BR-35`; `VAL-61`, `VAL-62`, `VAL-63`; `LAB-FE-014` |
| Contract version | `LAB-API-v1` amandemen `r7` dan `r8` — `approved`, terkunci 2026-09-14. Nol amandemen diminta task ini |
| Wewenang UI | Tata letak `DEV_DISCRETION` mengikuti `LAB-FE-002`; bentuk fitur mengikuti `rules/frontend/master-data-feature-standard.md` dan modul rujukan `lab-rejection-reasons` |
| Dependency | `BE-LAB-20` ✅ selesai 2026-09-15; `BE-LAB-25` ✅ selesai 2026-09-15 |
| Klasifikasi | `HEAVY` — fitur master data lengkap (7 berkas source + 2 registrasi) ditambah satu layar baca kedua, tiga hook, dan 9 thunk |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source); laporan ini pada `NewQuilvianSystemBackend/docs/module-blueprints/laboratorium/` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `686038858` |
| Commit backend yang dijadikan rujukan | `13665452` |
| Tanggal | 2026-09-16 |
| Status | **Selesai.** Seluruh butir DoD terpenuhi dan terbukti pada aplikasi yang benar-benar berjalan. Satu batas dilaporkan apa adanya pada bagian 8 |

---

## 1. Keadaan yang ditemukan di awal

Backend untuk jenis specimen **sudah lengkap** dan frontend-nya **nol**.

| Sisi | Keadaan yang dibuktikan |
| --- | --- |
| Backend | Sembilan endpoint grup `Lab Specimen Type` berstatus `Tersedia` pada kontrak, dan ketika dijalankan hari ini seluruhnya menjawab. `BE-LAB-20` mendirikan tabel beserta tujuh barisnya; `BE-LAB-25` menambahkan `GET /other-usage` |
| Frontend | Folder `src/app/health-services/master-data/lab-specimen-types/` **tidak ada**. Penelusuran `lab-specimen-type` pada `src` menghasilkan nol berkas |

Akibat nyatanya: **tujuh jenis specimen yang sudah tersimpan di basis data tidak dapat dilihat
maupun dikelola siapa pun lewat aplikasi**, dan keterangan `Lainnya` yang diketik petugas di meja
penerimaan tidak punya layar yang membacanya — padahal justru itu bahan yang dipakai kepala
instalasi untuk menaikkan jenis yang belum terdaftar menjadi jenis tetap.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya kepala instalasi laboratorium**, atau admin yang memegang kewenangan setara.

### 2.1 Mengelola daftar jenis specimen

1. Kepala instalasi membuka menu **Data Master → Jenis Specimen**.
2. Layar menampilkan empat kartu ringkasan — total jenis, aktif, nonaktif, dan berapa **jalan
   keluar `Lainnya`** yang aktif — lalu daftar jenis yang urut menurut urutan tampilnya.
3. Ia dapat mencari berdasarkan kode, nama, atau keterangan; menyaring aktif/nonaktif; dan
   mengubah jumlah baris per halaman.
4. Menekan **+ Tambah Jenis Specimen** membuka formulir berisi kode jenis, nama, keterangan, dan
   urutan tampil. Kode disimpan dalam huruf kapital.
5. Menekan **Perbarui** pada sebuah baris membuka formulir yang sama tanpa ruas kode — kode hanya
   ditetapkan sekali, karena wadah yang sudah tersimpan menunjuk kode itu.
6. Menekan **Nonaktifkan** menampilkan konfirmasi. Jenis yang dinonaktifkan tidak lagi muncul saat
   petugas mencatat wadah, tetapi tetap menempel pada wadah yang sudah tersimpan. **Tidak ada
   tombol Hapus**, dan itu disengaja.

### 2.2 Jalur tidak normal yang sengaja ditutup

**Baris `Lainnya` yang menjadi satu-satunya jalan keluar tidak dapat dinonaktifkan.** Tombol
Nonaktifkan pada baris itu tampil **mati**, dan ketika penunjuk diarahkan ke atasnya muncul
sebabnya: *"Jenis Lainnya harus tetap aktif, karena menjadi jalan keluar ketika jenis specimen
belum terdaftar."*

Alasannya konkret. Baris `Lainnya` adalah yang dipakai petugas ketika sampel yang datang berjenis
belum terdaftar. Bila ia dapat dimatikan, jalan buntu di meja penerimaan kembali — dan kembalinya
diam-diam, lewat satu klik pada layar pengelolaan yang tidak terlihat hubungannya dengan
penerimaan sampel. Kalimat penolakannya diambil **kata demi kata** dari yang dikembalikan backend,
sehingga petugas membaca kalimat yang sama di mana pun ia bertemu aturan ini.

Layar juga memunculkan peringatan tersendiri bila jumlah jalan keluar aktif **nol**: *"Tidak ada
jenis Lainnya yang aktif. Selama itu, wadah berjenis belum terdaftar tidak punya jalan keluar di
meja penerimaan."*

### 2.3 Membaca daftar pantau `Lainnya`

1. Dari menu **Data Master → Pantau Jenis Lainnya**, atau dari tombol **Daftar Pantau Lainnya** di
   layar pengelolaan.
2. Layar menampilkan setiap keterangan yang pernah diketik petugas beserta berapa kali ia dipakai,
   kapan pertama, dan kapan terakhir. Yang paling sering berada di atas.
3. **Keterangan ditampilkan apa adanya dan tidak disatukan.** "cairan kista", "Cairan Kista", dan
   "c. kista" muncul sebagai **tiga baris terpisah** — justru itulah yang perlu dilihat kepala
   instalasi, karena dari situ ia menyimpulkan ketiganya satu hal.
4. Menekan **Jadikan Jenis Tetap** membuka formulir tambah dengan **nama jenis sudah terisi** dari
   keterangan itu. Kodenya tetap diketik sendiri — menebaknya berarti mengarang penanda yang
   menempel selamanya pada wadah yang tersimpan.
5. Tanpa rentang tanggal, backend memakai **90 hari terakhir**. Rentang panjang disengaja: yang
   dicari adalah keterangan yang **berulang**, dan pengulangan tidak terlihat pada jendela satu
   minggu. Kalimat itu ditulis di layar supaya rentang bawaannya tidak disangka kesalahan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/frontend-roadmap.md` bagian 6b | Cakupan, DoD, dependency, dan acceptance criteria `FE-LAB-10` |
| `contracts/api-contract.md` bagian 6 | Kesembilan endpoint grup `Lab Specimen Type`, hak akses, dan bentuk permintaan/jawabannya |
| `Areas/.../DTOs/LabSpecimenTypeDtos.cs` | Nama ruas sebenarnya, batas panjang, dan ruas mana yang **tidak** ada pada permintaan tambah |
| `Areas/.../Services/LabSpecimenTypeService.cs` | Kalimat penolakan `VAL-61`, `VAL-62`, `VAL-63` apa adanya, dan cara `GET /other-usage` mengelompokkan |
| `Areas/.../Services/LabFilterMetadataFactory.cs` | Isi `GET /filters/metadata`; terbukti **tidak** membawa `DefaultFilter`, sehingga tidak ada hidrasi penyaring dari metadata |
| `rules/frontend/master-data-feature-standard.md` | Bentuk baku fitur master data: tujuh berkas source ditambah dua registrasi |
| `rules/frontend/ui-consistency-checklist.md`, `test-policy.md` | Kewajiban sebelum pekerjaan dinyatakan selesai |
| Seluruh berkas fitur `lab-rejection-reasons` | Modul rujukan visual dan struktural — fitur master data Laboratorium terdekat |
| `tests/e2e/lab-monitoring-date-guard.spec.mjs` | Pola pembuktian layar yang sudah dipakai `FE-LAB-18` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/master-data/lab-specimen-types/lab-specimen-type-constants.jsx` | **Baru.** `LAB_SPECIMEN_TYPE_CONFIG` sebagai sumber tunggal: endpoint, route, salinan teks, kolom, ringkasan, penyaring, dan pesan gagal per thunk |
| `src/lib/state/slice/health-services/master-data/master-data-lab-specimen-type-slice.jsx` | **Baru.** Sembilan thunk memetakan tepat ke sembilan endpoint, seluruhnya lewat `InstanceAxios` dan meneruskan `signal` |
| `src/utils/health-services/master-data/lab-specimen-types/lab-specimen-type-utils.jsx` | **Baru.** Fungsi murni: pembacaan ruas, formulir, validasi, pembentuk payload, penjaga `VAL-63`, dan parameter daftar pantau |
| `src/lib/hooks/health-services/master-data/lab-specimen-types/use-master-data-lab-specimen-type.jsx` | **Baru.** Controller halaman daftar beserta aksi aktif/nonaktif |
| `…/use-master-data-lab-specimen-type-editor.jsx` | **Baru.** Controller formulir tambah dan ubah; memuat barisnya lewat `GET /{id}` |
| `…/use-lab-specimen-other-usage.jsx` | **Baru.** Controller layar daftar pantau `Lainnya` |
| `src/components/view/health-services/master-data/lab-specimen-types/master-data-lab-specimen-type-view.jsx` | **Baru.** Halaman daftar |
| `…/add/lab-specimen-type-form-view.jsx` | **Baru.** Formulir dua mode |
| `…/other-usage/lab-specimen-other-usage-view.jsx` | **Baru.** Layar daftar pantau |
| `src/app/health-services/master-data/lab-specimen-types/page.jsx` + `lab-specimen-types-client.jsx` | **Baru.** Route daftar |
| `…/create/page.jsx` | **Baru.** Route tambah, berbatas `Suspense` |
| `…/other-usage/page.jsx` + `lab-specimen-other-usage-client.jsx` | **Baru.** Route daftar pantau |
| `…/[slug]/route-token.js` | **Baru.** Penjaga token route privat; `other-usage` ikut dicadangkan |
| `…/[slug]/update/page.jsx` | **Baru.** Route ubah, berbatas `Suspense` |
| `src/style/health-services/master-data/lab-specimen-types/lab-specimen-type.module.css` | **Baru.** Hanya token, nol nilai literal |
| `src/lib/state/store.jsx` | **+2 baris.** Reducer didaftarkan memakai `stateKey` dari config |
| `src/utils/menu-sidebar/menu-items.jsx` | **+2 entri menu.** Jenis Specimen dan Pantau Jenis Lainnya |
| `tests/unit/lab-specimen-type-utils.test.mjs` | **Baru, 15 uji unit** |
| `tests/e2e/lab-specimen-type-screen.spec.mjs` | **Baru, 7 pemeriksaan layar** |

### 3.3 Kepatuhan arsitektur frontend

- **Letaknya `master-data/`, bukan `laboratory-management/`.** `LAB-FE-014` dan `AC-49` mengikat:
  seluruh menu data induk berada di `health-services/master-data/`, dan folder
  `laboratory-management` hanya berisi layar operasional. Diverifikasi: nol berkas fitur ini yang
  jatuh di luar `master-data/`.
- **Nol arsitektur baru.** Nol instance Axios baru, nol `fetch`, nol factory atau hook master-data
  generik. Struktur disalin dari `lab-rejection-reasons` lalu diisi ulang, bukan diabstraksi.
- **Satu pemakaian ulang lintas folder, disengaja.** Layar daftar pantau mengimpor
  `validateDateRange` dan `todayDateValue` dari
  `src/lib/hooks/health-services/laboratory-management/lab-monitoring-rules.js`. `LAB-DEC-074`
  menyatakan aturan tanggal Laboratorium berlaku **module-wide**; menyalinnya berarti dua definisi
  "hari ini" pada satu modul, dan itu persis cara sebuah aturan bergeser tanpa ada yang
  menyadarinya. Yang diimpor adalah fungsi murni — nol React, nol Redux, nol permintaan.

#### Tiga selisih terhadap `master-data-feature-standard.md`, dilaporkan bukan didiamkan

Dokumen standar itu sendiri menetapkan bahwa bila source frontend berbeda, **source yang berlaku
dan selisihnya dilaporkan**. Ketiganya berikut:

| Selisih | Keadaan sebenarnya |
| --- | --- |
| Standar menuntut thunk `PATCH /{id}/status` dan `DELETE /{id}` | Grup ini **tidak punya keduanya**. Statusnya lewat `PUT /{id}/activation`, dan penghapusan memang tidak ada — backend mengumumkannya lewat `IsDeletable: false`. Sembilan thunk tetap terpenuhi, tetapi dua di antaranya berbeda dari daftar baku: `GET /other-usage` dan `PUT /{id}/activation` |
| Standar menuntut halaman detail ber-`BaseDetailView` | Fitur ini **tidak punya halaman detail**, sama seperti `lab-rejection-reasons`. Aksi aktif/nonaktif karena itu berada di kolom aksi halaman daftar. Cakupan `FE-LAB-10` menyebut tepat dua layar, dan halaman detail bukan salah satunya |
| Standar menyatakan fitur master data tidak menambah CSS Module | Fitur ini menambah satu, mengikuti preseden `lab-rejection-reasons` dan `lab-value-bounds`. Alasannya: aksi baris dan panel penanda terkunci menuntut tata letak yang alternatifnya adalah inline style — yang dilarang checklist konsistensi UI. **Nol nilai warna, radius, atau jarak ditulis sebagai literal** |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `DataTable` menampilkan *"Mengambil data jenis specimen..."*; daftar pantau menampilkan *"Mengambil daftar pantau pemakaian jenis Lainnya..."*. Kartu ringkasan punya keadaan memuatnya sendiri |
| Kosong | *"Data jenis specimen tidak ditemukan."* beserta *"Coba gunakan filter lain atau tambahkan data baru."* Pada daftar pantau: *"Belum ada pemakaian jenis Lainnya pada rentang ini."* beserta ajakan memperlebar rentang, karena rentang yang terlalu sempit memang penyebab paling sering |
| Gagal | Kalimat dari backend ditampilkan apa adanya pada alert; aksi yang gagal memunculkan toast merah berisi pesannya. Rentang tanggal yang tidak sah memunculkan **sebabnya** — *"Tgl Awal tidak boleh melewati Tgl Akhir."* — bukan sekadar daftar yang diam |
| Tanpa hak akses | Halaman dibungkus `AccessDeniedGate`. Tanpa `LabSpecimenType : Update`, tombol Perbarui dan Nonaktifkan **tidak dirender**; tanpa `: Create`, tombol Tambah dan Jadikan Jenis Tetap tidak dirender. Datanya tetap terbaca — yang hilang hanya aksinya |

> **Satu catatan tentang kewenangan yang pantas dibaca ulang kelak.** Selama daftar kewenangan
> belum diketahui — sedang diambil, atau pengambilannya gagal — aksi tetap ditampilkan. Itu jaring
> pengaman yang sudah berlaku pada `usePermission`, dan alasannya tertulis di sana: menyembunyikan
> aksi karena satu permintaan gagal menutup jalan bagi petugas yang sebenarnya berhak, sementara
> membiarkannya tampil paling buruk berakhir pada penolakan `403` yang terlihat dan dapat
> dilaporkan.

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Specimen Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-specimen-types/filters/metadata` | Pilihan jumlah baris, dan pemeriksaan bahwa penanda `Lainnya` memang masih terkunci backend | `LabSpecimenType : Read` |
| `GET` | `…/lab-specimen-types/summary` | Empat kartu ringkasan, dan **jumlah jalan keluar `Lainnya` yang aktif** yang menentukan berlakunya `VAL-63` di layar | `LabSpecimenType : Read` |
| `GET` | `…/lab-specimen-types` | Tabel pengelolaan beserta penyaring dan paginasinya | `LabSpecimenType : Read` |
| `GET` | `…/lab-specimen-types/options` | **Belum dipakai layar mana pun.** Thunk-nya disediakan karena standar master data menuntut satu thunk per endpoint; konsumennya adalah formulir penerimaan `FE-LAB-11` | `LabSpecimenType : Read` |
| `GET` | `…/lab-specimen-types/{id}` | Memuat satu baris pada formulir ubah, termasuk saat dibuka lewat tautan langsung | `LabSpecimenType : Read` |
| `GET` | `…/lab-specimen-types/other-usage` | Daftar pantau pemakaian `Lainnya` beserta penyaring rentang tanggal dan pencarian | `LabSpecimenType : Read` |
| `POST` | `…/lab-specimen-types` | Menambah jenis specimen | `LabSpecimenType : Create` |
| `PUT` | `…/lab-specimen-types/{id}` | Mengubah nama, keterangan, dan urutan tampil | `LabSpecimenType : Update` |
| `PUT` | `…/lab-specimen-types/{id}/activation` | Mengaktifkan atau menonaktifkan | `LabSpecimenType : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --quiet` pada seluruh berkas yang disentuh | Nol keluaran | `PASS` | Perintah selesai tanpa pesan |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **1014/1014 lulus** | `PASS` | 999 uji yang sudah ada tetap lulus, ditambah 15 uji baru |
| `npm run build` | Lulus, termasuk `postbuild` standalone | `PASS` | Keempat route fitur ini terdaftar pada keluarannya |
| `npx playwright test tests/e2e/lab-specimen-type-screen.spec.mjs` | **7 passed (11.1s)** | `PASS` | Dijalankan terhadap `.next/standalone` pada `127.0.0.1:3710` |
| Pemeriksaan API terhadap backend yang benar-benar berjalan | 6 pemeriksaan, seluruhnya sesuai | `PASS` | Rinciannya pada 6.2 |

Uji manual: **`PASS`** — dengan satu bagian `NOT FEASIBLE` yang disebut pada 6.3.

**Tidak dijalankan:** `npm run test:uat` — tidak diminta task ini.

> **Satu catatan perintah.** `npm run test:unit` gagal di Node 20 pada mesin ini karena bentuk
> glob-nya (`Could not find …tests\unit\**\*.test.mjs`). Bentuk direktori
> `node --import ./tests/helpers/register.mjs --test tests/unit/` menjalankan suite yang sama dan
> berhasil. Ini keadaan lingkungan yang sudah diketahui, bukan cacat yang lahir dari task ini.

### 6.1 Bukti layar — aplikasi yang benar-benar berjalan

Jawaban API dipasang lewat `page.route`, sehingga pemeriksaan ini **nol bergantung** pada backend
maupun basis data — pola yang sama dengan `FE-LAB-18`.

| Pemeriksaan | Yang dibuktikan |
| --- | --- |
| `AC-58` | Ketujuh jenis tampil, dan **urutannya sesuai `sortOrder` backend**. Dibuktikan dari posisi kode pada isi tabel, bukan dari jumlah barisnya — bila pengurutan di browser menyala kembali, `OTHER` ber-`sortOrder` 99 tidak akan lagi berada di posisi terakhir dan pemeriksaan ini gagal |
| `VAL-63` | Tombol Nonaktifkan pada baris `Lainnya` terakhir **mati**, dan `title`-nya berisi kalimat sebabnya. **Ditambah pembuktian terbalik**: baris biasa tetap hidup — tanpa itu, penjaga yang salah pasang dan mematikan seluruh baris akan tetap lolos |
| Kewenangan | Pengguna yang hanya memegang `Read` melihat ketujuh barisnya tetapi **nol tombol** Perbarui, Nonaktifkan, dan Tambah. Tombol Daftar Pantau tetap terbuka, karena ia layar baca |
| **DoD tautan langsung** | Alamat `/{id}/update` dibuka **tanpa pernah melewati halaman daftar**: jalur `GET /{id}` terpakai, formulir terisi "Blood", ruas kode tidak dirender, UUID hilang dari bilah alamat — dan **nol permintaan daftar dikirim** |
| `AC-60` | Ketiga ejaan tampil sebagai tiga baris terpisah, beserta jumlah pemakaiannya |
| `AC-99` module-wide | Rentang terbalik pada daftar pantau memunculkan kalimat sebabnya, **dan nol permintaan tambahan berangkat** selama rentangnya masih terbalik |
| Rentang sah | Permintaan pertama dikirim **tanpa** `startDate`/`endDate`, sehingga rentang bawaan 90 hari milik backend yang berlaku; sesudah tanggal dipilih, nilainya benar-benar terkirim |

**Dua pemeriksaan di atas mengukur sesuatu yang *tidak* terjadi**, dan itu disengaja. Butir DoD
"tautan langsung berfungsi tanpa membuka daftarnya lebih dulu" hanya dapat dibuktikan dengan
menunjukkan jalur daftar **nol** terpakai; satu-satunya cara membuktikan sesuatu tidak dikirim
adalah menghitung yang tiba.

> **Dua kekeliruan saya saat menulis spec ini, dicatat supaya tidak terulang.** Pertama,
> `page.getByDisplayValue` bukan API Playwright — itu milik Testing Library; yang berlaku
> `locator('input[name=…]')` beserta `toHaveValue`. Kedua, jawaban `/v1/auth/permissions`
> berbentuk `{ isSuperAdmin, permissions, totalPermission }`, **bukan larik telanjang**; larik
> telanjang membuat `keys` kosong sementara `loaded` menjadi benar, dan layar membacanya sebagai
> "tidak berwenang" lalu menyembunyikan seluruh tombol. Keduanya kekeliruan uji — bukan cacat
> produk. Yang kedua justru membuktikan penjagaan kewenangannya bekerja.

### 6.2 Bukti data — backend dan basis data yang sebenarnya

Backend dijalankan pada `https://localhost:7184` terhadap basis data `QuilvianNewDevYoga`.

| Pemeriksaan | Hasil sebenarnya |
| --- | --- |
| `GET /lab-specimen-types` | `totalData 7`, urut `sortOrder` 1..6 lalu 99: `BLOOD`, `URINE`, `BODYFLUID`, `SPUTUM`, `PUS`, `TISSUE`, `OTHER` |
| `GET /summary` | `totalJenis 7`, `aktif 7`, `nonaktif 0`, **`jalanKeluarLainnyaAktif 1`** |
| `GET /filters/metadata` | `isDeletable false`, `isOtherBucketEditable false`, `pageSizeOptions [10,25,50,100]` — keempatnya kelipatan 5 |
| `GET /{id}` pada baris `OTHER` | Detail terbaca lengkap, `isOtherBucket true`, `isActive true` |
| **`PUT /{id}/activation` menonaktifkan `Lainnya`** | Ditolak **`HTTP 422`**: *"Jenis Lainnya harus tetap aktif, karena menjadi jalan keluar ketika jenis specimen belum terdaftar."* — **sama kata demi kata** dengan kalimat yang dipakai layar |
| Keadaan sesudah penolakan | Diperiksa ulang dari koneksi baru: ringkasan **identik** dan baris `OTHER` tetap `isActive true`. **Nol baris berubah** |

Pemeriksaan penolakan itu sengaja dipilih karena jalurnya memang **tidak mengubah apa pun ketika
aturannya bekerja**, dan keadaan sebelum maupun sesudahnya tetap diperiksa terpisah untuk
membuktikannya.

### 6.3 Yang tidak dapat diverifikasi, disebut apa adanya

Roadmap menyebut satu skenario verifikasi manual: *"daftar pantau menampilkan 'cairan kista'
berjumlah tiga"*. Terhadap basis data yang sebenarnya, **`GET /other-usage` mengembalikan
`totalData 0`** — belum ada satu pun wadah berjenis `Lainnya` yang tercatat, karena jalur yang
mengisinya adalah formulir penerimaan `FE-LAB-11` yang belum dikerjakan.

`MANUAL TEST: NOT FEASIBLE` untuk bagian itu — **alasannya ketiadaan data, bukan ketiadaan layar**.
Membuat data uji pada basis data bersama adalah perubahan data di luar wewenang task frontend, dan
tidak dilakukan. Sebagai gantinya, perilaku layarnya dibuktikan pada 6.1 dengan jawaban yang
dipasang: tiga ejaan "cairan kista" beserta jumlah 3, 2, dan 1 tampil sebagai tiga baris terpisah.

### 6.4 Grep anti-regresi

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal pada style baru | Kosong |
| Tombol non-base pada view baru | Kosong |
| `<table>` mentah | Kosong |
| Bootstrap utility typography | Kosong |
| `!important` baru | Kosong |
| Typography pada style baru | **6 temuan, seluruhnya dipertahankan.** Keenamnya memakai token (`var(--font-size-*)`, `var(--font-weight-*)`, `var(--line-height-*)`) dan menyasar class milik fitur ini sendiri — `panelNote`, `lockedFieldLabel`, `lockedFieldHint` — **nol menyasar komponen shared**. Sama persis dengan `lab-rejection-reason.module.css` |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-49` — seluruh menu data induk berada di `health-services/master-data/`, folder `laboratory-management` hanya layar operasional | **Terpenuhi** | Keempat route fitur ini berada di `master-data/lab-specimen-types/`; nol berkas jatuh di `laboratory-management/` |
| `AC-58` — jenis specimen dipilih dari daftar terkendali | **Terpenuhi untuk bagian pengelolaannya** | Ketujuh jenis tampil urut dan dapat dikelola. Bagian "mengirim jenis sebagai teks bebas ditolak" adalah penegakan backend pada jalur pencatatan wadah, dan layar pemakainya adalah `FE-LAB-11` |
| `AC-60` — setiap pemakaian `Lainnya` terlihat pada daftar pantau beserta keterangan dan jumlah pemakaiannya | **Terpenuhi pada layar** | Layar berdiri, memanggil `GET /other-usage`, menampilkan keterangan, jumlah, pertama dan terakhir dipakai. Isinya nol hari ini karena datanya belum ada — lihat 6.3 |

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kedua layar dapat dicapai dari menu | **Terpenuhi** | Dua entri pada `menu-items.jsx`: **Jenis Specimen** dan **Pantau Jenis Lainnya**. Layar pengelolaan juga membawa tombol menuju daftar pantau, dan sebaliknya |
| Berada di `master-data/`, bukan folder laboratorium (`AC-49`) | **Terpenuhi** | Lihat baris `AC-49` di atas |
| Tombol kelola dijaga `LabSpecimenType : Create` dan `: Update` | **Terpenuhi** | Pemeriksaan layar dengan pengguna ber-`Read` saja: nol tombol Perbarui, Nonaktifkan, dan Tambah |
| Keadaan kosong, gagal, dan muat ulang tertangani | **Terpenuhi** | Bagian 4; muat ulang dibuktikan oleh pemeriksaan tautan langsung |
| **Tautan langsung ke layar kelola berfungsi tanpa membuka daftarnya lebih dulu** | **Terpenuhi** | Jalur `GET /{id}` terpakai, formulir terisi, dan **nol permintaan daftar dikirim**. Inilah butir yang `LAB-API-v1` `r6` lahir untuk mencegah terulangnya |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari lint maupun build. Satu peringatan proses: `npm run test:unit` gagal di Node 20 pada mesin ini karena bentuk glob-nya; bentuk direktori berhasil dan menjalankan suite yang sama |
| Masalah yang diketahui | Thunk `getLabSpecimenTypeOptions` **belum punya pembaca**. Ia disediakan karena standar master data menuntut satu thunk per endpoint, dan konsumennya adalah `FE-LAB-11`. Disebut di sini supaya tidak terbaca sebagai kelalaian — dan supaya tidak terulang pola `BE-LAB-26`, yaitu sesuatu yang berdiri tanpa pembaca lalu tidak menghasilkan galat apa pun sampai seseorang membutuhkannya |
| Dependency backend | `NONE` yang tersisa. `BE-LAB-20` dan `BE-LAB-25` keduanya selesai, dan kesembilan endpointnya terbukti menjawab hari ini |
| Perubahan sampingan | `NONE`. Folder `test-results/` yang dihasilkan Playwright dihapus sesudah pemeriksaan selesai — ia tidak diabaikan `.gitignore`, dan meninggalkannya berarti menambah berkas yang bukan bagian pekerjaan ini. **Perubahan `FE-LAB-18` yang belum di-commit pada working tree tidak disentuh** |
| Interupsi | `NONE` |
| Status Git | 8 berkas `M` (7 di antaranya milik `FE-LAB-18` yang belum di-commit, ditambah `store.jsx` dan `menu-items.jsx` milik task ini), 9 entri `??` baru. Nol `git add`, `commit`, atau `push` dijalankan |
| Langkah berikutnya | `FE-LAB-11` — formulir Penerimaan Sampling/Specimen. Ia yang akan memakai `GET /options`, dan ia pula yang akhirnya mengisi daftar pantau `Lainnya` sehingga skenario 6.3 dapat dijalankan sungguhan. Perlu diketahui: `FE-LAB-11` masih tertahan `LAB-COORD-006` dan `LAB-COORD-007` pada dua wilayahnya |
