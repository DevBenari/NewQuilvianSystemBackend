# Laporan Perubahan Backend — `BE-NV-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-010` |
| Judul | Login dan runtime akun tablet bed |
| Slice | `MVP-6` — `EPIC NV-05` (gelombang backend 3, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-010` |
| Trace | `FR-NV-034` (nonaktif langsung ditolak runtime), `FR-NV-036`; `NV-DEC-030`, `046`, `056`; `AD-02`; `G-09`; `VAL-NV-14` × tablet, `VAL-NV-26`, `27`; `NV-AC-041`; `UAT-12`, `14`, `26`; `contracts/api-contract.md` R2 §6 `GET /current`; `02-backend-architecture.md` R2.4.5, R2.5, R2.10; permission-audit R2 §6–§8 |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`) |
| Dependency | `BE-NV-009` ✅ (6 Nov 2026) |
| Klasifikasi | `HEAVY` — skor 10: satu repository (0), berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika sedang (1), endpoint baru sesuai kontrak (1), persistence existing (1), **dampak inti pada authentication** — login seluruh sistem (2), UI tidak ada (0); syarat lead `NV-DEC-030`/`056` |
| Task mode | `BACKEND` — perintah Rizki 6 Nov 2026 ("mulai sekarang BE-NV-010"), target tulis `NewQuilvianSystemBackend` branch `rizkiG` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, `Controllers/AuthController.cs`, `Constants/AuthorizationPolicies.cs`, `Program.cs` (restu `NV-DEC-056`), laporan ini, baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13fa1c4d` (branch `rizkiG`, sejajar `origin/rizkiG`); kode task ini masuk commit `589fb513` (Rizki, 6 Nov 2026 15.45) |
| Commit backend saat diuji | `589fb513`, working tree bersih; DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 7 Nov 2026 14.56 dan memuat `NvwBedTabletRuntimeController`, `NvwBedTabletContextService`, `NurseViewBedTabletAccess`, klaim `is_nurse_view_bed_tablet_account` |
| Tanggal | 2026-11-06 (implementasi); 2026-11-08 (uji sesudah build) |
| Status | ✅ **SELESAI 8 November 2026** — AC 5/5 terpetakan ke source dan terbukti runtime. QBE Strict `PASS` (8 berkas). Build oleh Rizki. Regresi login sesudah build **172/172** terhadap baseline yang direkam sebelum kode berjalan. Uji runtime tablet **13/13 `PASS`**. Log backend bersih dari kata sandi, token, dan identitas pasien. Riwayat: 🟡 SEBAGIAN 6 Nov 2026 sebelum build dan uji |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-NV-009`, admin bisa membuat akun login tablet, tetapi sistem belum mengenali akun itu
sebagai tablet. Akun `nvt-000001` masih login sebagai akun biasa tanpa hak (`BE-NV-009` no. 16),
belum ada layar runtime yang bisa dibukanya, dan tablet nonaktif yang mencoba login hanya mendapat
pesan "Akun terkunci sementara…", yang menyesatkan.

**Contoh.** Tablet bed 305-A dipasang di samping pasien. Tanpa task ini tablet tidak tahu bed
mana miliknya, sehingga tombol panggil (`BE-NV-011`) tidak punya tempat menempel.

---

## 2. Proses bisnis

**Tujuan:** tablet masuk dengan akun perangkatnya dan hanya bisa membuka runtime tablet untuk
bed-nya sendiri; akun TV dan akun manusia ditolak di runtime tablet; akun tablet ditolak di papan
TV; login akun lain tidak berubah.

**Langkah normal:**

1. Tablet login dengan akun `nvt-000001`. Sistem mengenali akun itu lewat `NvwBedTablet.LoginUserId`
   (resolver dipanggil **sesudah** resolver TV Nurse View; resolver Kiosk, display antrean, dan TV
   tidak diubah).
2. Seperti akun perangkat lain, pemeriksaan lokasi (geofence) dan masa akses akun dilewati.
3. Token berlaku 129.600 menit (90 hari, aturan bawaan perangkat) dan membawa klaim
   `is_nurse_view_bed_tablet_account = true`, `profile_type = NurseViewBedTablet`,
   `nurse_view_bed_tablet_id`, `nurse_view_bed_tablet_session_stamp`. Profil user `NurseViewBedTablet`.
