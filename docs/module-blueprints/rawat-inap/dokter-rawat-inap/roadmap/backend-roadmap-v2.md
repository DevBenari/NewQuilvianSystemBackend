# Roadmap Delivery Backend V2 — Sub-modul Dokter Rawat Inap

> ## Berkas ini **baru**, dan **tidak menggantikan** `backend-roadmap.md`
>
> | Hal | `backend-roadmap.md` (lama) | **`backend-roadmap-v2.md` (berkas ini)** |
> | --- | --- | --- |
> | Isinya | Task revision `0.3` s.d. `6`, termasuk `Gelombang 1A` | **Hanya** task penyelarasan `PRD-RWI-V2-001` revision `7` |
> | Rentang task ID | `BE-RWI-037` s.d. `BE-RWI-078` | **`BE-RWI-088` s.d. `BE-RWI-105`** |
> | Statusnya | **Tetap berlaku** sebagai register task lama | Register task baru |
> | Kenapa dipisah | Permintaan pemilik 16 September 2026: berkas lama sudah terlalu panjang untuk dibaca | — |
>
> **Nomor task tidak pernah dipakai ulang.** Deret `BE-RWI-###` berjalan lurus melintasi seluruh
> berkas roadmap dan ketiga sub-modul. Nomor `BE-RWI-079` s.d. `BE-RWI-087` dipakai
> `episode-rawat-inap`, dan `BE-RWI-106` s.d. `BE-RWI-126` dipakai `keperawatan`.
>
> Berkas lama: [`backend-roadmap.md`](./backend-roadmap.md) — jangan menambah task baru di sana.

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
entity_prefix: Inp
blueprint_id: RWI-BP-001
blueprint_revision: 7
blueprint_shape: COMPOSITE
submodule: dokter-rawat-inap
blueprint_root: docs/module-blueprints/rawat-inap/dokter-rawat-inap/
roadmap_file: roadmap/backend-roadmap-v2.md
roadmap_revision: 2
status: APPROVED
roadmap_mode: DELIVERY
realignment_phase: RLN-PH-07
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061)"
approved_at: "2026-09-16"
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
upstream_input: "PRD-RWI-V2-001 v2.0 — docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md"
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
contract_version: 0.6.0
decision_source: "00-interview-decisions.md revision 23; RWI-DEC terakhir 151"
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
task_id_range: BE-RWI-088..BE-RWI-105
task_id_next_free: BE-RWI-127
completed_tasks: [BE-RWI-088, BE-RWI-089, BE-RWI-090, BE-RWI-091, BE-RWI-092, BE-RWI-093, BE-RWI-094, BE-RWI-095, BE-RWI-096, BE-RWI-097, BE-RWI-098, BE-RWI-099, BE-RWI-100, BE-RWI-101, BE-RWI-102, BE-RWI-103, BE-RWI-104, BE-RWI-105]
blocked_tasks: []
last_updated: "2026-09-17 — BE-RWI-088 s.d. BE-RWI-105 SELESAI SELURUHNYA (18 task / 100% backend dokter-rawat-inap tuntas); BE-RWI-100 selesai; dotnet build mandiri oleh pemilik"
waves: [DOK-V2-0, DOK-V2-1, DOK-V2-2, DOK-V2-3]
migration_steps: [R1, R2, R3, R4, R5, R6, R7, R8, R9]
owned_tables_note: "Sub-modul ini TIDAK memiliki satu tabel pun — RWI-DEC-081. Seluruh tabel milik ClinicalManagement, PharmacyManagement, LaboratoryManagement, RadiologyManagement"
test_policy: "rules/backend/TEST_POLICY.md — backend tidak memelihara project automated test"
write_authority: "TIDAK diberikan di sini. Wewenang tulis source, migration, database, dan deployment dinyatakan terpisah per task"
```

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan Definition of Done sudah terbukti, laporan tracked-nya ada |
| 🟡 | Source-nya sudah ada tetapi kriterianya belum terbukti penuh |
| ⛔ | Prasyaratnya belum terpenuhi; nama blocker-nya disebut |
| tanpa tanda | Belum disentuh sama sekali |

---

## Peringatan kepemilikan tabel

`RWI-DEC-081` menetapkan bahwa sub-modul ini **tidak memiliki satu tabel pun**. Seluruh migration
pada roadmap ini menyentuh tabel milik modul lain:

| Migration | Tabel yang disentuh | Pemilik tabel |
| --- | --- | --- |
| `R2`, `R3`, `R7` | `TrxPatientIntegratedProgressNote`, `TrxPatientProcedure`, registrasi keutuhan | `ClinicalManagement` |
| `R4`, `R5`, `R6`, `R9` | `PhmPrescriptionItem`, rekonsiliasi, template dan order sliding scale | `PharmacyManagement` |
| `R8` | `LabOrder`, `RadOrder` | `LaboratoryManagement`, `RadiologyManagement` |

Task di bawah **MUST NOT** membuat tabel tandingan di dalam `InPatientManagement`. Bila saat
implementasi terasa butuh tabel baru milik Rawat Inap, yang benar adalah kembali ke `grill-me`,
bukan membuatnya diam-diam.

---

## Grafik Urutan Dependency

Roadmap ini memuat 18 task, melewati batas 15 node satu grafik. Karena itu grafiknya dipecah per
gelombang, dengan satu grafik ringkasan antar-gelombang di bawah ini.

### Ringkasan antar-gelombang

```text
DOK-V2-0 ─> DOK-V2-1 ─┬─> DOK-V2-2 ─> DOK-V2-4
                      │
                      └─> DOK-V2-3
```

Node pada grafik ringkasan ini adalah **gelombang**, bukan task. `DOK-V2-4` tidak punya task
backend — isinya seluruhnya frontend, dan ada pada
[`frontend-roadmap-v2.md`](./frontend-roadmap-v2.md).

### Legenda label asal

- `[BE-INP]` = task backend pada `episode-rawat-inap/roadmap/backend-roadmap-v2.md`
- `[BE-KEP]` = task backend pada `keperawatan/roadmap/backend-roadmap-v2.md`
- `[V0]`, `[V1]`, `[V2]` = task pada gelombang lain **di roadmap ini**, digambar sebagai cermin baca-saja pada grafik gelombang yang membutuhkannya
- `{...}` = gerbang atau keputusan yang menahan, bukan task

Node bertanda label asal adalah **cermin baca-saja** dan boleh muncul pada lebih dari satu grafik
gelombang. Task milik roadmap ini sendiri muncul **tepat satu kali**, yaitu pada grafik gelombangnya.

### `DOK-V2-0` — perbaikan penjaga tanpa perubahan bentuk data

```text
BE-RWI-088

BE-RWI-089

BE-RWI-090
```

Ketiganya tidak menunggu siapa pun dan boleh dikerjakan paralel hari pertama. Nol pasangan.

### `DOK-V2-1` — registrasi, jenis catatan, pesanan tindakan

```text
BE-RWI-079 [BE-INP] ─┬─> BE-RWI-091 ─┬─> BE-RWI-093
                     │               │
BE-RWI-088 [V0] ─────┘               └─> BE-RWI-092

BE-RWI-079 [BE-INP] ─> BE-RWI-094 ─┬─> BE-RWI-095 ─> BE-RWI-096
                                   │
BE-RWI-089 [V0] ───────────────────┘

BE-RWI-079 [BE-INP] ─┬─> BE-RWI-097 ─> BE-RWI-098
                     │
BE-RWI-090 [V0] ─────┘
```

~~`{GATE-YOGA}` = persetujuan Yoga Aji Pratama atas `my-authored` dan `serviceContext`.~~ **Gerbang
ini tertutup 2026-09-16 lewat `RWI-DEC-151`**, sehingga node-nya dicabut dari grafik dan `BE-RWI-092`
kini hanya menunggu `BE-RWI-091`. Sebelas pasangan.

### `DOK-V2-2` — resep, rekonsiliasi, sliding scale

```text
BE-RWI-099 ─┬─> BE-RWI-101
            │
            └───────────┬─> BE-RWI-100
                        │
