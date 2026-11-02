# Laporan Perubahan Backend — `BE-MDF-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-012` |
| Judul | Siklus verifikasi, persetujuan, dan penutupan periode jasa medis (*Verification, Approval, and Period Closure Lifecycle*) |
| Slice | `MVP-3` (`EPIC MDF-07`, `MDF-08`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MDF-DES-016`, `017`; `MF-DEC-009`; `MDF-STATE-1.0` §1–2; `MDF-PERM-1.0` §3; `MDF-VAL-1.0` §4.4, §4.7, §4.8, §4.9, §4.11, §6.4; skenario uji `T-50`..`T-63` |
| Contract version | `MDF-API-1.0`, `MDF-VAL-1.0`, `MDF-STATE-1.0`, `MDF-INTEGRATION-1.0`, seluruhnya terkunci 20 September 2026 |
| Dependency | `BE-MDF-011` — 🟡 sebagian: worklist layanan belum dapat dihitung; `BE-MDF-010` — 🟡 sebagian: mesin perhitungan periode |
| Klasifikasi | `HIGH` — Pemisahan wewenang tiga lapis (*Maker-Checker*), integritas data keuangan dan audit rumah sakit, validasi status transisi agregat periode dan item hasil jasa, penutupan permanen (*immutable closure*), transparansi audit tindakan medis |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Tanggal | 27 Oktober 2026 |
| Status | `SEBAGIAN` — implementasi source code DTO (`MedicalFeeServiceFeeDtos.cs`), ekstensi lifecycle dan query service (`MedicalFeePeriodService.cs`), REST Controller hasil jasa (`MedicalFeeServiceFeesController.cs`), serta sinkronisasi izin otorisasi controller periode (`MedicalFeePeriodsController.cs`) selesai memenuhi seluruh acceptance criteria dan arketipe akuntansi; `dotnet build` dilewati per instruksi batasan pemilik |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `FeeCalculation` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE) pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` & `EXTENSION` |
| Status registry | `ACTIVE` |
| QBE ID yang berlaku | `QBE-SVC-001` (Service layer memisahkan DbContext dan isolasi serialisasi transaksi), `QBE-NAM-001` (Konsistensi nama entity dan DTO), `QBE-PERM-001` (`[AccessController]`, `[AccessAction]`, `[AccessPermission]`) |
| QBE ID yang tidak berlaku | `QBE-DB-*` (Skema tabel `MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail` dan check constraint database `CK_*_Maker` sudah dibentuk pada `BE-MDF-008`) |
| Governance terbaca | `AGENTS.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Detail Akuntansi, Rekonsiliasi, dan Siklus Persetujuan Keuangan (`transaction-endpoint-standard.md` §2.2) |

---

## 1. Masalah yang Diperbaiki

Sebelum task `BE-MDF-012` diimplementasikan, modul Medical Fee Management telah memiliki mesin hitung (`BE-MDF-010`) dan pengelola layanan tertahan (`BE-MDF-011`), namun **belum memiliki antarmuka operasional dan alur verifikasi serta persetujuan hasil jasa dokter yang aman dan teruji**.

Permasalahan bisnis dan tata kelola rumah sakit yang terjadi:
1. **Peniadaan Transparansi "Dari Mana Angkanya"**: Dokter spesialis dan komite medik menuntut rincian transparan atas setiap rupiah jasa yang mereka terima. Tanpa endpoint detail tindakan (`GET /service-fees/{id}/details`), rumah sakit menghadapi potensi sengketa (*dispute*) jasa dokter tanpa bukti konkret rincian tindakan medis, nilai dasar kotor, persentase pembagian, dan nomor perjanjian kerja sama (PKS).
2. **Bahaya Kolusi dan Pelanggaran Tata Kelola (*Maker-Checker Breach*)**: Seseorang yang memverifikasi rincian jasa dokter (misalnya staf verifikator medis) dapat sekaligus menyetujui (*approve*) pencairan dana jika sistem tidak menerapkan pemisahan wewenang ketat. Hal ini melanggar prinsip tata kelola keuangan rumah sakit standar (`MDF-DES-017`, `MF-DEC-009`).
3. **Penyimpangan Status Asinkron**: Periode dapat diverifikasi padahal masih ada dokter yang hasil jasanya belum diperiksa (`T-55`), atau periode ditutup padahal masih terdapat tagihan tertahan yang belum diselesaikan (`MDF_PERIOD_HAS_UNRESOLVED`).
4. **Ketiadaan Verifikasi Massal**: Di rumah sakit dengan ratusan dokter spesialis, verifikasi satu per satu tanpa fitur *batch verification* memperlambat proses *closing* bulanan hingga berhari-hari.
5. **Manipulasi Manual yang Merusak Jejak Audit**: Bila terdapat endpoint pembuatan hasil jasa secara manual (`POST /service-fees`) atau pengubahan nilai langsung (`PUT /service-fees/{id}`), nilai jasa tidak lagi mencerminkan tagihan riil pasien (*BilInvoice*), sehingga membuka celah manipulasi data (*audit trail tampering*).

### Skenario Nyata di Rumah Sakit:
Pada penutupan jasa medis bulan Oktober 2026 di Rumah Sakit:
- Staf Verifikator Medis, **Budi**, memeriksa hasil perhitungan jasa dokter bedah **dr. Hendra, Sp.B**.
- Budi membuka rincian tindakan dr. Hendra melalui endpoint `GET /service-fees/{id}/details` dan mendapati tindakan "Laparotomi Eksplorasi" dengan nilai dasar Rp8.500.000,00 dan porsi sharing 20% (Rp1.700.000,00) berdasarkan PKS nomor `PKS-2026-0142`.
- Setelah data valid, Budi memverifikasi jasa dr. Hendra (`POST /service-fees/{id}/verification`).
- Kemudian Budi memanfaatkan fitur `POST /service-fees/batch-verification` untuk memverifikasi sisa seluruh dokter yang sudah valid pada periode tersebut.
- Setelah seluruh hasil jasa berstatus `Verified`, Budi memverifikasi periode kalkulasi (`POST /fee-periods/{id}/verification`).
- Saat Budi mencoba menyetujui (*Approve*) periode tersebut, sistem **menolak keras** dengan pesan pelanggaran Maker-Checker (`MDF_MAKER_CHECKER_VIOLATION`, `T-59`), karena pihak yang menyetujui wajib merupakan pejabat berbeda (misalnya **Ibu Dewi**, Manajer Keuangan / Direktur Medis).
- Bahkan jika seseorang berusaha membypass validasi aplikasi dengan update SQL langsung ke basis data, check constraint `CK_MdfFeePeriod_Maker` dan `CK_MdfServiceFee_Maker` pada level database PostgreSQL tetap menolak eksekusi tersebut (`T-61`).
- Setelah disetujui Ibu Dewi dan seluruh daftar layanan belum terhitung (*unresolved*) dipastikan tuntas (0 *Open*), periode ditutup secara permanen (`POST /fee-periods/{id}/closure`). Nilai jasa terkunci abadi dan tidak dapat diubah lagi (`T-52`).

---

## 2. Solusi yang Diterapkan

Sesuai spesifikasi `MDF-DES-016`, `017`, keputusan bisnis `MF-DEC-009`, matriks status `MDF-STATE-1.0` §1–2, matriks hak akses `MDF-PERM-1.0` §3, dan aturan validasi `MDF-VAL-1.0`:

1. **DTO Pengelolaan Hasil Jasa Medis (`MedicalFeeServiceFeeDtos.cs`)**:
   - `ServiceFeePagedQuery`: Parameter pencarian dan penyaringan terpaginasi (filter `PeriodId`, `PayeeType`, `PayeeReferenceId`, `Status`, `Search`, pengurutan, pagination).
   - `VerifyServiceFeeRequest`: DTO verifikasi tunggal dengan proteksi konkurensi optimistik (`ExpectedRowVersion`) dan catatan verifikator opsional.
   - `ReturnServiceFeeRequest`: DTO pengembalian status dari `Verified` ke `Calculated` dengan validasi wajib alasan minimal 3 karakter.
   - `BatchVerifyServiceFeesRequest`: DTO verifikasi massal seluruh jasa pada satu periode kalkulasi.
   - `ServiceFeeHeaderResponse`: Ringkasan satu baris hasil jasa penerima (dokter/staf) lengkap dengan nama penerima yang diselesaikan dari master data (`MstDoctor` atau `MstWorkforceProfile`), total nilai kotor (*GrossAmount*), total penyesuaian (*AdjustmentAmount*), total nilai akhir (*FinalAmount*), stempel verifikasi, stempel persetujuan, serta jumlah tindakan dan koreksi.
   - `ServiceFeeDetailBreakdownResponse`: Rincian per tindakan medis yang menjawab pertanyaan audit akuntansi "Dari mana angkanya" (memuat `sourceDomain`, `sourceDetailId`, `serviceDate`, `tariffName`, `roleCode`, `roleName`, `baseAmount`, `sharingPercentage`, `calculatedAmount`, `sharingRuleId`, dan `agreementNumber`).
   - `BatchVerifyResultResponse`: Hasil eksekusi verifikasi massal (`TotalFees`, `VerifiedCount`, `AlreadyVerifiedCount`).
   - `ServiceFeeFilterMetadataResponse`: Konfigurasi metadata filter, status, tipe penerima, dan opsi pengurutan tabel frontend.

2. **Penyempurnaan Logika Service (`MedicalFeePeriodService.cs`)**:
   - **Enforcement Aturan 4.7 (`T-55`) pada Verifikasi Periode**: Method `VerifyPeriodAsync` memeriksa seluruh `MdfServiceFee` pada periode tersebut. Jika ada satu saja hasil jasa yang masih berstatus `Calculated`, verifikasi periode ditolak dengan HTTP 422.
   - **Enforcement Maker-Checker Tiga Lapis (`T-59`, `T-61`) pada Persetujuan Periode**: Method `ApprovePeriodAsync` memvalidasi bahwa `userId` penyetuju tidak sama dengan `period.VerifiedBy`. Selain itu, loop terhadap seluruh `MdfServiceFee` memastikan `sf.VerifiedBy != userId` sebelum menandai status `Approved`. Hal ini selaras dengan check constraint database `CK_MdfFeePeriod_Maker` dan `CK_MdfServiceFee_Maker` (`"VerifiedBy" IS NULL OR "ApprovedBy" IS NULL OR "VerifiedBy" <> "ApprovedBy"`).
   - **Enforcement Aturan 4.11 (`T-52`) pada Penutupan Periode**: Method `ClosePeriodAsync` mengunci permanen periode berstatus `Approved` dan memastikan tidak ada `MdfUnresolvedService` yang berstatus `Open` (`MDF_PERIOD_HAS_UNRESOLVED`). Setelah `Closed`, seluruh aksi mutasi ditolak.
   - **Resolusi Nama Penerima Polimorfik (`EnrichPayeeNamesAsync`)**: Menyelesaikan nama penerima secara efisien dalam satu query batch ke `_dbContext.MstDoctors` untuk `Doctor` dan `_dbContext.MstWorkforceProfiles` untuk `Workforce`.
   - **Operasi Hasil Jasa Lengkap**:
     - `GetServiceFeeFilterMetadata()`
     - `GetServiceFeesPagedAsync(...)`
     - `GetServiceFeeByIdAsync(...)`
     - `GetServiceFeeDetailsAsync(...)`
     - `VerifyServiceFeeAsync(...)`
     - `ReturnServiceFeeAsync(...)` (menurunkan status periode kembali ke `Calculated` bila periode induk sudah `Verified`)
     - `BatchVerifyServiceFeesAsync(...)`

3. **REST API Controller Hasil Jasa (`MedicalFeeServiceFeesController.cs`)**:
   - Rute Kanonik: `/api/v1/health-services/medical-fee-management/service-fees`
   - Otorisasi Hak Akses:
     - Pembacaan (`GET /filters/metadata`, `GET /`, `GET /{id}`, `GET /{id}/details`) dilindungi izin `[AccessPermission("MedicalFeeServiceFees", "View")]`.
     - Verifikasi individual (`POST /{id}/verification`), pengembalian (`POST /{id}/return`), dan verifikasi massal (`POST /batch-verification`) dilindungi izin `[AccessPermission("MedicalFeeServiceFees", "Verify")]`.
   - Larangan Manipulasi Manual: Tidak ada endpoint `POST /service-fees` atau `PUT /service-fees/{id}` (`api-contract.md` bagian 9).

4. **Sinkronisasi Matriks Izin Controller Periode (`MedicalFeePeriodsController.cs`)**:
   - Menyelaraskan argumen `[AccessPermission]` dengan action name spesifik:
     - `Calculate` & `Reopen` -> `[AccessPermission("MedicalFeePeriods", "Calculate")]`
     - `Verify` & `Return` -> `[AccessPermission("MedicalFeePeriods", "Verify")]`
     - `Approve` -> `[AccessPermission("MedicalFeePeriods", "Approve")]`
     - `Close` -> `[AccessPermission("MedicalFeePeriods", "Close")]`
   - Hal ini menjamin segregasi wewenang `permission-audit-matrix.md` §3 dapat ditegakkan di level filter otorisasi sistem.

---

## 3. Alur Proses Bisnis Siklus Verifikasi, Persetujuan, dan Penutupan Jasa Medis

```mermaid
flowchart TD
    subgraph Tahap 1: Kalkulasi
        A["Periode Terbuka (Open)"] -->|POST /fee-periods/{id}/calculation| B["Kalkulasi Selesai (Calculated)"]
        B -->|POST /fee-periods/{id}/reopen| A
    end

    subgraph Tahap 2: Verifikasi Hasil Jasa
        B --> C["Hasil Jasa Terbit (Status: Calculated)"]
        C -->|POST /service-fees/{id}/verification atau batch-verification| D["Hasil Jasa Terverifikasi (Status: Verified)"]
        D -->|POST /service-fees/{id}/return| C
    end

    subgraph Tahap 3: Verifikasi Periode
        D --> E{"Apakah 100% Hasil Jasa Sudah Status 'Verified'?"}
        E -- "Tidak (Masih ada Calculated)" --> F["DITOLAK: HTTP 422 (MDF-VAL-1.0 §4.7, T-55)"]
        E -- "Ya (Semua Verified)" --> G["Verifikasi Periode: POST /fee-periods/{id}/verification (Status: Verified)"]
        G -->|POST /fee-periods/{id}/return| B
    end

    subgraph Tahap 4: Persetujuan Periode & Maker-Checker
        G --> H{"Pemeriksaan Maker-Checker: Apakah Penyetuju == Pemverifikasi?"}
        H -- "Sama (User yang sama)" --> I["DITOLAK: HTTP 422 (MDF-DES-017, T-59, T-61)"]
        H -- "Berbeda (Pejabat Terpisah)" --> J["Setujui Periode: POST /fee-periods/{id}/approval (Status: Approved)"]
        J --> K["Seluruh Hasil Jasa Otomatis Berstatus 'Approved'"]
    end

    subgraph Tahap 5: Penutupan Permanen
        J --> L{"Pemeriksaan Layanan Tertahan: Apakah Ada Unresolved Status 'Open'?"}
        L -- "Ada Layanan Open" --> M["DITOLAK: HTTP 422 MDF_PERIOD_HAS_UNRESOLVED"]
        L -- "Nol (Semua Resolved/Waived)" --> N["Tutup Permanen: POST /fee-periods/{id}/closure (Status: Closed)"]
        N --> O["Data Terkunci Abadi (MDF-VAL-1.0 §4.11, T-52) — Siap Handoff ke Finance"]
    end
```

### Tahapan Operasional Rinci:

1. **Penerbitan Hasil Kalkulasi**:
   - Setelah mesin kalkulasi dijalankan (`POST /fee-periods/{id}/calculation`), sistem menerbitkan baris header hasil jasa `MdfServiceFee` dan rincian tindakan `MdfServiceFeeDetail`. Status periode dan seluruh hasil jasa berstatus `Calculated`.
2. **Pemeriksaan Rincian Tindakan ("Dari Mana Angkanya")**:
   - Staf verifikator medis membuka menu rincian tindakan dokter (`GET /service-fees/{id}/details`).
   - Setiap baris tindakan memperlihatkan tanggal layanan, nama tindakan katalog, peran dokter pelaksana (misalnya Dokter Operator atau Dokter Anestesi), nilai kotor tagihan sebelum diskon (*BaseAmount*), persentase pembagian (*SharingPercentage*), hasil rupiah (*CalculatedAmount*), dan nomor PKS asal (*AgreementNumber*).
3. **Verifikasi Hasil Jasa**:
   - Verifikator dapat menyetujui hasil jasa satu per satu (`POST /service-fees/{id}/verification`) atau sekaligus untuk seluruh dokter pada periode tersebut (`POST /service-fees/batch-verification`). Status hasil jasa bertransisi menjadi `Verified`.
   - Jika terdapat kekeliruan (misalnya dokter mengajukan keberatan porsi peran), verifikator dapat mengembalikan status hasil jasa ke `Calculated` dengan menyertakan alasan (`POST /service-fees/{id}/return`).
4. **Verifikasi Periode**:
   - Ketika seluruh dokter sudah berstatus `Verified`, verifikator memverifikasi periode kalkulasi (`POST /fee-periods/{id}/verification`).
   - Sistem memvalidasi Rule 4.7 (`T-55`): jika masih terdapat satu saja dokter berstatus `Calculated`, verifikasi periode ditolak dengan pesan kesalahan eksplisit.
5. **Persetujuan Periode (*Maker-Checker*)**:
   - Manajer Keuangan / Direktur Medis melakukan persetujuan (`POST /fee-periods/{id}/approval`).
   - Sistem memvalidasi aturan Maker-Checker (`T-59`, `T-61`): pejabat penyetuju tidak boleh sama dengan petugas yang memverifikasi. Bila melanggar, transaksi di-rollback seketika.
   - Saat disetujui, status periode dan seluruh hasil jasa di dalamnya bertransisi menjadi `Approved`.
6. **Penutupan Permanen Periode**:
   - Setelah periode disetujui dan seluruh antrean layanan belum terhitung (*unresolved services*) sudah diselesaikan atau dikesampingkan dengan catatan resmi, petugas berwenang menutup periode (`POST /fee-periods/{id}/closure`).
   - Periode bertransisi ke status `Closed` dan terkunci abadi (`T-52`). Tidak ada pembukaan kembali, penghitungan ulang, atau mutasi data setelah penutupan ini. Data siap diserahkan ke Finance AP (`BE-MDF-014`).

---

## 4. Spesifikasi Endpoint REST API

### 4.1 Modul Hasil Jasa Medis (`Service Fees`)
**Swagger Tag Group:** `[Tags("Health Services / Medical Fee Management / Service Fees")]`  
**Base Route:** `api/v1/health-services/medical-fee-management/service-fees`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body / Query | Status Balasan | Model Response |
| :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| `GET` | `/filters/metadata` | Mengambil konfigurasi metadata filter, sorting, dan opsi status | `[AccessPermission("MedicalFeeServiceFees", "View")]` | — | `200 OK` | `ApiResponse<ServiceFeeFilterMetadataResponse>` |
| `GET` | `/` | Mengambil daftar hasil jasa medis terpaginasi | `[AccessPermission("MedicalFeeServiceFees", "View")]` | Query: `PeriodId`, `PayeeType`, `PayeeReferenceId`, `Status`, `Search`, `SortBy`, `SortDirection`, `PageNumber`, `PageSize` | `200 OK` | `ApiResponse<PagedResult<ServiceFeeHeaderResponse>>` |
| `GET` | `/{id:guid}` | Mengambil detail header satu hasil jasa medis | `[AccessPermission("MedicalFeeServiceFees", "View")]` | Param: `id` | `200 OK`<br>`404 NotFound` | `ApiResponse<ServiceFeeHeaderResponse>` |
| `GET` | `/{id:guid}/details` | Mengambil rincian tindakan pembentuk jasa ("Dari mana angkanya") | `[AccessPermission("MedicalFeeServiceFees", "View")]` | Param: `id` | `200 OK`<br>`404 NotFound` | `ApiResponse<List<ServiceFeeDetailBreakdownResponse>>` |
| `POST` | `/{id:guid}/verification` | Memverifikasi satu hasil jasa medis (Calculated -> Verified) | `[AccessPermission("MedicalFeeServiceFees", "Verify")]` | Body: `VerifyServiceFeeRequest` (`ExpectedRowVersion`, `Notes`) | `200 OK`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<ServiceFeeHeaderResponse>` |
| `POST` | `/{id:guid}/return` | Mengembalikan status hasil jasa (Verified -> Calculated) | `[AccessPermission("MedicalFeeServiceFees", "Verify")]` | Body: `ReturnServiceFeeRequest` (`ExpectedRowVersion`, `Reason` [min 3 char]) | `200 OK`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<ServiceFeeHeaderResponse>` |
| `POST` | `/batch-verification` | Memverifikasi massal seluruh hasil jasa pada satu periode kalkulasi | `[AccessPermission("MedicalFeeServiceFees", "Verify")]` | Body: `BatchVerifyServiceFeesRequest` (`PeriodId`) | `200 OK`<br>`404 NotFound`<br>`422 Unprocessable` | `ApiResponse<BatchVerifyResultResponse>` |

---

### 4.2 Modul Periode Kalkulasi (`Fee Periods` — Endpoint Siklus Status Terkait)
**Swagger Tag Group:** `[Tags("Health Services / Medical Fee Management / Fee Periods")]`  
**Base Route:** `api/v1/health-services/medical-fee-management/fee-periods`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body | Status Balasan | Model Response |
| :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| `POST` | `/{id:guid}/verification` | Memverifikasi periode kalkulasi (Calculated -> Verified) | `[AccessPermission("MedicalFeePeriods", "Verify")]` | Body: `VerifyFeePeriodRequest` (`ExpectedRowVersion`, `Notes`) | `200 OK`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/{id:guid}/return` | Mengembalikan status periode (Verified -> Calculated) | `[AccessPermission("MedicalFeePeriods", "Verify")]` | Body: `ReturnFeePeriodRequest` (`ExpectedRowVersion`, `Reason`) | `200 OK`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/{id:guid}/approval` | Menyetujui periode kalkulasi (Verified -> Approved) — **Maker-Checker Wajib** | `[AccessPermission("MedicalFeePeriods", "Approve")]` | Body: `ApproveFeePeriodRequest` (`ExpectedRowVersion`, `Notes`) | `200 OK`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/{id:guid}/closure` | Menutup periode secara permanen (Approved -> Closed) | `[AccessPermission("MedicalFeePeriods", "Close")]` | Body: `CloseFeePeriodRequest` (`ExpectedRowVersion`, `Notes`) | `200 OK`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<FeePeriodDetailResponse>` |

---

## 5. Pemetaan Skenario Uji Penerimaan (*Acceptance Test Matrix*)

| Kode Uji | Skenario Uji | Lapis Validasi | Status Bukti Source |
| :---: | :--- | :---: | :--- |
| `T-50` | Alur siklus status penuh: `Open` → `Calculated` → `Verified` → `Approved` → `Closed` | E (End-to-End) | **Terpenuhi pada source**: Didukung penuh oleh method transisi pada `MedicalFeePeriodService` dan controller terkait |
| `T-51` | Transisi langsung ilegal `Open` → `Approved` ditolak | I (Invariant) | **Terpenuhi pada source**: `ApprovePeriodAsync` melempar `InvalidOperationException` bila status saat ini bukan `Verified` |
| `T-52` | Periode `Closed` ke status apa pun ditolak keras | I (Invariant) | **Terpenuhi pada source**: Seluruh method mutasi (`Calculate`, `Reopen`, `Verify`, `Return`, `Approve`, `Close`) memeriksa status `Closed` dan menolak operasi |
| `T-53` | Buka kembali (*Reopen*) dari `Calculated` ke `Open` membersihkan seluruh rincian kalkulasi | I (Invariant) | **Terpenuhi pada source**: `ReopenPeriodAsync` melakukan soft-delete pada `MdfServiceFeeDetail`, `MdfServiceFee`, dan `MdfUnresolvedService` yang berstatus `Open` |
| `T-54` | Hitung ulang saat periode berstatus `Verified` ditolak | I (Invariant) | **Terpenuhi pada source**: `CalculatePeriodAsync` hanya mengizinkan status `Open` atau `Calculated` |
| `T-55` | Verifikasi periode saat masih ada hasil jasa berstatus `Calculated` ditolak | I (Invariant) | **Terpenuhi pada source**: `VerifyPeriodAsync` menghitung `unverifiedCount`; jika `> 0`, melempar `InvalidOperationException` dengan pesan pelanggaran Aturan 4.7 |
| `T-56` | Permintaan dengan `expectedRowVersion` usang menghasilkan HTTP 409 | I (Invariant) | **Terpenuhi pada source**: Seluruh method transisi melempar `DbUpdateConcurrencyException` jika `ExpectedRowVersion` tidak cocok, ditangkap controller menghasilkan HTTP 409 Conflict |
| `T-57` | Dua pengguna menyetujui bersamaan ditangani konkurensi optimistik | I (Invariant) | **Terpenuhi pada source**: RowVersion diperbarui menjadi `Guid.NewGuid()` pada setiap mutasi berstatus |
| `T-58` | Hasil jasa `HandedOff` dilarang diubah nilainya | I (Invariant) | **Terpenuhi pada source**: Endpoint pengubahan manual ditiadakan (`api-contract.md` §9); perubahan hanya melalui koreksi berjenjang pada `BE-MDF-013` |
| `T-59` | Pemverifikasi periode menyetujui periode ditolak (*Maker-Checker Violation*) | I (Invariant) | **Terpenuhi pada source**: `ApprovePeriodAsync` memeriksa `period.VerifiedBy.Value == userId` dan `sf.VerifiedBy.Value == userId` |
| `T-61` | Check constraint database `CK_MdfFeePeriod_Maker` dan `CK_MdfServiceFee_Maker` menolak bypass maker-checker langsung ke repository | I (Database Constraint) | **Terpenuhi pada skema database**: Check constraint terpasang di migrasi `BE-MDF-008` (`"VerifiedBy" IS NULL OR "ApprovedBy" IS NULL OR "VerifiedBy" <> "ApprovedBy"`) |
| `T-62` | Penyusun kesepakatan tarif mengaktifkan kesepakatannya sendiri ditolak | I (Invariant) | **Terpenuhi pada modul master data**: Matriks pemisahan wewenang `MedicalFeeSharingAgreement` |
| `T-63` | Peran jabatan tidak memegang pasangan izin dilarang | U (Otorisasi) | **Terpenuhi pada matriks izin**: `[AccessPermission]` membedakan wewenang `Verify`, `Approve`, dan `Close` secara terpisah |

---

## 6. Berkas yang Dibuat dan Diubah

### Berkas Baru:
1. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/DTOs/MedicalFeeServiceFeeDtos.cs`  
   DTO permintaan dan balasan hasil jasa medis, rincian tindakan (*breakdown*), verifikasi individual, verifikasi massal, dan metadata filter.
2. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Controllers/MedicalFeeServiceFeesController.cs`  
   REST API Controller dengan 7 endpoint hasil jasa medis, audit "Dari mana angkanya", dan verifikasi berotorisasi `[AccessPermission("MedicalFeeServiceFees", ...)]`.

### Berkas yang Diperbarui:
1. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Services/MedicalFeePeriodService.cs`  
   - Penambahan pemeriksaan Rule 4.7 (`T-55`) pada `VerifyPeriodAsync`.
   - Penambahan loop proteksi Maker-Checker pada tingkat item `MdfServiceFee` di `ApprovePeriodAsync` (`T-59`).
   - Implementasi 8 method baru hasil jasa medis (`GetServiceFeeFilterMetadata`, `GetServiceFeesPagedAsync`, `GetServiceFeeByIdAsync`, `GetServiceFeeDetailsAsync`, `VerifyServiceFeeAsync`, `ReturnServiceFeeAsync`, `BatchVerifyServiceFeesAsync`, dan `EnrichPayeeNamesAsync`).
2. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Controllers/MedicalFeePeriodsController.cs`  
   - Penyelarasan action permission attribute (`Calculate`, `Reopen`, `Verify`, `Return`, `Approve`, `Close`) dengan `permission-audit-matrix.md`.
3. `NewQuilvianSystemBackend/docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md`  
   - Pembaruan status `BE-MDF-012` menjadi `🟡 SEBAGIAN` dan pencatatan registri status task.
4. `NewQuilvianSystemBackend/docs/module-blueprints/medical-fee/roadmap/00-delivery-roadmap.md`  
   - Pembaruan status keselarasan delivery `BE-MDF-012` menjadi `🟡 Sebagian`.

---

## 7. Batasan dan Status Akhir

1. **Status Task**: `🟡 SEBAGIAN`
   - Seluruh kriteria penerimaan fungsional, arsitektural, keamanan Maker-Checker, dan kontrak REST API telah terpetakan secara lengkap dan valid pada *source code*.
   - Sesuai instruksi pemilik, perintah `dotnet build` dan eksekusi skenario uji runtime `NOT RUN` (tanpa build otomatis), sehingga status ditandai `SEBAGIAN` hingga kompilasi dan pengujian runtime dijalankan oleh pemilik sistem.
2. **Kesiapan Tahap Selanjutnya**:
   - Prasyarat untuk `BE-MDF-013` (Koreksi Berjenjang / *Tiered Adjustments*) kini terpenuhi.

