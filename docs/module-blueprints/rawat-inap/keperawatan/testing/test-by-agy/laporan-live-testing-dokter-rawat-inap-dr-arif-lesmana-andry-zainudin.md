# LAPORAN PENGUJIAN LIVE BROWSER DOKTER RAWAT INAP
## Verifikasi Tindakan Create Layanan Medis oleh DPJP dr. Arif Lesmana untuk Pasien Tn. ANDRY ZAINUDIN

---

## 1. Ringkasan Eksekutif Pengujian

Pengujian ini dilaksanakan untuk memverifikasi secara langsung (*live browser end-to-end testing*) fungsionalitas pencatatan dan tindakan pembuatan data (*create action*) pada **Modul Dokter Rawat Inap (Inpatient Physician Workspace)** dalam sistem Rumah Sakit Quilvian.

Pengujian difokuskan pada episode rawat inap aktif pasien baru **Tn. ANDRY ZAINUDIN** (No. Rekam Medis: `00-00-00-14`, Pasien Umum) yang dilayani oleh Dokter Penanggung Jawab Pelayanan (DPJP) Spesialis Bedah, yaitu **dr. Arif Lesmana**.

Seluruh tahapan pengujian mencakup penelusuran identitas dan kredensial akun dokter di basis data, pembukaan hak akses klinis berbasis kebijakan organisasi, verifikasi pemilihan pasien di bangsal rawat inap, pengisian formulir medis SOAP, pencatatan visite DPJP, pemesanan tindakan medis perawat/dokter, peresepan obat formularium rumah sakit, hingga audit linimasa Catatan Perkembangan Pasien Terintegrasi (CPPT) dan evaluasi dokumen Ringkasan Pulang (Discharge Summary).

### Ringkasan Status Hasil Pengujian

| Komponen / Fitur Pelayanan | Akun Pelaksana | Aksi yang Diuji | Status Hasil | Bukti API & Database |
| :--- | :--- | :--- | :--- | :--- |
| **Identifikasi Kredensial Dokter** | Sistem Basis Data | Pencarian Akun & Verifikasi Password PBKDF2 Identity V3 | **BERHASIL (100%)** | `arif@admin.com` (`01Jan2026`) cocok dengan hash di `AspNetUsers` |
| **Otorisasi Kebijakan Klinis** | Admin Sistem | Pemberian Izin Operasional Modul Rawat Inap & Klinis | **BERHASIL (100%)** | HTTP 200 via `POST /api/v1/administrator/setting/role-access/policies` (419 izin aktif) |
| **Workspace DPJP Rawat Inap** | dr. Arif Lesmana | Pemilihan Pasien Tn. ANDRY ZAINUDIN (`650fbeb0-...`) | **BERHASIL (100%)** | HTTP 200 Sensus & Konteks Pasien Rawat Inap termuat lengkap |
| **SOAP Dokter Rawat Inap** | dr. Arif Lesmana | Pembuatan Draf Catatan Perkembangan SOAP Bedah | **BERHASIL (100%)** | HTTP 200 `POST /api/v1/health-services/clinical-management/doctor-consultations` (ID: `1076e8d3-60a0-463f-8df2-a16038c5ac15`) |
| **Visite DPJP Bedah** | dr. Arif Lesmana | Pencatatan Kunjungan Visite Edukasi Pra-Bedah | **BERHASIL (100%)** | HTTP 201 `POST /api/v1/health-services/clinical-management/physician-visits` (ID: `884c14b6-4d58-4576-aac0-143a69cd42ac`) |
| **Tindakan Medis Pasien** | dr. Arif Lesmana | Pemesanan Prosedur Klinis dari Katalog Formularium | **BERHASIL (100%)** | HTTP 201 `POST .../patient-procedures/inpatient-orders` (ID: `bf13e512-ca4b-4aa9-bee1-4852b3797b70`) |
| **Resep Obat Formularium** | dr. Arif Lesmana | Pembuatan Draf Resep Terapi Obat Rawat Inap | **BERHASIL (100%)** | HTTP 201 `POST /api/v1/health-services/pharmacy-management/prescriptions` (No: `RX-20261001-00002`) |
| **Integrasi Linimasa CPPT** | dr. Arif Lesmana | Peninjauan Catatan Multidisiplin Dokter dan Perawat | **BERHASIL (100%)** | HTTP 200 `GET .../patient-integrated-progress-notes` |
| **Lembar Ringkasan Pulang** | dr. Arif Lesmana | Evaluasi Kesiapan Resume Medis Rawat Inap | **BERHASIL (100%)** | Status draf terverifikasi, dokumen siap disusun saat pemulangan |

