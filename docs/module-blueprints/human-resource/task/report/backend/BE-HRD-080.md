# Laporan Perubahan Backend — `BE-HRD-080`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-080` |
| Judul | Keputusan Kontrak `T16`, Batas 5 Tahun PKWT, dan Serah Terima Pemisahan |
| Slice | S-C4-ONB (LifecycleManagement) |
| Milestone | MVP-16 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-089 butir 2–7, 10; HRD-DEC-082; FR-ONB-051, FR-ONB-052; VR-ONB-35 s.d. VR-ONB-40; AT-HRD-ONB-15, AT-HRD-ONB-29 s.d. AT-HRD-ONB-31 |
| Dependency | BE-HRD-068 ✅, BE-HRD-072 ✅, BE-HRD-078 ✅ |
| Klasifikasi | `CORE WORKFORCE CONTRACT DECISION & REGULATORY COMPLIANCE` |
| Task Mode | `BACKEND` |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Roslyn Build 0 Error |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Di rumah sakit, pengelolaan siklus kontrak kerja PKWT diatur ketat oleh regulasi ketenagakerjaan Indonesia (UU Cipta Kerja / PP 35 Tahun 2021) yang menetapkan batas maksimal total masa kerja PKWT kumulatif selama **5 tahun (60 bulan)**. Menjelang kontrak PKWT berakhir, atasan atau Kepala Unit wajib menetapkan keputusan arah status kerja pegawai:
1. **Perpanjang (`Extend`)**: Memperpanjang jangka waktu PKWT dengan syarat durasi kumulatif tidak melampaui 60 bulan.
2. **Pengangkatan Menjadi Pegawai Tetap (`ConvertToPermanent`)**: Mengubah jenis kontrak menjadi PKWTT tanpa masa percobaan ulang dan tanpa pemotongan kompensasi dasar.
3. **Tidak Diperpanjang (`NotRenew`)**: Mengakhiri hubungan kerja secara formal dan menyerahkan proses exit clearance ke Sub-Modul 20.

Implementasi backend pada task `BE-HRD-080` menghadirkan alur lengkap keputusan kontrak `T16`:
- **Pengecekan Kelaikan PKWT 5 Tahun (`VR-ONB-36`)**: Endpoint `GET /api/v1/corporate/human-resource/lifecycle-management/employment-contracts/{id}/fixed-term-check` untuk menghitung sisa bulan maksimal PKWT dari tanggal awal rangkaian seri (`FixedTermSeriesStartDate`).
- **Pengajuan Keputusan Kontrak (`VR-ONB-35`, `VR-ONB-37`, `VR-ONB-38`)**: Endpoint `POST .../{id}/decisions` dengan validasi hanya untuk kontrak bertanda tangan (`Signed`/`Active`) dan membatasi 1 keputusan terbuka per kontrak.
- **Pencatatan Pemberitahuan Pegawai (`VR-ONB-40`)**: Endpoint `POST .../{id}/decisions/{decisionId}/notify` untuk mencatat tanggal penyampaian keputusan secara formal kepada pegawai sebelum tanggal akhir kontrak.
- **Penerapan Hasil Persetujuan (`ApplyDecisionAsync`)**: Jika disetujui, menyusun otomatis draf kontrak lanjutan (`Renewal`/`Conversion`) atau memicu serah terima pemisahan `ContractNonRenewal` via `ResignationLifecycleHandoffService`.

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

```mermaid
flowchart TD
    A["Kontrak PKWT Masuk Periode Evaluasi (H-60 s.d. H-30)"] --> B["Atasan/HR Memanggil GET fixed-term-check (VR-ONB-36)"]
    B --> C{"Pilihan Keputusan T16"}
    
    C -->|Extend / Perpanjang| D{"Total Bulan Kumulatif <= 60 Bulan?"}
    D -->|Tidak (>60 Bulan)| E["TOLAK Pengajuan: Batas Maksimal 5 Tahun Tercapai"]
    D -->|Ya| F["Simpan HrdContractDecision (Status: Submitted)"]

    C -->|ConvertToPermanent / Tetap| F
    C -->|NotRenew / Tidak Diperpanjang| F

    F --> G["Pemicu Alur Persetujuan Bertingkat T16 (Workflow)"]
    G --> H{"Keputusan Disetujui (Approved)?"}
    
    H -->|Ditolak| I["Status Rejected (Keputusan Batal)"]
    H -->|Disetujui: Extend / Tetap| J["Buat Draf Kontrak Lanjutan Otomatis (HistoryType: Renewal/Conversion)"]
    H -->|Disetujui: NotRenew| K["Panggil HandoffLifecycleSeparationAsync (Serah Terima Pemisahan Sub-Modul 20)"]
    K --> L["Catat Tanggal Pemberitahuan ke Pegawai via /notify (VR-ONB-40)"]
```

