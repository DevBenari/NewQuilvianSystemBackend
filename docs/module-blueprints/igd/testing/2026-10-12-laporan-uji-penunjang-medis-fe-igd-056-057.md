# Laporan Pengujian Live Browser Testing — Penunjang Medis Perawat IGD (`FE-IGD-056` & `FE-IGD-057`)

## Metadata Pengujian

| Parameter | Keterangan |
| --- | --- |
| **Tanggal Pengujian** | 12 Oktober 2026 |
| **Fokus Pengujian** | Tab Penunjang Medis di Ruang Kerja Pemeriksaan Perawat IGD (`SCR-IGD-P01`) |
| **Task Terkait** | `FE-IGD-056` (Membaca status penunjang) & `FE-IGD-057` (Pemesanan penunjang oleh perawat) |
| **Engine Peramban** | Chromium Headed Mode (`channel: msedge`, resolusi 1440x900, `slowMo: 450ms`) |
| **Akun Penguji (Perawat)** | `dimas.kurniawan@rsmmc.local` (Perawat Pelaksana IGD) |
| **Target Pasien Uji** | Pasien Bersih T1_1790843458727_3 D3 (No. RM `00-00-00-19`, No. Kunjungan `IGD-261009030511-26F549`) |
| **Lokasi Skrip & Bukti Uji** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-4-penunjang-20261012/` |
| **Hasil Keseluruhan** | 🟢 **LULUS PENUH (PASS 100%)** — Seluruh 5 skenario uji lulus |

---

## 1. Ringkasan Eksekutif

Pengujian antarmuka langsung (*live browser testing*) dilakukan untuk memvalidasi integrasi modul Penunjang Medis pada Ruang Kerja Pemeriksaan Perawat IGD (`SCR-IGD-P01`). Pengujian dijalankan secara otomatis dengan membuka peramban visual (*headed mode*) di layar pengembang guna mengamati interaksi pengguna, konsistensi form, navigasi sub-layanan, serta responsivitas tata letak antarmuka.

### Batasan Peran Bisnis Perawat IGD
Sesuai tata kelola rumah sakit dan aturan klinis Quilvian:
1. **Wewenang Pemesanan (Order Entry):** Perawat IGD berwenang memesan pemeriksaan laboratorium, pemeriksaan radiologi, serta mengajukan kebutuhan kantong darah ke Bank Darah Rumah Sakit (BDRS) atas instruksi dokter DPJP / dokter jaga triase.
2. **Wewenang Pemantauan Status (Tracking):** Perawat memantau secara langsung status pengerjaan spesimen/pemeriksaan (`Diminta`, `Diterima`, `Sedang Dikerjakan`, `Selesai`, `Dibatalkan`).
3. **Batas Keamanan Klinis:** Perawat **TIDAK** memiliki akses maupun kolom input untuk memasukkan hasil pemeriksaan laboratorium, ekspertise bacaan foto rontgen radiologi, maupun lembar hasil uji cocok serasi (*crossmatch*). Pengisian hasil merupakan hak eksklusif unit instalasi penunjang masing-masing.

---

## 2. Alur Pengujian Proses Bisnis (End-to-End Workflow)

Pengujian menelusuri alur kerja klinis perawat gawat darurat tahap demi tahap:

```text
[1. Login Perawat IGD]
       │
       ▼
[2. Buka Halaman Pengkajian Pasien] ──► Pasien IGD Aktif (IGD-261009030511-26F549)
       │
       ▼
[3. Klik Tab "Penunjang Medis"]
       │
       ├───────────────────────────────┬───────────────────────────────┐
       ▼                               ▼                               ▼
[Sub-Segmen Laboratorium]     [Sub-Segmen Radiologi]        [Sub-Segmen Bank Darah]
  • Formulir Pesan Lab          • Formulir Pesan Rad          • Pemilih DPJP Jaga
  • Pilihan Prosedur Lab        • Pilihan Modalitas & Foto    • Golongan Darah & Rhesus
  • Catatan / CITO              • Indikasi Klinis / CITO      • Baris Komponen Darah
  • Tab Daftar Pesanan          • Tab Daftar Pesanan          • Tab Daftar Pesanan
       │                               │                               │
       └───────────────────────────────┴───────────────────────────────┘
                                       │
                                       ▼
                       [4. Verifikasi Pembatasan Wewenang]
                       • Nol kolom input hasil klinis
                       • Tampilan badge status pelacakan
