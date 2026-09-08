# Laporan Perubahan Backend — `BE-BKC-024`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-024` — Pencabutan empat gerbang yang menahan tanggungan |
| `TASK TYPE` | Implementasi backend (pencabutan invarian bisnis pada mesin coverage) |
| `COMPLEXITY` | `MEDIUM` — logika uang disentuh (skor ≥ 1), tidak ada perubahan kontrak API/DTO/entity |
| `CLASSIFICATION SCORE` | 2 — satu faktor bernilai ≥ 1 (logika uang), tanpa kenaikan lain |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs` dan `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingCalculationServiceTests.cs` |
| Gelombang | `MVP-7` bagian pertama (`EPIC BKC-06`) |
| Blueprint | `01-existing-capability-map.md` § 17.4.E — `BKC-CQ-04`/`BKC-CQ-07` ditutup 4 September 2026 |
| Kontrak berlaku | `BIL-VALIDATION-0.6` (empat gerbang dicabut), `BIL-CALCULATION-0.6` |
| Backend cabang saat mulai | `Yasmina`, working tree sudah memuat pekerjaan `BE-BKC-022`/`BE-BKC-023` yang belum di-build/test pengguna |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE` |
| Status registry | **`ACTIVE`** |
| Keberlakuan | `NEW CODE` (perubahan logika penjaga) di dalam berkas modul yang sedang berjalan; bukan `Trx*` legacy, bukan `LEGACY MIGRATION` |
| QBE ID yang berlaku | `QBE-VAL-001` (invarian bisnis — inti task: mencabut dua gerbang), `QBE-SVC-001` (logika tetap di Module Service, tidak dipindah ke controller) |
| QBE ID yang **tidak** berlaku | `QBE-API-001`/`QBE-DTO-001` (tidak ada perubahan kontrak API/DTO), `QBE-ENT-001/002/003` (tidak ada field/entity baru), `QBE-MOD-002/003` (bukan modul baru), `QBE-NAM-*`, `QBE-CFG-*`, `QBE-DB-*` (tidak ada migration) |

### Selisih governance yang ditemukan dan dilaporkan

Sama seperti dicatat pada `task/report/backend/BE-BKC-022.md`: `rules/GLOBAL_RULES.md` tidak ada
(yang tersedia `rules/README.md`, aturan tetap terbaca); kontrak rekayasa berada di
`docs/engineering/` repository ini, bukan `rules/backend/engineering/`. Tidak ada selisih baru pada
task ini.

---

## 1. Masalah yang diperbaiki

Sebelum task ini, `RegistrationBillingCoverageAdapter.ResolveAsync` masih menggeser komponen biaya
ke `unresolved` (nominal menggantung, tidak berakhir pada penjamin maupun pasien) untuk dua kondisi:

1. `rule.CoverageStatus == "NeedApproval"` — status coverage-nya sendiri belum diputuskan.
2. `rule.MaxAmountPerMonth > 0` atau `rule.MaxQuantityPerMonth > 0` — limit bulanan terisi, padahal
   mesin pemakaian kumulatif untuk memeriksa limit itu belum dibangun.

Dua gerbang lain yang disebut judul task (`IsNeedApproval`/`IsNeedGuaranteeLetter` dan jalur tanpa
rule cocok) **sudah tercabut lebih dulu** lewat `BE-BKC-FIX-003`/`BE-BKC-FIX-004` di luar roadmap —
dikonfirmasi lewat pemindaian dampak sebelum task ini disetujui (`01-existing-capability-map.md`
§ 17.4.E). Pemilik menjawab `BKC-CQ-07` bahwa limit bulanan **tidak** menuntut mesin pemakaian
kumulatif pada rilis ini — cukup diperlakukan selalu tersedia, sama seperti gerbang lain yang
dicabut (`BKC-DEC-071`, `072`, `074`).

## 2. Proses bisnis

**Tujuan.** Tagihan pasien asuransi tidak lagi menyisakan nominal menggantung: setiap rupiah
berakhir pada penjamin (Subtotal Asuransi) atau pada pasien (Subtotal Mandiri).

**Pelaku.** Mesin kalkulasi (`RegistrationBillingCoverageAdapter.ResolveAsync`) menghitung; kasir
dan petugas Billing membacanya lewat Menu Pembayaran.

**Pemicu.** Setiap perhitungan ulang tagihan pasien asuransi — baik pratinjau maupun yang disimpan
sebagai versi.

**Aturan bisnis yang dicabut.**

