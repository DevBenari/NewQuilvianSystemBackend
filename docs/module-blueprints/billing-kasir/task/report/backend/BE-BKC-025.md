# Laporan Perubahan Backend — `BE-BKC-025`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-025` — Anomali data penjamin |
| `TASK TYPE` | Implementasi backend (kategori anomali baru + kontrak response bertambah) |
| `COMPLEXITY` | `HIGH` — mesin perhitungan uang disentuh (skor ≥ 1), kontrak API bertambah field pada empat DTO (skor 2), dan penjaga bisnis baru (`BIL-VAL-035`/`037`) beserta penjaga yang diretarget (`BIL-VAL-036`) |
| `CLASSIFICATION SCORE` | 4 |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/{Services,Dtos}/` dan `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingCalculationServiceTests.cs` |
| Gelombang | `MVP-7` bagian kedua (`EPIC BKC-07`) |
| Blueprint | `BIL-CASH-001` revision `0.8` — `approved` 4 September 2026 (kontrak dikunci) |
| Kontrak berlaku | `BIL-API-0.6`, `BIL-VALIDATION-0.6` (`BIL-VAL-035`–`037`) — `approved` |
| Backend cabang saat mulai | `Yasmina`, working tree sudah memuat `BE-BKC-022`/`023`/`024` yang belum di-build/test pengguna |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` — satu record baru (`BillingCoverageAnomaly`), field baru pada tiga record/DTO yang sudah ada, satu method privat baru (`Anomaly`) menggantikan satu method lama (`Unresolved`, dihapus karena tidak lagi dipanggil). Bukan `Trx*` legacy, bukan `LEGACY MIGRATION` |
| QBE ID yang berlaku | `QBE-VAL-001` (invarian bisnis — inti task), `QBE-API-001`/`QBE-DTO-001` (field baru pada DTO response yang sudah ada) |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*` (tidak ada entity/kolom baru), `QBE-MOD-002/003`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-DB-*` (tidak ada migration) |

Selisih governance sama seperti `BE-BKC-022`/`023`/`024`; tidak ada selisih baru.

---

## 1. Masalah yang diselesaikan

Sebelum task ini, empat precondition penjamin yang bermasalah (belum eligible, polis tidak aktif,
perusahaan asuransi belum dipilih, encounter tidak ditemukan) sama-sama membuat seluruh komponen
biaya menggantung (`unresolved`) lewat method `Unresolved(...)`. Akibatnya kasir melihat tagihan
yang tidak dapat dialokasikan ke siapa pun — persis perilaku "Penjamin Belum Terverifikasi" yang
sudah dikeluhkan sebelum `BE-BKC-024`, hanya berpindah nama.

## 2. Proses bisnis

**Tujuan.** Kesalahan data pendaftaran terlihat oleh petugas yang dapat menindaklanjutinya, tanpa
menahan pembayaran pasien yang sudah selesai berobat.

**Pelaku.** Mesin kalkulasi mendeteksi dan menghitung; kasir membaca peringatan dan tetap dapat
menerima pembayaran; petugas pendaftaran membetulkan data penjamin.

**Pemicu.** Setiap perhitungan tagihan pasien asuransi yang precondition penjaminnya bermasalah.

**Aturan bisnis.**

| Kode | Keadaan |
| --- | --- |
| `PAYER_NOT_ELIGIBLE` | `TrxPatientEncounterGuarantor.IsEligible = false` |
| `POLICY_INACTIVE` | `IsPolicyActive = false` |
| `INSURANCE_PROVIDER_MISSING` | `InsuranceProviderId` kosong padahal `PaymentType` bukan `Cash` |
| `ENCOUNTER_NOT_FOUND` | Encounter tidak ditemukan saat adapter membacanya (seharusnya tidak mungkin — lihat § 7) |

