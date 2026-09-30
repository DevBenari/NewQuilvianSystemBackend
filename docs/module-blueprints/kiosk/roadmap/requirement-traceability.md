# Requirement Traceability — Modul Kiosk

| Field | Nilai |
| --- | --- |
| Traceability ID | `KSK-TRACE-001` |
| Revision | `1` |
| Status | `approved` — Sukma Giri Pratama, 2026-09-30 |
| Blueprint ID | `KSK-BP-001` r1, status `approved` (Sukma Giri Pratama, 2026-09-30) |
| Roadmap | `KSK-RM-BE-001` r1, `KSK-RM-FE-001` r1 |
| SHA baseline | BE `419b910f`, FE `4ec51b0b` |
| Kontrak | `KSK-CONTRACT-v1` — `approved` |
| Hash masukan | `blueprint-manifest.md#artifact_hashes` |

Aturan baca: requirement tanpa task berarti tidak akan terwujud; task tanpa requirement lahir dari selera; task tanpa bukti belum selesai. Kolom **Status** diperbarui oleh build skill saat laporan task ditulis.

## 1. Requirement PRD → desain → task → bukti

| Requirement | Keputusan / desain | Kontrak | Task BE | Task FE | Bukti | Status |
| --- | --- | --- | --- | --- | --- | --- |
| KSK-RM-001 Menu Cek No. RM | `03` §2 | — | — | `FE-KSK-004` | `FR-KSK-010` | FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-RM-002 Hanya KTP/HP | `KSK-DSN-001/004` | api | `BE-KSK-001` | `FE-KSK-004` | AC-RM-001/003 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-RM-003 Validasi KTP | `KSK-DEC-019` | validation §1 | `BE-KSK-001` | `FE-KSK-003/004` | UAT 5–7, `KSK-AC-017` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-003](../task/report/frontend/FE-KSK-003.md), [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-RM-004 Validasi & normalisasi HP | `KSK-DEC-018`, `KSK-DSN-002` | validation §1 | `BE-KSK-001` | `FE-KSK-003` | `KSK-AC-016`, UAT 8 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-003](../task/report/frontend/FE-KSK-003.md) (30 Sep 2026; tabel normalisasi §1 identik) |
| KSK-RM-005 Pencarian | `KSK-DSN-003` | api, state §1 | `BE-KSK-001` | `FE-KSK-004` | UAT 1–4 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-RM-006 Ditemukan + CTA | `KSK-DEC-012` | validation §2 | `BE-KSK-001` | `FE-KSK-004`, `FE-KSK-007` | AC-RM-004, `KSK-AC-007` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md), [FE-KSK-007](../task/report/frontend/FE-KSK-007.md) (30 Sep – 1 Okt 2026; handoff dibaca Pasien Lama) |
| KSK-RM-007 Kartu Pasien minimal | `KSK-FACT-005` | api (`KioskPatientCardResponse`) | `BE-KSK-001` | `FE-KSK-004` | SEC-KSK-004 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-RM-008 Belum terdaftar | — | validation §2 | `BE-KSK-001` | `FE-KSK-004` | AC-RM-005 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-RM-009 Keputusan dari backend | `KSK-INV-001` | state §1 | `BE-KSK-001` | `FE-KSK-004` | UAT 9–10 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| DEC-KSK-001 HP dipakai banyak pasien | `KSK-DEC-006` | api, validation §2 | `BE-KSK-001` | `FE-KSK-004` | `KSK-AC-001` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KTP cocok ganda | `KSK-DEC-007` | api | `BE-KSK-001` | `FE-KSK-004` | `KSK-AC-002` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| Status pasien non-aktif / merged | `KSK-DEC-017`, `KSK-DSN-008` | api | `BE-KSK-001` | `FE-KSK-004` | `KSK-AC-014/015` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-OLD-001..009 Urutan 8 step | `KSK-DEC-002/014` | state §2 | — | `FE-KSK-007` | UAT 12–21 | FE ✅ — [FE-KSK-007](../task/report/frontend/FE-KSK-007.md) (30 Sep – 1 Okt 2026) |
| KSK-OLD-010 Tanpa lompat step | `KSK-INV-004` | state §2 | — | `FE-KSK-007` | UAT 15 | FE ✅ — [FE-KSK-007](../task/report/frontend/FE-KSK-007.md) (30 Sep – 1 Okt 2026) |
| Satu sesi kiosk per perjalanan | `KSK-DEC-014`, `KSK-DSN-007` | integration §1–2 | — | `FE-KSK-007` | `KSK-AC-010/011` | FE ✅ — [FE-KSK-007](../task/report/frontend/FE-KSK-007.md) (30 Sep – 1 Okt 2026) |
| Pengenalan kartu asuransi/member di Step 1 | `KSK-DSN-004` | api `searchType 3/4` | `BE-KSK-001` | `FE-KSK-007` | acceptance §2 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-007](../task/report/frontend/FE-KSK-007.md) (lookup `searchType 3/4`) |
| KSK-GUA-001 Pilih penjamin utama | `KSK-DEC-008` | validation §3 | — | `FE-KSK-008` | UAT 22 | FE ✅ — [FE-KSK-008](../task/report/frontend/FE-KSK-008.md) (30 Sep – 1 Okt 2026) |
| KSK-GUA-002 Satu penanggung per kunjungan | `KSK-DEC-008/013` | api §Patient Encounter | `BE-KSK-003` | `FE-KSK-008` | UAT 23–24, `KSK-AC-003/009` | BE ✅ `BE-KSK-003` 30 Sep 2026 — runtime AC 1–5 PASS ([laporan](../task/report/backend/BE-KSK-003.md)); FE ✅ — [FE-KSK-008](../task/report/frontend/FE-KSK-008.md) (Perusahaan `paymentType 3` / Asuransi `2` tersimpan) |
| DEC-KSK-004 Tidak ganti setelah Konfirmasi | `KSK-DEC-009` | state §2 | — | `FE-KSK-008` | acceptance §3 | FE ✅ — [FE-KSK-008](../task/report/frontend/FE-KSK-008.md) (30 Sep – 1 Okt 2026) |
| KSK-SCH-001 Dropdown jadwal dokter | `KSK-UI-005` | — | — | `FE-KSK-001` | UAT 25–26 | FE ✅ — [FE-KSK-001](../task/report/frontend/FE-KSK-001.md) (30 Sep 2026; sebelum/sesudah 2 resolusi) |
| KSK-UX-001 Loading & kirim ganda | — | validation `KSK-VAL-008` | — | `FE-KSK-004` | UAT 11 | FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| KSK-UX-002/003 Error ≠ belum terdaftar | `KSK-INV-002`, `KSK-DSN-003` | api kode status | `BE-KSK-001` | `FE-KSK-003/004` | UAT 9–10 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-003](../task/report/frontend/FE-KSK-003.md), [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| SEC-KSK-001/002/003 Validasi, sanitasi, query aman | — | validation §1 | `BE-KSK-001` | `FE-KSK-003` | acceptance §1 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-003](../task/report/frontend/FE-KSK-003.md) (30 Sep 2026) |
| SEC-KSK-004 Data minimal | — | api | `BE-KSK-001` | — | acceptance §1 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| SEC-KSK-005 Log tanpa KTP/HP | `KSK-GAP-001` | permission-audit §4 | `BE-KSK-001` | `FE-KSK-006` (console) | `KSK-AC-008`, `KSK-AC-013` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-006](../task/report/frontend/FE-KSK-006.md) (30 Sep 2026; Console alur Pasien Lama bersih) |
| SEC-KSK-006 Rate limit | `KSK-DEC-011`, `KSK-DSN-005` | api `429` | `BE-KSK-002` | `FE-KSK-004` | `KSK-AC-004` | BE ✅ — [BE-KSK-002](../task/report/backend/BE-KSK-002.md); FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md) (30 Sep 2026) |
| SEC-KSK-007 Session cleanup | `KSK-DEC-010` | state §1–2 | — | `FE-KSK-005` | UAT 28, `KSK-AC-005` | FE ✅ — [FE-KSK-005](../task/report/frontend/FE-KSK-005.md) (30 Sep 2026; uji browser skala 20 detik) |
| PRIV-2/3 Tanpa storage & URL | `KSK-DEC-012/016` | permission-audit §5 | `BE-KSK-001` | `FE-KSK-004`, `FE-KSK-006` | `KSK-AC-013`, PRIV-3 | FE ✅ — [FE-KSK-004](../task/report/frontend/FE-KSK-004.md), [FE-KSK-006](../task/report/frontend/FE-KSK-006.md) (30 Sep 2026; KTP/HP tidak di URL/Redux/console di Cek No. RM dan Step 1) |
| KSK-BASE-001 Nama > 3 kata | — | — | `BE-KSK-001` (`fullName` utuh) | `FE-KSK-002` | UAT 27 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE ✅ — [FE-KSK-002](../task/report/frontend/FE-KSK-002.md) (30 Sep 2026; nama lima kata utuh di Review/Konfirmasi/Tiket) |
| KSK-BASE-002 Bulan pendek | `KSK-DEC-015` | — | — | `FE-KSK-002` | `KSK-AC-012` | FE ✅ — [FE-KSK-002](../task/report/frontend/FE-KSK-002.md) (30 Sep 2026; '12 Sep 1990') |

