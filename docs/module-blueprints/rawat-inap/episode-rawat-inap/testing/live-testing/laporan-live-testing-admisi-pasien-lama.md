# Laporan Live Browser Testing — Admisi Rawat Inap: Pendaftaran Pasien Lama

| Hal | Isi |
|---|---|
| Menu yang diuji | Rawat Inap → Admisi Rawat Inap → **Pendaftaran Pasien Lama** |
| Alamat layar | `http://localhost:3000/health-services/inpatient-management/admissions/existing-patient` |
| Sub-modul blueprint | `rawat-inap/episode-rawat-inap` |
| Tanggal uji | Senin, 26 Oktober 2026, pukul ±10.25–10.52 WIB |
| Penguji | AI Agent (Claude Code). Seluruh klik, isian, dan pemeriksaan dilakukan agent di browser. |
| Browser | Google Chrome 155 (engine Chromium) tampil di layar (*headed*), dikendalikan `playwright` lewat CDP, `slowMo` 300–450 ms, layar 1920×945 |
| Frontend | `http://localhost:3000` — cabang `HamzahV2`, commit `2cd1c0610` |
| Backend | `https://localhost:7184/api` — cabang `MHamzah`, commit `e095f656` |
| Basis data | `QuilvianNewDevHamzah` (PostgreSQL pengembang). Untuk menu ini agent **tidak** menulis data langsung ke DB; semua data terbentuk lewat layar. |
| Akun | `superadmin@admin.com` (kata sandi tidak dicatat di dokumen ini) |
| Laporan menu lain pada sesi yang sama | `laporan-live-testing-admisi-bayi-baru-lahir-20261026.md`, `laporan-live-testing-admisi-transfer-igd-poliklinik.md` |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: alur Pendaftaran Pasien Lama berjalan tuntas dari pencarian sampai cetak persetujuan, tanpa layar putih, *crash*, atau galat server. Menu ini layak dipakai dengan catatan, setelah pemilik proses memutuskan aturan admisi ganda (PL-01).**

| Prioritas | Test case | Lulus | Gagal | Lulus dengan catatan / perlu keputusan |
|---|---|---|---|---|
| P0 Fungsi Kritis | 4 | 4 | 0 | 0 |
| P1 Kepatuhan Bisnis | 7 | 6 | 0 | 1 |
| P2 & P3 Validasi dan Data Ngasal | 13 | 12 | 1 | 0 |
| P4 Tampilan | 2 | 0 | 1 | 1 |
| P5 Kenyamanan | 1 | 0 | 0 | 1 |
| **Total** | **27** | **22** | **2** | **3** |

Yang sudah berjalan baik:

- Admisi Bapak Subagyo Sutrisno tersimpan utuh: kunjungan, episode *Draft*, reservasi bed 2 jam, order deposit ke Billing, data rujukan faskes luar, dan persetujuan bertanda tangan.
- Klik ganda pada tombol pemesanan tidak membuat data ganda (1 kunjungan, 1 episode, 1 reservasi).
- Muat ulang halaman (F5) di tengah alur memulihkan langkah dan pemesanan.
- Pasien yang sedang dirawat diblokir dengan arahan "Pindah Tempat Tidur". Draft lama yang kedaluwarsa gugur otomatis.
- Pengaman input lengkap: pencarian kosong, injeksi SQL, skrip HTML, NIK berhuruf, rujukan bertanggal masa depan, isian berisi spasi saja, dan batas panjang teks.
- Klausul persetujuan wajib dicentang aktif (perbaikan dari temuan 25 Oktober) dan NIK penanda tangan kini divalidasi.
- 95 panggilan API terekam: 0 galat 5xx, 0 galat 4xx, 0 error JavaScript.

Yang perlu ditindaklanjuti:

| No | Temuan | Tingkat |
|---|---|---|
| PL-01 | Admisi kedua tetap dapat dibuat untuk pasien yang masih punya admisi *Draft* dengan bed dipesan; teks peringatannya memakai jam UTC dan bulan berbahasa Inggris | P1 · Perlu keputusan |
| PL-02 | Nominal deposit tidak punya batas atas wajar dan kehilangan presisi di atas Rp 9 kuadriliun | P3 · Sedang |
| PL-03 | Denah Lantai 3 memiliki dua kamar berlabel "ODC 321" | P4 · Rendah |
| PL-04 | Bilah footer melayang menutupi isian form | P4 · Sedang |
| PL-05 | Kumpulan catatan istilah dan umpan balik | P5 · Rendah |

---

## 2. Alur Proses Bisnis Pendaftaran Pasien Lama

**Pemicu:** pasien yang sudah memiliki No. RM datang untuk dirawat inap, mandiri atau dengan surat rujukan.

**Contoh nyata dari uji ini:** Bapak Subagyo Sutrisno (68 tahun, RM 00-00-00-39) datang membawa surat rujukan Puskesmas Kecamatan Setiabudi dengan diagnosis pneumonia komunitas dan SpO2 92%. Keluarga membayar tunai. Petugas meminta deposit Rp 5.000.000 (di bawah kebijakan minimum Rp 10.000.000, tetapi admisi tetap boleh berjalan). Dokter penyakit dalam menjadi DPJP, dan pasien dipesankan bed 329-A di Kelas II. Istri pasien menandatangani persetujuan.

| Langkah | Nama di layar | Yang dilakukan petugas | Yang dilakukan sistem |
|---|---|---|---|
| 1 | Pasien Lama | Mencari dengan No. RM, NIK, atau tanggal lahir, lalu memilih pasien | Menampilkan identitas dan memeriksa kelayakan: sedang dirawat? punya admisi yang sedang disiapkan? |
| 2 | Jenis Kunjungan & Kategori | Memilih Kunjungan Umum atau Rujukan, kategori pasien (Umum/Pegawai RS/ODC), dan mengisi data rujukan faskes luar | Menolak rujukan yang tidak lengkap atau bertanggal masa depan |
| 3 | Pembayaran | Memilih Tunai/Umum, Asuransi, atau Penjamin Perusahaan | Menghapus langkah Deposit bila cara bayar bukan Tunai |
| 4 | Deposit | Mengisi nominal deposit yang diminta | Menampilkan kebijakan minimum dari Billing; deposit tidak menahan admisi |
| 5 | Dokter | Memilih DPJP, konsulen (opsional), catatan, tingkat ketergantungan, kebutuhan isolasi | Memvalidasi isian |
| 6 | Pilih Bed | Memilih kamar dan bed di denah | Menonaktifkan kamar yang tidak layak (contoh: ODC untuk pasien non-ODC, kamar penuh) |
| 7 | Booking Bed | Menekan "Pesan Tempat Tidur" lalu "Ya, Pesan" | Membuat kunjungan, episode *Draft*, order deposit, dan reservasi ±2 jam |
| 8 | Konfirmasi | Memeriksa ringkasan, lalu "Kunci Admisi & Cetak Persetujuan" | Menyimpan perubahan catatan/unit bila ada |
| 9 | Form Persetujuan | Mengisi penanda tangan, menyetujui 12 klausul, membubuhkan dan mengunci tanda tangan | Menyimpan persetujuan bertanda tangan |
| 10 | Cetak Persetujuan | Mencetak lembar persetujuan | — |

**Hasil akhir yang diharapkan:** episode berstatus *Draft* dengan bed dipesan. Pasien menjadi *Sedang Dirawat* setelah kedatangannya dikonfirmasi di Papan Tempat Tidur. Bila tidak dikonfirmasi sampai reservasi habis, episode *Draft* gugur otomatis.

---

## 3. Data Uji

