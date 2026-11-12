# Laporan Penyelesaian Task: BE-HRD-273 Service Laporan Insiden Disiplin, Filter Just Culture RS & Integrasi KMKP (INT-HR-86)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-273`
* **Nama Task**: Service Laporan Insiden Disiplin, Filter Just Culture RS & Integrasi KMKP (INT-HR-86)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 18: Hubungan Industrial, Disiplin & Terminasi
* **Slice Delivery**: `S-C5` (Kontrak API `v22`, Gelombang `MVP-54`)
* **Status**: `✅ Selesai (2026-11-09)`
* **Deliverables**:
  - `IDisciplinaryIncidentService.cs`
  - `DisciplinaryIncidentService.cs`

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
* **Ringkasan Pekerjaan**: Pencatatan insiden pelanggaran tata tertib kerja dengan klasifikasi Just Culture RS (Human Error, At-Risk Behavior, Reckless Misconduct) dan integrasi audit klinis Sub-15.
* **Aturan Bisnis & Validasi**: Mematuhi kriteria penerimaan dan validasi yang ditetapkan pada blueprint arsitektur backend, kamus data Bagian 23, dan matriks validasi Sub-modul 18.
* **Kriteria Penerimaan (Acceptance Criteria)**: Laporan insiden klinis wajib menyertakan nomor audit KMKP (VR-EMP-02); insiden Human Error dilarang dijatuhi sanksi SP (VR-EMP-03).

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET Core**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Arsitektur QBE**: `Invoke-QbeConformanceCheck.ps1` berstatus **PASS**.
* **Traceability Requirement**: Terpetakan pada requirement matrix `requirement-traceability.md` Bagian 1p (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`).


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok E — nomor dokumen dari angka acak |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | ✅ **SELESAI 10 November 2026** (dikerjakan ulang) |

### Masalah yang ditemukan audit

Nomor laporan insiden dibentuk `INC-{yyyyMM}-{acak 4 digit}` memakai `Random.Shared`. Dua laporan pada bulan yang sama bisa bernomor sama dan urutannya tidak berurutan (melanggar `QBE-CODE-003`).

### Perubahan yang dikerjakan

- Nomor kini dialokasikan provider bersama `NumberSeriesAllocator` (deret `HRD_ER_INCIDENT`, reset bulanan, 4 digit). Bentuk tetap `INC-202611-0001`, `INC-202611-0002`, dan seterusnya.
- Aktor deret diturunkan dari email petugas lewat `IEmployeeRelationUnitOfWork.FindUserIdByEmailAsync`; bila email tidak dikenal, dipakai aktor sistem `00000000-0000-0000-0000-000000000001` (pola yang sudah dipakai `EmployeeBenefitEnrollmentService`).
- Index unik `IncidentNumber` sudah ada sebelumnya (`TrxDisciplinaryIncidentReportConfiguration`), sehingga `QBE-CODE-004` terpenuhi.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/DisciplinaryIncidentService.cs`
- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Constants/EmployeeRelationNumberSeries.cs` (baru)
- `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/IEmployeeRelationUnitOfWork.cs`, `EmployeeRelationUnitOfWork.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nomor laporan insiden unik dan berurutan per bulan | Terpenuhi | alokasi atomik `pg_advisory_xact_lock` di `NumberSeriesAllocator`; index unik sudah ada |

### Database dan migration

`NOT APPLICABLE` — tidak ada perubahan skema untuk task ini.

### Catatan penutup

- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
