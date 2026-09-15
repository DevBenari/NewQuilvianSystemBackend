# Blueprint Manifest — Platform

```yaml
blueprint_id: PLT-BP-001
module_name: Platform
module_slug: platform
module_area: Platform       # DEC-PLT-009 — Area baru
module_owner: NumberSeriesManagement / Number Series
module_category: SHARED PLATFORM CAPABILITY
module_prefix: Num          # DEC-PLT-010 — ditetapkan; baris registry belum dicatat (OQ-PLT-014)
blueprint_shape: SINGLE
shape_decided_by: USER_CONFIRMED
shape_decision_date: 2026-09-09
shape_test_score: "1 dari 5 — ambang COMPOSITE adalah 3"
revision: 3
revision_3_scope: >-
  Approval blueprint PLT-SLICE-01 dan set kontrak v1 turun 9 September 2026.
revision_2_scope: >-
  Amendment pass 9 September 2026 — DEC-PLT-009 (Area Platform) dan DEC-PLT-010 (prefix Num)
  menutup OQ-PLT-012 dan OQ-PLT-013. Nol arsitektur, kontrak, atau epic yang berubah.
status: APPROVED
current_phase: PLT-PH-001
created_at: 2026-09-09T00:00:00+07:00
updated_at: 2026-09-09T00:00:00+07:00
backend_source_sha: f0d6855
backend_branch: sukmagp
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
frontend_branch: sukmagpV2
decision_revision: 3
capability_map_revision: 3
completeness_assessment_revision: 2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
domain_architecture_skip_reason: >-
  Kemampuan infrastruktur lintas modul, bukan alur kerja rumah sakit. Nol bounded context
  klinis, nol dampak billing, nol dampak keselamatan pasien secara langsung.
  02-requirement-completeness-assessment.md sudah menetapkan pelewatan ini sejak revisi 2.
contract_version: v1 (approved)
owners:
  - "Pemilik kontrak engineering backend: Andry — mesin alokasi + INV-PLT-001..004"
  - "Pemilik modul konsumen: menetapkan awalan dan format deretnya masing-masing"
approved_by:
  - "Sukma Giri Pratama (sukmagp) — blueprint PLT-SLICE-01 dan set kontrak v1, 2026-09-09"
approved_at: "2026-09-09"
approval_note: >-
  Dua approval berbeda, sengaja dicatat terpisah. PERTAMA, Andry sebagai pemilik kontrak
  engineering backend menyetujui DEC-PLT-002..005, DEC-PLT-007..010 beserta INV-PLT-001..004
  pada 9 September 2026 — itu approval KEPUTUSAN, sesuai pembagian kewenangan DEC-PLT-005.
  KEDUA, Sukma Giri Pratama sebagai pemilik kebutuhan menyetujui BLUEPRINT PLT-SLICE-01
  beserta set kontrak v1 pada tanggal yang sama. Approval blueprint membuka plan-module-delivery;
  ia TIDAK membuka wewenang menulis source, dan TIDAK menggantikan OQ-PLT-014 yang masih
  menahan pembuatan model pertama lewat QBE-MOD-002.
```

---

## 1. Kenapa blueprint ini ada

Empat blueprint modul sudah merencanakan pemakaian nomor bisnis dari provider bersama —
`bank-darah`, `billing-kasir`, `rawat-inap`, dan `rawat-jalan` (`FACT-PLT-006`). Tidak satu pun
dapat berjalan karena providernya belum ada.

Yang memicu blueprint ini secara langsung: **gerbang `G4` modul Bank Darah**. Tiga nomor —
`OrderNumber`, `RequestNumber`, dan `ProcedureNumber` — wajib dialokasikan provider atomik, dan
`QBE-CODE-002/003` melarang `Count+1`/`Max+1` sebagai gantinya. Sembilan task backend Bank Darah
tertahan di sana.

---

## 2. Bentuk blueprint dan alasannya

`SINGLE`, ditetapkan pemilik pada 9 September 2026 setelah uji pemecahan memberi skor **1 dari 5**.

| Syarat `COMPOSITE` | Terpenuhi | Bukti |
| --- | :---: | --- |
| Bounded context sendiri yang dimiliki modul ini | ❌ | Keempat slice berputar pada satu konsep: alokasi nomor bisnis |
| Mesin status sendiri, kosakata tidak beririsan | ❌ | Deret nomor punya pencacah, bukan mesin status |
| Resource hak akses sendiri dan pemilik peran berbeda | ❌ | Satu pemilik untuk seluruh slice |
| Dapat dirilis sebagai gelombang MVP terpisah | ✅ | `PLT-SLICE-02` bergantung `PLT-SLICE-01` |
| Master data sendiri dan pemilik approval sendiri | ❌ | Satu tabel deret, satu pemilik approval |

