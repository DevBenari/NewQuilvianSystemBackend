# Laporan Perubahan Backend — `BE-RWI-160`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-160` |
| Judul | Katalog tindakan rawat inap |
| Slice | Dokter Rawat Inap — Finishing, kontrak `0.7.0` |
| Roadmap | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-160` |
| Trace | `FR-RWF-070`; `RWI-DEC-165` butir 1; `AC-RWF-070`; `UAT-RWF-31`; `0.7.0`: API 13.5 (`GET clinical-management/patient-procedures/master-options` + `careSetting`, `audience`); backend 12.5, 12.6 |
| Contract version | `0.7.0` approved 2026-10-02 lewat `RWI-DEC-221`; blueprint `RWI-BP-001` revision `8` |
| Dependency | —; Frontend FE-RWI-176 menggunakan parameter baru; admin Master Data perlu mengisi penanda ketersediaan tindakan. |
| Klasifikasi | MEDIUM — repository 0; pemeriksaan 1; perubahan 0; bisnis 1; API 2; database 0; auth 1; workflow 0; total 5 |
| Task mode | `BACKEND` — user meminta implementasi seluruh task dalam roadmap menggunakan build-module-backend |
| Target tulis | Source backend dan dokumentasi task sub-modul ini; frontend read-only |
| Model | Codex berbasis GPT-6; tanpa sub-agent |
| Commit backend saat dikerjakan | `f32b2308291c8d02b083319dac4210d3431f899e`; branch `MHamzah`, upstream `origin/MHamzah` |
| Tanggal | 2026-10-05 |
| Status | **Coding selesai** dalam wewenang user. Validasi statis `PASS`; build, migration, API dan runtime `NOT RUN`. DoD penuh belum dibuktikan |
| Wewenang | Instruksi user 2026-10-05: "kerjakan semua task yang ada di file tersebut", "Tanpa Melakukan Dotnet build dan migration, hanya implementasi coding". Ini dicatat sebagai otorisasi eksekusi backend; tidak memberi otorisasi frontend/publikasi |

## 1. Masalah yang diperbaiki

Katalog tindakan belum menerima konteks bangsal dan audiens pemesan secara eksplisit. Pemilih perawat perlu menampilkan tindakan khusus perawat tanpa mengubah hasil pemanggil lama.

## 2. Proses bisnis

1. Pemanggil mengirim parameter pilihan `careSetting` dan `audience`.
2. `Inpatient` menyaring `IsAvailableForInpatient`; `Outpatient` menyaring `IsAvailableForOutpatient`.
3. `Doctor` menambah saringan `IsDoctorAction`; `Nurse` tetap memungkinkan tindakan khusus perawat.
4. Parameter yang tidak dikirim mempertahankan saringan `serviceType` dan `procedureType` lama. Nilai enum tidak dikenal ditolak 400.
5. Response, pencarian, batas jumlah, dan resolusi tarif master tetap memakai jalur existing.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Masukan: kartu task roadmap, `02-backend-architecture.md` bagian 12, `contracts/api-contract.md` bagian 13, `data/data-dictionary.md` bagian Finishing, `contracts/validation-matrix.md`, acceptance matrix dan manifest approval. Source yang diperiksa adalah berkas perubahan di bawah serta `InpatientClinicalContextService`, `InsuranceCoverageService`, `AccessPermissionService`, `PermissionRegistryDescriptor`, model/configuration order pemilik, `ApplicationDbContext`, `ApiResponse` dan `PagedResult`.

Governance dibaca dari `AGENTS.md`, engineering contract dan prefix registry repository, skill build-module-backend serta aturan global `C:/Users/Admin/.codex/rules/backend/`. Tidak ada aturan backend vendor lain yang digunakan.

**Backend Governance Preflight**

| Field | Hasil |
| --- | --- |
| Area | `HealthServices` |
| Module / Submodule / pemilik / prefix | ClinicalManagement / Cli / ACTIVE / LEGACY; pemilik Muhammad Hamzah |
| Keberlakuan | TOUCHED LEGACY pada controller; NEW CODE pada enum. QBE-SVC-001 tidak dipakai untuk memaksa refactor controller legacy di luar scope. |
| Registry | Modul pemilik sudah ACTIVE; tidak membuat entity operasional baru atau melakukan rename legacy |
| QBE berlaku | QBE-NAM-001, QBE-MOD-002, QBE-SVC-001 (ratchet legacy) |
| Source target dan actual | Snapshot desain `bf5c6bde`; source implementasi `f32b2308291c8d02b083319dac4210d3431f899e`. Focused impact review dilakukan terhadap endpoint, DTO, resolusi actor, assignment, owner transaction dan DI; delta aktual dicatat di bawah. Kontrak approved tidak diubah |
| Branch / Git | `MHamzah` / `origin/MHamzah`. Working tree sebelumnya berisi dokumen hasil pembaruan migration, snapshot dan dua migration user; semuanya dipertahankan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| [Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs](../../../../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs) | Parameter opsional, validasi enum, saringan setting/audiens; jalur lama dipertahankan |
| [Areas/HealthServices/ClinicalManagement/Enums/ProcedureCatalogCareSetting.cs](../../../../../../../Areas/HealthServices/ClinicalManagement/Enums/ProcedureCatalogCareSetting.cs) | Outpatient = 1, Inpatient = 2 |
| [Areas/HealthServices/ClinicalManagement/Enums/ProcedureCatalogAudience.cs](../../../../../../../Areas/HealthServices/ClinicalManagement/Enums/ProcedureCatalogAudience.cs) | Doctor = 1, Nurse = 2 |
| Laporan ini, roadmap backend, traceability Finishing dan manifest sub-modul | Status coding, bukti validasi aktual dan dependency lintas sub-modul |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tambahan opsional pada query `master-options`; envelope dan response existing tetap. Parameter disimpan nullable untuk membedakan tidak dikirim dari nilai eksplisit. |
| Database | NOT APPLICABLE — memakai penanda master yang sudah ada; tanpa perubahan schema. |
| Keamanan/Auth | Tetap PatientProcedure : Read. Tidak menambah role atau permission baru. |

**Delta implementasi terhadap rancangan/snapshot:** Snapshot perencanaan menyebut bawaan Outpatient/Doctor, sedangkan source HEAD tanpa `serviceType` membaca ketersediaan Outpatient ATAU Inpatient, dan tanpa `procedureType` membaca tindakan dokter ATAU perawat. Parameter baru dibuat nullable agar acceptance pemanggil lama identik tetap dipenuhi. Konsumen poliklinik perlu memakai `careSetting=Outpatient` atau `serviceType=outpatient`; bangsal memakai `careSetting=Inpatient`. `careSetting` eksplisit mengalahkan `serviceType`.

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/health-services/clinical-management/patient-procedures/master-options` | Katalog sesuai setting dan audiens | `PatientProcedure : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Path <25 source task> -Mode Strict` | 25 berkas; VIOLATION 0; REVIEW 0; INFO 0; Final result PASS | `PASS` | Checker repository dijalankan pada source implementasi, termasuk berkas baru; `Migrations/` dikecualikan karena bukan perubahan task |
| Parsing sintaks C# dan pemeriksaan kontrak statis | 25 berkas tanpa error sintaks; 9 endpoint dengan metadata cocok; 59 pemeriksaan, 0 kegagalan | `PASS` | Parser tree-sitter di direktori sementara, pemeriksaan metadata permission, nullable JSON price, guard assignment dan lock/replay. Ini bukan compiler atau pengujian runtime |
| Review diff dan `git diff --check` source | Tidak ada whitespace error; perubahan dibatasi task | `PASS` | Source controller/service/DTO/model/configuration dan satu registrasi DI ditinjau |
| `dotnet build` / compilation | Tidak dijalankan sesuai larangan user | `NOT RUN` | User menjalankan mandiri |
| Pembuatan/penerapan migration dan pemeriksaan SQL migration baru | Tidak dijalankan sesuai larangan user | `NOT RUN` | Model/configuration saja yang ditulis; migration dan snapshot existing tidak disentuh |
| Verifikasi API, JSON aktual, proses bisnis dan regresi poliklinik/IGD | Tidak menjalankan aplikasi atau request API | `NOT RUN` | Acceptance berikut memiliki bukti source, belum bukti runtime |

