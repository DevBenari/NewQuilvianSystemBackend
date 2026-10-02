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

---

## 2. Amandemen kontrak `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

**Status amandemen: `draft`.** Baseline kontrak `1.0.0` tetap `approved` (`RWI-DEC-162`) untuk isi yang tidak disentuh. Kontrak `1.1.0` **mencabut** sebagian isi `1.0.0` — webhook izin pulang, PIN dan pemeriksaan nama peran pada override, hitungan tarif kamar kedua, dan gerbang kasir pada pulang fisik (`02-backend-architecture.md` 9.1) — sehingga bagian yang dicabut itu tidak boleh lagi dijadikan dasar task baru walaupun `1.1.0` belum disetujui. Approval adalah tindakan manusia; dokumen ini **tidak** menandai apa pun `approved`. **Diselaraskan 2 Oktober 2026** dengan decision log revision `31` (`RWI-DEC-206` s.d. `220`) tanpa menaikkan versi kontrak; artefak yang terdampak memuat bagian Penyelarasan decision log revision `31`.

| Field | Nilai |
| --- | --- |
| `contract_version` | `1.0.0` → **`1.1.0`** (`draft`) |
| `input_revision` | `PRD-RWI-FINISHING-001` v`0.4`; decision log revision `31` (diselaraskan 2 Oktober 2026); gate `1.9` bagian 18; capability map `1.6` bagian 19 |
| `backend_commit_sha` | Audit `c8e99ce5`; diperiksa ulang terhadap HEAD **`8d96a978`** (1 dan 2 Oktober 2026) — perubahan sesudah audit hanya saringan pencarian census dan daftar pantau; tidak menyentuh area yang dirancang |
| `frontend_commit_sha` | Audit `22ad67330`; diperiksa ulang terhadap HEAD **`bf5af8090`** — gaya tampilan, pencarian, label menu Dashboard; tidak menyentuh area yang dirancang |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — batas domain diambil dari keputusan pemilik dan source yang dibaca |
| Kemampuan | `CAP-RWF-01` s.d. `04`, `CAP-RWF-15` — slice `INP-S23`, `INP-S24`, `INP-S25` (bagian hak rupiah) |
| Keputusan | `RWI-DEC-163` s.d. `170`, `186`, `187`, `192`, `195`, `196` (`RWI-DEC-185` superseded); penyelarasan `RWI-DEC-207`, `210`; fakta `RWI-FACT-041`, `042`, `046`, `047`, `050`, `052` |
| Peta modul | `../02-module-map.md` revision `4` bagian 7 — kepemilikan data, menu, urutan migration lintas sub-modul |

### 2.1 Tabel artefak dan hash

