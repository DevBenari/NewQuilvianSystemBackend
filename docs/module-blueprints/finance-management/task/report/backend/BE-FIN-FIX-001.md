# Laporan Perubahan Backend — `BE-FIN-FIX-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-FIX-001` |
| Judul | Data Tagihan (Finance › Transaksi A/R › Tagihan/Billing): daftar tagihan dengan saringan kategori, pencarian, penjamin, status, jenis pasien, periode, beserta ringkasan |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap asli. Memakai pola `FIX` modul lain (`BE-BKC-FIX-XXX`); modul Finance belum punya penomoran `FIX` |
| Roadmap | Tidak ada baris roadmap resmi. Permintaan langsung pengguna, 5 Oktober 2026 |
| Trace | `FIN-DEC-006` (EMPLOYEE_BENEFIT, belum tuntas), `FIN-DEC-048` (batch hanya PAYER), `FIN-DES-024`, `FIN-VAL-114`, `FIN-VAL-115`, `Keuangan.md` bagian AR › Data Tagihan/Billing |
| Contract version | Aditif: dua endpoint baca baru. Satu perubahan perilaku pada `POST receivable-invoice-batches` (validasi lebih ketat, bagian 3.3). Nomor versi kontrak tidak saya naikkan; `contracts/api-contract.md` milik skill blueprint dan belum diperbarui |
| Dependency | Tidak ada |
| Klasifikasi | `HEAVY` — skor 9: repository tulis 0; berkas diperiksa >20 → 2; berkas diubah 5 → 1; logika (gabungan 6 tabel, saringan berlapis, aturan "Sudah Dibuat") → 2; kontrak API (endpoint baru + validasi create lebih ketat) → 2; database (hanya perilaku query, tanpa skema) → 1; keamanan/auth 0; UI/workflow (menopang satu halaman baru) → 1 |
| Task mode | `BACKEND`. Repository V1 `QuilvianSystemBackendDev1` dan frontend dibaca sebagai rujukan, tidak diubah |
| Target tulis | `NewQuilvianSystemBackend` saja |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `46fa2a91f8c812d1d1c5e83a94aed3dddfccac93` (branch `Yasmina`, working tree bersih sebelum task) |
| Tanggal | 2026-10-05; dikoreksi 2026-10-06 |
| Status | 🟡 **KODE SELESAI DITULIS, VALIDASI BELUM DIJALANKAN.** `dotnet build` `NOT RUN`; tidak ada pengujian dengan data. Belum boleh ditandai selesai (`✅`) |
| Koreksi 2026-10-06 | Pemilik modul Finance mengoreksi cakupan "Belum Dibuat": piutang berstatus `SETTLED` (lunas dibayar, termasuk gabungan asuransi + excess tunai) yang belum pernah digabung Batch Tagihan semula sengaja disembunyikan — keputusan 5 Oktober yang ternyata salah. Status pembayaran dan status pembuatan AR/Invoice adalah dua hal terpisah; piutang lunas tetap perlu bisa dibuatkan AR/Invoice-nya. Lihat bagian 2 dan 3.2 untuk detail perubahan |

---

## 1. Masalah yang diperbaiki

Menu Tagihan/Billing di V2 hanya memiliki daftar Batch Tagihan dan satu daftar "piutang layak ditagih" per penjamin (`GET eligible-receivables`, hanya menerima Id penjamin). Halaman Data Tagihan seperti di V1 membutuhkan hal yang belum ada di backend:

- Satu daftar tagihan untuk tiga kategori (Perusahaan/Asuransi, Karyawan, Pasien Umum).
- Pencarian lintas No. Bill, No. Registrasi, nama pasien, No. RM, dan nama penjamin.
- Saringan "Belum Dibuat / Sudah Dibuat", jenis pasien, periode, dan rentang tanggal.
- Ringkasan (jumlah tagihan dan jumlah pasien) yang mengikuti saringan yang sama dengan tabel.
- Field pilihan asuransi/perusahaan.

Di V1 hampir semua ini dikerjakan di frontend: halaman meminta hingga 1.000 baris lalu menyaring di browser. Backend V2 harus menyaring di database.

---

## 2. Proses bisnis

**Membuka halaman Data Tagihan.**