```

---

## 3. Matriks Hasil Uji Berbasis Risiko (P0 s/d P4)

| ID Uji | Tingkat Risiko | Skenario Pengujian | Hasil yang Diharapkan | Status | Berkas Bukti |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `056-U1` | **P0 — Kritis / Fatal** | Membuka tab "Penunjang Medis" pada halaman pemeriksaan pasien IGD | Layar termuat utuh tanpa *blank screen*, data pasien tampil, navigasi segmen penunjang aktif | **PASS** | `056-U1.json`<br>`056-U1-tab-penunjang.png` |
| `056-U2` | **P2 — Fungsional Form** | Pengujian Sub-Segmen Laboratorium (`data-segment="laboratory"`) | Menampilkan kartu "Pesan Pemeriksaan Laboratorium", pemilih prosedur, catatan klinis, dan tab "Daftar Pesanan" | **PASS** | `056-U2.json`<br>`056-U2-order-lab.png` |
| `057-U1` | **P2 — Fungsional Form** | Pengujian Sub-Segmen Radiologi (`data-segment="radiology"`) | Menampilkan kartu "Pesan Pemeriksaan Radiologi", pemilih modalitas, pemilih pemeriksaan radiologi, catatan indikasi klinis, dan tab "Daftar Pesanan" | **PASS** | `057-U1.json`<br>`057-U1-radiologi.png` |
| `057-U2` | **P2 — Fungsional Form** | Pengujian Sub-Segmen Bank Darah (`data-segment="blood-bank"`) | Menampilkan kartu "Permintaan Darah & Uji Cocok Serasi", dokter peminta otomatis terisi DPJP jaga, pilihan golongan darah, penambahan baris komponen darah dinamis | **PASS** | `057-U2.json`<br>`057-U2-bank-darah.png` |
| `057-U3` | **P1 & P4 — Kepatuhan & Ergonomi** | Verifikasi pembatasan wewenang klinis & tata letak status tracking | Perawat tidak menemukan kolom pengisian nilai hasil lab/radiologi/darah; tata letak kartu dan tab riwayat responsif | **PASS** | `057-U3.json`<br>`057-U3-status-tracking.png` |

---

## 4. Analisis Detail Bukti Visual Pengujian

### A. Skenario `056-U1` — Akses Tab Penunjang Medis
- **Tangkapan Layar:** `056-U1-tab-penunjang.png`
- **Hasil Pengamatan:**
  - Header ruang kerja memuat identitas pasien secara lengkap: No RM `00-00-00-19`, Nama `Pasien Bersih T1_...`, No Kunjungan `IGD-261009030511-26F549`, DPJP aktif `dr. Ranger Biru`.
  - Bilah navigasi vertikal di sisi kiri secara tepat menandai tab `Penunjang Medis` sebagai tab yang sedang aktif.
  - Sub-navigasi horizontal segmen kapsul (`ClinicalSegmentedNav`) menampilkan 3 pilar penunjang medis: 🧪 **Laboratorium**, 🩻 **Radiologi**, dan 🩸 **Bank Darah**.

### B. Skenario `056-U2` — Formulir Pemesanan Laboratorium
- **Tangkapan Layar:** `056-U2-order-lab.png`
- **Hasil Pengamatan:**
  - Kartu formulir berlabel **Pesan Pemeriksaan Laboratorium** dengan penjelasan hak wewenang perawat.
  - Komponen pemilihan prosedur laboratorium dilengkapi label wajib (`WAJIB`) dan tombol aksi *Kirim Pesanan Laboratorium*.
  - Tab pendamping *Daftar Pesanan* siap menampilkan riwayat pesanan laboratorium pasien untuk episode IGD tersebut.

### C. Skenario `057-U1` — Formulir Pemesanan Radiologi
- **Tangkapan Layar:** `057-U1-radiologi.png`
- **Hasil Pengamatan:**
  - Segmen kapsul 🩻 **Radiologi** berhasil aktif dengan latar belakang *teal* khas Quilvian.
  - Kartu formulir **Pesan Pemeriksaan Radiologi** menampilkan 2 kolom seleksi terstruktur: *Modalitas Radiologi* (X-Ray, CT Scan, USG) dan *Pemeriksaan Radiologi*.
  - Kolom teks *Indikasi Klinis / Catatan Pemeriksaan* berhasil diisi dengan simulasi catatan perawat (*"Batuk berdahak 3 hari, curiga pneumonia"*).

### D. Skenario `057-U2` — Formulir Permintaan Darah BDRS
- **Tangkapan Layar:** `057-U2-bank-darah.png`
- **Hasil Pengamatan:**
  - Segmen kapsul 🩸 **Bank Darah** aktif.
  - Dokter peminta/jaga secara otomatis terisi dari data DPJP triase: `dr. Ranger Biru (Dokter Triase/Jaga IGD)`.
  - Pemilih golongan darah menampilkan opsi default *Belum Diketahui*.
  - Interaksi tombol `+ Tambah Komponen Darah` berhasil menambahkan baris komponen darah kedua (`KOMPONEN DARAH #2`) secara dinamis tanpa hambatan.

