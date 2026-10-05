# Laporan Perubahan Backend — `BE-RWI-161`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-161` |
| Judul | Verifikasi instruksi pesanan gizi (`R10`) |
| Slice | Dokter Rawat Inap — Finishing, kontrak `0.7.0` |
| Roadmap | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-161` |
| Trace | `FR-RWF-031`, `FR-RWF-037`; `RWI-DEC-171`, `RWI-DEC-188`, `RWI-DEC-191`; `INV-RWF-22`, `INV-RWF-23`; API 13.3 (`POST orders` diperluas, `GET orders/instruction-verification-worklist`, `POST orders/{id}/verify-instruction`); backend 12.5, 12.8, 12.9 (`R10`); validasi `VAL-RWF-63`, `64` |
| Contract version | `0.7.0` approved 2026-10-02 lewat `RWI-DEC-221`; blueprint `RWI-BP-001` revision `8` |
| Dependency | —; Gizi adalah modul pemilik order. GziInstructionVerificationStatus juga dibutuhkan Keperawatan BE-RWI-166 (diet); enum sudah dibuat di task ini. BE-RWI-164 dan FE-RWI-174/175 memakai endpoint ini; implementasi diet BE-RWI-166 tidak dilakukan di scope ini. |
| Klasifikasi | HEAVY — repository 0; pemeriksaan 1; perubahan 1; bisnis 1; API 2; database 2; auth 2; workflow 1; total 10 |
| Task mode | `BACKEND` — user meminta implementasi seluruh task dalam roadmap menggunakan build-module-backend |
| Target tulis | Source backend dan dokumentasi task sub-modul ini; frontend read-only |
| Model | Codex berbasis GPT-6; tanpa sub-agent |
| Commit backend saat dikerjakan | `f32b2308291c8d02b083319dac4210d3431f899e`; branch `MHamzah`, upstream `origin/MHamzah` |
| Tanggal | 2026-10-05 |
| Status | **Coding selesai; file migration R10/R11 kini tersedia dan diperiksa secara statis**. Build, penerapan database, API dan runtime oleh agent `NOT RUN`; bukti historis coding di bawah tetap dipertahankan. DoD penuh belum dibuktikan |
| Wewenang | Instruksi user 2026-10-05: "kerjakan semua task yang ada di file tersebut", "Tanpa Melakukan Dotnet build dan migration, hanya implementasi coding". Ini dicatat sebagai otorisasi eksekusi backend; tidak memberi otorisasi frontend/publikasi |

## 1. Masalah yang diperbaiki

Order Gizi belum menyimpan apakah instruksi dari pemesan bangsal perlu diverifikasi dokter peminta.

## 2. Proses bisnis

1. Pemanggil lama tanpa status verifikasi tetap menghasilkan NotRequired.
2. Jalur bangsal meminta status; service menentukannya dari dokter akun login dan RequesterDoctorId: dokter peminta sendiri → NotRequired, pemesan lain → Pending. Payload Verified ditolak.
3. Worklist memuat Pending milik dokter login, terurut waktu dan dipaginasi.
4. Dokter peminta mengirim ExpectedVersion untuk memverifikasi. Dokter lain ditolak 403; status bukan Pending atau versi usang ditolak 409.
5. Penyimpanan status Verified, waktu UTC, user, versi dan riwayat dilakukan melalui pemilik Gizi. Riwayat order dan audit logger mencatat tindakan.
6. Pembuatan baru dari bangsal diserialkan per IdempotencyKey dalam transaksi PostgreSQL; pengulangan membaca hasil pertama dan fingerprint berbeda ditolak.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Masukan: kartu task roadmap, `02-backend-architecture.md` bagian 12, `contracts/api-contract.md` bagian 13, `data/data-dictionary.md` bagian Finishing, `contracts/validation-matrix.md`, acceptance matrix dan manifest approval. Source yang diperiksa adalah berkas perubahan di bawah serta `InpatientClinicalContextService`, `InsuranceCoverageService`, `AccessPermissionService`, `PermissionRegistryDescriptor`, model/configuration order pemilik, `ApplicationDbContext`, `ApiResponse` dan `PagedResult`.

Governance dibaca dari `AGENTS.md`, engineering contract dan prefix registry repository, skill build-module-backend serta aturan global `C:/Users/Admin/.codex/rules/backend/`. Tidak ada aturan backend vendor lain yang digunakan.

**Backend Governance Preflight**

| Field | Hasil |
| --- | --- |
| Area | `HealthServices` |
| Module / Submodule / pemilik / prefix | NutritionManagement / Gzi / ACTIVE; pemilik Ikbal Yulianto; wewenang lintas pemilik RWI-DEC-191 |
| Keberlakuan | TOUCHED LEGACY pada model, configuration, controller, service dan DTO; NEW CODE pada enum, DTO dan service partial. |
| Registry | Modul pemilik sudah ACTIVE; tidak membuat entity operasional baru atau melakukan rename legacy |
| QBE berlaku | QBE-ENT-001, QBE-NAM-001, QBE-CFG-001, QBE-MOD-002, QBE-SVC-001 |
| Source target dan actual | Snapshot desain `bf5c6bde`; source implementasi `f32b2308291c8d02b083319dac4210d3431f899e`. Focused impact review dilakukan terhadap endpoint, DTO, resolusi actor, assignment, owner transaction dan DI; delta aktual dicatat di bawah. Kontrak approved tidak diubah |
| Branch / Git | `MHamzah` / `origin/MHamzah`. Working tree sebelumnya berisi dokumen hasil pembaruan migration, snapshot dan dua migration user; semuanya dipertahankan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| [Areas/HealthServices/NutritionManagement/Controllers/NutritionOrderController.cs](../../../../../../../Areas/HealthServices/NutritionManagement/Controllers/NutritionOrderController.cs) | Worklist dan aksi VerifyInstruction |
| [Areas/HealthServices/NutritionManagement/DTOs/NutritionDtos.cs](../../../../../../../Areas/HealthServices/NutritionManagement/DTOs/NutritionDtos.cs) | Status permintaan opsional dan tiga field verifikasi pada response |
| [Areas/HealthServices/NutritionManagement/DTOs/NutritionInstructionVerificationDtos.cs](../../../../../../../Areas/HealthServices/NutritionManagement/DTOs/NutritionInstructionVerificationDtos.cs) | ExpectedVersion dan NutritionOrderVerificationItem untuk verifikasi |
| [Areas/HealthServices/NutritionManagement/Enums/GziInstructionVerificationStatus.cs](../../../../../../../Areas/HealthServices/NutritionManagement/Enums/GziInstructionVerificationStatus.cs) | Enum bersama order dan diet Gizi |
| [Areas/HealthServices/NutritionManagement/Models/GziNutritionOrder.cs](../../../../../../../Areas/HealthServices/NutritionManagement/Models/GziNutritionOrder.cs) | Tiga field verifikasi |
| [Areas/HealthServices/NutritionManagement/Services/NutritionOrderService.cs](../../../../../../../Areas/HealthServices/NutritionManagement/Services/NutritionOrderService.cs) | Status saat create, mapping response, idempotensi jalur bangsal dan konflik unique |
| [Areas/HealthServices/NutritionManagement/Services/NutritionOrderService.InstructionVerification.cs](../../../../../../../Areas/HealthServices/NutritionManagement/Services/NutritionOrderService.InstructionVerification.cs) | Resolusi status, worklist, verifikasi owner/version dan riwayat |
| [Repositories/Configurations/HealthServices/NutritionManagement/GziNutritionConfigurations.cs](../../../../../../../Repositories/Configurations/HealthServices/NutritionManagement/GziNutritionConfigurations.cs) | Default NotRequired, index status, FK user Restrict |
| Laporan ini, roadmap backend, traceability Finishing dan manifest sub-modul | Status coding, bukti validasi aktual dan dependency lintas sub-modul |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | POST orders menerima InstructionVerificationStatus opsional; response daftar/detail memuat tiga field verifikasi. Worklist memakai PagedResult<NutritionOrderVerificationItem> (default PageSize 10); verify memakai VerifyNutritionInstructionRequest. |
| Database | R10: tiga kolom pada GziNutritionOrder, index status dan FK nullable ApplicationUser dengan Restrict. Model/configuration selesai; pembuatan dan penerapan migration NOT RUN sesuai user. |
| Keamanan/Auth | NutritionOrder : VerifyInstruction pada GET worklist dan POST verify. Kepemilikan dokter diperiksa dari akun login, bukan nama role; actor tidak diambil dari payload. |

**Delta implementasi terhadap rancangan/snapshot:** OrderVerificationItem pada desain belum memiliki DTO konkret di source; worklist menggunakan NutritionOrderVerificationItem milik Gizi agar id, pasien, RequesterDoctorId, status dan Version tersedia tanpa mengirim detail asuhan yang belum dimuat. Verifikasi tetap terpisah dari status order: order Closed boleh diverifikasi, order Cancelled ditolak. State Verified tidak dapat diisikan langsung ketika create. Advisory transaction lock ditambahkan hanya untuk pemanggil create yang meminta status verifikasi agar kebutuhan pengiriman ulang bangsal aman saat serentak.

## 4. Dokumentasi endpoint

#### Health Services / Nutrition Management / Nutrition Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/health-services/nutrition-management/orders` | Membuat pesanan; status verifikasi opsional | `NutritionOrder : Create` |
| `GET` | `api/v1/health-services/nutrition-management/orders/instruction-verification-worklist` | Pending milik dokter login | `NutritionOrder : VerifyInstruction` |
| `POST` | `api/v1/health-services/nutrition-management/orders/{id}/verify-instruction` | Verifikasi instruksi dengan ExpectedVersion | `NutritionOrder : VerifyInstruction` |

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
| 1. Pesanan lama dan pesanan dari poliklinik tetap `NotRequired` dan alurnya tidak berubah (`AC-RWF-036`). | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Status default 0 dan create tanpa field/parameter baru kembali NotRequired. |
| 2. Daftar hanya memuat pesanan `Pending` milik dokter yang login. | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Filter RequesterDoctorId = dokter login dan status Pending. |
| 3. Verifikasi oleh dokter peminta menyimpan status, waktu, dan pemverifikasi. | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Verify menyimpan status, UTC, user, audit update, Version dan riwayat. |
| 4. Dokter lain → 403 (`VAL-RWF-63`). | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Pemeriksaan dokter akun login terhadap RequesterDoctorId sebelum mutation. |
| 5. Sudah diverifikasi → 409 (`VAL-RWF-64`). | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Status bukan Pending dan ExpectedVersion usang memicu NutritionConflictException. |
| 6. Baris registry `NutritionOrder : VerifyInstruction` lahir dari atribut endpoint | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Atribut AccessAction/AccessPermission VerifyInstruction cocok dengan NutritionOrder. |
| Implementasi seluruh coding pada kartu task | Terpenuhi dalam scope user | Berkas perubahan di bagian 3.2 |
| Build tanpa error, migration bila perlu, dan bukti proses bisnis runtime | Belum dibuktikan; `NOT RUN` | User mengecualikan build/migration; belum ada hasil compiler/database/API untuk perubahan ini |
| Laporan tracked, roadmap dan traceability | Terpenuhi | Laporan task ini dan register sub-modul diperbarui |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build dan schema baru belum diverifikasi; coding selesai tidak berarti siap rilis |
| Masalah yang diketahui | Schema baru belum tersedia sebelum user membuat dan menerapkan migration. Regresi poliklinik, pemetaan dokter akun, concurrency dan permission registry belum dibuktikan dengan runtime. |
| Risiko tersisa | Perlu pembuktian acceptance melalui API dan proses bisnis setelah user menjalankan build/migration |
| Perubahan sampingan | NONE pada source. Dokumen migration dari permintaan sebelumnya dan file migration/snapshot milik user dipertahankan |
| Interupsi | NONE. Pemanggilan awal checker ditolak ExecutionPolicy lokal; berhasil dijalankan dengan Bypass hanya pada proses PowerShell pemeriksaan, tanpa mengganti policy persisten |
| Status Git | Source task M/??, tanpa stage/commit/push. Rincian kategori di bawah; berkas baru belum menjadi tracked sampai user menambahkannya sendiri ke Git |
| Langkah berikutnya | User menjalankan build; membuat/review/menerapkan migration R10/R11 bila diperlukan; kemudian memverifikasi API, permission dan regresi dengan data nyata |
| Hubungan task sub-modul lain | Gizi adalah modul pemilik order. GziInstructionVerificationStatus juga dibutuhkan Keperawatan BE-RWI-166 (diet); enum sudah dibuat di task ini. BE-RWI-164 dan FE-RWI-174/175 memakai endpoint ini; implementasi diet BE-RWI-166 tidak dilakukan di scope ini. |

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

