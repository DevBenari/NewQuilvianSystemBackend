# Laporan Uji Ulang DPJP Dokter Jaga IGD — `BE-IGD-056` dan `FE-IGD-037`

Tanggal pelaksanaan: 2026-10-11  
Pelaksana: Antigravity (Pair Programming AI Assistant)  
Panduan acuan: `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-11-panduan-uji-ulang-dpjp-dokter-jaga.md`  
Folder bukti mentah: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-dpjp-dokter-jaga-2026-10-11/`  
Keputusan acuan: `IGD-DEC-255`…`259` dan fakta `IGD-FACT-101`…`105`

---

## 1. Metadata Lingkungan Pengujian

| Komponen | Nilai / Kondisi | Catatan |
| --- | --- | --- |
| Tanggal Pengujian | 2026-10-11 | Putaran uji ulang skenario R-01 s.d. R-15 |
| Jam Mulai Putaran | 21:07 WIB | Dimulai dari tahap P0 (Pemeriksaan Awal) |
| Jam Selesai Putaran | 21:18 WIB | Selesai pada pemeriksaan R-14/R-15 |
| Server Frontend | Standalone Production Build (`node .next/standalone/server.js`) | Dilayani pada port 3000; `.next/BUILD_ID` bertanggal 11 Oktober 2026 20.51 WIB; terbukti `nextjs-portal === null` dan 0 lencana "N" di seluruh sesi layar |
| Layanan Backend | `https://localhost:7184` | ASP.NET Core Kestrel (.NET 9) berjalan dari build pemilik |
| Akun Pengujian | `dimas.kurniawan@rsmmc.local` | Peran Perawat IGD dari `process.env.QUILVIAN_PERAWAT_EMAIL` (bukan SuperAdmin, `isSuperAdmin: false`) |
| Viewport Browser Layar | 1440 × 900 (DPR 1.0) | Playwright Chromium (Google Chrome channel), sidebar menu terbuka |
| Header Otorisasi | Disamarkan (`Bearer [REDACTED]`, `Cookie: [REDACTED]`) | Seluruh rekaman jaringan (`*-network.json`) dan bukti API menyamarkan kredensial |
| Modifikasi Data Master & SQL | Nol (0) | Tidak ada modifikasi data master/jadwal/role, nol kueri SQL |

---

## 2. Pemeriksaan Awal (P0)

Pemeriksaan awal terhadap status tiga kunjungan acuan sebelum putaran R-01…R-15 dimulai:

| Kunjungan | Encounter ID | No. Kunjungan | Pasien (Layar/Data) | Status Riwayat Awal | Evaluasi P0 |
| :--- | :--- | :--- | :--- | :--- | :---: |
| **V1** | `7b4a5e54-d9ec-49b2-8249-eca1769588cb` | `IGD-261003021124-4C83AE` | `Rivaldo Januar` | 2 baris riwayat; DPJP aktif `dr. Ranger Biru` (D2), `isRosterOverride=true`, alasan *"Dua dokter jaga sedang resusitasi"* | **PASS** |
| **V2** | `e61307aa-cfb8-4292-aca6-1068c1b1dffe` | `IGD-261001033716-45CD48` | `Indra Gunawan` | 1 baris riwayat; DPJP aktif `dr. Ranger Biru` (D2), `isRosterOverride=true` | **PASS** |
| **V3** | `8051077d-162b-4ba0-9e3e-24c44aa2896e` | `IGD-261001034358-B98C47` | `An. Rizky` | 0 baris riwayat; belum memiliki DPJP | **PASS** |

Bukti tersimpan di [`data-uji.json`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-dpjp-dokter-jaga-2026-10-11/data-uji.json). Seluruh kondisi awal cocok 100% dengan prasyarat panduan.

---

## 3. Matriks Hasil Pengujian Uji Ulang (R-01…R-15)

