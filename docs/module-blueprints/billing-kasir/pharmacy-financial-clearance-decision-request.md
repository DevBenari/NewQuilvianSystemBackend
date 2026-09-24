# Billing — Permintaan Keputusan: Prescription Financial Clearance

**Jenis dokumen:** `BILLING OWNER DECISION REQUEST`
**Dependency:** Billing → Prescription Financial Clearance → Pharmacy `ReadyForPharmacy`
**Status:** `OPEN` — menunggu keputusan owner Billing
**Tanggal audit:** 18 September 2026 — **diperbarui 21 September 2026** setelah sinkronisasi source
**Basis audit awal:** `origin/QuilvianIntegrationBackend` @ `f59db92f`; isi `Areas/HealthServices/BillingManagement/` terverifikasi identik dengan `origin/Ikbal` @ `ce585574`
**Basis audit terbaru:** `origin/Ikbal` @ `1e279c6b` (30 commit masuk; 22 berkas Billing berubah, 0 berkas Pharmacy berubah)

Pembaruan 21 September memindahkan tiga pertanyaan menjadi `PARTIALLY ANSWERED BY CURRENT SOURCE` — lihat bagian **New Billing Evidence After Pull**. Tidak ada satu pun pertanyaan yang ditutup.

Dokumen ini **bukan** implementasi Farmasi dan **bukan** keputusan final. Ia mencatat apa yang sudah ada di Billing, apa yang belum, dan keputusan apa yang harus diambil owner Billing agar resep dapat masuk ke antrean Farmasi.

Tidak ada satu pun aturan finansial yang disimpulkan sendiri oleh Farmasi di dalam dokumen ini.

---

## 1. Current Billing Capability

| Capability | Status | Evidence |
|---|---|---|
| Prescription charge linkage | **ADA** | `BilChargeLine.SourceContext`, `.SourceAggregateId`, `.SourceItemId`, `.MilestoneFactId`, `.EffectType`; `BillingSourceContract.PrescriptionSourceContext` dan `.PrescriptionChargeEffectType` |
| Payment | **ADA** | `BilSettlement` (`InvoiceId`, `Purpose`, `Status`, `RequestedAmount`, `SuccessfulAmount`, `AllocatedAmount`, `IdempotencyKey`, `PayloadHash`, `CorrelationId`, `CausationId`), `BilPaymentAllocation`, `BilTender`, `BilChargeReceipt` |
| Insurance / guarantee | **ADA** | `BilInvoiceItemPayerAssignment` (`EncounterGuarantorId`, `PayerKind`, `AssignmentSource`, `Reason`); kontrak `BIL-INT-005` Pricing/Coverage → Billing; `BIL-INT-010` `InsuranceCoverageService` → Billing |
| Waiver | **ADA sebagian** | `BilWriteOffCase` (`Amount`, `IsFullSettlement`, `Status`, `Category`, `RequestedBy`, `ApprovedBy`). Istilahnya *write-off*, belum tentu sama dengan `PaymentWaived` pada resep |
| Settlement | **ADA** | `BillingSettlementService`, `BillingRefundService` |
| Reversal / refund / void | **ADA** | `BilPaymentAllocation.ReversesAllocationId` (16 rujukan), `ReverseAsync` (5), `VoidAsync` (2), `BilRefundCase`, `BilRefundLine` |
| **Outbound Pharmacy contract** | **TIDAK ADA** | Nol berkas Billing mengandung `IntegrationEvent`, `DomainEvent`, `Outbox`, `Publish`, `Notify`, `Webhook`, `Callback`, `IHostedService`, `BackgroundService`. Nol berkas Billing merujuk `PhmPrescription` atau `PharmacyManagement` |

### Katalog kontrak integrasi yang sudah ada

Seluruh `BIL-INT` beserta arahnya, dari `docs/module-blueprints/billing-kasir/contracts/integration-contract.md`:

| ID | Arah |
|---|---|
| `BIL-INT-001` | Registration → Billing |
| `BIL-INT-002` | Clinical/Lab/Radiology → Billing |
| `BIL-INT-003` | **Pharmacy → Billing** (dispensed final) |
| `BIL-INT-004` | Inpatient/Bed → Billing |
| `BIL-INT-005` | Pricing/Coverage → Billing |
| `BIL-INT-006` | Payment Provider → Billing |
| `BIL-INT-007` | Billing → AR |
| `BIL-INT-008` | Billing → AP |
| `BIL-INT-009` | Billing → AR/AP |
| `BIL-INT-010` | `InsuranceCoverageService` (Clinical) → Billing |