BE-RWI-114 [BE-KEP] ────┘

BE-RWI-102 ─> BE-RWI-103
```

Empat pasangan.

### `DOK-V2-3` — penunjang dan template

```text
BE-RWI-097 [V1] ─> BE-RWI-104

BE-RWI-099 [V2] ─> BE-RWI-105
```

~~`{GATE-LABRAD}` = persetujuan pemilik `LaboratoryManagement` dan `RadiologyManagement` atas kolom
instruksi.~~ **Gerbang ini tertutup 2026-09-16 lewat `RWI-DEC-153`** — pemiliknya bernama **Yoga Aji** —
sehingga node-nya dicabut dan `BE-RWI-104` kini hanya menunggu `BE-RWI-097`. Dua pasangan.

**Jumlah pasangan seluruh grafik: 0 + 11 + 4 + 2 = 17.** Jumlah entri kolom `Dependency` pada tabel
task: **17**. Keduanya cocok. Turun dari 19 karena `RWI-DEC-151` mencabut node `{GATE-YOGA}` dan
`RWI-DEC-153` mencabut node `{GATE-LABRAD}`.

### Tabel gelombang eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-088`, `BE-RWI-089`, `BE-RWI-090`, `BE-RWI-099`, `BE-RWI-102` — boleh paralel |
| 2 | `BE-RWI-088` dan `BE-RWI-079` [BE-INP] | `BE-RWI-091` |
| 2 | `BE-RWI-079` [BE-INP] | `BE-RWI-094` |
| 2 | `BE-RWI-090` dan `BE-RWI-079` [BE-INP] | `BE-RWI-097` |
| 2 | `BE-RWI-099` | `BE-RWI-101`, `BE-RWI-105` |
| 2 | `BE-RWI-099` dan `BE-RWI-114` [BE-KEP] | `BE-RWI-100` — dirilis bersama `KEP-V2-2` |
| 2 | `BE-RWI-102` | `BE-RWI-103` |
| 3 | `BE-RWI-091` | `BE-RWI-092`, `BE-RWI-093` — boleh paralel; `BE-RWI-092` lepas dari `⛔` lewat `RWI-DEC-151` |
| 3 | `BE-RWI-094`, `BE-RWI-089` | `BE-RWI-095` |
| 3 | `BE-RWI-097` | `BE-RWI-098`, `BE-RWI-104` — boleh paralel; `BE-RWI-104` lepas dari `⛔` lewat `RWI-DEC-153` |
| 4 | `BE-RWI-095` | `BE-RWI-096` |

Pemetaan gelombang PRD:

| Gelombang PRD | Isinya | Task backend |
| --- | --- | --- |
| `DOK-V2-0` | `FR-DOK-075`, `076`, `081`, `082` beserta regresinya | `BE-RWI-088` s.d. `BE-RWI-090` |
| `DOK-V2-1` | `EPIC DOK-11`, `EPIC DOK-12`, kolom `TrxPatientProcedure` | `BE-RWI-091` s.d. `BE-RWI-098` |
| `DOK-V2-2` | `EPIC DOK-13`, `EPIC DOK-14` | `BE-RWI-099` s.d. `BE-RWI-103` |
| `DOK-V2-3` | `EPIC DOK-15` bagian Lab/Rad; template | `BE-RWI-104`, `BE-RWI-105` |
| `DOK-V2-4` | `EPIC DOK-10`, `16`, `17` | **Nol task backend** — frontend saja |

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-088` | Hanya penulis yang menyunting konsepnya, dan hanya dokter berpenugasan yang menulis dokumen baru | `FR-DOK-075`, `FR-DOK-076`; `INV-DOK-14`, `INV-DOK-15` | `0.6.0` permission matrix | Penjaga penulis klinis yang sudah ada | `R1` — penulis tunggal konsep; penugasan dinilai pada **waktu klinis** dan wajib aktif saat disimpan | — | AC-1 s.d. AC-5 | Validasi source/QBE; `dotnet build` **NOT RUN** atas instruksi pemilik | `REPAIR` menyentuh 8 dari 9 titik panggil resolver / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-088.md) |
| `BE-RWI-089` | Hanya DPJP aktif yang memverifikasi CPPT | `FR-DOK-082`; `INV-DOK-16` | `0.6.0` permission matrix | Jalur verifikasi CPPT yang sudah ada | `R1` — peran DPJP dinilai pada **detik verifikasi**; konsulen dan dokter jaga `403` | — | AC-1 s.d. AC-4 | Validasi source/QBE; `dotnet build` **NOT RUN** atas instruksi pemilik | Salah nilai peran = catatan terverifikasi orang yang tidak berwenang / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-089.md) |
| `BE-RWI-090` | Tindakan baru tidak lahir pada episode yang sudah ditutup | `FR-DOK-081`; `RLN3-CAP-35` | `0.6.0` validation matrix | Pemeriksaan status episode yang sudah ada | `R1` — tindakan baru dari catatan dokter pada episode `Closed` ditolak `422` | — | AC-1 s.d. AC-3 | Validasi source/QBE; `dotnet build` **NOT RUN** atas instruksi pemilik | — / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-090.md) |
| `BE-RWI-091` | Satu konsep membentuk tepat satu registrasi keutuhan | `FR-DOK-074`, `FR-DOK-077`; `INV-DOK-18` | `0.6.0` state matrix | Mesin keutuhan `MedicalRecordManagement` | `R2` — registrasi sejak konsep untuk SOAP dan kajian medis rawat inap; tanda tangan memakai registrasi yang sama | `BE-RWI-088`, `BE-RWI-079` [BE-INP] | AC-1 s.d. AC-5 | Validasi source/QBE; `dotnet build` **PASS** 17-09-2026 | Registrasi ganda = riwayat dokumen pecah. Perubahan mesin **disetujui** 2026-09-16 `RWI-DEC-151` / Yoga Aji Pratama ✅ | ✅ [Laporan](../task/report/backend/BE-RWI-091.md) |
| `BE-RWI-092` | Dokter menemukan konsep dan catatan terkuncinya sendiri | `FR-DOK-079`; `RWI-DEC-127`, `RWI-DEC-142` | `0.6.0` API `my-authored` | Mesin keutuhan | Daftar `my-authored` — konsep dan catatan terkunci milik penulis login, identitas pasien **minimum saja** | `BE-RWI-091` | AC-1 s.d. AC-5 | Validasi source/QBE; `dotnet build` **NOT RUN** atas instruksi pemilik | ~~menunggu Yoga Aji Pratama~~ **disetujui 2026-09-16 `RWI-DEC-151`**; jalur baca data pasien wajib tetap sempit / Yoga Aji Pratama | ✅ [Laporan](../task/report/backend/BE-RWI-092.md) |
| `BE-RWI-093` | Penulis mengoreksi catatannya sendiri walau penugasannya sudah berakhir | `FR-DOK-080`; `RWI-DEC-127` | `0.6.0` permission matrix | Mesin addendum `RWI-FACT-013` | Addendum oleh penulis asli pada catatan final atau terkunci miliknya, tanpa penugasan aktif | `BE-RWI-091` | AC-1 s.d. AC-4 | Validasi reuse/source; `dotnet build` **NOT RUN** atas instruksi pemilik | Pengecualian sempit terhadap hanya-baca episode tertutup / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-093.md) |
| `BE-RWI-094` | CPPT tahu jenis catatan yang disimpan | `FR-DOK-085`; `RWI-DEC-141` | `0.6.0` data dictionary | `TrxPatientIntegratedProgressNote` | `R3` — kolom `NoteKind`; diperiksa terhadap profesi penulis; entri lama `Unspecified` **tanpa tebakan** | `BE-RWI-079` [BE-INP] | AC-1 s.d. AC-4 | Validasi source/QBE; build dan migration **NOT RUN** | Pengisian tebakan pada entri lama = pemalsuan data klinis / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-094.md) |
| `BE-RWI-095` | DPJP terakhir tetap dapat memverifikasi entri yang tertinggal | `FR-DOK-083`; `RWI-DEC-125`, `RWI-DEC-126` | `0.6.0` state matrix | Jalur verifikasi dari `BE-RWI-089` | Pada episode `Closed`, DPJP **terakhir** memverifikasi entri yang ditulis sebelum penutupan; keterlambatan tetap tercatat | `BE-RWI-094`, `BE-RWI-089` | AC-1 s.d. AC-5 | Validasi source/QBE; `dotnet build` **NOT RUN** atas instruksi pemilik | DPJP **sebelumnya** tidak boleh ikut memverifikasi / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-095.md) |
| `BE-RWI-096` | Entri yang menunggu verifikasi tidak hilang setelah episode ditutup | `FR-DOK-084` | `0.6.0` API daftar tunggu | Daftar tunggu verifikasi yang sudah ada | Daftar memuat episode `Closed` milik DPJP terakhir sampai seluruh entrinya terverifikasi | `BE-RWI-095` | AC-1 s.d. AC-4 | Validasi source/QBE; `dotnet build` **NOT RUN** atas instruksi pemilik | — / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-096.md) |
| `BE-RWI-097` | Perawat memesan tindakan atas instruksi dokter yang bertugas | `FR-DOK-100`, `101`, `102`; `RWI-DEC-133` | `0.6.0` data + API | `TrxPatientProcedure` | `R7` — enam kolom baru; **longgarkan `ConsultationId`**; `PatientProcedureOrderService` | `BE-RWI-090`, `BE-RWI-079` [BE-INP] | AC-1 s.d. AC-6 | Validasi source/skema; `dotnet build` **NOT RUN** atas instruksi pemilik | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-097.md) |
| `BE-RWI-098` | Dokter pemberi instruksi memverifikasi pesanan yang dibuat perawat | `FR-DOK-103`, `FR-DOK-104`; `INV-DOK-17` | `0.6.0` state matrix | Jalur pesanan dari `BE-RWI-097` | Verifikasi instruksi hanya oleh dokter pemberi instruksi; verifikasi **tidak** mengubah penginput dan isi; pelaksana menjadi penulis catatan pelaksanaan | `BE-RWI-097` | AC-1 s.d. AC-5 | Validasi source/QBE; `dotnet build` **PASS** 17-09-2026 | — / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-098.md) |
| `BE-RWI-099` | Dokter melihat seluruh resep pasien dalam satu periode | `FR-DOK-086`; `RWI-DEC-134` | `0.6.0` data + API | `PhmPrescriptionItem` | `R4` — lima kolom baru; endpoint Resep Harian bersaring periode, termasuk racikan dan obat pulang | — | AC-1 s.d. AC-5 | Validasi source/skema; build **PASS**; migration R4 diterapkan ke DB dev pribadi 17-09-2026 | Kolom baru pada tabel milik `PharmacyManagement` / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-099.md) |
| `BE-RWI-100` | Obat yang dihentikan berhenti sampai ke perawat | `FR-DOK-087`; `INT-KEP-09` | `0.6.0` integrasi | Mesin MAR dari `BE-RWI-114` | Penghentian butir beralasan membatalkan dosis MAR `Due` sesudahnya **dalam satu transaksi** | `BE-RWI-099`, `BE-RWI-114` [BE-KEP] | AC-1 s.d. AC-5 | Validasi source/QBE; pembatalan MAR & sliding scale terpadu; `dotnet build` mandiri oleh pemilik | Dosis `Administered` tidak boleh tersentuh / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-100.md) |
| `BE-RWI-101` | Obat bawaan pasien diputuskan nasibnya satu per satu | `FR-DOK-092`, `FR-DOK-093` | `0.6.0` data + state | Jalur resep dari `BE-RWI-099` | `R5` — tabel rekonsiliasi; keputusan Lanjut Sama, Lanjut Ubah, Hentikan; jalur obat non-formularium | `BE-RWI-099` | AC-1 s.d. AC-6 | Validasi source/skema; build **PASS**; migration R5 diterapkan ke DB dev pribadi 17-09-2026 | Perawat `403` pada jalur keputusan / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-101.md) |
| `BE-RWI-102` | Protokol sliding scale disahkan sebagai versi, bukan angka lepas | `FR-DOK-094`, `FR-DOK-095`; `RWI-DEC-146`, `RWI-DEC-147` | `0.6.0` data + state | — (`MISSING / NEW`) | `R6` — tabel template dan versi protokol; rentang menutup seluruh nilai tanpa tumpuk; pengesah bukan pengubah terakhir | — | AC-1 s.d. AC-6 | Validasi source/skema; build **PASS**; migration R6 diterapkan ke DB dev pribadi 17-09-2026; `RWI-OQ-097` tetap terbuka | Rentang bertumpuk atau berlubang = dosis insulin salah / **pemilik klinis belum ditunjuk** | ✅ [Laporan](../task/report/backend/BE-RWI-102.md) |
| `BE-RWI-103` | Order sliding scale per pasien lahir dari versi yang sah | `FR-DOK-096`, `097`, `098`, `099`; `RWI-DEC-146` | `0.6.0` API + state | Tabel dari `BE-RWI-102` | Order hanya dari versi `Approved`; rentang **tersalin**; penyesuaian wajib beralasan dan membuat versi order baru | `BE-RWI-102` | AC-1 s.d. AC-6 | Validasi source/QBE; `dotnet build` **PASS** 17-09-2026 | Versi template baru tidak boleh mengubah order berjalan / Muhammad Hamzah | ✅ [Laporan](../task/report/backend/BE-RWI-103.md) |
| `BE-RWI-104` | Pesanan Lab dan Radiologi membawa pemberi instruksi | `FR-DOK-106` | `0.6.0` integrasi | `LabOrder`, `RadOrder` | `R8` — empat kolom instruksi pada dua tabel milik modul lain | `BE-RWI-097` | AC-1 s.d. AC-4 | Validasi source/skema; build **PASS**; migration R8 diterapkan ke DB dev pribadi 17-09-2026; **regresi Lab/Rad NOT RUN** | ~~menunggu pemilik Lab/Rad~~ **disetujui 2026-09-16 `RWI-DEC-153`**; regresi Lab/Rad tetap wajib / Yoga Aji | ✅ [Laporan](../task/report/backend/BE-RWI-104.md) |
| `BE-RWI-105` | Template resep benar-benar milik dokter yang membuatnya | `FR-DOK-088`, `089`, `090`, `091` | `0.6.0` permission + validation | `PrescriptionTemplateService` | `R9` — pemilik dari **akun login**; hanya template milik sendiri; template kosong ditolak; butir bentrok ditandai | `BE-RWI-099` | AC-1 s.d. AC-6 | Validasi source/QBE; `dotnet build` **PASS** 17-09-2026; **regresi poliklinik NOT RUN** | **Mengubah perilaku poliklinik** — pemberitahuan pemilik `rawat-jalan` wajib / pemilik `rawat-jalan` | ✅ [Laporan](../task/report/backend/BE-RWI-105.md) |

---

## Kartu task

### `BE-RWI-088` — Penjaga penulis klinis: penulis tunggal dan penugasan pada waktu klinis

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 16 September 2026 — [laporan](../task/report/backend/BE-RWI-088.md); validasi source/QBE, `dotnet build` tidak dijalankan atas instruksi pemilik |
| **Gelombang** | 1 — `DOK-V2-0` |
| **Migration** | `R1` — tanpa perubahan bentuk data |

**Bisnis prosesnya.** Konsep catatan klinis adalah tulisan yang belum selesai. Yang boleh
menyelesaikannya hanya orang yang menulisnya — bukan DPJP, bukan supervisor, bukan siapa pun.
Alasannya bukan soal hak akses, melainkan soal isi: menyelesaikan konsep orang lain berarti
menandatangani kalimat yang bukan hasil pemeriksaan Anda.

Aturan kedua menyangkut **waktu**. Dokumen baru menuntut penugasan yang berlaku pada **waktu klinis**
dokumen itu, dan penugasan itu masih aktif saat dokumen disimpan. Ini yang membedakan "dokter yang
memang merawat pasien Selasa malam" dari "dokter mana pun yang kebetulan bisa membuka sistem hari
Kamis".

**Contoh.** dr. Yoga menulis SOAP Budi pukul 02.00 Selasa dan menyimpannya sebagai konsep. dr.
Ahmad, DPJP Budi, membuka konsep itu dan mencoba menyelesaikannya → `403`. Contoh kedua: catatan
bertanggal klinis 05.00 dikirim pukul 08.10 padahal penugasan dr. Yoga berakhir 07.00 → `403`. Pukul
08.40 kepala ruangan membuatkan penugasan singkat, dan kiriman yang sama → `201`.

**Cakupan yang diharapkan.** `R1` menyentuh **kode saja**, nol perubahan bentuk data. Impact scan
menemukan penjaga ini tidak ditegakkan pada **8 dari 9** titik panggil resolver konteks klinis —
delapan-delapannya masuk cakupan, bukan satu controller saja.

**Acceptance criteria.**

1. Hanya penulis yang menyunting dan menyelesaikan konsep rawat inap; DPJP aktif pun `403` — `FR-DOK-075`.
2. Dokumen baru menuntut penugasan yang berlaku pada waktu klinis dokumen **dan** aktif saat disimpan — `FR-DOK-076`.
3. Penjagaan itu berlaku pada **seluruh** titik panggil resolver konteks klinis rawat inap, bukan sebagian.
4. Jalur poliklinik tidak berubah perilakunya.
5. Regresi: dokter berpenugasan normal tetap dapat menulis dan menyelesaikan dokumennya sendiri.

**Bukti verifikasi.** `dotnet restore` dan `dotnet build`; review diff dan scope yang menyebut
berapa titik panggil disentuh; verifikasi proses bisnis untuk kedua contoh di atas.

**Definition of Done.** Laporan tracked ada di `task/report/backend/BE-RWI-088.md` dan menyebut
jumlah titik panggil yang diperbaiki; roadmap dan `requirement-traceability-v2.md` diperbarui.

---

### `BE-RWI-089` — Verifikasi CPPT hanya oleh DPJP aktif

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 16 September 2026 — [laporan](../task/report/backend/BE-RWI-089.md); validasi source/QBE, `dotnet build` tidak dijalankan atas instruksi pemilik |
| **Gelombang** | 1 — `DOK-V2-0` |
| **Migration** | `R1` |

**Bisnis prosesnya.** Verifikasi CPPT adalah pernyataan DPJP bahwa catatan profesi lain sudah ia
baca dan setujui. Kalau konsulen atau dokter jaga bisa memverifikasi, pernyataan itu kehilangan
arti. Peran DPJP dinilai **pada detik verifikasi**, bukan pada saat catatan ditulis — karena DPJP
bisa berganti di tengah episode.

**Contoh.** dr. Sari konsulen Budi menekan tombol Verifikasi pada catatan perawat → `403`. dr. Ahmad
yang DPJP menekan tombol yang sama → berhasil.

**Acceptance criteria.**

1. Verifikasi CPPT hanya diterima dari penugasan berperan **DPJP** yang aktif pada detik verifikasi.
2. Konsulen dan dokter jaga menerima `403` — `FR-DOK-082`.
3. Dokter yang dulu DPJP tetapi sudah digantikan menerima `403`.
4. Membaca CPPT tetap terbuka bagi seluruh dokter berpenugasan; yang dibatasi hanya verifikasinya.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian verifikasi CPPT; verifikasi
proses bisnis.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-090` — Tindakan baru pada episode tertutup ditolak

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 16 September 2026 — [laporan](../task/report/backend/BE-RWI-090.md); validasi source/QBE, `dotnet build` tidak dijalankan atas instruksi pemilik |
| **Gelombang** | 1 — `DOK-V2-0` |
| **Migration** | `R1` |

