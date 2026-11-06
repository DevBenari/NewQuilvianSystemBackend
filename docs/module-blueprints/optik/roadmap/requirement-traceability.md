# Traceability Requirement — Optik

| Field | Nilai |
|---|---|
| Revision | `5` — `DRAFT`, baris `BE-OPT-063` dan izin lintas modul (temuan audit `T1`, `T2`); sebelumnya `4` — `DRAFT`, baris amendment r10 (`OPT-DEC-081`..`083`); sebelumnya `3` — `DRAFT`, baris amendment r9 (`OPT-DEC-077`..`080`); penahan r2 dicabut 2026-11-06, sedangkan seluruh task amendment r9 dan r10 selesai 2026-11-06, dan persetujuan mitra tercatat (`OPT-EVD-003`); sebelumnya `2` — baris `BE-OPT-054` dan screen brief disetujui (2026-11-04); r1 `approved` (Yoga Aji Pratama, 2026-11-03) |
| Roadmap | `OPT-RM-BE-001` r6, `OPT-RM-FE-001` r3 (keduanya `DRAFT` untuk task baru) |
| Blueprint | `OPT-BP-001` r1 `approved` sisi Optik (2026-11-03); r2, r3, r4, dan r5 `approved` sisi Optik (2026-11-06) |
| SHA | BE `2a5094ca`, FE `b6dd6c304`. r3: BE `06e40fb7`, FE `811f22159` |
| Kontrak | `OPT-API-v1`, `OPT-STATE-v1`, `OPT-VAL-v1`, `OPT-INT-v1`, `OPT-PERM-v1`, `OPT-PRD-v1` — `approved` sisi Optik. Baris amendment: `OPT-API-v1.4`, `OPT-VAL-v1.3`, `OPT-INT-v1.3`, `OPT-STATE-v1.1`, `OPT-PERM-v1.4`, `OPT-PRD-v1.4` — `approved` sisi Optik 2026-11-06 |
| Input hash | Lihat [`blueprint-manifest.md`](../blueprint-manifest.md) bagian *Artifact hashes*; hash r5 dikunci 2026-11-06 |

Kolom *Status* hanya berisi keadaan rencana: **Direncanakan**, **⛔ Tertahan**, **Selesai** (dengan
bukti), atau **Di luar roadmap**. Seluruh task amendment r9 dan r10 selesai 2026-11-06. Selesai di sini berarti kode dan uji per task, bukan kesiapan modul; lihat [`testing/readiness-report.md`](../testing/readiness-report.md).

## 1. Requirement → Task → Bukti

