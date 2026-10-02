# Laporan Perubahan Frontend — `FE-LAB-15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-15` |
| Judul | Kolom Konfirmasi dan pop-up konfirmasi |
| Slice | `EPIC-LAB-12`, gelombang `MVP-5c` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6d |
| Trace | `FR-11.12`; `LAB-DEC-061`; `AC-94`, `AC-95` |
| Contract version | `LAB-API-v1` **`r12`** §7.1 `POST /lab-orders/{id}/confirm`; ruas tampil dari **`r14`** bagian 9 |
| Dependency | `BE-LAB-31` ✅, `BE-LAB-33` ✅, `BE-LAB-34` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — layar Laboratorium; artefak blueprint pada repository backend |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `4031fd3d7`, branch `YogaV2` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — keempat butir DoD terpenuhi dan **terbukti pada aplikasi yang benar-benar berjalan**, termasuk butir "konfirmasi kedua tidak mungkin dilakukan dari layar" |

---

## 1. Masalah yang diperbaiki

**Keadaan sebelum perubahan.** Petugas laboratorium memeriksa pesanan yang masuk dan memilih dokter
pemeriksa — dan mengerjakannya **di luar sistem**. `BE-LAB-30` sampai `BE-LAB-34` sudah mendirikan
seluruh sisi backendnya: status `Confirmed`, ketiga kolomnya, endpoint konfirmasi, dan ruas
tampilnya. Yang belum ada adalah layar yang memakainya.

**Akibat nyatanya.** Pada ketiga menu pemeriksaan, tidak ada cara membedakan pesanan yang sudah
diperiksa petugas dari yang belum — dan tidak ada cara mengonfirmasinya.

---

## 2. Proses bisnis

### 2.1 Alur normal, berurutan

1. Petugas membuka salah satu dari ketiga menu pemeriksaan — Patologi Klinik, Patologi Anatomi,
   atau Mikrobiologi.
2. Kolom **Konfirmasi** menunjukkan mana yang belum: berbunyi `Belum Terkonfirmasi`.
3. Petugas menekan **Konfirmasi** pada baris itu. Pop-up terbuka berisi **ringkasan pasien** —
   nama, nomor rekam medis, nomor kunjungan, dan pemeriksaannya — beserta **pemilih dokter
   pemeriksa**.
4. Tombol simpan **nonaktif** sampai dokter pemeriksa dipilih.
5. Sesudah disimpan, kolomnya berganti menjadi **nama konfirmator beserta tanggal dan waktu**, dan
   nama dokter pemeriksa di bawahnya. Tombol Konfirmasi baris itu menjadi **nonaktif**.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Pesanan sudah dikonfirmasi | Tombol **nonaktif** beserta alasannya: "Pesanan ini sudah dikonfirmasi. Konfirmasi hanya sah sekali." |
| Status bukan `Requested` — `Accepted`, `InProcess`, `Completed`, `Cancelled`, `OnHold` | Tombol **nonaktif** beserta alasannya |
| Dokter pemeriksa belum dipilih | Simpan tertahan, beserta kalimat "Pilih dokter pemeriksa terlebih dahulu." |
| Backend menolak — `409` atau `422` | Pop-up **tetap terbuka** beserta pesannya, sehingga petugas dapat memperbaiki tanpa mengulang dari awal |

### 2.3 Kenapa tombolnya dinonaktifkan, bukan disembunyikan

Petugas perlu melihat bahwa kolomnya memang ada dan **mengapa** ia tidak dapat ditekan.
Menyembunyikannya membuat baris yang sudah dikonfirmasi tampak berbeda bentuk dari baris lain, dan
petugas mencari tombol yang tidak ada.

### 2.4 Kenapa jejaknya dibaca dari `confirmedAt`, bukan dari statusnya saja

Pesanan yang dikonfirmasi lalu melaju ke `Accepted` **tetap pernah dikonfirmasi**. Membaca dari
status saja akan membuat kolomnya kembali berbunyi `Belum Terkonfirmasi` seolah langkah itu tidak
pernah terjadi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `lib/hooks/.../lab-confirmation-rules.js` | **Baru.** Aturan kolom dan muatan, murni dan dapat diuji |
| `lib/services/.../lab-order.service.js` | `confirmLabOrder` |
| `lib/state/slice/.../lab-monitoring-slice.jsx` | Keadaan konfirmasi **per disiplin**; thunk; pembaruan baris dari jawaban server |
| `lib/hooks/.../use-lab-monitoring.jsx` | Keadaan pop-up, pemilih dokter, dan penyimpanannya |
| `components/view/.../lab-monitoring-table-columns.jsx` | Kolom **Konfirmasi** dan kolom tombolnya |
| `components/view/.../lab-monitoring-view.jsx` | Pop-up konfirmasi |
| `style/.../lab-monitoring.module.css` | Gaya kolom dan pop-up, memakai token yang sudah ada |
| `tests/unit/lab-confirmation-rules.test.mjs` | **Baru.** Sebelas uji |

