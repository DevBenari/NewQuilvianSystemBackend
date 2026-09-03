# Billing dan Kasir — PRD → MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian — Billing dan Kasir |
| Modul | `billing-kasir` (blueprint `BIL-CASH-001`) |
| Kode modul (untuk penomoran ID dokumen ini) | `BKC` |
| Status | `approved` (Product/Domain Owner, 2 September 2026 13:53 WIB — "approval eksplisit sekarang untuk BKC-DEC-059–062") |
| Repository target | `NewQuilvianSystemBackend` (backend), `QuilvianSystemFrontendDev` (frontend) |
| Backend commit SHA baseline | `17b9c0e21e32b41a8dfd6dbde31462d52717646b` (branch `Yasmina`) |
| Frontend commit SHA baseline | `60febdcdbb39de6cebc2d825906bce949f3b5af3` (branch `yasmina`) |
| Cakupan dokumen ini | **Bukan** PRD seluruh modul Billing dan Kasir (modul itu sudah berjalan dan sebagian besar sudah diimplementasikan — lihat `MODULE-STATUS.md`). Dokumen ini adalah PRD→MVP untuk **satu slice baru**: entri manual invoice berbasis katalog tarif + coverage per item pada form "Buat Invoice Manual (Testing)", hasil `BKC-DEC-059`–`062`. Ini dokumen PRD→MVP **pertama** untuk modul ini — penomoran `EPIC BKC-01` dst. dimulai dari sini, bukan melanjutkan nomor lain yang belum ada |

## 2. Ringkasan eksekutif

Form "Buat Invoice Manual (Testing)" hari ini menerima nama item dan harga sebagai teks/angka bebas dari kasir/penguji — tidak ada jaminan item itu benar-benar ada di tarif rumah sakit, dan harga bisa diketik sembarang. Untuk pasien asuransi, tidak ada informasi apa pun soal item mana yang tercover sebelum item ditambahkan ke tagihan.

Hasil yang dikejar: item yang dimasukkan lewat form ini SELALU berasal dari tarif resmi (`MstTariff`) dengan harga yang tidak bisa diubah manual, dan untuk pasien asuransi, kasir/penguji melihat status coverage per item sebelum memilihnya — meniru bagaimana halaman input tindakan/obat resmi (belum dibangun) akan bekerja kelak. Menu Pembayaran juga menampilkan Subtotal Mandiri dan Subtotal Asuransi sebagai dua baris terpisah, bukan satu total gabungan.

## 3. Masalah produk

Kondisi sekarang (dibuktikan kode, SHA di atas):

- `create-manual-invoice-view.jsx` field `description` adalah `type: "text"` bebas; field `unitPrice` adalah `type: "number"` bebas — tidak ada validasi terhadap tarif resmi manapun.
- `AddCatalogChargeRequest` belum ada; endpoint yang dipakai (`POST from-source` via thunk `addAdhocBillingCharge`) menerima `unitPrice` apa adanya dari client (`UpsertChargeRequest.UnitPrice`, `BillingInvoiceDtos.cs:95`) tanpa pengecekan terhadap `MstTariff`.
- Tidak ada satu pun endpoint yang mengembalikan status coverage sebuah tarif untuk seorang pasien sebelum item ditambahkan. `InsuranceCoverageService.ResolveTariffAsync` (Clinical Management) sudah punya logikanya tapi belum pernah dibungkus endpoint HTTP mandiri.
- Menu Pembayaran (`menu-pembayaran-view.jsx`) menampilkan satu "Subtotal Tagihan" gabungan, dikurangi baris "Ditanggung Penjamin" — bukan dua subtotal sejajar.
- `RegistrationBillingCoverageAdapter.ResolveAsync` (mesin kalkulasi resmi) memindahkan komponen ke `unresolved` setiap kali `IsNeedApproval`/`IsNeedGuaranteeLetter` bernilai true, meskipun rule-nya sendiri berstatus `Covered`.

## 4. Visi produk

1. Kasir/penguji memilih kunjungan pasien pada form testing.
2. Sistem menampilkan kategori tarif (`MstTariffCategory`) sesuai yang sudah tersedia.
3. Kasir memilih kategori, lalu mencari nama layanan lewat dropdown yang datanya adalah baris `MstTariff` aktif, difilter otomatis sesuai unit layanan/klinik/kelas pasien kunjungan tersebut.
4. Untuk pasien asuransi, setiap opsi tarif menampilkan status coverage (Tercover/Tercover Sebagian/Tidak Tercover) hasil pengecekan langsung terhadap kontrak dan rule asuransi pasien.
5. Kasir memilih satu tarif; harga terisi otomatis dari `MstTariff.NormalPrice`, tidak bisa diketik ulang.
6. Item tersimpan ke invoice — tercover maupun tidak, tetap tercatat sebagai tagihan.
7. Di Menu Pembayaran, kasir melihat Subtotal Mandiri dan Subtotal Asuransi sebagai dua angka terpisah, mencerminkan item mana yang jadi tanggungan siapa.

## 5. Batas MVP

**Titik mulai:**

1. Encounter sudah dipilih pada form "Buat Invoice Manual (Testing)" (mekanisme pemilihan encounter tidak berubah).
2. Kategori tarif sudah dipilih (mekanisme tidak berubah — sudah `MstTariffCategory` sejak sebelum amendment ini).

**Titik akhir:**

1. Item tersimpan sebagai `BilInvoiceItem` dengan `TariffId` terisi, harga = `MstTariff.NormalPrice`, `SourceDomain="ADHOC_CATALOG"`.
2. Menu Pembayaran invoice manapun (bukan hanya hasil form ini) menampilkan Subtotal Mandiri dan Subtotal Asuransi terpisah.

**Di luar titik akhir ini** (bukan "ditunda" — memang tidak diminta, lihat § 8 untuk yang benar-benar ditunda): perubahan pada panel "Tambah Biaya Lain-lain" (`BKC-DEC-047` tetap berlaku), pembuatan halaman input tindakan/obat resmi (baru dikutip sebagai visi jangka panjang oleh Product/Domain Owner, belum diminta dibangun), dan penyatuan dua mesin coverage.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP ini |
| --- | --- |
| Kasir/Tim QA-Dev | Memakai form testing untuk membuat data invoice realistis; membaca badge coverage dan disclaimer-nya |
| Product/Domain Owner | Menyetujui `BKC-DEC-059`–`062` sebelum implementasi dimulai |
| Payer/Insurance + Finance/AR Owner | Mengonfirmasi amendemen sebagian atas `BKC-DEC-042` (pelepasan gating approval pada `RegistrationBillingCoverageAdapter`) |
| Finance/Tax Owner | Memverifikasi `MstTaxRule.AllocationRule` aktif — di luar blocking MVP ini atas permintaan eksplisit Product/Domain Owner |
| Backend/Frontend Billing | Implementasi sesuai `02-backend-architecture.md`/`03-frontend-architecture.md` amendment 2 September 2026 |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Dropdown item dari `MstTariff`, difilter kategori+konteks encounter, dengan search | `CAP-02` | Wajib; tanpa ini form tetap free-text |
| Harga otomatis dari `MstTariff.NormalPrice`, tidak dapat diinput manual | `CAP-03` | Wajib; ini invariant inti `BKC-DEC-059` |
| Endpoint preview coverage per tarif, reuse `InsuranceCoverageService.ResolveTariffAsync` | `CAP-04` | Wajib; tanpa ini Skenario B (pasien asuransi) tidak berjalan |
| Perilaku `IsNeedApproval`/`IsNeedGuaranteeLetter` tidak menggagalkan coverage pada rule `Covered` | `CAP-05` | Wajib untuk memenuhi `BKC-DEC-062`, dipersempit hanya untuk dua flag ini (lihat § 8 untuk yang ditunda) |
| Field `ServiceUnitId`/`ClinicId`/`PatientClassId` pada `ActiveEncounterOptionResponse` | `CAP-06` | Wajib; prasyarat teknis `CAP-02` |
| Subtotal Mandiri/Asuransi terpisah di Menu Pembayaran | `CAP-09` | Wajib; ini inti `BKC-DEC-062` bagian tampilan — data sudah ada, murni komposisi ulang |

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Menyatukan `RegistrationBillingCoverageAdapter` dan `InsuranceCoverageService` jadi satu mesin coverage | § 16.2.A (`01-existing-capability-map.md`) | Perubahan cross-module besar, menyentuh SEMUA invoice, belum ada keputusan eksplisit pemilik kedua modul | Kedua mesin tetap berjalan terpisah; badge preview diberi disclaimer "perkiraan" agar penguji tidak menganggapnya angka final |
| Pelepasan gating `MaxAmountPerMonth`/`MaxQuantityPerMonth` pada `RegistrationBillingCoverageAdapter` | `BKC-DEC-062` (interpretasi belum dikonfirmasi) | User baru mengonfirmasi soal flag approval, belum soal limit bulanan | Limit bulanan tetap menggeser komponen ke `unresolved`, sama seperti perilaku hari ini — tidak ada regresi, hanya tidak diperluas |
| Verifikasi/penyesuaian `MstTaxRule.AllocationRule` aktif | `CAP-07` | Product/Domain Owner eksplisit menyatakan ini menunggu keputusan bisnis lebih lanjut (2 September 2026) | Menu Pembayaran menampilkan field pajak apa adanya dari kalkulasi existing tanpa perubahan logika; kebenaran alokasi pajak ke Subtotal Mandiri bergantung konfigurasi yang sudah ada, diverifikasi terpisah oleh Finance/Tax Owner |

