# Integration Contract — Rawat Jalan Billing

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