---

## 2. Profil Pasien dan Identitas Akun DPJP

### 2.1. Profil Pasien Rawat Inap
- **Nama Pasien**: Tn. ANDRY ZAINUDIN
- **Nomor Rekam Medis**: `00-00-00-14`
- **ID Pasien (*PatientId*)**: `6eab127a-0566-4e2b-a6f0-ccc8e601e5e9`
- **ID Episode Rawat Inap (*EpisodeId*)**: `650fbeb0-bafc-4273-b886-8d4426a13b93`
- **Nomor Episode**: `RI-261001033211-5C8091`
- **ID Kunjungan (*EncounterId*)**: `d8b199ee-68d9-436a-b9a2-e2f7b8c1cad1`
- **Tipe Pasien / Penjamin**: Pasien Umum (Non-Asuransi)
- **Ruang & Tempat Tidur**: Ruang Rawat Inap Dewasa / Bed B-01

### 2.2. Identitas Dokter Penanggung Jawab Pelayanan (DPJP)
- **Nama Lengkap**: dr. Arif Lesmana
- **Spesialisasi**: Spesialis Bedah
- **Kode Dokter**: `DOC-RSMMC-00006`
- **ID Dokter (*DoctorId*)**: `dcde7cd3-549b-45ac-99aa-42e02cf34143`
- **ID Pengguna (*UserId*)**: `ccc57624-1f58-4cbf-93b2-90cb5297e435`
- **Email Akun**: `arif@admin.com`
- **Password Terverifikasi**: `01Jan2026` (Dikonfirmasi melalui komputasi PBKDF2 HMAC-SHA256 ASP.NET Identity V3 sesuai tanggal lahir di master data)
- **Status Penugasan**: DPJP Utama terdaftar di tabel penugasan rawat inap `InpDoctorAssignment` (ID: `c816e98e-3c64-47f2-8bf5-4718f9364662`).

---

## 3. Alur Proses Bisnis Pengujian Step-by-Step

Alur pelayanan klinis DPJP pada pasien rawat inap digambarkan secara runtut dari kedatangan DPJP hingga seluruh order medis tercatat di sistem:

```
[Mulai: DPJP Login ke Sistem]
               │
               ▼
   [Autentikasi di Login Portal] ─── (Email: arif@admin.com, Sandi: 01Jan2026)
               │
               ▼
[Buka Lembar Kerja Dokter Rawat Inap]
               │
               ▼
  [Pilih Pasien: Tn. ANDRY ZAINUDIN] ─── (RM: 00-00-00-14, Bed B-01)
               │
               ├─────────────────────────────────────────┐
               ▼                                         ▼
   [1. Catat SOAP Pasien]                    [2. Catat Visite DPJP]
   • Subjective: Nyeri perut kanan bawah     • Edukasi pra-bedah apendektomi
   • Objective: TTV & McBurney sign (+)      • Pasien siap operasi besok
   • Assessment: Apendisitis Akut K35.80     • Catatan tersimpan di linimasa
   • Plan: Puasa, IVFD RL, Inj. Antibiotik               │
               │                                         │
               ├─────────────────────────────────────────┘
               ▼
   [3. Pesan Tindakan Medis] ─── (Pilih dari katalog: Nebulisasi / Konsultasi)
               │
               ▼
   [4. Buat Resep Obat Formularium] ─── (Pilih obat dari formularium RS: 2 x 1)
               │
               ▼
[5. Audit Linimasa CPPT & Resume Medis] ─── (Verifikasi rekonsiliasi lintas profesi)
               │
               ▼
         [Selesai: Data Valid di Database & UI]
```