## 9. Alur bisnis target

**`FLOW-BKC-MVP-001` — Kasir menambah item katalog dengan pengecekan coverage**

1. Kasir membuka "Buat Invoice Manual (Testing)" dan memilih kunjungan pasien.
2. Sistem menampilkan konteks kunjungan (termasuk cara bayar — tunai/asuransi).
3. Kasir memilih Kategori Biaya (`MstTariffCategory`).
4. Sistem memuat opsi tarif dari `MstTariff`, difilter kategori dan konteks kunjungan.
5. Jika pasien asuransi, sistem menampilkan badge coverage per opsi (memanggil `GET catalog-charges/coverage-preview`).
6. Kasir memilih satu tarif. Harga terisi otomatis, tidak dapat diubah.
7. Kasir mengisi Qty dan submit.
8. Sistem memvalidasi tarif masih aktif/efektif (`BIL-VAL-025`), lalu menyimpan `BilInvoiceItem` dengan `TariffId`, harga dari `MstTariff.NormalPrice`, `SourceDomain="ADHOC_CATALOG"`.
9. Kasir dapat membuka Menu Pembayaran, melihat item baru masuk grup kategorinya, dan melihat Subtotal Mandiri/Subtotal Asuransi ter-update.

## 10. Epic dan functional requirement

### `EPIC BKC-01` — Entri item katalog tarif (berlaku pasien tunai maupun asuransi)

Tujuan: item pada form testing selalu berasal dari `MstTariff` dengan harga yang tidak bisa dimanipulasi manual.

> **`FR-BKC-001` — Dropdown item terikat katalog dan konteks encounter**
>
> Sistem menampilkan daftar item hanya dari `MstTariff` aktif yang cocok kategori terpilih DAN (scoping NULL ATAU sama dengan `ServiceUnitId`/`ClinicId`/`PatientClassId` encounter terpilih).
>
> **Contoh:** Kategori "Konsultasi" punya 3 baris `MstTariff` bernama "Konsultasi Dokter Umum" — satu scoping NULL (semua unit), satu khusus `ServiceUnitId=Poli-A`, satu khusus `ServiceUnitId=Poli-B`. Encounter terpilih berasal dari Poli-A. Dropdown menampilkan 2 opsi: yang NULL dan yang Poli-A — bukan ketiganya, bukan cuma satu yang salah.
>
> Disposisi: `MISSING / NEW` (komposisi dropdown FE baru; data layer `CAP-02` `Ready to reuse`).

> **`FR-BKC-002` — Harga tidak dapat diinput manual**
>
> Sistem mengisi harga dari `MstTariff.NormalPrice` milik tarif terpilih. Request penyimpanan (`AddCatalogChargeRequest`) tidak memiliki field harga sama sekali.
>
> **Contoh:** Kasir memilih tarif "Konsultasi Dokter Umum" seharga Rp100.000. Field Harga menampilkan "Rp100.000" sebagai teks, bukan kotak input. Tidak ada cara mengubahnya dari UI maupun dengan memodifikasi request (field-nya tidak ada di kontrak).
>
> Disposisi: `MISSING / NEW`.

> **`FR-BKC-003` — Tolak tarif tidak aktif/kedaluwarsa**
>
> Sistem menolak penyimpanan bila `TariffId` tidak ditemukan, `IsActive=false`, atau di luar `EffectiveStartDate`/`EffectiveEndDate`.
>
> **Contoh:** Tarif "Konsultasi Spesialis Lama" sudah dinonaktifkan bulan lalu tapi masih tersimpan di database. Bila ID-nya tetap dikirim (mis. request lama diputar ulang), sistem menolak dengan `422` dan pesan `BIL-VAL-025`, bukan menyimpan tagihan dengan tarif basi.
>
> Disposisi: `MISSING / NEW`.

> **`FR-BKC-004` — Keterlacakan sumber katalog vs free-form**
>
> `BilInvoiceItem` yang berasal dari entri katalog tersimpan dengan `SourceDomain="ADHOC_CATALOG"` dan `TariffId` terisi; item free-form (`"Tambah Biaya Lain-lain"`) tetap `SourceDomain="ADHOC"` dengan `TariffId=null`.
>
> **Contoh:** Laporan audit memfilter `SourceDomain="ADHOC_CATALOG"` untuk melihat berapa banyak entri manual yang harganya terverifikasi tarif resmi, terpisah dari entri bebas kasir.
>
> Disposisi: `MISSING / NEW`.

### `EPIC BKC-02` — Coverage per item untuk pasien asuransi

Tujuan: kasir/penguji melihat status coverage sebelum memilih item, dan item yang butuh approval tidak diperlakukan seolah gagal.

> **`FR-BKC-005` — Preview coverage per tarif**
>
> Untuk pasien asuransi, sistem menampilkan status coverage (`Covered`/`PartiallyCovered`/`NotCovered`/`NeedApproval`) per opsi tarif, dihitung dari `InsuranceCoverageService.ResolveTariffAsync`.
>
> **Contoh:** Pasien dengan asuransi X memilih kategori "Laboratorium". Tarif "Pemeriksaan Darah Lengkap" menampilkan badge "Tercover"; tarif "MRI" menampilkan badge "Tidak Tercover" karena tidak ada baris `MstInsuranceTariff` untuk kombinasi itu.
>
> Disposisi: `MISSING / NEW` (endpoint baru; logika `Reuse with adapter` dari `InsuranceCoverageService`).

> **`FR-BKC-006` — Item tidak tercover tetap bisa dipilih dan tetap masuk tagihan**
>
> Kasir dapat memilih tarif berstatus `NotCovered`; item tetap tersimpan sebagai `BilInvoiceItem` seperti biasa (perilaku ini TIDAK berubah dari sebelumnya — badge hanya informasi tambahan).
>
> **Contoh:** Kasir tetap menambahkan "MRI" (Tidak Tercover) karena pasien memintanya. Item masuk invoice; di Menu Pembayaran nilainya akan muncul di Subtotal Mandiri.
>
> Disposisi: `EXISTING / REUSE` (mekanisme penyimpanan item tidak berubah; hanya visibilitas informasi sebelum memilih yang baru).

> **`FR-BKC-007` — Rule butuh approval tidak menggagalkan perhitungan coverage**
>
> Pada `RegistrationBillingCoverageAdapter.ResolveAsync`, rule dengan `CoverageStatus="Covered"` yang juga bertanda `IsNeedApproval`/`IsNeedGuaranteeLetter` tetap dihitung tercover sesuai `CoveragePercent`-nya, tidak dipindah ke `unresolved`.
>
> **Contoh:** Rule coverage untuk "Fisioterapi" berstatus `Covered`, `CoveragePercent=80`, `IsNeedApproval=true`. Sebelum amendment: seluruh Rp500.000 masuk `unresolvedCoverageAmount` ("Penjamin Belum Terverifikasi"). Sesudah amendment: Rp400.000 (80%) masuk `primaryAmount` (Subtotal Asuransi), Rp100.000 sisanya masuk `patientAmount` (Subtotal Mandiri) — persis seperti rule tanpa flag approval.
>
> Disposisi: `EXTEND` (mengubah kondisi gating pada method existing).

### `EPIC BKC-03` — Subtotal Mandiri/Asuransi di Menu Pembayaran

Tujuan: kasir melihat dua subtotal terpisah untuk invoice mana pun, bukan hanya satu total gabungan.

> **`FR-BKC-008` — Dua baris subtotal terpisah** (`CAP-09`)
>
> Menu Pembayaran menampilkan "Subtotal Mandiri" (dari `patientAmount`) dan "Subtotal Asuransi" (dari `primaryAmount + excessAmount`) sebagai dua baris sejajar, bukan satu total dikurangi baris penjamin.
>
> **Contoh:** Invoice dengan `patientAmount=Rp150.000` dan `primaryAmount=Rp850.000` menampilkan "Subtotal Mandiri: Rp150.000" dan "Subtotal Asuransi: Rp850.000" berdampingan, bukan "Subtotal Tagihan: Rp1.000.000" dikurangi "Ditanggung Penjamin: Rp850.000".
>
> Disposisi: `EXTEND` (komposisi ulang tampilan; field data sumber `EXISTING / REUSE`).

## 11. Model status yang diusulkan

Tidak ada status baru pada `BilInvoice`/`BilInvoiceItem`/`BilCalculationVersion`. `SourceDomain` bertambah satu nilai valid (`"ADHOC_CATALOG"`) di samping nilai existing — bukan enum status lifecycle, murni penanda asal data. Lihat `contracts/state-transition-matrix.md` § Amendment 2 September 2026.

## 12. Sasaran arsitektur

| Yang dipakai ulang | Yang diperluas | Yang baru |
| --- | --- | --- |
| `GET Tariff/options` + `getTariffOptions`/`selectTariffOptions` (FE) — `CAP-02` | `BilInvoiceItem` (+`TariffId`), `ActiveEncounterOptionResponse` (+3 field), `RegistrationBillingCoverageAdapter.ResolveAsync` (gating dipersempit), `BillingChargeSourceAdapter.SourcePolicies` (+`ADHOC_CATALOG`) | `POST catalog-charges`, `GET catalog-charges/coverage-preview`, `AddCatalogChargeRequest`, `CatalogChargeCoveragePreviewResponse`, `BillingInvoiceService.AddCatalogChargeAsync`/`GetCatalogChargeCoveragePreviewAsync` |
| `InsuranceCoverageService.ResolveTariffAsync` (Clinical Management) — `CAP-04` | — | Thunk FE `addCatalogCharge`, `getCatalogChargeCoveragePreview` |

