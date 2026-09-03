# Laporan Perubahan Backend — `BE-BKC-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-020` |
| Judul | Endpoint preview coverage per tarif |
| Slice | Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026, blueprint revisi `0.5 approved`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-020` (baris 334–349) |
| Trace | `BKC-DEC-060`; `FR-BKC-005`; `CAP-04`; acceptance `BIL-AT-027`, `028`; `BIL-VAL-027` |
| Contract version | `BIL-API-0.4` (approved). Endpoint `GET /catalog-charges/coverage-preview` sudah dispesifikasi sebagai "Rencana (belum tersedia)" pada `contracts/api-contract.md` amendment 2 September 2026; Integration `BIL-INT-010` (in-process, `contracts/integration-contract.md`) |
| Dependency | `BE-BKC-001` saja. **Independen** dari `BE-BKC-018`/`019` — dikerjakan paralel pada sesi yang sama |
| Klasifikasi | `MEDIUM` — skor 5 (repo 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan 1, UI/workflow 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/{Dtos,Services,Controllers}`, `QuilvianSystemBackend.Tests/BillingManagement/`, `docs/module-blueprints/billing-kasir/` |
| Model | Claude Sonnet 5 |
| Governance Preflight | Area `HealthServices`; Module/pemilik `BillingManagement / Billing`; prefix `Bil` (registry, Lifecycle `ACTIVE`). Reuse lintas-module: `InsuranceCoverageService`/`EncounterInsuranceService` milik `ClinicalManagement` (juga `ACTIVE` pada registry), dipanggil via DI, bukan disalin/ditulis ulang. Keberlakuan `NEW CODE` (endpoint dan method baru; tidak ada entity operasional baru) |
| QBE ID berlaku | `QBE-SVC-001` (orkestrasi di Module Service; controller tidak akses `DbContext`), `QBE-API-001` (pola endpoint mengikuti `GetActiveEncounterOptions`/`AddOtherCharge`), `QBE-PERM-001` (reuse permission `BillingInvoice:Read`), `QBE-DTO-001` (DTO baru tidak membocorkan `InsuranceCoverageResult` mentah — lihat § 1) |
| Commit backend saat dikerjakan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source lengkap (DTO, constructor injection, method, endpoint) beserta 2 test domain. **Build/test sengaja tidak dijalankan oleh sesi ini** — atas instruksi eksplisit pengguna ("jangan lakukan build dibackground biar saya cek sendiri"), pengguna memverifikasi sendiri secara manual. Belum boleh ditandai selesai sampai hasil verifikasi dikonfirmasi |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, kasir/penguji tidak punya cara untuk mengetahui **sebelum** sebuah tarif
ditambahkan ke tagihan apakah tarif itu akan tercover asuransi pasien atau tidak, dan berapa
kira-kira sisa yang harus dibayar pasien. Satu-satunya cara mengetahuinya adalah menambahkan item
itu dulu ke invoice lalu menjalankan kalkulasi penuh — proses yang punya efek samping (membuat versi
kalkulasi baru) dan tidak cocok dipakai sekadar untuk "coba-coba lihat dulu" sebelum benar-benar
menagih.

Task ini menambahkan endpoint **baca-saja** (tanpa menulis apa pun) yang menjawab pertanyaan itu
langsung: diberi kunjungan pasien, tarif, dan kuantitas, sistem mengembalikan status coverage-nya
(tercover penuh, sebagian, tidak tercover, atau butuh approval), beserta perkiraan nominal yang
ditanggung asuransi dan yang harus dibayar pasien — **tanpa** membuat baris tagihan atau mengubah
apa pun di database.

Contoh konkret: kasir ingin tahu apakah "MRI Kepala" akan dicover asuransi BPJS pasien sebelum
benar-benar menambahkannya ke tagihan. Lewat endpoint ini, sistem langsung menjawab "Covered,
ditanggung Rp1.200.000 dari Rp1.500.000, pasien bayar Rp300.000" tanpa ada satu pun baris tagihan
yang tercipta.

