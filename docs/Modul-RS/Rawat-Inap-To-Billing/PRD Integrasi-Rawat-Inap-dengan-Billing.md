# PRD — Integrasi Rawat Inap dengan Billing

## 1. Informasi Dokumen

**Nama Fitur:** Integrasi Rawat Inap ↔ Billing/Kasir  
**Target Sistem:** Quilvian V2  
**Jenis Dokumen:** Shared Integration PRD  
**Rawat Inap Owner:** Muhammad Hamzah  
**Billing Owner:** Yasmina  
**Integration Coordinator:** Leader / Tech Lead  

### Kedudukan Dokumen

Dokumen ini merupakan **satu shared PRD** untuk kebutuhan integrasi antara Modul Rawat Inap dan Billing.

Dokumen ini **bukan berarti kedua Modul menjadi satu Modul atau satu blueprint**.

Setelah requirement integrasi dikunci, lifecycle tetap dipisahkan menjadi:

```text
Shared PRD
Integrasi Rawat Inap ↔ Billing
        │
        ├── Blueprint Rawat Inap
        │   Owner: Muhammad Hamzah
        │
        └── Blueprint Billing
            Owner: Yasmina
```

Dokumen ini digunakan sebagai input bersama agar kedua Modul mempunyai pemahaman bisnis dan kontrak integrasi yang sama.

---

# 2. Latar Belakang

Rawat Inap dan Billing mempunyai hubungan bisnis yang sangat erat.

Rawat Inap mengetahui kondisi faktual pelayanan pasien:

- pasien masuk Rawat Inap;
- episode dan encounter;
- kamar dan bed;
- kelas aktual;
- periode penggunaan kamar;
- mutasi kamar;
- koreksi kelas/kamar;
- pelayanan selama episode;
- permintaan pulang;
- discharge.

Billing bertanggung jawab terhadap dampak finansial:

- pembentukan charge;
- tarif kamar;
- kebijakan tarif;
- deposit;
- invoice;
- asuransi;
- excess pasien;
- pembayaran;
- bill sementara;
- close bill;
- adjustment/reversal;
- settlement;
- financial clearance.

Masalah yang perlu dicegah:

- Rawat Inap menghitung tagihan sendiri;
- Billing menyimpan ulang fakta occupancy yang berbeda dari Rawat Inap;
- dua developer mengerjakan logic yang sama;
- satu task dimiliki dua developer tanpa accountable owner;
- perubahan kontrak satu Modul merusak Modul lainnya;
- bug integrasi tidak mempunyai owner;
- `grill-me` Modul Rawat Inap membahas terlalu jauh aturan internal Billing;
- `grill-me` Billing mendesain ulang proses internal Rawat Inap.

---

# 3. Tujuan

Membangun integrasi Rawat Inap ↔ Billing dengan prinsip:

> **Rawat Inap menjadi source of truth terhadap fakta pelayanan, episode, dan occupancy pasien. Billing menjadi source of truth terhadap nilai finansial pasien.**

Target alur:

```text
Admission Rawat Inap
        ↓
Encounter / Episode aktif
        ↓
Occupancy kamar
        ↓
Billing menerima fakta pelayanan
        ↓
Room Charge dan charge pelayanan terbentuk
        ↓
Deposit / Asuransi / Patient Excess
        ↓
Bill Sementara
        ↓
Discharge Request
        ↓
Pre-Close Validation
        ↓
Close Bill
        ↓
Settlement
        ↓
Financial Clearance
        ↓
Rawat Inap menerima clearance
        ↓
Discharge selesai
```

---

# 4. Prinsip Pembagian Scope

Pembagian scope **tidak ditentukan berdasarkan jumlah requirement yang harus sama banyak**.

Pembagian dilakukan berdasarkan:

- ownership proses bisnis;
- ownership data;
- lifecycle data;
- source of truth;
- siapa yang menghasilkan informasi;
- siapa yang mengonsumsi informasi;
- siapa yang berwenang mengubah data;
- siapa yang bertanggung jawab terhadap hasil akhir.

Artinya:

```text
Muhammad Hamzah
tidak harus mempunyai jumlah task
yang sama dengan Yasmina.

Yang wajib adalah:
tidak ada overlap ownership
dan
tidak ada requirement tanpa owner.
```

---

# 5. Model Scope PRD

PRD dibagi menjadi tiga wilayah:

```text
┌──────────────────────────────────────────────┐
│            SHARED INTEGRATION PRD            │
└──────────────────────┬───────────────────────┘
                       │
       ┌───────────────┼───────────────┐
       │               │               │
       ▼               ▼               ▼
RAWAT INAP         SHARED          BILLING
SCOPE              CONTRACT        SCOPE

Muhammad           Muhammad        Yasmina
Hamzah             + Yasmina
```

## Scope A

**Rawat Inap — Muhammad Hamzah**

## Scope B

**Billing — Yasmina**

## Scope C

**Shared Integration Contract — Muhammad Hamzah + Yasmina**

Shared scope tidak berarti kedua developer mengerjakan source yang sama.

Shared scope adalah kontrak yang harus disepakati kedua pihak.

---