Detail lengkap: `02-backend-architecture.md` § Amendment 2 September 2026, `03-frontend-architecture.md` § Amendment 2 September 2026.

## 13. Sasaran kemampuan API

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/catalog-charges` | Tambah charge dari katalog tarif, harga server-side | `BillingInvoice : Create` | `AddCatalogChargeRequest` | `ApiResponse<InvoiceDetailResponse>` | `EPIC BKC-01` | **Rencana (belum tersedia)** |
| `GET` | `/catalog-charges/coverage-preview` | Preview coverage satu tarif (advisory) | `BillingInvoice : Read` | query `encounterId`, `tariffId`, `quantity` | `ApiResponse<CatalogChargeCoveragePreviewResponse>` | `EPIC BKC-02` | **Rencana (belum tersedia)** |

## 14. Matriks kewenangan

| Aksi | Kasir/QA-Dev | Billing | Product/Domain Owner |
| --- | :---: | :---: | :---: |
| `POST catalog-charges` | Ya, `[AccessPermission("BillingInvoice", "Create")]` | Ya | Lihat |
| `GET catalog-charges/coverage-preview` | Ya, `[AccessPermission("BillingInvoice", "Read")]` | Ya | Lihat |

Tidak ada peran baru; memakai resource `BillingInvoice` yang sudah ada.

## 15. Batas integrasi dan billing

Modul ini **MUST NOT** membangun ulang mesin coverage (`MstInsuranceCoverageRule`, `MstInsuranceTariff` tetap dimiliki/dikelola di luar billing-kasir), **MUST NOT** membuat endpoint HTTP baru di Clinical Management (pemanggilan `InsuranceCoverageService` cukup in-process/DI), dan **MUST NOT** mengubah kalkulasi pajak (`MstTaxRule`) — hanya tampilan yang berubah. Lihat `BIL-INT-010` (`contracts/integration-contract.md`) untuk kontrak pemanggilan in-process yang baru.

## 16. Guardrail regulasi

Tidak ada kewajiban rekam medis baru — form ini tidak menyentuh data klinis. Data pasien yang ditampilkan (nama, no. RM, no. kunjungan) mengikuti guardrail privasi existing modul (lihat `02-backend-architecture.md` § Security, privacy, exception, dan concurrency). `TariffId`, `ServiceUnitId`, `ClinicId`, `PatientClassId` bukan data sensitif.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-001` | `POST catalog-charges` **MUST** idempotent memakai `Idempotency-Key`, pola sama dengan `POST from-source` existing |
| `NFR-002` | `GET catalog-charges/coverage-preview` **MUST** read-only, tanpa transaksi database, aman dipanggil berulang tanpa efek samping |
| `NFR-003` | Kegagalan preview coverage **MUST NOT** memblokir alur submit item (fail-open pada UI, lihat `03-frontend-architecture.md`) |
| `NFR-004` | Perubahan gating pada `RegistrationBillingCoverageAdapter` **MUST** diverifikasi tidak meregresi test existing yang menyentuh coverage (`QuilvianSystemBackend.Tests/BillingManagement/BillingCalculationServiceTests.cs` dan 3 file lain yang mereferensikan adapter/rule — lihat § 16.1 CAP-05 pada `01-existing-capability-map.md`) |

## 18. Skenario UAT

> **`UAT-01` — Harga terisi otomatis dari tarif, tidak bisa diubah**
>
> **Kondisi awal:** Kasir membuka form testing, memilih kunjungan pasien tunai, kategori "Konsultasi".
>
> **Langkah:** Kasir memilih tarif "Konsultasi Dokter Umum" (Rp100.000) dari dropdown, mengisi Qty 1, submit.
>
> **Hasil yang diharapkan:** Invoice tersimpan dengan item seharga persis Rp100.000. Tidak ada kotak input harga yang dapat diketik di layar.

> **`UAT-02` — Tarif kedaluwarsa ditolak**
>
> **Kondisi awal:** Ada `MstTariff` dengan `IsActive=false`.
>
> **Langkah:** Request `POST catalog-charges` dikirim langsung (mem-bypass UI) dengan `TariffId` tarif nonaktif tersebut.
>
> **Hasil yang diharapkan:** Response `422` dengan pesan `BIL-VAL-025`. Tidak ada `BilInvoiceItem` baru tersimpan.

> **`UAT-03` — Item butuh approval tetap terhitung tercover**
>
> **Kondisi awal:** Pasien asuransi dengan rule coverage `CoverageStatus=Covered`, `CoveragePercent=100`, `IsNeedApproval=true` untuk tarif "Fisioterapi".
>
> **Langkah:** Kasir menambahkan item "Fisioterapi" lewat form testing, lalu membuka Menu Pembayaran dan menekan hitung ulang.
>
> **Hasil yang diharapkan:** Nilai fisioterapi masuk Subtotal Asuransi (bukan "Penjamin Belum Terverifikasi"). Badge preview pada form testing sebelumnya menunjukkan "Tercover" (bukan status gagal).

> **`UAT-04` — Preview berbeda dari final tetap dapat dijelaskan**
>
> **Kondisi awal:** Tarif punya `MstInsuranceCoverageRule` yang cocok tapi TIDAK punya baris `MstInsuranceTariff` untuk pasien tersebut.
>
> **Langkah:** Kasir melihat badge preview (menampilkan "Tidak Tercover" karena `InsuranceCoverageService` mensyaratkan `MstInsuranceTariff`), tetap menambahkan item, lalu membuka Menu Pembayaran.
>
> **Hasil yang diharapkan:** Menu Pembayaran menghitung ulang lewat `RegistrationBillingCoverageAdapter` (yang tidak mensyaratkan `MstInsuranceTariff`) — hasilnya BOLEH berbeda dari badge preview. Disclaimer "Perkiraan" pada badge preview memastikan kasir tidak kaget; tidak dianggap bug.

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Item form testing selalu dari `MstTariff`, harga tidak dapat diinput manual | `UAT-01`, `BIL-AT-025` |
| Tarif tidak aktif/kedaluwarsa ditolak | `UAT-02`, `BIL-AT-026` |
| Badge coverage 3 status tampil untuk pasien asuransi; approval tidak menggagalkan perhitungan | `UAT-03`, `BIL-AT-027` |
| Disparitas preview vs final terdokumentasi dan diberi disclaimer, bukan disembunyikan | `UAT-04`, `BIL-AT-028` |
| Menu Pembayaran menampilkan Subtotal Mandiri dan Subtotal Asuransi terpisah untuk invoice manapun | Manual click-through — belum ada bukti otomatis, dicatat sebagai kebutuhan verifikasi manual saat build |
| Seluruh dokumen kontrak (`02`–`03`, `erd/`, `contracts/`, `testing/`) konsisten menyebut endpoint/field yang sama | Tercatat pada masing-masing file amendment 2 September 2026 |
| Approval eksplisit `BKC-DEC-059`–`062` dari Product/Domain Owner | **Ya** — 2 September 2026 13:53 WIB, `00-interview-decisions.md` |
| Konfirmasi/persetujuan `BKC-DEC-062` dari Payer/Insurance + Finance/AR (owner asli `BKC-DEC-042`) | **Belum** — disetujui Product/Domain Owner tanpa konfirmasi terpisah dari owner asli; caveat tercatat pada baris `BKC-DEC-062`, lihat § 20 |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | Migration `TariffId` pada `BilInvoiceItem`; extend `ActiveEncounterOptionResponse`; registrasi `SourcePolicies["ADHOC_CATALOG"]` (`EPIC BKC-01` fondasi) | **Terpenuhi** — blueprint disetujui Product/Domain Owner 2 September 2026 |
| `MVP-1` | `POST catalog-charges`, `GET catalog-charges/coverage-preview`, perubahan gating `RegistrationBillingCoverageAdapter` (`EPIC BKC-01`, `EPIC BKC-02` backend) | `MVP-0` selesai. `BKC-DEC-062` sudah disetujui Product/Domain Owner (termasuk bagian gating adapter) — TANPA konfirmasi terpisah Payer/Insurance+Finance/AR selaku owner asli `BKC-DEC-042`, lihat caveat di `00-interview-decisions.md` |
| `MVP-2` | Frontend form testing: dropdown item, harga read-only, badge coverage, disclaimer (`EPIC BKC-01`, `EPIC BKC-02` frontend) | `MVP-1` selesai |
| `MVP-3` | Menu Pembayaran: Subtotal Mandiri/Asuransi terpisah (`EPIC BKC-03`) — secara teknis independen dari `MVP-2`, ditempatkan berurutan demi kesederhanaan roadmap, boleh dikerjakan paralel bila kapasitas ada | `MVP-0` selesai (tidak bergantung `MVP-1`/`MVP-2`) |
| `POST-MVP` | Penyatuan mesin coverage (§ 16.2.A), pelepasan gating limit bulanan, verifikasi `MstTaxRule.AllocationRule` | Di luar cakupan rilis ini |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| Approval eksplisit `BKC-DEC-059`–`061` (katalog tarif + preview, tanpa bagian amendemen `BKC-DEC-042`) | Product/Domain Owner | **Dijawab** 2 September 2026 13:53 WIB — tidak lagi memblokir | Tidak |
| Approval/konfirmasi `BKC-DEC-062` bagian gating adapter — idealnya dari Payer/Insurance + Finance/AR (owner asli `BKC-DEC-042`) | Payer/Insurance + Finance/AR Owner | **Dijawab oleh Product/Domain Owner** 2 September 2026 13:53 WIB dalam satu pernyataan approval yang sama, TANPA konfirmasi terpisah dari owner asli `BKC-DEC-042` — cukup untuk memulai `MVP-1`, tapi risiko provenance ini tetap tercatat; disarankan tetap diinformasikan ke Payer/Insurance+Finance/AR sebelum go-live produksi meski tidak memblokir coding | Tidak lagi (sudah dijawab) — risiko provenance tetap dicatat |
| Apakah `MaxAmountPerMonth`/`MaxQuantityPerMonth` juga dilepas gating-nya | Product/Domain Owner | Tidak menghentikan MVP (desain sudah memilih tetap gating) — hanya relevan bila cakupan diperluas nanti | Tidak |
| Nilai `MstTaxRule.AllocationRule` aktif saat ini | Finance/Tax Owner | Kebenaran "pajak hanya ke Subtotal Mandiri" tidak terverifikasi data — tampilan tetap jalan, isinya mungkin belum sesuai harapan sampai diverifikasi | Tidak (eksplisit ditunda atas permintaan Product/Domain Owner) |
| Kelengkapan data master `MstInsuranceTariff`/`MstInsuranceCoverageRule` untuk skenario UAT yang bermakna | Insurance/Finance Owner | `UAT-03`/`UAT-04` tidak dapat dijalankan dengan data nyata sampai master data terisi | Ya, untuk verifikasi UAT `EPIC BKC-02` saja — tidak memblokir coding |

