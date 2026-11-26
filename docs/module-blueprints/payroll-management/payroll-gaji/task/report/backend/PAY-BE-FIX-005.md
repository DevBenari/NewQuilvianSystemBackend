# Laporan Perubahan Backend — `PAY-BE-FIX-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-005` (ad-hoc, bukan kartu roadmap) |
| Judul | Kebijakan regulasi berversi (tarif PPh dan iuran BPJS) sebagai satu-satunya sumber tarif; dasar upah BPJS menurut klasifikasi tunjangan; persetujuan tersendiri nominal PPh manual; satu jalur kalkulasi; label pajak slip dari hasil hitung |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas keputusan final pemilik 2026-11-18; roadmap, traceability, dan `PGJ-C1` tidak disentuh (preseden `PAY-BE-FIX-001..004`) |
| Trace | "Keputusan Final — Penyempurnaan PPh, BPJS, Hak Akses, dan Jalur Payroll" (5 keputusan), melanjutkan `PAY-BE-FIX-004` |
| Dependency | `PAY-BE-FIX-004` (kolom `PrlTax`/`PrlInsurance`, **belum diterapkan**) dan DDL bagian 7 di bawah. Keduanya wajib diterapkan sebelum Backend dijalankan |
| Klasifikasi | `HIGH` — mesin hitung gaji dan pajak, tabel baru, kontrak baru |
| Task mode | `BACKEND` (Frontend lewat `PAY-FE-FIX-005`) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-18 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan. `dotnet build` NOT RUN, uji API/runtime NOT RUN, migration TIDAK dibuat, DDL TIDAK dijalankan** (larangan dari prompt sebelumnya tetap berlaku; keputusan final tidak mencabutnya). Kode belum pernah dikompilasi |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement`; sentuhan kecil ke `HumanResource/MasterData/PayrollAndBenefit` (Master Tunjangan) dan `HumanResource/PayrollManagement` (Pengaturan Penggajian, jalur Modern) |
| Prefix | `Prl` (registry: `Corporate | PayrollManagement | Payroll | Prl | ACTIVE`). Entity baru `PrlTaxPolicy`, `PrlBpjsPolicy` |
| Keberlakuan | `NEW CODE` (kebijakan, mesin PPh, penyelesai kebijakan, gerbang jalur lama); `TOUCHED LEGACY` (kalkulasi, controller profil pajak, master tunjangan) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (tanpa hardcode peran: pengesah = izin `Approve`, bukan nama peran), `QBE-AUD-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-DB-001/002` (skema dicatat sebagai DDL terpisah) |
| Checker QBE | **NOT RUN** |
| Hak akses baru (Akses Role) | `PayrollTaxPolicy`: Read, Create, Update, Submit, Approve, Reject, Retire, Delete. `PayrollBpjsPolicy`: sama. `WorkforceTaxProfile`: `ApproveManualAmount` (di samping `ViewAmount` dari FIX-004). Semuanya **diperiksa di Backend**; Frontend hanya menyaring tombol |

---

## 1. Keputusan 1 — master tarif PPh berversi

- **Tabel `PrlTaxPolicy`** (satu baris per versi): nomor versi unik, judul, dasar peraturan, masa berlaku, status (`DRAFT → PENDING_APPROVAL → APPROVED`, `REJECTED`, `RETIRED`), parameter (persen dan batas biaya jabatan, tambahan persen tanpa NPWP, kelipatan pembulatan PKP, pembulatan pajak, program premi pemberi kerja yang masuk bruto, program iuran pegawai pengurang, dasar pengenaan dan cara penerapan bukan pegawai, tarif PPh 26), serta tiga tabel JSON: **PTKP** (status, nilai, kategori TER), **TER** (kategori, bulanan/harian, batas, tarif), **lapisan Pasal 17**. **Tidak ada angka regulasi yang ditanam atau diisi sistem**; seluruhnya dimasukkan HRD.
- **Alur**: HRD menyusun draf → *Ajukan* (HRD menyatakan isi sudah diverifikasi terhadap dasar peraturan; Backend menolak bila tabel/parameter tidak lengkap atau lapisan tidak bersambung) → Finance *Sahkan* atau *Tolak* (alasan wajib). **Pengesah selalu pengguna yang berbeda dari pengaju** (403). Dua kebijakan APPROVED tidak boleh beririsan masa berlakunya (409). Kebijakan yang disahkan **tidak disunting**; perubahan = *Buat Revisi* (versi baru, draf) lalu *Pensiunkan* yang lama. Draf/ditolak dapat dihapus; yang pernah disahkan hanya dipensiunkan.
- **Mesin** (`PayrollTaxPolicyEngine`, murni, tanpa angka): 
  - PPh 21 pegawai tetap, masa pajak biasa: TER (kategori dari pemetaan PTKP pada kebijakan) × bruto.
  - Masa pajak terakhir (Desember, atau bulan `ResignDate`): penghasilan setahun − biaya jabatan (min persen, batas setahun, batas bulanan × masa kerja) − iuran pensiun → − PTKP → PKP dibulatkan ke bawah → lapisan Pasal 17 (× tambahan tanpa NPWP) − PPh yang sudah dipotong. Selisih negatif = penahan.
  - Bukan pegawai: dasar pengenaan % dan cara (per pembayaran atau kumulatif) **dari kebijakan**; tanpa itu penahan.
  - PPh 26: tarif kebijakan, atau tarif P3B pada profil bila disertai dasar.
- **Akumulasi tahun berjalan**: dibaca dari `TaxSnapshotJson`/`InsuranceSnapshotJson` payroll run yang **sudah disahkan** pada tahun yang sama (bukan draf, bukan run ini). Tidak lengkap (run sebelumnya tanpa rincian akumulasi, atau masa pajak yang seharusnya ada belum punya run disahkan) = penahan, bukan perkiraan.
- **Tanpa kebijakan APPROVED yang berlaku pada akhir periode** = penahan "Menunggu Verifikasi Kebijakan", bukan Rp0 dan bukan tarif cadangan.

## 2. Keputusan 2 — dasar upah BPJS menurut sifat tunjangan

- Kolom baru `IsFixedAllowance` dan `IncludeInBpjsWageBase` (nullable; **kosong = belum diklasifikasikan**) pada `MstAllowanceType` (Master Tunjangan) dan `PrlTransportAllowancePolicy`, **terpisah dari `IsTaxable`**. Validasi tulis: keduanya diisi bersamaan atau kosong bersamaan; hanya tunjangan tetap yang boleh masuk dasar upah; transportasi berbasis kehadiran/per hari/bukan `FixedMonthly` tidak pernah tunjangan tetap.
- Dasar upah = gaji pokok + tunjangan berklasifikasi **tetap dan masuk dasar upah**. Lembur, tunjangan kehadiran, dan penghasilan variabel (termasuk transportasi per kehadiran lewat input variabel) **tidak pernah** masuk. Setiap komponen dicatat pada snapshot iuran (`WageBaseItems`: masuk/tidak/belum diklasifikasikan beserta alasannya).
- Tunjangan **tanpa klasifikasi** (juga tunjangan cadangan profil `PrlPayroll.TotalAllowance`) = **penahan iuran BPJS** untuk penerima itu; tidak ditebak masuk atau tidak.

## 3. Keputusan 3 — nilai bawaan Pengaturan Penggajian diverifikasi

- **Tabel `PrlBpjsPolicy`** berversi dengan alur yang sama (HRD ajukan, Finance sahkan, pemisahan pengaju-pengesah, revisi, pensiun): kelompok risiko JKK (1–5) **wajib beserta dasar penetapannya**, plafon dan tarif BPJS Kesehatan, JHT, JP, JKK, JKM, masa berlaku. Tarif 0 ditolak saat diajukan.
- **Mesin tidak lagi membaca tarif dari `HrdPayrollSetting`** (snapshot run). Satu-satunya sumber: kebijakan BPJS APPROVED pada periode; versinya, dasar, dan kelompok risiko JKK disimpan pada snapshot iuran setiap penerima.
- Nilai bawaan Pengaturan Penggajian (mis. JKK 0,89%) kini **berstatus "Menunggu Verifikasi Kebijakan"** pada `GET payroll/settings` (`bpjsValuesStatus`), dan hanya dapat **disalin menjadi draf** (`POST bpjs-policies/from-setting`) dengan kelompok risiko JKK **dikosongkan** (wajib diisi HRD). Nilainya tidak pernah dianggap resmi.
- Backend **tidak** mengisi klasifikasi risiko JKK rumah sakit, tarif, atau plafon apa pun: itu keputusan HRD dan Finance dengan dasar tertulis.

## 4. Keputusan 4 — izin dan persetujuan nominal manual

- `ViewAmount` tetap hanya izin **melihat** nominal. **Persetujuan nominal PPh manual kini aksi tersendiri** (`ApproveManualAmount`, `POST .../manual-amount/approve|reject`): pemanggil wajib memegang izin itu **dan** `ViewAmount`, tidak boleh penetap nominal, dan hanya status `PENDING` yang dapat diputuskan; tolak wajib alasan. **Nominal manual baru dipakai mesin bila `APPROVED`**; mengubah nominal, dasar, metode, atau skema mengembalikannya ke `PENDING`. Pemanggil tanpa `ViewAmount` yang menyimpan data lain tidak mereset persetujuan. Setiap keputusan diaudit (tanpa nominal).
- Pemetaan izin ke peran **dilakukan lewat Akses Role** (HRD Payroll Administrator: Read/Update/ViewAmount/Submit; Finance: Approve/Reject; HRD staf: sesuai fungsi), bukan di kode.

## 5. Keputusan 5 — satu jalur kalkulasi dan satu sumber slip

- `PayrollRunModernController` diberi gerbang (`LegacyModernPayrollGateFilter`), hanya untuk aksi tulis; pembacaan riwayat tidak disentuh dan **tidak ada data historis yang dihapus**:
  1. Kalkulasi PPh dan BPJS jalur lama **selalu ditolak** (422): tarifnya tertanam/berasal dari nilai bawaan yang belum diverifikasi.
  2. Run yang sudah dihitung mesin Payroll Run resmi tidak dapat diubah jalur lama (422).
  3. Saklar `Payroll:Policy:LegacyModernMutationsEnabled` (**bawaan false**): seluruh aksi tulis jalur lama ditolak. Alasan: konsumen Frontend jalur lama (layar HR payroll lama) **tidak punya route maupun menu** (diperiksa statis). Setel `true` hanya untuk menyelesaikan run lama yang masih menggantung; riwayat database wajib diperiksa pemilik sebelum saklar dibiarkan mati.
- Mesin resmi membuang komponen penghasilan jalur lama (`GP01`, `TJ01`, `TF01`, `TR01`, `OT01`, `OT02`, `SH01`) selain komponen pajak/iuran lama (sudah pada FIX-004), sehingga penghasilan tidak terhitung ganda.
- **Slip**: nama komponen pajak diturunkan dari jenis PPh, skema, cara hitung, dan versi kebijakan yang dipakai (`PayrollTaxLabels`), mis. "Pajak Penghasilan (PPh Pasal 21, TER, kebijakan X)". Slip self-service lama tidak lagi memuat label tertanam "PPh 21 TER PMK 168/2023" maupun perkiraan iuran `TotalDeduction × 0,6`; tanpa baris penerima hanya total potongan yang tampil.

## 6. Kontrak API (additive)

| Endpoint | Perubahan |
| --- | --- |
| `GET/POST/PUT/DELETE api/v1/corporate/payroll-management/tax-policies[/{id}]`, `GET filters/metadata`, `GET availability`, `POST {id}/revise|submit|approve|reject|retire` | **baru** |
| `.../bpjs-policies` (sama) + `POST from-setting` | **baru** |
| `.../workforce-profiles/{id}/tax-profile` | + `manualAmountApprovalStatus`, `manualAmountSubmittedAt`, `manualAmountDecidedAt`, `manualAmountDecisionNote`, `canDecideManualAmount`, `treatyRatePercent`, `treatyBasis` (respons); `treatyRatePercent`, `treatyBasis` (request); `POST {id}/manual-amount/approve|reject`; metadata + `taxPolicyApproved`, `taxPolicyVersion`, `taxPolicyStatusLabel` (opsi skema otomatis kini `isSupported = true`) |
| Master Tunjangan, Kebijakan Tunjangan Transportasi | + `isFixedAllowance`, `includeInBpjsWageBase` (request dan respons) |
| `GET payroll/settings` | + `bpjsValuesStatus`, `bpjsValuesStatusLabel`, `bpjsPolicyApproved`, `bpjsPolicyVersion`, `taxPolicyApproved`, `taxPolicyVersion` |
| `GET payroll-data-readiness/summary` | + `taxPolicyApproved`, `taxPolicyVersion`, `taxPolicyStatusLabel`, `bpjsPolicy*` |
| `GET payroll-runs/{runId}/employees/{runEmployeeId}` | `tax` + `policyId`, `policyLegalBasis`, `manualAmountApprovalStatus`, `taxYear`, `taxMonth`, `isLastTaxPeriod`, `taxableGross`, `employerPremium`, `employeePension`, `detail` (langkah hitung); `insurance` + `policyId`, `policyLegalBasis`, `jkkRiskClass`, `wageBaseItems[]` |

Kode: `400` aksi tidak sah untuk status, `403` (pengaju mengesahkan sendiri; tanpa `ViewAmount`), `404`, `409` (masa berlaku beririsan), `422` (kebijakan tidak lengkap; jalur lama diblokir).

## 7. Kebutuhan skema (DDL terpisah — TIDAK dibuat dan TIDAK dijalankan)

Terapkan **sesudah** DDL `PAY-BE-FIX-004` bagian 7 dan **sebelum** Backend dirilis. Idempoten. Tanpa `CHECK constraint` (nilai dijaga Backend).

```sql
-- Kolom baru
ALTER TABLE public."PrlTax"
  ADD COLUMN IF NOT EXISTS "ManualAmountApprovalStatus"   varchar(20)  NULL,
  ADD COLUMN IF NOT EXISTS "ManualAmountSubmittedAt"      timestamptz  NULL,
  ADD COLUMN IF NOT EXISTS "ManualAmountSubmittedByUserId" uuid        NULL,
  ADD COLUMN IF NOT EXISTS "ManualAmountDecidedAt"        timestamptz  NULL,
  ADD COLUMN IF NOT EXISTS "ManualAmountDecidedByUserId"  uuid         NULL,
  ADD COLUMN IF NOT EXISTS "ManualAmountDecisionNote"     varchar(500) NULL,
  ADD COLUMN IF NOT EXISTS "TreatyRatePercent"            numeric(9,4) NULL,
  ADD COLUMN IF NOT EXISTS "TreatyBasis"                  varchar(500) NULL;