| Kebutuhan | Pasien | Kondisi di DB |
|---|---|---|
| Pasien layak untuk alur lengkap | SUBAGYO SUTRISNO (RM 00-00-00-39) | Tanpa episode, tanpa asuransi |
| Pasien yang sedang dirawat | Ny. ANISA LESTARI (RM 00-00-00-60) | Episode RI-261022-VIP-ANISA *Admitted* di VIP 302 |
| Pasien dengan draft lama | Budi Prasetyo (RM 00-00-00-59) | Episode *Draft* dari 23 Okt |
| Pencarian NIK | Budi Santoso (RM 00-00-00-17) | NIK 3171011205880001 |

---

## 4. Rincian Test Case

Keterangan: ✅ Lulus · ❌ Gagal · ⚠️ Lulus dengan catatan / perlu keputusan. Kolom "Bukti" menyebut nama atau awalan berkas di `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/ext-pasien-lama/` (tanpa `.png`).

Uji yang berisiko menyimpan data sampah (NIK penanda tangan) dijalankan dengan **pengaman pencegat**: bila layar meloloskan isian, request dihentikan di browser sebelum sampai ke server.

### P0 — Fungsi Kritis

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| PL-TC-01 | Membuka layar dari kartu "Pendaftaran Pasien Lama" | Langkah 1 dari 10 tampil tanpa galat | ✅ | `ext-01-layar-cari-pasien` |
| PL-TC-02 | Alur lengkap Bapak Subagyo: rujukan → Tunai → deposit Rp 5 jt → DPJP → bed 329-A → konfirmasi → persetujuan → cetak | Episode **RI-261026104414-CEE503** (*Draft*), kunjungan ENC-RSMMC-00219, reservasi s.d. 12.45 WIB, persetujuan 201 | ✅ | `ext-20` s.d. `ext-40` |
| PL-TC-03 | Klik ganda "Lanjut ke Pemesanan", "Pesan Tempat Tidur", dan "Ya, Pesan" | DB: 1 kunjungan, 1 episode, 1 reservasi; hanya 1 POST reservasi | ✅ | `ext-29`, `ext-30`, `ext-31` |
| PL-TC-04 | Muat ulang (F5) setelah bed dipesan | Langkah 7, nomor episode, dan sisa waktu pulih. Catatan: "Dipesan Pada" berubah menjadi "-" | ✅ | `ext-32-refresh-setelah-booking` |

### P1 — Kepatuhan Bisnis

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| PL-TC-05 | Pasien yang sedang dirawat (Ny. Anisa) | Diblokir: "Pasien tidak dapat diadmisi — Ny. ANISA LESTARI sudah dirawat pada episode RI-261022-VIP-ANISA di VIP 302 302-A. Bila memang pindah kamar, pakai perpindahan, bukan admisi baru." dengan tombol Pindah Tempat Tidur dan Lihat Detail Episode. Catatan: alasan baru tampil setelah tombol Lanjut ditekan | ✅ | `ext-05-blokir-sedang-dirawat`, `ext-06-anisa-klik-lanjut` |
| PL-TC-06 | Pasien dengan episode *Draft* lama (Budi Prasetyo) | Draft gugur otomatis saat kelayakan dibaca (status 0 → 4 sesuai `DraftEpisodeExpiryHours`); pasien boleh lanjut | ✅ | `ext-07-pasien-episode-draft` |
| PL-TC-07 | Pasien yang masih punya admisi *Draft* dengan bed dipesan (Subagyo, setelah PL-TC-02) | Hanya lencana "Ada Admisi Disiapkan"; admisi kedua tetap bisa dibuat | ⚠️ PL-01 | `ext-43b-langkah1-pasien-punya-draft` |
| PL-TC-08 | Deposit hanya untuk Tunai | Memilih Asuransi menghapus langkah Deposit (stepper 10 → 9 langkah) | ✅ | `ext-13-batal-dialog` |
| PL-TC-09 | Order deposit diteruskan ke Billing | DB: **DPO-20261026-00001**, Rp 5.000.000, UNPAID, jatuh tempo 29 Okt 2026; layar menjelaskan selisih Rp 5.000.000 terhadap kebijakan minimum | ✅ | `ext-24c-deposit-valid`, verifikasi DB |
| PL-TC-10 | Data rujukan faskes luar tersimpan | Nomor, faskes, dokter perujuk, dan waktu rujukan tersimpan di episode; tampil di Konfirmasi | ✅ | `ext-33-konfirmasi`, verifikasi DB |
| PL-TC-11 | Klausul persetujuan wajib disetujui aktif | Mulai "0 Dari 12 Disetujui"; membatalkan 1 klausul menjadikan "11 Dari 12" dan tombol Simpan nonaktif | ✅ | `ext-35-persetujuan-awal`, `ext-38-klausul-11-dari-12` |