| Requirement | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti yang diharapkan | Status |
|---|---|---|---|---|---|---|---|
| `FR-OPT-001` | `OPT-DEC-036` | BE §6 | Registry | `BE-OPT-000` ✅, `002` ✅ | — | Baris registry tercatat 2026-11-03; checker QBE + kontrol negatif lulus (`OPT-EVD-001`). Model nyata `Opt*` lulus QBE Strict ([`BE-OPT-002`](../task/report/backend/BE-OPT-002.md)) | Selesai |
| `FR-OPT-002` | `OPT-DEC-032` | BE §4.1 | — | `BE-OPT-001` ✅ | — | Uji enum lulus ([`BE-OPT-001`](../task/report/backend/BE-OPT-001.md)) | Selesai |
| `FR-OPT-010` | `OPT-DEC-006`, `043` | BE §9 | `INT-OPT-01` | `BE-OPT-010` ✅ | `FE-OPT-002` ✅ ([laporan](../task/report/frontend/FE-OPT-002.md)), `010` ✅ ([laporan](../task/report/frontend/FE-OPT-010.md)) | `UAT-01` — lulus E2E API tiruan dan backend lokal (`ENC-RSMMC-00186`) | BE dan FE selesai |
| `FR-OPT-011` | `OPT-DEC-037`, `039` | BE §4.1 | `INT-OPT-10` | `BE-OPT-003` ✅ ([laporan](../task/report/backend/BE-OPT-003.md)) | `FE-OPT-010` ✅ ([laporan](../task/report/frontend/FE-OPT-010.md)) | `OPT-AC-023`, `UAT-02` — uji harness lulus; layar lulus E2E API tiruan; tautan rujukan tersimpan dan tampil dengan backend lokal | BE dan FE selesai |
| `FR-OPT-012` | `OPT-DEC-030`, `078` | BE §4.2 | `INT-OPT-01`, `OPT-VAL-v1.1` §8 | `BE-OPT-061` ✅ ([laporan](../task/report/backend/BE-OPT-061.md)) | `FE-OPT-042` ✅ ([laporan](../task/report/frontend/FE-OPT-042.md)) | `OPT-AC-036`, `UAT-30`, `31` terbukti di harness dan E2E API tiruan | BE selesai (harness); FE selesai; alur asli belum diuji; `OPT-APR-013` ✅ |
| `FR-OPT-020` | `OPT-DEC-005`, `012` | BE §4.2 | `OPT-VAL-001`..`003` | `BE-OPT-004` ✅ (pembaca izin WIB, [laporan](../task/report/backend/BE-OPT-004.md)), `011` ✅ ([laporan](../task/report/backend/BE-OPT-011.md); SIP + STR `OPT-DEC-056`, profesi `058`) | `FE-OPT-011` ✅ ([laporan](../task/report/frontend/FE-OPT-011.md)) | `OPT-AC-001` lulus dengan backend lokal; `UAT-04` lulus E2E API tiruan; `OPT-AC-002` terbukti di harness | BE dan FE selesai; kode profesi konfigurasi belum diisi |
| `FR-OPT-021` | `OPT-DEC-042`, `047` | BE §9 | `INT-OPT-04` | `BE-OPT-031` ✅ ([laporan](../task/report/backend/BE-OPT-031.md)) | `FE-OPT-011` ✅ ([laporan](../task/report/frontend/FE-OPT-011.md)) | `OPT-AC-024` — final + 2 revisi = satu baris tarif, terbukti di harness dengan rantai Billing; `UAT-03` — layar lulus E2E API tiruan | BE selesai (harness); FE selesai; alur asli menunggu kode profesi dan akun RO/Sp.M |
| `FR-OPT-022` | `OPT-DEC-082`, `083` | BE §4.2, §9 | `OPT-INT-v1.2` | `BE-OPT-062` ✅ ([laporan](../task/report/backend/BE-OPT-062.md)) | — | `OPT-AC-039`..`041`, `UAT-36`, `37` terbukti di harness; rantai asli belum (`B1`, `B4`, `B5`) | BE selesai (harness); `OPT-APR-015` ✅ |
| `FR-OPT-030` | `OPT-DEC-045` | BE §4.2 | `OPT-VAL-004`, `005`, `010`..`015` | `BE-OPT-012` ✅ ([laporan](../task/report/backend/BE-OPT-012.md)) | `FE-OPT-001` ✅, `012` ✅ ([laporan](../task/report/frontend/FE-OPT-012.md)) | `OPT-AC-028`; layar lulus E2E API tiruan | BE selesai; FE selesai |
| `FR-OPT-031` | `OPT-DEC-045` | BE §4.2 | `OPT-VAL-016` | `BE-OPT-012` ✅ (rumus transposisi sama dengan FE) | `FE-OPT-001` ✅, `012` ✅ ([laporan](../task/report/frontend/FE-OPT-012.md)) | `OPT-AC-028`, `UAT-05`. Pratinjau transposisi layar lulus uji unit ([`FE-OPT-001`](../task/report/frontend/FE-OPT-001.md)) dan E2E API tiruan | BE selesai; FE selesai |
| `FR-OPT-032` | `OPT-DEC-007`, `023` | ERD klinis | `OPT-VAL-022`, `023` | `BE-OPT-012` ✅ (tolak revisi terkunci, `CheckUsableForOrderAsync`), `034` ✅ (pengisian kunci, [laporan](../task/report/backend/BE-OPT-034.md)) | `FE-OPT-012` ✅ ([laporan](../task/report/frontend/FE-OPT-012.md)) | `OPT-AC-004` terbukti (revisi resep pesanan dalam pengerjaan `409 OPT-VAL-022`), `012`; `UAT-06` — layar lulus E2E API tiruan | BE selesai (harness); FE selesai; alur asli menunggu kode profesi dan akun RO/Sp.M |
| `FR-OPT-033` | `OPT-DEC-077` | BE §4.2, §10 | `OPT-API-v1.2`, `OPT-VAL-v1.1` §7 | `BE-OPT-060` ✅ ([laporan](../task/report/backend/BE-OPT-060.md)) | `FE-OPT-040` ✅ ([laporan](../task/report/frontend/FE-OPT-040.md)) | `OPT-AC-035` | BE selesai (harness); FE selesai (E2E API tiruan); migration diterapkan ke devYoga 2026-11-06; alur dengan backend asli belum diuji (`B1`, `B2`) |
| `FR-OPT-040` | `OPT-DEC-008`, `009`, `028` | BE §9 | `OPT-VAL-046` | `BE-OPT-032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)), `034` ✅ ([laporan](../task/report/backend/BE-OPT-034.md)) | `FE-OPT-020` ✅ ([laporan](../task/report/frontend/FE-OPT-020.md)) | `OPT-AC-005`, `006`, `UAT-07`, `08` terbukti di harness; layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-041` | `OPT-DEC-044`, `078` | BE §9 | `INT-OPT-06` | `BE-OPT-032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)), `061` ✅ ([laporan](../task/report/backend/BE-OPT-061.md)) | `FE-OPT-020` ✅ ([laporan](../task/report/frontend/FE-OPT-020.md)) | `OPT-AC-027` — terbukti di harness dengan penjamin sungguhan lewat Registrasi dan rantai Billing (`BE-OPT-061`); layar menampilkan angka backend apa adanya; data asli menunggu aturan tanggungan dan `B5` | BE selesai (harness); FE selesai |
| `FR-OPT-042` | `OPT-DEC-019`, `060` | BE §9 | `INT-OPT-07` | `BE-OPT-033` ✅ ([laporan](../task/report/backend/BE-OPT-033.md)), `034` ✅ ([laporan](../task/report/backend/BE-OPT-034.md)) | — | `OPT-AC-008` — tekan dua kali = stok terpotong sekali dan satu riwayat; stok kurang membatalkan seluruh transisi | Selesai (harness); serentak belum diuji |
| `FR-OPT-043` | `OPT-DEC-035` | BE §9 | `OPT-VAL-047` | `BE-OPT-032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)), `034` ✅ ([laporan](../task/report/backend/BE-OPT-034.md)) | `FE-OPT-020` ✅ ([laporan](../task/report/frontend/FE-OPT-020.md)) | `OPT-AC-019`, `UAT-09` terbukti di harness (penolakan menyebut nomor pesanan pertama); layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-044` | `OPT-DEC-077` | BE §4.2, §10 | `OPT-API-v1.2`, `OPT-VAL-v1.1` §7 | `BE-OPT-060` ✅ ([laporan](../task/report/backend/BE-OPT-060.md)) | `FE-OPT-040` ✅ ([laporan](../task/report/frontend/FE-OPT-040.md)) | `OPT-AC-035`, `UAT-32` | BE selesai (harness); FE selesai (E2E API tiruan); migration diterapkan ke devYoga 2026-11-06; alur dengan backend asli belum diuji (`B1`, `B2`) |
| `FR-OPT-050` | `OPT-DEC-020`, `046` | State §3 | `OPT-VAL-042` | `BE-OPT-042` ✅ ([laporan](../task/report/backend/BE-OPT-042.md)) | `FE-OPT-022` ✅ ([laporan](../task/report/frontend/FE-OPT-022.md)) | `OPT-AC-010`, `029`, `UAT-10`, `11` terbukti di harness; layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-060` | `OPT-DEC-048` | State §3 | `OPT-VAL-050`..`052` | `BE-OPT-040` ✅ ([laporan](../task/report/backend/BE-OPT-040.md)) | `FE-OPT-021` ✅ ([laporan](../task/report/frontend/FE-OPT-021.md)) | `OPT-AC-009`, `UAT-12`, `13` terbukti di harness; layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-061` | `OPT-DEC-010`, `038` | State §3 | `OPT-VAL-053`..`055` | `BE-OPT-041` ✅ ([laporan](../task/report/backend/BE-OPT-041.md)) | `FE-OPT-021` ✅ ([laporan](../task/report/frontend/FE-OPT-021.md)) | `OPT-AC-007`, `017`, `021`, `UAT-14`, `15` terbukti di harness (kredit baris menunggu persetujuan Billing); layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-062` | `OPT-DEC-018`, `049` | State §3 | `OPT-VAL-056` | `BE-OPT-041` ✅ ([laporan](../task/report/backend/BE-OPT-041.md)) | `FE-OPT-021` ✅ ([laporan](../task/report/frontend/FE-OPT-021.md)) | `OPT-DEC-049`: tutup sesudah 90 hari, tetap ditagih penuh tanpa bongkar stok, serah susulan menutup tagihan dan kunjungan — terbukti di harness; layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-070` | `OPT-DEC-015`, `041`, `043` | State §4 | `OPT-VAL-060`..`063` | `BE-OPT-050` ✅ ([laporan](../task/report/backend/BE-OPT-050.md)) | `FE-OPT-030` ✅ ([laporan](../task/report/frontend/FE-OPT-030.md)) | `OPT-AC-022`, `026`, `UAT-17` terbukti di harness; layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-071` | `OPT-DEC-015` | State §4 | `OPT-VAL-064` | `BE-OPT-050` ✅ ([laporan](../task/report/backend/BE-OPT-050.md)) | `FE-OPT-030` ✅ ([laporan](../task/report/frontend/FE-OPT-030.md)) | `OPT-AC-013`, `UAT-16` terbukti di harness; layar lulus E2E API tiruan; tautan remake ke pesanan asal belum ada (`OptOrderResponse` tanpa `ParentOrderId`) | BE selesai (harness); FE selesai |
| `FR-OPT-072` | `OPT-DEC-077` | BE §4.2, §10 | `OPT-API-v1.2`, `OPT-VAL-v1.1` §7 | `BE-OPT-060` ✅ ([laporan](../task/report/backend/BE-OPT-060.md)) | `FE-OPT-040` ✅ ([laporan](../task/report/frontend/FE-OPT-040.md)) | `OPT-AC-035`, `UAT-33` | BE selesai (harness); FE selesai (E2E API tiruan); migration diterapkan ke devYoga 2026-11-06; alur dengan backend asli belum diuji (`B1`, `B2`) |
| `FR-OPT-080` | `OPT-DEC-029` | BE §9 | `INT-OPT-04` | `BE-OPT-030` ✅ ([laporan](../task/report/backend/BE-OPT-030.md)), `032` ✅ ([laporan](../task/report/backend/BE-OPT-032.md)) | — | `OPT-AC-018` — dua pesanan berframe sama = dua baris tagihan terpisah, terbukti di harness | Selesai (harness) |
| `FR-OPT-081` | `OPT-DEC-043` | BE §9 | `INT-OPT-02` | `BE-OPT-030` ✅ ([laporan](../task/report/backend/BE-OPT-030.md)), `040` ✅ ([laporan](../task/report/backend/BE-OPT-040.md)) | — | `OPT-AC-025`, `UAT-18`, `UAT-12` — tertahan selama pesanan berjalan; tertutup saat pesanan terakhir diserahkan dan lunas, terbukti di harness | BE selesai (harness) |
| `FR-OPT-082` | `OPT-DEC-027`, `040`, `079` | BE §9 | `INT-OPT-11` (`OPT-INT-v1.1`) | `BE-OPT-063` ✅ ([laporan](../task/report/backend/BE-OPT-063.md); uji kontrak pada endpoint Billing yang ada) | `FE-OPT-003` ✅ ([laporan](../task/report/frontend/FE-OPT-003.md); thunk Billing dipakai ulang), `020` ✅ ([laporan](../task/report/frontend/FE-OPT-020.md)), `041` ✅ ([laporan](../task/report/frontend/FE-OPT-041.md)) | Bagian MVP: `OPT-AC-016` (10%), `UAT-29` terbukti di harness pada Billing sungguhan (`BE-OPT-063`); layar lulus E2E API tiruan. Kalimat kedua `OPT-AC-011`, bagian 15% `OPT-AC-016`, dan `UAT-19` dikeluarkan dari MVP (`OPT-DEC-079`), pindah ke slice `OPT-APR-004` | Kode selesai; `OPT-APR-014` ✅, kebijakan datanya belum dibuat; layar Ajukan Diskon berbasis kebijakan selesai |
| `FR-OPT-083` | `OPT-DEC-079` | Brief §5.8 | `INT-OPT-11` (`OPT-INT-v1.1`) | — (penyaringan hanya di layar) | `FE-OPT-041` ✅ ([laporan](../task/report/frontend/FE-OPT-041.md)) | `OPT-AC-037`, `UAT-28` lulus E2E API tiruan | FE selesai; `OPT-APR-014`, `OPT-OQ-020` ✅ |
| `FR-OPT-090` | `OPT-DEC-024` | BE §4.2 | `INT-OPT-09` | `BE-OPT-013` ✅ ([laporan](../task/report/backend/BE-OPT-013.md)) | `FE-OPT-033` ✅ ([laporan](../task/report/frontend/FE-OPT-033.md); layar milik Rekam Medis, tanpa kode Optik) | `OPT-AC-014`, `UAT-20`; layar Rekam Medis menampilkan jenis Optik di E2E API tiruan; metadata backend lokal memuat kedua jenis | BE selesai (harness); berkas RM menyala dengan data asli belum diuji |
| `FR-OPT-091` | `OPT-DEC-042`, `055` | BE §9 | `INT-OPT-08` | `BE-OPT-051` ✅ ([laporan](../task/report/backend/BE-OPT-051.md)) | — | Uji adapter Medical Fee: pemeriksaan final tampil berpelaksana dan cocok dengan baris tagihan `OPTICAL` | BE selesai (harness) |
| `FR-OPT-100` | `OPT-DEC-025` | BE §4.2 | `OPT-API-v1` | `BE-OPT-052` ✅ ([laporan](../task/report/backend/BE-OPT-052.md)) | `FE-OPT-032` ✅ ([laporan](../task/report/frontend/FE-OPT-032.md)) | `UAT-22`, `23` terbukti di harness; penjualan sama dengan baris tagihan `OPTICAL`; layar lulus E2E API tiruan | BE selesai (harness); FE selesai |
| `FR-OPT-110` | `OPT-DEC-031` | BE §8 | — | `BE-OPT-005` ✅ (+ data `MstDrug.IsPrescribable`) | `FE-OPT-031` ✅ ([laporan](../task/report/frontend/FE-OPT-031.md)) | `OPT-AC-020`, `UAT-26` belum dapat diuji: devYoga belum punya barang `IsPrescribable = false`; layar pendaftaran hanya menawarkan barang bertanda itu | FE selesai; data master tertunda |
| `FR-OPT-111` | `OPT-DEC-009`, `040` | BE §4.1 | `OPT-VAL-070`..`073` | `BE-OPT-005` ✅ | `FE-OPT-031` ✅ ([laporan](../task/report/frontend/FE-OPT-031.md)) | `OPT-AC-006`, `UAT-24`, `25` terbukti di harness; layar lulus E2E API tiruan; `UAT-25` juga ditolak backend lokal | BE selesai (harness); FE selesai |
| `FR-OPT-112` | `OPT-DEC-080` | Brief §5.12 | `OPT-API-v1.2` (`Drug : Read`, `OPT-PERM-v1.1`) | `BE-OPT-005` ✅ (`PUT item-profiles/{drugId}`) | `FE-OPT-031` ✅ ([laporan](../task/report/frontend/FE-OPT-031.md)), `042` ✅ ([laporan](../task/report/frontend/FE-OPT-042.md)) | `OPT-AC-038`, `UAT-34`, `35` lulus E2E API tiruan; data asli menunggu `B2` | FE selesai; brief r3 disetujui 2026-11-06 |
| `NFR-008` | `OPT-DEC-082` | BE §9 | `OPT-INT-v1.2` | `BE-OPT-062` ✅ ([laporan](../task/report/backend/BE-OPT-062.md)) | — | `OPT-AC-039`, `040` terbukti di harness | BE selesai (harness) |
| Izin lintas modul | `OPT-DEC-085`, `086` | Brief bagian 8 | `OPT-PERM-v1.4` | — (izin sudah ada) | — (layar sudah memanggilnya) | `OPT-AC-042`, `UAT-38`, `39` belum diuji; perlu akun petugas | Direncanakan untuk uji alur asli; menunggu `B3` |
| `NFR-001`..`007` | — | PRD §17 | Seluruh kontrak | `BE-OPT-053` ✅ ([laporan](../task/report/backend/BE-OPT-053.md)) | `FE-OPT-002` ✅ ([laporan](../task/report/frontend/FE-OPT-002.md)), `003` ✅ ([laporan](../task/report/frontend/FE-OPT-003.md)), `033` ✅ ([laporan](../task/report/frontend/FE-OPT-033.md)) | Laporan uji menyeluruh: matriks izin, E2E penuh, privasi log; atomisitas/konkurensi/idempotensi/fail-closed terbukti per task; layar lulus E2E API tiruan; alur ujung-ke-ujung dengan data asli belum dijalankan | BE selesai (harness); FE selesai |
| Layar `N08`, `N09`, `N11` (penyesuaian brief) | `OPT-DEC-067`, `075`, `076` | Brief §5.8, §5.10 | `OPT-API-v1.1` | `BE-OPT-054` ✅ ([laporan](../task/report/backend/BE-OPT-054.md)) | `FE-OPT-020` ✅ ([laporan](../task/report/frontend/FE-OPT-020.md)), `021` ✅ ([laporan](../task/report/frontend/FE-OPT-021.md)) | `OPT-AC-031`, `034` terbukti di harness; kriteria (1)–(8) `BE-OPT-054` lulus; pratinjau harga, kartu penyaring, dan kolom Tanda lulus E2E API tiruan | BE selesai (harness); FE selesai |

