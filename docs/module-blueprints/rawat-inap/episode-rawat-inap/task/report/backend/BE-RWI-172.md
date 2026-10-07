# Laporan Perubahan Backend — `BE-RWI-172`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-172` |
| Judul | Bentuk data `MasterData` Finishing (`K8` + `E4`) |
| Slice | `MVP-1` / `RWF-W3` — fondasi master data Finishing |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-172` |
| Trace | `FR-RWF-045`, `047`, `060`, `061`, `088`; `RWI-DEC-173`, `179`, `193`, `196`, `220` (6); `02-module-map.md` 7.4 |
| Contract version | `episode-rawat-inap` `0.10.0` **`approved`** (`RWI-DEC-221`, 2 Oktober 2026); `keperawatan` kamus data 12.14 dan API 8.2 |
| Dependency | — |
| Klasifikasi | `HEAVY` — skor 11: repository 0, berkas diperiksa 1, berkas diubah 2, logika 1, kontrak API 2, database 2, keamanan 0, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/MasterData/**`, `Areas/HealthServices/InPatientManagement/Services/InpSettingService.cs`, `Repositories/Configurations/HealthServices/**`, `Repositories/ApplicationDbContext.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`; sebagian berkas task ini sudah di-commit pemilik pada `e2ded614`, sisanya masih perubahan kerja) |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode dan penerapan skema maju selesai. Pembaruan 5 Oktober 2026: build terintegrasi `PASS` dan migration `20261005033044_AddRawatInapFinishing` diterapkan berdasarkan output pengguna; API/alur bisnis dan rollback belum dijalankan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MasterData` (milik seluruh tim, `RWI-DEC-193`); pembaca `InPatientManagement` |
| Prefix registry | `Mst` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (dua model dan empat enum baru); `TOUCHED LEGACY` (`MstTariff`, `MstInpatientSetting`, `TariffController`, `InpatientSettingController`) |
| QBE yang berlaku | `QBE-ENT-001` (model turunan `IdentityModel`), `QBE-NAM-001` (prefix `Mst`), `QBE-DTO-001`, `QBE-VAL-001`, `QBE-SVC-001` (pengaturan lewat service) |
| Wewenang | Source: ya. Migration: **tidak** (keputusan pengguna). Database: tidak. Deployment: tidak |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, master data belum dapat menyimpan apa pun yang dibutuhkan Finishing: tidak ada
daftar jenis alat medis, tarif tidak dapat menandai dirinya sebagai jasa anestesi atau sewa kamar
operasi, tidak ada master butir persiapan bedah, dan pengaturan Rawat Inap belum punya ambang
Daftar Pantau. Akibatnya pra-operasi, biaya operasi, pemakaian alat, dan dua daftar pantau baru
tidak punya tempat menyimpan konfigurasinya.

## 2. Proses bisnis

1. Admin Master Data mengelola tarif seperti biasa. Tarif lama tetap diterima tanpa isian baru;
   isian baru (`MedicalEquipmentId`, `SurgeryComponentType`, `ChargeBasis`, `ChargeRounding`)
   opsional.
2. Contoh: tarif "Sewa Kamar Operasi Kelas 1" disimpan dengan `SurgeryComponentType = OperatingRoomRent`,
   `ChargeBasis = PerHour`, `ChargeRounding = CeilingWholeUnit`. Operasi 95 menit kelak ditagih 2 jam.
3. Satu tarif hanya boleh merujuk satu objek: tindakan, obat, jenis alat, **atau** komponen operasi.
   Tarif yang mengisi dua sekaligus ditolak 400.
4. Admin mengubah pengaturan Rawat Inap: dua ambang baru 1–1440 menit (bawaan 60 dan 30). Isian
   kosong dari layar lama mempertahankan nilai tersimpan.
5. Jalur tidak normal: ambang 0 atau 2000 → 400 "… harus antara 1 dan 1440 menit"; jenis alat
   nonaktif dipilih pada tarif → 400.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstTariff.cs`, `MstTariffConfiguration.cs`, `TariffController.cs`, `TariffDtos.cs`,
