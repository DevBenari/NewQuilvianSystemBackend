# Rawat Jalan — Module Status

> **Blueprint ini adalah umbrella end-to-end Rawat Jalan, dari Dokter sampai Revenue Cycle.**
> Ia memuat **dua scope kepemilikan yang terpisah** dan tidak boleh dinilai dengan satu angka.
> Batas keduanya dijelaskan pada [roadmap/doctor-consultation-roadmap.md](roadmap/doctor-consultation-roadmap.md)
> bagian `1`.
>
> | Scope | Prefix | Roadmap | Pemilik |
> |---|---|---|---|
> | **Doctor / Clinical** — sampai `Selesai Konsultasi` dan clinical handoff | `RJ-DOC` | [roadmap/doctor-consultation-roadmap.md](roadmap/doctor-consultation-roadmap.md) | Dokter / Clinical |
> | **Billing / Revenue Cycle** — consumer clinical fact | `RJ-BIL` | [roadmap/backend-roadmap.md](roadmap/backend-roadmap.md), [roadmap/frontend-roadmap.md](roadmap/frontend-roadmap.md) | Billing / Finance / Payer |
>
> **Ketidaklengkapan `RJ-BIL-*` bukan kekurangan developer Dokter.** Folio, charge, tarif, payer,
> alokasi, financial action, rekonsiliasi, klaim, pembayaran, dan settlement adalah
> `DOWNSTREAM — NOT PART OF DOCTOR DEFINITION OF DONE`.

## Status per scope — audit `2026-08-31`

`CURRENT STATE`. Diaudit terhadap backend `HEAD` `801a4f5` cabang `sukmagp` dan frontend `HEAD`
`baca965` cabang `QuilvianDevV2`.

| Scope | Status | Dasar |
|---|---|---|
| **Overall Rawat Jalan End-to-End** | `PARTIAL` | Kedua scope belum selesai |
| **Doctor Clinical Delivery** | `PARTIAL` — roadmap `OWNER_APPROVED`, kontrak `FROZEN`, `RJ-DOC-BE-001` dan `RJ-DOC-BE-002` ✅ `COMPLETE` | Dari `28` capability `MANDATORY`: **`19` `COMPLETE`, `7` `PARTIAL`, `2` `MISSING`**. Jalur `Selesai Konsultasi` yang semula `BROKEN` sudah **disatukan** ke finalisasi canonical `2026-08-31`: konsultasi benar-benar menjadi `Completed`, resep difinalkan, dan kunjungan berhenti di `ConsultationCompleted`. Sisanya adalah pengetatan — validasi mengikat, idempotency/concurrency, penguncian, durabilitas handoff, audit, dan verifikasi |
| **Billing Delivery** | `PARTIAL — NEEDS REVERIFICATION` | Angka `5 dari 9` dan `2 dari 7` adalah snapshot `2026-08-28` yang **tidak** cocok dengan `HEAD`; lihat peringatan di bawah |
| **Cross-module Integration** | `PARTIAL` | Jalur fact tindakan, Lab, dan Radiologi hidup; **jalur fact resep tidak pernah terbit** karena finalisasi konsultasi tidak pernah tercapai |

> **PERINGATAN STALENESS — seluruh angka Billing di bawah adalah `HISTORICAL SNAPSHOT`.**
> Backend `HEAD` `801a4f5` tertinggal `144` commit dari `6b25e60` yang tercatat di sini. Audit
> `2026-08-31` menemukan gambaran Billing **terbalik** dari yang tercatat:
>
> | Task | Klaim revisi `21` | Keadaan pada `HEAD` |
> |---|---|---|
> | `RJ-BIL-BE-004` Radiologi | `NOT_STARTED`, *"area belum ada sama sekali"* | **`SOURCE EXISTS`** — `17` berkas, `RadOrderController`, `RadStudyService`, `RadSafetyGateEvaluator`, dua berkas test. Acceptance **belum** dinilai ulang, sehingga **bukan** `COMPLETE` |
> | `RJ-BIL-BE-006` financial action | `COMPLETE`, *"46 test lulus"* | **`SOURCE ABSENT`** — tidak ada `BilFinancialAction` maupun approval policy |
> | `RJ-BIL-BE-007` rekonsiliasi | `COMPLETE`, *"delapan endpoint"* | **`SOURCE ABSENT`** — `BillingFolioController` hanya `3` route baseline `BE-001` |
> | `RJ-BIL-FE-001`/`FE-002` | `COMPLETE`, *"88 test lulus"* | Layar **ada**; berkas test yang dirujuk **tidak ditemukan** |
>
> Manifest revisi `21` mencatat `BE-006` dan `BE-007` sebagai working tree yang belum di-commit.
> Bukti menunjukkan working tree itu tidak pernah masuk cabang ini.
>
> **Menilai ulang status task Billing adalah wewenang pemilik Billing** dan sengaja tidak
> dilakukan dari task koreksi roadmap Dokter. Keberadaan folder bukan bukti acceptance terpenuhi,
> dan ketiadaan folder pada satu cabang bukan bukti pekerjaan tidak pernah ada.

