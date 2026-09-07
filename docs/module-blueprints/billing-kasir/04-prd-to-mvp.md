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

---

# Amendment 4 September 2026 — PRD → MVP: Pembagian tanggungan penjamin, anomali data, dan gerbang PPN

## B1. Identitas dokumen amendment

| Field | Nilai |
| --- | --- |
| Produk | Sistem Informasi Rumah Sakit Quilvian |
| Modul | Billing dan Kasir (`billing-kasir`), blueprint `BIL-CASH-001` |
| Kode modul untuk penomoran | `BKC` |
| Status | **draft** — approval adalah tindakan manusia dan belum diberikan |
| Revision blueprint | `0.7` |
| Repository target | `NewQuilvianSystemBackend` (backend), `QuilvianSystemFrontendDev` (frontend) |
| Commit SHA baseline backend | `ffeb45a83a6282982214668acc57e15ac0652f04` **beserta working tree yang belum di-commit** |
| Commit SHA baseline frontend | `00210f9a5fb2f4f69e57b8c90c57c63c788da792` **beserta working tree yang belum di-commit** |
| Keputusan bisnis dasar | `BKC-DEC-070`–`079`, approved Product/Domain Owner 4 September 2026 |
| Keputusan arsitektur | `BKC-DES-010`–`020`, status `draft` |
| Ringkasan cakupan | Tagihan berhenti menampilkan baris "Penjamin Belum Terverifikasi"; masalah data pendaftaran ditampilkan sebagai peringatan tersendiri; PPN obat dan alat kesehatan dibebaskan untuk rawat inap |

## B2. Ringkasan eksekutif

Tiga hal yang berubah bagi orang yang benar-benar memakai sistem ini:

1. **Kasir berhenti melihat angka yang tidak dapat ditagihkan kepada siapa pun.** Baris "Penjamin Belum Terverifikasi" hilang. Setiap rupiah pada tagihan kini punya penanggung yang jelas: pasien atau perusahaan asuransi.
2. **Kesalahan data pendaftaran akhirnya terlihat dan tertulis penyebabnya.** Sebelumnya, penjamin yang belum dinyatakan layak berakhir sebagai angka menggantung tanpa penjelasan. Sekarang kasir melihat kalimat yang menyebut apa yang salah dan siapa yang harus membetulkannya, sambil tetap dapat menerima pembayaran.
3. **Pasien rawat inap berhenti dikenai PPN atas obat dan alat kesehatan.** Pasien rawat jalan dan IGD tetap dikenai. Tagihan rawat inap yang masih berjalan akan turun nilainya pada perhitungan ulang pertama setelah perubahan ini berlaku.

Yang **tidak** berubah, dan itu disengaja: cara menghitung berapa yang ditanggung asuransi (persentase, urun biaya, batas atas) sama persis seperti sekarang.

## B3. Masalah produk

| Masalah | Bukti kode | Akibat nyata |
| --- | --- | --- |
| Tagihan menampilkan bucket "Penjamin Belum Terverifikasi" yang isinya lima keadaan berbeda | `BillingCoverageAdapter.cs` — lima jalur berbeda menulis ke variabel `unresolved` yang sama | Kasir tidak tahu apakah angka itu akan ditanggung asuransi, harus ditagih ke pasien, atau menandakan ada data salah. Ketiganya terlihat identik |
| Item yang aturannya sudah menyatakan ditanggung tetap tertahan hanya karena aturan itu juga menandai "butuh persetujuan" | `BillingCoverageAdapter.cs` — cabang `NeedApproval` dan limit bulanan | Subtotal Asuransi terlalu kecil pada banyak kunjungan, padahal persetujuan adalah urusan administrasi terpisah, bukan penolakan tanggungan (`CAP-05`) |
| Item yang tidak punya aturan tanggungan berakhir menggantung, bukan menjadi tagihan pasien | `BillingCoverageAdapter.cs` — cabang `rule is null` | Formula yang sudah disetujui `BKC-DEC-062` menempatkannya di Subtotal Mandiri, tetapi kode menempatkannya di bucket menggantung |
| Data penjamin yang salah tidak menghasilkan peringatan apa pun | `BillingCoverageAdapter.cs` — `Unresolved(...)` mengembalikan angka tanpa keterangan | Kesalahan pendaftaran baru ketahuan saat klaim ditolak perusahaan asuransi, berbulan-bulan kemudian |
| PPN dikenakan tanpa melihat jenis kunjungan | `BillingCalculationService.ApplyInvoiceTax` hanya menyaring `item.IsPharmacy` | Pasien rawat inap dikenai PPN yang menurut keputusan pemilik produk seharusnya dibebaskan (`BKC-DEC-078`) |
| Nilai pembagian PPN antar-penanggung belum pernah diverifikasi | `MstTaxRule` tidak punya seed di migration mana pun; nilainya murni data runtime | Bila nilainya bukan berimbang, seluruh PPN rawat jalan salah dialokasikan tanpa peringatan apa pun (`CAP-07`) |

## B4. Visi produk

Rantai keterhubungan yang ingin dicapai, ditulis sebagai urutan:

1. Petugas pendaftaran mencatat penjamin kunjungan beserta kelayakan dan keaktifan polisnya.
2. Jenis kunjungan tercatat pada tagihan saat tagihan itu dibuka, dan tidak berubah setelahnya.
3. Setiap biaya yang masuk dicocokkan dengan aturan tanggungan perusahaan asuransi, satu per satu.
4. Rupiah yang ditanggung asuransi dihitung untuk setiap baris, bukan hanya totalnya.
5. PPN dikenakan hanya atas obat dan alat kesehatan, dan hanya pada kunjungan rawat jalan atau IGD.
6. PPN yang dikenakan mengikuti nasib barang yang dipajakinya: ikut ke asuransi bila barangnya ditanggung, ke pasien bila tidak.
7. Kasir melihat dua subtotal yang menjumlah persis ke total tagihan, ditambah peringatan terpisah bila ada data pendaftaran yang bermasalah.
8. Lembar Invoice Asuransi memakai angka per baris yang sama, tanpa menghitung ulang apa pun.

## B5. Batas MVP amendment ini

**Titik mulai:** sebuah tagihan berstatus `OPEN` dihitung ulang, baik sebagai pratinjau di Menu Pembayaran maupun sebagai versi kalkulasi yang disimpan.

**Titik akhir:** kasir melihat Ringkasan Pembayaran yang seluruh barisnya menjumlah persis ke Total Tagihan, tanpa baris "Penjamin Belum Terverifikasi", dengan peringatan anomali data bila ada, dan dengan PPN yang benar menurut jenis kunjungannya.

**Di dalam batas:**

1. Pencabutan gerbang persetujuan dan limit bulanan pada penilaian penjamin.
2. Pengalihan jalur "tidak ada aturan cocok" menjadi tanggungan pasien.
3. Kategori anomali data beserta kode, kalimat, dan cara menampilkannya.
4. Penyempitan makna `unresolvedAmount` menjadi selisih yang tidak ditagihkan.
5. Gerbang PPN berdasarkan jenis kunjungan.
6. Perubahan tampilan Ringkasan Pembayaran dan badge per baris di Menu Pembayaran.

**Di luar batas:**

1. Formula perhitungan tanggungan (`CalculateCoveredAmount`) — tidak berubah satu baris pun.
2. Proses verifikasi penjamin di modul Registrasi.
3. Koreksi nilai `MstTaxRule.AllocationRule` di database — itu tindakan data, bukan pekerjaan kode.
4. Pemblokiran finalisasi tagihan yang beranomali — aturan bisnis baru yang belum diminta.
5. Penghapusan kolom `IsNeedApproval`, `IsNeedGuaranteeLetter`, `MaxAmountPerMonth`, `MaxQuantityPerMonth` dari master data.

## B6. Pelaku sasaran

| Pelaku | Tanggung jawab di dalam MVP ini |
| --- | --- |
| Kasir | Membaca Ringkasan Pembayaran, menerima pembayaran porsi pasien, dan menindaklanjuti peringatan anomali data dengan menghubungi Pendaftaran |
| Supervisor Billing | Memicu perhitungan ulang, dan memutuskan apakah tagihan beranomali ditahan atau diteruskan |
| Petugas Pendaftaran | Membetulkan kelayakan penjamin, keaktifan polis, dan pilihan perusahaan asuransi — di modul Registrasi, bukan di layar kasir |
| Admin Data Induk Asuransi | Melengkapi aturan tanggungan, karena setelah pencabutan gerbang, tarif tanpa aturan langsung menjadi tanggungan pasien tanpa peringatan |
| Finance/Tax Owner | Memastikan tarif PPN yang aktif memakai pembagian berimbang, dan hanya ada satu tarif aktif pada satu waktu |

