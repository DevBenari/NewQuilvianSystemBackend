# Laporan Perubahan Backend — `BE-RWI-182`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-182` |
| Judul | Dua daftar pantau tertunda |
| Slice | `MVP-2` / `RWF-W7` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-182` |
| Trace | `FR-RWF-088`; `RWI-DEC-201`, `216`, `220` (6); gate G-18; API 11.9 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-172` ✅; `BE-RWI-177` ✅; `BE-RWI-181` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, diperiksa 1, diubah 1, logika 0, kontrak API 2, database 1, keamanan 0, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/{Controllers,DTOs}` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (Daftar Pantau), membaca OK dan permintaan admisi |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (`InpatientMonitoringController`, DTO pemantauan) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001` |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Kepala ruangan tidak punya cara melihat serah terima pasca operasi yang terlalu lama belum
diterima, maupun permintaan admisi dari kamar pulih yang terlalu lama belum ditindaklanjuti.

## 2. Proses bisnis

1. Daftar Pantau membaca dua kartu baru dengan `InpatientMonitoring : Read`.
2. "Serah terima pasca operasi tertunda": serah terima `Sent` yang menunggu lebih lama dari
   `PendingSurgicalHandoverAlertMinutes` (bawaan 60). Contoh: dikirim ke ICU 08.00, pukul 09.05 muncul
   dengan `PatientInDestinationUnit = false` bila Budi belum dipindahkan.
3. "Permintaan admisi tertunda": permintaan `Pending` lebih lama dari `PendingAdmissionReferralAlertMinutes`
   (bawaan 30).
4. Admin mengubah ambang di pengaturan → isi daftar berubah pada pembacaan berikutnya (tanpa cache).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientMonitoringController.cs`, `InpatientMonitoringDtos.cs`, `OperatingRoomHandoverQueryService.cs`,
`InpAdmissionReferralService.cs`, `InpSettingService.cs`; kontrak 11.9.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Controllers/InpatientMonitoringController.cs` | Dua endpoint baru; dependensi `OperatingRoomHandoverQueryService`, `InpAdmissionReferralService` |
| `InPatientManagement/DTOs/InpatientMonitoringDtos.cs` | `PendingSurgicalHandoverQuery`, `PendingAdmissionReferralQuery` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru API 11.9; respons memakai bentuk API 11.5.3 dan 11.6 |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Memakai `InpatientMonitoring : Read` yang sudah ada |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/pending-surgical-handovers` | Serah terima pasca operasi tertunda (`DestinationUnitId`, paging) | `InpatientMonitoring : Read` |
| `GET` | `/pending-admission-referrals` | Permintaan admisi tertunda (`Search`, paging) | `InpatientMonitoring : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff dan scope | Baca saja, `OverdueOnly = true` | `PASS` | Daftar 3.2 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Verifikasi API dengan data uji melewati ambang | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Serah terima > 60 menit muncul dengan tanda "pasien belum di unit tujuan" | Terpenuhi (source) | `PatientInDestinationUnit`, `IsOverdue` |
| 2. Permintaan admisi > 30 menit muncul | Terpenuhi (source) | `GetPagedAsync` dengan `OverdueOnly` |
| 3. Mengubah ambang mengubah isi daftar | Terpenuhi (source) | Ambang dibaca tiap permintaan dari `InpSettingService` |
| 4. Tanpa `InpatientMonitoring : Read` → 403 | Terpenuhi (source) | Atribut endpoint |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `NONE` di luar build |
| Perubahan sampingan | Konstruktor `InpatientMonitoringController` bertambah dua parameter |
| Interupsi | `NONE` |
| Status Git | `M` `InpatientMonitoringController.cs`, `InpatientMonitoringDtos.cs` |
| Langkah berikutnya | Verifikasi dengan data melewati ambang sesudah build |
