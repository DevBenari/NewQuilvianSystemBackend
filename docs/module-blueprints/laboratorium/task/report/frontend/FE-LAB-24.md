# Laporan Perubahan Frontend — `FE-LAB-24`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-24` |
| Judul | Dua layar data induk: Organisme dan Antibiotik |
| Slice | `MVP-6` — gelombang Mikrobiologi, `EPIC-LAB-13` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6e.1 |
| Trace | `FR-13.6`; `LAB-DEC-084`; `LAB-FE-014`; [`03-frontend-architecture.md`](../../../03-frontend-architecture.md) bagian 12 |
| Contract version | `LAB-API-v1` **`r24` bagian 19.4** `approved`, dilengkapi amandemen **`r31` bagian 26** `approved` 2026-09-22 |
| Wewenang UI | Dua layar data induk baru di `health-services/master-data/`. **Batasnya:** nol layar Laboratorium lain disentuh, nol perubahan backend, nol jalur penghapusan dibangun |
| Dependency | `BE-LAB-44` ✅ selesai 2026-09-18 (tabel dan empat endpoint); `BE-LAB-65` ✅ selesai 2026-09-22 (empat endpoint baseline sisanya dan ruas `discContentUg`) |
| Klasifikasi | `HEAVY` — dua fitur data induk utuh, 30 berkas baru, 8 route baru, 14 endpoint dikonsumsi pada 2 grup Swagger, 2 registrasi global. *Catatan pembukuan:* dokumen penilaian canonical `TASK_CLASSIFICATION.md` hanya ada di `rules/backend/`, dan `AGENTS.md` frontend melarang memakai `rules/backend/` sebagai aturan task frontend — karena itu nilainya ditulis deskriptif, bukan sebagai skor bernomor |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`, `tests/unit/`), ditambah berkas laporan ini beserta tautan buktinya pada roadmap, traceability, dan manifest modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `6794cc3e6` — branch `YogaV2`, upstream `origin/YogaV2` |
| Commit backend yang dijadikan rujukan | `c6f6ab31` — branch `yoga` |
| Tanggal | 2026-09-22 |
| Status | **`SELESAI` pada sisi kode.** Lint, uji unit, build, dan pemeriksaan route terhadap aplikasi yang berjalan seluruhnya lulus. **Satu butir verifikasi belum tercapai dan disebut apa adanya:** layar belum dijalankan terhadap backend yang berdiri — lihat bagian 6 |

---

## 1. Keadaan yang ditemukan di awal

**Kedua layar belum ada sama sekali.** Penelusuran `src` untuk `lab-organisms` dan
`lab-antibiotics` hanya menemukan dua hal, dan keduanya bukan layar pengelolaan:

| Yang sudah ada | Bentuknya | Siapa yang memakai |
| --- | --- | --- |
| `LABORATORY_API.labOrganisms` dan `.labAntibiotics` pada `laboratory-constants.jsx` | Alamat dasar, **dibaca saja** | Layar hasil Mikrobiologi (`FE-LAB-31`, `FE-LAB-32`) |
| Pendaftaran `labOrganisms` dan `labAntibiotics` pada `health-service-select-resources.js` | Alamat `/options`, isi kotak pilihan | Layar rentang breakpoint (`FE-LAB-34`) |

Artinya: kedua daftar sudah **dibaca** dari tiga layar, tetapi **nol layar yang dapat
mengisinya**. Kepala instalasi tidak punya satu pun jalan menambah kuman atau antibiotik dari
aplikasi. Itulah `B3` pada audit kesiapan, dan itulah bentuk kegagalan yang sudah dua kali dibayar
modul lain — `LAB-COORD-006` dan `MST-POS-WRITE`, keduanya tabel yang berdiri tanpa pernah terisi.

**Penahannya sudah gugur, dan pembukuannya tertinggal.** Blok roadmap masih bertulis
`MENUNGGU BE-LAB-44` padahal `BE-LAB-44` selesai 2026-09-18; statusnya dikoreksi 2026-09-22. Yang
benar-benar membuka task ini adalah `BE-LAB-65`, yang melengkapi permukaannya dari empat menjadi
delapan endpoint. Tanpa `GET /{id}`, formulir ubah yang dibuka lewat tautan langsung nol punya
jalan memuat barisnya — dan **gagalnya diam**, kelas kesalahan yang sudah dibayar modul ini sekali
lewat `r6` sesudah `FE-LAB-03`.

**Satu kolom sudah ada di tabel tetapi nol punya jalan diisi.** `LabAntibiotic.DiscContentUg`
berdiri sejak `BE-LAB-60`; `BE-LAB-64` membangun angka `missingDiscContent` yang melaporkan berapa
baris masih kosong; dan sampai `BE-LAB-65` membuka ruasnya, laporan itu menyebutkan pekerjaan
tersisa tanpa menyediakan satu pun tempat mengerjakannya. Layar Antibiotik pada task inilah tempat
itu.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Tujuan, pelaku, dan pemicu

| Butir | Isi |
| --- | --- |
| **Tujuan** | Daftar kuman dan daftar antibiotik terisi dan terpelihara dari aplikasi, sehingga analis dapat memilihnya saat mencatat hasil Mikrobiologi |
| **Pelaku** | **Kepala instalasi laboratorium.** Ia yang menambah, mengubah, dan menonaktifkan. Analis hanya **memakai** daftarnya dari layar hasil, dan `LAB-DEC-098` butir 5 melarang layar hasil menumbuhkan data induk — daftar yang tumbuh dari meja kerja akan penuh ejaan berbeda untuk hal yang sama |
| **Pemicu** | Kuman atau antibiotik baru perlu masuk panel uji; atau yang lama ditarik dari daftar |
| **Prasyarat** | Pengguna punya hak akses `LabOrganism : Create`/`Update` atau `LabAntibiotic : Create`/`Update` |

### 2.2 Langkah utama — menambah satu kuman

1. Kepala instalasi membuka menu **Data Master → Organisme Mikrobiologi**.
2. Layar menampilkan empat kartu ringkasan (Total, Aktif, Nonaktif, Punya Breakpoint) dan daftar
   kuman yang sudah ada.
3. Ia menekan **+ Tambah Organisme Mikrobiologi**.
4. Ia mengisi **Kode Organisme** (contoh `BCAT`), **Nama Organisme**
   (contoh `Branhamella catarrhalis`), **Keterangan** bila perlu, dan **Urutan Tampil**.
5. Ia menekan **Simpan Organisme Mikrobiologi**.
6. Layar menampilkan pesan hijau *"Organisme berhasil ditambahkan."* lalu membuka halaman detail
   baris yang baru saja disimpan.

Kuman baru **selalu lahir aktif**. Ruas status tidak ada di formulir tambah, karena menambah
sesuatu dalam keadaan nonaktif adalah pekerjaan yang harus diulang.

### 2.3 Langkah utama — menarik satu kuman dari daftar

1. Pada baris kuman yang bersangkutan, ia menekan **Nonaktifkan**.
2. Muncul pertanyaan yang **menyebutkan akibatnya**, bukan sekadar "Anda yakin?":

   > *Organisme Branhamella catarrhalis tidak akan lagi muncul saat analis mencatat isolat. Isolat
   > yang sudah tersimpan tetap menyimpan organisme ini, dan barisnya tetap terlihat di daftar ini.*

3. Setelah ia menekan **Ya, Nonaktifkan**, penanda baris berubah menjadi **Nonaktif**. **Barisnya
   tetap ada di daftar** (`AC-118`) — tanpa itu, kuman yang dinonaktifkan nol akan pernah dapat
   diaktifkan kembali.

**Tidak ada tombol Hapus di mana pun pada kedua layar** (`AC-117`), dan itu keputusan klinis:
isolat pasien yang sudah tercatat menunjuk ke baris ini. Menghapusnya berarti menghapus temuan
pasien.

### 2.4 Langkah utama — mengisi kandungan cakram antibiotik

1. Ia membuka **Data Master → Antibiotik**.
2. Kartu ringkasan **Tanpa Kandungan Cakram** menunjukkan berapa antibiotik aktif yang kolom `UG`
   pada cetakan antibiogram-nya masih kosong. Contoh: angkanya `1`.
3. Ia menekan **Perbarui** pada baris yang kosong, mengisi **Kandungan Cakram (UG)** dengan `10`,
   lalu menyimpan.
4. Angka **Tanpa Kandungan Cakram** turun dari `1` menjadi `0`, dan kolom **Kandungan Cakram (UG)**
   pada daftar terisi `10`.

**Kosong bukan nol.** Bila ruas itu dikosongkan, yang dikirim adalah nilai kosong (`null`), bukan
angka `0`. Mengirim `0` akan mencetak angka nol pada lembar antibiogram **dan sekaligus** menghapus
baris itu dari hitungan "pekerjaan tersisa" — pekerjaan hilang dari pandangan tanpa pernah
dikerjakan. Antibiotik yang hanya diuji dengan metode dilusi memang tidak punya cakram, dan untuk
baris seperti itu kosong adalah jawaban yang benar.

### 2.5 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Daftar masih kosong | Tabel menampilkan *"Data organisme Mikrobiologi tidak ditemukan."* beserta ajakan menambah data baru |
| Gagal memuat daftar | Kalimat galat dari backend tampil apa adanya di atas tabel, dan tombol Reset tetap dapat ditekan |
| Tautan detail dibuka dari sesi lain | *"Tautan detail organisme tidak valid. Silakan buka ulang dari daftar data."* — dan permintaan detail **nol dikirim** |
| Kode yang diketik sudah dipakai | Backend menjawab `409`, dan pesannya tampil apa adanya sebagai notifikasi merah; formulir **tidak** dikosongkan |
| Tanpa hak akses | Halaman diganti tampilan akses ditolak; tombol Tambah, Perbarui, dan Nonaktifkan **nol dirender** bagi pengguna tanpa kewenangan |
| Alamat memakai kata cadangan, misalnya `.../lab-organisms/update` | Halaman tidak ditemukan; alamat itu nol pernah diperlakukan sebagai penanda baris |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `rules/frontend/master-data-feature-standard.md` | Bentuk wajib satu fitur data induk: tujuh berkas source ditambah dua registrasi |
| `rules/frontend/test-policy.md`, `ui-consistency-checklist.md` | Kewajiban uji dan grep anti-regresi |
| `AGENTS.md` frontend | Larangan factory/generator, batas mode task, aturan Redux dan Axios |
| `roadmap/frontend-roadmap.md` bagian 6e.1 | Cakupan, acceptance criteria, dan DoD |
| `LabOrganismController.cs`, `LabAntibioticController.cs` | Kedelapan endpoint per grup, nama aksi hak akses, nilai `[Tags(...)]` |
| `LabMicrobiologyMasterDataDtos.cs` | Bentuk request dan response yang sebenarnya |
| `LabFilterMetadataFactory.cs`, `LabFilterAndSummaryDtos.cs` | Isi `GET /filters/metadata`, termasuk `IsDeletable` |
| `LabMicrobiologyMasterDataService.cs` | Perilaku `UpdateAsync` terhadap `IsActive` — lihat bagian 3.4 |
| `lab-susceptibility-breakpoint` (14 berkas) | Modul rujukan utama: bentuk paling mutakhir, punya halaman detail, nol CSS Module |
| `lab-specimen-types` (12 berkas) | Modul rujukan kedua, disebut roadmap: pola data induk Laboratorium tanpa penghapusan |
| `health-service-select-resources.js` | Tempat alamat `/options` kedua katalog sudah terdaftar |
| `src/lib/state/store.jsx`, `src/utils/menu-sidebar/menu-items.jsx` | Pola registrasi reducer dan menu |

### 3.2 Berkas yang berubah

**Dua berkas disunting, 30 berkas baru.**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/store.jsx` | Dua reducer didaftarkan: `masterDataLabOrganism` dan `masterDataLabAntibiotic`, memakai `stateKey` dari config |
| `src/utils/menu-sidebar/menu-items.jsx` | Dua entri menu baru — **Organisme Mikrobiologi** dan **Antibiotik** — disisipkan tepat sebelum **Rentang Breakpoint**, mengikuti urutan pemakaiannya: katalog lebih dulu, rentang yang merujuk katalog sesudahnya |

