# Laporan Perubahan Backend — `BE-IGD-079`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-079` |
| Judul | Dukungan Backend Tingkat Ketergantungan Pasien pada Form Permintaan Rawat Inap & Surat Pengantar |
| Slice | `EPIC IGD-15` · Vertical Slice B4 / Amandemen Analis |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18 |
| Requirement | `FR-IGD-121`; `AT-IGD-241` |
| Keputusan | `IGD-DEC-312` |
| Contract version | API **`0.19.0`** §12.3 & §12.7; validation **`0.18.0`** §14.2 aturan 11; data dictionary §11 |
| Dependency | `BE-IGD-074` ✅, `BE-IGD-077` ✅ |
| Klasifikasi | `MEDIUM` — penambahan kolom skema database, migrasi EF Core, enum baru, dan penguatan validasi sebelum tanda tangan DPJP |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): enum baru, model, EF configuration, DTOs, service, migration EF Core |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ **DONE — source selesai, migration diaplikasikan, build 0 error, QBE Strict PASS.** Kolom `PatientDependencyLevel` (enum `EmergencyPatientDependencyLevel`, integer nullable di database) berhasil ditambahkan pada tabel `EmgAdmissionRequest`. Validasi `RequestSignatureAsync` menegakkan kewajiban pengisian sebelum DPJP menandatangani dokumen. Nilai tingkat ketergantungan dibekukan pada snapshot JSON dan dikembalikan pada endpoint `/print-data` untuk dicetak pada dokumen fisik Surat Pengantar Rawat Inap. |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik dan prefix registry | IGD, prefix **`Emg`**, `ACTIVE / LEGACY`. |
| Keberlakuan | `TOUCHED LEGACY` + 1 berkas Enum baru `EmergencyPatientDependencyLevel.cs` |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001` |
| `Program.cs` | Nol baris perubahan. |
| Migration | `20261016064300_AddPatientDependencyLevelToAdmissionRequest` diaplikasikan ke database PostgreSQL. |

---

## 1. Latar Belakang & Kebutuhan Bisnis

Atas masukan analis pengguna rumah sakit pada alur transfer pasien rawat inap dari IGD:
1. **Kesiapan Bangsal Rawat Inap:** Sebelum pasien dipindahkan ke bangsal rawat inap tujuan, perawat bangsal dan petugas admisi perlu mengetahui derajat bantuan perawatan yang dibutuhkan pasien (tingkat ketergantungan / *Patient Dependency Level*).
2. **Kategori Ketergantungan:**
   - **Low Dependency (Ketergantungan Rendah / Minimal Care):** Pasien mampu mandiri untuk sebagian besar aktivitas harian (makan, minum, eliminasi), hanya memerlukan observasi tanda vital berkala dan pengobatan oral.
   - **Moderate Dependency (Ketergantungan Sedang / Partial Care):** Pasien membutuhkan bantuan parsial dari perawat untuk mobilitas, eliminasi, atau memiliki infus intravena/oksigenasi kontinyu.
   - **High Dependency (Ketergantungan Tinggi / Total Care):** Pasien bedridden, tidak sadar / delirium, memerlukan pemantauan ketat, pengawasan jalan napas, atau persiapan ruang intensif (ICU/HCU).
3. **Penyelesaian Teknis Backend:**
   - Menyediakan enum resmi `EmergencyPatientDependencyLevel` (1 = Low, 2 = Moderate, 3 = High).
   - Menambahkan kolom `PatientDependencyLevel` pada tabel `EmgAdmissionRequest` melalui migrasi EF Core resmi.
   - Mengizinkan draf baru tersimpan tanpa nilai awal (`null`) agar perawat sadar melakukan asesmen klinis terhadap pasien.
   - Mencegah pengajuan tanda tangan DPJP IGD (`RequestSignatureAsync`) bila tingkat ketergantungan belum dipilih (HTTP 400 Bad Request: *"Lengkapi unit layanan, kelas perawatan, DPJP usulan, estimasi lama rawat, tingkat ketergantungan pasien, dan indikasi klinis sebelum meminta tanda tangan."*).
   - Membekukan nilai dan label deskriptifnya (`PatientDependencyLevelName`) ke dalam `SnapshotJson` saat ditandatangani.
   - Mengembalikan nilai pada endpoint `GET /print-data` untuk dicetak pada dokumen fisik Surat Pengantar Rawat Inap.

---

## 2. Rincian Berkas yang Berubah

| Berkas | Perubahan Teknis |
| :--- | :--- |
| `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyPatientDependencyLevel.cs` | Enum baru: `Low = 1`, `Moderate = 2`, `High = 3`. |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgAdmissionRequest.cs` | Penambahan properti `public EmergencyPatientDependencyLevel? PatientDependencyLevel { get; set; }`. |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgAdmissionRequestConfiguration.cs` | Konfigurasi EF Core: `builder.Property(x => x.PatientDependencyLevel).HasConversion<int>().IsRequired(false);`. |
| `Migrations/20261016064300_AddPatientDependencyLevelToAdmissionRequest.cs` | Migration EF Core menambahkan kolom `PatientDependencyLevel` integer nullable pada tabel `public."EmgAdmissionRequest"`. |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyAdmissionRequestDtos.cs` | Penambahan `PatientDependencyLevel` dan `PatientDependencyLevelName` pada DTO `UpdateEmergencyAdmissionRequestRequest`, `EmergencyAdmissionRequestSummary`, `EmergencyAdmissionRequestResponse`, `EmergencyAdmissionRequestSnapshot`, dan `EmergencyAdmissionRequestPrintResponse`. |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.AdmissionRequest.cs` | 1. Pembaruan konstanta `PesanIsianBelumLengkap`.<br>2. Pemetaan `request.PatientDependencyLevel` pada penyimpanan draf `PerbaruiPermintaanRawatInapAsync`.<br>3. Validasi kewajiban `form.PatientDependencyLevel.HasValue` pada `MintaTandaTanganAsync` dan `TandatanganiAsync`.<br>4. Pembekuan ke `SnapshotJson` pada `BentukSalinanBekuAsync`.<br>5. Pemetaan respons detail pada `BentukResponsAsync` dan respons cetak pada `AmbilDataCetakPermintaanAsync`.<br>6. Proyeksi kueri ringkasan pada `ProyeksiRingkasan`. |

---

## 3. Spesifikasi Endpoint Bergaya Swagger

### `[Tags("Health Services / Emergency Installation Management / Emergency Admission Request")]`

| Method | Path | Deskripsi | Auth / Role | Request Payload | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `PUT` | `/api/v1/health-services/emergency-installation-management/emergency-admission-requests/{id}` | Memperbarui isian draf Form Permintaan Rawat Inap, termasuk tingkat ketergantungan pasien | `EmergencyAdmissionRequest:Update` (Perawat IGD) | `UpdateEmergencyAdmissionRequestRequest` (`patientDependencyLevel: 1/2/3`) | `ApiResponse<EmergencyAdmissionRequestResponse>` |
| `POST` | `/api/v1/health-services/emergency-installation-management/emergency-admission-requests/{id}/request-signature` | Mengajukan permohonan tanda tangan DPJP IGD (wajib memvalidasi tingkat ketergantungan telah dipilih) | `EmergencyAdmissionRequest:Update` (Perawat IGD) | `EmergencyAdmissionRequestRowVersionRequest` | `ApiResponse<EmergencyAdmissionRequestResponse>` |
| `GET` | `/api/v1/health-services/emergency-installation-management/emergency-admission-requests/{id}/print-data` | Mengambil data cetak Surat Pengantar Rawat Inap fisik termasuk tingkat ketergantungan dari salinan beku | `EmergencyAdmissionRequest:Read` (Staf Medis / Admisi) | — | `ApiResponse<EmergencyAdmissionRequestPrintResponse>` |

---

## 4. Bukti Verifikasi

### 4.1 Kompilasi Backend C#
```powershell
dotnet build -p:RunAnalyzers=false
```
Hasil:
```text
    286 Warning(s)
    0 Error(s)
Time Elapsed 00:01:58.76
```

### 4.2 Penerapan Migrasi Basis Data PostgreSQL
```powershell
dotnet ef database update
```
Hasil:
```text
Applying migration '20261016064300_AddPatientDependencyLevelToAdmissionRequest'.
Done.
```

### 4.3 Evaluasi QBE Strict Conformance
```powershell
powershell -ExecutionPolicy Bypass -File tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict
```
Hasil:
```text
QBE Conformance Report
Checker mode: Strict
Scope: WorkingTree
Files evaluated: 11
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
Final result: PASS
```

---

## 5. Kesimpulan & Penandaan Status Task

Task **`BE-IGD-079`** telah selesai secara penuh dengan seluruh acceptance criteria terpenuhi (1–7). Backend ASP.NET Core siap mendukung integrasi antarmuka frontend pada task pasangannya **`FE-IGD-066`**.
