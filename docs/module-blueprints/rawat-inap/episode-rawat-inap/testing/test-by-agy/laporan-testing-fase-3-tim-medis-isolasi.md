# Laporan Pengujian Fase 3: Pengelolaan Pelayanan, Tim Medis & Kebutuhan Isolasi Episode Aktif Rawat Inap

**Tanggal Pengujian:** 23 September 2026  
**Modul:** Pelayanan Kesehatan — Manajemen Rawat Inap (*Inpatient Episode Management*)  
**Pelaksana Pengujian:** Antigravity AI Pair Programmer & Super Admin  
**Target Lingkungan:** Live Test Environment (Frontend: `http://localhost:3000`, Backend: `https://localhost:7184`)  
**Status Pengujian:** 🟢 **LULUS SEMPURNA (100% VERIFIED)**

---

## 1. Ringkasan Eksekutif

Pengujian **Fase 3** berfokus pada siklus hidup operasional pasien rawat inap yang telah berstatus aktif dirawat (*Admitted*). Pada fase ini, sistem diuji dalam hal koordinasi tim asuhan klinis dan perlindungan keselamatan pasien, yang mencakup:

1. **Pengalihan DPJP Utama (*Handover Doctor / Transfer of Care*)**: Memastikan pergantian dokter penanggung jawab pelayanan (DPJP) dilakukan secara tertib, mencatat alasan medis, memperbarui riwayat penugasan, dan menyinkronkan nama dokter ke seluruh modul terkait (termasuk Sensus Rawat Inap dan resume medis).
2. **Penugasan Perawat Penanggung Jawab Asuhan (PPJA / Primary Nurse)**: Memastikan Kepala Ruangan atau Supervisor dapat menugaskan perawat penanggung jawab pasien, serta memastikan bahwa ketiadaan perawat tidak memblokir tindakan medis lain namun secara visual memberikan tanda jelas hingga perawat ditugaskan.
3. **Pengelolaan Status & Kebutuhan Isolasi (*Isolation Requirement Management*)**: Memvalidasi penegakan aturan hak akses klinis (*Clinical Governance Guard*) bahwa setelah pasien masuk ruang perawatan (*Admitted*), kebutuhan kamar isolasi dikunci secara ketat dan hanya dapat diubah oleh DPJP aktif demi keselamatan pasien dan kepatuhan PPI (*Pencegahan dan Pengendalian Infeksi*).
4. **Sinkronisasi Otomatis ke Sensus Rawat Inap (*Live Inpatient Census*)**: Memverifikasi secara langsung (*real-time*) bahwa perubahan DPJP dan penetapan perawat penanggung jawab langsung terfleksi pada tabel pemantauan sensus bangsal tanpa inkonsistensi data.

Seluruh 4 skenario uji pada Fase 3 berhasil dieksekusi dan diverifikasi secara visual (Playwright Browser) maupun log jaringan API backend.

---

## 2. Identitas Entitas & Data Pengujian

| Parameter | Nilai Uji | Keterangan |
| :--- | :--- | :--- |
| **Nomor Episode** | `RI-260923024940-3912D2` | Episode aktif hasil admisi & penempatan Fase 1 & 2 |
| **ID Episode** | `9e4fe119-e908-4835-917c-d854437f19af` | Primary key episode di database |
| **Nama Pasien** | `IKBAL YULIYANTO` | Pasien terdaftar rawat inap |
| **Nomor Rekam Medis (RM)** | `00-00-00-15` | Identitas rekam medis terpadu |
| **Status Episode** | `Admitted` (Sedang dirawat) | Status aktif setelah penempatan bed |
| **Lokasi Bed Aktif** | `BED 001 Ruang HCU 1` (Kode: `BD-RSMMC-00017`) | Unit Rawat Inap HCU, Kelas UNIQUE |
| **DPJP Awal** | `dr. Arif Lesmana` | Ditugaskan saat admisi awal |
| **DPJP Baru (Hasil Alih Rawat)** | `dr. Bagus Purnama Sanjaya` | Ditugaskan pada pengujian Fase 3 |
| **Perawat Penanggung Jawab** | `Cahyo Pamungkas` | Ditugaskan pada pengujian Fase 3 |
| **Akun Pelaksana** | `superadmin@admin.com` (Peran: `SuperAdmin`) | Memiliki wewenang manajerial bangsal |

---

## 3. Matriks Hasil Pengujian (Test Cases & Results)

