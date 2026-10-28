# Laporan Pelaksanaan Task Backend: BE-OPR-012

| Metadata | Nilai |
| :--- | :--- |
| **Task ID** | `BE-OPR-012` |
| **Nama Fitur / Komponen** | Engine Timeline Jadwal Ruang OK & Buffer Sterilisasi Otomatis (30–45 Menit) |
| **Modul** | Modul Operasi (Instalasi Bedah Sentral / IBS) |
| **Status** | **Selesai (✅)** |
| **Tanggal Eksekusi** | 24 Oktober 2026 |
| **Keputusan Terkait** | `OPS-DEC-038`, `OPS-DEC-039` |
| **Kontrak Terkunci** | `opr-api-v1`, `opr-state-v1`, `05-desain-amandemen-operasi-dan-admisi.md` Bagian 4.1 |

---

## 1. Ringkasan Pekerjaan

Sesuai dengan standar operasional rumah sakit dan amandemen `OPS-DEC-038` serta `OPS-DEC-039`, telah dibangun engine backend untuk visualisasi jadwal ruang operasi (IBS) berbasis slot grid harian beserta penegakan jeda dekontaminasi dan sterilisasi ruangan:
1. **Endpoint Timeline Ruangan**: Menyediakan data slot waktu per kamar bedah pada tanggal yang diminta dengan klasifikasi slot `Occupied`, `SterilizationBuffer`, dan `Available`.
2. **Buffer Sterilisasi Otomatis (45 Menit)**: Pasca operasi selesai, sistem secara otomatis menambahkan slot jeda pembersihan dan dekontaminasi ruangan selama 45 menit (menggantikan default 30 menit).
3. **Pencegahan Benturan Jadwal (Anti-Collision Guard)**: Sistem menolak secara ketat setiap usaha pemesanan jadwal baru yang tumpang tindih dengan durasi bedah maupun jendela buffer sterilisasi kamar dengan melemparkan `OperatingRoomConflictException` (`409 Conflict`, kode `OPR003`).
4. **Proteksi Hak Akses**: Endpoint dilindungi dengan atribut otorisasi `[Authorize]`, `[AccessAction("Read", ...)]`, dan `[AccessPermission("OperatingRoomSchedule", "Read")]`.

---

## 2. Rincian Berkas yang Ditambahkan / Dimodifikasi

1. **DTO Baru**:
   - [`OperatingRoomTimelineDtos.cs`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/DTOs/OperatingRoomTimelineDtos.cs)
     - `OprRoomTimelineResponse`: Memuat `RoomId`, `RoomName`, `Floor`, dan daftar `Slots`.
     - `OprTimelineSlotResponse`: Memuat `SlotType` (`Occupied`, `SterilizationBuffer`, `Available`), detail pasien, nomor booking, nama tindakan, nama operator & anestesi, durasi sterilisasi, serta rentang waktu `StartAt` dan `EndAt` (`DateTimeOffset` dengan offset WIB UTC+7).

2. **Opsi Konfigurasi**:
   - [`OperatingRoomSchedulingOptions.cs`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Options/OperatingRoomSchedulingOptions.cs)
     - Menyesuaikan `DefaultBufferAfterMinutes = 45` sesuai ketetapan `OPS-DEC-039`.

3. **Service Logic**:
   - [`OperatingRoomSchedulingService.cs`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomSchedulingService.cs)
     - Menambahkan method `GetRoomsTimelineAsync(string date, string? floorLocation, CancellationToken cancellationToken)`.
     - Melakukan query ruangan aktif bertipe `RoomType.OperatingRoom`.
     - Melakukan query seluruh jadwal aktif pada tanggal tersebut (`IsCurrent && !IsDelete && Status != Cancelled && Status != Rejected`).
     - Menyusun slot waktu secara sekuensial (mulai pukul 07:00 hingga 21:00 WIB atau rentang jam kasus yang ada).
     - Menghasilkan slot `SterilizationBuffer` selama 45 menit setelah setiap tindakan bedah.

4. **Controller Baru**:
   - [`OperatingRoomTimelineController.cs`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Controllers/OperatingRoomTimelineController.cs)
     - Route: `GET /api/v1/health-services/operating-room-management/rooms/timeline`
     - Tag: `Health Services / Operating Room Management / Scheduling`
     - Filter izin: `[AccessPermission("OperatingRoomSchedule", "Read")]`

5. **Pengujian Otomatis (Unit Tests)**:
   - [`OperatingRoomTimelineTests.cs`](file:///c:/Users/Asoiii/Documents/Quilvian/SourceCode/QuilvianFinal/NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.OperatingRoomTests/OperatingRoomTimelineTests.cs)
     - `Timeline_menampilkan_slot_occupied_buffer_dan_available`: Memvalidasi pembentukan slot waktu terisi, buffer 45 menit, dan sisa waktu tersedia.
     - `Benturan_jadwal_pada_buffer_sterilisasi_ditolak_409_conflict`: Memvalidasi bahwa penjadwalan yang menabrak buffer 45 menit ditolak `409 Conflict` (`OPR003`).
     - `Ruang_tanpa_jadwal_menghasilkan_slot_available_penuh`: Memvalidasi ruangan kosong menghasilkan 1 slot `Available` penuh.

---

## 3. Bukti Verifikasi & Pengujian

### 3.1 Unit Test Target
Perintah yang dijalankan:
```bash
dotnet test NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.OperatingRoomTests/QuilvianSystemBackend.OperatingRoomTests.csproj -p:SkipMigrationMetadata=true --filter "FullyQualifiedName~OperatingRoomTimelineTests"
```
**Hasil**:
```text
Passed!  - Failed: 0, Passed: 3, Skipped: 0, Total: 3, Duration: 37 s - QuilvianSystemBackend.OperatingRoomTests.dll (net9.0)
```

### 3.2 Uji Regresi Smoke & Matriks Hak Akses
Perintah yang dijalankan:
```bash
dotnet test NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.OperatingRoomTests/QuilvianSystemBackend.OperatingRoomTests.csproj -p:SkipMigrationMetadata=true --filter "FullyQualifiedName~SeedSmokeTests|FullyQualifiedName~PermissionMatrixTests"
```
**Hasil**:
```text
Passed!  - Failed: 0, Passed: 38, Skipped: 0, Total: 38, Duration: 42 s - QuilvianSystemBackend.OperatingRoomTests.dll (net9.0)
```
Seluruh 38 skenario audit izin dan data seed demo lulus tanpa regresi.

---

## 4. Status Traceability Roadmap

- Task `BE-OPR-012` dinyatakan **SELESAI (✅)**.
- Task berikutnya yang siap dikerjakan: **`BE-OPR-013`** (Endpoint Lookup Booking Operasi Aktif untuk Admisi Rawat Inap).