4. Tablet memanggil `GET /bed-tablet-runtime/current`. Pada setiap permintaan sistem memeriksa ulang
   bahwa tablet masih `Active` dan sesinya bukan sesi lama, lalu mencatat `LastOnlineAt` paling
   sering sekali per 60 detik.
5. Jawaban berisi kode tablet, bed, kamar, lantai — **selalu** dari tablet, tidak pernah dari
   request — dan `IsBedOccupied` dari penempatan aktif Rawat Inap (`EndDateTime IS NULL`, tidak
   terhapus). Tanpa nama, inisial, No. RM, maupun umur pasien (`G-09`).

**Akun lain.** Setiap token baru bertambah satu klaim `is_nurse_view_bed_tablet_account = false`.
Itulah satu-satunya beda yang diizinkan AC-1.

**Jalur tidak normal:**

| Kejadian | Jawaban sistem | Kode |
| --- | --- | --- |
| Tablet nonaktif login dengan sandi benar | "Tablet bed dinonaktifkan. Hubungi IT." (`AD-02`, `VAL-NV-27`) | `401` |
| Tablet nonaktif login dengan sandi salah | Pesan terkunci standar, supaya status tablet tidak terbaca orang yang tidak tahu sandinya | `401` |
| Tablet dinonaktifkan saat sedang menyala | Permintaan berikutnya: "Tablet ini tidak dapat dipakai. Hubungi IT." (`VAL-NV-26`) | `403` |
| Token lama sesudah Atur Ulang Login | "Sesi perangkat sudah tidak berlaku. Silakan login ulang." (`NV-SESSION-STALE`) | `403` |
| `POST /auth/refresh` dengan token tablet nonaktif / token lama | Pesan `AD-02` / pesan sesi lama | `401` |
| Akun TV, SuperAdmin/Administrator, atau pegawai membuka runtime tablet | Ditolak policy, tanpa isi | `403` |
| Akun tablet membuka papan TV (`/display-runtime/current`, `/board`) | Ditolak policy TV, karena tidak membawa `nurse_view_device_id` | `403` |

**Contoh tablet dinonaktifkan.** Admin menonaktifkan `NVT-000003` pukul 14.00.00. Pukul 14.00.30
tablet memuat ulang keadaannya dan ditolak `403`; layar tablet (`FE-NV-009`) kembali ke login.
Petugas yang mencoba login lagi dengan sandi benar membaca "Tablet bed dinonaktifkan. Hubungi IT."

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: sama dengan `BE-NV-009` (`AGENTS.md`, `CLAUDE.md`, `docs/engineering/*`, suite skill
  `rules/backend/*`, `role-access-rules.md`, `rule-output/status-task-roadmap.md`)
- Blueprint: kartu `BE-NV-010`, "Berkas bersama per task"; api R2 §6; validation `VAL-NV-26`, `27`;
  `02-backend-architecture.md` R2.4.5, R2.4.6, R2.5, R2.10; permission-audit R2 §6–§8;
  `03-frontend-architecture.md` R2.5, R2.7, R2.10; laporan `BE-NV-003.md` (pola dan baseline)
