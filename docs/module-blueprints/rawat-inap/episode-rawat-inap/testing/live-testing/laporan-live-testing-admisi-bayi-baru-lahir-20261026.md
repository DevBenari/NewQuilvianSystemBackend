# Laporan Live Browser Testing — Admisi Rawat Inap: Admisi Bayi Baru Lahir (Uji Ulang 26 Oktober 2026)

| Hal | Isi |
|---|---|
| Menu yang diuji | Rawat Inap → Admisi Rawat Inap → **Admisi Bayi Baru Lahir** |
| Alamat layar | `http://localhost:3000/health-services/inpatient-management/admissions/newborn` |
| Sub-modul blueprint | `rawat-inap/episode-rawat-inap` |
| Tanggal uji | Senin, 26 Oktober 2026, pukul ±10.53–11.12 WIB, ditambah uji ulang tanggal lahir 1990 pukul ±11.45 WIB |
| Jenis uji | Uji ulang setelah perbaikan, dibandingkan dengan `laporan-live-testing-admisi-bayi-baru-lahir.md` (uji 25 Oktober 2026) |
| Penguji | AI Agent (Claude Code). Seluruh klik, isian, dan pemeriksaan dilakukan agent di browser. |
| Browser | Google Chrome 155 (engine Chromium) tampil di layar (*headed*), dikendalikan `playwright` lewat CDP, `slowMo` 300–450 ms, layar 1920×945 |
| Frontend | `http://localhost:3000` — cabang `HamzahV2`, commit `2cd1c0610` (memuat `04836b070` "enhance newborn admission process and validation") |
| Backend | `https://localhost:7184/api` — cabang `MHamzah`, commit `e095f656` |
| Basis data | `QuilvianNewDevHamzah`. Untuk menu ini agent **tidak** menulis data langsung ke DB; semua data terbentuk lewat layar. |
| Akun | `superadmin@admin.com` (kata sandi tidak dicatat di dokumen ini) |
| Laporan menu lain pada sesi yang sama | `laporan-live-testing-admisi-pasien-lama.md`, `laporan-live-testing-admisi-transfer-igd-poliklinik.md` |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: alur Admisi Bayi Baru Lahir sudah jauh lebih matang daripada uji 25 Oktober. Sembilan dari sepuluh temuan kemarin sudah diperbaiki, dan dua jalur perawatan (Perinatologi dan NICU) berjalan tuntas sampai kartu pasien. Menu ini hampir siap; satu masalah integritas data RM bayi (BY-01) perlu diperbaiki sebelum dipakai di lapangan.**

| Prioritas | Test case | Lulus | Gagal | Lulus dengan catatan |
|---|---|---|---|---|
| P0 Fungsi Kritis | 4 | 4 | 0 | 0 |
| P1 Kepatuhan Bisnis dan Keselamatan | 9 | 8 | 1 | 0 |
| P2 & P3 Validasi dan Data Ngasal | 6 | 6 | 0 | 0 |
| P4 Tampilan | 2 | 1 | 0 | 1 |
| P5 Kenyamanan | 1 | 1 | 0 | 0 |
| **Total** | **22** | **20** | **1** | **1** |

Yang sudah berjalan baik:

- **Gelang identitas sudah benar**: "BY. NY. FITRI HANDAYANI / BAYI 1" dan, untuk nama khusus, "ARYA PRATAMA / BAYI 1", keduanya dengan baris "IBU: …". Gelang dan label kini memuat nama yang sama.
- **Penempatan bayi aman**: bayi Perinatologi hanya bisa ke ruang bayi (boks di kamar ibu lain dan bed NICU nonaktif); bayi NICU hanya ke NICU; Rawat Gabung ditolak dengan arahan jelas bila kamar ibu tidak punya boks.
- **DPJP bayi**: dokter anak diurutkan teratas dengan lencana "Disarankan"; memilih dokter non-anak mewajibkan alasan, dan alasan itu tercatat di catatan admisi.
- **Data lahir**: jam lahir yang belum terjadi, tanggal sebelum kasus persalinan, dan tanggal masa depan ditolak dengan pesan yang jelas. Nama bertag HTML ditolak; NIK berhuruf disaring.
- **Persetujuan wali**: hanya orang tua/wali, identitas ibu terisi otomatis, klausul wajib dicentang aktif.
- 93 panggilan API terekam: 0 galat 4xx/5xx, 0 error JavaScript. Panggilan yang kemarin selalu gagal (`surgical-booking-lookup` 404 dan `general-consent/print-data` 409) tidak muncul lagi.

