# Laporan Perubahan Backend — `PAY-BE-FIX-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-021` (ad-hoc lanjutan seri `PAY-BE-FIX`); melaksanakan fase F2 dan F3 dari `PAY-BE-FIX-012` / `PAY-BE-FIX-020` |
| Judul | Kebijakan Tunjangan umum per Grade Gaji dan penetapan tunjangan pegawai berbasis kebijakan (backend) |
| Area / Modul | Corporate / PayrollManagement, prefix `Prl` (registry `ACTIVE`), `NEW CODE`. Tidak ada entity `Mst`/`Trx` baru |
| Branch / HEAD | BE `Yasmina` `ab340bc7de4e9a041ca3a8e0289dc910f417ec82` (dikonfirmasi pemilik 23 Nov 2026); working tree berisi pekerjaan `PAY-BE-FIX-015` s.d. `019` yang belum di-commit |
| Klasifikasi | `HEAVY` (3 entity, 2 tabel baru, kolom baru pada tabel penetapan, 4 endpoint, jalur uang). Dikerjakan hanya sebagai F2+F3; F4–F7 tidak |
| Task mode | `BACKEND` |
| Tanggal | 23 November 2026 |
| Status | 🟡 **Source selesai; belum dibuild.** DDL wajib diterapkan pemilik SEBELUM backend dijalankan: entity `PrlWorkforceAllowance` kini membaca 4 kolom baru, sehingga seluruh query tunjangan pegawai gagal bila DDL belum ada |

## 1. Jawaban pemilik yang dipakai

| Pertanyaan | Jawaban | Dampak |
| --- | --- | --- |
| Branch | `Yasmina` / `yasmina` | Dipakai |
| Cakupan | F2 + F3 | Entity + DDL + endpoint eligible-types, policies, preview, POST. **Tanpa** frontend, mesin hitung, atau penghapusan jalur Transportasi |
| Transportasi, metode apa yang rutin | "Semua metode" | **Tidak dapat dipenuhi penuh pada fase ini; lihat bagian 2** |

## 2. Hal yang sengaja tidak dilakukan (kondisi henti), dan mengapa

Pemilik memilih "semua metode transport lewat alur umum". Mesin hitung (`PayrollCalculationService.cs:1239`) **melewati** semua penetapan pegawai berkategori Transport, dan membayar **setiap** penetapan `Approved` bukan-Transport tiap periode. Akibatnya, bila POST baru membuat `PrlWorkforceAllowance` untuk:

- **Transportasi**: tunjangan **tidak terbayar** (dilewati), sedangkan jalur lama `PrlTransportAllowance` tetap membayar; atau, bila jalur lama dibiarkan, terbayar ganda saat mesin hitung diubah.
- **Shift, Siaga, Risiko** (dan Transport `PerAttendance`/`PerDay`/`Reimbursement`): tunjangan **terbayar otomatis tiap bulan tanpa aktivitas**, melanggar aturan "pemilihan kebijakan bukan perintah bayar".

Karena itu POST baru hanya menerima jenis **RecurringPolicy** (rutin, berulang, kategori bukan Shift/OnCall/Hazard/Transport/Variable, metode bukan input manual). Jenis lain **tetap dapat dipilih** (`Selectable = true`) dan tampil dengan alasan; Transportasi tetap menampilkan kebijakan lamanya (`Source = TransportLegacy`) hanya-baca. Membuka transport sepenuhnya membutuhkan F5 (pemindahan data) dan F6 (mesin hitung membaca sumber baru), yang belum dikerjakan. **NEEDS CONFIRMATION**: lanjutkan F5/F6 untuk transport `FixedMonthly` dulu?

