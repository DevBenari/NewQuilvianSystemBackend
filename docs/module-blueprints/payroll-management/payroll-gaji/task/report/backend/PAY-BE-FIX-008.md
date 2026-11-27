# Laporan Perubahan Backend — `PAY-BE-FIX-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-008` (ad-hoc, bukan kartu roadmap; melanjutkan seri `PAY-BE-FIX-001..007`) |
| Judul | Hybrid Allowance Management: satu aturan "gaji disetujui", siklus persetujuan penetapan gaji, deteksi hasil basi, dan perlindungan tunjangan yang sudah dipakai payroll |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas master prompt pemilik 2026-11-20; roadmap dan traceability tidak disentuh (preseden `PAY-BE-FIX-001..007`) |
| Dependency | **Tidak ada DDL, migration, atau perubahan data.** Frontend di `PAY-FE-FIX-008`. Tiga butir yang butuh skema ditandai `BLOCKED` di bagian 6 |
| Klasifikasi | `HIGH` — jalur penggajian, kontrak API, kewenangan persetujuan kompensasi |
| Task mode | `BACKEND` (frontend referensi read-only) |
| Branch / HEAD | `Yasmina` / `63ad7db73891be0cd39e7b8a8b78bb22637e3c11` (cocok dengan snapshot 20 Nov 2026; tidak berpindah branch). **Working tree sudah kotor sebelum task** (penghapusan BPJS Payroll dan lain-lain); file di bagian 4 adalah yang disentuh task ini |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-20 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan. `dotnet build` NOT RUN (dilarang task), migration TIDAK dibuat dan TIDAK dijalankan, tidak ada pengujian runtime.** QBE ReportOnly: PASS (20 berkas). Tiga butir BLOCKED menunggu keputusan skema |

## 1. Ringkasan untuk pembaca awam

Sebelum perbaikan, "gaji disetujui" punya **tiga arti berbeda** di tiga tempat: cukup `IsActive`, atau `ApprovedAt` saja, atau `ApprovedAt` dan `ApprovedByUserId`. Akibatnya penetapan gaji yang hanya aktif bisa terbaca sebagai disetujui di layar maupun di kesiapan payroll.

Perbaikan ini menyatukannya (`WfpSalaryAssignmentRules`): **disetujui = `ApprovedAt` dan `ApprovedByUserId` sama-sama terisi.** `IsActive` hanya berarti baris tidak dinonaktifkan.

**Satu keputusan pemilik menentukan bentuk akhirnya.** Saat audit ditemukan keputusan "peniadaan approval" (commit FE `718e2c33c`, 19 Nov 2026, `yasmina04`): penetapan aktif dianggap sah tanpa approval, dan tes unit melarang Payroll memfilter `ApprovedAt`. Itu berlawanan dengan P0-01/02/04 pada brief. Karena `Create` selalu menyimpan `ApprovedAt = null` dan tidak ditemukan layar "Setujui penetapan gaji" di UI HRD, **menegakkan approval sekarang akan menahan hampir semua penerima**. Pemilik memilih (2026-11-20): **sakelar kebijakan `Payroll:Policy:RequireApprovedSalaryAssignment`, bawaan `false`.** Kode approval-wajib sudah dibangun penuh, tetapi baru berlaku saat sakelar dinyalakan. Selama `false`, perilaku payroll 19 Nov tidak berubah.

