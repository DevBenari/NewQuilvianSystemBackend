# Laporan Perubahan Backend — `BE-HRD-363`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-363` |
| Judul | Verifikasi slice S-A8: kontrak v27, hak akses di Akses Role, build, dan QBE |
| Slice | `S-A8` / Verifikasi — gelombang 3 |
| Roadmap | `roadmap/backend-roadmap.md` bagian Slice S-A8, kartu `BE-HRD-363` |
| Trace | Keempat endpoint `api/v1/corporate/human-resource/workforce-core/payroll-data`; API Contract §33, Permission & Audit §31; `HRD-DEC-219`, `HRD-DEC-223` |
| Contract version | `v27` bagian 33 — approved 16 November 2026 (`HRD-DEC-015`); tidak diubah (satu penyegaran dicatat di bagian 5) |
| Dependency | `BE-HRD-360` ✅, `BE-HRD-362` ✅ (18 November 2026); `BE-HRD-361` ✅ (17 November 2026) |
| Klasifikasi | `SMALL` — skor 2: satu repository (0), sekitar 10 berkas diperiksa (1), 1 berkas source diubah satu baris (0), tanpa perubahan API/schema (0), tanpa data sensitif yang ditulis (0), ditambah 1 untuk verifikasi provider-nyata |
| Task mode | `BACKEND` (instruksi pengguna 18 November 2026 "lanjut task berikutnya") |
| Target tulis | `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforcePayrollDataController.cs` (satu atribut); uji baru `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataContractTests.cs`; dokumen roadmap/traceability/indeks uji/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a17a2bc3` (branch `yoga`); source slice S-A8 di-commit di `dc723f0f`; perubahan `BE-HRD-353`–`358`, `360`–`362`, `FIX-002`–`004` belum di-commit |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terbukti: uji kesesuaian kontrak 8/8 (dua kontrol negatif tertangkap), proyek uji HR 272/272, `dotnet build` 0 galat, QBE `Strict` tanpa temuan (probe kontrol tertangkap di dua pemanggilan), katalog Akses Role devYoga memuat `Read` dan `Update`, tanpa migration |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `WorkforceCore` |
| Pemilik / prefix registry | `Human Resource` / `Wfp`; tanpa entity baru |
| Keberlakuan | `NEW CODE` (source slice S-A8, 16 November 2026) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-AUD-001`, `QBE-LOG-001` (daftar preflight roadmap); pemeriksa otomatis menilai `QBE-ENT-001`, `QBE-NAM-001`, `QBE-CFG-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-MOD-002`, `QBE-SVC-001` |
| Tidak berlaku | `QBE-DB-*` (tanpa schema/migration) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |

---

## 1. Keselarasan dengan kontrak v27