---

## 2. Proses bisnis

**Tujuan**: sistem menjawab "apakah tarif ini tercover untuk pasien ini" sebelum item ditambahkan,
tanpa efek samping.

**Pelaku**: kasir (lewat frontend `FE-BKC-014`/`015`, belum dikerjakan) atau penguji API langsung.

**Pemicu**: kasir memilih tarif dan kuantitas pada layar entri, sebelum menekan tombol "Tambah".

**Langkah**:

1. Client memanggil `GET /catalog-charges/coverage-preview?encounterId=...&tariffId=...&quantity=...`.
2. Server memanggil `InsuranceCoverageService.ResolveTariffAsync` yang **sudah ada** (dimiliki
   modul Clinical Management, satu-satunya tempat kalkulasi coverage untuk resep maupun tindakan
   di seluruh sistem) — task ini **tidak menulis ulang** logika pencocokan tarif asuransi/rule
   coverage sedikit pun, murni memanggilnya lewat dependency injection.
3. Method itu membaca konteks pembayaran kunjungan (tunai atau asuransi, provider apa, plan apa),
   mencari tarif kontrak asuransi (`MstInsuranceTariff`) dan rule coverage (`MstInsuranceCoverageRule`)
   yang paling spesifik cocok, lalu menghitung nominal tertanggung dan nominal pasien.
4. Hasilnya dipetakan **sebagian saja** ke response — status coverage, persentase, harga satuan,
   total, nominal tertanggung, nominal pasien, dan flag butuh-approval. Field yang bersifat
   internal/operasional untuk approver (`ApprovalInstruction`, `InsuranceCoverageRuleId`,
   `BillingInstruction`) **sengaja tidak diteruskan** — preview ini konsumsinya kasir pada layar
   entri, bukan alur approval.
5. Response selalu membawa `IsAdvisory=true` — penanda eksplisit bahwa angka ini **bukan** angka
   final. Kalkulasi final invoice sungguhan tetap lewat `RegistrationBillingCoverageAdapter` saat
   invoice benar-benar dihitung (`BE-BKC-021`), dan angkanya **boleh berbeda** dari preview ini
   (didokumentasikan eksplisit pada `§ 16.2.A` blueprint — lihat `BIL-AT-028`).

**Aturan yang berlaku**: `BIL-VAL-027` (validasi encounter/tarif). Rule dengan `IsNeedApproval=true`
**tetap dihitung tercover penuh** pada preview ini — approval hanya informasi bagi kasir, bukan
alasan preview menjawab "tidak tercover" atau gagal (`BIL-AT-027`).

**Status yang dihasilkan**: tidak ada — endpoint ini murni baca, tidak menyentuh invoice, item,
atau status apa pun.

**Jalur tidak normal**: `EncounterId`/`TariffId` tidak ditemukan, tidak aktif, atau konteks
pembayaran kunjungan tidak valid (mis. sumber pembayaran belum lengkap, polis sudah berakhir) →
`422` dengan pesan yang menjelaskan alasannya (diteruskan apa adanya dari
`InsuranceCoverageService`, tidak diterjemahkan ulang).

