# Laporan Live Browser Testing — Admisi Rawat Inap: Verifikasi Perbaikan PLAN-REPAIR-EPS-041

| Hal | Isi |
|---|---|
| Menu yang diuji | Rawat Inap → Admisi Rawat Inap — jalur **Transfer IGD & Poliklinik**, **Pendaftaran Pasien Lama**, **Admisi Bayi Baru Lahir**; regresi **Manajemen Tempat Tidur** |
| Alamat layar | `http://localhost:3000/health-services/inpatient-management/admissions/{transfer,existing-patient,newborn}` dan `/bed-board` |
| Sub-modul blueprint | `rawat-inap/episode-rawat-inap` |
| Tujuan | Menjalankan verifikasi menyeluruh bagian 8 [`PLAN-REPAIR-EPS-041`](../../docs/plan-repair/plan-repair-041-temuan-uji-admisi-lama-bayi-transfer.md) |
| Tanggal uji | Selasa, 27 Oktober 2026, pukul ±10.25–11.10 WIB |
| Penguji | AI Agent (Claude Code). Seluruh klik, isian, dan pemeriksaan dilakukan agent di browser; tidak ada langkah manual oleh manusia. |
| Browser | Google Chrome (engine Chromium) tampil di layar (*headed*, maksimal), dikendalikan `playwright` lewat CDP port 9334, `slowMo` 450 ms |
| Frontend | `next dev` di `http://localhost:3000` — cabang `HamzahV2`, basis commit `be9c2f129` + perubahan `FIX-EPS-041-FE` (belum di-commit) |
| Backend | `https://localhost:7184/api` — cabang `MHamzah`, commit `ee56c659` (memuat `1872b399`), dijalankan dari hasil `dotnet build` ke folder sementara agar DLL repository tidak terkunci |
| Basis data | `QuilvianNewDevHamzah` (PostgreSQL pengembang), izin pemilik untuk data uji |
| Akun | `superadmin@admin.com` (kata sandi tidak dicatat) |
| Laporan acuan | [Transfer 26-10](laporan-live-testing-admisi-transfer-igd-poliklinik.md), [Pasien Lama 26-10](laporan-live-testing-admisi-pasien-lama.md), [Bayi 26-10](laporan-live-testing-admisi-bayi-baru-lahir-20261026.md) |

---

## 1. Ringkasan Eksekutif

**Kesimpulan: tujuh dari delapan skenario bagian 8 lulus. Pasien isolasi kini hanya ditawari bed isolasi dan berhasil
diadmisi ke VIP 316; pasien yang sedang dirawat tertahan di langkah 1 tanpa satu pun permintaan tulis; kebutuhan IGD,
DPJP usulan, diagnosis, dan kelas Poli terbawa; ketergantungan tersimpan; bayi yang RM-nya sudah ada dilanjutkan tanpa
RM baru; deposit raksasa tertahan. Satu hal belum tuntas: konsulen pada admisi transfer (dan, ternyata, juga jalur
biasa) ditolak server karena aturan "dokter pendukung hanya pada episode yang sedang dirawat" — perlu keputusan
pemilik.**

| Skenario bagian 8 | Hasil | Catatan |
|---|---|---|
| 1. Transfer IGD isolasi baru | `PASS` | Chip Isolasi, sakelar tercentang + keterangan, hanya VIP 316/317 aktif, admisi tersimpan di VIP 316-A |
| 2. Poli `SPRI/2026/10/AGY04` (pasien sedang dirawat) | `PASS` | Diblokir di langkah 1, alert + "Pindah Tempat Tidur", 0 POST |
| 3. Transfer IGD CITO + konsulen + High Dependency | `PASS dengan catatan` | DPJP usulan terisi, High Dependency tersimpan dan tampil; **konsulen ditolak server 422** (lihat temuan VR-01) |
| 4. Batalkan admisi transfer | `PASS` | Dialog menyebut permintaan tidak kembali ke antrean; antrean terbukti tidak memuatnya lagi |
| 5. Ibu dengan bayi tanpa episode | `PASS` | "Lanjutkan admisi bayi ini" sampai pemesanan boks, tanpa RM baru (aktivasi tidak dijalankan) |
| 6. Deposit dua puluh angka 9 (Pasien Lama) | `PASS` | Ditahan di langkah Deposit dengan pesan; "12,5" memunculkan petunjuk |
| 7. Regresi Pasien Lama, Bayi, Papan Tempat Tidur | `PASS` | Pasien Lama selesai sampai "Selesai"; Bayi Perinatologi; Papan termuat normal |
| 8. Jaringan | `PASS` | 511 respons API: 508 sukses, 3 × 422 yang diharapkan, **0 respons 5xx, 0 galat JavaScript** |

