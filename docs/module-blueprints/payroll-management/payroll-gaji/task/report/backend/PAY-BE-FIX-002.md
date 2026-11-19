# Laporan Perubahan Backend — `PAY-BE-FIX-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-002` (ad-hoc, bukan kartu roadmap) |
| Judul | Tunjangan Pegawai: penetapan tunjangan tetap khusus satu pegawai berbasis Master Tunjangan, terintegrasi ke mesin hitung Payroll Run |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap, traceability, dan kontrak `PGJ-C1` tidak disentuh (preseden `PAY-BE-FIX-001`) |
| Trace | Perintah pemilik 2026-11-17 (master prompt "Penambahan Section Tunjangan Pegawai pada Detail Profil Penggajian"). Tanpa FR-PGJ/PRL-DEC bernomor — tidak dikarang |
| Contract version | **Tidak tercakup `PGJ-C1`.** Kontrak baru bersifat additive (satu controller baru, satu entity baru, satu komponen mesin baru); amandemen kontrak belum ditulis |
| Klasifikasi | `HIGH` — entity persisted baru, tabel baru tanpa migration, perubahan mesin hitung gaji |
| Task mode | `BACKEND` (Frontend lewat `PAY-FE-FIX-002`, target tulis terpisah) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-17 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan.** `dotnet build` NOT RUN (dilarang pada task ini; kode belum pernah dikompilasi). Migration **tidak dibuat** (dilarang) sehingga **fitur belum dapat dipakai sebelum DDL pada bagian 6 diterapkan**. Uji API dan runtime NOT RUN |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`: `Corporate | PayrollManagement / Payroll | Prl | ACTIVE`). Entity baru `PrlWorkforceAllowance` |
| Keberlakuan | `NEW CODE` untuk entity, konfigurasi, DTO, service, dan controller baru; `TOUCHED LEGACY` untuk `PayrollCalculationService` |
| QBE yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-CFG-001` (`IEntityTypeConfiguration`), `QBE-NAM-001/002` (prefix `Prl`, bukan `Trx`), `QBE-SVC-001` (controller hanya memanggil service), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001/AUD-001` (audit memuat aktor), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-PAGE-001` |
| Checker QBE | `Invoke-QbeConformanceCheck.ps1 -Mode ReportOnly` pada lima berkas baru/diubah: `VIOLATION 0, REVIEW 0`, `Final result: PASS`. Catatan: checker melaporkan `Files evaluated: 1` (bukan lima), jadi cakupannya sebagian |
| Hak akses | Resource baru `WorkforceAllowanceAssignment` dengan aksi `Read`, `ViewAmount`, `Create`, `Update`, `Submit`, `Approve`, `Reject`, `Deactivate`. Tanpa `IsInRole`, nama peran, atau `UserType`. Izin `ReadDetail` pada `PayrollCompensation` **tidak** memberi hak mengubah tunjangan |

---

## 1. Hasil pemeriksaan entity existing

| Entity | Peran | Cocok untuk penetapan per pegawai? |
| --- | --- | --- |
| `MstAllowanceType` | Master jenis tunjangan (metode `Fixed`, `Percentage`, `PolicyBased`, `ManualInput`) | Referensi saja; tidak diubah |
| `MstSalaryStructureAllowance` | Tunjangan tetap per **Struktur Gaji**, berlaku untuk semua pemilik struktur | **Tidak** (menyimpan tambahan satu pegawai di sini akan mengubah pegawai lain) |
| `PrlPayroll.TotalAllowance` | Angka agregat kompatibilitas | **Tidak** (tanpa jenis, sumber, persetujuan, masa berlaku) |
| `PrlPayrollVariableInput` | Input terikat satu payroll run | **Tidak** (bukan tunjangan berulang sebelum run dibuat) |
| `PrlPayrollEmployeeComponent` | Hasil hitung per run | Tujuan integrasi (komponen baru `PAY_WORKFORCE_ALLOWANCE`) |
| `PrlTransportAllowance` / `...Policy` | Transportasi berbasis kebijakan, satu per pegawai | Dipertahankan apa adanya; tidak dibuat implementasi kedua |