**Hasil akhir**: kasir mendapat perkiraan coverage sebelum menekan tombol tambah, tanpa risiko
membuat data yang tidak jadi dipakai.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/billing-kasir/{roadmap/backend-roadmap.md, contracts/api-contract.md,
testing/acceptance-test-matrix.md}`; `Areas/HealthServices/ClinicalManagement/Services/
{InsuranceCoverageService.cs, EncounterInsuranceService.cs}`; `Areas/HealthServices/
BillingManagement/Billing/{Services/BillingInvoiceService.cs, Dtos/BillingInvoiceDtos.cs,
Controllers/BillingInvoicesController.cs}`; `Areas/HealthServices/MasterData/Models/
{MstInsuranceTariff.cs, MstInsuranceCoverageRule.cs}`; `Areas/HealthServices/PatientManagement/
MasterData/Models/MstPatientInsurance.cs`; `Areas/Administrator/MasterData/Models/
MstInsuranceProvider.cs`; `Areas/HealthServices/RegistrationManagement/Models/
TrxPatientEncounterGuarantor.cs`; `Areas/HealthServices/RegistrationManagement/Enums/
EncounterPaymentType.cs`; `Repositories/ApplicationDbContext.cs` (nama `DbSet` seluruh entity di
atas); `Program.cs` (konfirmasi `InsuranceCoverageService` sudah `AddScoped`);
`QuilvianSystemBackend.Tests/BillingManagement/BillingInvoiceServiceTests.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Billing/Dtos/BillingInvoiceDtos.cs` | DTO baru `CatalogChargeCoveragePreviewResponse` — subset field dari `InsuranceCoverageResult` (lihat § 1 untuk daftar field yang sengaja dikecualikan) |
| `Areas/.../Billing/Services/BillingInvoiceService.cs` | Constructor injection `InsuranceCoverageService` baru (parameter ke-6, tidak mengubah urutan parameter lama); method baru `GetCatalogChargeCoveragePreviewAsync` (memanggil `ResolveTariffAsync`, memetakan hasil, melempar `BillingInvoiceValidationException` bila `!IsValid`) |
| `Areas/.../Billing/Controllers/BillingInvoicesController.cs` | Action baru `GET /catalog-charges/coverage-preview` — `[AccessAction("Read", ..., SortOrder=14)]`, `[AccessPermission("BillingInvoice","Read")]`, catch `BillingInvoiceValidationException` → `422` |
| `QuilvianSystemBackend.Tests/BillingManagement/BillingInvoiceServiceTests.cs` | Helper `CreateService` diperbarui menyuntik `InsuranceCoverageService` baru (satu-satunya titik konstruksi `BillingInvoiceService` di seluruh test project — dicek lewat pencarian menyeluruh, tidak ada pemanggil lain yang perlu diperbarui); 2 test domain baru (§ 5); tambah `using` `Areas.Administrator.MasterData.Models` dan `Areas.HealthServices.ClinicalManagement.Services` |
| `docs/module-blueprints/billing-kasir/contracts/api-contract.md` | Status baris `GET /catalog-charges/coverage-preview` diperbarui dari "Rencana (belum tersedia)" menjadi "Diimplementasikan (backend, belum diverifikasi manual)"; catatan tanggal ditambahkan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /catalog-charges/coverage-preview` — sudah dispesifikasi sebelumnya di `contracts/api-contract.md` (status berubah dari "Rencana" menjadi "Diimplementasikan"). Tidak mengubah endpoint yang sudah ada |
| Database | `NOT APPLICABLE` — endpoint baca-saja, tidak ada `SaveChanges`, tidak ada migration. Sesuai DoD "Endpoint read-only tanpa transaksi" |
| Keamanan/Auth | Reuse permission `BillingInvoice : Read` yang sudah ada. `[AccessAction]`/`[AccessPermission]` dipasang sesuai `role-access-rules.md`. Response **sengaja tidak** membawa field internal rule (`ApprovalInstruction`, `InsuranceCoverageRuleId`, `BillingInstruction`) sesuai catatan risiko roadmap — lihat § 1 dan § 3.2 |