## 2. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (prefix `Prl`, ACTIVE) dan `HumanResource/WorkforceCore` (prefix `Wfp`, ACTIVE / LEGACY) |
| Prefix / entity baru | **Tidak ada entity, tabel, kolom, atau indeks baru.** `QBE-MOD-002/003`, `QBE-NAM-004` tidak terpicu |
| Keberlakuan | `TOUCHED LEGACY` untuk seluruh berkas. `WfpSalaryAssignmentRules` adalah kelas murni baru (tanpa persistence) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (metadata `[AccessAction]`/`[AccessPermission]` seluruh aksi dipertahankan persis; tidak ada aksi baru), `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` (semua perubahan DTO aditif), `QBE-AUD-001` |
| Checker QBE | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode ReportOnly` pada 20 berkas: **VIOLATION 0, REVIEW 0, Final result PASS.** Checker hanya mengimplementasikan 7 aturan (ENT-001, NAM-001, CFG-001, CODE-002/003, MOD-002, SVC-001); ini bukan pengganti review |
| AUTOMATED TEST | `NOT APPLICABLE — tidak ada test baru backend dibuat`. Catatan: folder `Tests/` masih terlacak di repo (bertentangan dengan `TEST_POLICY.md` §1); satu tes memanggil `new PayrollHrdSourceReader(ctx)` sehingga konstruktor reader dijaga kompatibel (parameter opsional) |

## 3. Status defect brief

| ID | Defect | Status | Cara |
| --- | --- | --- | --- |
| P0-01 | `LoadSalariesAsync` mengunci `HasApproved:true / HasPending:false` | ✅ source | Dihitung dari data. Sakelar mati mempertahankan nilai lama secara sengaja (keputusan 19 Nov); menyala memakai nilai nyata. `PayrollReadinessEvaluator` kini memunculkan isu "belum disetujui" hanya bila tidak ada penetapan disetujui |
| P0-02 | `IsApproved = IsActive \|\| …` | ✅ source | `IsApproved` jujur di controller, `PayrollCompensationService` (riwayat dan `SalaryApproved`), dan ditambah `ApprovalStatus` + `ApprovalRequired` |
| P0-03 | `Update` mengubah gaji tanpa membatalkan approval | ✅ source | Perubahan struktur/grade/gaji pokok/mata uang/frekuensi/tanggal membatalkan approval; hanya **penutupan masa berlaku** (tanggal akhir diisi atau dimajukan) mempertahankannya; penetapan yang sudah dipakai payroll yang disahkan tidak boleh diubah (409) |
| P0-04 | Kalkulasi memuat penetapan tanpa filter approval | ✅ source (di balik sakelar) | `PayrollCalculationService`: menyala = hanya disetujui, belum disetujui jadi penahan atau sebab gagal; mati = perilaku 19 Nov |
| P0-05 | `ClearPrimaryAsync` menurunkan semua penetapan utama tanpa melihat tanggal | 🟡 **sebagian, BLOCKED skema** | Yang beririsan tanggal kini **ditolak 409** (tidak diam-diam diturunkan); yang tidak beririsan masih diturunkan karena indeks unik database (lihat 6.1). Hari yang tidak tertutup penetapan utama kini diperingatkan |
| P1-01 | Overlap hanya untuk struktur+grade sama | ✅ source | Penetapan utama aktif yang beririsan ditolak di `Create/Update/Approve/SetPrimary/UpdateStatus`; pemeriksaan lama dipertahankan |
| P1-02 | Tunjangan struktur diubah langsung | 🟡 **sebagian, BLOCKED skema** | Baris yang sudah dipakai payroll butuh konfirmasi eksplisit (`AcknowledgeUsedInPayroll`) dan ditandai; versi berlaku dan approval terpisah menunggu skema (6.2) |
| P1-03 | Pengaju dapat menyetujui sendiri | ✅ source (di balik kebijakan) | `PayrollWorkforceAllowanceService` dan `Approve` penetapan gaji kini memeriksa `RequireDifferentApprover` yang **sudah ada**; nilai bawaan `false` **tidak diubah** |
| P1-04 | Cadangan `PrlPayroll.TotalAllowance` | 🟡 rencana | Tidak dihapus. Rencana aman dan SQL baca-saja di 7 |

## 4. Perubahan per berkas

`WorkforceCore` = `Areas/Corporate/HumanResource/WorkforceCore`, `Payroll` = `Areas/Corporate/PayrollManagement`.

| Berkas | Jenis | Perubahan |
| --- | --- | --- |
| `WorkforceCore/Services/WfpSalaryAssignmentRules.cs` | baru | Satu definisi disetujui (fungsi + `Expression` untuk EF), irisan tanggal, deteksi perubahan finansial dan penutupan masa berlaku |
| `WorkforceCore/Controllers/WfpSalaryAssignmentController.cs` | ubah | Siklus approval, overlap semua penetapan utama, penjaga payroll final, transaksi Serializable + advisory lock per pegawai, audit tanpa nominal, tautan SK opsional, respons aditif. Route, verb, dan metadata akses tidak berubah |
| `WorkforceCore/DTOs/WfpSalaryAssignmentDtos.cs` | ubah (aditif) | `ApprovalStatus`, `ApprovalRequired`, `PersonnelActionOrderId/Number`, `IsLockedByFinalPayroll`, `LastFinalPayrollPeriodEnd`; request `PersonnelActionOrderId` |
| `WorkforceCore/DTOs/WorkforceCoreCrossEmployeeDtos.cs`, `Services/WorkforceCoreCrossEmployeeQueryService.cs` | ubah (aditif) | `ApprovalRequired` pada rekap lintas pegawai |
| `WorkforceCore/Services/WorkforcePayrollDataService.cs` | ubah | Label "Menunggu Persetujuan HRD" (sudah didefinisikan tetapi tidak pernah terpakai) |
| `Payroll/Options/PayrollPolicyOptions.cs` | ubah | `RequireApprovedSalaryAssignment = false` (opsi baru, bawaan mati) |
| `Payroll/Services/PayrollHrdSourceReader.cs` | ubah | P0-01; konstruktor opsional untuk kompatibilitas |
| `Payroll/Services/PayrollReadinessEvaluator.cs` | ubah | Isu `PendingSalaryMessage` |
| `Payroll/Services/PayrollCalculationService.cs` | ubah | Pemisahan disetujui/menunggu, penahan, peringatan cakupan hari, peringatan SK Promosi/Demosi tanpa penetapan baru, catatan snapshot kebijakan |
| `Payroll/Services/PayrollCompensationService.cs`, `DTOs/PayrollCompensationDtos.cs` | ubah | `IsApproved`/`SalaryApproved`/`ApprovalStatus`/`ApprovalRequired` |
| `Payroll/Services/PayrollHrdImportService.cs` | ubah | Filter approval mengikuti sakelar (variabel `approvedSalary` sebelumnya tidak menyaring apa pun) |
| `Payroll/Services/PayrollWorkforceAllowanceService.cs`, `DTOs/PayrollWorkforceAllowanceDtos.cs` | ubah | `SalaryBasis.Approved` mengikuti sakelar, `NotPayableReason`, kontrol pengaju≠pengesah |
| `Payroll/Services/PayrollSalaryStructureAllowanceService.cs`, `Controllers/PayrollSalaryStructureAllowancesController.cs`, `DTOs/PayrollSalaryStructureAllowanceDtos.cs` | ubah | `IsUsedInPayroll`, `IsUsedInFinalPayroll`, konfirmasi `AcknowledgeUsedInPayroll` |
| `Payroll/Services/PayrollRunService.cs`, `DTOs/PayrollRunDtos.cs` | ubah | Deteksi sumber kompensasi berubah setelah hitung (`NeedsRecalculation`, `RecalculationReasons`), dipakai sebagai penjaga Submit dan Approve |
| `WorkforceCore/DTOs/WfpSalaryAssignmentDtos.cs`, `Controllers/WfpSalaryAssignmentController.cs` | ubah (putaran 2) | `BaseSalary` respons menjadi `decimal?` dan **`null`** tanpa `ViewAmount` (sebelumnya `0`) |
| `Payroll/Controllers/PayrollSalaryStructureAllowancesController.cs`, `Services/PayrollSalaryStructureAllowanceService.cs`, `DTOs/PayrollSalaryStructureAllowanceDtos.cs` | ubah (putaran 2) | Penjaga `ViewAmount` pada daftar, tambah, dan ubah; endpoint baru `GET {id}/amount` yang mendaftarkan izin di Akses Role; `Amount`/`Percentage`/`MaximumAmount` menjadi nullable + `AmountsVisible` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataListTests.cs` | ubah (putaran 2) | Lihat bagian 10.1. Tidak ada berkas yang dihapus |