**Bisnis prosesnya.** Episode yang sudah ditutup bersifat hanya-baca. Sekarang jalur catatan dokter
masih bisa melahirkan tindakan baru pada episode `Closed` — artinya tindakan medis tercatat terjadi
pada perawatan yang sudah selesai, dan itu bisa ikut tertagih.

**Acceptance criteria.**

1. Permintaan membuat tindakan baru dari catatan dokter pada episode `Closed` ditolak `422`.
2. Pesan penolakannya menyebut alasannya, bukan galat umum.
3. Episode berstatus selain `Closed` tidak berubah perilakunya.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-091` — Registrasi keutuhan sejak konsep (migration R2)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 16 September 2026** — [laporan](../task/report/backend/BE-RWI-091.md); validasi source/QBE; **`dotnet build` PASS 17 September 2026**; merujuk `RWI-DEC-151` |
| **Gelombang** | 2 — `DOK-V2-1` |
| **Migration** | `R2` — kode saja; registrasi `Draft` yang telanjur terbentuk **tidak dihapus** saat mundur |

**Bisnis prosesnya.** Mesin keutuhan dokumen selama ini baru mencatat dokumen pada saat
ditandatangani. Akibatnya konsep tidak punya identitas: tidak ada yang bisa ditunjuk saat episode
ditutup, dan tidak ada yang bisa dicari dokter di "Catatan Saya". `R2` memindahkan pendaftaran itu
ke **saat konsep pertama kali disimpan**.

**Contoh.** dr. Yoga mengetik SOAP; simpan otomatis berjalan dua kali pukul 06.31. Yang terbentuk
**satu** registrasi `Draft`, bukan dua. Pukul 08.00 ia menandatanganinya — registrasi yang sama naik
menjadi `Signed`, dengan waktu klinis 06.30 dan waktu tanda tangan 08.00 tersimpan **terpisah**.

**Acceptance criteria.**

1. Konsep SOAP dan kajian medis rawat inap membentuk **tepat satu** registrasi `Draft` sejak simpan pertama — `FR-DOK-074`.
2. Simpan otomatis berulang tidak menggandakan registrasi; `RegisterAsync` idempoten.
3. Tanda tangan memakai registrasi yang sama, tidak membuat yang baru.
4. Penulis dapat menyelesaikan konsepnya **setelah penugasannya berakhir**, sepanjang konsep itu dibuat saat penugasan masih aktif — `FR-DOK-077`.
5. Waktu klinis dan waktu tanda tangan tersimpan sebagai dua nilai terpisah.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis; uji idempoten dengan dua kiriman
simpan otomatis berturut-turut, keluarannya ditempel ke laporan.

**Definition of Done.** ~~Pemberitahuan kepada Yoga Aji Pratama sebagai pemilik mesin keutuhan
tercatat pada laporan task~~ — **butir ini sudah terpenuhi lebih awal**: Yoga Aji Pratama menyetujui
registrasi keutuhan sejak konsep pada 16 September 2026, tercatat `RWI-DEC-151`. Laporan task cukup
**merujuk `RWI-DEC-151`**. Laporan tracked ada; roadmap dan traceability diperbarui.

**Yang persetujuan itu tidak berikan.** Wewenang menulis source pada mesin keutuhan tetap dinyatakan
terpisah saat task ini dikirim ke builder.

---

### `BE-RWI-092` — Daftar `my-authored` untuk Catatan Saya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 2026-09-16** — validasi source/QBE; `dotnet build` tidak dijalankan atas instruksi pemilik. ~~⛔ menunggu persetujuan Yoga Aji Pratama~~ — gerbang tertutup lewat `RWI-DEC-151` |
| **Gelombang** | 3 — `DOK-V2-1` |

**Bisnis prosesnya.** `RWI-DEC-111` membuat kartu pasien hilang dari daftar dokter begitu
penugasannya berakhir. Itu benar untuk akses pasien, tetapi menutup satu hal yang sah: dokter
mengoreksi catatannya sendiri. `RWI-DEC-127` membuka jalan sempit — daftar "Catatan Saya" yang
**hanya** memuat catatan buatan dokter login, dengan identitas pasien seminimum mungkin.

**Contoh.** dr. Yoga jaga malam Selasa dan salah ketik dosis pada SOAP pukul 02.00. Penugasannya
berakhir 07.00. Kamis ia membuka "Catatan Saya", menemukan SOAP itu, dan menambah addendum. Ia
**tidak** bisa membuka CPPT, resep, atau data Budi yang lain.

**Acceptance criteria.**

1. Daftar hanya memuat registrasi yang penulisnya adalah pengguna login.
2. Setiap baris membawa identitas pasien **minimum** — cukup untuk mengenali, tidak lebih.
3. Daftar memuat konsep `Draft` maupun catatan `LockedUnsigned` dan `Signed` milik penulis.
4. Daftar dikelompokkan di bawah episode rawat inap.
5. Endpoint ini **tidak** membuka jalur baca apa pun ke data pasien selain catatan milik penulis.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis yang secara
khusus mencoba mengakses data pasien lain lewat endpoint ini dan memastikan tertutup.

**~~Blocker~~ — tertutup 16 September 2026.** `{GATE-YOGA}` sudah dijawab: Yoga Aji Pratama
menyetujui penambahan `my-authored` dan `serviceContext` pada mesin keutuhan miliknya, tercatat
`RWI-DEC-151`. Butir terbuka `04-prd-to-mvp.md` 22.20 nomor 10 ikut tertutup.

**Yang persetujuan itu tidak berikan.** Ia menyetujui **perubahannya**, bukan pelaksanaannya.
Wewenang menulis source pada mesin keutuhan tetap dinyatakan terpisah saat task ini dikirim ke
builder. Batas cakupan pada kriteria 5 juga tidak longgar karena persetujuan ini: endpoint ini tetap
**tidak boleh** membuka jalur baca ke data pasien selain catatan milik penulis.

**Definition of Done.** Laporan tracked ada dan merujuk `RWI-DEC-151`; roadmap dan traceability
diperbarui.

---

### `BE-RWI-093` — Addendum oleh penulis asli tanpa penugasan aktif

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 2026-09-16** — `EXISTING / REUSE`; validasi source, tanpa `dotnet build` atas instruksi pemilik |
| **Gelombang** | 3 — `DOK-V2-1` |

**Bisnis prosesnya.** Kesalahan pada catatan klinis harus bisa dikoreksi oleh penulisnya. Mesin
addendum sudah mengenali penulis asli sebagai pihak yang berwenang (`RWI-FACT-013`). Yang perlu
dipastikan: kewenangan itu **tidak ikut hilang** ketika penugasan dokter berakhir atau episode
ditutup. `RWI-DEC-127` menetapkan ini sebagai pengecualian sempit — hanya addendum, hanya catatan
milik sendiri, dan **bukan** izin menulis catatan baru.

**Acceptance criteria.**

1. Penulis asli menambah addendum pada catatan `Signed` atau `LockedUnsigned` miliknya walau penugasannya sudah berakhir.
2. Penulis asli menambah addendum walau episode sudah `Closed`.
3. Dokter lain — termasuk DPJP aktif — **tidak** dapat menambah addendum pada catatan yang bukan miliknya lewat jalur ini.
4. Jalur ini **tidak** mengizinkan pembuatan catatan baru pada episode tertutup.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis yang menguji keempat kriteria,
termasuk yang ditolak.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-094` — Jenis catatan pada CPPT (migration R3)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 2026-09-16** — migration R3 dibuat tetapi tidak dijalankan; tanpa `dotnet build` atas instruksi pemilik |
| **Gelombang** | 2 — `DOK-V2-1` |
| **Migration** | `R3` — kolom `NoteKind` pada `TrxPatientIntegratedProgressNote`, milik `ClinicalManagement` |