- Pola: `Controllers/AuthController.cs` (resolver TV baris 1882, klaim baris 2060–2071, `Login`,
  `Me`, `Refresh`, lima fungsi token/cookie/respons); `NvwDeviceContextService.cs`;
  `NvwDisplayRuntimeController.cs`; `NvwDisplayRuntimeDtos.cs`; `Constants/AuthorizationPolicies.cs`;
  `Program.cs` (policy `NurseViewDisplayRead`); `Services/Security/PermissionRegistryDescriptor.cs`
  (deteksi policy yang disetujui); `InpBedPlacementConfiguration.cs` dan
  `InpBedManagementQueryService.cs` (definisi penempatan aktif)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Services/NvwBedTabletContextService.cs` | **Baru.** Konstanta klaim; `ResolveLoginContextAsync` (resolver login); `ResolveActiveTabletAsync` (status `Active` + stempel sesi + `LastOnlineAt` ≤ 1×/60 detik); `GetCurrentAsync` (bed dari tablet, `IsBedOccupied`, `ActiveCall`, `RefreshIntervalSeconds`, `ServerTimeUtc`) |
| `…/Services/NvwPatientCallQueryService.cs` | **Baru.** `GetForTabletAsync` (panggilan `Waiting`/`Claimed` bed tablet; selalu kosong sampai `BE-NV-011`), `MapTabletCall`, label prioritas dan status. Bagian konsol dan strip menyusul di `BE-NV-012`/`015` |
| `…/Controllers/NvwBedTabletRuntimeController.cs` | **Baru.** `GET /current`, `[Authorize(Policy = NurseViewBedTabletAccess)]`, `[Tags("Health Services / Nurse View Management / Bed Tablet Runtime")]` |
| `…/DTOs/NvwBedTabletRuntimeDtos.cs` | **Baru.** `NvwBedTabletRuntimeResponse`, `NvwTabletCallResponse` (api §6), `NvwBedTabletLoginContext` |
| `…/Services/NvwResult.cs` | +4 baris: kode dan pesan `VAL-NV-26`, `VAL-NV-27` |
| `Controllers/AuthController.cs` | Berkas bersama — rincian di bawah |
| `Constants/AuthorizationPolicies.cs` | Berkas bersama — rincian di bawah |
| `Program.cs` | Berkas bersama — rincian di bawah |

Nol baris komentar baru, termasuk pada tambahan di berkas bersama.

**Diff ringkas per berkas bersama (syarat kartu dan `NV-DEC-030`/`056`):**

| Berkas bersama | Yang ditambahkan | Yang tidak disentuh |
| --- | --- | --- |
| `Controllers/AuthController.cs` (+146/−21; ke-21 baris "dihapus" adalah baris yang sama dengan satu argumen `bedTabletContext` ditambahkan) | Satu dependensi `NvwBedTabletContextService`; resolver `ResolveNurseViewBedTabletLoginContextAsync` dengan `try/catch` (pola TV); pada `Login`, `Me`, `Refresh` satu baris pemanggilan resolver; `bedTabletContext.IsBedTabletAccount` pada `isDeviceLoginAccount` (`Login`, `Refresh`); di cabang terkunci, hanya tablet nonaktif + sandi benar yang mendapat pesan `AD-02`; blok tolak tablet tidak aktif (`Login`, `Refresh`) dan tolak stempel sesi lama (`Refresh`); blok klaim tablet `true`/`false` di `GenerateJwtToken`; parameter opsional `bedTabletContext` pada `GenerateJwtToken`, `SetAuthCookie`, `ResolveJwtExpireMinutes`, `BuildAuthInfoResponse`, `BuildUserResponse`; satu cabang `profileType = NurseViewBedTablet` sesudah `NurseViewDevice` | Isi `ResolveKioskLoginContextAsync`, `ResolveQueueDisplayLoginContextAsync`, `ResolveNurseViewLoginContextAsync`; klaim Kiosk, display antrean, TV; `/fingerprint-login`; DTO `UserLoginResponse`, `AuthInfoResponse`; urutan pemeriksaan login yang sudah ada |
| `Constants/AuthorizationPolicies.cs` (+4/−1) | Konstanta `NurseViewBedTabletAccess` dan satu entri di `ApprovedAlternativeAuthorization`. Alasan: akun tablet tidak punya Departemen × Posisi, sehingga `HasAccessAsync` tidak punya baris untuk diperiksa (sama dengan Kiosk, display antrean, TV) | Policy lain |
| `Program.cs` (+12/−0) | `AddScoped<NvwPatientCallQueryService>()`, `AddScoped<NvwBedTabletContextService>()`; satu `AddPolicy(NurseViewBedTabletAccess)` yang wajib klaim `nurse_view_bed_tablet_id` tidak kosong, **tanpa** jalan pintas SuperAdmin/Administrator | Policy `KioskRead`, `QueueDisplayRuntimeRead`, `QueueDisplayRead`, `NurseViewDisplayRead` |

**Tiga keputusan developer di luar teks kartu:**

1. **Runtime tablet tanpa `[AccessController]`/`[AccessAction]`.** Runtime TV (`BE-NV-003`) memasang
   keduanya, sehingga `/auth/permissions` akun akses penuh bertambah satu butir. Untuk tablet,
   permission-audit R2 §6 hanya mendaftar tiga resource baru dan AC-1 hanya mengizinkan beda satu
   klaim. Gerbang integritas tetap lulus karena policy-nya ada di `ApprovedAlternativeAuthorization`
   (`PermissionRegistryDescriptor.HasApprovedAlternativeAuthorization`), dan endpoint ini tidak
   muncul di layar Akses Role — memang tidak boleh diberikan ke peran manusia (§7 kolom Runtime).
2. **Akun tablet diperlakukan sebagai akun perangkat** pada geofence, masa akses akun, dan masa
   berlaku token (129.600 menit), meniru TV dan display antrean (`NV-DEC-030`). Tanpa ini tablet
   kiosk di samping bed harus login ulang tiap 60 menit oleh pasien. Status dan sesinya tetap
   diperiksa di setiap permintaan, sehingga penonaktifan berlaku pada permintaan berikutnya.
3. **`RefreshIntervalSeconds` = 15 bila ada panggilan aktif, 60 bila tidak** — angka `NV-FE-009`
   yang disahkan `NV-DEC-055`. Selama `BE-NV-011` belum ada, nilainya selalu 60.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /bed-tablet-runtime/current` sesuai api §6. Token semua akun bertambah klaim `is_nurse_view_bed_tablet_account = false`. Tidak ada field respons login yang berubah; profil `NurseViewBedTablet` hanya untuk akun tablet |