## 2. Coverage gap

| Gap | Sebab | Arah |
|---|---|---|
| `UAT-21` (izin Rekam Medis) | Penjaganya milik Rekam Medis, bukan task Optik | Dibuktikan saat `BE-OPT-013` bersama pemilik Rekam Medis |
| `OPT-AC-011` bagian persetujuan diskon di atas batas | Dikeluarkan dari MVP (`OPT-DEC-079`) | `POST-MVP` (`OPT-APR-004`); di MVP dibuktikan bahwa pilihan di atas 10% tidak ada (`UAT-28`) |
| Kiriman serentak `OPT-AC-035` | `BE-OPT-060` membuktikannya lewat peniruan satu koneksi pada indeks unik sungguhan. Dua koneksi yang benar-benar serentak butuh PostgreSQL | Dibuktikan bersama `B5` |
| ~~`OPT-AC-027` dengan penjamin sungguhan~~ | Ditutup di harness oleh `BE-OPT-061` | Data asli menunggu `B5` |
| ~~`FR-OPT-041` tanpa penjamin sungguhan~~ | Ditutup di harness oleh `BE-OPT-061` | — |
| Aturan tanggungan perusahaan untuk barang Optik | Temuan `BE-OPT-061`: tanpa `MstCompanyGuarantorCoverageRule` yang cocok, tanggungan Rp 0 | Data Master Data, sebelum uji alur asli |
| ~~Risiko `R2` tanpa keputusan~~ | Ditutup oleh `OPT-DEC-082`, `083`, dan `BE-OPT-062` | `OPT-APR-015` ✅ |
| ~~Diskon pada Billing (`T2`)~~ | Ditutup di harness oleh `BE-OPT-063` | Data asli menunggu `OPT-DISC-10` dan `B5` |
| Kunjungan tertutup tanpa tagihan | Temuan `BE-OPT-062`: Billing membuat tagihan `OPEN` baru pada kunjungan `Completed` | `OPT-OQ-023`, disarankan dikonfirmasi Yasmina sebelum rilis |
| Tarif berlaku sesudah tanggal pemeriksaan | Billing mencari tarif menurut waktu final, sehingga kasus ini masuk rekonsiliasi | Sebagian terbukti di `BE-OPT-062` kriteria (9); isi antrean rekonsiliasi belum diperiksa; disampaikan ke petugas tarif saat mengisi `B4` |
| ~~Selisih `Idempotency-Key` pada tindakan atas data yang sudah ada~~ | Ditutup `OPT-DEC-081`; kontrak `OPT-API-v1.3` | — |
| Rincian kolom visus/autoref | Ditutup 2026-11-03: disahkan `OPT-DEC-057`, notasi `OPT-DEC-059` | — |
| Rentang BC/DIA | `PROPOSED` | Konfirmasi RO/Sp.M sebelum `BE-OPT-012` diuji |
| Profesi RO dan jenis izin di HR (`OPT-CAP-07`) | Data belum diperiksa | Kueri baca-saja saat `BE-OPT-004` |
| ~~Seluruh UAT layar~~ | Ditutup: screen brief `approved` 2026-11-04 ([`05-screen-brief.md`](../05-screen-brief.md)) | `FE-OPT-010`..`033` |

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
| ✅ Screen brief (2026-11-04) | Yoga Aji Pratama | — |
| ✅ Persetujuan task `BE-OPT-054` (roadmap backend r3, 2026-11-04) | Yoga Aji Pratama | `BE-OPT-054`; bagian pratinjau harga `FE-OPT-020` dan penyaring Kepala `FE-OPT-021` |
| ✅ Blueprint r2, kontrak, dan kunci hash (2026-11-06) | Yoga Aji Pratama | — |
| ✅ `OPT-APR-013` (`OPT-EVD-003`) | Muhammad Hamzah | — |
| ✅ `OPT-APR-014`, `OPT-OQ-020` (`OPT-EVD-003`) | Yasmina; Yoga Aji Pratama | — |
| ✅ Blueprint r3, kontrak, dan kunci hash (2026-11-06) | Yoga Aji Pratama | — |
| ✅ `OPT-APR-015` (`OPT-EVD-003`) | Yasmina | — |
| ✅ Blueprint r4, kontrak, dan kunci hash (2026-11-06) | Yoga Aji Pratama | — |
| ✅ `OPT-OQ-024` izin baca lintas modul (`OPT-DEC-086`) | Yoga Aji Pratama | — |
