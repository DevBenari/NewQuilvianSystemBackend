# Integration Contract — Rawat Jalan Billing

> **Revisi `27` (28 September 2026):** amendment **V2 — Rawat Jalan ke invoice canonical** ada di bagian akhir berkas ini dan **berlaku** untuk jalur fakta klinis → tagihan. Isi di atasnya adalah riwayat desain `RJ-BIL`.

| Field | Nilai |
|---|---|
| Contract version | `RJ-BIL-INT-001@1.0.0` |
| Status | `draft` |
| Owner | Billing Integration + domain owners |
| External activation | `BLOCKED` oleh `RJ-BIL-DEP-009` |

## Ownership boundary

Kontrak ini adalah **satu-satunya** jalur resmi dari klinis ke finansial. Ia juga menjadi garis
Definition of Done: apa yang ada di sisi kiri milik Dokter / Clinical, apa yang ada di sisi kanan
milik Billing.

```text
DOCTOR / CLINICAL                        BILLING / REVENUE CYCLE
(producer)                               (consumer)

Selesai Konsultasi
Prescription finalize
Procedure execute            ====>       RecognizeMilestoneAsync
Lab specimen accepted      KONTRAK       Folio . Charge . Tariff
Radiology acquisition         INI        Payer . Payment . Claim
        |                                        |
        v                                        v
TrxClinicalMilestoneFact                 BilFolio, BilChargeLine,
(durable, versioned,                     BilChargeComponent,
 idempotent)                             BilProcessingEffect
```

**Yang dijamin producer.** Untuk setiap **eligible** clinical milestone: identitas fakta yang
stabil, versi yang monoton, idempotency key, snapshot rujukan, fakta ditulis **sebelum** dispatch
sehingga kegagalan dispatch tidak menghilangkannya, fakta yang belum terkirim dapat ditemukan
kembali, dan retry producer memakai identity yang sama sehingga **tidak menggandakan fakta logis**.

**Yang bukan tanggung jawab producer:** nominal akhir, tarif, alokasi penanggung, tanggungan
pasien, status pembayaran, klaim, settlement, rekonsiliasi finansial, dead-letter finansial,
recovery report Billing, dan **pencegahan duplikasi charge**. Producer tidak boleh menghitung atau
menetapkan satu pun di antaranya.

**Yang wajib dilakukan consumer:** **consumer-side idempotency**. Producer menjamin identitas dan
versi yang stabil; menjamin bahwa satu identitas tidak menghasilkan dua charge adalah kewajiban
consumer, memakai `IdempotencyKey`, `MilestoneFactId`, dan `MilestoneFactVersion` yang diterimanya.

**Yang bukan tanggung jawab consumer:** kebenaran klinis. Consumer tidak boleh menolak,
membatalkan, atau menunda penyelesaian konsultasi.

### Eligibility — nol fakta tidak selalu berarti kesalahan

Aturan yang berlaku adalah **per eligible milestone**, bukan per konsultasi:

```text
untuk SETIAP eligible clinical milestone
        -->  tepat satu fakta logis yang durable, ber-versi
```

Konsekuensinya wajib dibaca kedua pihak:

| Keadaan | Verdict |
|---|---|
| Konsultasi selesai **tanpa** eligible milestone — tanpa resep, tanpa tindakan, tanpa order penunjang | **`VALID`.** Nol fakta adalah hasil yang benar. Bukan galat, bukan gap |
| Konsultasi selesai **dengan** eligible milestone, fakta seharusnya terbit tetapi tidak ada | **`RECOVERABLE PRODUCER GAP`.** Dapat ditemukan dan dikirim ulang producer |
| Konsultasi selesai, fakta terbit, consumer belum memprosesnya | Urusan consumer. Bukan gap producer |

Consumer **tidak boleh** menyimpulkan adanya kesalahan hanya karena sebuah konsultasi menjadi
`COMPLETED` tanpa disertai fakta. Tidak setiap konsultasi menghasilkan resep, tindakan, atau
pemeriksaan penunjang, dan memaksakan fakta hanya agar Billing menerima sesuatu akan menciptakan
charge yang tidak pernah terjadi secara klinis.

**Arah kegagalan.** Kegagalan Billing **tidak boleh** membatalkan clinical completion yang sudah
committed. Aturan ini ditegakkan secara teknis, bukan hanya didokumentasikan:
`ClinicalMilestoneFactProducer` melempar `InvalidOperationException` bila dipanggil di dalam
transaksi klinis yang masih terbuka. Pemanggil wajib commit lebih dulu, baru menerbitkan fakta.
Kegagalan penyerahan dikembalikan sebagai keterangan — bukan sebagai pembatalan konsultasi.

