# Laporan Perubahan Backend — `BE-BD-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-021` |
| Judul | Blood Unit Gate Projection |
| Slice | 3 — Kantong darah, pemberian, dan penyelesaiannya (prasyarat backend `FE-BD-005`) |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) — kartu `BE-BD-021` |
| Trace | `DEC-BD-013/027/028/038/042`; `INV-BD-030`; `VAL-BD-017/018/019/020/020b/065/066/079`; kewajiban layar `FE-BD-008`, `FE-BD-013`, `FE-BD-021` pada acceptance `FE-BD-005`; backlog "proyeksi gerbang pemberian" pada [laporan `FE-BD-005`](../frontend/FE-BD-005.md) bagian 8; keputusan pemilik `R1`–`R5` 25 September 2026 (bagian 2) |
| Contract version | api-contract `v5` (`approved`, `Sukmagp` 19 September 2026) + Amendment **`D7`** (25 September 2026, aditif; nomor set kontrak tidak dinaikkan) |
| Dependency | `BE-BD-007` ✅, `BE-BD-008` ✅, keputusan pemilik `R1`–`R5` ✅ 25 September 2026 |
| Klasifikasi | `LIGHT` — satu service disentuh (satu fungsi baca + dua `return` evaluator), tiga berkas DTO aditif; nol endpoint, nol entity, nol migration, nol hak akses |
| Task mode | `BACKEND` — backend target tulis; frontend referensi read-only |
| Target tulis | `NewQuilvianSystemBackend`: source `Areas/HealthServices/BloodBankManagement/**`; dokumen `docs/module-blueprints/bank-darah/**` (laporan, kartu roadmap atas perintah pemilik, amandemen kontrak, matriks acceptance, traceability) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | Basis `fcda77b1` (`docs(bank-darah): close FE-BD-008 validation evidence`), branch `sukmagp`. Belum di-commit |
| Tanggal | 25 September 2026 |
| Status | ✅ **SELESAI — 25 September 2026.** Ketujuh acceptance `AC-BD-125`..`AC-BD-131` terpenuhi. Build `0 Error(s)` / `214 Warning(s)` (sama dengan baseline); `has-pending-model-changes` bersih, nol migration; QBE Strict `PASS` (4 berkas, `VIOLATION 0`); validasi runtime R0–R13 **15/15 `PASS`** lewat HTTP sungguhan terhadap `QuilvianNewDevSukma`, dengan ekspektasi SQL independen. **Batas bukti:** aktor tunggal `superadmin`; dua asersi skrip (R9, R12) dibetulkan sesudah run tanpa mengulang aksi akhir (bagian 5.2). Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BloodBankManagement` |
| Pemilik / prefix registry | `Bbk` — Blood Bank, lifecycle **`ACTIVE`** (registry baris 30, aktivasi 3 September 2026) |
| Keberlakuan | `TOUCHED LEGACY` pada `BbkBloodUnitService.cs` (disentuh sempit); `NEW CODE` untuk dua DTO baru |
| QBE yang berlaku | `QBE-API-001` (boundary dan envelope `ApiResponse<T>` tetap), `QBE-DTO-001` (DTO publik terpisah; record evaluator internal tidak diekspos — keputusan `R2`), `QBE-MOD-002` (tidak ada entity baru, tidak terpicu) |
| Branch | `sukmagp`, working tree bersih saat mulai |
| Governance terbaca | `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/TASK_RULES.md`, `REPORT_TEMPLATE.md` (suite skill `1.18.0`) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, layar detail kantong tidak tahu apakah kantong **boleh diberikan** sampai petugas
menekan tombol **Berikan** lalu ditolak. Penilaiannya hanya ada di dalam backend
(`EvaluateIssuanceGateAsync` dan `EvaluateEmergencyBypassAsync`), dan `GET /blood-units/{id}` tidak
memulangkannya. Akibatnya bagi petugas:

- Bukti kecocokan yang sudah **kedaluwarsa** baru ketahuan sesudah Berikan ditekan (`VAL-BD-020`).
- Hasil **Tidak cocok** tidak menutup tombol Berikan sebelum percobaan pertama ditolak (`VAL-BD-079`).
- Pada jalur darurat, petugas harus menebak gerbang mana yang dilewati. Tebakan yang salah ditolak
  `VAL-BD-066`.

Contoh: kantong `TEST-BD007-20260914171359-01` punya bukti Cocok pukul 02.29 UTC 16 September. Komponennya
berlaku 24 jam, jadi bukti itu habis pukul 02.29 UTC 17 September. Sebelum task ini, layar pada 25 September
tetap menawarkan Berikan seperti biasa. Sesudahnya, detail kantong langsung memulangkan
`validationCode = VAL-BD-020` dan `validUntil = 2026-09-17T02:29:35.431`.

Frontend dilarang menghitung sendiri (keputusan pemilik `FE-BD-005` no. 3), sehingga tiga butir acceptance
`FE-BD-005` — `FE-BD-008`, `FE-BD-013`, `FE-BD-021` — tertahan data yang tidak dikirim backend.

---

## 2. Proses bisnis

### 2.1 Keputusan pemilik, 25 September 2026

Sesudah audit read-only pada sesi yang sama, `Sukmagp` memutuskan:

| No | Keputusan |
| --- | --- |
| `R1` | Proyeksi **hanya** untuk kantong berstatus `Allocated`: `issuanceGate` dan `emergencyBypass`. Status lain → keduanya `null`. Aturan bisnis tidak diubah |
| `R2` | DTO publik terpisah `BloodUnitIssuanceGateDto` dan `BloodUnitEmergencyBypassDto`; record evaluator internal tidak diekspos |
| `R3` | `ValidUntil` nullable, hanya terisi ketika evaluator memang menghitungnya; `null` berarti tidak dihitung |
| `R4` | `IsCurrentStorageLocationActive` **tidak** diubah; selisih definisinya dicatat sebagai technical debt; tidak membuka task baru |
| `R5` | Tanpa `requiredBypassScope`; frontend memetakan `EvidenceGateClosed` dan `LocationGateClosed` sendiri |
| — | Scope: tanpa migration, tanpa `VAL-BD` baru, tanpa perubahan `AvailableActions`, tanpa perubahan perilaku `issue`/`emergency-issue`. Sesudahnya: build, validasi runtime, laporan, roadmap. Tidak di-commit sebelum validasi final |

### 2.2 Alur yang dilihat petugas

1. Petugas Bank Darah membuka detail kantong berstatus **Dialokasikan**.
2. Backend menilai gerbang pemberian **dengan fungsi yang sama** yang dipakai tombol Berikan, lalu
   memulangkan hasilnya pada `issuanceGate`:
   - terbuka → `isOpen = true`, bukti yang akan dipakai, dan `validUntil` (batas berlaku bukti);
   - tertutup → kode dan pesan penahan pertama, persis pesan yang akan muncul bila Berikan ditekan.
3. Pada waktu yang sama backend menilai **kedua** gerbang jalur darurat secara terpisah dan memulangkannya
   pada `emergencyBypass`. Contoh: kulkas nonaktif dengan bukti yang masih berlaku menghasilkan
   `locationGateClosed = true`, `evidenceGateClosed = false`, sehingga cakupan darurat yang sah hanya
   "lokasi penyimpanan tidak aktif".
4. Saat Berikan atau Jalur Darurat ditekan, backend **menilai ulang**. Proyeksi hanya petunjuk. Bila keadaan
   berubah di antaranya, misalnya bukti habis masa berlakunya selagi layar terbuka, tindakan tetap ditolak
   dengan kode yang sama seperti sebelum task ini.

### 2.3 Urutan penilaian gerbang normal (tidak berubah)

| Urutan | Syarat | Kode bila gagal | `validUntil` |
| ---: | --- | --- | --- |
| 1 | Status `Allocated` | `VAL-BD-017` | `null` |
| 2 | Punya penempatan di lokasi aktif | `VAL-BD-065` | `null` |
| 3 | Punya alokasi aktif ke pasien | `VAL-BD-017` | `null` |
| 4 | Ada bukti yang belum gugur | `VAL-BD-018` | `null` |
| 5 | Ada bukti untuk pasien tujuan | `VAL-BD-019` | `null` |
| 6 | Komponen punya masa berlaku bukti | `VAL-BD-020b` | `null` |
| 7 | Bukti terbaru pasien menyatakan Cocok | `VAL-BD-079` | `null` |
| 8 | Bukti belum lewat `checkedAt + masa berlaku` | `VAL-BD-020` | **terisi** (lampau) |
| — | Seluruhnya terpenuhi | terbuka | **terisi** (masa depan) |

Penilaian berhenti pada penahan pertama. Karena itu kantong di kulkas nonaktif memulangkan `VAL-BD-065`
tanpa `validUntil`, walaupun buktinya juga bermasalah. Keadaan buktinya tetap terbaca pada
`emergencyBypass.evidenceGateClosed`.

### 2.4 Pemetaan jalur darurat (`R5`)

| `evidenceGateClosed` | `locationGateClosed` | `bypassScope` yang diterima |
| :---: | :---: | --- |
| `true` | `false` | `CompatibilityEvidence` (`0`) |
| `false` | `true` | `InactiveStorageLocation` (`1`) |
| `true` | `true` | `Both` (`2`) |
| `false` | `false` | Tidak ada — jalur normal terbuka; setiap cakupan ditolak `VAL-BD-066` |

### 2.5 Jalur tidak normal

| Keadaan | Hasil |
| --- | --- |
| Kantong bukan `Allocated` (Diterima, Tersedia, Diberikan, Menunggu keputusan, dst.) | Kedua isian `null` (`R1`). Tanpa aturan ini, penilaian jalur darurat — yang memang tidak memeriksa status — akan menyatakan "gerbang bukti tertutup" pada kantong yang sudah Diberikan |
| Detail dibaca lewat respons aksi (`POST /compatibility-evidence`, `/allocate`, `/cancel-allocation`, dst.) | Kedua isian ikut terbawa, karena respons sukses aksi dibangun dengan fungsi detail yang sama |
| Keadaan berubah sesudah detail dibaca | Tindakan menilai ulang; kode dan pesan penolakan tidak berubah dari sebelum task ini |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Source: `BbkBloodUnitService.cs` (`GetDetailAsync`, `EvaluateIssuanceGateAsync`,
  `EvaluateCompatibilityEvidenceGateAsync`, `EvaluateEmergencyBypassAsync`, `IssueAsync`,
  `EmergencyIssueAsync`, `AvailableActionsFor`), `BbkBloodUnitController.cs`, `BloodUnitDtos.cs`,
  `CompatibilityEvidenceDtos.cs`, `EmergencyAuthorizationDtos.cs`, `BloodUnitAllocationDtos.cs`,
  enum `BbkEmergencyBypassScope`, `BbkEmergencyAuthorizerRole`.
- Blueprint: laporan `FE-BD-005`, `BE-BD-007`, `BE-BD-008`, `BE-BD-020`; `contracts/api-contract.md` grup
  Blood Unit; `testing/acceptance-test-matrix.md`; `roadmap/backend-roadmap.md`;
  `roadmap/requirement-traceability.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/DTOs/CompatibilityEvidenceDtos.cs` | Record internal `BloodUnitIssuanceGateResult` mendapat parameter opsional terakhir `DateTime? ValidUntil = null` (pemanggil lama tidak berubah). DTO publik baru `BloodUnitIssuanceGateDto` |
| `Areas/HealthServices/BloodBankManagement/DTOs/EmergencyAuthorizationDtos.cs` | DTO publik baru `BloodUnitEmergencyBypassDto` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` | `BloodUnitDetailDto` mendapat `IssuanceGate` dan `EmergencyBypass` (nullable) |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | (a) `GetDetailAsync`: pada kantong `Allocated` memanggil kedua evaluator yang sudah ada lalu memetakan hasilnya ke DTO publik; status lain `null`. (b) `EvaluateCompatibilityEvidenceGateAsync`: variabel `validUntil` yang **sudah** dihitung diteruskan pada dua `return` yang menghitungnya (`VAL-BD-020` dan terbuka). Urutan, kondisi, kode, dan pesan tidak disentuh |
| `docs/.../contracts/api-contract.md` | Amendment `v5` **`D7`** pada grup Blood Unit; baris `last_changed_in` |
| `docs/.../testing/acceptance-test-matrix.md` | Bagian 14 baru, `AC-BD-125` sampai `AC-BD-131`; baris `last_changed_in` |
| `docs/.../roadmap/backend-roadmap.md` | Kartu `BE-BD-021` baru — atas perintah pemilik ("update roadmap: kartu `BE-BD-021`"), mengikuti pola kartu `BE-BD-020` |
| `docs/.../roadmap/requirement-traceability.md` | Bukti `BE-BD-021` pada baris bukti kecocokan dan jalur darurat; baris `AC-BD-125`..`131` |
| `docs/.../task/report/backend/BE-BD-021.md` | Laporan ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** `BloodUnitDetailDto` memperoleh dua isian nullable, pada `GET /{id}` dan pada setiap respons sukses aksi kantong yang memulangkan `BloodUnitDetailDto`. Nol endpoint baru, nol isian dihapus/diganti nama, request dan kode galat tidak berubah. Amendment `D7` |
| Database | **Nol** entity, konfigurasi EF, maupun migration. Hanya query baca tambahan (sekitar enam) pada detail kantong `Allocated`. `has-pending-model-changes` bersih; tidak ada migration yang dibuat maupun diterapkan |
| Keamanan/Auth | **Nol** `[AccessAction]`/`[AccessPermission]` baru atau berubah. Isian baru hanya memuat data yang sudah terbaca oleh `BloodUnit : Read` (pasien tujuan pada `CurrentAllocation`, id bukti pada `CompatibilityEvidences`, kode dan pesan yang sama dengan respons `422`) |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Unit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-units/{id}` | Detail kantong; **kini** membawa `issuanceGate` dan `emergencyBypass` pada kantong Dialokasikan | `BloodUnit : Read` |
| `POST` | `/{id}/compatibility-evidence`, `/{id}/allocate`, `/{id}/cancel-allocation`, `/{id}/issue`, `/{id}/emergency-issue`, `/{id}/storage-location`, dan aksi lain yang memulangkan detail | Respons sukses ikut membawa kedua isian; perilaku aksi tidak berubah | Tidak berubah |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>/out` | **`Build succeeded`** — **`0 Error(s)`, `214 Warning(s)`**, `01:16:54` | `PASS` | Jumlah peringatan sama dengan baseline `BE-BD-017`..`020`. Tiga `CS1573` pada `BbkBloodUnitService.cs` berada di record `BloodUnitResult` lama (barisnya bergeser karena sisipan task ini), **bukan** di baris task. Nol peringatan dari ketiga berkas DTO |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | Sah karena `git diff --name-only` memuat nol berkas `Models/`, `Configurations/`, `Migrations/`, maupun DbContext. Log `HostAbortedException` adalah perilaku normal tool EF saat membangun host |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (WorkingTree) | 4 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0` | `PASS` | `Final result: PASS` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test terpisah pada repository (sesuai `AGENTS.md`) |
| Validasi runtime R0–R13 (15 pemeriksaan) | **15 dari 15 `PASS`** sesudah dua asersi skrip dibetulkan (bagian 5.2) | `PASS` | Bagian 5.1–5.2 |

Uji manual: `PASS` — dijalankan langsung agent lewat HTTP sungguhan (bagian 5.1).

**Tidak dijalankan:** jalur `403` dengan aktor tanpa `BloodUnit : Read` (tidak ada akun non-SuperAdmin yang
kredensialnya tersedia bagi agent; task ini tidak mengubah atribut hak akses mana pun). Keadaan
`VAL-BD-017` karena alokasi aktif hilang pada kantong `Allocated` tidak dapat dibentuk tanpa merusak data
lewat SQL, sehingga tidak dijalankan; penilaiannya tidak disentuh task ini.

### 5.1 Cara validasi runtime

Aplikasi hasil build dijalankan dari folder output scratchpad pada `http://localhost:5217`
(`ASPNETCORE_ENVIRONMENT=Development`) terhadap **`QuilvianNewDevSukma`**. Autentikasi memakai cookie sesi
`superadmin` hasil `POST /api/v1/Auth/login`; kredensial seed dibaca dari konfigurasi Development tanpa
dicetak. Cookie bertanda `secure`, sehingga klien uji mengirimnya sebagai header `Cookie` lewat `http`
lokal. Ekspektasi — status, bukti terbaru, masa berlaku komponen, pasien alokasi aktif, dan
`checkedAt + masa berlaku` — dihitung **lebih dulu** dan independen lewat SQL baca, lalu dibandingkan
dengan respons API.

