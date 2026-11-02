# Laporan Live Browser Testing — Admisi Rawat Inap: Admisi Transfer IGD & Poliklinik

| Hal | Isi |
|---|---|
| Menu yang diuji | Rawat Inap → Admisi Rawat Inap → **Admisi Transfer IGD & Poliklinik** |
| Alamat layar | `http://localhost:3000/health-services/inpatient-management/admissions/transfer` |
| Sub-modul blueprint | `rawat-inap/episode-rawat-inap` |
| Tanggal uji | Senin, 26 Oktober 2026, pukul ±11.13–11.35 WIB |
| Penguji | AI Agent (Claude Code). Seluruh klik, isian, dan pemeriksaan dilakukan agent di browser. |
| Browser | Google Chrome 155 (engine Chromium) tampil di layar (*headed*), dikendalikan `playwright` lewat CDP, `slowMo` 300–450 ms, layar 1920×945 |
| Frontend | `http://localhost:3000` — cabang `HamzahV2`, commit `2cd1c0610` |
| Backend | `https://localhost:7184/api` — cabang `MHamzah`, commit `e095f656` |
| Basis data | `QuilvianNewDevHamzah` (PostgreSQL pengembang) |
| Izin data | Antrean transfer **kosong** sebelum uji. Pemilik DB memberi izin tertulis untuk menyiapkan data uji langsung di DB; agent menulis **12 baris** data sumber transfer (Bagian 4). Semua admisi dibuat lewat layar aplikasi. |
| Akun | `superadmin@admin.com` (kata sandi tidak dicatat di dokumen ini) |
| Laporan menu lain pada sesi yang sama | `laporan-live-testing-admisi-pasien-lama.md`, `laporan-live-testing-admisi-bayi-baru-lahir-20261026.md` |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: transaksi inti transfer sudah kuat — atomik, aman dari klik ganda, dan tidak meninggalkan data sisa saat ditolak. Namun menu ini belum layak dipakai di lapangan sebelum empat temuan P1 diperbaiki: penanganan pasien isolasi, pemeriksaan kelayakan, pembatalan, dan konsulen yang hilang.**

| Prioritas | Test case | Lulus | Gagal | Lulus dengan catatan |
|---|---|---|---|---|
| P0 Fungsi Kritis | 6 | 6 | 0 | 0 |
| P1 Kepatuhan Bisnis dan Keselamatan | 5 | 0 | 4 | 1 |
| P2 & P3 Fungsi dan Validasi | 3 | 3 | 0 | 0 |
| P4 & P5 Tampilan dan Kenyamanan | 1 | 0 | 0 | 1 |
| **Total** | **15** | **9** | **4** | **2** |

Yang sudah berjalan baik:

- Antrean menampilkan permintaan dari IGD dan Poli, dengan CITO di urutan teratas dan penanda "Melebihi 60 menit".
- Verifikasi menampilkan identitas, TTV yang diurai rapi dari teks IGD, dan instruksi tindak lanjut.
- Transaksi `admit` membuat kunjungan, penjamin, episode, reservasi, order deposit, dan menandai disposisi IGD *Executed* sekaligus.
- Klik ganda "Ya, Pesan" hanya menghasilkan satu transaksi.
- Saat server menolak (409/422), tidak ada kunjungan atau episode yang tertinggal, dan isian petugas tetap utuh.
- Permintaan yang sudah diproses hilang dari antrean; membuka ulang URL-nya tidak membuat admisi ganda.
- 195 panggilan API terekam: 0 galat 5xx, 0 error JavaScript; 2 respons 4xx adalah penolakan bisnis yang memang diuji.

Yang harus diperbaiki lebih dulu:

| No | Temuan | Tingkat | Dampak di rumah sakit |
|---|---|---|---|
| TR-01 | Pasien IGD yang **butuh isolasi** tidak bisa diadmisi dan layar menawarkan bangsal biasa; master data tidak punya bed isolasi | P1 · **Kritis** | Risiko penularan airborne bila petugas mematikan tanda isolasi agar bisa memesan bed |
| TR-02 | Tidak ada pemeriksaan kelayakan di langkah Verifikasi; pasien yang sedang dirawat baru ditolak di langkah 7 | P1 · Tinggi | Waktu petugas terbuang; antrean memuat permintaan yang mustahil diproses |
| TR-03 | Admisi transfer yang dibatalkan **tidak kembali ke antrean** | P1 · Tinggi (perlu keputusan) | Pasien IGD tidak bisa diproses ulang dari antrean; tautan ke kunjungan IGD terputus |
| TR-04 | **Konsulen dan tingkat ketergantungan tidak tersimpan** | P2 · Tinggi | Konsulen tidak mendapat penugasan; perencanaan perawat memakai data kosong |
| TR-05 | Sebagian data rujukan IGD/Poli tidak terbawa ke layar | P2 · Sedang | DPJP terisi dokter jaga IGD, bukan DPJP yang diusulkan |
| TR-06 | Teks, status, dan kode galat yang belum rapi | P4/P5 · Rendah | — |
| TR-07 | Bilah footer melayang menutupi isian (juga di menu lain) | P4 · Sedang | — |

---

## 2. Alur Proses Bisnis Admisi Transfer

**Pemicu:**
- **IGD:** dokter jaga menetapkan disposisi "Rawat Inap" dan menandatangani permintaan rawat inap (`EmgAdmissionRequest` berstatus *Signed*).
- **Poliklinik:** dokter menerbitkan Surat Pengantar Rawat Inap (`CliDoctorCertificate` jenis 4, status *Terbit*).

**Contoh nyata dari uji ini:** Bapak Hendro Kusuma (61 tahun) datang ke IGD dengan sesak napas berat dan nyeri dada; dokter jaga menduga NSTEMI dengan gagal jantung akut dan menetapkan rawat inap **CITO**. Setelah menunggu 113 menit, petugas admisi membuka antrean, memverifikasi rujukan, memilih bayar Tunai dengan deposit Rp 10.000.000, memilih dokter jantung sebagai DPJP, lalu memesan VIP 306. Pasien sendiri menandatangani persetujuan.

| Langkah | Nama di layar | Yang dilakukan petugas | Yang dilakukan sistem |
|---|---|---|---|
| 1 | Antrean Transfer IGD/Poli | Mencari/menyaring permintaan, menekan "Proses Admisi" | Mengurutkan: CITO → member prioritas → lama tunggu |
| 2 | Verifikasi Pasien | Mencocokkan identitas dan instruksi rujukan | Menampilkan identitas, TTV, alasan, dan instruksi dari IGD/Poli |
| 3 | Pembayaran | Memilih Tunai, Asuransi, atau Penjamin Perusahaan | Menghapus langkah Deposit bila bukan Tunai |
| 4 | Deposit | Mengisi nominal deposit | — |
| 5 | Dokter | Memeriksa DPJP (terisi dari dokter perujuk), konsulen, catatan, ketergantungan, isolasi | Memvalidasi isian |
| 6 | Pilih Bed | Memilih kamar dan bed | — |
| 7 | Booking Bed | "Pesan Tempat Tidur" → "Ya, Pesan" | Satu transaksi `admit`: kunjungan, penjamin, episode *Draft*, isolasi, reservasi ±2 jam, deposit, disposisi IGD *Executed* |
| 8 | Konfirmasi | Memeriksa ringkasan, "Kunci Admisi & Cetak Persetujuan" | — |
| 9 | Form Persetujuan | Penanda tangan, 12 klausul, tanda tangan | Menyimpan persetujuan |
| 10 | Cetak Persetujuan | Mencetak | Pasien menjadi *Sedang Dirawat* setelah kedatangan dikonfirmasi di Papan Tempat Tidur |

---

## 3. Data Uji

### 3.1 Permintaan transfer yang disiapkan agent

Antrean kosong sebelum uji. Satu-satunya disposisi IGD lama di DB sudah *Executed*, dan enam Surat Pengantar Poli lama sudah memiliki hasil admisi. Agent menyiapkan empat permintaan baru dengan skrip `admisi-1026-seed-transfer.py`, yang menyalin pola baris seed yang sudah ada (`11111111-*` untuk IGD, `22222222-*` untuk Poli).