# 6. Scope `grill-me` Rawat Inap

## Owner

**Muhammad Hamzah**

## Modul

**Rawat Inap**

`grill-me` Rawat Inap hanya membahas hal-hal yang menjadi tanggung jawab Rawat Inap.

### Di Dalam Scope

#### 6.1 Encounter dan Episode

- kapan episode Rawat Inap dianggap aktif;
- identifier yang digunakan;
- relasi episode terhadap encounter;
- kapan episode dianggap selesai;
- apa yang dikirim ke Billing ketika episode dimulai.

#### 6.2 Occupancy Kamar

- pasien berada di kamar apa;
- bed apa;
- kelas aktual;
- waktu mulai occupancy;
- waktu akhir occupancy;
- perpindahan kamar;
- perpindahan kelas;
- histori occupancy.

#### 6.3 Koreksi Occupancy

- siapa boleh memperbaiki kamar;
- siapa boleh memperbaiki kelas;
- alasan koreksi;
- effective time koreksi;
- histori perubahan;
- bagaimana perubahan diinformasikan kepada Billing.

#### 6.4 Mutasi Kamar

Contoh:

```text
VIP
↓
ICU
```

Rawat Inap menentukan:

```text
kapan mutasi terjadi;
dari kamar mana;
ke kamar mana;
kelas sebelumnya;
kelas sesudahnya.
```

Rawat Inap **tidak menentukan nominal charge**.

#### 6.5 Discharge Request

- kapan pasien dianggap mengajukan proses pulang;
- status discharge;
- prasyarat klinis;
- kapan Rawat Inap meminta status finansial;
- perilaku ketika Billing belum clear.

#### 6.6 Financial Clearance Consumption

Rawat Inap menentukan:

- bagaimana membaca financial clearance;
- bagaimana menampilkan status;
- apa yang terjadi bila `BLOCKED`;
- apa yang terjadi bila `CLEARED`;
- bagaimana discharge final dikunci oleh hasil Billing.

#### 6.7 UI Rawat Inap yang Berhubungan dengan Billing

Rawat Inap boleh menampilkan:

- status Billing;
- deposit summary;
- outstanding;
- financial clearance;
- blocker;
- navigasi menuju Billing bila pengguna mempunyai permission.

Angka finansial selalu berasal dari Billing.

---

# 7. Out of Scope `grill-me` Rawat Inap

Dalam sesi Muhammad Hamzah, hal berikut **tidak boleh digali sebagai internal Rawat Inap**:

- cara menghitung tarif kamar;
- nominal room charge;
- rumus admin fee;
- payment processing;
- payment gateway;
- settlement;
- invoice calculation;
- insurance tariff;
- guaranteed amount calculation;
- patient excess calculation;
- deposit ledger;
- metode pembayaran;
- aturan QRIS/kartu/transfer;
- adjustment nilai finansial;
- accounting posting;
- struktur internal invoice Billing.

Jika muncul pertanyaan tersebut, catat:

```text
Di luar scope Rawat Inap
Owner: Billing / Yasmina
```

Yang boleh ditanyakan hanya titik sentuh.

Contoh yang benar:

> Data occupancy apa yang harus dikirim Rawat Inap agar Billing dapat menghitung room charge?

Bukan:

> Bagaimana rumus tarif kamar harus dihitung di Rawat Inap?

---

# 8. Scope `grill-me` Billing

## Owner

**Yasmina**

## Modul

**Billing/Kasir**

### Di Dalam Scope

#### 8.1 Billing Episode

- bagaimana Billing mengenali episode RANAP;
- penggunaan `EncounterId`;
- hubungan episode dengan invoice;
- status finansial episode.

#### 8.2 Room Charge

Billing menentukan:

- tarif kamar;
- effective tariff;
- daily charge;
- admission-time policy;
- transfer policy;
- room class policy;
- payer tariff;
- charge lifecycle.

#### 8.3 Room Charge Correction

- adjustment;
- reversal;
- void;
- repricing;
- audit finansial;
- reconciliation.

#### 8.4 Deposit

- minimum deposit;
- balance;
- movement;
- shortfall;
- penggunaan deposit;
- refund jika relevan;
- requirement sebelum tindakan besar.

#### 8.5 Insurance

- guaranteed amount;
- covered amount;
- patient responsibility;
- excess;
- benefit;
- master tarif penjamin.

#### 8.6 Invoice

- provisional bill;
- invoice;
- invoice item;
- consolidation;
- close bill;
- finalization.

#### 8.7 Pembayaran

Metode yang perlu didukung sesuai bisnis:

- tunai;
- kartu;
- transfer;
- QRIS;
- potong deposit.

#### 8.8 Financial Clearance

Billing menjadi pemilik keputusan finansial:

```text
PENDING
BLOCKED
CLEARED
REVOKED
```

atau terminology final yang disetujui.

Billing menentukan blocker finansial.

#### 8.9 Close Bill

Billing menentukan:

- kapan bill dapat ditutup;
- pre-close validation;
- posting lock;
- reopening;
- late adjustment;
- settlement.

---

# 9. Out of Scope `grill-me` Billing