Kesimpulan: **tidak ada** entity penetapan tunjangan individual per `WorkforceProfileId + AllowanceTypeId`. Entity baru `PrlWorkforceAllowance` dibuat (tanpa duplikasi konsep).

## 2. Model data

`PrlWorkforceAllowance` (`public."PrlWorkforceAllowance"`): `Id`, `WorkforceProfileId`, `AllowanceTypeId`, `PayrollComponentId?`, `CalculationMethod` (`FixedAmount` | `PercentageOfBaseSalary`, konstanta yang sama dengan tunjangan struktur), `Amount`, `Percentage`, `CurrencyCode`, snapshot syarat master (`IsTaxable`, `IsProrated`, `RequiresApproval`, `MaximumAmount`), `EffectiveStartDate`, `EffectiveEndDate?`, `AllowanceStatus` (`Draft`, `PendingApproval`, `Approved`, `Rejected`, `Inactive`), `StatusReason`, `SubmittedAt/By`, `DecidedAt/By`, `Notes`, `IsActive`, dan kolom `IdentityModel`. Pemilik data adalah `WorkforceProfileId`: tidak bergantung dan tidak membuat `PrlPayroll`.

Snapshot disalin dari master saat penetapan dibuat/diubah supaya perubahan master tidak diam-diam mengubah syarat yang sudah disetujui. Nominal dan persen adalah nilai yang ditetapkan untuk pegawai itu.

## 3. Kontrak API

Dasar: `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/allowance-assignments`. Arketipe: sub-proses ter-scope induk (standar transaksi 2.3). Tanpa `DELETE` dan tanpa `GET /options` (jenis tunjangan dibaca dari endpoint options Master Tunjangan yang sudah ada).

| Method | Path | Izin | Keterangan |
| --- | --- | --- | --- |
| `GET` | `/` | `Read` | Gabungan tiga sumber (Struktur Gaji, Khusus Pegawai, Kebijakan/transportasi), `PagedResult`, filter `source`, `status`, `search`, `pageNumber`, `pageSize` (maks 100) |
| `GET` | `/summary` | `Read` | Jumlah aktif, menunggu persetujuan, per sumber, total estimasi (hanya bila `ViewAmount`), catatan |
| `GET` | `/{id}` | `Read` | Satu penetapan khusus pegawai |
| `GET` | `/{id}/amount` | `ViewAmount` | Pintu izin nominal agar kemampuan muncul di layar Akses Role |
| `POST` | `/` | `Create` | `{ allowanceTypeId, amount?, percentage?, effectiveStartDate, effectiveEndDate?, notes? }` |
| `PUT` | `/{id}` | `Update` | Sama + `expectedRowVersion` |
| `POST` | `/{id}/submit` | `Submit` | Draft/Rejected → PendingApproval |
| `POST` | `/{id}/approve` | `Approve` | PendingApproval → Approved |
| `POST` | `/{id}/reject` | `Reject` | PendingApproval → Rejected, `reason` wajib |
| `POST` | `/{id}/deactivate` | `Deactivate` | → Inactive, `reason` wajib |

Respons item memuat `sourceType`, `sourceLabel`, `status`, `isEffectiveNow`, nominal/persen/maksimum/estimasi (**null bila tanpa `ViewAmount`, tidak pernah 0**), `usedInPayroll`, `isEditable`, `lockReason`, `availableActions` (bantuan tampilan; setiap endpoint memeriksa ulang), `managementTarget` (`SalaryStructure` | `TransportAllowance`), `managementReferenceId`, dan `rowVersion`. Kode status: `400` validasi/transisi tidak sah, `403`, `404`, `409` bentrok versi, tumpang-tindih, atau duplikasi struktur.

## 4. Aturan bisnis yang dijaga server

