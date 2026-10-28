# Laporan Perubahan Backend — `BE-NV-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-001` |
| Judul | Fondasi registry, model, configuration |
| Slice | `MVP-0` — fondasi data Nurse View |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-001` |
| Trace | `NV-DEC-010`, `016`, `024`, `028`, `029`; `02-backend-architecture.md` §4.1, §4.2, §5–§8; `data/data-dictionary.md` §2, §4; `G-01` |
| Contract version | `0.1.0` — `approved` (Rizki Gunawan, 2026-10-28) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sederhana (0), kontrak API tidak ada (0), dampak schema/entity (2), auth tidak ada (0), UI tidak ada (0); dibulatkan konservatif karena menyentuh registry bersama |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: registry, `Areas/HealthServices/NurseViewManagement/{Models,Enums}`, `Repositories/Configurations/HealthServices/NurseViewManagement/`, `Repositories/ApplicationDbContext.cs`, laporan dan status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a6f977a3` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 2026-10-28 |
| Status | ✅ **SELESAI** — 2026-10-28. Kode, registry, QBE Strict `PASS`; build, migration `20261028082048_AddNurseViewManagement`, dan `database update` dijalankan Rizki |

---

## 1. Masalah yang diperbaiki

Modul Nurse View belum punya tempat menyimpan data apa pun. Dua hal yang wajib disimpan modul
ini — daftar TV display per lantai dan interval ukur ulang TTV yang disahkan Komite Keperawatan —
belum punya tabel. Selain itu prefix `Nvw` belum terdaftar, sehingga aturan `QBE-MOD-002` akan
menolak model `Nvw*` pertama.

Contoh akibatnya: tanpa tabel perangkat, admin IT tidak bisa mendaftarkan "TV Nurse Station
Lantai 3", sehingga TV itu tidak pernah bisa login dan papan tidak pernah tampil.

---

## 2. Proses bisnis

Task ini hanya membangun fondasi data; belum ada layar atau endpoint. Proses yang dilayaninya
nanti:

1. **Admin IT** mendaftarkan TV untuk satu lantai. Sistem menyimpannya berstatus `PendingLogin`
   (angka `1`) dengan kode unik dari number series, misalnya `NVW-000001`.
2. Admin membuat login → status `Active` (`2`). Bila TV hilang, admin menonaktifkan → `Inactive`
   (`3`), lengkap dengan waktu, pelaku, dan alasan.
3. **Admin IT** memasukkan interval ukur ulang TTV per tingkat EWS **sesudah** Komite
   Keperawatan mengesahkannya, beserta nomor surat dan tanggal pengesahan.

Aturan yang dijaga di tingkat database:

| Aturan | Contoh |
| --- | --- |
| Kode perangkat unik | Dua perangkat `NVW-000001` ditolak database |
| Satu akun login paling banyak satu perangkat | Akun yang sama tidak bisa dipasang ke dua TV |
| Satu interval aktif per tingkat EWS | Dua baris aktif untuk `Medium` ditolak; baris lama yang sudah ditandai hapus tidak dihitung |
| Tingkat EWS hanya 1–4 | `Unknown` (0) ditolak database |
| Interval 1–1440 menit | `0` atau `2000` ditolak database |
| Data diubah dua orang bersamaan | Kolom `Version` menjadi penanda; perubahan yang kalah balapan ditolak saat disimpan |
| Tidak ada angka interval bawaan | Tabel interval lahir kosong (`G-01`) |

