# Rawat Inap — Blueprint Manifest: Integrasi Rawat Inap ↔ Kasir / Billing

Manifest tingkat sub-modul untuk **Integrasi Rawat Inap ↔ Kasir / Billing** (`integrasi-billing`), mencakup slice kemampuan **`INP-S22`** (`INT-CAP-01` s.d. `INT-CAP-06` / `RANAP-INT-001` s.d. `006`).

| Field | Nilai |
|---|---|
| `blueprint_id` | `RWI-BP-001-INT-BIL` |
| `parent_blueprint_id` | `RWI-BP-001` (`rawat-inap`) |
| `submodule_slug` | `integrasi-billing` |
| `revision` | **`1.0.0`** — 17 September 2026 (`Asia/Jakarta`), perancangan to-be blueprint integrasi rawat inap dengan billing |
| `status` | **`approved`** — disetujui resmi oleh Muhammad Hamzah (Product & Domain Owner Rawat Inap) pada 17 September 2026 lewat `RWI-DEC-162` |
| `module` | `InPatientManagement` bertukar data dengan `BillingManagement` |
| `scope_type` | `SHARED_INTEGRATION_SUBMODULE` |
| `slice_id` | **`INP-S22`** |
| `owners` | Product/Domain Rawat Inap: **Muhammad Hamzah**; Product/Domain Billing: **Yasmina**; Tech Lead: **Leader** |
| `approved_by` | **Muhammad Hamzah** (Rawat Inap) |
| `approved_at` | **2026-09-17** |
| `approval_decision` | **`RWI-DEC-162`** |
| `backend_commit_sha` | `fe7e60d4b2ef1eecffa72cef4f4fd33f9dbe0344` (branch `MHamzah`) |
| `frontend_commit_sha` | `2c00758832f834cff0288bef4f0d2fcf1161fb52` (branch `HamzahV2`) |
| `contract_version` | `1.0.0` |
| `primary_source` | `docs/Modul-RS/Rawat-Inap-To-Billing/PRD Integrasi-Rawat-Inap-dengan-Billing.md` (2.282 baris) |
| `interview_evidence` | `docs/module-blueprints/rawat-inap/00-interview-decisions.md` revision `26`, `RWI-DEC-156` s.d. `RWI-DEC-162`, `RWI-AC-236` s.d. `RWI-AC-241` |
| `capability_evidence` | `docs/module-blueprints/rawat-inap/01-existing-capability-map.md` revision `1.5` Bagian 18 (`INT-CAP-01` s.d. `INT-CAP-06`) |
| `requirement_gate` | `docs/module-blueprints/rawat-inap/evidence/02-requirement-completeness-gate.md` revision `1.7` Bagian 16 (`READY_FOR_DOMAIN_DESIGN`) |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — batas bounded context dan relasi aggregate sudah diputuskan secara tegas oleh Product Owner lewat `RWI-DEC-156` s.d. `161` |
| `compatibility_impact` | Penambahan 1 tabel outbox baru (`InpIntegrationOutbox`), penambahan event publisher worker, penambahan endpoint pembacaan status kasir operasional (tanpa rupiah), penambahan webhook clearance & endpoint supervisor override pemulangan darurat |

---

## 1. Daftar Berkas Blueprint Sub-Modul

| Nama Berkas | Kontrak Versi | Status | Deskripsi |
|---|:---:|:---:|---|
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `1.0.0` | `approved` | Arsitektur backend: bounded context, event outbox, transaction boundary, class diagram, dan rencana migration |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `1.0.0` | `approved` | Arsitektur frontend: UI bangsal tanpa nominal rupiah, badge status kasir, clearance gate, supervisor override |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `1.0.0` | `approved` | Dokumen PRD ke MVP: batasan rilis, epic MUST HAVE, kriteria penerimaan, dan Definition of Done |
| [`05-skema-tampilan.md`](./05-skema-tampilan.md) | `1.0.0` | `approved` | Skematik detail layar dan komponen interaktif status billing operasional di bangsal rawat inap |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | `1.0.0` | `approved` | Flowchart alur bisnis integrasi pokok dari admisi hingga pemulangan fisik dan invoice kasir |
| [`flowcharts/01-admisi-ke-billing.md`](./flowcharts/01-admisi-ke-billing.md) | `1.0.0` | `approved` | Flowchart pembentukan folio tagihan kasir dan inisiasi sewa kamar harian sejak bed occupied |
| [`flowcharts/02-mutasi-koreksi-kamar.md`](./flowcharts/02-mutasi-koreksi-kamar.md) | `1.0.0` | `approved` | Flowchart perpindahan kamar dan koreksi kelas/kamar saat status tagihan OPEN vs penolakan saat CLOSED |
| [`flowcharts/03-clearance-dan-auto-reblock.md`](./flowcharts/03-clearance-dan-auto-reblock.md) | `1.0.0` | `approved` | Flowchart persetujuan kasir, pencabutan clearance (*Auto-Reblock*), dan penanganan darurat *Supervisor Override* |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `1.0.0` | `approved` | Kamus data tabel `InpIntegrationOutbox`, audit occupancy, dan perpanjangan entitas pemulangan |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `1.0.0` | `approved` | Spesifikasi endpoint REST Swagger: query status kasir, webhook sinyal clearance, dan supervisor override |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `1.0.0` | `approved` | Spesifikasi shared integration contract: Producer/Consumer event outbox dan idempotency key |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `1.0.0` | `approved` | Matriks transisi status admisi, penempatan bed, kelayakan kasir, dan pengiriman event outbox |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `1.0.0` | `approved` | Matriks validasi bisnis, precondition checks, dan pesan penolakan sistem |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `1.0.0` | `approved` | Matriks hak akses (`InpatientBilling:View`, `InpatientSupervisor:Override`) dan pencatatan jejak audit |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `1.0.0` | `approved` | Matriks pengujian otomatis dan UAT integrasi end-to-end (`UAT-INT-001` s.d. `013`) |
| [`roadmap/backend-roadmap.md`](./roadmap/backend-roadmap.md) | `1.0.0` | `approved` | Rencana pengiriman backend: 8 task vertical slice (`BE-RWI-127` s.d. `BE-RWI-134`) dalam 4 gelombang |
| [`roadmap/frontend-roadmap.md`](./roadmap/frontend-roadmap.md) | `1.0.0` | `approved` | Rencana pengiriman frontend: 6 task UI/UX (`FE-RWI-095` s.d. `FE-RWI-100`) dalam 3 gelombang |
| [`roadmap/requirement-traceability.md`](./roadmap/requirement-traceability.md) | `1.0.0` | `approved` | Matriks keterlacakan: 18 FR, 6 AC, 6 NFR, 13 UAT ke task BE/FE tanpa gap |
