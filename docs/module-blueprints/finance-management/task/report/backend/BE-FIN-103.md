# Laporan Perubahan Backend — `BE-FIN-103`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-103` |
| Judul | HR Benefit: enrollment pegawai dan verifikasi anggota keluarga |
| Slice | `REV-19` Slice A — Fondasi HR Benefit dan Registrasi (`EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, amandemen `REV-19` (tabel task) dan `REV-21` |
| Trace | `FIN-DEC-185`, `189`, `201`, `202`, `204`, `207`, `211`; `FR-FIN-242`, `243`; `FIN-STATE-1.9` `R.1`, `R.2`; `FIN-VAL-1.12` `FIN-VAL-249`..`254`, `271` |
| Contract version | `FIN-API-1.10` `R.1`, `R.2` dan `FIN-PERM-1.11` — **approved** sisi pemilik Finance, HR, Billing (`FIN-DEC-212`, `223`) |
| Dependency | `{FIN-OQ-104}` ✅ (`FIN-DEC-206`), `{FIN-OQ-105}` ✅ (`FIN-DEC-212`) |
| Klasifikasi | `HEAVY` — skor 11: repo 0, berkas diperiksa >20 (2), berkas diubah >8 (2), logika bisnis kompleks (2), kontrak API baru (2), dampak schema (2), keamanan terkait (1), UI 0 |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("mulai kerjakan", 13 Oktober 2026), lintas modul ke `Areas/Corporate/HumanResource/BenefitManagement` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/BenefitManagement/**`, `Repositories/Configurations/Corporate/HumanResource/BenefitManagement/**`, `Program.cs`, laporan ini, dan baris status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `c6ca3042` (branch `Yasmina`, upstream `origin/Yasmina`; working tree memuat perubahan pemilik yang belum di-commit, tidak disentuh) |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ **Selesai** — source selesai; `dotnet build` dan migrasi `20261013055556_TableHrdBenefitBalanceMovements` dijalankan pemilik dan dilaporkan berhasil (13 Oktober 2026). Verifikasi manual runtime belum dijalankan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | Corporate / Human Resource / BenefitManagement |
| Registry | Baris `Corporate / SelfServices — Human Resource — Hrd — ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` untuk service, controller, DTO, konstanta; `TOUCHED LEGACY` untuk entity `Trx*` dan konfigurasinya |
| Entity operasional baru | **Tidak ada** pada task ini (`HrdBenefitBalanceMovement` milik `BE-FIN-104`), sehingga `QBE-MOD-002`/`003` tidak terpicu |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-CODE-001`..`003`, `QBE-CFG-002`, `QBE-DEL-001`. `QBE-NAM-001` dipatuhi: tidak ada nama `Trx*` baru; entity `Trx*` lama tidak di-rename (itu `LEGACY MIGRATION` tersendiri) |

---

## 1. Masalah yang diperbaiki

Model enrollment benefit (`TrxEmployeeBenefitEnrollment`) dan tanggungan (`TrxEmployeeBenefitDependent`) sudah ada di database,
tetapi **tidak ada satu pun service maupun endpoint** yang mengelolanya. Akibatnya HR tidak bisa mencatat bahwa Budi (nama samaran)
punya benefit layanan RS dengan plafon Rp 10.000.000, apalagi bahwa anaknya Sari ditanggung. Tanpa data ini, aturan "pasien keluarga
harus terdaftar dulu" (`FIN-DEC-204`) tidak dapat ditegakkan, dan Billing tidak punya sisa plafon untuk dibaca.

## 2. Proses bisnis

**Tujuan.** HR mendaftarkan pegawai ke plan benefit dan menetapkan siapa saja anggota keluarga yang ditanggung, masing-masing dengan saldo plafon sendiri.

| Langkah | Pelaku | Status sesudahnya |
| ---: | --- | --- |
| 1. Membuat enrollment: pilih pegawai dan plan | Petugas HR (`Create`) | `Draft` |
| 2. Mengajukan | Petugas HR (`Submit`) | `Submitted` |
| 3. Menyetujui — **bukan** oleh pengaju | Pejabat HR (`Approve`) | `Approved` |
| 4. Mengaktifkan: saldo awal = batas plafon plan | Pejabat HR (`Activate`) | `Active` |
| 5. Mendaftarkan anggota keluarga sebagai tanggungan | Petugas HR (`Create` tanggungan) | `Draft` |
| 6. Mengajukan verifikasi | Petugas HR (`Request`) | `Requested` |
| 7. Memverifikasi: masa berlaku dan batas plafon tanggungan | Pejabat HR (`Verify`) | `Verified` |

**Aturan dengan contoh.**

| Aturan | Contoh |
| --- | --- |
| Saldo awal pegawai = batas plan saat diaktifkan (`FIN-DEC-207`, `FIN-VAL-250`) | Plan Rp 10.000.000 → `CoverageLimitAmount` = `RemainingAmount` = Rp 10.000.000, `UsedAmount` Rp 0 |
| Plan tanpa batas plafon tidak bisa diaktifkan | Plan dengan batas kosong → `422` "Plan benefit ini belum memiliki batas plafon…" |
| Satu benefit layanan RS aktif per pegawai pada periode berimpit (`FIN-VAL-249`) | Budi sudah aktif 1 Jan–31 Des 2026 pada plan Medical; mengaktifkan plan Medical kedua 1 Jun 2026 → `422` |
| Pengaju ≠ penyetuju (`FIN-VAL-271`) | Pengguna yang mengajukan lalu menyetujui → `403` |
| Tanggungan harus anggota keluarga aktif milik pegawai yang sama (`FIN-VAL-251`) | Memilih anggota keluarga pegawai lain → `422` |
| Hubungan harus diizinkan `CoverageType` (`FIN-VAL-252`) | Plan `EmployeeAndChildren`, mendaftarkan pasangan → `422` "Jenis hubungan SPOUSE tidak ditanggung…" |
| Jumlah tanggungan terbuka ≤ `MaximumDependents` (`FIN-VAL-253`) | Plan maksimum 3; tanggungan ke-4 → `422` |
| Kode hubungan diturunkan sistem (`FIN-DEC-211`) | `Father` di HR → `PARENT`; `Sibling` → `OTHER` |
| Verifikasi wajib batas plafon > 0 (`FIN-VAL-254`) | Batas Rp 5.000.000 → saldo Sari Rp 5.000.000 |

**Jalur tidak normal.** Pembatalan enrollment wajib beralasan dan bisa dari `Draft`, `Submitted`, `Approved`, atau `Active`.
Tanggungan yang ditolak wajib beralasan; yang sudah terverifikasi dapat diakhiri dengan tanggal dan alasan. Tanggungan yang ditolak
atau diakhiri dapat didaftarkan ulang sebagai baris baru. Perubahan bersamaan oleh dua petugas ditolak `409` lewat `RowVersion`.

**Hasil akhir.** Enrollment `Active` dengan saldo pegawai, dan tanggungan `Verified` dengan saldo masing-masing. Ini bahan untuk
pemeriksaan kelayakan (`BE-FIN-106`) dan pemakaian plafon (`BE-FIN-104`, `108`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `CLAUDE.md`; aturan suite `rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`, `CROSS_REPO_RULES`, `REVIEW_RULES`,
`API_RULES`, `DATABASE_RULES`, `REPORT_TEMPLATE`, `engineering/BACKEND_ENGINEERING_CONTRACT.md`), `transaction-endpoint-standard.md`,
`role-access-rules.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; model dan konfigurasi `TrxEmployeeBenefitEnrollment`,
`TrxEmployeeBenefitDependent`, `MstBenefitPlan`, `MstBenefitType`, `WfpFamilyMember`, `MstEmployee`, `IdentityModel`;
`BenefitPlanController.cs` (aturan `MaximumDependents`); pola pembanding `FinanceReceivableInstallmentPlansController.cs` dan
`FinanceReceivableInstallmentPlanService.cs`; `ResignationController.cs` (pola `[AccessController]` HR); `NumberSeriesAllocator`,
`NumberAllocationRequest`, `FinanceReceivableNumberGenerator.cs`; `Helpers/AppDateTimeHelper.cs`; `Program.cs`; kontrak blueprint terkait.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/BenefitManagement/Constants/HrBenefitConstants.cs` | **Baru.** Status enrollment dan tanggungan, kode hubungan baku beserta pemetaannya, aturan `CoverageType`, penanda kategori `Medical`, nama aksi |
| `.../BenefitManagement/DTOs/EmployeeBenefitEnrollmentDtos.cs` | **Baru.** Request dan response enrollment, ringkasan per pegawai, ringkasan status, metadata filter, riwayat status |
| `.../BenefitManagement/DTOs/EmployeeBenefitDependentDtos.cs` | **Baru.** Request dan response tanggungan |
| `.../BenefitManagement/Services/EmployeeBenefitEnrollmentService.cs` | **Baru.** CRUD terbatas dan lifecycle enrollment; nomor dari `NumberSeriesAllocator` (`HRD_BENEFIT_ENROLLMENT`, `NEVER`); transaksi `Serializable` dan kunci advisory; audit log tanpa data sensitif |
| `.../BenefitManagement/Services/EmployeeBenefitDependentService.cs` | **Baru.** Pendaftaran dan lifecycle tanggungan; hitung ulang `ActiveDependentCount` |
| `.../BenefitManagement/Controllers/EmployeeBenefitEnrollmentController.cs` | **Baru.** 12 endpoint; pemetaan exception ke 400/403/404/409/422 |
| `.../BenefitManagement/Controllers/EmployeeBenefitDependentController.cs` | **Baru.** 6 endpoint |
| `.../BenefitManagement/Models/TrxEmployeeBenefitEnrollment.cs` | + kolom `RowVersion` (`Guid`) |
| `.../BenefitManagement/Models/TrxEmployeeBenefitDependent.cs` | + kolom `RowVersion` (`Guid`) |
| `Repositories/Configurations/.../TrxEmployeeBenefitEnrollmentConfiguration.cs` | + `CK_TrxEmployeeBenefitEnrollment_Balance`, + `CK_TrxEmployeeBenefitEnrollment_MakerChecker`, + `UX_TrxEmployeeBenefitEnrollment_OpenPerPlan`, `RowVersion` sebagai concurrency token |
| `Repositories/Configurations/.../TrxEmployeeBenefitDependentConfiguration.cs` | + `CK_TrxEmployeeBenefitDependent_Balance`, + `UX_TrxEmployeeBenefitDependent_OpenPerFamilyMember`, filter indeks unik lama dipersempit ke status terbuka, `RowVersion` sebagai concurrency token |
| `Program.cs` | Pendaftaran `AddScoped` dua service baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 18 endpoint baru (bagian 4). Delta terhadap `FIN-API-1.10`: lihat bagian 7 |
| Database | Dua kolom baru, tiga check constraint baru, dua indeks unik baru, satu filter indeks diubah. **Migration `20261013055556_TableHrdBenefitBalanceMovements` dibuat dan diterapkan pemilik (dilaporkan berhasil).** Lihat bagian 5 |
| Keamanan/Auth | Dua resource hak akses baru (`EmployeeBenefitEnrollment`, `EmployeeBenefitDependent`) di modul Akses Role `HUMAN_RESOURCE_BENEFIT_MANAGEMENT`; maker-checker ditegakkan service dan database; log tanpa nama, NIK, atau tanggal lahir |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Benefit Management / Employee Benefit Enrollment

Base URL: `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status untuk saringan layar | `EmployeeBenefitEnrollment : Read` |
| `GET` | `/summary` | Jumlah enrollment per status | `EmployeeBenefitEnrollment : Read` |
| `GET` | `` | Daftar berpaging (cari nomor, NIP, nama; saring pegawai, plan, status) | `EmployeeBenefitEnrollment : Read` |
| `GET` | `/{id}` | Rincian beserta tanggungan dan `AvailableActions` | `EmployeeBenefitEnrollment : Read` |
| `GET` | `/by-employee/{employeeId}` | Benefit layanan RS aktif satu pegawai dan saldo per orang | `EmployeeBenefitEnrollment : Read` |
| `GET` | `/{id}/status-history` | Riwayat status | `EmployeeBenefitEnrollment : Read` |
| `POST` | `` | Membuat enrollment `Draft` | `EmployeeBenefitEnrollment : Create` |
| `PUT` | `/{id}` | Menyunting enrollment `Draft` | `EmployeeBenefitEnrollment : Update` |
| `POST` | `/{id}/submit` | Mengajukan | `EmployeeBenefitEnrollment : Submit` |
| `POST` | `/{id}/approve` | Menyetujui | `EmployeeBenefitEnrollment : Approve` |
| `POST` | `/{id}/activate` | Mengaktifkan dan menetapkan saldo awal | `EmployeeBenefitEnrollment : Activate` |
| `POST` | `/{id}/cancel` | Membatalkan dengan alasan | `EmployeeBenefitEnrollment : Cancel` |

#### Corporate / Human Resource / Benefit Management / Employee Benefit Dependent

Base URL: `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments/{enrollmentId}/dependents`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `` | Daftar tanggungan | `EmployeeBenefitDependent : Read` |
| `POST` | `` | Mendaftarkan anggota keluarga | `EmployeeBenefitDependent : Create` |
| `POST` | `/{id}/request-verification` | Mengajukan verifikasi | `EmployeeBenefitDependent : Request` |
| `POST` | `/{id}/verify` | Memverifikasi beserta masa berlaku dan batas plafon | `EmployeeBenefitDependent : Verify` |
| `POST` | `/{id}/reject` | Menolak dengan alasan | `EmployeeBenefitDependent : Reject` |
| `POST` | `/{id}/end` | Mengakhiri dengan tanggal dan alasan | `EmployeeBenefitDependent : End` |

**Kode status.** `400` aksi tidak sah untuk status saat ini. `403` pengaju mencoba menyetujui sendiri. `404` data tidak ditemukan.
`409` data diubah petugas lain atau sudah ada enrollment/tanggungan terbuka yang sama. `422` aturan bisnis (`FIN-VAL-249`..`254`).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Tidak ada entity baru; tidak ada nama `Trx*` baru; controller tidak mengakses context | `PASS` | Bagian *Backend Governance Preflight* |
| Pencocokan hak akses: `ControllerName` = argumen 1 `[AccessPermission]`; argumen 1 `[AccessAction]` = argumen 2 `[AccessPermission]`; `AccessType` hanya Read/Create/Update | Cocok pada 18 endpoint | `PASS` | Review berkas controller |
| Review diff dan scope | Hanya berkas pada 3.2; perubahan pemilik lain di working tree tidak disentuh | `PASS` | `git diff --stat` |
| `dotnet restore` / `dotnet build` | Dijalankan pemilik dan dilaporkan **berhasil** (13 Oktober 2026); keluarannya tidak dilihat agent | `PASS` (dilaporkan pemilik) | Laporan pemilik pada sesi 13 Oktober 2026 |
| Pembuatan dan penerapan migration | Migration `20261013055556_TableHrdBenefitBalanceMovements` (memuat `RowVersion`, tiga check constraint, dua indeks unik, filter indeks diubah, dan tabel buku mutasi `BE-FIN-104`) dibuat dan diterapkan pemilik; dilaporkan berhasil | `PASS` (dilaporkan pemilik) | Berkas ada di `Migrations/` |
| Verifikasi kontrak dan proses bisnis runtime | — | `NOT RUN` | Uji manual belum dijalankan |

Uji manual: `REQUIRED` (setelah build dan migration).
`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Perintah yang perlu dijalankan pemilik**

```bash
dotnet build
dotnet ef migrations add AddHrBenefitEnrollmentLifecycleConstraints
```

Sebelum menerapkan migration, periksa data yang dapat menggagalkan constraint baru (kueri baca saja):

```sql
SELECT COUNT(*) FROM public."TrxEmployeeBenefitEnrollment" WHERE "UsedAmount" < 0 OR "RemainingAmount" < 0;
SELECT COUNT(*) FROM public."TrxEmployeeBenefitDependent" WHERE "UsedAmount" < 0 OR "RemainingAmount" < 0;
SELECT "WorkforceProfileId","BenefitPlanId",COUNT(*) FROM public."TrxEmployeeBenefitEnrollment"
 WHERE "EnrollmentStatus" IN ('Draft','Submitted','Approved','Active') AND "IsDelete" = false
 GROUP BY 1,2 HAVING COUNT(*) > 1;
```

Ketiganya harus nol baris/nol. Kolom `RowVersion` baru akan terisi nilai bawaan untuk baris lama; aman sebagai token konkurensi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pegawai yang di-enroll ke plan berplafon Rp 10.000.000 punya `RemainingAmount` Rp 10.000.000 | Terpenuhi di source, belum terbukti runtime | `EmployeeBenefitEnrollmentService.ActivateAsync` |
| 2. Dependent tidak bisa diverifikasi tanpa `FamilyMemberId` yang valid dan aktif | Terpenuhi di source | `CreateAsync` mewajibkan anggota keluarga aktif milik pegawai yang sama; verifikasi hanya dari `Requested` |
| 3. Melebihi `MaximumDependents` ditolak | Terpenuhi di source | `EmployeeBenefitDependentService.CreateAsync` |
| 4. Dependent di luar masa berlaku tidak dihitung eligible | Terpenuhi di source | Masa berlaku disimpan saat verifikasi; penilaian terhadap tanggal layanan dilakukan `EmployeeBenefitEligibilityService.CheckAsync` (`BE-FIN-104`) dan hasilnya `DEPENDENT_OUT_OF_PERIOD` |
| 5. Hak akses enrollment terpisah dari hak akses master data | Terpenuhi di source | Resource `EmployeeBenefitEnrollment`/`EmployeeBenefitDependent`, terpisah dari `BenefitPlan` |
| DoD: preflight selesai | Terpenuhi | Bagian preflight |
| DoD: build PASS atas permintaan pemilik | Terpenuhi (dilaporkan pemilik) | Build berhasil |
| DoD: kontrak API cocok | Terpenuhi dengan delta tercatat | Bagian 7 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta kontrak (`FIN-API-1.10`, `FIN-PERM-1.11`, kamus data 9.3) | (1) Kolom `RowVersion` ditambahkan pada kedua tabel; kontrak mewajibkan `RowVersion` di request, tetapi kamus data 9.3 menulis "nol kolom baru". (2) Endpoint `filters/metadata`, `summary`, dan `status-history` ditambahkan sesuai standar endpoint transaksi; riwayat status diturunkan dari kolom jejak model. (3) `GET /{id}/movements` **tidak** dibuat; buku mutasinya milik `BE-FIN-104`. (4) Field `Description` pada pendaftaran tanggungan tidak dibuat karena model tidak punya kolomnya. (5) Argumen pertama `[AccessController]` memakai kode modul `HUMAN_RESOURCE_BENEFIT_MANAGEMENT` mengikuti pola HR, bukan kode per controller yang tertulis di `FIN-PERM-1.11` `R.2`. (6) Filter indeks unik lama pada tanggungan dipersempit agar pendaftaran ulang setelah ditolak atau diakhiri mungkin. (7) Status `Expired` tidak dipasang otomatis; respons membawa `IsWithinEffectivePeriod` |
| Tafsiran yang perlu dikonfirmasi pemilik HR | `MaximumDependents = 0` dibaca "tidak ada tanggungan diizinkan" (selaras `BenefitTypeController`) |
| Masalah yang diketahui | Pemeriksaan "benefit layanan RS ganda" berjalan di service karena butuh data jenis benefit; tidak dapat berupa indeks database |
| Risiko tersisa | Uji manual runtime belum dijalankan; keluaran build dan migrasi tidak dilihat agent |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas task: ` M` dua model, dua konfigurasi, `Program.cs`; `??` folder `Constants/`, `Controllers/`, `DTOs/`, `Services/` di `BenefitManagement`, dan laporan ini. Perubahan lain di working tree milik pemilik dan tidak disentuh |
| `requirement-traceability.md` | Tidak ada pada blueprint ini; traceability dipegang tabel di roadmap. Tidak dibuat berkas baru tanpa diminta |
| Langkah berikutnya | `BE-FIN-104` dan `BE-FIN-106` sudah dikerjakan di atas task ini |
