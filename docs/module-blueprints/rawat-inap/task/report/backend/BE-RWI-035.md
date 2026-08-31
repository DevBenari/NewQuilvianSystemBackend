# Laporan Perubahan Backend — `BE-RWI-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-035` |
| Judul | Encounter admin dapat membawa penjamin perusahaan |
| Slice | `S10 — Encounter membawa penjamin perusahaan` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, entri `BE-RWI-035` |
| Trace | `RWI-CAP-002` **Wajib**; `RWI-DEC-075`; `RWI-UI-GAP-002`; `FE-RWI-024`; `FE-RWI-025`; persetujuan Product/Domain 31 Agustus 2026 |
| Contract version | [`RWI-ENC-PAYER-001` versi `1.0.0`](../../../contracts/encounter-company-guarantor-contract.md), status `APPROVED`, hash `48bf0a73c511bf92315006330eb2a728e3363ec2be87736f7246b927c19f960b` — **diverifikasi ulang lewat `sha256sum` pada waktu eksekusi dan cocok** |
| Dependency | Tidak menunggu task backend lain. `FE-RWI-025` menunggu task ini |
| Klasifikasi | `HEAVY` — skor 10: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 2, database 2, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, migration, test, dan register modul `rawat-inap` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `d341cf505016e1c0d27d14a1c9f31f5c71545434`, branch `MHamzah` |
| Tanggal | 31 Agustus 2026 |
| Status | **Selesai.** Delapan acceptance criteria terpenuhi. Migration dibuat di source **dan sudah diterapkan ke database dev pemilik** atas wewenang eksplisit pemilik repository, 31 Agustus 2026. Database bersama/target lain **belum** disentuh |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RegistrationManagement` |
| Submodule | `NOT APPLICABLE` — capability tinggal langsung di modul pemiliknya |
| Pemilik/prefix registry | `HealthServices` / `RegistrationManagement / Registration` / `BUSINESS DOMAIN / MODULE` / prefix `Reg` / lifecycle `ACTIVE / LEGACY` |
| Status registry | Terdaftar dan `ACTIVE`. Master yang dirujuk memakai prefix `Mst` yang juga terdaftar dan `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-ENT-003`, `QBE-CFG-002`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-MOD-002` dan `QBE-MOD-003` — tidak ada folder atau modul baru. `QBE-NAM-001` sampai `004` — tidak ada entity baru yang dibuat. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION`. `QBE-CODE-001` sampai `006` — generator nomor sumber pembayaran tidak disentuh |

**Kenapa `TOUCHED LEGACY`, bukan `NEW CODE`.** Task ini tidak membuat satu pun entity baru. Yang
dikerjakan adalah penambahan kolom, validasi, dan mapping pada entity legacy
`TrxPatientEncounterGuarantor` beserta controller pemiliknya yang sudah ada. Dua konsekuensinya
dicatat sebagai temuan pada bagian 7, bukan diperbaiki tanpa wewenang: nama `Trx*` yang tetap
dipertahankan, dan controller yang tetap mengakses `ApplicationDbContext` secara langsung.

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, sistem **tidak dapat menyimpan penjamin perusahaan pada kunjungan pasien.**

Layar admisi sudah bisa menampilkan dan memilih kartu penjamin perusahaan — pekerjaan itu selesai
pada `FE-RWI-024`. Tetapi ketika petugas menekan simpan, backend hanya mengenal dua metode
pembayaran: Tunai dan Asuransi. Pilihan perusahaan yang sudah dibuat petugas hilang di perjalanan.

Akibat nyatanya bagi pengguna, dengan contoh:

> Pasien **Budi Santoso** adalah karyawan **PT Sehat Sentosa** dengan nomor karyawan `EMP-00125`.
> Perusahaannya menjamin biaya berobat. Petugas admisi memilih kartu perusahaan itu di layar,
> lalu menyimpan. Kunjungan yang tersimpan tercatat **Tunai**.

Tiga hal ikut rusak karena itu:

1. **Kasir menagih orang yang salah.** Kunjungan tercatat tunai, sehingga tagihan diarahkan ke
   pasien, bukan ke perusahaannya.
2. **Tidak ada jejak audit.** Tidak ada catatan bahwa kunjungan itu dijamin perusahaan, sehingga
   penagihan ke perusahaan tidak punya dasar.
3. **Petugas harus menambal secara manual.** Satu-satunya jalan keluar adalah mencatat penjamin di
   luar sistem, dan catatan di luar sistem tidak dapat diperiksa siapa pun.

Sesudah perubahan ini, kunjungan menyimpan **satu** sumber pembayaran Penjamin Perusahaan lengkap
dengan referensi kartu, referensi perusahaan, dan salinan data perusahaan pada saat pendaftaran.

---

## 2. Proses bisnis

| Unsur | Ketentuan |
| --- | --- |
| Tujuan | Membawa penjamin perusahaan yang dipilih pada langkah Pembayaran sampai menjadi sumber pembayaran kunjungan |
| Pelaku | Petugas admisi yang memiliki hak akses `PatientEncounter : Create` |
| Pemicu | Petugas menyelesaikan langkah Dokter pada alur admisi, lalu layar membuat kunjungan |
| Prasyarat | Pasien, unit layanan, dan kartu pasien-perusahaan sudah ada; kartu dan perusahaan aktif; kartu eligible; tanggal kunjungan berada dalam masa berlaku |
| Hasil akhir | Satu baris kunjungan dan satu baris sumber pembayaran tersimpan bersama-sama, atau tidak tersimpan sama sekali |

### 2.1 Langkah yang berurutan

1. Layar admisi mengirim permintaan ke `POST /admin` dengan tipe pembayaran `3` dan `PatientCompanyGuarantorId` yang dipilih petugas.
2. Backend memastikan pengguna memiliki hak akses `PatientEncounter : Create`.
3. Backend memastikan permintaannya **tidak campuran**: untuk Penjamin Perusahaan, metode pembayaran tunai dan kartu asuransi harus kosong.
4. Backend memeriksa kelayakan kartu perusahaan secara berurutan: kartunya ada, milik pasien yang sama, aktif, eligible, perusahaannya aktif, dan tanggal kunjungan berada dalam masa berlaku.
5. Backend menyimpan kunjungan beserta sumber pembayarannya dalam satu transaksi database.
6. Backend menjawab dengan tipe pembayaran, referensi kartu, referensi perusahaan, dan salinan data yang dapat dibaca manusia.

### 2.2 Aturan masa berlaku, dengan contoh

Masa berlaku bersifat **inklusif di kedua ujungnya**. Bila kartu perusahaan berlaku
**1 sampai 31 Agustus 2026**:

| Tanggal kunjungan | Hasil | Alasan |
| --- | --- | --- |
| 31 Juli 2026 | Ditolak `400` | Kartu belum berlaku |
| 1 Agustus 2026 | Diterima | Hari pertama masih termasuk |
| 15 Agustus 2026 | Diterima | Di dalam masa berlaku |
| 31 Agustus 2026 | Diterima | Hari terakhir masih termasuk |
| 1 September 2026 | Ditolak `400` | Kartu sudah kedaluwarsa |

Kartu yang tanggal awal atau tanggal akhirnya dikosongkan pada master dianggap **tidak berbatas**
pada ujung itu.

### 2.3 Matriks tiga sumber pembayaran

Satu kunjungan hanya boleh punya **satu** sumber pembayaran. Tabel berikut mengunci kombinasi yang
sah:

| Tipe pembayaran | `PaymentMethodId` | `PatientInsuranceId` | `PatientCompanyGuarantorId` |
| --- | --- | --- | --- |
| `Cash` (`1`) — Tunai | Boleh kosong; bila diisi harus aktif dan tersedia untuk registrasi | Harus kosong | Harus kosong |
| `Insurance` (`2`) — Asuransi | Harus kosong | Wajib dan harus lolos pemeriksaan kelayakan | Harus kosong |
| `CompanyGuarantor` (`3`) — Penjamin Perusahaan | Harus kosong | Harus kosong | Wajib dan harus lolos pemeriksaan kelayakan |

### 2.4 Jalur tidak normal

| Keadaan | Jawaban backend | Pesan |
| --- | --- | --- |
| Tipe `3` tetapi kartu tidak dipilih | `400` | `PatientCompanyGuarantorId wajib diisi untuk pembayaran Penjamin Perusahaan.` |
| Tipe `3` sekaligus mengirim kartu asuransi | `400` | `PatientInsuranceId harus kosong untuk pembayaran Penjamin Perusahaan.` |
| Tipe `3` sekaligus mengirim metode pembayaran tunai | `400` | `PaymentMethodId harus kosong untuk pembayaran Penjamin Perusahaan.` |
| Kartu tidak ditemukan | `400` | `Penjamin perusahaan pasien tidak ditemukan.` |
| Kartu milik pasien lain | `400` | `Penjamin perusahaan yang dipilih bukan milik pasien pada encounter.` |
| Kartu tidak aktif | `400` | `Penjamin perusahaan pasien tidak aktif.` |
| Kartu tidak eligible | `400` | `Penjamin perusahaan pasien tidak eligible.` |
| Perusahaannya tidak aktif atau terhapus | `400` | `Perusahaan penjamin tidak valid atau tidak aktif.` |
| Tanggal kunjungan sebelum masa berlaku | `400` | `Penjamin perusahaan belum berlaku pada tanggal kunjungan.` |
| Tanggal kunjungan sesudah masa berlaku | `400` | `Penjamin perusahaan sudah kedaluwarsa pada tanggal kunjungan.` |
| Tipe `3` dikirim ke route kiosk | `400` | `Tipe pembayaran Penjamin Perusahaan hanya tersedia pada registrasi petugas.` |
| Penyimpanan database gagal | `500` | Kunjungan dan sumber pembayaran sama-sama tidak tersimpan |

**Kenapa pesan penolakan sengaja dibuat pendek.** Ketika kartu milik pasien lain dipakai, pesannya
tidak menyebut nama pasien, nomor rekam medis, maupun nomor karyawan pemilik kartu. Bila pesannya
menyebutkan itu, siapa pun yang bisa membuat kunjungan dapat menebak-nebak nomor kartu untuk
memancing data pasien lain keluar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Dokumen tata kelola dan kontrak**

- `AGENTS.md` backend
- `rules/GLOBAL_RULES.md`, `rules/backend/TASK_RULES.md`, `rules/backend/TASK_CLASSIFICATION.md`, `rules/backend/API_RULES.md`, `rules/backend/DATABASE_RULES.md`, `rules/backend/REVIEW_RULES.md`, `rules/backend/REPORT_TEMPLATE.md`, `rules/backend/role-access-rules.md`
- `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `rules/rule-output/lokasi-laporan-task.md`
- `docs/module-blueprints/rawat-inap/contracts/encounter-company-guarantor-contract.md`
- `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md`, `.../requirement-traceability.md`

