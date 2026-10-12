# Laporan Perubahan Backend — `BE-FIN-095`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-095` |
| Judul | Finance menerima laporan hasil potongan payroll HR secara otomatis dan idempoten, menumpuk sisa ke periode berikutnya |
| Slice | `REV-18B4` — Inbound Webhook Payroll & Koreksi Reversal |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-200`..`205`, `225`..`227`; `FIN-DEC-191`, `192`; `FIN-DES-100` |
| Contract version | `FIN-API-1.9` §P.1; `FIN-INTEGRATION-1.8` §P.3; `FIN-VAL-1.11` `FIN-VAL-240`, `248` (semuanya `draft`/sebagian `approved` — lihat §0 soal status `FIN-OQ-091`) |
| Dependency | `BE-FIN-094` 🟡 — belum ✅. Dilanjutkan dengan pola yang sama dengan task sebelumnya pada sesi ini |
| Klasifikasi | `HEAVY` — endpoint baru, service diperluas (`BE-FIN-094`), 1 field baru pada entity `BE-FIN-092` (`FinReceivableInstallment.LastPayrollPeriodId`), model bisnis carry-forward yang dirancang sendiri karena kontrak tidak menetapkan mekanismenya persis |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{Controllers,DTOs,Services,Models}/`, `Repositories/Configurations/.../FinReceivableInstallmentConfiguration.cs` (nol perubahan — field baru tidak butuh konfigurasi khusus), `Program.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis, termasuk model carry-forward yang DIRANCANG SENDIRI** (kontrak tidak menetapkan mekanismenya — lihat §0). `dotnet build` **TIDAK DIJALANKAN**. Dibangun di atas 4 task sebelumnya yang semuanya belum ✅ — risiko compile majemuk 5 task |

---

## 0. Temuan sebelum implementasi — WAJIB dibaca

**A. Status `FIN-OQ-091` tidak konsisten antar dokumen blueprint.** `00-interview-decisions.md` baris 2664 mencatat `FIN-OQ-091` **CLOSED** (ditutup `FIN-DEC-190`/`FIN-DEC-191`, 6 Oktober 2026). Tetapi `02-backend-architecture.md` (baris 5744, 5950), `contracts/api-contract.md` (baris 1170), `contracts/state-transition-matrix.md` (baris 674), dan `evidence/24-gerbang-kelengkapan...md` (baris 152, menandainya `BLOCKING`) **masih** menulis "tertahan FIN-OQ-091". Disimpulkan ini **bukti basi** (dokumen-dokumen itu tidak ikut diperbarui saat OQ ditutup), bukan blocker yang sesungguhnya masih berlaku — roadmap `REV-18` sendiri (disetujui 6 Oktober 2026, tanggal yang sama dengan penutupan OQ) sudah mencantumkan `BE-FIN-094`/`095` sebagai task konkret dengan kontrak API/integrasi/validasi yang rinci, yang mustahil ada bila OQ ini masih sungguh-sungguh terbuka. **Tidak menghentikan task**, tetapi dicatat sebagai temuan dokumentasi yang perlu disinkronkan pemilik blueprint.

**B. Model carry-forward antar-angsuran TIDAK ditetapkan persis oleh kontrak, dan teks blueprint secara literal mengandung ketegangan internal.** `M.2.3`/`L.3.1`/`L.3.2` secara eksplisit mendemonstrasikan sisa SEBAGIAN "dibawa" ke angsuran berikutnya. Tetapi `state-transition-matrix.md` O.2 JUGA mencantumkan transisi langsung `TERTUNGGAK → TERBAYAR_SEBAGIAN/TERBAYAR` pada **baris yang sama**, menyiratkan baris lama dapat menerima hasil susulan. Dua baca ini tegang bila digabung naif (berisiko menghitung dua kali). **Yang menyelesaikannya:** `CK_FinReceivableInstallment_Balance` (`BE-FIN-092`) MEWAJIBKAN `OutstandingAmount = ScheduledAmount + CarriedOverAmount - PaidAmount` — formula ini membuktikan baris TIDAK PERNAH bisa "dikosongkan paksa" tanpa menaikkan `PaidAmount` secara artifisial (yang akan merusak makna "berapa yang benar-benar terpotong"). Karena itu, model yang saya rancang:

1. Setiap kiriman memperbarui `PaidAmount`/`OutstandingAmount` baris **sasaran** secara langsung lewat formula itu sendiri — baris boleh menerima kiriman susulan (menjawab ketegangan di atas).
2. Pelimpahan ke `CarriedOverAmount` baris berikutnya terjadi **tepat sekali** per baris — hanya saat baris itu **pertama kali** meninggalkan status `DIJADWALKAN`. Kiriman susulan pada baris yang sama tidak melimpah lagi.

Ini **keputusan desain task ini**, bukan sesuatu yang eksplisit disetujui pemilik — **MUST** ditinjau dan diresmikan HR/Finance Owner, terutama bila niat aslinya berbeda.

**C. Edge case angsuran TERAKHIR tanpa baris tujuan pelimpahan tidak terjawab (`FIN-OQ-092` sebagian terbuka).** Bila baris yang menyisakan tunggakan adalah angsuran terakhir pada perjanjian, sisa dibiarkan menumpuk pada baris itu sendiri — tidak ada mekanisme lain yang ditetapkan. Dicatat sebagai keterbatasan, bukan ditebak.

**D. Field baru `FinReceivableInstallment.LastPayrollPeriodId`.** Kontrak minta idempotensi `(InstallmentId, PayrollPeriodId)`, tetapi entity `BE-FIN-092` tidak punya tempat menyimpan `PayrollPeriodId` yang pernah diterima. Ditambahkan `Guid? LastPayrollPeriodId` — **TOUCHED LEGACY** atas entity task lain yang belum ✅ (belum pernah di-build, belum ada migration) — dicatat eksplisit, bukan disembunyikan sebagai "sudah dari sana".

---

## 1. Proses bisnis

**Pemicu:** `POST /receivables/installments/payroll-results`, dikirim HR segera setelah payroll run disahkan (`FIN-DEC-191`).

**Alur:**
1. Idempotensi diperiksa lebih dulu: bila `(InstallmentId, PayrollPeriodId)` persis sama dengan kiriman terakhir yang tercatat pada baris itu → `200` dengan `isIdempotentReplay = true`, **nol** perubahan apa pun (`FIN-VAL-240`, `L.3.8`, `M.2.4`).
2. Validasi status angsuran: `TERBAYAR`/`DIBATALKAN` → `422` ("bukan dalam status menunggu pembayaran", persis bunyi kontrak).
3. Validasi `SEBAGIAN` wajib `DeductedAmount > 0` (`FIN-VAL-248`); `GAGAL` dipaksa `DeductedAmount = 0` tanpa peduli nilai yang dikirim.
4. Validasi `DeductedAmount` tidak melebihi sisa angsuran SAAT INI (`FIN-VAL-239`).
5. Bila `DeductedAmount > 0`: saldo `FinReceivable.OutstandingAmount` dikurangi, `AllocatedAmount` dinaikkan (lihat §0.E teknis), status piutang diperbarui (`PARTIAL`/`SETTLED`), satu baris mutasi `POTONGAN-GAJI` ditulis lewat `FinanceSubledgerMovementService` (dipakai ulang, bukan ditulis manual).
6. Baris angsuran diperbarui: `PaidAmount`/`OutstandingAmount` berkurang, `Status` ditentukan dari sisa (`TERBAYAR` bila 0, kalau tidak `TERBAYAR_SEBAGIAN`/`TERTUNGGAK` sesuai status yang dikirim).
7. Bila ini pertama kalinya baris meninggalkan `DIJADWALKAN` dan masih bersisa: sisa dilimpahkan ke `CarriedOverAmount`+`OutstandingAmount` baris berikutnya (§0.B).
8. Seluruhnya di dalam satu transaksi `Serializable` + advisory lock per `InstallmentId`.

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/integration-contract.md` §P.3 (payload, status, idempotensi — dibaca penuh sebelumnya)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` `FIN-VAL-239`, `240`, `248`
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` O.2, O.4
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` `M.2.2`..`M.2.4`, `L.3.1`..`L.3.9`, `L.8.3`
- `docs/module-blueprints/finance-management/00-interview-decisions.md` (`FIN-OQ-091`, `FIN-OQ-092`, `FIN-DEC-191`)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs` (`RecordReceivableMovementAsync` — dibaca penuh untuk memahami precondition transaksi aktif dan validasi saldo)
- `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableConfiguration.cs` (`CK_FinReceivable_Balance` — menemukan kebutuhan `AllocatedAmount`, lihat §2.3)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInstallment.cs` | Tambah `Guid? LastPayrollPeriodId` (§0.D) |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivablePayrollResultDtos.cs` | **Baru.** `PayrollResultRequest`, `PayrollResultResponse`, `PayrollResultStatuses` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncService.cs` | Diperluas (`BE-FIN-094`): `ProcessResultAsync` + `ResolveReceivableAsync` + transaksi/lock privat + `PayrollResultValidationException`; constructor menerima `FinanceSubledgerMovementService` |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesPayrollController.cs` | **Baru.** 1 endpoint (`POST .../payroll-results`) |
| `Program.cs` | `AddScoped<FinanceSubledgerMovementService>()` (belum terdaftar — temuan yang sama dengan `BE-FIN-093` §3.A, bukan temuan baru) |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 1 endpoint baru, `FIN-API-1.9` §P.1 |
| Database | `NOT APPLICABLE` — nol migration. 1 kolom nullable baru pada entity `FinReceivableInstallment` yang **migration-nya sendiri belum dibuat** (`BE-FIN-092`), jadi kolom ini otomatis ikut masuk migration itu nanti, bukan migration terpisah |
| Keamanan/Auth | Resource baru `FinanceReceivablePayroll` (`SubmitResult`). **Temuan desain:** kontrak minta "Service-to-Service Token / System Worker Identity" — repository ini **tidak punya** skema autentikasi service-to-service di mana pun (diperiksa, nol preseden). Mengarang skema baru untuk satu endpoint ini berarti arsitektur keamanan paralel — endpoint ini karena itu memakai `[Authorize]`+`AccessPermission` yang sama persis dengan endpoint lain; akun integrasi HR diberi hak lewat Akses Role seperti akun service lain. **MUST ditinjau** pemilik keamanan bila maksud aslinya memang butuh skema terpisah |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| Review manual: `CK_FinReceivableInstallment_Balance` tetap konsisten pada baris sasaran maupun baris pelimpahan setelah mutasi | Dibuktikan aljabar baris per baris (§0.B, dicatat di komentar source) | `PASS` (review manual) | `FinanceReceivablePayrollSyncService.cs` komentar sebelum `ProcessResultAsync` |
| Review manual: `CK_FinReceivable_Balance` (`OriginalAmount = Outstanding+Allocated+Adjusted+WrittenOff`) tetap konsisten setelah `OutstandingAmount` dikurangi | **Ditemukan DAN diperbaiki selama task ini** — draf awal hanya mengurangi `OutstandingAmount` tanpa menaikkan bucket lain, yang akan melanggar constraint ini. Diperbaiki: `AllocatedAmount += deductedAmount` ditambahkan | `PASS` (review manual, bug ditemukan sebelum "selesai" diklaim) | `FinanceReceivablePayrollSyncService.cs`, komentar `CK_FinReceivable_Balance` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