| Butir | Kontrak | Source | Hasil |
| --- | --- | --- | --- |
| Base route, tag, controller akses | `api/v1/corporate/human-resource/workforce-core/payroll-data`; tag "Corporate / Human Resource / Workforce Core / Payroll Data"; modul `HUMAN_RESOURCE_WORKFORCE_CORE`, `ControllerName = "WorkforcePayrollData"`; pengguna harus masuk | Sama; `[Authorize]`; `VisibleInRoleAccess` benar, `IsSystemOnly` salah | Sesuai |
| `GET /summary` | `WorkforcePayrollData : Read`; `ApiResponse<WorkforcePayrollDataSummaryResponse>` | Sama | Sesuai |
| `GET /` | `WorkforcePayrollData : Read`; query `WorkforcePayrollDataQuery`; `ApiResponse<PagedResult<WorkforcePayrollDataItemResponse>>` | Sama | Sesuai |
| `GET /{workforceProfileId}` | `WorkforcePayrollData : Update`; `404` berpesan | Sama; `200`, `404` dideklarasikan | Sesuai |
| `PUT /{workforceProfileId}` | `WorkforcePayrollData : Update`; `400`, `403` (opsi setuju/verifikasi tanpa hak), `404` | **Delta**: `403` dikembalikan action tetapi tidak dideklarasikan | Diperbaiki — `[ProducesResponseType(…, Status403Forbidden)]` ditambahkan |
| `403` tanpa hak endpoint | Berlaku untuk keempat endpoint | Datang dari filter `[AccessPermission]`; tidak dideklarasikan per action, sama dengan konvensi controller HR (21 dari 256 controller HR mendeklarasikan `403`, seluruhnya untuk `403` yang dikembalikan action sendiri) | Sesuai konvensi |
| `[AccessPermission]` = `ControllerName` + nama `[AccessAction]` (Permission §31) | `WorkforcePayrollData` + `Read`/`Update` | Sama untuk keempat action; nama tampilan "Read/Update Workforce Payroll Data" | Sesuai |
| Hak `Import` dihapus (`HRD-DEC-223`) | Hanya `Read` dan `Update` | Hanya `Read` dan `Update` di source dan di `SysActionAccess` devYoga | Sesuai |
| DTO §33.2 dan §33.3 | Daftar ruas per DTO | Seluruh ruas sama (camelCase JSON); bawaan `pageNumber` 1, `pageSize` 25; opsi bawaan `false`; hasil bawaan "Tidak diubah" | Sesuai, kecuali satu ruas tambahan (baris berikut) |
| `WorkforcePayrollDataSaveResponse` | `salaryAssignment`, `bankAccount`, `warnings` | Ditambah `workforceProfileId` (gema id dari path, tidak sensitif); dibaca frontend `normalizeWorkforcePayrollSaveResult` | **Tidak diubah**; dicatat untuk penyegaran kontrak (bagian 5) |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforcePayrollDataController.cs` | Action `Save` mendeklarasikan `[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]`. Perilaku tidak berubah; Swagger kini menampilkan `403`. CRLF dipertahankan (126 CR = 126 baris); +1 |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataContractTests.cs` (baru) | 8 uji (4 baris teori + 4 fakta): route/tag/identitas akses controller; per endpoint verb, path, hak, nama dan tipe `[AccessAction]`, kode status yang dideklarasikan; tipe respons `200`; hanya hak `Read`/`Update`; nama ruas seluruh DTO dan nilai bawaannya |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test --filter WorkforcePayrollDataContractTests` | 8/8 lulus | `PASS` (AC1, AC2) | Keluaran `dotnet test` |
| Kontrol negatif (berkas dipulihkan dan dibandingkan byte demi byte tiap putaran) | K1 controller sebelum perbaikan (tanpa `403`) → baris teori `Save` gagal; K2 hak `Detail` diturunkan ke `Read` → baris teori `Detail` gagal | `PASS` | Keluaran `dotnet test` per kontrol |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`) | 272/272 lulus (264 + 8 baru); termasuk uji S-A8 lain: detail 6, simpan 8, daftar 4 | `PASS` | Keluaran `dotnet test` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Build succeeded, 0 galat; 12 peringatan, tidak satu pun dari berkas S-A8 (build `--no-incremental` disaring "WorkforcePayrollData": 0 baris) | `PASS` (AC3) | Keluaran `dotnet build` |
| Pemeriksa QBE `ReportOnly` atas controller, service, DTO + probe kontrol `ZzzQbeProbeController.cs` (dua pemanggilan tool terpisah) | Kedua pemanggilan: `Files evaluated: 4`; probe kena 2 `VIOLATION` `QBE-CODE-002` dan 2 `REVIEW` `QBE-SVC-001`; berkas S-A8 tanpa temuan | `PASS` | Keluaran pemeriksa |
| Pemeriksa QBE `Strict` atas controller, service, DTO (probe sudah dihapus) | `Files evaluated: 3`, `VIOLATION: 0`, `REVIEW: 0`, "Findings: none", "Final result: PASS" | `PASS` (AC3) | Keluaran pemeriksa |
| Pembersihan probe | `git status --short Areas/Corporate/HumanResource/WorkforceCore/` hanya menampilkan controller dan service (diubah oleh `360`–`363`); probe tidak tersisa | `PASS` | `git status` |
| Startup Development | "Permission registry valid. 2457 identitas kanonik dari 2457 kemampuan terdaftar." | `PASS` | `be-363.log` |
| Basis data devYoga (baca saja) | `SysControllerAccess` `WorkforcePayrollData` modul `HUMAN_RESOURCE_WORKFORCE_CORE`, aktif, tampil di Akses Role; `SysActionAccess` hanya `Read` dan `Update` (aktif, tampil, bukan sistem); tanpa sisa `Import` | `PASS` (AC2) | `q363.mjs` |
| API devYoga — katalog Akses Role | `GET role-access/resources` dan `GET role-access/resources/structured`: "Corporate / Human Resource Workforce Core / Workforce Payroll Data", `canAssign: true`, `totalAction: 2`, aksi `Read` dan `Update` | `PASS` (AC2) | `at-363.mjs`, `at-log.jsonl` ("363 …") |
| Pemberian hak lewat Akses Role | Sudah dibuktikan `BE-HRD-361` 17 November 2026: jabatan gilang diberi `Read` saja → detail `403`, diberi `Read` + `Update` → `200`, lalu dipulihkan identik | `PASS` (AC2) | Laporan `BE-HRD-361` |
| Swagger dokumen `corporate` | `GET summary` 200; `GET /` 200; `GET {id}` 200, 404; `PUT {id}` 200, 400, **403**, 404; tanpa deskripsi endpoint | `PASS` | `at-log.jsonl` ("363 Swagger corporate") |
| Migration | Working tree 0 berkas `Migrations/`; commit source slice `dc723f0f` tidak menyentuh `Migrations/`; slice memakai tabel `WfpSalaryAssignment`, `WfpBankAccount`, `Mst*` yang sudah ada | `PASS` (AC4) | `git status`, `git show --name-only dc723f0f` |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Empat endpoint selaras dengan kontrak v27 (route, DTO, kode status, hak akses) | Terpenuhi sesudah `403` dideklarasikan pada `PUT`; satu ruas tambahan pada respons simpan dicatat untuk penyegaran kontrak | Uji kontrak + Swagger |
| 2. Argumen `[AccessPermission]` = `ControllerName` + nama `[AccessAction]`; hak tampil di Akses Role | Terpenuhi | Uji kontrak; basis data dan katalog Akses Role devYoga; pemberian hak `BE-HRD-361` |
| 3. `dotnet build` 0 galat; QBE preflight dan conformance tanpa pelanggaran | Terpenuhi | `dotnet build`; pemeriksa QBE `Strict` |
| 4. Tidak ada migration, disebut eksplisit | Terpenuhi: **tidak ada migration** pada slice S-A8 dan tidak ada eksekusi database oleh task ini | `git` |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Penyegaran kontrak untuk pass blueprint | API Contract §33.3 `WorkforcePayrollDataSaveResponse`: tambahkan `workforceProfileId` (GUID, gema id dari path) yang sudah ada di source dan dibaca frontend. Kontrak terkunci hash, jadi tidak disunting saat delivery |
| Status slice backend | `BE-HRD-360` s.d. `BE-HRD-363` ✅. Bagian backend gerbang G4 terpenuhi; bagian frontend (`npm run build`, uji browser) milik `FE-HRD-325` s.d. `FE-HRD-327` |
| Risiko tersisa | Dari `BE-HRD-360`: ringkasan Kelengkapan Data Payroll menilai "ada penetapan utama yang disetujui", HRD menilai penetapan utama terbaru (berbeda hanya bila satu orang punya lebih dari satu penetapan utama aktif; devYoga 0 kasus). Dari `BE-HRD-362`: tanpa riwayat versi gaji dan tanpa kontrol konkurensi (keputusan 3) |
| Perubahan sampingan | `NONE`. Probe QBE dihapus; kontrol negatif dipulihkan identik |
| Interupsi | `NONE` |
| Status Git | 1 berkas source diubah (+1 atribut), 1 berkas uji baru, laporan ini (baru), dokumen roadmap/traceability/indeks uji. Tidak ada commit |
| Task berikutnya | Frontend slice S-A8: `FE-HRD-325`, `FE-HRD-326` (🟡), `FE-HRD-327` |