> ## ✅ Sisi producer sudah dibekukan — `2026-08-31`
>
> | Gate | Kontrak | Status |
> |---|---|---|
> | `RJ-DOC-INT-001` Completion Contract | `RJ-DOC-COMPLETION-001@1.0.0` | **`FROZEN`** |
> | `RJ-DOC-INT-002` Producer Handoff Contract | `RJ-DOC-HANDOFF-001@1.0.0` | **`FROZEN`** |
>
> Artefaknya: [doctor-consultation-contracts.md](doctor-consultation-contracts.md).
> Keputusan owner: `RJ-DOC-DEC-006`.
>
> **Yang wajib dibaca consumer Billing dari kontrak beku itu:**
>
> 1. Aturan handoff berlaku **per eligible clinical milestone**, bukan per konsultasi.
>    Konsultasi tanpa eligible milestone menghasilkan **nol fakta**, dan itu **sah**. Aturan
>    `every consultation must have a fact` **dilarang**.
> 2. Eligibility mandatory saat ini hanya **`Prescription finalization`** dan
>    **`Procedure execution`**. Lab dan Radiologi berstatus `CONDITIONAL` (`RJ-DOC-DEC-002`).
> 3. **Consumer wajib menerapkan consumer-side idempotency.** Producer menjamin identitas dan versi
>    stabil; `charge deduplication` **bukan** jaminan producer.
> 4. Consumer tidak boleh menolak, membatalkan, atau menunda penyelesaian konsultasi.
>
> Perlu diketahui bahwa pada source yang diaudit `2026-08-31`, fakta resep **belum pernah terbit
> sama sekali** karena finalisasi konsultasi tidak pernah tercapai dari alur dokter. Itu adalah
> `RECOVERABLE PRODUCER GAP` milik `RJ-DOC-BE-001` dan `BE-005`, bukan cacat consumer. Rinciannya
> pada [../roadmap/doctor-consultation-roadmap.md](../roadmap/doctor-consultation-roadmap.md)
> bagian `2.1` dan `3`.

## Internal clinical fact contract

Produsen: Clinical, Pharmacy, Laboratory, Radiology. Consumer: Billing Integration.

Minimum identity: `SourceContext`, `SourceAggregateId`, optional `SourceItemId`,
`MilestoneFactId`, `MilestoneFactVersion`, `EncounterId`, `EffectType`, `OccurredAt`,
`CorrelationId`, `CausationId`, dan `IdempotencyKey`.

Processing harus idempotent. Retry infrastructure memakai key/version yang sama. Correction
source memakai version baru. Timeout menjadi `OutcomeUnknown`; tidak boleh diasumsikan gagal atau
berhasil.

## Payer contract

Payer Management mengirim eligibility/authorization/claim/adjudication decision yang versioned.
Billing mengubahnya menjadi allocation. External rejection tidak menghapus charge. Manual
decision wajib diberi label `ManualOperator` dan menyertakan evidence, actor, reason, amount, dan
waktu.

## Cashier/Finance contract

Billing memberikan financial reference. Cashier mengirim payment/refund outcome. Finance mengirim
posting/reversal/accounting outcome. Tidak satu pun boleh mengubah clinical fact.

## External adapter contract

### Normalized adapter (Rencana, belum tersedia)

Adapter wajib menyatakan dukungan idempotency, status query, cancellation, amendment, partial
approval, claim submission, timeout, retry, dan reconciliation. Nama vendor, endpoint,
credential, certificate, payload, dan environment tidak boleh ditebak.

Production activation hanya setelah contract owner, security, sandbox/UAT, duplicate/status-query,
reconciliation, support escalation, dan cutover approval tersedia.



---