Dalam sesi Yasmina, hal berikut **tidak boleh didesain sebagai internal Billing**:

- bagaimana admission Rawat Inap disimpan;
- struktur internal episode Rawat Inap;
- workflow bed assignment;
- algoritme availability bed;
- UI mutasi kamar Rawat Inap;
- aturan klinis discharge;
- resume medis;
- nursing discharge;
- struktur dokumentasi dokter;
- bagaimana kelas aktual dicatat secara internal Rawat Inap.

Billing hanya membutuhkan kontraknya.

Contoh pertanyaan yang benar:

> Field apa dari occupancy yang menjadi input Room Charge Engine?

Bukan:

> Bagaimana Rawat Inap harus membangun tabel occupancy?

---

# 10. Shared Integration Scope

Bagian ini dibaca oleh Muhammad Hamzah dan Yasmina.

Keputusan di bagian ini tidak boleh ditetapkan sepihak.

## Shared Topics

### 10.1 Identifier

Minimal harus disepakati:

```text
EncounterId
EpisodeId bila diperlukan
SourceDetailId
```

### 10.2 Occupancy Contract

Harus disepakati:

```text
EncounterId
RoomId
BedId
RoomClassId

StartAt
EndAt

ChangeType
SourceDetailId
Version
```

### 10.3 Correction Contract

Harus disepakati:

```text
SourceDetailId
PreviousVersion
NewVersion
CorrectionReason
EffectiveAt
```

### 10.4 Billing Summary Contract

Minimal:

```text
EncounterId

BillingStatus

CurrentCharge

DepositRequired
DepositBalance
DepositShortfall

Outstanding

FinancialClearanceStatus

Blockers[]
```

### 10.5 Financial Clearance Contract

Minimal harus menjawab:

- status clearance;
- alasan blocked;
- waktu keputusan;
- reference Billing;
- apakah clearance dapat revoked;
- bagaimana Rawat Inap bereaksi terhadap revoke.

### 10.6 Failure Contract

Harus ditentukan:

- timeout;
- retry;
- duplicate request;
- stale response;
- Billing unavailable;
- Rawat Inap unavailable;
- reconciliation.

### 10.7 Versioning Contract

Contract harus mempunyai version.

Contoh:

```text
RI-BILL-OCCUPANCY-v1
BILL-RI-FINANCIAL-CLEARANCE-v1
```

---

# 11. Approval Shared Contract

Shared Integration Contract harus disepakati:

```text
Producer
+
Consumer
```

Contoh:

```text
RI-BILL-OCCUPANCY-v1

Producer:
Muhammad Hamzah / Rawat Inap

Consumer:
Yasmina / Billing
```

Contract belum dianggap approved bila hanya salah satu sisi setuju.

Untuk arah sebaliknya:

```text
BILL-RI-FINANCIAL-CLEARANCE-v1

Producer:
Yasmina / Billing

Consumer:
Muhammad Hamzah / Rawat Inap
```

Jika terjadi conflict:

```text
Muhammad
        \
         > Integration Coordinator
        /
Yasmina
```

Contract tetap `DRAFT` atau `CONFLICT` sampai keputusan selesai.

---

# 12. Scope Routing Matrix

| Requirement | Owner `grill-me` | Modul lain berperan sebagai |
|---|---|---|
| Admission RANAP | Muhammad Hamzah | Billing consumer bila perlu |
| Encounter | Muhammad Hamzah | Billing consumer |
| Episode RANAP | Muhammad Hamzah | Billing consumer |
| Penempatan kamar | Muhammad Hamzah | Billing menerima fakta |
| Bed | Muhammad Hamzah | Billing reference bila perlu |
| Kelas aktual | Muhammad Hamzah | Billing menggunakan untuk pricing |
| Occupancy Start/End | Muhammad Hamzah | Billing consumer |
| Mutasi kamar | Muhammad Hamzah | Billing menerima perubahan |
| Koreksi kelas | Muhammad Hamzah | Billing melakukan repricing |
| Tarif kamar | Yasmina | Rawat Inap tidak menghitung |
| Room Charge | Yasmina | Rawat Inap sumber occupancy |
| Aturan jam masuk | Yasmina | Rawat Inap menyediakan timestamp |
| Split charge pindah kamar | Yasmina | Rawat Inap menyediakan segment |
| Deposit | Yasmina | Rawat Inap read-only |
| Admin fee | Yasmina | Rawat Inap tidak menghitung |
| Insurance | Yasmina | Rawat Inap read-only jika perlu |
| Patient excess | Yasmina | Rawat Inap read-only |
| Bill sementara | Yasmina | Rawat Inap boleh mengarahkan user |
| Close bill | Yasmina | Rawat Inap menerima status |
| Posting lock | Yasmina | Modul klinis harus menghormati |
| Financial Clearance | Yasmina | Muhammad consumer |
| Discharge Gate | Muhammad Hamzah | Bergantung output Billing |
| Occupancy Contract | Shared | RI producer, Billing consumer |
| Financial Clearance Contract | Shared | Billing producer, RI consumer |
| Retry / idempotency | Shared | Kedua sisi patuh |
| Contract version | Shared | Kedua sisi approve |

