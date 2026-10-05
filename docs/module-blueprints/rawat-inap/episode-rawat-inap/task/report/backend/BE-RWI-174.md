# Laporan Perubahan Backend — `BE-RWI-174`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-174` |
| Judul | Jenis layanan bedah, rencana anestesi, dan status Ditolak (`E5` bagian kasus) |
| Slice | `MVP-1` / `RWF-W3` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-174` |
| Trace | `FR-RWF-043`, `044`, `086`; `RWI-DEC-175`, `204`, `208`; `INV-RWF-30`, `31`; `AC-RWF-085`, `099`; `UAT-RWF-24`; API 11.5.1, 11.9; state matrix 9.1; `VAL-RWF-81` s.d. `83` |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`); perubahan modul OK disetujui Ikbal Yulianto (`RWI-DEC-208`) |
| Dependency | — |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 1, diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/OperatingRoomManagement/**`, `Repositories/Configurations/HealthServices/OperatingRoomManagement/OprCaseConfiguration.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode dan penerapan skema maju selesai. Pembaruan 5 Oktober 2026: build terintegrasi `PASS` dan migration `20261005033044_AddRawatInapFinishing` diterapkan berdasarkan output pengguna; API/alur bisnis dan rollback belum dijalankan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OperatingRoomManagement` (pemilik Ikbal Yulianto) |
| Prefix registry | `Opr` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (`OprCase`, service dan controller kasus) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya (persetujuan pemilik OK `RWI-DEC-208`). Migration: tidak (keputusan pengguna) |

---

## 1. Masalah yang diperbaiki

Kasus OK belum mencatat apakah operasi bedah umum atau obstetri dan anestesi apa yang
direncanakan. Petugas OK juga tidak dapat menolak pesanan bangsal: satu-satunya jalan adalah
"batal", sehingga laporan mencampur pesanan yang ditolak dengan operasi yang dibatalkan.

## 2. Proses bisnis

1. Pemesan mengirim `POST cases` dengan `SurgicalServiceType` (kosong = `General`) dan
   `PlannedAnesthesiaType` opsional. Kasus lama terbaca `General` tanpa rencana anestesi.
2. Petugas penjadwalan OK membuka kasus berstatus Diminta lalu menolak dengan alasan 10–500
   karakter dan header `Idempotency-Key`.
3. Contoh: kasus milik Budi S. berstatus Diminta ditolak dengan alasan "Hasil lab pra-operasi belum
   ada" → status `Rejected`, penolak, waktu, dan alasan tersimpan; versi pra-operasi yang ada
   menjadi `Superseded`; order "Appendektomi" bebas dirujuk kasus baru.
4. Kasus `Rejected` final: ubah, jadwalkan, tunda, batal, mulai, atau tolak lagi → 422
   `OPR-CASE-REJ-002` "Kasus yang ditolak tidak dapat diubah; pesan ulang sebagai kasus baru".
5. Jalur tidak normal: tolak kasus selain Diminta → 422 `OPR-CASE-REJ-001`; versi berubah → 409
   `OPR012`; alasan < 10 karakter → 400.
6. Laporan pemakaian ruang memisahkan `RejectedCases` dari `CancelledCases`; daftar operasi
   menampilkan status `Rejected` sendiri.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OprCase.cs`, `OprStatusHistory.cs`, `OperatingRoomEnums.cs`, `OperatingRoomCaseDtos.cs`,
`OperatingRoomCaseService.cs`, `OperatingRoomCaseController.cs`, `OperatingRoomCommandSupport.cs`,
`OperatingRoomSchedulingService.cs`, `OperatingRoomExecutionService.cs`,
`OperatingRoomScheduleController.cs`, `OperatingRoomReportService.cs`, `OperatingRoomReportDtos.cs`,
`OprCaseConfiguration.cs`; kontrak 11.5.1, state 9.1, validasi 14, kamus data 19.2–19.3.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Enums/OperatingRoomEnums.cs` | `OprCaseStatus.Rejected = 8`; enum baru `OprSurgicalServiceType`, `OprPlannedAnesthesiaType` (juga `OprWardPreOpStatus`, `OprBodyView` untuk `BE-RWI-176`) |
| `Models/OprCase.cs`, `OprCaseConfiguration.cs` | Lima kolom baru; `CK_OprCase_Rejected`; bawaan `General` |
| `DTOs/OperatingRoomCaseDtos.cs` | Dua isian `POST`; `RejectOprCaseRequest`; respons + `SurgicalServiceType`, `PlannedAnesthesiaType`, `RejectedAt`, `RejectedByName`, `RejectionReason`, `LastStatusReason`, `WardPreOpStatus`, `HandoverStatus` |
| `Services/OperatingRoomCaseService.cs` | `RejectAsync`, `EnsureNotRejected`, sidik jari idempotensi kompatibel ke belakang, order kasus Ditolak bebas dirujuk ulang |
| `Services/OperatingRoomCommandSupport.cs` | Aksi `Reject` pada `Requested` |
| `Services/OperatingRoomSchedulingService.cs`, `OperatingRoomExecutionService.cs` | Penjaga `OPR-CASE-REJ-002` pada jadwal, tunda, batal, mulai; `Rejected` termasuk status tertutup |
| `Controllers/OperatingRoomCaseController.cs` | `PATCH /{id}/reject`; `PUT` kini meneruskan 422 |
| `Services/OperatingRoomReportService.cs`, `DTOs/OperatingRoomReportDtos.cs` | `RejectedCases` terpisah dari `CancelledCases` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif pada `POST`/`GET cases`; endpoint baru `PATCH /{id}/reject`. **Delta kontrak:** hitungan terpisah tampil sebagai `RejectedCases` pada `reports/utilization` (pola nama `CancelledCases` yang sudah ada), karena `reports/operations` berbentuk daftar per baris dengan status `Rejected` tersendiri |
| Database | lima kolom `OprCase` dan constraint `CK_OprCase_Rejected`. Perubahan `E5` bagian kasus tercakup dalam `20261005033044_AddRawatInapFinishing`; pengguna melaporkan penerapan berhasil (`Done.`), bukti diterima 5 Oktober 2026. Nama database/lingkungan tidak disebut |
| Keamanan/Auth | Permission baru `OperatingRoomCase : Reject` (diberikan admin hak akses pada petugas penjadwalan OK) |

## 4. Dokumentasi endpoint

#### Health Services / Operating Room Management / Cases

Base URL: `api/v1/health-services/operating-room-management/cases`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Menerima `SurgicalServiceType` dan `PlannedAnesthesiaType` | `OperatingRoomCase : Create` |
| `GET` | `/`, `/{id}` | Respons ditambah delapan isian; saringan `Status` menerima `Rejected` | `OperatingRoomCase : Read` |
| `PATCH` | `/{id}/reject` | Tolak order Diminta; `Idempotency-Key` + `{ Reason, ExpectedVersion }` | `OperatingRoomCase : Reject` |

#### Health Services / Operating Room Management / Reports

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `api/v1/health-services/operating-room-management/reports/utilization` | Bertambah `RejectedCases` | `OperatingRoomCase : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Prefix `Opr`; argumen permission cocok | `PASS` | Review source |
| Review diff dan scope | Sesuai kartu | `PASS` | Daftar 3.2 |
| Pemeriksaan bentrokan nama tipe | Tidak ada | `PASS` | Sesi 2 Oktober 2026 |
| Build project melalui `dotnet ef database update` | `Build succeeded.` | `PASS` | Output pengguna diterima 5 Oktober 2026; bukan eksekusi ulang oleh agent atau perintah `dotnet build` tersendiri; jumlah warning tidak disertakan |
| Migration `E5` bagian kasus | Tercakup dalam `20261005033044_AddRawatInapFinishing`; `Up()`/`Down()` tersedia dan penerapan maju berhasil menurut output pengguna | `PASS` (penerapan maju); rollback `NOT RUN` | Output pengguna 5 Oktober 2026 dan source migration |
| Verifikasi API dan proses bisnis `UAT-RWF-24`, regresi alur OK | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT RUN`. Alasan belum ada build/database berlaku pada sesi 2 Oktober 2026; bukti terbaru menunjukkan build dan penerapan migration, tetapi belum ada hasil uji API/alur bisnis.

### 5.1 Pembaruan bukti 5 Oktober 2026

Build project saat `dotnet ef database update` **PASS** menurut output pengguna yang diterima 5 Oktober 2026 (`Build succeeded.`); migration `20261005033044_AddRawatInapFinishing` diterapkan sampai `Done.`. Uji API, regresi, alur klinis, dan rollback `Down()` tetap `NOT RUN`. Nama database dan lingkungan tidak tercantum pada output. Catatan pengecualian 2 Oktober 2026 adalah riwayat sesi implementasi, bukan status build/migration terkini.

Perubahan `E5` bagian kasus: lima kolom `OprCase` dan constraint `CK_OprCase_Rejected`. Output lengkap, source migration, dan pemetaan lintas task ada pada [laporan BE-RWI-172](BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kasus lama terbaca `General` tanpa rencana anestesi | Terpenuhi (source) | `HasDefaultValue(General)`; `PlannedAnesthesiaType` nullable |
| 2. Tolak tanpa alasan atau < 10 karakter → 400; dari `Requested` → `Rejected` | Terpenuhi (source) | `RejectAsync` |
| 3. Tolak dari status lain → 422 `OPR-CASE-REJ-001` | Terpenuhi (source) | `RejectNotRequestedCode` |
| 4. Ubah/jadwal/tunda/batal/mulai kasus `Rejected` → 422 `OPR-CASE-REJ-002` | Terpenuhi (source) | `EnsureNotRejected` di keempat service |
| 5. Laporan memisahkan Ditolak dari Dibatalkan | Terpenuhi (source) | `RejectedCases` |
| 6. `POST cases` tetap menerima banyak tindakan bagi petugas OK | Terpenuhi (source) | Validasi `Procedures` tidak berubah |
| 7. Alur OK lama tidak berubah | Terpenuhi (source, belum diuji) | Sidik jari idempotensi lama tetap sama bila isian baru bawaan |
| DoD build tanpa error | Terpenuhi melalui build project terintegrasi pada perintah EF | Output pengguna: `Build succeeded.`; diterima 5 Oktober 2026 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nilai `OprPlannedAnesthesiaType` masih **usulan** desain 12.8 dan perlu disahkan pemilik OK |
| Masalah yang diketahui | `WardPreOpStatus` bergantung tabel `BE-RWI-176` (migration yang sama, `E5`) |
| Risiko tersisa | Regresi alur OK belum dibuktikan runtime |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` `OperatingRoomEnums.cs`, `OprCase.cs`, `OprCaseConfiguration.cs`, `OperatingRoomCaseDtos.cs`, `OperatingRoomCaseService.cs`, `OperatingRoomCommandSupport.cs`, `OperatingRoomSchedulingService.cs`, `OperatingRoomExecutionService.cs`, `OperatingRoomCaseController.cs`, `OperatingRoomReportService.cs`, `OperatingRoomReportDtos.cs` |
| Langkah berikutnya | Pemilik OK mengesahkan nilai rencana anestesi; jalankan `UAT-RWF-24` dan regresi alur OK. |
