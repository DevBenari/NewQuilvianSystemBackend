# Laporan Perubahan Backend — `BE-RWI-101`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-101` |
| Judul | Rekonsiliasi obat bawaan (migration R5) |
| Slice | Gelombang 2 — `DOK-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-101` |
| Trace | `FR-DOK-092`, `FR-DOK-093`; `RWI-DEC-132`, `RWI-DEC-133`, `RWI-DEC-134`; `VAL-DOK-52`, `52a`, `53`, `53a`, `53b`; state matrix 0.6.0 bagian 8.5; api-contract 12.8 dan 12.9; `INT-DOK-17`; kamus data 13.2 dan 13.3 |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-099` ✅ |
| Klasifikasi | `HEAVY` — dua tabel baru, controller baru, pengisian draft resep, jalur non-formularium pada `MasterData` |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`, `Areas/HealthServices/MasterData`, `Repositories`, `Migrations`, `Program.cs`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 16 September 2026 |
| Status | ✅ Selesai berdasarkan validasi statis source dan skema; `dotnet build` dan eksekusi migration **NOT RUN** atas instruksi pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement`; `HealthServices / MasterData` (jalur non-formularium) |
| Registry/prefix | `Phm` `ACTIVE`; entity baru `PhmMedicationReconciliationItem`, `PhmMedicationReconciliationDecision` memakai prefix pemilik yang terdaftar (`QBE-NAM-002`); tabel, DbSet, configuration sepaket |
| Keberlakuan | `NEW CODE` (model, configuration, service, controller); `TOUCHED LEGACY` (`DrugController`) |
| QBE relevan | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-NAM-001/002`, `QBE-MOD-001/002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` |
| Hak akses baru | Resource `MedicationReconciliation` : `Read`, `Create`, `Update`, **`Decide`** |
| Persetujuan pemilik | Muhammad Hamzah (`PharmacyManagement`, `MasterData`) — `RWI-DEC-062`, `RWI-DEC-132`, `RWI-DEC-134` |
| Database | Migration `20260916005000_AddMedicationReconciliation` dibuat, **tidak dijalankan** |

## 1. Masalah yang diperbaiki

Backend V2 tidak punya penyimpan obat yang dibawa pasien dari rumah maupun keputusan dokter atasnya
(`RWI-FACT-032`). Obat jantung yang terlewat keputusannya bisa fatal, dan keputusan yang ditimpa di
tempat (pola V1) menghapus riwayat terapi.

## 2. Proses bisnis

1. Senin 14.20 Ns. Siti mencatat Amlodipin 10 mg 1×1 oral dan Metformin 500 mg 3×1 oral milik Budi →
   `POST /medication-reconciliations` dua kali → `201`. `DrugId` wajib dari master obat; perawat wajib
   bertugas di unit episode; status `Pending`.
2. Obat yang belum ada di master obat didaftarkan dulu lewat
   `POST /master-data/drugs/non-formulary-registrations` oleh pemegang `Drug : Create`; server
   **selalu** menulis `IsFormulary = false`.
3. 15.00 dr. Ahmad (penugasan aktif) memutuskan:
   - Amlodipin `ContinueSame` → butir **draft** resep terbentuk (obat, dosis, frekuensi disalin; penanda
     formularium dari master obat). Draft yang dipakai: draft yang disebut dokter, atau draft resep
     rawat inap milik dokter itu pada episode, atau draft baru pada catatan dokternya yang masih terbuka.
   - Metformin `ContinueModified` → butir draft terbentuk untuk diubah aturan pakainya di layar resep.
   - Vitamin `Stopped` → tidak ada butir.
4. Sebelum resep diselesaikan dr. Ahmad mengganti Metformin menjadi `Stopped` → baris keputusan nomor 2
   menunjuk keputusan nomor 1, dan butir draft Metformin dikeluarkan dari draft **dalam transaksi yang
   sama**. Riwayat keputusan lama tetap tersimpan (`GET /{id}/decisions`).
5. **Jalur tidak normal:**
   - Ns. Siti mencoba `POST /{id}/decisions` → `403` "Keputusan per obat hanya dapat diambil dokter yang
     merawat pasien ini."
   - Resep Amlodipin sudah diselesaikan, lalu keputusan diganti → `409` "Resep dari keputusan sebelumnya
     sudah aktif. Ubah terapi lewat resep atau hentikan obatnya."
   - Perawat membatalkan baris yang sudah diputuskan → `409` VAL-DOK-53a.
   - Tidak ada catatan dokter terbuka untuk menampung draft baru → `422` beserta arahan membuat catatan
     dokter lebih dulu; keputusan tidak tersimpan (`INT-DOK-17`: tanpa butir draft tidak ada keputusan
     "Lanjut").
   - Perawatan ditutup → `422`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 13.2, 13.3, 13.13; arsitektur 0.5 bagian 11.4–11.8; `PrescriptionController.cs`,