### 5.2 Hasil validasi runtime — 25 September 2026

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; `GET /{id}` tanpa cookie | Login `200`; tanpa cookie `401` | — | `PASS` |
| R1 | Satu kantong untuk setiap status bukan `Allocated` yang ada di database: `Received`, `Available`, `Issued`, `PendingReview`, `Reallocated`, `ReturnedToProvider`, `NotUsable` | Ketujuhnya `issuanceGate = null` dan `emergencyBypass = null`. `availableActions` tetap seperti semula (misalnya `Issued` → `RequestIssuanceCorrection`; `PendingReview` → `MoveStorageLocation`, `ResolveReallocate`, `ResolveReturn`, `ResolveNotUsable`) | `AC-BD-126`, `AC-BD-131` | `PASS` |
| R2 | `TEST-BD006-20260914094559-02`, belum ada bukti | `VAL-BD-018`, pesan "Bukti pemeriksaan kecocokan belum tercatat…", `compatibilityEvidenceId`/`validUntil` `null`. Bypass: bukti tertutup, lokasi terbuka, pasien = alokasi aktif (SQL) | `AC-BD-125`, `AC-BD-127` | `PASS` |
| R3 | `TEST-BD007-20260914171359-05`, komponen `TBD007-N` tanpa masa berlaku | `VAL-BD-020b`, `validUntil` `null`. Bypass: bukti tertutup | `AC-BD-127` | `PASS` |
| R4 | `TEST-BD007-20260914171359-03`, bukti terbaru Tidak cocok | `VAL-BD-079`, `compatibilityEvidenceId` = bukti terbaru (SQL), **`validUntil` `null`** | `AC-BD-127` | `PASS` |
| R5 | `TEST-BD007-20260914171359-01`, bukti Cocok `2026-09-16T02:29:35.431Z`, komponen 24 jam | `VAL-BD-020`, **`validUntil = 2026-09-17T02:29:35.431Z`** — sama persis dengan SQL `checkedAt + 24 jam`, di masa lampau | `AC-BD-127` | `PASS` |
| R6 | Bukti Cocok dicatat pada kantong yang sama, `checkedAt` = satu jam lalu | `POST /compatibility-evidence` `200`; **respons POST itu sendiri** membawa `isOpen = true`, pesan "Gerbang pemberian terbuka.", bukti baru, **`validUntil = 2026-09-26T02:30:19Z`** (= `checkedAt + 24 jam`). Bypass: kedua gerbang terbuka, `validCompatibilityEvidenceId` = bukti baru | `AC-BD-125`, `AC-BD-127` | `PASS` |
| R7a | Kulkas `TBD007-LOCY` dinonaktifkan (`PATCH /status` `200`); kantong `-01` dengan bukti berlaku | `VAL-BD-065`, `compatibilityEvidenceId`/`validUntil` `null` (penilaian berhenti di lokasi). Bypass: **bukti terbuka, lokasi tertutup**, `validCompatibilityEvidenceId` tetap bukti R6 | `AC-BD-128` | `PASS` |
| R7b | Kulkas yang sama; kantong `-03` dengan bukti Tidak cocok | `VAL-BD-065`. Bypass: **bukti tertutup dan lokasi tertutup** | `AC-BD-128` | `PASS` |
| R8 | `POST /issue` pada `-01` dalam keadaan R7a | `422`, `errors.code = VAL-BD-065`, **pesan identik** dengan `issuanceGate.message` | `AC-BD-129` | `PASS` |
| R9 | `POST /emergency-issue` pada `-03` (R7b): cakupan `0`, lalu cakupan hasil pemetaan kedua boolean | Cakupan `0` → `422 VAL-BD-066`. Pemetaan `(true, true)` → `2` → **`200`**. Kantong **Diberikan** (`unitStatus 4`), `issuedViaEmergency = true`, satu otorisasi berlingkup `2` (SQL); proyeksi sesudahnya `null` | `AC-BD-130`, `AC-BD-126` | `PASS`* |
| R10 | Kulkas diaktifkan kembali (`200`, `IsActive = true` diverifikasi SQL); `-01` | Proyeksi `isOpen = true` → `POST /issue` **`200`**; `CompatibilityEvidenceIdUsed` = bukti R6 (SQL); proyeksi sesudahnya `null` | `AC-BD-129` | `PASS` |
| R11 | `POST /issue` pada kantong berproyeksi `018` (`TBD006-…-02`), `020b` (`TBD007-…-05`), `079` (`TBD010-…-04`) | Ketiganya `422` dengan kode yang sama dan **pesan identik** dengan `issuanceGate.message`. Versi ketiga kantong tidak berubah (SQL: tetap `3`) | `AC-BD-129`, `AC-BD-131` | `PASS` |
| R12 | `TEST-BD007-20260914171359-04` (bukti Cocok milik pasien `1967de99…`): `cancel-allocation` (`TBD007-BATALALOK`), lalu `allocate` ke `ORD-00000084` milik pasien `e8b91912…` | Batal `200` → **Tersedia** (`unitStatus 2`), proyeksi `null`. Alokasi `200` → respons membawa **`VAL-BD-019`**, bypass bukti tertutup dengan `patientId` = pasien baru | `AC-BD-127`, `AC-BD-126` | `PASS`* |
| R13 | Pada keadaan R12: `POST /issue`; `POST /emergency-issue` cakupan `1` (lokasi, padahal lokasi aktif) | Issue `422 VAL-BD-019`, pesan identik proyeksi. Emergency `422 VAL-BD-066` — sesuai pemetaan (`true, false` → hanya cakupan `0` yang sah) | `AC-BD-129`, `AC-BD-130` | `PASS` |

