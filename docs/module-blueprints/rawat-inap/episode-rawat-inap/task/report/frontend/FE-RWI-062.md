# Laporan Perubahan Frontend — `FE-RWI-062`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-062` |
| Judul | Layar berhenti menjelaskan penolakan yang tidak pernah terjadi lagi |
| Slice | `F14` — pembersihan frontend |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 5, kartu `FE-RWI-062` |
| Trace | `RWI-DEC-101`; `FR-RI-180`; `03-frontend-architecture.md` revision `0.7` bagian 4.3A.1 |
| Contract version | `contracts/api-contract.md` `0.8.0` dan `contracts/validation-matrix.md` `0.8.0` — keduanya **`approved`** 11 September 2026 oleh Muhammad Hamzah lewat `RWI-DEC-105` |
| Wewenang UI | Kartu berbunyi **nol keputusan rupa baru**; warna, jarak, dan susunan tetap `DEV_DISCRETION`. Wewenang itu tidak dipakai — tidak ada satu pun baris JSX atau CSS yang diubah |
| Dependency | `BE-RWI-073` ✅ **selesai** 11 September 2026, dibuktikan laporan [`task/report/backend/BE-RWI-073.md`](../backend/BE-RWI-073.md) dan source `InpBedOccupancyService.cs` |
| Klasifikasi | `LIGHT` — skor 3. Repository 1, berkas diperiksa 9, berkas diubah 4 (2 source, 2 test), logika bisnis 0, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 0 |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev/src/`, `QuilvianSystemFrontendDev/tests/`; ditambah wewenang lintas repository yang sempit untuk laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` |
| Model | `claude-opus-5` |
| Commit frontend saat dikerjakan | `7f6b9356f6349d516570d6d603ca026f2c7f4ec2` pada branch `HamzahV2`, upstream `origin/HamzahV2` — perubahan task ini **masih lokal, belum di-commit** |
| Commit backend yang dijadikan rujukan | `d6858a9dba706f96a77e5e1855a9e4ae7cd78566` pada branch `MHamzah` — dibaca saja, tidak diubah |
| Tanggal | 12 September 2026 |
| Status | ✅ **Selesai 12 September 2026.** Keenam acceptance criteria terpetakan ke source. `npm run test:unit` 737 lulus 0 gagal, `npm run lint:errors` bersih, `npm run build` berhasil. Satu butir verifikasi tidak dapat dijalankan dan ditulis apa adanya: bukti peramban terhalang `RWI-UI-GAP-007` dan ketiadaan `playwright.config.*` di repository |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sampai 11 September 2026, aturan Kelayakan Penempatan nomor 6 menolak pasien bila kamar yang
dituju sedang dihuni pasien berjenis kelamin lain. Penolakan itu dikirim server dengan kode
`ROOM_GENDER_MIXED`, dan layar memetakan kodenya supaya dapat dibedakan dari aturan lain.

`RWI-DEC-101` mencabut aturan itu seluruhnya, dan `BE-RWI-073` sudah menghentikan penerbitannya di
server pada 11 September 2026. Akibatnya frontend menyimpan pemetaan untuk sebuah penolakan yang
**tidak akan pernah terbit lagi**. Pemetaan yang menganggur seperti itu bukan sekadar kode mati:
pembaca berikutnya akan mengira layar masih perlu menangani pencampuran kamar, lalu menghidupkan
kembali aturan yang sudah sengaja dicabut.

### 1.2 Bukti keadaan awal

Pencarian pada repository frontend sebelum pekerjaan ini, di luar folder hasil build:

