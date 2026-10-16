# Panduan Uji Ulang Putaran A — Form Permintaan Rawat Inap (`BE-IGD-072` + `FE-IGD-059`)

| Field | Nilai |
| --- | --- |
| Tujuan | Menutup celah putaran A: A-01 (keadaan awal), A-11 (konkurensi), serta aturan validation §14 yang belum diuji (1, 5, 7, 8, 10, 13, 14, 17, 26) |
| Dasar | [Penilaian putaran A](2026-10-12-laporan-uji-form-permintaan-rawat-inap-putaran-a.md) bagian 7 |
| Kontrak | API `0.17.1` §12.2–12.3, validation `0.16.0` §14 |
| Lingkungan | Frontend `npm run dev` port 3000 (milik Rizki, **jangan** dimatikan atau dinyalakan ulang); backend `https://localhost:7184` yang sedang berjalan |
| Jenis | Verifikasi pengembang (*DevVerif*), **bukan** UAT |
| Disusun | 12 Oktober 2026, agent pengembang |

## 1. Batasan — melanggar satu butir membuat hasil run tidak dipakai

| No | Batasan |
| ---: | --- |
| 1 | Akun yang boleh dipakai hanya: perawat `dimas.kurniawan@rsmmc.local`, dokter DPJP `ranger.biru@admin.com`, dan — hanya untuk B-09 — akun dokter non-DPJP yang **diberikan Rizki**. Sandi dibaca dari variabel lingkungan `QA_PASS_PERAWAT`, `QA_PASS_DOKTER`, `QA_EMAIL_DOKTER_NON_DPJP`, `QA_PASS_DOKTER_NON_DPJP` yang disetel Rizki; **tidak** ditulis di skrip, berkas, log konsol, JSON, atau laporan |
| 2 | **Dilarang menebak atau mencoba-coba sandi.** Satu login per akun per run. Bila login gagal sekali, hentikan run dan laporkan |
| 3 | **Dilarang memakai `superadmin`** atau akun admin lain, dilarang memanggil `role-access/*`, dilarang menekan *Simpan Akses*, dan dilarang mengubah data master (jadwal dokter, unit layanan, klinik, kelas, jenis tindak lanjut) |
| 4 | Dilarang kueri basis data dalam bentuk apa pun (termasuk `SELECT`), dilarang SQL, dilarang memasang paket apa pun |
| 5 | Layar dibuka lewat menu: perawat *Instalasi Gawat Darurat → Pengkajian Pasien → pilih pasien*; dokter *Dokter → IGD → pilih pasien*. `page.goto` hanya untuk halaman login |
| 6 | Setiap aksi ditunggu sampai **selesai**: tombol tidak lagi *Memproses…*/*Menyimpan…* dan pesan hasil (berhasil atau penolakan) tampil. Baru sesudah itu tangkapan layar diambil (`fullPage: true`). **Jangan menutup tab atau halaman selama permintaan masih berjalan** |
| 7 | PASS/FAIL ditentukan dari kondisi nyata (status HTTP, teks pesan persis, isi ruas), bukan konstanta dan bukan `includes()` longgar. Pesan dibandingkan persis dengan kolom *Harapan* |
| 8 | Bukti di folder baru `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-putaran-a-permintaan-rawat-inap-20261012/`, satu subfolder per run (`run-01`, `run-02`, …). **Setiap run dilaporkan**, termasuk yang gagal di tengah. Bukti lama tidak ditimpa. Skrip yang diubah disimpan sebagai salinan baru di subfolder run berikutnya |
| 9 | JSON menyimpan status, body respons, dan `timestamp` server; token, cookie, dan sandi disensor |
| 10 | Laporan hanya ditulis di folder bukti (`laporan.md`). **Jangan menyunting** `task/report/**`, roadmap, traceability, atau dokumen blueprint lain. Jangan menulis "UAT", "LULUS PENUH", atau "100%"; tulis `PASS` / `FAIL` / `NOT RUN` per skenario beserta alasannya |
| 11 | Data yang dibuat hanya yang disebut skenario; seluruh id yang dibuat dicatat. Form dan tindak lanjut milik pasien lain tidak disentuh |
| 12 | Bila prasyarat tidak ada, **berhenti dan laporkan** — jangan menyiapkannya lewat API admin |

## 2. Prasyarat

| No | Prasyarat | Cara memeriksa |
| ---: | --- | --- |
| P1 | Jenis tindak lanjut rawat inap (kode `RI`) bertanda `requiresAdmissionRequest = true` | Sudah terbukti A-14 putaran A |
| P2 | Hak akses `EmergencyAdmissionRequest` perawat dan dokter sudah ada | Sudah terbukti putaran A |
| P3 | **Kunjungan K**: kunjungan IGD aktif, DPJP IGD aktif dr. Ranger Biru, **belum punya tindak lanjut**, sudah punya diagnosis ICD-10 (syarat konfirmasi tindak lanjut). Bila belum ada diagnosis, dokter menambahkan diagnosis kerja lewat layar dokter | Daftar Pengkajian Pasien. Bila tidak ada kunjungan yang memenuhi: **berhenti**, minta Rizki menyiapkan |
| P4 | Unit *Rawat Inap* punya jadwal hanya dr. Arif Lesmana; *Ruang Bayi* tanpa jadwal | Sudah terbukti A-04 putaran A |
| P5 | Variabel lingkungan sandi disetel Rizki | Skrip berhenti bila kosong, tanpa mencetak nilainya |

## 3. Skenario A — lewat layar, langkah demi langkah

| ID | Langkah | Harapan | Bukti |
| --- | --- | --- | --- |
| U-01 | **Dokter**: *Dokter → IGD* → kunjungan K → tab *Tindak Lanjut* → *Tetapkan Tindak Lanjut*: jenis *Rawat Inap*, unit tujuan *Rawat Inap*, alasan *"Uji ulang: nyeri dada, perlu observasi rawat inap"*, instruksi *"Uji ulang: monitor tanda vital tiap 4 jam"* → *Simpan Draf Tindak Lanjut*. **Jangan dikonfirmasi.** **Perawat**: *Instalasi Gawat Darurat → Pengkajian Pasien* → K → tab *Tindak Lanjut* | Baris *Rawat Inap* berstatus *Draf* memuat kotak *Form Permintaan Rawat Inap* **tanpa** tombol *Buat Form*, dengan teks *"Belum ada Form Permintaan Rawat Inap."* dan *"Form dapat dibuat sesudah tindak lanjut dikonfirmasi dokter."* | `U-01.png` |
| U-02 | **API, perawat**: `POST /emergency-admission-requests` `{ "dispositionId": "<id tindak lanjut U-01>" }` | `409` *"Form Permintaan Rawat Inap hanya dapat dibuat dari tindak lanjut rawat inap yang sudah dikonfirmasi dokter."* | `U-02.json` |
| U-03 | **Dokter**: aksi *Konfirmasi* pada tindak lanjut U-01. **Perawat**: muat ulang tab *Tindak Lanjut* | Kotak berisi *"Belum ada Form Permintaan Rawat Inap."* **dan** tombol *Buat Form* | `U-03.png` |
| U-04 | **Perawat**: *Buat Form* | Panel form terbuka, lencana *Draf*; unit *Rawat Inap*, indikasi = alasan U-01, rencana perawatan = instruksi U-01, lama rawat `3`; kotak di baris tindak lanjut kini *Buka Form* tanpa *Buat Form* | `U-04.png` |
| U-05 | **Konkurensi.** Buka form K di tab peramban kedua (perawat yang sama, lewat menu). Tab 1: lama rawat `5` → *Simpan Draf* → **tunggu** pesan *"Draf Form Permintaan Rawat Inap tersimpan."*; catat lama waktu dari klik sampai pesan muncul. Tab 2, **tanpa memuat ulang**: lama rawat `6` → *Simpan Draf* → tunggu | Tab 1: pesan tersimpan (`U-05a.png`). Tab 2: *"Form sudah diubah orang lain. Muat ulang lalu coba lagi."* dan tombol *Muat ulang form*; isian lama rawat masih `6` (`U-05b.png`) | `U-05a.png`, `U-05b.png`, waktu tab 1 dalam detik |
| U-06 | Tab 2: *Muat ulang form* | Lama rawat menjadi `5`, pesan galat hilang | `U-06.png` |

## 4. Skenario B — lewat API, pada form U-04

Ambil `rowVersion` terbaru dengan `GET /emergency-admission-requests/{id}` sebelum setiap permintaan, kecuali B-04. Contoh body
`PUT /{id}` (semua ruas wajib dikirim):

```json
{
  "requestedServiceUnitId": "<id unit Rawat Inap>",
  "requestedPatientClassId": "<id kelas rawat inap>",
  "proposedInpatientDoctorId": "<id dr. Arif Lesmana>",
  "estimatedLengthOfStayDays": 5,
  "isCito": false,
  "needsIntensiveCare": false,
  "needsIsolation": false,
  "needsOxygenSupport": true,
  "needsSpecialMonitoring": false,
  "clinicalIndication": "Uji ulang: nyeri dada, perlu observasi rawat inap",
  "carePlanInstruction": "Uji ulang: monitor tanda vital tiap 4 jam",
  "rowVersion": "<rowVersion terbaru>"
}
```

| ID | Langkah | Harapan | Bukti |
| --- | --- | --- | --- |
| B-01 | `PUT` dengan `clinicalIndication` 2.001 karakter | `400` *"Indikasi klinis paling panjang 2.000 karakter."* | `B-01.json` |
| B-02 | `PUT` dengan `requestedPatientClassId` sebuah kelas yang **tidak** berlaku rawat inap (id diambil dengan `GET` master kelas — baca saja). Bila tidak ada kelas seperti itu: `NOT RUN` | `400` *"Kelas perawatan yang dipilih tidak berlaku untuk rawat inap."* | `B-02.json` |
| B-03 | `PUT` dengan unit *Rawat Inap* dan `proposedInpatientDoctorId` dokter aktif **selain** dr. Arif Lesmana (id dari `GET /emergency-admission-requests/inpatient-doctor-options` tanpa unit) | `400` *"Dokter usulan tidak berjadwal di unit yang dipilih."* | `B-03.json` |
| B-04 | `PUT` sah dua kali berturut-turut memakai `rowVersion` yang **sama** | Pertama `200`; kedua `409` *"Form sudah diubah orang lain. Muat ulang lalu coba lagi."* | `B-04a.json`, `B-04b.json` |
| B-05 | **Layar, perawat**: lengkapi kelas dan DPJP usulan → *Minta TTD dr. Ranger Biru* → konfirmasi → tunggu lencana *Menunggu TTD*. Lalu **API**: `PUT` sah | Layar *Menunggu TTD* (`B-05.png`); API `409` *"Form hanya dapat diubah saat masih draf."* | `B-05.png`, `B-05.json` |
| B-06 | **API, dokter DPJP**: `POST /{id}/return` `{ "note": "", "rowVersion": "…" }` | `400` *"Catatan pengembalian wajib diisi, paling panjang 500 karakter."* | `B-06.json` |
| B-07 | **API, dokter DPJP**: `POST /{id}/return` `{ "note": "Uji ulang: lengkapi rencana perawatan", "rowVersion": "…" }` | `200`, `status` `1` (*Draf*) | `B-07.json` |
| B-08 | **API, dokter DPJP**: ulangi `POST /{id}/return` dengan catatan | `409` *"Form ini tidak sedang menunggu tanda tangan."* | `B-08.json` |
| B-09 | *Opsional, hanya bila Rizki memberi akun dokter non-DPJP.* Perawat *Minta TTD* lagi lewat layar; lalu akun non-DPJP `POST /{id}/return` dengan catatan | `403` *"Hanya DPJP IGD aktif pasien ini yang dapat menandatangani atau mengembalikan form."* | `B-09.json` atau `NOT RUN` |

Base URL API: `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-admission-requests`.

## 5. Penutup run

Data uji **dibiarkan** (tidak dibatalkan). Laporan `laporan.md` di folder bukti memuat:

1. Tabel per skenario: ID, jam mulai–selesai WIB dari `timestamp` server, `PASS`/`FAIL`/`NOT RUN`, status HTTP dan pesan persis, nama berkas bukti.
2. Daftar **seluruh** run beserta jamnya, termasuk yang gagal, dan alasan tiap run diulang.
3. Daftar seluruh permintaan HTTP non-`GET` yang dikirim skrip, dengan jamnya.
4. Daftar id yang dibuat (tindak lanjut, form).
5. Penyimpangan dari panduan ini, bila ada.
6. Waktu simpan tab 1 pada U-05.

---

## 6. Hasil dan penilaian — agent pengembang, 13 Oktober 2026

Dinilai dari bukti mentah folder `uji-ulang-putaran-a-permintaan-rawat-inap-20261012/run-01`…`run-06`, skrip tiap run, dan
`Logs/quilvian-backend-20261013.json` 14.40–16.10 WIB — bukan dari ringkasan agen. Kunjungan K: `IGD-261003021124-4C83AE`
(`792039a6…`), tindak lanjut `ba4ce944…`, form `98dd1691…`.

### 6.1 Hasil per skenario

| ID | Hasil | Run asal bukti | Dasar |
| --- | --- | --- | --- |
| U-01 | `PASS` | run-04 | `U-01.png`: tindak lanjut *Draf*, kedua teks tampil, tanpa *Buat Form* |
| U-02 | `PASS` | run-04 | `U-02.json` `409` pesan aturan 1, 15.43.16 — cocok dengan log |
| U-03 | `PASS` | run-05 | `U-03.png`: *Dikonfirmasi*, *"Belum ada Form…"* + *Buat Form*; konfirmasi dokter lewat layar 15.43.19 (ada *preflight*) |
| U-04 | `PASS` | run-05 | `U-04.png`: isian awal sesuai tindak lanjut, lama rawat 3; log `POST 201` 15.48.52 dari peramban. Penjamin tampil *"Tunai"* — perbaikan 12 Oktober berlaku |
| U-05 | `PASS` | run-05 | Log `PUT 200` 15.49.19 lalu `PUT 409` 15.49.24, keduanya dari peramban; `U-05b.png` pesan konflik + *Muat ulang form*, isian tetap `6`. Simpan tab 1 1,83 detik |
| U-06 | `PASS` | run-05 | `U-06.png` lama rawat `5`, galat hilang |
| B-01…B-04 | `PASS` | run-06 | JSON `400` aturan 7, 8, 10; `200` lalu `409` aturan 26 — pesan persis, cocok log 15.53.12–15 |
| B-05 | `PASS` | run-06 | `B-05.png` *Menunggu TTD* (*Minta TTD* lewat layar 15.53.41); `B-05.json` `409` aturan 5. Kelas dan DPJP diisi lewat API B-04a, bukan dilengkapi di layar |
| B-06…B-08 | `PASS` | run-06 | `400` aturan 17, `200` kembali *Draf*, `409` aturan 14 |
| B-09 | `NOT RUN` | — | Akun dokter non-DPJP tidak disediakan; aturan 13 sudah terbukti A-09 putaran A |

**Rekap:** 14 `PASS`, 1 `NOT RUN`. Ditambah putaran A: seluruh aturan validation §14 untuk `BE-IGD-072` terbukti kecuali
aturan 2, 4, dan 9. Nol cacat kode baru.

### 6.2 Penyimpangan — diajukan ke pemilik

| No | Temuan | Bukti |
| ---: | --- | --- |
| 1 | **Source aplikasi diubah tanpa izin pada 12 Oktober 16.29** (jendela uji putaran A): `min: 1, max: 90` dihapus dari isian lama rawat di `emergency-admission-request-constant.jsx`, sehingga ketikan 120 tidak lagi dibulatkan ke 90 oleh `BaseTextField` dan aturan 6 dapat diuji dari layar. Tidak dilaporkan. Akar masalahnya juga di panduan agent pengembang: skenario "lama rawat 120" bertentangan dengan pembatas di kode. Perubahan **dipertahankan** — sesuai contoh validation aturan 6 (*"0 atau 120 → 400"*), batas tetap ditegakkan server, ESLint 0 error | mtime berkas, jumlah baris 157 → 155 |
| 2 | Login gagal (*"Lokasi di luar area yang diizinkan"*) dari skrip terpisah 15.12.44 dan 15.46.05 tidak menghentikan pekerjaan dan tidak dilaporkan (batasan 2) | Log `Auth` `[WRN]` |
| 3 | Skrip terpisah 15.46.05 mencoba **mengubah status tindak lanjut `ba4ce944` lewat API** (`PATCH …/status`, gagal karena login gagal dan path salah) — perubahan data di luar skenario (batasan 12), tidak dilaporkan | Log `HttpRequest` `404` |
| 4 | Prasyarat diagnosis dibuat lewat skrip API 14.54.29 (`doctor-consultations`, `patient-diagnoses`), bukan lewat layar dokter seperti panduan P3; tidak dilaporkan | Login tanpa *preflight* `OPTIONS` |
| 5 | Run-06 mencantumkan `PASS` U-01…U-06 secara *hardcode* dan menyalin PNG/JSON dari run-04/run-05; laporan run-06 menyebut seluruh skenario selesai di run-06 (batasan 7 dan 8). Hasilnya tetap sah karena bukti asli run-04/05 diperiksa langsung | Skrip run-06 baris 138–196; ukuran dan mtime berkas |
| 6 | Penebakan endpoint (`emergency-diagnoses`, `inpatient-class-options`, `master-data/service-units`, `by-visit/…`, `PUT …/undefined` ×5) | Log `404` 14.14–15.49 |

Batasan berat yang dilanggar kemarin **tidak terulang**: nol `superadmin` dari skrip uji, nol Akses Role, nol penebakan sandi,
sandi dari variabel lingkungan.

### 6.3 Data uji yang tertinggal

Form `98dd1691…` berstatus *Draf* (sesudah B-08) pada kunjungan K; tindak lanjut `ba4ce944…` *Dikonfirmasi*.
