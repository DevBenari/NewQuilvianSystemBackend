# LGS-BE-ADH-03 — Penyempurnaan persetujuan permintaan barang unit (LGS-DEC-002)

- TASK ID: `LGS-BE-ADH-03` (task ad-hoc). Lanjutan `LGS-BE-ADH-02` atas instruksi pemrakarsa "penyempurnaan sebelum fitur dinyatakan selesai", 17 November 2026
- TASK MODE: BACKEND (dinyatakan eksplisit oleh pemrakarsa)
- WRITE TARGET: `NewQuilvianSystemBackend`, worktree `C:\tmp\phm`, branch `Ikbal`, HEAD `d946b20b`
- MODEL: Claude Opus 5.5
- KETERBERLAKUAN: NEW CODE di modul existing `PharmacyManagement` (registry `Phm`, `ACTIVE / LEGACY`); tidak ada entity, tabel, maupun migration baru. QBE: QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-DTO-001, QBE-VAL-001

## File

Baru:

- `Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalAuthority.cs`: kewenangan atasan unit dan izin CITO eksplisit
- `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs`
- `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests.cs`: 34 test

Diubah:

- `Services/StockRequestService.cs`: konstruktor menerima `IConfiguration?` (opsional, tanpa perubahan registrasi DI); kewenangan domain di `DecideAsync`; aturan peralihan di `FulfillAsync`, `GetPagedAsync`, `GetDetailAsync`; saringan `FulfillableOnly`
- `DTOs/StockRequestDtos.cs`: `StockRequestPagedQuery.FulfillableOnly`; ringkasan `IsLegacySubmission`; detail `HasUnitAuthority`, `UnitAuthorityMessage`, `HasEmergencyAuthority`
- `Controllers/StockRequestController.cs`: dokumentasi endpoint saja

Tidak diubah: `AccessPermissionService`, `AccessPermissionFilter`, `appsettings*.json`, kebijakan SuperAdmin.

## 1. Otorisasi CITO yang tidak dapat dilewati peran SuperAdmin

Masalah: filter `[AccessPermission]` meloloskan SuperAdmin selama
`Security:Authorization:EnforceClinicalPolicyForSuperAdmin = false` (nilai di `appsettings.json`).

Perbaikan tanpa mengubah kebijakan global: `DecideAsync` memanggil
`StockRequestApprovalAuthority.HasEmergencyGrantAsync`. Pemeriksaan ini membaca jalur kebijakan
yang sama dengan `AccessPermissionService.HasAccessAsync`: penempatan jabatan pengguna
(`ApplicationUserOrganization`, aktif, tidak batal, masih berlaku) × `SysAccessPolicy` (diizinkan,
aktif) untuk aksi `StockRequest:AuthorizeEmergency`. Bedanya, pemeriksaan ini:

- tidak punya jalan pintas SuperAdmin,
- tidak punya jalan pintas aksi system-only,
- tidak ikut saklar `Security:Authorization:Enabled`.

Akibatnya:

| Pengguna | Filter controller | Pemeriksaan domain | Hasil |
| --- | --- | --- | --- |
| SuperAdmin tanpa jabatan berizin | lolos | tolak | `403` |
| SuperAdmin dengan jabatan yang diberi izin | lolos | lolos | boleh |
| Petugas dengan jabatan yang diberi izin | lolos | lolos | boleh |
| Petugas tanpa izin | tolak | — | `403` |
| Pemohon walau berizin | lolos | tolak (larangan memutus sendiri) | `403` |

Karena belum ada jabatan yang diberi izin ini, jalur CITO tertutup bagi semua orang sampai
`LGS-OQ-021` dijawab.

## 2. Kewenangan atasan per unit dari struktur organisasi yang sudah ada

Struktur yang dipakai, tanpa tabel baru dan tanpa relasi karangan:

```text
PhmStockRequest.RequestingServiceUnitId
  → MstServiceUnit.OrganizationUnitId              (kolom yang sudah ada)
    → MstOrganizationUnit (aktif)
      ← WfpOrganizationAssignment                   (penempatan HR)
          OrganizationUnitId = unit itu
          IsManagerialAssignment = true
          aktif, tidak batal, berlaku saat ini
          WorkforceProfileId = ApplicationUser.WorkforceProfileId
```

Penyaring masa berlaku sama dengan alur persetujuan HR
(`WorkflowService.ResolveByOrganizationAssignmentAsync`). Hanya satu tingkat: atasan unit
organisasi induk tidak ikut berwenang.

Bila tautan atau penempatan belum ada, jawabannya tidak berwenang beserta sebabnya:

| Sebab | Pesan |
| --- | --- |
| Unit layanan belum ditautkan | "Unit layanan {nama} belum ditautkan ke unit organisasi, sehingga atasan unitnya belum dapat ditentukan…" |
| Unit organisasi tidak aktif | "Unit organisasi yang ditautkan ke {nama} tidak aktif." |
| Akun tanpa profil tenaga kerja | "Akun Anda belum terhubung ke profil tenaga kerja…" |
| Bukan atasan unit | "Anda tidak tercatat sebagai atasan unit {nama}…" |

`WfpManagerAssignment` (atasan langsung per pegawai) juga ada di skema, tetapi tidak dipakai:
itu atasan pemohon, bukan atasan unit. Pilihan ini dibuka untuk dikonfirmasi pada `LGS-OQ-020`.

## 3. Persetujuan butuh izin dan kewenangan unit sekaligus

- Izin `StockRequest:Approve`: filter controller (kebijakan platform, tidak diubah).
- Kewenangan atas unit peminta: `DecideAsync`, untuk `approve` dan `reject`. Pemeriksaan ini
  berlaku juga bagi SuperAdmin dan tidak ikut saklar pengembangan.
- Larangan memutus permintaan sendiri tetap diperiksa lebih dulu.
- `GET /{id}` mengisi `hasUnitAuthority`, `unitAuthorityMessage`, dan `hasEmergencyAuthority`
  dengan aturan yang sama, supaya layar menjelaskan sebab penolakan sebelum tombol ditekan.

## 4. Aturan peralihan untuk permintaan `Submitted` lama

Audit data (baca saja, 2026-11-17):

| Basis data | Permintaan | Submitted | Keterangan |
| --- | --- | --- | --- |
| `QuilvianNewDevMaster` (tim) | 0 | 0 | Tabel kosong |
| `QuilvianNewDevIkbalFr` (lokal) | 1 | 0 | Satu permintaan berstatus Selesai, 28 Oktober 2026 |

Aturan yang diterapkan, dengan konfigurasi `PharmacyManagement:StockRequest:ApprovalEnforcedFromUtc`:

| Keadaan | Penyerahan langsung | Diputus atasan |
| --- | --- | --- |
| Batas tidak diatur | Tidak; wajib disetujui | Ya |
| Diajukan sebelum batas | Ya; riwayat mencatat Submitted → Completed | Ya |
| Diajukan sesudah batas | Tidak; wajib disetujui | Ya |
| Nilai batas tidak dapat dibaca | Service gagal dibuat (`InvalidOperationException`), bukan diabaikan diam-diam | — |

Permintaan lama diberi tanda `isLegacySubmission` dan ikut tampil di antrean gudang lewat
`fulfillableOnly=true`. Aturan dan nilai batas per lingkungan menunggu `LGS-OQ-023`.

## 5. Konfigurasi Development yang mematikan pemeriksaan izin

- `appsettings.Development.json` (di-ignore git, hanya di mesin ini) berisi
  `Security:Authorization:Enabled = false`.
- `AccessPermissionService` mengabaikan saklar itu hanya di Production. **Staging atau lingkungan
  non-Production lain ikut terlepas bila nilainya diset false di sana.**