Yang perlu ditindaklanjuti:

| No | Temuan | Tingkat |
|---|---|---|
| BY-01 | Pendaftaran bayi yang ditinggal setelah langkah Identitas meninggalkan **RM bayi tanpa episode**. Tidak ada jalur melanjutkannya; percobaan berikutnya membuat RM baru dan urutan "Bayi ke-n" salah. Saat ini ada 11 RM bayi seperti itu di DB. | P1 · Tinggi |
| BY-02 | Kumpulan catatan kecil tampilan dan data master | P4/P5 · Rendah |
| BY-03 | Bilah footer melayang menutupi isian form (juga di menu lain) | P4 · Sedang |

---

## 2. Status Temuan Uji 25 Oktober 2026

| Temuan kemarin | Status hari ini | Bukti |
|---|---|---|
| ISU-01 Gelang "BY. NY. NY." dan nama khusus diabaikan | **Sudah diperbaiki** | BY-TC-09 |
| ISU-02 Bayi Perinatologi bisa ditempatkan di boks kamar ibu lain atau bed NICU | **Sudah diperbaiki** (aturan backend `NEWBORN_NEONATAL_ROOM_REQUIRED` dan `NEWBORN_INTENSIVE_BED_RESERVED`). Saran agar denah langsung membuka lantai ruang bayi belum diterapkan (BY-02 butir 1) | BY-TC-06 |
| ISU-03 Tanggal lahir dibandingkan dengan tanggal UTC | **Sudah diperbaiki di kode**: `NewbornPatientRegistrationService` kini membandingkan tanggal + jam lahir dengan waktu RS (`hospitalNow`). Tidak dapat dibuktikan langsung karena uji berlangsung siang hari | pembacaan kode |
| ISU-04 Jam lahir yang belum terjadi dan tanggal 1990 diterima | **Sudah diperbaiki** | BY-TC-15, BY-TC-16 |
| ISU-05 NIK penanda tangan persetujuan tanpa validasi | **Sudah diperbaiki** (form persetujuan yang sama diuji di menu Pasien Lama, PL-TC-24) | laporan Pasien Lama |
| ISU-06 DPJP bayi tidak dibatasi dokter anak | **Sudah diperbaiki** | BY-TC-08 |
| ISU-07 Dua belas klausul tercentang sejak awal | **Sudah diperbaiki**: mulai "0 Dari 12 Disetujui" | BY-TC-11 |
| ISU-08 Picker tanggal lahir terpotong | **Sudah diperbaiki** | BY-TC-20 |
| ISU-09 "Tempat Tidur Pasien Ibu: Belum terbaca" | **Sudah diperbaiki**: "330-A (Sedang Ditempati Ibu)" | BY-TC-20 |
| ISU-10 Catatan kecil | Butir 1 (status kasus berbahasa Inggris), 2 (teks kartu pasien), 3 (ringkasan "Neonatal terpisah"), 4 (alasan ganda NICU), 7 (panggilan 404/409), 8 (tag HTML pada nama), dan 9 (huruf pada NIK) sudah diperbaiki. Butir 6 (kode bed umum seperti `BD-RSMMC-00001`) masih ada. Butir 5 dan 10 tidak diuji ulang | BY-TC-17, BY-TC-22 |

---

## 3. Alur Proses Bisnis Admisi Bayi

