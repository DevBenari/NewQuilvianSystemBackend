# Laporan Live Browser Testing — Admisi Bayi Baru Lahir (Rawat Gabung, Perinatologi, NICU)

| Hal | Isi |
|---|---|
| Menu yang diuji | Rawat Inap → Admisi Rawat Inap → **Admisi Bayi Baru Lahir** |
| Alamat layar | `http://localhost:3000/health-services/inpatient-management/admissions/newborn` |
| Sub-modul blueprint | `rawat-inap/episode-rawat-inap` |
| Tanggal uji | Minggu, 25 Oktober 2026, pukul 11.05–11.35 WIB |
| Penguji | AI Agent (Claude Code) — seluruh klik, isian, dan pemeriksaan dilakukan agent |
| Browser | Google Chrome (engine Chromium) lewat `playwright.chromium`, mode tampil di layar (*headed*), `slowMo` 450 ms, layar 1920×945 |
| Frontend | `http://localhost:3000` — cabang `HamzahV2`, commit `9cad353ba` |
| Backend | `https://localhost:7184/api` — cabang `MHamzah`, commit `985a1fc7` |
| Basis data | `QuilvianNewDevHamzah` (PostgreSQL pengembang). Agent **tidak** menjalankan `INSERT`/`UPDATE`/`DELETE` langsung; semua data uji dibuat lewat layar aplikasi. |
| Akun | `superadmin@admin.com` (kata sandi tidak dicatat di dokumen ini) |
| Data pasien uji | Disiapkan pemilik proses: 3 ibu utama (RM 00-00-00-68, -69, -70) dan 3 ibu cadangan (RM 00-00-00-65, -66, -67) |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: alur admisi bayi berjalan tuntas untuk ketiga jalur perawatan, tetapi belum layak dipakai di lapangan sebelum dua temuan keselamatan pasien diperbaiki.**

Yang sudah berjalan baik:

- Ketiga jalur — **Rawat Gabung**, **Perinatologi**, dan **NICU** — berhasil dari pemilihan ibu sampai kartu pasien, tanpa layar putih, tanpa *crash*, dan tanpa satu pun galat server (0 dari 204 panggilan API berstatus 5xx).
- Server membuat nama otomatis "By. Ny. [Nama Ibu]", nomor rekam medis, dan urutan kelahiran dengan benar.
- Penjamin bayi mengikuti penjamin ibu (BPJS Kesehatan, Tunai, dan Mandiri Inhealth — ketiganya terbukti).
- Bed yang dipesan berubah menjadi **Terisi** atas nama bayi yang benar, dan data ibu langsung menampilkan bayinya.
- Sebagian besar pengaman input bekerja: form kosong ditolak, tanggal lahir besok ditolak, klik ganda tidak membuat pesanan ganda, persetujuan tanpa tanda tangan tidak bisa disimpan, pendaftaran bayi kembar wajib dikonfirmasi, dan tombol Batal membersihkan form tanpa meninggalkan data.

Yang harus diperbaiki lebih dulu:

| No | Temuan | Tingkat | Dampak di rumah sakit |
|---|---|---|---|
| ISU-01 | **Gelang identitas bayi mencetak nama yang salah**: awalan "NY." ganda, dan nama bayi yang diisi petugas diabaikan. Gelang dan label bayi yang sama memuat dua nama berbeda. | P1 · **Kritis** | Risiko salah identifikasi bayi (Sasaran Keselamatan Pasien 1). |
| ISU-02 | Untuk bayi **Perinatologi**, server juga menawarkan **boks di kamar ibu lain** dan **bed NICU**, dan keduanya bisa dipesan. | P1 · **Tinggi** | Bayi dapat ditempatkan di kamar ibu yang bukan ibunya, atau memakai bed intensif yang seharusnya disiapkan untuk bayi kritis. |
| ISU-03 | Backend memeriksa "tanggal lahir di masa depan" memakai **tanggal UTC**. Bayi yang lahir pukul **00.00–06.59 WIB** akan ditolak. | P1 · **Tinggi** (bukti kode) | Bayi yang lahir dini hari tidak bisa didaftarkan sampai pukul 07.00 WIB. |
| ISU-04 | Tanggal/jam lahir yang tidak masuk akal tetap diterima (misalnya pukul 23.50 hari ini saat jam baru 11.15, atau tahun 1990). | P2 · Sedang | Data kelahiran palsu atau keliru masuk ke rekam medis. |

Rincian seluruh 10 temuan ada di Bagian 6.

---

## 2. Alur Proses Bisnis Admisi Bayi (Sesuai Layar)

**Pemicu:** seorang ibu menjalani persalinan sesar (kasus operasi obstetri berstatus *Sedang berlangsung* atau *Selesai*), lalu petugas admisi mendaftarkan bayinya ke rawat inap.