Tidak disentuh sengaja: `Migrations/*`, `ApplicationDbContext`, konfigurasi EF, `PayrollEarningsCalculationService` (legacy; sudah digerbang `LegacyModernPayrollGateFilter`, lihat 8), Finance Handoff, jasa medis, BPJS, dan seluruh nilai bawaan `PayrollPolicyOptions` yang lain.

## 5. Kontrak API (gaya Swagger)

Semua perubahan **aditif** kecuali empat perilaku berikut: (1) aksi tulis penetapan gaji kini dapat menjawab **409**; (2) `DELETE` tunjangan struktur yang terpakai dijawab **409** tanpa `acknowledgeUsedInPayroll`; (3) `baseSalary` penetapan gaji bernilai **`null`** (sebelumnya `0`) tanpa `ViewAmount`; (4) nominal/persen/batas tunjangan struktur bernilai **`null`** tanpa izin `ViewAmount` yang baru. Butir 3 dan 4 mengubah nilai yang diterima konsumen; konsumen frontend di repo ini sudah disesuaikan (`PAY-FE-FIX-008`).

**Langkah operasional sebelum dipakai:** izin `SalaryStructureAllowance : ViewAmount` adalah kemampuan baru di Akses Role. Peran yang selama ini melihat nominal tunjangan struktur **tidak otomatis memilikinya**; sampai administrator mencentangnya, nominal tampil "Tidak ditampilkan" bagi peran itu.