Fitur **Organisme Mikrobiologi** — 15 berkas baru:

| Berkas | Isi |
| --- | --- |
| `src/lib/constants/health-services/master-data/lab-organisms/lab-organism-constants.jsx` | Objek `LAB_ORGANISM_CONFIG` tunggal: endpoint, route, salinan teks, kolom, ringkasan, ruas formulir, baris detail, dan pesan gagal |
| `src/lib/state/slice/health-services/master-data/master-data-lab-organism-slice.jsx` | Tujuh thunk, state, reducer, dan selector |
| `src/utils/health-services/master-data/lab-organisms/lab-organism-utils.jsx` | Fungsi murni: baca ruas, bentuk formulir, validasi, bentuk payload, baris detail |
| `src/lib/hooks/.../lab-organisms/use-master-data-lab-organism.jsx` | Controller halaman daftar, termasuk aksi aktif/nonaktif |
| `…/use-master-data-lab-organism-detail.jsx` | Controller halaman detail |
| `…/use-master-data-lab-organism-editor.jsx` | Controller formulir tambah dan ubah |
| `src/components/view/.../lab-organisms/master-data-lab-organism-view.jsx` | Tampilan halaman daftar |
| `…/detail/lab-organism-detail-view.jsx` | Tampilan halaman detail |
| `…/add/lab-organism-form-view.jsx` | Tampilan formulir, dipakai kedua mode |
| `src/app/health-services/master-data/lab-organisms/page.jsx` | Route daftar |
| `…/lab-organisms-client.jsx` | Pembungkus client |
| `…/create/page.jsx` | Route tambah |
| `…/[slug]/page.jsx` | Route detail, beserta penolakan kata cadangan |
| `…/[slug]/update/page.jsx` | Route ubah, beserta penolakan kata cadangan |