| Kode | Asal | Pasien | Isi klinis | Tujuan uji |
|---|---|---|---|---|
| `EMG-AGY-1026-01` | IGD, **CITO**, diputuskan 75 menit sebelum uji | Hendro Kusuma (RM 00-00-00-62) | Suspek NSTEMI, SpO2 91% | Alur lengkap IGD, urutan CITO |
| `SPRI/2026/10/AGY02` | Poli | Dewi Lestari (RM 00-00-00-63) | DBD dengan tanda bahaya, minta Kelas I | Alur lengkap Poli |
| `SPRI/2026/10/AGY04` | Poli | Ny. ANISA LESTARI (RM 00-00-00-60, **sedang dirawat** di VIP 302) | Hipertensi urgensi | Pemeriksaan kelayakan |
| `EMG-AGY-1026-03` | IGD, butuh isolasi (`NeedsIsolation = true`) | REZA FIRMANSYAH (RM 00-00-00-41) | Suspek TB paru aktif | Penanganan isolasi dan pembatalan |

Tabel yang ditulis: `RegPatientEncounter` (4 baris), `EmgVisit` (2), `EmgDisposition` (2, status *Confirmed*), `EmgAdmissionRequest` (2, status *Signed*), `CliDoctorCertificate` (2). Semua Id berawalan `a1026000-0000-4000-8000-` dan catatan berisi "DATA UJI AGENT 2026-10-26".

---

## 4. Rincian Test Case

Keterangan: ✅ Lulus · ❌ Gagal · ⚠️ Lulus dengan catatan / perlu keputusan. Kolom "Bukti" menyebut nama atau awalan berkas di `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/trf-transfer/` (tanpa `.png`).

### P0 — Fungsi Kritis

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| TR-TC-01 | Antrean termuat dan berurutan | "4 Pasien Sesuai Filter", "3 Menunggu > 60 Mnt"; Hendro (CITO) teratas meski bukan yang terlama, lalu Dewi (158 mnt), Anisa (78 mnt), Reza (58 mnt) | ✅ | `trf-01-antrean` |
| TR-TC-02 | Alur lengkap IGD CITO (Hendro, Tunai, deposit Rp 10 jt, VIP 306-A) | `admit` 201; episode **RI-261026111914-67CACD** *Draft*; disposisi IGD *Executed*; order **DPO-20261026-00002** Rp 10 jt UNPAID; `PlannedDepositAmount` 10 jt; DPJP dr. Nadia Anggun Hutajulu; persetujuan tersimpan | ✅ | `trf-20` s.d. `trf-30` |
| TR-TC-03 | Alur lengkap Poli (Dewi, Tunai tanpa deposit, KELAS I 330-B) | Episode **RI-261026112302-11FF94**; sumber `Outpatient`; tanpa order deposit; persetujuan tersimpan | ✅ | `trf-40` s.d. `trf-49` |
| TR-TC-04 | Klik ganda "Ya, Pesan" | Hanya 1 POST `admit`; 1 reservasi di DB | ✅ | `trf-26-hendro…` |
| TR-TC-05 | Penolakan server tidak meninggalkan data; isian tetap utuh | Setelah 409 (Anisa) dan 422 (Reza): 0 kunjungan baru, 0 episode baru. "Periksa Ulang" mengembalikan layar Booking dengan bed, DPJP, dan isian lain utuh | ✅ | `trf-16-anisa…`, `trf-58-reza-isolasi…`, `trf-59-reza-periksa-ulang` |
| TR-TC-06 | Permintaan yang sudah diproses | Hendro hilang dari antrean. Membuka ulang URL transfernya melanjutkan episode yang sama di langkah Konfirmasi, tidak membuat admisi baru | ✅ | `trf-31-antrean-setelah-hendro`, `trf-32-url-transfer-sudah-diproses` |

