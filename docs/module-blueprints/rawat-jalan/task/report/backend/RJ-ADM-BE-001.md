# RJ-ADM-BE-001 — Tiket Antrean Admisi dan Handoff ke Antrean Poli Internal

| Field | Nilai |
| --- | --- |
| Status | 🟡 Sebagian — source dan migration selesai ditulis; build dan verifikasi belum dijalankan |
| Tanggal | 2026-11-02 |
| Task mode | `BACKEND MODE` (instruksi pemilik), FE dikerjakan terpisah di `RJ-ADM-FE-001` |
| Branch / SHA | `sukmagp` @ `bc482d01` + working tree |
| Blueprint | Cetak Antrean Admisi Rawat Jalan v1.1, task T01–T05 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Registry | `Reg` = Registration, lifecycle `ACTIVE / LEGACY` — entity baru sah memakai `Reg*` |
| Keberlakuan | `NEW CODE` (tiket, service, controller); `TOUCHED LEGACY` (`PatientEncounterController`, `QueueHub`, `QueueRealtimeService`) |
| QBE berlaku | ENT-001, NAM-001/002, CFG-001, MOD-001, SVC-001, API-001, PERM-001, LOG-001, CODE-001..006, VAL-001, TXN-001, DTO-001, PAGE-001, DEL-001 |

## Keputusan desain

1. **Tiket admisi bukan `RegQueue`** (bagian 15): entity baru `RegAdmissionTicket` + `RegAdmissionTicketHistory`, karena tiket terbit sebelum `PatientId`/`EncounterId` ada.
2. **Antrean poli internal memakai mekanisme existing.** `CreateEncounterCoreAsync` tetap membentuk `RegQueue` lewat `OutpatientQueueNumberAllocator`; tiket hanya dikunci (`FOR UPDATE`) dan ditautkan di transaksi yang sama. Tidak ada allocator atau tabel antrean poli kedua.
3. **Nomor admisi** dari `NumberSeriesAllocator` (`SequenceKey = REG_ADMISSION_TICKET_<GROUP>`, reset `DAILY`), dibentuk ulang menjadi `A-015`. Unique `(ServiceDate, QueueGroupCode, SequenceNumber)`.
4. **Panggil Berikutnya** mengklaim tiket dengan `FOR UPDATE SKIP LOCKED`; loket yang masih memegang tiket Dipanggil/Dilayani ditolak 409.
5. **Kelompok dan loket** dari konfigurasi `HealthServices:Registration:AdmissionQueue` (bawaan satu kelompok `REGULER`/`A`, `IsBpjsServiceAvailable` menyembunyikan kelompok BPJS), divalidasi saat start.
6. **Display dan realtime** tanpa identitas pasien dan tanpa nomor poli internal.

## Delta kontrak

| Endpoint | Otorisasi |
| --- | --- |
| `GET kiosk-admission-tickets/config`, `POST kiosk-admission-tickets`, `POST {id}/reprint`, `POST {id}/print-result` | Policy `KioskRead` (pola `KioskPatientLookupController`) |
| `GET admission-queues/filters/metadata`, `/summary`, `/`, `/{id}` | `AdmissionQueue : Read` |
| `POST admission-queues/call-next`, `/{id}/recall` | `AdmissionQueue : Call` |
| `POST admission-queues/{id}/start-service`, `/skip`, `/complete` | `AdmissionQueue : Update` |
| `POST admission-queues/{id}/cancel` | `AdmissionQueue : Cancel` |
| `GET admission-queue-display` | Policy `QueueDisplayRuntimeRead` |
| `POST patient-encounters/admin` | Field baru opsional `admissionTicketId`; respons menambah `admissionTicketId`, `admissionTicketDisplayNumber`. Kiosk menolak field ini. 409 bila tiket tidak lagi Dilayani/sudah tertaut. |
| SignalR `/hubs/queues` | `JoinAdmissionQueueBoard` / `JoinAdmissionQueueDisplay`, event `AdmissionQueueChanged` |

Hak akses `AdmissionQueue` perlu dicentang admin di layar Akses Role.

## Berkas

Baru: `Enums/AdmissionTicketStatus.cs`, `Enums/AdmissionTicketPrintStatus.cs`, `Models/RegAdmissionTicket.cs`, `Models/RegAdmissionTicketHistory.cs`, `DTOS/AdmissionTicketDtos.cs`, `Services/AdmissionQueueOptions.cs`, `Services/AdmissionTicketService.cs`, `Controllers/AdmissionQueueController.cs`, `Controllers/KioskAdmissionTicketController.cs`, `Controllers/AdmissionQueueDisplayController.cs`, `Repositories/Configurations/HealthServices/RegAdmissionTicket(History)Configuration.cs`, `Migrations/20261102090000_AddRegAdmissionTicket(.Designer).cs`.

Diubah: `PatientEncounterController.cs`, `PatientEncounterDtos.cs`, `QueueRealtimeService.cs`, `Hubs/QueueHub.cs`, `ApplicationDbContext.cs`, `Program.cs`, `appsettings.json`, `ApplicationDbContextModelSnapshot.cs` (+280 baris, hanya tambahan).