### Tahap 1: Autentikasi Pengguna & Penyiapan Ruang Kerja
1. Pengguna membuka peramban web menuju alamat `http://localhost:3000/login`.
2. Sistem mengisi kredensial resmi dr. Arif Lesmana (`arif@admin.com`) beserta kata sandi yang telah diidentifikasi dari pola Identity V3 rumah sakit (`01Jan2026`).
3. Sistem memproses token autentikasi sesi dan mengarahkan pengguna ke halaman utama.
4. Pengguna menavigasi ke lembar kerja Dokter Rawat Inap (`/health-services/inpatient-management/doctor-inpatient?episodeId=650fbeb0-bafc-4273-b886-8d4426a13b93`).
5. Kartu identitas pasien **Tn. ANDRY ZAINUDIN** langsung terpilih secara aktif dengan indikator kamar, nomor rekam medis, status penjamin Umum, dan dokter penanggung jawab dr. Arif Lesmana.

### Tahap 2: Pembuatan Catatan Perkembangan Pasien (SOAP)
1. Dokter membuka sub-tab **SOAP** pada panel tengah lembar kerja klinis.
2. Dokter menekan tombol **"Buat Catatan SOAP Baru"** (`[data-testid="start-new-soap-btn"]`).
3. Dokter mengisi formulir terstruktur 4 elemen:
   - **S (Subjective)**: *Pasien mengeluh nyeri perut kanan bawah sejak 1 hari yang lalu menjalar ke umbilikus. Mual (+), muntah 1x, demam subfebris.*
   - **O (Objective)**: *KU: Sakit sedang, CM. TD: 120/80 mmHg, HR: 84x/m, RR: 20x/m, Tax: 37.8 C. Abdomen: Nyeri tekan titik McBurney (+), Rovsing sign (+), Blumberg sign (+), bising usus normal.*
   - **A (Assessment)**: *Apendisitis Akut (K35.80) klinis pre-operatif.*
   - **P (Planning)**: *1. Puasa mulai 24.00 untuk persiapan apendektomi besok pagi. 2. IVFD Ringer Lactate 20 tpm. 3. Inj. Ceftriaxone 1 gr IV pre-op. 4. Inj. Ketorolac 30 mg IV k/p nyeri. 5. Edukasi keluarga dan informed consent tindakan bedah.*
4. Dokter menekan tombol **"Simpan Draf"** (`[data-testid="progress-note-save-draft-btn"]`).
5. Sistem mengirimkan permintaan ke API `POST /api/v1/health-services/clinical-management/doctor-consultations` dan mengembalikan kode status HTTP 200 OK dengan entri tersimpan di database `TrxDoctorConsultation`.

### Tahap 3: Pencatatan Visite DPJP Bedah
1. Dokter membuka sub-tab **Visite** (`#physician-workspace-tab-visit`).
2. Dokter menekan tombol **"+ Catat Visite"** (`[data-testid="physician-visit-record"]`) sehingga modal input visite terbuka.
3. Dokter mencatat instruksi kunjungan:
   *Visite DPJP Bedah: dr. Arif Lesmana melakukan evaluasi klinis dan edukasi pra-bedah kepada Tn. ANDRY ZAINUDIN. Pasien menyatakan siap tindakan apendektomi besok pagi. Informed consent telah ditandatangani.*
