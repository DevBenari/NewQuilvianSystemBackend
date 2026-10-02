# Laporan Perubahan Backend — `BE-BKC-023`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-023` — Endpoint lembar "Invoice Asuransi" |
| `TASK TYPE` | Implementasi backend (satu endpoint baca baru + satu service baru + DTO baru) |
| `COMPLEXITY` | `MEDIUM` |
| `CLASSIFICATION SCORE` | 3 — kontrak API baru (skor 2) ditambah data sensitif nomor polis (skor 1) |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/` |
| Gelombang | `MVP-5` (`EPIC BKC-05`) |
| Blueprint | `BIL-CASH-001` revision `0.8` — `approved` 4 September 2026 (kontrak dikunci) |
| Kontrak berlaku | `BIL-API-0.5`, `BIL-VALIDATION-0.5` — `approved` |
| Backend SHA saat mulai | `fd4a605` (branch `Yasmina`), di atas working tree hasil `BE-BKC-022` |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE`, registry `ACTIVE` — sama dengan `BE-BKC-022`, tidak diperiksa ulang dari awal |
| Keberlakuan | `NEW CODE` — satu file DTO baru, satu file service baru, satu action baru pada controller yang sudah ada, satu baris registrasi DI. Bukan `Trx*` legacy, bukan `LEGACY MIGRATION` |
| QBE ID yang berlaku | `QBE-API-001` (endpoint/response/status), `QBE-DTO-001` (DTO terpisah dari entity), `QBE-SVC-001` (logika di Module Service baru, bukan di controller) |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-002/003`, `QBE-CODE-*`, `QBE-DB-*` — tidak ada entity, migration, atau konfigurasi EF baru |

Selisih governance yang sama seperti dilaporkan `BE-BKC-022.md` (nama berkas `rules/GLOBAL_RULES.md` vs `rules/README.md`; lokasi kontrak rekayasa di `docs/engineering/`) tetap berlaku dan tidak diulang di sini.

### Gerbang yang belum tertutup — dibaca terbuka, tidak disembunyikan

| Gerbang | Isi | Keadaan |
| --- | --- | --- |
| `BKC-GATE-03` | Penilaian Security atas pemakaian ulang `BillingInvoice : Read` untuk lembar berisi nomor polis dan nomor anggota | **Belum ditutup.** Kontrak `BIL-API-0.5`/`BIL-PERMISSION-0.5` yang sudah dikunci memang memilih hak akses ini (`02-backend-architecture.md` § Security menyatakan pemakaian ulang ini disengaja), sehingga implementasi mengikuti kontrak yang sudah disetujui. Penilaian Security tetap prasyarat sebelum produksi, bukan sebelum menulis source |
| Sequencing `BE-BKC-022` | Task ini menunggu `BE-BKC-022` selesai **dan terverifikasi** | `BE-BKC-022` source selesai, build/test belum dijalankan (menunggu pengguna). Task ini tetap dilanjutkan atas instruksi eksplisit pengguna |

Pengguna mengonfirmasi lanjut ("Lanjutkan BE-BKC-023 ... Tanpa test build project") dengan kedua gerbang di atas sudah dinyatakan terbuka pada permintaan skill. Laporan ini mencatatnya apa adanya, bukan menyembunyikannya.

---

## 1. Masalah yang diselesaikan

Sebelum task ini, tidak ada satu jalur pun bagi kasir untuk mengambil seluruh isi lembar "Invoice
Asuransi" dalam satu permintaan. Layar terpaksa memanggil endpoint master data, endpoint kalkulasi,
dan endpoint detail invoice secara terpisah lalu menjahitnya sendiri di browser — tiga permintaan
yang bisa gagal terpisah dan berpotensi menghasilkan lembar setengah jadi yang tetap tercetak.

## 2. Proses bisnis

**Tujuan.** Rumah sakit menyerahkan satu lembar yang layak dibaca perusahaan asuransi: identitas
pasien, blok perusahaan asuransi, baris biaya yang ditanggung beserta rupiahnya, dan totalnya.

**Pelaku.** Kasir dan petugas Billing membaca serta mencetak; tidak ada yang menyunting isinya dari
layar — koreksi angka lewat item invoice atau Pengecualian Finansial, bukan lewat lembar cetak.

**Pemicu.** Kasir membuka tab "Invoice Asuransi" pada halaman Dokumen Kasir (frontend, di luar
scope task ini — `FE-BKC-018`).

**Langkah utama.**

1. Sistem membaca invoice; menolak dengan `404` bila tidak ditemukan.
2. Sistem membaca sumber angka: invoice `OPEN` dihitung segar (sama dengan Menu Pembayaran);
   invoice `FINAL`/`CLOSED`/`SETTLED_BY_WRITE_OFF` dibaca dari versi kalkulasi yang sudah terkunci.
3. Sistem membaca penjamin aktif kunjungan dan menentukan `payerKind`.
4. Bila `INSURANCE`, sistem membaca blok perusahaan dari `MstInsuranceProvider` dan nomor polis dari
   salinan data pendaftaran (`TrxPatientEncounterGuarantor`), **bukan** dari data polis pasien yang
   berlaku hari ini.
5. Sistem menyaring baris yang rupiah tanggungannya lebih dari nol, lalu menjumlahkan totalnya.
6. Sistem menyusun `warnings` untuk setiap keadaan yang membuat lembar tidak dapat diterbitkan.

**Aturan bisnis.** Penyaringan baris dan penyusunan blok perusahaan dikerjakan **server**; layar
tidak menyaring maupun menghitung apa pun sendiri (`BKC-DEC-068`).

**Perubahan status.** Tidak ada. Endpoint ini murni membaca.

**Jalur tidak normal.** Kunjungan tunai, kunjungan penjamin perusahaan, kunjungan tanpa data
penjamin, tagihan tanpa baris tercover, dan tagihan lama tanpa rincian per baris — kelimanya
dijawab `200` dengan `isPrintable=false` beserta `warnings`, **bukan** galat (`BKC-DES-008`).
Kalkulasi yang gagal (misalnya dua aturan pajak aktif bersamaan) tetap `422` dengan pesan asli
mesin kalkulasi — kegagalan hitung **tidak boleh** ditelan menjadi dokumen kosong.

**Hasil akhir.** Lembar siap cetak, atau keterangan biru yang menjelaskan kenapa lembar tidak dapat
diterbitkan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/02-backend-architecture.md` § Amendment 3 September 2026 | Spesifikasi DTO, service, dan class diagram target |
| `docs/module-blueprints/billing-kasir/contracts/api-contract.md` § Amendment 3 September 2026 | Bentuk `InsuranceInvoiceDocumentResponse`, kode status, endpoint |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` | Pola action GET, penanganan exception, `SortOrder` yang sudah dipakai |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` | `PreviewCalculationAsync`, `MapResponse` (internal static), guard `invoice.Status != Open` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` | Pola `LoadPatientSummaryAsync` — query patient/encounter/room/service-unit/patient-class yang sudah ada |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs` | Bentuk `CalculationResponse`, `CalculationItemResponse`, `AdministrationFeeCalculationResponse`, `RoomChargeCalculationResponse`, `CoverageCalculationResponse` yang sebenarnya ada di source |
| `Areas/HealthServices/BillingManagement/Billing/Models/{BilInvoice,BilInvoiceItem,BilCalculationVersion}.cs` | Field entity dan `DbSet` |
| `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs`, `.../Enums/EncounterPaymentType.cs` | Sumber `payerKind` dan snapshot polis |
| `Areas/Administrator/MasterData/Models/MstInsuranceProvider.cs` | Field blok perusahaan asuransi |
| `Repositories/ApplicationDbContext.cs` | Nama `DbSet` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Dtos/BillingInsuranceInvoiceDtos.cs` | **Baru.** Lima DTO response (`InsuranceInvoiceDocumentResponse`, `InsuranceInvoicePatientResponse`, `InsuranceInvoicePayerResponse`, `InsuranceInvoiceItemResponse`, `InsuranceInvoiceTotalResponse`) dan dua kelas konstanta (`InsuranceInvoicePayerKinds`, `InsuranceInvoiceItemKinds`) |
| `Billing/Services/BillingInsuranceInvoiceDocumentService.cs` | **Baru.** `GetDocumentAsync` beserta tiga method privat (`LoadPatientAsync`, `LoadPayerAsync`, `BuildItemsAsync`) |
| `Billing/Controllers/BillingInvoicesController.cs` | Constructor bertambah satu parameter (`BillingInsuranceInvoiceDocumentService`); satu action baru `GET {id:guid}/insurance-invoice-document` |
| `Billing/BillingManagementServiceCollectionExtensions.cs` | Satu baris `services.AddScoped<BillingInsuranceInvoiceDocumentService>();` |

Total: **4 berkas** (2 baru, 2 diperbarui). Tidak ada berkas lain tersentuh; tidak ada migration.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Additive.** Satu endpoint `GET` baru. Tidak ada endpoint lama yang berubah bentuk |
| `DATABASE IMPACT` | **Nihil.** Seluruhnya baca (`AsNoTracking`); tidak ada kolom, index, entity, maupun migration |
| `SECURITY IMPACT` | Endpoint baru dengan `[AccessPermission("BillingInvoice", "Read")]` — pemakaian ulang hak akses yang sudah disetujui kontrak, **belum** dinilai Security (`BKC-GATE-03`, dicatat terbuka di atas). Tidak ada custom logger pada `GET` ini (mengikuti konvensi project), sehingga tidak ada jejak "siapa membuka dokumen ini" — keterbatasan yang diketahui, bukan kelalaian |
| `VISUAL REFERENCE` | `NOT REQUIRED` — tidak ada perubahan tampilan; konsumen frontend (`FE-BKC-018`) di luar scope task ini |

## 4. Dokumentasi endpoint

### `[Tags("Health Services / Billing Management / Billing / Invoices")]`

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id:guid}/insurance-invoice-document` | Menyusun seluruh isi lembar Invoice Asuransi satu tagihan | `BillingInvoice : Read` | Path `id` | `ApiResponse<InsuranceInvoiceDocumentResponse>` |

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Permintaan berhasil dibaca. **Termasuk** ketika lembar tidak dapat diterbitkan — periksa `isPrintable` dan `warnings` |
| `403` | Pengguna tidak punya hak akses untuk membaca invoice ini (ditangani middleware authorization, bukan kode task ini) |
| `404` | Invoice tidak ditemukan, atau (kondisi seharusnya-tidak-pernah-terjadi) versi kalkulasi terkunci milik invoice non-`OPEN` tidak ditemukan |
| `422` | Kalkulasi pratinjau gagal (misalnya dua aturan pajak aktif bersamaan) — pesan asli dari mesin kalkulasi diteruskan apa adanya |

