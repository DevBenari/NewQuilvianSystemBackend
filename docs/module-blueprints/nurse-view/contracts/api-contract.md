# API Contract — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.2` · status `approved` — Rizki Gunawan, 2026-11-03 (`NV-DEC-033`). Sebelumnya `0.1.1`, 2026-11-02 (`NV-DEC-031`); `0.1.0`, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-10-28 |
| `input_revision` | `00-interview-decisions.md` rev `2`; `02-backend-architecture.md` rev `1` |
| Kompatibilitas | Seluruh endpoint baru; tidak ada endpoint lama yang berubah perilaku |
| Traceability | `NV-DEC-004`–`026`, `NV-AC-001`–`022` |

Seluruh endpoint di dokumen ini **Rencana (belum tersedia)**. Pembungkus respons:
`ApiResponse<T>.Ok(data, pesan)` dan `ApiResponse<T>.Fail(kode, pesan)`.

Kolom **Hak akses** di sini adalah satu-satunya pemetaan endpoint ke hak akses di blueprint ini.

> **Revision 2 (`0.2.0` `approved`, `NV-DEC-055`) — 2026-11-05.** Endpoint tablet bed dan panggilan pasien ada di
> [Revision 2](#revision-2--tablet-bed-dan-panggilan-pasien). Bagian 1–4 tetap `0.1.2`.

## 1. Health Services / Nurse View Management / Display Device

Base URL: `api/v1/health-services/nurse-view-management/display-devices`
Pengguna: Admin IT (akun manusia).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar perangkat, bisa disaring lantai, status, dan kata kunci | `NurseViewDisplayDevice : Read` | query `NvwDisplayDevicePagedQuery` (`floorCode`, `status`, `search`, `page`, `pageSize`) | `PagedResult<NvwDisplayDeviceResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/floors` | Daftar lantai yang dikenal sistem (dari `FloorCode` kamar aktif) untuk pilihan form | `NurseViewDisplayDevice : Read` | — | `List<NvwFloorOptionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail satu perangkat | `NurseViewDisplayDevice : Read` | — | `NvwDisplayDeviceDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mendaftarkan perangkat baru; kode diterbitkan sistem | `NurseViewDisplayDevice : Create` | `NvwDisplayDeviceCreateRequest` | `NvwDisplayDeviceDetailResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah nama, lantai, lokasi, keterangan | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceUpdateRequest` | `NvwDisplayDeviceDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/login` | Membuat login perangkat; kata sandi tampil sekali | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceLoginRequest` (`version`) | `NvwDisplayDeviceLoginResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/login/reset` | Mengatur ulang login; juga mengaktifkan kembali perangkat `Inactive` | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceLoginRequest` (`version`) | `NvwDisplayDeviceLoginResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan perangkat; login dikunci seketika | `NurseViewDisplayDevice : Update` | `NvwDisplayDeviceDeactivateRequest` (`reason`, `version`) | `NvwDisplayDeviceDetailResponse` | **Rencana (belum tersedia)** |

### DTO

| Class | Jenis | Field |
|---|---|---|
| `NvwDisplayDeviceCreateRequest` | Create | `DeviceName` (wajib, ≤100), `FloorCode` (wajib, ≤20), `LocationName` (≤150), `Description` (≤500) |
| `NvwDisplayDeviceUpdateRequest` | Update | sama dengan Create + `Version` (wajib) |
| `NvwDisplayDeviceLoginRequest` | Status | `Version` (wajib) |
| `NvwDisplayDeviceDeactivateRequest` | Status | `Reason` (wajib, 5–500), `Version` (wajib) |
| `NvwDisplayDeviceResponse` | Response | `Id`, `DeviceCode`, `DeviceName`, `FloorCode`, `LocationName`, `DeviceStatus`, `DeviceStatusLabel`, `HasLogin`, `LastOnlineAt`, `IsOnline` (`LastOnlineAt` ≤ 3 menit lalu) |
| `NvwDisplayDeviceDetailResponse` | Response | Response + `Description`, `LoginUserName`, `LoginIssuedAt`, `LoginIssuedByName`, `DeactivatedAt`, `DeactivationReason`, `Version`, `AvailableActions` |
| `NvwDisplayDeviceLoginResponse` | Response | `DeviceId`, `DeviceCode`, `LoginUserName`, `Password` (**hanya di sini, sekali**), `Version` |
| `NvwFloorOptionResponse` | Option | `FloorCode`, `FloorName`, `RoomCount`, `BedCount` |

**Kode status.** `200` berhasil. `400` isian tidak lengkap atau formatnya salah. `403` tidak punya
hak akses. `404` perangkat tidak ditemukan. `409` tindakan tidak sah untuk status perangkat saat
ini, atau data sudah diubah orang lain (`Version` tidak cocok). `422` lantai tidak dikenal.

**Contoh.** Admin membuat login untuk `NVW-000001`. Response:
`{ "deviceCode": "NVW-000001", "loginUserName": "nvw-000001", "password": "Xk7#p2Qm9v", "version": 2 }`.
Membuka `GET /{id}` sesudahnya tidak pernah memuat `password` (`NV-AC-018`).

## 2. Health Services / Nurse View Management / Vital Sign Interval

Base URL: `api/v1/health-services/nurse-view-management/vital-sign-intervals`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Himpunan interval aktif beserta rujukan pengesahannya | `NurseViewVitalSignInterval : Read` | — | `NvwVitalSignIntervalSetResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Mengganti seluruh himpunan (ganti-seluruh) | `NurseViewVitalSignInterval : Update` | `NvwVitalSignIntervalReplaceRequest` | `NvwVitalSignIntervalSetResponse` | **Rencana (belum tersedia)** |

| Class | Jenis | Field |
|---|---|---|
| `NvwVitalSignIntervalReplaceRequest` | Update | `Items[]` (`EwsRiskLevel`, `IntervalMinutes`), `RatificationReference` (wajib, ≤200), `RatifiedOn` (wajib, tidak di masa depan) |
| `NvwVitalSignIntervalSetResponse` | Response | `Items[]` (`EwsRiskLevel`, `EwsRiskLevelLabel`, `IntervalMinutes`), `RatificationReference`, `RatifiedOn`, `RecordedByName`, `RecordedAt`, `IsConfigured` (`false` bila kosong) |

`Items` boleh berisi sebagian tingkat; tingkat yang tidak ada berarti tanpa tanda untuk tingkat
itu. `Items` kosong berarti mencabut seluruh interval (kembali aman-gagal), dan tetap wajib
menyertakan rujukan pengesahan pencabutannya.

**Contoh body.**
`{ "items": [ { "ewsRiskLevel": 1, "intervalMinutes": 720 }, { "ewsRiskLevel": 2, "intervalMinutes": 240 } ], "ratificationReference": "SK-KOMKEP/012/X/2026", "ratifiedOn": "2026-10-20" }`

**Kode status.** `200` berhasil. `400` tingkat ganda, tingkat `Unknown`, menit di luar 1–1440,
rujukan kosong, atau tanggal di masa depan. `403` tidak punya hak akses.

## 3. Health Services / Nurse View Management / Display Runtime

Base URL: `api/v1/health-services/nurse-view-management/display-runtime`
Pengguna: **akun perangkat**, policy `NurseViewDisplayRead`, tanpa `[AccessPermission]`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/current` | Identitas perangkat yang login dan pengaturan muat ulang | policy `NurseViewDisplayRead` | — | `NvwDisplayRuntimeCurrentResponse` | **Rencana (belum tersedia)** |
| `GET` | `/board` | Papan lantai milik perangkat | policy `NurseViewDisplayRead` | — (**tidak ada** parameter lantai) | `NvwFloorBoardResponse` | **Rencana (belum tersedia)** |

### DTO papan

| Class | Field |
|---|---|
| `NvwDisplayRuntimeCurrentResponse` | `DeviceCode`, `DeviceName`, `FloorCode`, `FloorName`, `RefreshIntervalSeconds` (= 60), `StaleAfterSeconds` (= 120), `ServerTimeUtc` |
| `NvwFloorBoardResponse` | `FloorCode`, `FloorName`, `AsOfUtc`, `IntervalsConfigured`, `Wards[]` |
| `NvwWardBoardResponse` | `ServiceUnitId`, `ServiceUnitName`, `RosterAvailable`, `OnDutyNurses[]` (`DisplayName`, `ShiftLabel`), `Beds[]` |
| `NvwBedCardResponse` | `BedId`, `BedCode`, `BedName`, `RoomName`, `StatusCode`, `StatusLabel`, `IsNewbornBed`, `Patient?`, `ResponsibleNurseName?`, `HasResponsibleNurse`, `VitalSign?`, `IsCritical`, `HasHighFallRisk`, `HasActiveAllergy`, `RequiresIsolation`, `IsolationMismatch` |
| `NvwMaskedPatientResponse` | `Initials`, `GenderCode`, `AgeLabel` |
| `NvwVitalSignCardResponse` | `ObservedAtUtc`, `AgeMinutes`, `EarlyWarningScore?`, `EwsRiskLevel`, `IsCritical`, `Systolic?`, `Diastolic?`, `PulseRate?`, `RespiratoryRate?`, `Temperature?`, `OxygenSaturation?`, `IsOverdue`, `OverdueMinutes?` |

`StatusLabel` papan: `Occupied` "Terisi", `Reserved` "Dipesan — pasien akan datang",
`WaitingCleaning`/`Cleaning` "Dibersihkan", `Available` "Kosong", `Unavailable` "Tidak tersedia".

**Tidak pernah ada di response:** `FullName`, `MedicalRecordNumber`, `BirthDate`, nama alergen,
DPJP, diagnosis, `EpisodeId`, `PatientId`.

**Contoh satu kartu.**
`{ "bedCode": "301-A", "statusCode": "Occupied", "patient": { "initials": "B.S.", "genderCode": "L", "ageLabel": "64 th" }, "responsibleNurseName": "Sari", "vitalSign": { "earlyWarningScore": 6, "ewsRiskLevel": "Medium", "ageMinutes": 270, "isOverdue": true, "overdueMinutes": 30 }, "hasHighFallRisk": true, "hasActiveAllergy": true, "requiresIsolation": false, "isolationMismatch": false }`

**Kode status.** `200` berhasil. `401` belum login atau token kedaluwarsa. `403` akun bukan akun
perangkat Nurse View, atau perangkat `Inactive` — TV kembali ke layar login.

**Catatan `0.1.1`.** Akun tanpa klaim perangkat ditolak policy dengan `403` tanpa isi, sama seperti
`QueueDisplayRuntimeController`. Pesan `VAL-NV-14` hanya keluar bila klaim ada tetapi perangkatnya
tidak lagi ditemukan. Token yang terbit sebelum Atur Ulang Login terakhir ditolak `403` kode
`NV-SESSION-STALE` ("Sesi perangkat sudah tidak berlaku. Silakan login ulang.") dan `401` pada
`POST /auth/refresh`, memenuhi `state-transition-matrix` "sesi lama tidak berlaku".

**Catatan `0.1.2`.** `RosterAvailable = true` hanya bila `OnDutyNurses` berisi minimal satu perawat/bidan
jaga; bangsal tanpa `OrganizationUnitId`, roster kosong, atau roster tanpa perawat/bidan menjawab `false`
dengan daftar kosong. `ShiftLabel` berisi nama shift, atau jam "07.00–14.00" bila nama kosong. Bentuk
field tidak berubah (`NV-DEC-033`).

## 4. Realtime — `QueueHub` (`/hubs/queues`)

| Metode hub | Pemanggil | Kegunaan | Status |
|---|---|---|---|
| `JoinNurseViewFloor()` | Akun perangkat | Bergabung ke grup `nurse-view-floor:{FloorCode}` milik perangkat; tanpa parameter | **Rencana (belum tersedia)** |
| `LeaveNurseViewFloor()` | Akun perangkat | Keluar grup | **Rencana (belum tersedia)** |

| Event | Payload | Pengirim | Status |
|---|---|---|---|
| `NurseViewBoardChanged` | `{ "eventType": "VitalSignChanged", "bedId": "…", "floorCode": "3", "occurredAtUtc": "…" }` | `NvwRealtimeService`, dari `PatientVitalSignController` | **Rencana (belum tersedia)**, menunggu `NV-OQ-016` |

Penerima memuat ulang papan; payload tidak membawa data pasien maupun nilai TTV (`NV-AC-022`).

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.2.0` · status `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-11-05 |
| `input_revision` | `00-interview-decisions.md` rev `5`; `02-backend-architecture.md` Revision 2 |
| Kompatibilitas | Menambah endpoint dan satu field opsional (`ActiveCallPriority` pada `NvwBedCardResponse`). Tidak ada endpoint `0.1.2` yang berubah perilaku |
| Traceability | `NV-DEC-034`–`054`; `NV-AC-028`–`045` |

Seluruh endpoint di bagian ini **Rencana (belum tersedia)**. Kode pesan ada di
`contracts/validation-matrix.md` R2. Waktu dikirim UTC (`…Utc`/`…At`), lama menunggu dalam detik
dihitung server pada saat response dibuat.

### 5. Health Services / Nurse View Management / Bed Tablet

Base URL: `api/v1/health-services/nurse-view-management/bed-tablets`
Contract version: `0.2.0` — status `approved` (`NV-DEC-055`). Pengguna: Admin IT.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar tablet, disaring lantai, status, kata kunci | `NurseViewBedTablet : Read` | query `NvwBedTabletPagedQuery` (`floorCode`, `status`, `search`, `page`, `pageSize`) | `PagedResult<NvwBedTabletResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/floors` | Pilihan lantai | `NurseViewBedTablet : Read` | — | `List<NvwFloorOptionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/bed-options` | Pilihan bed satu lantai, beserta tanda sudah punya tablet | `NurseViewBedTablet : Read` | query `floorCode` (wajib) | `List<NvwBedOptionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian tablet | `NurseViewBedTablet : Read` | — | `NvwBedTabletDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Daftarkan tablet untuk satu bed; kode dari sistem | `NurseViewBedTablet : Create` | `NvwBedTabletCreateRequest` | `NvwBedTabletDetailResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah nama, bed, lokasi, keterangan | `NurseViewBedTablet : Update` | `NvwBedTabletUpdateRequest` | `NvwBedTabletDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/login` | Buat login; kata sandi tampil sekali | `NurseViewBedTablet : Update` | `NvwBedTabletLoginRequest` (`version`) | `NvwBedTabletLoginResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/login/reset` | Atur ulang login; juga mengaktifkan kembali tablet `Inactive` | `NurseViewBedTablet : Update` | `NvwBedTabletLoginRequest` (`version`) | `NvwBedTabletLoginResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/deactivate` | Nonaktifkan; login dikunci seketika | `NurseViewBedTablet : Update` | `NvwBedTabletDeactivateRequest` (`reason`, `version`) | `NvwBedTabletDetailResponse` | **Rencana (belum tersedia)** |

| Class | Jenis | Field |
|---|---|---|
| `NvwBedTabletCreateRequest` | Create | `DeviceName` (wajib, ≤100), `BedId` (wajib), `LocationNote` (≤150), `Description` (≤500) |
| `NvwBedTabletUpdateRequest` | Update | sama dengan Create + `Version` (wajib) |
| `NvwBedTabletLoginRequest` | Status | `Version` (wajib) |
| `NvwBedTabletDeactivateRequest` | Status | `Reason` (wajib, 5–500), `Version` (wajib) |
| `NvwBedTabletResponse` | Response | `Id`, `DeviceCode`, `DeviceName`, `BedId`, `BedCode`, `BedName`, `RoomName`, `FloorCode`, `DeviceStatus`, `DeviceStatusLabel`, `HasLogin`, `LastOnlineAt`, `IsOnline` (`LastOnlineAt` ≤ 3 menit lalu) |
| `NvwBedTabletDetailResponse` | Response | Response + `LocationNote`, `Description`, `LoginUserName`, `LoginIssuedAt`, `LoginIssuedByName`, `DeactivatedAt`, `DeactivationReason`, `HasActiveCall`, `Version`, `AvailableActions` |
| `NvwBedTabletLoginResponse` | Response | `TabletId`, `DeviceCode`, `LoginUserName`, `Password` (**hanya di sini, sekali**), `Version` |
| `NvwBedOptionResponse` | Option | `BedId`, `BedCode`, `BedName`, `RoomName`, `FloorCode`, `HasTabletInUse` |

**Kode status.** `200` berhasil. `400` isian salah (`VAL-NV-17`, `24`). `403` tidak punya hak
akses. `404` tablet tidak ditemukan. `409` bed sudah punya tablet (`VAL-NV-19`), pindah bed
saat ada panggilan aktif (`VAL-NV-20`), versi tidak cocok (`VAL-NV-21`), atau tindakan login
tidak sah (`VAL-NV-22`, `23`, `25`). `422` bed tidak sah (`VAL-NV-18`).

**Contoh.** Admin mendaftarkan "Tablet Bed 305-A" untuk bed 305-A, lalu membuat login. Response:
`{ "deviceCode": "NVT-000001", "loginUserName": "nvt-000001", "password": "Rq4#mZ8tLw", "version": 2 }`.

### 6. Health Services / Nurse View Management / Bed Tablet Runtime

Base URL: `api/v1/health-services/nurse-view-management/bed-tablet-runtime`
Contract version: `0.2.0` — status `approved` (`NV-DEC-055`). Pengguna: **akun tablet bed**, policy
`NurseViewBedTabletAccess`, tanpa `[AccessPermission]`. Bed selalu dari tablet yang login.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/current` | Bed tablet, berpenghuni atau tidak, dan panggilan aktifnya | policy `NurseViewBedTabletAccess` | — | `NvwBedTabletRuntimeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/calls` | Tekan "Panggil perawat" atau "Darurat": membuat panggilan, menambah hitungan tekan, atau menaikkan menjadi Darurat | policy `NurseViewBedTabletAccess` | header `Idempotency-Key` (UUID, wajib); body `NvwTabletCallRequest` (`priority`) | `NvwTabletCallResponse` | **Rencana (belum tersedia)** |
| `POST` | `/calls/current/cancel` | Pasien membatalkan panggilan biasa yang masih menunggu | policy `NurseViewBedTabletAccess` | header `Idempotency-Key`; body `{ "version": n }` | `NvwTabletCallResponse` | **Rencana (belum tersedia)** |
| `POST` | `/calls/current/nurse-arrived` | Perawat menahan "Perawat hadir" 3 detik; panggilan selesai di bed | policy `NurseViewBedTabletAccess` | header `Idempotency-Key`; body `{ "version": n }` | `NvwTabletCallResponse` | **Rencana (belum tersedia)** |

| Class | Field |
|---|---|
| `NvwBedTabletRuntimeResponse` | `DeviceCode`, `BedCode`, `BedName`, `RoomName`, `FloorCode`, `IsBedOccupied`, `ActiveCall?` (`NvwTabletCallResponse`), `RefreshIntervalSeconds`, `ServerTimeUtc` |
| `NvwTabletCallRequest` | `Priority` (`1` Panggilan, `2` Darurat) |
| `NvwTabletCallResponse` | `CallId`, `Priority`, `PriorityLabel`, `CallStatus`, `StatusLabel`, `FirstPressedAt`, `WaitingSeconds`, `PressCount`, `ClaimedByName?`, `CanCancel`, `CanEscalate`, `CanConfirmNurseArrived`, `IsActive`, `Version` |

**Tidak pernah ada di response tablet:** nama, inisial, No. RM, umur pasien (`G-09`).

Aturan `POST /calls`:

| Keadaan bed | Prioritas dikirim | Hasil |
|---|---|---|
| Tidak berpenghuni, tanpa panggilan aktif | apa pun | `409` `VAL-NV-28`; layar tablet sudah menyembunyikan kedua tombol |
| Tidak berpenghuni, masih ada panggilan aktif (`NV-DEC-050`) | apa pun | `409` `VAL-NV-28`; panggilan lama tetap menunggu perawat |
| Berpenghuni, tanpa panggilan aktif | `1` atau `2` | Panggilan baru `Waiting`, jejak `Pressed` |
| Ada panggilan aktif `Normal` | `1` | `PressCount` + 1, jejak `PressedAgain`; jam awal tetap |
| Ada panggilan aktif `Normal` | `2` | Naik menjadi `Emergency`, `EscalatedAt` terisi, jejak `EscalatedToEmergency`; jam awal tetap |
| Ada panggilan aktif `Emergency` | `1` atau `2` | `PressCount` + 1, jejak `PressedAgain`; tetap Darurat (`G-13`) |
| `Idempotency-Key` sudah pernah dipakai | — | Keadaan saat ini dikembalikan, tanpa jejak baru |

**Contoh body dan response.** Body `{ "priority": 2 }`. Response
`{ "callId": "…", "priority": 2, "priorityLabel": "Darurat", "callStatus": 1, "statusLabel": "Menunggu", "waitingSeconds": 0, "pressCount": 1, "claimedByName": null, "canCancel": false, "canEscalate": false, "canConfirmNurseArrived": true, "isActive": true, "version": 1 }`.

**Kode status.** `200` berhasil. `400` `Idempotency-Key` kosong atau bukan UUID (`VAL-NV-29`),
prioritas tidak dikenal (`VAL-NV-30`). `401` belum login. `403` bukan akun tablet aktif, atau
sesi sudah diganti (`VAL-NV-26`, `NV-SESSION-STALE`). `404` tidak ada panggilan aktif untuk
dibatalkan atau dikonfirmasi (`VAL-NV-32`). `409` bed kosong (`VAL-NV-28`), batal tidak
diizinkan (`VAL-NV-31`), panggilan sudah selesai (`VAL-NV-37`), atau versi berubah (`VAL-NV-38`).

### 7. Health Services / Nurse View Management / Patient Call

Base URL: `api/v1/health-services/nurse-view-management/patient-calls`
Contract version: `0.2.0` — status `approved` (`NV-DEC-055`). Pengguna: perawat dan kepala ruang lewat Konsol
Panggilan, dengan akun sendiri.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/floors` | Pilihan lantai untuk konsol | `NurseViewPatientCall : Read` | — | `List<NvwFloorOptionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/active` | Panggilan aktif satu lantai, berurutan | `NurseViewPatientCall : Read` | query `floorCode` (wajib) | `NvwConsoleCallListResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/claim` | "Saya tangani" | `NurseViewPatientCall : Update` | `{ "version": n }` | `NvwConsoleCallResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/take-over` | "Ambil alih" dari pengklaim lain | `NurseViewPatientCall : Update` | `{ "version": n }` | `NvwConsoleCallResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/release` | "Lepas" oleh pengklaim | `NurseViewPatientCall : Update` | `{ "version": n }` | `NvwConsoleCallResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/close` | "Tutup panggilan" dengan alasan wajib | `NurseViewPatientCall : Update` | `NvwConsoleCloseRequest` | `NvwConsoleCallResponse` | **Rencana (belum tersedia)** |

| Class | Field |
|---|---|
| `NvwConsoleCallListResponse` | `FloorCode`, `FloorName`, `ThresholdsConfigured`, `Items[]`, `ServerTimeUtc` |
| `NvwConsoleCallResponse` | `CallId`, `BedId`, `BedCode`, `BedName`, `RoomName`, `Patient?` (`NvwMaskedPatientResponse`: `Initials`, `GenderCode`, `AgeLabel`), `Priority`, `PriorityLabel`, `CallStatus`, `StatusLabel`, `FirstPressedAt`, `WaitingSeconds`, `PressCount`, `ClaimedByUserId?`, `ClaimedByName?`, `ClaimedAt?`, `IsClaimedByMe`, `WaitLevel` (`None`/`Warning`/`Critical`), `AvailableActions[]` (`Claim`, `TakeOver`, `Release`, `Close`), `Version` |
| `NvwConsoleCloseRequest` | `Reason` (`2` Tablet tidak dapat dipakai, `3` Sudah ditangani — lupa konfirmasi di tablet, `4` Lainnya), `Note` (wajib 5–250 bila `Reason = 4`), `Version` (wajib) |

Urutan `Items`: `Emergency` dulu, lalu `FirstPressedAt` paling awal (`NV-DEC-038`). Panggilan
yang sudah selesai tidak ikut.

**Contoh item.**
`{ "bedCode": "331-C", "patient": { "initials": "R.A.", "genderCode": "P", "ageLabel": "72 th" }, "priorityLabel": "Darurat", "statusLabel": "Menunggu", "waitingSeconds": 20, "pressCount": 1, "claimedByName": null, "waitLevel": "None", "availableActions": ["Claim", "Close"], "version": 1 }`

**Kode status.** `200` berhasil. `400` alasan atau keterangan tidak sah (`VAL-NV-36`). `403`
tidak punya hak akses, atau melepas klaim milik orang lain (`VAL-NV-35`). `404` panggilan tidak
ditemukan. `409` klaim pada panggilan yang sudah diklaim (`VAL-NV-33`), ambil alih yang tidak
sah (`VAL-NV-34`), panggilan sudah selesai (`VAL-NV-37`), atau versi berubah (`VAL-NV-38`).
`422` lantai tidak dikenal (`VAL-NV-39`).

#### 7a. Peta lantai untuk Monitor perawat — kontrak `0.2.2` (`NV-DEC-058`, tambahan)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/floor-board` | Papan satu lantai untuk akun perawat beserta ringkasan semua lantai | `NurseViewPatientCall : Read` | query `floorCode` (wajib) | `NvwNurseMonitorResponse` | **Rencana (`BE-NV-017`)** |

| Class | Field |
|---|---|
| `NvwNurseMonitorResponse` | `Board` (`NvwFloorBoardResponse`, sama persis dengan `GET /display-runtime/board`), `Floors[]`, `ServerTimeUtc` |
| `NvwNurseMonitorFloorResponse` | `FloorCode`, `FloorName`, `WardNames[]` (nama unit layanan kamar di lantai itu), `RoomCount`, `BedCount`, `WaitingCallCount`, `ClaimedCallCount` |

`Board` memakai query papan TV apa adanya, jadi identitas pasien tetap inisial · jenis kelamin · umur
(`NV-DEC-044`, `NV-DEC-008`), disamarkan di backend. `Floors` berurutan seperti `GET /floors`.
Panggilan dan tombol aksi tetap dari `GET /active` dan endpoint aksi di atas; endpoint ini hanya baca.
**Kode status.** `200` berhasil. `403` tidak punya hak akses. `422` lantai tidak dikenal (`VAL-NV-39`).

### 8. Health Services / Nurse View Management / Patient Call Wait Threshold

Base URL: `api/v1/health-services/nurse-view-management/patient-call-wait-thresholds`
Contract version: `0.2.0` — status `approved` (`NV-DEC-055`). Pengguna: Admin IT.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Ambang aktif beserta rujukan pengesahannya | `NurseViewPatientCallWaitThreshold : Read` | — | `NvwPatientCallWaitThresholdResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Ganti-seluruh ambang, atau mencabutnya | `NurseViewPatientCallWaitThreshold : Update` | `NvwPatientCallWaitThresholdReplaceRequest` | `NvwPatientCallWaitThresholdResponse` | **Rencana (belum tersedia)** |

| Class | Field |
|---|---|
| `NvwPatientCallWaitThresholdReplaceRequest` | `WarningAfterMinutes?`, `CriticalAfterMinutes?` (keduanya kosong = cabut, kembali tanpa warna), `RatificationReference` (wajib, ≤200), `RatifiedOn` (wajib, tidak di masa depan) |
| `NvwPatientCallWaitThresholdResponse` | `IsConfigured`, `WarningAfterMinutes?`, `CriticalAfterMinutes?`, `RatificationReference?`, `RatifiedOn?`, `RecordedByName?`, `RecordedAt?` |

**Kode status.** `200` berhasil. `400` angka di luar batas atau urutan salah, rujukan kosong,
tanggal di masa depan (`VAL-NV-40`). `403` tidak punya hak akses.

### 9. Health Services / Nurse View Management / Display Runtime — tambahan

Base URL tetap `api/v1/health-services/nurse-view-management/display-runtime`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/active-calls` | Strip Panggilan Aktif lantai milik TV | policy `NurseViewDisplayRead` | — (tanpa parameter lantai) | `NvwDisplayCallStripResponse` | **Rencana (belum tersedia)** |
| `GET` | `/board` | Tetap; `NvwBedCardResponse` ditambah `ActiveCallPriority?` (`1`/`2`, kosong bila tidak memanggil) | policy `NurseViewDisplayRead` | — | `NvwFloorBoardResponse` | Sudah ada; field **Rencana (belum tersedia)** |

| Class | Field |
|---|---|
| `NvwDisplayCallStripResponse` | `FloorCode`, `ThresholdsConfigured`, `TotalActive`, `Items[]` (`BedCode`, `BedName`, `RoomName`, `Priority`, `PriorityLabel`, `CallStatus`, `FirstPressedAt`, `WaitingSeconds`, `ClaimedByName?`, `WaitLevel`), `ServerTimeUtc` |

**Tidak pernah ada di strip TV:** inisial, nama, No. RM, umur pasien (`NV-DEC-044`). Akun tablet
ditolak `403` karena tidak membawa klaim `nurse_view_device_id`.

### 10. Realtime — `QueueHub` (`/hubs/queues`) — tambahan

| Metode hub | Pemanggil | Kegunaan | Status |
|---|---|---|---|
| `JoinNurseViewCallConsole(string floorCode)` | Akun manusia dengan `NurseViewPatientCall : Read` | Grup `nurse-view-call-console:{FloorCode}`. Akun display antrean, TV Nurse View, dan tablet ditolak `HubException` (`VAL-NV-41`) | **Rencana (belum tersedia)** |
| `LeaveNurseViewCallConsole(string floorCode)` | Idem | Keluar grup | **Rencana (belum tersedia)** |
| `JoinNurseViewBedTablet()` | Akun tablet aktif | Grup `nurse-view-bed-tablet:{TabletId}`, tanpa parameter | **Rencana (belum tersedia)** |
| `LeaveNurseViewBedTablet()` | Idem | Keluar grup | **Rencana (belum tersedia)** |

| Event | Payload | Dikirim ke | Status |
|---|---|---|---|
| `NurseViewPatientCallChanged` | `{ "callId": "…", "bedId": "…", "floorCode": "3", "changeType": "Created", "occurredAtUtc": "…" }`; `changeType` salah satu `Created`, `PressedAgain`, `Escalated`, `Claimed`, `TakenOver`, `Released`, `Handled`, `Cancelled`, `ClosedBySystem` | `nurse-view-floor:{FloorCode}` (TV), `nurse-view-call-console:{FloorCode}`, `nurse-view-bed-tablet:{TabletId}` | **Rencana (belum tersedia)** |

Penerima memuat ulang bagiannya sendiri: TV memuat strip, konsol memuat daftar, tablet memuat
`GET /current`. Payload tidak membawa identitas pasien maupun nama perawat.
