# Laporan Perubahan Backend — `BE-BKC-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-018` |
| Judul | Fondasi katalog tarif pada `BilInvoiceItem` |
| Slice | Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026, blueprint revisi `0.5 approved`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-018` (baris 291–304) |
| Trace | `BKC-DEC-059`, `BKC-DEC-061`; `CAP-02`, `CAP-06` (`01-existing-capability-map.md` § 16); `FR-BKC-001`, `FR-BKC-004` (`04-prd-to-mvp.md`) |
| Contract version | `BIL-API-0.4` (approved). Amendment `02-backend-architecture.md` § *Amendment 2 September 2026*. Tidak ada status baru pada `contracts/state-transition-matrix.md` |
| Dependency | `BE-BKC-001` (sudah ada, tidak diverifikasi ulang pada task ini); tidak bergantung task lain pada slice ini |
| Klasifikasi | `MEDIUM` — skor 7 (repo 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 1, kontrak API 1, database 2, keamanan 0, UI/workflow 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/{Models,Services,Dtos}`, `Repositories/Configurations/.../BilInvoiceItemConfiguration.cs`, `Migrations/` (source saja, DB tidak dijalankan), `QuilvianSystemBackend.Tests/BillingManagement/`, `docs/module-blueprints/billing-kasir/` |
| Model | Claude Sonnet 5 |
| Governance Preflight | Area `HealthServices`; Module/pemilik `BillingManagement / Billing`; prefix `Bil` (registry `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, Lifecycle `ACTIVE`); keberlakuan `TOUCHED LEGACY` (menambah kolom pada entity `BilInvoiceItem` yang sudah ada, tidak membuat entity operasional baru) |
| QBE ID berlaku | `QBE-CFG-001` (configuration FK+index baru), `QBE-ENT-002` (nullability `TariffId` mengikuti semantik domain — nullable karena entri lama/ADHOC tidak punya tarif induk), `QBE-DTO-001` (DTO tidak membocorkan entity EF) |
| Commit backend saat dikerjakan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source lengkap dan build lulus (`0 Error`); migration ter-generate sebagai source (DB belum dijalankan — sesuai DoD). Dua kegagalan test pra-eksisting ditemukan dan dikonfirmasi tidak terkait task ini (lihat § 5 dan § 7) |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, `BilInvoiceItem` (baris tagihan pada invoice) sama sekali tidak punya cara
untuk mencatat bahwa harganya berasal dari daftar tarif resmi (`MstTariff`). Setiap item hanya
berupa deskripsi dan harga bebas yang dicatat kasir — tidak ada bedanya secara data antara
"Konsultasi Dokter Spesialis Rp150.000" yang diambil dari tarif resmi rumah sakit dengan "Biaya
Fotokopi Rp5.000" yang memang sengaja diketik manual. Akibatnya laporan/audit tidak bisa memisahkan
mana tagihan yang harganya dijamin sama dengan tarif resmi dan mana yang harganya murni ketikan
kasir.

Selain itu, layar pemilihan kunjungan pasien (dipakai form "Buat Invoice Manual") tidak
mengekspos unit layanan, klinik, atau kelas pasien dari kunjungan tersebut — padahal informasi ini
dibutuhkan supaya nanti (`BE-BKC-019`, `FE-BKC-014`) picker tarif katalog bisa menyaring hanya
tarif yang benar-benar berlaku untuk kunjungan itu. Contoh konkret: tarif "Rawat ICU per hari"
seharusnya tidak pernah muncul sebagai pilihan ketika kasir menagih pasien rawat jalan Poli Anak;
tanpa `ServiceUnitId`/`ClinicId`/`PatientClassId` pada respons, frontend tidak punya dasar untuk
menyaringnya.

Task ini adalah **fondasi data saja** — belum ada endpoint baru untuk benar-benar menambah charge
dari katalog (itu scope `BE-BKC-019`, sudah dikerjakan pada sesi yang sama, lihat laporan
`BE-BKC-019.md`).

---

## 2. Proses bisnis

**Tujuan**: menyiapkan kolom referensi tarif pada baris tagihan, siklus hidup source-domain baru
untuknya, dan konteks kunjungan yang dibutuhkan untuk memfilter tarif — tanpa mengubah perilaku
transaksi yang sudah berjalan.

**Pelaku**: tidak ada aktor manusia langsung pada task ini (murni fondasi backend); pelaku
downstream-nya adalah kasir (lewat `BE-BKC-019`) dan frontend picker tarif (`FE-BKC-014`).

**Langkah**:

1. `BilInvoiceItem` mendapat kolom baru `TariffId` (`Guid?`, nullable) beserta navigasi ke
   `MstTariff`. Nullable karena mayoritas item yang sudah ada — baik dari domain producer klinis
   (`PROCEDURE`, `LABORATORY`, dst.) maupun entri bebas kasir (`ADHOC`) — memang tidak punya tarif
   induk resmi.
2. `BillingChargeSourceAdapter` (yang mengatur siklus hidup tiap source-domain: status apa yang
   boleh ditagihkan, kapan boleh dibatalkan, kapan dianggap "pelayanan sudah selesai") mendapat
   entri baru `"ADHOC_CATALOG"`. Siklus hidupnya persis sama dengan `"ADHOC"` yang sudah ada —
   begitu dicatat, langsung dianggap selesai (`completeOnEntry`) karena tidak menunggu pemenuhan
   order dari modul klinis manapun, dan masih bisa dibatalkan kasir sendiri selama invoice belum
   final. Bedanya murni penamaan, supaya laporan/audit bisa membedakan "item dari katalog tarif
   resmi" dari "item ketikan bebas kasir".
3. Respons daftar kunjungan aktif (`GET .../encounter-options`, dipakai form Buat Invoice Manual)
   diperluas dengan tiga field baru: `ServiceUnitId`, `ClinicId`, `PatientClassId` — diambil
   langsung dari kunjungan (`TrxPatientEncounter`) yang sama, tanpa query tambahan.

**Aturan yang berlaku**: kolom `TariffId` memakai `DeleteBehavior.Restrict` pada foreign key-nya —
artinya sebuah `MstTariff` yang sudah pernah dipakai untuk menagih **tidak boleh dihapus** dari
master data selama masih dirujuk baris tagihan manapun; penghapusan akan ditolak database, bukan
diam-diam menghapus riwayat tagihan.

**Status yang dihasilkan**: tidak ada status baru pada invoice maupun item — task ini murni
menambah kolom referensi dan satu source-domain baru, bukan mengubah mesin status yang sudah ada.

**Jalur tidak normal**: tidak berlaku pada task ini (tidak ada endpoint baru yang bisa gagal secara
runtime; validasinya murni pada level database/EF configuration).

**Hasil akhir**: `BilInvoiceItem` siap menyimpan referensi tarif resmi begitu `BE-BKC-019`
mengaktifkan endpoint penulisnya; picker kunjungan sudah membawa konteks yang dibutuhkan untuk
menyaring tarif per unit layanan/klinik/kelas pasien.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/billing-kasir/{roadmap/backend-roadmap.md, contracts/api-contract.md,
contracts/erd/01-billing-account-charge.md}`; `Areas/HealthServices/BillingManagement/Billing/
{Models/BilInvoiceItem.cs, Services/BillingChargeSourceAdapter.cs, Services/
BillingInvoiceService.cs, Dtos/BillingInvoiceDtos.cs}`; `Areas/HealthServices/MasterData/Models/
MstTariff.cs`; `Repositories/{ApplicationDbContext.cs, Configurations/HealthServices/
BillingManagement/Billing/BilInvoiceItemConfiguration.cs}`; `QuilvianSystemBackend.Tests/
BillingManagement/{BillingInvoiceServiceTests.cs, BillingFinalizationServiceTests.cs,
BillingCalculationServiceTests.cs}`; `QuilvianEngineeringSkills/Claude/.claude/rules/backend/
engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Billing/Models/BilInvoiceItem.cs` | Tambah `Guid? TariffId` dan navigasi `MstTariff? Tariff` |
| `Repositories/Configurations/.../BilInvoiceItemConfiguration.cs` | Tambah `HasOne(Tariff).WithMany().HasForeignKey(TariffId).OnDelete(Restrict)` dan `HasIndex(TariffId)` |
| `Areas/.../Billing/Services/BillingChargeSourceAdapter.cs` | Tambah `SourcePolicies["ADHOC_CATALOG"]` — siklus hidup identik `"ADHOC"` (`completeOnEntry: true`) |
| `Areas/.../Billing/Dtos/BillingInvoiceDtos.cs` | `ActiveEncounterOptionResponse` diperluas `ServiceUnitId` (`Guid`), `ClinicId`/`PatientClassId` (`Guid?`) |
| `Areas/.../Billing/Services/BillingInvoiceService.cs` | `GetActiveEncounterOptionsAsync` memetakan tiga field baru dari `TrxPatientEncounter` ke respons |
| `Migrations/20260903015730_AddTariffIdToBilInvoiceItem.cs` (+`.Designer.cs`) | Migration baru: `AddColumn TariffId`, `CreateIndex`, `AddForeignKey` dengan `ReferentialAction.Restrict` — **source saja, belum dijalankan** |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Snapshot model diperbarui otomatis oleh `dotnet ef migrations add` mengikuti perubahan di atas |
| `QuilvianSystemBackend.Tests/BillingManagement/BillingChargeSourceAdapterTests.cs` (baru) | 3 unit test murni domain (tanpa database) untuk policy `"ADHOC_CATALOG"` |
| `QuilvianSystemBackend.Tests/BillingManagement/BillingInvoiceServiceTests.cs` | Tambah `ActiveEncounterOptionsExposeServiceUnitClinicAndPatientClassForTariffFiltering` (contract test 3 field baru) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delta aditif pada respons `GET .../encounter-options` (3 field baru). Tidak breaking — consumer lama yang belum membaca field ini tidak terpengaruh. Tidak ada endpoint baru pada task ini |
| Database | `AddColumn` `TariffId` (`Guid`, nullable) + index + FK `Restrict` ke `MstTariff` pada `BilInvoiceItem`. Migration sudah digenerate sebagai source (`20260903015730_AddTariffIdToBilInvoiceItem`), **belum dijalankan ke database manapun** — eksekusinya wewenang terpisah sesuai `TASK_RULES.md` |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan permission, authorization, atau data privasi pada task ini |

---

## 4. Dokumentasi endpoint

Task ini tidak menambah endpoint baru, tetapi memperluas respons satu endpoint yang sudah ada.

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/encounter-options` | Daftar kunjungan aktif untuk dipilih pada form Buat Invoice Manual — kini membawa `ServiceUnitId`/`ClinicId`/`PatientClassId` untuk menyaring tarif katalog per konteks kunjungan (dikonsumsi `BE-BKC-019`/`FE-BKC-014`) | `BillingInvoice : Read` (tidak berubah) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet msbuild QuilvianSystemBackend.csproj -t:Compile -nologo` | Berhasil tanpa error | `PASS` | `0 Error CS` |
| `dotnet ef migrations add AddTariffIdToBilInvoiceItem` (setelah full `dotnet build`, bukan `--no-build`) | Migration berisi `AddColumn`/`CreateIndex`/`AddForeignKey` sesuai acceptance criteria | `PASS` | Isi `20260903015730_AddTariffIdToBilInvoiceItem.cs` |
| `dotnet test --filter "FullyQualifiedName~Billing"` (setelah seluruh test task ini ditambahkan) | `Total: 195, Passed: 193, Failed: 2` | `PASS` (untuk lingkup task ini) + `EXISTING / ENVIRONMENT ISSUE` (2 kegagalan, lihat catatan) | Keluaran `dotnet test` sesi ini |
| `BillingChargeSourceAdapterTests` (3 test baru: `AdhocCatalogIsAcceptedAsBillableSourceDomain`, `AdhocCatalogOrderIsCompleteImmediatelyOnEntry`, `AdhocCatalogItemCanStillBeVoidedFromAddedStatus`) | Lulus | `PASS` | Termasuk dalam hasil `dotnet test` di atas |
| `ActiveEncounterOptionsExposeServiceUnitClinicAndPatientClassForTariffFiltering` | Lulus | `PASS` | Termasuk dalam hasil `dotnet test` di atas |

**Catatan 2 kegagalan test yang bukan bagian task ini** (diverifikasi lewat `git stash` — kedua test
ini gagal identik walau seluruh perubahan `BE-BKC-018` distash keluar, jadi bukan regresi dari task
ini):

1. `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`
   mengharapkan status `FINAL`, padahal `BillingFinalizationService.cs` (sudah berjalan sejak sesi
   sebelumnya, dengan komentar penjelas eksplisit di source-nya) sengaja menghasilkan `CLOSED` untuk
   finalisasi normal yang lunas penuh — `FINAL` sekarang hanya untuk departure exception yang masih
   punya sisa piutang. Test-nya yang belum diperbarui mengikuti perubahan itu.
2. `BillingCalculationServiceTests.RecalculateCreatesImmutableVersionsWithTaxProvenance` men-seed
   tax rule dengan jendela efektif terikat tanggal tetap (`2026-08-21 ± 1 hari`), padahal
   `BillingCalculationService.LoadInvoiceTaxRuleAsync` mencocokkan rule terhadap
   `DateTimeOffset.UtcNow` sungguhan pada saat kalkulasi berjalan — desain yang memang disengaja
   untuk RANAP (tagihan bertambah tiap hari, rule pajak yang berlaku ditentukan saat itu juga).
   Jendela test sudah lewat sejak tanggal berjalan melampaui `2026-08-22`.

Keduanya adalah **test yang basi (stale assertion)**, bukan bug produksi, dan **di luar wewenang
tulis task ini** (bukan bagian `BE-BKC-018`/`BE-BKC-019`; `CLAUDE.md` backend mewajibkan setiap
perubahan source lewat task/slice tersendiri). Direkomendasikan sebagai task terpisah — lihat § 7.

Uji manual: `NOT APPLICABLE` — murni fondasi data backend, tidak ada endpoint baru atau perubahan
UI untuk diklik-coba.

**Tidak dijalankan:** `dotnet ef database update` (eksekusi migration ke database — wewenang
terpisah dari implementasi source, sesuai DoD "DB tidak dijalankan"); verifikasi FK `Restrict`
terhadap Postgres sungguhan (EF InMemory yang dipakai test tidak menegakkan foreign key).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kolom `TariffId` nullable, FK `Restrict` tervalidasi | Terpenuhi (source) | `BilInvoiceItem.cs` (`Guid? TariffId`), `BilInvoiceItemConfiguration.cs` (`OnDelete(Restrict)`), migration `20260903015730_...`. Penegakan sungguhan terhadap Postgres belum diverifikasi (DB belum dijalankan) |
| `"ADHOC_CATALOG"` diterima `BillingChargeSourceAdapter.ValidateAndNormalize` | Terpenuhi | `BillingChargeSourceAdapterTests.AdhocCatalogIsAcceptedAsBillableSourceDomain` — lulus |
| `ActiveEncounterOptionResponse` mengembalikan 3 field baru tanpa breaking existing consumer | Terpenuhi | `ActiveEncounterOptionsExposeServiceUnitClinicAndPatientClassForTariffFiltering` — lulus; field bersifat aditif (tidak menghapus/mengubah field lama) |
| DoD: Migration source + configuration + test lulus; build lulus; DB tidak dijalankan | Terpenuhi | § 5 di atas; tidak ada perintah `dotnet ef database update` dijalankan sesi ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet ef migrations add ... --no-build` sempat menghasilkan migration **kosong** (binary lama, bukan error eksplisit) karena project belum di-build ulang setelah perubahan model; diperbaiki dengan `dotnet build` penuh lebih dulu, baru `--no-build` pada perintah migration menghasilkan diff yang benar |
| Masalah yang diketahui | Dua test billing pra-eksisting gagal dan **tidak diperbaiki pada task ini** (di luar wewenang tulis `BE-BKC-018`/`019`) — lihat rincian akar masalah dan bukti stash di § 5. Direkomendasikan jadi task/slice tersendiri: perbarui assertion `BillingFinalizationServiceTests` (`FINAL`→`CLOSED` untuk kasus lunas penuh) dan jendela efektif `TaxRule` pada `BillingCalculationServiceTests` (anchor ke `DateTimeOffset.UtcNow` saat test berjalan, bukan tanggal tetap) |
| Risiko tersisa | FK `Restrict` pada `TariffId` belum diuji terhadap Postgres sungguhan — EF InMemory (dipakai seluruh test task ini) tidak menegakkan foreign key constraint. Perlu diverifikasi saat migration benar-benar dijalankan ke database dev |
| Perubahan sampingan | `NONE` |
| Interupsi | Task ini sebelumnya sempat dilaporkan `BLOCKED — canonical governance unavailable` pada sesi terdahulu (direktori `rules/backend/` skill suite belum lengkap saat itu); pengguna meng-override eksplisit lewat pengulangan invocation task yang identik. Pada sesi ini `rules/backend/` sudah lengkap (lihat Governance Preflight pada Metadata) sehingga tidak ada blocker governance aktif untuk task ini |
| Status Git | `On branch Yasmina, up to date with 'origin/Yasmina'`. Belum staged/commit — lihat `git status --short` gabungan `BE-BKC-018`+`BE-BKC-019` pada laporan `BE-BKC-019.md` § 7 (kedua task berbagi working tree yang sama pada sesi ini) |
| Langkah berikutnya | 1) Jalankan `dotnet ef database update` di database dev milik pemilik repository (wewenang terpisah). 2) Lanjut `BE-BKC-019` — **sudah dikerjakan pada sesi yang sama**, lihat `task/report/backend/BE-BKC-019.md`. 3) Pertimbangkan task/slice terpisah untuk memperbaiki dua test pra-eksisting pada § 7 di atas |
