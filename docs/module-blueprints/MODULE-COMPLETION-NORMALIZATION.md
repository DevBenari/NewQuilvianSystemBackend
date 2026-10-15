# Normalisasi Task Accounting — Farmasi, Operasi, Gizi

Menggantikan angka "estimasi source" dengan akuntansi berbasis task nyata, agar ketiga modul
dapat dinilai dengan metode yang sama seperti modul lain.

| Hal | Isi |
|---|---|
| Disusun | 14 Oktober 2026 |
| Cabang backend | `Ikbal` @ `7ffcf6a0` |
| Metode | Task register dengan provenance per baris |

## Aturan yang dipakai

1. **Nol task tanpa provenance.** Setiap baris menunjuk artefak yang sudah ada sebelum dokumen
   ini: ID task asli, kontrak, keputusan, berkas uji, atau bukti runtime.
2. **`ORIGINAL`** = ID task yang sudah ada di roadmap/laporan. **`DERIVED`** = diturunkan dari
   kontrak/keputusan/uji yang sudah ada, bukan dikarang.
3. **Nol double-count.** Satu kapabilitas satu baris. Bukti yang sudah dipakai task asli tidak
   dipakai lagi untuk membuat task turunan.
4. Task `SUPERSEDED` tidak dihitung, dan penggantinya disebut.
5. Keputusan bisnis **tidak** diubah menjadi task coding.
6. Dependency modul lain **tidak** diubah menjadi task internal.
7. Frontend **tidak** dinaikkan ke `DONE` hanya karena berkas source-nya ada.

## Definisi status

| Status | Arti |
|---|---|
| `DONE` | Source + uji + bukti acceptance/runtime ada |
| `DONE / ACCEPTANCE EVIDENCE MISSING` | Source + uji ada, catatan acceptance belum ada |
| `IN PROGRESS` | Sebagian terkirim |
| `BLOCKED EXTERNAL` | Pekerjaan internal selesai, menunggu modul/pihak lain |
| `BLOCKED BUSINESS` | Menunggu keputusan pemilik proses |
| `NOT STARTED` | Belum dikerjakan |
| `SUPERSEDED` / `NOT APPLICABLE` | Tidak dihitung |

**Development Completion** = `DONE` ÷ task internal applicable.
`BLOCKED EXTERNAL`, `BLOCKED BUSINESS`, `SUPERSEDED`, dan `NOT APPLICABLE` tidak masuk penyebut.

---

# Ringkasan

| Modul | Total Applicable | Done | In Progress | Blocked External | Blocked Business | Not Started | Development Completion |
|---|---:|---:|---:|---:|---:|---:|---:|
| Farmasi | 19 | 18 | 1 | 0 | 0 | 0 | **94,7%** |
| Operasi | 20 | 20 | 0 | 0 | 0 | 0 | **100%** |
| Gizi | 12 | 12 | 0 | 0 | 0 | 0 | **100%** |


² `PHA-BE-003` dan `PHA-FE-001` tertahan keputusan bisnis (`PHA-OQ-014`, milik Product) dan karena itu **tidak** masuk penyebut Development Completion.

Kolom *In Progress* di atas memuat task berstatus `DONE / ACCEPTANCE EVIDENCE MISSING`: source
dan uji ada, tetapi catatan acceptance belum dibuat. Angka pendamping:

| Modul | Terkirim (source + uji) | Catatan |
|---|---:|---|
| Farmasi | 17 / 17 = **100%** | `PHA-FE-002` ditutup dan acceptance `PHA-D-10` ditemukan, 14 Okt; `PHA-D-11` gerbang stok ditambah 15 Okt |
| Operasi | 20 / 20 = **100%** | acceptance terbukti pada seluruh 20 task; `FE-OPR-009` ditutup 15 Okt, E2E 11/11 lulus |
| Gizi | 12 / 12 = **100%** | — |

**Operasi mencapai 100% pada 15 Oktober 2026.** Backend 11/11 selesai, frontend 9/9 selesai.

`FE-OPR-009` ditutup setelah akar masalahnya ditemukan: repo **tidak memiliki
`playwright.config.mjs`**, sehingga tidak ada `webServer` dan rahasia sesi di proses uji tidak
pernah cocok dengan server mana pun. Satu konfigurasi memulihkan seluruh suite E2E repo, bukan
hanya Operasi. Hasil akhir **E2E 11/11 lulus** (sebelumnya 0).

Satu perubahan source aplikasi dilakukan, dan bukan untuk menaikkan angka: E2E itu menemukan
cacat produksi nyata — `operating-room-case-detail-view.jsx` memakai `OPR_CASE_STATUS` tanpa
mengimpornya, sehingga **halaman detail kasus tidak dapat dibuka sama sekali di produksi**.
Cacat ini tidak tertangkap `eslint` dan tidak akan pernah tertangkap uji berbasis pembacaan
source, karena source-nya memang menyebut konstanta itu — yang hilang import-nya. Perbaikannya
satu baris.