### P1 — Kepatuhan Bisnis dan Keselamatan

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| TR-TC-07 | Pasien yang sedang dirawat ada di antrean (Ny. Anisa) | Verifikasi tidak memeriksa kelayakan dan berlencana "TERDAFTAR"; enam langkah dapat diisi; baru ditolak **409 PATIENT_ALREADY_ADMITTED** di langkah 7 | ❌ TR-02 | `trf-10-anisaa-verifikasi`, `trf-12-anisa…`, `trf-16-anisa…` |
| TR-TC-08 | Pasien yang butuh isolasi (Reza, suspek TB) | Tidak ada penanda isolasi dari IGD; sakelar isolasi tidak tercentang otomatis; denah menawarkan bangsal biasa; master 0 bed isolasi; ditolak **422** di langkah 7 | ❌ TR-01 | `trf-50-rezaa-verifikasi`, `trf-53-reza-pilih-bed-isolasi`, `trf-55-reza-dialog-328`, `trf-58-reza-isolasi…`; daftar status kamar Lantai 2–9 dari keluaran skrip `admisi-1026-bed-kamar.mjs` |
| TR-TC-09 | Konsulen dan tingkat ketergantungan | Konsulen dr. Arif Lesmana dan ketergantungan High/Low tidak tersimpan di DB; Konfirmasi tidak menampilkan konsulen | ❌ TR-04 | `trf-22-hendroc-dokter`, `trf-52-reza-dokter-konsulen-isolasi`, `trf-65-reza-konfirmasi`, verifikasi DB |
| TR-TC-10 | Batalkan admisi transfer (Reza) | Alasan wajib; episode *Cancelled*; bed 307-A dilepas. Permintaan IGD **tidak kembali ke antrean** | ❌ TR-03 | `trf-70-reza-dialog-batal-admisi`, `trf-71-reza-setelah-batal`, `trf-72-antrean-setelah-batal` |
| TR-TC-11 | Kelengkapan data rujukan di Verifikasi | DPJP usulan IGD, kelas kamar yang diminta Poli, diagnosis surat pengantar, dan kebutuhan O2/monitoring tidak tampil | ⚠️ TR-05 | `trf-20-hendroa-verifikasi`, `trf-40-dewia-verifikasi` |

### P2 & P3 — Fungsi dan Validasi

| ID | Skenario | Masukan | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|---|
| TR-TC-12 | Cari dan filter antrean | `reza`, `00-00-00-63`, `ENC-EMG-AGY-1026-01`, `zzzz`, `<script>`; filter Asal "Poli" | Hasil tepat; dua masukan terakhir kosong tanpa galat; filter Poli hanya menampilkan Dewi dan Anisa | ✅ | `trf-02-cari-xss`, `trf-03-filter-poli` |
| TR-TC-13 | Pembatalan admisi | Tanpa alasan, lalu dengan alasan | Tombol "Batalkan Admisi" nonaktif tanpa alasan; setelah diisi, `PATCH …/cancel` 200 dan 307-A kembali *Tersedia* | ✅ | `trf-70`, `trf-71`, verifikasi DB |
| TR-TC-14 | Isian otomatis dari rujukan | — | DPJP terisi dokter perujuk; catatan admisi terisi alasan disposisi | ✅ | `trf-22-hendroc-dokter` |

### P4 & P5 — Tampilan dan Kenyamanan

| ID | Skenario | Hasil yang diamati | Hasil | Bukti |
|---|---|---|---|---|
| TR-TC-15 | Teks dan status | Lihat TR-06 dan TR-07 | ⚠️ | `trf-27-hendro-konfirmasi`, `trf-16-anisa…`, `trf-50-rezaa-verifikasi` |

**Rekap:** 15 test case — 9 lulus, 4 gagal, 2 lulus dengan catatan / perlu keputusan.

---

## 5. Temuan Isu

### TR-01 — Pasien yang butuh isolasi tidak dapat diadmisi, dan layar menawarkan bangsal biasa · P1 · Kritis

**Apa yang terjadi.** Permintaan rawat inap IGD untuk Reza Firmansyah (suspek TB paru, `NeedsIsolation = true`) masuk ke antrean.

1. Antrean dan Verifikasi tidak menandai kebutuhan isolasi. Informasinya hanya ada di teks bebas: "Rawat di ruang isolasi tekanan negatif, masker N95 untuk petugas…".
2. Di langkah Dokter, sakelar "Pasien membutuhkan isolasi" **tidak tercentang otomatis**.
3. Setelah petugas menyalakannya sendiri dan mengisi keterangan, denah tetap mengaktifkan:
   - bangsal Kelas II 3-bed, termasuk **KELAS II 329 yang sudah dipesan untuk Bapak Subagyo (68 th, pneumonia)**;
   - Kelas I 428/429 yang ditempati ibu nifas;
   - kamar ODC;
   - VIP 304/305 yang sudah penuh tidak ditandai nonaktif.
