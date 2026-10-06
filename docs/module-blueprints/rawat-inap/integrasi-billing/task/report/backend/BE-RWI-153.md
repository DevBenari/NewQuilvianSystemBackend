# Laporan Perubahan Backend — `BE-RWI-153`

## Pembaruan eksekusi menyeluruh — 5 Oktober 2026

Bagian ini adalah bukti terbaru. Bagian laporan sebelumnya, bila ada, dipertahankan sebagai riwayat.

### Metadata eksekusi terakhir

| Field | Nilai |
| --- | --- |
| Task | `BE-RWI-153` — Keluar ruangan berjejak dan gerbang penutupan |
| Roadmap / slice | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-153` |
| Tanggal / mode | 5 Oktober 2026 / BACKEND |
| Status | SELESAI KODE — build dan validasi terbatas PASS; UAT klinis lengkap NOT RUN |
| Branch / upstream | `MHamzah` / `origin/MHamzah`; sesuai manifest |
| SHA awal sesi | `0a10899435bcd4cd54e998a060465589b6652643` |
| Kontrak / trace | `RWI-BP-001` revision 8; API `1.1.0 approved`; API 3.2 (`record-departure`, `closure-readiness`, `close`, `close-with-override`), 3.5 (dua daftar pantau); state 5; validasi 2 (`INP-DEP-001`) |
| Wewenang | Pengguna meminta seluruh BE-RWI-146–159, build dan migration pada akhir; database update development diotorisasi roadmap 5 Oktober 2026 |
| Target tulis | NewQuilvianSystemBackend; source dalam slice task, migration I6, laporan, roadmap dan traceability integrasi-billing |
| Dependency | `BE-RWI-146`, `BE-RWI-149`, `BE-RWI-152`; source/schema dependency tersedia; bukti klinis lengkap tidak diturunkan dari status kode |
| Area / Module / prefix | HealthServices / InPatientManagement (Inp) dan BillingManagement (Bil), ACTIVE |
| Applicability | Review ulang source existing; tidak ada perubahan source task ini pada sesi terbaru; LEGACY MIGRATION NOT APPLICABLE |
| Klasifikasi | `HEAVY` — skor 12: repository 0, berkas diperiksa 2, berkas diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| QBE applicable | QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-VAL-001, QBE-TXN-001, QBE-LOG-001, QBE-AUD-001 |
| Governance | AGENTS.md, engineering repository, dan rules/backend suite terbaca; engineering identik dengan suite, registry repository lebih baru pada domain lain; Bil/Inp/Phm sama ACTIVE. Repository canonical mengikuti AGENTS.md |
| Model | Codex, GPT-6 |

### Masalah, proses bisnis, dan perubahan

Pasien boleh keluar ruangan setelah petugas mengakui status kasir yang belum bersih. Sistem menyimpan status saat keluar dan mengosongkan bed. Penutupan normal tetap membutuhkan `CLEARED`; override membutuhkan permission dan alasan. Versi episode mendukung konflik `ExpectedVersion`.

Tidak ada perubahan source baru untuk task ini. `InpEpisode.Version` telah diterapkan oleh migration dependency sebelumnya; delta terhadap kamus data 6.3 tetap terbuka.

Berkas bukti:

- `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`
- `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs`

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kontrak existing dipertahankan sesuai kartu; perubahan removal yang disetujui tetap berlaku |
| Database | Kolom Version existing sudah diterapkan dalam AddRawatInapKeperawatanFinishing; tidak membuat migration ulang |
| Audit / keamanan | Database audit tetap melalui IdentityModel/SaveChanges existing; application log terpisah. Hak akses mengikuti atribut Access; tidak ada wewenang dari nama peran |
| Consumer frontend | Frontend dibaca saja; consumer lama masih perlu mengikuti pemisahan Read/ViewAmount dan pencabutan route sesuai task pendamping |

### Endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/record-departure` | Catat keluar ruangan | `InpatientDischarge : RecordDeparture` |
| `GET` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/closure-readiness` | Baca gerbang penutupan | `InpatientDischarge : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/close` | Penutupan sesudah izin kasir | `InpatientDischarge : Close` |
| `POST` | `/api/v1/health-services/inpatient-management/discharges/{episodeId}/close-with-override` | Penutupan khusus beralasan | `InpatientDischarge : CloseOverride` |

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
| 1. Status `PENDING` + tanpa pengakuan → 409; dengan pengakuan → bed kosong dan status saat keluar tersimpan. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 2. `close` dengan status selain `CLEARED` → ditolak. | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 3. Izin dicabut sesudah disetujui → tombol penutupan kembali terkunci (`UAT-RWF-03`). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 4. Override tercatat dengan alasan dan status saat itu, dan muncul di laporan (`UAT-RWF-12`). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |
| 5. Daftar "pulang sebelum izin kasir" memuat status saat keluar dan status sekarang | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`; `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` |

