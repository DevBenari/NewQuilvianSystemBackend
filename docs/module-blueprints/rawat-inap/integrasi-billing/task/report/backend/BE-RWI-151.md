# Laporan Perubahan Backend — `BE-RWI-151`

## Pembaruan eksekusi menyeluruh — 5 Oktober 2026

Bagian ini adalah bukti terbaru. Bagian laporan sebelumnya, bila ada, dipertahankan sebagai riwayat.

### Metadata eksekusi terakhir

| Field | Nilai |
| --- | --- |
| Task | `BE-RWI-151` — Outbox jujur |
| Roadmap / slice | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-151` |
| Tanggal / mode | 5 Oktober 2026 / BACKEND |
| Status | SELESAI KODE — build dan validasi terbatas PASS; UAT klinis lengkap NOT RUN |
| Branch / upstream | `MHamzah` / `origin/MHamzah`; sesuai manifest |
| SHA awal sesi | `0a10899435bcd4cd54e998a060465589b6652643` |
| Kontrak / trace | `RWI-BP-001` revision 8; API `1.1.0 approved`; Integrasi 4.2 (daftar putih, kunci idempotensi, kegagalan); backend 9.6, 9.11 (`ProcessingLeaseSeconds = 300`, `BatchSize = 50`, `MaxRetry = 10`) |
| Wewenang | Pengguna meminta seluruh BE-RWI-146–159, build dan migration pada akhir; database update development diotorisasi roadmap 5 Oktober 2026 |
| Target tulis | NewQuilvianSystemBackend; source dalam slice task, migration I6, laporan, roadmap dan traceability integrasi-billing |
| Dependency | `BE-RWI-150`; source/schema dependency tersedia; bukti klinis lengkap tidak diturunkan dari status kode |
| Area / Module / prefix | HealthServices / InPatientManagement (Inp) dan BillingManagement (Bil), ACTIVE |
| Applicability | NEW CODE dan/atau TOUCHED LEGACY sesuai berkas; LEGACY MIGRATION NOT APPLICABLE |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, diperiksa 1, diubah 2, logika 2, kontrak API 0, database 1, keamanan 1, workflow 1 |
| QBE applicable | QBE-SVC-001, QBE-VAL-001, QBE-TXN-001, QBE-LOG-001, QBE-AUD-001 |
| Governance | AGENTS.md, engineering repository, dan rules/backend suite terbaca; engineering identik dengan suite, registry repository lebih baru pada domain lain; Bil/Inp/Phm sama ACTIVE. Repository canonical mengikuti AGENTS.md |
| Model | Codex, GPT-6 |

### Masalah, proses bisnis, dan perubahan

Worker mengambil pesan, mengirimkannya ke receiver, dan hanya menandai `Published` bila hasil `Accepted` disertai `ReceiptId` yang tidak kosong. Kegagalan memakai backoff; sepuluh kegagalan menjadi `DeadLetter`. Pesan tetap delapan field daftar putih.

Memperketat syarat sukses worker menjadi `receipt.Accepted && receipt.ReceiptId != Guid.Empty`.

Berkas bukti:

- `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs#ProcessOutboxBatchAsync`
- `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs`

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kontrak existing dipertahankan sesuai kartu; perubahan removal yang disetujui tetap berlaku |
| Database | Tidak menambah schema pada task ini; dependency I1/I2/E6/I6 telah diterapkan |
| Audit / keamanan | Database audit tetap melalui IdentityModel/SaveChanges existing; application log terpisah. Hak akses mengikuti atribut Access; tidak ada wewenang dari nama peran |
| Consumer frontend | Frontend dibaca saja; consumer lama masih perlu mengikuti pemisahan Read/ViewAmount dan pencabutan route sesuai task pendamping |

### Endpoint

