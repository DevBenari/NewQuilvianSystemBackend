# Laporan Perubahan Backend — `BE-MDF-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-013` |
| Judul | Koreksi berjenjang hasil jasa medis (*Tiered Service Fee Adjustments*) |
| Slice | `MVP-3` (`EPIC MDF-09`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MF-DEC-007`, `MDF-DES-017`; `MDF-STATE-1.0` §3; `MDF-PERM-1.0` §3; `MDF-VAL-1.0` §6.1, §6.5–§6.12; skenario uji `T-60`..`T-61`, `T-64`..`T-70` |
| Contract version | `MDF-API-1.0`, `MDF-VAL-1.0`, `MDF-STATE-1.0`, `MDF-INTEGRATION-1.0`, seluruhnya terkunci 20 September 2026 |
| Dependency | `BE-MDF-012` — 🟡 sebagian: siklus verifikasi dan persetujuan hasil jasa; `BE-MDF-008` — 🟡 sebagian: skema tabel transaksi kalkulasi |
| Klasifikasi | `HIGH` — Pemisahan wewenang dua lapis (*Maker-Checker*), integritas nilai keuangan rumah sakit, pencegahan nilai akhir negatif, idempotensi transaksi perbankan/keuangan, dan larangan manipulasi langsung terhadap data historis yang sudah disetujui atau diserahkan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Tanggal | 27 Oktober 2026 |
| Status | `SEBAGIAN` — implementasi source code DTO (`MedicalFeeAdjustmentDtos.cs`), service domain koreksi (`MedicalFeeAdjustmentService.cs`), controller koreksi tersendiri (`MedicalFeeAdjustmentsController.cs`), integrasi endpoint pada controller hasil jasa (`MedicalFeeServiceFeesController.cs`), dan pendaftaran DI di `Program.cs` selesai memenuhi seluruh acceptance criteria dan arketipe akuntansi; `dotnet build` dilewati per instruksi batasan pemilik |

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
| QBE ID yang tidak berlaku | `QBE-DB-*` (Skema tabel `MdfServiceFeeAdjustment` dan check constraint database `CK_MdfServiceFeeAdjustment_*` sudah dibentuk pada `BE-MDF-008`) |
| Governance terbaca | `AGENTS.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Transaksi / Detail Akuntansi & Log Penyesuaian Keuangan (`transaction-endpoint-standard.md` §2.2) |

---

## 1. Masalah yang Diperbaiki

Di rumah sakit, operasional medis dinamis sering kali menemukan kekeliruan administrasi, pembatalan tindakan, komplain porsi dokter, atau kesepakatan kompensasi khusus yang baru diketahui **setelah periode perhitungan jasa ditutup (*Closed*) atau setelah hasil jasa disetujui (*Approved*)**.

Tanpa adanya mekanisme penyesuaian koreksi berjenjang (`MdfServiceFeeAdjustment`):
1. **Kebuntuan Penyesuaian Pascaperiode Ditutup**: Sesuai aturan kekal tata kelola (`MDF-VAL-1.0 §4.11`, `T-52`), periode yang sudah ditutup tidak boleh dibuka kembali (*Reopen*) atau dihitung ulang (*Recalculate*). Tanpa fitur koreksi selisih, staf administrasi tidak dapat membetulkan hak dokter tanpa melanggar prinsip pembukuan permanen.
2. **Bahaya Manipulasi Langsung (*Direct In-Place Edit*)**: Jika staf diperbolehkan mengubah nilai `GrossAmount` atau `CalculatedAmount` secara langsung pada baris historis, jejak audit akuntansi akan rusak (*audit trail tampering*). Direksi dan auditor eksternal tidak dapat mengetahui kapan, oleh siapa, dan atas dasar apa angka tersebut berubah.
3. **Risiko Kolusi dan Pencairan Fiktif**: Jika orang yang mengajukan penambahan jasa dokter (misalnya staf keuangan) dapat sekaligus menyetujui pengajuan tersebut tanpa melibatkan atasan, risiko *fraud* keuangan sangat tinggi.
4. **Bahaya Nilai Akhir Minus**: Pengurangan jasa yang tidak divalidasi dapat membuat hak dokter bernilai negatif (`FinalAmount < 0`), yang membingungkan integrasi penggajian dan sistem hutang (*Accounts Payable*) Finance.
5. **Eksekusi Ganda Jaringan (*Double Submission*)**: Ketidakstabilan jaringan saat menekan tombol pengajuan koreksi dapat menyebabkan satu klaim penyesuaian tersimpan dua kali jika tidak dilindungi `Idempotency-Key`.

### Skenario Nyata di Rumah Sakit:
Pada periode Oktober 2026 yang telah disetujui:
- **dr. Hendra, Sp.B** memiliki total hak jasa kotor sebesar Rp15.000.000,00.
- Diketahui terdapat satu tindakan "Apendektomi Cito" tanggal 18 Oktober yang pasiennya membatalkan penagihan asuransi dan diselesaikan secara damai oleh komite medik, sehingga disepakati porsi dokter dikurangi Rp1.500.000,00.
- Staf Verifikator Medis, **Budi**, tidak boleh mengubah angka tagihan atau menghitung ulang periode. Sebaliknya, Budi mengajukan koreksi pengurang (*Deduction*) sebesar Rp1.500.000,00 dengan alasan resmi: "Penyesuaian pembatalan tindakan klaim asuransi pasien Ny. Sesuai notulen komite medik 25 Okt 2026" beserta header `Idempotency-Key`.
- Status koreksi terbit sebagai `Pending` dengan nomor unik `ADJ-202610-0001`. Nilai jasa dr. Hendra saat ini **belum berubah sama sekali**.
- Ketika Budi mencoba menyetujui koreksi tersebut, sistem menolak keras dengan error `MDF_MAKER_CHECKER_VIOLATION` (`T-60`, `T-61`).
- Pengajuan ini kemudian diperiksa oleh Manajer Pelayanan Medis / Keuangan, **Ibu Dewi**. Setelah memverifikasi berkas, Ibu Dewi menyetujui koreksi (`Approved`).
- Seketika di dalam satu transaksi serializable yang aman, status koreksi menjadi `Approved`, stempel audit (`ApprovedBy`, `ApprovedAt`) tercatat, `AdjustmentAmount` pada hasil jasa dr. Hendra menjadi -Rp1.500.000,00, dan `FinalAmount` dr. Hendra menjadi Rp13.500.000,00 secara presisi (`T-64`).

---

## 2. Solusi yang Diterapkan

Berdasarkan keputusan bisnis `MF-DEC-007`, arsitektur `MDF-DES-017`, siklus hidup `MDF-STATE-1.0` §3, dan matriks validasi `MDF-VAL-1.0` §6:

1. **DTO Pengelolaan Penyesuaian Koreksi (`MedicalFeeAdjustmentDtos.cs`)**:
   - `CreateAdjustmentRequest`: Validasi arah (`Addition`/`Deduction`), nominal (`Amount > 0`), alasan wajib (`Reason`, 10–500 karakter), dan token konkurensi optimistik (`ExpectedRowVersion`).
   - `ApproveAdjustmentRequest`: Validasi persetujuan dengan token konkurensi dan catatan opsional.
   - `RejectAdjustmentRequest`: Validasi penolakan dengan alasan wajib minimal 10 karakter (`RejectionReason`).
   - `ServiceFeeAdjustmentResponse`: Respon lengkap yang menyertakan nomor koreksi, nomor hasil jasa, nominal, arah, status (`Pending`, `Approved`, `Rejected`), stempel pengaju dan penyetuju (diperkaya nama lengkap dari `Users`), serta nilai terkini `GrossAmount`, `AdjustmentAmount`, dan `FinalAmount`.
   - `AdjustmentPagedQuery` & `AdjustmentFilterMetadataResponse`: DTO query dan metadata filter lintas hasil jasa.

2. **Domain Service Koreksi Berjenjang (`MedicalFeeAdjustmentService.cs`)**:
   - **Prinsip Imutabilitas Nilai Historis (`MF-DEC-007`)**: Nilai kotor `GrossAmount` tidak pernah diedit langsung. Perubahan nilai akhir hanya terjadi melalui akumulasi `AdjustmentAmount` yang disetujui (`FinalAmount = GrossAmount + AdjustmentAmount`, Rule 6.1).
   - **Enforcement Maker-Checker Dua Lapis (`MDF-DES-017`, `T-60`, `T-61`)**:
     - *Lapis Aplikasi/Service*: Method `ApproveAdjustmentAsync` dan `RejectAdjustmentAsync` memeriksa `if (adj.RequestedBy == actorUserId) throw new InvalidOperationException("... MDF_MAKER_CHECKER_VIOLATION ...")` (HTTP 422).
     - *Lapis Basis Data*: Check constraint database PostgreSQL `CK_MdfServiceFeeAdjustment_Maker` (`"ApprovedBy" IS NULL OR "ApprovedBy" <> "RequestedBy"`) menolak operasi bahkan jika perintah UPDATE dikirim langsung ke database tanpa melalui service (`T-61`).
   - **Proteksi Status Final Permanen (`T-68`)**:
     - Koreksi berstatus `Approved` atau `Rejected` tidak dapat diedit atau dibatalkan langsung (`MDF_ADJUSTMENT_STATE_INVALID`, HTTP 422).
     - Pembatalan koreksi yang telah disetujui wajib dilakukan dengan mengajukan koreksi baru dengan arah berlawanan (*contrary adjustment*).
   - **Proteksi Nilai Akhir Non-Negatif (`T-67`)**:
     - Baik saat pengajuan (*projection check*) maupun saat persetujuan (*execution check*), koreksi pengurang (*Deduction*) yang menyebabkan `FinalAmount < 0` ditolak dengan HTTP 422 (`MDF-VAL-1.0 §6.11`).
   - **Proteksi Eksekusi Ganda dengan Idempotency-Key (`T-70`)**:
     - Method `RequestAdjustmentAsync` memvalidasi keberadaan header `Idempotency-Key` (HTTP 400 bila absen).
     - Menggunakan `ConcurrentDictionary` thread-safe: jika kunci idempotensi yang sama dikirimkan berulang, sistem mengembalikan hasil transaksi yang telah diproses sebelumnya tanpa menambah baris duplikat.
   - **Penolakan Wajib Berisi Alasan (`T-69`)**:
     - Method `RejectAdjustmentAsync` menolak jika `RejectionReason` kurang dari 10 karakter (HTTP 400).
   - **Penomoran Otomatis Rapi**:
     - Format penomoran: `ADJ-{yyyyMM}-{seq:D4}` (contoh: `ADJ-202610-0001`).

3. **REST API Controller Terintegrasi**:
   - **Sub-Route pada Hasil Jasa (`MedicalFeeServiceFeesController.cs`)**:
     - `GET /service-fees/{id}/adjustments` (Riwayat koreksi hasil jasa)
     - `GET /service-fees/{id}/adjustments/{adjId}` (Detail satu koreksi)
     - `POST /service-fees/{id}/adjustments` (Pengajuan koreksi ber-idempotensi)
     - `POST /service-fees/{id}/adjustments/{adjId}/approval` (Persetujuan Maker-Checker)
     - `POST /service-fees/{id}/adjustments/{adjId}/rejection` (Penolakan dengan alasan)
   - **Dedicated Controller Koreksi Lintas Hasil Jasa (`MedicalFeeAdjustmentsController.cs`)**:
     - `GET /adjustments/filters/metadata`
     - `GET /adjustments` (Pencarian dan monitoring seluruh koreksi)
     - `GET /adjustments/{id}` (Detail langsung via ID koreksi)
   - **Otorisasi Granular (`MDF-PERM-1.0 §3`)**:
     - Pengajuan dilindungi `[AccessPermission("MedicalFeeServiceFees", "RequestAdjustment")]`.
     - Persetujuan dan penolakan dilindungi `[AccessPermission("MedicalFeeServiceFees", "ApproveAdjustment")]`.
     - Kedua izin ini tidak boleh diberikan kepada satu pengguna/peran jabatan yang sama demi kepatuhan pemisahan wewenang.

4. **Pendaftaran Dependency Injection**:
   - `MedicalFeeAdjustmentService` didaftarkan sebagai `AddScoped` pada `Program.cs`.

---

## 3. Alur Proses Bisnis Pengajuan dan Persetujuan Koreksi Jasa Medis

```mermaid
flowchart TD
    subgraph Pengajuan Koreksi
        A["Temuan Kekeliruan / Pembatalan Tindakan Pascakalkulasi"] --> B["Staf Mengajukan Koreksi: POST /service-fees/{id}/adjustments"]
        B --> C{"Validasi Input: Idempotency-Key, Alasan >= 10 Karakter, Amount > 0"}
        C -- "Tidak Valid" --> D["Ditolak: HTTP 400 Bad Request (T-65, T-66, T-70)"]
        C -- "Valid" --> E{"Apakah Koreksi Deduction Menghasilkan FinalAmount < 0?"}
        E -- "Ya (Negatif)" --> F["Ditolak: HTTP 422 MDF-VAL-1.0 §6.11 (T-67)"]
        E -- "Tidak (Sah)" --> G["Terbitkan Baris Koreksi: Status 'Pending', Nomor: ADJ-YYYYMM-XXXX"]
    end

    subgraph Pemeriksaan & Persetujuan (Maker-Checker)
        G --> H["Pejabat Berwenang Memeriksa Riwayat Koreksi"]
        H --> I{"Apakah Aksi: Setujui atau Tolak?"}
        
        I -- "Setujui (Approval)" --> J{"Pemeriksaan Maker-Checker: Apakah Penyetuju == Pengaju?"}
        J -- "Sama (Pelanggaran)" --> K["DITOLAK: HTTP 422 MDF_MAKER_CHECKER_VIOLATION (T-60, T-61)"]
        J -- "Berbeda (Pejabat Terpisah)" --> L["Eksekusi Transaksi Serializable"]
        L --> M["Status Koreksi -> Approved"]
        L --> N["Update MdfServiceFee: AdjustmentAmount & FinalAmount Diperbarui (T-64)"]
        
        I -- "Tolak (Rejection)" --> O{"Apakah Alasan Penolakan Terisi >= 10 Karakter?"}
        O -- "Kurang / Kosong" --> P["Ditolak: HTTP 400 Bad Request (T-69)"]
        O -- "Lengkap" --> Q["Status Koreksi -> Rejected (Nilai Jasa Tidak Berubah)"]
    end

    subgraph Imutabilitas Status Final
        M --> R["Status 'Approved' Terkunci Permanen (T-68)"]
        Q --> S["Status 'Rejected' Terkunci Permanen (T-68)"]
        R -. "Jika Perlu Dibatalkan" .-> T["Ajukan Koreksi Baru Berarah Berlawanan (MF-DEC-007)"]
    end
```

---

## 4. Spesifikasi Endpoint REST API

### 4.1 Endpoint Koreksi pada Resource Hasil Jasa Medis
**Swagger Tag Group:** `[Tags("Health Services / Medical Fee Management / Service Fees")]`  
**Base Route:** `api/v1/health-services/medical-fee-management/service-fees/{id:guid}/adjustments`

| Method | Path | Deskripsi | Otorisasi / Permission | Header & Request Body | Status Balasan | Model Response |
| :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| `GET` | `/{id:guid}/adjustments` | Mengambil seluruh riwayat koreksi untuk satu hasil jasa dokter | `[AccessPermission("MedicalFeeServiceFees", "View")]` | — | `200 OK`<br>`404 NotFound` | `ApiResponse<List<ServiceFeeAdjustmentResponse>>` |
| `GET` | `/{id:guid}/adjustments/{adjId:guid}` | Mengambil detail satu koreksi penyesuaian | `[AccessPermission("MedicalFeeServiceFees", "View")]` | Param: `id`, `adjId` | `200 OK`<br>`404 NotFound` | `ApiResponse<ServiceFeeAdjustmentResponse>` |
| `POST` | `/{id:guid}/adjustments` | Mengajukan penyesuaian koreksi baru (status: `Pending`) | `[AccessPermission("MedicalFeeServiceFees", "RequestAdjustment")]` | Header: `Idempotency-Key` (Wajib)<br>Body: `CreateAdjustmentRequest` (`Direction`, `Amount`, `Reason`, `ExpectedRowVersion`) | `201 Created`<br>`400 BadRequest`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<ServiceFeeAdjustmentResponse>` |
| `POST` | `/{id:guid}/adjustments/{adjId:guid}/approval` | Menyetujui pengajuan koreksi (Pending -> Approved) — **Maker-Checker** | `[AccessPermission("MedicalFeeServiceFees", "ApproveAdjustment")]` | Param: `id`, `adjId`<br>Body: `ApproveAdjustmentRequest` (`ExpectedRowVersion`, `Notes`) | `200 OK`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<ServiceFeeAdjustmentResponse>` |
| `POST` | `/{id:guid}/adjustments/{adjId:guid}/rejection` | Menolak pengajuan koreksi (Pending -> Rejected) | `[AccessPermission("MedicalFeeServiceFees", "ApproveAdjustment")]` | Param: `id`, `adjId`<br>Body: `RejectAdjustmentRequest` (`RejectionReason` [min 10 char], `ExpectedRowVersion`) | `200 OK`<br>`400 BadRequest`<br>`404 NotFound`<br>`409 Conflict`<br>`422 Unprocessable` | `ApiResponse<ServiceFeeAdjustmentResponse>` |

---

### 4.2 Endpoint Monitoring & Audit Koreksi Lintas Hasil Jasa
**Swagger Tag Group:** `[Tags("Health Services / Medical Fee Management / Adjustments")]`  
**Base Route:** `api/v1/health-services/medical-fee-management/adjustments`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body / Query | Status Balasan | Model Response |
| :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| `GET` | `/filters/metadata` | Mengambil opsi status dan arah penyesuaian untuk filter UI | `[AccessPermission("MedicalFeeAdjustments", "View")]` | — | `200 OK` | `ApiResponse<AdjustmentFilterMetadataResponse>` |
| `GET` | `/` | Mengambil daftar seluruh penyesuaian koreksi terpaginasi | `[AccessPermission("MedicalFeeAdjustments", "View")]` | Query: `ServiceFeeId`, `Status`, `Direction`, `Search`, `SortBy`, `SortDirection`, `PageNumber`, `PageSize` | `200 OK` | `ApiResponse<PagedResult<ServiceFeeAdjustmentResponse>>` |
| `GET` | `/{id:guid}` | Mengambil detail penyesuaian langsung via ID koreksi | `[AccessPermission("MedicalFeeAdjustments", "View")]` | Param: `id` | `200 OK`<br>`404 NotFound` | `ApiResponse<ServiceFeeAdjustmentResponse>` |

---

## 5. Pemetaan Skenario Uji Penerimaan (*Acceptance Test Matrix*)

| Kode Uji | Skenario Uji | Lapis Validasi | Status Bukti Source |
| :---: | :--- | :---: | :--- |
| `T-60` | Pengaju menyetujui koreksinya sendiri — ditolak lewat service | I (Invariant) | **Terpenuhi pada source**: `ApproveAdjustmentAsync` memvalidasi `adj.RequestedBy == actorUserId` dan melempar `InvalidOperationException` dengan kode `MDF_MAKER_CHECKER_VIOLATION` |
| `T-61` | Pengaju menyetujui koreksinya sendiri — ditolak langsung pada database | I (Database Constraint) | **Terpenuhi pada skema database**: Check constraint PostgreSQL `CK_MdfServiceFeeAdjustment_Maker` (`"ApprovedBy" IS NULL OR "ApprovedBy" <> "RequestedBy"`) menolak update SQL langsung |
| `T-64` | Ajukan koreksi penambah (*Addition*), setujui, periksa `FinalAmount` | E (End-to-End) | **Terpenuhi pada source**: Service mengakumulasikan `newAdjustmentTotal = sf.AdjustmentAmount + adj.Amount`, memperbarui `sf.FinalAmount = sf.GrossAmount + newAdjustmentTotal`, dan mencatat stempel audit |
| `T-65` | Pengajuan koreksi tanpa alasan | U (User Input) | **Terpenuhi pada source**: DTO validasi `[Required]`, `[StringLength(500, MinimumLength = 10)]` dan pemeriksaan service `trimmedReason.Length < 10` melempar `ArgumentException` (HTTP 400) |
| `T-66` | Pengajuan koreksi bernilai nol atau negatif | U (User Input) | **Terpenuhi pada source**: DTO validasi `[Range(0.01, ...)]` dan pemeriksaan service `request.Amount <= 0` melempar `ArgumentException` (HTTP 400) |
| `T-67` | Koreksi pengurang (*Deduction*) melebihi nilai kotor jasa medis | I (Invariant) | **Terpenuhi pada source**: Baik saat pengajuan maupun persetujuan, jika `sf.GrossAmount + newAdjustmentTotal < 0`, sistem melempar `InvalidOperationException` (HTTP 422) |
| `T-68` | Mengubah koreksi yang sudah berstatus `Approved` atau `Rejected` | I (Invariant) | **Terpenuhi pada source**: Persetujuan dan penolakan memvalidasi `adj.Status != MdfAdjustmentStatuses.Pending` dan melempar `InvalidOperationException` dengan kode `MDF_ADJUSTMENT_STATE_INVALID` (HTTP 422) |
| `T-69` | Menolak koreksi tanpa `rejectionReason` | I (Invariant) | **Terpenuhi pada source**: `RejectAdjustmentAsync` memeriksa `trimmedRejection.Length < 10` dan melempar `ArgumentException` (HTTP 400) |
| `T-70` | Mengajukan koreksi dengan `Idempotency-Key` sama dua kali | I (Invariant) | **Terpenuhi pada source**: Service menyimpan hasil transaksi pada memory cache idempotensi; pengiriman berulang dengan key sama mengembalikan hasil asli tanpa membuat baris ganda di database |

---

## 6. Berkas yang Dibuat dan Diubah

### Berkas Baru:
1. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/DTOs/MedicalFeeAdjustmentDtos.cs`  
   DTO pengajuan (`CreateAdjustmentRequest`), persetujuan (`ApproveAdjustmentRequest`), penolakan (`RejectAdjustmentRequest`), respon detail (`ServiceFeeAdjustmentResponse`), query terpaginasi (`AdjustmentPagedQuery`), dan metadata filter (`AdjustmentFilterMetadataResponse`).
2. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Services/MedicalFeeAdjustmentService.cs`  
   Service bisnis penyesuaian koreksi jasa medis lengkap dengan Maker-Checker dua lapis, kalkulasi selisih nominal, isolasi transaksi serializable, dan proteksi idempotensi.
3. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Controllers/MedicalFeeAdjustmentsController.cs`  
   REST API Controller monitoring penyesuaian koreksi lintas hasil jasa (`/filters/metadata`, `/`, `/{id}`).

### Berkas yang Diperbarui:
1. `NewQuilvianSystemBackend/Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Controllers/MedicalFeeServiceFeesController.cs`  
   Penambahan 5 endpoint manajemen penyesuaian koreksi pada resource hasil jasa medis (`GET {id}/adjustments`, `GET {id}/adjustments/{adjId}`, `POST {id}/adjustments`, `POST {id}/adjustments/{adjId}/approval`, `POST {id}/adjustments/{adjId}/rejection`).
2. `NewQuilvianSystemBackend/Program.cs`  
   Pendaftaran dependency injection `builder.Services.AddScoped<MedicalFeeAdjustmentService>()`.
3. `NewQuilvianSystemBackend/docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md`  
   Pembaruan status task `BE-MDF-013` menjadi `🟡 SEBAGIAN` pada grafik dependency, tabel rincian task, dan register status task.
4. `NewQuilvianSystemBackend/docs/module-blueprints/medical-fee/roadmap/00-delivery-roadmap.md`  
   Pembaruan status keselarasan delivery `BE-MDF-013` menjadi `🟡 Sebagian`.

---

## 7. Batasan dan Status Akhir

1. **Status Task**: `🟡 SEBAGIAN`
   - Seluruh kriteria penerimaan fungsional, arsitektural, kontrol Maker-Checker dua lapis, validasi non-negatif, proteksi idempotensi, dan kontrak REST API telah terpetakan secara lengkap dan valid pada *source code*.
   - Sesuai instruksi pemilik, perintah `dotnet build` dan eksekusi skenario uji runtime `NOT RUN` (tanpa build otomatis), sehingga status ditandai `SEBAGIAN` hingga kompilasi dan pengujian runtime dijalankan oleh pemilik sistem.
2. **Kesiapan Tahap Selanjutnya**:
   - Prasyarat untuk `BE-MDF-014` (*Penyerahan ke Finance AP — Handoff*) dan `BE-MDF-017` (*Pembatasan Data Penghasilan Tenaga Medis*) kini terpenuhi.

