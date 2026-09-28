# Roadmap Backend — Rawat Jalan ke Invoice Canonical (V2)

## Metadata

```yaml
blueprint_id: RJ-BIL-BP-001
scope_name: "Rawat Jalan → Invoice Canonical (V2)"
scope_prefix: RJ-E2E
task_prefix: BE-RJE
module_slug: rawat-jalan
blueprint_revision: 27
roadmap_revision: 1
status: OWNER_APPROVED
approval_gate: "RJ-E2E-DEC-017 — Sukma Giri, 2026-09-28"
design_approval: "RJ-E2E-DEC-015 — Sukma Giri, 2026-09-28"
contract_versions:
  - "RJ-E2E-CONTRACT-001@1.0.0 (approved)"
  - "RJ-E2E-CONTRACT-001@1.0.1 (draft) — hanya BE-RJE-013"
decision_revision: 18
input_hashes:
  prd_rawat_jalan: "39C60B262E6B38B4B6DF6F5B33AA4C013E8E0B6BC4CE715E721F7A5BEFBF59D2"
  capability_map_v2: "9ED01454F34A665787ED805A872C4DD77FE421CFEE59EC16AA2F6BA85B7F6615"
  prd_to_mvp: "lihat blueprint-manifest.md revisi 27"
backend_sha: "063d38bc306bb6b46bdf088513fa6cdcc80399d8 (sukmagp)"
frontend_sha: "83b8b72744d4afaaedb3d2af9dd0b83fb272f9d6 (sukmagpV2)"
owners:
  - "Product/Domain: Sukma Giri"
  - "Billing/Revenue Cycle: Sukma Giri (pemilik blueprint)"
  - "Pharmacy (titik sentuh BE-RJE-008): sign-off formal OPEN"
  - "Registration (titik sentuh BE-RJE-006): OPEN"
implementation_authority: "GRANTED — BE-RJE-001 (RJ-E2E-DEC-017), BE-RJE-002 (RJ-E2E-DEC-018); task lain NOT_GRANTED"
builder_execution: "EXECUTED — BE-RJE-001, BE-RJE-002 (2026-09-28); task lain NOT_AUTHORIZED"
verification_pattern: "Pola Bank Darah — tanpa project/folder test backend"
frontend_roadmap: roadmap/e2e-frontend-roadmap.md
traceability: roadmap/e2e-requirement-traceability.md
```

> **Approval desain bukan izin menulis code.** `RJ-E2E-DEC-015` menyetujui desain dan kontrak
> `1.0.0`. Setiap task tetap memerlukan handoff, wewenang tulis, dan preflight tersendiri.
>
> **Berkas ini terpisah dari `backend-roadmap.md`** (roadmap `RJ-BIL` revisi `1`, `HISTORICAL
> SNAPSHOT`), sama seperti `doctor-consultation-roadmap.md` untuk scope Dokter. Ketiganya tidak
> boleh dijumlahkan progress-nya.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD sudah terbukti |
| 🟡 | Source sudah ada tetapi kriteria belum terbukti penuh |
| ⛔ | Prasyarat berupa keputusan atau gerbang belum terpenuhi |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
BE-RJE-001 ✅ ─┬─> BE-RJE-002 ✅ ─> BE-RJE-003 ─┬─> BE-RJE-005 ─┬─> BE-RJE-008
               │                                │               │
               │                                │               └─> BE-RJE-009
               │                                │
               │                                ├─> BE-RJE-007
               │                                │
               │                                ├─> BE-RJE-014
               │                                │
               │                                └─> BE-RJE-010 ─┐
               │                                                │
               └─> BE-RJE-011 ─────────────────────────────────┴─> BE-RJE-012

BE-RJE-004

BE-RJE-006

