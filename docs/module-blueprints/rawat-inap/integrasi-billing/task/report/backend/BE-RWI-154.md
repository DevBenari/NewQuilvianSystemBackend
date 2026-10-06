# Laporan Perubahan Backend — `BE-RWI-154`

## Pembaruan eksekusi menyeluruh — 5 Oktober 2026

Bagian ini adalah bukti terbaru. Bagian laporan sebelumnya, bila ada, dipertahankan sebagai riwayat.

### Metadata eksekusi terakhir

| Field | Nilai |
| --- | --- |
| Task | `BE-RWI-154` — Koreksi salah catat penempatan |
| Roadmap / slice | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-154` |
| Tanggal / mode | 5 Oktober 2026 / BACKEND |
| Status | SELESAI KODE — build dan validasi terbatas PASS; UAT klinis lengkap NOT RUN |
| Branch / upstream | `MHamzah` / `origin/MHamzah`; sesuai manifest |
| SHA awal sesi | `0a10899435bcd4cd54e998a060465589b6652643` |
| Kontrak / trace | `RWI-BP-001` revision 8; API `1.1.0 approved`; API 3.4 (`POST placements/{placementId}/corrections`, `InpatientBedOccupancy : Correct`; `placements/by-episode` bertambah tiga field; transfer menerbitkan `BED_OCCUPIED`); state 5 |
| Wewenang | Pengguna meminta seluruh BE-RWI-146–159, build dan migration pada akhir; database update development diotorisasi roadmap 5 Oktober 2026 |
| Target tulis | NewQuilvianSystemBackend; source dalam slice task, migration I6, laporan, roadmap dan traceability integrasi-billing |
| Dependency | `BE-RWI-151`; source/schema dependency tersedia; bukti klinis lengkap tidak diturunkan dari status kode |
| Area / Module / prefix | HealthServices / InPatientManagement (Inp) dan BillingManagement (Bil), ACTIVE |
| Applicability | Review ulang source existing; tidak ada perubahan source task ini pada sesi terbaru; LEGACY MIGRATION NOT APPLICABLE |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 1, keamanan 1, workflow 1 |
| QBE applicable | QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-VAL-001, QBE-TXN-001, QBE-AUD-001 |
| Governance | AGENTS.md, engineering repository, dan rules/backend suite terbaca; engineering identik dengan suite, registry repository lebih baru pada domain lain; Bil/Inp/Phm sama ACTIVE. Repository canonical mengikuti AGENTS.md |
| Model | Codex, GPT-6 |

### Masalah, proses bisnis, dan perubahan

Petugas mengoreksi salah catat penempatan dengan alasan. Invoice FINAL menolak koreksi; invoice OPEN membentuk baris koreksi baru dan menandai baris lama. Hitungan Billing mengabaikan penempatan superseded. Pindah biasa dan koreksi tetap berbeda pada event dan riwayat.

Tidak ada perubahan source baru untuk task ini; konfigurasi FK dan hitungan existing diperiksa ulang.

Berkas bukti:

- `Areas/HealthServices/InPatientManagement/Services/InpPlacementCorrectionService.cs`
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs`

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kontrak existing dipertahankan sesuai kartu; perubahan removal yang disetujui tetap berlaku |
| Database | Tidak menambah schema pada task ini; dependency I1/I2/E6/I6 telah diterapkan |
| Audit / keamanan | Database audit tetap melalui IdentityModel/SaveChanges existing; application log terpisah. Hak akses mengikuti atribut Access; tidak ada wewenang dari nama peran |
| Consumer frontend | Frontend dibaca saja; consumer lama masih perlu mengikuti pemisahan Read/ViewAmount dan pencabutan route sesuai task pendamping |