---

## Billing scope — status revisi `21` (belum diverifikasi ulang)

| Field | Value |
|---|---|
| Blueprint ID | `RJ-BIL-BP-001` |
| Module name | Dokter / Rawat Jalan Billing |
| Revision | `25` |
| Module status | `PARTIAL` |
| Current phase | `RJ-BIL-PH-009` — Delivery Execution |
| Last verified at | `2026-08-28T14:02:11+07:00` |
| Backend source SHA | `6b25e6049e60e055593968abe463262b59842527` cabang `sukmagp` |
| Frontend source SHA | `32db4acbe690c5fa0058e570b46e69f9cb81155a` cabang `QuilvianDevV2` — **tertinggal 1 commit** dari `HEAD` `bd31dc99` per `2026-08-28`; manifest mencatat SHA lain lagi (`ab4bd836`, tertinggal 11 commit). Penyamaannya adalah wewenang pemilik frontend dan **sengaja tidak dilakukan** dari task `RJ-BIL-FE-001` |
| IMPLEMENTATION_AUTHORITY | **Backend** — `GRANTED` untuk `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-006`, dan `RJ-BIL-BE-007`; task lain tetap memerlukan handoff tersendiri. **Frontend** — `GRANTED` sejak `RJ-BIL-DEC-013` (`2026-08-28`) |
| BUILDER_EXECUTION | **Backend** — `EXECUTED` untuk `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-006`, dan `RJ-BIL-BE-007`; `NOT_AUTHORIZED` untuk task lain. Wewenang `RJ-BIL-BE-006` diberikan `RJ-BIL-DEC-012`. **Frontend** — `AUTHORIZED` untuk `RJ-BIL-FE-001`, `RJ-BIL-FE-002` bagian Lab, `RJ-BIL-FE-004`, dan `RJ-BIL-FE-005`; `NOT_AUTHORIZED` untuk `FE-003`, `FE-006`, dan `FE-007` karena endpoint pasangannya belum ada. `EXECUTED` untuk `RJ-BIL-FE-001` dan `RJ-BIL-FE-002` per `2026-08-28` |
| Readiness verdict | `NOT_READY` per `2026-08-24` — [testing/readiness-report.md](testing/readiness-report.md). Verdict baru hanya boleh diterbitkan oleh `verify-module-readiness` yang dijalankan ulang |
| External adapter | `RJ-BIL-DEP-009 = INACTIVE / OUT_OF_SCOPE` |

## Phase state

| Completed phases | Active phases | Blocked phases |
|---|---|---|
| `RJ-BIL-PH-001` — interview/closure; `RJ-BIL-PH-002` — capability audit; `RJ-BIL-PH-003` — requirement gate; `RJ-BIL-PH-004` — core domain architecture; `RJ-BIL-PH-006` — target blueprint draft; `RJ-BIL-PH-007` — owner approval; `RJ-BIL-PH-008` — delivery planning | `RJ-BIL-PH-009` — delivery execution | `RJ-BIL-PH-005` — external adapter activation |

## Delivery state

> Tabel ini menghitung **task Billing saja**. Ia bukan progress developer Dokter, dan tidak boleh
> dipakai untuk menjawab apakah pekerjaan Dokter sudah selesai.

| Backend | Frontend | Integration | Verification |
|---|---|---|---|
| `IN_PROGRESS` — `5` dari `9` task selesai | `IN_PROGRESS` — `2` dari `7` task selesai (`RJ-BIL-FE-001`, `RJ-BIL-FE-002`) | `NOT_STARTED` | `PARTIAL` — `BE-001`, `BE-002`, `BE-003`, `BE-006`, `BE-007`, `FE-001`, dan `FE-002` |

### Delivery state scope Dokter — `CURRENT STATE`

| Contract gate | Backend | Frontend | Verification |
|---|---|---|---|
| ✅ **`COMPLETE / FROZEN`** — `2` dari `2` (`RJ-DOC-INT-001`, `INT-002`) | `IN_PROGRESS` — **`2` dari `7`** task `RJ-DOC-BE-*` selesai (`BE-001` dan `BE-002` ✅ `2026-08-31`); `BE-003` dan `BE-005` `ELIGIBLE` | `NOT_STARTED` — `0` dari `4` task `RJ-DOC-FE-*`; `FE-001` s.d. `FE-003` `ELIGIBLE` | `PARTIAL` — `23` uji acceptance scope Dokter lulus; `155` uji project lulus `0` gagal |

