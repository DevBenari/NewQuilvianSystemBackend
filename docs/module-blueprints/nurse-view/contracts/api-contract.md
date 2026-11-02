# API Contract — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.0` · status `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-10-28 |
| `input_revision` | `00-interview-decisions.md` rev `2`; `02-backend-architecture.md` rev `1` |
| Kompatibilitas | Seluruh endpoint baru; tidak ada endpoint lama yang berubah perilaku |
| Traceability | `NV-DEC-004`–`026`, `NV-AC-001`–`022` |

Seluruh endpoint di dokumen ini **Rencana (belum tersedia)**. Pembungkus respons:
`ApiResponse<T>.Ok(data, pesan)` dan `ApiResponse<T>.Fail(kode, pesan)`.

Kolom **Hak akses** di sini adalah satu-satunya pemetaan endpoint ke hak akses di blueprint ini.

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

## 4. Realtime — `QueueHub` (`/hubs/queues`)

| Metode hub | Pemanggil | Kegunaan | Status |
|---|---|---|---|
| `JoinNurseViewFloor()` | Akun perangkat | Bergabung ke grup `nurse-view-floor:{FloorCode}` milik perangkat; tanpa parameter | **Rencana (belum tersedia)** |
| `LeaveNurseViewFloor()` | Akun perangkat | Keluar grup | **Rencana (belum tersedia)** |

| Event | Payload | Pengirim | Status |
|---|---|---|---|
| `NurseViewBoardChanged` | `{ "eventType": "VitalSignChanged", "bedId": "…", "floorCode": "3", "occurredAtUtc": "…" }` | `NvwRealtimeService`, dari `PatientVitalSignController` | **Rencana (belum tersedia)**, menunggu `NV-OQ-016` |

Penerima memuat ulang papan; payload tidak membawa data pasien maupun nilai TTV (`NV-AC-022`).
