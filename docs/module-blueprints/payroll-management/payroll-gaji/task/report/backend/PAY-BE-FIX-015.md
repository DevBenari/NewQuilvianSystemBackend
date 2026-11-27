# Laporan Perbaikan Backend — `PAY-BE-FIX-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-015` (ad-hoc lanjutan seri `PAY-BE-FIX`); pasangan frontend `PAY-FE-FIX-015` |
| Judul | PPh 21 Payroll Run saat histori payroll bulan sebelumnya tidak tersedia: audit, perbaikan terbatas, dan rancangan Opening Tax Balance |
| Slice | Corporate / Payroll Management (`Prl`), mesin PPh 21, Profil Pajak HRD, simulasi |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc; roadmap dan traceability tidak diubah (`NOT APPLICABLE`) |
| Trace | Lanjutan `PAY-BE-FIX-009` (TER/YTD/gerbang mesin ganda) dan `PAY-BE-FIX-005` (kebijakan tarif berversi) |
| Contract version | Aditif; tidak ada field dihapus atau diganti nama |
| Dependency | Opening Tax Balance membutuhkan perubahan database (bagian 9) yang **belum disetujui**; tidak ada migration dibuat |
| Klasifikasi | `MEDIUM` — skor 7 (berkas diperiksa >20: 2; berkas diubah 5: 1; logika sedang: 1; kontrak API aditif: 1; keamanan/validasi berkaitan: 1; UI/workflow: 1; database 0 karena tanpa schema) |
| Task mode | `BACKEND` (frontend dikerjakan sebagai `PAY-FE-FIX-015`) |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `ab340bc7de4e9a041ca3a8e0289dc910f417ec82` (branch `Yasmina`; SHA `7e6d3b38` pada prompt adalah leluhurnya) |
| Tanggal | 23 November 2026 |
| Status | 🟡 **Source selesai; `dotnet build` `NOT RUN`; tidak ada eksekusi runtime.** Opening Tax Balance hanya dirancang |

**Backend Governance Preflight.** Area `Corporate`; Module `PayrollManagement`; prefix `Prl` terdaftar `ACTIVE`; `TOUCHED LEGACY`; tanpa entity baru sehingga QBE-ENT/NAM/MOD tidak berlaku. QBE yang berlaku: QBE-API-001 (respons tetap `ApiResponse<T>`, perubahan aditif), QBE-VAL-001 (validasi invarian), QBE-PERM-001 (atribut Access tidak diubah), QBE-DTO-001.

---

## 1. Root cause analysis

### 1.1 Yang terbukti dari source (fakta)

| Pertanyaan | Temuan | Bukti |
| --- | --- | --- |
| Apakah TER masa pajak biasa bergantung pada histori payroll? | **Tidak.** Cabang `!IsLastTaxPeriod` hanya memakai bruto bulan berjalan, tabel PTKP, dan tabel TER. Ia tidak membaca `Ytd.IsComplete` maupun `Ytd.Issues` | `PayrollTaxPolicyEngine.ComputePermanentEmployee`, cabang TER |
| Apakah bulan hilang menahan masa pajak biasa? | **Tidak.** `BuildYtd` hanya menambahkan penahan bulan hilang bila `isLastTaxPeriod` | `PayrollCalculationService.BuildYtd`, blok `if (isLastTaxPeriod) { issues.Add(issueMessage); }` |
| Kapan sebuah masa disebut terakhir? | Desember, **atau** `ResignDate` pegawai jatuh di dalam periode berjalan (kasus November yang ternyata terakhir) | `PayrollCalculationService.CalculateEmployee`, `isLastTaxPeriod` |
| Apakah tanggal cutover sistem dianggap tanggal mulai bekerja? | **Tidak.** Bulan yang seharusnya ada dihitung sejak `JoinDate` bila tahunnya sama, selain itu sejak Januari. `JoinDate` kosong berarti seluruh bulan sejak Januari dianggap perlu dibuktikan (sikap hati-hati), bukan tanggal cutover | `BuildYtd`: `joinDate.HasValue && joinDate.Value.Year == taxYear ? joinDate.Value.Month : 1` |
| Apakah kegagalan PPh menggagalkan seluruh `Calculate`? | **Tidak.** Penahan pajak masuk daftar `blockers`, bukan `errors`: pegawai tetap berstatus `Calculated` dan komponen lain tersimpan; run berstatus `Calculated`. Yang tertahan adalah `Submit` dan `Approve` lewat `EnsureReadyToDecide` | `PayrollCalculationService.CalculateRunAsync`; `PayrollRunService.CalculateAsync`, `EnsureReadyToDecide` |
| Apakah Opening Tax Balance sudah berfungsi? | **Belum.** Tidak ada entity, konfigurasi EF, maupun migration; DI memakai sumber yang selalu "tidak tersedia" | `Program.cs` baris 444; pencarian `OpeningTaxBalance` pada `Repositories/` dan `Migrations/`: nol hasil |
| Apakah nominal manual dibatasi periode? | Hanya oleh `EffectiveStartDate`/`EffectiveEndDate` profil pajak lewat `OverlapsPeriod`. **Tanggal berakhir tidak wajib**, sehingga nominal manual yang disetujui tanpa tanggal berakhir terpakai lagi pada periode berikutnya | `PayrollTaxInsuranceRules.AssessTax` |

