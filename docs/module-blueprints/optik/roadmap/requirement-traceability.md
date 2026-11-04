# Traceability Requirement — Optik

| Field | Nilai |
|---|---|
| Revision | `1`, status `approved` (Yoga Aji Pratama, 2026-11-03) |
| Roadmap | `OPT-RM-BE-001` r1, `OPT-RM-FE-001` r1 |
| Blueprint | `OPT-BP-001` r1 `approved` sisi Optik (2026-11-03) |
| SHA | BE `2a5094ca`, FE `b6dd6c304` |
| Kontrak | `OPT-API-v1`, `OPT-STATE-v1`, `OPT-VAL-v1`, `OPT-INT-v1`, `OPT-PERM-v1`, `OPT-PRD-v1` — `approved` sisi Optik |
| Input hash | Lihat [`blueprint-manifest.md`](../blueprint-manifest.md) bagian *Artifact hashes* |

Kolom *Status* hanya berisi keadaan rencana: **Direncanakan**, **⛔ Tertahan**, **Selesai** (dengan
bukti), atau **Di luar roadmap**. Tidak ada task implementasi yang selesai pada revisi ini.

## 1. Requirement → Task → Bukti

| Requirement | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti yang diharapkan | Status |
|---|---|---|---|---|---|---|---|
| `FR-OPT-001` | `OPT-DEC-036` | BE §6 | Registry | `BE-OPT-000` ✅, `002` ✅ | — | Baris registry tercatat 2026-11-03; checker QBE + kontrol negatif lulus (`OPT-EVD-001`). Model nyata `Opt*` lulus QBE Strict ([`BE-OPT-002`](../task/report/backend/BE-OPT-002.md)) | Selesai |
| `FR-OPT-002` | `OPT-DEC-032` | BE §4.1 | — | `BE-OPT-001` ✅ | — | Uji enum lulus ([`BE-OPT-001`](../task/report/backend/BE-OPT-001.md)) | Selesai |
| `FR-OPT-010` | `OPT-DEC-006`, `043` | BE §9 | `INT-OPT-01` | `BE-OPT-010` ✅ | `FE-OPT-002`, `010` ⛔ | `UAT-01` | BE selesai; FE direncanakan |
| `FR-OPT-011` | `OPT-DEC-037`, `039` | BE §4.1 | `INT-OPT-10` | `BE-OPT-003` ✅ ([laporan](../task/report/backend/BE-OPT-003.md)) | `FE-OPT-010` ⛔ | `OPT-AC-023`, `UAT-02` — uji harness lulus; migration belum diterapkan | BE selesai (harness); FE direncanakan |
| `FR-OPT-020` | `OPT-DEC-005`, `012` | BE §4.2 | `OPT-VAL-001`..`003` | `BE-OPT-004` ✅ (pembaca izin WIB, [laporan](../task/report/backend/BE-OPT-004.md)), `011` ✅ ([laporan](../task/report/backend/BE-OPT-011.md); SIP + STR `OPT-DEC-056`, profesi `058`) | `FE-OPT-011` ⛔ | `OPT-AC-001`, `002`, `UAT-04` | BE selesai; FE direncanakan |
| `FR-OPT-021` | `OPT-DEC-042`, `047` | BE §9 | `INT-OPT-04` | `BE-OPT-031` ✅ ([laporan](../task/report/backend/BE-OPT-031.md)) | `FE-OPT-011` ⛔ | `OPT-AC-024` — final + 2 revisi = satu baris tarif, terbukti di harness dengan rantai Billing; `UAT-03` | BE selesai (harness); FE direncanakan |
| `FR-OPT-030` | `OPT-DEC-045` | BE §4.2 | `OPT-VAL-004`, `005`, `010`..`015` | `BE-OPT-012` ✅ ([laporan](../task/report/backend/BE-OPT-012.md)) | `FE-OPT-001` ✅, `012` ⛔ | `OPT-AC-028` | BE selesai; FE direncanakan |
| `FR-OPT-031` | `OPT-DEC-045` | BE §4.2 | `OPT-VAL-016` | `BE-OPT-012` ✅ (rumus transposisi sama dengan FE) | `FE-OPT-001` ✅, `012` ⛔ | `OPT-AC-028`, `UAT-05`. Pratinjau transposisi layar lulus uji unit ([`FE-OPT-001`](../task/report/frontend/FE-OPT-001.md)) | BE selesai; FE direncanakan |
| `FR-OPT-032` | `OPT-DEC-007`, `023` | ERD klinis | `OPT-VAL-022`, `023` | `BE-OPT-012` ✅ (tolak revisi terkunci, `CheckUsableForOrderAsync`), `034` ✅ (pengisian kunci, [laporan](../task/report/backend/BE-OPT-034.md)) | `FE-OPT-012` ⛔ | `OPT-AC-004` terbukti (revisi resep pesanan dalam pengerjaan `409 OPT-VAL-022`), `012`, `UAT-06` | BE selesai (harness); FE direncanakan |
| `FR-OPT-040` | `OPT-DEC-008`, `009`, `028` | BE §9 | `OPT-VAL-046` | `BE-OPT-032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)), `034` ✅ ([laporan](../task/report/backend/BE-OPT-034.md)) | `FE-OPT-020` ⛔ | `OPT-AC-005`, `006`, `UAT-07`, `08` terbukti di harness | BE selesai (harness); FE direncanakan |
| `FR-OPT-041` | `OPT-DEC-044` | BE §9 | `INT-OPT-06` | `BE-OPT-032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)) | `FE-OPT-020` ⛔ | `OPT-AC-027` — rumus bagian pasien sesudah tanggungan terbukti (uji unit); belum dengan penjamin sungguhan di harness | BE selesai (harness); FE direncanakan |
| `FR-OPT-042` | `OPT-DEC-019`, `060` | BE §9 | `INT-OPT-07` | `BE-OPT-033` ✅ ([laporan](../task/report/backend/BE-OPT-033.md)), `034` ✅ ([laporan](../task/report/backend/BE-OPT-034.md)) | — | `OPT-AC-008` — tekan dua kali = stok terpotong sekali dan satu riwayat; stok kurang membatalkan seluruh transisi | Selesai (harness); serentak belum diuji |
| `FR-OPT-043` | `OPT-DEC-035` | BE §9 | `OPT-VAL-047` | `BE-OPT-032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)), `034` ✅ ([laporan](../task/report/backend/BE-OPT-034.md)) | `FE-OPT-020` ⛔ | `OPT-AC-019`, `UAT-09` terbukti di harness (penolakan menyebut nomor pesanan pertama) | BE selesai (harness); FE direncanakan |
| `FR-OPT-050` | `OPT-DEC-020`, `046` | State §3 | `OPT-VAL-042` | `BE-OPT-042` ✅ ([laporan](../task/report/backend/BE-OPT-042.md)) | `FE-OPT-022` ⛔ | `OPT-AC-010`, `029`, `UAT-10`, `11` terbukti di harness | BE selesai (harness); FE direncanakan |
| `FR-OPT-060` | `OPT-DEC-048` | State §3 | `OPT-VAL-050`..`052` | `BE-OPT-040` ✅ ([laporan](../task/report/backend/BE-OPT-040.md)) | `FE-OPT-021` ⛔ | `OPT-AC-009`, `UAT-12`, `13` terbukti di harness | BE selesai (harness); FE direncanakan |
| `FR-OPT-061` | `OPT-DEC-010`, `038` | State §3 | `OPT-VAL-053`..`055` | `BE-OPT-041` ✅ ([laporan](../task/report/backend/BE-OPT-041.md)) | `FE-OPT-021` ⛔ | `OPT-AC-007`, `017`, `021`, `UAT-14`, `15` terbukti di harness (kredit baris menunggu persetujuan Billing) | BE selesai (harness); FE direncanakan |
| `FR-OPT-062` | `OPT-DEC-018`, `049` | State §3 | `OPT-VAL-056` | `BE-OPT-041` ✅ ([laporan](../task/report/backend/BE-OPT-041.md)) | `FE-OPT-021` ⛔ | `OPT-DEC-049`: tutup sesudah 90 hari, tetap ditagih penuh tanpa bongkar stok, serah susulan menutup tagihan dan kunjungan — terbukti di harness | BE selesai (harness); FE direncanakan |
| `FR-OPT-070` | `OPT-DEC-015`, `041`, `043` | State §4 | `OPT-VAL-060`..`063` | `BE-OPT-050` ✅ ([laporan](../task/report/backend/BE-OPT-050.md)) | `FE-OPT-030` ⛔ | `OPT-AC-022`, `026`, `UAT-17` terbukti di harness | BE selesai (harness); FE direncanakan |
| `FR-OPT-071` | `OPT-DEC-015` | State §4 | `OPT-VAL-064` | `BE-OPT-050` ✅ ([laporan](../task/report/backend/BE-OPT-050.md)) | `FE-OPT-030` ⛔ | `OPT-AC-013`, `UAT-16` terbukti di harness | BE selesai (harness); FE direncanakan |
| `FR-OPT-080` | `OPT-DEC-029` | BE §9 | `INT-OPT-04` | `BE-OPT-030` ✅ ([laporan](../task/report/backend/BE-OPT-030.md)), `032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)) | — | `OPT-AC-018` — dua pesanan berframe sama = dua baris tagihan terpisah, terbukti di harness | Selesai (harness) |
| `FR-OPT-081` | `OPT-DEC-043` | BE §9 | `INT-OPT-02` | `BE-OPT-030` ✅ ([laporan](../task/report/backend/BE-OPT-030.md)), `040` ✅ ([laporan](../task/report/backend/BE-OPT-040.md)) | — | `OPT-AC-025`, `UAT-18`, `UAT-12` — tertahan selama pesanan berjalan; tertutup saat pesanan terakhir diserahkan dan lunas, terbukti di harness | BE selesai (harness) |
| `FR-OPT-082` | `OPT-DEC-027`, `040` | BE §9 | `INT-OPT-11` | — (endpoint Billing yang ada) | `FE-OPT-003`, `020` ⛔ | `OPT-AC-016`, `011`, `UAT-19` | ⛔ Tertahan (`OPT-APR-004`, `POST-MVP`, untuk persetujuan di atas batas; diskon ≤ 10% direncanakan) |
| `FR-OPT-090` | `OPT-DEC-024` | BE §4.2 | `INT-OPT-09` | `BE-OPT-013` ✅ ([laporan](../task/report/backend/BE-OPT-013.md)) | — | `OPT-AC-014`, `UAT-20` | BE selesai (harness); berkas RM menyala belum diuji |
| `FR-OPT-091` | `OPT-DEC-042`, `055` | BE §9 | `INT-OPT-08` | `BE-OPT-051` ✅ ([laporan](../task/report/backend/BE-OPT-051.md)) | — | Uji adapter Medical Fee: pemeriksaan final tampil berpelaksana dan cocok dengan baris tagihan `OPTICAL` | BE selesai (harness) |
| `FR-OPT-100` | `OPT-DEC-025` | BE §4.2 | `OPT-API-v1` | `BE-OPT-052` ✅ ([laporan](../task/report/backend/BE-OPT-052.md)) | `FE-OPT-032` ⛔ | `UAT-22`, `23` terbukti di harness; penjualan sama dengan baris tagihan `OPTICAL` | BE selesai (harness); FE direncanakan |
| `FR-OPT-110` | `OPT-DEC-031` | BE §8 | — | `BE-OPT-005` ✅ (+ data `MstDrug.IsPrescribable`) | `FE-OPT-031` ⛔ | `OPT-AC-020`, `UAT-26` | Direncanakan |
| `FR-OPT-111` | `OPT-DEC-009`, `040` | BE §4.1 | `OPT-VAL-070`..`073` | `BE-OPT-005` ✅ | `FE-OPT-031` ⛔ | `OPT-AC-006`, `UAT-24`, `25` | Direncanakan |
| `NFR-001`..`007` | — | PRD §17 | Seluruh kontrak | `BE-OPT-053` ✅ ([laporan](../task/report/backend/BE-OPT-053.md)) | `FE-OPT-002`, `003` | Laporan uji menyeluruh: matriks izin, E2E penuh, privasi log; atomisitas/konkurensi/idempotensi/fail-closed terbukti per task | BE selesai (harness); FE direncanakan |

