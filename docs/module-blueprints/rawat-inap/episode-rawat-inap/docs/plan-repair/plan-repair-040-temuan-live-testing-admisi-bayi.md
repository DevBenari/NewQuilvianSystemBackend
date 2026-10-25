# PLAN-REPAIR-040 — Gelang bayi bernama benar, bed Perinatologi hanya di ruang bayi, waktu lahir tervalidasi zona WIB, dan persetujuan wali terisi otomatis

```yaml
plan_id: PLAN-REPAIR-EPS-040
issue: ../issue/issue-040-temuan-live-testing-admisi-bayi.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-25"
diputuskan_oleh: "Pemilik (chat 25-10-2026: seluruh rencana + rekomendasi K-01–K-06)"
tanggal_keputusan: "2026-10-25"
basis_source_backend: "985a1fc75c2bb69163251bb7744bead949ae7285 (MHamzah)"
basis_source_frontend: "9cad353ba926ca3750ca03791b2ff4d5b6fddf5a (HamzahV2)"
```

Rencana ini memuat 23 perbaikan:
- **9 backend**, 8 frontend (satu di antaranya juga merevisi dokumen), dan 6 lintas keduanya.
- Lima perbaikan semula menunggu keputusan pemilik (K-01, K-02, K-03, K-05, K-06); **seluruh keputusan diambil 25-10-2026** memakai rekomendasi.
- Seluruh 23 perbaikan sudah dikerjakan di source pada 25-10-2026 (`FIX-EPS-040-BE`, `FIX-EPS-040-FE`); build dan uji layar belum.

