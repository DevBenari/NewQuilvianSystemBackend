# Laporan Perubahan Backend — `BE-RWI-185`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-185` |
| Judul | Bentuk data master Workspace PPRI dan saringan penutupan episode (`E9`) |
| Slice | Slice A — Fondasi data dan bacaan cetak; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-185` |
| Trace | `FR-RWA-030`, `050`, `126`; `RWI-DEC-241` butir 3, `243`, `247`; `INV-RWA-12`; `RWI-AC-361`; backend 13.9, 13.11, 13.12; data 20.15, 20.16, 20.18; API 12.1 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | — |
| Klasifikasi | `HEAVY` — data master bersama, migration, perilaku penutupan episode |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/MasterData/{Enums,Models}`, configuration `MasterData`, `InpDischargeService.Closure.cs`, `InpSettingService.cs`, migration |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS`; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MasterData` (pemilik master), dibaca `InPatientManagement` |
| Prefix registry | `Mst` — `ACTIVE`; `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (`MstInpatientClearanceItem` dengan `SortOrder` lama; tidak ada `SortOrder` baru), `NEW CODE` (dua enum) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-DEL-001` |
| Wewenang | Source: ya. Build dan migration: dijalankan **pengguna** (keputusan 8 Oktober 2026) |
| Temuan preflight | Salinan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` di repo lebih baru daripada salinan bawaan skill (baris Finance, `Mst` mencakup Corporate, `Gzi`/`Gz`). Preflight memakai salinan repo; `Mst` dan `Inp` `ACTIVE` di keduanya |

---

## 1. Kebutuhan

Workspace PPRI memakai master butir administrasi yang sama dengan daftar periksa penutupan episode
untuk butir Serah Terima Pasien Baru (`STPB-*`), dan pengaturan Rawat Inap perlu menampung kode
formulir, kota penandatanganan, batas umur gelang bayi, serta kode label. Tanpa pembeda jenis,
butir serah terima wajib akan ikut menahan penutupan **setiap** episode.

## 2. Proses bisnis

1. Butir lama otomatis berjenis `EpisodeClosure` (bawaan basis data `1`), sehingga daftar periksa
   penutupan tidak berubah.
2. Butir `NewPatientHandover` boleh punya induk (sub-butir 02A–02C) dan sumber saran sistem; butir
   penutupan tidak boleh keduanya (`CK_MstInpatientClearanceItem_Type`).
3. Daftar dan penandaan butir penutupan episode hanya membaca butir `EpisodeClosure`. Menandai
   butir serah terima lewat endpoint penutupan → `404` "Butir administrasi tidak ditemukan.".
4. Pengaturan lama mendapat `InfantWristbandMaxAgeYears = 5`; isian cetak lain kosong sampai diisi
   admin atau seeder non-produksi (`BE-RWI-186`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstInpatientClearanceItem.cs`, `MstInpatientSetting.cs` beserta configuration; `InpDischargeService.Closure.cs`;
`InpSettingService.cs`; kamus data 20.15, 20.16, 20.18; backend 13.9, 13.11, 13.12.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MasterData/Enums/MstClearanceChecklistType.cs` | Baru — `EpisodeClosure = 1`, `NewPatientHandover = 2` |
| `MasterData/Enums/MstHandoverSuggestionSource.cs` | Baru — `None = 0` s.d. `WristbandPrinted = 6` |
| `MasterData/Models/MstInpatientClearanceItem.cs` | `ChecklistType`, `ParentItemId`, `HandoverSuggestionSource`, navigasi `ParentItem` |
| `Repositories/.../MasterData/MstInpatientClearanceItemConfiguration.cs` | `CK_MstInpatientClearanceItem_Type`, konversi `int` + bawaan, index (`ChecklistType`, `IsActive`) dan (`ParentItemId`), FK diri sendiri `Restrict` |
| `MasterData/Models/MstInpatientSetting.cs` | Sebelas isian: delapan kode formulir, `DocumentSigningCity`, `InfantWristbandMaxAgeYears` (5), `PatientLabelHospitalCode` |
| `Repositories/.../MasterData/MstInpatientSettingConfiguration.cs` | Panjang maksimum 50/100/30; bawaan `5` |
| `InPatientManagement/Services/InpDischargeService.Closure.cs` | Saringan `ChecklistType == EpisodeClosure` pada `GetClearanceChecklistAsync` dan `MarkClearanceItemAsync` |
| `InPatientManagement/Services/InpSettingService.cs` | `InpatientSettingValues` membawa sebelas nilai baru; batas umur di luar 0–16 jatuh ke 5 |
| `Migrations/20261008055604_AddWorkspacePpriAdmissionDocuments.cs` (+ `.Designer.cs`, snapshot) | Bagian `E9` (dibuat pengguna lewat `dotnet ef migrations add`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.1: perilaku daftar/penandaan penutupan disaring jenis; bentuk respons tetap |
| Database | 3 kolom `MstInpatientClearanceItem`, 11 kolom `MstInpatientSetting`, 1 FK, 1 `CHECK`, 2 index. Diterapkan pengguna 8 Oktober 2026 |
| Keamanan/Auth | Tidak berubah |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{episodeId}/clearance` | Hanya butir `EpisodeClosure` | `InpatientDischarge : Read` (tetap) |
| `POST` | `/{episodeId}/clearance/{itemId}/mark` | Butir jenis lain → `404` "Butir administrasi tidak ditemukan." | `InpatientDischarge : Update` (tetap) |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Prefix `Mst`/`Inp` `ACTIVE`; tidak ada `SortOrder` baru; FK `Restrict` | `PASS` | Review source |
| `dotnet build` (pengguna) | `QuilvianSystemBackend succeeded with 250 warning(s) (261,7s)`, 0 error | `PASS` | Output pengguna 8 Oktober 2026, lihat 5.1 |
| `dotnet ef migrations add AddWorkspacePpriAdmissionDocuments --no-build` (pengguna) | `Done.` | `PASS` | Output pengguna |
| Tinjauan migration terhadap DDL 20.18 | Kolom, tipe, bawaan, nullability, FK, `CHECK`, index sama; delta nama FK lihat 5.1 | `PASS` | Source migration |
| `dotnet ef database update` (pengguna) | `Applying migration '20261008055604_AddWorkspacePpriAdmissionDocuments'.` → `Done.` | `PASS` (maju); rollback `NOT RUN` | Output pengguna; log `%TEMP%\ef-update.log` |
| Regresi alur penutupan episode, API daftar/penandaan | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | Pengguna menguji mandiri dengan agent lain |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

