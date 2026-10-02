# Laporan Perubahan Backend — `BE-BKC-026`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-026` — Verifikasi gerbang PPN rawat inap versus rawat jalan |
| `TASK TYPE` | Verifikasi (bukan pekerjaan kode) — menulis test acceptance untuk gerbang yang sudah ter-commit |
| `COMPLEXITY` | `LIGHT` — tidak ada logika baru; menulis test atas perilaku existing dan memperbaiki satu fixture basi |
| `CLASSIFICATION SCORE` | 1 — logika uang tersentuh secara tidak langsung (test atas nilai finansial), tidak ada kontrak API/DTO/entity baru |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingCalculationServiceTests.cs` saja. **Tidak ada source aplikasi yang diubah** |
| Gelombang | `MVP-8` (`EPIC BKC-08`) |
| Blueprint | `01-existing-capability-map.md`, `backend-roadmap.md` § `BE-BKC-026` — `BKC-GATE-04` tertutup 4 September 2026 |
| Kontrak berlaku | `BIL-VALIDATION-0.6` (`BIL-VAL-038`, `BIL-VAL-039`), `BIL-CALCULATION-0.6` |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE` |
| Status registry | **`ACTIVE`** |
| Keberlakuan | `NEW CODE` — seluruh perubahan berada di berkas test modul yang sedang berjalan; tidak ada `Trx*`, tidak ada `LEGACY MIGRATION` |
| QBE ID yang berlaku | `QBE-VAL-001` (memverifikasi invarian bisnis yang sudah ada, tidak menciptakan baru) |
| QBE ID yang **tidak** berlaku | `QBE-API-001`/`QBE-DTO-001`/`QBE-SVC-001` (tidak ada perubahan controller/service/DTO), `QBE-ENT-*`, `QBE-MOD-*`, `QBE-DB-*` (tidak ada entity/module/migration baru) |

### Selisih governance yang ditemukan dan dilaporkan

Sama seperti dicatat pada laporan task sebelumnya di modul ini (`BE-BKC-022`, `BE-BKC-024`):
`rules/GLOBAL_RULES.md` tidak ada (yang tersedia `rules/README.md`); kontrak rekayasa berada di
`docs/engineering/` repository ini. Tidak ada selisih baru.

---

## 1. Sifat task ini

`BE-BKC-026` **diturunkan dari task koding menjadi task verifikasi** pada 4 September 2026 —
tercatat eksplisit di `backend-roadmap.md`. Pemindaian dampak menemukan seluruh gerbang PPN
rawat inap/rawat jalan **sudah terimplementasi** lewat task ad-hoc `BE-BKC-FIX-004` di luar
roadmap:

- `BillingCalculationService.cs:174` — `isOutpatientForTax = invoice.ServiceType !=
  AdministrationFeeServiceTypes.Ranap`.
- `ApplyInvoiceTax` (baris 760–815) — mengembalikan basis pajak kosong bila `!isOutpatient`, dan
  membatasi basisnya hanya ke item `Category.IsPharmacy == true` ketika outpatient.

**Tidak ada satu baris source aplikasi yang diubah task ini.** Yang dikerjakan murni menulis test
yang membuktikan gerbang itu benar, sesuai instruksi task: "Bukan pekerjaan kode."

## 2. Temuan yang ditemukan saat menulis test verifikasi

Saat menyiapkan fixture untuk membuktikan basis pajak (acceptance #6 — hanya obat/alkes yang kena
PPN), ditemukan **satu test existing yang sudah basi**:
`RecalculateCreatesImmutableVersionsWithTaxProvenance`. Test ini dibuat sebelum gerbang
`IsPharmacy` ada, dan komentarnya sendiri masih menyatakan `"*"` sebagai *"tax rule tingkat
invoice"* — klaim yang sudah tidak benar sejak `BE-BKC-FIX-004` (`TaxableCategory` kini murni
label, tidak pernah dipakai untuk gerbang apa pun; lihat komentar
`BillingCalculationService.cs:717-720`). Item yang dibuat `SeedInvoiceAsync` sebelum perbaikan ini
**tidak pernah mengisi `Category.IsPharmacy`** (default `false`), sehingga item itu semestinya
gagal masuk basis pajak dan `TaxAmount` semestinya nol — bukan `11_000m` seperti yang di-assert
test ini.

**Ini persis risiko yang diperingatkan `BKC-OQ-085` dan alasan task ini wajib dijalankan sebelum
rawat inap hidup**: satu test yang mengklaim membuktikan "tax provenance" ternyata memakai fixture
yang sudah tidak sesuai dengan gerbang yang sudah aktif di source, dan baru terlihat saat
verifikasi eksplisit dilakukan.