## 2. Keputusan → task

| Decision | Task |
| --- | --- |
| `KSK-DEC-001..005` (proses, scope, bentuk, approver) | Seluruh roadmap |
| `KSK-DEC-006`, `007`, `017`, `018`, `019` | `BE-KSK-001`, `FE-KSK-003/004` |
| `KSK-DEC-008`, `009` | `FE-KSK-008` |
| `KSK-DEC-010` | `FE-KSK-005` |
| `KSK-DEC-011` | `BE-KSK-002` |
| `KSK-DEC-012` | `FE-KSK-004`, `FE-KSK-007` |
| `KSK-DEC-013` | `BE-KSK-003`, `FE-KSK-008` |
| `KSK-DEC-002`, `014` | `FE-KSK-007` |
| `KSK-DEC-015` | `FE-KSK-002` |
| `KSK-DEC-016` | `FE-KSK-006` |
| `KSK-DSN-001..006`, `008` | `BE-KSK-001/002` |
| `KSK-DSN-007`, `009` | `FE-KSK-003`, `FE-KSK-007` |

## 3. Coverage gap

| Hal | Sebab | Penanganan |
| --- | --- | --- |
| `NFR-002` p95 < 500 ms di produksi | DB dev hanya 16 pasien | Diukur pemilik pasca-rilis; bila gagal, ajukan index ekspresi ke PatientManagement |
| Uji perangkat Kiosk fisik | Tidak tersedia di dev | UAT oleh pemilik |
| `KSK-RISK-001` sesi ganda saat retry setelah timeout `scan-result` | Perilaku existing | Dicatat; tidak ditutup revisi ini |
| `KSK-RISK-003` `PATCH …/primary` masih bisa dipanggil akun Kiosk | `KSK-GAP-011` | POST-MVP |