**Tags:** `Corporate / Payroll Management / Salary Assignment`

| Method | Route | Perubahan |
| --- | --- | --- |
| GET | `/api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/salary-assignments` | Respons +`approvalStatus`, `approvalRequired`, `personnelActionOrderId/Number`, `isLockedByFinalPayroll`, `lastFinalPayrollPeriodEnd`; `isApproved` kini jujur; `operationalStatus` "Menunggu Persetujuan" hanya bila sakelar menyala. **`baseSalary` kini `null` (bukan `0`) bila `amountsVisible` false** |
| GET | `…/salary-assignments/{id}` | idem |
| POST | `…/salary-assignments` | Body +`personnelActionOrderId` (opsional). 409 bila beririsan dengan penetapan utama aktif lain. `data` kini `{ id, demotedPrimaryAssignmentIds }` |
| PUT | `…/salary-assignments/{id}` | 409 bila terkunci payroll final atau beririsan. `data` kini `{ approvalReset, approvalStatus, demotedPrimaryAssignmentIds }` |
| PATCH | `…/salary-assignments/{id}/approval` | Idempoten; 403 bila `RequireDifferentApprover` menyala dan pembuat = pengesah; 409 bila tidak aktif/beririsan/terpakai payroll final saat dibatalkan |
| PATCH | `…/salary-assignments/{id}/status`, `…/primary` | 409 bila beririsan atau terpakai payroll final saat dinonaktifkan |
| DELETE | `…/salary-assignments/{id}` | 409 bila sudah dipakai payroll yang disahkan |

**Tags:** `Corporate / Payroll Management / Salary Structure Allowances`

| Method | Route | Perubahan |
| --- | --- | --- |
| GET | `/api/v1/corporate/payroll-management/master-data/salary-structures/{salaryStructureId}/allowances` | Respons +`isUsedInPayroll`, `isUsedInFinalPayroll`, `amountsVisible`. **Tanpa izin `ViewAmount`: `amount`, `percentage`, `maximumAmount` bernilai `null`** (sebelumnya selalu terkirim) |
| GET | `…/allowances/{id}/amount` | **Baru.** `[AccessAction("ViewAmount")]` + `[AccessPermission("SalaryStructureAllowance", "ViewAmount")]`; pintu izin agar kemampuan muncul di Akses Role (pola `WorkforceAllowanceAssignment`). Pemilik izin melihat nominal pada daftar, tambah, dan ubah |
| POST | `…/allowances` | Respons tambah dimasker sama dengan daftar |
| PUT | `…/allowances/{id}` | Body +`acknowledgeUsedInPayroll`; 409 bila hak finansial baris terpakai diubah tanpa konfirmasi; respons dimasker sesuai izin |
| DELETE | `…/allowances/{id}?acknowledgeUsedInPayroll=true` | Query baru; 409 tanpa konfirmasi untuk baris terpakai |

**Tags:** `Corporate / Payroll Management / Workforce Allowance Assignments` dan `Payroll Runs`