**Catatan penyimpangan dari rencana desain — `SortOrder`.** `02-backend-architecture.md` merancang
`SortOrder = 9` dengan asumsi `8` adalah nomor tertinggi yang dipakai (`calculation-preview`).
Pemeriksaan langsung controller menemukan **enam** action lain sudah memakai `9` sampai `14`
(ditambahkan task-task ad-hoc sesudah desain itu ditulis: `encounter-options`, `other-charge-types`,
`other-charges`, `charge-summary`, `catalog-charges`, `coverage-preview`). Memakai `9` akan
bertabrakan dengan `GetActiveEncounterOptions`. Endpoint ini memakai **`15`** — nomor tertinggi
saat ini (`14`) ditambah satu — bukan penyimpangan sepihak, melainkan mengikuti source yang lebih
baru daripada dokumen desain.

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna pada task ini: "Tanpa test build project" — konsisten dengan instruksi yang berlaku sepanjang sesi |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Sama seperti di atas |
| Verifikasi statis field DTO vs sumbernya | **LULUS** | Manual | Setiap field `InsuranceInvoiceItemResponse`/`InsuranceInvoicePatientResponse`/`InsuranceInvoicePayerResponse` ditelusuri satu-per-satu ke properti nyata pada `CalculationItemResponse`, `AdministrationFeeCalculationResponse`, `RoomChargeCalculationResponse`, `BilInvoiceItem`, `MstTariffCategory`, `TrxPatientEncounter`, `MstPatient`, `TrxPatientEncounterGuarantor`, `MstInsuranceProvider` — **satu kesalahan ditemukan dan diperbaiki sendiri** selama penulisan: draf pertama sempat membaca `Description`/`CategoryName`/`Quantity`/`UnitPrice` dari `CalculationItemResponse` (yang sebenarnya tidak memilikinya), diperbaiki menjadi membaca dari `BilInvoiceItem`+`Category` |
| Verifikasi statis `DbSet`/namespace | **LULUS** | Manual | `BilInvoices`, `BilInvoiceItems`, `BilCalculationVersions`, `TrxPatientEncounters`, `TrxPatientEncounterGuarantors`, `MstPatients`, `MstInsuranceProviders`, `MstRooms`, `MstServiceUnits`, `MstPatientClasses` — seluruhnya dicocokkan ke `ApplicationDbContext.cs` |
| Verifikasi statis akses `internal static MapResponse` | **LULUS** | Manual | `BillingCalculationService.MapResponse` bersifat `internal`; `BillingInsuranceInvoiceDocumentService` berada di assembly dan namespace `Services` yang sama, sehingga dapat diakses tanpa mengubah visibilitasnya |
| Verifikasi statis `SortOrder` unik | **LULUS** | Manual | `grep SortOrder` pada controller mengonfirmasi `15` belum dipakai action lain |
| Cakupan diff | **LULUS** | Manual | `git status --short` menunjukkan tepat 2 berkas baru + 2 berkas diperbarui pada task ini; nol migration, nol entity |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** Build dan test wajib
> dijalankan pengguna. Laporan ini mencatat keadaan sebenarnya, bukan mengklaim keberhasilan.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| `BIL-AT-031` — lembar memuat tiga baris tercover; item tak-tercover tidak ada | **Tercakup oleh implementasi**; angka penuh menunggu test/verifikasi manual |
| `BIL-AT-032` — blok perusahaan dari `MstInsuranceProvider`; polis dari salinan pendaftaran | **Tercakup** — `LoadPayerAsync` membaca `MstInsuranceProviders` untuk blok perusahaan dan `guarantor.*Snapshot` untuk polis, bukan `MstPatientInsurance` terkini |
| `BIL-AT-033` — tiga permintaan (tunai/perusahaan/tanpa penjamin) ketiganya `200`, bukan `422`/`404` | **Tercakup** — `switch` pada `payerKind` selalu mengisi `warnings` dan tidak pernah melempar exception untuk ketiga keadaan itu |
| `BIL-AT-034` — tagihan `FINAL` bersalinan lama: `200`, rincian tidak tersedia, total tetap sah | **Tercakup** — cabang `IsPerItemBreakdownAvailable=false` mengosongkan `Items` tetapi `Totals` tetap dibaca dari field top-level `CalculationResponse` (kolom relasional) |
| `BIL-AT-035` — payload tidak memuat `ruleCode`/`cardNumber`/kontak PIC | **Terpenuhi by design** — DTO `InsuranceInvoicePayerResponse` secara sengaja tidak memiliki field-field itu sama sekali (lihat `BillingInsuranceInvoiceDtos.cs`), bukan sekadar dikosongkan saat runtime |
| Endpoint murni baca, tanpa transaksi | **Terpenuhi** — seluruh query `AsNoTracking`; tidak ada `SaveChangesAsync` |
| Tidak ada migration | **Terpenuhi** |
| Build dan test lulus | **BELUM** — menunggu pengguna |

