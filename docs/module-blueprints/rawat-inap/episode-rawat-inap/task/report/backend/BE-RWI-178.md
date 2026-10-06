# Laporan Perubahan Backend — `BE-RWI-178`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-178` |
| Judul | Ekstraksi `PatientProcedureExecutionService` |
| Slice | `MVP-1` / `RWF-W3` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-178` |
| Trace | `FR-RWF-047`; `RWI-DEC-196`; backend 12.7 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, diperiksa 1, diubah 1, logika 1, kontrak API 1, database 1, keamanan 0, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement/{Services,Controllers}`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` dan regresi runtime **dikecualikan atas keputusan pengguna 2 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Prefix registry | `Trx`/`Cli` (tabel yang ada; tidak ada tabel baru) |
| Keberlakuan | `TOUCHED LEGACY` (`PatientProcedureController`); `NEW CODE` (service) |
| QBE yang berlaku | `QBE-SVC-001` (logika keluar dari controller), `QBE-LOG-001` |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Penyelesaian order tindakan — status `Completed`, dokumen rekam medis tertanda tangan, dan fakta
tagih ke Billing — hidup di dalam controller `PATCH patient-procedures/{id}/execute`. Kamar Operasi
tidak dapat menyelesaikan order saat kasus selesai tanpa memanggil HTTP ke dirinya sendiri.

## 2. Proses bisnis

1. Layar tindakan memanggil `execute` seperti biasa. Controller kini memanggil
   `PatientProcedureExecutionService.ExecuteAsync`: urutan penjaga (batal, approval, sudah selesai,
   pelaksana rawat inap), penandaan selesai, pendaftaran dokumen pada `SaveChanges` yang sama, lalu
   fakta tagih **sesudah** commit — identik dengan sebelumnya, termasuk kode status dan pesannya.
2. Kamar Operasi memanggil `ExecuteFromOperatingRoomAsync` saat kasus `Completed`: pelaksana = akun
   dokter operator (`ApplicationUser.DoctorId`), waktu pelaksanaan = waktu kasus selesai, **tanpa**
   dokumen tindakan baru (dokumen klinisnya laporan operasi final OK).
3. Contoh: kasus OK selesai 11.40 untuk order "Sectio Caesarea" → order `Completed`, `PerformedAt`
   11.40, satu fakta `ProcedureCharge`. Efek dijalankan ulang → order sudah selesai, tidak ada fakta
   kedua.
4. Jalur tidak normal: order batal → tidak diselesaikan; order butuh approval yang belum ada →
   tidak diselesaikan dan dilaporkan sebagai kegagalan efek.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientProcedureController.cs` (`execute`, `cancel`, pembentuk fakta), `PatientProcedureOrderService.cs`,
`ClinicalDocumentIntegrityService.cs`, `ClinicalMilestoneFactProducer.cs`, `ClinicalMilestoneFactDtos.cs`,
`PatientProcedureDtos.cs`, `TrxPatientProcedure.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureExecutionService.cs` | Baru — `ExecuteAsync` (perilaku lama), `ExecuteFromOperatingRoomAsync`, pembentuk fakta tunggal `BuildProcedureFactRequest` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | `execute` memanggil service; pembentuk fakta controller mendelegasikan ke service; dependensi `ClinicalDocumentIntegrityService` yang tak terpakai dicabut dari konstruktor |
| `Program.cs` | Registrasi `PatientProcedureExecutionService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — endpoint `execute` tidak berubah bentuk, kode status, maupun pesan |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` — permission `PatientProcedure : Execute` tetap |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `api/v1/health-services/clinical-management/patient-procedures/{id}/execute` | Tetap; kini lewat service | `PatientProcedure : Execute` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff "perubahan struktur, bukan perilaku" | Urutan penjaga, pesan, satu `SaveChanges`, fakta sesudah commit dipertahankan | `PASS` | Perbandingan blok lama dan service |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Regresi penyelesaian tindakan rawat jalan dan rawat inap | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `execute` menghasilkan status, dokumen, dan fakta tagih yang sama | Terpenuhi (source) | `ExecuteAsync` memindahkan blok lama apa adanya |
| 2. `ExecuteFromOperatingRoomAsync` pada order `Completed` tidak membuat apa pun | Terpenuhi (source) | Penjaga `IsExecuted && Completed` → `AlreadyExecuted` |
| 3. Fakta tagih tetap diterbitkan sesudah commit | Terpenuhi (source) | `EmitChargeEligibilityAsync` sesudah `SaveChangesAsync` |
| DoD build dan regresi | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Bila dokter operator belum tertaut akun (`DoctorId`), pelaksana jatuh ke akun yang membuat kasus selesai |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Endpoint dipakai luas — regresi wajib dijalankan pemilik sesudah build |
| Perubahan sampingan | Konstruktor `PatientProcedureController` kehilangan satu parameter (DI menyesuaikan otomatis) |
| Interupsi | `NONE` |
| Status Git | `??` `PatientProcedureExecutionService.cs`; `M` `PatientProcedureController.cs`, `Program.cs` |
| Langkah berikutnya | Regresi `execute` rawat jalan dan rawat inap sesudah build |