- Dengan saklar itu, filter `StockRequest:Approve` tidak diperiksa sama sekali di mesin ini.
  Kewenangan atasan unit dan izin CITO eksplisit tetap berlaku, karena diperiksa di service.
- **Hasil uji manual dengan konfigurasi ini bukan bukti keamanan untuk izin controller.** Untuk
  membuktikannya, jalankan dengan pemeriksaan aktif, misalnya variabel lingkungan
  `Security__Authorization__Enabled=true`, tanpa mengubah berkasnya.

## 6. Test backend (`StockRequestApprovalTests`, 34 test)

| Kelompok | Test |
| --- | --- |
| Kewenangan atasan unit | atasan unit menyetujui dan riwayat mencatat pelaku/waktu/status; menolak beralasan; staf tanpa penempatan manajerial ditolak; atasan unit lain ditolak; penempatan berakhir ditolak; unit layanan belum ditautkan ditolak; SuperAdmin tanpa penempatan atasan ditolak; detail menjelaskan kewenangan |
| Otorisasi CITO | petugas dengan izin eksplisit berhasil; SuperAdmin saja ditolak; atasan unit tanpa izin CITO ditolak; izin dari penempatan berakhir ditolak; CITO untuk permintaan rutin `PHM017`; alasan kosong `PHM015` |
| Larangan memutus sendiri | pemohon yang juga atasan unit; pemohon yang memegang izin CITO; atasan yang membuatkan permintaan |
| Keputusan ganda dan konkurensi | keputusan kedua `PHM018` (setuju/tolak/CITO), satu riwayat Disetujui; kunci sama tidak menggandakan; kunci sama isi beda `PHM013`; versi tertinggal `PHM012`; token versi menolak penyimpanan dari salinan lama (`DbUpdateConcurrencyException`) |
| Transisi status | Submitted tidak dapat diserahkan `PHM016`; Disetujui → Selesai (sebagian); Ditolak tidak dapat diserahkan/disetujui/dibatalkan; Draft tidak dapat diputus; pengajuan CITO tanpa alasan `PHM014`; Disetujui masih dapat dibatalkan |
| Aturan peralihan | tanpa batas tetap wajib disetujui; sebelum batas boleh diserahkan langsung; sesudah batas wajib disetujui; permintaan lama tetap dapat diputus; antrean gudang memuat Disetujui dan permintaan lama saja; nilai batas tidak sah menggagalkan service |

Keterbatasan: dua keputusan yang benar-benar bersamaan hanya dapat dibuktikan pada PostgreSQL.
Yang dibuktikan di SQLite adalah token versi dan penolakan keputusan kedua.

## Validasi

| Pemeriksaan | Hasil | Catatan |
| --- | --- | --- |
| `dotnet build`, `dotnet test` | NOT RUN | Belum ada izin khusus |
| Keseimbangan `{}`, `()`, `[]` | PASS | Service 95/95; Authority 12/12; DTO; Controller 61/61; Harness 61/61; Tests 43/43 |
| DbSet dan properti yang dirujuk test | PASS | Seluruh 12 DbSet ada di `ApplicationDbContext`; properti model diperiksa satu per satu |
| Registrasi DI | PASS (statis) | `AddScoped<StockRequestService>()` tidak berubah; `IConfiguration` sudah terdaftar dan parameter baru opsional |

## Penutup

- MANUAL TEST: NOT FEASIBLE. Build, test runner, dan dev server belum diizinkan
- DATABASE: hanya query baca untuk audit; tidak ada perubahan data maupun skema
- INCIDENTAL CHANGES: NONE
- GIT STATUS: belum di-commit
- NEXT RECOMMENDED STEP: (1) izin `dotnet build -p:SkipMigrationMetadata=true` dan
  `dotnet test Tests/QuilvianSystemBackend.PharmacyTests --filter StockRequestApprovalTests`;
  (2) isi data struktur organisasi minimal untuk satu unit uji; (3) beri izin CITO ke satu jabatan
  uji; (4) uji runtime dengan `Security__Authorization__Enabled=true`
