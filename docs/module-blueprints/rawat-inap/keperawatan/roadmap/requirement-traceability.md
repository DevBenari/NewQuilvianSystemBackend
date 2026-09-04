# Requirement Traceability — Sub-modul Keperawatan Rawat Inap

## Metadata

```yaml
blueprint_id: RWI-BP-001
blueprint_revision: 5
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
traceability_revision: 1
status: DRAFT
approval_gate: BLUEPRINT_NOT_YET_APPROVED
contract_versions: 0.2.0
backend_source_sha: 93b3227c431401d8f586dec4e1fb25fbf41766e3
frontend_source_sha: 8d6e0998c16d60e19a9f43949758e8895d5c47d0
requirement_readiness: INP-S16 PARTIALLY_READY
requirement_gate_revision: 1.4
```

---

## 0. Apa yang dijaga dokumen ini

Pada bentuk `COMPOSITE`, berkas traceability menjadi milik **masing-masing sub-modul**, padahal
requirement lahir dari **satu** wawancara di tingkat modul. Akibatnya sudah dicatat
`bentuk-blueprint.md` bagian 5: kemampuan yang tidak ditugaskan ke sub-modul mana pun **tidak
diperiksa siapa pun**.

Dokumen ini menjaga jatah `keperawatan` saja: lima kemampuan sesuai `RWI-DEC-083`. Pemeriksaan
kemampuan yatim untuk seluruh 28 kemampuan tetap dipegang
[`../../02-module-map.md`](../../02-module-map.md) bagian 4.

---

## 1. Kemampuan ke epic ke task

| Kemampuan | Epic | Requirement | Task backend | Task frontend | Status |
| --- | --- | --- | --- | --- | --- |
| `CAP-012` Nursing Assessment | `EPIC KEP-01` | `FR-KEP-001` s.d. `FR-KEP-004` | `BE-RWI-054` | `FE-RWI-051` | ⛔ `BLOCKED` |
| `CAP-012` Nursing Assessment | `EPIC KEP-02` | `FR-KEP-005` s.d. `FR-KEP-011` | `BE-RWI-055` s.d. `BE-RWI-058` | `FE-RWI-052`, `FE-RWI-053` | ⛔ `BLOCKED` |
| `CAP-013` Nursing Care | `EPIC KEP-03` | `FR-KEP-012` s.d. `FR-KEP-017` | `BE-RWI-059`, `BE-RWI-060` | `FE-RWI-054` | ⛔ `BLOCKED` |
| `CAP-014` Nursing Interventions | `EPIC KEP-04` | `FR-KEP-018` s.d. `FR-KEP-023` | `BE-RWI-061` s.d. `BE-RWI-063` | `FE-RWI-055` | ⛔ `BLOCKED` |
| `CAP-012` kepatuhan | `EPIC KEP-05` | `FR-KEP-024` s.d. `FR-KEP-026` | `BE-RWI-064` | `FE-RWI-056` | ⛔ `BLOCKED` |
| `CAP-016` Equipment Usage | `EPIC KEP-06` | `FR-KEP-027`, `FR-KEP-028` | **Nol task, disengaja** | **Nol task, disengaja** | `DEFERRED` |
| `CAP-027` Nutrition Care | — | Skrining ikut `EPIC KEP-01`/`KEP-02`; rujukan `INT-KEP-04` | Sebagian `BE-RWI-056` | Sebagian `FE-RWI-052` | Sebagian; sisanya menunggu modul Gizi |

**Nol kemampuan tanpa epic pemilik.** `CAP-016` punya epic dan punya sub-modul pemilik; ia
`DEFERRED`, bukan yatim — perbedaan itu diuji `RWI-AC-169`.

---

## 2. Requirement ke keputusan ke kontrak ke bukti