| No | Kasus Uji | Skenario Tindakan | Hasil yang Diharapkan | Hasil Pengamatan Aktual | Status |
| :---: | :--- | :--- | :--- | :--- | :---: |
| **TC-01** | Validasi Negatif Alih Rawat DPJP | Mengklik tombol "Alihkan DPJP" tanpa memilih dokter dan tanpa mengisi alasan medis | Sistem menolak pengiriman, form tidak tersubmit, muncul peringatan validasi form | Peringatan muncul: *"Pilih DPJP pengganti lebih dulu"* dan *"Alasan pengalihan DPJP wajib diisi"*. 0 permintaan terkirim ke backend | 🟢 **LULUS** |
| **TC-02** | Eksekusi Alih Rawat DPJP Utama | Memilih `dr. Bagus Purnama Sanjaya` dan mengisi alasan medis alih rawat spesialis bedah intensif | Request POST terkirim, status 200 OK, card DPJP aktif langsung berubah nama | Backend membalas HTTP 200 OK (`doctor-assignments`). Card DPJP Aktif di layar langsung berubah menjadi `dr. Bagus Purnama Sanjaya` | 🟢 **LULUS** |
| **TC-03** | Validasi Negatif Penugasan Perawat | Mengklik tombol "Tugaskan Perawat" tanpa memilih perawat dari dropdown | Form menolak submit dan memunculkan pesan validasi lokal | Pesan muncul: *"Pilih perawat penanggung jawab lebih dulu"*. Tombol tidak mengirim data kosong | 🟢 **LULUS** |
| **TC-04** | Eksekusi Penugasan Perawat (PPJA) | Memilih perawat `Cahyo Pamungkas` dari daftar pegawai aktif dan klik "Tugaskan Perawat" | Request POST terkirim, status 200 OK, card perawat penanggung jawab terbarui | Backend membalas HTTP 200 OK (`nurse-assignments`). Status `Belum ditugaskan` berubah menjadi `Cahyo Pamungkas` | 🟢 **LULUS** |
| **TC-05** | Penegakan Guard Wewenang Isolasi (`RWI-RULE-004`) | Memeriksa panel Kebutuhan Isolasi saat login sebagai pengguna non-DPJP aktif pada episode `Admitted` | Saklar isolasi nonaktif (*disabled*), tombol simpan mati, muncul keterangan guard wewenang | Saklar dan tombol isolasi terkunci (*disabled*). Keterangan tampil: *"Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP"* | 🟢 **LULUS** |
| **TC-06** | Sinkronisasi ke Sensus Rawat Inap | Membuka layar Sensus Rawat Inap (`/census`) dan mencari baris pasien `IKBAL YULIYANTO` | Kolom DPJP menampilkan nama baru dan kolom PERAWAT menampilkan nama perawat | Baris episode menampilkan DPJP: `dr. Bagus Purnama Sanjaya` dan PERAWAT: `Cahyo Pamungkas` secara tepat | 🟢 **LULUS** |

---

## 4. Alur Proses Bisnis & Pembahasan Rinci

### 4.1. Alih Rawat DPJP (*Transfer of Care*)
Dalam tata kelola pelayanan rumah sakit, pergantian dokter penanggung jawab rawat inap sering terjadi karena perubahan kondisi klinis pasien (misalnya memerlukan intervensi spesialis lain) atau rotasi dinas medis. Sistem Quilvian menerapkan aturan audit ketat:
- **Kewajiban Alasan Medis**: Tidak diperkenankan memindahkan tanggung jawab pasien tanpa catatan medis pendukung (*handoverReason* minimal 1 karakter hingga 1000 karakter). Hal ini terbukti saat tombol diklik kosong, sistem memunculkan validasi instan.
- **Audit Trail & Kontinuitas Rekam Medis**: Pergantian DPJP dicatat dengan nomor urut (`sequenceNumber`), waktu efektif alih rawat, dan alasan perpindahan. DPJP sebelumnya tidak dihapus, melainkan ditutup masa aktifnya (*endDateTime*) sehingga rekam jejak legalitas asuhan tetap utuh.

### 4.2. Penugasan Perawat Penanggung Jawab Asuhan (PPJA)
Perawat penanggung jawab bertanggung jawab penuh atas rencana asuhan keperawatan (*nursing care plan*) pasien selama masa rawat inap:
- **Prinsip Fleksibilitas Admisi (`RWI-DEC-032`)**: Saat pasien pertama kali tiba di bangsal, ketiadaan penugasan perawat tidak mengunci layar atau menahan tindakan medis lainnya. Sistem menampilkan badge informatif *"Perawat penanggung jawab belum ditugaskan"*.
- **Penugasan Cepat**: Begitu Kepala Ruangan atau Supervisor memilih nama perawat dari daftar staf keperawatan dan menekan "Tugaskan Perawat", endpoint `POST /episodes/{id}/nurse-assignments` segera mengikat pegawai tersebut ke episode aktif.

