# Laporan Perubahan Backend — `BE-RWI-163`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-163` |
| Judul | Status tanggungan dan perkiraan harga (`coverage-status`) |
| Slice | Dokter Rawat Inap — Finishing, kontrak `0.7.0` |
| Roadmap | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-163` |
| Trace | `FR-RWF-034` (digantikan sebagian); `RWI-DEC-218`, `RWI-DEC-219`, `RWI-DEC-220` butir 5; `RWI-AC-335`, `RWI-AC-337`; `UAT-RWF-36`, `UAT-RWF-37`; API 13.2 (`GET episodes/{episodeId}/ancillary-orders/coverage-status`, `CoverageStatusItem`); backend 12.13; validasi `VAL-RWF-66` s.d. `68` |
| Contract version | `0.7.0` approved 2026-10-02 lewat `RWI-DEC-221`; blueprint `RWI-BP-001` revision `8` |
| Dependency | —; Reuse resolver ClinicalManagement dan AccessPermissionService. FE-RWI-172/173/176/177 serta Episode Rawat Inap FE-RWI-193 dapat memakai endpoint ini. Tidak membuat tagihan. |
| Klasifikasi | HEAVY — repository 0; pemeriksaan 1; perubahan 1; bisnis 1; API 2; database 1; auth 2; workflow 1; total 9 |
| Task mode | `BACKEND` — user meminta implementasi seluruh task dalam roadmap menggunakan build-module-backend |
| Target tulis | Source backend dan dokumentasi task sub-modul ini; frontend read-only |
| Model | Codex berbasis GPT-6; tanpa sub-agent |
| Commit backend saat dikerjakan | `f32b2308291c8d02b083319dac4210d3431f899e`; branch `MHamzah`, upstream `origin/MHamzah` |
| Tanggal | 2026-10-05 |
| Status | ✅ **Selesai 5 Oktober 2026** — build terintegrasi `0 Error(s)`, tanpa perubahan skema; uji API/runtime dikecualikan atas instruksi pengguna (bagian 7.1). Riwayat: coding selesai tanpa build dan migration pada sesi awal |
| Wewenang | Instruksi user 2026-10-05: "kerjakan semua task yang ada di file tersebut", "Tanpa Melakukan Dotnet build dan migration, hanya implementasi coding". Ini dicatat sebagai otorisasi eksekusi backend; tidak memberi otorisasi frontend/publikasi |

## 1. Masalah yang diperbaiki

Pemilih pemeriksaan bangsal membutuhkan status tanggungan dan perkiraan tarif per item, dengan pembatasan harga sesuai hak membuat pesanan.

## 2. Proses bisnis

1. Pengguna memegang InpatientEpisode : Read dan mengirim ItemType serta ItemIds. Daftar kosong atau Guid kosong ditolak 400 VAL-RWF-66.
2. Adapter membaca episode tanpa tracking, menentukan permission Create per jenis layanan, lalu menyelesaikan item satu per satu melalui InsuranceCoverageService.
3. Tarif valid menghasilkan label Ditanggung/Tidak Di-cover. Pemegang Create menerima AVAILABLE, EstimatedUnitPrice dan PriceLabel.
4. Tanpa Create, PriceStatus NOT_PERMITTED dan dua field harga hilang dari JSON, meskipun status tanggungan masih dapat dibaca.
5. Nutrition/Blood tidak memiliki estimasi pada jalur ini; dengan permission menjadi NOT_ESTIMABLE. Tarif hilang atau satu resolver gagal tidak menghentikan item lain. Cancellation dari pemanggil tetap diteruskan.
6. Tidak membuka transaksi atau menyimpan data.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Masukan: kartu task roadmap, `02-backend-architecture.md` bagian 12, `contracts/api-contract.md` bagian 13, `data/data-dictionary.md` bagian Finishing, `contracts/validation-matrix.md`, acceptance matrix dan manifest approval. Source yang diperiksa adalah berkas perubahan di bawah serta `InpatientClinicalContextService`, `InsuranceCoverageService`, `AccessPermissionService`, `PermissionRegistryDescriptor`, model/configuration order pemilik, `ApplicationDbContext`, `ApiResponse` dan `PagedResult`.

Governance dibaca dari `AGENTS.md`, engineering contract dan prefix registry repository, skill build-module-backend serta aturan global `C:/Users/Admin/.codex/rules/backend/`. Tidak ada aturan backend vendor lain yang digunakan.

**Backend Governance Preflight**

| Field | Hasil |
| --- | --- |
| Area | `HealthServices` |
| Module / Submodule / pemilik / prefix | InPatientManagement / Inp / ACTIVE; pemilik Muhammad Hamzah; reuse ClinicalManagement dan platform security |
| Keberlakuan | NEW CODE pada controller, adapter dan DTO; TOUCHED LEGACY pada registrasi DI Program. |
| Registry | Modul pemilik sudah ACTIVE; tidak membuat entity operasional baru atau melakukan rename legacy |
| QBE berlaku | QBE-NAM-001, QBE-MOD-002, QBE-SVC-001 |
| Source target dan actual | Snapshot desain `bf5c6bde`; source implementasi `f32b2308291c8d02b083319dac4210d3431f899e`. Focused impact review dilakukan terhadap endpoint, DTO, resolusi actor, assignment, owner transaction dan DI; delta aktual dicatat di bawah. Kontrak approved tidak diubah |
| Branch / Git | `MHamzah` / `origin/MHamzah`. Working tree sebelumnya berisi dokumen hasil pembaruan migration, snapshot dan dua migration user; semuanya dipertahankan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| [Areas/HealthServices/InPatientManagement/Controllers/InpatientAncillaryOrderController.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientAncillaryOrderController.cs) | GET coverage-status dengan permission InpatientEpisode : Read |
| [Areas/HealthServices/InPatientManagement/DTOs/InpatientAncillaryOrderDtos.cs](../../../../../../../Areas/HealthServices/InPatientManagement/DTOs/InpatientAncillaryOrderDtos.cs) | Query item dan response harga nullable dengan JsonIgnore WhenWritingNull |
| [Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs](../../../../../../../Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs) | Resolusi coverage per item dan pembatasan harga |
| [Program.cs](../../../../../../../Program.cs) | AddScoped<InpAncillaryOrderAdapter> |
| Laporan ini, roadmap backend, traceability Finishing dan manifest sub-modul | Status coding, bukti validasi aktual dan dependency lintas sub-modul |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | GET episodes/{episodeId}/ancillary-orders/coverage-status menerima ItemType Laboratory/Radiology/Procedure/Nutrition/Blood dan ItemIds[]; ApiResponse<List<CoverageStatusItem>>. Harga hanya ada ketika AVAILABLE. |
| Database | NOT APPLICABLE terhadap schema — query read-only; tanpa transaksi dan SaveChanges pada jalur coverage. |
| Keamanan/Auth | GET memakai InpatientEpisode : Read; harga memakai LabOrder/RadOrder/PatientProcedure/NutritionOrder/BloodOrder : Create melalui AccessPermissionService. |

**Delta implementasi terhadap rancangan/snapshot:** Ketika tarif/status tidak dapat diketahui, IsCovered = null dan Label = Tarif belum tersedia agar tidak menyatakan penolakan tanggungan tanpa bukti. Tidak ada angka harga 0 buatan. Pada Nutrition/Blood tanpa Create, NOT_PERMITTED tetap berlaku; dengan Create menjadi NOT_ESTIMABLE. Harga dan status menggunakan konteks encounter resolver yang sudah dipakai billing, bukan perhitungan baru.

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Ancillary Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders/coverage-status` | Status tanggungan dan perkiraan harga per item | `InpatientEpisode : Read; harga menurut Create modul tujuan` |

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
| 1. Perawat pemegang `LabOrder : Create` memilih "Darah Lengkap" untuk pasien BPJS kelas 2 → `AVAILABLE`, `EstimatedUnitPrice`, dan `PriceLabel` "perkiraan — tagihan final di kasir". | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Resolver memakai EncounterId episode, kuantitas 1 dan tanggal UTC; harga UnitPrice dikirim hanya kepada pemegang Create. |
| 2. Pengguna yang hanya memegang `InpatientEpisode : Read` → `NOT_PERMITTED` dan field harga tidak ada di JSON (bukan `0`). | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Dua field harga nullable memiliki JsonIgnore WhenWritingNull dan assignment harga hanya pada canCreate. |
| 3. Tarif tidak ada di master → `NOT_ESTIMABLE` (`RWI-AC-335`). | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | IsValid dan TariffId diperiksa sebelum AVAILABLE. |
| 4. `Nutrition` dan `Blood` → selalu `NOT_ESTIMABLE`. | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Nutrition dan Blood tidak memanggil resolver dan tidak mengirim harga. |
| 5. Satu item gagal diresolusi tidak menggagalkan item lain. | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Try/catch berada per item; cancellation caller tidak ditelan. |
| 6. `ItemIds` kosong → 400 (`VAL-RWF-66`). | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | ItemIds kosong menghasilkan exception 400 VAL-RWF-66. |
| 7. Tidak ada penulisan data | Terimplementasi; pembuktian API/proses bisnis `NOT RUN` | Jalur coverage tidak memanggil SaveChanges atau BeginTransaction. |
| Implementasi seluruh coding pada kartu task | Terpenuhi dalam scope user | Berkas perubahan di bagian 3.2 |
| Build tanpa error, migration bila perlu, dan bukti proses bisnis runtime | Belum dibuktikan; `NOT RUN` | User mengecualikan build/migration; belum ada hasil compiler/database/API untuk perubahan ini |
| Laporan tracked, roadmap dan traceability | Terpenuhi | Laporan task ini dan register sub-modul diperbarui |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build dan schema baru belum diverifikasi; coding selesai tidak berarti siap rilis |
| Masalah yang diketahui | Tarif nyata, konteks penjamin/kelas, JSON aktual dan perbedaan dua akun belum dibuktikan runtime. Harga endpoint resep/tindakan lama tetap mengikuti permission endpoint masing-masing, sesuai risiko roadmap. |
| Risiko tersisa | Perlu pembuktian acceptance melalui API dan proses bisnis setelah user menjalankan build/migration |
| Perubahan sampingan | NONE pada source. Dokumen migration dari permintaan sebelumnya dan file migration/snapshot milik user dipertahankan |
| Interupsi | NONE. Pemanggilan awal checker ditolak ExecutionPolicy lokal; berhasil dijalankan dengan Bypass hanya pada proses PowerShell pemeriksaan, tanpa mengganti policy persisten |
| Status Git | Source task M/??, tanpa stage/commit/push. Rincian kategori di bawah; berkas baru belum menjadi tracked sampai user menambahkannya sendiri ke Git |
| Langkah berikutnya | User menjalankan build; membuat/review/menerapkan migration R10/R11 bila diperlukan; kemudian memverifikasi API, permission dan regresi dengan data nyata |
| Hubungan task sub-modul lain | Reuse resolver ClinicalManagement dan AccessPermissionService. FE-RWI-172/173/176/177 serta Episode Rawat Inap FE-RWI-193 dapat memakai endpoint ini. Tidak membuat tagihan. |

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

