# Laporan Perubahan Backend — `BE-MDF-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-009` |
| Judul | Adapter sumber layanan |
| Slice | `MVP-2` (`EPIC MDF-04`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MDF-DES-011`, `MF-DEC-003`, `015`; `MDF-INTEGRATION-1.0` §2; `MF-CAP-002`, `005`, `006` |
| Contract version | `MDF-INTEGRATION-1.0`, `MDF-API-1.0`, `MDF-VAL-1.0`, seluruhnya terkunci 20 September 2026 |
| Dependency | `BE-MDF-008` — 🟡 sebagian: migration kalkulasi dibuat pada source; `BE-MDF-006` — 🟡 sebagian: kesepakatan tarif dibuat pada source |
| Klasifikasi | `MEDIUM` — Service adapter abstraksi multi-sumber, DTO normalisasi `AdaptedServiceItem`, pemetaan tim operasi penuh, pemetaan pelaksana tunggal klinis dan laboratorium, pengabaian radiologi, serta registrasi DI di `Program.cs` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Commit backend saat dikerjakan | `548785ac` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | `SEBAGIAN` — implementasi kode DTO, adapter service, dan registrasi DI selesai lengkap memenuhi seluruh acceptance criteria dan invariant integrasi, namun `dotnet build` dan uji runtime `T-25`, `T-38`, `T-39` dilewati per instruksi batasan lingkungan pemilik |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `FeeCalculation` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE) pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` |
| Status registry | `ACTIVE` |
| QBE ID yang berlaku | `QBE-SVC-001` (Service layer memisahkan akses DbContext dan logika adaptasi sumber), `QBE-NAM-001` (Penamaan konsisten), `QBE-MOD-001` |
| QBE ID yang tidak berlaku | `QBE-DB-*` (Tidak ada migrasi database), `QBE-PERM-*` (Adapter internal, dipanggil oleh service kalkulasi) |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Transaksi / Integrasi — Adapter abstraksi baca-saja (*read-only*) untuk normalisasi sumber tindakan medis lintas modul rumah sakit |

---

## 1. Masalah yang diperbaiki

Sebelum task ini dikerjakan, sistem Quilvian belum memiliki jembatan adapter untuk membaca dan menormalisasi tindakan medis dari modul-modul pelayanan rumah sakit ke dalam format yang dapat diproses oleh mesin kalkulasi jasa medis (`MedicalFeeCalculationService`).

Setiap modul pelayanan di rumah sakit memiliki struktur data pelaksana yang sangat berbeda:
1. **Kamar Operasi (`OperatingRoomManagement`)**: Menggunakan struktur tim (`OprTeamMember`) di mana satu tindakan operasi dapat melibatkan beberapa dokter dan perawat dengan peran berbeda (Operator Utama, Asisten, Anestesi, Perawat Instrumen).
2. **Poliklinik & Rawat Inap (`ClinicalManagement`)**: Menggunakan model `TrxPatientProcedure` dengan pelaksana tunggal (`DoctorId`) tanpa peran eksplisit.
3. **Laboratorium (`LaboratoryManagement`)**: Menggunakan `LabOrder` dengan dokter pemeriksa opsional (`ExaminerDoctorId`) yang seringkali belum terisi saat pesanan dibuat.
4. **Radiologi (`RadiologyManagement`)**: Tidak mencatat dokter pelaksana sama sekali (`MF-DEC-015`).
5. **Billing (`BillingManagement`)**: Menyimpan nilai kotor tagihan sebelum diskon pada `BilInvoiceItem`.

Tanpa kelas adapter ini, logika penarikan data yang rumit dan heterogen tersebut akan tercecer ke dalam service kalkulasi jasa medis, memicu *tight coupling*, duplikasi kode, dan rawan kesalahan saat ada perubahan skema pada salah satu modul sumber.

Contoh konkret skenario rumah sakit:
Sebuah tindakan operasi *Laparoskopi Apendiktomi* pada tanggal 12 Oktober 2026 dilaksanakan oleh tim yang terdiri dari **Dr. Budi, Sp.B** (Operator Utama), **Dr. Citra** (Asisten Operator), dan **Dr. Dodi, Sp.An** (Dokter Anestesi). Tagihan kotor operasi di Billing adalah Rp 15.000.000 (sebelum diskon). `MedicalFeeServiceSourceAdapter` mengisolasi kasus ini dan otomatis menghasilkan **tiga item layanan terpisah**:
- Item 1: Dr. Budi, Peran: Operator Utama (`PrimarySurgeon`), Nilai Dasar: Rp 15.000.000.
- Item 2: Dr. Citra, Peran: Asisten Bedah (`AssistantSurgeon`), Nilai Dasar: Rp 15.000.000.
- Item 3: Dr. Dodi, Peran: Anestesi (`Anesthesiologist`), Nilai Dasar: Rp 15.000.000.
Dengan demikian, mesin kalkulasi jasa medis dapat langsung memproses hak masing-masing dokter tanpa perlu mengetahui detail internal tabel kamar operasi.

---

## 2. Proses bisnis & Invarian Integrasi

| Aspek | Isi |
| --- | --- |
| Tujuan | Menyediakan lapisan adapter terpadu untuk mengekstrak dan menormalisasi data tindakan medis dari berbagai modul sumber ke dalam DTO `AdaptedServiceItem` |
| Pelaku | Dipanggil oleh mesin kalkulasi jasa medis (`MedicalFeeCalculationService`) saat proses kalkulasi periode dijalankan |
| Sifat Operasi | **Baca saja (*Read-Only*)** — tidak melakukan transaksi penulisan ke modul sumber maupun ke database |

Invarian dan aturan integrasi yang ditegakkan (`MDF-INTEGRATION-1.0` §2):

1. **Dukungan Tim Kamar Operasi Penuh (`OPERATING_ROOM`, Acceptance Criteria `T-25`)**:
   - Satu kasus operasi dengan $N$ anggota tim yang aktif menghasilkan tepat $N$ baris rincian layanan mandiri.
   - Peran kamar operasi (`OprTeamRole`) dipetakan ke data induk peran jasa medis (`MstMedicalFeeRole`) melalui kolom `OprTeamRoleMapping`. Jika peran belum dipetakan, ditandai `ROLE_UNMAPPED`.
   - `SourceDetailId` diformat konsisten: `"{OprCaseId}/{OprTeamMemberId}"`.
2. **Pelaksana Tunggal Prosedur Klinis (`PROCEDURE`)**:
   - Pelaksana diambil dari `TrxPatientProcedure.DoctorId`.
   - Peran diasumsikan sebagai peran utama rumah sakit (`MstMedicalFeeRole.IsPrimaryRole == true`).
   - Tindakan operasi yang terkait dengan pembedahan kamar operasi (`IsSurgeryRelated == true`) dilewati agar tidak terjadi perhitungan ganda dengan modul Kamar Operasi.
   - `SourceDetailId` diformat: `"{TrxPatientProcedureId}"`.
3. **Pelaksana Tunggal Laboratorium & Deteksi Data Tertahan (`LABORATORY`, Acceptance Criteria `T-39`)**:
   - Pelaksana diambil dari `LabOrder.ExaminerDoctorId`.
   - Peran diasumsikan sebagai peran utama (`IsPrimaryRole == true`).
   - Bila `ExaminerDoctorId` kosong/null, adapter menandai item tersebut sebagai `PERFORMER_MISSING` dengan alasan detail `"Pesanan laboratorium {OrderNumber} belum memiliki dokter pemeriksa (ExaminerDoctorId)"`.
   - `SourceDetailId` diformat: `"{LabOrderId}"`.
4. **Pengabaian Radiologi Sepenuhnya (Acceptance Criteria `T-38`, `MF-DEC-015`)**:
   - Modul Radiologi tidak diproses sama sekali oleh adapter: tidak ditarik, tidak dihitung, dan tidak dicatat ke daftar belum terhitung (`MdfUnresolvedService`).
5. **Invarian Nilai Kotor Baris Tagihan (`MDF-DES-012`, `MF-DEC-013`, `T-27`, `T-28`)**:
   - `BaseAmount` selalu diambil dari `Quantity × UnitPrice` baris tagihan `BilInvoiceItem` pada faktur yang berstatus `FINAL` atau `CLOSED`.
   - Diskon promo dan diskon dokter **tidak dikurangkan** oleh Medical Fee.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` baris 77 | Memastikan acceptance criteria dan batasan task `BE-MDF-009` |
| `docs/module-blueprints/medical-fee/02-backend-architecture.md` § 4.10 | Spesifikasi fungsi `MedicalFeeServiceSourceAdapter` dan isolasi multi-sumber |
| `docs/module-blueprints/medical-fee/contracts/integration-contract.md` § 2 | Spesifikasi teknis pemetaan sumber Kamar Operasi, Klinis, Lab, Radiologi, dan Billing |
| `docs/module-blueprints/medical-fee/testing/acceptance-test-matrix.md` | Memeriksa kriteria uji `T-25` (operasi tim 3 orang), `T-38` (radiologi diabaikan), dan `T-39` (lab tanpa dokter pemeriksa) |
| `Areas/HealthServices/OperatingRoomManagement/Models/*.cs` | Memeriksa model `OprCase`, `OprTeamMember`, dan enum `OprTeamRole` |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` | Memeriksa model tindakan klinis dan dokter pelaksana |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | Memeriksa model pesanan laboratorium dan `ExaminerDoctorId` |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs` | Memeriksa kaitan tagihan dan penghitungan nilai kotor |
| `Program.cs` | Pendaftaran dependency injection service |

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/DTOs/MedicalFeeSourceAdapterDtos.cs` | **Baru.** DTO `AdaptedServiceItem` yang menormalisasi identitas tindakan, domain sumber, nilai kotor sebelum diskon, pelaksana, peran, dan status unresolved |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Services/MedicalFeeServiceSourceAdapter.cs` | **Baru.** Lapisan service adapter yang membaca data dari `OprCase`/`OprTeamMember`, `TrxPatientProcedure`, `LabOrder`, serta mencocokkannya ke `BilInvoiceItem` Billing |
| `Program.cs` | **Diperbarui.** Menambahkan using `FeeCalculation.Services` dan mendaftarkan `builder.Services.AddScoped<MedicalFeeServiceSourceAdapter>();` di bawah seksi `MEDICAL FEE MANAGEMENT` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak mempublikasikan endpoint HTTP publik; service ini merupakan komponen internal backend |
| Database | Murni operasi pembacaan (*read-only*), nol mutasi basis data, nol dampak skema tabel |
| Keamanan/Auth | Tidak memerlukan hak akses kontroler; mewarisi otorisasi service pemanggil (`MedicalFeeCalculationService`) |

---

## 4. Dokumentasi Arsitektur Adapter & Struktur Data

### Diagram Alur Normalisasi Multi-Sumber

```mermaid
flowchart LR
    subgraph Sumber Layanan
        OK["Kamar Operasi<br/>(OprCase & OprTeamMember)"]
        KL["Klinis & Rawat Inap<br/>(TrxPatientProcedure)"]
        LB["Laboratorium<br/>(LabOrder)"]
        RD["Radiologi<br/>(Diabaikan Penuh)"]
    end

    subgraph Adapter
        AD["MedicalFeeServiceSourceAdapter"]
        BIL["Pencocokan Billing<br/>(BilInvoiceItem: Qty x Price)"]
        ROL["Pemetaan Peran<br/>(MstMedicalFeeRole)"]
    end

    subgraph Normalisasi
        OUT["AdaptedServiceItem<br/>• Pelaksana (Doctor/Workforce)<br/>• Peran Jasa Medis<br/>• BaseAmount Kotor<br/>• Status Unresolved"]
    end

    OK -->|Tim Penuh: 1 Orang = 1 Item| AD
    KL -->|Pelaksana Tunggal| AD
    LB -->|Pelaksana Tunggal| AD
    RD -.->|Diabaikan (MF-DEC-015)| AD
    AD <--> BIL
    AD <--> ROL
    AD --> OUT
```

### Spesifikasi DTO `AdaptedServiceItem`

| Properti | Tipe Data | Deskripsi |
| :--- | :--- | :--- |
| `SourceDomain` | `string` | Domain modul sumber: `OPERATING_ROOM`, `PROCEDURE`, `LABORATORY`, `ADHOC`, `ADHOC_CATALOG` |
| `SourceDetailId` | `string` | ID rincian unik pada modul sumber (contoh: `"{CaseId}/{MemberId}"` untuk operasi) |
| `InvoiceItemId` | `Guid?` | ID baris tagihan pada `BilInvoiceItem` yang sudah final |
| `TariffId` | `Guid?` | ID tarif katalog tindakan (bila ada) |
| `TariffCategoryId` | `Guid?` | ID kategori tarif (bila ada) |
| `ServiceDescription` | `string` | Nama tindakan atau deskripsi layanan medis |
| `ServiceDate` | `DateOnly` | Tanggal pelaksanaan tindakan medis |
| `BaseAmount` | `decimal` | Nilai kotor baris tagihan (`Quantity × UnitPrice`), sebelum diskon |
| `PayeeType` | `string` | Jenis penerima: `Doctor` atau `Workforce` |
| `PayeeReferenceId` | `Guid?` | ID dokter (`MstDoctor.Id`) atau staf (`MstWorkforceProfile.Id`), null jika belum diinput |
| `PayeeName` | `string` | Nama dokter/staf pelaksana untuk tampilan audit |
| `RoleId` | `Guid?` | ID peran pada `MstMedicalFeeRole`, null jika peran belum terpetakan |
| `RoleCode` | `string?` | Kode peran jasa medis |
| `IsPrimaryRole` | `bool` | Menandakan apakah peran tersebut merupakan peran utama |
| `IsPerformerMissing` | `bool` | True jika pelaksana tindakan belum diinput pada modul sumber |
| `IsRoleUnmapped` | `bool` | True jika peran kamar operasi belum terpetakan di `MstMedicalFeeRole` |
| `UnresolvedReason` | `string?` | Kode alasan tertahan (`PERFORMER_MISSING`, `ROLE_UNMAPPED`, dsb.) |
| `UnresolvedReasonDetail`| `string?` | Keterangan detail untuk ditindaklanjuti pengelola jasa |

---

## 5. Verifikasi

| Skenario atau pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik: **tanpa `dotnet build` otomatis** |
| Checker `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan | `NOT RUN` | `pwsh` tidak terpasang pada lingkungan eksekusi |
| Review diff dan cakupan perubahan | Tepat 2 berkas baru dan 1 berkas `Program.cs` dimodifikasi | `PASS` | `git status --short` |
| Verifikasi Operasi Bertim (Skenario `T-25`) | 1 `OprCase` dengan $N$ `OprTeamMember` menghasilkan $N$ item terpisah dengan `SourceDetailId = "{case.Id}/{member.Id}"` | `PASS` | `MedicalFeeServiceSourceAdapter.cs` metode `ExtractOperatingRoomServicesAsync` |
| Verifikasi Radiologi Diabaikan (Skenario `T-38`) | Radiologi tidak diproses, tidak diekstrak, dan tidak dicatat ke unresolved | `PASS` | `MedicalFeeServiceSourceAdapter.cs` tidak memanggil modul Radiologi per `MF-DEC-015` |
| Verifikasi LabOrder Tanpa Pemeriksa (Skenario `T-39`) | `LabOrder.ExaminerDoctorId == null` menghasilkan `UnresolvedReason = PERFORMER_MISSING` | `PASS` | `MedicalFeeServiceSourceAdapter.cs` metode `ExtractLaboratoryServicesAsync` |
| Verifikasi Nilai Kotor Sebelum Diskon (Skenario `T-27`, `T-28`) | Menggunakan `Quantity * UnitPrice` dari `BilInvoiceItem` murni tanpa pengurangan diskon | `PASS` | `MedicalFeeServiceSourceAdapter.cs` baris perhitungan `baseAmount` |
| Pendaftaran Dependency Injection | `MedicalFeeServiceSourceAdapter` terdaftar sebagai scoped di `Program.cs` | `PASS` | `Program.cs` di bawah blok `MEDICAL FEE MANAGEMENT` |
| Pemeriksaan karakter baris (Line endings) | LF murni, UTF-8 tanpa BOM, nol karakter `\r` | `PASS` | `grep -c $'\r'` bernilai 0 pada seluruh berkas |
| Skenario uji runtime `T-25`, `T-38`, `T-39` | Tidak dijalankan | `NOT RUN` | Menuntut kompilasi runtime dan data uji aktif di basis data |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Acceptance: "Operasi bertim tiga orang → tiga rincian" | Terpetakan pada source | `ExtractOperatingRoomServicesAsync` memetakan setiap `OprTeamMember` menjadi satu `AdaptedServiceItem` dengan `SourceDetailId = "{case.Id}/{member.Id}"` |
| Acceptance: "Radiologi diabaikan sepenuhnya" | Terpetakan pada source | Layanan radiologi tidak diproses dan tidak dicatat ke unresolved per `MF-DEC-015` |
| Acceptance: "LabOrder tanpa ExaminerDoctorId ditandai PERFORMER_MISSING" | Terpetakan pada source | `ExtractLaboratoryServicesAsync` memeriksa `ExaminerDoctorId` dan mengisi `UnresolvedReason = PERFORMER_MISSING` bila kosong |
| DoD: "Perbedaan antar sumber terisolasi di satu berkas" | Terpenuhi | Seluruh logika pemetaan Kamar Operasi, Klinis, Lab, dan Billing terpusat di `MedicalFeeServiceSourceAdapter.cs` |
| Pendaftaran Dependency Injection | Terpenuhi | `builder.Services.AddScoped<MedicalFeeServiceSourceAdapter>()` terdaftar pada `Program.cs` |
| Validasi build | **Belum terpenuhi** | `NOT RUN` atas instruksi eksplisit pemilik (tanpa build otomatis) |

Catatan: Status task ditandai sebagai `🟡 SEBAGIAN` karena implementasi kode telah lengkap dan memenuhi acceptance criteria pada source, namun kompilasi `dotnet build` dan verifikasi skenario uji `T-25`, `T-38`, `T-39` belum dijalankan sesuai instruksi batasan lingkungan.

---

## 7. Catatan penutup dan Langkah Selanjutnya

1. **Kesiapan Mesin Perhitungan**:
   Dengan tersedianya `MedicalFeeServiceSourceAdapter`, mesin perhitungan jasa medis (`BE-MDF-010`) kini dapat mengonsumsi data tindakan terstandarisasi untuk mengalokasikan porsi tarif sharing dan menulis rincian jasa.
2. **Task Berikutnya**:
   - **`BE-MDF-010`**: Mesin perhitungan periode (`MedicalFeeCalculationService`, `MedicalFeePeriodService` — menghitung pembagian persentase tarif kotor, snapshot aturan, dan penulisan hasil).
   - **`BE-MDF-011`**: Daftar layanan belum dapat dihitung (`MedicalFeeUnresolvedServicesController` beserta rekap per alasan).
   - **`BE-MDF-018`**: Entity penyerahan ke Finance (`MdfFinanceHandoff`).