| Lokasi | Temuan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` baris 12 | Satu entri `ROOM_GENDER_MIXED` di dalam `PLACEMENT_RULE_CODES` |
| `src/components/features/health-services/inpatient-management/placement-failure-list.jsx` baris 14–16 | Komentar yang menerangkan bahwa kalimat penolakan **pencampuran jenis kelamin** ditampilkan lengkap dengan nama kamarnya |
| `tests/unit/inpatient-placement.test.mjs` baris 57, 82, 129 | Tiga assertion memakai `ROOM_GENDER_MIXED` sebagai contoh |
| `tests/e2e/inpatient-episode-detail.spec.mjs` baris 26, 605, 648 | Satu skenario perpindahan yang seluruh penolakannya bersandar pada `ROOM_GENDER_MIXED` |

### 1.3 Temuan yang mengubah cakupan pekerjaan

Kartu roadmap meminta "sesuaikan pesan `PATIENT_GENDER_UNKNOWN` mengikuti kalimat baru pada
validation matrix". Pemeriksaan source membuktikan **frontend tidak pernah menyusun kalimat itu
sendiri**. Kalimat setiap penolakan datang apa adanya dari server lewat kolom `errors[].message`,
lalu ditampilkan tanpa diubah. Pencarian kalimat lama "di kamar yang belum ada penghuninya" pada
`src/` dan `tests/` menghasilkan **nol** temuan.

Artinya kriteria kedua sudah terpenuhi secara struktural sejak awal, dan cara mempertahankannya
bukan mengedit kalimat di layar, melainkan memastikan layar tetap tidak mengarang kalimat sendiri.
Itu yang sekarang dijaga assertion baru pada test unit.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas admisi, perawat ruangan, kepala ruangan, dan supervisor — masing-
masing menurut hak akses yang diberikan admin. Hak aksesnya **tidak berubah** oleh task ini.

**Kapan layar dibuka.** Saat petugas menempatkan pasien rawat inap ke tempat tidur, memesan tempat
tidur, atau memindahkan pasien yang sudah dirawat ke tempat tidur lain.

**Langkah berurutan pada jalur normal.**

1. Petugas membuka Papan Tempat Tidur atau halaman Detail Episode.
2. Petugas memilih satu tempat tidur dari daftar. Daftar itu datang dari server apa adanya; layar
   tidak menyaring ulang tempat tidur menurut jenis kelamin maupun penghuni kamar.
3. Petugas menekan aksi penempatan, pemesanan, atau perpindahan.
4. Server menjalankan pemeriksaan kelayakan, lalu menyimpan bila lolos.
5. Layar menampilkan hasilnya.

**Jalur tidak normal — penolakan aturan (422).** Server mengirim daftar aturan yang gagal. Layar
menampilkan judul "Penempatan ditolak aturan kelayakan", lalu **setiap** aturan ditampilkan sebagai
satu baris berisi nomor aturannya dan kalimat servernya apa adanya. Kalimatnya tidak diringkas dan
tidak diganti kalimat buatan layar.

**Jalur tidak normal — keadaan berubah (409).** Misalnya tempat tidur direbut pasien lain. Layar
menampilkan judul "Keadaan tempat tidur sudah berubah", memuat ulang daftar tempat tidur, dan
menambahkan keterangan bahwa isian yang sudah diketik tetap tersimpan di layar.

**Yang berubah bagi pengguna sesudah task ini.**

| Keadaan | Sebelum | Sesudah |
| --- | --- | --- |
| Ny. Sari ditempatkan di kamar yang sedang dihuni Tn. Budi, pada tempat tidur yang menerima laki-laki dan perempuan | Ditolak, layar menjelaskan "Kamar Melati 1 sedang dihuni pasien Laki-laki…" | **Berhasil, tanpa peringatan apa pun di layar** |
| Pasien yang jenis kelaminnya belum tercatat ditempatkan di kamar berpenghuni, pada tempat tidur yang menerima keduanya | Ditolak, dan petugas disuruh mencari kamar yang belum ada penghuninya | **Berhasil, tanpa peringatan apa pun di layar** |
| Pasien yang jenis kelaminnya belum tercatat ditempatkan di tempat tidur khusus satu jenis kelamin | Ditolak | **Tetap ditolak**, dengan kalimat "Jenis kelamin pasien belum tercatat. Pilih tempat tidur yang menerima laki-laki dan perempuan." |
| Tn. Budi ditempatkan di tempat tidur bertanda perempuan saja | Ditolak `BED_GENDER_MISMATCH` | **Tetap ditolak** dengan kalimat yang sama |
| Pasien butuh isolasi ditempatkan di tempat tidur biasa, dan sebaliknya | Ditolak beserta penanda arah isolasinya | **Tetap ditolak** beserta penanda arah isolasinya |

Perlu ditegaskan: perubahan pada layar bersifat **pasif**. Layar berhenti menjelaskan pencampuran
kamar karena servernya berhenti menolak, bukan karena layar menyembunyikan sesuatu. Bila suatu saat
server mengirim kode yang tidak dikenal layar, kalimatnya tetap ditampilkan apa adanya — yang hilang
hanya penanda tambahan, bukan pesannya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola.**

- `AGENTS.md` frontend
- `rules/GLOBAL_RULES.md`
- `rules/frontend/frontend-architecture.md`, `base-component-decision-gate.md`, `ui-consistency-checklist.md`, `test-policy.md`, `REPORT_TEMPLATE.md`
- `rules/rule-output/status-task-roadmap.md`, `lokasi-laporan-task.md`

Pemeriksaan tambahan: folder `agents/rules/` **tidak ada** di working tree frontend, sesuai
pencabutannya. Tidak ada sisa peninggalan yang perlu dilaporkan.

**Kontrak dan keputusan.**

- `contracts/validation-matrix.md` `0.8.0` bagian 3 dan blok perubahan `0.8.0`
- `roadmap/frontend-roadmap.md` bagian `F14`, kartu `FE-RWI-062`
- `task/report/backend/BE-RWI-073.md` bagian 1, 2, dan 2.1

**Source backend, dibaca saja.**

- `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` — blok aturan 2, 4,
  5, 7, dan 8 pada `EvaluatePlacementEligibilityAsync`, ditambah `BedTakenMessage` dan
  `BedGenderMessage`. Dibaca untuk menyalin kalimat penolakan **apa adanya** ke dalam test, bukan
  mengarangnya

**Source frontend.**

- `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx`
- `src/components/features/health-services/inpatient-management/placement-failure-list.jsx`
- `tests/unit/inpatient-placement.test.mjs`
- `tests/unit/inpatient-bed-board.test.mjs`
- `tests/e2e/inpatient-episode-detail.spec.mjs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` | Entri `ROOM_GENDER_MIXED` dihapus dari `PLACEMENT_RULE_CODES`. Ditambah catatan yang menerangkan bahwa aturan 6 dicabut `RWI-DEC-101`, nomornya tidak dipakai ulang, dan nomor 7 serta 8 sengaja tidak bergeser. Catatan itu **sengaja tidak menyebut kodenya secara harfiah** supaya kriteria pertama benar-benar nol hasil pada pencarian `src/` |
| `src/components/features/health-services/inpatient-management/placement-failure-list.jsx` | Komentar yang menerangkan penolakan pencampuran jenis kelamin diarahkan ke contoh yang masih terbit, yaitu kode tempat tidur pada penolakan tempat tidur yang sudah ditempati. **Nol perubahan JSX, props, styling, maupun perilaku** |
| `tests/unit/inpatient-placement.test.mjs` | Ketiga assertion disesuaikan, tidak satu pun dihapus. Rinciannya di bagian 3.4 |
| `tests/e2e/inpatient-episode-detail.spec.mjs` | Skenario penolakan pada jalur perpindahan dialihkan dari aturan 6 ke aturan 4 `BED_GENDER_MISMATCH`, yang masih menolak dengan 422 pada jalur yang sama. Judul test dan dua komentarnya ikut disesuaikan |

### 3.3 Kepatuhan arsitektur frontend

| Hal | Kepatuhan |
| --- | --- |
| Penempatan folder | Tidak ada berkas baru. Keempat berkas yang diubah tetap di folder aslinya |
| Alur dependensi | Tetap `view → component → utils`. Arahnya tidak dibalik dan tidak ada import baru |
| Redux, service, Axios | **Tidak disentuh.** Task ini tidak memanggil API dan tidak mengubah thunk mana pun |
| Base component | `InformationAlert` dan `StatusBadge` tetap dipakai apa adanya. Nol komponen baru, nol rute baru, nol butir menu baru — sesuai baris `Reuse` pada kartu roadmap |
| Design token | Tidak ada nilai visual yang ditulis. Tidak ada stylesheet yang disentuh |
| Arsitektur paralel | Nol. Tidak ada abstraksi, factory, wrapper, atau pola baru |

### 3.4 Cara ketiga assertion disesuaikan, bukan dihapus

Baris `Risk/Blocker` pada kartu roadmap memperingatkan bahwa satu assertion tidak boleh sekadar
dihapus. Peringatan itu diperluas ke ketiganya: setiap contoh `ROOM_GENDER_MIXED` diganti contoh
aturan yang **masih terbit**, sehingga cakupan test tidak menyusut satu pun.

| Assertion | Sebelum | Sesudah | Alasan penggantinya |
| --- | --- | --- | --- |
| "422 menghasilkan daftar aturan yang gagal, bukan satu kalimat" | Daftar berisi aturan 6 `ROOM_GENDER_MIXED` dan aturan 7 `ISOLATION_REQUIRED` | Daftar berisi aturan 5 `PATIENT_GENDER_UNKNOWN` dan aturan 7 `ISOLATION_REQUIRED` | Yang diuji adalah kemampuan menampilkan **dua** aturan sekaligus, bukan aturan tertentu. Aturan 5 masih terbit dan kalimatnya sudah memakai bentuk baru kontrak `0.8.0` |
| "pesan pencampuran kamar ditampilkan apa adanya, termasuk nama kamarnya" | Aturan 6, memeriksa nama kamar `Melati 1` bertahan di kalimatnya | Aturan 2 `BED_OCCUPIED_BY_OTHER`, memeriksa kode tempat tidur `BD-RSMMC-00042` bertahan di kalimatnya | Maksud aslinya adalah membuktikan penanda spesifik milik server **tidak hilang** saat kalimat diteruskan ke layar. Aturan 2 adalah penolakan yang tersisa dan kalimatnya memang membawa kode tempat tidur |
| "dua pesan isolasi yang berlawanan arah tidak tertukar" | `isIsolationFailure({ code: "ROOM_GENDER_MIXED" })` bernilai salah | `isIsolationFailure({ code: "BED_GENDER_MISMATCH" })` bernilai salah | Inilah butir yang diperingatkan kartu roadmap. Pembedaan isolasi tetap butuh **contoh tandingan** berupa kode yang bukan kegagalan isolasi. Aturan 4 masih terbit dan memenuhi peran itu persis |

Satu assertion **ditambahkan** pada test pertama:

```js
assert.doesNotMatch(failure.failures[0].message, /penghuni|kamar/i);
```

Assertion itu menjaga kriteria kedua secara langsung: kalimat `PATIENT_GENDER_UNKNOWN` tidak boleh
lagi menyuruh petugas mencari kamar kosong. Bila suatu saat kalimat lama dihidupkan kembali, test
inilah yang gagal lebih dulu.

Seluruh kalimat penolakan pada test disalin **apa adanya** dari `InpBedOccupancyService.cs`, bukan
diketik ulang dari ingatan:

| Kode | Kalimat | Sumber |
| --- | --- | --- |
| `PATIENT_GENDER_UNKNOWN` | "Jenis kelamin pasien belum tercatat. Pilih tempat tidur yang menerima laki-laki dan perempuan." | `InpBedOccupancyService.cs` blok aturan 5 |
| `BED_OCCUPIED_BY_OTHER` | "Tempat tidur BD-RSMMC-00042 sudah ditempati pasien lain. Silakan pilih tempat tidur lain; isian admisi Anda tetap tersimpan." | `BedTakenMessage` |
| `BED_GENDER_MISMATCH` | "Tempat tidur ini hanya untuk pasien perempuan." | `BedGenderMessage` |

### 3.5 Perubahan di luar daftar `Cakupan`, beserta alasannya

Kartu roadmap menyebut empat butir cakupan. Satu berkas di luar daftar itu ikut diubah, dan
disebutkan apa adanya di sini:

`src/components/features/health-services/inpatient-management/placement-failure-list.jsx` — hanya
blok komentarnya. Komentar itu menerangkan bahwa layar menampilkan nama kamar pada penolakan
pencampuran jenis kelamin. Begitu pemetaannya dicabut, komentar tersebut menjadi **keterangan yang
menyesatkan akibat perubahan ini sendiri**, bukan technical debt yang sudah ada sebelumnya.
Membiarkannya berarti meninggalkan dokumentasi yang justru menjelaskan penolakan yang tidak pernah
terjadi lagi — persis yang diminta task ini dihentikan. Perubahannya nol baris JSX, nol props, nol
styling, dan nol perilaku, sehingga baris `Reuse` kartu roadmap — `PlacementFailureList` dipakai
apa adanya — tetap dihormati.

---

## 4. State yang ditangani di layar

Tidak ada state baru yang dibuat. Keempat state existing tetap berperilaku sama persis, dan
dicantumkan di sini sebagai bukti bahwa tidak ada yang rusak:

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol aksi terkunci selama permintaannya berjalan, dijaga penjaga in-flight `placementInFlight` pada `use-inpatient-bed-board-actions.jsx`. **Tidak berubah** |
| Kosong | Ketika server menolak tanpa mengirim daftar aturan, layar tetap menampilkan kalimat utama dari server dan daftar aturannya tidak dirender sama sekali. Dijaga test "penolakan tanpa daftar aturan tetap menampilkan pesan server". **Tidak berubah** |
| Gagal | Penolakan 422 menampilkan judul "Penempatan ditolak aturan kelayakan" beserta daftar aturannya. Penolakan 409 menampilkan judul "Keadaan tempat tidur sudah berubah", memuat ulang daftar tempat tidur, lalu menambahkan kalimat "Daftar tempat tidur sudah dimuat ulang. Pilih tempat tidur lain; isian yang sudah Anda ketik tetap tersimpan di layar ini." **Tidak berubah** |
| Tanpa hak akses | Dimiliki `AccessDeniedGate` pada layar pemanggil, di luar komponen penolakan ini. **Tidak disentuh** |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini tidak memanggil, menambah, atau mengubah pemanggilan API mana pun.
Perubahannya terbatas pada pemetaan kode penolakan dan berkas test. Endpoint penempatan, pemesanan,
dan perpindahan tetap dipanggil oleh hook existing tanpa perubahan.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Keluaran `eslint . --quiet` kosong |
| `npm run test:unit` | 737 test lulus, 0 gagal, 0 dilewati | `PASS` | Ringkasan runner: `tests 737`, `pass 737`, `fail 0`, `duration_ms 5859` |
| `npm run build` | Build Next.js berhasil sampai `postbuild`; `prepare-standalone` menyalin static dan public assets | `PASS` | Keluaran "Standalone runtime siap dijalankan." |
| `node --check tests/unit/inpatient-placement.test.mjs` | Berkas terurai tanpa galat sintaks | `PASS` | Keluaran perintah |
| `node --check tests/e2e/inpatient-episode-detail.spec.mjs` | Berkas terurai tanpa galat sintaks | `PASS` | Keluaran perintah |
| Kriteria 1 — pencarian `ROOM_GENDER_MIXED` pada `src/` | Nol hasil | `PASS` | `grep -rn "ROOM_GENDER_MIXED" src/` tidak mengembalikan baris |
| Kriteria 2 — pencarian kalimat "di kamar yang belum ada penghuninya" pada `src/` dan `tests/` | Nol hasil | `PASS` | `grep -rni` atas kalimat itu keluar dengan status 1 |
| Grep anti-regresi warna literal pada kedua berkas `src/` yang diubah | Nol hasil | `PASS` | Pencarian `#rrggbb` dan `rgba(` pada kedua berkas |
| Grep anti-regresi typography pada kedua berkas `src/` yang diubah | Nol hasil | `PASS` | Pencarian `font-size`, `font-weight`, `line-height` |
| Grep anti-regresi tombol mentah, tabel mentah, inline style, `!important` | Nol hasil | `PASS` | Pencarian `<button`, `.btn`, `<table`, inline style, `!important` |
| Skenario e2e "penolakan aturan kelayakan lewat jalur perpindahan" di peramban | Tidak dijalankan | `NOT RUN` | Alasannya di bawah |
| Penempatan nyata ke kamar berpenghuni lewat peramban | Tidak dijalankan | `NOT RUN` | Alasannya di bawah |

