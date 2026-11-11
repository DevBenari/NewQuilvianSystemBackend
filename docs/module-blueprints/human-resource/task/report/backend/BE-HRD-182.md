# Laporan Penyelesaian Backend Task: BE-HRD-182

## 1. Identitas Task
* **ID Task**: `BE-HRD-182`
* **Nama Task**: Layanan pendaftaran cepat BPJS TK H+0 Onboarding & aktivasi bertahap paket kesehatan pasca-lulus masa percobaan 3 bulan (`TrxEmployeeInsuranceEnrollment`, `BEN-B1`, `VR-BEN-11`..`13`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-37: Administrasi Asuransi, Tanggungan Keluarga ESS & Verifikasi Berkas Fisik`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Rumah sakit memiliki risiko keselamatan kerja yang sangat tinggi (paparan patogen menular, kecelakaan jarum suntik, dan kelelahan kerja intensif). Oleh karena itu, regulasi ketenagakerjaan dan standar akreditasi KARS mewajibkan perlindungan jaminan sosial tenaga medis diterapkan secara tegas dan bertahap:

1. **Aktivasi Cepat BPJS Ketenagakerjaan H+0 Onboarding (`BEN-B1`, `VR-BEN-11`)**:
   - Seluruh tenaga medis dan staf baru yang mulai bekerja wajib langsung didaftarkan pada program BPJS Ketenagakerjaan (khususnya Jaminan Kecelakaan Kerja / JKK dan Jaminan Kematian / JKM).
   - Tanggal efektif kepesertaan (`EffectiveStartDate`) wajib tepat sama dengan hari pertama dinas (`JoinDate` / H+0 Onboarding).
2. **Penahanan Paket Kesehatan Selama Masa Percobaan 3 Bulan (`BEN-B1`, `VR-BEN-12`)**:
   - Paket asuransi komersial tambahan dan subsidi rawat inap/jalan di RS Quilvian sendiri ditahan dengan status `PendingActivation` selama masa evaluasi kerja < 90 hari.
   - Hal ini melindungi rumah sakit dari biaya klaim elektif pegawai yang tidak menyelesaikan masa percobaannya.
3. **Aktivasi Bertahap Pasca-Lulus Masa Percobaan (`VR-BEN-13`)**:
   - Setelah pegawai resmi dinyatakan lulus evaluasi kinerja 3 bulan (`ProbationPassed`) dan terbit SK Pengangkatan, HR Benefit Officer mengaktifkan paket kesehatan lengkap melalui endpoint `POST /enrollments/{id}/activate-probation`.
   - Parameter `ProbationEvaluationId` wajib disertakan sebagai bukti kelulusan yang sah.
4. **Pencatatan Nomor Identitas Kepesertaan (`VR-BEN-09`, `VR-BEN-10`)**:
   - Nomor kartu BPJS Kesehatan divalidasi tepat 13 digit angka.
   - Nomor kartu BPJS Ketenagakerjaan (KPJ) divalidasi tepat 11 digit angka.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeInsuranceEnrollment.cs`
* Penambahan atribut:
  - `BpjsKesehatanNumber` (VARCHAR(50), 13 digit angka).
  - `BpjsKetenagakerjaanNumber` (VARCHAR(50), 11 digit angka).
  - `PrivateInsurancePolicyNumber` (VARCHAR(100), nomor polis asuransi komersial).
  - `ProbationEvaluationId` (UUID, rujukan ke SK evaluasi masa percobaan Sub-05).
  - `TerminationDate` & `TerminationReason` (data terminasi kepesertaan).
* Konfigurasi Fluent API: `Repositories/Configurations/Corporate/HumanResource/BenefitManagement/TrxEmployeeInsuranceEnrollmentConfiguration.cs`.

### B. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitEnrollmentDtos.cs`
* `EmployeeEnrollmentListQuery`: Filter pencarian daftar kepesertaan berdasarkan status aktivasi, unit kerja, dan kata kunci nama/nomor staf.
* `EmployeeEnrollmentListItemDto`: Menampilkan data staf, nomor kartu BPJS TK/Kes, nomor polis swasta, status aktivasi, dan indikator probation.
* `ActivateProbationRequestDto`: Payload aktivasi pasca-lulus masa percobaan.
* `UpdatePolicyNumbersRequestDto`: Payload pembaruan nomor kartu BPJS dan polis swasta.

### C. Logika Bisnis & Validasi Layanan
File: `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitEnrollmentService.cs`
* `GetEnrollmentsPagedAsync`: Mengambil daftar kepesertaan pegawai dengan pagination dan pencarian multi-kolom.
* `GetEnrollmentDetailAsync`: Menampilkan rincian paket, nomor polis, tanggal efektif, dan daftar tanggungan keluarga terverifikasi.
* `ActivateProbationEnrollmentAsync`: Mengubah status enrollment menjadi `Active`, mengisi tanggal efektif, dan merekam audit ID kelulusan.
* `UpdatePolicyNumbersAsync`: Memvalidasi regex 13 digit BPJS Kes (`VR-BEN-09`) dan 11 digit BPJS TK (`VR-BEN-10`).

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Onboarding Staf Laboratorium Baru**:
  - Mas Dennis bergabung sebagai Pranata Lab pada 1 November 2026.
  - Sistem mendaftarkan kepesertaan:
    * BPJS TK (JKK/JKM): Status **`Active`** (berlaku sejak H+0, 1 November 2026).
    * Asuransi Swasta Tambahan: Status **`PendingActivation`** (Masa Probation).
  - Jika terjadi insiden kecelakaan kerja tertusuk jarum di hari ke-10 dinas, perlindungan BPJS TK langsung menanggung seluruh biaya rawat darurat nakes tanpa hambatan.

* **Skenario 2 — Kelulusan Masa Percobaan 3 Bulan**:
  - Pada 1 Februari 2027, evaluasi kinerja 3 bulan Dennis selesai dengan hasil memuaskan (`ProbationPassed`, SK No. `SK-HRD-2027-0201`).
  - HR memanggil `POST /enrollments/{id}/activate-probation` dengan menyertakan ID evaluasi.
  - Status kepesertaan Dennis berubah menjadi **`Active`** penuh.
  - Dennis berhak menggunakan kamar rawat inap Kelas 1 dan subsidi berobat RS Quilvian.

---

## 5. Verifikasi & Integritas Bukti
* **Audit Trail**: Dicatat log audit `[AUDIT-BEN-03]` saat aktivasi paket pasca-probation dilakukan oleh HR.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-BEN-03` dan `AT-HRD-BEN-10`.