{RJ-E2E-CONTRACT-001@1.0.1 ✅} ─> BE-RJE-013
```

Dependency ditulis sebagai **prasyarat langsung** saja. Contoh: `BE-RJE-003` juga membutuhkan
kolom dari `BE-RJE-001`, tetapi itu sudah dijamin lewat `BE-RJE-002`, sehingga tidak digambar
dua kali. Jumlah pasangan prasyarat → task: **12**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RJE-001` ✅, `BE-RJE-004`, `BE-RJE-006` — boleh paralel |
| 2 | `BE-RJE-001` | `BE-RJE-002` ✅, `BE-RJE-011` — boleh paralel |
| 3 | `BE-RJE-002` | `BE-RJE-003` |
| 4 | `BE-RJE-003` | `BE-RJE-005`, `BE-RJE-007`, `BE-RJE-010`, `BE-RJE-014` — boleh paralel |
| 5 | `BE-RJE-005` / `BE-RJE-010` + `BE-RJE-011` | `BE-RJE-008`, `BE-RJE-009`, `BE-RJE-012` — boleh paralel |
| — | ⛔ menunggu approval `RJ-E2E-CONTRACT-001@1.0.1` | `BE-RJE-013` |

**Cara membaca:** hari ini tiga task boleh dimulai bersamaan — fondasi data (`001`), pengaman
`from-source` (`004`), dan status kunjungan tanpa dokter (`006`). `BE-RJE-003` adalah leher botol:
enam task menunggunya. `BE-RJE-013` baru boleh dimulai setelah kontrak `1.0.1` disetujui.

## Pemetaan ke gelombang MVP (`04-prd-to-mvp.md` bagian 20)

| Gelombang MVP | Epic | Task backend |
| --- | --- | --- |
| `MVP-0` | `EPIC RJE-01` | `BE-RJE-001`, `BE-RJE-002` |
| `MVP-1` | `EPIC RJE-02`, `EPIC RJE-08` | `BE-RJE-003`, `BE-RJE-004`, `BE-RJE-005`, `BE-RJE-006` |
| `MVP-2` | `EPIC RJE-03`, `EPIC RJE-04` | `BE-RJE-007`, `BE-RJE-008` |
| `MVP-3` | `EPIC RJE-05`, `EPIC RJE-06` | `BE-RJE-009`, `BE-RJE-010`, `BE-RJE-011`, `BE-RJE-012` |
| `MVP-4` | `EPIC RJE-07` | `BE-RJE-013`, `BE-RJE-014` |

