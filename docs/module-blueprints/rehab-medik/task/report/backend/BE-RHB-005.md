# Laporan Perubahan Backend — `BE-RHB-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-005` |
| Judul | Konfigurasi layanan rehab |
| Slice | S-A — Fondasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-005` |
| Trace | RHB-DEC-004, 017, 020; V-CFG-01/02; FR-RHB-070 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Discipline* |
| Dependency | `BE-RHB-001` ✅, `BE-RHB-003` ✅ |
| Klasifikasi | `STANDARD` — master data baru, satu controller, tanpa schema baru (tabel dari `BE-RHB-002`) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs,Seeders,Constants}` + `Program.cs` + dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-05 |
| Status | **✅ Selesai** (2026-11-05). Source selesai, build `PASS`, QBE Strict `PASS`, uji API manual di dev 26/26 sesuai harapan, cek DB `PASS`, seeder idempoten. Satu cabang belum diuji di runtime: penolakan `DELETE` untuk layanan yang sudah dipakai (kriteria 3). Cabang ini terbukti secara statis, dan pembuktian runtime-nya diteruskan ke verifikasi `BE-RHB-007` (§6). |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE` (2026-11-05) |
| Jenis capability | **Master data**: standar 9 endpoint baseline + sub-resource |
| QBE yang berlaku | QBE-NAM (prefix `Rhb`/`Rehab`), QBE-SVC-001 (service langsung ke `ApplicationDbContext`, tanpa generic repository, tanpa DbContext di controller), QBE-API-001, standar hak akses (`[AccessController]`/`[AccessAction]`/`[AccessPermission]` dengan konstanta) |

## 1. Masalah yang diperbaiki

Modul rehab belum punya daftar layanan (Fisioterapi, Terapi Okupasi, Terapi Wicara,
Ortotik-Prostetik). Tanpa daftar itu, dokter tidak bisa meresepkan disiplin, tidak ada aturan
profesi mana yang boleh mengisi asesmen disiplin, dan tidak ada daftar tindakan yang bisa
ditagih.

**Contoh.** Admin mengaktifkan Fisioterapi. Sistem menolak aktivasi itu kalau profesi klinis
belum dipilih, karena tanpa profesi sistem tidak tahu siapa yang berwenang menandatangani sesi
fisioterapi.

## 2. Proses bisnis

1. Saat aplikasi dinyalakan, seeder mengisi empat layanan dalam keadaan **tidak aktif** dan
   tanpa profesi, lalu memetakan instrumen awal per layanan dan untuk asesmen medis.
2. Admin konfigurasi membuka layanan dan memilih profesi klinis dari data HR.
3. Admin memilih tindakan dari master tindakan yang aktif, lalu memilih jenis instrumen
   asesmen.
4. Admin mengaktifkan layanan. Bila profesi kosong, aktivasi ditolak (422 `RHB-VAL-070`).
5. Layanan yang sudah dipakai permintaan atau episode tidak dapat dihapus (409
   `RHB-VAL-072`); admin cukup menonaktifkannya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Services/RhbOperationResult.cs` | Baru | `RhbResultKind`, `RhbResult<T>` (`Ok`, `Created`, `Invalid`, `Forbidden`, `NotFound`, `Conflict`, `Rule`), mengikuti pola `HmdResult<T>` |
| `Controllers/RhbHttp.cs` | Baru | `ActorId`, pemetaan hasil ke 200/201/400/403/404/409/422, logger kategori `HealthServices.RehabilitationManagement` tanpa kolom sensitif |
| `DTOs/RehabDisciplineDtos.cs` | Baru | Request/response list, detail, create, update, status, option, summary, metadata filter, tindakan, instrumen |
| `Services/RhbConfigurationService.cs` | Baru | Metadata filter, ringkasan, daftar (maks. 100 per halaman; periode `today`/`last7days`/`thisMonth`/`thisYear`), options, detail, create/update/status/delete, tindakan, instrumen |
| `Controllers/RehabDisciplineController.cs` | Baru | 15 endpoint (§4) |
| `Seeders/RehabilitationMasterDataSeeder.cs` | Baru | 4 layanan + peta instrumen + instrumen asesmen medis; idempoten |
| `Constants/RhbErrorCodes.cs` | Diperbarui | **Delta:** kode teknis `RHB-VAL-072`..`076` (§7) |
| `Program.cs` | Diperbarui | `AddScoped<RhbConfigurationService>()`, `RunStartupSeederAsync("RehabilitationMasterDataSeeder", ...)` sesudah seeder Hemodialisa |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru *Health Services / Rehabilitation Management / Rehab Discipline* |
| Database | Tidak ada schema baru. Seeder menulis 4 baris `RhbDiscipline` dan 9 baris `RhbDisciplineInstrument` saat startup pertama, bila `SeedDefaultData:Enabled` tidak dimatikan. |
| Keamanan/Auth | Satu Resource `RehabConfiguration`; Read/Create/Update/Delete diatur admin di layar Akses Role. Tidak ada role yang di-hardcode. |