| Sebelumnya | Sesudah task ini |
| --- | --- |
| `CoverageStatus == "NeedApproval"` menggeser seluruh nominal komponen ke `unresolved` | Rule berstatus `NeedApproval` kini dihitung persis seperti `Covered` — mengikuti `CoveragePercent`, `CoPaymentAmount`, `MaxCoverageAmount`, dan batas per kunjungan seperti biasa |
| `MaxAmountPerMonth > 0` atau `MaxQuantityPerMonth > 0` menggeser seluruh nominal komponen ke `unresolved` | Kedua field diabaikan sepenuhnya di `ResolveAsync` — diperlakukan **selalu tersedia** sampai mesin pemakaian kumulatif dibangun kelak (coverage gap tertunda, bukan bagian rilis ini) |

**Yang TIDAK berubah (regresi yang wajib tetap benar).**

| Gerbang | Keadaan |
| --- | --- |
| `IsNeedApproval`/`IsNeedGuaranteeLetter` | Sudah tidak menggeser ke unresolved sejak `BE-BKC-FIX-003` — tidak disentuh task ini |
| Jalur tanpa rule cocok | Sudah langsung jadi porsi pasien sejak `BE-BKC-FIX-004` — tidak disentuh task ini |
| `CoverageStatus == "NotCovered"` | **Tetap** menggeser ke unresolved kecuali `IsAllowExcessPaymentByPatient = true` — tidak disentuh task ini |
| `MaxAmountPerVisit`/`MaxQuantityPerVisit` (batas **per kunjungan**) | **Tetap berlaku** — `BKC-DEC-071` hanya mencabut batas **bulanan**. Ini regresi yang paling mudah tertukar karena kedua limit bertetangga langsung di kode |

**Kolom tidak dihapus.** `CoverageStatus`, `MaxAmountPerMonth`, dan `MaxQuantityPerMonth` tetap ada
pada `MstInsuranceCoverageRule`, tetap dapat diisi admin, dan tetap terbaca untuk keperluan
penasihat di layar entri master data. Yang dicabut hanya kemampuannya menahan perhitungan tagihan
di `ResolveAsync`.

