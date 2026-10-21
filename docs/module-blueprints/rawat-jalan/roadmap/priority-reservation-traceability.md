# Traceability — Reservasi Pasien Prioritas (Amendment RP)

| Field | Nilai |
|---|---|
| Roadmap | `RJ-PRI-RM-BE-001` r1 dan `RJ-PRI-RM-FE-001` r1 (`approved`, `RJ-PRI-DEC-031`) |
| Blueprint | `rawat-jalan` revisi `33` `approved` (`RJ-PRI-DEC-030`) |
| Kontrak | `RJ-PRI-RESV-001@1.0.0` `approved` |
| SHA | BE `551e1cbb`, FE `1fd858322` |
| Hash masukan | `00-interview-decisions.md` `089ef11d6812`, `04-prd-to-mvp.md` `a7962b2158b1`, `testing/acceptance-test-matrix.md` `ce8039c0fb65` (manifest Revisi 33) |

## 1. Requirement ke task dan bukti

| Requirement | Decision | Desain | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|
| `FR-RP-001`, `002` | `RJ-PRI-DEC-004` | `02` RP.4.1 | `BE-RJP-001` | — | `R-RP-01`, `02`, `UAT-RP-01`, `02` | ✅ 2026-10-16 |
| `FR-RP-010` | `RJ-PRI-DEC-005` | `02` RP.4.3 | `BE-RJP-003` | `FE-RJP-001` | `R-RP-03`, `UAT-RP-03` | ✅ 2026-10-16 |
| `FR-RP-011`, `012` | `RJ-PRI-DEC-025` | `02` RP.4.3 | `BE-RJP-004` | — | `R-RP-04`, `05`, `UAT-RP-04` | ✅ 2026-10-16 |
| `FR-RP-013` | `RJ-PRI-DEC-025` | `03` FE-RP-05 | — | `FE-RJP-001` | `UAT-RP-03` (layar) | ✅ 2026-10-16 |
| `FR-RP-020` s.d. `022` | `RJ-PRI-DEC-006`, `007`; `CRM-DEC-032` | `02` RP.4.2 | `BE-RJP-002` (memenuhi `BE-CRM-022`) | — | `R-RP-06`, `07`, `UAT-RP-05`, `06`, `crm-membership` `AT-36` | ✅ 2026-10-16 |
| `FR-RP-030` | `RJ-PRI-DEC-008`, `009` | `02` RP.4.4, data RP-2 | `BE-RJP-005`, `BE-RJP-006` | `FE-RJP-003`, `FE-RJP-004` | `R-RP-08`, `UAT-RP-07`, `08` | ✅ 2026-10-16, 2026-10-19, 2026-10-20 |
| `FR-RP-031` | `RJ-PRI-DEC-021` | `INV-RP-01` | `BE-RJP-005`, `BE-RJP-006` | `FE-RJP-004` | `R-RP-09`, `UAT-RP-09` | ✅ 2026-10-16, 2026-10-19, 2026-10-20 |
| `FR-RP-032` | `RJ-PRI-DEC-021` | `VAL-RP-06` | `BE-RJP-006` | `FE-RJP-004` | `R-RP-08` (varian) | ✅ 2026-10-19, 2026-10-20 |
| `FR-RP-033` | `RJ-PRI-DEC-012`, `023` | state RP-B | `BE-RJP-007` | `FE-RJP-005` | `R-RP-15` | ✅ 2026-10-16, 2026-10-20 |
| `FR-RP-034` | `RJ-PRI-DEC-015`, `019` | data RP-2 | `BE-RJP-007` | `FE-RJP-005` | `R-RP-19` (izin `Screen`) | ✅ 2026-10-16, 2026-10-20 |
| `FR-RP-035` | `RJ-PRI-DEC-022`, `029` | state RP-B | `BE-RJP-007` | `FE-RJP-005` | `R-RP-19`, `R-RP-24`, `R-RP-26` | ✅ 2026-10-16, 2026-10-20 |
| `FR-RP-036` | `RJ-PRI-DEC-013` | `INT-RP-08` | `BE-RJP-009` | — | `R-RP-18`, `UAT-RP-10` | ✅ 2026-10-16 |
| `FR-RP-037` | `RJ-PRI-DEC-012`, `022`, `023` | data RP-3 | `BE-RJP-006`, `BE-RJP-007` | `FE-RJP-005` | `R-RP-15` | ✅ 2026-10-16, 2026-10-19, 2026-10-20 |
| `FR-RP-038` | `RJ-PRI-DEC-014`, `026` | `INT-RP-07` | `BE-RJP-010` | `FE-RJP-003`, `FE-RJP-005` | `R-RP-14` | ✅ 2026-10-16, 2026-10-19, 2026-10-20 |
| `FR-RP-040` | `RJ-PRI-DEC-009`, `010`; `RJ-PRI-DEC-030` (1) | `INT-RP-03` | `BE-RJP-008` | `FE-RJP-006` | `R-RP-16`, `UAT-RP-11` | ✅ 2026-10-16, 2026-10-19 |
| `FR-RP-041` | `RJ-PRI-DEC-007`, `024` | state RP-D | `BE-RJP-002`, `BE-RJP-008` | — | `R-RP-20` | ✅ 2026-10-16 |
| `FR-RP-042` | `RJ-PRI-DEC-016`, `020` | `VAL-RP-15` s.d. `20` | `BE-RJP-008` | `FE-RJP-006` | `R-RP-17`, `UAT-RP-12` | ✅ 2026-10-16, 2026-10-19 |
| `FR-RP-043` | `RJ-PRI-DEC-027` | `INT-RP-04` | `BE-RJP-008` | — | `R-RP-21`, `R-RP-22`, `UAT-RP-13` | ✅ 2026-10-16 |
| `FR-RP-050` | `RJ-PRI-DEC-012` | `INV-RP-05` | `BE-RJP-006` (terbit), `BE-RJP-012` (pakai) | — | `R-RP-23` | ✅ 2026-10-16, 2026-10-19 |
| `FR-RP-051` | `RJ-PRI-DEC-008`, `012` | `api-contract` Arrival | `BE-RJP-012` | `FE-RJP-008` | `R-RP-11` s.d. `13`, `UAT-RP-14` | ✅ 2026-10-16, 2026-10-19 |
| `FR-RP-052` | `RJ-PRI-DEC-028` | state RP-B | `BE-RJP-007` | `FE-RJP-005` | `R-RP-23`, `UAT-RP-15` | ✅ 2026-10-16, 2026-10-20 |
| `FR-RP-053` | `RJ-PRI-DEC-012` | `VAL-RP-09` | `BE-RJP-012` | `FE-RJP-008` | `R-RP-25` | ✅ 2026-10-16, 2026-10-19 |
| `FR-RP-060` | `RJ-PRI-DEC-017`, `018` | `02` RP.4.5 | `BE-RJP-011` | `FE-RJP-007` | `R-RP-29`, `UAT-RP-16`, `17` | ✅ 2026-10-16, 2026-10-19 |
| `FR-RP-061` | `RJ-PRI-DEC-017` | `03` FE-RP-06 | `BE-RJP-011` | `FE-RJP-006`, `FE-RJP-007` | `R-RP-21` | ✅ 2026-10-16, 2026-10-19 |
| `FR-RP-062` | `RJ-PRI-DEC-011` | `api-contract` Issued | `BE-RJP-006` | `FE-RJP-004` | Uji layar `FE-RJP-004` | ✅ 2026-10-19, 2026-10-20 |
| `FR-RP-070` | PDF 2C; `RJ-PRI-FE-004` | `03` FE-RP-04 | — | `FE-RJP-002` | `R-RP-28`, `UAT-RP-18`, `19` | ✅ 2026-10-16 |
| Regresi | — | `02` RP.14 | `BE-RJP-001`, `002`, `008` | `FE-RJP-006` | `R-RP-30` | ✅ 2026-10-16, 2026-10-19 |
| Butir menu terjangkau | `03` RP-FE.2 | — | — | `FE-RJP-004` | Uji layar | ✅ 2026-10-20 |

