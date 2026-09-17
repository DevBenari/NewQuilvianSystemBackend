# Laporan Perubahan Backend — `BE-RWI-098`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-098` |
| Judul | Verifikasi instruksi pesanan tindakan |
| Slice | Gelombang 3 — `DOK-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-098` |
| Trace | `FR-DOK-103`, `FR-DOK-104`; `INV-DOK-17`; `RWI-DEC-139`; `VAL-DOK-50`, `VAL-DOK-50a`; state matrix 0.6.0 bagian 8.3; api-contract 12.5 |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-097` ✅ (dengan perbaikan kompilasi pada task ini — bagian 1) |
| Klasifikasi | `MEDIUM` — dua endpoint baru, satu penjaga pelaksanaan, dan perbaikan service fondasi |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 16 September 2026 |
| Status | ✅ Selesai; validasi statis source. **`dotnet build` PASS 17 September 2026** (bagian 8); uji runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / ClinicalManagement` |
| Registry/prefix | `Cli` `ACTIVE / LEGACY`; tabel `TrxPatientProcedure` legacy, nol kolom baru pada task ini |
| Keberlakuan | `NEW CODE` pada service; `TOUCHED LEGACY` pada `PatientProcedureController` |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-DTO-001` |
| Hak akses baru | `PatientProcedure : Verify` (`AccessType = Update`) — permission-audit-matrix 0.6.0 bagian 6.1 |
| Database | Nol migration |

## 1. Masalah yang diperbaiki

Pesanan tindakan yang dimasukkan perawat (lahir di `BE-RWI-097`) berstatus instruksi `Pending`,
tetapi belum ada jalur bagi dokter pemberi instruksi untuk mengkonfirmasinya, dan belum ada daftar
tunggunya. Selain itu **`PatientProcedureOrderService` hasil `BE-RWI-097` tidak akan terkompilasi**:

| Temuan pada kode `BE-RWI-097` | Akibat | Perbaikan |
| --- | --- | --- |
| Memakai `ILoggerService` yang tidak ada di repository | Galat kompilasi `CS0246` | Diganti `LoggerService` (`QuilvianSystemBackend.Services.Logging`) |
| `ResolveActorDoctorIdAsync(actorUserId, ct)` tanpa argumen `ClaimsPrincipal` (service dan `PatientProcedureController.CancelProcedure`) | Galat kompilasi `CS1503` | Memakai tanda tangan sebenarnya `(user, actorUserId, ct)` |
| Membaca `MstProcedure.Price` yang tidak ada | Galat kompilasi `CS1061` | Harga dari `InsuranceCoverageService.ResolveProcedureAsync`, sumber tarif yang sama dengan jalur tindakan poliklinik |
| `PatientProcedureController.UpdateProcedure` memakai `InpEpisode`/`InpEpisodeStatus` tanpa `using` | Galat kompilasi `CS0246` | `using` `InPatientManagement.Models` dan `.Enums` ditambahkan |
| Pesanan baru dijawab `200`, bukan `201` | Menyimpang dari kontrak 12.5 | Factory `PatientProcedureOrderResult.Created` |
| Perawat pemesan tidak diperiksa bertugas di unit episode | Menyimpang dari state matrix 8.3 (`RWI-DEC-100`) | Pemeriksaan `NursingActorService` + `IsNurseOnDutyAtEpisodeAsync` |

## 2. Proses bisnis

1. 23.00 Ns. Siti memesan cek GDS untuk Budi atas instruksi telepon dr. Ahmad → pesanan `Ordered`,
   instruksi `Pending`, penginput Ns. Siti.
2. 07.30 dr. Rina (DPJP) menekan Verifikasi → `403` "Hanya dokter pemberi instruksi yang dapat
   memverifikasi pesanan ini."
3. 08.00 dr. Ahmad membuka daftar tunggunya (`GET /instruction-verification-worklist`), lalu menekan
   Verifikasi → `Verified`, `InstructionVerifiedAt` dan `InstructionVerifiedByUserId` terisi.
   Penginput, dokter pesanan, tindakan, jumlah, catatan, dan status pesanan **tidak berubah**.
4. 08.01 dr. Ahmad menekan lagi → `409` "Pesanan ini sudah diverifikasi atau tidak memerlukan
   verifikasi." Pesanan yang dibuat dokter sendiri (`NotRequired`) juga dijawab `409`.
5. Verifikasi tidak bergantung status pesanan dan tidak menuntut penugasan aktif: pemberi instruksi
   yang penugasannya sudah berakhir tetap dapat memverifikasi (permission-audit-matrix 8.6).
6. **Pelaksanaan (kriteria 4).** Ns. Rina menandai tindakan dilaksanakan → akun login menjadi
   `PerformedByUserId` dan penanda tangan registrasi keutuhan `Procedure`; mesin addendum hanya
   mengenali penulis asli itu (atau pengganti sah `RWI-DEC-088`). Pada tindakan rawat inap, pelaksana
   wajib berwenang: dokter lewat penugasan aktif, perawat lewat unit episode; episode tertutup `422`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientProcedureOrderService.cs`, `PatientProcedureController.cs`, `PatientProcedureDtos.cs`,