**Contoh berangka.** Konsultasi Rp 100.000, aturan tanggungan 80% dengan `MaxAmountPerMonth =
50.000` (tanpa memeriksa pemakaian bulan berjalan). Sebelum task ini: seluruh Rp 100.000
menggantung (`unresolved`). Sesudah task ini: Rp 80.000 ke Subtotal Asuransi, Rp 20.000 ke Subtotal
Mandiri, nominal menggantung nol.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs` | Lokasi kedua gerbang yang dicabut (`ResolveAsync`) |
| `Areas/HealthServices/MasterData/Models/MstInsuranceCoverageRule.cs` | Konfirmasi tipe `CoverageStatus` (string), `MaxAmountPerMonth`/`MaxQuantityPerMonth` (nullable) tidak berubah — kolom dipertahankan |
| `Areas/HealthServices/MasterData/Services/` (advisory tariff preview) | Konfirmasi `FindCoverageRuleAsync` tidak memakai gerbang yang sama — di luar scope task, tidak tersentuh |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingCalculationServiceTests.cs` | Pola test dan fixture existing untuk `RegistrationBillingCoverageAdapter` |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`, `01-existing-capability-map.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `AGENTS.md` | Kontrak task dan preflight governance |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Services/BillingCoverageAdapter.cs` | Menghapus blok `if (CoverageStatus=="NeedApproval" \|\| MaxAmountPerMonth>0 \|\| MaxQuantityPerMonth>0) { unresolved += ...; continue; }` pada `ResolveAsync`; rule yang sebelumnya masuk cabang ini kini jatuh ke perhitungan `Covered` biasa (`CalculateCoveredAmount` + batas per kunjungan). Komentar di sekitarnya diperbarui agar tidak lagi menyebut gerbang yang sudah dicabut sebagai "dipertahankan" |
| `Tests/.../BillingCalculationServiceTests.cs` | Dua test lama yang menegaskan perilaku gerbang LAMA diganti namanya dan dibalik assert-nya (`RegistrationCoverageAdapterNoLongerGatesRuleWithNeedApprovalCoverageStatus`, `RegistrationCoverageAdapterNoLongerGatesRuleWithMonthlyLimit`); ditambah dua test baru: satu regresi batas per kunjungan (`RegistrationCoverageAdapterStillEnforcesPerVisitLimit`, memakai `MaxAmountPerMonth` besar berdampingan dengan `MaxAmountPerVisit` kecil untuk membuktikan keduanya tidak tertukar), dan satu untuk `BIL-AT-039` yang sebelumnya belum punya test sama sekali (`RegistrationCoverageAdapterNotCoveredRuleWithExcessAllowedBecomesPatientPortion`) |

Total: **2 berkas source/test berubah.** Tidak ada DTO, controller, entity, atau migration
tersentuh.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Nihil pada bentuk kontrak.** Tidak ada field, endpoint, atau status baru. Nilai numerik `primaryAmount`/`patientAmount`/`unresolvedAmount` pada response yang sudah ada **berubah** untuk tagihan yang sebelumnya kena salah satu dari dua gerbang ini — perubahan finansial yang memang menjadi tujuan task, bukan breaking change bentuk kontrak |
| `DATABASE IMPACT` | **Nihil.** Tidak ada kolom, index, entity, maupun migration. `CoverageStatus`, `MaxAmountPerMonth`, `MaxQuantityPerMonth` tetap ada di `MstInsuranceCoverageRule` apa adanya |
| `SECURITY IMPACT` | **Nihil.** Tidak ada perubahan authorization, authentication, atau data yang diekspos |
| `VISUAL REFERENCE` | `NOT REQUIRED` — tidak ada perubahan tampilan; dampaknya murni pada angka Subtotal Asuransi/Mandiri yang sudah ditampilkan Menu Pembayaran existing |

## 4. Dokumentasi endpoint

**Tidak ada endpoint baru dan tidak ada field baru.** Endpoint yang terdampak nilainya (bukan
bentuknya) adalah dua endpoint yang sama dengan `BE-BKC-022`:

| Method | Path | Dampak |
| --- | --- | --- |
| `GET` | `api/v1/health-services/billing-management/billing/invoices/{id:guid}/calculation-preview` | `breakdown.coverage.primaryAmount`/`patientAmount`/`unresolvedAmount` berubah untuk tagihan yang rule-nya `CoverageStatus="NeedApproval"` atau punya `MaxAmountPerMonth`/`MaxQuantityPerMonth` > 0 |
| `POST` | `api/v1/health-services/billing-management/billing/invoices/{id:guid}/recalculate` | Sama |

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna pada task ini: tanpa build/test, pengguna menjalankan sendiri secara manual |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas |
| Verifikasi statis logika | **LULUS** | Manual | Pembacaan ulang `ResolveAsync` setelah edit: urutan cabang kini `rule is null` → `NotCovered` → default (`CalculateCoveredAmount` + jepitan `MaxAmountPerVisit`). Tidak ada referensi tersisa ke `MaxAmountPerMonth`/`MaxQuantityPerMonth` di luar komentar (`grep` pada seluruh `Areas/HealthServices/BillingManagement`) |
| Verifikasi statis field model | **LULUS** | Manual | `MstInsuranceCoverageRule.cs` — `CoverageStatus`, `MaxAmountPerMonth`, `MaxQuantityPerMonth` tetap ada, tidak dihapus/di-rename |
| Cakupan diff | **LULUS** | Manual | `git diff --stat` pada dua berkas task ini menunjukkan tepat: `BillingCoverageAdapter.cs` (+/-43 baris, murni penghapusan blok + komentar), `BillingCalculationServiceTests.cs` (+242/-46 baris, seluruhnya di dalam method test). Tidak ada berkas lain tersentuh oleh task ini |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** Build dan test wajib
> dijalankan pengguna secara manual sesuai instruksi eksplisit pada task ini.

### Test yang diubah

| Test | Sebelumnya | Sesudah |
| --- | --- | --- |
| `RegistrationCoverageAdapterStillGatesRuleWithNeedApprovalCoverageStatus` → **diganti nama** `RegistrationCoverageAdapterNoLongerGatesRuleWithNeedApprovalCoverageStatus` | Assert `PrimaryAmount=0`, `UnresolvedAmount=100.000` | Assert `PrimaryAmount=80.000`, `PatientAmount=20.000`, `UnresolvedAmount=0` |
| `RegistrationCoverageAdapterStillGatesRuleWithMonthlyLimit` → **diganti nama** `RegistrationCoverageAdapterNoLongerGatesRuleWithMonthlyLimit` | Assert `PrimaryAmount=0`, `UnresolvedAmount=100.000` | Assert `PrimaryAmount=80.000`, `PatientAmount=20.000`, `UnresolvedAmount=0`; ditambah `MaxQuantityPerMonth=1` supaya kedua field limit bulanan sama-sama teruji |

### Test baru

| Test | Membuktikan |
| --- | --- |
| `RegistrationCoverageAdapterStillEnforcesPerVisitLimit` | Regresi wajib DoD: `MaxAmountPerVisit` tetap menjepit meski `MaxAmountPerMonth` besar diisi berdampingan pada rule yang sama — 80.000 dijepit ke 30.000, sisanya (70.000) jadi porsi pasien, bukan unresolved |
| `RegistrationCoverageAdapterNotCoveredRuleWithExcessAllowedBecomesPatientPortion` | `BIL-AT-039` — sebelumnya tidak punya test sama sekali; membuktikan cabang `NotCovered` (tidak disentuh task ini) tetap mengembalikan seluruh nominal sebagai porsi pasien saat `IsAllowExcessPaymentByPatient=true` |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| `BIL-AT-036` — `Covered` 100% dengan `IsNeedApproval=true`: seluruh nominal `primaryAmount`, unresolved nol | **Tercakup** — `RegistrationCoverageAdapterCoversItemEvenWhenRuleNeedsApprovalOrGuaranteeLetter` (test existing, tidak disentuh task ini, tetap lulus secara logis karena `IsNeedApproval` bukan `CoverageStatus`) |
| `BIL-AT-037` — `Covered` 80% dengan `MaxAmountPerMonth`: 80% penjamin, 20% pasien, unresolved nol | **Tercakup** — inilah acceptance baru task ini; `RegistrationCoverageAdapterNoLongerGatesRuleWithMonthlyLimit` |
| `BIL-AT-038` — item tanpa rule cocok: seluruh nominal jadi porsi pasien | **Sudah terpenuhi sejak `BE-BKC-FIX-004`**, tidak disentuh task ini; tercakup test existing `RegistrationCoverageAdapterUsesApprovedGenericPrimaryRule` sebagai kasus bertetangga (rule tidak match menghasilkan outcome 0/0 di baris `rule is null`) |
| `BIL-AT-039` — `NotCovered` dengan `IsAllowExcessPaymentByPatient=true`: seluruh nominal jadi porsi pasien | **Tercakup** — test baru `RegistrationCoverageAdapterNotCoveredRuleWithExcessAllowedBecomesPatientPortion`, sebelumnya tidak ada test sama sekali untuk cabang ini |
| `BIL-AT-052` — Subtotal Mandiri + Asuransi + Pajak Mandiri + Pajak Asuransi + Selisih Tidak Ditagihkan = Total Tagihan | **Di luar scope perubahan adapter ini** — dijaga penjumlahan total oleh `BillingCalculationService` (tidak disentuh task ini) dan penjaga `BIL-VAL-028` dari `BE-BKC-022`; tidak ditambah test baru pada task ini karena tidak ada logika penjumlahan total yang berubah |
| Sisa dua gerbang tercabut | **Terpenuhi** — `CoverageStatus="NeedApproval"` dan `MaxAmountPerMonth`/`MaxQuantityPerMonth` tidak lagi menggeser ke unresolved |
| Batas per kunjungan terbukti masih berlaku | **Terpenuhi** — test baru `RegistrationCoverageAdapterStillEnforcesPerVisitLimit` |
| Tagihan pasien tunai tidak berubah sama sekali | **Terpenuhi** — jalur `SelfPay()` tidak tersentuh sama sekali oleh perubahan ini (berada di luar `foreach` yang diedit) |
| `BIL-AT-036` dan `038` diverifikasi tetap lulus (bukan regresi) | **Tercakup secara statis** — kedua test yang membuktikannya tidak diubah dan tidak berada di jalur kode yang disentuh; angka pastinya menunggu `dotnet test` dijalankan pengguna |
| Build dan test lulus | **BELUM** — menunggu pengguna, sesuai instruksi eksplisit task ini |

**Definition of Done belum tercapai.** Yang tersisa hanya menjalankan build dan test.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Perubahan ini mengubah nominal finansial **seluruh** tagihan asuransi yang rule-nya berstatus `NeedApproval` atau memakai `MaxAmountPerMonth`/`MaxQuantityPerMonth` — bukan hanya data uji. Tagihan yang sebelumnya menampilkan nominal menggantung untuk kasus ini akan mulai menampilkan Subtotal Asuransi/Mandiri terisi setelah build/test dijalankan dan dirilis |
| `KNOWN ISSUES` | Limit bulanan diperlakukan **selalu tersedia** tanpa mesin pemakaian kumulatif, sesuai jawaban pemilik atas `BKC-CQ-07` — dicatat sebagai coverage gap tertunda di `01-existing-capability-map.md` § 17.4.E, bukan cacat yang lolos tanpa sepengetahuan pemilik |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi |
| `INCIDENTAL CHANGES` | `NONE` |
| `INTERRUPTIONS` | `NONE` |
| `GIT STATUS` | 2 berkas source/test task ini berubah, belum di-stage. Working tree juga memuat perubahan tersendiri dari `BE-BKC-022`/`BE-BKC-023` (berkas berbeda) yang belum di-build/test pengguna — tidak disentuh atau digabung oleh task ini. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet build` dan `dotnet test` secara manual mencakup seluruh perubahan yang menumpuk (`BE-BKC-022`, `BE-BKC-023`, `BE-BKC-024`). Bila lulus, task ini selesai dan `BE-BKC-025` (bergantung sequencing pada task ini) dapat dimulai |