NOT APPLICABLE — task ini memproses service/event/schema tanpa endpoint HTTP baru. Endpoint existing yang tidak berubah dijelaskan pada bagian riwayat bila ada.

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
| 1. Field di luar daftar putih ditolak. | PASS — daftar putih menerima delapan field dan menolak tambahan SourceEncounterId | `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs#ProcessOutboxBatchAsync`; `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs` |
| 2. Tanpa tanda terima, pesan tidak pernah `Published`. | Terpenuhi pada source: Accepted dan ReceiptId tidak kosong; worker nyata NOT RUN | `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs#ProcessOutboxBatchAsync`; `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs` |
| 3. Billing mati → `Failed`, dicoba ulang dengan backoff. | Terpenuhi pada review source; gangguan Billing NOT RUN | `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs#ProcessOutboxBatchAsync`; `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs` |
| 4. Gagal 10 kali → `DeadLetter`. | PASS — sepuluh RecordFailure menghasilkan DeadLetter | `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs#ProcessOutboxBatchAsync`; `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs` |
| 5. Pesan `Processing` yang sewanya habis diambil ulang | Terpenuhi pada review source; sewa worker nyata NOT RUN | `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs#ProcessOutboxBatchAsync`; `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs` |

Implementasi, build, migration development, dan laporan tersedia. DoD alur klinis lengkap tetap **belum terbukti** pada butir berstatus NOT RUN; status kode tidak sama dengan sign-off rilis.

### Catatan penutup

Dokumen `rules/backend/TEST_POLICY.md` yang dirujuk roadmap tidak tersedia pada suite ini. Semua governance canonical wajib lainnya terbaca; validasi mengikuti AGENTS.md dan review rules yang tersedia. Tidak ada project test repository. Dokumen luar slice yang sudah berubah sebelum atau selama sesi tidak disentuh. Tidak ada stage, commit, push, deployment, atau replay produksi.

Status Git task: source/laporan M atau ?? sesuai berkas baru; tiga berkas tarif kamar lama D pada BE-RWI-147. Laporan dibuat pada working tree yang belum di-commit. Interupsi: NONE. Langkah berikutnya: consumer frontend pendamping dan UAT klinis sesuai acceptance matrix.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-151` |
| Judul | Outbox jujur |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-151` |
| Trace | `FR-RWF-014`, `016`; `INV-RWF-04`, `05`; `RWI-DEC-161`, `166`; integrasi 4.2; backend 9.6, 9.11; state 5.1 |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-150` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, diperiksa 1, diubah 2, logika 2, kontrak API 0, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/{Services,Models,Options,Workers}`, `Program.cs` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (outbox), memanggil `BillingManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (outbox service, worker, model); `NEW CODE` (options) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-LOG-001`, `QBE-SEC-001` (data minimum dalam pesan) |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Worker lama menandai pesan `Published` tanpa ada penerima, sehingga status "terkirim" tidak
membuktikan apa pun. Isi pesan juga membawa data pasien dan penjamin yang tidak dibutuhkan Billing,
dan pesan yang tersangkut `Processing` tidak pernah diambil ulang.

## 2. Proses bisnis

1. Pendaftaran pesan (`EnqueueEventAsync`) hanya menerima empat jenis event dan tiga `SourceType`;
   isi pesan disusun dari delapan field daftar putih dan diperiksa ulang `IsWhitelisted`. Field lain
   → ditolak.
2. Kunci idempotensi `INPATIENT:<SourceType>:<SourceId>:<Version>` (`RWI-DEC-161` butir 4).
3. Worker mengambil `BatchSize` (50) pesan `Pending`, `Failed` yang jatuh tempo, atau `Processing`
   yang sewanya (300 detik) habis; menandainya `Processing` + `ProcessingStartedAtUtc`.
4. Worker memanggil `BillingInpatientEventReceiver` dalam proses yang sama. `Accepted = true` →
   `Published` dengan `AcknowledgedReceiptId`. Selain itu → `Failed`, coba ulang dengan backoff
   `min(2^n × 5 detik, 3600 detik)`; sesudah `MaxRetry` (10) kali → `DeadLetter`.