| Method | Route | Perubahan |
| --- | --- | --- |
| GET | `…/workforce-profiles/{id}/allowance-assignments` | Item +`notPayableReason` |
| POST | `…/allowance-assignments/{id}/approve` | 403 bila `RequireDifferentApprover` menyala dan pengaju = pengesah |
| GET | `/api/v1/corporate/payroll-management/payroll-runs/{id}` | +`needsRecalculation`, `recalculationReasons` (hanya run belum disahkan) |
| POST | `…/payroll-runs/{id}/submit` dan `/approve` | 409 "Sumber kompensasi berubah setelah perhitungan terakhir" |

## 6. Dependency skema (BLOCKED) — desain, bukan implementasi

Tidak ada migration, DDL, atau kolom baru yang dibuat atau dirujuk runtime. Berikut desain untuk keputusan pemilik; **tidak diuji dan tidak dijalankan.**

### 6.1 Penetapan utama pada periode historis berbeda (P0-05)

Penyebab: indeks unik parsial `IX_WfpSalaryAssignment_WorkforceProfileId` (`IsPrimary AND IsActive AND NOT IsDelete`, migration `initializeBigModulHRD2`) mengizinkan **hanya satu** penetapan utama aktif per orang, kapan pun tanggalnya. Karena itu promosi di pertengahan bulan memaksa penetapan lama diturunkan dari Utama, dan Payroll hanya membaca penetapan utama, sehingga hari sebelum promosi tidak tercakup.

Contoh: Rina naik grade 16 Nov. Penetapan lama (1 Jan–15 Nov) diturunkan dari Utama saat yang baru dibuat. Hasil hitung November hanya membayar 15 hari (16–30 Nov). Setelah perbaikan ini, hari yang tidak tercakup **diperingatkan** (angka tidak berubah).

Usulan DDL (perlu persetujuan, migration, dan uji data):

```sql
DROP INDEX IF EXISTS "IX_WfpSalaryAssignment_WorkforceProfileId";
CREATE INDEX "IX_WfpSalaryAssignment_WorkforceProfileId_Primary"
  ON "WfpSalaryAssignment" ("WorkforceProfileId")
  WHERE "IsPrimary" = true AND "IsActive" = true AND "IsDelete" = false;
CREATE EXTENSION IF NOT EXISTS btree_gist;
ALTER TABLE "WfpSalaryAssignment" ADD CONSTRAINT "EX_WfpSalaryAssignment_PrimaryNoOverlap"
  EXCLUDE USING gist ("WorkforceProfileId" WITH =,
    daterange("EffectiveStartDate", COALESCE("EffectiveEndDate", 'infinity'::date), '[]') WITH &&)
  WHERE ("IsPrimary" AND "IsActive" AND NOT "IsDelete");
```

Setelahnya, `ClearPrimaryAsync` tidak lagi perlu menurunkan siapa pun dan Payroll langsung memakai segmen tanggal. Sebelum DDL dijalankan, pemilik data perlu memeriksa baris yang sudah beririsan.

### 6.2 Versi dan tanggal efektif tunjangan struktur (P1-02)

| Opsi | Penilaian |
| --- | --- |
| A. Struktur gaji berversi (struktur baru per perubahan) | Ditolak: memaksa seluruh `WfpSalaryAssignment` dipindah ke struktur baru; duplikasi kepemilikan data |
| **B. Tanggal efektif + status persetujuan pada baris `MstSalaryStructureAllowance` (rekomendasi)** | Baris lama ditutup, baris baru dibuat; payroll memilih baris per segmen tanggal; tidak ada tabel baru |
| C. Tabel versi kebijakan terpisah | Ditolak: dua sumber kebenaran untuk hal yang sama |

Opsi B butuh kolom `EffectiveStartDate`, `EffectiveEndDate`, `ApprovalStatus`, `SubmittedByUserId`, `ApprovedByUserId`, `ApprovedAt`, dan pengganti indeks unik `(SalaryStructureId, AllowanceTypeId)` menjadi pencegah irisan tanggal. Untuk transaksi yang sudah ada: run Draft/Calculated menghitung ulang dengan versi berlaku; run Approved/Closed tidak berubah (snapshot komponen sudah menyimpan metode, nominal, dan detail).

### 6.3 Status Draft terpisah dari Menunggu Persetujuan, dan pengaju nyata

