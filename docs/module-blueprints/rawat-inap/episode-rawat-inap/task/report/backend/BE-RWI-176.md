# Laporan Perubahan Backend — `BE-RWI-176`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-176` |
| Judul | Pra-operasi bangsal berversi dan syarat "Siap" (`E5` bagian pra-operasi) |
| Slice | `MVP-1` / `RWF-W3` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-176` |
| Trace | `FR-RWF-045`, `048`, `090`; `RWI-DEC-173`, `174`, `199`; `INV-RWF-26`, `27`; `AC-RWF-042`, `046`, `093`, `094`; `UAT-RWF-21`; API 11.3; state 9.2; `VAL-RWF-74` s.d. `80`; kamus data 19.4–19.6 |
| Contract version | `0.10.0` **`approved`** (`RWI-DEC-221`); modul OK disetujui `RWI-DEC-208` |
| Dependency | `BE-RWI-173` — ✅; `BE-RWI-174` — ✅ |
| Klasifikasi | `HEAVY` — skor 12: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/OperatingRoomManagement/**`, `Repositories/Configurations/HealthServices/OperatingRoomManagement/**`, `ApplicationDbContext.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e2ded614` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ Implementasi kode dan penerapan skema maju selesai. Pembaruan 5 Oktober 2026: build terintegrasi `PASS` dan migration `20261005033044_AddRawatInapFinishing` diterapkan berdasarkan output pengguna; API/alur bisnis dan rollback belum dijalankan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OperatingRoomManagement` |
| Prefix registry | `Opr` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (tiga model, service); `TOUCHED LEGACY` (`OperatingRoomPreparationService`, controller, penjadwalan, pembatalan) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Migration: tidak (keputusan pengguna) |

---

## 1. Masalah yang diperbaiki

Persiapan pasien dari bangsal (puasa, gelang identitas, hasil lab, penandaan sisi operasi) tidak
tercatat di sistem dan tidak menahan kasus menjadi "Siap". OK bisa menyatakan kasus siap walaupun
bangsal belum mengirim catatan pra-operasi, atau walaupun catatan itu sudah basi karena kasus ditunda.

## 2. Proses bisnis

1. Perawat bangsal membuka Catatan Pra-Operasi kasus. Bila belum ada versi, server mengembalikan
   templat berisi butir aktif master (`IsTemplate = true`).
2. Perawat menyimpan draf: centang butir, catatan butir (misalnya "Puasa sejak 22.00"), titik
   penandaan pada gambar tubuh (persen 0–100), sisi penandaan.
3. Perawat mengirim. Server membekukan tanda vital terakhir kunjungan (TD, nadi, napas, suhu, SpO₂)
   dan nyeri terakhir (pengkajian nyeri, atau skala nyeri pada tanda vital) sebagai potret.
4. Perawat OK dari **akun berbeda** mengonfirmasi butir dan penandaan. Bila seluruh butir wajib dan
   penandaan terkonfirmasi, versi menjadi `Confirmed`; bila syarat lain sudah lengkap, kasus langsung
   naik ke `Ready` dalam transaksi yang sama.
5. Kasus ditunda → versi `Sent`/`Confirmed` menjadi `NeedsUpdate` dalam transaksi penundaan. Draf
   berikutnya membuat versi baru yang menyalin butir lama sebagai usulan; saat dikirim TD terbaru
   dibekukan dan versi lama menjadi `Superseded` (`UAT-RWF-21`: TD 120/80 versi 1 → 160/100 versi 2).
6. Kasus ditolak atau dibatalkan → seluruh versi `Superseded`.
7. Jalur tidak normal: tanpa tanda vital → 422 `OPR-WPO-005`; butir wajib belum dicentang → 422
   `OPR-WPO-003` beserta daftar butir; sisi penandaan ≠ sisi pesanan → 422 `OPR-WPO-001`; konfirmasi
   oleh pengirim → 422 `OPR-WPO-002`; kasus `Rejected`/`Cancelled`/`InProgress`/`Completed` → 422
   `OPR-WPO-004`; versi berubah → 409.
8. Gerbang "Siap" memuat kode baru `WARD_PRE_OP_INCOMPLETE` / `WARD_PRE_OP_NEEDS_UPDATE` pada
   `Blockers[]`. Jalur darurat (bypass) yang sudah ada tetap melewati syarat ini dengan alasan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OperatingRoomPreparationService.cs`, `OperatingRoomPreparationController.cs`,
