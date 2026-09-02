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