1. Petugas Finance memilih kategori, lalu (opsional) memilih asuransi/perusahaan lewat field pilihan, mengetik pencarian, memilih status, jenis pasien, dan periode.
2. Backend menerjemahkan saringan menjadi satu query SQL, menghitung total, jumlah tagihan, dan jumlah pasien unik atas **seluruh** hasil saringan, lalu mengambil satu halaman.
3. Setiap baris memuat petunjuk `canCreateInvoice` dan, bila belum bisa dibuatkan tagihan, alasannya.
4. Untuk membuat tagihan, frontend mengirim Id piutang (`id` pada baris) ke `POST /receivable-invoice-batches` yang sudah ada. Backend memeriksa ulang semuanya (bagian 3.3).

**Aturan "Belum Dibuat / Sudah Dibuat".**

| Status | Arti | Contoh |
| --- | --- | --- |
| Sudah Dibuat | Piutang menjadi anggota Batch Tagihan yang statusnya **bukan** `CANCELLED` | Piutang `AR-001` ada di batch `DRAFT` atau `ISSUED` → Sudah Dibuat |
| Belum Dibuat | Piutang tidak menjadi anggota batch aktif | Piutang `AR-002` belum pernah digabung → Belum Dibuat |

Cakupan daftar (dikoreksi 2026-10-06): piutang yang bukan `CANCELLED`, dan masih `OUTSTANDING`/`PARTIAL`/`SETTLED` atau sudah tergabung dalam batch aktif. Piutang `SETTLED` (lunas dibayar, mis. gabungan asuransi + excess tunai) yang belum pernah digabung tetap ditampilkan sebagai "Belum Dibuat" dan tetap bisa dibuatkan AR/Invoice — status pembayaran dan status pembuatan AR/Invoice adalah dua hal terpisah. Hanya piutang `WRITTEN_OFF` (dihapus-buku) yang tidak pernah digabung yang tidak ditampilkan, sehingga setiap baris tepat salah satu dari "Belum Dibuat" atau "Sudah Dibuat".

**Jumlah tagihan dan contoh.** Jumlah tagihan = nominal rincian piutang, yaitu nominal yang masuk ke Finance/AR.

> **Contoh:** tagihan Rp 10.000.000, penjamin menanggung Rp 7.000.000. Billing menyerahkan Rp 7.000.000 ke Finance, maka baris Data Tagihan menampilkan Rp 7.000.000. Finance tidak menghitung ulang diskon, pajak, coverage, atau deposit.

**Jumlah pasien.** Jumlah pasien unik (`PatientId` berbeda) pada seluruh hasil saringan. Baris tanpa `PatientId` (item migrasi lama) tidak dihitung.

> **Contoh:** pasien A punya 2 tagihan dan pasien B punya 1 → `patientCount` 2, bukan 3.

**Jalur tidak normal.**