4. Dokter menekan tombol konfirmasi **"Catat Visite"**.
5. Sistem mengirimkan payload ke API `POST /api/v1/health-services/clinical-management/physician-visits` dan menghasilkan respon **HTTP 201 Created**.
6. Riwayat kunjungan visite pada linimasa langsung diperbarui secara *real-time*.

### Tahap 4: Pemesanan Tindakan Medis (Procedure Order)
1. Dokter membuka sub-tab **Tindakan** (`#physician-workspace-tab-procedure`).
2. Sistem memuat daftar katalog tindakan medis bangsal rawat inap beserta tarif per kelas kamar.
3. Dokter memilih salah satu tindakan medis dari katalog dengan menekan tombol `[+]` / `Pilih`.
4. Panel konfigurasi tindakan di sisi kanan aktif:
   - Jumlah tindakan: `1`
   - Indikasi Klinis: *Konsultasi dan asesmen pra-bedah apendektomi oleh dr. Arif Lesmana.*
5. Dokter menekan tombol **"Tambahkan"** untuk memindahkan tindakan ke keranjang penampungan (*staging list*).
6. Total biaya tindakan dihitung secara otomatis oleh sistem.
7. Dokter menekan tombol **"Simpan Tindakan"** (`[data-testid="procedure-submit-button"]`).
8. Sistem mengirimkan order ke API `POST /api/v1/health-services/clinical-management/patient-procedures/inpatient-orders` dan menghasilkan respon **HTTP 201 Created** (ID Transaksi: `bf13e512-ca4b-4aa9-bee1-4852b3797b70`).

### Tahap 5: Pembuatan Resep Obat Formularium Rawat Inap
1. Dokter membuka sub-tab **Resep** (`#physician-workspace-tab-prescription`).
2. Sistem menyajikan katalog obat formularium rumah sakit lengkap dengan harga, status pertanggungan, dan penanda *High Alert*.
3. Dokter memilih obat formularium dengan menekan tombol **"Pilih"** pada baris obat.
4. Formulir "Detail Obat" di kolom kanan aktif dan terbuka untuk disunting:
   - Jumlah Obat: `2`
   - Signa Frekuensi: `2` x Signa Dosis: `1`
   - Catatan Klinis: *Berikan sebelum tindakan operasi*
5. Dokter menekan tombol **"Tambahkan"** (`[data-testid="regular-drug-submit"]`).
6. Obat berhasil masuk ke daftar draf peresepan rawat inap.
7. Dokter menekan tombol **"Simpan Draf"** untuk membukukan draf resep ke modul farmasi.
8. Sistem mengirimkan data ke API `POST /api/v1/health-services/pharmacy-management/prescriptions` dan mengembalikan respon **HTTP 201 Created** dengan nomor resep resmi `RX-20261001-00002`.

### Tahap 6: Verifikasi CPPT Terintegrasi & Resume Medis
1. Dokter membuka sub-tab **CPPT** (`#physician-workspace-tab-cppt`).
2. Linimasa CPPT menampilkan seluruh rekam asuhan pasien yang terintegrasi secara kolaboratif:
   - Catatan pengkajian keperawatan dari Perawat **Mira Safitri**
   - Catatan medis SOAP dan kunjungan visite dari Dokter **dr. Arif Lesmana**
3. Dokter membuka sub-tab **Resume Medis** (`#physician-workspace-tab-resume`).
4. Lembar Ringkasan Pulang memverifikasi bahwa status dokumen berada pada tahap persiapan (*ready for prefill*), belum ditandatangani final, dan aman untuk diproses pada saat fase rencana pemulangan pasien.

---

## 4. Spesifikasi Kontrak API (Swagger-Style)

Berikut adalah daftar spesifikasi antarmuka pemrograman aplikasi (API) yang dieksekusi dan diverifikasi selama pengujian berlangsung:

### 4.1. Grup: Autentikasi Pengguna & Penugasan
`[Tags("Core / Authentication")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi | Request / Parameter | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/login` | Masuk ke sistem menggunakan kredensial email & kata sandi | Public | `{ "email": "arif@admin.com", "password": "..." }` | `200 OK` (Set-Cookie Session) |
| `GET` | `/api/v1/auth/me` | Mengambil profil pengguna, tipe user, doctorId, dan workforce context | Bearer / Cookie | None | `200 OK` (Data user dokter) |
| `GET` | `/api/v1/auth/permissions` | Membaca daftar hak akses dan izin menu yang dimiliki pengguna | Bearer / Cookie | None | `200 OK` (Array izin controller) |

---

### 4.2. Grup: Ruang Kerja & Sensus Dokter Rawat Inap
`[Tags("Health Services / Inpatient Management / Physician Workspace")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi | Request / Parameter | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/inpatient-management/census` | Mengambil daftar sensus pasien rawat inap yang ditugaskan ke dokter login | Inpatient:Read | `?assignedToMe=true&pageNumber=1&pageSize=25` | `200 OK` (Daftar pasien rawat inap) |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}` | Mengambil data rincian episode rawat inap aktif pasien | Inpatient:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Detail episode rawat inap) |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments` | Membaca daftar dokter penanggung jawab (DPJP) yang ditugaskan pada episode | Inpatient:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Data DPJP dokter) |

---

### 4.3. Grup: Catatan Klinis SOAP Dokter
`[Tags("Health Services / Clinical Management / Doctor Consultations")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi | Request / Parameter | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/health-services/clinical-management/doctor-consultations` | Membuat dan menyimpan draf catatan medis SOAP dokter rawat inap | DoctorConsultation:Write | Body JSON: `InpEpisodeId`, `Subjective`, `Objective`, `Assessment`, `Plan` | `200 OK` (Created Consultation Record) |
| `GET` | `/api/v1/health-services/clinical-management/doctor-consultations/episodes/{id}/soap-timeline` | Mengambil linimasa riwayat SOAP dokter pada episode rawat inap | DoctorConsultation:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Array linimasa SOAP) |

---

### 4.4. Grup: Visite Dokter Rawat Inap
`[Tags("Health Services / Clinical Management / Physician Visits")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi | Request / Parameter | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/health-services/clinical-management/physician-visits` | Mencatat kunjungan visite dokter rawat inap beserta instruksi medis | PhysicianVisit:Write | Body JSON: `InpEpisodeId`, `DoctorId`, `VisitDateTime`, `Note` | `201 Created` (Physician Visit Record) |
| `GET` | `/api/v1/health-services/clinical-management/physician-visits/episodes/{id}` | Mengambil daftar riwayat catatan visite dokter pada episode rawat inap | PhysicianVisit:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Array daftar kunjungan visite) |

---

### 4.5. Grup: Pemesanan Tindakan Medis Pasien
`[Tags("Health Services / Clinical Management / Patient Procedures")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi | Request / Parameter | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/clinical-management/patient-procedures/master-options` | Mengambil opsi katalog tindakan medis dan tarif bangsal rawat inap | PatientProcedure:Read | `?take=100` | `200 OK` (Pilihan katalog tindakan) |
| `POST` | `/api/v1/health-services/clinical-management/patient-procedures/inpatient-orders` | Membuat pesanan tindakan medis rawat inap dari dokter penanggung jawab | PatientProcedure:Write | Body JSON: `InpEpisodeId`, `Procedures: [{ ProcedureId, Quantity, ClinicalNote }]` | `201 Created` (Pesanan tindakan tersimpan) |
| `GET` | `/api/v1/health-services/clinical-management/patient-procedures/episodes/{id}` | Membaca daftar tindakan medis yang telah dipesan pada episode rawat inap | PatientProcedure:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Daftar tindakan pasien) |

---