Capability `MANDATORY` per `2026-08-31` sesudah `RJ-DOC-BE-001` dan `RJ-DOC-BE-002`:
**`19` `COMPLETE`** · `7` `PARTIAL` · `2` `MISSING` · `0` `NEEDS CONFIRMATION` — denominator `28`.

Ditutup `RJ-DOC-BE-001`: `CAP-008B` finalisasi resep, `CAP-015` tombol ke finalisasi canonical,
`CAP-016` status `COMPLETED`, `CAP-017` `CompletedAt`/`CompletedByUserId`, dan `CAP-030` satu
canonical completion path. Ditutup `RJ-DOC-BE-002`: `CAP-014` validasi authoritative yang mengikat.

`CAP-021` producer handoff resep **sengaja tetap `PARTIAL`**: jalur emisinya kini benar-benar
terjangkau, tetapi belum dibuktikan uji end-to-end dengan resep, sehingga tidak dinaikkan tanpa
bukti. `CAP-025` audit trail juga **sengaja tetap `PARTIAL`** — perluasannya adalah `RJ-DOC-BE-006`.
Yang masih `PARTIAL`: `CAP-018`, `019`, `020`, `021`, `023`, `025`, `028`. Yang masih `MISSING`:
`CAP-027` dan `CAP-029`.

Capability `CONDITIONAL` (`2`) dan `ARCHITECTURAL INVARIANT` (`3`, seluruhnya `VERIFIED`)
**tidak** ikut dihitung.

| Governance scope Dokter | Keadaan |
|---|---|
| Roadmap | [roadmap/doctor-consultation-roadmap.md](roadmap/doctor-consultation-roadmap.md) revision `5` — **`OWNER_APPROVED`** `2026-08-31`, `RJ-DOC-DEC-001` |
| Kontrak | [contracts/doctor-consultation-contracts.md](contracts/doctor-consultation-contracts.md) — `RJ-DOC-COMPLETION-001@1.0.0` dan `RJ-DOC-HANDOFF-001@1.0.0`, keduanya **`FROZEN`** |
| Open question | **Nihil.** `RJ-DOC-OQ-001` s.d. `OQ-006` seluruhnya tertutup |
| `IMPLEMENTATION_AUTHORITY` | **`GRANTED — RJ-DOC-BE-001` dan `RJ-DOC-BE-002`**; task lain ⛔ `NOT_GRANTED` |
| `BUILDER_EXECUTION` | **`EXECUTED — RJ-DOC-BE-001` dan `RJ-DOC-BE-002`**; task lain ⛔ `NOT_AUTHORIZED` |
| `RJ-DOC-BE-001` | ✅ `COMPLETE` `2026-08-31` — [laporan](task/report/backend/RJ-DOC-BE-001.md) |
| `RJ-DOC-BE-002` | ✅ `COMPLETE` `2026-08-31` — [laporan](task/report/backend/RJ-DOC-BE-002.md) |

> `ELIGIBLE` berarti dependency kontraknya terpenuhi — **bukan** berarti boleh dikerjakan.
> Wewenang tulis diberikan terpisah per task.

### Progress task backend

| Task | Status | Bukti |
|---|---|---|
| `RJ-BIL-BE-001` | `COMPLETE` | [execution-evidence-RJ-BIL-BE-001.md](execution-evidence-RJ-BIL-BE-001.md) |
| `RJ-BIL-BE-002` | `COMPLETE` | [execution-evidence-RJ-BIL-BE-002.md](execution-evidence-RJ-BIL-BE-002.md) |
| `RJ-BIL-BE-005` | `BLOCKED` | Menunggu keputusan owner atas `RJ-BIL-CONFLICT-001` |
| `RJ-BIL-BE-003` | `IMPLEMENTATION_COMPLETE` — governance `OPEN` | [execution-evidence-RJ-BIL-BE-003.md](execution-evidence-RJ-BIL-BE-003.md) |
| `RJ-BIL-BE-004` | `NOT_STARTED` | Greenfield penuh; area `RadiologyManagement` belum ada pada source |
| `RJ-BIL-BE-006` s.d. `RJ-BIL-BE-009` | `NOT_STARTED` | Menunggu dependency roadmap |

Baseline persistence Billing Operational sudah berdiri: empat tabel `BilFolio`,
`BilChargeLine`, `BilChargeComponent`, dan `BilProcessingEffect` beserta delapan index sudah
diterapkan ke database `QuilvianNewDevTim01`, dan keempat acceptance criteria `RJ-BIL-BE-001`
terbukti melalui `10` automated test yang lulus.

