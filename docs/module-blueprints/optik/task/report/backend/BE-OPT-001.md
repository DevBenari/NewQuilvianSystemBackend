# Laporan Perubahan Backend — `BE-OPT-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-001` |
| Judul | Jenis unit layanan Optik |
| Slice | `MVP-0` — `EPIC OPT-00` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-001` |
| Trace | `FR-OPT-002`; `OPT-DEC-032`, `OPT-DEC-053`; `02-backend-architecture.md` bagian 4.1 dan 8 |
| Contract version | `02-backend-architecture.md` r1 (`approved`); belum ada endpoint |
| Dependency | Persetujuan Master Data — Muhammad Hamzah (`OPT-DEC-053`, `OPT-EVD-002`) |
| Klasifikasi | `LIGHT` — satu nilai enum, satu label, satu seeder idempoten |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs`, `Areas/HealthServices/OpticalManagement/Seeders/`, `Tests/`. **Tambahan di luar daftar awal, dengan alasan di bagian 3.2:** `Areas/HealthServices/MasterData/Services/MedicalRecordCompletenessProfileService.cs` (satu label) dan `Program.cs` (pendaftaran seeder) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Tanpa migration. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MasterData` (enum, label) dan `OpticalManagement` (seeder) |
| Registry | `Mst` (MasterData) dan `Opt` / `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` untuk `ServiceUnitType.cs`, `MedicalRecordCompletenessProfileService.cs`, dan `Program.cs`; `NEW CODE` untuk seeder |
| QBE yang berlaku | `QBE-ENUM-001`, `QBE-MOD-001`, `QBE-MOD-002` (tidak berlaku: tidak ada entity baru) |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Daftar jenis unit layanan belum mengenal "Optik" (`OPT-CAP-03`). Tanpa nilai ini, unit Optik
hanya bisa dicatat sebagai "Lainnya", sehingga laporan dan penyaringan menurut jenis unit tidak
bisa memisahkan Optik (`OPT-DEC-032`).

---

## 2. Proses bisnis

1. Saat backend menyala, seeder memastikan ada **satu** unit layanan berjenis Optik (kode `SU-OPT-001`, nama "Unit Optik").
2. Admin Master Data bisa mengganti nama, lokasi, dan organisasinya lewat layar unit layanan yang sudah ada. Startup berikutnya tidak menimpa perubahan itu.
3. Kunjungan Optik nantinya didaftarkan ke unit ini lewat jalur unit penunjang (`BE-OPT-010`). Unit tidak mewajibkan dokter maupun antrean, karena pemeriksanya RO atau Sp.M (`OPT-DEC-005`).

**Jalur tidak normal:**
- Admin sudah membuat unit Optik sendiri dengan kode lain → seeder tidak menambah unit kedua.
- Admin menghapus unit Optik → seeder tidak menghidupkannya kembali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ServiceUnitType.cs`; seluruh pemakaian `ServiceUnitType.*` di `Areas/` (pemetaan label); `ServiceUnitController.cs` (label otomatis dari nama enum dan daftar opsi lewat `Enum.GetValues`); `HemodialysisMasterDataSeeder.cs` (pola seeder unit); `Program.cs` (`RunStartupSeederAsync`); `EncounterIntakeService.cs#LoadServiceUnitAsync`; `ApplicationDbContext.cs` (tidak ada global query filter).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs` | Tambah `Optical = 11` di ujung, sesudah `Hemodialysis = 10` |
| `Areas/HealthServices/MasterData/Services/MedicalRecordCompletenessProfileService.cs` | Tambah label `ServiceUnitType.Optical => "Optik"`. **Alasan di luar daftar awal:** tanpa baris ini, jenis Optik tampil "Tidak diketahui" di profil kelengkapan rekam medis. Perubahan satu baris dan bagian langsung dari penambahan enum yang disetujui Master Data |
| `Areas/HealthServices/OpticalManagement/Seeders/OptReferenceDataSeeder.cs` | **Baru.** Unit `SU-OPT-001` "Unit Optik", `ServiceUnitType.Optical`; terdaftar untuk pendaftaran; tanpa dokter, antrean, skrining, kiosk, perjanjian, atau order darah; idempoten. `BE-OPT-004` akan menambah pengaturan Optik ke seeder ini |
| `Program.cs` | Pendaftaran `OptReferenceDataSeeder` lewat `RunStartupSeederAsync`, sesudah seeder MCU. **Alasan:** seeder tidak akan pernah berjalan tanpa baris ini |
| `Tests/QuilvianSystemBackend.OpticalTests/` | **Baru** (diabaikan Git, `/Tests/*`). Harness SQLite disalin dari `MedicalRecordTests` + 6 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET` metadata unit layanan kini memuat opsi `Value = 11`, `Name = "Optical"`. Labelnya `"Optical"` karena `ServiceUnitController` membentuk label dari nama enum (perilaku as-is). Pemetaan label Indonesia di layar unit layanan adalah urusan Master Data/frontend |
| Database | Tanpa migration (enum disimpan `int`). **Startup berikutnya pada basis data mana pun akan menyisipkan satu baris `MstServiceUnit` `SU-OPT-001`**, kecuali sudah ada unit Optik atau `SeedDefaultData:Enabled = false` |
| Keamanan/Auth | `NOT APPLICABLE` |

**Bukti kriteria "unit Optik dapat dipilih `EncounterIntakeService`":**
`EncounterIntakeService.LoadServiceUnitAsync` (`:375`) hanya mensyaratkan unit `IsActive` dan
`!IsDelete`, tanpa menyaring jenis unit. Unit dari seeder memenuhi keduanya. Uji pendaftaran
kunjungan nyata dilakukan di `BE-OPT-010` (`OptVisitService`).

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 6 lulus, 0 gagal (31 detik) | `PASS` | Keluaran `dotnet test` |
| Nilai lama tidak bergeser (`Hemodialysis` = 10, `Other` = 99) dan `Optical` = 11 | Lulus; jumlah anggota enum 13 | `PASS` | Uji `Nilai_lama_ServiceUnitType_tidak_bergeser...` |
| Label Indonesia "Optik" | Lulus | `PASS` | Uji label |
| Seeder menambah satu unit dengan penanda penunjang | Lulus | `PASS` | Uji seeder |
| Seeder idempoten; nama/lokasi ubahan admin tidak ditimpa | Lulus | `PASS` | Uji idempoten |
| Unit Optik berkode lain buatan admin tidak digandakan | Lulus | `PASS` | Uji kode lain |
| Unit Optik yang dihapus tidak dihidupkan kembali | Lulus | `PASS` | Uji terhapus |
| Bonus `BE-OPT-002`: 12 tabel Optik terbentuk dari model EF di SQLite (`EnsureCreated`) | Lulus | `PASS` | Seluruh uji memanggil `TestDatabase.Create()` |
| QBE checker atas 4 berkas + kontrol `ZzzRegistryProbe` multi-baris | Kontrol terkena QBE-MOD-002/CFG-001; 4 berkas bersih | `PASS` | Keluaran checker (pemanggilan kedua; probe satu baris tidak dikenali checker, lihat bagian 7) |

**Tidak dijalankan:** startup backend terhadap devYoga, karena akan menyisipkan baris unit dan wewenang
database tidak diberikan. Uji `EncounterIntakeService` penuh dijadwalkan di `BE-OPT-010`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Nilai lama tidak bergeser (uji enum: `Hemodialysis` tetap 10) | Terpenuhi | Uji enum |
| (2) Unit Optik dapat dipilih `EncounterIntakeService` | Terpenuhi lewat pembuktian kode | `LoadServiceUnitAsync` tanpa penyaring jenis + unit seeder aktif. Uji berjalan di `BE-OPT-010` |
| Tanpa migration | Terpenuhi | Enum disimpan `int` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Startup berikutnya menyisipkan satu baris `MstServiceUnit` di basis data yang dipakai backend (devYoga bila backend lokal dinyalakan) |
| Masalah yang diketahui | Label opsi di `ServiceUnitController` tetap `"Optical"` (dibentuk otomatis dari nama enum, sama seperti jenis lain berbahasa Inggris). Tidak diubah karena di luar scope |
| Risiko tersisa | Rendah. `src/lib/constants/health-services/master-data/service-unit-constants.jsx` tidak menulis daftar jenis unit sendiri (dicek 2026-11-03), sehingga opsi diduga dibaca dari metadata backend dan nilai 11 ikut tampil dengan label `"Optical"`. Belum diverifikasi di layar |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Interupsi | `NONE` |
| Status Git | `M Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs`, `M Areas/HealthServices/MasterData/Services/MedicalRecordCompletenessProfileService.cs`, `M Program.cs`, `?? Areas/HealthServices/OpticalManagement/Seeders/OptReferenceDataSeeder.cs`, ditambah perubahan `BE-OPT-000`/`002` yang belum di-commit |
| Langkah berikutnya | `BE-OPT-004` (memperluas `OptReferenceDataSeeder` dengan pengaturan, nomor dokumen, dan pembaca izin praktik) |