---

# 13. Source of Truth

| Informasi | Source of Truth |
|---|---|
| Encounter pasien | Rawat Inap |
| Episode Rawat Inap | Rawat Inap |
| Kamar aktual | Rawat Inap |
| Bed aktual | Rawat Inap |
| Kelas aktual | Rawat Inap |
| Waktu occupancy | Rawat Inap |
| Mutasi kamar | Rawat Inap |
| Koreksi fakta occupancy | Rawat Inap |
| Discharge | Rawat Inap |
| Tarif kamar | Billing |
| Room charge | Billing |
| Admission-time pricing policy | Billing |
| Transfer pricing policy | Billing |
| Deposit | Billing |
| Invoice | Billing |
| Insurance allocation | Billing |
| Payment | Billing |
| Outstanding | Billing |
| Settlement | Billing |
| Financial Clearance | Billing |

---

# 14. Identifier Integrasi

`EncounterId` menjadi identifier utama hubungan Rawat Inap dan Billing.

```text
Patient
   │
   ▼
EncounterId
   │
   ├────────────── Rawat Inap
   │
   └────────────── Billing
```

Billing boleh memiliki:

```text
BillingEpisodeId
BillingAccountId
InvoiceId
```

tetapi seluruhnya harus traceable ke:

```text
EncounterId
```

---

# 15. Occupancy Kamar

Rawat Inap menyediakan fakta:

```text
EncounterId

RoomId
BedId
RoomClassId

StartAt
EndAt
```

Contoh:

```text
16 Sep 2026 15:00
VIP Deluxe

        ↓

18 Sep 2026 10:00
ICU
```

Maka terdapat occupancy:

```text
Segment 1
VIP Deluxe
16 Sep 15:00
→ 18 Sep 10:00

Segment 2
ICU
18 Sep 10:00
→ selesai
```

Rawat Inap tidak menentukan:

```text
Rp5.500.000
```

atau nominal lain.

---

# 16. Room Charge Engine

Billing mengubah fakta occupancy menjadi charge.

```text
Occupancy
   ↓
Effective Room Tariff
   ↓
Admission-Time Policy
   ↓
Same-Day Transfer Policy
   ↓
Payer / Insurance Policy
   ↓
Room Charge
```

Minimal charge harus traceable terhadap:

```text
EncounterId

SourceDomain = INPATIENT
SourceType   = ROOM_STAY
SourceDetailId

ServiceDate

BaseAmount
AppliedPolicy
Multiplier
FinalAmount

Version
```

---

# 17. Aturan Tarif Kamar Berdasarkan Jam Masuk

Requirement bisnis:

### Masuk setelah 18:00

Room charge hari pertama:

```text
50%
```

### Masuk setelah 22:00

Room charge hari pertama:

```text
20%
```

### Kondisi setelah tengah malam

Hari sebelumnya tidak dikenakan room charge sesuai ketentuan bisnis.

Aturan tidak boleh hard-coded tersebar.

Gunakan policy yang dapat mempunyai:

```text
PolicyCode

EffectiveFrom
EffectiveTo

StartLocalTime
EndLocalTime

Multiplier

Priority

RoomClassId?
FacilityId?
PayerType?
```

---

# 18. Boundary Waktu yang Belum Diputuskan

Masih perlu keputusan untuk:

```text
18:00 tepat
22:00 tepat
00:00 tepat
```

Belum boleh diasumsikan sebagai:

```text
>
atau
>=
```

sampai owner bisnis menetapkan.

---

# 19. Pindah Kamar pada Hari yang Sama

Requirement:

Jika pasien berpindah dua kamar pada service day yang sama:

```text
Kamar A
→
Kamar B
```

maka:

```text
Kamar A = 50%
Kamar B = 50%
```

Contoh:

```text
VIP
Rp4.000.000 × 50%
= Rp2.000.000

ICU
Rp8.000.000 × 50%
= Rp4.000.000

Total
Rp6.000.000
```

---

# 20. Multiple Transfer di Hari yang Sama

Kasus:

```text
Kamar A
→ Kamar B
→ ICU
```

belum mempunyai keputusan final.

Sistem tidak boleh otomatis mengasumsikan:

```text
33.33%
33.33%
33.33%
```

tanpa keputusan bisnis.

Status:

```text
OPEN BUSINESS DECISION
```

---

# 21. Edit Kelas/Kamar Aktual

Admission/role yang berwenang dapat memperbaiki jika:

```text
Tercatat:
Kelas I

Aktual:
VIP
```

Rawat Inap memperbaiki:

```text
fakta occupancy
```

bukan nominal charge.

Audit minimal:

```text
PreviousRoomClass
CorrectedRoomClass

EffectiveFrom
EffectiveTo

Reason

CorrectedBy
CorrectedAt
```

Billing kemudian melakukan repricing.

---

# 22. Koreksi Tagihan Kamar

Jika kamar lupa ditutup dan charge terus berjalan:

```text
❌ hard delete charge
❌ overwrite history
```

Target:

```text
Original Charge
       ↓
REVERSED / VOIDED
       ↓
Correction
       ↓
Corrected Charge
```