Satu-satunya arah keluar dari Billing menuju **AR/AP**. **Tidak ada kontrak Billing → modul klinis mana pun**, termasuk Farmasi.

---

## 2. Existing Prescription Link

Resep dapat dikenali di lapisan charge, tetapi rantainya **terputus sebelum mencapai pembayaran**:

```
PhmPrescription.Id
   │
   ├─► BilChargeLine.SourceAggregateId        (SourceContext = "Prescription")
   │      └─► BilChargeLine.FolioId ─► BilFolio
   │
   ╳  TIDAK ADA TAUTAN LANGSUNG
   │
   └─► BilInvoiceItem (SourceDomain, SourceDetailId)
          └─► BilInvoice ─► BilSettlement ─► BilPaymentAllocation
                                               TargetType = Invoice
```

Dua fakta yang membuat Farmasi tidak dapat menyimpulkan sendiri:

1. **`BilChargeLine` dan `BilInvoiceItem` tidak saling merujuk.** Pencarian silang pada source menghasilkan **0** kecocokan. Charge line hidup di bawah `BilFolio`; invoice item hidup di bawah `BilInvoice` dengan penanda sumber terpisah — `SourceDomain` bertipe `string`, dan nilai yang terlihat di source hanya `ADHOC` serta `ADHOC_CATALOG`.
2. **Pembayaran dialokasikan ke Invoice secara utuh.** `BilPaymentAllocation.TargetType = BillingAllocationTargetTypes.Invoice` — bukan per item, apalagi per resep.

Akibatnya, dari satu invoice yang lunas **tidak ada cara deterministik** untuk menyatakan "resep X sudah clear" tanpa aturan yang ditetapkan Billing.

---

## 3. Missing Contract

1. **Tidak ada kontrak berarah Billing → Pharmacy.** Lihat katalog pada bagian 1.
2. **Tidak ada mekanisme penerbitan apa pun** di Billing — nol event, outbox, publisher, notifier, webhook, callback, background worker.
3. **Tidak ada definisi *financial clearance* per resep.** `BilInvoice.Status` hanya `OPEN` / `FINAL` / `CLOSED` — status dokumen, bukan status pembayaran.
4. **Tidak ada jembatan `BilChargeLine` ↔ `BilInvoiceItem`**, sehingga resep tidak dapat ditelusuri sampai pembayaran.

### Catatan batas yang sudah berlaku

`integration-contract.md` bagian *"Batas terhadap Pharmacy Management"* menyatakan Billing **MUST NOT** membuat, mengubah, membatalkan, atau menandai hapus satu baris pun milik Farmasi. Bila nanti diputuskan Billing yang menulis status resep, **batas itu harus diamandemen lebih dulu**.

Dokumen yang sama juga sudah mencatat satu titik singgung terbuka: *"Kolom penanda 'sudah ditagih' pada baris penyerahan Farmasi sudah ada tetapi belum diketahui dipakai proses apa."*

---

## 4. Mekanisme Billing → Pharmacy

### Option A — Push / Event

Billing menerbitkan outcome, Farmasi mengonsumsinya.

| Hal | Konsekuensi |
|---|---|
| Infrastruktur | Harus dibangun dari nol — Billing saat ini tidak punya mekanisme penerbitan apa pun |
| Ownership boundary | **Harus diamandemen**, karena Billing menjadi penulis status milik Farmasi |
| Kelebihan | Farmasi mendapat pembaruan segera tanpa polling |

### Option B — Pull / Projection

Billing menyediakan pembacaan read-only; Farmasi memanggil lalu memproyeksikan sendiri.

| Hal | Konsekuensi |
|---|---|
| Infrastruktur | Tidak perlu event/outbox baru |
| Ownership boundary | **Tidak dilanggar** — Billing tidak menulis apa pun milik Farmasi |
| Kelebihan | Billing tetap authoritative; Farmasi hanya membaca |
| Tetap dibutuhkan | Definisi clearance (`BIL-PHM-OQ-001`–`004`) tidak hilang, hanya berpindah ke sisi baca |