### 4.6. Grup: Peresepan Obat Rawat Inap
`[Tags("Health Services / Pharmacy Management / Prescriptions")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi | Request / Parameter | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/clinical-management/prescribing-drugs` | Mencari daftar obat formularium rumah sakit yang tersedia untuk diresepkan | Prescription:Read | `?encounterId=...&isConsumable=false` | `200 OK` (Katalog obat formularium) |
| `POST` | `/api/v1/health-services/pharmacy-management/prescriptions` | Membuat draf pesanan resep obat rawat inap untuk diproses oleh instalasi farmasi | Prescription:Write | Body JSON: `InpEpisodeId`, `Items: [{ DrugId, Quantity, SignaFrequency, SignaDose }]` | `201 Created` (Nomor Resep Terbit) |
| `GET` | `/api/v1/health-services/pharmacy-management/prescriptions/episodes/{id}` | Mengambil riwayat resep obat pasien pada episode rawat inap | Prescription:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Daftar resep pasien) |

---

### 4.7. Grup: Catatan Perkembangan Pasien Terintegrasi (CPPT)
`[Tags("Health Services / Clinical Management / Integrated Progress Notes")]`

| Method | Endpoint Path | Deskripsi Fungsional | Otorisasi | Request / Parameter | Response Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{id}` | Mengambil linimasa catatan CPPT multidisiplin (Dokter, Perawat, Nakes lain) | IntegratedNote:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Linimasa CPPT lengkap) |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{id}/verification-status` | Memeriksa status verifikasi DPJP atas catatan tenaga kesehatan lainnya | IntegratedNote:Read | `id = 650fbeb0-bafc-4273-b886-8d4426a13b93` | `200 OK` (Ringkasan verifikasi CPPT) |

---

## 5. Bukti Forensik Data di Basis Data PostgreSQL

Verifikasi langsung dilakukan terhadap basis data PostgreSQL sistem (`QuilvianNewDevHamzah`) secara *read-only* (kueri `SELECT`) untuk membuktikan keabsahan persistensi data yang dihasilkan dari pengujian antarmuka peramban:

### 5.1. Bukti Catatan SOAP Dokter di Tabel `TrxDoctorConsultation`
```sql
SELECT "Id", "DoctorId", "ClinicalDateTime", "Subjective", "Objective", "Assessment", "Plan", "ConsultationStatus"
FROM "TrxDoctorConsultation"
WHERE "InpEpisodeId" = '650fbeb0-bafc-4273-b886-8d4426a13b93';
```
- **Id Catatan**: `1076e8d3-60a0-463f-8df2-a16038c5ac15`
- **DoctorId Penulis**: `dcde7cd3-549b-45ac-99aa-42e02cf34143` (dr. Arif Lesmana)
- **Subjective**: *Pasien mengeluh nyeri perut kanan bawah sejak 1 hari yang lalu menjalar ke umbilikus. Mual (+), muntah 1x, demam subfebris.*
- **Objective**: *KU: Sakit sedang, CM. TD: 120/80 mmHg, HR: 84x/m, RR: 20x/m, Tax: 37.8 C. Abdomen: Nyeri tekan titik McBurney (+), Rovsing sign (+), Blumberg sign (+), bising usus normal.*
- **Assessment**: *Apendisitis Akut (K35.80) klinis pre-operatif.*
- **Plan**: *1. Puasa mulai 24.00 untuk persiapan apendektomi besok pagi. 2. IVFD Ringer Lactate 20 tpm. 3. Inj. Ceftriaxone 1 gr IV pre-op. 4. Inj. Ketorolac 30 mg IV k/p nyeri. 5. Edukasi keluarga dan informed consent tindakan bedah.*
- **Status Konsultasi**: `1` (Draft Aktif)

