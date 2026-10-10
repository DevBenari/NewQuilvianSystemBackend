# Laporan Perubahan Backend — `BE-FIN-093`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-093` |
| Judul | Petugas Finance dapat mengajukan, menyetujui (maker-checker keras), dan membatalkan perjanjian cicilan piutang pegawai |
| Slice | `REV-18B2` — Perjanjian Cicilan, Clearance, Pelunasan & Migrasi |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-192`..`199`; `FIN-DEC-165`, `166`; `FIN-DES-099` |
| Contract version | `FIN-API-1.9` O.2 (`draft`); `FIN-VAL-1.10` `FIN-VAL-230`..`238` (`approved`); `FIN-STATE-1.8` O.1 |
| Dependency | `BE-FIN-091` 🟡, `BE-FIN-092` 🟡 — **keduanya belum ✅** dan belum pernah dikompilasi. Dilanjutkan atas konfirmasi eksplisit pengguna pada sesi ini (opsi "lanjutkan source-only, tetap tanpa build") setelah risikonya disampaikan |
| Klasifikasi | `HEAVY` — 1 service baru (7 method publik, transaksi + locking + maker-checker), 1 controller baru (7 endpoint), 1 berkas DTO baru (13 class), pendaftaran DI baru. Nol migration (nol perubahan schema) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{Controllers,DTOs,Services}/`, `Program.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis. `dotnet build` TIDAK DIJALANKAN** (instruksi eksplisit pengguna). Dibangun di atas `BE-FIN-091`/`BE-FIN-092` yang juga belum pernah dikompilasi — risiko compile error majemuk tiga task, disadari dan diterima pengguna secara eksplisit. Temuan signifikan: **seluruh service `Corporate/FinanceManagement` tampak tidak terdaftar DI di `Program.cs`** — lihat bagian 3 |

---

## 0. Backend Governance Preflight

**Area/Module:** `Corporate` / `FinanceManagement/Receivable`, prefix `Fin`, status registry `ACTIVE`. Module sudah terdaftar — nol blocker QBE-MOD.

**Applicability:** `NEW CODE` penuh — service, controller, dan DTO baru seluruhnya.

**QBE ID yang berlaku:** QBE-SVC-001 (controller tidak menyentuh `ApplicationDbContext` langsung — seluruh akses data lewat `FinanceReceivableInstallmentPlanService`), QBE-API-001 (boundary API/response/status mengikuti `FIN-API-1.9` O.2 apa adanya), QBE-PERM-001 (`[AccessAction]`/`[AccessPermission]` pada ketujuh endpoint, argumen dicocokkan baris per baris terhadap `role-access-rules.md` — lihat bagian 2.3), QBE-CODE-002/003 (`PlanNumber` dialokasikan service via Guid, bukan controller, bukan Count/Max/Last+1), QBE-DTO-001 (entity EF tidak pernah diekspos sebagai response — seluruhnya dipetakan ke DTO), QBE-VAL-001 (`FIN-VAL-230`..`238` diterapkan — lihat bagian 4), QBE-TXN-001 (transaksi `Serializable` + advisory lock pada `Create`/`Approve`/`Cancel`, pola sama persis `FinanceBillingIntakeService`), QBE-LOG-001 (`AuditAsync` pada setiap transisi status).

**Pola yang diikuti dari source existing, bukan ditulis ulang:**
- Infrastruktur transaksi (`BeginTransactionAsync`/`AcquireLockAsync`/`CommitAsync`/`RollbackAsync`) — disalin pola persis dari `FinanceBillingIntakeService.cs`, bukan diabstraksi jadi helper bersama (`AGENTS.md`: "jangan menciptakan abstraksi... ketika implementasi terdekat tidak menggunakannya").
- Resolusi nama pengguna (`GetUserNameAsync`/`GetUserNamesAsync`, `DisplayName ?? UserName ?? Email ?? UserCode`) — disalin pola persis dari `FinanceOpeningItemBatchService.cs` (BE-FIN-089).
- Exception per-domain (`InstallmentPlanValidationException` 422, `InstallmentPlanConflictException` 409) dan `Failure()`/`IsHandled()` switch pada controller — pola persis `ReceivableConflictException`/`ReceivableValidationException` pada `FinanceArController.cs`/`FinanceReceivablesController.cs`.
- Generate nomor via Guid, bukan Count/Max/Last+1 — pola persis `GenerateReceivableNumber()` pada `FinanceBillingIntakeService.cs`.

---

## 1. Proses bisnis

**Pelaku:** Staf Finance (mengajukan, membatalkan), penyetuju Finance lain (menyetujui, menolak, membatalkan).

**Alur normal:**
1. Staf mengajukan perjanjian atas kartu piutang `EMPLOYEE_BENEFIT` — `POST /receivables/{id}/installment-plans`. Status `MENUNGGU`, **nol baris jadwal** terbentuk (L.1.1).
2. Penyetuju **lain** (bukan pengaju) menyetujui — `POST /receivable-installment-plans/{id}/approve`. Seluruh baris jadwal dibangkitkan **sekaligus** dalam transaksi yang sama: periode berurutan mulai `FirstDeductionPeriod`, nominal rata kecuali baris terakhir menampung sisa pembulatan (L.1.2-L.1.4). Status menjadi `DISETUJUI`.
3. Penyetuju dapat menolak (`/reject`, wajib alasan) atau pengaju dapat menarik pengajuannya sendiri selagi `MENUNGGU` (`/cancel`) — keduanya sah, berbeda pelaku, berbeda makna (L.1.5, L.1.6).
4. Perjanjian yang sudah `DISETUJUI` dapat dibatalkan penyetuju; seluruh angsuran yang belum `TERBAYAR` ikut dibatalkan.

**Jalur tidak normal yang ditegakkan:**
- Pengaju mencoba menyetujui pengajuannya sendiri → `422`, ditegakkan **dua lapis**: service (pesan jelas) dan `CK_FinReceivableInstallmentPlan_MakerChecker` (keras — L.2.1, L.8.1, **tidak dapat dilewati** walau administrator memberi satu peran hak `Create` **dan** `Approve` sekaligus).
- Piutang bukan `EMPLOYEE_BENEFIT`, sudah lunas/dihapus-buku/dibatalkan, atau periode gaji pertama sudah lewat → `422` masing-masing dengan pesan spesifik.
- Dua pengajuan aktif atas piutang yang sama (hampir bersamaan) → `409`, ditegakkan **dua lapis**: pre-check service di dalam advisory lock, dan `IX_FinReceivableInstallmentPlan_ReceivableId_Active` sebagai backstop keras (L.2.3).
- Sisa piutang berubah antara pengajuan dan persetujuan (misal ada penerimaan tunai di antaranya) → `422`, jadwal **MUST NOT** terbentuk (L.2.6) — divalidasi **di dalam** transaksi persetujuan sebelum satu pun baris `FinReceivableInstallment` ditulis.

**Yang sengaja TIDAK diimplementasikan task ini** — lihat bagian 3.

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/api-contract.md` O.1-O.2 (kontrak endpoint dan DTO, dibaca penuh)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` `FIN-VAL-230`..`238`
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` O.1-O.2 (state machine plan dan installment)
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` `L.1.1`..`L.1.7`, `L.2.1`..`L.2.12`, `L.8.1`
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (pola transaksi/lock)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs` (pola resolusi nama pengguna)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs`, `FinanceReceivablesController.cs` (pola exception per-domain dan `Failure()`)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceBusinessDate.cs` (tanggal bisnis WIB)
- `Program.cs` (pola registrasi DI — lihat temuan bagian 3)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableInstallmentPlanDtos.cs` | **Baru.** 13 class: 4 request, 1 query, `InstallmentPlanResponse` + 2 turunannya (`List`, `Detail` — pola pewarisan sama dengan `FinReceiptDetailResponse : FinReceiptResponse`), `InstallmentResponse` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInstallmentPlanService.cs` | **Baru.** 7 method publik (`CreateAsync`, `GetHistoryAsync`, `GetPagedAsync`, `GetDetailAsync`, `ApproveAsync`, `RejectAsync`, `CancelAsync`) + infrastruktur transaksi/lock/mapping privat, 2 exception class |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivableInstallmentPlansController.cs` | **Baru.** 7 endpoint sesuai `FIN-API-1.9` O.2 apa adanya |
| `Program.cs` | Tambah `using` namespace `Receivable.Services`; tambah `AddScoped<FinanceReceivableInstallmentPlanService>()` beserta catatan temuan (bagian 3) |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 7 endpoint baru, persis `FIN-API-1.9` O.2 — lihat bagian 2.4 |
| Database | `NOT APPLICABLE` — nol migration, nol perubahan schema. Seluruh tabel yang dipakai (`FinReceivableInstallmentPlan`, `FinReceivableInstallment`, `FinReceivable`) sudah disediakan `BE-FIN-092` |
| Keamanan/Auth | `ControllerName = "FinanceReceivableInstallmentPlan"` (resource baru). Lima `AccessAction` (`Create`, `Read`, `Approve`, `Reject`, `Cancel`), argumen `[AccessPermission]` dicocokkan baris per baris terhadap `ControllerName`/nama `[AccessAction]` pada method yang sama — nol penyimpangan (lihat `role-access-rules.md`). `BenefitOwnerId`/`BenefitRelationship` pada `InstallmentPlanListResponse` ditandai sensitif sesuai kamus data; tidak masuk `AuditAsync` (yang hanya mencatat `PlanId`/`ActorUserId`) |

### 2.4 Dokumentasi endpoint

#### `Corporate / Finance Management / Receivable / Installment Plan`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/receivables/{receivableId}/installment-plans` | Mengajukan perjanjian angsuran atas satu kartu piutang | `FinanceReceivableInstallmentPlan : Create` |
| `GET` | `/receivables/{receivableId}/installment-plans` | Riwayat perjanjian satu kartu piutang, termasuk yang ditolak | `FinanceReceivableInstallmentPlan : Read` |
| `GET` | `/receivable-installment-plans` | Daftar kerja: perjanjian menunggu dan yang sudah diputuskan | `FinanceReceivableInstallmentPlan : Read` |
| `GET` | `/receivable-installment-plans/{id}` | Rincian satu perjanjian beserta seluruh jadwal angsurannya | `FinanceReceivableInstallmentPlan : Read` |
| `POST` | `/receivable-installment-plans/{id}/approve` | Menyetujui, membangkitkan seluruh baris jadwal | `FinanceReceivableInstallmentPlan : Approve` |
| `POST` | `/receivable-installment-plans/{id}/reject` | Menolak beserta alasannya | `FinanceReceivableInstallmentPlan : Reject` |
| `POST` | `/receivable-installment-plans/{id}/cancel` | Membatalkan beserta alasannya | `FinanceReceivableInstallmentPlan : Cancel` |

