# Preflight `RJ-BIL-BE-006` — Financial Action, Approval, Close/Reopen

| Field | Nilai |
|---|---|
| Task | `RJ-BIL-BE-006` |
| Outcome | Menyediakan financial action, approval, dan close/reopen |
| Blueprint | `RJ-BIL-BP-001` revision `14` |
| Requirement sumber | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `015` |
| Task approval | `APPROVED_FOR_EXECUTION` pada `2026-08-21` |
| Jenis dokumen | Preflight read-only — **belum ada satu baris code pun yang ditulis atau diubah** |
| Build dijalankan | `TIDAK` |
| Database disentuh | `TIDAK` |
| Wewenang yang dipakai | `READ_DISCOVERY_AUTHORITY`, `QBE_PREFLIGHT_AUTHORITY` |
| Wewenang yang belum ada | `BUILDER_EXECUTION` untuk `RJ-BIL-BE-006` — masih `NOT_AUTHORIZED` |
| Tanggal | `2026-08-27` |
| Verdict preflight | **`BLOCKED_BY_OWNER_DECISION`** — satu keputusan arsitektur belum diambil, lihat bagian `5` |

## 1. Ringkasan untuk pembaca non-teknis

Tugas ini menjawab: **siapa yang boleh membatalkan, mengoreksi, menggratiskan, atau mengembalikan
uang sebuah tagihan — dan siapa yang harus menyetujuinya.**

Aturannya sudah Anda kunci pada `RJ-BIL-GATE-DEC-006`. Intinya satu kalimat: **orang yang mengajukan
tidak boleh menyetujui permintaannya sendiri.**

Preflight ini memeriksa apakah sistem sudah punya mesin persetujuan yang bisa dipakai ulang.
Jawabannya: **ada, dan mesinnya bagus** — tetapi mesin itu milik modul Kepegawaian, dibuat untuk cuti
dan lembur, dan pada dua titik ia **tidak menegakkan aturan yang Anda kunci untuk uang.**

Karena itu ada satu keputusan yang harus Anda ambil sebelum ada code yang ditulis.

## 2. Yang sudah terkunci dan tidak perlu ditanyakan lagi

`RJ-BIL-GATE-DEC-006` berstatus `locked-draft` dan sudah sangat rinci. Yang berikut ini **tidak boleh
dikarang ulang** dan sudah menjadi acuan:

| Terkunci | Isi |
|---|---|
| Pemisahan capability | Charge create/finalize, adjustment, void create/approve, reversal create/approve, refund create/approve/execute, waiver, write-off, financial-review, manual override, folio close/reopen — seluruhnya capability terpisah |
| Maker-checker | Maker dan checker **wajib** berbeda `UserId` terautentikasi/efektif. Delegasi tidak boleh membuat orang efektif yang sama menjadi keduanya |
| Self-approval | Memiliki capability create **dan** approve sekaligus **tidak** memberi hak self-approval |
| High-risk tanpa memandang nominal | Void/reversal terhadap `Paid`, `Posted`, `Claimed`, `Settled`; refund settled payment; reopen folio tertutup; koreksi lintas encounter |
| Lifecycle approval | `Draft → Submitted → PendingApproval → Approved`, dengan `Rejected`, `ReturnedForRevision`, `Cancelled`, dan opsional `Expired`. Expired **bukan** approved |
| Urutan | Approval terjadi **sebelum** mutasi finansial efektif. `PendingApproval` tidak mengubah canonical financial state |
| Fail-closed | Ketiadaan checker, policy/threshold valid, atau authority yang dapat ditentukan **mempertahankan** `PendingApproval` atau `BlockedByPolicyConfiguration`. SLA hanya memicu eskalasi, **tidak pernah** bypass |

Yang **belum** ditetapkan hanyalah angkanya: `RJ-BIL-OQ-004` — matriks nominal/risiko yang menentukan
kapan supervisor approval diperlukan. Ini **tidak** memblokir, karena `RJ-BIL-GATE-DEC-006` sudah
menetapkan perilaku saat threshold belum ada: permintaan **tetap** `PendingApproval` /
`BlockedByPolicyConfiguration`. Pola yang sama sudah dipakai `RJ-BIL-DEC-010` pada `BE-007`.