Gelombang eksekusi di atas boleh lebih cepat dari gelombang MVP (contoh: `BE-RJE-011` bisa jalan di
gelombang 2), karena urutan MVP adalah urutan rilis, sedangkan gelombang eksekusi adalah urutan
teknis paling awal yang aman.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RJE-001` ✅ | Kolom sinkron, kolom rekonsiliasi, master kebijakan, dan backfill baris lama tersedia | `FR-RJE-001`, `002`, `004`; `RJ-E2E-DEC-009`, `014` | `1.0.0` | `BilChargeLine`, `CliClinicalMilestoneFact`, pola master Billing | Model, enum, configuration, migration tulis tangan, seed | — | Lihat kartu | Pola Bank Darah + `UAT-22` | Snapshot EF meleset (`RJ-BIL-DEC-018`) / Billing | Kartu |
| `BE-RJE-002` ✅ | Kontrak adapter `1.3`, konteks `Consultation`, dan penanda pembatalan sampai ke folio | `FR-RJE-003`; `RJ-E2E-DEC-001`, `005` | `1.0.0` | `ContractBillingChargeSourceAdapter`, `BillingSourceContract` | Adapter, konstanta, DTO folio, producer | `BE-RJE-001` | Kartu | Pola Bank Darah + `UAT-23` | Pemanggil lama kontrak `1.2` / Billing | Kartu |
| `BE-RJE-003` | Tindakan, Lab, Radiologi otomatis masuk invoice berharga katalog | `FR-RJE-010`..`013`; `RJ-E2E-DEC-003`, `006` | `1.0.0` | `UpsertChargeAsync`, `BilChargeReceipt`, `MstTariff` | Jembatan, resolver (3 domain), pemicu dari folio | `BE-RJE-002` | Kartu | Pola Bank Darah + `UAT-01`, `02`, `24` | Leher botol 6 task / Billing | Kartu |
| `BE-RJE-004` | `from-source` menolak domain klinis Rawat Jalan | `FR-RJE-014`; `RJ-E2E-DEC-006` | `1.0.0` | `BillingInvoicesController.FromSource` | Satu pemeriksaan + `422` | — | Kartu | Pola Bank Darah + `UAT-03` | Konsumen `ADHOC` kasir / Billing | Kartu |
| `BE-RJE-005` | Perubahan setelah invoice final menjadi adjustment | `FR-RJE-015`; `RJ-BIL-DEC-004` | `1.0.0` | `CreateAdjustmentAsync` | Cabang pasca-final di jembatan | `BE-RJE-003` | Kartu | Pola Bank Darah + `UAT-04` | Invoice `CLOSED` mungkin menolak adjustment / Billing | Kartu |
| `BE-RJE-006` | Kunjungan tanpa dokter berhenti di `Billing` | `FR-RJE-070`; `RJ-E2E-DEC-007`, `013` | `1.0.0` | `NurseStationQueueController` | Satu cabang status | — | Kartu | Pola Bank Darah + `UAT-20`, `21` | Laporan/layar yang mengandalkan `Completed` / Registration | Kartu |
| `BE-RJE-007` | Jasa konsultasi tertagih otomatis | `FR-RJE-020`, `021`; `RJ-E2E-DEC-001`, `012` | `1.0.0` | Finalisasi canonical, `MstDoctorServiceRule`, `MstTariff` | Fakta konsultasi + resolver `CONSULTATION` | `BE-RJE-003` | Kartu | Pola Bank Darah + `UAT-05`, `06` | Tarif konsultasi belum diisi per klinik / Billing | Kartu |
| `BE-RJE-008` | Obat dua tahap; deadlock farmasi hilang | `FR-RJE-030`..`032`; `RJ-E2E-DEC-005` | `1.0.0` | Clearance farmasi, `PrescriptionDispensingService` | Resolver `PHARMACY`, fakta tahap 2 | `BE-RJE-005` | Kartu | Pola Bank Darah + `UAT-07`, `08`, `09` | Titik sentuh Pharmacy / Pharmacy + Billing | Kartu |
| `BE-RJE-009` | Pembatalan tanpa menghapus riwayat, tanpa pembatalan palsu | `FR-RJE-040`..`042`; `RJ-E2E-DEC-010` | `1.0.0` | `VoidItemAsync`, `CreateAdjustmentAsync`, *CASE A* producer | Cabang pembatalan di jembatan | `BE-RJE-005` | Kartu | Pola Bank Darah + `UAT-10`, `11`, `12` | — / Billing | Kartu |
| `BE-RJE-010` | Sinkron invoice yang gagal dicoba ulang otomatis | `FR-RJE-051`; `RJ-E2E-DEC-009` | `1.0.0` | Pola `InpatientIntegrationOutboxWorker` | Pekerja latar + jadwal dari master | `BE-RJE-003` | Kartu | Pola Bank Darah + `UAT-13`, `14` | Beban database / Billing | Kartu |
| `BE-RJE-011` | Fakta `Pending`/`OutcomeUnknown` dikirim ulang otomatis | `FR-RJE-050`; `RJ-E2E-DEC-009`; `AC-RJ-014` | `1.0.0` | `ClinicalMilestoneFactProducer` | `RedispatchAsync` + pekerja latar | `BE-RJE-001` | Kartu | Pola Bank Darah | Irisan `RJ-DOC-BE-005` / Clinical Integration | Kartu |
| `BE-RJE-012` | Petugas Billing dapat menangani antrean dan mengatur kebijakan | `FR-RJE-052`, `054` | `1.0.0` | `[AccessController]`, `PagedResult` | 2 controller, 2 service, DTO | `BE-RJE-010`, `BE-RJE-011` | Kartu | Pola Bank Darah + `UAT-15`, `16` | Balapan petugas vs pekerja / Billing | Kartu |
| `BE-RJE-013` | `finish-consultation` membawa `BillingHandoffIssues` | `FR-RJE-062`; `RJ-E2E-FE-004` | `1.0.1` (draft) | `ConsultationFinalizationResponse` | Satu field aditif | `{RJ-E2E-CONTRACT-001@1.0.1}` | Kartu | Pola Bank Darah + `UAT-19` | — / Registration + Clinical | Kartu |
| `BE-RJE-014` | Dokter dapat membaca ringkasan tagihan satu kunjungan | `FR-RJE-060`; `RJ-E2E-DEC-008` | `1.0.0` | `PreviewCalculationAsync`, adapter coverage | Controller, service, DTO | `BE-RJE-003` | Kartu | Pola Bank Darah + `UAT-17`, `18`, `24` | Data sensitif / Security | Kartu |

## Aturan yang berlaku untuk setiap task

- **Governance saat eksekusi.** QBE preflight dan kesesuaian engineering diselesaikan pada waktu
  eksekusi dari `AGENTS.md` backend, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
  `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, dan `rules/backend/`. Roadmap ini tidak
  menggantikan preflight itu.