1. Pegawai harus ada dan aktif di HRD (`MstWorkforceProfile`); 404 bila tidak. Profil payroll **tidak** diperlukan dan **tidak** dibuat.
2. Jenis tunjangan harus ada, aktif, `IsRecurring`, bermetode `Fixed` atau `Percentage`, bukan kategori `Shift/OnCall/Hazard/Transport/Variable`, tidak dipakai kebijakan shift/siaga/risiko/transport aktif, dan komponen payroll-nya (bila ada) aktif. `PolicyBased`/`ManualInput` ditolak dengan arahan ke kebijakan atau Penghasilan Variabel.
3. Nominal `> 0` dan tidak melebihi `MaximumAmount` master; persen `> 0` dan `≤ 100`. Kategori, metode, mata uang, pajak, prorata, dan persetujuan **tidak dipercaya dari klien**.
4. Tanggal mulai berada dalam masa berlaku master; bila master berakhir, tanggal akhir wajib dan tidak melewatinya; akhir tidak lebih awal dari mulai.
5. Satu jenis paling banyak satu penetapan hidup (Draft/PendingApproval/Approved) pada rentang yang beririsan per pegawai (409).
6. Jenis yang sudah ada di Struktur Gaji pegawai pada rentang yang sama ditolak (409): tidak dihitung dua kali.
7. Status awal mengikuti `RequiresApproval` master: butuh persetujuan → `PendingApproval`; tidak butuh → `Approved` otomatis (dicatat sebagai keputusan sistem). Mengubah isi membatalkan persetujuan sebelumnya (kembali `Draft` untuk jenis yang butuh persetujuan).
8. Persetujuan memakai izin `Approve` sendiri; pengaju **boleh** menyetujui sendiri, sama dengan penghasilan variabel manual (`PAY-BE-20`).
9. Penetapan yang sudah pernah menjadi komponen payroll run (`SourceType = WorkforceAllowance`) tidak dapat diubah isinya; koreksinya menonaktifkan lalu membuat penetapan baru. Tidak ada penghapusan fisik.
10. Penulisan memakai transaksi `Serializable` + advisory lock per pegawai (`PayrollTransactionRunner`), token versi `PayrollVersionToken`, dan audit (`WorkforceAllowance.*`: identitas, status sebelum/sesudah, aktor, alasan; **tanpa nominal**).

## 5. Integrasi mesin hitung

Hanya `PayrollCalculationService` yang dipakai Payroll Run aktif (`PayrollRunService.CalculateAsync`) dan Simulasi; `PayrollEarningsCalculationService` milik alur "Modern" lama (`HumanResource/PayrollManagement`) tidak disentuh.

- Dimuat: `PrlWorkforceAllowances` yang `Approved`, aktif, tidak dihapus, dan berlaku pada periode.
- Dihitung: nominal tetap (dibatasi maksimum snapshot) atau gaji pokok penetapan **yang disetujui** × persen / 100 (per segmen bila gaji pokok berganti di tengah periode); diprorata hari berlaku / hari periode bila `IsProrated` snapshot dan kebijakan `ProrateFixedAllowances`; tanpa prorata, per jenis hanya penetapan yang berakhir paling akhir di jendela yang dibayar penuh.
- Dilewati dengan peringatan: jenis sudah tidak aktif di master, jenis sudah berasal dari Struktur Gaji (tidak ganda), mata uang berbeda, persentase tanpa gaji pokok yang disetujui. Dua penetapan jenis sama yang beririsan di periode adalah data tidak konsisten dan menggagalkan pegawai (bukan dibayar dua kali).
- Hasil: komponen `PAY_WORKFORCE_ALLOWANCE` (`SourceType = WorkforceAllowance`, `SourceId = PrlWorkforceAllowance.Id`), masuk `TotalRecurringEarning` dan `GrossPay`, `IsTaxable` mengikuti snapshot. Slip gaji membaca komponen lewat mekanisme yang sudah ada. Run final tidak dihitung ulang (`AcceptsInputs`), sehingga snapshotnya tidak berubah.
- Tunjangan berbasis kebijakan/aktivitas dan penghasilan variabel per periode **tidak** dihitung dari penetapan ini; jalurnya tetap kebijakan (transportasi) dan Penghasilan Variabel.