## B7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Tanggungan asuransi dihitung tanpa tertahan gerbang persetujuan dan limit bulanan | `CAP-05` | **Wajib.** Tanpa ini, Subtotal Asuransi tetap salah pada mayoritas kunjungan, dan `BKC-DEC-071` tidak berjalan sama sekali |
| Rupiah tanggungan per baris tersedia dan dapat dipercaya | `CAP-09` | **Wajib.** Ringkasan Pembayaran, badge per baris, dan lembar Invoice Asuransi ketiganya bersumber dari angka ini |
| Kategori anomali data beserta kalimat penjelasnya | `CAP-05` (turunan) | **Wajib.** Tanpa ini, `BKC-DEC-073` tidak dapat dipenuhi: pemeriksaan tetap ada tetapi hasilnya tidak sampai ke siapa pun |
| PPN dibebaskan untuk kunjungan rawat inap | `CAP-07` | **Wajib.** `BKC-DEC-078` menyatakan pembebasan ini sebagai kewajiban, bukan penyempurnaan; tagihan rawat inap hari ini memungut pajak yang tidak seharusnya |
| Pembagian PPN mengikuti nasib barang yang dipajakinya | `CAP-07` | **Wajib**, tetapi **tanpa perubahan kode** — mekanismenya sudah ada; yang wajib adalah verifikasi nilai data induknya |

## B8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Pemblokiran finalisasi tagihan yang beranomali data | — | Aturan bisnis baru yang tidak diminta `BKC-DEC-073`; memblokir finalisasi menahan penutupan tagihan karena kesalahan data unit lain | Peringatan kuning di Menu Pembayaran, ditambah kode anomali yang ikut tersimpan pada versi kalkulasi sehingga dapat dicari kemudian |
| Pemeriksaan limit pemakaian bulanan di tempat lain (misalnya layar persetujuan) | — | `BKC-DEC-071` **mencabut** pemeriksaan itu, bukan memindahkannya. Membangun tempat baru berarti mengarang requirement | Tidak ada pengganti, dan itu memang keputusan yang diambil. Klaim yang melewati limit dikoreksi lewat Pengecualian Finansial (`BKC-DEC-032`–`035`) |
| Peringatan otomatis bila tarif PPN aktif tidak memakai pembagian berimbang | — | Butuh keputusan pemilik data induk tentang apakah salah konfigurasi menghentikan kalkulasi atau sekadar memperingatkan | Pemeriksaan manual satu kali sebelum perubahan diberlakukan, ditambah nilai pembagian yang sudah ikut tampil pada rincian pajak per baris |
| Keputusan PPN untuk pemeriksaan kesehatan berkala dan konsultasi jarak jauh | — | `BKC-DEC-078`–`079` hanya menyebut rawat jalan, IGD, dan rawat inap | Keduanya dikenai PPN mengikuti bentuk daftar bebas pajak (`BKC-DES-019`), dan hasilnya dilampirkan pada `BKC-OQ-083` sebagai bahan keputusan |
| Penghapusan empat kolom master yang berhenti dibaca | — | Kolomnya masih dibaca `InsuranceCoverageService` dan masih berarti bagi petugas klaim | Kolomnya tetap ada dan tetap dapat diisi; keterangan pada layar master data menyatakan bahwa keempatnya tidak lagi menahan perhitungan tagihan |

## B9. Alur bisnis target

Alur `FLOW-BKC-MVP-002` — perhitungan tagihan sampai kasir melihat ringkasan yang benar. Gambar prosesnya ada di [`flowcharts/pembagian-tanggungan-penjamin.md`](flowcharts/pembagian-tanggungan-penjamin.md) dan [`flowcharts/ppn-obat-alkes.md`](flowcharts/ppn-obat-alkes.md); di sini hanya urutannya.

1. Kasir membuka Menu Pembayaran untuk satu tagihan.
2. Sistem mengambil seluruh baris biaya yang masih berlaku pada tagihan itu.
3. Sistem menghitung biaya administrasi dan biaya kamar.
4. Sistem memeriksa jenis kunjungan tagihan. Bila rawat inap, tidak ada PPN sama sekali; selain itu, PPN dihitung atas obat dan alat kesehatan lalu dibagikan ke tiap baris obat.
5. Sistem menyusun daftar potongan biaya yang akan dinilai penjamin — setiap item, pajak tiap item, biaya administrasi, dan biaya kamar.
6. Sistem memeriksa sumber pembayaran kunjungan. Tunai berarti seluruhnya tanggungan pasien, dan proses berhenti di sini.
7. Sistem memeriksa kelengkapan data penjamin. Bila bermasalah, seluruh potongan biaya ditandai anomali data, nilainya jatuh ke pasien, dan kodenya dicatat.
8. Untuk setiap potongan biaya, sistem mencari aturan tanggungan yang cocok. Tidak ada aturan berarti tanggungan pasien.
9. Aturan yang menyatakan tidak ditanggung berarti tanggungan pasien, kecuali kontrak melarang menagihkannya.
10. Aturan yang menyatakan ditanggung dihitung sesuai persentase, urun biaya, dan batas atasnya — tanpa dipengaruhi penanda persetujuan maupun limit bulanan.
11. Sisa yang belum tertanggung menjadi tanggungan pasien, atau menjadi selisih yang tidak ditagihkan bila kontrak melarangnya.
12. Sistem memeriksa bahwa jumlah tanggungan per baris sama persis dengan total tanggungan. Bila tidak, perhitungan dihentikan.
13. Kasir melihat Subtotal Mandiri, Subtotal Asuransi, Pajak Mandiri, Pajak Asuransi, dan Total Tagihan yang menjumlah persis.
14. Bila ada anomali data, peringatan kuning muncul di atas ringkasan beserta nominal terdampak dan tindakan yang harus dilakukan.
15. Kasir menerima pembayaran porsi pasien.

## B10. Epic dan functional requirement

### `EPIC BKC-06` — Pembagian tanggungan penjamin tanpa bucket menggantung

**Tujuan.** Setiap rupiah pada tagihan punya penanggung yang jelas.

**Disposisi backend:** `EXTEND` — adapter dan mesin kalkulasi sudah ada; cabang keputusannya yang berubah.

> **FR-BKC-021 — Penanda persetujuan tidak lagi menahan tanggungan**
>
> Aturan yang menyatakan sebuah tarif ditanggung tetap dihitung penuh walaupun aturan itu menandai butuh persetujuan atau surat jaminan.
>
> **Contoh:** Fisioterapi Rp 300.000 memiliki aturan ditanggung 80% yang juga menandai "butuh surat jaminan". Sebelumnya seluruh Rp 300.000 tertahan. Sekarang Rp 240.000 masuk Subtotal Asuransi dan Rp 60.000 masuk Subtotal Mandiri.

> **FR-BKC-022 — Limit pemakaian bulanan tidak lagi menahan tanggungan**
>
> Aturan yang mencantumkan batas nominal atau batas jumlah per bulan tetap dihitung penuh.
>
> **Contoh:** Konsultasi Rp 100.000 memiliki aturan ditanggung 100% dengan batas Rp 500.000 per bulan. Sebelumnya seluruh Rp 100.000 tertahan karena pemakaian kumulatif tidak dapat diperiksa. Sekarang Rp 100.000 masuk Subtotal Asuransi.

> **FR-BKC-023 — Tarif tanpa aturan menjadi tanggungan pasien**
>
> Potongan biaya yang tidak cocok dengan satu pun aturan tanggungan seluruhnya menjadi porsi pasien, bukan bucket menggantung.
>
> **Contoh:** Vitamin C Rp 25.000 tidak punya aturan. Subtotal Mandiri bertambah Rp 25.000, `unresolvedAmount` tetap Rp 0, dan badge barisnya "Tunai".

