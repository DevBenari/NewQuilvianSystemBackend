# Farmasi — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `PHA-BP-001` |
| Module name | Farmasi |
| Revision | `4` |
| Module status | `SUBSTANTIAL` — alurnya kini tersambung ujung ke ujung dan terbukti runtime |
| Current phase | Gelombang Financial Clearance **terverifikasi runtime**; menunggu approval policy charge pra-dispense milik owner Billing |
| Last verified at | `2026-10-01T15:40:00+07:00` |
| Backend source SHA | `98355a495956dea291b2f195d642fb91ead1cb71` (branch `Ikbal`) |
| Frontend source SHA | `f43dbdeb1612dfdb6b2e4973cf45b5dafd7dd56e` (branch `Ikbalv2`) |

## Yang berubah pada revisi 4

Empat hal tertutup sejak revisi 3, dan satu hal baru terbuka.

| Hal | Keadaan |
|---|---|
| `GAP-PHA-FE-001` workflow farmasi tengah tanpa UI | **Selesai** — panel tiga tab pada detail resep: telaah, penyiapan, telaah obat akhir |
| `GAP-PHA-FE-002` Etiket Obat tanpa entry point | **Selesai** — bagian Etiket Obat pada detail resep, tanpa butir menu baru |
| `GAP-PHA-BE-001` resep tidak pernah dapat difinalkan | **Selesai** — keputusan status awal dipindah ke `PrescriptionWorkflowService.ApplyInitialClinicalState`, dijaga 8 uji regresi |
| `BE-BKC-067` penerbitan financial clearance | **Terverifikasi runtime** — rantai penuh terbukti; rincian pada [`verifikasi-runtime-be-bkc-067.md`](verifikasi-runtime-be-bkc-067.md) |
| Producer tagihan obat | **Baru** — `PrescriptionBillingChargeProducer` mengirim tagihan pada transisi tahap 1 → 2. **Belum menyala** sampai policy Billing disetujui; lihat [`requirement-billing-charge-pra-dispense.md`](requirement-billing-charge-pra-dispense.md) |

Tangga tahap yang berlaku dan sudah terbukti ujung ke ujung:

```
1 WaitingForClinicalFinalization
  -> 2 WaitingForPayment      finalisasi konsultasi yang sah
  -> 4 QueuedAtPharmacy       konsumsi surat clearance Billing (PHA-DEC-069, melompati tahap 3)
  -> 5 VerifiedByPharmacy     telaah disetujui
  -> 6 BeingPrepared          penyiapan dimulai
  -> 12 WaitingForFinalCheck  penyiapan diselesaikan
  -> 7 ReadyForHandover       telaah obat akhir lolos
```

| Sumbu | Status |
|---|---|
| Backend | `SUBSTANTIAL` |
| Frontend | `SUBSTANTIAL` |
| Integrasi | `SUBSTANTIAL` — clearance terbukti ujung ke ujung; producer tagihan menunggu approval Billing |
| Verifikasi | `MODERATE` — **17 uji** regresi ditambah bukti runtime penuh; masih tipis untuk 31.982 baris |

**Perkiraan ketuntasan: ~90%.** Diukur ketat "berfungsi hari ini di integration", producer
tagihan belum menyala sehingga angkanya lebih dekat ~86%.

Uji pada `Tests/QuilvianSystemBackend.PharmacyTests`:

| Berkas | Jumlah | Yang dijaga |
|---|---|---|
| `PrescriptionFulfillmentStageTests` | 8 | regresi `GAP-PHA-BE-001`; tangga tahap pemenuhan dan penomorannya |
| `BillingChargeProducerTests` | 9 | kunci idempotensi deterministik dan nilainya yang sudah terpakai; gerbang resep tanpa item maupun tanpa harga; penolakan tegas saat kategori tarif farmasi tidak ada; `SourceStatus` dan `ContractVersion` yang diterima Billing |

## Permukaan yang sudah berdiri

Dihitung dari source pada SHA di atas, bukan dari perencanaan.

| Hal | Jumlah |
| --- | --- |
| Controller | 23 |
| Service | 38 |
| Model | 35 |
| Endpoint | 137 |
| Baris kode `Areas/HealthServices/PharmacyManagement` | 31.622 |
| Halaman frontend `pharmacy` | 22 |

Sebagai pembanding pada repositori yang sama: Operasi 7.229 baris dengan 36 endpoint, Gizi 4.776
baris dengan 37 endpoint. Farmasi adalah modul dengan permukaan terbesar.

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `PHA-PH-001` sampai `PHA-PH-008` | Gelombang Financial Clearance (`PHA-BE-004`/`005`/`006`) | `PHA-PH-010`; lanjutan gelombang Financial Clearance menunggu Billing |

`PHA-PH-008` dinyatakan selesai berdasarkan source, bukan berdasarkan laporan task: resolver
routing Depo ada sebagai `PharmacyDepotRoutingService.cs` beserta `PharmacyDepotRoutingDtos.cs`.
Laporan task `PHA-BE-001` tidak pernah ditulis, sehingga bukti acceptance-nya belum tercatat.

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `SUBSTANTIAL` | `SUBSTANTIAL` | `PARTIAL` | `WEAK` |

`Verification` dinyatakan `WEAK` karena **Farmasi belum memiliki satu pun uji otomatis**.
Satu-satunya proyek uji pada repositori adalah `Tests/QuilvianSystemBackend.OperatingRoomTests`.
Dengan 137 endpoint, itu risiko terbesar yang masih tersisa.

