# Dokter / Rawat Jalan Billing — Module Status

| Field | Value |
|---|---|
| Blueprint ID | `RJ-BIL-BP-001` |
| Module name | Dokter / Rawat Jalan Billing |
| Revision | `13` |
| Module status | `PARTIAL` |
| Current phase | `RJ-BIL-PH-009` — Delivery Execution |
| Last verified at | `2026-08-24T00:00:00+07:00` |
| Backend source SHA | `36456ead5d8d116e5631aef859df3d55b0ec7e81` cabang `sukmagp` |
| Frontend source SHA | `29422c83eaf6fd231cbb72f2ba04e306367934e1` cabang `QuilvianDevV2` |
| IMPLEMENTATION_AUTHORITY | `GRANTED` untuk `RJ-BIL-BE-001` saja; task lain tetap memerlukan handoff tersendiri |
| BUILDER_EXECUTION | `EXECUTED` untuk `RJ-BIL-BE-001`; `NOT_AUTHORIZED` untuk task lain |
| External adapter | `RJ-BIL-DEP-009 = INACTIVE / OUT_OF_SCOPE` |

## Phase state

| Completed phases | Active phases | Blocked phases |
|---|---|---|
| `RJ-BIL-PH-001` — interview/closure; `RJ-BIL-PH-002` — capability audit; `RJ-BIL-PH-003` — requirement gate; `RJ-BIL-PH-004` — core domain architecture; `RJ-BIL-PH-006` — target blueprint draft; `RJ-BIL-PH-007` — owner approval; `RJ-BIL-PH-008` — delivery planning | `RJ-BIL-PH-009` — delivery execution | `RJ-BIL-PH-005` — external adapter activation |

## Delivery state

| Backend | Frontend | Integration | Verification |
|---|---|---|---|
| `IN_PROGRESS` — `1` dari `9` task selesai | `APPROVED_FOR_EXECUTION`, belum dimulai | `NOT_STARTED` | `PARTIAL` — hanya `RJ-BIL-BE-001` |

### Progress task backend

| Task | Status | Bukti |
|---|---|---|
| `RJ-BIL-BE-001` | `COMPLETE` | [execution-evidence-RJ-BIL-BE-001.md](execution-evidence-RJ-BIL-BE-001.md) |
| `RJ-BIL-BE-002` | `BLOCKED` | Menunggu keputusan owner atas `RJ-BIL-CONFLICT-006` |
| `RJ-BIL-BE-005` | `BLOCKED` | Menunggu keputusan owner atas `RJ-BIL-CONFLICT-001` |
| `RJ-BIL-BE-003`, `RJ-BIL-BE-004` | `NOT_STARTED` | Tidak diblokir konflik mana pun; memerlukan owner domain dan handoff tersendiri |
| `RJ-BIL-BE-006` s.d. `RJ-BIL-BE-009` | `NOT_STARTED` | Menunggu dependency roadmap |

Baseline persistence Billing Operational sudah berdiri: empat tabel `BilFolio`,
`BilChargeLine`, `BilChargeComponent`, dan `BilProcessingEffect` beserta delapan index sudah
diterapkan ke database `QuilvianNewDevTim01`, dan keempat acceptance criteria `RJ-BIL-BE-001`
terbukti melalui `10` automated test yang lulus.

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
|---|---|---|---|---|
| `RJ-BIL-DEP-009` | Kontrak, keamanan, sandbox/UAT, idempotency, status-query, dan reconciliation external adapter belum tersedia | Payer/Insurance + Integration | External activation | Manual/internal payer workflow tetap berjalan |
| `RJ-BIL-CONFLICT-001` | Payment source encounter as-is one-to-one bertentangan dengan multi-payer target; memblokir `RJ-BIL-BE-005` | Registration + Billing/Finance | `RJ-BIL-BE-005` | Sudah diaudit; `CONFIRMED` dengan source confidence `HIGH`. Lihat temuan di bawah |
| `RJ-BIL-CONFLICT-006` | Pharmacy masih memiliki financial mutation legacy; memblokir `RJ-BIL-BE-002` | Pharmacy + Billing/Payer | `RJ-BIL-BE-002` | Lihat temuan di bawah — keputusan menjadi jauh lebih sempit |

