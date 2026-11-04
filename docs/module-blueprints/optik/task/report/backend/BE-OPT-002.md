# Laporan Perubahan Backend — `BE-OPT-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-002` |
| Judul | Skema Optik |
| Slice | `MVP-0` — `EPIC OPT-00` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r1, `approved` 2026-11-03) — Rincian `BE-OPT-002` |
| Trace | `FR-OPT-001`; `OPT-DEC-036`; `erd/` r1, `erd/data-dictionary.md` r1; `OPT-PERM-v1`; `02-backend-architecture.md` bagian 4–7 |
| Contract version | `erd/` r1 dan `OPT-PERM-v1` — `approved` sisi Optik (Yoga Aji Pratama, 2026-11-03). Belum ada endpoint |
| Dependency | `BE-OPT-000` ✅ (registry `Opt` / `ACTIVE`, 2026-11-03) |
| Klasifikasi | `MEDIUM` — 12 entity baru + configuration + migration; tanpa logika bisnis, endpoint, atau perubahan tabel modul lain |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Models,Enums,Constants}/`, `Repositories/Configurations/HealthServices/OpticalManagement/`, `Repositories/ApplicationDbContext.cs`, `Migrations/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`, upstream `origin/yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Migration **dibuat, belum diterapkan** ke basis data mana pun. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `OpticalManagement / Optical` |
| Registry | `Opt` / `ACTIVE` (catatan perubahan 2026-11-03, Muhammad Hamzah) |
| Keberlakuan | `NEW CODE` untuk seluruh berkas `Opt*`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs` (hanya region baru) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-CODE-004`, `QBE-ENUM-001`, `QBE-DEL-001` |
| Wewenang terpisah | Source ✅ diberikan · pembuatan migration ✅ diberikan · eksekusi database ✘ tidak diberikan · deployment ✘ tidak diberikan |

---

## 1. Masalah yang diperbaiki

Modul Optik belum punya tempat menyimpan data apa pun (`OPT-CAP-10`). Pemeriksaan refraksi, resep,
pesanan, keluhan garansi, profil barang, dan pengaturan Optik belum bisa dicatat. Task ini membuat
wadah datanya sesuai kamus data yang sudah disetujui. Belum ada layanan maupun endpoint.

---

## 2. Proses bisnis

Belum ada proses yang berjalan pada task ini. Tabel yang dibuat menjadi fondasi alur
`FLOW-OPT-MVP-001`:

1. Pemeriksaan refraksi (`OptRefractionExam`) dengan hasil ukur berversi (`OptRefractionExamRevision`).
2. Resep (`OptPrescription`) dengan isi berversi (`OptPrescriptionVersion`). Silinder bernotasi minus, dan teks asli resep plus tersimpan.
3. Pesanan (`OptOrder`) dengan baris barang (`OptOrderItem`), pengajuan batal (`OptOrderCancellation`), dan riwayat status (`OptOrderStatusHistory`).
4. Keluhan garansi (`OptWarrantyClaim`) yang menunjuk pesanan asal dan pesanan remake.
5. Profil barang Optik (`OptItemProfile`) dan pengaturan unit (`OptSetting`, riwayat `OptSettingRevision`).