---

# Amendment 3 September 2026 — PRD → MVP: Dokumen "Invoice Asuransi"

## A1. Identitas dokumen amendment

| Field | Nilai |
| --- | --- |
| Cakupan | Dokumen "Invoice Asuransi" pada halaman Dokumen Kasir, beserta fondasi kalkulasi yang dibutuhkannya |
| Revision | `0.6` |
| Status | **draft** — belum di-approve manusia |
| Keputusan bisnis dasar | `BKC-DEC-065`, `066`, `067`, `068`, `069` — approved Product/Domain Owner 3 September 2026 |
| Keputusan arsitektur | `BKC-DES-001`–`BKC-DES-009` (`02-backend-architecture.md`, draft) |
| Turunan dari | `02-backend-architecture.md` § Amendment 3 September 2026, `03-frontend-architecture.md` § Amendment 3 September 2026 (kedua), `contracts/*` § Amendment 3 September 2026, `erd/data-dictionary.md` § Amendment 3 September 2026 |
| Backend/frontend SHA diaudit | `a42b651d7518060dcc5e7df46cb495ef822b57f5` / `00210f9a5fb2f4f69e57b8c90c57c63c788da792` |

Seluruh entity, status, permission, dan endpoint yang disebut di bawah **sudah** tercatat pada dokumen arsitektur dan kontrak di atas. Bagian ini menurunkan, tidak menciptakan.

## A2. Ringkasan eksekutif

Rumah sakit sudah bisa mencetak Kwitansi (bukti pembayaran satu tender) dan Struk Pasien (rincian tagihan pasien). Yang belum ada adalah lembar yang bisa diserahkan ke perusahaan asuransi: satu halaman yang menyebutkan pasiennya siapa, perusahaan asuransinya siapa, layanan apa saja yang ditanggung, dan berapa rupiah yang ditanggung untuk setiap layanan itu.

Menyediakannya ternyata bukan pekerjaan tampilan. Mesin kalkulasi tagihan hari ini sudah menghitung berapa yang ditanggung penjamin untuk setiap potongan biaya, tetapi angka itu **dibuang** dan hanya totalnya yang disimpan. Karena itu amendment ini punya dua bagian: membuka pecahan rupiah per baris di backend, lalu memakainya untuk lembar dokumen.

## A3. Masalah produk

1. Pihak asuransi tidak punya lembar resmi dari sistem yang merinci tanggungannya per layanan. Yang tersedia hari ini adalah Struk Pasien, yang justru berisi seluruh item termasuk yang dibayar sendiri pasien — bukan yang mereka butuhkan.
2. Kasir tidak bisa menjawab pertanyaan "layanan ini ditanggung berapa?" dari layar mana pun. Menu Pembayaran hanya menampilkan satu angka Subtotal Asuransi untuk seluruh tagihan.
3. Penanda "Penjamin"/"Mandiri" per baris di Menu Pembayaran hari ini adalah **tebakan**: ia menggabungkan penanda `coverable` (apakah kategorinya boleh diklaim) dengan pemeriksaan apakah invoice itu dapat coverage sama sekali. Pada tagihan campuran, penanda ini bisa menandai baris yang sebenarnya tidak ditanggung sebagai "Penjamin". Ini tercatat sebagai jalan pintas pada `FE-BKC-FIX-006`.
4. Tab `Claim Letter` yang paling dekat maknanya sudah dicadangkan untuk modul `InsuranceManagement` yang belum diotorisasi dibangun, sehingga tidak bisa dipakai (`BKC-DEC-065`).

## A4. Visi produk

Satu lembar yang dapat dibaca dan dipercaya tiga pihak sekaligus — pasien, rumah sakit, dan perusahaan asuransi (`BKC-DEC-066`) — yang angkanya berasal dari mesin kalkulasi yang sama dengan yang menagih pasien, bukan dari perhitungan kedua yang bisa berbeda.

## A5. Batas MVP amendment ini

**Titik mulai:**

1. Sudah ada invoice untuk kunjungan pasien asuransi (mekanisme pembuatan invoice tidak berubah).
2. Kunjungan itu punya baris penjamin aktif bertipe `Insurance` dengan `InsuranceProviderId` terisi (data registrasi, di luar scope).
3. Ada sedikitnya satu `MstInsuranceCoverageRule` yang cocok, sehingga ada yang benar-benar ditanggung (data master, di luar scope).

**Titik akhir:**

1. Mesin kalkulasi mengembalikan `coveredAmount` per baris item, per biaya administrasi, dan per biaya kamar, yang jumlahnya sama persis dengan total tanggungan penjamin.
2. `GET {id}/insurance-invoice-document` mengembalikan lembar berisi identitas pasien, blok perusahaan asuransi, baris-baris yang ditanggung beserta rupiahnya, dan totalnya.
3. Tab "Invoice Asuransi" pada halaman Dokumen Kasir menampilkan lembar itu dan dapat mencetaknya sebagai PDF A4.
4. Keadaan yang tidak dapat menghasilkan dokumen (kunjungan tunai, penjamin perusahaan, tidak ada item tercover, snapshot lama) dijelaskan dengan bahasa yang dipahami kasir dan tombol cetaknya dimatikan.

**Di luar titik akhir ini** — bukan "ditunda", memang tidak diminta: pengiriman klaim elektronik ke perusahaan asuransi; isi resmi tab `Claim Letter`; tombol WhatsApp/Email untuk lembar ini; jejak audit siapa mencetak dokumen; dan perubahan formula coverage apa pun.

## A6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP ini |
| --- | --- |
| Kasir | Membuka tab Invoice Asuransi, memeriksa isinya, mencetak/mengunduh, dan menyerahkan lembarnya |
| Petugas Billing/Klaim | Memakai lembar sebagai lampiran pengajuan ke perusahaan asuransi |
| Pasien | Menerima lembar sebagai penjelasan bagian mana yang ditanggung asuransi dan bagian mana yang dibayar sendiri |
| Product/Domain Owner | Menyetujui `BKC-DES-001`–`009`, khususnya `BKC-DES-007` (nomor dokumen memakai nomor invoice) |
| Insurance/Finance Owner | Mengisi `MstInsuranceProvider` (nama, nomor kontrak, alamat) dan `MstInsuranceCoverageRule` agar dokumen punya isi |
| Security Owner | Menilai keputusan memakai ulang `BillingInvoice : Read` alih-alih permission tersendiri |
| Backend/Frontend Billing | Implementasi sesuai dokumen arsitektur amendment 3 September 2026 |

## A7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Pecahan rupiah tanggungan penjamin per baris biaya, diekspos dari perhitungan yang sudah ada | `CAP-05` (mesin coverage, § 16.1 `01-existing-capability-map.md`) — status asal `Ready to reuse`, kini diperluas keluarannya | **Wajib.** Tanpa ini `BKC-DEC-069` tidak dapat dipenuhi dan dokumen hanya bisa menampilkan badge status |
| Penanda ketersediaan rincian per baris untuk versi kalkulasi lama | Turunan `BKC-DES-004` | **Wajib.** Tanpa ini invoice lama menampilkan Rp 0 yang salah dan tidak terlihat salah |
| Endpoint penyusun lembar dokumen, termasuk penyaringan baris yang ditanggung | Baru | **Wajib.** `BKC-DEC-068` menetapkan penyaringan, dan penyaringan finansial tidak boleh di browser |
| Blok perusahaan asuransi dari `MstInsuranceProvider` | `MstInsuranceProvider` sudah ada dan sudah dirujuk `TrxPatientEncounterGuarantor` | **Wajib.** Ini inti `BKC-DEC-067` |
| Tab "Invoice Asuransi" beserta lembar dan cetak PDF | Halaman Dokumen Kasir sudah ada (`FE-BKC-017`); pola dokumen cetak sudah ada (`KwitansiDocument`) | **Wajib.** Ini bagian yang dilihat pengguna |
| Ukuran kertas PDF dapat dipilih pemanggil | `use-dokumen-kasir.js` `buildPdf` sudah ada, hari ini mengunci A5 | **Wajib.** Tabel dokumen ini tidak muat di A5 dan akan terpotong |