**Risiko compile majemuk 5 task** (`BE-FIN-091`..`095`). Belum satu pun dikompilasi.

Uji manual: `NOT FEASIBLE` — butuh build sukses, migration diterapkan, data piutang+perjanjian+angsuran nyata, dan payload HR yang valid.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `M.2.2` BERHASIL penuh → `TERBAYAR`, saldo berkurang, mutasi `POTONGAN-GAJI` | **Terpenuhi struktural**, belum terverifikasi runtime | `ProcessResultAsync` |
| `M.2.3`/`L.3.1`/`L.3.2` SEBAGIAN → `TERBAYAR_SEBAGIAN`, sisa dibawa ke baris berikutnya | **Terpenuhi struktural** sesuai model §0.B | `ProcessResultAsync` |
| `M.2.4`/`L.3.8`/`FIN-VAL-240` Idempotensi kiriman ganda | **Terpenuhi struktural** — `LastPayrollPeriodId` dicek sebelum mutasi apa pun | `ProcessResultAsync` |
| `L.3.3` Potongan gagal → `TERTUNGGAK`, perjanjian tetap `DISETUJUI` | **Terpenuhi struktural** — task ini tidak pernah menyentuh `FinReceivableInstallmentPlan.Status` | `ProcessResultAsync` |
| `L.3.4`/`L.3.5` Tunggakan dilunasi bertahap/penuh pada kiriman susulan | **Terpenuhi struktural berdasarkan model §0.B** (interpretasi yang saya rancang, bukan eksplisit kontrak) | `ProcessResultAsync` |
| `L.3.6` Finance tidak memeriksa batas potongan | **Terpenuhi by design** — nol pemeriksaan "melebihi batas" di luar `FIN-VAL-239` (sisa angsuran itu sendiri) | `ProcessResultAsync` |
| `L.3.7`/`FIN-VAL-239` Potongan melebihi sisa → `422` | **Terpenuhi struktural** | `ProcessResultAsync` |
| `L.3.9` Pembatalan induk membatalkan baris belum terbayar | **Belum disentuh task ini** — ini perilaku `CancelAsync` pada `BE-FIN-093`, sudah ada sejak task itu, tidak diverifikasi ulang di sini | `FinanceReceivableInstallmentPlanService.CancelAsync` (`BE-FIN-093`) |
| `L.8.3` | Catatan dokumentasi (bukan skenario eksekusi) — **terpenuhi** oleh keberadaan endpoint ini sendiri | — |