**Uji manual: `NOT FEASIBLE`.**

Alasannya konkret dan ada dua, keduanya bukan pilihan pelaksana:

1. **Tidak ada `playwright.config.*` di repository.** Pencarian berkas konfigurasi Playwright di
   luar `node_modules` mengembalikan nol hasil, sehingga `npm run test:e2e` tidak punya konfigurasi
   untuk dijalankan. Batas ini sudah tercatat pada `rules/frontend/test-policy.md`, jadi bukan
   temuan baru task ini.
2. **`RWI-UI-GAP-007` masih terbuka.** Environment target belum punya kamar, tempat tidur, dan
   episode yang layak untuk membuktikan penempatan nyata ke kamar berpenghuni. Gerbang itu dicatat
   roadmap sebagai milik Admin Master Data/Tim Master Data.

Baris `Verification` pada kartu roadmap sendiri berbunyi "Bukti peramban mengikuti kebijakan
pemilik yang berlaku saat eksekusi", dan ketiga perintah yang **diwajibkan** kartu itu — unit test
`.mjs` yang disesuaikan, `npm run lint:errors`, dan `npm run build` — seluruhnya dijalankan dan
lulus.

**`AUTOMATED TEST: npm run test:unit — PASS`** (737 lulus, 0 gagal).

**Tidak dijalankan:**

