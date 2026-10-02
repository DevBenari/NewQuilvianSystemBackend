# Status Modul Operasi

Diukur dari source 1 Oktober 2026.

| Sumbu | Status | Angka |
|---|---|---|
| Backend | `SUBSTANTIAL` | 9 controller · 10 service · 36 endpoint · 14 model · 7.229 baris |
| Frontend | `SUBSTANTIAL` | 11 halaman · 4 folder view · 8 service |
| Integrasi | `SUBSTANTIAL` | kontrak event outbox internal berdiri dan terbukti runtime |
| Verifikasi | `STRONG` | **70 uji** — tertinggi dari tiga modul |

**Perkiraan ketuntasan: ~82%.**

## Roadmap backend

`BE-OPR-001` sampai `BE-OPR-010` **Selesai**. `BE-OPR-011` **Selesai — kurang tes**: pengujian
penerimaannya didelegasikan pemilik kebutuhan kepada analisnya.

Rincian beserta pencabutan tiga penghalang lama ada di
[`roadmap/backend-roadmap.md`](roadmap/backend-roadmap.md).

## Uji

`Tests/QuilvianSystemBackend.OperatingRoomTests` — 70 uji:

| Berkas | Jumlah | Yang dijaga |
|---|---|---|
| `PermissionMatrixTests` | 25 | `AccessPermission` pada setiap aksi controller |
| `IntegrationOutboxTests` | 10 | satu peristiwa satu pesan; percobaan ulang tidak menduplikasi |
| `StateTransitionTests` | 9 | tangga status kasus operasi |
| `ConcurrencyAndIdempotencyTests` | 8 | dua permintaan bersamaan dan kiriman berulang |
| `MaterialSerialTests` | 8 | `OPR014` serial implant unik dalam satu kasus |
| `SeedSmokeTests` | 4 | seeder demo menghasilkan data yang dapat dipakai |
| `AuditPrivacyTests` | 3 | jejak audit dan data yang tidak boleh terbaca |

## Kontrak event outbox

`OprIntegrationDelivery` membawa `EventId` (indeks unik), `EventType`, `EventVersion`,
`OccurredAt`, dan `PayloadJson` bertipe `jsonb`. `EventId` deterministik:

```
EventId = SHA256("opr-event|{destination}|{idempotencyKey}")[..16]
```

Keunikannya berlapis dua — `EventId` unik dan `(Destination, IdempotencyKey)` unik — sehingga
pengiriman ulang tidak pernah melahirkan pesan kedua. Terbukti runtime: tiga permintaan
menghasilkan dua pesan.

Kegagalan publikasi **tidak** membatalkan transaksi klinis, dan outbox dapat diproses ulang.
Tidak ada dependency baru ke consumer tertentu; adapter ke Billing dan Inventory tetap menunggu
kontrak pemiliknya.

## Sisa pekerjaan

| Hal | Prioritas | Catatan |
|---|---|---|
| Tes penerimaan `BE-OPR-011` | tinggi | didelegasikan ke analis pemilik kebutuhan; bukan pekerjaan source |
| Indeks unik serial implant di basis data | sedang | butuh keputusan bisnis: syarat "belum digantikan koreksi" tidak dapat dinyatakan sebagai indeks tersaring |
| Adapter consumer Billing dan Inventory | sedang | menunggu kontrak pemilik API masing-masing |
| Uji laporan (`BE-OPR-010`) | rendah | filter, rentang tanggal, paging, privasi, dan performa belum punya uji tersendiri |

## Penghalang di luar modul

| Hal | Catatan |
|---|---|
| Rantai migration | [`docs/engineering/blocker-rantai-migration.md`](../../engineering/blocker-rantai-migration.md) — basis data dev dibangun dari baseline hasil squash |
| `LoggerService` audit user | [`docs/engineering/task-core-logger-audit-user.md`](../../engineering/task-core-logger-audit-user.md) — task Core/shared, sengaja tidak diperbaiki dari modul Operasi |
