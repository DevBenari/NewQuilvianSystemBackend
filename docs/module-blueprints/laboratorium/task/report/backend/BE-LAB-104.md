# Laporan Perubahan Backend — `BE-LAB-104`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-104` |
| Judul | Skema penyerahan hasil, setelan aktivasi dan daftar sensitif, izin, dan seeder |
| Slice | `S17-1` penyerahan hasil kepada pasien — gelombang `MVP-17a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6aw.1 |
| Trace | `FR-22.10`, `FR-22.11`, `FR-22.12`; `LAB-DEC-294`..`296`; `02-backend-architecture.md` 34.4–34.8, 34.10 no. 2, 3, 8, 12; `INV-80` |
| Contract version | `LAB-API-v1` **`r51`** 46.3; `LAB-PERM-v1` **revision 19** bagian 21; `LAB-VAL-v1` **`r23`** `VAL-178`, `VAL-179`; `LAB-STATE-v1` **`r13`** 15.2 — keempatnya **`approved` 2026-11-27** (Yoga Aji Pratama) |
| Dependency | Nol |
| Klasifikasi | `HEAVY` — repository 0, berkas diperiksa 2 (±25), berkas diubah 2 (21 baru, 4 diubah), logika 1 (urutan penjaga), kontrak API 2 (lima endpoint baru), database 2 (enam tabel + migration), keamanan 2 (resource izin baru + seeder kebijakan) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Lab, `ApplicationDbContext`, `Program.cs`, `appsettings.json`, file migration; laporan, roadmap, traceability. **Migration tidak diterapkan**, **tanpa** menulis basis data bersama, **tanpa** commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `693941d5` (branch `yoga`) — sama dengan SHA roadmap |
| Tanggal | 2026-11-27 |
| Status | ✅ **Selesai — batas verifikasi**: build lulus; harness SQLite **54/54**; SQL migration ditinjau. HTTP pada aplikasi berjalan menunggu migration diterapkan ke devYoga (langkah rilis `MVP-17e` no. 1). Tiga aksi `LabPatientResult` baru terdaftar ketika endpointnya lahir (bagian 7) |

### Gerbang masukan

| Butir | Hasil |
| --- | --- |
| Status task | `SIAP DIKERJAKAN` — task pertama `MVP-17` |
| Kontrak | Empat kontrak `approved` 2026-11-27; nol perubahan kontrak pada task ini |
| Hash masukan | 13 berkas pada tabel *Hash masukan* traceability `MVP-17` dihitung ulang (`tr -d '\r' \| sha256sum`) sesudah implementasi — **13/13 sama** |
| Source | `HEAD` = `693941d5` = SHA roadmap — tidak basi |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Governance terbaca | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` pada suite Skill (`TASK_RULES`, `DATABASE_RULES`, `API_RULES`, `REPORT_TEMPLATE`) |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix | `Lab` — **`ACTIVE`** (registry baris 31) |
| Keberlakuan | Enam model, enam konfigurasi, enum, DTO, service, controller, seeder = **`NEW CODE`**; `ApplicationDbContext`, `Program.cs`, `appsettings.json`, snapshot migration = **`TOUCHED LEGACY`** (aditif) |
| QBE yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-NAM-001`/`002` (prefix `Lab`, tanpa `Trx`), `QBE-CFG-001` (`IEntityTypeConfiguration` per entity), `QBE-MOD-001`/`002` (submodul terdaftar), `QBE-SVC-001` (controller → service), `QBE-API-001` (`ApiResponse`, `404/409/422`), `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001` (pelaku pada log), `QBE-DEL-001` (cabut = tanda hapus beralasan), `QBE-AUD-001` |
| Tidak berlaku | `QBE-CODE-*` — nol nomor bisnis pada task ini (nomor dokumen = `BE-LAB-106`); `QBE-NAM-003`, `QBE-DB-*` — bukan `LEGACY MIGRATION` |
| Wewenang migration | Pembuatan file migration: preferensi pemilik 2026-10-05 (`AGENTS.md` *Aturan Entity Framework*). **Penerapan** ke basis data: belum ada instruksi — tidak dilakukan |
| Pengecualian | Nol |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, Laboratorium **tidak punya tempat** untuk dokumen hasil yang diserahkan kepada pasien, catatan pengiriman,
maupun pengaman yang memutuskan kapan penyerahan boleh dimulai. Kolom *Terkirim ke Pasien* selalu *Belum Terkirim*
(`CAP-P34-14`, `CAP-P34-16`).

Task ini meletakkan **fondasinya**: enam tabel aggregate dokumen hasil pasien dan setelannya, layar-API bagi Kepala
Instalasi untuk **mengaktifkan penyerahan per disiplin** dan **mengelola daftar pemeriksaan sensitif**, serta pemberian izin
bawaan. **Belum ada perilaku penyerahan** — dokumen, kirim, dan cetak menyusul pada `BE-LAB-105`..`107` dan `BE-LAB-41`.

## 2. Proses bisnis

**Jalur normal — membuka penyerahan sesudah konfirmasi klinis.**

1. Hari ini penyerahan hasil **nonaktif** untuk Patologi Klinik dan Mikrobiologi — tanpa baris aktivasi berarti nonaktif.
2. dr. Bima (Kepala Instalasi, diberi `LabPatientDeliverySetting : Read/Update` lewat Role Access) membuka setelan:
   `GET /lab-patient-delivery-settings` → keduanya nonaktif, daftar sensitif kosong, `verificationBaseUrlConfigured: false`.
3. Ia menandai *Anti-HIV* sensitif: `POST /sensitive-procedures` `{"procedureId": "…", "note": "Pendampingan dokter"}` →
   `201`.
4. Sesudah alamat verifikasi QR dikonfigurasi admin platform, ia mengaktifkan Patologi Klinik:
   `PUT /ClinicalPathology/activation` `{"enabled": true, "reason": "Konfirmasi LAB-SIGN-002 1 Desember", "sensitiveListConfirmed": true}`
   → `200`; satu baris jejak tercatat (pelaku, waktu, alasan).
5. Bila terjadi insiden, ia menonaktifkan lagi dengan alasan — jejak bertambah, dokumen dan catatan kirim kelak **tidak**
   berubah.

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Mengaktifkan Patologi Anatomi | `422` *Penyerahan hasil Patologi Anatomi belum tersedia.* |
| Alasan kosong / spasi | `422` *Tuliskan alasan perubahan.* |
| Mengaktifkan tanpa centang daftar sensitif | `422` *Tegaskan bahwa daftar pemeriksaan sensitif sudah ditetapkan.* |
| Mengaktifkan yang sudah aktif / menonaktifkan yang sudah nonaktif | `409` `NoChange` *Penyerahan Patologi Klinik sudah aktif.* |
| Mengaktifkan selama alamat verifikasi kosong | `409` `VerificationUrlMissing` *Alamat verifikasi QR belum dikonfigurasi; hubungi admin platform.* |
| Menandai pemeriksaan yang sudah ditandai | `409` `AlreadyMarked` |
| Jenis pemeriksaan tidak ada | `404` *Jenis pemeriksaan tidak ditemukan.* |
| Mencabut tanpa alasan | `422` *Tuliskan alasan pencabutan.* |
| Analis (tanpa izin setelan) | `403` |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`LabOrderReschedule.cs` + konfigurasinya (pola tabel riwayat), `LabOrderReschedulePermissionSeeder.cs` (pola seeder),
`LabDisciplineSettingController.cs`/`Service.cs` (pola setelan per disiplin, pelaku, `AuditAsync`), `LabOrderController.cs`
(pemetaan `409` ber-`code`), `ApplicationDbContext.cs`, `Program.cs` (DI, urutan seeder sesudah `AccessMenuSeeder`),
`MstProcedure.cs`, `IdentityModel.cs`, `ApiResponse.cs`, `LoggerService.cs`, konfigurasi FK `ApplicationUser` Billing,
`appsettings.json` (`FileStorage`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Enums/LabPatientResultEnums.cs` | **Baru** — lima enum 34.6 |
| `…/Models/LabPatientResultDocument.cs`, `LabPatientResultVersion.cs`, `LabPatientResultApproval.cs`, `LabPatientResultDelivery.cs` | **Baru** — aggregate `AGG-LAB-04` (kamus data 25.1–25.4) |
| `…/Models/LabPatientDeliveryActivation.cs`, `LabSensitiveProcedure.cs` | **Baru** — setelan (25.5–25.6) |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/` enam `*Configuration.cs` | **Baru** — FK `Restrict`, index unik tersaring `IsDelete = false`, `IsConcurrencyToken` |
| `Repositories/ApplicationDbContext.cs` | Enam `DbSet` |
| `…/DTOs/LabPatientResultDtos.cs` | **Baru** — tiga request + empat response 46.3 |
| `…/Services/LabPatientDeliverySettingService.cs` | **Baru** — aktivasi (`VAL-178`), jejak, daftar sensitif (`VAL-179`); `LabPatientResultConflictException` (`409` + `code`), `LabPatientResultValidationException` (`422`) |
| `…/Controllers/LabPatientDeliverySettingController.cs` | **Baru** — lima endpoint, `LabPatientDeliverySetting : Read/Update` |
| `…/Seeders/LabPatientResultPermissionSeeder.cs` | **Baru** — salin `LabOrder : Hold` → `LabPatientResult : Send/Print`, sekali jalan per aksi; `Approve` tidak pernah |
| `Program.cs` | DI `LabPatientDeliverySettingService`; `RunStartupSeederAsync("LabPatientResultPermissionSeeder", …)` sesudah seeder jadwal ulang |
| `appsettings.json` | `LabPatientResult:VerificationBaseUrl` kosong (34.10 no. 12) |
| `Migrations/20261127073202_AddLabPatientResultDelivery.cs` + `.Designer.cs`, `ApplicationDbContextModelSnapshot.cs` | **Baru**/diperbarui — enam `CreateTable`, index, FK; **bagian Farmasi dibuang** (bagian 7) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint baru 46.3. Nol perubahan pada endpoint yang ada |
| Database | Enam tabel baru `public.LabPatientResultDocument`, `LabPatientResultVersion`, `LabPatientResultApproval`, `LabPatientResultDelivery`, `LabPatientDeliveryActivation`, `LabSensitiveProcedure`; **nol** kolom/`ALTER`/`DROP` pada tabel yang ada. Migration **belum diterapkan** |
| Keamanan/Auth | Resource izin baru `LabPatientDeliverySetting` (`Read`/`Update`) — terpisah dari `LabDisciplineSetting : Update` (`LAB-DEC-296`). Seeder memberi `LabPatientResult : Send/Print` kepada pemegang `LabOrder : Hold`; `Approve` tidak. Pelaku wajib pengguna nyata (FK `AspNetUsers`, `401` bila tidak dikenali). Log memuat pelaku dan disiplin, **tanpa** alasan |

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Patient Delivery Setting

Base URL: `api/v1/health-services/laboratory-management/lab-patient-delivery-settings`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Aktivasi PK dan Mikrobiologi, daftar pemeriksaan sensitif, kesiapan alamat verifikasi | `LabPatientDeliverySetting : Read` |
| `PUT` | `/{discipline}/activation` | Mengaktifkan/menonaktifkan penyerahan satu disiplin, beralasan | `LabPatientDeliverySetting : Update` |
| `GET` | `/{discipline}/activation-history` | Jejak aktivasi satu disiplin, terbaru di atas | `LabPatientDeliverySetting : Read` |
| `POST` | `/sensitive-procedures` | Menandai satu jenis pemeriksaan sensitif | `LabPatientDeliverySetting : Update` |
| `DELETE` | `/sensitive-procedures/{id:guid}` | Mencabut penanda sensitif, beralasan (body) | `LabPatientDeliverySetting : Update` |

`{discipline}` = nama enum `LabDiscipline`: `ClinicalPathology`, `Microbiology`; `AnatomicalPathology` → `422`; nilai tak
dikenal → `400`. Respons `409` membawa `errors.code`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` (server lokal BE/FE mati, nol proses `dotnet` lain) | Berhasil, 4 m 18 d | `PASS` | Keluaran *Build succeeded* |
| `dotnet ef migrations add AddLabPatientResultDelivery` | Berhasil; memuat dua tabel Farmasi di luar scope → dibuang (bagian 7) | `PASS` | 6 `CreateTable` + 6 `DropTable`; snapshot +515 baris, −0 |
| Build ulang lewat harness (backend dengan migration tersunting) | Berhasil | `PASS` | *Build succeeded* |
| `dotnet ef migrations script 20261126091428_AddProcurementSourcing AddLabPatientResultDelivery` | 6 `CREATE TABLE`, 9 FK `ON DELETE RESTRICT`, 5 `CREATE UNIQUE INDEX … WHERE "IsDelete" = false`, nol `ALTER`/`DROP` | `PASS` | SQL di scratchpad sesi |
| Harness SQLite `T-S17-10` aktivasi (21 kasus) | Seluruh jalur 46.3/`VAL-178` sesuai urutan; nol tulis saat ditolak; jejak 2 baris terbaru di atas; label Indonesia; log tanpa alasan; `401` pengguna tak dikenal | `PASS` | 21/21 |
| Harness `T-S17-11` daftar sensitif (11 kasus) | Tambah `201`, ganda `409`, tidak ada `404`, cabut tanpa alasan `422`, cabut beralasan → tanda hapus + pelaku, tandai ulang = baris baru | `PASS` | 11/11 |
| Harness atribut + registry (9 kasus) | Route, tag, lima verb/izin; `PermissionRegistryValidator` lulus; `LabPatientDeliverySetting` Read/Update di modul Lab; `LabDisciplineSetting` tidak bertambah aksi | `PASS` | 9/9 |
| Harness `T-S17-12` seeder + filter izin sungguhan (10 kasus) | Seeder no-op sebelum aksi terdaftar; memberi `Send`+`Print` hanya kepada pemegang `Hold` aktif; `Approve` tidak; sekali jalan; pencabutan tidak dipulihkan; Analis `403` pada setelan, Kepala `200`; Analis `Print` `200`, `Approve` `403` | `PASS` | 10/10 |
| Harness skema (3 kasus) | Enam tabel `public`; 9 FK seluruhnya `Restrict`; tanpa kolom angka pada `LabOrder` | `PASS` | 3/3 — total **54/54** |
| Indeks unik tersaring pada data nyata | Harness SQLite mencabutnya; ganda dijaga pemeriksaan layanan | `NOT RUN` | Dibuktikan lewat SQL migration; PostgreSQL portabel tidak dijalankan |
| Kontrol negatif (mutan) | Tidak dijalankan | `NOT RUN` | Uji tiap aturan memakai pesan/kode persis; risiko rendah |
| HTTP devYoga | Migration belum diterapkan | `NOT RUN` | Langkah rilis `MVP-17e` no. 1–3 |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-370` sisi setelan: disiplin belum diaktifkan = keadaan bawaan | Terpenuhi | GET awal nonaktif; gerbang yang membacanya = `BE-LAB-106` |
| `AC-371`: aktivasi tercatat (pelaku, waktu, alasan); pemegang `LabDisciplineSetting : Update` tanpa izin aktivasi → `403` | Terpenuhi | Jejak + filter izin; registry tidak menambah aksi pada `LabDisciplineSetting` |
| `AC-372` sisi setelan: menonaktifkan dapat dibalik, tercatat | Terpenuhi | Aktif → nonaktif, dua baris jejak. *Dokumen/catatan tidak berubah* diuji `BE-LAB-106`/`41` |
| `AC-364` sisi seeder: pemberian bawaan `Send` | Terpenuhi pada kode | Seeder; aksi nyata terdaftar ketika endpoint lahir |
| `T-S17-10`, `T-S17-11`, `T-S17-12` lulus | Terpenuhi (harness) | 54/54 |
| Migration + Designer + snapshot di working tree, **tidak** diterapkan | Terpenuhi | `git status` |
| Laporan `task/report/backend/BE-LAB-104.md` | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Temuan di luar scope:** model `PhmStockOpname` dan `PhmStockOpnameLine` (Farmasi, merge `3b9c6f843`) masuk source **tanpa migration**. `migrations add` ikut membangkitkan keduanya; bagian itu **dibuang** dari migration, Designer, dan snapshot dengan pencocokan kurung (sisa kata `PhmStockOpname` = 0), sehingga migration ini hanya memuat enam tabel Lab. Akibatnya **migration berikutnya oleh siapa pun akan membawa kedua tabel Farmasi** — perlu disampaikan ke pemilik Farmasi |
| Masalah yang diketahui | (1) Aksi `LabPatientResult : Send/Print/Approve` (`LAB-PERM-v1` 21.2) **belum terdaftar** — registri lahir dari atribut endpoint, dan endpointnya milik `BE-LAB-105`/`106`/`41`/`107`. Seeder sudah aman: no-op sampai aksi lahir, lalu menyalin sekali. (2) Kontrak 46.3 menulis `AnatomicPathology`; nama enum sebenarnya `AnatomicalPathology` — perilakunya sama (`422`), penyegaran teks kontrak dicatat untuk pass blueprint (kontrak terkunci hash). (3) Respons setelan menambah `disciplineName` (label Indonesia) di samping `discipline` — aditif. (4) Kelas exception ditaruh di akhir `LabPatientDeliverySettingService.cs` (pola Lab), bukan berkas tersendiri |
| Risiko tersisa | Rendah. Dua perubahan aktivasi bersamaan dapat menyisipkan dua baris berkeadaan sama — keadaan kini tetap benar (baris terbaru) |
| Perubahan sampingan | Dua tabel Farmasi pada migration/Designer/snapshot yang dibangkitkan tool — **dibuang** (di luar scope, muncul sebagai efek samping, bukan pekerjaan Lab) |
| Interupsi | `NONE` |
| Status Git | Source task ini: 21 berkas baru + `ApplicationDbContext.cs`, `Program.cs`, `appsettings.json`, `ApplicationDbContextModelSnapshot.cs` diubah. Dokumen blueprint Lab diubah oleh sesi ini (desain/roadmap `MVP-17`). `docs/module-blueprints/human-resource/…` (2 diubah, 1 baru) **bukan** dari task ini |
| Langkah berikutnya | `BE-LAB-105` — isi dokumen, templat PDF, *Preview*. Penerapan migration ke devYoga menunggu instruksi pemilik (`MVP-17e` no. 1) |