## 2. Acceptance criteria decision log ke skenario

| AC (decision log) | Skenario |
|---|---|
| 1 | `R-RP-01` |
| 2 | `R-RP-03` |
| 3 | `R-RP-06` |
| 4 | `R-RP-07` |
| 5 | `R-RP-08` |
| 6 | `R-RP-09` |
| 7 | `R-RP-10` |
| 8 | `R-RP-11`, `R-RP-12` |
| 9 | `R-RP-13` |
| 10 | `R-RP-14` |
| 11 | `R-RP-15` |
| 12 | `R-RP-16`, `R-RP-17` |
| 13 | `R-RP-18` |
| 14 | `R-RP-19` |
| 15 | `R-RP-20` |
| 16 | `R-RP-21` |
| 17 | `R-RP-14` |
| 18 | `R-RP-21`, `R-RP-22` |
| 19 | `R-RP-23` |
| 20 | `R-RP-23` |
| 21 | `R-RP-24` |
| 22 | `R-RP-04` |

Seluruh 22 acceptance criteria punya skenario.

## 3. Coverage gap

| ID | Gap | Sebab | Penanganan |
|---|---|---|---|
| `GAP-RP-01` | Go-live link publik (`BE-RJP-012`, `FE-RJP-008`) | `RJ-PRI-OQ-003`: aksesibilitas internet domain RS dan penerusan IP asli belum terbukti | Development dan uji lokal tetap jalan. Go-live tercatat terpisah. 2026-10-21: `PublicArrivalBaseUrl` bawaan repo kini server dev; staging/production disetel lewat environment variable server ([`BE-RJP-005` §8](../task/report/backend/BE-RJP-005.md)). Aksesibilitas internet dan IP asli masih terbuka |
| `GAP-RP-02` | Nomor prioritas benar-benar terbit untuk member | `RJ-PRI-OQ-006`: master tingkat belum diisi `PriorityQueue = true` (Yoga/Marketing) | Data uji di `QuilvianNewDevSukma` diisi lewat layar Membership Tier dengan izin. Go-live menunggu data asli |
| `GAP-RP-03` | Akses Role untuk admisi dan petugas skrining | Operasional, bukan task kode | Daftar periksa go-live: centang `OutpatientPriorityReservation` sesuai `permission-audit-matrix.md` RP-2 |
| `GAP-RP-04` | Tinjauan keamanan endpoint publik | Satu-satunya endpoint bisnis tanpa login | Masuk DoD `BE-RJP-012` sebagai tinjauan manusia sebelum go-live |
| `GAP-RP-05` | WA otomatis, lonceng, peringatan lama tunggu, diskon, laporan | `POST-MVP` (`04` RP-8) | Tidak direncanakan di roadmap ini |
| `GAP-RP-06` | `UAT-RP-03` butuh perangkat display fisik untuk suara | Uji suara di browser terbatas | Uji layar memeriksa payload dan daftar. Suara diperiksa saat UAT perangkat |

## 4. Rujukan lintas roadmap

| Roadmap lain | Task | Hubungan |
|---|---|---|
| `crm-membership/roadmap/backend-roadmap.md` | `BE-CRM-010` ✅ | Prasyarat `BE-RJP-002` |
| `crm-membership/roadmap/backend-roadmap.md` | `BE-CRM-022` | Dipenuhi oleh `BE-RJP-002`. Tidak dikerjakan dua kali |