## 3. Yang diimplementasikan (IMPLEMENTED di source)

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Models/PrlAllowancePolicy.cs` | **Baru**: `PrlAllowancePolicy`, `PrlAllowancePolicyGradeRate`, `WorkforceAllowanceAssignmentSources` |
| `.../Models/PrlWorkforceAllowance.cs` | +`AssignmentSource`, `AllowancePolicyId`, `AllowancePolicyGradeRateId`, `SalaryGradeId` + navigasi |
| `Repositories/Configurations/Corporate/PayrollManagement/PrlAllowancePolicyConfiguration.cs` | **Baru**: dua konfigurasi dengan CHECK/FK/indeks bernama (sama dengan DDL) |
| `.../PrlWorkforceAllowanceConfiguration.cs` | 2 CHECK, 3 FK, 3 indeks bernama |
| `Repositories/ApplicationDbContext.cs` | `DbSet` `PrlAllowancePolicies`, `PrlAllowancePolicyGradeRates` |
| `.../DTOs/PayrollAllowancePolicyAssignmentDtos.cs` | **Baru**: kontrak GET/POST |
| `.../Services/PayrollWorkforceAllowanceService.cs` | Kelas dijadikan `partial` (tanpa perubahan lain) |
| `.../Services/PayrollWorkforceAllowanceService.Policy.cs` | **Baru**: klasifikasi jenis, grade efektif, resolusi kebijakan, GET, POST |
| `.../Controllers/PayrollWorkforceAllowancesController.cs` | 4 endpoint baru; izin existing (`Read`, `Create`) |

Tidak ada migration, tidak ada perubahan `ModelSnapshot`.

## 4. Kontrak (IMPLEMENTED di source; belum dijalankan)

Dasar: `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/allowance-assignments`. GET tidak menulis data.

| Metode | Rute | Izin | Fungsi |
| --- | --- | --- | --- |
| GET | `/eligible-types` | `Read` | Semua jenis aktif + `Kind`, `Selectable`, `Assignable`, `Payable`, `HasPolicyForGrade`, `Reason`; Grade Gaji efektif pegawai |
| GET | `/policies?allowanceTypeId=` | `Read` | Kebijakan aktif untuk jenis itu yang punya tarif untuk Grade Gaji efektif (`General`), atau kebijakan transportasi lama hanya-baca (`TransportLegacy`, `Assignable=false`) |
| GET | `/preview?allowanceTypeId=&allowancePolicyId=` | `Read`; nominal hanya bila `ViewAmount` | Grade, tarif, metode, batas, masa berlaku **hasil resolusi backend**, `IsOpenEnded` eksplisit, sumber tiap batas tanggal |
| POST | `/from-policy` | `Create` | Body: `AllowanceTypeId`, `AllowancePolicyId`, `Notes?`. Mengembalikan penetapan (`WorkforceAllowanceItemResponse`) |

**Resolusi (satu tempat, dipakai preview dan simpan):** Grade Gaji = `WfpSalaryAssignment` utama aktif yang berlaku hari ini (tanpa persetujuan tambahan; bukan `EmployeeGradeId`). Mulai = paling akhir dari hari ini, mulai kebijakan, mulai jenis; tarif harus sudah berlaku saat itu. Akhir = paling awal dari kebijakan, tarif, jenis, penetapan gaji; kosong **hanya** bila semuanya kosong dan dinyatakan `IsOpenEnded`. Nilai: metode dari kebijakan; nominal/persen dari tarif grade; batas = yang terkecil dari tarif dan jenis; komponen = kebijakan lalu jenis. Cakupan legal entity/fasilitas kebijakan dicocokkan dengan Struktur Gaji pegawai dan hanya menolak bila keduanya terisi dan berbeda.

**POST:** transaksi Serializable + advisory lock per pegawai (kunci yang sama dengan penetapan langsung). Permintaan identik (jenis, kebijakan, grade, tanggal mulai, status hidup) mengembalikan penetapan yang ada (idempoten). Selain itu memakai penjaga yang sama dengan penetapan langsung: tidak boleh beririsan dengan penetapan hidup jenis yang sama (409) dan tidak boleh menduplikasi Struktur Gaji (409). Status awal mengikuti `RequiresApproval` jenis (kebijakan tidak menimpa). Audit tanpa nominal: jenis, kebijakan, revisi, tarif, grade, status, pelaku.

## 5. DDL (diterapkan pemilik; idempoten, satu transaksi)

Urutan: DDL → backend. Nama constraint/indeks paling panjang 51 karakter (batas PostgreSQL 63). Tanggal disimpan `timestamptz` tengah malam UTC (sama dengan `PrlWorkforceAllowance`).

```sql
BEGIN;