| Requirement | Keputusan / aturan | Kontrak | Acceptance test | Task |
| --- | --- | --- | --- | --- |
| `FR-KEP-001` | `RWI-DEC-062`, `RWI-RULE-026`; PRD 16.2 aturan 2 | Integration `0.2.0` `INT-KEP-01` | `AC-CAP012-01` | `BE-RWI-054` |
| `FR-KEP-002` | PRD 16.2 aturan 1 | Validation `0.2.0` | Jalur gagal episode tidak sah | `BE-RWI-054` |
| `FR-KEP-003` | `RWI-DEC-051` | — | Test regresi poliklinik dan IGD | `BE-RWI-054` |
| `FR-KEP-004` | `RWI-DEC-081`; PRD 16.2 aturan 1 | API `0.2.0` | Pengkajian terbaca per episode | `BE-RWI-054` |
| `FR-KEP-005` | PRD 16.2 aturan 3 | State transition `0.2.0` bagian 1 | `AC-CAP012-02` | `BE-RWI-056` |
| `FR-KEP-006` | PRD 16.2 aturan 3 | Validation `0.2.0` | Jalur gagal pengkajian awal kedua | `BE-RWI-056`, `FE-RWI-052` |
| `FR-KEP-007` | PRD 16.2 aturan 6 | API `0.2.0` timeline | `AC-CAP012-02` | `BE-RWI-058`, `FE-RWI-053` |
| `FR-KEP-008` | PRD 16.2 aturan 13, 27.3 aturan 7 | API `0.2.0` amend; State transition `0.2.0` | `AC-CAP012-05` | `BE-RWI-057`, `FE-RWI-052` |
| `FR-KEP-009` | PRD 16.2 aturan 12 | State transition `0.2.0` transisi tidak sah | Percobaan hard-delete ditolak | `BE-RWI-057` |
| `FR-KEP-010` | `RWI-RULE-021`; PRD 16.2 aturan 11 | Validation `0.2.0` | `AC-CAP012-04` | `BE-RWI-055`, `BE-RWI-058` |
| `FR-KEP-011` | PRD 16.2 aturan 11 | Validation `0.2.0` | Skenario master kosong | `BE-RWI-055`, `FE-RWI-053` |
| `FR-KEP-012` | PRD `CAP-013` aturan 1; `RWI-DEC-083` | API `0.2.0` Nursing Care Plan | `AC-CAP013-01` | `BE-RWI-059` |
| `FR-KEP-013` | PRD `CAP-013` aturan 2 | API `0.2.0` | `AC-CAP013-01` | `BE-RWI-059`, `FE-RWI-054` |
| `FR-KEP-014` | PRD `CAP-013` aturan 5 | API `0.2.0` revisions | `AC-CAP013-02` | `BE-RWI-060`, `FE-RWI-054` |
| `FR-KEP-015` | PRD `CAP-013` aturan 2 | State transition `0.2.0` bagian 2 | Tutup butir tanpa evaluasi ditolak | `BE-RWI-059`, `FE-RWI-054` |
| `FR-KEP-016` | PRD `CAP-013` aturan 6 | State transition `0.2.0` bagian 2 | `AC-CAP013-03` | `BE-RWI-060` |
| `FR-KEP-017` | `INV-KEP-02`; PRD `CAP-013` AC-03 | State transition `0.2.0` | `AC-CAP013-03` | `BE-RWI-060`, `FE-RWI-054` |
| `FR-KEP-018` | PRD `CAP-014` aturan 1, 2 | API `0.2.0` Nursing Intervention | Tindakan tersimpan lengkap | `BE-RWI-061`, `FE-RWI-055` |
| `FR-KEP-019` | PRD `CAP-014` aturan 3 | API `0.2.0` | Tindakan mendadak tanpa rencana | `BE-RWI-061`, `FE-RWI-055` |
| `FR-KEP-020` | PRD `CAP-014` aturan 1 | API `0.2.0` `Idempotency-Key` | `AC-CAP014-01` | `BE-RWI-061`, `FE-RWI-055` |
| `FR-KEP-021` | PRD `CAP-014` aturan 5 | Integration `0.2.0` `INT-KEP-05` | `AC-CAP014-02` | `BE-RWI-062`, `FE-RWI-055` |
| `FR-KEP-022` | PRD `CAP-014` AC-03 | Permission `0.1.0`; State transition `0.2.0` bagian 3 | `AC-CAP014-03` | `BE-RWI-062`, `FE-RWI-055` |
| `FR-KEP-023` | PRD `CAP-014` aturan 4; `RWI-RULE-026` | Integration `0.2.0` `INT-KEP-03` | Nol tabel baru | `BE-RWI-063` |
| `FR-KEP-024` | `RWI-RULE-023`, `RWI-DEC-032` | API `0.2.0` | Daftar memuat episode terlambat | `BE-RWI-064`, `FE-RWI-056` |
| `FR-KEP-025` | `RWI-DEC-032` | — | Daftar kosong berbunyi benar | `FE-RWI-056` |
| `FR-KEP-026` | PRD 16.2 aturan 11 | — | Keterlambatan tidak menahan tindakan | `BE-RWI-064` |
| `FR-KEP-027` | `RWI-DEC-089` | — | **Tidak diuji** | **Nol task** |
| `FR-KEP-028` | `RWI-DEC-089` | — | **Tidak diuji** | **Nol task** |

