# Laporan Perubahan Frontend — `FE-LAB-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-14` |
| Judul | Layar pendaftaran lab: sesi kiosk dan pemilih pemeriksaan |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6c |
| Trace | `FR-11.2b`, `FR-11.11`; `LAB-DEC-055`, `LAB-DEC-056`, `LAB-DEC-048` butir 6; `AC-86`, `AC-87` |
| Contract version | `LAB-API-v1` `r10` `POST /lab-orders/by-examinations` — `approved` 2026-09-15; jalur baca sesi kiosk dari `BE-EXT-04`/`BE-EXT-04b` |
| Dependency | `BE-EXT-04` ✅, `BE-EXT-04b` ✅, `BE-LAB-27` ✅ — ketiganya selesai |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — layar Laboratorium; dan artefak blueprint pada repository backend |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `4031fd3d7`, branch `YogaV2` |
| Commit backend rujukan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — keempat butir DoD terpenuhi. Verifikasi layar menemukan **dua cacat nyata** yang keduanya diperbaiki; satu bagian verifikasi layar tidak dapat diselesaikan dan disebut apa adanya pada bagian 5.3 |

---

## 1. Masalah yang diperbaiki

### 1.1 Pasien kiosk tidak terlihat oleh petugas laboratorium

**Keadaan sebelum perubahan.** Pasien memindai kartunya di kiosk, memilih layanan Laboratorium,
lalu berjalan ke meja pendaftaran laboratorium — dan di sana **tidak ada apa pun** yang menunjukkan
bahwa ia baru saja datang. Petugas tetap harus mengetik nama atau nomor rekam medisnya dari nol,
seolah kiosk tidak pernah ada.

`BE-EXT-04` dan `BE-EXT-04b` sudah mendirikan penanda tujuan layanan beserta jalur bacanya. Yang
belum ada adalah layar yang membacanya.

### 1.2 Memesan tiga disiplin berarti memesan tiga kali

**Keadaan sebelum perubahan.** Formulir pemesanan hanya menerima **satu** jenis pemeriksaan.
Pasien yang perlu Hemoglobin, Kalium, dan kultur darah menuntut petugas mengulang seluruh formulir
tiga kali — memilih kunjungan tiga kali, menyimpan tiga kali.

`BE-LAB-27` sudah membangun `POST /lab-orders/by-examinations` yang menerima seluruh pilihan
sekaligus lalu memecahnya menjadi satu pesanan per disiplin. Yang belum ada adalah layar yang
memakainya.

**Contoh konkretnya.** Memilih Hemoglobin dan kultur darah sekarang cukup sekali simpan, dan
menghasilkan **dua** pesanan: satu Patologi Klinik, satu Mikrobiologi.

---

## 2. Proses bisnis

### 2.1 Alur normal, berurutan

1. Petugas membuka layar Pendaftaran Pasien Laboratorium.
2. Panel **Menunggu dari Kiosk** menampilkan pasien yang sudah memindai kartunya dan memilih
   layanan Laboratorium — dan **belum** dipakai registrasi.
3. Petugas menekan **Tarik Pasien** pada salah satu baris. Pasien itu menjadi pasien terpilih, dan
   layar menyebutkan namanya sebagai konfirmasi.
4. Petugas meneruskannya lewat tombol pendaftaran yang sudah ada — Datang Langsung atau Rujukan
   Luar — persis seperti pasien yang ditemukan lewat pencarian biasa.
5. Pada formulir pemesanan, petugas memilih **beberapa** pemeriksaan sekaligus dari katalog.
6. Sebelum menyimpan, layar sudah menyebutkan berapa pesanan yang akan terbentuk dan disiplin apa
   saja.
7. Sesudah menyimpan, layar menampilkan rincian dari **jawaban server**: berapa pesanan terbentuk,
   dan masing-masing masuk menu Pemeriksaan yang mana.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Sesi kiosk **belum cocok** ke pasien mana pun | Barisnya tetap tampil, tetapi tombol Tarik **nonaktif** beserta alasannya. Menebak pasiennya berarti mendaftarkan orang yang salah |