## A8. Kemampuan yang ditunda

| Kemampuan | ID/asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Rincian per baris untuk invoice yang sudah `FINAL`/`CLOSED` sebelum pembaruan | `BKC-DES-003`, § Rencana migration butir 2 | Menulis ulang `BreakdownSnapshot` yang sudah terkunci akan merusak bukti kalkulasi yang kolom itu ada untuk melindunginya | Dokumen tetap menampilkan **total** tanggungan penjamin (tersimpan sebagai kolom relasional `BilCalculationVersion.PrimaryAmount`) beserta keterangan bahwa rincian per item tidak tersedia. Petugas klaim tetap bisa menagih totalnya, sama seperti sebelum amendment ini ada |
| Penanda "Penjamin"/"Mandiri" per baris di Menu Pembayaran memakai rupiah sungguhan | `FE-BKC-FIX-006`, `03-frontend-architecture.md` § Perubahan tampilan Menu Pembayaran | Menyentuh layar yang paling sering dipakai kasir demi perbaikan kosmetik, sementara tab baru belum terbukti berjalan | Penanda hari ini tetap berjalan apa adanya (gabungan `coverable` dan total invoice) — tidak ada regresi, hanya belum diperbaiki. Kasir yang butuh angka pasti per baris dapat membuka tab Invoice Asuransi |
| Jejak audit siapa mencetak dokumen dan kapan | `02-backend-architecture.md` § Yang sengaja tidak dibuat | `GET` tidak dicatat custom logger sesuai konvensi project; jejak cetak butuh keputusan Security/Compliance dan tabel penyimpannya sendiri | Tidak ada pengganti — keterbatasan ini dinyatakan terbuka, bukan ditutupi. Lembar tercetak mencantumkan `calculationVersionNo` dan `calculatedAt` sehingga setidaknya dapat ditelusuri ke versi kalkulasi mana |
| Dukungan penjamin perusahaan tempat kerja (`MstCompanyGuarantor`) | `BKC-DEC-067` | Pemilik produk memilih perusahaan asuransi secara eksplisit; penjamin perusahaan punya bentuk dokumen dan alur tagih yang berbeda | Endpoint tetap menjawab `200` dengan penjelasan bahwa penjamin kunjungan itu bukan perusahaan asuransi. Penagihan ke perusahaan tempat kerja tetap memakai cara yang berjalan hari ini di luar sistem |
| Permission tersendiri untuk mencetak dokumen asuransi | `02-backend-architecture.md` § Security | Menambah resource permission baru berarti seluruh role harus di-remap sebelum fitur bisa dipakai siapa pun | Memakai `BillingInvoice : Read` yang sudah ada. Konsekuensinya dinyatakan terbuka pada `contracts/permission-audit-matrix.md` agar Security dapat menilai, bukan disembunyikan |

## A9. Alur bisnis target

**`FLOW-BKC-MVP-002` — Kasir menerbitkan Invoice Asuransi**

**Tujuan.** Menghasilkan satu lembar yang dapat diserahkan ke perusahaan asuransi dan dipahami pasien, berisi layanan yang ditanggung beserta rupiahnya.

**Pelaku.** Kasir menerbitkan dan mencetak. Petugas Billing/Klaim memakai lembarnya. Tidak ada persetujuan yang diperlukan — dokumen ini bacaan, bukan transaksi.

**Pemicu.** Kasir membuka Menu Pembayaran satu invoice, menekan "Dokumen Kasir", lalu memilih tab "Invoice Asuransi".

**Prasyarat.** Invoice sudah ada; kunjungan berpenjamin asuransi dengan perusahaan yang terdaftar di master; ada aturan coverage yang cocok sehingga ada yang benar-benar ditanggung.

**Langkah utama.**

1. Kasir membuka halaman Dokumen Kasir untuk satu invoice.
2. Kasir memilih tab "Invoice Asuransi".
3. Sistem meminta lembar dokumen ke `GET {id}/insurance-invoice-document`.
4. Untuk tagihan yang masih berjalan (`OPEN`), sistem menghitung ulang tagihan lebih dulu agar angkanya sama dengan yang dilihat kasir di Menu Pembayaran. Untuk tagihan yang sudah difinalkan, sistem membaca versi kalkulasi yang terkunci.
5. Sistem menentukan jenis penjamin kunjungan. Bila bukan asuransi, sistem berhenti di sini dan mengirim penjelasannya.
6. Sistem mengambil nama, nomor kontrak, dan alamat perusahaan asuransi dari master, serta nomor polis dan nomor anggota dari snapshot registrasi.
7. Sistem menyusun baris dokumen: hanya potongan biaya yang rupiah tanggungannya lebih dari nol.
8. Sistem menjumlahkan seluruh baris dan memastikan jumlahnya sama dengan total tanggungan penjamin. Bila tidak sama, permintaan dihentikan dengan pesan galat — lembar yang tidak menjumlah tidak boleh terbit.
9. Layar menampilkan lembar siap cetak beserta peringatan bila ada.
10. Kasir menekan "Cetak Invoice Asuransi". Berkas PDF A4 terunduh, dan kasir mencetak atau melampirkannya secara manual.

**Aturan bisnis yang berlaku.**

- Hanya potongan biaya dengan tanggungan lebih dari nol yang tampil (`BKC-DEC-068`).
- Setiap baris **wajib** menampilkan rupiah yang ditanggung, bukan hanya penanda status (`BKC-DEC-069`).
- "Perusahaan" pada dokumen adalah perusahaan asuransi, bukan perusahaan tempat pasien bekerja (`BKC-DEC-067`).
- Nomor dokumen adalah nomor invoice; tidak ada seri nomor tersendiri (`BKC-DES-007`).
- Nomor polis dan nomor anggota diambil dari keadaan saat registrasi, bukan dari kartu pasien yang berlaku sekarang (`BKC-DES-009`).

**Perubahan status.** Tidak ada. Mencetak dokumen tidak mengubah status invoice, tidak menandai klaim sebagai diajukan, dan tidak menciptakan piutang penjamin. Ketiganya tetap milik jalur finalisasi (`BKC-DEC-024`).

**Jalur tidak normal.**

| Keadaan | Yang dilihat kasir |
| --- | --- |
| Kunjungan dibayar tunai | Keterangan biru: kunjungan ini dibayar mandiri, tidak ada Invoice Asuransi yang dapat diterbitkan. Tanpa lembar, tanpa tombol cetak |
| Penjamin adalah perusahaan tempat kerja | Keterangan biru: penjamin kunjungan ini bukan perusahaan asuransi, dokumen belum mendukung penjamin perusahaan |
| Data penjamin kunjungan belum tercatat | Keterangan biru beserta arahan melengkapi data penjamin di Registrasi |
| Tidak ada satu pun item yang ditanggung | Keterangan biru: tidak ada item yang ditanggung asuransi pada tagihan ini |
| Tagihan difinalkan sebelum pembaruan sistem | Total tanggungan tetap tampil, beserta keterangan bahwa rincian per item tidak tersedia. Tombol cetak dimatikan |
| Perusahaan asuransi tidak ada di master | Blok asuransi berisi tanda hubung beserta arahan menghubungi admin master data |
| Tagihan tidak dapat dihitung (misalnya dua aturan pajak aktif) | Pesan galat merah berisi sebab sesungguhnya dari mesin kalkulasi — **bukan** lembar kosong |

**Hasil akhir.** Satu berkas PDF berada di komputer kasir. Tidak ada data yang berubah di sistem. Perusahaan asuransi menerima lembar yang angkanya dapat ditelusuri ke versi kalkulasi tertentu; pasien menerima penjelasan bagian mana yang ditanggung.

## A10. Epic dan functional requirement

### `EPIC BKC-04` — Pecahan rupiah tanggungan penjamin per baris biaya

Tujuan: angka "berapa yang ditanggung asuransi untuk layanan ini" tersedia untuk setiap potongan biaya, berasal dari perhitungan yang sudah ada, dan selalu menjumlah ke total tanggungan.

> **`FR-BKC-009` — Mesin coverage mencatat hasil per komponen**
>
> Sistem mengembalikan, untuk setiap potongan biaya yang dinilai penjamin, berapa rupiah ditanggung dan berapa rupiah yang statusnya belum jelas. Formula perhitungannya **tidak** berubah — yang berubah hanya hasil per komponen ikut disimpan alih-alih dibuang.
>
> **Contoh:** Item "Fisioterapi" Rp 300.000 dengan aturan `Covered` 80% menghasilkan tanggungan Rp 240.000 dan sisa Rp 60.000. Sebelum amendment, hanya Rp 240.000 yang ikut ke total invoice dan tidak ada cara mengetahui bahwa Rp 240.000 itu milik fisioterapi. Sesudah amendment, angka Rp 240.000 tercatat beralamat baris fisioterapi.
>
> Disposisi: `EXTEND` — `RegistrationBillingCoverageAdapter.ResolveAsync` sudah menghitungnya (`CAP-05`, `Ready to reuse`); keluarannya diperluas.