ALTER TABLE public."MstAllowanceType"
  ADD COLUMN IF NOT EXISTS "IsFixedAllowance"      boolean NULL,
  ADD COLUMN IF NOT EXISTS "IncludeInBpjsWageBase" boolean NULL;

ALTER TABLE public."PrlTransportAllowancePolicy"
  ADD COLUMN IF NOT EXISTS "IsFixedAllowance"      boolean NULL,
  ADD COLUMN IF NOT EXISTS "IncludeInBpjsWageBase" boolean NULL;

-- Kebijakan tarif PPh berversi
CREATE TABLE IF NOT EXISTS public."PrlTaxPolicy" (
  "Id" uuid NOT NULL PRIMARY KEY,
  "PolicyVersion" varchar(100) NOT NULL,
  "Title" varchar(200) NOT NULL,
  "LegalBasis" varchar(1000) NOT NULL,
  "EffectiveStartDate" timestamptz NOT NULL,
  "EffectiveEndDate" timestamptz NULL,
  "PolicyStatus" varchar(30) NOT NULL DEFAULT 'DRAFT',
  "PositionCostRatePercent" numeric(9,4) NOT NULL,
  "PositionCostMonthlyCap" numeric(18,2) NOT NULL,
  "PositionCostAnnualCap" numeric(18,2) NOT NULL,
  "NoNpwpSurchargePercent" numeric(9,4) NOT NULL,
  "PkpRoundingUnit" numeric(18,2) NOT NULL,
  "RoundTaxToWholeRupiah" boolean NOT NULL,
  "EmployerPremiumPrograms" varchar(50) NULL,
  "PensionDeductionPrograms" varchar(50) NULL,
  "NonEmployeeBasisPercent" numeric(9,4) NULL,
  "NonEmployeeMethod" varchar(30) NULL,
  "Pph26RatePercent" numeric(9,4) NULL,
  "PtkpJson" text NOT NULL DEFAULT '[]',
  "TerRatesJson" text NOT NULL DEFAULT '[]',
  "BracketsJson" text NOT NULL DEFAULT '[]',
  "StatusReason" varchar(500) NULL,
  "SubmittedAt" timestamptz NULL, "SubmittedByUserId" uuid NULL,
  "DecidedAt" timestamptz NULL, "DecidedByUserId" uuid NULL,
  "RetiredAt" timestamptz NULL,
  "Notes" varchar(1000) NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreateDateTime" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP, "CreateBy" uuid NOT NULL,
  "UpdateDateTime" timestamptz NULL, "UpdateBy" uuid NOT NULL,
  "DeleteDateTime" timestamptz NULL, "DeleteBy" uuid NOT NULL,
  "CancelDateTime" timestamptz NULL, "CancelBy" uuid NOT NULL,
  "IsCancel" boolean NOT NULL DEFAULT false, "IsDelete" boolean NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_PrlTaxPolicy_PolicyVersion" ON public."PrlTaxPolicy" ("PolicyVersion") WHERE "IsDelete" = false;
CREATE INDEX IF NOT EXISTS "IX_PrlTaxPolicy_Status_Effective" ON public."PrlTaxPolicy" ("PolicyStatus", "EffectiveStartDate", "EffectiveEndDate", "IsDelete");

-- Kebijakan iuran BPJS berversi
CREATE TABLE IF NOT EXISTS public."PrlBpjsPolicy" (
  "Id" uuid NOT NULL PRIMARY KEY,
  "PolicyVersion" varchar(100) NOT NULL,
  "Title" varchar(200) NOT NULL,
  "LegalBasis" varchar(1000) NOT NULL,
  "EffectiveStartDate" timestamptz NOT NULL,
  "EffectiveEndDate" timestamptz NULL,
  "PolicyStatus" varchar(30) NOT NULL DEFAULT 'DRAFT',
  "JkkRiskClass" integer NULL,
  "JkkRiskBasis" varchar(1000) NULL,
  "HealthCapAmount" numeric(18,2) NOT NULL,
  "HealthEmployeeRate" numeric(9,4) NOT NULL,
  "HealthEmployerRate" numeric(9,4) NOT NULL,
  "JhtEmployeeRate" numeric(9,4) NOT NULL,
  "JhtEmployerRate" numeric(9,4) NOT NULL,
  "JpCapAmount" numeric(18,2) NOT NULL,
  "JpEmployeeRate" numeric(9,4) NOT NULL,
  "JpEmployerRate" numeric(9,4) NOT NULL,
  "JkkRate" numeric(9,4) NOT NULL,
  "JkmRate" numeric(9,4) NOT NULL,
  "StatusReason" varchar(500) NULL,
  "SubmittedAt" timestamptz NULL, "SubmittedByUserId" uuid NULL,
  "DecidedAt" timestamptz NULL, "DecidedByUserId" uuid NULL,
  "RetiredAt" timestamptz NULL,
  "Notes" varchar(1000) NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreateDateTime" timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP, "CreateBy" uuid NOT NULL,
  "UpdateDateTime" timestamptz NULL, "UpdateBy" uuid NOT NULL,
  "DeleteDateTime" timestamptz NULL, "DeleteBy" uuid NOT NULL,
  "CancelDateTime" timestamptz NULL, "CancelBy" uuid NOT NULL,
  "IsCancel" boolean NOT NULL DEFAULT false, "IsDelete" boolean NOT NULL DEFAULT false
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_PrlBpjsPolicy_PolicyVersion" ON public."PrlBpjsPolicy" ("PolicyVersion") WHERE "IsDelete" = false;
CREATE INDEX IF NOT EXISTS "IX_PrlBpjsPolicy_Status_Effective" ON public."PrlBpjsPolicy" ("PolicyStatus", "EffectiveStartDate", "EffectiveEndDate", "IsDelete");
```

`ApplicationDbContextModelSnapshot` tidak diperbarui (tanpa migration). Aksi izin baru muncul di Akses Role setelah sinkronisasi atribut.

### Mengisi data awal — dua jalur (saya tidak menjalankan DML)

- **Klasifikasi tunjangan** (wajib sebelum iuran BPJS dapat dihitung untuk penerima bertunjangan): UI — Payroll › Data Induk › **Jenis Tunjangan** dan **Tunjangan Transportasi** (isian "Tunjangan Tetap" dan "Masuk Dasar Upah BPJS"). SQL aman-ulang (contoh; isi nilainya sesuai perjanjian kerja, jangan dijalankan tanpa verifikasi): `UPDATE public."MstAllowanceType" SET "IsFixedAllowance" = true, "IncludeInBpjsWageBase" = true WHERE "AllowanceTypeCode" = '<kode>' AND "IsFixedAllowance" IS NULL;`
- **Kebijakan iuran BPJS**: UI — Payroll › **Kebijakan Iuran BPJS** › "Salin dari Pengaturan Penggajian" (atau Susun baru) › lengkapi kelompok risiko JKK + dasar › Ajukan (HRD) › Sahkan (Finance, pengguna berbeda). SQL (draf dari pengaturan aktif; kelompok risiko JKK sengaja kosong; versi dan dasar diisi pemilik): `INSERT INTO public."PrlBpjsPolicy" ("Id","PolicyVersion","Title","LegalBasis","EffectiveStartDate","PolicyStatus","HealthCapAmount","HealthEmployeeRate","HealthEmployerRate","JhtEmployeeRate","JhtEmployerRate","JpCapAmount","JpEmployeeRate","JpEmployerRate","JkkRate","JkmRate","CreateBy","UpdateBy","DeleteBy","CancelBy") SELECT gen_random_uuid(),'<versi>','Draf dari Pengaturan Penggajian','<dasar>','<tanggal mulai>','DRAFT',"BpjsKesehatanCapAmount","BpjsKesehatanEmployeeRate","BpjsKesehatanEmployerRate","BpjsKetenagakerjaanJhtEmployeeRate","BpjsKetenagakerjaanJhtEmployerRate","BpjsKetenagakerjaanJpCapAmount","BpjsKetenagakerjaanJpEmployeeRate","BpjsKetenagakerjaanJpEmployerRate","BpjsKetenagakerjaanJkkRate","BpjsKetenagakerjaanJkmRate",'00000000-0000-0000-0000-000000000000','00000000-0000-0000-0000-000000000000','00000000-0000-0000-0000-000000000000','00000000-0000-0000-0000-000000000000' FROM public."HrdPayrollSetting" WHERE "IsActive" AND NOT "IsDelete" AND NOT EXISTS (SELECT 1 FROM public."PrlBpjsPolicy" WHERE "PolicyVersion" = '<versi>');` Pengajuan dan pengesahan tetap lewat layar (pemisahan pengaju-pengesah dijaga aplikasi).
- **Kebijakan tarif PPh**: **hanya UI** — Payroll › **Kebijakan Tarif PPh** (tabel PTKP/TER/Pasal 17 ditempel dari peraturan). Tidak ada skrip SQL berisi angka tarif karena angka regulasi tidak boleh bersumber dari saya; isi dari lampiran peraturan yang berlaku lalu verifikasi sebelum diajukan.

## 8. Asumsi dan keputusan yang masih terbuka

| # | Butir | Status |
| --- | --- | --- |
| 1 | **Isi tabel TER, PTKP, Pasal 17, dan parameter** belum ada: PPh otomatis tetap menahan sampai HRD memasukkannya dan Finance mengesahkannya. Nilai harus dicocokkan ke PMK 168/2023 dan PP 58/2023 oleh pemilik pajak; **tidak diverifikasi pada task ini** | NEEDS OWNER ACTION |
| 2 | **Bruto kena pajak**: komponen `IsTaxable` dikurangi potongan cuti tanpa gaji/mangkir/potongan kehadiran (penghasilan yang tidak dibayarkan), ditambah premi pemberi kerja dan dikurangi iuran pensiun menurut daftar program pada kebijakan | NEEDS CONFIRMATION |
| 3 | **Masa pajak terakhir** = Desember atau bulan `ResignDate`; biaya jabatan memakai jumlah masa pajak yang punya run + 1; selisih negatif = penahan (pengembalian kelebihan potong butuh keputusan manual) | NEEDS CONFIRMATION |
| 4 | Tanpa formula pada mesin (penahan): pegawai tidak tetap (harian/mingguan/satuan), metode GrossUp, masa pajak terakhir untuk pegawai dengan pemberi kerja sebelumnya, TER harian (tabel disimpan, belum dipakai). Nominal manual disetujui atau pengecualian tetap jalur yang sah | BLOCKED per jenis |
| 5 | Penerima bukan pegawai (dokter mitra): cara penerapan (kumulatif atau per pembayaran) dan dasar pengenaan ditentukan kebijakan; kebenaran regulasinya **bergantung isi kebijakan yang disahkan**, bukan kode | NEEDS OWNER ACTION |
| 6 | **Dampak operasional**: sampai kebijakan PPh/BPJS disahkan dan tunjangan diklasifikasikan, penerima terkait **menahan pengajuan Payroll Run**. Itu perilaku yang diminta, tetapi akan terlihat segera pada run berikutnya | DIKETAHUI |
| 7 | Saklar jalur lama bawaan **false**; riwayat database jalur lama belum diperiksa. Run lama yang masih menggantung tidak dapat dilanjutkan sampai saklar dinyalakan | NEEDS OWNER ACTION |
| 8 | Run yang disahkan sebelum task ini tidak punya rincian akumulasi pada snapshot; bila pegawai beralih ke PPh otomatis di tengah tahun, masa pajak terakhirnya menahan sampai diselesaikan dengan nominal manual disetujui | DIKETAHUI |

## 9. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build`, `dotnet restore`, `dotnet ef`, migration, DDL | **NOT RUN** (dilarang) |
| Automated test backend | `AUTOMATED TEST: NOT APPLICABLE` — tidak ada project test (TEST_POLICY). Kontrak dikunci dari Frontend (`payroll-policy.test.mjs`, 12 uji, membaca source BE) |
| Tinjauan kompilasi lewat pembacaan | Konstruktor dan DI (`PayrollCalculationService`, `PayrollCompensationService`, `PayrollDataReadinessService`, `WfpTaxController`), tipe tuple, `with` pada record, nama lokal, dan tabrakan nama antar-namespace diperiksa manual. **Belum dikompilasi**; risiko galat kompilasi tetap ada |
| Uji hitung, API, runtime, TC | **NOT RUN** |

`git status`: tidak ada yang di-stage atau di-commit. Perubahan tugas sebelumnya masih berada di working tree yang sama.

## 10. Migration yang dibuat pemilik dan penggantian namanya (18 November 2026)

Pemilik men-generate migration dari model ini di luar sesi agent dengan nama `ProbeCheck2` (`Migrations/20261118035706_ProbeCheck2.cs` dan `.Designer.cs`). Isinya mencakup seluruh skema `PAY-BE-FIX-004` dan `PAY-BE-FIX-005` (kolom `PrlTax`, `PrlInsurance`, `MstAllowanceType`, `PrlTransportAllowancePolicy`; tabel `PrlTaxPolicy`, `PrlBpjsPolicy`) **ditambah tabel `PrlWorkforceAllowance`** (Tunjangan Pegawai, DDL-nya semula di `PAY-BE-FIX-002`). Atas permintaan pemilik, namanya diganti menjadi `AddPayrollStatutoryPolicyTaxInsuranceAndWorkforceAllowance`:

- `Migrations/20261118035706_AddPayrollStatutoryPolicyTaxInsuranceAndWorkforceAllowance.cs` dan `.Designer.cs`; nama kelas `partial class` pada keduanya dan atribut `[Migration("20261118035706_AddPayrollStatutoryPolicyTaxInsuranceAndWorkforceAllowance")]` pada Designer ikut diganti. Timestamp tidak diubah.
- Pemeriksaan: tidak ada sisa `ProbeCheck` pada kode; BOM UTF-8 dan akhir baris CRLF utuh (jumlah CR sama dengan jumlah baris pada kedua berkas); ukuran berkas bertambah tepat sebesar selisih panjang nama. Isi `Up`/`Down` dan `ApplicationDbContextModelSnapshot.cs` tidak disentuh. **Belum dikompilasi dan belum diterapkan** (`dotnet build`/`dotnet ef` NOT RUN).

> **Perhatian bila migration ini sudah pernah diterapkan dengan nama lama.** ID migration adalah `<timestamp>_<nama>`, jadi rename mengubah ID. Bila `dotnet ef database update` sudah dijalankan dengan nama `ProbeCheck2`, EF akan menganggap ID baru belum diterapkan dan mencoba membuat ulang kolom/tabel (gagal "already exists"). Perbaiki riwayatnya lebih dulu pada database yang bersangkutan (langkah pemilik; saya tidak menjalankan DML):
>
> ```sql
> UPDATE public."__EFMigrationsHistory"
>    SET "MigrationId" = '20261118035706_AddPayrollStatutoryPolicyTaxInsuranceAndWorkforceAllowance'
>  WHERE "MigrationId" = '20261118035706_ProbeCheck2';
> ```
>
> Bila DDL pada bagian 7 sudah dijalankan manual lebih dulu, migration ini akan gagal pada objek yang sudah ada; pilih satu jalur (DDL manual **atau** migration) per database.