Fitur **Antibiotik** — 14 berkas baru dengan bentuk yang sama persis, pada folder `lab-antibiotics`
dan berawalan `lab-antibiotic-`.

Ditambah:

| Berkas | Isi |
| --- | --- |
| `src/style/health-services/master-data/shared/lab-microbiology-catalog.module.css` | Satu kelas `.rowActions` — lihat alasannya pada bagian 3.3 |
| `tests/unit/lab-microbiology-catalog-fe24-rules.test.mjs` | 30 uji unit atas aturan murni kedua fitur |

### 3.3 Kepatuhan arsitektur frontend

| Butir | Yang dilakukan |
| --- | --- |
| Bentuk wajib data induk | Tujuh berkas source ditambah dua registrasi, persis seperti `master-data-feature-standard.md` bagian 1 |
| Nol abstraksi baru | Struktur modul rujukan **disalin lalu diisi ulang**, bukan diabstraksi menjadi satu generator. Nol factory, nol hook data induk generik, nol arsitektur konstanta bersama |
| Redux | Slice mengikuti `master-data-lab-susceptibility-breakpoint-slice.jsx`: bentuk state, pembantu `sanitize*`, `isAbortAction`, `mergeItemById`, dan nama selector |
| Axios | Seluruh permintaan lewat `InstanceAxios`. Nol instance baru, nol `fetch`, nol penyusunan alamat dasar sendiri |
| Pembatalan permintaan | Setiap effect yang mengirim permintaan punya `abort` pada cleanup, dan pembatalan **nol menulis pesan galat** |
| `totalPage` | Dibaca dari backend, nol dihitung ulang dari panjang daftar |
| Larangan UUID | Navigasi memakai token route privat; `safeString` mengembalikan `-` untuk nilai berbentuk UUID |
| Base component | `Hero`, `SummaryCards`, `DataFilter`, `DataTable`, `StatusBadge`, `ConfirmModal`, `ToastStack`, `BaseEditorView`, `BaseDetailView`, `AccessDeniedGate`, `RegionPagination` — seluruhnya dipakai ulang, nol komponen baru dibuat |