**Pemicu:** seorang ibu menjalani persalinan melalui kasus operasi obstetri (*Sedang berlangsung* atau *Selesai*), lalu petugas mendaftarkan bayinya ke rawat inap.

**Contoh nyata dari uji ini:**

- **Jalur 1 — Perinatologi.** Bayi Ny. Fitri Handayani lahir pukul 09.30. Ibu dirawat di Kelas I 330, yang tidak punya boks bayi. Petugas mencoba Rawat Gabung, sistem menolak dan menawarkan Perinatologi. Bayi ditempatkan di BAYI-1-B, penjamin BPJS mengikuti ibu.
- **Jalur 2 — NICU.** Bayi Ny. Nurul Aini, diberi nama "Arya Pratama", lahir pukul 10.20 dengan APGAR 4/6 dan butuh CPAP. Dokter anak sedang mengoperasi SC lain, sehingga dokter umum jaga menjadi DPJP sementara dengan alasan tercatat. Bayi ditempatkan di NICU-1-C, penjamin Tunai mengikuti ibu.

| Langkah | Nama di layar | Yang dilakukan petugas | Yang dilakukan sistem |
|---|---|---|---|
| 1 | Pilih Ibu Persalinan | Mencari ibu memakai nama, No. RM, atau nomor kasus | Menampilkan kasus, lokasi ibu, penjamin, bayi yang sudah terdaftar, dan pratinjau "Bayi ke-n"; meminta konfirmasi bila kembar |
| 2 | Identitas Bayi | Mengisi tanggal dan jam lahir, jenis kelamin, tempat lahir; nama dan NIK opsional | **Membuat pasien bayi dan No. RM**, nama otomatis "By. Ny. [Ibu]", urutan kelahiran |
| 3 | Perawatan dan DPJP Bayi | Memilih Rawat Gabung / Perinatologi / NICU, sumber keputusan, DPJP (alasan bila bukan dokter anak) | Memvalidasi pilihan |
| 4 | Penempatan dan Pemesanan | Rawat Gabung: konfirmasi boks di kamar ibu. Perinatologi/NICU: memilih bed di denah | Membuat kunjungan dan episode *Draft*, lalu menahan bed ±2 jam |
| 5 | Persetujuan Wali | Memeriksa identitas wali (terisi dari ibu), menyetujui 12 klausul, tanda tangan | Menyimpan persetujuan (wajib orang tua/wali) |
| 6 | Tinjau dan Aktifkan | Memeriksa ringkasan, menekan "Aktifkan Admisi Bayi" | Episode menjadi *Admitted*, bed *Terisi* |
| 7 | Dokumen Bayi | Mencetak gelang dan label | Gelang dan label dari data server |
| 8 | Kartu Pasien | Mencetak kartu | Kartu pasien bayi |

**Hasil akhir yang diharapkan:** bayi tercatat sebagai pasien rawat inap aktif yang tertaut ke ibu dan kasus persalinannya, menempati bed sesuai kebutuhan klinis, dengan penjamin mengikuti ibu dan gelang identitas yang benar.

---

## 4. Data Uji

| Ibu | RM | Kasus | Lokasi ibu | Penjamin | Bayi sebelum uji | Dipakai untuk |
|---|---|---|---|---|---|---|
| Ny. Fitri Handayani | 00-00-00-65 | OPR-OBS-261023-065 | Kelas I 330 (tanpa boks) | BPJS Kesehatan | 0 | Jalur 1 dan uji negatif identitas |
| Ny. Nurul Aini | 00-00-00-67 | OPR-OBS-261023-067 | Kelas I 429 | Tunai / Pribadi | 0 | Jalur 2 |
| Ny. Maria Veronica | 00-00-00-66 | OPR-OBS-261023-066 | Kelas I 428 | Prudential Indonesia | 1 ("Ferran", RM 74, tanpa episode) | Uji kembar dan BY-01 (tidak disimpan) |

---

## 5. Rincian Test Case