### P2 & P3 — Fungsi, Validasi, dan Data Ngasal

| ID | Skenario | Masukan | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|---|
| PL-TC-12 | Cari dengan kotak kosong | — | Tombol Cari nonaktif, 0 request | ✅ | `ext-02-cari-kosong` |
| PL-TC-13 | No. RM tidak ada | `99-99-99-99` | "Pasien tidak ditemukan dengan kata kunci pencarian tersebut." + tombol Daftarkan sebagai Pasien Baru | ✅ | `ext-02-rm-tidak-ada` |
| PL-TC-14 | Injeksi SQL dan skrip di kotak cari | `' OR 1=1 --`, `<img src=x onerror=alert(1)>` | Dikirim ter-*encode*, hasil kosong, skrip tidak berjalan | ✅ | `ext-02-rm-sql-injection`, `ext-02-rm-xss` |
| PL-TC-15 | NIK berhuruf | `32abcdXYZ` | "NIK pasien harus terdiri dari 16 digit angka." tanpa request | ✅ | `ext-02-nik-huruf` |
| PL-TC-16 | NIK valid dan No. RM sebagian | `3171011205880001`, `39` | Pasien yang tepat ditemukan | ✅ | `ext-02-nik-valid`, `ext-02-rm-sebagian-39` |
| PL-TC-17 | Lanjut tanpa memilih pasien | — | Tombol Lanjut nonaktif | ✅ | `ext-03-lanjut-tanpa-pilih` |
| PL-TC-18 | Lanjut tanpa cara bayar; Asuransi untuk pasien tanpa polis | — | Lanjut nonaktif; "Belum ada asuransi tersimpan", syarat 1/2 | ✅ | `ext-11`, `ext-12` |
| PL-TC-19 | Batal Pendaftaran | — | Dialog "Batalkan Pendaftaran?", kembali ke menu, 0 request tulis, sessionStorage bersih | ✅ | `ext-13`, `ext-14` |
| PL-TC-20 | Rujukan kosong dan berisi spasi saja | `"   "` di semua kolom | Pesan per kolom + spanduk "Seluruh data rujukan faskes luar wajib diisi lengkap (Nomor, Tanggal/Jam, Faskes, Dokter, dan Diagnosa)." | ✅ | `ext-21-rujukan-kosong`, `ext-21-rujukan-spasi` |
| PL-TC-21 | Tanggal rujukan besok; nomor rujukan 260 karakter | 27 Okt 2026 | "Tanggal dan jam rujukan tidak boleh melewati waktu sekarang."; teks terpotong 250 karakter | ✅ | `ext-22a`, `ext-22b`, `ext-22c` |
| PL-TC-22 | Nominal deposit ngasal | `-500000`, `abc!@#`, `12,5`, dua puluh angka `9` | Minus dan huruf dibuang ✅; `12,5` menjadi Rp 125; **20 digit diterima dan tampil sebagai Rp 1.000.000.000.000.000**, petugas dapat lanjut | ❌ PL-02 | `ext-24a`, `ext-24b`, `ext-24c` |
| PL-TC-23 | DPJP kosong; catatan admisi 1.100 karakter; catatan klinis 600 karakter | — | Lanjut nonaktif tanpa DPJP; teks terpotong 1.000 dan 500 karakter | ✅ | `ext-26a`, `ext-26b` |
| PL-TC-24 | NIK dan No. HP penanda tangan sampah | `ABCDE<script>99999999999999`, `08abc12345678901234` | NIK disaring menjadi `ABCDEscript99999` lalu ditolak "NIK harus 16 angka, atau paspor 6–9 huruf/angka."; No. HP dibersihkan menjadi 13 digit; Simpan nonaktif; 0 request | ✅ | `ext-36-nik-sampaha-form-terisi` |

