# Laporan Perubahan Frontend — `FE-LAB-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-17` |
| Judul | Print membuka preview lebih dulu |
| Slice | `MVP-5c`, `EPIC-LAB-12` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6d |
| Trace | `REC2-NEW-006`, `LAB-DEC-061`, `BR-47`, `BR-48`; ketiga ketetapan pemilik modul 2026-09-16 |
| Contract version | `LAB-API-v1` **`r15`** bagian 10 — `approved` 2026-09-16, dilaksanakan `BE-LAB-35` ✅ |
| Wewenang UI | Tombol Print **membuka preview**, tidak langsung mencetak; pencetakan dilakukan dari preview; tanda tangan pembuat order, konfirmator, dan dokter pemeriksa tetap tampil |
| Dependency | `BE-LAB-30`..`BE-LAB-35` ✅. **Nol penahan** |
| Klasifikasi | `MEDIUM` — 3 berkas baru, 3 berkas diubah, satu dokumen cetak baru, nol arsitektur baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; repository backend hanya untuk laporan ini beserta tautan buktinya |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `4031fd3d7` pada branch `YogaV2` (perubahan task ini belum di-stage maupun di-commit) |
| Commit backend yang dijadikan rujukan | `e2152709` pada branch `yoga` (dibaca saja; perubahan `BE-LAB-35` ada di working tree dan belum di-commit) |
| Tanggal | 2026-09-16 |
| Status | **`SELESAI`** — ketiga butir DoD terpenuhi dan terbukti pada aplikasi yang benar-benar berjalan |

---

## 1. Keadaan yang ditemukan di awal

Task ini pernah ⛔ `TERTAHAN` pada pagi hari yang sama, dengan **tiga** penahan. Ketiganya sudah
tertutup sebelum satu baris pun ditulis:

| Penahan | Keadaan saat implementasi dimulai |
| --- | --- |
| (a) Tombol Print tidak pernah ada — 0 kemunculan `print`/`cetak` di seluruh modul | **Cakupan, bukan penahan.** `react-to-print` sudah dipakai resep, signa obat, surat pengantar dokter, kartu pasien kiosk, dan dokumen kasir. Mekanismenya tidak perlu dibangun |
| (b) Dua dari tiga tanda tangan tidak dapat diisi | **Ditutup** `r13`/`BE-LAB-33` lalu `r14`/`BE-LAB-34` |
| (c) Isi pesanan tidak dapat dibaca siapa pun di luar backend | **Ditutup** `r15`/`BE-LAB-35` pada hari yang sama ia ditemukan |

Ketiga keputusan pemilik modul juga sudah ditetapkan 2026-09-16 — **ditanyakan, bukan dikarang**,
sesuai larangan yang tertulis pada kartu tasknya sendiri.

### 1.1 Satu keadaan data yang menentukan bentuk pengujian

`BE-LAB-35` mencatat `LabOrderedProcedure` berisi **0 baris** pada database. Artinya **seluruh
pesanan nyata hari ini menempuh jalur "daftar terpesan kosong"** — jalur yang paling mungkin
dilihat petugas, dan karena itu diuji lebih teliti daripada jalur jamaknya.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya petugas laboratorium**, pada ketiga menu pemeriksaan: Patologi Klinik, Patologi
Anatomi, dan Mikrobiologi.

1. Petugas menekan **Cetak** pada baris pesanan yang dituju. Tombolnya berdiri di kolom aksi,
   bersebelahan dengan Konfirmasi dan Batalkan.
2. **Jendela pratinjau terbuka. Kertas belum keluar** — inilah butir DoD task ini.
3. Selagi rincian pesanan dimuat, jendela menampilkan `Memuat rincian pesanan...` dan tombol
   **Cetak** di dalamnya **nonaktif**.
4. Sesudah rincian tiba, dokumen tampil utuh: identitas pasien, nomor kunjungan, status,
   waktu diminta dan waktu konfirmasi, daftar pemeriksaan yang dipesan, lalu tiga blok tanda
   tangan berjajar.
5. Petugas menekan **Cetak** di dalam jendela. Barulah kertas keluar.
6. Menekan **Tutup** menutup jendela **tanpa mencetak apa pun**.

### 2.1 Isi dokumennya, dan satu aturan yang menentukan benar-salahnya