> **`FR-BKC-010` — Alamat alokasi tidak boleh tertukar antar baris**
>
> Sistem mengalamati setiap alokasi memakai kunci teks yang unik per potongan biaya (`ITEM:{invoiceItemId}`, `TAX:ITEM:{invoiceItemId}`, `ADMINISTRATION_FEE`, `TAX:ADMINISTRATION_FEE`, `ROOM_CHARGE`, `TAX:ROOM_CHARGE`), bukan memakai `ComponentId`.
>
> **Contoh:** Satu invoice punya dua item dan aturan pajak aktif. Kedua baris pajaknya membawa `ComponentId` yang sama, yaitu `TaxRuleId` dari satu-satunya aturan pajak aktif — sistem hanya mengizinkan satu aturan pajak aktif pada satu waktu. Bila alokasi dialamati dengan `ComponentId`, porsi pajak kedua item akan bertumpuk jadi satu dan salah satu item kehilangan porsi pajaknya.
>
> Disposisi: `MISSING / NEW` — kunci ini belum ada di kode mana pun.

> **`FR-BKC-011` — Rincian per baris wajib menjumlah ke total**
>
> Sistem menghentikan perhitungan bila jumlah tanggungan seluruh baris tidak sama dengan total tanggungan penjamin, dengan pesan yang menyuruh pengguna menghubungi tim teknis.
>
> **Contoh:** Tiga baris tercover Rp 100.000, Rp 240.000, dan Rp 15.000 berjumlah Rp 355.000. Bila total tanggungan yang dihitung mesin coverage ternyata Rp 350.000, perhitungan gagal — bukan diteruskan dengan selisih Rp 5.000 yang nanti muncul sebagai lembar tagihan yang tidak menjumlah di meja petugas klaim asuransi.
>
> Disposisi: `MISSING / NEW` (`BIL-VAL-028`).

> **`FR-BKC-012` — Versi kalkulasi lama menyatakan keterbatasannya sendiri**
>
> Sistem menyertakan penanda `isPerItemAllocationAvailable` pada hasil kalkulasi. Penanda ini bernilai `false` untuk versi kalkulasi yang tersimpan sebelum pembaruan ini, dan setiap pembaca **wajib** memeriksanya sebelum memercayai angka per baris.
>
> **Contoh:** Versi kalkulasi yang tersimpan 1 September 2026 tidak punya angka per baris. Dibaca setelah pembaruan, seluruh rincian per barisnya terbaca Rp 0. Penanda `false` inilah yang membedakan "asuransi menanggung Rp 0" dari "kami tidak punya rinciannya" — dua hal yang akibatnya jauh berbeda bagi petugas klaim.
>
> Disposisi: `MISSING / NEW` (`BKC-DES-004`).

> **`FR-BKC-013` — Biaya administrasi dan biaya kamar ikut punya rupiah tanggungan sendiri**
>
> Sistem menyediakan angka tanggungan untuk biaya administrasi dan biaya kamar, yang keduanya bukan item invoice tetapi bisa ditanggung penjamin.
>
> **Contoh:** Biaya administrasi Rp 15.000 dengan penanda `Coverable=true` dan aturan `Covered` 100% ditanggung penuh. Bila angka ini tidak tersedia, lembar dokumen hanya menjumlah Rp 340.000 dari tiga item, sementara total tanggungan Rp 355.000 — selisih Rp 15.000 yang tidak dapat dijelaskan kepada pihak asuransi.
>
> Disposisi: `EXTEND` — `AdministrationFeeCalculationResponse` dan `RoomChargeCalculationResponse` sudah ada; ditambah tiga field masing-masing (`BKC-DES-005`).

### `EPIC BKC-05` — Dokumen "Invoice Asuransi" pada Dokumen Kasir

Tujuan: kasir dapat menerbitkan, membaca, dan mencetak satu lembar yang layak diserahkan ke perusahaan asuransi.

> **`FR-BKC-014` — Endpoint penyusun lembar dokumen**
>
> Sistem menyediakan satu permintaan baca yang mengembalikan seluruh isi lembar: identitas pasien, blok perusahaan asuransi, baris-baris yang ditanggung beserta rupiahnya, total, penanda dapat-dicetak, dan daftar peringatan.
>
> **Contoh:** Satu permintaan `GET .../{id}/insurance-invoice-document` cukup untuk merender lembar utuh. Layar tidak perlu memanggil endpoint master data, endpoint kalkulasi, dan endpoint detail invoice lalu menjahitnya sendiri — penjahitan di browser berarti tiga permintaan yang bisa gagal terpisah dan lembar setengah jadi yang tetap tercetak.
>
> Disposisi: `MISSING / NEW`.

> **`FR-BKC-015` — Hanya baris yang ditanggung asuransi yang tampil**
>
> Sistem hanya menyertakan potongan biaya yang rupiah tanggungannya lebih dari nol. Penyaringan dikerjakan server, dan layar **tidak** menyaring apa pun.
>
> **Contoh:** Tagihan berisi Konsultasi Rp 100.000 (ditanggung penuh), Fisioterapi Rp 300.000 (ditanggung Rp 240.000), dan Vitamin C Rp 25.000 (tidak ada aturan cocok). Lembar memuat dua baris pertama saja. Vitamin C tetap ada di Struk Pasien karena pasien memang harus membayarnya, tetapi tidak ada urusannya dengan perusahaan asuransi.
>
> Disposisi: `MISSING / NEW` (`BKC-DEC-068`).

> **`FR-BKC-016` — Blok perusahaan berasal dari perusahaan asuransi**
>
> Sistem mengambil nama, nama grup, jenis, metode klaim, nomor kontrak, dan alamat dari master perusahaan asuransi yang dirujuk penjamin kunjungan. Nomor polis, nomor anggota, nama paket, dan kelas diambil dari snapshot registrasi kunjungan itu.
>
> **Contoh:** Kunjungan berpenjamin perusahaan asuransi samaran "Asuransi Sejahtera Nusantara" menampilkan nama itu beserta nomor kontrak kerja samanya. Bila pasien mengganti polis dua bulan setelah kunjungan, cetakan kedua tetap menampilkan nomor polis yang berlaku saat kunjungan — bukan yang berlaku hari ini.
>
> Disposisi: `MISSING / NEW` pada Billing; `EXISTING / REUSE` untuk sumber datanya (`MstInsuranceProvider`, `TrxPatientEncounterGuarantor`) — `BKC-DEC-067`, `BKC-DES-009`.

> **`FR-BKC-017` — Keadaan yang tidak dapat menghasilkan dokumen dijelaskan, bukan digagalkan**
>
> Sistem menjawab kunjungan tunai, kunjungan penjamin perusahaan, kunjungan tanpa data penjamin, tagihan tanpa item tercover, dan versi kalkulasi lama sebagai permintaan **berhasil** dengan penanda tidak-dapat-dicetak beserta penjelasannya. Layar menampilkannya sebagai keterangan biru, bukan pesan galat merah.
>
> **Contoh:** Kasir membuka tab Invoice Asuransi untuk kunjungan pasien tunai. Yang muncul adalah keterangan biru "Kunjungan ini dibayar mandiri, sehingga tidak ada Invoice Asuransi yang dapat diterbitkan." Kasir mengerti dan pindah tab. Bila ini dijawab sebagai galat merah, kasir akan mengira sistem sedang rusak dan melaporkannya sebagai bug.
>
> Disposisi: `MISSING / NEW` (`BKC-DES-008`, `BIL-VAL-029`–`034`).

> **`FR-BKC-018` — Tab dan lembar cetak pada halaman Dokumen Kasir**
>
> Sistem menyediakan tab ketiga bernama "Invoice Asuransi", sejajar Kwitansi dan Struk Pasien dan sebelum enam tab placeholder, yang menampilkan lembar siap cetak dan tombol cetak/unduh PDF.
>
> **Contoh:** Kasir menekan "Dokumen Kasir" dari Menu Pembayaran, memilih tab ketiga, membaca lembarnya, lalu menekan "Cetak Invoice Asuransi". Berkas `Invoice-Asuransi-BIL20260903000001.pdf` terunduh. Tab `Claim Letter` di sebelahnya tetap placeholder dan tidak tersentuh.
>
> Disposisi: `EXTEND` — halaman Dokumen Kasir dan pola komponen dokumen cetak sudah ada (`FE-BKC-017`, `KwitansiDocument`); ditambah satu tab dan satu komponen.

> **`FR-BKC-019` — Ukuran kertas PDF dapat dipilih pemanggil**
>
> Pembuat PDF menerima ukuran kertas sebagai pilihan, dengan bawaan tetap A5. Lembar Invoice Asuransi memakai A4.
>
> **Contoh:** Tabel Invoice Asuransi punya tujuh kolom, termasuk "Ditanggung Asuransi" dan "Porsi Pasien". Pada A5 (148 mm) kolom paling kanan terpotong dan angka rupiahnya tidak terbaca — persis kolom yang paling penting bagi pihak asuransi. Kwitansi dan Struk Pasien tetap A5 dan tidak berubah sama sekali.
>
> Disposisi: `EXTEND` — `buildPdf` sudah ada; ditambah satu parameter opsional.

> **`FR-BKC-020` — Lembar menyatakan kesegaran dan status tagihannya**
>
> Lembar mencantumkan nomor versi kalkulasi dan waktu perhitungannya. Bila tagihan masih berjalan, lembar menyatakannya secara tertulis.
>
> **Contoh:** Lembar dari tagihan `OPEN` memuat keterangan "Tagihan masih berjalan — angka dapat berubah sampai tagihan difinalkan." Tanpa keterangan ini, lembar dari tagihan yang belum selesai bisa ditagihkan oleh petugas klaim sebagai angka final, lalu berselisih dengan tagihan sesungguhnya beberapa hari kemudian.
>
> Disposisi: `MISSING / NEW`.