Setiap kode menghasilkan kalkulasi yang **berhasil** (`200`), bukan galat. Seluruh komponen
coverable jatuh ke pasien, dan `DataAnomalyAmount` menjadi penanda **di atas** pembagian itu — bukan
bucket uang ketiga.

**Perubahan status.** Tidak ada.

**Jalur tidak normal.** `BIL-VAL-035` menghentikan perhitungan bila nominal anomali melebihi biaya
yang memenuhi syarat. `BIL-VAL-036` (diretarget dari menguji `unresolvedAmount` menjadi menguji
`dataAnomalyAmount`) menghentikan perhitungan bila `PrimaryStatus` mengklaim `REJECTED` tanpa
anomali tercatat — penjaga terakhir yang mencegah tanggungan ditolak diam-diam berpindah ke pasien.
`BIL-VAL-037` adalah invariant internal (kode kosong padahal anomali terdeteksi) yang seharusnya
tidak pernah tercapai lewat adapter ini sendiri.

**Hasil akhir.** Tagihan tetap dapat dibayar sampai tuntas; kode anomali ikut tersimpan pada versi
kalkulasi untuk ditelusuri kembali.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/02-backend-architecture.md` § Amendment 4 September 2026 | Spesifikasi `BillingCoverageAnomaly`, field baru per class, tabel "Perubahan perilaku `ResolveAsync`" |
| `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` | Wording persis `BIL-VAL-035`–`037` |
| `Areas/.../Billing/Services/BillingCoverageAdapter.cs` | Lokasi utama perubahan — jalur (4) `ResolveAsync`, `Unresolved()` yang diganti |
| `Areas/.../Billing/Services/BillingCalculationService.cs` | `outcomeByComponent` copy-back, `ApplyCoverageWaterfall` |
| `Areas/.../Billing/Dtos/BillingInvoiceDtos.cs` | Bentuk `CalculationItemResponse`, `AdministrationFeeCalculationResponse`, `RoomChargeCalculationResponse`, `CoverageCalculationResponse` yang sudah ada |
| `Tests/.../BillingCalculationServiceTests.cs` | Pola test integrasi `RegistrationBillingCoverageAdapter` yang sudah dipakai `BE-BKC-024` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Services/BillingCoverageAdapter.cs` | Record baru `BillingCoverageAnomaly(Code, Message)`; `BillingCoverageComponentOutcome` +`DataAnomalyAmount`; `BillingCoverageDecision` +`DataAnomalyAmount`, +`Anomalies`; jalur (4) `ResolveAsync` dipecah jadi empat pemeriksaan terpisah (dari satu `\|\|` gabungan); method `Unresolved(...)` **dihapus**, digantikan `Anomaly(...)` |
| `Billing/Dtos/BillingInvoiceDtos.cs` | `CalculationItemResponse` +`ItemDataAnomalyAmount`/`TaxDataAnomalyAmount`; `AdministrationFeeCalculationResponse`/`RoomChargeCalculationResponse` +`DataAnomalyAmount`; `CoverageCalculationResponse` +`DataAnomalyAmount`/`HasDataAnomaly`/`AnomalyCodes`/`AnomalyMessages` |
| `Billing/Services/BillingCalculationService.cs` | `outcomeByComponent` copy-back menyalin `DataAnomalyAmount` untuk item/pajak/admin-fee/room-charge; `ApplyCoverageWaterfall` menambah `BIL-VAL-035`/`037`, meretarget `BIL-VAL-036`, mengisi field baru pada `CoverageCalculationResponse` |
| `Tests/.../BillingCalculationServiceTests.cs` | Lima titik konstruksi `BillingCoverageDecision`/`BillingCoverageComponentOutcome` disesuaikan arity (`FixedCoverageAdapter` ×2, `AllocatingCoverageAdapter`, `MisallocatingCoverageAdapter`, `SelfPayCoverageAdapter`, plus satu di `BillingAllocationServiceTests.cs`); satu test lama (`RejectedCoverageRemainsUnresolvedAndDoesNotShiftToPatient`) diganti nama dan dibalik jadi `RejectedCoverageWithoutDataAnomalyIsRejected` (BIL-AT-043); tiga test baru (`RegistrationCoverageAdapterPayerNotEligibleBecomesAnomalyAndPatientPortion`, `...PolicyInactiveBecomesAnomalyWithCorrectCode`, `...MissingInsuranceProviderBecomesAnomalyWithCorrectCode`) |