### Option C — Shared Read Model

Satu proyeksi yang ditulis Billing dan dibaca Farmasi.

| Hal | Konsekuensi |
|---|---|
| Infrastruktur | Tabel/model bersama baru |
| Ownership boundary | Perlu pemilik skema yang eksplisit |
| Tetap dibutuhkan | Definisi clearance yang sama |

### TECHNICAL RECOMMENDATION — NOT OWNER DECISION

> **`OPTION B — Pull / Projection`** dicatat sebagai rekomendasi teknis, **bukan keputusan**.
>
> Alasannya: Billing tetap authoritative; Billing tidak menulis row Pharmacy; tidak memerlukan infrastruktur event/outbox baru; tidak melanggar ownership boundary yang berlaku sekarang; dan Farmasi hanya mengonsumsi outcome read-only.
>
> Pemilihan mekanisme tetap merupakan keputusan owner Billing pada `BIL-PHM-OQ-006`.

---

## 5. Reversal

Kapabilitasnya sudah ada di Billing: `ReversesAllocationId`, `ReverseAsync`, `VoidAsync`, `BilRefundCase`, `BilRefundLine`.

Yang belum ditetapkan adalah **dampaknya terhadap resep** ketika pembayaran dibalik setelah resep bergerak maju. Pertanyaan ini dirinci pada `BIL-PHM-OQ-007` untuk setiap state Farmasi.

Yang perlu diperhatikan owner Billing: pada `PartiallyDispensed` dan `Dispensed`, **obat sudah keluar dan stok sudah berkurang**. Menarik status mundur di titik itu tidak mengembalikan obatnya. Farmasi tidak menetapkan aturan ini sendiri.

---

## 6. Idempotency dan Replay

`BilSettlement` sudah membawa `IdempotencyKey`, `PayloadHash`, `CorrelationId`, dan `CausationId`.

Syarat dari sisi Farmasi, apa pun mekanisme yang dipilih:

1. Penerimaan/pembacaan ulang dengan kunci yang sama **tidak boleh** menggerakkan status dua kali.
2. Kedatangan yang tidak berurutan **tidak boleh** membuat resep mundur secara keliru.
3. Outcome harus dapat ditelusuri ke transaksi Billing yang menerbitkannya.

Sumber kunci ditetapkan pada `BIL-PHM-OQ-008`.

---

## 7. Ownership Boundary

**Billing owns:**

- financial truth
- `Paid`
- `InsuranceApproved`
- semantik `PaymentWaived` / write-off
- settlement
- reversal / refund / void
- payer / claim outcome

**Pharmacy owns:**

- projection `PhmPrescription.PaymentStatus`
- transisi `WaitingForPayment` → `ReadyForPharmacy`
- seluruh workflow setelah resep financially cleared

**Batas tegas:**

- Billing **tidak** menulis langsung entity Pharmacy menurut kontrak yang berlaku sekarang.
- Pharmacy **tidak boleh** menentukan sendiri status finansial.

Method `MarkPaidAsync`, `MarkInsuranceApprovedAsync`, `MarkPaymentWaivedAsync`, dan `CompletePaymentAsync` tetap dicabut dari modul klinis sesuai `RJ-BIL-BE-002` / `RJ-BIL-CONFLICT-006`, dan **tidak** akan dikembalikan.

---

## 8. Proposed Contract Shape

> **`PROPOSAL — REQUIRES BILLING OWNER APPROVAL`**

Bentuk konseptual, bukan kontrak final:

```json
{
  "sourceContext": "Prescription",
  "sourceAggregateId": "<PhmPrescription.Id>",
  "financialOutcome": "Paid | InsuranceApproved | PaymentWaived",
  "occurredAt": "<saat outcome sah menurut Billing>",
  "sourceTransactionId": "<BilSettlement.Id | BilWriteOffCase.Id>",
  "idempotencyKey": "<unik per outcome, aman untuk replay>"
}
```

Ketiga nilai `financialOutcome` di atas hanya mencerminkan status yang **sudah** dipakai `PrescriptionDispensingService` sebagai syarat penyerahan. Nama final enum, event, route, DTO, dan tabel **tidak dikunci** dan menunggu keputusan owner Billing pada `BIL-PHM-OQ-006`.