- `npm run test:e2e` — tidak ada `playwright.config.*`; `AGENTS.md` juga melarang menjalankannya
  kecuali environment mendukung atau task mengharuskan, dan kartu ini tidak mengharuskannya.
- `npm run test:uat` — `AGENTS.md` melarang tanpa permintaan eksplisit user.
- `npm run lint:strict` — validasi minimum `AGENTS.md` menetapkan `lint:errors`, dan task tidak
  memperluasnya.

**`UI GATE: N/A`** — gerbang keputusan base component dilewati sesuai
`base-component-decision-gate.md` bagian "Kapan berlaku", yang melewatkan task yang hanya menyentuh
utility, constant, atau dokumentasi. Task ini tidak menulis satu baris JSX maupun CSS: yang berubah
adalah satu constant map, satu blok komentar, dan dua berkas test. Nol komponen berstatus `EXTEND`,
`COMPOSE`, `WRAP`, maupun `NEW`, sehingga tidak ada pilihan bernomor yang perlu diajukan ke user.

---

## 7. Acceptance criteria dan Definition of Done

### 7.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pemetaan `ROOM_GENDER_MIXED` nol hasil pada pencarian `src/` | **Terpenuhi** | Pencarian kode itu pada `src/` nol baris. Entri pada `PLACEMENT_RULE_CODES` dihapus, dan catatan penggantinya sengaja tidak menyebut kodenya secara harfiah supaya pencariannya benar-benar bersih |
| 2. Pesan `PATIENT_GENDER_UNKNOWN` tidak lagi memuat kalimat "di kamar yang belum ada penghuninya" | **Terpenuhi** | Frontend tidak pernah menyusun kalimat itu; ia meneruskan `errors[].message` dari server apa adanya, dan server sudah memakai kalimat `0.8.0` sejak `BE-RWI-073`. Pencarian kalimat lama pada `src/` dan `tests/` nol hasil, dan assertion baru `assert.doesNotMatch` menjaganya tetap begitu |
| 3. Penempatan ke kamar berpenghuni berhasil tanpa peringatan apa pun di layar | **Terpenuhi pada sisi frontend** | Layar tidak punya jalan untuk memperingatkan: ia tidak menghitung kelayakan sendiri, tidak membaca penghuni kamar, dan hanya merender `failures[]` yang dikirim server. Dijaga test existing `inpatient-bed-board.test.mjs`, yang menolak munculnya `isForMale`, `requiresIsolation`, maupun `gender` di utils, hook, dan papan. Sisi server sudah dibuktikan `BE-RWI-073`. **Bukti peramban ujung-ke-ujung tidak dijalankan** — lihat bagian 6 |
| 4. `BED_GENDER_MISMATCH` **tetap** tampil dengan pesan yang benar | **Terpenuhi** | Kodenya tetap ada pada `PLACEMENT_RULE_CODES`, dan kalimatnya tetap dirender apa adanya oleh `PlacementFailureList`. Sekarang justru diuji **dua kali**: sebagai penolakan 422 yang tampil di layar pada skenario e2e perpindahan, dan sebagai contoh tandingan bukan-isolasi pada test unit |
| 5. Pesan isolasi **tetap** tampil | **Terpenuhi** | `ISOLATION_REQUIRED` dan `ISOLATION_BED_RESERVED` tidak disentuh, beserta `ISOLATION_DIRECTION_LABELS` yang memberi penanda arahnya. Test "dua pesan isolasi yang berlawanan arah tidak tertukar" tetap lulus |
| 6. `isIsolationFailure` tetap membedakan kegagalan isolasi dari kegagalan lain | **Terpenuhi** | Fungsinya tidak diubah sama sekali. Contoh tandingannya diganti dari `ROOM_GENDER_MIXED` menjadi `BED_GENDER_MISMATCH`, sehingga pembedaannya tetap teruji terhadap kode yang benar-benar masih terbit — persis yang diminta baris `Risk/Blocker` |