**Source aplikasi**

- `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`
- `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs`
- `Areas/HealthServices/RegistrationManagement/Enums/EncounterPaymentType.cs`
- `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs`, `.../TrxPatientEncounter.cs`
- `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs`
- `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientInsurance.cs`
- `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs`, `.../MstInsuranceProvider.cs`
- `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs`
- `Areas/HealthServices/BillingManagement/MasterData/Models/MstPaymentMethod.cs`
- `Repositories/Configurations/HealthServices/TrxPatientEncounterGuarantorConfiguration.cs`
- `Seeders/AccessMenuSeeder.cs`, `Filters/AccessPermissionFilter.cs`
- `Models/ApplicationUser.cs`, `Hubs/QueueHub.cs`, `Services/.../QueueRealtimeService.cs`

**Pola test pembanding**

- `QuilvianSystemBackend.Tests/HealthServices/EmergencyInstallationManagement/EmergencyControllerTestWorld.cs`
- `QuilvianSystemBackend.Tests/InPatientManagement/IsolatedInpatientDbContextFactory.cs`
- `QuilvianSystemBackend.Tests/InPatientManagement/InpatientEpisodeControllerContractTests.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Enums/EncounterPaymentType.cs` | Menambah `CompanyGuarantor = 3` berlabel **Penjamin Perusahaan**. `Cash = 1` dan `Insurance = 2` tidak disentuh |
| `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs` | Menambah dua referensi (`PatientCompanyGuarantorId`, `CompanyGuarantorId`), tiga salinan data (`CompanyGuarantorCodeSnapshot`, `EmployeeNumberSnapshot`, `EmployeeNameSnapshot`), dan dua navigation. Komentar tipe pembayaran diperbarui agar menyebut tipe ketiga |
| `Repositories/Configurations/HealthServices/TrxPatientEncounterGuarantorConfiguration.cs` | Menambah konfigurasi kelima kolom, dua relasi ber-`Restrict`, dan dua index. Index unik `EncounterId` yang menjamin satu kunjungan satu sumber pembayaran tidak diubah |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | `PatientEncounterCreateRequest` menerima `PatientCompanyGuarantorId`; `PatientEncounterPaymentResponse` mengembalikan lima field aditif; `PatientEncounterSummaryResponse` menambah penghitung `CompanyGuarantorEncounter` |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Memisahkan jalur petugas dari kiosk lewat `CreateEncounterCoreAsync`; matriks validasi tiga payer; pemuat kelayakan kartu perusahaan; pembentukan sumber pembayaran tipe 3; mapping response; penghitung summary; diagnostik database ikut menyebut referensi baru |
| `Migrations/20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor.cs` (baru) | Migration aditif: lima kolom nullable, dua index, dua foreign key `Restrict`. Tidak ada operasi merusak |
| `Migrations/20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor.Designer.cs` (baru) | Berkas pendamping hasil generate |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot model ikut hasil generate. Selisihnya **hanya** kelima kolom, dua index, dan dua relasi di atas |
| `QuilvianSystemBackend.Tests/HealthServices/RegistrationManagement/PatientEncounterTestWorld.cs` (baru) | Dunia uji controller kunjungan pasien |
| `QuilvianSystemBackend.Tests/HealthServices/RegistrationManagement/PatientEncounterCompanyGuarantorTests.cs` (baru) | 25 test yang membuktikan kontrak `RWI-ENC-PAYER-001 1.0.0` |