- **Verifikasi pola Bank Darah** (`02-backend-architecture.md` bagian `V2.13`): `dotnet build … -o
  <scratchpad>`, `dotnet ef migrations has-pending-model-changes --no-build`,
  `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`, lalu validasi runtime R0..Rn lewat HTTP
  terhadap **`QuilvianNewDevSukma`** dengan data samaran, dan keadaan database sesudah run dicatat.
  `dotnet test` ditulis `NOT RUN — tidak ada project test`. **Folder `Tests/` dilarang dibuat.**
- **Database.** Migration hanya diterapkan ke `QuilvianNewDevSukma`. Database lain memerlukan
  wewenang tersendiri.
- **Laporan** ditulis ke `task/report/backend/<TASK-ID>.md`, dan tanda status pada grafik, tabel,
  serta kartu diperbarui bersama.

## Kartu task

### ✅ `BE-RJE-001` — Fondasi data sinkron dan rekonsiliasi

| Field | Isi |
| --- | --- |
| **Status** | ✅ `COMPLETE` 2026-09-28 — 5/5 AC terbukti. Build penuh `0 Error(s)`/`230 Warning(s)` (= baseline), `has-pending-model-changes` bersih, QBE strict `PASS` (11 berkas, 0 pelanggaran), `dotnet test` `NOT RUN` (tanpa project test), migration diterapkan ke `QuilvianNewDevSukma`, runtime DB R0–R10 `PASS`. Dikecualikan: backfill dibuktikan dengan data sintetis (tidak ada data lama RJ); `Down()` dibuktikan di DB dev pemilik, bukan salinan. [Laporan](../task/report/backend/BE-RJE-001.md) |
| **Outcome** | Tabel siap menampung status sinkron invoice dan rekonsiliasi; baris lama tidak hilang dan tidak ditagih otomatis |
| **Cakupan** | `BillingInvoiceSyncStatus` (enum baru); 16 kolom + 3 FK + 2 index di **`BilProcessingEffect`** (kontrak `1.0.2`, `RJ-E2E-DEC-016`; semula `BilChargeLine`); 5 kolom + 1 index di `CliClinicalMilestoneFact`; `MstBillingSyncPolicy` beserta configuration dan seed 2 baris; migration `AddClinicalChargeInvoiceSync` **ditulis tangan**; backfill `LEGACY_PRE_BRIDGE`. Rincian: `data/data-dictionary.md` bagian 2 dan 4; `02` bagian `V2.9`–`V2.11` |
| **Di luar cakupan** | Logika jembatan, pekerja, endpoint |
| **Acceptance criteria** | 1. Migration diterapkan ke `QuilvianNewDevSukma` tanpa error dan `has-pending-model-changes` bersih. 2. Seluruh baris `BilProcessingEffect` lama bernilai `0` atau `4`, tanpa `NULL`. 3. Efek lama Rawat Jalan dari lima konteks yang menghasilkan folio bernilai `4` dengan kode `LEGACY_PRE_BRIDGE`; nol item invoice baru. 4. `MstBillingSyncPolicy` berisi `FACT_DISPATCH` dan `INVOICE_SYNC` (5/60/3600, aktif); `PolicyCode` unik. 5. `Down()` membalik seluruh perubahan pada salinan database uji |
| **Verifikasi** | Build, EF, QBE; query hitung sebelum/sesudah backfill; `UAT-22` |
| **Risiko** | `dotnet ef migrations add` akan membawa ratusan operasi modul lain (`RJ-BIL-DEC-018`) — migration wajib tulis tangan dan snapshot hanya diperbarui untuk entitas ini |
| **DoD** | Kelima AC terbukti; laporan `task/report/backend/BE-RJE-001.md` |

### ✅ `BE-RJE-002` — Kontrak adapter `BIL-INTEGRATION-1.3`