| Database | Tanpa migration, tanpa perubahan skema. Menulis hanya `NvwBedTablet.LastOnlineAt`. Membaca `NvwBedTablet`, `MstBed`, `MstRoom`, `InpBedPlacement`, `NvwPatientCall`, `AspNetUsers` |
| Keamanan/Auth | Policy baru di `ApprovedAlternativeAuthorization`. Pemisahan TV ↔ tablet: policy TV wajib `nurse_view_device_id`, policy tablet wajib `nurse_view_bed_tablet_id`, dan resolver membaca dua tabel berbeda — akun tidak bisa memegang keduanya. Beda pesan sandi benar/salah hanya pada tablet nonaktif; sandi selalu diganti saat diaktifkan kembali |

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Bed Tablet Runtime

Base URL: `api/v1/health-services/nurse-view-management/bed-tablet-runtime`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/current` | Bed tablet, berpenghuni atau tidak, panggilan aktifnya, interval muat ulang, jam server | policy `NurseViewBedTabletAccess` |

Kode status: `200` berhasil · `401` belum login/token kedaluwarsa · `403` bukan akun tablet, tablet
tidak aktif (`VAL-NV-26`), atau sesi lama (`NV-SESSION-STALE`).

**Contoh response.**
`{ "deviceCode": "NVT-000003", "bedCode": "BD-RSMMC-00015", "bedName": "330-A", "roomName": "KELAS I 330", "floorCode": "3", "isBedOccupied": true, "activeCall": null, "refreshIntervalSeconds": 60, "serverTimeUtc": "…" }`

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <8 berkas>` (array path) | Files evaluated **8** (= 8 berkas) · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-06 |
| Review diff berkas bersama | Hanya tambahan; 21 baris `AuthController` berubah karena satu argumen tambahan | `PASS` | `git diff` |
| Nol komentar baru; akhiran baris | Nol `//`/`/*` pada berkas baru dan pada baris tambahan berkas bersama; 8 berkas CRLF; `AuthController.cs` tetap ber-BOM; `Program.cs` tetap tanpa baris baru di akhir | `PASS` | `grep`, hitungan byte Node |
| Baseline regresi login, direkam sebelum kode berjalan (backend `13fa1c4d`; DLL 10.56, rute tablet masih `404`) | 12 skenario: SuperAdmin, perawat, perawat ruangan, dokter, dokter kedua, admisi, LOKET, Kiosk `KSK-RSMMC-00009`, display antrean `QDD-RSMMC-00003`, TV `NVW-000002`, sandi salah, email tak dikenal. Per akun: status, pesan, auth, isi user, atribut cookie, klaim token, `/me`, `/permissions` (isi), lima probe policy, `/refresh` | `PASS` (terekam) | Rujukan: `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-010-20261106/baseline-2026-11-06T04-31-22-400Z/` (11.31 WIB). Rekaman pertama `baseline-2026-11-06T04-20-41-467Z` (8 skenario) disimpan sebagai cadangan |
| Uji pembanding terhadap baseline itu sendiri | 86/104 lolos pada rekaman pertama; 18 yang gagal tepat tiga cek per akun yang menuntut BE-NV-010 (klaim baru di login dan refresh, rute 404 → 403) | `PASS` (alat bekerja) | `bandingkan.mjs` |
| `dotnet build` | Dijalankan Rizki (prompt sesi 8 Nov menyebut build 6 Nov 15.34; DLL di disk bertanggal 7 Nov 14.56, dari working tree bersih di `589fb513`). Backend dijalankan Rizki `dotnet run --no-build` 8 Nov 09.14 WIB; rute baru tanpa login → `401` | `PASS` menurut pernyataan Rizki; jumlah warning tidak dinyatakan | DLL memuat keempat simbol task ini (pemeriksaan byte agent) |
| Regresi login sesudah build (AC-1, `NV-AC-041`) — 8 Nov 09.22 WIB | **172/172** (10 akun × 17 cek + 2 login gagal). Pemeriksaan agent langsung atas JSON mentah: pada **setiap** akun, beda klaim login dan refresh hanya `is_nurse_view_bed_tablet_account: null → "false"`; TV tambahan `nurse_view_session_stamp` (memang baru karena atur ulang); probe `bedTabletCurrent` `404 → 403`; probe lain, `/me`, `/permissions`, isi user, cookie, dan masa berlaku sama | `PASS` | `sesudah-2026-11-08T02-22-11-365Z/` + `perbandingan.json` |
| Pra-cek bed uji (SELECT read-only, diulang tepat sebelum uji) | `BD-RSMMC-00028` (326-C, lantai 3): 1 penempatan aktif, 1 pasien, 0 tablet status 1/2, 0 panggilan aktif. `BD-RSMMC-00001` (BAYI-1-A, lantai 2): 0 penempatan aktif, 0 tablet status 1/2, 0 panggilan aktif. Nama dan No. RM pasien diteruskan ke skrip lewat env tanpa dicetak | `PASS` | `pra-cek.json`; skrip `cek-bed.mjs` |
| Uji runtime tablet no. 1–13 (AC-2 s.d. AC-5) — 8 Nov 09.23–09.24 WIB | **13/13 `PASS`** — rincian di bawah | `PASS` | `tablet-2026-11-08T02-23-10-364Z/hasil.json`, `percobaan.json` (45 permintaan) |
| Pemindaian log backend (`quilvian-backend-20261108.json`, 290 baris sehari) | Jendela A 100 permintaan HTTP, jendela B 45 (= 45 `percobaan.json`). Rute tablet di B: `200` ×4 (no. 3, 4, 8, 10), `403` ×6 (no. 6 ×4, 10, 11). Nol `Error`/`Fatal`. Penulisan di B hanya yang direncanakan (lihat di bawah). Kemunculan sandi env ketujuh akun, nama pasien, No. RM, pola sandi tablet `Nvt@…`, TV `Nvw@…`, reset kiosk/display `Uji…`, kedua sandi salah, ruas `password` berisi nilai, dan token JWT: **semuanya 0** | `PASS` | `pindai-log-agent.json` (skrip `pindai-log.mjs`) |
| Keadaan DB sesudah uji (SELECT read-only) | `NVT-000011`, `NVT-000012` status 3 dengan alasan; 0 tablet status 1/2 dan 0 `NvwPatientCall` di kedua bed uji; policy jabatan uji 20 butir aktif, 0 butir `NurseViewBedTablet` | `PASS` | Kueri agent 8 Nov 09.25 WIB |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

