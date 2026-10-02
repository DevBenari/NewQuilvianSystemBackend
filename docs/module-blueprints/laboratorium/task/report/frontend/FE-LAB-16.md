# Laporan Perubahan Frontend — `FE-LAB-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-16` |
| Judul | Pop-up pembatalan beralasan dan alert konfirmasi akhir |
| Slice | `EPIC-LAB-12`, gelombang `MVP-5c` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6d |
| Trace | `FR-11.13`; `LAB-DEC-063`; `AC-96`, `AC-97` |
| Contract version | `LAB-API-v1` **`r12`** §7.2 `PUT /lab-orders/{id}/cancel`; `LAB-VAL-v1` `r6` `VAL-74`, `VAL-75` |
| Dependency | `BE-LAB-32` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — layar Laboratorium; artefak blueprint pada repository backend |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `4031fd3d7`, branch `YogaV2` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — ketiga butir DoD terpenuhi dan **terbukti pada aplikasi yang benar-benar berjalan**, termasuk butir yang paling menentukan: **menutup alert konfirmasi akhir tidak mengirim permintaan apa pun** |

---

## 1. Masalah yang diperbaiki

**Keadaan sebelum perubahan.** Tidak ada satu pun cara membatalkan pesanan dari layar
Laboratorium. `BE-LAB-32` sudah mewajibkan alasan pembatalan dan mempersempit statusnya, tetapi
aturan itu tidak pernah disentuh siapa pun dari layar — **nol pembatalan pesanan pernah terjadi**.

**Dua bahaya yang ditutup task ini.** Pertama, pembatalan **tanpa alasan** — yang membuat
pertanyaan "kenapa pemeriksaan pasien ini dibatalkan?" tidak punya jawaban berhari-hari kemudian.
Kedua, pembatalan **karena salah klik** — dan pembatalan menghentikan seluruh wadah yang sedang
berjalan, sehingga salah klik di sini berarti pekerjaan pasien yang terhenti.

---

## 2. Proses bisnis

### 2.1 Alur normal, dua tahap dan berurutan

1. Petugas membuka salah satu dari ketiga menu pemeriksaan.
2. Aksi **Batalkan** tampil **hanya** pada pesanan yang memang masih dapat dibatalkan.
3. **Tahap 1** — pop-up **Batalkan Pemeriksaan** terbuka, memuat ringkasan pasien dan isian
   **Alasan Pembatalan**. Tombol `Lanjut Pembatalan` **nonaktif** sampai alasannya terisi.
4. Menekan `Lanjut Pembatalan` **belum mengirim apa pun**. Ia hanya membawa alasannya ke tahap
   berikutnya.
5. **Tahap 2** — **alert konfirmasi akhir** terbuka, mengulang **apa yang akan dibatalkan**
   beserta **alasan yang baru saja diketik**. Tombolnya berbunyi `Konfirmasi Pembatalan`.
6. Barulah permintaan dikirim. Sesudah berhasil, barisnya menjadi `Cancelled` dan aksi Batalkan
   **hilang** dari baris itu.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Alasan kosong atau hanya spasi | `Lanjut Pembatalan` **nonaktif**. Backend juga menolak `422` (`VAL-74`), sehingga layar bukan satu-satunya penjaga |
| **Menutup alert konfirmasi akhir** | Kembali ke keadaan semula; **nol permintaan dikirim**; barisnya tidak berubah |
| Status tidak sah — `Accepted`, `InProcess`, `Completed`, `Cancelled`, `OnHold` | Aksi Batalkan **tidak ditampilkan** |
| Backend menolak `409` | Alert tetap terbuka beserta pesannya — pesanannya sudah melewati keadaan yang menerima pembatalan, dan daftarnya perlu dimuat ulang |

### 2.3 Kenapa dua tahap, bukan satu

Satu pop-up berisi isian alasan **dan** tombol kirim berarti satu klik memisahkan petugas dari
pembatalan yang tidak dapat diurungkan. Tahap kedua memaksanya membaca ulang apa yang akan
dibatalkan — beserta kalimat yang baru saja ia tulis sendiri — sebelum menyetujuinya.

### 2.4 Kenapa aksinya disembunyikan, bukan dinonaktifkan