### 5.1 Bukti build dan migration bersama (8 Oktober 2026)

Bukti ini berlaku untuk seluruh batch `BE-RWI-185` s.d. `189` dan `192` s.d. `202`; laporan task lain merujuk ke sini.

1. **Build.** Pengguna menjalankan `dotnet build` sesudah seluruh kode batch selesai (DLL pukul 12.43 WIB; perubahan kode terakhir 12.30 WIB). Hasil: `succeeded with 250 warning(s)`, **0 error**. Restore memberi 5 warning `NU1902`/`NU1903` paket `SixLabors.ImageSharp` 3.1.11 yang sudah ada. Tempelan output terpotong di 50.000 karakter, sehingga daftar warning tidak diterima utuh. Warning yang terbaca seluruhnya berasal dari berkas lama. Ada atau tidaknya warning dari berkas baru batch ini **tidak terverifikasi**.
2. **Percobaan pertama `database update` gagal.** Saat itu file migration belum dibuat, karena langkah `migrations add` belum sempat jalan ketika sesi agent terputus. Teks error tidak diterima karena tempelan terpotong. Penyebabnya diselesaikan dengan langkah 3.
3. **Migration.** `dotnet ef migrations add AddWorkspacePpriAdmissionDocuments --no-build` → `Done.`; lahir `Migrations/20261008055604_AddWorkspacePpriAdmissionDocuments.cs` dan `.Designer.cs`; snapshot diperbarui.
4. **Penerapan.** `dotnet ef database update` → `Build succeeded.` → `Applying migration '20261008055604_AddWorkspacePpriAdmissionDocuments'.` → `Done.` Basis data pengembangan milik pemilik; nama basis data dan connection string tidak dicatat.
5. **Tinjauan isi migration** terhadap DDL 20.18: hanya perubahan `E9` dan `E10`, tanpa perubahan lain yang ikut terbawa; `Down()` simetris.
   - **Delta 1.** `E9` dan `E10` digabung dalam **satu** migration atas instruksi pengguna ("migration hanya sekali"). Urutan di dalam `Up()` tetap `E9` → `E10`.
   - **Delta 2.** Empat nama FK dipotong EF menjadi 63 karakter berakhiran `~`, sesuai batas identifier PostgreSQL:
     - `FK_InpAdmissionCostDifferenceStatement_InpAdmissionDocument_Do~`
     - `FK_InpAdmissionDocumentSignature_InpAdmissionDocument_Document~`
     - `FK_InpAdmissionHandoverItem_MstInpatientClearanceItem_Clearanc~`
     - `FK_MstInpatientClearanceItem_MstInpatientClearanceItem_ParentI~`
6. **Seeder.** Pengguna menyalakan `Seeders__RunInpatientMasterDataSeed=true`; hasil seeder saat aplikasi dinyalakan belum dilaporkan (`NOT RUN` dari sisi agent).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Satu migration `E9` sesuai kamus data, punya `Down()` | Terpenuhi dengan delta | Bagian `E9` di `20261008055604_…` sama dengan DDL 20.18; digabung dengan `E10` (delta 1); `Down()` ada, rollback `NOT RUN` |
| 2. Butir lama `EpisodeClosure`; daftar periksa penutupan sama | Terpenuhi (source + migration) | `defaultValue: 1`; saringan hanya menyempitkan ke jenis lama. Regresi runtime dikecualikan atas keputusan pengguna |
| 3. Butir `NewPatientHandover` tidak muncul; `mark` → `404` | Terpenuhi (source) | `InpDischargeService.Closure.cs` dua titik saringan. Uji runtime dikecualikan |
| 4. Pengaturan lama `InfantWristbandMaxAgeYears = 5`, isian lain kosong | Terpenuhi (migration diterapkan) | `defaultValue: 5`; kolom lain `nullable: true` |
| 5. Tidak ada `SortOrder` baru | Terpenuhi | Migration tidak memuat `SortOrder` |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Langkah mundur: nonaktifkan butir `STPB-*` sebelum memundurkan kode (backend 13.12) |
| Masalah yang diketahui | — |
| Risiko tersisa | Rollback `Down()` belum dijalankan |
| Perubahan sampingan | — |
| Interupsi | Sesi agent terputus saat build; build dan migration kemudian dijalankan pengguna |
| Status Git | `??` dua enum, migration baru; `M` model dan configuration master, `InpDischargeService.Closure.cs`, `InpSettingService.cs`, snapshot |
| Langkah berikutnya | Verifikasi runtime oleh pengguna; admin mengisi butir serah terima produksi |
