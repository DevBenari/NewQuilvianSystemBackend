# Laporan Perubahan Backend — `BE-RWI-117`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-117` |
| Judul | Dugaan reaksi obat sebagai alergi tertaut (migration K6) |
| Slice | Gelombang 4 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-117` |
| Trace | `FR-KEP-070`; `INT-KEP-13`; `VAL-KEP-35a`/`b`; `AC-MVP-029`; kamus data 11.3; api-contract 7.10 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-115` ✅ 17 September 2026 |
| Klasifikasi | `MEDIUM` — dua kolom tabel legacy berisi data, penulisan lintas modul baca |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, `Repositories/`, `Migrations/`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — empat kriteria terpetakan; migration K6 **ditulis, tidak dijalankan**; `dotnet build` **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; tabel legacy `TrxPatientAllergy` (bukan rename) |
| Keberlakuan | `TOUCHED LEGACY` (controller, tabel) + `NEW CODE` (`AdverseDrugReactionService`) |
| QBE relevan | `QBE-SVC-001` (endpoint baru tanpa `DbContext` di controller), `QBE-CFG-002`, `QBE-CODE-006`, `QBE-VAL-001`, `QBE-LOG-001` |
| Nomor bisnis | `AllergyRecordNumber` jalur ini dari `NumberSeriesAllocator` deret `CLI_PATIENT_ALLERGY_ADR`, awalan `ADR` — **tidak** memakai `Count+1` legacy `ALG-…` sehingga kedua deret tidak bertabrakan |
| Database | `20260917104000_AddAllergyMedicationAdministrationLink` (K6); `Down` menolak bila kolom berisi data; belum diterapkan |

## 1. Masalah yang diperbaiki

Reaksi yang dilihat perawat setelah pemberian obat tidak tercatat di tempat yang akan terbaca saat obat itu diresepkan
lagi. Mencatatnya di MAR mengubah rekam pemberian obat; mencatatnya sebagai alergi terkonfirmasi melangkahi dokter.

## 2. Proses bisnis

1. Ceftriaxone Budi `Administered` 08.05. Pukul 08.25 Ns. Siti melihat ruam gatal di dada.
2. `POST patient-allergies/from-medication-administration` `{ medicationAdministrationId, reactionDescription, severity, reactionOnsetAt }`.
3. Lahir satu `TrxPatientAllergy`: kategori `Drug`, `DrugId` dosis, nama obat dari butir resep, kepastian `Suspected`,
   belum diverifikasi, `SourceMedicationAdministrationId` dan `InpEpisodeId` terisi, peringatan alergi aktif.
4. Baris MAR **tidak disentuh** — dosis dibaca dan dikunci selama transaksi agar dua kiriman bersamaan tidak melahirkan
   dua dugaan, tetapi status dan isinya tetap.
5. Dokter memverifikasi lewat `PATCH patient-allergies/{id}/verify` yang sudah ada.
6. **Jalur tidak normal:** deskripsi kosong → `400`; dosis belum `Administered` → `409` "Dugaan reaksi hanya dicatat untuk
   obat yang sudah diberikan."; perawatan ditutup → `422`; unit lain → `403`; kiriman ulang atas dosis yang sama →
   `200` alergi yang sama; pasien sudah punya alergi hidup untuk obat yang sama → `409` (aturan duplikat yang sama dengan
   pencatatan alergi manual).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`TrxPatientAllergy.cs`, `TrxPatientAllergyConfiguration.cs`, `PatientAllergyController.cs` (validasi duplikat, penomoran,
pemetaan respons), integration contract 8.7, api-contract 7.10.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAllergy.cs` | `InpEpisodeId`, `SourceMedicationAdministrationId` |
| `Repositories/Configurations/HealthServices/TrxPatientAllergyConfiguration.cs` | Dua FK `Restrict` dan dua index bernama |
| `Migrations/20260917104000_AddAllergyMedicationAdministrationLink.cs`, snapshot | K6 |
| `Areas/HealthServices/ClinicalManagement/DTOs/AdverseDrugReactionDtos.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Services/AdverseDrugReactionService.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAllergyController.cs` | Endpoint baru mendelegasikan ke service; injeksi service |
| `Program.cs` | Registrasi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif sesuai 7.10. Delta: `Idempotency-Key` tidak disimpan karena `TrxPatientAllergy` tidak punya kolomnya; kiriman ulang dikenali dari dosis yang sama |
| Database | Dua kolom nullable pada tabel legacy |
| Keamanan/Auth | `PatientAllergy : Create` yang sudah ada; `ReactionDescription` dan `ClinicalNote` tidak masuk payload logger |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Allergy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/from-medication-administration` | Dugaan reaksi obat dari satu dosis MAR | `PatientAllergy : Create` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source: nol penulisan `PhmMedicationAdministration` di service | Dosis dibaca `AsNoTracking` | `PASS` | `AdverseDrugReactionService` |
| `dotnet build`, verifikasi skema | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |
| Verifikasi proses bisnis MAR tidak berubah | Tidak dijalankan | `NOT RUN` | Menunggu build dan database |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Dua kolom baru `TrxPatientAllergy` | Terpenuhi | Model, konfigurasi, K6 |
| 2. Dugaan dicatat dari dosis dan tertaut | Terpenuhi | `SourceMedicationAdministrationId = dosis.Id` |
| 3. Tidak mengubah status/isi dosis MAR | Terpenuhi | Dosis `AsNoTracking`; nol perubahan terlacak |
| 4. Alergi lahir `Suspected` | Terpenuhi | `Certainty = Suspected`, `IsVerified = false` |
| DoD: `dotnet build`, verifikasi skema dan proses bisnis | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Penerima notifikasi aktif (gate `G-15`) tidak dibangun; dugaan terbaca lewat peringatan alergi aktif dan saringan `certainty=Suspected&isVerified=false` yang sudah ada |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Build dan migration belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` model, konfigurasi, controller; berkas baru `??` |
| Langkah berikutnya | Frontend tombol "Laporkan dugaan reaksi" |