\* **Riwayat percobaan, apa adanya.** Run pertama berhenti di R1 karena cookie `secure` tidak dikirim
cookiejar lewat `http` (`401`); klien uji dibetulkan. Pada run kedua, R9 dan R12 tercatat `FAIL` oleh
**asersi skrip yang keliru**: skrip mengira `Issued = 5` dan `Available = 4`, padahal enum
`BbkBloodUnitStatus` menetapkan `Available = 2`, `Issued = 4`, `PendingReview = 5`. Respons API yang
tertangkap justru `unitStatus 4` (R9) dan `2` (R12), dan SQL sesudah run mengonfirmasi: `-03` Diberikan
lewat jalur darurat dengan satu otorisasi berlingkup `Both`, `-04` berstatus `Allocated` kembali dengan
alokasi ke pasien baru. Seluruh syarat lain pada kedua skenario terpenuhi pada run yang sama. Kedua skenario
**tidak** diulang, karena pemberian bersifat akhir dan keadaan yang dibuktikan sudah terekam.

### 5.3 Keadaan database sesudah run

| Data | Sebelum | Sesudah |
| --- | --- | --- |
| `TEST-BD007-20260914171359-01` | Dialokasikan, v5, 1 bukti | **Diberikan jalur normal**, v7, 2 bukti (bukti Cocok R6 dipakai) |
| `TEST-BD007-20260914171359-03` | Dialokasikan, v3, 1 bukti | **Diberikan jalur darurat**, v4, 1 otorisasi (`Both`, Dokter Bank Darah, `TBD008-DARURAT`) |
| `TEST-BD007-20260914171359-04` | Dialokasikan ke `ORD-00000083`, v7 | **Dialokasikan ke `ORD-00000084`** (pasien lain), v9; alokasi lama `Cancelled` |
| `TEST-BD006-…-02`, `TEST-BD007-…-05`, `TEST-BD010-…-04` | Dialokasikan, v3 | Tidak berubah — penolakan `422` tidak mengubah data |
| Lokasi `TBD007-LOCY` | Aktif | **Aktif** kembali (diverifikasi SQL); `UpdateDateTime`/`UpdateBy` terisi oleh `PATCH` |