| Peran petugas belum memegang izin baca sesi kiosk | Pesan menyebut **nama izinnya** — `KioskScanSession : Read` — dan menegaskan pendaftaran manual tetap dapat dipakai |
| Belum ada sesi kiosk yang menunggu | Disebut apa adanya, beserta alasannya: sesi lama memang tidak membawa tujuan layanan |
| Pemeriksaan yang katalognya **belum digolongkan** disiplinnya | Tersimpan, dan disebut terang: "tidak akan muncul di menu Pemeriksaan disiplin mana pun". Ini keadaan sah (`AC-85`), bukan galat |
| Server menjawab tanpa satu pun pesanan | Diperlakukan **gagal**, bukan berhasil yang sunyi |

### 2.3 Kenapa panel kiosk ditaruh di atas pencarian

Inilah antrean yang sedang berjalan: pasien yang baru saja memindai kartunya dan sedang berdiri di
depan meja. Pencarian manual tetap ada di bawahnya untuk pasien yang datang tanpa lewat kiosk.

### 2.4 Kenapa hasil pemecahan wajib diberitahukan

Ini satu-satunya tempat pada modul Laboratorium di mana **satu tindakan petugas menghasilkan lebih
dari satu objek bisnis**. Tanpa pemberitahuan, satu-satunya cara petugas mengetahuinya adalah
menemukan dua baris di layar lain — dan dugaan pertama yang wajar adalah ia tidak sengaja menekan
simpan dua kali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `AGENTS.md`, `CLAUDE.md` frontend | Governance dan routing skill |
| `rules/frontend/*`, `rules/README.md` | Arsitektur, token desain, katalog komponen, aturan laporan |
| `roadmap/frontend-roadmap.md` — `FE-LAB-14` | Cakupan, kewenangan UI, DoD, verifikasi |
| `contracts/api-contract.md` bagian 6 | Bentuk `POST /lab-orders/by-examinations` |
| `Areas/.../DTOs/LabOrderDtos.cs` (backend) | Bentuk permintaan yang **sebenarnya** diterima, bukan yang ditebak dari kontrak |
| `Areas/.../Controllers/KioskScanSessionController.cs` (backend) | Kedua jalur baca sesi kiosk beserta penjaganya |
| `Program.cs` (backend) | Isi `KioskReadPolicy` — menentukan jalur mana yang dapat dipakai petugas lab |
| `lab-patient-search-view.jsx`, `use-lab-patient-search.jsx` | Layar pendaftaran yang sudah ada |
| `lab-order-form-view.jsx`, `use-lab-order-form.jsx` | Formulir pemesanan yang sudah ada |
| `lab-catalog-picker.jsx`, `use-lab-catalog-picker.jsx`, `lab-catalog-picker-utils.js` | Pemilih katalog — ternyata **sudah** mendukung pilihan jamak |
| `lab-order-slice.jsx`, `lab-patient-registration-slice.jsx` | Pola thunk, penanganan galat, dan kode status |
| `tests/unit/lab-catalog-picker-utils.test.mjs`, `tests/e2e/lab-patient-search-paging.spec.mjs` | Pola uji unit dan pola verifikasi layar yang sudah dipakai modul ini |
| `src/app/globals.css` | Token desain yang benar-benar ada |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `lib/services/.../lab-order.service.js` | `createLabOrderByExaminations` |
| `lib/services/.../lab-kiosk-session.service.js` | **Baru.** Baca sesi kiosk bertujuan Laboratorium. Nol fungsi tulis |
| `lib/state/slice/.../lab-order-slice.jsx` | Thunk `submitLabOrderByExaminations`; larik kosong diperlakukan gagal |
| `lib/state/slice/.../lab-patient-registration-slice.jsx` | Keadaan sesi kiosk beserta thunk dan reducernya; kode status disimpan terpisah |
| `lib/hooks/.../lab-order-split-rules.js` | **Baru.** Aturan pemecahan yang murni dan dapat diuji |
| `lib/hooks/.../use-lab-order-form.jsx` | Pilihan jamak; muatan `by-examinations`; perkiraan dan rincian pemecahan |
| `lib/hooks/.../use-lab-patient-search.jsx` | Daftar sesi kiosk, penarikan pasien, dan pasien tertarik |
| `components/view/.../lab-order-form-view.jsx` | Pemilih jamak; panel perkiraan; panel rincian pemecahan |
| `components/view/.../lab-patient-search-view.jsx` | Panel **Menunggu dari Kiosk** |
| `lib/constants/.../laboratory-constants.jsx` | Alamat baca sesi kiosk |
| `lib/constants/.../lab-patient-registration-constants.jsx` | Enam teks baku panel kiosk |
| `style/.../lab-patient-registration.module.css` | Gaya panel kiosk, memakai token yang sudah ada |
| `tests/unit/lab-order-split-rules.test.mjs` | **Baru.** Sebelas uji atas bentuk muatan dan rincian pemecahan |