## A11. Model status yang diusulkan

Tidak ada status baru dan tidak ada transisi baru pada `BilInvoice`, `BilInvoiceItem`, maupun `BilCalculationVersion`. Yang ada hanyalah ketergantungan sumber angka pada status invoice yang sudah ada — lihat `contracts/state-transition-matrix.md` § Amendment 3 September 2026.

## A12. Sasaran arsitektur

| Yang dipakai ulang | Yang diperluas | Yang baru |
| --- | --- | --- |
| `MstInsuranceProvider`, `TrxPatientEncounterGuarantor`, `MstPatient`, `TrxPatientEncounter` (hanya dibaca) | `BillingCoverageComponent` (+`ComponentKey`), `BillingCoverageDecision` (+`Allocations`), `RegistrationBillingCoverageAdapter.ResolveAsync`, `BuildCoverageComponents`, `ApplyCoverageWaterfall` | `BillingCoverageComponentAllocation`, `BillingInsuranceInvoiceDocumentService`, `BillingInsuranceInvoiceDtos.cs` (5 DTO + 2 kelas konstanta), `GET {id}/insurance-invoice-document` |
| `BilCalculationVersion.BreakdownSnapshot` (kolom sama, isi lebih kaya) | `CalculationItemResponse` (+5 field), `AdministrationFeeCalculationResponse`/`RoomChargeCalculationResponse` (+3 field), `CoverageCalculationResponse` (+1 field), `BillingCalculationContract.Version` | — |
| Halaman Dokumen Kasir, `KwitansiDocument`/`StrukPasienDocument` sebagai pola, `useBillingInvoiceDetail`, `terbilangRupiah` | `dokumen-kasir-view.jsx`, `use-dokumen-kasir-page.js`, `use-dokumen-kasir.js` (`buildPdf`), `billing-invoice-slice.jsx`, `billing-invoice-constants.js` | `invoice-asuransi-document.jsx`, thunk `getInsuranceInvoiceDocument` |

**Tanpa tabel baru, tanpa kolom baru, tanpa migration** (`BKC-DES-003`). Detail lengkap: `02-backend-architecture.md` dan `03-frontend-architecture.md` § Amendment 3 September 2026.

## A13. Sasaran kemampuan API

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}/insurance-invoice-document` | Menyusun lembar Invoice Asuransi satu invoice | `BillingInvoice : Read` | — | `ApiResponse<InsuranceInvoiceDocumentResponse>` | `EPIC BKC-05` | **Rencana (belum tersedia)** |

Perubahan aditif pada response yang sudah ada (`GET /{id}`, `GET /{id}/calculation-preview`, `POST /{id}/recalculate`) tercatat pada `contracts/api-contract.md` § Amendment 3 September 2026. Kode status beserta artinya bagi pengguna juga di sana.

## A14. Matriks kewenangan

| Aksi | Kasir | Petugas Billing | Product/Domain Owner | Security Owner |
| --- | :---: | :---: | :---: | :---: |
| `GET {id}/insurance-invoice-document` | Ya, `[AccessPermission("BillingInvoice", "Read")]` | Ya | Lihat | Menilai keputusan pemakaian ulang permission |
| Mencetak/mengunduh lembar | Ya | Ya | Lihat | — |
| Mengubah isi lembar dari layar | Tidak | Tidak | Tidak | — |

Tidak ada peran baru dan tidak ada resource permission baru. Konsekuensi pemakaian ulang `BillingInvoice : Read` dinyatakan terbuka pada `contracts/permission-audit-matrix.md` § Amendment 3 September 2026.

## A15. Batas integrasi dan billing

Modul ini **MUST NOT** menyalin data perusahaan asuransi ke tabel `Bil*`, **MUST NOT** membaca `MstPatientInsurance` sebagai sumber nomor polis (`BKC-DES-009`), **MUST NOT** menghitung ulang coverage di luar `RegistrationBillingCoverageAdapter`, dan **MUST NOT** membuat kontrak pengiriman klaim ke perusahaan asuransi — yang terakhir tetap milik `InsuranceManagement` dan tetap bergantung `INS-DEC-005` yang belum diputuskan. Dua bacaan lintas konteks yang ditambahkan tercatat sebagai `BIL-INT-011` dan `BIL-INT-012` (`contracts/integration-contract.md`).

Dampak billing: **tidak ada**. Amendment ini tidak mengubah satu pun nominal yang ditagihkan, tidak menciptakan piutang, dan tidak mengubah kapan piutang penjamin lahir (`BKC-DEC-024` tetap berlaku).

## A16. Guardrail regulasi

Tidak ada kewajiban rekam medis baru — dokumen tidak memuat diagnosis, keluhan, maupun catatan klinis. Deskripsi layanan pada lembar adalah nama tarif administratif (misalnya "Fisioterapi"), tetap ditandai sensitif karena rangkaiannya dapat menyiratkan kondisi pasien. Nomor polis dan nomor anggota tampil karena pihak asuransi membutuhkannya untuk mengenali klaim; nomor kartu asuransi secara sengaja **tidak** disertakan. Seluruh field sensitif **MUST NOT** masuk payload log mana pun — `GET` tidak memakai custom logger.

## A17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-005` | `GET {id}/insurance-invoice-document` **MUST** read-only: tanpa transaksi, tanpa `Idempotency-Key`, aman dipanggil berulang tanpa efek samping |
| `NFR-006` | Perubahan pada `RegistrationBillingCoverageAdapter.ResolveAsync` **MUST** menghasilkan nominal `primaryAmount`/`unresolvedAmount` yang sama persis dengan sebelumnya untuk seluruh test coverage yang sudah ada. Satu saja nilai yang berubah berarti formula ikut tersentuh |
| `NFR-007` | Snapshot yang lahir pada `BIL-CALCULATION-0.5` **MUST** tetap dapat dibaca kode versi sebelumnya tanpa galat, sehingga rollback tidak memerlukan langkah mundur basis data |
| `NFR-008` | Lembar dokumen **MUST** dapat dicetak utuh pada kertas A4 tanpa kolom terpotong; Kwitansi dan Struk Pasien **MUST** tetap A5 |
| `NFR-009` | Tab Invoice Asuransi **MUST NOT** memanggil endpoint dokumen selama tab-nya belum dibuka — endpoint ini memicu kalkulasi pratinjau di server |
| `NFR-010` | Kegagalan menghitung tagihan **MUST** dilaporkan sebagai galat, **MUST NOT** disamarkan menjadi lembar kosong |

## A18. Skenario UAT

> **`UAT-05` — Lembar menampilkan rupiah tanggungan per baris dan menjumlah** *(jalur berhasil, `EPIC BKC-04`, `EPIC BKC-05`)*
>
> **Kondisi awal:** Pasien asuransi dengan perusahaan samaran "Asuransi Sejahtera Nusantara". Tagihan berisi Konsultasi Dokter Umum Rp 100.000 (aturan `Covered` 100%), Fisioterapi Rp 300.000 (aturan `Covered` 80%), Vitamin C Rp 25.000 (tanpa aturan cocok), biaya administrasi Rp 15.000 (`Coverable=true`, aturan `Covered` 100%).
>
> **Langkah:** Kasir membuka Menu Pembayaran, menekan "Dokumen Kasir", memilih tab "Invoice Asuransi".
>
> **Hasil yang diharapkan:** Lembar memuat tiga baris — Konsultasi Rp 100.000, Fisioterapi Rp 240.000, Biaya Administrasi Rp 15.000 — dengan total tanggungan Rp 355.000. Angka Rp 355.000 itu sama persis dengan Subtotal Asuransi yang tampil di Menu Pembayaran. Vitamin C tidak muncul.

> **`UAT-06` — Item yang tidak ditanggung tidak ikut tercetak** *(jalur berhasil, `EPIC BKC-05`)*
>
> **Kondisi awal:** Sama seperti `UAT-05`.
>
> **Langkah:** Kasir membandingkan tab "Struk Pasien" dan tab "Invoice Asuransi" untuk invoice yang sama.
>
> **Hasil yang diharapkan:** Struk Pasien memuat empat baris termasuk Vitamin C. Invoice Asuransi memuat tiga baris tanpa Vitamin C. Keduanya berasal dari invoice yang sama dan tidak ada yang salah — keduanya menjawab pertanyaan yang berbeda.

> **`UAT-07` — Kunjungan tunai dijelaskan, bukan digagalkan** *(jalur gagal, `EPIC BKC-05`)*
>
> **Kondisi awal:** Kunjungan pasien tunai yang sudah punya invoice berisi beberapa item.
>
> **Langkah:** Kasir membuka tab "Invoice Asuransi".
>
> **Hasil yang diharapkan:** Muncul keterangan **biru** "Kunjungan ini dibayar mandiri, sehingga tidak ada Invoice Asuransi yang dapat diterbitkan." Tidak ada lembar dokumen dan tidak ada tombol cetak. **Bukan** pesan galat merah, dan bukan lembar berisi tabel kosong.

