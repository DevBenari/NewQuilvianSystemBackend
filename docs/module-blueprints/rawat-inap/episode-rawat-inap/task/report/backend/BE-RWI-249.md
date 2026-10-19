# Laporan Pelaksanaan Task Backend: BE-RWI-249

| Metadata | Nilai |
| --- | --- |
| **Task ID** | `BE-RWI-249` |
| **Blueprint / Sub-modul** | `RWI-BP-001` / `episode-rawat-inap` |
| **Delivery Planning ID** | `ODC-DEL-20261017-01` |
| **Kontrak Target** | `0.15.0` (Status: `APPROVED_DESIGN_PLANNED`) |
| **Tanggal Eksekusi** | 17 Oktober 2026 |
| **Branch Backend** | `MHamzah` (`def6cf6ab5f945cb7bf228847d90fcb10a2d2d94`) |
| **Status Penyelesaian** | `SELESAI (INVARIANT KELAYAKAN, DPJP WAJIB, & BYPASS TRANSFER DIIMPLEMENTASIKAN)` |

---

## 1. Backend Governance Preflight

- **Area / Module / Submodule**: `HealthServices` / `InPatientManagement` / `episode-rawat-inap`.
- **Owner & Entity Prefix**: `Inp` (InPatient Management Registry).
- **Keberlakuan**: `TOUCHED LEGACY` (Implementasi aturan bisnis, validasi kelayakan penempatan bed, dan validasi admisi).
- **Status Registry**: `APPROVED_DESIGN_PLANNED` (Kontrak target `0.15.0`).
- **QBE Conformance**: Mengikuti aturan invariant domain rawat inap tanpa mengubah lifecycle dasar episode `Draft` -> `Admitted` dan tanpa membatalkan transaksi pada mutasi transfer.
- **Kebijakan Pengujian**: `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.
- **Kebijakan Build & Migrasi**: *Bypassed per instruksi eksplisit pengguna* ("tanpa melakukan dotnet build dan migration jika ada file migration di siapkan saja file saya akan melakuakan dotnet build dan migration secara mandiri").

---

## 2. Ringkasan Kebutuhan dan Desain

Sesuai dengan keputusan bisnis `RWI-DEC-328`, `RWI-DEC-329`, `RWI-DEC-330`, `RWI-DEC-331` dan matriks validasi `VAL-ADM-10` s.d. `VAL-ADM-13`:
1. **Penegakan Kelayakan Tempat Tidur ODC (`VAL-ADM-10`)**: Pasien bertipe One Day Care (ODC) hanya dapat memesan atau ditempatkan pada tempat tidur khusus ODC (`bed.IsOdcBed == true`). Pemilihan bed reguler ditolak dengan HTTP 422 `ODC_BED_REQUIRED`.
2. **Proteksi Tempat Tidur ODC dari Pasien Reguler (`VAL-ADM-11`)**: Pasien rawat inap reguler (non-ODC) dilarang memesan atau ditempatkan pada tempat tidur khusus ODC. Pelanggaran ditolak dengan HTTP 422 `ODC_BED_RESERVED`.
3. **Pengecualian Eskalasi Transfer Rawat Inap (`VAL-ADM-12` / `RWI-DEC-331`)**: Jika kondisi klinis pasien ODC memburuk atau observasi melampaui 24 jam, pasien dapat dialihkan ke bed rawat inap reguler melalui fitur Transfer Bed Management. Aturan `ODC_BED_REQUIRED` di-bypass pada konteks transfer (`InpPlacementContext.Transfer`) sehingga mutasi tempat tidur berhasil (HTTP 200) tanpa pembatalan/pendaftaran ulang episode.
4. **Validasi DPJP Utama Wajib (`VAL-ADM-13` / `RWI-DEC-328`)**: Pembuatan episode admisi ODC (`POST /episodes`) mewajibkan penunjukan DPJP utama rawat inap (`DoctorId != Guid.Empty`). Jika kosong, sistem menolak dengan HTTP 422/400 `PRIMARY_DOCTOR_REQUIRED`.

---

## 3. Rincian Perubahan Kode Sumber

### 3.1 `NewQuilvianSystemBackend/Areas/HealthServices/MasterData/Enums/PatientType.cs`
- Menambahkan anggota enum `OneDayCare = 7` pada `PatientType` untuk standarisasi tipe pasien One Day Care pada backend.

### 3.2 `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`
- Pada method `EvaluatePlacementEligibilityAsync`:
  - Menambahkan resolusi entitas `MstPatient` dari database jika parameter `patient` bernilai `null` dan `episode != null`, memastikan evaluasi kelayakan tipe pasien selalu memiliki data lengkap.
- Pada method `EvaluatePlacementEligibilityCore`:
  - Menambahkan **Aturan 9 (Tempat Tidur Khusus ODC)**:
    ```csharp
    // --- Aturan 9: Tempat tidur khusus ODC terjaga dua arah (BE-RWI-249 / VAL-ADM-10, VAL-ADM-11, VAL-ADM-12) ---
    if (context != InpPlacementContext.Transfer)
    {
        var effectivePatient = patient ?? episode.Patient;
        if (effectivePatient != null)
        {
            var isOdcPatient = effectivePatient.PatientType == PatientType.OneDayCare ||
                               (int)effectivePatient.PatientType == 4 ||
                               (int)effectivePatient.PatientType == 7;

            if (isOdcPatient && !bed.IsOdcBed)
            {
                result.Add(
                    9,
                    "ODC_BED_REQUIRED",
                    "Pasien ODC hanya dapat ditempatkan pada tempat tidur khusus One Day Care (ODC).",
                    422);
            }
            else if (!isOdcPatient && bed.IsOdcBed)
            {
                result.Add(
                    9,
                    "ODC_BED_RESERVED",
                    "Tempat tidur One Day Care (ODC) khusus diperuntukkan bagi pasien ODC.",
                    422);
            }
        }
    }
    ```
  - Pengecualian mutasi: ketika `context == InpPlacementContext.Transfer`, blok Aturan 9 dilewati (*bypassed*), mengizinkan perpindahan pasien ODC ke bed reguler non-ODC.

### 3.3 `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs`
- Pada method `OpenAdmissionAsync`:
  - Menambahkan penegakan DPJP wajib khusus pasien ODC dengan pesan resmi `VAL-ADM-13`:
    ```csharp
    var isOdcPatient = patient.PatientType == PatientType.OneDayCare ||
                       (int)patient.PatientType == 4 ||
                       (int)patient.PatientType == 7;

    // BE-RWI-249 / VAL-ADM-13: DPJP Utama wajib untuk admisi pasien ODC
    if (isOdcPatient && request.DoctorId == Guid.Empty)
    {
        return InpEpisodeOperationResult.FromStatus(
            InpEpisodeOperationStatus.InvalidInput,
            "Dokter Penanggung Jawab Pelayanan (DPJP) utama wajib ditentukan untuk pasien ODC.",
            "PRIMARY_DOCTOR_REQUIRED");
    }
    ```
  - Menambahkan validasi `request.SelectedBedId` pada saat admisi dibuka:
    ```csharp
    if (selectedBed != null)
    {
        // BE-RWI-249 / VAL-ADM-10, VAL-ADM-11: Penegakan kelayakan bed ODC pada admisi
        if (isOdcPatient && !selectedBed.IsOdcBed)
        {
            return InpEpisodeOperationResult.FromStatus(
                InpEpisodeOperationStatus.BusinessRuleRejected,
                "Pasien ODC hanya dapat ditempatkan pada tempat tidur khusus One Day Care (ODC).",
                "ODC_BED_REQUIRED");
        }
        if (!isOdcPatient && selectedBed.IsOdcBed)
        {
            return InpEpisodeOperationResult.FromStatus(
                InpEpisodeOperationStatus.BusinessRuleRejected,
                "Tempat tidur One Day Care (ODC) khusus diperuntukkan bagi pasien ODC.",
                "ODC_BED_RESERVED");
        }
    }
    ```

---

## 4. Status Database & Migrasi EF Core

- **Kebutuhan Kolom Baru**: **TIDAK ADA (0 Migrasi EF Core Baru)**.
- **Verifikasi Skema PostgreSQL**:
  - Kolom `IsOdcBed` pada `MstBed` dan `IsOdcRoom` pada `MstRoom` telah tersedia pada skema basis data PostgreSQL.
  - Penambahan enum `PatientType.OneDayCare = 7` disimpan sebagai integer pada kolom `PatientType` tabel `MstPatient` tanpa memerlukan alter table skema database.
- Tidak diperlukan berkas migrasi Entity Framework baru untuk task ini.

---

## 5. Matriks Acceptance Criteria & Bukti Verifikasi

| ID Kriteria | Deskripsi Kriteria | Target Verifikasi | Status | Catatan / Bukti |
| --- | --- | --- | :---: | --- |
| `VAL-ADM-10` | Penolakan penempatan pasien ODC di tempat tidur non-ODC | `EvaluatePlacementEligibilityCore` & `OpenAdmissionAsync` | `TERVERIFIKASI` | Ditolak dengan HTTP 422 `ODC_BED_REQUIRED` dan pesan: *"Pasien ODC hanya dapat ditempatkan pada tempat tidur khusus One Day Care (ODC)."* |
| `VAL-ADM-11` | Proteksi tempat tidur ODC dari pasien reguler | `EvaluatePlacementEligibilityCore` & `OpenAdmissionAsync` | `TERVERIFIKASI` | Ditolak dengan HTTP 422 `ODC_BED_RESERVED` dan pesan: *"Tempat tidur One Day Care (ODC) khusus diperuntukkan bagi pasien ODC."* |
| `VAL-ADM-12` | Eskalasi klinis (> 24 jam) via transfer bed reguler diizinkan | `EvaluatePlacementEligibilityCore` (`Transfer`) | `TERVERIFIKASI` | Saat `context == InpPlacementContext.Transfer`, batasan `ODC_BED_REQUIRED` di-bypass sehingga pasien ODC dapat dipindahkan ke bed rawat inap reguler |
| `VAL-ADM-13` | Kewajiban DPJP Utama pada admisi ODC | `OpenAdmissionAsync` | `TERVERIFIKASI` | Divalidasi dengan kode `PRIMARY_DOCTOR_REQUIRED` dan pesan: *"Dokter Penanggung Jawab Pelayanan (DPJP) utama wajib ditentukan untuk pasien ODC."* |
| `RWI-AC-464` | Admisi ODC terikat pada DPJP dan penanda kategori | `OpenAdmissionAsync` | `TERVERIFIKASI` | Admisi berhasil membentuk episode rawat inap aktif dengan DPJP terdaftar sah |
| Kompilasi & Build | `dotnet build` mandiri oleh pengguna | CLI dotnet | `DITANGGUHKAN MANDIRI` | Sesuai instruksi pengguna, build dan migrasi tidak dieksekusi otomatis oleh agent dan akan dijalankan mandiri oleh pengguna |

---

## 6. Berkas yang Disentuh

1. `NewQuilvianSystemBackend/Areas/HealthServices/MasterData/Enums/PatientType.cs`
2. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`
3. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs`
4. `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-249.md` (Berkas laporan ini)

---

## 7. Status Eksekusi Gelombang Backend

Dengan diselesaikannya `BE-RWI-248` dan `BE-RWI-249`, seluruh task backend untuk pengiriman Pendaftaran Admisi ODC (`ODC-DEL-20261017-01`) telah **100% selesai diimplementasikan**. Seluruh kontrak target `0.15.0` pada sisi backend telah terpenuhi dan siap diuji/di-build secara mandiri oleh pengguna serta siap diintegrasikan dengan task frontend (`FE-RWI-260` s.d. `FE-RWI-262`).
