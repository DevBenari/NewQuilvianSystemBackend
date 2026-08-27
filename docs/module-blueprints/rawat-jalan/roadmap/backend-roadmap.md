# Roadmap Backend — Rawat Jalan Billing

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revision `13` |
| Roadmap revision | `1` — **tidak dinaikkan**; pembaruan ini hanya menyentuh status eksekusi, tidak menyentuh cakupan, acceptance criteria, maupun dependency task mana pun |
| Status roadmap | `APPROVED_FOR_EXECUTION` — approval seluruh task diberikan; handoff dan wewenang tulis tetap wajib saat eksekusi |
| Scope | Core internal/manual |
| Contract | `RJ-BIL-CONTRACT-001@1.0.0` |
| Decision revision | `12` |
| Domain architecture | revision `1`, core independen dari `DOMAIN_ARCHITECTURE_PARTIAL` |
| Backend source SHA | `6b25e60` cabang `sukmagp`; working tree `RJ-BIL-BE-003`, `RJ-BIL-BE-007`, dan remediasi penamaan QBE belum di-commit |
| Frontend source SHA | `29422c83eaf6fd231cbb72f2ba04e306367934e1` cabang `QuilvianDevV2` |
| Approval | `OWNER_APPROVED` pada `2026-08-21` |
| External adapter | `RJ-BIL-DEP-009 = INACTIVE / OUT OF CURRENT DELIVERY SCOPE` |
| Task approval | `RJ-BIL-BE-001` s.d. `RJ-BIL-BE-009` disetujui pengguna pada `2026-08-21` |
| Status seluruh task revision 1 | `APPROVED_FOR_EXECUTION` |
| IMPLEMENTATION_AUTHORITY | `GRANTED` untuk `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, dan `RJ-BIL-BE-007` |
| BUILDER_EXECUTION | `EXECUTED` untuk `RJ-BIL-BE-001`, `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-007`; task lain `NOT_AUTHORIZED` |
| Progress | `4` dari `9` task backend selesai per `2026-08-27`; governance `RJ-BIL-BE-003` dan `RJ-BIL-BE-007` masih `OPEN`; `QBE-MOD-002` untuk modul `Lab` sudah `CLOSED` |
| Pembaruan status | `2026-08-27` — penandaan status task. **Tanpa build**, tanpa eksekusi task baru, tanpa mutasi database |

## Arti tanda status

| Tanda | Arti | Syarat |
|---|---|---|
| ✅ | **SELESAI** | Code sudah dibuat, build lulus, dan test acceptance-nya lulus dengan bukti tercatat |
| 🟡 | **CODE SIAP, BELUM DI-BUILD** | Code sudah dibuat, tetapi build dan test belum dijalankan — sehingga belum ada bukti apa pun bahwa code itu benar-benar berjalan |
| ⛔ | **TERBLOKIR** | Tidak dapat dikerjakan sebelum satu keputusan, satu penunjukan owner, atau task pendahulunya selesai |

Tanda ini menilai **keadaan build**, bukan restu governance. Sebuah task dapat bertanda ✅ sementara
governance sign-off-nya masih `OPEN`; kolom `Governance` pada tabel di bawah menjaga perbedaan itu
tetap terbaca. ✅ **tidak pernah** berarti boleh deploy.

## Progress eksekusi

| Task | Tanda | Keadaan | Governance | Bukti |
|---|---|---|---|---|
| `RJ-BIL-BE-001` | ✅ | SELESAI | — | [execution-evidence-RJ-BIL-BE-001.md](../execution-evidence-RJ-BIL-BE-001.md) |
| `RJ-BIL-BE-002` | ✅ | SELESAI | `RJ-BIL-BE-002-BLOCKER-001` terbuka — pintu masuk telaah farmasi; tidak menahan acceptance criteria maupun task berikutnya | [execution-evidence-RJ-BIL-BE-002.md](../execution-evidence-RJ-BIL-BE-002.md) |
| `RJ-BIL-BE-003` | ✅ | SELESAI — `39` test berbasis database yang sebelumnya tertahan kini sudah dijalankan dan lulus | Sign-off Lab, Clinical Governance, dan Billing/Finance tetap `OPEN` | [execution-evidence-RJ-BIL-BE-003.md](../execution-evidence-RJ-BIL-BE-003.md); [preflight-RJ-BIL-BE-003.md](../preflight-RJ-BIL-BE-003.md); [be-rj-bil-003-remediasi-penamaan-qbe.md](../task/report/backend/be-rj-bil-003-remediasi-penamaan-qbe.md) |
| `RJ-BIL-BE-004` | ⛔ | **TERBLOKIR, menunggu penunjukan owner `RadiologyManagement` dan kenaikan prefix `Rad` dari `PLANNED` ke `ACTIVE` lebih dulu** | Radiology owner belum ada; Clinical Governance `OPEN` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `19`; area `RadiologyManagement` belum ada di source |
| `RJ-BIL-BE-005` | ⛔ | **TERBLOKIR, menunggu keputusan owner atas `RJ-BIL-CONFLICT-001` lebih dulu** — bentuk allocation ditentukan `RJ-BIL-OQ-001`, `OQ-002`, dan `OQ-005` | Direksi Rumah Sakit dan joint clinical-financial governance | [RJ-BIL-CONFLICT-001-source-audit.md](../RJ-BIL-CONFLICT-001-source-audit.md) bagian `12` |
| `RJ-BIL-BE-006` | ⛔ | **TERBLOKIR, menunggu `BUILDER_EXECUTION` dinaikkan dari `NOT_AUTHORIZED` lebih dulu.** Dependency teknisnya sudah terpenuhi sejak `RJ-BIL-BE-007` selesai, tetapi kolom `Dependency` juga menuntut Workflow, Finance, dan Security owner yang belum bernama | `RJ-BIL-GATE-DEC-006` berstatus `locked-draft`; formal sign-off Finance, Security/Privacy, dan delegated executive `OPEN`. Matriks nominal approval `RJ-BIL-OQ-004` belum ditetapkan | `RJ-BIL-DEC-008`; `RJ-BIL-GATE-DEC-006` |
| `RJ-BIL-BE-007` | ✅ | SELESAI — `37` test acceptance lulus di dalam suite `111` test | Billing/Finance dan Integration owner tetap `OPEN` | [be-rj-bil-007-reconciliation-case-dan-recovery-status.md](../task/report/backend/be-rj-bil-007-reconciliation-case-dan-recovery-status.md) |
| `RJ-BIL-BE-008` | ⛔ | **TERBLOKIR, menunggu `RJ-BIL-BE-005` selesai lebih dulu** — claim dan settlement per payer juga bergantung pada `RJ-BIL-OQ-007` | Payer, Finance, dan Integration owner | [RJ-BIL-CONFLICT-001-source-audit.md](../RJ-BIL-CONFLICT-001-source-audit.md) bagian `12` |
| `RJ-BIL-BE-009` | ⛔ | **TERBLOKIR, menunggu `RJ-BIL-BE-001` s.d. `RJ-BIL-BE-008` selesai lebih dulu** — cakupannya menutup coverage gap seluruh task | QA dan domain owner | Kolom `Dependency` tabel task: `BE-001..008` |

### Tidak ada task bertanda 🟡 per `2026-08-27`

Tanda 🟡 berarti code sudah ditulis tetapi belum pernah di-build. Per tanggal ini tidak ada task
yang berada dalam keadaan itu, dan hal tersebut dapat diperiksa **tanpa menjalankan build**:

| Pemeriksaan | Hasil |
|---|---|
| Berkas `.cs` terbaru pada working tree | `2026-08-27 11:39` |
| Hasil build terakhir, `bin/Debug/net9.0/QuilvianSystemBackend.dll` | `2026-08-27 12:04` |
| Kesimpulan | Hasil build **lebih baru** daripada seluruh source; tidak ada code yang belum ter-build |
| Test terakhir | `111` lulus, `0` gagal |

Pembaruan pada `2026-08-27` sesudah pukul `12:04` hanya menyentuh berkas `.md`, sehingga tidak ada
source yang berubah setelah build terakhir.

## Keadaan migration per `2026-08-27`

Diverifikasi dengan `dotnet ef migrations list --no-build` — tanpa build, read-only terhadap
database:

| Pemeriksaan | Hasil |
|---|---|
| Migration berstatus `(Pending)` | `0` |

Artinya seluruh migration blueprint ini sudah terpasang pada database pengembangan bersama
`QuilvianNewDevTim01`, termasuk ketiga migration yang pada revisi sebelumnya masih tercatat
tertunda:

| Migration | Catatan revisi sebelumnya | Keadaan sebenarnya per `2026-08-27` |
|---|---|---|
| `20260824091610_AddLaboratorySpecimenLifecycle` | *"tidak diterapkan ke database mana pun"* | **Sudah diterapkan** |
| `20260826101500_RenameClinicalMilestoneFactToBillingOwnership` | *"belum diterapkan"* | **Sudah diterapkan** |
| `20260827040349_AddBillingReconciliationCase` | — | **Sudah diterapkan** |

Ketiganya terpasang melalui `Database.Migrate()` yang dipanggil `BillingTestDatabaseFixture`
sebelum test pertama, di bawah otorisasi `RJ-BIL-DEC-009`. Penerapan terbatas pada database
pengembangan. Penanda `prod`, `production`, `live`, `staging`, `stage`, dan `uat` ditolak **mutlak**
oleh penjagaan fixture dan tidak mengenal opt-in apa pun; hal itu dikunci oleh test tersendiri.

Migration `20260821033911_AddBillingOperationalBaseline` diterapkan lebih dahulu atas otorisasi
terpisah pada `2026-08-21`, dan dua migration `RJ-BIL-BE-002` —
`20260824074649_AddClinicalMilestoneFactHandoff` serta `20260824080430_StoreClinicalFactSnapshotAsText` —
ikut terpasang melalui jalur fixture yang sama. Riwayat dan dampaknya ada pada bagian `8`
[execution-evidence-RJ-BIL-BE-002.md](../execution-evidence-RJ-BIL-BE-002.md).

## Catatan blocker yang masih berjalan

Audit read-only `RJ-BIL-CONFLICT-001` per `2026-08-24` menyimpulkan konflik `CONFIRMED` dengan
source confidence `HIGH`, tanpa memerlukan perubahan code saat ini. Cakupan `RJ-BIL-BE-005` —
allocation multi-payer dan patient responsibility — belum dapat dirancang sebelum `RJ-BIL-OQ-001`,
`OQ-002`, dan `OQ-005` dijawab, karena bentuk allocation-nya ditentukan jawaban tersebut.
`RJ-BIL-BE-006` dan `RJ-BIL-BE-008` terdampak tidak langsung karena keduanya bekerja di atas hasil
allocation.

`RJ-BIL-BE-002` menyisakan satu blocker kebijakan, `RJ-BIL-BE-002-BLOCKER-001`, yaitu pintu masuk
telaah farmasi setelah kewenangan finansial klinis dihapus. Blocker itu tidak menahan acceptance
criteria `RJ-BIL-BE-002` dan tidak menahan task berikutnya.

Preflight read-only `RJ-BIL-BE-003` per `2026-08-24` menyimpulkan lifecycle Lab sudah terkunci penuh
pada `RJ-BIL-GATE-DEC-003`, sehingga tidak ada SOP lab yang perlu dikarang. Empat pertanyaan pemilik
`RJ-BIL-OQ-008` s.d. `RJ-BIL-OQ-011` dijawab author pada tanggal yang sama, lalu `RJ-BIL-BE-003`
dieksekusi. Kelayakan tagih dinilai per specimen/komponen pemeriksaan, sehingga satu pesanan
`Rp450.000` yang salah satu komponennya ditolak menagih `Rp350.000`.

## Remediasi penamaan QBE per `2026-08-26`

`RJ-BIL-BE-002` dan `RJ-BIL-BE-003` dikerjakan tanpa pernah melewati `AGENTS.md` dan
`docs/engineering/`. Tiga entity melanggar `QBE-NAM-001`, `QBE-NAM-002`, dan `QBE-MOD-002`, lalu
diperbaiki:

| Sebelum | Sesudah | Dampak schema |
| --- | --- | --- |
| `TrxLabSpecimen` | `LabSpecimen` | Nihil — nama tabelnya memang sudah `LabSpecimen` sejak `20260824091610`; yang salah hanya nama kelasnya |
| `TrxLabTransitionHistory` | `LabTransitionHistory` | Nihil, dengan alasan yang sama |
| `TrxClinicalMilestoneFact` | `BilClinicalMilestoneFact`, berpindah ke `BillingManagement/Operational/` | Rename tabel yang mempertahankan data melalui `20260826101500` |

`QBE-MOD-002` kini **`CLOSED`**. `LaboratoryManagement / Lab` sudah berstatus `ACTIVE` pada
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `18`, melalui `RJ-BIL-DEC-007` yang
menunjuk **Sukma Giri** sebagai Product/Domain Owner `LaboratoryManagement`.
`FORMAL_LAB_GOVERNANCE_SIGNOFF` dan `CLINICAL_GOVERNANCE_SIGNOFF` tetap `OPEN`. Rincian lengkap
beserta bukti verifikasi ada pada
[task/report/backend/be-rj-bil-003-remediasi-penamaan-qbe.md](../task/report/backend/be-rj-bil-003-remediasi-penamaan-qbe.md).

## `RJ-BIL-BE-007` selesai per `2026-08-27`

Ketiga acceptance criteria terbukti melalui `37` automated test, di dalam suite `111` test yang
seluruhnya lulus. Empat kemampuan yang ditambahkan: pemindaian rekonsiliasi yang idempoten,
kepemilikan dan penyelesaian case, gerbang kesiapan penutupan folio, dan pencarian status
pemrosesan kanonik berdasarkan identitas sumber.

Delapan endpoint di bawah `api/v1/health-services/billing-management/reconciliation` dengan empat
permission terpisah — `Read`, `Scan`, `Assign`, dan `Resolve` — beserta dua tabel baru,
`BilReconciliationCase` dan `MstBillingReconciliationPolicy`. Tidak ada satu pun endpoint di sana
yang memindahkan uang.

Menjalankan test terhadap database sungguhan untuk pertama kalinya sejak `RJ-BIL-BE-002`
memunculkan lima cacat yang sebelumnya tidak terlihat — termasuk satu celah kehilangan pendapatan
pada `RJ-BIL-BE-003`, di mana sampel tetap dinyatakan layak sementara fakta tagihannya ditolak
diam-diam. Seluruhnya diperbaiki dan dicatat pada bagian `7` laporan task.

Satu hal tetap terbuka dan bukan milik blueprint ini: `MstRegister` tidak memiliki migration di
mana pun, sehingga database yang benar-benar baru tidak akan memilikinya. Dilaporkan kepada
pemilik modulnya melalui `RJ-BIL-NOTICE-001`.

## Aturan eksekusi

Roadmap ini telah disetujui untuk eksekusi task. Builder tetap memerlukan handoff task,
wewenang tulis backend, dan QBE preflight pada waktu eksekusi. Tidak ada task di bawah ini yang memberi
izin migration apply, database mutation, deployment, commit, atau publish.

## Task backend

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| ✅ `RJ-BIL-BE-001` | Menetapkan baseline dan memperkeras Billing Folio/Charge Operational | `RJ-BIL-GATE-DEC-001`, `008`; `RJ-BIL-CAP-012`, `017` | API/State/Validation `1.0.0` | `BilFolio`, `BilChargeLine`, `BilChargeComponent`, `BilProcessingEffect` working tree | Preflight QBE, configuration path, DbContext, unique/index/concurrency, API validation, audit, migration plan; tidak menjalankan migration | `RJ-BIL-DEP-001`, `006`, `008` | Folio unik per encounter; duplicate key menghasilkan replay; stale version ditolak; tidak ada clinical financial mutation | Build backend, targeted integration test, migration review, permission review | Working tree belum committed; Backend owner | Source/build/test evidence, migration artifact reviewed, no unauthorized DB apply |
| ✅ `RJ-BIL-BE-002` | Menyediakan clinical fact handoff yang idempotent untuk Prescription dan Procedure | `RJ-BIL-GATE-DEC-001`, `005`, `008`; `RJ-BIL-CAP-005`, `008` | Integration `RJ-BIL-INT-001@1.0.0` | Prescription/procedure lifecycle existing | Adapter producer fact, stable source/version, milestone mapping, retry/outcome contract; deprecate financial authority legacy secara bertahap | `RJ-BIL-BE-001`, Pharmacy, Clinical | Clinical endpoint tidak menetapkan `Paid`; retry tidak menggandakan charge; correction memakai version baru | Contract test + replay/concurrency test + audit evidence | Pharmacy/Clinical/Billing owners | Producer contract reviewed, tests pass, compatibility notes recorded |
| ✅ `RJ-BIL-BE-003` | Menyediakan Lab milestone minimal sampai `Accepted` | `RJ-BIL-GATE-DEC-003`; `RJ-BIL-CAP-010` | State/Validation `RJ-BIL-STATE-001@1.0.0` | Existing `LabOrder` sebagai start point | Extend lifecycle order/specimen/acceptance boundary dan emit fact; result release tetap scope Lab | `RJ-BIL-BE-001`, Lab owner | `Requested/Collected/Received` tidak menjadi initial charge; `Accepted` menghasilkan eligibility fact; rejected/recollection berhistori | Domain/state test, safety test, integration replay | Lab/Clinical Governance | Lifecycle evidence, acceptance test matrix updated, no invented SOP |
| ⛔ `RJ-BIL-BE-004` | Menetapkan Radiology operational boundary dan acquisition fact | `RJ-BIL-GATE-DEC-004`; `RJ-BIL-CAP-011` | State/Integration `RJ-BIL-STATE-001@1.0.0`, `RJ-BIL-INT-001@1.0.0` | Shared procedure/tariff/Encounter reference | New Radiology capability design/implementation contract; safety gate, study, repeat/abort, usable acquisition fact; tidak mengaktifkan external RIS/PACS | `RJ-BIL-BE-001`, Radiology owner, Clinical Governance | Acquisition ditolak tanpa identity/safety gate; performed usable study menjadi eligibility; repeat mempertahankan original | Domain/integration/safety test | Radiology/Clinical Governance | Scope/owner/SOP evidence approved; external integration remains inactive |
| ⛔ `RJ-BIL-BE-005` | Menyediakan allocation multi-payer dan patient responsibility | `RJ-BIL-GATE-DEC-002`; `RJ-BIL-CAP-013` | API/Validation `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` | `EncounterId`, payer reference, tariff snapshot | New allocation version, nominal absolute, residual, payer decision reference, over-allocation guard | `RJ-BIL-BE-001`, Payer owner | Rp1.000.000 dapat menjadi A Rp600.000 + B Rp250.000 + patient Rp150.000; superseding version tidak menimpa histori | Domain/API/property test | Billing/Payer/Finance | Allocation contract, invariants, tests, audit evidence |
| ⛔ `RJ-BIL-BE-006` | Menyediakan financial action, approval, close/reopen | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `015` | Permission/State `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` | Workflow maker-checker existing | Void/adjustment/reversal/refund/FOC/write-off, approval policy reference, revalidation, close gates; tidak menghapus original charge | `RJ-BIL-BE-001`, Workflow, Finance, Security | Self-approval ditolak; pending approval tidak mengubah state; close ditolak saat reconciliation pending | Authorization/integration/audit test | Finance/Security | Policy/version evidence, SOD test, rollback/replay evidence |
| ✅ `RJ-BIL-BE-007` | Menyediakan reconciliation case dan recovery status | `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-017` | Integration `RJ-BIL-INT-001@1.0.0` | `BilProcessingEffect` provisional | OutcomeUnknown, partial component, dead-letter/review, status query, case owner/SLA, recovery report | `RJ-BIL-BE-001`, Integration owner | Timeout tidak menggandakan charge; failed component visible; folio close blocked sampai case resolved | Failure-injection/recovery/concurrency test | Billing/Integration | Reconciliation contract, report evidence, unresolved cases visible |
| ⛔ `RJ-BIL-BE-008` | Menyediakan manual payer/claim/settlement workflow internal | `RJ-BIL-GATE-DEC-009`; `RJ-BIL-CAP-022` | Integration `RJ-BIL-INT-001@1.0.0` | Manual operator flow | Authorization, claim, adjudication, settlement status; label `ManualOperator`; external adapter interface only | `RJ-BIL-BE-005`, `007`, Payer/Finance | Claim approved tetap `PaymentPending`; manual outcome tidak disebut external success; rejection mempertahankan charge | Workflow/API/integration test | Payer/Finance/Integration | Manual contract and audit accepted; adapter remains disabled |
| ⛔ `RJ-BIL-BE-009` | Menutup coverage gap automated verification | `RJ-BIL-GATE-DEC-001..009`; `RJ-BIL-CAP-021` | Acceptance `testing/acceptance-test-matrix.md` | Existing source tests bila tersedia | Add targeted test project/spec for lifecycle, duplicate, allocation, correction, approval, outage | BE-001..008 | Semua acceptance critical memiliki bukti test atau gap owner | Test report and traceability review | QA/domain owners | Coverage report, known gaps assigned, no false DONE |

## Dependency sequence

`BE-001 → BE-002/BE-003/BE-004/BE-007 → BE-005 → BE-006 → BE-008 → BE-009`.

`BE-003` dan `BE-004` dapat berjalan paralel setelah kontrak baseline `BE-001` tersedia.
`RJ-BIL-DEP-009` tidak termasuk sequence dan tetap inactive.

**Koreksi `2026-08-26` oleh `RJ-BIL-DEC-008`.** Revisi sebelumnya menuliskan
`... → BE-005 → BE-006/BE-007 → ...`, yang bertentangan dengan kolom `Dependency` pada tabel
task: `BE-007` di sana hanya bergantung pada `BE-001` dan Integration owner. Karena `BE-005`
berstatus `BLOCKED` menunggu `RJ-BIL-OQ-001` s.d. `OQ-007`, urutan lama menghentikan **seluruh**
backend tanpa alasan teknis.

Dua bukti menentukan koreksi ini. Pertama, acceptance criteria `BE-006` berbunyi *"close ditolak
saat reconciliation pending"* — jadi `BE-006` justru bergantung pada `BE-007`, bukan sebaliknya.
Kedua, rekonsiliasi `BE-007` bekerja pada outcome pemrosesan fakta klinis melalui
`BilProcessingEffect` dan tidak menyentuh alokasi multi-payer yang menjadi isi `BE-005`.

Yang mengikat adalah kolom `Dependency` pada tabel task. Baris naratif ini adalah ringkasannya,
bukan sumber kebenaran yang terpisah.

## Handoff builder

Setiap handoff ke `build-module-backend` wajib menyertakan task ID, approval task, contract hash,
source SHA/working tree state, dependency state, QBE preflight, dan bukti acceptance yang diminta.
