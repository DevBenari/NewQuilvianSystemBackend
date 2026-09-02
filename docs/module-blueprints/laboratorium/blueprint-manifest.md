# Laboratorium — Blueprint Manifest

```yaml
blueprint_id: LAB-BP-001
module_name: Laboratorium
module_slug: laboratorium
module_prefix: LAB
revision: 19
status: approved-with-pending-reconciliation
bentuk: SINGLE
created_at: 2026-09-01T00:00:00+07:00
updated_at: 2026-09-02T00:00:00+07:00

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

backend_commit_sha: "c87d9c0"   # cabang yoga, sinkron dengan origin/yoga per 2026-09-01
backend_impact_scan:
  from: "9124900"
  to: "c87d9c0"
  dilakukan: 2026-09-01
  temuan_berdampak:
    - ClinicalBillingIntegration dipindah ke ClinicalManagement
    - TrxClinicalMilestoneFact dinamai ulang menjadi CliClinicalMilestoneFact (migration RenameClinicalMilestoneFactToCliPrefix)
    - Configuration Laboratorium dipindah ke Repositories/Configurations/HealthServices/LaboratoryManagement/
    - Berkas uji dipindah dari tests/ ke Tests/
  tidak_berdampak:
    - Model Laboratorium tidak berubah; temuan capability map atas model tetap sahih
    - Perubahan LabOrderService dan LabSpecimenService hanya pada baris using
frontend_commit_sha: "688daff90"

input_revisions:
  decisions: 21   # rev 21 hanya merapikan tabel Riwayat Revisi; tidak mengubah satu keputusan pun, sehingga kontrak yang dikunci terhadap rev 20 tetap sahih
  capability_map: 2   # STALE dicabut 2026-09-02 lewat impact scan; CAP-11 dan utang teknis diverifikasi tidak berubah
  requirement_gate: LAB-RCG-001-r4
  domain_architecture: LAB-DA-001-r4

input_hashes:                  # sha256 penuh atas isi ber-line-ending LF, mengikuti konvensi pharmacy dan billing-kasir. Dihitung ulang 2026-09-02
  00-interview-decisions.md: 75d285252aa5bce7fcaf5d90242da0d30fbd58a92a16aca3377683243be45f61
  01-existing-capability-map.md: 703a8dffe23971ecc09f416a516e4d83e26cd834cae277a7875fab8c484f6117
  02-requirement-completeness-assessment.md: 3de86c8242a313a5a864a1eaa1cfffdb21149658789f01095d0ec847a9c072d1
  03-domain-architecture.md: 3279c0ef2309b52feab77d870f782f4ce02134b2457fa46bfd09d868d98493de
input_hashes_method: |
  sha256 atas isi berkas dengan line ending LF — sama dengan isi blob yang disimpan Git.
  Perintah: tr -d '\r' < <berkas> | sha256sum
  Konvensi diambil dari pharmacy/blueprint-manifest.md dan billing-kasir/blueprint-manifest.md,
  dan diverifikasi cocok dengan keempat artifact_hashes pharmacy pada 2026-09-02.
  Nilai revision 14 sampai 17 keliru: hanya 16 digit heksadesimal dan tidak cocok dengan isi
  berkas mana pun. Seluruhnya diganti, bukan dipotong ulang.

requirement_gate_readiness: PARTIALLY_READY
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
domain_architecture_revision: LAB-DA-001-r4

contract_versions:            # seluruhnya dikunci 2026-09-02 oleh Yoga Aji Pratama selaku pemilik modul
  - LAB-API-v1: approved      # revision 3
  - LAB-STATE-v1: approved    # revision 2
  - LAB-VAL-v1: approved      # revision 3
  - LAB-INT-v1: approved      # revision 3
  - LAB-PERM-v1: approved     # revision 3
contract_lock_scope: |
  Penguncian mencakup seluruh isi kontrak KECUALI penamaan MstLabValueBound dan
  MstLabValueOption, yang masih menunggu LAB-OPEN-021. Task roadmap yang membuat kedua tabel
  itu tetap BLOCKED; task lain boleh berjalan paralel BE/FE.
  Input hash sudah dihitung ulang 2026-09-02 sebagai sha256 penuh; lihat input_hashes dan input_hashes_method.

evidence_baseline:
  source: Analisis_Konsolidasi_Modul_Laboratorium.md
  adopted_by: LAB-DEC-025 .. LAB-DEC-031
  adopted_at: 2026-09-01
  authority_level: bukti sistem berjalan dan analis — tingkat 4-7, di bawah keputusan pemilik
  limitation: audio video belum ditranskripsi; aturan yang hanya disampaikan lisan belum tercakup

active_blockers:
  - LAB-SIGN-001    # tanda tangan klinis — memblokir S4, S4b, S4c, S5, S6
  - LAB-AMD-001     # amandemen rawat-jalan — memblokir S1b
  - LAB-OPEN-018    # rules root terpasang tertinggal jauh: 13 dari 29 berkas, GLOBAL_RULES.md dan backend/engineering/ hilang — memblokir IMPLEMENTATION
  - LAB-OPEN-019    # lifecycle registry LaboratoryManagement masih PLANNED — QBE-MOD-002 menahan entity Lab*. Memblokir IMPLEMENTATION, bukan PLANNING
  - LAB-OPEN-020    # Invoke-QbeConformanceCheck.ps1 menunjuk agents/rules/engineering/ yang sudah dicabut — TOOL ERROR, gerbang QBE di CI mati. Pemilik: Andry Zain <andryzain01@gmail.com>
  - LAB-OPEN-021    # prefix data induk Lab* vs Mst* — dipisahkan dari LAB-OPEN-018 yang ID-nya sempat dipakai ganda
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
  not_covered: LAB-OPEN-012 memerlukan jawaban faktual; LAB-SIGN-001 memerlukan wewenang klinis. LAB-OPEN-002 ditutup 2026-09-01 oleh LAB-FACT-007, menurunkan LAB-OPEN-018 dan LAB-OPEN-019. Pemeriksaan 2026-09-02 menurunkan LAB-OPEN-020 (checker QBE) dan memisahkan LAB-OPEN-021 (prefix) dari LAB-OPEN-018

inherited_decisions:
  source: rawat-jalan / RJ-BIL-GATE-DEC-003
  status: locked-draft, tata kelola formal OPEN
  ids: [LAB-INH-001 .. LAB-INH-013]
```

