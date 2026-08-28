# Roadmap Delivery Backend — Modul Rawat Jalan Billing

## Metadata

```yaml
blueprint_id: RJ-BIL-BP-001
module_name: Dokter / Rawat Jalan Billing
module_slug: rawat-jalan
module_prefix: RJ-BIL
roadmap_revision: 1
input_revisions:
  blueprint-manifest.md: 18
  00-interview-decisions.md: 13
status: APPROVED_FOR_EXECUTION
approval_gate: OWNER_APPROVED
scope: Core internal/manual
owners:
  - "Product/Domain: Sukma Giri; juga Product/Domain Owner LaboratoryManagement lewat RJ-BIL-DEC-007"
  - "Billing/Revenue Cycle: OPEN"
  - "API authority: OPEN"
  - "Security/Privacy: OPEN"
  - "Radiology: BELUM DITUNJUK"
approved_by:
  - "User-provided approval authority — RJ-BIL-BE-001 s.d. RJ-BIL-BE-009 pada 2026-08-21"
approved_at: "2026-08-21"
domain_architecture:
  revision: 1
  readiness: "DOMAIN_ARCHITECTURE_PARTIAL — core internal/manual siap independen"
contract_versions:
  - "RJ-BIL-CONTRACT-001@1.0.0 (OWNER_APPROVED)"
source_commits:
  backend: "6b25e6049e60e055593968abe463262b59842527 cabang sukmagp"
  frontend: "29422c83eaf6fd231cbb72f2ba04e306367934e1 cabang QuilvianDevV2"
working_tree_state: "RJ-BIL-BE-003, RJ-BIL-BE-006, RJ-BIL-BE-007, dan remediasi penamaan QBE belum di-commit"
implementation_authority:
  granted: [RJ-BIL-BE-001, RJ-BIL-BE-002, RJ-BIL-BE-003, RJ-BIL-BE-006, RJ-BIL-BE-007]
builder_execution:
  executed: [RJ-BIL-BE-001, RJ-BIL-BE-002, RJ-BIL-BE-003, RJ-BIL-BE-006, RJ-BIL-BE-007]
  not_authorized: [RJ-BIL-BE-004, RJ-BIL-BE-005, RJ-BIL-BE-008, RJ-BIL-BE-009]
external_adapter: "RJ-BIL-DEP-009 = INACTIVE / OUT OF CURRENT DELIVERY SCOPE"
task_count: 9
progress: "5 dari 9 task backend selesai per 2026-08-27"
test_evidence: "157 test lulus, 0 gagal — dijalankan 2026-08-27 terhadap QuilvianNewDevTim01 di bawah RJ-BIL-DEC-009"
last_status_update: "2026-08-27 — RJ-BIL-DEC-011 dan RJ-BIL-DEC-012 diambil, RJ-BIL-BE-006 dibangun, diuji, dan migration-nya diterapkan ke database pengembangan"
```

---

## 0. Peringatan yang tidak boleh dilewati

> **Roadmap ini berstatus `APPROVED_FOR_EXECUTION` sejak 2026-08-21.** Kesembilan task
> `RJ-BIL-BE-001` s.d. `RJ-BIL-BE-009` sudah disetujui pemilik pekerjaan.
>
> Approval itu **bukan** izin menulis code. Setiap task tetap memerlukan tiga hal terpisah pada
> waktu eksekusi: handoff task, wewenang tulis backend, dan QBE preflight. Tidak satu pun task di
> bawah memberi izin menerapkan migration, memutasi database, melakukan deployment, commit,
> maupun publish.
>
> **Roadmap revision tidak dinaikkan.** Pembaruan `2026-08-27` dan `2026-08-28` hanya menyentuh
> status eksekusi dan bentuk penyajian — tidak menyentuh cakupan, acceptance criteria, maupun
> dependency task mana pun.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya | Syarat |
| :---: | --- | --- |
| ✅ | **SELESAI** | Code sudah dibuat, build lulus, dan test acceptance-nya lulus dengan bukti tercatat |
| 🟡 | **KODE SIAP, BELUM DI-BUILD** | Code sudah ditulis, tetapi build dan test belum dijalankan — sehingga belum ada bukti apa pun bahwa code itu benar-benar berjalan |
| ⛔ | **TERBLOKIR** | Tidak dapat dikerjakan sebelum satu keputusan, satu penunjukan owner, atau task pendahulunya selesai |

> **Tanda ini menilai keadaan build, bukan restu governance.** Sebuah task dapat bertanda ✅
> sementara governance sign-off-nya masih `OPEN`. Baris **Governance** pada tiap task di bagian 4
> menjaga perbedaan itu tetap terbaca. ✅ **tidak pernah** berarti boleh deploy.

**Keadaan per 28 Agustus 2026**, diverifikasi ulang terhadap source dan database pada tanggal itu.

| Hal | Keadaannya |
| --- | --- |
| Task selesai | **5 dari 9** — `RJ-BIL-BE-001`, `002`, `003`, `006`, `007` |
| Task terblokir | **4 dari 9** — `RJ-BIL-BE-004`, `005`, `008`, `009` |
| Task bertanda 🟡 | **Tidak ada.** Seluruh code yang ada sudah pernah di-build; buktinya pada bagian 5 |
| Test backend | `157` lulus, `0` gagal |
| Migration `(Pending)` | `0` |
| Governance sign-off | `RJ-BIL-BE-003`, `RJ-BIL-BE-006`, dan `RJ-BIL-BE-007` masih `OPEN` |

> **Yang menahan modul ini hari ini bukan kode, melainkan keputusan dan penunjukan owner.**
> Keempat task yang terblokir tidak satu pun terhenti karena kesulitan teknis. Satu menunggu
> jawaban pemilik atas `RJ-BIL-CONFLICT-001`, satu menunggu penunjukan owner Radiologi, dan dua
> menunggu pendahulunya. Menambah tenaga programmer tidak memindahkan satu pun dari keempatnya.

---

## 1. Cara membaca roadmap ini

Pekerjaan dipecah menjadi **slice**, bukan lapisan teknis. Satu slice adalah satu hasil yang dapat
dirasakan petugas dan dapat diperiksa benar atau salahnya. "Pemeriksaan lab yang ditolak tidak
ikut tertagih" adalah slice; "buat semua model" bukan.