---

## 3. Temuan yang dicatat, bukan didiamkan

**A. `FinanceReceivableInstallmentPlanService` tidak dapat diselesaikan DI tanpa diperbaiki — dan ternyata hampir seluruh service `Corporate/FinanceManagement` lain juga demikian.** Saat memeriksa `Program.cs` untuk mendaftarkan service baru ini, `grep -n "AddScoped<Finance" Program.cs` memulangkan **nol hasil** — bukan hanya untuk service ini, tetapi untuk prefix `Finance` apa pun. Diperiksa langsung: `FinanceBillingIntakeService`, `FinanceReceivableService`, dan tampaknya seluruh service pada `Collection/`, `Payable/`, `Purchasing/`, `CashManagement/`, `AccountingIntegration/`, `PettyCash/` **tidak terdaftar** `AddScoped` di `Program.cs`, meski controller dan `using` namespace-nya sudah ada (contoh: baris `using ...BillingIntake.Services` ada sejak lama, tetapi `AddScoped<FinanceBillingIntakeService>()` tidak pernah ditulis). Tidak ada mekanisme scanning reflektif (`Scrutor` tidak ada pada `.csproj`) yang bisa menjelaskan ini lewat jalur lain.

**Akibatnya:** controller mana pun pada rumpun `Corporate/FinanceManagement` yang constructor-nya meminta salah satu service ini akan melempar `InvalidOperationException: Unable to resolve service...` pada request **pertama** — bukan saat `dotnet build`. Inilah sebabnya laporan task terdahulu bisa mencatat "Build PASS" bertahun-tahun tanpa pernah menangkap ini: compile sukses tidak membuktikan DI terselesaikan saat runtime, dan tidak ada task sebelumnya yang sempat menjalankan server sungguhan untuk rumpun Finance.