Jalur tidak normal (nonaktif, versi bentrok) baru punya layar dan pesan di `BE-NV-002` dan
`BE-NV-004`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `NewQuilvianSystemBackend/AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
  `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- Suite skill: `rules/backend/TASK_RULES.md`, `DATABASE_RULES.md`, `TASK_CLASSIFICATION.md`,
  `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `backend-project-profile.md`
- Pola terdekat: `Areas/HealthServices/HemodialysisManagement/Models/HmdMachine.cs`,
  `Repositories/Configurations/HealthServices/HemodialysisManagement/HmdChecklistItemConfiguration.cs`,
  `HmdMachineConfiguration.cs` (concurrency), `HmdEpisodeConfiguration.cs` (index terfilter),
  `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilAdjustmentConfiguration.cs`
  (check constraint), `Repositories/Configurations/HealthServices/InPatientManagement/InpBedCleaningAttemptConfiguration.cs`
  (FK ke `ApplicationUser`)
- `Repositories/ApplicationDbContext.cs` (`ApplyConfigurationsFromAssembly`, region DbSet)
- `Areas/HealthServices/ClinicalManagement/Enums/EwsRiskLevel.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Baris `HealthServices / NurseViewManagement / Nurse View / BUSINESS DOMAIN / MODULE / Nvw / ACTIVE`; kepanjangan `Nvw = Nurse View`; catatan lifecycle 2026-10-28 berbingkai "propagasi persetujuan yang sudah ada (`NV-DEC-029`), bukan persetujuan baru" |
| `Areas/HealthServices/NurseViewManagement/Enums/NvwDisplayDeviceStatus.cs` | **Baru.** `PendingLogin = 1`, `Active = 2`, `Inactive = 3` |
| `Areas/HealthServices/NurseViewManagement/Models/NvwDisplayDevice.cs` | **Baru.** 15 kolom sesuai kamus data §2.1, mewarisi `IdentityModel`, navigation `LoginUser` |
| `Areas/HealthServices/NurseViewManagement/Models/NvwVitalSignInterval.cs` | **Baru.** 5 kolom sesuai kamus data §2.2; `EwsRiskLevel` memakai enum milik Clinical |
| `Repositories/Configurations/HealthServices/NurseViewManagement/NvwDisplayDeviceConfiguration.cs` | **Baru.** Panjang kolom, enum sebagai integer bawaan `1`, concurrency `Version`, FK `LoginUserId` → `ApplicationUser` `Restrict`, unique `DeviceCode`, unique terfilter `LoginUserId IS NOT NULL`, index `(FloorCode, DeviceStatus)` |
| `Repositories/Configurations/HealthServices/NurseViewManagement/NvwVitalSignIntervalConfiguration.cs` | **Baru.** Check constraint tingkat 1–4 dan menit 1–1440, unique terfilter `EwsRiskLevel` untuk `IsDelete = false` |
| `Repositories/ApplicationDbContext.cs` | `using` modul + region `NURSE VIEW MANAGEMENT` berisi `NvwDisplayDevices`, `NvwVitalSignIntervals` (6 baris tambahan, tanpa baris lain berubah) |