## Migration dan database

Migration, Designer, dan snapshot **disusun tangan tanpa build**. Designer hanya memuat atribut `[DbContext]`/`[Migration]` (pola `MigrationMetadata.g.cs`); `BuildTargetModel` tidak ada. Diterapkan ke DB dev pemilik `QuilvianNewDevSukma` pada 2026-11-02; database bersama belum.

## Validasi

| Butir | Hasil |
| --- | --- |
| `dotnet build` | Dijalankan manual oleh pemilik, 2026-11-02 10:26 (`bin/Debug`) |
| Pembersihan DB dev | 2026-11-02, atas izin pemilik: 6 tabel sisa migration yatim `20261029034530_AddAdmissionQueueTicket` (tidak ada di source mana pun; 13 tiket uji) + kolom `MstKioskDevice.IsAllowAdmissionTicket` + baris riwayatnya dihapus. Penyebab 500 `column r.CalledAt does not exist` saat uji kiosk. |
| `ef database update` | PASS — `Applying migration '20261102090000_AddRegAdmissionTicket'. Done.` (`QuilvianNewDevSukma`) |
| `ef migrations has-pending-model-changes` | PASS — `No changes have been made to the model since the last migration.` Tabel 44 kolom, 7 index/PK sesuai konfigurasi |
| Uji API / UAT | **BELUM DIJALANKAN** |
| Review diff/scope | Dilakukan; tidak ada perubahan di luar Registration, hub antrean, Program, appsettings, migration |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## Risiko tersisa

1. Database lain yang pernah menerapkan migration yatim `20261029034530_AddAdmissionQueueTicket` perlu pembersihan yang sama sebelum `database update`.
2. Registrasi lewat rujukan Laboratorium memakai endpoint lain dan tidak menautkan tiket; petugas menutup tiket dengan catatan.
3. Fasilitas tunggal: unique nomor per tanggal + kelompok, tanpa `facility_id` (belum ada konsep multi-fasilitas di source).
4. Deteksi printer kosong sebelum terbit (bagian 12 baris 1) tidak mungkin dari browser; status cetak dikonfirmasi pasien di kiosk.

## Task berikutnya

Build → `has-pending-model-changes` → `database update` (dev pemilik) → bukti UAT-01..13 → ubah status ke ✅.

## Revisi 2026-11-02 — dukungan layout Daftar Antrean Admisi

- `POST admission-queues/{id}/call` (`AdmissionQueue : Call`): panggil satu tiket Menunggu ke loket; loket yang masih memegang nomor ditolak 409 (`ResolveCounterHeldErrorAsync`, dipakai bersama call-next).
- `complete` kini juga dari Dipanggil (catatan wajib bila belum tertaut kunjungan).
- `availableActions`: Menunggu `call, cancel`; Dipanggil `recall, skip, start-service, complete, cancel`.
- List dan summary menerima `serviceDateFrom/serviceDateTo` (inklusif); `serviceDate` tunggal tetap diterima. Summary menambah `serviceDateTo`. Urutan list: tanggal terbaru dulu, lalu nomor.
- Tanpa perubahan schema/migration. Validasi: **belum build** (instruksi pemilik).

## Revisi 2026-11-02 — data display

- `GET admission-queue-display` menambah `waitingTickets` (maks. 8: Menunggu urut terbit, lalu Dilewati) tanpa identitas pasien. Tanpa perubahan schema. Validasi: belum build.

## Revisi 2026-11-02 — master display untuk layout Admisi Rawat Jalan

- `QueueDisplayLayoutType.AdmissionBoard = 6` (label "Admisi Rawat Jalan"); opsi metadata dibangkitkan dari enum.
- `MstQueueDisplayDevice.NurseStationClusterId` menjadi nullable; validasi master: cluster wajib kecuali layout Admisi, dan dikosongkan untuk layout Admisi (`ResolveClusterId`). DTO master/login/runtime ikut `Guid?`.
- Redirect login perangkat (`AuthController` dan `QueueDisplayDeviceController.BuildQueueDisplayRedirectPath`) ke `/queue-display/admission` untuk layout Admisi; parameter cluster hanya dikirim bila ada.
- `queue-display-runtime/current` menambah `layoutType`; query poli berbasis cluster aman untuk perangkat tanpa cluster.
- Migration `20261102140000_MakeQueueDisplayDeviceClusterOptional` (AlterColumn nullable) + snapshot, disusun tangan. Validasi: **belum build**, belum `database update`.

## Revisi 2026-11-02 — `displayScope` pada master display

- `GET queue-display-devices` dan `GET queue-display-devices/summary` menerima `displayScope` = `service` (selain layout Admisi) atau `admission` (layout Admisi); kosong = semua. Tanpa perubahan schema. Validasi: belum build.

## Revisi 2026-11-02 — Terakhir Online display admisi

- `GET admission-queue-display` memperbarui `MstQueueDisplayDevice.LastOnlineDateTime` untuk akun perangkat (`queue_display_device_id`) ber-layout Admisi (`AdmissionTicketService.TouchDisplayDeviceAsync`, `ExecuteUpdate`). Tanpa schema. Validasi: belum build.