Uji manual: **REQUIRED — NOT RUN**, dilakukan setelah build dan migration oleh user. Tidak membuat atau menjalankan project automated test; ini mengikuti kebijakan backend repository.

**Tidak dijalankan:** dotnet build, compilation, dotnet ef, migration, database update/query, aplikasi/hosted service, pengujian API/runtime, frontend lint/build, stage/commit/push/deploy.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tanpa parameter → hasil identik dengan sebelum perubahan. | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Cabang baru berjalan hanya bila parameter dikirim; saringan lama tetap ada. |
| 2. `careSetting=Inpatient` menampilkan tindakan `IsAvailableForInpatient = true` dan menyembunyikan yang `false`. | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Cabang careSetting Inpatient memakai IsAvailableForInpatient. |
| 3. `audience=Nurse` menampilkan tindakan khusus perawat. | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Audience Nurse tidak menambah saringan IsDoctorAction. |
| 4. Tindakan khusus rawat inap (`IsAvailableForOutpatient = false`) tampil di bangsal, tidak di poliklinik (`UAT-RWF-31`) | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Outpatient eksplisit memakai IsAvailableForOutpatient; pengaturan master diperlukan. |
| Implementasi seluruh coding pada kartu task | Terpenuhi dalam scope user | Berkas perubahan di bagian 3.2 |
| Build tanpa error, migration bila perlu, dan bukti proses bisnis runtime | Belum dibuktikan; `NOT RUN` | User mengecualikan build/migration; belum ada hasil compiler/database/API untuk perubahan ini |
| Laporan tracked, roadmap dan traceability | Terpenuhi | Laporan task ini dan register sub-modul diperbarui |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build dan schema baru belum diverifikasi; coding selesai tidak berarti siap rilis |
| Masalah yang diketahui | Penanda IsAvailableForInpatient bawaan true membuat katalog bergantung pada penataan master; hasil data aktual dan regresi caller lama belum diuji runtime. |
| Risiko tersisa | Perlu pembuktian acceptance melalui API dan proses bisnis setelah user menjalankan build/migration |
| Perubahan sampingan | NONE pada source. Dokumen migration dari permintaan sebelumnya dan file migration/snapshot milik user dipertahankan |
| Interupsi | NONE. Pemanggilan awal checker ditolak ExecutionPolicy lokal; berhasil dijalankan dengan Bypass hanya pada proses PowerShell pemeriksaan, tanpa mengganti policy persisten |
| Status Git | Source task M/??, tanpa stage/commit/push. Rincian kategori di bawah; berkas baru belum menjadi tracked sampai user menambahkannya sendiri ke Git |
| Langkah berikutnya | User menjalankan build; membuat/review/menerapkan migration R10/R11 bila diperlukan; kemudian memverifikasi API, permission dan regresi dengan data nyata |
| Hubungan task sub-modul lain | Frontend FE-RWI-176 menggunakan parameter baru; admin Master Data perlu mengisi penanda ketersediaan tindakan. |