## 4. Dokumentasi endpoint

**[Tags("Health Services / Rehabilitation Management / Rehab Discipline")]** — base URL
`/api/v1/health-services/rehabilitation-management/rehab-disciplines`

| Method | Path | Kegunaan | Hak akses | Kode status |
| --- | --- | --- | --- | --- |
| GET | `/filters/metadata` | Pilihan filter (status, periode, jenis instrumen 8–13 + `PainScale`) | `RehabConfiguration.Read` | 200 |
| GET | `/summary` | Jumlah total, aktif, tidak aktif | `RehabConfiguration.Read` | 200 |
| GET | `/` | Daftar ber-halaman, cari kode/nama, filter status/periode | `RehabConfiguration.Read` | 200, 400 (`RHB-VAL-076`) |
| GET | `/options` | Layanan aktif untuk dropdown | `RehabConfiguration.Read` | 200 |
| GET | `/{id}` | Detail | `RehabConfiguration.Read` | 200, 404 |
| POST | `/` | Buat layanan | `RehabConfiguration.Create` | 201, 400 (`075`), 409 (`071`), 422 (`070`) |
| PUT | `/{id}` | Ubah nama/profesi/status; kode tidak bisa diubah | `RehabConfiguration.Update` | 200, 404, 422 (`070`) |
| PATCH | `/{id}/status` | Aktif/nonaktif | `RehabConfiguration.Update` | 200, 404, 422 (`070`) |
| DELETE | `/{id}` | Hapus lunak | `RehabConfiguration.Delete` | 200, 404, 409 (`072`) |
| GET | `/{id}/procedures` | Tindakan layanan | `RehabConfiguration.Read` | 200, 404 |
| PUT | `/{id}/procedures` | Simpan daftar tindakan (yang dilepas dinonaktifkan, bukan dihapus) | `RehabConfiguration.Update` | 200, 404, 422 (`073`) |
| GET | `/{id}/instruments` | Jenis instrumen layanan | `RehabConfiguration.Read` | 200, 404 |
| PUT | `/{id}/instruments` | Simpan jenis instrumen | `RehabConfiguration.Update` | 200, 400 (`074`), 404 |
| GET | `/medical-instruments` | Instrumen asesmen medis Sp.KFR | `RehabConfiguration.Read` | 200 |
| PUT | `/medical-instruments` | Simpan instrumen asesmen medis | `RehabConfiguration.Update` | 200, 400 (`074`) |

Contoh penolakan aktivasi tanpa profesi:

```json
{
  "statusCode": 422,
  "message": "Tetapkan profesi klinis untuk layanan ini.",
  "errors": { "Code": "RHB-VAL-070", "Fields": ["professionId"], "Details": null }
}
```

Nama field di `Fields` per kode: `075` → `disciplineCode` atau `disciplineName`; `071` →
`disciplineCode`; `070` → `professionId`; `073` → `procedures`; `074` → `instrumentKinds`.
`072` dan `076` tidak menunjuk field.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE conformance `-Mode Strict` (14 berkas `BE-RHB-004` + `BE-RHB-005`) | 0 violation | `PASS` | Dijalankan agent 2026-11-05 |
| `dotnet build` Debug, perintah owner, dijalankan agent | `0 Error(s)`, 426 warning (seluruhnya di luar folder Rehab), `EXIT=0`, 5 menit 10 detik | `PASS` | Log build 12:25:26–12:30:37; 0 warning dengan path `\RehabilitationManagement\` |
| Review statis kriteria (§6) | Seluruh kriteria punya baris kode yang menegakkannya | `PASS` (statis) | Baris di §6 |
| Hak akses: argumen ke-1 `[AccessPermission]` = `ControllerName`, argumen ke-2 = `[AccessAction]` pada method yang sama | Semua 15 endpoint memakai konstanta `RehabilitationPermissions.Configuration.Resource` + aksi | `PASS` | `RehabDisciplineController.cs` |
| Build ulang sesudah perbaikan `Fields` (perintah owner) | `0 Error(s)`, 426 warning, 0 warning Rehab, `EXIT=0`, 3 menit 56 detik (12:34:35–12:38:32) | `PASS` | Log `build-BE-RHB-005-fields.log` |
| Seeder saat startup pertama (izin owner 2026-11-05) | Log: "menambahkan 4 disiplin dan 9 peta instrumen", 2099 ms | `PASS` | Backend Debug di `https://localhost:7184`, `ASPNETCORE_ENVIRONMENT=Development`. Startup tidak memanggil `Migrate`. |
| Seeder idempoten (startup kedua) | Log: "menambahkan 0 disiplin dan 0 peta instrumen", 215 ms | `PASS` | Backend dinyalakan ulang |
| Tanpa login | `GET /summary` → 401 | `PASS` | curl |
| Uji API manual (SuperAdmin, skrip di scratchpad, kredensial dibaca dari config tanpa dicetak) | 26 pemeriksaan, seluruhnya sesuai status, kode, dan field yang diharapkan. Tabel di bawah. | `PASS` | Lihat catatan casing di bawah tabel |
| Cek DB read-only (`SET TRANSACTION READ ONLY`, ditolak bila DB bukan `QuilvianNewDevSukma`) | Sesuai | `PASS` | Rincian di bawah |