`OperatingRoomPreparationDtos.cs`, `OperatingRoomSchedulingService.cs`,
`OperatingRoomExecutionService.cs`, `OperatingRoomCaseService.cs`, `TrxPatientVitalSign.cs`,
`TrxPatientAssessment.cs`, `CliAssessmentInstrumentResponse.cs`, `CliClinicalInstrument*.cs`,
`NursingAssessmentDocumentService.cs`, `MstSurgicalPreparationItem.cs`; kontrak 11.3, state 9.2,
validasi 14, kamus data 19.4–19.6, DDL 19.12.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Models/OprWardPreOpNote.cs`, `OprWardPreOpItem.cs`, `OprWardPreOpSiteMark.cs` | Baru — tiga tabel |
| `Repositories/.../OprWardPreOpNoteConfiguration.cs`, `OprWardPreOpItemConfiguration.cs`, `OprWardPreOpSiteMarkConfiguration.cs` | Baru — unique `UX_OprWardPreOpNote_Case_Version`, unique parsial `UX_OprWardPreOpNote_OneOpen`, `UX_OprWardPreOpItem_Note_Item`, check constraint dua akun dan koordinat, FK `Restrict`/`Cascade` sesuai kamus |
| `Repositories/ApplicationDbContext.cs` | `DbSet` tiga tabel |
| `DTOs/OprWardPreOpDtos.cs` | Baru — request draf/kirim/konfirmasi, respons versi, potret |
| `Services/OprWardPreOpService.cs` | Baru — baca, draf, kirim, konfirmasi, `MarkNeedsUpdateAsync`, `SupersedeAllAsync`, `ReadGateAsync` |
| `Services/OperatingRoomPreparationService.cs` | Syarat keempat gerbang Siap, `Blockers[]` berkode, `TryCompleteReadinessAsync` |
| `DTOs/OperatingRoomPreparationDtos.cs` | `Blockers` |
| `Controllers/OperatingRoomPreparationController.cs` | Lima endpoint `ward-pre-op` |
| `Services/OperatingRoomSchedulingService.cs` | Penundaan menandai `NeedsUpdate` dalam transaksi yang sama |
| `Services/OperatingRoomExecutionService.cs`, `OperatingRoomCaseService.cs` | Batal dan tolak menggugurkan versi pra-operasi |
| `Program.cs` | Registrasi `OprWardPreOpService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint baru; `GET preparation` bertambah `Blockers[]` (kode untuk seluruh prasyarat, termasuk dua kode baru). **Delta:** `GET ward-pre-op` mengembalikan templat bila belum ada versi; `BodyView` berupa enum angka mengikuti gaya DTO OK |
| Database | `OprWardPreOpNote`, `OprWardPreOpItem`, dan `OprWardPreOpSiteMark`. Perubahan `E5` bagian pra-operasi tercakup dalam `20261005033044_AddRawatInapFinishing`; pengguna melaporkan penerapan berhasil (`Done.`), bukti diterima 5 Oktober 2026. Nama database/lingkungan tidak disebut |
| Keamanan/Auth | Permission baru `OperatingRoomWardPreOp : Read/Send/Confirm` |

## 4. Dokumentasi endpoint

#### Health Services / Operating Room Management / Preparation

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/preparation`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/ward-pre-op` | Versi terbaru (atau templat) beserta butir dan penandaan | `OperatingRoomWardPreOp : Read` |
| `GET` | `/ward-pre-op/versions` | Seluruh versi | `OperatingRoomWardPreOp : Read` |
| `PUT` | `/ward-pre-op/draft` | Simpan draf; versi baru bila `NeedsUpdate` | `OperatingRoomWardPreOp : Send` |
| `PATCH` | `/ward-pre-op/send` | Kirim; potret dibekukan; `Idempotency-Key` | `OperatingRoomWardPreOp : Send` |
| `PATCH` | `/ward-pre-op/confirm` | Konfirmasi penerima; `Idempotency-Key` | `OperatingRoomWardPreOp : Confirm` |
| `GET` | `/` | Kesiapan; `Blockers[]` bertambah dua kode | `OperatingRoomPreparation : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight dan kesesuaian | Prefix `Opr`; service pemilik `DbContext` | `PASS` | Review source |
| Review diff dan scope | Sesuai kartu | `PASS` | Daftar 3.2 |
| Pemeriksaan bentrokan nama tipe | Tidak ada | `PASS` | Sesi 2 Oktober 2026 |
| Build project melalui `dotnet ef database update` | `Build succeeded.` | `PASS` | Output pengguna diterima 5 Oktober 2026; bukan eksekusi ulang oleh agent atau perintah `dotnet build` tersendiri; jumlah warning tidak disertakan |
| Migration `E5` bagian pra-operasi | Tercakup dalam `20261005033044_AddRawatInapFinishing`; `Up()`/`Down()` tersedia dan penerapan maju berhasil menurut output pengguna | `PASS` (penerapan maju); rollback `NOT RUN` | Output pengguna 5 Oktober 2026 dan source migration |
| Verifikasi proses bisnis dua akun | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT RUN`. Alasan belum ada build/database berlaku pada sesi 2 Oktober 2026; bukti terbaru menunjukkan build dan penerapan migration, tetapi belum ada hasil uji API/alur bisnis.