**Contoh nyata dari uji ini:** Ny. Anggraini Puspita (RM 00-00-00-68) dirawat di VIP 303 setelah SC. Bayinya lahir pukul 09.30, bugar (APGAR 8/9), dan dokter anak memutuskan rawat gabung. Petugas mendaftarkan bayi itu dan menempatkannya di boks bayi VIP 303.

| Langkah | Nama di layar | Apa yang dilakukan petugas | Apa yang dilakukan sistem |
|---|---|---|---|
| 1 | Pilih Ibu Persalinan | Mencari ibu memakai nama, No. RM, atau nomor kasus | Menampilkan kasus, lokasi ibu, penjamin, jumlah bayi terdaftar, dan pratinjau "Bayi ke-n" |
| 2 | Identitas Bayi | Mengisi tanggal dan jam lahir, jenis kelamin, tempat lahir; nama dan NIK boleh kosong | Membuat pasien bayi, **No. RM**, nama otomatis "By. Ny. [Nama Ibu]", dan urutan kelahiran |
| 3 | Perawatan dan DPJP Bayi | Memilih Rawat Gabung / Perinatologi / NICU, mengisi sumber keputusan klinis, memilih DPJP | Memvalidasi pilihan (rawat gabung wajib punya episode ibu yang aktif) |
| 4 | Penempatan dan Pemesanan | Rawat Gabung: konfirmasi boks di kamar ibu. Perinatologi/NICU: memilih bed di denah | Membuat encounter dan episode *Draft*, lalu menahan bed (reservasi ±2 jam) |
| 5 | Persetujuan Wali | Mengisi data orang tua/wali, meninjau 12 klausul, membubuhkan dan mengunci tanda tangan | Menyimpan persetujuan umum rawat inap (wajib orang tua/wali untuk bayi) |
| 6 | Tinjau dan Aktifkan | Memeriksa ringkasan, lalu menekan **Aktifkan Admisi Bayi** | Mencatat penempatan; episode menjadi **Admitted** dan bed menjadi **Terisi** |
| 7 | Dokumen Bayi | Mencetak gelang dan label | Menyusun gelang dan label dari data server |
| 8 | Kartu Pasien | Mencetak kartu | Menampilkan identitas akhir bayi |

**Hasil akhir yang diharapkan:** bayi tercatat sebagai pasien rawat inap aktif yang tertaut ke ibu dan kasus persalinannya, menempati bed yang sesuai kebutuhan klinis, dengan penjamin mengikuti ibu dan gelang identitas yang benar.

---

## 3. Hasil Tiga Jalur Perawatan

| Jalur | Ibu (RM · Kasus) | Penjamin ibu | Bayi yang terbentuk | Bed yang ditargetkan → hasil | Status akhir di server |
|---|---|---|---|---|---|
| 1. Rawat Gabung | Ny. Anggraini Puspita (00-00-00-68 · OPR-OBS-261023-068) | BPJS Kesehatan | **By. Ny. Anggraini Puspita** · RM **00-00-00-71** · Perempuan · Bayi ke-1 | 303-BOKS di VIP 303 → **303-BOKS, Terisi** ✅ | Episode RI-261025041519-F8366C **Admitted**, kelas VIP, penjamin BPJS |
| 2. Perinatologi | Ny. Rina Marlina (00-00-00-69 · OPR-OBS-261023-069) | Tunai / Pribadi | **Bagas Pratama** (nama diisi petugas) · RM **00-00-00-72** · Laki-laki · Bayi ke-1 | BAYI-1-A (R. Bayi Lt. 2) → **BAYI-1-A, Terisi** ✅ | Episode RI-261025042436-D614FC **Admitted**, kelas PERINATOLOGI, unit Ruang Bayi |
| 3. NICU | Ny. Tri Wahyuni (00-00-00-70 · OPR-OBS-261023-070) | Mandiri Inhealth | **By. Ny. Tri Wahyuni** · RM **00-00-00-73** · Laki-laki · Bayi ke-1 | NICU-1-B (Ruang NICU Lt. 2) → **NICU-1-B, Terisi** ✅ | Episode RI-261025042902-1FF0D0 **Admitted**, kelas NICU, intensif |

Setelah ketiga admisi, daftar kandidat ibu menampilkan masing-masing ibu dengan "1 bayi, aktif" pada lokasi yang benar dan pratinjau "Bayi ke-2". Artinya data ibu dan bayi tidak tertukar.

---

## 4. Rincian Test Case

Keterangan hasil: ✅ Lulus · ❌ Gagal · ⚠️ Lulus dengan catatan / perlu keputusan.