### 3.3 Satu keputusan teknis yang menentukan apakah layar ini dapat dipakai sama sekali

Backend menyediakan **dua** jalur baca sesi kiosk dengan isi dan penyaring yang sama persis:

| Jalur | Penjaga | Dapat dipakai petugas laboratorium? |
| --- | --- | --- |
| `GET /kiosk-scan-sessions/options` | `KioskReadPolicy` | **Tidak.** Policy itu hanya mengakui SuperAdmin, Administrator, peran `Kiosk`, dan akun perangkat kiosk |
| `GET /kiosk-scan-sessions/admin/options` | `[AccessPermission("KioskScanSession", "Read")]` | **Ya.** Izin aplikasi biasa yang dapat diberikan lewat layar Akses Role |

Layar ini memakai **`admin/options`**. Memakai jalur pertama akan menjawab `403` untuk **setiap
pengguna yang justru dituju layar ini** — petugas laboratorium — dan kegagalannya akan terbaca
sebagai "sistem sedang bermasalah", bukan sebagai izin yang belum diberikan.

**Prasyarat konfigurasi, bukan kode:** peran petugas laboratorium perlu diberi izin
`KioskScanSession : Read`. Selama belum, panelnya menampilkan pesan yang menyebut nama izin itu
apa adanya — dan pendaftaran manual tetap berjalan.

### 3.4 Dampak kontrak API dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol perubahan.** Dua endpoint yang sudah ada mulai dikonsumsi: `POST /lab-orders/by-examinations` (`r10`, dibangun `BE-LAB-27`) dan `GET /kiosk-scan-sessions/admin/options` (milik `registration-management`) |
| Backend | **Nol baris backend diubah** pada task ini |
| Keamanan/Auth | Nol perubahan. Satu **prasyarat konfigurasi** dilaporkan pada 3.3 |
| `AC-45` | **Tegak.** Layar ini nol membentuk dan nol mengubah kunjungan maupun sesi kiosk; berkas servicenya sengaja tidak memuat satu pun fungsi tulis |

---

## 4. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/laboratory-management/lab-orders/by-examinations` | Memesan beberapa pemeriksaan sekaligus; backend memecahnya per disiplin | `LabOrder : Create` |