### 3.3 Cara jalur petugas dipisahkan dari kiosk

Sebelum perubahan, `POST /admin` **meneruskan permintaannya bulat-bulat** ke method kiosk:

```csharp
public async Task<IActionResult> CreateEncounterForAdmin(PatientEncounterCreateRequest request)
{
    return await CreateEncounterForKiosk(request);
}
```

Bentuk itu berbahaya untuk task ini: kemampuan apa pun yang ditambahkan ke method kiosk otomatis
juga dimiliki kiosk. Kontrak bagian 7 secara khusus melarang itu.

Sesudah perubahan, keduanya memanggil satu proses bersama dengan **satu pembeda kemampuan yang
eksplisit**:

| Route | Wewenang Penjamin Perusahaan | Jejak log |
| --- | --- | --- |
| `POST /admin` | `allowCompanyGuarantor: true` | `PatientEncounter.CreateEncounterForAdmin` |
| `POST /` dan `POST /kiosk` | `allowCompanyGuarantor: false` | `PatientEncounter.CreateEncounterForKiosk` |

Jejak log ikut dipisahkan supaya pemeriksa dapat membedakan kunjungan yang dibuat petugas dari
yang dibuat kiosk. Sebelumnya keduanya tercatat dengan nama yang sama.

### 3.4 Data yang disimpan pada sumber pembayaran