> **`UAT-08` — Tagihan lama jujur soal keterbatasannya** *(jalur gagal, `EPIC BKC-04`)*
>
> **Kondisi awal:** Invoice pasien asuransi yang sudah `FINAL` sebelum pembaruan sistem, dengan total tanggungan penjamin Rp 500.000 tersimpan.
>
> **Langkah:** Kasir membuka tab "Invoice Asuransi".
>
> **Hasil yang diharapkan:** Total tanggungan Rp 500.000 tetap tampil, disertai keterangan "Rincian per item tidak tersedia untuk tagihan yang difinalkan sebelum pembaruan sistem ini. Total tanggungan penjamin tetap sah." Tombol cetak tidak muncul. **MUST NOT** menampilkan tabel berisi baris-baris Rp 0 yang terlihat seperti asuransi tidak menanggung apa pun.

> **`UAT-09` — Rincian yang tidak menjumlah tidak pernah terbit** *(jalur gagal, `EPIC BKC-04`)*
>
> **Kondisi awal:** Keadaan buatan di lingkungan uji: alokasi per baris sengaja dibuat berjumlah Rp 350.000 sementara total tanggungan Rp 355.000.
>
> **Langkah:** Sistem menghitung tagihan.
>
> **Hasil yang diharapkan:** Perhitungan **gagal** dengan pesan "Rincian tanggungan penjamin per baris tidak menjumlah ke total tanggungan; hubungi tim teknis." Tidak ada versi kalkulasi tersimpan dan tidak ada lembar yang terbit dengan selisih Rp 5.000.

> **`UAT-10` — Dokumen tidak membocorkan isi kesepakatan asuransi** *(jalur gagal, `EPIC BKC-05`)*
>
> **Kondisi awal:** Aturan coverage yang dipakai punya `RuleCode`, `ApprovalInstruction`, dan `BillingInstruction` terisi. Kartu asuransi pasien punya nomor kartu terisi.
>
> **Langkah:** Petugas memeriksa isi response endpoint dokumen dan lembar tercetak.
>
> **Hasil yang diharapkan:** Tidak ada kode aturan, instruksi approval, instruksi billing, nomor kartu, maupun kontak PIC perusahaan asuransi di response maupun di lembar. Yang tampil hanya nama, nama grup, jenis, metode klaim, nomor kontrak, dan alamat perusahaan.

> **`UAT-11` — Cetak A4 utuh, Kwitansi tetap A5** *(jalur berhasil, `EPIC BKC-05`)*
>
> **Kondisi awal:** Invoice seperti `UAT-05`.
>
> **Langkah:** Kasir menekan "Cetak Invoice Asuransi", lalu berpindah ke tab Kwitansi dan menekan "Cetak Kwitansi Pasien".
>
> **Hasil yang diharapkan:** Berkas pertama berukuran A4 dengan seluruh kolom tabel terbaca utuh, termasuk kolom paling kanan. Berkas kedua berukuran A5 dengan tampilan yang sama persis seperti sebelum amendment ini.

## A19. Definition of Done

| Butir | Dapat dijawab | Bukti |
| --- | --- | --- |
| Setiap potongan biaya punya rupiah tanggungan penjamin, dan jumlahnya sama dengan total tanggungan | Ya / Belum | `UAT-05`, `BIL-AT-029` |
| Alokasi tidak tertukar antar baris pajak | Ya / Belum | `BIL-AT-030` |
| Rincian yang tidak menjumlah menghentikan perhitungan | Ya / Belum | `UAT-09`, `BIL-AT-029` |
| Versi kalkulasi lama menyatakan keterbatasannya, bukan menampilkan Rp 0 | Ya / Belum | `UAT-08`, `BIL-AT-034` |
| Lembar hanya memuat baris yang ditanggung asuransi | Ya / Belum | `UAT-06`, `BIL-AT-031` |
| Blok perusahaan berasal dari perusahaan asuransi, dan polis dari snapshot registrasi | Ya / Belum | `BIL-AT-032` |
| Kunjungan bukan-asuransi dijelaskan sebagai keadaan wajar | Ya / Belum | `UAT-07`, `BIL-AT-033` |
| Dokumen tidak membocorkan isi kesepakatan asuransi maupun nomor kartu | Ya / Belum | `UAT-10`, `BIL-AT-035` |
| Cetak A4 utuh; Kwitansi dan Struk Pasien tetap A5 tanpa regresi | Ya / Belum | `UAT-11`, § Regresi pada `testing/acceptance-test-matrix.md` |
| Seluruh nominal pada test coverage yang sudah ada tidak berubah | Ya / Belum | § Regresi pada `testing/acceptance-test-matrix.md` (`NFR-006`) |
| Seluruh dokumen kontrak menyebut endpoint, field, dan kode validasi yang sama | Ya / Belum | Amendment 3 September 2026 pada `02`, `03`, `erd/`, `contracts/`, `testing/` |
| Approval eksplisit `BKC-DEC-065`–`069` dari Product/Domain Owner | **Ya** | 3 September 2026, `00-interview-decisions.md` |
| Approval `BKC-DES-001`–`009` (keputusan arsitektur amendment ini) | **Belum** | Blueprint masih `draft`; approval tetap tindakan manusia |
| Penilaian Security atas pemakaian ulang `BillingInvoice : Read` | **Belum** | Lihat § A20 pertanyaan terbuka |

## A20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-4` | Fondasi kalkulasi: `ComponentKey`, `BillingCoverageComponentAllocation`, `Allocations` pada `BillingCoverageDecision`, pembagian alokasi di `ApplyCoverageWaterfall`, `BIL-VAL-028`, field baru pada empat DTO, versi kontrak `0.4` → `0.5` (`EPIC BKC-04`) | Approval `BKC-DES-001`–`006` beserta wewenang tulis backend. Tidak bergantung `MVP-0`–`MVP-3` |
| `MVP-5` | Endpoint dokumen: `BillingInsuranceInvoiceDocumentService`, `BillingInsuranceInvoiceDtos.cs`, `GET {id}/insurance-invoice-document`, registrasi DI (`EPIC BKC-05` backend) | `MVP-4` **selesai dan terverifikasi**. Dideploy sebelum `MVP-4` akan selalu melaporkan rincian tidak tersedia untuk semua invoice |
| `MVP-6` | Frontend: tab, `invoice-asuransi-document.jsx`, thunk dan state, `buildPdf` berparameter ukuran kertas, perbaikan inisialisasi tab dari query string (`EPIC BKC-05` frontend) | `MVP-5` selesai. Kontrak response terkunci lebih dulu — `BKC-DEC-069` sudah menyatakan ini bukan pekerjaan frontend murni |
| `POST-MVP` | Penanda per baris di Menu Pembayaran memakai rupiah sungguhan; jejak audit cetak dokumen; dukungan penjamin perusahaan; permission tersendiri untuk cetak dokumen asuransi; perapian kepala surat rumah sakit yang tersalin di tiga komponen; perapian nama folder `Dtos/` | Di luar cakupan rilis ini; masing-masing butuh keputusan pemiliknya sendiri |

Epic berstatus `OPEN DECISION`: **tidak ada**. Seluruh functional requirement `FR-BKC-009`–`FR-BKC-020` berdisposisi `EXISTING / REUSE`, `EXTEND`, atau `MISSING / NEW`.

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| Approval `BKC-DES-001`–`009`, khususnya `BKC-DES-007` (nomor dokumen memakai `InvoiceNumber`, tanpa seri nomor tersendiri) | Product/Domain Owner | Bila kelak diminta nomor tersendiri, seri nomor baru harus ditambahkan dan lembar yang sudah tercetak akan memakai penomoran berbeda dari yang berikutnya | **Ya** — blueprint tidak boleh diteruskan ke `/plan-module-delivery` sebelum dijawab |
| Apakah pemakaian ulang `BillingInvoice : Read` untuk mencetak dokumen berisi nomor polis dapat diterima, atau perlu permission tersendiri | Security Owner | Bila kelak dipisah, role harus di-remap dan pengguna yang tadinya bisa mencetak akan kehilangan akses tanpa perubahan permintaan bisnis | **Ya** untuk `MVP-5`; `MVP-4` tidak terpengaruh karena tidak menambah endpoint |
| Apakah tidak adanya jejak audit "siapa mencetak dokumen ini" dapat diterima | Security/Compliance Owner | Bila terjadi sengketa klaim, tidak ada cara mengetahui lembar mana yang pernah keluar dan oleh siapa | Tidak — dicatat sebagai keterbatasan yang diketahui, tidak menghentikan MVP |
| Apakah lembar dari tagihan yang masih `OPEN` boleh diserahkan ke perusahaan asuransi | Billing/Finance/AR Owner | Angkanya masih bisa berubah; lembar sudah mencantumkan keterangan tagihan berjalan, tetapi tidak ada pencegahan teknis | Tidak — `BKC-DEC-066` menghendaki dokumen dapat dipakai tiga pihak, termasuk sebelum finalisasi |
| Kelengkapan `MstInsuranceProvider` (nomor kontrak, alamat) dan `MstInsuranceCoverageRule` untuk skenario UAT yang bermakna | Insurance/Finance Owner | `UAT-05`, `UAT-06`, dan `UAT-10` tidak dapat dijalankan dengan data nyata sampai master terisi | Ya, untuk verifikasi UAT saja — tidak memblokir coding |
| Wewenang tulis backend dan frontend (task mode, branch) | Pengguna | Prasyarat prosedural sebelum `/build-module-backend` dan `/build-module-frontend` dijalankan | Ya, untuk implementasi — bukan untuk desain |

**Status dokumen ini: `draft`.** Terdapat pertanyaan terbuka bertanda memblokir, sehingga amendment ini **MUST NOT** diteruskan ke `/plan-module-delivery` sebelum `BKC-DES-001`–`009` disetujui dan Security menilai pemakaian ulang permission.