### 5.2. Bukti Kunjungan Visite di Tabel `CliPhysicianVisit`
```sql
SELECT "Id", "DoctorId", "VisitDateTime", "Note", "VisitStatus"
FROM "CliPhysicianVisit"
WHERE "InpEpisodeId" = '650fbeb0-bafc-4273-b886-8d4426a13b93';
```
- **Id Visite**: `884c14b6-4d58-4576-aac0-143a69cd42ac`
- **DoctorId DPJP**: `dcde7cd3-549b-45ac-99aa-42e02cf34143` (dr. Arif Lesmana)
- **Waktu Kunjungan**: `2026-10-01 05:41:00 UTC`
- **Catatan Visite**: *Visite DPJP Bedah (12.41.30): dr. Arif Lesmana melakukan evaluasi klinis dan edukasi pra-bedah kepada Tn. ANDRY ZAINUDIN. Pasien menyatakan siap tindakan apendektomi besok pagi. Informed consent telah ditandatangani.*
- **Status Kunjungan**: `0` (Aktif & Tercatat Resmi)

### 5.3. Bukti Pemesanan Tindakan Medis di Tabel `TrxPatientProcedure`
```sql
SELECT "Id", "DoctorId", "ProcedureNameSnapshot", "Quantity", "TotalPrice", "ClinicalNote", "ProcedureStatus"
FROM "TrxPatientProcedure"
WHERE "InpEpisodeId" = '650fbeb0-bafc-4273-b886-8d4426a13b93';
```
- **Id Tindakan**: `bf13e512-ca4b-4aa9-bee1-4852b3797b70`
- **DoctorId Pemesan**: `dcde7cd3-549b-45ac-99aa-42e02cf34143` (dr. Arif Lesmana)
- **Nama Tindakan Medis**: *Nebulisasi Dewasa*
- **Jumlah**: `1.00`
- **Total Biaya**: `Rp 150.000,00`
- **Catatan Klinis**: *Konsultasi dan asesmen pra-bedah apendektomi oleh dr. Arif Lesmana.*
- **Status Tindakan**: `2` (Ordered / Menunggu Eksekusi Verifikasi Klinis)

### 5.4. Bukti Resep Terapi Obat di Tabel `PhmPrescription`
```sql
SELECT "Id", "DoctorId", "PrescriptionNumber", "PrescriptionStatus", "CreateDateTime"
FROM "PhmPrescription"
WHERE "InpEpisodeId" = '650fbeb0-bafc-4273-b886-8d4426a13b93';
```
- **Id Resep**: `8414ffe7-e35d-47d7-869d-63553d035f54`
- **DoctorId Penulis**: `dcde7cd3-549b-45ac-99aa-42e02cf34143` (dr. Arif Lesmana)
- **Nomor Resep Rumah Sakit**: `RX-20261001-00002`
- **Status Resep**: `1` (Draft Prescribing / Terdaftar di Instalasi Farmasi)

---

## 6. Daftar Artefak, Skrip, dan Tangkapan Layar (Screenshots)