Hasil uji API manual (2026-11-05 14:27–14:28):

| Pemeriksaan | Status | `errors.code` / `fields` |
| --- | --- | --- |
| 8 endpoint baca (`/filters/metadata`, `/summary`, `/`, `/options`, `/{id}`, `/{id}/procedures`, `/{id}/instruments`, `/medical-instruments`) | 200 ×8 | — |
| Ringkasan sesudah seeder | `total 4, aktif 0, tidak aktif 4, tanpa profesi 4` | — |
| `GET /{id}` acak | 404 | — |
| `GET /?startDate=2026-12-01&endDate=2026-11-01` | 400 | `RHB-VAL-076` / `[]` |
| (1) `PATCH /PHYSIOTHERAPY/status` aktif tanpa profesi | 422 | `RHB-VAL-070` / `["professionId"]` |
| (1) `POST` aktif tanpa profesi | 422 | `RHB-VAL-070` / `["professionId"]` |
| (1) `PUT` mengosongkan profesi saat layanan aktif | 422 | `RHB-VAL-070` / `["professionId"]` |
| (2) `POST` kode `PHYSIOTHERAPY` | 409 | `RHB-VAL-071` / `["disciplineCode"]` |
| (2) `POST` memakai ulang kode layanan yang sudah dihapus lunak | 409 | `RHB-VAL-071` / `["disciplineCode"]` |
| `POST` kode `kode salah` | 400 | `RHB-VAL-075` / `["disciplineCode"]` |
| (4) `PUT /procedures` id tindakan yang tidak ada di master | 422 | `RHB-VAL-073` / `["procedures"]` |
| (4) `PUT /procedures` tindakan aktif (`PR-RSMMC-00002`) | 200 | — |
| `PUT /procedures` daftar kosong | 200 — baris tindakan menjadi `IsActive=false`, tidak dihapus | — |
| (5) `PUT /{id}/instruments` `[9, 99]` | 400; instrumen fisioterapi tetap `[3, 9, 10]` | `RHB-VAL-074` / `["instrumentKinds"]` |
| (5) `PUT /medical-instruments` `[99]` | 400 | `RHB-VAL-074` / `["instrumentKinds"]` |
| (5) `PUT /{id}/instruments` `[8, 9]` | 200 | — |
| `POST` layanan sementara `RHB_UJI_1514D85E` aktif + profesi klinis | 201 | — |
| `PATCH` nonaktif | 200 | — |
| (3) `DELETE` layanan belum dipakai | 200 | — |
| `DELETE` ulang | 404 | — |

**Catatan casing.** Di kabel, `errors` dikirim sebagai `{ "code", "fields", "details" }` dalam
camelCase oleh serializer default. `validation-matrix.md` menuliskannya `{ Code, Fields }`, yaitu
nama properti di C#. Helper FE membaca kedua bentuk (`code ?? Code`, `fields ?? Fields`).
Pada putaran pertama, skrip uji membaca PascalCase sehingga mencetak 10 `FAIL` palsu. Body yang
tercetak untuk ke-10 baris itu menunjukkan status, kode, dan field yang benar, dan tabel di atas
disusun dari body tersebut. Skrip tidak dijalankan ulang agar DB dev tidak bertambah baris uji
lagi.

Hasil cek DB:

- Empat baris seed tidak berubah: `IsActive=false`, `ProfessionId=NULL`, `IsDelete=false`. Tetap
  9 peta instrumen seed dan 0 tindakan pada layanan seed.
- `RHB_UJI_1514D85E` (`e86f1d74-2950-4730-8f2c-1cbb3dbcbfdc`): `IsDelete=true`, `IsActive=false`,
  `CreateBy`/`DeleteBy`/`DeleteDateTime` terisi. Tindakannya `IsActive=false, IsDelete=false`.
  Instrumen 8 dan 9 aktif.