### Endpoint

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/placements/{placementId}/corrections` | Koreksi salah catat penempatan | `InpatientBedOccupancy : Correct` |

### Verifikasi terbaru

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE Strict WorkingTree akhir | 27 file; VIOLATION 0, REVIEW 0, INFO 0 | PASS |
| Build awal | Dua CS1061 replay (`Status`); diperbaiki menjadi `EpisodeStatus` | NEW ERROR — sudah diperbaiki |
| `dotnet build QuilvianSystemBackend.csproj --nologo --no-restore` | Build akhir mencakup semua source dan metadata migration; 0 Error(s), 233 Warning(s), 00:05:31.27, exit 0 | PASS |
| `dotnet ef migrations add AddRawatInapBillingEncounterLink --project QuilvianSystemBackend.csproj --no-build` | Migration I6 dibuat setelah implementasi source lengkap; scope Up hanya tabel tautan | PASS |
| `dotnet ef database update --project QuilvianSystemBackend.csproj --no-build` dengan Development | Applying I6, Done., exit 0; pemeriksaan `GetPendingMigrationsAsync` menghasilkan 0 | PASS |
| Konfirmasi database setelah perbaikan penutup | Database already up to date, Done., exit 0; tidak ada migration tambahan | PASS |
| Verifikasi runtime terbatas terhadap DLL akhir | 35 pemeriksaan PASS: serialisasi, 11 metadata action, filter 401/403, whitelist, DeadLetter, EF model, pending migration, DryRun, query rincian | PASS |
| DryRun database development | 3 kandidat; data outbox dan RequiresReview/RowVersion sebelum/sesudah identik | PASS |
| Pembacaan rincian tersimpan | Breakdown dan aggregate konsisten pada 3 episode development; tidak memanggil kalkulasi/tulis | PASS |
| Sampel penyerahan resep untuk retur | 0 prescription sumber ditemukan; perhitungan retur nyata tidak dibuktikan | NOT RUN |
| UAT klinis lengkap, akun HTTP nyata, replay tulis, gangguan worker, regresi rawat jalan dan rollback Down | Tidak dijalankan; hasil pemeriksaan terbatas tidak menjadi bukti skenario ini | NOT RUN |
| QBE percobaan ulang | Git stderr peringatan CRLF memenuhi pipe checker dan menahan proses; dipulihkan dengan core.safecrlf=false hanya pada environment proses, tanpa mengubah konfigurasi repository atau mode Strict | EXISTING / ENVIRONMENT ISSUE, dipulihkan |
| Warning compiler dan harness | 233 warning aplikasi; harness sementara menampilkan MSB3277 DependencyModel 9.0.12/9.0.18; tidak mengubah package aplikasi | EXISTING / ENVIRONMENT ISSUE — warning aplikasi tidak seluruhnya dibuktikan sebagai baseline |



Uji manual: **PASS terbatas**. Principal filter berupa identitas sintetis bernama Cashier tanpa ID pengguna valid; hasil 403 membuktikan filter berjalan, bukan UAT akun rumah sakit dengan role-permission nyata. Pemeriksaan dijalankan melalui console sementara di luar repository; tidak ada task/project automated test baru. Database memakai konfigurasi Development, nama database bermarker development dan tanpa marker produksi; host remote, bukan loopback. Connection string dan data pasien tidak dicatat.

Bukti output lokal (`%TEMP%` = `C:/Users/Admin/AppData/Local/Temp`):

| File | SHA256 |
| --- | --- |
| `Quilvian-billing-finishing-final-build.log` | `7B241A1273B063C92AA2F5043DC119557AA202166AC296A66478215DD360FA41` |
| `Quilvian-billing-finishing-database-update.log` | `6FE5D4EFB20FA0C5267EFDE2783F8698FEC643E2BE47982E9097DB5CDA894B14` |
| `Quilvian-billing-finishing-database-final-check.log` | `DA95D2A434379F615CAE4C85266F2E21C49E4E8A120A4CE9F9F5EBEEFE2DFA7F` |
| `Quilvian-billing-finishing-runtime-final.log` | `9322A732FA830AA4F40F9BB9BD53172B1D7F6C8D35D2436C85E70B7932A75F66` |
| `Quilvian-billing-finishing-qbe-final.log` | `D9FB75C1901806EBA0CF386A83275FCB4598223561A895830C327013D9FB47DF` |

### Acceptance criteria dan Definition of Done terbaru

| Kriteria roadmap | Status | Bukti |
| --- | --- | --- |
| 1. Koreksi saat invoice `FINAL` → 422 "tagihan sudah difinalkan". | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpPlacementCorrectionService.cs`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` |
| 2. Koreksi membuat baris baru dan menandai baris lama, alasan wajib. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpPlacementCorrectionService.cs`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` |
| 3. Transfer dan koreksi dapat dibedakan pada riwayat. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpPlacementCorrectionService.cs`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` |
| 4. Skenario `RSK-RWF-02`: pindah kamar lalu koreksi salah catat — tarif kamar per hari tidak dobel dan tidak kurang (contoh berangka di laporan) | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpPlacementCorrectionService.cs`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` |

Implementasi, build, migration development, dan laporan tersedia. DoD alur klinis lengkap tetap **belum terbukti** pada butir berstatus NOT RUN; status kode tidak sama dengan sign-off rilis.

### Catatan penutup

Dokumen `rules/backend/TEST_POLICY.md` yang dirujuk roadmap tidak tersedia pada suite ini. Semua governance canonical wajib lainnya terbaca; validasi mengikuti AGENTS.md dan review rules yang tersedia. Tidak ada project test repository. Dokumen luar slice yang sudah berubah sebelum atau selama sesi tidak disentuh. Tidak ada stage, commit, push, deployment, atau replay produksi.

Status Git task: laporan M; source existing task diperiksa ulang. Laporan dibuat pada working tree yang belum di-commit. Interupsi: NONE. Langkah berikutnya: consumer frontend pendamping dan UAT klinis sesuai acceptance matrix.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-154` |
| Judul | Koreksi salah catat penempatan |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-154` |
| Trace | `FR-RWF-019`; `RWI-DEC-157`, `192` (g); `RSK-RWF-02`; `INV-RWF-07`; `INT-RWF-03`; `UAT-RWF-25`; API 3.4; state 5.4; `VAL-RWF-07` s.d. `12` |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-151` ✅ |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/**`, `BillingCalculationService.cs`, `InpatientClearanceService.cs`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement`; bacaan Billing lewat adapter |
| Prefix registry | `Inp`, `Bil` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (correction service, adapter); `TOUCHED LEGACY` (`InpBedOccupancyService`, controller, `BillingCalculationService`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Salah catat kamar, bed, kelas, atau waktu penempatan tidak dapat diperbaiki tanpa memalsukan
transfer, dan penanda `IsSuperseded` dipakai bersama transfer dan koreksi sehingga tarif kamar
berisiko dobel atau hilang.

## 2. Proses bisnis

1. Pemegang `InpatientBedOccupancy : Correct` mengirim koreksi dengan alasan wajib, minimal satu
   field (bed, kelas, waktu mulai/selesai), dan `ExpectedVersion`.
2. Status invoice dibaca langsung dari Billing lewat `IInpBillingClearanceAdapter`. Tak terbaca →
   422 `INP-COR-002`; bukan `OPEN`/`NONE` → 422 `INP-COR-001` "tagihan sudah difinalkan".
3. Koreksi membuat baris baru (`CorrectsPlacementId`), baris lama berakhir `CorrectedEntry` dan
   diberi `SupersededByCorrectionId`. Versi berubah → 409 `INP-COR-003`; bed tujuan tidak layak →
   422 `INP-COR-004`. Event `OCCUPANCY_CORRECTED` diantrekan.
4. Transfer biasa kini menerbitkan `BED_OCCUPIED`; `OCCUPANCY_CORRECTED` hanya milik koreksi.
5. Billing mengecualikan baris yang `SupersededByCorrectionId`-nya terisi; `IsSuperseded` (diakhiri
   transfer) **tidak** dipakai sebagai saringan.
6. Contoh `RSK-RWF-02` (hitungan dari source): Budi kelas 2 sejak 1 Okt 08.00, pindah ke kamar
   kelas 1 pada 3 Okt 08.00, lalu kepala ruangan mengoreksi baris kelas 1 menjadi VIP. Hasil:
   1–3 Okt dihitung kelas 2 (2 hari, baris diakhiri transfer tetap tertagih), 3 Okt dan seterusnya
   dihitung VIP; baris kelas 1 tersimpan sebagai jejak tetapi tidak tertagih — tidak dobel, tidak
   kurang.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpBedOccupancyService.cs` (transfer, linimasa), `InpatientBedOccupancyController.cs`,
`BillingCalculationService.cs` (`CalculateRoomChargeAsync`), `InpatientClearanceService.cs`,
`InpBillingDepositAdapter.cs` (pola adapter), kontrak API 3.4.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Services/InpPlacementCorrectionService.cs` | Baru — validasi, gerbang invoice, kode `INP-COR-001` s.d. `004` |
| `InPatientManagement/Services/InpBedOccupancyService.cs` | `ApplyPlacementCorrectionAsync`; transfer menerbitkan `BED_OCCUPIED` |
| `InPatientManagement/Services/IInpBillingClearanceAdapter.cs`, `InpBillingClearanceAdapter.cs` | Baru — pembaca status kasir/invoice gagal-tertutup |
| `BillingManagement/Billing/Services/IInpatientClearanceService.cs`, `InpatientClearanceService.cs`, `Dtos/InpatientClearanceDtos.cs` | `GetLatestStatusAsync` → `InpatientClearanceStatusView` (status, kendala, `InvoiceStatus`) |
| `InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | `POST placements/{placementId}/corrections` |
| `InPatientManagement/DTOs/InpatientCorrectionDtos.cs`, `InpatientBedOccupancyDtos.cs` | `CorrectPlacementRequest`; `placements/by-episode` + `CorrectsPlacementId`, `SupersededByCorrectionId`, `IsCorrection` |
| `InPatientManagement/Enums/InpBedPlacementEndReason.cs` | `CorrectedEntry = 5` |
| `BillingManagement/Billing/Services/BillingCalculationService.cs` | Saringan `SupersededByCorrectionId == null` |
| `Program.cs` | Registrasi adapter dan correction service |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru API 3.4; tiga field aditif pada `placements/by-episode` |
| Database | Memakai kolom `I1` |
| Keamanan/Auth | Permission baru `InpatientBedOccupancy : Correct` |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/health-services/inpatient-management/bed-occupancies/placements/{placementId}/corrections` | Koreksi salah catat selama invoice `OPEN`/belum terbentuk | `InpatientBedOccupancy : Correct` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review atribut `[AccessAction("Correct")]`/`[AccessPermission("InpatientBedOccupancy", "Correct")]` | Cocok | `PASS` | Controller |
| Review saringan tarif kamar | Hanya `SupersededByCorrectionId` | `PASS` | `CalculateRoomChargeAsync` |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Skenario `RSK-RWF-02` dengan data | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Invoice `FINAL` → 422 "tagihan sudah difinalkan" | Terpenuhi (source) | `INP-COR-001` |
| 2. Baris baru, baris lama ditandai, alasan wajib | Terpenuhi (source) | `ApplyPlacementCorrectionAsync`, validasi alasan |
| 3. Transfer dan koreksi dapat dibedakan | Terpenuhi (source) | `IsCorrection`, `EndReason` `Transfer` vs `CorrectedEntry` |
| 4. `RSK-RWF-02` tidak dobel, tidak kurang | Terpenuhi (source) | Contoh hitungan bagian 2 butir 6 |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Adapter status kasir dibuat di sini untuk gerbang invoice; cakupan penuh `BE-RWI-152` (endpoint `billing-status`, penghapusan `billing-details`) **belum** dikerjakan |
| Masalah yang diketahui | Invoice belum terbentuk (`NONE`) dianggap aman dikoreksi |
| Risiko tersisa | `IsSuperseded` tetap dipakai transfer; pembaca lain harus memakai `SupersededByCorrectionId` untuk koreksi |
| Perubahan sampingan | `InpatientClearanceService` bertambah `GetLatestStatusAsync` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??`. `InpBedOccupancyService.cs` kini juga diubah `BE-RWI-183` (belum di-commit) |
| Langkah berikutnya | Admin memberikan `InpatientBedOccupancy : Correct`; uji skenario `RSK-RWF-02` |
