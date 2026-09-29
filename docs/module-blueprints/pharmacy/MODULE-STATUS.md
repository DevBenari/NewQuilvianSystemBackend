# Farmasi — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `PHA-BP-001` |
| Module name | Farmasi |
| Revision | `3` |
| Module status | `PARTIAL` — modul terbesar yang sudah berjalan; gelombang terakhir menunggu Billing |
| Current phase | Gelombang Financial Clearance — `PHA-BE-004`/`005`/`006` source selesai, verifikasi belum |
| Last verified at | `2026-09-29T09:45:00+07:00` |
| Backend source SHA | `4585f463ea3498e19fcdf2c475f567b052ec152a` (branch `Ikbal`) |
| Frontend source SHA | `1b4209ce9d10039565860c7d67c23e4c859754c5` (branch `Ikbalv2`) |

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

`Verification` sengaja dinyatakan `WEAK` dan itu temuan paling penting pada pemutakhiran ini:
**Farmasi belum memiliki satu pun uji otomatis.** Satu-satunya proyek uji pada repositori adalah
`Tests/QuilvianSystemBackend.OperatingRoomTests`. Dengan 137 endpoint, itu risiko terbesar yang
masih sepenuhnya berada di tangan tim ini dan tidak menunggu pihak mana pun.

## Gelombang Financial Clearance

| Task | Status | Yang sudah ada | Yang belum |
| --- | --- | --- | --- |
| `PHA-BE-004` | 🟡 | `PhmPrescriptionFinancialProjection`, `PrescriptionFinancialClearanceService`, configuration, DI, migration | QBE Conformance, verifikasi runtime, protokol pengakuan surat, keputusan nilai kolom pembayaran saat `REVOKED` |
| `PHA-BE-005` | 🟡 | Empat gerbang penahanan terpasang | Verifikasi runtime |
| `PHA-BE-006` | 🟡 | `FinancialClearance` pada response detail resep dan layar kerja | Verifikasi runtime |
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

Dua hal yang tidak menunggu siapa pun:

1. **Mulai proyek uji Farmasi.** Polanya sudah ada dan terbukti pada
   `Tests/QuilvianSystemBackend.OperatingRoomTests`: xunit, SQLite dalam memori, dibangun dengan
   `-p:SkipMigrationMetadata=true`. Mulai dari resolver routing Depo (`PHA-BE-002`, sudah
   direncanakan) dan gerbang penahanan `PHA-BE-005`, karena keduanya aturan yang paling mudah
   rusak diam-diam.
2. **Tulis laporan acceptance `PHA-BE-001`** supaya `PHA-PH-008` punya bukti, bukan hanya source.

Setelah Billing menerbitkan `BE-BKC-067`/`068`, lanjutkan verifikasi runtime `PHA-BE-004`/`005`/
`006` dan kerjakan `PHA-FE-002`.

## Status contract

`PARTIAL` dipakai karena permukaan modul sudah berdiri luas dan dipakai, tetapi dua hal belum
terpenuhi: gelombang terakhir belum dapat diselesaikan tanpa Billing, dan belum ada bukti
otomatis yang menjaga perilaku 137 endpoint itu tetap benar ketika kode berubah.