---

## 2. Alur Proses Bisnis yang Diuji

1. **Transfer IGD isolasi.** Dokter IGD meminta rawat inap untuk Reza Firmansyah (suspek TB, butuh isolasi). Petugas
   admisi membuka antrean → **Proses Admisi** → layar Verifikasi menampilkan kebutuhan "Isolasi", DPJP usulan, dan dokter
   yang memutuskan → Pembayaran (Tunai) → Deposit → Dokter (DPJP usulan dan sakelar isolasi sudah terisi) → Pilih Bed
   (hanya bed isolasi) → Booking → Konfirmasi.
2. **Pasien yang sedang dirawat.** Surat pengantar Poli untuk Ny. Anisa Lestari yang sedang dirawat di VIP 302 dibuka →
   layar langsung memeriksa kelayakan → petugas diarahkan ke Pindah Tempat Tidur / Detail Episode, bukan membuka admisi
   kedua.
3. **Transfer CITO dengan konsulen.** Sama dengan nomor 1, ditambah dokter pendukung dan High Dependency di langkah
   Dokter; sesudah `admit`, layar menyimpan ketergantungan dan mencoba menyimpan dokter pendukung.
4. **Pembatalan.** Dari Konfirmasi petugas menekan Batalkan Admisi; dialog mengingatkan bahwa permintaan IGD tidak
   kembali ke antrean (kontrak EPS-006/K-02).
5. **Bayi yang RM-nya sudah ada.** Ibu Ny. Maria Veronica dipilih → baris "Ferran · RM 00-00-00-74 — Admisi belum
   selesai" → **Lanjutkan admisi bayi ini** → Perawatan dan DPJP → Penempatan Perinatologi → pemesanan boks.
6. **Pasien Lama dengan deposit.** Reza Pratama dicari → Jenis Kunjungan → Pembayaran Tunai → Deposit (uji nilai ngasal,
   lalu Rp 5.000.000) → Dokter (High Dependency) → Pilih Bed → Booking → Konfirmasi (order deposit tampil) → Form
   Persetujuan (tanda tangan) → Cetak → **Selesai**.

---

## 3. Data Uji

### 3.1 Permintaan transfer yang disiapkan agent

Antrean disiapkan dengan skrip `admisi-1027-seed-transfer.py` (pola seeder 26-10, Id berawalan `a1027000-`, catatan
"DATA UJI AGENT 2026-10-27"); 14 baris ditulis atas izin pemilik DB.

| Kode | Pasien | Jenis | Kebutuhan / isi |
|---|---|---|---|
| `EMG-AGY-1027-11` | Reza Firmansyah (RM 00-00-00-41) | IGD | Isolasi; DPJP usulan dr. Rendy Pangalila; diputuskan dr. Joni Abraham Pratama |
| `EMG-AGY-1027-12` | Siti Rahmawati (RM 00-00-00-61) | IGD | CITO; Oksigen + Monitoring Khusus |
| `EMG-AGY-1027-13` | Hendro Kusuma (RM 00-00-00-62) | IGD | Pasien punya admisi Draft lain |
| `SPRI/2026/10/AGY1027-14` | Reza Pratama (RM 00-00-00-64) | Poli | Diagnosa "Demam Berdarah Dengue", Kelas I |
| `SPRI/2026/10/AGY04` (data 26-10) | Ny. Anisa Lestari (RM 00-00-00-60) | Poli | Pasien sedang dirawat di VIP 302 |

### 3.2 Admisi yang terbentuk lalu dibatalkan

