# Laporan Perubahan Backend — `BE-RWI-155`

## Pembaruan eksekusi menyeluruh — 5 Oktober 2026

Bagian ini adalah bukti terbaru. Bagian laporan sebelumnya, bila ada, dipertahankan sebagai riwayat.

### Metadata eksekusi terakhir

| Field | Nilai |
| --- | --- |
| Task | `BE-RWI-155` — Jembatan layanan klinis `RANAP`, biaya admin, dan finalisasi |
| Roadmap / slice | [backend-roadmap-finishing.md](../../../roadmap/backend-roadmap-finishing.md), kartu `BE-RWI-155` |
| Tanggal / mode | 5 Oktober 2026 / BACKEND |
| Status | SELESAI KODE — build dan validasi terbatas PASS; UAT klinis lengkap NOT RUN |
| Branch / upstream | `MHamzah` / `origin/MHamzah`; sesuai manifest |
| SHA awal sesi | `0a10899435bcd4cd54e998a060465589b6652643` |
| Kontrak / trace | `RWI-BP-001` revision 8; API `1.1.0 approved`; API 3.9 (`review-queue`, `review-resolution`); backend 9.6 |
| Wewenang | Pengguna meminta seluruh BE-RWI-146–159, build dan migration pada akhir; database update development diotorisasi roadmap 5 Oktober 2026 |
| Target tulis | NewQuilvianSystemBackend; source dalam slice task, migration I6, laporan, roadmap dan traceability integrasi-billing |
| Dependency | `BE-RWI-150`; source/schema dependency tersedia; bukti klinis lengkap tidak diturunkan dari status kode |
| Area / Module / prefix | HealthServices / BillingManagement (Bil), ACTIVE |
| Applicability | NEW CODE dan/atau TOUCHED LEGACY sesuai berkas; LEGACY MIGRATION NOT APPLICABLE |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 1, keamanan 1, workflow 1 |
| QBE applicable | QBE-SVC-001, QBE-VAL-001, QBE-TXN-001, QBE-LOG-001, QBE-AUD-001 |
| Governance | AGENTS.md, engineering repository, dan rules/backend suite terbaca; engineering identik dengan suite, registry repository lebih baru pada domain lain; Bil/Inp/Phm sama ACTIVE. Repository canonical mengikuti AGENTS.md |
| Model | Codex, GPT-6 |

### Masalah, proses bisnis, dan perubahan

Fakta layanan rawat inap diteruskan melalui folio ke invoice `RANAP`. Resep rawat inap yang belum diserahkan belum ditagih; penyerahan obat menjadi sumber tagihan dan MAR tidak menambahnya. Biaya admin termasuk hitungan kamar; finalisasi menolak invoice yang perlu review atau tarif belum ada.

Memperbaiki stage resep rawat inap sebelum penyerahan menjadi `NotApplicable`; alur tagih rawat jalan existing tetap dipakai.

Berkas bukti:

- `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs#BuildPlanAsync`
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs`

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
| 1. Tindakan "Pasang infus" `Completed` dan lab "Darah Lengkap" pasien rawat inap masuk invoice `RANAP` dengan harga master tarif (`AC-RWF-011`). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs#BuildPlanAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` |
| 2. Obat masuk saat diserahkan; MAR bukan sumber tagihan (`AC-RWF-090`, `091`). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs#BuildPlanAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` |
| 3. Biaya admin dihitung untuk invoice bertarif kamar (`AC-RWF-013`). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs#BuildPlanAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` |
| 4. Finalisasi ditolak bila `RequiresReview` (`BIL-FIN-020`) atau ada "tarif belum ada" (`BIL-FIN-021`). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs#BuildPlanAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` |
| 5. Penyelesaian pemeriksaan ditolak bila biaya kamar manual dan otomatis sama-sama aktif (`BIL-REV-001`). | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs#BuildPlanAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` |
| 6. Regresi rawat jalan: tindakan, lab, dan obat pasien rawat jalan menghasilkan invoice yang sama dengan sebelum perubahan | Implementasi terpenuhi pada review source; bukti alur klinis runtime NOT RUN | `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs#BuildPlanAsync`; `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` |