Keterangan: ✅ Lulus · ❌ Gagal · ⚠️ Lulus dengan catatan. Kolom "Bukti" menyebut nama atau awalan berkas di `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/nwb-bayi/` (tanpa `.png`).

Uji data ngasal pada identitas bayi dijalankan dengan **pengaman pencegat**: bila layar meloloskan isian, request ke `patients/newborns` dihentikan di browser sebelum sampai ke server. Pada seluruh uji ini layar sudah menolak lebih dulu, sehingga tidak ada request yang perlu dicegat.

### P0 — Fungsi Kritis

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| BY-TC-01 | Membuka layar; lanjut tanpa memilih ibu | "Pilih ibu dari kasus operasi persalinan sebelum melanjutkan." | ✅ | `nwb-01-pilih-ibu`, `nwb-02-lanjut-tanpa-ibu` |
| BY-TC-02 | Jalur Perinatologi (Ny. Fitri, BPJS) sampai kartu pasien | RM **00-00-00-75** "By. Ny. Fitri Handayani", Bayi ke-1, episode **RI-261026110238-1B66C1** *Admitted*, BAYI-1-B *Terisi* | ✅ | `nwb-12` s.d. `nwb-31` |
| BY-TC-03 | Jalur NICU (Ny. Nurul, Tunai), nama khusus "Arya Pratama" | RM **00-00-00-76**, episode **RI-261026110742-22F418** *Admitted*, `RequiresNeonatalIntensiveCare=true`, NICU-1-C *Terisi* | ✅ | `nwb-43` s.d. `nwb-56` |
| BY-TC-04 | Tidak ada galat server atau *crash* | 93 panggilan terekam: 0 respons 4xx/5xx, 0 error JavaScript | ✅ | log jaringan |

### P1 — Kepatuhan Bisnis dan Keselamatan

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| BY-TC-05 | Rawat Gabung, padahal kamar ibu (Kelas I 330) tidak punya boks | "Kamar ibu (Kelas KELAS I · KELAS I 330) tidak memiliki boks bayi. Rawat gabung hanya dapat dilakukan di kamar yang memiliki boks. Pindahkan ibu ke kamar berboks atau pilih Ruang Perinatologi."; tombol pesan nonaktif; ada pintasan "Ubah ke Perawatan Perinatologi" | ✅ | `nwb-15-rawat-gabung-tanpa-boksb-setelah-lanjut` |
| BY-TC-06 | Bed yang ditawarkan untuk Perinatologi | Lantai 3: tidak satu pun kamar dewasa/boks VIP aktif. Lantai 2: hanya PERINATOLOGI BAYI-1 dan BAYI-2 aktif; NICU dan ICU nonaktif "tidak ada bed yang layak untuk pasien ini" | ✅ | `nwb-16-ubah-perinatologi`, `nwb-19-dialog-bayi1`; daftar status kamar per lantai dari keluaran skrip `admisi-1026-bed-kamar.mjs` (atribut `aria-label`/`aria-disabled` denah) |
| BY-TC-07 | Bed yang ditawarkan untuk NICU | Hanya kamar NICU aktif; Perinatologi dan ICU nonaktif | ✅ | `nwb-45-nicu-penempatan`, `nwb-47-dialog-nicu`; daftar status kamar dari keluaran skrip |
| BY-TC-08 | DPJP bukan dokter anak | Dokter anak teratas "Dokter Anak · Disarankan". Dokter Gigi tanpa alasan → "Alasan memilih dokter non-Anak sebagai DPJP bayi wajib diisi."; alasan spasi saja ditolak; alasan valid tercatat di catatan admisi sebagai "Alasan DPJP non-Anak: …" | ✅ | `nwb-13`, `nwb-14-dpjp-gigi-tanpa-alasan`, `nwb-44-alasan-spasi`, verifikasi DB |
| BY-TC-09 | Gelang dan label identitas | Jalur 1: "BY. NY. FITRI HANDAYANI / BAYI 1 — IBU: FITRI HANDAYANI". Jalur 2: "ARYA PRATAMA / BAYI 1 — IBU: NURUL AINI". Label memakai nama yang sama | ✅ | `nwb-29-gelang-bayi`, `nwb-30-label-bayi`, `nwb-55-gelang-nama-khusus`, `nwb-56-label-nama-khusus` |
| BY-TC-10 | Penjamin bayi mengikuti ibu | Ringkasan "Penjamin mengikuti ibu: BPJS Kesehatan" / "Tunai / Pribadi"; DB `PaymentType` 2 dan 1 | ✅ | `nwb-26-tinjau`, verifikasi DB |
| BY-TC-11 | Persetujuan oleh orang tua/wali | Pilihan "Pasien Sendiri" tidak ditawarkan, dengan penjelasan hukum; nama, NIK, hubungan "Ibu Kandung", HP, dan alamat ibu terisi otomatis; klausul mulai 0 dari 12 | ✅ | `nwb-24-persetujuan-wali` |
| BY-TC-12 | Bayi kembar wajib dikonfirmasi | Tanpa centang: "Bayi dari kasus persalinan ini sudah terdaftar di rawat inap. Beri tanda centang konfirmasi pendaftaran bayi kembar (Bayi ke-2) untuk melanjutkan." | ✅ | `nwb-60b-ibu-dipilih`, `nwb-61-kembar-tanpa-konfirmasi` |
| BY-TC-13 | Bayi yang sudah terdaftar tetapi belum diadmisi ("Ferran", RM 74) | Ditampilkan "Terdaftar" dan disebut "sudah terdaftar di rawat inap"; tidak ada jalur melanjutkan admisinya | ❌ BY-01 | `nwb-60b-ibu-dipilih` |