Seluruh admisi uji dibatalkan lewat dialog Batalkan Admisi supaya bed dan pasien kembali bersih: `RI-261027103124-1159F5`
(VIP 316, transfer isolasi), `RI-261027103633-304915` (VIP 307, CITO), `RI-261027104548-81CF05` (Kelas II 329, Hendro),
`RI-261027104812-F99C5E` (boks BAYI-1-C, bayi Ferran), `RI-261027105512-EB0A69` (Kelas II 328, Pasien Lama, persetujuan
tersimpan). Satu `admit` uji negatif ditolak 422 dan tidak meninggalkan baris apa pun.

---

## 4. Rincian Test Case

### P0 — Fungsi Kritis

| ID | Skenario | Hasil yang diharapkan | Hasil sebenarnya | Status | Bukti |
|---|---|---|---|---|---|
| VR-P0-01 | Admisi transfer isolasi sampai pemesanan | Tersimpan di bed isolasi | `POST admit` 201, VIP 316-A dipesan, episode `RI-261027103124-1159F5` | `PASS` | `transfer/s1-i-*` |
| VR-P0-02 | Admisi Pasien Lama lengkap sampai Selesai | Tersimpan, persetujuan tersimpan, keluar ke Daftar Kerja | `POST consent` 201; "Selesai" membuka `/episodes` | `PASS` | `pasien-lama/s9-*` |
| VR-P0-03 | Penolakan `admit` tidak meninggalkan data | Transaksi dibatalkan utuh | 422; tidak ada episode/kunjungan hasil untuk `AGY1027-14` | `PASS` | `transfer/s11-a-*`, kueri DB |
| VR-P0-04 | Tidak ada galat server/JS | 0 × 5xx, 0 pageerror | 511 respons, 0 × 5xx, 0 pageerror | `PASS` | `*/network.log` |

### P1 — Kepatuhan Bisnis dan Keselamatan

| ID | Skenario | Hasil yang diharapkan | Hasil sebenarnya | Status | Bukti |
|---|---|---|---|---|---|
| VR-P1-01 | Denah pasien isolasi (FIX-03) | Hanya bed isolasi aktif | `available-beds?…patientId=…&requiresIsolation=true&includeIneligible=true`; hanya VIP 316/317 aktif; 329/307 ditolak `ISOLATION_REQUIRED` | `PASS` | `transfer/s1-d-pilih-bed` |
| VR-P1-02 | Denah pasien biasa | ODC dan bed isolasi nonaktif | ODC 319–322 dan VIP 316/317 nonaktif, bangsal biasa aktif | `PASS` | `transfer/s3-*` |
| VR-P1-03 | Pasien sedang dirawat lewat transfer (FIX-06) | Diblokir di langkah 1, 0 POST | Badge "Sedang Dirawat", alert server, Lanjut nonaktif dengan alasan, 0 request tulis | `PASS` | `transfer/s2-*` |
| VR-P1-04 | Pasien dengan admisi Draft lain | Peringatan, Lanjut tetap dapat dipakai | "Ada Admisi Disiapkan", tombol "Lanjutkan Admisi RI-…", Lanjut aktif | `PASS` | `transfer/s2d-*` |
| VR-P1-05 | Batal admisi transfer (FIX-07, K-02) | Dialog menyebut permintaan tidak kembali | "Permintaan rawat inap dari IGD (No. Kunjungan EMG-AGY-1027-11) tidak kembali ke antrean…"; antrean `Confirmed` tidak memuatnya | `PASS` | `transfer/s4-*` |
| VR-P1-06 | Ketergantungan transfer (FIX-08) | High/Low tersimpan | `CareDependencyLevel` 1 (CITO) dan 2 (Hendro), satu riwayat | `PASS` | kueri DB |
| VR-P1-07 | Konsulen transfer (FIX-08) | Konsulen tersimpan sebagai penugasan | **422** "Dokter pendukung hanya dapat dilibatkan pada episode yang sedang dirawat…" | `FAIL` (aturan server) | `transfer/s3-*`, temuan VR-01 |
| VR-P1-08 | Bayi lanjutkan admisi (FIX-10) | Tanpa RM baru | 0 POST pasien; RM 74 kini punya kunjungan + episode; ibu tetap 1 bayi | `PASS` | `bayi/s6-*`, kueri DB |
| VR-P1-09 | Kode galat `admit` (FIX-04) | `ISOLATION_REQUIRED`, bukan `BED_NOT_READY` | `errors.code: "ISOLATION_REQUIRED"` | `PASS` | `transfer/s11-a-*` |