## 6. Kebutuhan skema (dependency deployment terpisah — migration TIDAK dibuat)

Instruksi pemilik: tanpa migration pada pekerjaan ini. Tabel harus ada **sebelum** build Backend ini berjalan: tanpa tabel, `GET allowance-assignments` **dan perhitungan Payroll Run/Simulasi** (yang memuat `PrlWorkforceAllowances`) gagal dengan "relation does not exist". Urutan rilis: DDL → Backend → Frontend. `Migrations/` dan model snapshot tidak diubah (`PendingModelChangesWarning` diabaikan di `Program.cs`, sehingga startup tidak gagal).

```sql
CREATE TABLE IF NOT EXISTS public."PrlWorkforceAllowance" (
    "Id"                  uuid NOT NULL,
    "WorkforceProfileId"  uuid NOT NULL,
    "AllowanceTypeId"     uuid NOT NULL,
    "PayrollComponentId"  uuid NULL,
    "CalculationMethod"   character varying(30)  NOT NULL,
    "Amount"              numeric(18,2) NOT NULL,
    "Percentage"          numeric(7,2)  NOT NULL,
    "CurrencyCode"        character varying(3)   NOT NULL DEFAULT 'IDR',
    "IsTaxable"           boolean NOT NULL DEFAULT TRUE,
    "IsProrated"          boolean NOT NULL DEFAULT TRUE,
    "RequiresApproval"    boolean NOT NULL DEFAULT FALSE,
    "MaximumAmount"       numeric(18,2) NULL,
    "EffectiveStartDate"  timestamp with time zone NOT NULL,
    "EffectiveEndDate"    timestamp with time zone NULL,
    "AllowanceStatus"     character varying(30)  NOT NULL DEFAULT 'Draft',
    "StatusReason"        character varying(500) NULL,
    "SubmittedAt"         timestamp with time zone NULL,
    "SubmittedByUserId"   uuid NULL,
    "DecidedAt"           timestamp with time zone NULL,
    "DecidedByUserId"     uuid NULL,
    "Notes"               character varying(1000) NULL,
    "IsActive"            boolean NOT NULL DEFAULT TRUE,
    "CreateDateTime"      timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreateBy"            uuid NOT NULL,
    "UpdateDateTime"      timestamp with time zone NULL,
    "UpdateBy"            uuid NOT NULL,
    "DeleteDateTime"      timestamp with time zone NULL,
    "DeleteBy"            uuid NOT NULL,
    "CancelDateTime"      timestamp with time zone NULL,
    "CancelBy"            uuid NOT NULL,
    "IsCancel"            boolean NOT NULL DEFAULT FALSE,
    "IsDelete"            boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_PrlWorkforceAllowance" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PrlWorkforceAllowance_MstWorkforceProfile_WorkforceProfileId"
        FOREIGN KEY ("WorkforceProfileId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_PrlWorkforceAllowance_MstAllowanceType_AllowanceTypeId"
        FOREIGN KEY ("AllowanceTypeId") REFERENCES public."MstAllowanceType" ("Id") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_PrlWorkforceAllowance_WorkforceProfileId_AllowanceTypeId_IsDelete"
    ON public."PrlWorkforceAllowance" ("WorkforceProfileId", "AllowanceTypeId", "IsDelete");

CREATE INDEX IF NOT EXISTS "IX_PrlWorkforceAllowance_AllowanceStatus_IsActive_EffectiveStartDate_EffectiveEndDate_IsDelete"
    ON public."PrlWorkforceAllowance" ("AllowanceStatus", "IsActive", "EffectiveStartDate", "EffectiveEndDate", "IsDelete");
```