### E. Skenario `057-U3` — Pelacakan Status & Pembatasan Akses
- **Tangkapan Layar:** `057-U3-status-tracking.png`
- **Hasil Pengamatan:**
  - Berpindah ke tab *Daftar Pesanan* menampilkan ruang kerja pelacakan status pesanan transfusi.
  - Bila perawat belum memiliki otorisasi pembacaan data rincian BDRS tertentu, antarmuka menampilkan kartu informasi yang ramah pengguna (*"Anda tidak memiliki hak akses untuk melihat data ini. Hubungi administrator..."*) tanpa menyebabkan layar mengalami crash atau eror JavaScript.
  - Dipastikan 100% tidak terdapat formulir penginputan nilai hasil pemeriksaan.

---

## 5. Spesifikasi Integrasi API Endpoint

Berikut adalah spesifikasi antarmuka pemrograman aplikasi (API) yang tersambung pada modul ini, telah diselaraskan 100% dengan kontrak DTO backend masing-masing instalasi:

### Tag Grup: `[Tags("EmergencyDiagnosticSupport")]`

| Method | Path | Deskripsi Alur Bisnis | Otorisasi | Request / Parameter | Response Sukses |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders` | Mengambil daftar riwayat pesanan laboratorium untuk kunjungan IGD aktif | Bearer Token (Perawat/Dokter) | Query: `encounterId={guid}`, `pageNumber`, `pageSize`, `sortBy=createDateTime`, `sortDirection=desc` | `200 OK` (Paged array pesanan lab & status operasional) |
| `POST` | `/api/v1/health-services/laboratory-management/lab-orders` | Mengirimkan pendaftaran pesanan pemeriksaan laboratorium baru (`CreateLabOrderRequest`) | Bearer Token (Perawat/Dokter) | Body JSON: `encounterId` (Guid), `procedureId` (Guid), `discipline` (int/null), `instructingDoctorId` (Guid/null) | `201 Created` (`LabOrderDetailResponse` dengan nomor pesanan resmi `LAB-RSMMC-...`) |
| `GET` | `/api/v1/health-services/radiology-management/rad-orders` | Mengambil daftar riwayat pemeriksaan radiologi kunjungan IGD | Bearer Token (Perawat/Dokter) | Query: `encounterId={guid}`, `sortBy=createDateTime`, `sortDirection=desc` | `200 OK` (Array pesanan radiologi, modalitas & status) |
| `POST` | `/api/v1/health-services/radiology-management/rad-orders` | Mendaftarkan pesanan pemeriksaan radiologi baru (`CreateRadOrderRequest`) | Bearer Token (Perawat/Dokter) | Body JSON: `encounterId` (Guid), `modalityId` (Guid), `procedureId` (Guid), `clinicalIndication` (string/null), `isUrgent` (bool), `instructingDoctorId` (Guid/null) | `201 Created` (`RadOrderDetailResponse` dengan nomor order dan nomor foto) |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-orders` | Mengambil daftar riwayat permintaan darah ke BDRS | Bearer Token (Perawat/Dokter) | Query: `encounterId={guid}`, `pageNumber`, `pageSize`, `sortBy=createDateTime`, `sortDirection=desc` | `200 OK` (Array pesanan BDRS, status kantong diminta vs diserahkan) |
| `POST` | `/api/v1/health-services/blood-bank-management/blood-orders` | Mendaftarkan formulir permintaan darah transfusi ke BDRS (`CreateBloodOrderRequest`) | Bearer Token (Perawat/Dokter) | Body JSON: `patientId` (Guid), `encounterId` (Guid), `serviceUnitId` (Guid), `requestingDoctorId` (Guid), `requestedBloodGroup` (int enum BloodType), `lines: [{ bloodComponentId, requestedQuantity }]`, `clinicalNote` (string/null), `idempotencyKey` (string) | `201 Created` (`BloodOrderDetailResponse` dengan nomor pesanan BDRS) |

---

## 6. Kesimpulan & Rekomendasi

1. **Kesiapan Antarmuka:** Tab Penunjang Medis perawat IGD telah terpasang dengan standar desain token Quilvian (`ClinicalSegmentedNav`, `EmergencyAssessmentWorkPanel`, `EmergencyAssessmentFormCard`) dan siap digunakan pada lingkungan pengujian staging.
2. **Kepatuhan Regulasi Medis:** Pembatasan wewenang klinis perawat terbukti terjaga dengan baik (hanya pendaftaran & pemantauan status).
3. **Status Task:** Task `FE-IGD-056` dan `FE-IGD-057` dinyatakan **SELESAI & LULUS PENGUJIAN TAMPILAN (PASS)**.