| Kolom target | Diambil dari | Aturan |
| --- | --- | --- |
| `PatientCompanyGuarantorId` | `MstPatientCompanyGuarantor.Id` | Hanya untuk tipe `3` |
| `CompanyGuarantorId` | `MstPatientCompanyGuarantor.CompanyGuarantorId` | Hanya untuk tipe `3` |
| `PaymentSourceNameSnapshot` | Nama perusahaan | Salinan, tidak ikut berubah saat master disunting |
| `CompanyGuarantorCodeSnapshot` | Kode perusahaan | Salinan untuk audit dan tampilan |
| `EmployeeNumberSnapshot` | Nomor karyawan | Salinan pembeda hubungan pasien-perusahaan |
| `EmployeeNameSnapshot` | Nama karyawan | Salinan, boleh kosong |
| `BenefitPlanCodeSnapshot` | Kode benefit plan | Memakai kolom salinan yang sudah ada |
| `PlanNameSnapshot` | Nama benefit plan | Memakai kolom salinan yang sudah ada |
| `ClassNameSnapshot` | Nama kelas | Memakai kolom salinan yang sudah ada |
| `EffectiveStartDateSnapshot` / `EffectiveEndDateSnapshot` | Masa berlaku kartu | Memakai kolom salinan yang sudah ada |
| `IsEligible` | Eligibility kartu saat registrasi | Selalu `true` pada create yang berhasil |
| `IsPolicyActive` | Hasil pemeriksaan masa berlaku | Selalu `true` pada create yang berhasil |
| `PaymentMethodId`, `PatientInsuranceId`, `InsuranceProviderId` | — | **Wajib `null`** pada baris tipe `3` |

**Kenapa data perusahaan disalin, bukan dibaca ulang lewat relasi.** Bila nanti nama atau kode
`PT Sehat Sentosa` diperbaiki di master data, kunjungan yang sudah terjadi tetap harus menunjukkan
nama dan kode **pada saat pendaftaran**. Tanpa salinan itu, riwayat penagihan ikut berubah
surut setiap kali master disunting.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** `POST /admin` menerima satu field baru `PatientCompanyGuarantorId`; response payment menambah lima field; summary menambah satu penghitung; opsi filter menambah satu nilai. Tidak ada field, nilai enum, endpoint, atau pembungkus response yang diubah atau dihapus. Payload Tunai dan Asuransi lama tetap berarti sama |
| Database | **Ada dampak schema.** Lima kolom nullable, dua index, dan dua foreign key `Restrict` pada `public."TrxPatientEncounterGuarantor"`. Migration `20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor` sudah **diterapkan ke database dev pemilik** atas wewenang eksplisit pemilik repository. Rinciannya pada bagian 5.3. Database bersama, staging, dan production **belum** disentuh dan tetap memerlukan wewenang terpisah |
| Keamanan/Auth | **Tidak ada perubahan hak akses.** `POST /admin` tetap dijaga `PatientEncounter : Create`; route kiosk tetap memakai policy `KioskRead` dan `[AccessAction("Create", …)]` yang sama. Yang berubah adalah **cakupan kemampuan**: kiosk secara tegas ditolak memakai tipe `3`, sehingga wewenang kiosk menyempit relatif terhadap kemampuan baru, bukan melebar. Tidak ada `IsInRole`, daftar nama peran, nama departemen, nama posisi, atau `UserType` yang dipakai menentukan kewenangan |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/admin` | Membuat kunjungan petugas dengan tepat satu sumber pembayaran, **termasuk Penjamin Perusahaan** | `PatientEncounter : Create` |
| `POST` | `/` dan `/kiosk` | Membuat kunjungan kiosk. Tetap terbatas Tunai dan Asuransi | Policy `KioskRead` |
| `GET` | `/admin/{id}` | Detail kunjungan, kini mengenali tipe `3` | `PatientEncounter : Read` |
| `GET` | `/admin` | Daftar kunjungan, kini mengenali tipe `3` | `PatientEncounter : Read` |
| `GET` | `/admin/summary` | Ringkasan, kini menyertakan `CompanyGuarantorEncounter` | `PatientEncounter : Read` |
| `GET` | `/admin/filters/metadata` | Opsi filter, kini menyertakan **Penjamin Perusahaan** | `PatientEncounter : Read` |

Kode response `POST /admin`:

- `200` — kunjungan dan sumber pembayaran berhasil tersimpan.
- `400` — payload campuran, kartu tidak cocok dengan pasien, kartu atau perusahaan tidak aktif, tidak eligible, atau di luar masa berlaku.
- `401` — pengguna belum terautentikasi.
- `403` — pengguna tidak memiliki `PatientEncounter : Create`.
- `500` — transaksi gagal disimpan; kunjungan dan sumber pembayaran tidak tersimpan separuh.

Status endpoint setelah task ini: **Tersedia**.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `sha256sum docs/module-blueprints/rawat-inap/contracts/encounter-company-guarantor-contract.md` | `48bf0a73c511bf92315006330eb2a728e3363ec2be87736f7246b927c19f960b`, cocok dengan hash pada roadmap | `PASS` | Keluaran perintah |
| `git diff --stat 64d7419…..HEAD -- Areas/HealthServices/RegistrationManagement` | Kosong — tidak ada drift dari snapshot kontrak | `PASS` | Keluaran perintah |
| `git diff --stat 64d7419…..HEAD -- Areas/HealthServices/PatientManagement …` | Kosong — master penjamin juga tidak berubah | `PASS` | Keluaran perintah |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil. `0 Error(s)`, `135 Warning(s)` | `PASS` | Keluaran perintah |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil. `0 Error(s)` | `PASS` | Keluaran perintah |
| Peringatan compiler pada berkas yang berubah | Nol. Seluruh 135 peringatan berasal dari berkas legacy yang tidak disentuh | `PASS` | Keluaran build disaring nama berkas yang berubah |
| `dotnet test --filter PatientEncounterCompanyGuarantorTests` | `Passed! Failed: 0, Passed: 25, Total: 25` | `PASS` | Keluaran perintah |
| `dotnet test` seluruh project test | `Passed! Failed: 0, Passed: 786, Total: 786, Duration: 35 s` | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddCompanyGuarantorToPatientEncounterGuarantor` | `Done.` Migration aditif terbentuk | `PASS` | Berkas migration |
| Pemeriksaan drift snapshot EF | Selisih `ApplicationDbContextModelSnapshot.cs` hanya kelima kolom, dua index, dua relasi. Tidak ada model lain ikut terbawa | `PASS` | `git diff` snapshot |
| Penerapan migration ke database dev pemilik | Berhasil. Lima kolom, dua index, dua foreign key, dan satu baris `__EFMigrationsHistory` terbentuk | `PASS` | Bagian 5.3 |
| Tabrakan dua transaksi terhadap PostgreSQL sungguhan | Sengaja tidak dijalankan | `NOT RUN` | Tidak ada connection string yang diberi wewenang; lihat bagian 7 |
| Pembuktian `403` untuk pengguna tanpa `PatientEncounter : Create` | Sengaja tidak dijalankan | `NOT RUN` | Butuh aplikasi berjalan beserta databasenya; lihat bagian 7 |