---

## 9. Decisions Required From Billing Owner

Seluruhnya berstatus `OWNER DECISION REQUIRED`. Tidak satu pun dijawab oleh Farmasi.

### `BIL-PHM-OQ-001` — `PARTIALLY ANSWERED BY CURRENT SOURCE`
Apa definisi authoritative `Paid` untuk satu prescription?

**Sudah terjawab sebagian.** `BillingInvoiceClosureService.SyncClosureAsync` kini menetapkan aturan penutupan tagihan:

```
FINAL  + outstanding <= 0  ->  CLOSED
CLOSED + outstanding >  0  ->  FINAL
```

Artinya Billing **sudah punya definisi authoritative** bahwa sebuah invoice tidak lagi memiliki kewajiban terutang milik pasien, lengkap dengan pemulihan otomatis ketika outstanding kembali muncul.

**Tetap OPEN.** Belum ada aturan yang menyatakan `Invoice CLOSED` → prescription tertentu = `Paid`. Resep masih berada pada charge source (`BilChargeLine`) yang belum terhubung secara deterministic ke invoice item maupun pembayaran — lihat `BIL-PHM-OQ-004` dan `BIL-PHM-OQ-005`.

### `BIL-PHM-OQ-002`
Apa definisi authoritative `InsuranceApproved`?

### `BIL-PHM-OQ-003` — `PARTIALLY ANSWERED BY CURRENT SOURCE`
Apakah `BilWriteOffCase` / full settlement ekuivalen dengan `PaymentWaived` untuk prescription?

**Sudah terjawab sebagian.** Billing kini punya status invoice tersendiri untuk write-off penuh:

```
full approved write-off  ->  SETTLED_BY_WRITE_OFF
```

Dan keputusan Billing menegaskan batasnya — `BKC-DEC-036`: *"Write-off tidak pernah `PAID`; full write-off menjadi `SETTLED_BY_WRITE_OFF`, partial menyisakan balance, dan reversal memulihkan AR."* Jadi di sisi Billing sudah jelas bahwa **`SETTLED_BY_WRITE_OFF` bukan `PAID`**.

**Tetap OPEN.** Belum diputuskan apakah `SETTLED_BY_WRITE_OFF` boleh diproyeksikan Farmasi menjadi `PrescriptionPaymentStatus.PaymentWaived`. Kesetaraan itu adalah keputusan finansial dan membutuhkan persetujuan owner Billing.

### `BIL-PHM-OQ-004`
Financial clearance berlaku pada level apa — Prescription, ChargeLine, Invoice, Folio, atau Encounter?

### `BIL-PHM-OQ-005`
Bagaimana `PhmPrescription.Id` ditelusuri secara deterministic sampai settlement/payment? Apakah Billing menambahkan linkage eksplisit charge → invoice item, menetapkan clearance pada level Invoice/Folio, atau memakai desain lain?

### `BIL-PHM-OQ-006`
Mekanisme handoff mana yang dipilih — Push/Event, Pull/Projection, atau Shared Read Model?

### `BIL-PHM-OQ-007` — `PARTIALLY ANSWERED BY CURRENT SOURCE`
Apa aturan reversal berdasarkan state Farmasi saat itu?

**Sudah terjawab sebagian — di sisi Billing.** Mekanisme pembukaan kembali tagihan sudah tersedia dan otomatis:

```
CLOSED + outstanding kembali > 0  ->  FINAL
```

Ditopang infrastruktur yang sudah ada: `ReversesAllocationId`, `ReverseAsync`, `VoidAsync`, `BilRefundCase`, `BilRefundLine`, dan pemulihan AR pada reversal write-off (`BKC-DEC-036`). Artinya Billing **mampu** memberi tahu bahwa sebuah tagihan kembali terutang.

**Tetap OPEN — dampaknya ke Farmasi.** Belum diputuskan state Farmasi mana yang boleh ditarik mundur ketika hal itu terjadi. Farmasi tidak menetapkannya sendiri.