### P0 — Fungsi Kritis

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| TC-01 | Login dan membuka layar Admisi Bayi | `POST /auth/login` 200; layar "Langkah 1 dari 8" tampil tanpa galat halaman | ✅ | `00`, `01`, `02` |
| TC-02 | Jalur Rawat Gabung sampai kartu pasien | Semua langkah tersimpan (identitas 200, encounter 200, episode 200, reservasi 200, persetujuan 201, penempatan 200) | ✅ | `10`–`39` |
| TC-03 | Jalur Perinatologi sampai kartu pasien | Idem; bed BAYI-1-A terkonfirmasi | ✅ | `42`–`59` |
| TC-04 | Jalur NICU sampai kartu pasien | Idem; bed NICU-1-B terkonfirmasi | ✅ | `70`–`80` |
| TC-05 | Tidak ada galat server atau *crash* | 0 respons 5xx dari 204 panggilan API; 0 *page error* JavaScript | ✅ | log jaringan |
| TC-06 | Data ibu–bayi tidak tertukar | Episode, bed, dan daftar ibu di server menunjuk pasangan yang benar | ✅ | verifikasi API (Bagian 3) |

### P1 — Kepatuhan Bisnis dan Keselamatan

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| TC-07 | Nama, No. RM, dan urutan kelahiran dibuat server | "By. Ny. Anggraini Puspita", RM 00-00-00-71, Bayi ke-1 | ✅ | `22`, `35` |
| TC-08 | Penjamin bayi mengikuti ibu | BPJS / Tunai / Mandiri Inhealth tampil di ringkasan; encounter membawa `inheritedPaymentSourceId` milik ibu | ✅ | `35`, `55`, `77` |
| TC-09 | Persetujuan bayi wajib oleh orang tua/wali | Pilihan "Pasien Sendiri" dinonaktifkan, dengan penjelasan hukum | ✅ | `29` |
| TC-10 | Rawat gabung hanya di boks kamar ibu | Hanya 303-BOKS di VIP 303 yang ditawarkan | ✅ | `26` |
| TC-11 | NICU hanya bed intensif | Server hanya mengembalikan NICU-1-B | ✅ | `73`, `74` |
| TC-12 | Perinatologi hanya bed perinatologi | Server juga menawarkan NICU-1-B, 304-BOKS, dan **305-BOKS (kamar Ny. Tri Wahyuni)** — semuanya bisa dipesan | ❌ ISU-02 | `45`, `46` |
| TC-13 | Gelang identitas sama dengan identitas pasien | Gelang "BY. NY. NY. …" di 3/3 jalur; nama khusus "Bagas Pratama" tidak muncul di gelang | ❌ ISU-01 | `37`, `58`, `79` |
| TC-14 | Pendaftaran bayi kembar harus dikonfirmasi | Tanpa centang konfirmasi muncul pesan "…konfirmasi pendaftaran bayi kembar (Bayi ke-2)…", 0 request tulis | ✅ | `90` |
| TC-15 | DPJP bayi adalah dokter anak | Daftar memuat 11 dokter semua spesialisasi (Gigi, Ortopedi, Jantung, dll.) tanpa peringatan | ⚠️ ISU-06 | `23`, `25` |
| TC-16 | Wali menyetujui klausul secara aktif | 12 klausul sudah tercentang sejak awal | ⚠️ ISU-07 | `29` |

### P2 & P3 — Fungsi, Validasi, dan Data Ngasal

Uji yang berisiko membuat pasien sampah dijalankan dengan **pengaman pencegat**: bila frontend meloloskan isian, request dihentikan di browser sebelum sampai ke server. Pesan "Network Error" pada screenshot uji tersebut berasal dari pengaman ini, bukan dari aplikasi.