| ID | Skenario / Langkah | Tipe | Hasil | Status HTTP / Kondisi Teramati | Berkas Bukti |
| :--- | :--- | :---: | :---: | :--- | :--- |
| **R-01** | `GET /on-duty-doctors?onlyOnDuty=true` | API | **PASS** | HTTP 200; Setiap baris bernilai `isOnDuty=true`; nol baris `isOnDuty=false`. | `R-01-api.json` |
| **R-02** | `GET /on-duty-doctors?search=` (*Ranger*, *Farhan*, *Maya*) | API | **PASS** | HTTP 200; *Ranger* menampilkan D2 (`isOnDuty=false`) tanpa D1; *Farhan* menampilkan D1 (`isOnDuty=true`) tanpa baris non-duty; *Maya* tidak memuat D3. | `R-02-api.json` |
| **R-03** | `GET /active?emergencyVisitId={V1}` | API | **PASS** | HTTP 200; `doctorId` = D2, `isRosterOverride=true`, `rosterOverrideReason` = *"Dua dokter jaga sedang resusitasi"*. | `R-03-api.json` |
| **R-04** | Cari baris penugasan dengan `effectiveFrom` sebelum `2026-10-11T00:00:00Z` | API | **PASS** | Ditemukan pada kunjungan `9677ceea-f874-47c1-8d76-258f51298a7d` baris tanggal `2026-10-09T03:05:11.961659Z` dengan `isRosterOverride=false` dan `rosterOverrideReason=null`. | `R-04-api.json` |
| **R-05** | Urutan validasi penolakan (a) POST V1 + D2 tanpa alasan, (b) POST V1 + D1 | API | **PASS** | (a) HTTP 400 persis *"Dokter ini tidak sedang jaga IGD pada waktu penugasan. Isi alasan memilih dokter di luar jadwal jaga."* (bukan 409). (b) HTTP 409 persis *"Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter."*. Riwayat V1 tetap 2 baris. | `R-05-api.json` |
| **R-06** | `POST /{activeId V2}/handover` ke D1 dengan alasan override terisi | API | **PASS** | HTTP 200; Aturan 11 terbukti: penugasan ke dokter jaga (D1) mengabaikan alasan override (tersimpan `isRosterOverride=false` dan `rosterOverrideReason=null`), `assignmentReason` tersimpan *"Uji aturan 11"*. | `R-06-api.json` |
| **R-08** | Layar Pintu A dan Pintu B untuk V1 | Layar | **PASS** | Pintu A: banner DPJP memuat baris persis *"Di luar jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"*. Pintu B: ruas Dokter PJ IGD memuat *"dr. Ranger Biru • Di luar jadwal jaga — alasan: Dua dokter jaga sedang resusitasi"*. | `R-08-pintuA.png`<br>`R-08-pintuA-network.json`<br>`R-08-pintuB.png`<br>`R-08-pintuB-network.json` |
| **R-09** | Layar Pintu A dan Pintu B untuk V2 (pasca R-06, sebelum R-07) | Layar | **PASS** | Pintu A: banner DPJP menampilkan `dr. Farhan Malik` TANPA baris *Di luar jadwal jaga*. Pintu B: ruas Dokter PJ IGD hanya menampilkan *"dr. Farhan Malik"*. | `R-09-pintuA.png`<br>`R-09-pintuA-network.json`<br>`R-09-pintuB.png`<br>`R-09-pintuB-network.json` |
| **R-07** | `POST /{activeId V2 dari R-06}/handover` ke D2 dengan spasi pada override reason | API | **PASS** | HTTP 200; Pemangkasan server terbukti: `isRosterOverride=true`, `rosterOverrideReason` persis *"Uji pemangkasan alasan di server"* (spasi di kedua ujung dipangkas backend). | `R-07-api.json` |
| **R-10** | Layar Pintu A untuk V2 (pasca R-07) | Layar | **PASS** | Banner DPJP memuat baris persis *"Di luar jadwal jaga — alasan: Uji pemangkasan alasan di server"*. | `R-10-pintuA.png`<br>`R-10-pintuA-network.json` |
| **R-11** | Layar Pintu A V3: validasi alasan kosong & batas 500 karakter | Layar | **PASS** | Memilih D2 memunculkan isian alasan; saat kosong tombol simpan nonaktif dan pesan *"Alasan memilih dokter di luar jadwal jaga wajib diisi."* tampil; input 501 huruf A terpotong menjadi 500 karakter (`maxlength="500"`). Nol POST dikirim. | `R-11-kosong.png`<br>`R-11-500.png`<br>`R-11-network.json` |
| **R-12** | Layar Pintu A V3: intersepsi galat 500 daftar dokter | Layar | **PASS** | `page.route` mem-fulfill 500; modal menampilkan pesan galat kegagalan muat daftar dokter; tidak ada dokter yang dapat dipilih; nol permintaan ke `/corporate/human-resource/master-data/doctors`. | `R-12.png`<br>`R-12-network.json` |
| **R-13** | Layar Pintu A V3: intersepsi penolakan 400 spesialis & reload daftar | Layar & API | **PASS** | POST diintersepsi mengubah `doctorId` menjadi spesialis D3; backend membalas HTTP 400 dengan pesan persis; modal tetap terbuka menampilkan pesan tersebut; daftar dokter dimuat ulang pasca galat (`GET on-duty-doctors` terpicu otomatis); riwayat V3 tetap 0 baris. | `R-13.png`<br>`R-13-network.json`<br>`R-13-api.json` |
| **R-14** | Jendela tanpa jadwal: `GET /on-duty-doctors` tanpa `at` | API | **NOT RUN** | Pada jam uji (21:18 WIB), shift dinas malam aktif berjalan (`dr. Farhan Malik` Shift Malam 21:00 - 07:00). Berdasarkan panduan pasal 96: dicatat `NOT RUN` beserta jamnya tanpa mengubah jadwal master dev. | `R-14-api.json` |
| **R-15** | Jendela tanpa jadwal: Layar Pintu A V3 penetapan saat roster kosong | Layar | **NOT RUN** | Mengikuti prasyarat R-14 yang tidak terpenuhi pada jam uji; dicatat `NOT RUN` sesuai klausul pasal 96. | `percobaan.json` |