**DoD roadmap** ("Build PASS, unit test idempotensi, laporan task tracked"): **belum terpenuhi** — `dotnet build` sengaja tidak dijalankan.

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Model carry-forward §0.B adalah **rancangan task ini**, bukan spesifikasi kontrak eksplisit — **MUST** diresmikan atau dikoreksi pemilik Finance/HR sebelum dianggap final. (2) Status `FIN-OQ-091` tidak konsisten antar dokumen (§0.A) — perlu disinkronkan pemilik blueprint. (3) Source belum pernah dikompilasi |
| Masalah yang diketahui | (1) Edge case angsuran terakhir tanpa tujuan pelimpahan (§0.C, `FIN-OQ-092`). (2) Skema autentikasi endpoint ini memakai pola `[Authorize]` biasa, bukan "service-to-service token" seperti disebut kontrak (§2.3) |
| Risiko tersisa | (1) Risiko compile 5 task majemuk. (2) Bila model carry-forward §0.B ternyata salah tafsir, perlu migrasi data perbaikan setelah berjalan di produksi — belum ada mekanisme reversal untuk kesalahan jenis ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | `git status --short`: 1 field baru pada `FinReceivableInstallment.cs` (`BE-FIN-092`), 2 file baru (DTO, Controller), `FinanceReceivablePayrollSyncService.cs` (`BE-FIN-094`) diperluas, `Program.cs` dimodifikasi |
| Langkah berikutnya | (1) Pemilik Finance/HR meninjau dan meresmikan (atau mengoreksi) model carry-forward §0.B — ini keputusan bisnis yang belum pernah diputuskan eksplisit. (2) Pemilik menjalankan `dotnet build` untuk memvalidasi `BE-FIN-091`..`095` sekaligus. (3) Sinkronkan status `FIN-OQ-091` di seluruh dokumen blueprint (§0.A). (4) Setelah build bersih dan migration diterapkan, uji end-to-end siklus penuh: ajukan → setuju → kirim hasil BERHASIL/SEBAGIAN/GAGAL → verifikasi saldo dan status |
