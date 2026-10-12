# Status Modul Operasi

Diukur dari source 1 Oktober 2026, disegarkan 2 Oktober 2026.

| Sumbu | Status | Angka |
|---|---|---|
| Backend | `SUBSTANTIAL` | 9 controller · 10 service · 36 endpoint · 14 model · 7.229 baris |
| Frontend | `SUBSTANTIAL` | 11 halaman · 4 folder view · 8 service |
| Integrasi | `SUBSTANTIAL` | kontrak event outbox internal berdiri dan terbukti runtime |
| Verifikasi | `STRONG` | **120 uji** |

**Perkiraan ketuntasan, dipisah per sumbu:**

| Sumbu | Angka | Dasarnya |
|---|---|---|
| Source selesai | **~92%** | `BE-OPR-001`–`011` berdiri; yang belum ada adapter consumer Billing dan Inventory |
| Uji selesai | **~95%** | 120 uji, termasuk 41 uji izin dan audit 36 endpoint |
| Terbukti runtime | **~85%** | `401` dan `403` terbukti runtime 6 Oktober; `200` tertahan satu kolom |
| Terhalang luar | ~5% | satu migration (`20260901073655`), kontrak API Billing/Inventory, dan dua keputusan bisnis |

Acceptance `200`/`403`/`401` sudah disusun lengkap sebagai runbook siap jalan —
[`runbook-acceptance-be-opr-011.md`](runbook-acceptance-be-opr-011.md). Keputusan serial implant
terkumpul di [`keputusan-menunggu-pemilik-proses.md`](../keputusan-menunggu-pemilik-proses.md)
sebagai `K-2` dan `K-3`.

## Roadmap backend

`BE-OPR-001` sampai `BE-OPR-010` **Selesai**.

`BE-OPR-011` = **Implemented / Contract Verified / Runtime 401+403 Proven / 200 Blocked by One Column**.
Kontraknya terpasang penuh dan sudah diverifikasi. Sejak 6 Oktober 2026 **`401` dan `403` terbukti
runtime** — `403` dengan `EnforceClinicalPolicyForSuperAdmin` menyala, sehingga akun ujinya melewati
jalur keputusan yang sama persis seperti pengguna biasa. Yang tersisa hanya `200`, dan penahannya
tunggal: kolom `AspNetUserOrganization.SourceAssignmentId` tidak ada di basis data dev, sehingga
projection izinnya tidak dapat ditulis lewat jalur apa pun. Kolom itu milik migration
`20260901073655_A0AuthorizationIntegrityProjection` yang belum dijalankan. Rinciannya pada
[`verifikasi-kontrak-be-opr-011.md`](verifikasi-kontrak-be-opr-011.md).

Rincian beserta pencabutan tiga penghalang lama ada di
[`roadmap/backend-roadmap.md`](roadmap/backend-roadmap.md).

## Uji

`Tests/QuilvianSystemBackend.OperatingRoomTests` — 120 uji:

| Berkas | Jumlah | Yang dijaga |
|---|---|---|
| `PermissionMatrixTests` | 25 | `AccessPermission` pada setiap aksi controller |
| `IntegrationOutboxTests` | 10 | satu peristiwa satu pesan; percobaan ulang tidak menduplikasi |
| `StateTransitionTests` | 9 | tangga status kasus operasi |
| `ConcurrencyAndIdempotencyTests` | 8 | dua permintaan bersamaan dan kiriman berulang |
| `MaterialSerialTests` | 8 | `OPR014` serial implant unik dalam satu kasus |
| `SeedSmokeTests` | 4 | seeder demo menghasilkan data yang dapat dipakai |
| `AuditPrivacyTests` | 3 | jejak audit dan data yang tidak boleh terbaca |
| `ReportTests` | 34 | `BE-OPR-010` — penyaring, rentang tanggal, paging, bentuk keluaran |
| `ReportPermissionTests` | 13 | kontrak izin ketiga laporan, termasuk pembedaan izin material |

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
| Tes penerimaan `BE-OPR-011` | tinggi | kontrak diverifikasi (41 uji + audit 36 endpoint), `401` dan `403` terbukti runtime; `200` menunggu satu kolom migration |
| Indeks unik serial implant di basis data | sedang | butuh keputusan bisnis: syarat "belum digantikan koreksi" tidak dapat dinyatakan sebagai indeks tersaring |
| Adapter consumer Billing dan Inventory | sedang | menunggu kontrak pemilik API masing-masing |
| Penolakan `403` runtime per peran | — | ✅ **terbukti runtime 6 Oktober 2026**. Sisa yang terkait: `200` menunggu kolom `AspNetUserOrganization.SourceAssignmentId` dari migration `20260901073655` |
| Performa laporan | rendah | belum diukur pada volume besar |

## Bug yang sudah ditutup

| ID | Isi |
|---|---|
| [`BUG-OPR-BE-001`](bug-opr-be-001-tanggal-akhir-tidak-inklusif.md) | ✅ Tanggal akhir laporan membuang seluruh data hari itu. Ketiga laporan kini memakai penolong `NormalkanRentang`; rentang terbalik ditolak 400 |

## Penghalang di luar modul

| Hal | Catatan |
|---|---|
| Rantai migration | [`docs/engineering/blocker-rantai-migration.md`](../../engineering/blocker-rantai-migration.md) — basis data dev dibangun dari baseline hasil squash |
| Startup seeder | [`docs/engineering/blocker-startup-seeder-tabel-hilang.md`](../../engineering/blocker-startup-seeder-tabel-hilang.md) — dua seeder mematikan startup pada basis data yang belum lengkap |
| `LoggerService` audit user | [`docs/engineering/task-core-logger-audit-user.md`](../../engineering/task-core-logger-audit-user.md) — task Core/shared, sengaja tidak diperbaiki dari modul Operasi |