# Amendment V2 — Rawat Jalan ke Invoice Canonical

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-E2E-CONTRACT-001@1.0.0` |
| Status | `approved` — Sukma Giri, 2026-09-28 |
| Owner | Billing Integration; producer: Clinical (konsultasi, tindakan), Pharmacy (resep), Laboratory, Radiology |
| Traceability | `RJ-E2E-DEC-001`, `003`, `005`, `006`, `009`, `010`, `011` |
| Compatibility impact | `BIL-INTEGRATION-1.3` ditambahkan; `0.4` dan `1.2` tetap diterima dengan aturan lama |

Seluruh integrasi di amendment ini **internal di dalam satu proses aplikasi** (pemanggilan service).
Tidak ada sistem luar; `RJ-BIL-DEP-009` tetap `INACTIVE`.

## V2-1. Arah baca dan tulis

| Dari | Ke | Cara | Isi | Arah |
|---|---|---|---|---|
| Transaksi klinis (konsultasi, tindakan, Lab, Radiologi, resep) | `ClinicalMilestoneFactProducer` | Pemanggilan service **setelah commit klinis** | `ClinicalMilestoneFactRequest` | Tulis ke ledger fakta |
| `ClinicalMilestoneFactProducer` | `BillingFolioService.RecognizeMilestoneAsync` | Pemanggilan service | `RecognizeBillingMilestoneRequest` + **`IsClinicalCancellation`** (baru) | Tulis ke folio |
| `BillingFolioService` | `BillingClinicalChargeBridgeService` | Pemanggilan service setelah commit folio | `ChargeLineId` | Mulai sinkron |
| `BillingClinicalChargeBridgeService` | `BillingInvoiceService` / `BillingFinancialExceptionService` | Pemanggilan service | `UpsertChargeRequest` / void / adjustment | Tulis ke invoice |
| `EncounterBillingSummaryService` | Invoice, kalkulasi, folio | Baca | Total dan status | Baca saja |
| Frontend Rawat Jalan | `GET /encounter-billing-summaries/{encounterId}` | HTTP | — | **Baca saja.** Frontend Rawat Jalan tidak pernah memanggil endpoint tulis Billing |

## V2-2. Kontrak fakta per producer

| Producer | `SourceContext` / `EffectType` | Pemicu | `SourceAggregateId` / `SourceItemId` | `Quantity` / `Unit` | `RuleSnapshot` wajib |
|---|---|---|---|---|---|
| `ConsultationFinalizationService` | `Consultation` / `ConsultationCharge` (**baru**) | Konsultasi menjadi `Completed` lewat finalisasi canonical | konsultasi / — | `1` / `KALI` | `{ milestone: "ConsultationCompleted", doctorId, clinicId }` |
| `PatientProcedureController` (sudah ada) | `Procedure` / `ProcedureCharge` | Tindakan dieksekusi | tindakan / — | dari tindakan | tidak berubah |
| `LabSpecimenService` (sudah ada) | `Laboratory` / `LaboratoryCharge` | Spesimen diterima | order / pemeriksaan | `1` / pemeriksaan | tidak berubah |
| `RadStudyService` (sudah ada) | `Radiology` / `RadiologyCharge` | Mutu study diterima | order / study | `1` / pemeriksaan | tidak berubah; `repeatCause` dibaca jembatan |
| `ConsultationFinalizationService` (resep tahap 1) | `Prescription` / `PrescriptionCharge` | Resep difinalkan bersama konsultasi | resep / — | jumlah item | `{ milestone: "ClinicalFinalization" }` (**baru**) |
| `PrescriptionDispensingService` (resep tahap 2, **baru**) | `Prescription` / `PrescriptionCharge` | Status resep menjadi `Dispensed` atau `PartiallyDispensed` | resep / — | jumlah item diserahkan | `{ milestone: "Dispensed", items: [{ prescriptionItemId, dispensedQuantity }] }` |

Fakta **tidak** membawa SOAP, diagnosis, anamnesis, hasil pemeriksaan, maupun aturan pakai obat
(`SEC-RJ-005`). Harga dalam `TariffSnapshot` tetap boleh dikirim producer lama demi kompatibilitas,
tetapi **tidak dipakai** sebagai harga invoice (`RJ-E2E-DEC-006`).

Resep tahap 2 memakai **identitas fakta yang sama** dengan tahap 1 dan versi naik. Producer yang
sudah ada menangani ini sebagai *CASE B* (charge sudah terbentuk → revisi baru).

**Contoh:** Resep R/0912 (samaran) — tahap 1 versi 1 saat Selesai Konsultasi. Farmasi menyerahkan
sebagian → versi 2 `Dispensed` sebagian; sisa diserahkan esok hari → versi 3. Invoice mengikuti
versi tertinggi; versi 1 dan 2 tetap tercatat di ledger.

## V2-3. Idempotency, timeout, dan kirim ulang

| Mata rantai | Kunci idempotency | Bila hasil tidak pasti | Kirim ulang oleh |
|---|---|---|---|
| Fakta → folio | `CliClinicalMilestoneFact.IdempotencyKey` (sudah ada) | `OutcomeUnknown`; **dilarang** menebak berhasil atau gagal | `ClinicalFactDispatchWorker`, lalu antrean manual |
| Folio → invoice | Guid deterministik `RJ-E2E|{MilestoneFactId}|{MilestoneFactVersion}` → `BilChargeReceipt` | Baris tetap `Pending`/`Failed`; `UpsertChargeAsync` me-replay bila kunci sudah pernah diterima | `BilInvoiceSyncWorker`, lalu antrean manual |
| Folio → adjustment | Kunci deterministik yang sama → `BilAdjustment.IdempotencyKey` | Sama | Sama |

Jadwal kirim ulang dan batasnya dibaca dari `MstBillingSyncPolicy` (`FACT_DISPATCH`,
`INVOICE_SYNC`). Tanpa baris aktif, tidak ada kirim ulang otomatis (fail-closed).

## V2-4. Yang tidak boleh terjadi (`AC-RJ-001`..`015`)

| Larangan | Penjaga |
|---|---|
| Dua invoice aktif untuk satu kunjungan | Unique index `BilInvoice.EncounterId` + advisory lock di `UpsertChargeAsync` |
| Dua item untuk satu (sumber, versi) karena retry atau klik ganda | Kunci deterministik + `BilChargeReceipt` + pemeriksaan versi |
| Item masuk invoice pasien lain | `EncounterId` diambil dari fakta, bukan dari permintaan klien |
| Harga dari browser | Frontend Rawat Jalan tidak punya endpoint tulis; `from-source` menolak domain klinis Rawat Jalan |
| Clinical truth batal karena Billing gagal | Fakta ditulis setelah commit klinis; kegagalan hanya mengubah status sinkron |
| Baris folio yang tak pernah sampai invoice tanpa diketahui siapa pun | Setiap baris layak berakhir `Synced`, `ReconciliationRequired`, atau `Resolved` — tidak ada keadaan diam |

## V2-5. Di luar kontrak ini

| Hal | Pemilik | Catatan |
|---|---|---|
| Penerusan fakta Bank Darah dan hemodialisis ke invoice | Pemilik modul masing-masing | Tetap `NotApplicable` (`SOURCE_OUT_OF_SCOPE`) |
| Penerusan untuk kunjungan rawat inap, IGD, MCU, telemedicine | Pemilik modul masing-masing | Tetap `NotApplicable` (`NOT_OUTPATIENT`) |
| `CONSUMABLE` / `USED` | Belum ada | `RJ-E2E-DEC-011` |
| Pembagian jasa medis (`DoctorShare`) | Medical Fee | `0` pada amendment ini |


---

# Amendment DP — Daftar Pasien Rawat Jalan (`RJ-DOC-ENCLIST-001@1.0.0`, `draft`)

Tidak ada kontrak integrasi baru, karena fitur ini hanya membaca dan membatalkan data kunjungan di
dalam aplikasi yang sama dan tidak memanggil sistem luar maupun modul Billing. Satu-satunya efek
samping lintas komponen adalah notifikasi realtime antrean batal (`QueueRealtimeService`) yang
sudah ada; ia dikirim setelah commit dan kegagalannya tidak membatalkan pembatalan. Ditinjau
ulang bila pembatalan kunjungan kelak harus mengabari Billing atau BPJS.

---

# Amendment KT — Konsultasi Tertunda (`RJ-DOC-PENDCONS-001@1.0.0`, `draft`)

Tidak berlaku: tidak ada integrasi baru, event baru, atau handoff baru. Finalisasi konsultasi
tertunda memakai jalur handoff Billing yang sama dengan konsultasi hari ini
(`RJ-E2E-CONTRACT-001`); tidak ada perubahan.


# Amendment PM-B — `RJ-DOC-REFERRAL-001@1.0.0`

| Field | Nilai |
|---|---|
| `last_changed_in` | `RJ-DOC-REFERRAL-001@1.0.0` — `approved` |
| Owner | Sukma Giri |
| `approved_by` / `approved_at` | Sukma Giri / 2026-10-08 |
| `input_revision` | Decision log *Amendment PM-B* (`RJ-DOC-DEC-068`..`082`) |

| Integrasi | Arah | Kontrak | Perilaku gagal |
|---|---|---|---|
| Agent Plustek — OCR kartu asuransi | Browser → agent lokal `127.0.0.1:9100` | **Belum ada** (`RJ-DOC-OQ-PM-01`). Yang dibutuhkan: jenis dokumen kartu asuransi; field hasil nama asuransi dan No. polis | Agent belum mendukung / field kosong → gambar saja tersimpan, pemeriksaan match tidak berjalan (masa transisi) |
| Agent Plustek — scan surat rujukan (Kiosk) | Browser → agent `POST /scanner/scan` (tanpa OCR, sudah ada) | Gambar per halaman | Gagal scan → pesan, boleh ulang; kunjungan belum dibuat sampai pasien lanjut |
| Registrasi Laboratorium | Frontend RJ → `POST /lab-patient-registrations/external-referral` (sudah ada) | `RegisterLabExternalReferralRequest` + `IdempotencyKey` | Gagal → tidak ada kunjungan; ulang dengan kunci yang sama |
| Rincian rujukan Lab | Frontend RJ → `PUT /patient-encounters/{id}/referral` | `RJ-DOC-REFERRAL-001` | Gagal sesudah kunjungan lab ada → "Rincian rujukan belum tersimpan" + Coba lagi; kunjungan tampil "Rujukan belum lengkap" |