DDL ini aman diulang (`IF NOT EXISTS`). Bila pemilik kelak memilih migration EF, nama tabel, kolom, dan indeks di atas harus cocok dengan `PrlWorkforceAllowanceConfiguration`. Agen tidak menjalankan DDL ini.

## 7. File

Baru: `Models/PrlWorkforceAllowance.cs`, `Repositories/Configurations/Corporate/PayrollManagement/PrlWorkforceAllowanceConfiguration.cs`, `DTOs/PayrollWorkforceAllowanceDtos.cs`, `Services/PayrollWorkforceAllowanceService.cs`, `Controllers/PayrollWorkforceAllowancesController.cs`.
Diubah: `Repositories/ApplicationDbContext.cs` (satu `DbSet`), `Program.cs` (satu registrasi DI), `Constants/PayrollStatusCatalog.cs` (kode komponen dan sumber), `Services/PayrollCalculationService.cs` (pemuatan, `BuildWorkforceAllowanceLines`, total, snapshot kebijakan).

## 8. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet build` / `dotnet restore` | **NOT RUN** (dilarang pada task ini) |
| Migration / `dotnet ef` | **NOT CREATED / NOT RUN** (dilarang; DDL pada bagian 6) |
| Automated test backend | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`TEST_POLICY.md`) |
| Checker QBE (`ReportOnly`) | PASS, 0 pelanggaran (cakupan 1 berkas, lihat preflight) |
| Kontrak ke Frontend | 43/43 uji Frontend lulus, termasuk pencocokan rute, izin, nama aksi, status, dan properti DTO dengan source ini (`tests/unit/payroll-workforce-allowance.test.mjs`, 0 dilewati) |
| Uji API, runtime, UAT | **NOT RUN** |

Pemeriksaan statis atas TC bisnis: tidak dibayar sebelum disetujui (TC-09: kueri mesin `AllowanceStatus == Approved`), tidak ganda dengan struktur (TC-22: `structureTypeIds`), tidak mengubah tunjangan pegawai lain (TC-07: penetapan terikat `WorkforceProfileId`), run final tidak berubah (TC-16: mesin tidak menghitung ulang run final; penetapan terpakai terkunci), tanpa izin `Create` ditolak (TC-17: `[AccessPermission]`), tanpa `ViewAmount` tanpa nominal (TC-18: respons null). **Semuanya belum dibuktikan saat dijalankan.**

## 9. Risiko dan hal yang belum selesai

- **Kode belum pernah dikompilasi.** Konstruktor proyeksi EF besar (`IndividualRow`, 23 argumen) dan langkah `PayrollTransactionRunner` adalah titik yang paling perlu dilihat saat build pertama.
- **Tabel wajib ada sebelum Backend dijalankan** (bagian 6), atau Payroll Run dan Simulasi gagal menghitung.
- Tunjangan yang **belum disetujui** tidak dibayar: jenis yang butuh persetujuan baru ikut payroll setelah `approve`. Mengubah isi penetapan yang sudah disetujui mengembalikannya ke `Draft` (pembayaran berhenti sampai disetujui ulang).
- Keputusan saya: pengaju boleh menyetujui sendiri (mengikuti `PAY-BE-20`); bila pemilik ingin pemisahan pengaju-pengesah, perlu aturan tambahan.
- Izin `AllowanceType : Read` tetap dibutuhkan penyusun penetapan agar dapat memuat pilihan jenis tunjangan (endpoint options milik master).
- Dokter yang punya `WorkforceProfileId` ikut tercakup (populasi sama dengan Profil Penggajian).
- Pengecualian struktur gaji global "tanpa aturan pengecualian yang jelas" ditolak sepenuhnya; belum ada mekanisme pengecualian.
- Amandemen kontrak `PGJ-C1`/blueprint untuk resource dan tabel baru belum ditulis (gerbang `G1`).
- `git status`: tidak ada yang di-stage atau di-commit.