5. Contoh: Billing mati saat Budi diadmisi → pesan `Failed` (percobaan 1, coba lagi ±10 detik),
   … ; begitu Billing hidup, percobaan berikutnya `Published`. Bila 10 kali gagal → `DeadLetter`
   untuk ditangani putar ulang (`BE-RWI-157`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpIntegrationOutboxService.cs`, `IInpIntegrationOutboxService.cs`, `InpatientIntegrationOutboxWorker.cs`,
`InpIntegrationOutbox.cs`, `OutboxStatus.cs`, seluruh pemanggil `EnqueueEventAsync`
(`InpEpisodeService`, `InpBedOccupancyService`, `InpatientClearanceGateService`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Services/IInpIntegrationOutboxService.cs`, `InpIntegrationOutboxService.cs` | Tanda tangan baru berbasis penanda kejadian; `InpIntegrationOutboxPayload` (daftar putih, kunci idempotensi, `IsWhitelisted`) |
| `InPatientManagement/Models/InpIntegrationOutbox.cs` | `MarkPublished(receiptId)`, `RecordFailure(…, maxRetry)` |
| `InPatientManagement/Options/InpatientIntegrationOutboxOptions.cs` | Baru — `ProcessingLeaseSeconds = 300`, `BatchSize = 50`, `MaxRetry = 10` |
| `InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs` | Sewa pemrosesan, panggilan penerima Billing, `Published` hanya bila diterima, backoff, `DeadLetter` |
| `InPatientManagement/Services/InpEpisodeService.cs` | `ADMISSION_CONFIRMED` tanpa data penjamin/pasien |
| `InPatientManagement/Services/InpBedOccupancyService.cs` | Event tempat tidur memakai tanda tangan baru |
| `InPatientManagement/Services/InpatientClearanceGateService.cs` | Hanya penyesuaian tanda tangan outbox (service ini dihapus `BE-RWI-153`) |
| `Program.cs` | `Configure<InpatientIntegrationOutboxOptions>` dari bagian `InpatientIntegrationOutbox` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Memakai kolom `I1` (`ProcessingStartedAtUtc`, `AcknowledgedReceiptId`) |
| Keamanan/Auth | Data pasien dan penjamin tidak lagi tersalin ke pesan (`INV-RWF-05`) |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — pemantauan outbox ada pada `BE-RWI-157`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review pemanggil `EnqueueEventAsync` | Semua memakai tanda tangan baru | `PASS` | Review source |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Proses bisnis lima kriteria dengan worker berjalan | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Field di luar daftar putih ditolak | Terpenuhi (source) | `IsWhitelisted`, `AllowedFields` |
| 2. Tanpa tanda terima pesan tidak pernah `Published` | Terpenuhi (source) | `MarkPublished` hanya bila `receipt.Accepted` |
| 3. Billing mati → `Failed` dengan backoff | Terpenuhi (source) | `RecordFailure`, rumus backoff worker |
| 4. Gagal 10 kali → `DeadLetter` | Terpenuhi (source) | `RetryCount >= maxRetry` |
| 5. `Processing` yang sewanya habis diambil ulang | Terpenuhi (source) | Saringan `ProcessingStartedAtUtc < leaseExpiredBefore` |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `appsettings.json` tidak diubah; nilai bawaan opsi ada di kode dan dapat ditimpa bagian `InpatientIntegrationOutbox` |
| Masalah yang diketahui | Kunci idempotensi `BED_RELEASED` kini berbasis penempatan (sebelumnya episode) |
| Risiko tersisa | Pesan lama yang dulu `Published` tanpa terkirim ditangani putar ulang `BE-RWI-157` |
| Perubahan sampingan | Isi `ADMISSION_CONFIRMED` tidak lagi memuat penjamin, pasien, dan waktu admisi |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??` |
| Langkah berikutnya | Uji worker dengan Billing hidup/mati sesudah build dan migration `I1`/`I2` |