### 7.2 Definition of Done

| Butir | Status | Catatan |
| --- | --- | --- |
| Keenam acceptance criteria terpetakan ke source | **Terpenuhi** | Tabel 7.1; setiap baris menunjuk berkas atau perintah yang dapat ditelusuri |
| Ketiga berkas test disesuaikan, bukan dihapus | **Terpenuhi** | Ketiga assertion pada `tests/unit/inpatient-placement.test.mjs` diganti contoh aturan yang masih terbit, dan skenario `tests/e2e/inpatient-episode-detail.spec.mjs` dialihkan ke aturan 4. Nol test dihapus, nol assertion hilang, satu assertion justru ditambahkan. **Catatan redaksi:** butir DoD berbunyi "ketiga berkas test", sedangkan baris `Cakupan` hanya menyebut **dua** berkas test ditambah satu berkas utility. Keduanya sudah dikerjakan seluruhnya, jadi selisih ini soal penulisan kartu, bukan pekerjaan yang tertinggal |
| Lint dan build lulus | **Terpenuhi** | `npm run lint:errors` bersih; `npm run build` berhasil sampai `postbuild` |
| Dirilis pada gelombang yang sama dengan `BE-RWI-073` | **Belum dapat dinyatakan** | Ini butir **rilis**, bukan butir implementasi, dan tidak dapat dipenuhi dari repository frontend. Perubahan frontend masih lokal dan belum di-commit; `AGENTS.md` melarang commit, push, merge, dan deploy tanpa permintaan eksplisit. Butir ini dipegang pemilik rilis |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE`. `npm run lint:errors` tidak mengeluarkan error maupun peringatan pada keempat berkas yang diubah. Tidak ada `NEW ERROR`, dan tidak ada `EXISTING WARNING` yang tersentuh |
| Masalah yang diketahui | Komentar pada `tests/unit/inpatient-bed-board.test.mjs` baris 194 masih berbunyi "Delapan aturan Kelayakan Penempatan menyangkut jenis kelamin pasien, **penghuni kamar**, dan kebutuhan isolasi". Penghuni kamar tidak lagi termasuk. **Sengaja tidak diubah**: berkas itu di luar `Cakupan` kartu, menguji kepedulian lain — papan tempat tidur tidak menyaring ulang — dan assertion-nya tidak terpengaruh sama sekali. Dilaporkan sebagai technical debt tanpa diperbaiki, sesuai aturan cakupan `AGENTS.md` |
| Dependency backend | `NONE` yang masih terbuka. `BE-RWI-073` ✅ selesai 11 September 2026 dan sudah menghentikan penerbitan kode aturan 6 di server. Komentar XML pada `InpBedOccupancyService.cs` baris 1249 memang masih menyebut kode itu, tetapi itu **catatan sejarah yang disengaja** — ia menerangkan pencabutannya dan alasan nomor 6 dibiarkan kosong, bukan kode aktif. Tidak ada yang perlu dikerjakan backend |
| Perubahan sampingan | `NONE`. Tidak ada berkas yang ter-generate, dipulihkan, atau dihapus di luar keempat berkas yang disengaja. `.next/` adalah hasil `npm run build` dan tidak terlacak Git |
| Interupsi | `NONE`. Pekerjaan berjalan dari awal sampai selesai tanpa terputus |
| Status Git | `git status --short` pada `QuilvianSystemFrontendDev`:<br>`M src/components/features/health-services/inpatient-management/placement-failure-list.jsx`<br>`M src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx`<br>`M tests/e2e/inpatient-episode-detail.spec.mjs`<br>`M tests/unit/inpatient-placement.test.mjs`<br>Tidak ada berkas yang di-stage, di-commit, maupun di-push |
| Langkah berikutnya | Pemilik rilis menggabungkan perubahan frontend ini pada gelombang yang sama dengan `BE-RWI-073`, sesuai butir DoD terakhir yang belum dapat dinyatakan dari sisi frontend |