**Perbaikan yang diambil:** menambahkan parameter opsional `isPharmacy` pada helper
`SeedInvoiceAsync` (default `false`, seluruh 20+ pemanggil lama tidak terdampak), lalu mengisi
`isPharmacy: true` pada test yang basi tadi serta memperbaiki komentarnya. **Nilai yang di-assert
tidak diubah** — yang berubah hanya fixture-nya, sama seperti pola perbaikan dua test basi pada
`BE-BKC-022`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` | Lokasi gerbang `isOutpatientForTax` dan `ApplyInvoiceTax` yang diverifikasi |
| `Areas/HealthServices/BillingManagement/MasterData/DTOs/AdministrationFeePolicyDtos.cs` | Konfirmasi konstanta `AdministrationFeeServiceTypes` (`Rajal`, `Igd`, `Ranap`) — tidak ada konstanta `Mcu` |
| `Areas/HealthServices/MasterData/Models/MstTariffCategory.cs` | Konfirmasi field `IsPharmacy` |
| `Areas/HealthServices/BillingManagement/MasterData/Services/TaxRuleService.cs` | Konfirmasi `TaxableCategory` tidak lagi dipakai untuk gerbang perhitungan |
| `Tests/.../BillingCalculationServiceTests.cs` | Pola test dan fixture `SeedInvoiceAsync` existing |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Kontrak dan acceptance criteria task ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Tests/.../BillingCalculationServiceTests.cs` | (1) `SeedInvoiceAsync` diberi parameter opsional `isPharmacy = false`, diteruskan ke `category.IsPharmacy`; (2) fixture basi pada `RecalculateCreatesImmutableVersionsWithTaxProvenance` diperbaiki (`isPharmacy: true`) beserta komentarnya; (3) enam test acceptance baru ditambahkan untuk `BE-BKC-026` |

Total: **1 berkas test berubah.** Tidak ada source aplikasi, DTO, controller, entity, atau
migration tersentuh — sesuai sifat task ini sebagai verifikasi murni.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Nihil.** Tidak ada perubahan kontrak, karena tidak ada source yang diubah |
| `DATABASE IMPACT` | **Nihil** |
| `SECURITY IMPACT` | **Nihil** |
| `VISUAL REFERENCE` | `NOT REQUIRED` |

## 4. Test yang ditambahkan (memetakan langsung ke acceptance criteria)

| Test | Acceptance | Membuktikan |
| --- | --- | --- |
| `InpatientPharmacyItemsAreExemptFromTax` | `BIL-AT-044` | Tagihan `RANAP` dengan item obat (`IsPharmacy=true`): `TaxAmount=0`, `Breakdown.Taxes` kosong, `PatientAmount=100.000` — tidak ada PPN sama sekali |
| `OutpatientPharmacyItemsAreTaxed` | `BIL-AT-045` | Tagihan `RAJAL` dengan fixture identik (hanya `ServiceType` berbeda): `TaxAmount=11.000`, `PatientAmount=111.000` — PPN tetap dikenakan |
| `EmergencyPharmacyItemsAreTaxedLikeOutpatient` | `BIL-AT-046` | Tagihan `IGD` diperlakukan sama seperti `RAJAL` — **tidak** ikut dibebaskan seperti `RANAP` |
| `McuPharmacyItemsAreTaxedAsDefaultBehavior` | `BIL-AT-051` | Tagihan `ServiceType="MCU"` (belum ada konstantanya, belum dipakai) tetap kena PPN sebagai perilaku bawaan — dilampirkan sebagai bahan keputusan sebelum `MCU` diaktifkan, sesuai jawaban pemilik atas `BKC-CQ-03` |
| `UnknownOrEmptyServiceTypeIsStillTaxedAndDoesNotHaltCalculation` | `BIL-VAL-039` | `ServiceType=""` tetap kena PPN dan **tidak** melempar exception — gerbangnya `!= "RANAP"` otomatis benar untuk string apa pun selain persis `"RANAP"` |
| `OutpatientNonPharmacyItemsAreNeverPartOfTaxBase` | Acceptance #6 (basis pajak) | Tagihan `RAJAL` dengan item **bukan** obat/alkes (`isProcedure: true`, `IsPharmacy=false`): `TaxAmount=0` meski rawat jalan — jasa tindakan/konsultasi tidak pernah masuk basis PPN |

Acceptance #7 (jenis kunjungan diambil dari tagihan, bukan dari pendaftaran terkini) sudah
terbukti oleh struktur kode itu sendiri — `isOutpatientForTax` dibaca dari `invoice.ServiceType`,
bukan dari `encounter` — dan tercermin di seluruh test di atas yang memakai `SeedInvoiceAsync`
dengan `ServiceType` invoice sebagai parameter tunggal penentu gerbang.

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna pada task ini: tanpa build/test, pengguna menjalankan sendiri secara manual |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas — **inilah acceptance test yang sesungguhnya untuk task verifikasi ini**, jadi task ini secara harfiah belum bisa dinyatakan selesai sampai dijalankan |
| Verifikasi statis gerbang | **LULUS** | Manual | Pembacaan ulang `BillingCalculationService.cs:174` dan `ApplyInvoiceTax` mengonfirmasi gerbang persis seperti dijelaskan roadmap: `ServiceType != "RANAP"` menentukan taxable, `Category.IsPharmacy` membatasi basis |
| Verifikasi statis fixture | **LULUS** | Manual | `SeedInvoiceAsync` dengan parameter baru `isPharmacy` tidak mengubah signature 20+ pemanggil lama (parameter opsional dengan default `false` identik dengan perilaku sebelumnya) |
| Cakupan diff | **LULUS** | Manual | `git diff --stat` menunjukkan tepat 1 berkas (`BillingCalculationServiceTests.cs`, +363/-22 baris). Tidak ada source aplikasi tersentuh |