| ID | Skenario | Masukan | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|---|
| TC-17 | Lanjut tanpa memilih ibu | — | "Pilih ibu dari kasus operasi persalinan sebelum melanjutkan." | ✅ | `03` |
| TC-18 | Identitas kosong | — | Tiga pesan wajib isi (tanggal, jam, jenis kelamin); tidak ada request | ✅ | `12` |
| TC-19 | Tanggal lahir besok | 26 Okt 2026 | "Tanggal lahir tidak boleh di masa depan." | ✅ | `16` |
| TC-20 | Jam lahir yang belum terjadi | Hari ini 23.50 (jam saat uji ±11.15) | **Diterima** frontend, request terkirim | ❌ ISU-04 | `18` |
| TC-21 | Tanggal lahir absurd | 01 Jan 1990 | **Diterima** frontend, request terkirim | ❌ ISU-04 | `20` |
| TC-22 | NIK bayi tidak valid | `32abcd…` / 10 digit / 20 digit | Huruf dan 10 digit ditolak saat simpan; 20 digit dipotong menjadi 16 | ✅ (catatan: huruf masih bisa diketik) | `17` |
| TC-23 | Nama panjang dan skrip | 130 huruf; `<img src=x onerror=…>` | Dipotong 100 karakter; skrip **tidak** dieksekusi; tag HTML tetap diterima sebagai nama | ⚠️ ISU-10 | `21` |
| TC-24 | Perawatan, sumber keputusan, DPJP kosong | — | Ditahan di langkah 3, pesan "Pilih DPJP bayi." | ✅ | `24`, `25` |
| TC-25 | Klik ganda "Pesan Boks Bayi & Lanjut" | *double click* | Hanya **satu** encounter, satu episode, satu reservasi | ✅ | `27`, `28` |
| TC-26 | Persetujuan tidak lengkap | Kanvas kosong; 1 klausul dibatalkan | Tombol Kunci dan Simpan nonaktif; penghitung "11 Dari 12 Disetujui" | ✅ | `31`, `32` |
| TC-27 | No. HP penanda tangan | `08abc12345678901234` | Huruf dibuang, dibatasi 13 digit | ✅ | `31` |
| TC-28 | NIK penanda tangan sampah | `ABCDE<script>99999999999999` | **Diterima** frontend; backend hanya membatasi 50 karakter | ❌ ISU-05 | `33` |
| TC-29 | Batal Pendaftaran sebelum simpan | Ibu cadangan Ny. Nurul Aini | Konfirmasi muncul, 0 request tulis, kembali ke menu, ibu tetap 0 bayi | ✅ | `60`–`62` |
| TC-30 | State tidak terbawa antar admisi | "Admisi Baru" setelah Jalur 1 | Form kembali kosong; tidak ada ibu/identitas lama | ✅ | `40`–`42` |

### P4 — Tampilan

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| TC-31 | Picker tanggal lahir di layar 1920×945 | Popover terbuka ke bawah dan terpotong; baris 25–31 dan tombol "Hari ini" tidak terjangkau | ❌ ISU-08 | `19` |
| TC-32 | Konfirmasi boks rawat gabung | "Tempat Tidur Pasien Ibu: Belum terbaca" padahal ibu di 303-A | ❌ ISU-09 | `26` |

### P5 — Kenyamanan Pemakaian

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| TC-33 | Kejelasan teks dan umpan balik | Enam catatan kecil (lihat ISU-10) | ⚠️ ISU-10 | beberapa |

**Rekap:** 33 test case — 23 lulus, 6 gagal, 4 lulus dengan catatan / perlu keputusan.

---

## 5. Endpoint yang Terlibat (Gaya Swagger)

Semua endpoint memakai otorisasi `[Authorize]` (cookie/token sesi login).