Uji manual: `REQUIRED`.

**Tidak dijalankan:** `dotnet build`, `dotnet run`, `dotnet ef` — milik Rizki. Tanpa migration.

**Catatan baseline.** Untuk merekam Kiosk, display antrean, dan TV, sandi `KSK-RSMMC-00009`,
`QDD-RSMMC-00003`, dan `NVW-000002` diatur ulang lewat sesi SuperAdmin (pola `BE-NV-003`); sandinya
hanya pernah ada di memori skrip. `NVW-000002` dipilih karena belum pernah online, jadi tidak ada TV
yang terputus. Skrip `sesudah` mengatur ulang ketiganya sekali lagi.

### Perintah untuk Rizki

```bash
# PC kantor
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false -p:SkipMigrationMetadata=true
# laptop
dotnet build-server shutdown
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false -p:SkipMigrationMetadata=true -m:1 -nodeReuse:false
# jalankan ulang backend
dotnet run -p:RunAnalyzers=false --no-build
```

### Uji sesudah build — urut

**Bagian A — regresi login (AC-1, `NV-AC-041`).** `node rekam-login.mjs sesudah`, lalu
`node bandingkan.mjs baseline-2026-11-06T04-31-22-400Z <folder-sesudah>`. Lulus bila setiap akun
17/17: status, pesan, auth, isi user, cookie, `/me`, `/permissions`, dan probe sama dengan baseline;
klaim yang berbeda hanya `is_nurse_view_bed_tablet_account = false` (ditambah stempel sesi TV yang
memang baru karena atur ulang); `bed-tablet-runtime/current` berubah dari `404` menjadi `403`. Dua
login gagal: status, pesan, dan cookie sama.