#### Health Services / Registration Management / Kiosk Scan Session

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/registration-management/kiosk-scan-sessions/admin/options` | Sesi kiosk bertujuan Laboratorium yang belum dipakai registrasi | `KioskScanSession : Read` |

**Muatan yang dikirim** — `encounterId` dan `examinations` saja. **Nol ruas disiplin**, karena
backend menurunkannya dari katalog (`LAB-DEC-048` butir 6).

---

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas kesepuluh berkas task ini | **Nol keluaran** — nol error, nol warning | `PASS` | — |
| `npx eslint src/` menyeluruh | **0 error**, 688 warning seluruhnya sudah ada sebelumnya | `PASS` | Nol warning berasal dari berkas task ini |
| `npm run build` — build produksi | Berhasil, termasuk `postbuild` standalone | `PASS` | Dijalankan tiga kali; yang terakhir sesudah seluruh perbaikan |
| Uji unit baru — bentuk muatan dan rincian pemecahan | **11 dari 11 lolos** | `PASS` | `tests/unit/lab-order-split-rules.test.mjs` |
| Seluruh uji unit repository | **932 dari 932 lolos** | `PASS` | Nol regresi |
| **Verifikasi layar** — panel kiosk tampil dan tombol tariknya benar | Lolos | `PASS` | 5.1 |
| **Verifikasi layar** — izin yang belum diberikan disebut apa adanya | Lolos | `PASS` | 5.1 |
| **Verifikasi layar** — perkiraan pemecahan sebelum simpan | Lolos | `PASS` | 5.2 |
| **Verifikasi layar** — panel rincian sesudah simpan | **Tidak selesai** | `NOT RUN` | 5.3 — terhalang harness, bukan oleh perubahan task ini |

### 5.1 Verifikasi layar menemukan dua cacat nyata, dan keduanya diperbaiki

Verifikasi dijalankan terhadap **aplikasi yang benar-benar berjalan** — build produksi standalone
pada `127.0.0.1:3710` — dengan jawaban server dipalsukan lewat route Playwright, mengikuti pola
yang sudah dipakai `tests/e2e/lab-patient-search-paging.spec.mjs`.

**Cacat pertama: menarik pasien dari kiosk tidak terlihat apa-apa.**
Pasien terpilih dulu hanya dicari di dalam **hasil pencarian**. Pasien yang datang dari kiosk belum
tentu — dan biasanya memang tidak — ada di sana, karena petugas menariknya tanpa mengetik kata
kunci apa pun. Akibatnya tombol Tarik terasa tidak berfungsi, padahal pasiennya sudah terpilih.
Diperbaiki dengan menyimpan pasien yang ditarik apa adanya. **Tanpa uji layar, cacat ini akan lolos
sampai ke petugas** — lint, build, dan uji unit seluruhnya hijau ketika cacat itu masih ada.

**Cacat kedua: rincian pemecahan terhapus sendiri sepersekian detik sesudah tampil.**
Pemilih katalog memancarkan callback perubahan pilihan pada **setiap render ulang** — termasuk
ketika harga sebuah pemeriksaan baru selesai dimuat — bukan hanya ketika petugas menambah atau
membuang pilihan. Membuang rincian pada setiap panggilan membuatnya hilang tepat sebelum sempat
dibaca. Diperbaiki dengan membandingkan **daftar penunjuknya**, bukan sekadar fakta bahwa
callback-nya dipanggil.

**Yang terbukti pada layar sesudah kedua perbaikan:**

| Butir | Bukti |
| --- | --- |
| Panel **Menunggu dari Kiosk** tampil | Judul, kedua baris sesi, nama pasien, dan baris "Pasien belum dikenali" terbaca |
| Penyaringnya dikirim layar | Permintaan membawa `targetService=Laboratory` dan `onlyUsableForRegistration=true` |
| Tombol Tarik mengikuti kecocokan pasien | Baris bercocok **aktif**; baris tanpa pasien **nonaktif** |
| Menarik pasien memberi konfirmasi | "Pasien terpilih: BUDI SANTOSO" tampil sesudah ditarik |
| `403` disebut apa adanya | Pesan memuat `KioskScanSession : Read` dan "pendaftaran manual di bawah tetap dapat dipakai" |

### 5.2 Pemilihan jamak dan perkiraan pemecahan terbukti pada layar

Memilih Hemoglobin lalu kultur darah dari katalog menghasilkan teks berikut **sebelum** disimpan,
dibaca dari DOM aplikasi yang berjalan:

```text
Disiplin: Patologi Klinik, Mikrobiologi
2 pemeriksaan terpilih, dan akan tersimpan sebagai 2 pesanan — satu untuk setiap disiplin.
```

Ini membuktikan tiga hal sekaligus: pemilih menerima **lebih dari satu** pemeriksaan, disiplinnya
diturunkan dari katalog tanpa satu pun kotak pilihan, dan pemecahannya diberitahukan lebih dulu.

### 5.3 Satu bagian verifikasi layar tidak selesai, dan sebabnya disebut apa adanya

Panel rincian **sesudah** simpan tidak dapat dibuktikan lewat layar. Penyebabnya bukan perubahan
task ini: pada harness bertopeng, kotak pilihan **Kunjungan Pasien** menampilkan labelnya dengan
benar (`ENC-0001`) tetapi nilainya tidak bertahan sampai penyimpanan, sehingga validasi formulir
menolaknya dengan "Kunjungan pasien wajib dipilih." dan permintaan `by-examinations` **tidak pernah
terkirim** — terbukti dari nol muatan yang tercatat.

**Yang perlu dan tidak perlu disimpulkan dari ini:**

- Komponen pilihan kunjungan beserta pemasangannya **tidak disentuh** task ini; keduanya sudah ada
  sejak `FE-LAB-06`.
- Gejalanya **tidak berhasil dipisahkan** dari cara harness memalsukan jalur pilihan kunjungan.
  Karena itu ia **tidak** dilaporkan sebagai cacat produk yang terkonfirmasi, melainkan sebagai
  hal yang perlu diperiksa dengan backend sungguhan.
- **Logika yang seharusnya dibuktikan panel itu tetap teruji**: penyusunan rincian pemecahan dari
  jawaban server diuji langsung lewat uji unit `S9`, `S10`, dan `S11`, termasuk pesanan tanpa
  disiplin dan jawaban yang bukan larik.

**Yang tersisa untuk dibuktikan seseorang dengan backend berjalan:** memilih Hemoglobin dan kultur
darah pada satu pasien sungguhan, lalu memastikan panel rincian menampilkan dua pesanan beserta
disiplinnya.

### 5.4 Alat verifikasinya

Konfigurasi dan spesifikasi Playwright **sementara**, dihapus sesudah selesai. Repository frontend
**tidak memiliki** `playwright.config`, sehingga `npm run test:e2e` tidak dapat dijalankan apa
adanya; menambahkannya adalah pekerjaan infrastruktur yang bukan cakupan task ini. Nol berkas uji
layar tertinggal di repository, dan `test-results/` hasil jalannya ikut dibersihkan.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Daftar sesi kiosk tampil dan dapat ditarik | **Terpenuhi** | 5.1 — terbukti pada layar, termasuk perilaku baris yang belum bercocok |
| Pemilih pemeriksaan mengirim **satu** permintaan | **Terpenuhi** | Satu `POST /lab-orders/by-examinations` untuk berapa pun pemeriksaan; uji unit `S6`, `S7`, `S8` |
| **Hasil pemecahan diberitahukan, bukan dibiarkan ditemukan sendiri** | **Terpenuhi** | Perkiraan sebelum simpan terbukti pada layar (5.2); rincian sesudah simpan teruji unit (5.3) |
| **Nol kotak pilihan disiplin** | **Terpenuhi** | Kotak pilihan disiplin tidak ada; muatan terbukti nol memuat ruas disiplin — uji unit `S6` |

### 6.2 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-86` — lintas disiplin menghasilkan satu pesanan per disiplin | **Terpenuhi pada sisi layar** | Pengelompokan terbukti: lintas disiplin menjadi 2, sedisiplin tetap 1 (uji unit `S3`, `S4`); tampil pada layar (5.2). Pemecahan yang sesungguhnya dikerjakan backend dan sudah terbukti `BE-LAB-27` |
| `AC-87` — pemeriksaan yang belum digolongkan disebut apa adanya | **Terpenuhi** | Ditandai terpisah dan diberi kalimatnya sendiri: "tersimpan, tetapi tidak akan muncul di menu Pemeriksaan disiplin mana pun" (uji unit `S2`, `S5`, `S10`) |