### 5.1 Pembaruan bukti 5 Oktober 2026

Build project saat `dotnet ef database update` **PASS** menurut output pengguna yang diterima 5 Oktober 2026 (`Build succeeded.`); migration `20261005033044_AddRawatInapFinishing` diterapkan sampai `Done.`. Uji API, regresi, alur klinis, dan rollback `Down()` tetap `NOT RUN`. Nama database dan lingkungan tidak tercantum pada output. Catatan pengecualian 2 Oktober 2026 adalah riwayat sesi implementasi, bukan status build/migration terkini.

Perubahan `E5` bagian pra-operasi: `OprWardPreOpNote`, `OprWardPreOpItem`, dan `OprWardPreOpSiteMark`. Output lengkap, source migration, dan pemetaan lintas task ada pada [laporan BE-RWI-172](BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kirim tanpa tanda vital → 422 `OPR-WPO-005`; potret dari pencatatan terbaru | Terpenuhi (source) | `SendAsync` |
| 2. Butir wajib pengirim belum dikonfirmasi → 422 `OPR-WPO-003` | Terpenuhi (source) | `SendAsync` |
| 3. Sisi penandaan berbeda → 422 `OPR-WPO-001` | Terpenuhi (source) | `EnsureLateralityMatches` (draf, kirim, konfirmasi) |
| 4. Konfirmasi dari akun yang sama → 422 `OPR-WPO-002` | Terpenuhi (source) | `ConfirmAsync`; `CK_OprWardPreOpNote_TwoAccounts` |
| 5. Tidak dapat Siap sebelum versi terbaru terkonfirmasi; `Blockers[]` memuat kode baru | Terpenuhi (source) | `ReadGateAsync`, `BuildOutstanding` |
| 6. Ditunda → `NeedsUpdate`; versi baru menyalin butir dan memuat TD terbaru | Terpenuhi (source) | `MarkNeedsUpdateAsync`, `CreateVersionAsync`, `SendAsync` |
| 7. Kasus `Rejected`/`Cancelled`/`InProgress`/`Completed` → 422 `OPR-WPO-004` | Terpenuhi (source) | `EnsureCaseWritable` |
| 8. Jalur bypass darurat tetap berlaku | Terpenuhi (source) | Syarat pra-operasi di dalam blok `!bypass` |
| DoD build tanpa error | Terpenuhi melalui build project terintegrasi pada perintah EF | Output pengguna: `Build succeeded.`; diterima 5 Oktober 2026 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Gerbang berlaku untuk **semua** kasus OK (desain tidak membatasinya pada pesanan bangsal); kasus poliklinik/ODC pun butuh pra-operasi terkonfirmasi atau jalur darurat |
| Masalah yang diketahui | Catatan penerima pada butir ditambahkan ke kolom catatan butir dengan awalan "Penerima:" karena kamus data hanya punya satu kolom catatan |
| Risiko tersisa | Kasus terjadwal yang sudah berjalan di produksi akan tertahan di `Scheduled` sampai pra-operasi dikirim dan dikonfirmasi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` tiga model, tiga configuration, `OprWardPreOpDtos.cs`, `OprWardPreOpService.cs`; `M` `OperatingRoomPreparationService.cs`, `OperatingRoomPreparationController.cs`, `OperatingRoomPreparationDtos.cs`, `OperatingRoomSchedulingService.cs`, `OperatingRoomExecutionService.cs`, `ApplicationDbContext.cs`, `Program.cs` |
| Langkah berikutnya | Jalankan verifikasi pra-operasi dengan dua akun dan regresi gerbang Siap. |