Skema hanya menyimpan `ApprovedAt`/`ApprovedByUserId`, jadi hanya dua status yang dapat dibuktikan: `Approved` dan `PendingApproval`. Menambah `SubmittedAt`/`SubmittedByUserId` pada `WfpSalaryAssignment` memberi status Draft dan pengaju sebenarnya. Sampai itu ada, pengaju diperkirakan dari pembuat/pengubah terakhir pada pemeriksaan `RequireDifferentApprover`.

## 7. Rencana penghentian cadangan `PrlPayroll.TotalAllowance` (P1-04)

`AllowProfileAllowanceFallback = true` **tidak diubah.** Cadangan hanya terpakai bila Struktur Gaji pegawai tidak memuat tunjangan (peringatan sudah ditulis per pegawai). Langkah aman: (1) ukur, (2) isi tunjangan struktur atau penetapan khusus, (3) ukur ulang sampai nol, (4) baru matikan opsi, (5) hapus kode di task terpisah.

SQL baca-saja untuk langkah 1 (tidak mengubah data; nama kolom diverifikasi dari model):

```sql
SELECT COUNT(*) AS pegawai_bergantung_cadangan
FROM "PrlPayroll" p
WHERE NOT p."IsDelete" AND p."IsActive" AND p."IsPayrollEligible" AND p."TotalAllowance" > 0
  AND NOT EXISTS (
    SELECT 1 FROM "WfpSalaryAssignment" a
    JOIN "MstSalaryStructureAllowance" l ON l."SalaryStructureId" = a."SalaryStructureId" AND l."IsActive" AND NOT l."IsDelete"
    WHERE a."WorkforceProfileId" = p."WorkforceProfileId" AND a."IsActive" AND NOT a."IsDelete" AND a."IsPrimary");
```

**Sebelum menyalakan `RequireApprovedSalaryAssignment`** jalankan juga (baca-saja):

```sql
SELECT COUNT(*) FILTER (WHERE "ApprovedAt" IS NULL OR "ApprovedByUserId" IS NULL) AS belum_disetujui, COUNT(*) AS total
FROM "WfpSalaryAssignment" WHERE "IsActive" AND "IsPrimary" AND NOT "IsDelete";
```

Dua jalur mengisi persetujuan: **UI**, belum ada layar "Setujui penetapan gaji" di HRD (endpoint `PATCH …/approval` sudah ada); **SQL**, pengesahan massal atas data lama adalah keputusan pemilik karena menyatakan seseorang telah menyetujui. Saya tidak menjalankan DML.

## 8. Jalur legacy dan area yang tidak diubah

`PayrollEarningsCalculationService` masih berisi tarif tunjangan berbasis nama jabatan/departemen. Ia hanya terjangkau lewat `PayrollRunModernController`, yang seluruh aksi tulisnya ditolak oleh `LegacyModernPayrollGateFilter` selama `LegacyModernMutationsEnabled = false` (bawaan), dan ditolak pada run yang sudah dihitung mesin resmi. Dengan kata lain, jalur itu tidak menjadi sumber pembayaran ganda pada konfigurasi bawaan. Menghapusnya bukan kebutuhan task ini dan diserahkan ke keputusan terpisah. Finance Handoff, jasa medis (`MedicalFeePayrollPathEnabled = false`), dan BPJS (`EnableBpjsInPayroll = false`) tidak disentuh.

## 9. Rencana UAT (TIDAK ADA yang dijalankan)

