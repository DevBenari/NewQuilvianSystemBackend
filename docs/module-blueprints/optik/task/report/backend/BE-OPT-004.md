# Laporan Perubahan Backend — `BE-OPT-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-004` |
| Judul | Nomor, pembaca izin, seeder |
| Slice | `MVP-0` — `EPIC OPT-00` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-004` |
| Trace | `OPT-DEC-009`, `012`, `040`, `041`, `045`; `QBE-CODE-001..006`; `OPT-VAL-001`, `002`; `INT-OPT-03`; `02-backend-architecture.md` bagian 4.2 dan 8 |
| Contract version | `OPT-VAL-v1`, `OPT-INT-v1` — `approved` sisi Optik |
| Dependency | `BE-OPT-002` ✅ (tabel `OptSetting`), `BE-OPT-001` ✅ (unit Optik) |
| Klasifikasi | `MEDIUM` — dua service baru, perluasan seeder, DI; tanpa endpoint, tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/Services/`, `Seeders/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE` (service dan seeder Optik); `TOUCHED LEGACY` (`Program.cs`, hanya pendaftaran) |
| QBE yang berlaku | `QBE-CODE-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-CODE-005`, `QBE-CODE-006`, `QBE-SVC-001`, `QBE-MOD-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Layanan Optik berikutnya (pemeriksaan, resep, pesanan, keluhan) butuh tiga fondasi yang belum ada:
- nomor dokumen yang aman dari tabrakan;
- pemeriksaan izin praktik RO/Sp.M yang benar terhadap tanggal WIB;
- pengaturan bawaan unit Optik.

Tanpa pengaturan bawaan, minimum uang muka, usia resep, dan masa garansi tidak punya nilai sama sekali.

---

## 2. Proses bisnis

1. **Nomor dokumen.** Saat sebuah dokumen dibuat (oleh task berikutnya), service meminta nomor ke `OptNumberService`. **Contoh:** pemeriksaan pertama `OPT-EXM-RSMMC-000001`, resep pertama `OPT-RX-RSMMC-000001`, pesanan kedua `OPT-ORD-RSMMC-000002`. Nomor tidak dihitung dari isi tabel, jadi pesanan yang gagal disimpan tidak membuat nomornya dipakai ulang.
2. **Izin praktik.** Sebelum RO/Sp.M menyimpan pemeriksaan atau resep, service bertanya ke `OptPractitionerLicenseReader`: apakah akun ini tertaut ke pegawai yang SIP-nya berlaku pada tanggal kalender WIB itu? **Contoh:** SIP Rina berakhir 31-10-2026. Pada 03-11-2026 jawabannya "SIP Anda tidak berlaku sejak 31-10-2026.". SIP yang berakhir 03-11-2026 masih berlaku sampai 23.59 WIB malam itu.
3. **Pengaturan bawaan.** Saat backend menyala, seeder mengisi satu pengaturan untuk unit Optik: uang muka 50%, pengingat 30 hari, tutup 90 hari, usia resep 12 bulan, garansi 30 hari/6 bulan/6 bulan, BC 7,50–9,50, DIA 13,00–15,00. Admin Optik kelak mengubahnya lewat `BE-OPT-005`, dan seeder tidak menimpa ubahan itu.

**Jalur tidak normal:**
- Akun tanpa tautan pegawai → "Anda tidak berwenang membuat resep atau pemeriksaan refraksi.".
- SIP belum diverifikasi atau dicabut → tidak berlaku.
- Tabel `OptSetting` belum ada di basis data karena migration belum diterapkan → pengaturan dilewati dengan peringatan, sedangkan unit tetap diisi dan backend tetap menyala.
- Unit Optik dihapus admin → tidak ada pengaturan yang diisi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`McuOrderNumberService.cs` (pola nomor), `NumberAllocationRequest.cs`, `NumberSeriesAllocator.cs` (kunci `pg_advisory_xact_lock` dilewati di luar PostgreSQL), `NumberSeriesResetPolicies.cs`; `DoctorQueueController.cs` (pembacaan STR/SIP dengan `AppDateTimeHelper.OperationalDate()`), `PrescriptionCopyService.cs` (perbandingan `ExpiredDate >= DateTime.UtcNow`, tidak ditiru); `WfpCredentialLicense.cs`, `CredentialVerificationStatus.cs`, `ApplicationUser.cs`, `MstWorkforceProfile.cs`; `Helpers/AppDateTimeHelper.cs`; `Program.cs` (`Seeders:ContinueOnFailure` bawaan `false`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Services/OptNumberService.cs` | **Baru.** Empat deret `OPT-EXM`, `OPT-RX`, `OPT-ORD`, `OPT-CLM` (awalan `…-RSMMC`, enam digit minimum, reset `NEVER`) lewat `NumberSeriesAllocator` |
| `Areas/HealthServices/OpticalManagement/Services/OptPractitionerLicenseReader.cs` | **Baru.** Baca SIP (dan STR, disiapkan) per tanggal kalender WIB lewat `ApplicationUser.WorkforceProfileId` → `WfpCredentialLicense`; syarat aktif, tidak terhapus, tidak dicabut, terverifikasi, sudah terbit, belum kedaluwarsa; pesan sama dengan `OPT-VAL-001`/`002` |
| `Areas/HealthServices/OpticalManagement/Seeders/OptReferenceDataSeeder.cs` | **Diperbarui.** Tambah satu `OptSetting` bawaan untuk unit Optik tertua yang belum terhapus; pengaman `42P01` agar startup tidak mati sebelum migration diterapkan |
| `Program.cs` | **Diperbarui.** `AddScoped<OptNumberService>()`, `AddScoped<OptPractitionerLicenseReader>()`, satu `using`, komentar seeder diperbarui |
| `Tests/QuilvianSystemBackend.OpticalTests/Be004NumberLicenseSeederTests.cs` | **Baru** (diabaikan Git). 11 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum ada endpoint |
| Database | Tanpa migration. Alokasi nomor pertama kali membuat baris `NumNumberSeries` per deret (perilaku alokator). Startup berikutnya mengisi satu `OptSetting` **bila** tabelnya sudah ada; bila belum, dilewati dengan peringatan |
| Keamanan/Auth | Pembaca izin hanya membaca data HR. Nomor izin praktik ikut dikembalikan untuk snapshot (`ExaminerLicenseNumberSnapshot`), bukan kolom bertanda Sensitif di kamus data |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` (build proyek utama ikut) | 17 lulus, 0 gagal (6 dari `BE-OPT-001` + 11 baru) | `PASS` | Keluaran `dotnet test` |
| Empat deret berbentuk kontrak dan monotonik | `OPT-EXM-RSMMC-000001`, `…000002`, `OPT-RX-…-000001`, `OPT-ORD-…-000001/000002`, `OPT-CLM-…-000001` | `PASS` | Uji nomor |
| Nomor tidak dari `COUNT/MAX` tabel dokumen | Dua nomor terbit tanpa satu pun `OptOrder`; pencacah deret = 2; nomor berikutnya `…000003` | `PASS` | Uji `Nomor_tidak_dihitung_dari_data...` |
| SIP berakhir 31-10-2026 pada 03-11-2026 WIB | Ditolak "SIP Anda tidak berlaku sejak 31-10-2026." | `PASS` | Uji |
| SIP berakhir 03-11-2026: 23.59 WIB (16.59 UTC) dan 00.00 WIB (17.00 UTC) | Berlaku lalu ditolak "…sejak 03-11-2026." | `PASS` | Uji |
| SIP belum diverifikasi / dicabut | Tidak berlaku | `PASS` | Uji |
| SIP lama kedaluwarsa + SIP baru varian `sip-ro` | SIP baru dipilih | `PASS` | Uji |
| STR tidak dianggap SIP; STR dapat dibaca terpisah | Benar | `PASS` | Uji |
| Akun tanpa tautan pegawai | `OPT-VAL-001` | `PASS` | Uji |
| Seeder mengisi pengaturan bawaan | Seluruh 11 nilai sesuai `OPT-DEC-009/040/041` dan usulan BC/DIA | `PASS` | Uji |
| Seeder idempoten; uang muka **0%** ubahan admin tetap 0 | Benar | `PASS` | Uji (membuktikan keputusan tanpa `HasDefaultValue` di `BE-OPT-002`) |
| Unit Optik terhapus → tidak ada pengaturan | Benar | `PASS` | Uji |
| QBE checker atas 4 berkas + kontrol `ZzzRegistryProbe` multi-baris | Kontrol terkena QBE-MOD-002/CFG-001; 4 berkas bersih | `PASS` | Keluaran checker; probe dihapus |

**Tidak dijalankan / tidak dapat dibuktikan di harness:**
- jalur pengaman `42P01` seeder (hanya terjadi di PostgreSQL);
- perilaku berebut nomor (kunci advisory hanya di PostgreSQL, lingkup `PLT-BE-004`);
- startup backend terhadap devYoga (wewenang database tidak diberikan).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) SIP berakhir 31-10-2026 → tidak berlaku 03-11-2026 WIB; berakhir 03-11-2026 → berlaku sampai 23.59 WIB (`OPT-AC-002`) | Terpenuhi | Dua uji izin |
| (2) Nomor unik monotonik tanpa Count+1 | Terpenuhi | Dua uji nomor; index unik nomor pada tabel dari `BE-OPT-002` |
| (3) Seeder idempoten | Terpenuhi | Uji idempoten |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bawaan `Seeders:ContinueOnFailure` = `false`: seeder yang melempar galat mematikan startup. Karena itu pengisian `OptSetting` dilewati (bukan gagal) selama migration `AddOpticalManagement` belum diterapkan |
| Masalah yang diketahui | **`OPT-OQ-017` (baru):** `OPT-DEC-012` menyebut "STR/SIP", sedangkan bunyi `OPT-VAL-002` hanya SIP. Pembaca kini mensyaratkan SIP; STR dapat dibaca lewat `CheckStrAsync`. Apakah STR juga wajib berlaku perlu diputuskan pemilik sebelum `BE-OPT-011` |
| Risiko tersisa | Pencocokan jenis izin memakai awalan `SIP`/`STR`. Bila HR menulis jenis izin RO dengan nama lain (`OPT-CAP-07`), pemeriksa tertolak. Perlu kueri baca-saja data HR devYoga dengan izin |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Interupsi | `NONE` |
| Status Git | `M Program.cs` dan perubahan `BE-OPT-000/001/002` sebelumnya; `?? Areas/HealthServices/OpticalManagement/` (kini memuat `Services/OptNumberService.cs`, `Services/OptPractitionerLicenseReader.cs`) |
| Langkah berikutnya | `BE-OPT-005` (API data referensi) atau `BE-OPT-010` (kunjungan Optik). Keputusan `OPT-OQ-017` dibutuhkan sebelum `BE-OPT-011` |