-- 1. Kebijakan Tunjangan umum
CREATE TABLE IF NOT EXISTS "public"."PrlAllowancePolicy" (
    "Id" uuid NOT NULL,
    "AllowanceTypeId" uuid NOT NULL,
    "PolicyCode" varchar(50) NOT NULL,
    "PolicyName" varchar(150) NOT NULL,
    "CalculationMethod" varchar(30) NOT NULL,
    "CurrencyCode" varchar(3) NOT NULL DEFAULT 'IDR',
    "PayrollComponentId" uuid NULL,
    "LegalEntityId" uuid NULL,
    "HospitalSiteId" uuid NULL,
    "EffectiveStartDate" timestamp with time zone NOT NULL,
    "EffectiveEndDate" timestamp with time zone NULL,
    "RevisionNo" integer NOT NULL DEFAULT 1,
    "SupersedesPolicyId" uuid NULL,
    "Description" varchar(500) NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreateDateTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreateBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "UpdateDateTime" timestamp with time zone NULL,
    "UpdateBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "DeleteDateTime" timestamp with time zone NULL,
    "DeleteBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "CancelDateTime" timestamp with time zone NULL,
    "CancelBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "IsCancel" boolean NOT NULL DEFAULT false,
    "IsDelete" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_PrlAllowancePolicy" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_PrlAllowancePolicy_Method" CHECK ("CalculationMethod" IN ('FixedAmount','PercentageOfBaseSalary')),
    CONSTRAINT "CK_PrlAllowancePolicy_Dates" CHECK ("EffectiveEndDate" IS NULL OR "EffectiveEndDate" >= "EffectiveStartDate"),
    CONSTRAINT "CK_PrlAllowancePolicy_Revision" CHECK ("RevisionNo" >= 1),
    CONSTRAINT "FK_PrlAllowancePolicy_AllowanceType" FOREIGN KEY ("AllowanceTypeId") REFERENCES "public"."MstAllowanceType" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrlAllowancePolicy_PayrollComponent" FOREIGN KEY ("PayrollComponentId") REFERENCES "public"."MstPayrollComponent" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrlAllowancePolicy_LegalEntity" FOREIGN KEY ("LegalEntityId") REFERENCES "public"."MstLegalEntity" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrlAllowancePolicy_HospitalSite" FOREIGN KEY ("HospitalSiteId") REFERENCES "public"."MstHospitalSite" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrlAllowancePolicy_Supersedes" FOREIGN KEY ("SupersedesPolicyId") REFERENCES "public"."PrlAllowancePolicy" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_PrlAllowancePolicy_PolicyCode" ON "public"."PrlAllowancePolicy" ("PolicyCode") WHERE "IsDelete" = false;
CREATE INDEX IF NOT EXISTS "IX_PrlAllowancePolicy_AllowanceType_Active" ON "public"."PrlAllowancePolicy" ("AllowanceTypeId", "IsActive", "IsDelete");
CREATE INDEX IF NOT EXISTS "IX_PrlAllowancePolicy_PayrollComponentId" ON "public"."PrlAllowancePolicy" ("PayrollComponentId");
CREATE INDEX IF NOT EXISTS "IX_PrlAllowancePolicy_LegalEntityId" ON "public"."PrlAllowancePolicy" ("LegalEntityId");
CREATE INDEX IF NOT EXISTS "IX_PrlAllowancePolicy_HospitalSiteId" ON "public"."PrlAllowancePolicy" ("HospitalSiteId");
CREATE INDEX IF NOT EXISTS "IX_PrlAllowancePolicy_SupersedesPolicyId" ON "public"."PrlAllowancePolicy" ("SupersedesPolicyId");

-- 2. Tarif kebijakan per Grade Gaji (MstSalaryGrade.Id; BUKAN grade pegawai)
CREATE TABLE IF NOT EXISTS "public"."PrlAllowancePolicyGradeRate" (
    "Id" uuid NOT NULL,
    "AllowancePolicyId" uuid NOT NULL,
    "SalaryGradeId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL DEFAULT 0,
    "Percentage" numeric(7,2) NOT NULL DEFAULT 0,
    "MaximumAmount" numeric(18,2) NULL,
    "EffectiveStartDate" timestamp with time zone NOT NULL,
    "EffectiveEndDate" timestamp with time zone NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreateDateTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreateBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "UpdateDateTime" timestamp with time zone NULL,
    "UpdateBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "DeleteDateTime" timestamp with time zone NULL,
    "DeleteBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "CancelDateTime" timestamp with time zone NULL,
    "CancelBy" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    "IsCancel" boolean NOT NULL DEFAULT false,
    "IsDelete" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_PrlAllowancePolicyGradeRate" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_PrlAllowancePolicyGradeRate_Amounts" CHECK ("Amount" >= 0 AND "Percentage" >= 0 AND "Percentage" <= 100 AND ("MaximumAmount" IS NULL OR "MaximumAmount" > 0)),
    CONSTRAINT "CK_PrlAllowancePolicyGradeRate_Value" CHECK ("Amount" > 0 OR "Percentage" > 0),
    CONSTRAINT "CK_PrlAllowancePolicyGradeRate_Dates" CHECK ("EffectiveEndDate" IS NULL OR "EffectiveEndDate" >= "EffectiveStartDate"),
    CONSTRAINT "FK_PrlAllowancePolicyGradeRate_Policy" FOREIGN KEY ("AllowancePolicyId") REFERENCES "public"."PrlAllowancePolicy" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrlAllowancePolicyGradeRate_SalaryGrade" FOREIGN KEY ("SalaryGradeId") REFERENCES "public"."MstSalaryGrade" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_PrlAllowancePolicyGradeRate_Policy_Grade_Start" ON "public"."PrlAllowancePolicyGradeRate" ("AllowancePolicyId", "SalaryGradeId", "EffectiveStartDate") WHERE "IsDelete" = false;
CREATE INDEX IF NOT EXISTS "IX_PrlAllowancePolicyGradeRate_SalaryGrade_Active" ON "public"."PrlAllowancePolicyGradeRate" ("SalaryGradeId", "IsActive", "IsDelete");

-- 3. Penetapan pegawai terpadu: kolom tautan (data lama otomatis 'Direct')
ALTER TABLE "public"."PrlWorkforceAllowance"
    ADD COLUMN IF NOT EXISTS "AssignmentSource" varchar(20) NOT NULL DEFAULT 'Direct',
    ADD COLUMN IF NOT EXISTS "AllowancePolicyId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "AllowancePolicyGradeRateId" uuid NULL,
    ADD COLUMN IF NOT EXISTS "SalaryGradeId" uuid NULL;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlWorkforceAllowance_AssignmentSource') THEN
        ALTER TABLE "public"."PrlWorkforceAllowance" ADD CONSTRAINT "CK_PrlWorkforceAllowance_AssignmentSource"
            CHECK ("AssignmentSource" IN ('Direct','Policy'));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'CK_PrlWorkforceAllowance_PolicySourceHasLinks') THEN
        ALTER TABLE "public"."PrlWorkforceAllowance" ADD CONSTRAINT "CK_PrlWorkforceAllowance_PolicySourceHasLinks"
            CHECK ("AssignmentSource" <> 'Policy' OR ("AllowancePolicyId" IS NOT NULL AND "AllowancePolicyGradeRateId" IS NOT NULL AND "SalaryGradeId" IS NOT NULL));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PrlWorkforceAllowance_AllowancePolicy') THEN
        ALTER TABLE "public"."PrlWorkforceAllowance" ADD CONSTRAINT "FK_PrlWorkforceAllowance_AllowancePolicy"
            FOREIGN KEY ("AllowancePolicyId") REFERENCES "public"."PrlAllowancePolicy" ("Id") ON DELETE RESTRICT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PrlWorkforceAllowance_AllowancePolicyGradeRate') THEN
        ALTER TABLE "public"."PrlWorkforceAllowance" ADD CONSTRAINT "FK_PrlWorkforceAllowance_AllowancePolicyGradeRate"
            FOREIGN KEY ("AllowancePolicyGradeRateId") REFERENCES "public"."PrlAllowancePolicyGradeRate" ("Id") ON DELETE RESTRICT;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_PrlWorkforceAllowance_SalaryGrade') THEN
        ALTER TABLE "public"."PrlWorkforceAllowance" ADD CONSTRAINT "FK_PrlWorkforceAllowance_SalaryGrade"
            FOREIGN KEY ("SalaryGradeId") REFERENCES "public"."MstSalaryGrade" ("Id") ON DELETE RESTRICT;
    END IF;
END $$;
CREATE INDEX IF NOT EXISTS "IX_PrlWorkforceAllowance_AllowancePolicyId" ON "public"."PrlWorkforceAllowance" ("AllowancePolicyId");
CREATE INDEX IF NOT EXISTS "IX_PrlWorkforceAllowance_AllowancePolicyGradeRateId" ON "public"."PrlWorkforceAllowance" ("AllowancePolicyGradeRateId");
CREATE INDEX IF NOT EXISTS "IX_PrlWorkforceAllowance_SalaryGradeId" ON "public"."PrlWorkforceAllowance" ("SalaryGradeId");

COMMIT;
```

### 5.1 Template pengisian kebijakan (ilustrasi; nilai harus dari HRD/Finance)

Belum ada layar pengelola kebijakan umum (F2 layar belum dikerjakan), sehingga kebijakan dan tarif diisi lewat SQL aman-ulang berikut atau layar yang akan dibuat kemudian.

```sql
-- TEMPLATE pengisian kebijakan (aman diulang). Nilai di bawah HANYA ILUSTRASI: ganti kode jenis, kode grade, dan nominal dengan keputusan
-- HRD dan Finance. Tanggal wajib tengah malam UTC. Jalankan sebagai SATU transaksi setelah DDL di atas.
BEGIN;

INSERT INTO "public"."PrlAllowancePolicy"
    ("Id", "AllowanceTypeId", "PolicyCode", "PolicyName", "CalculationMethod", "CurrencyCode", "EffectiveStartDate")
SELECT gen_random_uuid(), t."Id", 'POL-CONTOH-001', 'Contoh Kebijakan Tunjangan Bulanan', 'FixedAmount', 'IDR', TIMESTAMPTZ '2026-01-01 00:00:00+00'
FROM "public"."MstAllowanceType" t
WHERE t."AllowanceTypeCode" = 'KODE_JENIS_TUNJANGAN' AND t."IsDelete" = false
  AND NOT EXISTS (SELECT 1 FROM "public"."PrlAllowancePolicy" p WHERE p."PolicyCode" = 'POL-CONTOH-001' AND p."IsDelete" = false);

INSERT INTO "public"."PrlAllowancePolicyGradeRate"
    ("Id", "AllowancePolicyId", "SalaryGradeId", "Amount", "EffectiveStartDate")
SELECT gen_random_uuid(), p."Id", g."Id", v."Amount", TIMESTAMPTZ '2026-01-01 00:00:00+00'
FROM "public"."PrlAllowancePolicy" p
JOIN (VALUES ('KODE_GRADE_A', 300000::numeric), ('KODE_GRADE_B', 400000::numeric)) AS v("GradeCode", "Amount") ON true
JOIN "public"."MstSalaryGrade" g ON g."SalaryGradeCode" = v."GradeCode" AND g."IsDelete" = false
WHERE p."PolicyCode" = 'POL-CONTOH-001' AND p."IsDelete" = false
  AND NOT EXISTS (SELECT 1 FROM "public"."PrlAllowancePolicyGradeRate" r
                  WHERE r."AllowancePolicyId" = p."Id" AND r."SalaryGradeId" = g."Id" AND r."IsDelete" = false);

COMMIT;

-- Pemeriksaan tarif beririsan (harus 0 baris):
SELECT a."Id", b."Id" FROM "public"."PrlAllowancePolicyGradeRate" a
JOIN "public"."PrlAllowancePolicyGradeRate" b
  ON a."AllowancePolicyId" = b."AllowancePolicyId" AND a."SalaryGradeId" = b."SalaryGradeId" AND a."Id" < b."Id"
WHERE a."IsDelete" = false AND b."IsDelete" = false AND a."IsActive" AND b."IsActive"
  AND a."EffectiveStartDate" <= COALESCE(b."EffectiveEndDate", 'infinity'::timestamptz)
  AND b."EffectiveStartDate" <= COALESCE(a."EffectiveEndDate", 'infinity'::timestamptz);
```

## 6. Pemisahan status

| Kategori | Isi |
| --- | --- |
| IMPLEMENTED (source) | Bagian 3 dan 4. Belum dibuild |
| PROPOSED | Layar pengelola kebijakan umum; provenance pada daftar tunjangan pegawai; F4 (frontend), F5, F6, F7 |
| NOT VERIFIED | `dotnet build`, perilaku runtime, DDL terbukti diterapkan, RBAC data nyata |
| BLOCKED_BY_SCHEMA | Tidak ada pada source ini; **runtime bergantung pada DDL bagian 5** |
| NEEDS CONFIRMATION | Transport "semua metode" (bagian 2); pemetaan `EmployeeGradeId`→`SalaryGradeId` untuk kebijakan transport lama; cakupan legal entity/fasilitas pegawai (profil pegawai tidak menyimpannya, dipakai dari Struktur Gaji) |

## 7. Static review (subset yang relevan dengan source ini)

| Skenario (nomor prompt) | Status |
| --- | --- |
| 5–9, 12 (dropdown jenis/kebijakan, kebijakan sesuai Grade Gaji, `SalaryGradeId` ≠ `EmployeeGradeId`, masa berlaku ter-resolve) | Static Review Passed pada source (resolusi memakai `WfpSalaryAssignment.SalaryGradeId`; tidak ada pemetaan ke `EmployeeGradeId`); belum diverifikasi runtime |
| 9, 17–18 (nominal dari backend; POST hanya identitas; backend validasi ulang) | Static Review Passed (DTO POST hanya `AllowanceTypeId`, `AllowancePolicyId`, `Notes`) |
| 19–20, 32 (masuk sumber benar; tanpa duplikat; race) | Static Review Passed (`PrlWorkforceAllowance`, advisory lock, idempoten, penjaga irisan) |
| 21 (struktur tidak ganda) | Static Review Passed (`EnsureNotInSalaryStructureAsync`) |
| 22 (transport lama tidak ganda) | Static Review Passed untuk POST baru (Transport tidak dapat ditetapkan lewat jalur ini) |
| 25–28 (bonus, insentif, lembur, aktivitas) | Static Review Passed: `Variable`/`Activity`/`Manual` ditolak dengan alasan; mesin hitung tidak diubah |
| 29–30 (RBAC, `ViewAmount`) | Static Review Passed: izin existing; nominal preview `null` tanpa `ViewAmount`; audit tanpa nominal |
| 1–3, 10–11, 13–16, 33 (frontend) | Not Verified: frontend belum dikerjakan (F4) |
| 23–24 (grade berganti, payroll final utuh) | Not Verified: tanggal akhir dibatasi penetapan gaji; mesin hitung dan snapshot tidak disentuh |
| 31 (list/summary direfresh) | Not Verified (frontend) |
| 34–35 | Tidak ada orphan terdeteksi statis; tidak ada migration/snapshot |

## 8. Risiko

- **Urutan deploy**: DDL → backend. Backend tanpa DDL membuat query `PrlWorkforceAllowance` gagal.
- Penetapan Policy menyimpan provenance, tetapi **mesin hitung belum membacanya** (provenance pada hasil payroll adalah F6).
- Tarif beririsan tidak dapat dijadikan unique index; dijaga template/pemeriksaan DDL dan layanan pengelola kebijakan yang belum ada.
- `PrlAllowancePolicy.LegalEntityId`/`HospitalSiteId` dicocokkan dengan Struktur Gaji pegawai (profil pegawai tidak menyimpan cakupan).

## 9. Pernyataan

`dotnet build`, `npm run build`, migration, dan uji runtime **tidak dijalankan**. Tidak ada commit, push, atau deploy. `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis`; tes teks di repo frontend (`payroll-allowance-policy-assignment.test.mjs`) hanya mencocokkan teks source dan DDL.