### P2 & P3 — Fungsi dan Validasi (termasuk data ngasal)

| ID | Skenario | Hasil yang diharapkan | Hasil sebenarnya | Status | Bukti |
|---|---|---|---|---|---|
| VR-P2-01 | Isian awal Dokter transfer (FIX-02) | DPJP usulan terpilih; isolasi + keterangan | dr. Rendy terpilih; sakelar on; "Permintaan IGD menyebut kebutuhan isolasi" | `PASS` | `transfer/s1-c-dokter` |
| VR-P2-02 | Gerbang kembar tanpa centang (FIX-10) | Pesan menyebut kedua pilihan | "Pilih \"Lanjutkan admisi bayi ini\" … atau centang \"Ini bayi yang berbeda\"…" | `PASS` | `bayi/s6-b-*` |
| VR-P2-03 | Picker tanggal lahir (FIX-12) | Hanya rentang sah aktif | 22–27 Okt aktif, sisanya nonaktif (kasus diminta 23-10, hari ini 27-10) | `PASS` | `bayi/s7-b-*` |
| VR-P2-04 | Deposit "12,5" (FIX-15) | Petunjuk tanpa koma | "Ketik tanpa koma, contoh 12500000 untuk Rp 12,5 juta." | `PASS` | `pasien-lama/s9-b-*` |
| VR-P2-05 | Deposit dua puluh angka 9 (FIX-15) | Ditahan dengan pesan | Tampil 9.999.999.999.999.999; "Nominal melebihi batas yang didukung…"; tetap Langkah 4 Deposit; fokus ke isian | `PASS` | `pasien-lama/s9-c-*` |
| VR-P2-06 | Galat simpan transfer (FIX-09) | Judul "Admisi Belum Tersimpan" | Judul itu tampil di atas Booking; "Gagal Memuat Permintaan" tidak muncul | `PASS` | `transfer/s11-a-*` |
| VR-P2-07 | Konsulen ditolak — pesan layar | Tidak menawarkan aksi yang pasti gagal | Kalimat server + arahan ke Detail Episode, tanpa "Simpan Ulang" | `PASS` | `transfer/s5-j-*` |

### P4 & P5 — Tampilan dan Kenyamanan

| ID | Skenario | Hasil yang diharapkan | Hasil sebenarnya | Status | Bukti |
|---|---|---|---|---|---|
| VR-P4-01 | Kartu rujukan transfer | DPJP usulan, Diputuskan oleh, Kebutuhan, Alasan Rawat Inap | Sesuai skema 3.1; Poli menampilkan Kelas Diminta dan Diagnosa | `PASS` | `transfer/s1-a`, `s2c-a` |
| VR-P4-02 | Catatan Disposisi | Tidak tampil "." | TTV "…SpO2 95%." → baris tidak tampil; CITO → "ronkhi basah basal" | `PASS` | `transfer/s1-a`, `s3-a` |
| VR-P4-03 | Status bed di Konfirmasi | "Dipesan" | "Status Tempat Tidur: Dipesan" | `PASS` | `transfer/s1-k-*` |
| VR-P4-04 | Konfirmasi Pasien Lama (FIX-18) | Order deposit + ketergantungan | "DPO-20261027-00001 · Rp 5.000.000 · Belum Dibayar"; "Tingkat Ketergantungan: High Dependency" | `PASS` | `pasien-lama/s9-k-*` |
| VR-P4-05 | Ringkasan denah saat memuat (FIX-17) | "Memuat denah…" | Muncul pada 314 ms; "Belum ada kamar…" tidak pernah muncul | `PASS` | skrip `admisi-1027-pantau-denah.mjs` |
| VR-P4-06 | "Dipesan Pada" sesudah F5 (FIX-17) | Tetap terbaca | "27 Okt 2026, 10.31" sebelum dan sesudah F5 | `PASS` | `transfer/s1-j-*` |
| VR-P4-07 | Tombol Selesai (FIX-17) | Tanpa dialog "Tinggalkan Admisi?" | Langsung ke Daftar Kerja Episode | `PASS` | `pasien-lama/s9-o-*` |
| VR-P4-08 | Waktu lahir dan label kelas (FIX-12) | "25 Okt 2026, 13.51 WIB"; tanpa "Kelas KELAS" | Sesuai; rawat gabung "KELAS I 428 · KELAS I" | `PASS` | `bayi/s6-m-*`, `bayi/s8-e-*` |
| VR-P4-09 | Lantai denah neonatal (FIX-12) | Lantai ruang bayi | Lantai terakhir diset 3 → denah Perinatologi membuka Ruang Bayi/NICU; lantai terakhir tetap 3 | `PASS` | `bayi/s6-e-denah-bayi` |
| VR-P5-01 | Nama pasien di dialog batal | Nama tampil | Awalnya "Pasien"; diperbaiki pada sesi ini, kini "Siti Rahmawati" | `PASS` sesudah perbaikan | `transfer/s3-o-*` |

