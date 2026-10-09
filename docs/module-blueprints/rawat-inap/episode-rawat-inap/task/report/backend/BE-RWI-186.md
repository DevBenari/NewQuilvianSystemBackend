# Laporan Perubahan Backend — `BE-RWI-186`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-186` |
| Judul | Endpoint master butir serah terima dan pengaturan cetak, data awal non-produksi |
| Slice | Slice A; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-186` |
| Trace | `FR-RWA-030`, `126`; `RWI-DEC-048`, `241`, `243`, `247`; `VAL-RWA-50` s.d. `55`; `RWI-AC-360`, `368`; API 12.4; validation 15.7; backend 13.8.4, 13.13 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-185` ✅, `BE-RWI-192` ✅ |
| Klasifikasi | `MEDIUM` — endpoint master yang ada, validasi baru, seeder |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/MasterData/{DTOs,Services,Controllers,Seeders}`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS`, migration diterapkan ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Verifikasi API/seeder runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MasterData` |
| Prefix registry | `Mst` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (service, controller, seeder yang ada) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001` |
| Wewenang | Source: ya. Menjalankan seeder: pengguna |

---

## 1. Kebutuhan

Admin perlu mengelola jenis, induk sub-butir, dan sumber saran butir administrasi, serta sebelas isian
cetak. Lingkungan pengembangan/UAT butuh 18 butir `STPB-*` dan nilai cetak V1 supaya Serah Terima
dapat diuji. Nilai V1 (kode formulir, kota) hanya boleh ada di seeder (`RWI-AC-368`).

## 2. Proses bisnis

1. Daftar, opsi, dan ringkasan butir menerima `checklistType` (kosong = semua). Respons memuat jenis,
   nama induk, dan sumber saran.
2. Simpan butir ditolak bila:
   - induknya bukan butir utama berjenis sama (`422 MST-ICI-001`);
   - butir penutupan punya induk atau sumber saran (`422 MST-ICI-002`);
   - jenis butir yang sudah dipakai penandaan atau dokumen serah terima diubah (`422 MST-ICI-003`);
   - sumber saran sudah dipakai butir aktif lain (`409 MST-ICI-004`).
3. Layar lama yang tidak mengirim isian baru tetap berhasil. Butir baru menjadi `EpisodeClosure`; ubah
   butir mempertahankan jenis, induk, dan sumber yang tersimpan.
4. Pengaturan menerima sebelas isian opsional: `null` = pertahankan, `""` = kosongkan. Batas umur
   gelang bayi di luar 0–16 ditolak `400 MST-IST-001`, isian terlalu panjang `400 MST-IST-002`.
