# Laporan Perubahan Backend — `RJ-DOC-REV-BE-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-016` |
| Judul | Layar publik dan realtime |
| Slice | Amendment AQ — antrean prioritas, member, dan privasi layar publik |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `17` |
| Trace | `RJ-DOC-DEC-057`, `058`, `060` |
| Contract version | Delta aditif Queue Display Device, Queue Display Runtime, Nurse/Doctor Queue, payload realtime |
| Dependency | `RJ-DOC-REV-BE-015` ✅ |
| Klasifikasi | `MEDIUM` — satu repository 0, diperiksa 9–20 1, diubah >8 2, logika sedang 1, kontrak berubah 2, schema (kolom dalam migration `BE-015`) 2, privasi inti 2 = 10, dicatat `HEAVY` karena dampak privasi |
| Task mode | `CROSS-REPO MODE`, backend lebih dulu (`RJ-DOC-DEC-060`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7d5f354f` (`sukmagp`), belum di-commit |
| Tanggal | 2026-10-08 |
| Status | Selesai — build, QBE Strict, uji runtime gabungan `26/26 PASS` |

## 1. Masalah yang diperbaiki

API layar publik selalu mengirim `MaskedPatientName`, `MedicalRecordNumber`, `EncounterNumber`, dan `PatientId`,
dan suara panggilan menyebut nama pasien (`F-AQ-4`). Tidak ada cara memisah layar Regular dan Member.

## 2. Proses bisnis

1. Admin mengatur *Audience Display* per perangkat: `All` (bawaan, perilaku lama), `Regular Only`, `Member Only`.
2. Layar memanggil `queue-display-runtime/items`, `summary`, dan `called`. Backend menyaring antrean menurut snapshot `QueueAudienceSnapshot` sebelum data dikirim; layar tidak pernah menerima antrean audience lain.
3. Untuk tiap antrean, backend menerapkan `PublicDisplayModeSnapshot`:

| Mode | Nama | Nama samaran | No. RM | Nomor kunjungan / `PatientId` | Suara |
| --- | --- | --- | --- | --- | --- |
| `Default`, `FullName` | Mengikuti `ShowPatientName` perangkat | Ya | Ya | Ya | Seperti template |
| `MaskedName` | Kosong | Ya | Kosong | Ya | Kalimat bernama dibuang |
| `QueueNumberOnly` | Kosong | Kosong | Kosong | Kosong / `Guid.Empty` | Kalimat bernama dibuang |

Contoh suara pasien `QueueNumberOnly` yang dipanggil perawat: "Nomor antrian Poli Bedah nomor dua. Silakan menuju meja perawat. Terima kasih." — kalimat "Atas nama …" tidak diucapkan.

4. Ruang kerja perawat dan dokter (staf login) tetap menerima identitas lengkap, ditambah penanda internal `isMemberQueue` dan `queueAudience`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`QueueDisplayRuntimeController`, `QueueDisplayRuntimeDtos`, `QueueVoiceService`, `QueueRealtimeService`, `QueueRealtimeDtos`, `QueueDisplayDeviceController`/DTO, `NurseStationQueueController`/DTO, `DoctorQueueController`/DTO, `AuthController` (login display).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Administrator/MasterData/Enums/QueueDisplayAudienceMode.cs` | Enum baru |
| `Areas/Administrator/MasterData/Models/MstQueueDisplayDevice.cs` + configuration | Kolom `QueueAudienceMode` (bawaan `All`) |
| `Areas/Administrator/MasterData/DTOs/QueueDisplayDeviceDtos.cs`, `Controllers/QueueDisplayDeviceController.cs` | Field response/request (update kosong = nilai lama), validasi enum, `queueAudienceModeOptions`, metadata form |
| `Areas/HealthServices/RegistrationManagement/Controllers/QueueDisplayRuntimeController.cs`, `DTOS/QueueDisplayRuntimeDtos.cs` | Penyaringan audience, privasi per mode, `queueAudienceMode` pada `current` |
| `Services/QueueVoiceService.cs` | Kalimat template berisi `{patientName}`/`{medicalRecordNumber}` dibuang untuk `MaskedName`/`QueueNumberOnly` |
| `Services/QueueRealtimeService.cs`, `DTOS/QueueRealtimeDtos.cs` | `queueAudience` pada payload (non-sensitif) |
| `NurseStationQueueController`, `DoctorQueueController` + DTO | `isMemberQueue`, `queueAudience` pada item antrean |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif. Field lama tetap ada; untuk mode privasi nilainya dikosongkan, bukan dihapus |
| Database | Kolom `MstQueueDisplayDevice.QueueAudienceMode` dalam migration `BE-015` |
| Keamanan/Auth | Privasi ditegakkan backend. Atribut akses tidak berubah. Payload realtime tidak memuat nama atau data membership |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Queue Display Runtime

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/queue-display-runtime/current` | Info perangkat, kini dengan `queueAudienceMode` | Policy `QueueDisplayRuntimeRead` |
| `GET` | `.../queue-display-runtime/items` | Antrean tersaring audience dan privasi | Policy `QueueDisplayRuntimeRead` |
| `GET` | `.../queue-display-runtime/summary` | Ringkasan tersaring audience | Policy `QueueDisplayRuntimeRead` |
| `GET` | `.../queue-display-runtime/called` | Panggilan terakhir tersaring audience; suara tanpa nama bila dilarang | Policy `QueueDisplayRuntimeRead` |

#### Administrator / Master Data / Queue Display Device

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/administrator/master-data/queue-display-devices/filters/metadata` | `queueAudienceModeOptions` | `QueueDisplayDevice : Read` |
| `POST`/`PUT` | `/api/v1/administrator/master-data/queue-display-devices[/{id}]` | Menyimpan `queueAudienceMode` | `QueueDisplayDevice : Create`/`Update` |

## 5. Verifikasi

Build, EF, QBE, dan lingkungan runtime sama dengan [`RJ-DOC-REV-BE-015`](RJ-DOC-REV-BE-015.md) §5.
Tiga perangkat uji (`TV REGULAR`, `TV MEMBER`, `TV ALL`, `ShowPatientName=true`) login lewat `/api/v1/Auth/login`.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| R1 | `current` memuat mode perangkat | `queueAudienceMode=2` | `PASS` |
| T9a | Klasifikasi pasien rahasia | `QueueNumberOnly`, member, **bukan** prioritas | `PASS` |
| T9b | API publik untuk `QueueNumberOnly` | `patientName`, `maskedPatientName`, `medicalRecordNumber`, `encounterNumber` kosong; `patientId` `Guid.Empty`; nama dan No. RM tidak ada di body | `PASS` |
| T9c | Prioritas tidak otomatis rahasia | Antrean prioritas `Default` tetap bernama di TV ALL | `PASS` |
| T9d | Data membership tidak bocor | Tidak ada `membershipTier`/`memberNumber`/kode tier uji di body | `PASS` |
| T9e | Panggilan perawat pasien rahasia | `voiceText` "Nomor antrian Poli Bedah nomor dua. Silakan menuju meja perawat. Terima kasih." | `PASS` |
| T10 | TV Regular | Hanya `B004, B006, B008, B012, B013, B014, B016, B017` | `PASS` |
| T10b | TV Regular tidak menerima panggilan antrean Member | `called` kosong | `PASS` |
| T11 | TV Member | Hanya `B001, B003, B005, B007, B009, B010, B011, B015` | `PASS` |
| T15b | Perawat (staf login) tetap melihat identitas pasien rahasia | `patientName` terisi | `PASS` |
| T15c | Alur perawat existing (panggil, mulai, selesai skrining) lalu antrean dokter | `200/200/200`, item `isMemberQueue=true`, `queueAudience=2` | `PASS` |

Realtime: payload hanya ditambah `queueAudience`; grup dan event SignalR tidak berubah. Tidak diuji lewat
WebSocket (`NOT RUN`); dibuktikan lewat kode.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Skenario 9 — `QueueNumberOnly` tanpa nama/MRN/nomor member | Terpenuhi | T9b, T9d, T9e |
| Skenario 10 — Regular Display tanpa antrean Member | Terpenuhi | T10, T10b |
| Skenario 11 — Member Display tanpa antrean Regular | Terpenuhi | T11 |
| Skenario 15 — workspace perawat/dokter tetap berjalan | Terpenuhi | T15b, T15c |
| Penyaringan di backend, bukan React | Terpenuhi | `BuildDisplayQueueQuery`, `MapItemResponse` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Mode `Default` mempertahankan perilaku lama: `MaskedPatientName` dan No. RM tetap terkirim ke layar publik. Ini temuan privasi lama; ubah tier ke `MaskedName`/`QueueNumberOnly` bila perlu |
| Masalah yang diketahui | Payload realtime ke grup cluster masih memuat `PatientId` (GUID, perilaku lama) |
| Risiko tersisa | Template suara dari konfigurasi yang menaruh nama di luar kalimat ber-placeholder tidak tersaring; template bawaan aman |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Sama dengan `BE-015` |
| Langkah berikutnya | `RJ-DOC-REV-FE-016` |