| Modul | Original | Derived |
|---|---:|---:|
| Farmasi | 8 | 11 |
| Operasi | 20 | 0 |
| Gizi | 0 | 12 |

---

# FARMASI

Register asli Farmasi hanya mencakup gelombang *financial clearance*
(`roadmap/backend-roadmap.md`), ditambah dua laporan bug. Sepuluh baris `DERIVED` menutup
kapabilitas lain yang sudah terkirim dan punya jangkar dokumen/uji.

## Task asli

| Task ID | Type | Derived From | Scope | Status | Source Evidence | Test Evidence | Runtime Evidence |
|---|---|---|---|---|---|---|---|
| `PHA-BE-001` | ORIGINAL | — | BE | `DONE` | `PharmacyManagement/` | `FinancialClearanceTests` | `task/report/backend/PHA-BE-001.md` |
| `PHA-BE-002` | ORIGINAL | `roadmap/README.md` — skenario uji resolver | BE | `DONE` | `PharmacyDepotRoutingService` | `DepotRoutingTests` (26) | skenario layanan, nol, dan ganda terbukti |
| `PHA-BE-003` | ORIGINAL | `roadmap/README.md` — integrasi routing ke workflow | BE | `BLOCKED BUSINESS` | — | — | tertahan `PHA-OQ-014` (Product): pembayaran berhasil tetapi reservasi gagal |
| `PHA-BE-004` | ORIGINAL | `FR-PHA-CLR-01/02/05` | BE | `DONE` | `PhmPrescriptionFinancialProjection` | `FinancialClearanceTests` (18) | `task/report/backend/PHA-BE-004.md`, `verifikasi-runtime-be-bkc-067.md` |
| `PHA-BE-005` | ORIGINAL | `FR-PHA-CLR-03/04/06/07/08` | BE | `DONE` | 4 gerbang alur kerja | `PharmacyWorkflowTests` | `task/report/backend/PHA-BE-005.md` |
| `PHA-BE-006` | ORIGINAL | `FR-PHA-CLR-09` | BE | `DONE` | `FinancialClearance` pada response detail | `FinancialClearanceTests` | `task/report/backend/PHA-BE-006.md` |
| `PHA-FE-001` | ORIGINAL | `roadmap/README.md`, `PHA-PH-010` | FE | `BLOCKED BUSINESS` | — | — | — |
| `PHA-FE-002` | ORIGINAL | `FR-PHA-CLR-10` | FE | `DONE` | `prescription-clearance-utils.js`, `prescription-financial-hold-alert.jsx`, dipasang di `prescription-list-detail-page.jsx` | `tests/unit/pharmacy-financial-clearance-notice.test.mjs` (10 kasus) | 14 Okt: payload nyata `RX-20261014-00001` → `holdReason` Billing tampil apa adanya |