Implementasi, build, migration development, dan laporan tersedia. DoD alur klinis lengkap tetap **belum terbukti** pada butir berstatus NOT RUN; status kode tidak sama dengan sign-off rilis.

### Catatan penutup

Dokumen `rules/backend/TEST_POLICY.md` yang dirujuk roadmap tidak tersedia pada suite ini. Semua governance canonical wajib lainnya terbaca; validasi mengikuti AGENTS.md dan review rules yang tersedia. Tidak ada project test repository. Dokumen luar slice yang sudah berubah sebelum atau selama sesi tidak disentuh. Tidak ada stage, commit, push, deployment, atau replay produksi.

Status Git task: source/laporan M atau ?? sesuai berkas baru; tiga berkas tarif kamar lama D pada BE-RWI-147. Laporan dibuat pada working tree yang belum di-commit. Interupsi: NONE. Langkah berikutnya: consumer frontend pendamping dan UAT klinis sesuai acceptance matrix.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-155` |
| Judul | Jembatan layanan klinis `RANAP`, biaya admin, dan finalisasi |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-155` |
| Trace | `FR-RWF-011`, `012`, `015`; `RWI-DEC-192`, `195`; `INV-RWF-08`; `VAL-RWF-15` s.d. `17`; `AC-RWF-011`, `013`, `090`, `091`; API 3.9; backend 9.6; state 5.5 |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | `BE-RWI-150` ✅ |
| Klasifikasi | `HEAVY` — skor 11: repository 0, diperiksa 2, diubah 2, logika 2, kontrak API 2, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Billing/**` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026** |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Prefix registry | `Bil` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (jembatan, kalkulasi, finalisasi, invoice service, controller) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001` |
| Wewenang | Source: ya. Database: tidak |

---

## 1. Masalah yang diperbaiki

Jembatan layanan klinis Billing hanya menagih kunjungan rawat jalan, sehingga tindakan, lab,
radiologi, obat, dan konsultasi pasien rawat inap harus diinput kasir. Biaya admin rawat inap
belum memperhitungkan tarif kamar, dan invoice dapat difinalkan walau ada biaya kamar dobel atau
layanan bertarif kosong.

## 2. Proses bisnis

1. Jembatan menerima kunjungan `Outpatient` **dan** `Inpatient` dengan titik tagih yang sama;
   tujuan tagihan adalah invoice kunjungan itu sendiri (`RANAP` untuk rawat inap).
2. Contoh `AC-RWF-011`: tindakan "Pasang infus" `Completed` dan lab "Darah Lengkap" pasien rawat
   inap masuk invoice `RANAP` dengan harga master tarif. Obat masuk saat `DISPENSED`; MAR bukan
   domain sumber tagihan (`AC-RWF-090`, `091`).
3. Kalkulasi invoice `RANAP` menghitung tarif kamar lebih dulu, lalu biaya admin dengan dasar
   jasa non-farmasi **termasuk** tarif kamar (`AC-RWF-013`).
4. Invoice `RANAP` yang memuat biaya kamar manual bersamaan dengan tarif kamar otomatis ditandai
   `RequiresReview` (`ReviewReasonCode` biaya kamar manual + otomatis).
5. Finalisasi ditolak bila `RequiresReview` (`BIL-FIN-020`) atau masih ada layanan "tarif belum ada"
   (`BIL-FIN-021`, termasuk segmen tarif kamar tanpa tarif) — juga lewat jalur departure exception.
6. Kasir membuka antrean "perlu diperiksa", membatalkan baris kamar yang dobel, lalu menyelesaikan
   pemeriksaan. Bila manual dan otomatis masih sama-sama aktif → 422 `BIL-REV-001`.
7. Pencatatan manual layanan klinis lewat `from-source` kini juga ditolak untuk kunjungan rawat
   inap, karena domainnya sudah dimiliki jembatan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingClinicalChargeBridgeService.cs`, `BillingSourceTariffResolver.cs`, `BillingChargeSourceAdapter.cs`,
`BillingCalculationService.cs`, `AdministrationFeeCalculationService.cs`, `BillingFinalizationService.cs`,
`BillingInvoiceService.cs`, `BillingInvoicesController.cs`; kontrak API 3.9, backend 9.6.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Services/BillingClinicalChargeBridgeService.cs` | Menerima `EncounterType.Inpatient` |
| `Billing/Services/BillingCalculationService.cs` | Tarif kamar sebelum biaya admin; dasar biaya admin + tarif kamar untuk `RANAP`; penandaan `RequiresReview`; `IsManualRoomChargeItem` |
| `Billing/Services/BillingFinalizationService.cs`, `Dtos/BillingFinalizationDtos.cs` | `BIL-FIN-020`, `BIL-FIN-021`; `BlockingCodes` pada kesiapan finalisasi |
| `Billing/Services/BillingInvoiceService.cs` | `GetReviewQueueAsync`, `ResolveReviewAsync` (`BIL-REV-001`); penjaga `from-source` meliputi rawat inap |
| `Billing/Dtos/BillingInvoiceDtos.cs` | `InvoiceReviewQueueQuery`, `InvoiceReviewItemResponse`, `ResolveInvoiceReviewRequest` |
| `Billing/Controllers/BillingInvoicesController.cs` | Dua endpoint API 3.9 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint API 3.9; kesiapan finalisasi bertambah `BlockingCodes` (aditif) |
| Database | Memakai kolom `I2` pada `BilInvoice` |
| Keamanan/Auth | Memakai `BillingInvoice : Read` dan `: Update` yang sudah ada, sesuai kontrak |

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/review-queue` | Antrean invoice "perlu diperiksa" | `BillingInvoice : Read` |
| `POST` | `/{id}/review-resolution` | Menyelesaikan pemeriksaan; 422 `BIL-REV-001` | `BillingInvoice : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review jalur rawat jalan (regresi) | Kondisi hanya diperluas; logika titik tagih tidak berubah | `PASS` | Review diff |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Proses bisnis per jenis layanan dengan contoh berangka | Belum dijalankan | `NOT RUN` | Butuh build dan database termigrasi |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tindakan dan lab rawat inap masuk invoice `RANAP` dengan harga master | Terpenuhi (source) | Jembatan menerima `Inpatient`; `BillingSourceTariffResolver` |
| 2. Obat saat diserahkan; MAR bukan sumber | Terpenuhi (source) | Kebijakan `PHARMACY` `DISPENSED`; tidak ada domain MAR |
| 3. Biaya admin untuk invoice bertarif kamar | Terpenuhi (source) | `CalculateAdministrationFeeAsync` |
| 4. Finalisasi ditolak `BIL-FIN-020`/`021` | Terpenuhi (source) | `BillingFinalizationService` |
| 5. Penyelesaian ditolak `BIL-REV-001` | Terpenuhi (source) | `ResolveReviewAsync` |
| 6. Regresi rawat jalan tidak berubah | Terpenuhi (source, review diff) | Belum dibuktikan runtime |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pesan penolakan `from-source` berubah karena kini meliputi rawat inap |
| Masalah yang diketahui | `NONE`. Kedua endpoint memakai kunci `Read`/`Update` yang sudah ada, mengikuti pola controller ini (banyak action berbagi kunci yang sama) |
| Risiko tersisa | Titik tagih rawat inap disamakan dengan rawat jalan; regresi rawat jalan perlu diuji runtime |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??`. `BillingInvoiceService.cs` kini juga diubah `BE-RWI-179` (belum di-commit) |
| Langkah berikutnya | Uji per jenis layanan dan regresi rawat jalan sesudah build |