### P4 — Tampilan

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| PL-TC-25 | Label kamar di denah Lantai 3 | Dua kamar berlabel "ODC 321" | ❌ PL-03 | `ext-27b-pilih-bed-tunggu` |
| PL-TC-26 | Tampilan saat denah memuat; footer | Saat memuat tertulis "Belum ada kamar pada lantai ini."; bilah footer melayang menutupi isian | ⚠️ PL-04, PL-05 | `ext-27-pilih-bed`, `ext-36-nik-sampaha-form-terisi` |

### P5 — Kenyamanan Pemakaian

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| PL-TC-27 | Kejelasan istilah dan umpan balik | Lihat PL-05 | ⚠️ PL-05 | `ext-08`, `ext-34`, `ext-41-selesai` |

**Rekap:** 27 test case — 22 lulus, 2 gagal, 3 lulus dengan catatan / perlu keputusan.

---

## 5. Temuan Isu

### PL-01 — Admisi kedua dapat dibuat untuk pasien yang masih punya admisi sedang disiapkan · P1 · Perlu keputusan

**Apa yang terjadi.** Setelah PL-TC-02, Bapak Subagyo memiliki episode RI-261026104414-CEE503 berstatus *Draft* dengan bed 329-A dipesan. Saat pasien yang sama dicari lagi, server menjawab `canContinue: true` disertai peringatan, dan layar hanya menampilkan lencana "Ada Admisi Disiapkan". Petugas dapat melanjutkan sampai memesan bed kedua.

Teks peringatan dari server juga memakai jam UTC dan bulan berbahasa Inggris:

> "Pasien ini punya admisi lain yang sedang disiapkan, yaitu RI-261026104414-CEE503 sejak 26 October 2026 03:44. …"

Admisi tersebut sebenarnya dibuat pukul 10.44 WIB.

**Contoh dampak.** Petugas pagi memesankan bed 329-A untuk Bapak Subagyo. Petugas siang, yang tidak tahu, mendaftarkannya lagi dan memesan bed 412-B. Dua bed tertahan untuk satu pasien selama dua jam, dan Papan Tempat Tidur menampilkan dua pesanan atas nama yang sama.

**Rekomendasi.** Pemilik proses memutuskan apakah dua admisi yang sedang disiapkan untuk satu pasien boleh ada.
- Bila tidak boleh: jadikan blokir dengan tombol "Lanjutkan admisi RI-…".
- Bila boleh (misalnya admisi terpisah yang sah): tampilkan peringatan sebagai spanduk yang wajib dibaca, bukan hanya lencana.

Di kedua pilihan, perbaiki format waktu peringatan ke WIB dan bahasa Indonesia.

### PL-02 — Nominal deposit tanpa batas atas wajar dan kehilangan presisi · P3 · Sedang

**Apa yang terjadi.** Isian deposit menerima hingga 16 digit lalu memprosesnya sebagai `Number` JavaScript (`src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx:96-110`). Angka di atas Rp 9.007.199.254.740.991 kehilangan presisi. Mengetik dua puluh angka 9 menghasilkan tampilan "1.000.000.000.000.000", dan petugas tetap dapat lanjut ke langkah Dokter. Ketikan "12,5" (maksud petugas: 12,5 juta) menjadi Rp 125 tanpa peringatan.

**Contoh dampak.** Petugas salah ketik satu angka nol berlebih berkali-kali, dan keluarga pasien menerima order deposit bernilai miliaran di Kasir.

**Rekomendasi.** Tetapkan batas atas wajar (misalnya mengikuti kebijakan Billing atau ≤ Rp 1 miliar) dengan pesan bila terlampaui. Pertimbangkan peringatan bila petugas mengetik koma.