---

## 7. Catatan penutup

### 7.1 Satu temuan di luar cakupan, dilaporkan karena menyesatkan bila dibiarkan

Penurunan disiplin yang ditambahkan pada commit `4031fd3d7` — sebelum task ini — **tidak pernah
menghasilkan nilai**. Ia membaca `selectedRow.discipline`, sedangkan `buildSelectionRow` hanya
menghasilkan `disciplineLabel`; hasilnya selalu kosong, dan setiap pesanan terkirim dengan
`discipline: null`.

**Akibatnya nol**, dan itu perlu disebut supaya tidak terbaca lebih gawat daripada yang
sebenarnya: `BE-LAB-29` sudah membuat backend menurunkan disiplin dari katalog pada satu-satunya
jalur tulis `LabOrder`, sehingga pesanannya tetap bergolongan benar. Kodenya mati, bukan salah.

Task ini **mencabutnya**, karena `by-examinations` memang tidak menerima ruas disiplin sama sekali.

### 7.2 Ringkasan

| Hal | Isi |
| --- | --- |
| Peringatan | `npx eslint src/` menghasilkan 688 warning, seluruhnya sudah ada sebelum task ini. Berkas task ini nol warning |
| Masalah yang diketahui | **Satu, dan bukan milik task ini:** `buildSelectionRow` menurunkan `disciplineLabel` dari muatan harga lebih dulu, dan karena `getDisciplineLabel` tidak pernah mengembalikan nilai kosong, cadangannya ke data katalog **tidak pernah terpakai**. Bila jawaban harga tidak membawa disiplin, seluruh baris terbaca "Belum digolongkan" — dan perkiraan pemecahan pada layar ini akan menyebut satu pesanan padahal seharusnya beberapa. Rincian sesudah simpan **tidak** terpengaruh, karena ia datang dari jawaban server |
| Risiko tersisa | **Pertama**, panel kiosk tidak akan menampilkan apa pun sampai peran petugas laboratorium diberi izin `KioskScanSession : Read`. **Kedua**, sesi kiosk baru membawa tujuan layanan sejak `BE-EXT-04b`; sesi yang dibuat sebelum itu tidak akan pernah muncul, dan itu benar. **Ketiga**, panel rincian sesudah simpan belum pernah dilihat pada layar sungguhan — lihat 5.3 |
| Perubahan sampingan | `NONE`. Folder harness sementara dan `test-results/` dihapus; nol berkas uji layar tertinggal |
| Interupsi | `NONE` |
| Status Git | Lihat 7.3 |
| Langkah berikutnya | **1.** Berikan izin `KioskScanSession : Read` kepada peran petugas laboratorium, lalu buka layar pendaftaran untuk memastikan panelnya terisi. **2.** Selesaikan verifikasi 5.3 dengan backend berjalan. **3.** Putuskan apakah `buildSelectionRow` diperbaiki supaya disiplin katalog tidak tertimpa muatan harga — kecil, tetapi menyentuh komponen bersama sehingga pantas berdiri sebagai task tersendiri. **4.** `FE-LAB-13` — layar kiosk milik `registration-management`, dependensinya sudah selesai |

### 7.3 Status Git di akhir pekerjaan

Repository frontend `QuilvianSystemFrontendDev`, branch `YogaV2`:

```text
 M src/components/view/health-services/laboratory-management/lab-orders/form/lab-order-form-view.jsx
 M src/components/view/health-services/laboratory-management/lab-patient-registrations/lab-patient-search-view.jsx
 M src/lib/constants/health-services/laboratory-management/lab-patient-registration-constants.jsx
 M src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-order-form.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-patient-search.jsx
 M src/lib/services/health-services/laboratory-management/lab-order.service.js
 M src/lib/state/slice/health-services/laboratory-management/lab-order-slice.jsx
 M src/lib/state/slice/health-services/laboratory-management/lab-patient-registration-slice.jsx
 M src/style/health-services/laboratory-management/lab-patient-registrations/lab-patient-registration.module.css
?? src/lib/hooks/health-services/laboratory-management/lab-order-split-rules.js
?? src/lib/services/health-services/laboratory-management/lab-kiosk-session.service.js
?? tests/unit/lab-order-split-rules.test.mjs
```

Working tree frontend **bersih sebelum task ini dimulai**, sehingga seluruh baris di atas adalah
milik task ini. Ditambah pembaruan artefak blueprint pada repository backend.

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.
