# Laporan Perubahan Backend — `BE-RJP-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-005` |
| Judul | Skema reservasi dan migration |
| Slice | `MVP-1`, `EPIC RP-04` (fondasi data) |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-005` |
| Trace | `FR-RP-030` (data); `RJ-PRI-DEC-009`, `012`, `019`, `021`, `024`; `INV-RP-01`, `04`, `05` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved`; `data/data-dictionary.md` RP-2, RP-3, RP-5 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 6: diperiksa 9–20 (1), diubah 10+ (1), logika rendah (0), kontrak data baru (2), keamanan berkaitan (1, hash kode link), workflow terbatas (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | `Areas/HealthServices/RegistrationManagement/{Models,Enums,Services}/`, `Repositories/Configurations/HealthServices/`, `Repositories/ApplicationDbContext.cs`, `Program.cs`, `appsettings.json`, `Migrations/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-16 |
| Status | ✅ Selesai — build, EF, QBE `PASS`; migration diterapkan ke `QuilvianNewDevSukma` atas izin |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `NEW CODE` untuk dua entity, tiga enum, dua configuration, dan options; `TOUCHED LEGACY` untuk `EncounterRegistrationSource`, `ApplicationDbContext`, `Program.cs`, `appsettings.json` |
| QBE yang berlaku | `QBE-ENT-001` (entity mewarisi `IdentityModel`, prefix `Reg`), `QBE-CFG-001` (configuration terpisah, `DeleteBehavior.Restrict`), `QBE-NAM-001`/`002` (tanpa `Trx`), `QBE-CODE-001` (nomor reservasi lewat `NumberSeriesAllocator` di `BE-RJP-006`, bukan Count/Max+1), `QBE-DB-001` (migration hanya berisi entity ini) |
| Catatan role access | Task ini tidak membuat endpoint. Izin `PriorityReservation` dibuat di `BE-RJP-006` |

---

## 1. Masalah yang diselesaikan

Reservasi pasien prioritas yang dibuat lewat WA atau telepon belum punya tempat penyimpanan. Reservasi ini sengaja
**bukan** kunjungan (`RJ-PRI-DEC-009`): poli, dokter, dan nomor antrean baru ditentukan saat admisi melengkapinya.
Karena itu dibutuhkan tabel sendiri beserta riwayat perubahan statusnya.

## 2. Proses bisnis yang didukung data ini

1. Petugas membuat reservasi untuk pasien member Aktif pada tanggal tertentu. Sistem menyimpan nomor reservasi, status
   `Menunggu Kedatangan`, hash kode link konfirmasi, dan salinan (snapshot) data member saat itu.
2. Pasien atau petugas menandai kedatangan. Sistem menyimpan waktu, sumber, dan petugasnya.
3. Admisi mengisi checklist skrining awal (batuk, demam, risiko jatuh).
4. Admisi melengkapi reservasi menjadi kunjungan. Reservasi menyimpan `EncounterId` hasilnya.
5. Reservasi yang tidak dilanjutkan ditutup: dibatalkan dengan alasan, ditutup otomatis sebagai tidak datang, atau
   ditandai sudah terdaftar lewat jalur lain.
6. Setiap perubahan status dicatat satu baris di tabel riwayat.

Contoh: reservasi `RSV-261016-0001` untuk pasien Gold dibuat pukul 08.00. Pasien membuka link pukul 09.10, status
menjadi `Sudah Tiba`. Admisi melengkapinya ke Poli Penyakit Dalam pukul 09.20, status menjadi `Sudah Jadi Kunjungan`.
Tabel riwayat memuat tiga baris: `CREATED`, `ARRIVED`, `CONVERTED`.

Satu pasien hanya boleh punya satu reservasi terbuka (`Menunggu Kedatangan` atau `Sudah Tiba`) per tanggal
(`INV-RP-01`). Aturan ini dijaga index unik bersyarat di database, bukan hanya di kode.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`data/data-dictionary.md` RP-2/RP-3/RP-5, `02-backend-architecture.md` RP.3 dan RP.11, `Models/IdentityModel.cs`,
model dan configuration `crm-membership` (pola `RowVersion`), `RegPatientEncounter`, `MstPatientMembership`,
`ApplicationDbContext`, `Program.cs`, daftar migration terbaru.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Models/RegPriorityReservation.cs` | Baru | Seluruh kolom RP-2, `RowVersion`, navigasi `Patient`, `PatientMembershipSnapshot`, `Encounter`, `Histories` |
| `Areas/HealthServices/RegistrationManagement/Models/RegPriorityReservationHistory.cs` | Baru | Seluruh kolom RP-3 dan konstanta kode kejadian (`CREATED`, `ARRIVED`, `ARRIVAL_REVERTED`, `SCREENED`, `CODE_REISSUED`, `CONVERTED`, `CANCELLED`, `NO_SHOW`, `REGISTERED_ELSEWHERE`) |
| `Areas/HealthServices/RegistrationManagement/Enums/PriorityReservationStatus.cs` | Baru | `AwaitingArrival = 1`, `Arrived = 2`, `Converted = 3`, `Cancelled = 4`, `NoShow = 5`, `RegisteredElsewhere = 6`, beserta nama tampilan |
| `Areas/HealthServices/RegistrationManagement/Enums/PriorityReservationArrivalSource.cs` | Baru | `PatientLink = 1`, `Staff = 2` |
| `Areas/HealthServices/RegistrationManagement/Enums/PriorityReservationEventSource.cs` | Baru | `Staff = 1`, `PatientLink = 2`, `System = 3` |
| `Areas/HealthServices/RegistrationManagement/Enums/EncounterRegistrationSource.cs` | Diperbarui | `PriorityReservation = 6` |
| `Repositories/Configurations/HealthServices/RegPriorityReservationConfiguration.cs` | Baru | Tabel `public."RegPriorityReservation"`, panjang kolom, bawaan, enam index, tiga FK `Restrict` |
| `Repositories/Configurations/HealthServices/RegPriorityReservationHistoryConfiguration.cs` | Baru | Tabel `public."RegPriorityReservationHistory"`, index `(PriorityReservationId, OccurredAt)`, FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | Diperbarui | `DbSet` `RegPriorityReservations` dan `RegPriorityReservationHistories` |
| `Areas/HealthServices/RegistrationManagement/Services/PriorityReservationOptions.cs` | Baru | Section `HealthServices:Registration:PriorityReservation`: `Enabled`, `MaxDaysAhead` (30), `PriorityWaitingRoomName`, `PublicArrivalBaseUrl`, `ArrivalLinkPermitPerMinute` (20), `Closure.Enabled`, `Closure.RunAtLocalTime` ("00:05") |
| `Program.cs` | Diperbarui | `Configure<PriorityReservationOptions>` |
| `appsettings.json` | Diperbarui | Kunci `PriorityReservation` dengan nilai bawaan di atas |
| `Migrations/20261016144037_AddOutpatientPriorityReservation.cs` + `.Designer.cs` | Baru | Dua tabel, index, FK. `Down` menghapus kedua tabel |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui | +269 baris, hanya untuk dua entity ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Belum ada endpoint. Endpoint dibuat mulai `BE-RJP-006` |
| Database | Dua tabel baru. Tidak ada kolom tabel lama yang diubah. Dapat diterapkan tanpa mematikan layanan. Langkah mundur: `dotnet ef database update 20261016110000_AddRadOrderPhotoNumber` (menghapus kedua tabel; data reservasi ikut hilang) |
| Keamanan | Kode link konfirmasi tidak pernah disimpan. Yang disimpan hanya `ArrivalCodeHash` (SHA-256 hex, unik). Kolom ini bertanda **Sensitif** dan tidak boleh masuk log |
| Konfigurasi | Nilai bawaan aman untuk dev. `PublicArrivalBaseUrl` wajib disetel per lingkungan sebelum `BE-RJP-012` dipakai |

## 4. Skema yang terbentuk

Diambil dari `dotnet ef migrations script 20261016110000_AddRadOrderPhotoNumber 20261016144037_AddOutpatientPriorityReservation`.

| Index | Kolom | Syarat |
| --- | --- | --- |
| `IX_RegPriorityReservation_ReservationNumber` | `ReservationNumber` | Unique |
| `IX_RegPriorityReservation_ArrivalCodeHash` | `ArrivalCodeHash` | Unique |
| `IX_RegPriorityReservation_EncounterId` | `EncounterId` | Unique, `"EncounterId" IS NOT NULL` |
| `UX_RegPriorityReservation_Open_Patient_VisitDate` | `(PatientId, VisitDate)` | Unique, `"Status" IN (1, 2) AND NOT "IsDelete"` (`INV-RP-01`) |
| `IX_RegPriorityReservation_VisitDate_Status` | `(VisitDate, Status)` | — |
| `IX_RegPriorityReservation_PatientMembershipIdSnapshot` | `PatientMembershipIdSnapshot` | — |
| `IX_RegPriorityReservationHistory_PriorityReservationId_OccurredAt` | `(PriorityReservationId, OccurredAt)` | — |

FK: `PatientId → MstPatient`, `PatientMembershipIdSnapshot → MstPatientMembership`,
`EncounterId → RegPatientEncounter`, `PriorityReservationId → RegPriorityReservation`. Semuanya `ON DELETE RESTRICT`.

Bawaan kolom: `Status = 1`, `IsPrioritySnapshot = false`, `QueuePriorityLevelSnapshot = 0`,
`QueueAudienceSnapshot = 1`, `PublicDisplayModeSnapshot = 1`. `VisitDate` bertipe `date`; kolom waktu bertipe
`timestamp with time zone`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:UseSharedCompilation=false -o <scratchpad>/be-build` | 0 error, 287 warning. Tidak ada warning dari berkas task ini | `PASS` | Log build |
| `dotnet ef migrations has-pending-model-changes --configuration Release` | "No changes have been made to the model since the last migration." exit 0 | `PASS` | Keluaran perintah |
| QBE Strict, 10 berkas task | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Keluaran `Invoke-QbeConformanceCheck.ps1` |
| Kolom script migration dibandingkan kamus data RP-2/RP-3 (skrip pembanding) | Semua kolom kamus data ada dengan tipe dan nullability sama. Kolom tambahan hanya kolom warisan `IdentityModel` | `PASS` | Skrip pembanding di scratchpad |
| Enam index pada reservasi + satu pada riwayat, FK `Restrict` | Sesuai bagian 4 | `PASS` | Script migration |
| `Down` | Menghapus `RegPriorityReservationHistory` lalu `RegPriorityReservation` | `PASS` | Berkas migration |
| Penerapan ke `QuilvianNewDevSukma` | Diterapkan bersama tiga migration Radiologi yang masih pending (`20261016090000`, `100000`, `110000`) atas izin user. `__EFMigrationsHistory` teratas: `20261016144037_AddOutpatientPriorityReservation` | `PASS` | Kueri riwayat migration |

Uji runtime HTTP tidak relevan karena belum ada endpoint. Backend uji 7185 berjalan normal dengan schema baru selama
uji `FE-RJP-001`/`002`.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kolom, tipe, bawaan, enam index, FK `Restrict` persis kamus data | Terpenuhi | Bagian 4 dan 5 |
| (2) `Down` menghapus kedua tabel | Terpenuhi | Berkas migration |
| (3) `has-pending-model-changes` bersih | Terpenuhi | Keluaran perintah |
| (4) Build lulus | Terpenuhi | Log build |
| Penerapan ke DB hanya dengan izin | Terpenuhi | Izin `RJ-PRI-DEC-031` dan persetujuan menerapkan empat migration |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga migration Radiologi milik modul lain ikut diterapkan ke DB dev karena urutannya lebih awal. Ini disetujui user. Lingkungan lain wajib menerapkan keempatnya berurutan |
| Masalah yang diketahui | — |
| Risiko tersisa | Bila ada migration modul lain dengan timestamp lebih awal yang di-merge kemudian, urutan penerapan di lingkungan lain perlu dicek |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi berpindah konteks sesudah migration diterapkan. Dilanjutkan dari status Git dan riwayat migration DB; tidak ada langkah ganda |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `BE-RJP-006` (buat reservasi dan kueri) |

---

## 8. Rework 21 Oktober 2026 — alamat link konfirmasi per lingkungan

### 8.1 Metadata rework

| Field | Nilai |
| --- | --- |
| Pemicu | Link konfirmasi kedatangan yang terbit di server berisi `http://localhost:3000/priority-arrival/...`, sehingga tidak bisa dibuka pasien |
| Trace | `02-backend-architecture.md` RP.11 (`PublicArrivalBaseUrl`), `GAP-RP-01`, `RJ-PRI-OQ-003` |
| Klasifikasi | `LIGHT` — skor 0: satu repository, diperiksa ≤ 8 berkas inti, diubah 2 berkas konfigurasi, tanpa logika, API, database, maupun auth |
| Task mode | `BACKEND`, atas permintaan user (sukmagp) pada 2026-10-21 |
| Target tulis | `appsettings.json`, `appsettings.Development.json`, laporan ini, roadmap dan traceability Reservasi Prioritas |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `c594fabd` (`sukmagp`, upstream `origin/sukmagp`), working tree bersih sebelum task |
| Status | ✅ Selesai untuk repo. Nilai staging dan production tetap disetel di server (lihat 8.3) |

| Backend Governance Preflight | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — hanya nilai konfigurasi, tanpa kode C# |
| QBE yang berlaku | `NOT APPLICABLE` — tidak ada entity, configuration EF, penamaan, kode nomor, atau migration yang disentuh |
| Catatan role access | `NOT APPLICABLE` — tidak ada endpoint yang dibuat atau disentuh |
| Selisih governance | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` di repo berbeda isi dengan salinan di suite skill 1.18.0. Sesuai `AGENTS.md`, salinan repo yang dipakai. Entri `RegistrationManagement` tidak terdampak |

### 8.2 Masalah dan proses bisnis

Link konfirmasi dibentuk backend dari `PublicArrivalBaseUrl` + `/` + kode. Nilai bawaan di `appsettings.json`
masih alamat laptop pengembang. Image Docker yang sama dipromosikan dari dev ke staging lalu production, dan
`appsettings.Development.json` dikecualikan oleh `.gitignore` dan `.dockerignore`. Akibatnya setiap server yang
tidak menimpa kunci ini menerbitkan link `localhost`.

Contoh: admisi membuat reservasi `RSV-20261021-0001` di server dev. Pasien menerima
`http://localhost:3000/priority-arrival/<kode>` lewat WhatsApp. Di HP pasien, alamat itu menunjuk ke HP itu sendiri,
jadi halaman tidak terbuka dan pasien tidak bisa menekan "Saya sudah tiba".

Alur sesudah perbaikan:

1. Admisi membuat reservasi atau menekan **Kirim Ulang Link**.
2. Backend membaca `PublicArrivalBaseUrl` dari konfigurasi lingkungan tempat ia berjalan.
3. Link dikirim ke pasien dengan domain frontend lingkungan itu, misalnya
   `https://dev.quilvian-mmchospital.com/priority-arrival/<kode>`.
4. Jalur tidak normal: link yang terbit **sebelum** perubahan tetap berisi `localhost`. Admisi menerbitkan ulang
   lewat **Kirim Ulang Link**; kode lama tidak berlaku lagi sesudah diterbitkan ulang.

### 8.3 Perubahan dan nilai per lingkungan

| Berkas | Perubahan |
| --- | --- |
| `appsettings.json` | `HealthServices:Registration:PriorityReservation:PublicArrivalBaseUrl` diubah dari `http://localhost:3000/priority-arrival` menjadi `https://dev.quilvian-mmchospital.com/priority-arrival`. Mengikuti pola `FileStorage:PublicBaseUrl` yang sudah menunjuk server dev |
| `appsettings.Development.json` (tidak dilacak Git) | Menambah section `HealthServices:Registration:PriorityReservation` yang hanya berisi `PublicArrivalBaseUrl = http://localhost:3000/priority-arrival`, agar uji lokal tetap memakai frontend lokal. Kunci lain tetap diwarisi dari `appsettings.json` |

`PriorityReservationOptions.cs` tidak diubah. Nilai bawaan di kelas itu hanya berlaku bila kunci hilang dari
konfigurasi, dan `appsettings.json` selalu memuat kunci ini.

Nilai yang wajib disetel di server lewat environment variable
`HealthServices__Registration__PriorityReservation__PublicArrivalBaseUrl`:

| Lingkungan | Nilai | Sumber |
| --- | --- | --- |
| Lokal (`Development`) | `http://localhost:3000/priority-arrival` | `appsettings.Development.json` |
| Server dev | `https://dev.quilvian-mmchospital.com/priority-arrival` | Bawaan `appsettings.json`; environment variable tidak wajib |
| Staging | `https://sta.quilvian-mmchospital.com/priority-arrival` | Environment variable di server |
| Production | `https://quilvian-mmchospital.com/priority-arrival` | Environment variable di server; nilai sesuai RP.11 |

Skrip deploy (`/opt/QuilvianDeveloper/deploy.sh`, `/opt/QuilvianProductionV2/deploy-bluegreen.sh`) ada di server,
bukan di repo, sehingga tidak diubah task ini.

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — bentuk respons tetap; hanya domain di field link yang berubah |
| Database | `NOT APPLICABLE` — tidak ada schema, entity, atau migration |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi. Kode link tetap tidak tercatat di konfigurasi maupun log |

### 8.4 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Parse JSON kedua berkas (`node`) | Keduanya valid | `PASS` | Keluaran perintah |
| Nilai efektif, image server (hanya `appsettings.json`) | `https://dev.quilvian-mmchospital.com/priority-arrival` | `PASS` | Keluaran perintah |
| Nilai efektif, lokal `Development` (override) | `http://localhost:3000/priority-arrival`; override hanya berisi satu kunci, enam kunci lain tetap dari `appsettings.json` | `PASS` | Keluaran perintah |
| `dotnet build` ke `bin\Debug` | Kompilasi selesai, tetapi salin `QuilvianSystemBackend.exe` gagal (`MSB3027`/`MSB3021`) karena backend lokal sedang berjalan (PID 18356) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `dotnet build -o <scratchpad>/build-out` | 0 error, 10 warning (build inkremental). `appsettings.json` hasil build memuat alamat server dev | `PASS` | Keluaran perintah |

Uji manual: `REQUIRED` — sesudah deploy, terbitkan satu link di server dev lalu buka dari HP di luar jaringan
kantor.

**Tidak dijalankan:** uji runtime HTTP pembuatan reservasi (perubahan hanya nilai konfigurasi; backend lokal
user sedang berjalan dan tidak dimulai ulang), QBE Strict (tidak ada berkas C#), dan `dotnet test` — `NOT RUN`.

### 8.5 Catatan penutup rework

| Hal | Isi |
| --- | --- |
| Peringatan | Backend lokal yang sedang berjalan perlu dimulai ulang agar override `appsettings.Development.json` terbaca. Nilainya sama dengan sebelumnya, jadi perilaku lokal tidak berubah |
| Masalah yang diketahui | Domain frontend production diambil dari RP.11 dan daftar CORS. Belum dicek langsung terhadap server |
| Risiko tersisa | Bila environment variable lupa disetel di staging atau production, link menunjuk ke server dev. Pasien bisa membuka halaman, tetapi kode tidak dikenal di database lingkungan itu sehingga muncul 404. `GAP-RP-01` (aksesibilitas internet dan IP asli) tetap terbuka |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M appsettings.json`, ` M` laporan ini, roadmap, dan traceability. `appsettings.Development.json` tidak dilacak Git. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | Setel environment variable di server staging dan production, buat ulang container, lalu uji manual link dari HP |