`TrxPatientProcedure.cs`, `InpatientClinicalContextService.cs`, `NursingActorService.cs`,
`InsuranceCoverageService.cs`, `ClinicalNoteAddendumService.cs`, `LoggerService.cs`,
`PatientIntegratedProgressNoteController.cs` (pola `Verify` dan daftar tunggu CPPT).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs` | Ditulis ulang: perbaikan kompilasi `BE-RWI-097`; `VerifyInstructionAsync` (VAL-DOK-50/50a); `GetInstructionVerificationWorklistAsync`; `EnsureInpatientExecutorAsync`; `Created` (201) |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | Endpoint `PATCH /{id}/verify-instruction` dan `GET /instruction-verification-worklist`; penjaga pelaksana pada `PATCH /{id}/execute`; perbaikan `ResolveActorDoctorIdAsync` dan `using` |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs` | DTO `InstructionVerificationItemResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru; `POST /inpatient-orders` kini `201` untuk pesanan baru; `PATCH /{id}/execute` rawat inap menolak pelaksana tidak berwenang (`403`) dan episode tertutup (`422`) |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Hak akses baru `PatientProcedure : Verify`; `[AccessAction("Verify")]` dan `[AccessPermission("PatientProcedure","Verify")]` cocok huruf demi huruf; pembatasan pemberi instruksi dari data pesanan, bukan dari nama peran |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/verify-instruction` | Dokter pemberi instruksi memverifikasi pesanan perawat | `PatientProcedure : Verify` |
| `GET` | `/instruction-verification-worklist` | Pesanan `Pending` milik dokter login, berhalaman | `PatientProcedure : Read` |
| `PATCH` | `/{id}/execute` | Menandai dilaksanakan; pelaksana dari akun login (perilaku rawat inap diperketat) | `PatientProcedure : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–5 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang | Nol temuan pada kode task | `PASS` | Skrip analisis statis sesi ini |
| Verifikasi anggota tipe yang dipakai (`MstProcedure`, `InsuranceCoverageResult`, `PatientProcedureResponse`, `InpEpisode`) | Seluruh properti ada | `PASS` | Pembacaan model/DTO |
| `dotnet build` (perintah ringan pemilik: `-m:1`, tanpa shared compilation, tanpa analyzer) | Build ke-5 pada 17 September 2026: **0 error, 212 warning** | `PASS` | Bagian 8 |
| Verifikasi kontrak API dan proses bisnis runtime | Tidak dijalankan | `NOT RUN` | Menunggu build |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Hanya dokter pemberi instruksi yang memverifikasi | Terpenuhi (statis) | `VerifyInstructionAsync`: dokter akun login wajib sama dengan `InstructingDoctorId` |
| 2. Verifikasi tidak mengubah penginput maupun isi | Terpenuhi (statis) | Hanya `InstructionVerification*` dan jejak audit ubah yang ditulis |
| 3. `NotRequired` → `Pending` → `Verified` | Terpenuhi (statis) | `CreateInpatientOrderAsync` menulis `NotRequired`/`Pending`; verifikasi hanya dari `Pending` |
| 4. Pelaksana menjadi penulis catatan pelaksanaan; addendum hanya oleh pelaksana | Terpenuhi (statis) | `ExecuteProcedure`: `PerformedByUserId` = akun login dan penanda tangan `RegisterSignedAsync`; `ClinicalNoteAddendumService` mengenali `AuthorUserId`; penjaga `EnsureInpatientExecutorAsync` |
| 5. Dokter lain menerima `403` | Terpenuhi (statis) | Cabang `Forbidden` VAL-DOK-50 |
| `dotnet build` | Terpenuhi | `PASS` 17 September 2026 — bagian 8 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `BE-RWI-097` ditandai selesai padahal service fondasinya tidak terkompilasi; perbaikan dicatat di sini, laporan `BE-RWI-097` tidak diubah |
| Masalah yang diketahui | Method `UpdateOrderAsync` dan `CancelOrderAsync` pada service belum dipanggil controller (controller memakai logikanya sendiri) — warisan `BE-RWI-097`, tidak dirapikan |
| Risiko tersisa | Belum ada bukti runtime; penjaga pelaksana rawat inap mengetatkan `PATCH /execute` untuk pengguna tanpa tautan dokter maupun pegawai |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas task ini berstatus `M` (3 berkas) |
| Langkah berikutnya | Beri hak `PatientProcedure : Verify` kepada peran dokter di layar Akses Role; uji dengan akun non-SuperAdmin |

## 8. Verifikasi susulan — 17 September 2026

Atas permintaan pemilik, build dan migration dijalankan setelah laporan ini ditulis. Perintah build
yang dipakai persis perintah pemilik:
`dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false`.

| Langkah | Hasil | Tindakan |
| --- | --- | --- |
| Build ke-1 | Gagal dalam 12 detik — `CS1002` pada `ApplicationDbContextModelSnapshot.cs`: relasi `ConsultationId` milik `TrxPatientProcedure` kehilangan `;` (sisa suntingan `BE-RWI-097`) | `;` ditambahkan |
| Build ke-2 | Pemeriksaan tipe penuh: **1 error** — `CS1931` pada `CpptVerificationService.cs`: variabel query `episode` bentrok dengan variabel lokal `episode` (kode `BE-RWI-096`) | Variabel query diganti nama `episodeBerjalan`, logika tidak berubah |
| Build ke-3 | 0 error. `dotnet ef` menolak model: FK `SupersedesDecisionId` (merujuk tabelnya sendiri) dan FK `ReconciliationItemId` pada `PhmMedicationReconciliationDecision` bernama sama setelah dipotong 63 karakter — EF Core tidak memberi akhiran unik pada FK yang merujuk tabelnya sendiri. **Galat ini juga akan membuat aplikasi gagal saat `DbContext` pertama dipakai** | Nama constraint eksplisit `FK_PhmMedicationReconciliationDecision_SupersedesDecisionId` pada configuration, migration R5, dan snapshot |
| Build ke-4 | 0 error. Snapshot gagal dibaca EF: lima navigasi koleksi (`Decisions`, `Versions`, `Ranges`) ditulis di blok relasi, sebelum relasinya dideklarasikan | Dipindahkan ke bagian navigasi snapshot; diperiksa tanpa build dengan simulasi urutan navigasi (0 galat), perbandingan DDL snapshot vs model runtime di 632 tabel, dan SQL migration vs model runtime (0 temuan) |
| Build ke-5 | **0 error, 212 warning** (garis dasar 211) | — |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | — |
| `dotnet ef database update --no-build` ke `QuilvianNewDevHamzah` | `Done.` Delapan migration diterapkan: `20260916000000` s.d. `20260916007000` (termasuk R3, R7 milik task sebelumnya, lalu R4, R5, R6, R8) | `migrations list` sesudahnya: nol `Pending` |

**Yang masih `NOT RUN`:** uji kontrak API dan proses bisnis runtime, uji jalur mundur migration
(`Down`), serta regresi yang disyaratkan kartu roadmap. Database yang disentuh hanya
`QuilvianNewDevHamzah` milik pemilik; database tim, staging, dan production tidak disentuh.