> **FR-BKC-024 — Batas per kunjungan tetap berlaku**
>
> Batas nominal per kunjungan dan batas jumlah per kunjungan tetap membatasi tanggungan.
>
> **Contoh:** Aturan menanggung 100% dengan batas Rp 200.000 per kunjungan. Tindakan Rp 300.000 menghasilkan Rp 200.000 ke Subtotal Asuransi dan Rp 100.000 ke Subtotal Mandiri.

> **FR-BKC-025 — Selisih yang tidak boleh ditagihkan punya barisnya sendiri**
>
> Sisa yang menurut kontrak penjamin tidak boleh dibebankan ke pasien tampil sebagai baris bernama, dan hanya muncul bila nilainya lebih besar dari nol.
>
> **Contoh:** Aturan menanggung 70% dengan penanda "kelebihan tidak boleh ditagihkan ke pasien". Tindakan Rp 100.000 menghasilkan Rp 70.000 ke Subtotal Asuransi, Rp 0 ke Subtotal Mandiri, dan Rp 30.000 pada baris "Selisih Tidak Ditagihkan (kontrak penjamin)".

> **FR-BKC-026 — Ringkasan pembayaran selalu menjumlah**
>
> Subtotal Mandiri ditambah Subtotal Asuransi ditambah Pajak Mandiri ditambah Pajak Asuransi ditambah Selisih Tidak Ditagihkan sama persis dengan Total Tagihan.
>
> **Contoh:** Tagihan Rp 425.000 dengan Subtotal Mandiri Rp 85.000, Subtotal Asuransi Rp 340.000, pajak Rp 0, dan selisih Rp 0. Jumlahnya Rp 425.000, tanpa selisih satu rupiah pun.

### `EPIC BKC-07` — Anomali data penjamin

**Tujuan.** Kesalahan data pendaftaran terlihat oleh orang yang dapat menindaklanjutinya, tanpa menahan pembayaran pasien.

**Disposisi backend:** `MISSING / NEW` — kategori ini belum ada dalam bentuk apa pun.

> **FR-BKC-027 — Empat keadaan data ditandai sebagai anomali**
>
> Penjamin yang belum dinyatakan layak, polis yang tercatat tidak aktif, perusahaan asuransi yang belum dipilih, dan kunjungan yang tidak ditemukan masing-masing menghasilkan kode anomali tersendiri.
>
> **Contoh:** Kunjungan dengan kelayakan penjamin belum dicentang menghasilkan `hasDataAnomaly` bernilai benar dan `anomalyCodes` berisi `PAYER_NOT_ELIGIBLE`.

> **FR-BKC-028 — Anomali tidak menghentikan perhitungan**
>
> Tagihan dengan anomali data tetap dapat dihitung, tetap dapat dibayar, dan tetap menjumlah.
>
> **Contoh:** Biaya coverable Rp 440.000 dengan kelayakan belum dicentang. Perhitungan berhasil, Subtotal Mandiri Rp 440.000, Subtotal Asuransi Rp 0, Total Tagihan Rp 440.000. Kasir menerima Rp 440.000.

> **FR-BKC-029 — Anomali tampil sebagai peringatan, bukan baris subtotal**
>
> Nominal anomali tidak pernah muncul sebagai baris di dalam Ringkasan Pembayaran.
>
> **Contoh:** Pada kasus di atas, "Rp 440.000" muncul di dalam kalimat peringatan kuning di atas ringkasan, sementara Ringkasan Pembayaran hanya memuat Subtotal Mandiri, Subtotal Asuransi, dan Total Tagihan.

> **FR-BKC-030 — Tanggungan yang ditolak tidak boleh diam-diam menjadi tagihan pasien**
>
> Nilai yang penjaminnya menolak hanya boleh menjadi porsi pasien bila tercatat sebagai anomali data. Bila tidak, perhitungan dihentikan.
>
> **Contoh:** Bila suatu saat kode menghasilkan status ditolak tanpa mengisi nominal anomali, perhitungan gagal dengan pesan "Coverage yang ditolak tidak boleh otomatis dipindahkan ke pasien tanpa policy kontrak." dan versi kalkulasi baru tidak dibuat.

> **FR-BKC-031 — Kode anomali ikut tersimpan pada versi kalkulasi**
>
> Perhitungan yang disimpan membawa kode anomalinya, sehingga alasan sebuah tagihan jatuh ke pasien dapat ditelusuri berbulan-bulan kemudian.
>
> **Contoh:** Versi kalkulasi nomor 7 pada tagihan tertentu menyimpan `anomalyCodes` berisi `POLICY_INACTIVE`, dan itu terbaca kembali saat sengketa klaim dibuka.

### `EPIC BKC-08` — Gerbang PPN rawat jalan versus rawat inap

**Tujuan.** PPN obat dan alat kesehatan dikenakan pada kunjungan yang seharusnya, dan dibebaskan pada yang seharusnya.

**Disposisi backend:** `EXTEND` — basis pajak sudah ada; syarat jenis kunjungan yang belum ada.

> **FR-BKC-032 — Kunjungan rawat inap dibebaskan PPN sepenuhnya**
>
> Tagihan yang jenis kunjungannya rawat inap tidak memuat satu rupiah pun PPN, apa pun cara pembayarannya.
>
> **Contoh:** Pasien rawat inap menerima obat Rp 1.000.000 dan biaya kamar Rp 2.000.000 dengan tarif PPN 11%. Total tagihan Rp 3.000.000, bukan Rp 3.110.000.

> **FR-BKC-033 — Kunjungan rawat jalan dan IGD tetap dikenai PPN**
>
> Tagihan rawat jalan dan gawat darurat tetap memuat PPN atas obat dan alat kesehatan.
>
> **Contoh:** Pasien IGD menerima alat kesehatan Rp 200.000. PPN Rp 22.000 tetap dikenakan.

> **FR-BKC-034 — Basis pajak tetap hanya obat dan alat kesehatan**
>
> Jasa konsultasi, tindakan, biaya administrasi, dan biaya kamar tidak pernah masuk basis pajak.
>
> **Contoh:** Tagihan rawat jalan berisi Konsultasi Rp 100.000 dan Amoksisilin Rp 50.000 dengan tarif 11%. PPN yang ditagihkan Rp 5.500, bukan Rp 16.500.

> **FR-BKC-035 — Jenis kunjungan diambil dari tagihan, bukan dari pendaftaran terkini**
>
> Basis pajak sebuah tagihan tidak berubah bila jenis kunjungan dibetulkan setelah tagihan dibuka.
>
> **Contoh:** Tagihan dibuka sebagai rawat jalan pada 1 September. Pada 15 September pendaftaran membetulkannya menjadi rawat inap. Tagihan itu tetap dikenai PPN; membebaskannya menuntut tagihan dibatalkan dan dibuka ulang.

> **FR-BKC-036 — Jenis kunjungan yang tidak dikenal tetap dikenai PPN**
>
> Nilai jenis kunjungan yang kosong atau di luar daftar yang dikenal tidak membebaskan pajak dan tidak menghentikan perhitungan.
>
> **Contoh:** Tagihan dengan jenis kunjungan berisi teks yang tidak dikenal tetap dikenai PPN atas obatnya, dan perhitungannya tetap berhasil.

> **FR-BKC-037 — PPN mengikuti nasib barang yang dipajakinya**
>
> Pajak atas obat yang ditanggung asuransi ikut ditanggung asuransi; pajak atas obat yang tidak ditanggung menjadi tanggungan pasien.
>
> **Contoh:** Rawat jalan berasuransi menerima Amoksisilin Rp 100.000 (ditanggung penuh) dan Vitamin C Rp 50.000 (tidak ditanggung), tarif 11%. Pajak Asuransi Rp 11.000, Pajak Mandiri Rp 5.500.

**Catatan disposisi khusus untuk `FR-BKC-037`:** disposisinya `EXISTING / REUSE`. Mekanismenya sudah lengkap di kode; yang diperlukan adalah memastikan tarif PPN yang aktif memakai pembagian berimbang. Ini tindakan data, bukan pekerjaan kode (`BKC-DES-020`).

## B11. Model status yang diusulkan

**Tidak ada status baru.** Amendment ini tidak menambah, menghapus, maupun mengubah satu pun status tagihan, tender, penyelesaian, shift, atau pengecualian finansial. Rinciannya ada di [`contracts/state-transition-matrix.md`](contracts/state-transition-matrix.md) bagian amendment 4 September 2026.