---

## 4. Daftar Seluruh Percobaan (`percobaan.json`)

Sesuai Aturan 8, 9, dan 11, setiap sesi percobaan dicatat transparan beserta jam mulai, jam selesai, jumlah request, dan alasan perbaikan:

| No | Skrip | Tujuan / Skenario | Jam Mulai (WIB) | Jam Selesai (WIB) | Request | Hasil | Alasan / Catatan |
| ---: | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| 1 | `run-p0.mjs` | P0 — Pemeriksaan awal status V1, V2, V3 | 21:07:22 | 21:07:28 | 4 | **PASS** | Keadaan awal V1, V2, dan V3 cocok 100% dengan panduan. |
| 2 | `run-r01-to-r06.mjs` | R-01 (API: `onlyOnDuty=true`) | 21:08:28 | 21:08:29 | 1 | **PASS** | Respons 200 dengan setiap baris `isOnDuty=true`. |
| 3 | `run-r01-to-r06.mjs` | R-02 (API: pencarian dokter) | 21:08:29 | 21:08:35 | 3 | **PASS** | Filter `?search=` memfilter dokter sesuai harapan. |
| 4 | `run-r01-to-r06.mjs` | R-03 (API: `GET /active`) | 21:08:35 | 21:08:36 | 1 | **PASS** | V1 DPJP aktif D2 override alasan tersimpan. |
| 5 | `run-r01-to-r06.mjs` | R-04 (API: baris historis pra-11 Oktober) | 21:08:36 | 21:08:45 | 2 | **PASS** | Baris historis tanggal 09-10-2026 bernilai `false`/`null`. |
| 6 | `run-r01-to-r06.mjs` | R-05 (API: urutan validasi 400 vs 409) | 21:08:45 | 21:08:48 | 3 | **PASS** | Penolakan 400 mendahului 409; riwayat V1 tetap 2 baris. |
| 7 | `run-r01-to-r06.mjs` | R-06 (API: handover aturan 11 dokter jaga) | 21:08:48 | 21:08:49 | 2 | **PASS** | Alasan override diabaikan saat menugaskan dokter jaga. |
| 8 | `run-r08.mjs` | R-08 (Layar Pintu A & B untuk V1) | 21:09:39 | 21:10:15 | 20 | **PASS** | Banner DPJP Pintu A dan Pintu B menampilkan teks penanda override persis sesuai harapan. |
| 9 | `run-r09.mjs` | R-09 (Layar Pintu A & B untuk V2 pasca R-06) | 21:10:58 | 21:11:26 | 20 | **PASS** | Banner DPJP V2 menampilkan dokter jaga tanpa teks override. |
| 10 | `run-r07.mjs` | R-07 (API: handover pemangkasan server) | 21:11:47 | 21:11:57 | 3 | **PASS** | Spasi di kedua ujung dipangkas oleh server. |
| 11 | `run-r10.mjs` | R-10 (Layar V2 Pintu A pasca R-07) | 21:12:24 | 21:12:36 | 11 | **PASS** | Banner DPJP V2 menampilkan teks override hasil pangkasan server. |
| 12 | `run-r11.mjs` | R-11 (Layar V3 Pintu A: validasi & batas 500) | 21:13:07 | 21:13:20 | 11 | **PASS** | Validasi tombol saat kosong dan pembatasan 500 karakter terbukti. |
| 13 | `run-r12.mjs` | R-12 (Layar V3 Pintu A: galat 500 dokter) | 21:13:49 | 21:14:00 | 11 | **PASS** | Pesan galat tampil di modal, nol fallback ke master data. |
| 14 | `run-r13.mjs` | R-13 Percobaan 1 (Layar V3 Pintu A penolakan 400) | 21:14:36 | 21:14:50 | 7 | **SCRIPT ERROR** | Strict mode violation pada selector dialog Playwright (`locator('.modal-dialog, [role="dialog"]')` menemukan 2 elemen). Diperbaiki pada `run-r13-p2.mjs`. |
| 15 | `run-r13-p2.mjs` | R-13 Percobaan 2 (Layar V3 Pintu A penolakan 400) | 21:15:30 | 21:15:56 | 12 | **SCRIPT ERROR** | Browser ditutup (`browser.close()`) saat request POST sedang diproses backend sehingga koneksi terputus (`OperationCanceledException`). Diperbaiki pada `run-r13-p3.mjs` dengan `page.waitForResponse`. |
| 16 | `run-r13-p3.mjs` | R-13 Percobaan 3 (Layar V3 Pintu A penolakan 400) | 21:17:19 | 21:17:39 | 13 | **PASS** | Penolakan 400 tampil apa adanya di modal, modal tetap terbuka, daftar dokter dimuat ulang otomatis, dan riwayat V3 tetap 0 baris. |
| 17 | `run-r14-r15.mjs` | R-14 (API: roster kosong) | 21:18:34 | 21:18:46 | 2 | **NOT RUN** | Shift Malam (21.00–07.00) dr. Farhan Malik sedang aktif; dicatat NOT RUN sesuai pasal 96 panduan. |
| 18 | `run-r14-r15.mjs` | R-15 (Layar V3: roster kosong) | 21:18:34 | 21:18:46 | 0 | **NOT RUN** | Mengikuti status R-14; dicatat NOT RUN sesuai pasal 96 panduan. |

