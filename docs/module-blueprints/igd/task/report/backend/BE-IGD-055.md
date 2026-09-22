# Laporan Perubahan Backend — `BE-IGD-055`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-055` |
| Judul | Kunjungan IGD lahir lewat Mulai Triage atau Tangani Segera (`POST start-triage`) |
| Slice | `S3` (dan `S8`) · `EPIC IGD-11` · `MVP-7` |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian R3.13 |
| Trace | `FR-IGD-075`, `FR-IGD-076`, `FR-IGD-077`, `FR-IGD-085`; `AT-IGD-172`, `173`, `174`, `184`; `IGD-DEC-139` butir 3, `143`, `146`, `147`, `151`, `158`, `160`, `161`; `IGD-DEC-157` (kontrak disetujui); `IGD-OQ-108` (terbuka, tidak menahan) |
| Contract version | API **`0.11.0`** §8.3.2, §8.3.4 (tiga ruas waktu tiba); validation **`0.8.0`** §10.2 (dan §1 aturan 3–4 yang dirujuknya); state **`0.5.0`** §8.1, §8.3; integration **`0.4.0`** §5.3 baris `start-triage`; permission/audit **`0.5.0`** §7.1 baris `Create`. Seluruh bagian encounter-first **`approved`** (`IGD-DEC-157`, 22 September 2026), terkunci hash. Task ini **tidak** mengubah berkas kontrak |
| Dependency | `BE-IGD-025` ✅, `BE-IGD-051` ✅ (atas penilaian pemilik, 22 September 2026) |
| Klasifikasi | `HEAVY` — skor 11: cakupan repository 0, berkas diperiksa 2 (> 20 source + dokumen), berkas diubah 1 (8 source), logika bisnis 2 (idempotensi, kunci, tabrakan dua mode), kontrak API 2 (endpoint baru + tiga ruas response), database 2 (tiga kolom + FK, migration), keamanan/auth 1 (izin `Create` dipakai bersama, `IGD-DEC-158`), UI/workflow 1 |
| Task mode | `BACKEND` — go-ahead pemilik 22 September 2026 lewat `build-module-backend`. Target tulis: source IGD (`EmergencyInstallationManagement`, konfigurasi EF `EmgVisit`) dan `docs/module-blueprints/igd/` (laporan, roadmap, traceability). **Tidak** ada wewenang `dotnet build`, membuat/menjalankan migration, menulis basis data, commit, push, merge, pindah branch; **tidak** menyentuh `Program.cs`, berkas Registrasi, berkas kontrak, frontend, dan `DataProtectionKeys/key-*.xml` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `48a78703` pada branch `rizkiG`, sejajar `origin/rizkiG` (commit pemilik yang memuat `BE-IGD-051`). **Perubahan task ini belum di-commit.** Baseline task = `48a78703`; satu-satunya perubahan terbuka sebelum task dimulai adalah satu baris `BE-IGD-051.md` dari sesi lalu, yang tidak disentuh |
| Tanggal | 22 September 2026 |
| Status | 🟡 **SEBAGIAN — 22 September 2026.** Implementation Complete: kesepuluh kriteria terpetakan ke source. Terbukti oleh agent: sebagian kriteria 6 (pembacaan source: nol jalur yang mengisi waktu tiba dari jam klien untuk `ImmediateCare`) dan kriteria 8 dari sisi source (hanya `POST start-triage` yang membuat kunjungan). **Belum:** migration `AddEmergencyArrivalTimeSource` (kriteria 9, milik pemilik), `dotnet build` (kriteria 10, milik pemilik — `NOT RUN`), uji API dan uji paralel (kriteria 1–8). **Aplikasi tidak boleh dijalankan terhadap basis data sebelum migration diterapkan** — bagian 7 |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` dan `CLAUDE.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (repo); `rules/backend/` suite skill 1.17.1 (`TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE`, `API_RULES`, `DATABASE_RULES`, `CROSS_REPO_RULES`, `backend-project-profile`); `rules/rule-output/` (`status-task-roadmap`, `lokasi-laporan-task`) |
| Selisih governance | Registry di repo **berbeda** dari salinan suite (selisih lama `ACC-DEP-007`). Yang berlaku versi repo (`AGENTS.md`). Baris `Emg` ada dan identik di keduanya. Kontrak engineering repo dan suite **identik** (`diff -q`). Nol folder `agents/rules/` atau `.codex/` peninggalan |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, prefix `Emg`, `ACTIVE / LEGACY` — nol entity baru, jadi nol blocker `QBE-MOD-002` |
| Keberlakuan | `NEW CODE`: `StartVisitAsync` beserta helper-nya, `FindOpenEpisodeAsync`, `LockPatientEpisodeAsync`, action `StartTriage`, dua enum baru, `StartEmergencyVisitRequest`. `TOUCHED LEGACY`: `EmgVisit` (+3 kolom), `EmgVisitConfiguration`, `EmergencyVisitResponse`, `EmergencyVisitController` (controller lama memakai `DbContext` langsung — **tidak** di-refactor; action baru **tidak** menyentuh `DbContext`) |
| QBE ID yang berlaku | `QBE-SVC-001` (orkestrasi di `EmergencyVisitService`; `StartTriage` hanya memanggil service), `QBE-API-001` (route `api/v1/…`, envelope `ApiResponse<T>`, kode status kontrak), `QBE-PERM-001` (pasangan `[AccessAction("Create", …)]` + `[AccessPermission("EmergencyVisit", "Create")]` pada method yang sama), `QBE-VAL-001` (aturan validation §10.2 dan invarian satu kunjungan per encounter), `QBE-TXN-001` (transaksi eksplisit + kunci per pasien), `QBE-LOG-001` (event `EmergencyVisit.StartTriage`; aktor diambil `LoggerService` dari klaim token), `QBE-DTO-001` (entity tidak terekspos), `QBE-ENT-002` (tiga kolom baru bertipe dan bernullability sesuai semantik), `QBE-ENT-003` (tidak ada kolom murni presentasi — nama pelaku diproyeksikan, bukan disimpan), `QBE-CFG-001`/`CFG-002` (konversi, bawaan, dan FK di `EmgVisitConfiguration`), `QBE-ENUM-001` (enum milik modul IGD), `QBE-AUD-001` (jejak konfirmasi di kolom data, terpisah dari log aplikasi) |
| Tidak berlaku | `QBE-NAM-*`, `QBE-MOD-002/003` (nol entity/tabel baru); `QBE-CODE-*` (nomor kunjungan memakai `GenerateVisitNumberAsync` yang sudah ada, tidak diubah); `QBE-DB-*` (bukan `LEGACY MIGRATION`); `QBE-PAGE-001`, `QBE-OPT-001` (nol endpoint daftar); `QBE-DEL-001` (nol jalur hapus baru) |
| Branch | `rizkiG` — branch backend Rizki, sesuai penetapan pemilik; upstream `origin/rizkiG`, sejajar (`48a78703`). Tidak di-sinkronkan, tidak di-commit |
| Hardcode role | Tidak ditemukan pada berkas yang disentuh. Tidak ada `IsInRole`, nama peran, departemen, atau `UserType` sebagai penentu kewenangan |
| Endpoint standard | Transaksi, arketipe **aggregate ber-lifecycle**: kelahiran kunjungan adalah aksi bernama `POST /start-triage`, bukan `POST /` generik dan bukan `PATCH /status` |

