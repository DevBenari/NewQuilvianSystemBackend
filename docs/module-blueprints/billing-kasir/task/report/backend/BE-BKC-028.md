# Laporan Perubahan Backend — `BE-BKC-028`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-028` — Perutean selisih yang tidak dapat ditagihkan |
| `TASK TYPE` | Implementasi backend (satu cabang akumulator dipindahkan + kontrak API bertambah field) |
| `COMPLEXITY` | `HIGH` — mesin perhitungan uang disentuh (skor ≥ 1), kontrak API bertambah field pada lima DTO (skor 2), `BillingCoverageDecision`/`BillingCoverageComponentOutcome` bertambah argumen posisional lagi (risiko urutan argumen, dicatat eksplisit oleh roadmap) |
| `CLASSIFICATION SCORE` | 4 |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/{Services,Dtos}/` dan `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/{BillingCalculationServiceTests.cs,BillingAllocationServiceTests.cs}` |
| Gelombang | `MVP-11` bagian kedua (`EPIC BKC-09`) |
| Trace | `FR-BKC-038`, `FR-BKC-039`; `BKC-DEC-080`; `BKC-DES-021`, `BKC-DES-022` |
| Blueprint | `BIL-CASH-001` revisi `0.8` — kontrak dikunci 4 September 2026 |
| Kontrak berlaku | `BIL-API-0.7`, `BIL-VALIDATION-0.7` (`BIL-VAL-043`), `BIL-CALCULATION-0.7` — seluruhnya `approved` |
| Dependency | `BE-BKC-027` (kolom `BilCalculationVersion.NonBillableResidualAmount` — berkas migration sudah dibuat, **belum dijalankan ke basis data**); `MVP-7` (`BE-BKC-024`, `BE-BKC-025`, source selesai) |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` — satu akumulator baru dan field baru pada dua record/beberapa DTO yang sudah ada, dipola persis seperti `BE-BKC-025`. Bukan `Trx*` legacy, bukan `LEGACY MIGRATION` (tidak ada migration pada task ini — kolom fisiknya sudah disiapkan `BE-BKC-027`) |
| QBE ID yang berlaku | `QBE-VAL-001` (invarian bisnis baru `BIL-VAL-043` — inti task), `QBE-API-001`/`QBE-DTO-001` (field baru pada lima DTO response yang sudah ada) |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`/`QBE-CFG-*`/`QBE-DB-*` (tidak ada entity/kolom/migration baru pada task ini — sudah selesai `BE-BKC-027`), `QBE-MOD-002/003`, `QBE-NAM-*` |

Selisih governance sama seperti `BE-BKC-025`; tidak ada selisih baru.

---

## 1. Masalah yang diselesaikan

Sejak `BE-BKC-024`, residual yang kontrak penjamin larang ditagihkan ke pasien
(`IsAllowExcessPaymentByPatient = false`) berakhir sebagai `UnresolvedAmount` — angka bernama yang
**tidak punya tindak lanjut**. Uangnya sudah benar (tidak jatuh ke pasien), tapi nasibnya berhenti di
layar tanpa mekanisme penyelesaian. Task ini memindahkan nominal itu ke ember baru
(`NonBillableResidualAmount`) yang **punya** tindak lanjut: menunggu Finance mengajukan write-off
kategori `NON_BILLABLE_RESIDUAL` lewat mekanisme Pengecualian Finansial yang sudah ada
(`BE-BKC-029`, belum dikerjakan). Task ini **hanya** memindahkan ke mana nominalnya pergi dan
mempersistnya ke kolom hasil `BE-BKC-027` — belum membangun layar/endpoint pengajuan write-off itu
sendiri.

## 2. Proses bisnis

**Tujuan.** Selisih yang menurut kontrak asuransi bukan tanggungan pasien mendapat ember bernama
yang punya pemilik tindak lanjut (Finance), bukan berhenti sebagai angka pasif.

**Pelaku.** Mesin kalkulasi mendeteksi dan menghitung nominalnya secara otomatis setiap kali
tagihan dihitung ulang atau pratinjau dibuka; **tidak ada** pelaku manusia dalam task ini — pengajuan
write-off oleh Finance adalah scope `BE-BKC-029`.

**Pemicu.** Setiap perhitungan tagihan asuransi dengan komponen yang cocok rule `Covered` namun
nominal cakupannya kurang dari nilai komponen (residual dari cap `MaxAmountPerVisit`/
`MaxQuantityPerVisit`/`CoveragePercent`/`CoPaymentAmount`), **dan** rule itu menandai
`IsAllowExcessPaymentByPatient = false`.

**Aturan bisnis.** Satu-satunya titik tangkap adalah cabang residual jalur (5) di dalam
`RegistrationBillingCoverageAdapter.ResolveAsync` — **bukan** `ApplyCoverageWaterfall`, karena
hanya `ResolveAsync` yang memegang `rule.IsAllowExcessPaymentByPatient` (`BKC-DES-022`). Akumulator
`unresolved += residual` untuk cabang `IsAllowExcessPaymentByPatient = false` diganti
`nonBillableResidual += residual`. Cabang `IsAllowExcessPaymentByPatient = true` **tidak disentuh**
sama sekali — residual tetap jatuh ke pasien lewat identitas turunan yang sama seperti sebelumnya
(`BKC-DEC-070`). Jalur (2) `NotCovered` **sengaja tidak disentuh** pada task ini — perluasannya ke
jalur (2) adalah scope `BE-BKC-030` (revisi `0.9`, `BKC-DES-026`/`027`, kontrak masih **draft**,
belum diberi wewenang).

**Perubahan status.** Tidak ada — task ini tidak menyentuh state machine invoice maupun write-off.

**Jalur tidak normal.** `BIL-VAL-043` (baru) menghentikan perhitungan bila
`primaryAmount + excessAmount + unresolvedAmount + nonBillableResidualAmount` melebihi biaya yang
memenuhi syarat — diperiksa **terpisah** dari batas `primary+excess+unresolved` yang sudah ada
(yang **tidak diubah**, `BKC-DES-021`) supaya pesannya tetap tepat menyebut sebabnya. Berlawanan
arah dengan `BIL-VAL-035` (`DataAnomalyAmount`, dikecualikan dari batas ini): nominal anomali data
sudah terwakili sebagai porsi pasien (menjumlahkannya berarti menghitung dua kali), sedangkan
residual non-billable **tidak** terwakili di suku mana pun sehingga wajib dijumlahkan supaya ada
penjaga yang mencegahnya membengkak melebihi biaya tagihannya.

**Hasil akhir.** Yang dibayar pasien **tidak bergeser satu rupiah pun** — nominal yang sama yang
dulu mengalir lewat `UnresolvedAmount` kini mengalir lewat `NonBillableResidualAmount`, dan formula
`PatientAmount` tetap mengeluarkannya dari porsi pasien (hanya berpindah suku pengurang, bukan
pengurang baru). Yang berubah adalah **nasibnya sesudahnya**: sebelumnya berhenti sebagai angka
tanpa tindak lanjut, kini muncul di layar Pengecualian Finansial (`BE-BKC-029`) sebagai nominal yang
menunggu pengajuan write-off.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/02-backend-architecture.md` § amendment revisi `0.8` (baris ±1160–1420) | Spesifikasi persis titik tangkap, urutan field record, formula `PatientAmount`, dan wording `BIL-VAL-043` |
| `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` § amendment "Residual non-billable dirutekan ke write-off" | Wording persis `BIL-VAL-040`–`043` dan status `approved` amendment-nya |
| `Areas/.../Billing/Services/BillingCoverageAdapter.cs` | Lokasi utama perubahan — cabang residual jalur (5) di `ResolveAsync`, tiga titik konstruksi `BillingCoverageDecision` (`ResolveAsync`, `SelfPay`, `Anomaly`) |
| `Areas/.../Billing/Services/BillingCalculationService.cs` | `outcomeByComponent` copy-back, `ApplyCoverageWaterfall`, persist kolom `BilCalculationVersion.NonBillableResidualAmount` |
| `Areas/.../Billing/Dtos/BillingInvoiceDtos.cs` | Bentuk `CoverageCalculationResponse`, `CalculationItemResponse`, `AdministrationFeeCalculationResponse`, `RoomChargeCalculationResponse` yang sudah ada |
| `Tests/.../BillingCalculationServiceTests.cs` | Pola test integrasi `RegistrationBillingCoverageAdapter` yang sudah dipakai `BE-BKC-024`/`025` |
| `Areas/HealthServices/MasterData/Models/MstInsuranceCoverageRule.cs` | Konfirmasi `IsAllowExcessPaymentByPatient` bawaannya `true` (baris 47) — dampak amendment ini terbatas pada pengecualian yang di-set sengaja |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Services/BillingCoverageAdapter.cs` | `BillingCoverageComponentOutcome` +`NonBillableResidualAmount`; `BillingCoverageDecision` +`NonBillableResidualAmount`; cabang residual jalur (5) di `ResolveAsync`: `unresolved += residual` → `nonBillableResidual += residual` untuk `IsAllowExcessPaymentByPatient = false`; tiga titik konstruksi record (`ResolveAsync`, `SelfPay()`, `Anomaly()`) disesuaikan arity — keduanya (`SelfPay`/`Anomaly`) mengembalikan `NonBillableResidualAmount = 0` |
| `Billing/Dtos/BillingInvoiceDtos.cs` | `CoverageCalculationResponse` +`NonBillableResidualAmount`/+`HasNonBillableResidual`; `CalculationItemResponse` +`ItemNonBillableResidualAmount`/+`TaxNonBillableResidualAmount`; `AdministrationFeeCalculationResponse`/`RoomChargeCalculationResponse` +`NonBillableResidualAmount` |
| `Billing/Services/BillingCalculationService.cs` | `outcomeByComponent` copy-back menyalin `NonBillableResidualAmount` untuk item/pajak/admin-fee/room-charge; `version.NonBillableResidualAmount = coverageResult.NonBillableResidualAmount` (persist ke kolom `BE-BKC-027`); `ApplyCoverageWaterfall` menambah `BIL-VAL-043`, formula `PatientAmount` menjadi `residualAfterExcess − unresolvedAmount − nonBillableResidualAmount`, mengisi field baru pada `CoverageCalculationResponse` |
| `Tests/.../BillingCalculationServiceTests.cs` | Enam titik konstruksi record disesuaikan arity (`FixedCoverageAdapter` ×2, `AllocatingCoverageAdapter`, `MisallocatingCoverageAdapter`, `SelfPayCoverageAdapter`, plus outcome di `AllocatingCoverageAdapter`); tiga test baru (`RegistrationCoverageAdapterResidualBecomesNonBillableWhenExcessNotAllowed`, `...ResidualStaysPatientPortionWhenExcessAllowed`, `CalculationEnginePreviewNeverCreatesWriteOffCaseForNonBillableResidual`); dua assertion baru ditambahkan ke test anomali `BE-BKC-025` yang sudah ada (`RegistrationCoverageAdapterPayerNotEligibleBecomesAnomalyAndPatientPortion`) untuk membuktikan acceptance 5 |
| `Tests/.../BillingAllocationServiceTests.cs` | Satu titik konstruksi (`SelfPayCoverageAdapter` lokal) disesuaikan arity |

Total: **5 berkas source/test berubah**. Tidak ada DTO/controller/entity/migration baru selain
field yang disebut di atas — kolom fisiknya sudah disiapkan `BE-BKC-027` (belum dijalankan ke
basis data).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Additive.** Tujuh field baru pada lima DTO response yang sudah ada. Tidak ada field yang dihapus atau berubah arti. Satu perilaku **berubah nilainya**: komponen dengan rule `Covered` + residual + `IsAllowExcessPaymentByPatient = false` kini menghasilkan `unresolvedAmount = 0` dan `nonBillableResidualAmount` terisi (dulu sebaliknya) — `PatientAmount` totalnya **tidak berubah** |
| `DATABASE IMPACT` | **Nihil pada task ini.** Task ini hanya MENULIS ke kolom `BilCalculationVersion.NonBillableResidualAmount` yang sudah disiapkan `BE-BKC-027` (`ADD COLUMN` belum dijalankan ke basis data mana pun — lihat § 5 Risiko) |
| `SECURITY IMPACT` | **Nihil.** Tidak ada perubahan authorization |
| `VISUAL REFERENCE` | `NOT REQUIRED` — konsumen frontend (`FE-BKC-019`) di luar scope task ini |

## 4. Dokumentasi endpoint

**Tidak ada endpoint baru.** Field bertambah pada response dua endpoint yang sama dengan
`BE-BKC-022`/`024`/`025`:

| Method | Path | Dampak |
| --- | --- | --- |
| `GET` | `.../billing/invoices/{id:guid}/calculation-preview` | `breakdown.coverage.nonBillableResidualAmount`/`hasNonBillableResidual` baru; `breakdown.items[].itemNonBillableResidualAmount`/`taxNonBillableResidualAmount` baru; `breakdown.administrationFee`/`roomCharge.nonBillableResidualAmount` baru |
| `POST` | `.../billing/invoices/{id:guid}/recalculate` | Sama |

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Perhitungan berhasil. **Termasuk** ketika ada residual non-billable — bukan kegagalan permintaan |
| `422` | Perhitungan melanggar batas yang dijaga — **termasuk `BIL-VAL-043` yang baru** |

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna: build backend dijalankan manual |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas |
| Verifikasi statis seluruh titik konstruksi record | **LULUS** | Manual | `grep -rn "new BillingCoverageDecision(\|new BillingCoverageComponentOutcome("` pada seluruh repository menunjukkan tepat 14 titik (termasuk selector `Anomaly()`), seluruhnya sudah memakai arity baru (11 dan 6 argumen berturut-turut) |
| Verifikasi statis jalur (2) `NotCovered` tidak tersentuh | **LULUS** | Manual | Baris `notCoveredUnresolved`/`unresolved += notCoveredUnresolved` pada cabang `NotCovered` tidak diubah — hanya argumen outcome-nya bertambah satu `0` untuk arity, nilai fungsionalnya identik |
| Verifikasi matematis `PatientAmount` tidak pernah negatif | **LULUS (analisis manual)** | Manual | `BIL-VAL-043` menjamin `unresolved + nonBillableResidual ≤ coverableAmount − primary − excess`; karena `coverableAmount ≤ eligibleAmount` (coverable adalah subset komponen yang menyusun eligibleAmount, seluruh `Amount` non-negatif), maka `coverableAmount − primary − excess ≤ residualAfterExcess`, sehingga `unresolved + nonBillableResidual ≤ residualAfterExcess` — `PatientAmount = residualAfterExcess − unresolved − nonBillableResidual ≥ 0` terjamin selama `BIL-VAL-043` lolos. Pemeriksaan `unresolvedAmount > residualAfterExcess` yang sudah ada **tidak diubah** (redundan tapi tidak salah, dan tidak diminta dokumen) |
| Cakupan diff | **LULUS** | Manual | Perubahan manual persis 5 berkas source/test (§ 3.2); nol migration, nol entity, nol controller baru |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** Build dan test wajib
> dijalankan pengguna. Laporan ini mencatat keadaan sebenarnya, bukan mengklaim keberhasilan.

### Test baru

| Test | Membuktikan |
| --- | --- |
| `RegistrationCoverageAdapterResidualBecomesNonBillableWhenExcessNotAllowed` | `BIL-AT-055` — rule `Covered` 70% dengan `IsAllowExcessPaymentByPatient = false`: `primaryAmount=70.000`, `patientAmount=0`, `unresolvedAmount=0`, `nonBillableResidualAmount=30.000`, `hasNonBillableResidual=true`, Total Tagihan (`GrossAmount`) tetap `100.000` |
| `RegistrationCoverageAdapterResidualStaysPatientPortionWhenExcessAllowed` | `BIL-AT-056` — uji pasangan langsung: rule **persis sama** kecuali `IsAllowExcessPaymentByPatient = true` menghasilkan `patientAmount=30.000`, `nonBillableResidualAmount=0`. Membuktikan cabang `true` benar-benar tidak disentuh |
| `CalculationEnginePreviewNeverCreatesWriteOffCaseForNonBillableResidual` | `BIL-AT-057` — `PreviewCalculationAsync` dipanggil 10× berturut-turut pada tagihan dengan residual non-billable; `db.BilWriteOffCases` dan `db.BilCalculationVersions` tetap kosong sesudahnya (pratinjau tidak pernah menyimpan apa pun, dan mesin kalkulasi tidak pernah mengajukan write-off sendiri, `BKC-DES-023`) |
| (assertion tambahan) `RegistrationCoverageAdapterPayerNotEligibleBecomesAnomalyAndPatientPortion` | Acceptance 5 — jalur anomali data (`BE-BKC-025`) tetap mengembalikan `nonBillableResidualAmount=0`/`hasNonBillableResidual=false`, membuktikan `Anomaly()` tidak pernah mengisinya |

**Jalur pasien tunai (`SelfPay()`) tidak diuji lewat integration test baru.** Sama seperti
`DataAnomalyAmount` pada `BE-BKC-025`, `NonBillableResidualAmount = 0` pada `SelfPay()` adalah
literal yang di-hardcode (bukan hasil komputasi) dan tidak ada test integrasi existing yang
menempuh jalur `SelfPay()` lewat adapter sungguhan (`RegistrationBillingCoverageAdapter`) — gap
verifikasi pra-existing, bukan sesuatu yang diperkenalkan task ini; membuat test baru khusus untuk
literal ini dinilai tidak proporsional terhadap risikonya.

`BIL-VAL-043` **tidak** ditambah test overflow terpisah — proporsional, sama seperti keputusan
`BE-BKC-025` untuk `BIL-VAL-035`: `nonBillableResidual` yang dihasilkan `ResolveAsync` selalu berupa
sub-jumlah komponen coverable (tidak pernah melebihi `coverableAmount` by construction), sehingga
overflow hanya dapat diuji lewat adapter palsu yang sengaja cacat (pola `MisallocatingCoverageAdapter`)
— tidak dibuat karena bukan acceptance criteria bernomor pada roadmap task ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| `BIL-AT-055` — `primaryAmount` 70.000, selisih 30.000, nominal menggantung nol, porsi pasien nol, Total Tagihan tetap 100.000 | **Tercakup** |
| `BIL-AT-056` — uji pasangan `IsAllowExcessPaymentByPatient = true` → porsi pasien 30.000, selisih nol; cabang `true` tidak ikut berpindah | **Tercakup** |
| `BIL-AT-057` — sepuluh kali pratinjau berturut-turut tidak melahirkan satu pun kasus penanggungan | **Tercakup** |
| `BIL-VAL-043` — `primary+excess+unresolved+nonBillableResidual` tidak boleh melebihi biaya yang memenuhi syarat | **Terpenuhi di kode**; tidak ditambah test overflow terpisah — lihat § 5 |
| Jalur pasien tunai dan jalur anomali data tetap mengembalikan selisih bernilai nol | **Tercakup** untuk jalur anomali (assertion baru); jalur `SelfPay()` diverifikasi statis (literal hardcoded) — lihat § 5 |
| Total Tagihan, Subtotal Mandiri, Subtotal Asuransi, outstanding pasien tidak bergeser satu rupiah pun | **Terpenuhi by design** — dibuktikan matematis (§ 5) dan test `BIL-AT-055`/`056` (`PatientAmount` identik antara cabang `true`/`false` untuk komponen yang sama, hanya nasib selisihnya yang beda) |
| Build dan test lulus | **BELUM** — menunggu pengguna |

**Definition of Done belum tercapai.** Yang tersisa hanya menjalankan build dan test.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Kolom persist `BilCalculationVersion.NonBillableResidualAmount` (`BE-BKC-027`) **belum ada secara fisik** di basis data mana pun — migration-nya sudah dibuat dan direview tetapi belum dijalankan (`BKC-GATE-09`). Kode task ini AKAN GAGAL saat runtime bila dijalankan terhadap basis data yang belum menerima migration itu (EF Core akan mencoba menulis kolom yang tidak ada). **Migration `BE-BKC-027` MUST dijalankan lebih dulu** sebelum kode task ini diaktifkan di lingkungan mana pun |
| `KNOWN ISSUES` | Check constraint `CK_BilCalculationVersion_Amounts` (dibuat `BE-BKC-027`) **tidak** memasukkan `NonBillableResidualAmount >= 0` ke dalam daftar kolom yang dijaga — perlindungan non-negatif untuk kolom itu saat ini murni di lapisan aplikasi (`if (nonBillableResidualAmount < 0) throw ...` pada `ApplyCoverageWaterfall`). Menambah constraint itu memerlukan migration baru, di luar scope task ini (task ini tidak diberi wewenang database); dicatat sebagai gap pertahanan-berlapis, bukan dibiarkan diam-diam |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi dan migration `BE-BKC-027` sudah dijalankan |
| `INCIDENTAL CHANGES` | Satu berkas di luar Billing langsung (`Tests/.../BillingAllocationServiceTests.cs`) tersentuh **hanya** untuk menyesuaikan arity `BillingCoverageDecision` pada `SelfPayCoverageAdapter` lokalnya sendiri — bukan perluasan scope, konsekuensi record positional yang sama dipakai lintas berkas test |
| `INTERRUPTIONS` | Tidak ada |
| `GIT STATUS` | 5 berkas task ini berubah, belum di-stage. Working tree juga memuat perubahan tersendiri dari `BE-BKC-022`–`027` yang belum di-build/test resmi oleh pengguna — task ini melanjutkan DI ATAS perubahan itu, bukan menimpanya. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet test` mencakup seluruh perubahan yang menumpuk (`BE-BKC-022` s.d. `028`). Migration `BE-BKC-027` **MUST** dijalankan ke basis data (otorisasi terpisah, `BKC-GATE-09`) sebelum kode task ini dapat berfungsi di lingkungan mana pun. Task backend berikutnya sesuai urutan: `BE-BKC-029` (`BLOCKED` oleh sequencing `BE-BKC-028`) |

## Update 6 September 2026 — migration `BE-BKC-027` dieksekusi, peringatan di atas tidak lagi berlaku

`dotnet build`/`dotnet test` dikonfirmasi lulus pengguna, dan migration
`20260904232421_AddWriteOffCategoryAndNonBillableResidual` sudah dieksekusi ke database dev.
Dibuktikan langsung lewat query read-only: kolom `BilCalculationVersion.NonBillableResidualAmount`
kini ada secara fisik (`NOT NULL`, default `0`). **Kode task ini tidak lagi akan gagal runtime**
karena ketiadaan kolom. Detail audit lengkap: `task/report/backend/BE-BKC-032.md`.

Yang tersisa bukan lagi soal kode/migration, melainkan **data uji**: database dev saat ini belum
punya satu pun baris `BilCalculationVersion` (nol transaksi tersimpan sama sekali), sehingga belum
ada satu pun kasus non-billable residual nyata untuk didemonstrasikan sebagai bukti keluar `BIL-AT-055`–`057`.