`RJ-BIL-BE-002` menutup `RJ-BIL-CONFLICT-006` dan memindahkan kewenangan finansial dari modul
klinis ke Billing. Modul klinis kini hanya menerbitkan fakta melalui tabel baru
`TrxClinicalMilestoneFact`, dan Billing yang menentukan akibat finansialnya. Seluruhnya terbukti
melalui `22` automated test yang lulus.

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
|---|---|---|---|---|
| `RJ-BIL-DEP-009` | Kontrak, keamanan, sandbox/UAT, idempotency, status-query, dan reconciliation external adapter belum tersedia | Payer/Insurance + Integration | External activation | Manual/internal payer workflow tetap berjalan |
| `RJ-BIL-CONFLICT-001` | Payment source encounter as-is one-to-one bertentangan dengan multi-payer target; memblokir `RJ-BIL-BE-005` | Registration + Billing/Finance | `RJ-BIL-BE-005` | Sudah diaudit; `CONFIRMED` dengan source confidence `HIGH`. Lihat temuan di bawah |
| `RJ-BIL-CONFLICT-006` | Pharmacy memiliki financial mutation legacy | Pharmacy + Billing/Payer | — | `CLOSED` pada `2026-08-24` oleh keputusan author `1A` dan `1B`; dilaksanakan pada `RJ-BIL-BE-002` |

`RJ-BIL-CONFLICT-006` sudah dijawab author dan ditutup; pelaksanaannya ada pada
[execution-evidence-RJ-BIL-BE-002.md](execution-evidence-RJ-BIL-BE-002.md).
`RJ-BIL-CONFLICT-001` masih terbuka dan pertanyaannya tersedia siap jawab pada
[owner-decision-request-RJ-BIL-001.md](owner-decision-request-RJ-BIL-001.md).

### Temuan `RJ-BIL-CONFLICT-006` per `2026-08-24` — `CLOSED`

Bagian ini disimpan sebagai riwayat. Keadaan yang dijelaskan di bawah adalah keadaan **sebelum**
`RJ-BIL-BE-002` dikerjakan.

| Field | Nilai |
|---|---|
| Status | `CLOSED` |
| Keputusan author | `1A` menyetujui penonaktifan empat endpoint; `1B` mencabut kewenangan `PaymentStatus` dari pembatalan klinis |
| Pelaksanaan | `RJ-BIL-BE-002` |
| Bukti | [execution-evidence-RJ-BIL-BE-002.md](execution-evidence-RJ-BIL-BE-002.md) |

Keempat endpoint dan lima method workflow finansialnya sudah dihapus dari source, bukan sekadar
disembunyikan dari Swagger. Pembatalan resep tetap ada sebagai alur klinis, tetapi tidak lagi
menulis status pembayaran.

Keadaan sebelum perbaikan, disimpan sebagai riwayat:

`Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` memuat
empat endpoint yang menetapkan status finansial dari alur klinis:

| Route | Method | Hak akses | Baris |
|---|---|---|---|
| `{id}/billing-generated` | `PATCH` | `Prescription : Update` | `379` |
| `{id}/payment-paid` | `PATCH` | `Prescription : Update` | `398` |
| `{id}/insurance-approved` | `PATCH` | `Prescription : Update` | `405` |
| `{id}/payment-waived` | `PATCH` | `Prescription : Update` | `412` |

Artinya siapa pun yang memegang izin klinis `Prescription : Update` dapat menyatakan sebuah
resep lunas. Ini bertentangan langsung dengan acceptance criteria `RJ-BIL-BE-002`, yaitu
*clinical endpoint tidak menetapkan `Paid`*, dan dengan invariant `#3` decision log.

Penelusuran pada `V2QuilvianSystemFrontendDev/src` commit `29422c8` **tidak menemukan satu pun
pemanggilan** terhadap keempat route tersebut. Seluruh `PATCH` dari frontend ke resep hanya
menuju `/autosave` dan `/cancel`. Di sisi backend, keempat method workflow tersebut juga tidak
dipanggil dari mana pun selain controller-nya sendiri.

Perlu dicatat bahwa `CompletePayment` pada baris `461` adalah helper privat, bukan route kelima;
ia implementasi bersama di balik `payment-paid`, `insurance-approved`, dan `payment-waived`.

### Bagian kedua yang tidak boleh disamakan

Decision log menyebut `CONFLICT-006` juga mencakup *cancellation yang mengubah payment status*.
Penelusuran menemukan bahwa bagian ini berperilaku berbeda dari keempat endpoint di atas.