**Yang saya kerjakan:** mendaftarkan **hanya** `FinanceReceivableInstallmentPlanService` (service milik task ini sendiri) di `Program.cs`, dengan komentar yang menunjuk temuan ini. **Yang TIDAK saya kerjakan:** memperbaiki pendaftaran untuk service Finance lain — itu di luar cakupan `BE-FIN-093`, menyentuh puluhan file dan berpotensi berdampak luas, butuh task/otorisasi tersendiri. Pemilik repository MUST menugaskan audit DI menyeluruh untuk rumpun `Corporate/FinanceManagement` sebelum modul mana pun di dalamnya dianggap siap dipakai live.

**B. `L.1.7` (perjanjian `SELESAI` otomatis) secara struktural tidak dapat diimplementasikan task ini.** Transisi ini dipicu "seluruh angsuran berstatus `TERBAYAR`", dan status angsuran hanya berubah lewat hasil potongan gaji dari HR (`INT-HR-FIN-001`, slice `S3`, **tertahan** `FIN-OQ-091`, dikerjakan `BE-FIN-095` — yang bahkan belum ada task pemilik eksplisit di repository ini). Tidak ada jalur kode apa pun pada `BE-FIN-093` yang memanggil hasil potongan, sehingga tidak ada tempat untuk memasang pemeriksaan ini tanpa mengarang mekanisme yang belum disepakati. Dicatat `NOT APPLICABLE` pada bagian 5, bukan diam-diam dilewatkan.