Total: **4 berkas source/test berubah** (termasuk satu berkas tersentuh di luar direktori Billing
langsung: `BillingAllocationServiceTests.cs`, hanya karena memakai `BillingCoverageDecision` yang
sama). Tidak ada DTO/controller/entity/migration baru selain field yang disebut di atas.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Additive.** Tujuh field baru pada empat DTO response yang sudah ada (`CalculationItemResponse` ×2, `AdministrationFeeCalculationResponse` ×1, `RoomChargeCalculationResponse` ×1, `CoverageCalculationResponse` ×4 termasuk `IsPerItemAllocationAvailable` yang sudah ada sejak `BE-BKC-022`). Tidak ada field yang dihapus atau berubah arti. Satu perilaku **berubah nilainya**: empat precondition yang dulu menghasilkan `unresolved` kini menghasilkan `primaryAmount=0` + `dataAnomalyAmount` terisi — perubahan finansial yang memang tujuan task |
| `DATABASE IMPACT` | **Nihil.** Tidak ada kolom, index, entity, maupun migration. Field baru hidup di dalam `BreakdownSnapshot` JSON yang sudah ada |
| `SECURITY IMPACT` | **Nihil.** Tidak ada perubahan authorization. `Message` pada `BillingCoverageAnomaly` **tidak** memuat nomor polis, nama pasien, atau data sensitif lain — seluruhnya kalimat generik per kode |
| `VISUAL REFERENCE` | `NOT REQUIRED` — konsumen frontend (`FE-BKC-020`) di luar scope task ini |

## 4. Dokumentasi endpoint

**Tidak ada endpoint baru.** Field bertambah pada response dua endpoint yang sama dengan
`BE-BKC-022`/`024`:

| Method | Path | Dampak |
| --- | --- | --- |
| `GET` | `.../billing/invoices/{id:guid}/calculation-preview` | `breakdown.coverage.dataAnomalyAmount`/`hasDataAnomaly`/`anomalyCodes`/`anomalyMessages` baru; `breakdown.items[].itemDataAnomalyAmount`/`taxDataAnomalyAmount` baru; `breakdown.administrationFee`/`roomCharge.dataAnomalyAmount` baru |
| `POST` | `.../billing/invoices/{id:guid}/recalculate` | Sama |

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Perhitungan berhasil. **Termasuk** ketika ada anomali data penjamin — anomali bukan kegagalan permintaan |
| `422` | Perhitungan melanggar batas yang dijaga — **termasuk `BIL-VAL-035`/`036`/`037` yang baru** |

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna: build backend dijalankan manual |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas |
| Verifikasi statis seluruh titik konstruksi record | **LULUS** | Manual | `grep -rn "new BillingCoverageDecision(\|new BillingCoverageComponentOutcome("` pada `Areas/` dan `Tests/` menunjukkan tepat 13 titik, seluruhnya sudah memakai arity baru (10 dan 5 argumen berturut-turut) |
| Verifikasi statis rujukan ke `Unresolved(...)` yang dihapus | **LULUS** | Manual | `grep -n "Unresolved("` pada `BillingCoverageAdapter.cs` hanya menyisakan penyebutan di dalam komentar, tidak ada pemanggilan method |
| Verifikasi statis formula `PatientAmount` untuk jalur anomali | **LULUS (analisis manual)** | Manual | Untuk jalur `Anomaly()`: `primary=0`, `unresolved=0`, sehingga `residualAfterExcess = eligibleAmount` dan `PatientAmount = residualAfterExcess - unresolvedAmount = eligibleAmount` — rumus `ApplyCoverageWaterfall` **tidak perlu diubah** untuk memastikan nominal anomali jatuh ke pasien; identitas ini didokumentasikan di komentar `Anomaly()` |
| Cakupan diff | **LULUS** | Manual | `git status --short` menunjukkan tepat 4 berkas task ini; nol migration, nol entity, nol controller |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** Build dan test wajib
> dijalankan pengguna. Laporan ini mencatat keadaan sebenarnya, bukan mengklaim keberhasilan.

