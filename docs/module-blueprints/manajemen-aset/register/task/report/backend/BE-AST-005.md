# Laporan Perubahan Backend — `BE-AST-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-005` |
| Judul | Service bersama |
| Slice | `REG-S1` Fondasi (gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-005` |
| Trace | `AST-DES-004`, `005`, `016`, `020`; `integration-contract.md` bagian 3 (`INT-REG-20`, `24`, `25`); `02-backend-architecture.md` bagian 3, 4.4, 5.2, 5.4; `validation-matrix.md` `AST-VAL-REG-013`, `015`, `016`, `024`, `070..072`; `AT-REG-46`, `48` |
| Contract version | `AST-REG-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`). Task ini tidak membuat endpoint |
| Dependency | `BE-AST-004` ✅ 2026-11-16 (25 tabel sudah ada di basis data) |
| Klasifikasi | `MEDIUM` — satu repository, 10 berkas, logika otorisasi berbasis data, penyimpanan berkas, alokasi nomor; tanpa kontrak API, tanpa schema baru, tanpa UI |
| Task mode | `BACKEND` — perintah Rizki "lanjutkan" sesudah `BE-AST-004` ✅, 2026-11-16 |
| Target tulis | `Areas/Corporate/AssetManagement/Register/Services/` (7 berkas); `Areas/Corporate/AssetManagement/AssetManagementServiceCollectionExtensions.cs`; `Program.cs` (+3 baris); `Repositories/ApplicationDbContext.cs` (+2 baris); laporan ini; baris status roadmap dan `requirement-traceability.md` sub-modul `register` dan `nilai` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `c2822ab3` (branch `rizkiG`, sama dengan `origin/rizkiG`) |
| Mesin | PC kantor |
| Tanggal | 2026-11-16 |
| Status | ✅ Selesai — keenam acceptance criteria terpetakan ke source; QBE Strict `PASS`; nol baris komentar; `dotnet build` oleh Rizki 16 November 2026 berhasil (DLL 10:47:47 WIB memuat 23 dari 23 penanda). Belum di-commit. **Riwayat:** 🟡 sebelum build |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AssetManagement` (Manajemen Aset & Inventaris) |
| Submodule | `Register` (service bersama untuk kelima sub-modul) |
| Pemilik / prefix registry | `AssetManagement / Register / Register Aset` / `Ast` / `ACTIVE` |
| Keberlakuan | `NEW CODE` (8 berkas baru); `TOUCHED LEGACY` untuk `Program.cs` (+3 baris) dan `ApplicationDbContext.cs` (+2 baris), keduanya aditif |
| QBE ID yang berlaku | `QBE-SVC-001` (tidak ada controller yang menyentuh `ApplicationDbContext`), `QBE-CODE-003`/`QBE-CODE-006` (nomor hanya lewat `NumberSeriesAllocator`, tanpa Count/Max+1), larangan hardcode peran (kewenangan dari `HasAccessAsync` dan data penugasan), `QBE-MOD-002`. Tidak berlaku: `QBE-ENT/CFG` (tanpa entity baru), `QBE-API/PERM/DTO` (tanpa endpoint), `QBE-DB-*` (tanpa migration) |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-AST-004`, kedua puluh lima tabel Aset sudah ada di basis data, tetapi belum ada kode yang boleh
menulisnya. Setiap sub-modul membutuhkan lima kemampuan yang sama:
1. tahu unit mana milik pengguna dan siapa Kepala Unit;
2. menulis linimasa aset;
3. mengunci aset agar tidak masuk dua proses sekaligus;
4. menyimpan lampiran dengan aman;
5. menerbitkan nomor inventaris dan nomor dokumen.

Contoh: sebelum task ini, mutasi monitor dari IGD ke ICU (`pergerakan`) dan usulan penghapusan monitor yang sama
(`penghapusan`) tidak punya cara bersama untuk saling mengetahui. Keduanya bisa berjalan bersamaan untuk aset
yang sama.

---

## 2. Proses bisnis

| Service | Yang dikerjakan | Contoh |
|---|---|---|
| `AssetActorResolver` | Pengguna → akun aktif → profil tenaga kerja → penugasan aktif hari ini (`WfpOrganizationAssignment`) → daftar unit. Penugasan manajerial menjadikan pengguna Kepala Unit unit itu. Penanda `AssetRegister : ReadAllUnits` dibaca lewat `HasAccessAsync` | Perawat kepala IGD dengan penugasan manajerial aktif di IGD: `UnitIds = {IGD}`, `HeadOfUnitIds = {IGD}`. Membuka aset ICU → `403 AST-VAL-REG-016` "Aset ini bukan milik unit Anda." (`AT-REG-46`) |
| `AssetHistoryService` | Satu-satunya penulis `AstAssetHistory`; menambah baris ke transaksi pemanggil tanpa `SaveChanges` | Penyerahan dikonfirmasi → baris `Ditempatkan`, sub-modul `register`, dari "Gudang" ke "IGD-RESUS" |
| `AssetProcessLockService` | Memasang dan melepas `ActiveProcessKind` + `ActiveProcessId` pada aset, mengganti `RowVersion` | Monitor sedang dimutasi. Usulan penghapusan → `409 AST-VAL-REG-024` "Aset AST-ALKMED-000001 sedang dalam proses mutasi. Selesaikan proses itu terlebih dahulu." |
| `AssetAttachmentService` | Menyimpan berkas dengan ekstensi dan ukuran dari `AssetManagement:Attachment`, hash SHA-256, audit unggah/unduh | Pengaturan kosong → `503 AST-VAL-REG-072` (`AT-REG-48`). Foto 12 MB dengan batas 5 MB → `413 AST-VAL-REG-071` |
| `AssetNumberingService` | Nomor inventaris `AST-<KODE>-######` (`AST_INV_<KODE>`, `NEVER`) dan nomor dokumen `AST-PNY`, `AST-KND`, `AST-PMF`, `AST-IMP` lewat `NumberSeriesAllocator` | Persetujuan aset pertama di kelompok `ALKMED` → `AST-ALKMED-000001`. Bila pemanggil gagal sesudahnya, nomor itu hangus dan aset berikutnya mendapat `000002` |

Aturan *fail-closed* pada resolver:

| Keadaan pengguna | Unit yang terlihat |
|---|---|
| Tidak login, klaim `user_id` tidak sah | Tidak ada; `CanReadAllUnits = false` |
| Akun nonaktif, tanpa profil tenaga kerja, atau tanpa penugasan aktif hari ini | Tidak ada (AC 1) |
| Penugasan berakhir kemarin, atau baru mulai besok | Unit itu tidak terhitung |
| Punya penanda `ReadAllUnits` | Semua unit (lewat `CanAccessUnit`), terlepas dari penugasan |

Tanggal penugasan dibandingkan dengan tanggal operasional WIB hari ini, persis `HrdUnitScopeService`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu dan tabel task roadmap; `02-backend-architecture.md` bagian 1, 3, 4.4, 5.2, 5.4, 6; `integration-contract.md`
bagian 3; `validation-matrix.md` bagian 2, 3, 7; `permission-audit-matrix.md`; `acceptance-test-matrix.md`
`AT-REG-46`, `48`; kamus data 3.1, 3.5, 3.7. Preseden source: `NumberSeriesAllocator`, `NumberAllocationRequest`,
`NumberSeriesResetPolicies`, `FinanceReceivableNumberGenerator`; `HrdUnitScopeService`;
`AccessPermissionService.HasAccessAsync`; `AccessExplicitPermissionAttribute` dan
`PermissionRegistryDescriptor` (aturan penanda); `FinanceTransactionProofService`/`Options`;
`NvwResult`/`NvwHttp`; `GlobalExceptionMiddleware`; `BillingManagementServiceCollectionExtensions`;
`ApplicationDbContext.StampEpisodeVersions`; model `AstAsset`, `AstAssetHistory`, `AstAttachment`,
`WfpOrganizationAssignment`, `ApplicationUser`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AssetManagement/Register/Services/AssetManagementException.cs` | Baru — exception modul yang membawa status HTTP + kode validasi, ditambah `AssetRegisterErrorCodes` dan `AssetRegisterMessages` (pesan persis matriks validasi) |
| `.../Register/Services/AssetActorResolver.cs` | Baru — `AssetActorResolver` (cache per request) dan `AssetActor` (`UnitIds`, `HeadOfUnitIds`, `EmployeeId`, `CanReadAllUnits`, `CanAccessUnit`, `IsHeadOfUnit`, `EnsureCanAccessUnit`) |
| `.../Register/Services/AssetHistoryService.cs` | Baru — `Append(AssetHistoryEntry)`, `AssetHistoryEntry`, `AssetSubModules` (`register`, `pergerakan`, `inventarisasi`, `nilai`, `penghapusan`) |
| `.../Register/Services/AssetProcessLockService.cs` | Baru — `Acquire`, `Release`, `IsLocked`, `ProcessName` |
| `.../Register/Services/AssetAttachmentOptions.cs` | Baru — `AllowedExtensions` (bawaan kosong), `MaxFileSizeBytes` (bawaan kosong), `IsConfigured` |
| `.../Register/Services/AssetAttachmentService.cs` | Baru — `SaveAsync`, `GetForOwnerAsync`, `OpenReadAsync`, `AssetAttachmentContent` |
| `.../Register/Services/AssetNumberingService.cs` | Baru — `AllocateInventoryNumberAsync`, `AllocateDocumentNumberAsync`, `NormalizeInventoryCode`, `AssetDocumentNumberSeries` |
| `Areas/Corporate/AssetManagement/AssetManagementServiceCollectionExtensions.cs` | Baru — `AddAssetManagement(IConfiguration)`: opsi `AssetManagement:Attachment` + 5 `AddScoped` |
| `Program.cs` | +1 `using`, +1 baris `builder.Services.AddAssetManagement(builder.Configuration);` (+1 baris kosong) sesudah blok Nurse View |
| `Repositories/ApplicationDbContext.cs` | +2 baris di `StampEpisodeVersions`: `AstAssetHistory` yang `Modified`/`Deleted` ditolak (`"Linimasa aset bersifat append-only."`) |

Seluruh berkas baru **tanpa satu pun baris komentar**.

### 3.3 Keputusan teknis di dalam cakupan

| Keputusan | Alasan |
|---|---|
| Registrasi lewat `AddAssetManagement()` di folder modul, ditambah satu baris pemanggilan di `Program.cs` | Kartu task meminta "registrasi `AddScoped`", dan arsitektur bagian 6 menyebut `Program.cs` diperbarui. Dengan extension, service modul berikutnya (`BE-AST-006..037`) cukup ditambahkan di berkas modul tanpa menyentuh `Program.cs` lagi. Preseden `AddBillingManagement`. Ini menjaga sejauh mungkin aturan Rizki 28 Agt 2026 untuk tidak menambah kode ke `Program.cs` |
| Galat bisnis sebagai `AssetManagementException` (status + kode), bukan tipe hasil seperti `NvwResult` | Kelima service adalah pembantu yang dipanggil service lain di dalam transaksi pemanggil. Exception menghentikan transaksi tanpa setiap pemanggil harus meneruskan hasil. `GlobalExceptionMiddleware` memetakan semua exception ke 500, jadi **controller `BE-AST-006` dan sesudahnya wajib menangkap `AssetManagementException`** dan menjawab `StatusCode` + `Code` + `Message` |
| Deklarasi `[assembly: AccessExplicitPermission("…", "AssetRegister", "ReadAllUnits", …)]` **belum** dibuat | `PermissionRegistryDescriptor` melempar `InvalidOperationException` saat startup bila penanda menunjuk resource yang belum lahir dari controller. Resource `AssetRegister` baru lahir bersama `AssetsController` (`BE-AST-007`). Sampai saat itu `HasAccessAsync` hanya bernilai benar bagi superadmin, sehingga pengguna lain dibatasi ke unitnya sendiri (*fail-closed*). Polanya sama dengan `HrdUnitScopeService` (`BE-HRD-349`) |
| `AssetActor.HeadOfUnitIds` ⊆ `UnitIds` | Penugasan manajerial adalah penugasan aktif juga, sehingga Kepala Unit selalu bagian dari unitnya |
| Resolver hanya memakai unit penugasan langsung, tanpa unit anak | Blueprint menyebut "unit pengguna". Perluasan ke unit anak seperti `HrdUnitScopeService.GetReadScopeAsync` belum diputuskan |
| `Acquire` idempoten untuk proses yang sama; `Release` atas kunci kosong tidak berbuat apa-apa; `Release` oleh proses lain → `409 AST-LOCK-NOT-OWNED` | Proses yang diulang (misalnya retry) tidak gagal karena kuncinya sendiri, dan proses lain tidak dapat melepas kunci yang bukan miliknya |
| `Acquire(..., expectedRowVersion)` opsional → `409 AST-VAL-REG-015` bila berbeda | Kontrak `INT-REG-20` "memeriksa `RowVersion`". Perubahan bersamaan yang lolos pemeriksaan ini tetap tertangkap token konkurensi `RowVersion` saat `SaveChanges` |
| Lampiran: urutan pemeriksaan 503 → berkas kosong → jenis → ukuran | `AT-REG-48` mengharapkan 503 walau berkasnya sah. Jenis berkas ditolak bila ekstensinya tidak diizinkan konfigurasi, diblokir (termasuk `.svg`), tidak punya jenis media kanonik, atau `Content-Type`-nya tidak cocok. Ekstensi yang dikenal: `.pdf`, `.jpg`, `.jpeg`, `.png`, `.xlsx` (impor `BE-AST-012`) |
| Lampiran memanggil `SaveChangesAsync` sendiri, tanpa membuka transaksi | Pola `FinanceTransactionProofService`. Bila pemanggil sudah membuka transaksi, baris ikut transaksi itu. Bila baris gagal disimpan, berkas fisik dihapus. Bila transaksi pemanggil dibatalkan sesudahnya, berkas fisik tertinggal tanpa baris (risiko di bagian 7) |
| Akar penyimpanan `FileStorage:UploadRootPath` (bawaan `uploads/`) + `asset-management/attachments/yyyy/MM/<guid><ext>` | Sama dengan Finance; nama berkas dibangkitkan sehingga tidak ada jalur dari masukan pengguna |
| Kode teknis di luar matriks: `AST-ATT-EMPTY` (400), `AST-ATT-NOT-FOUND` (404), `AST-LOCK-NOT-OWNED` (409) | Matriks tidak punya kode untuk berkas kosong, lampiran tidak ditemukan, dan pelepasan kunci oleh proses lain. **Delta kontrak**: perlu dicatat di `validation-matrix.md` oleh pemilik |
| Penjaga append-only `AstAssetHistory` di `StampEpisodeVersions` | Rekomendasi laporan `BE-AST-003` diputuskan di task ini; `AST-DES-004` menyatakan linimasa tidak pernah diubah atau dihapus. Sekarang dijaga di `SaveChanges`, bukan hanya oleh konvensi |
| Deret dokumen sub-modul lain (`MUT`, `PJM`, `OPN`, `NLI`, `HPS`, `PLH`) belum didefinisikan | Kartu hanya menyebut empat deret register; deret lain ditambahkan task sub-modulnya ke `AssetDocumentNumberSeries` |
| `appsettings*.json` tidak diubah | Lampiran sengaja *fail-closed*. Nilai `AssetManagement:Attachment` diisi administrator per lingkungan (bagian 5) |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tanpa endpoint. Delta kode galat di bagian 3.3 |
| Database | Tanpa migration dan tanpa perubahan schema. Perilaku `SaveChanges` berubah untuk satu tabel: mengubah atau menghapus baris `AstAssetHistory` kini melempar `InvalidOperationException` |
| Keamanan/Auth | Kewenangan seluruhnya dari `HasAccessAsync` dan data penugasan HR; tidak ada nama peran, departemen, posisi, maupun `UserType`. Lampiran: daftar ekstensi diblokir, pencocokan jenis media, SHA-256, jalur dibangkitkan server dan diperiksa tetap di bawah akar, `StoredPath` tidak dikembalikan ke pemanggil. Audit lewat `LoggerService.AuditAsync` tanpa nama berkas asli |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Files evaluated: 10`; `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`; `PASS` | `PASS` | 8 berkas baru + `Program.cs` + `ApplicationDbContext.cs` |
| Nol baris komentar | 0 baris `//`, `///`, `/* */` di 8 berkas baru | `PASS` | `grep` |
| Kolisi nama | 15 nama tipe baru masing-masing satu deklarasi | `PASS` | `grep` |
| Akhiran baris | 8 berkas baru CRLF tanpa BOM; `Program.cs` dan `ApplicationDbContext.cs` tetap CRLF, 0 LF tunggal | `PASS` | Hitung byte lewat Node |
| Review diff dan cakupan | `Program.cs` +3/−0; `ApplicationDbContext.cs` +2/−0; tidak ada `Migrations/`, `*.csproj`, `appsettings*`, atau modul lain yang berubah | `PASS` | `git diff --stat` |
| Tinjauan statis AC ↔ source | 6/6 terpetakan (bagian 6) | `PASS` | — |
| `dotnet build` | Dijalankan Rizki 16 November 2026 dan dilaporkan berhasil. Agent memeriksa hasilnya: `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 10:47:47 WIB, **sesudah** suntingan source terakhir (10:28:40 WIB); tidak ada berkas modul aset, `Program.cs`, atau `ApplicationDbContext.cs` yang lebih baru dari DLL; DLL memuat 23 dari 23 penanda — 15 tipe baru, `AddAssetManagement`, namespace `…Register.Services`, serta literal `Linimasa aset bersifat append-only.`, `AssetManagement:Attachment`, `AST_INV_`, `AST_HANDOVER`, `AST-VAL-REG-024`, `AssetRegister`, `ReadAllUnits` | `PASS` | Build yang gagal tidak menerbitkan DLL baru, jadi DLL ini membuktikan 0 error. **Jumlah warning tidak dilaporkan** |
| Perilaku runtime (`AT-REG-46`, `AT-REG-48`) | — | `NOT RUN` | Butuh endpoint; diuji dari layar bersama `BE-AST-006`/`007` dan FE |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Perintah build untuk pemilik:**

```powershell
dotnet build -p:RunAnalyzers=false
```

**Konfigurasi lampiran** (diisi per lingkungan oleh administrator; contoh bentuk, bukan nilai yang ditetapkan):

```json
"AssetManagement": {
  "Attachment": {
    "AllowedExtensions": [ ".pdf", ".jpg", ".jpeg", ".png", ".xlsx" ],
    "MaxFileSizeBytes": 5242880
  }
}
```

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pengguna tanpa penugasan aktif → daftar unit kosong (fail-closed) | Terpenuhi | `AssetActorResolver.ResolveAsync`: anonim, klaim tidak sah, akun nonaktif, tanpa `WorkforceProfileId`, atau tanpa penugasan yang `IsActive`, `!IsDelete`, punya `OrganizationUnitId`, dan berlaku hari ini → `UnitIds` kosong |
| 2. Penugasan manajerial aktif → Kepala Unit unit itu | Terpenuhi | `HeadOfUnitIds` dari `IsManagerialAssignment` pada penugasan aktif yang sama; `AssetActor.IsHeadOfUnit` |
| 3. Penanda `AssetRegister : ReadAllUnits` dibaca lewat `HasAccessAsync` | Terpenuhi | `HasAccessAsync(user, "AssetRegister", "ReadAllUnits")` → `CanReadAllUnits`, dipakai `CanAccessUnit`/`EnsureCanAccessUnit` (`403 AST-VAL-REG-016`). Deklarasi penanda menyusul `BE-AST-007` (bagian 3.3) |
| 4. Kunci proses menolak `409` bila sudah terpasang | Terpenuhi | `AssetProcessLockService.Acquire` → `409 AST-VAL-REG-024`, pesan persis matriks |
| 5. Lampiran menolak `503` bila pengaturan kosong, `413` bila terlalu besar | Terpenuhi | `AssetAttachmentService.SaveAsync`: `!IsConfigured` → `503 AST-VAL-REG-072`; `Length > MaxFileSizeBytes` → `413 AST-VAL-REG-071` |
| 6. Nomor aset `AST-<KODE>-######` lewat `NumberSeriesAllocator` (`NEVER`) | Terpenuhi | `AllocateInventoryNumberAsync`: `SequenceKey = AST_INV_<KODE>`, `Prefix = AST-<KODE>`, 6 digit, `NumberSeriesResetPolicies.Never`; alokator merakit `{prefix}-{urut}` untuk deret `GLOBAL`. Kode 1–11 `[A-Z0-9]` (awalan ≤ 15 karakter batas alokator). Gagal alokasi → `409 AST-VAL-REG-013` |
| Cakupan: 5 service, opsi `AssetManagement:Attachment`, registrasi `AddScoped` | Terpenuhi | Bagian 3.2 |
| `DoD-BE`: `V-BE` dijalankan dan dicatat apa adanya | Terpenuhi | QBE dan tinjauan oleh agent; `dotnet build` oleh Rizki, diperiksa dari DLL. Uji perilaku endpoint `NOT APPLICABLE` di task ini (tanpa endpoint) |
| `DoD-BE`: laporan, roadmap, traceability, `git status --short`; tanpa commit kecuali diminta | Terpenuhi | Laporan ini; bagian 7 |

Rekap: **6 dari 6 acceptance criteria terpenuhi** dan seluruh butir `DoD-BE` terpenuhi → `✅`.
Riwayat: sebelum build, butir `dotnet build` belum terpenuhi dan status task `🟡`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan tool |
| Masalah yang diketahui | (1) `AST-UNK-004`: apakah HR sudah mengisi penugasan manajerial Kepala Unit belum diketahui. Tanpa data itu, `HeadOfUnitIds` kosong untuk semua orang dan konfirmasi penyerahan/mutasi tidak dapat dilakukan Kepala Unit. Ini masalah data, bukan kode. (2) Delta kode galat `AST-ATT-EMPTY`, `AST-ATT-NOT-FOUND`, `AST-LOCK-NOT-OWNED` belum ada di matriks validasi |
| Risiko tersisa | (1) Jumlah warning build tidak tercatat. (2) Berkas lampiran dapat tertinggal tanpa baris bila transaksi pemanggil dibatalkan sesudah unggah. Tidak bocor (tidak ada baris yang menunjuknya), tetapi memakan ruang; pembersihan berkala belum direncanakan. (3) Sampai `BE-AST-007`, hanya superadmin yang lolos `ReadAllUnits` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | (1) Commit `BE-AST-005`. (2) `BE-AST-006` API master. Controller-nya wajib memetakan `AssetManagementException`. (3) `BE-AST-007` wajib membuat deklarasi `[assembly: AccessExplicitPermission]` untuk `AssetRegister : ReadAllUnits` bersama `AssetsController`. (4) `BE-AST-015` [NLI] kini terbuka (`BE-AST-014` ✅ + `BE-AST-005`) |

`git status --short` di akhir task:

```text
 M Program.cs
 M Repositories/ApplicationDbContext.cs
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/backend-roadmap.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/requirement-traceability.md
 M docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md
 M docs/module-blueprints/nurse-view/roadmap/requirement-traceability.md
 M docs/module-blueprints/nurse-view/task/report/frontend/FE-NV-016.md
?? Areas/Corporate/AssetManagement/AssetManagementServiceCollectionExtensions.cs
?? Areas/Corporate/AssetManagement/Register/Services/
?? docs/module-blueprints/manajemen-aset/register/task/report/backend/BE-AST-005.md
```

Ketiga berkas `nurse-view` sudah berubah sebelum task ini dan bukan milik `BE-AST-005`.
