# Data Dictionary — Rawat Jalan Billing

Seluruh model mewarisi `IdentityModel`: `CreateDateTime`, `CreateBy`, `UpdateDateTime`,
`UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan
`IsDelete`. Penghapusan adalah penandaan, bukan penghapusan baris. DDL di bawah dokumentasi
bentuk EF Core, bukan skrip yang dijalankan.

## Working tree operational tables

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `BilFolio.Id` | uuid | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Identitas folio |
| `BilFolio.EncounterId` | uuid | Ya | — | UK aktif | Registration Encounter | Restrict | Tidak | Satu folio aktif per encounter |
| `BilFolio.Status` | int | Ya | `Open` | — | — | — | Tidak | Status folio |
| `BilFolio.Version` | int | Ya | `1` | concurrency | — | — | Tidak | Optimistic concurrency |
| `BilChargeLine.Id` | uuid | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Identitas charge line |
| `BilChargeLine.FolioId` | uuid | Ya | — | Index | `BilFolio` | Restrict | Tidak | Folio induk |
| `BilChargeLine.SourceContext` | varchar(50) | Ya | kosong | unique tuple | Clinical source | — | Tidak | Context sumber fact |
| `BilChargeLine.SourceAggregateId` | uuid | Ya | — | unique tuple | Source aggregate | — | Tidak | Aggregate sumber |
| `BilChargeLine.MilestoneFactId` | uuid | Ya | — | unique tuple | Source fact | — | Tidak | Fact milestone |
| `BilChargeLine.MilestoneFactVersion` | int | Ya | — | unique tuple | Source fact | — | Tidak | Versi fact |
| `BilChargeLine.CalculationStatus` | int | Ya | `Received` | — | — | — | Tidak | Status kalkulasi |
| `BilChargeLine.GrossAmount` | numeric(18,2) | Tidak | — | — | — | — | Tidak | Nominal gross jika sudah diketahui |
| `BilChargeLine.EligibleAmount` | numeric(18,2) | Tidak | — | — | — | — | Tidak | Nominal eligible |
| `BilChargeComponent.ComponentKey` | varchar(100) | Ya | kosong | UK per line | `BilChargeLine` | Restrict | Tidak | Kunci komponen |
| `BilChargeComponent.Quantity` | numeric(18,6) | Tidak | — | — | — | — | Tidak | Quantity aktual |
| `BilChargeComponent.TariffSnapshot` | jsonb | Tidak | — | — | — | — | Tidak | Snapshot tarif |
| `BilChargeComponent.RuleSnapshot` | jsonb | Tidak | — | — | — | — | Tidak | Snapshot rule |
| `BilChargeComponent.CalculatedAmount` | numeric(18,2) | Tidak | — | — | — | — | Tidak | Hasil kalkulasi |
| `BilProcessingEffect.IdempotencyKey` | varchar(128) | Ya | — | UK bersama consumer/operation | — | — | Tidak | Kunci retry stabil |
| `BilProcessingEffect.RequestFingerprint` | varchar(64) | Ya | — | — | — | — | Tidak | Hash input material |
| `BilProcessingEffect.Outcome` | int | Ya | `Received` | — | — | — | Tidak | Outcome processing |
| `BilProcessingEffect.ErrorMessage` | varchar(1000) | Tidak | — | — | — | — | Tidak | Pesan error operasional |

### Bentuk DDL ringkas

```sql
-- Dokumentasi bentuk EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."BilFolio" (
  "Id" uuid NOT NULL,
  "EncounterId" uuid NOT NULL,
  "Status" integer NOT NULL,
  "Version" integer NOT NULL,
  "IsActive" boolean NOT NULL,
  CONSTRAINT "PK_BilFolio" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_BilFolio_EncounterId"
  ON public."BilFolio" ("EncounterId") WHERE "IsDelete" = false;
```

DDL lengkap harus diambil ulang dari EF configuration saat implementation task disetujui.