| Field | Isi |
| --- | --- |
| **Status** | ✅ `COMPLETE` 2026-09-28 — 5/5 AC terbukti. Build penuh `0 Error(s)`/`230 Warning(s)` (= baseline), `has-pending-model-changes` bersih, QBE strict `PASS` (5 berkas), `dotnet test` `NOT RUN` (tanpa project test), runtime R0–R13 16/16 `PASS` terhadap `QuilvianNewDevSukma`. AC 4 dibuktikan lewat pembatalan tindakan sungguhan (tidak ada data Lab). [Laporan](../task/report/backend/BE-RJE-002.md) |
| **Outcome** | Billing mengenali `PHARMACY`/`PRESCRIBED` dan `CONSULTATION`/`COMPLETED`; folio tahu mana baris pembatalan |
| **Cakupan** | `ContractBillingChargeSourceAdapter`: konstanta `1.3`, kebijakan `PHARMACY` (`PRESCRIBED`, `DISPENSED` / void dari `PRESCRIBED` / `CANCELLED`) dan `CONSULTATION`; aturan keras `:87-88` diganti aturan per versi kontrak. `BillingSourceContract`: `Consultation`/`ConsultationCharge`. `RecognizeBillingMilestoneRequest.IsClinicalCancellation`; producer mengisinya dari `MilestoneKind`; `BillingFolioService` menyimpannya ke kolom baru |
| **Acceptance criteria** | 1. `PHARMACY`/`PRESCRIBED` dengan kontrak `1.2` tetap ditolak "Jumlah obat yang diserahkan belum final." (`UAT-23`). 2. Dengan `1.3` diterima. 3. `CONSULTATION`/`COMPLETED` dengan `1.3` diterima; dengan status lain ditolak. 4. Fakta pembatalan Lab menghasilkan baris folio `IsClinicalCancellation = true`. 5. Perilaku domain lain tidak berubah |
| **Verifikasi** | Pola Bank Darah; skenario runtime lewat `from-source` untuk domain non-Rawat-Jalan dan lewat pembatalan Lab |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-003` — Jembatan folio → invoice untuk tindakan, Lab, Radiologi

| Field | Isi |
| --- | --- |
| **Outcome** | Tn. A (samaran) selesai dikerjakan Nebulizer → item `PROCEDURE` Rp75.000 muncul di invoice kunjungannya tanpa input kasir |
| **Cakupan** | `BillingClinicalChargeBridgeService` (kelayakan `V2.7.1`, pemetaan `V2.7.2`, kunci deterministik, panggilan `UpsertChargeAsync`, penulisan kolom sinkron dengan cek `Version`); `BillingSourceTariffResolver` untuk `PROCEDURE`, `LABORATORY`, `RADIOLOGY`; pemanggilan jembatan dari `BillingFolioService` setelah commit; registrasi DI |
| **Di luar cakupan** | Invoice non-`OPEN` (`BE-RJE-005`), pembatalan (`BE-RJE-009`), pekerja (`BE-RJE-010`), konsultasi dan resep |
| **Acceptance criteria** | 1. Tindakan dieksekusi → satu item `PROCEDURE` dengan harga `MstTariff` (bukan snapshot). 2. Lab diterima → item `LABORATORY` per pemeriksaan. 3. Study diterima → item `RADIOLOGY`; pengulangan `InternalHospitalError` → `NotApplicable`. 4. Baris yang sama diproses dua kali → satu item. 5. Tarif tidak ada → `ReconciliationRequired` `TARIFF_NOT_FOUND`, tanpa item Rp0 (`UAT-02`). 6. Kunjungan rawat inap → `NotApplicable`. 7. Invoice Rawat Jalan tidak memuat item `REGISTRATION`; biaya admin tetap di `AdministrationFeeAmount` (`RJ-E2E-DEC-002`). 8. Kunjungan berasuransi → invoice terbentuk dan dihitung Billing (`UAT-24`, sisi backend) |
| **Verifikasi** | Pola Bank Darah; `UAT-01` (tanpa konsultasi/resep), `UAT-02`, `UAT-24` |
| **Risiko** | Enam task menunggu task ini; jangan memperluas cakupan |
| **DoD** | Delapan AC terbukti; laporan |

### `BE-RJE-004` — Pengaman `from-source`

| Field | Isi |
| --- | --- |
| **Outcome** | Pelayanan Rawat Jalan tidak dapat dicatat ulang dengan harga bebas lewat API |
| **Cakupan** | `from-source` menolak `422 RJE-VAL-010` bila `SourceDomain` salah satu dari lima domain klinis dan kunjungan `Outpatient` |
| **Acceptance criteria** | 1. `PROCEDURE` Rp1 untuk kunjungan Rawat Jalan → `422 RJE-VAL-010`, nol item baru (`UAT-03`). 2. `ADHOC` tetap diterima. 3. `EMERGENCY` dan `ROOM_STAY` tidak berubah |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-005` — Perubahan setelah invoice final