---

## 1. Masalah yang diperbaiki

Hari ini kunjungan IGD lahir di **loket pendaftaran**, pada saat yang sama dengan encounter. Petugas loket
mengisi "waktu tiba" dengan jam saat itu, dan status kunjungan langsung "Menunggu Triage". Keputusan
encounter-first (`IGD-DEC-139`, `143`, `147`) memindahkan titik lahir itu ke **perawat triage**:

1. Loket hanya membuat encounter. Pasien muncul di daftar *Menunggu Triage* tanpa kunjungan IGD.
2. Perawat menekan **Mulai Triage** (sambil memastikan waktu tiba) atau **Tangani Segera** (pasien gawat,
   tanpa ketikan apa pun). Saat itulah kunjungan IGD lahir.

Sebelum task ini, backend tidak punya pintu untuk langkah kedua. Task ini membuat pintunya, lengkap dengan
penjaga supaya satu encounter tidak pernah melahirkan dua kunjungan — walaupun tombolnya ditekan dua kali
atau oleh dua perawat pada detik yang sama.

*Contoh.* Pasien Budi didaftarkan loket pukul 09.35 (encounter `ENC-RSMMC-00123`). Perawat Ani melihatnya
di daftar, menanyakan kapan Budi tiba, mengubah isian 09.35 menjadi 09.25, lalu menekan **Mulai Triage**.
Backend melahirkan kunjungan `IGD-…` berstatus *Menunggu Triage*, waktu tiba 09.25 **dikonfirmasi** atas
nama Ani. Dua detik kemudian perawat Citra menekan **Tangani Segera** untuk Budi yang sama: tidak ada
kunjungan kedua; kunjungan Ani diteruskan ke *Dalam Penanganan*.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Kunjungan IGD lahir tepat satu kali, pada saat perawat triage mulai bekerja, dengan waktu tiba yang jelas asal-usulnya |
| Pelaku | Perawat triage (hak akses `EmergencyVisit : Create`). Keterbatasan yang diterima `IGD-DEC-158`: petugas loket yang memegang izin yang sama secara teknis dapat memanggilnya lewat API; pelakunya tetap terekam |
| Pemicu | Tombol **Mulai Triage** atau **Tangani Segera** pada baris encounter di daftar *Menunggu Triage* (layar dibuat `FE-IGD-036`) |
| Masukan | `encounterId`, `mode`; untuk Mulai Triage juga `arrivalDateTime`; opsional `arrivalModeId`, `caseTypeId`, `chiefComplaint`, `isUnknownPatient` + `temporaryPatientAlias` |
| Keluaran | Kunjungan IGD (`EmergencyVisitResponse`) beserta tiga ruas waktu tiba |

**Langkah berurutan di backend.**