Yang berubah adalah kosakata **status turunan per baris** yang dibaca layar: `belum_terverifikasi` dihapus, `anomali_data` lahir, dan `penjamin` serta `tunai` tidak berubah.

## B12. Sasaran arsitektur

| Yang dipakai ulang | Yang diperluas | Yang baru |
| --- | --- | --- |
| `CalculateCoveredAmount` dan `Matches` — tidak berubah satu baris pun | `RegistrationBillingCoverageAdapter.ResolveAsync` — empat cabang keputusannya | `BillingCoverageAnomaly` — record kontrak internal, bukan tabel |
| `BilCalculationVersion.BreakdownSnapshot` sebagai tempat rincian | `BillingCoverageDecision` dan `BillingCoverageComponentOutcome` — tambah nominal anomali | `IsTaxExemptServiceType` — method pembantu statis |
| `BilInvoice.ServiceType` sebagai snapshot jenis kunjungan | `ApplyInvoiceTax` — tambah satu parameter jenis kunjungan | Empat kode anomali beserta kalimatnya |
| `TaxComponentCoverable` — mekanisme pembagian pajak per penanggung | `ApplyCoverageWaterfall` — penjaga `BIL-VAL-036` diretarget | — |
| `BillingCoverageComponentOutcome` dari `BE-BKC-FIX-003` | `BuildCoverageComponents` — jenis komponen pajak non-item dipisah | — |

**Tidak ada tabel baru, tidak ada kolom baru, tidak ada migration** (`BKC-DES-020`).

## B13. Sasaran kemampuan API

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing-invoices`

**Tidak ada endpoint baru.** Seluruh kemampuan amendment ini terbawa oleh dua endpoint yang sudah ada, dengan payload yang bertambah field.

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{id:guid}/calculation-preview` | Menghitung ulang tagihan tanpa menyimpannya | `BillingInvoice : Read` | Path `id` | `ApiResponse<CalculationResponse>` | `EPIC BKC-06`, `BKC-07`, `BKC-08` | Diimplementasikan — payload bertambah field |
| `POST` | `/{id:guid}/recalculate` | Menghitung ulang dan menyimpannya sebagai versi baru | `BillingInvoice : Update` | Path `id` + `RecalculateInvoiceRequest` | `ApiResponse<CalculationResponse>` | `EPIC BKC-06`, `BKC-07`, `BKC-08` | Diimplementasikan — payload bertambah field |

Daftar field yang bertambah beserta artinya ada di [`contracts/api-contract.md`](contracts/api-contract.md) bagian amendment 4 September 2026; tidak diulang di sini supaya tidak ada dua sumber yang dapat menyimpang.

## B14. Matriks kewenangan

| Peran rumah sakit | Kemampuan | String hak akses |
| --- | --- | --- |
| Kasir | Melihat ringkasan pembayaran, peringatan anomali, dan badge per baris | `[AccessPermission("BillingInvoice", "Read")]` |
| Supervisor Billing | Memicu perhitungan ulang yang disimpan | `[AccessPermission("BillingInvoice", "Update")]` |
| Admin Data Induk | Mengubah tarif PPN termasuk cara pembagiannya | `[AccessPermission("TaxRule", "Update")]` |
| Admin Data Induk Asuransi | Mengubah aturan tanggungan | `[AccessPermission("InsuranceCoverageRule", "Update")]` |

**Tidak ada Resource maupun Action baru.** Rincian audit dan kewenangan yang tidak dapat dijaga mesin hak akses ada di [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md).

## B15. Batas integrasi dan billing

Yang **MUST NOT** dibuat sendiri oleh modul ini:

1. **Verifikasi kelayakan penjamin.** Modul ini membaca hasilnya, tidak pernah menetapkannya. Kesalahan dilaporkan sebagai anomali, bukan diperbaiki diam-diam.
2. **Penetapan jenis kunjungan.** Modul ini membaca snapshot yang dibuatnya sendiri saat tagihan dibuka, dari nilai yang ditetapkan Registrasi.
3. **Penentuan tarif dan cara pembagian PPN.** Diambil apa adanya dari data induk. Tidak boleh ada nilai yang ditulis tetap di kode.
4. **Pelaporan PPN ke otoritas pajak.** Tetap milik proses keuangan di luar modul ini.
5. **Pengajuan klaim elektronik ke perusahaan asuransi.** Tetap milik modul asuransi yang belum aktif.

## B16. Guardrail regulasi

| Kewajiban | Yang mengikat MVP ini |
| --- | --- |
| Pembebasan PPN atas jasa pelayanan kesehatan | Basis pajak tetap dibatasi ke obat dan alat kesehatan. Pembebasan tambahan untuk rawat inap adalah keputusan pemilik produk yang tercatat pada `BKC-DEC-078`, dan dilaksanakan lewat data serta gerbang jenis kunjungan — bukan lewat tafsir hukum yang dikarang modul ini |
| Ketertelusuran perhitungan | Setiap versi kalkulasi yang disimpan bersifat tidak dapat diubah. Perubahan aturan berlaku ke depan; tagihan yang sudah difinalisasi tetap memakai angka lamanya |
| Kerahasiaan data pasien | Nomor polis, nomor anggota, nomor rekam medis, nama pasien, dan uraian layanan **MUST NOT** masuk catatan sistem mana pun. Yang boleh dicatat hanya nomor tagihan dan kode anomali |
| Kerahasiaan kesepakatan komersial | Kode aturan, nama aturan, instruksi persetujuan, dan instruksi penagihan milik perusahaan asuransi **MUST NOT** keluar ke kontrak yang dibaca layar |
| Bukti perubahan tarif pajak | Perubahan cara pembagian PPN tercatat pada catatan perubahan data induk beserta nilai sebelum dan sesudahnya |

## B17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-010` | Perhitungan tagihan tetap berjalan dalam satu transaksi dengan tingkat isolasi dan kunci yang sudah dipakai sekarang; amendment ini tidak menambah titik tulis apa pun |
| `NFR-011` | Jumlah tanggungan per baris **MUST** sama persis dengan total tanggungan, tanpa toleransi pembulatan. Pelanggarannya menghentikan perhitungan |
| `NFR-012` | Field baru **MUST** dapat dibaca dari versi kalkulasi lama tanpa galat, dengan nilai bawaan nol atau kosong |
| `NFR-013` | Perubahan aturan **MUST NOT** mengubah angka pada versi kalkulasi yang sudah terkunci; tidak ada penulisan ulang data lama |
| `NFR-014` | Kesalahan konfigurasi tarif pajak — dua tarif aktif bersamaan — **MUST** tetap terdeteksi pada kunjungan rawat inap, walaupun pajaknya dibebaskan |
| `NFR-015` | Perubahan yang mengubah arti sebuah field tanpa mengubah namanya **MUST** disosialisasikan ke konsumen sebelum diberlakukan |
| `NFR-016` | Slice ini **MUST NOT** dinyatakan selesai tanpa `dotnet build` yang benar-benar dijalankan dan lulus — dua task ad-hoc yang mendahuluinya berstatus terblokir untuk pemeriksaan otomatis |

## B18. Skenario UAT

> **UAT-11 — Tagihan asuransi normal menjumlah persis**
>
> **Kondisi awal:** kunjungan rawat jalan pasien asuransi dengan data penjamin lengkap. Tiga item: Konsultasi Rp 100.000 (aturan menanggung 100%), Fisioterapi Rp 300.000 (aturan menanggung 80%), Vitamin C Rp 25.000 (tanpa aturan).
>
> **Langkah:** kasir membuka Menu Pembayaran.
>
> **Hasil yang diharapkan:** Subtotal Mandiri Rp 85.000, Subtotal Asuransi Rp 340.000, Total Tagihan Rp 425.000. Baris "Penjamin Belum Terverifikasi" **tidak ada** di layar. Badge per baris berturut-turut Penjamin, Penjamin, Tunai.

> **UAT-12 — Aturan berpenanda persetujuan tidak lagi menahan (jalur yang dulu gagal)**
>
> **Kondisi awal:** kunjungan yang sama, tetapi aturan Fisioterapi juga menandai butuh surat jaminan dan mencantumkan batas Rp 500.000 per bulan.
>
> **Langkah:** kasir membuka Menu Pembayaran.
>
> **Hasil yang diharapkan:** hasilnya **sama persis** dengan `UAT-11`. Sebelum perubahan, Rp 300.000 tertahan di bucket menggantung dan Subtotal Asuransi hanya Rp 100.000.