## 3. Kapabilitas yang sudah ada di source

Kolom `Reuse` pada roadmap berbunyi *"Workflow maker-checker existing"*. Kapabilitas itu memang ada,
dan lebih lengkap dari dugaan:

| Komponen | Lokasi | Catatan |
|---|---|---|
| `WorkflowService` | `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs` | `3.384` baris; `CreateAsync`, `SubmitAsync`, `ApproveAsync` |
| `TrxWorkflowInstance` | `.../WorkflowManagement/Models/` | Punya `ReferenceType` + `ReferenceId` — **polimorfik**, jadi modul mana pun secara teknis bisa menempel |
| `MstApprovalMatrix` | `.../MasterData/Workflow/Models/` | Punya `MinimumAmount`, `MaximumAmount`, `CurrencyCode`, `EffectiveStartDate/EndDate`, `Priority`, `IsFallback` |
| `TrxApprovalAction` | `.../WorkflowManagement/Models/` | Punya `IdempotencyKey`, delegasi, snapshot alasan, `IsSystemAction` |
| `TrxApprovalDelegation`, `MstApprovalDelegationPolicy` | `.../Workflow*/Models/` | Delegasi berbatas waktu |
| `TrxWorkflowStatusHistory` | `.../WorkflowManagement/Models/` | Jejak status |

Mesin ini **berbentuk generik**. Ambang nominal yang dibutuhkan `RJ-BIL-OQ-004` bahkan sudah ada
tempatnya di `MstApprovalMatrix`.

## 4. Tiga temuan yang membuat pemakaian ulang tidak sesederhana kelihatannya

### 4.1 Mesinnya milik Kepegawaian, bukan milik bersama

| Bukti | Nilai |
|---|---|
| Lokasi seluruh entity dan service | `Areas/Corporate/HumanResource/` |
| `MstWorkflowDefinition.WorkflowCategory` | default `"HumanResource"` |
| `MstWorkflowDefinition.RequestType` | `LeaveRequest`, `OvertimeRequest`, `TravelRequest`, `ExpenseReimbursement`, `ScheduleChange`, `ShiftSwap`, `PayrollAdjustment`, `PerformanceReview`, `Credentialing`, `TrainingRequest`, `Other` — **tidak ada satu pun jenis finansial Billing** |
| Konteks actor | membawa `WorkforceProfileId`, `EmployeeCategoryId`, `EmploymentTypeId` |
| Sumber approver | `RequesterManager`, `ManagerLevel`, `Position`, `OrganizationUnit`, `DepartmentHead`, `OrganizationHead`, `SiteHr`, `CorporateHr` — struktur organisasi kepegawaian. Hanya `Role` dan `SpecificUser` yang netral |

Memakainya berarti Billing menulis baris ke tabel milik modul lain, dan kebenaran finansial Rawat
Jalan bergantung pada schema serta master data yang bukan miliknya. Ini kelas persoalan yang sama
dengan `QBE-MOD-002` yang baru saja ditutup untuk Lab.

### 4.2 Self-approval dapat dinyalakan lewat konfigurasi

`MstWorkflowStep.AllowSelfApproval` adalah `bool` per step, default `false` — **tetapi dapat diubah
menjadi `true`** melalui `WorkflowStepController`.

`RJ-BIL-GATE-DEC-006` menyatakan larangan self-approval **tanpa syarat**. Bila Billing menumpang
mesin ini, seorang admin workflow di modul Kepegawaian dapat menyalakan `AllowSelfApproval` pada step
Billing dan **mematikan invariant finansial yang sudah Anda kunci — tanpa Billing mengetahuinya.**

### 4.3 Penjagaannya hanya di satu titik, dan bukan di titik persetujuan

| Pemeriksaan | Hasil |
|---|---|
| `AllowSelfApproval` dirujuk di `WorkflowService.cs` baris | `543` (saat menyusun assignment) dan `2954` (hanya dibaca untuk response) |
| `ApproveAsync` (baris `898` dst.) membandingkan actor dengan `RequestedByUserId` | **Tidak** |
| `ApprovalDelegationService.cs` (`2.739` baris) merujuk `RequestedByUserId` | **Tidak sama sekali** |