### P2 & P3 — Validasi dan Data Ngasal

| ID | Skenario | Masukan | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|---|
| BY-TC-14 | Tanggal lahir besok | 27 Okt 2026 | "Tanggal lahir tidak boleh di masa depan." | ✅ | `nwb-05`, `nwb-07-tanggal-besok-identitas` |
| BY-TC-15 | Jam lahir yang belum terjadi | Hari ini pukul 23.50 (saat uji ±10.57) | "Waktu lahir belum terjadi. Sekarang 26 Okt 2026 pukul 10.57 WIB." | ✅ | `nwb-08-jam-belum-terjadi-identitas` |
| BY-TC-16 | Tanggal lahir sebelum kasus persalinan | 21 Okt 2026 (kasus Ny. Fitri diminta 23 Okt); 1 Jan 1990 (kasus Ny. Maria diminta 23 Okt, diuji ±11.45 WIB lewat jalur kembar lalu dibatalkan) | Keduanya: "Tanggal lahir lebih awal dari kasus persalinan (diminta 23 Okt 2026)."; 0 request ke server | ✅ | `nwb-10-tanggal-21okt-identitas`, `nwb-09-tanggal-1990-identitas` |
| BY-TC-17 | Nama bertag HTML; NIK bayi berhuruf | `<img src=x onerror=alert(1)>`; `32ab-cd71!05xyz2604999999` | "Nama hanya boleh berisi huruf, spasi, titik, koma, apostrof, dan tanda hubung."; skrip tidak berjalan; NIK disaring menjadi `3271052604999999` | ✅ | `nwb-11-nama-html-nik-huruf-identitas` |
| BY-TC-18 | "Admisi Baru" setelah satu admisi selesai | — | Form kembali ke langkah 1 tanpa ibu atau identitas sebelumnya | ✅ | `nwb-40-admisi-baru`, `nwb-41-form-bersih` |
| BY-TC-19 | Batal Pendaftaran di tengah alur | Ny. Maria Veronica | Kembali ke menu, 0 data tersimpan | ✅ | `nwb-61-kembar-tanpa-konfirmasi` |