4. Setelah "Ya, Pesan", server menolak: "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat tidur isolasi." (HTTP 422, kode `BED_NOT_READY`), dengan judul layar "Gagal Memuat Permintaan Rawat Inap".
5. Master data saat ini memiliki **0 bed** dengan `IsIsolationBed = true`, sehingga tidak ada bed yang bisa dipilih untuk pasien ini.

**Contoh dampak.** Petugas admisi jaga malam yang terus ditolak akan mematikan sakelar isolasi agar pasien bisa dipesankan bed. Uji ini membuktikan jalan pintas itu berhasil: tanpa sakelar isolasi, episode RI-261026113134-555BA6 terbentuk di VIP 307 (kemudian dibatalkan agent). Bila yang dipilih bangsal Kelas II, pasien TB airborne dirawat sekamar dengan pasien lain.

**Akar masalah.**
- Jalur transfer memanggil `bed-occupancies/available-beds` dan `bed-management/floor-plan` tanpa konteks pasien atau isolasi, karena episode baru dibuat saat `admit`. Kelayakan bed khusus pasien (Aturan 7 di `InpBedOccupancyService.cs:1748`) baru dinilai server ketika reservasi.
- `InpAdmissionTransferService.Queries.cs` tidak memetakan `EmgAdmissionRequest.NeedsIsolation` ke respons antrean.
- Semua kegagalan reservasi transfer dibungkus kode `BED_NOT_READY` (`InpAdmissionTransferService.cs:153-154`), sehingga kode aslinya (`ISOLATION_REQUIRED`) hilang.

**Rekomendasi.**
1. Tetapkan bed isolasi di master data, atau ambil keputusan pemilik proses bila RS memang belum punya ruang isolasi (misalnya kamar tunggal sebagai isolasi sementara).
2. Bawa `NeedsIsolation`, `NeedsIntensiveCare`, `NeedsOxygenSupport`, dan `NeedsSpecialMonitoring` ke respons antrean; tandai di antrean dan Verifikasi; isi otomatis sakelar isolasi.
3. Denah transfer meminta kelayakan dengan konteks pasien dan isolasi, seperti jalur Pasien Lama.
4. Teruskan kode galat asli ke layar.

### TR-02 — Tidak ada pemeriksaan kelayakan di langkah Verifikasi · P1 · Tinggi

**Apa yang terjadi.** Ny. Anisa Lestari sedang dirawat di VIP 302. Surat Pengantar Rawat Inap untuknya tampil di antrean tanpa peringatan. Layar Verifikasi tidak memanggil `episodes/admission-eligibility` (menu Pasien Lama memanggilnya), menampilkan lencana "TERDAFTAR", dan membolehkan Lanjut. Petugas dapat mengisi pembayaran, deposit, DPJP, dan memilih bed KELAS II 328-A. Penolakan baru muncul setelah "Ya, Pesan":

> 409 — "Ny. ANISA LESTARI sudah dirawat pada episode RI-261022-VIP-ANISA di VIP 302 302-A. Bila memang pindah kamar, pakai perpindahan, bukan admisi baru." (`PATIENT_ALREADY_ADMITTED`)

Datanya aman: server memblokir sebelum menulis apa pun (`FindPresentEpisodeAsync`).

**Contoh dampak.** Dokter Poli keliru menerbitkan surat pengantar untuk pasien yang sudah dirawat. Petugas admisi baru mengetahuinya setelah mengisi enam langkah dan membicarakan deposit dengan keluarga.

**Rekomendasi.** Periksa kelayakan saat permintaan dibuka (langkah 2) dan tampilkan alasan beserta tindakan (Pindah Tempat Tidur / Lihat Episode), seperti di menu Pasien Lama. Tandai permintaan seperti ini di antrean. Ganti judul galat penyimpanan menjadi "Admisi tidak dapat disimpan".

### TR-03 — Admisi transfer yang dibatalkan tidak kembali ke antrean · P1 · Tinggi (perlu keputusan)

**Apa yang terjadi.** Admisi transfer Reza dibatalkan dengan alasan. Episode menjadi *Cancelled* dan bed 307-A dilepas, tetapi:

- `EmgDisposition.DispositionStatus` tetap 3 (*Executed*);
- kunjungan rawat inap yang dibatalkan masih menyimpan `AdmissionTransferId`;
- antrean hanya menampilkan permintaan *Confirmed*, sehingga Reza hilang dari antrean (antrean tinggal berisi Ny. Anisa);
- server menolak `admit` ulang untuk permintaan yang sudah punya kunjungan hasil (`TRANSFER_ALREADY_PROCESSED`).

**Contoh dampak.** Layar Konfirmasi sendiri berkata: "DPJP dan penjamin sudah melekat pada kunjungan. Jika keliru, admisi harus dibatalkan dan dibuka ulang." Bila petugas mengikuti saran ini, pasien IGD tidak lagi muncul di antrean. Admisi ulang terpaksa lewat menu Pasien Lama, sehingga tautan ke kunjungan IGD, pewarisan penjamin, dan status CITO hilang.

**Rekomendasi.** Pemilik proses memilih salah satu:
- (a) pembatalan admisi transfer mengembalikan permintaan ke *Confirmed* dan melepas tautan kunjungan, sehingga bisa diproses ulang; atau
- (b) permintaan ditutup permanen dan IGD/Poli wajib menerbitkan permintaan baru.

Setelah itu, sesuaikan teks di layar Konfirmasi dan dialog pembatalan.

### TR-04 — Konsulen dan tingkat ketergantungan hilang · P2 · Tinggi

**Apa yang terjadi.**

| Episode | Menu | Ketergantungan dipilih | `CareDependencyLevel` di DB | Konsulen ditambahkan | Penugasan dokter di DB |
|---|---|---|---|---|---|
| RI-261026104414-CEE503 | Pasien Lama (pembanding) | High | 1 (High) ✅ | — | DPJP |
| RI-261026111914-67CACD | Transfer IGD | High | **kosong** | — | DPJP |
| RI-261026113134-555BA6 | Transfer IGD | Low | **kosong** | dr. Arif Lesmana (Konsulen, Bedah) | **hanya DPJP** |

Layar Konfirmasi tidak menampilkan konsulen, sehingga petugas tidak sadar datanya hilang.

**Akar masalah.** Menu Pasien Lama menyimpan dokter pendukung dan tingkat ketergantungan setelah episode terbentuk (`src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx`, sekitar baris 805–880). Jalur transfer memakai `transfer.submit` lalu `adoptTransferEpisode` (baris 955–960), yang tidak menjalankan kedua penyimpanan itu.

**Rekomendasi.** Setelah `admit` berhasil, jalankan penyimpanan dokter pendukung dan ketergantungan yang sama (atau sertakan keduanya di payload `admit`). Tampilkan konsulen di Konfirmasi.

### TR-05 — Data rujukan IGD/Poli yang tidak terbawa · P2 · Sedang

| Data sumber | Ada di DB | Tampil di layar |
|---|---|---|
| DPJP rawat inap yang diusulkan IGD (`EmgAdmissionRequest.ProposedInpatientDoctorId` — dr. Rendy Pangalila) | Ya | Tidak. "DPJP DITUJU / PEMERIKSA" berisi dokter jaga IGD yang memutuskan (dr. Joni Abraham Pratama, Ortopedi), lalu dipakai sebagai DPJP terisi otomatis untuk pasien jantung |
| Kelas kamar yang diminta Poli (`RequestedRoomClass` "Kelas I") | Ya | Tidak. "Rekomendasi Ruang / Bangsal" hanya "Rawat Inap" |
| Diagnosis surat pengantar ("Demam Berdarah Dengue", "Hipertensi Urgensi") | Ya | Tidak. Kolom "Diagnosa / Indikasi Rawat Inap" berisi alasan rujukan |
| Kebutuhan O2, monitoring khusus, perkiraan lama rawat dari IGD | Ya | Tidak |
| Jenis kelamin, NIK, telepon | Ya, di master pasien | `null` di respons API antrean (layar Verifikasi mengambil ulang dari master pasien) |

**Rekomendasi.** Lengkapi respons `admission-transfers`, tampilkan di Verifikasi, dan isi DPJP otomatis dari DPJP yang diusulkan, bukan dari dokter jaga IGD.