### 1.2 Kesimpulan

Pada source `HEAD` ini **tidak ada** mekanisme yang membuat TER November gagal hanya karena Januari–Oktober tidak ada. Dugaan penyebab gagal pada kasus pengguna, yang **tidak dapat dibuktikan dari source** karena data aktual tidak diakses:

| Dugaan | Cara memeriksa (tanpa mengubah data) | Status |
| --- | --- | --- |
| Pegawai yang bermasalah `ResignDate`-nya jatuh di bulan itu, sehingga bulan itu masa pajak **terakhir** | Rincian PPh pegawai di Payroll Run: baris "Masa Pajak" kini menyebut biasa atau terakhir | **Belum terverifikasi** |
| Periode yang dihitung ternyata **Desember** | Sama | **Belum terverifikasi** |
| Kebijakan tarif tidak ada, PTKP kosong, atau status NPWP tidak konsisten (penyebab non-histori) | Penahan per pegawai pada detail run | **Belum terverifikasi** |
| Layar lama menampilkan "bulan hilang" sebagai penahan padahal TER | Sebelum perubahan, detail TER tidak membawa bulan hilang; pesan penahan pada FE hanya muncul pada masa pajak terakhir | Ditutup dengan `PAY-FE-FIX-015` |
| Backend yang berjalan memakai versi lebih lama dari source ini | Bandingkan versi yang dideploy dengan `ab340bc7` | **Belum terverifikasi** |

**Placeholder yang tetap kosong (tidak dikarang):** `PayrollPeriodId`, `PayrollRunId`, `WorkforceProfileId` kasus aktual; isi `ValidationSummaryJson` dan `TaxSnapshotJson`; apakah ada keputusan cutover; apakah data Januari–November ada di sistem lama.

---

## 2. Before vs after

| Aspek | Sebelum | Sesudah |
| --- | --- | --- |
| Formula TER dan penahan masa pajak terakhir | Sudah benar | **Tidak diubah** |
| Detail TER pegawai tetap | Tidak memuat bulan tercakup atau hilang sehingga petugas tidak tahu kesiapan rekonsiliasi Desember | Memuat `ytdCoveredMonths`, `ytdOpeningBalanceMonths`, `ytdMissingMonths`, `ytdOpeningBalanceFeatureAvailable` sebagai **informasi**; `ytdRequired` tetap `false` |
| Respons simulasi | Tanpa masa pajak | + `TaxYear`, `TaxMonth`, `IsLastTaxPeriod` |
| Nominal manual (penulisan) | Tanggal mulai dan berakhir opsional | **Keduanya wajib**: nominal tidak berlaku tanpa batas waktu |
| Nominal manual (konfigurasi lama tanpa tanggal berakhir) | Dipakai tanpa catatan | Tetap dipakai (payroll berjalan tidak terputus) disertai **peringatan** "tidak dibatasi tanggal berakhir" |