Status Git pada penutupan implementasi source (sebelum laporan/register sesi ini ditambahkan):

```text
M Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodOrderController.cs
 M Areas/HealthServices/BloodBankManagement/DTOs/BloodOrderDtos.cs
 M Areas/HealthServices/BloodBankManagement/Models/BbkBloodOrder.cs
 M Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.cs
 M Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs
 M Areas/HealthServices/NutritionManagement/Controllers/NutritionOrderController.cs
 M Areas/HealthServices/NutritionManagement/DTOs/NutritionDtos.cs
 M Areas/HealthServices/NutritionManagement/Models/GziNutritionOrder.cs
 M Areas/HealthServices/NutritionManagement/Services/NutritionOrderService.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Program.cs
 M Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodOrderConfiguration.cs
 M Repositories/Configurations/HealthServices/NutritionManagement/GziNutritionConfigurations.cs
 M docs/module-blueprints/rawat-inap/02-module-map.md
 M docs/module-blueprints/rawat-inap/blueprint-manifest.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/blueprint-manifest.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap-finishing.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/requirement-traceability-finishing.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-172.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-174.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-176.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-181.md
 M docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-183.md
 M docs/module-blueprints/rawat-inap/integrasi-billing/blueprint-manifest.md
 M docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap-finishing.md
 M docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/requirement-traceability-finishing.md
 M docs/module-blueprints/rawat-inap/integrasi-billing/task/report/backend/BE-RWI-149.md
 M docs/module-blueprints/rawat-inap/keperawatan/blueprint-manifest.md
 M docs/module-blueprints/rawat-inap/keperawatan/roadmap/requirement-traceability-finishing.md
?? Areas/HealthServices/BloodBankManagement/DTOs/BloodInstructionVerificationDtos.cs
?? Areas/HealthServices/BloodBankManagement/Enums/BbkInstructionVerificationStatus.cs
?? Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.InstructionVerification.cs
?? Areas/HealthServices/ClinicalManagement/Enums/ProcedureCatalogAudience.cs
?? Areas/HealthServices/ClinicalManagement/Enums/ProcedureCatalogCareSetting.cs
?? Areas/HealthServices/InPatientManagement/Controllers/InpatientAncillaryOrderController.cs
?? Areas/HealthServices/InPatientManagement/Controllers/InpatientBloodOrderController.cs
?? Areas/HealthServices/InPatientManagement/Controllers/InpatientNutritionOrderController.cs
?? Areas/HealthServices/InPatientManagement/DTOs/InpatientAncillaryOrderDtos.cs
?? Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs
?? Areas/HealthServices/NutritionManagement/DTOs/NutritionInstructionVerificationDtos.cs
?? Areas/HealthServices/NutritionManagement/Enums/GziInstructionVerificationStatus.cs
?? Areas/HealthServices/NutritionManagement/Services/NutritionOrderService.InstructionVerification.cs
?? Migrations/20261005033044_AddRawatInapFinishing.Designer.cs
?? Migrations/20261005033044_AddRawatInapFinishing.cs
```

Laporan `BE-RWI-160.md` ditambahkan pada lokasi canonical; roadmap backend, traceability Finishing dan manifest sub-modul diperbarui sesudah laporan. Tidak melakukan stage atau publikasi Git.