Pengujiannya diserahkan ke analis penguji (keputusan 29 September 2026). Karena itu task yang
sumbernya sudah lengkap dan hanya menunggu pembuktian ditandai **`Selesai — kurang tes`**, bukan
`Sebagian`: yang tertinggal bukan pekerjaan pembangunan. Penandaan itu **tidak** dipakai untuk
task yang masih menunggu keputusan bisnis atau dependency modul lain — keduanya bukan soal
pengujian dan tetap ditandai apa adanya.

## Gelombang Financial Clearance

| Task | Status | Yang sudah ada | Yang belum |
| --- | --- | --- | --- |
| `PHA-BE-004` | 🟡 | `PhmPrescriptionFinancialProjection`, `PrescriptionFinancialClearanceService`, configuration, DI, migration | QBE Conformance, verifikasi runtime, **protokol pengakuan surat**, **keputusan nilai kolom pembayaran saat `REVOKED`**. Dua yang terakhir keputusan bisnis, bukan pengujian — karena itu tetap 🟡 |
| `PHA-BE-005` | 🟡 | Empat gerbang penahanan terpasang | Verifikasi runtime, tetapi tertahan `BE-BKC-068` milik `billing-kasir`. Bukan semata soal pengujian |
| `PHA-BE-006` | **Selesai — kurang tes** | `FinancialClearance` pada response detail resep dan layar kerja | Hanya verifikasi runtime, tanpa dependency dan tanpa keputusan terbuka. Diserahkan ke analis penguji |
| `PHA-FE-002` | `NOT_STARTED` | — | Tidak ditemukan jejak `financialClearance` maupun `clearanceStatus` pada source frontend |

### Koreksi terhadap catatan sebelumnya

Roadmap backend menyatakan migration `AddPrescriptionFinancialProjection` **belum dijalankan ke
database mana pun**. Itu tidak lagi benar. Diperiksa 29 September 2026 pada basis data
pengembangan `localhost/QuilvianNewDevIkbalFr`:

- `20260922060000_AddPrescriptionFinancialProjection` tercatat pada `__EFMigrationsHistory`;
- tabel `PhmPrescriptionFinancialProjection` ada.

Lingkungan lain belum diperiksa. Wewenang eksekusi migration tetap terpisah sebagaimana
`PHA-DEC-071`.

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |
| `BE-BKC-067` | Surat clearance belum terbit dari sisi penerbit | `billing-kasir` | `PHA-BE-004` | Source konsumsi sudah siap menunggu masukan |
| `BE-BKC-068` | Permukaan pemeriksaan ulang belum tersedia | `billing-kasir` | `PHA-BE-005` | Gerbang penahanan sudah terpasang |
| `PHA-DEP-001` | Billing/Kasir belum terbukti authoritative untuk pembayaran, jaminan, reversal, dan refund | Billing/Finance | Kontrak integrasi dan payment gate | Penilaian requirement dan desain inventory dapat dilanjutkan |
| `PHA-DEP-002` | Saldo, ledger, reservasi atomik, batch, dan mutasi stok belum tersedia | Pharmacy/Inventory | Dispensing dan persediaan | Arsitektur domain dan roadmap dapat disusun |
| `PHA-DEP-003` | SOP dan approval formal kewenangan apoteker, checker kedua, retur, recall, obat khusus belum tersedia | Pharmacy/Clinical Governance | Permission dan safety control | Slice routing Depo dapat dirancang independen |

Seluruh lanjutan gelombang Financial Clearance menunggu sisi penerbit Billing berdiri. Itu bukan
urutan yang dipilih: tanpa surat yang terbit, slice ini tidak punya masukan apa pun.

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |
| `00-interview-decisions.md` | `36d7eca7cd3d4b3f1f6520a6fe9340936cced320` | `4585f463ea3498e19fcdf2c475f567b052ec152a` | Sinkronisasi metadata keputusan; keputusan bisnis tetap berasal dari persetujuan owner |
| `01-existing-capability-map.md` | `39b8b69f...` | `4585f463ea3498e19fcdf2c475f567b052ec152a` | Map belum dinormalisasi ke struktur template dan belum mencerminkan 137 endpoint yang sekarang ada |
| `roadmap/backend-roadmap.md` | — | — | Menyatakan migration clearance belum dijalankan; lihat koreksi di atas |
| `PHA-BE-001` | — | — | Source ada, laporan task tidak pernah ditulis; bukti acceptance belum tercatat |

## Next recommended task

1. **Tulis laporan acceptance `PHA-BE-001`** supaya `PHA-PH-008` punya bukti, bukan hanya source.
2. **Serahkan `PHA-BE-006` ke analis penguji** untuk verifikasi runtime.
3. **Putuskan protokol pengakuan surat dan nilai kolom pembayaran saat `REVOKED`** — dua keputusan
   yang menahan `PHA-BE-004`, dan tidak akan terselesaikan oleh pengujian.

Pengujian otomatis Farmasi diserahkan ke analis penguji. Bila nanti dibuat dari sisi
pembangunan, polanya sudah terbukti pada `Tests/QuilvianSystemBackend.OperatingRoomTests`: xunit,
SQLite dalam memori, dibangun dengan `-p:SkipMigrationMetadata=true`.

Setelah Billing menerbitkan `BE-BKC-067`/`068`, lanjutkan verifikasi runtime `PHA-BE-004`/`005`/
`006` dan kerjakan `PHA-FE-002`.

## Status contract

`PARTIAL` dipakai karena permukaan modul sudah berdiri luas dan dipakai, tetapi dua hal belum
terpenuhi: gelombang terakhir belum dapat diselesaikan tanpa Billing, dan belum ada bukti
otomatis yang menjaga perilaku 137 endpoint itu tetap benar ketika kode berubah.