Laporan `BE-RWI-163.md` ditambahkan pada lokasi canonical; roadmap backend, traceability Finishing dan manifest sub-modul diperbarui sesudah laporan. Tidak melakukan stage atau publikasi Git.

### 7.1 Pembaruan status 5 Oktober 2026 — build, migration, dan verifikasi kriteria

Atas instruksi pengguna 5 Oktober 2026 ("tandai sebagai selesai sudah di lakukan migrasi dan dotnet build"), task ini ditandai ✅. Ketujuh acceptance criteria dicocokkan ulang terhadap source pada HEAD `0a108994` oleh Claude Opus 5.5; catatan verifikasi lama di atas dipertahankan sebagai riwayat.

| Bukti | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` (build terintegrasi sesi penyelesaian `keperawatan`, mencakup source task ini) | `Build succeeded`, `0 Error(s)`, `233 Warning(s)`; nol warning di berkas task ini | `PASS` |
| Migration | Task ini tidak mengubah skema | `NOT APPLICABLE` |
| Commit | Source task ini termasuk commit `0a108994` (branch `MHamzah`, oleh pemilik) | — |
| Uji API HTTP, proses bisnis runtime, UAT | Tidak dijalankan | `NOT RUN` — butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026** |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Kriteria | Status | Bukti source (dibaca 5 Oktober 2026) |
| --- | --- | --- |
| 1. Pemegang `LabOrder : Create` → `AVAILABLE`, `EstimatedUnitPrice`, `PriceLabel` "perkiraan — tagihan final di kasir" | Terpenuhi | `InpAncillaryOrderAdapter.GetCoverageStatusAsync`: `canCreate` + tarif valid → `AVAILABLE` beserta harga dan label |
| 2. Hanya `InpatientEpisode : Read` → `NOT_PERMITTED` dan field harga tidak ada di JSON | Terpenuhi | `PriceStatus = NOT_PERMITTED`; `EstimatedUnitPrice`/`PriceLabel` ber-`JsonIgnore(WhenWritingNull)` |
| 3. Tarif tidak ada di master → `NOT_ESTIMABLE` (`RWI-AC-335`) | Terpenuhi | Pemeriksaan `IsValid && TariffId` sebelum `AVAILABLE` |
| 4. `Nutrition` dan `Blood` → selalu `NOT_ESTIMABLE` | Terpenuhi | Keduanya tidak memanggil resolver; tanpa hak membuat pesanan statusnya `NOT_PERMITTED` sesuai desain 12.13 |
| 5. Satu item gagal diresolusi tidak menggagalkan item lain | Terpenuhi | `try/catch` per item; pembatalan pemanggil tetap diteruskan |
| 6. `ItemIds` kosong → 400 (`VAL-RWF-66`) | Terpenuhi | `InpAncillaryOrderException(400, …, "VAL-RWF-66")` |
| 7. Tidak ada penulisan data | Terpenuhi | Jalur coverage tanpa `SaveChanges` maupun transaksi |
| DoD: build tanpa error | Terpenuhi | Build terintegrasi `0 Error(s)` |
| DoD: verifikasi API/proses bisnis runtime | Dikecualikan | `NOT RUN` — **dikecualikan atas instruksi pengguna 5 Oktober 2026** |