Uji manual: `NOT FEASIBLE` — memerlukan aplikasi berjalan beserta database dan akun berperan
sungguhan, dan keduanya bukan wewenang task ini.

### 5.1 Rincian 25 test

| Kelompok | Jumlah | Yang dibuktikan |
| --- | ---: | --- |
| Kontrak nilai enum | 2 | `Cash = 1` dan `Insurance = 2` tidak bergeser; `CompanyGuarantor = 3` ada; opsi filter menampilkan label **Penjamin Perusahaan** |
| Jalur sukses dan penyimpanan | 4 | Kedua referensi perusahaan terisi; ketiga referensi Tunai/Asuransi `null`; tujuh salinan data benar; salinan tidak berubah ketika master disunting sesudahnya; response create, detail, dan summary mengenali tipe `3` |
| Payload campuran | 5 | Lima kombinasi campuran ditolak `400` dengan pesan yang tepat, dan **nol baris** tersimpan |
| Kelayakan kartu | 7 | Kartu milik pasien lain, kartu tidak aktif, kartu tidak eligible, perusahaan tidak aktif, kartu tidak ditemukan, sebelum masa berlaku, dan sesudah masa berlaku |
| Pemisahan petugas dan kiosk | 3 | Kiosk menolak tipe `3`; kiosk tetap menerima Tunai dan Asuransi |
| Regresi dua payer lama | 2 | Payload Tunai dan Asuransi tersimpan persis seperti sebelumnya, dan kelima field baru bernilai `null` |
| Atomisitas | 1 | Ketika penyimpanan digagalkan, kunjungan maupun sumber pembayaran sama-sama tidak tersisa |
| Inklusivitas masa berlaku | 1 | Kunjungan pada hari terakhir masa berlaku tetap diterima |

Kebocoran data pasien lain diuji secara khusus: pesan penolakan dipastikan **tidak mengandung**
nama pasien, nomor karyawan, maupun nomor rekam medis pemilik kartu.

### 5.2 Batas pembuktian provider InMemory

Test memakai provider InMemory, mengikuti pola yang sudah dipakai `IsolatedInpatientDbContextFactory`.
Tiga hal karena itu **tidak** dibuktikan di sini dan tercatat sebagai risiko:

1. **Index unik dan foreign key tidak ditegakkan.** Bahwa database menolak dua sumber pembayaran untuk satu kunjungan bergantung pada index unik `EncounterId` yang memang sudah ada, dan hanya dapat dibuktikan terhadap PostgreSQL sungguhan.
2. **Transaksi tidak nyata.** Yang dibuktikan adalah bahwa kunjungan dan sumber pembayaran masuk ke **satu** `SaveChangesAsync`, sehingga kegagalan menyisakan nol baris. Bahwa PostgreSQL benar-benar mengembalikan perubahan saat transaksi digagalkan adalah pembuktian terpisah.
3. **Pipeline MVC tidak berjalan.** `[Authorize]` dan `[AccessPermission]` tidak ikut dijalankan, sehingga jawaban `401` dan `403` tidak dibuktikan di sini.

**Satu catatan harness yang perlu diketahui pemelihara berikutnya.** Dunia uji wajib menyemai satu
baris `ApplicationUser` untuk pelakunya. `RegisteredByUserId` adalah relasi wajib, dan
`Include(x => x.RegisteredByUser)` pada jalur baca akan **menjatuhkan kunjungan dari hasil query**
bila baris penggunanya tidak ada. Pada PostgreSQL keadaan itu mustahil karena dijaga foreign key;
pada InMemory tidak. Sifat ini sudah ada sebelum task ini dan bukan cacat produk.

