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
| KSK-RM-001 Menu Cek No. RM | `03` §2 | — | — | `FE-KSK-004` | `FR-KSK-010` | Belum |
| KSK-RM-002 Hanya KTP/HP | `KSK-DSN-001/004` | api | `BE-KSK-001` | `FE-KSK-004` | AC-RM-001/003 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-RM-003 Validasi KTP | `KSK-DEC-019` | validation §1 | `BE-KSK-001` | `FE-KSK-003/004` | UAT 5–7, `KSK-AC-017` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-RM-004 Validasi & normalisasi HP | `KSK-DEC-018`, `KSK-DSN-002` | validation §1 | `BE-KSK-001` | `FE-KSK-003` | `KSK-AC-016`, UAT 8 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-RM-005 Pencarian | `KSK-DSN-003` | api, state §1 | `BE-KSK-001` | `FE-KSK-004` | UAT 1–4 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-RM-006 Ditemukan + CTA | `KSK-DEC-012` | validation §2 | `BE-KSK-001` | `FE-KSK-004`, `FE-KSK-007` | AC-RM-004, `KSK-AC-007` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-RM-007 Kartu Pasien minimal | `KSK-FACT-005` | api (`KioskPatientCardResponse`) | `BE-KSK-001` | `FE-KSK-004` | SEC-KSK-004 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-RM-008 Belum terdaftar | — | validation §2 | `BE-KSK-001` | `FE-KSK-004` | AC-RM-005 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-RM-009 Keputusan dari backend | `KSK-INV-001` | state §1 | `BE-KSK-001` | `FE-KSK-004` | UAT 9–10 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| DEC-KSK-001 HP dipakai banyak pasien | `KSK-DEC-006` | api, validation §2 | `BE-KSK-001` | `FE-KSK-004` | `KSK-AC-001` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KTP cocok ganda | `KSK-DEC-007` | api | `BE-KSK-001` | `FE-KSK-004` | `KSK-AC-002` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| Status pasien non-aktif / merged | `KSK-DEC-017`, `KSK-DSN-008` | api | `BE-KSK-001` | `FE-KSK-004` | `KSK-AC-014/015` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-OLD-001..009 Urutan 8 step | `KSK-DEC-002/014` | state §2 | — | `FE-KSK-007` | UAT 12–21 | Belum (⛔ `KSK-OQ-004`) |
| KSK-OLD-010 Tanpa lompat step | `KSK-INV-004` | state §2 | — | `FE-KSK-007` | UAT 15 | Belum (⛔) |
| Satu sesi kiosk per perjalanan | `KSK-DEC-014`, `KSK-DSN-007` | integration §1–2 | — | `FE-KSK-007` | `KSK-AC-010/011` | Belum (⛔) |
| Pengenalan kartu asuransi/member di Step 1 | `KSK-DSN-004` | api `searchType 3/4` | `BE-KSK-001` | `FE-KSK-007` | acceptance §2 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-GUA-001 Pilih penjamin utama | `KSK-DEC-008` | validation §3 | — | `FE-KSK-008` | UAT 22 | Belum (⛔) |
| KSK-GUA-002 Satu penanggung per kunjungan | `KSK-DEC-008/013` | api §Patient Encounter | `BE-KSK-003` | `FE-KSK-008` | UAT 23–24, `KSK-AC-003/009` | Belum (⛔ `KSK-OQ-005`) |
| DEC-KSK-004 Tidak ganti setelah Konfirmasi | `KSK-DEC-009` | state §2 | — | `FE-KSK-008` | acceptance §3 | Belum (⛔) |
| KSK-SCH-001 Dropdown jadwal dokter | `KSK-UI-005` | — | — | `FE-KSK-001` | UAT 25–26 | Belum |
| KSK-UX-001 Loading & kirim ganda | — | validation `KSK-VAL-008` | — | `FE-KSK-004` | UAT 11 | Belum |
| KSK-UX-002/003 Error ≠ belum terdaftar | `KSK-INV-002`, `KSK-DSN-003` | api kode status | `BE-KSK-001` | `FE-KSK-003/004` | UAT 9–10 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| SEC-KSK-001/002/003 Validasi, sanitasi, query aman | — | validation §1 | `BE-KSK-001` | `FE-KSK-003` | acceptance §1 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| SEC-KSK-004 Data minimal | — | api | `BE-KSK-001` | — | acceptance §1 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| SEC-KSK-005 Log tanpa KTP/HP | `KSK-GAP-001` | permission-audit §4 | `BE-KSK-001` | `FE-KSK-006` (console) | `KSK-AC-008`, `KSK-AC-013` | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| SEC-KSK-006 Rate limit | `KSK-DEC-011`, `KSK-DSN-005` | api `429` | `BE-KSK-002` | `FE-KSK-004` | `KSK-AC-004` | BE ✅ — [BE-KSK-002](../task/report/backend/BE-KSK-002.md); FE belum |
| SEC-KSK-007 Session cleanup | `KSK-DEC-010` | state §1–2 | — | `FE-KSK-005` | UAT 28, `KSK-AC-005` | Belum |
| PRIV-2/3 Tanpa storage & URL | `KSK-DEC-012/016` | permission-audit §5 | `BE-KSK-001` | `FE-KSK-004`, `FE-KSK-006` | `KSK-AC-013`, PRIV-3 | Belum |
| KSK-BASE-001 Nama > 3 kata | — | — | `BE-KSK-001` (`fullName` utuh) | `FE-KSK-002` | UAT 27 | BE ✅ — [BE-KSK-001](../task/report/backend/BE-KSK-001.md); FE belum |
| KSK-BASE-002 Bulan pendek | `KSK-DEC-015` | — | — | `FE-KSK-002` | `KSK-AC-012` | Belum |

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