| Field | Isi |
| --- | --- |
| **Outcome** | Lab yang diterima setelah invoice `FINAL` menjadi adjustment `DEBIT` yang menunggu persetujuan, bukan hilang atau ditolak |
| **Cakupan** | Cabang `V2.7.5` untuk tagih baru/versi naik/selisih nol pada invoice `FINAL`/`CLOSED`/`SETTLED_BY_WRITE_OFF`; pencatatan `InvoiceAdjustmentId`; kode `ADJUSTMENT_REJECTED`, `NO_FINANCIAL_CHANGE` |
| **Preflight wajib** | Pastikan apakah `CreateAdjustmentAsync` menerima invoice `CLOSED` dan `SETTLED_BY_WRITE_OFF`. Hasilnya dicatat di laporan |
| **Acceptance criteria** | 1. Tagih baru pada invoice `FINAL` → adjustment `DEBIT` `SUBMITTED` sebesar harga (`UAT-04`). 2. Selisih nol → `Synced` `NO_FINANCIAL_CHANGE`, tanpa adjustment. 3. Penolakan service → `ReconciliationRequired` `ADJUSTMENT_REJECTED`. 4. Kirim ulang tidak membuat adjustment kedua |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; hasil preflight tercatat; laporan |

### `BE-RJE-006` — Kunjungan tanpa dokter berhenti di `Billing`

| Field | Isi |
| --- | --- |
| **Outcome** | Pasien yang hanya diskrining perawat tidak lagi tertutup sebelum ditagih |
| **Cakupan** | Cabang `!IsDoctorRequired` di `NurseStationQueueController`: kunjungan `Billing`, `CompletedAt` tidak diisi; antrean perawat tetap `Completed`. Controller legacy memakai `ApplicationDbContext` langsung — **tidak dirapikan** di task ini |
| **Preflight wajib** | Cari pembaca `EncounterStatus.Completed`/`CompletedAt` untuk kunjungan tanpa dokter (laporan, layar antrean) dan catat dampaknya |
| **Acceptance criteria** | 1. Tanpa dokter → `Billing`, `CompletedAt` kosong (`UAT-20`). 2. Dengan dokter → `WaitingForDoctor` tidak berubah (`UAT-21`) |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; dampak preflight tercatat; laporan |

### `BE-RJE-007` — Jasa konsultasi

| Field | Isi |
| --- | --- |
| **Outcome** | Setiap konsultasi yang selesai lewat finalisasi canonical menghasilkan satu item jasa konsultasi berharga katalog |
| **Cakupan** | `ConsultationFinalizationService` menerbitkan fakta `Consultation` setelah commit (termasuk dari `finish-consultation`); resolver `CONSULTATION` menurut `RJ-E2E-DEC-012` |
| **Acceptance criteria** | 1. Rule dr. B bertarif Rp150.000 → item Rp150.000. 2. Tanpa rule, tarif klinik + kelas → dipakai. 3. Tanpa keduanya, tarif klinik → dipakai. 4. Tanpa tarif sama sekali → antrean `TARIFF_NOT_FOUND`, konsultasi tetap selesai (`UAT-06`). 5. Selesai ditekan dua kali → satu item (`UAT-05`). 6. Konsultasi batal sebelum selesai → tanpa fakta |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-008` — Obat dua tahap

| Field | Isi |
| --- | --- |
| **Outcome** | Tn. A membayar resep Rp35.000, farmasi dapat menelaah dan menyerahkan obat, dan tagihan menyesuaikan jumlah yang diserahkan |
| **Cakupan** | `RuleSnapshot.milestone = "ClinicalFinalization"` pada fakta resep tahap 1; resolver `PHARMACY` (Σ jumlah × tarif, satu item per resep); `PrescriptionDispensingService` menerbitkan fakta tahap 2 setelah `Dispensed`/`PartiallyDispensed` dengan jumlah per item |
| **Titik sentuh** | Pharmacy. Sign-off formal Farmasi tetap `OPEN` dan menahan production, bukan task ini |
| **Acceptance criteria** | 1. Resep difinalkan → item `PRESCRIBED` Rp35.000. 2. Invoice dilunasi → clearance `CLEARED`; telaah dan serah obat berjalan (`UAT-07`). 3. Belum lunas → farmasi tetap ditolak (`UAT-09`). 4. Serah 8 dari 10 Vitamin C pada invoice `FINAL` → adjustment `CREDIT` Rp4.000 (`UAT-08`); pada invoice `OPEN` → item `DISPENSED` Rp31.000. 5. Satu obat tanpa tarif → seluruh resep di antrean |
| **Verifikasi** | Pola Bank Darah; alur runtime utuh dokter → kasir → farmasi |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-009` — Pembatalan dan koreksi