**Bagian B — runtime tablet (AC-2 s.d. AC-5).** `node uji-tablet.mjs <bedTerisi> <bedKosong>` dengan
izin sementara `NurseViewBedTablet` pada jabatan uji (salin → tambah → pulihkan) dan dua tablet uji
baru:

| No | Langkah | Hasil yang diharapkan | AC |
| ---: | --- | --- | --- |
| 1 | Login tablet aktif | `200`; klaim tablet `true`, `profile_type` dan profil `NurseViewBedTablet`, id cocok, stempel ada, `is_nurse_view_account` false, tanpa `nurse_view_device_id`; 129.600 menit | AC-1 sisi tablet |
| 2 | Login tablet tanpa koordinat | `200` (geofence dilewati) | — |
| 3 | `GET /current` tablet bed terisi | `200`; `isBedOccupied` true; tanpa kunci `fullName`, `medicalRecordNumber`, `initials`, `ageLabel`, `patientId`, `episodeId`, `birthDate`; nama lengkap dan No. RM pasien bed itu tidak muncul | AC-5 |
| 4 | `GET /current` tablet bed kosong | `200`; `isBedOccupied` false | AC-5 |
| 5 | Token tablet ke `/display-runtime/current` dan `/board` | `403`, `403` | AC-2 |
| 6 | Token TV, SuperAdmin, LOKET, perawat ke `/bed-tablet-runtime/current` | `403` ×4 | AC-3 |
| 7 | Token TV ke papannya sendiri | `200` | AC-1 |
| 8 | Dua `GET /current` berurutan | `LastOnlineAt` terisi sekali; `isOnline` true di daftar admin | — |
| 9 | `/auth/refresh` tablet aktif | `200`; klaim tablet tetap | — |
| 10 | Atur ulang login, lalu token lama | `GET` `403` `NV-SESSION-STALE`; refresh `401`; sandi baru `200` | `VAL-NV-26` sesi lama |
| 11 | Nonaktifkan, tunggu 30 detik, `GET /current` | `403` `VAL-NV-26` | AC-4 |
| 12 | `/auth/refresh` tablet nonaktif | `401` "Tablet bed dinonaktifkan. Hubungi IT." | AC-4 |
| 13 | Login tablet nonaktif sandi benar / salah | `401` pesan `AD-02` / pesan terkunci standar | AC-4, `VAL-NV-27` |

Kedua tablet uji dinonaktifkan dan policy jabatan uji dipulihkan di blok `finally`.

### Hasil uji sesudah build — 8 November 2026

Verifikasi developer oleh agent, bukan UAT. Putusan diambil dari JSON mentah dan log backend.

**Bagian A — 172/172.** Lihat tabel verifikasi di atas.

**Bagian B — 13/13 `PASS`.** Tablet uji `NVT-000011` (bed `BD-RSMMC-00028` 326-C, berpenghuni)
dan `NVT-000012` (bed `BD-RSMMC-00001` BAYI-1-A, kosong), dibuat lewat API admin oleh aktor
Perawat Ruangan berizin sementara.