**Sisa data uji.** Satu baris layanan terhapus lunak `RHB_UJI_1514D85E` beserta 1 tindakan dan 2
instrumen tertinggal di `QuilvianNewDevSukma`. Baris ini tidak muncul di daftar maupun options.
Kodenya tidak bisa dipakai ulang karena unique index tidak bersyarat.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Aktivasi tanpa profesi klinis → 422 `RHB-VAL-070` | Terpenuhi (runtime) | 3 jalur (`PATCH status`, `POST`, `PUT`) → 422 `RHB-VAL-070`; `RhbConfigurationService` create/update/status |
| (2) Kode ganda → 409 `RHB-VAL-071` | Terpenuhi (runtime) | Kode aktif dan kode baris terhapus → 409 `RHB-VAL-071` |
| (3) `DELETE` ditolak bila sudah dipakai permintaan/episode | Terpenuhi (statis + runtime sebagian) | Runtime: `DELETE` layanan yang belum dipakai → 200, soft delete + audit di DB. Penolakan untuk layanan yang sudah dipakai ada di `IsInUseAsync` (cek `RhbRequestDiscipline`, `RhbEpisodeDiscipline`, `RhbProgram`) → 409 `RHB-VAL-072`. Cabang ini baru bisa diuji setelah ada endpoint permintaan. **Pembaruan 2026-11-05:** diuji di verifikasi `BE-RHB-007`. `DELETE` layanan `RHB_UJI_10722EA2` yang dipakai permintaan dijawab 409 `RHB-VAL-072` ([laporan](BE-RHB-007.md) §5). |
| (4) Tindakan hanya dari `MstProcedure` aktif | Terpenuhi (runtime) | Id tidak dikenal → 422 `RHB-VAL-073`; tindakan aktif → 200 |
| (5) Instrumen hanya dari `ClinicalInstrumentKind` terdaftar | Terpenuhi (runtime) | `99` → 400 `RHB-VAL-074`, isi lama tidak berubah; `8, 9` → 200 |
| DoD: build owner | Terpenuhi | §5 (dua build, 0 error) |
| DoD: laporan | Terpenuhi | Berkas ini |
| Verifikasi kartu: API manual + cek DB | Terpenuhi | §5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perbaikan selama review | Saat laporan ditulis, ditemukan bahwa `RhbHttp` mengirim `errors` sebagai `{ Code, Details }`, padahal `validation-matrix.md` v1 menetapkan `{ Code, Fields }`, dan helper FE `rehabServiceHelper.js` (`FE-RHB-001`) membaca `Fields`. BE disesuaikan dengan kontrak: `RhbResult<T>` mendapat `Fields` + `WithFields(...)`, dan `errors` kini `{ Code, Fields, Details }`. `Details` dipertahankan untuk rincian layar seperti daftar kandidat duplikasi. QBE Strict ulang 3 berkas: 0 violation. Build ulang: lihat §5. |
| Delta kontrak | (a) Kode teknis tambahan di luar validation-matrix v1: `RHB-VAL-072` layanan sudah dipakai, `073` tindakan tidak tersedia, `074` jenis instrumen tidak dikenal, `075` kode/nama tidak sah, `076` rentang halaman tidak sah. (b) **Seeder: keempat layanan diisi tidak aktif**, sedangkan rencana awal menyebut tiga aktif dan O&P tidak aktif. Alasannya: profesi adalah data HR yang dipetakan admin (RHB-DEC-020), dan layanan aktif tanpa profesi melanggar `RHB-VAL-070`. Admin yang mengaktifkan setelah memilih profesi. |
| Peringatan | Log startup memuat dua `ERR` dari scheduler absensi HR (`SystemActorUserId` belum dikonfigurasi). Error ini tidak terkait Rehab. Backend sudah dimatikan kembali sesudah uji. |
| Risiko tersisa | Kriteria (3) jalur "sudah dipakai" baru bisa dibuktikan secara runtime setelah `BE-RHB-007`. Sisa data uji `RHB_UJI_1514D85E` (terhapus lunak) ada di DB dev. |
| Temuan kontrak | `validation-matrix.md` menulis `{ Code, Fields }` dalam PascalCase, sedangkan di kabel bentuknya camelCase. Usulan untuk revisi kontrak berikutnya: tulis `{ code, fields }`. Ini bukan perubahan perilaku. |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks percakapan. Dilanjutkan dari build yang sudah selesai. |
| Status Git | `M Program.cs`, `M`/`??` berkas di `Areas/HealthServices/RehabilitationManagement/` |
| Langkah berikutnya | `BE-RHB-006` (lookup, pemeriksaan encounter, kewenangan profesi) kini 🟡. `FE-RHB-002` masih menunggu `FE-RHB-001` ✅ (lint). |