Kedua blocker di atas sudah dituangkan menjadi pertanyaan siap dijawab pada
[owner-decision-request-RJ-BIL-001.md](owner-decision-request-RJ-BIL-001.md). Yang menahan
`RJ-BIL-BE-002` dan `RJ-BIL-BE-005` sekarang adalah keputusan owner, bukan kekurangan bukti.

### Temuan `RJ-BIL-CONFLICT-006` per `2026-08-24`

`Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` masih memuat
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

Source `BillingManagement/Operational/**`, `Program.cs`, dan
`Repositories/ApplicationDbContext.cs` sudah di-commit dan tidak lagi berstatus provisional.

| Bukti | Keadaan |
|---|---|
| Build backend | `PASS` — `0` error |
| Migration | `20260821033911_AddBillingOperationalBaseline` di-review statis lalu diterapkan |
| Database | `QuilvianNewDevTim01`; `86` migration terdaftar, `0` pending |
| Test project | `Tests/QuilvianSystemBackend.BillingTests/`, xUnit, terdaftar pada solution |
| Automated test | `10` test, `10` lulus, `0` gagal |
| Permission review | `PASS` terhadap `RJ-BIL-PERM-001@1.0.0` |
| Audit | `LoggerService.AuditAsync` pada jalur sukses dan konflik versi; idempotency key disimpan sebagai hash |

Rincian lengkap ada pada [execution-evidence-RJ-BIL-BE-001.md](execution-evidence-RJ-BIL-BE-001.md).

Cakupan test masih terbatas pada acceptance criteria `RJ-BIL-BE-001`. Skenario
`BIL_IDEMPOTENCY_CONFLICT`, outcome unknown, partial component, multi-payer allocation,
financial correction, maker-checker, dan folio close belum diuji dan tetap menjadi cakupan
`RJ-BIL-BE-009`.

## Next recommended task

Urutan dependency roadmap adalah `BE-001 → BE-002/BE-003/BE-004 → BE-005 → BE-006/BE-007 →
BE-008 → BE-009`. `RJ-BIL-BE-001` sudah selesai, sehingga tiga task dapat dipertimbangkan.

| Urutan | Task | Kesiapan |
|---|---|---|
| 1 | Mengirim [owner-decision-request-RJ-BIL-001.md](owner-decision-request-RJ-BIL-001.md) ke owner | Siap; bukti kedua blocker sudah lengkap dan pertanyaannya sudah berbentuk pilihan |
| 2 | `RJ-BIL-BE-002` | `BLOCKED` sampai pertanyaan `1A` dan `1B` dijawab |
| 3 | `RJ-BIL-BE-005` | `BLOCKED` sampai `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005` dijawab |
| 4 | `RJ-BIL-BE-003` atau `RJ-BIL-BE-004` | Dapat dimulai tanpa menunggu jawaban owner mana pun; memerlukan owner Lab atau Radiology beserta Clinical Governance |

Dua blocker menunggu orang, bukan menunggu pekerjaan teknis. Karena itu langkah pertama adalah
mengirim permintaan keputusan, bukan menunggu lebih lama. Dokumen tersebut menyusun sembilan
pertanyaan beserta pilihan jawaban dan akibat masing-masing, ditambah satu formulir isian, agar
owner dapat menjawab tanpa membaca source.

Bila jawaban owner belum tersedia, `RJ-BIL-BE-003` dan `RJ-BIL-BE-004` tetap dapat dimulai karena
keduanya tidak bergantung pada `RJ-BIL-CONFLICT-001` maupun `RJ-BIL-CONFLICT-006`. Keduanya
menghasilkan fact klinis, bukan alokasi finansial. Yang tetap dibutuhkan adalah owner domain
masing-masing, karena lifecycle Lab dan Radiology belum ada pada source, serta handoff task dan
wewenang tulis tersendiri — `IMPLEMENTATION_AUTHORITY` saat ini hanya mencakup `RJ-BIL-BE-001`.

Builder tetap memerlukan handoff task, wewenang tulis, dan preflight eksekusi untuk setiap task.
Jangan mengaktifkan external adapter `RJ-BIL-DEP-009`.
