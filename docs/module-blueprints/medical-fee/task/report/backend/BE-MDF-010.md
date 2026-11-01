# Laporan Perubahan Backend — `BE-MDF-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-010` |
| Judul | Mesin perhitungan periode jasa medis |
| Slice | `MVP-2` (`EPIC MDF-04`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MDF-DES-007`, `010`, `012`, `013`, `014`, `015`, `016`, `017`; `MF-DEC-013`, `014`; `MDF-VAL-1.0` §5; `MDF-API-1.0` §4; `MDF-STATE-1.0` §1 |
| Contract version | `MDF-API-1.0`, `MDF-VAL-1.0`, `MDF-STATE-1.0`, `MDF-INTEGRATION-1.0`, seluruhnya terkunci 20 September 2026 |
| Dependency | `BE-MDF-009` — 🟡 sebagian: adapter sumber layanan dibuat pada source; `BE-MDF-008` — 🟡 sebagian: migration kalkulasi dibuat pada source |
| Klasifikasi | `HIGH` — Mesin kalkulasi jasa medis bulanan, siklus hidup periode kalkulasi, pembulatan finansial 2 desimal, snapshot aturan tarif berversi, deteksi 6 kategori unresolved, kontrol transaksi Serializable, Maker-Checker, dan proteksi penutupan permanen |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Commit backend saat dikerjakan | `548785ac` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | `SEBAGIAN` — implementasi kode DTO (`MedicalFeePeriodDtos.cs`), mesin kalkulasi (`MedicalFeeCalculationService.cs`), siklus periode (`MedicalFeePeriodService.cs`), REST Controller (`MedicalFeePeriodsController.cs`), dan registrasi DI di `Program.cs` selesai lengkap memenuhi seluruh kriteria penerimaan, namun `dotnet build` dan uji runtime `T-20`..`T-24`, `T-27`..`T-37` dilewati per instruksi batasan lingkungan pemilik |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `FeeCalculation` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE) pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` |
| Status registry | `ACTIVE` |
| QBE ID yang berlaku | `QBE-SVC-001` (Service layer memisahkan akses DbContext dan kalkulasi domain), `QBE-NAM-001` (Penamaan konsisten), `QBE-MOD-001`, `QBE-PERM-001` (`[AccessController]`, `[AccessAction]`, `[AccessPermission]`) |
| QBE ID yang tidak berlaku | `QBE-DB-*` (Tidak ada migrasi database baru; menggunakan skema dari `BE-MDF-008`) |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Transaksi / Domain Core — Mesin kalkulasi jasa medis bulanan, manajemen siklus hidup periode, kalkulasi ulang non-duplikatif, dan penanganan layanan tertahan (*unresolved*) |

---

## 1. Masalah yang diperbaiki

Sebelum task `BE-MDF-010` dikerjakan, modul `MedicalFeeManagement` sudah memiliki skema entitas database (`BE-MDF-007`), migrasi DDL (`BE-MDF-008`), dan adapter pembaca sumber layanan (`BE-MDF-009`), namun **belum memiliki mesin perhitungan dan pengelola siklus periode jasa medis**.

Akibatnya:
1. **Tidak Ada Eksekutor Perhitungan**: Staf administrasi medis dan keuangan rumah sakit belum dapat menghitung hak jasa dokter dan tenaga medis dari ribuan transaksi tindakan medis bulanan.
2. **Ketiadaan Snapshot Aturan**: Bila persentase bagi hasil dokter berubah di kemudian hari, belum ada mekanisme snapshot aturan tarif (`SharingRuleId`) per rincian layanan untuk mencegah pergeseran angka historis.
3. **Risiko Duplikasi Perhitungan Ulang**: Tanpa mekanisme kalkulasi ulang yang bersih (*replace, not duplicate*), menjalankan kalkulasi ulang ketika ada perbaikan data pasien akan melipatgandakan nominal jasa medis.
4. **Tidak Ada Penampung Masalah (*Unresolved*)**: Tindakan medis yang dokternya belum terdaftar di kesepakatan atau belum memiliki aturan tarif akan hilang tanpa jejak atau menyebabkan kegagalan sistem total (*hard crash*), bukan ditampung secara elegan ke dalam antrean investigasi.
5. **Ketiadaan Tata Kelola Periode**: Belum ada siklus hidup periode (`Open` → `Calculated` → `Verified` → `Approved` → `Closed`) yang menjamin kepatuhan audit rumah sakit, prinsip pemisahan tugas *Maker-Checker*, serta pencegahan penutupan periode bila masih ada layanan yang menggantung.

Contoh konkret skenario rumah sakit:
Rumah sakit menutup pelayanan bulan September 2026 dengan 15.842 tindakan medis. Salah satu tindakannya adalah operasi Laparotomi oleh **Dr. Hendra, Sp.B** dengan tarif kotor Billing Rp 10.000.000 (meskipun pasien mendapat diskon promo RS sebesar Rp 2.000.000, hak jasa dokter tetap dihitung dari nilai kotor Rp 10.000.000 sesuai `MF-DEC-013`). Berdasarkan aturan PKS Dr. Hendra, porsi dokter operator adalah 35%.
- Mesin kalkulasi menghitung: Rp 10.000.000 × 35% = Rp 3.500.000,00.
- Snapshot ID aturan tarif disimpan pada baris rincian `MdfServiceFeeDetail`.
- Jika terdapat tindakan lain di mana dokter pelaksana belum dipilih oleh perawat, tindakan tersebut otomatis masuk ke tabel `MdfUnresolvedService` dengan alasan `PERFORMER_MISSING`, sehingga uang jasa tidak salah sasaran dan dapat ditelusuri.
- Saat verifikasi, dokter verifikator (dr. Maya) tidak boleh merangkap sebagai penyetujui (*Maker-Checker*).
- Periode tidak dapat ditutup (*Closed*) sampai kasus `PERFORMER_MISSING` tersebut diselesaikan atau dikesampingkan (*Waived*).

---

## 2. Solusi yang Diterapkan

Sesuai spesifikasi `MDF-DES-007`, `010`, `012`, `013`, `014`, `015`, `016`, `017` dan kontrak `MDF-API-1.0` §4, diimplementasikan komponen backend terintegrasi:

1. **DTO Periode dan Kalkulasi (`MedicalFeePeriodDtos.cs`)**:
   - Request DTO: `CreateFeePeriodRequest`, `CalculatePeriodRequest`, `ReopenFeePeriodRequest`, `VerifyFeePeriodRequest`, `ReturnFeePeriodRequest`, `ApproveFeePeriodRequest`, `CloseFeePeriodRequest`, `FeePeriodPagedQuery`.
   - Response DTO: `FeePeriodHeaderResponse`, `FeePeriodDetailResponse`, `FeePeriodSummaryResponse`, `CalculationResultResponse`, `FeePeriodFilterMetadataResponse`, `FeePeriodDefaultFilterResponse`, `FeePeriodSortOptionResponse`.

2. **Mesin Kalkulasi Jasa Medis (`MedicalFeeCalculationService.cs`)**:
   - **Isolasi Transaksi Serializable**: Dijalankan di dalam transaksi `IsolationLevel.Serializable` untuk mencegah konflik pembacaan atau penulisan konkuren pada transaksi billing dan kesepakatan tarif.
   - **Dasar Nilai Kotor (`BaseAmount`)**: Menghitung jasa medis murni dari `Quantity × UnitPrice` baris tagihan kotor sebelum potongan diskon rumah sakit (`MDF-DES-012`, `MF-DEC-013`).
   - **Snapshot dan Pembulatan Finansial**: Mencatat `SharingRuleId` dan `AgreementNumber` pada setiap rincian layanan, dengan pembulatan matematis 2 desimal standar finansial `MidpointRounding.AwayFromZero` (`MDF-DES-010`, `MF-DEC-014`).
   - **Kalkulasi Ulang Non-Duplikatif**: Pada periode `Calculated`, kalkulasi ulang membersihkan (*soft delete*) rincian lama dan unresolved status `Open`, serta mempertahankan penyesuaian/koreksi yang sudah disetujui (`MDF-DES-015`). Periode `Verified` ke atas menolak kalkulasi ulang (HTTP 422).
   - **Pencocokan Aturan Spesifik ke Umum**: Aturan tarif dicocokkan berdasarkan spesifisitas tertinggi (`TariffId` cocok > katalog umum), dan memvalidasi masa berlaku kontrak HR (`ContractExpired`).
   - **Pencatatan Unresolved Otomatis**: Mendeteksi 6 alasan layanan tertahan (`PERFORMER_MISSING`, `AGREEMENT_MISSING`, `RULE_MISSING`, `CONTRACT_EXPIRED`, `ROLE_UNMAPPED`, `SOURCE_UNSUPPORTED`) dan mengelompokkannya secara langsung di balasan respons (`unresolvedByReason`).

3. **Pengelola Siklus Hidup Periode (`MedicalFeePeriodService.cs`)**:
   - `GetFilterMetadata()`: Menyediakan opsi status, opsi pengurutan, arah sort, dan ukuran halaman untuk tabel frontend.
   - `GetSummaryAsync()`: Menghitung rekap kuantitatif periode (Total, Open, Calculated, Verified, Approved, Closed).
   - `GetPagedAsync()`: Menampilkan daftar periode dengan filter pencarian kode, tahun, status, serta agregasi nominal kotor, jumlah penerima, dan jumlah layanan tertahan.
   - `GetByIdAsync()`: Menampilkan detail periode lengkap dengan agregasi nilai kotor, koreksi, nilai akhir, serta rekap layanan belum terhitung per kategori alasan.
   - `CreatePeriodAsync()`: Membuka periode baru berstatus `Open`, memvalidasi rentang tanggal (`PeriodEnd >= PeriodStart`) dan keunikan kode periode.
   - `ReopenPeriodAsync()`: Mengembalikan periode dari `Calculated` ke `Open`, membersihkan rincian kalkulasi dan unresolved `Open`.
   - `VerifyPeriodAsync()`: Menaikkan status periode dan seluruh hasil jasa dari `Calculated` ke `Verified`.
   - `ReturnPeriodAsync()`: Mengembalikan status periode dan seluruh hasil jasa dari `Verified` ke `Calculated` dengan pencatatan alasan pengembalian.
   - `ApprovePeriodAsync()`: Menyetujui periode (`Verified` → `Approved`) dengan validasi mutlak prinsip *Maker-Checker* (`ApprovedBy != VerifiedBy`).
   - `ClosePeriodAsync()`: Mengunci periode secara permanen (`Approved` → `Closed`). Memvalidasi bahwa tidak boleh ada baris `MdfUnresolvedService` yang berstatus `Open` (HTTP 422 `MDF_PERIOD_HAS_UNRESOLVED`).

4. **REST API Controller (`MedicalFeePeriodsController.cs`)**:
   - Rute: `/api/v1/health-services/medical-fee-management/fee-periods`
   - Otorisasi terpadu: `[AccessController("MedicalFeePeriods")]`, `[AccessAction]`, `[AccessPermission("MedicalFeePeriods", ...)]`.
   - Dukungan header konkurensi optimistik `ExpectedRowVersion` (HTTP 409) dan idempotensi `Idempotency-Key`.
   - Audit logging terstruktur melalui `LoggerService`.

5. **Pendaftaran Dependency Injection (`Program.cs`)**:
   - Mendaftarkan `MedicalFeeCalculationService` dan `MedicalFeePeriodService` sebagai Scoped service di container ASP.NET Core.

---

## 3. Alur Proses Bisnis Siklus Periode dan Perhitungan

```text
[Kasir / Pelayanan Selesai]
        │
        ▼
1. Buka Periode (POST /fee-periods)
   - Status awal: OPEN
   - Menentukan rentang tanggal cut-off (contoh: 2026-10-01 s/d 2026-10-31)
        │
        ▼
2. Jalankan Kalkulasi (POST /fee-periods/{id}/calculation)
   - Transaksi SERIALIZABLE dimulai
   - MedicalFeeServiceSourceAdapter menarik seluruh tindakan selesai
   - Mesin mencocokkan PKS & Aturan Tarif aktif per dokter/staf
   - Dihitung porsi jasa (GrossAmount) dari nilai kotor tagihan
   - Baris bermasalah ditampung ke MdfUnresolvedService (status: OPEN)
   - Status periode berubah: CALCULATED
   - Jika perlu hitung ulang: Rincian lama diganti, bukan digandakan!
        │
   ┌────┴───────────────────────────┐
   ▼                                ▼
[Ada salah data?]          [Angka sudah sesuai]
   │                                │
Buka Kembali (Reopen)               ▼
(Status kembali ke OPEN,    3. Verifikasi (POST /fee-periods/{id}/verification)
 rincian lama dibersihkan)     - Pemeriksaan oleh Verifikator Medis
                               - Status periode & hasil jasa: VERIFIED
                                    │
                               ┌────┴──────────────────────┐
                               ▼                           ▼
                        [Ditemukan salah]           [Verifikasi lolos]
                               │                           │
                        Kembalikan (Return)                ▼
                        (Status kembali ke          4. Persetujuan (POST /fee-periods/{id}/approval)
                         CALCULATED)                   - Dilakukan oleh Direktur / Manajer Medis
                                                       - WAJIB BEDA ORANG dari verifikator (Maker-Checker)
                                                       - Status periode & hasil jasa: APPROVED
                                                           │
                                                           ▼
                                                    5. Penutupan Periode (POST /fee-periods/{id}/closure)
                                                       - Pemeriksaan Unresolved: APAKAH MASIH ADA STATUS OPEN?
                                                         * Jika ADA: DITOLAK 422 (MDF_PERIOD_HAS_UNRESOLVED)
                                                         * Jika BERSIH / WAIVED: Periode dikunci PERMANEN (CLOSED)
```

---

## 4. Spesifikasi Endpoint Bergaya Swagger

Tag Swagger: `[Tags("Health Services / Medical Fee Management / Fee Periods")]`  
Base URL: `/api/v1/health-services/medical-fee-management/fee-periods`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body / Header | Status & Balasan Utama |
|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Mengambil konfigurasi filter, opsi status, dan sorting tabel periode | `[AccessPermission("MedicalFeePeriods", "Read")]` | — | `200 OK`<br>`ApiResponse<FeePeriodFilterMetadataResponse>` |
| `GET` | `/summary` | Mengambil ringkasan kuantitatif periode (total, open, calculated, dsb) | `[AccessPermission("MedicalFeePeriods", "Read")]` | — | `200 OK`<br>`ApiResponse<FeePeriodSummaryResponse>` |
| `GET` | `/` | Mengambil daftar periode terpaginasi dengan pencarian dan filter tahun/status | `[AccessPermission("MedicalFeePeriods", "Read")]` | Query: `Search`, `Status`, `Year`, `PageNumber`, `PageSize` | `200 OK`<br>`ApiResponse<PagedResult<FeePeriodHeaderResponse>>` |
| `GET` | `/{id}` | Mengambil detail lengkap satu periode beserta rincian statistik & daftar unresolved | `[AccessPermission("MedicalFeePeriods", "Read")]` | Path: `id` (GUID) | `200 OK` / `404 Not Found`<br>`ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/` | Membuka periode kalkulasi jasa medis baru | `[AccessPermission("MedicalFeePeriods", "Create")]` | JSON: `CreateFeePeriodRequest`<br>(`PeriodCode`, `PeriodStart`, `PeriodEnd`) | `201 Created` / `422 UnprocessableEntity`<br>`ApiResponse<FeePeriodHeaderResponse>` |
| `POST` | `/{id}/calculation` | Menjalankan mesin kalkulasi jasa medis untuk periode terkait | `[AccessPermission("MedicalFeePeriods", "Update")]` | Header: `Idempotency-Key`<br>JSON: `CalculatePeriodRequest`<br>(`ExpectedRowVersion`) | `200 OK` / `409 Conflict` / `422 Unprocessable`<br>`ApiResponse<CalculationResultResponse>` |
| `POST` | `/{id}/reopen` | Membuka kembali periode dari status `Calculated` ke status `Open` | `[AccessPermission("MedicalFeePeriods", "Update")]` | JSON: `ReopenFeePeriodRequest`<br>(`ExpectedRowVersion`, `Notes`) | `200 OK` / `409 Conflict` / `422 Unprocessable`<br>`ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/{id}/verification` | Memverifikasi hasil perhitungan periode (`Calculated` → `Verified`) | `[AccessPermission("MedicalFeePeriods", "Update")]` | JSON: `VerifyFeePeriodRequest`<br>(`ExpectedRowVersion`, `Notes`) | `200 OK` / `409 Conflict` / `422 Unprocessable`<br>`ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/{id}/return` | Mengembalikan status periode yang telah diverifikasi kembali ke `Calculated` | `[AccessPermission("MedicalFeePeriods", "Update")]` | JSON: `ReturnFeePeriodRequest`<br>(`ExpectedRowVersion`, `Reason`) | `200 OK` / `409 Conflict` / `422 Unprocessable`<br>`ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/{id}/approval` | Menyetujui hasil kalkulasi periode (`Verified` → `Approved`) dengan Maker-Checker | `[AccessPermission("MedicalFeePeriods", "Update")]` | JSON: `ApproveFeePeriodRequest`<br>(`ExpectedRowVersion`, `Notes`) | `200 OK` / `409 Conflict` / `422 Unprocessable`<br>`ApiResponse<FeePeriodDetailResponse>` |
| `POST` | `/{id}/closure` | Menutup periode secara permanen (`Approved` → `Closed`). Ditolak bila ada unresolved `Open` | `[AccessPermission("MedicalFeePeriods", "Update")]` | JSON: `CloseFeePeriodRequest`<br>(`ExpectedRowVersion`, `Notes`) | `200 OK` / `409 Conflict` / `422 Unprocessable`<br>`ApiResponse<FeePeriodDetailResponse>` |

---

## 5. Berkas yang Diubah / Ditambahkan

| Berkas | Peran |
|---|---|
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/DTOs/MedicalFeePeriodDtos.cs` | DTO request/response untuk operasional kalkulasi dan siklus hidup periode |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Services/MedicalFeeCalculationService.cs` | Mesin perhitungan jasa medis bulanan, transaksi serializable, pencocokan aturan, snapshot persentase, dan pencatatan unresolved |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Services/MedicalFeePeriodService.cs` | Service siklus hidup periode, query terpaginasi, transisi status, Maker-Checker, dan proteksi penutupan |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Controllers/MedicalFeePeriodsController.cs` | API Controller 11 endpoint dengan otorisasi role, audit logging, dan manajemen status code |
| `Program.cs` | Pendaftaran DI: `AddScoped<MedicalFeeCalculationService>` dan `AddScoped<MedicalFeePeriodService>` |

---

## 6. Verifikasi dan Batasan

- **Pemeriksaan Karakter Akhir Baris (*Line Endings*)**: Seluruh 5 berkas baru dan modifikasi diverifikasi berformat Unix LF (`0 \r`).
- **Integritas Aturan Workspace**: Tidak menjalankan `dotnet build`, `dotnet test`, atau `dotnet ef database update` secara otomatis sesuai mandat eksplisit pengguna (`tanpa dotnet build automatis`).
- **Status Task**: Ditandai `🟡 SEBAGIAN` pada roadmap, menunggu pengujian kompilasi dan eksekusi test runner saat lingkungan runtime dibuka secara resmi.

