# Manajemen Aset & Inventaris — Blueprint Manifest

Manifest **tingkat modul**. Modul ini berbentuk `COMPOSITE`: berkas ini memegang identitas modul,
snapshot SHA, masukan hulu, dan **registry sub-modul**. Status desain, `contract_versions`,
approval, dan dependency masing-masing sub-modul dipegang manifest di dalam folder sub-modul itu.

Status modul **diturunkan** dari status sub-modul dan tidak ditulis tangan
(`rules/rule-output/bentuk-blueprint.md` bagian 7).

| Field | Value |
|---|---|
| `blueprint_id` | `AST-BP-001` |
| `module_name` | Manajemen Aset & Inventaris |
| `module_slug` | `manajemen-aset` |
| `module_prefix` | Entity `Ast`; task `BE-AST-###` dan `FE-AST-###`, satu deret bersama untuk seluruh sub-modul |
| `module_area` | `Areas/Corporate/AssetManagement` — **usulan, belum terdaftar** di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Pendaftaran registry (QBE-MOD-002/003) adalah task backend terpisah sebelum model pertama dibuat |
| `revision` | `1` |
| `blueprint_shape` | **`COMPOSITE`** — 5 sub-modul, `AST-DEC-001` |
| `shape_decided_by` | **`USER_CONFIRMED`** — agent menyarankan COMPOSITE 5 sub-modul, pemilik memilih opsi itu (2026-11-12) |
| `status` | **`approved`** (diturunkan: kelima sub-modul `approved` 2026-11-12, `AST-DEC-051`) |
| `current_phase` | `AST-PH-004` — implementasi per task; `AST-PH-001..003` `DONE`. Task pertama yang boleh diminta: `BE-AST-001` (registry) |
| `owners` | Product/Domain owner, API, security, frontend authority: **Rizki** (`AST-DEC-002`); keputusan akuntansi juga Rizki selaku pemilik modul Akuntansi |
| `branch` | Backend `rizkiG`, frontend `RizkiV2` (`AST-DEC-002`) |
| `regulatory_regime` | RS swasta — kebijakan internal badan hukum pemilik RS dan standar akuntansi aset tetap (PSAK 216, dahulu PSAK 16). Istilah seperti "Pemindahtanganan" dipakai sebagai nama menu, bukan sebagai rujukan aturan Barang Milik Daerah/Negara (`AST-DEC-003`) |
| `created_at` | 2026-11-12T08:55:21+07:00 |
| `updated_at` | 2026-11-12 — desain disetujui (`AST-DEC-051`) dan roadmap `REV-1` ditulis |
| `last_verified_at` | null |
| `backend_source_sha` | `1f54a17551ac49a1d12f3186ce982c1e8107c91e` (branch `rizkiG`) |
| `frontend_source_sha` | `e49e8a8158c652f4405588c8a787f9355baea690` (branch `RizkiV2`) |
| `baseline_requirement` | Daftar cakupan 11 menu dari pemilik, sesi 2026-11-12 — disalin utuh pada `00-interview-decisions.md` bagian 1 |
| `input_revision_hash` | SHA-256/16: `00-interview-decisions.md` `d6b4ccd1b203aa6f`; `01-existing-capability-map.md` `3898335bafb19b38`; `02-module-map.md` `66cdae06d0e36955`; `requirement-completeness-assessment.md` `4653266a70abca3f` |
| `decision_revision` | `5` — `AST-DEC-001..051` |
| `contract_versions` | `register` `AST-REG-1.0.0`, `pergerakan` `AST-MOV-1.0.0`, `inventarisasi` `AST-INV-1.0.0`, `nilai` `AST-NLI-1.0.0`, `penghapusan` `AST-HPS-1.0.0` — seluruhnya `approved` 2026-11-12 (`AST-DEC-051`); rincian dan `artifact_hashes` di manifest sub-modul |
| `active_dependency_ids` | Prasyarat registry `Ast` (`QBE-MOD-002/003`); `DEP-AST-01` (pusat biaya pada kejadian Akuntansi), `DEP-AST-02` (jenis kejadian + aturan posting), `DEP-AST-03` (nonaktifkan jurnal berulang penyusutan), `DEP-AST-04` (akun layanan pengirim) — `nilai/contracts/integration-contract.md` bagian 5; titik sentuh `TS-01..TS-13` |
| `active_roadmap_revision` | `REV-1` (2026-11-12) — satu roadmap per sub-modul; **37 task backend** `BE-AST-001..037`, **26 task frontend** `FE-AST-001..026`; ID bebas berikutnya `BE-AST-038` dan `FE-AST-027`. Satu task ⛔: `BE-AST-032` menunggu `GATE-AST-ACC` |
| `requirement_readiness` | `READY_FOR_DOMAIN_DESIGN` untuk seluruh MVP; slice `N6` Revaluasi `BUSINESS_DECISION_REQUIRED` (`DEFERRED`) — [`requirement-completeness-assessment.md`](requirement-completeness-assessment.md) `AST-RCG-001` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — alasan di `AST-RCG-001` bagian 7 |
| `supersedes` | null |