| Aspek | Empat endpoint pembayaran | `PATCH {id}/cancel` |
|---|---|---|
| Konsumen frontend | Tidak ada | **Ada** — `cancelPrescription` pada `prescription-workspace-service.js:94`, dipakai `use-prescription-workspace.js:608` |
| Mutasi finansial | Menetapkan `Paid`, `InsuranceApproved`, `PaymentWaived`, `BillingGenerated` | `PrescriptionWorkflowService.cs:118` menetapkan `PaymentStatus = Cancelled` |
| Risiko penonaktifan | Rendah; tidak ada layar yang rusak | **Tinggi**; membatalkan resep adalah alur dokter yang aktif dipakai |

`CancelAsync` sudah memiliki pengaman: pembatalan ditolak bila resep sudah diproses farmasi
(`QueuedAtPharmacy` sampai `Dispensed`). Namun ia tetap menulis `PaymentStatus` dari modul
klinis, sehingga tetap termasuk pelanggaran ownership yang sama.

### Bentuk keputusan yang dibutuhkan

Keputusan terbelah menjadi dua bagian dengan bobot yang sangat berbeda.

**Bagian A — empat endpoint pembayaran.** Bukan negosiasi *breaking change*, melainkan
persetujuan menonaktifkan permukaan API berbahaya yang sudah tidak dipakai layar mana pun.
Pertanyaannya sempit: boleh dinonaktifkan sebagai bagian `RJ-BIL-BE-002`, dan bila belum boleh,
konsumen mana yang masih membutuhkannya.

**Bagian B — `PATCH {id}/cancel`.** Tidak boleh sekadar dihapus. Alur pembatalan resep oleh
dokter aktif dipakai, sehingga memerlukan rencana peralihan: apakah pembatalan klinis cukup
menetapkan `PrescriptionStatus` dan `FulfillmentStatus` lalu menerbitkan fact ke Billing untuk
menentukan konsekuensi finansialnya, dan bagaimana resep yang sudah memiliki charge diperlakukan.

Arah kebijakan pada `01-requirement-completeness-gate.md` — compatibility boundary terbatas
dengan deprecation bertahap — cocok untuk kedua bagian, tetapi Bagian B memerlukan desain
tersendiri di dalam `RJ-BIL-BE-002`, bukan sekadar penghapusan.

### Temuan `RJ-BIL-CONFLICT-001` per `2026-08-24`

Audit source read-only sudah dilakukan. Rincian lengkap beserta `20` baris bukti `file:line`
ada pada [RJ-BIL-CONFLICT-001-source-audit.md](RJ-BIL-CONFLICT-001-source-audit.md).

| Field | Hasil |
|---|---|
| Status konflik | `CONFIRMED` |
| Source confidence | `HIGH` |
| Perubahan code diperlukan sekarang | `NO` |
| Keputusan domain diperlukan | `YES` |
| Kesiapan `RJ-BIL-BE-005` | `BLOCKED` |

Encounter dikunci satu payment source pada tiga lapisan sekaligus: dokumentasi entity,
konfigurasi EF `HasOne().WithOne()`, dan unique index `IX_TrxPatientEncounterGuarantor_EncounterId`
dengan filter baris hidup. Kontrak registrasi berbentuk XOR tunggal, enum `EncounterPaymentType`
hanya memuat `Cash` dan `Insurance`, dan kiosk frontend mengirim satu asuransi.

Enam konflik terkonfirmasi:

| ID | Isi |
|---|---|
| `-A` | Unique index database mengunci satu payment source per encounter |
| `-B` | Kontrak registrasi hanya menerima satu payer |
| `-C` | Billing tidak memiliki satu pun field payer, sehingga tidak ada tempat untuk allocation |
| `-D` | Multi-payer pernah ada lalu **dihapus** — migration `20260712123508` menghapus `44` kolom |
| `-E` | Empat enum multi-payer tertinggal tanpa satu pun pemakai |
| `-F` | Patient responsibility hidup di lapisan klinis, bukan finansial — beririsan dengan `RJ-BIL-CONFLICT-006` |

Dua catatan yang mengubah bentuk keputusan dibanding rumusan blocker semula. Pertama, multi-payer
bukan kemampuan yang belum sempat dibangun melainkan kemampuan yang dihapus, sehingga pertanyaan
pertama kepada owner sebaiknya *mengapa dulu dihapus dan apakah alasannya masih berlaku*. Kedua,
pembagian dua pihak antara satu asuransi dan pasien sudah berjalan dan sudah tersimpan melalui
`CoveredAmount` dan `PatientPayAmount` pada entity klinis, sehingga yang benar-benar belum ada
adalah lebih dari satu penanggung sekaligus.

`RJ-BIL-OQ-004` — apakah dua asuransi pada satu kunjungan merupakan kasus nyata — adalah
pertanyaan penentu. Bila jawabannya tidak, `RJ-BIL-BE-005` menyusut menjadi pemindahan kepemilikan
angka dari lapisan klinis ke Billing.