| Kejadian | Hasil |
| --- | --- |
| `category`/`billingStatus`/`patientType`/`period` bernilai tidak dikenal | `400` dengan daftar nilai yang diterima |
| `startDate` setelah `endDate` | `400` "Tanggal mulai tidak boleh setelah tanggal akhir." |
| `entityId` tidak ada pada master penjamin (company) atau master pasien | `404` |
| Kategori Karyawan | Daftar kosong dengan `notice`: jalur piutang `EMPLOYEE_BENEFIT` belum ada (`FIN-DEC-006`) |
| Tidak ada hasil | Daftar kosong, `totalAmount` 0, `patientCount` 0 |
| Membuat batch untuk piutang `CANCELLED`/`SETTLED`/`WRITTEN_OFF` | `422` (validasi baru, bagian 3.3) |
| Membuat batch untuk piutang yang sudah tergabung | `409` (sudah ada), sekarang juga bila terjadi balapan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**V1 (rujukan):** `MainKasirController.cs` (`PagedKasir`), `BillingKunjunganReadService.cs` (`GetBillingPagedAsync`, `BillingPagedQuery`, `ApplyPeriodeFilter`), `Areas/Finance/AR/*` (`ARHeader` dan kawan-kawan), `FormBuatTagihanBilling.jsx`, `mainKasirSlice.jsx`, `PeriodeFilter.cs`.
**V2:** `FinReceivable`, `FinReceivableItem`, `FinReceivableInvoiceBatch(+Item)`, `FinBillingHandoffIntake`, `BilArHandoff`, `FinanceBillingIntakeService`, `BillingArApHandoffService`, `FinanceReceivablesController`, `FinanceReceivableInvoiceBatchesController`, `FinanceReceivableInvoiceBatchService`, `FinanceReceivableService`, `FinanceBusinessDate`, `PagedResult`, `ApiResponse`, konfigurasi indeks batch item, `InsuranceProviderController`/`PatientController` (endpoint options), `docs/module-blueprints/finance-management/*`, `menu-items.jsx` frontend V2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableBillingDataDtos.cs` | **Baru.** Parameter, konstanta nilai, DTO baris/ringkasan/halaman, DTO pilihan penjamin |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableBillingDataService.cs` | **Baru.** Query baca saja: `GetAsync` (daftar + ringkasan) dan `GetPayerOptionsAsync` |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` | Dua endpoint `GET` baru; konstruktor menerima service baru |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Mendaftarkan service baru (di tempat service Finance lainnya didaftarkan) |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInvoiceBatchService.cs` | `CreateAsync`: tolak piutang yang tidak layak tagih, jelaskan penyebab bila terhalang batch yang dibatalkan, ubah pelanggaran indeks unik menjadi `409`. **Dikoreksi 2026-10-06**: `notBillable` kini menerima `SETTLED`, bukan hanya `OUTSTANDING`/`PARTIAL` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableBillingDataService.cs` | **Dikoreksi 2026-10-06**: cakupan `BuildQuery` dan `ResolveCreateBlockedReason` kini menyertakan piutang `SETTLED` sebagai "Belum Dibuat" dan layak dibuatkan AR/Invoice |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru (bagian 4). Perubahan perilaku `POST /receivable-invoice-batches`: sekarang menolak (`422`) piutang berstatus selain `OUTSTANDING`/`PARTIAL` atau bernilai ≤ 0 — sebelumnya diterima. Ini menyamakan POST dengan `GET eligible-receivables` yang memang tidak pernah menampilkannya. Pelanggaran indeks unik pada race kini `409`, bukan `500` |
| Database | **Tidak ada migration, tidak ada perubahan skema.** Query memakai kolom dan indeks yang sudah ada |
| Keamanan/Auth | Memakai `FinanceReceivable : Read` yang sudah terdaftar; tanpa action baru, sehingga layar Akses Role tidak berubah. Tidak ada `IsInRole`/`UserType` |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receivable

Base URL: `api/v1/corporate/finance-management/receivables`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/billing-data` | Data Tagihan berhalaman beserta ringkasan atas seluruh hasil saringan | `FinanceReceivable : Read` | Query: `category`, `search`, `entityId`, `billingStatus`, `patientType`, `period`, `startDate`, `endDate`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `ApiResponse<BillingDataPagedResponse>` |
| `GET` | `/billing-data/payer-options` | Isi field pilihan asuransi/perusahaan | `FinanceReceivable : Read` | Query: `search`, `limit` (1-50, bawaan 20) | `ApiResponse<List<BillingDataPayerOptionResponse>>` |
| `POST` | `/api/v1/corporate/finance-management/receivable-invoice-batches` (sudah ada) | Membuat Batch Tagihan dari Id piutang | `FinanceReceivableInvoiceBatch : Create` | Body `CreateReceivableInvoiceBatchRequest` | Perilaku validasi diperketat |

Kode status `GET /billing-data`: `200` berhasil; `400` nilai saringan tidak dikenal atau rentang tanggal terbalik; `404` penjamin/pasien terpilih tidak ada; `401`/`403` tidak masuk atau tidak berhak.

Bentuk respons `data`: `pageNumber`, `pageSize`, `totalData`, `totalPage`, `items[]`, `summary { totalAmount, patientCount, entityName, periodLabel }`, `notice`.

**Pemetaan nama (spesifikasi → V2):** `page` → `pageNumber`; `id` → `items[].id` (Id piutang); `tanggal` → `billingDate`; `noBill` → `invoiceNumber`; `noRegistrasi` → `encounterNumber`; `noRM` → `medicalRecordNumber`; `namaPasien` → `patientName`; `jumlahTagihan` → `billingAmount`; `pagination.totalItems/totalPages` → `totalData/totalPage` (bentuk `PagedResult` V2 yang sudah baku).

**Pemetaan filter UI → parameter API → kolom database**