### P4 — Tampilan

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| BY-TC-20 | Picker tanggal lahir; tempat tidur ibu di konfirmasi Rawat Gabung | Picker tidak terpotong (atas 202 px, bawah 554 px dari tinggi layar 945 px); "Tempat Tidur Pasien Ibu: 330-A (Sedang Ditempati Ibu)" | ✅ | `nwb-04-picker-tanggal`, `nwb-15-…` |
| BY-TC-21 | Rincian teks dan data master | Lihat BY-02 dan BY-03 | ⚠️ | beberapa |

### P5 — Kenyamanan Pemakaian

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| BY-TC-22 | Kejelasan teks | Status kasus "Sedang berlangsung"/"Selesai"; kartu "Admisi aktif — Episode … aktif. Pasien sudah menempati R. Bayi · BAYI-1-B."; ringkasan "Ruang Perinatologi"/"Ruang NICU"; bed terisi hanya satu alasan "sedang ditempati pasien lain" | ✅ | `nwb-03b-ibu-dipilih`, `nwb-31-kartu-pasien`, `nwb-26-tinjau`, `nwb-47-dialog-nicu` |

**Rekap:** 22 test case — 20 lulus, 1 gagal, 1 lulus dengan catatan.

---

## 6. Temuan Isu

### BY-01 — RM bayi tanpa episode dan urutan kelahiran yang menggelembung · P1 · Tinggi

**Apa yang terjadi.** Langkah 2 (Identitas Bayi) langsung membuat pasien bayi beserta No. RM. Bila petugas meninggalkan admisi sebelum langkah 4 (Penempatan), pasien itu tertinggal tanpa kunjungan dan episode. Saat ibu yang sama dipilih lagi:

- bayi lama ditampilkan "Terdaftar", dan layar menyebutnya "sudah terdaftar di rawat inap" serta "telah selesai diadmisi";
- tidak ada tombol untuk melanjutkan admisi bayi itu;
- satu-satunya jalan adalah mencentang "bayi kembar (Bayi ke-2)", yang membuat RM baru.

API `newborn-mother-candidates` sebenarnya sudah mengirim `episodeId: null` untuk bayi seperti itu, tetapi layar tidak membedakannya dari bayi yang sudah dirawat.

Di DB saat ini ada **11 pasien bayi tanpa kunjungan maupun episode**:

| RM | Nama | Dibuat |
|---|---|---|
| 00-00-00-35 | Bayi SITI NURHALIZA | 15 Okt |
| 00-00-00-42, -43, -44 | By. Ny. DEWI ANGGRAINI | 21–22 Okt |
| 00-00-00-48, -55 | By. Ny. AGNES YULIANI RAJA GUK GUK | 22 Okt |
| 00-00-00-50, -51, -54 | By. Ny. RATNA SARI DEWI | 22 Okt |
| 00-00-00-56 | By. Ny. ANISA LESTARI | 22 Okt |
| 00-00-00-74 | Ferran (ibu Ny. Maria Veronica) | 25 Okt |

Akibatnya, Ny. DEWI ANGGRAINI tercatat memiliki 6 "bayi" (RM 42–47) dan Ny. RATNA SARI DEWI 5 "bayi".

**Contoh dampak.** Bidan menyimpan identitas bayi, lalu dipanggil ke Kamar Bersalin sebelum memesan boks. Petugas shift berikutnya mengulang dari awal dan mendaftarkan bayi yang sama sebagai "Bayi ke-2" dengan RM baru. Hasil laboratorium dan resep bayi itu bisa terpecah di dua RM, dan gelang mencetak "BAYI 2" untuk anak tunggal.

**Rekomendasi.**
1. Tampilkan bayi terdaftar tanpa episode sebagai "Admisi belum selesai" dengan tombol "Lanjutkan admisi bayi ini".
2. Ubah teks konfirmasi kembar agar tidak menyebut "sudah diadmisi" bila bayi sebelumnya belum punya episode.
3. Hitung urutan kelahiran hanya dari bayi yang benar-benar berbeda (atau minta konfirmasi "ini bayi yang berbeda").
4. Sediakan prosedur penonaktifan atau penggabungan untuk 11 RM bayi yatim yang sudah ada, dengan persetujuan Rekam Medis.