5. Seeder hanya berjalan bila `Seeders:RunInpatientMasterDataSeed` menyala dan menolak `Production`.
   Ia mengisi nilai cetak V1 hanya pada isian kosong, dan menyisipkan 18 butir `STPB-01` s.d. `15`
   serta `02A` s.d. `02C` (induk `STPB-02`) per kode, idempoten.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientClearanceItemService`/`Controller`, `InpatientSettingService`/`Controller`, `InpatientMasterDataSeeder`,
`Program.cs` (alur seeder startup), validation 15.7, backend 13.13.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MasterData/DTOs/InpatientClearanceItemDtos.cs` | Isian jenis/induk/sumber pada respons dan request; opsi enum pada metadata; `ChecklistType` pada saringan bawaan |
| `MasterData/Services/InpatientClearanceItemService.cs` | Saringan `checklistType`; `ValidateTypeRulesAsync`, `ValidateSuggestionSourceAsync`; kode `MST-ICI-001` s.d. `004`; status `BusinessRuleRejected` (422) dan `Conflict` (409) |
| `MasterData/Controllers/InpatientClearanceItemController.cs` | Query `checklistType`; kode alasan dibawa pada `errors` |
| `MasterData/DTOs/InpatientSettingDtos.cs` | Sebelas isian respons dan request |
| `MasterData/Services/InpatientSettingService.cs` | `ValidatePrintFields` (`MST-IST-001`, `002`), `KeepOrReplace` |
| `MasterData/Controllers/InpatientSettingController.cs` | Pemetaan isian baru dan kode alasan |
| `MasterData/Seeders/InpatientMasterDataSeeder.cs` | `SeedPrintSettingValuesAsync`, `SeedNewPatientHandoverItemsAsync` (18 butir, `SortOrder` 10–150), penghitung hasil |
| `Program.cs` | Pemanggil seeder dipulihkan (`SeedInpatientMasterDataAsync` + `RunStartupSeederAsync`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | API 12.4 (aditif) |
| Database | Tidak ada perubahan skema di task ini; data awal hanya non-produksi |
| Keamanan/Auth | Hak akses tetap (`InpatientClearanceItem`, `InpatientSetting`) |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Inpatient Clearance Item

Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/`, `/options`, `/summary` | Query `checklistType?`; respons + jenis, induk, sumber saran | `InpatientClearanceItem : Read` |
| `GET` | `/{id}` | Respons + tiga isian | `InpatientClearanceItem : Read` |
| `POST` | `/` | Request + `ChecklistType?` (bawaan `EpisodeClosure`), `ParentItemId?`, `HandoverSuggestionSource?` | `InpatientClearanceItem : Create` |
| `PUT` | `/{id}` | Sama; `MST-ICI-003` | `InpatientClearanceItem : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan butir bersumber saran yang sudah dipakai → `409 MST-ICI-004` | Tetap |

#### Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Respons + sebelas isian | `InpatientSetting : Read` |
| `PUT` | `/{id}` | Request + sebelas isian opsional; `MST-IST-001`, `002` | `InpatientSetting : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Service pemilik `DbContext`; controller tanpa query | `PASS` | Review source |
| Pencarian nilai V1 di luar seeder | Kode formulir V1 dan kota "Jakarta" hanya di `InpatientMasterDataSeeder.cs` | `PASS` | Pencarian source 8 Oktober 2026 |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| API master dan seeder runtime | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | Pengguna menyalakan `Seeders__RunInpatientMasterDataSeed`; hasil belum dilaporkan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `checklistType = NewPatientHandover` hanya butir serah terima, dengan nama induk | Terpenuhi (source) | Saringan pada daftar/opsi/ringkasan; `ParentItemName` |
| 2. `MST-ICI-001` s.d. `004` | Terpenuhi (source) | `ValidateTypeRulesAsync`, `ValidateSuggestionSourceAsync`; `003` memeriksa `InpClearanceMark` dan `InpAdmissionHandoverItem` |
| 3. Batas umur 0–16 dan panjang isian; `GET` sebelas isian | Terpenuhi (source) | `ValidatePrintFields`; DTO respons |
| 4. Seeder `Development` menyisipkan 18 butir dan nilai V1 hanya bila kosong; `Production` menolak | Terpenuhi (source) | `SeedNewPatientHandoverItemsAsync`, `SeedPrintSettingValuesAsync`; penolakan produksi lama tetap. Run dikecualikan |
| 5. Permintaan lama tanpa isian baru tetap berhasil, butir `EpisodeClosure` | Terpenuhi (source) | Request nullable; `Update` mempertahankan nilai tersimpan |
| DoD build | Terpenuhi | Build pengguna 0 error |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pemanggil seeder master Rawat Inap sempat **tidak ada** di `Program.cs` (kunci konfigurasi ada sejak `BE-RWI-002` tetapi tidak dibaca); dipulihkan di task ini, bawaannya tetap mati |
| Masalah yang diketahui | Kontrak tidak selaras: state 10.5 menyebut ubah jenis butir terpakai `422 MST-ICI-004`, sedangkan validation 15.7 (sumber kode resmi) menetapkan `MST-ICI-003`, dan `MST-ICI-004` = sumber saran ganda `409`. Kode mengikuti validation 15.7; state 10.5 perlu diselaraskan pemilik desain |
| Risiko tersisa | Seeder yang menemukan sumber saran sudah terpakai menyisipkan butir tanpa sumber dan mencatat warning |
| Perubahan sampingan | — |
| Interupsi | `NONE` |
| Status Git | `M` enam berkas `MasterData`, `InpatientMasterDataSeeder.cs`, `Program.cs` |
| Langkah berikutnya | Jalankan aplikasi sekali dengan seeder menyala di lingkungan pengembangan; admin mengisi butir produksi |