**Dampak bila payroll baru dimulai di tengah tahun:** November sampai Desember-sebelum-akhir tetap terhitung dengan TER tanpa run historis. Bulan-bulan yang belum tercatat kini terlihat sebagai informasi kesiapan, tidak sebagai kegagalan. Pada masa pajak terakhir, PPh tetap tidak menjadi angka sah sampai ada riwayat, saldo awal (belum tersedia), atau nominal manual disetujui.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan | Alasan |
| --- | --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollTaxPolicyEngine.cs` | Cabang TER mencatat empat kunci kesiapan riwayat pada `detail` | Informasi akhir tahun; **tanpa** mengubah hasil, `ytdRequired`, atau penahan |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollSimulationDtos.cs` | `PayrollSimulationTaxResponse` + `TaxYear`, `TaxMonth`, `IsLastTaxPeriod` | Layar simulasi perlu menyatakan masa pajak dan apakah terakhir |
| `Areas/Corporate/PayrollManagement/Services/PayrollSimulationService.cs` | Mengisi tiga field itu dari snapshot yang sama dengan Payroll Run | Satu sumber kebenaran |
| `Areas/Corporate/PayrollManagement/Services/PayrollTaxInsuranceRules.cs` | Cabang nominal manual disetujui: peringatan bila tanpa `EffectiveEndDate` | Mencegah pemakaian ulang diam-diam; tidak menahan payroll yang sudah berjalan |
| `Areas/Corporate/PayrollManagement/Controllers/WfpTaxController.cs` | `ValidateRequestAsync`: skema `APPROVED_MANUAL_AMOUNT` mewajibkan tanggal mulai dan berakhir (400) | Nominal manual adalah pengecualian bertanggal, bukan tetapan permanen |

**Sengaja tidak diubah:** `BuildYtd`, `PayrollTaxYtd.Empty`, formula TER dan Pasal 17, `PayrollRunService` (status, `Submit`, `Approve`, `EnsureReadyToDecide`), aturan maker-checker `ApproveManualAmount`, kategori bukan pegawai, serta seluruh snapshot final. Tidak ada tombol atau parameter `SkipTaxValidation`/`IgnoreMissingHistory`/`BypassPph`. Tidak ada migration, tidak ada perubahan `ModelSnapshot`.

### 3.1 Tiga keadaan kalkulasi vs finalisasi (sudah ada; dipertahankan)

| Keadaan | Bagaimana terwakili di kontrak existing |
| --- | --- |
| Perhitungan selesai | `RunStatus = Calculated`; pegawai `CalculationState = Calculated` |
| PPh membutuhkan tindakan | Penahan pada `Blockers` run dan `ValidationIssues` pegawai; `Tax.ResultStatus` = `ERROR` atau `NOT_CONFIGURED`; nominal pada status itu **bukan hasil** |
| Siap disahkan | `Blockers` kosong, `ErrorEmployeeCount = 0`, `ProcessedEmployeeCount > 0` (`EnsureReadyToDecide`, diperiksa ulang pada `Submit` **dan** `Approve`) |

Peringatan (mis. riwayat belum tercatat pada TER, nominal manual tanpa tanggal berakhir) masuk `Warnings` dan **tidak** menahan.

---

## 4. Contoh perhitungan

> Nilai tarif, PTKP, dan lapisan di bawah adalah **contoh ilustrasi** supaya urutan langkah terlihat, bukan bacaan dari kebijakan `PrlTaxPolicy` Anda. Angka resmi selalu dari kebijakan yang disahkan.