> **`VALIDATION` belum lengkap.** Task verifikasi ini secara definisi belum selesai sampai
> `dotnet test` benar-benar dijalankan dan keenam test baru (plus fixture yang diperbaiki) lulus.
> Instruksi eksplisit pengguna pada task ini: build/test dijalankan manual oleh pengguna, bukan
> agent.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| `BIL-AT-044` — `RANAP` tidak ada PPN | **Test ditulis**, `InpatientPharmacyItemsAreExemptFromTax`. Lulus/gagal menunggu `dotnet test` |
| `BIL-AT-045` — `RAJAL` dengan obat sama kena PPN | **Test ditulis**, `OutpatientPharmacyItemsAreTaxed` |
| `BIL-AT-046` — `IGD` diperlakukan sama dengan rawat jalan | **Test ditulis**, `EmergencyPharmacyItemsAreTaxedLikeOutpatient` |
| `BIL-AT-051` — `MCU` tetap kena PPN (belum dipakai) | **Test ditulis**, `McuPharmacyItemsAreTaxedAsDefaultBehavior`. Hasilnya dilampirkan sebagai bahan keputusan `BKC-CQ-03`, bukan aktivasi `MCU` |
| Jenis kunjungan kosong/tidak dikenal tetap kena PPN, tidak menghentikan perhitungan | **Test ditulis**, `UnknownOrEmptyServiceTypeIsStillTaxedAndDoesNotHaltCalculation` |
| Basis pajak hanya obat/alkes | **Test ditulis**, `OutpatientNonPharmacyItemsAreNeverPartOfTaxBase` — sekaligus memperbaiki test lama yang secara diam-diam mengasumsikan sebaliknya |
| Jenis kunjungan diambil dari tagihan, bukan pendaftaran | **Terbukti dari struktur kode** — tidak butuh test terpisah, dijelaskan di §4 |
| Dijalankan **sebelum** rawat inap dinyatakan hidup | **Terpenuhi secara waktu** — belum ada tagihan rawat inap sungguhan (dikonfirmasi pemilik, `BKC-OQ-085`) |
| Hasilnya lulus dan terdokumentasi | **BELUM** — menunggu `dotnet test` dari pengguna; laporan ini akan diperbarui dengan hasil sebenarnya begitu tersedia |

**Definition of Done belum tercapai.** Test sudah lengkap menutupi seluruh acceptance criteria
task ini; yang tersisa murni menjalankan `dotnet test` dan mendokumentasikan hasilnya.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Ditemukan satu test existing (`RecalculateCreatesImmutableVersionsWithTaxProvenance`) yang sebelum task ini memakai fixture tidak konsisten dengan gerbang PPN yang sudah aktif di source — kemungkinan besar sudah gagal sejak `BE-BKC-FIX-004` di-commit, tanpa terdeteksi karena `dotnet test` belum dijalankan pada sesi-sesi sebelumnya juga. Sudah diperbaiki pada task ini (§2) |
| `KNOWN ISSUES` | `MCU` belum diaktifkan sebagai `ServiceType` operasional (tidak ada konstanta pada `AdministrationFeeServiceTypes`) — test `McuPharmacyItemsAreTaxedAsDefaultBehavior` memakai literal string sesuai catatan roadmap, bukan konstanta resmi |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi |
| `INCIDENTAL CHANGES` | Perbaikan fixture basi pada `RecalculateCreatesImmutableVersionsWithTaxProvenance` — dilakukan karena ditemukan langsung saat menulis test verifikasi task ini, nilai assert tidak berubah, hanya setup kategorinya |
| `INTERRUPTIONS` | `NONE` |
| `GIT STATUS` | 1 berkas test task ini berubah, belum di-stage. Working tree juga memuat pekerjaan `BE-BKC-022`/`BE-BKC-023`/`BE-BKC-024` (berkas berbeda) yang belum di-build/test pengguna — tidak disentuh atau digabung oleh task ini. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet build` dan `dotnet test` secara manual mencakup seluruh perubahan yang menumpuk (`BE-BKC-022`, `023`, `024`, `026`). Perhatikan khusus keenam test baru task ini dan fixture yang diperbaiki — bila `RecalculateCreatesImmutableVersionsWithTaxProvenance` tetap gagal meski sudah diperbaiki, kemungkinan ada gerbang PPN lain yang belum ditemukan pemindaian ini. Setelah lulus, `BKC-GATE-04` resmi tertutup dengan bukti, dan rawat inap boleh menerima tagihan pertama |
