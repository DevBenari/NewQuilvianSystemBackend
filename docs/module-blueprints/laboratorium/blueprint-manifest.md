# Laboratorium — Blueprint Manifest

```yaml
blueprint_id: LAB-BP-001
module_name: Laboratorium
module_slug: laboratorium
module_prefix: LAB
revision: 13
status: approved-with-pending-reconciliation
created_at: 2026-09-01T00:00:00+07:00
updated_at: 2026-09-01T00:00:00+07:00

scope:
  release: MVP Rilis 1 — bagian yang sudah lolos kedua gerbang
  slices_in_scope: [S1a, S2, S3, S7, S10, S11, S13a, S13b, S14, S15]
  slices_out_of_scope: [S1b, S2b, S4, S4b, S4c, S5, S6, S8, S9, S16, S17, S18, S19]
  discipline: Patologi Klinik, Patologi Anatomi, dan Mikrobiologi (LAB-DEC-025). Bank Darah di luar scope

owners:
  product_domain: Yoga Aji Pratama <yogaaji452@gmail.com>
  api: belum ditetapkan
  security: belum ditetapkan
  frontend_authority: konvensi project + DEV_DISCRETION (LAB-DEC-010)
  clinical_governance: belum ditetapkan — lihat LAB-SIGN-001

approved_by: Yoga Aji Pratama <yogaaji452@gmail.com>
approved_at: 2026-09-01

backend_commit_sha: "9124900"   # cabang yoga, 7 commit tertinggal dari origin/yoga c9692d0; tujuhnya tidak menyentuh Laboratorium
frontend_commit_sha: "688daff90"

input_revisions:
  decisions: 19
  capability_map: 1
  requirement_gate: LAB-RCG-001-r4
  domain_architecture: LAB-DA-001-r4

input_hashes:
  00-interview-decisions.md: "3b25b87d970204cf"
  01-existing-capability-map.md: "4824412cc812b7bd"
  02-requirement-completeness-assessment.md: "85b46acddbe88d10"
  03-domain-architecture.md: "a1f8954068484a1d"

requirement_gate_readiness: PARTIALLY_READY
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
domain_architecture_revision: LAB-DA-001-r4

contract_versions:
  - LAB-API-v1: draft
  - LAB-STATE-v1: draft
  - LAB-VAL-v1: draft
  - LAB-INT-v1: draft
  - LAB-PERM-v1: draft

evidence_baseline:
  source: Analisis_Konsolidasi_Modul_Laboratorium.md
  adopted_by: LAB-DEC-025 .. LAB-DEC-031
  adopted_at: 2026-09-01
  authority_level: bukti sistem berjalan dan analis — tingkat 4-7, di bawah keputusan pemilik
  limitation: audio video belum ditranskripsi; aturan yang hanya disampaikan lisan belum tercakup

active_blockers:
  - LAB-SIGN-001    # tanda tangan klinis — memblokir S4, S4b, S4c, S5, S6
  - LAB-AMD-001     # amandemen rawat-jalan — memblokir S1b
  - LAB-OPEN-018    # rules root terpasang belum memuat rules/backend/engineering/ — memblokir IMPLEMENTATION
  - LAB-OPEN-019    # lifecycle registry LaboratoryManagement masih PLANNED — QBE-MOD-002 menahan entity Lab*
  - LAB-OPEN-012    # jumlah data lab existing belum diverifikasi — prasyarat migration
  - LAB-OPEN-013    # dampak cito dan duplo pada tarif
  - LAB-OPEN-014    # nilai kritis untuk mikrobiologi dan patologi anatomi
  - LAB-OPEN-017    # makna penanda Definitif
  - LAB-P0-001      # matriks kewenangan per peran
  - LAB-P0-002      # urutan status resmi, termasuk Confirmed
  - LAB-P0-003      # aturan pembatalan dan koreksi
  - LAB-P0-004      # alur nilai kritis lengkap
  - LAB-P0-005      # integrasi alat laboratorium
  - LAB-P0-006      # kebijakan jejak audit
  - LAB-P0-007      # aturan tagihan dan cakupan
  - LAB-P0-008      # penyelarasan antaraplikasi

cross_module_approvals:
  request_id: LAB-REQ-001
  approved_by: andryzainhome <andryzain01@gmail.com>; sukmagp — Sukma Giri Pratama <sukmagiri11@gmail.com>
  approved_at: 2026-09-01
  scope: LAB-COORD-001 .. LAB-COORD-005
  not_covered: LAB-OPEN-012 memerlukan jawaban faktual; LAB-SIGN-001 memerlukan wewenang klinis. LAB-OPEN-002 ditutup 2026-09-01 oleh LAB-FACT-007, menurunkan LAB-OPEN-018 dan LAB-OPEN-019

inherited_decisions:
  source: rawat-jalan / RJ-BIL-GATE-DEC-003
  status: locked-draft, tata kelola formal OPEN
  ids: [LAB-INH-001 .. LAB-INH-013]
```