### PL-03 — Label kamar ganda "ODC 321" di denah Lantai 3 · P4 · Rendah

Denah Lantai 3 memuat dua kamar berlabel "ODC 321" (urutan dari kiri: 322, 321, 321, 320). Daftar kamar dari layar juga mengembalikan dua entri "ODC 321". Petugas dapat salah menyebut kamar saat serah terima. **Rekomendasi:** perbaiki nama kamar pada master data atau pemetaan denah.

### PL-04 — Bilah footer melayang menutupi isian form · P4 · Sedang

Bilah "Privacy Policy · Terms of Use · Quilvian System" melayang di bagian bawah layar dan menutupi isian saat halaman digulir, misalnya kolom "Hubungan dengan Pasien" dan "No. Handphone" di Form Persetujuan serta bagian kanvas tanda tangan. Klik pada area itu tidak sampai ke isian di bawahnya. Temuan ini juga terlihat di menu Bayi dan Transfer. **Rekomendasi:** footer tidak melayang di atas konten, atau konten diberi ruang bawah setinggi footer.

### PL-05 — Catatan istilah dan umpan balik · P5 · Rendah

1. Lencana "Sedang Dirawat" tampil, tetapi tombol Lanjut tetap aktif; alasan blokir baru muncul setelah diklik.
2. Pilihan bawaan di langkah 2 (Kunjungan Umum, Pasien Umum) hanya ditandai warna ikon, sehingga sulit dikenali.
3. Tombol "Kunci Admisi" tidak menulis apa pun ke server bila tidak ada perubahan, tetapi lembar cetak menyatakan "Admisi sudah dikunci pada langkah Konfirmasi". Penguncian yang sebenarnya hanya status di layar.
4. Tombol "Selesai" di langkah terakhir memunculkan peringatan "Tinggalkan Admisi?" walau semua langkah sudah tuntas.
5. Halaman Konfirmasi tidak menampilkan order deposit dan tingkat ketergantungan.
6. Saat denah memuat, tertulis "Belum ada kamar pada lantai ini." sebelum data datang.
7. Setelah F5, "Dipesan Pada" di Booking Bed menjadi "-".
8. Kolom `PlannedDepositAmount` episode kosong di jalur ini (order deposit tetap terbit), sedangkan jalur Transfer mengisinya.
9. Picker tanggal rujukan masih mengizinkan klik tanggal masa depan (ditolak saat lanjut).

---

## 6. Endpoint yang Terlibat (Gaya Swagger)

Semua endpoint memakai `[Authorize]` (sesi login cookie).

### `[Tags("Health Services / Patient Management / Master Data / Patient")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| GET | `/api/v1/health-services/patient-management/master-data/patients/admin` | Cari pasien terdaftar | `isActive`, `medicalRecordNumber` / `identityNumber` / tanggal lahir, `pageNumber`, `pageSize` | 200 |
| GET | `/api/v1/health-services/patient-management/master-data/patients/admin/{id}` | Detail identitas pasien | — | 200 |

### `[Tags("Health Services / Inpatient Management / Inpatient Episode")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility` | Kelayakan admisi | `patientId` | 200 — `canContinue`, `blockers[]`, `presentEpisode`, `draftEpisode`, `warnings[]` |
| POST | `/api/v1/health-services/inpatient-management/episodes` | Episode *Draft* + order deposit awal | `patientId`, `encounterId`, `serviceUnitId`, `patientClassId`, `doctorId`, `externalReferral`, `notes`, `selectedBedId`, `requestedDepositAmount`, `depositPolicyId` | 200 |
| GET | `/api/v1/health-services/inpatient-management/episodes/{id}` | Detail episode | — | 200 |

### `[Tags("Health Services / Registration Management / Patient Encounter")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| POST | `/api/v1/health-services/registration-management/patient-encounters/admin` | Kunjungan rawat inap | 200 — ENC-RSMMC-00219 |