**Dua penyimpangan dari standar, keduanya disengaja dan dicatat di sini apa adanya.**

**Pertama: nol thunk `//options`, walaupun standar bagian 3 memetakan sembilan endpoint ke sembilan
thunk.** Alamat `/options` kedua katalog **sudah terdaftar** di
`src/lib/hooks/select/health-service/health-service-select-resources.js` sejak `FE-LAB-34`, dengan
alasan yang ditulis di berkas itu sendiri: registry tersebut satu-satunya tempat repository
menyimpan alamat `/options`, dan alamat yang sama ditulis dua kali adalah cara yang pasti
menghasilkan dua daftar berbeda ketika salah satunya kelak disaring. Menambah thunk kedua berarti
menuliskannya untuk kedua kalinya. Karena `AGENTS.md` frontend menang atas dokumen standar, dan
karena keputusan itu sudah tertulis di source, **source yang diikuti**. Kedua slice punya tujuh
thunk, bukan delapan; endpoint kedelapan tetap terkonsumsi, hanya dari tempat yang sudah ada.

**Kedua: satu CSS Module baru, walaupun standar bagian 12 menyatakan fitur data induk nol menambah
CSS Module.** Sebabnya sempit. 55 fitur data induk HR yang konform nol punya kolom aksi — barisnya
dibuka lewat detail, dan penghapusannya tinggal di sana. Kedua layar ini **nol punya penghapusan**
(`AC-117`), sementara standar bagian 7.2 melarang tombol Aktifkan dan Nonaktifkan dirender di
halaman detail. Satu-satunya tempat tersisa bagi penonaktifan adalah kolom aksi halaman daftar, dan
kolom itu butuh jarak antar tombolnya. Alternatifnya inline style, yang dilarang checklist
konsistensi UI. Berkasnya berisi **satu kelas**, nol nilai warna/jarak literal, dan diletakkan di
`shared/` mengikuti `src/style/hr/master-data/shared/` — dua layar kembar dengan kebutuhan sama
persis, dan berkas lima baris yang disalin dua kali adalah hal yang justru dihindari folder itu.
Preseden yang sama sudah ada pada `lab-specimen-types`, `lab-rejection-reasons`, dan
`lab-value-bounds`.