1. **Periksa permintaan** (tanpa menyentuh basis data): `encounterId` ada, `mode` bernilai `Triage` atau
   `ImmediateCare`; untuk `Triage` waktu tiba wajib dan tidak melewati jam server; pasien tanpa identitas
   wajib punya nama sementara; cara datang dan jenis kasus (bila diisi) harus ada di master.
2. **Buka transaksi**, baca encounter. Tidak ada → `404`. Bukan `Emergency` → `400`.
3. **Ambil kunci per pasien** `EMG_EPISODE_{patientId}` (`pg_advisory_xact_lock`). Permintaan lain untuk
   pasien yang sama **menunggu** di sini sampai transaksi pertama selesai, lalu membaca hasilnya.
4. **Baca ulang encounter di dalam kunci.** Sudah berakhir (lima tanda `IsEncounterEnded`) → `409`.
   Pembacaan ulang ini menutup celah "NoShow dan Mulai Triage serentak".
5. **Cari kunjungan milik encounter ini**, termasuk yang pernah dihapus lunak:
   - pernah dihapus lunak (K4) → `409`;
   - ada dan masih aktif → **jawaban idempoten `200`**: kunjungan yang sama. Hanya satu kasus yang
     mengubah sesuatu: `Tangani Segera` pada kunjungan *Tiba* atau *Menunggu Triage* diteruskan ke
     *Dalam Penanganan* lewat penjaga transisi yang sudah ada. Status **tidak pernah mundur**.
6. **Tidak ada kunjungan** → periksa apakah pasien masih punya **kunjungan lain** yang berjalan pada
   encounter berbeda (klausa B). Ada → `409` dengan pesan validation §1 aturan 4. Encounter lain tanpa
   kunjungan (klausa A) **tidak** menolak (`IGD-OQ-108`).
7. **Lahirkan kunjungan**: nomor dari `GenerateVisitNumberAsync`, pasien dan unit layanan **dari encounter**
   (bukan dari body), keluhan utama bawaan = salinan keluhan encounter.
8. **Simpan dan commit.** Bila penyimpanan bentrok unique index `EmgVisit.EncounterId` (jalur lama
   `POST /emergency-visits` yang belum memakai kunci), transaksi dibatalkan dan langkah 2–8 diulang
   **sekali**; percobaan kedua menemukan kunjungan yang sudah ada dan menjawab sesuai langkah 5.

**Dua mode kelahiran.**

| Mode | Status awal | Waktu tiba | Sumber (`arrivalTimeSource`) | Pelaku & waktu konfirmasi |
| --- | --- | --- | --- | --- |
| `Triage` (Mulai Triage) | `WaitingForTriage` (2) | Nilai kiriman perawat | `Confirmed` (2) | Pengguna token, jam server |
| `ImmediateCare` (Tangani Segera) | `InTreatment` (4), `TreatmentStartedAt` = jam server | `RegisteredAt` encounter — nilai kiriman **diabaikan** | `Fallback` (1) | Kosong |

**Contoh berangka — tabrakan dua mode (`IGD-DEC-143`).** Encounter E belum punya kunjungan.

| Urutan tiba di kunci | Permintaan pertama | Permintaan kedua | Hasil akhir |
| --- | --- | --- | --- |
| Ani (Triage) lebih dulu | `201`, `WaitingForTriage` | Citra (ImmediateCare) → `200`, diteruskan ke `InTreatment` | Satu kunjungan, `InTreatment`, waktu tiba **tetap** konfirmasi Ani |
| Citra (ImmediateCare) lebih dulu | `201`, `InTreatment`, tiba = `RegisteredAt` (`Fallback`) | Ani (Triage) → `200`, status tidak diubah | Satu kunjungan, `InTreatment`; waktu tiba dikoreksi kelak lewat `PATCH /{id}/arrival-time` (`BE-IGD-058`) |

Pada kedua urutan Tangani Segera **menang**, dan hanya ada satu kunjungan.