**Ketiga menu memakai komponen yang sama** — satu susunan kolom, satu hook, satu pop-up. Tidak ada
satu pun berkas per disiplin.

### 3.2 Baris diperbarui dari jawaban server, bukan ditebak layar

Sesudah konfirmasi berhasil, barisnya diperbarui di tempat memakai `orderStatus`, `confirmedAt`,
`confirmedByName`, dan `examinerDoctorName` **dari jawaban server**. Dengan begitu kolomnya
menampilkan nama dan waktu yang **benar-benar tersimpan**, dan tombolnya nonaktif karena statusnya
memang sudah berpindah — bukan karena layar mengingat pernah menekannya.

### 3.3 Pop-up ditutup hanya bila berhasil

Penolakan dibiarkan terbaca **di dalam** pop-up beserta isian yang sudah dipilih petugas. Untuk
`422` — dokter pemeriksa tidak ditemukan atau tidak aktif — ia dapat langsung mengganti pilihannya.

### 3.4 Keadaan konfirmasi disimpan per disiplin

Mengikuti bentuk state daftar dan penyaringnya yang memang sudah per disiplin. Konfirmasi yang
sedang berjalan pada satu menu karena itu tidak menonaktifkan tombol pada menu disiplin lain.

### 3.5 Dampak kontrak API dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol perubahan.** Satu endpoint yang sudah ada mulai dikonsumsi: `POST /lab-orders/{id}/confirm` |
| Backend | **Nol baris backend diubah** pada task ini |
| Keamanan/Auth | Nol perubahan. **Nol kotak isian konfirmator** pada pop-up — namanya datang dari server, dan ruasnya memang tidak ada pada DTO permintaan |

---

## 4. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/laboratory-management/lab-orders/{id}/confirm` | Mengonfirmasi pesanan beserta dokter pemeriksanya | `LabOrder : Update` |

**Muatan yang dikirim** — `examinerDoctorId` saja. Nol ruas konfirmator, nol ruas waktu.

---

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas keenam berkas task ini | **Nol keluaran** | `PASS` | — |
| `npm run build` — build produksi | Berhasil, `Compiled successfully` | `PASS` | — |
| Uji unit baru | **11 dari 11 lolos** | `PASS` | `tests/unit/lab-confirmation-rules.test.mjs` |
| Seluruh uji unit repository | **943 dari 943 lolos** | `PASS` | Nol regresi |
| **Layar** — kolom membedakan yang sudah dan belum dikonfirmasi | Lolos | `PASS` | 5.1 |
| **Layar** — pop-up menahan simpan sampai dokter dipilih | Lolos | `PASS` | 5.1 |

### 5.1 Verifikasi layar terhadap aplikasi yang benar-benar berjalan

Dijalankan terhadap build produksi standalone pada `127.0.0.1:3710`, jawaban server dipalsukan
lewat route Playwright — mengikuti pola `tests/e2e/lab-patient-search-paging.spec.mjs`.

**Yang terbukti terbaca dari DOM aplikasi:**

| Butir | Bukti |
| --- | --- |
| Baris belum dikonfirmasi | `Belum Terkonfirmasi` |
| Baris sudah dikonfirmasi | `Dewi` · `16 Sep 2026, 09.30` · `Dokter pemeriksa: dr. Fajar Rama Ragussa` |
| **Nol label `Terkonfirmasi` tambahan** | Teks tabel diperiksa sesudah `Belum Terkonfirmasi` dibuang — nol kemunculan tersisa |
| **`AC-94` — konfirmasi kedua tidak mungkin** | Dua tombol Konfirmasi: baris `Requested` **aktif**, baris `Confirmed` **nonaktif** |
| Ringkasan pasien pada pop-up | `No. RM RM-0001` tampil |
| **`AC-95` — simpan ditahan** | Tombol `Simpan Konfirmasi` **nonaktif**; kalimat "Pilih dokter pemeriksa terlebih dahulu." tampil |
| **Nol kotak isian konfirmator** | Teks pop-up diperiksa — nol kemunculan kata "konfirmator" |
| Muatan yang dikirim | Tepat `["examinerDoctorId"]`, bernilai penunjuk dokter yang dipilih |
| Sesudah berhasil | Baris diperbarui dari jawaban server; tombolnya menjadi **nonaktif** |

**Satu asersi uji sempat gagal, dan yang salah adalah ujinya.** Regex `/(^|\s)Terkonfirmasi(\s|$)/`
ikut mencocoki kata di dalam **"Belum Terkonfirmasi"** — teks yang memang seharusnya ada.
Diperbaiki dengan membuang teks bawaan itu lebih dulu sebelum diperiksa. Produknya benar sejak
awal; asersinya yang terlalu longgar.