Berbeda dengan tombol Konfirmasi `FE-LAB-15` yang **dinonaktifkan**, aksi Batalkan
**disembunyikan** pada status yang tidak sah — mengikuti kewenangan UI apa adanya. Pembatalan
adalah tindakan yang menghapus pekerjaan pasien; menampilkannya sebagai tombol redup tetap
mengundang orang mencobanya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `lib/hooks/.../lab-cancellation-rules.js` | **Baru.** Aturan tampil, muatan, dan ringkasan alert |
| `lib/services/.../lab-order.service.js` | `cancelLabOrder` |
| `lib/state/slice/.../lab-monitoring-slice.jsx` | Keadaan pembatalan per disiplin; thunk; pembaruan baris |
| `lib/hooks/.../use-lab-monitoring.jsx` | Keadaan dua tahap dan penyimpanannya |
| `components/view/.../lab-monitoring-table-columns.jsx` | Aksi Batalkan, disembunyikan pada status yang tidak sah |
| `components/view/.../lab-monitoring-view.jsx` | Kedua modal |
| `style/.../lab-monitoring.module.css` | Gaya dua aksi pada satu kolom |
| `tests/unit/lab-cancellation-rules.test.mjs` | **Baru.** Sembilan uji |

### 3.2 Dua tahap disimpan sebagai dua keadaan terpisah

`cancelTarget` untuk tahap 1, `cancelPending` untuk tahap 2 — **bukan** satu keadaan dengan
penanda di dalamnya. Dengan begitu keduanya tidak dapat terbuka bersamaan, dan menutup tahap 2
benar-benar berarti tidak ada permintaan yang dikirim.

### 3.3 Daftar status ditulis sebagai yang **diizinkan**

`STATUS_DAPAT_DIBATALKAN` memuat `Requested` dan `Confirmed` saja. Ditulis sebagai daftar yang
diizinkan, bukan yang dilarang, supaya **status baru yang kelak ditambahkan tidak otomatis menjadi
dapat dibatalkan** — ia harus dimasukkan secara sadar. Diuji langsung lewat `S3`.

### 3.4 Satu selisih terhadap kewenangan UI, dan alasannya

Kewenangan UI menulis aksi Batalkan tidak ditampilkan pada tiga status: `Diproses`, `Selesai`, dan
`Dibatalkan`. `VAL-75` mempersempit lebih jauh — hanya `Requested` dan `Confirmed` yang sah,
sehingga `Accepted` dan `OnHold` juga ditolak backend.

**Yang diikuti adalah kontraknya.** Menampilkan aksi pada `Accepted` atau `OnHold` berarti memasang
tombol yang **selalu gagal** dengan `409`. Selisih ini dilaporkan, bukan didiamkan; bila pemilik
menghendaki aksinya tetap tampil di sana, itu perubahan kewenangan UI tersendiri.

### 3.5 Dampak kontrak API dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol perubahan.** Satu endpoint yang sudah ada mulai dikonsumsi |
| Backend | **Nol baris backend diubah** |
| Keamanan/Auth | Nol perubahan. Alasan pembatalan disimpan backend sebagai `ReasonNote` pada jejak audit; layar nol menyimpannya |

---

## 4. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/v1/health-services/laboratory-management/lab-orders/{id}/cancel` | Membatalkan pesanan beserta wadah yang masih berjalan | `LabOrder : Update` |

**Muatan yang dikirim** — `cancelReason` saja, sudah dirapikan.

---

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas keenam berkas task ini | **Nol keluaran** | `PASS` | — |
| `npm run build` — build produksi | Berhasil, `Compiled successfully` | `PASS` | — |
| Uji unit baru | **9 dari 9 lolos** | `PASS` | `tests/unit/lab-cancellation-rules.test.mjs` |
| Seluruh uji unit repository | **952 dari 952 lolos** | `PASS` | Nol regresi |
| **Layar** — aksi Batalkan disembunyikan pada status yang tidak sah | Lolos | `PASS` | 5.1 |
| **Layar** — **menutup alert TIDAK mengirim permintaan** | Lolos | `PASS` | 5.1 |
| **Layar** — menyetujui alert mengirim alasan yang sudah dirapikan | Lolos | `PASS` | 5.1 |

### 5.1 Verifikasi layar terhadap aplikasi yang benar-benar berjalan

Dijalankan terhadap build produksi standalone pada `127.0.0.1:3710`, jawaban server dipalsukan
lewat route Playwright. **3 dari 3 lolos.**