**C. `FIN-VAL-232`/`L.2.5` secara struktural tidak tercapai lewat endpoint publik.** `CreateInstallmentPlanRequest` (`O.2.1`) **tidak** memuat ruas `TotalAgreedAmount` — nilai itu **selalu dihitung server** dari `InstallmentCount × InstallmentAmount`. Skenario `L.2.5` ("Ajukan total Rp 4.000.000 dengan 3 × Rp 1.000.000") mengandaikan klien dapat mengirim `Total` yang tidak cocok, tetapi bentuk request yang dikontrakkan tidak mengizinkannya. `CK_FinReceivableInstallmentPlan_Total` (dari `BE-FIN-092`) tetap terpasang sebagai pertahanan berlapis, tetapi lewat jalur `CreateAsync` ini constraint itu **tidak pernah dapat dilanggar** secara struktural — perhitungannya selalu konsisten dengan dirinya sendiri. Dicatat sebagai temuan kontrak, bukan ditambahkan ruas baru secara sepihak (itu mengubah kontrak `FIN-API-1.9` yang berstatus `draft`, perlu persetujuan pemilik).

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| `dotnet ef migrations add` | **TIDAK DIJALANKAN** | `NOT RUN` | `NOT APPLICABLE` juga — nol perubahan schema pada task ini |
| Review diff manual (DTO, service, controller, `Program.cs`) | Dicocokkan baris per baris terhadap `FIN-API-1.9` O.2, `FIN-VAL-230`..`238`, state-transition-matrix O.1-O.2 | `PASS` (review manual, bukan compiler) | Bagian 1-3 di atas |
| Pencocokan `[AccessPermission]` vs `[AccessController]`/`[AccessAction]` per method | Ketujuh endpoint diperiksa satu per satu, nol penyimpangan argumen | `PASS` (review manual) | `FinanceReceivableInstallmentPlansController.cs` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

**Risiko compile belum nol dan majemuk.** Task ini dibangun di atas `BE-FIN-092` (entity `FinReceivableInstallmentPlan`/`FinReceivableInstallment`) yang **juga** belum pernah dikompilasi. Bila ada kesalahan nama property atau tipe pada `BE-FIN-092`, kesalahan itu akan ikut muncul saat `BE-FIN-093` di-build, bercampur dengan kesalahan baru task ini sendiri (bila ada) — keduanya baru akan terlihat bersamaan saat pemilik menjalankan `dotnet build`.