Daftar pemeriksaan mengikuti aturan `LAB-API-v1` `r15` bagian 10.7, dan aturan itu ditegakkan di
**satu tempat** — `lab-print-rules.js`:

> Bila `orderedProcedures` **terisi**, itulah isi pesanan, dan `procedureName` **tidak** dicetak
> sebagai isi. Bila **kosong**, pesanan itu berpemeriksaan tunggal lewat jalur lama
> `POST /lab-orders`, sehingga `procedureName` **adalah** isi lengkapnya.

Contoh konkret. Pesanan Hemoglobin + Kalium + Natrium yang digabung `BR-47` mencetak **ketiganya**
beserta kesegeraan masing-masing — bukan hanya `Hemoglobin` yang kebetulan menjadi wakilnya.
Tanpa aturan ini, dokumen resminya akan menyebut satu pemeriksaan dan diam soal dua lainnya.

Baris yang **dibatalkan ikut dicetak** beserta keterangan `Dibatalkan`. Menyaringnya berarti
dokumen tidak dapat membedakan pemeriksaan yang tidak pernah dipesan dari yang dipesan lalu
dibatalkan.

### 2.2 Blok tanda tangan

Tiga kolom sejajar di kaki halaman: **Pembuat Order**, **Konfirmator**, **Dokter Pemeriksa**.
Masing-masing memuat nama tercetak, ruang tanda tangan, dan garisnya.

**Ketiganya selalu dicetak, termasuk yang namanya masih kosong** — ketetapan pemilik modul. Garis
kosong itu sendiri adalah jejak bahwa pesanannya belum dikonfirmasi; menghilangkan bloknya justru
menghapus informasi itu dari dokumen.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Rincian pesanan gagal dimuat | Pratinjau menampilkan `InformationAlert` merah berisi pesannya, dan tombol Cetak **tetap nonaktif**. Dokumen setengah terisi tidak pernah dapat dicetak |
| Pesanan tanpa baris terpesan | **Keadaan sah, bukan galat.** `procedureName` dicetak sebagai isi lengkap, disertai keterangan bahwa pesanan itu berpemeriksaan tunggal |
| Pesanan tanpa nama pemeriksaan sama sekali | Dokumen menampilkan `Pesanan ini tidak memuat rincian pemeriksaan.` — bukan tabel kosong tanpa penjelasan |
| Petugas menutup pratinjau lalu menekan Cetak pada baris lain sebelum permintaan pertama selesai | Jawaban yang datang terlambat **dibuang**. Penanda permintaan menjaga agar pasien yang sedang dilihat tidak tertimpa pasien sebelumnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/frontend-roadmap.md` bagian 6d; `contracts/api-contract.md` bagian 10; laporan
`BE-LAB-35`; backend `LabOrderDtos.cs`, `LabOrderService.cs`, `LabMonitoringDtos.cs`,
`LabOrderController.cs` (dibaca saja); frontend `lab-monitoring-view.jsx`,
`lab-monitoring-table-columns.jsx`, `use-lab-monitoring.jsx`, `lab-order.service.js`,
`confirm-modal.jsx`, `print-signa-obat/index.jsx`, `kiosk-home-view.jsx`,
`lab-order-constants.jsx`, dan `design-tokens.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/laboratory-management/lab-print-rules.js` | **Baru.** Aturan murni isi dokumen: daftar pemeriksaan yang dicetak, ketiga blok tanda tangan, kepala dokumen, label Bahasa Indonesia, dan nama berkas |
| `src/components/view/health-services/laboratory-management/lab-monitoring/lab-order-print-document.jsx` | **Baru.** Lembar ringkasan satu pesanan, memakai `forwardRef` agar dapat dicetak |
| `src/style/health-services/laboratory-management/lab-monitoring/lab-order-print.module.css` | **Baru.** Gaya dokumennya. Seluruh warna memakai token; nol hex, rgb, dan hsl baru |
| `src/lib/hooks/health-services/laboratory-management/use-lab-monitoring.jsx` | Keadaan pratinjau, pengambilan detail pesanan, dan penanda permintaan |
| `src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view.jsx` | Pratinjau `ConfirmModal` beserta pemicu `useReactToPrint` |
| `src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-table-columns.jsx` | Tombol Cetak pada kolom aksi |
| `tests/unit/lab-print-rules.test.mjs` | **Baru.** 14 uji unit |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya mengikuti pola yang sudah ada: view → hook → service → `InstanceAxios`
bersama. Nol Axios instance baru, nol slice Redux baru, nol abstraksi generik.

**Komponen bersama dipakai ulang, bukan dibuat baru.** Pratinjaunya memakai `ConfirmModal` yang
sudah dipakai pop-up konfirmasi dan pembatalan pada layar yang sama, dengan `size="lg"` dan
`showIcon={false}`; tombol barisnya memakai `BaseButton`; pesan galatnya memakai
`InformationAlert`. **Nol komponen base baru dibuat**, sehingga gerbang keputusan base component
tidak menghasilkan satu pun elemen berstatus `NEW`.

**Detail pesanan diambil lewat service yang sudah ada** — `getLabOrderDetail` pada
`lab-order.service.js` — bukan lewat panggilan baru. Amandemen `r15` memang sengaja mengisi jalur
yang sudah dipakai, bukan melahirkan jalur baru.

**Satu keputusan tentang gaya yang perlu dibaca.** `lab-order-print.module.css` memiliki
typography-nya sendiri, dan itu disengaja: berkas itu menata sebuah **dokumen di atas kertas**,
bukan layar. Nol aturan di dalamnya menyasar Hero, SummaryGrid, DataFilter, DataTable,
BaseButton, StatusBadge, BaseFormControl, maupun Pagination, sehingga kontrak typography komponen
bersama tidak tersentuh. Keenam grep anti-regresi `ui-consistency-checklist.md` dijalankan atas
berkas ini dan hasilnya nol — termasuk nol `!important` dan nol blok `prefers-color-scheme`.

**Logo memakai `Image` react-bootstrap, bukan `next/image`**, mengikuti cetak signa obat. Dokumen
ini dikloning `react-to-print` ke jendela cetak, dan pembungkus lazy-load beserta `srcset` milik
`next/image` tidak dapat diandalkan pada salinan itu. Pilihan ini sekaligus membuat berkas baru
ini **nol peringatan lint**, berbeda dari `<img>` mentah yang dipakai layar kiosk.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `Memuat rincian pesanan...` di dalam pratinjau, dan tombol Cetak **nonaktif** |
| Kosong | Pesanan tanpa rincian pemeriksaan menampilkan `Pesanan ini tidak memuat rincian pemeriksaan.` Daftar terpesan kosong **bukan** keadaan kosong — ia jalur sah dan mencetak `procedureName` |
| Gagal | `InformationAlert` merah berisi pesan dari server, atau `Rincian pesanan gagal dimuat. Muat ulang lalu coba lagi.` Tombol Cetak tetap nonaktif |
| Tanpa hak akses | Tidak berubah. Layar tetap dibungkus `AccessDeniedGate`, dan endpoint detailnya memakai `LabOrder : Read` yang sudah dipegang pemakai layar ini |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}` | Mengambil nama pembuat order dan **daftar pemeriksaan yang benar-benar dipesan** | `LabOrder : Read` |