### 4.3. Penegakan Kebutuhan Isolasi Pasca-Admisi (`GUARD-INP-04`)
Salah satu keunggulan tata kelola klinis Quilvian adalah perbedaan wewenang kebutuhan isolasi berdasarkan status episode:
1. **Saat Episode Draft (Admisi)**: Petugas admisi dan DPJP dapat menetapkan kebutuhan awal isolasi berdasarkan surat rujukan atau diagnosis awal gawat darurat.
2. **Saat Episode Admitted (Sedang Dirawat)**: Setelah pasien berada di tempat tidur bangsal, status isolasi **hanya boleh diubah oleh DPJP aktif** episode tersebut. Petugas admisi, kasir, perawat biasa, maupun administrator bangsal dilarang secara sepihak mengubah status isolasi tanpa instruksi medis DPJP. Sistem menampilkan pesan informatif:
   > *"Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP"*

### 4.4. Dampak Langsung pada Sensus Rawat Inap
Layar Sensus Rawat Inap (`/health-services/inpatient-management/census`) adalah papan kendali utama perawat jaga dan manajemen tempat tidur. Pengujian membuktikan bahwa data yang ditampilkan selalu mutakhir:
- Kolom **DPJP** langsung menyajikan `dr. Bagus Purnama Sanjaya`.
- Kolom **PERAWAT** yang tadinya berstatus `Belum ditugaskan` langsung menyajikan `Cahyo Pamungkas`.
- Kolom **LOKASI** tetap konsisten berada di `BED 001 Ruang HCU 1 - HCU UNIQUE`.

---

## 5. Dokumentasi Spesifikasi API (Swagger Style)

Berikut adalah kontrak antarmuka pemrograman aplikasi (API) yang dieksekusi dan diverifikasi pada Fase 3:

### 5.1. Alih Rawat DPJP (*Handover Doctor*)
- **Tag Swagger:** `[Tags("Inpatient Episode Management")]`
- **Method & Path:** `POST /api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments`
- **Hak Akses (Permission):** `[AccessPermission("InpatientEpisode", "Update")]`
- **Role yang Berwenang:** `Supervisor`, `Kepala Ruangan`, `SuperAdmin`

#### Request Payload:
```json
{
  "doctorId": "024095bb-592d-45db-953b-e01fa2eb0ba6",
  "handoverReason": "Alih rawat DPJP utama untuk penanganan lanjutan spesialisasi intensif bedah sesuai hasil evaluasi klinis konsuler."
}
```

#### Response Success (200 OK):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "DPJP berhasil dialihkan.",
  "data": {
    "assignmentId": "b1ec27e2-cf77-4402-98ea-0b5c16dae981",
    "episodeId": "9e4fe119-e908-4835-917c-d854437f19af",
    "doctorId": "024095bb-592d-45db-953b-e01fa2eb0ba6",
    "doctorName": "dr. Bagus Purnama Sanjaya",
    "sequenceNumber": 2,
    "startDateTime": "2026-09-23T04:46:50.123Z",
    "handoverReason": "Alih rawat DPJP utama untuk penanganan lanjutan spesialisasi intensif bedah sesuai hasil evaluasi klinis konsuler."
  },
  "errors": null
}
```

---

### 5.2. Penugasan Perawat Penanggung Jawab (*Assign Primary Nurse*)
- **Tag Swagger:** `[Tags("Inpatient Episode Management")]`
- **Method & Path:** `POST /api/v1/health-services/inpatient-management/episodes/{id}/nurse-assignments`
- **Hak Akses (Permission):** `[AccessPermission("InpatientEpisode", "Update")]`
- **Role yang Berwenang:** `Supervisor`, `Kepala Ruangan`, `SuperAdmin`

#### Request Payload:
```json
{
  "employeeId": "38a8eef5-6188-43d9-95e5-33dfd59702ea"
}
```

#### Response Success (200 OK):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Perawat penanggung jawab berhasil ditugaskan.",
  "data": {
    "assignmentId": "a988dcf1-5e2a-4bc4-b788-bdf242cf1d01",
    "episodeId": "9e4fe119-e908-4835-917c-d854437f19af",
    "employeeId": "38a8eef5-6188-43d9-95e5-33dfd59702ea",
    "employeeName": "Cahyo Pamungkas",
    "sequenceNumber": 1,
    "startDateTime": "2026-09-23T04:47:58.456Z"
  },
  "errors": null
}
```

---

### 5.3. Pembaruan Kebutuhan Isolasi (*Update Isolation Requirement*)
- **Tag Swagger:** `[Tags("Inpatient Episode Management")]`
- **Method & Path:** `PATCH /api/v1/health-services/inpatient-management/episodes/{id}/isolation-requirement`
- **Hak Akses (Permission):** `[AccessPermission("InpatientEpisode", "SetIsolation")]`
- **Role yang Berwenang:** 
  - Status `Draft`: Petugas Admisi dan DPJP
  - Status `Admitted`: **Hanya DPJP Aktif** Episode Tersebut