| State Farmasi | Dampak reversal? |
|---|---|
| `WaitingForPayment` | |
| `ReadyForPharmacy` | |
| `QueuedAtPharmacy` | |
| `VerifiedByPharmacy` | |
| `InPreparation` | |
| `AwaitingFinalCheck` | |
| `ReadyToDispense` | |
| `PartiallyDispensed` | obat sudah keluar sebagian |
| `Dispensed` | obat sudah keluar |

### `BIL-PHM-OQ-008`
Apa sumber `idempotencyKey` untuk financial-clearance outcome — memakai `BilSettlement.IdempotencyKey` yang sudah ada, atau kunci baru per outcome?

---

## New Billing Evidence After Pull

Audit ulang pada `origin/Ikbal` @ `1e279c6b` (21 September 2026) menemukan kemampuan berikut **sudah tersedia** di source Billing:

| Bukti | Keterangan |
|---|---|
| `BillingInvoiceClosureService` | Layanan penutupan tagihan, tersedia |
| `FINAL + outstanding <= 0 -> CLOSED` | Aturan penutupan otomatis |
| `CLOSED + outstanding > 0 -> FINAL` | Pembukaan kembali otomatis saat outstanding muncul lagi |
| `SETTLED_BY_WRITE_OFF` | Status invoice tersendiri untuk write-off penuh; `BKC-DEC-036` menegaskan ini **bukan** `PAID` |
| `IsFullyPaid`, `PaidAmount`, `OutstandingAmount` | Tersedia pada response baca/riwayat pembayaran yang relevan |
| `ReverseAsync`, `VoidAsync`, `ReversesAllocationId`, `BilRefundCase`, `BilRefundLine` | Infrastruktur reversal/void/refund tersedia |

**Penegasan penting.** Bukti-bukti di atas **memperkuat financial truth di Billing**, tetapi **belum membuat Prescription-level financial clearance**. Seluruhnya bekerja pada level **Invoice**, sedangkan resep tertaut pada **charge line**. Audit ulang mengonfirmasi tiga hal berikut **masih nol** di source terbaru:

| Pemeriksaan | Hasil |
|---|---|
| Tautan `BilChargeLine` ↔ `BilInvoiceItem` | **0** |
| Berkas Billing yang merujuk `PhmPrescription` | **0** |
| Mekanisme keluar Billing (`IntegrationEvent`, `DomainEvent`, `Outbox`, `Publish`, `Webhook`, `Callback`) | **0** |

---

## 10. Current Blocker Status

> **`Billing → ReadyForPharmacy = BLOCKED BY BILLING CONTRACT`**

### Blocker setelah pembaruan — dipersempit

Billing sekarang sudah tahu:

> *"invoice ini sudah tidak punya outstanding"*

Yang belum diketahui Farmasi:

> *"apakah prescription X termasuk kewajiban yang sudah clear pada invoice tersebut?"*
> *"bagaimana Pharmacy mendapatkan jawaban itu?"*

Karena itu blocker menyempit menjadi tiga:

1. **Prescription → Invoice/payment deterministic linkage** — `BIL-PHM-OQ-004`, `BIL-PHM-OQ-005`
2. **Billing → Pharmacy handoff mechanism** — `BIL-PHM-OQ-006`
3. **Mapping Billing financial outcome → Pharmacy `PaymentStatus`** — `BIL-PHM-OQ-001`, `002`, `003`, `007`, `008`

### Status Farmasi saat ini

P0 Final Check sudah selesai pada source lokal. Rantai Farmasi kini utuh:

```
Review
  → Preparation
  → AwaitingFinalCheck
  → Final Check
  → ReadyToDispense
  → Dispensing
```

Blocker core Farmasi yang tersisa **hanya satu**:

```
WaitingForPayment
  → [Billing Financial Clearance]
  → ReadyForPharmacy
```

Begitu `BIL-PHM-OQ-001` sampai `008` dijawab, pekerjaan Farmasi yang tersisa kecil: consumer idempoten, proyeksi ke `PhmPrescription.PaymentStatus`, dan transisi ke `ReadyForPharmacy`. `PrescriptionReviewService` sudah menerima `ReadyForPharmacy` sejak sekarang.

### Technical follow-up (bukan blocker bisnis)

- Proyek test backend tidak tersedia di repository saat ini.
- `TrxPrescriptionFinalCheck` belum punya kolom nomor percobaan; riwayat tetap tertelusuri lewat `StartedAt` dan `CreateDateTime`.