### Test yang diubah

| Test | Sebelumnya | Sesudah |
| --- | --- | --- |
| `RejectedCoverageRemainsUnresolvedAndDoesNotShiftToPatient` → **diganti nama** `RejectedCoverageWithoutDataAnomalyIsRejected` | Assert `PatientAmount=0`, `UnresolvedCoverageAmount=100.000` (decision REJECTED tanpa anomali **lolos**) | Assert `BillingCalculationValidationException` dilempar (`BIL-VAL-036` retargeted kini menolaknya), tidak ada versi kalkulasi dibuat |

### Test baru

| Test | Membuktikan |
| --- | --- |
| `RegistrationCoverageAdapterPayerNotEligibleBecomesAnomalyAndPatientPortion` | `BIL-AT-041` — `IsEligible=false` dengan rule `Covered` yang sebenarnya cocok tetap menghasilkan `primaryAmount=0`, `patientAmount=100.000`, `unresolvedAmount=0`, `dataAnomalyAmount=100.000`, kode `PAYER_NOT_ELIGIBLE`, satu pesan berisi kata "Registrasi" |
| `RegistrationCoverageAdapterPolicyInactiveBecomesAnomalyWithCorrectCode` | `BIL-AT-042` — `IsPolicyActive=false` menghasilkan kode `POLICY_INACTIVE` yang tepat (nominal sudah dibuktikan test di atas) |
| `RegistrationCoverageAdapterMissingInsuranceProviderBecomesAnomalyWithCorrectCode` | `INSURANCE_PROVIDER_MISSING` — cakupan tambahan untuk DoD "empat kode dihasilkan pada keadaan yang benar" |

**`ENCOUNTER_NOT_FOUND` sengaja tidak diuji lewat integration test.** `BillingCalculationService.CalculateAsync`
sudah memuat encounter (dan gagal lebih dulu dengan `KeyNotFoundException` bila tidak ada) sebelum
memanggil adapter ini — jalur itu secara struktural tidak dapat dipicu lewat pipeline normal tanpa
memanipulasi database di tengah satu perhitungan yang sama. Menguji ini butuh cara yang dipaksakan
(mis. adapter palsu terpisah); dicatat sebagai batasan verifikasi, bukan dihindari diam-diam.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| `BIL-AT-041` — kelayakan `false`: `200`, `dataAnomalyAmount=440000`-setara, seluruh nominal ke pasien, `primaryAmount` nol | **Tercakup** — nominal uji `100.000` (bukan `440.000` dari contoh dokumen; nilai berbeda, logika identik) |
| `BIL-AT-042` — polis tidak aktif → kode `POLICY_INACTIVE` | **Tercakup** |
| `BIL-AT-043` — jalur `REJECTED` dipaksa tanpa anomali → `422`, versi kalkulasi tidak dibuat | **Tercakup** |
| `BIL-VAL-035` — nominal anomali tidak melebihi biaya yang memenuhi syarat | **Terpenuhi di kode**; tidak ditambah test overflow terpisah — proporsional, karena jalur `Anomaly()` satu-satunya sumber `DataAnomalyAmount` selalu menyetelnya persis sama dengan `coverableAmount` (tidak pernah lebih), sehingga overflow hanya dapat diuji lewat adapter palsu yang sengaja cacat (pola sama seperti `MisallocatingCoverageAdapter` untuk `BIL-VAL-028`) — tidak dibuat karena bukan acceptance criteria bernomor pada roadmap task ini |
| Nominal anomali tidak pernah muncul sebagai baris di Ringkasan Pembayaran | **Di luar scope backend** — ini aturan tampilan frontend (`FE-BKC-020`); backend hanya menyediakan `hasDataAnomaly`/`anomalyCodes`/`anomalyMessages` terpisah dari `items[]` |
| Empat kode dihasilkan pada keadaan yang benar | **Tercakup** untuk tiga kode (`PAYER_NOT_ELIGIBLE`, `POLICY_INACTIVE`, `INSURANCE_PROVIDER_MISSING`); `ENCOUNTER_NOT_FOUND` diverifikasi statis (pembacaan kode), lihat § 5 |
| Tagihan beranomali tetap dapat dibayar sampai selesai | **Terpenuhi by design** — kalkulasi mengembalikan `200`, tidak ada perubahan pada jalur pembayaran |
| Kode anomali ikut tersimpan pada versi kalkulasi | **Terpenuhi** — `AnomalyCodes`/`AnomalyMessages` bagian dari `CoverageCalculationResponse` yang ikut ke `BreakdownSnapshot` |
| Build dan test lulus | **BELUM** — menunggu pengguna |