`RJ-BIL-BE-002`, `RJ-BIL-BE-003`, dan `RJ-BIL-BE-004` tidak terdampak konflik ini.

## Evidence state

Source `RJ-BIL-BE-001` sudah di-commit. Source `RJ-BIL-BE-002` masih berupa working tree yang
belum di-commit.

| Bukti | Keadaan |
|---|---|
| Build backend | `PASS` — `0` error, `0` warning |
| Migration | `3` migration modul ini: baseline Billing, ledger fakta klinis, dan perubahan tipe kolom snapshot |
| Database | `QuilvianNewDevTim01`; `88` migration terdaftar, `0` pending |
| Test project | `Tests/QuilvianSystemBackend.BillingTests/`, xUnit, terdaftar pada solution |
| Automated test | `22` test, `22` lulus, `0` gagal |
| Permission review | `PASS` terhadap `RJ-BIL-PERM-001@1.0.0` |
| Security review | `PASS` untuk `RJ-BIL-BE-002` |
| Audit | `LoggerService.AuditAsync` pada jalur Billing dan jalur penyerahan fakta klinis; idempotency key disimpan sebagai hash |

Rincian ada pada [execution-evidence-RJ-BIL-BE-001.md](execution-evidence-RJ-BIL-BE-001.md) dan
[execution-evidence-RJ-BIL-BE-002.md](execution-evidence-RJ-BIL-BE-002.md).

Perlu diketahui: `BillingTestDatabaseFixture` menjalankan `Database.Migrate()` sebelum test
pertama, sehingga menjalankan `dotnet test` ikut menerapkan migration ke `QuilvianNewDevTim01`.
Penjelasan dan dampaknya ada pada bagian `8` execution evidence `RJ-BIL-BE-002`.

Cakupan test menutup acceptance criteria `RJ-BIL-BE-001` dan `RJ-BIL-BE-002`. Skenario partial
component, multi-payer allocation, financial correction, maker-checker, dan folio close belum
diuji dan tetap menjadi cakupan `RJ-BIL-BE-009`.

## Next recommended task

> ### `HISTORICAL SNAPSHOT — DO NOT USE AS CURRENT STATUS`
>
> Seluruh bagian ini beserta subbagiannya adalah potret scope Billing per `2026-08-28`. Setiap
> pernyataan `selesai`, `BLOCKED`, dan *"belum ada pada source"* di bawah **belum** diverifikasi
> ulang terhadap `HEAD` `801a4f5`, dan sebagiannya terbukti terbalik — lihat peringatan staleness
> pada bagian `Status per scope`. Menilai ulangnya adalah wewenang pemilik Billing.
>
> Langkah berikutnya untuk **scope Dokter** tidak ada di bagian ini, melainkan pada
> [roadmap/doctor-consultation-roadmap.md](roadmap/doctor-consultation-roadmap.md) bagian `4`:
> gerbang kontrak `RJ-DOC-INT-001` dan `RJ-DOC-INT-002`.

Urutan dependency roadmap adalah `BE-001 → BE-002/BE-003/BE-004 → BE-005 → BE-006/BE-007 →
BE-008 → BE-009`. `RJ-BIL-BE-001` dan `RJ-BIL-BE-002` sudah selesai.

Lima task backend selesai: `RJ-BIL-BE-001`, `002`, `003`, `006`, dan `007`. Empat sisanya
terblokir, dan **tidak satu pun terblokir karena kesulitan teknis.**

### Yang hanya dapat dibuka pemilik

Butir `2` sudah tertutup `2026-08-28`. Dua sisanya berada di luar jangkauan programmer mana pun,
dan keduanya masih menahan sisa modul:

| Urutan | Yang dibutuhkan | Membuka | Cara |
|---|---|---|---|
| 1 | **Jawaban `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005`** | `RJ-BIL-BE-005`, lalu `RJ-BIL-BE-008` | Pertanyaan sudah tersusun siap jawab pada [owner-decision-request-RJ-BIL-001.md](owner-decision-request-RJ-BIL-001.md). `RJ-BIL-OQ-004` adalah penentunya: bila dua asuransi pada satu kunjungan **tidak** terjadi, `RJ-BIL-BE-005` menyusut menjadi pemindahan kepemilikan angka dari lapisan klinis ke Billing |
| 2 | ~~**Wewenang tulis frontend**~~ — **SUDAH DIBERIKAN** | `RJ-BIL-FE-001`, `FE-002` bagian Lab, `FE-004`, dan `FE-005` | **Tertutup `2026-08-28` oleh `RJ-BIL-DEC-013`.** `IMPLEMENTATION_AUTHORITY` frontend `GRANTED`; `BUILDER_EXECUTION` `AUTHORIZED` untuk keempat task itu saja. `FE-003`, `FE-006`, dan `FE-007` tetap `NOT_AUTHORIZED` karena endpoint-nya belum ada |
| 3 | **Penunjukan owner `RadiologyManagement`** dan kenaikan prefix `Rad` dari `PLANNED` ke `ACTIVE` | `RJ-BIL-BE-004` | Pola yang sama sudah dipakai `RJ-BIL-DEC-007` untuk `LaboratoryManagement` |

