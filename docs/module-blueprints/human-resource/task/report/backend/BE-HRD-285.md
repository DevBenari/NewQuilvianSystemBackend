# Laporan Penyelesaian Task: BE-HRD-285 Controller API Kontrak v22 5 Tag Swagger, Hard-Block Payroll Final INT-HR-87 & Audit Verifikasi E2E

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-285`
* **Nama Task**: Controller API Kontrak v22 5 Tag Swagger, Hard-Block Payroll Final INT-HR-87 & Audit Verifikasi E2E
* **Modul / Sub-Modul**: Human Resource / Sub-modul 18: Hubungan Industrial, Disiplin & Terminasi
* **Slice Delivery**: `S-C5` (Kontrak API `v22`, Gelombang `Verifikasi`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `EmployeeDisciplinaryController.cs`
  - `EmployeeGrievanceController.cs`
  - `BipartiteForumController.cs`
  - `EmployeeSeparationController.cs`
  - `MultiDeptExitClearanceController.cs`
  - `IExitClearanceStatusProvider.cs`
  - `ItAccessRevocationSyncWorker in EmployeeRelationWorkers.cs`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement` (Sub-modul 18: Hubungan Industrial & Terminasi)
* **Prefix Registry**: `Hrd` / `Trx` / `Mst` (Kepatuhan Kamus Data Bagian 23)
* **Klasifikasi Kode**: `NEW CODE & EXTENSION`
* **Wewenang**: Implementasi modul backend sesuai kontrak terkunci `v22` dan aturan bisnis `VR-EMP-01` s.d. `VR-EMP-25`.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam tata kelola kepegawaian rumah sakit, kepatuhan hubungan industrial dan pemutusan ikatan kerja memiliki dampak hukum, finansial, dan keselamatan operasional yang kritis:
1. **Prinsip Just Culture RS**: Membedakan pelanggaran tata tertib kerja menjadi Human Error (edukasi/pembinaan Sub-16), At-Risk Behavior (peringatan terukur), dan Reckless Misconduct (sanksi berat/terminasi).
2. **Kepatuhan Regulasi Ketenagakerjaan**: Penegakan masa berlaku SP 1-3 tepat 6 bulan kalender sesuai Pasal 52 PP No. 35 Tahun 2021, jaminan upah penuh 100% pada masa skorsing investigasi sesuai Pasal 157A UU Ketenagakerjaan, dan formula pesangon deterministik.
3. **Pusat Kendali Exit Clearance Multi-Departemen**: Penegakan gembok keamanan (*Hard-Block*) Sub-10 Payroll (`INT-HR-87`), memastikan hak gaji dan pesangon terakhir tidak dapat dicairkan sebelum 5 departemen (Unit Kerja, Umum/Aset, Farmasi/Alkes, Keuangan, IT) memberikan persetujuan bebas tanggungan fisik dan finansial.

---

## 4. Rincian Implementasi & Alur Teknis
* **Ringkasan Pekerjaan**: Implementasi seluruh 24 endpoint RESTful API pada 5 Controller Swagger Kontrak v22, service provider gerbang hard-block payroll Sub-10 (INT-HR-87, VR-EMP-25), worker penutupan IAM EMP-W6, dan kompilasi bersih 0 error.
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 23, dan matriks validasi Sub-modul 18.
* **Kriteria Penerimaan (Acceptance Criteria)**: 5 Controller terdaftar di Swagger UI dengan 5 Tag resmi; rilis payroll akhir terblokir otomatis sebelum clearance tuntas 100% (VR-EMP-25); kompilasi .NET 0 error; QBE Conformance PASS.

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS**.
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1p (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`).

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (cabut/pulihkan akses SIMRS otomatis)

**Masalah yang diperbaiki:** `ItAccessRevocationSyncWorker` (`EMP-W6`) hanya menulis log, sehingga perawat yang diskors tetap bisa masuk SIMRS dan membuka rekam medis.

**Keputusan yang diterapkan:** sistem **mengunci dan memulihkan akses secara otomatis**, tanpa menunggu admin IT. Dasarnya: SIMRS/EMR berjalan di aplikasi ini dan pemeriksaan hak akses per request hanya menerima akun `IsActive`. Maka setiap 15 menit worker: (1) untuk skorsing `Active` dengan `IsSystemAccessRevoked`, menonaktifkan akun pegawai (`IsActive = false`, `LockoutEnd` sampai tanggal akhir skorsing) dan mencatat baris `Locked` di `HrdSuspensionAccessAction` beserta keadaan aktif semula; (2) untuk skorsing yang berakhir atau pegawai yang direhabilitasi, memulihkan **hanya** akun yang ia kunci sendiri ke keadaan semula dan mencatat `Restored`. Skorsing yang berujung PHK tidak memulihkan akses. Pemegang hak akses `EmployeeDisciplinary : ManageSuspension` diberi tahu setiap tindakan. Akses fisik kartu pintar berada di sistem gedung yang tidak terhubung dan tetap manual.

Contoh: perawat A diskors 1–30 November. Paling lambat 15 menit sesudah surat skorsing aktif, akun A dinonaktifkan; request berikutnya ditolak pemeriksaan hak akses walaupun token lama masih dipegang. 1 Desember, atau saat HR mencatat rehabilitasi, akun A aktif kembali otomatis.

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/HrdSuspensionAccessAction.cs` + konfigurasi EF | Jejak kunci/pulih per akun |
| `.../EmployeeRelationManagement/Workers/EmployeeRelationWorkers.cs` | `EMP-W6` mengunci/memulihkan; `EMP-W2`..`W5` mengirim pemberitahuan |
| `Repositories/ApplicationDbContext.cs` | `DbSet<HrdSuspensionAccessAction>` |
| `Migrations/20261111020000_AddHrdNotificationAndSuspensionAccess.cs` | Tabel `HrdSuspensionAccessAction` |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Status 🟡: kode selesai, migration belum dijalankan, `AT-HRD-EMP-22` `NOT RUN`. Risiko: akun yang sudah nonaktif sebelum skorsing tidak dihidupkan saat pemulihan (disengaja).


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.