### 5.3 Penerapan migration ke database dev pemilik

Pemilik repository memberi wewenang eksplisit untuk menerapkan migration ini ke database dev
miliknya, `QuilvianNewDevHamzah`, pada 31 Agustus 2026. Rincian koneksi sengaja tidak dicatat di
sini sesuai aturan keselamatan rahasia.

**Temuan yang muncul sebelum penerapan, dan kenapa penting.** `dotnet ef migrations list`
menunjukkan **enam** migration tertunda, bukan satu. Migration task ini berada paling belakang,
dan EF menerapkan migration secara berurutan — sehingga `dotnet ef database update` apa adanya
akan lebih dulu menjalankan lima migration milik pekerjaan lain:

| Migration tertunda di depan | Isi yang merusak |
| --- | --- |
| `20260826090500_ImplementIgdFullPatientJourney` | `DROP TABLE` 2 tabel; `DROP COLUMN` ±20 kolom pada `TrxEmergencyDeparture`, `TrxEmergencyVisit`, `MstServiceUnit`; `SET NOT NULL` pada `TrxPatientAssessment.QueueId` |
| `20260827030000` s.d. `20260827060000` — empat migration rename | `ALTER TABLE … RENAME` tabel dan constraint `Trx*` → `Emg*` |
| `20260828063909_RepairCanonicalEfModelBaseline` | `Up()` kosong — tidak berdampak |

Temuan ini diangkat ke pemilik sebelum perintah apa pun dijalankan. Pemilik memilih jalur bedah:
**menerapkan migration task ini saja**, dan membiarkan kelima migration lain tetap tertunda persis
seperti keadaan sebelumnya.

**Cara penerapannya.** SQL-nya tidak ditulis tangan, melainkan dihasilkan EF sendiri supaya identik
dengan yang akan dijalankan migration nanti:

```text
dotnet ef migrations script 20260828063909_RepairCanonicalEfModelBaseline                             20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor                             --idempotent
```

Skripnya berjalan dalam satu `START TRANSACTION … COMMIT`, setiap pernyataan dibungkus penjaga
`IF NOT EXISTS`, dan pernyataan terakhirnya mendaftarkan sendiri baris `__EFMigrationsHistory`.
Tidak ada satu pun `DROP` di dalamnya.

**Bukti sebelum dan sesudah.**

| Pemeriksaan | Sebelum | Sesudah |
| --- | --- | --- |
| Kelima kolom baru ada pada `TrxPatientEncounterGuarantor` | `0` dari 5 | **5** dari 5, seluruhnya `nullable=YES` dengan tipe dan panjang sesuai konfigurasi EF |
| Index `IX_…_CompanyGuarantorId` dan `IX_…_PatientCompanyGuarantorId` | Tidak ada | **Ada keduanya** |
| Foreign key ke `MstCompanyGuarantor` dan `MstPatientCompanyGuarantor` | Tidak ada | **Ada keduanya**, `ON DELETE RESTRICT` |
| Baris `__EFMigrationsHistory` | Tidak ada | **Ada**, `ProductVersion` `9.0.18` |
| Jumlah baris sumber pembayaran existing | 170 | **170** — tidak ada yang hilang |
| Sebaran tipe pembayaran existing | — | **164 Tunai, 6 Asuransi** — tidak ada yang berubah |
| Baris existing yang kelima kolom barunya terisi | — | **0** — seluruh baris lama tetap `NULL`, sebagaimana mestinya untuk kolom nullable |
| Kelima migration IGD | `Pending` | **Tetap `Pending`** — tidak ada yang ikut terbawa |

Dikonfirmasi ulang lewat `dotnet ef migrations list`: hanya
`20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor` yang berpindah status menjadi
diterapkan; keenam baris lainnya tetap `(Pending)`.

**Peringatan untuk penerapan berikutnya.** Database bersama, staging, dan production akan
menghadapi **enam** migration tertunda sekaligus, bukan satu. Menjalankan `dotnet ef database
update` apa adanya di sana akan ikut menghapus tabel dan kolom IGD. Urutannya perlu direncanakan
pemilik database lebih dulu, dan pemeriksaan `TrxPatientAssessment.QueueId` yang bernilai kosong
perlu dilakukan sebelum `SET NOT NULL` dijalankan, karena baris kosong akan menggagalkan migration
di tengah jalan.