`PrescriptionItemController.cs`, `PrescriptionAggregateService.cs`, `PrescriptionNumberService.cs`,
`PrescriptionSummaryService.cs`, `PrescriptionWorkflowService.cs`, `PrescriptionValidationService.cs`,
`DrugController.cs`, `DrugDtos.cs`, `MstDrug.cs`, `NursingActorService.cs`,
`InpatientClinicalContextService.cs`, registry prefix.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Enums/ReconciliationDecisionType.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Enums/HomeMedicationRoute.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Models/PhmMedicationReconciliationItem.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Models/PhmMedicationReconciliationDecision.cs` | Baru |
| `Repositories/Configurations/HealthServices/PharmacyManagement/MedicationReconciliationConfigurations.cs` | Baru — dua configuration; unique parsial `IdempotencyKey`; unique `(ReconciliationItemId, SequenceNumber)` |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` |
| `Migrations/20260916005000_AddMedicationReconciliation.cs` | Baru — R5; `Down` ditolak bila sudah ada obat bawaan tercatat |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Dua blok entity dan dua blok relasi |
| `Areas/HealthServices/PharmacyManagement/DTOs/MedicationReconciliationDtos.cs` | Baru |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationReconciliationService.cs` | Baru — catat, batal, putuskan/ganti keputusan, isi draft resep |
| `Areas/HealthServices/PharmacyManagement/Controllers/MedicationReconciliationController.cs` | Baru |
| `Areas/HealthServices/MasterData/Controllers/DrugController.cs`, `DTOs/DrugDtos.cs` | Endpoint `POST /non-formulary-registrations` + `CreateNonFormularyDrugRequest` |
| `Program.cs` | Registrasi `MedicationReconciliationService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru Medication Reconciliation (5 endpoint) dan satu endpoint Drug; tambahan murni |
| Database | R5: dua tabel baru kosong; migration dibuat, **tidak dijalankan** |
| Keamanan/Auth | Resource baru `MedicationReconciliation` dengan aksi `Decide` terpisah dari `Create`; seluruh pasangan `[AccessAction]`/`[AccessPermission]` cocok; nol hak akses baru untuk `Drug`. `Note` dan `DecisionNote` sensitif, tidak masuk payload logger |

## 4. Dokumentasi endpoint

#### Health Services / Pharmacy Management / Medication Reconciliation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Obat bawaan satu episode beserta keputusan terakhir | `MedicationReconciliation : Read` |
| `POST` | `/` | Perawat mencatat obat bawaan (`201`; kiriman ulang berkunci sama `200`) | `MedicationReconciliation : Create` |
| `PATCH` | `/{id}/cancel` | Membatalkan baris salah catat sebelum ada keputusan; alasan wajib | `MedicationReconciliation : Update` |
| `POST` | `/{id}/decisions` | Dokter memutuskan atau mengganti keputusan | `MedicationReconciliation : Decide` |
| `GET` | `/{id}/decisions` | Riwayat keputusan satu obat | `MedicationReconciliation : Read` |

#### Health Services / Master Data / Drug

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/non-formulary-registrations` | Mendaftarkan obat bawaan sebagai non-formularium; `IsFormulary` dipaksa `false` | `Drug : Create` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Kesesuaian dua tabel dengan kamus data 13.2, 13.3, DDL 13.13 | Kolom, tipe, bawaan, index, FK sama | `PASS` | Model, configuration, migration, snapshot |
| Nama constraint ≤ 63 karakter mengikuti pemotongan EF Core | Seluruhnya sesuai | `PASS` | Perhitungan nama sesi ini |
| Snapshot hanya bertambah | Nol baris terhapus | `PASS` | `git diff --diff-algorithm=histogram` |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang | Nol temuan pada kode task | `PASS` | Skrip analisis statis sesi ini |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik |
| Verifikasi skema pada Postgres sekali pakai | Tidak dijalankan | `NOT RUN` | Migration belum dieksekusi |
| Verifikasi kontrak API dan proses bisnis keenam kriteria (runtime) | Tidak dijalankan | `NOT RUN` | Menunggu build |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tabel rekonsiliasi beserta status `Pending`, `ContinueSame`, `ContinueModified`, `Stopped` | Terpenuhi (statis) | Dua model + enum `ReconciliationDecisionType` + migration R5 |
| 2. "Lanjut" membentuk butir draft, bukan resep aktif | Terpenuhi (statis) | `ResolveTargetDraftAsync` hanya memakai/membuat resep `Draft`; `AddDraftItemAsync` |
| 3. Perawat memutuskan → `403` | Terpenuhi (statis) | `DecideAsync`: dokter tertaut akun + penugasan aktif; hak akses `Decide` terpisah |
| 4. Keputusan dapat diganti selama butir hasil masih draft; riwayat tersimpan | Terpenuhi (statis) | Baris keputusan baru dengan `SupersedesDecisionId`; butir draft dikeluarkan pada ganti ke `Stopped` |
| 5. Setelah resep aktif, penggantian ditolak `409` | Terpenuhi (statis) | `IsPrescriptionDraftAsync` → VAL-DOK-52a |
| 6. Obat non-formularium dapat didaftarkan | Terpenuhi (statis) | `DrugController.RegisterNonFormularyDrug` |
| `dotnet build`, migration | Belum diverifikasi runtime | `NOT RUN` atas instruksi pemilik |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kamus data 13.2 tidak menyediakan kolom alasan pembatalan baris obat bawaan; alasan tetap wajib dan dicatat pada jejak audit logger (`AuditAsync`), tanpa menambah kolom di luar kamus data |
| Masalah yang diketahui | (a) Draft resep baru hanya dapat dibuat pada catatan dokter yang masih terbuka, karena `PhmPrescription.ConsultationId` wajib dan konsultasi bayangan dilarang (arsitektur 11.10) — delta terhadap kalimat kontrak "membuat draft baru bila belum ada". (b) Temuan di luar scope: `PrescriptionController.CreatePrescription` menulis `FulfillmentStatus = WaitingForPayment` pada draft, padahal validasi finalisasi menuntut `WaitingForClinicalFinalization`; tidak diubah, draft dari rekonsiliasi memakai `WaitingForClinicalFinalization` |
| Risiko tersisa | Obat non-formularium yang didaftarkan tanpa ketiga satuan tersimpan `IsPrescribable = false` mengikuti aturan master obat yang berlaku |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` (11); berkas lain `M` |
| Langkah berikutnya | `dotnet build`; jalankan migration R5; beri hak `MedicationReconciliation` di Akses Role (perawat tanpa `Decide`); uji keenam kriteria dengan akun non-SuperAdmin |
