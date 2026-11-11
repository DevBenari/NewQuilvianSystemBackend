# Laporan Penyelesaian Task: BE-HRD-277 Service Skorsing Upah Penuh (Paid Suspension) & Event Pembekuan Akses SIMRS (INT-HR-88)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-277`
* **Nama Task**: Service Skorsing Upah Penuh (Paid Suspension) & Event Pembekuan Akses SIMRS (INT-HR-88)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 18: Hubungan Industrial, Disiplin & Terminasi
* **Slice Delivery**: `S-C5` (Kontrak API `v22`, Gelombang `MVP-55`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `IDisciplinarySuspensionService.cs`
  - `DisciplinarySuspensionService.cs`

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
* **Ringkasan Pekerjaan**: Penetapan skorsing investigasi dengan jaminan upah 100% Pasal 157A UU Ketenagakerjaan dan pembekuan seketika akun login SSO SIMRS, EMR, PACS serta smart card akses fisik.
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 23, dan matriks validasi Sub-modul 18.
* **Kriteria Penerimaan (Acceptance Criteria)**: Durasi skorsing maks 180 hari (VR-EMP-11); hak upah PaidFull 100% (VR-EMP-12); is_system_access_revoked = true & is_physical_access_revoked = true (VR-EMP-13).

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS**.
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1p (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`).