| Artefak | Bagian | Status | SHA-256 |
| --- | --- | --- | --- |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | bagian 9 | `draft` | `29777e590ee52abb47116c163a4e613e225abc2431e9b9372f4d86e7e41962aa` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | bagian 6 | `draft` | `20bcb128b18c35b31fb91ed47e4932af03d238820b7bb8ff7f61a6cf7245139f` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | bagian 8 | `draft` | `95f1a0238b5c5ce53b031627f50156359163fc44220a56280bf81730febdf429` |
| [`05-skema-tampilan.md`](./05-skema-tampilan.md) | catatan "sebagian digantikan" di kepala | `draft` | `166be1aff140d794260579771fb4d60cb16b6bbb1fcbee058e0b3ccdf8cebbaf` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | bagian 3 | `draft` | `b2f75a70485ce4a4954fadc18cab643e0af0e3ca645564fc0ae419eb72bb4183` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | bagian 5 | `draft` | `5578bd1de46547b0f2e8e2cf383385721c5b3c5293287d1368460aa1711e94a9` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | bagian 2 | `draft` | `b9fc8a755c95265382869848ca78f04257a6735e3a4ba3eff2129e85e9df0517` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | bagian 4 | `draft` | `566ad7d5a5bc7db06cd582840b02d84260b2b3a1b3e6d6ec30ef742fbd30653e` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | bagian 5 | `draft` | `78da2be39941c7db5bb27ed73ba5a4093884f59807f272c472c1441c6f1adaee` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | bagian 6 | `draft` | `58b5e101318114ea143aca81604d41911e329ecc95b5b42c2f2a2963208e6d92` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | bagian 4 | `draft` | `532c0b8ee6e4e0ecb120abd3de49a7c849c0dc51bf94ed9a8548c20e945fb6ce` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | bagian 3 | `draft` | `7573f6036a8d2725cd8154c9cf4801e3fa9f2a7bdb37e779065a4a04697d43ba` |
| [`flowcharts/03-clearance-dan-auto-reblock.md`](./flowcharts/03-clearance-dan-auto-reblock.md) | catatan "digantikan" di kepala | `draft` | `2442cad04ddcb22473dde0d170dc4608bdbf3cac1d9bdc41071dcedef3866c80` |
| [`flowcharts/04-keluar-ruangan-dan-penutupan.md`](./flowcharts/04-keluar-ruangan-dan-penutupan.md) | baru | `draft` | `257728ec3b0ba8c3435733a3be61f4047deb1b0fa79bdaadd06a97e91bff11b2` |
| [`flowcharts/05-ketukan-pintu-billing.md`](./flowcharts/05-ketukan-pintu-billing.md) | baru | `draft` | `3076285118cd8e7aa0682baa1542579882bed13e53f6d7f7255a2389da0f539d` |
| [`flowcharts/06-koreksi-penempatan-dan-putar-ulang.md`](./flowcharts/06-koreksi-penempatan-dan-putar-ulang.md) | baru | `draft` | `723e8e8fa0c2c2275fd4118a73d0c40a2b174ce0710eb9a6b6c1313be58e8484` |

Hash di bawah adalah hash saat amandemen ditulis, **bukan** hash approval. Approval tetap tindakan manusia; saat disetujui, hash dihitung ulang dan menjadi acuan deteksi perubahan berikutnya.

### 2.2 Dependency dan gerbang

| Bergantung pada | Untuk apa | Keadaan | Menahan |
| --- | --- | --- | --- |
| Billing — Yasmina | Penerima ketukan pintu, kuitansi, invoice perlu diperiksa, jembatan `RANAP`, `/amounts` | **Disetujui** `RWI-DEC-192` | Tidak |
| Farmasi — Ikbal Yulianto | Retur obat membatalkan tagihan (`INT-RWF-05`) | **Disetujui** `RWI-DEC-210` | Tidak |
| Billing — `billing-kasir` `BKC-DEC-118` (`BILL-INT-007`) | Penyelesaian satu kwitansi untuk invoice kunjungan asal dan `RANAP` yang tertaut (`RWI-DEC-207`) | Keputusan Billing `approved`; pembayaran multi-invoice belum ada di source (hari ini per invoice) | Bagian satu transaksi pada `RWI-AC-330` saja |
| `keperawatan` `0.6.0` | Penutupan pemakaian alat saat keluar ruangan (`INT-RWF-06`); layar Tagihan Pasien `FE-KEP-23` | Dirancang pada pass yang sama, `draft` | Tidak |
| `episode-rawat-inap` `0.10.0` | Laporan transfer membaca `CorrectsPlacementId`; Billing membaca `InpAdmissionReferral` untuk tautan kunjungan asal (`INT-RWF-29`, `I6` sesudah `E6`) | Dirancang pada pass yang sama, `draft` | Tautan `RWI-DEC-207` |

### 2.3 Handoff

| Field | Nilai |
| --- | --- |
| `blueprint_id` / `contract_version` | `RWI-BP-001` / `1.1.0` |
| `approval_status` | **`draft`** — `approved_by` dan `approved_at` kosong |
| `blocking_questions` | **Nol.** Tidak ada gerbang persetujuan modul tetangga (`RWI-DEC-210`) |
| `next_owner` | Approval pemilik atas kontrak `1.1.0` (Muhammad Hamzah & Yasmina) → `plan-module-delivery` untuk `RWF-W0` dan `RWF-W1` |