| No | Terukur |
| ---: | --- |
| 1 | `200`; `profileType` `NurseViewBedTablet`; 129.600 menit; klaim tablet `"true"`, id cocok, stempel ada; `is_nurse_view_account` `"false"`; tanpa `nurse_view_device_id` |
| 2 | `200` "Login berhasil." tanpa koordinat |
| 3 | `200` `{ deviceCode NVT-000011, bedCode BD-RSMMC-00028, bedName 326-C, roomName KELAS II 326, floorCode 3, isBedOccupied true, activeCall null, refreshIntervalSeconds 60, serverTimeUtc }`; nol kunci identitas; nama dan No. RM pasien bed itu tidak muncul |
| 4 | `200` `{ NVT-000012, BD-RSMMC-00001, BAYI-1-A, R. Bayi, lantai 2, isBedOccupied false }` |
| 5 | `/display-runtime/current` `403`, `/board` `403` |
| 6 | TV `NVW-000002`, SuperAdmin, LOKET, perawat → `403` ×4 |
| 7 | TV ke papannya sendiri `200` |
| 8 | `LastOnlineAt` 02.23.18Z sebelum dan sesudah `GET` kedua (tidak ditulis ulang); `isOnline` true |
| 9 | Refresh `200`, klaim tablet dan id tetap |
| 10 | Atur ulang `200`; token lama `403` `NV-SESSION-STALE`; refresh token lama `401` "Sesi perangkat sudah tidak berlaku. Silakan login ulang."; login sandi baru `200`; `GET /current` `200` |
| 11 | Nonaktif `200`; 30 detik kemudian `403` `VAL-NV-26` "Tablet ini tidak dapat dipakai. Hubungi IT." |
| 12 | Refresh tablet nonaktif `401` "Tablet bed dinonaktifkan. Hubungi IT." |
| 13 | Login sandi benar `401` "Tablet bed dinonaktifkan. Hubungi IT."; sandi salah `401` "Akun terkunci sementara karena terlalu banyak percobaan login gagal." |

**Penulisan di log jendela B (02.23.10–02.24.14Z).** SuperAdmin: `POST role-access/policies` ×2
(pasang izin sementara, pulihkan) dan `POST display-devices/{NVW-000002}/login/reset`
(persiapan no. 6). Perawat Ruangan: `POST bed-tablets` ×2, `POST bed-tablets/{id}/login` ×2,
`POST …/login/reset` ×1 (no. 10), `PATCH …/deactivate` ×2 (no. 11 dan pembersihan TB). Sisanya
`POST /auth/login` dan `/auth/refresh`. Policy pulih identik: 20 → 20 butir.

**Catatan pengamatan (bukan kegagalan).**

- Login tablet nonaktif ditolak di cabang "akun terkunci", karena penonaktifan `BE-NV-009`
  mengunci akun. Karena itu log mencatatnya sebagai `Login gagal. Akun terkunci.` dengan
  `UserName` `nvt-000011`. Pesan khusus `Login tablet bed Nurse View gagal. Tablet …` di
  `AuthController` hanya terpakai bila tablet tidak aktif tetapi akunnya tidak terkunci, dan uji
  ini tidak melewatinya (`logLoginTabletGagal` 0). Tablet tetap terlacak lewat `UserName`.
- AC-3 "akun Administrator" diwakili SuperAdmin, peran yang dibebaskan dari pemeriksaan di policy
  lain. Policy tablet tidak punya jalan pintas peran, dan SuperAdmin ditolak `403`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 regresi login perawat, SuperAdmin/Administrator, Kiosk, display antrean, TV, satu login gagal; satu-satunya beda klaim `is_nurse_view_bed_tablet_account = false` | ✅ Terbukti 8 Nov 2026 | Source §3.2; bagian A 172/172, beda klaim hanya `is_nurse_view_bed_tablet_account = "false"` (plus stempel sesi TV karena atur ulang); sisi tablet uji B no. 1 |
