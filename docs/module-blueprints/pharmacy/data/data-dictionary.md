# Farmasi — Kamus Data

Berkas ini lahir pada 21 September 2026 bersama slice Financial Clearance, mengikuti struktur
keluaran canonical yang berlaku sekarang.

`erd/data-dictionary.md` yang sudah ada **tidak disentuh** dan tetap berlaku untuk slice Routing
Depo beserta tabel Farmasi yang sudah berjalan. Pemindahan isinya ke berkas ini adalah pekerjaan
perapian tersendiri, bukan bagian slice ini — preseden yang sama dipakai `billing-kasir`, yang
memiliki `data/` baru sementara `erd/` lamanya dibiarkan apa adanya.

Seluruh tabel mewarisi `IdentityModel`. Sepuluh kolom audit warisannya — pembuatan, pemutakhiran,
penghapusan, pembatalan beserta pelakunya — **tidak** diulang per tabel di bawah. Penghapusan
bersifat penandaan, bukan penghapusan sungguhan.

## Tabel pada slice Financial Clearance

| Tabel | Status | Modul pemilik |
| --- | --- | --- |
| `PhmPrescriptionFinancialProjection` | **Baru** | `pharmacy` |
| `PhmPrescription` | Sudah ada | `pharmacy` |
| `BilPrescriptionClearanceHandoff` | Sudah ada di modul lain | `billing-kasir` — dibaca, **MUST NOT** ditulis |

## `PhmPrescriptionFinancialProjection` — Baru

Salinan keadaan finansial sebuah resep menurut Billing. **Bukan** sumber kebenaran.

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | — | PK | Tidak | — |
| `PrescriptionId` | `Guid` | Ya | — | — | **Unik** | Tidak | Foreign key ke resep. Tepat satu baris per resep |
| `InvoiceId` | `Guid` | Ya | — | — | Index | Tidak | Identitas tagihan di Billing, **tanpa** foreign key lintas modul |
| `ClearanceStatus` | `string` | Ya | `UNKNOWN` | 25 | Index | Tidak | `CLEARED`, `REVOKED`, `UNKNOWN`, `PENDING_VERIFICATION`, `STALE` |
| `FinancialOutcome` | `string` | Tidak | `null` | 30 | — | Tidak | `PAID`, `INSURANCE_APPROVED`, `PAYMENT_WAIVED`. Kosong selain saat `CLEARED` |
| `ReasonCode` | `string` | Tidak | `null` | 40 | — | Tidak | Sebab perubahan terakhir, disalin apa adanya dari surat |
| `FinancialVersion` | `long` | Ya | `0` | — | — | Tidak | Nomor versi surat terakhir yang diterima. Surat bernomor lebih rendah ditolak |
| `EffectiveAt` | `DateTimeOffset?` | Tidak | `null` | — | — | Tidak | Waktu berlaku menurut Billing, bukan waktu disalin |
| `SourceHandoffId` | `Guid?` | Tidak | `null` | — | — | Tidak | Surat yang menghasilkan keadaan ini. **Sengaja bukan foreign key** — tidak mengunci tabel milik modul lain |
| `SyncState` | `string` | Ya | `NEVER_SYNCED` | 30 | Index | Tidak | `NEVER_SYNCED`, `SYNCED`, `FAILED` |
| `SyncedAt` | `DateTimeOffset?` | Tidak | `null` | — | — | Tidak | Kapan salinan terakhir berhasil diperbarui |
| `LastAttemptAt` | `DateTimeOffset?` | Tidak | `null` | — | — | Tidak | Kapan percobaan terakhir dilakukan, berhasil atau tidak |
| `RetryCount` | `int` | Ya | `0` | — | — | Tidak | Jumlah percobaan gagal berturut-turut |
| `ErrorMessage` | `string` | Tidak | `null` | 1000 | — | Tidak | Sebab kegagalan terakhir. **MUST NOT** memuat data klinis |
| `CorrelationId` | `Guid?` | Tidak | `null` | — | — | Tidak | Rantai telusur ke peristiwa di Billing |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | Tidak | Kendali konkurensi optimistik |

**Perilaku hapus:** `DeleteBehavior.Restrict` pada relasi ke resep.