---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | `EncounterPaymentType` mempertahankan `Cash = 1` dan `Insurance = 2`, lalu menambah `CompanyGuarantor = 3` berlabel **Penjamin Perusahaan** | **Terpenuhi** | `EncounterPaymentType.cs`; test `MempertahankanNilaiTunaiDanAsuransiSertaMenambahPenjaminPerusahaan` dan `MetadataFilterMenampilkanOpsiPenjaminPerusahaan` |
| 2 | `POST /admin` menerima `PatientCompanyGuarantorId` hanya untuk tipe 3 dan menolak semua kombinasi payer campuran dengan kode 400 yang dapat dipahami | **Terpenuhi** | Matriks pada `ValidateCreateRequestAsync`; test `MenolakPayloadPayerCampuran` dengan 5 kombinasi |
| 3 | Kartu perusahaan wajib aktif, tidak dihapus, eligible, milik `PatientId` yang sama, berlaku pada tanggal encounter, dan menunjuk perusahaan aktif; kegagalan tidak membocorkan data pasien lain | **Terpenuhi** | `LoadValidPatientCompanyGuarantorAsync`; 7 test kelayakan, termasuk pemeriksaan bahwa pesan tidak memuat nama, nomor karyawan, atau nomor rekam medis |
| 4 | Encounter dan satu payment source tersimpan atomik; payment source menyimpan kedua foreign key perusahaan dan snapshot yang dikunci kontrak, sementara ketiga referensi Tunai/Asuransi bernilai `null` | **Terpenuhi** untuk lingkup source | Test `EncounterPetugasMenyimpanReferensiDanSnapshotPerusahaan` dan `KegagalanPenyimpananTidakMenyisakanEncounterMaupunSumberPembayaran`. Batas InMemory dicatat pada bagian 5.2 |
| 5 | Response create/detail/list yang memuat payment, opsi filter, dan summary mengenali tipe 3 serta mengembalikan field aditif; encounter perusahaan tidak merusak projection antrean dokter/perawat | **Terpenuhi** | Test `ResponseCreateMengembalikanFieldAditifPenjaminPerusahaan` dan `DetailDanSummaryMengenaliEncounterPenjaminPerusahaan`. Projection antrean tidak membaca kolom payer mana pun sehingga tidak terdampak; 786 test hijau termasuk seluruh test modul tetangga |
| 6 | Route `/admin` tetap memerlukan `PatientEncounter : Create`; route `/` dan `/kiosk` menolak tipe 3 sehingga wewenang kiosk tidak meluas | **Terpenuhi** | Atribut `[AccessPermission("PatientEncounter", "Create")]` tidak diubah; test `RouteKioskMenolakPenjaminPerusahaan` dan `RouteKioskTetapMenerimaTunaiDanAsuransi` |
| 7 | Kasus Tunai dan Asuransi existing tetap lulus tanpa perubahan payload; nilai enum lama tidak bergeser | **Terpenuhi** | Test `EncounterTunaiTetapTersimpanTanpaFieldPerusahaan` dan `EncounterAsuransiTetapTersimpanTanpaFieldPerusahaan`; 786 test hijau |
| 8 | Migration EF hanya dibuat dalam source dan tidak diterapkan ke database bersama/target tanpa otorisasi terpisah | **Terpenuhi** | Migration `20260831075231_…` ada di source. Penerapan **hanya** ke database dev pemilik, dan **hanya** sesudah pemilik memberi wewenang eksplisit atas database itu. Database bersama/target tidak disentuh. Bukti pada bagian 5.3 |

### 6.2 Definition of Done

| Butir DoD | Status | Catatan |
| --- | --- | --- |
| Delapan acceptance criteria lulus | **Terpenuhi** | Tabel 6.1 |
| Kontrak dan mapping source cocok | **Terpenuhi** | Kontrak bagian 3, 4, 5, 6, dan 7 dipetakan satu per satu ke source; hash kontrak diverifikasi ulang |
| Test/build backend hijau | **Terpenuhi** | `0 Error(s)`; 786/786 test lulus |
| Laporan task tracked menyertakan bukti file/symbol/SHA | **Terpenuhi** | Berkas ini; commit `d341cf5…`; snapshot kontrak `64d7419…` |
| `RWI-UI-GAP-002` ditandai tertutup untuk backend | **Terpenuhi** | `requirement-traceability.md` diperbarui pada task ini |
| Tidak ada migration yang diterapkan tanpa otorisasi | **Terpenuhi** | Penerapan dilakukan sesudah wewenang eksplisit diberikan pemilik untuk database dev miliknya, dan dibatasi hanya pada migration task ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan `135 Warning(s)`, seluruhnya `CS0618`, `CS8619`, `CS0162`, dan analyzer xUnit dari berkas legacy yang **tidak** disentuh task ini. Berkas yang berubah pada task ini menghasilkan **nol** peringatan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 7.3 |

### 7.1 Temuan yang dicatat, bukan diperbaiki

Empat hal ditemukan selama pengerjaan. Semuanya berada di luar wewenang task ini dan **sengaja
tidak disentuh**, sesuai aturan legacy ratchet dan batas cakupan kontrak bagian 8.