### 3.4 Satu jebakan backend yang ditutup di layar, dan kenapa ia berbahaya

`UpdateLabOrganismRequest.IsActive` dan `UpdateLabAntibioticRequest.IsActive` **bernilai `true`
ketika permintaannya tidak memuat ruas itu**, dan `UpdateAsync` menyetel `entity.IsActive` tanpa
syarat:

```csharp
entity.IsActive = request.IsActive;
```

Akibatnya: formulir ubah yang tidak mengirim status akan **menghidupkan kembali kuman yang sudah
ditarik dari daftar** hanya karena seseorang membetulkan ejaan keterangannya. Nol galat muncul,
nol pesan, dan kuman itu kembali muncul di kotak pilihan analis.

Karena itu ruas **Status Aktif** ada pada formulir ubah kedua layar, bertanda `updateOnly`, dan
nilainya dibaca **apa adanya dari baris yang sedang disunting** — bukan dari nilai bawaan. Dua uji
unit mengunci perilaku ini, satu per fitur.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| **Memuat** | Tabel menampilkan *"Mengambil data organisme Mikrobiologi..."* / *"Mengambil data antibiotik..."*; kartu ringkasan menampilkan keadaan memuatnya sendiri; tombol Reset dan pemilih filter dinonaktifkan sementara |
| **Kosong** | *"Data organisme Mikrobiologi tidak ditemukan."* beserta *"Coba gunakan filter lain atau tambahkan data baru."* Pada halaman detail: *"Informasi detail belum tersedia."* |
| **Gagal** | Kalimat galat dari backend tampil apa adanya di atas tabel. Pemulihannya: ubah filter, tekan Reset, atau buka ulang halaman. Kegagalan menyimpan muncul sebagai notifikasi merah **tanpa mengosongkan formulir**, sehingga isian pengguna nol hilang |
| **Tanpa hak akses** | Halaman diganti tampilan akses ditolak. Tombol **+ Tambah**, **Perbarui**, dan **Nonaktifkan/Aktifkan** nol dirender bagi pengguna tanpa kewenangan `Create`/`Update`. Penegakan sesungguhnya tetap di backend |
| **Tautan tidak sah** | *"Tautan detail organisme tidak valid. Silakan buka ulang dari daftar data."*, dan permintaan detail nol dikirim |
| **Backend berubah diam-diam** | Bila `GET /filters/metadata` suatu saat mengumumkan `IsDeletable: true`, layar memunculkan peringatan kuning yang meminta selisihnya dilaporkan — **diperiksa terbalik**, supaya yang tercatat adalah backend yang berubah, bukan layar yang tertinggal tanpa ada yang tahu |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Organism

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-organisms/filters/metadata` | Pilihan jumlah baris, dan penanda `IsDeletable` yang dibaca layar | `LabOrganism : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-organisms/summary` | Empat kartu ringkasan | `LabOrganism : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-organisms` | Daftar utama, **termasuk baris nonaktif** | `LabOrganism : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-organisms/{id}` | Halaman detail dan pemuatan formulir ubah | `LabOrganism : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-organisms` | Menambah kuman baru | `LabOrganism : Create` |
| `PUT` | `/v1/health-services/laboratory-management/lab-organisms/{id}` | Mengubah nama, keterangan, urutan, dan status | `LabOrganism : Update` |
| `PATCH` | `/v1/health-services/laboratory-management/lab-organisms/{id}/status` | Tombol Aktifkan/Nonaktifkan pada kolom aksi | `LabOrganism : Update` |
| `GET` | `/v1/health-services/laboratory-management/lab-organisms/options` | **Nol dipanggil layar ini.** Dikonsumsi layar rentang breakpoint lewat registry pilihan yang sudah ada | `LabOrganism : Read` |

#### Health Services / Laboratory Management / Lab Antibiotic

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-antibiotics/filters/metadata` | Pilihan jumlah baris, dan penanda `IsDeletable` | `LabAntibiotic : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-antibiotics/summary` | Lima kartu ringkasan, termasuk **Tanpa Kandungan Cakram** | `LabAntibiotic : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-antibiotics` | Daftar utama, termasuk baris nonaktif | `LabAntibiotic : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-antibiotics/{id}` | Halaman detail dan pemuatan formulir ubah | `LabAntibiotic : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-antibiotics` | Menambah antibiotik baru | `LabAntibiotic : Create` |
| `PUT` | `/v1/health-services/laboratory-management/lab-antibiotics/{id}` | Mengubah nama, kandungan cakram, keterangan, urutan, dan status | `LabAntibiotic : Update` |
| `PATCH` | `/v1/health-services/laboratory-management/lab-antibiotics/{id}/status` | Tombol Aktifkan/Nonaktifkan pada kolom aksi | `LabAntibiotic : Update` |
| `GET` | `/v1/health-services/laboratory-management/lab-antibiotics/options` | **Nol dipanggil layar ini.** Dikonsumsi layar rentang breakpoint | `LabAntibiotic : Read` |