Implementasi, build, migration development, dan laporan tersedia. DoD alur klinis lengkap tetap **belum terbukti** pada butir berstatus NOT RUN; status kode tidak sama dengan sign-off rilis.

### Catatan penutup

Delta `InpEpisode.Version` terhadap kamus data 6.3 sudah ada sebelum sesi dan tetap membutuhkan keputusan pemilik. Dokumen `rules/backend/TEST_POLICY.md` yang dirujuk roadmap tidak tersedia pada suite ini. Semua governance canonical wajib lainnya terbaca; validasi mengikuti AGENTS.md dan review rules yang tersedia. Tidak ada project test repository. Dokumen luar slice yang sudah berubah sebelum atau selama sesi tidak disentuh. Tidak ada stage, commit, push, deployment, atau replay produksi.

Status Git task: laporan M; source existing task diperiksa ulang. Laporan dibuat pada working tree yang belum di-commit. Interupsi: NONE. Langkah berikutnya: consumer frontend pendamping dan UAT klinis sesuai acceptance matrix.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-153` |
| Judul | Keluar ruangan berjejak dan gerbang penutupan |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-153` |
| Trace | `FR-RWF-005` s.d. `FR-RWF-008`; `RWI-DEC-186`, `RWI-DEC-187`; `UAT-RWF-03`, `UAT-RWF-12` |
| Contract version | `integrasi-billing` `1.1.0` **`approved`** (`RWI-DEC-221`): API 3.2 (`record-departure`, `closure-readiness`, `close`, `close-with-override`), 3.5 (dua daftar pantau); state 5; validasi 2 (`VAL-RWF-01` s.d. `05`) |
| Dependency | `BE-RWI-146` ✅, `BE-RWI-149` 🟡 (skema `I1` sudah diterapkan lewat migration gabungan; selisih hanya pengemasan), `BE-RWI-152` ✅ |
| Klasifikasi | `HEAVY` — skor 12: repository 0, berkas diperiksa 2, berkas diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` (dependency `BE-RWI-168` keperawatan, disetujui pengguna 5 Oktober 2026) |
| Target tulis | `Areas/HealthServices/InPatientManagement/**`, `Repositories/**`, `Program.cs`, `Migrations/**` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, perbaikan, build akhir, migration, database update, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kelima acceptance criteria terpetakan ke source; build akhir `PASS`; kolom `InpEpisode.Version` diterapkan ke database development. Satu delta kontrak dicatat untuk keputusan pemilik (bagian 3.3) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement`; Billing dibaca lewat `IInpBillingClearanceAdapter`; penutupan alat memanggil `ClinicalManagement` sesudah commit |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (`InpDischargeService.Departure.cs`, DTO keberangkatan); `TOUCHED LEGACY` (penutupan, controller, monitoring, `InpEpisode`, `ApplicationDbContext`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CFG-002` |
| Wewenang | Source: ya. Migration: ya (`AGENTS.md` 5 Oktober 2026). Database development: ya, instruksi pengguna 5 Oktober 2026. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Sebelumnya keluar ruangan dan penutupan episode bergantung pada salinan status kasir di Rawat Inap (`InpatientClearanceGateService`) dan penanda manual. Kasir bisa tanpa sengaja menahan pasien yang secara fisik sudah pulang, sementara episode dapat ditutup dengan status kasir yang basi. Tidak ada jejak status kasir saat pasien keluar, sehingga tim keuangan tidak tahu pasien mana yang pulang sebelum izin kasir.

## 2. Proses bisnis

**Tujuan.** Perawat dapat mencatat pasien meninggalkan ruangan tanpa ditahan kasir, dengan peringatan dan jejak status kasir; episode hanya ditutup normal bila izin kasir `CLEARED` (`RWI-DEC-186`).

**Pelaku.**

| Pelaku | Peran |
| --- | --- |
| Perawat bangsal | Mencatat keluar ruangan (`InpatientDischarge : RecordDeparture`) |
| Petugas penutupan | Menutup normal (`InpatientDischarge : Close`) |
| Pemegang `CloseOverride` | Menutup tanpa izin kasir dengan alasan (`BE-RWI-146`) |
| Kasir/Billing | Pemilik tunggal status izin pulang |
| Tim keuangan | Membaca daftar pulang sebelum izin dan daftar penutupan tanpa izin |

**Langkah utama — keluar ruangan.**

1. Episode harus `DischargePending` (DPJP sudah menyatakan boleh pulang) dan belum tercatat keluar; baris episode dikunci.
2. Server memeriksa waktu keluar: tidak di masa depan, tidak sebelum keputusan pulang, tidak sebelum penempatan bed, dan tidak sebelum pemakaian alat yang masih berjalan.
3. Server membaca status kasir lewat adapter. Bila bukan `CLEARED` atau tidak terbaca dan peringatan belum diakui, server menjawab `409` `INP-DEP-001` tanpa mengubah apa pun.
4. Dengan pengakuan, dalam satu transaksi server melepas bed, mengantrekan `BED_RELEASED`, mencatat waktu dan pelaku keluar, serta menyimpan status kasir yang teramati beserta waktunya.
5. Sesudah commit, pemakaian alat yang masih berjalan ditutup (`BE-RWI-168`); kegagalannya hanya menjadi peringatan dan dipulihkan worker.

**Langkah utama — penutupan.**

1. `close` mengunci episode, mencocokkan `ExpectedVersion`, lalu membaca status kasir langsung. Status selain `CLEARED` ditolak `422` `INP-CLS-010`; Billing tidak terbaca ditolak `422` `INP-CLS-011`.
2. Syarat penutupan lain tetap berlaku; status kasir saat penutupan disimpan.
3. `close-with-override` melewati syarat kasir saja, dengan alasan, dan menyimpan status kasir saat itu.

**Contoh berangka (`UAT-RWF-03`, `UAT-RWF-12`).** DPJP menyatakan pasien Budi boleh pulang pukul 09.00. Pukul 11.00 status kasir masih `PENDING`. Perawat mencatat keluar tanpa pengakuan → `409` `INP-DEP-001` "Kasir belum memberi izin pulang…". Dikirim ulang dengan pengakuan → `200`, bed langsung kosong, tercatat "keluar 11.00, status kasir `Pending`". Pukul 13.00 kasir memberi `CLEARED`, lalu pukul 13.30 obat susulan dari Farmasi membuat izin `REVOKED`; `closure-readiness` kembali menunjukkan syarat kasir belum terpenuhi dan `close` ditolak `INP-CLS-010`. Pukul 15.00 izin `CLEARED` lagi; `close` dengan `ExpectedVersion` terbaru berhasil dan status `Cleared` tersimpan. Daftar "pulang sebelum izin kasir" menampilkan Budi dengan status saat keluar `Pending` dan status sekarang `CLEARED`.

**Perubahan status episode.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `DischargePending` | Keluar ruangan | `DischargePending` (tetap) | `RecordDeparture` | Pengakuan peringatan bila kasir bukan `CLEARED` |
| `DischargePending` | Tutup normal | `Closed` | `Close` | Kasir `CLEARED`, syarat lain terpenuhi, versi cocok |
| `DischargePending` | Tutup dengan override | `Closed` (tanpa izin kasir) | `CloseOverride` | Alasan jelas, syarat lain terpenuhi, versi cocok |

**Jalur tidak normal.** Kepergian tidak dapat dibatalkan (`RWI-RULE-036`); pasien yang belum jadi pulang menjalani admisi baru. Penutupan dengan versi basi ditolak `409`. Kegagalan penutupan alat tidak membatalkan kepergian.

**Hasil akhir.** Billing menjadi satu-satunya sumber izin pulang; setiap kepergian dan penutupan membawa jejak status kasir.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 3.2 dan 3.5, state 5, validasi 2, kamus data 6.3 (`InpEpisode`), `InpDischargeService` (keputusan, penutupan, keberangkatan lama), `InpBedOccupancyService.ReleaseActivePlacementAsync`, `IInpIntegrationOutboxService`, `IInpBillingClearanceAdapter`, `InpatientMonitoringController`, `InpCensusQueryService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs` | Baru. Keluar ruangan dengan kunci, peringatan `INP-DEP-001`, pelepasan bed, `BED_RELEASED`, jejak status kasir, penutupan alat sesudah commit; daftar pulang sebelum izin. Review Claude: pesan disamakan dengan `VAL-RWF-01`/`02`; dokumentasi method dipindahkan dari berkas penutupan |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientDepartureDtos.cs` | Baru. Respons keberangkatan dan item daftar pantau |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Syarat kasir dibaca langsung; `INP-CLS-010`/`011`; `ExpectedVersion` dengan kunci `FOR UPDATE`; status kasir saat penutupan disimpan; keberangkatan lama dipindahkan. Review Claude: pesan disamakan dengan `VAL-RWF-03`/`04`; komentar XML yatim dirapikan |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Dependency adapter status kasir, outbox, scope factory, logger |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | `record-departure` mengembalikan `InpatientDepartureResponse`; kegagalan membawa kode |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientClosureDtos.cs` | `CloseEpisodeRequest.ExpectedVersion`; `RecordDepartureRequest.ClearanceWarningAcknowledged` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs`, `DTOs/InpatientMonitoringDtos.cs`, `Services/InpCensusQueryService.cs` | Daftar `departures-before-clearance`; `ClosureClearanceObserved` pada daftar penutupan tanpa izin |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`, `Repositories/Configurations/HealthServices/InPatientManagement/InpEpisodeConfiguration.cs` | Kolom `Version` (bawaan 1). Review Claude: **bukan** concurrency token |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs`, `Services/InpEpisodeService.cs` | `Version` pada detail episode; hasil operasi membawa `Departure` |
| `Repositories/ApplicationDbContext.cs` | Hook `SaveChanges` menaikkan `InpEpisode.Version` setiap episode berubah |
| `Areas/HealthServices/InPatientManagement/Services/IInpatientClearanceGateService.cs`, `InpatientClearanceGateService.cs`, registrasinya di `Program.cs` | **Dihapus** |
| Migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` | `AddColumn InpEpisode.Version integer not null default 1` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `record-departure` kini dapat menjawab `409` `INP-DEP-001` dan mengembalikan `InpatientDepartureResponse`; `close`/`close-with-override` mewajibkan `ExpectedVersion`; daftar pantau baru `departures-before-clearance`. Seluruhnya sesuai API 3.2/3.5 |
| Database | Kolom baru `InpEpisode.Version`. **Delta kontrak:** kamus data 6.3 menyatakan "tidak ada kolom versi baru", sedangkan API 3.2 mewajibkan `ExpectedVersion`. Agar `ExpectedVersion` bermakna, kolom ditambahkan dan dinaikkan setiap episode berubah, tetapi sengaja **tidak** dijadikan concurrency token supaya alur lama yang menyunting episode tidak berubah perilaku. Perlu diselaraskan pemilik blueprint lewat `manage-module-blueprint`. **Diterapkan** ke database development 5 Oktober 2026 (baris lama bernilai 1) |
| Keamanan/Auth | Tidak ada kemampuan baru selain daftar pantau di bawah `InpatientMonitoring : Read`. Salinan status kasir lokal tidak lagi menentukan apa pun |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{episodeId}/record-departure` | Satu-satunya tindakan keluar ruangan; bed lepas seketika dan status kasir tercatat | `InpatientDischarge : RecordDeparture` | `RecordDepartureRequest { DepartedAt?, ClearanceWarningAcknowledged }` | `InpatientDepartureResponse` |
| `GET` | `/{episodeId}/closure-readiness` | Syarat penutupan; syarat kasir dibaca langsung dari Billing | `InpatientDischarge : Read` | — | `ClosureReadinessResponse` |
| `POST` | `/{episodeId}/close` | Penutupan normal; wajib `CLEARED` | `InpatientDischarge : Close` | `CloseEpisodeRequest { ExpectedVersion, Note? }` | `InpatientEpisodeDetailResponse` |
| `POST` | `/{episodeId}/close-with-override` | Penutupan tanpa izin kasir (lihat `BE-RWI-146`) | `InpatientDischarge : CloseOverride` | `CloseEpisodeOverrideRequest { Reason, ExpectedVersion }` | `InpatientEpisodeDetailResponse` |

Kode status `record-departure`: `200` kepergian tercatat; `400` waktu tidak sah; `404` episode tidak ditemukan; `409` `INP-DEP-001` kasir belum memberi izin atau tidak terbaca (tampilkan peringatan lalu kirim ulang dengan pengakuan), atau kepergian sudah dicatat; `422` keputusan pulang belum ada. Kode status `close`: `400` `ExpectedVersion` kosong; `409` versi berubah; `422` `INP-CLS-010` kasir belum memberi izin, `INP-CLS-011` status kasir tidak dapat dibaca, atau syarat lain belum terpenuhi.

#### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/departures-before-clearance` | Pasien pulang sebelum izin kasir, dengan status saat keluar dan status sekarang | `InpatientMonitoring : Read` | `DepartureBeforeClearanceQuery` | `PagedResult<DepartureBeforeClearanceItem>` |
| `GET` | `/closures-without-financial-clearance` | Episode ditutup dengan override, kini memuat status kasir saat penutupan | `InpatientMonitoring : Read` | Query yang sudah ada | Daftar `OverrideClosureItemResponse` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir; CS1587 akibat komentar yatim sudah dirapikan |
| `dotnet ef migrations add` dan `dotnet ef database update --no-build` | `AddColumn` `InpEpisode.Version` bawaan 1 di migration `20261005071042`; diterapkan `Done.`; nol `Pending` | `PASS` | Keluaran perintah |
| Pemeriksaan concurrency | Tidak ada penangkap `DbUpdateConcurrencyException` di `InPatientManagement`; karena itu token dilepas dan penutupan memakai kunci `FOR UPDATE` + pemeriksaan versi eksplisit | `PASS` | Pencarian source; `InpDischargeService.Closure.cs` `CloseEpisodeInternalAsync` |
| Pemeriksaan statis atribut hak akses | Endpoint discharge dan monitoring cocok | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — `UAT-RWF-03` | `PENDING` tanpa pengakuan → `409`; dengan pengakuan → bed lepas, `DepartureClearanceObserved` tersimpan; `REVOKED` → `INP-CLS-010` | `PASS` | `RecordPatientDepartureAsync`; `CloseEpisodeInternalAsync`; `BuildClosureConditionsAsync` syarat 4 |
| Verifikasi proses bisnis — `UAT-RWF-12` | Override menyimpan `ClosureClearanceObserved` dan muncul pada `closures-without-financial-clearance` | `PASS` | `CloseEpisodeInternalAsync`; `InpCensusQueryService` |
| Uji runtime | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Status `PENDING` + tanpa pengakuan → 409; dengan pengakuan → bed kosong dan status saat keluar tersimpan | Terpenuhi | `RecordPatientDepartureAsync` |
| 2. `close` dengan status selain `CLEARED` → ditolak | Terpenuhi | `CloseEpisodeInternalAsync` (`INP-CLS-010`/`011`) |
| 3. Izin dicabut sesudah disetujui → tombol penutupan kembali terkunci | Terpenuhi | `closure-readiness` membaca Billing langsung (`BuildClosureConditionsAsync`) |
| 4. Override tercatat dengan alasan dan status saat itu, dan muncul di laporan | Terpenuhi | `ClosureClearanceObserved`; daftar `closures-without-financial-clearance` |
| 5. Daftar "pulang sebelum izin kasir" memuat status saat keluar dan status sekarang | Terpenuhi | `GetDeparturesBeforeClearanceAsync` |
| Cakupan: hapus `InpatientClearanceGateService` | Terpenuhi | Berkas dan registrasi dihapus |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: uji runtime | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Hook `SaveChanges` di `ApplicationDbContext` menaikkan `Version` setiap kali episode berubah, dari alur mana pun |
| Masalah yang diketahui | **Perlu keputusan pemilik:** kolom `InpEpisode.Version` menyimpang dari kamus data 6.3 (lihat 3.3) |
| Risiko tersisa | Perubahan perilaku pulang terasa bagi perawat dan kasir; belum diuji runtime. Frontend lama tanpa `ExpectedVersion` akan mendapat `400` saat menutup |
| Perubahan sampingan | Review Claude: concurrency token pada `Version` dilepas untuk mencegah error 500 baru di alur Rawat Inap lain |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-167`, `FE-RWI-168`, `FE-RWI-169`; penyelarasan kamus data 6.3 |