| Filter UI | Parameter | Kolom database |
| --- | --- | --- |
| Kategori Perusahaan/Asuransi · Pasien Umum · Karyawan | `category` = `company` · `generalPatient` · `employee` | `FinReceivable.DebtorType` = `PAYER` · `PATIENT_GUARANTOR` · `EMPLOYEE_BENEFIT` |
| Kolom pencarian | `search` | `BilInvoice.InvoiceNumber`, `RegPatientEncounter.EncounterNumber`, `MstPatient.FullName`, `MstPatient.MedicalRecordNumber`; kategori company juga `MstInsuranceProvider.InsuranceProviderName`, `MstCompanyGuarantor.CompanyGuarantorName`, `RegPatientEncounterGuarantor.PaymentSourceNameSnapshot` |
| Pilih asuransi/perusahaan | `entityId` (kategori company) | `FinReceivable.DebtorReferenceId` (asuransi), atau `RegPatientEncounterGuarantor.CompanyGuarantorId` pada kunjungan bila `DebtorReferenceId` kosong (perusahaan) |
| Pilih pasien | `entityId` (kategori lain) | `FinReceivableItem.PatientId` |
| Status Tagihan | `billingStatus` = `all` · `notCreated` · `created` | Keanggotaan `FinReceivableInvoiceBatchItem` (`IsDelete = false`) pada `FinReceivableInvoiceBatch.Status <> 'CANCELLED'` |
| Jenis Pasien | `patientType` = `all` · `outpatient` · `inpatient` · `emergency` | `RegPatientEncounter.EncounterType` = `Outpatient` · `Inpatient` · `Emergency` |
| Periode | `period` = `all` · `today` · `yesterday` · `thisWeek` · `thisMonth` · `lastMonth` | `RegPatientEncounter.EncounterDate`; bila kunjungan tidak terbaca, `FinReceivable.RecognizedAt` |
| Tanggal awal/akhir | `startDate`, `endDate` (`yyyy-MM-dd`) | Kolom yang sama dengan periode. Batas bawah inklusif, batas atas eksklusif (awal hari berikutnya WIB). Bila dikirim, **mengalahkan** `period` |
| Urutan | `sortBy` = `date` (bawaan) · `invoiceNumber` · `encounterNumber` · `medicalRecordNumber` · `patientName` · `amount`; `sortDirection` | Selalu ditambah urutan Id agar halaman stabil |