| Butir | Bukti |
| --- | --- |
| **`AC-97`** — aksi disembunyikan | Lima baris dipasang: `Requested`, `Confirmed`, `InProcess`, `Completed`, `Cancelled`. **Tepat 2 tombol Batalkan** tampil |
| **`AC-96`** — alasan wajib | `Lanjut Pembatalan` **nonaktif** sebelum alasan diisi, aktif sesudahnya |
| Tahap 1 tidak mengirim apa pun | Sesudah `Lanjut Pembatalan` ditekan, **nol permintaan** tercatat |
| Alert mengulang apa yang dibatalkan | `Alasan: Pasien menolak pengambilan sampel` tampil pada alert |
| **Menutup alert tidak mengirim permintaan** | Sesudah `Kembali` ditekan dan ditunggu, **nol permintaan** tercatat; barisnya tidak berubah dan aksinya masih ada |
| Menyetujui alert | Muatan tepat `["cancelReason"]`, bernilai **sudah dirapikan** — spasi di awal dan akhir hilang |
| Sesudah berhasil | Baris menjadi `Cancelled` dan aksi Batalkan **hilang** |

### 5.2 Alat verifikasinya

Konfigurasi dan spesifikasi Playwright **sementara**, dihapus sesudah selesai. **Nol berkas uji
layar tertinggal di repository**, dan `test-results/` ikut dibersihkan.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Alasan wajib ditegakkan **di layar** dan **tetap ditegakkan backend** | **Terpenuhi** | Tombol nonaktif di layar (5.1); `VAL-74` menolak `422` di backend, terbukti [`BE-LAB-32.md`](../backend/BE-LAB-32.md) |
| Aksi Batalkan **tidak tampil** pada status yang tidak sah | **Terpenuhi** | Tepat 2 dari 5 baris membawanya — 5.1; uji unit `S1`, `S2`, `S3` |
| **Menutup alert tidak mengirim permintaan** | **Terpenuhi** | Nol permintaan tercatat sesudah alert ditutup — 5.1 |

### 6.2 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-96` — pembatalan **tanpa alasan ditolak**; alasan tersimpan pada jejak audit dan terbaca kembali | **Terpenuhi** | Layar menahan; backend menolak `422` dan menyimpan `ReasonNote` — keduanya sudah terbukti terpisah |
| `AC-97` — pembatalan **ditolak** pada status yang tidak sah; `Requested` dan `Confirmed` **tetap dapat dibatalkan** | **Terpenuhi** | Aksinya hanya tampil pada kedua status itu; backend menolak `409` untuk sisanya |

**Kedua kriteria `FR-11.13` kini tertutup ujung ke ujung** — backend sejak `BE-LAB-32`, layar sejak
task ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning dari berkas task ini |
| Masalah yang diketahui | **Satu selisih terhadap kewenangan UI, dilaporkan bukan didiamkan:** kewenangan menulis tiga status yang dilarang, sedangkan kontrak `VAL-75` mempersempit lebih jauh. Yang diikuti kontraknya — lihat 3.4 |
| Risiko tersisa | **Pertama**, daftar yang sudah terbuka beberapa menit dapat basi: pesanan yang statusnya sudah berpindah akan tetap menampilkan aksi Batalkan sampai dimuat ulang. Menekannya **tidak berbahaya** — backend menolak `409` dan pesannya terbaca di alert. **Kedua**, pembatalan yang berhasil hanya memperbarui `orderStatus` pada baris itu; jumlah wadah yang ikut dibatalkan **tidak** ditampilkan, padahal backend melaporkannya. Menampilkannya berarti menambah keterangan yang belum diminta kewenangan UI mana pun. **Ketiga**, verifikasi layar memakai jawaban server yang dipalsukan; jalur ke backend sungguhan belum pernah dilewati dari layar ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | **1.** `FE-LAB-13` — layar kiosk; penahannya terangkat, dan ia membuat panel kiosk `FE-LAB-14` benar-benar terisi. **2.** `FE-LAB-17` tetap ⛔ — fitur cetak belum pernah ada, dan tiga keputusan pemilik belum diambil. **3.** Gelombang `MVP-5c` tinggal `FE-LAB-17` |

### 7.1 Status Git

Berkas milik task ini pada `QuilvianSystemFrontendDev`, branch `YogaV2`:

```text
 M src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-table-columns.jsx
 M src/components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-monitoring.jsx
 M src/lib/services/health-services/laboratory-management/lab-order.service.js
 M src/lib/state/slice/health-services/laboratory-management/lab-monitoring-slice.jsx
 M src/style/health-services/laboratory-management/lab-monitoring/lab-monitoring.module.css
?? src/lib/hooks/health-services/laboratory-management/lab-cancellation-rules.js
?? tests/unit/lab-cancellation-rules.test.mjs
```

Enam berkas pertama juga memuat pekerjaan `FE-LAB-15`; keduanya belum di-commit, dan bagian milik
masing-masing task terpisah jelas.

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.