> **UAT-13 — Data penjamin bermasalah, pembayaran tetap dapat diselesaikan**
>
> **Kondisi awal:** kunjungan asuransi dengan biaya coverable Rp 440.000, tetapi kelayakan penjamin belum dicentang petugas pendaftaran.
>
> **Langkah:** kasir membuka Menu Pembayaran, lalu menerima pembayaran tunai penuh.
>
> **Hasil yang diharapkan:** peringatan kuning tampil di atas ringkasan, menyebut Rp 440.000 dan mengarahkan ke Registrasi. Subtotal Mandiri Rp 440.000, Subtotal Asuransi Rp 0. Tombol pembayaran tetap aktif dan pembayaran selesai. Tidak ada baris bernama anomali di dalam ringkasan.

> **UAT-14 — Perhitungan dihentikan ketika tanggungan ditolak tanpa alasan tercatat (jalur gagal)**
>
> **Kondisi awal:** kondisi yang disimulasikan, ketika penilaian penjamin menghasilkan status ditolak tetapi nominal anomalinya tidak terisi.
>
> **Langkah:** perhitungan ulang dijalankan.
>
> **Hasil yang diharapkan:** perhitungan gagal dengan pesan "Coverage yang ditolak tidak boleh otomatis dipindahkan ke pasien tanpa policy kontrak." Versi kalkulasi baru tidak dibuat, dan angka tagihan tidak berubah.

> **UAT-15 — Rawat inap bebas PPN**
>
> **Kondisi awal:** tagihan rawat inap berisi obat Rp 1.000.000 dan biaya kamar Rp 2.000.000. Tarif PPN aktif 11% dengan pembagian berimbang.
>
> **Langkah:** kasir membuka Menu Pembayaran.
>
> **Hasil yang diharapkan:** Total Tagihan Rp 3.000.000. Pajak Mandiri dan Pajak Asuransi keduanya Rp 0. Tidak ada baris pajak pada rincian item.

> **UAT-16 — Rawat jalan dan IGD tetap kena PPN**
>
> **Kondisi awal:** dua tagihan berisi obat Rp 1.000.000 yang sama, satu rawat jalan dan satu IGD.
>
> **Langkah:** kasir membuka keduanya.
>
> **Hasil yang diharapkan:** keduanya memuat PPN Rp 110.000 dan bertotal Rp 1.110.000.

> **UAT-17 — PPN obat yang tidak ditanggung dibebankan ke pasien (jalur yang mudah salah)**
>
> **Kondisi awal:** rawat jalan pasien asuransi, Amoksisilin Rp 100.000 ditanggung penuh, Vitamin C Rp 50.000 tanpa aturan. Tarif 11%.
>
> **Langkah:** kasir membuka Menu Pembayaran.
>
> **Hasil yang diharapkan:** Pajak Asuransi Rp 11.000 dan Pajak Mandiri Rp 5.500. **Bukan** Pajak Asuransi Rp 16.500 — itu perilaku yang salah dan menandakan tarif pajak aktif tidak memakai pembagian berimbang.

> **UAT-18 — Selisih yang tidak boleh ditagihkan tampil dengan namanya sendiri**
>
> **Kondisi awal:** aturan menanggung 70% dengan penanda kelebihan tidak boleh ditagihkan ke pasien. Tindakan Rp 100.000.
>
> **Langkah:** kasir membuka Menu Pembayaran.
>
> **Hasil yang diharapkan:** Subtotal Asuransi Rp 70.000, Subtotal Mandiri Rp 0, dan baris "Selisih Tidak Ditagihkan (kontrak penjamin)" berisi Rp 30.000. Baris itu **tidak muncul** pada tagihan lain yang selisihnya nol.

> **UAT-19 — Tagihan lama tidak berubah (jalur regresi)**
>
> **Kondisi awal:** tagihan rawat inap yang sudah difinalisasi sebelum perubahan ini, memuat PPN atas obatnya.
>
> **Langkah:** kasir mencetak ulang Kwitansi dan Struk Pasien.
>
> **Hasil yang diharapkan:** angkanya sama persis dengan cetakan sebelumnya, termasuk PPN lamanya. Tidak ada perhitungan ulang yang berjalan.

> **UAT-20 — Pasien tunai tidak terpengaruh sama sekali (jalur regresi)**
>
> **Kondisi awal:** kunjungan rawat jalan pasien tunai berisi tiga item dan satu obat.
>
> **Langkah:** kasir membuka Menu Pembayaran.
>
> **Hasil yang diharapkan:** seluruh baris berbadge "Tunai", Subtotal Asuransi Rp 0, tidak ada peringatan anomali, dan totalnya sama persis dengan sebelum perubahan.

## B19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Aturan yang menandai butuh persetujuan tidak lagi menahan tanggungan | `UAT-12`, `BIL-AT-036` |
| Aturan yang mencantumkan limit bulanan tidak lagi menahan tanggungan | `UAT-12`, `BIL-AT-037` |
| Batas per kunjungan masih berlaku | `BIL-AT-052`, regresi pada `testing/acceptance-test-matrix.md` |
| Tarif tanpa aturan menjadi tanggungan pasien, bukan bucket menggantung | `UAT-11`, `BIL-AT-038` |
| Baris "Penjamin Belum Terverifikasi" tidak ada di markup Menu Pembayaran | `UAT-11`, `BIL-AT-053` |
| Ringkasan Pembayaran menjumlah persis ke Total Tagihan | `UAT-11`, `BIL-AT-052` |
| Empat kode anomali dihasilkan pada keadaan yang benar | `UAT-13`, `BIL-AT-041`, `BIL-AT-042` |
| Tagihan beranomali tetap dapat dibayar sampai selesai | `UAT-13`, `BIL-AT-054` |
| Tanggungan yang ditolak tidak dapat diam-diam menjadi tagihan pasien | `UAT-14`, `BIL-AT-043` |
| Kunjungan rawat inap tidak memuat satu rupiah pun PPN | `UAT-15`, `BIL-AT-044` |
| Kunjungan rawat jalan dan IGD tetap memuat PPN | `UAT-16`, `BIL-AT-045`, `BIL-AT-046` |
| PPN obat yang tidak ditanggung dibebankan ke pasien | `UAT-17`, `BIL-AT-048` |
| Selisih yang tidak boleh ditagihkan punya baris bernama sendiri | `UAT-18`, `BIL-AT-040` |
| Tagihan yang sudah difinalisasi tidak berubah angkanya | `UAT-19`, regresi pada `testing/acceptance-test-matrix.md` |
| Pasien tunai tidak terpengaruh | `UAT-20`, regresi pada `testing/acceptance-test-matrix.md` |
| Versi kalkulasi lama tetap dapat dibaca tanpa galat | `BIL-AT-050` |
| Kunci komponen pajak non-item tidak dapat bertabrakan | `BIL-AT-049` |
| Nilai cara pembagian PPN pada tarif aktif sudah diperiksa dan hasilnya dicatat | Rencana data master awal pada `02-backend-architecture.md`; bukti keluar pada `testing/acceptance-test-matrix.md` butir 4 |
| `dotnet build` benar-benar dijalankan dan lulus | Bukti keluar pada `testing/acceptance-test-matrix.md` butir 1 |
| Dampak penurunan total tagihan rawat inap sudah dihitung sebelum diberlakukan | Bukti keluar pada `testing/acceptance-test-matrix.md` butir 3; lihat `BKC-OQ-085` |

