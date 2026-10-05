# Laporan Perubahan Backend — `BE-RWI-181`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-181` |
| Judul | Permintaan admisi dari kamar pulih (`E6`) |
| Slice | `MVP-2` / `RWF-W7` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-181` |
| Trace | `FR-RWF-080`, `089`; `RWI-DEC-201`, `207`, `208`, `220` (1); `INV-RWF-32`; `AC-RWF-080`, `088`, `089`; `UAT-RWF-16`, `33`, `41`; API 11.5.2, 11.6; state 9.4; `VAL-RWF-87` s.d. `89`; kamus data 19.8 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`); panggilan dari OK disetujui `RWI-DEC-208` |
| Dependency | `BE-RWI-174` — ✅ |
| Klasifikasi | `HEAVY` — skor 12: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/**`, `OperatingRoomRecoveryService.cs`, `OperatingRoomRecoveryDtos.cs`, configuration, `ApplicationDbContext.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` dan migration `E6` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (pemilik data); dipanggil `OperatingRoomManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (model, service, controller); `TOUCHED LEGACY` (`InpEpisodeService`, `InpatientEpisodeController`, `OperatingRoomRecoveryService`) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Migration `E6`: tidak (keputusan pengguna) |

---

## 1. Masalah yang diperbaiki

Keputusan kamar pulih "Rawat inap" atau "ICU" untuk pasien poliklinik/IGD/ODC tidak menghasilkan
apa pun di Rawat Inap: petugas admisi tidak tahu ada pasien yang menunggu dirawat, dan admisi dapat
dibuka tanpa merujuk asal operasinya.

## 2. Proses bisnis

1. Dokter anestesi menyimpan keputusan kamar pulih `Inpatient`/`Icu` (status siap keluar atau keluar).
2. Sesudah commit OK, `CreateFromRecoveryAsync`: pasien tanpa episode hadir → satu permintaan
   `Pending` (kunjungan asal = `OprCase.EncounterId`, usulan DPJP = dokter operator). Pasien dengan
   episode hadir → tidak dibuat (`NotNeeded`, serah terima biasa). Respons OK membawa
   `AdmissionReferralState`.
3. Keputusan berubah ke `OtherUnit`/`Discharged` → permintaan `Pending` kasus itu dibatalkan beralasan.
4. Petugas admisi membuka daftar permintaan (terlama lebih dulu, `WaitingMinutes`, `IsOverdue`) dan
   membuka admisi dengan `AdmissionReferralId`; permintaan menjadi `Completed` dalam transaksi admisi.
5. Contoh `UAT-RWF-16`/`33`: pasien Poli Bedah dioperasi, keputusan "Rawat inap" → satu permintaan
   `Pending`; admisi biasa untuk pasien itu → 409 `INP-ADM-REF-001` "Pasien punya permintaan admisi
   dari kamar pulih; buka admisi dari permintaan itu"; admisi dari permintaan → berhasil.
6. Jalur tidak normal: permintaan sudah selesai/batal atau milik pasien lain → 422 `INP-ADM-REF-002`;
   tidak pernah ada episode yang dibuat otomatis.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpEpisodeService.cs`, `InpatientEpisodeController.cs`, `InpatientEpisodeDtos.cs`, `InpEpisodeConfiguration.cs`,
`OperatingRoomRecoveryService.cs`, `OperatingRoomRecoveryDtos.cs`, `RegPatientEncounter.cs`, `InpSettingService.cs`;
kontrak 11.6, state 9.4, kamus data 19.8, DDL 19.12.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Enums/InpAdmissionReferralStatus.cs` | Baru — `InpAdmissionReferralStatus`, `InpRequestedCareLevel`, `InpAdmissionReferralState` |
| `InPatientManagement/Models/InpAdmissionReferral.cs` | Baru |
| `Repositories/.../InpAdmissionReferralConfiguration.cs` | Baru — `UX_..._Patient_Pending`, `UX_..._Case_Pending`, `UX_..._CompletedEpisode`, `IX_..._Status_RequestedAt`, `CK_InpAdmissionReferral_State`, FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | `DbSet` `InpAdmissionReferrals` |
| `InPatientManagement/Services/InpAdmissionReferralService.cs` | Baru — buat/batal dari kamar pulih, penjaga admisi, penyelesaian, bacaan |
| `InPatientManagement/DTOs/InpatientAdmissionReferralDtos.cs`, `Controllers/InpatientAdmissionReferralController.cs` | Baru — dua endpoint baca |
| `InPatientManagement/DTOs/InpatientEpisodeDtos.cs`, `Services/InpEpisodeService.cs`, `Controllers/InpatientEpisodeController.cs` | `AdmissionReferralId`; penjaga `INP-ADM-REF-001/002`; kode kontrak di `errors.Code` |
| `OperatingRoomManagement/Services/OperatingRoomRecoveryService.cs`, `DTOs/OperatingRoomRecoveryDtos.cs` | Panggilan sesudah commit; `AdmissionReferralState` |
| `Program.cs` | Registrasi `InpAdmissionReferralService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru API 11.6; `POST episodes` + `AdmissionReferralId`; `PUT …/execution/recovery` + `AdmissionReferralState`. Penolakan admisi kini membawa `errors.Code` (aditif) |
| Database | Tabel baru. **Migration `E6` belum dibuat** — wajib sesudah `E5` dan sebelum `I6` (`BE-RWI-159`) |
| Keamanan/Auth | Permission baru `InpatientAdmissionReferral : Read` |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Admission Referral

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/health-services/inpatient-management/admission-referrals` | Daftar; `Status` (bawaan `Pending`), `OverdueOnly`, `Search`, paging | `InpatientAdmissionReferral : Read` |
| `GET` | `api/v1/health-services/inpatient-management/admission-referrals/{id}` | Detail untuk mengisi awal admisi | `InpatientAdmissionReferral : Read` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/health-services/inpatient-management/episodes` | Bertambah `AdmissionReferralId`; 409 `INP-ADM-REF-001`, 422 `INP-ADM-REF-002` | `InpatientEpisode : Create` |

#### Health Services / Operating Room Management / Execution

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `api/v1/health-services/operating-room-management/cases/{caseId}/execution/recovery` | Perilaku baru + `AdmissionReferralState` | `OperatingRoomAnesthesia : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Prefix `Inp`; service pemilik `DbContext` | `PASS` | Review source |
| Pemeriksaan bentrokan nama tipe | Tidak ada | `PASS` | Sesi 2 Oktober 2026 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Migration `E6` | Tidak dibuat | `NOT RUN` | Keputusan pengguna |
| Proses bisnis ujung ke ujung dengan kasus OK | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pasien poliklinik + keputusan `Inpatient` → satu permintaan `Pending` | Terpenuhi (source) | `CreateFromRecoveryAsync` |
| 2. Pasien dengan episode hadir → tidak ada permintaan | Terpenuhi (source) | `HasPresentEpisodeAsync` |
| 3. Keputusan berubah dari `Inpatient`/`Icu` → permintaan dibatalkan | Terpenuhi (source) | `CancelFromRecoveryAsync` |
| 4. Admisi biasa → 409 `INP-ADM-REF-001`; admisi dari permintaan berhasil dan `Completed` | Terpenuhi (source) | `CheckForAdmissionAsync`, `MarkCompleted` dalam transaksi admisi |
| 5. Permintaan selesai/batal → 422 `INP-ADM-REF-002` | Terpenuhi (source) | `ReferralNotPendingCode` |
| 6. Tidak ada episode dibuat otomatis | Terpenuhi (source) | Service tidak menulis `InpEpisode` |
| 7. `ADMISSION_CONFIRMED` tetap daftar putih | Terpenuhi (source) | Outbox tidak disentuh |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `RowVersion` sebagai `Guid` bertanda konkurensi (kamus: `xmin`) |
| Masalah yang diketahui | Bila pasien sudah punya permintaan `Pending` dari kasus lain, kasus kedua tidak membuat permintaan baru (`NotNeeded`) |
| Risiko tersisa | Tabel ini target FK `BilInvoiceEncounterLink` (`BE-RWI-159`) |
| Perubahan sampingan | `InpEpisodeOperationResult` bertambah `Code` dan `FromStatus` (aditif) |
| Interupsi | `NONE` |
| Status Git | `??` enum, model, configuration, service, DTO, controller permintaan admisi; `M` `InpEpisodeService.cs`, `InpatientEpisodeController.cs`, `InpatientEpisodeDtos.cs`, `OperatingRoomRecoveryService.cs`, `OperatingRoomRecoveryDtos.cs`, `ApplicationDbContext.cs`, `Program.cs` |
| Langkah berikutnya | Migration `E6` sesudah `E5`; admin memberikan `InpatientAdmissionReferral : Read` pada petugas admisi |