**Jalur tidak normal.** Encounter hilang (`404`), bukan gawat darurat (`400`), sudah berakhir (`409`),
kunjungannya pernah dihapus (`409` — daftarkan ulang pasien), pasien masih di kunjungan IGD lain (`409` —
buka kunjungan itu), permintaan tidak lengkap (`400`). Membuka detail atau membaca daftar **tidak pernah**
melahirkan kunjungan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Governance | `AGENTS.md`, `CLAUDE.md`, `docs/engineering/*`, `rules/backend/*`, `rules/rule-output/status-task-roadmap.md`, `lokasi-laporan-task.md` |
| Kontrak | `contracts/api-contract.md` §8, `validation-matrix.md` §1, §10.1–10.2, `state-transition-matrix.md` §8, `integration-contract.md` §5, `permission-audit-matrix.md` §7; `02-backend-architecture.md` §13 |
| Source IGD | `EmergencyVisitController.cs`, `EmergencyVisitService.cs`, `EmergencyEpisodeRule.cs`, `EmergencyVisitDtos.cs`, `EmgVisit.cs`, `EmgVisitConfiguration.cs`, `EmergencyDepartureService.cs` (pola `Hasil<T>`), `EmergencyDoctorAssignmentService.cs` dan `EmergencyObservationDetailController.cs` (pola nama pelaku), enum IGD |
| Source lain (baca saja) | `RegPatientEncounter.cs`, `PatientEncounterNumberService.cs` (pola kunci), `ApplicationUser.cs`, `ApiResponse.cs`, `LoggerService.cs`, `AccessMenuSeeder.cs`, `BbkProviderRequestService.cs` (pola `UniqueViolation`), `AttendanceProcessingService.cs` (pola `NormalizeUtc`), `BillingFinalizationsController.cs` (pola 201/200), `Program.cs` (baca saja: tanpa konverter enum string, tanpa retry strategy), snapshot EF (tipe kolom) |
| Frontend (baca saja) | `emergency-registration.utils.js` (nilai bawaan kunjungan dari layar hari ini) |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyArrivalTimeSource.cs` | **Baru.** `Unverified = 0`, `Fallback = 1`, `Confirmed = 2` (state §8.3) |
| `…/Enums/EmergencyVisitStartMode.cs` | **Baru.** `Triage = 1`, `ImmediateCare = 2` — hanya untuk request |
| `…/Models/EmgVisit.cs` | + `ArrivalTimeSource` (bawaan `Unverified`), `ArrivalConfirmedByUserId`, `ArrivalConfirmedAt`, navigasi `ArrivalConfirmedByUser` |
| `Repositories/Configurations/…/EmgVisitConfiguration.cs` | `ArrivalTimeSource` `HasConversion<int>()` + `HasDefaultValue(Unverified)`; FK `ArrivalConfirmedByUserId` → `AspNetUsers`, `Restrict` |
| `…/DTOs/EmergencyVisitDtos.cs` | + `StartEmergencyVisitRequest`; `EmergencyVisitResponse` + `ArrivalTimeSource`, `ArrivalConfirmedByName`, `ArrivalConfirmedAt` |
| `…/Services/EmergencyEpisodeRule.cs` | + `EncounterNotEnded` (negasi rumus yang sama, bukan salinan), `OpenEpisodeKind`, `OpenEpisode`, `FindOpenEpisodeAsync` (klausa A+B, mendahulukan kunjungan), `LockPatientEpisodeAsync` (menolak bila di luar transaksi; tanpa efek di luar PostgreSQL) |
| `…/Services/EmergencyVisitService.cs` | + `Hasil<T>`, `StartVisitAsync`, `PesanKunjunganLainBerjalan`, dan helper privat (`MulaiKunjunganDalamKunciAsync`, `ParseStartMode`, `IsUniqueViolation`, `FormatWaktuLokal`, `NormalizeUtc`, …). Method lama tidak diubah |
| `…/Controllers/EmergencyVisitController.cs` | + action `StartTriage`; `ToResponse` membawa tiga ruas waktu tiba; `GET /` dan `GET /{id}` memuat `ArrivalConfirmedByUser` supaya nama pengonfirmasi terbaca |

Nol perubahan pada `Program.cs`, `Migrations/`, `ApplicationDbContext.cs`, berkas Registrasi, berkas kontrak,
dan frontend. Kode baru ditulis **tanpa baris komentar**, atas permintaan pemilik di tengah task.

**Selisih terhadap kartu dan kontrak** — dicatat supaya pemilik dapat menolaknya satu per satu:

| No | Selisih | Alasan |
| ---: | --- | --- |
| 1 | Kunjungan baru diisi `RegistrationStatus = Registered`, `RegistrationCompletedAt = RegisteredAt` encounter, `RegistrationCompletedByUserId = null`, `IsImmediateCareAllowed = true` | Kontrak tidak menetapkannya. Nilainya mengikuti bawaan layar pendaftaran hari ini (`emergency-registration.utils.js` — `REGISTERED`, `isImmediateCareAllowed` bawaan `true`). Pelaku pendaftaran dikosongkan karena perawat bukan pendaftar; pendaftar sebenarnya tetap tercatat pada encounter |
| 2 | `arrivalModeId`/`caseTypeId` yang tidak ada di master → `400` (*"ArrivalModeId tidak ditemukan."*, *"CaseTypeId tidak ditemukan."*) | Validasi teknis dengan pesan jalur lama; tanpa ini nilai salah berakhir sebagai galat foreign key |
| 3 | Pengaturan `AllowUnknownPatient` dan kecocokan unit dengan `EmgSetting` **tidak** diperiksa | Tidak ada di validation §10.2; kartu aturan 8: unit diambil dari encounter |
| 4 | Encounter `Outpatient` (masa transisi `IGD-DEC-109`) ditolak `400` | Validation §10.2 aturan 4 menyebut `Emergency` saja; berbeda dari `PeriksaJenisEncounter` jalur lama yang masih menerima `Outpatient` |
| 5 | `mode` diterima tanpa membedakan huruf besar/kecil | Kontrak menyebut `Triage`/`ImmediateCare`; `triage` tidak lagi dijawab pesan yang membingungkan |
| 6 | `arrivalDateTime` tanpa zona waktu dianggap UTC | Konvensi `NormalizeUtc` di repository; contoh kontrak memakai akhiran `Z` |
| 7 | "tiba pukul {waktu}" ditulis `HH.mm WIB tanggal dd-MM-yyyy` pada zona `Asia/Jakarta` (cadangan `SE Asia Standard Time`, lalu UTC+7) | Kontrak tidak menetapkan format; zona `Asia/Jakarta` dipakai konfigurasi repository |
| 8 | `{status}` pada pesan "sudah berakhir" adalah nama status akhir yang disimpulkan dari lima tanda (misalnya `Cancelled` bila `IsCancel` benar walau statusnya masih `Registered`) | Menulis `Registered` pada pesan "sudah berakhir" akan menyesatkan |
| 9 | EF membuat index `IX_EmgVisit_ArrivalConfirmedByUserId` untuk FK baru | Konvensi EF untuk setiap FK (sama dengan `RegistrationCompletedByUserId`); `02-backend-architecture.md` §13.6 menulis "Index: tidak ada yang baru" |
| 10 | Deskripsi `[AccessAction("Create", …)]` pada `StartTriage` merangkum ketiga pemakai izin `Create` | Pemindai Akses Role menyimpan satu baris per `(controller, aksi)`; teks deskripsi pemenang terakhir yang tampil. Izinnya tetap satu, `EmergencyVisit : Create` — kosmetik |
| 11 | Amplop sukses membawa `statusCode` 201/200 sesuai HTTP | Pola `BillingFinalizationsController`; controller Kepergian IGD mengirim amplop 200 pada HTTP 201 |
| 12 | Penyimpanan diulang **sekali** bila bentrok unique index | Kontrak: "insert kedua ditangkap, lalu dijawab sesuai tabel"; pengulangan menemukan kunjungan yang sudah ada |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Satu endpoint baru `POST /start-triage`; tiga ruas baru pada `EmergencyVisitResponse` (semua endpoint yang mengembalikannya). Nol ruas lama berubah, nol route lama berubah. `POST /emergency-visits` jalur lama **tidak** diubah (milik `BE-IGD-053`) — kunjungan dari jalur itu bersumber `Unverified` |
| Database | **Schema berubah, migration BELUM dibuat.** Tiga kolom pada `public."EmgVisit"` + FK + index FK. Migration `AddEmergencyArrivalTimeSource` **dibuat dan diterapkan pemilik** (bagian 5.2). Nol eksekusi basis data oleh agent |
| Keamanan/Auth | Izin yang sudah ada `EmergencyVisit : Create` — nol izin baru. Pasangan atribut identik huruf demi huruf dengan pemakai lain. Pelaku dan waktu konfirmasi diambil dari token dan jam server, tidak dari body. Keterbatasan `IGD-DEC-158` tetap berlaku |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/start-triage` | Melahirkan kunjungan IGD dari encounter — Mulai Triage atau Tangani Segera; idempoten | `EmergencyVisit : Create` |