---

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/catalog-charges/coverage-preview` | Preview advisory (bukan angka final) apakah satu tarif tercover asuransi pasien pada satu kunjungan, beserta perkiraan nominal — baca-saja, tidak membuat baris tagihan (`BKC-DEC-060`) | `BillingInvoice : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` / `dotnet test` | **Sengaja tidak dijalankan oleh sesi ini** | `NOT RUN` | Instruksi eksplisit pengguna: "jangan lakukan build dibackground biar saya cek sendiri" — pengguna memverifikasi sendiri secara manual |
| `CatalogChargeCoveragePreviewIsCoveredEvenWhenRuleNeedsApproval` (`BIL-AT-027`) | Ditulis, **belum dijalankan** | `NOT RUN` | Kode test di `BillingInvoiceServiceTests.cs`; men-seed skenario asuransi lengkap (provider, polis pasien, tarif, tarif kontrak asuransi, rule coverage dengan `IsNeedApproval=true`) dan memverifikasi `CoverageStatus != "NotCovered"`, `CoveredAmount > 0`, `IsNeedApproval == true`, `IsAdvisory == true` |
| `CatalogChargeCoveragePreviewRejectsUnknownEncounter` (`BIL-VAL-027`) | Ditulis, **belum dijalankan** | `NOT RUN` | Memanggil dengan `encounterId`/`tariffId` acak pada database kosong, memverifikasi `BillingInvoiceValidationException` dilempar |

Uji manual: `NOT FEASIBLE` pada sesi ini — endpoint baru, belum ada frontend (`FE-BKC-014`/`015`
belum dikerjakan) untuk klik-coba; verifikasi lewat Swagger/Postman terautentikasi belum dilakukan.

**Tidak dijalankan:** seluruh build/test (lihat baris pertama tabel di atas — keputusan sadar
pengguna, bukan kelalaian); eksekusi database (`NOT APPLICABLE` — endpoint baca-saja); sanitasi
contoh Swagger (di luar scope task, belum ada task tersendiri).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-027` — tarif dengan rule `CoverageStatus=Covered, IsNeedApproval=true` untuk pasien asuransi → preview mengembalikan `CoveredAmount`/`PatientPayAmount` terhitung penuh, `IsNeedApproval=true` hanya info, BUKAN `NotCovered`/gagal | Terpenuhi (source + test, **belum dijalankan** — lihat § 5) | `CatalogChargeCoveragePreviewIsCoveredEvenWhenRuleNeedsApproval` |
| `BIL-AT-028` — disparitas preview vs kalkulasi final terdokumentasi | Terpenuhi (dokumentasi) | Komentar eksplisit pada `CatalogChargeCoveragePreviewResponse` dan `GetCatalogChargeCoveragePreviewAsync` menyatakan preview ini bisa berbeda dari kalkulasi final `RegistrationBillingCoverageAdapter`; `IsAdvisory=true` sebagai penanda runtime untuk FE menampilkan disclaimer |
| `BIL-VAL-027` — validasi encounter/tarif | Terpenuhi (source + test, **belum dijalankan**) | `CatalogChargeCoveragePreviewRejectsUnknownEncounter`; `!result.IsValid` dari `ResolveTariffAsync` dipetakan ke `BillingInvoiceValidationException` |
| Response **MUST NOT** membocorkan field internal rule (`RuleCode`/`ApprovalInstruction`) | Terpenuhi | `CatalogChargeCoveragePreviewResponse` tidak memiliki field `ApprovalInstruction`, `InsuranceCoverageRuleId`, `BillingInstruction`, atau `RuleCode` — hanya field ringkasan untuk kasir (lihat § 3.2) |
| DoD: Endpoint read-only tanpa transaksi; tests lulus; build lulus | **Belum sepenuhnya terpenuhi** — endpoint read-only tanpa transaksi sudah terpenuhi (tidak ada `SaveChanges` sama sekali pada jalur ini); tests ditulis tapi **belum dijalankan**; build **belum dijalankan** (lihat § 5) | Lihat § 5 dan § 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `BillingInvoiceService` mendapat parameter constructor baru (`InsuranceCoverageService`) — **satu-satunya** titik konstruksi manual di seluruh codebase adalah helper `CreateService` pada `BillingInvoiceServiceTests.cs` (dikonfirmasi lewat pencarian `new BillingInvoiceService(` di seluruh repository, hanya 1 hasil), sudah diperbarui. Registrasi DI produksi (`Program.cs`) tidak perlu diubah karena constructor injection ASP.NET Core menyuntik otomatis berdasarkan tipe parameter, dan `InsuranceCoverageService` sudah terdaftar `AddScoped` sebelumnya |
| Masalah yang diketahui | Preview ini **tidak** memperhitungkan diskon tingkat invoice, coverage waterfall multi-item, atau limit polis yang sudah terpakai item lain pada invoice yang sama — ini bukan bug, tapi keterbatasan yang memang disengaja dan didokumentasikan (`BIL-AT-028`, `§ 16.2.A`): preview per-tarif tunggal, bukan simulasi invoice penuh. Kalkulasi final tetap satu-satunya sumber angka mengikat |
| Risiko tersisa | Test domain (2 test baru) dan build **belum pernah dijalankan sama sekali** pada sesi ini (bukan pra-eksisting yang gagal seperti `BE-BKC-018`/`019` — benar-benar belum dieksekusi). Ada kemungkinan realistis ditemukan galat kompilasi atau assertion yang salah saat pengguna menjalankannya sendiri, mengingat skenario seed asuransi lengkap (7 entity berbeda) ditulis tanpa verifikasi compile sama sekali |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna secara eksplisit meminta agar sesi ini **tidak** menjalankan build/test sama sekali, baik di latar depan maupun latar belakang ("jangan lakukan build dibackground biar saya cek sendiri") — sebagai respons atas dua percobaan build latar belakang pada `BE-BKC-019` yang saling bertabrakan sebelumnya. Instruksi ini diikuti penuh pada task ini: seluruh implementasi ditulis dan ditinjau lewat pembacaan source (bukan eksekusi), tanpa satu pun perintah `dotnet build`/`dotnet test` dijalankan |
| Status Git | `On branch Yasmina, up to date with 'origin/Yasmina'`. Modified: `Areas/HealthServices/BillingManagement/Billing/{Controllers/BillingInvoicesController.cs, Dtos/BillingInvoiceDtos.cs, Models/BilInvoiceItem.cs, Services/BillingChargeSourceAdapter.cs, Services/BillingInvoiceService.cs}`, `Migrations/ApplicationDbContextModelSnapshot.cs`, `QuilvianSystemBackend.Tests/BillingManagement/BillingInvoiceServiceTests.cs`, `Repositories/Configurations/.../BilInvoiceItemConfiguration.cs`, `docs/module-blueprints/billing-kasir/{contracts/api-contract.md, roadmap/backend-roadmap.md, roadmap/requirement-traceability.md}`. Untracked: `Migrations/20260903015730_AddTariffIdToBilInvoiceItem.{cs,Designer.cs}`, `QuilvianSystemBackend.Tests/BillingManagement/BillingChargeSourceAdapterTests.cs`, `docs/module-blueprints/billing-kasir/task/report/backend/{BE-BKC-018.md,BE-BKC-019.md,BE-BKC-020.md}`. Belum staged/commit — `BE-BKC-018`/`019`/`020` berbagi working tree yang sama pada sesi ini |
| Langkah berikutnya | 1) **Pengguna menjalankan `dotnet build` lalu `dotnet test --filter "FullyQualifiedName~Billing"` secara manual** dan membagikan hasilnya — ini prasyarat mutlak sebelum `BE-BKC-018`/`019`/`020` boleh ditandai selesai. 2) Bila ada galat kompilasi pada test coverage-preview (skenario paling berisiko karena belum pernah dicek), laporkan agar diperbaiki. 3) Setelah ketiga task ini terverifikasi, `BE-BKC-021` (penyempitan gating approval, **berisiko tinggi/global**) adalah task tersisa terakhir pada slice ini — butuh konfirmasi otorisasi eksplisit terpisah sesuai `BKC-DEC-062` sebelum dikerjakan |