Audit wajib menyimpan:

- siapa;
- kapan;
- alasan;
- nominal lama;
- nominal baru;
- source occupancy;
- encounter;
- invoice.

---

# 23. Idempotency

Posting ulang source yang sama tidak boleh menghasilkan charge ganda.

Contract minimal:

```text
SourceDomain
SourceDetailId
Version
```

atau:

```text
IdempotencyKey
```

Contoh:

```text
ROOM_STAY:ABC:1
```

Request pertama:

```text
CREATED
```

Retry:

```text
RETURN EXISTING
```

---

# 24. Extra Bed

Extra bed bukan occupancy utama pasien.

Dikelola sebagai additional Billing charge.

Contoh:

```text
SourceType = EXTRA_BED
```

Billing mengelola:

- tarif;
- qty;
- tanggal;
- audit;
- adjustment.

---

# 25. Makanan Pendamping

Benefit asuransi dapat mempunyai plafon.

Contoh:

```text
AIA Financial
Maksimal Rp500.000/hari
```

Jika struk aktual:

```text
Rp325.000
```

covered:

```text
Rp325.000
```

Jika:

```text
Rp650.000
```

dan kontrak maksimal Rp500.000:

```text
Covered          Rp500.000
Patient Excess   Rp150.000
```

jika kontrak penjamin menetapkan kelebihan menjadi tanggung jawab pasien.

---

# 26. Deposit Rawat Inap

Billing menjadi owner deposit.

Requirement awal:

```text
minimal 30% dari estimasi biaya
```

Contoh:

```text
Estimated Cost      Rp20.000.000
Required Deposit    Rp 6.000.000
Paid Deposit        Rp 4.000.000
Shortfall           Rp 2.000.000
```

Rawat Inap hanya membaca hasil.

---

# 27. Deposit Sebelum Tindakan Besar

Requirement:

Deposit harus memenuhi 100% sebelum tindakan besar.

Namun calculation base belum final.

Perlu diputuskan apakah 100% berarti:

- estimasi tindakan besar;
- estimasi seluruh episode;
- patient responsibility;
- dasar lain.

Status:

```text
OPEN BUSINESS DECISION
```

---

# 28. Bill Sementara

Billing menyediakan:

```text
Cetak Bill Sementara
```

Bill sementara:

- tidak menutup bill;
- tidak menerbitkan financial clearance;
- charge masih dapat bertambah;
- room charge masih berjalan;
- tindakan/resep masih dapat masuk.

Dokumen harus menunjukkan:

```text
BILL SEMENTARA
BELUM MERUPAKAN TAGIHAN FINAL
```

---

# 29. Biaya Administrasi Rawat Inap

Requirement terbaru:

```text
7% dari eligible bill
maksimal Rp6.000.000
```

Formula:

```text
Admin Fee =
MIN(
  EligibleAmount × 7%,
  Rp6.000.000
)
```

Contoh:

```text
Rp50.000.000
× 7%
=
Rp3.500.000
```

Contoh cap:

```text
Rp100.000.000
× 7%
=
Rp7.000.000

Admin Fee Final:
Rp6.000.000
```

## Konflik Existing V2

Existing V2 sebelumnya mempunyai keputusan bahwa admin fee berupa nominal tetap dari master.

Karena itu:

```text
7% maksimal Rp6 juta
```

belum boleh mengganti rule lama secara diam-diam.

Harus ada keputusan:

```text
existing decision
→ superseded
```

bila requirement terbaru dinyatakan final.

Status:

```text
OPEN BUSINESS DECISION
```

Owner utama:

**Billing / Yasmina + Business Owner**

---

# 30. Insurance Guaranteed dan Patient Excess

Billing wajib memisahkan:

```text
Covered / Guaranteed
```

dan:

```text
Patient Responsibility / Excess
```

Contoh:

```text
Room Charge          Rp5.000.000
Insurance Covered    Rp4.500.000
Patient Excess       Rp  500.000
```

Allocation harus traceable sampai charge asal.

---

# 31. IGD → Rawat Inap

Jika pasien:

```text
IGD
↓
Rawat Inap
```

bill awal dapat tetap mempunyai reference terpisah.

Pada final settlement:

```text
IGD Bill ──────┐
               ├── Consolidated Settlement
RANAP Bill ────┘
```

Jangan melakukan destructive merge yang menghilangkan source.

Sistem harus tetap dapat menjawab:

```text
charge ini berasal dari IGD
atau
Rawat Inap
```

---

# 32. Close Bill

Ketika pasien akan pulang:

```text
Discharge Request
       ↓
Pre-Close Validation
       ↓
Close Bill
```

Setelah:

```text
CLOSED
```

posting charge normal harus terkunci.

Contoh:

```text
dokter menambah tindakan
↓
Billing Posting Guard
↓
REJECT
```

Lock wajib berada di backend.

---

# 33. Pre-Close Validation

Sebelum close:

- occupancy telah sesuai;
- tidak ada room correction yang belum selesai;
- charge pelayanan tersinkron;
- farmasi/retur resolved;
- lab/radiologi resolved;
- deposit direkonsiliasi;
- insurance allocation resolved;
- IGD/RANAP consolidation selesai jika applicable;
- admin fee final;
- patient excess diketahui;
- payment state tidak ambigu.

---

# 34. Financial Clearance

Billing menjadi source of truth.

Financial clearance hanya dapat diterbitkan apabila seluruh blocker relevan selesai.

Contoh:

```text
FinancialClearanceStatus = BLOCKED

Blockers:
- PATIENT_EXCESS_NOT_PAID
- ROOM_CORRECTION_PENDING
```

Setelah seluruh syarat selesai:

```text
FinancialClearanceStatus = CLEARED
```

Rawat Inap kemudian dapat menyelesaikan discharge.

---

# 35. Manual Financial Override

Manual override bukan default.

Digunakan hanya untuk exception yang mempunyai authority jelas.

Wajib menyimpan:

```text
User
Role
Reason
Timestamp
PreviousStatus
NewStatus
EncounterId
```

---

# 36. Kontrak A — Rawat Inap → Billing

## Producer

**Muhammad Hamzah**

## Consumer

**Yasmina**

Minimum contract:

```text
EncounterId
EpisodeId

RoomId
BedId
RoomClassId

OccupancyStartAt
OccupancyEndAt

ChangeType

SourceDetailId
Version
```

Possible `ChangeType`:

```text
ADMITTED
ROOM_ASSIGNED
ROOM_TRANSFERRED
ROOM_CLASS_CORRECTED
OCCUPANCY_CORRECTED
OCCUPANCY_CLOSED
DISCHARGE_REQUESTED
DISCHARGED
```

---

# 37. Kontrak B — Billing → Rawat Inap

## Producer

**Yasmina**

## Consumer

**Muhammad Hamzah**

Minimum contract:

```text
EncounterId

BillingStatus

DepositRequired
DepositBalance
DepositShortfall

CurrentCharge
Outstanding

FinancialClearanceStatus

Blockers[]
```

---

# 38. Aturan `grill-me`

PRD ini menjadi input bagi **dua sesi/pass berbeda**.

## Pass A — Muhammad Hamzah

```text
$grill-me

Modul:
Rawat Inap

Input:
PRD Integrasi Rawat Inap ↔ Billing

Gunakan hanya bagian:
- Scope Rawat Inap
- Shared Integration Scope
- requirement lintas modul yang menyentuh Rawat Inap

Jangan menggali internal Billing.
```

## Pass B — Yasmina

```text
$grill-me

Modul:
Billing

Input:
PRD Integrasi Rawat Inap ↔ Billing

Gunakan hanya bagian:
- Scope Billing
- Shared Integration Scope
- requirement lintas modul yang menyentuh Billing

Jangan menggali internal Rawat Inap.
```

---

# 39. Jika `grill-me` Menemukan Pertanyaan Modul Lain

Contoh Muhammad menemukan pertanyaan:

> Bagaimana formula insurance excess dihitung?

Jangan dijawab dalam Rawat Inap.

Catat:

```text
OUT-OF-SCOPE
Target Module: Billing
Owner: Yasmina
```

Sebaliknya bila Yasmina menemukan:

> Siapa boleh mengubah kelas aktual pasien?

Catat:

```text
OUT-OF-SCOPE
Target Module: Rawat Inap
Owner: Muhammad Hamzah
```

---

# 40. Workflow Agent Setelah PRD

## Rawat Inap

```text
Shared PRD
   ↓
grill-me Rawat Inap
   ↓
trace-existing-capabilities
   ↓
grill-me Closure Pass bila ada conflict/unknown
   ↓
requirement-completeness-gate
   ↓
hospital-domain-architect
(opsional bila dibutuhkan)
   ↓
design-business-module
   ↓
Human Approval
   ↓
plan-module-delivery
```

## Billing

```text
Shared PRD
   ↓
grill-me Billing
   ↓
trace-existing-capabilities
   ↓
grill-me Closure Pass bila ada conflict/unknown
   ↓
requirement-completeness-gate
   ↓
hospital-domain-architect
(opsional bila dibutuhkan)
   ↓
design-business-module
   ↓
Human Approval
   ↓
plan-module-delivery
```

---

# 41. Hasil Blueprint

Setelah workflow berjalan, hasilnya tetap terpisah:

```text
docs/module-blueprints/

├── rawat-inap/
│   ├── 00-interview-decisions.md
│   ├── 01-existing-capability-map.md
│   ├── 02-backend-architecture.md
│   ├── 03-frontend-architecture.md
│   ├── 04-prd-to-mvp.md
│   ├── contracts/
│   └── roadmap/
│
└── billing-kasir/
    ├── 00-interview-decisions.md
    ├── 01-existing-capability-map.md
    ├── 02-backend-architecture.md
    ├── 03-frontend-architecture.md
    ├── 04-prd-to-mvp.md
    ├── contracts/
    └── roadmap/
```

Shared PRD tidak menggantikan kedua blueprint tersebut.

---

# 42. Ownership Task Rawat Inap

## RANAP-INT-001 — Encounter & Episode Integration

Owner:

**Muhammad Hamzah**