**Bisnis prosesnya.** CPPT berisi catatan dari banyak profesi. Tanpa penanda jenis, lini masanya
tidak bisa disaring, dan perawat yang menulis SOAP keperawatan tidak bisa dibedakan dari dokter yang
menulis catatan perkembangan. `NoteKind` menjadi penanda itu, dan diperiksa terhadap **profesi
penulis** supaya perawat tidak menyimpan catatan berjenis dokter.

**Yang sengaja tidak dilakukan.** Entri lama diberi nilai `Unspecified` dan **dibiarkan begitu**.
Menebak jenis catatan lama dari isinya adalah pemalsuan data klinis.

**Acceptance criteria.**

1. Kolom `NoteKind` ada pada `TrxPatientIntegratedProgressNote`.
2. Jenis yang dikirim diperiksa terhadap profesi penulis; kombinasi yang tidak sah ditolak.
3. Seluruh entri lama bernilai `Unspecified`, tanpa pengisian tebakan.
4. Lini masa dapat disaring berdasarkan jenis catatan lewat kontrak `0.6.0`.

**Bukti verifikasi.** `dotnet build`; verifikasi skema pada Postgres sekali pakai; verifikasi
kontrak API.

**Catatan lintas sub-modul.** Kolom ini dipakai `keperawatan` lewat `FR-KEP-077` — SOAP dan Catatan
Keperawatan disimpan sebagai CPPT berjenis `NursingSoap` dan `NursingNarrative`. Bentuk enum-nya
dikunci di sini, bukan di sana.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-095` — Verifikasi CPPT pada episode yang sudah ditutup

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 2026-09-16** — validasi source/QBE; tanpa `dotnet build` atas instruksi pemilik |
| **Gelombang** | 3 — `DOK-V2-1` |

**Bisnis prosesnya.** Episode ditutup, tetapi ada entri CPPT yang belum sempat diverifikasi. Kalau
verifikasi ikut tertutup, entri itu menggantung selamanya. `RWI-DEC-126` membuka pengecualian:
**DPJP terakhir** episode itu tetap boleh memverifikasi entri yang tertinggal, lewat daftar pantau
verifikasinya. Pengecualian ini hanya untuk **verifikasi**, bukan untuk menulis catatan baru.
Keterlambatannya tetap tercatat apa adanya, dan DPJP **sebelumnya** tidak ikut mendapat hak ini.

**Contoh.** Entri perawat Sabtu 21.00 belum diverifikasi saat episode ditutup Minggu. Senin 16.00
DPJP terakhir memverifikasinya. Sistem mencatat verifikasi itu sah, **dan** mencatat keterlambatan
19 jam.

**Acceptance criteria.**

1. Pada episode `Closed`, DPJP terakhir dapat memverifikasi entri yang ditulis **sebelum** penutupan.
2. DPJP **sebelumnya** yang sudah digantikan tetap `403`.
3. Keterlambatan tetap tercatat dan tidak dihapus oleh pengecualian ini.
4. Pengecualian ini **tidak** membuka penulisan catatan baru pada episode tertutup.
5. Entri yang ditulis sesudah penutupan — bila ada — tidak masuk cakupan pengecualian.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis untuk kelima kriteria, termasuk yang
ditolak.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-096` — Daftar tunggu verifikasi memuat episode tertutup

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 2026-09-16** — validasi source/QBE; tanpa `dotnet build` atas instruksi pemilik |
| **Gelombang** | 4 — `DOK-V2-1` |

**Bisnis prosesnya.** Pengecualian pada `BE-RWI-095` tidak ada gunanya kalau DPJP tidak tahu entri
mana yang tertinggal. Daftar tunggu verifikasi karena itu ikut memuat episode `Closed` miliknya,
dan baru melepasnya setelah seluruh entri di dalamnya terverifikasi.

**Acceptance criteria.**