**Request** (`StartEmergencyVisitRequest`).

```json
{ "encounterId": "3f2a1c9e-…", "mode": "Triage", "arrivalDateTime": "2026-09-22T02:25:00Z" }
```

```json
{ "encounterId": "3f2a1c9e-…", "mode": "ImmediateCare" }
```

**Response** — `ApiResponse<EmergencyVisitResponse>`; ruas baru:

| Ruas | Tipe | Isi |
| --- | --- | --- |
| `arrivalTimeSource` | `int` | `0` data lama/jalur loket, `1` fallback `RegisteredAt`, `2` dikonfirmasi perawat |
| `arrivalConfirmedByName` | `string?` | `DisplayName` → `UserName` → `Email` → `UserCode` pengonfirmasi |
| `arrivalConfirmedAt` | `datetime?` | Jam server saat konfirmasi |

| Kode | Keadaan | Pesan (validation §10.2) |
| --- | --- | --- |
| `201` | Kunjungan lahir | *"Kunjungan IGD dimulai dan menunggu triage."* / *"Kunjungan IGD dimulai dengan penanganan segera."* |
| `200` | Kunjungan sudah ada (idempoten) | *"Kunjungan IGD untuk encounter ini sudah ada dengan status {status}."* |
| `400` | `encounterId` kosong | *"encounterId wajib diisi."* |
| `400` | `mode` salah/kosong | *"Pilih Mulai Triage atau Tangani Segera."* |
| `400` | `Triage` tanpa waktu tiba | *"Waktu tiba wajib diisi untuk memulai triage."* |
| `400` | Waktu tiba di masa depan | *"Waktu tiba tidak boleh melewati waktu sekarang."* |
| `400` | Tanpa identitas, tanpa nama sementara | *"Nama sementara wajib diisi untuk pasien yang belum diketahui identitasnya."* |
| `400` | Encounter bukan Emergency | *"Encounter ini bukan kunjungan gawat darurat."* |
| `404` | Encounter tidak ada | *"Encounter tidak ditemukan."* |
| `409` | Encounter berakhir | *"Encounter ini sudah berakhir ({status}). Daftarkan ulang pasien bila ia kembali."* |
| `409` | K4 | *"Kunjungan IGD untuk encounter ini pernah dihapus, sehingga encounter ini tidak dapat dimulai lagi. Daftarkan ulang pasien."* |
| `409` | Kunjungan lain berjalan | *"Pasien ini masih memiliki kunjungan IGD aktif bernomor {nomor}, tiba pukul {HH.mm} WIB tanggal {dd-MM-yyyy}. Buka kunjungan tersebut, jangan mendaftar ulang."* |
| `401` / `403` | Tanpa token / tanpa izin | Penanganan standar |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pembacaan diff dan scope | 6 berkas diubah + 2 berkas baru, seluruhnya di modul IGD; nol `Program.cs`, `Migrations/`, `ApplicationDbContext.cs`, Registrasi, kontrak, frontend, `DataProtectionKeys` | `PASS` | `git status --short` bagian 7 |
| Nol baris komentar baru | `git diff -U0` baris tambahan yang diawali `//` = **0**; kedua enum baru 0 | `PASS` | Perintah `grep -c` atas diff |
| Format berkas | BOM dipertahankan pada berkas yang memilikinya; line ending seragam per berkas (`git ls-files --eol`); index tetap LF (`text=auto`) | `PASS` | Pemeriksaan agent |
| Pemeriksaan rahasia | Nol credential, token, connection string, atau isi kunci pada diff dan laporan | `PASS` | Pembacaan diff |
| Pembacaan source kriteria 6 | Waktu tiba hanya dihitung pada cabang `mode == Triage` (`EmergencyVisitService.cs:654-663`); `ImmediateCare` memakai `encounter.RegisteredAt` (`:784`) | `PASS` (bagian source saja) | Pembacaan source |
| Pembacaan source kriteria 8 | Satu-satunya `new EmgVisit` baru ada di `MulaiKunjunganDalamKunciAsync` (`:775`); `GET /`, `GET /{id}`, `GET /active-episode` hanya membaca (`AsNoTracking`) | `PASS` (bagian source saja) | `grep "new EmgVisit"` + pembacaan source |
| `dotnet build` | Belum — milik pemilik | `NOT RUN` | Larangan task |
| Migration `AddEmergencyArrivalTimeSource` | Belum dibuat — milik pemilik | `NOT RUN` | Larangan task |
| Uji API S1–S10, uji paralel, kueri hitungan | Belum — butuh migration dan build | `NOT RUN` | — |