## Daftar artefak

| Berkas | Ditulis oleh | Status |
|---|---|---|
| `blueprint-manifest.md` | `design-business-module` | rev 12 |
| `00-interview-decisions.md` | `grill-me` | rev 18 — **36 keputusan `approved`**; 5 koordinasi lintas modul ditutup |
| `01-existing-capability-map.md` | `trace-existing-capabilities` | rev 1 |
| `02-requirement-completeness-assessment.md` | `requirement-completeness-gate` | rev 5 — `PARTIALLY_READY`, 10 dari 21 bagian siap |
| `03-domain-architecture.md` | `hospital-domain-architect` | rev 4 — **`DOMAIN_ARCHITECTURE_READY`** untuk 10 slice |
| `05-evidence-reconciliation.md` | `design-business-module` | rev 2 — `RECONCILED` |
| `02-backend-architecture.md` | `design-business-module` | rev 3 |
| `03-frontend-architecture.md` | `design-business-module` | rev 3 — menu data induk mengikuti konvensi FE yang sudah ada |
| `04-prd-to-mvp.md` | `design-business-module` | rev 2 — batas MVP diperluas, 10 epic, 5 gelombang |
| `erd/00-context-erd.md` | `design-business-module` | rev 2 — data induk perujuk dan kolom disiplin masuk |
| `erd/laboratory-operations.md` | `design-business-module` | rev 2 — lapis rujukan katalog, harga, cakupan |
| `erd/data-dictionary.md` | `design-business-module` | rev 2 — empat tabel milik modul lain didokumentasikan |
| `contracts/api-contract.md` | `design-business-module` | rev 2 — 3 grup endpoint baru |
| `contracts/state-transition-matrix.md` | `design-business-module` | rev 1, sudah disesuaikan `LAB-DEC-026` |
| `contracts/validation-matrix.md` | `design-business-module` | rev 2 — VAL-40 sampai VAL-50 |
| `contracts/integration-contract.md` | `design-business-module` | rev 2 — `INT-05` dan `INT-06` |
| `contracts/permission-audit-matrix.md` | `design-business-module` | rev 2 — 9 kewenangan baru |
| `testing/acceptance-test-matrix.md` | `design-business-module` | rev 2 — AC-39 sampai AC-51 |

### Artefak operasional — di luar daftar hash

| Berkas | Sifat | Status |
|---|---|---|
| `approval-requests/2026-09-01-permintaan-koordinasi-lintas-modul.md` | Operasional, bukan artefak desain | **`dijawab sebagian`** — 5 disetujui, 2 menunggu jawaban faktual, 1 di luar wewenang |

## Catatan status

Blueprint **disetujui** pemilik modul pada 2026-09-01. Seluruh artefak desain tetap berstatus
`draft` karena cakupan berubah setelah bukti lapangan diadopsi. Tidak ada satu baris source
aplikasi yang diubah pada tahap ini.

### Artefak yang masih perlu disesuaikan

| Berkas | Yang belum masuk | Keadaan |
|---|---|---|
| `erd/00-context-erd.md` | Instansi dan dokter perujuk sebagai data induk milik `BC-MD`; kolom disiplin pada `MstProcedure` | **Terbuka** — `LAB-COORD-004` dan `LAB-COORD-005` disetujui 2026-09-01, siap dirancang |
| `erd/laboratory-operations.md` | Relasi ke data induk perujuk | Sama seperti di atas |
| `erd/data-dictionary.md` | Kolom kunci data induk perujuk; kolom disiplin `MstProcedure` | Sama seperti di atas |
| `contracts/state-transition-matrix.md` | Perpindahan status pendaftaran | Tidak ada lifecycle baru milik Laboratorium; kunjungan mengikuti lifecycle Registrasi |

Ketiga berkas ERD kini **siap dilengkapi** setelah koordinasi lintas modul disetujui.



Nomor awalan berkas sengaja berulang — `02-` dan `03-` masing-masing dipakai dua kali. Ini
mengikuti pola yang sudah dipakai modul `pharmacy` dan `operations`, di mana artefak gerbang
dan artefak desain hidup berdampingan.

## Pemicu perubahan revision

| Pemicu | Akibat |
|---|---|
| Backend bergerak dari `9124900` | Impact scan `trace-existing-capabilities`; manifest naik revision |
| Frontend bergerak dari `688daff90` | Impact scan bagian frontend |
| Salah satu blocker aktif ditutup | Slice terkait masuk scope; seluruh artefak desain naik revision |
| `LAB-DEC-024` berubah | Arsitektur backend, ERD, dan kontrak wajib disusun ulang |