---

## 3. Coverage gap yang diakui

| Gap | Sebabnya | Kapan tertutup |
| --- | --- | --- |
| `FR-KEP-027`, `FR-KEP-028` tanpa task dan tanpa test | `RWI-DEC-089` mengeluarkan `EPIC KEP-06` dari scope rilis pertama secara tertulis | Setelah modul persediaan/aset ada dan `RWI-OQ-048` dibuka kembali — `RWI-AC-171` |
| `AC-CAP027-01` rujukan gizi hanya sebagian | Modul Gizi berstatus `PLANNED`; PRD 23.1 menaruh Nutrition Assessment/Care di sana | Setelah modul Gizi berdiri |
| `AC-CAP027-02` kewenangan ahli gizi | Sama seperti di atas; di luar kendali sub-modul ini | Setelah modul Gizi berdiri |
| Nilai batas waktu klinis belum ada | `RWI-RULE-021` menunggu pemilik klinis; PRD 16.2 aturan 11 menjadikannya konfigurasi | Mekanismenya tetap diuji sekarang; angkanya menyusul tanpa mengubah kode |
| Katalog SDKI/SLKI/SIKI | PRD `CAP-013` aturan 3 bersyarat; pemakaiannya belum dinyatakan | Setelah Clinical governance menyatakan pemakaiannya |

Kelima gap ini **tercatat**, bukan tersembunyi. Tidak satu pun dari kelimanya menahan 26
functional requirement yang aktif.

---

## 4. Keputusan yang masih terbuka dan pengaruhnya

| Butir | Status | Pengaruh pada roadmap ini |
| --- | --- | --- |
| **Approval blueprint `keperawatan`** | **Belum ada** | **Menahan seluruh 17 task.** Ini gerbang tunggal yang paling menentukan |
| `INT-KEP-01` resolver konteks klinis | Gap **teknis**, milik `ClinicalManagement` | Menahan `BE-RWI-054`, dan lewat itu seluruh task lain |
| Butir konsistensi mesin koreksi, `04-prd-to-mvp.md` bagian 20.1 | `PROPOSED` / `NON_BLOCKING_STANDARD` | Tidak menahan. Bila dijawab berbeda, `BE-RWI-057` dan `FE-RWI-052` berubah bentuk |
| `DEC-INP-009` supersession `RWI-DEC-004` dan `RWI-DEC-034` | `OPEN`, non-blocking | Bila pemilik justru menegaskan `CAP-013` tetap di luar scope, `EPIC KEP-03` dicabut beserta `BE-RWI-059`, `BE-RWI-060`, dan `FE-RWI-054` |
| `RWI-RULE-021` batas waktu klinis | Belum final | Tidak menahan desain maupun pembangunan; menahan **produksi** |
| Urutan daftar di dalam `FE-INP-09` | Belum ditetapkan | Menyentuh `FE-RWI-056`; wajib diputuskan bersama, bukan sendiri-sendiri |

---

## 5. Ringkasan hitungan

| Hal | Jumlah |
| --- | ---: |
| Kemampuan dalam scope sub-modul | 5 |
| Kemampuan aktif | 4 |
| Kemampuan `DEFERRED` | 1 |
| Epic aktif | 5 |
| Epic `DEFERRED` | 1 |
| Functional requirement aktif | 26 |
| Functional requirement `DEFERRED` | 2 |
| Task backend | 11 |
| Task frontend | 6 |
| Task berstatus `BLOCKED` hari ini | **17 dari 17** |
| Tabel baru milik Rawat Inap | **0** |
| Butir menu baru | **0** |