`MstInpatientSetting.cs`, `MstInpatientSettingConfiguration.cs`, `InpatientSettingController.cs`,
`InpatientSettingService.cs`, `InpatientSettingDtos.cs`, `InpSettingService.cs`,
`ApplicationDbContext.cs`; kamus data 19.7, 19.10, 19.12 dan `keperawatan` 12.14.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Enums/MstEquipmentChargeUnit.cs`, `MstEquipmentRoundingRule.cs`, `MstSurgeryComponentType.cs`, `MstTariffChargeBasis.cs` | Baru — empat enum kamus data |
| `Areas/HealthServices/MasterData/Models/MstMedicalEquipment.cs` | Baru — master jenis alat (`K8`) |
| `Areas/HealthServices/MasterData/Models/MstSurgicalPreparationItem.cs` | Baru — master butir persiapan bedah (`E4`) |
| `Repositories/Configurations/HealthServices/MasterData/MstMedicalEquipmentConfiguration.cs`, `MstSurgicalPreparationItemConfiguration.cs` | Baru — unique parsial `UX_MstSurgicalPreparationItem_Code`, token konkurensi |
| `Areas/HealthServices/MasterData/Models/MstTariff.cs`, `Repositories/Configurations/HealthServices/MstTariffConfiguration.cs` | Empat kolom baru, FK `MedicalEquipmentId` `Restrict`, bawaan aman |
| `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs`, `Repositories/Configurations/HealthServices/MasterData/MstInpatientSettingConfiguration.cs` | Dua kolom ambang, bawaan 60 dan 30 |
| `Areas/HealthServices/MasterData/DTOs/TariffDtos.cs`, `Controllers/TariffController.cs` | Isian baru opsional, validasi satu rujukan objek, bawaan saat tambah, pertahankan saat ubah |
| `Areas/HealthServices/MasterData/DTOs/InpatientSettingDtos.cs`, `Services/InpatientSettingService.cs`, `Controllers/InpatientSettingController.cs` | Dua ambang di respons dan `PUT`, validasi 1–1440 |
| `Areas/HealthServices/InPatientManagement/Services/InpSettingService.cs` | Nilai ambang ikut terbaca (`InpatientSettingValues`) |
| `Repositories/ApplicationDbContext.cs` | `DbSet` `MstMedicalEquipments`, `MstSurgicalPreparationItems` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: tarif empat isian opsional; pengaturan dua isian. Pemanggil lama tetap berfungsi (regresi `RWI-DEC-193`) |
| Database | `MstMedicalEquipment`, `MstSurgicalPreparationItem`, empat kolom `MstTariff`, dan dua kolom `MstInpatientSetting`. Perubahan `K8` + `E4` tercakup dalam `20261005033044_AddRawatInapFinishing`; pengguna melaporkan penerapan berhasil (`Done.`), bukti diterima 5 Oktober 2026. Nama database/lingkungan tidak disebut |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint atau permission baru |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Tariff

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/health-services/master-data/tariffs` | Tambah tarif; empat isian komponen opsional | `Tariff : Create` |
| `PUT` | `api/v1/health-services/master-data/tariffs/{id}` | Ubah tarif; isian kosong mempertahankan nilai lama | `Tariff : Update` |