### BY-02 — Catatan kecil tampilan dan data master · P4/P5 · Rendah

1. Denah untuk bayi Perinatologi/NICU masih terbuka di Lantai 3 (kamar dewasa); petugas harus berpindah ke Lantai 2 sendiri.
2. Tanggal masa depan masih bisa diklik di picker tanggal lahir (ditolak saat simpan), dan daftar tahun picker menawarkan 2027–2031.
3. Teks "Kelas KELAS I · KELAS I 330" (kata kelas berulang) di konfirmasi Rawat Gabung.
4. Waktu lahir tampil dengan format mesin "2026-10-26 · 09:30:00" di Tinjau dan Dokumen Bayi.
5. Label pasien memuat kode "SITE-MMC-001" di baris nama ("BY. NY. FITRI HANDAYANI / BAYI 1 / SITE-MMC-001").
6. Bed NICU tercatat pada unit layanan "Ruang Bayi", padahal master memiliki unit "NICU".
7. Kode bed umum masih dipakai di ringkasan (ISU-10 butir 6 kemarin).

### BY-03 — Bilah footer melayang menutupi isian form · P4 · Sedang

Bilah "Privacy Policy · Terms of Use · Quilvian System" melayang di bawah layar dan menutupi bagian form saat digulir, termasuk kanvas tanda tangan Persetujuan Wali. Goresan tanda tangan yang jatuh di area itu tidak terbaca. Temuan yang sama tercatat di menu Pasien Lama (PL-04) dan Transfer.

---

## 7. Endpoint yang Terlibat (Gaya Swagger)

Semua endpoint memakai `[Authorize]` (sesi login cookie).