| Field | Isi |
| --- | --- |
| **Outcome** | Salah pasien dapat dikoreksi tanpa menghapus riwayat tagihan |
| **Cakupan** | Cabang pembatalan jembatan: void bila item masih di status void normal dan invoice `OPEN`; selain itu adjustment `CREDIT`. *CASE A* producer tetap tanpa akibat |
| **Acceptance criteria** | 1. Lab `ACCEPTED` batal, invoice `OPEN` → item `VOIDED` (`UAT-10`). 2. Nebulizer `PERFORMED` batal → adjustment `CREDIT` `SUBMITTED`, item tetap `ACTIVE` (`UAT-11`). 3. Lab batal sebelum diterima → tanpa void dan adjustment (`UAT-12`). 4. Resep `PRESCRIBED` batal sebelum diproses → item `VOIDED` |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-010` — Pekerja kirim ulang sinkron invoice

| Field | Isi |
| --- | --- |
| **Outcome** | Gangguan sementara tidak membuat tagihan tertinggal |
| **Cakupan** | `BilInvoiceSyncWorker` (`Billing/Workers/`), batch 50, jadwal `min(Base × 2^(n−1), Max)` dari `MstBillingSyncPolicy`; fail-closed bila tidak ada kebijakan aktif; `RETRY_EXHAUSTED`, `SYNC_POLICY_INACTIVE`; registrasi `AddHostedService` |
| **Acceptance criteria** | 1. Koneksi invoice diputus lalu pulih → item muncul sekali (`UAT-13`). 2. Gagal ke-5 → `ReconciliationRequired` `RETRY_EXHAUSTED`. 3. Kebijakan nonaktif → langsung antrean (`UAT-14`). 4. Pekerja dan pemanggilan langsung bersamaan → satu item |
| **Verifikasi** | Pola Bank Darah; waktu jadwal dicatat |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-011` — Pekerja kirim ulang fakta klinis

| Field | Isi |
| --- | --- |
| **Outcome** | Fakta yang hasil penyerahannya tidak pasti dikirim ulang dengan identitas yang sama sampai pasti, atau masuk antrean |
| **Cakupan** | `ClinicalMilestoneFactProducer.RedispatchAsync`; pengisian `NextDispatchAttemptAt` dan `ReconciliationRequiredAt`; `ClinicalFactDispatchWorker` (`ClinicalManagement/Workers/`) memakai kebijakan `FACT_DISPATCH` |
| **Irisan dengan scope Dokter** | Task ini **mewujudkan** AC `5` dan `6` `RJ-DOC-BE-005` (fakta yang belum terkirim dapat ditemukan dan dikirim ulang dengan identitas sama). AC `1`–`4` `RJ-DOC-BE-005` (deteksi *producer gap* antara commit klinis dan penulisan fakta) **tetap** milik roadmap Dokter dan wajib memakai ulang `RedispatchAsync`, bukan membangun jalur kedua |
| **Acceptance criteria** | 1. Fakta `OutcomeUnknown` dikirim ulang dengan `IdempotencyKey` yang sama; tidak ada fakta baru (`AC-RJ-014`). 2. Batas percobaan → `ReconciliationRequiredAt` terisi. 3. Kebijakan nonaktif → tidak ada kirim ulang otomatis |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; laporan; catatan rujukan ke `RJ-DOC-BE-005` |

