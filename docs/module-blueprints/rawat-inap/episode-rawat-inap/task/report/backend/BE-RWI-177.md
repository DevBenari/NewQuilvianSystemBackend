# Laporan Perubahan Backend — `BE-RWI-177`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-177` |
| Judul | Penerima sah serah terima pasca operasi |
| Slice | `MVP-1` / `RWF-W3` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-177` |
| Trace | `FR-RWF-046`, `049`, `088`; `RWI-DEC-177`, `189`, `220` (3); `INV-RWF-28`; `AC-RWF-043`, `047`, `049`, `087`; `UAT-RWF-13`; API 11.5.2, 11.5.3; `VAL-RWF-84` s.d. `86`; backend 12.11 (`E8`), 12.12 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | — |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 1, diubah 2, logika 2, kontrak API 2, database 1, keamanan 2, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/OperatingRoomManagement/**`, `Areas/HealthServices/InPatientManagement/Services/InpPatientLocationQuery.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `OperatingRoomManagement` (serah terima); `InPatientManagement` (bacaan lokasi bed) |
| Prefix registry | `Opr`, `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (recovery service/controller); `NEW CODE` (query service, controller daftar, seeder hak) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001` |
| Wewenang | Source: ya. Data hak akses: lewat seeder startup sekali jalan (bukan migration) |

---

## 1. Masalah yang diperbaiki

Satu hak `OperatingRoomHandover : Update` dipakai untuk mengirim sekaligus menerima serah terima,
dan penerima tidak diperiksa. Perawat OK dapat "menerima" kirimannya sendiri, dan serah terima ke
ICU dapat diterima walaupun pasien masih di bangsal.

## 2. Proses bisnis

1. Perawat OK mengirim serah terima ke unit tujuan (hak `: Send`).
2. Perawat unit tujuan (hak `: Receive`, diberikan admin; **tidak** disalin otomatis) menerima atau
   menolak beralasan. Penerima/penolak tidak boleh pengirimnya → 422 `OPR-HO-002`.
3. Menerima menuntut pasien sudah menempati bed aktif di unit tujuan. Contoh `UAT-RWF-13`: Budi masih
   di Melati saat serah terima ke ICU → 422 `OPR-HO-001` "Pindahkan pasien ke tempat tidur di unit ini
   lewat Transfer Pasien sebelum menerima serah terima". Menolak tidak butuh syarat ini.
4. Menerima tidak memindahkan bed; tarif kamar bed asal tetap berjalan selama di OK.
5. Bangsal dan Daftar Pantau membaca daftar serah terima per unit tujuan: `PatientInDestinationUnit`,
   `WaitingMinutes`, `IsOverdue` (ambang `PendingSurgicalHandoverAlertMinutes`, bawaan 60).
6. Startup sekali jalan: setiap peran (departemen + posisi) yang memegang `Update` lama mendapat
   `Send`. Begitu `Send` pernah punya kebijakan, seeder tidak menyentuhnya lagi.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OperatingRoomRecoveryController.cs`, `OperatingRoomRecoveryService.cs`, `OprHandover.cs`,
