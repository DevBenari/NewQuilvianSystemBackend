# Laporan Perubahan Backend — `BE-BD-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-012` |
| Judul | Tindakan Bank Darah dicatat tanpa penyaluran biaya |
| Slice | `MVP-1` — jalur bekas `G4`; `ProcedureNumber` dari provider nomor bersama |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` revisi 8, blok task `BE-BD-012` |
| Trace | `DEC-BD-016`, `DEC-BD-021`, `DEC-BD-034`, **`DEC-BD-048`**, **`DEC-BD-049`** · `BD-AGG-05`, `BD-DOM-12` · `INV-BD-024` · `contracts/api-contract.md` §Blood Bank Procedure · `contracts/state-transition-matrix.md` §5 · `contracts/validation-matrix.md` §5 (`VAL-BD-026/027`, **`VAL-BD-084` baru**) · `data/data-dictionary.md` §`BbkBloodBankProcedure` · `testing/acceptance-test-matrix.md` §10 |
| Acceptance criteria | `AC-BD-098`, `AC-BD-099`, `AC-BD-100`, `AC-BD-101`, `AC-BD-102` — roadmap revisi 8 |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`); decision revision `12` |
| Dependency | `G1` ✅ · `G2b` ✅ · `G4` ✅ · `BE-BD-003` ✅ |
| Klasifikasi | `HEAVY` — skor 10: satu repository (0), berkas diperiksa lebih dari 20 (2), logika kompleks (2), memakai kontrak yang sudah disetujui (1), entity baru beserta migration (2), hak akses berkaitan tetapi bukan inti (1), dampak finansial lintas modul (2) |
| Task mode | `BACKEND` — dinyatakan eksplisit oleh pemilik pekerjaan |
| Target tulis | `NewQuilvianSystemBackend` — source `BE-BD-012`, migration, `Tests/**`, laporan, roadmap, traceability, `MODULE-STATUS`, serta tiga delta kontrak pada bagian 7 |
| Wewenang database | Migration dibuat dan diterapkan **hanya** ke `QuilvianNewDevSukma` — **dipakai**. Nama database diperiksa skrip penjaga sebelum apply; connection string tidak pernah dicetak |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `1edc87f` cabang `sukmagp` — "docs(bank-darah): unblock BE-BD-012 procedure delivery" |
| Tanggal | `2026-09-11` |
| Status | ✅ **`SELESAI`** — kelima kriteria terbukti; build, test, uji PostgreSQL, migration, dan `has-pending-model-changes` lulus. Dua tafsiran menunggu konfirmasi pemilik tanpa menahan kriteria mana pun (bagian 7). **Riwayat:** ⛔ `BLOCKED` pada hari yang sama sebelum implementasi, menunggu tiga keputusan yang lalu diputuskan pemilik lewat `DEC-BD-048`, `DEC-BD-049`, dan roadmap revisi 8 |

---

## 1. Masalah yang diselesaikan

Bank Darah melakukan tindakan — misalnya uji silang serasi atau penyiapan komponen — yang kelak menjadi
dasar biaya. `DEC-BD-021` menetapkan bahwa biaya Bank Darah **berasal dari tindakan, bukan dari
kantong**. Sebelum task ini tindakan itu tidak tercatat di mana pun.

Task ini mencatat tindakan beserta **salinan tarifnya** pada saat kejadian, lalu menyatakannya selesai.
Karena angkanya disalin, perubahan data induk tarif sesudahnya tidak mengubah catatan lama. Tidak ada
satu rupiah pun yang disalurkan ke Billing; penyaluran itu milik `BE-BD-013` dan masih tertahan
`DEC-BD-016`.

**Contoh.** Tindakan "Uji Silang Serasi" punya tarif umum Rp150.000 dan tarif kelas VIP Rp250.000.

| Keadaan | Hasil |
| --- | --- |
| Pasien dirawat di kelas VIP | Tarif VIP terpilih; salinan **Rp250.000** |
| Pasien dirawat di kelas 3, tidak ada tarif kelas 3 | Tarif umum menjadi cadangan; salinan **Rp150.000** |
| Tindakan hanya punya tarif kelas 1, pasien VIP | **Ditolak `422` `VAL-BD-084`** — tarif kelas lain tidak pernah dipakai |
| Sebulan kemudian tarif VIP dinaikkan menjadi Rp300.000 | Tindakan lama tetap **Rp250.000**; tindakan baru memakai Rp300.000 |