## Daftar artefak

| Berkas | Ditulis oleh | Status |
|---|---|---|
| `blueprint-manifest.md` | `design-business-module` | rev 19 |
| `00-interview-decisions.md` | `grill-me` | rev 21 — **36 keputusan `approved`**; 5 koordinasi lintas modul ditutup |
| `01-existing-capability-map.md` | `trace-existing-capabilities` | rev 2 — impact scan 2026-09-02, `STALE` dicabut, tidak ada status kemampuan yang berubah |
| `02-requirement-completeness-assessment.md` | `requirement-completeness-gate` | rev 5 — `PARTIALLY_READY`, 10 dari 21 bagian siap |
| `03-domain-architecture.md` | `hospital-domain-architect` | rev 4 — **`DOMAIN_ARCHITECTURE_READY`** untuk 10 slice |
| `05-evidence-reconciliation.md` | `design-business-module` | rev 2 — `RECONCILED` |
| `02-backend-architecture.md` | `design-business-module` | rev 3 |
| `03-frontend-architecture.md` | `design-business-module` | rev 3 — menu data induk mengikuti konvensi FE yang sudah ada |
| `04-prd-to-mvp.md` | `design-business-module` | rev 2 — batas MVP diperluas, 10 epic, 5 gelombang |
| `erd/00-context-erd.md` | `design-business-module` | rev 2 — data induk perujuk dan kolom disiplin masuk |
| `erd/laboratory-operations.md` | `design-business-module` | rev 2 — lapis rujukan katalog, harga, cakupan |
| `erd/data-dictionary.md` | `design-business-module` | rev 2 — empat tabel milik modul lain didokumentasikan |
| `contracts/api-contract.md` | `design-business-module` | rev 3 — `approved`, dikunci 2026-09-02; 3 grup endpoint baru |
| `contracts/state-transition-matrix.md` | `design-business-module` | rev 2 — `approved`, dikunci 2026-09-02; sudah disesuaikan `LAB-DEC-026` |
| `contracts/validation-matrix.md` | `design-business-module` | rev 3 — `approved`, dikunci 2026-09-02; VAL-40 sampai VAL-50 |
| `contracts/integration-contract.md` | `design-business-module` | rev 3 — `approved`, dikunci 2026-09-02; `INT-05` dan `INT-06` |
| `contracts/permission-audit-matrix.md` | `design-business-module` | rev 3 — `approved`, dikunci 2026-09-02; 9 kewenangan baru |
| `testing/acceptance-test-matrix.md` | `design-business-module` | rev 3 — dua coverage gap ditutup: AC-11 alur lintas unit dan AC-19 batas reagen |
| `roadmap/backend-roadmap.md` | `plan-module-delivery` | rev 1 — `DRAFT`, 15 task Laboratorium + 3 task eksternal, 4 gelombang |
| `roadmap/frontend-roadmap.md` | `plan-module-delivery` | rev 1 — `DRAFT`, 9 task, dipasangkan ke gelombang backendnya |
| `roadmap/traceability.md` | `plan-module-delivery` | rev 1 — `DRAFT`, 45 FR dan 28 AC terpetakan; 2 coverage gap dicatat |