Outcome:

Billing dapat menggunakan identifier episode yang konsisten.

---

## RANAP-INT-002 — Standardisasi Occupancy

Owner:

**Muhammad Hamzah**

Outcome:

Histori:

```text
Room
Bed
Class
StartAt
EndAt
```

tersedia dan stabil.

---

## RANAP-INT-003 — Mutasi & Koreksi Occupancy

Owner:

**Muhammad Hamzah**

Outcome:

Perubahan dapat dikonsumsi Billing tanpa kehilangan histori.

---

## RANAP-INT-004 — Billing Summary Consumption

Owner:

**Muhammad Hamzah**

Outcome:

Rawat Inap dapat membaca:

```text
Deposit
Current Charge
Outstanding
Financial Status
```

tanpa menghitung sendiri.

---

## RANAP-INT-005 — Financial Clearance Discharge Gate

Owner:

**Muhammad Hamzah**

Outcome:

Discharge final bergantung hasil financial clearance Billing.

---

# 43. Ownership Task Billing

## BILL-INT-001 — Consume Inpatient Episode

Owner:

**Yasmina**

Dependency:

```text
RANAP-INT-001
```

---

## BILL-INT-002 — Room Charge Engine

Owner:

**Yasmina**

Meliputi:

- room tariff;
- admission time;
- same-day transfer;
- payer pricing;
- effective policy.

Dependency:

```text
RANAP-INT-002
```

---

## BILL-INT-003 — Room Charge Adjustment / Reversal

Owner:

**Yasmina**

Dependency:

```text
RANAP-INT-003
```

---

## BILL-INT-004 — Provisional Bill

Owner:

**Yasmina**

---

## BILL-INT-005 — Deposit Policy

Owner:

**Yasmina**

---

## BILL-INT-006 — Insurance & Patient Excess

Owner:

**Yasmina**

---

## BILL-INT-007 — IGD/RANAP Consolidation

Owner:

**Yasmina**

---

## BILL-INT-008 — Close Bill & Posting Lock

Owner:

**Yasmina**

---

## BILL-INT-009 — Financial Clearance

Owner:

**Yasmina**

Blocking:

```text
RANAP-INT-005
```

---

# 44. Dependency Antar Developer

Contoh:

```text
RANAP-INT-001
        │
        └──>
        BILL-INT-001
```

```text
RANAP-INT-002
        │
        └──>
        BILL-INT-002
```

```text
RANAP-INT-003
        │
        └──>
        BILL-INT-003
```

Arah balik:

```text
BILL-INT-009
        │
        └──>
        RANAP-INT-005
```

Tidak semua dependency bergerak dari Rawat Inap ke Billing.

---

# 45. Contract-First Development

Muhammad dan Yasmina tidak perlu saling menunggu seluruh Modul selesai.

Contoh:

Occupancy endpoint belum selesai.

Yasmina dapat menggunakan contract fixture:

```json
{
  "encounterId": "ENC-001",
  "roomId": "ROOM-501",
  "roomClassId": "VIP",
  "startAt": "2026-09-16T15:00:00+07:00",
  "endAt": null,
  "sourceDetailId": "OCC-001",
  "version": 1
}
```

Sebaliknya Muhammad dapat menggunakan mock:

```json
{
  "encounterId": "ENC-001",
  "billingStatus": "OPEN",
  "depositBalance": 5000000,
  "outstanding": 3500000,
  "financialClearanceStatus": "BLOCKED",
  "blockers": [
    "PATIENT_EXCESS_NOT_PAID"
  ]
}
```

Kerja paralel baru aman ketika contract sudah disepakati.

---

# 46. Definition of Ready Integration Contract

Contract dianggap siap jika sudah mempunyai:

- nama contract;
- version;
- producer;
- consumer;
- identifier;
- fields;
- required/optional;
- enum/status;
- error behavior;
- retry behavior;
- idempotency;
- contoh request/response;
- compatibility policy;
- approval producer;
- approval consumer.

---

# 47. End-to-End Acceptance

Integration Epic belum selesai hanya karena:

```text
Muhammad DONE
+
Yasmina DONE
```

Harus lulus end-to-end.

## UAT-INT-001

Pasien baru masuk Rawat Inap.

Expected:

- Encounter aktif;
- Billing dapat mengenali episode.

## UAT-INT-002

Pasien mendapatkan kamar.

Expected:

- occupancy tersedia;
- room charge terbentuk satu kali.

## UAT-INT-003

Pasien masuk malam.

Expected:

- policy jam masuk diterapkan dengan benar.

## UAT-INT-004

Pasien pindah ke ICU di hari yang sama.

Expected:

- charge terbagi sesuai policy 50:50.

## UAT-INT-005

Kelas salah kemudian dikoreksi.

Expected:

- histori occupancy tetap;
- Billing reverse/adjust charge lama;
- corrected charge terbentuk.

## UAT-INT-006

Network retry terjadi.

Expected:

- tidak muncul duplicate room charge.

## UAT-INT-007

Bill sementara dicetak.

Expected:

- tagihan tidak terkunci;
- charge masih dapat bertambah.