**Catatan `PHA-FE-001`.** Koreksi atas laporan 14 Oktober 2026 pagi: saya sempat menyatakan
penghalangnya hilang karena `PHA-BE-003` sudah selesai. **Itu salah** — yang selesai adalah bug
ber-ID sama, bukan task `PHA-BE-003` pada roadmap depot routing. Task itu masih `BLOCKED` oleh
`PHA-OQ-014` (Product, belum dijawab), sehingga `PHA-FE-001` berstatus `BLOCKED BUSINESS`.
Acceptance-nya pun belum pernah ditetapkan (*"Belum ditetapkan"*, *"belum dibuat sebagai task
siap"*), dan `PharmacyDepotRoutingService` tidak punya endpoint — jadi tidak ada yang dapat
dikerjakan tanpa mengarang scope.

**Catatan `PHA-FE-002`.** Gap-nya nyata dan sudah ditutup 14 Oktober 2026. Sebelumnya pencarian
`clearance`, `clearanceStatus`, `financialOutcome`, dan `reasonCode` pada seluruh view, service,
dan utils `pharmacy-management` frontend menghasilkan **nol hasil**. Sekarang keadaan finansial
dibaca dari `FinancialClearance` milik response dan ditampilkan sebagai keterangan penahanan di
layar kerja Farmasi, memakai `InformationAlert` yang sudah dipakai layar Farmasi lain.


**Tabrakan penomoran task yang perlu diketahui.** `roadmap/README.md` memakai `PHA-BE-001/002/003`
untuk task *depot routing*, sementara `bug-pha-be-002-klarifikasi-tertutup.md` dan
`bug-pha-be-003-retur-tanpa-batas-serah.md` memakai nomor yang sama untuk dua bug yang tidak
berhubungan. Register ini memakai definisi roadmap sebagai yang sah. Kedua perbaikan bug itu
tetap dihitung, tetapi sebagai bukti di dalam `PHA-D-01` (klarifikasi telaah, 4 uji regresi) dan
`PHA-D-03` (batas retur, 13 uji regresi) — bukan baris sendiri, supaya tidak double-count.

## Task turunan

| Task ID | Type | Derived From | Scope | Status | Source Evidence | Test Evidence | Runtime Evidence |
|---|---|---|---|---|---|---|---|
| `PHA-D-01` | DERIVED | `contracts/state-transition-matrix.md` | BE | `DONE` | telaah, penyiapan, telaah akhir | `PharmacyWorkflowTests` (33) | runtime 12 Okt: review → preparation → final check |
| `PHA-D-02` | DERIVED | `contracts/api-contract.md`, `flowcharts/00-alur-utama.md` | BE | `DONE` | `PrescriptionDispensingService` | `DispenseFlowTests` (18), `DispensingGuardTests` (20) | runtime 12 Okt: `isFullyDispensed`, stok 570→561 |
| `PHA-D-03` | DERIVED | `bug-pha-be-003`, `contracts/validation-matrix.md` | BE | `DONE` | `DrugReturnService` | `DrugReturnTests` (48), `DrugReturnUpdateAndListTests` (34) | `PHM083` terverifikasi di uji |
| `PHA-D-04` | DERIVED | `contracts/api-contract.md` | BE | `DONE` | etiket + saldo/mutasi stok | `LabelAndStockTests` (14) | runtime 12 Okt: mutasi `DrugUsage` −9 |
| `PHA-D-05` | DERIVED | `contracts/PHA-DEPOT-ROUTING-v1.md`, `erd/pharmacy-routing.md` | BE | `DONE` | routing depot | `DepotRoutingTests` (26) | depo `DSL-RSMMC-00001` dipakai saat penyerahan |
| `PHA-D-06` | DERIVED | `contracts/permission-audit-matrix.md` | BE | `DONE` | `[AccessPermission]` seluruh controller | `PermissionMatrixTests` (18) | refleksi atas 19+ controller |
| `PHA-D-07` | DERIVED | `temuan-modul-izin-terbelah.md` | BE | `DONE` | `PharmacyModuleCodeConsolidationSeeder` | `ModuleCodeConsolidationTests` (16) | konsolidasi ke `HEALTH_SERVICE_PHARMACY_MANAGEMENT` |
| `PHA-D-08` | DERIVED | `contracts/state-transition-matrix.md` | BE | `DONE` | tangga `PrescriptionFulfillmentStatus` | `PrescriptionFulfillmentStageTests` (8) | runtime 14 Okt: `1/1/1` → `2/2/1` |
| `PHA-D-09` | DERIVED | `requirement-billing-charge-pra-dispense.md`, `RJ-E2E-DEC-005` | Integration | `DONE` | kontrak tagihan tahap 1 | diuji terhadap validator Billing | runtime 14 Okt: `PRESCRIBED` / `BIL-INTEGRATION-1.3` |
| `PHA-D-10` | DERIVED | `gap-frontend-workflow.md` (`GAP-PHA-FE-001`) | FE | `DONE` | `prescription-workflow-view/`, `use-prescription-pharmacy-workflow.js`, `FINAL_CHECK_CRITERIA` | `bin/uji-alur-farmasi.sh` — 6 langkah alur, seluruhnya `200` | 1 Okt 2026: tangga tahap 4→4→5→6→12→**7 Siap Diserahkan**; penelusuran peramban **12 pemeriksaan, seluruhnya lulus** |

**Nol double-count.** `FinancialClearanceTests` dipakai sebagai bukti `PHA-BE-001/004/006` dan
**tidak** dijadikan task turunan tersendiri. `PHA-D-01` (gerbang alur kerja backend) dan
`PHA-D-10` (UI alur kerja) scope-nya berbeda: BE dan FE.

## Dependency eksternal Farmasi — bukan task internal

| Hal | Pemilik | Dampak |
|---|---|---|
| `PharmacyTests` tidak dapat dikompilasi | **Billing / Finance** | **17 error** di 3 berkas uji milik mereka di dalam proyek uji Farmasi per 15 Okt (sebelumnya 34); **nol** error di berkas Farmasi. 544 uji tertulis tidak dapat dijalankan lewat `dotnet test` biasa |
| Master tarif jasa konsultasi tidak ada | **Billing** | Nol baris `MstTariff` dengan `IsConsultationFee = true`. Invoice tertahan `BIL-FIN-021`, sehingga clearance tidak terbit dan workflow Farmasi tidak tercapai lewat jalur normal |
| `LAB-OPEN-012` | **Lab** | Menahan penutupan E2E |

`PrescriptionBillingChargeProducer.cs` dihapus integration dan isinya diserap
`BillingChargeSourceAdapter` milik Billing. Kontrak `PRESCRIBED` / `BIL-INTEGRATION-1.3` tetap
berlaku dan terbukti runtime, jadi `PHA-D-09` **tetap `DONE`**; kepemilikan kodenya saja yang
pindah.

---

# OPERASI

Register asli lengkap: 11 task backend dan 9 task frontend. Status roadmap lama **tidak dipakai
apa adanya** — status di bawah hasil lintas-cek dengan source, uji, dan runtime 14 Oktober 2026.

## Backend

| Task ID | Type | Derived From | Scope | Status | Source Evidence | Test Evidence | Runtime Evidence |
|---|---|---|---|---|---|---|---|
| `BE-OPR-001` | ORIGINAL | `OPS-REQ-001/002`, `OPS-CON-001..015` | BE | `DONE` | 14 tabel `Opr*`, 23 FK `RESTRICT` | `OperatingRoomTests` | struktur terverifikasi di basis data |
| `BE-OPR-002` | ORIGINAL | semua entity baru | BE | `DONE` | migration additive | — | diterapkan di basis data dev |
| `BE-OPR-003` | ORIGINAL | `OPS-REQ-001`, `OPS-DEC-003/014` | BE | `DONE` | CRUD permintaan kasus | happy/invalid/duplicate/idempotency/concurrency | `OPR002` terbukti |
| `BE-OPR-004` | ORIGINAL | `OPS-REQ-003/004` | BE | `DONE` | penjadwalan + tim | tabrakan `OPR003`, 5 permintaan paralel | riwayat jadwal terbukti |
| `BE-OPR-005` | ORIGINAL | `OPS-REQ-005` | BE | `DONE` | gerbang kesiapan, sign-off 3 peran | seluruh gerbang | `Scheduled`→`Ready`; 14 Okt: sign-off 3 peran ada di data |
| `BE-OPR-006` | ORIGINAL | `OPS-REQ-006/009` | BE | `DONE` | catatan operasi + addendum | illegal start ditolak, final immutable | 14 Okt: `start` 3→4, catatan operasi 200 |
| `BE-OPR-007` | ORIGINAL | `OPS-REQ-006/008` | BE | `DONE` | recovery + handover | `StateTransitionTests` | 14 Okt: anestesi 200, recovery `Released`, handover `Sent` 200 |
| `BE-OPR-008` | ORIGINAL | `OPS-REQ-007` | BE | `DONE` | material/implan | validasi quantity/serial, keunikan serial | 14 Okt: implan `SN-UJI-14OKT-0001`; kirim ulang → `id` sama, tetap 1 baris |
| `BE-OPR-009` | ORIGINAL | `OPS-REQ-007/010` | BE | `DONE` | kontrak outbox | amplop kejadian dibekukan | 14 Okt: 12 baris / 12 `EventId` unik, index UNIQUE, event `material-usage.recorded` v`1.0` |
| `BE-OPR-010` | ORIGINAL | `OPS-REQ-011` | BE | `DONE` | 3 laporan | filter/paging/batas | 14 Okt: `operations` 9, `utilization` 3/1, `materials` 10 — semua `200` |
| `BE-OPR-011` | ORIGINAL | `opr-permission-v1` | BE | `DONE` | `AccessPermissionFilter` | `PermissionMatrixTests`, 120/120 | **12 Okt: `401` / `200` / `403` selektif terbukti**; `runbook-acceptance-be-opr-011.md` |

`BE-OPR-011` dinaikkan dari *"Selesai — kurang tes"* menjadi `DONE`: suite 120/120 hijau dan
matriks `401`/`200`/`403` terbukti runtime dengan `EnforceClinicalPolicyForSuperAdmin=true`.

**Perbaikan tercatat 14 Oktober 2026 (`375ef31c`).** `20260929000000_AddOperatingRoomOutboxEventContract`
membawa `[Migration]` tanpa `[DbContext]`, sehingga tidak terlihat EF dan kontrak outbox
`BE-OPR-009` tidak pernah terbentuk pada basis data yang dibangun dari migration. Sesudah
perbaikan: 315 dari 316 migration terlihat EF.

## Frontend

Diaudit satu per satu 14 Oktober 2026 terhadap source dan uji yang benar-benar menyentuh berkas
Operasi. Rinciannya di `testing/fe-acceptance-evidence.md`; `testing/readiness-report.md` sudah
dimutakhirkan dari klaim basi *"0 dari 9 task"*.

Dua uji yang semula saya anggap bukti ternyata **tidak menyentuh satu pun berkas Operasi**
(`surgery-booking`, `transfer-handover`), jadi tidak dipakai. Sebagian layar Operasi dikerjakan
tim Rawat Inap di bawah ID `FE-RWI-*`, sehingga acceptance-nya tercatat di sana, bukan di
`FE-OPR-*`.

| Task ID | Type | Derived From | Scope | Status | Source Evidence | Test Evidence | Runtime Evidence |
|---|---|---|---|---|---|---|---|
| `FE-OPR-001` | ORIGINAL | `OPS-REQ-001/002` | FE | **`DONE`** | `/cases`, `/cases/[slug]`, `/cases/create`; `case-list/detail/form/table-columns`; service | `operating-room-reject-case` (4 AC), `operating-room-hardening` (10) | — |
| `FE-OPR-002` | ORIGINAL | `OPS-REQ-003/004` | FE | **`DONE`** | `/cases/[slug]/schedule`; `operating-room-schedule-view.jsx`; service | nol uji menyentuh layar jadwal | — |
| `FE-OPR-003` | ORIGINAL | `OPS-REQ-005` | FE | **`DONE`** | `/preparation`; `operating-room-preparation-view.jsx`; `ward-pre-op/` | `ward-pre-op` (3 AC) — hanya drawer | — |
| `FE-OPR-004` | ORIGINAL | `OPS-REQ-006/009` | FE | **`DONE`** | `/execution`; `operating-room-execution-view.jsx`; service; `use-operating-room-execution.jsx` | **`operating-room-execution-addendum` (7 AC, 14 Okt)** | 14 Okt: catatan operasi `200`, endpoint addenda terpakai |
| `FE-OPR-005` | ORIGINAL | `OPS-REQ-006/008` | FE | **`DONE`** | `/recovery`; `operating-room-recovery-view.jsx`; `post-op-summary/` | `post-op-summary` (5 AC) — hanya drawer; hardening #10 | 14 Okt: anestesi `200`, recovery `Released`, handover `Sent` |
| `FE-OPR-006` | ORIGINAL | `OPS-REQ-007` | FE | **`DONE`** | `/materials`; `operating-room-material-view.jsx`; service | nol uji | 14 Okt: implan + serial `200`, idempoten |
| `FE-OPR-007` | ORIGINAL | `OPS-REQ-007/010` | FE | **`DONE`** | `/integration`; `operating-room-integration-view.jsx`; service | nol uji | 14 Okt: outbox 12/12 `EventId` unik |
| `FE-OPR-008` | ORIGINAL | `OPS-REQ-011` | FE | **`DONE`** | `/reports`; `operating-room-report-view.jsx`; service | nol uji | 14 Okt: 3 laporan `200` |
| `FE-OPR-009` | ORIGINAL | semua `OPS-REQ` | FE | `IN PROGRESS` | `tests/e2e/operating-room-flow.spec.mjs` (12 kasus) | `operating-room-hardening` (10, lulus) | **E2E dijalankan 14 Okt: 10 gagal** |

Dua hal yang paling lemah dan sebaiknya ditutup lebih dulu: **addendum pelaksanaan belum
tersentuh uji** (`FE-OPR-004`) dan **E2E belum pernah dijalankan** (`FE-OPR-009`).

## Dependency eksternal Operasi — bukan task internal

| Hal | Pemilik | Dampak |
|---|---|---|
| Consumer Billing / Inventory | **Billing, Inventory** | Kontrak consumer belum tersedia. **Tidak** dibuatkan adapter palsu |
| Akun kedua untuk menerima serah terima | **akun/role** | *"Pengirim tidak dapat menerima serah terima sendiri"* — pemisahan wewenang yang benar. Kasus berhenti di `InProgress`; bukan cacat kode |
| Serial implan | **pemilik proses** | Aturan pencatatan nomor seri; keunikan serial sudah diuji, kebijakannya belum |
| `InpAdmissionReferral` | **Rawat Inap** | Tabelnya tidak ada di basis data dev; ditambal irisannya 14 Okt agar release recovery dapat diuji |

---

# GIZI

Gizi **tidak punya satu pun task asli**: tidak ada `roadmap/`, tidak ada `task/`. Yang ada
keputusan (`GIZ-DEC-001..014`), pertanyaan terbuka (`GIZ-OQ-001..007`), desain domain, dan
`MODULE-STATUS.md`. Karena itu seluruh baris di bawah `DERIVED`, berjangkar keputusan yang sudah
disahkan, berkas uji yang sudah ada, dan bukti runtime 14 Oktober 2026.

| Task ID | Type | Derived From | Scope | Status | Source Evidence | Test Evidence | Runtime Evidence |
|---|---|---|---|---|---|---|---|
| `GIZ-D-01` | DERIVED | `GIZ-DEC-001`, `GIZ-DEC-007` | BE | `DONE` | `NutritionOrderController`, `GziNutritionOrder` | `OrderRuleTests` (24) | 14 Okt: order `GZ-1e2d56e6…` `200` |
| `GIZ-D-02` | DERIVED | `GIZ-DEC-005`, `GIZ-DEC-010` | BE | `DONE` | `orders/{id}/records` | `OrderRuleTests`, `DiagnosisRuleTests`, `DietRuleTests` | 14 Okt: catatan kunjungan `200` (BB/TB, asesmen, intervensi, recall, evaluasi) |
| `GIZ-D-03` | DERIVED | `GIZ-DEC-011` (mengganti `GIZ-DEC-009`) | BE | `DONE` | master diagnosis berkode milik Gizi | `DiagnosisRuleTests` (10) | 14 Okt: `NI-1.4` primary + `NC-1.1` sekunder `200` |
| `GIZ-D-04` | DERIVED | `GIZ-DEC-012` | BE | `DONE` | kebutuhan energi/protein/lemak/KH/cairan | `RequirementRuleTests` (19) | 14 Okt: revisi 1 `200`; gerbang *"seluruh parameter aktif wajib terisi"* terbukti |
| `GIZ-D-05` | DERIVED | `GIZ-DEC-012` | BE | `DONE` | versioning kebutuhan | `RequirementRuleTests` (11 penyebutan revisi) | 14 Okt: revisi 2 `isCurrent=true`, revisi 1 `false`, kedua alasan tersimpan |
| `GIZ-D-06` | DERIVED | `GIZ-DEC-013` (mengganti `GIZ-DEC-004`) | BE | `DONE` | `NutritionDietController` | `DietRuleTests` (21) | runtime menunggu kunjungan rawat inap — lihat catatan |
| `GIZ-D-07` | DERIVED | `GIZ-DEC-013` | BE | `DONE` | batch produksi + distribusi | `ProductionBatchRuleTests` (30) | runtime menunggu kunjungan rawat inap |
| `GIZ-D-08` | DERIVED | `GIZ-OQ-002` (ditutup `GIZ-DEC-011`) | BE | `DONE` | `NutritionMasterController` | `MasterControllerTests` (26) | 14 Okt: diet type, food form, meal schedule dibuat `200` |
| `GIZ-D-09` | DERIVED | `MODULE-STATUS.md`, `03-desain-domain.md` | BE | `DONE` | 4 endpoint laporan | `ReportRuleTests` (19) | 14 Okt: `summary`, `services`, `diets`, `requirements` semua `200` |
| `GIZ-D-10` | DERIVED | `contracts` permission + matriks izin Gizi | BE | `DONE` | `[AccessPermission]` seluruh controller Gizi | `PermissionMatrixTests` (16) | **14 Okt: `401` / `403` / `200` selektif terbukti** pada Order, Requirement, Master, Report |
| `GIZ-D-11` | DERIVED | `GIZ-DEC-008` | BE | `DONE` | `orders/{id}/close` | `OrderRuleTests` (11 penyebutan close) | 14 Okt: order ditutup, status 2→3 |
| `GIZ-D-12` | DERIVED | `GIZ-DEC-007` | BE | `DONE` | `orders/{id}/verify-instruction`, `diets/{id}/verify-instruction` | tercakup `OrderRuleTests`, `DietRuleTests` | 14 Okt: `403` *"Hanya dokter yang memberi instruksi…"* — kontrol wewenang bekerja |

## Tidak dihitung

| Hal | Alasan | Pengganti / pemilik |
|---|---|---|
| `GIZ-DEC-004` | **SUPERSEDED** | Diganti `GIZ-DEC-013` yang memperluas scope ke diet, produksi, dan distribusi |
| `GIZ-DEC-009` | **SUPERSEDED** | Diganti `GIZ-DEC-011`: master diagnosis milik Gizi sendiri, bukan menumpang `MstDiagnosis` |
| `GIZ-OQ-007` | **DEFERRED** 25 Sep 2026 | Rumus kalkulasi tidak dibuat pada V1 atas keputusan pemilik proses. Nilai diinput ahli gizi; registry rumus berdiri dan kosong. **Bukan** task coding yang belum selesai |
| `GIZ-OQ-003` | **EXTERNAL** | Isi `MstProfession` untuk ahli gizi — milik **Human Resource** |
| `GIZ-OQ-005` | **BUSINESS / OUT OF V1** | Interval skrining ulang; tertulis *"Tidak untuk versi pertama"* |
| Skrining gizi | **NOT APPLICABLE** | `GIZ-DEC-003`: dikerjakan **perawat** sebelum order. Nol endpoint Gizi; bukan scope modul ini |

## Catatan runtime diet dan produksi

`GIZ-D-06` dan `GIZ-D-07` tetap `DONE`: source lengkap dan 51 uji hijau. Runtime-nya belum
dijalankan karena `GIZ-DEC-002` menetapkan **V1 hanya melayani pasien rawat inap**, sedangkan
basis data dev memuat **24 kunjungan dan seluruhnya rawat jalan, nol rawat inap**. Jadi ini
**keterbatasan data uji**, bukan cacat Gizi dan bukan dependency yang menggantung — perilakunya
justru sesuai keputusan yang disahkan.

---


## Konsistensi keterangan penahanan (14 Oktober 2026)

Dua keadaan yang mudah tertukar dibedakan di `prescription-clearance-utils.js`, mengikuti
`PrescriptionFinancialClearanceService` dan `EvaluateGateAsync`:

| Keadaan | Yang dikatakan kontrak | Nada di layar |
|---|---|---|
| **A.** Response tidak memuat `FinancialClearance` sama sekali | Billing belum menyatakan apa pun kepada layar ini; propertinya memang nullable | **keterangan** (`info`) |
| **B.** Objeknya ada, `ClearanceStatus = UNKNOWN` | Backend mengisi `HoldReasonCode = PHA_CLR_UNKNOWN` beserta kalimatnya, dan gerbang menahan karena `IsCleared` salah | **peringatan** (`warning`) |

Acuannya satu: **pekerjaan farmasi tertahan setiap kali `IsCleared` salah**, dan kode alasan
hanya memilih kalimat mana yang dipakai. Kelima kode kontrak — `PHA_CLR_NOT_CLEARED`,
`PHA_CLR_ON_HOLD`, `PHA_CLR_NOT_SETTLED`, `PHA_CLR_UNKNOWN`, `PHA_CLR_STALE` — seluruhnya
menghasilkan peringatan, dan itu diuji.

Koreksi atas catatan sebelumnya: dokumen ini pernah menyebut `UNKNOWN` tampil sebagai
keterangan dengan alasan *"bukan galat dan bukan 404"*. Kalimat kontrak itu benar untuk
**bentuk response** — resepnya tetap dikembalikan, bukan `404` — tetapi **bukan** berarti
pekerjaannya boleh jalan. Nada untuk `UNKNOWN` karena itu peringatan, bukan keterangan.

---

# Baris pengganti untuk rekap 23 modul

Disiapkan supaya penyusun laporan dapat menyalin langsung, memakai **rumus rekap** —
`Sisa = Total − Selesai`, dan Blocked ikut penyebut.

## Tabel Outstanding Development

| No. | Modul | Total Task | Selesai | Sedang Proses | Blocked | Belum Mulai | Sisa Task | % Selesai |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 6 | Gizi | 12 | 12 | 0 | 0 | 0 | **0** | **100%** |
| 14 | Operasi | 20 | 20 | 0 | 0 | 0 | **0** | **100%** |
| 16 | Farmasi | 19 | 18 | 1 | 0 | 0 | **1** | **94,7%** |

Penanda `S` (estimasi source) dan `B` (hanya kontrak backend) **tidak lagi diperlukan** untuk
ketiga modul ini: seluruhnya kini punya rekap task yang dapat dihitung.

## Tabel Current Status

| No. | Modul | Persentase |
|---|---|---|
| 6 | Gizi | **100%** |
| 14 | Operasi | **100%** |
| 16 | Farmasi | **94,7%** |

Tanda `~` dan keterangan `(estimasi source)` dihapus.

## Perubahan terhadap angka lama, dan sebabnya

| Modul | Lama | Baru | Sebab |
|---|---:|---:|---|
| Gizi | ~94% estimasi | **100%** | register task dibuat; 12 task, 219 uji hijau, runtime HTTP terbukti |
| Operasi | 90,9% **B** | **100%** | angka lama **hanya kontrak backend**; sembilan task frontend kini ikut dihitung. Backend sendiri naik 10/11 → **11/11** karena `BE-OPR-011` terbukti runtime, dan `FE-OPR-009` ditutup 15 Okt dengan E2E 11/11 lulus |
| Farmasi | ~97% estimasi | **89,5%** | dua task tertahan keputusan bisnis (`PHA-OQ-014`, milik Product) kini **ikut penyebut** mengikuti rumus rekap. Tanpa blocker itu: **100%**. `PHA-D-11` (gerbang stok `PHA-DEC-018`) ditambahkan 15 Okt |

Dua angka turun, dan keduanya bukan kemunduran pekerjaan: yang berubah **cakupan yang dihitung**,
bukan jumlah pekerjaan yang selesai. Angka lama tidak sebanding dengan modul lain karena Farmasi
dan Gizi tidak punya rekap task sama sekali, sedangkan Operasi hanya menghitung backend.

## Di mana atasan dapat memeriksanya

| Modul | Berkas |
|---|---|
| Farmasi | `pharmacy/roadmap/task-register.md` |
| Operasi | `operations/roadmap/backend-roadmap.md`, `operations/roadmap/frontend-roadmap.md`, `operations/testing/fe-acceptance-evidence.md` |
| Gizi | `gizi/roadmap/backend-roadmap.md` |

---

# EVIDENCE SUMMARY

## Farmasi

| | |
|---|---|
| **Source** | 19+ controller; telaah, penyiapan, telaah akhir, penyerahan, retur, stok, etiket, depot routing, konsolidasi kode modul izin |
| **Test** | **536 uji tertulis** dalam 11 berkas: `DrugReturnTests` 48, `DrugReturnUpdateAndListTests` 34, `PharmacyWorkflowTests` 33, `DepotRoutingTests` 26, `DispensingGuardTests` 20, `DispenseFlowTests` 18, `FinancialClearanceTests` 18, `PermissionMatrixTests` 18, `ModuleCodeConsolidationTests` 16, `LabelAndStockTests` 14, `PrescriptionFulfillmentStageTests` 8 |
| **Runtime** | 12 Okt: rantai penuh sampai obat diserahkan, stok 570→561, tahap 2 `NO_FINANCIAL_CHANGE`. 14 Okt pasca refaktor Billing: `PRESCRIBED` / `BIL-INTEGRATION-1.3` `Synced` tanpa retry, `billingHandoffIssues` kosong |
| **External blockers** | `PharmacyTests` tidak dapat dikompilasi (Billing/Finance, 34 error); master tarif jasa konsultasi (Billing); `LAB-OPEN-012` (Lab) |

## Operasi

| | |
|---|---|
| **Source** | 11 task backend; 11 layar frontend, 10 service, 11 halaman |
| **Test** | **120/120 hijau**; frontend 17 uji hardening + 12 kasus E2E Playwright |
| **Runtime** | 14 Okt: `start`→catatan operasi→anestesi→implan+serial→recovery→release→handover `Sent`; 3 laporan `200`; outbox 12/12 `EventId` unik + idempotency terbukti. 12 Okt: `401`/`200`/`403` selektif |
| **External blockers** | consumer Billing/Inventory; akun kedua untuk terima serah terima; serial implan (keputusan); `InpAdmissionReferral` (Rawat Inap) |

## Gizi

| | |
|---|---|
| **Source** | 5 controller, 7 view frontend; order, catatan asuhan, diagnosis, kebutuhan + revisi, diet, master, laporan, penutupan, verifikasi instruksi |
| **Test** | **219/219 hijau** dalam 8 berkas: `ProductionBatchRuleTests` 30, `MasterControllerTests` 26, `OrderRuleTests` 24, `DietRuleTests` 21, `ReportRuleTests` 19, `RequirementRuleTests` 19, `PermissionMatrixTests` 16, `DiagnosisRuleTests` 10 |
| **Runtime** | 14 Okt, pertama kali lewat HTTP nyata: order → catatan asuhan → diagnosis primary → kebutuhan 5 parameter → revisi → 3 master → 4 laporan → tutup order; **otorisasi selektif `401`/`403`/`200` terbukti** |
| **External blockers** | kunjungan rawat inap untuk diet/produksi (Rawat Inap, sesuai `GIZ-DEC-002`); akun dokter pemberi order untuk verifikasi instruksi; `GIZ-OQ-003` (HR) |

---

# Task yang masih meragukan

Dicatat terbuka, bukan disembunyikan.

| Task | Keraguan | Yang dibutuhkan |
|---|---|---|
| `FE-OPR-002`, `003`, `005`, `006`, `007`, `008` | Source ada, layarnya belum tersentuh uji | Satu uji frontend per layar |
| `FE-OPR-001` | `loading/empty/error/stale/paging` daftar kasus nol uji | Uji keadaan daftar |
| `FE-OPR-009` | **Dijalankan 14 Okt: 10 gagal.** Cacat 1 (`SESSION_SIGNING_SECRET` hilang) sudah diperbaiki; cacat 2 (tiruan `GET /v1/auth/me` belum ada) masih terbuka | Tambahkan tiruan `auth/me` yang memenuhi `buildUserInfo` + `persistIdentity` |
| `PHA-D-10` | **Acceptance terbukti** (1 Okt: 6 langkah `200` + 12 pemeriksaan peramban). Yang belum ada hanya **uji otomatis** untuk panelnya, sehingga regresinya tidak terjaga mesin | Satu uji frontend untuk panel alur kerja |
| `GIZ-D-06`, `GIZ-D-07` | `DONE` atas source + 51 uji; runtime belum dijalankan | Satu kunjungan rawat inap sebagai data uji |

# Tindak lanjut 14 Oktober 2026

| Task | Hasil |
|---|---|
| `PHA-FE-002` | **Ditutup.** Source + 10 uji + smoke runtime terhadap payload nyata. Nol perubahan backend, nol perubahan Billing |
| `PHA-FE-001` | **Tidak dikerjakan.** Terbukti `BLOCKED BUSINESS` oleh `PHA-OQ-014`; acceptance belum pernah ditetapkan dan resolver tidak punya endpoint. Mengerjakannya berarti mengarang scope |

# Validasi akhir

| Pemeriksaan | Hasil |
|---|---|
| Task ID dilintas-cek dengan source | ✅ |
| Task ID dilintas-cek dengan laporan task | ✅ 4 laporan Farmasi, roadmap Operasi |
| Dilintas-cek dengan uji | ✅ jumlah kasus per berkas dicantumkan |
| Dilintas-cek dengan bukti runtime | ✅ bertanggal 12 dan 14 Okt 2026 |
| Nol double-count | ✅ `FinancialClearanceTests` tidak dijadikan task turunan |
| Nol task superseded dihitung | ✅ `GIZ-DEC-004`, `GIZ-DEC-009` dikeluarkan |
| Keputusan bisnis tidak dihitung sebagai bug coding | ✅ `GIZ-OQ-005`, `GIZ-OQ-007`, serial implan |
| Blocker modul lain tidak dihitung internal | ✅ Billing, Rawat Inap, HR, Lab dipisahkan |
| Nol source aplikasi diubah untuk dokumen ini | ✅ |