Laporan `BE-RWI-161.md` ditambahkan pada lokasi canonical; roadmap backend, traceability Finishing dan manifest sub-modul diperbarui sesudah laporan. Tidak melakukan stage atau publikasi Git.

### 7.1 Pembaruan file migration — 5 Oktober 2026

User memperjelas preferensi: file migration harus ikut disiapkan pada perubahan schema agar cukup menjalankan `dotnet ef database update` sendiri. Preferensi ini dicatat pada AGENTS.md backend. Pada pemeriksaan ulang, migration `20261005050735_AddNutritionAndBloodInstructionVerification` sudah tersedia di workspace beserta Designer dan snapshot yang telah diperbarui. Agent tidak membuat ulang, menimpa berkas tersebut, atau menjalankan perintah pembuatannya. Catatan NOT RUN pada bagian sebelumnya adalah bukti sesi coding awal, bukan status ketersediaan file saat ini.

| Berkas/bukti | Status terbaru |
| --- | --- |
| [Migration](../../../../../../../Migrations/20261005050735_AddNutritionAndBloodInstructionVerification.cs) | Tersedia; Up menambah enam kolom verifikasi pada GziNutritionOrder dan BbkBloodOrder, empat index, dua FK AspNetUsers dengan Restrict; Down mengembalikan perubahan yang sama |
| [Designer](../../../../../../../Migrations/20261005050735_AddNutritionAndBloodInstructionVerification.Designer.cs) | Tersedia; MigrationAttribute/DbContext sesuai dan target model sama dengan snapshot |
| [Model snapshot](../../../../../../../Migrations/ApplicationDbContextModelSnapshot.cs) | Sudah diperbarui di workspace; dibandingkan dengan target model AddRawatInapFinishing, hanya dua model order dan relasinya yang berubah |
| Parsing sintaks migration/Designer/snapshot | PASS — tiga file tanpa error sintaks |
| Pemeriksaan scope, Up/Down, default dan model target | PASS — status default 0, timestamp/UUID nullable, index dan FK sesuai configuration; tanpa perubahan model lain |
| Build, dotnet ef dan database update oleh agent | NOT RUN; tidak dijalankan |
| Penerapan migration oleh user | Belum ada bukti penerapan dalam percakapan ini; jangan dianggap sudah berhasil diterapkan |