Uji manual: `NOT FEASIBLE` — butuh server berjalan dengan DI terselesaikan, migration `BE-FIN-092` diterapkan, dan data piutang `EMPLOYEE_BENEFIT` sungguhan; tak satu pun tersedia pada sesi ini.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `L.1.1` Pengajuan, nol jadwal | **Terpenuhi struktural.** `CreateAsync` tidak pernah menulis `FinReceivableInstallment` | `CreateAsync` |
| `L.1.2` Jadwal lahir dari persetujuan | **Terpenuhi struktural.** `ApproveAsync` membangkitkan N baris dalam satu transaksi | `ApproveAsync` |
| `L.1.3` Periode berurutan | **Terpenuhi struktural.** `AddMonths` dipanggil berurutan per baris | `ApproveAsync`, helper `AddMonths` |
| `L.1.4` Pembulatan di baris terakhir | **Terpenuhi struktural.** Baris terakhir = `TotalAgreedAmount - InstallmentAmount × (Count-1)` | `ApproveAsync` |
| `L.1.5` Penolakan beserta alasan | **Terpenuhi struktural.** `RejectAsync`, `RejectionReason` wajib (anotasi DTO) | `RejectAsync` |
| `L.1.6` Menarik pengajuan sendiri | **Terpenuhi struktural.** `CancelAsync` sah dari `MENUNGGU`, nol pembatasan aktor (`FIN-DEC-166` hanya melarang approve) | `CancelAsync` |
| `L.1.7` Selesai otomatis | **`NOT APPLICABLE` pada task ini** — lihat temuan B bagian 3 | — |
| `L.2.1`, `L.8.1` Maker-checker | **Terpenuhi struktural, dua lapis.** Service + `CK_..._MakerChecker` (`BE-FIN-092`) | `ApproveAsync` |
| `L.2.2` Jenis debitur salah | **Terpenuhi struktural.** `FIN-VAL-230` | `CreateAsync` |
| `L.2.3` Dua perjanjian aktif | **Terpenuhi struktural, dua lapis.** Lock + pre-check + unique index (`BE-FIN-092`) | `CreateAsync` |
| `L.2.4` Pengajuan ulang sesudah ditolak/dibatalkan | **Terpenuhi struktural.** Unique index hanya menyaring `MENUNGGU`/`DISETUJUI` | Index `IX_FinReceivableInstallmentPlan_ReceivableId_Active` (`BE-FIN-092`) |
| `L.2.5` Total tidak cocok | **`NOT APPLICABLE` secara struktural** — lihat temuan C bagian 3 | — |
| `L.2.6` Sisa berubah saat persetujuan | **Terpenuhi struktural.** `FIN-VAL-233`, divalidasi sebelum menulis baris jadwal | `ApproveAsync` |
| `L.2.7` Periode sudah lewat | **Terpenuhi struktural.** `FIN-VAL-235` | `CreateAsync` |
| `L.2.8` Piutang lunas/dihapus/dibatalkan | **Terpenuhi struktural.** `FIN-VAL-236` | `CreateAsync` |
| `L.2.9` Jumlah angsuran di luar batas | **Terpenuhi struktural.** `[Range(2,60)]` pada DTO | `CreateInstallmentPlanRequest` |
| `L.2.10` Alasan kosong | **Terpenuhi struktural.** `[Required]` pada `RejectionReason`/`CancelReason` | DTO |
| `L.2.11` Menyetujui yang `DITOLAK` | **Terpenuhi struktural.** `ApproveAsync` menolak non-`MENUNGGU` dengan `409` | `ApproveAsync` |
| `L.2.12` Mengubah status angsuran langsung | **Terpenuhi by design.** Nol endpoint PATCH status angsuran dibuat task ini | Kontrak `O.2` tidak memuatnya |

**"Terpenuhi struktural"** berarti terpetakan ke source yang ada dan diverifikasi lewat review manual, **belum** diverifikasi lewat eksekusi runtime (compiler maupun HTTP) — lihat bagian 4.

**DoD roadmap** ("Build PASS, unit test PASS, laporan task tracked"): **belum terpenuhi** — `dotnet build` sengaja tidak dijalankan.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Source **belum pernah dikompilasi**, dibangun di atas `BE-FIN-092` yang juga belum. (2) Temuan DI pada bagian 3.A **MUST** ditindaklanjuti pemilik sebelum rumpun Finance mana pun dianggap siap jalan — ini bukan temuan kosmetik |
| Masalah yang diketahui | `L.1.7` dan `L.2.5` tidak tercapai lewat task ini — keduanya dijelaskan bagian 3, bukan cacat implementasi |
| Risiko tersisa | (1) Risiko compile majemuk 3 task (`BE-FIN-091`, `092`, `093`) belum nol. (2) Tanpa perbaikan temuan DI, endpoint task ini **akan** melempar exception pada request pertama walau compile sukses. (3) Belum ada migration yang diterapkan — fitur ini sama sekali belum bisa diuji end-to-end |
| Perubahan sampingan | `NONE` |
| Interupsi | Pesan "Kerjakan BE-FIN-094" sempat masuk di tengah riset task ini, lalu dikoreksi kembali ke `BE-FIN-093` oleh pengguna pada pesan berikutnya. Dilanjutkan `BE-FIN-093` sesuai instruksi terakhir, nol pekerjaan `BE-FIN-094` dimulai |
| Status Git | `git status --short`: 4 file baru (Controller, DTOs, Service, dan — tercatat di `BE-FIN-092` — 4 Model + 4 Configuration), `Program.cs` dimodifikasi (using + registrasi DI) |
| Langkah berikutnya | (1) Pemilik menjalankan `dotnet build` untuk memvalidasi `BE-FIN-091`+`092`+`093` sekaligus. (2) Audit DI menyeluruh rumpun `Corporate/FinanceManagement` (temuan 3.A) — disarankan task/slice tersendiri. (3) Setelah build bersih, `dotnet ef migrations add` untuk `BE-FIN-092`, lalu `dotnet ef database update`, baru endpoint task ini dapat diuji end-to-end |