`OprHandoverConfiguration.cs`, `InpBedPlacement.cs`, `InpEpisodeConfiguration.cs`,
`InpEpisodeService.Assignments.cs`, `AccessMenuSeeder.cs`, `PermissionRegistryDescriptor.cs`,
`PermissionRegistryValidator.cs`, `SysAccessPolicy.cs`, `InpSettingService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpPatientLocationQuery.cs` | Baru — `IsPatientInUnitAsync`, `GetCurrentLocationAsync`, `GetCurrentLocationsAsync` |
| `OperatingRoomManagement/Services/OperatingRoomRecoveryService.cs` | Aturan `OPR-HO-001`/`002` pada penerimaan |
| `OperatingRoomManagement/Controllers/OperatingRoomRecoveryController.cs` | `POST handovers` → `: Send`; `PATCH accept` → `: Receive` |
| `OperatingRoomManagement/DTOs/OprHandoverQueueDtos.cs` | Baru — saringan dan baris daftar |
| `OperatingRoomManagement/Services/OperatingRoomHandoverQueryService.cs` | Baru — daftar per unit/status, `OverdueOnly` |
| `OperatingRoomManagement/Controllers/OperatingRoomHandoverQueryController.cs` | Baru — `GET operating-room-management/handovers` |
| `OperatingRoomManagement/Seeders/OperatingRoomHandoverPermissionSeeder.cs` | Baru — salin `Update` → `Send`, sekali jalan |
| `Program.cs` | Registrasi service dan seeder (sesudah `AccessMenuSeeder`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Perubahan permission dua endpoint; grup baru `Handovers` |
| Database | `NOT APPLICABLE` untuk schema; data `SysAccessPolicy` ditambah seeder |
| Keamanan/Auth | **Inti task.** Kemampuan `OperatingRoomHandover : Update` tidak lagi dideklarasikan sehingga ditutup `AccessMenuSeeder`; `Send` disalin; `Receive` wajib diberikan admin hak akses pada perawat unit rawat inap dan ICU |

## 4. Dokumentasi endpoint

#### Health Services / Operating Room Management / Execution

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/execution`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/handovers` | Kirim serah terima | `OperatingRoomHandover : Send` |
| `PATCH` | `/handovers/{handoverId}/accept` | Terima atau tolak; aturan penerima dan bed | `OperatingRoomHandover : Receive` |

#### Health Services / Operating Room Management / Handovers

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/health-services/operating-room-management/handovers` | Daftar per unit tujuan; `DestinationUnitId`, `Status`, `OverdueOnly`, paging | `OperatingRoomHandover : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Argumen permission cocok `[AccessAction]`; resource sama modul OK | `PASS` | Review source |
| Review registry permission | Tidak ada resource ganda antar modul | `PASS` | `PermissionRegistryDescriptor` |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Verifikasi proses bisnis pemindahan pasien ke ICU | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Serah terima ke ICU saat pasien di bangsal → 422 `OPR-HO-001` | Terpenuhi (source) | `AcceptHandoverAsync` + `IsPatientInUnitAsync` |
| 2. Pengirim menerima sendiri → 422 `OPR-HO-002` | Terpenuhi (source) | `AcceptHandoverAsync` |
| 3. Tanpa `: Receive` → 403 | Terpenuhi (source) | `[AccessPermission("OperatingRoomHandover","Receive")]` |
| 4. Menerima tidak memindahkan bed | Terpenuhi (source) | Tidak ada penulisan `InpBedPlacement` |
| 5. Daftar menampilkan `PatientInDestinationUnit`, `WaitingMinutes`, `IsOverdue` | Terpenuhi (source) | `OperatingRoomHandoverQueryService` |
| 6. Peran pemegang `Update` kini memegang `Send` | Terpenuhi (source) | `OperatingRoomHandoverPermissionSeeder` |
| DoD build tanpa error | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sesudah rilis, penerimaan serah terima ditolak 403 untuk semua perawat sampai admin memberikan `: Receive` |
| Masalah yang diketahui | Aturan baru tetap berlaku walaupun `OperatingRoomRuleRelaxation` menyala (hanya syarat keanggotaan tim yang dilonggarkan) |
| Risiko tersisa | Perubahan permission pada peran yang sudah ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` `InpPatientLocationQuery.cs`, `OprHandoverQueueDtos.cs`, `OperatingRoomHandoverQueryService.cs`, `OperatingRoomHandoverQueryController.cs`, `OperatingRoomHandoverPermissionSeeder.cs`; `M` `OperatingRoomRecoveryService.cs`, `OperatingRoomRecoveryController.cs`, `Program.cs` |
| Langkah berikutnya | Admin hak akses memberikan `OperatingRoomHandover : Receive` pada perawat unit tujuan |