## B20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-7` | Fondasi penilaian penjamin: cabang `ResolveAsync` diubah, `BillingCoverageAnomaly` lahir, nominal anomali ditambahkan pada tiga kontrak, `BIL-VAL-035`–`037`, penjaga diretarget, jenis komponen pajak non-item dipisah, versi kontrak `0.4` → `0.6` (`EPIC BKC-06`, `EPIC BKC-07` backend) | Approval `BKC-DES-010`–`017` beserta wewenang tulis backend. **Working tree `BE-BKC-FIX-003` MUST di-commit atau dikonfirmasi lebih dulu** — bila perubahan itu dibuang, cakupan gelombang ini melebar jauh |
| `MVP-8` | Gerbang PPN: `IsTaxExemptServiceType`, parameter jenis kunjungan pada `ApplyInvoiceTax`, `BIL-VAL-038`–`039` (`EPIC BKC-08`) | Dapat berjalan bersamaan dengan `MVP-7` — keduanya menyentuh berkas yang sama tetapi method yang berbeda. **Dampak penurunan total tagihan rawat inap MUST dihitung lebih dulu** (`BKC-OQ-085`) |
| `MVP-9` | Frontend Menu Pembayaran: baris "Penjamin Belum Terverifikasi" dihapus, peringatan anomali, badge `anomali_data`, baris selisih tidak ditagihkan (`EPIC BKC-06`, `BKC-07` frontend) | `MVP-7` **selesai dan terverifikasi hidup**. Dideploy lebih dulu akan menampilkan Rp 0 pada kolom anomali sehingga masalah data terlihat seolah tidak ada |
| `MVP-10` | Tindakan data induk: verifikasi dan koreksi cara pembagian pada tarif PPN aktif, pemeriksaan hanya ada satu tarif aktif (`FR-BKC-037`) | Dapat dikerjakan kapan saja, **tidak** memblokir gelombang lain, tetapi `UAT-17` tidak dapat lulus sebelum ini selesai. Bukan pekerjaan kode |
| `POST-MVP` | Keputusan PPN untuk pemeriksaan kesehatan berkala dan konsultasi jarak jauh; peringatan otomatis bila cara pembagian PPN salah; pemblokiran finalisasi tagihan beranomali; penyatuan kamus data `erd/` dan `data/`; keterangan pada layar master data bahwa empat kolom tidak lagi menahan perhitungan | Di luar cakupan rilis ini; masing-masing butuh keputusan pemiliknya sendiri |

Epic berstatus `OPEN DECISION`: **tidak ada**. Seluruh functional requirement `FR-BKC-021`–`FR-BKC-037` berdisposisi `EXISTING / REUSE`, `EXTEND`, atau `MISSING / NEW`.

### Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | --- | :---: |
| `BKC-OQ-082` | Approval `BKC-DES-010`–`020`, khususnya `BKC-DES-011` (nominal anomali jatuh ke pasien, bukan menjadi bucket ketiga) | Product/Domain Owner + Finance/AR | Bila kelak diminta bucket ketiga, seluruh perhitungan porsi pasien dan penjaga `BIL-VAL-036` harus dirancang ulang | **Ya** — blueprint tidak boleh diteruskan ke `/plan-module-delivery` sebelum dijawab |
| `BKC-OQ-083` | Apakah pemeriksaan kesehatan berkala (`MCU`), konsultasi jarak jauh (`TELEMEDICINE`), dan penjualan bebas (`OTC`) dikenai PPN atau dibebaskan | Product/Domain Owner + Finance/Tax | Ketiganya akan dikenai PPN mengikuti bentuk daftar bebas pajak. Bila jawabannya dibebaskan, pasien telanjur dipungut pajak dan harus dikembalikan | **Ya** bila rumah sakit benar-benar memakai ketiga jenis kunjungan itu; tidak bila belum dipakai |
| `BKC-OQ-084` | ~~Apakah tafsir "residual mengikuti perilaku saat ini" sudah benar~~ **DITUTUP 4 September 2026 oleh `BKC-DEC-080`** | Product/Domain Owner | Jawabannya **membalik** tafsir agent: residual dengan `IsAllowExcessPaymentByPatient = false` **bukan** berhenti sebagai selisih tanpa tindak lanjut, melainkan ditanggung rumah sakit lewat write-off. Desainnya ada pada Bagian C dokumen ini beserta `BKC-DES-021`–`025` | ~~Ya~~ **Tidak lagi — ditutup** |
| `BKC-OQ-085` | Berapa besar penurunan total tagihan rawat inap yang masih berjalan, dan bagaimana kelebihan bayar yang timbul diselesaikan | Billing/Finance/AR Owner | Tagihan rawat inap yang sudah menerima deposit sebesar total lama akan menjadi kelebihan bayar. Tanpa hitungan awal, jumlah kasus yang perlu dikembalikan tidak diketahui | **Ya** untuk `MVP-8`; gelombang lain tidak terpengaruh |
| `BKC-OQ-086` | Apakah finalisasi tagihan diblokir ketika masih ada anomali data | Billing Owner | Desain ini memilih **tidak memblokir**, hanya memperingatkan. Bila jawabannya memblokir, satu aturan transisi status baru harus ditambahkan | Tidak — pilihan bawaan aman dan dapat diubah kemudian |
| `BKC-OQ-087` | Bagaimana menjelaskan selisih antara badge penasihat di layar entri (yang masih menampilkan "butuh persetujuan") dan perhitungan tagihan (yang sudah menanggungnya penuh) | Product/Domain Owner | Petugas melihat dua keterangan yang terlihat bertentangan untuk item yang sama | Tidak — tidak menghentikan implementasi, tetapi wajib dijelaskan sebelum pengguna dilatih |
| `BKC-OQ-088` | Apakah salah konfigurasi cara pembagian PPN perlu peringatan otomatis, atau cukup pemeriksaan manual sekali | Finance/Tax Owner | Tanpa peringatan, seluruh PPN rawat jalan dapat salah dialokasikan tanpa ada yang memergoki, karena angkanya tetap menjumlah | Tidak — ditunda ke `POST-MVP` |
| `BKC-OQ-089` | Apakah kamus data dipindahkan seluruhnya dari `erd/` ke `data/`, atau keduanya dipertahankan | Pemilik blueprint | Selama keduanya ada, pembaca harus tahu mana yang memuat apa. Pemindahan menyentuh rujukan pada belasan berkas | Tidak — milik `/manage-module-blueprint`, bukan pass desain |
| `BKC-OQ-090` | Apakah perubahan `BE-BKC-FIX-003` dan `FE-BKC-FIX-008` yang belum di-commit akan dipertahankan | Pemilik repository | Bila dibuang, `MVP-7` melebar dari "menyempurnakan" menjadi "membangun dari nol", dan seluruh estimasi gelombang berubah | **Ya** — kondisi awal `MVP-7` tidak dapat ditetapkan tanpa jawaban ini |
| `BKC-OQ-091` | Ratifikasi `blueprint_shape: SINGLE` | Pemilik modul | Bentuk blueprint tidak pernah tercatat eksplisit pada manifest, walaupun strukturnya sudah `SINGLE` sejak revision `0.2`. Tanpa ratifikasi, tidak ada catatan siapa yang memutuskannya | Tidak — struktur berkas tidak berubah; ini pencatatan keputusan yang selama ini implisit |
| `BKC-OQ-092` | Wewenang tulis backend dan frontend (task mode, branch) | Pengguna | Prasyarat prosedural sebelum `/build-module-backend` dan `/build-module-frontend` dijalankan | Ya, untuk implementasi — bukan untuk desain |

**Status dokumen ini: `draft`.** Dari lima pertanyaan terbuka yang semula bertanda memblokir, tiga sudah ditutup 4 September 2026: `BKC-OQ-082` oleh `BKC-DEC-084`+`BKC-DEC-085`, `BKC-OQ-084` oleh `BKC-DEC-080` (desainnya pada Bagian C), dan `BKC-OQ-090` oleh `BKC-DEC-081`. **Yang masih memblokir: `BKC-OQ-083` dan `BKC-OQ-085`.**

---

# Bagian C — Amendment lanjutan 4 September 2026: penanggungan selisih yang tidak dapat ditagihkan

## C1. Identitas dokumen amendment

| Aspek | Nilai |
| --- | --- |
| Revisi blueprint | `0.8` |
| Status | **draft** — approval tetap tindakan manusia |
| Keputusan bisnis dasar | **`BKC-DEC-080`** (`approved` Product/Domain Owner, 4 September 2026), `BKC-DEC-036` (`approved` 20 Agustus 2026) |
| Keputusan arsitektur | `BKC-DES-021`–`BKC-DES-025` (**draft**) |
| Pertanyaan terbuka yang ditutup | `BKC-OQ-084` |
| Pertanyaan terbuka baru | `BKC-OQ-093`, `BKC-OQ-094` — keduanya **tidak memblokir** |

## C2. Masalah produk yang diselesaikan