### TR-06 — Teks, status, dan kode galat · P4/P5 · Rendah

1. Status bed di Konfirmasi tampil "Active" (bahasa Inggris), sedangkan menu Pasien Lama menulis "Dipesan".
2. Judul "Gagal Memuat Permintaan Rawat Inap" dipakai untuk penolakan simpan, padahal permintaan berhasil dimuat.
3. Kode galat isolasi tampil sebagai `BED_NOT_READY` (lihat TR-01).
4. "Catatan Disposisi: ." — sisa tanda baca setelah TTV diurai dari teks IGD.
5. Nomor kunjungan hasil transfer berformat `ENC-RSMMC-261026111914-1EB311`, berbeda dari menu lain (`ENC-RSMMC-00219`).
6. `admit` menjawab HTTP 201, tetapi `statusCode` di badan respons 200.
7. Membuka ulang URL transfer yang sudah lengkap kembali ke Konfirmasi dan meminta "Lengkapi persetujuan pasien" padahal persetujuan sudah tersimpan.
8. Denah membuka lantai terakhir yang dipakai sesi (Lantai 2 sisa menu Bayi), bukan lantai yang relevan.

### TR-07 — Bilah footer melayang menutupi isian form · P4 · Sedang

Bilah "Privacy Policy · Terms of Use · Quilvian System" melayang di bawah layar dan menutupi isian saat digulir, termasuk kanvas tanda tangan Form Persetujuan. Temuan yang sama tercatat di menu Pasien Lama (PL-04) dan Bayi (BY-03).

---

## 6. Endpoint yang Terlibat (Gaya Swagger)

Semua endpoint memakai `[Authorize]` (sesi login cookie).

### `[Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/admission-transfers` | Antrean transfer IGD/Poli | `status` (bawaan `Confirmed`), `sourceType`, `search`, `overdueOnly`, `pageNumber`, `pageSize` | 200 — `transferId`, `sourceType`, `patientName`, `isCito`, `waitingMinutes`, `isOverdue`, `dispositionReason`, `patientCondition`, `followUpInstruction`, `decidedByDoctorName` |
| GET | `/api/v1/health-services/inpatient-management/admission-transfers/{id}` | Detail satu permintaan | `sourceType` (`Emergency` / `Outpatient`) | 200 |
| POST | `/api/v1/health-services/inpatient-management/admission-transfers/admit` | Satu transaksi admisi transfer | `transferDispositionId`, `sourceType`, `patientId`, `serviceUnitId`, `patientClassId`, `doctorId`, `bedId`, `expectedBedVersion`, `paymentType`, `depositAmount`, `requiresIsolation`, `isolationNote`, `notes` | 201 "Admisi transfer dan reservasi tersimpan. Lanjutkan persetujuan pasien."; 409 `PATIENT_ALREADY_ADMITTED`; 422 `BED_NOT_READY` |

### `[Tags("Health Services / Inpatient Management / Inpatient Episode")]`