Uji manual: `REQUIRED` — skenario 5.3, sesudah migration diterapkan dan build berhasil.

**Tidak dijalankan:** `dotnet build`, `dotnet ef`, kueri basis data, dan aplikasi — seluruhnya di luar wewenang
task. Tidak ada proyek test backend (dihapus 11 September 2026), jadi tidak ada `dotnet test`.

### 5.1 Perintah build untuk pemilik

```bash
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
```

Hasil yang diharapkan: 0 error; jumlah warning sama dengan baseline Anda. Galat kompilasi pada delapan berkas di
bagian 3.2 adalah `NEW ERROR` task ini dan dikembalikan ke agent.

### 5.2 Migration untuk pemilik (`AddEmergencyArrivalTimeSource`)

Urutan: build (5.1) → buat migration → periksa → tambahkan penjaga `Down()` → (opsional) uji Down/Up di
Docker lokal → terapkan.

```bash
dotnet ef migrations add AddEmergencyArrivalTimeSource --no-build
```

**Isi `Up()` yang diharapkan — tidak lebih:**

| Operasi | Isi |
| --- | --- |
| `AddColumn` | `ArrivalTimeSource` `integer NOT NULL DEFAULT 0` |
| `AddColumn` | `ArrivalConfirmedByUserId` `uuid NULL` |
| `AddColumn` | `ArrivalConfirmedAt` `timestamp with time zone NULL` |
| `CreateIndex` | `IX_EmgVisit_ArrivalConfirmedByUserId` |
| `AddForeignKey` | `FK_EmgVisit_AspNetUsers_ArrivalConfirmedByUserId`, `ON DELETE RESTRICT` |

Bila `Up()` memuat operasi lain — terutama `DropTable`/`DropColumn` milik modul lain — **berhenti**: itu gejala
snapshot yang kehilangan blok modul lain. `git diff` snapshot seharusnya hanya menambah tiga properti, satu
index, dan satu relasi pada blok `EmgVisit`.

**Penjaga `Down()`** — sisipkan di baris pertama `Down()` (pola `BE-IGD-048`):

```csharp
migrationBuilder.Sql("""
    DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."EmgVisit" WHERE "ArrivalTimeSource" <> 0) THEN
            RAISE EXCEPTION 'Down BE-IGD-055 dihentikan: ada kunjungan IGD dengan sumber waktu tiba Fallback atau Confirmed (ArrivalTimeSource <> 0). Menghapus kolom ini menghilangkan konfirmasi perawat tanpa jejak.';
        END IF;
    END $$;
    """);
```

Terapkan: `dotnet ef database update --no-build`. Seluruh baris lama otomatis bernilai `0` (`Unverified`) —
tidak ada konfirmasi yang dikarang.