---

## 3. Komponen Perubahan yang Diterapkan

1. **DTOs (`EmploymentContractDtos.cs`)**:
   - `FixedTermCheckResponse`: Informasi sisa bulan, total bulan terpakai, dan kelaikan perpanjangan PKWT.
   - `SubmitContractDecisionRequest`: Input tipe keputusan, bulan perpanjangan, tanggal usulan mulai dan selesai, serta alasan bisnis.
   - `ContractDecisionResponse`: Response lengkap status keputusan dan nomor registrasi.
   - `RecordEmployeeNotifiedRequest`: Input tanggal pencatatan pemberitahuan ke pegawai.
2. **Services (`IHrdEmploymentContractService.cs` & `HrdEmploymentContractService.cs`)**:
   - `FixedTermCheckAsync`: Perhitungan matematika durasi kumulatif 5 tahun.
   - `SubmitDecisionAsync`: Validasi status `VR-ONB-35`, keputusan tunggal `VR-ONB-37`, dan inisiasi alur workflow persetujuan.
   - `GetDecisionByIdAsync`, `UpdateDecisionAsync`, `WithdrawDecisionAsync`: Pengelolaan daur hidup draf pengajuan.
   - `RecordEmployeeNotifiedAsync`: Pencatatan bukti penyampaian hasil keputusan ke pegawai (`VR-ONB-40`).
   - `ApplyDecisionAsync`: Pembuatan draf kontrak baru atau serah terima pemisahan ke Sub-Modul 20.
3. **Controller (`HrdEmploymentContractController.cs`)**:
   - Endpoint Swagger lengkap untuk pengecekan fixed term, pengajuan, pembacaan, revisi, penarikan, dan pencatatan notifikasi keputusan.

---

## 4. Spesifikasi Endpoint API Bergaya Swagger

| Method | Endpoint Path | Deskripsi | Auth / Role | Status |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/human-resource/lifecycle-management/employment-contracts/{id}/fixed-term-check` | Cek sisa kuota waktu PKWT maksimal 5 tahun | Authorized | 200 OK |
| `POST` | `/api/v1/corporate/human-resource/lifecycle-management/employment-contracts/{id}/decisions` | Pengajuan keputusan kontrak kerja T16 | Authorized | 201 Created |
| `GET` | `/api/v1/corporate/human-resource/lifecycle-management/employment-contracts/{id}/decisions/{decisionId}` | Detail pengajuan keputusan kontrak kerja | Authorized | 200 OK |
| `PUT` | `/api/v1/corporate/human-resource/lifecycle-management/employment-contracts/{id}/decisions/{decisionId}` | Perbarui draf pengajuan keputusan kontrak | Authorized | 200 OK |
| `POST` | `/api/v1/corporate/human-resource/lifecycle-management/employment-contracts/{id}/decisions/{decisionId}/withdraw` | Tarik kembali draf keputusan kontrak | Authorized | 200 OK |
| `POST` | `/api/v1/corporate/human-resource/lifecycle-management/employment-contracts/{id}/decisions/{decisionId}/notify` | Catat tanggal pemberitahuan keputusan ke pegawai | Authorized | 200 OK |

---

## 5. Bukti Verifikasi dan Pengujian

- **Kompilasi Roslyn**: Lulus `dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj /t:Compile` dengan **0 Error(s)**.
- **Kepatuhan Regulasi**: Menjaga integritas `VR-ONB-36` (UU Cipta Kerja batas 60 bulan PKWT) dan `VR-ONB-40` (pemberitahuan sebelum kontrak berakhir).