| UAT | Cakupan source | Catatan |
| --- | --- | --- |
| 01 Karyawan baru dapat tunjangan struktur otomatis | ✅ (`BuildStructureAllowanceLines` yang sudah ada) | Penetapan harus sah menurut sakelar |
| 02 Struktur sama, tunjangan bawaan sama | ✅ | Tidak berubah |
| 03 Grade sama, struktur beda | ✅ | Tidak berubah |
| 04 Medis: struktur + khusus tanpa ganda | ✅ | Penjaga duplikat yang sudah ada |
| 05 Nonmedis tanpa tunjangan aktivitas medis | ✅ | SA-06 yang sudah ada |
| 06 Aktif belum disetujui tidak dianggap Approved | ✅ tampilan; 🟡 pembayaran | Pembayaran hanya bila sakelar menyala |
| 07 Ubah gaji setelah approval butuh approval baru | ✅ | `Update` membatalkan approval |
| 08 Promosi pertengahan bulan → dua segmen | 🟡 **BLOCKED skema 6.1** | Kini diperingatkan |
| 09 Grade lama dipakai untuk tanggal lama | 🟡 BLOCKED 6.1 | idem |
| 10 Tunjangan khusus PendingApproval tidak dibayar | ✅ | Sudah ada |
| 11 Jenis sama dari struktur dan individual | ✅ | Sudah ada |
| 12 Irisan tunjangan individual ditolak | ✅ | Sudah ada |
| 13 Perubahan master setelah run final tidak mengubah snapshot | ✅ | Snapshot komponen; penjaga payroll final |
| 14 Perubahan setelah hitung memicu hitung ulang | ✅ | `NeedsRecalculation` + 409 pada Submit/Approve |
| 15 Tanpa `ViewAmount` tidak melihat nominal | ✅ source (putaran 2) | Penetapan gaji (`baseSalary` null), tunjangan pegawai, dan kini tunjangan struktur dijaga. Belum dibuktikan runtime |
| 16 Run gagal validasi tidak bisa disahkan lewat request langsung | ✅ | `EnsureReadyToDecide` + penjaga baru |
| 17 Tunjangan aktivitas tanpa data tidak dibayar | ✅ | Tidak berubah |
| 18 Hitung ulang tanpa komponen ganda | ✅ | Tidak berubah |
| 19 Koreksi retroaktif pada Closed lewat jalur sah | 🟡 | Prosedur off-cycle belum ada; run final tidak ditulis ulang |
| 20 Legacy, Finance AP jasa medis, BPJS tidak berubah | ✅ | Bagian 8 |
| 21 Endpoint FE sesuai kontrak BE | 🟡 | Ditelusuri statis di `PAY-FE-FIX-008` |

## 10. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet restore` / `dotnet build` | **NOT RUN** (dilarang task). Kompilasi tidak terbukti; risiko kesalahan sintaks atau tipe tetap ada |
| Migration / `dotnet ef` / Update-Database | **TIDAK DIBUAT, TIDAK DIJALANKAN** (`git status -- Migrations`: 0 berkas) |
| QBE ReportOnly (20 berkas, putaran 1; 10 berkas yang berubah lagi, putaran 2) | **PASS** pada keduanya, VIOLATION 0, REVIEW 0 |
| Review diff / scope | Dilakukan lewat pembacaan; DI: `IOptions<PayrollPolicyOptions>` sudah terdaftar (`Program.cs` baris 437) dan tidak ada registrasi baru |
| Tes unit FE yang membaca source BE | Putaran 1: 161 dari 162 lulus pada 9 berkas; 1 gagal pada tes menu sidebar PayrollRun (area yang tidak disentuh). Putaran 2 (9 berkas terkait penetapan gaji, tunjangan, kesiapan; ditambah 3 tes baru): **170 dari 170 lulus**; `payroll-run-screens.test.mjs` tidak ikut dijalankan ulang. Ini pemeriksaan teks source dan fungsi murni FE, **bukan** bukti perilaku runtime BE |
| Pengujian runtime / UAT | **NOT RUN** |

### 10.1 Folder `Tests/` backend (putaran 2)

`Tests/` masih terlacak (166 berkas) walau `TEST_POLICY.md` §1 menyatakan folder itu sudah tidak ada dan `QuilvianSystemBackend.sln` tidak memuatnya. **Tidak ada berkas yang dihapus**; menghapusnya adalah keputusan struktur repo milik pemilik. Perubahan hanya pada `WorkforcePayrollDataListTests.cs`, dibatasi pada hal yang disebabkan source saya:

| Perubahan | Alasan |
| --- | --- |
| Seed `Gaji(...)` mengisi `ApprovedByUserId` bila `disetujui` | Aturan disetujui kini `ApprovedAt` **dan** `ApprovedByUserId` |
| Pembaca dibuat lewat helper `Pembaca(ctx)` dengan `RequireApprovedSalaryAssignment = true` | Tes ini menilai status disetujui/menunggu; pada mode bawaan (mati) status menunggu tidak pernah muncul. Konstruktor lama `new PayrollHrdSourceReader(ctx)` tetap valid |
| `Layanan()` memberi pembaca sungguhan, bukan `null!` | `null!` sudah menyebabkan `NullReferenceException` sebelum pekerjaan ini |
| Pesan menunggu persetujuan memakai `PayrollReadinessEvaluator.PendingSalaryMessage` | Teks pesan itu berubah |

**Tidak diperbaiki karena di luar penyebab saya dan tidak dapat diverifikasi tanpa menjalankan tes:** pesan lain di berkas yang sama sudah berbeda dari HEAD sebelum pekerjaan ini (misalnya "Belum ada penetapan gaji utama." vs "Belum ada penetapan gaji pada Master HRD." di `WorkforcePayrollDataService`). `dotnet test` NOT RUN. Rekomendasi: pemilik memutuskan memensiunkan `Tests/` sesuai `TEST_POLICY.md` atau memperbaruinya utuh.

## 11. Risiko residual dan keputusan menunggu

1. **Sakelar `RequireApprovedSalaryAssignment` bawaan mati**: tujuan brief (gaji sah hanya yang disetujui) belum aktif sampai pemilik menyalakannya setelah layar persetujuan dan pengisian data lama siap.
2. Penetapan utama yang tidak beririsan masih diturunkan dari Utama (6.1); hari sebelum promosi belum terbayar sampai skema berubah. Hanya peringatan.
3. Peringatan cakupan hari dan peringatan SK Promosi/Demosi belum menjadi penahan: wajib atau tidaknya penetapan baru per jenis SK dan aturan prorata/cut-off belum disahkan. SK yang dicakup: `Promotion`, `Demotion`, status `Scheduled`/`Applied`.
4. Konfirmasi `AcknowledgeUsedInPayroll` bukan persetujuan; otorisasi perubahan nominal tunjangan struktur menunggu skema 6.2.
5. Pengaju penetapan gaji diperkirakan dari pembuat/pengubah terakhir (6.3).
6. ~~`GET …/salary-structures/{id}/allowances` mengembalikan nominal tanpa `ViewAmount`.~~ **Diperbaiki (putaran 2).** Dampak operasional: peran yang biasa melihat nominal harus diberi izin baru `ViewAmount` (lihat bagian 5).
7. ~~Respons penetapan gaji tanpa `ViewAmount` mengirim `baseSalary: 0`.~~ **Diperbaiki (putaran 2):** kini `null`. Sisa celah yang dicatat: pengguna dengan izin Update tetapi tanpa `ViewAmount` tidak bisa melihat nominal saat ini, sehingga formulir ubah mewajibkan mengetik nominal baru (kontrak `UpdateWfpSalaryAssignmentRequest.BaseSalary` tidak nullable). Mengubah ini ke "pertahankan nilai lama" adalah perubahan kontrak tersendiri.
8. Cakupan stale-detection memakai waktu ubah baris; perubahan master Jenis Tunjangan (`MaximumAmount`, dan sebagainya) tidak ikut dideteksi.
9. Tes lama yang mengunci "peniadaan approval" diperbarui ke aturan sakelar (lihat `PAY-FE-FIX-008`); keputusan itu tidak dicabut.

## 12. Status Git

Perubahan task ini hanya pada berkas di bagian 4 dan laporan ini. Tidak ada stage, commit, push, pull, merge, atau perubahan remote. Working tree backend sudah memuat banyak perubahan lain sebelum task dimulai (BPJS Payroll dan seterusnya); itu tidak disentuh.

**Perubahan Migrations yang bukan dari task ini:** pada putaran 2 `Migrations/ApplicationDbContextModelSnapshot.cs` terlihat berubah (+101 baris, menambah entity `HrdOvertimeSetting`) dan sempat muncul berkas `20261120044204_AddHrdOvertimeSettingTable` (kemudian tidak ada lagi di folder). Itu pekerjaan lain (HRD Overtime Setting), tidak ada baris yang menyentuh `WfpSalaryAssignment`, `SalaryStructureAllowance`, atau `Prl*`, dan tidak saya sentuh. Task ini tidak membuat atau menjalankan migration.