Keempat slice adalah **gelombang pengiriman**, bukan rumpun kepemilikan. Aturan bentuk melarang
`COMPOSITE` dipilih atas dasar itu.

---

## 3. Berkas blueprint

| Berkas | Keadaan |
| --- | --- |
| `blueprint-manifest.md` | **Baru** — berkas ini |
| `00-interview-decisions.md` | Sudah ada, revisi 3 |
| `02-existing-capability-map.md` | Sudah ada, revisi 3 |
| `02-requirement-completeness-assessment.md` | Sudah ada, revisi 2 — bukan bagian 14 berkas kontrak; artefak `requirement-completeness-gate` |
| `02-backend-architecture.md` | **Baru** |
| `03-frontend-architecture.md` | **Baru** |
| `04-prd-to-mvp.md` | **Baru** |
| `flowcharts/00-alur-utama.md` | **Baru** |
| `flowcharts/alokasi-nomor.md` | **Baru** |
| `data/data-dictionary.md` | **Baru** |
| `contracts/api-contract.md` | **Baru** |
| `contracts/state-transition-matrix.md` | **Baru** |
| `contracts/validation-matrix.md` | **Baru** |
| `contracts/integration-contract.md` | **Baru** |
| `contracts/permission-audit-matrix.md` | **Baru** |
| `testing/acceptance-test-matrix.md` | **Baru** |

**Selisih penamaan yang dilaporkan, bukan diperbaiki diam-diam.** Kontrak output menyebut
`01-existing-capability-map.md`; repository ini memakai `02-existing-capability-map.md` pada
seluruh modul, termasuk `bank-darah`. Nama existing dipertahankan supaya rujukan dari
`02-requirement-completeness-assessment.md` tidak putus.

---

## 4. Prasyarat yang belum terpenuhi

| Prasyarat | Keadaan | Akibat |
| --- | --- | --- |
| **Entri registry modul Platform** | **Isinya sudah diputuskan** 9 September 2026 — `DEC-PLT-009` (Area `Platform`) dan `DEC-PLT-010` (prefix `Num`). **Barisnya belum dicatat** ke `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, dan lifecycle belum `ACTIVE` (`OQ-PLT-014`) | **Implementasi tetap `BLOCKED BY QBE-MOD-002`.** Perencanaan **tidak** terblokir. Model pertama tidak boleh dibuat sebelum barisnya ada dan `ACTIVE`. Baris siap-salin ada di `02-backend-architecture.md` §A.3 |
| Approval blueprint | ✅ **Turun 9 September 2026** oleh `Sukma Giri Pratama` (`sukmagp`) | `plan-module-delivery` **terbuka**. Approval ini **tidak** membuka wewenang menulis source |

---

## 5. Keputusan yang menjadi dasar

| ID | Isi ringkas | Status |
| --- | --- | --- |
| `DEC-PLT-001` | Scope dikunci pada alokasi nomor bisnis | ✅ `approved` |
| `DEC-PLT-002` | Nomor tidak pernah dipakai ulang | ✅ `approved` |
| `DEC-PLT-003` | Alokator baru dipakai kode baru sejak hari pertama; titik lama pindah bertahap | ✅ `approved` |
| `DEC-PLT-004` | Deret berjalan terus, tidak pernah diulang | ✅ `approved` |
| `DEC-PLT-005` | Kewenangan dibagi dua: mesin milik platform, format milik modul | ✅ `approved` |
| `DEC-PLT-007` | Mesin diekstrak dari `BillingNumberSeriesService`, bukan dibangun dari nol | ✅ `approved` |
| `DEC-PLT-008` | Nomor hangus bila transaksi bisnis dibatalkan | ✅ `approved` |
| `DEC-PLT-009` | Area `Platform` baru · `NumberSeriesManagement / Number Series` · `SHARED PLATFORM CAPABILITY` | ✅ `approved` |
| `DEC-PLT-010` | Prefix `Num` = *Number Series* | ✅ `approved` |
| `INV-PLT-001`..`004` | Invariant lintas modul | ✅ `approved` |
| `OQ-PLT-014` | Baris registry `Platform`/`Num` dicatat dan `ACTIVE` | ⛔ Terbuka — **menahan implementasi**, isi gelombang `MVP-0` |
| `DEC-PLT-006` | Penerimaan pelanggaran `INV-PLT-001` selama peralihan | ⛔ Terbuka — milik `PLT-SLICE-02` |
| `OQ-PLT-005` | Panjang nomor dan deret yang hampir habis | ⛔ Terbuka — `IMPLEMENTATION` |
| `OQ-PLT-006` | Kode fasilitas di dalam awalan | ⛔ Terbuka — `LATER SLICE` |
| `OQ-PLT-009` | Penelusuran nomor kembar yang mungkin sudah terbit | ⛔ Terbuka — `PLT-SLICE-04` |

Keempat yang terbuka **tidak** menahan `PLT-SLICE-01`.