Artinya penyaringan maker hanya terjadi **sekali, saat assignment dibuat**. Tidak ada pemeriksaan
kedua pada saat persetujuan diberikan, dan delegasi tidak pernah dibandingkan dengan pengaju.
`RJ-BIL-GATE-DEC-006` justru menyebut kasus ini secara eksplisit: *"Delegation tidak boleh membuat
orang efektif yang sama menjadi keduanya."*

Selain itu, ketika tidak ada approver valid, mesin ini **menggagalkan permintaan dengan `400`**
(baris `556`). `RJ-BIL-GATE-DEC-006` menuntut sebaliknya: permintaan **bertahan** sebagai
`PendingApproval` atau `BlockedByPolicyConfiguration`.

> Temuan `4.2` dan `4.3` adalah pengamatan read-only terhadap modul milik pihak lain, dibatasi pada
> ketiga titik penegakan di atas. Ini **bukan** laporan cacat modul Kepegawaian — untuk cuti dan
> lembur perilaku itu mungkin memang dikehendaki. Yang dinyatakan di sini sempit: perilaku itu
> **tidak sepadan** dengan invariant finansial yang dikunci `RJ-BIL-GATE-DEC-006`.

## 5. Keputusan yang diperlukan sebelum ada code ditulis

`RJ-BIL-BE-006-OQ-001` — **Dari mana `RJ-BIL-BE-006` memperoleh maker-checker?**

| Opsi | Konsekuensi |
|---|---|
| `A` Pakai mesin Workflow Kepegawaian apa adanya, dengan `ReferenceType = "BilFinancialAction"` | Paling cepat. Tetapi Billing menulis ke tabel modul lain, dan dua invariant `GATE-DEC-006` tidak terjamin: self-approval dapat dinyalakan lewat konfigurasi, dan delegasi tidak dibandingkan dengan pengaju |
| `B` Billing memiliki approval-nya sendiri dengan prefix `Bil` | Invariant `GATE-DEC-006` ditegakkan di dalam code Billing dan **tidak dapat dimatikan** dari layar konfigurasi modul lain. Harganya: kapabilitas serupa ada di dua tempat |
| `C` Minta owner Workflow mengangkat mesin itu menjadi milik bersama lintas modul | Paling benar secara arsitektur. Tetapi owner Workflow **belum bernama**, sehingga `BE-006` berhenti lagi untuk waktu yang tidak dapat diperkirakan |

Catatan: `B` tidak menutup jalan ke `C`. Bila kelak mesin bersama tersedia, permintaan approval
Billing dapat dicerminkan ke sana tanpa memindahkan kewenangan finansialnya.

## 6. Gate `qv-build-be`

| Gate | Hasil |
|---|---|
| Blueprint canonical ada | `PASS` |
| Task approved/ready | `PASS` — `APPROVED_FOR_EXECUTION` |
| Requirement/decision ID | `PASS` — `RJ-BIL-GATE-DEC-006` |
| Contract version | `PASS` — `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` |
| Dependency teknis | `PASS` — gerbang penutupan folio `BE-007` sudah tersedia |
| Acceptance criteria | `PASS` — tiga kriteria tertulis |
| Test plan | `PASS` — authorization/integration/audit test |
| Manifest revision/hash | `PASS` — revision `14`, seluruh hash cocok per `2026-08-27` |
| Dependency owner | **`FAIL`** — kolom `Dependency` menuntut Workflow, Finance, dan Security owner; ketiganya belum bernama |
| `BUILDER_EXECUTION` | **`FAIL`** — `NOT_AUTHORIZED` |

**Verdict: `BLOCKED_BY_OWNER_DECISION`.** Dua gate gagal, dan keduanya hanya dapat dibuka pemilik
blueprint. Tidak ada code yang ditulis.

## 7. Yang akan dikerjakan begitu keputusan turun

Tidak dituliskan di sini sebagai rancangan, karena bentuknya ditentukan jawaban
`RJ-BIL-BE-006-OQ-001`. Yang pasti tidak berubah apa pun jawabannya: ketiga acceptance criteria
`RJ-BIL-BE-006` — self-approval ditolak, pending approval tidak mengubah state, dan close ditolak
saat reconciliation pending — beserta seluruh butir terkunci pada bagian `2`.