---

## 5. Pernyataan Kepatuhan Aturan yang Mengikat (Aturan 1–11)

1. **Aturan 1 (Build Produksi Port 3000)**: Terbukti dilayani oleh `node .next/standalone/server.js` (`.next/BUILD_ID` pukul 20.51 WIB). Seluruh rekaman membuktikan `nextjs-portal === null` dan tidak ada lencana "N" di pojok kiri bawah.
2. **Aturan 2 (Dimensi Layar & Jeda Screenshot)**: Viewport 1440 × 900, sidebar terbuka, tangkapan layar diambil minimal 1000–1500 ms sesudah respons terakhir.
3. **Aturan 3 (Akun Perawat IGD)**: Login murni dengan akun `dimas.kurniawan@rsmmc.local`. Nol permintaan ke `/api/v1/administrator/**`.
4. **Aturan 4 (Kerahasiaan Kredensial)**: Nol sandi atau token di skrip, JSON bukti, maupun laporan ini. Header `Authorization` dan `Cookie` disamarkan menjadi `[REDACTED]`.
5. **Aturan 5 (Integritas Data Master & Tanpa SQL)**: Nol modifikasi data master, jadwal dokter, unit layanan, atau role access. Nol kueri SQL dijalankan.
6. **Aturan 6 (Batasan Penulisan Data)**: Penulisan data yang terjadi hanya pada R-05 (dua POST ditolak 400 & 409), R-06 (handover berhasil 200), R-07 (handover berhasil 200), dan R-13 (POST ditolak 400). Nol penulisan data liar.
7. **Aturan 7 (Pencatatan Seluruh Permintaan)**: Setiap permintaan dicatat di `percobaan.json` dan rekaman jaringan, tanpa ada pemindaian `?at=` liar di luar skenario.
8. **Aturan 8 (Penyimpanan Seluruh Versi Skrip & URL Jaringan)**: Seluruh versi skrip (`run-p0.mjs` s.d. `run-r14-r15.mjs`, termasuk `run-r13.mjs`, `run-r13-p2.mjs`, dan `run-r13-p3.mjs`) disimpan utuh di folder bukti tanpa ada yang ditimpa. Rekaman jaringan mencakup seluruh URL `/api/v1/`.
9. **Aturan 9 (Kecocokan Log & Nama Pasien Riil)**: Daftar percobaan memuat jam mulai-selesai WIB yang cocok dengan log backend. Nama pasien diverifikasi langsung dari antarmuka layar dan API (`Rivaldo Januar`, `Indra Gunawan`, `An. Rizky`).
10. **Aturan 10 (Penggunaan `page.route`)**: `page.route` hanya digunakan pada skenario R-12 (mock 500) dan R-13 (mock 400 spesialis) persis sesuai kode panduan.
11. **Aturan 11 (Distribusi Status & Tanpa Frasa Terlarang)**: Kegagalan skrip diklasifikasikan sebagai `SCRIPT ERROR` dan dilaporkan transparan. Laporan tidak memuat frasa terlarang.