| Method | Path | Deskripsi | Request utama | Respons yang teramati |
|---|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/episodes/{id}` | Detail episode | — | 200 |
| PATCH | `/api/v1/health-services/inpatient-management/episodes/{id}/cancel` | Batalkan admisi | alasan pembatalan (wajib) | 200 "Admisi berhasil dibatalkan." — `admissionTransferId` tetap terisi |

### `[Tags("Health Services / Inpatient Management / Bed Occupancy")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| GET | `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Bed tersedia per unit (**tanpa konteks pasien** pada jalur transfer) | 200 |
| GET | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Ringkasan bed | 200 |

### `[Tags("Health Services / Inpatient Management / Inpatient Admission Consent")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| POST | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` | Persetujuan dan tanda tangan digital | 201 "Formulir persetujuan rawat inap dan tanda tangan digital berhasil disimpan." |

### `[Tags("Health Services / Billing Management / Billing / Patient Funds")]`

| Method | Path | Deskripsi | Respons yang teramati |
|---|---|---|---|
| GET | `/api/v1/health-services/billing-management/billing/patient-funds/deposit-policies` | Kebijakan minimum deposit | 200 |

Endpoint pendukung (semua 200): `bed-management/floor-plan`, `patients/admin`, `patients/admin/{id}`, `patient-identity-documents`, `patient-memberships/admin/patients/{id}/active`, `patient-insurances/admin/options`, `patient-company-guarantors/admin/options`, `admissions/surgical-booking-lookup`.

**Ringkasan status jaringan yang terekam untuk menu ini:** 195 panggilan — GET 2xx ×186, POST 2xx ×7, POST 4xx ×2 (409 dan 422 yang diuji), 5xx ×0, error JavaScript ×0.

---

## 7. Data Uji yang Terbentuk di `QuilvianNewDevHamzah`

| Permintaan | Pasien | Episode | Status | Bed | Lainnya |
|---|---|---|---|---|---|
| `EMG-AGY-1026-01` (IGD CITO) | Hendro Kusuma (RM 62) | RI-261026111914-67CACD | *Draft* | VIP 306-A dipesan s.d. ±13.19 WIB | Kunjungan ENC-RSMMC-261026111914-1EB311, DPO-20261026-00002 Rp 10 jt, persetujuan (pasien sendiri), disposisi IGD *Executed* |
| `SPRI/2026/10/AGY02` (Poli) | Dewi Lestari (RM 63) | RI-261026112302-11FF94 | *Draft* | KELAS I 330-B dipesan s.d. ±13.23 WIB | Persetujuan (pasien sendiri), tanpa deposit |
| `EMG-AGY-1026-03` (IGD isolasi) | Reza Firmansyah (RM 41) | RI-261026113134-555BA6 | *Cancelled* | VIP 307-A sudah dilepas | Alasan pembatalan tercatat; disposisi IGD tetap *Executed* (TR-03) |
| `SPRI/2026/10/AGY04` (Poli) | Ny. ANISA LESTARI (RM 60) | — | Masih di antrean | — | Sengaja dibiarkan untuk uji ulang TR-02 |

Dua episode *Draft* akan gugur otomatis bila kedatangan tidak dikonfirmasi di Papan Tempat Tidur sebelum reservasi habis. Agent tidak menghapus data apa pun. Bila perlu dibersihkan, data sumber dapat ditelusuri dari Id berawalan `a1026000-` dan kode di kolom pertama.

---

## 8. Rekomendasi Urutan Perbaikan

1. **TR-01** — isolasi: master bed isolasi, penanda dari IGD, dan denah transfer yang sadar konteks pasien.
2. **TR-02** — pemeriksaan kelayakan di langkah Verifikasi.
3. **TR-03** — keputusan pemilik proses tentang pembatalan, lalu terapkan.
4. **TR-04** — simpan konsulen dan ketergantungan.
5. **TR-05**, **TR-07**.
6. **TR-06** pada rilis berikutnya.

**Uji ulang setelah perbaikan:**
- TR-TC-07: permintaan Ny. Anisa (`SPRI/2026/10/AGY04`) masih tersedia di antrean.
- TR-TC-08 dan TR-TC-10: buat permintaan IGD isolasi baru dengan `admisi-1026-seed-transfer.py` (ubah kode dan pasien), karena permintaan Reza sudah *Executed*.
- TR-TC-09: ulangi alur transfer dengan konsulen dan ketergantungan, lalu periksa `InpDoctorAssignment` dan `InpCareDependencyHistory`.

---

## 9. Lampiran Artefak

| Jenis | Lokasi |
|---|---|
| Screenshot (81 berkas) | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/trf-transfer/` |
| Log jaringan | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1026/trf-transfer/network.log` |
| Sambungan browser Chromium headed | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1026-open-browser.ps1`, `admisi-1026-cdp.mjs` |
| Skrip langkah uji | `admisi-1026-trf-01-antrean.mjs`, `admisi-1026-trf-proses.mjs`, `admisi-1026-trf-rantai.mjs`, `admisi-1026-bed-kamar.mjs`, `admisi-1026-bed-pilih.mjs`, `admisi-1026-pesan.mjs`, `admisi-1026-persetujuan.mjs`, `admisi-1026-click.mjs` di folder `scripts/` yang sama |
| Penyiapan data transfer | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1026-seed-transfer.py` (tanpa `--apply` hanya menampilkan rencana) |
| Kueri DB baca-saja | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1026-db.py` |