### 5.3 Skenario uji API untuk pemilik

Token pengguna yang memegang `EmergencyVisit : Create`. "Encounter baru" = encounter `Emergency` yang dibuat
lewat pendaftaran dan **belum** punya kunjungan IGD (cek dengan kueri hitungan di bawah = 0). Selama
`FE-IGD-036` belum ada, layar pendaftaran masih melahirkan kunjungan sendiri — encounter uji perlu dibuat lewat
`POST /patient-encounters` langsung.

| Skenario | Langkah | Hasil yang diharapkan | Kriteria |
| --- | --- | --- | --- |
| S1 | Encounter baru; `mode: "Triage"`, `arrivalDateTime` = `RegisteredAt` − 10 menit | `201`; `visitStatus` 2; `arrivalTimeSource` 2; `arrivalDateTime` sama dengan kiriman; `arrivalConfirmedByName` = nama Anda; `arrivalConfirmedAt` ≈ sekarang | 1 |
| S2 | Encounter baru; `{ encounterId, mode: "ImmediateCare" }` | `201`; `visitStatus` 4; `treatmentStartedAt` ≈ sekarang; `arrivalDateTime` = `RegisteredAt`; `arrivalTimeSource` 1; `arrivalConfirmedByName` `null` | 2 |
| S3 | Ulangi S2 pada encounter baru lain dengan tambahan `arrivalDateTime: "2020-01-01T00:00:00Z"` | Tetap `RegisteredAt`, sumber 1 — nilai kiriman diabaikan | 6 |
| S4 | Ulangi permintaan S1 persis | `200`; `id` sama; `visitStatus` tetap 2; hitungan = 1 | 4 |
| S5 | Pada kunjungan S1 kirim `mode: "ImmediateCare"`; lalu kirim `mode: "Triage"` lagi | Pertama `200`, `visitStatus` 4; kedua `200`, tetap 4 (tidak mundur) | 3, 4 |
| S6 | Encounter baru; kirim **Triage** dan **ImmediateCare** serentak (dua jendela/`Start-Job` PowerShell) | Satu `201` + satu `200`; hitungan = 1; `visitStatus` akhir 4 | 3 |
| S7 | `encounterId` acak → `404`; encounter `Outpatient` → `400`; encounter kunjungan S2 sesudah `PATCH /{id}/complete` → `409` "sudah berakhir (Completed)"; encounter yang kunjungannya di-`DELETE` → `409` K4 | Sesuai tabel bagian 4 | 5 |
| S8 | Pasien P punya kunjungan berjalan pada encounter A; buat encounter B untuk P; `start-triage` B | `409` "Pasien ini masih memiliki kunjungan IGD aktif bernomor …, tiba pukul … WIB tanggal …" | 5 |
| S9 | `Triage` tanpa `arrivalDateTime` → `400`; `arrivalDateTime` = sekarang + 1 jam → `400`; `mode: "abc"` → `400`; tanpa `encounterId` → `400` | Pesan sesuai tabel bagian 4 | 6 |
| S10 | `isUnknownPatient: true` tanpa alias → `400`; dengan alias `"Tn. X kecelakaan"` → `201`, alias tersimpan | Sesuai | 7 |
| S11 | Encounter baru: `GET /`, `GET /{id}` kunjungan lain, `GET /active-episode?patientId=` pasiennya | Hitungan encounter itu tetap 0 | 8 |
| S12 | Pengguna tanpa `EmergencyVisit : Create` | `403` | — |

Kueri hitungan (baca-saja, dijalankan pemilik):

```sql
SELECT COUNT(*) AS jumlah_kunjungan, MIN("VisitStatus") AS status, MIN("ArrivalTimeSource") AS sumber
FROM public."EmgVisit"
WHERE "EncounterId" = '<encounterId>';
```

---

## 6. Acceptance criteria dan Definition of Done