## 1. Registry sub-modul

Sub-modul dipisah menurut **siklus hidup data dan pemilik persetujuan**, bukan menurut butir menu.
Hasil uji pemecahan (`bentuk-blueprint.md` bagian 4.1) per sub-modul: `00-interview-decisions.md` bagian 11.

| Slug | Rumpun kemampuan | Menu cakupan pemilik yang dilayani | Status | Uji pemecahan |
|---|---|---|---|---|
| [`register`](register/blueprint-manifest.md) | Master klasifikasi, pencatatan aset, nomor inventaris, label, dokumen kepemilikan, penempatan dan penanggung jawab, kondisi dan status, pemanfaatan dan pengamanan, impor data awal | 1 (sebagian), 2, 3, 4, 6, 8, 11 (sebagian) | `approved` — `AST-REG-1.0.0` | 5/5 |
| [`pergerakan`](pergerakan/blueprint-manifest.md) | Mutasi antar unit, persetujuan, serah terima, peminjaman sementara, pengembalian | 1.4, 5, 11.2 | `approved` — `AST-MOV-1.0.0` | 4/5 |
| [`inventarisasi`](inventarisasi/blueprint-manifest.md) | Jadwal inventarisasi, pemeriksaan fisik, pencocokan, selisih, verifikasi, berita acara | 1.5, 7, 11.4 | `approved` — `AST-INV-1.0.0` | 4/5 |
| [`nilai`](nilai/blueprint-manifest.md) | Harga perolehan, nilai buku, riwayat perubahan nilai, penyusutan, rekonsiliasi dengan akuntansi | 9, 11.6 | `approved` — `AST-NLI-1.0.0` | 5/5 |
| [`penghapusan`](penghapusan/blueprint-manifest.md) | Usulan penghapusan, pemeriksaan kelayakan, persetujuan, pemindahtanganan, berita acara, riwayat aset dihapus | 1.6, 10, 11.5 | `approved` — `AST-HPS-1.0.0` | 4/5 |

Dashboard (menu 1) dan laporan (menu 11) tidak menjadi sub-modul sendiri karena tidak punya data
atau siklus status milik sendiri. Peletakan tiap kartu dashboard dan tiap laporan ditetapkan pada
`02-module-map.md`.

## 2. Keputusan struktur

| ID | Keputusan | Diputuskan oleh | Tanggal |
|---|---|---|---|
| `AST-DEC-001` | Bentuk `COMPOSITE` dengan 5 sub-modul: `register`, `pergerakan`, `inventarisasi`, `nilai`, `penghapusan` | Rizki (jawaban pertanyaan pop-up) | 2026-11-12 |
| `AST-DEC-002` | Pemilik modul Rizki; branch backend `rizkiG`, frontend `RizkiV2` | Rizki | 2026-11-12 |
| `AST-DEC-003` | Rezim aset: RS swasta, standar akuntansi aset tetap (PSAK 216) dan kebijakan internal | Rizki | 2026-11-12 |
| `AST-DEC-004` | Identitas teknis: Area `Areas/Corporate/AssetManagement`, prefix `Ast`, slug blueprint `manajemen-aset`. Prefix `Ast` diperiksa belum dipakai di source backend (`class Ast*` nol hasil) maupun registry | Rizki | 2026-11-12 |

## 3. Mode dan batas wewenang

Berkas di folder ini ditulis dalam `MODULE BLUEPRINT MODE` atas permintaan pemilik ("buatkan
module baru, dengan blueprint, PRD, evidence, roadmap"). Mode ini **tidak** memberi wewenang menulis
source backend/frontend, mendaftarkan registry, membuat atau menjalankan migration, maupun
publikasi Git.

## 4. Keadaan fase (2026-11-12)

| Fase | Isi | Status | Bukti |
|---|---|---|---|
| `AST-PH-001` | Wawancara (12 putaran, 50 keputusan), audit kemampuan, gerbang kelengkapan | `DONE` | `00-interview-decisions.md` rev 4, `01-existing-capability-map.md` 1.0, `requirement-completeness-assessment.md` `AST-RCG-001` |
| `AST-PH-002` | Desain lima sub-modul + peta modul | `DONE` — disetujui Rizki 2026-11-12 (`AST-DEC-051`) | `02-module-map.md`, 64 artefak sub-modul |
| `AST-PH-003` | Roadmap BE/FE per sub-modul dan traceability | `DONE` — `REV-1` 2026-11-12, grafik dependency diperiksa skrip (garis lurus, pasangan = kolom `Dependency`) | — |
| `AST-PH-004` | Implementasi per task | `READY` — menunggu wewenang `TASK MODE: BACKEND`/`FRONTEND MODE` per task | — |

Yang **tidak** dilakukan sampai fase ini: nol source backend/frontend, nol registry, nol migration, nol build,
nol publikasi Git. Seluruh pekerjaan sesi 2026-11-12 terbatas pada `docs/module-blueprints/manajemen-aset/**`.