---

## 2. Proses bisnis

| Hal | Isi |
| --- | --- |
| Pelaku | Petugas Bank Darah mencatat; Dokter BDRS sebagai penanggung jawab tindakan |
| Langkah | **(1)** Petugas memilih order darah, tindakan bertarif, dan dokter BDRS. **(2)** Backend membaca unit dan kelas pasien dari kunjungan order. **(3)** Backend memilih tarif, menyalin kode, nama, dan nominalnya, lalu menerbitkan nomor. **(4)** Petugas menyatakan tindakan selesai |
| Status | `Recorded` (Dicatat) → `Completed` (Selesai). Tidak ada jalur lain |
| Yang dikirim client | Hanya `BloodOrderId`, `ProcedureRefId`, dan `BdrsDoctorId`. **Tidak ada** isian unit, kelas, tarif, maupun nominal |
| Yang ditentukan backend | Unit dan kelas (`DEC-BD-048`), tarif dan nominal (`DEC-BD-049`), nomor (`NumberSeriesAllocator`), petugas (dari akun yang login) |
| Batas | Nol endpoint, service, entity, atau producer penyaluran biaya (`DEC-BD-016`, `AC-BD-102`) |

### 2.1 Cara backend memilih tarif (`DEC-BD-049`)

1. **Kandidat** — `MstTariff` yang tidak dihapus, aktif, `ProcedureId`-nya sama dengan tindakan, dan
   berlaku pada tanggal pencatatan: `EffectiveStartDate` kosong atau sudah lewat, `EffectiveEndDate`
   kosong atau belum lewat.
2. **Cocok dengan kunjungan** — kolom `PatientClassId`, `ServiceUnitId`, dan `ClinicId` tarif boleh
   kosong (berlaku umum) atau wajib sama dengan kunjungan. Ini predikat yang sama persis dengan
   `InsuranceCoverageService.FindProcedureTariffAsync`. Tarif milik **kelas lain**, **unit lain**, atau
   **klinik lain** karena itu tidak pernah menjadi kandidat.
3. **Urutan** — tarif kelas pasien lebih dulu, lalu yang terikat klinik, lalu yang terikat unit, lalu
   tanggal mulai berlaku terbaru, lalu `SortOrder`, lalu `Id`. Tarif tanpa kelas dengan begitu hanya
   dipakai bila tarif kelas pasien tidak ada.
4. **Tidak ada kandidat** — ditolak `422` `VAL-BD-084`, **sebelum** nomor diminta, sehingga deret nomor
   tidak berlubang.
5. **Nominal** — `NormalPrice` tarif terpilih, tidak pernah negatif. Tanpa pengali, diskon, maupun
   coverage asuransi.