Seluruhnya kantong uji `TEST-`, mengikuti perlakuan data uji `FE-BD-005` dan `BE-BD-020`: dibiarkan dan
dicatat.

---

## 6. Acceptance criteria dan Definition of Done

Rumusan lengkap ada pada [matriks acceptance](../../../testing/acceptance-test-matrix.md) bagian 14.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-125` — proyeksi pada kantong `Allocated`, nilainya sama dengan penilaian gerbang | **Terpenuhi** | R2–R7, R12: nilai sesuai ekspektasi SQL; keduanya dari evaluator yang sama dengan tindakannya (diff bagian 3.2) |
| `AC-BD-126` — `null` di luar `Allocated` (`R1`) | **Terpenuhi** | R1 (tujuh status), R9 dan R10 (sesudah Diberikan), R12 (sesudah batal alokasi) |
| `AC-BD-127` — kode gerbang bukti dan `validUntil` (`R3`) | **Terpenuhi** | `018` R2, `019` R12, `020b` R3, `079` R4, `020` R5 (lampau, tepat), terbuka R6 (masa depan, tepat); `validUntil` `null` pada kode lain |
| `AC-BD-128` — lokasi nonaktif | **Terpenuhi** | R7a (bukti terbuka, lokasi tertutup) dan R7b (keduanya tertutup) |
| `AC-BD-129` — sejalan dengan `issue` | **Terpenuhi** | R8 (`065`), R11 (`018`/`020b`/`079`), R13 (`019`): `422` dengan kode dan pesan identik; R10: terbuka → `200` memakai bukti yang sama |
| `AC-BD-130` — sejalan dengan `emergency-issue` (`R5`) | **Terpenuhi** | R9: cakupan hasil pemetaan `200`, cakupan lain `066`; R13: cakupan yang tidak sesuai pemetaan `066` |
| `AC-BD-131` — tanpa regresi | **Terpenuhi** | Diff: `AvailableActionsFor`, `IssueAsync`, `EmergencyIssueAsync`, controller, dan atribut hak akses tidak disentuh; urutan/kondisi/kode/pesan evaluator tidak berubah (hanya `validUntil` diteruskan). Nol migration (`has-pending-model-changes` bersih), nol `VAL-BD` baru. R1 `availableActions` tetap; R11 penolakan tanpa perubahan data |
| DoD: build | **Terpenuhi** | `0 Error(s)` / `214 Warning(s)` |
| DoD: runtime validation | **Terpenuhi** | R0–R13, 15/15 `PASS` (bagian 5.2) |
| DoD: laporan, kontrak, roadmap | **Terpenuhi** | Laporan ini; Amendment `D7`; bagian 14 matriks acceptance; kartu roadmap; traceability |
| DoD: tidak di-commit sebelum validasi final | **Terpenuhi** | Belum ada commit; menunggu instruksi pemilik |

**Batas bukti.** Aktor tunggal `superadmin`; jalur `403` tidak ditembakkan karena tidak ada atribut hak
akses yang berubah. `VAL-BD-017` di dalam proyeksi tidak dibentuk (bagian 5).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Proyeksi adalah **petunjuk, bukan izin**: nilainya potret saat detail dibaca. `issue` dan `emergency-issue` tetap menilai ulang, jadi frontend tetap wajib menangani `422` (misalnya bukti yang lewat `validUntil` selagi layar terbuka) |
| Masalah yang diketahui | **Technical debt (`R4`, tidak dibuka sebagai task):** `isCurrentStorageLocationActive` pada detail memakai `IsActive && !IsDelete`, sedangkan gerbang lokasi juga memeriksa `!IsCancel`. Pada lokasi yang dibatalkan tetapi masih aktif, penanda bisa tampil "aktif" sementara `emergencyBypass.locationGateClosed = true`. Acuan gerbang adalah `locationGateClosed`. **Temuan dokumen:** tabel ringkasan status (bagian 3) dan grafik dependency (bagian 4) `backend-roadmap.md` tidak memuat `BE-BD-019`, `BE-BD-020`, maupun `BE-BD-021`; ketiganya hanya ada sebagai kartu. Tidak diubah task ini — penyusunannya milik perencanaan roadmap. **Lingkungan:** scheduler absensi HR gagal menyisipkan `HrdAttendanceProcessingRun` saat aplikasi uji berjalan; tidak terkait Bank Darah, nol galat pada endpoint kantong |
| Risiko tersisa | Sekitar enam query baca tambahan per detail kantong `Allocated` (gerbang bukti dinilai dua kali, sekali per evaluator), demi menjaga satu sumber aturan. Tidak ada pada status lain. Butir golongan darah (`VAL-BD-034`) pada `FE-BD-005` tetap gap terpisah dan tidak dijawab task ini |
| Perubahan sampingan | `NONE` di repository. Alat bantu sekali pakai (skrip SQL/HTTP, output build, log aplikasi) berada di scratchpad sesi, di luar repository. Di database: data uji pada bagian 5.3 |
| Interupsi | Dua monitor build berakhir oleh batas waktu 30 menit sebelum build selesai; dipasang ulang tanpa mengulang build. Run runtime pertama gagal di klien uji (cookie `secure`), dibetulkan lalu dijalankan penuh sekali |
| Status Git | `M` tiga berkas DTO, `BbkBloodUnitService.cs`, `api-contract.md`, `acceptance-test-matrix.md`, `backend-roadmap.md`, `requirement-traceability.md`; `??` laporan ini. Belum di-stage atau di-commit |
| Langkah berikutnya | (1) Pemilik meninjau diff lalu memberi instruksi commit. (2) Task frontend lanjutan `FE-BD-005` memakai `issuanceGate`/`emergencyBypass` untuk `FE-BD-008`, `FE-BD-013`, dan `FE-BD-021`; butir golongan darah tetap menahan ✅ penuh `FE-BD-005` |