**Definition of Done belum tercapai.** Yang tersisa hanya menjalankan build dan test.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Perubahan ini mengubah nominal finansial **seluruh** tagihan asuransi yang precondition penjaminnya bermasalah — bukan hanya data uji. Tagihan yang sebelumnya menampilkan nominal menggantung untuk kasus ini akan mulai menampilkan Subtotal Mandiri terisi beserta peringatan setelah build/test dijalankan dan dirilis |
| `KNOWN ISSUES` | `ENCOUNTER_NOT_FOUND` tidak dapat diverifikasi lewat integration test tanpa cara yang dipaksakan — lihat § 5. `PrimaryStatus` untuk jalur anomali disetel `"NO_COVERAGE"` (bukan string baru) — keputusan desain yang tidak eksplisit disebutkan dokumen arsitektur, diambil supaya konsumen lama yang membaca `PrimaryStatus` mentah tidak salah mengira anomali sebagai penolakan klaim; dicatat di komentar `Anomaly()` |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi |
| `INCIDENTAL CHANGES` | Satu berkas di luar direktori Billing (`Tests/.../BillingAllocationServiceTests.cs`) tersentuh **hanya** untuk menyesuaikan arity `BillingCoverageDecision` pada `SelfPayCoverageAdapter` lokalnya sendiri — bukan perluasan scope, murni konsekuensi record positional yang sama dipakai lintas berkas test |
| `INTERRUPTIONS` | File test berubah di disk di antara sesi kerja (kemungkinan dari task `BE-BKC-026` yang berjalan sebelumnya pada sesi yang sama) — dibaca ulang sebelum menyunting lebih lanjut, tidak ada penyuntingan ganda |
| `GIT STATUS` | 4 berkas task ini berubah, belum di-stage. Working tree juga memuat perubahan tersendiri dari `BE-BKC-022`/`023`/`024` (berkas tumpang tindih pada `BillingCoverageAdapter.cs`, `BillingCalculationService.cs`, `BillingCalculationServiceTests.cs` — task ini melanjutkan DI ATAS perubahan itu, bukan menimpanya) yang belum di-build/test pengguna. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet build` dan `dotnet test` secara manual mencakup seluruh perubahan yang menumpuk (`BE-BKC-022` s.d. `025`). Bila lulus, task ini selesai dan `BE-BKC-026` (sudah `READY`, murni verifikasi tanpa kode) atau `BE-BKC-027` (migration, menunggu `BE-BKC-025` selesai) dapat dilanjutkan sesuai urutan backend-dulu yang diminta pengguna |