**Kenapa tidak memanggil `InsuranceCoverageService` langsung.** Method pemilihnya `private`, dan jalur
publiknya menghitung coverage asuransi: ia menolak bila konteks asuransi kunjungan belum siap, dan
memulangkan harga kontrak asuransi, bukan nominal `MstTariff`. Mencatat tindakan Bank Darah tidak boleh
bergantung pada kesiapan asuransi. Predikatnya disalin, dan setiap cabangnya diuji tersendiri (bagian 6).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · `CLAUDE.md` · suite skill `1.18.0`: `rules/backend/*`, `BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `transaction-endpoint-standard.md`, `role-access-rules.md`, `rules/rule-output/status-task-roadmap.md` |
| Kontrak modul | Kartu `BE-BD-012` roadmap revisi 8 · `requirement-traceability.md` · `api-contract.md` §Blood Bank Procedure · `validation-matrix.md` §5 · `data-dictionary.md` §`BbkBloodBankProcedure` dan §`BbkTransitionHistory` · `00-interview-decisions.md` §8.28 dan §9 · `acceptance-test-matrix.md` §10 |
| Source pembanding | `InsuranceCoverageService.cs` · `EncounterInsuranceService.cs` · `MstTariff.cs` · `MstProcedure.cs` · `MstTariffCategory.cs` · `MstPatientClass.cs` · `TrxPatientEncounter.cs` · `BbkBloodOrderService.cs` · `BbkProviderRequestService.cs` beserta controller-nya · `BbkTransitionHistory.cs` · `NumberSeriesAllocator.cs` · `BillingTestDatabaseFixture.cs` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkProcedureStatus.cs` | **Baru.** `Recorded = 0` "Dicatat", `Completed = 1` "Selesai" |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodBankProcedure.cs` | **Baru.** Entity `IdentityModel`, 13 kolom persis kamus data |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodBankProcedureConfiguration.cs` | **Baru.** `decimal(18,2)`, `ProcedureStatus` sebagai concurrency token, index unik `ProcedureNumber`, enam FK `Restrict` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodBankProcedureDtos.cs` | **Baru.** Satu request tiga isian, DTO daftar, DTO detail |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodBankProcedureService.cs` | **Baru.** Daftar, detail, catat, selesai; resolusi tarif; nomor `TND`; riwayat |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodBankProcedureController.cs` | **Baru.** Tepat empat endpoint kontrak `v4` |
| `Areas/HealthServices/BloodBankManagement/Models/BbkTransitionHistory.cs` | Konstanta `BbkTransitionScopes.BloodBankProcedure` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<BbkBloodBankProcedure> BbkBloodBankProcedures` |
| `Program.cs` | `AddScoped<BbkBloodBankProcedureService>()` |
| `Migrations/20260911072451_AddBbkBloodBankProcedure.cs` + `.Designer.cs` | **Baru.** Satu tabel, delapan index, enam FK `Restrict` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Bangkitan EF |
| `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/BloodBankProcedure/BloodBankProcedureServiceTests.cs` | **Baru.** 28 test service |
| `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/BloodBankProcedure/BloodBankProcedureServiceTests.Endpoints.cs` | **Baru.** 11 test kontrak endpoint dan pemetaan HTTP |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/BankDarah/BloodBankProcedurePostgresTests.cs` | **Baru.** 4 uji PostgreSQL |
| `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/BloodBankRoleAccessContractTests.cs` | Controller ke-8 didaftarkan; cakupan butir hak akses **25 → 28** dari 39 |
| `docs/module-blueprints/bank-darah/contracts/validation-matrix.md` | `VAL-BD-084` (bagian 7) |
| `docs/module-blueprints/bank-darah/data/data-dictionary.md` | Nilai `Scope` kelima dan catatan index `TariffId` (bagian 7) |
| `docs/module-blueprints/bank-darah/00-interview-decisions.md` | Klarifikasi pelaksanaan `DEC-BD-049` pada §8.28 (bagian 7) |
| `docs/module-blueprints/bank-darah/task/report/backend/BE-BD-012.md` | Laporan ini, menggantikan laporan ⛔ pada hari yang sama |
| `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` · `roadmap/requirement-traceability.md` · `roadmap/frontend-roadmap.md` · `MODULE-STATUS.md` | Tanda status dan tautan bukti. Pada roadmap frontend hanya cermin `BE-BD-012` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint baru, persis kontrak `v4`. Nol `GET /options`, `PUT`, `PATCH`, maupun `DELETE` — tindakan yang sudah terjadi tidak disunting atau dihapus |
| Database | Satu tabel baru `public."BbkBloodBankProcedure"`. Diterapkan ke `QuilvianNewDevSukma` saja. Nol tabel lama diubah |
| Keamanan/Auth | Tiga butir hak akses baru: `BloodBankProcedure : Read`, `Create`, `Update`. Nol hardcode peran. Petugas pencatat diambil dari klaim login, tidak pernah dari body |
| Billing | **Nol.** Nol ketergantungan ke `BillingManagement`, nol baris di tabel Billing (bagian 6) |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | `BloodBankManagement / Blood Bank` → prefix **`Bbk`** |
| Status registry | **`ACTIVE`** |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-ENT-001/003` · `QBE-NAM-001/002/004` · `QBE-CFG-001` · `QBE-MOD-001/002/003` · `QBE-SVC-001` · `QBE-API-001` · `QBE-PERM-001` · `QBE-LOG-001` · `QBE-DTO-001` · `QBE-VAL-001` · `QBE-TXN-001` · `QBE-CODE-001..006` untuk `ProcedureNumber` |
| Pengecualian QBE | `NONE` |
| Preflight | **Lolos** |

---

## 4. Dokumentasi endpoint