Setiap task memakai ID tetap `RJ-BIL-BE-nnn`. ID tidak pernah dipakai ulang walaupun task
dibatalkan, supaya rujukan pada laporan lama tidak berubah arti.

Istilah yang dipakai berulang:

| Istilah | Arti |
| --- | --- |
| *Folio* | Wadah tagihan satu kunjungan. Satu `EncounterId` hanya boleh punya satu folio |
| *Clinical fact* | Kabar dari modul klinis bahwa sesuatu benar-benar terjadi — obat diserahkan, tindakan dikerjakan, sampel diterima. Ia **tidak** menyatakan akibat finansialnya |
| *Milestone* | Titik dalam alur klinis yang menjadi syarat sebuah tagihan boleh lahir. Sebelum milestone tercapai, tidak ada charge |
| *`OutcomeUnknown`* | Hasil pemrosesan belum dapat dipastikan. **Bukan** gagal dan **bukan** berhasil. Perbedaan ini mengikat sampai ke layar |
| *Replay* | Permintaan yang sama dikirim ulang dan **tidak** menghasilkan tagihan kedua |
| *Allocation* | Pembagian satu tagihan ke beberapa penanggung beserta sisa yang ditanggung pasien |
| *Maker-checker* | Yang mengajukan dan yang menyetujui wajib orang berbeda. Memegang kedua izin sekaligus tidak memberi hak menyetujui permintaan sendiri |

---

## 2. Keadaan awal yang menentukan urutan

| Fakta | Bukti | Akibat pada urutan |
| --- | --- | --- |
| Payment source lama pada Encounter berbentuk **satu kunjungan satu penanggung** | `RJ-BIL-DEP-001`; `RJ-BIL-CONFLICT-001` | `RJ-BIL-BE-005` tidak dapat dirancang sebelum pemiliknya memutuskan. Ini gerbang keputusan, bukan gerbang teknis |
| Modul klinis pernah memegang kewenangan finansial | `RJ-BIL-CONFLICT-006`; `RJ-BIL-DEP-002` | `RJ-BIL-BE-002` mencabutnya lebih dulu, sebelum ada task lain yang bergantung pada angka tagihan |
| Tidak ada satu pun capability Radiologi operasional pada source | `RJ-BIL-DEP-005` berstatus `MISSING`; registry `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `19` | `RJ-BIL-BE-004` adalah greenfield penuh dan **tidak boleh dimulai** sebelum owner `RadiologyManagement` ditunjuk dan prefix `Rad` naik `PLANNED` → `ACTIVE` |
| Tidak ada orchestration maupun claim source penanggung pada snapshot | `RJ-BIL-DEP-007` berstatus `MISSING` | `RJ-BIL-BE-008` hanya boleh berbentuk alur manual internal, berlabel `ManualOperator` |
| Tidak ditemukan test project backend maupun spec frontend relevan pada snapshot audit | `RJ-BIL-CAP-021` berstatus `Missing` | Test bukan pekerjaan terpisah di akhir. Ia menempel pada tiap task, dan `RJ-BIL-BE-009` hanya menutup sisanya |
| Adapter payer eksternal belum punya kontrak, kredensial, sandbox/UAT, maupun bukti rekonsiliasi | `RJ-BIL-DEP-009` berstatus `UNKNOWN` | Adapter tetap `INACTIVE`. Ia **tidak** memblokir modul; ia hanya memblokir aktivasi eksternal |

Fakta pertama yang paling mudah terlewat, jadi akibatnya ditulis di sini:

> **`RJ-BIL-CONFLICT-001` berstatus `CONFIRMED` dengan source confidence `HIGH`** sejak audit
> read-only `2026-08-24`, dan **tidak memerlukan perubahan code saat ini**. Yang belum ada adalah
> jawabannya. Selama `RJ-BIL-OQ-001`, `OQ-002`, dan `OQ-005` belum dijawab, bentuk allocation
> belum dapat dirancang — dan karena `RJ-BIL-BE-008` bekerja di atas hasil allocation, ia ikut
> berhenti. Pertanyaannya sudah tersusun siap jawab pada
> [owner-decision-request-RJ-BIL-001.md](../owner-decision-request-RJ-BIL-001.md).

---

## 3. Slice dan milestone

| Slice | Hasil yang dapat diperiksa | Task |
| --- | --- | --- |
| **S0 — Tagihan punya wadah dan penjaga** | Folio unik per kunjungan; permintaan kembar tidak menghasilkan tagihan kedua | ✅ `RJ-BIL-BE-001` |
| **S1 — Fakta klinis masuk tanpa modul klinis ikut menetapkan uang** | Resep dan tindakan menerbitkan fakta; Billing yang menentukan akibat finansialnya | ✅ `RJ-BIL-BE-002` |
| **S2 — Penunjang menagih hanya yang benar-benar dikerjakan** | Lab menagih sejak `Accepted`; komponen yang ditolak tidak ikut tertagih. Radiologi menyusul | ✅ `RJ-BIL-BE-003`; ⛔ `RJ-BIL-BE-004` |
| **S3 — Kegagalan pemrosesan terlihat dan dapat dipulihkan** | Timeout tidak menggandakan tagihan; komponen gagal terlihat; folio tidak dapat ditutup selagi case terbuka | ✅ `RJ-BIL-BE-007` |
| **S4 — Satu tagihan dapat dibagi ke beberapa penanggung** | Rp1.000.000 dapat menjadi A Rp600.000 + B Rp250.000 + pasien Rp150.000, dan versi lamanya tetap terbaca | ⛔ `RJ-BIL-BE-005` |
| **S5 — Tindakan finansial punya persetujuan** | Void, koreksi, refund, gratis, dan hapus buku melewati maker-checker; pengaju tidak dapat menyetujui sendiri | ✅ `RJ-BIL-BE-006` |
| **S6 — Klaim dan settlement manual per penanggung** | Klaim yang disetujui tetap `PaymentPending`; adapter eksternal tetap mati | ⛔ `RJ-BIL-BE-008` |
| **S7 — Kesiapan sebelum sign-off** | Setiap acceptance criteria kritis punya bukti test atau pemilik gap-nya | ⛔ `RJ-BIL-BE-009` |

### Urutan dependency

```text
RJ-BIL-BE-001 (folio + charge line + component + processing effect)  ✅ SELESAI
   ├── RJ-BIL-BE-002 (clinical fact handoff resep & tindakan)   ✅ SELESAI
   ├── RJ-BIL-BE-003 (milestone Lab sampai Accepted)            ✅ SELESAI
   ├── RJ-BIL-BE-004 (boundary Radiologi + acquisition fact)    ⛔ owner RadiologyManagement belum ditunjuk
   ├── RJ-BIL-BE-007 (reconciliation case + recovery status)    ✅ SELESAI
   │      └── RJ-BIL-BE-006 (financial action + approval + close/reopen)  ✅ SELESAI
   └── RJ-BIL-BE-005 (allocation multi-payer + patient responsibility)
          │                                                     ⛔ menunggu RJ-BIL-CONFLICT-001
          └── RJ-BIL-BE-008 (payer/claim/settlement manual internal)   ⛔ menunggu BE-005