## UAT-INT-008

Pasien mempunyai deposit kurang.

Expected:

- Rawat Inap membaca shortfall dari Billing.

## UAT-INT-009

Pasien menggunakan asuransi.

Expected:

- covered dan patient excess terpisah.

## UAT-INT-010

Pasien berasal dari IGD.

Expected:

- final settlement terkonsolidasi;
- source IGD tetap traceable.

## UAT-INT-011

Close Bill dilakukan.

Expected:

- posting normal berikutnya ditolak.

## UAT-INT-012

Outstanding masih ada.

Expected:

```text
Financial Clearance = BLOCKED
```

## UAT-INT-013

Settlement selesai.

Expected:

```text
Financial Clearance = CLEARED
```

dan Rawat Inap dapat melanjutkan discharge.

---

# 48. Open Business Decisions

## DEC-INT-001

Boundary jam:

```text
18:00 tepat
```

normal atau 50%?

## DEC-INT-002

Boundary:

```text
22:00 tepat
```

50% atau 20%?

## DEC-INT-003

Definisi operasional:

```text
00:00 tepat
```

## DEC-INT-004

Multiple same-day room transfer:

```text
A → B → C
```

bagaimana pembagian tarif?

## DEC-BILL-001

Admin fee:

```text
7%, cap Rp6 juta
```

versus existing nominal tetap.

Perlu supersede decision.

## DEC-BILL-002

Deposit 100% sebelum tindakan besar menggunakan dasar perhitungan apa?

---

# 49. Definition of Done Muhammad Hamzah

Rawat Inap dianggap selesai bila:

- episode/encounter stabil;
- occupancy historis tersedia;
- mutasi tersedia;
- koreksi tersedia;
- contract Rawat Inap → Billing terpenuhi;
- tidak ada calculation finansial duplikat di Rawat Inap;
- Billing summary dapat dikonsumsi;
- financial clearance dapat dikonsumsi;
- discharge gate berjalan;
- acceptance Rawat Inap lulus.

---

# 50. Definition of Done Yasmina

Billing dianggap selesai bila:

- episode RANAP dapat dikenali;
- occupancy dapat dikonsumsi;
- room charge idempotent;
- admission-time policy berjalan;
- transfer policy berjalan;
- adjustment/reversal berjalan;
- deposit berjalan;
- insurance/excess berjalan;
- provisional bill berjalan;
- close bill berjalan;
- posting lock berjalan;
- financial clearance tersedia;
- acceptance Billing lulus.

---

# 51. Definition of Done Integrasi

Integrasi baru dianggap selesai bila:

```text
Rawat Inap
       ↓
Billing
       ↓
Rawat Inap
```

bekerja end-to-end.

Minimal:

1. Encounter dapat dikenali.
2. Occupancy menghasilkan charge.
3. Retry tidak duplicate.
4. Mutasi menghasilkan charge yang benar.
5. Koreksi tidak menghapus history.
6. Deposit terbaca.
7. Insurance allocation terbaca.
8. Bill sementara tersedia.
9. Close bill mengunci posting.
10. Financial clearance berasal dari Billing.
11. Discharge menggunakan hasil clearance.
12. Semua source dapat diaudit.

---

# 52. Prinsip Final Ownership

Gunakan:

```text
1 Shared PRD
+
2 Module-Scoped Grill-Me
+
1 Shared Integration Contract
+
2 Module Blueprints
+
2 Module Roadmaps
+
Cross-Module Dependencies
+
1 End-to-End Acceptance
```

Bukan:

```text
1 Shared PRD
+
1 Grill-Me campuran
+
1 Task dimiliki dua developer
```

---

# 53. Prinsip Final Bisnis

Rawat Inap bertanggung jawab menjawab:

> **Apa yang benar-benar terjadi pada pasien?**

Billing bertanggung jawab menjawab:

> **Apa dampak finansial dari kejadian tersebut?**

Integrasi menjawab:

> **Data apa yang berpindah dari satu Modul ke Modul lainnya, kapan berpindah, dengan kontrak apa, dan bagaimana kedua Modul bereaksi ketika proses berhasil maupun gagal?**

---

# 54. Ringkasan Eksekusi untuk Developer

## Muhammad Hamzah

Gunakan PRD ini tetapi fokus hanya pada:

```text
Scope Rawat Inap
+
Shared Integration Scope
```

Jalankan lifecycle:

```text
grill-me
→ trace-existing-capabilities
→ closure bila perlu
→ requirement-completeness-gate
→ design
→ approval
→ plan-module-delivery
```

## Yasmina

Gunakan PRD ini tetapi fokus hanya pada:

```text
Scope Billing
+
Shared Integration Scope
```

Jalankan lifecycle:

```text
grill-me
→ trace-existing-capabilities
→ closure bila perlu
→ requirement-completeness-gate
→ design
→ approval
→ plan-module-delivery
```

## Shared Contract

Muhammad Hamzah dan Yasmina wajib sama-sama menyetujui:

```text
Rawat Inap → Billing contract
dan
Billing → Rawat Inap contract
```

sebelum task yang bergantung kepada contract tersebut dianggap siap implementasi.