Grup Swagger **Health Services / Blood Bank Management / Blood Bank Procedure**. Base URL
`api/v1/health-services/blood-bank-management/blood-bank-procedures`. Seluruh endpoint menuntut login.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar tindakan Bank Darah | `BloodBankProcedure : Read` |
| `GET` | `/{id}` | Detail tindakan beserta riwayatnya | `BloodBankProcedure : Read` |
| `POST` | `/` | Catat tindakan atas satu order | `BloodBankProcedure : Create` |
| `POST` | `/{id}/complete` | Nyatakan tindakan selesai | `BloodBankProcedure : Update` |

### `GET /`

Query: `search` (nomor tindakan, kode/nama tindakan, nomor order, nama atau nomor rekam medis pasien),
`bloodOrderId`, `patientId`, `procedureStatus` (`0` Dicatat, `1` Selesai), `sortBy`
(`procedureNumber`, `procedureStatus`, bawaan waktu dibuat), `sortDirection` (`asc`/`desc`, bawaan
`desc`), `pageNumber` (bawaan 1), `pageSize` (bawaan 25, paling banyak 100).

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Daftar tindakan Bank Darah berhasil diambil.",
  "data": {
    "pageNumber": 1, "pageSize": 25, "totalData": 1, "totalPage": 1,
    "items": [{
      "id": "…", "procedureNumber": "TND-00000001",
      "bloodOrderId": "…", "orderNumber": "ORD-00000012",
      "patientName": "Pasien A", "medicalRecordNumber": "RM-A",
      "procedureCodeSnapshot": "BDR-SILANG", "procedureNameSnapshot": "Uji Silang Serasi",
      "tariffAmountSnapshot": 250000.00,
      "bdrsDoctorName": "dr. BDRS",
      "procedureStatus": 0, "procedureStatusLabel": "Dicatat"
    }]
  }
}
```

### `GET /{id}`

`200` memuat seluruh isian daftar ditambah kunjungan (`encounterNumber`), unit, kelas pasien,
`performedByUserId`, `tariffId`, `completedAt`, `completedByUserId`, `transitions` (riwayat
`Create` lalu `Complete`), `availableActions` (`["Complete"]` selama `Recorded`), dan kolom audit.
`404` bila tidak ada atau sudah dihapus.

### `POST /`

```json
{
  "bloodOrderId": "3f5c…",
  "procedureRefId": "8a21…",
  "bdrsDoctorId": "c7d4…"
}
```

Isian lain di body — misalnya `tariffAmount` atau `patientClassId` — **tidak punya tempat** di request
dan tidak pernah dibaca.

| HTTP | Kapan | Pesan |
| --- | --- | --- |
| `200` | Tercatat | "Tindakan Bank Darah berhasil dicatat." + detail lengkap |
| `400` | Order kosong, tidak ada, atau dihapus — `VAL-BD-026` | "Tindakan Bank Darah wajib menunjuk satu order yang sah." |
| `400` | Tindakan tidak ada atau nonaktif; dokter tidak ada atau dihapus; akun login tidak dikenali | Kalimat masing-masing |
| `422` | Tidak ada tarif yang cocok — `VAL-BD-084` | "Tarif tindakan ini belum diatur untuk kelas pasien dan unit kunjungan ini. Hubungi bagian data induk tarif." |
| `422` | Kunjungan order tidak ada, atau tidak mencatat kelas pasien | Kalimat masing-masing (bagian 7) |

### `POST /{id}/complete`

Tanpa body.

| HTTP | Kapan | Pesan |
| --- | --- | --- |
| `200` | `Recorded` → `Completed` | "Tindakan Bank Darah dinyatakan selesai." + detail |
| `404` | Tindakan tidak ada | "Tindakan Bank Darah tidak ditemukan atau sudah dihapus." |
| `409` | Petugas lain menyelesaikannya lebih dulu, di saat bersamaan | "Tindakan ini baru saja diubah petugas lain. Muat ulang lalu ulangi tindakan ini." |
| `422` | Tindakan sudah `Completed` | "Tindakan berstatus Selesai tidak dapat dinyatakan selesai lagi." |

Tidak ada respons yang memuat stack trace, SQL, connection string, maupun detail exception.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj -p:RunAnalyzers=false` | `0 Error(s)` | `PASS` |
| Build `QuilvianSystemBackend.Tests`, `IntegrationTests.Postgres`, `UnitTests.Sqlite`, `UnitTests.InMemory` dengan `-p:RunAnalyzers=false` | Keempatnya `0 Error(s)` | `PASS` |
| `dotnet ef migrations add AddBbkBloodBankProcedure` | Satu tabel `BbkBloodBankProcedure`; isi migration diperiksa: nol tabel lain tersentuh | `PASS` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` |
| Skrip penjaga, sebelum apply | Database `QuilvianNewDevSukma`; 140 migration, 139 applied, tepat 1 pending = migration task ini | `PASS` |
| Skrip penjaga `-Apply` | `QuilvianNewDevSukma` — **140 applied, 0 pending** | `PASS` |
| `dotnet test QuilvianSystemBackend.Tests` | **600/600 lulus** — naik dari 561; 39 test tindakan baru | `PASS` |
| Uji PostgreSQL Bank Darah di `QuilvianNewDevSukma` | **13/13 lulus** — 4 order darah, 5 permintaan PMI, **4 tindakan baru** | `PASS` |
| `dotnet test UnitTests.Sqlite` | **231/231 lulus** | `PASS` |
| `dotnet test UnitTests.InMemory` | 896/905 — **9 gagal, seluruhnya Billing** (`BillingInvoiceServiceTests`, `BillingCalculationServiceTests`, `BillingFinalizationServiceTests`) | `PRE-EXISTING` — baseline sebelum task ini; nol test Bank Darah di dalamnya |

Build dijalankan dengan `-p:RunAnalyzers=false` atas instruksi pemilik. Karena itu jumlah warning
**tidak** dibandingkan dengan baseline `210 Warning(s)` laporan sebelumnya, yang diukur dengan analyzer
aktif.

**Satu kegagalan selama pengerjaan, sudah diperbaiki.** Test InMemory penyelesaian bersamaan semula
menuntut tepat satu baris riwayat `Complete`, dan mendapat dua. Hasil `409` untuk petugas kedua sudah
benar; yang terjadi adalah provider InMemory tidak bertransaksi, sehingga baris riwayat milik
penyelesaian yang ditolak tertulis sebelum pemeriksaan concurrency token. Di PostgreSQL `SaveChanges`
berjalan dalam satu transaksi dan baris itu ikut batal — dibuktikan
`PenyelesaianBersamaanDuaPetugas_HanyaSatuYangTercatat`. Test InMemory kini memeriksa status dan
pelaku yang tersimpan. Nol perubahan source.

Uji manual: `NOT APPLICABLE` — seluruh perilaku dibuktikan otomatis.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-098` pencatatan tindakan | ✅ **Terbukti** | `AC_BD_098_TindakanDicatat_BernomorDariProvider_KonteksDariKunjungan_StatusRecorded` — nomor `TND-00000001` dari provider, order, tindakan, dokter, petugas login, unit ICU dan kelas VIP dari kunjungan, `TariffId`, `Recorded`, baris riwayat `Create`. `NomorTindakan_BerurutanDanTidakPernahSama`. Jalur gagal: `AC_BD_098_OrderTidakSah_Ditolak_VAL_BD_026` (kosong, acak, dihapus), `TindakanBertarifTidakSah_Ditolak` (kosong, acak, nonaktif), `PelakuAtauDokterTidakSah_Ditolak` — seluruhnya tanpa baris dan tanpa nomor terbit. PostgreSQL: `SepuluhTindakanSerentak_SepuluhNomorBerbeda`, `NomorTindakan_DijagaIndexUnikFisikDiDatabase`, `TindakanDenganTarifFiktif_DitolakForeignKeyDatabase` |
| `AC-BD-099` resolusi tarif | ✅ **Terbukti** | `AC_BD_099_TarifKelasPasienTerpilih_BukanKelasLainNonaktifKedaluwarsaAtauUnitLain` — dari tujuh tarif, hanya VIP Rp250.000 terpilih; tarif kelas 1, VIP nonaktif, VIP kedaluwarsa, VIP belum berlaku, dan VIP milik unit lain tidak. `AC_BD_099_TarifUmumMenjadiCadangan_BilaTarifKelasPasienTidakAda` — kelas 3 → Rp150.000. `AC_BD_099_KelasPasienDiutamakan_LaluTarifUmumYangPalingSpesifik`. `AC_BD_099_TarifMilikKelasLainTidakPernahDipakai_Ditolak422`, `AC_BD_099_TindakanTanpaTarifSamaSekali_Ditolak422_NomorTidakTerbit`, `AC_BD_099_HanyaTarifNonaktifKedaluwarsaAtauBelumBerlaku_Ditolak422`. Nominal client: `AC_BD_099_PermintaanPencatatan_TidakMemuatNominalTarifUnitMaupunKelas` — request hanya punya tiga isian. HTTP: `Controller_TarifTidakTersedia_Menjadi422_VAL_BD_084` |
| `AC-BD-100` salinan beku | ✅ **Terbukti** | `AC_BD_100_DataIndukTindakanDanTarifBerubah_SalinanPadaTindakanLamaTidakBerubah` — kode, nama, dan nominal di data induk diganti; tindakan lama tetap `BDR-SILANG`, "Uji Silang Serasi", Rp250.000, sedangkan tindakan baru memakai nilai baru |
| `AC-BD-101` penyelesaian | ✅ **Terbukti** | `AC_BD_101_RecordedMenjadiCompleted_TransisiDanAuditTersimpan` — status, label, `UpdateBy`, `UpdateDateTime`, `CompletedByUserId`, `CompletedAt`, riwayat `Create` → `Complete`. Jalur gagal: `AC_BD_101_MenyelesaikanDuaKali_DitolakTerkendali_StatusDanAuditTidakBergerak` (`422`, tetap satu baris riwayat), `AC_BD_101_PenyelesaianBersamaanDuaPetugas_YangKeduaDitolak409`, `AC_BD_101_TindakanTidakAdaAtauTanpaPelaku_Ditolak`. PostgreSQL: `PenyelesaianBersamaanDuaPetugas_HanyaSatuYangTercatat` — tepat satu baris riwayat |
| `AC-BD-102` tanpa Billing | ✅ **Terbukti** | `AC_BD_102_DibuatDanDiselesaikan_NolBarisBilling` — nol baris `BilInvoice`, `BilInvoiceItem`, `BilFolio`, `BilChargeLine`, `BilChargeComponent`, `BilProcessingEffect`. `AC_BD_102_TidakAdaTipeBankDarahYangBergantungPadaBillingAtauProducerBiaya` — nol tipe Bank Darah bergantung pada `BillingManagement` atau producer biaya; entity tanpa kolom tagihan. `Controller_TepatEmpatEndpointKontrak_TanpaJalurBilling` |