### 5.2 Alat verifikasinya

Konfigurasi dan spesifikasi Playwright **sementara**, dihapus sesudah selesai. Repository frontend
belum memiliki `playwright.config`, sehingga `npm run test:e2e` tidak dapat dijalankan apa adanya;
menambahkannya adalah pekerjaan infrastruktur yang bukan cakupan task ini. **Nol berkas uji layar
tertinggal di repository**, dan `test-results/` ikut dibersihkan.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kolom menampilkan **nama dan waktu** sesudah konfirmasi | **Terpenuhi** | Terbaca dari DOM: `Dewi` · `16 Sep 2026, 09.30` — 5.1 |
| Tombol **nonaktif** sesudahnya | **Terpenuhi** | Baris `Confirmed` nonaktif; dan nonaktif lagi sesudah konfirmasi berhasil — 5.1 |
| Pemilih dokter **wajib terisi** sebelum simpan | **Terpenuhi** | Tombol simpan nonaktif; uji unit `S11` menutup lima bentuk nilai kosong |
| **Ketiga menu memakai komponen yang sama** | **Terpenuhi** | Satu susunan kolom, satu hook, satu pop-up; nol berkas per disiplin |

### 6.2 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-94` — konfirmasi hanya sah **sekali**; nama konfirmator dan waktu **terbaca pada daftar** | **Terpenuhi** | Kolomnya terbaca dari DOM; tombol baris terkonfirmasi nonaktif — 5.1. Aturan "sekali" juga ditegakkan backend (`VAL-70`), sehingga layar bukan satu-satunya penjaga |
| `AC-95` — konfirmasi **menolak** bila dokter belum dipilih; dokter yang dipilih **tampil pada daftar** | **Terpenuhi untuk bagian daftar** | Simpan tertahan di layar dan `422` di backend; `Dokter pemeriksa: dr. Fajar Rama Ragussa` terbaca pada kolomnya. **Bagian "ringkasan cetak" bukan cakupan task ini** — ia milik `FE-LAB-17`, yang masih ⛔ tertahan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning dari berkas task ini |
| Masalah yang diketahui | **Satu, dan ia milik `AC-95`:** bagian "tampil pada **ringkasan cetak**" tidak dapat ditutup task ini karena fitur cetaknya belum ada — `FE-LAB-17` masih ⛔ tertahan oleh penahan yang tidak tersentuh `r13` maupun `r14` |
| Risiko tersisa | **Pertama**, daftar yang sudah terbuka beberapa menit dapat basi: pesanan yang dikonfirmasi petugas lain akan tetap menampilkan tombol aktif sampai daftarnya dimuat ulang. Menekannya **tidak berbahaya** — backend menolak `409` dan pesannya terbaca di pop-up — tetapi petugas perlu memuat ulang untuk melihat keadaan sebenarnya. **Kedua**, pemilih dokter menarik seluruh dokter aktif tanpa penyaring disiplin; `VAL-73` hanya menuntut dokternya ada dan aktif, sehingga ini sesuai kontrak — tetapi pada rumah sakit dengan ratusan dokter, pencarian namanya menjadi satu-satunya cara yang nyaman. **Ketiga**, verifikasi layar memakai jawaban server yang dipalsukan; jalur ke backend sungguhan belum pernah dilewati dari layar ini |
| Perubahan sampingan | `NONE`. Folder harness sementara dan `test-results/` dihapus |
| Interupsi | `NONE` |
| Langkah berikutnya | **1.** `FE-LAB-16` — pop-up pembatalan beralasan; nol penahan, label tombolnya sudah ditetapkan `Konfirmasi Pembatalan`. **2.** `FE-LAB-13` — layar kiosk; penahannya terangkat, dan ia membuat panel kiosk `FE-LAB-14` benar-benar terisi. **3.** `FE-LAB-17` tetap ⛔ |

### 7.1 Status Git

Berkas milik task ini pada `QuilvianSystemFrontendDev`, branch `YogaV2`:

```text
 M src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-table-columns.jsx
 M src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-monitoring.jsx
 M src/lib/services/health-services/laboratory-management/lab-order.service.js
 M src/lib/state/slice/health-services/laboratory-management/lab-monitoring-slice.jsx
 M src/style/health-services/laboratory-management/lab-monitoring/lab-monitoring.module.css
?? src/lib/hooks/health-services/laboratory-management/lab-confirmation-rules.js
?? tests/unit/lab-confirmation-rules.test.mjs
```

Berkas lain yang berstatus berubah pada repository itu milik `FE-LAB-14`, dan tidak disentuh.

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.
