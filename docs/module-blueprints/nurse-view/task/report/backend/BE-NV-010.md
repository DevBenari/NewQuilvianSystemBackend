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
| Commit backend saat dikerjakan | `13fa1c4d` (branch `rizkiG`, sejajar `origin/rizkiG`); perubahan belum di-commit |
| Tanggal | 2026-11-06 |
| Status | 🟡 **SEBAGIAN** — AC 5/5 terpetakan ke source; QBE Strict `PASS` (8 berkas, 0 violation, 0 review); baseline regresi login 12 skenario sudah direkam **sebelum** kode berjalan. **Belum:** `dotnet build` oleh Rizki, perbandingan regresi sesudah build (AC-1), dan uji runtime tablet no. 1–13 (AC-2 s.d. AC-5) |

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
| `dotnet build` | Belum | `NOT RUN` | Milik Rizki |
| Regresi login sesudah build (AC-1) | Belum | `NOT RUN` | `node rekam-login.mjs sesudah` lalu `node bandingkan.mjs <baseline> <sesudah>` |
| Uji runtime tablet (AC-2 s.d. AC-5) | Belum | `NOT RUN` | `node uji-tablet.mjs <bedTerisi> <bedKosong>`, 13 cek |

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

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 regresi login perawat, SuperAdmin/Administrator, Kiosk, display antrean, TV, satu login gagal; satu-satunya beda klaim `is_nurse_view_bed_tablet_account = false` | Terpetakan ke source; **regresi belum dibandingkan** | Resolver tablet hanya menambah klaim `false` untuk akun non-tablet; tidak ada cabang lama yang berubah (§3.2). Baseline sudah direkam |
| AC-2 akun tablet `403` di `/display-runtime/current` dan `/board` | Terpetakan ke source; runtime belum | Policy `NurseViewDisplayRead` wajib `nurse_view_device_id`; token tablet tidak membawanya. Uji B no. 5 |
| AC-3 akun TV dan Administrator `403` di `/bed-tablet-runtime/current` | Terpetakan ke source; runtime belum | Policy `NurseViewBedTabletAccess` wajib `nurse_view_bed_tablet_id`, tanpa jalan pintas peran. Uji B no. 6 |
| AC-4 tablet dinonaktifkan → permintaan berikutnya `403`; login ulang sandi benar → pesan `AD-02` (`VAL-NV-27`) | Terpetakan ke source; runtime belum | `ResolveActiveTabletAsync` (status per permintaan); cabang terkunci `AuthController.Login`; `Refresh`. Uji B no. 11–13 |
| AC-5 `GET /current` bed berpenghuni → `IsBedOccupied = true` tanpa identitas; bed kosong → `false` | Terpetakan ke source; runtime belum | `GetCurrentAsync`: DTO tanpa field identitas; penempatan aktif `EndDateTime == null && !IsDelete`. Uji B no. 3–4 |
| DoD — regresi login lulus dan direkam | **Belum terpenuhi** | Menunggu build + bagian A |
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
| Risiko tersisa | **Regresi login seluruh sistem** sampai bagian A lulus (syarat lead). Token tablet berlaku 90 hari; mitigasinya pemeriksaan status dan stempel di setiap permintaan |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu perintah pembersihan folder sementara ditolak pengaman sesi; perintah itu tidak perlu dan tidak dijalankan ulang |
| Status Git | Perubahan task ini: `M Controllers/AuthController.cs`, `M Constants/AuthorizationPolicies.cs`, `M Program.cs`, `M Areas/HealthServices/NurseViewManagement/Services/NvwResult.cs`; `?? …/Controllers/NvwBedTabletRuntimeController.cs`, `?? …/DTOs/NvwBedTabletRuntimeDtos.cs`, `?? …/Services/NvwBedTabletContextService.cs`, `?? …/Services/NvwPatientCallQueryService.cs`; laporan ini dan baris status roadmap/traceability. Dokumen hasil uji `BE-NV-009`/`016` dari sesi yang sama juga masih `M` |
| Langkah berikutnya | Rizki build → agent menjalankan bagian A dan B → ✅ bila lulus. Sesudah itu `BE-NV-011` |
