# Laporan Penyelesaian Backend Task: BE-HRD-181

## 1. Identitas Task
* **ID Task**: `BE-HRD-181`
* **Nama Task**: Layanan pendaftaran mandiri tanggungan keluarga ESS berlampiran KK/Akta Nikah/Lahir & penegakan kuota maks 1 pasangan + 3 anak sah (`TrxEmployeeBenefitDependent`, `BEN-B2`, `VR-BEN-01`..`02`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-37: Administrasi Asuransi, Tanggungan Keluarga ESS & Verifikasi Berkas Fisik`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Rumah Sakit Quilvian memberikan fasilitas pertanggungan kesehatan dan subsidi perawatan bagi keluarga staf medis dan non-medis. Agar pemberian manfaat ini tepat sasaran dan mencegah pembengkakan beban operasional (*benefit fraud*), sistem Employee Self-Service (ESS) membatasi pendaftaran tanggungan hanya untuk **keluarga inti yang sah secara hukum**.

Sesuai aturan bisnis `BEN-B2` dan `HRD-DEC-135`:
1. **Pembatasan Anggota Keluarga Inti**:
   - Hanya hubungan **Pasangan Sah** (`Spouse`) dan **Anak Sah** (`Child`) yang dapat didaftarkan.
   - Pendaftaran anggota keluarga besar seperti orang tua kandung, mertua, keponakan, atau famili lainnya ditolak secara otomatis oleh sistem.
2. **Penegakan Kuota Pasangan Sah (`VR-BEN-01`)**:
   - Pegawai hanya berhak mendaftarkan maksimal **1 pasangan sah** yang berstatus aktif atau dalam proses verifikasi.
   - Pendaftaran pasangan kedua ditolak dengan pesan: *"Pendaftaran pasangan ditolak. Pegawai hanya berhak mendaftarkan maksimal 1 pasangan sah (VR-BEN-01, BEN-B2)."*
3. **Penegakan Kuota Anak Sah (`VR-BEN-02`)**:
   - Pegawai berhak mendaftarkan maksimal **3 anak kandung atau adopsi sah**.
   - Pendaftaran anak ke-4 ditolak otomatis dengan pesan: *"Pendaftaran anak ditolak. Kuota pertanggungan anak sah maksimal adalah 3 orang (VR-BEN-02, BEN-B2)."*
4. **Validasi Format NIK & Kartu Keluarga (`VR-BEN-03`, `VR-BEN-04`)**:
   - Nomor Induk Kependudukan (NIK) dan Nomor Kartu Keluarga (KK) wajib berupa string angka tepat 16 digit.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeBenefitDependent.cs`
* Penambahan kolom kependudukan dan status berkas:
  - `NationalIdNumber` (VARCHAR(16), NIK 16 digit).
  - `FamilyCardNumber` (VARCHAR(16), Nomor KK 16 digit).
  - `VerificationStatus` (VARCHAR(50), status `Draft`, `Submitted`, `Verified`, `Rejected`).
  - Indeks keunikan NIK per pegawai untuk mencegah pendaftaran ganda anggota keluarga yang sama.

### B. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitEnrollmentDtos.cs`
* `CreateEssDependentRequestDto`: Payload pendaftaran ESS memuat NIK, No KK, Nama, Hubungan (`Spouse`/`Child`), Tanggal Lahir, Gender, dan lampiran berkas.
* `EmployeeFamilyQuotaSummaryDto`: Rangkuman pemakaian kuota pasangan (maks 1) dan kuota anak (maks 3), indikator `CanAddSpouse` dan `CanAddChild`.
* `EmployeeDependentSummaryDto`: Ringkasan data tanggungan yang ditampilkan pada portal ESS pegawai.

### C. Logika Bisnis & Validasi Layanan
File: `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitEnrollmentService.cs`
* Metode: `CreateEssDependentAsync`, `GetMyFamilyDependentsAsync`, `GetMyFamilyQuotaSummaryAsync`.
* Mengunci transaksi dan memeriksa kuota sebelum menyimpan record baru dengan status `Submitted` untuk diverifikasi oleh HR Benefit Officer.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Perawat Mendaftarkan Suami dan 2 Anak Melalui ESS**:
  - Ns. Siti membuka portal ESS pada menu *My Family Benefits*.
  - Ringkasan kuota awal: Pasangan `0/1` (`CanAddSpouse = true`), Anak `0/3` (`CanAddChild = true`).
  - Siti mendaftarkan suaminya (Ahmad, NIK: 3201011234560001, No KK: 3201019876540001). Status tercatat `Submitted`.
  - Siti mendaftarkan anak ke-1 (Rizky, usia 5 th) dan anak ke-2 (Aisyah, usia 2 th).
  - Ringkasan kuota akhir: Pasangan `1/1` (`CanAddSpouse = false`), Anak `2/3` (`CanAddChild = true`).
  - Jika Siti mencoba mendaftarkan pasangan lain, sistem langsung memunculkan pesan penolakan `VR-BEN-01`.

* **Skenario 2 — Dokter Mencoba Mendaftarkan Anak ke-4**:
  - dr. Hendra Sp.A sudah memiliki 3 anak terdaftar di sistem (`Child 3/3`).
  - dr. Hendra mengajukan anak ke-4 melalui ESS.
  - Sistem menolak penyimpanan data dengan pesan: *"Pendaftaran anak ditolak. Kuota pertanggungan anak sah maksimal adalah 3 orang (VR-BEN-02, BEN-B2)."*

---

## 5. Verifikasi & Integritas Bukti
* **Pencegahan Data Duplikat**: Validasi NIK unik mencegah satu anggota keluarga terdaftar lebih dari satu kali.
* **Audit Trail**: Dicatat log audit `[AUDIT-BEN-01]` saat pendaftaran diajukan oleh pegawai.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-BEN-04` dan `AT-HRD-BEN-05`.