**Bukti tambahan di luar kelima kriteria.** Hak akses: `Endpoint_MemakaiRouteDanButirHakAksesYangDikunciKontrak`
(empat endpoint) dan `BloodBankRoleAccessContractTests` (28 dari 39). Pelaku dari klaim login:
`Controller_TanpaKlaimPengguna_TidakAdaTindakanYangLahirMaupunSelesai`. Kolom persis kamus data:
`KolomTersimpan_SamaPersisDenganKamusData`.

### 6.2 Definition of Done

| Butir DoD | Status |
| --- | --- |
| Source dalam scope selesai | **Terpenuhi** — entity, konfigurasi, DTO, service, controller, registrasi |
| Number Series Platform direuse | **Terpenuhi** — `NumberSeriesAllocator`, deret `BBK_PROCEDURE`, awalan `TND`, 8 digit, tanpa pengulangan |
| Seluruh AC terbukti | **Terpenuhi** — 5 dari 5 |
| Build | **Terpenuhi** — `0 Error(s)` |
| Test | **Terpenuhi** — 600/600, 231/231; InMemory hanya 9 kegagalan Billing baseline |
| PostgreSQL | **Terpenuhi** — 13/13 |
| Migration diterapkan | **Terpenuhi** — `QuilvianNewDevSukma` `140/140` |
| Pending clean dan `has-pending-model-changes` | **Terpenuhi** |
| Laporan, roadmap, traceability tersinkron | **Terpenuhi** — berkas ini dan register pada bagian 3.2 |