## 2. Coverage gap

| Gap | Sebab | Arah |
|---|---|---|
| `UAT-21` (izin Rekam Medis) | Penjaganya milik Rekam Medis, bukan task Optik | Dibuktikan saat `BE-OPT-013` bersama pemilik Rekam Medis |
| `OPT-AC-011` bagian persetujuan diskon di atas batas | Billing belum punya persetujuan per `ApproverRole` | `POST-MVP` (`OPT-APR-004`); di MVP hanya dibuktikan bahwa diskon tertahan |
| Rincian kolom visus/autoref | Ditutup 2026-11-03: disahkan `OPT-DEC-057`, notasi `OPT-DEC-059` | — |
| Rentang BC/DIA | `PROPOSED` | Konfirmasi RO/Sp.M sebelum `BE-OPT-012` diuji |
| Profesi RO dan jenis izin di HR (`OPT-CAP-07`) | Data belum diperiksa | Kueri baca-saja saat `BE-OPT-004` |
| Seluruh UAT layar | Screen brief belum ada | `FE-OPT-000` |

## 3. Persetujuan yang menahan task (status 2026-11-03, `OPT-EVD-002`)

| Persetujuan | Pemilik | Task yang tertahan |
|---|---|---|
| ~~Registry `Opt`~~ ✅ 2026-11-03 | Pemilik registry | — (penghalang dicabut) |
| ✅ `OPT-APR-001`, `002`, `011`, `012` | Yasmina (Billing) | — |
| `OPT-APR-003`, `004` | Billing | `POST-MVP`; kebijakan diskon Optik (data) |
| ✅ `OPT-APR-005` | Ikbal (Farmasi), Muhammad Hamzah (Master Data) | Bagian stok kembali: `OPT-DEC-060` (Stok Masuk batch sama), konfirmasi Ikbal belum tercatat |
| ✅ `OPT-APR-006` | Muhammad Hamzah | — |
| ✅ `OPT-APR-007` | Yoga Aji Pratama | — |
| ✅ `OPT-APR-008`, `010` | Muhammad Hamzah | — |
| ✅ `OPT-APR-009` | Yasmin | — |
| Screen brief | Yoga Aji Pratama | `FE-OPT-010`..`032` |