**Nol `DELETE` pada kedua grup**, dan itu bukan kelalaian pembukuan — endpoint-nya memang tidak ada,
dan `AC-117` menuntut layarnya tidak menyediakan tombolnya.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada seluruh berkas baru dan kedua berkas yang disunting | **0 error, 4 warning** | `PASS` | Keempat warning berasal dari satu aturan, `react-hooks/set-state-in-effect`, pada penyelesaian token route. Baseline diukur: `lab-susceptibility-breakpoint` dan `lab-specimen-types` menghasilkan **aturan dan jumlah yang sama persis** — 0 error, 4 warning. Nol kelas peringatan baru |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-microbiology-catalog-fe24-rules.test.mjs` | **30 lulus, 0 gagal** | `PASS` | Keluaran perintah |
| Seluruh suite unit repository | **1586 lulus, 6 gagal dari 1592** | `EXISTING / ENVIRONMENT ISSUE` | Keenam kegagalan **sudah ada sebelum task ini** dan nol menyentuh Laboratorium: satu menuntut entri menu `/corporate/accounting/reconciliation` yang memang belum ada, lima sisanya milik `FE-RWI-042`/`FE-RWI-043`. Jumlah lulus naik dari 1556 menjadi 1586, tepat sebesar 30 uji baru; jumlah gagal **tidak bertambah** |
| `npm run build` | **Berhasil, exit code 0** | `PASS` | Kedelapan route baru terkompilasi, diverifikasi pada `.next/app-path-routes-manifest.json` |
| Membuka empat route utama pada aplikasi yang berjalan | `200` pada keempatnya, dengan judul halaman yang benar | `PASS` | `Master Data Organisme Mikrobiologi`, `Master Data Antibiotik`, `Tambah Organisme Mikrobiologi`, `Tambah Antibiotik` |
| Membuka halaman detail dengan penanda biasa | Halaman detail dirender | `PASS` | `.../lab-organisms/abc123` dan `.../lab-antibiotics/abc123` |
| Membuka halaman detail dengan kata cadangan | Halaman tidak ditemukan | `PASS` | `.../lab-organisms/update` dan `.../lab-antibiotics/edit` merender halaman `not-found`. Perilakunya **sama persis** dengan route rujukan `lab-susceptibility-breakpoint` |
| Enam grep anti-regresi `ui-consistency-checklist.md` | **Nol hasil pada keenamnya** | `PASS` | Nol warna literal, nol tipografi literal, nol `!important`, nol `<button>`/`btn-*` mentah, nol `<table>` mentah, nol kelas utilitas `fw-*`/`fs-*` pada berkas baru |
| Menjalankan layar terhadap backend yang berdiri | **Belum tercapai** | `BLOCKED` | Backend gagal start: *"Jwt:Key belum dikonfigurasi di appsettings.json."* Buildnya berhasil; yang kurang konfigurasi environment, bukan kode |

**Uji manual: `NOT FEASIBLE` untuk skenario yang membutuhkan data.**

Alasannya konkret, bukan umum. Backend tidak berjalan pada putaran ini — port `7184` nol mendengar.
Backend **dicoba dinyalakan** dengan `dotnet run` dan buildnya berhasil, tetapi aplikasinya berhenti
saat start dengan pesan *"Jwt:Key belum dikonfigurasi di appsettings.json."* Nilai kunci itu adalah
rahasia environment setempat; ia **sengaja tidak diisi, ditebak, maupun dibuat** dari task ini, dan
isinya nol ditulis di mana pun pada laporan ini. Menyalakan backend karena itu perlu dilakukan
pemilik environment.

Tanpa backend, kesembilan kontrol berikut nol dapat dibuktikan bekerja:
pemilih status, pemilih jumlah baris, kotak pencarian, tombol Reset, paginasi, tombol
Aktifkan/Nonaktifkan beserta modal konfirmasinya, penyimpanan formulir tambah, penyimpanan formulir
ubah, dan turunnya angka **Tanpa Kandungan Cakram** setelah kandungan cakram diisi.

Yang **dapat** dibuktikan tanpa backend sudah dibuktikan: kedelapan route berdiri dan merender
halaman yang benar, penjaga kata cadangan bekerja, dan seluruh aturan murni — bentuk payload,
validasi, baris detail — terkunci 30 uji unit.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `npm run test:e2e` | `test-policy.md` menetapkan e2e hanya dijalankan bila task memintanya. `FE-LAB-24` nol memintanya |
| Pemanggilan kedelapan endpoint terhadap database sungguhan | Milik `BE-LAB-65`, dan sudah dilakukan di sana pada 2026-09-22 |
| Mengisi data induknya | `B3` pada audit kesiapan. Task ini membuka **layarnya**; mengisinya pekerjaan kepala instalasi |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-116` — keduanya berada di `master-data/`, **bukan** di folder Laboratorium (`LAB-FE-014`) | **Terpenuhi** | Route `src/app/health-services/master-data/lab-organisms/` dan `.../lab-antibiotics/`; tampilan di `src/components/view/health-services/master-data/`; entri menu di bawah **Data Master**. Nol berkas ditaruh di folder `laboratory-management` |
| `AC-117` — **nol tombol Hapus**, hanya penonaktifan | **Terpenuhi** | Dibuktikan **terbalik** pada tiga lapis: kedua slice nol punya thunk penghapusan; halaman detail merender **dua** aksi saja, `Kembali` dan `Perbarui`, dan nol meneruskan `deleteConfirm`; kolom aksi daftar merender `Perbarui` dan `Aktifkan/Nonaktifkan` saja. Dua uji unit mengunci `isDeletable: false` dan ketiadaan pesan gagal penghapusan pada kedua config |
| `AC-118` — baris nonaktif tetap terlihat dengan penanda, tidak hilang dari daftar | **Terpenuhi** | Nilai awal filter status **kosong**, sehingga aktif dan nonaktif sama-sama ditampilkan; kolom **Status** merender `StatusBadge` bertulis `Aktif`/`Nonaktif`; jawaban `PATCH` memperbarui baris **di tempat** tanpa membuangnya dari daftar. Tiga uji unit mengunci ketiganya |
| **DoD** — kedua layar berjalan | **Terpenuhi pada sisi kode, belum diklik di peramban** | Kedelapan route terkompilasi pada build produksi dan menjawab `200` pada aplikasi yang berjalan, dengan judul halaman yang benar. Yang belum: menjalankannya terhadap backend berisi data — lihat bagian 6 |
| **DoD** — `AC-116`..`AC-118` terbukti | **Terpenuhi** | Ketiganya di atas |
| **DoD** — seluruh uji Laboratorium tetap lulus | **Terpenuhi** | Nol kegagalan baru. Jumlah gagal tetap 6, keenamnya sudah ada sebelumnya dan nol menyentuh Laboratorium |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Peringatan** | Empat warning lint dari satu aturan, `react-hooks/set-state-in-effect`, pada penyelesaian token route. Jumlah dan aturannya **sama persis** dengan kedua modul rujukan, sehingga nol kelas peringatan baru diperkenalkan. Memperbaikinya berarti mengubah pola penyelesaian token di seluruh repository, dan itu task tersendiri |
| **Masalah yang diketahui** | **Satu cacat warisan ditemukan dan diperbaiki hanya pada berkas baru.** Pemeriksaan panjang maksimal pada `validateForm` modul rujukan **nol pernah menyala**: nilainya dipotong lebih dulu oleh `safeInputString(value, field.maxLength)`, sehingga perbandingan `panjang > maxLength` selalu salah. Akibatnya kode yang kepanjangan **tersimpan diam-diam dalam bentuk terpotong** — untuk kode organisme dan antibiotik itu berarti penanda permanen yang berbeda dari yang diketik, tanpa satu pun pesan. Ditemukan karena uji unitnya ditulis lebih dulu dan gagal. Pada kedua berkas baru, panjangnya kini diukur **sebelum** dipotong, dan dua uji menguncinya. **`lab-specimen-type-utils.jsx` dan `lab-susceptibility-breakpoint-utils.jsx` sengaja nol disentuh** — keduanya di luar cakupan task ini, dan cacatnya dilaporkan di sini alih-alih ditambal diam-diam |
| **Dependency backend** | `NONE` yang menahan. Kedua pasangan backend selesai. **Satu dependency yang bukan kode masih terbuka:** daftar organisme dan antibiotik **belum terisi** (`B3`). Layar ini membuka jalannya; pengisiannya pekerjaan kepala instalasi. Selama daftarnya kosong, `AC-127` pada `FE-LAB-26` — peringatan daftar kuman kosong — adalah yang dilihat analis di layar hasil Mikrobiologi |
| **Perubahan sampingan** | `NONE`. `git status --short` memuat tepat 32 baris: 2 berkas disunting dan 30 berkas baru, seluruhnya milik task ini |
| **Interupsi** | **Satu, dan akibatnya dipulihkan.** Build produksi dijalankan ketika dev server frontend sedang hidup, sehingga keduanya menulis ke folder `.next` yang sama dan **route dinamis pada dev server berubah menjadi galat `500` — termasuk route fitur lama** `lab-susceptibility-breakpoint`, yang membuktikan sebabnya bukan kode task ini. Dev server dimatikan lalu dinyalakan ulang, dan seluruh route dinamis kembali `200`. Nol berkas source terdampak |
| **Status Git** | **Frontend (`QuilvianSystemFrontendDev`, branch `YogaV2`):** `M src/lib/state/store.jsx`, `M src/utils/menu-sidebar/menu-items.jsx`, ditambah 30 berkas baru pada 12 folder. **Backend (`NewQuilvianSystemBackend`, branch `yoga`):** empat berkas dokumentasi saja — `M blueprint-manifest.md`, `M roadmap/frontend-roadmap.md`, `M roadmap/traceability.md`, dan laporan ini. **Nol berkas source backend disentuh**, sesuai batas wewenang `FRONTEND MODE`. **Nol operasi Git dijalankan** — nol `add`, `commit`, `push`, `pull`, `merge`, maupun `rebase` |
| **Langkah berikutnya** | 1. Isi `Jwt:Key` pada konfigurasi environment setempat, jalankan backend, lalu buktikan kesembilan kontrol interaktif pada bagian 6 benar-benar bekerja. 2. Isi kedua daftar dari layar ini, sehingga `B3` tertutup **dari aplikasi** — bukan lewat SQL langsung ke basis data. 3. Sesudah daftarnya terisi, `AC-126` dan `AC-127` pada `FE-LAB-26` dapat dilihat bekerja, dan blok itu dapat ditandai `DIGANTIKAN` penuh. 4. `FE-LAB-27` masih tertahan: ketiga grup data induk Patologi Anatomi memikul gap yang sama persis dengan yang ditutup `BE-LAB-65`, dan gap itu **masih terbuka** |