**Nol kolom klinis.** Tidak ada nama obat, dosis, aturan pakai, diagnosis, maupun nama pasien.
Itu hasil rancangan kontrak di sisi Billing, yang memang hanya mengirim identitas dan keadaan.

### Mengapa satu baris per resep, bukan riwayat

Riwayat surat sudah dijaga permanen di sisi Billing (`BKC-DEC-109`). Menyalin riwayat itu ke
Farmasi menciptakan salinan kedua yang dapat menyimpang, dan menimbulkan pertanyaan mana yang
benar ketika keduanya berbeda. Farmasi cukup memegang keadaan terkini beserta nomor versinya —
cukup untuk menolak surat basi, dan cukup untuk direkonsiliasi kapan saja.

## `PhmPrescription` — Sudah ada, nol kolom berubah

Kolom kunci yang dipakai aturan bisnis slice ini. Berkas model:
`Areas/HealthServices/PharmacyManagement/Models/PhmPrescription.cs`.

| Kolom | Peran pada slice ini | Perubahan |
| --- | --- | --- |
| `Id` | Dirujuk salinan finansial | Tidak ada |
| `FulfillmentStatus` | Gerbang memeriksa dan memindahkannya dari menunggu pembayaran ke antrean | Tidak ada perubahan kolom; yang berubah adalah **adanya** jalur yang memindahkannya |
| `PaymentStatus` | Salinan kenyamanan, satu penulis | Tidak ada perubahan kolom; yang berubah adalah siapa yang berhak menulisnya |
| `PrescriptionStatus` | Tidak disentuh slice ini | Tidak ada |

Tiga kolom nominal yang perlu diperhatikan pembaca berikutnya: `CoveredAmount`,
`PatientPayAmount`, dan sejenisnya pada resep dan barisnya **tidak dipakai** slice ini.
`RJ-BIL-CONFLICT-001` menandainya sebagai angka finansial milik modul klinis yang kepemilikannya
sedang dipindahkan ke Billing. Memakainya sebagai dasar keputusan finansial akan memutar balik
`PHA-DEC-063`. Slice ini sengaja tidak menyentuhnya, dan perapiannya pekerjaan tersendiri.

## Skema DDL

Bagian ini **dokumentasi bentuk**, bukan skrip yang dijalankan. Skema sebenarnya dibangun EF
Core dari berkas configuration.

```sql
CREATE TABLE public."PhmPrescriptionFinancialProjection" (
    "Id"                uuid          NOT NULL,
    "PrescriptionId"    uuid          NOT NULL,
    "InvoiceId"         uuid          NOT NULL,
    "ClearanceStatus"   varchar(25)   NOT NULL DEFAULT 'UNKNOWN',
    "FinancialOutcome"  varchar(30)   NULL,
    "ReasonCode"        varchar(40)   NULL,
    "FinancialVersion"  bigint        NOT NULL DEFAULT 0,
    "EffectiveAt"       timestamptz   NULL,
    "SourceHandoffId"   uuid          NULL,
    "SyncState"         varchar(30)   NOT NULL DEFAULT 'NEVER_SYNCED',
    "SyncedAt"          timestamptz   NULL,
    "LastAttemptAt"     timestamptz   NULL,
    "RetryCount"        integer       NOT NULL DEFAULT 0,
    "ErrorMessage"      varchar(1000) NULL,
    "CorrelationId"     uuid          NULL,
    "RowVersion"        uuid          NOT NULL,
    CONSTRAINT "PK_PhmPrescriptionFinancialProjection" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PhmPrescriptionFinancialProjection_Prescription"
        FOREIGN KEY ("PrescriptionId")
        REFERENCES public."PhmPrescription" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_PhmPrescriptionFinancialProjection_Prescription"
    ON public."PhmPrescriptionFinancialProjection" ("PrescriptionId");
CREATE INDEX "IX_PhmPrescriptionFinancialProjection_ClearanceStatus"
    ON public."PhmPrescriptionFinancialProjection" ("ClearanceStatus");
CREATE INDEX "IX_PhmPrescriptionFinancialProjection_SyncState"
    ON public."PhmPrescriptionFinancialProjection" ("SyncState");
CREATE INDEX "IX_PhmPrescriptionFinancialProjection_Invoice"
    ON public."PhmPrescriptionFinancialProjection" ("InvoiceId");
```

Trace `PHA-DEC-063`–`070`, `PHA-DES-001`–`006`.