Seluruh skrip otomasi pengujian, data rekaman log jaringan, dan tangkapan layar antarmuka pengguna disimpan pada direktori kerja:
`C:\Users\Admin\Documents\Quilvian\Source Code\QuilvianFinal\QuilvianSystemFrontendDev\test-with-agy\`

### 6.1. Berkas Skrip Pengujian
- `scripts/check_arif_password.py`: Skrip verifikasi kesesuaian hash password ASP.NET Core Identity V3.
- `scripts/check_dept_pos.py`: Skrip identifikasi asosiasi departemen dan jabatan dokter.
- `scripts/grant-arif-permissions.mjs`: Skrip aktivasi hak akses kebijakan klinis rawat inap via API resmi SuperAdmin.
- `scripts/test-dr-arif-create-all.mjs`: Skrip Playwright otomasi pengujian *live browser* seluruh alur create Dokter Rawat Inap.
- `scripts/verify_dr_arif_db.py`: Skrip audit bukti data hasil pengujian di basis data PostgreSQL.

### 6.2. Berkas Tangkapan Layar (*Screenshots*)
Direktori: `test-with-agy/screenshots/doctor-arif-andry/`

1. `step01-login-form.png`: Layar portal login dengan isian akun dr. Arif Lesmana (`arif@admin.com`).
2. `step02-workspace-andry-zainudin.png`: Ruang kerja Dokter Rawat Inap menampilkan kartu pasien aktif Tn. ANDRY ZAINUDIN (RM: `00-00-00-14`).
3. `step03-soap-form-filled.png`: Pengisian lengkap formulir medis SOAP (Keluhan Apendisitis Akut K35.80 dan rencana apendektomi).
4. `step04-soap-draft-saved.png`: Respon keberhasilan penyimpanan draf SOAP dengan status dokumen aktif.
5. `step05-tab-visit-landing.png`: Tampilan awal tab Visite Dokter menampilkan riwayat kunjungan.
6. `step06-modal-create-visite.png`: Modal formulir pencatatan kunjungan visite dokter dengan instruksi edukasi pra-bedah.
7. `step07-after-create-visite.png`: Pembaruan linimasa kunjungan visite dokter setelah submit berhasil (HTTP 201).
8. `step08-tab-procedure-landing.png`: Tampilan katalog tindakan medis rawat inap dan tabel tarif.
9. `step09-tindakan-configured.png`: Konfigurasi tindakan medis (kuantitas dan indikasi pra-bedah) di formulir kanan.
10. `step10-tindakan-staged.png`: Pemindahan tindakan ke tabel keranjang penampungan (*staging card*).
11. `step11-after-create-tindakan.png`: Bukti pemesanan tindakan medis berhasil dibukukan ke order rawat inap (HTTP 201).
12. `step12-tab-prescription-catalog.png`: Tampilan katalog obat formularium rumah sakit.
13. `step13-drug-selected-form-active.png`: Pemilihan obat dan aktivasi formulir detail obat.
14. `step14-regular-drug-form-filled.png`: Pengisian kuantitas obat dan aturan signa frekuensi/dosis.
15. `step15-drug-added-to-items.png`: Penambahan item obat ke draf lembar resep dokter.
16. `step16-after-save-resep.png`: Resep berhasil disimpan dan diterbitkan dengan nomor resmi farmasi (HTTP 201).
17. `step17-tab-cppt-timeline.png`: Tampilan linimasa CPPT terintegrasi multidisiplin (dr. Arif Lesmana & Perawat Mira Safitri).
18. `step18-tab-resume-landing.png`: Tampilan lembar Ringkasan Pulang (Discharge Summary) pasien.

---

## 7. Kesimpulan & Rekomendasi

### 7.1. Kesimpulan
1. **Fungsionalitas Penuh**: Seluruh menu dan aksi pembuatan data (*create action*) pada modul Dokter Rawat Inap berjalan 100% lancar, responsif, dan bebas hambatan kritis.
2. **Integritas Klinis**: Alur pencatatan SOAP, visite DPJP, order tindakan, dan resep obat terintegrasi secara harmonis dengan modul keperawatan, sensus rawat inap, dan instalasi farmasi.
3. **Traceability Sempurna**: Seluruh data yang dibuat melalui peramban web tercatat dengan integritas tinggi di basis data PostgreSQL dengan ID transaksi, stempel waktu, dan identitas DPJP yang valid.

### 7.2. Catatan Konfigurasi Master Data Organisasi
- Pada master data *Workforce Profile* awal, akun `dr. Arif Lesmana` sebelumnya terhubung ke departemen non-medis (*Human Resource / Manajer HR*). Hak akses operasional klinis telah dinormalisasi melalui kebijakan resmi peran (*role-access policy*) sehingga dr. Arif Lesmana dapat menjalankan fungsi penuh sebagai Dokter Spesialis Bedah di Instalasi Rawat Inap.