| AC-2 akun tablet `403` di `/display-runtime/current` dan `/board` | ✅ Terbukti 8 Nov 2026 | Policy `NurseViewDisplayRead` wajib `nurse_view_device_id`; uji B no. 5 `403`, `403`; no. 7 TV tetap `200` |
| AC-3 akun TV dan Administrator `403` di `/bed-tablet-runtime/current` | ✅ Terbukti 8 Nov 2026 | Policy `NurseViewBedTabletAccess` tanpa jalan pintas peran; uji B no. 6 `403` ×4 (TV, SuperAdmin, LOKET, perawat); bagian A: `403` untuk sepuluh akun non-tablet |
| AC-4 tablet dinonaktifkan → permintaan berikutnya `403`; login ulang sandi benar → pesan `AD-02` (`VAL-NV-27`) | ✅ Terbukti 8 Nov 2026 | Uji B no. 11 (`403` `VAL-NV-26` 30 detik sesudah nonaktif), no. 12 (refresh `401` `AD-02`), no. 13 (sandi benar `AD-02`, sandi salah pesan terkunci standar); no. 10 sesi lama |
| AC-5 `GET /current` bed berpenghuni → `IsBedOccupied = true` tanpa identitas; bed kosong → `false` | ✅ Terbukti 8 Nov 2026 | Uji B no. 3 (`true`, nol kunci identitas, nama dan No. RM pasien tidak muncul), no. 4 (`false`) |
| DoD — regresi login lulus dan direkam | Terpenuhi | Bagian A 172/172, rekaman `sesudah-2026-11-08T02-22-11-365Z/` |
| DoD — build Rizki lulus | Terpenuhi | Backend `--no-build` berjalan dari DLL yang memuat kode task ini |
| DoD — laporan memuat diff ringkas tiap berkas bersama | Terpenuhi | §3.2 |
| DoD — QBE `PASS` | Terpenuhi | §5 |

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (akses data di service; `AuthController` hanya
memanggil `NvwBedTabletContextService`), `QBE-API-001`, `QBE-PERM-001` (policy perangkat terdaftar
di `ApprovedAlternativeAuthorization`, tanpa hardcode peran), `QBE-DTO-001`, `QBE-LOG-001` (log
penolakan login tablet memuat kode dan Id tablet di teks pesan). Tidak berlaku: `QBE-ENT-*`,
`QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*`, `QBE-TXN-001` (satu penulisan `LastOnlineAt`).

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix
`Nvw` `ACTIVE` (registry baris 45) · applicability `NEW CODE`; `AuthController.cs`,
`AuthorizationPolicies.cs`, `Program.cs` = `TOUCHED LEGACY` (tambahan saja, restu `NV-DEC-056`
dengan syarat `NV-DEC-030`) · branch `rizkiG`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sesudah build, **jangan** menjalankan ulang `rekam-login.mjs baseline`: baseline harus berasal dari backend sebelum perubahan. Yang dijalankan hanya mode `sesudah` |
| Masalah yang diketahui | `GET /current` membaca `ActiveCall` per bed, bukan per tablet, karena panggilan melekat pada bed (`NV-DEC-050`). Bila tablet lama sudah diganti saat panggilan masih aktif, tablet pengganti ikut menampilkan panggilan itu — perilaku yang diinginkan untuk "Perawat hadir" |
| Risiko tersisa | Regresi login seluruh sistem **sudah diuji** (bagian A 172/172), sehingga syarat lead untuk sisi backend terpenuhi. Sisi frontend tetap milik `FE-NV-009`. Token tablet berlaku 90 hari; mitigasinya pemeriksaan status dan stempel di setiap permintaan, terbukti pada uji B no. 10–12 |
| Perubahan sampingan | Kode: `NONE`. Data di DB dev bersama akibat uji 8 Nov: (1) tablet `NVT-000011` dan `NVT-000012` dibuat lalu dinonaktifkan dengan alasan; (2) sandi `KSK-RSMMC-00009`, `QDD-RSMMC-00003`, dan TV `NVW-000002` diatur ulang (dua kali untuk TV). Sandinya hanya ada di memori skrip, sama dengan saat baseline. `NVW-000002` kini berstatus aktif dan pernah online; (3) policy jabatan uji diubah sementara lalu dipulihkan identik |
| Interupsi | 6 Nov: satu perintah pembersihan folder sementara ditolak pengaman sesi; perintah itu tidak perlu dan tidak dijalankan ulang. 8 Nov: backend belum hidup di awal sesi, lalu dijalankan Rizki |
| Status Git | Kode task ini sudah di-commit Rizki di `589fb513`. Perubahan sesi 8 Nov hanya dokumen: laporan ini, `backend-roadmap.md`, `frontend-roadmap.md`, `requirement-traceability.md`. Bukti uji ada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-be-nv-010-20261106/` (di-ignore git) |
| Langkah berikutnya | `BE-NV-011` (panggilan dari tablet) dan `FE-NV-009` (login tablet ke layar tablet bed, regresi redirect lima akun) sekarang boleh dimulai |