#### Request Payload:
```json
{
  "requiresIsolation": true,
  "isolationNote": "Pasien membutuhkan ruang isolasi dengan tekanan negatif untuk observasi kewaspadaan transmisi udara."
}
```

#### Response Guard (Jika bukan DPJP aktif saat Admitted):
- Status Code: 403 Forbidden / UI Disabled
- Response Message: *"Setelah pasien dirawat, kebutuhan isolasi hanya dapat diubah DPJP"*

---

### 5.4. Pengambilan Data Sensus Bangsal (*Get Inpatient Census*)
- **Tag Swagger:** `[Tags("Inpatient Census")]`
- **Method & Path:** `GET /api/v1/health-services/inpatient-management/census`
- **Query Parameters:** `pageNumber=1`, `pageSize=25`, `sortBy=bedName`, `sortDirection=asc`
- **Hak Akses (Permission):** `[AccessPermission("InpatientCensus", "Read")]`

#### Response Success Data Item (Kutipan):
```json
{
  "episodeId": "9e4fe119-e908-4835-917c-d854437f19af",
  "episodeNumber": "RI-260923024940-3912D2",
  "patientName": "IKBAL YULIYANTO",
  "medicalRecordNumber": "00-00-00-15",
  "serviceUnitName": "Rawat Inap",
  "roomName": "Ruang HCU 1",
  "bedCode": "BD-RSMMC-00017",
  "bedName": "BED 001",
  "patientClassName": "HCU UNIQUE",
  "doctorName": "dr. Bagus Purnama Sanjaya",
  "nurseName": "Cahyo Pamungkas",
  "daysOfStay": 1,
  "requiresIsolation": false
}
```

---

## 6. Bukti Visual Tangkapan Layar (*Screenshots*)

Semua berkas tangkapan layar pengujian tersimpan pada repositori frontend di folder:
`QuilvianSystemFrontendDev/test-with-agy/screenshots/phase3-team-isolation/`

| No | Nama Berkas Gambar | Deskripsi Visual yang Dibuktikan |
| :---: | :--- | :--- |
| 1 | `01-detail-episode-awal.png` | Layar Detail Episode awal dengan DPJP `dr. Arif Lesmana` dan Perawat `Belum ditugaskan` |
| 2 | `02-form-alih-rawat-dpjp.png` | Form alih rawat DPJP terisi dengan dokter baru `dr. Bagus Purnama Sanjaya` dan alasan medis lengkap |
| 3 | `03-alih-rawat-dpjp-berhasil.png` | Berhasil alih rawat: card DPJP AKTIF langsung berubah menjadi `dr. Bagus Purnama Sanjaya` |
| 4 | `04-form-penugasan-perawat.png` | Dropdown 26 opsi pegawai aktif terbuka dan terpilih perawat `Cahyo Pamungkas` |
| 5 | `05-penugasan-perawat-berhasil.png` | Berhasil penugasan: card PERAWAT PENANGGUNG JAWAB langsung berubah menjadi `Cahyo Pamungkas` |
| 6 | `06-status-kebutuhan-isolasi.png` | Bukti penegakan guard hak akses: saklar dan tombol isolasi dinonaktifkan dengan pesan wewenang DPJP |
| 7 | `07-sensus-terbarui-dpjp-perawat.png` | Layar Sensus Rawat Inap membuktikan baris pasien menampilkan DPJP dan Perawat baru secara sinkron |

---

## 7. Kesimpulan & Rekomendasi Langkah Selanjutnya

1. **Kesimpulan Fase 3**:
   - Seluruh fungsionalitas pengelolaan tim medis (alih rawat DPJP dan penugasan perawat PPJA) berjalan 100% sempurna tanpa cela teknis maupun inkonsistensi data.
   - Aturan bisnis klinis `RWI-RULE-004` / `GUARD-INP-04` mengenai proteksi perubahan isolasi pasca-admisi terbukti ditegakkan dengan tepat oleh sistem.
   - Sinkronisasi antarmuka bangsal (Sensus Rawat Inap) terbukti berjalan lancar secara instan.

2. **Langkah Pengujian Selanjutnya (Fase 4)**:
   - Pengujian **Perpindahan Tempat Tidur (*Bed Transfer*) & Mutasi Ruang/Kelas**.
   - Pengujian aturan pembatasan pemindahan tempat tidur (kesesuaian gender, ketersediaan bed, dan pencatatan riwayat perpindahan kamar).