1. Daftar memuat episode `Closed` yang DPJP terakhirnya adalah pengguna login.
2. Episode keluar dari daftar setelah **seluruh** entrinya terverifikasi.
3. Episode `Closed` milik DPJP lain tidak muncul.
4. Entri yang lewat batas verifikasi ditandai `Overdue`.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian daftar tunggu verifikasi.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-097` — Pesanan tindakan dengan pemberi instruksi (migration R7)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026** — [Laporan](../task/report/backend/BE-RWI-097.md) |
| **Gelombang** | 2 — `DOK-V2-1` |
| **Migration** | `R7` — **mengubah perilaku endpoint yang dipakai poliklinik**; mundurnya tidak simetris |

**Bisnis prosesnya.** Di rawat inap, yang memasukkan pesanan tindakan biasanya perawat, atas
instruksi dokter yang sedang bertugas. Sistem sekarang mewajibkan `ConsultationId` — yang hanya ada
di alur poliklinik. Akibatnya perawat rawat inap tidak bisa memesan sama sekali. `R7` melonggarkan
kewajiban itu **khusus** untuk jalur rawat inap, sambil menambahkan kolom pemberi instruksi supaya
tetap jelas siapa dokter yang menyuruh.

**Contoh.** Perawat Rina memesan fisioterapi untuk Budi atas instruksi dr. Ahmad. Pesanan tersimpan
tanpa `ConsultationId`, dengan pemberi instruksi dr. Ahmad dan penginput Rina dari akun loginnya.
Di poliklinik, pesanan tanpa `ConsultationId` **tetap ditolak** dengan pesan yang sama seperti
sebelumnya.

**Acceptance criteria.**

1. Enam kolom baru ada pada `TrxPatientProcedure` sesuai `data-dictionary` `0.6.0`.
2. `ConsultationId` menjadi nullable, dan penjaganya berpindah ke tingkat jalur.
3. Perawat membuat pesanan rawat inap dengan dokter pemberi instruksi yang **sedang bertugas**; penginput diambil dari akun login — `FR-DOK-100`.
4. Pesanan **poliklinik** tanpa `ConsultationId` tetap ditolak dengan pesan lama — `FR-DOK-101`.
5. Pesanan hanya diubah oleh penginputnya; dibatalkan oleh penginput atau DPJP aktif, wajib beralasan — `FR-DOK-102`.
6. Regresi poliklinik hijau: alur pemesanan tindakan rawat jalan tidak berubah hasilnya.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API; **verifikasi regresi
poliklinik** dengan hasilnya ditempel apa adanya.

**Risiko dan kewajiban koordinasi.** `R7` adalah salah satu dari **dua** langkah `dokter-rawat-inap`
yang mengubah perilaku poliklinik. ~~Pemberitahuan kepada pemilik blueprint `rawat-jalan` wajib
dilakukan dan dicatat sebelum rilis~~ — **sudah terpenuhi**: **Sukma GP** menyetujui perubahan ini
pada 16 September 2026, tercatat `RWI-DEC-152`. Mundurnya tetap tidak simetris: mengembalikan
`ConsultationId` menjadi `NOT NULL` gagal bila sudah ada pesanan perawat tanpa konsultasi.

**Yang tidak ikut longgar.** Kriteria 6 **regresi poliklinik tetap wajib dijalankan sungguhan**.
Persetujuan Sukma GP menyatakan perubahannya boleh; ia tidak menggantikan bukti bahwa alur rawat
jalan masih bekerja.

**Definition of Done.** Laporan tracked merujuk `RWI-DEC-152`; regresi poliklinik hijau; roadmap dan
traceability diperbarui.

---

### `BE-RWI-098` — Verifikasi instruksi pesanan tindakan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 16 September 2026** — [laporan](../task/report/backend/BE-RWI-098.md); validasi source/QBE; **`dotnet build` PASS 17 September 2026** |
| **Gelombang** | 3 — `DOK-V2-1` |

**Bisnis prosesnya.** Pesanan yang dimasukkan perawat perlu dikonfirmasi bahwa instruksinya memang
dari dokter. Yang mengkonfirmasi adalah **dokter pemberi instruksi yang tertulis pada pesanan itu**
— bukan DPJP, bukan dokter lain. Konfirmasi itu tidak mengubah isi pesanan maupun siapa
penginputnya; ia hanya menambah status verifikasi.

**Acceptance criteria.**

1. Hanya dokter pemberi instruksi yang tercantum yang dapat memverifikasi — `FR-DOK-104`.
2. Verifikasi **tidak** mengubah penginput maupun isi pesanan.
3. Status verifikasi instruksi bergerak `NotRequired` → `Pending` → `Verified` sesuai `INV-DOK-17`.
4. Pelaksana tindakan menjadi penulis catatan pelaksanaan, dan addendum atasnya hanya oleh pelaksana — `FR-DOK-103`.
5. Dokter lain yang mencoba memverifikasi menerima `403`.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-099` — Resep Harian dan lima kolom butir resep (migration R4)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 16 September 2026** — [laporan](../task/report/backend/BE-RWI-099.md); validasi source/skema; **build PASS dan migration R4 diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** |
| **Gelombang** | 1 — `DOK-V2-2` |
| **Migration** | `R4` — lima kolom pada `PhmPrescriptionItem`, milik `PharmacyManagement` |

**Bisnis prosesnya.** Dokter yang visite pagi perlu tahu obat apa saja yang sedang berjalan hari
ini, termasuk racikan dan obat pulang. Sekarang informasi itu tersebar di beberapa layar. Resep
Harian mengumpulkannya dalam satu daftar bersaring periode.

**Endpoint bergaya Swagger.**

```yaml
GET /api/pharmacy-management/prescriptions/daily:
  summary: Resep harian satu pasien pada satu periode
  x-permission: "Prescription : Read"
  parameters:
    - { name: encounterId, in: query, required: true, schema: { type: string, format: uuid } }
    - name: period
      in: query
      schema: { type: string, enum: [Today, ThisWeek, ThisMonth, Range], default: Today }
    - { name: from, in: query, schema: { type: string, format: date }, description: "Wajib bila period=Range" }
    - { name: to,   in: query, schema: { type: string, format: date }, description: "Wajib bila period=Range" }
  responses:
    "200":
      description: Butir resep pada periode itu, termasuk racikan dan obat pulang
    "422": { description: "period=Range tanpa from atau to" }
```

**Acceptance criteria.**

1. Lima kolom baru ada pada `PhmPrescriptionItem` sesuai `data-dictionary` `0.6.0`.
2. Saring periode Hari Ini, Minggu Ini, Bulan Ini, dan Rentang bekerja sesuai definisinya.
3. Racikan dan obat pulang ikut tampil, tidak tersaring keluar.
4. `period=Range` tanpa `from` atau `to` ditolak `422`.
5. Migration mundur menghapus kolom selama tabel belum berisi data pasien sungguhan.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-100` — Penghentian butir resep membatalkan dosis MAR

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 17 September 2026.** Kelima acceptance criteria terpetakan ke source. Endpoint `PATCH /items/{itemId}/stop` dipasang pada `PrescriptionController`. Validasi `VAL-DOK-51` (penugasan dokter aktif), `VAL-DOK-51a` (alasan wajib), `VAL-DOK-51b` (penolakan butir sudah dihentikan), dan status episode Closed/Cancelled (`422`) ditegakkan. Pembatalan seluruh dosis MAR `Due` setelah waktu penghentian (`CancelDueDosesForItemAsync`) dan penghentian order sliding scale aktif (`StopForPrescriptionItemAsync`) terintegrasi dalam **satu transaksi database atomik** dengan rollback otomatis. `dotnet build` **NOT RUN** atas instruksi eksplisit pemilik (build mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-100.md) |
| **Gelombang** | 2 — `DOK-V2-2`, dirilis bersama `KEP-V2-2` |

**Bisnis prosesnya.** Dokter menghentikan sebuah obat. Kalau dosis yang sudah terjadwal di MAR tidak
ikut dibatalkan, perawat tetap melihatnya sebagai dosis yang harus diberikan — dan obat yang sudah
dihentikan tetap masuk ke pasien. Karena itu penghentian dan pembatalan dosis terjadi **dalam satu
transaksi**.

**Contoh.** Ceftriaxone dihentikan pukul 09.10. Dosis pukul 20.00 yang masih `Due` menjadi
`Cancelled`. Dosis pukul 08.00 yang sudah `Administered` tetap utuh — itu fakta pemberian obat yang
sudah terjadi.

**Acceptance criteria.**

1. Penghentian butir wajib beralasan, dan hanya oleh dokter yang sedang bertugas.
2. Seluruh dosis `Due` **setelah** waktu penghentian menjadi `Cancelled` dalam transaksi yang sama.
3. Dosis `Administered`, `Held`, `Refused`, dan `Missed` tidak disentuh.
4. Butir yang sudah dihentikan **tidak dapat dihidupkan kembali**.
5. Galat buatan pada langkah mana pun → nol perubahan tersimpan.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis; uji galat buatan pada Postgres
sekali pakai.

**Definition of Done.** Task ini **tidak boleh** dimulai sebelum `BE-RWI-114` [BE-KEP] mendarat.
Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-101` — Rekonsiliasi obat bawaan (migration R5)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 16 September 2026** — [laporan](../task/report/backend/BE-RWI-101.md); validasi source/skema; **build PASS dan migration R5 diterapkan ke `QuilvianNewDevHamzah` 17 September 2026** |
| **Gelombang** | 2 — `DOK-V2-2` |
| **Migration** | `R5` — tabel rekonsiliasi dan jalur pendaftaran obat non-formularium |