| # | Temuan | Kenapa tidak diperbaiki sekarang |
| ---: | --- | --- |
| 1 | **Entity masih bernama `Trx*`.** `TrxPatientEncounterGuarantor` dan `TrxPatientEncounter` melanggar `QBE-NAM-001` bila diperlakukan sebagai kode baru | Menormalkannya menjadi `Reg*` adalah `LEGACY MIGRATION` yang menuntut rename tabel fisik bersama source-nya (`QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002`), dan itu kampanye terpisah yang harus dinyatakan eksplisit. Roadmap `Scope` maupun kontrak bagian 8 tidak memberinya wewenang |
| 2 | **Controller mengakses `ApplicationDbContext` langsung.** `PatientEncounterController` sepanjang 2.573 baris tidak memakai module service, sehingga menyimpang dari `QBE-SVC-001` | `AGENTS.md` melarang menciptakan abstraksi baru ketika implementasi terdekat tidak memakainya. Membuat module service hanya untuk satu jalur validasi akan menghasilkan dua arsitektur yang berjalan sejajar di dalam satu controller — persis yang dilarang `API_RULES.md`. Perbaikannya perlu task tersendiri untuk seluruh controller |
| 3 | **Generator nomor sumber pembayaran memakai pola yang dilarang `QBE-CODE-003`.** `GenerateRunningCodeAsync` membaca seluruh kode yang ada lalu mencari angka kosong berikutnya, tanpa alokasi atomik di database. Dua pendaftaran serentak berpeluang memperebutkan nomor yang sama | Kode ini tidak disentuh task ini dan dipakai bersama oleh nomor kunjungan maupun nomor sumber pembayaran. Menggantinya dengan provider number-series yang atomik mengubah penomoran seluruh modul registrasi dan menuntut wewenang tersendiri. Index unik `PaymentSourceNumber` yang sudah ada menahan akibat terburuknya menjadi kegagalan `500`, bukan nomor kembar |
| 4 | **`POST /admin` tidak punya `[AccessAction]` pada method-nya sendiri.** Baris registry `Create` didaftarkan oleh method kiosk, dan `[AccessPermission("PatientEncounter", "Create")]` pada method admin menemukannya karena seeder mengunci baris berdasarkan pasangan controller dan nama action | Secara runtime **tidak** bermasalah: barisnya ada, sehingga admin dapat memberikannya lewat layar Akses Role, dan tidak terjadi `403` permanen. Pola yang sama dipakai konsisten oleh seluruh pasangan `…ForAdmin` / `…ForKiosk` pada controller ini. Roadmap kriteria 6 justru meminta hak aksesnya **dipertahankan apa adanya**, dan kontrak bagian 8 mengeluarkan perubahan hak akses dari cakupan |

### 7.2 Risiko tersisa

| Risiko | Keterangan |
| --- | --- |
| **Database selain dev pemilik belum dimigrasikan** | Schema dev pemilik sudah berubah, tetapi database bersama, staging, dan production belum. Terhadap database yang belum dimigrasikan, `POST /admin` dengan tipe `3` akan gagal di lapisan database. Penerapannya tetap wewenang terpisah |
| **Tabrakan dua pendaftaran serentak belum diuji** | Provider InMemory tidak menegakkan index unik. Gerbang terbuka yang sama sudah dicatat roadmap untuk `BE-RWI-011` dan berlaku juga di sini |
| **`403` belum dibuktikan runtime** | Bahwa pengguna tanpa `PatientEncounter : Create` benar-benar ditolak baru dibuktikan lewat atribut, belum lewat permintaan HTTP sungguhan |
| **`RWI-OQ-046` tetap terbuka** | `InpEpisodeService.BuildInpatientEncounter` masih membuat kunjungan sendiri dengan `PaymentType = Cash` yang ditanam di kode dan **tanpa** baris sumber pembayaran. Task ini tidak menutup jalur itu karena berada di luar cakupan kontrak. Selama jalur itu terbuka, admisi lewat jalan pintas tersebut tetap tercatat tunai walau pasiennya berpenjamin perusahaan |
| **Flag surat jaminan belum dipakai** | `IsNeedGuaranteeLetter`, `IsNeedEmployeeVerification`, dan `IsAllowExcessPaymentByPatient` tetap sekadar informasi master. Kontrak bagian 4 memang melarang task ini mengarang alur surat jaminan baru |

### 7.3 Status Git

```text
 M Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs
 M Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs
 M Areas/HealthServices/RegistrationManagement/Enums/EncounterPaymentType.cs
 M Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/Configurations/HealthServices/TrxPatientEncounterGuarantorConfiguration.cs
?? Migrations/20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor.Designer.cs
?? Migrations/20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor.cs
?? QuilvianSystemBackend.Tests/HealthServices/RegistrationManagement/
```

Seluruh perubahan di atas dibuat oleh task ini. Tidak ada perubahan pengguna yang sudah ada
sebelumnya di working tree ketika task dimulai. Berkas dokumentasi yang ikut diperbarui —
laporan ini, `backend-roadmap.md`, dan `requirement-traceability.md` — bertambah sesudah keluaran
di atas diambil.

Tidak ada `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, atau `switch` yang dijalankan.

### 7.4 Langkah berikutnya

| Urutan | Langkah | Penanggung jawab |
| ---: | --- | --- |
| 1 | Menerapkan migration yang sama ke database bersama, staging, dan production lewat wewenang terpisah. Di sana **enam** migration akan tertunda sekaligus, jadi urutannya perlu direncanakan lebih dulu — lihat peringatan pada bagian 5.3 | Pemilik database |
| 2 | Memulai `FE-RWI-025`, yang menunggu task ini | Frontend |
| 3 | Membuktikan `401`/`403` dan tabrakan dua pendaftaran serentak terhadap PostgreSQL sungguhan sesudah migration diterapkan | Backend/QA |
| 4 | Memutuskan apakah `RWI-OQ-046` ditutup, yaitu jalur admisi yang membuat kunjungan tunai tanpa sumber pembayaran | Product/Domain |
| 5 | Mempertimbangkan empat temuan pada bagian 7.1 sebagai kandidat task tersendiri | Backend/API RegistrationManagement |