**Definition of Done belum tercapai.** Yang tersisa: build/test pengguna, verifikasi manual
ter-autentikasi (di luar wewenang sesi ini — memerlukan lingkungan login), dan penilaian Security
atas `BKC-GATE-03` sebelum produksi.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Endpoint mereuse `BillingInvoice : Read` untuk dokumen berisi nomor polis dan nomor anggota. Ini **pilihan desain yang sudah dikunci** di kontrak (`02-backend-architecture.md` § Security menyatakan eksplisit "disengaja"), bukan keputusan task ini — tetapi `BKC-GATE-03` (penilaian Security) tetap terbuka dan **wajib** ditutup sebelum rilis produksi |
| `KNOWN ISSUES` | `SortOrder` desain (`9`) sudah dipakai action lain; task ini memakai `15` — lihat § 4. `CategoryCode`/`Quantity`/`UnitPrice` untuk baris `ADMINISTRATION_FEE`/`ROOM_CHARGE` diisi dari `AdministrationFeeCalculationResponse.PolicyCode`/`AppliedAmount` (bukan dari master), karena kedua komponen itu bukan `BilInvoiceItem` dan tidak punya kategori tarif sendiri — konsisten dengan `02-backend-architecture.md` yang menyatakan keduanya "tidak punya baris di Items" pada model lama dan harus menempel di response komponennya sendiri |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — memerlukan lingkungan ter-autentikasi dan data uji dengan penjamin asuransi nyata |
| `INCIDENTAL CHANGES` | `NONE` |
| `INTERRUPTIONS` | Sesi dimulai ulang (model diganti Opus → Sonnet) di tengah task; dilanjutkan dari instruksi pengguna "Lanjutkan BE-BKC-023" tanpa penyuntingan ganda — pekerjaan `BE-BKC-022` sebelumnya (working tree) dibiarkan apa adanya |
| `GIT STATUS` | 2 berkas baru (belum ter-track), 2 berkas diperbarui, di atas working tree `BE-BKC-022` (3 berkas) dan 13 berkas dokumentasi blueprint dari sesi sebelumnya. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet build` dan `dotnet test` secara manual (mencakup verifikasi `BE-BKC-022` dan `BE-BKC-023` sekaligus, karena keduanya berbagi working tree yang sama). Bila lulus, lanjutkan `BE-BKC-024` (`READY_FOR_TASK_APPROVAL`, tanpa gerbang) sesuai urutan yang diminta pengguna: seluruh task backend dahulu, baru frontend |
