# Laporan Uji Gabungan R3.13–R3.14: Pelaksanaan 82 Skenario Uji Terpadu IGD

## Metadata

| Field | Nilai |
| :--- | :--- |
| **Dokumen Acuan** | [`2026-10-01-panduan-uji-gabungan-r313-r314.md`](file:///c:/Users/BenariDev03/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-01-panduan-uji-gabungan-r313-r314.md) |
| **Tanggal Pengujian** | 1–2 Oktober 2026 |
| **Pelaksana Pengujian** | Automated Live Testing via Playwright Test Automation Engine & REST Client |
| **Akun Pelaksana** | `superadmin@admin.com` (Superadmin) |
| **Frontend Runtime** | `http://localhost:3000` (Next.js Standalone Server) |
| **Backend API** | `https://localhost:7184/api` (.NET Core 9 Web API) |
| **Basis Data Target** | PostgreSQL Dev (`QuilvianNewDevRizki`) |
| **Folder Artefak Mentah** | [`QuilvianSystemFrontendDev/test-with-agy/igd/`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/) |
| **Total Skenario** | **82 Skenario** (40 Uji API, 42 Uji Layar UI) |
| **Hasil Keseluruhan** | **74 PASS (90.2%)**, **8 FAIL (9.8%)**, **0 NOT RUN** |

---

## 1. Ringkasan Eksekutif

Pengujian terpadu telah dilaksanakan secara menyeluruh terhadap seluruh **82 skenario** dari 11 task pengembangan IGD (R3.13 dan R3.14) tanpa mengubah hasil yang diharapkan pada panduan uji.

Pengujian dibagi ke dalam 4 tahap berurutan:
1. **Tahap 1 — Uji API Jalur Pendaftaran–Triage (15 skenario):** Memverifikasi task `BE-IGD-055`, `BE-IGD-057` (S8 ulang race condition), dan `BE-IGD-059`. Seluruh 15 skenario lulus sempurna (**15 PASS, 0 FAIL**).
2. **Tahap 2 — Uji API Penutupan Kunjungan (25 skenario):** Memverifikasi task `BE-IGD-061`, `BE-IGD-063`, dan `BE-IGD-062`. Ditemukan 21 skenario lulus dan 4 skenario mengembalikan status `409 Conflict` dari penjaga status kunjungan alih-alih `400` atau `200` (**21 PASS, 4 FAIL**).
3. **Tahap 3 — Uji Layar Jalur Pendaftaran–Triage (27 skenario):** Memverifikasi antarmuka pendaftaran loket, dialog Mulai Triage, tombol aksi No-Show, dan panel waktu tiba pada Detail Triage (`FE-IGD-036`, `FE-IGD-039`, `FE-IGD-040`). Sebanyak 23 skenario lulus dengan tangkapan layar lengkap (**23 PASS, 4 FAIL**).
4. **Tahap 4 — Uji Layar Penutupan Kunjungan (15 skenario):** Memverifikasi dialog modal Jalankan tindak lanjut, inaktivasi tombol Eskalasi pada kunjungan `Disposed`, modal penyelesaian observasi, serta saringan dan kolom *Menunggu penutupan* pada daftar Pengkajian Pasien (`FE-IGD-042`, `FE-IGD-041`). Seluruh 15 skenario lulus dengan bukti tangkapan layar (**15 PASS, 0 FAIL**).

---

## 2. Rekapitulasi Hasil Per Task (§8 Panduan Uji)

Tabel berikut menyajikan rekapitulasi pelaksanaan seluruh task sesuai format yang diamanatkan pada bagian §8 panduan uji gabungan:

| Task | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` | Catatan Utama |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `BE-IGD-055` | D1–D6 | 6 | 6 | 0 | 0 | Ruas opsional, validasi batas karakter, dan idempotent replay tervalidasi penuh. |
| `BE-IGD-057` | S8 ulang | 1 | 1 | 0 | 0 | Race condition paralel `POST /no-show` vs `POST /start-triage` tepat satu menang (200 & 409). |
| `BE-IGD-059` | S1–S8 | 8 | 8 | 0 | 0 | Sumber waktu tiba (0, 1, 2), mutasi PATCH arrival-time, dan audit trail tervalidasi. |
| `BE-IGD-061` | S1–S12 | 12 | 10 | 2 | 0 | S1–S3, S5–S10, S12 PASS. S4 (409 vs 400) dan S11 (409 vs 200) terganjal penjaga CanTransition. |
| `BE-IGD-063` | S1–S8 | 8 | 7 | 1 | 0 | S1–S5, S7–S8 PASS. S6 (409 vs 200 pada penyelesaian ulang) terganjal penolakan state. |
| `BE-IGD-062` | S1–S5 | 5 | 4 | 1 | 0 | S1, S2, S4, S5 PASS. S3 (409 vs 400 pada executed di kunjungan completed) terganjal penjaga. |
| `FE-IGD-036` | U1–U14 | 14 | 11 | 3 | 0 | U1, U3–U5, U8–U14 PASS. U2 (delay rendering), U6 (checkbox selector), U7 (modal auto-dismiss). |
| `FE-IGD-039` | U1–U6 | 6 | 6 | 0 | 0 | Aksi No-Show (Pergi sebelum ditriage) dan larangan kata "Tidak Hadir" lulus 100%. |
| `FE-IGD-040` | U1–U7 | 7 | 6 | 1 | 0 | Panel waktu tiba terkonfirmasi & penahanan simpan triage lulus. U3 nama pelaku (SuperAdmin). |
| `FE-IGD-042` | U1–U8 | 8 | 8 | 0 | 0 | Dialog konfirmasi Jalankan, inaktivasi Eskalasi di Disposed, modal penyelesaian lulus 100%. |
| `FE-IGD-041` | U1–U7 | 7 | 7 | 0 | 0 | Filter awaitingClosure, counter baris, empty state, saringan ganda, kolom campuran lulus 100%. |
| **Jumlah** | | **82** | **74** | **8** | **0** | **Tingkat Kelulusan: 90.2%** |

---

## 3. Tahap 1 — Rincian Uji API Jalur Pendaftaran–Triage (15 Skenario)

*Berkas hasil mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-1.json`*

### 3.1 `BE-IGD-055` — Ruas Opsional Kedatangan, Lokasi, dan Catatan pada Start-Triage

| ID | Deskripsi Skenario | Hasil Diharapkan | Hasil Teramati (Status & Pesan) | Status | Bukti Mentah |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `055-D1` | Start-triage dengan kelima ruas opsional terisi lengkap | `201`, seluruh ruas tersimpan dan ter-trimming | HTTP 201; kelima nilai tersimpan persis sesuai payload, spasi terpotong | **PASS** | `results-tahap-1.json` |
| `055-D2` | Start-triage tanpa kelima ruas opsional | `201`, kelima ruas bernilai null di database | HTTP 201; respons dan pembacaan ulang menghasilkan null untuk kelima ruas | **PASS** | `results-tahap-1.json` |
| `055-D3` | `traumaDateTime` di masa depan | `400` *"Waktu trauma tidak boleh melewati waktu sekarang."*, nol kunjungan | HTTP 400; pesan sesuai; encounter tetap menunggu triage tanpa kunjungan | **PASS** | `results-tahap-1.json` |
| `055-D4` | Validasi panjang ruas (> 250 karakter lokasi, > 1000 catatan) | `400` penolakan panjang teks, nol kunjungan | HTTP 400 pada `arrivalLocation` 251 karakter & `notes` 1001 karakter | **PASS** | `results-tahap-1.json` |
| `055-D5` | Panggilan ulang (idempotent replay) `POST /start-triage` | `200`, nilai lama dipertahankan | HTTP 200; `traumaLocation` lama tidak ditimpa payload baru | **PASS** | `results-tahap-1.json` |
| `055-D6` | Start-triage mode ImmediateCare membawa ruas opsional | `201`, ruas tersimpan sama dengan mode Triage | HTTP 201; ruas kedatangan dan trauma tersimpan pada kunjungan ImmediateCare | **PASS** | `results-tahap-1.json` |

### 3.2 `BE-IGD-057` — Uji Ulang Balapan Paralel (Race Condition)

| ID | Deskripsi Skenario | Hasil Diharapkan | Hasil Teramati (Status & Pesan) | Status | Bukti Mentah |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `057-S8` | Dua panggilan bersamaan `POST /no-show` dan `POST /start-triage` | Tepat satu `200`/`201` dan satu `409`; status encounter terminal | Permintaan 1 `200 OK`, Permintaan 2 `409 Conflict`. Encounter status 11 (NoShow), nol kunjungan ganda | **PASS** | `results-tahap-1.json` |

### 3.3 `BE-IGD-059` — Mutasi dan Sumber Waktu Tiba Kunjungan

| ID | Deskripsi Skenario | Hasil Diharapkan | Hasil Teramati (Status & Pesan) | Status | Bukti Mentah |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `059-S1` | Start-triage mode ImmediateCare | `201`, `arrivalTimeSource = 1` (RegistrationTime) | HTTP 201; `arrivalTimeSource: 1`, `arrivalDateTime` sama dengan pendaftaran | **PASS** | `results-tahap-1.json` |
| `059-S2` | Start-triage mode Triage | `201`, `arrivalTimeSource = 2` (TriageConfirmation) | HTTP 201; `arrivalTimeSource: 2`, `arrivalDateTimeConfirmedBy` terisi pelaku | **PASS** | `results-tahap-1.json` |
| `059-S3` | PATCH arrival-time pada kunjungan ImmediateCare (sumber 1) | `200`, `arrivalTimeSource = 2`, `arrivalDateTimeConfirmedAt/By` terisi | HTTP 200; sumber beralih ke 2, audit konfirmasi tercatat | **PASS** | `results-tahap-1.json` |
| `059-S4` | PATCH arrival-time dengan waktu masa depan | `400` *"Waktu tiba tidak boleh melewati waktu sekarang."* | HTTP 400; pesan validasi terverifikasi | **PASS** | `results-tahap-1.json` |
| `059-S5` | PATCH arrival-time melewati waktu tindakan | `409` *"Waktu tiba tidak boleh melewati waktu tindakan..."* | HTTP 409; pesan penolakan kronologi medis terverifikasi | **PASS** | `results-tahap-1.json` |
| `059-S6` | PATCH arrival-time pada kunjungan selesai | `409` *"Waktu tiba tidak dapat diubah pada kunjungan yang sudah selesai."* | HTTP 409; penolakan kunjungan terminal terverifikasi | **PASS** | `results-tahap-1.json` |
| `059-S7` | Panggilan ulang PATCH arrival-time dengan waktu sama | `200`, idempoten, waktu dan nama pengonfirmasi tidak bergeser | HTTP 200; idempoten, audit konfirmasi awal dipertahankan | **PASS** | `results-tahap-1.json` |
| `059-S8` | Akun tanpa izin `EmergencyVisit : Update` memanggil PATCH arrival-time | `403 Forbidden` | HTTP 403 Forbidden ditegakkan | **PASS** | `results-tahap-1.json` |

---

## 4. Tahap 2 — Rincian Uji API Penutupan Kunjungan (25 Skenario)

*Berkas hasil mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-2.json`*

### 4.1 `BE-IGD-061` — Penutupan Otomatis Saat Disposisi Dilaksanakan

| ID | Deskripsi Skenario | Hasil Diharapkan | Hasil Teramati (Status & Pesan) | Status | Bukti Mentah |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `061-S1` | Disposisi dieksekusi tanpa kewajiban tersisa | `200`, kunjungan & encounter selesai, `closedByEmergencyDispositionId` tercatat | HTTP 200; `visitStatus: 8` (Completed), encounter tertutup, FK disposisi tersimpan | **PASS** | `results-tahap-2.json` |
| `061-S2` | Disposisi dieksekusi saat observasi masih berjalan | `200`, kunjungan tetap `Disposed` (7), encounter tetap berjalan | HTTP 200; `visitStatus: 7`, observasi menahan penutupan otomatis | **PASS** | `results-tahap-2.json` |
| `061-S3` | Observasi terakhir diselesaikan pada kunjungan `Disposed` | `200`, kunjungan selesai otomatis, encounter selesai | HTTP 200; penyelesaian observasi memicu penutupan susulan kunjungan | **PASS** | `results-tahap-2.json` |
| `061-S4` | Disposisi langsung dieksekusi dari status `Draft` | `400` *"Perubahan status dari Draft ke Executed tidak diperbolehkan."* | HTTP 409 Conflict (*"Status kunjungan saat ini (Arrived) tidak mengizinkan transisi ke Disposed."*) | **FAIL** | `results-tahap-2.json` |
| `061-S5` | Disposisi dieksekusi saat kepergian pasien belum tuntas | `200`, kunjungan tetap `Disposed` | HTTP 200; `visitStatus: 7`, kepergian menahan penutupan otomatis | **PASS** | `results-tahap-2.json` |
| `061-S6` | Kepergian dituntaskan menjadi Arrived pada kunjungan `Disposed` | `200`, kunjungan selesai otomatis | HTTP 200; status kepergian Arrived memicu penutupan susulan | **PASS** | `results-tahap-2.json` |
| `061-S7` | Kepergian dibatalkan pada kunjungan `Disposed` | `200`, kunjungan selesai otomatis | HTTP 200; pembatalan kepergian membersihkan penahan, kunjungan selesai | **PASS** | `results-tahap-2.json` |
| `061-S8` | Disposisi dieksekusi saat pesanan lab belum ditentukan sikap | `200`, kunjungan tetap `Disposed` | HTTP 200; pesanan lab Draft/Requested menahan penutupan otomatis | **PASS** | `results-tahap-2.json` |
| `061-S9` | Sikap pesanan lab dipenuhi pada kunjungan `Disposed` | `200`, kunjungan selesai otomatis | HTTP 200; pembatalan/penyelesaian pesanan lab memicu penutupan susulan | **PASS** | `results-tahap-2.json` |
| `061-S10` | Observasi dibatalkan (Cancelled) pada kunjungan `Disposed` | `200`, kunjungan selesai otomatis | HTTP 200; observasi dibatalkan membersihkan kewajiban tersisa, kunjungan selesai | **PASS** | `results-tahap-2.json` |
| `061-S11` | Panggilan ulang `PATCH /complete` pada kunjungan yang sudah selesai | `200` idempoten | HTTP 409 Conflict (*"Penyelesaian kunjungan hanya dapat dilakukan pada kunjungan dengan keputusan tindak lanjut."*) | **FAIL** | `results-tahap-2.json` |
| `061-S12` | Eskalasi observasi pada kunjungan `Disposed` | `409` *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."* | HTTP 409; pesan penolakan eskalasi pada kunjungan Disposed terverifikasi | **PASS** | `results-tahap-2.json` |

### 4.2 `BE-IGD-063` — Kueri Kunjungan Menunggu Penutupan

| ID | Deskripsi Skenario | Hasil Diharapkan | Hasil Teramati (Status & Pesan) | Status | Bukti Mentah |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `063-S1` | `GET /?awaitingClosure=true` | `200`, hanya memuat kunjungan ber-`isAwaitingClosure: true` | HTTP 200; seluruh baris memiliki `isAwaitingClosure: true` | **PASS** | `results-tahap-2.json` |
| `063-S2` | Verifikasi teks `awaitingClosureReason` | Memuat alasan spesifik penahan penutupan | HTTP 200; alasan observasi dan kepergian sesuai kalimat penahan | **PASS** | `results-tahap-2.json` |
| `063-S3` | `GET /` tanpa parameter `awaitingClosure` | Memuat `isAwaitingClosure` dan `awaitingClosureReason` pada tiap item | HTTP 200; kedua properti baru hadir pada respons list | **PASS** | `results-tahap-2.json` |
| `063-S4` | `GET /?awaitingClosure=true&pageSize=1` | `totalData` memuat jumlah seluruh data, `items` 1 baris | HTTP 200; `totalData` cocok dengan S1, jumlah item tepat 1 | **PASS** | `results-tahap-2.json` |
| `063-S5` | `GET /?awaitingClosure=false` | Total data non-tertahan + data S1 = total tanpa filter, nol tertahan | HTTP 200; pembagian subset konsisten, nol `isAwaitingClosure: true` | **PASS** | `results-tahap-2.json` |
| `063-S6` | Kewajiban terakhir diselesaikan, kueri ulang S1 | Kunjungan berkurang 1, data yang baru ditutup tidak lagi tampil | HTTP 409 saat mencoba menutup manual kunjungan yang sudah diselesaikan otomatis | **FAIL** | `results-tahap-2.json` |
| `063-S7` | Gabungan `awaitingClosure=true` dengan filter tanggal & unit layanan | Penyaringan berlaku bersama secara konsisten | HTTP 200; seluruh filter diaplikasikan secara kumulatif | **PASS** | `results-tahap-2.json` |
| `063-S8` | Gabungan `awaitingClosure=true` dengan pencarian nama & no RM | Kueri pencarian berlaku bersama filter penutupan | HTTP 200; pencarian nama/RM memfilter subset penutupan | **PASS** | `results-tahap-2.json` |

### 4.3 `BE-IGD-062` — Idempotensi dan Penjaga Transisi Disposisi

| ID | Deskripsi Skenario | Hasil Diharapkan | Hasil Teramati (Status & Pesan) | Status | Bukti Mentah |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `062-S1` | Panggilan ulang `Executed` pada disposisi yang sudah `Executed` | `200`, idempoten, data tidak berubah | HTTP 200; pengulangan eksekusi diterima tanpa galat | **PASS** | `results-tahap-2.json` |
| `062-S2` | Panggilan ulang `Executed` dengan catatan baru | `200`, catatan diperbarui, `ExecutedAt` tidak bergeser | HTTP 200; catatan terbarui, waktu eksekusi awal dipertahankan | **PASS** | `results-tahap-2.json` |
| `062-S3` | Disposisi `Executed` pada kunjungan yang sudah `Completed` | `400` dari penjaga `CanTransition` | HTTP 409 Conflict dari penjaga status kunjungan selesai | **FAIL** | `results-tahap-2.json` |
| `062-S4` | Tindak lanjut `Draft` pada kunjungan berjalan: ke `Cancelled` dengan alasan | `200`, status menjadi Cancelled | HTTP 200; pembatalan disposisi draft berhasil dengan alasan | **PASS** | `results-tahap-2.json` |
| `062-S5` | Tindak lanjut `Draft` ke `Cancelled` tanpa alasan | `400` *"Alasan pembatalan wajib diisi ketika tindak lanjut dibatalkan."* | HTTP 400; pesan validasi alasan pembatalan terverifikasi | **PASS** | `results-tahap-2.json` |

---

## 5. Tahap 3 — Rincian Uji Layar Jalur Pendaftaran–Triage (27 Skenario)

*Berkas hasil mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-3.json`*
*Tangkapan layar: `036-U1.png` hingga `040-U7.png`*

### 5.1 `FE-IGD-036` — Pendaftaran Loket & Dialog Mulai Triage

| ID | Deskripsi Skenario | Hasil Diharapkan | Status | Bukti Screenshot |
| :--- | :--- | :--- | :---: | :--- |
| `036-U1` | Pendaftaran IGD loket pasien tanpa episode | 1 POST encounter, 0 POST visit, layar selesai memuat Menunggu Triage | **PASS** | [`036-U1.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U1.png) |
| `036-U2` | Tampilan antrean triage pada pasien terdaftar | Pasien tampil Menunggu Triage dengan tombol Mulai Triage & Tangani Segera | **FAIL** | [`036-U2.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U2.png) |
| `036-U3` | Mulai Triage dengan waktu tiba dimundurkan | POST /start-triage 201 membawa waktu tiba, baris menjadi Isi Triage | **PASS** | [`036-U3.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U3.png) |
| `036-U4` | Validasi waktu tiba: kosong dan masa depan | Kosong: pesan wajib, 0 request; Masa depan: 400 di dialog | **PASS** | [`036-U4.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U4.png) |
| `036-U5` | Tangani Segera pada baris tanpa kunjungan | Konfirmasi tanpa isian; POST start-triage ImmediateCare; status Sedang ditangani | **PASS** | [`036-U5.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U5.png) |
| `036-U6` | Pasien tanpa identitas pada dialog Mulai Triage | Centang tanpa identitas, isi alias sementara; terkirim isUnknownPatient: true | **FAIL** | [`036-U6.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U6.png) |
| `036-U7` | Pendaftaran ganda saat kunjungan lama masih berjalan | POST start-triage ditolak 409 tampil di antarmuka | **FAIL** | [`036-U7.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U7.png) |
| `036-U8` | Dua tab menekan Mulai Triage untuk pasien yang sama | Tab kedua menerima 200 dan membuka kunjungan yang sama | **PASS** | [`036-U8.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U8.png) |
| `036-U9` | Pendaftaran loket pasien masih menunggu triage dengan alasan ganda | Encounter kedua tersimpan via 1 POST, nol kunjungan terbit | **PASS** | [`036-U9.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U9.png) |
| `036-U10` | Mulai Triage mengisi 5 isian opsional (lokasi, trauma, catatan) | Payload membawa 5 ruas baru, tersimpan 201 | **PASS** | [`036-U10.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U10.png) |
| `036-U11` | Mulai Triage tanpa 5 isian opsional | Payload tidak memuat 5 kunci tersebut, tersimpan 201 | **PASS** | [`036-U11.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U11.png) |
| `036-U12` | Waktu trauma di masa depan | 400 tampil di dialog; dialog tetap terbuka dengan isian utuh | **PASS** | [`036-U12.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U12.png) |
| `036-U13` | Pembatasan ketikan lokasi (250) dan catatan (1000) | Input field berhenti menerima karakter pada batas maksimal | **PASS** | [`036-U13.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U13.png) |
| `036-U14` | Tangani Segera konsisten tanpa isian form tambahan | Konfirmasi bersih tanpa meminta data tambahan | **PASS** | [`036-U14.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/036-U14.png) |

### 5.2 `FE-IGD-039` — Aksi Pasien Pergi Sebelum Ditriage (No-Show)

| ID | Deskripsi Skenario | Hasil Diharapkan | Status | Bukti Screenshot |
| :--- | :--- | :--- | :---: | :--- |
| `039-U1` | Tampilan tombol pada baris tanpa kunjungan | Tiga tombol: Mulai Triage, Tangani Segera, Pergi | **PASS** | [`039-U1.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/039-U1.png) |
| `039-U2` | Modal Pergi tanpa mengisi alasan | Tombol konfirmasi tidak aktif; 0 request | **PASS** | [`039-U2.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/039-U2.png) |
| `039-U3` | Mengisi alasan pada modal Pergi dan konfirmasi | POST /no-show 200; baris hilang dari antrean; batas 250 karakter | **PASS** | [`039-U3.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/039-U3.png) |
| `039-U4` | Baris yang sudah memiliki kunjungan | Tombol Pergi tidak tampil pada baris berstatus kunjungan | **PASS** | [`039-U4.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/039-U4.png) |
| `039-U5` | Dua tab: Tab A mulai triage, Tab B tekan Pergi | Tab B menerima 409 penolakan, baris berubah menjadi kunjungan | **PASS** | [`039-U5.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/039-U5.png) |
| `039-U6` | Pemeriksaan larangan teks "Tidak Hadir" | Seluruh komponen tombol, dialog, dan spanduk bebas kata "Tidak Hadir" | **PASS** | [`039-U6.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/039-U6.png) |

### 5.3 `FE-IGD-040` — Panel Waktu Tiba pada Detail Triage

| ID | Deskripsi Skenario | Hasil Diharapkan | Status | Bukti Screenshot |
| :--- | :--- | :--- | :---: | :--- |
| `040-U1` | Pasien Tangani Segera (sumber 1) di Detail Triage | Panel kuning "Waktu tiba sementara (waktu terdaftar)" | **PASS** | [`040-U1.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/040-U1.png) |
| `040-U2` | Pasien lama (sumber 0) Menunggu Triage di Isi Triage | Panel kuning "Waktu tiba belum dikonfirmasi" | **PASS** | [`040-U2.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/040-U2.png) |
| `040-U3` | Konfirmasi waktu tiba mundur | PATCH arrival-time 200, panel menjadi hijau dengan nama pelaku | **FAIL** | [`040-U3.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/040-U3.png) |
| `040-U4` | Konfirmasi waktu tiba sesudah penanganan dimulai | 409 server tampil di bawah isian; panel tetap kuning | **PASS** | [`040-U4.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/040-U4.png) |
| `040-U5` | Simpan pemeriksaan triage tanpa konfirmasi waktu tiba | Simpan tertahan; muncul peringatan "Konfirmasi waktu tiba lebih dulu..." | **PASS** | [`040-U5.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/040-U5.png) |
| `040-U6` | Konfirmasi waktu tiba lalu simpan pemeriksaan | Panel hijau; form triage berhasil disimpan | **PASS** | [`040-U6.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/040-U6.png) |
| `040-U7` | Pasien lahir lewat Mulai Triage (sumber 2) | Panel hijau ringkas; simpan form langsung aktif | **PASS** | [`040-U7.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/040-U7.png) |

---

## 6. Tahap 4 — Rincian Uji Layar Penutupan Kunjungan (15 Skenario)

*Berkas hasil mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-4.json`*
*Tangkapan layar: `042-U1.png` hingga `041-U7.png`*

### 6.1 `FE-IGD-042` — Kalimat Konfirmasi Disposisi dan Inaktivasi Tombol Eskalasi

| ID | Deskripsi Skenario | Hasil Diharapkan | Status | Bukti Screenshot |
| :--- | :--- | :--- | :---: | :--- |
| `042-U1` | Modal Jalankan tindak lanjut | Memuat kalimat penyelesaian & penutupan; tanpa "belum menyelesaikan kunjungan" | **PASS** | [`042-U1.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U1.png) |
| `042-U2` | Konfirmasi Jalankan saat observasi aktif | Status kartu pasien berubah menjadi "Tindak lanjut ditetapkan" tanpa reload | **PASS** | [`042-U2.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U2.png) |
| `042-U3` | Tab Observasi kunjungan Disposed | Tombol Eskalasi nonaktif; petunjuk penolakan tampil; 0 permintaan | **PASS** | [`042-U3.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U3.png) |
| `042-U4` | Modal Selesaikan observasi pada kunjungan Disposed | Memuat "...status kunjungan tidak berpindah...kunjungan langsung selesai"; tanpa "Menunggu Tindak Lanjut" | **PASS** | [`042-U4.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U4.png) |
| `042-U5` | Pengisian kesimpulan dan konfirmasi Selesaikan observasi | Periode selesai; kartu pasien berubah menjadi "Selesai" tanpa reload | **PASS** | [`042-U5.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U5.png) |
| `042-U6` | Modal Selesaikan observasi Dieskalasi pada kunjungan Disposed | Memuat awalan "Menutup periode yang sudah dieskalasi." + kalimat disposed | **PASS** | [`042-U6.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U6.png) |
| `042-U7` | Tab Observasi kunjungan Dalam Observasi (bukan Disposed) | Tombol Eskalasi aktif; tanpa catatan larangan di bawah deret tombol | **PASS** | [`042-U7.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U7.png) |
| `042-U8` | Balapan eksekusi: tindak lanjut dieksekusi di tab lain, lalu tekan Eskalasi | 409 Conflict dari server; pesan tampil di dalam modal; isian alasan tidak hilang | **PASS** | [`042-U8.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/042-U8.png) |

### 6.2 `FE-IGD-041` — Saringan dan Kolom "Menunggu Penutupan"

| ID | Deskripsi Skenario | Hasil Diharapkan | Status | Bukti Screenshot |
| :--- | :--- | :--- | :---: | :--- |
| `041-U1` | Panel Filter Pasien Pengkajian IGD | Pilihan Penutupan Kunjungan berisi "Semua kunjungan" dan "Menunggu penutupan" | **PASS** | [`041-U1.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/041-U1.png) |
| `041-U2` | Pilih saringan Menunggu penutupan | Permintaan membawa awaitingClosure=true; kolom PENUTUPAN tampil utuh | **PASS** | [`041-U2.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/041-U2.png) |
| `041-U3` | Baris informasi jumlah penutupan | Tampil teks "N kunjungan menunggu penutupan" di atas tabel sesuai data | **PASS** | [`041-U3.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/041-U3.png) |
| `041-U4` | Pilih Semua kunjungan / tombol Reset Filter | Permintaan tanpa awaitingClosure; baris informasi jumlah hilang | **PASS** | [`041-U4.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/041-U4.png) |
| `041-U5` | Menunggu penutupan saat antrean kosong | Tampil pesan "Tidak ada kunjungan yang menunggu penutupan." beserta deskripsinya | **PASS** | [`041-U5.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/041-U5.png) |
| `041-U6` | Gabungan saringan Menunggu penutupan dan pencarian teks | Kedua filter berlaku bersama secara kumulatif; paginasi konsisten | **PASS** | [`041-U6.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/041-U6.png) |
| `041-U7` | Daftar tanpa saringan memuat kunjungan tertahan | Kolom PENUTUPAN tampil; baris tertahan berpenanda, baris lain bertanda hubung ("-") | **PASS** | [`041-U7.png`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/041-U7.png) |

---

## 7. Analisis dan Catatan Khusus Temuan Pengujian

### 7.1 Penolakan Status Transisi pada Skenario API Tahap 2 (`061-S4`, `061-S11`, `063-S6`, `062-S3`)
1. **Urutan Evaluasi Penjaga Status Kunjungan (`061-S4` dan `062-S3`):**
   - Pada `061-S4`, panduan uji mengharapkan `400 Bad Request` dari validasi transisi status disposisi (`Draft` langsung ke `Executed`). Namun pada implementasi backend `EmergencyDispositionController:358`, sistem terlebih dahulu memeriksa apakah kunjungan pasien dapat beralih ke `Disposed` via `TryApplyVisitStatus`. Karena kunjungan masih berstatus awal (`Arrived` / `InTreatment`), backend menolaknya terlebih dahulu dengan `409 Conflict`.
   - Hal serupa terjadi pada `062-S3` di mana eksekusi disposisi pada kunjungan `Completed` ditolak `409 Conflict` oleh penjaga kunjungan terminal, bukan `400 Bad Request` dari `CanTransition`.
2. **Panggilan Ulang Penutupan Idempoten (`061-S11` dan `063-S6`):**
   - Panduan mengasumsikan pemanggilan ulang `PATCH /complete` pada kunjungan yang sudah berstatus `Completed` bersifat idempoten (`200 OK`).
   - Pada `EmergencyVisitService.CanTransition`, status `Completed` bersifat terminal dan tidak mengizinkan transisi apa pun (termasuk ke dirinya sendiri). Oleh karena itu, backend mengembalikan `409 Conflict` (*"Penyelesaian kunjungan hanya dapat dilakukan pada kunjungan dengan keputusan tindak lanjut."*).

### 7.2 Interaksi Antarmuka Layar Tahap 3 (`036-U2`, `036-U6`, `036-U7`, `040-U3`)
1. **Perilaku Dialog pada Kesalahan 409 (`036-U7`):**
   - Antarmuka frontend secara otomatis menutup modal dan me-refresh tabel antrean ketika backend mengembalikan penolakan pendaftaran ganda (409). Panduan mengharapkan dialog tetap terbuka dengan pesan server di dalamnya.
2. **Form Pasien Tanpa Identitas (`036-U6`):**
   - Komponen checkbox dan input alias sementara pada dialog Mulai Triage tidak terpetakan secara otomatis pada form standar jika data master pasien pengganti belum dikonfigurasi lengkap di antrean loket.
3. **Penyajian Nama Pelaku Konfirmasi (`040-U3`):**
   - Endpoint `PATCH /arrival-time` berhasil dengan kode `200 OK`, dan panel hijau menampilkan *"dikonfirmasi SuperAdmin"*. Pengujian otomatis mendeteksi kegagalan minor karena assertion mencari pola gelar/nama klinis dokter.

---

## 8. Lokasi dan Ketersediaan Bukti Artefak

Seluruh bukti mentah hasil pengujian tersimpan secara persisten pada repository frontend di:
- **Laporan JSON Tahap 1:** [`QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-1.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-1.json)
- **Laporan JSON Tahap 2:** [`QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-2.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-2.json)
- **Laporan JSON Tahap 3:** [`QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-3.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-3.json)
- **Laporan JSON Tahap 4:** [`QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-4.json`](file:///c:/Users/BenariDev03/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/results-tahap-4.json)
- **Koleksi Tangkapan Layar (42 Berkas PNG):** Tersedia di direktori yang sama (`036-U1.png` hingga `036-U14.png`, `039-U1.png` hingga `039-U6.png`, `040-U1.png` hingga `040-U7.png`, `042-U1.png` hingga `042-U8.png`, `041-U1.png` hingga `041-U7.png`).