RJ-BIL-BE-009 (menutup coverage gap automated verification) — paling akhir, menunggu BE-001 s.d. BE-008
```

**Yang boleh paralel.** Setelah `RJ-BIL-BE-001` selesai, empat jalur tidak saling bergantung:
`BE-002`, `BE-003`, `BE-004`, dan `BE-007`. `RJ-BIL-DEP-009` tidak termasuk sequence mana pun dan
tetap `INACTIVE`.

> **Koreksi `2026-08-26` oleh `RJ-BIL-DEC-008`.** Revisi sebelumnya menuliskan
> `... → BE-005 → BE-006/BE-007 → ...`, yang bertentangan dengan baris **Dependency** pada task:
> `BE-007` di sana hanya bergantung pada `BE-001` dan Integration owner. Karena `BE-005` terblokir
> menunggu `RJ-BIL-OQ-001` s.d. `OQ-007`, urutan lama menghentikan **seluruh** backend tanpa satu
> pun alasan teknis.
>
> Dua bukti menentukan koreksi ini. Pertama, acceptance criteria `BE-006` berbunyi *"close ditolak
> saat reconciliation pending"* — jadi `BE-006` justru bergantung pada `BE-007`, bukan sebaliknya.
> Kedua, rekonsiliasi `BE-007` bekerja pada outcome pemrosesan fakta klinis melalui
> `BilProcessingEffect` dan tidak menyentuh alokasi multi-payer yang menjadi isi `BE-005`.
>
> Yang mengikat adalah baris **Dependency** pada tiap task. Diagram di atas adalah ringkasannya,
> bukan sumber kebenaran yang terpisah.

---

## 4. Task

### ✅ `RJ-BIL-BE-001` — Tagihan satu kunjungan punya wadah tunggal dan penjaganya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI.** Keempat acceptance criteria terbukti melalui `10` automated test yang lulus. Empat tabel `BilFolio`, `BilChargeLine`, `BilChargeComponent`, dan `BilProcessingEffect` beserta delapan index sudah diterapkan ke database pengembangan `QuilvianNewDevTim01` |
| **Governance** | — |
| **Outcome** | Menetapkan baseline dan memperkeras Billing Folio/Charge Operational, sehingga satu kunjungan hanya punya satu wadah tagihan dan permintaan yang terkirim dua kali tidak menghasilkan tagihan kedua |
| **Trace** | `RJ-BIL-GATE-DEC-001`, `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-012`, `RJ-BIL-CAP-017` |
| **Kontrak** | API/State/Validation `1.0.0` |
| **Reuse** | `BilFolio`, `BilChargeLine`, `BilChargeComponent`, dan `BilProcessingEffect` pada working tree `BillingManagement/Operational/**` |
| **Scope** | Preflight QBE, configuration path, `DbContext`, unique/index/concurrency, API validation, audit, dan rencana migration. **Tidak** menjalankan migration |
| **Dependency** | `RJ-BIL-DEP-001`, `RJ-BIL-DEP-006`, `RJ-BIL-DEP-008` |
| **Acceptance criteria** | 1. Folio unik per encounter. 2. Duplicate key menghasilkan replay, bukan tagihan kedua. 3. Stale version ditolak. 4. Tidak ada satu pun clinical financial mutation |
| **Verifikasi** | Build backend; targeted integration test; migration review; permission review |
| **Risiko/pemilik** | Working tree belum di-commit, sehingga bukti bersifat provisional sampai commit dilakukan. Owner: Backend |
| **DoD** | Bukti source, build, dan test lengkap; artefak migration ditinjau; tidak ada penerapan database tanpa wewenang |
| **Bukti** | [execution-evidence-RJ-BIL-BE-001.md](../execution-evidence-RJ-BIL-BE-001.md) |

---

### ✅ `RJ-BIL-BE-002` — Modul klinis berhenti menetapkan uang

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI.** Terbukti melalui `22` automated test yang lulus. `RJ-BIL-CONFLICT-006` ditutup: empat endpoint finansial dan lima method workflow-nya **dihapus dari source**, bukan sekadar disembunyikan dari Swagger. Pembatalan resep tetap ada sebagai alur klinis, tetapi tidak lagi menulis status pembayaran |
| **Governance** | `RJ-BIL-BE-002-BLOCKER-001` **terbuka** — pintu masuk telaah farmasi setelah kewenangan finansial klinis dihapus. Blocker ini **tidak** menahan acceptance criteria task ini dan **tidak** menahan task berikutnya |
| **Outcome** | Menyediakan clinical fact handoff yang idempotent untuk resep dan tindakan. Modul klinis kini hanya menerbitkan fakta melalui `BilClinicalMilestoneFact`, dan Billing yang menentukan akibat finansialnya |
| **Trace** | `RJ-BIL-GATE-DEC-001`, `005`, `008`; `RJ-BIL-CAP-005`, `RJ-BIL-CAP-008` |
| **Kontrak** | Integration `RJ-BIL-INT-001@1.0.0` |
| **Reuse** | Lifecycle resep dan tindakan yang sudah ada |
| **Scope** | Adapter penerbit fakta; source dan version yang stabil; pemetaan milestone; kontrak retry/outcome; pencabutan bertahap kewenangan finansial legacy |
| **Dependency** | `RJ-BIL-BE-001`; Pharmacy; Clinical |
| **Acceptance criteria** | 1. Clinical endpoint tidak menetapkan `Paid`. 2. Retry tidak menggandakan charge. 3. Koreksi memakai version baru, bukan menimpa yang lama |
| **Verifikasi** | Contract test; replay/concurrency test; bukti audit |
| **Risiko/pemilik** | Pharmacy, Clinical, dan Billing owner |
| **DoD** | Kontrak producer ditinjau; test lulus; catatan kompatibilitas tercatat |
| **Bukti** | [execution-evidence-RJ-BIL-BE-002.md](../execution-evidence-RJ-BIL-BE-002.md) |

---

### ✅ `RJ-BIL-BE-003` — Lab menagih hanya pemeriksaan yang benar-benar diterima

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI.** `39` test berbasis database yang pada revisi sebelumnya masih tertahan kini sudah dijalankan dan **lulus** |
| **Governance** | Sign-off **Lab**, **Clinical Governance**, dan **Billing/Finance** tetap `OPEN`. `QBE-MOD-002` sudah **`CLOSED`** — lihat bagian 6 |
| **Outcome** | Menyediakan Lab milestone minimal sampai `Accepted`. Kelayakan tagih dinilai **per specimen/komponen pemeriksaan**: satu pesanan `Rp450.000` yang salah satu komponennya ditolak menagih `Rp350.000` — bukan `Rp450.000`, dan bukan nol |
| **Trace** | `RJ-BIL-GATE-DEC-003`; `RJ-BIL-CAP-010` |
| **Kontrak** | State/Validation `RJ-BIL-STATE-001@1.0.0` |
| **Reuse** | `LabOrder` yang sudah ada sebagai titik mulai |
| **Scope** | Perluasan lifecycle order/specimen/acceptance boundary dan penerbitan fakta. Pelepasan hasil pemeriksaan tetap scope modul Lab |
| **Dependency** | `RJ-BIL-BE-001`; Lab owner |
| **Acceptance criteria** | 1. `Requested`, `Collected`, dan `Received` tidak menjadi initial charge. 2. `Accepted` menghasilkan eligibility fact. 3. Penolakan dan pengambilan ulang sampel meninggalkan riwayat |
| **Verifikasi** | Domain/state test; safety test; integration replay |
| **Risiko/pemilik** | Lab dan Clinical Governance |
| **DoD** | Bukti lifecycle lengkap; matriks acceptance test diperbarui; tidak ada SOP yang dikarang |
| **Bukti** | [execution-evidence-RJ-BIL-BE-003.md](../execution-evidence-RJ-BIL-BE-003.md) · [preflight-RJ-BIL-BE-003.md](../preflight-RJ-BIL-BE-003.md) · [remediasi penamaan QBE](../task/report/backend/be-rj-bil-003-remediasi-penamaan-qbe.md) |

> **Preflight read-only `2026-08-24`** menyimpulkan lifecycle Lab sudah terkunci penuh pada
> `RJ-BIL-GATE-DEC-003`, sehingga tidak ada SOP lab yang perlu dikarang. Empat pertanyaan pemilik
> `RJ-BIL-OQ-008` s.d. `RJ-BIL-OQ-011` dijawab author pada tanggal yang sama, lalu task
> dieksekusi.

---

### ⛔ `RJ-BIL-BE-004` — Radiologi menagih hanya pemeriksaan yang benar-benar dikerjakan

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Menunggu **penunjukan owner `RadiologyManagement`** dan kenaikan prefix `Rad` dari `PLANNED` ke `ACTIVE` lebih dulu. Area `RadiologyManagement` **belum ada sama sekali** pada source; ini greenfield penuh |
| **Governance** | Radiology owner **belum ada**; Clinical Governance `OPEN` |
| **Outcome** | Menetapkan Radiology operational boundary dan acquisition fact, sehingga pemeriksaan yang batal atau diulang tidak ikut tertagih, dan pemeriksaan tanpa gerbang keselamatan tidak pernah lahir |
| **Trace** | `RJ-BIL-GATE-DEC-004`; `RJ-BIL-CAP-011`; `RJ-BIL-DEP-005` berstatus `MISSING` |
| **Kontrak** | State/Integration `RJ-BIL-STATE-001@1.0.0`, `RJ-BIL-INT-001@1.0.0` |
| **Reuse** | Referensi bersama tindakan, tarif, dan Encounter |
| **Scope** | Perancangan dan implementasi capability Radiologi baru: gerbang keselamatan, study, pengulangan/pembatalan, dan acquisition fact yang dapat dipakai. **Tidak** mengaktifkan RIS/PACS eksternal |
| **Dependency** | `RJ-BIL-BE-001`; Radiology owner; Clinical Governance |
| **Acceptance criteria** | 1. Acquisition ditolak tanpa gerbang identitas dan keselamatan. 2. Study yang benar-benar dikerjakan dan dapat dipakai menjadi eligibility. 3. Pengulangan mempertahankan study aslinya |
| **Verifikasi** | Domain/integration/safety test |
| **Risiko/pemilik** | Radiology dan Clinical Governance |
| **DoD** | Bukti scope, owner, dan SOP disetujui; integrasi eksternal tetap tidak aktif |
| **Bukti** | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `19`; area `RadiologyManagement` tidak ditemukan pada source |

---

### ⛔ `RJ-BIL-BE-005` — Satu tagihan dapat dibagi ke beberapa penanggung

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Menunggu **keputusan pemilik atas `RJ-BIL-CONFLICT-001`** lebih dulu. Bentuk allocation ditentukan jawaban `RJ-BIL-OQ-001`, `RJ-BIL-OQ-002`, dan `RJ-BIL-OQ-005`; selama ketiganya belum dijawab, task ini **tidak dapat dirancang** — bukan sekadar belum dikerjakan |
| **Governance** | Direksi Rumah Sakit dan joint clinical-financial governance |
| **Outcome** | Menyediakan allocation multi-payer dan patient responsibility, sehingga satu kunjungan dapat ditanggung lebih dari satu pihak tanpa versi lamanya hilang |
| **Trace** | `RJ-BIL-GATE-DEC-002`; `RJ-BIL-CAP-013`; `RJ-BIL-CONFLICT-001` |
| **Kontrak** | API/Validation `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` |
| **Reuse** | `EncounterId`, referensi penanggung, dan snapshot tarif |
| **Scope** | Versi allocation baru, nominal absolut, sisa tanggungan, rujukan keputusan penanggung, dan penjaga kelebihan alokasi |
| **Dependency** | `RJ-BIL-BE-001`; Payer owner; **jawaban `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005`** |
| **Acceptance criteria** | 1. `Rp1.000.000` dapat menjadi A `Rp600.000` + B `Rp250.000` + pasien `Rp150.000`. 2. Versi yang menggantikan **tidak** menimpa histori versi sebelumnya |
| **Verifikasi** | Domain/API/property test |
| **Risiko/pemilik** | Billing, Payer, dan Finance |
| **DoD** | Kontrak allocation, invariant, test, dan bukti audit lengkap |
| **Bukti** | [RJ-BIL-CONFLICT-001-source-audit.md](../RJ-BIL-CONFLICT-001-source-audit.md) bagian `12` · [owner-decision-request-RJ-BIL-001.md](../owner-decision-request-RJ-BIL-001.md) |

---

### ✅ `RJ-BIL-BE-006` — Tindakan finansial melewati persetujuan, bukan kehendak satu orang

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI.** Ketiga acceptance criteria terbukti melalui `46` automated test, di dalam suite `157` test yang seluruhnya lulus. Migration `20260827075329_AddBillingFinancialAction` diterapkan ke database pengembangan `QuilvianNewDevTim01` atas `RJ-BIL-DEC-009`. Kedua gate preflight ditutup lebih dulu oleh `RJ-BIL-DEC-011` dan `RJ-BIL-DEC-012` |
| **Governance** | `RJ-BIL-GATE-DEC-006` berstatus `locked-draft`. Sign-off formal **Finance**, **Security/Privacy**, dan **delegated executive** masih `OPEN` dan menjadi syarat sebelum production. Matriks nominal approval `RJ-BIL-OQ-004` belum ditetapkan — akibatnya tindakan yang bergantung ambang berhenti pada `BlockedByPolicyConfiguration`, dan itu memang perilaku yang dikunci keputusannya. Owner Workflow **keluar dari jalur kritis** sejak `RJ-BIL-DEC-011` |
| **Outcome** | Menyediakan financial action, approval, dan close/reopen — sehingga pembatalan, koreksi, pengembalian uang, penggratisan, dan hapus buku selalu melewati orang kedua |
| **Trace** | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `RJ-BIL-CAP-015`; `RJ-BIL-DEC-008` |
| **Kontrak** | Permission/State `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` |
| **Reuse** | **Diputuskan `RJ-BIL-DEC-011`: tidak menumpang mesin Workflow Kepegawaian.** Mesin itu memang lengkap, tetapi dua invariant `GATE-DEC-006` tidak dapat ditegakkan di sana — larangan self-approval berupa `bool` per step yang dapat dinyalakan dari layar konfigurasi modul lain, dan penyaringan maker hanya terjadi sekali saat assignment dibuat. Billing memiliki entity approval-nya sendiri berprefix `Bil`. Yang tetap dipakai ulang: `BilFolio`, `BilChargeLine`, dan gerbang penutupan `RJ-BIL-BE-007` |
| **Scope** | Void, adjustment, reversal, refund, FOC, write-off, manual override, reopen folio; rujukan policy approval berversi dan berlaku-tanggal; revalidasi sasaran; gerbang penutupan. **Tidak** menghapus charge asli, dan **tidak** memindahkan uang |
| **Dependency** | `RJ-BIL-BE-001`, `RJ-BIL-BE-007`; Finance dan Security owner untuk sign-off production. Owner Workflow tidak lagi termasuk sejak `RJ-BIL-DEC-011` |
| **Acceptance criteria** | 1. Self-approval ditolak. 2. Permintaan yang masih menunggu persetujuan **tidak** mengubah state finansial kanonik. 3. Penutupan folio ditolak selama masih ada rekonsiliasi tertunda |
| **Verifikasi** | Authorization/integration/audit test — `46` test lulus (`25` murni tanpa database, `21` berbasis database). Tiga kegagalan yang sempat muncul seluruhnya ada pada assertion test, nol pada produk; rinciannya pada laporan task bagian `7` |
| **Risiko/pemilik** | Finance dan Security |
| **DoD** | Bukti policy dan version; test pemisahan wewenang; bukti rollback/replay — **terpenuhi**. Yang **tidak** ditutup DoD ini: sign-off Finance dan Security/Privacy tetap `OPEN`, sehingga ✅ berarti implementasi selesai dan terverifikasi, bukan boleh dipakai untuk pasien sungguhan |
| **Bukti** | [preflight-RJ-BIL-BE-006.md](../preflight-RJ-BIL-BE-006.md); [be-rj-bil-006-tindakan-finansial-dan-persetujuan.md](../task/report/backend/be-rj-bil-006-tindakan-finansial-dan-persetujuan.md) |

---

### ✅ `RJ-BIL-BE-007` — Kegagalan pemrosesan terlihat dan dapat dipulihkan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI `2026-08-27`.** Ketiga acceptance criteria terbukti melalui `37` automated test, di dalam suite `111` test yang seluruhnya lulus |
| **Governance** | Billing/Finance dan Integration owner tetap `OPEN` |
| **Outcome** | Menyediakan reconciliation case dan recovery status. Empat kemampuan yang ditambahkan: pemindaian rekonsiliasi yang idempoten, kepemilikan dan penyelesaian case, gerbang kesiapan penutupan folio, dan pencarian status pemrosesan kanonik berdasarkan identitas sumber |
| **Trace** | `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-017` |
| **Kontrak** | Integration `RJ-BIL-INT-001@1.0.0` |
| **Reuse** | `BilProcessingEffect` provisional |
| **Scope** | `OutcomeUnknown`, komponen yang gagal sebagian, dead-letter/review, status query, pemilik case dan SLA, laporan pemulihan. Delapan endpoint di bawah `api/v1/health-services/billing-management/reconciliation` dengan empat permission terpisah — `Read`, `Scan`, `Assign`, `Resolve` — dan dua tabel baru, `BilReconciliationCase` serta `MstBillingReconciliationPolicy`. **Tidak ada satu pun endpoint di sana yang memindahkan uang** |
| **Dependency** | `RJ-BIL-BE-001`; Integration owner |
| **Acceptance criteria** | 1. Timeout tidak menggandakan charge. 2. Komponen yang gagal terlihat. 3. Penutupan folio terblokir sampai case-nya selesai |
| **Verifikasi** | Failure-injection/recovery/concurrency test |
| **Risiko/pemilik** | Billing dan Integration |
| **DoD** | Kontrak rekonsiliasi, bukti laporan, dan case yang belum selesai terlihat |
| **Bukti** | [be-rj-bil-007-reconciliation-case-dan-recovery-status.md](../task/report/backend/be-rj-bil-007-reconciliation-case-dan-recovery-status.md) |

> **Menjalankan test terhadap database sungguhan untuk pertama kalinya sejak `RJ-BIL-BE-002`
> memunculkan lima cacat yang sebelumnya tidak terlihat** — termasuk satu celah kehilangan
> pendapatan pada `RJ-BIL-BE-003`, di mana sampel tetap dinyatakan layak sementara fakta
> tagihannya ditolak diam-diam. Seluruhnya diperbaiki dan dicatat pada bagian `7` laporan task.
>
> Satu hal tetap terbuka dan **bukan milik blueprint ini**: `MstRegister` tidak memiliki migration
> di mana pun, sehingga database yang benar-benar baru tidak akan memilikinya. Sudah dilaporkan
> kepada pemilik modulnya melalui `RJ-BIL-NOTICE-001`.

---

### ⛔ `RJ-BIL-BE-008` — Klaim dan settlement penanggung dapat dijalankan manual

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Menunggu `RJ-BIL-BE-005` selesai lebih dulu. Klaim dan settlement per penanggung juga bergantung pada jawaban `RJ-BIL-OQ-007` |
| **Governance** | Payer, Finance, dan Integration owner |
| **Outcome** | Menyediakan manual payer/claim/settlement workflow internal, sehingga rumah sakit dapat menagih penanggung tanpa satu pun adapter eksternal menyala |
| **Trace** | `RJ-BIL-GATE-DEC-009`; `RJ-BIL-CAP-022`; `RJ-BIL-DEP-007` berstatus `MISSING` |
| **Kontrak** | Integration `RJ-BIL-INT-001@1.0.0` |
| **Reuse** | Alur operator manual |
| **Scope** | Authorization, klaim, adjudikasi, dan status settlement; label `ManualOperator`; adapter eksternal hanya berbentuk interface |
| **Dependency** | `RJ-BIL-BE-005`, `RJ-BIL-BE-007`; Payer dan Finance owner |
| **Acceptance criteria** | 1. Klaim yang disetujui tetap `PaymentPending`. 2. Hasil manual **tidak** disebut sebagai keberhasilan eksternal. 3. Penolakan mempertahankan charge-nya |
| **Verifikasi** | Workflow/API/integration test |
| **Risiko/pemilik** | Payer, Finance, dan Integration |
| **DoD** | Kontrak manual dan audit diterima; adapter tetap mati |
| **Bukti** | [RJ-BIL-CONFLICT-001-source-audit.md](../RJ-BIL-CONFLICT-001-source-audit.md) bagian `12` |

---

### ⛔ `RJ-BIL-BE-009` — Setiap acceptance criteria kritis punya bukti test

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Menunggu `RJ-BIL-BE-001` s.d. `RJ-BIL-BE-008` selesai lebih dulu, karena cakupannya menutup coverage gap seluruh task |
| **Governance** | QA dan domain owner |
| **Outcome** | Menutup coverage gap automated verification, sehingga tidak ada satu pun acceptance criteria kritis yang berstatus selesai tanpa bukti |
| **Trace** | `RJ-BIL-GATE-DEC-001` s.d. `RJ-BIL-GATE-DEC-009`; `RJ-BIL-CAP-021` berstatus `Missing` pada snapshot audit |
| **Kontrak** | Acceptance `testing/acceptance-test-matrix.md` |
| **Reuse** | Test yang sudah ada pada source bila tersedia |
| **Scope** | Menambah test project atau spec terarah untuk lifecycle, duplikasi, allocation, koreksi, approval, dan gangguan layanan |
| **Dependency** | `RJ-BIL-BE-001` s.d. `RJ-BIL-BE-008` |
| **Acceptance criteria** | 1. Setiap acceptance criteria kritis punya bukti test **atau** pemilik gap-nya bernama |
| **Verifikasi** | Laporan test dan tinjauan traceability |
| **Risiko/pemilik** | QA dan domain owner |
| **DoD** | Laporan cakupan lengkap; gap yang diketahui sudah ada pemiliknya; **tidak ada satu pun status selesai yang palsu** |
| **Bukti** | Baris **Dependency** task ini: `BE-001` s.d. `BE-008` |

---

## 5. Bukti build dan keadaan migration per `2026-08-27`

### Tidak ada task bertanda 🟡

Tanda 🟡 berarti code sudah ditulis tetapi belum pernah di-build. Per `2026-08-27` tidak ada task
yang berada dalam keadaan itu.

| Pemeriksaan | Hasil |
| --- | --- |
| Build | `0` error, `138` warning — seluruh warning berada di modul lain, nol pada berkas `RJ-BIL-BE-006` |
| Test | **`157` lulus, `0` gagal**, `1 m 16 s` |
| Migration berstatus `(Pending)` | `0` |

`RJ-BIL-BE-006` sempat bertanda 🟡 selama beberapa jam pada tanggal ini, ketika code-nya sudah
ditulis tetapi build belum dijalankan. Tanda itu sekarang ✅.

> **Tiga kegagalan yang muncul dalam perjalanan, dan semuanya ada pada test.**
>
> | Kegagalan | Sebab | Perbaikan |
> | --- | --- | --- |
> | `51` test gagal `42P01 relation does not exist` | Tabel `RJ-BIL-BE-006` belum ada; migration memang belum dibangkitkan. Jumlahnya besar karena pembersihan teardown menyentuh tabel baru pada setiap kelas test berbasis database | Migration dibangkitkan lalu diterapkan |
> | `2` test menuntut folio berstatus `Open` | Salah tebak: folio hasil seed berstatus `ReviewRequired`. Ketiga assertion inti — penutupan ditolak, kode galat, alasan penghalang — sudah lulus | Diganti menjadi lebih ketat: rekam status sebelum, bandingkan sesudah, pastikan tidak berubah sama sekali |
> | `1` test idempotensi meleset `2` tick | Beda presisi, bukan pelaksanaan ganda. `DateTime` .NET berpresisi 100 nanodetik; kolom `timestamp with time zone` berpresisi mikrodetik | Membandingkan nilai tersimpan dengan nilai tersimpan. **Toleransi waktu sengaja tidak dipakai** — test idempotensi adalah test yang paling tidak boleh dilonggarkan |
>
> Nol kegagalan berasal dari produk.

### Keadaan migration

Diverifikasi dengan `dotnet ef migrations list --no-build` — tanpa build, read-only terhadap
database:

| Pemeriksaan | Hasil |
| --- | --- |
| Migration berstatus `(Pending)` | `0` |

Artinya seluruh migration blueprint ini sudah terpasang pada database pengembangan bersama
`QuilvianNewDevTim01`, termasuk ketiga migration yang pada revisi sebelumnya masih tercatat
tertunda:

| Migration | Catatan revisi sebelumnya | Keadaan sebenarnya per `2026-08-27` |
| --- | --- | --- |
| `20260824091610_AddLaboratorySpecimenLifecycle` | *"tidak diterapkan ke database mana pun"* | **Sudah diterapkan** |
| `20260826101500_RenameClinicalMilestoneFactToBillingOwnership` | *"belum diterapkan"* | **Sudah diterapkan** |
| `20260827040349_AddBillingReconciliationCase` | — | **Sudah diterapkan** |
| `20260827075329_AddBillingFinancialAction` | — | **Sudah diterapkan** — `RJ-BIL-BE-006` |

Seluruhnya terpasang melalui `Database.Migrate()` yang dipanggil `BillingTestDatabaseFixture`
sebelum test pertama, di bawah otorisasi `RJ-BIL-DEC-009`. Penerapan terbatas pada database
pengembangan.

> **Masalah `RJ-BIL-BE-007` tidak terulang.** Waktu itu EF menerbitkan `CreateTable` untuk tiga
> tabel milik modul lain karena snapshot kehilangan entity-nya, dan migration-nya gagal `42P07`
> di database yang sudah berjalan. Kali ini snapshot utuh — `525` → `529` entity, persis `+4` —
> dan migration yang dibangkitkan hanya memuat `4` `CreateTable` beserta `15` `CreateIndex`,
> seluruhnya milik `RJ-BIL-BE-006`. Tidak ada tabel modul lain dan tidak ada satu pun operasi
> foreign key yang tidak diminta.

> **Penjagaan yang tidak mengenal opt-in.** Penanda `prod`, `production`, `live`, `staging`,
> `stage`, dan `uat` ditolak **mutlak** oleh penjagaan fixture. Tidak ada flag atau konfigurasi
> apa pun yang dapat melewatinya, dan hal itu dikunci oleh test tersendiri.

Migration `20260821033911_AddBillingOperationalBaseline` diterapkan lebih dahulu atas otorisasi
terpisah pada `2026-08-21`. Dua migration `RJ-BIL-BE-002` —
`20260824074649_AddClinicalMilestoneFactHandoff` dan
`20260824080430_StoreClinicalFactSnapshotAsText` — ikut terpasang melalui jalur fixture yang sama.
Riwayat dan dampaknya ada pada bagian `8`
[execution-evidence-RJ-BIL-BE-002.md](../execution-evidence-RJ-BIL-BE-002.md).

---

## 6. Riwayat pelaksanaan yang perlu diingat

### Remediasi penamaan QBE per `2026-08-26`

`RJ-BIL-BE-002` dan `RJ-BIL-BE-003` dikerjakan **tanpa pernah melewati** `AGENTS.md` dan
`docs/engineering/`. Tiga entity melanggar `QBE-NAM-001`, `QBE-NAM-002`, dan `QBE-MOD-002`, lalu
diperbaiki:

| Sebelum | Sesudah | Dampak schema |
| --- | --- | --- |
| `TrxLabSpecimen` | `LabSpecimen` | Nihil — nama tabelnya memang sudah `LabSpecimen` sejak `20260824091610`; yang salah hanya nama kelasnya |
| `TrxLabTransitionHistory` | `LabTransitionHistory` | Nihil, dengan alasan yang sama |
| `TrxClinicalMilestoneFact` | `BilClinicalMilestoneFact`, berpindah ke `BillingManagement/Operational/` | Rename tabel yang mempertahankan data, melalui `20260826101500` |

`QBE-MOD-002` kini **`CLOSED`**. `LaboratoryManagement / Lab` sudah berstatus `ACTIVE` pada
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `18`, melalui `RJ-BIL-DEC-007` yang
menunjuk **Sukma Giri** sebagai Product/Domain Owner `LaboratoryManagement`.
`FORMAL_LAB_GOVERNANCE_SIGNOFF` dan `CLINICAL_GOVERNANCE_SIGNOFF` tetap `OPEN`. Rincian lengkap
beserta bukti verifikasi ada pada
[be-rj-bil-003-remediasi-penamaan-qbe.md](../task/report/backend/be-rj-bil-003-remediasi-penamaan-qbe.md).

### Blocker yang masih berjalan

| Blocker | Keadaannya | Menahan |
| --- | --- | --- |
| `RJ-BIL-CONFLICT-001` | Audit read-only `2026-08-24` menyimpulkan `CONFIRMED` dengan source confidence `HIGH`, **tanpa memerlukan perubahan code saat ini**. Yang belum ada adalah jawaban `RJ-BIL-OQ-001`, `OQ-002`, dan `OQ-005` | `RJ-BIL-BE-005` tidak dapat dirancang. `BE-008` terdampak tidak langsung karena ia bekerja di atas hasil allocation. `BE-006` **tidak lagi** terdampak — ia selesai `2026-08-27` tanpa menunggu allocation |
| `RJ-BIL-BE-002-BLOCKER-001` | Satu blocker kebijakan: pintu masuk telaah farmasi setelah kewenangan finansial klinis dihapus | **Tidak menahan apa pun** — bukan acceptance criteria `RJ-BIL-BE-002`, bukan pula task berikutnya |
| `RJ-BIL-NOTICE-001` | `MstRegister` tidak memiliki migration di mana pun, sehingga database yang benar-benar baru tidak akan memilikinya | Bukan milik blueprint ini; sudah dilaporkan kepada pemilik modulnya |

---

## 7. Gerbang yang masih terbuka

| Gerbang | Keadaannya | Menahan |
| --- | --- | --- |
| **Jawaban `RJ-BIL-OQ-001`, `OQ-002`, `OQ-005`** | Pertanyaan sudah tersusun siap jawab pada [owner-decision-request-RJ-BIL-001.md](../owner-decision-request-RJ-BIL-001.md); belum dijawab | `RJ-BIL-BE-005`, dan lewat dependency-nya `RJ-BIL-BE-008` |
| **Owner `RadiologyManagement`** | Belum ditunjuk. Prefix `Rad` masih `PLANNED` pada registry baris `19` | `RJ-BIL-BE-004` tidak boleh dimulai |
| **Matriks nominal approval `RJ-BIL-OQ-004`** | Belum ditetapkan Finance. `MstBillingApprovalPolicy` sengaja **kosong tanpa seed** — `RJ-BIL-GATE-DEC-006` melarang memakai default approver/threshold | Tindakan finansial yang bergantung ambang berhenti pada `BlockedByPolicyConfiguration`. Tagihan deterministik normal tidak terpengaruh |
| Finance dan Security owner | Belum bernama. `RJ-BIL-GATE-DEC-006` masih `locked-draft`. Owner Workflow **tidak lagi** termasuk sejak `RJ-BIL-DEC-011` | Sign-off production `RJ-BIL-BE-006`, bukan penulisan code-nya |
| Sign-off Lab, Clinical Governance, Billing/Finance | `OPEN`. Tanda ✅ pada `RJ-BIL-BE-003` dan `RJ-BIL-BE-007` **tidak** menutup gerbang ini | Bukan penandaan selesai; yang tertahan adalah **pemakaian untuk pasien sungguhan** |
| Working tree belum di-commit | `RJ-BIL-BE-003`, `RJ-BIL-BE-006`, `RJ-BIL-BE-007`, dan remediasi penamaan QBE | Bukti bersifat provisional; builder wajib preflight ulang |
| ~~Build dan test `RJ-BIL-BE-006`~~ | **DITUTUP `2026-08-27`.** `157` test lulus, `0` gagal | — |
| ~~Migration `RJ-BIL-BE-006`~~ | **DITUTUP `2026-08-27`.** `20260827075329_AddBillingFinancialAction` diterapkan ke `QuilvianNewDevTim01` atas `RJ-BIL-DEC-009` | — |
| ~~`BUILDER_EXECUTION` untuk `RJ-BIL-BE-006`~~ | **DITUTUP `2026-08-27`** oleh `RJ-BIL-DEC-012`. Naik menjadi `AUTHORIZED` atas otoritas pemilik blueprint | — |
| ~~Keputusan arsitektur maker-checker `RJ-BIL-BE-006`~~ | **DITUTUP `2026-08-27`** oleh `RJ-BIL-DEC-011`. Billing memiliki entity approval-nya sendiri, tidak menumpang mesin Workflow Kepegawaian | — |
| ~~`QBE-MOD-002` lifecycle Lab~~ | **DITUTUP `2026-08-26`** oleh `RJ-BIL-DEC-007`. `LaboratoryManagement / Lab` naik ke `ACTIVE` | — |
| ~~`RJ-BIL-CONFLICT-006` kewenangan finansial Pharmacy~~ | **DITUTUP `2026-08-24`** oleh keputusan author `1A` dan `1B`; dilaksanakan pada `RJ-BIL-BE-002` | — |
| `RJ-BIL-DEP-009` adapter payer eksternal | Kontrak, kredensial, sandbox/UAT, dan bukti rekonsiliasi belum ada | **Hanya** aktivasi eksternal. Alur payer manual tetap berjalan |
| UI visual authority | Belum dikunci | Detail visual tetap `DEV_DISCRETION` — lihat [roadmap frontend](frontend-roadmap.md) |

---

## 8. Yang sengaja tidak ada di roadmap ini

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Aktivasi adapter payer eksternal bernama | `RJ-BIL-DEP-009` belum punya kontrak, kredensial, sandbox/UAT, maupun bukti rekonsiliasi. Ia tetap `INACTIVE / OUT OF CURRENT DELIVERY SCOPE` |
| Pelepasan hasil pemeriksaan laboratorium | Tetap scope modul Lab. `RJ-BIL-BE-003` berhenti pada `Accepted` |
| Integrasi RIS/PACS | `RJ-BIL-BE-004` merancang boundary Radiologi, **bukan** menyalakan sistem eksternalnya |
| Orchestration klaim otomatis | Tidak ada orchestration maupun claim source pada snapshot — `RJ-BIL-DEP-007` berstatus `MISSING`. Release ini manual, berlabel `ManualOperator` |
| Mutasi finansial dari modul klinis | Dicabut oleh `RJ-BIL-BE-002`. Modul klinis hanya menerbitkan fakta |
| Endpoint rekonsiliasi yang memindahkan uang | Kedelapan endpoint `RJ-BIL-BE-007` hanya memindai, menugaskan, dan menyelesaikan case |

Keenam butir itu adalah **keadaan yang disengaja**, bukan cakupan yang terlupa.

---

## 9. Aturan eksekusi dan handoff builder

Roadmap ini sudah disetujui untuk eksekusi task. Builder tetap memerlukan **handoff task**,
**wewenang tulis backend**, dan **QBE preflight** pada waktu eksekusi.

Setiap handoff ke `build-module-backend` wajib menyertakan:

| Yang wajib disertakan | Contoh |
| --- | --- |
| Task ID | `RJ-BIL-BE-006` |
| Approval task | `APPROVED_FOR_EXECUTION` pada `2026-08-21` |
| Contract hash | `RJ-BIL-CONTRACT-001@1.0.0` |
| Source SHA dan keadaan working tree | `6b25e60` cabang `sukmagp`; working tree belum di-commit |
| Keadaan dependency | Task pendahulu selesai; owner sudah bernama |
| QBE preflight | Hasil pemeriksaan penamaan, ownership, dan lifecycle modul |
| Bukti acceptance yang diminta | Sesuai baris **Verifikasi** dan **DoD** task tersebut |

> **Tidak satu pun task di dokumen ini memberi izin** menerapkan migration, memutasi database,
> melakukan deployment, commit, maupun publish. Kelima hal itu adalah wewenang terpisah yang
> harus diminta tersendiri.