**Bisnis prosesnya.** Pasien masuk membawa obat dari rumah. Setiap obat itu harus diputuskan
nasibnya oleh dokter: dilanjutkan apa adanya, dilanjutkan dengan perubahan, atau dihentikan.
Keputusan yang tidak diambil sama berbahayanya dengan keputusan yang salah — obat jantung yang
terlewat bisa fatal.

**Contoh.** Budi membawa tiga obat. dr. Ahmad memutuskan: Amlodipine "Lanjut Sama", Metformin
"Lanjut Ubah" menjadi setengah dosis, Vitamin "Hentikan". Dua keputusan "Lanjut" membentuk butir
draft resep. Sebelum resep itu aktif, dr. Ahmad masih boleh mengganti keputusannya; riwayat
keputusan lamanya tetap tersimpan.

**Acceptance criteria.**

1. Tabel rekonsiliasi ada, beserta status `Pending`, `ContinueSame`, `ContinueModified`, `Stopped`.
2. Keputusan "Lanjut" membentuk butir **draft** resep, bukan resep aktif — `FR-DOK-092`.
3. Perawat yang mencoba memutuskan menerima `403`.
4. Keputusan dapat diganti selama butir hasilnya masih draft; riwayat keputusan lama tersimpan — `FR-DOK-093`.
5. Setelah resep aktif, penggantian keputusan ditolak `409`.
6. Obat bawaan yang tidak ada di formularium dapat didaftarkan lewat jalur non-formularium.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API; verifikasi proses
bisnis untuk keenam kriteria.

**Catatan lintas sub-modul.** Perawat **membaca** keputusan ini dari ruang kerjanya lewat
`FR-KEP-078`, dan mencatat obat bawaan. Yang perawat tidak boleh lakukan adalah memutuskannya.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-102` — Template dan versi protokol sliding scale (migration R6)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 17 September 2026** — [laporan](../task/report/backend/BE-RWI-102.md); validasi source/skema; **build PASS dan migration R6 diterapkan ke `QuilvianNewDevHamzah` 17 September 2026**. **`RWI-OQ-097` tetap terbuka** |
| **Gelombang** | 1 — `DOK-V2-2` |
| **Migration** | `R6` — tabel template dan order sliding scale, milik `PharmacyManagement` (`RWI-DEC-147`) |

**Bisnis prosesnya.** Sliding scale adalah aturan "kalau gula darah sekian, suntik insulin sekian
unit". Aturan itu tidak boleh diketik ulang per pasien, karena satu salah ketik berarti dosis
insulin yang salah. `RWI-DEC-146` menetapkan dua lapis: **template standar berversi yang disahkan**,
dan order per pasien yang boleh disesuaikan dengan alasan. Task ini membangun lapis pertama.

**Yang paling menentukan keselamatan.** Rentang pada satu versi harus menutup **seluruh** nilai yang
mungkin, tanpa tumpang tindih dan tanpa lubang. Rentang bertumpuk berarti satu nilai gula darah
menghasilkan dua dosis berbeda; rentang berlubang berarti ada nilai yang tidak menghasilkan dosis
sama sekali.

**Contoh penolakan.** Rentang 200–260 dan 250–299 ditolak karena bertumpuk di 250–260. Versi yang
dimulai dari 150 tanpa rentang di bawahnya ditolak karena nilai di bawah 150 tidak tertutup.

**Acceptance criteria.**

1. Tabel template dan versi protokol ada di `PharmacyManagement`, dengan status `Draft`, `Approved`, `Retired`.
2. Rentang pada satu versi divalidasi **tidak bertumpuk dan tidak berlubang** — `FR-DOK-094`.
3. Hanya **satu** versi `Approved` per template pada satu waktu.
4. Pengesah versi **bukan** pengubah terakhirnya — `FR-DOK-095`.
5. Pengesahan memensiunkan versi sah sebelumnya pada **transaksi yang sama**.
6. Versi `Draft` tidak dapat dipakai membuat order.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API; verifikasi proses
bisnis termasuk kedua contoh penolakan di atas.

**Risiko yang terbuka — diperbarui 16 September 2026.** Manajemen rumah sakit sudah menyetujui
**pemakaian** sliding scale pada layanan pasien lewat `RWI-DEC-155`, sehingga butir 22.20 nomor 13
tertutup pada bagian kewenangannya. Yang **belum** ada adalah **nama** pengesah isi protokol, dicatat
sebagai **`RWI-OQ-097`**. Akibatnya konkret: mesin ini boleh dibangun dan diuji, tetapi selama nama
itu kosong **nol versi protokol dapat dinaikkan menjadi `Approved`**, karena kriteria 4 menuntut
pengesah yang berbeda dari pengubah terakhir dan tidak ada akun yang berhak.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui; `RWI-OQ-097`
dicatat sebagai masih terbuka.

---

### `BE-RWI-103` — Order sliding scale per pasien

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 17 September 2026** — [laporan](../task/report/backend/BE-RWI-103.md); validasi source/QBE; **`dotnet build` PASS 17 September 2026**. Jalur penghentian lewat butir insulin aktif setelah `BE-RWI-100` |
| **Gelombang** | 2 — `DOK-V2-2` |

**Bisnis prosesnya.** Order adalah penerapan template pada satu pasien. Rentangnya **disalin** dari
versi yang disahkan, bukan dirujuk — supaya template yang berubah besok tidak diam-diam mengubah
dosis pasien yang sedang berjalan. Dokter boleh menyesuaikannya, tetapi wajib menyebutkan alasan.

**Contoh.** dr. Ahmad membuat order untuk Budi dari template dewasa, lalu memotong dosisnya separuh
dengan alasan "pasien sensitif insulin". GDS 280 yang biasanya menghasilkan 6 unit kini menghasilkan
3 unit. Besoknya template versi baru disahkan — order Budi **tidak berubah**.

**Acceptance criteria.**

1. Order hanya dapat dibuat dari versi berstatus `Approved` — `FR-DOK-096`.
2. Rentang **tersalin** ke order, bukan dirujuk.
3. Penyesuaian dosis wajib beralasan, dan membuat **versi order baru**.
4. Versi template baru **tidak** mengubah order yang sudah berjalan — `FR-DOK-097`.
5. Kiriman yang memakai nomor versi order basi ditolak — `FR-DOK-098`.
6. Menghentikan order atau butir insulinnya menghentikan pelaksanaan berikutnya — `FR-DOK-099`.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis untuk contoh
di atas, termasuk pengesahan versi template baru.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-104` — Kolom instruksi pada pesanan Lab dan Radiologi (migration R8)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 17 September 2026** — [laporan](../task/report/backend/BE-RWI-104.md); validasi source/skema; **build PASS dan migration R8 diterapkan ke `QuilvianNewDevHamzah` 17 September 2026**; **regresi Lab/Rad NOT RUN**; merujuk `RWI-DEC-153` |
| **Gelombang** | 3 — `DOK-V2-3` |
| **Migration** | `R8` — empat kolom pada `LabOrder` dan `RadOrder`, **milik modul lain** |