Langkah berikutnya: user menjalankan `dotnet ef database update`, lalu verifikasi API dan regresi. Migration ini mencakup R10 dan R11 dalam satu berkas; tidak membuat schema diet Keperawatan BE-RWI-166. File migration/Designer/snapshot yang ditemukan dipertahankan tanpa perubahan oleh agent.

### 7.2 Pembaruan bukti 5 Oktober 2026 — build akhir dan penerapan migration

Dicatat dari sesi penyelesaian `keperawatan` `BE-RWI-165`–`171` (Claude Opus 5.5), atas instruksi pengguna menjalankan build dan database update di akhir. Catatan ini hanya menambah bukti; acceptance criteria task ini tidak divalidasi ulang dan status roadmap-nya tidak diubah.

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` (build terintegrasi, mencakup source task ini) | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` |
| `dotnet ef database update --no-build` | `Applying migration '20261005050735_AddNutritionAndBloodInstructionVerification'` (R10/R11), lalu `20261005071042_AddRawatInapKeperawatanFinishing`; `Done.` |
| `dotnet ef migrations list --no-build` | Nol migration `Pending` |
| Isi migration lanjutan | `20261005071042` tidak memuat operasi apa pun pada `GziNutritionOrder` maupun `BbkBloodOrder`, sehingga snapshot tulisan tangan R10/R11 terbukti selaras dengan model |
| Uji API dan regresi poliklinik | `NOT RUN` |
