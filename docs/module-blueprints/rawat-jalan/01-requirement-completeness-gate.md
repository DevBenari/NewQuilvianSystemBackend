# Rawat Jalan Billing — Requirement Completeness Gate

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` |
| Decision revision | `10` |
| Kontrak target | `RJ-BIL-CONTRACT-001` versi `1.0.0` |
| Status keputusan | `OWNER_APPROVED` untuk Release 1 internal/manual |
| Assessment status | `PARTIALLY_READY` |
| Dinilai pada | `2026-08-20` |
| Backend source SHA | `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` |
| Frontend source SHA | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Hash keputusan | `sha256:115509A84A681646E800D7F6C3382345F31F79C13B2800B6727F356C680D4B0E` |
| Hash capability map | `sha256:A91E5EB7A507D8AF6A31B87782D84423B41C284F76CD748D01CFCB262C4213B4` |

## 1. Batas penilaian

Penilaian ini membatasi scope pada Release 1 internal/manual Rawat Jalan Billing. Scope ini
mencakup fakta layanan klinis yang menjadi sumber charge, billing folio per `EncounterId`,
alokasi multi-payer, tanggung jawab pasien, koreksi finansial, proyeksi finansial read-only di
Farmasi, idempotency, rekonsiliasi, serta workflow payer/claim manual.

Adapter eksternal bernama (misalnya AdMedika atau BPJS/JKN) tidak termasuk aktivasi Release 1.
Adapter tersebut hanya boleh dirancang sebagai interface dan tetap `disabled` sampai gate
produksi pada `RJ-BIL-GATE-DEC-009` terpenuhi.

## 2. Bukti yang dipakai

| Bukti | Kegunaan | Status |
|---|---|---|
| `00-interview-decisions.md`, revision 10 | Tujuan, ownership, lifecycle, invariant, acceptance criteria, dan release scope | `CONFIRMED` untuk intent Release 1; approval formal organisasi tetap menjadi bukti tata kelola terpisah |
| `01-existing-capability-map.md` | Keadaan source backend/frontend dan conflict pada SHA yang diaudit | `CONFIRMED` sebagai bukti AS-IS |
| Backend `9b26be3...` | Snapshot implementasi aktif | `CONFIRMED` |
| Frontend `ab4bd83...` | Snapshot consumer aktif | `CONFIRMED` |
| `indonesia-hospital-domain-reference` | Tidak dipakai pada assessment ini | `NOT_YET_AVAILABLE` dan tidak menjadi dasar kebijakan |

## 3. Temuan 18 dimensi kelengkapan

| Dimensi | Temuan Release 1 internal/manual | Klasifikasi | Dampak |
|---|---|---|---|
| Tujuan | Mengubah fakta layanan Rawat Jalan menjadi charge dan settlement yang dapat ditelusuri tanpa mencampur order klinis dengan status finansial | `CONFIRMED` | — |
| Aktor | Clinical unit, Lab, Radiology, Pharmacy, Billing/Revenue Cycle, Payer Management, Cashier, Finance, checker/supervisor, dan integration owner telah dibedakan | `CONFIRMED` | — |
| Pemicu/prasyarat | `EncounterId`, order/fact layanan, tariff/rule yang berlaku, serta folio harus tersedia; external adapter tidak menjadi prasyarat manual | `CONFIRMED` | — |
| Alur utama | Fact layanan diterima → divalidasi/idempotent → charge eligibility → charge/komponen → allocation → payment/claim manual → rekonsiliasi → penutupan folio | `CONFIRMED` | — |
| Exception | Cancel, partial service, repeat, rejected payer, timeout, duplicate, outage, pending approval, dan reconciliation case sudah didefinisikan | `CONFIRMED` | — |
| Data minimum | Encounter/source fact identity, versi, quantity/material, tariff snapshot, charge, allocation, actor, waktu, reason, correlation, dan audit | `CONFIRMED` | — |
| Aturan/validasi | Tidak boleh over-allocation; charge tepat satu kali; final charge mengikuti milestone; financial correction memakai workflow berwenang | `CONFIRMED` | — |
| Status/lifecycle | Lifecycle order, specimen, study, charge, approval, processing outcome, claim, settlement, dan reconciliation dipisahkan | `CONFIRMED` | — |
| Authorization | Pemilik domain dipisahkan; maker-checker wajib untuk risiko tinggi; clinical tidak boleh memutasi financial truth | `CONFIRMED` | Nilai threshold/matriks otorisasi final masih konfigurasi Finance |
| Dependency antarmodul | Registration/Encounter, Clinical, Pharmacy, Laboratory, Radiology, Billing, Payer, Cashier, Finance, Workflow, dan Security | `CONFIRMED` | — |
| Integrasi internal/eksternal | Internal contract dan manual workflow masuk Release 1; adapter eksternal hanya interface | `CONFIRMED` | Aktivasi eksternal tetap gated |
| Hasil akhir | Charge dan allocation tersimpan, payment/claim dapat direkonsiliasi, serta folio hanya ditutup bila prerequisite terpenuhi | `CONFIRMED` | — |
| Pembatalan/koreksi | Clinical cancellation menghasilkan fact; Billing menentukan void/adjustment/reversal/refund/FOC/write-off sesuai state | `CONFIRMED` | — |
| Audit/histori | History immutable, version/superseding reference, actor, reason, timestamp, policy, dan correlation dipertahankan | `CONFIRMED` | — |
| Notifikasi | Warning/escalation untuk SLA dan exception boleh dipakai; notifikasi tidak memberi approval atau bypass | `CONFIRMED` | Kanal notifikasi detail belum menjadi blocker domain |
| Dampak billing | Milestone, actual consumption, tariff snapshot, allocation, patient responsibility, settlement, dan correction telah ditentukan | `CONFIRMED` | Formula/threshold yang belum dikonfigurasi tidak boleh ditebak |
| Keselamatan klinis | Identity verification, specimen safety, radiology safety gate, dan urgensi dispensing dipisahkan dari pembayaran | `CONFIRMED` | SOP klinis detail tetap milik clinical governance |
| Pelaporan/traceability | Setiap outcome dapat ditelusuri dari encounter ke source fact, charge, allocation, payment/claim, dan reconciliation case | `CONFIRMED` | — |

## 4. Bukti dan gap material

### 4.1 `CONFIRMED`

- `RJ-BIL-GATE-DEC-001` sampai `RJ-BIL-GATE-DEC-009` telah dipromosikan menjadi
  `OWNER_APPROVED` untuk `RJ-BIL-CONTRACT-001` versi `1.0.0`.
- Release 1 secara eksplisit internal/manual; tidak menyatakan production activation adapter
  eksternal.
- Billing adalah pemilik financial truth. `EncounterId` hanya correlation/aggregation key.
- Existing source belum dianggap target contract. Conflict pada Pharmacy, payment source
  encounter, dan capability Lab/Radiology menjadi bukti pekerjaan downstream.

### 4.2 `PROPOSED`

| Butir | Alasan belum `CONFIRMED` | Dampak |
|---|---|---|
| Vocabulary endpoint, entity, event, dan nama status final | Keputusan mengunci makna bisnis tetapi sengaja belum mengunci nama teknis | `NON_BLOCKING_STANDARD` untuk domain design; harus dikunci pada kontrak hilir |
| Kanal notifikasi/escalation | Kebutuhan warning sudah ada, kanal belum ditetapkan | `CONFIGURABLE_DEFAULT` |

### 4.3 `MISSING`

| Butir | Owner | Dampak |
|---|---|---|
| Nilai threshold dan matriks approval Finance per unit/risiko | Finance/Security | `CONFIGURABLE_DEFAULT`; operasi high-risk fail-closed tanpa policy valid |
| SOP klinis detail untuk safety gate Lab/Radiology dan urgent dispensing | Clinical Governance/Lab/Radiology/Pharmacy | `NON_BLOCKING_STANDARD` untuk boundary domain, tetapi memblokir konfigurasi operasional tertentu |
| Kontrak sistem eksternal, credential, sandbox/UAT, dan support escalation | Payer/Insurance + Integration | `BLOCKING` hanya untuk aktivasi adapter eksternal; tidak memblokir manual Release 1 |

### 4.4 Konflik AS-IS terhadap target

Tidak ada `CONFLICT` requirement yang belum terselesaikan pada decision revision `10`.
Keputusan multi-payer, pemisahan financial ownership, serta scope Release 1 sudah
`OWNER_APPROVED`. Identifier berikut tetap dipertahankan karena source yang diaudit belum
sesuai dengan target. Jadi status ini adalah konflik implementasi/dependency, bukan alasan untuk
mengganti keputusan bisnis yang sudah disetujui.

| Identifier | Ketidaksesuaian source | Status bukti | Disposisi | Dampak fase |
|---|---|---|---|---|
| `RJ-BIL-CONFLICT-001` | Payment source encounter masih one-to-one dan enum aktif hanya `Cash/Insurance`, sedangkan target memakai folio multi-payer | `CONFLICT` AS-IS | Target requirement sudah terselesaikan melalui `RJ-BIL-REQ-X001`; perlu adapter/migrasi pada fase implementasi | Tidak memblokir domain design; memblokir reuse langsung dan migrasi tanpa desain |
| `RJ-BIL-CONFLICT-005` / `RJ-BIL-CONFLICT-006` | Pharmacy masih dapat menandai `Paid`, `InsuranceApproved`, `PaymentWaived`, atau status billing dari flow klinis | `CONFLICT` AS-IS | Pertahankan sebagai compatibility boundary terbatas; arahkan financial mutation ke Billing/Payer; deprecation bertahap | Tidak memblokir domain design; memblokir reuse endpoint legacy sebagai source of truth |
| `RJ-BIL-CONFLICT-003` | Source frontend/backend tidak membuktikan journey order operasional Lab/Radiology yang diklaim tersedia | `CONFLICT` klaim-vs-source | Klasifikasikan capability sebagai `EXTEND`/`MISSING`; jangan mengklaim reuse | Tidak memblokir domain design target; memblokir reuse capability existing |

Konsekuensi praktis: `CONFLICT` pada tabel di atas tetap menjadi dependency untuk
`hospital-domain-architect` dan `design-business-module`, tetapi bukan `BUSINESS_DECISION_REQUIRED`
untuk slice internal/manual yang sudah disetujui.

## 5. Kesiapan per slice

| Slice | Kesiapan | Batas |
|---|---|---|
| Clinical fact handoff: prescription, procedure, Lab acceptance, Radiology acquisition | `READY_FOR_DOMAIN_DESIGN` | Domain hanya mendefinisikan fact/milestone; tidak membuat financial status |
| Billing folio, charge component, tariff snapshot, patient responsibility | `READY_FOR_DOMAIN_DESIGN` | Detail formula yang tidak disetujui tetap configurable dan versioned |
| Multi-payer allocation dan manual settlement | `READY_FOR_DOMAIN_DESIGN` | External adjudication tidak diasumsikan berhasil |
| Financial correction, maker-checker, close/reopen, reconciliation | `READY_FOR_DOMAIN_DESIGN` | Threshold policy harus fail-closed bila belum tersedia |
| Pharmacy clinical fulfillment + read-only financial projection | `READY_FOR_DOMAIN_DESIGN` | Endpoint legacy financial mutation bukan reuse langsung |
| Internal payer/manual authorization, claim, adjudication, settlement | `READY_FOR_DOMAIN_DESIGN` | Manual outcome harus dilabeli `ManualOperator` |
| Named external adapters dan production activation | `BUSINESS_DECISION_REQUIRED` | `RJ-BIL-GATE-DEC-009`; berhenti sampai contract/security/UAT/reconciliation gate lengkap |

Kesiapan modul secara keseluruhan adalah `PARTIALLY_READY`: seluruh core internal/manual dapat
masuk arsitektur domain, sedangkan adapter eksternal tetap terblokir secara terisolasi.

## 6. Apa yang boleh berjalan

1. Handoff slice internal/manual ke `hospital-domain-architect`.
2. Penyusunan bounded context, ownership, aggregate, lifecycle, audit, billing impact, dan
   safety boundary berdasarkan decision revision 10.
3. Desain interface adapter eksternal tanpa mengaktifkan sistem tertentu.

## 7. Apa yang harus berhenti

1. Jangan menganggap capability map sebagai bukti bahwa billing, Lab, Radiology, atau payer
   sudah tersedia di source.
2. Jangan memakai endpoint Pharmacy yang menandai `Paid`, `InsuranceApproved`, atau
   `PaymentWaived` sebagai financial source of truth.
3. Jangan mengaktifkan adapter eksternal tanpa gate produksi `RJ-BIL-GATE-DEC-009`.
4. Jangan menurunkan entity atau endpoint final langsung dari layar, menu, atau nama task.

## 8. Handoff berikutnya

Handoff yang tepat adalah `hospital-domain-architect` untuk scope berikut:

`RJ-BIL-BP-001`, revision keputusan `10`, `RJ-BIL-CONTRACT-001@1.0.0`, backend SHA
`9b26be3...`, frontend SHA `ab4bd83...`, requirement readiness `PARTIALLY_READY`, dengan
slice internal/manual yang masing-masing berstatus `READY_FOR_DOMAIN_DESIGN`.

Keluaran yang diminta: arsitektur domain berbasis bukti, bukan source code, migration, endpoint,
atau ERD implementasi. `design-business-module` dapat menyusun artefak hilir untuk slice yang
secara eksplisit berstatus `DOMAIN_ARCHITECTURE_READY` atau slice siap yang independen dari
`DOMAIN_ARCHITECTURE_PARTIAL`. Pada assessment ini, core internal/manual adalah slice independen
tersebut; aktivasi adapter eksternal tetap tidak boleh masuk desain implementasi.