Dipanggil **hanya ketika tombol Cetak ditekan**, satu kali per pembukaan pratinjau. Layar daftar
tidak ikut memanggilnya.

**Nol permintaan tulis dikirim task ini.** Mencetak tidak mengubah apa pun di server, dan hal itu
diperiksa langsung — lihat 6.2.

Ruas yang dibaca dari baris daftar (`LabMonitoringItemResponse`, sudah ada sebelumnya):
`patientName`, `medicalRecordNumber`, `encounterNumber`, `discipline`, `orderStatus`,
`procedureCode`, `procedureName`, `hasCito`, `requestedAt`, `confirmedAt`, `confirmedByName`,
`examinerDoctorName`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas keenam berkas task ini | **Nol keluaran** — nol error, nol peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` seluruh repository | Nol keluaran | `PASS` | Keluaran perintah |
| Keenam grep anti-regresi `ui-consistency-checklist.md` | Nol temuan pada keenamnya | `PASS` | 6.1 |
| Uji unit baru | **14 dari 14 lolos** | `PASS` | `tests/unit/lab-print-rules.test.mjs` |
| Seluruh uji unit repository | **977 dari 977 lolos** | `PASS` | 963 uji lama lolos tanpa satu pun disentuh |
| `npm run build` — build produksi | `Compiled successfully`, `Standalone runtime siap dijalankan` | `PASS` | Keluaran perintah |
| **Layar** — 8 pemeriksaan terhadap aplikasi yang benar-benar berjalan | **8 dari 8 lolos** | `PASS` | 6.2 |

**Uji manual: `PASS`.**

**Tidak dijalankan:** `npm run test:e2e` dan `npm run test:uat` — tidak diminta task ini.
Pencetakan ke **printer fisik** juga tidak dijalankan: yang dapat dibuktikan dari harness adalah
permintaan cetak benar-benar dipicu, bukan hasil di atas kertas.

### 6.1 Grep anti-regresi

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal pada stylesheet baru | **0** |
| Typography yang menimpa komponen bersama | **0** selector — dua kecocokan yang muncul ada di dalam komentar |
| Tombol non-base pada berkas jsx baru | **0** |
| `<table>` tanpa kontrak typography | **0** — satu tabel, dan ia membawa `data-flat-table="true"` |
| Bootstrap utility typography di dalam blok tabel | **0** |
| `!important` baru | **0** |
| Blok `prefers-color-scheme` baru | **0** |

### 6.2 Verifikasi layar terhadap aplikasi yang benar-benar berjalan

Dijalankan terhadap build produksi standalone pada `127.0.0.1:3710`, jawaban server dipalsukan
lewat route Playwright, dan **`window.print` diganti pencatat** supaya "tidak mengeluarkan kertas"
dapat dibuktikan, bukan diasumsikan. **8 dari 8 lolos.**

| Butir | Bukti |
| --- | --- |
| `S1` tombol Cetak berdiri pada setiap baris | Tepat 2 tombol untuk 2 baris |
| `S2` **satu klik Cetak tidak mengeluarkan kertas** | Pratinjau terbuka, dan sesudah ditunggu **`window.print` tercatat 0 kali** |
| `S3` pratinjau memuat daftar terpesan, bukan hanya wakilnya | `Hemoglobin`, `Kalium`, dan `Natrium` ketiganya tampil |
| `S3` baris `Cancelled` ikut tercetak beserta keterangannya | `Dibatalkan` dan `Sudah masuk wadah` keduanya tampil |
| `S3` detail dipanggil **tepat sekali**, dan nol permintaan tulis | 1 pemanggilan `GET /lab-orders/{id}`; **0** permintaan non-`GET` |
| `S4` ketiga blok tanda tangan tampil pada pesanan **belum** dikonfirmasi | Ketiga label peran tampil; `dr. Sinta Maharani` tampil; `Belum dikonfirmasi` tampil |
| `S5` pesanan tanpa baris terpesan mencetak `procedureName` sebagai isi lengkap | `Glukosa Darah Sewaktu` tampil beserta keterangan pemeriksaan tunggal, dan **nol pesan galat** |
| `S6` tombol Cetak di dalam pratinjau **benar-benar mencetak** | Pencatat `print` naik di atas 0; **0** permintaan tulis menyertainya |
| `S7` **menutup pratinjau tidak mencetak dan tidak mengirim apa pun** | Sesudah `Tutup` ditekan dan ditunggu: `print` tercatat **0** kali, permintaan tulis **0** |
| **`AC-95`** ketiga **nama** tanda tangan tercetak pada pesanan yang **sudah** dikonfirmasi | `dr. Sinta Maharani`, `Dewi`, dan `dr. Fajar Rama Ragussa` ketiganya tampil; **nol** blok bertanda kosong |

**Satu asersi sempat gagal, dan yang salah adalah ujinya — bukan produknya.** `S6` semula
melaporkan `print` tidak pernah terpanggil. Penyebabnya bukan tombol yang tidak berfungsi:
`react-to-print` v3 mencetak lewat **iframe**, sehingga stub pada `window.print` halaman utama
memang tidak akan pernah tertangkap. Harness diperbaiki agar mencatat ke `window.top` dari frame
mana pun, dan asersinya lolos. Kegagalannya **diperiksa, bukan diasumsikan sebagai masalah
harness** — menganggapnya begitu tanpa bukti persis akan menyembunyikan cacat sungguhan bila
dugaan itu keliru.

**Pemeriksaan `AC-95` ditambahkan menyusul, sesudah ditemukan bahwa `S4` memakai baris yang belum
dikonfirmasi** sehingga hanya membuktikan blok tanda tangannya ada, bukan namanya tercetak. Itu
kriteria yang menjadi alasan task ini ada, dan membiarkannya tanpa bukti langsung akan mengulang
persis pelajaran `AC-83`.

### 6.3 Alat verifikasinya

Konfigurasi dan spesifikasi Playwright **sementara**, dihapus sesudah selesai. **Nol berkas uji
layar tertinggal di repository**, dan `test-results/` ikut dibersihkan — dibuktikan lewat
`git status --short` pada bagian 8.

---

## 7. Acceptance criteria dan Definition of Done

### 7.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Print **membuka preview** | **Terpenuhi** | 6.2 `S2` — pratinjau terbuka dan `window.print` tercatat **0** kali |
| Preview **dapat dicetak** | **Terpenuhi** | 6.2 `S6` — pencatat `print` naik sesudah tombol di dalam pratinjau ditekan |
| **Tanda tangan tetap tampil** | **Terpenuhi** | 6.2 `S4` dan `AC-95` — ketiga blok selalu ada; ketiga nama tercetak pada pesanan yang sudah dikonfirmasi |

### 7.2 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-95` — konfirmasi **menolak** bila dokter pemeriksa belum dipilih, dan dokter yang dipilih **tampil pada daftar serta ringkasan cetak** | **TERPENUHI PENUH** | Penolakan: backend `VAL-72`/`VAL-73` terbukti `422` pada [`BE-LAB-31.md`](../backend/BE-LAB-31.md), dan tombol simpan nonaktif di layar pada [`FE-LAB-15.md`](FE-LAB-15.md). Tampil pada daftar: [`FE-LAB-15.md`](FE-LAB-15.md). **Tampil pada ringkasan cetak: task ini**, 6.2 baris `AC-95` |