Tidak ada baris komentar pada berkas kode baru, sesuai ketentuan proyek.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum ada endpoint; dimulai `BE-NV-002` |
| Database | Dua tabel baru `public."NvwDisplayDevice"` dan `public."NvwVitalSignInterval"`. Migration `Migrations/20261028082048_AddNurseViewManagement.cs` (+ Designer, snapshot +180 baris) dibuat dan diterapkan Rizki ke DB dev pribadinya, 2026-10-28. DB bersama dan deployment: belum, wewenang terpisah |
| Keamanan/Auth | `NOT APPLICABLE` untuk task ini. FK `LoginUserId` ke `AspNetUsers` disiapkan untuk `BE-NV-002`/`003`; tidak ada kata sandi yang disimpan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <6 berkas>` | Files evaluated 6 · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker, sesi 2026-10-28 |
| Pemeriksaan komentar (`grep "//\|/*"` pada folder baru) | Nol hasil | `PASS` | Keluaran perintah |
| Review diff `ApplicationDbContext.cs` dan registry | Hanya tambahan: 6 + 3 baris | `PASS` | `git diff --stat` |
| `dotnet build -p:RunAnalyzers=false` | Lulus | `PASS` | Dinyatakan Rizki Gunawan, 2026-10-28 (keluaran tidak dilampirkan; pernyataan owner diterima sebagai bukti) |
| `dotnet ef migrations add AddNurseViewManagement` | Berkas `20261028082048_AddNurseViewManagement` terbentuk | `PASS` | Diperiksa agent: `Up()` = 2 `CreateTable` Nvw, 4 `CreateIndex` (2 unik terfilter: `"LoginUserId" IS NOT NULL`, `"IsDelete" = false`), FK `FK_NvwDisplayDevice_AspNetUsers_LoginUserId`, 2 check constraint; `Down()` = 2 `DropTable`; snapshot +180/−0 baris, nol baris modul lain |
| `dotnet ef database update` | Diterapkan | `PASS` | Dinyatakan Rizki Gunawan, 2026-10-28, DB dev pribadi |

Uji manual: `NOT APPLICABLE` — belum ada layar maupun endpoint.

**Tidak dijalankan:** unit test — dikecualikan atas keputusan Rizki Gunawan (ketentuan proyek:
agent tidak menulis/menjalankan unit test).

### Perintah untuk Rizki

Dijalankan dari folder `NewQuilvianSystemBackend`, berurutan:

```bash
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
dotnet ef migrations add AddNurseViewManagement --no-build
dotnet ef database update --no-build
```

**Periksa hasil migration sebelum `database update`:** `Up()` hanya boleh berisi `CreateTable`
untuk `NvwDisplayDevice` dan `NvwVitalSignInterval` beserta index dan check constraint-nya.
Bila muncul tabel atau kolom modul lain, itu tanda snapshot EF tertinggal (pernah terjadi lewat
resolusi merge). Hentikan dan jangan terapkan; migration itu perlu dibersihkan dulu.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 baris registry ada sebelum file model pertama dibuat | **Terpenuhi dengan catatan** | Baris ada di registry. **Catatan jujur:** di dalam sesi, berkas model ditulis beberapa saat sebelum baris registry. Keduanya berada dalam satu perubahan yang belum di-commit, sehingga baris registry hadir bersamaan dengan model pertamanya di setiap commit dan setiap pemeriksaan checker |
| AC-2 kolom, tipe, panjang, bawaan, index sama dengan kamus data | Terpenuhi | §3.2; `data-dictionary.md` §2, §4 |
| AC-3 tidak ada seed angka interval | Terpenuhi | Tidak ada seeder maupun `HasData` |
| AC-4 tidak ada `Trx*` | Terpenuhi | QBE `QBE-NAM-001` lulus |
| DoD — build Rizki lulus | Terpenuhi | Pernyataan Rizki 2026-10-28 |
| DoD — migration dibuat dan diterapkan | Terpenuhi | `20261028082048_AddNurseViewManagement`, diperiksa bersih; diterapkan menurut Rizki |
| DoD — laporan ada | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-ENT-001`, `QBE-ENT-002`, `QBE-NAM-001`, `QBE-NAM-002`,
`QBE-NAM-004`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-CODE-004`
(unique `DeviceCode`). Keberlakuan: `NEW CODE`, kecuali `ApplicationDbContext.cs` = `TOUCHED
LEGACY` (hanya tambahan).

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` ·
pemilik Rizki Gunawan · prefix `Nvw` · registry `ACTIVE` (2026-10-28, `NV-DEC-029`) · applicability
`NEW CODE`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Beberapa catatan lifecycle registry terbaru (`Rad`, `Pat`/Membership, `Mcu`) menyebut Muhammad Hamzah sebagai **pemilik registry**. Baris `Nvw` ditulis pemilik modul sesuai langkah 5 prosedur registry dan preseden `Acc`; sebaiknya Hamzah diberi tahu ada baris baru. Salinan canonical suite skill belum memuat `Nvw` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Tidak ada drift snapshot (diperiksa). Migration belum diterapkan ke DB bersama — wewenang terpisah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan task ini: `M Migrations/ApplicationDbContextModelSnapshot.cs`, `?? Migrations/20261028082048_AddNurseViewManagement{,.Designer}.cs` (dibuat Rizki), `M Repositories/ApplicationDbContext.cs`, `M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `?? Areas/HealthServices/NurseViewManagement/`, `?? Repositories/Configurations/HealthServices/NurseViewManagement/`, `?? docs/module-blueprints/nurse-view/`. Perubahan **milik pengguna sebelumnya**, tidak disentuh: empat berkas `EmergencyInstallationManagement`, `docs/module-blueprints/igd/00-interview-decisions.md`, `igd/task/report/backend/BE-IGD-086.md`, `igd/task/report/frontend/FE-IGD-072.md`, dan dua folder `rawat-inap/*/docs/task-lintas-modul/` (dokumen TLM sesi ini) |
| Langkah berikutnya | `BE-NV-002` dan `BE-NV-004` (boleh paralel) |