Jenis pasien lain (Medical Checkup, Telemedicine) hanya muncul pada `all`, dengan `patientType` `other`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` | Tidak dijalankan | `NOT RUN` | Tidak ada paket atau berkas proyek yang diubah |
| `dotnet build` | **Tidak dijalankan** (instruksi pengguna: hanya atas permintaan). **Tidak ada bukti kode ini dapat dikompilasi** | `NOT RUN` | — |
| Review diff dan scope | 3 berkas dimodifikasi, 2 berkas baru, semuanya dalam cakupan | `PASS` (pembacaan) | `git status --short` |
| Pembacaan statis | Nama properti, DbSet, namespace, dan tipe nullable diperiksa terhadap source; dua variabel bernama `from` diganti agar tidak bentrok dengan kata kunci query | `PASS` (pembacaan) | Pemeriksaan sesi |
| Kecocokan dengan `GET eligible-receivables` | Predikat "anggota batch aktif" identik | `PASS` (pembacaan) | `ActiveBatchedReceivableIds` vs `GetEligibleReceivablesAsync` |
| 28 skenario uji pada spesifikasi (company semua/tertentu, kata kunci No. Bill/No. Reg/pasien/No. RM, karyawan, pasien umum, status, jenis pasien, semua periode, rentang tanggal, gabungan, tanpa hasil, paginasi, total, pasien unik, duplikat AR, print, Excel) | Belum dijalankan; tidak ada database berisi data pada sesi ini. Print dan Excel: tidak ada endpoint di V2 (bagian 7) | `NOT RUN` | — |
| `EXPLAIN (ANALYZE, BUFFERS)` | Tidak dijalankan | `NOT RUN` | — |
| Migration | Tidak ada | `NOT APPLICABLE` | Tidak ada perubahan skema |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** build, restore, `EXPLAIN`, dan seluruh skenario uji. Alasan: instruksi pengguna agar perintah berat hanya atas permintaan, dan tidak tersedianya database berisi data.

---

## 6. Acceptance criteria dan Definition of Done

Task ad-hoc tanpa kriteria roadmap. Kriteria diambil dari spesifikasi pengguna.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Filter server-side: kategori, pencarian, penjamin, status, jenis pasien, periode, rentang tanggal, halaman, urutan | Terpenuhi di kode | `BuildQuery` dan turunannya, semuanya LINQ ke SQL |
| Pencarian tanpa filter di memori | Terpenuhi di kode | `EF.Functions.ILike` dengan escape; tidak ada `ToList` sebelum filter |
| Filter penjamin berlaku untuk asuransi **dan** perusahaan, dengan field pilihan | Terpenuhi di kode | `entityId`, `payer-options` (perubahan permintaan pengguna, bagian 7) |
| Pilihan karyawan/pasien lewat autocomplete | Terpenuhi lewat endpoint yang sudah ada | `GET /api/v1/health-services/patient-management/master-data/patients/options` (pencarian dan paginasi); tidak membuat endpoint baru |
| Status Belum/Sudah Dibuat memakai sumber kebenaran yang ada | Terpenuhi di kode | Keanggotaan batch aktif |
| Jenis pasien memakai enum, bukan nama ruangan | Terpenuhi di kode | `EncounterType` |
| Periode dan rentang tanggal tanpa kehilangan transaksi 23:59:59 | Terpenuhi di kode | Batas atas eksklusif (awal hari berikutnya) |
| Ringkasan atas seluruh hasil saringan, bukan halaman | Terpenuhi di kode | `Count`, `Sum`, `Distinct` pada query dasar sebelum paginasi |
| Tidak membuat jalur AR baru; duplikat tidak menghasilkan batch ganda | Terpenuhi di kode | Memakai `POST receivable-invoice-batches`; indeks unik + validasi + `409` |
| Print dan Export | **Tidak berlaku**: tidak ada endpoint cetak/ekspor Data Tagihan di V2 untuk diperluas | Pencarian controller Finance |
| Karyawan | **Belum terpenuhi sepenuhnya**: kategori ada tetapi kosong sampai `FIN-DEC-006` selesai | Bagian 7 |
| Batch untuk piutang perusahaan penjamin | **Belum terpenuhi**: bisa disaring dan dilihat, belum bisa dibuatkan batch | Bagian 7 |
| Kode dapat dikompilasi; skenario uji lulus | **Belum terpenuhi** | `NOT RUN` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan pengguna pada sesi ini | (1) Kategori: Perusahaan/Asuransi + Pasien Umum; Karyawan kosong. (2) Tanggal: tanggal kunjungan. (3) Penjamin: awalnya "asuransi dulu"; **diubah pengguna di tengah pengerjaan** menjadi asuransi **dan** perusahaan, dengan field pilihan. (4) Buat AR: memakai Batch yang sudah ada; kategori lain hanya baca |
| Cara penjamin perusahaan dikenali | Billing hanya mengisi `FinReceivable.DebtorReferenceId` dari `InsuranceProviderId`, sehingga piutang perusahaan penjamin tidak punya referensi. Agar filter perusahaan berfungsi **tanpa mengubah Billing dan tanpa mengisi ulang data piutang**, backend membaca perusahaan dari penjamin aktif pada kunjungan (yang utama dulu) bila `DebtorReferenceId` kosong. Akibatnya: filter dan pencarian perusahaan berfungsi untuk data lama dan baru; jika penjamin pada kunjungan diubah setelah piutang terbentuk, hasil mengikuti penjamin terkini |
| Batas yang tersisa | Piutang perusahaan penjamin tetap tidak bisa dibuatkan Batch Tagihan: `POST receivable-invoice-batches` mensyaratkan satu `DebtorReferenceId` bersama (`FIN-VAL-115`). Baris ditandai `canCreateInvoice = false` dengan alasan `NO_DEBTOR_REFERENCE`. Untuk membukanya perlu perubahan di Billing (isi `DebtorReferenceId` dengan Id perusahaan) dan pengisian ulang piutang lama — lintas modul, menyentuh buku piutang, jadi saya tidak mengerjakannya tanpa persetujuan |
| Temuan di luar scope (tidak diubah) | (a) **Batch yang dibatalkan tidak membebaskan piutang anggotanya.** `CancelAsync` hanya mengubah status batch; baris anggota tetap `IsDelete = false` dan indeks unik `IX_FinReceivableInvoiceBatchItem_ActiveReceivable` tetap menahannya, sementara `GET eligible-receivables` menyebutnya layak. Akibatnya piutang dari batch yang dibatalkan tidak bisa digabung ulang. Konfigurasi indeks sendiri mencatat ini sebagai "keputusan service yang menyusul". Saya hanya membuat penolakannya jujur: baris Data Tagihan memberi alasan `IN_CANCELLED_BATCH`, dan `POST` menjawab `409` dengan penjelasan yang benar. Perbaikan sebenarnya (menandai anggota batch terhapus saat batch dibatalkan, atau filter indeks lain) mengubah perilaku layar Canceled Invoice dan perlu keputusan pemilik Finance. (b) Intake Billing tidak pernah menghasilkan `EMPLOYEE_BENEFIT`, jadi kategori Karyawan kosong. (c) Pencarian "ID Karyawan" belum dapat dibuat karena tidak ada sumber ID karyawan pada piutang. (d) `ReceivableQuery`/`GET /receivables` tidak diperluas karena bentuk responsnya tidak mendukung ringkasan tanpa mengubah kontrak; `billing-data` ditambahkan sebagai sub-resource baca |
| Perbedaan V1 dan implementasi V2 | (1) V1 menyaring kategori dan status di browser dari 1.000 baris; V2 di database. (2) V1 menandai "sudah dibuat" lewat `PUT` terpisah setelah AR dibuat, tanpa kunci unik; V2 memakai keanggotaan batch dengan indeks unik. (3) V1 menentukan Karyawan lewat nama asuransi hardcode "RS Benefit"; V2 memakai jenis debitur. (4) V1 memakai `Kunjungan.CreateDateTime` dengan batas hari UTC; V2 memakai tanggal kunjungan dengan batas hari WIB (`FinanceBusinessDate`). (5) V1 menerapkan `periode` dan rentang tanggal sekaligus (irisan); V2: rentang tanggal eksplisit mengalahkan `period`. (6) Minggu dimulai hari Minggu seperti V1. (7) V1 mensyaratkan `startDate` dan `endDate` bersamaan; V2 menerima salah satu. (8) V1 mencari nama/No. pasien saja; V2 menambah No. Bill, No. Registrasi, dan nama penjamin |
| Risiko tersisa | (1) Kode belum dikompilasi; translasi LINQ (terutama subquery skalar dalam proyeksi dan `IN (subquery)`) baru terbukti saat dijalankan. (2) Subquery perusahaan penjamin dihitung per baris pada query dasar; dengan data besar pantau `EXPLAIN`, dan bila perlu pertimbangkan indeks `RegPatientEncounterGuarantor (EncounterId)` — indeks `EncounterId` sudah ada. (3) Validasi baru pada `POST receivable-invoice-batches` bisa menolak permintaan yang dulu lolos untuk piutang yang tidak layak tagih; frontend yang hanya memakai `eligible-receivables` tidak terdampak. (4) Satu baris = satu rincian piutang; saat ini satu piutang memiliki tepat satu rincian, tetapi model mengizinkan lebih |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Pengguna mengubah keputusan penjamin di tengah pengerjaan; sudah diterapkan |
| Status Git | `M` FinanceReceivablesController.cs, FinanceReceivableInvoiceBatchService.cs, BillingManagementServiceCollectionExtensions.cs; `??` FinanceReceivableBillingDataDtos.cs, FinanceReceivableBillingDataService.cs, dan laporan ini. Tidak ada stage, commit, push, atau perubahan branch |
| Langkah berikutnya | 1) `dotnet build` pada proyek aplikasi. 2) Uji `GET /billing-data` dan `/payer-options` dengan data; jalankan `EXPLAIN` untuk kategori company dengan pencarian. 3) Putuskan: mengubah Billing agar membawa penjamin perusahaan (membuka pembuatan batch perusahaan). 4) Putuskan perbaikan pembebasan piutang pada batch yang dibatalkan. 5) Putuskan sumber data Karyawan (`FIN-DEC-006`). 6) Minta pemilik blueprint memperbarui `contracts/api-contract.md` dengan dua endpoint baru. 7) Setelah build dan uji lulus, ubah status task ini menjadi `✅` |