Dengan tertutupnya bagian ringkasan cetak, **`AC-94` dan `AC-95` kini keduanya terpenuhi penuh**,
sehingga `FR-11.12` berpindah menjadi `SELESAI`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Nol peringatan baru.** Berkas dokumen cetak sempat membawa satu peringatan `@next/next/no-img-element`; diselesaikan dengan memakai `Image` react-bootstrap seperti cetak signa obat, bukan dengan mengabaikannya |
| Masalah yang diketahui | **Jalur daftar terpesan terisi belum pernah terlihat pada data sungguhan.** `LabOrderedProcedure` berisi 0 baris pada database, sehingga jalur itu hanya terbukti lewat jawaban server yang dipalsukan dan uji unit. Jalur kosongnya — yang ditempuh seluruh pesanan nyata hari ini — terbukti pada keduanya. **Pencetakan ke printer fisik juga belum pernah dijalankan**; yang terbukti adalah permintaan cetaknya dipicu |
| Dependency backend | Nol tersisa. `r15` `approved` dan `BE-LAB-35` ✅ selesai pada hari yang sama |
| Perubahan sampingan | `NONE`. `lab-monitoring-slice.jsx` dan `lab-monitoring.module.css` yang tampak berubah pada `git status` berasal dari `FE-LAB-15` dan `FE-LAB-16` yang belum di-commit; nol di antaranya disentuh task ini |
| Interupsi | `NONE` |
| Status Git | Dilampirkan pada 8.1 |
| Langkah berikutnya | **Seluruh layar modul Laboratorium selesai.** Yang tersisa di luar layar: `r16` beserta task pelaksananya untuk `VAL-59`/`AC-66` — turunan keputusan `LAB-CONFLICT-006` pilihan A yang diambil 2026-09-16 dan **belum ditulis**; dan `BE-EXT-05` ⛔ yang tertahan `LAB-OPEN-025` serta `LAB-OPEN-026`, keduanya milik `registration-management` |

### 8.1 Status Git

Branch `YogaV2`, upstream `origin/YogaV2`. **Nol `git add`, nol commit, nol push.**

Berkas milik `FE-LAB-17`:

```text
 M src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-table-columns.jsx
 M src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-monitoring.jsx
?? src/components/view/health-services/laboratory-management/lab-monitoring/lab-order-print-document.jsx
?? src/lib/hooks/health-services/laboratory-management/lab-print-rules.js
?? src/style/health-services/laboratory-management/lab-monitoring/lab-order-print.module.css
?? tests/unit/lab-print-rules.test.mjs
```

Berkas lain pada working tree berasal dari `FE-LAB-13`, `FE-LAB-14`, `FE-LAB-15`, dan
`FE-LAB-16` yang belum di-commit, dan tidak disentuh task ini.