#### Health Services / Master Data / Inpatient Setting

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/health-services/master-data/inpatient-settings` | Respons memuat `PendingSurgicalHandoverAlertMinutes`, `PendingAdmissionReferralAlertMinutes` | `InpatientSetting : Read` |
| `PUT` | `api/v1/health-services/master-data/inpatient-settings/{id}` | Mengubah ambang 1–1440 menit | `InpatientSetting : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian engineering | Prefix `Mst`, `IdentityModel`, DTO terpisah, tanpa generic repository | `PASS` | Review source sesi 2 Oktober 2026 |
| Review diff dan scope | Perubahan hanya pada cakupan kartu | `PASS` | Daftar 3.2 |
| Pemeriksaan bentrokan nama tipe antar-namespace (pencarian teks) | Tidak ada nama ganda | `PASS` | Sesi 2 Oktober 2026 |
| Build project melalui `dotnet ef database update` | `Build succeeded.` | `PASS` | Output pengguna diterima 5 Oktober 2026; bukan eksekusi ulang oleh agent atau perintah `dotnet build` tersendiri; jumlah warning tidak disertakan |
| Keberadaan migration dan `Up()`/`Down()` | File `.cs` dan `.Designer.cs` tersedia; perubahan `K8` + `E4` tercakup | `PASS` | `Migrations/20261005033044_AddRawatInapFinishing.cs`; pemeriksaan skrip SQL khusus dan eksekusi rollback belum dijalankan |
| Verifikasi API tarif dan pengaturan, regresi tarif | Dibaca dari source; belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT RUN`. Alasan belum ada build/database berlaku pada sesi 2 Oktober 2026; bukti terbaru menunjukkan build dan penerapan migration, tetapi belum ada hasil uji API/alur bisnis.

**Riwayat 2 Oktober 2026:** agent tidak menjalankan `dotnet build`, pembuatan migration, atau pemeriksaan skrip migration atas instruksi pengguna "Jangan lakukan dotnet build dan migrasi". Bukti terbaru pada bagian 5.1; perintah `dotnet build` tersendiri dan pemeriksaan skrip SQL khusus tidak dibuktikan output ini.

### 5.1 Pembaruan bukti 5 Oktober 2026

Pengguna mengirim output berikut dan meminta dokumen diperbarui. Perintah dijalankan pengguna dari repository `NewQuilvianSystemBackend`; agent tidak menjalankan ulang build atau operasi database.

```text
dotnet ef database update
Build started...
Build succeeded.
Acquiring an exclusive lock for migration application. See https://aka.ms/efcore-docs-migrations-lock for more information if this takes too long.
Applying migration '20261005033044_AddRawatInapFinishing'.
Done.
```

| Bukti | Status | Batas bukti |
| --- | --- | --- |
| Build project yang dipanggil perintah EF | `PASS` | `Build succeeded.`; bukan hasil perintah `dotnet build` terpisah; jumlah warning tidak disertakan |
| Penerapan migration maju | `PASS` berdasarkan output pengguna | `Applying migration` lalu `Done.`; nama database/lingkungan tidak disertakan |
| Source migration | `PASS` (pemeriksaan statis) | `Up()` membuat delapan tabel dan 27 kolom; `Down()` memuat penghapusan delapan tabel dan 27 kolom yang sama. Ini membuktikan keberadaan operasi, bukan keberhasilan rollback |
| Eksekusi rollback, pemeriksaan skrip SQL khusus, dan query katalog/riwayat database oleh agent | `NOT RUN` | Tidak dijalankan pada pembaruan dokumentasi ini |
| API, regresi, dan alur klinis ujung ke ujung | `NOT RUN` | Output penerapan database tidak membuktikan perilaku aplikasi |

Pemetaan perubahan dalam satu migration aktual:

| Task | Langkah desain | Perubahan utama |
| --- | --- | --- |
| `BE-RWI-149` | `I1` + `I2` | kolom `InpEpisode`, `InpBedPlacement`, `InpIntegrationOutboxes`, `BilInvoice`, serta tabel `BilInpatientEventReceipt` |
| `BE-RWI-172` | `K8` + `E4` | `MstMedicalEquipment`, `MstSurgicalPreparationItem`, empat kolom `MstTariff`, dan dua kolom `MstInpatientSetting` |
| `BE-RWI-174` | `E5` bagian kasus | lima kolom `OprCase` dan constraint `CK_OprCase_Rejected` |
| `BE-RWI-176` | `E5` bagian pra-operasi | `OprWardPreOpNote`, `OprWardPreOpItem`, dan `OprWardPreOpSiteMark` |
| `BE-RWI-181` | `E6` | tabel `InpAdmissionReferral` |
| `BE-RWI-183` | `E7` | tabel `CliTransferHandover` |

Source dibaca pada HEAD `f32b2308291c8d02b083319dac4210d3431f899e` ditambah dua file migration baru dan perubahan snapshot yang belum di-commit. File: `Migrations/20261005033044_AddRawatInapFinishing.cs`, `Migrations/20261005033044_AddRawatInapFinishing.Designer.cs`, dan `Migrations/ApplicationDbContextModelSnapshot.cs`. SHA desain/perencanaan tetap menjadi snapshot historis.

**Pengemasan aktual.** Satu migration memuat `I1` + `I2`, `K8` + `E4`, `E5`, `E6`, dan `E7`. Pada `Up()`, master persiapan dibuat sebelum tabel pra-operasi dan kolom `OprCase` ditambah sebelum `InpAdmissionReferral`. Rencana `BE-RWI-149` menyebut dua migration terpisah; laporan task itu mempertahankan penyimpangan tersebut. Penerapan ini tidak membuktikan `I6`, data seeder `E8`, rilis produksi, atau kesiapan seluruh modul. Contoh: tabel pra-operasi sudah tercakup migration, tetapi konfirmasi oleh dua akun tetap perlu diverifikasi terpisah.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Satu migration memuat seluruh perubahan dan punya `Down()` | Terpenuhi (source); penerapan maju berhasil menurut output pengguna | `20261005033044_AddRawatInapFinishing` mencakup seluruh perubahan `K8` + `E4`, bersama perubahan task lain; `Down()` tersedia, belum diuji eksekusinya |
| 2. Nama, tipe, nullability, bawaan, index, FK `Restrict` sama dengan kamus data | Terpenuhi (source) | `MstSurgicalPreparationItemConfiguration`, `MstMedicalEquipmentConfiguration`, `MstTariffConfiguration` |
| 3. Tarif lama tidak berubah; endpoint lama tetap menerima permintaan tanpa isian baru | Terpenuhi (source) | Isian `null` = bawaan saat tambah, pertahankan saat ubah (`TariffController`) |
| 4. Pengaturan menolak ambang di luar 1–1440 | Terpenuhi (source) | `[Range(1,1440)]` dan `InpatientSettingService.ValidateAsync` |
| 5. Tidak ada perubahan perilaku Billing | Terpenuhi | Billing tidak disentuh task ini |
| DoD `dotnet build` tanpa error | Terpenuhi melalui build project terintegrasi pada perintah EF | Output pengguna: `Build succeeded.`; diterima 5 Oktober 2026 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `RowVersion` diterapkan sebagai `Guid` bertanda konkurensi; kamus data menulis `xmin`, sedangkan repository belum memakai `xmin` di mana pun |
| Masalah yang diketahui | Kekurangan file migration `K8` + `E4` sudah ditutup. Migration aktual juga mencakup task lain; pemetaan pada bagian 5.1 |
| Risiko tersisa | `MstTariff` dibaca Billing; build terintegrasi berhasil, tetapi API, regresi tarif lama, dan rollback belum diverifikasi |
| Perubahan sampingan | `NONE` |
| Interupsi | Pergantian konteks sesi; dilanjutkan dari keadaan source yang dibaca ulang. Sebagian berkas sudah di-commit pemilik pada `e2ded614` |
| Status Git | Perubahan kerja task ini: `M` `MstTariff.cs`, `MstInpatientSetting.cs`, `TariffController.cs`, `TariffDtos.cs`, `InpatientSettingController.cs`, `InpatientSettingDtos.cs`, `InpatientSettingService.cs`, `InpSettingService.cs`, `MstTariffConfiguration.cs`, `MstInpatientSettingConfiguration.cs`; berkas baru task ini sudah ada di `e2ded614` |
| Langkah berikutnya | Jalankan verifikasi API tarif/pengaturan dan regresi tarif lama; build terintegrasi dan penerapan migration maju sudah dibuktikan output pengguna. |