Sebagian kontrak kerja sama rumah sakit dengan perusahaan asuransi memuat dua ketentuan sekaligus: perusahaan hanya menanggung sebagian biaya, **dan** rumah sakit dilarang menagihkan selisihnya kepada pasien. Selisih itu bukan milik siapa pun.

Revisi `0.7` sudah menempatkan selisih tersebut di luar tagihan pasien — pasien tidak pernah diminta membayarnya. Yang belum ada adalah **kelanjutannya**: nominalnya berhenti sebagai angka di layar, tanpa seorang pun yang diminta bertindak dan tanpa catatan resmi bahwa rumah sakit yang menanggungnya. Bagi pemeriksa, uang itu hilang tanpa keputusan bernama pelaku.

`BKC-DEC-080` menutup celah itu: selisih tersebut **ditanggung rumah sakit lewat jalur Pengecualian Finansial/write-off yang sudah berjalan sejak baseline** (`BKC-DEC-036`) — jalur yang sudah punya pengajuan beralasan, persetujuan orang kedua, jejak audit, dan pembatalan lewat catatan koreksi.

## C3. Batas MVP amendment ini

**Titik mulai:** tagihan dihitung dan memuat selisih perhitungan yang menurut kontrak penjamin tidak dapat ditagihkan ke pasien.

**Titik akhir:** selisih itu tercatat ditanggung rumah sakit lewat kasus penanggungan yang diajukan petugas keuangan, disetujui orang kedua, dan dapat dibatalkan bila keliru — **tanpa** mengubah satu rupiah pun pada tagihan pasien.

**Di dalam batas:** satu ember nominal baru pada perhitungan tagihan, satu kategori pada kasus penanggungan, plafon dan aturan penolakan yang menyertainya, serta tampilan nominal sisa pada layar Pengecualian Finansial tagihan yang sedang dibuka.

**Di luar batas, beserta penggantinya selama MVP berjalan:**

| Yang ditunda | Alasan | Pengganti selama MVP |
| --- | --- | --- |
| Daftar kerja lintas tagihan berisi seluruh selisih yang menunggu penanggungan | Menuntut endpoint pencarian baru beserta hak aksesnya; tidak diminta `BKC-DEC-080` | Nominalnya terlihat pada layar Pengecualian Finansial tagihan yang dibuka. Untuk sapuan berkala, Finance memakai laporan tagihan yang sudah ada |
| Pemblokiran finalisasi tagihan selama selisih belum ditanggung | Aturan bisnis baru yang tidak diminta; mengikuti preseden `BKC-OQ-086` | Peringatan pada layar finalisasi, tanpa menahan. Diajukan sebagai `BKC-OQ-094` |
| Perutean selisih dari aturan yang menyatakan "tidak ditanggung" (bukan sisa perhitungan) | `BKC-DEC-080` menyebut hanya sisa perhitungan | Perilaku revisi `0.7` dipertahankan apa adanya. Diajukan sebagai `BKC-OQ-093` |
| Pengajuan penanggungan secara otomatis oleh sistem | `BKC-DES-023` — memeriksa dua orang atas uang rumah sakit akan runtuh | Sistem menghitung, menandai, dan menyiapkan nominalnya; petugas keuangan yang mengajukan |

## C4. Pelaku sasaran

| Pelaku | Yang dilakukan pada amendment ini |
| --- | --- |
| Kasir | **Tidak melakukan apa pun yang baru.** Angka yang dilihat dan ditagihkannya tidak berubah sama sekali |
| Petugas keuangan | Membaca nominal selisih yang sudah disiapkan sistem, memeriksanya terhadap kontrak, lalu mengajukan penanggungan beserta alasannya |
| Atasan keuangan | Menyetujui atau menolak pengajuan; membatalkan penanggungan yang keliru |
| Pemilik data asuransi | Meninjau ulang aturan yang selisihnya terasa keliru, sebelum penanggungan diajukan |

## C5. Epic dan functional requirement

### `EPIC BKC-09` — Penanggungan selisih yang tidak dapat ditagihkan

**Tujuan.** Setiap rupiah yang tidak dapat ditagihkan kepada siapa pun berakhir sebagai keputusan bernama pelaku, bukan sebagai angka yang berhenti di layar.

**Disposisi backend:** `EXTEND` — mesin perhitungan, kasus penanggungan, alur pengajuan, persetujuan, dan pembatalan **seluruhnya sudah ada**; yang bertambah adalah satu ember nominal, satu kategori, dan plafon yang menyertainya. Kemampuan asal pada `01-existing-capability-map.md`: kemampuan Pengecualian Finansial (write-off/adjustment/refund) berstatus `READY TO REUSE`, dan mesin coverage berstatus `EXTEND`.

> **FR-BKC-038 — Selisih yang tidak dapat ditagihkan punya embernya sendiri**
>
> Sisa perhitungan tanggungan yang menurut kontrak penjamin tidak boleh ditagihkan ke pasien dicatat sebagai nominal tersendiri, terpisah dari nominal masalah data pendaftaran dan terpisah dari sisa aturan "tidak ditanggung".
>
> **Contoh:** Fisioterapi Rp 300.000 ditanggung 80% dengan penanda selisih tidak boleh ditagihkan. Rp 240.000 masuk Subtotal Asuransi, Rp 0 masuk Subtotal Mandiri, dan Rp 60.000 masuk ember selisih tidak dapat ditagihkan.
>
> Disposisi: `MISSING / NEW`.

> **FR-BKC-039 — Tagihan pasien tidak bergeser satu rupiah pun**
>
> Munculnya ember baru ini tidak mengubah Total Tagihan, Subtotal Mandiri, Subtotal Asuransi, maupun nominal yang ditagihkan kasir.
>
> **Contoh:** tagihan Rp 425.000 dengan selisih Rp 60.000 tetap menagih Rp 25.000 ke pasien, sama persis seperti sebelum amendment ini — nominalnya memang sudah dikeluarkan dari tagihan pasien sejak revisi sebelumnya.
>
> Disposisi: `EXISTING / REUSE`.

> **FR-BKC-040 — Nominal selisih disiapkan sistem, pengajuannya oleh manusia**
>
> Layar Pengecualian Finansial menampilkan sisa selisih yang belum ditanggung untuk tagihan yang sedang dibuka, dan mengisikannya ke formulir pengajuan. Sistem **tidak** mengajukan sendiri.
>
> **Contoh:** petugas membuka tagihan itu, melihat "Selisih tidak dapat ditagihkan yang belum ditanggung: Rp 60.000", menekan tombol pengajuan, nominalnya sudah terisi, dan ia tinggal menuliskan alasannya.
>
> Disposisi: `MISSING / NEW`.

> **FR-BKC-041 — Pengajuan diperiksa dua orang**
>
> Pengaju tidak dapat menyetujui pengajuannya sendiri, sama seperti seluruh penanggungan yang sudah berjalan.
>
> **Contoh:** petugas A mengajukan Rp 60.000; ketika A mencoba menyetujuinya sendiri, sistem menolak. Atasan B yang menyetujuinya.
>
> Disposisi: `EXISTING / REUSE`.

> **FR-BKC-042 — Nominal penanggungan dibatasi selisihnya sendiri**
>
> Nominal yang diajukan tidak boleh melebihi sisa selisih pada tagihan itu — bukan dibatasi sisa tagihan pasien.
>
> **Contoh:** pada tagihan dengan selisih Rp 60.000 dan sisa tagihan pasien Rp 25.000, pengajuan Rp 60.000 **diterima** walaupun melebihi Rp 25.000, sedangkan pengajuan Rp 75.000 **ditolak** walaupun masih di bawah Total Tagihan Rp 425.000.
>
> Disposisi: `MISSING / NEW`.

> **FR-BKC-043 — Penanggungan tidak mengurangi tagihan pasien dan tidak melunaskannya**
>
> Setelah penanggungan disetujui, sisa tagihan pasien tetap seperti semula dan status tagihan tidak berpindah menjadi "diselesaikan lewat penanggungan".
>
> **Contoh:** sisa tagihan pasien tetap Rp 25.000 sesudah Rp 60.000 ditanggung. Kwitansi pasien tidak menyebut angka Rp 60.000 sama sekali.
>
> Disposisi: `MISSING / NEW`.