Nol butir DoD dikecualikan.

---

## 7. Delta kontrak dan tafsiran

| No | Hal | Yang dijalankan | Dicatat di | Butuh keputusan? |
| ---: | --- | --- | --- | --- |
| 1 | **Kode penolakan "tarif tidak ditemukan"** | `VAL-BD-084`, `422`, melanjutkan `VAL-BD-083` | `validation-matrix.md` §5 | Tidak — turunan langsung `DEC-BD-049` |
| 2 | **`Scope` riwayat untuk tindakan** | Nilai kelima `BloodBankProcedure`; kolom sudah `string(30)`, nol perubahan schema | `data-dictionary.md` §`BbkTransitionHistory` | Tidak |
| 3 | **Urutan prioritas tarif** | Kelas pasien lebih dulu, lalu klinik, unit, tanggal mulai terbaru. Teks `DEC-BD-049` menulis "klinik, lalu unit, lalu kelas", yang pada satu keadaan membuat tarif tanpa kelas mengalahkan tarif kelas pasien — bertentangan dengan "tarif tanpa kelas menjadi cadangan" pada keputusan yang sama | `00-interview-decisions.md` §8.28, klarifikasi pelaksanaan | Tidak — mengikuti instruksi pemilik pada task ini. Dibuktikan `AC_BD_099_KelasPasienDiutamakan_LaluTarifUmumYangPalingSpesifik` |
| 4 | **Kunjungan tanpa kelas pasien** | Ditolak `422`, tanpa kode, nomor tidak terbit. Kamus data mewajibkan `PatientClassId`, dan §8.28 menyatakan kamus data tidak berubah. Dibuktikan `KunjunganTanpaKelasPasien_Ditolak422` | §8.28 | **Ya — konfirmasi pemilik.** Kalimat `DEC-BD-049` "bila kunjungan tidak mencatat kelas, hanya tarif tanpa kelas yang cocok" dapat dibaca sebagai "tetap boleh dicatat". Bila itu yang dikehendaki: satu migration aditif menjadikan `PatientClassId` boleh kosong, lalu penolakan ini dicabut. Tidak menahan kriteria mana pun |
| 5 | **Arti "order sah"** (`VAL-BD-026`) | Order ada, tidak dihapus, dan tidak ber-flag batal. Status bisnisnya tidak dibatasi, sehingga tindakan tetap dapat dicatat atas order `Fulfilled` atau `Expired` — tindakannya bisa sudah dikerjakan sebelum order berhenti | Laporan ini | **Ya — konfirmasi pemilik proses BDRS.** Pertanyaan yang sama sudah diajukan laporan ⛔ pagi ini |
| 6 | Index `IX_BbkBloodBankProcedure_TariffId` | Dibuat EF otomatis untuk FK; kamus data menulis "—" | `data-dictionary.md`, catatan ¹ | Tidak |
| 7 | Concurrency token | `ProcedureStatus` sebagai token, karena kamus data tidak punya kolom `Version`. Penyelesaian ganda serentak menghasilkan tepat satu keberhasilan | Laporan ini | Tidak |
| 8 | Syarat data induk | Tindakan wajib aktif dan tidak dihapus; dokter BDRS wajib ada dan tidak dihapus; nominal tidak pernah negatif | Laporan ini | Tidak |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Dua konfirmasi pemilik pada bagian 7 nomor 4 dan 5 |
| Risiko tersisa | **(1)** Predikat tarif **disalin**, tidak dipanggil bersama. Bila `InsuranceCoverageService` kelak mengubah predikatnya, salinan Bank Darah dapat menyimpang dari angka Billing; penyatuan ke satu resolver bersama menjadi pekerjaan pemilik Billing. **(2)** Tanggal berlaku tarif dinilai pada tanggal UTC pencatatan, sama dengan `InsuranceCoverageService` |
| Temuan di luar scope | **(1)** `UnitTests.InMemory` punya 9 kegagalan Billing baseline — milik BillingManagement. **(2)** Kartu `FE-BD-010` pada roadmap frontend masih menulis "`BE-BD-012` menunggu `BE-BD-003`"; statusnya milik perencanaan frontend. Yang diubah task ini hanya cermin `BE-BD-012` |
| Perubahan sampingan | `NONE` — snapshot migration adalah bangkitan EF milik migration task ini |
| Interupsi | Satu kali: konteks sesi dipadatkan setelah empat berkas pertama ditulis. Dilanjutkan dari `git status` dan isi berkas yang terverifikasi, tanpa penulisan ganda |
| Status Git | Seluruh perubahan pada bagian 3.2 belum di-stage, belum di-commit. Nol push |
| Langkah berikutnya | Keputusan penerusan tiga kriteria `BE-BD-004` oleh pemilik roadmap, lalu `BE-BD-015` di jalur kritis |