| Kasus | Langkah | Hasil dan status |
| --- | --- | --- |
| **A. November, TER, tanpa histori** (pegawai tetap, TK/0, Gross) | Bruto kena pajak Rp10.000.000; PTKP TK/0 dipetakan ke kategori TER A; tarif lapisan A untuk bruto itu 2%; 10.000.000 × 2% | PPh Rp200.000. `CALCULATED`, `ytdRequired = false`. Gaji bersih = bruto − PPh − potongan lain (tanpa potongan lain: Rp9.800.000). Bulan Januari–Oktober tercatat sebagai **informasi**, tidak menahan |
| **B. Desember, riwayat lengkap** (bruto bulanan Rp10.000.000 selama 12 bulan; PPh yang sudah dipotong Januari–November = 11 × Rp200.000 = Rp2.200.000) | Bruto setahun Rp120.000.000. Biaya jabatan 5% = Rp6.000.000 (di bawah batas Rp500.000/bulan × 12). Netto Rp114.000.000. PTKP Rp54.000.000 → PKP Rp60.000.000. Pasal 17: 5% × 60.000.000 = Rp3.000.000. Dikurangi yang sudah dipotong Rp2.200.000 | PPh Desember Rp800.000. `CALCULATED`, `reconciliationOutcome = UNDERWITHHELD_CATCH_UP` |
| **C. Desember, tanpa riwayat** (hanya November ada) | Bulan Januari–Oktober tidak tercakup run disahkan maupun saldo awal. Mesin **tidak** mengasumsikan bulan itu nol | Tidak ada angka PPh. `ERROR` (penahan) dengan daftar bulan hilang dan jalur pemulihan. Komponen gaji lain tetap terhitung; `Submit` dan `Approve` ditolak. **Bukan** Rp0 |
| **D. Desember, nominal manual disetujui** | Petugas HRD menetapkan nominal beserta dasar dan masa berlaku 1–31 Desember. Pengguna lain dengan izin `ApproveManualAmount` menyetujui. Mesin memakai nominal itu | `CALCULATED` bersumber nominal manual. Pada periode Januari berikutnya konfigurasi tidak beririsan: `NotEffective`, `NOT_CONFIGURED` (penahan) |

---

## 5. Kontrak API

Tidak ada endpoint baru dan tidak ada yang dihapus.

#### Corporate / Payroll Management / Payroll Runs

