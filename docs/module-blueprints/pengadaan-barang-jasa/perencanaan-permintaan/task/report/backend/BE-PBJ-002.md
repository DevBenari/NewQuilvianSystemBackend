# Laporan Perubahan Backend — `BE-PBJ-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-002` |
| Judul | Kerangka Area Pengadaan |
| Slice | `MVP-0` — fondasi |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-002` |
| Trace | `PBJ-DEC-020`, `PBJ-DEC-031`, `PBJ-DEC-035`, `PBJ-DEC-036`; `02-backend-architecture.md` §4–5, §9; kamus data §5; `PBJ-PP-PERM` 1.1 §1, §4–5 |
| Contract version | Set `1.0`, kecuali `PBJ-PP-PERM` → `1.1` (`approved`, `PBJ-DEC-036`, 12 November 2026) |
| Dependency | Gerbang `PBJ-OQ-002` ✅ (registry `Prc` `ACTIVE`, `PBJ-DEC-035`) |
| Klasifikasi | `MEDIUM` (skor 6: berkas diperiksa 1, berkas diubah 2, logika 1, database 2; keamanan/auth tidak dinilai sebagai skor inti) |
| Task mode | `BACKEND` (diberikan pemilik 12 November 2026) |
| Target tulis | `NewQuilvianSystemBackend`: Area `Corporate/ProcurementManagement`, konfigurasi EF, `ApplicationDbContext.cs`, `Program.cs`, migration `AddProcurementFoundation` (+ Designer, snapshot); laporan dan status blueprint. Basis data dev **tidak** disentuh atas instruksi pemilik |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `100c011d` (working tree belum di-commit; tanpa stage/commit/push) |
| Tanggal | 12 November 2026 |
| Status | 🟡 **SEBAGIAN.** Seluruh kode dan migration ada; build dan QBE `PASS`. Yang belum: skema di basis data sungguhan dan uji runtime, karena migration belum diterapkan (pemilik memilih basis data dev tidak disentuh) dan aplikasi belum bisa start di basis data dev (migration rekan tertunda, lihat `BE-PBJ-001`) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Common` |
| Registry | `Corporate / Procurement | ProcurementManagement / Common / Dokumen Bersama Pengadaan | BUSINESS DOMAIN / MODULE | Prc | ACTIVE` (baris 2026-11-12, `PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE` (Area baru); `TOUCHED LEGACY` hanya untuk `ApplicationDbContext.cs` dan `Program.cs` (penambahan baris) |
| QBE yang berlaku | `QBE-ENT-001` (`IdentityModel`), `QBE-NAM-002` (`Prc` dari registry), `QBE-CFG-001` (konfigurasi EF), `QBE-MOD-001/002/003` (Area terdaftar sebelum model pertama), `QBE-SVC-001` (belum ada controller), `QBE-LOG-001` (riwayat menyertakan aktor), `QBE-AUD-001` (riwayat dokumen terpisah dari logger aplikasi), `QBE-DTO-001` (riwayat dibaca lewat DTO). `QBE-CODE-*` belum berlaku: konstanta nomor dibuat, tetapi alokasinya baru dipakai mulai `BE-PBJ-003` |
| Hasil checker | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode ReportOnly`, 17 berkas: **PASS** (0 violation, 0 review, 0 info) |

## 1. Masalah yang diperbaiki

Modul Pengadaan belum punya tempat di backend. Tidak ada Area, registrasi service, tabel riwayat untuk
jejak audit setiap dokumen (`permission-audit-matrix.md` §5), dan tidak ada aturan bersama "pengguna hanya
melihat dokumen unitnya" (`PBJ-DEC-020`). Tanpa fondasi ini, setiap task dokumen (PR, rencana, pagu) harus
membuatnya sendiri, dan aturan cakupan unit bisa berbeda di tiap layar.

## 2. Proses bisnis

**Riwayat dokumen.** Setiap aksi atas dokumen Pengadaan menambah satu baris riwayat, dalam transaksi yang sama
dengan perubahan dokumennya. Bila transaksi gagal, riwayat juga tidak tercatat.

**Contoh:** PR-000031 diajukan → baris `SUBMIT` (`DRAFT` → `SUBMITTED`). Lalu dikembalikan dengan alasan
"Spesifikasi kurang jelas" → baris `RETURN` (`SUBMITTED` → `RETURNED`, beserta alasannya). Baris tidak pernah
diubah atau dihapus.

**Cakupan unit.** Sebelum menampilkan daftar atau detail, service dokumen bertanya ke
`ProcurementDataScopeService`:
1. Bila pengguna memegang `<resource dokumen> : ReadAllUnits` (`PBJ-DEC-036`), ia melihat semua unit.
2. Bila tidak, ia hanya melihat departemen tempat ia punya penempatan yang sah. Sah artinya aktif, tidak
   dihapus, tidak dibatalkan, dan tanggal berlakunya mencakup hari ini. Aturan ini sama persis dengan jalur
   otorisasi `AccessPermissionService`.
3. Pengguna tanpa penempatan sah tidak melihat dokumen unit mana pun.

**Contoh:** pengguna dengan penempatan aktif di IGD dan Poli (sekunder) melihat PR IGD dan Poli. Penempatan di
Radiologi yang berakhir 31 Oktober 2026 tidak ikut dihitung pada 12 November 2026.

**Jalur tidak normal:** aksi `ReadAllUnits` yang belum didaftarkan (karena controller dokumennya belum ada)
dijawab "tidak berhak" oleh `HasAccessAsync` (*fail-closed*), sehingga pengguna jatuh ke cakupan departemennya
sendiri. SuperAdmin tetap melihat semua unit, mengikuti perilaku `HasAccessAsync` yang sudah ada.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`RhbStatusHistory.cs`, `RhbStatusHistoryWriter.cs`, `RhbStatusHistoryConfiguration.cs`, `RhbNumberSeries.cs`,
`RehabilitationPermissions.cs` (pembanding `NEW CODE` terdekat); `AccessPermissionService.cs` (`HasAccessAsync`,
kelayakan penempatan); `Attributes/AccessExplicitPermissionAttribute.cs` dan contohnya di Radiologi;
`Models/ApplicationUserOrganization.cs`; `ApplicationDbContext.cs`; `Program.cs`; `NumberSeriesResetPolicies.cs`;
fungsi `Resolve-RegistryOwnership` pada checker QBE; snapshot migration.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/ProcurementManagement/ProcurementManagementServiceCollectionExtensions.cs` | **Baru.** `AddProcurementManagement()`: `AddScoped` dua service fondasi |
| `Areas/Corporate/ProcurementManagement/Common/Constants/ProcurementDocumentTypes.cs` | **Baru.** `PBJ_PR`, `PBJ_NEED_PLAN`, `PBJ_PACKAGE_PLAN`, `PBJ_BUDGET`, `PBJ_ITEM`, awalan `PBJ_`, `IsKnown` |
| `.../Common/Constants/ProcurementStatuses.cs` | **Baru.** Kosakata status sesuai `PBJ-PP-STATE` 1.0, plus himpunan "memakan sisa rencana" dan "final" |
| `.../Common/Constants/ProcurementNumberSeries.cs` | **Baru.** Kunci dan awalan `PR`/`RKB`/`RPK`/`KTL`, reset `NEVER`, 6 digit (`PBJ-DEC-031`) |
| `.../Common/Constants/ProcurementWorkflowCodes.cs` | **Baru.** Kode definisi `PBJ-*`, kategori `Procurement`, kode kondisi `PBJ_OVER_BUDGET`/`PBJ_OUT_OF_PLAN` |
| `.../Common/Constants/ProcurementPermissions.cs` | **Baru.** Kode modul `CORPORATE_PROCUREMENT_MANAGEMENT`, nama resource, aksi `ReadAllUnits` |
| `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | **Baru.** Kode aksi riwayat |
| `.../Common/Models/PrcDocumentHistory.cs` | **Baru.** Model sesuai kamus data §5 |
| `.../Common/DTOs/DocumentHistoryDtos.cs` | **Baru.** `DocumentHistoryResponse` (cuplikan `SnapshotJson` tidak diekspos) |
| `.../Common/Services/ProcurementDocumentHistoryWriter.cs` | **Baru.** `Record` (hanya menambah ke change tracker; menolak jenis dokumen tak dikenal, ID kosong, aktor kosong) dan `ReadAsync`. Tidak ada metode ubah/hapus |
| `.../Common/Services/ProcurementDataScopeService.cs` | **Baru.** `ResolveAsync(user, resource)` → `ProcurementDataScope` (`IsAllUnits`, `DepartmentIds`, `CanSee`, `IsEmpty`) |
| `Repositories/Configurations/Corporate/ProcurementManagement/Common/PrcDocumentHistoryConfiguration.cs` | **Baru.** Tabel, panjang kolom, `jsonb`, tiga index |
| `Repositories/ApplicationDbContext.cs` | `using` + `DbSet<PrcDocumentHistory> PrcDocumentHistories` (+4 baris) |
| `Program.cs` | `using` + `builder.Services.AddProcurementManagement();` (+5 baris) |
| `Migrations/20261112130000_AddProcurementFoundation.cs` + `.Designer.cs` | **Baru.** `CreateTable PrcDocumentHistory` + 3 index; `Down` = `DropTable` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +92 baris entri `PrcDocumentHistory` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`: belum ada endpoint. `DocumentHistoryResponse` disiapkan untuk endpoint `GET /{id}/history` milik task dokumen |
| Database | Satu tabel baru `public."PrcDocumentHistory"`, tanpa FK. Migration **dibuat, tidak diterapkan** |
| Keamanan/Auth | Cakupan unit kini satu sumber. **Delta kontrak `PBJ-PP-PERM` 1.0 → 1.1 (`PBJ-DEC-036`):** resource `ProcurementCrossUnit : Read` dicabut karena `AccessExplicitPermission` hanya mendukung aksi pada resource yang sudah punya endpoint. Penggantinya penanda `<resource dokumen> : ReadAllUnits`, didaftarkan oleh `BE-PBJ-006`/`009`/`010`/`011`. Tidak ada nama peran, departemen, atau jabatan di kode |

**Delta penamaan migration:** roadmap menyebut master dan katalog sebagai "bagian" `AddProcurementFoundation`.
Karena satu migration tidak dapat dibagi antar task, migration ini hanya berisi riwayat dokumen; `BE-PBJ-003`
membuat migration sendiri untuk master dan katalog.

## 4. Dokumentasi endpoint

`NOT APPLICABLE`: task tidak membuat endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (perintah baku pemilik) — build 1 | 0 error, 609 warning (3 menit 15 detik); 1 warning baru `CS1570` dari karakter `&` di komentar XML berkas extension | `PASS` (dengan 1 warning baru) | Log build |
| Build 2 (setelah `&` → `&amp;`) | 0 error, 608 warning (sama dengan baseline `BE-PBJ-001`), 0 warning dari berkas Pengadaan; 2 menit 19 detik | `PASS` | Log build |
| EF `migrations script 20261112120000_… 20261112130000_AddProcurementFoundation` | Satu `CREATE TABLE public."PrcDocumentHistory"` (22 kolom, PK) + 3 `CREATE INDEX` + baris riwayat | `PASS` | Keluaran skrip (kriteria skema) |
| Konsistensi snapshot (migration pemeriksa sementara, dibuat lalu dihapus; snapshot dikembalikan dari cadangan) | Tidak memuat `PrcDocumentHistory` maupun `ActivationConditionCode`; sisa selisih = drift HR lama yang sama (200 `AlterColumn`) | `PASS` | Bandingan dengan pemeriksaan `BE-PBJ-001` |
| QBE conformance (17 berkas) | 0 violation / 0 review / 0 info | `PASS` | Laporan checker (`AT-PP-60`) |
| Kriteria 2: riwayat hanya bisa ditambah | `ProcurementDocumentHistoryWriter` hanya punya `Record` dan `ReadAsync` | `PASS` (statis) | Review source |
| Kriteria 3: tanpa penempatan → cakupan kosong | Query penempatan kosong → `Departments(userId, [])` → `IsEmpty = true`, `CanSee` selalu `false` | `PASS` (statis) | Review source |
| Kriteria 4: izin lintas unit | Diganti delta `PBJ-DEC-036`; `ResolveAsync` menanyakan `<resource> : ReadAllUnits`, dan aksi tak terdaftar menghasilkan `false` (`AccessPermissionService` baris `actionAccess == null`) | `PASS` (statis, sesuai kontrak 1.1) | Review source |
| Skema di basis data dan uji runtime | Tidak dijalankan | `NOT RUN` | Pemilik memilih basis data dev tidak disentuh; aplikasi juga belum bisa start di basis data dev (migration rekan `20261110044224` tertunda) |

- **Uji manual:** `NOT FEASIBLE` di lingkungan saat ini.
- **AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Status migration dan basis data

| Butir | Status |
| --- | --- |
| `20261112130000_AddProcurementFoundation` (+ Designer, snapshot) | Dibuat |
| Diterapkan | **Tidak** (instruksi pemilik: basis data dev tidak disentuh) |
| Urutan | Setelah `20261112120000_AddWorkflowStepActivationCondition` (sudah diterapkan di dev). Database dulu, lalu aplikasi: model memuat `DbSet<PrcDocumentHistory>`, tetapi belum ada kode yang membacanya saat startup |

## 7. Risiko yang tersisa

| Risiko | Penanganan |
| --- | --- |
| Pengguna dengan banyak penempatan di unit berbeda melihat dokumen semua unit penempatannya | Disengaja: mengikuti aturan otorisasi yang ada (penempatan sekunder ikut dihitung) |
| Migration rekan yang tertunda di basis data dev menghalangi uji runtime seluruh task Pengadaan | Perlu dirapikan pemilik basis data dev; dicatat di `MODULE-STATUS.md` |
| Salinan registry di suite skill belum memuat baris `Prc` | Penyelarasan oleh pemegang registry suite |

## 8. Task berikutnya

`BE-PBJ-003` (katalog dan master) dan `BE-PBJ-004` (lampiran) sekarang dapat dikerjakan, dan boleh paralel.