### Sisa pekerjaan backend

| Task | Keadaan | Yang menahannya |
|---|---|---|
| `RJ-BIL-BE-005` | `BLOCKED` | Butir 1 di atas |
| `RJ-BIL-BE-008` | `BLOCKED` | `RJ-BIL-BE-005`, ditambah `RJ-BIL-OQ-007` |
| `RJ-BIL-BE-004` | `BLOCKED` | Butir 3 di atas. Greenfield penuh — area `RadiologyManagement` belum ada sama sekali pada source, sehingga bebannya jauh lebih besar daripada `RJ-BIL-BE-003` |
| `RJ-BIL-BE-009` | `BLOCKED` | Seluruh task di atas |

### Progress task frontend

Wewenang tulis diberikan `RJ-BIL-DEC-013` pada `2026-08-28`, terbatas pada empat task yang backend
pasangannya sudah selesai.

| Task | Tanda | Status | Bukti |
|---|---|---|---|
| `RJ-BIL-FE-001` | ✅ | `COMPLETE` | [fe-rj-bil-001-baca-tagihan-satu-kunjungan.md](task/report/frontend/fe-rj-bil-001-baca-tagihan-satu-kunjungan.md) |
| `RJ-BIL-FE-002` | ✅ | `COMPLETE` untuk Resep, Tindakan, dan Laboratorium | [fe-rj-bil-002-batas-klinis-dan-finansial.md](task/report/frontend/fe-rj-bil-002-batas-klinis-dan-finansial.md). **Bagian Radiologi tetap ⛔** selama `RJ-BIL-BE-004` terblokir; ketiadaannya diumumkan di layar |
| `RJ-BIL-FE-004` | — | `AUTHORIZED`, belum dikerjakan | — |
| `RJ-BIL-FE-005` | — | `AUTHORIZED`, belum dikerjakan | — |
| `RJ-BIL-FE-003` | ⛔ | `NOT_AUTHORIZED` | Endpoint `RJ-BIL-BE-005` belum ada |
| `RJ-BIL-FE-006` | ⛔ | `NOT_AUTHORIZED` | Endpoint `RJ-BIL-BE-008` belum ada |
| `RJ-BIL-FE-007` | ⛔ | `NOT_AUTHORIZED` | Menunggu `FE-001` s.d. `FE-006` |

**Bukti verifikasi frontend per `2026-08-28`:** `88` test unit lulus, `0` gagal; `next build` lulus
dengan exit `0`; lint berkas Billing `0` masalah. **Component render test belum ada** — harness
project memakai `node --test` tanpa `@testing-library`, sehingga render test memang belum mungkin
ditulis. Penutupannya adalah cakupan `RJ-BIL-FE-007`.

### Gerbang governance yang tidak menahan penulisan code

Keempat butir ini **tidak** menghentikan pekerjaan, tetapi menahan pemakaian untuk pasien
sungguhan. Jangan diperlakukan sebagai sudah beres hanya karena task-nya bertanda ✅:

| Gerbang | Menahan |
|---|---|
| `FORMAL_FINANCE_SIGNOFF` dan `SECURITY_PRIVACY_SIGNOFF` | Aktivasi production `RJ-BIL-BE-006` |
| Sign-off Lab dan Clinical Governance | Aktivasi production `RJ-BIL-BE-003` |
| Sign-off Billing/Finance dan Integration | Aktivasi production `RJ-BIL-BE-007` |
| `FRONTEND_AUTHORITY` visual dan `SECURITY_PRIVACY_SIGNOFF` | Aktivasi production `RJ-BIL-FE-001` dan `RJ-BIL-FE-002`. Kedua layar belum masuk navigasi mana pun dan hanya dapat dicapai dengan mengetik URL |
| Tinjauan domain owner atas batas klinis–finansial | Definition of Done `RJ-BIL-FE-002`. Implementasinya selesai dan teruji, tetapi tinjauannya hanya dapat dilakukan manusia |
| `RJ-BIL-OQ-004` matriks nominal approval | Tindakan finansial yang bergantung ambang berhenti pada `BlockedByPolicyConfiguration` — perilaku yang memang dikunci `RJ-BIL-GATE-DEC-006`, bukan kerusakan |