Base URL: `api/v1/corporate/payroll-management/payroll-runs`

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/calculate` | Tidak berubah. `detail` pajak pegawai tetap pada TER kini memuat empat kunci `ytd*` | `PayrollRun : Calculate` |
| `POST` | `/{id}/submit` | Tidak berubah; tetap menolak selama ada penahan | `PayrollRun : Submit` |
| `POST` | `/{id}/approve` | Tidak berubah; tetap menolak selama ada penahan | `PayrollRun : Approve` |
| `POST` | `/simulations` | Respons `tax` + `taxYear`, `taxMonth`, `isLastTaxPeriod` (aditif) | `PayrollRun : Simulate` |

#### Corporate / Payroll Management / Workforce Tax

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `POST` / `PUT` | profil pajak pegawai | **400 baru**: skema `APPROVED_MANUAL_AMOUNT` tanpa `effectiveStartDate` atau `effectiveEndDate` | `WorkforceTaxProfile : Create / Update` |
| `POST` | `.../manual-amount/approve` | Tidak berubah (izin `ApproveManualAmount`, penyetuju ≠ penetap) | `WorkforceTaxProfile : ApproveManualAmount` |

**Kompatibilitas.** Klien lama yang membaca respons tidak terdampak (field tambahan). **Satu perubahan perilaku:** klien yang menyimpan skema nominal manual tanpa tanggal kini mendapat 400. Menyunting profil lain yang memakai skema manual tanpa tanggal berakhir juga harus melengkapi tanggalnya; perubahan skema atau nominal membatalkan persetujuan lama (perilaku existing), sehingga persetujuan ulang diperlukan.

---

## 6. Kesesuaian UAT (25 skenario pada prompt)

Klasifikasi jujur: **tidak ada satu pun yang dijalankan terhadap backend yang berjalan.** `dotnet build`, server, dan database dilarang. "Source" berarti ditelusuri pada kode; "tes FE" berarti fungsi murni FE dieksekusi nyata (`tests/unit/payroll-tax-history-context.test.mjs`, 12 lulus).

| UAT | Hasil | Dasar |
| --- | --- | --- |
| 01, 03 | Sesuai pada source | Cabang TER tidak membaca `Ytd`; bulan hilang hanya menahan bila `isLastTaxPeriod` |
| 02 | Rumus sesuai (10.000.000 × 2% = 200.000); tarif dari kebijakan | Source; contoh bagian 4 |
| 04, 12 | Sesuai pada source | `isLastTaxPeriod` memeriksa `ResignDate`; `ptkp is null` memblokir; PTKP kosong → `NOT_CONFIGURED` |
| 05, 15, 16 | Sesuai pada source | `blockers` vs `errors`; `EnsureReadyToDecide` |
| 06, 07 | Sesuai pada source | `AssessTax`: nominal dipakai hanya bila `Approved` |
| 08 | Sesuai pada source | `UnavailablePayrollOpeningTaxBalanceSource` ⇒ `OpeningBalanceFeatureAvailable = false`, bukan saldo nol |
| 09 | Sesuai pada source | Irisan cakupan saldo awal dan run ditolak di `BuildYtd` |
| 10, 11 | Sesuai pada source | `BuildYtd` memakai `JoinDate` bila ada dan tahunnya sama; kosong → sejak Januari, bukan tanggal cutover |
| 13 | Sesuai pada source | `AutoPolicyMissing` → `ERROR` |
| 14 | Sesuai pada source | `autoAmount > 0 ? Calculated : ValidZero` |
| 17 | Sesuai pada source | Run final tidak dihitung ulang (`AcceptsInputs`); snapshot disimpan per baris |
| 18 | Sesuai pada source | Simulasi berjalan dalam transaksi yang dibatalkan |
| 19 | Sesuai pada source | Perlakuan beban Gross/Net dari satu `AssessTax` |
| 20 | **Diperbaiki** | Penulisan manual wajib bertanggal; konfigurasi lama diberi peringatan. Jendela tanggal menentukan: tanpa irisan → `NotEffective` |
| 21 | Sesuai pada source | `yearStart` dari `periodEnd.Year` pada `LoadPriorTaxRowsAsync` |
| 22, 23 | **Terbukti oleh tes FE** | Test 1–4 |
| 24 | Sesuai pada source | `ComputeNonEmployee` tidak disentuh |
| 25 | Sesuai pada source | Tidak ada parameter bypass; `Approve` memeriksa ulang di backend |

---

## 7. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` (dilarang). **Kompilasi belum dibuktikan** |
| Tinjauan tipe dan simbol | `TaxYear`/`TaxMonth` bertipe `int?` pada `PayrollTaxSnapshot` dan DTO; `TaxCalculationModes` sudah dipakai pada berkas controller yang sama; `Result(..., IReadOnlyList<string>? warnings)` menerima `string[]` | `PASS` (bacaan kode) |
| Akhir baris | Berkas CRLF tetap CRLF, LF tetap LF | `PASS` |
| Tes FE yang membaca source backend | 12 + 19 lulus, 0 gagal | `PASS` (mencocokkan teks source, **bukan** eksekusi C#) |
| Runtime, Swagger, SQL hasil EF | Tidak dilakukan | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

---

## 8. Peta terhadap kondisi berhenti pada prompt

| Kondisi | Status |
| --- | --- |
| Root cause dijelaskan dari source dan data tersedia | Terpenuhi, dengan batas (bagian 1.2) |
| TER biasa tidak bergantung histori | Terpenuhi (sudah demikian) |
| Kalkulasi vs finalisasi dibedakan | Terpenuhi (sudah demikian; dipertahankan) |
| `MissingMonths` tidak menyesatkan | Backend: informasi dicatat; FE: `PAY-FE-FIX-015` |
| Jalur pengecualian resmi | Nominal manual diperketat; Opening Tax Balance **belum tersedia** (bagian 9) |
| Tanpa PPh nol fiktif | Terpenuhi |
| Tanpa perubahan payroll yang sudah disahkan | Terpenuhi |
| Estimasi masa pajak terakhir pada simulasi | **Tidak dibuat** (lihat 10) |

---

## 9. Rancangan Opening Tax Balance (belum diimplementasikan)

**Status (diperbarui 23 November 2026): source service dan controller sudah dibangun pada `PAY-BE-FIX-016`; tabel tetap menunggu DDL di bawah diterapkan pemilik.** Prefix `Prl` sudah terdaftar `ACTIVE`, jadi QBE-MOD-002 bukan penghalang; penghalangnya wewenang membuat tabel. Bagian di bawah adalah rancangan asal; nama izin, rute, dan aturan yang berlaku ada di laporan `PAY-BE-FIX-016`.

Sudah ada dan dipakai: `OpeningTaxBalanceStatuses`, `OpeningTaxBalanceEmployerSources`, `PayrollOpeningTaxBalanceFacts`, `IPayrollOpeningTaxBalanceSource`. `BuildYtd` **sudah** menerapkan aturan pemakaian: hanya `Approved`, tahun pajak sama, sumber `SameEmployerPreCutover`, bulan di luar rentang atau beririsan dengan run disahkan ditolak. Yang hilang hanya penyimpanan dan layar pengelolaan.

| Komponen | Rancangan |
| --- | --- |
| Entity | `PrlOpeningTaxBalance : IdentityModel`, tabel `public."PrlOpeningTaxBalance"` |
| Kolom | `WorkforceProfileId`, `TaxYear`, `CoveredMonths` (daftar bulan 1–12, **bukan** jumlah), `EmployerSource` (`SAME_EMPLOYER_PRE_CUTOVER` / `PREVIOUS_EMPLOYER`), `TaxableGross`, `EmployerPremium`, `PensionDeduction`, `TaxWithheld`, `SourceReference` (dokumen asal), `Status` (`DRAFT`/`PENDING_APPROVAL`/`APPROVED`/`REJECTED`/`SUPERSEDED`), `Version`, `SubmittedByUserId`, `ApprovedAt`, `ApprovedByUserId`, `DecisionNote`, kolom audit `IdentityModel` |
| Indeks | Unik (`WorkforceProfileId`, `TaxYear`, `EmployerSource`, `Version`) tersaring `IsDelete = false`; indeks (`WorkforceProfileId`, `TaxYear`, `Status`) |
| Constraint | `CK`: bulan 1–12; nominal ≥ 0; `PREVIOUS_EMPLOYER` tidak dipakai mesin sebelum aturannya diputuskan |
| Service | `PayrollOpeningTaxBalanceService` (CRUD, ajukan, setujui/tolak, versi baru); implementasi `IPayrollOpeningTaxBalanceSource` membaca hanya `APPROVED` |
| Controller | `.../payroll-management/opening-tax-balances` dengan izin Read/Create/Update serta `Approve` terpisah; penyetuju ≠ pengaju (seperti nominal manual) dan wajib izin melihat nominal |
| Aturan | Saldo hanya dipakai setelah `APPROVED`; satu baris agregat bukan satu bulan (jumlah bulan dari `CoveredMonths`); ditolak bila `CoveredMonths` beririsan dengan run disahkan; perubahan memakai versi baru, bukan menimpa; tidak mengubah run final; tanpa run historis palsu |
| Rilis | DDL (disetujui pemilik) → backend → frontend; `Program.cs:444` diganti ke implementasi tersimpan |

### 9.1 Rancangan DDL (belum diterapkan; bukan migration)

Rancangan untuk ditinjau pemilik; **tidak dijalankan oleh agent**. Idempoten (`IF NOT EXISTS`, constraint dibuat lewat blok `DO` yang memeriksa `pg_constraint`). Seluruh nama ≤ 57 karakter (batas PostgreSQL 63) dan **harus dipasang identik** pada konfigurasi EF dengan `HasConstraintName`/`HasDatabaseName`, supaya migration berikutnya tidak bentrok. Urutan rilis: DDL → backend → frontend.

```sql
CREATE TABLE IF NOT EXISTS public."PrlOpeningTaxBalance" (
    "Id"                 uuid NOT NULL,
    "WorkforceProfileId" uuid NOT NULL,
    "TaxYear"            integer NOT NULL,
    "CoveredMonths"      integer[] NOT NULL,            -- bulan 1..12 yang dicakup; BUKAN jumlah baris
    "EmployerSource"     character varying(40) NOT NULL, -- SAME_EMPLOYER_PRE_CUTOVER | PREVIOUS_EMPLOYER
    "TaxableGross"       numeric(18,2) NOT NULL,
    "EmployerPremium"    numeric(18,2) NOT NULL DEFAULT 0,
    "PensionDeduction"   numeric(18,2) NOT NULL DEFAULT 0,
    "TaxWithheld"        numeric(18,2) NOT NULL,
    "SourceReference"    character varying(500) NULL,    -- dokumen asal (slip, 1721-A1, sistem lama)
    "Status"             character varying(30) NOT NULL DEFAULT 'DRAFT',
    "Version"            integer NOT NULL DEFAULT 1,
    "SubmittedAt"        timestamp with time zone NULL,
    "SubmittedByUserId"  uuid NULL,
    "ApprovedAt"         timestamp with time zone NULL,
    "ApprovedByUserId"   uuid NULL,
    "RejectedAt"         timestamp with time zone NULL,  -- penolakan: pelaku dan waktu dicatat (PAY-BE-FIX-016)
    "RejectedByUserId"   uuid NULL,
    "DecisionNote"       character varying(1000) NULL,
    "CreateDateTime"     timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreateBy"           uuid NOT NULL,
    "UpdateDateTime"     timestamp with time zone NULL,
    "UpdateBy"           uuid NOT NULL,
    "DeleteDateTime"     timestamp with time zone NULL,
    "DeleteBy"           uuid NOT NULL,
    "CancelDateTime"     timestamp with time zone NULL,
    "CancelBy"           uuid NOT NULL,
    "IsCancel"           boolean NOT NULL DEFAULT FALSE,
    "IsDelete"           boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_PrlOpeningTaxBalance" PRIMARY KEY ("Id")
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PrlOpeningTaxBalance_WorkforceProfile') THEN
        ALTER TABLE public."PrlOpeningTaxBalance"
            ADD CONSTRAINT "FK_PrlOpeningTaxBalance_WorkforceProfile"
            FOREIGN KEY ("WorkforceProfileId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_Status') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_Status"
            CHECK ("Status" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','SUPERSEDED'));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_EmployerSource') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_EmployerSource"
            CHECK ("EmployerSource" IN ('SAME_EMPLOYER_PRE_CUTOVER','PREVIOUS_EMPLOYER'));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_TaxYear') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_TaxYear"
            CHECK ("TaxYear" BETWEEN 2000 AND 2100);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_Version') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_Version"
            CHECK ("Version" >= 1);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_CoveredMonths') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_CoveredMonths"
            CHECK (cardinality("CoveredMonths") BETWEEN 1 AND 12
                   AND "CoveredMonths" <@ ARRAY[1,2,3,4,5,6,7,8,9,10,11,12]::integer[]);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_Amounts') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_Amounts"
            CHECK ("TaxableGross" >= 0 AND "EmployerPremium" >= 0 AND "PensionDeduction" >= 0 AND "TaxWithheld" >= 0);
    END IF;
    -- APPROVED wajib punya penyetuju, waktu, dan dasar dokumen.
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_ApprovedHasDecision') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_ApprovedHasDecision"
            CHECK ("Status" <> 'APPROVED' OR ("ApprovedAt" IS NOT NULL AND "ApprovedByUserId" IS NOT NULL));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_ApprovedHasSource') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_ApprovedHasSource"
            CHECK ("Status" <> 'APPROVED' OR length(btrim(coalesce("SourceReference", ''))) > 0);
    END IF;
    -- Penolakan wajib punya pelaku, waktu, dan alasan. TIDAK ada larangan pelaku sama dengan pengaju (dibedakan lewat izin/role).
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_RejectedHasDecision') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_RejectedHasDecision"
            CHECK ("Status" <> 'REJECTED'
                   OR ("RejectedAt" IS NOT NULL AND "RejectedByUserId" IS NOT NULL AND length(btrim(coalesce("DecisionNote", ''))) > 0));
    END IF;
    -- Maker-checker di tingkat database: HANYA persetujuan; penyetuju tidak boleh sama dengan pengaju.
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_MakerChecker') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_MakerChecker"
            CHECK ("ApprovedByUserId" IS NULL OR "SubmittedByUserId" IS NULL OR "ApprovedByUserId" <> "SubmittedByUserId");
    END IF;
END $$;

-- Satu nomor versi per pegawai, tahun, dan sumber.
CREATE UNIQUE INDEX IF NOT EXISTS "UX_PrlOpeningTaxBalance_Profile_Year_Source_Version"
    ON public."PrlOpeningTaxBalance" ("WorkforceProfileId", "TaxYear", "EmployerSource", "Version")
    WHERE "IsDelete" = FALSE;

-- Hanya satu versi APPROVED per pegawai, tahun, dan sumber (versi lama menjadi SUPERSEDED).
CREATE UNIQUE INDEX IF NOT EXISTS "UX_PrlOpeningTaxBalance_OneApprovedPerSource"
    ON public."PrlOpeningTaxBalance" ("WorkforceProfileId", "TaxYear", "EmployerSource")
    WHERE "Status" = 'APPROVED' AND "IsDelete" = FALSE;

CREATE INDEX IF NOT EXISTS "IX_PrlOpeningTaxBalance_WorkforceProfileId_TaxYear_Status"
    ON public."PrlOpeningTaxBalance" ("WorkforceProfileId", "TaxYear", "Status", "IsDelete");
```

**Patch bila tabel sudah dibuat dengan versi DDL sebelum `PAY-BE-FIX-016`** (tanpa kolom penolakan). Idempoten; jalankan sekali, **sebelum** backend `PAY-BE-FIX-016` dijalankan. Tidak ada baris `REJECTED` pada tabel itu karena backend sebelumnya belum berjalan, sehingga constraint aman ditambahkan.

```sql
ALTER TABLE public."PrlOpeningTaxBalance" ADD COLUMN IF NOT EXISTS "RejectedAt"       timestamp with time zone NULL;
ALTER TABLE public."PrlOpeningTaxBalance" ADD COLUMN IF NOT EXISTS "RejectedByUserId" uuid NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlOpeningTaxBalance_RejectedHasDecision') THEN
        ALTER TABLE public."PrlOpeningTaxBalance" ADD CONSTRAINT "CK_PrlOpeningTaxBalance_RejectedHasDecision"
            CHECK ("Status" <> 'REJECTED'
                   OR ("RejectedAt" IS NOT NULL AND "RejectedByUserId" IS NOT NULL AND length(btrim(coalesce("DecisionNote", ''))) > 0));
    END IF;
END $$;
```

Yang **sengaja tidak** dijaga di database, dan tetap di service: (1) irisan `CoveredMonths` dengan payroll run yang sudah disahkan (butuh membaca snapshot run); (2) irisan antar baris `APPROVED` berbeda sumber; (3) bahwa `PREVIOUS_EMPLOYER` belum dipakai mesin. Exclusion constraint untuk irisan antar baris membutuhkan ekstensi (`btree_gist`, `intarray`) dan tidak dimasukkan tanpa persetujuan.

---

## 10. Pending decisions

| No | Keputusan atau data | Pemilik | Dampak bila belum |
| ---: | --- | --- | --- |
| 1 | Kebijakan cutover payroll: Desember 2026 diselesaikan dengan nominal manual, saldo awal, atau data sistem lama | Pemilik proses pajak/Finance | Desember tidak dapat diajukan |
| 2 | Bukti riwayat Januari–November (slip, 1721-A1, atau sistem lama) | HRD/Pajak | Tidak ada dasar sah angka Desember; **angka final tidak dapat ditentukan dari data yang ada** |
| 3 | Persetujuan perubahan database Opening Tax Balance (bagian 9) | Pemilik repository | Jalur saldo awal tetap tidak tersedia |
| 4 | Apakah konfigurasi nominal manual lama tanpa tanggal berakhir ditingkatkan dari peringatan menjadi penahan | Pemilik proses pajak | Saat ini hanya peringatan, sehingga masih terpakai pada periode berikutnya sampai dilengkapi |
| 5 | Estimasi nonfiskal masa pajak terakhir pada simulasi: perlu keputusan dasar asumsi tertulis | Pemilik proses pajak | Tidak dibuat; simulasi masa pajak terakhir tanpa riwayat menampilkan penjelasan, bukan angka |
| 6 | Perlakuan masa kerja parsial dan kelebihan potong (sudah ditandai peringatan/penahan pada mesin) | Pemilik proses pajak | Tidak berubah oleh task ini |
| 7 | Risiko produksi: versi backend yang dideploy belum dicocokkan dengan source; `JoinDate` pegawai yang kosong dibaca sebagai "sejak Januari" | Pemilik repository | Diagnosis kasus aktual belum final |

---

## 11. Catatan penutup

| Hal | Isi |
| --- | --- |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada stage, commit, push, pull, merge, atau rebase |

```text
$ git status --short   (backend, branch Yasmina, HEAD ab340bc7)
 M Areas/Corporate/PayrollManagement/Controllers/WfpTaxController.cs
 M Areas/Corporate/PayrollManagement/DTOs/PayrollSimulationDtos.cs
 M Areas/Corporate/PayrollManagement/Services/PayrollSimulationService.cs
 M Areas/Corporate/PayrollManagement/Services/PayrollTaxInsuranceRules.cs
 M Areas/Corporate/PayrollManagement/Services/PayrollTaxPolicyEngine.cs
?? docs/module-blueprints/payroll-management/payroll-gaji/task/report/backend/PAY-BE-FIX-015.md
```