`ISS-EPS-040-20` (CSP foto profil) tidak masuk rencana ini karena milik konfigurasi platform frontend.

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-040-01` | `ISS-EPS-040-01` | Bersihkan sapaan ganda "NY. NY." pada gelang bayi | BE | 1 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 4/4 AC ada di source; 0/4 terbukti (build pemilik, uji layar belum) | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |
| `FIX-EPS-040-02` | `ISS-EPS-040-01` | Gelang memakai nama resmi bayi ditambah baris nama ibu | BE + FE + dokumen | 1 | `FIX-EPS-040-BE`, `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 4/4 AC ada di source; 0/4 terbukti; revisi BAYI-S09 (bagian 7) belum | [BE](../../task/report/backend/FIX-EPS-040-BE.md), [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-03` | `ISS-EPS-040-02` | Mode Neonatal hanya menerima bed di kamar khusus bayi; boks kamar dewasa hanya untuk rawat gabung ibunya sendiri | BE | 1 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 5/5 AC ada di source; 0/5 terbukti; rilis menunggu P-01 | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |
| `FIX-EPS-040-04` | `ISS-EPS-040-02` | Bayi non-intensif tidak ditawari bed NICU | BE | 2 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 2/2 AC ada di source (AC3 tidak berlaku, K-02 = tidak); 0/2 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |
| `FIX-EPS-040-05` | `ISS-EPS-040-03` | Pemeriksaan "masa depan" pada pendaftaran bayi memakai waktu rumah sakit (WIB) | BE | 1 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |
| `FIX-EPS-040-06` | `ISS-EPS-040-04` | Tolak waktu lahir (tanggal + jam) yang belum terjadi di layar dan server | FE + BE | 1 | `FIX-EPS-040-BE`, `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md), [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-07` | `ISS-EPS-040-04` | Batas bawah tanggal lahir terhadap kasus persalinan | FE + BE | 2 | `FIX-EPS-040-BE`, `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md), [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-08` | `ISS-EPS-040-05` | Wajibkan dan validasi NIK, No. HP, dan hubungan wali sesuai skema 3.12 | FE + BE | 2 | `FIX-EPS-040-BE`, `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 5/5 AC ada di source; 0/5 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md), [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-09` | `ISS-EPS-040-06` | Terapkan aturan DPJP bayi | FE + BE + dokumen | 2 | `FIX-EPS-040-BE`, `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 4/4 AC ada di source; 0/4 terbukti; revisi kontrak 022 (bagian 7) belum | [BE](../../task/report/backend/FIX-EPS-040-BE.md), [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-10` | `ISS-EPS-040-07` | Terapkan keadaan awal klausul persetujuan | FE + dokumen | 2 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti; revisi skema 3.12 (bagian 7) belum | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-11` | `ISS-EPS-040-08` | Arah buka picker tanggal mengikuti tinggi popover sebenarnya | FE (base component) | 2 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-12` | `ISS-EPS-040-09` | Tambahkan bed dan kelas penempatan ibu pada respons kandidat | BE | 3 | `FIX-EPS-040-BE`, `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md), [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-13` | `ISS-EPS-040-10` | Label status kasus persalinan dalam Bahasa Indonesia | BE | 3 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |
| `FIX-EPS-040-14` | `ISS-EPS-040-11` | Pesan Kartu Pasien mengikuti status episode | FE | 2 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-15` | `ISS-EPS-040-12` | Ringkasan perawatan memakai tiga label kartu | FE | 3 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-16` | `ISS-EPS-040-13` | Alasan `BED_NOT_READY` tidak ditambahkan bila alasan spesifik berlaku | BE | 3 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |
| `FIX-EPS-040-17` | `ISS-EPS-040-14` | Keterangan "Menyiapkan admisi bayi…" selama episode disiapkan | FE | 3 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-18` | `ISS-EPS-040-15` | Nama bed di tinjauan sama dengan langkah 4 | FE | 3 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-19` | `ISS-EPS-040-16` | Lewati lookup booking operasi untuk pasien bayi | FE | 3 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-20` | `ISS-EPS-040-17`, `ISS-EPS-040-T1` | Isi otomatis wali dari identitas ibu; tidak memanggil print-data selama Draft | FE | 2 | `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 3/3 AC ada di source; 0/3 terbukti | [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-21` | `ISS-EPS-040-18`, `ISS-EPS-040-19` | Sanitasi karakter nama bayi dan NIK bayi | FE + BE | 3 | `FIX-EPS-040-BE`, `FIX-EPS-040-FE` | 🟡 SEBAGIAN — 4/4 AC ada di source; 0/4 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md), [FE](../../task/report/frontend/FIX-EPS-040-FE.md) |
| `FIX-EPS-040-22` | `ISS-EPS-040-T2` | Nomor episode dan encounter dibentuk dari waktu rumah sakit | BE | 3 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 2/2 AC ada di source; 0/2 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |
| `FIX-EPS-040-23` | `ISS-EPS-040-T3` | Tag Swagger persetujuan mengikuti pola modul | BE | 3 | `FIX-EPS-040-BE` | 🟡 SEBAGIAN — 1/1 AC ada di source; 0/1 terbukti | [BE](../../task/report/backend/FIX-EPS-040-BE.md) |

**Ringkasan (25-10-2026): 0 dari 23 perbaikan selesai; 23 sebagian — source seluruhnya selesai, sedangkan `dotnet build` (dijalankan pemilik), lint/test/build frontend, dan uji layar V-01–V-10 belum. Kode BE-RWI-183/184 yang membuat HEAD gagal build ikut dinetralkan atas persetujuan pemilik (lihat laporan BE).**

Catatan data di luar register:
- Kode bed BAYI-1/BAYI-2 yang berbentuk umum (`BD-RSMMC-00001`) dikoreksi lewat pemilik master data
  (`ISS-EPS-040-15`, jenis `DATA`).
- Bila hasil P-01 menunjukkan kamar R. Bayi atau Ruang NICU belum bertanda `IsForNewborn`, penandanya
  dikoreksi lewat proses master data sebelum `FIX-EPS-040-03` dirilis.

---

## 2. Solusi terpilih per temuan

### ISS-EPS-040-01 — Gelang bayi "BY. NY. NY." dan mengabaikan nama resmi bayi

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Pakai `NormalizeMotherName` pada aturan gelang saja | Perubahan terkecil; awalan ganda hilang | Nama khusus bayi tetap diabaikan; gelang dan label tetap bisa berbeda |
| B | Gelang memakai penyusun nama yang sama dengan label (`patient.FullName` + "/ BAYI n"), ditambah baris nama ibu | Gelang = label = kartu; nama ibu tetap ada sesuai MTG-BAYI-04 | Butuh keputusan format (K-01) dan penyesuaian tampilan gelang |
| C | Gelang hanya nama resmi bayi | Sederhana | Melanggar MTG-BAYI-04 (nama ibu wajib di gelang) |

**Solusi terpilih: Opsi A sekarang sebagai `FIX-01`, lalu Opsi B sebagai `FIX-02` setelah K-01.**

1. Awalan ganda adalah kesalahan murni yang tidak butuh keputusan. Pembersihnya sudah ada di
   `NewbornPatientRegistrationService.cs:407-416`.
2. Opsi B memenuhi skema BAYI-S09 (`plan-repair-016`, baris 654) "nama bayi awal/resmi dan nama ibu",
   dan memakai ulang penyusun nama label (`InpAdmissionWorkspaceQueryService.cs:386-387`).

**Kenapa bukan opsi lain.** Opsi C membuang nama ibu yang diwajibkan rapat MTG-BAYI-04.

### ISS-EPS-040-02 — Bed Perinatologi menawarkan boks kamar ibu lain dan bed NICU

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Di `ValidateBed`, mode Neonatal mensyaratkan `room.IsForNewborn = true` | Satu aturan server; berlaku untuk daftar bed, denah, reservasi, dan penempatan sekaligus | Bergantung pada penanda kamar yang benar (P-01) |
| B | Saring daftar di frontend | Cepat | Melanggar prinsip "kelayakan ditentukan server"; reservasi lewat API tetap lolos |
| C | Larang boks yang `BedNumber = 'BOKS'` untuk mode Neonatal | Tanpa bergantung penanda kamar | Bergantung pada konvensi penamaan, rapuh |

**Solusi terpilih: Opsi A (`FIX-03`), ditambah aturan NICU (`FIX-04`) setelah K-02.**

1. Layar sudah menyatakan "Kelayakan penempatan ditentukan server" dan tidak menyaring sendiri,
   sehingga perbaikan harus di server.
2. Penanda `MstRoom.IsForNewborn` sudah ada (`MstRoom.cs:39`), jadi tidak perlu kolom baru.

**Kenapa bukan opsi lain.** B meninggalkan celah di API. C bergantung pada teks kode bed.

### ISS-EPS-040-03 — Tanggal lahir dibandingkan dengan tanggal UTC

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Bandingkan dengan "hari ini" zona rumah sakit memakai pola `InpAdmissionText.LocalToday` atau `FinanceBusinessDate` | Pola yang sudah ada; zona waktu dari profil rumah sakit | Berkas di area Manajemen Pasien memakai helper area lain |
| B | Tambah toleransi +1 hari | Paling cepat | Tetap salah konsep; menerima tanggal besok pada sore hari |

**Solusi terpilih: Opsi A.** Zona waktu diambil dari profil rumah sakit, bukan konstanta. Pemilik modul
`patient-management` diberi tahu karena berkasnya berada di area itu.

**Kenapa bukan opsi lain.** B membuka celah sebaliknya.

### ISS-EPS-040-04 — Waktu lahir yang belum terjadi dan tanggal absurd diterima

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Validasi gabungan tanggal + jam terhadap waktu sekarang (WIB) di layar dan server; batas bawah sesuai K-03 | Layar mencerminkan server (prinsip 4) | Perlu helper waktu lokal di layar |
| B | Tambah `maxDate` pada picker | Tanggal masa depan tidak bisa diklik | Picker dipakai ±222 berkas dan belum punya fitur batas; jam yang belum terjadi tetap lolos |

**Solusi terpilih: Opsi A.** Penambahan batas pada picker boleh menyusul sebagai peningkatan base
component, tetapi tidak menjadi syarat.

**Kenapa bukan opsi lain.** B tidak menutup kasus jam 23.50 dan radiusnya besar.

### ISS-EPS-040-05 — Identitas wali tidak divalidasi sesuai skema

Hanya satu cara yang wajar: tegakkan skema 3.12 yang sudah disetujui.
- NIK, No. HP, dan hubungan wajib diisi.
- NIK 16 digit, atau paspor 6–9 huruf/angka sesuai rekomendasi K-04.
- Server memakai aturan yang sama.
- Tombol simpan tetap mengikuti skema (aktif bila lengkap), ditambah daftar "yang belum lengkap" di
  dekat tombol supaya sebab tombol mati terbaca (prinsip 3).

### ISS-EPS-040-06 — DPJP bayi tidak dibatasi dokter anak

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Hanya dokter Anak yang tampil | Tegas | Bayi tanpa Sp.A jaga malam tidak bisa diadmisi |
| B | Dokter Anak tampil teratas dan diberi penanda; memilih spesialisasi lain wajib alasan | Aman sekaligus luwes; ada jejak keputusan | Perlu kolom alasan |
| C | Tetap seperti sekarang | Tanpa perubahan | Risiko DPJP salah tidak terlihat |

**Solusi terpilih: Opsi B** (rekomendasi K-05). Menunggu keputusan karena ini kebijakan klinis.

### ISS-EPS-040-07 — Klausul persetujuan tercentang sejak awal

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Klausul kosong saat dibuka, dicentang satu per satu | Persetujuan paling eksplisit | 12 klik, memperlambat admisi |
| B | Klausul kosong + satu tombol "Saya telah membaca dan menyetujui seluruh klausul" | Ada tindakan aktif; tetap cepat | Satu centang untuk 12 butir |
| C | Tetap tercentang | Tercepat | Tidak ada bukti tindakan aktif |

**Solusi terpilih: Opsi B** (rekomendasi K-06). Menunggu keputusan pemilik bersama Komite Etik/Hukum.
Skema 3.12 ikut direvisi (bagian 7).

### ISS-EPS-040-08 — Picker tanggal membuka ke bawah lalu terpotong

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Arah ditentukan dari tinggi popover yang diukur (atau `POPOVER_ESTIMATED_HEIGHT`), bukan ambang 240 px | Sama dengan perilaku `FilterTimePicker` | Menyentuh base component lintas modul |
| B | Batasi tinggi popover lalu beri gulir dalam | Tidak mengubah arah | Kalender bergulir terasa janggal; tombol "Hari ini" tetap bisa tertutup |

**Solusi terpilih: Opsi A.**

### ISS-EPS-040-09 sampai ISS-EPS-040-19, T1–T3

Masing-masing hanya punya satu cara yang wajar. Cara tersebut dituliskan langsung pada rincian
perbaikan di bagian 4. Alasan umumnya: perbaikan berupa koreksi teks, field, atau syarat yang sudah
jelas dari skema dan kontrak, dengan radius kecil dan tanpa keputusan bisnis.

Pengecualian `ISS-EPS-040-17` / T1:
- **Opsi A (terpilih):** layar mengambil identitas ibu dari `newborn.mother` yang sudah dimuat, dan
  tidak memanggil print-data selama Draft.
- **Opsi B:** membuka guard backend untuk print-data saat Draft. B ditolak karena guard
  `INP-ADM-DOC-002` sengaja melindungi dokumen admisi yang belum dikunci (`InpAdmissionWriteGuard.cs:51`).

---

## 3. Skema tampilan sebelum → sesudah

Tanda rangka mengikuti `05-skema-tampilan.md` bagian 1. Data contoh pseudonim.

### 3.1 Langkah 7 Dokumen Bayi — Gelang (FIX-01, FIX-02)

```text
SEBELUM
┌── Gelang Bayi ─────────────────────────────────────────┐
│ BY. NY. NY. SITI AMINAH / BAYI 1                         │
│ 25 Okt 2026 (0 hr)              No. RM: 00-00-00-90      │
└──────────────────────────────────────────────────────────┘
Label Pasien: BAGAS PRATAMA / BAYI 1 / SITE-MMC-001      ← berbeda

SESUDAH FIX-01 (tanpa keputusan)
┌── Gelang Bayi ─────────────────────────────────────────┐
│ BY. NY. SITI AMINAH / BAYI 1                             │
│ 25 Okt 2026 (0 hr)              No. RM: 00-00-00-90      │
└──────────────────────────────────────────────────────────┘

SESUDAH FIX-02 (rekomendasi K-01)
┌── Gelang Bayi ─────────────────────────────────────────┐
│ BAGAS PRATAMA / BAYI 1          ← sama dengan label      │
│ IBU: SITI AMINAH                                         │
│ 25 Okt 2026 (0 hr)              No. RM: 00-00-00-90      │
└──────────────────────────────────────────────────────────┘
(bayi tanpa nama khusus → baris 1 "BY. NY. SITI AMINAH / BAYI 1")
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Baris nama | Nama resmi pasien kapital + "/ BAYI n" | `IdentityWristbandResponse.DisplayName` | `IdentityLabelsSection` (tetap) |
| Baris ibu (FIX-02) | "IBU: " + nama ibu tanpa sapaan | field baru, misalnya `MotherLine` | `IdentityLabelsSection` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Cetak (Gelang) | utama | Tidak berubah | Tidak berubah |
| Muat Ulang | kedua | Tidak berubah | Tidak berubah |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Tidak berubah |
| Ibu tidak tercatat | Baris 1 nama resmi bayi saja; baris ibu tidak tampil |
| Gagal / tidak berhak | Tidak berubah |

### 3.2 Langkah 4 Perinatologi — denah dan dialog kamar (FIX-03, FIX-04, FIX-16)

```text
SEBELUM (mode Perinatologi)
Denah dibuka di: Lantai 3 (kamar dewasa)
┌── VIP 305 ─────────────────────────────────────────────┐
│ 305-A     Terisi · [Ibu C]   BELUM DAPAT DIPILIH          │
│ 305-BOKS  Tersedia           [ Pilih bed ini ]   ← salah  │
└──────────────────────────────────────────────────────────┘
┌── Ruang NICU ──────────────────────────────────────────┐
│ NICU-1-A  Terisi · Bed belum siap atau masih dipegang…    │
│           Tempat tidur ini sedang ditempati pasien lain.  │
│ NICU-1-B  Tersedia           [ Pilih bed ini ]   ← K-02   │
└──────────────────────────────────────────────────────────┘

SESUDAH
Denah dibuka di: Lantai 2 (ruang bayi)
┌── VIP 305 ─────────────────────────────────────────────┐
│ 305-A     Terisi · [Ibu C]   (×) BELUM DAPAT DIPILIH      │
│ 305-BOKS  Tersedia           (×) Boks rawat gabung hanya  │
│                              untuk bayi dari ibu di kamar │
│                              ini. Pilih kamar ruang bayi. │
└──────────────────────────────────────────────────────────┘
┌── Ruang NICU ──────────────────────────────────────────┐
│ NICU-1-A  Terisi · Tempat tidur ini sedang ditempati      │
│           pasien lain.                    ← satu alasan    │
│ NICU-1-B  Tersedia  (×) Bed intensif hanya untuk bayi     │
│           yang membutuhkan NICU.          ← bila K-02 = tidak │
└──────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Denah | Lantai dengan bed layak pertama | `available-beds` setelah aturan baru | denah existing (tanpa perubahan kode bila DUGAAN lantai default terbukti) |
| Alasan per bed | Satu alasan paling spesifik | `Eligibility` dari server | dialog kamar existing |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Pilih bed ini | utama | Hanya bed yang `Eligibility.IsEligible = true` | Tidak berubah |

| Keadaan | Tampilan |
| --- | --- |
| Tidak ada bed ruang bayi kosong | "Tidak ada bed ruang bayi yang tersedia. Hubungi Ruang Bayi atau pilih NICU bila bayi membutuhkan perawatan intensif." |
| Gagal memuat | Tidak berubah |

### 3.3 Langkah 2 Identitas Bayi (FIX-06, FIX-07, FIX-11, FIX-21)

```text
SEBELUM
Tanggal lahir* [ 25 Okt 2026 ▾ ]   Jam lahir (WIB)* [ 23:50 ▾ ]   → terkirim ke server
Tanggal lahir* [ 01 Jan 1990 ▾ ]                                  → terkirim ke server
NIK bayi       [ 32abcd5678901234 ]                               → baru ditolak saat Simpan
Picker tanggal terbuka ke bawah, baris 25–31 dan [ Hari ini ] terpotong

SESUDAH
Tanggal lahir* [ 25 Okt 2026 ▾ ]   Jam lahir (WIB)* [ 23:50 ▾ ]
⚠ Waktu lahir belum terjadi. Sekarang 25 Okt 2026 pukul 11.15 WIB.
Tanggal lahir* [ 01 Jan 1990 ▾ ]
⚠ Tanggal lahir lebih awal dari kasus persalinan (diminta 23 Okt 2026).   ← FIX-07, rekomendasi K-03
NIK bayi       [ 3256789012 ]        ← huruf tidak dapat diketik
Nama Bayi      [ Bagas Pratama ]     ← hanya huruf, spasi, titik, koma, apostrof, tanda hubung
Picker tanggal terbuka ke atas bila ruang bawah kurang; [ Hari ini ] selalu terlihat
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Tanggal + jam lahir | Isian yang ada, ditambah pesan validasi gabungan | `validateNewbornIdentity` | `BaseDateField`, `BaseTimeField` |
| NIK bayi | Hanya angka, maks 16 | isian | `BaseInputField` dengan penyaring |
| Nama bayi | Karakter terbatas, maks 100 | isian | `BaseInputField` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Simpan Identitas & Lanjut | utama | Selalu (tidak berubah) | Validasi layar dulu; bila lolos, kirim ke server |

| Keadaan | Tampilan |
| --- | --- |
| Server menolak (misalnya jam berbeda karena jam komputer petugas salah) | Pesan server tampil di atas tombol, isian tidak hilang |

### 3.4 Langkah 5 Persetujuan Wali (FIX-08, FIX-10, FIX-20)

```text
SEBELUM
Hubungan*  (•) Orang Tua  ( ) Wali yang Sah
Nama Lengkap Penanda Tangan* [                      ]   ← kosong
Nomor Identitas (NIK / Paspor) [ ABCDE<script>999… ]    ← diterima
No. Handphone [            ]  Hubungan dengan Pasien [ ]  ← tidak wajib
12 Dari 12 Disetujui  [✓]1 [✓]2 … [✓]12                 ← tercentang sejak awal
[ Simpan & Lanjut ke Cetak ] (mati, tanpa sebab)

SESUDAH
Hubungan*  (•) Orang Tua  ( ) Wali yang Sah
Nama Lengkap Penanda Tangan* [ Siti Aminah          ]   ← dari identitas ibu
NIK / Paspor*                [ 3174015505950001     ]   ← dari identitas ibu bila ada
⚠ NIK harus 16 angka, atau paspor 6–9 huruf/angka.
No. Handphone*               [ 081298765432         ]
Hubungan dengan Pasien*      [ Ibu Kandung        ▾ ]
0 Dari 12 Disetujui   [ ]1 [ ]2 … [ ]12                  ← rekomendasi K-06
[ ] Saya telah membaca dan menyetujui seluruh klausul
Belum lengkap: NIK, tanda tangan.                        ← sebab tombol mati
[ Simpan & Lanjut ke Cetak ]
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Identitas wali | Nama, NIK, HP, alamat ibu; hubungan "Ibu Kandung" | `newborn.mother` (`fetchInpatientAdmissionPatientDetail`) | form existing |
| Klausul | 12 butir; keadaan awal sesuai K-06 | konstanta klausul | checklist existing |
| Daftar belum lengkap | Isian wajib yang belum terpenuhi | `canSubmit` yang dirinci | teks bantu |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Simpan & Lanjut ke Cetak | utama | Sesuai skema 3.12: lengkap dan tanda tangan terkunci | Tidak berubah |
| Setujui seluruh klausul (K-06) | centang | Selalu | Mencentang ke-12 klausul |

| Keadaan | Tampilan |
| --- | --- |
| Identitas ibu belum termuat | Kolom kosong, bisa diisi manual, ditambah keterangan "Identitas ibu belum terbaca" |
| Wali bukan ibu (pilih "Wali yang Sah") | Kolom dikosongkan untuk diisi manual |

### 3.5 Langkah 4 Rawat Gabung — kartu konfirmasi (FIX-12)

```text
SEBELUM
Tempat Tidur Pasien Ibu   Belum terbaca
SESUDAH
Tempat Tidur Pasien Ibu   303-A (Sedang Ditempati Ibu)
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Tempat tidur ibu | Nama bed penempatan aktif ibu | `NewbornMotherCandidateResponse.CurrentBedName` (baru) | `BaseDetailCard` existing |

Tombol tidak berubah. Keadaan "Belum terbaca" hanya tampil bila ibu memang tidak punya penempatan aktif.

### 3.6 Langkah 1, 4, 6, 8 — teks (FIX-13, FIX-14, FIX-15, FIX-17, FIX-18)

```text
SEBELUM                                           SESUDAH
Kasus  OPR-OBS-…-068 · InProgress                 Kasus  OPR-OBS-…-068 · Sedang berlangsung
[ Pesan Tempat Tidur ] (abu-abu ±6 dtk)           Menyiapkan admisi bayi…  [ Pesan Tempat Tidur ] (abu-abu)
Perawatan  Neonatal terpisah                      Perawatan  Ruang NICU
Tempat dipesan  BD-RSMMC-BOKS-303                 Tempat dipesan  VIP 303 · 303-BOKS
Episode RI-… menunggu konfirmasi kedatangan       Episode RI-… aktif. Bayi sudah menempati
di Papan Tempat Tidur.                            Ruang NICU · NICU-1-B.
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Status kasus | Label Indonesia | `DeliveryCaseStatusName` | `BaseDetailCard` |
| Keterangan persiapan | Teks selama `bed.preparing` | hook bed | teks bantu |
| Perawatan | Label kartu terpilih | `NEWBORN_CARE_CARD_OPTIONS` | `BaseDetailCard` |
| Tempat dipesan | Nama kamar · nama bed | reservasi / bed terpilih | `BaseDetailCard` |
| Pesan Kartu Pasien | Mengikuti `episodeStatus` | episode | `InformationAlert` |

Tombol tidak berubah. Keadaan: bila status episode belum terbaca, Kartu Pasien memakai teks umum
`INPATIENT_PATIENT_CARD_MESSAGES.completed`.

### 3.7 Langkah 3 DPJP (FIX-09, rekomendasi K-05)

```text
SEBELUM: 11 dokter berurutan nama, tanpa penanda
SESUDAH
DPJP Bayi*
  Dokter Anak (disarankan)
    [ dr. Contoh Sp.A ]  Anak  [ Pilih DPJP ]
  Dokter lain
    [ dr. Contoh Lain ]  Gigi  [ Pilih DPJP ]
⚠ (bila dokter lain dipilih) Alasan memilih dokter non-Anak* [                ]
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Daftar dokter | Dua kelompok menurut spesialisasi | `fetchInpatientDoctorOptions` | `inpatient-doctor-selection-card` |
| Alasan | Wajib bila non-Anak | isian | `BaseInputField` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Lanjut ke Penempatan | utama | Selalu | Validasi DPJP dan alasan; pesan tampil bila kurang |

Keadaan: bila tidak ada dokter Anak aktif, kelompok "Dokter Anak" menampilkan "Tidak ada dokter anak
aktif" dan alasan wajib.

---

## 4. Rincian perbaikan

### Endpoint yang tersentuh perbaikan

#### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | `/newborns` | Mendaftarkan pasien bayi dari kasus persalinan ibu | `Patient : Create` | `CreateNewbornPatientRequest` + header `Idempotency-Key` | `NewbornPatientCreateResponse` |

Kode status: `200` berhasil; `400` tanggal/jam lahir atau isian tidak valid (FIX-05/06/07/21 menambah
alasan); `409` NIK bayi sudah dipakai atau kunci idempotency dipakai dengan isi berbeda; `422` aturan
bisnis ditolak; `503` penjaga nomor RM menolak.

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/newborn-mother-candidates` | Daftar ibu dari kasus persalinan yang dapat dipakai | `InpatientEpisode : Read` | `search`, `pageNumber`, `pageSize` | daftar `NewbornMotherCandidateResponse` (FIX-12, FIX-13) |
| POST | `/` | Membuka episode Draft bayi | `InpatientEpisode : Create` | episode + `newbornAdmission` | episode (FIX-09 aturan DPJP; FIX-22 nomor episode) |

Kode status: `200` berhasil; `400`/`422` konteks bayi atau DPJP tidak memenuhi aturan.

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/available-beds` | Bed yang layak untuk pasien | `InpatientBedOccupancy : Read` | `isForNewborn`, `newbornCareMode`, `requiresIntensiveCare`, `episodeId` | daftar bed + kelayakan (FIX-03, FIX-04, FIX-16) |
| POST | `/reservations` | Menahan bed | `InpatientBedOccupancy : Create` | `episodeId`, `bedId`, versi | reservasi |
| POST | `/placements` | Aktivasi penempatan | `InpatientBedOccupancy : Create` | `episodeId`, `bedId`, `expectedBedVersion` | penempatan |

Kode status: `200` berhasil; `422` bed tidak layak, misalnya `NEWBORN_BED_REQUIRED` atau kode baru dari
FIX-03/04.

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/identity-labels` | Data gelang dan label | `InpatientAdmissionDocument : Print` | — | `IdentityLabelResponse` (FIX-01, FIX-02) |
| GET | `/general-consent/print-data` | Bahan cetak persetujuan | `InpatientAdmissionDocument : Read` | — | bahan cetak; `409 INP-ADM-DOC-002` bila episode Draft (FIX-20 berhenti memanggilnya saat Draft) |

#### InpatientAdmissionConsent (menjadi "Health Services / Inpatient Management / Inpatient Admission Consent" oleh FIX-23)

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/consent`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | Menyimpan persetujuan dan tanda tangan | `InpatientEpisode : Create` | penanda tangan, klausul, tanda tangan | persetujuan (FIX-08 validasi baru) |
| GET | `/` | Membaca persetujuan tersimpan | `InpatientEpisode : Read` | — | persetujuan |

Kode status: `201` tersimpan; `400` isian wali tidak valid (bertambah oleh FIX-08).

#### Health Services / Inpatient Management / Admissions

Base URL: `api/v1/health-services/inpatient-management/admissions`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/surgical-booking-lookup` | Jadwal IBS pasien untuk admisi | `InpatientAdmission : Read` | `patientId` | ringkasan booking; `404` bila tidak ada (FIX-19 tidak memanggilnya untuk bayi) |

---

### FIX-EPS-040-01 — Bersihkan sapaan ganda "NY. NY." pada gelang bayi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-01` (sebagian) |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpWristbandRules.cs` (`BuildDisplayName`) |
| **Radius dampak** | Semua gelang bayi pada Dokumen Bayi dan Workspace PPRI; gelang dewasa tidak tersentuh |
| **Bergantung pada** | Tidak ada |

**Langkah.**
1. Sebelum merangkai "BY. NY. ", buang awalan "Ny. ", "Ny ", "Nyonya ", dan "Ibu " dari nama ibu dengan
   aturan yang sama seperti `NormalizeMotherName`.
2. Pindahkan pembersih itu ke satu tempat bersama yang dipakai pendaftaran bayi dan gelang, supaya tidak
   ada dua salinan.
3. Tambahkan uji unit.

**Acceptance criteria.**
1. Ibu "Ny. Siti Aminah", bayi ke-1 → gelang "BY. NY. SITI AMINAH / BAYI 1".
2. Ibu "Siti Aminah" (tanpa sapaan) → hasil sama dengan AC 1.
3. Ibu "NYONYA Siti Aminah" → "BY. NY. SITI AMINAH / BAYI 1".
4. Pasien dewasa tidak berubah.

**Verifikasi.** `dotnet build QuilvianSystemBackend.csproj`; uji unit `InpWristbandRules` pada proyek tes
tempat tes rawat inap bayi berada (saat ini `Tests/QuilvianSystemBackend.OperatingRoomTests`); uji layar
langkah 7 pada satu bayi.

**Risiko.** Nama ibu yang memang berawalan "Ibu" sebagai nama asli ikut terpotong. Risikonya sama dengan
pendaftaran bayi yang sudah berjalan; deteksi lewat AC 1–3.

### FIX-EPS-040-02 — Gelang memakai nama resmi bayi ditambah baris nama ibu

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-01` |
| **Area** | Backend + Frontend + dokumen |
| **Jenis perubahan** | Source backend (`InpWristbandRules`, `IdentityWristbandResponse`), source frontend (tampilan gelang di `identity-labels-section`), revisi skema BAYI-S09 |
| **Berkas yang diubah** | `InpWristbandRules.cs`, `InpAdmissionWorkspaceQueryService.cs:401-416`, DTO `IdentityWristbandResponse`, `src/components/view/health-services/inpatient-management/admission-workspace/sections/identity-labels/*` |
| **Radius dampak** | Gelang bayi di Dokumen Bayi dan Workspace PPRI; cetak ulang gelang bayi lama ikut berubah format |
| **Bergantung pada** | K-01; `FIX-EPS-040-01` |

**Langkah.**
1. Baris nama gelang bayi memakai penyusun yang sama dengan label: nama resmi pasien kapital + "/ BAYI n".
2. Tambahkan field baris ibu (misalnya `MotherLine` = "IBU: " + nama ibu bersih) untuk pasien bayi
   dengan ibu tercatat.
3. Tampilkan baris ibu di gelang bayi.
4. Revisi skema BAYI-S09 (bagian 7).

**Acceptance criteria.**
1. Bayi bernama khusus "Bagas Pratama", ibu "Ny. Siti Aminah" → baris 1 "BAGAS PRATAMA / BAYI 1",
   baris 2 "IBU: SITI AMINAH". Label baris 1 identik.
2. Bayi tanpa nama khusus → baris 1 "BY. NY. SITI AMINAH / BAYI 1", baris 2 "IBU: SITI AMINAH".
3. Bayi tanpa ibu tercatat → baris 1 nama resmi, tanpa baris ibu.
4. Riwayat cetak tetap tercatat seperti sebelumnya.

**Verifikasi.** `dotnet build`; uji unit penyusun nama; `npm run lint:errors`, `npm run test:unit`,
`npm run build`; uji layar langkah 7 untuk dua bayi (bernama dan tidak).

**Risiko.** Ukuran gelang fisik terbatas. Baris tambahan harus dicocokkan dengan printer gelang; deteksi
lewat cetak uji.

### FIX-EPS-040-03 — Mode Neonatal hanya menerima bed di kamar khusus bayi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-02` (boks kamar ibu lain) |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpNewbornAdmissionRules.cs` (`ValidateBed`) |
| **Radius dampak** | `available-beds`, denah bayi, reservasi, dan penempatan bayi mode Neonatal. Rawat gabung tidak berubah. Perpindahan klinis bayi setelah admisi tidak tersentuh (aturan hanya berlaku `isAdmission`) |
| **Bergantung pada** | Jawaban P-01 (penanda `MstRoom.IsForNewborn` kamar ruang bayi) |

**Langkah.**
1. Pada admisi bayi mode `Neonatal`, tolak bed yang kamarnya `IsForNewborn = false` dengan kode baru,
   misalnya `NEWBORN_NEONATAL_ROOM_REQUIRED`, dan pesan "Boks rawat gabung hanya untuk bayi dari ibu di
   kamar ini. Pilih kamar ruang bayi."
2. Pastikan kelayakan ini terbaca di `available-beds` (`includeIneligible=true`) supaya dialog denah
   menampilkan alasannya.
3. Tambahkan uji unit.

**Acceptance criteria.**
1. Bayi mode Perinatologi: daftar layak tidak memuat 304-BOKS dan 305-BOKS; keduanya tampil dengan
   alasan di atas.
2. Reservasi langsung ke 305-BOKS lewat API untuk bayi Perinatologi ditolak `422` dengan kode baru.
3. Bayi rawat gabung dari ibu di VIP 303 tetap mendapat 303-BOKS.
4. BAYI-1-A tetap layak.
5. Denah mode Perinatologi terbuka di Lantai 2 (membuktikan DUGAAN pada issue; bila tidak, buat FIX
   lanjutan frontend).

**Verifikasi.** `dotnet build`; uji unit `ValidateBed` untuk empat kombinasi (Neonatal × kamar bayi/kamar
dewasa, RoomingIn × kamar ibu/kamar lain); uji layar jalur Perinatologi memakai ibu cadangan.

**Risiko.** Bila kamar R. Bayi belum bertanda `IsForNewborn`, semua bed Perinatologi ikut tertolak.
Deteksi lewat P-01 sebelum rilis, dan lewat AC 4.

### FIX-EPS-040-04 — Bayi non-intensif tidak ditawari bed NICU

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-02` (NICU) |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `InpNewbornAdmissionRules.cs` (`ValidateBed`) |
| **Radius dampak** | Admisi bayi mode Neonatal non-intensif |
| **Bergantung pada** | K-02; `FIX-EPS-040-03` (fungsi yang sama) |

**Langkah.** Bila K-02 = "tidak", tolak bed `IsIntensiveCareBed` atau kamar `IsIntensiveCare` untuk
admisi bayi non-intensif, dengan pesan "Bed intensif hanya untuk bayi yang membutuhkan NICU."

**Acceptance criteria.**
1. Bayi Perinatologi tidak mendapat NICU-1-B sebagai bed layak.
2. Bayi NICU tetap mendapat NICU-1-B.
3. Bila K-02 memilih limpahan, perbaikan ini diganti rancangan fitur limpahan, dan baris register
   diperbarui.

**Verifikasi.** `dotnet build`; uji unit; uji layar kedua jalur.

**Risiko.** Saat ruang bayi penuh, bayi non-intensif tidak punya bed. Itu memang keputusan K-02, dan
pesan keadaan kosong pada skema 3.2 menjelaskannya.

### FIX-EPS-040-05 — Pemeriksaan "masa depan" memakai waktu rumah sakit

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-03` |
| **Area** | Backend (area Manajemen Pasien) |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Areas/HealthServices/PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs:93-94`; helper tanggal lokal bersama |
| **Radius dampak** | Pendaftaran bayi dari admisi rawat inap. Pemilik modul `patient-management` diberi tahu karena berkasnya di area mereka |
| **Bergantung pada** | Tidak ada |

**Langkah.**
1. Tentukan "sekarang" dalam zona waktu rumah sakit (profil rumah sakit, default Asia/Jakarta, cadangan
   Windows "SE Asia Standard Time"), memakai pola `InpAdmissionText.LocalToday` atau `FinanceBusinessDate`.
2. Bandingkan gabungan `BirthDate` + `BirthTime` dengan waktu sekarang di zona yang sama.
3. Tambahkan uji unit dengan jam tiruan.

**Acceptance criteria.**
1. Jam server 2026-10-25T18:15Z (01.15 WIB 26-10), lahir 26-10-2026 pukul 01.00 → diterima.
2. Jam server yang sama, lahir 26-10-2026 pukul 02.00 → ditolak "belum terjadi".
3. Jam server 2026-10-25T04:15Z (11.15 WIB), lahir 26-10-2026 → ditolak.

**Verifikasi.** `dotnet build`; uji unit di `Tests/QuilvianSystemBackend.PatientManagementTests`.

**Risiko.** Jam server yang tidak sinkron membuat batas bergeser. Deteksi lewat log waktu server.

### FIX-EPS-040-06 — Tolak waktu lahir yang belum terjadi di layar dan server

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-04` (waktu belum terjadi) |
| **Area** | Frontend + Backend |
| **Jenis perubahan** | Source frontend dan backend |
| **Berkas yang diubah** | `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js:62-86`; backend sesuai FIX-05 |
| **Radius dampak** | Langkah 2 Identitas Bayi; pendaftaran bayi |
| **Bergantung pada** | `FIX-EPS-040-05` (bagian server memakai helper yang sama) |

**Langkah.**
1. Di layar, gabungkan tanggal dan jam, lalu bandingkan dengan waktu sekarang Asia/Jakarta.
2. Tampilkan pesan "Waktu lahir belum terjadi. Sekarang <tanggal> pukul <jam> WIB."
3. Server menolak dengan pesan yang sama.

**Acceptance criteria.**
1. Pukul 11.15 WIB, isian hari ini 23.50 → pesan tampil di layar, tidak ada request ke server.
2. Isian hari ini 09.30 → lolos.
3. Request langsung ke API dengan waktu belum terjadi → `400`.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit` (uji `validateNewbornIdentity` dengan jam
tiruan), `npm run build`; `dotnet build`.

**Risiko.** Jam komputer petugas yang salah. Server tetap menjadi penentu akhir, sehingga pesan server
ditampilkan apa adanya.

### FIX-EPS-040-07 — Batas bawah tanggal lahir terhadap kasus persalinan

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-04` (tanggal absurd) |
| **Area** | Frontend + Backend |
| **Jenis perubahan** | Source frontend dan backend |
| **Berkas yang diubah** | `inpatient-newborn-admission-utils.js`; `NewbornPatientRegistrationService.cs` (memakai `OprCase` yang sudah dibaca di baris 149-165) |
| **Radius dampak** | Langkah 2; pendaftaran bayi |
| **Bergantung pada** | K-03; `FIX-EPS-040-06` |

**Langkah.** Terapkan batas K-03. Contoh rekomendasi: tanggal lahir ≥ tanggal kasus persalinan diminta
(`RequestedAt`, sudah ada di kandidat) dikurangi 1 hari.

**Acceptance criteria.**
1. Kasus diminta 23-10-2026; tanggal lahir 01-01-1990 → ditolak layar dan server dengan pesan "Tanggal
   lahir lebih awal dari kasus persalinan (diminta 23 Okt 2026)."
2. Tanggal lahir 22-10-2026 → diterima.
3. Tanggal lahir 21-10-2026 → ditolak.

**Verifikasi.** Sama dengan FIX-06.

**Risiko.** Kasus darurat yang dicatat terlambat. Toleransi 1 hari pada rekomendasi menampung kasus ini.

### FIX-EPS-040-08 — Wajibkan dan validasi identitas wali

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-05` |
| **Area** | Frontend + Backend |
| **Jenis perubahan** | Source frontend dan backend |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-admission-consent-form-step.jsx:260-275`; `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionConsentDtos.cs` dan validasi service persetujuan |
| **Radius dampak** | Langkah Persetujuan pada **semua** tipe admisi (pasien baru, lama, ODC, transfer, bayi) dan Edit Persetujuan di Workspace PPRI |
| **Bergantung pada** | `FIX-EPS-040-20` (berkas yang sama; kerjakan setelahnya); K-04 tidak menahan (pakai rekomendasi) |

**Langkah.**
1. `canSubmit` mensyaratkan NIK/paspor, No. HP, dan hubungan untuk penanda tangan selain "Pasien
   Sendiri".
2. Validasi format: NIK `^\d{16}$` atau paspor `^[A-Z0-9]{6,9}$` (huruf besar); No. HP 10–13 angka.
3. Tampilkan "Belum lengkap: …" di dekat tombol simpan.
4. Server menerapkan aturan yang sama.

**Acceptance criteria.**
1. NIK `ABCDE<script>999…` → pesan "NIK harus 16 angka, atau paspor 6–9 huruf/angka"; tombol tetap mati
   dengan daftar sebab.
2. NIK `3174015505950001`, HP `081298765432`, hubungan "Ibu Kandung", tanda tangan terkunci → tersimpan
   `201`.
3. Paspor `A1234567` → diterima.
4. Request API dengan NIK berisi simbol → `400`.
5. Persetujuan "Pasien Sendiri" pada pasien dewasa tetap bekerja seperti sebelumnya.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; `dotnet build`; uji layar
persetujuan bayi dan pasien dewasa.

**Risiko.** Persetujuan lama dengan NIK tidak valid gagal diedit ulang di Workspace PPRI. Deteksi lewat uji
Edit Persetujuan pada data lama; bila perlu, aturan ketat hanya untuk penyimpanan baru.

### FIX-EPS-040-09 — Terapkan aturan DPJP bayi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-06` |
| **Area** | Frontend + Backend + dokumen |
| **Jenis perubahan** | Source frontend (`inpatient-newborn-admission-steps.jsx` `NewbornCareStep`, `inpatient-doctor-selection-card.jsx`), source backend (`InpNewbornAdmissionRules.ValidateContext`), revisi kontrak 022 dan skema |
| **Berkas yang diubah** | Sesuai kolom jenis perubahan |
| **Radius dampak** | Langkah 3 admisi bayi; komponen kartu dokter juga dipakai admisi dewasa, sehingga pengelompokan hanya aktif pada mode bayi |
| **Bergantung pada** | K-05 |

**Langkah (mengikuti rekomendasi K-05).**
1. Kelompokkan dokter menurut spesialisasi Anak.
2. Wajibkan alasan bila memilih non-Anak.
3. Simpan alasan sebagai catatan admisi.
4. Server menolak DPJP non-Anak tanpa alasan.

**Acceptance criteria.**
1. Dokter Anak tampil di kelompok "Dokter Anak (disarankan)".
2. Memilih dokter Gigi tanpa alasan → "Alasan memilih dokter non-Anak wajib diisi."
3. Dengan alasan "Sp.A jaga sedang operasi, ditangani dr. umum jaga" → lanjut; alasan tercatat di catatan
   admisi.
4. Admisi dewasa tidak berubah.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; `dotnet build`.

**Risiko.** Data spesialisasi dokter yang tidak lengkap membuat dokter Anak masuk kelompok lain. Deteksi
lewat pemeriksaan master dokter.

### FIX-EPS-040-10 — Terapkan keadaan awal klausul persetujuan

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-07` |
| **Area** | Frontend + dokumen |
| **Jenis perubahan** | Source frontend; revisi skema 3.12 |
| **Berkas yang diubah** | `inpatient-admission-consent-form-step.jsx:189-208` |
| **Radius dampak** | Langkah Persetujuan pada semua tipe admisi |
| **Bergantung pada** | K-06; `FIX-EPS-040-08` (berkas yang sama) |

**Langkah (mengikuti rekomendasi K-06).**
1. Keadaan awal `agreedClauses = []` untuk persetujuan baru.
2. Tambahkan centang "Saya telah membaca dan menyetujui seluruh klausul".
3. Persetujuan tersimpan yang dibuka ulang tetap memuat klausul tersimpannya (baris 104-106).

**Acceptance criteria.**
1. Persetujuan baru dibuka → "0 Dari 12 Disetujui".
2. Centang setuju-semua → "12 Dari 12 Disetujui".
3. Persetujuan yang sudah tersimpan dibuka ulang → tetap 12/12.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`.

**Risiko.** Admisi sedikit lebih lama. Sudah ditimbang pada K-06.

### FIX-EPS-040-11 — Arah buka picker tanggal mengikuti tinggi popover

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-08` |
| **Area** | Frontend — base component |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/features/base-features/filter-date-picker.jsx:25-26, 177-188` |
| **Radius dampak** | Seluruh pemakai `FilterDatePicker` / `BaseDateField` (±222 berkas lintas modul). Pemilik base component diberi tahu |
| **Bergantung pada** | Tidak ada |

**Langkah.**
1. Pilih "bawah" hanya bila ruang bawah ≥ tinggi popover. Tinggi diambil dari ukuran popover setelah
   tampil, atau `POPOVER_ESTIMATED_HEIGHT`.
2. Bila kedua sisi kurang, pilih sisi yang lebih lapang lalu batasi tinggi dengan gulir dalam.

**Acceptance criteria.**
1. Layar 1920×945, pemicu di y≈683 → popover terbuka ke atas; tombol "Hari ini" terlihat tanpa
   menggulir.
2. Pemicu di bagian atas layar → tetap terbuka ke bawah.
3. Tiga layar lain pemakai picker (misalnya filter tanggal daftar pasien, tanggal lahir pendaftaran IGD,
   filter laporan) tetap normal.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji layar pada tiga layar AC 3.

**Risiko.** Perubahan perilaku pada layar lain. Deteksi lewat AC 3.

### FIX-EPS-040-12 — Bed dan kelas penempatan ibu pada respons kandidat

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-09` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend (DTO + query) |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/DTOs/InpatientNewbornDtos.cs:38-61`; `InpEpisodeService.NewbornCandidates.cs` |
| **Radius dampak** | Langkah 1 dan 4 admisi bayi (field tambahan, tidak menghapus field lama) |
| **Bergantung pada** | Tidak ada |

**Langkah.** Tambahkan `CurrentBedId`, `CurrentBedCode`, `CurrentBedName`, dan `CurrentPatientClassName`
dari penempatan aktif ibu.

**Acceptance criteria.**
1. Kandidat ibu di VIP 303 bed 303-A → `currentBedName = "303-A"`, `currentPatientClassName = "VIP"`.
2. Kartu konfirmasi rawat gabung menampilkan "303-A (Sedang Ditempati Ibu)".
3. Ibu tanpa penempatan aktif → field `null`, kartu menampilkan "Belum terbaca".

**Verifikasi.** `dotnet build`; uji layar langkah 4 rawat gabung.

**Risiko.** Tambahan join memperlambat daftar kandidat. Deteksi lewat waktu respons pada 50 kandidat.

### FIX-EPS-040-13 — Label status kasus persalinan berbahasa Indonesia

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-10` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `InpEpisodeService.NewbornCandidates.cs:248` |
| **Radius dampak** | Langkah 1 admisi bayi; konsumen lain field `DeliveryCaseStatusName` (periksa dengan `rg`) |
| **Bergantung pada** | Tidak ada |

**Langkah.** Petakan `InProgress` → "Sedang berlangsung" dan `Completed` → "Selesai". Pakai pemetaan
label status OK yang sudah ada bila tersedia.

**Acceptance criteria.**
1. Kasus …-068 → "OPR-OBS-…-068 · Sedang berlangsung".
2. Kasus …-069 → "… · Selesai".

**Verifikasi.** `dotnet build`; uji layar langkah 1.

**Risiko.** Konsumen yang membandingkan string Inggris. Deteksi lewat pencarian pemakai field.

### FIX-EPS-040-14 — Pesan Kartu Pasien mengikuti status episode

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-11` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx:276-284`; pemanggil di `inpatient-admission-view.jsx:240-266` |
| **Radius dampak** | Langkah Kartu Pasien pada semua tipe admisi. Pesan alur dewasa tetap |
| **Bergantung pada** | Tidak ada |

**Langkah.**
1. Teruskan `episodeStatus` dan lokasi ke komponen kartu.
2. Bila status `Admitted`, tulis "Episode <nomor> aktif. Pasien sudah menempati <kamar> · <bed>."
3. Bila masih Draft dengan reservasi, pakai teks lama.

**Acceptance criteria.**
1. Bayi NICU teraktivasi → "Episode RI-… aktif. Pasien sudah menempati Ruang NICU · NICU-1-B."
2. Admisi dewasa yang baru memesan bed → teks lama "menunggu konfirmasi kedatangan…".

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`.

**Risiko.** Kecil.

### FIX-EPS-040-15 — Ringkasan perawatan memakai tiga label kartu

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-12` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-newborn-admission-steps.jsx:662` |
| **Radius dampak** | Langkah 6 admisi bayi |
| **Bergantung pada** | Tidak ada |

**Langkah.** Tentukan label dari `careMode` + `requiresIntensiveCare` memakai `NEWBORN_CARE_CARD_OPTIONS`,
dengan pola yang sama seperti `activeCareKey` di baris 221-225.

**Acceptance criteria.**
1. NICU → "Ruang NICU".
2. Perinatologi → "Ruang Perinatologi".
3. Rawat gabung → "Rawat Gabung dengan Ibu".

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`.

**Risiko.** Kecil.

### FIX-EPS-040-16 — Alasan `BED_NOT_READY` tidak menumpuk dengan alasan spesifik

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-13` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs:1670-1671` |
| **Radius dampak** | Alasan kelayakan bed pada seluruh denah admisi dan transfer |
| **Bergantung pada** | Tidak ada |

**Langkah.** Tambahkan `BED_NOT_READY` hanya bila tidak ada alasan yang lebih spesifik (terisi, dipesan
pasien lain, ditutup, pembersihan).

**Acceptance criteria.**
1. NICU-1-A terisi → hanya "Tempat tidur ini sedang ditempati pasien lain."
2. Bed berstatus `WaitingCleaning` tanpa penghuni → tetap menampilkan "Bed belum siap…".

**Verifikasi.** `dotnet build`; uji unit evaluasi kelayakan.

**Risiko.** Ada keadaan tidak siap yang kehilangan alasannya. Deteksi lewat AC 2.

### FIX-EPS-040-17 — Keterangan "Menyiapkan admisi bayi…"

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-14` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx:480-490`; `use-inpatient-admission-bed.jsx` (ekspos `preparing`, sudah ada) |
| **Radius dampak** | Langkah pemesanan bed pada semua tipe admisi |
| **Bergantung pada** | Tidak ada |

**Langkah.** Selama `preparing` atau `!episodeId`, tampilkan "Menyiapkan admisi… (membuat kunjungan dan
episode)" di dekat tombol.

**Acceptance criteria.**
1. Klik "Lanjut ke Pemesanan" → teks tampil sampai tombol aktif.
2. Teks hilang setelah episode terbentuk.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`.

**Risiko.** Kecil.

### FIX-EPS-040-18 — Nama bed di tinjauan sama dengan langkah 4

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-15` (bagian tampilan) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-newborn-admission-steps.jsx:665` |
| **Radius dampak** | Langkah 6 admisi bayi |
| **Bergantung pada** | Tidak ada |

**Langkah.** Tampilkan `roomName · bedName` dari reservasi bila `selectedBedLocation` kosong.

**Acceptance criteria.**
1. Rawat gabung → "VIP 303 · 303-BOKS".
2. Perinatologi → "R. Bayi · BAYI-1-A".

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`.

**Risiko.** Kecil.

### FIX-EPS-040-19 — Lewati lookup booking operasi untuk pasien bayi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-16` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx:690-705` |
| **Radius dampak** | Pemuatan detail pasien di admisi; pasien dewasa tetap melakukan lookup (`FE-RWI-280`) |
| **Bergantung pada** | Tidak ada |

**Langkah.** Lewati `fetchInpatientAdmissionSurgicalBookingLookup` bila pasien terpilih adalah bayi baru
lahir (`isNewborn`).

**Acceptance criteria.**
1. Admisi bayi → tidak ada request `surgical-booking-lookup` di tab Network.
2. Admisi pasien lama dewasa dengan jadwal IBS → kartu booking operasi tetap tampil.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`.

**Risiko.** Kecil.

### FIX-EPS-040-20 — Isi otomatis wali dari identitas ibu; tanpa print-data selama Draft

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-17`, `ISS-EPS-040-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `inpatient-admission-consent-form-step.jsx:73-87, 117-175`; penerusan `newborn.mother` dari `inpatient-admission-view.jsx` |
| **Radius dampak** | Langkah Persetujuan pada semua tipe admisi. Kandidat dari print-data tetap dipakai bila episode sudah bukan Draft (misalnya pelanjutan) |
| **Bergantung pada** | Tidak ada |

**Langkah.**
1. Jangan memanggil print-data saat episode berstatus Draft.
2. Untuk bayi dengan penanda tangan "Orang Tua", isi nama, NIK, No. HP, dan alamat dari `newborn.mother`,
   dengan hubungan "Ibu Kandung".
3. Isian tetap dapat diubah.
4. Memilih "Wali yang Sah" mengosongkan isian.

**Acceptance criteria.**
1. Langkah 5 admisi bayi dibuka → nama ibu terisi; tidak ada `409` dari print-data di Network.
2. Memilih "Wali yang Sah" → isian kosong.
3. Admisi dewasa "Pasien Sendiri" tetap terisi dari pasien.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; uji layar langkah 5 bayi.

**Risiko.** Identitas ibu tanpa NIK membuat FIX-08 meminta NIK diisi manual. Itu perilaku yang benar.

### FIX-EPS-040-21 — Sanitasi karakter nama bayi dan NIK bayi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-18`, `ISS-EPS-040-19` |
| **Area** | Frontend + Backend |
| **Jenis perubahan** | Source frontend dan backend |
| **Berkas yang diubah** | `inpatient-newborn-admission-steps.jsx:158-202`; `inpatient-newborn-admission-utils.js`; `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientDtos.cs:495-512` |
| **Radius dampak** | Langkah 2 admisi bayi; endpoint `POST /patients/newborns` (pemilik `patient-management` diberi tahu) |
| **Bergantung pada** | `FIX-EPS-040-06` (berkas utils yang sama) |

**Langkah.**
1. NIK bayi menyaring karakter non-angka saat diketik, memakai pola No. HP wali
   (`consent-form-step.jsx:124`).
2. Nama bayi hanya menerima huruf (termasuk huruf beraksen), spasi, titik, koma, apostrof, dan tanda
   hubung.
3. Server menerapkan pola nama yang sama.

**Acceptance criteria.**
1. Mengetik `32abcd56` pada NIK → terisi `3256`.
2. Nama `<img src=x>Bayi` → pesan "Nama hanya boleh berisi huruf, spasi, titik, koma, apostrof, dan tanda
   hubung."
3. Nama "Muhammad Al-Fatih" dan "Ni Kadek O'Neil" → diterima.
4. Request API dengan nama bertag → `400`.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`; `dotnet build`; uji unit di
`PatientManagementTests`.

**Risiko.** Nama sah dengan karakter khusus lain tertolak. Deteksi lewat AC 3; tambahkan karakter bila
ditemukan.

### FIX-EPS-040-22 — Nomor episode dan encounter dari waktu rumah sakit

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-T2` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpEpisodeNumberService.cs:47`; `InpEpisodeService.cs:1613` |
| **Radius dampak** | Nomor episode semua admisi rawat inap baru. Nomor lama tidak diubah |
| **Bergantung pada** | `FIX-EPS-040-05` (helper waktu lokal yang sama) |

**Langkah.** Ganti `DateTime.UtcNow` dengan waktu lokal rumah sakit saat membentuk bagian `yyMMddHHmmss`.

**Acceptance criteria.**
1. Admisi pukul 11.15.19 WIB tanggal 25-10-2026 → nomor berawalan `RI-261025111519-`.
2. Admisi pukul 01.10 WIB tanggal 26-10-2026 → berawalan `RI-261026011…`.

**Verifikasi.** `dotnet build`; uji unit `InpEpisodeNumberService.Generate` (sudah terpisah untuk diuji,
baris 50-53).

**Risiko.** Nomor lama (UTC) dan nomor baru (WIB) tercampur. Keunikan tetap dijaga sufiks acak 6
karakter.

### FIX-EPS-040-23 — Tag Swagger persetujuan mengikuti pola modul

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-040-T3` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionConsentController.cs` |
| **Radius dampak** | Pengelompokan Swagger saja; rute dan kontrak tidak berubah |
| **Bergantung pada** | Tidak ada |

**Langkah.** Ganti menjadi `[Tags("Health Services / Inpatient Management / Inpatient Admission Consent")]`.

**Acceptance criteria.**
1. Swagger menampilkan endpoint persetujuan di grup "Health Services / Inpatient Management / Inpatient
   Admission Consent".

**Verifikasi.** `dotnet build`; buka `/swagger`.

**Risiko.** Kecil.

---

## 5. Urutan pengerjaan

```text
🟡 FIX-EPS-040-01   tanpa dependency — boleh dikerjakan sekarang
└── K-01 (keputusan pemilik)
    └── 🟡 FIX-EPS-040-02   K-01 diputuskan 25-10; memakai pembersih nama dari FIX-01
P-01 (hasil query penanda kamar)
└── 🟡 FIX-EPS-040-03   boleh dikerjakan sekarang; rilis setelah P-01 terjawab
    └── K-02 (keputusan pemilik)
        └── 🟡 FIX-EPS-040-04   K-02 diputuskan 25-10; fungsi yang sama dengan FIX-03
🟡 FIX-EPS-040-05   tanpa dependency — boleh dikerjakan sekarang
├── 🟡 FIX-EPS-040-06   memakai helper waktu lokal dari FIX-05
│   ├── 🟡 FIX-EPS-040-21   berkas utils yang sama
│   └── K-03 (keputusan pemilik)
│       └── 🟡 FIX-EPS-040-07   K-03 diputuskan 25-10
└── 🟡 FIX-EPS-040-22   memakai helper waktu lokal dari FIX-05
🟡 FIX-EPS-040-20   tanpa dependency
└── 🟡 FIX-EPS-040-08   berkas yang sama; K-04 tidak menahan
    └── K-06 (keputusan pemilik)
        └── 🟡 FIX-EPS-040-10   K-06 diputuskan 25-10; berkas yang sama
K-05 (keputusan pemilik)
└── 🟡 FIX-EPS-040-09   K-05 diputuskan 25-10
🟡 FIX-EPS-040-11   tanpa dependency (base component lintas modul)
🟡 FIX-EPS-040-12   tanpa dependency
🟡 FIX-EPS-040-13   tanpa dependency
🟡 FIX-EPS-040-14   tanpa dependency
🟡 FIX-EPS-040-15   tanpa dependency
🟡 FIX-EPS-040-16   tanpa dependency
🟡 FIX-EPS-040-17   tanpa dependency
🟡 FIX-EPS-040-18   tanpa dependency
🟡 FIX-EPS-040-19   tanpa dependency
🟡 FIX-EPS-040-23   tanpa dependency
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| K-01 | Format gelang bayi | Baris 1 nama resmi bayi (= label), baris 2 "IBU: [NAMA IBU]" | `FIX-EPS-040-02` |
| K-02 | Bayi non-intensif memakai bed NICU? | Tidak di alur admisi; limpahan sebagai fitur terpisah bila dibutuhkan | `FIX-EPS-040-04` |
| K-03 | Batas bawah tanggal lahir | Tidak lebih awal dari tanggal kasus persalinan diminta dikurangi 1 hari | `FIX-EPS-040-07` |
| K-04 | Jenis identitas wali | NIK 16 digit atau paspor 6–9 huruf/angka | Tidak — `FIX-EPS-040-08` memakai rekomendasi |
| K-05 | DPJP bayi | Dokter Anak diprioritaskan; non-Anak wajib alasan | `FIX-EPS-040-09` |
| K-06 | Keadaan awal klausul | Kosong + satu centang setuju-semua | `FIX-EPS-040-10` |
| K-07 | Data uji live testing 25-10-2026 | Tutup lewat alur pulang berwenang sebelum uji ulang (Ruang NICU penuh) | Tidak menahan perbaikan; menahan uji ulang jalur NICU |

Pertanyaan pelapor P-01 (query penanda kamar) menahan **rilis** `FIX-EPS-040-03`, bukan pengerjaannya.

**Keputusan yang sudah diambil (pemilik, chat 25-10-2026):** K-01, K-02, K-03, K-04, K-05, dan K-06 = rekomendasi pada tabel di atas. K-07 belum diputuskan. P-01 belum dijawab.

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi | Dipicu |
| --- | --- | --- | --- | --- |
| `docs/plan-repair/plan-repair-016-alur-admisi-episode-bayi.md` | Skema BAYI-S09, baris 654 | "Gelang: Nama bayi awal/resmi dan nama ibu; pengenal tambahan menurut format RS" | "Gelang: baris 1 nama resmi pasien kapital + '/ BAYI n' (sama dengan label); baris 2 'IBU: [nama ibu tanpa sapaan]'" | `FIX-EPS-040-02` setelah K-01 |
| `docs/issue/issue-016-alur-admisi-episode-bayi.md` | K-02 (baris 354), bagian format nama/gelang | "detail format … masih perlu ditetapkan" | Rujuk keputusan K-01 issue-040 beserta tanggalnya | K-01 |
| `05-skema-tampilan.md` | 3.12, rangka butir persetujuan dan tabel Tombol | Butir digambar `[✓]`; tidak menyebut keadaan awal | Butir digambar `[ ]` saat dibuka + centang "Saya telah membaca dan menyetujui seluruh klausul"; Simpan aktif bila 12 butir disetujui | `FIX-EPS-040-10` setelah K-06 |
| `05-skema-tampilan.md` | 3.12, Identitas Wali | "NIK *" | "NIK / Paspor * (NIK 16 angka atau paspor 6–9 huruf/angka)"; "Belum lengkap: …" di dekat tombol simpan | `FIX-EPS-040-08` |
| `contracts/newborn-admission-repair-022.md` | Tabel langkah, baris 41 (`doctor`) | "Rencana perawatan dan DPJP terverifikasi" | Tambah aturan DPJP bayi sesuai K-05 | `FIX-EPS-040-09` |
| `contracts/newborn-admission-repair-022.md` | Kode status, baris 116 | "`400` Bentuk request atau tanggal/jam lahir tidak valid" | Rinci: waktu lahir belum terjadi menurut WIB; lebih awal dari batas K-03 | `FIX-EPS-040-05/06/07` |
| `docs/plan-repair/plan-repair-031-tiga-tipe-kamar-bayi-rawat-gabung-perinatologi-nicu.md` | Aturan per kartu | Kartu Perinatologi → kamar BAYI-1/BAYI-2 (tanpa aturan server tertulis) | Tambah: "Server menolak boks di kamar dewasa untuk mode Neonatal (`FIX-EPS-040-03`); bed NICU untuk non-intensif sesuai K-02" | `FIX-EPS-040-03/04` |

Revisi dokumen dikerjakan lewat jalur blueprint yang berwenang, bukan oleh `diagnose-module-issue`.

---

## 8. Verifikasi menyeluruh

Jalankan setelah semua perbaikan yang disetujui selesai, memakai ibu cadangan yang belum dipakai
(kasus …-065, …-066, …-067) dan skrip `QuilvianSystemFrontendDev/test-with-agy/scripts/admisi-bayi-baru-lahir.e2e.mjs`.
Ruang NICU perlu dikosongkan lebih dulu (K-07).

| No | Skenario | Hasil yang diharapkan |
| ---: | --- | --- |
| V-01 | Rawat gabung, bayi tanpa nama khusus | Gelang "BY. NY. [IBU] / BAYI 1" + baris ibu (bila K-01); label baris 1 identik; kartu konfirmasi menampilkan bed ibu |
| V-02 | Perinatologi, bayi bernama khusus | Denah terbuka di Lantai 2; boks kamar dewasa dan NICU tidak dapat dipilih; gelang memakai nama khusus; ringkasan "Ruang Perinatologi" |
| V-03 | NICU | Hanya bed NICU; ringkasan "Ruang NICU"; Kartu Pasien "aktif … Ruang NICU · NICU-1-x" |
| V-04 | Identitas: hari ini 23.50 saat jam 11.xx; tanggal 1990; NIK huruf; nama bertag | Ditolak di layar dengan pesan jelas; tidak ada request ke server |
| V-05 | Uji unit waktu lahir pukul 01.15 WIB | Diterima server (FIX-05) |
| V-06 | Persetujuan bayi | Nama ibu terisi otomatis; NIK sampah ditolak; klausul sesuai K-06; tanpa 409 di Network |
| V-07 | DPJP non-Anak (bila K-05) | Alasan wajib dan tercatat |
| V-08 | Picker tanggal di layar 1920×945 | Terbuka ke atas; "Hari ini" terlihat |
| V-09 | Network keseluruhan sesi | Tanpa 5xx; tanpa 404 lookup booking; tanpa 409 print-data |
| V-10 | Nomor episode | Berbasis jam WIB |

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-25 | Rencana dibuat: 23 perbaikan (5 terblokir keputusan K-01, K-02, K-03, K-05, K-06), menunggu persetujuan pemilik | `diagnose-module-issue` |
| 2026-10-25 | Pemilik menyetujui seluruh rencana beserta rekomendasi K-01–K-06. Status → `DISETUJUI`; 23 perbaikan dikerjakan di source (`FIX-EPS-040-BE`, `FIX-EPS-040-FE`), seluruhnya `🟡` sampai build dan uji layar. Atas persetujuan pemilik, kode BE-RWI-183 (bed-hold) dan BE-RWI-184 (consumer kelahiran) yang membuat HEAD `985a1fc7` gagal build dinetralkan | `build-module-backend`, `build-module-frontend` |