| No | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Mulai Triage dengan waktu tiba 10 menit sebelum waktu terdaftar → `201`, `WaitingForTriage`, sumber `Confirmed` | Belum terpenuhi — source ada | `EmergencyVisitService.cs:775-800`; uji S1 `NOT RUN` |
| 2 | Tangani Segera pada baris tanpa kunjungan → `201`, `InTreatment` dalam satu permintaan tanpa isian, tiba = waktu terdaftar, sumber `Fallback` | Belum terpenuhi — source ada | `:784-795`; uji S2 `NOT RUN` |
| 3 | Mulai Triage dan Tangani Segera paralel → tepat satu kunjungan, status akhir `InTreatment` | Belum terpenuhi — source ada | Kunci `EmergencyEpisodeRule.cs:125-148`, penerusan `EmergencyVisitService.cs:751-758`, pengulangan `:695-702`; uji S5/S6 `NOT RUN` |
| 4 | Panggilan kedua `Triage` → `200`, kunjungan sama, status tidak mundur | Belum terpenuhi — source ada | `:741-761`; uji S4/S5 `NOT RUN` |
| 5 | Encounter berakhir → `409`; bukan Emergency → `400`; tidak ada → `404`; K4 → `409`; kunjungan lain berjalan → `409` | Belum terpenuhi — source ada | `:721-749`, `:763-770`; uji S7/S8 `NOT RUN` |
| 6 | Waktu tiba di masa depan → `400`; nol jalur yang mengisi waktu tiba dari jam klien untuk mode `ImmediateCare` | **Sebagian** — bagian "baca source" terpenuhi | Pembacaan source bagian 5 `PASS`; uji S3/S9 `NOT RUN` |
| 7 | Pasien rekam pengganti: `isUnknownPatient` + alias tersimpan; tanpa alias → `400` | Belum terpenuhi — source ada | `:665-669`, `:789-790`; uji S10 `NOT RUN` |
| 8 | Membuka detail atau daftar tidak melahirkan kunjungan | **Sebagian** — bagian source terpenuhi | Pembacaan source bagian 5 `PASS`; hitungan S11 `NOT RUN` |
| 9 | Migration: baris lama `Unverified`; `Down()` berpenjaga diuji di basis data terpisah; snapshot hanya bertambah tiga kolom | Belum terpenuhi — **milik pemilik** | Bagian 5.2. Catatan: snapshot juga akan memuat index + relasi FK (selisih 9) |
| 10 | Build 0 error, warning sama dengan baseline | Belum terpenuhi — **milik pemilik** | Bagian 5.1 |

**DoD.** Acceptance 1–10 belum terbukti penuh; laporan tracked ada (berkas ini). `BE-IGD-053`, `057`, `058`
**belum** boleh dianggap terbuka sampai migration diterapkan dan build berhasil — ketiganya memakai kolom dan
method dari task ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Model sudah memuat tiga kolom baru.** Sampai migration diterapkan, setiap kueri ke `EmgVisit` (daftar kunjungan, triage, penugasan dokter, dst.) akan gagal "column does not exist" pada basis data yang belum dimigrasi. Jangan menjalankan aplikasi hasil build ini terhadap basis data mana pun sebelum `database update` |
| Masalah yang diketahui | (1) Jalur lama `POST /emergency-visits` belum mengambil kunci per pasien — tabrakannya dengan `start-triage` hanya dijaga unique index + pengulangan sekali; ditutup `BE-IGD-053`. (2) `PUT /emergency-visits/{id}` masih dapat mengubah `ArrivalDateTime` tanpa mengubah sumbernya — ditutup `BE-IGD-058`. (3) Respons `PUT`/`PATCH` lama menampilkan `arrivalConfirmedByName` `null` karena navigasinya tidak dimuat di jalur itu; `GET` menampilkannya benar. (4) `IGD-OQ-108`: encounter kedua hasil override belum dapat `start-triage` selama kunjungan lama berjalan (disengaja, klausa B) |
| Risiko tersisa | `HasDefaultValue(Unverified)` bernilai sama dengan bawaan CLR (`0`): EF tidak mengirim kolom itu saat nilainya `Unverified` dan basis data mengisi `0` — hasilnya identik. Keterbatasan izin bersama `IGD-DEC-158` tetap ada. Layar belum memakai endpoint ini (`FE-IGD-036`) |
| Temuan di luar scope | **Kunci DataProtection sudah terdorong ke remote.** `origin/rizkiG` = `48a78703`: commit `69953e98` memuat `DataProtectionKeys/key-6f6691ec-….xml`, dan `48a78703` menamainya ulang menjadi `key-2c83ad0f-….xml` (kunci baru hasil app). `key-d92e412f-….xml` juga masih ter-track. Karena sudah di-push, `git commit --amend` tidak lagi cukup — perlu keputusan pemilik (misalnya mencabut kunci dari tracking + `.gitignore`, dan menganggap kunci yang terdorong bocor). Agent tidak menyentuh git maupun berkas kunci. Selain itu `MODULE-STATUS.md` (artefak akar) **tidak** diperbarui — di luar wewenang build skill; angka progres di sana perlu diselaraskan lewat `manage-module-blueprint` |
| Perubahan sampingan | `NONE` |
| Interupsi | Pesan pemilik di tengah task: "jangan ada baris line comment" — diterapkan pada seluruh kode baru (nol `//` dan `///`); dicatat sebagai preferensi tetap |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | (1) Pemilik: build 5.1 → migration 5.2 → uji 5.3, lalu kabari hasilnya untuk penandaan ulang. (2) Sesudah migration diterapkan: `BE-IGD-052` (migration berikutnya dibuat di atas milik task ini), lalu `BE-IGD-054` (tanpa migration). (3) `BE-IGD-053`, `057`, `058` sesudah task ini ✅ |

```text
 M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyVisitDtos.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyEpisodeRule.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs
 M Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgVisitConfiguration.cs
 M docs/module-blueprints/igd/roadmap/backend-roadmap.md
 M docs/module-blueprints/igd/roadmap/requirement-traceability.md
 M docs/module-blueprints/igd/task/report/backend/BE-IGD-051.md        (sebelum task; tidak disentuh)
?? Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyArrivalTimeSource.cs
?? Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyVisitStartMode.cs
?? docs/module-blueprints/igd/task/report/backend/BE-IGD-055.md
```
