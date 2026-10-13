# Laporan Perubahan Backend — `BE-FIN-104`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-104` |
| Judul | HR Benefit: baca, pakai, dan kembalikan plafon per orang |
| Slice | `REV-19` Slice A — Fondasi HR Benefit dan Registrasi (`EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-19` |
| Trace | `FIN-DEC-185`, `202`, `205`, `207`; `FIN-STATE-1.9` `R.3`; `FIN-VAL-1.12` `FIN-VAL-255`..`258`; `02-backend-architecture.md` `R.3`, `R.4`, `R.10` |
| Contract version | `FIN-INTEGRATION-1.9` `R.3` (`INT-BIL-HR-001`), `R.2.3`; `FIN-API-1.10` `R.1` (`GET /{id}/movements`) — **approved** (`FIN-DEC-212`, `223`) |
| Dependency | `BE-FIN-103` 🟡 (source ada di commit `a73f5593`; build dan migration belum), `{FIN-OQ-105}` ✅ |
| Klasifikasi | `HEAVY` — skor 12: berkas diperiksa >20 (2), berkas diubah >8 (2), logika bisnis kompleks: idempotensi dan konkurensi saldo (2), kontrak integrasi dan API baru (2), dampak schema: tabel baru (2), keamanan terkait (1), lintas modul (1), UI 0 |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("mulai kerjakan BE-FIN-104", 13 Oktober 2026), lintas modul ke `Areas/Corporate/HumanResource/BenefitManagement` |
| Target tulis | `Areas/Corporate/HumanResource/BenefitManagement/**`, `Repositories/Configurations/Corporate/HumanResource/BenefitManagement/**`, satu baris `DbSet` di `ApplicationDbContext.cs`, dua baris DI di `Program.cs`, laporan ini, dan baris status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a73f5593` (branch `Yasmina`; commit pemilik yang memuat source `BE-FIN-103`) |
| Tanggal | 13 Oktober 2026 |
| Status | 🟡 **Sebagian** — source selesai; `dotnet build` **NOT RUN** (pemilik meminta tanpa build); file migration **belum dibuat** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | Corporate / Human Resource / BenefitManagement |
| Registry | Baris `Corporate / SelfServices — Human Resource — BUSINESS DOMAIN — Hrd — ACTIVE / LEGACY`. Registry menyatakan entity operasional baru pada submodule HR memakai prefix pemilik baris itu, yaitu `Hrd` |
| Keberlakuan | `NEW CODE` untuk `HrdBenefitBalanceMovement`, konfigurasinya, dua service, dan DTO; `TOUCHED LEGACY` untuk controller enrollment (satu endpoint ditambah), `ApplicationDbContext`, dan `Program.cs` |
| Entity operasional baru | `HrdBenefitBalanceMovement` — `QBE-MOD-002` **terpenuhi** (baris registry `ACTIVE`, prefix `Hrd`; bukan dikarang dari nama folder). Pola pembanding: `HrdAttendance*` di `AttendanceManagement` |
| QBE yang berlaku | `QBE-MOD-002`, `QBE-MOD-003`, `QBE-NAM-001` (entity baru `Hrd*`, bukan `Trx*`), `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-CFG-002`, `QBE-DEL-001` (buku mutasi hanya menambah; tanpa hapus) |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-FIN-103`, HR sudah bisa mencatat bahwa Sari (nama samaran) punya plafon Rp 3.000.000. Namun belum ada cara aman
bagi Billing untuk **bertanya** berapa sisanya, **mengurangi** saat tagihan final, dan **mengembalikan** bila tagihan
dikoreksi. Tanpa ini ada dua bahaya:

- Tagihan yang difinalisasi ulang karena jaringan putus bisa mengurangi plafon dua kali.
- Dua kasir yang memfinalisasi tagihan orang yang sama pada detik yang sama bisa membuat saldo negatif.

## 2. Proses bisnis

**Tujuan.** Plafon di HR berkurang tepat satu kali per serah terima porsi RS Benefit, kembali utuh bila dikoreksi, dan
setiap gerakan tercatat di buku mutasi yang tidak bisa diubah.

| Langkah | Pelaku | Hasil |
| ---: | --- | --- |
| 1. Baca plafon: kelayakan diperiksa ulang pada tanggal layanan, lalu saldo orang itu dikembalikan | Billing (pratinjau dan finalisasi) | Layak + sisa, atau kode alasan |
| 2. Pakai plafon sebesar porsi RS Benefit, di dalam transaksi finalisasi Billing | Billing | Saldo berkurang; satu mutasi `USE` |
| 3. Billing menyimpan seluruh perubahan sekaligus | Billing | Gagal salah satu → semuanya batal, plafon tidak berkurang |
| 4. Kembalikan plafon saat tagihan dikoreksi atau dibatalkan | Billing (`BE-FIN-108`, `109`) | Saldo bertambah penuh; satu mutasi `RETURN` |
| 5. Petugas HR melihat buku mutasi enrollment | Petugas HR (`EmployeeBenefitEnrollment : Read`) | Daftar pemakaian dan pengembalian |

**Aturan dengan contoh** (sisa plafon Sari Rp 3.000.000).

| Peristiwa | Hasil | Sisa |
| --- | --- | ---: |
| Pakai Rp 3.000.000 untuk invoice A (kunci K1) | `APPLIED` | Rp 0 |
| Pakai lagi dengan kunci K1, Rp 3.000.000, invoice A (finalisasi diulang) | `ALREADY_APPLIED`, tidak ada perubahan | Rp 0 |
| Pakai kunci K1 dengan Rp 2.500.000 | `KEY_CONFLICT` (`409` bila dipetakan ke API) | Rp 0 |
| Kembalikan kunci K1 | `RETURNED` Rp 3.000.000 | Rp 3.000.000 |
| Kembalikan kunci K1 lagi | `ALREADY_RETURNED` | Rp 3.000.000 |
| Pakai kunci K1 lagi sesudah dikembalikan | `KEY_CONFLICT` — koreksi memakai serah terima pengganti dengan kunci baru | Rp 3.000.000 |
| Pakai Rp 3.500.000 dengan kunci K2 | `INSUFFICIENT`, membawa sisa terkini Rp 3.000.000 (`422` bila dipetakan) | Rp 3.000.000 |
| Tanggungan Sari diakhiri HR, lalu Billing memakai | `NOT_USABLE` (`422` bila dipetakan) | tetap |

**Jalur tidak normal.** Bila layanan dipanggil di luar transaksi, ia menolak bekerja (`InvalidOperationException`) agar
pengurangan saldo tidak pernah lepas dari penerbitan serah terima. Bila saldo terpakai lebih kecil dari mutasi yang
hendak dikembalikan (data tidak konsisten), pengembalian ditolak `EmployeeBenefitConflictException` dan transaksi
pemanggil dibatalkan. Petugas HR yang menyunting tanggungan bersamaan dengan pemakaian saldo menerima `409`, karena
pemakaian mengganti `RowVersion`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; aturan `rules/backend/` (`TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `API_RULES`, `DATABASE_RULES`,
`REPORT_TEMPLATE`, `TEST_POLICY`); `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; kontrak `integration-contract.md`
`R.2.3`, `R.3`; `api-contract.md` `R.1`; `validation-matrix.md` `R.2`; `state-transition-matrix.md` `R.3`;
`data/data-dictionary.md` 9.1–9.3; `02-backend-architecture.md` `R.2`–`R.10`; `HrdAttendanceRawLog.cs` dan konfigurasinya
(pola `Hrd*`); `IdentityModel.cs`; `MstEmployee.cs`, `MstBenefitPlan.cs`, `WfpFamilyMember.cs`, `BilInvoice.cs`;
`LoggerService.cs` (memastikan logger tidak menulis lewat `DbContext`); `BillingFinalizationService.cs` (isolasi
transaksi `Serializable`); `AppDateTimeHelper.cs`; seluruh berkas `BE-FIN-103`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/BenefitManagement/Models/HrdBenefitBalanceMovement.cs` | **Baru.** Buku mutasi plafon (kamus data 9.2) |
| `Repositories/Configurations/Corporate/HumanResource/BenefitManagement/HrdBenefitBalanceMovementConfiguration.cs` | **Baru.** Lima check constraint, dua indeks unik, tiga indeks, tiga FK `Restrict` |
| `.../BenefitManagement/Services/EmployeeBenefitBalanceService.cs` | **Baru.** `ReadAsync` (BacaPlafon), `UseAsync` (PakaiPlafon), `ReturnAsync` (KembalikanPlafon), `GetMovementsAsync` |
| `.../BenefitManagement/Services/EmployeeBenefitEligibilityService.cs` | **Baru.** `CheckAsync` (PeriksaKelayakan, `R.2.3`) dan pesan per kode alasan |
| `.../BenefitManagement/DTOs/EmployeeBenefitBalanceDtos.cs` | **Baru.** Request/hasil tiga operasi; `BenefitMovementPagedQuery`, `BenefitMovementResponse` |
| `.../BenefitManagement/DTOs/EmployeeBenefitEligibilityDtos.cs` | **Baru.** Request dan hasil kelayakan |
| `.../BenefitManagement/Constants/HrBenefitConstants.cs` | + `BenefitPersonKinds`, `BenefitMovementTypes`, `BenefitEligibilityReasonCodes`, `BenefitUseResults`, `BenefitReturnResults` |
| `.../BenefitManagement/Controllers/EmployeeBenefitEnrollmentController.cs` | + `GET /{id}/movements`; konstruktor menerima `EmployeeBenefitBalanceService` |
| `.../BenefitManagement/Services/EmployeeBenefitEnrollmentService.cs` | Literal `"ENROLLMENT"`/`"DEPENDENT"` diganti konstanta (tanpa perubahan perilaku) |
| `Repositories/ApplicationDbContext.cs` | + `DbSet<HrdBenefitBalanceMovement> HrdBenefitBalanceMovements` |
| `Program.cs` | + `AddScoped` untuk `EmployeeBenefitEligibilityService` dan `EmployeeBenefitBalanceService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak integrasi | `INT-BIL-HR-001` tersedia sebagai layanan in-process untuk `BE-FIN-107`/`108`. Nama method berbahasa Inggris: `ReadAsync` = BacaPlafon, `UseAsync` = PakaiPlafon, `ReturnAsync` = KembalikanPlafon |
| Kontrak API | Satu endpoint baru `GET /{id}/movements` (bagian 4). Delta: bagian 7 |
| Database | Satu tabel baru, lima check constraint, dua indeks unik, tiga indeks biasa. **File migration belum dibuat; belum diterapkan** (bagian 5) |
| Keamanan/Auth | Tiga operasi plafon **tidak** punya endpoint, sehingga hanya bisa dipanggil kode backend (Billing), bukan pengguna. Buku mutasi dibaca dengan hak akses enrollment yang sudah ada (`EmployeeBenefitEnrollment : Read`). Log audit hanya memuat ID, jenis, dan nominal; tanpa nama, NIK, atau hubungan |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Benefit Management / Employee Benefit Enrollment

Base URL: `api/v1/corporate/human-resource/benefit-management/employee-benefit-enrollments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}/movements` | Buku mutasi plafon enrollment itu (pegawai sendiri dan tanggungannya), terbaru di atas | `EmployeeBenefitEnrollment : Read` | Query `MovementType` (`USE`/`RETURN`), `From`, `To` (tanggal), `PageNumber`, `PageSize` (maks. 50) | `ApiResponse<PagedResult<BenefitMovementResponse>>` |

Kode status: `200` berhasil; `400` `MovementType` selain `USE`/`RETURN`; `403` tanpa hak; `404` enrollment tidak ditemukan.

**Layanan in-process (bukan endpoint)** — `EmployeeBenefitBalanceService`:

| Operasi | Masukan | Hasil |
| --- | --- | --- |
| `ReadAsync` | `EmployeeId`, `FamilyMemberId?`, `AsOfDate` | `IsUsable`, `ReasonCode` (kode `R.2.3`), `BenefitPersonKind`, `BenefitPersonId`, `EnrollmentId`, `RelationshipCode`, `LimitAmount`, `UsedAmount`, `RemainingAmount`, `PlanCode` |
| `UseAsync` | `SourceHandoffKey`, `InvoiceId`, `BenefitPersonKind`, `BenefitPersonId`, `Amount`, `ActorUserId?` | `APPLIED`, `ALREADY_APPLIED`, `KEY_CONFLICT`, `INSUFFICIENT`, `NOT_USABLE`, beserta `RemainingAmount` terkini |
| `ReturnAsync` | `SourceHandoffKey`, `Reason?`, `ActorUserId?` | `RETURNED`, `ALREADY_RETURNED`, `NOT_FOUND` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Entity baru berprefix `Hrd` sesuai baris registry `ACTIVE`; controller tidak mengakses context | `PASS` | Bagian preflight |
| Pencocokan hak akses endpoint baru | `ControllerName` = `EmployeeBenefitEnrollment` = argumen 1 `[AccessPermission]`; argumen 1 `[AccessAction]` = `Read` = argumen 2 `[AccessPermission]`; `AccessType = Read` | `PASS` | Review controller |
| Review diff dan scope | Hanya berkas pada 3.2; tidak ada perubahan pemilik lain di working tree | `PASS` | `git status --short` |
| Diagnostik IDE | Language server VS Code tidak melaporkan berkas C# sama sekali, sehingga ini **bukan** bukti kompilasi | `NOT AVAILABLE` | `getDiagnostics` |
| `dotnet restore` / `dotnet build` | — | `NOT RUN` | Pemilik meminta tanpa build. **Wajib dijalankan sebelum task ditandai ✅** |
| Pembuatan file migration | `20261013055556_TableHrdBenefitBalanceMovements` ada di working tree, dibuat **di luar sesi agent** pukul 12:55. Isinya diperiksa: tabel `HrdBenefitBalanceMovement` (5 check constraint, 2 indeks unik, 3 indeks, 3 FK), `RowVersion` kedua tabel, constraint dan indeks `BE-FIN-103` | `DONE` (oleh pemilik) | Berkas migration; **belum diterapkan** ke database |
| Verifikasi proses bisnis runtime (contoh bagian 2) | — | `NOT RUN` | Tidak ada pemanggil runtime sampai `BE-FIN-107`/`108`; uji manual memerlukan build dan migration |

Uji manual: `REQUIRED` setelah build dan migration, idealnya bersama `BE-FIN-108`.
`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Perintah yang perlu dijalankan pemilik.** Migration `BE-FIN-103` belum pernah dibuat (commit `a73f5593` tidak memuatnya),
sehingga satu migration mencakup keduanya, sesuai nama di `02-backend-architecture.md` `R.8`:

```bash
dotnet build
dotnet ef migrations add AddHrBenefitBalanceMovementAndConstraints
```

Isi yang diharapkan: tabel `HrdBenefitBalanceMovement`; kolom `RowVersion` pada `TrxEmployeeBenefitEnrollment` dan
`TrxEmployeeBenefitDependent`; check constraint saldo dan maker-checker; indeks unik terbuka per plan dan per anggota
keluarga; filter indeks lama tanggungan yang dipersempit. Sebelum diterapkan, jalankan tiga kueri pra-periksa pada
[laporan BE-FIN-103](BE-FIN-103.md) bagian 5; ketiganya harus nol. Tabel baru tidak memerlukan pengisian data lama.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Sisa Rp 3.000.000, pakai Rp 3.000.000 untuk invoice A: sisa Rp 0 | Terpenuhi di source, belum terbukti runtime | `UseAsync`: syarat `RemainingAmount >= amount` membolehkan sama dengan |
| 2. Pakai invoice A lagi: sisa tetap Rp 0, jawaban sukses (idempoten) | Terpenuhi di source | `UseAsync` → `ALREADY_APPLIED` bila kunci, orang, nominal, dan invoice sama; mutasi yang belum tersimpan juga diperiksa |
| 3. Kembalikan invoice A: sisa Rp 3.000.000 | Terpenuhi di source | `ReturnAsync` mengembalikan nominal `USE` secara penuh |
| 4. Pakai melebihi sisa ditolak `422` | Terpenuhi di source sebagai hasil `INSUFFICIENT`; pemetaan ke `422` dilakukan pemanggil karena tidak ada endpoint | `UseAsync` |
| 5. Dua finalisasi bersamaan untuk orang yang sama: tidak ada pengurangan melebihi saldo | Terpenuhi di source, belum terbukti runtime | Kunci advisory per orang, `UPDATE` bersyarat, dan `CK_*_Balance` |
| DoD: preflight selesai | Terpenuhi | Bagian preflight |
| DoD: kontrak cocok | Terpenuhi dengan delta tercatat | Bagian 7 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| DoD (dari Verifikasi roadmap): `dotnet build` atas permintaan pemilik | **Belum** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta terhadap roadmap | Cakupan roadmap menulis "kunci `InvoiceId`"; kontrak yang disetujui (`R.3`) memakai `sourceHandoffKey` = `HandoffKey` baris `PAYER`. Diikuti **kontrak**. `InvoiceId` tetap disimpan dan ikut dicocokkan pada pengulangan |
| Delta lingkup | `EmployeeBenefitEligibilityService.CheckAsync` (PeriksaKelayakan) dibuat di sini karena BacaPlafon wajib menjalankannya ulang (`R.3.1`). `CariPegawai`, `DaftarKeluarga`, dan adapter Registrasi tetap milik `BE-FIN-106`. Efek sampingnya: kriteria 4 `BE-FIN-103` (di luar masa berlaku tidak dihitung layak) kini punya source |
| Delta kontrak API | `BenefitMovementResponse` ditambah `BenefitPersonKind`, `BenefitPersonId`, `PersonName`, `InvoiceId`, dan `ReasonText` agar buku mutasi enrollment yang memuat beberapa orang dapat dibaca. `InvoiceNumber` dibaca dari `BilInvoice` (baca saja, tanpa FK) |
| Delta kamus data | Indeks tambahan `IX_HrdBenefitBalanceMovement_Dependent` dan `IX_HrdBenefitBalanceMovement_InvoiceId` (pencarian per tanggungan dan per invoice) |
| Tafsiran yang dicatat | (1) PakaiPlafon memeriksa kelayakan **berdasarkan status** saja; masa berlaku terhadap tanggal layanan sudah diperiksa BacaPlafon pada transaksi finalisasi yang sama (`R.3.4` langkah 2), karena masukan PakaiPlafon tidak membawa tanggal. (2) KembalikanPlafon tetap mengembalikan walaupun orangnya sudah tidak aktif, agar `Used = Σ USE − Σ RETURN` tetap benar. (3) Tanggungan yang pernah ada tetapi ditolak atau diakhiri dilaporkan `DEPENDENT_NOT_ELIGIBLE`, bukan `DEPENDENT_NOT_REGISTERED` |
| Masalah yang diketahui | Di bawah isolasi `Serializable` milik `BillingFinalizationService`, finalisasi kedua yang menunggu kunci orang yang sama dapat gagal dengan galat serialisasi (`40001`) alih-alih `INSUFFICIENT`. Saldo tetap aman; `BE-FIN-108` perlu menangani galat itu sebagai "coba lagi" |
| Risiko tersisa | Kode belum pernah dikompilasi; sintaks `ExecuteUpdateAsync`/`SetProperty` belum diverifikasi compiler. Bila pemanggil sedang melacak baris saldo yang sama, nilainya dimuat ulang dan perubahan pemanggil atas baris itu hilang (Billing tidak menulis tabel HR, sehingga tidak diharapkan terjadi) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Di tengah sesi ditemukan pemilik sudah meng-commit `BE-FIN-103` (`a73f5593`); pekerjaan dilanjutkan di atas commit itu |
| Status Git | ` M` `HrBenefitConstants.cs`, `EmployeeBenefitEnrollmentController.cs`, `EmployeeBenefitEnrollmentService.cs`, `ApplicationDbContext.cs`, `Program.cs`; `??` dua DTO, dua service, satu model, satu konfigurasi, dan laporan ini |
| `requirement-traceability.md` | Tidak ada pada blueprint ini; traceability dipegang tabel di roadmap |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` dan `dotnet ef migrations add AddHrBenefitBalanceMovementAndConstraints`, lalu menerapkannya. `BE-FIN-106` (butuh `BE-FIN-105`) dan `BE-FIN-107`/`108` adalah pemakai layanan ini |