**Contoh aturan yang sudah dijaga basis data:** dua pengajuan batal yang sama-sama `Requested` untuk
satu pesanan ditolak index unik terfilter `UX_OptOrderCancellation_OrderId_Requested`. Satu keluhan
tidak bisa menunjuk dua pesanan remake, karena `UX_OptWarrantyClaim_RemakeOrderId` mencegah satu
pesanan remake dipakai dua keluhan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
`rules/backend/DATABASE_RULES.md`; model dan configuration MCU (`McuOrder`, `McuOrderStatusHistory` dan configuration-nya) sebagai pola;
`Repositories/ApplicationDbContext.cs`; `tooling/migrations/Update-MigrationHistory.ps1` (alat arsip, tidak dipakai);
`erd/data-dictionary.md` r1.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Enums/OpticalEnums.cs` | **Baru.** 13 enum (`02-backend-architecture.md` bagian 4.4) |
| `Areas/HealthServices/OpticalManagement/Constants/OpticalPermissions.cs` | **Baru.** Resource dan action `OPT-PERM-v1`, disalin persis |
| `Areas/HealthServices/OpticalManagement/Models/*.cs` (12 berkas) | **Baru.** 12 entity `Opt*` turunan `IdentityModel`, ber-`[Table]` |
| `Repositories/Configurations/HealthServices/OpticalManagement/*Configuration.cs` (12 berkas) | **Baru.** Enum `HasConversion<int>`, presisi decimal, `Version` sebagai concurrency token, index sesuai kamus data, seluruh FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | **Diperbarui.** Satu `using` dan region `OPTICAL MANAGEMENT` berisi 12 DbSet (bentuk jamak) |
| `Migrations/20261103050001_AddOpticalManagement.cs` + `.Designer.cs` | **Baru.** 12 `CreateTable`, 34 `CreateIndex`, 28 FK `Restrict` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | **Diperbarui.** +1.507 baris, 0 baris terhapus; hanya entity Optik |

Model dan configuration dibangkitkan dari definisi kolom yang sama dengan pembangkit kamus data,
supaya source dan `erd/data-dictionary.md` tidak dapat berbeda.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum ada controller |
| Database | 12 tabel baru di schema `public`; tidak ada tabel modul lain yang diubah. Migration **belum diterapkan** ke basis data mana pun. Penerapan butuh otorisasi terpisah; bila ada migration lain tertunda di basis data tujuan, pakai skrip `--idempotent` |
| Keamanan/Auth | Konstanta izin tersedia untuk task berikutnya; belum ada endpoint yang memakainya. Kolom bertanda Sensitif di kamus data diberi catatan "SENSITIF: jangan masuk logger" pada XML doc model |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet`/`node` berjalan | `PASS` | Keluaran `Get-Process` |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 error; 848 warning, tidak satu pun dari berkas Optik; 2 menit 7 detik | `PASS` | Keluaran build |
| `dotnet ef migrations add AddOpticalManagement --no-build` | Berhasil | `PASS` | `20261103050001_AddOpticalManagement` |
| Isi migration: hanya tabel Optik | 12 `CreateTable`, 34 `CreateIndex`, tidak ada `AlterColumn`/`AddColumn`/`DropColumn` pada tabel lain | `PASS` | Analisis operasi `Up` |
| FK | 28 FK, sama dengan DDL; tidak ada `Cascade` maupun `SetNull` | `PASS` | Analisis `Up` |
| `Down` | Menghapus tepat 12 tabel Optik dalam urutan terbalik FK | `PASS` | Analisis `Down` |
| Snapshot | +1.507 / −0 baris, hanya entity Optik | `PASS` | `git diff --numstat` |
| QBE checker `-Mode Strict` atas 28 berkas | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| QBE checker diulang (pemanggilan terpisah) + kontrol `ZzzRegistryProbe` | Kontrol terkena QBE-MOD-002 dan QBE-CFG-001; 28 berkas Optik tetap bersih | `PASS` | Keluaran checker; probe dihapus, `git status --short Areas/` hanya `OpticalManagement/` |

**Tidak dijalankan:** `dotnet ef database update` atau skrip SQL apa pun (wewenang database tidak
diberikan); uji integrasi harness (belum ada logika service; uji perilaku dimulai di `BE-OPT-004`
dan sesudahnya).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Seluruh index unik dan FK `Restrict` sesuai DDL kamus data | Terpenuhi, dengan delta | 30 index sama dengan DDL + 4 index FK otomatis EF (bagian 7); 28 FK `Restrict` |
| (2) Unique terfilter `OptOrderCancellation` (`Status = 1`) dan `OptWarrantyClaim.RemakeOrderId` | Terpenuhi | `filter: "\"Status\" = 1 AND \"IsDelete\" = false"` dan `filter: "\"RemakeOrderId\" IS NOT NULL"` |
| (3) `OptPrescription.LockedByOrderId` tanpa FK fisik | Terpenuhi | Kolom `uuid NULL` tanpa constraint FK |
| (4) `Down` menghapus 12 tabel tanpa menyentuh tabel lain | Terpenuhi pada isi migration | Analisis `Down`; **belum dijalankan** di basis data (`UAT-27` menunggu izin penerapan) |
| Build `-p:RunAnalyzers=False` lulus | Terpenuhi | 0 error |
| Checker QBE dengan kontrol negatif | Terpenuhi | Bagian 5 |
| Laporan task | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `HasDefaultValue` sengaja **tidak** dipakai untuk kolom decimal bawaan `OptSetting` (50%, 7,50, …). EF menganggap 0 sebagai "tidak diisi", sehingga Admin Optik yang menyetel uang muka 0% akan tersimpan 50%. Nilai bawaan cukup dari initializer model |
| Masalah yang diketahui | **Delta terhadap DDL kamus data (terkunci hash, tidak disunting):** EF menambah 4 index FK otomatis yang tidak tertulis di DDL, yaitu `IX_OptOrder_PrescriptionVersionId`, `IX_OptRefractionExamRevision_RevisedByWorkforceProfileId`, `IX_OptWarrantyClaim_ExaminedByWorkforceProfileId`, dan `IX_OptWarrantyClaim_ReexamRefractionExamId`. Keempatnya hanya mempercepat join dan aman. Penyegaran kamus data dicatat untuk revisi blueprint berikutnya |
| Risiko tersisa | Migration belum pernah dijalankan di PostgreSQL. Kesalahan bentuk baru terlihat saat diterapkan. Disarankan diterapkan ke devYoga lewat skrip `--idempotent` setelah `migrations list` diperiksa, dengan izin pemilik |
| Perubahan sampingan | `NONE`. Probe registry sementara dibuat dan dihapus |
| Interupsi | `NONE` |
| Status Git | `M Migrations/ApplicationDbContextModelSnapshot.cs`, `M Repositories/ApplicationDbContext.cs`, `M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (dari `BE-OPT-000`), `?? Areas/HealthServices/OpticalManagement/`, `?? Migrations/20261103050001_AddOpticalManagement.Designer.cs`, `?? Migrations/20261103050001_AddOpticalManagement.cs`, `?? Repositories/Configurations/HealthServices/OpticalManagement/`, `?? docs/module-blueprints/optik/` |
| Langkah berikutnya | `BE-OPT-004` (nomor dokumen, pembaca izin praktik WIB, seeder pengaturan), lalu `BE-OPT-005` dan `BE-OPT-010`. Penerapan migration ke devYoga sebagai izin terpisah |