### `[Tags("Health Services / Inpatient Management / Inpatient Episode")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/episodes/newborn-mother-candidates` | Daftar ibu dari kasus persalinan | `search`, `pageNumber`, `pageSize` | 200 — `deliveryCaseNumber`, `currentRoomName`, `paymentSourceName`, `registeredBabyCount`, `nextBirthOrderPreview`, `registeredBabies[]` (`episodeId`, `episodeStatus`) |
| GET | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility` | Kelayakan admisi bayi | `patientId` | 200 |
| POST | `/api/v1/health-services/inpatient-management/episodes` | Episode *Draft* bayi | `newbornAdmission` (`careMode`, `careSource`, `requiresIntensiveCare`, `nonPediatricDoctorReason`), `motherEpisodeId`, `doctorId`, `notes` | 200 |
| GET | `/api/v1/health-services/inpatient-management/episodes/{id}` | Detail episode | — | 200 |

### `[Tags("Health Services / Patient Management / Master Data / Patient")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| POST | `/api/v1/health-services/patient-management/master-data/patients/newborns` | Mendaftarkan pasien bayi (header `Idempotency-Key`) | `motherPatientId`, `deliveryCaseId`, `fullName?`, `birthDate`, `birthTime`, `gender`, `birthPlace?`, `identityNumber?` | 200 "Pasien bayi berhasil didaftarkan." — `medicalRecordNumber`, `birthOrder` |

### `[Tags("Health Services / Registration Management / Patient Encounter")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| POST | `/api/v1/health-services/registration-management/patient-encounters/admin` | Kunjungan rawat inap bayi dengan penjamin warisan ibu | 200 |

### `[Tags("Health Services / Inpatient Management / Bed Occupancy")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/bed-occupancies/floor-plan` | Denah dengan kelayakan pasien (`floorCode`, `purpose=Admission`, `episodeId`) | 200 |
| POST | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations` | Menahan bed bayi | 200 "Pemesanan tersimpan." |
| POST | `/api/v1/health-services/inpatient-management/bed-occupancies/placements` | Aktivasi penempatan | 200 "Penempatan tersimpan." |

### `[Tags("Health Services / Inpatient Management / Inpatient Admission Consent")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| POST | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` | Persetujuan wali dan tanda tangan digital | `signerType` (Parent/Guardian), `signerName`, `signerIdentityNumber`, `agreedClauses[]`, `signatureImageBase64` | 201 "Formulir persetujuan rawat inap dan tanda tangan digital berhasil disimpan." |
| GET | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` | Membaca persetujuan tersimpan | — | 200 |

Catatan: tag consent kini sudah mengikuti pola modul (laporan 25 Okt mencatat `InpatientAdmissionConsent`).

Endpoint pendukung (semua 200): `episodes/{id}/admission-workspace/identity-labels`, `episodes/{id}/admission-workspace/print-logs`, `bed-management/floor-plan`, `patients/admin/{id}`.

**Ringkasan status jaringan yang terekam untuk menu ini:** 93 panggilan — GET 2xx ×81, POST 2xx ×12, 4xx ×0, 5xx ×0, error JavaScript ×0.

---

## 8. Data Uji yang Terbentuk di `QuilvianNewDevHamzah`

| Jalur | Pasien bayi | Episode | Status | Bed | Catatan |
|---|---|---|---|---|---|
| 1 — Perinatologi | By. Ny. Fitri Handayani · **RM 00-00-00-75** · Perempuan · Bayi ke-1 · lahir 26 Okt 09.30 di "RS MMC - Kamar Operasi" | RI-261026110238-1B66C1 | *Admitted* | BAYI-1-B (*Terisi*) | DPJP dr. Dewi Lestari (Anak), penjamin BPJS, persetujuan wali |
| 2 — NICU | Arya Pratama · **RM 00-00-00-76** · Laki-laki · Bayi ke-1 · lahir 26 Okt 10.20 | RI-261026110742-22F418 | *Admitted* | NICU-1-C (*Terisi*) | DPJP dr. Ranger Biru (Umum) dengan alasan tercatat, penjamin Tunai, persetujuan wali |

Ny. Maria Veronica tidak berubah (uji kembar dibatalkan sebelum menyimpan). Setelah uji ini, bed ruang bayi yang masih kosong: BAYI-1-C, BAYI-1-D, BAYI-2-B, BAYI-2-C, BAYI-2-D. NICU penuh (3 dari 3). Agent tidak menghapus data apa pun.

---

## 9. Rekomendasi Urutan Perbaikan

1. **BY-01** — jalur melanjutkan admisi bayi yang tertinggal dan pembersihan 11 RM bayi yatim.
2. **BY-03** — footer yang menutupi kanvas tanda tangan.
3. **BY-02** pada rilis berikutnya, dimulai dari butir 1 (denah langsung membuka lantai ruang bayi) dan butir 5 (kode situs di label).

Setelah perbaikan BY-01, uji ulang dengan Ny. Maria Veronica: bayi "Ferran" (RM 74) seharusnya dapat dilanjutkan admisinya tanpa membuat RM baru.

---

## 10. Lampiran Artefak

| Jenis | Lokasi |
|---|---|
| Screenshot (66 berkas) | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/nwb-bayi/` |
| Log jaringan | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/nwb-bayi/network.log` |
| Sambungan browser Chromium headed | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1026-open-browser.ps1`, `admisi-1026-cdp.mjs` |
| Skrip langkah uji | `admisi-1026-nwb-01-buka.mjs`, `admisi-1026-nwb-02-ibu.mjs`, `admisi-1026-nwb-picker.mjs`, `admisi-1026-nwb-identitas.mjs` (dengan pengaman pencegat `cegat:true`), `admisi-1026-nwb-perawatan.mjs`, `admisi-1026-bed-kamar.mjs`, `admisi-1026-bed-pilih.mjs`, `admisi-1026-pesan.mjs`, `admisi-1026-persetujuan.mjs` di folder `scripts/` yang sama |
| Kueri DB baca-saja | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1026-db.py` |