### Keputusan kebijakan yang masih menggantung

| Butir | Keadaan |
|---|---|
| `RJ-BIL-BE-002-BLOCKER-001` | **Terbuka.** Alur telaah farmasi tidak lagi memiliki pintu masuk setelah kewenangan finansial klinis dihapus. Keputusan kebijakan farmasi; opsi dan dampaknya sudah disiapkan. **Tidak menahan** task mana pun |
| ~~Menyediakan database test khusus~~ | **Selesai `2026-08-27`** melalui `RJ-BIL-DEC-009`: test berjalan terhadap database dev bersama lewat opt-in eksplisit |
| `RJ-BIL-NOTICE-001` | **Terbuka, dan bukan milik blueprint ini.** `MstRegister` tidak memiliki migration di mana pun, sehingga database yang benar-benar baru tidak akan memilikinya. Sudah dilaporkan kepada pemilik modulnya |

`RJ-BIL-BE-002-BLOCKER-001` perlu dijelaskan agar tidak disalahpahami sebagai kerusakan baru.
`PrescriptionReviewService` mensyaratkan status pemenuhan `ReadyForPharmacy` sebelum telaah
farmasi dapat dimulai, dan satu-satunya penulis status itu adalah method pembayaran yang baru
saja dihapus. Karena tidak ada layar yang pernah memanggil endpoint tersebut, alur ini memang
sudah tidak dapat dicapai dari frontend sebelum `RJ-BIL-BE-002` dikerjakan. Penghapusan endpoint
membuat celah yang sudah ada menjadi terlihat, bukan menciptakannya.

`RJ-BIL-BE-003` dan `RJ-BIL-BE-004` menghasilkan fact klinis, bukan alokasi finansial, sehingga
tidak bergantung pada `RJ-BIL-CONFLICT-001`. Keduanya tetap memerlukan handoff task dan wewenang
tulis tersendiri.

Preflight `RJ-BIL-BE-003` per `2026-08-24` mengoreksi satu asumsi pada revisi sebelumnya: lifecycle
Lab **sudah** tersedia sebagai requirement terkunci pada `RJ-BIL-GATE-DEC-003`, yang tidak ada
adalah implementasinya pada source. Modul Laboratorium hanya berisi `4` file untuk membuat dan
membatalkan order, tanpa kolom status dan tanpa entity specimen, serta tanpa satu pun consumer
frontend. Modul Tindakan sudah menolak procedure ber-flag `IsLaboratory`, sehingga tidak ada risiko
tagihan ganda terhadap `RJ-BIL-BE-002`. Rinciannya pada
[preflight-RJ-BIL-BE-003.md](preflight-RJ-BIL-BE-003.md).

`RJ-BIL-BE-004` berbeda keadaannya: area `RadiologyManagement` belum ada sama sekali pada source,
sehingga bebannya jauh lebih besar daripada `RJ-BIL-BE-003`.

> **KOREKSI `2026-08-31` — paragraf di atas adalah `HISTORICAL SNAPSHOT` dan sudah usang.**
> Audit read-only terhadap backend `801a4f5` menemukan `Areas/HealthServices/RadiologyManagement/**`
> **sudah ada**, berisi `17` berkas termasuk `RadOrderController`, `RadStudyController`,
> `RadStudyService`, `RadSafetyGateEvaluator`, model `RadOrder`/`RadStudy`/`RadStudySafetyCheck`,
> dan pemanggilan `ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync` pada
> `RadStudyService.cs:958`. Terdapat pula `Radiology/RadiologyStudyLifecycleTests.cs` dan
> `Radiology/RadiologySafetyGateTests.cs`. Yang **masih** nihil adalah consumer frontend-nya.
>
> Klasifikasi yang berlaku: **`SOURCE EXISTS — ROADMAP TASK STATUS NEEDS REVERIFICATION`.**
>
> `RJ-BIL-BE-004` **tidak** ditandai `COMPLETE` dari koreksi ini. Keberadaan folder dan nama berkas
> bukan bukti bahwa acceptance criteria task terpenuhi — safety gate menolak acquisition tanpa
> identitas, performed study menjadi eligibility, dan repeat mempertahankan original semuanya perlu
> dinilai satu per satu. Penilaian itu, penutupan blocker owner `RadiologyManagement`, dan
> pemutakhiran bukti Billing lainnya adalah **wewenang pemilik Billing**, dan sengaja tidak
> dilakukan dari task koreksi roadmap Dokter.

Builder tetap memerlukan handoff task, wewenang tulis, dan preflight eksekusi untuk setiap task.
Jangan mengaktifkan external adapter `RJ-BIL-DEP-009`.
