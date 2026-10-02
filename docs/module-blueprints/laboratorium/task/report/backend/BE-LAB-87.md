# Laporan Perubahan Backend — `BE-LAB-87`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-87` |
| Judul | Identitas pasien pada rincian order |
| Slice | Susulan `MVP-8c` — dipakai Halaman Hasil Patologi Klinik per order (`FE-LAB-36`) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6ao |
| Trace | `LAB-DEC-166`; `03-frontend-architecture.md` amandemen 2026-09-24 (*Informasi pasien*) |
| Contract version | `LAB-API-v1` **`r38`** bagian 33 — **`approved` 2026-10-01** (Yoga Aji Pratama) |
| Dependency | — |
| Klasifikasi | `LIGHT` — 2 berkas; 8 ruas aditif pada satu respons; nol migration, nol izin, nol endpoint baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement` dan dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `55b032b0` (branch `yoga`) |
| Tanggal | 2026-10-01 |
| Status | ✅ **`SELESAI`** — kedelapan ruas terisi pada rincian order PK, Mikrobiologi, dan PA lewat HTTP, identik dengan daftar pantau; build 0 error; startup tanpa baris `Error`/`Fatal` |

---

## 1. Masalah yang diperbaiki

Halaman Hasil Patologi Klinik per order wajib menampilkan *Informasi pasien* — nama, No. RM, umur, jenis
kelamin, tipe kunjungan, unit layanan — dengan sumber yang sama dengan halaman Mikrobiologi, yaitu rincian
order `GET /lab-orders/{id}`. Rincian order **tidak membawa satu pun ruas pasien**, sehingga analis mengisi
hasil tanpa melihat pasiennya. Laporan Patologi Anatomi sudah membaca `patientName` dan
`medicalRecordNumber` dari rincian order yang sama dan karena itu selalu menampilkan "-".

---

## 2. Proses bisnis

1. Analis membuka satu order dari daftar pantau.
2. Layar memanggil rincian order. Kini rincian itu menyebut pasiennya: *AGNES YULIANI RAJA GUK GUK*,
   No. RM *00-00-00-13*, perempuan, lahir 13 Juli 2000, kunjungan rawat jalan di *Laboratorium Klinik*.
3. Layar menghitung umur dari tanggal lahir pada tanggal hari ini — *26 tahun* pada 1 Oktober 2026 — dan
   menampilkannya di atas tabel hasil.

**Jalur tidak normal:** order tanpa kunjungan → kedelapan ruas kosong, layar menampilkan "-". Pasien tanpa
tanggal lahir → umur "-".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Kenapa dibaca |
| --- | --- |
| `Services/LabMonitoringService.cs` | Jalur baca pasien daftar pantau — ditiru supaya satu pasien terbaca sama |
| `Services/LabOrderService.cs` (`GetDetailAsync`, `MapDetailResponse`) | Proyeksi rincian order; dua pemakai lain bentuk respons yang sama |
| `MstPatient.cs`, `RegPatientEncounter.cs`, `MstServiceUnit.cs`, `EncounterType.cs` | Nama dan tipe ruas sumber |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | `LabOrderDetailResponse` + `PatientId`, `PatientName`, `MedicalRecordNumber`, `Gender`, `BirthDate`, `EncounterNumber`, `EncounterType`, `ServiceUnitName` — seluruhnya boleh kosong |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | Proyeksi `GetDetailAsync`: sub-query `MstPatient` lewat `Encounter.PatientId` (pola `LabMonitoringService`), nomor/tipe kunjungan dan nama unit dari `Encounter` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `r38` bagian 33 — aditif; nol ruas berubah arti |
| Database | Nol schema, nol migration |
| Izin | Tidak berubah — `LabOrder : Read`. Identitas yang sama sudah dibuka daftar pantau kepada petugas laboratorium |
| Respons tindakan tulis | `MapDetailResponse` (buat/pecah order) tidak diubah; kedelapan ruas di sana kosong — sesuai 33.2 |
| Konsumen lama | Nol dampak; laporan PA kini terisi |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}` | Rincian order beserta identitas pasien (ruas baru `r38` 33.2) | `LabOrder : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error (106 detik); nol peringatan pada kedua berkas yang disentuh | `PASS` | Keluaran build |
| Startup Development (`https://localhost:7184`) | Seluruh seeder lolos; nol baris `Error`/`Fatal` | `PASS` | Log aplikasi |
| `GET /lab-orders/{id}` order PK `LAB-RSMMC-000001`, akun analis | Kedelapan ruas terisi; `patientId`, nama, No. RM, `gender`, No. kunjungan, `encounterType` **identik** dengan daftar pantau; `birthDate` `2000-07-13`; unit *Laboratorium Klinik* | `PASS` | Skrip HTTP |
| Order Mikrobiologi `LAB-RSMMC-000014` | Kedelapan ruas terisi, identik dengan daftar pantau; unit *Rawat Jalan* | `PASS` | Skrip HTTP |
| Order PA `LAB-RSMMC-000015` | `patientName` dan `medicalRecordNumber` terisi, sama dengan daftar pantau — laporan PA tidak lagi "-" | `PASS` | Skrip HTTP |
| Layar `FE-LAB-36` | *Informasi Pasien*: nama, No. RM, *26 tahun*, *Perempuan*, *Rawat Jalan*, *Laboratorium Klinik* | `PASS` | [`FE-LAB-36.md`](../frontend/FE-LAB-36.md) bagian 10 |

**Tidak dijalankan:** build dengan analyzer — memakai `-p:RunAnalyzers=False` sesuai ketentuan build lokal.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Rincian order PK dan Mikrobiologi membawa kedelapan ruas | Terpenuhi | Bagian 5 |
| Nilai bersama sama dengan daftar pantau | Terpenuhi | Bagian 5 |
| Konsumen lama tidak berubah | Terpenuhi — ruas aditif | Bagian 3.3 |
| DoD — build hijau; laporan `BE-LAB-87.md` | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan baru |
| Masalah yang diketahui | `NONE` |
| Perubahan sampingan | `NONE` |
| Status Git | ` M` `LabOrderDtos.cs`, `LabOrderService.cs`; dokumen blueprint Laboratorium. **Nol operasi Git dijalankan** |
| Langkah berikutnya | Halaman Mikrobiologi per order dapat menampilkan informasi pasien yang sama bila diputuskan |