### `[Tags("Health Services / Inpatient Management / Bed Occupancy")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Ringkasan bed per unit | 200 |
| GET | `/api/v1/health-services/inpatient-management/bed-occupancies/floor-plan` | Denah dengan kelayakan pasien (`floorCode`, `purpose=Admission`, `episodeId`) | 200 |
| POST | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations` | Menahan bed | 200 "Pemesanan tersimpan." |

### `[Tags("Health Services / Inpatient Management / Inpatient Admission Consent")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| POST | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` | Persetujuan umum dan tanda tangan digital | `signerType`, `signerName`, `signerIdentityNumber`, `agreedClauses[]`, `signatureImageBase64` | 201 "Formulir persetujuan rawat inap dan tanda tangan digital berhasil disimpan." |

### `[Tags("Health Services / Billing Management / Billing / Patient Funds")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| GET | `/api/v1/health-services/billing-management/billing/patient-funds/deposit-policies` | Kebijakan minimum deposit | 200 — minimum Rp 10.000.000, jatuh tempo 3 hari kerja |

Endpoint pendukung yang juga dipanggil (semua 200): `patient-memberships/admin/patients/{id}/active`, `patient-insurances/admin/options`, `patient-company-guarantors/admin/options`, `admissions/surgical-booking-lookup`, `master-data/service-units/options`, `patient-classes/options`, `rooms/options`.

**Ringkasan status jaringan yang terekam untuk menu ini:** 95 panggilan — GET 2xx ×93, POST 2xx ×2, 4xx ×0, 5xx ×0, error JavaScript ×0. (Sebagian panggilan dibuat oleh skrip pendek yang tidak merekam log; seluruh penyimpanan diverifikasi langsung di DB.)

---

## 7. Data Uji yang Terbentuk di `QuilvianNewDevHamzah`

| Data | Nilai |
|---|---|
| Pasien | SUBAGYO SUTRISNO (RM 00-00-00-39) |
| Kunjungan | ENC-RSMMC-00219 (Tunai) |
| Episode | RI-261026104414-CEE503 — *Draft*, DPJP dr. Bagus Purnama Sanjaya, High Dependency, rujukan Puskesmas Kecamatan Setiabudi |
| Bed | 329-A (KELAS II 329), dipesan s.d. ±12.45 WIB |
| Order deposit | DPO-20261026-00001 — Rp 5.000.000, UNPAID |
| Persetujuan | Penanda tangan Sri Wahyuningsih (istri), 12 klausul, tanda tangan digital |

Dua percobaan lain (Budi Prasetyo dan admisi kedua Bapak Subagyo) dibatalkan sebelum menyimpan apa pun. Episode *Draft* di atas akan gugur otomatis bila kedatangan tidak dikonfirmasi di Papan Tempat Tidur sebelum reservasi habis. Agent tidak menghapus data apa pun.

---

## 8. Rekomendasi Urutan Perbaikan

1. Keputusan pemilik proses untuk **PL-01**, lalu terapkan blokir atau peringatan yang jelas.
2. **PL-04** footer, karena menghalangi isian di semua menu admisi.
3. **PL-02** batas deposit.
4. **PL-03** dan **PL-05** pada rilis berikutnya.

---

## 9. Lampiran Artefak

| Jenis | Lokasi |
|---|---|
| Screenshot (59 berkas) | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/ext-pasien-lama/` |
| Log jaringan | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/ext-pasien-lama/network.log` |
| Sambungan browser Chromium headed | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1026-open-browser.ps1`, `admisi-1026-cdp.mjs` |
| Skrip langkah uji | `admisi-1026-ext-*.mjs`, `admisi-1026-click.mjs`, `admisi-1026-deposit.mjs`, `admisi-1026-dokter.mjs`, `admisi-1026-bed-kamar.mjs`, `admisi-1026-bed-pilih.mjs`, `admisi-1026-pesan.mjs`, `admisi-1026-persetujuan.mjs`, `admisi-1026-reload.mjs` di folder `scripts/` yang sama |
| Kueri DB baca-saja | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1026-db.py` (koneksi dibaca dari `appsettings.Development.json`) |