### `[Tags("Health Services / Inpatient Management / Inpatient Episode")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/episodes/newborn-mother-candidates` | Daftar ibu dari kasus persalinan yang dapat dipakai | `search`, `pageNumber`, `pageSize` | 200 — `deliveryCaseNumber`, `motherName`, `currentRoomName`, `paymentSourceName`, `registeredBabyCount`, `registeredBabies[]` |
| GET | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility` | Cek kelayakan admisi pasien | `patientId` | 200 |
| POST | `/api/v1/health-services/inpatient-management/episodes` | Membuka episode *Draft* | encounter, DPJP, `newbornAdmission` (careMode, careSource, requiresIntensiveCare), `motherEpisodeId` | 200 "Admisi berhasil dibuka." |
| GET | `/api/v1/health-services/inpatient-management/episodes/{id}` | Detail episode | — | 200 — `episodeStatus` 1 "Admitted", `newbornAdmission`, `motherEpisodeNumber` |

### `[Tags("Health Services / Patient Management / Master Data / Patient")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| POST | `/api/v1/health-services/patient-management/master-data/patients/newborns` | Mendaftarkan pasien bayi (header `Idempotency-Key`) | `motherPatientId`, `deliveryCaseId`, `fullName?`, `birthDate`, `birthTime`, `gender`, `birthPlace?`, `identityNumber?` | 200 "Pasien bayi berhasil didaftarkan." — `medicalRecordNumber`, `fullName`, `birthOrder` |

### `[Tags("Health Services / Registration Management / Patient Encounter")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| POST | `/api/v1/health-services/registration-management/patient-encounters/admin` | Membuat encounter rawat inap bayi | `patientId`, `serviceUnitId`, `patientClassId`, `doctorId`, `paymentType`, `inheritedPaymentSourceId` | 200 "Transaksi kunjungan pasien berhasil dibuat." — ENC-RSMMC-00216/00217/00218 |

### `[Tags("Health Services / Inpatient Management / Bed Occupancy")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Bed yang layak untuk pasien | `isForNewborn`, `newbornCareMode`, `motherEpisodeId`, `requiresIntensiveCare`, `includeIneligible` | 200 — daftar bed dengan `isBookable` (lihat ISU-02) |
| GET | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Papan tempat tidur | `isForNewborn`, `roomId?` | 200 |
| POST | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations` | Menahan bed | `episodeId`, `bedId`, versi bed | 200 "Pemesanan tersimpan." |
| POST | `/api/v1/health-services/inpatient-management/bed-occupancies/placements` | Aktivasi penempatan | `episodeId`, `bedId`, `expectedBedVersion` | 200 "Penempatan tersimpan." |

### `[Tags("InpatientAdmissionConsent")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| POST | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` | Menyimpan persetujuan umum dan tanda tangan | `signerType` (Parent/Guardian), `signerName`, `signerIdentityNumber`, `agreedClauses[]`, `signatureImageBase64` | 201 "Formulir persetujuan rawat inap dan tanda tangan digital berhasil disimpan." |

Catatan: nama tag `InpatientAdmissionConsent` belum mengikuti pola tag modul lain ("Health Services / Inpatient Management / …").

**Ringkasan status jaringan selama sesi:** GET 200 ×179, POST 200 ×16, POST 201 ×3, GET 404 ×3, GET 409 ×3, 5xx ×0.

---

## 6. Temuan Isu

### ISU-01 — Gelang identitas bayi mencetak nama yang salah · P1 · Kritis

**Apa yang terjadi.** Di ketiga jalur, gelang bayi tercetak dengan awalan "NY." ganda. Pada Jalur 2, petugas mengisi nama "Bagas Pratama"; kartu pasien dan label memakai nama itu, tetapi gelang tetap memakai nama ibu.

| Jalur | Nama di data pasien / kartu / label | Nama di gelang |
|---|---|---|
| 1 | By. Ny. Anggraini Puspita / "BY. NY. ANGGRAINI PUSPITA / BAYI 1" | **BY. NY. NY. ANGGRAINI PUSPITA / BAYI 1** |
| 2 | Bagas Pratama / "BAGAS PRATAMA / BAYI 1" | **BY. NY. NY. RINA MARLINA / BAYI 1** |
| 3 | By. Ny. Tri Wahyuni | **BY. NY. NY. TRI WAHYUNI / BAYI 1** |

**Contoh dampak.** Perawat ruang bayi mencocokkan gelang dengan label botol ASI atau label tabung darah. Gelang bertuliskan "BY. NY. NY. RINA MARLINA", sedangkan label bertuliskan "BAGAS PRATAMA". Perawat ragu, atau lebih buruk lagi menganggap keduanya bayi berbeda. Ketidakcocokan seperti ini persis yang ingin dicegah oleh Sasaran Keselamatan Pasien 1 (identifikasi pasien dengan benar).

Layar identitas juga menjanjikan "nama ini akan digunakan pada kartu dan gelang pasien", tetapi janji itu tidak dipenuhi oleh gelang.

**Akar masalah.** `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpWristbandRules.cs:123-126` selalu menyusun `"BY. NY. " + nama ibu` bila bayi punya ibu tercatat:

1. Nama ibu di master pasien sudah berawalan "Ny." (contoh "Ny. Rina Marlina"), sedangkan aturan gelang tidak membuang awalan itu. Pendaftaran bayi sebenarnya punya pembersih awalan (`NormalizeMotherName` di `NewbornPatientRegistrationService.cs:407-416`), tetapi aturan gelang tidak memakainya.
2. Nama khusus bayi (`fullName`) tidak pernah dipakai oleh aturan gelang.

**Rekomendasi.** Gelang memakai nama yang sama dengan nama resmi pasien (`MstPatient.FullName`) dalam huruf kapital, ditambah "/ BAYI n". Bila nama resmi masih nama otomatis, hasilnya tetap "BY. NY. RINA MARLINA / BAYI 1". Tambahkan uji unit untuk ibu bernama "Ny. …" dan untuk bayi bernama khusus. Pemilik proses perlu memastikan aturan penamaan gelang yang berlaku di RS (nama bayi atau nama ibu) sebelum perbaikan.

Bukti: `37-j1-temuan-gelang-ny-ganda.png`, `38-j1-label-bayi.png`, `57-j2-label-bayi.png`, `58-j2-temuan-gelang-abaikan-nama-khusus.png`, `79-j3-gelang-bayi.png`.

---

### ISU-02 — Bayi Perinatologi dapat ditempatkan di boks kamar ibu lain atau di bed NICU · P1 · Tinggi

**Apa yang terjadi.** Untuk bayi Ny. Rina Marlina dengan mode Perinatologi (non-intensif), server mengembalikan 10 bed yang semuanya `isBookable=true`:

- 7 bed R. Bayi (BAYI-1-A … BAYI-2-D) — sesuai.
- **NICU-1-B** — bed intensif, padahal bayi tidak membutuhkan perawatan intensif.
- **304-BOKS** — boks rawat gabung di kamar ibunya sendiri, padahal petugas memilih "neonatal terpisah".
- **305-BOKS** — boks di kamar **VIP 305 yang ditempati Ny. Tri Wahyuni (RM 00-00-00-70)**. Pada denah, dialog kamar VIP 305 menampilkan "305-A Terisi · Ny. Tri Wahyuni" dan di bawahnya "305-BOKS Tersedia · **Pilih bed ini**".

**Contoh dampak.** Petugas yang terburu-buru mengklik kamar VIP 305 dan memesan 305-BOKS. Bayi Ny. Rina akhirnya dirawat di samping ibu lain. Pada pagi hari, Ny. Tri menyusui bayi yang bukan bayinya. Skenario kedua: satu-satunya bed NICU kosong terpakai oleh bayi sehat, lalu bayi kritis yang lahir sejam kemudian tidak mendapat inkubator.

Agent tidak memesan bed tersebut. Bukti cukup berupa respons server dan dialog yang dapat diklik.

**Akar masalah.** `InpNewbornAdmissionRules.ValidateBed` (`NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpNewbornAdmissionRules.cs:83-101`) hanya mengatur dua hal: rawat gabung wajib di kamar ibu, dan perawatan intensif wajib bed intensif. Untuk mode "Neonatal" non-intensif tidak ada larangan memakai boks rawat gabung (boks yang berada di kamar dewasa) maupun bed intensif.

**Rekomendasi.**
1. Mode Neonatal tidak boleh memakai bed boks yang berada di kamar perawatan dewasa (kamar ibu mana pun).
2. Bayi non-intensif tidak memakai bed NICU, kecuali ada kebijakan RS yang mengizinkan "limpahan" dengan alasan tercatat. **Butuh keputusan pemilik proses.**
3. Denah sebaiknya langsung membuka lantai ruang bayi; saat ini denah membuka Lantai 3 (kamar dewasa).

Bukti: `45-j2-pilihan-bed-perinatologi.png`, `46-j2-temuan-boks-kamar-ibu-lain-bisa-dipilih.png`.

---

### ISU-03 — Bayi yang lahir pukul 00.00–06.59 WIB ditolak sebagai "masa depan" · P1 · Tinggi (berbukti kode, belum dibuktikan live)

**Apa yang terjadi.** Backend menolak tanggal lahir yang lebih besar dari `DateTime.UtcNow.Date` (`NewbornPatientRegistrationService.cs:93-94`). Tanggal yang dikirim layar adalah tanggal WIB. Selama pukul 00.00–06.59 WIB, tanggal UTC masih tanggal kemarin.

**Contoh.** Bayi lahir 26 Oktober 2026 pukul 01.15 WIB. Layar (yang memakai zona Asia/Jakarta) menerima tanggal 26 Oktober, lalu mengirimnya ke server. Server membandingkan dengan tanggal UTC, yaitu 25 Oktober pukul 18.15 UTC, sehingga 26 > 25 dan pendaftaran ditolak dengan pesan "Tanggal lahir bayi wajib valid dan tidak boleh di masa depan." Petugas jaga malam baru bisa mendaftarkan bayi setelah pukul 07.00.

Uji dilakukan pukul ±11.15 WIB, sehingga kondisi ini tidak dapat dibuktikan langsung. Kesimpulan diambil dari pembacaan kode.

**Rekomendasi.** Bandingkan gabungan tanggal + jam lahir dengan waktu sekarang dalam zona WIB (atau ubah ke UTC dengan benar sebelum dibandingkan). Tambahkan uji unit untuk kelahiran pukul 00.30 WIB.

---

### ISU-04 — Tanggal dan jam lahir yang tidak masuk akal tetap diterima · P2 · Sedang

**Apa yang terjadi.**

| Masukan | Layar | Backend (dari kode) |
|---|---|---|
| Hari ini pukul **23.50** (saat uji baru pukul 11.15) | Diterima, request terkirim | Diterima — hanya tanggal yang diperiksa |
| **01 Januari 1990** | Diterima, request terkirim | Diterima — tidak ada batas bawah |

**Contoh dampak.** Petugas salah memilih tahun di picker. Bayi tercatat lahir tahun 1990, dan umur di gelang, perhitungan dosis obat bayi, serta laporan neonatal menjadi kacau.

**Rekomendasi.** Tolak waktu lahir yang belum terjadi (gabungan tanggal + jam). Batasi tanggal lahir agar tidak lebih awal dari tanggal kasus persalinan (atau maksimal beberapa hari ke belakang). Nonaktifkan tanggal masa depan di picker.

Bukti: `18-neg-jam-lahir-belum-terjadi.png`, `20-neg-tanggal-lahir-1990.png`.

---

### ISU-05 — NIK penanda tangan persetujuan tanpa validasi · P3 · Sedang

Field "Nomor Identitas (NIK / Paspor)" menerima huruf, simbol, dan teks seperti `ABCDE<script>99999999999999` (27 karakter). Layar mengirimnya apa adanya, dan backend hanya membatasi panjang 50 karakter (`InpatientAdmissionConsentDtos.cs:35-36`). Akibatnya dokumen persetujuan bertanda tangan dapat memuat nomor identitas yang jelas tidak sah.

**Rekomendasi.** Validasi sesuai jenis identitas: NIK 16 digit angka, paspor alfanumerik 6–9 karakter, tanpa simbol. Bukti: `33-neg-nik-penanda-tangan-sampah.png` (request dicegat agent, tidak tersimpan).

---

### ISU-06 — DPJP bayi tidak dibatasi dokter anak · P1 · Perlu keputusan

Judul halaman menyebut "tentukan DPJP Anak", tetapi daftar DPJP memuat seluruh 11 dokter, termasuk Gigi, Ortopedi, dan Jantung, tanpa peringatan apa pun. Contoh: dr. Maya Permata Sari (Gigi) dapat dipilih sebagai DPJP bayi NICU.

**Rekomendasi.** Pemilik proses menetapkan aturannya: apakah DPJP bayi wajib Sp.A, atau boleh dokter umum/jaga dengan peringatan. Setelah itu, terapkan filter atau peringatan di layar dan validasi di server. Bukti: `23-j1-langkah-perawatan-dpjp.png`.

---

### ISU-07 — Dua belas klausul persetujuan sudah tercentang sejak awal · P1 · Perlu keputusan

Saat langkah Persetujuan Wali dibuka, penghitung langsung menunjukkan "12 Dari 12 Disetujui". Wali bisa menandatangani tanpa pernah membaca atau mencentang satu klausul pun. Praktik persetujuan yang baik umumnya meminta tindakan aktif.

**Rekomendasi.** Pemilik proses dan Komite Etik/Hukum memutuskan apakah klausul harus dicentang satu per satu (atau melalui satu centang "Saya telah membaca seluruh klausul"). Bukti: `29-j1-persetujuan-wali-awal.png`.

---

### ISU-08 — Picker tanggal lahir terpotong di bawah layar · P4 · Sedang

Pada layar 1920×945, popover tanggal terbuka ke bawah dan melewati batas layar. Baris tanggal 25–31 serta tombol "Hari ini" dan "Hapus" tidak bisa diklik sebelum halaman digulir. Padahal tanggal hari ini justru yang paling sering dipilih untuk bayi baru lahir. Picker jam di form yang sama sudah bisa membalik ke atas, sedangkan picker tanggal tidak.

**Rekomendasi.** Samakan logika penempatan popover tanggal dengan popover jam. Bukti: `19-p4-picker-tanggal-terpotong.png`.

---

### ISU-09 — "Tempat Tidur Pasien Ibu: Belum terbaca" pada konfirmasi rawat gabung · P4 · Rendah

Kartu konfirmasi boks menampilkan "Belum terbaca" untuk tempat tidur ibu, padahal Ny. Anggraini menempati 303-A. API kandidat ibu tidak mengirim `currentBedCode`, `currentBedName`, maupun `currentPatientClassName`, sementara layar membaca ketiga field itu.

**Rekomendasi.** Tambahkan field tersebut pada respons `newborn-mother-candidates`. Bukti: `26-j1-langkah-penempatan-rawat-gabung.png`.

---

### ISU-10 — Kumpulan catatan kejelasan dan kebersihan teknis · P5 · Rendah

1. Status kasus tampil dalam bahasa Inggris mentah: "InProgress", "Completed" (`10-j1-ibu-terpilih.png`).
2. Kartu Pasien menulis "menunggu konfirmasi kedatangan di Papan Tempat Tidur", padahal episode sudah **Admitted** dan bed sudah **Terisi** (`39-j1-kartu-pasien.png`).
3. Ringkasan "Perawatan" hanya menulis "Neonatal terpisah", baik untuk Perinatologi maupun NICU (`55`, `77`).
4. Dialog Ruang NICU menampilkan dua alasan untuk NICU-1-A sekaligus: "Bed belum siap atau masih dipegang" dan "sedang ditempati pasien lain". Alasan pertama menyesatkan (`74-j3-ruang-nicu-dialog.png`).
5. Tombol "Pesan Tempat Tidur" nonaktif ±6 detik tanpa keterangan, selama sistem menyiapkan encounter dan episode (`49-j2-langkah-pemesanan.png`).
6. Penamaan bed berbeda antarlangkah: "303-BOKS" di konfirmasi, tetapi "BD-RSMMC-BOKS-303" di ringkasan. Bed BAYI-1-A memakai kode umum "BD-RSMMC-00001".
7. Panggilan yang tidak relevan: `GET admissions/surgical-booking-lookup` untuk pasien bayi (404 di setiap jalur), dan `GET …/general-consent/print-data` sebelum admisi dikonfirmasi (409 "Admisi belum dikonfirmasi" di setiap jalur). Keduanya tidak mengganggu alur, tetapi muncul sebagai galat di konsol.
8. Nama bayi menerima tag HTML. Skrip tidak dieksekusi di layar (aman), tetapi nama seperti itu akan tercetak di kartu, label, dan dokumen lain. Pertimbangkan daftar karakter yang diizinkan.
9. Kolom NIK bayi masih menerima huruf saat diketik, walau ditolak saat disimpan.
10. Foto profil pengguna diblokir *Content Security Policy* (`img-src` tidak memuat `api-dev.quilvian-mmchospital.com`), sehingga konsol penuh galat berulang.

---

## 7. Data Uji yang Terbentuk di Database `QuilvianNewDevHamzah`

Semua data berikut dibuat lewat layar aplikasi, sesuai permintaan pengujian, dan tidak dihapus oleh agent:

| Jalur | Pasien bayi | Encounter | Episode | Bed (Terisi) |
|---|---|---|---|---|
| 1 | PAT-RSMMC-00071 · RM 00-00-00-71 · By. Ny. Anggraini Puspita | ENC-RSMMC-00216 | RI-261025041519-F8366C (Admitted) | 303-BOKS / BD-RSMMC-BOKS-303 |
| 2 | PAT-RSMMC-00072 · RM 00-00-00-72 · Bagas Pratama | ENC-RSMMC-00217 | RI-261025042436-D614FC (Admitted) | BAYI-1-A / BD-RSMMC-00001 |
| 3 | PAT-RSMMC-00073 · RM 00-00-00-73 · By. Ny. Tri Wahyuni | ENC-RSMMC-00218 | RI-261025042902-1FF0D0 (Admitted) | NICU-1-B / BD-RSMMC-NICU-02 |

Masing-masing episode juga memiliki satu formulir persetujuan wali bertanda tangan. Ibu cadangan **tidak berubah**: Ny. Nurul Aini dan Ny. Fitri Handayani hanya dipakai untuk uji Batal (0 data tersimpan), sedangkan Ny. Maria Veronica tidak disentuh. Ketiganya masih siap dipakai untuk uji ulang setelah perbaikan.

---

## 8. Rekomendasi Urutan Perbaikan

1. **ISU-01** gelang identitas — memengaruhi setiap bayi yang didaftarkan.
2. **ISU-02** kelayakan bed Perinatologi — cegah boks kamar ibu lain; putuskan kebijakan bed NICU.
3. **ISU-03** zona waktu tanggal lahir — memblokir kelahiran dini hari.
4. **ISU-04** dan **ISU-05** validasi data lahir dan identitas penanda tangan.
5. Ambil keputusan untuk **ISU-06** (DPJP) dan **ISU-07** (klausul), lalu terapkan.
6. **ISU-08**, **ISU-09**, dan **ISU-10** pada rilis berikutnya.

Setelah perbaikan, jalankan ulang ketiga jalur memakai ibu cadangan (Ny. Fitri Handayani, Ny. Maria Veronica, Ny. Nurul Aini).

---

## 9. Lampiran Artefak

| Jenis | Lokasi |
|---|---|
| Screenshot (70 berkas) | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-bayi-baru-lahir/` (ditambah 3 berkas dry-run di `admisi-bayi-replay-OPR-OBS-261023-065/`) |
| Log jaringan (204 panggilan API) | `QuilvianSystemFrontendDev/test-with-agy/logs/admisi-bayi-baru-lahir-network.jsonl` |
| Driver Chromium headed yang dikendalikan agent | `QuilvianSystemFrontendDev/test-with-agy/scripts/agy-driver.mjs` |
| Skrip skenario yang dapat dijalankan ulang | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-bayi-baru-lahir.e2e.mjs` — sudah diverifikasi dalam mode `--dry-run` (Ny. Fitri Handayani, hanya request login, tanpa data tersimpan) |

Penamaan screenshot: `0x` persiapan, `1x–3x` Jalur 1 dan uji negatif identitas/persetujuan, `4x–5x` Jalur 2, `6x` uji Batal, `7x–8x` Jalur 3, `90` uji bayi kembar. Kredensial dibaca dari variabel lingkungan `AGY_EMAIL` / `AGY_PASSWORD` dan tidak tersimpan di skrip.