**Bisnis prosesnya.** Sama seperti pesanan tindakan, pesanan laboratorium dan radiologi di rawat
inap juga dimasukkan perawat atas instruksi dokter. Supaya jelas siapa yang menyuruh dan apakah
instruksinya sudah dikonfirmasi, kedua tabel pesanan itu perlu membawa pemberi instruksi dan status
verifikasi.

**Acceptance criteria.**

1. Empat kolom instruksi ada pada `LabOrder` dan `RadOrder`.
2. Pesanan Lab/Rad dari rawat inap membawa pemberi instruksi dan status verifikasi — `FR-DOK-106`.
3. Alur pemesanan Lab/Rad dari modul lain tidak berubah perilakunya.
4. Migration mundur menghapus kolom selama bernilai bawaan.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API; regresi alur Lab/Rad
yang sudah ada.

**~~Blocker~~ — tertutup 16 September 2026.** `{GATE-LABRAD}` sudah dijawab: **Yoga Aji**, pemilik
`LaboratoryManagement` dan `RadiologyManagement`, menyetujui penambahan keempat kolom itu, tercatat
`RWI-DEC-153`. Butir terbuka `04-prd-to-mvp.md` 22.20 nomor 9 ikut tertutup.

**Yang tidak ikut longgar.** Kriteria 3 tetap mengikat: alur pemesanan Lab/Rad dari modul lain
**tidak boleh berubah perilakunya**, dan regresinya wajib dijalankan sungguhan. Persetujuan pemilik
membuka izin menambah kolom, bukan izin mengubah alur mereka. Wewenang menulis source dan migration
pada kedua modul itu juga tetap dinyatakan terpisah.

**Definition of Done.** Laporan tracked ada dan merujuk `RWI-DEC-153`; regresi Lab/Rad hijau;
roadmap dan traceability diperbarui.

---

### `BE-RWI-105` — Template resep milik dokter login (migration R9)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **Selesai 17 September 2026** — [laporan](../task/report/backend/BE-RWI-105.md); validasi source/QBE; **`dotnet build` PASS 17 September 2026**; **regresi poliklinik NOT RUN**; merujuk `RWI-DEC-152` |
| **Gelombang** | 2 — `DOK-V2-3` |
| **Migration** | `R9` — **mengubah perilaku poliklinik**; pemberitahuan pemilik `rawat-jalan` wajib |

**Bisnis prosesnya.** Template resep adalah kebiasaan meresepkan milik seorang dokter. Sekarang
kepemilikannya bisa dikirim dari luar, sehingga satu dokter bisa membuat template atas nama dokter
lain — dan template itu muncul di ruang kerja orang yang tidak pernah membuatnya. `R9` mengikat
kepemilikan ke **akun login**.

**Acceptance criteria.**

1. Template dibuat, diubah, dan dihapus **hanya** atas nama dokter login — berlaku umum termasuk poliklinik — `FR-DOK-088`.
2. Ruang kerja rawat inap hanya menampilkan dan memakai template milik dokter login; perawat tidak memakai template — `FR-DOK-089`.
3. Template kosong ditolak — `FR-DOK-091`.
4. Pemakaian template **menandai** butir yang bentrok alergi atau obatnya tidak tersedia, tanpa menggagalkan butir lain — `FR-DOK-090`.
5. Draft yang masih membawa butir bertanda ditolak saat disimpan.
6. Regresi poliklinik hijau: alur template rawat jalan tetap bekerja untuk pemiliknya sendiri.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis; **verifikasi regresi poliklinik**
dengan hasilnya ditempel apa adanya.

**Risiko dan kewajiban koordinasi.** `R9` adalah langkah **kedua** yang mengubah perilaku poliklinik.
~~Pemberitahuan kepada pemilik blueprint `rawat-jalan` wajib dilakukan dan dicatat sebelum rilis~~ —
**sudah terpenuhi** lewat `RWI-DEC-152`, pemberi persetujuan **Sukma GP**.

**Yang tidak ikut longgar.** Kriteria 6 **regresi poliklinik tetap wajib dijalankan sungguhan**.

**Definition of Done.** Laporan tracked merujuk `RWI-DEC-152`; regresi poliklinik hijau; roadmap dan
traceability diperbarui.

---

## Kebijakan verifikasi backend

Mengikuti `rules/backend/TEST_POLICY.md`:

- Backend **tidak memelihara project automated test**. Tidak adanya automated test **bukan** coverage gap dan tidak pernah menahan `✅`.
- Roadmap ini karena itu **tidak memuat** satu pun task "write unit tests", "integration tests", maupun pembuatan test project.
- Kata "regresi" pada roadmap ini berarti **verifikasi proses bisnis yang dijalankan sungguhan** pada alur yang sudah ada, bukan automated test suite.
- Bukti verifikasi yang dipetakan: QBE preflight dan conformance, review diff dan scope, `dotnet restore` serta `dotnet build`, verifikasi kontrak API, verifikasi proses bisnis, dan verifikasi runtime pada Postgres sekali pakai untuk task bermigration.
- Kesesuaian QBE dan aturan engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` repository backend target.

---

## Gerbang yang masih terbuka

| Gerbang | Jenis | Menahan | Siapa yang membukanya |
| --- | --- | --- | --- |
| ~~`{GATE-YOGA}` — `my-authored` dan `serviceContext`~~ | **TERTUTUP 2026-09-16** lewat `RWI-DEC-151` | ~~`BE-RWI-092`~~ — kini bebas | Yoga Aji Pratama ✅ |
| ~~`{GATE-LABRAD}` — kolom instruksi Lab/Rad~~ | **TERTUTUP 2026-09-16** lewat `RWI-DEC-153` | ~~`BE-RWI-104`~~ — kini bebas | **Yoga Aji** ✅ |
| ~~Pemberitahuan pemilik `rawat-jalan` atas `R7`~~ | **TERTUTUP 2026-09-16** lewat `RWI-DEC-152` | ~~Rilis `BE-RWI-097`~~ — **regresi poliklinik tetap wajib** | **Sukma GP** ✅ |
| ~~Pemberitahuan pemilik `rawat-jalan` atas `R9`~~ | **TERTUTUP 2026-09-16** lewat `RWI-DEC-152` | ~~Rilis `BE-RWI-105`~~ — **regresi poliklinik tetap wajib** | **Sukma GP** ✅ |
| Pengesah isi protokol sliding scale — **`RWI-OQ-097`** | Gerbang produksi, **sebagian tertutup** | Kewenangan memakai **sudah** diberikan `RWI-DEC-155`; yang belum: **nama** pengesah isi, tanpanya nol versi dapat dinaikkan `Approved` | Manajemen rumah sakit |
| `BE-RWI-079` [BE-INP] | Dependency lintas sub-modul | `BE-RWI-091`, `094`, `097` | Roadmap `episode-rawat-inap` |
| `BE-RWI-114` [BE-KEP] | Dependency lintas sub-modul | `BE-RWI-100` | Roadmap `keperawatan` |

---

## Traceability

Tabel penuh ada di [`requirement-traceability-v2.md`](./requirement-traceability-v2.md).
Empat puluh tiga FR revision `7` (`FR-DOK-069` s.d. `FR-DOK-111`) dipetakan di sana; yang
berdisposisi **Frontend** tidak punya task backend dan dicatat apa adanya sebagai baris frontend,
bukan sebagai gap.