### Artefak operasional — di luar daftar hash

| Berkas | Sifat | Status |
|---|---|---|
| `approval-requests/2026-09-01-permintaan-koordinasi-lintas-modul.md` | Operasional, bukan artefak desain | **`dijawab sebagian`** — 6 selesai, 5 terbuka. Butir 9, 10, dan 11 diajukan 2026-09-02 |

## Catatan status

Blueprint **disetujui** pemilik modul pada 2026-09-01. Seluruh artefak desain tetap berstatus
`draft` karena cakupan berubah setelah bukti lapangan diadopsi. Tidak ada satu baris source
aplikasi yang diubah pada tahap ini.

### Artefak yang masih perlu disesuaikan

| Berkas | Yang belum masuk | Keadaan |
|---|---|---|
| `erd/00-context-erd.md` | Instansi dan dokter perujuk sebagai data induk milik `BC-MD`; kolom disiplin pada `MstProcedure` | ✅ **Selesai** — diverifikasi 2026-09-02: `MstReferralInstitution`, `MstReferralDoctor`, dan `LabDiscipline` sudah masuk beserta relasinya |
| `erd/laboratory-operations.md` | Relasi ke data induk perujuk | ✅ **Selesai** — diverifikasi 2026-09-02 |
| `erd/data-dictionary.md` | Kolom kunci data induk perujuk; kolom disiplin `MstProcedure` | ✅ **Selesai** — diverifikasi 2026-09-02, termasuk DDL bagian 9b |
| `contracts/state-transition-matrix.md` | Perpindahan status pendaftaran | Tidak ada lifecycle baru milik Laboratorium; kunjungan mengikuti lifecycle Registrasi |

### Yang menahan penerbitan roadmap — per 2026-09-02

`LAB-OPEN-019` **tidak** menahan roadmap; ia menahan implementasi. Ketiga penahan penerbitan
roadmap sudah dibereskan pada 2026-09-02:

| # | Penahan | Keadaan |
|---|---|---|
| ~~1~~ | ~~Kelima kontrak masih `draft`~~ | ✅ **Selesai.** Kelimanya dikunci `approved` oleh pemilik modul, diberi `Revision`, `approved_by`/`approved_at`, dan `Backend SHA` |
| ~~2~~ | ~~Kontrak tertinggal dari keputusan~~ | ✅ **Selesai.** `Input revision` diselaraskan ke `Decisions rev 20`; daftar artefak dikoreksi dari rev 18 ke rev 20. Isi kontrak diverifikasi sudah memuat perubahan revision 18, 19, dan 20 — tidak ada rujukan `Trx*` usang yang tersisa |
| ~~3~~ | ~~`capability_map` berstatus `STALE`~~ | ✅ **Selesai.** Impact scan dijalankan, `CAP-11` dan bagian utang teknis diverifikasi tidak berubah, `STALE` dicabut. Peta naik ke revision 2 |

**Roadmap sudah diterbitkan 2026-09-02** di `roadmap/` — `backend-roadmap.md`,
`frontend-roadmap.md`, dan `traceability.md`, seluruhnya revision 1 berstatus `DRAFT`. Ketentuan
yang berlaku padanya:

| Ketentuan | Alasan |
|---|---|
| Task yang membuat entity `Lab*` bertanda `BLOCKED` | `LAB-OPEN-019` — lifecycle registry masih `PLANNED` |
| Task yang membuat dua tabel batas nilai bertanda `BLOCKED` | `LAB-OPEN-021` — penamaan `Mst` atau `Lab` belum ditetapkan |
| Task implementasi backend mana pun tidak boleh dieksekusi | `LAB-OPEN-018` — rules root runtime belum lengkap; `AGENTS.md` memaksa `BLOCKED — canonical governance unavailable` |
| Migration pemisahan wadah tetap tertahan | `LAB-OPEN-012` — jumlah baris `TrxLabSpecimen` produksi belum diketahui |

### Utang pembukuan — ✅ seluruhnya ditutup 2026-09-02

| Butir | Keadaan |
|---|---|
| ~~`input_hashes` masih milik revisi lama~~ | ✅ **Ditutup.** Konvensinya bukan hash 16 digit melainkan **sha256 penuh atas isi ber-line-ending LF**, ditemukan dari `pharmacy` dan `billing-kasir`. Metodenya diuji lebih dulu terhadap keempat `artifact_hashes` pharmacy dan cocok persis, baru dipakai. Keempat hash Laboratorium dihitung ulang dan diverifikasi ulang setelah seluruh suntingan selesai |
| ~~`Riwayat Revisi` pada `00-interview-decisions.md` memuat baris revision 19 dua kali~~ | ✅ **Ditutup.** Kedua salinan digabungkan; keduanya sempat bertentangan soal lokasi canonical dan soal arti `LAB-OPEN-018`. Dicatat sebagai decisions revision 21 |

**Cara memeriksa ulang hash kapan pun:**

```bash
tr -d '\r' < 00-interview-decisions.md | sha256sum
```

Hasilnya wajib sama dengan nilai pada `input_hashes`. Bila berbeda, berkas masukan berubah dan
seluruh artefak turunannya — termasuk kontrak dan roadmap — menjadi stale.



Nomor awalan berkas sengaja berulang — `02-` dan `03-` masing-masing dipakai dua kali. Ini
mengikuti pola yang sudah dipakai modul `pharmacy` dan `operations`, di mana artefak gerbang
dan artefak desain hidup berdampingan.

## Pemicu perubahan revision

| Pemicu | Akibat |
|---|---|
| Backend bergerak dari `c87d9c0` | Impact scan `trace-existing-capabilities`; manifest naik revision |
| Frontend bergerak dari `688daff90` | Impact scan bagian frontend |
| Salah satu blocker aktif ditutup | Slice terkait masuk scope; seluruh artefak desain naik revision |
| `LAB-DEC-024` berubah | Arsitektur backend, ERD, dan kontrak wajib disusun ulang |