---

## 5. Temuan Isu

### VR-01 — Dokter pendukung tidak dapat ditetapkan saat admisi (semua jalur) · P1 · Tinggi (perlu keputusan)

**Apa yang terjadi.** Pada transfer Siti Rahmawati dan Hendro Kusuma, `POST /episodes/{id}/doctor-assignments/supporting`
dijawab 422: "Dokter pendukung hanya dapat dilibatkan pada episode yang sedang dirawat atau sedang menunggu pulang."
Episode hasil admisi masih Draft sampai kedatangan pasien dikonfirmasi, jadi konsulen yang diisi di langkah Dokter tidak
pernah tersimpan. Aturan yang sama berlaku untuk jalur Pasien Baru/Lama — di sana kegagalan ini selama ini hanya dicatat
di konsol peramban.

**Sebab.** Aturan sengaja di `InpEpisodeService.Assignments.cs` (komentar: "Episode Draft belum punya pasien di
ruangan"). Rencana `FIX-EPS-041-08` (opsi A) mengandaikan endpoint itu menerima episode Draft.

**Yang sudah dilakukan.** Layar kini memberi tahu petugas dengan kalimat server dan arahan "Tetapkan dari Detail Episode
setelah kedatangan pasien dikonfirmasi di Papan Tempat Tidur", tanpa tombol Simpan Ulang yang pasti gagal.

**Keputusan yang dibutuhkan (usulan K-07).** (a) Izinkan dokter pendukung pada episode Draft (perubahan aturan backend);
(b) simpan konsulen sebagai rencana dan tetapkan otomatis saat kedatangan dikonfirmasi; atau (c) pertahankan aturan, dan
langkah Dokter tidak lagi menawarkan isian konsulen saat admisi. Rekomendasi: (b) — klinisnya konsulen memang baru bekerja
setelah pasien tiba, tetapi rencana dari IGD tidak hilang.

### VR-02 — Dokumen serah terima transfer gagal dibuat di DB pengembang · P3 · Rendah (lingkungan)

Setiap `admit` transfer mencatat `ERR` di log backend: insert `CliTransferHandover` gagal karena kolom `FromPlacementId`
masih `NOT NULL`. Model sudah nullable dan migration `20261022130000_MakeTransferHandoverPlacementsNullable` sudah ada,
tetapi **belum diterapkan** di `QuilvianNewDevHamzah`. Kegagalan ditangkap sehingga admisi tetap 201. Tindak lanjut:
pemilik DB menerapkan migration tersebut (agent tidak menjalankan migration).

**Pembaruan 27-10-2026.** Sebabnya ternyata migration itu ditulis tangan tanpa berkas `.Designer.cs`, sehingga EF tidak
pernah mengenalinya. Atas permintaan pemilik, Designer-nya ditambahkan dan migration diterapkan di DB Hamzah; kini
`FromPlacementId` dan `ToPlacementId` boleh kosong. Penyimpanan dokumen serah terima pada `admit` transfer belum diuji
ulang. Rincian: [laporan `FIX-EPS-041-BE`](../../task/report/backend/FIX-EPS-041-BE.md) bagian 8.

### VR-03 — Tab peramban sempat tidak menjawab sesudah Fast Refresh · P5 · Catatan lingkungan

Sesudah agent menyunting hook di tengah sesi, tab yang sedang terbuka berhenti menjawab CDP. Tab baru dengan URL dan
aksi yang sama berjalan normal dan masalahnya tidak terulang; dicatat sebagai efek `next dev`, bukan perilaku aplikasi.

### Hal yang belum terbukti di layar

| Butir | Sebab |
|---|---|
| `FIX-EPS-041-09` butir 6 (buka ulang URL transfer yang persetujuannya sudah tersimpan) | Tidak ada transfer yang diselesaikan sampai persetujuan pada sesi ini; dugaan issue belum dibuktikan |
| `FIX-EPS-041-10` butir 4 (ibu tanpa bayi) | Semua kandidat ibu di DB sudah punya bayi (`NOT FEASIBLE`); logika tidak berubah |
| `FIX-EPS-041-03` butir 1 — teks alasan isolasi di layar | Alasan server ada pada data bed, tetapi kamar yang seluruh bed-nya tidak layak tidak dapat dibuka, jadi kalimatnya tidak terlihat (sama dengan jalur bayi) |
| Aktivasi bayi sampai Dokumen Bayi | Pemesanan boks sudah membuktikan RM tidak ganda; aktivasi dibatalkan agar data uji bersih |

---

## 6. Endpoint yang Terlibat (Gaya Swagger)

### `[Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]`

| Method | Path | Kegunaan dalam uji | Hasil |
|---|---|---|---|
| `GET` | `/api/v1/health-services/inpatient-management/admission-transfers` | Antrean; cek permintaan batal tidak kembali | 200 |
| `GET` | `/api/v1/health-services/inpatient-management/admission-transfers/{id}?sourceType=` | Sumber dengan kolom baru FIX-01 | 200 |
| `POST` | `/api/v1/health-services/inpatient-management/admission-transfers/admit` | Admisi atomik | 201 (`statusCode: 201`); 422 `ISOLATION_REQUIRED` pada uji negatif |

### `[Tags("Health Services / Inpatient Management / Inpatient Episode")]`

| Method | Path | Kegunaan dalam uji | Hasil |
|---|---|---|---|
| `GET` | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility?patientId=` | Kelayakan langkah 1 | 200 |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/newborn-mother-candidates` | Kandidat ibu + `pendingBabyCount` | 200 |
| `PATCH` | `/api/v1/health-services/inpatient-management/episodes/{id}/care-dependency` | Ketergantungan sesudah `admit` | 200 |
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/supporting` | Konsulen | 422 (VR-01) |
| `PATCH` | `/api/v1/health-services/inpatient-management/episodes/{id}/cancel` | Pembatalan data uji | 200 |

### `[Tags("Health Services / Inpatient Management / Inpatient Admission Consent")]`

| Method | Path | Kegunaan dalam uji | Hasil |
|---|---|---|---|
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{id}/consent` | Persetujuan Pasien Lama | 201 |

### `[Tags("Health Services / Inpatient Management / Bed Occupancy")]`

| Method | Path | Kegunaan dalam uji | Hasil |
|---|---|---|---|
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Kelayakan bed dengan `patientId` + `requiresIsolation` | 200 |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Pemulihan pemesanan + `reservationReservedAt` | 200 |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations` | Pemesanan jalur biasa dan bayi | 200 |

---

## 7. Lampiran Artefak

| Artefak | Lokasi |
|---|---|
| Screenshot (113) dan `network.log` per topik | `QuilvianSystemFrontendDev/test-with-agy/screenshots/admisi-1027-041/{00-login,transfer,pasien-lama,bayi,papan,api}/` |
| Skrip langkah uji | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1027-*.mjs` |
| Seeder data uji | `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-1027-seed-transfer.py` |
| Laporan task | [`FIX-EPS-041-FE`](../../task/report/frontend/FIX-EPS-041-FE.md), [`FIX-EPS-041-BE`](../../task/report/backend/FIX-EPS-041-BE.md) |