> **FR-BKC-044 — Penanggungan yang keliru dapat dibatalkan**
>
> Pembatalan menghasilkan catatan koreksi, membuka kembali selisihnya untuk diajukan ulang, dan tidak menghapus riwayat.
>
> **Contoh:** Rp 60.000 yang ternyata salah kontrak dibatalkan; sisa selisih kembali menjadi Rp 60.000, sisa tagihan pasien **tetap** Rp 25.000, dan kedua catatan tetap terbaca di riwayat.
>
> Disposisi: `EXISTING / REUSE`.

## C6. Skenario UAT

| ID | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- |
| `UAT-21` | Berhasil | Kasir membuka tagihan yang memuat selisih tidak dapat ditagihkan | Ringkasan pembayaran menjumlah persis ke Total Tagihan; nominal yang ditagihkan **tidak** memuat selisih itu; kasir dapat menerima pembayaran sampai tuntas tanpa hambatan |
| `UAT-22` | Berhasil | Petugas keuangan mengajukan penanggungan atas selisih itu, atasannya menyetujui | Sisa selisih menjadi nol; sisa tagihan pasien **tidak berubah**; status tagihan **tidak berpindah**; catatan penanggungan terbaca beserta nama pengaju, penyetuju, dan alasannya |
| `UAT-23` | Berhasil | Penanggungan yang keliru dibatalkan | Selisih terbuka kembali dan dapat diajukan ulang; sisa tagihan pasien **tetap** tidak berubah; riwayat kedua catatan tetap ada |
| `UAT-24` | Gagal | Petugas mengajukan nominal yang melebihi sisa selisih | Ditolak beserta pesan yang menyebut selisihnya, bukan menyebut tagihan pasien. Tidak ada catatan yang terbentuk |
| `UAT-25` | Gagal | Pengaju mencoba menyetujui pengajuannya sendiri | Ditolak beserta pesan pemeriksaan dua orang. Pengajuan tetap menunggu persetujuan |
| `UAT-26` | Gagal | Petugas mengajukan penanggungan sambil menandainya sebagai pelunasan penuh tagihan | Ditolak. Tagihan pasien tidak boleh dinyatakan lunas oleh penanggungan atas uang yang bukan tagihan pasien |
| `UAT-27` | Gagal | Tidak ada seorang pun yang mengajukan penanggungan selama sebulan | Tagihan tetap dapat difinalkan; peringatan muncul pada layar finalisasi. **Ini keadaan yang dipilih sengaja** (`BKC-OQ-094`), bukan kelalaian desain |

## C7. Definition of Done

| Butir | Bukti |
| --- | --- |
| Selisih yang tidak dapat ditagihkan tercatat pada ember tersendiri, bukan bercampur dengan nominal masalah data | `UAT-21`, `BIL-AT-055` |
| Cabang "kontrak mengizinkan" tetap jatuh ke pasien seperti semula | `BIL-AT-056` |
| Total Tagihan dan nominal yang ditagihkan kasir tidak bergeser satu rupiah pun | `UAT-21`, regresi pada `testing/acceptance-test-matrix.md` |
| Mesin perhitungan tidak pernah membuat catatan penanggungan sendiri | `BIL-AT-057` |
| Nominal penanggungan dibatasi selisihnya, bukan sisa tagihan pasien | `UAT-24`, `BIL-AT-058` |
| Penanggungan yang disetujui tidak mengurangi sisa tagihan pasien dan tidak memindahkan status tagihan | `UAT-22`, `BIL-AT-059` |
| Pemeriksaan dua orang berlaku utuh untuk kategori baru | `UAT-25`, `BIL-AT-060` |
| Pembatalan membuka kembali selisih tanpa menyentuh tagihan pasien | `UAT-23`, `BIL-AT-061` |
| Write-off piutang pasien yang sudah berjalan tidak berubah perilakunya | Regresi pada `testing/acceptance-test-matrix.md` |
| Migration dua kolom dibuat, direview, dan nilai bawaannya diperiksa pada baris lama | Bukti keluar butir 2 pada `testing/acceptance-test-matrix.md` |
| Jumlah aturan asuransi aktif bernilai "selisih tidak boleh ditagihkan" sudah dihitung sebagai perkiraan beban kerja | Bukti keluar butir 4 pada `testing/acceptance-test-matrix.md` |
| `dotnet build` benar-benar dijalankan dan lulus | Bukti keluar butir 1 pada `testing/acceptance-test-matrix.md` |

## C8. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-11` | Migration dua kolom (`BilWriteOffCase.Category`, `BilCalculationVersion.NonBillableResidualAmount`) beserta index, cabang residual pada adapter, penerusan dan persist nominal, plafon dan penjaga per kategori pada pengecualian finansial, `BIL-VAL-040`–`043`, versi kontrak `0.6` → `0.7` (`EPIC BKC-09` backend) | Approval `BKC-DES-021`–`025` beserta wewenang tulis backend. **`MVP-7` MUST selesai lebih dulu** — cabang residual yang diubah gelombang ini berada di dalam method yang sama dengan yang dirombak `MVP-7`, dan mengerjakan keduanya bersamaan akan bertabrakan pada berkas yang sama |
| `MVP-12` | Frontend: nominal sisa selisih pada layar Pengecualian Finansial, pilihan kategori pada formulir pengajuan, pre-fill nominal, peringatan pada layar finalisasi (`EPIC BKC-09` frontend) | `MVP-11` **selesai dan terverifikasi hidup**. Dideploy lebih dulu akan menampilkan Rp 0 pada sisa selisih sehingga petugas menyimpulkan tidak ada yang perlu ditanggung |
| `POST-MVP` | Daftar kerja lintas tagihan untuk selisih yang menunggu penanggungan; keputusan atas jalur "tidak ditanggung" (`BKC-OQ-093`); keputusan pemblokiran finalisasi (`BKC-OQ-094`); perapian ketidaksesuaian `AdjustmentType` pada `erd/03-financial-exception-adjustment.md` | Di luar cakupan rilis ini; masing-masing butuh keputusan pemiliknya sendiri |

Epic berstatus `OPEN DECISION`: **tidak ada**. Seluruh `FR-BKC-038`–`044` berdisposisi `EXISTING / REUSE`, `EXTEND`, atau `MISSING / NEW`.

### Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | --- | :---: |
| `BKC-OQ-093` | Apakah selisih dari aturan yang menyatakan "tidak ditanggung" (bukan sisa perhitungan) — dengan penanda selisih tidak boleh ditagihkan ke pasien — juga ditanggung rumah sakit lewat jalur yang sama | Product/Domain Owner + Finance/AR | Ekonominya sama persis dengan sisa perhitungan, tetapi `BKC-DEC-080` menyebut hanya sisa perhitungan. Desain ini **tidak** ikut memindahkannya; jalur itu tetap seperti revisi `0.7`. Bila jawabannya "ikut", perubahannya satu baris akumulator — embernya sudah ada. **Rekomendasi desain: ikut dirutekan**, agar tidak ada dua perlakuan berbeda untuk uang yang sama-sama tidak dapat ditagihkan | **Tidak** — perilaku yang berlaku sekarang sudah `approved` dan tidak berubah; jawabannya memperluas, bukan membongkar |
| `BKC-OQ-094` | (a) Apakah finalisasi tagihan diblokir selama selisih belum ditanggung, atau cukup diperingatkan; (b) bagaimana catatan penanggungan kategori baru ini diperlakukan pada penyerahan AR/AP dan pembukuan | Billing Owner (a) + Finance/AR (b) | (a) Desain ini memilih **memperingatkan, bukan memblokir**, mengikuti preseden `BKC-OQ-086`. Bila jawabannya memblokir, satu baris transisi status baru wajib ditambahkan. (b) Desain ini memakai perilaku penyerahan yang sudah berjalan apa adanya untuk kedua kategori; bila pembukuan menuntut pemisahan akun, penyesuaiannya ada di modul penyerahan, bukan di sini | **Tidak** — pilihan bawaannya aman dan dapat diubah kemudian tanpa membongkar kontrak |

**Status Bagian C: `draft`.** Tidak ada pertanyaan terbuka bertanda memblokir yang lahir dari amendment ini, dan `BKC-OQ-084` yang semula memblokir sudah ditutup. Yang masih memblokir `/plan-module-delivery` untuk modul ini adalah sisa Bagian B — `BKC-OQ-083` dan `BKC-OQ-085` — beserta approval manusia atas `BKC-DES-021`–`025`.