### `BE-RJE-012` — API antrean rekonsiliasi dan kebijakan

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas Billing dapat melihat, mengirim ulang, dan menyelesaikan item antrean; admin dapat mengubah kebijakan tanpa rilis |
| **Cakupan** | `BillingChargeReconciliationController`/`Service` (3 endpoint), `BillingSyncPolicyController`/`Service` (2 endpoint), DTO; butir hak akses `BillingChargeReconciliation : Read/Update`, `BillingSyncPolicy : Read/Update`; audit |
| **Acceptance criteria** | 1. Daftar memuat fakta dan baris folio yang gagal/perlu rekonsiliasi, dengan saringan dan halaman. 2. Kirim ulang item `TARIFF_NOT_FOUND` setelah tarif diisi → `Synced` (`UAT-15`). 3. Selesaikan tanpa alasan → `422 RJE-VAL-022` (`UAT-16`). 4. Item `Synced` dikirim ulang → `422 RJE-VAL-020`. 5. Balapan dengan pekerja → `409 RJE-VAL-025`. 6. Kebijakan `MaxAttemptCount = 25` → `422 RJE-VAL-030`; `RowVersion` basi → `409 RJE-VAL-031`. 7. Tanpa butir → `403` |
| **Verifikasi** | Pola Bank Darah; `403` diuji dengan akun tanpa butir bila tersedia, bila tidak dicatat `NOT RUN` beserta alasannya |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-013` — `BillingHandoffIssues` pada `finish-consultation`

| Field | Isi |
| --- | --- |
| **Status** | Prasyarat kontrak `1.0.1` **terpenuhi** — `approved` `RJ-E2E-DEC-017` (2026-09-28). Belum dikerjakan; tabel gelombang masih menulis ⛔ dan diperbarui saat revisi roadmap berikutnya |
| **Outcome** | Dokter yang menekan Selesai dari antrean melihat masalah penyerahan tagihan |
| **Cakupan** | `DoctorQueueActionResponse` + `BillingHandoffIssues: string[]` yang disalin dari hasil finalisasi |
| **Acceptance criteria** | 1. Billing gagal saat Selesai → respons `200` memuat daftar masalah; konsultasi selesai. 2. Tanpa masalah → daftar kosong. 3. Konsumen lama tidak rusak |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; laporan |

### `BE-RJE-014` — Endpoint Ringkasan Billing kunjungan

| Field | Isi |
| --- | --- |
| **Outcome** | Dokter membaca status dan total tagihan kunjungan yang ia tangani, tanpa harga per item dan tanpa akses ke invoice pasien lain |
| **Cakupan** | `EncounterBillingSummaryController`/`Service`, `EncounterBillingSummaryResponse`; total dari `PreviewCalculationAsync`; hitungan sinkron dari folio; butir `EncounterBillingSummary : Read` |
| **Acceptance criteria** | 1. Kunjungan tanpa invoice → `200 NO_INVOICE` (`UAT-17`). 2. Angka sama dengan `calculation-preview`. 3. Respons tanpa `UnitPrice`/`TotalPrice`. 4. Pengguna dengan butir ringkasan saja → `403` pada `GET /billing/invoices/{id}` (`UAT-18`). 5. Tanpa sesi → `401` |
| **Verifikasi** | Pola Bank Darah |
| **DoD** | AC terbukti; laporan |

## Coverage gap

| Gap | Keterangan | Penanganan |
| --- | --- | --- |
| E2E lintas task (`UAT-01` utuh dengan konsultasi + resep + kasir + farmasi) | Tidak dimiliki satu task pun | `verify-module-readiness` setelah `MVP-4` |
| `RJ-DOC-BE-005` AC `1`–`4` | Milik roadmap Dokter | Tetap di `doctor-consultation-roadmap.md` |
| `SEC-RJ-004` XSS | Frontend | `FE-RJE-001`, `FE-RJE-003` |
| Layar master kebijakan kirim ulang | `POST-MVP` | — |
